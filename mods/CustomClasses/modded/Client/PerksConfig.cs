using BepInEx.Configuration;

namespace CustomClasses.Client;

/// <summary>
///     Item 050 — ConfigEntry (F12) dos perks/drawbacks por classe. Reorganizado 2026-07-10: uma seção POR CLASSE
///     (perks + drawbacks juntos), prefixo numérico controla a ordem no ConfigurationManager (que ordena alfabético),
///     e as descrições são bilíngues (PT / EN — o F12 não segue o idioma do jogo, então mostra os dois).
///     Os patches leem <c>.Value</c> NO APPLY-TIME (sem cache) → mudança no F12 vale durante a raid (DoD).
///     Nomes de seção centralizados nas constantes <c>Sec*</c> (usadas também pelo <see cref="Plugin"/>).
/// </summary>
internal static class PerksConfig
{
    // Nomes de seção (fonte única — o Plugin.cs referencia SecGeneral/SecInterface). Prefixo numérico = ordem no F12.
    internal const string SecGeneral = "0 · General";
    internal const string SecInterface = "1 · Interface & Position";
    internal const string SecMedic = "2 · Combat Medic";
    internal const string SecRifleman = "3 · Rifleman";
    internal const string SecHunter = "4 · Hunter";
    internal const string SecStealth = "5 · Stealth";
    internal const string SecScavenger = "6 · Scavenger";
    internal const string SecTank = "7 · Tank";
    internal const string SecVanillaFixes = "8 · Vanilla Skill Fixes";

    // 0 · General — notificação + diagnóstico
    internal static ConfigEntry<bool>? ShowRaidPerksNotification;
    internal static ConfigEntry<bool>? DiagnosticsEnabled;

    // 1 · Interface & Position
    internal static ConfigEntry<float>? ClassTabOffsetX;
    internal static ConfigEntry<bool>? ClassDetailOnLoading;
    internal static ConfigEntry<float>? LoadingPanelScale;
    internal static ConfigEntry<float>? WeightMarkerOffsetX;
    internal static ConfigEntry<float>? WeightMarkerOffsetY;

    // 2 · Combat Medic
    internal static ConfigEntry<bool>? EfficientMetabolismEnabled;
    internal static ConfigEntry<float>? EfficientMetabolismHungerThirst;
    internal static ConfigEntry<bool>? ShakyHandsEnabled;
    internal static ConfigEntry<float>? ShakyHandsRecoil;

    // 3 · Rifleman
    internal static ConfigEntry<bool>? CoolUnderFireEnabled;
    internal static ConfigEntry<float>? CoolUnderFireFlinch;
    internal static ConfigEntry<float>? CoolUnderFireMalfChance;
    internal static ConfigEntry<bool>? AdrenalineEnabled;
    internal static ConfigEntry<float>? AdrenalineDuration;
    internal static ConfigEntry<float>? AdrenalineCooldown;
    internal static ConfigEntry<float>? AdrenalineRecoil;
    internal static ConfigEntry<float>? AdrenalineReloadTime;
    internal static ConfigEntry<float>? AdrenalineAdsTime;
    internal static ConfigEntry<bool>? LoudOperatorRiflemanEnabled;     // desdobrado do compartilhado (2026-07-10)
    internal static ConfigEntry<float>? LoudOperatorRiflemanSoundRadius;

    // 4 · Hunter
    internal static ConfigEntry<bool>? StalkerEnabled;        // ruído de movimento (irmão do Ghost Step do Furtivo)
    internal static ConfigEntry<float>? StalkerSoundRadius;
    internal static ConfigEntry<bool>? SharpshooterEnabled;
    internal static ConfigEntry<float>? SharpshooterAdsTime;
    internal static ConfigEntry<bool>? IronLungsEnabled;
    internal static ConfigEntry<float>? IronLungsBreathDrain;
    internal static ConfigEntry<bool>? SteadyArmsEnabled;
    internal static ConfigEntry<float>? SteadyArmsDrain;
    internal static ConfigEntry<bool>? RootedEnabled;
    internal static ConfigEntry<float>? RootedAdsSpeed;

