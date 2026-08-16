using BepInEx.Configuration;
using UnityEngine;
using TRL_SpeakFromTarkov.Audio;
using TRL_SpeakFromTarkov.Network;
using TRL_SpeakFromTarkov.UI;
using System;
using System.Collections.Concurrent;
using Comfort.Common;
using EFT.UI;

namespace TRL_SpeakFromTarkov.Core
{
    public class VoipController : MonoBehaviour
    {
        public static VoipController Instance { get; private set; } = null!;
        
        public MicrophoneCapturer capturer { get; private set; } = null!;
        public VoipProcessor processor { get; private set; } = null!;
        public RemoteSpeaker echoSpeaker { get; private set; } = null!;
        public BotVoiceBridge botVoiceBridge { get; private set; } = null!;
        private SftNetwork network = null!;
        private VoipHUD hud = null!;
        public InRaidVoipHUD inRaidHud { get; private set; } = null!;
        public VoiceCalibrationHUD calibrationHud { get; private set; } = null!;
        public PlayerVolumeMixerHUD playerMixerHud { get; private set; } = null!;
        private ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();
        
        public byte CurrentChannel { get; private set; } = 1;
        public MenuVoipHUD menuHud { get; private set; } = null!;

        public void SetCurrentChannel(byte channelId)
        {
            CurrentChannel = channelId;
            VoIPPlugin.Log.LogInfo($"[SFT] Canal de áudio alterado para: {channelId}");
        }
        
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            
            int sampleRate = VoIPPlugin.SampleRate.Value;
            int frameSize = VoIPPlugin.FrameSize;
            
            bool isHeadless = Fika.Core.Main.Utils.FikaBackendUtils.IsHeadless;
            
            network = gameObject.AddComponent<SftNetwork>();
            network.Initialize(sampleRate, frameSize);
            
            if (isHeadless)
            {
                VoIPPlugin.Log.LogInfo("[SFT] Servidor Headless detectado. Inicializando apenas a rede P2P (SftNetwork) silenciosa.");
                return;
            }
            
            var capturerGo = new GameObject("SftMicrophoneCapturer");
            capturerGo.transform.SetParent(this.transform);
            capturer = capturerGo.AddComponent<MicrophoneCapturer>();
            capturer.Initialize(sampleRate, frameSize);
            
            processor = gameObject.AddComponent<VoipProcessor>();
            processor.Initialize(sampleRate, frameSize);
            
            botVoiceBridge = gameObject.AddComponent<BotVoiceBridge>();
            processor.botVoiceBridge = botVoiceBridge;
            
            var echoGo = new GameObject("SftEchoSpeaker");
            echoGo.transform.SetParent(this.transform);
            echoSpeaker = echoGo.AddComponent<RemoteSpeaker>();
            echoSpeaker.Initialize(sampleRate, frameSize, 0f); // 2D Audio for Echo
            
            hud = gameObject.AddComponent<VoipHUD>();
            hud.Initialize();
            hud.Processor = processor;

            inRaidHud = gameObject.AddComponent<InRaidVoipHUD>();
            inRaidHud.Initialize();
            inRaidHud.Processor = processor;

            calibrationHud = gameObject.AddComponent<VoiceCalibrationHUD>();
            calibrationHud.Initialize();
            calibrationHud.Processor = processor;
            
            playerMixerHud = gameObject.AddComponent<PlayerVolumeMixerHUD>();

            menuHud = gameObject.AddComponent<MenuVoipHUD>();
            
            // Wiring Events
            capturer.OnAudioDataCaptured += (samples) => {
                if (processor.CapturerFilter == null && capturer.Filter != null)
                {
                    processor.CapturerFilter = capturer.Filter;
                }
                processor.ProcessAudio(samples);
            };
            processor.OnOpusDataEncoded += (opusData, voiceLevel) => {
                // Roda na THREAD DE CAPTURA. Broadcast apenas ENFILEIRA; a transmissão acontece no
                // Update do SftNetwork (main thread), porque FikaClient/FikaServer serializam todos
                // os envios num único NetDataWriter de instância sem lock — enviar daqui corromperia
                // esse buffer compartilhado e colocaria um datagrama malformado na rede.
                network.Broadcast(opusData, CurrentChannel, voiceLevel);

                if (VoIPPlugin.EnableEcho != null && VoIPPlugin.EnableEcho.Value && !processor.IsMuted)
                {
                    mainThreadActions.Enqueue(() => {
                        float delay = VoIPPlugin.EchoDelay.Value;
                        if (delay <= 0.02f)
                        {
                            if (echoSpeaker != null)
                            {
                                echoSpeaker.SetVolume(VoIPPlugin.EchoVolume.Value);
                                echoSpeaker.EnqueuePacket(opusData);
                            }
                            else
                            {
                                VoIPPlugin.Log.LogError("[SFT-DEBUG] echoSpeaker é NULL!");
                            }
                        }
                        else
                        {
                            StartCoroutine(DelayEcho(opusData, delay));
                        }
                    });
                }
            };
            
