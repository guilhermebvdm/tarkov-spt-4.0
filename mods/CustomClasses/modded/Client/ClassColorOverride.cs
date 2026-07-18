using UnityEngine;

namespace CustomClasses.Client;

/// <summary>
///     067 — resolver da COR de cada classe: <b>override do F12 (por classe) → fallback da cor do server</b>.
///     Espelha a filosofia do <see cref="PerkLine"/><c>.Live</c> (B4): a "sentinela" é o toggle
///     <c>Override color</c> de cada classe (default OFF → usa a cor do server, que segue sendo a fonte de
///     verdade — <c>ClassVisualRegistry</c> → rotas → hex). Ligar sobrescreve SÓ aquela classe.
///     <para>
///     Chaveado por <b>NameEn EN</b> ("Combat Medic" … "Tank"/"Naked") — bate com
///     <see cref="SkillMultipliers.ClassNameEn"/> e <see cref="ClassIdentities.Identity"/><c>.NameEn</c>,
///     ambos preenchidos pelo server com <c>displayName.en</c> (nunca a edition key). Resolvido NO ACESSADOR
///     de <c>NameColor</c> (em <see cref="SkillMultipliers"/> e na <c>Identity</c>), então TODOS os
///     consumidores — chat, menu (+ AccentColor do Menu-Overhaul), loading, party, tooltips,
///     <see cref="PerksPanelView"/> — pegam o override de graça, inclusive os gates de string CRUA que não
///     passam pelo <c>ClassIdentityView.ResolveColor</c>.
///     </para>
///     Lazy (relido a cada acesso; sem cache) — a mudança no F12 vale na hora (o menu re-aplica via
///     <c>PerksConfig.ClassColorsChanged</c>; as outras superfícies pegam no próximo render).
/// </summary>
internal static class ClassColorOverride
{
    /// <summary>
    ///     Hex (#RRGGBB, sempre OPACO) do override ATIVO da classe, ou <c>null</c> para usar a cor do server.
    ///     Null quando: sem classe, classe sem entrada no F12, ou toggle <c>Override color</c> desligado.
    /// </summary>
    internal static string? Resolve(string? classNameEn)
    {
        if (string.IsNullOrEmpty(classNameEn)
            || !PerksConfig.ClassColors.TryGetValue(classNameEn!, out var entry)
            || !entry.Override.Value)
        {
            return null;
        }

        // ToHtmlStringRGB descarta o alpha → cor de nome sempre opaca (o alpha do picker é irrelevante aqui;
        // o toggle é quem liga/desliga). Casa com o resto do pipeline, que lê hex #RRGGBB do server.
        return "#" + ColorUtility.ToHtmlStringRGB(entry.Color.Value);
    }
}
