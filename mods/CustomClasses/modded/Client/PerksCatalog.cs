using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFT;            // ESkillId
using UnityEngine;    // Sprite

namespace CustomClasses.Client;

/// <summary>Direção "boa" da propriedade: maior é melhor (speed/carry/ergo) vs menor é melhor (dano/recuo/fome/ruído).</summary>
internal enum Polarity { HigherBetter, LowerBetter }

/// <summary>Como exibir o valor de uma propriedade.</summary>
internal enum ValueFormat { Percent, Multiplier, Flag }

/// <summary>
///     Item 050/053/059 — catálogo bilíngue (EN/pt-br) das classes como **propriedades atômicas**. Cada perk/drawback
///     nomeado é um <see cref="PerkGroup"/>; cada efeito é um <see cref="PerkLine"/>. **Perk/drawback e o token de valor
///     são DERIVADOS** de `Multiplier` + `Polarity` + `Format` (nada escrito à mão). Biblioteca compartilhada por chave
///     (Pack Mule definido 1×). Fonte dos multiplicadores: docs/class-design.md + defaults do <see cref="PerksConfig"/>.
///     Consumido por: aba CLASS (059, cards em 2 colunas), notificação de raid (compacta) e PerkDiagnostics.
/// </summary>
internal static class PerksCatalog
{
    /// <summary>Propriedade atômica (1 variável). perk/drawback + token de valor DERIVADOS.</summary>
    internal sealed class PerkLine
    {
        public string TitleEn = "", TitlePt = "";   // fix in-game 2026-07-03: nome ÚNICO por efeito (card + notificação)
        public string LabelEn = "", LabelPt = "";
        public ValueFormat Format;
        private float _multiplier = 1f;
        /// <summary>
        ///     B4 (balance 2026-07-10): resolve o multiplicador VIVO do F12 (lazy — relido a cada acesso, como o
        ///     footer 060). Null = usa o valor do catálogo. Cada lambda embute o fallback (F12 não bindado) e a
        ///     transformação quando o card ≠ F12 (ex.: Iron Lungs duração = 1/dreno; Pack Mule = 1 + bônus).
        /// </summary>
        public Func<float>? Live;
        /// <summary>Percent/Multiplier (ignorado em Flag). B4: se <see cref="Live"/> != null, vem do F12 vivo.</summary>
        public float Multiplier { get => Live?.Invoke() ?? _multiplier; set => _multiplier = value; }
        public Polarity Polarity;       // classifica (Percent/Multiplier)
        public bool FlagIsPerk;         // Flag qualitativa: perk/drawback explícito
        public bool Pending;            // efeito deferido → "em breve" só nesta linha
        public EBuffId Icon;            // 059 CLASS#3: ícone de efeito da tela SKILLS (StaticIcons.BuffIdSprites)

        public bool IsPerk => Format == ValueFormat.Flag
            ? FlagIsPerk
            : (Polarity == Polarity.HigherBetter) == (Multiplier > 1f);
        public string Title => GameLocale.IsPortuguese ? TitlePt : TitleEn;
        public string Label => GameLocale.IsPortuguese ? LabelPt : LabelEn;
        public string ValueToken => MultiplierFormat.ValueToken(this);   // "+30%" / "×0.85" / ""
        public string Text => (ValueToken.Length > 0 ? ValueToken + " " : "") + Label;
    }

    /// <summary>Grupo nomeado (o "perk") = linhas atômicas HOMOGÊNEAS (todas perk OU todas drawback).</summary>
    internal sealed class PerkGroup
    {
        public string NameEn = "", NamePt = "";
        public ESkillId? Icon, IconAlt;
        public PerkLine[] Lines = Array.Empty<PerkLine>();

        public bool IsPerk => Lines.Length > 0 && Lines[0].IsPerk;   // seção (homogêneo)
        public string Name => GameLocale.IsPortuguese ? NamePt : NameEn;
        public bool AllPending => Lines.Length > 0 && Lines.All(l => l.Pending);
    }

