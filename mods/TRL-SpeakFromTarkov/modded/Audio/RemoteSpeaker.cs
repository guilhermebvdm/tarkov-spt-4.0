using System;
using UnityEngine;
using Concentus.Structs;
using System.Collections.Concurrent;
using Comfort.Common;
using EFT;

namespace TRL_SpeakFromTarkov.Audio
{
    public class RemoteSpeaker : MonoBehaviour
    {
        private OpusDecoder decoder = null!;
        private AudioSource audioSource = null!;
        public float CurrentOutputLevel { get; private set; }
        public bool IsPlaying => audioSource != null && audioSource.isPlaying;
        public int PacketsEnqueued => packetsEnqueuedCount;
        public int FilterCalls => filterReadCallsCount;
        public bool IsBuffering => isBuffering;
        public int BufferAvailable => (streamBuffer != null && streamBuffer.Length > 0) ? (streamWritePos - streamReadPos + streamBuffer.Length) % streamBuffer.Length : 0;
        public int UnderrunCount { get; private set; }
        private int lastUnderrunCheckCount = -1;
        public string TargetProfileId { get; set; } = null!;
        private float lpfState = 0f;

        public int GetRecentUnderruns()
        {
            int current = UnderrunCount;
            if (lastUnderrunCheckCount < 0)
            {
                lastUnderrunCheckCount = current;
                return 0;
            }
            int delta = current - lastUnderrunCheckCount;
            if (delta < 0) delta = 0;
            lastUnderrunCheckCount = current;
            return delta;
        }

        private ConcurrentQueue<byte[]> packetQueue = new ConcurrentQueue<byte[]>();
        private int packetsEnqueuedCount = 0;
        private int filterReadCallsCount = 0;
        private float lastSample = 0f;

        private int sampleRate;
        private int frameSize;
        private float[] opusDecodeBuffer = null!;

        private float currentDistanceTarget = 30f;
        private float smoothedDistance = 30f;
        private float _lastReanchorTry = 0f;

        // Oclusão Física Zero-GC
        private float _lastOcclusionCheckTime = 0f;
        private bool _isOccluded = false;
        private float _targetOcclusionFactor = 1.0f;
        private float _targetOcclusionAirDamping = 1.0f;
        private float _smoothedOcclusionFactor = 1.0f;
        private float _smoothedOcclusionAirDamping = 1.0f;
        private static readonly RaycastHit[] _occlusionHits = new RaycastHit[1];
        private static int _occlusionLayerMask = -1;

        private float[] streamBuffer = null!;
        private volatile int streamWritePos = 0;
        private volatile int streamReadPos  = 0;

        private bool isBuffering = true;
        
        // Jitter settings: initial buffering requires 150ms, recovery requires 40ms
        private int jitterInitialSamples;
        private int jitterRecoverySamples;
        private int currentJitterTarget;

        private float currentSpatialBlend = 1f;

        public void Initialize(int sampleRate, int frameSize, float spatialBlend = 1f)
        {
            this.sampleRate = sampleRate;
            this.frameSize  = frameSize;
            this.currentSpatialBlend = spatialBlend;

            int jitterMs = spatialBlend == 0f ? 100 : 150;
            if (spatialBlend > 0f && VoIPPlugin.NetworkJitterBufferMs != null)
                jitterMs = (int)VoIPPlugin.NetworkJitterBufferMs.Value;

            jitterInitialSamples = (int)(sampleRate * (jitterMs / 1000f));
            jitterRecoverySamples = (int)(sampleRate * 0.050f); // 50ms
            currentJitterTarget   = jitterInitialSamples;

            this.opusDecodeBuffer = new float[frameSize];
            this.streamBuffer     = new float[sampleRate * 3]; // 3s de buffer

            try
            {
#pragma warning disable CS0618
                decoder = new OpusDecoder(sampleRate, 1);
#pragma warning restore CS0618
            }
            catch (Exception ex)
            {
                VoIPPlugin.Log.LogError($"[SFT] RemoteSpeaker Opus erro: {ex.Message}");
            }

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = spatialBlend;
            audioSource.spatialize    = spatialBlend > 0f;
            audioSource.dopplerLevel  = 0f;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.minDistance = 2f;
            
            // Ignora o Mixer/Listener do Tarkov para evitar que o jogo multe o VoIP
            audioSource.ignoreListenerVolume = true;
            audioSource.ignoreListenerPause = true;
            audioSource.bypassEffects = true;
            audioSource.bypassListenerEffects = true;
            audioSource.bypassReverbZones = true;
            
            float maxDist = 30f;
            if (VoIPPlugin.MaxHearingDistance != null)
                maxDist = VoIPPlugin.MaxHearingDistance.Value;
                
            audioSource.maxDistance = maxDist;
            currentDistanceTarget = maxDist;
            smoothedDistance = maxDist;
            audioSource.loop          = true;
            
            var clip = AudioClip.Create("SftStream", sampleRate, 1, sampleRate, false);
            float[] dummyData = new float[sampleRate];
            for(int i = 0; i < dummyData.Length; i++) dummyData[i] = 0.001f;
            clip.SetData(dummyData, 0);
            
            audioSource.clip = clip;
            audioSource.Play();
        }

