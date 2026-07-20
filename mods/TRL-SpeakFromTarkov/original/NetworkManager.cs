using BepInEx.Logging;
using Concentus.Structs;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace SpeakFromTarkov
{
    internal class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }
        public bool IsSessionActive { get; private set; } = true;
        private UdpClient client;
        private IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 50000);
        private OpusDecoder decoder;
        private ManualLogSource Log => VoIPPlugin.Log;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            client = new UdpClient(0);
#pragma warning disable CS0618
            decoder = new OpusDecoder(VoIPPlugin.SampleRate.Value, 1);
#pragma warning restore CS0618
            Task.Run(ReceiveData);
            Log.LogInfo("[SFT] NetworkManager UDP iniciado (localhost).");
        }

        private async void ReceiveData()
        {
            while (true)
            {
                try
                {
                    var result = await client.ReceiveAsync();
                    if (result.Buffer.Length < 2) continue;
                    byte channel = result.Buffer[1];
                    if (channel != VoiceChatManager.Instance.currentChannel) continue;

                    byte[] opus = new byte[result.Buffer.Length - 2];
                    Array.Copy(result.Buffer, 2, opus, 0, opus.Length);
                    float[] pcm = new float[VoIPPlugin.FrameSize];  // CORRIGIDO: sem .Value
#pragma warning disable CS0618
                    decoder.Decode(opus, 0, opus.Length, pcm, 0, VoIPPlugin.FrameSize, false);  // CORRIGIDO
#pragma warning restore CS0618
                    AudioSource.PlayClipAtPoint(pcm.ToClip(), Camera.main.transform.position, 0.5f);
                }
                catch { }
            }
        }

        public void BroadcastVoipData(byte[] data, byte channel)
        {
            if (!IsSessionActive || client == null) return;
            byte[] packet = new byte[data.Length + 2];
            packet[0] = 10; packet[1] = channel;
            Array.Copy(data, 0, packet, 2, data.Length);
            client.Send(packet, packet.Length, endPoint);
        }

        public void SetSessionActive(bool active) => IsSessionActive = active;

        public void Cleanup()
        {
            client?.Close();
            decoder?.Dispose();
            Log.LogInfo("[SFT] NetworkManager encerrado com segurança.");
        }
    }

    public static class AudioClipExtensions
    {
        public static AudioClip ToClip(this float[] data)
        {
            var clip = AudioClip.Create("VOIP", data.Length, 1, VoIPPlugin.SampleRate.Value, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}