            // StartCapture NÃO é chamado aqui.
            // Vivox inicializa o microfone durante o carregamento do jogo e trava o dispositivo.
            // Aguardamos o menu carregar (OnMenuSceneLoaded) para tentar depois que o Vivox terminar.
            VoIPPlugin.Log.LogInfo("[SFT] VoipController iniciado. Aguardando menu para abrir microfone...");
        }
        
        private System.Collections.IEnumerator DelayEcho(byte[] data, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (echoSpeaker != null)
            {
                echoSpeaker.SetVolume(VoIPPlugin.EchoVolume.Value);
                echoSpeaker.EnqueuePacket(data);
            }
            else
            {
                VoIPPlugin.Log.LogError("[SFT-DEBUG] DelayEcho: echoSpeaker é NULL!");
            }
        }
        
        private bool isMenuLoaded = false;

        // Chamado pelo VOIPPlugin quando a cena MenuUIScene carrega
        public void OnMenuSceneLoaded()
        {
            if (isMenuLoaded || capturer == null) return;
            isMenuLoaded = true;
            
            VoIPPlugin.Log.LogInfo("[SFT] MenuUIScene carregado com sucesso. Agendando abertura limpa do microfone em 1.0s (pós-Vivox)...");
            StartCoroutine(DelayedStartVoipCaptureCo());
        }

        private System.Collections.IEnumerator DelayedStartVoipCaptureCo()
        {
            // Margem de segurança de 3.0s após a UI do menu carregar completamente
            yield return new WaitForSecondsRealtime(3.0f);

            // No Menu Principal, NÃO ativamos o microfone automaticamente!
            // O microfone permanece DESLIGADO até o jogador Criar ou Entrar em um Canal no MenuVoipHUD.
            VoIPPlugin.Log.LogInfo("[SFT] Menu principal do Tarkov 100% carregado. Microfone mantido DESLIGADO no menu até entrar em um canal.");
        }

        public void EnableMenuCapture(bool enable)
        {
            if (Singleton<EFT.GameWorld>.Instantiated) return;

            if (enable)
            {
                if (capturer != null)
                {
                    string device = GetEftMicrophone();
                    capturer.StartCapture(device);
                    VoIPPlugin.Log.LogInfo("[SFT-MENU] Microfone ATIVADO para o canal do menu.");
                }
            }
            else
            {
                if (capturer != null && capturer.IsRecording)
                {
                    capturer.StopCapture();
                    VoIPPlugin.Log.LogInfo("[SFT-MENU] Microfone DESLIGADO ao sair do canal do menu.");
                }
            }
        }

        public void StartVoipCapture()
        {
            if (capturer == null || capturer.IsRecording) return;
            string device = GetEftMicrophone();
            capturer.StartCapture(device);
        }
        
        private string GetEftMicrophone()
        {
            try
            {
                var device = global::SoundSettingsControllerClass.DefaultMicrophone;
                if (!string.IsNullOrEmpty(device)) 
                {
                    return device;
                }
            }
            catch (Exception ex)
            {
                VoIPPlugin.Log.LogWarning($"[SFT] Não foi possível obter microfone do Tarkov ({ex.Message}). Usando fallback.");
            }
            return VoIPPlugin.GetSelectedMicrophone();
        }
        
        public void OnMicrophoneChanged(string newDevice)
        {
            if (!isMenuLoaded) return;

            if (capturer != null)
            {
                VoIPPlugin.Log.LogInfo($"[SFT] Mudança de microfone solicitada para: '{newDevice}'");
                capturer.StopCapture();
                capturer.StartCapture(newDevice);
            }
        }
        
        public void ToggleModState(bool isEnabled)
        {
            if (isEnabled)
            {
                VoIPPlugin.Log.LogInfo("[SFT] Mod de voz reativado no F12.");
                if (isMenuLoaded && capturer != null && !capturer.IsRecording)
                {
                    if (Singleton<EFT.GameWorld>.Instantiated)
                    {
                        StartVoipCapture();
                    }
                }
            }
            else
            {
                VoIPPlugin.Log.LogInfo("[SFT] Mod de voz totalmente desativado no F12. Parando captura de mic...");
                if (capturer != null && capturer.IsRecording)
                {
                    capturer.StopCapture();
                }
                if (echoSpeaker != null)
                {
                    echoSpeaker.SetVolume(0f);
                }
            }
        }

