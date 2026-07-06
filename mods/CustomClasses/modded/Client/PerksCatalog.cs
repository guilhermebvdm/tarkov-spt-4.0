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
        public float Multiplier = 1f;   // Percent/Multiplier (ignorado em Flag)
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
    private static PerkLine P(string tEn, string tPt, string en, string pt, ValueFormat fmt, float mult, Polarity pol, EBuffId icon = EBuffId.None, bool pending = false)
        => new() { TitleEn = tEn, TitlePt = tPt, LabelEn = en, LabelPt = pt, Format = fmt, Multiplier = mult, Polarity = pol, Icon = icon, Pending = pending };
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
            P("Rapid Care", "Cuidado Rápido", "heal/stab use time", "tempo de cura/estabilização", ValueFormat.Percent, 0.7f, Polarity.LowerBetter, EBuffId.VitalityBuffRegeneration, pending: true),
            P("Swift Surgeon", "Cirurgião Ágil", "surgery time", "tempo de cirurgia", ValueFormat.Percent, 0.5f, Polarity.LowerBetter, EBuffId.SurgerySpeed, pending: true),
            Flag("Mobile Surgery", "Cirurgia em Movimento", "surgery on the move", "cirurgia em movimento", isPerk: true, EBuffId.SurgeryReducePenalty, pending: true),
        }),
        ["shaky_hands"] = G("Shaky Hands", "Mãos Trêmulas", ESkillId.RecoilControl, new[]
        {
            P("Shaky Hands", "Mãos Trêmulas", "recoil", "recuo", ValueFormat.Multiplier, 1.25f, Polarity.LowerBetter, EBuffId.RecoilControlImprove),
        }),

        // 🔫 Fuzileiro
        ["cool_under_fire"] = G("Cool Under Fire", "Sangue-frio", ESkillId.StressResistance, new[]
        {
            P("Cool Under Fire", "Sangue-frio", "flinch when hit", "flinch ao levar dano", ValueFormat.Multiplier, 0.5f, Polarity.LowerBetter, EBuffId.AimMasterWiggle),
            P("Anti-Jam", "Antitravamento", "weapon jam chance", "chance de travamento", ValueFormat.Multiplier, 0.5f, Polarity.LowerBetter, EBuffId.TroubleFixing),
        }),
        ["adrenaline"] = G("Adrenaline", "Adrenalina", ESkillId.AimMaster, new[]
        {
            P("Adrenaline Grip", "Pegada de Adrenalina", "recoil (combat window)", "recuo (janela de combate)", ValueFormat.Multiplier, 0.7f, Polarity.LowerBetter, EBuffId.RecoilControlImprove),
            P("Adrenaline Reload", "Recarga de Adrenalina", "reload time (combat window)", "recarga (janela de combate)", ValueFormat.Multiplier, 0.8f, Polarity.LowerBetter, EBuffId.WeaponReloadBuff),
            P("Adrenaline Focus", "Foco de Adrenalina", "ADS time (combat window)", "ADS (janela de combate)", ValueFormat.Multiplier, 0.8f, Polarity.LowerBetter, EBuffId.AimMasterSpeed),
        }),
        ["loud_operator"] = G("Loud Operator", "Barulhento", ESkillId.SilentOps, new[]   // compartilhado Fuzileiro + Tanque (2026-07-05)
        {
            P("Loud Operator", "Barulhento", "noise", "ruído", ValueFormat.Percent, 1.3f, Polarity.LowerBetter, EBuffId.CovertMovementSoundVolume),
        }),

        // 🎯 Caçador
        ["sharpshooter"] = G("Sharpshooter", "Atirador", ESkillId.DrawMaster, new[]
        {
            P("Sharpshooter", "Atirador", "aim (ADS) time, all weapons", "mira (ADS), todas as armas", ValueFormat.Percent, 0.85f, Polarity.LowerBetter, EBuffId.AimMasterSpeed),
        }),
        ["iron_lungs"] = G("Iron Lungs", "Fôlego de Aço", ESkillId.Sniping, new[]
        {
            P("Iron Lungs", "Fôlego de Aço", "breath hold duration", "duração da respiração", ValueFormat.Percent, 1.5f, Polarity.HigherBetter, EBuffId.EnduranceBuffBreathTimeInc),
            P("Steady Arms", "Braços Firmes", "arm fatigue when aiming", "fadiga de braço ao mirar", ValueFormat.Percent, 0.65f, Polarity.LowerBetter, EBuffId.EnduranceHands),   // 051 ENTREGUE (hook no stances)
            P("Calm Sights", "Mira Serena", "sway", "oscilação (sway)", ValueFormat.Percent, 0.7f, Polarity.LowerBetter, EBuffId.AimMasterWiggle, pending: true),
        }),
        ["rooted"] = G("Rooted", "Enraizado", ESkillId.CovertMovement, new[]
        {
            P("Rooted", "Enraizado", "move speed while aiming", "velocidade ao mirar", ValueFormat.Percent, 0.85f, Polarity.HigherBetter, EBuffId.CovertMovementSpeed),
        }),

        // 👻 Furtivo
        ["ghost_step"] = G("Ghost Step", "Passo Fantasma", ESkillId.CovertMovement, new[]
        {
            P("Ghost Step", "Passo Fantasma", "all player noise", "todo o ruído do player", ValueFormat.Percent, 0.7f, Polarity.LowerBetter, EBuffId.CovertMovementSoundVolume),
        }),
        ["execution"] = G("Execution", "Execução", ESkillId.Melee, new[]
        {
            P("Execution", "Execução", "melee damage", "dano de melee", ValueFormat.Multiplier, 5f, Polarity.HigherBetter, EBuffId.StrengthBuffMeleePowerInc),
            P("Swift Blade", "Lâmina Veloz", "move speed with melee", "velocidade c/ melee na mão", ValueFormat.Percent, 1.1f, Polarity.HigherBetter, EBuffId.StrengthBuffSprintSpeedInc),
        }),
        ["rattled"] = G("Rattled", "Abalado", ESkillId.StressResistance, new[]
        {
            P("Rattled", "Abalado", "aim punch when hit", "tranco na mira ao ser atingido", ValueFormat.Percent, 1.5f, Polarity.LowerBetter, EBuffId.AimMasterWiggle),
        }),

        // 🎒 Saqueador
        ["quick_hands"] = G("Quick Hands", "Mãos Rápidas", ESkillId.Search, new[]
        {
            Flag("Quick Hands", "Mãos Rápidas", "search 2 items at once", "revista 2 itens de uma vez", isPerk: true, EBuffId.SearchDouble, pending: true),
        }),
        ["silent_looter"] = G("Silent Looter", "Saque Silencioso", ESkillId.SilentOps, new[]
        {
            Flag("Silent Looter", "Saque Silencioso", "silent looting", "saque silencioso", isPerk: true, EBuffId.CovertMovementSoundVolume),
        }),
        ["overladen"] = G("Overladen", "Sobrecarregado", ESkillId.Endurance, new[]
        {
            Flag("Overladen", "Sobrecarregado", "inertia scales with weight", "inércia escala com o peso", isPerk: false, EBuffId.StrengthBuffLiftWeightInc),
        }),

        // 🛡️ Tanque
        ["pack_mule"] = G("Pack Mule", "Mula de Carga", ESkillId.Strength, new[]   // compartilhado Saqueador + Tanque
        {
            P("Pack Mule", "Mula de Carga", "carry limit", "limite de carga", ValueFormat.Percent, 1.3f, Polarity.HigherBetter, EBuffId.StrengthBuffLiftWeightInc),
        }),
        ["bulwark"] = G("Bulwark", "Couraça", ESkillId.HeavyVests, new[]
        {
            P("Bulwark", "Couraça", "damage taken", "dano recebido", ValueFormat.Percent, 0.85f, Polarity.LowerBetter, EBuffId.HealthEliteAbsorbDamage),
        }, iconAlt: ESkillId.Vitality),
        ["bunker"] = G("Bunker", "Bunker", ESkillId.LMG, new[]
        {
            // fix in-game 2026-07-03: ergo → ícone de bipé (mais legível que WeaponErgonomicsBuff) e
            // GL → ícone de arremesso/granada (StrengthBuffThrowDistanceInc); sem sprite → fallback do grupo.
            P("Steady Mount", "Apoio Firme", "recoil (LMG/HMG/GL)", "recuo (LMG/HMG/GL)", ValueFormat.Multiplier, 0.85f, Polarity.LowerBetter, EBuffId.RecoilControlImprove),
            P("Heavy Handling", "Manejo Pesado", "ergonomics (LMG/HMG/GL)", "ergonomia (LMG/HMG/GL)", ValueFormat.Multiplier, 1.15f, Polarity.HigherBetter, EBuffId.BipodErgonomicsGainPerLevel),
            Flag("Grenadier", "Granadeiro", "GL: no ergo penalty", "lança-granadas: sem penalidade de ergo", isPerk: true, EBuffId.StrengthBuffThrowDistanceInc),
            Flag("Tireless Arms", "Braços Incansáveis", "no arm fatigue (heavy weapon)", "braço não cansa (arma pesada)", isPerk: true, EBuffId.EnduranceHands),   // 051 ENTREGUE (hook no stances)
        }, iconAlt: ESkillId.RecoilControl),
        ["heavy_frame"] = G("Heavy Frame", "Estrutura Pesada", ESkillId.Endurance, new[]
        {
            P("Heavy Frame", "Estrutura Pesada", "move speed", "velocidade", ValueFormat.Percent, 0.9f, Polarity.HigherBetter, EBuffId.StrengthBuffSprintSpeedInc),
            P("Heavy Appetite", "Apetite Pesado", "hunger/thirst drain", "fome/sede", ValueFormat.Percent, 1.3f, Polarity.LowerBetter, EBuffId.MetabolismEnergyExpenses),
        }),
    };

    // Composição por classe (chave EN estável = displayName.en). Ordem = ordem de exibição.
    private static readonly Dictionary<string, string[]> ByClass = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Combat Medic"] = new[] { "combat_medic", "shaky_hands" },
        ["Rifleman"]     = new[] { "cool_under_fire", "adrenaline", "loud_operator" },
        ["Hunter"]       = new[] { "sharpshooter", "iron_lungs", "rooted" },
        ["Stealth"]      = new[] { "ghost_step", "execution", "rattled" },
        ["Scavenger"]    = new[] { "quick_hands", "silent_looter", "pack_mule", "overladen" },
        ["Tank"]         = new[] { "pack_mule", "bulwark", "bunker", "heavy_frame", "loud_operator" },   // loud_operator: +ruído (2026-07-05, decisão do usuário)
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

            foreach (var l in g.Lines.Where(l => l.Format != ValueFormat.Flag && Mathf.Approximately(l.Multiplier, 1f)))
            {
                Plugin.Log?.LogWarning($"[CustomClasses][059] linha '{l.LabelEn}' em '{key}' tem Multiplier==1 (sem efeito → classificação inválida).");
            }
        }
    }
}
