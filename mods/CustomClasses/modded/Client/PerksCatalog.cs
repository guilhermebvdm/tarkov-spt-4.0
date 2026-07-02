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
        public string LabelEn = "", LabelPt = "";
        public ValueFormat Format;
        public float Multiplier = 1f;   // Percent/Multiplier (ignorado em Flag)
        public Polarity Polarity;       // classifica (Percent/Multiplier)
        public bool FlagIsPerk;         // Flag qualitativa: perk/drawback explícito
        public bool Pending;            // efeito deferido → "em breve" só nesta linha

        public bool IsPerk => Format == ValueFormat.Flag
            ? FlagIsPerk
            : (Polarity == Polarity.HigherBetter) == (Multiplier > 1f);
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

    // Fábricas de linha.
    private static PerkLine P(string en, string pt, ValueFormat fmt, float mult, Polarity pol, bool pending = false)
        => new() { LabelEn = en, LabelPt = pt, Format = fmt, Multiplier = mult, Polarity = pol, Pending = pending };
    private static PerkLine Flag(string en, string pt, bool isPerk, bool pending = false)
        => new() { LabelEn = en, LabelPt = pt, Format = ValueFormat.Flag, FlagIsPerk = isPerk, Pending = pending };
    private static PerkGroup G(string nameEn, string namePt, ESkillId? icon, PerkLine[] lines, ESkillId? iconAlt = null)
        => new() { NameEn = nameEn, NamePt = namePt, Icon = icon, IconAlt = iconAlt, Lines = lines };

    // Biblioteca: cada perk/drawback definido 1× por chave estável. Ícone = domínio (sprite via SkillIdSprites).
    private static readonly Dictionary<string, PerkGroup> Library = new(StringComparer.OrdinalIgnoreCase)
    {
        // 🩺 Médico
        ["combat_medic"] = G("Combat Medic", "Médico de Combate", ESkillId.Surgery, new[]
        {
            P("heal/stab use time", "tempo de cura/estabilização", ValueFormat.Percent, 0.7f, Polarity.LowerBetter, pending: true),
            P("surgery time", "tempo de cirurgia", ValueFormat.Percent, 0.5f, Polarity.LowerBetter, pending: true),
            Flag("surgery on the move", "cirurgia em movimento", isPerk: true, pending: true),
        }),
        ["shaky_hands"] = G("Shaky Hands", "Mãos Trêmulas", ESkillId.RecoilControl, new[]
        {
            P("recoil", "recuo", ValueFormat.Multiplier, 1.25f, Polarity.LowerBetter),
        }),

        // 🔫 Fuzileiro
        ["cool_under_fire"] = G("Cool Under Fire", "Sangue-frio", ESkillId.StressResistance, new[]
        {
            P("flinch when hit", "flinch ao levar dano", ValueFormat.Multiplier, 0.5f, Polarity.LowerBetter),
            P("weapon jam chance", "chance de travamento", ValueFormat.Multiplier, 0.5f, Polarity.LowerBetter),
        }),
        ["adrenaline"] = G("Adrenaline", "Adrenalina", ESkillId.AimMaster, new[]
        {
            P("recoil (combat window)", "recuo (janela de combate)", ValueFormat.Multiplier, 0.7f, Polarity.LowerBetter),
            P("reload time (combat window)", "recarga (janela de combate)", ValueFormat.Multiplier, 0.8f, Polarity.LowerBetter),
            P("ADS time (combat window)", "ADS (janela de combate)", ValueFormat.Multiplier, 0.8f, Polarity.LowerBetter),
        }),
        ["loud_operator"] = G("Loud Operator", "Barulhento", ESkillId.SilentOps, new[]
        {
            P("noise", "ruído", ValueFormat.Percent, 1.3f, Polarity.LowerBetter),
        }),

        // 🎯 Caçador
        ["sharpshooter"] = G("Sharpshooter", "Atirador", ESkillId.DrawMaster, new[]
        {
            P("aim (ADS) time, all weapons", "mira (ADS), todas as armas", ValueFormat.Percent, 0.85f, Polarity.LowerBetter),
        }),
        ["iron_lungs"] = G("Iron Lungs", "Fôlego de Aço", ESkillId.Sniping, new[]
        {
            P("breath hold duration", "duração da respiração", ValueFormat.Percent, 1.5f, Polarity.HigherBetter),
            P("arm fatigue when aiming", "fadiga de braço ao mirar", ValueFormat.Percent, 0.65f, Polarity.LowerBetter),
            P("sway", "oscilação (sway)", ValueFormat.Percent, 0.7f, Polarity.LowerBetter, pending: true),
        }),
        ["rooted"] = G("Rooted", "Enraizado", ESkillId.CovertMovement, new[]
        {
            P("move speed while aiming", "velocidade ao mirar", ValueFormat.Percent, 0.85f, Polarity.HigherBetter),
        }),

        // 👻 Furtivo
        ["ghost_step"] = G("Ghost Step", "Passo Fantasma", ESkillId.CovertMovement, new[]
        {
            P("all player noise", "todo o ruído do player", ValueFormat.Percent, 0.7f, Polarity.LowerBetter),
        }),
        ["execution"] = G("Execution", "Execução", ESkillId.Melee, new[]
        {
            P("melee damage", "dano de melee", ValueFormat.Multiplier, 5f, Polarity.HigherBetter),
            P("move speed with melee", "velocidade c/ melee na mão", ValueFormat.Percent, 1.1f, Polarity.HigherBetter),
        }),
        ["rattled"] = G("Rattled", "Abalado", ESkillId.StressResistance, new[]
        {
            P("aim punch when hit", "tranco na mira ao ser atingido", ValueFormat.Percent, 1.5f, Polarity.LowerBetter),
        }),

        // 🎒 Saqueador
        ["quick_hands"] = G("Quick Hands", "Mãos Rápidas", ESkillId.Search, new[]
        {
            Flag("search 2 items at once", "revista 2 itens de uma vez", isPerk: true, pending: true),
        }),
        ["silent_looter"] = G("Silent Looter", "Saque Silencioso", ESkillId.SilentOps, new[]
        {
            Flag("silent looting", "saque silencioso", isPerk: true),
        }),
        ["overladen"] = G("Overladen", "Sobrecarregado", ESkillId.Endurance, new[]
        {
            Flag("inertia scales with weight", "inércia escala com o peso", isPerk: false),
        }),

        // 🛡️ Tanque
        ["pack_mule"] = G("Pack Mule", "Mula de Carga", ESkillId.Strength, new[]   // compartilhado Saqueador + Tanque
        {
            P("carry limit", "limite de carga", ValueFormat.Percent, 1.3f, Polarity.HigherBetter),
        }),
        ["bulwark"] = G("Bulwark", "Couraça", ESkillId.HeavyVests, new[]
        {
            P("damage taken", "dano recebido", ValueFormat.Percent, 0.85f, Polarity.LowerBetter),
        }, iconAlt: ESkillId.Vitality),
        ["bunker"] = G("Bunker", "Bunker", ESkillId.LMG, new[]
        {
            P("recoil (LMG/HMG/GL)", "recuo (LMG/HMG/GL)", ValueFormat.Multiplier, 0.85f, Polarity.LowerBetter),
            P("ergonomics (LMG/HMG/GL)", "ergonomia (LMG/HMG/GL)", ValueFormat.Multiplier, 1.15f, Polarity.HigherBetter),
            Flag("GL: no ergo penalty", "lança-granadas: sem penalidade de ergo", isPerk: true),
            Flag("no arm fatigue (heavy weapon)", "braço não cansa (arma pesada)", isPerk: true),
        }, iconAlt: ESkillId.RecoilControl),
        ["heavy_frame"] = G("Heavy Frame", "Estrutura Pesada", ESkillId.Endurance, new[]
        {
            P("move speed", "velocidade", ValueFormat.Percent, 0.9f, Polarity.HigherBetter),
            P("hunger/thirst drain", "fome/sede", ValueFormat.Percent, 1.3f, Polarity.LowerBetter),
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
        ["Tank"]         = new[] { "pack_mule", "bulwark", "bunker", "heavy_frame" },
    };

    private static bool _validated;

    /// <summary>Grupos da classe local (via SkillMultipliers.ClassNameEn). Null se vanilla/desconhecida.</summary>
    internal static PerkGroup[]? LocalGroups()
    {
        ValidateOnce();
        var key = SkillMultipliers.ClassNameEn;
        if (key == null || !ByClass.TryGetValue(key, out var keys))
        {
            return null;
        }

        return keys.Select(k => Library.TryGetValue(k, out var g) ? g : null)
                   .Where(g => g != null)
                   .ToArray()!;
    }

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
    ///     Notificação de início de raid — COMPACTA: título + **uma linha por grupo** (nome colorido por IsPerk).
    ///     Sem linhas atômicas (o toast é pequeno). Deferidos entram normal aqui; o "em breve" fica só no painel.
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
            var hex = g.IsPerk ? MultiplierFormat.GreenHex : MultiplierFormat.RedHex;
            sb.Append("<color=").Append(hex).Append(">").Append(g.Name).Append("</color>\n");
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
