using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using TrlWeatherSync.SeasonCycle.Models;

namespace TrlWeatherSync.SeasonCycle;

// ref: mods/Skills-Extended/modded/Server/Core/ConfigController.cs:11-46 (mesmo padrão, adaptado)
// TypePriority = OnLoadOrder.PreSptModLoader é uma tentativa de rodar cedo, mas App.cs:68-71 (foreach sobre
// IEnumerable<IOnLoad>) não expõe, no código vendorizado deste repo, garantia de que a prioridade é respeitada
// entre mods diferentes (ordenação real, se existir, vive dentro do pacote SPTarkov.DI, fora deste repo) —
// ref: PA-01-01. Por isso SeasonCycleUpdater.Apply() nunca assume Config != null (ver null-guard lá).
[Injectable(InjectionType.Singleton, null, OnLoadOrder.PreSptModLoader)]
public class SeasonCycleConfigController(
    ISptLogger<SeasonCycleConfigController> logger,
    FileUtil fileUtil,
    JsonUtil jsonUtil
) : IOnLoad
{
    public SeasonCycleConfig Config { get; private set; } = null!;

    // Pasta do assembly instalado — decisão do usuário (2026-09-11): sem segmento "Resources/" intermediário,
    // fica SPT/user/mods/TRL-WeatherSync/Config/season-cycle.json em vez de .../Resources/Configs/....
    private static readonly string AssemblyDirectory =
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

    // Reload: só carrega uma vez, no boot — decisão consciente (PA-01-02): o usuário reinicia o SPT Server
    // manualmente após editar season-cycle.json, mesmo fluxo já usado pros outros mods do repo. A progressão
    // da estação em si NÃO depende de reload (SeasonCycleUpdater recalcula a cada tick a partir do tempo real).
    public async Task OnLoad()
    {
        // ref: PA-01-01 — App.cs:68-71 não embrulha IOnLoad.OnLoad() em try/catch; uma exceção aqui
        // (arquivo ausente/malformado) propagaria e poderia abortar o boot do servidor inteiro, não só deste mod.
        try
        {
            var path = Path.Combine(AssemblyDirectory, "Config", "season-cycle.json");
            var text = await fileUtil.ReadFileAsync(path);
            Config = jsonUtil.Deserialize<SeasonCycleConfig>(text)!;
            logger.Info($"TRL-WeatherSync SeasonCycle: config carregada — {Config.CycleOrder.Count} sub-fases.");
        }
        catch (Exception ex)
        {
            logger.Error($"TRL-WeatherSync SeasonCycle: falha ao carregar season-cycle.json — {ex.Message}");
        }
    }
}
