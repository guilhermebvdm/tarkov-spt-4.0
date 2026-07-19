using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityEngine;
using Band_Aid;
using TrueTrauma;
using System.Reflection;
using TRLImmersiveCombatMedicine.Trauma;

namespace TRLImmersiveCombatMedicine
{
    [BepInPlugin("com.trl.immersivecombatmedicine", "TRL-ImmersiveCombatMedicine", "1.2.2")]
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

        // --- Trauma 2.0 Configs (spec 002 §3) ---
        public static ConfigEntry<bool> ConfigTrauma2Enabled;
        public static ConfigEntry<bool> ConfigIncludeAdrenaline;
        public static ConfigEntry<float> ConfigOneShotCooldown;
        public static ConfigEntry<float> ConfigPollingHz;
        public static ConfigEntry<bool> ConfigVerboseEngineLog;
        public static ConfigEntry<bool> ConfigConsumerLegsEffects;
        public static ConfigEntry<bool> ConfigConsumerFallCycle;
        public static ConfigEntry<bool> ConfigConsumerArmsEffects;
        public static ConfigEntry<bool> ConfigConsumerStomachEffects;
        public static ConfigEntry<bool> ConfigConsumerBlackout2;
        public static ConfigEntry<bool> ConfigDebugTestConsumer;

        private void Awake()
        {
            Instance = this;
            ModLogger = base.Logger;
            ModLogger.LogInfo("TRL-ImmersiveCombatMedicine Plugin v1.2.2 carregado.");

            // Inicializações combinadas
            ItemDatabase.Initialize();

            // Configs TrueTrauma
            TraumaState.Logger = Logger;
            ConfigMasterEnabled = Config.Bind("1. Geral (Trauma)", "Ativar Mod", true, "Liga ou desliga todo o funcionamento do mod.");
            ConfigBlackoutEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de Desmaio", true, "Ativa o desmaio ao receber muito dano massivo.");
            ConfigLegsEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de Pernas", true, "Cair no chão ao perder as pernas.");
            ConfigArmsEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de Braços", true, "Perder a mira ao perder os braços.");
            ConfigStomachEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de Estomago", true, "Ficar sem ar ao tomar tiro no estômago.");
            // ref: CR-04 — piso de 5s: duração baixa (~3-5s no teste) colapsava blackout+grace
            // num flap instantâneo (andar "desmaiado", timers sumindo antes do visual).
            ConfigBlackoutDuration = Config.Bind("3. Balanceamento (Trauma)", "Duracao do Desmaio", 20f,
                new ConfigDescription("Quanto tempo (segundos) o jogador fica desmaiado. ALINHAR ENTRE TODOS OS PEERS.", new AcceptableValueRange<float>(5f, 120f)));

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

            // Configs Trauma 2.0 (spec 002 §3) — keys em EN (migração dos textos antigos é o item 010).
            // Semântica dos toggles: motor publica com "Ativar Mod" E "Enable Trauma 2.0" on; consumidores
            // auto-gateiam pelos toggles da seção 6 (nascem OFF até os itens 003+ entregarem).
            var advanced = new ConfigurationManagerAttributes { IsAdvanced = true };
            ConfigTrauma2Enabled = Config.Bind("5. Trauma 2.0 (Motor)", "Enable Trauma 2.0", true,
                "Liga o motor de estados de trauma. Sem consumidores ligados não há NENHUM efeito de gameplay — só rastreamento e log. Desligar mid-raid publica a saída de todos os estados ativos.");
            ConfigIncludeAdrenaline = Config.Bind("5. Trauma 2.0 (Motor)", "Include Adrenaline As Painkiller", true,
                "Berserk/adrenalina conta como analgésico (paridade com o jogo — é o que o EFT considera em OnPainkillers).");
            ConfigOneShotCooldown = Config.Bind("5. Trauma 2.0 (Motor)", "One-Shot Cooldown Seconds", 4f,
                new ConfigDescription("Anti-thrash (decisão 19): o mesmo one-shot involuntário (agachar/cair) não re-dispara nesse intervalo, por jogador e por tipo. Ciclos internos dos consumidores são isentos.",
                    new AcceptableValueRange<float>(3f, 5f)));
            ConfigPollingHz = Config.Bind("5. Trauma 2.0 (Motor)", "Reconciliation Polling Hz", 2f,
                new ConfigDescription("Frequência do polling de reconciliação (cobre só caminhos sem evento: cirurgia FullRestore, revive do Fika, transit heal). Teto 4 Hz (D19).",
                    new AcceptableValueRange<float>(1f, 4f), advanced));
            ConfigVerboseEngineLog = Config.Bind("5. Trauma 2.0 (Motor)", "Verbose Engine Log", false,
                new ConfigDescription("Loga detalhes de avaliação/polling. Transições de estado e supressões são SEMPRE logadas, independente desta opção.",
                    null, advanced));
            ConfigConsumerLegsEffects = Config.Bind("6. Trauma 2.0 (Consumidores)", "Legs Effects (item 003)", false,
                "Placeholder — efeitos de mancar N1/N2. Sem função até o item 003.");
            ConfigConsumerFallCycle = Config.Bind("6. Trauma 2.0 (Consumidores)", "Fall Cycle (item 004)", false,
                "Placeholder — cair + ciclo de levantar. Sem função até o item 004.");
            ConfigConsumerArmsEffects = Config.Bind("6. Trauma 2.0 (Consumidores)", "Arms Effects (item 005)", false,
                "Placeholder — tremor + cancela-ADS. Sem função até o item 005.");
            ConfigConsumerStomachEffects = Config.Bind("6. Trauma 2.0 (Consumidores)", "Stomach Effects (item 006)", false,
                "Placeholder — agachar involuntário do estômago. Sem função até o item 006.");
            ConfigConsumerBlackout2 = Config.Bind("6. Trauma 2.0 (Consumidores)", "Blackout 2.0 (item 007)", false,
                "Placeholder — desmaio percentual. Sem função até o item 007 (o desmaio ATUAL segue no toggle antigo \"Sistema de Desmaio\").");
            ConfigDebugTestConsumer = Config.Bind("6. Trauma 2.0 (Consumidores)", "Debug Test Consumer", false,
                new ConfigDescription("Consumidor de teste SEM efeito de gameplay: registra-se ATIVO para as TRÊS regiões (pernas/braços/estômago), destravando o toast/i18n para validação (AC5 da spec funcional).",
                    null, advanced));

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
            gameObject.AddComponent<TraumaEngine>(); // motor Trauma 2.0 (spec 002) — inerte até OnRaidStarted()

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
            // ref: spec 002 §2 — motor Trauma 2.0: reset idempotente + subscribe + avaliação
            // inicial estabelecedora; dispara de novo na chegada de transit (novo GameWorld)
            TraumaEngine.OnRaidStarted();
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

                // ref: CR-04-11 — desligar o desmaio no F12 DURANTE um desmaio deixava
                // o jogador preso em Downed (o MainLoopPatch limpa os timers sem
                // acordar, e este early-return impedia o WakeLocalPlayer de rodar).
                if (TraumaState.IsFainted)
                {
                    var gwOff = Comfort.Common.Singleton<EFT.GameWorld>.Instance;
                    if (gwOff?.MainPlayer != null)
                        WakeLocalPlayer(gwOff, gwOff.MainPlayer.ProfileId);
                }
                return;
            }

