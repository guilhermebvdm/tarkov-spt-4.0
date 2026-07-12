using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityEngine;
using Band_Aid;
using TrueTrauma;
using System.Reflection;

namespace TRLImmersiveCombatMedicine
{
    [BepInPlugin("com.trl.immersivecombatmedicine", "TRL-ImmersiveCombatMedicine", "1.0.0")]
    public class TRLImmersiveCombatMedicinePlugin : BaseUnityPlugin
    {
        public static TRLImmersiveCombatMedicinePlugin Instance;
        public static BepInEx.Logging.ManualLogSource ModLogger;
        private Harmony _harmony;

        // --- TrueTrauma Configs ---
        public static ConfigEntry<bool> ConfigMasterEnabled;
        public static ConfigEntry<bool> ConfigLegsEnabled;
        public static ConfigEntry<bool> ConfigArmsEnabled;
        public static ConfigEntry<bool> ConfigStomachEnabled;
        public static ConfigEntry<bool> ConfigBlackoutEnabled;
        public static ConfigEntry<float> ConfigBlackoutDuration;

        // --- Band-Aid Configs ---
        public static ConfigEntry<KeyboardShortcut> MedicInteractKey;
        public static ConfigEntry<KeyboardShortcut> EmergencyDropKey;
        public static ConfigEntry<KeyboardShortcut> ShoulderTapKey;
        public static ConfigEntry<EBandAidPressMode> MedicInteractMode;
        public static ConfigEntry<EBandAidPressMode> EmergencyDropMode;
        public static ConfigEntry<EBandAidPressMode> ShoulderTapMode;

