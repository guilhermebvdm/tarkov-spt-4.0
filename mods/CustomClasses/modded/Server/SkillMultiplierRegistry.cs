using SPTarkov.DI.Annotations;

namespace CustomClasses;

/// <summary>
///     Item 005: fatores de ganho de XP de skill por classe (edition) → (skill → fator).
///     Preenchido pelo loader (CustomClassesMod) e lido pelo SkillMultipliersRouter.
///     SINGLETON: precisa ser a MESMA instância vista pelo loader e pelo router
///     (o default do [Injectable] é Scoped — ref: SPTarkov.DI/Annotations/Injectable.cs:7).
/// </summary>
[Injectable(InjectionType.Singleton)]
public class SkillMultiplierRegistry
{
    private readonly Dictionary<string, Dictionary<string, double>> _byEdition = new(StringComparer.Ordinal);

    /// <summary>Grava (ou substitui) os fatores de uma classe/edition.</summary>
    public void Set(string edition, Dictionary<string, double> multipliers)
    {
        _byEdition[edition] = multipliers;
    }

    /// <summary>Fatores da edition (vazio se não for classe do mod).</summary>
    public Dictionary<string, double> Get(string edition)
    {
        return _byEdition.TryGetValue(edition, out var m) ? m : new Dictionary<string, double>();
    }
}
