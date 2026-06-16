using System.Reflection;
using System.Text.Json.Serialization;                   // JsonPropertyName (settings.jsonc)
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;                          // IOnLoad, OnLoadOrder
using SPTarkov.Server.Core.Helpers;                     // ModHelper
using SPTarkov.Server.Core.Utils;                       // FileUtil, JsonUtil
using SPTarkov.Server.Core.Models.Utils;                // ISptLogger

namespace CustomClasses;

/// <summary>
///     Registers character "classes" as selectable launcher editions by loading one JSON file
///     per class from <c>config/classes/</c> and injecting each into the profiles database after
///     it is loaded (PostDBModLoader). Adding a class = dropping a <c>.json</c>/<c>.jsonc</c> file
///     (no recompile). Invalid files are skipped with a clear log; the others still load.
///     Item 021: the per-class validate/build/commit logic lives in <see cref="ClassRegistrar"/> —
///     the SAME pipeline the web editor uses, so boot and editor share validation by construction.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]   // ref: OnLoadOrder.cs:9
public class CustomClassesMod(
    ModHelper modHelper,
    FileUtil fileUtil,
    JsonUtil jsonUtil,
    ClassRegistrar classRegistrar,
    ISptLogger<CustomClassesMod> logger
) : IOnLoad
{
    public Task OnLoad()
    {
        var classesPath = System.IO.Path.Combine(   // System.IO.Path: evita ambiguidade com ...Tables.Path
            modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()),   // ref: ModHelper.cs:10
            "config", "classes");

        if (!fileUtil.DirectoryExists(classesPath))   // ref: FileUtil.cs:48
        {
            logger.Info($"[CustomClasses] No classes folder at '{classesPath}' — no custom classes registered.");
            return Task.CompletedTask;
        }

        // Item 006: detecta o Skills-Extended (soft) p/ avisar sobre multiplicadores de skills que dependem dele.
        logger.Info($"[CustomClasses] Skills-Extended detectado: {(classRegistrar.SkillsExtendedInstalled ? "sim" : "não")}.");

        // Língua dos NOMES de classe no launcher (config/settings.jsonc). O launcher mostra a CHAVE da
        // edition (= name); não há locale p/ o nome. Então, em "pt"/"en", registramos a edition usando
        // displayName[língua] (Caçador/Hunter). "name"/ausente = usa o name cru. ⚠ re-chaveia editions.
        var language = LoadLauncherLanguage();
        if (language != "name") logger.Info($"[CustomClasses] launcher language = '{language}' — editions registradas pelo displayName.{language}.");

        // Non-recursive: only the top of config/classes/ (subfolders ignored — handy for drafts). PA-01-03
        var files = fileUtil.GetFiles(classesPath, false, "*.json")   // ref: FileUtil.cs:11
            .Concat(fileUtil.GetFiles(classesPath, false, "*.jsonc"))
            .Distinct()   // CR-01-02: dedupe — defesa contra overlap de glob *.json/*.jsonc em alguns sistemas
            .ToList();

        int loaded = 0, skipped = 0;

        foreach (var file in files)
        {
            var fileName = fileUtil.GetFileNameAndExtension(file);   // ref: FileUtil.cs:33
            try
            {
                var def = jsonUtil.Deserialize<ClassDefinition>(fileUtil.ReadFile(file));   // ref: ModHelper.cs:28
                if (def is null || string.IsNullOrWhiteSpace(def.Name))
                {
                    logger.Error($"[CustomClasses] '{fileName}': missing required 'name' — skipped.");
                    skipped++;
                    continue;
                }

                if (!def.Enabled)
                {
                    logger.Info($"[CustomClasses] '{def.Name}' is disabled in '{fileName}' — skipped.");
                    skipped++;
                    continue;
                }

                def = ApplyLauncherLanguage(def, language);   // edition key = displayName[língua] quando configurado

                // Item 021: dry-run (validate + build) then commit — same logs/behaviour as the pre-021
                // monolithic RegisterClass. allowReplace=false at boot: never overwrite an existing edition.
                var plan = classRegistrar.ValidateAndBuild(def, fileName, allowReplace: false, out _);
                if (plan is not null)
                {
                    classRegistrar.Commit(plan);
                    loaded++;
                }
                else
                {
                    skipped++;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"[CustomClasses] '{fileName}': failed to parse/register — skipped. {ex.Message}");
                skipped++;
            }
        }

        logger.Info($"[CustomClasses] Loaded {loaded} class(es), skipped {skipped}, from '{classesPath}'.");
        return Task.CompletedTask;
    }

    /// <summary>Lê config/settings.jsonc → língua dos nomes de edition no launcher ("pt"/"en"/"name").</summary>
    private string LoadLauncherLanguage()
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
            return lang is "pt" or "en" ? lang : "name";
        }
        catch (Exception ex)
        {
            logger.Warning($"[CustomClasses] config/settings.jsonc inválido — usando 'name'. {ex.Message}");
            return "name";
        }
    }

    /// <summary>Quando a língua é "pt"/"en", troca o name (chave da edition no launcher) pelo displayName
    /// correspondente. Sem displayName na língua → mantém o name. NÃO altera o displayName (in-game segue
    /// localizado normalmente).</summary>
    private static ClassDefinition ApplyLauncherLanguage(ClassDefinition def, string language)
    {
        var localized = language switch
        {
            "pt" => def.DisplayName?.Pt,
            "en" => def.DisplayName?.En,
            _ => null,
        };
        return string.IsNullOrWhiteSpace(localized) || localized == def.Name
            ? def
            : def with { Name = localized! };
    }

    /// <summary>config/settings.jsonc — opções globais do mod (separadas dos arquivos de classe).</summary>
    private sealed record LauncherSettings
    {
        /// <summary>"pt" | "en" → nome de edition no launcher pelo displayName; "name"/ausente = name cru.</summary>
        [JsonPropertyName("language")] public string? Language { get; init; }
    }
}
