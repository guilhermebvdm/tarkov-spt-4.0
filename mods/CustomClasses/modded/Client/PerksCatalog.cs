using System.Collections.Generic;
using System.Text;

namespace CustomClasses.Client;

/// <summary>
///     Item 050/053 — catálogo bilíngue (EN/pt-br) dos perks 🔧 e drawbacks 🔻 por classe (chave estável = `name`).
///     Fonte: docs/class-design.md. Usado pela notificação de início de raid (RaidPerksNotificationPatch) e,
///     futuramente, pela aba "Perks/Drawback" (item 053). Texto resolvido pelo idioma do EFT (GameLocale).
/// </summary>
internal static class PerksCatalog
{
    internal sealed class Entry
    {
        public bool IsPerk;
        public string En = "";
        public string Pt = "";
        public string Text => GameLocale.IsPortuguese ? Pt : En;
    }

    private static Entry P(string en, string pt) => new() { IsPerk = true, En = en, Pt = pt };
    private static Entry D(string en, string pt) => new() { IsPerk = false, En = en, Pt = pt };

    // Chaveado pelo `name` estável (= displayName.en). Case-insensitive na consulta.
    private static readonly Dictionary<string, Entry[]> ByClass = new(System.StringComparer.OrdinalIgnoreCase)
    {
        ["Combat Medic"] = new[]
        {
            P("Combat Medic — faster meds & surgery, surgery on the move", "Médico de Combate — cura/cirurgia mais rápidas, cirurgia em movimento"),
            D("Shaky Hands — recoil ×1.25", "Mãos Trêmulas — recuo ×1.25"),
        },
        ["Rifleman"] = new[]
        {
            P("Cool Under Fire — less flinch when hit, anti-jam", "Sangue-frio — menos flinch ao levar dano, antitravamento"),
            P("Adrenaline — combat window: −recoil/−reload/−ADS", "Adrenalina — janela de combate: −recuo/−recarga/−ADS"),
            D("Loud Operator — +30% noise", "Barulhento — +30% de ruído"),
        },
        ["Hunter"] = new[]
        {
            P("Sharpshooter — fast pistol draw, faster sniper/DMR ADS", "Atirador — saque de pistola rápido, ADS sniper/DMR mais rápido"),
            P("Iron Lungs — longer breath hold, less sway & arm fatigue", "Fôlego de Aço — respiração longa, menos sway e fadiga de braço"),
            D("Rooted — −15% move speed while aiming", "Enraizado — −15% de velocidade enquanto mira"),
        },
        ["Stealth"] = new[]
        {
            P("Ghost Step — −30% all player noise", "Passo Fantasma — −30% de todo o ruído do player"),
            P("Execution — ×5 melee, +10% speed with melee", "Execução — melee ×5, +10% de velocidade c/ melee"),
            D("Rattled — +50% aim punch when hit", "Abalado — +50% de tranco na mira ao ser atingido"),
        },
        ["Scavenger"] = new[]
        {
            P("Quick Hands — search 2 items at once", "Mãos Rápidas — revista 2 itens de uma vez"),
            P("Silent Looter — quieter loot sounds (to your ears)", "Saque Silencioso — sons de saque mais baixos (no seu fone)"),
            P("Pack Mule — +30% carry limit", "Mula de Carga — +30% de limite de carga"),
            D("Overladen — inertia scales with weight", "Sobrecarregado — inércia escala com o peso"),
        },
        ["Tank"] = new[]
        {
            P("Pack Mule — +30% carry limit", "Mula de Carga — +30% de limite de carga"),
            P("Bulwark — −15% damage taken", "Couraça — −15% de dano recebido"),
            P("Bunker — heavy weapons (LMG/HMG/GL): −recoil, +ergo; GL no ergo penalty; no arm fatigue", "Bunker — armas pesadas (LMG/HMG/GL): −recuo, +ergo; lança-granadas sem penalidade de ergo; braço não cansa"),
            D("Heavy Frame — −10% speed, +30% hunger/thirst", "Estrutura Pesada — −10% velocidade, +30% fome/sede"),
        },
    };

    /// <summary>Entradas da classe local (resolvida via SkillMultipliers.ClassNameEn). Null se vanilla/desconhecida.</summary>
    internal static Entry[]? LocalEntries()
    {
        var key = SkillMultipliers.ClassNameEn;
        return key != null && ByClass.TryGetValue(key, out var e) ? e : null;
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
}