        public void SetVolume(float volume)
        {
            if (audioSource != null)
            {
                float normVolume = volume > 1.0f ? volume / 100.0f : volume;
                audioSource.volume = Mathf.Clamp01(normVolume);
            }
        }

        public void EnqueuePacket(byte[] opusData, float voiceLevel = 0f)
        {
            if (opusData == null || opusData.Length <= 2) return;

            packetsEnqueuedCount++;
            if (VoIPPlugin.EnableDebugLogs != null && VoIPPlugin.EnableDebugLogs.Value && packetsEnqueuedCount % 50 == 1)
            {
                VoIPPlugin.Log.LogInfo($"[SFT-DEBUG] Echo/Speaker pacotes enfileirados: {packetsEnqueuedCount} (tamanho do pacote: {opusData.Length} bytes)");
            }
            
            float maxBase = 30f;
            if (VoIPPlugin.MaxHearingDistance != null)
                maxBase = VoIPPlugin.MaxHearingDistance.Value;
                
            // Mapeia VoiceLevel (0 a 1) para um multiplicador de distância.
            // Whisper (~0.01) -> 0.65x (20m). Normal (~0.1) -> 1.0x (30m). Grito (>0.3) -> 2.0x (60m).
            float distanceMultiplier = Mathf.Clamp((voiceLevel * 10f), 0.65f, 2.0f);
            currentDistanceTarget = maxBase * distanceMultiplier;

            // Decodificação Off-Thread imediata no callback de recepção da rede
            if (decoder != null && opusDecodeBuffer != null && streamBuffer != null)
            {
                try
                {
#pragma warning disable CS0618
                    int len = decoder.Decode(opusData, 0, opusData.Length, opusDecodeBuffer, 0, frameSize, false);
#pragma warning restore CS0618

                    if (len > 0)
                    {
                        int currentWritePos = streamWritePos;
                        for (int i = 0; i < len; i++)
                        {
                            streamBuffer[(currentWritePos + i) % streamBuffer.Length] = opusDecodeBuffer[i];
                        }
                        streamWritePos = (currentWritePos + len) % streamBuffer.Length;
                    }
                }
                catch (Exception decEx)
                {
                    VoIPPlugin.Log.LogWarning($"[SFT] Ignorando frame Opus corrompido/inválido ({opusData.Length} bytes): {decEx.Message}");
                }
            }
        }

        private bool isEmergency2DMode = false;

        public void SetEmergency2DMode(bool active)
        {
            if (isEmergency2DMode == active) return;
            isEmergency2DMode = active;

            if (audioSource != null)
            {
                audioSource.spatialBlend = active ? 0f : currentSpatialBlend;
                audioSource.spatialize = !active && (currentSpatialBlend > 0f);
            }
        }

