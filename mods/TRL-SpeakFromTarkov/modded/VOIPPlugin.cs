using BepInEx;
using System;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TRL_SpeakFromTarkov
{
    [BepInPlugin("trl.speakfromtarkov", "TRL-SpeakFromTarkov", "1.3.0")]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.HardDependency)]
    public class VoIPPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        public static ConfigEntry<string> MicrophoneDevice { get; private set; }
        public static string[] MicrophoneNames { get; private set; }
        public static ConfigEntry<bool> DisableFikaVOIP { get; private set; }
        public static ConfigEntry<float> VADThreshold { get; private set; }
        public static ConfigEntry<float> EchoDelay { get; private set; }
        public static ConfigEntry<float> EchoVolume { get; private set; }
        public static ConfigEntry<float> MicGain { get; private set; }
        public static ConfigEntry<int> SampleRate { get; private set; }

        // CONFIGURAÇÃO IDEAL: KeyboardShortcut
        public static ConfigEntry<KeyboardShortcut> PushToTalkKey { get; private set; }
        public static ConfigEntry<KeyboardShortcut> ToggleModeKey { get; private set; }
        public static ConfigEntry<KeyboardShortcut> MuteKey { get; private set; }

        public static ConfigEntry<float> VADDecayTime { get; private set; }
        public static ConfigEntry<float> MaxAudioLevel { get; private set; }

        // Filtros de áudio
        public static ConfigEntry<float> HPFCutoff          { get; private set; }
        public static ConfigEntry<float> NoiseGateThreshold { get; private set; }
        public static ConfigEntry<float> NoiseGateHoldMs    { get; private set; }

        public static ConfigEntry<bool> UseRNNoise          { get; private set; }
        public static ConfigEntry<float> RNNoiseVADThreshold { get; private set; }
        public static ConfigEntry<float> RNNoiseGateHoldMs    { get; private set; }
        public static ConfigEntry<int> RNNoiseLatency       { get; private set; }

        public static int FrameSize => (int)Math.Round(SampleRate.Value * 0.02);

        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string libname);

        void Awake()
        {
            Log = base.Logger;
            Log.LogInfo($"[SFT] Iniciando TRL-SpeakFromTarkov v1.3.0...");

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

            MicrophoneNames = Microphone.devices.Length > 0 ? Microphone.devices : new string[] { "Nenhum microfone detectado" };
            var micList = new AcceptableValueList<string>(MicrophoneNames);

            MicrophoneDevice = Config.Bind("VOIP", "Microfone", MicrophoneNames[0], new ConfigDescription("Selecione o microfone.", micList));
            MicrophoneDevice.SettingChanged += (sender, args) =>
            {
                if (TRL_SpeakFromTarkov.Core.VoipController.Instance != null)
                {
                    TRL_SpeakFromTarkov.Core.VoipController.Instance.OnMicrophoneChanged(MicrophoneDevice.Value);
                }
            };

            DisableFikaVOIP = Config.Bind("VOIP", "Desativar VOIP do Fika", true);
            VADThreshold = Config.Bind("VOIP", "Limiar VAD", 0.01f);
            EchoDelay = Config.Bind("VOIP", "Delay do Eco", 0.3f);
            EchoVolume = Config.Bind("VOIP", "Volume do Eco", 1.0f);
            MicGain = Config.Bind("VOIP", "Ganho do Microfone", 1.0f);
            SampleRate = Config.Bind("VOIP", "SampleRate", 48000);

            // KeyboardShortcut: clica → aperta combinação
            PushToTalkKey = Config.Bind("VOIP", "PushToTalk", new KeyboardShortcut(KeyCode.V, KeyCode.LeftControl), "PTT (ex: Ctrl+V)");
            ToggleModeKey = Config.Bind("VOIP", "Toggle Mode", new KeyboardShortcut(KeyCode.P), "Alternar modo");
            MuteKey = Config.Bind("VOIP", "Mute", new KeyboardShortcut(KeyCode.M), "Mutar");

            VADDecayTime = Config.Bind("VOIP", "VAD Decay Time", 0.7f);
            MaxAudioLevel = Config.Bind("VOIP", "Max Audio Level", 0.015f);

            HPFCutoff          = Config.Bind("Filtros", "HPF Cutoff (Hz)", 80f,
                new ConfigDescription("Frequência de corte do filtro passa-alta. Remove ruído de baixa frequência (teclado, mesa). Padrão: 80Hz",
                    new AcceptableValueRange<float>(20f, 500f)));
            NoiseGateThreshold = Config.Bind("Filtros", "Noise Gate Threshold", 0.008f,
                new ConfigDescription("RMS mínimo para abrir o gate de ruído. Abaixo disso o áudio é silenciado. Padrão: 0.008",
                    new AcceptableValueRange<float>(0.001f, 0.1f)));
            NoiseGateHoldMs    = Config.Bind("Filtros", "Noise Gate Hold (ms)", 150f,
                new ConfigDescription("Tempo em ms que o gate fica aberto após silêncio (evita cortar no meio da fala). Padrão: 150ms",
                    new AcceptableValueRange<float>(50f, 500f)));

            UseRNNoise         = Config.Bind("Filtros (RNNoise)", "Habilitar RNNoise", true,
                new ConfigDescription("Ativa o filtro de supressão de ruídos neural RNNoise. Se desativado, usa o filtro clássico (HPF+Gate)."));
            RNNoiseVADThreshold = Config.Bind("Filtros (RNNoise)", "Limiar VAD RNNoise", 0.35f,
                new ConfigDescription("Sensibilidade da detecção de voz do RNNoise. Valores maiores exigem fala mais clara para passar o som.",
                    new AcceptableValueRange<float>(0.0f, 1.0f)));
            RNNoiseGateHoldMs  = Config.Bind("Filtros (RNNoise)", "Hold Time RNNoise (ms)", 150f,
                new ConfigDescription("Tempo que o canal permanece aberto após o término da fala com RNNoise.",
                    new AcceptableValueRange<float>(50f, 500f)));
            RNNoiseLatency     = Config.Bind("Filtros (RNNoise)", "Latencia RNNoise (amostras)", 2048,
                new ConfigDescription("Tamanho da latência inicial da fila em amostras. Aumente se a voz picotar. Padrão: 2048",
                    new AcceptableValueRange<int>(1, 4096)));

            RNNoiseLatency.SettingChanged += (sender, args) =>
            {
                if (TRL_SpeakFromTarkov.Core.VoipController.Instance != null)
                {
                    TRL_SpeakFromTarkov.Core.VoipController.Instance.OnMicrophoneChanged(MicrophoneDevice.Value);
                }
            };

            LogBindings();

            this.gameObject.AddComponent<TRL_SpeakFromTarkov.Core.VoipController>();

            if (DisableFikaVOIP.Value)
            {
                Log.LogInfo("[SFT] Desativando transmissões do Fika VOIP Client via ModulePatch...");
                new GameSessionPatcher.FikaVoipSendPatch().Enable();
                new GameSessionPatcher.FikaVoipReceivePatch().Enable();
            }

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

        public static string GetSelectedMicrophone() => MicrophoneDevice.Value;

        void OnDestroy() {}

        void OnApplicationQuit()
        {
            TRL_SpeakFromTarkov.Core.VoipController.Instance?.Cleanup();
            Log.LogInfo("[SFT] Cleanup.");
            System.Threading.Thread.Sleep(500);
        }
    }
}