using BepInEx.Logging;
using Comfort.Common;
using Fika.Core.Networking;
using System.Collections.Generic;
using UnityEngine;
using EFT;
using System.Linq;
using TRL_SpeakFromTarkov.Audio;

namespace TRL_SpeakFromTarkov.Network
{
    public class SftNetwork : MonoBehaviour
    {
        private ManualLogSource Log => VoIPPlugin.Log;
        private Dictionary<string, RemoteSpeaker> remoteSpeakers = new Dictionary<string, RemoteSpeaker>();
        
        public bool IsSessionActive { get; private set; } = false;
        private int sampleRate;
        private int frameSize;
        
        public void Initialize(int sampleRate, int frameSize)
        {
            this.sampleRate = sampleRate;
            this.frameSize = frameSize;
        }

        public void InitFikaSession()
        {
            if (Singleton<IFikaNetworkManager>.Instantiated)
            {
                Singleton<IFikaNetworkManager>.Instance.RegisterPacket<SftAudioPacket>(OnReceiveVoipData);
                IsSessionActive = true;
                Log.LogInfo("[SFT] SftNetwork conectado ao FIKA.");
            }
        }

        public void Broadcast(byte[] opusData, byte channel)
        {
            if (!IsSessionActive || !Singleton<IFikaNetworkManager>.Instantiated) return;
            
            string myProfileId = string.Empty;
            if (Singleton<GameWorld>.Instantiated && Singleton<GameWorld>.Instance.MainPlayer != null)
            {
                myProfileId = Singleton<GameWorld>.Instance.MainPlayer.ProfileId;
            }

            SftAudioPacket packet = new SftAudioPacket 
            { 
                ProfileId = myProfileId ?? string.Empty,
                Channel = channel, 
                AudioData = opusData 
            };
            
            Singleton<IFikaNetworkManager>.Instance.SendData(ref packet, Fika.Core.Networking.LiteNetLib.DeliveryMethod.Sequenced, broadcast: true);
        }

        private void OnReceiveVoipData(SftAudioPacket packet)
        {
            if (!IsSessionActive) return;
            
            // Rejeita pacotes próprios (Eco loopback local)
            if (Singleton<GameWorld>.Instantiated && Singleton<GameWorld>.Instance.MainPlayer != null)
            {
                if (packet.ProfileId == Singleton<GameWorld>.Instance.MainPlayer.ProfileId)
                    return;
            }

            // A validação de Canal deve ser feita pelo Controller no futuro, mas para MVP fica aqui
            if (Core.VoipController.Instance != null && packet.Channel != Core.VoipController.Instance.CurrentChannel) 
                return;

            RemoteSpeaker speaker;
            if (!remoteSpeakers.TryGetValue(packet.ProfileId, out speaker) || speaker == null)
            {
                speaker = CreateRemoteSpeaker(packet.ProfileId);
                if (speaker == null) return;
                remoteSpeakers[packet.ProfileId] = speaker;
            }
            
            // Atualiza posição do speaker no mundo
            if (Singleton<GameWorld>.Instantiated)
            {
                var player = Singleton<GameWorld>.Instance.AllAlivePlayersList.FirstOrDefault(p => p.ProfileId == packet.ProfileId);
                if (player != null)
                {
                    if (speaker.transform.parent == null)
                    {
                        speaker.transform.SetParent(player.Transform.Original, false);
                        speaker.transform.localPosition = Vector3.up * 1.6f;
                    }
                }
            }

            speaker.EnqueuePacket(packet.AudioData);
        }

        private RemoteSpeaker CreateRemoteSpeaker(string profileId)
        {
            var go = new GameObject($"SftRemoteSpeaker_{profileId}");
            var speaker = go.AddComponent<RemoteSpeaker>();
            speaker.Initialize(sampleRate, frameSize);
            return speaker;
        }

        public void StopSession()
        {
            IsSessionActive = false;
            
            if (Singleton<IFikaNetworkManager>.Instantiated)
            {
                Singleton<IFikaNetworkManager>.Instance.UnregisterPacket<SftAudioPacket>();
            }
            
            // Limpa todos os falantes remotos (evita memory leak)
            foreach (var kvp in remoteSpeakers)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value.gameObject);
            }
            remoteSpeakers.Clear();
        }

        void OnDestroy()
        {
            StopSession();
        }
    }
}
