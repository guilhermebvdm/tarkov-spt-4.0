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
        private Dictionary<byte, string> netIdToProfile = new Dictionary<byte, string>();
        private Dictionary<string, byte> profileToNetId = new Dictionary<string, byte>();
        private byte nextNetId = 1;

        public bool IsSessionActive { get; private set; } = false;
        public string LocalSessionId { get; private set; } = System.Guid.NewGuid().ToString();
        private int sampleRate;
        private int frameSize;

        public void Initialize(int sampleRate, int frameSize)
        {
            this.sampleRate = sampleRate;
            this.frameSize = frameSize;
        }

        private IFikaNetworkManager registeredManagerInstance = null;

        void Update()
        {
            EnsurePacketRegistered();
        }

        public void EnsurePacketRegistered()
        {
            if (Singleton<IFikaNetworkManager>.Instantiated)
            {
                var currentManager = Singleton<IFikaNetworkManager>.Instance;
                if (registeredManagerInstance != currentManager)
                {
                    try
                    {
                        currentManager.RegisterPacket<SftAudioPacket>(OnReceiveVoipData);
                        currentManager.RegisterPacket<SftHandshakePacket>(OnReceiveHandshake);
                        registeredManagerInstance = currentManager;
                        Log.LogInfo("[SFT] SftAudioPacket e SftHandshakePacket registrados com sucesso no FIKA.");
                    }
                    catch (System.Exception ex)
                    {
                        Log.LogWarning($"[SFT] Aviso ao registrar pacotes no FIKA: {ex.Message}");
                    }
                }
            }
        }

        private byte GetOrCreateNetId(string profileId)
        {
            if (profileToNetId.TryGetValue(profileId, out byte id))
                return id;

            id = nextNetId++;
            if (nextNetId == 0) nextNetId = 1; // Wrap seguro de 1 a 255

            profileToNetId[profileId] = id;
            netIdToProfile[id] = profileId;

            // Transmite Handshake confiável (Channel 0 Reliable) para notificar todos os pares
            SftHandshakePacket handshake = new SftHandshakePacket
            {
                PlayerNetId = id,
                ProfileId = profileId
            };
            if (Singleton<IFikaNetworkManager>.Instantiated)
            {
                Singleton<IFikaNetworkManager>.Instance.SendData(ref handshake, Fika.Core.Networking.LiteNetLib.DeliveryMethod.ReliableOrdered, broadcast: true);
            }
            return id;
        }

        private void OnReceiveHandshake(SftHandshakePacket packet)
        {
            if (packet.PlayerNetId > 0 && !string.IsNullOrEmpty(packet.ProfileId))
            {
                netIdToProfile[packet.PlayerNetId] = packet.ProfileId;
                profileToNetId[packet.ProfileId] = packet.PlayerNetId;
            }
        }

        public void InitFikaSession()
        {
            EnsurePacketRegistered();
            IsSessionActive = true;
            Log.LogInfo("[SFT] SftNetwork sessão ativada no FIKA.");
        }

        public void Broadcast(byte[] opusData, byte channel, float voiceLevel = 0f)
        {
            try
            {
                EnsurePacketRegistered();
                if (!IsSessionActive || !Singleton<IFikaNetworkManager>.Instantiated) return;
                if (VoIPPlugin.EnableMod != null && !VoIPPlugin.EnableMod.Value) return;

                if (voiceLevel < 0.002f) return;

                string myProfileId = LocalSessionId;
                if (Singleton<GameWorld>.Instantiated && Singleton<GameWorld>.Instance.MainPlayer != null)
                {
                    string pId = Singleton<GameWorld>.Instance.MainPlayer.ProfileId;
                    if (!string.IsNullOrEmpty(pId)) myProfileId = pId;
                }

                byte netId = GetOrCreateNetId(myProfileId);

                SftAudioPacket packet = new SftAudioPacket
                {
                    PlayerNetId = netId,
                    Channel = channel,
                    AudioData = opusData,
                    VoiceLevel = voiceLevel
                };

                Singleton<IFikaNetworkManager>.Instance.SendData(ref packet, Fika.Core.Networking.LiteNetLib.DeliveryMethod.Unreliable, broadcast: true);
            }
            catch (System.Exception ex)
            {
                Log?.LogError($"[SFT] Erro ao enviar Broadcast de áudio: {ex.Message}");
            }
        }

        private void OnReceiveVoipData(SftAudioPacket packet)
        {
            try
            {
                if (!Singleton<GameWorld>.Instantiated) return;
                if (VoIPPlugin.EnableMod != null && !VoIPPlugin.EnableMod.Value) return;
                if (packet.Magic != SftAudioPacket.MAGIC_HEADER) return;
                if (packet.AudioData == null || packet.AudioData.Length == 0) return;

                // Resolve o ProfileId pelo PlayerNetId
                string senderProfileId = null;
                if (packet.PlayerNetId > 0)
                {
                    netIdToProfile.TryGetValue(packet.PlayerNetId, out senderProfileId);
                }

                if (string.IsNullOrEmpty(senderProfileId)) return;

                // Rejeita pacotes próprios (Eco loopback local)
                if (senderProfileId == LocalSessionId) return;
                if (Singleton<GameWorld>.Instantiated && Singleton<GameWorld>.Instance.MainPlayer != null)
                {
                    if (senderProfileId == Singleton<GameWorld>.Instance.MainPlayer.ProfileId)
                        return;
                }

                if (Core.VoipController.Instance != null && packet.Channel != Core.VoipController.Instance.CurrentChannel)
                    return;

                RemoteSpeaker speaker;
                if (!remoteSpeakers.TryGetValue(senderProfileId, out speaker) || speaker == null)
                {
                    speaker = CreateRemoteSpeaker(senderProfileId);
                    if (speaker == null) return;
                    remoteSpeakers[senderProfileId] = speaker;
                }

                // Atualiza ancoragem do alto-falante 3D na cabeça/corpo do jogador
                if (Singleton<GameWorld>.Instantiated)
                {
                    var player = Singleton<GameWorld>.Instance.AllAlivePlayersList?.FirstOrDefault(p => p != null && p.ProfileId == senderProfileId);
                    if (player != null && player.Transform != null)
                    {
                        Transform targetBone = player.PlayerBones != null && player.PlayerBones.Head != null ? player.PlayerBones.Head.Original : player.Transform.Original;
                        if (speaker.transform.parent != targetBone)
                        {
                            speaker.transform.SetParent(targetBone, false);
                            speaker.transform.localPosition = targetBone == player.Transform.Original ? Vector3.up * 1.6f : Vector3.zero;
                        }
                        speaker.SetEmergency2DMode(false);
                    }
                    else
                    {
                        // Em desync posicional ou avatar ausente, ativa o modo 2D de emergência para manter o áudio audível
                        speaker.SetEmergency2DMode(true);
                    }
                }

                speaker.EnqueuePacket(packet.AudioData, packet.VoiceLevel);
            }
            catch (System.Exception ex)
            {
                Log?.LogError($"[SFT] Erro no callback OnReceiveVoipData: {ex.Message}");
            }
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

            foreach (var kvp in remoteSpeakers)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value.gameObject);
            }
            remoteSpeakers.Clear();
            netIdToProfile.Clear();
            profileToNetId.Clear();
            nextNetId = 1;
        }

        void OnDestroy()
        {
            StopSession();
        }
    }
}
