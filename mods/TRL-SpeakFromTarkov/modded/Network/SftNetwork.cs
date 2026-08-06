using BepInEx.Logging;
using Comfort.Common;
using Fika.Core.Networking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using EFT;
using System.Linq;
using TRL_SpeakFromTarkov.Audio;

namespace TRL_SpeakFromTarkov.Network
{
    [Serializable]
    public class SftChannelPostDto
    {
        public int channelId;
        public string channelName = string.Empty;
        public string hostProfileId = string.Empty;
        public string hostNickname = string.Empty;
        public string targetProfileId = string.Empty;
        public int action;
    }

    public class SftNetwork : MonoBehaviour
    {
        private static ManualLogSource Log => VoIPPlugin.Log;
        private Dictionary<string, RemoteSpeaker> remoteSpeakers = new Dictionary<string, RemoteSpeaker>();

        public bool IsSessionActive { get; private set; } = false;
        public string LocalSessionId { get; private set; } = System.Guid.NewGuid().ToString();
        private int sampleRate;
        private int frameSize;

        /// <summary>
        /// Instância viva do componente. O callback de rede é ESTÁTICO (ver EnsurePacketsRegistered):
        /// o delegate registrado no NetPacketProcessor do FIKA sobrevive à destruição do MonoBehaviour,
        /// então capturar `this` deixaria um callback apontando para um objeto Unity destruído.
        /// </summary>
        private static SftNetwork _instance;
        public static SftNetwork Instance => _instance;

        /// <summary>
        /// Rastreamento por REFERÊNCIA de instância (não por flag bool): o FIKA destrói e recria o
        /// IFikaNetworkManager em cada transição menu → lobby → raid, e a nova instância tem o
        /// NetPacketProcessor vazio. Comparar a referência é o que detecta essa troca.
        /// </summary>
        private static IFikaNetworkManager _lastRegisteredManager;

        /// <summary>Frame de áudio aguardando envio na main thread.</summary>
        private struct PendingAudio
        {
            public byte[] Data;
            public byte Channel;
            public float VoiceLevel;
        }

        /// <summary>
        /// Fila de saída. O encoder Opus roda numa thread de captura em background, mas
        /// FikaClient/FikaServer serializam TODOS os envios num único NetDataWriter de instância
        /// (`FikaClient._dataWriter`) sem qualquer lock. Chamar SendData da thread de captura
        /// enquanto a main thread envia PlayerState corrompe esse buffer compartilhado e coloca
        /// um datagrama malformado na rede — origem real do
        /// `ParseException: Undefined packet in NetDataReader` e do desync de posição.
        /// Por isso a thread de captura apenas ENFILEIRA; quem transmite é o Update (main thread).
        /// </summary>
        private readonly ConcurrentQueue<PendingAudio> sendQueue = new ConcurrentQueue<PendingAudio>();

        /// <summary>~500 ms de áudio a 20 ms/frame. Dimensionado para atravessar um hitch típico do
        /// Tarkov (entrada de jogador, carregamento) sem cortar a fala: só o backlog além disso é
        /// descartado, frame mais antigo primeiro. Em regime estacionário a fila só satura abaixo
        /// de ~2 FPS.</summary>
        private const int MaxQueuedFrames = 25;

        // Throttle de log: os catch de rede ficam em caminhos de ~50 Hz. Sem limite, uma falha
        // sistemática (peer com formato divergente) despejaria dezenas de stack traces por segundo
        // no console do BepInEx, causando hitching e escondendo o erro real.
        private const int ErrorLogIntervalMs = 5000;
        private static int _lastErrorLogTick;
        private static int _suppressedErrors;

        /// <summary>
        /// Tipos de exceção que já renderam stack trace completo. A chave é o TIPO (e não um
        /// booleano global): uma falha diferente que apareça raids depois ainda precisa do trace,
        /// caso contrário o throttle esconderia justamente o problema novo.
        /// </summary>
        private static readonly HashSet<Type> _tracedExceptionTypes = new HashSet<Type>();

