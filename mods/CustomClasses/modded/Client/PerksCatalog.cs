using System.Collections.Generic;
using System.Text;
using EFT;            // ESkillId
using UnityEngine;    // Sprite

namespace CustomClasses.Client;

/// <summary>
///     Item 050/053 — catálogo bilíngue (EN/pt-br) dos perks 🔧 e drawbacks 🔻 por classe (chave estável = `name`).
///     Fonte: docs/class-design.md. Usado pela notificação de início de raid (RaidPerksNotificationPatch) e pela
///     aba "CLASS" (item 053, lista de cards). Cada entrada carrega um <see cref="ESkillId"/> temático (domínio) cujo
///     sprite é reusado do próprio EFT (mesmo ícone que aparece sobre a barra de progresso da skill). Idioma via GameLocale.
/// </summary>
internal static class PerksCatalog
{
    internal sealed class Entry
    {
        public bool IsPerk;
        public string En = "";
        public string Pt = "";
        public ESkillId? Icon;      // ícone temático (domínio); sprite via SkillIdSprites. null = sem ícone.
        public ESkillId? IconAlt;   // CR-01-05: fallback quando Icon não tem sprite no dicionário (ex.: LMG/HeavyVests).
        public bool Pending;        // CR-02-01/02: efeito ainda DEFERIDO (não implementado in-game) → UI marca "em breve".
        public string Text => GameLocale.IsPortuguese ? Pt : En;
        public string Name => SplitNameEffect(Text).name;      // parte antes do " — "
        public string Effect => SplitNameEffect(Text).effect;  // parte depois do " — "
    }

    private static Entry P(string en, string pt, ESkillId? icon = null, ESkillId? iconAlt = null, bool pending = false) => new() { IsPerk = true, En = en, Pt = pt, Icon = icon, IconAlt = iconAlt, Pending = pending };
    private static Entry D(string en, string pt, ESkillId? icon = null, ESkillId? iconAlt = null, bool pending = false) => new() { IsPerk = false, En = en, Pt = pt, Icon = icon, IconAlt = iconAlt, Pending = pending };

    // Chaveado pelo `name` estável (= displayName.en). Case-insensitive na consulta.
    // Ícone = DOMÍNIO do efeito (ex.: recuo→RecoilControl, som→SilentOps); a cor perk/drawback dá a valência.
    private static readonly Dictionary<string, Entry[]> ByClass = new(System.StringComparer.OrdinalIgnoreCase)
    {
        ["Combat Medic"] = new[]
        {
            P("Combat Medic — faster meds & surgery, surgery on the move", "Médico de Combate — cura/cirurgia mais rápidas, cirurgia em movimento", ESkillId.Surgery, pending: true),   // CR-02-01: deferido
            D("Shaky Hands — recoil ×1.25", "Mãos Trêmulas — recuo ×1.25", ESkillId.RecoilControl),
        },
        ["Rifleman"] = new[]
        {
            P("Cool Under Fire — less flinch when hit, anti-jam", "Sangue-frio — menos flinch ao levar dano, antitravamento", ESkillId.StressResistance),
            P("Adrenaline — combat window: −recoil/−reload/−ADS", "Adrenalina — janela de combate: −recuo/−recarga/−ADS", ESkillId.AimMaster),
            D("Loud Operator — +30% noise", "Barulhento — +30% de ruído", ESkillId.SilentOps),
        },
        ["Hunter"] = new[]
        {
            P("Sharpshooter — faster aim (ADS) on all weapons", "Atirador — mira (ADS) mais rápida em todas as armas", ESkillId.DrawMaster),   // CR-02-04: impl = ADS ×0.85 sempre (sem saque de pistola / penalidade de AR)
            P("Iron Lungs — longer breath hold & less arm fatigue", "Fôlego de Aço — respiração longa e menos fadiga de braço", ESkillId.Sniping),   // CR-02-03: "less sway" removido (deferido)
            D("Rooted — −15% move speed while aiming", "Enraizado — −15% de velocidade enquanto mira", ESkillId.CovertMovement),
        },
        ["Stealth"] = new[]
        {
            P("Ghost Step — −30% all player noise", "Passo Fantasma — −30% de todo o ruído do player", ESkillId.CovertMovement),
            P("Execution — ×5 melee, +10% speed with melee", "Execução — melee ×5, +10% de velocidade c/ melee", ESkillId.Melee),
            D("Rattled — +50% aim punch when hit", "Abalado — +50% de tranco na mira ao ser atingido", ESkillId.StressResistance),
        },
        ["Scavenger"] = new[]
        {
            P("Quick Hands — search 2 items at once", "Mãos Rápidas — revista 2 itens de uma vez", ESkillId.Search, pending: true),   // CR-02-02: deferido (server-side)
            P("Silent Looter — quieter looting (bots hear it less + your ears)", "Saque Silencioso — saque mais silencioso (bots ouvem menos + no seu fone)", ESkillId.SilentOps),
            P("Pack Mule — +30% carry limit", "Mula de Carga — +30% de limite de carga", ESkillId.Strength),
            D("Overladen — inertia scales with weight", "Sobrecarregado — inércia escala com o peso", ESkillId.Endurance),
        },
        ["Tank"] = new[]
        {
            P("Pack Mule — +30% carry limit", "Mula de Carga — +30% de limite de carga", ESkillId.Strength),
            P("Bulwark — −15% damage taken", "Couraça — −15% de dano recebido", ESkillId.HeavyVests, ESkillId.Vitality),
            P("Bunker — heavy weapons (LMG/HMG/GL): −recoil, +ergo; GL no ergo penalty; no arm fatigue", "Bunker — armas pesadas (LMG/HMG/GL): −recuo, +ergo; lança-granadas sem penalidade de ergo; braço não cansa", ESkillId.LMG, ESkillId.RecoilControl),
            D("Heavy Frame — −10% speed, +30% hunger/thirst", "Estrutura Pesada — −10% velocidade, +30% fome/sede", ESkillId.Endurance),
        },
    };