    // Fábricas de linha. (tEn/tPt = título ÚNICO do efeito — fix in-game 2026-07-03)
    private static PerkLine P(string tEn, string tPt, string en, string pt, ValueFormat fmt, float mult, Polarity pol, EBuffId icon = EBuffId.None, bool pending = false, Func<float>? live = null)
        => new() { TitleEn = tEn, TitlePt = tPt, LabelEn = en, LabelPt = pt, Format = fmt, Multiplier = mult, Polarity = pol, Icon = icon, Pending = pending, Live = live };
    private static PerkLine Flag(string tEn, string tPt, string en, string pt, bool isPerk, EBuffId icon = EBuffId.None, bool pending = false)
        => new() { TitleEn = tEn, TitlePt = tPt, LabelEn = en, LabelPt = pt, Format = ValueFormat.Flag, FlagIsPerk = isPerk, Icon = icon, Pending = pending };
    private static PerkGroup G(string nameEn, string namePt, ESkillId? icon, PerkLine[] lines, ESkillId? iconAlt = null)
        => new() { NameEn = nameEn, NamePt = namePt, Icon = icon, IconAlt = iconAlt, Lines = lines };

    // Biblioteca: cada perk/drawback definido 1× por chave estável. Ícone = domínio (sprite via SkillIdSprites).
    private static readonly Dictionary<string, PerkGroup> Library = new(StringComparer.OrdinalIgnoreCase)
    {
        // 🩺 Médico
        ["combat_medic"] = G("Combat Medic", "Médico de Combate", ESkillId.Surgery, new[]
        {
            // 072 (2026-07-13): os 3 saíram do "em breve" — efeito E animação casados (ver ClassMedicPatches).
            P("Rapid Care", "Cuidado Rápido", "heal/stab use time", "tempo de cura/estabilização", ValueFormat.Percent, 0.7f, Polarity.LowerBetter, EBuffId.VitalityBuffRegeneration, live: () => PerksConfig.RapidCareUseTime?.Value ?? 0.7f),
            P("Swift Surgeon", "Cirurgião Ágil", "surgery time", "tempo de cirurgia", ValueFormat.Percent, 0.5f, Polarity.LowerBetter, EBuffId.SurgerySpeed, live: () => PerksConfig.SwiftSurgeonTime?.Value ?? 0.5f),
            // 079: "Cirurgia em Movimento" (Mobile Surgery) REMOVIDA do Médico.
            // 076 (2026-07-19): a cirurgia restaura o membro a ~80% do HP máximo (piso configurável), em vez da
            // cicatriz grande do vanilla. Vale na auto-cirurgia + aliado via ICM.
            Flag("Restorative Surgery", "Cirurgia Restauradora", "surgery restores ~80% limb max HP", "cirurgia restaura ~80% do HP máx do membro", isPerk: true, EBuffId.SurgeryReducePenalty),
        }),
        ["efficient_metabolism"] = G("Efficient Metabolism", "Metabolismo Eficiente", ESkillId.Metabolism, new[]   // B17 (2026-07-10): 1º perk vivo do Médico
        {
            P("Efficient Metabolism", "Metabolismo Eficiente", "hunger/thirst drain", "fome/sede", ValueFormat.Percent, 0.85f, Polarity.LowerBetter, EBuffId.MetabolismEnergyExpenses, live: () => PerksConfig.EfficientMetabolismHungerThirst?.Value ?? 0.85f),
        }),
        // 079: "Shaky Hands / Mãos Trêmulas" renomeado p/ "Unskilled / Falta de habilidade" — agora Médico E Saqueador.
        ["shaky_hands"] = G("Unskilled", "Falta de habilidade", ESkillId.RecoilControl, new[]
        {
            P("Unskilled", "Falta de habilidade", "recoil (lack of skill)", "recuo (falta de habilidade)", ValueFormat.Multiplier, 1.25f, Polarity.LowerBetter, EBuffId.RecoilControlImprove, live: () => PerksConfig.ShakyHandsRecoil?.Value ?? 1.25f),
        }),

        // 🔫 Fuzileiro
        ["cool_under_fire"] = G("Cool Under Fire", "Sangue-frio", ESkillId.StressResistance, new[]
        {
            P("Cool Under Fire", "Sangue-frio", "flinch when hit", "flinch ao levar dano", ValueFormat.Multiplier, 0.5f, Polarity.LowerBetter, EBuffId.AimMasterWiggle, live: () => PerksConfig.CoolUnderFireFlinch?.Value ?? 0.5f),
            P("Anti-Jam", "Antitravamento", "weapon jam chance", "chance de travamento", ValueFormat.Multiplier, 0.5f, Polarity.LowerBetter, EBuffId.TroubleFixing, live: () => PerksConfig.CoolUnderFireMalfChance?.Value ?? 0.5f),
        }),
        ["adrenaline"] = G("Adrenaline", "Adrenalina", ESkillId.AimMaster, new[]
        {
            P("Adrenaline Grip", "Pegada de Adrenalina", "recoil (combat window)", "recuo (janela de combate)", ValueFormat.Multiplier, 0.7f, Polarity.LowerBetter, EBuffId.RecoilControlImprove, live: () => PerksConfig.AdrenalineRecoil?.Value ?? 0.7f),
            P("Adrenaline Reload", "Recarga de Adrenalina", "reload time (combat window)", "recarga (janela de combate)", ValueFormat.Multiplier, 0.8f, Polarity.LowerBetter, EBuffId.WeaponReloadBuff, live: () => PerksConfig.AdrenalineReloadTime?.Value ?? 0.8f),
            P("Adrenaline Focus", "Foco de Adrenalina", "ADS time (combat window)", "ADS (janela de combate)", ValueFormat.Multiplier, 0.8f, Polarity.LowerBetter, EBuffId.AimMasterSpeed, live: () => PerksConfig.AdrenalineAdsTime?.Value ?? 0.8f),
        }),
        // Loud Operator desdobrado por classe (2026-07-10): cada card lê a config DA SUA classe.
        ["loud_operator_rifleman"] = G("Loud Operator", "Barulhento", ESkillId.SilentOps, new[]
        {
            P("Loud Operator", "Barulhento", "noise", "ruído", ValueFormat.Percent, 1.3f, Polarity.LowerBetter, EBuffId.CovertMovementSoundVolume, live: () => PerksConfig.LoudOperatorRiflemanSoundRadius?.Value ?? 1.3f),
        }),
        ["loud_operator_tank"] = G("Loud Operator", "Barulhento", ESkillId.SilentOps, new[]
        {
            P("Loud Operator", "Barulhento", "noise", "ruído", ValueFormat.Percent, 1.3f, Polarity.LowerBetter, EBuffId.CovertMovementSoundVolume, live: () => PerksConfig.LoudOperatorTankSoundRadius?.Value ?? 1.3f),
        }),

        // 🎯 Caçador
        ["sharpshooter"] = G("Sharpshooter", "Atirador", ESkillId.DrawMaster, new[]
        {
            P("Sharpshooter", "Atirador", "aim (ADS) time, all weapons", "mira (ADS), todas as armas", ValueFormat.Percent, 0.85f, Polarity.LowerBetter, EBuffId.AimMasterSpeed, live: () => PerksConfig.SharpshooterAdsTime?.Value ?? 0.85f),
        }),
        ["iron_lungs"] = G("Iron Lungs", "Fôlego de Aço", ESkillId.Sniping, new[]
        {
            // B4: o card mostra a DURAÇÃO (+X%), mas o F12 é o DRENO (×). duração = 1/dreno → 1/0.667 ≈ 1.5 (+50%).
            P("Iron Lungs", "Fôlego de Aço", "breath hold duration", "duração da respiração", ValueFormat.Percent, 1.5f, Polarity.HigherBetter, EBuffId.EnduranceBuffBreathTimeInc, live: () => 1f / (PerksConfig.IronLungsBreathDrain?.Value ?? 0.667f)),
            P("Steady Arms", "Braços Firmes", "arm fatigue when aiming", "fadiga de braço ao mirar", ValueFormat.Percent, 0.65f, Polarity.LowerBetter, EBuffId.EnduranceHands, live: () => PerksConfig.SteadyArmsDrain?.Value ?? 0.65f),   // 051 ENTREGUE (hook no stances)
            // 072: label diz "mira/movimento" DE PROPÓSITO — o SwayFactors não cobre o sway de RESPIRAÇÃO (outro
            // effector). O card mentiria se dissesse só "sway"; a respiração do Caçador é o Iron Lungs, logo acima.
            P("Calm Sights", "Mira Serena", "aim/movement sway", "oscilação de mira/movimento", ValueFormat.Percent, 0.7f, Polarity.LowerBetter, EBuffId.AimMasterWiggle, live: () => PerksConfig.CalmSightsSway?.Value ?? 0.7f),
        }),
        // Stalker (2026-07-11): irmão mais fraco do Ghost Step (−20% vs −30%) — o Furtivo segue dono da furtividade.
        ["stalker"] = G("Stalker", "Espreita", ESkillId.CovertMovement, new[]
        {
            P("Stalker", "Espreita", "movement/action noise", "ruído de movimento/ações", ValueFormat.Percent, 0.8f, Polarity.LowerBetter, EBuffId.CovertMovementSoundVolume, live: () => PerksConfig.StalkerSoundRadius?.Value ?? 0.8f),
        }),
        ["rooted"] = G("Rooted", "Enraizado", ESkillId.CovertMovement, new[]
        {
            P("Rooted", "Enraizado", "move speed while aiming", "velocidade ao mirar", ValueFormat.Percent, 0.85f, Polarity.HigherBetter, EBuffId.CovertMovementSpeed, live: () => PerksConfig.RootedAdsSpeed?.Value ?? 0.85f),
        }),

        // 👻 Furtivo
        ["ghost_step"] = G("Ghost Step", "Passo Fantasma", ESkillId.CovertMovement, new[]
        {
            // B2 (2026-07-10): label corrigido — os 3 pipelines cobrem movimento/ações; TIROS ficam de fora (o rótulo antigo mentia).
            P("Ghost Step", "Passo Fantasma", "movement/action noise", "ruído de movimento/ações", ValueFormat.Percent, 0.7f, Polarity.LowerBetter, EBuffId.CovertMovementSoundVolume, live: () => PerksConfig.GhostStepSoundRadius?.Value ?? 0.7f),
        }),
        ["execution"] = G("Execution", "Execução", ESkillId.Melee, new[]
        {
            P("Execution", "Execução", "melee damage", "dano de melee", ValueFormat.Multiplier, 3.5f, Polarity.HigherBetter, EBuffId.StrengthBuffMeleePowerInc, live: () => PerksConfig.ExecutionMeleeDamage?.Value ?? 3.5f),
            P("Swift Blade", "Lâmina Veloz", "move speed with melee", "velocidade c/ melee na mão", ValueFormat.Percent, 1.1f, Polarity.HigherBetter, EBuffId.StrengthBuffSprintSpeedInc, live: () => PerksConfig.ExecutionMoveSpeed?.Value ?? 1.1f),
        }),
        ["rattled"] = G("Rattled", "Abalado", ESkillId.StressResistance, new[]
        {
            P("Rattled", "Abalado", "aim punch when hit", "tranco na mira ao ser atingido", ValueFormat.Percent, 1.5f, Polarity.LowerBetter, EBuffId.AimMasterWiggle, live: () => PerksConfig.RattledAimPunch?.Value ?? 1.5f),
        }),

        // 🎒 Saqueador
        ["quick_hands"] = G("Quick Hands", "Mãos Rápidas", ESkillId.Search, new[]
        {
            // 061 ENTREGUE (2026-07-11): antecipa o bônus elite da Search (buff SearchDouble) — sai do "em breve".
            Flag("Quick Hands", "Mãos Rápidas", "search 2 containers at once", "revista 2 contêineres de uma vez", isPerk: true, EBuffId.SearchDouble),
        }),
        ["silent_looter"] = G("Silent Looter", "Saque Silencioso", ESkillId.SilentOps, new[]
        {
            Flag("Silent Looter", "Saque Silencioso", "silent looting", "saque silencioso", isPerk: true, EBuffId.CovertMovementSoundVolume),
        }),
        // 079: Overladen REMOVIDO (substituído pela Lebre, item 081). Grupos NOVOS do 079:
        // Light Frame (Caçador + Furtivo): carga reduzida — live = 1 + penalidade negativa (<1 → drawback).
        ["light_frame"] = G("Light Frame", "Estrutura Leve", ESkillId.Strength, new[]
        {
            P("Light Frame", "Estrutura Leve", "carry limit", "limite de carga", ValueFormat.Percent, 0.8f, Polarity.HigherBetter, EBuffId.StrengthBuffLiftWeightInc, live: () => 1f + (PerksConfig.LightFrameCarryPenalty?.Value ?? -0.2f)),
        }),
        // Loud Looter / Saque Barulhento (Fuzileiro): loot mais alto (>1 → drawback).
        ["loud_looter"] = G("Loud Looter", "Saque Barulhento", ESkillId.SilentOps, new[]
        {
            P("Loud Looter", "Saque Barulhento", "interaction/loot volume", "volume de interação/loot", ValueFormat.Percent, 1.3f, Polarity.LowerBetter, EBuffId.CovertMovementSoundVolume, live: () => PerksConfig.LoudLooterVolume?.Value ?? 1.3f),
        }),
        // 080 — Saque Rápido (Caçador + Fuzileiro + Furtivo): saque da arma do coldre (Holster) mais rápido.
        ["quick_draw"] = G("Quick Draw", "Saque Rápido", ESkillId.DrawMaster, new[]
        {
            P("Quick Draw", "Saque Rápido", "holster draw time", "tempo de saque do coldre", ValueFormat.Percent, 0.8f, Polarity.LowerBetter, EBuffId.AimMasterSpeed, live: () => PerksConfig.QuickDrawTime?.Value ?? 0.8f),
        }),
        // 081 — Lebre (Saqueador): +velocidade de movimento enquanto NÃO está pesado (overweight nativo == 0).
        ["lebre"] = G("Hare", "Lebre", ESkillId.Endurance, new[]
        {
            P("Hare", "Lebre", "move speed when light", "velocidade quando leve", ValueFormat.Percent, 1.3f, Polarity.HigherBetter, EBuffId.StrengthBuffSprintSpeedInc, live: () => PerksConfig.LebreSpeed?.Value ?? 1.3f),
        }),

        // 🛡️ Tanque
        // Pack Mule desdobrado por classe (2026-07-10): cada card lê a config DA SUA classe (B4: o F12 é o bônus cru → card = 1 + bônus).
        ["pack_mule_scav"] = G("Pack Mule", "Mula de Carga", ESkillId.Strength, new[]
        {
            P("Pack Mule", "Mula de Carga", "carry limit", "limite de carga", ValueFormat.Percent, 1.3f, Polarity.HigherBetter, EBuffId.StrengthBuffLiftWeightInc, live: () => 1f + (PerksConfig.PackMuleScavCarryBonus?.Value ?? 0.3f)),
        }),
        ["pack_mule_tank"] = G("Pack Mule", "Mula de Carga", ESkillId.Strength, new[]
        {
            P("Pack Mule", "Mula de Carga", "carry limit", "limite de carga", ValueFormat.Percent, 1.3f, Polarity.HigherBetter, EBuffId.StrengthBuffLiftWeightInc, live: () => 1f + (PerksConfig.PackMuleTankCarryBonus?.Value ?? 0.3f)),
        }),
        ["bulwark"] = G("Bulwark", "Couraça", ESkillId.HeavyVests, new[]
        {
            // B6 (2026-07-11): a Couraça virou CONDICIONAL — o label conta a condição, senão o jogador não
            // entende por que perdeu o perk ao tirar o colete.
            P("Bulwark", "Couraça", "damage taken (with heavy armor)", "dano recebido (com colete pesado)", ValueFormat.Percent, 0.85f, Polarity.LowerBetter, EBuffId.HealthEliteAbsorbDamage, live: () => PerksConfig.BulwarkDamageTaken?.Value ?? 0.85f),
        }, iconAlt: ESkillId.Vitality),
        ["bunker"] = G("Bunker", "Bunker", ESkillId.LMG, new[]
        {
            // fix in-game 2026-07-03: ergo → ícone de bipé (mais legível que WeaponErgonomicsBuff) e
            // GL → ícone de arremesso/granada (StrengthBuffThrowDistanceInc); sem sprite → fallback do grupo.
            P("Steady Mount", "Apoio Firme", "recoil (LMG/HMG/GL)", "recuo (LMG/HMG/GL)", ValueFormat.Multiplier, 0.85f, Polarity.LowerBetter, EBuffId.RecoilControlImprove, live: () => PerksConfig.BunkerHeavyRecoil?.Value ?? 0.85f),
            P("Heavy Handling", "Manejo Pesado", "ergonomics (LMG/HMG/GL)", "ergonomia (LMG/HMG/GL)", ValueFormat.Multiplier, 1.15f, Polarity.HigherBetter, EBuffId.BipodErgonomicsGainPerLevel, live: () => PerksConfig.BunkerHeavyErgo?.Value ?? 1.15f),
            Flag("Grenadier", "Granadeiro", "GL: no ergo penalty", "lança-granadas: sem penalidade de ergo", isPerk: true, EBuffId.StrengthBuffThrowDistanceInc),
            // B16 (2026-07-11): deixou de ser imunidade ABSOLUTA (×0) → agora é ×0.20 (cansa 5× mais devagar).
            // Por isso saiu de Flag ("braço não cansa" — passou a MENTIR) para linha com valor VIVO do F12.
            P("Tireless Arms", "Braços Incansáveis", "arm fatigue (heavy weapon)", "fadiga de braço (arma pesada)", ValueFormat.Multiplier, 0.20f, Polarity.LowerBetter, EBuffId.EnduranceHands, live: () => PerksConfig.TirelessArmsDrain?.Value ?? 0.2f),   // 051 (hook no stances)
        }, iconAlt: ESkillId.RecoilControl),
        ["heavy_frame"] = G("Heavy Frame", "Estrutura Pesada", ESkillId.Endurance, new[]
        {
            P("Heavy Frame", "Estrutura Pesada", "move speed", "velocidade", ValueFormat.Percent, 0.9f, Polarity.HigherBetter, EBuffId.StrengthBuffSprintSpeedInc, live: () => PerksConfig.HeavyFrameMoveSpeed?.Value ?? 0.9f),
            P("Heavy Appetite", "Apetite Pesado", "hunger/thirst drain", "fome/sede", ValueFormat.Percent, 1.3f, Polarity.LowerBetter, EBuffId.MetabolismEnergyExpenses, live: () => PerksConfig.HeavyFrameHungerThirst?.Value ?? 1.3f),
        }),
    };