        /// <summary>
        /// Loga a exceção completa na primeira ocorrência de cada TIPO e, depois, no máximo uma vez
        /// a cada 5 s com a contagem do que foi suprimido. Usa Environment.TickCount (e não
        /// Time.time) porque também é chamado da thread de captura, onde a API da Unity não pode
        /// ser tocada.
        /// </summary>
        internal static void LogErrorThrottled(string context, Exception ex)
        {
            var now = Environment.TickCount;

            bool firstOfType;
            lock (_tracedExceptionTypes)
            {
                firstOfType = ex != null && _tracedExceptionTypes.Add(ex.GetType());
            }

            if (firstOfType)
            {
                _lastErrorLogTick = now;
                Log?.LogError($"[SFT] {context}: {ex}");
                return;
            }

            // Subtração de ints trata o wrap-around de TickCount corretamente.
            if (now - _lastErrorLogTick < ErrorLogIntervalMs)
            {
                Interlocked.Increment(ref _suppressedErrors);
                return;
            }

            _lastErrorLogTick = now;
            var suppressed = Interlocked.Exchange(ref _suppressedErrors, 0);
            Log?.LogError($"[SFT] {context}: {ex?.Message}"
                + (suppressed > 0 ? $" (+{suppressed} falhas suprimidas nos últimos {ErrorLogIntervalMs / 1000}s)" : string.Empty));
        }

        public void Initialize(int sampleRate, int frameSize)
        {
            this.sampleRate = sampleRate;
            this.frameSize = frameSize;
        }

        void Awake()
        {
            _instance = this;
        }

        void Update()
        {
            EnsurePacketsRegistered();
            DrainSendQueue();
        }

        /// <summary>
        /// Garante que o pacote esteja registrado na instância ATIVA do IFikaNetworkManager.
        /// Chamado no Update do plugin, no Update deste componente e antes de cada envio.
        /// </summary>
        public static void EnsurePacketsRegistered()
        {
            if (!Singleton<IFikaNetworkManager>.Instantiated)
            {
                _lastRegisteredManager = null;
                return;
            }

            var currentManager = Singleton<IFikaNetworkManager>.Instance;
            if (currentManager == null) return;
            if (_lastRegisteredManager == currentManager) return;

            try
            {
                currentManager.RegisterPacket<SftAudioPacketV2>(OnReceiveVoipDataV2);
                currentManager.RegisterPacket<SftAudioPacket>(OnReceiveVoipDataLegacy);
                currentManager.RegisterPacket<SftChannelAnnouncementPacket>(OnReceiveChannelAnnouncement);

                _lastRegisteredManager = currentManager;
                Log?.LogInfo($"[SFT] Pacotes VOIP (+Canais de Menu) registrados no NetPacketProcessor do FIKA ({currentManager.GetType().Name}).");
            }
            catch (Exception ex)
            {
                Log?.LogError($"[SFT] Falha ao registrar pacotes no FIKA: {ex}");
            }
        }

        public void InitFikaSession()
        {
            EnsurePacketsRegistered();
            IsSessionActive = true;
            Log?.LogInfo("[SFT] SftNetwork sessão ativada no FIKA.");
        }

        /// <summary>
        /// Chamado da THREAD DE CAPTURA. Faz só o mínimo thread-safe: valida e enfileira.
        /// Nada aqui pode tocar API da Unity, do EFT ou do FIKA — ver comentário de sendQueue.
        /// </summary>
        public void Broadcast(byte[] opusData, byte channel, float voiceLevel = 0f)
        {
            try
            {
                if (!IsSessionActive) return;
                if (VoIPPlugin.EnableMod != null && !VoIPPlugin.EnableMod.Value) return;
                if (opusData == null || opusData.Length == 0) return;

                // Nível de áudio desprezível: não transmite silêncio (evita flood no LiteNetLib).
                if (voiceLevel < 0.002f) return;

                // Descarta o frame mais antigo se a main thread não estiver drenando.
                while (sendQueue.Count >= MaxQueuedFrames && sendQueue.TryDequeue(out _)) { }

                sendQueue.Enqueue(new PendingAudio
                {
                    Data = opusData,
                    Channel = channel,
                    VoiceLevel = voiceLevel
                });
            }
            catch (Exception ex)
            {
                LogErrorThrottled("Erro ao enfileirar áudio para envio", ex);
            }
        }

