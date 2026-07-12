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
        public static ConfigEntry<EBandAidPressMode> MedicInteractMode;
        public static ConfigEntry<EBandAidPressMode> EmergencyDropMode;
        public static ConfigEntry<float> MedicInteractDistance;

        private void Awake()
        {
            Instance = this;
            ModLogger = base.Logger;
            ModLogger.LogInfo("TRL-ImmersiveCombatMedicine Plugin v1.0.0 carregado.");

            // Inicializações combinadas
            ItemDatabase.Initialize();

            // Configs TrueTrauma
            TraumaState.Logger = Logger;
            ConfigMasterEnabled = Config.Bind("1. Geral (Trauma)", "Ativar Mod", true, "Liga ou desliga todo o funcionamento do mod.");
            ConfigBlackoutEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de Desmaio", true, "Ativa o desmaio ao receber muito dano massivo.");
            ConfigLegsEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de Pernas", true, "Cair no chão ao perder as pernas.");
            ConfigArmsEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de Braços", true, "Perder a mira ao perder os braços.");
            ConfigStomachEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de Estomago", true, "Ficar sem ar ao tomar tiro no estômago.");
            ConfigBlackoutDuration = Config.Bind("3. Balanceamento (Trauma)", "Duracao do Desmaio", 20f, "Quanto tempo (segundos) o jogador fica desmaiado.");

            // Configs Band-Aid
            MedicInteractKey = Config.Bind("4. Keybinds (Medic)", "Medic Interact Key", new KeyboardShortcut(KeyCode.F), "Tecla para FECHAR o modo medico (a abertura e pelo painel nativo de interacao, tecla F do jogo).");
            EmergencyDropKey = Config.Bind("4. Keybinds (Medic)", "Emergency Drop Key", new KeyboardShortcut(KeyCode.F), "Tecla para drop emergencial do item durante animação de cura.");
            MedicInteractMode = Config.Bind("4. Keybinds (Medic)", "Medic Interact Mode", EBandAidPressMode.Hold, "Modo de ativação: Press (aperta e solta), Hold (segura), DoubleTap (aperta 2x).");
            EmergencyDropMode = Config.Bind("4. Keybinds (Medic)", "Emergency Drop Mode", EBandAidPressMode.Press, "Modo de ativação do drop emergencial.");
            // Regra ÚNICA de distância: o prompt e o acionamento usam este valor (o
            // controller dirige o ActionPanel nativo por scan próprio — os caps do
            // vanilla, 1,3m/2,5m, não se aplicam). Reduzir ao empacotar para o server.
            MedicInteractDistance = Config.Bind("4. Keybinds (Medic)", "Medic Interact Distance", 5f,
                new ConfigDescription("Distancia (m) do prompt E do acionamento do modo medico (mesma regra). Valor alto para testes; reduzir no pacote final.",
                    new AcceptableValueRange<float>(1f, 15f)));

            // ref: CR-02 — o reparo de encoding (CR-01-06) corrigiu a KEY
            // 'Sistema de Braços' (era mojibake); BepInEx casa por bytes, então o
            // valor salvo do usuário virou órfão. Migração one-time do valor antigo.
            MigrateOrphanedConfigKeys();

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

            // ref: CR-01-14 — PatchAll único aborta TODAS as classes ainda não
            // processadas se um TargetMethod falhar (ordem de GetTypes() é
            // não-determinística). Processar POR CLASSE isola falhas e loga qual
            // patch quebrou, sem derrubar o resto do Awake.
            _harmony = new Harmony("com.trl.immersivecombatmedicine");
            // ref: CR-02 — GetTypesFromAssembly tolera ReflectionTypeLoadException
            // (tipos não-carregáveis, ex.: Fika ausente) devolvendo os que carregaram.
            foreach (var patchType in AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly()))
            {
                try
                {
                    _harmony.CreateClassProcessor(patchType).Patch(); // no-op sem [HarmonyPatch]
                }
                catch (Exception ex)
                {
                    ModLogger.LogError($"[Patch] Falha ao aplicar {patchType.Name}: {ex.Message}");
                }
            }

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
            // O tratamento disso ficará na classe dedicada ou adaptaremos o código de BandAidPlugin aqui.
        }

        /// <summary>
        /// ref: CR-02 — copia o valor da key antiga com bytes quebrados
        /// ("Sistema de BraÃ§os") para a key corrigida, uma única vez.
        /// OrphanedEntries é internal no BepInEx → reflection.
        /// </summary>
        private void MigrateOrphanedConfigKeys()
        {
            try
            {
                var orphansProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
                if (!(orphansProp?.GetValue(Config) is System.Collections.IDictionary orphans)) return;

                string oldKey = "Sistema de BraÃ§os"; // mojibake literal da key antiga
                object orphanDef = null;
                bool oldValue = false;
                foreach (System.Collections.DictionaryEntry entry in orphans)
                {
                    var def = entry.Key;
                    string section = AccessTools.Property(def.GetType(), "Section")?.GetValue(def) as string;
                    string key = AccessTools.Property(def.GetType(), "Key")?.GetValue(def) as string;
                    if (section == "2. Mecanicas (Trauma)" && key == oldKey &&
                        bool.TryParse(entry.Value as string, out oldValue))
                    {
                        orphanDef = def;
                        break;
                    }
                }
                if (orphanDef != null)
                {
                    // ref: CR-03-01 — REMOVER o órfão antes do Save: o BepInEx persiste
                    // OrphanedEntries no .cfg e as repopula no Reload — sem o Remove, a
                    // "migração" rodaria TODO boot, re-clobberando a escolha do usuário
                    // feita via F12 com o valor antigo.
                    ConfigArmsEnabled.Value = oldValue;
                    orphans.Remove(orphanDef);
                    Config.Save();
                    ModLogger.LogWarning($"[Config] Valor órfão migrado (one-time): 'Sistema de Braços' = {oldValue}; key antiga removida do .cfg.");
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogWarning($"MigrateOrphanedConfigKeys: {ex.Message}");
            }
        }

        public static void OnRaidStartCleanup()
        {
            // ref: CR-01-09 — ResetAll cobre TODOS os campos (a lista manual antiga
            // esquecia IsFainted e LegPenaltyTimers → prone/wake fantasma na raid seguinte)
            TraumaState.ResetAll();
            AudioListener.volume = 1f; // ref: CR-01-13 — cinto-e-suspensório no início
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

            // Lógica unificada de Update aqui
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

