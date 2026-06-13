using SPTarkov.DI.Annotations;

namespace CustomClasses;

/// <summary>
///     Item 011: identidade visual por classe (edition → ícone/cor). Registra TODA classe do mod,
///     mesmo sem ícone/cor — serve de fonte de "esta edition é classe do mod?" para o router
///     (assim uma classe sem skillMultipliers ainda expõe nome/ícone/cor ao client).
///     SINGLETON: mesma instância vista pelo loader (CustomClassesMod) e pelo router.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class ClassVisualRegistry
{
    // item 008 (i18n): nome localizado da classe (en/pt) — exposto ao client, que resolve pelo idioma do EFT.
    public sealed record Visual(string? IconFile, string? NameColor, string? DisplayNameEn, string? DisplayNamePt);

    private readonly Dictionary<string, Visual> _byEdition = new(StringComparer.Ordinal);

    /// <summary>Grava (ou substitui) a identidade visual de uma classe/edition.</summary>
    public void Set(string edition, string? iconFile, string? nameColor, string? displayNameEn = null, string? displayNamePt = null)
    {
        _byEdition[edition] = new Visual(iconFile, nameColor, displayNameEn, displayNamePt);
    }

    /// <summary>True se a edition é uma classe registrada pelo mod.</summary>
    public bool Contains(string edition) => _byEdition.ContainsKey(edition);

    /// <summary>Identidade visual da edition (null se não for classe do mod).</summary>
    public Visual? Get(string edition) => _byEdition.TryGetValue(edition, out var v) ? v : null;

    /// <summary>Item 021: remove a edition do registry (hot-remove do editor). True se existia.</summary>
    public bool Remove(string edition) => _byEdition.Remove(edition);

    /// <summary>Item 021: editions registradas pelo mod (enumeração p/ o editor).</summary>
    public IReadOnlyCollection<string> Editions => _byEdition.Keys;
}