    // 5 · Stealth
    internal static ConfigEntry<bool>? ExecutionSpeedEnabled;
    internal static ConfigEntry<float>? ExecutionMoveSpeed;
    internal static ConfigEntry<bool>? ExecutionMeleeEnabled;
    internal static ConfigEntry<float>? ExecutionMeleeDamage;
    internal static ConfigEntry<bool>? GhostStepEnabled;
    internal static ConfigEntry<float>? GhostStepSoundRadius;
    internal static ConfigEntry<bool>? RattledEnabled;
    internal static ConfigEntry<float>? RattledAimPunch;

    // 6 · Scavenger
    internal static ConfigEntry<bool>? QuickHandsEnabled;   // 061: busca 2 contêineres (bônus elite da Search, antecipado)
    internal static ConfigEntry<bool>? SilentLooterEnabled;
    internal static ConfigEntry<float>? SilentLooterVolume;
    internal static ConfigEntry<bool>? PackMuleScavEnabled;       // desdobrado do compartilhado (2026-07-10)
    internal static ConfigEntry<float>? PackMuleScavCarryBonus;
    internal static ConfigEntry<bool>? OverladenEnabled;
    internal static ConfigEntry<float>? OverladenInertia;

    // 7 · Tank
    internal static ConfigEntry<bool>? BulwarkEnabled;
    internal static ConfigEntry<float>? BulwarkDamageTaken;
    internal static ConfigEntry<bool>? BunkerEnabled;
    internal static ConfigEntry<float>? BunkerHeavyRecoil;
    internal static ConfigEntry<float>? BunkerHeavyErgo;
    internal static ConfigEntry<bool>? TirelessArmsEnabled;
    internal static ConfigEntry<float>? TirelessArmsDrain;
    internal static ConfigEntry<bool>? HeavyFrameEnabled;
    internal static ConfigEntry<float>? HeavyFrameMoveSpeed;
    internal static ConfigEntry<float>? HeavyFrameHungerThirst;
    internal static ConfigEntry<bool>? PackMuleTankEnabled;       // desdobrado do compartilhado (2026-07-10)
    internal static ConfigEntry<float>? PackMuleTankCarryBonus;
    internal static ConfigEntry<bool>? LoudOperatorTankEnabled;   // desdobrado do compartilhado (2026-07-10)
    internal static ConfigEntry<float>? LoudOperatorTankSoundRadius;

    // 8 · Vanilla Skill Fixes — Weapon Mastery (058)
    internal static ConfigEntry<bool>? WeaponMasteryEnabled;
    internal static ConfigEntry<float>? MasteryXpPerShot;
    internal static ConfigEntry<float>? MasteryRecoilPerLevel;
    internal static ConfigEntry<float>? MasteryErgoPerLevel;