    /// <summary>Entradas da classe local (resolvida via SkillMultipliers.ClassNameEn). Null se vanilla/desconhecida.</summary>
    internal static Entry[]? LocalEntries()
    {
        var key = SkillMultipliers.ClassNameEn;
        return key != null && ByClass.TryGetValue(key, out var e) ? e : null;
    }

    /// <summary>Sprite do ícone temático da entrada (mesmo dicionário que a tela de Skills usa). Null se sem ícone/ausente.</summary>
    internal static Sprite? IconSprite(Entry e)
    {
        try
        {
            var dict = EFTHardSettings.Instance?.StaticIcons?.SkillIdSprites;
            if (dict == null)
            {
                return null;
            }

            var sprite = e.Icon != null ? dict.GetValueOrDefault(e.Icon.Value) : null;
            if (sprite == null && e.IconAlt != null)
            {
                sprite = dict.GetValueOrDefault(e.IconAlt.Value);   // CR-01-05: fallback quando o ícone primário não tem sprite
            }

            return sprite;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Texto rich-text multilinha p/ a notificação: perks em verde, drawbacks em vermelho.
    ///     Null se a classe não tem entradas. Cores reusam o MultiplierFormat (mesmo padrão da tela de Skills).
    /// </summary>
    internal static string? BuildNotificationText()
    {
        var entries = LocalEntries();
        if (entries == null || entries.Length == 0)
        {
            return null;
        }

        var sb = new StringBuilder();
        var header = SkillMultipliers.ClassName;
        if (!string.IsNullOrEmpty(header))
        {
            sb.Append("<b>").Append(header).Append("</b>\n");
        }

        foreach (var e in entries)
        {
            var hex = e.IsPerk ? MultiplierFormat.GreenHex : MultiplierFormat.RedHex;
            sb.Append("<color=").Append(hex).Append(">").Append(e.Text).Append("</color>\n");
        }

        return sb.ToString().TrimEnd('\n');
    }

    /// <summary>
    ///     Fallback (overlay legado): texto rich-text do painel — título + lista com ▲/▼. Null se sem entradas.
    ///     A aba CLASS (053) usa cards em vez deste texto, mas o overlay desabilitado ainda referencia isto.
    /// </summary>
    internal static string? BuildPanelText()
    {
        var entries = LocalEntries();
        if (entries == null || entries.Length == 0)
        {
            return null;
        }

        var sb = new StringBuilder();
        var header = SkillMultipliers.ClassName;
        var classHex = string.IsNullOrWhiteSpace(SkillMultipliers.NameColor) ? "#ffffff" : SkillMultipliers.NameColor;
        var title = GameLocale.IsPortuguese ? "Perks e Drawbacks" : "Perks & Drawbacks";
        if (!string.IsNullOrEmpty(header))
        {
            sb.Append("<size=150%><b><color=").Append(classHex).Append(">").Append(header.ToUpperInvariant())
                .Append("</color></b></size>   <color=#7a7a7a><i>").Append(title).Append("</i></size>\n\n");
        }

        foreach (var e in entries)
        {
            var mark = e.IsPerk
                ? "<color=" + MultiplierFormat.GreenHex + ">▲</color>"
                : "<color=" + MultiplierFormat.RedHex + ">▼</color>";
            sb.Append(mark).Append("  <b>").Append(e.Name).Append("</b>");
            if (!string.IsNullOrEmpty(e.Effect))
            {
                sb.Append("   <color=#a8a8a8>").Append(e.Effect).Append("</color>");
            }

            sb.Append('\n');
        }

        return sb.ToString().TrimEnd('\n');
    }

    /// <summary>Separa "Nome — efeito" (em-dash) em (nome, efeito). Sem em-dash → (texto, "").</summary>
    private static (string name, string effect) SplitNameEffect(string text)
    {
        var idx = text.IndexOf(" — ", System.StringComparison.Ordinal);   // " — " (em-dash)
        return idx >= 0 ? (text.Substring(0, idx), text.Substring(idx + 3)) : (text, string.Empty);
    }
}
