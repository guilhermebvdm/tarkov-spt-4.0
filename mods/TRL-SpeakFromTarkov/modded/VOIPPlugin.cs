using BepInEx;
using System;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TRL_SpeakFromTarkov
{
    [BepInPlugin("trl.speakfromtarkov", "TRL-SpeakFromTarkov", "1.5.0")]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.HardDependency)]
    public class VoIPPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log = null!;

        public static ConfigEntry<string> MicrophoneDevice { get; private set; } = null!;
        public static string[] MicrophoneNames { get; private set; } = null!;
        public static System.Collections.Generic.Dictionary<string, string> MicRealNames = new System.Collections.Generic.Dictionary<string, string>();
        public static ConfigEntry<bool> EnableMod { get; private set; } = null!;
        public static ConfigEntry<TRL_SpeakFromTarkov.Audio.VoipProcessor.VoipMode> TransmissionMode { get; private set; } = null!;
        public static ConfigEntry<float> VADThreshold { get; private set; } = null!;
        public static ConfigEntry<bool> EnableEcho { get; private set; } = null!;
        public static ConfigEntry<float> EchoDelay { get; private set; } = null!;
        public static ConfigEntry<float> EchoVolume { get; private set; } = null!;
        public static ConfigEntry<float> MicGain { get; private set; } = null!;
        public static ConfigEntry<int> SampleRate { get; private set; } = null!;
        public static ConfigEntry<float> NetworkJitterBufferMs { get; private set; } = null!;
        
        // Studio Quality configs
        public static ConfigEntry<int> OpusBitrate { get; private set; } = null!;
        public static ConfigEntry<int> OpusComplexity { get; private set; } = null!;
        public static ConfigEntry<bool> OpusFEC { get; private set; } = null!;
        public static ConfigEntry<bool> EnableAGC { get; private set; } = null!;
        public static ConfigEntry<bool> EnableLimiter { get; private set; } = null!;
        public static ConfigEntry<float> LPFCutoff { get; private set; } = null!;
        public static ConfigEntry<float> MaxHearingDistance { get; private set; } = null!;
        public static ConfigEntry<float> OutputVolume { get; private set; } = null!;

        // Bot Interaction
        public static ConfigEntry<bool> EnableBotInteraction { get; private set; } = null!;
        public static ConfigEntry<float> BotVoiceDebugVolume { get; private set; } = null!;

        // CONFIGURAÇÃO IDEAL: KeyboardShortcut
        public static ConfigEntry<KeyboardShortcut> PushToTalkKey { get; private set; } = null!;
        public static ConfigEntry<KeyboardShortcut> ToggleModeKey { get; private set; } = null!;
        public static ConfigEntry<KeyboardShortcut> MuteKey { get; private set; } = null!;
        public static ConfigEntry<bool> EnableInRaidVoipHUD { get; private set; } = null!;
        public static ConfigEntry<TRL_SpeakFromTarkov.UI.HudVisibilityMode> HudVisibility { get; private set; } = null!;
        public static ConfigEntry<float> ShiftStancePanelX { get; private set; } = null!;
        public static ConfigEntry<float> InRaidHUDOffsetX { get; private set; } = null!;
        public static ConfigEntry<float> InRaidHUDOffsetY { get; private set; } = null!;
        public static ConfigEntry<bool> EnableDebugVoipHUD { get; private set; } = null!;

        // Calibração de Voz Personalizada (Wizard)
        public static ConfigEntry<float> WhisperThreshold { get; private set; } = null!;
        public static ConfigEntry<float> NormalThreshold { get; private set; } = null!;
        public static ConfigEntry<float> LoudThreshold { get; private set; } = null!;
        public static ConfigEntry<KeyboardShortcut> OpenCalibrationKey { get; private set; } = null!;

        public static ConfigEntry<bool> EnableDebugLogs { get; private set; } = null!;
        public static bool IsAudioDebugActive { get; set; } = false;

        public static ConfigEntry<float> VADDecayTime { get; private set; } = null!;
        public static ConfigEntry<float> MaxAudioLevel { get; private set; } = null!;

        // Filtros de áudio
        public static ConfigEntry<float> HPFCutoff          { get; private set; } = null!;
        public static ConfigEntry<float> NoiseGateThreshold { get; private set; } = null!;
        public static ConfigEntry<float> NoiseGateHoldMs    { get; private set; } = null!;

        public static ConfigEntry<bool> UseRNNoise          { get; private set; } = null!;
        public static ConfigEntry<float> RNNoiseVADThreshold { get; private set; } = null!;
        public static ConfigEntry<float> RNNoiseGateHoldMs    { get; private set; } = null!;
        public static ConfigEntry<int> RNNoiseLatency       { get; private set; } = null!;

        public static int FrameSize => (int)Math.Round(SampleRate.Value * 0.040);

        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string libname);

        void Awake()
        {
            Log = base.Logger;
            // Versão vem do BepInPlugin (fonte canônica) — hardcodar aqui já fez o log mentir
            // sobre a build em campo, e a paridade de versão entre peers é diagnóstico crítico
            // de rede (o hash do pacote deriva do nome do tipo, não da versão).
            Log.LogInfo($"[SFT] Iniciando TRL-SpeakFromTarkov v{Info.Metadata.Version}...");

            // Carrega explicitamente a rnnoise.dll da pasta do plugin
            try
            {
                string assemblyFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string dllPath = System.IO.Path.Combine(assemblyFolder, "rnnoise.dll");
                if (System.IO.File.Exists(dllPath))
                {
                    IntPtr hLib = LoadLibrary(dllPath);
                    if (hLib != IntPtr.Zero)
                        Log.LogInfo($"[SFT] rnnoise.dll carregada com sucesso via LoadLibrary: {dllPath}");
                    else
                        Log.LogWarning($"[SFT] Falha ao carregar rnnoise.dll via LoadLibrary (Erro: {System.Runtime.InteropServices.Marshal.GetLastWin32Error()}): {dllPath}");
                }
                else
                {
                    Log.LogWarning($"[SFT] rnnoise.dll nao encontrada na pasta do plugin: {dllPath}");
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[SFT] Erro ao carregar rnnoise.dll: {ex.Message}");
            }

            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
                Application.RequestUserAuthorization(UserAuthorization.Microphone);

            var rawMics = Microphone.devices.Length > 0 ? Microphone.devices : new string[] { "Nenhum microfone detectado" };
            MicrophoneNames = new string[rawMics.Length];
            
            for (int i = 0; i < rawMics.Length; i++)
            {
                string realName = rawMics[i];
                string displayName = realName;
                if (displayName.Length > 40)
                {
                    displayName = displayName.Substring(0, 37) + "...";
                }
                
                // Em caso de colisão de nomes truncados
                int suffix = 1;
                string originalDisplay = displayName;
                while (MicRealNames.ContainsKey(displayName))
                {
                    displayName = originalDisplay + " (" + suffix + ")";
                    suffix++;
                }

                MicRealNames[displayName] = realName;
                MicrophoneNames[i] = displayName;
            }

            var micList = new AcceptableValueList<string>(MicrophoneNames);

            MicrophoneDevice = Config.Bind("VOIP", "Microphone Device", MicrophoneNames[0], new ConfigDescription("Select the active microphone device.", micList));
            MicrophoneDevice.SettingChanged += (sender, args) =>
            {
                if (TRL_SpeakFromTarkov.Core.VoipController.Instance != null)
                {
                    TRL_SpeakFromTarkov.Core.VoipController.Instance.OnMicrophoneChanged(GetSelectedMicrophone());
                }
            };

            EnableMod = Config.Bind("General", "Enable Voice Mod", true, "If disabled, completely turns off voice capture and playback (as if the mod was not installed).");
            EnableMod.SettingChanged += (sender, args) =>
            {
                if (TRL_SpeakFromTarkov.Core.VoipController.Instance != null)
                {
                    TRL_SpeakFromTarkov.Core.VoipController.Instance.ToggleModState(EnableMod.Value);
                }
            };
            
            TransmissionMode = Config.Bind("VOIP", "Transmission Mode", TRL_SpeakFromTarkov.Audio.VoipProcessor.VoipMode.VAD, "Select voice transmission mode: VAD (Voice Activity Detection), PTT (Push To Talk) or Open (Always On).");
            TransmissionMode.SettingChanged += (sender, args) =>
            {
                if (TRL_SpeakFromTarkov.Core.VoipController.Instance != null && TRL_SpeakFromTarkov.Core.VoipController.Instance.processor != null)
                {
                    TRL_SpeakFromTarkov.Core.VoipController.Instance.processor.CurrentMode = TransmissionMode.Value;
                }
            };
            VADThreshold = Config.Bind("VOIP", "VAD Sensitivity Threshold", 0.005f, "Sensitivity threshold for VAD voice activation.");
            EnableEcho = Config.Bind("VOIP", "Enable Local Echo Loopback", true, "If enabled, plays back your own voice locally for microphone testing.");
            EchoDelay = Config.Bind("VOIP", "Echo Delay (s)", 0.0f, "Echo loopback delay in seconds (0.0 = instant real-time feedback).");
            EchoVolume = Config.Bind("VOIP", "Echo Volume", 1.0f, "Volume level of local echo loopback (0.0 to 1.0).");
            MicGain = Config.Bind("VOIP", "Microphone Gain", 1.0f,
                new ConfigDescription("Boosts raw audio input BEFORE filters and noise gate. Default: 1.0"));
            OutputVolume = Config.Bind("VOIP", "Output Volume", 1.0f,
                new ConfigDescription("Adjusts final output volume heard by peers (does not affect filters). Default: 1.0",
                    new AcceptableValueRange<float>(0.1f, 5.0f)));
            SampleRate = Config.Bind("VOIP", "Sample Rate", 48000);

            // Shortcuts
            PushToTalkKey = Config.Bind("VOIP", "Push To Talk Key", new KeyboardShortcut(KeyCode.V), "PTT shortcut key (e.g. V)");
            ToggleModeKey = Config.Bind("VOIP", "Toggle Mode Key", new KeyboardShortcut(KeyCode.P), "Toggle transmission mode shortcut key");
            MuteKey = Config.Bind("VOIP", "Mute Key", new KeyboardShortcut(KeyCode.M, KeyCode.LeftControl), "Mute microphone shortcut key");

            // UI / HUD Settings
            EnableInRaidVoipHUD = Config.Bind("UI / HUD Settings", "Show In-Raid VOIP HUD", true, "Displays thin vertical VOIP bar in-raid positioned to the left of the stance panel (BattleStancePanel).");
            HudVisibility = Config.Bind("UI / HUD Settings", "HUD Visibility", TRL_SpeakFromTarkov.UI.HudVisibilityMode.VoiceActivity,
                "Controls when the in-raid VOIP bar is displayed:\n• Hidden: Never visible\n• AlwaysVisible: Constantly visible during raid\n• SyncHUD: Synchronizes 1:1 with vanilla game HUD autohide\n• VoiceActivity: Automatically appears when voice is captured (RNNoise/VAD).");
            ShiftStancePanelX = Config.Bind("UI / HUD Settings", "Vanilla Stance Panel Offset X (Pixels)", 15f,
                new ConfigDescription("Shifts original EFT stance panel (stamina/pose) to the right on X axis to make room on screen edge. Default: 15px",
                    new AcceptableValueRange<float>(-50f, 150f)));
            InRaidHUDOffsetX = Config.Bind("UI / HUD Settings", "In-Raid HUD Offset X (Pixels)", 0f,
                new ConfigDescription("Horizontal offset for in-raid VOIP bar (X). Positive = Right, Negative = Left.",
                    new AcceptableValueRange<float>(-300f, 300f)));
            InRaidHUDOffsetY = Config.Bind("UI / HUD Settings", "In-Raid HUD Offset Y (Pixels)", 0f,
                new ConfigDescription("Vertical offset for in-raid VOIP bar (Y). Positive = Down, Negative = Up.",
                    new AcceptableValueRange<float>(-300f, 300f)));
            EnableDebugVoipHUD = Config.Bind("UI / HUD Settings", "Show Profiler / Debug HUD", false, "Displays extended diagnostic panel at the top of the screen.");

            // Voice Calibration Wizard (English Interface & Manual F12 Sliders)
            WhisperThreshold = Config.Bind("Voice Calibration", "Whisper Threshold (Notch 1)", 0.015f,
                new ConfigDescription("Calibrated RMS sensitivity threshold for Whisper (Level 1). Can be fine-tuned manually.",
                    new AcceptableValueRange<float>(0.001f, 0.300f)));
            NormalThreshold = Config.Bind("Voice Calibration", "Normal Voice Threshold (Notch 2)", 0.060f,
                new ConfigDescription("Calibrated RMS sensitivity threshold for Normal Voice (Level 2). Can be fine-tuned manually.",
                    new AcceptableValueRange<float>(0.002f, 0.400f)));
            LoudThreshold = Config.Bind("Voice Calibration", "Loud Voice Threshold (Max Ceiling)", 0.180f,
                new ConfigDescription("Calibrated RMS sensitivity threshold for Loud Voice / Shouting (Level 3). Can be fine-tuned manually.",
                    new AcceptableValueRange<float>(0.005f, 0.500f)));
            OpenCalibrationKey = Config.Bind("Voice Calibration", "Open Calibration Wizard Shortcut", new KeyboardShortcut(KeyCode.F8), "Shortcut key to open the interactive Voice Calibration Wizard.");
            EnableDebugLogs = Config.Bind("Diagnostics", "Enable Debug Logs", false, "If enabled, prints detailed packet enqueue logs to console. Default: false");

            VADDecayTime = Config.Bind("VOIP", "VAD Decay Time (s)", 0.7f);
            MaxAudioLevel = Config.Bind("VOIP", "Max Audio Level Ceiling", 0.015f);

            HPFCutoff          = Config.Bind("Audio Filters", "HPF Cutoff (Hz)", 80f,
                new ConfigDescription("High-pass filter cutoff frequency. Removes low-frequency rumble (keyboard, desk clicks). Default: 80Hz",
                    new AcceptableValueRange<float>(20f, 500f)));
            LPFCutoff          = Config.Bind("Audio Filters", "LPF Cutoff (Hz)", 8000f,
                new ConfigDescription("Low-pass filter cutoff frequency. Removes harsh static and high-pitched noise. Default: 8000Hz",
                    new AcceptableValueRange<float>(3000f, 20000f)));
            EnableAGC          = Config.Bind("Audio Filters", "Enable AGC (Automatic Gain Control)", true,
                new ConfigDescription("Normalizes voice volume smoothly, boosting whispers and limiting shouts."));
            EnableLimiter      = Config.Bind("Audio Filters", "Enable Audio Limiter", true,
                new ConfigDescription("Prevents audio clipping and protects listeners' ears from extreme shouting."));
            NoiseGateThreshold = Config.Bind("Audio Filters", "Noise Gate Threshold", 0.008f,
                new ConfigDescription("Minimum RMS sensitivity to open noise gate. Default: 0.008",
                    new AcceptableValueRange<float>(0.001f, 0.1f)));
            NoiseGateHoldMs    = Config.Bind("Audio Filters", "Noise Gate Hold (ms)", 150f,
                new ConfigDescription("Time in ms noise gate stays open after speech stops. Default: 150ms",
                    new AcceptableValueRange<float>(50f, 500f)));

            UseRNNoise         = Config.Bind("Neural Filters (RNNoise)", "Enable RNNoise Suppressor", true,
                new ConfigDescription("Enables RNNoise neural network noise suppression. If disabled, uses classic filters (HPF+Gate)."));
            RNNoiseVADThreshold = Config.Bind("Neural Filters (RNNoise)", "RNNoise VAD Threshold", 0.35f,
                new ConfigDescription("RNNoise voice detection probability threshold. Higher values require clearer speech.",
                    new AcceptableValueRange<float>(0.0f, 1.0f)));
            RNNoiseGateHoldMs  = Config.Bind("Neural Filters (RNNoise)", "RNNoise Hold Time (ms)", 150f,
                new ConfigDescription("Time channel stays open after speech stops with RNNoise.",
                    new AcceptableValueRange<float>(50f, 500f)));
            RNNoiseLatency     = Config.Bind("Neural Filters (RNNoise)", "RNNoise Queue Latency (Samples)", 960,
                new ConfigDescription("Initial queue latency size in samples (960 = 20ms aligned).",
                    new AcceptableValueRange<int>(1, 4096)));
                    
            NetworkJitterBufferMs = Config.Bind("Network", "Initial Jitter Buffer (ms)", 150f,
                new ConfigDescription("Buffered audio time before playback. Increase if audio stutters (e.g. 200, 300). Default: 150ms",
                    new AcceptableValueRange<float>(50f, 1000f)));
            MaxHearingDistance = Config.Bind("Network", "Max VOIP Hearing Distance (Meters)", 30f,
                new ConfigDescription("Maximum distance in meters where voice can be heard in 3D world. Default: 30m",
                    new AcceptableValueRange<float>(5f, 200f)));
            OpusBitrate = Config.Bind("Network (Opus)", "Opus Bitrate (kbps)", 24000,
                new ConfigDescription("Audio quality and compression rate. 12000 = Basic, 24000 = Standard/Discord, 64000 = Crystal Clear. Default: 24000",
                    new AcceptableValueRange<int>(8000, 64000)));
            OpusComplexity = Config.Bind("Network (Opus)", "Encoder Complexity", 5,
                new ConfigDescription("Encoder CPU usage (0 = Lowest, 10 = Best Quality). Default: 5",
                    new AcceptableValueRange<int>(0, 10)));
            OpusFEC = Config.Bind("Network (Opus)", "Forward Error Correction (FEC)", true,
                new ConfigDescription("Enables redundancy for lossy networks to reconstruct lost audio packets."));

            EnableBotInteraction = Config.Bind("AI Bot Interaction", "Enable Bot Reactivity", true,
                new ConfigDescription("If enabled, AI bots listen to player voice in 3D world and react/respond verbally."));
            BotVoiceDebugVolume = Config.Bind("AI Bot Interaction", "Bot Speech Debug Volume", 0.5f,
                new ConfigDescription("Local debug phrase playback volume (0.5 = 50%, 0.0 = Muted). Default: 0.5",
                    new AcceptableValueRange<float>(0.0f, 1.0f)));

            RNNoiseLatency.SettingChanged += (sender, args) =>
            {
                if (TRL_SpeakFromTarkov.Core.VoipController.Instance != null)
                {
                    TRL_SpeakFromTarkov.Core.VoipController.Instance.OnMicrophoneChanged(MicrophoneDevice.Value);
                }
            };

            LogBindings();

            this.gameObject.AddComponent<TRL_SpeakFromTarkov.Core.VoipController>();

            Log.LogInfo("[SFT] Forçando a desativação do Fika VOIP Client nativo via ModulePatch...");
            new GameSessionPatcher.FikaVoipSendPatch().Enable();
            new GameSessionPatcher.FikaVoipReceivePatch().Enable();

            GameSessionPatcher.Init();
            SceneManager.sceneLoaded += OnSceneLoaded;

            Log.LogInfo("[SFT] Mod carregado!");
        }

        private void LogBindings()
        {
            Log.LogInfo($"[SFT] PTT: {PushToTalkKey.Value}");
            Log.LogInfo($"[SFT] Toggle: {ToggleModeKey.Value}");
            Log.LogInfo($"[SFT] Mute: {MuteKey.Value}");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Log.LogInfo($"[SFT] Cena: {scene.name}");
            if (TRL_SpeakFromTarkov.Core.VoipController.Instance != null)
            {
                TRL_SpeakFromTarkov.Core.VoipController.Instance.enabled = true;
                
                // Abre o microfone só depois que o menu carregou (Vivox já terminou de inicializar)
                if (scene.name == "MenuUIScene")
                {
                    TRL_SpeakFromTarkov.Core.VoipController.Instance.OnMenuSceneLoaded();
                }
            }
        }

        public static string GetSelectedMicrophone()
        {
            string display = MicrophoneDevice.Value;
            if (display != null && MicRealNames.TryGetValue(display, out string realName))
                return realName;
            return display ?? string.Empty;
        }

        /// <summary>
        /// Redundância defensiva do registro de pacotes, exigida pelo guia canônico de rede FIKA
        /// (o Ensure deve estar no Update do próprio plugin). O SftNetwork já chama o mesmo método
        /// no Update dele e nasce no mesmo frame — AddComponent executa Awake sincronamente — então
        /// não há janela de frames entre os dois. O valor aqui é cobrir o caso de o SftNetwork ter
        /// sido destruído: o registro segue vivo na instância ativa do IFikaNetworkManager.
        /// </summary>
        void Update()
        {
            TRL_SpeakFromTarkov.Network.SftNetwork.EnsurePacketsRegistered();
        }

        void OnDestroy() {}

        void OnApplicationQuit()
        {
            TRL_SpeakFromTarkov.Core.VoipController.Instance?.Cleanup();
            Log.LogInfo("[SFT] Cleanup.");
            System.Threading.Thread.Sleep(500);
        }
    }
}