        /// <summary>Transmite os frames enfileirados. SEMPRE na main thread (chamado do Update).</summary>
        private void DrainSendQueue()
        {
            if (sendQueue.IsEmpty) return;

            if (!IsSessionActive || !Singleton<IFikaNetworkManager>.Instantiated)
            {
                while (sendQueue.TryDequeue(out _)) { }
                return;
            }

            string myProfileId = LocalSessionId;
            if (Singleton<GameWorld>.Instantiated && Singleton<GameWorld>.Instance.MainPlayer != null)
            {
                string pId = Singleton<GameWorld>.Instance.MainPlayer.ProfileId;
                if (!string.IsNullOrEmpty(pId)) myProfileId = pId;
            }

            // Fora do laço: o registro depende só da instância do manager, não do frame de áudio.
            EnsurePacketsRegistered();

            while (sendQueue.TryDequeue(out var pending))
            {
                try
                {
                    var packet = new SftAudioPacketV2
                    {
                        ProfileId = myProfileId,
                        Channel = pending.Channel,
                        AudioData = pending.Data,
                        VoiceLevel = pending.VoiceLevel
                    };
                    Singleton<IFikaNetworkManager>.Instance.SendData(
                        ref packet, Fika.Core.Networking.LiteNetLib.DeliveryMethod.Unreliable, broadcast: true);
                }
                catch (Exception ex)
                {
                    LogErrorThrottled("Erro ao transmitir frame de áudio", ex);
                }
            }
        }

        /// <summary>
        /// Airbag de recepção: o corpo inteiro é protegido. Uma exceção que escape daqui sobe pelo
        /// ReadAllPackets do LiteNetLib e descarta o restante do lote de pacotes daquele frame —
        /// inclusive os dos outros mods e os de movimento do FIKA.
        /// </summary>
        private static void OnReceiveVoipDataV2(SftAudioPacketV2 packet)
        {
            DispatchVoipPacket(packet.ProfileId, packet.Channel, packet.AudioData, packet.VoiceLevel,
                nameof(OnReceiveVoipDataV2));
        }

        private static void OnReceiveVoipDataLegacy(SftAudioPacket packet)
        {
            DispatchVoipPacket(packet.ProfileId, packet.Channel, packet.AudioData, packet.VoiceLevel,
                nameof(OnReceiveVoipDataLegacy));
        }

        private static void OnReceiveChannelAnnouncement(SftChannelAnnouncementPacket packet)
        {
            try
            {
                if (UI.MenuVoipHUD.Instance != null)
                {
                    UI.MenuVoipHUD.Instance.HandleChannelAnnouncement(packet);
                }
            }
            catch (Exception ex)
            {
                LogErrorThrottled("Erro no callback OnReceiveChannelAnnouncement", ex);
            }
        }

        private static float _lastAnnounceTime = 0f;

        public static void BroadcastChannelAnnouncement(byte channelId, string channelName, string hostProfileId, string hostNickname, byte action, string targetProfileId = "")
        {
            try
            {
                // Debounce Anti-Spam: ignora chamadas repetidas em curto intervalo (< 0.8s) para qualquer ação
                if (Time.time - _lastAnnounceTime < 0.8f)
                {
                    return;
                }
                _lastAnnounceTime = Time.time;

                var packet = new SftChannelAnnouncementPacket
                {
                    ChannelId = channelId,
                    ChannelName = channelName,
                    HostProfileId = hostProfileId,
                    HostNickname = hostNickname,
                    TargetProfileId = targetProfileId ?? string.Empty,
                    Action = action
                };

                // 1. Envia via LiteNetLib se a sessão do FIKA estiver ativa
                if (Singleton<IFikaNetworkManager>.Instantiated)
                {
                    EnsurePacketsRegistered();
                    Singleton<IFikaNetworkManager>.Instance.SendData(ref packet, Fika.Core.Networking.LiteNetLib.DeliveryMethod.ReliableOrdered, broadcast: true);
                }

                // 2. Transmite via HTTP para o Servidor SPT (Visível para TODOS no Menu Principal)
                Task.Run(() =>
                {
                    try
                    {
                        var dto = new SftChannelPostDto
                        {
                            channelId = (int)channelId,
                            channelName = channelName ?? "",
                            hostProfileId = hostProfileId ?? "",
                            hostNickname = hostNickname ?? "",
                            targetProfileId = targetProfileId ?? "",
                            action = (int)action
                        };
                        string json = UnityEngine.JsonUtility.ToJson(dto);
                        string serverUrl = SPT.Common.Http.RequestHandler.Host;
                        if (!serverUrl.EndsWith("/")) serverUrl += "/";
                        
                        using var client = new System.Net.Http.HttpClient();
                        client.Timeout = TimeSpan.FromSeconds(3);
                        var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
                        var response = client.PostAsync(serverUrl + "sft/channels/announce", content).Result;
                    }
                    catch { }
                });
            }
            catch (Exception ex)
            {
                LogErrorThrottled("Erro ao transmitir anúncio de canal de menu", ex);
            }
        }

