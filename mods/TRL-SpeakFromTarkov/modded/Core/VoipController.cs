using BepInEx.Configuration;
using UnityEngine;
using TRL_SpeakFromTarkov.Audio;
using TRL_SpeakFromTarkov.Network;
using TRL_SpeakFromTarkov.UI;
using System;

namespace TRL_SpeakFromTarkov.Core
{
    public class VoipController : MonoBehaviour
    {
        public static VoipController Instance { get; private set; }
        
        private MicrophoneCapturer capturer;
        private VoipProcessor processor;
        private SftNetwork network;
        private VoipHUD hud;
        private RemoteSpeaker echoSpeaker;
        
        public byte CurrentChannel { get; private set; } = 1;
        
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
                VoIPPlugin.Log.LogInfo("[SFT] Servidor Headless detectado. Inicializando apenas a rede P2P (SftNetwork).");
                return;
            }
            
            capturer = gameObject.AddComponent<MicrophoneCapturer>();
            capturer.Initialize(sampleRate, frameSize);
            
            processor = gameObject.AddComponent<VoipProcessor>();
            processor.Initialize(sampleRate, frameSize);
            
            echoSpeaker = gameObject.AddComponent<RemoteSpeaker>();
            echoSpeaker.Initialize(sampleRate, frameSize, 0f); // 2D Audio for Echo
            
            hud = gameObject.AddComponent<VoipHUD>();
            hud.Initialize();
            hud.Processor = processor;
            
            // Wiring Events
            capturer.OnAudioDataCaptured += processor.ProcessAudio;
            processor.OnOpusDataEncoded += (opusData) => {
                network.Broadcast(opusData, CurrentChannel);
                
                if (VoIPPlugin.EchoDelay.Value > 0f && !processor.IsMuted)
                {
                    StartCoroutine(DelayEcho(opusData, VoIPPlugin.EchoDelay.Value));
                }
            };
            
            // StartCapture NÃO é chamado aqui.
            // Vivox inicializa o microfone durante o carregamento do jogo e trava o dispositivo.
            // Aguardamos o menu carregar (OnMenuSceneLoaded) para tentar depois que o Vivox terminar.
            VoIPPlugin.Log.LogInfo("[SFT] VoipController iniciado. Aguardando menu para abrir microfone...");
        }
        
        private System.Collections.IEnumerator DelayEcho(byte[] data, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (echoSpeaker != null)
            {
                echoSpeaker.SetVolume(VoIPPlugin.EchoVolume.Value);
                echoSpeaker.EnqueuePacket(data);
            }
        }
        
        private bool isMenuLoaded = false;
        private Coroutine? fallbackStartCoroutine = null;

        // Chamado pelo VOIPPlugin quando a cena MenuUIScene carrega
        public void OnMenuSceneLoaded()
        {
            if (isMenuLoaded || capturer == null) return;
            isMenuLoaded = true;
            
            VoIPPlugin.Log.LogInfo("[SFT] Menu carregado. Iniciando timer fallback de 12 segundos para a captura do microfone...");
            fallbackStartCoroutine = StartCoroutine(FallbackStartVoipCo());
        }

        private System.Collections.IEnumerator FallbackStartVoipCo()
        {
            yield return new WaitForSeconds(12f);
            VoIPPlugin.Log.LogInfo("[SFT] Tempo limite de fallback atingido (12s). Forçando inicialização da captura de microfone...");
            fallbackStartCoroutine = null;
            StartVoipCapture();
        }

        public void OnHipLoadCompleted()
        {
            if (capturer == null || capturer.IsRecording) return;

            VoIPPlugin.Log.LogInfo("[SFT] Carregamento seguro detectado (/hip/load concluído). Inicializando captura de microfone...");
            if (fallbackStartCoroutine != null)
            {
                StopCoroutine(fallbackStartCoroutine);
                fallbackStartCoroutine = null;
            }
            StartVoipCapture();
        }

        private void StartVoipCapture()
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
                    VoIPPlugin.Log.LogInfo($"[SFT] Usando microfone nativo do Tarkov: {device}");
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
            if (capturer != null)
            {
                VoIPPlugin.Log.LogInfo($"[SFT] Mudança de microfone solicitada no F12 para: '{newDevice}'");
                capturer.StopCapture();
                capturer.StartCapture(newDevice);
            }
        }

        private float micRetryTimer = 0f;
        private const float MIC_RETRY_INTERVAL = 5f;
        
        void Update()
        {
            if (capturer == null) return;
            
            HandleKeys();
            hud.CurrentChannel = CurrentChannel;
            
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
        }
        
        private void HandleKeys()
        {
            if (IsShortcutDown(VoIPPlugin.ToggleModeKey.Value))
            {
                processor.CurrentMode = (VoipProcessor.VoipMode)(((int)processor.CurrentMode + 1) % 3);
                VoIPPlugin.Log.LogInfo($"[SFT] Modo → {processor.CurrentMode}");
            }

            if (IsShortcutDown(VoIPPlugin.MuteKey.Value))
            {
                processor.IsMuted = !processor.IsMuted;
                VoIPPlugin.Log.LogInfo($"[SFT] Mute: {(processor.IsMuted ? "ON" : "OFF")}");
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