        private void Awake()
        {
            Instance = this;
            ModLogger = base.Logger;
            ModLogger.LogInfo("TRL-ImmersiveCombatMedicine Plugin v1.0.0 carregado.");

            // InicializaÃ§Ãµes combinadas
            ItemDatabase.Initialize();

            // Configs TrueTrauma
            TraumaState.Logger = Logger;
            ConfigMasterEnabled = Config.Bind("1. Geral (Trauma)", "Ativar Mod", true, "Liga ou desliga todo o funcionamento do mod.");
            ConfigBlackoutEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de Desmaio", true, "Ativa o desmaio ao receber muito dano massivo.");
            ConfigLegsEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de Pernas", true, "Cair no chÃ£o ao perder as pernas.");
            ConfigArmsEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de BraÃ§os", true, "Perder a mira ao perder os braÃ§os.");
            ConfigStomachEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de Estomago", true, "Ficar sem ar ao tomar tiro no estÃ´mago.");
            ConfigBlackoutDuration = Config.Bind("3. Balanceamento (Trauma)", "Duracao do Desmaio", 20f, "Quanto tempo (segundos) o jogador fica desmaiado.");

            // Configs Band-Aid
            MedicInteractKey = Config.Bind("4. Keybinds (Medic)", "Medic Interact Key", new KeyboardShortcut(KeyCode.F), "Tecla para interagir com paciente.");
            EmergencyDropKey = Config.Bind("4. Keybinds (Medic)", "Emergency Drop Key", new KeyboardShortcut(KeyCode.F), "Tecla para drop emergencial do item durante animaÃ§Ã£o de cura.");
            ShoulderTapKey = Config.Bind("4. Keybinds (Medic)", "Shoulder Tap Key", new KeyboardShortcut(KeyCode.F), "Tecla para toque no ombro (aviso CQB ao aliado prÃ³ximo).");
            MedicInteractMode = Config.Bind("4. Keybinds (Medic)", "Medic Interact Mode", EBandAidPressMode.Hold, "Modo de ativaÃ§Ã£o: Press (aperta e solta), Hold (segura), DoubleTap (aperta 2x).");
            EmergencyDropMode = Config.Bind("4. Keybinds (Medic)", "Emergency Drop Mode", EBandAidPressMode.Press, "Modo de ativaÃ§Ã£o do drop emergencial.");
            ShoulderTapMode = Config.Bind("4. Keybinds (Medic)", "Shoulder Tap Mode", EBandAidPressMode.DoubleTap, "Modo de ativaÃ§Ã£o do toque no ombro.");

            // Feature de debug: invisibilidade para bots (host-only)
            DebugBotInvisibility.Init(Config);

            // Carregador de imagens (Band-Aid)
            string pluginPath = System.IO.Path.GetDirectoryName(Info.Location);
            ImageLoader.Init(pluginPath);

            // Componentes no GameObject do PRÓPRIO plugin (BepInEx manager): o boot do
            // EFT destrói GameObjects órfãos criados durante o chainloader (provado por
            // [DEBUG-ICM] OnDestroy logo após "Chainloader startup complete") — o manager
            // do BepInEx sobrevive a sessão inteira. DontDestroyOnLoad NÃO protege de
            // destruição explícita.
            gameObject.AddComponent<BandAidUI>();
            gameObject.AddComponent<BandAidController>();

            // [DEBUG-ICM] sondas de lifecycle — remover após diagnóstico do prompt F
            _debugHost = gameObject;
            _debugCtrl = gameObject.GetComponent<BandAidController>();
            ModLogger.LogWarning($"[DEBUG-ICM] componentes no plugin GO | active={gameObject.activeInHierarchy} | ctrl!=null={_debugCtrl != null} | ctrl.enabled={(_debugCtrl != null ? _debugCtrl.enabled.ToString() : "n/a")}");

            // Setup reflection para TrueTrauma
            TraumaState.PlayerField = typeof(EFT.MovementContext).GetField("_player", BindingFlags.NonPublic | BindingFlags.Instance);

            // Harmony Patch (Aplica todos os patches no assembly)
            _harmony = new Harmony("com.trl.immersivecombatmedicine");
            _harmony.PatchAll();

            // Patch manual de limpeza de raid
            try
            {
                var onGameStarted = typeof(EFT.GameWorld).GetMethod(nameof(EFT.GameWorld.OnGameStarted));
                if (onGameStarted != null)
                {
                    _harmony.Patch(onGameStarted, prefix: new HarmonyMethod(typeof(TRLImmersiveCombatMedicinePlugin), nameof(OnRaidStartCleanup)));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Erro ao registrar patch OnGameStarted: {ex.Message}");
            }

            // Fika packet registration
            try
            {
                if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.fika.core"))
                {
                    Band_Aid.BandAidNetworkHandler.CheckInit();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"TrueTrauma Fika Integration Error: {ex.Message}");
            }

            // Registrar handler de handshake (Band-Aid)
            BandAidNetworkHandler.OnHealCheckResponse += OnHealCheckResponseHandler;
        }

        private void OnDestroy()
        {
            BandAidNetworkHandler.OnHealCheckResponse -= OnHealCheckResponseHandler;
        }

        private void OnHealCheckResponseHandler(BandAidHealCheckResponsePacket response)
        {
            // O tratamento disso ficarÃ¡ na classe dedicada ou adaptaremos o cÃ³digo de BandAidPlugin aqui.
        }

        public static void OnRaidStartCleanup()
        {
            TraumaState.BlackoutTimers.Clear();
            TraumaState.BlackoutStartTimes.Clear();
            TraumaState.GraceTimers.Clear();
            TraumaState.FaintedPlayerIds.Clear();
            TraumaState.ImpactTimers.Clear();
            TraumaState.AimingFatigueTimers.Clear();
            TraumaState.VoiceCooldowns.Clear();
            TraumaState.BotLegsBrokenStartTimes.Clear();
            TraumaState.EffectIntensity = 0f;
            TraumaState.Logger.LogInfo("TRL-ImmersiveCombatMedicine: Estado limpo para nova raid.");
        }

        // [DEBUG-ICM] heartbeat — remover após diagnóstico do prompt F
        private static GameObject _debugHost;
        private static BandAidController _debugCtrl;
        private float _debugNextBeat = 0f;

        private void Update()
        {
            // [DEBUG-ICM] roda ANTES de qualquer early-return: Plugin.Update comprovadamente vive em raid
            if (Time.time >= _debugNextBeat)
            {
                _debugNextBeat = Time.time + 10f;
                var gw = Comfort.Common.Singleton<EFT.GameWorld>.Instance;
                string host = _debugHost == null ? "DESTRUÍDO" : (_debugHost.activeInHierarchy ? "ativo" : "INATIVO");
                string ctrl = _debugCtrl == null ? "DESTRUÍDO" : (_debugCtrl.enabled ? "enabled" : "DISABLED");
                ModLogger.LogWarning($"[DEBUG-ICM] beat | host={host} | ctrl={ctrl} | world={(gw != null)} | mainPlayer={(gw?.MainPlayer != null)}");
            }

            // LÃ³gica unificada de Update aqui
            if (!ConfigMasterEnabled.Value || !ConfigBlackoutEnabled.Value)
            {
                TraumaState.EffectIntensity = 0f;
                if (AudioListener.volume != 1f) AudioListener.volume = 1f;
                return;
            }

            var gameWorld = Comfort.Common.Singleton<EFT.GameWorld>.Instance;
            if (gameWorld == null || gameWorld.MainPlayer == null) return;

            string localId = gameWorld.MainPlayer.ProfileId;
            float targetIntensity = 0f;

            if (TraumaState.BlackoutStartTimes.TryGetValue(localId, out float startTime))
            {
                float duration = ConfigBlackoutDuration.Value;
                float timeElapsed = Time.time - startTime;

                if (timeElapsed <= duration)
                {
                    if (!TraumaState.IsFainted)
                    {
                        TraumaState.IsFainted = true;
                        if (TraumaState.Logger != null) TraumaState.Logger.LogInfo("TRL-ICM: Jogador entrou em Coma/Desmaio (Fika Ragdoll)!");
                        var fikaPlayer = gameWorld.MainPlayer as Fika.Core.Main.Players.FikaPlayer;
                        if (fikaPlayer != null) 
                        {
                            fikaPlayer.ToggleDowned(true);
                            var bleedout = fikaPlayer.gameObject.GetComponent("Bleedout");
                            if (bleedout != null) UnityEngine.Object.Destroy(bleedout);
                        }
                    }

                    if (timeElapsed <= 1f) targetIntensity = Mathf.Lerp(0f, 1f, timeElapsed / 1f);
                    else if (timeElapsed <= (duration - 2f)) targetIntensity = 1f;
                    else targetIntensity = Mathf.Lerp(1f, 0f, (timeElapsed - (duration - 2f)) / 2f);
                }
                else
                {
                    if (TraumaState.IsFainted)
                    {
                        TraumaState.IsFainted = false;
                        if (TraumaState.Logger != null) TraumaState.Logger.LogInfo("TRL-ICM: Jogador acordou do Desmaio.");
                        
                        var fikaPlayer = gameWorld.MainPlayer as Fika.Core.Main.Players.FikaPlayer;
                        if (fikaPlayer != null) 
                        {
                            fikaPlayer.ToggleDowned(false);
                            gameWorld.MainPlayer.MovementContext.IsInPronePose = true;
                        }

                        TraumaState.BlackoutStartTimes.Remove(localId);
                        TraumaState.BlackoutTimers.Remove(localId);
                    }
                }
            }
            else
            {
                if (TraumaState.IsFainted)
                {
                    TraumaState.IsFainted = false;
                    var fikaPlayer = gameWorld.MainPlayer as Fika.Core.Main.Players.FikaPlayer;
                    if (fikaPlayer != null) 
                    {
                        fikaPlayer.ToggleDowned(false);
                        gameWorld.MainPlayer.MovementContext.IsInPronePose = true;
                    }
                }
            }

            TraumaState.EffectIntensity = Mathf.Lerp(TraumaState.EffectIntensity, targetIntensity, Time.deltaTime * 5f);
            AudioListener.volume = Mathf.Lerp(1f, 0.05f, TraumaState.EffectIntensity);
        }
    }
}

