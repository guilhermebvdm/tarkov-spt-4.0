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
    // ref: spec 004 §1.9 (PA-02-05) — soft-dep garante BigBrain carregado ANTES do gate do Chainloader em
    // TraumaBotFall.RegisterLayer (sem o atributo, a ordem de load do BepInEx 5 pode falso-negativar
    // PluginInfos com BigBrain instalado). GUID confirmado: bigbrain_full/BigBrainPlugin.cs:10.
    [BepInDependency("xyz.drakia.bigbrain", BepInDependency.DependencyFlags.SoftDependency)]
    // ref: item 013 / CR-01-01 — SoftDependency do Fika: FikaRevivePatch e BandAidNetworkHandler
    // resolvem tipos do Fika por nome (AccessTools.TypeByName). Sem declarar a dependência a ordem
    // de carga do BepInEx é indeterminada; se este plugin subir antes do Fika, os alvos não resolvem
    // e os patches são dispensados em silêncio. Funcionava por acidente de ordenação até aqui.
    // Padrão já usado no repo por DiscordRaidMap e MOAR-Client.
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("com.trl.immersivecombatmedicine", "TRL-ImmersiveCombatMedicine", "1.12.0")]
    public class TRLImmersiveCombatMedicinePlugin : BaseUnityPlugin
    {
        public static TRLImmersiveCombatMedicinePlugin Instance;
        public static BepInEx.Logging.ManualLogSource ModLogger;
        private Harmony _harmony;

        // --- TrueTrauma Configs ---
        public static ConfigEntry<bool> ConfigMasterEnabled;
        public static ConfigEntry<bool> ConfigBlackoutEnabled;
        // ref: item 008 — ConfigBlackoutDuration (campo único fixo) REMOVIDO; substituído pelo
        // par min/max abaixo. Migração do valor antigo por CÓPIA (não descarte) em
        // MigrateOrphanedConfigKeys().
        public static ConfigEntry<float> ConfigBlackoutDurationMin;
        public static ConfigEntry<float> ConfigBlackoutDurationMax;

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

        // --- Trauma 2.0 — Pernas (spec 003 §3) ---
        public static ConfigEntry<float> ConfigLegsN1TargetPercent;
        public static ConfigEntry<float> ConfigLegsN2TargetPercent;
        public static ConfigEntry<bool> ConfigBlockSprintOnN2;
        public static ConfigEntry<float> ConfigBotCrouchDipSeconds;

        // --- Trauma 2.0 — Queda (spec 004 §3) ---
        public static ConfigEntry<float> ConfigFallWindowSeconds;
        public static ConfigEntry<float> ConfigFallBlockSeconds;
        public static ConfigEntry<float> ConfigBotFallHoldSeconds;

        // --- Trauma 2.0 — Braços (spec 005 §3) ---
        public static ConfigEntry<float> ConfigArmsAdsCancelZ2Seconds;
        public static ConfigEntry<float> ConfigArmsAdsCancelQ2Seconds;
        public static ConfigEntry<float> ConfigArmsAdsCancelZ2Q2Seconds;
        public static ConfigEntry<float> ConfigArmsReAdsLockoutSeconds;

        // --- Trauma 2.0 — Estômago (spec 006 §3) ---
        public static ConfigEntry<float> ConfigStomachCrouchChancePercent;
        public static ConfigEntry<float> ConfigStomachCrouchChancePkPercent;

        // --- Trauma 2.0 — Desmaio (spec 007 §3) ---
        public static ConfigEntry<float> ConfigBlackoutChestPercent;
        public static ConfigEntry<float> ConfigBlackoutHeadPercent;
        public static ConfigEntry<float> ConfigBlackoutChestAbsoluteFloor;
        public static ConfigEntry<float> ConfigBlackoutHeadAbsoluteFloor;

        private void Awake()
        {
            Instance = this;
            ModLogger = base.Logger;
            ModLogger.LogInfo($"TRL-ImmersiveCombatMedicine Plugin v{Info.Metadata.Version} carregado.");

            // Inicializações combinadas
            ItemDatabase.Initialize();

            // Configs TrueTrauma
            TraumaState.Logger = Logger;
            ConfigMasterEnabled = Config.Bind("1. Geral (Trauma)", "Ativar Mod", true, "Liga ou desliga todo o funcionamento do mod.");
            // ref: item 010 — as 3 keys legadas (Sistema de Pernas/Braços/Estomago) eram vestígio puro
            // do sistema pré-Trauma-2.0; nenhum patch lê .Value delas fora da migração histórica do
            // mojibake (já removida em MigrateOrphanedConfigKeys() abaixo) — removidas por inteiro.
            ConfigBlackoutEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de Desmaio", true, "Ativa o desmaio ao receber muito dano massivo.");
            // ref: CR-04 — piso de 5s herdado: duração baixa (~3-5s no teste) colapsava blackout+grace
            // num flap instantâneo (andar "desmaiado", timers sumindo antes do visual).
            // ref: item 008 — par min/max substitui o campo fixo único; default 20/20 preserva o
            // comportamento antigo em instalação nova (identidade com o valor fixo anterior).
            ConfigBlackoutDurationMin = Config.Bind("3. Balanceamento (Trauma)", "Duracao Minima do Desmaio", 20f,
                new ConfigDescription(
                    "Duração MÍNIMA (segundos) do desmaio — sorteada uniformemente até o máximo abaixo a cada novo desmaio (independente de sorteios anteriores). ALINHAR ENTRE TODOS OS PEERS. Se este valor for maior que o Máximo, os dois são trocados antes do sorteio (config sempre produz um resultado válido).",
                    new AcceptableValueRange<float>(5f, 120f)));
            ConfigBlackoutDurationMax = Config.Bind("3. Balanceamento (Trauma)", "Duracao Maxima do Desmaio", 20f,
                new ConfigDescription(
                    "Duração MÁXIMA (segundos) do desmaio — sorteada uniformemente a partir do Mínimo acima a cada novo desmaio. ALINHAR ENTRE TODOS OS PEERS. Com Mínimo == Máximo, o comportamento é idêntico a uma duração fixa (caso degenerado do sorteio, não um caso especial).",
                    new AcceptableValueRange<float>(5f, 120f)));

            // Configs Band-Aid
            MedicInteractKey = Config.Bind("4. Keybinds (Medic)", "Medic Interact Key", new KeyboardShortcut(KeyCode.F), "Tecla para FECHAR o modo medico (a abertura e pelo painel nativo de interacao, tecla F do jogo).");
            EmergencyDropKey = Config.Bind("4. Keybinds (Medic)", "Emergency Drop Key", new KeyboardShortcut(KeyCode.F), "Tecla para drop emergencial do item durante animação de cura.");
            MedicInteractMode = Config.Bind("4. Keybinds (Medic)", "Medic Interact Mode", EBandAidPressMode.Hold, "Modo de ativação: Press (aperta e solta), Hold (segura), DoubleTap (aperta 2x).");
            EmergencyDropMode = Config.Bind("4. Keybinds (Medic)", "Emergency Drop Mode", EBandAidPressMode.Press, "Modo de ativação do drop emergencial.");
            // Regra ÚNICA de distância: o prompt e o acionamento usam este valor (o
            // controller dirige o ActionPanel nativo por scan próprio — os caps do
            // vanilla, 1,3m/2,5m, não se aplicam).
            MedicInteractDistance = Config.Bind("4. Keybinds (Medic)", "Medic Interact Distance", 3.5f,
                new ConfigDescription("Distancia (m) do prompt E do acionamento do modo medico (mesma regra).",
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
            // ref: code-review 1 do 003 — RENAME-AT-DELIVERY: "Legs Effects (item 003)" → "Legs Effects".
            // A key nova nasce ON p/ TODOS (a órfã do placeholder é DELETADA em MigrateOrphanedConfigKeys sem
            // copiar o valor — o false de placeholder não é escolha do usuário). Mesmo padrão vale p/ os
            // placeholders 004/005/006/007 nas entregas deles.
            ConfigConsumerLegsEffects = Config.Bind("6. Trauma 2.0 (Consumidores)", "Legs Effects", true,
                "Mancar N1/N2 + agachar involuntário (item 003). Governado pelo master Trauma 2.0; desligar mid-raid desfaz caps e cancela agachares pendentes.");
            // ref: spec 004 §3 — RENAME-AT-DELIVERY: "Fall Cycle (item 004)" → "Fall Cycle", nasce ON
            // (órfã do placeholder DELETADA sem copiar valor em MigrateOrphanedConfigKeys — padrão do 003).
            ConfigConsumerFallCycle = Config.Bind("6. Trauma 2.0 (Consumidores)", "Fall Cycle", true,
                "Cair + ciclo de levantar (item 004). Governado pelo master Trauma 2.0; desligar mid-raid destrava o levantar na hora, cancela quedas pendentes e libera bots (o mancar interim do 003 NÃO volta). OFF com Legs Effects ON: o aviso (toast) da 1ª ocorrência da linha Cair ainda aparece — registry de consumidores é por região (PA-01-14).");
            // ref: spec 005 §3 — RENAME-AT-DELIVERY: "Arms Effects (item 005)" → "Arms Effects", nasce ON
            // (órfã do placeholder DELETADA sem copiar valor em MigrateOrphanedConfigKeys — padrão do 003/004).
            ConfigConsumerArmsEffects = Config.Bind("6. Trauma 2.0 (Consumidores)", "Arms Effects", true,
                "Tremor contínuo + cancelamento de ADS escalonado (item 005). Governado pelo master Trauma 2.0; desligar mid-raid remove o tremor e cancela o lockout.");
            // ref: spec 006 §3 — RENAME-AT-DELIVERY do placeholder de estômago: deletar a key órfã
            // "Stomach Effects (item 006)" SEM copiar o valor (a key nova "Stomach Effects" nasce ON — o false de
            // placeholder não é escolha do usuário). Mesmo padrão do 003/004/005 (lição CR-03-01: sem o
            // delete + Save, o BepInEx re-persiste a key morta a cada boot).
            ConfigConsumerStomachEffects = Config.Bind("6. Trauma 2.0 (Consumidores)", "Stomach Effects", true,
                "Agachar involuntário probabilístico ao zerar o estômago (item 006). Governado pelo master Trauma 2.0; " +
                "desligar mid-raid cancela agachares pendentes DO ESTÔMAGO (não toca os de pernas); o \"sem ar\" legado NÃO volta.");
            // ref: spec 007 §3 — RENAME-AT-DELIVERY do placeholder de desmaio: deletar a key órfã
            // "Blackout 2.0 (item 007)" SEM copiar o valor (a key nova "Blackout 2.0" nasce ON — o false de
            // placeholder não é escolha do usuário). Mesmo padrão do 003/004/005/006 (lição CR-03-01: sem o
            // delete + Save, o BepInEx re-persiste a key morta a cada boot). ConfigBlackoutEnabled ("Sistema
            // de Desmaio") CONTINUA sendo o master de todo o pipeline; este toggle é só o sub-toggle da lógica
            // de entrada percentual (decisão de design da spec funcional 007).
            ConfigConsumerBlackout2 = Config.Bind("6. Trauma 2.0 (Consumidores)", "Blackout 2.0", true,
                "Gatilho percentual de desmaio (item 007): tórax ≥50% da vida atual (piso 25 de dano absoluto) rola p=50%, imune sob analgésico; cabeça ≥25% da vida atual (piso 10) rola p=50%, p=25% sob analgésico. Governado pelo master \"Sistema de Desmaio\" — este toggle decide SÓ a lógica de entrada (percentual ou nenhuma); o limiar fixo legado NÃO volta mesmo desligado.");
            ConfigDebugTestConsumer = Config.Bind("6. Trauma 2.0 (Consumidores)", "Debug Test Consumer", false,
                new ConfigDescription("Consumidor de teste SEM efeito de gameplay: registra-se ATIVO para as TRÊS regiões (pernas/braços/estômago), destravando o toast/i18n para validação (AC5 da spec funcional).",
                    null, advanced));

            // Configs Trauma 2.0 — Pernas (spec 003 §3)
            ConfigLegsN1TargetPercent = Config.Bind("7. Trauma 2.0 (Pernas)", "N1 Target Total Speed Percent", 80f,
                new ConfigDescription("Velocidade TOTAL experienciada no Mancar N1, em % do baseline (composto com classe/skill). Se a penalidade vanilla for mais dura que o alvo, vale o vanilla (clamp logado — nunca acelera o jogador).",
                    new AcceptableValueRange<float>(50f, 95f)));
            ConfigLegsN2TargetPercent = Config.Bind("7. Trauma 2.0 (Pernas)", "N2 Target Total Speed Percent", 55f,
                new ConfigDescription("Velocidade TOTAL experienciada no Mancar N2, em % do baseline. Mesma regra de clamp do N1. Se configurado ACIMA do N1, vale o efetivo min(N2, N1) — N2 nunca é mais leve que N1 (warn no log, 1x).",
                    new AcceptableValueRange<float>(30f, 90f)));
            ConfigBlockSprintOnN2 = Config.Bind("7. Trauma 2.0 (Pernas)", "Block Sprint On N2", true,
                "Em Mancar N2 o sprint fica bloqueado, inclusive sob analgésico (o vanilla libera sprint com analgésico; este toggle mantém o bloqueio do mod). N1 segue a regra vanilla.");
            ConfigBotCrouchDipSeconds = Config.Bind("7. Trauma 2.0 (Pernas)", "Bot Crouch Dip Seconds", 0.7f,
                new ConfigDescription("Duração do dip de agachar de bot FORA de combate antes de devolver a pose (em combate o SAIN restaura sozinho).",
                    new AcceptableValueRange<float>(0.3f, 1.5f), advanced));

            // Configs Trauma 2.0 — Queda (spec 004 §3). Timers lidos por .Value no INÍCIO de cada fase
            // (deadline absoluto — contagem em andamento nunca re-baseada; AC1).
            ConfigFallWindowSeconds = Config.Bind("8. Trauma 2.0 (Queda)", "Fall Window Seconds", 3f,
                new ConfigDescription("JANELA: tempo DE PÉ antes de cair de novo com as duas pernas quebradas (linha Cair). Conta do fim do levantar; mudanças valem a partir da PRÓXIMA janela iniciada. Piso 1s intencional (0 degeneraria em prone permanente).",
                    new AcceptableValueRange<float>(1f, 10f)));
            ConfigFallBlockSeconds = Config.Bind("8. Trauma 2.0 (Queda)", "Fall Block Seconds", 15f,
                new ConfigDescription("BLOQUEIO: tempo no chão sem poder levantar após cada queda (tentar dá som de dor e nada acontece; rastejar é livre). Mudanças valem a partir do PRÓXIMO bloqueio iniciado. Piso 5s intencional (0 anularia o ciclo, conflitando com o anti-thrash do motor).",
                    new AcceptableValueRange<float>(5f, 60f)));
            ConfigBotFallHoldSeconds = Config.Bind("8. Trauma 2.0 (Queda)", "Bot Fall Hold Seconds", 15f,
                new ConfigDescription("Tempo MÍNIMO que um bot com linha Cair fica no chão SEM combater antes de a IA poder levantar (ao levantar, é re-derrubado enquanto a condição durar). Separado dos timers humanos.",
                    new AcceptableValueRange<float>(5f, 120f)));

            // Configs Trauma 2.0 — Braços (spec 005 §3). Seção 9 — PA-02-02: a 8 é a Queda do 004.
            // Timers lidos por .Value a cada uso (sem cache); efetivo do Z2+Q2 = min dos três (warn 1x).
            ConfigArmsAdsCancelZ2Seconds = Config.Bind("9. Trauma 2.0 (Braços)", "ADS Cancel Seconds (Zeroed x2)", 4f,
                new ConfigDescription("Segundos de mira sustentada com 2 braços ZERADOS até o cancelamento do ADS. Soltar a mira reseta o timer.",
                    new AcceptableValueRange<float>(1f, 10f)));
            ConfigArmsAdsCancelQ2Seconds = Config.Bind("9. Trauma 2.0 (Braços)", "ADS Cancel Seconds (Fractured x2)", 3f,
                new ConfigDescription("Segundos com 2 braços FRATURADOS até o cancelamento (fratura pior que zerado por design — decisão 3).",
                    new AcceptableValueRange<float>(1f, 10f)));
            ConfigArmsAdsCancelZ2Q2Seconds = Config.Bind("9. Trauma 2.0 (Braços)", "ADS Cancel Seconds (Zeroed + Fractured x2)", 2f,
                new ConfigDescription("Segundos com 2 braços zerados E 2 fraturados. Efetivo = min dos três timers — a linha mais severa nunca fica mais lenta que as outras (warn no log, 1x).",
                    new AcceptableValueRange<float>(1f, 10f)));
            ConfigArmsReAdsLockoutSeconds = Config.Bind("9. Trauma 2.0 (Braços)", "Re-ADS Lockout Seconds", 1.5f,
                new ConfigDescription("Bloqueio de re-mirar após o cancelamento (persiste à troca de arma). Tentativa durante o bloqueio dispara voz de dor (1 por janela). Faixa fixada pela decisão 17 (1–1,5 s).",
                    new AcceptableValueRange<float>(1f, 1.5f)));

            // Configs Trauma 2.0 — Estômago (spec 006 §3). Sliders INDEPENDENTES entre si — sem clamp (diferente
            // do min(N2,N1) do 003, que protege invariante de severidade; aqui não há invariante — inverter é
            // permitido, premissa p/ o item 011). Lidos por .Value a cada roll (sem cache).
            ConfigStomachCrouchChancePercent = Config.Bind("10. Trauma 2.0 (Estômago)", "Stomach Crouch Chance Percent", 75f,
                new ConfigDescription("Chance (%) de agachar involuntário ao ZERAR o estômago SEM analgésico ativo. Rolada 1× por zerada " +
                    "(curar e zerar de novo rola de novo; estômago que permanece zerado não re-rola). 0 = nunca agacha (rolls seguem logados); 100 = sempre.",
                    new AcceptableValueRange<float>(0f, 100f)));
            ConfigStomachCrouchChancePkPercent = Config.Bind("10. Trauma 2.0 (Estômago)", "Stomach Crouch Chance Under Painkiller Percent", 25f,
                new ConfigDescription("Chance (%) com analgésico ativo NO INSTANTE da zerada (valor congelado nessa hora — tomar/expirar analgésico " +
                    "depois não muda nada até a próxima zerada). Independente do slider sem analgésico — sem trava entre eles; inverter é permitido.",
                    new AcceptableValueRange<float>(0f, 100f)));

            // Configs Trauma 2.0 — Desmaio (spec 007 §3). As probabilidades de roll (50%/50%/25%) são
            // constantes fixas no código (TraumaBlackoutTrigger) — só os 4 números abaixo são configuráveis.
            ConfigBlackoutChestPercent = Config.Bind("11. Trauma 2.0 (Desmaio)", "Chest Faint Percent Threshold", 50f,
                new ConfigDescription("% da vida ATUAL do tórax (pré-tiro) que um hit precisa remover para rolar desmaio (p=50%; imune sob analgésico — decisão 9). Precisa TAMBÉM atingir o piso absoluto abaixo (decisão 15).",
                    new AcceptableValueRange<float>(0f, 100f)));
            ConfigBlackoutHeadPercent = Config.Bind("11. Trauma 2.0 (Desmaio)", "Head Faint Percent Threshold", 25f,
                new ConfigDescription("% da vida ATUAL da cabeça (pré-tiro) que um hit precisa remover para rolar desmaio (p=50% sem analgésico, p=25% sob analgésico — cabeça NÃO fica imune). Precisa TAMBÉM atingir o piso absoluto abaixo.",
                    new AcceptableValueRange<float>(0f, 100f)));
            ConfigBlackoutChestAbsoluteFloor = Config.Bind("11. Trauma 2.0 (Desmaio)", "Chest Faint Absolute Damage Floor", 25f,
                new ConfigDescription("Piso de segurança (decisão 15): dano ABSOLUTO mínimo no hit do tórax, além do percentual acima — evita desmaio por hit percentualmente grande mas fisicamente insignificante (ex.: 5 de dano em tórax com 8 de vida = 62% mas só 5 de dano).",
                    new AcceptableValueRange<float>(0f, 100f)));
            ConfigBlackoutHeadAbsoluteFloor = Config.Bind("11. Trauma 2.0 (Desmaio)", "Head Faint Absolute Damage Floor", 10f,
                new ConfigDescription("Piso de segurança (decisão 15): dano ABSOLUTO mínimo no hit da cabeça, além do percentual acima.",
                    new AcceptableValueRange<float>(0f, 100f)));

            // ref: CR-02 — o reparo de encoding (CR-01-06) corrigiu a KEY
            // 'Sistema de Braços' (era mojibake); BepInEx casa por bytes, então o
            // valor salvo do usuário virou órfão. Migração one-time do valor antigo.
            MigrateOrphanedConfigKeys();

            // Feature de debug: invisibilidade para bots (host-only)
            DebugBotInvisibility.Init(Config);

            // Carregador de imagens (Band-Aid)
            string pluginPath = System.IO.Path.GetDirectoryName(Info.Location);
            ImageLoader.Init(pluginPath);

            // Componentes no GameObject do PRÓPRIO plugin (BepInEx manager): o boot do EFT destrói
            // GameObjects órfãos criados durante o chainloader (achado da Sessão 2, diagnóstico do
            // prompt F) — o manager do BepInEx sobrevive à sessão inteira. DontDestroyOnLoad NÃO
            // protege de destruição explícita.
            gameObject.AddComponent<BandAidUI>();
            gameObject.AddComponent<BandAidController>();
            gameObject.AddComponent<TraumaEngine>(); // motor Trauma 2.0 (spec 002) — inerte até OnRaidStarted()
            gameObject.AddComponent<TraumaLegsConsumer>(); // consumidor de pernas (spec 003) — efeitos gateados pelo toggle
            gameObject.AddComponent<TraumaFallCycleConsumer>(); // ciclo de queda (spec 004) — DEPOIS do TraumaEngine (ordem do 003)
            gameObject.AddComponent<TraumaArmsConsumer>(); // consumidor de braços (spec 005) — DEPOIS do TraumaEngine (replay vazio inofensivo — padrão 003)
            gameObject.AddComponent<TraumaStomachConsumer>(); // consumidor de estômago (spec 006) — DEPOIS do TraumaEngine (ordem do 003; replay vazio inofensivo)
            TraumaBotFall.RegisterLayer(); // camada BigBrain do hold de bot — gateada por Chainloader ("xyz.drakia.bigbrain")

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

            // ref: item 010 (PA-01-04) — o handler morto/duplicado OnHealCheckResponseHandler(...) e
            // seu subscribe (BandAidNetworkHandler.OnHealCheckResponse += ...) foram removidos por
            // inteiro: corpo sempre vazio, nunca preenchido; o tratamento real já existe em
            // BandAidController.OnHealCheckResponseHandler (subscrito separadamente no Awake() do
            // controller). O OnDestroy() deste plugin só existia para desinscrever esse handler morto
            // — removido junto (sem substituto: este plugin não segura mais nenhuma inscrição própria).
        }

        /// <summary>
        /// Migrações one-time de keys órfãs (renomeações/remoções ao longo dos itens 003-008).
        /// OrphanedEntries é internal no BepInEx → reflection. A migração do mojibake "Sistema de
        /// Braços" (CR-02, 2026-07-12) foi removida no item 010 junto com o campo ConfigArmsEnabled —
        /// já tinha cumprido seu papel one-time em toda instalação real.
        /// </summary>
        private void MigrateOrphanedConfigKeys()
        {
            try
            {
                var orphansProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
                if (!(orphansProp?.GetValue(Config) is System.Collections.IDictionary orphans)) return;

                // ref: item 008 — MIGRAÇÃO POR CÓPIA (não descarte, diferente do padrão rename-at-delivery
                // dos placeholders 003-007): "Duracao do Desmaio" era um valor ATIVO e real (ajustado ao
                // vivo por lição de UX documentada — P-2.13/P-2.15), não um placeholder inerte. Copiar o
                // valor antigo para OS DOIS campos novos reproduz o comportamento fixo anterior
                // exatamente (min==max==valorAntigo) — usuário não percebe mudança até abrir o F12.
                // PA-01-01 (review técnica 01): parse com CultureInfo.InvariantCulture — BepInEx
                // (TomlTypeConverter) sempre grava/lê floats em cultura invariante; sem isso, num
                // processo com cultura pt-BR/de-DE (ponto decimal → separador de milhar), um valor
                // legado como "47.5" seria lido como 475 → clampado a 120 pelo AcceptableValueRange,
                // migrando Min=Max=120 (o OPOSTO do objetivo). Confirmado por decompile do
                // BepInEx.dll real que nenhuma migração existente no mod hoje faz parse de float
                // (só bool, insensível a isso) — risco genuinamente novo deste item.
                object legacyDurationDef = null;
                float legacyDurationValue = 20f;
                foreach (System.Collections.DictionaryEntry entry in orphans)
                {
                    var def = entry.Key;
                    string section = AccessTools.Property(def.GetType(), "Section")?.GetValue(def) as string;
                    string key = AccessTools.Property(def.GetType(), "Key")?.GetValue(def) as string;
                    if (section == "3. Balanceamento (Trauma)" && key == "Duracao do Desmaio" &&
                        float.TryParse(entry.Value as string, System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out legacyDurationValue))
                    {
                        legacyDurationDef = def;
                        break;
                    }
                }
                if (legacyDurationDef != null)
                {
                    ConfigBlackoutDurationMin.Value = legacyDurationValue;
                    ConfigBlackoutDurationMax.Value = legacyDurationValue;
                    orphans.Remove(legacyDurationDef);
                    Config.Save();
                    ModLogger.LogWarning($"[Config] 'Duracao do Desmaio' (valor real do usuário) migrado por CÓPIA para Min E Max = {legacyDurationValue:F0}s; key antiga removida do .cfg.");
                }

                // ref: code-review 1 do 003 — RENAME-AT-DELIVERY: deletar a key órfã do placeholder
                // "Legs Effects (item 003)" SEM copiar o valor (o false de placeholder não é escolha do
                // usuário — a key nova "Legs Effects" nasce ON). Lição CR-03-01: sem o delete + Save, o
                // BepInEx re-persiste a key morta a cada boot. Repetir o padrão nos placeholders
                // 004/005/006/007 quando cada item entregar.
                object legacyLegsDef = null;
                foreach (System.Collections.DictionaryEntry entry in orphans)
                {
                    var def = entry.Key;
                    string section = AccessTools.Property(def.GetType(), "Section")?.GetValue(def) as string;
                    string key = AccessTools.Property(def.GetType(), "Key")?.GetValue(def) as string;
                    if (section == "6. Trauma 2.0 (Consumidores)" && key == "Legs Effects (item 003)")
                    {
                        legacyLegsDef = def;
                        break;
                    }
                }
                if (legacyLegsDef != null)
                {
                    orphans.Remove(legacyLegsDef);
                    Config.Save();
                    ModLogger.LogWarning("[Config] Key placeholder órfã DELETADA (rename-at-delivery, sem copiar valor): 'Legs Effects (item 003)' → 'Legs Effects'.");
                }

                // ref: spec 004 §3 — RENAME-AT-DELIVERY do placeholder do ciclo de queda: deletar a key órfã
                // "Fall Cycle (item 004)" SEM copiar o valor (a key nova "Fall Cycle" nasce ON). Mesmo padrão
                // do 003 (lição CR-03-01: sem o delete + Save, o BepInEx re-persiste a key morta a cada boot).
                object legacyFallDef = null;
                foreach (System.Collections.DictionaryEntry entry in orphans)
                {
                    var def = entry.Key;
                    string section = AccessTools.Property(def.GetType(), "Section")?.GetValue(def) as string;
                    string key = AccessTools.Property(def.GetType(), "Key")?.GetValue(def) as string;
                    if (section == "6. Trauma 2.0 (Consumidores)" && key == "Fall Cycle (item 004)")
                    {
                        legacyFallDef = def;
                        break;
                    }
                }
                if (legacyFallDef != null)
                {
                    orphans.Remove(legacyFallDef);
                    Config.Save();
                    ModLogger.LogWarning("[Config] Key placeholder órfã DELETADA (rename-at-delivery, sem copiar valor): 'Fall Cycle (item 004)' → 'Fall Cycle'.");
                }

                // ref: spec 005 §3 — RENAME-AT-DELIVERY do placeholder de braços: deletar a key órfã
                // "Arms Effects (item 005)" SEM copiar o valor (a key nova "Arms Effects" nasce ON — o false de
                // placeholder não é escolha do usuário). Mesmo padrão do 003/004 (lição CR-03-01: sem o
                // delete + Save, o BepInEx re-persiste a key morta a cada boot).
                object legacyArmsDef = null;
                foreach (System.Collections.DictionaryEntry entry in orphans)
                {
                    var def = entry.Key;
                    string section = AccessTools.Property(def.GetType(), "Section")?.GetValue(def) as string;
                    string key = AccessTools.Property(def.GetType(), "Key")?.GetValue(def) as string;
                    if (section == "6. Trauma 2.0 (Consumidores)" && key == "Arms Effects (item 005)")
                    {
                        legacyArmsDef = def;
                        break;
                    }
                }
                if (legacyArmsDef != null)
                {
                    orphans.Remove(legacyArmsDef);
                    Config.Save();
                    ModLogger.LogWarning("[Config] Key placeholder órfã DELETADA (rename-at-delivery, sem copiar valor): 'Arms Effects (item 005)' → 'Arms Effects'.");
                }

                // ref: spec 006 §3 — RENAME-AT-DELIVERY do placeholder de estômago: deletar a key órfã
                // "Stomach Effects (item 006)" SEM copiar o valor (a key nova "Stomach Effects" nasce ON — o
                // false de placeholder não é escolha do usuário). Mesmo padrão do 003/004/005 (lição CR-03-01:
                // sem o delete + Save, o BepInEx re-persiste a key morta a cada boot).
                object legacyStomachDef = null;
                foreach (System.Collections.DictionaryEntry entry in orphans)
                {
                    var def = entry.Key;
                    string section = AccessTools.Property(def.GetType(), "Section")?.GetValue(def) as string;
                    string key = AccessTools.Property(def.GetType(), "Key")?.GetValue(def) as string;
                    if (section == "6. Trauma 2.0 (Consumidores)" && key == "Stomach Effects (item 006)")
                    {
                        legacyStomachDef = def;
                        break;
                    }
                }
                if (legacyStomachDef != null)
                {
                    orphans.Remove(legacyStomachDef);
                    Config.Save();
                    ModLogger.LogWarning("[Config] Key placeholder órfã DELETADA (rename-at-delivery, sem copiar valor): 'Stomach Effects (item 006)' → 'Stomach Effects'.");
                }

                // ref: spec 007 §3 — RENAME-AT-DELIVERY do placeholder de desmaio: deletar a key órfã
                // "Blackout 2.0 (item 007)" SEM copiar o valor (a key nova "Blackout 2.0" nasce ON — o
                // false de placeholder não é escolha do usuário). Mesmo padrão do 003/004/005/006 (lição
                // CR-03-01: sem o delete + Save, o BepInEx re-persiste a key morta a cada boot).
                object legacyBlackoutDef = null;
                foreach (System.Collections.DictionaryEntry entry in orphans)
                {
                    var def = entry.Key;
                    string section = AccessTools.Property(def.GetType(), "Section")?.GetValue(def) as string;
                    string key = AccessTools.Property(def.GetType(), "Key")?.GetValue(def) as string;
                    if (section == "6. Trauma 2.0 (Consumidores)" && key == "Blackout 2.0 (item 007)")
                    {
                        legacyBlackoutDef = def;
                        break;
                    }
                }
                if (legacyBlackoutDef != null)
                {
                    orphans.Remove(legacyBlackoutDef);
                    Config.Save();
                    ModLogger.LogWarning("[Config] Key placeholder órfã DELETADA (rename-at-delivery, sem copiar valor): 'Blackout 2.0 (item 007)' → 'Blackout 2.0'.");
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogWarning($"MigrateOrphanedConfigKeys: {ex.Message}");
            }
        }

        public static void OnRaidStartCleanup()
        {
            // ref: item 020 — mede ANTES de limpar: qualquer coisa viva aqui é resíduo da raid
            // anterior, e é o único ponto onde a contagem é confiável (a limpeza de fim de raid é
            // uma cascata assíncrona por consumidor, medir no meio dela daria falso positivo).
            // Sem esta linha, o teste do S1b só podia SUPOR que nada vazou.
            TraumaPurge.Audit(TraumaPurge.PhaseBefore);

            // ref: CR-01-09 — ResetAll cobre TODOS os campos (a lista manual antiga
            // esquecia IsFainted e LegPenaltyTimers → prone/wake fantasma na raid seguinte)
            TraumaState.ResetAll();
            AudioListener.volume = 1f; // ref: CR-01-13 — cinto-e-suspensório no início
            // ref: spec 002 §2 — motor Trauma 2.0: reset idempotente + subscribe + avaliação
            // inicial estabelecedora; dispara de novo na chegada de transit (novo GameWorld)
            TraumaEngine.OnRaidStarted();
            TraumaState.Logger.LogInfo("TRL-ImmersiveCombatMedicine: Estado limpo para nova raid.");

            // ref: item 020 — confirma que a limpeza desta entrada zerou o estado TRANSITÓRIO
            // (desmaio, cooldowns, voz, áudio). Resíduo aqui é falha do mecanismo, não da raid
            // anterior: loga como erro. O sweep estabelecedor do OnRaidStarted acima repovoa
            // records/caps/tremor de propósito quando o spawn é ferido — estado legítimo, excluído
            // desta medição (ver a nota de design 2 em TraumaPurge).
            TraumaPurge.Audit(TraumaPurge.PhaseAfter);
        }

        private static bool? _isFikaInstalled;
        private static bool IsFikaInstalled => _isFikaInstalled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.fika.core");

        private void Update()
        {
            if (IsFikaInstalled)
            {
                Band_Aid.BandAidNetworkHandler.EnsurePacketsRegistered();
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

            // ref: CR-04 (auditoria do desmaio) — RELÓGIO ÚNICO: o wake é dirigido pelo deadline
            // ABSOLUTO gravado na entrada (BlackoutTimers); StartTimes serve só à rampa visual.
            // ref: item 008 — fallback (ramo só alcançado se BlackoutStartTimes não tiver entrada
            // para localId, o que não ocorre em operação normal — HealthPatches.cs e
            // BandAidNetworkHandler.cs sempre gravam os dois dicts juntos) usa o MÍNIMO configurado
            // em vez do extinto ConfigBlackoutDuration — mesmo raciocínio conservador do
            // FikaBridge: errar para uma duração mais curta é mais seguro.
            if (TraumaState.BlackoutTimers.TryGetValue(localId, out float wakeDeadline))
            {
                float duration = ConfigBlackoutDurationMin.Value;
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