    internal static void Bind(ConfigFile config)
    {
        // ───────────────────────── 0 · General ─────────────────────────
        ShowRaidPerksNotification = config.Bind(
            SecGeneral, "Raid-start perks notification", true,
            "Notificação no início da raid listando os perks (verde) e drawbacks (vermelho) da classe. / Raid-start notification listing the class's perks and drawbacks.");
        DiagnosticsEnabled = config.Bind(
            SecGeneral, "Perk Diagnostics overlay", false,
            "Overlay ao vivo das propriedades afetadas pelos perks do seu player (validação). / Live overlay of the properties affected by your player's perks.");

        // ─────────────────── 1 · Interface & Position ───────────────────
        ClassTabOffsetX = config.Bind(
            SecInterface, "Class Tab — X offset", 0f,
            new ConfigDescription(
                "Ajuste fino da posição horizontal do botão da aba CLASS (px). Só use se a aba não alinhar. / Fine-tune the CLASS tab button horizontal position (px).",
                new AcceptableValueRange<float>(-400f, 400f)));
        ClassDetailOnLoading = config.Bind(
            SecInterface, "Class Detail on Loading Screen", true,
            "Mostra o detalhe da sua classe (perks/drawbacks) no seu nome na tela de carregamento da raid (FIKA). / Show your class detail on the FIKA raid loading screen.");
        LoadingPanelScale = config.Bind(
            SecInterface, "Class Detail — Loading panel scale", 0.75f,
            new ConfigDescription(
                "Escala (zoom-out) do popover de classe no loading (0.75 = 75%). Mesma área na tela, conteúdo menor. / Scale of the loading-screen class popover (same footprint, smaller content).",
                new AcceptableValueRange<float>(0.5f, 1f)));
        // P-13.3 (2026-07-11): defaults calibrados in-game pelo usuário (antes 0/0 = marcador fora de posição).
        WeightMarkerOffsetX = config.Bind(
            SecInterface, "Weight Marker — X offset", -107.0423f,
            new ConfigDescription(
                "Ajuste horizontal (px) do marcador '▲ +X%' no peso (aba Health). Negativo = esquerda. / Horizontal offset (px) of the weight '▲ +X%' marker (Health tab).",
                new AcceptableValueRange<float>(-600f, 600f)));
        WeightMarkerOffsetY = config.Bind(
            SecInterface, "Weight Marker — Y offset", 50.70423f,
            new ConfigDescription(
                "Ajuste vertical (px) do marcador '▲ +X%' no peso (aba Health). Positivo = para cima. / Vertical offset (px) of the weight '▲ +X%' marker (positive = up).",
                new AcceptableValueRange<float>(-600f, 600f)));

        // ───────────────────────── 2 · Combat Medic ─────────────────────────
        // B17: primeiro perk VIVO do Médico — fome/sede ×0.85 (lever do Heavy Frame, branch por classe).
        EfficientMetabolismEnabled = config.Bind(
            SecMedic, "Efficient Metabolism — Enabled", true,
            "Médico: fome/sede drenam mais devagar (metabolismo eficiente). / Combat Medic: slower hunger/thirst drain.");
        EfficientMetabolismHungerThirst = config.Bind(
            SecMedic, "Efficient Metabolism — Hunger/thirst drain", 0.85f,
            new ConfigDescription(
                "Multiplicador do dreno de fome/sede do Médico (0.85 = 15% mais devagar). / Combat Medic hunger/thirst drain multiplier (0.85 = 15% slower).",
                new AcceptableValueRange<float>(0.5f, 1f)));
        // B1: default OFF até os perks do Médico existirem (hoje o Metabolismo já cobre — mas o recuo fica desligado por padrão).
        ShakyHandsEnabled = config.Bind(
            SecMedic, "Shaky Hands — Enabled", false,
            "Médico: +recuo (mãos trêmulas). / Combat Medic: more recoil (shaky hands).");
        ShakyHandsRecoil = config.Bind(
            SecMedic, "Shaky Hands — Recoil mult", 1.25f,
            new ConfigDescription(
                "Multiplicador de recuo do Médico (1.25 = +25%). / Combat Medic recoil multiplier (1.25 = +25%).",
                new AcceptableValueRange<float>(1f, 2f)));

        // ───────────────────────── 3 · Rifleman ─────────────────────────
        CoolUnderFireEnabled = config.Bind(
            SecRifleman, "Cool Under Fire — Enabled", true,
            "Fuzileiro: menos flinch (tranco de câmera) ao levar dano. / Rifleman: less flinch (camera jolt) when hit.");
        CoolUnderFireFlinch = config.Bind(
            SecRifleman, "Cool Under Fire — Flinch mult", 0.50f,
            new ConfigDescription(
                "Multiplicador do tranco ao levar dano (0.50 = −50%). / Aim-punch multiplier when hit (0.50 = −50%).",
                new AcceptableValueRange<float>(0f, 1f)));
        CoolUnderFireMalfChance = config.Bind(
            SecRifleman, "Cool Under Fire — Malfunction chance mult", 0.50f,
            new ConfigDescription(
                "Multiplicador da chance de travamento da arma (0.50 = −50%, anti-jam). / Weapon malfunction chance multiplier (0.50 = −50%, anti-jam).",
                new AcceptableValueRange<float>(0f, 1f)));
        AdrenalineEnabled = config.Bind(
            SecRifleman, "Adrenaline — Enabled", true,
            "Fuzileiro: causar/receber dano abre uma janela com recuo/recarga/ADS melhores. / Rifleman: dealing/taking damage opens a window with better recoil/reload/ADS.");
        AdrenalineDuration = config.Bind(
            SecRifleman, "Adrenaline — Window (s)", 25f,
            new ConfigDescription(
                "Duração da janela em segundos (renovável a cada novo dano). / Window duration in seconds (renewed on each new damage).",
                new AcceptableValueRange<float>(5f, 120f)));
        AdrenalineCooldown = config.Bind(
            SecRifleman, "Adrenaline — Cooldown (s)", 120f,
            new ConfigDescription(
                "Cooldown após a janela, antes de poder reativar. / Cooldown after the window before it can re-trigger.",
                new AcceptableValueRange<float>(0f, 600f)));
        AdrenalineRecoil = config.Bind(
            SecRifleman, "Adrenaline — Recoil mult", 0.70f,
            new ConfigDescription(
                "Multiplicador de recuo na janela (0.70 = −30%). / Recoil multiplier during the window (0.70 = −30%).",
                new AcceptableValueRange<float>(0.3f, 1f)));
        AdrenalineReloadTime = config.Bind(
            SecRifleman, "Adrenaline — Reload time mult", 0.80f,
            new ConfigDescription(
                "Multiplicador do TEMPO de recarga na janela (0.80 = 20% mais rápido). / Reload time multiplier during the window (0.80 = 20% faster).",
                new AcceptableValueRange<float>(0.3f, 1f)));
        AdrenalineAdsTime = config.Bind(
            SecRifleman, "Adrenaline — ADS time mult", 0.80f,
            new ConfigDescription(
                "Multiplicador do TEMPO de ADS na janela (0.80 = 20% mais rápido). / ADS time multiplier during the window (0.80 = 20% faster).",
                new AcceptableValueRange<float>(0.3f, 1f)));
        LoudOperatorRiflemanEnabled = config.Bind(
            SecRifleman, "Loud Operator — Enabled", true,
            "Fuzileiro: aumenta o raio de audibilidade dos seus sons de movimento. / Rifleman: increases the audibility radius of your movement sounds.");
        LoudOperatorRiflemanSoundRadius = config.Bind(
            SecRifleman, "Loud Operator — Sound radius mult", 1.30f,
            new ConfigDescription(
                "Multiplicador do raio de som de movimento do Fuzileiro (1.30 = +30%). / Rifleman movement-sound radius multiplier (1.30 = +30%).",
                new AcceptableValueRange<float>(1f, 2f)));

        // ───────────────────────── 4 · Hunter ─────────────────────────
        // Stalker (2026-07-11): irmão do Ghost Step do Furtivo, porém mais fraco (−20% vs −30%) — o Furtivo
        // continua sendo o dono da furtividade. Mesmos 3 pipelines de som (rolloff, IA base, SAIN).
        StalkerEnabled = config.Bind(
            SecHunter, "Stalker — Enabled", true,
            "Caçador: reduz o raio de audibilidade dos seus sons de movimento (espreita). / Hunter: quieter movement (smaller audibility radius).");
        StalkerSoundRadius = config.Bind(
            SecHunter, "Stalker — Sound radius mult", 0.80f,
            new ConfigDescription(
                "Multiplicador do raio de som de movimento do Caçador (0.80 = −20%). / Hunter movement-sound radius multiplier (0.80 = −20%).",
                new AcceptableValueRange<float>(0.1f, 1f)));

        SharpshooterEnabled = config.Bind(
            SecHunter, "Sharpshooter — Enabled", true,
            "Caçador: mira (ADS) mais rápido. / Hunter: faster ADS.");
        SharpshooterAdsTime = config.Bind(
            SecHunter, "Sharpshooter — ADS time mult", 0.85f,
            new ConfigDescription(
                "Multiplicador do TEMPO de ADS do Caçador (0.85 = 15% mais rápido). / Hunter ADS time multiplier (0.85 = 15% faster).",
                new AcceptableValueRange<float>(0.5f, 1f)));
        IronLungsEnabled = config.Bind(
            SecHunter, "Iron Lungs — Enabled", true,
            "Caçador: segura a respiração por mais tempo. / Hunter: holds breath longer.");
        // B3: dreno ×0.667 ⇒ duração ×1.5 (+50% exatos, o que o card anuncia).
        IronLungsBreathDrain = config.Bind(
            SecHunter, "Iron Lungs — Breath drain mult", 0.667f,
            new ConfigDescription(
                "Multiplicador do consumo de O₂ ao prender a respiração (0.667 → +50% de duração). / Hold-breath O2 drain multiplier (0.667 → +50% duration).",
                new AcceptableValueRange<float>(0.2f, 1f)));
        SteadyArmsEnabled = config.Bind(
            SecHunter, "Steady Arms — Enabled", true,
            "Caçador: braço cansa mais devagar ao mirar (compõe com o stances mod; sem ele, inativo). / Hunter: slower arm fatigue while aiming (requires the stances mod).");
        SteadyArmsDrain = config.Bind(
            SecHunter, "Steady Arms — ADS arm drain mult", 0.65f,
            new ConfigDescription(
                "Multiplicador do dreno de braço do Caçador em ADS (0.65 = 35% mais lento). Requer o stances mod. / Hunter ADS arm-drain multiplier (0.65 = 35% slower). Requires the stances mod.",
                new AcceptableValueRange<float>(0.2f, 1f)));
        RootedEnabled = config.Bind(
            SecHunter, "Rooted — Enabled", true,
            "Caçador: −velocidade de movimento enquanto mira (ADS). / Hunter: slower movement while aiming (ADS).");
        RootedAdsSpeed = config.Bind(
            SecHunter, "Rooted — ADS move speed", 0.85f,
            new ConfigDescription(
                "Velocidade do Caçador enquanto mira (0.85 = −15%). / Hunter move speed while aiming (0.85 = −15%).",
                new AcceptableValueRange<float>(0.5f, 1f)));

        // ───────────────────────── 5 · Stealth ─────────────────────────
        ExecutionSpeedEnabled = config.Bind(
            SecStealth, "Execution — Melee move speed Enabled", true,
            "Furtivo: +velocidade de movimento com a melee na mão. / Stealth: +move speed with the melee in hand.");
        ExecutionMoveSpeed = config.Bind(
            SecStealth, "Execution — Melee move speed", 1.10f,
            new ConfigDescription(
                "Velocidade do Furtivo com a melee na mão (1.10 = +10%). / Stealth move speed with the melee in hand (1.10 = +10%).",
                new AcceptableValueRange<float>(1f, 1.5f)));
        ExecutionMeleeEnabled = config.Bind(
            SecStealth, "Execution — Melee damage Enabled", true,
            "Furtivo: multiplica o dano de golpe de faca. / Stealth: multiplies knife melee damage.");
        ExecutionMeleeDamage = config.Bind(
            SecStealth, "Execution — Melee damage mult", 5.0f,
            new ConfigDescription(
                "Multiplicador do dano de melee do Furtivo (5.0 = 5×, execução). / Stealth melee damage multiplier (5.0 = 5×, execution).",
                new AcceptableValueRange<float>(1f, 10f)));
        GhostStepEnabled = config.Bind(
            SecStealth, "Ghost Step — Enabled", true,
            "Furtivo: reduz o raio de audibilidade dos seus sons de movimento. / Stealth: reduces the audibility radius of your movement sounds.");
        // B2: 0.70 = exatamente o −30% que o card anuncia.
        GhostStepSoundRadius = config.Bind(
            SecStealth, "Ghost Step — Sound radius mult", 0.70f,
            new ConfigDescription(
                "Multiplicador do raio de som de movimento do Furtivo (0.70 = −30%). / Stealth movement-sound radius multiplier (0.70 = −30%).",
                new AcceptableValueRange<float>(0.1f, 1f)));
        RattledEnabled = config.Bind(
            SecStealth, "Rattled — Enabled", true,
            "Furtivo: +tranco de câmera ao levar dano. / Stealth: stronger aim-punch when hit.");
        RattledAimPunch = config.Bind(
            SecStealth, "Rattled — Aim-punch mult", 1.50f,
            new ConfigDescription(
                "Multiplicador do tranco ao levar dano (1.50 = +50%). / Aim-punch multiplier when hit (1.50 = +50%).",
                new AcceptableValueRange<float>(1f, 3f)));

        // ───────────────────────── 6 · Scavenger ─────────────────────────
        // 061: antecipa o bônus ELITE vanilla da skill Search (buff SearchDouble, nível 51) — não é mecânica nova.
        QuickHandsEnabled = config.Bind(
            SecScavenger, "Quick Hands — Enabled", true,
            "Saqueador: revista 2 contêineres ao mesmo tempo (bônus elite da skill Search, desde o início). / Scavenger: search two containers at once (the Search skill's elite bonus, from the start).");

        SilentLooterEnabled = config.Bind(
            SecScavenger, "Silent Looter — Enabled", true,
            "Saqueador: sons de interação/loot mais baixos. / Scavenger: quieter interaction/loot sounds.");
        SilentLooterVolume = config.Bind(
            SecScavenger, "Silent Looter — Volume mult", 0.40f,
            new ConfigDescription(
                "Multiplicador do volume de interação/loot do Saqueador (0.40 = −60%). / Scavenger interaction-sound volume multiplier (0.40 = −60%).",
                new AcceptableValueRange<float>(0.1f, 1f)));
        PackMuleScavEnabled = config.Bind(
            SecScavenger, "Pack Mule — Enabled", true,
            "Saqueador: +limite de carga (piso, não soma com a Strength). / Scavenger: +carry limit (floor, does not stack with Strength).");
        PackMuleScavCarryBonus = config.Bind(
            SecScavenger, "Pack Mule — Carry limit bonus", 0.30f,
            new ConfigDescription(
                "Piso do bônus de limite de carga do Saqueador (0.30 = +30%). / Scavenger carry-limit bonus floor (0.30 = +30%).",
                new AcceptableValueRange<float>(0f, 1f)));
        OverladenEnabled = config.Bind(
            SecScavenger, "Overladen — Enabled", true,
            "Saqueador: inércia escala mais com o peso (movimento clunky carregado). / Scavenger: inertia scales more with weight.");
        OverladenInertia = config.Bind(
            SecScavenger, "Overladen — Inertia mult", 1.50f,
            new ConfigDescription(
                "Multiplicador de inércia do Saqueador (1.50 = +50% sobre a inércia já escalada pelo peso). / Scavenger inertia multiplier (1.50).",
                new AcceptableValueRange<float>(1f, 3f)));

        // ───────────────────────── 7 · Tank ─────────────────────────
        BulwarkEnabled = config.Bind(
            SecTank, "Bulwark — Enabled", true,
            "Tanque: reduz o dano recebido na vida. / Tank: reduces incoming health damage.");
        BulwarkDamageTaken = config.Bind(
            SecTank, "Bulwark — Damage taken", 0.85f,
            new ConfigDescription(
                "Multiplicador do dano recebido (0.85 = −15%). / Incoming damage multiplier (0.85 = −15%).",
                new AcceptableValueRange<float>(0.5f, 1f)));
        BunkerEnabled = config.Bind(
            SecTank, "Bunker — Enabled", true,
            "Tanque: com arma pesada (LMG/HMG/GL) na mão, menos recuo e mais ergonomia. / Tank: heavy weapons (LMG/HMG/GL) handle better.");
        BunkerHeavyRecoil = config.Bind(
            SecTank, "Bunker — Heavy weapon recoil mult", 0.85f,
            new ConfigDescription(
                "Multiplicador de recuo com arma pesada (0.85 = −15%). / Heavy-weapon recoil multiplier (0.85 = −15%).",
                new AcceptableValueRange<float>(0.5f, 1f)));
        BunkerHeavyErgo = config.Bind(
            SecTank, "Bunker — Heavy weapon ergo mult", 1.15f,
            new ConfigDescription(
                "Multiplicador de ergonomia com arma pesada (1.15 = +15%). / Heavy-weapon ergonomics multiplier (1.15 = +15%).",
                new AcceptableValueRange<float>(1f, 1.5f)));
        TirelessArmsEnabled = config.Bind(
            SecTank, "Tireless Arms — Enabled", true,
            "Tanque: braço não cansa segurando arma pesada (compõe com o stances mod; sem ele, inativo). / Tank: no arm fatigue holding heavy weapons (requires the stances mod).");
        TirelessArmsDrain = config.Bind(
            SecTank, "Tireless Arms — Heavy arm drain mult", 0f,
            new ConfigDescription(
                "Multiplicador do dreno de braço do Tanque com arma pesada (0 = não drena). Requer o stances mod. / Tank heavy-weapon arm-drain multiplier (0 = no drain). Requires the stances mod.",
                new AcceptableValueRange<float>(0f, 1f)));
        HeavyFrameEnabled = config.Bind(
            SecTank, "Heavy Frame — Enabled", true,
            "Tanque: −velocidade de movimento (estrutura pesada). / Tank: slower movement (heavy frame).");
        HeavyFrameMoveSpeed = config.Bind(
            SecTank, "Heavy Frame — Move speed", 0.90f,
            new ConfigDescription(
                "Multiplicador de velocidade do Tanque (0.90 = −10%). / Tank move speed multiplier (0.90 = −10%).",
                new AcceptableValueRange<float>(0.5f, 1f)));
        HeavyFrameHungerThirst = config.Bind(
            SecTank, "Heavy Frame — Hunger/thirst drain", 1.30f,
            new ConfigDescription(
                "Multiplicador do dreno de fome/sede do Tanque (1.30 = +30% mais rápido). / Tank hunger/thirst drain multiplier (1.30 = +30% faster).",
                new AcceptableValueRange<float>(1f, 2f)));
        PackMuleTankEnabled = config.Bind(
            SecTank, "Pack Mule — Enabled", true,
            "Tanque: +limite de carga (piso, não soma com a Strength). / Tank: +carry limit (floor, does not stack with Strength).");
        PackMuleTankCarryBonus = config.Bind(
            SecTank, "Pack Mule — Carry limit bonus", 0.30f,
            new ConfigDescription(
                "Piso do bônus de limite de carga do Tanque (0.30 = +30%). / Tank carry-limit bonus floor (0.30 = +30%).",
                new AcceptableValueRange<float>(0f, 1f)));
        LoudOperatorTankEnabled = config.Bind(
            SecTank, "Loud Operator — Enabled", true,
            "Tanque: aumenta o raio de audibilidade dos seus sons de movimento. / Tank: increases the audibility radius of your movement sounds.");
        LoudOperatorTankSoundRadius = config.Bind(
            SecTank, "Loud Operator — Sound radius mult", 1.30f,
            new ConfigDescription(
                "Multiplicador do raio de som de movimento do Tanque (1.30 = +30%). / Tank movement-sound radius multiplier (1.30 = +30%).",
                new AcceptableValueRange<float>(1f, 2f)));

        // ───────────────────── 8 · Vanilla Skill Fixes ─────────────────────
        WeaponMasteryEnabled = config.Bind(
            SecVanillaFixes, "Weapon Mastery — Enabled", true,
            "Ativa as maestrias inertes: XP por disparo do underbarrel (GP-25/M203) + bônus por nível de SMG/LMG/Launcher/Underbarrel. / Enables inert weapon masteries: underbarrel XP per shot + per-level recoil/ergo bonuses.");
        MasteryXpPerShot = config.Bind(
            SecVanillaFixes, "Underbarrel XP per shot", 0.5f,
            new ConfigDescription(
                "XP de Underbarrel Launchers por DISPARO do GP-25/M203 (0.5 = paridade de esforço com SMG). / Underbarrel Launchers XP per shot fired (0.5 = effort parity with SMG).",
                new AcceptableValueRange<float>(0f, 1f)));
        MasteryRecoilPerLevel = config.Bind(
            SecVanillaFixes, "Recoil bonus per level", 0.004f,
            new ConfigDescription(
                "Redução de recuo por nível da maestria da arma na mão (0.004 = −0.4%/nível; paridade WeaponSkillRecoilBonusPerLevel). / Recoil reduction per mastery level of the held weapon (0.004 = −0.4%/level).",
                new AcceptableValueRange<float>(0f, 0.02f)));
        MasteryErgoPerLevel = config.Bind(
            SecVanillaFixes, "Ergo bonus per level", 0.002f,
            new ConfigDescription(
                "Aumento de ergonomia por nível da maestria da arma na mão (0.002 = +0.2%/nível). / Ergonomics increase per mastery level of the held weapon (0.002 = +0.2%/level).",
                new AcceptableValueRange<float>(0f, 0.02f)));
    }
}
