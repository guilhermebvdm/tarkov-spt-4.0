using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeakFromTarkov
{
    [BepInPlugin("com.umbigopreto.speakfromtarkov", "SpeakFromTarkov", "1.0.0")]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.HardDependency)]
    public class VoIPPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        private Harmony _harmony;

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

        public static int FrameSize => (int)(SampleRate.Value * 0.02f);

        void Awake()
        {
            Log = base.Logger;
            Log.LogInfo($"[SFT] Iniciando SpeakFromTarkov v1.0.0...");

            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
                Application.RequestUserAuthorization(UserAuthorization.Microphone);

            MicrophoneNames = Microphone.devices.Length > 0 ? Microphone.devices : new string[] { "Nenhum microfone detectado" };
            var micList = new AcceptableValueList<string>(MicrophoneNames);

            MicrophoneDevice = Config.Bind("VOIP", "Microfone", MicrophoneNames[0], new ConfigDescription("Selecione o microfone.", micList));
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

            LogBindings();

            GameObject networkObject = new GameObject("SFT_NetworkManager");
            DontDestroyOnLoad(networkObject);
            networkObject.AddComponent<NetworkManager>();

            GameObject voipObject = new GameObject("SFT_VoiceChatManager");
            DontDestroyOnLoad(voipObject);
            voipObject.AddComponent<VoiceChatManager>();

            _harmony = new Harmony("com.umbigopreto.speakfromtarkov");
            GameSessionPatcher.Init(_harmony);
            SceneManager.sceneLoaded += OnSceneLoaded;

            if (DisableFikaVOIP.Value)
                PatchFikaVOIP();

            Log.LogInfo("[SFT] Mod carregado!");
        }

        private void LogBindings()
        {
            Log.LogInfo($"[SFT] PTT: {PushToTalkKey.Value}");
            Log.LogInfo($"[SFT] Toggle: {ToggleModeKey.Value}");
            Log.LogInfo($"[SFT] Mute: {MuteKey.Value}");
        }

        private void PatchFikaVOIP()
        {
            Log.LogInfo("[SFT] Desativando Fika VOIP...");
            _harmony.PatchAll(typeof(Fika.Core.Networking.VOIP.FikaVOIPClient));
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Log.LogInfo($"[SFT] Cena: {scene.name}");
            if (VoiceChatManager.Instance != null)
                VoiceChatManager.Instance.enabled = true;
        }

        public static string GetSelectedMicrophone() => MicrophoneDevice.Value;

        void OnDestroy() => _harmony?.UnpatchSelf();

        void OnApplicationQuit()
        {
            VoiceChatManager.Instance?.Cleanup();
            NetworkManager.Instance?.Cleanup();
            Log.LogInfo("[SFT] Cleanup.");
            System.Threading.Thread.Sleep(500);
        }
    }
}