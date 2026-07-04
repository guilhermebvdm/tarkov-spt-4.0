using System.Reflection;
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

        // Língua dos NOMES de classe no launcher: o re-chaveamento (pt/en → displayName[lang]) vive
        // no pipeline compartilhado desde o item 058 (ref: CR-01-01) — LauncherLanguageConfig →
        // ClassRegistrar.ValidateAndBuild. Boot, Save/hotApply e Delete produzem a MESMA chave;
        // o boot não tem mais lógica própria de língua (o config loga a língua ativa no 1º uso).

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

}
