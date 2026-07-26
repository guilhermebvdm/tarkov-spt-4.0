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
                        registeredManagerInstance = currentManager;
                        Log.LogInfo("[SFT] SftAudioPacket registrado com sucesso no NetPacketProcessor do FIKA.");
                    }
                    catch (System.Exception ex)
                    {
                        Log.LogWarning($"[SFT] Aviso ao registrar pacote no FIKA: {ex.Message}");
                    }
                }
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
                
                // Se o nível de áudio for desprezível (< 0.002f), ignora o envio (evita flood de silêncio no LiteNetLib)
                if (voiceLevel < 0.002f) return;

                string myProfileId = LocalSessionId;
                if (Singleton<GameWorld>.Instantiated && Singleton<GameWorld>.Instance.MainPlayer != null)
                {
                    string pId = Singleton<GameWorld>.Instance.MainPlayer.ProfileId;
                    if (!string.IsNullOrEmpty(pId)) myProfileId = pId;
                }

                SftAudioPacket packet = new SftAudioPacket 
                { 
                    ProfileId = myProfileId,
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
                
                // Rejeita pacotes próprios (Eco loopback local)
                if (packet.ProfileId == LocalSessionId) return;
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
                
                // Atualiza ancoragem do alto-falante 3D na cabeça/corpo do jogador
                if (Singleton<GameWorld>.Instantiated)
                {
                    var player = Singleton<GameWorld>.Instance.AllAlivePlayersList?.FirstOrDefault(p => p != null && p.ProfileId == packet.ProfileId);
                    if (player != null)
                    {
                        Transform targetBone = player.PlayerBones != null && player.PlayerBones.Head != null ? player.PlayerBones.Head.Original : player.Transform.Original;
                        if (speaker.transform.parent != targetBone)
                        {
                            speaker.transform.SetParent(targetBone, false);
                            speaker.transform.localPosition = targetBone == player.Transform.Original ? Vector3.up * 1.6f : Vector3.zero;
                        }
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
            
            // NUNCA chamamos UnregisterPacket<SftAudioPacket>() aqui!
            // Desregistrar a classe de pacote do FIKA faz com que o NetPacketProcessor do FIKA lance
            // 'ParseException: Undefined packet in NetDataReader: 20270', o que interrompe a leitura de rede
            // do FIKA e causa desync total de movimento/posições dos jogadores no jogo!

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