        private float micRetryTimer = 0f;
        private const float MIC_RETRY_INTERVAL = 5f;
        
        void Update()
        {
            if (VoIPPlugin.EnableMod != null && !VoIPPlugin.EnableMod.Value) return;
            if (capturer == null || !isMenuLoaded) return;
            
            while (mainThreadActions.TryDequeue(out Action action))
            {
                action?.Invoke();
            }
            
            HandleKeys();
            if (hud != null) hud.CurrentChannel = CurrentChannel;
            if (inRaidHud != null) inRaidHud.CurrentChannel = CurrentChannel;
            
            // Interação de Voz com Bots (Main Thread Safe + Janela de Amostragem 250ms)
            if (processor != null && botVoiceBridge != null)
            {
                if (Singleton<EFT.GameWorld>.Instantiated && Singleton<EFT.GameWorld>.Instance.MainPlayer != null)
                {
                    botVoiceBridge.ProcessVoiceFrame(
                        Singleton<EFT.GameWorld>.Instance.MainPlayer, 
                        processor.DisplayLevel, 
                        processor.IsTransmitting
                    );
                }
            }
            
            // Retry automático se a captura abortou por clip inválido (freq=0)
            if (!capturer.IsRecording)
            {
                micRetryTimer += Time.deltaTime;
                if (micRetryTimer >= MIC_RETRY_INTERVAL)
                {
                    micRetryTimer = 0f;
                    VoIPPlugin.Log.LogInfo("[SFT] Tentando reabrir microfone...");
                    string device = GetEftMicrophone();
                    capturer.StartCapture(device);
                }
            }
            else
            {
                micRetryTimer = 0f;
            }

            RunAudioProfilerReport();
        }

        private float profilerLogTimer = 0f;
        private void RunAudioProfilerReport()
        {
            if (!VoIPPlugin.IsAudioDebugActive) return;
            
            profilerLogTimer += Time.deltaTime;
            if (profilerLogTimer >= 0.5f)
            {
                profilerLogTimer = 0f;
                
                string micName = GetEftMicrophone();
                bool micRec = capturer != null && capturer.IsRecording;
                int hwRate = capturer != null ? capturer.ActualSampleRate : 0;
                bool isResampling = capturer != null && capturer.IsResampling;
                int clips = capturer != null ? capturer.ClipCount : 0;
                
                float inRMS = processor != null ? processor.RawLevel : 0f;
                float displayLvl = processor != null ? processor.DisplayLevel : 0f;
                bool isTx = processor != null && processor.IsTransmitting;
                string mode = processor != null ? processor.CurrentMode.ToString() : "N/A";
                int opusBytes = processor != null ? processor.LastOpusBytes : 0;
                
                float outRMS = echoSpeaker != null ? echoSpeaker.CurrentOutputLevel : 0f;
                bool echoPlaying = echoSpeaker != null && echoSpeaker.IsPlaying;
                int echoPackets = echoSpeaker != null ? echoSpeaker.PacketsEnqueued : 0;
                int echoFilterCalls = echoSpeaker != null ? echoSpeaker.FilterCalls : 0;
                int echoAvail = echoSpeaker != null ? echoSpeaker.BufferAvailable : 0;
                bool echoBuf = echoSpeaker != null && echoSpeaker.IsBuffering;
                int totalUnderruns = echoSpeaker != null ? echoSpeaker.UnderrunCount : 0;
                int recentUnderruns = echoSpeaker != null ? echoSpeaker.GetRecentUnderruns() : 0;

                VoIPPlugin.Log.LogInfo($"[SFT-PROFILER] ── DIAGNÓSTICO PROFUNDO DE ÁUDIO ──");
                VoIPPlugin.Log.LogInfo($"[SFT-PROFILER] 1. CAPTURA RAW : Mic='{micName}' | HwDSP={hwRate}Hz | Resampling={isResampling} | PeakClips={clips} | RawRMS={inRMS:F4}");
                VoIPPlugin.Log.LogInfo($"[SFT-PROFILER] 2. PROCESSOR   : Modo={mode} | Transmitindo={isTx} | RNNoise={VoIPPlugin.UseRNNoise.Value} | AGC={VoIPPlugin.EnableAGC.Value} | OpusByte={opusBytes}B");
                VoIPPlugin.Log.LogInfo($"[SFT-PROFILER] 3. REPRODUÇÃO  : Tocando={echoPlaying} | OutRMS={outRMS:F4} | Pacotes={echoPackets} | Calls={echoFilterCalls} | Avail={echoAvail} | Underruns (0.5s)={recentUnderruns} (Total: {totalUnderruns})");
                
                // Alertas de Vilões da Qualidade em tempo real (baseado no delta dos últimos 0.5s)
                if (isResampling)
                    VoIPPlugin.Log.LogWarning($"[SFT-PROFILER] ⚠️ ALERTA RESAMPLING: O Windows está entregando áudio em {hwRate}Hz em vez de 48000Hz nativo. Isso causa chiado por interpolação!");
                if (clips > 0)
                    VoIPPlugin.Log.LogWarning($"[SFT-PROFILER] ⚠️ ALERTA CLIPPING: {clips} amostras estouraram >0.90 no microfone. Reduza 'Ganho do Microfone' no F12!");
                if (recentUnderruns > 0)
                    VoIPPlugin.Log.LogWarning($"[SFT-PROFILER] ⚠️ ALERTA UNDERRUN RECENTE: Ocorreram {recentUnderruns} engasgos no alto-falante nos últimos 0.5s!");
                
                VoIPPlugin.Log.LogInfo($"[SFT-PROFILER] ──────────────────────────────────────────");
            }
        }
        