            var gameWorld = Comfort.Common.Singleton<EFT.GameWorld>.Instance;
            if (gameWorld == null || gameWorld.MainPlayer == null) return;

            string localId = gameWorld.MainPlayer.ProfileId;
            float targetIntensity = 0f;

            // ref: CR-04 (auditoria do desmaio) — RELÓGIO ÚNICO: o wake é dirigido pelo
            // deadline ABSOLUTO gravado na entrada (BlackoutTimers), o mesmo que o
            // MainLoopPatch usa. StartTimes serve só à rampa visual. Antes, este bloco
            // recalculava com ConfigBlackoutDuration AO VIVO — mudar a config no F12
            // durante o desmaio deslocava o wake e divergia dos outros leitores.
            if (TraumaState.BlackoutTimers.TryGetValue(localId, out float wakeDeadline))
            {
                float duration = ConfigBlackoutDuration.Value;
                if (TraumaState.BlackoutStartTimes.TryGetValue(localId, out float startTime))
                    duration = Mathf.Max(0.1f, wakeDeadline - startTime);
                else
                    startTime = wakeDeadline - duration;
                float timeElapsed = Time.time - startTime;

                if (Time.time < wakeDeadline)
                {
                    if (!TraumaState.IsFainted)
                    {
                        TraumaState.IsFainted = true;
                        if (TraumaState.Logger != null) TraumaState.Logger.LogInfo($"TRL-ICM: Jogador entrou em Coma/Desmaio (Fika Ragdoll)! dur={duration:F0}s");
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
                    WakeLocalPlayer(gameWorld, localId);
                }
            }
            else if (TraumaState.IsFainted)
            {
                // Deadline sumiu por outro caminho (ex.: revive/reset) — acordar limpo
                WakeLocalPlayer(gameWorld, localId);
            }

            TraumaState.EffectIntensity = Mathf.Lerp(TraumaState.EffectIntensity, targetIntensity, Time.deltaTime * 5f);
            AudioListener.volume = Mathf.Lerp(1f, 0.05f, TraumaState.EffectIntensity);
        }

        /// <summary>
        /// ref: CR-04 — WAKE COMPLETO: ao acordar o jogador vê e controla (sem prone
        /// forçado pós-wake, requisito do usuário) e o grace anti-IA de 5s começa
        /// AGORA (não na entrada do desmaio). FaintedPlayerIds mantém o escudo até o
        /// fim do grace (o MainLoopPatch remove + envia False + RestoreAggro).
        /// </summary>
        private void WakeLocalPlayer(EFT.GameWorld gameWorld, string localId)
        {
            if (!TraumaState.IsFainted) return;
            TraumaState.IsFainted = false;
            if (TraumaState.Logger != null) TraumaState.Logger.LogInfo("TRL-ICM: Jogador acordou do Desmaio (grace de 5s iniciando AGORA).");

            var fikaPlayer = gameWorld.MainPlayer as Fika.Core.Main.Players.FikaPlayer;
            if (fikaPlayer != null)
            {
                fikaPlayer.ToggleDowned(false);
                // ref: CR-04 — NÃO re-forçar IsInPronePose: acordar = controlar
            }

            TraumaState.BlackoutStartTimes.Remove(localId);
            TraumaState.BlackoutTimers.Remove(localId);
            // Grace ancorado no WAKE (requisito): 5s de proteção com o jogador consciente
            TraumaState.GraceTimers[localId] = Time.time + 5f;
        }
    }
}

