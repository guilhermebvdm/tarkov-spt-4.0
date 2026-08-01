using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

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
    internal const string SecNaked = "8 · Naked";                         // 067 — seção própria do Peladão (cor; futuro 068)
    internal const string SecVanillaFixes = "9 · Vanilla Skill Fixes";    // 067 — era "8 ·"; renomear a seção RESETA as 4 props (BREAKING)

    // 067 — override de COR por classe (F12). ClassColors: NameEn EN ("Combat Medic"…"Tank"/"Naked") →
    // (toggle 'Override color' + ConfigEntry<Color> com o color picker nativo do ConfigurationManager).
    // Populado no Bind por BindClassColor; lido por ClassColorOverride.Resolve. Default do toggle = OFF → a cor
    // do server (ClassVisualRegistry) segue sendo a fonte de verdade; ligar sobrescreve SÓ aquela classe.
    internal sealed class ClassColorEntry
    {
        internal readonly ConfigEntry<bool> Override;
        internal readonly ConfigEntry<Color> Color;

        internal ClassColorEntry(ConfigEntry<bool> ovr, ConfigEntry<Color> color)
        {
            Override = ovr;
            Color = color;
        }
    }

    // OrdinalIgnoreCase: alinha com o ClassNameEnOf/IsClass (case-insensitive no resto do mod) e tolera drift de
    // caixa. ⚠️ Limitação: as 7 chaves são os nomes EN SHIPPED — se o usuário RENOMEAR displayName.en de uma
    // classe pelo editor web, a chave deixa de casar e o override daquela classe vira no-op (a cor cai no server,
    // que já reflete o rename → o nome segue colorido, só o lever do F12 desconecta). Cobrir rename exigiria
    // derivar as seções do registry (fora do escopo do 067).
    internal static readonly Dictionary<string, ClassColorEntry> ClassColors = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>067 — disparado quando qualquer toggle/cor de classe muda no F12 (usado p/ re-aplicar a cor no
    /// menu ao vivo; as outras superfícies resolvem no próximo render). Ver <see cref="ClassColorOverride"/>.</summary>
    internal static event Action? ClassColorsChanged;

    // ─────────────── Ordenação do F12 (review de propriedades MP-01-01/02/10) ───────────────
    // O ConfigurationManager ordena os itens de CADA seção ALFABETICAMENTE pela key — o que jogava o "— Enabled" de
    // cada perk para o meio (sempre que um valor começa com letra < E) e separava o par de cor. Este espelho
    // duck-typed do atributo do CM (os NOMES dos campos TÊM que bater) + o helper BindOrdered injetam um Order
    // DECRESCENTE na ordem em que os binds aparecem no código → o F12 passa a espelhar o código. Como o código já
    // binda "Enabled" antes dos valores e a cor por último, sai logicamente ordenado sem reordenar uma linha, e sem
    // tocar em (seção, key) → NÃO breaking (valores salvos preservados). Order MAIOR = mais no topo da seção.
    internal sealed class ConfigurationManagerAttributes
    {
        public int? Order;
        public bool? IsAdvanced;
    }

    private static int _order = short.MaxValue;   // 1º bind = topo; decrementa a cada BindOrdered (contador compartilhado Plugin+PerksConfig)

    /// <summary>Bind com Order automático — tooltip simples (sem range).</summary>
    internal static ConfigEntry<T> BindOrdered<T>(ConfigFile cfg, string section, string key, T def, string tooltip, bool advanced = false)
    {
        var attr = new ConfigurationManagerAttributes { Order = _order--, IsAdvanced = advanced ? true : (bool?)null };
        return cfg.Bind(section, key, def, new ConfigDescription(tooltip, null, attr));
    }

    /// <summary>Bind com Order automático — recebe um <see cref="ConfigDescription"/> já montado (com range) e anexa a tag Order às existentes.</summary>
    internal static ConfigEntry<T> BindOrdered<T>(ConfigFile cfg, string section, string key, T def, ConfigDescription desc)
    {
        var attr = new ConfigurationManagerAttributes { Order = _order-- };
        object[] tags;
        if (desc.Tags == null || desc.Tags.Length == 0)
        {
            tags = new object[] { attr };
        }
        else
        {
            tags = new object[desc.Tags.Length + 1];
            Array.Copy(desc.Tags, tags, desc.Tags.Length);
            tags[desc.Tags.Length] = attr;
        }

        return cfg.Bind(section, key, def, new ConfigDescription(desc.Description, desc.AcceptableValues, tags));
    }

    // 0 · General — notificação + diagnóstico + piso global de recuo (B15)
    internal static ConfigEntry<bool>? ShowRaidPerksNotification;
    internal static ConfigEntry<bool>? DiagnosticsEnabled;
    internal static ConfigEntry<bool>? RecoilFloorEnabled;
    internal static ConfigEntry<float>? RecoilFloor;

    // 1 · Interface & Position
    internal static ConfigEntry<float>? ClassTabOffsetX;
    internal static ConfigEntry<bool>? ClassDetailOnLoading;
    internal static ConfigEntry<float>? LoadingPanelScale;
    internal static ConfigEntry<float>? WeightMarkerOffsetX;
    internal static ConfigEntry<float>? WeightMarkerOffsetY;

    // 2 · Combat Medic
    internal static ConfigEntry<bool>? EfficientMetabolismEnabled;
    internal static ConfigEntry<float>? EfficientMetabolismHungerThirst;
    internal static ConfigEntry<bool>? RapidCareEnabled;          // 072
    internal static ConfigEntry<float>? RapidCareUseTime;         // 072
    internal static ConfigEntry<bool>? SwiftSurgeonEnabled;       // 072
    internal static ConfigEntry<float>? SwiftSurgeonTime;         // 072
    // 079: Mobile Surgery REMOVIDO (decisão do usuário — o Médico não anda mais em cirurgia).
    internal static ConfigEntry<bool>? RestorativeSurgeryEnabled;    // 076
    internal static ConfigEntry<float>? RestorativeSurgeryRetention; // 076 (v0.6.1: piso de HP máx retido, era "penalty mult")
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
    internal static ConfigEntry<bool>? CalmSightsEnabled;         // 072
    internal static ConfigEntry<float>? CalmSightsSway;           // 072
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
    internal static ConfigEntry<bool>? SilentKnifeEnabled;   // 083 (Morte Silenciosa — faca sem som)

    // 6 · Scavenger
    internal static ConfigEntry<bool>? QuickHandsEnabled;   // 061: busca 2 contêineres (bônus elite da Search, antecipado)
    internal static ConfigEntry<bool>? SilentLooterEnabled;
    internal static ConfigEntry<float>? SilentLooterVolume;
    internal static ConfigEntry<bool>? PackMuleScavEnabled;       // desdobrado do compartilhado (2026-07-10)
    internal static ConfigEntry<float>? PackMuleScavCarryBonus;
    // 079: Overladen REMOVIDO (substituído pela Lebre, item 081). Levers novos do 079:
    internal static ConfigEntry<bool>? LightFrameEnabled;         // 079 Caçador + Furtivo (carga reduzida)
    internal static ConfigEntry<float>? LightFrameCarryPenalty;   // 079
    internal static ConfigEntry<bool>? LoudLooterEnabled;         // 079 Fuzileiro (loot barulhento)
    internal static ConfigEntry<float>? LoudLooterVolume;         // 079
    internal static ConfigEntry<bool>? QuickDrawEnabled;          // 080 Caçador+Fuzileiro+Furtivo (saque do holster)
    internal static ConfigEntry<float>? QuickDrawDrawInTime;      // 087 fase 3 — SACAR a arma (draw-in)
    internal static ConfigEntry<float>? QuickDrawPutAwayTime;     // 088 fase 1 — GUARDAR a arma anterior (put-away)
    internal static ConfigEntry<bool>? LebreEnabled;             // 081 Saqueador (velocidade quando leve)
    internal static ConfigEntry<float>? LebreSpeed;              // 081
    internal static ConfigEntry<bool>? MedrosoEnabled;           // 082 Saqueador (tremor sob fogo)
    internal static ConfigEntry<float>? MedrosoDuration;         // 082
    internal static ConfigEntry<float>? MedrosoCooldown;         // 082
    internal static ConfigEntry<float>? MedrosoSuppressDistance; // 082

    // 7 · Tank
    internal static ConfigEntry<bool>? BulwarkEnabled;
    internal static ConfigEntry<float>? BulwarkDamageTaken;
    internal static ConfigEntry<bool>? BulwarkRequireHeavyArmor;   // B6: só com armadura pesada equipada
    internal static ConfigEntry<int>? BulwarkMinArmorClass;
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
    internal static ConfigEntry<bool>? ShotgunReloadEnabled;     // 084 (recarga de escopeta tubular mais rápida)
    internal static ConfigEntry<float>? ShotgunReloadTime;       // 084

    // 9 · Vanilla Skill Fixes — Weapon Mastery (058; renumerado 8→9 no 067)
    internal static ConfigEntry<bool>? WeaponMasteryEnabled;
    internal static ConfigEntry<float>? MasteryXpPerShot;
    internal static ConfigEntry<float>? MasteryRecoilPerLevel;
    internal static ConfigEntry<float>? MasteryErgoPerLevel;

    internal static void Bind(ConfigFile config)
    {
        EnsureColorConverter();   // 067: serialização de Color no .cfg (antes de qualquer BindClassColor)

        // ───────────────────────── 0 · General ─────────────────────────
        ShowRaidPerksNotification = BindOrdered(config, 
            SecGeneral, "Raid-start perks notification", true,
            "Notificação no início da raid listando os perks (verde) e drawbacks (vermelho) da classe. / Raid-start notification listing the class's perks and drawbacks.");
        DiagnosticsEnabled = BindOrdered(config, 
            SecGeneral, "Perk Diagnostics overlay", false,
            "Overlay ao vivo das propriedades afetadas pelos perks do SEU player + log dos perks de SOM aplicados aos PEERS (coop; sai no LogOutput.log). Só para validação. / "
            + "Live overlay of the properties affected by YOUR player's perks + a log of the SOUND perks applied to PEERS (coop; written to LogOutput.log). Validation only.",
            advanced: true);   // MP-01-10: ferramenta de debug → esconde no "Advanced" do F12

        // B15 (balance 2026-07-11): os multiplicadores de recuo empilham por PRODUTO (maestria × Bunker/
        // Adrenalina). A maestria tem piso próprio (0.5, inalcançável no cap 51), mas o PRODUTO não tinha
        // piso nenhum: Tanque+LMG+maestria 51 ≈ ×0.68 · Fuzileiro na janela de Adrenalina ≈ ×0.56. Piso 0.60
        // morde essencialmente a janela de Adrenalina — o resto passa incólume.
        RecoilFloorEnabled = BindOrdered(config, 
            SecGeneral, "Recoil floor — Enabled", true,
            "Piso do recuo COMBINADO (maestria × perks). Impede que o produto dos multiplicadores derrube o recuo demais. / Floor for the COMBINED recoil multiplier (mastery × perks).");
        RecoilFloor = BindOrdered(config, 
            SecGeneral, "Recoil floor — Min combined mult", 0.60f,
            new ConfigDescription(
                "Recuo mínimo como fração do original (0.60 = nunca abaixo de −40% no total). / Minimum recoil as a fraction of the original (0.60 = never below -40% combined).",
                new AcceptableValueRange<float>(0.3f, 1f)));

        // ─────────────────── 1 · Interface & Position ───────────────────
        ClassTabOffsetX = BindOrdered(config, 
            SecInterface, "Class Tab — X offset", 0f,
            new ConfigDescription(
                "Ajuste fino da posição horizontal do botão da aba CLASS (px). Só use se a aba não alinhar. / Fine-tune the CLASS tab button horizontal position (px).",
                new AcceptableValueRange<float>(-400f, 400f)));
        ClassDetailOnLoading = BindOrdered(config, 
            SecInterface, "Class Detail on Loading Screen", true,
            "Mostra o detalhe da sua classe (perks/drawbacks) no seu nome na tela de carregamento da raid (FIKA). / Show your class detail on the FIKA raid loading screen.");
        LoadingPanelScale = BindOrdered(config, 
            SecInterface, "Class Detail — Loading panel scale", 0.75f,
            new ConfigDescription(
                "Escala (zoom-out) do popover de classe no loading (0.75 = 75%). Mesma área na tela, conteúdo menor. / Scale of the loading-screen class popover (same footprint, smaller content).",
                new AcceptableValueRange<float>(0.5f, 1f)));
        // P-13.3 (2026-07-11): defaults calibrados in-game pelo usuário (antes 0/0 = marcador fora de posição).
        WeightMarkerOffsetX = BindOrdered(config, 
            SecInterface, "Weight Marker — X offset", -107.0423f,
            new ConfigDescription(
                "Ajuste horizontal (px) do marcador '▲ +X%' no peso (aba Health). Negativo = esquerda. / Horizontal offset (px) of the weight '▲ +X%' marker (Health tab).",
                new AcceptableValueRange<float>(-600f, 600f)));
        WeightMarkerOffsetY = BindOrdered(config, 
            SecInterface, "Weight Marker — Y offset", 50.70423f,
            new ConfigDescription(
                "Ajuste vertical (px) do marcador '▲ +X%' no peso (aba Health). Positivo = para cima. / Vertical offset (px) of the weight '▲ +X%' marker (positive = up).",
                new AcceptableValueRange<float>(-600f, 600f)));

        // ───────────────────────── 2 · Combat Medic ─────────────────────────
        // B17: primeiro perk VIVO do Médico — fome/sede ×0.85 (lever do Heavy Frame, branch por classe).
        EfficientMetabolismEnabled = BindOrdered(config, 
            SecMedic, "Efficient Metabolism — Enabled", true,
            "Médico: fome/sede drenam mais devagar (metabolismo eficiente). / Combat Medic: slower hunger/thirst drain.");
        EfficientMetabolismHungerThirst = BindOrdered(config, 
            SecMedic, "Efficient Metabolism — Hunger/thirst drain", 0.85f,
            new ConfigDescription(
                "Multiplicador do dreno de fome/sede do Médico (0.85 = 15% mais devagar). / Combat Medic hunger/thirst drain multiplier (0.85 = 15% slower).",
                new AcceptableValueRange<float>(0.5f, 1f)));
        // 072 — os 3 perks de assinatura do Médico, deferidos no 050 e agora implementados.
        RapidCareEnabled = BindOrdered(config, 
            SecMedic, "Rapid Care — Enabled", true,
            "Médico: curativos e estabilizações são mais rápidos (efeito E animação). / Combat Medic: faster heals and stabilizations (both effect and animation).");
        RapidCareUseTime = BindOrdered(config, 
            SecMedic, "Rapid Care — Use time mult", 0.75f,
            new ConfigDescription(
                "Multiplicador do tempo de uso de itens médicos (0.75 = 25% mais rápido). Não vale para o kit de cirurgia (veja Swift Surgeon). / Medical item use-time multiplier (0.75 = 25% faster). Does not apply to the surgery kit (see Swift Surgeon).",
                new AcceptableValueRange<float>(0.3f, 1f)));
        SwiftSurgeonEnabled = BindOrdered(config, 
            SecMedic, "Swift Surgeon — Enabled", true,
            "Médico: cirurgia (CMS/Surv12) muito mais rápida. / Combat Medic: much faster surgery (CMS/Surv12).");
        SwiftSurgeonTime = BindOrdered(config, 
            SecMedic, "Swift Surgeon — Surgery time mult", 0.75f,
            new ConfigDescription(
                "Multiplicador do tempo de cirurgia (0.75 = 25% mais rápido). A skill Surgery do jogador segue valendo por cima. / Surgery time multiplier (0.75 = 25% faster). The player's Surgery skill still stacks on top.",
                new AcceptableValueRange<float>(0.3f, 1f)));
        // 079: Mobile Surgery REMOVIDO (o Médico não anda mais em cirurgia — nem própria nem de aliado).
        // 076 — a cirurgia do Médico não deixa a "cicatriz" permanente de HP máximo. Vale p/ a própria cirurgia
        // (caminho nativo) E p/ aliados operados via ICM (TRL-ImmersiveCombatMedicine), gateado pela classe do OPERADOR.
        RestorativeSurgeryEnabled = BindOrdered(config, 
            SecMedic, "Restorative Surgery — Enabled", true,
            "Médico: a cirurgia restaura o membro a ~80% do HP MÁXIMO (configurável abaixo), em vez da cicatriz grande do vanilla (CMS mantém só 25–45%, Surv12 60–72%). Vale para a cirurgia no próprio Médico e nos aliados que ele opera (via ICM). / Combat Medic: surgery restores the limb to ~80% of MAX HP (configurable below) instead of vanilla's big scar (CMS keeps only 25–45%, Surv12 60–72%). Applies to the medic's own surgery and to allies they operate on (via ICM).");
        RestorativeSurgeryRetention = BindOrdered(config, 
            SecMedic, "Restorative Surgery — Restored max HP", 0.80f,
            new ConfigDescription(
                "Fração MÍNIMA do HP máximo que o membro operado retém (0.80 = volta com 80%). É um PISO: nunca pior que o vanilla, e a skill Surgery do jogador pode melhorar ALÉM disto. / Minimum fraction of the limb's max HP retained after surgery (0.80 = comes back at 80%). It's a FLOOR: never worse than vanilla, and the player's Surgery skill can push beyond it.",
                new AcceptableValueRange<float>(0f, 1f)));
        // 079: "Shaky Hands" renomeado p/ "Unskilled" / "Falta de habilidade" + LIGADO (era OFF) + agora
        // Médico E Saqueador (gate no ShootRecoilPatch). Key F12 renomeada → reseta o valor salvo (changelog).
        ShakyHandsEnabled = BindOrdered(config, 
            SecMedic, "Unskilled — Enabled", true,
            "Médico/Saqueador: +recuo por falta de habilidade com armas de fogo. / Combat Medic/Scavenger: more recoil from lack of firearm skill.");
        ShakyHandsRecoil = BindOrdered(config, 
            SecMedic, "Unskilled — Recoil mult", 1.25f,
            new ConfigDescription(
                "Multiplicador de recuo por falta de habilidade (1.25 = +25%). / Recoil multiplier from lack of skill (1.25 = +25%).",
                new AcceptableValueRange<float>(1f, 2f)));
        BindClassColor(config, SecMedic, "Combat Medic", "#6f9455");   // 067

        // ───────────────────────── 3 · Rifleman ─────────────────────────
        CoolUnderFireEnabled = BindOrdered(config, 
            SecRifleman, "Cool Under Fire — Enabled", true,
            "Fuzileiro: menos flinch (tranco de câmera) ao levar dano. / Rifleman: less flinch (camera jolt) when hit.");
        CoolUnderFireFlinch = BindOrdered(config, 
            SecRifleman, "Cool Under Fire — Flinch mult", 0.50f,
            new ConfigDescription(
                "Multiplicador do tranco ao levar dano (0.50 = −50%). / Aim-punch multiplier when hit (0.50 = −50%).",
                new AcceptableValueRange<float>(0f, 1f)));
        CoolUnderFireMalfChance = BindOrdered(config, 
            SecRifleman, "Cool Under Fire — Malfunction chance mult", 0.50f,
            new ConfigDescription(
                "Multiplicador da chance de travamento da arma (0.50 = −50%, anti-jam). / Weapon malfunction chance multiplier (0.50 = −50%, anti-jam).",
                new AcceptableValueRange<float>(0f, 1f)));
        AdrenalineEnabled = BindOrdered(config, 
            SecRifleman, "Adrenaline — Enabled", true,
            "Fuzileiro: causar/receber dano abre uma janela com recuo/recarga/ADS melhores. / Rifleman: dealing/taking damage opens a window with better recoil/reload/ADS.");
        AdrenalineDuration = BindOrdered(config, 
            SecRifleman, "Adrenaline — Window (s)", 25f,
            new ConfigDescription(
                "Duração da janela em segundos (renovável a cada novo dano). / Window duration in seconds (renewed on each new damage).",
                new AcceptableValueRange<float>(5f, 120f)));
        AdrenalineCooldown = BindOrdered(config, 
            SecRifleman, "Adrenaline — Cooldown (s)", 120f,
            new ConfigDescription(
                "Cooldown após a janela, antes de poder reativar. / Cooldown after the window before it can re-trigger.",
                new AcceptableValueRange<float>(0f, 600f)));
        AdrenalineRecoil = BindOrdered(config, 
            SecRifleman, "Adrenaline — Recoil mult", 0.70f,
            new ConfigDescription(
                "Multiplicador de recuo na janela (0.70 = −30%). / Recoil multiplier during the window (0.70 = −30%).",
                new AcceptableValueRange<float>(0.3f, 1f)));
        AdrenalineReloadTime = BindOrdered(config, 
            SecRifleman, "Adrenaline — Reload time mult", 0.7f,
            new ConfigDescription(
                "Multiplicador do TEMPO de recarga na janela (0.7 = 30% mais rápido). / Reload time multiplier during the window (0.7 = 30% faster).",
                new AcceptableValueRange<float>(0.3f, 1f)));
        AdrenalineAdsTime = BindOrdered(config, 
            SecRifleman, "Adrenaline — ADS time mult", 0.7f,
            new ConfigDescription(
                "Multiplicador do TEMPO de ADS na janela (0.7 = 30% mais rápido). / ADS time multiplier during the window (0.7 = 30% faster).",
                new AcceptableValueRange<float>(0.3f, 1f)));
        LoudOperatorRiflemanEnabled = BindOrdered(config, 
            SecRifleman, "Loud Operator — Enabled", true,
            "Fuzileiro: aumenta o raio de audibilidade dos seus sons de movimento. / Rifleman: increases the audibility radius of your movement sounds.");
        LoudOperatorRiflemanSoundRadius = BindOrdered(config, 
            SecRifleman, "Loud Operator — Sound radius mult", 1.30f,
            new ConfigDescription(
                "Multiplicador do raio de som de movimento do Fuzileiro (1.30 = +30%). / Rifleman movement-sound radius multiplier (1.30 = +30%).",
                new AcceptableValueRange<float>(1f, 2f)));
        BindClassColor(config, SecRifleman, "Rifleman", "#b0573a");   // 067

        // ───────────────────────── 4 · Hunter ─────────────────────────
        // Stalker (2026-07-11): irmão do Ghost Step do Furtivo, porém mais fraco (−20% vs −30%) — o Furtivo
        // continua sendo o dono da furtividade. Mesmos 3 pipelines de som (rolloff, IA base, SAIN).
        StalkerEnabled = BindOrdered(config, 
            SecHunter, "Stalker — Enabled", true,
            "Caçador: reduz o raio de audibilidade dos seus sons de movimento (espreita). / Hunter: quieter movement (smaller audibility radius).");
        StalkerSoundRadius = BindOrdered(config, 
            SecHunter, "Stalker — Sound radius mult", 0.80f,
            new ConfigDescription(
                "Multiplicador do raio de som de movimento do Caçador (0.80 = −20%). / Hunter movement-sound radius multiplier (0.80 = −20%).",
                new AcceptableValueRange<float>(0.1f, 1f)));

        SharpshooterEnabled = BindOrdered(config, 
            SecHunter, "Sharpshooter — Enabled", true,
            "Caçador: mira (ADS) mais rápido. / Hunter: faster ADS.");
        SharpshooterAdsTime = BindOrdered(config, 
            SecHunter, "Sharpshooter — ADS time mult", 0.85f,
            new ConfigDescription(
                "Multiplicador do TEMPO de ADS do Caçador (0.85 = 15% mais rápido). / Hunter ADS time multiplier (0.85 = 15% faster).",
                new AcceptableValueRange<float>(0.5f, 1f)));
        IronLungsEnabled = BindOrdered(config, 
            SecHunter, "Iron Lungs — Enabled", true,
            "Caçador: segura a respiração por mais tempo. / Hunter: holds breath longer.");
        // B3: dreno ×0.667 ⇒ duração ×1.5 (+50% exatos, o que o card anuncia).
        IronLungsBreathDrain = BindOrdered(config, 
            SecHunter, "Iron Lungs — Breath drain mult", 0.7f,
            new ConfigDescription(
                "Multiplicador do consumo de O₂ ao prender a respiração (0.7 → +43% de duração). / Hold-breath O2 drain multiplier (0.7 → +43% duration).",
                new AcceptableValueRange<float>(0.2f, 1f)));
        SteadyArmsEnabled = BindOrdered(config, 
            SecHunter, "Steady Arms — Enabled", true,
            "Caçador: braço cansa mais devagar ao mirar (compõe com o stances mod; sem ele, inativo). / Hunter: slower arm fatigue while aiming (requires the stances mod).");
        SteadyArmsDrain = BindOrdered(config, 
            SecHunter, "Steady Arms — ADS arm drain mult", 0.65f,
            new ConfigDescription(
                "Multiplicador do dreno de braço do Caçador em ADS (0.65 = 35% mais lento). Requer o stances mod. / Hunter ADS arm-drain multiplier (0.65 = 35% slower). Requires the stances mod.",
                new AcceptableValueRange<float>(0.2f, 1f)));
        // 072 — Calm Sights. ⚠️ Afeta o sway de MIRA/MOVIMENTO (mouse), não o de RESPIRAÇÃO (outro effector).
        CalmSightsEnabled = BindOrdered(config, 
            SecHunter, "Calm Sights — Enabled", true,
            "Caçador: a arma oscila menos (sway de mira/movimento). Não afeta o sway da respiração — para isso, veja Iron Lungs. / Hunter: less weapon sway (aim/movement sway). Does not affect breathing sway — see Iron Lungs for that.");
        CalmSightsSway = BindOrdered(config, 
            SecHunter, "Calm Sights — Sway mult", 0.7f,
            new ConfigDescription(
                "Multiplicador da oscilação (sway) da arma (0.7 = 30% menos). / Weapon sway multiplier (0.7 = 30% less).",
                new AcceptableValueRange<float>(0.3f, 1f)));
        RootedEnabled = BindOrdered(config, 
            SecHunter, "Rooted — Enabled", true,
            "Caçador: −velocidade de movimento enquanto mira (ADS). / Hunter: slower movement while aiming (ADS).");
        RootedAdsSpeed = BindOrdered(config, 
            SecHunter, "Rooted — ADS move speed", 0.85f,
            new ConfigDescription(
                "Velocidade do Caçador enquanto mira (0.85 = −15%). / Hunter move speed while aiming (0.85 = −15%).",
                new AcceptableValueRange<float>(0.5f, 1f)));
        BindClassColor(config, SecHunter, "Hunter", "#c2973f");   // 067

        // ───────────────────────── 5 · Stealth ─────────────────────────
        ExecutionSpeedEnabled = BindOrdered(config, 
            SecStealth, "Execution Speed — Enabled", true,
            "Furtivo: +velocidade de movimento com a melee na mão. / Stealth: +move speed with the melee in hand.");
        ExecutionMoveSpeed = BindOrdered(config, 
            SecStealth, "Execution Speed — Move speed mult", 1.10f,
            new ConfigDescription(
                "Velocidade do Furtivo com a melee na mão (1.10 = +10%). / Stealth move speed with the melee in hand (1.10 = +10%).",
                new AcceptableValueRange<float>(1f, 1.5f)));
        ExecutionMeleeEnabled = BindOrdered(config, 
            SecStealth, "Execution Melee — Enabled", true,
            "Furtivo: multiplica o dano de golpe de faca. / Stealth: multiplies knife melee damage.");
        // B7 (balance 2026-07-11): ×5 → ×3.5. O ×5 era one-shot trivial (kill garantido); ×3.5 mantém a
        // faca como arma real de execução, mas exige posicionamento — sem o kill automático.
        ExecutionMeleeDamage = BindOrdered(config, 
            SecStealth, "Execution Melee — Damage mult", 3.5f,
            new ConfigDescription(
                "Multiplicador do dano de melee do Furtivo (3.5 = 3.5×, execução). / Stealth melee damage multiplier (3.5 = 3.5×, execution).",
                new AcceptableValueRange<float>(1f, 10f)));
        GhostStepEnabled = BindOrdered(config, 
            SecStealth, "Ghost Step — Enabled", true,
            "Furtivo: reduz o raio de audibilidade dos seus sons de movimento. / Stealth: reduces the audibility radius of your movement sounds.");
        // B2: 0.70 = exatamente o −30% que o card anuncia.
        GhostStepSoundRadius = BindOrdered(config, 
            SecStealth, "Ghost Step — Sound radius mult", 0.70f,
            new ConfigDescription(
                "Multiplicador do raio de som de movimento do Furtivo (0.70 = −30%). / Stealth movement-sound radius multiplier (0.70 = −30%).",
                new AcceptableValueRange<float>(0.1f, 1f)));
        RattledEnabled = BindOrdered(config, 
            SecStealth, "Rattled — Enabled", true,
            "Furtivo: +tranco de câmera ao levar dano. / Stealth: stronger aim-punch when hit.");
        RattledAimPunch = BindOrdered(config, 
            SecStealth, "Rattled — Aim-punch mult", 1.50f,
            new ConfigDescription(
                "Multiplicador do tranco ao levar dano (1.50 = +50%). / Aim-punch multiplier when hit (1.50 = +50%).",
                new AcceptableValueRange<float>(1f, 3f)));
        // 083 — Morte Silenciosa (Furtivo): a faca não faz som (sacar + golpe + acerto). Um único choke de áudio
        // (BaseSoundPlayer.PlayClip) gateado por faca + classe do EMISSOR — cobre coop (você não ouve a faca do peer
        // Furtivo). A IA nunca foi alertada por SOM de faca no vanilla (só pelo dano), então não há nada a suprimir lá.
        SilentKnifeEnabled = BindOrdered(config, 
            SecStealth, "Silent Knife — Enabled", true,
            "Furtivo: a faca não faz barulho (sacar, golpear e acertar são silenciosos). / Stealth: the knife makes no sound (drawing, swinging and hitting are all silent).");
        BindClassColor(config, SecStealth, "Stealth", "#8b8fa3");   // 067

        // ───────────────────────── 6 · Scavenger ─────────────────────────
        // 061: antecipa o bônus ELITE vanilla da skill Search (buff SearchDouble, nível 51) — não é mecânica nova.
        QuickHandsEnabled = BindOrdered(config, 
            SecScavenger, "Quick Hands — Enabled", true,
            "Saqueador: revista 2 contêineres ao mesmo tempo (bônus elite da skill Search, desde o início). / Scavenger: search two containers at once (the Search skill's elite bonus, from the start).");

        SilentLooterEnabled = BindOrdered(config, 
            SecScavenger, "Silent Looter — Enabled", true,
            "Saqueador: sons de interação/loot mais baixos. / Scavenger: quieter interaction/loot sounds.");
        SilentLooterVolume = BindOrdered(config, 
            SecScavenger, "Silent Looter — Volume mult", 0.40f,
            new ConfigDescription(
                "Multiplicador do volume de interação/loot do Saqueador (0.40 = −60%). / Scavenger interaction-sound volume multiplier (0.40 = −60%).",
                new AcceptableValueRange<float>(0.1f, 1f)));
        PackMuleScavEnabled = BindOrdered(config, 
            SecScavenger, "Pack Mule — Enabled", true,
            "Saqueador: +limite de carga (piso, não soma com a Strength). / Scavenger: +carry limit (floor, does not stack with Strength).");
        PackMuleScavCarryBonus = BindOrdered(config, 
            SecScavenger, "Pack Mule — Carry limit bonus", 0.30f,
            new ConfigDescription(
                "Piso do bônus de limite de carga do Saqueador (0.30 = +30%). / Scavenger carry-limit bonus floor (0.30 = +30%).",
                new AcceptableValueRange<float>(0f, 1f)));
        // 079: Overladen REMOVIDO (substituído pela Lebre, item 081). Aqui entram os 2 levers NOVOS do 079
        // (a seção no F12 vem do 1º arg SecHunter/SecRifleman, não da posição física no código).
        // Light Frame (Caçador + Furtivo): limite de carga REDUZIDO. Valor NEGATIVO (teto, não piso — ver PackMulePatch).
        LightFrameEnabled = BindOrdered(config, 
            SecHunter, "Light Frame — Enabled", true,
            "Caçador/Furtivo: limite de carga reduzido (estrutura leve — leva menos loot). / Hunter/Stealth: reduced carry limit (light frame).");
        LightFrameCarryPenalty = BindOrdered(config, 
            SecHunter, "Light Frame — Carry limit penalty", -0.20f,
            new ConfigDescription(
                "Redução do limite de carga (−0.20 = −20%). Valor NEGATIVO. / Carry-limit reduction (−0.20 = −20%). Negative.",
                new AcceptableValueRange<float>(-0.5f, 0f)));
        // Loud Looter / Saque Barulhento (Fuzileiro): som de interação/loot mais ALTO (a IA ouve mais — requer SAIN).
        LoudLooterEnabled = BindOrdered(config, 
            SecRifleman, "Loud Looter — Enabled", true,
            "Fuzileiro: som de interação/loot mais ALTO (a IA ouve mais; o canal de IA requer SAIN). / Rifleman: LOUDER interaction/loot sound (AI hears more; AI channel needs SAIN).");
        LoudLooterVolume = BindOrdered(config, 
            SecRifleman, "Loud Looter — Volume mult", 1.30f,
            new ConfigDescription(
                "Multiplicador do volume de interação/loot (1.30 = +30%). / Interaction/loot volume multiplier (1.30 = +30%).",
                new AcceptableValueRange<float>(1f, 2f)));
        // 080/087/088 — Saque Rápido (Caçador + Fuzileiro + Furtivo): acelera a TROCA para a arma do slot HOLSTER.
        // DOIS tempos independentes; a fase 2 (transição) encurta de brinde ao acelerar a fase 1. Seção do F12 vem
        // do 1º arg SecHunter (mesma config p/ as 3 classes).
        QuickDrawEnabled = BindOrdered(config, 
            SecHunter, "Quick Draw — Enabled", true,
            "Caçador/Fuzileiro/Furtivo: acelera a TROCA para a arma do coldre (guardar a anterior + sacar a do coldre). / Hunter/Rifleman/Stealth: faster SWAP to the Holster weapon (put-away + draw-in).");
        QuickDrawDrawInTime = BindOrdered(config, 
            SecHunter, "Quick Draw — Draw-in time mult (phase 3)", 0.65f,
            new ConfigDescription(
                "Fase 3 — TEMPO de SACAR a arma do coldre (trazê-la à mão). 0.65 = 35% mais rápido; 1.0 = desliga. / Phase 3 — time to DRAW the holster weapon (bring to hand). 0.65 = 35% faster; 1.0 = off.",
                new AcceptableValueRange<float>(0.3f, 1f)));
        QuickDrawPutAwayTime = BindOrdered(config, 
            SecHunter, "Quick Draw — Put-away time mult (phase 1)", 0.75f,
            new ConfigDescription(
                "Fase 1 — TEMPO de GUARDAR a arma anterior ao trocar para o coldre (a transição encurta junto). 0.75 = 25% mais rápido; 1.0 = desliga. / Phase 1 — time to PUT AWAY the previous weapon. 0.75 = 25% faster; 1.0 = off.",
                new AcceptableValueRange<float>(0.3f, 1f)));
        // 081 — Lebre (Saqueador): +velocidade de movimento enquanto NÃO estiver pesado (Overweight nativo == 0).
        LebreEnabled = BindOrdered(config, 
            SecScavenger, "Hare — Enabled", true,
            "Saqueador: +velocidade de movimento enquanto NÃO estiver pesado (sem o ícone de sobrepeso/bigorna). / Scavenger: +move speed while NOT overweight (no overweight icon).");
        LebreSpeed = BindOrdered(config, 
            SecScavenger, "Hare — Move speed mult", 1.30f,
            new ConfigDescription(
                "Multiplicador de velocidade quando leve (1.30 = +30%). Desliga automaticamente ao ficar pesado. / Move-speed multiplier while light (1.30 = +30%). Auto-off when overweight.",
                new AcceptableValueRange<float>(1f, 1.5f)));
        // 082 — Medroso (Saqueador): mãos trêmulas SOB FOGO (levar tiro OU bala passar perto). Porta a lógica do
        // mod UnderFire (o UnderFire global deve ser desativado — senão TODOS ganham o tremor, não só o Scav).
        MedrosoEnabled = BindOrdered(config, 
            SecScavenger, "Nervous — Enabled", true,
            "Saqueador: mãos trêmulas (tremor) ao levar tiro OU sob supressão (bala passa perto). / Scavenger: shaky hands (tremor) when shot OR suppressed (bullet fly-by).");
        MedrosoDuration = BindOrdered(config, 
            SecScavenger, "Nervous — Tremor duration (s)", 6f,
            new ConfigDescription("Duração do tremor (segundos). / Tremor duration (seconds).", new AcceptableValueRange<float>(1f, 20f)));
        MedrosoCooldown = BindOrdered(config, 
            SecScavenger, "Nervous — Cooldown (s)", 8f,
            new ConfigDescription("Espera antes de o tremor poder re-disparar. / Cooldown before the tremor can re-trigger.", new AcceptableValueRange<float>(0f, 30f)));
        MedrosoSuppressDistance = BindOrdered(config, 
            SecScavenger, "Nervous — Suppression distance (m)", 4f,
            new ConfigDescription("Distância (m) que a bala passa perto p/ contar como supressão (0 = só ao levar tiro). / Bullet fly-by distance (m) counting as suppression (0 = only when hit).", new AcceptableValueRange<float>(0f, 20f)));
        BindClassColor(config, SecScavenger, "Scavenger", "#c4ad45");   // 067

        // ───────────────────────── 7 · Tank ─────────────────────────
        BulwarkEnabled = BindOrdered(config, 
            SecTank, "Bulwark — Enabled", true,
            "Tanque: reduz o dano recebido na vida. / Tank: reduces incoming health damage.");
        BulwarkDamageTaken = BindOrdered(config, 
            SecTank, "Bulwark — Damage taken", 0.85f,
            new ConfigDescription(
                "Multiplicador do dano recebido (0.85 = −15%). / Incoming damage multiplier (0.85 = −15%).",
                new AcceptableValueRange<float>(0.5f, 1f)));
        // B6 (balance 2026-07-11): a Couraça era INCONDICIONAL (o Tanque levava −15% até pelado). Agora exige
        // estar de fato BLINDADO: armadura equipada de classe >= X. Temático, counterável, e casa com o
        // HeavyVests ×2 que a classe treina. Desligue o toggle p/ voltar ao comportamento incondicional.
        BulwarkRequireHeavyArmor = BindOrdered(config, 
            SecTank, "Bulwark — Require heavy armor", true,
            "Tanque: a Couraça só vale com armadura pesada equipada (sem ela, dano normal). / Tank: Bulwark only applies while wearing heavy armor.");
        BulwarkMinArmorClass = BindOrdered(config, 
            SecTank, "Bulwark — Min armor class", 4,
            new ConfigDescription(
                "Classe mínima da armadura equipada para a Couraça valer (4 = colete pesado). / Minimum equipped armor class for Bulwark to apply.",
                new AcceptableValueRange<int>(1, 6)));
        BunkerEnabled = BindOrdered(config, 
            SecTank, "Bunker — Enabled", true,
            "Tanque: com arma pesada (LMG/HMG/GL) na mão, menos recuo e mais ergonomia. / Tank: heavy weapons (LMG/HMG/GL) handle better.");
        BunkerHeavyRecoil = BindOrdered(config, 
            SecTank, "Bunker — Heavy weapon recoil mult", 0.7f,
            new ConfigDescription(
                "Multiplicador de recuo com arma pesada (0.7 = −30%). / Heavy-weapon recoil multiplier (0.7 = −30%).",
                new AcceptableValueRange<float>(0.5f, 1f)));
        BunkerHeavyErgo = BindOrdered(config, 
            SecTank, "Bunker — Heavy weapon ergo mult", 1.15f,
            new ConfigDescription(
                "Multiplicador de ergonomia com arma pesada (1.15 = +15%). / Heavy-weapon ergonomics multiplier (1.15 = +15%).",
                new AcceptableValueRange<float>(1f, 1.5f)));
        TirelessArmsEnabled = BindOrdered(config, 
            SecTank, "Tireless Arms — Enabled", true,
            "Tanque: braço cansa MUITO devagar segurando arma pesada (compõe com o stances mod; sem ele, inativo). / Tank: very slow arm fatigue holding heavy weapons (requires the stances mod).");
        // B16 (balance 2026-07-11): 0 → 0.2. Imunidade ABSOLUTA (×0) era outlier — o especialista em mira
        // (Caçador) tem ×0.65. Com 0.2 o braço cansa 5× mais devagar: preserva a fantasia sem imunidade.
        TirelessArmsDrain = BindOrdered(config, 
            SecTank, "Tireless Arms — Heavy arm drain mult", 0.5f,
            new ConfigDescription(
                "Multiplicador do dreno de braço do Tanque com arma pesada (0.5 = 2× mais lento; 0 = não drena). Requer o stances mod. / Tank heavy-weapon arm-drain multiplier (0.5 = 2x slower). Requires the stances mod.",
                new AcceptableValueRange<float>(0f, 1f)));
        HeavyFrameEnabled = BindOrdered(config, 
            SecTank, "Heavy Frame — Enabled", true,
            "Tanque: −velocidade de movimento (estrutura pesada). / Tank: slower movement (heavy frame).");
        HeavyFrameMoveSpeed = BindOrdered(config, 
            SecTank, "Heavy Frame — Move speed", 0.90f,
            new ConfigDescription(
                "Multiplicador de velocidade do Tanque (0.90 = −10%). / Tank move speed multiplier (0.90 = −10%).",
                new AcceptableValueRange<float>(0.5f, 1f)));
        HeavyFrameHungerThirst = BindOrdered(config, 
            SecTank, "Heavy Frame — Hunger/thirst drain", 1.15f,
            new ConfigDescription(
                "Multiplicador do dreno de fome/sede do Tanque (1.15 = +15% mais rápido). / Tank hunger/thirst drain multiplier (1.15 = +15% faster).",
                new AcceptableValueRange<float>(1f, 2f)));
        PackMuleTankEnabled = BindOrdered(config, 
            SecTank, "Pack Mule — Enabled", true,
            "Tanque: +limite de carga (piso, não soma com a Strength). / Tank: +carry limit (floor, does not stack with Strength).");
        PackMuleTankCarryBonus = BindOrdered(config, 
            SecTank, "Pack Mule — Carry limit bonus", 0.30f,
            new ConfigDescription(
                "Piso do bônus de limite de carga do Tanque (0.30 = +30%). / Tank carry-limit bonus floor (0.30 = +30%).",
                new AcceptableValueRange<float>(0f, 1f)));
        LoudOperatorTankEnabled = BindOrdered(config, 
            SecTank, "Loud Operator — Enabled", true,
            "Tanque: aumenta o raio de audibilidade dos seus sons de movimento. / Tank: increases the audibility radius of your movement sounds.");
        LoudOperatorTankSoundRadius = BindOrdered(config, 
            SecTank, "Loud Operator — Sound radius mult", 1.30f,
            new ConfigDescription(
                "Multiplicador do raio de som de movimento do Tanque (1.30 = +30%). / Tank movement-sound radius multiplier (1.30 = +30%).",
                new AcceptableValueRange<float>(1f, 2f)));
        // 084 — Recarga Rápida Escopeta (Tanque): acelera a recarga de escopetas de TUBO (shell-a-shell). A mecânica
        // elite "2 cartuchos por vez" (Mag Drills) NÃO existe no EFT — o fallback do épico é reduzir o TEMPO. Só
        // escopeta tubular (Weapon.SupportsInternalReload); Saiga com carregador destacável fica de fora.
        ShotgunReloadEnabled = BindOrdered(config, 
            SecTank, "Shotgun Reload — Enabled", true,
            "Tanque: recarrega escopetas de tubo (shell-a-shell) mais rápido. Não afeta escopetas com carregador destacável (Saiga). / Tank: faster tube-fed (shell-by-shell) shotgun reload. Does not affect detachable-magazine shotguns (Saiga).");
        ShotgunReloadTime = BindOrdered(config, 
            SecTank, "Shotgun Reload — Reload time mult", 0.6f,
            new ConfigDescription(
                "Multiplicador do TEMPO de recarga da escopeta (0.6 = 40% mais rápido). / Shotgun reload TIME multiplier (0.6 = 40% faster).",
                new AcceptableValueRange<float>(0.4f, 1f)));
        BindClassColor(config, SecTank, "Tank", "#6b7280");   // 067

        // ───────────────────────── 8 · Naked ─────────────────────────
        // 067: o Peladão não tem perks (classe raiz, sem buff), mas ganha seção própria para a COR — e, no
        // futuro, o texto de mérito (068). Sem isto ele não teria onde configurar a cor no F12.
        BindClassColor(config, SecNaked, "Naked", "#c28a60");   // 067

        // ───────────────────── 9 · Vanilla Skill Fixes ─────────────────────
        WeaponMasteryEnabled = BindOrdered(config, 
            SecVanillaFixes, "Weapon Mastery — Enabled", true,
            "Ativa as maestrias inertes: XP por disparo do underbarrel (GP-25/M203) + bônus por nível de SMG/LMG/Launcher/Underbarrel. / Enables inert weapon masteries: underbarrel XP per shot + per-level recoil/ergo bonuses.");
        MasteryXpPerShot = BindOrdered(config, 
            SecVanillaFixes, "Weapon Mastery — Underbarrel XP per shot", 0.5f,
            new ConfigDescription(
                "XP de Underbarrel Launchers por DISPARO do GP-25/M203 (0.5 = paridade de esforço com SMG). / Underbarrel Launchers XP per shot fired (0.5 = effort parity with SMG).",
                new AcceptableValueRange<float>(0f, 1f)));
        MasteryRecoilPerLevel = BindOrdered(config, 
            SecVanillaFixes, "Weapon Mastery — Recoil bonus per level", 0.004f,
            new ConfigDescription(
                "Redução de recuo por nível da maestria da arma na mão (0.004 = −0.4%/nível; paridade WeaponSkillRecoilBonusPerLevel). / Recoil reduction per mastery level of the held weapon (0.004 = −0.4%/level).",
                new AcceptableValueRange<float>(0f, 0.02f)));
        MasteryErgoPerLevel = BindOrdered(config, 
            SecVanillaFixes, "Weapon Mastery — Ergo bonus per level", 0.002f,
            new ConfigDescription(
                "Aumento de ergonomia por nível da maestria da arma na mão (0.002 = +0.2%/nível). / Ergonomics increase per mastery level of the held weapon (0.002 = +0.2%/level).",
                new AcceptableValueRange<float>(0f, 0.02f)));
    }

    /// <summary>
    ///     067 — binda o par de cor de uma classe (toggle 'Override color' + color picker) na seção dela e
    ///     registra em <see cref="ClassColors"/> pela chave <paramref name="classNameEn"/> (NameEn EN). O default
    ///     do picker = a cor ATUAL do server (<paramref name="serverHex"/>), então ligar o toggle preserva o
    ///     visual até o usuário mexer. Qualquer mudança dispara <see cref="ClassColorsChanged"/> (live no menu).
    /// </summary>
    private static void BindClassColor(ConfigFile config, string section, string classNameEn, string serverHex)
    {
        var ovr = BindOrdered(config, 
            section, "Override color", false,
            "Sobrescreve a cor do nome/ícone desta classe pela 'Class color' abaixo. Desligado (default) = usa a cor do server. / Override this class's name/icon color with 'Class color' below. Off (default) = use the server color.");
        var col = BindOrdered(config, 
            section, "Class color", Hex(serverHex),
            "Cor do nome/ícone da classe — só vale com 'Override color' ligado. O alpha é ignorado (a cor do nome é sempre opaca). / Class name/icon color — only applies when 'Override color' is on. Alpha is ignored (name color is always opaque).");

        ClassColors[classNameEn] = new ClassColorEntry(ovr, col);
        ovr.SettingChanged += (_, _) => ClassColorsChanged?.Invoke();
        col.SettingChanged += (_, _) => ClassColorsChanged?.Invoke();
    }

    /// <summary>067 — hex "#RRGGBB" → Color (fallback branco se malformado). Usado só p/ o DEFAULT do picker.</summary>
    private static Color Hex(string hex)
    {
        return ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.white;
    }

    /// <summary>
    ///     067 — o BepInEx NÃO registra um TomlTypeConverter para <see cref="Color"/> por default (só primitivos/
    ///     enums/string), então sem isto o Bind de um <c>ConfigEntry&lt;Color&gt;</c> lançaria ao serializar no
    ///     .cfg. Registra um converter #RRGGBBAA idempotente (guarda <c>CanConvert</c>: se o ConfigurationManager
    ///     ou outro mod já registrou, não duplica). O color picker nativo do CM vem do DRAWER (por tipo),
    ///     independente deste converter de disco.
    /// </summary>
    private static void EnsureColorConverter()
    {
        if (TomlTypeConverter.CanConvert(typeof(Color)))
        {
            return;
        }

        TomlTypeConverter.AddConverter(typeof(Color), new TypeConverter
        {
            ConvertToString = (obj, _) => "#" + ColorUtility.ToHtmlStringRGBA((Color)obj),
            ConvertToObject = (str, _) => ColorUtility.TryParseHtmlString(str, out var c) ? c : Color.white,
        });
    }
}
