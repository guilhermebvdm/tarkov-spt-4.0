using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;                 // IOnLoad, OnLoadOrder
using SPTarkov.Server.Core.Helpers;            // ModHelper
using SPTarkov.Server.Core.Servers;            // ConfigServer
using SPTarkov.Server.Core.Models.Spt.Config;  // CoreConfig
using SPTarkov.Server.Core.Models.Utils;       // ISptLogger
using SPTarkov.Server.Core.Utils;              // FileUtil

namespace CustomClasses;

/// <summary>
///     Item 009: oculta edições vanilla da tela de criação de perfil do launcher, adicionando suas keys
///     a CoreConfig.Features.CreateNewProfileTypesBlacklist (LauncherController.cs:41). Config-driven.
///     Não toca templates/perfis → perfis já criados seguem jogáveis.
///     Limitação conhecida (PA-01-01): o launcher v2 (/launcher/v2/types → LauncherV2Controller.Types)
///     NÃO consulta a blacklist; esta ocultação vale para o launcher v1 (/launcher/server/connect),
///     que é o usado pelo launcher SPT atual.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]   // ref: OnLoadOrder.cs
public class HiddenEditionsLoader(
    ModHelper modHelper,
    FileUtil fileUtil,
    ConfigServer configServer,
    ISptLogger<HiddenEditionsLoader> logger
) : IOnLoad
{
    public Task OnLoad()
    {
        // Mesmo padrão de path do CustomClassesMod.OnLoad (PA-01-04).
        var configPath = System.IO.Path.Combine(
            modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()), "config");   // ref: ModHelper.cs:10
        var configFile = System.IO.Path.Combine(configPath, "hidden-editions.jsonc");

        if (!fileUtil.FileExists(configFile))
        {
            logger.Info("[CustomClasses] hidden-editions.jsonc ausente — nenhuma edição ocultada.");
            return Task.CompletedTask;
        }

        var cfg = modHelper.GetJsonDataFromFile<HiddenEditionsConfig>(configPath, "hidden-editions.jsonc");   // ref: ModHelper.cs:22

        var hide = cfg?.Hide;
        if (hide is null || hide.Count == 0)
        {
            logger.Info("[CustomClasses] hidden-editions: nenhuma edição a ocultar.");
            return Task.CompletedTask;
        }

        // ref: CoreConfig.cs:267 — HashSet<string>; mesma instância singleton lida pelo LauncherController no connect.
        var blacklist = configServer.GetConfig<CoreConfig>().Features.CreateNewProfileTypesBlacklist;
        var added = 0;
        foreach (var key in hide)
        {
            if (!string.IsNullOrWhiteSpace(key) && blacklist.Add(key.Trim()))   // HashSet.Add = idempotente
            {
                added++;
            }
        }

        logger.Info($"[CustomClasses] {added} edição(ões) vanilla ocultada(s) do launcher (v1).");
        return Task.CompletedTask;
    }
}