        private static void DispatchVoipPacket(string profileId, byte channel, byte[] audioData, float voiceLevel, string context)
        {
            try
            {
                if (VoIPPlugin.EnableMod != null && !VoIPPlugin.EnableMod.Value) return;

                bool inRaid = Singleton<GameWorld>.Instantiated;
                bool inMenuChannel = (UI.MenuVoipHUD.Instance != null && UI.MenuVoipHUD.Instance.ConnectedChannelId.HasValue);

                if (!inRaid && !inMenuChannel) return;

                if (Fika.Core.Main.Utils.FikaBackendUtils.IsHeadless) return;

                var self = _instance;
                if (self == null) return;

                self.HandleVoipPacket(profileId, channel, audioData, voiceLevel);
            }
            catch (Exception ex)
            {
                LogErrorThrottled($"Erro no callback {context}", ex);
            }
        }

        private void HandleVoipPacket(string profileId, byte channel, byte[] audioData, float voiceLevel)
        {
            if (audioData == null || audioData.Length == 0) return;
            if (string.IsNullOrEmpty(profileId)) return;

            if (audioData.Length > SftAudioPacketV2.MaxAudioBytes) return;

            if (profileId == LocalSessionId) return;

            bool inRaid = Singleton<GameWorld>.Instantiated;
            var gameWorld = inRaid ? Singleton<GameWorld>.Instance : null;

            if (inRaid && gameWorld != null && gameWorld.MainPlayer != null && profileId == gameWorld.MainPlayer.ProfileId) return;

            if (Core.VoipController.Instance != null && channel != Core.VoipController.Instance.CurrentChannel)
                return;

            RemoteSpeaker speaker;
            if (!remoteSpeakers.TryGetValue(profileId, out speaker) || speaker == null)
            {
                speaker = CreateRemoteSpeaker(profileId);
                if (speaker == null) return;
                remoteSpeakers[profileId] = speaker;
            }

            if (inRaid && gameWorld != null)
            {
                speaker.SetEmergency2DMode(false);
                Player player = gameWorld.GetAlivePlayerByProfileID(profileId);
                if (player == null && gameWorld.AllAlivePlayersList != null)
                {
                    player = gameWorld.AllAlivePlayersList.FirstOrDefault(p => p != null && (p.ProfileId == profileId || (p.Profile != null && p.Profile.Id == profileId)));
                }

                if (player != null)
                {
                    Transform targetBone = player.PlayerBones != null && player.PlayerBones.Head != null
                        ? player.PlayerBones.Head.Original
                        : player.Transform.Original;
                    if (speaker.transform.parent != targetBone)
                    {
                        speaker.transform.SetParent(targetBone, false);
                        speaker.transform.localPosition = targetBone == player.Transform.Original ? Vector3.up * 1.6f : Vector3.zero;
                    }
                }
            }
            else
            {
                // Modo 2D Estéreo para o Menu Principal
                speaker.SetEmergency2DMode(true);
            }

            speaker.EnqueuePacket(audioData, voiceLevel);
        }

        private RemoteSpeaker CreateRemoteSpeaker(string profileId)
        {
            var go = new GameObject($"SftRemoteSpeaker_{profileId}");
            var speaker = go.AddComponent<RemoteSpeaker>();
            speaker.TargetProfileId = profileId;
            speaker.Initialize(sampleRate, frameSize);
            return speaker;
        }

        public void StopSession()
        {
            IsSessionActive = false;

            // NUNCA chamamos UnregisterPacket<SftAudioPacket>() aqui!
            // Remover a subscrição do NetPacketProcessor compartilhado do FIKA faz com que pacotes
            // ainda em voo (ou retidos em buffer) cheguem sem handler, lançando
            // 'ParseException: Undefined packet in NetDataReader' e interrompendo a leitura de rede
            // do FIKA — o que causa desync total de movimento/posição dos jogadores.
            // O gate de "fora de raid" é feito por guard clause no callback, não por desregistro.

            while (sendQueue.TryDequeue(out _)) { }

            foreach (var kvp in remoteSpeakers)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value.gameObject);
            }
            remoteSpeakers.Clear();
        }

        void OnDestroy()
        {
            if (_instance == this) _instance = null;
            StopSession();
        }
    }
}
