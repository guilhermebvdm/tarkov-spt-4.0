using BepInEx.Configuration;

namespace CustomClasses.Client;

/// <summary>
///     Item 050 — ConfigEntry (F12) dos perks/drawbacks por classe. Fatia 050.0: Bulwark + Pack Mule.
///     Os patches leem <c>.Value</c> NO APPLY-TIME (sem cache) → mudança no F12 vale durante a raid (DoD).
///     Próximas fatias (050.1–050.4) acrescentam suas entries aqui.
/// </summary>
internal static class PerksConfig
{
    // 🛡️ Tanque — Bulwark (dano recebido)
    internal static ConfigEntry<bool>? BulwarkEnabled;
    internal static ConfigEntry<float>? BulwarkDamageTaken;

    // 🎒 Saqueador + 🛡️ Tanque — Pack Mule (limite de carga, piso)
    internal static ConfigEntry<bool>? PackMuleEnabled;
    internal static ConfigEntry<float>? PackMuleCarryBonus;

    // UI — notificação de perks/drawbacks no início da raid
    internal static ConfigEntry<bool>? ShowRaidPerksNotification;

    // 🔻 Heavy Frame (Tanque) — velocidade (050.1) + fome/sede (050.3)
    internal static ConfigEntry<bool>? HeavyFrameEnabled;
    internal static ConfigEntry<float>? HeavyFrameMoveSpeed;
    internal static ConfigEntry<float>? HeavyFrameHungerThirst;

    // 🔧 Execution (Furtivo) — dano de melee (050.3)
    internal static ConfigEntry<bool>? ExecutionMeleeEnabled;
    internal static ConfigEntry<float>? ExecutionMeleeDamage;

    // 🔧 Cool Under Fire (Fuzileiro) — anti-jam (050.3; flinch é 050.2)
    internal static ConfigEntry<float>? CoolUnderFireMalfChance;

    // 🔧 Ghost Step (Furtivo) — raio de som de movimento (050.4)
    internal static ConfigEntry<bool>? GhostStepEnabled;
    internal static ConfigEntry<float>? GhostStepSoundRadius;

    // 🔻 Loud Operator (Fuzileiro) — raio de som de movimento (050.4)
    internal static ConfigEntry<bool>? LoudOperatorEnabled;
    internal static ConfigEntry<float>? LoudOperatorSoundRadius;

    // 🔧 Silent Looter (Saqueador) — som de interação/loot (050.4)
    internal static ConfigEntry<bool>? SilentLooterEnabled;
    internal static ConfigEntry<float>? SilentLooterVolume;

    // 🔧 Bunker (Tanque) — armas pesadas: recuo + ergo (050.4)
    internal static ConfigEntry<bool>? BunkerEnabled;
    internal static ConfigEntry<float>? BunkerHeavyRecoil;
    internal static ConfigEntry<float>? BunkerHeavyErgo;

    // 🔧 Sharpshooter (Caçador) — ADS mais rápido (050.4)
    internal static ConfigEntry<bool>? SharpshooterEnabled;
    internal static ConfigEntry<float>? SharpshooterAdsTime;

    // 🔧 Iron Lungs (Caçador) — segura a respiração por mais tempo (050.4)
    internal static ConfigEntry<bool>? IronLungsEnabled;
    internal static ConfigEntry<float>? IronLungsBreathDrain;

    // 🔻 Overladen (Saqueador) — inércia por peso (050.1)
    internal static ConfigEntry<bool>? OverladenEnabled;
    internal static ConfigEntry<float>? OverladenInertia;

    // 🔻 Rooted (Caçador) — velocidade ao mirar (050.1)
    internal static ConfigEntry<bool>? RootedEnabled;
    internal static ConfigEntry<float>? RootedAdsSpeed;

    // 🔧 Execution (Furtivo) — velocidade com melee na mão (050.1; dano melee ×5 vem no 050.3)
    internal static ConfigEntry<bool>? ExecutionSpeedEnabled;
    internal static ConfigEntry<float>? ExecutionMoveSpeed;

    // 🔻 Shaky Hands (Médico) — recuo (050.2)
    internal static ConfigEntry<bool>? ShakyHandsEnabled;
    internal static ConfigEntry<float>? ShakyHandsRecoil;

    // 🔻 Rattled (Furtivo) — aim-punch ao levar dano (050.2)
    internal static ConfigEntry<bool>? RattledEnabled;
    internal static ConfigEntry<float>? RattledAimPunch;

