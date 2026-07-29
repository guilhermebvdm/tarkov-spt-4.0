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
        internal static ManualLogSource Log;

        public static ConfigEntry<string> MicrophoneDevice { get; private set; }
        public static string[] MicrophoneNames { get; private set; }
        public static System.Collections.Generic.Dictionary<string, string> MicRealNames = new System.Collections.Generic.Dictionary<string, string>();
        public static ConfigEntry<bool> EnableMod { get; private set; }
        public static ConfigEntry<TRL_SpeakFromTarkov.Audio.VoipProcessor.VoipMode> TransmissionMode { get; private set; }
        public static ConfigEntry<float> VADThreshold { get; private set; }
        public static ConfigEntry<bool> EnableEcho { get; private set; }
        public static ConfigEntry<float> EchoDelay { get; private set; }
        public static ConfigEntry<float> EchoVolume { get; private set; }
        public static ConfigEntry<float> MicGain { get; private set; }
        public static ConfigEntry<int> SampleRate { get; private set; }
        public static ConfigEntry<float> NetworkJitterBufferMs { get; private set; }
        
        // Studio Quality configs
        public static ConfigEntry<int> OpusBitrate { get; private set; }
        public static ConfigEntry<int> OpusComplexity { get; private set; }
        public static ConfigEntry<bool> OpusFEC { get; private set; }
        public static ConfigEntry<bool> EnableAGC { get; private set; }
        public static ConfigEntry<bool> EnableLimiter { get; private set; }
        public static ConfigEntry<float> LPFCutoff { get; private set; }
        public static ConfigEntry<float> MaxHearingDistance { get; private set; }
        public static ConfigEntry<float> OutputVolume { get; private set; }

        // Bot Interaction
        public static ConfigEntry<bool> EnableBotInteraction { get; private set; }
        public static ConfigEntry<float> BotVoiceDebugVolume { get; private set; }

        // CONFIGURAÇÃO IDEAL: KeyboardShortcut
        public static ConfigEntry<KeyboardShortcut> PushToTalkKey { get; private set; }
        public static ConfigEntry<KeyboardShortcut> ToggleModeKey { get; private set; }
        public static ConfigEntry<KeyboardShortcut> MuteKey { get; private set; }
        public static ConfigEntry<KeyboardShortcut> DebugToggleKey { get; private set; }
        public static ConfigEntry<bool> EnableDebugLogs { get; private set; }
        public static bool IsAudioDebugActive { get; set; } = false;

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

        public static int FrameSize => (int)Math.Round(SampleRate.Value * 0.040);

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

            MicrophoneDevice = Config.Bind("VOIP", "Microfone", MicrophoneNames[0], new ConfigDescription("Selecione o microfone.", micList));
            MicrophoneDevice.SettingChanged += (sender, args) =>
            {
                if (TRL_SpeakFromTarkov.Core.VoipController.Instance != null)
                {
                    TRL_SpeakFromTarkov.Core.VoipController.Instance.OnMicrophoneChanged(GetSelectedMicrophone());
                }
            };

            EnableMod = Config.Bind("Geral", "Habilitar Mod de Voz", true, "Se desativado, desliga totalmente a captação e reprodução de voz (como se o mod não estivesse instalado).");
            EnableMod.SettingChanged += (sender, args) =>
            {
                if (TRL_SpeakFromTarkov.Core.VoipController.Instance != null)
                {
                    TRL_SpeakFromTarkov.Core.VoipController.Instance.ToggleModState(EnableMod.Value);
                }
            };
            
            TransmissionMode = Config.Bind("VOIP", "Modo de Transmissao", TRL_SpeakFromTarkov.Audio.VoipProcessor.VoipMode.VAD, "Selecione o modo de voz: VAD (Ativação por Voz), PTT (Push To Talk) ou Open (Sempre Aberto).");
            TransmissionMode.SettingChanged += (sender, args) =>
            {
                if (TRL_SpeakFromTarkov.Core.VoipController.Instance != null && TRL_SpeakFromTarkov.Core.VoipController.Instance.processor != null)
                {
                    TRL_SpeakFromTarkov.Core.VoipController.Instance.processor.CurrentMode = TransmissionMode.Value;
                }
            };
            VADThreshold = Config.Bind("VOIP", "Limiar VAD", 0.005f, "Sensibilidade para voz em VAD.");
            EnableEcho = Config.Bind("VOIP", "Habilitar Retorno de Eco", true, "Se ativado, reproduz sua própria voz no alto-falante local para teste de áudio.");
            EchoDelay = Config.Bind("VOIP", "Delay do Eco", 0.0f, "Atraso do eco em segundos (0.0 = retorno instantâneo em tempo real).");
            EchoVolume = Config.Bind("VOIP", "Volume do Eco", 1.0f, "Volume do retorno do eco (0.0 a 1.0 ou 0 a 100%).");
            MicGain = Config.Bind("VOIP", "Ganho do Microfone", 1.0f,
                new ConfigDescription("Aumenta a captação bruta ANTES dos filtros e do gate. Padrão: 1.0"));
            OutputVolume = Config.Bind("VOIP", "Volume de Saída", 1.0f,
                new ConfigDescription("Aumenta ou abaixa o volume FINAL que seus amigos vão ouvir (não afeta os filtros). Padrão: 1.0",
                    new AcceptableValueRange<float>(0.1f, 5.0f)));
            SampleRate = Config.Bind("VOIP", "SampleRate", 48000);

            // KeyboardShortcut: clica → aperta combinação
            PushToTalkKey = Config.Bind("VOIP", "PushToTalk", new KeyboardShortcut(KeyCode.V), "PTT (ex: V)");
            ToggleModeKey = Config.Bind("VOIP", "Toggle Mode", new KeyboardShortcut(KeyCode.P), "Alternar modo");
            MuteKey = Config.Bind("VOIP", "Mute", new KeyboardShortcut(KeyCode.M, KeyCode.LeftControl), "Mutar");
            DebugToggleKey = Config.Bind("Diagnostico", "Teclar Debug Audio (Profiler)", new KeyboardShortcut(KeyCode.F9), "Pressione para iniciar/parar o profiler de áudio no console.");
            EnableDebugLogs = Config.Bind("Diagnostico", "Habilitar Logs de Debug", false, "Se ativado, imprime mensagens detalhadas de enfileiramento no console. Padrão: false");

            VADDecayTime = Config.Bind("VOIP", "VAD Decay Time", 0.7f);
            MaxAudioLevel = Config.Bind("VOIP", "Max Audio Level", 0.015f);

            HPFCutoff          = Config.Bind("Filtros", "HPF Cutoff (Hz)", 80f,
                new ConfigDescription("Frequência de corte do filtro passa-alta. Remove ruído de baixa frequência (teclado, mesa). Padrão: 80Hz",
                    new AcceptableValueRange<float>(20f, 500f)));
            LPFCutoff          = Config.Bind("Filtros", "LPF Cutoff (Hz)", 8000f,
                new ConfigDescription("Frequência de corte do filtro passa-baixa. Remove estática e sons extremamente agudos. Padrão: 8000Hz",
                    new AcceptableValueRange<float>(3000f, 20000f)));
            EnableAGC          = Config.Bind("Filtros", "Habilitar AGC (Controle Automático de Ganho)", true,
                new ConfigDescription("Aumenta a voz se você sussurrar e abaixa se gritar, normalizando o volume de forma suave."));
            EnableLimiter      = Config.Bind("Filtros", "Habilitar Limiter (Protetor de Ouvido)", true,
                new ConfigDescription("Impede que a voz estoure, esmagando gritos extremamente altos para proteger a audição alheia."));
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
            RNNoiseLatency     = Config.Bind("Filtros (RNNoise)", "Latencia RNNoise (amostras)", 960,
                new ConfigDescription("Tamanho da latência inicial da fila em amostras (960 = 20ms alinhado).",
                    new AcceptableValueRange<int>(1, 4096)));
                    
            NetworkJitterBufferMs = Config.Bind("Rede", "Jitter Buffer Inicial (ms)", 150f,
                new ConfigDescription("Tempo de áudio guardado antes de tocar (Network Jitter Buffer). Se a voz picotar ou der mal contato na raid, AUMENTE este valor (ex: 200, 300). Padrão: 150ms",
                    new AcceptableValueRange<float>(50f, 1000f)));
            MaxHearingDistance = Config.Bind("Rede", "Distância Máxima do VOIP (Metros)", 30f,
                new ConfigDescription("Distância máxima (em metros) onde a voz ainda pode ser ouvida pelos outros jogadores num volume normal. Padrão: 30m",
                    new AcceptableValueRange<float>(5f, 200f)));
            OpusBitrate = Config.Bind("Rede (Opus)", "Bitrate (kbps)", 24000,
                new ConfigDescription("Qualidade do áudio (taxa de compressão). 12000 = Básico, 24000 = Padrão/Discord, 64000 = Áudio Cristalino. Padrão: 24000",
                    new AcceptableValueRange<int>(8000, 64000)));
            OpusComplexity = Config.Bind("Rede (Opus)", "Complexidade", 5,
                new ConfigDescription("Uso de CPU do codificador. 0 = Leve (pior qualidade), 10 = Pesado (melhor qualidade). Padrão: 5",
                    new AcceptableValueRange<int>(0, 10)));
            OpusFEC = Config.Bind("Rede (Opus)", "Correção de Erros (FEC)", true,
                new ConfigDescription("Se ativado, envia redundância. Em internets ruins, reconstrói partes perdidas da voz magicamente."));

            EnableBotInteraction = Config.Bind("Interacao IA (Bots)", "Habilitar Reatividade dos Bots", true,
                new ConfigDescription("Se ativado, os bots escutam a sua voz no mundo 3D e reagem/respondem verbalmente."));
            BotVoiceDebugVolume = Config.Bind("Interacao IA (Bots)", "Volume de Debug da Fala (EPhraseTrigger)", 0.5f,
                new ConfigDescription("Volume local da fala do personagem durante o teste de debug (0.5 = 50%, 0.0 = Silencioso). Padrão: 0.5 (50%)",
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
            return display;
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