        private void HandleKeys()
        {
            if (IsShortcutDown(VoIPPlugin.ToggleModeKey.Value))
            {
                processor.CurrentMode = (VoipProcessor.VoipMode)(((int)processor.CurrentMode + 1) % 3);
                if (VoIPPlugin.TransmissionMode != null)
                {
                    VoIPPlugin.TransmissionMode.Value = VoIPPlugin.GetVoipModeString(processor.CurrentMode);
                }
                VoIPPlugin.Log.LogInfo($"[SFT] Modo → {processor.CurrentMode}");
            }

            if (IsShortcutDown(VoIPPlugin.MuteKey.Value))
            {
                processor.IsMuted = !processor.IsMuted;
                VoIPPlugin.Log.LogInfo($"[SFT] Mute: {(processor.IsMuted ? "ON" : "OFF")}");
            }

            if (VoIPPlugin.OpenCalibrationKey != null && IsShortcutDown(VoIPPlugin.OpenCalibrationKey.Value))
            {
                if (calibrationHud != null)
                {
                    calibrationHud.ToggleWizard();
                }
            }

            if (VoIPPlugin.PlayerMixerKey != null && IsShortcutDown(VoIPPlugin.PlayerMixerKey.Value))
            {
                if (playerMixerHud != null)
                {
                    playerMixerHud.Toggle();
                }
            }

            processor.IsPTTActive = IsShortcutHeld(VoIPPlugin.PushToTalkKey.Value);
        }
        
        private bool IsShortcutEmpty(KeyboardShortcut shortcut)
        {
            return shortcut.MainKey == KeyCode.None;
        }

        private bool IsShortcutHeld(KeyboardShortcut shortcut)
        {
            if (IsShortcutEmpty(shortcut)) return false;
            if (!Input.GetKey(shortcut.MainKey)) return false;

            foreach (var mod in shortcut.Modifiers)
            {
                if (!Input.GetKey(mod)) return false;
            }
            return true;
        }

        private bool IsShortcutDown(KeyboardShortcut shortcut)
        {
            if (IsShortcutEmpty(shortcut)) return false;
            if (!Input.GetKeyDown(shortcut.MainKey)) return false;

            foreach (var mod in shortcut.Modifiers)
            {
                if (!Input.GetKey(mod)) return false;
            }
            return true;
        }

        public void SetGameStateChannel(bool inRaid) 
        {
            CurrentChannel = inRaid ? (byte)0 : (byte)1;
            
            if (inRaid && !network.IsSessionActive)
            {
                network.InitFikaSession();
            }
            else if (!inRaid && network.IsSessionActive)
            {
                network.StopSession();
            }
        }
        
        public void SetPlayerStatus(bool isSpectator) 
        {
            CurrentChannel = isSpectator ? (byte)2 : CurrentChannel;
        }
        
        public void Cleanup()
        {
            capturer?.StopCapture();
            network?.StopSession();
        }
        
        void OnDestroy()
        {
            Cleanup();
        }
    }
}