    // 🔧 Cool Under Fire (Fuzileiro) — menos flinch ao levar dano (050.2; anti-jam vem no 050.3)
    internal static ConfigEntry<bool>? CoolUnderFireEnabled;
    internal static ConfigEntry<float>? CoolUnderFireFlinch;

    // 🔧 Adrenaline (Fuzileiro) — janela de combate: recuo/recarga/ADS (050.2)
    internal static ConfigEntry<bool>? AdrenalineEnabled;
    internal static ConfigEntry<float>? AdrenalineDuration;
    internal static ConfigEntry<float>? AdrenalineCooldown;
    internal static ConfigEntry<float>? AdrenalineRecoil;
    internal static ConfigEntry<float>? AdrenalineReloadTime;
    internal static ConfigEntry<float>? AdrenalineAdsTime;

    internal static void Bind(ConfigFile config)
    {
        BulwarkEnabled = config.Bind(
            "Perks — Tank", "Bulwark — Enabled", true,
            "Tank: reduz o dano recebido na vida. / Tank: reduces incoming health damage.");
        BulwarkDamageTaken = config.Bind(
            "Perks — Tank", "Bulwark — Damage taken", 0.85f,
            new ConfigDescription(
                "Multiplicador do dano recebido (0.85 = -15%). / Incoming damage multiplier.",
                new AcceptableValueRange<float>(0.5f, 1f)));

        PackMuleEnabled = config.Bind(
            "Perks — Scavenger & Tank", "Pack Mule — Enabled", true,
            "Scavenger/Tank: +limite de carga (piso, não soma com a Strength). / Carry limit floor.");
        PackMuleCarryBonus = config.Bind(
            "Perks — Scavenger & Tank", "Pack Mule — Carry limit bonus", 0.30f,
            new ConfigDescription(
                "Piso do bônus de limite de carga (0.30 = +30%). / Carry limit bonus floor.",
                new AcceptableValueRange<float>(0f, 1f)));

        ShowRaidPerksNotification = config.Bind(
            "Perks — UI", "Raid-start perks notification", true,
            "Notificação no início da raid listando os perks (verde) e drawbacks (vermelho) da classe. / Raid-start notification listing the class's perks/drawbacks.");

        HeavyFrameEnabled = config.Bind(
            "Drawbacks — Tank", "Heavy Frame — Enabled", true,
            "Tank: −velocidade de movimento (estrutura pesada). / Tank: slower movement.");
        HeavyFrameMoveSpeed = config.Bind(
            "Drawbacks — Tank", "Heavy Frame — Move speed", 0.90f,
            new ConfigDescription(
                "Multiplicador de velocidade do Tanque (0.90 = -10%). / Tank move speed multiplier.",
                new AcceptableValueRange<float>(0.5f, 1f)));
        HeavyFrameHungerThirst = config.Bind(
            "Drawbacks — Tank", "Heavy Frame — Hunger/thirst drain", 1.30f,
            new ConfigDescription(
                "Multiplicador do dreno de fome/sede do Tanque (1.30 = +30% mais rápido). / Tank hunger/thirst drain multiplier.",
                new AcceptableValueRange<float>(1f, 2f)));

        OverladenEnabled = config.Bind(
            "Drawbacks — Scavenger", "Overladen — Enabled", true,
            "Scavenger: inércia escala mais com o peso (movimento clunky carregado). / Scavenger: more inertia with weight.");
        OverladenInertia = config.Bind(
            "Drawbacks — Scavenger", "Overladen — Inertia mult", 1.50f,
            new ConfigDescription(
                "Multiplicador de inércia do Scavenger (1.50 = +50% sobre a inércia já escalada pelo peso). / Scavenger inertia multiplier.",
                new AcceptableValueRange<float>(1f, 3f)));

        RootedEnabled = config.Bind(
            "Drawbacks — Hunter", "Rooted — Enabled", true,
            "Hunter: −velocidade de movimento enquanto mira (ADS). / Hunter: slower while aiming.");
        RootedAdsSpeed = config.Bind(
            "Drawbacks — Hunter", "Rooted — ADS move speed", 0.85f,
            new ConfigDescription(
                "Velocidade do Hunter enquanto mira (0.85 = -15%). / Hunter move speed while aiming.",
                new AcceptableValueRange<float>(0.5f, 1f)));

        ExecutionSpeedEnabled = config.Bind(
            "Perks — Stealth", "Execution — Melee move speed Enabled", true,
            "Stealth: +velocidade de movimento com a melee na mão. / Stealth: +speed with melee in hand.");
        ExecutionMoveSpeed = config.Bind(
            "Perks — Stealth", "Execution — Melee move speed", 1.10f,
            new ConfigDescription(
                "Velocidade do Stealth com a melee na mão (1.10 = +10%). / Stealth move speed with melee.",
                new AcceptableValueRange<float>(1f, 1.5f)));
        ExecutionMeleeEnabled = config.Bind(
            "Perks — Stealth", "Execution — Melee damage Enabled", true,
            "Stealth: multiplica o dano de golpe de faca. / Stealth: multiplies knife melee damage.");
        ExecutionMeleeDamage = config.Bind(
            "Perks — Stealth", "Execution — Melee damage mult", 5.0f,
            new ConfigDescription(
                "Multiplicador do dano de melee do Stealth (5.0 = 5×, execução). / Stealth melee damage multiplier.",
                new AcceptableValueRange<float>(1f, 10f)));

        ShakyHandsEnabled = config.Bind(
            "Drawbacks — Combat Medic", "Shaky Hands — Enabled", true,
            "Combat Medic: +recuo (mãos trêmulas). / Combat Medic: more recoil.");
        ShakyHandsRecoil = config.Bind(
            "Drawbacks — Combat Medic", "Shaky Hands — Recoil mult", 1.25f,
            new ConfigDescription(
                "Multiplicador de recuo do Médico (1.25 = +25%). / Combat Medic recoil multiplier.",
                new AcceptableValueRange<float>(1f, 2f)));

        RattledEnabled = config.Bind(
            "Drawbacks — Stealth", "Rattled — Enabled", true,
            "Stealth: +tranco de câmera ao levar dano. / Stealth: stronger aim-punch when hit.");
        RattledAimPunch = config.Bind(
            "Drawbacks — Stealth", "Rattled — Aim-punch mult", 1.50f,
            new ConfigDescription(
                "Multiplicador do tranco ao levar dano (1.50 = +50%). / Aim-punch multiplier when hit.",
                new AcceptableValueRange<float>(1f, 3f)));

        CoolUnderFireEnabled = config.Bind(
            "Perks — Rifleman", "Cool Under Fire — Enabled", true,
            "Rifleman: menos flinch (tranco de câmera) ao levar dano. / Rifleman: less flinch when hit.");
        CoolUnderFireFlinch = config.Bind(
            "Perks — Rifleman", "Cool Under Fire — Flinch mult", 0.50f,
            new ConfigDescription(
                "Multiplicador do tranco ao levar dano (0.50 = -50%). / Aim-punch multiplier when hit.",
                new AcceptableValueRange<float>(0f, 1f)));
        CoolUnderFireMalfChance = config.Bind(
            "Perks — Rifleman", "Cool Under Fire — Malfunction chance mult", 0.50f,
            new ConfigDescription(
                "Multiplicador da chance de travamento da arma (0.50 = -50%, anti-jam). / Weapon malfunction chance multiplier.",
                new AcceptableValueRange<float>(0f, 1f)));

        GhostStepEnabled = config.Bind(
            "Perks — Stealth", "Ghost Step — Enabled", true,
            "Stealth: reduz o raio de audibilidade dos seus sons de movimento. / Stealth: quieter movement (smaller audibility radius).");
        GhostStepSoundRadius = config.Bind(
            "Perks — Stealth", "Ghost Step — Sound radius mult", 0.40f,
            new ConfigDescription(
                "Multiplicador do raio de som de movimento do Stealth (0.40 = -60%). / Stealth movement-sound radius multiplier.",
                new AcceptableValueRange<float>(0.1f, 1f)));

        LoudOperatorEnabled = config.Bind(
            "Drawbacks — Rifleman", "Loud Operator — Enabled", true,
            "Rifleman: aumenta o raio de audibilidade dos seus sons de movimento. / Rifleman: louder movement (bigger audibility radius).");
        LoudOperatorSoundRadius = config.Bind(
            "Drawbacks — Rifleman", "Loud Operator — Sound radius mult", 1.30f,
            new ConfigDescription(
                "Multiplicador do raio de som de movimento do Rifleman (1.30 = +30%). / Rifleman movement-sound radius multiplier.",
                new AcceptableValueRange<float>(1f, 2f)));

        SilentLooterEnabled = config.Bind(
            "Perks — Scavenger", "Silent Looter — Enabled", true,
            "Scavenger: sons de interação/loot mais baixos. / Scavenger: quieter loot/interaction sounds.");
        SilentLooterVolume = config.Bind(
            "Perks — Scavenger", "Silent Looter — Volume mult", 0.40f,
            new ConfigDescription(
                "Multiplicador do volume de interação/loot do Scavenger (0.40 = -60%). / Scavenger interaction-sound volume multiplier.",
                new AcceptableValueRange<float>(0.1f, 1f)));

        BunkerEnabled = config.Bind(
            "Perks — Tank", "Bunker — Enabled", true,
            "Tank: com arma pesada (LMG/HMG/GL/underbarrel) na mão, menos recuo e mais ergonomia. / Tank: heavy weapons handle better.");
        BunkerHeavyRecoil = config.Bind(
            "Perks — Tank", "Bunker — Heavy weapon recoil mult", 0.85f,
            new ConfigDescription(
                "Multiplicador de recuo com arma pesada (0.85 = -15%). / Heavy-weapon recoil multiplier.",
                new AcceptableValueRange<float>(0.5f, 1f)));
        BunkerHeavyErgo = config.Bind(
            "Perks — Tank", "Bunker — Heavy weapon ergo mult", 1.15f,
            new ConfigDescription(
                "Multiplicador de ergonomia com arma pesada (1.15 = +15%). / Heavy-weapon ergonomics multiplier.",
                new AcceptableValueRange<float>(1f, 1.5f)));

        SharpshooterEnabled = config.Bind(
            "Perks — Hunter", "Sharpshooter — Enabled", true,
            "Hunter: mira (ADS) mais rápido. / Hunter: faster ADS.");
        SharpshooterAdsTime = config.Bind(
            "Perks — Hunter", "Sharpshooter — ADS time mult", 0.85f,
            new ConfigDescription(
                "Multiplicador do TEMPO de ADS do Hunter (0.85 = 15% mais rápido). / Hunter ADS time multiplier.",
                new AcceptableValueRange<float>(0.5f, 1f)));

        IronLungsEnabled = config.Bind(
            "Perks — Hunter", "Iron Lungs — Enabled", true,
            "Hunter: segura a respiração por mais tempo. / Hunter: holds breath longer.");
        IronLungsBreathDrain = config.Bind(
            "Perks — Hunter", "Iron Lungs — Breath drain mult", 0.50f,
            new ConfigDescription(
                "Multiplicador do consumo de O₂ ao prender a respiração (0.50 = metade → ~2× o tempo). / Hold-breath O2 drain multiplier.",
                new AcceptableValueRange<float>(0.2f, 1f)));

        AdrenalineEnabled = config.Bind(
            "Perks — Rifleman", "Adrenaline — Enabled", true,
            "Rifleman: causar/receber dano abre uma janela com recuo/recarga/ADS melhores. / Rifleman: dealing/taking damage opens a combat window.");
        AdrenalineDuration = config.Bind(
            "Perks — Rifleman", "Adrenaline — Window (s)", 25f,
            new ConfigDescription(
                "Duração da janela em segundos (renovável a cada novo dano). / Window duration in seconds.",
                new AcceptableValueRange<float>(5f, 120f)));
        AdrenalineCooldown = config.Bind(
            "Perks — Rifleman", "Adrenaline — Cooldown (s)", 120f,
            new ConfigDescription(
                "Cooldown após a janela, antes de poder reativar. / Cooldown after the window before re-trigger.",
                new AcceptableValueRange<float>(0f, 600f)));
        AdrenalineRecoil = config.Bind(
            "Perks — Rifleman", "Adrenaline — Recoil mult", 0.70f,
            new ConfigDescription(
                "Multiplicador de recuo na janela (0.70 = -30%). / Recoil multiplier during the window.",
                new AcceptableValueRange<float>(0.3f, 1f)));
        AdrenalineReloadTime = config.Bind(
            "Perks — Rifleman", "Adrenaline — Reload time mult", 0.80f,
            new ConfigDescription(
                "Multiplicador do TEMPO de recarga na janela (0.80 = 20% mais rápido). / Reload time multiplier.",
                new AcceptableValueRange<float>(0.3f, 1f)));
        AdrenalineAdsTime = config.Bind(
            "Perks — Rifleman", "Adrenaline — ADS time mult", 0.80f,
            new ConfigDescription(
                "Multiplicador do TEMPO de ADS na janela (0.80 = 20% mais rápido). / ADS time multiplier.",
                new AcceptableValueRange<float>(0.3f, 1f)));
    }
}