        void Update()
        {
            try
            {
                if (audioSource != null)
                {
                    if (float.IsNaN(currentDistanceTarget) || float.IsInfinity(currentDistanceTarget) || currentDistanceTarget <= 0f)
                    {
                        currentDistanceTarget = 30f;
                    }

                    if (float.IsNaN(smoothedDistance) || float.IsInfinity(smoothedDistance) || smoothedDistance <= 0f)
                    {
                        smoothedDistance = currentDistanceTarget;
                    }

                    smoothedDistance = Mathf.Lerp(smoothedDistance, currentDistanceTarget, Time.deltaTime * 5f);
                    smoothedDistance = Mathf.Clamp(smoothedDistance, 1f, 150f);
                    audioSource.maxDistance = smoothedDistance;

                    // Força o Play caso o jogo tenha pausado ou o AudioSource tenha falhado no Awake
                    if (!audioSource.isPlaying && audioSource.enabled && gameObject.activeInHierarchy)
                    {
                        audioSource.Play();
                    }
                }

                // Re-ancoragem Dinâmica: Se o alto-falante ainda não está preso à cabeça do jogador remoto (ficou em 0,0,0)
                if (transform.parent == null && !string.IsNullOrEmpty(TargetProfileId) && Singleton<GameWorld>.Instantiated)
                {
                    if (Time.time - _lastReanchorTry >= 2.0f)
                    {
                        _lastReanchorTry = Time.time;
                        var gameWorld = Singleton<GameWorld>.Instance;
                        Player player = gameWorld.GetAlivePlayerByProfileID(TargetProfileId);
                        if (player == null && gameWorld.AllAlivePlayersList != null)
                        {
                            player = System.Linq.Enumerable.FirstOrDefault(gameWorld.AllAlivePlayersList, p => p != null && (p.ProfileId == TargetProfileId || (p.Profile != null && p.Profile.Id == TargetProfileId)));
                        }

                        if (player != null)
                        {
                            Transform targetBone = player.PlayerBones != null && player.PlayerBones.Head != null
                                ? player.PlayerBones.Head.Original
                                : player.Transform.Original;
                            transform.SetParent(targetBone, false);
                            transform.localPosition = targetBone == player.Transform.Original ? Vector3.up * 1.6f : Vector3.zero;
                            VoIPPlugin.Log.LogInfo($"[SFT-3D] RemoteSpeaker de {TargetProfileId} re-ancorado com sucesso ao boneco {(player.Profile != null ? player.Profile.Nickname : player.ProfileId)}!");
                        }
                    }
                }

                // ── OCLUSÃO FÍSICA POR GEOMETRIA & PORTAS (200ms = 5Hz Zero-GC) ──
                if (Time.time - _lastOcclusionCheckTime >= 0.20f)
                {
                    _lastOcclusionCheckTime = Time.time;
                    bool occlusionEnabled = VoIPPlugin.EnableOcclusion == null || VoIPPlugin.EnableOcclusion.Value;

                    if (occlusionEnabled && currentSpatialBlend > 0f && !isEmergency2DMode)
                    {
                        if (_occlusionLayerMask == -1)
                        {
                            try
                            {
                                _occlusionLayerMask = LayerMaskClass.HighPolyWithTerrainMask | LayerMaskClass.DoorLayer | LayerMaskClass.InteractiveLayer;
                            }
                            catch
                            {
                                _occlusionLayerMask = LayerMask.GetMask("HighPolyWithRaycast", "Terrain", "LowPoly", "Interactive", "Doors", "Default");
                            }
                        }

                        Vector3 targetHeadPos = transform.position;
                        Vector3 listenerPos = Vector3.zero;
                        if (Camera.main != null)
                        {
                            listenerPos = Camera.main.transform.position;
                        }
                        else if (Singleton<GameWorld>.Instantiated && Singleton<GameWorld>.Instance.MainPlayer != null)
                        {
                            listenerPos = Singleton<GameWorld>.Instance.MainPlayer.Position + Vector3.up * 1.6f;
                        }

                        if (listenerPos != Vector3.zero && Vector3.Distance(listenerPos, targetHeadPos) > 1.2f)
                        {
                            _isOccluded = Physics.Linecast(listenerPos, targetHeadPos, out RaycastHit hit, _occlusionLayerMask, QueryTriggerInteraction.Ignore);
                        }
                        else
                        {
                            _isOccluded = false;
                        }
                    }
                    else
                    {
                        _isOccluded = false;
                    }

                    _targetOcclusionFactor = _isOccluded ? 0.50f : 1.0f;
                    _targetOcclusionAirDamping = _isOccluded ? 0.25f : 1.0f;
                }
            }
            catch (Exception ex)
            {
                VoIPPlugin.Log.LogError($"[SFT] Erro no Update de RemoteSpeaker: {ex.Message}");
            }
        }

