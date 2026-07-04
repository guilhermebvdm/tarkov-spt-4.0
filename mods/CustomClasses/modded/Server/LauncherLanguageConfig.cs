using System.Reflection;
using System.Text.Json.Serialization;                   // JsonPropertyName (settings.jsonc)
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;                     // ModHelper
using SPTarkov.Server.Core.Models.Utils;                // ISptLogger
using SPTarkov.Server.Core.Utils;                       // FileUtil, JsonUtil

namespace CustomClasses;

/// <summary>
///     Item 058 (ref: CR-01-01, fix estrutural): resolução da LÍNGUA da chave de edition, extraída de
///     <c>CustomClassesMod</c> (onde era privada e só o boot aplicava) para um singleton compartilhado.
///     Consumida por <see cref="ClassRegistrar.ValidateAndBuild"/> — assim boot, Save/hotApply e Delete
///     produzem a MESMA editionKey em qualquer <c>language</c> (antes, um Save+hotApply sob
///     <c>language=pt</c> registrava a edition EN transitória e perfis criados nessa janela apontavam
///     para edition fantasma após restart).
///     <para>
///     <c>config/settings.jsonc → language</c>: "pt"/"en" → chave = <c>displayName[lang]</c> (fallback
///     <c>name</c>); "name"/ausente/inválido → <c>name</c> cru. Carregado UMA vez (lazy; trocar o valor
///     exige restart — comportamento documentado no próprio settings.jsonc). O transform é preocupação
///     de REGISTRO: o <c>name</c> no arquivo de classe nunca é reescrito.
///     </para>
/// </summary>
[Injectable(InjectionType.Singleton)]
public class LauncherLanguageConfig(
    ModHelper modHelper,
    FileUtil fileUtil,
    JsonUtil jsonUtil,
    ISptLogger<LauncherLanguageConfig> logger
)
{
    private string? _language;

    /// <summary>"pt" | "en" | "name" (default/fallback). Lazy — primeira leitura carrega o settings.jsonc.</summary>
    public string Language => _language ??= Load();

    /// <summary>
    ///     Chave de edition EFETIVA de uma definição: <c>displayName[Language]</c> quando configurado e
    ///     não-vazio, senão <c>name</c>; sempre trimmed. Única fonte de verdade do re-chaveamento —
    ///     nenhum caller deve re-derivar isso à mão. (ref: CR-01-01/CR-01-02)
    /// </summary>
    public string ResolveEditionKey(ClassDefinition def)
    {
        var localized = Language switch
        {
            "pt" => def.DisplayName?.Pt,
            "en" => def.DisplayName?.En,
            _ => null,
        };
        var key = string.IsNullOrWhiteSpace(localized) ? def.Name : localized;
        return (key ?? string.Empty).Trim();
    }

    /// <summary>Lê config/settings.jsonc (mesma lógica do antigo CustomClassesMod.LoadLauncherLanguage).</summary>
    private string Load()
    {
        try
        {
            var settingsPath = System.IO.Path.Combine(
                modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()), "config", "settings.jsonc");
            if (!System.IO.File.Exists(settingsPath))
            {
                return "name";   // sem config → mantém o name cru (compat)
            }

            var settings = jsonUtil.Deserialize<LauncherSettings>(fileUtil.ReadFile(settingsPath));
            var lang = settings?.Language?.Trim().ToLowerInvariant();
            var resolved = lang is "pt" or "en" ? lang : "name";
            if (resolved != "name")
            {
                logger.Info($"[CustomClasses] launcher language = '{resolved}' — editions registradas pelo displayName.{resolved}.");
            }

            return resolved;
        }
        catch (Exception ex)
        {
            logger.Warning($"[CustomClasses] config/settings.jsonc inválido — usando 'name'. {ex.Message}");
            return "name";
        }
    }

    /// <summary>config/settings.jsonc — opções globais do mod (separadas dos arquivos de classe).</summary>
    private sealed record LauncherSettings
    {
        /// <summary>"pt" | "en" → nome de edition no launcher pelo displayName; "name"/ausente = name cru.</summary>
        [JsonPropertyName("language")] public string? Language { get; init; }
    }
}
