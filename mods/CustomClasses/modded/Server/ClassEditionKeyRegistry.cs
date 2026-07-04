using SPTarkov.DI.Annotations;

namespace CustomClasses;

/// <summary>
///     Item 058: mapa <c>fileName</c> (config/classes/*.json[c]) → chave de edition EFETIVAMENTE
///     registrada no ProfileTemplates. Gravado em <see cref="ClassRegistrar.Commit"/> — a única fonte
///     da chave real: com <c>settings.jsonc language=pt/en</c> o pipeline re-chaveia a edition para
///     <c>displayName[lang]</c> (<see cref="LauncherLanguageConfig.ResolveEditionKey"/>, aplicada em
///     ValidateAndBuild — ref: CR-01-01), então <c>def.name</c> NÃO é confiável como chave.
///     Limpo em <see cref="ClassRegistrar.Remove"/>.
///     Consumido pelo <see cref="ClassListRouter"/> para expor <c>editionKey</c> sem re-derivar a língua.
///     <para>
///     SINGLETON + concorrência: mesmo padrão do <see cref="ClassVisualRegistry"/> (Dictionary simples;
///     escrita no boot/hot-apply, leitura em threads HTTP — races de hot-apply aceitas, server local
///     single-user, premissa do item 021).
///     </para>
/// </summary>
[Injectable(InjectionType.Singleton)]
public class ClassEditionKeyRegistry
{
    // fileName em OrdinalIgnoreCase — mesma convenção do entry-cache do ClassEditorService (item 037).
    private readonly Dictionary<string, string> _editionByFile = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Grava (ou substitui) a chave de edition registrada a partir de um arquivo de classe.</summary>
    public void Set(string fileName, string editionKey)
    {
        _editionByFile[fileName] = editionKey;
    }

    /// <summary>Chave registrada a partir do arquivo — null se nunca comitou (skip no boot) ou foi removida.</summary>
    public string? GetEditionKey(string fileName)
    {
        return _editionByFile.TryGetValue(fileName, out var key) ? key : null;
    }

    /// <summary>Remove todo mapeamento apontando para a edition (hot-remove do editor). Chave em Ordinal —
    /// mesma comparação do ProfileTemplates/ClassVisualRegistry.</summary>
    public void RemoveByEdition(string editionKey)
    {
        foreach (var file in _editionByFile
                     .Where(kv => string.Equals(kv.Value, editionKey, StringComparison.Ordinal))
                     .Select(kv => kv.Key)
                     .ToList())
        {
            _editionByFile.Remove(file);
        }
    }
}