        void OnAudioFilterRead(float[] data, int channels)
        {
            filterReadCallsCount++;
            int wPos = streamWritePos;
            int rPos = streamReadPos;
            int available = (wPos - rPos + streamBuffer.Length) % streamBuffer.Length;

            if (VoIPPlugin.EnableDebugLogs != null && VoIPPlugin.EnableDebugLogs.Value && filterReadCallsCount % 500 == 1)
            {
                VoIPPlugin.Log.LogInfo($"[SFT-DEBUG] OnAudioFilterRead chamado #{filterReadCallsCount}. Available={available}, target={currentJitterTarget}, buffering={isBuffering}, channels={channels}");
            }

            // FIX: Clock Drift (Overrun Prevention)
            // Se o relógio do Sender for levemente mais rápido que o do Receiver,
            // o buffer vai encher lentamente ao longo de minutos.
            // Para evitar delay absurdo ou estouro de buffer, nós dropamos pacotes velhos.
            int maxAllowedDelay = jitterInitialSamples * 2; // Pelo menos 2x o Jitter inicial
            if (available > maxAllowedDelay)
            {
                // Avança o ponteiro de leitura para manter apenas o equivalente ao Jitter Inicial
                rPos = (wPos - jitterInitialSamples + streamBuffer.Length) % streamBuffer.Length;
                available = jitterInitialSamples;
            }

            if (isBuffering)
            {
                if (available >= currentJitterTarget)
                    isBuffering = false;
            }
            else
            {
                if (available == 0)
                {
                    isBuffering = true;
                    // Switch to recovery target for future underruns to minimize gap
                    currentJitterTarget = jitterRecoverySamples;
                }
            }

            bool shouldPlay = !isBuffering;

            // ATENUAÇÃO 3D DE PROXIMIDADE E ABSORÇÃO ATMOSFÉRICA DO AR
            float distanceAttenuation = 1.0f;
            float airDampingAlpha = 1.0f; // 1.0 = Brilho total de perto, <0.4 = Agudos abafados pelo ar
            Vector3 listenerPos = Vector3.zero;

            if (currentSpatialBlend > 0f)
            {
                if (Camera.main != null)
                {
                    listenerPos = Camera.main.transform.position;
                }
                else if (Singleton<GameWorld>.Instantiated && Singleton<GameWorld>.Instance.MainPlayer != null)
                {
                    listenerPos = Singleton<GameWorld>.Instance.MainPlayer.Position;
                }

                if (listenerPos != Vector3.zero)
                {
                    float dist = Vector3.Distance(listenerPos, transform.position);
                    float minD = audioSource != null ? audioSource.minDistance : 1.5f;
                    float maxD = currentDistanceTarget; // Distância máxima configurável (ex: 30m)

                    if (dist <= minD)
                    {
                        distanceAttenuation = 1.0f;
                        airDampingAlpha = 1.0f;
                    }
                    else if (dist >= maxD)
                    {
                        distanceAttenuation = 0.0f; // Silêncio absoluto se estiver a mais de 30m (ex: 600m)!
                        airDampingAlpha = 0.15f;
                    }
                    else
                    {
                        // 1. Curva Acústica Suave (-3dB por dobra de distância em escala humana):
                        // 1.5m -> 100% | 5m -> ~85% | 10m -> ~68% | 20m -> ~38% | 30m -> 0%
                        float normD = (dist - minD) / (maxD - minD);
                        distanceAttenuation = Mathf.Pow(1.0f - normD, 1.2f);

                        // 2. Absorção Atmosférica do Ar Nítida (preserva o brilho e agudos a média distância):
                        airDampingAlpha = Mathf.Lerp(1.0f, 0.60f, normD);
                    }
                }
            }

            // ── INTERPOLAÇÃO DA OCLUSÃO FÍSICA POR PAREDES & PORTAS ──
            _smoothedOcclusionFactor = Mathf.Lerp(_smoothedOcclusionFactor, _targetOcclusionFactor, 0.05f);
            _smoothedOcclusionAirDamping = Mathf.Lerp(_smoothedOcclusionAirDamping, _targetOcclusionAirDamping, 0.05f);

            distanceAttenuation *= _smoothedOcclusionFactor;
            airDampingAlpha *= _smoothedOcclusionAirDamping;

            float volumeMult = audioSource != null ? audioSource.volume : 1.0f;
            if (float.IsNaN(distanceAttenuation) || float.IsInfinity(distanceAttenuation)) distanceAttenuation = 1.0f;
            if (float.IsNaN(volumeMult) || float.IsInfinity(volumeMult)) volumeMult = 1.0f;

            float finalAttenuation = distanceAttenuation * volumeMult;

            // ── CÁLCULO DE PANNING 3D ESTÉREO (DIREÇÃO ESQUERDA / DIREITA) ──
            float panLeftGain = 1.0f;
            float panRightGain = 1.0f;

            if (currentSpatialBlend > 0f && listenerPos != Vector3.zero)
            {
                Vector3 listenerRight = Vector3.right;
                if (Camera.main != null)
                {
                    listenerRight = Camera.main.transform.right;
                }
                else if (Singleton<GameWorld>.Instantiated && Singleton<GameWorld>.Instance.MainPlayer != null)
                {
                    var mp = Singleton<GameWorld>.Instance.MainPlayer;
                    if (mp.Transform != null) listenerRight = mp.Transform.Original.right;
                }

                Vector3 dirToSpeaker = transform.position - listenerPos;
                dirToSpeaker.y = 0; // Panning horizontal plano XZ
                if (dirToSpeaker.sqrMagnitude > 0.01f)
                {
                    dirToSpeaker.Normalize();
                    float pan = Vector3.Dot(dirToSpeaker, listenerRight); // -1.0 (Esquerda) até +1.0 (Direita)
                    pan = Mathf.Clamp(pan, -1.0f, 1.0f);

                    // Lei de Panning Potência Constante (-3dB Pan Law):
                    float angle = (pan + 1.0f) * Mathf.PI * 0.25f; // [0, PI/2]
                    panLeftGain  = Mathf.Cos(angle);
                    panRightGain = Mathf.Sin(angle);
                }
            }

            if (float.IsNaN(panLeftGain) || float.IsInfinity(panLeftGain)) panLeftGain = 1.0f;
            if (float.IsNaN(panRightGain) || float.IsInfinity(panRightGain)) panRightGain = 1.0f;

            for (int i = 0; i < data.Length; i += channels)
            {
                if (shouldPlay && rPos != wPos)
                {
                    lastSample = streamBuffer[rPos];
                    rPos = (rPos + 1) % streamBuffer.Length;
                }
                else
                {
                    if (shouldPlay && rPos == wPos) UnderrunCount++;
                    // Decay suave (fade-out de 1ms) para evitar o estalo de descontinuidade DC (chiado)
                    lastSample *= 0.95f;
                }

                float targetSample = lastSample * finalAttenuation;

                // Aplica o filtro de absorção do ar (Single-Pole Low-Pass Filter ultraleve)
                lpfState = lpfState + airDampingAlpha * (targetSample - lpfState);

                if (channels >= 2)
                {
                    data[i + 0] = lpfState * panLeftGain;  // Orelha Esquerda
                    data[i + 1] = lpfState * panRightGain; // Orelha Direita
                    for (int c = 2; c < channels; c++)
                        data[i + c] = lpfState;
                }
                else
                {
                    data[i] = lpfState;
                }
            }

            streamReadPos = rPos;
            
            float sum = 0f;
            for (int i = 0; i < data.Length; i++)
            {
                sum += data[i] * data[i];
            }
            CurrentOutputLevel = Mathf.Sqrt(sum / data.Length);
        }

        void OnDestroy()
        {
            decoder = null!;
        }
    }
}