    // Composição por classe (chave EN estável = displayName.en). Ordem = ordem de exibição.
    private static readonly Dictionary<string, string[]> ByClass = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Combat Medic"] = new[] { "combat_medic", "efficient_metabolism", "shaky_hands", "rattled" },   // 079: −Mobile Surgery, +Abalado
        ["Rifleman"]     = new[] { "cool_under_fire", "adrenaline", "loud_operator_rifleman", "loud_looter", "quick_draw" },   // 079 +Saque Barulhento · 080 +Saque Rápido
        ["Hunter"]       = new[] { "sharpshooter", "iron_lungs", "stalker", "rooted", "light_frame", "quick_draw" },   // 079 +Estrutura Leve · 080 +Saque Rápido
        ["Stealth"]      = new[] { "ghost_step", "execution", "rattled", "light_frame", "quick_draw" },   // 079 +Estrutura Leve · 080 +Saque Rápido
        ["Scavenger"]    = new[] { "quick_hands", "silent_looter", "pack_mule_scav", "lebre", "shaky_hands" },   // 079 −Overladen +Falta de habilidade · 081 +Lebre
        ["Tank"]         = new[] { "pack_mule_tank", "bulwark", "bunker", "heavy_frame", "loud_operator_tank" },   // Pack Mule + Loud Operator próprios (desdobrados 2026-07-10)
    };

    private static bool _validated;

    /// <summary>057 — grupos de QUALQUER classe pela chave EN estável (ByClass). Null se desconhecida.</summary>
    internal static PerkGroup[]? GroupsFor(string? classNameEn)
    {
        ValidateOnce();
        if (classNameEn == null || !ByClass.TryGetValue(classNameEn, out var keys))
        {
            return null;
        }

        return keys.Select(k => Library.TryGetValue(k, out var g) ? g : null)
                   .Where(g => g != null)
                   .ToArray()!;
    }

    /// <summary>Grupos da classe local (via SkillMultipliers.ClassNameEn). Null se vanilla/desconhecida.</summary>
    internal static PerkGroup[]? LocalGroups() => GroupsFor(SkillMultipliers.ClassNameEn);

    /// <summary>Sprite do ícone temático do grupo (mesmo dicionário da tela de Skills). Icon → IconAlt → null.</summary>
    internal static Sprite? IconSprite(PerkGroup g)
    {
        try
        {
            var dict = EFTHardSettings.Instance?.StaticIcons?.SkillIdSprites;
            if (dict == null)
            {
                return null;
            }

            var sprite = g.Icon != null ? dict.GetValueOrDefault(g.Icon.Value) : null;
            return sprite == null && g.IconAlt != null ? dict.GetValueOrDefault(g.IconAlt.Value) : sprite;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     059 CLASS#3 — sprite do EFEITO atômico: o MESMO ícone dos quadradinhos de buff da tela de Skills
    ///     (ref: EFT.UI.BuffIcon.smethod_0 → StaticIcons.BuffIdSprites[EBuffId]). Null se a linha não tem
    ///     mapeamento (caller cai no ícone do grupo via <see cref="IconSprite"/>).
    /// </summary>
    internal static Sprite? BuffSprite(PerkLine line)
    {
        try
        {
            return line.Icon == EBuffId.None
                ? null
                : EFTHardSettings.Instance?.StaticIcons?.BuffIdSprites?.GetValueOrDefault(line.Icon);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Notificação de início de raid — fix in-game 2026-07-03: **uma linha por EFEITO** no mesmo vocabulário
    ///     da aba CLASS (título único colorido por IsPerk + token + label esmaecidos). Deferidos entram normal
    ///     aqui; o "em breve" fica só no painel.
    /// </summary>
    internal static string? BuildNotificationText()
    {
        var groups = LocalGroups();
        if (groups == null || groups.Length == 0)
        {
            return null;
        }

        var sb = new StringBuilder();
        var header = SkillMultipliers.ClassName;
        if (!string.IsNullOrEmpty(header))
        {
            sb.Append("<b>").Append(header).Append("</b>\n");
        }

        foreach (var g in groups)
        {
            foreach (var line in g.Lines)
            {
                var hex = line.IsPerk ? MultiplierFormat.GreenHex : MultiplierFormat.RedHex;
                sb.Append("<color=").Append(hex).Append("><b>").Append(line.Title).Append("</b></color>");
                sb.Append(" <color=#c9c9c9>").Append(line.Text).Append("</color>\n");
            }
        }

        return sb.ToString().TrimEnd('\n');
    }

    /// <summary>Invariantes (1×): grupo homogêneo + linha quantitativa com Multiplier ≠ 1. Só loga aviso.</summary>
    private static void ValidateOnce()
    {
        if (_validated)
        {
            return;
        }

        _validated = true;
        foreach (var kv in Library)
        {
            var key = kv.Key;
            var g = kv.Value;
            if (g.Lines.Length > 0 && !g.Lines.All(l => l.IsPerk == g.Lines[0].IsPerk))
            {
                Plugin.Log?.LogWarning($"[CustomClasses][059] grupo '{key}' NÃO é homogêneo (perk/drawback misturados na coluna).");
            }

            // `l.Live is null` (code-review 2026-07-11): esta invariante vale para os valores HARDCODED do
            // catálogo. Linhas com `Live` resolvem o F12 do USUÁRIO — e neutralizar um perk lá (ex.: Tireless
            // Arms em 1.0, dentro do range válido) é escolha legítima dele, não erro de catálogo.
            foreach (var l in g.Lines.Where(l => l.Format != ValueFormat.Flag && l.Live is null && Mathf.Approximately(l.Multiplier, 1f)))
            {
                Plugin.Log?.LogWarning($"[CustomClasses][059] linha '{l.LabelEn}' em '{key}' tem Multiplier==1 (sem efeito → classificação inválida).");
            }
        }
    }
}
