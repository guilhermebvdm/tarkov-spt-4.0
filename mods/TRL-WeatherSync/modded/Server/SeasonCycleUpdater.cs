using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using TrlWeatherSync.SeasonCycle.Models;

namespace TrlWeatherSync.SeasonCycle;

// ref: CR-01-01 — TypePriority = OnLoadOrder.Database (200000) garante que este OnLoad roda ANTES de
// GameCallbacks.OnLoad() (300000, GameCallbacks.cs:14), que dispara PostDbLoadService.PerformPostDbLoadActions()
// (PostDbLoadService.cs:126-127) → RaidWeatherService.GenerateFutureWeatherAndCache() — um cache de previsão
// de clima gerado UMA VEZ no boot e só regenerado quando expira (RaidWeatherService.cs:97-108). Sem essa
// prioridade, a(s) primeira(s) raid(s) após cada restart do SPT Server podiam usar a estação real do
// calendário em vez da estação do ciclo deste mod, mesmo com OverrideSeason já certo (que é lido ao vivo,
// mas o CACHE de pesos de clima não era).
[Injectable(InjectionType.Singleton, null, OnLoadOrder.Database)]
public class SeasonCycleUpdater(
    ISptLogger<SeasonCycleUpdater> logger,
    SeasonCycleConfigController configController,
    ConfigServer configServer
) : IOnLoad, IOnUpdate
{
    // ref: references/spt-source/Libraries/SPTarkov.Server.Core/Services/SeasonalEventService.cs:307-312
    //      GetActiveWeatherSeason() lê OverrideSeason ANTES do calendário real — único gancho necessário.
    // ref: references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/WeatherController.cs:27
    //      mesma API (GetConfig<WeatherConfig>()) usada pelo código nativo do servidor, hoje [Obsolete] p/ 4.1 (risco §7 da spec técnica).
    public Task OnLoad()
    {
        Apply();
        return Task.CompletedTask;
    }

    public Task<bool> OnUpdate(long secondsSinceLastRun)
    {
        Apply();
        return Task.FromResult(true); // recálculo é barato e idempotente — sem necessidade de cache/throttle próprio
    }

    private void Apply()
    {
        // ref: PA-01-01 — configController.Config pode ainda ser null se este OnLoad rodar antes do
        // SeasonCycleConfigController.OnLoad() terminar (ordem entre IOnLoad de mods diferentes não é
        // garantida pelo código vendorizado, App.cs:68). Auto-cura no próximo OnUpdate (~5s depois).
        var cfg = configController.Config;
        if (cfg == null)
        {
            logger.Warning("TRL-WeatherSync SeasonCycle: config ainda não carregada — tentando de novo no próximo tick.");
            return;
        }

        try
        {
            // ref: CR-01-03 — capturado uma vez em vez de reler a propriedade WeatherConfig (que reexecuta
            // ConfigServer.GetConfig<T>(), um scan LINQ + lookup) a cada uso abaixo.
            var weatherConfig = configServer.GetConfig<WeatherConfig>();

            // A estação CÍCLICA (calendário real do mod) é sempre calculada, mesmo com fixedSeason ativo —
            // ref: PA-01-04. "Estação fixa" trava só a estética/sub-fase nativa (folhagem, neve acumulada),
            // NÃO a chance de sol/chuva/neve — decisão do usuário: clima continua variando mesmo travado.
            var cyclicSeasonName = ResolveSeasonFromCycle(cfg);
            var activeSeasonName = cfg.FixedSeason ?? cyclicSeasonName;

            // Season.STORM não existe no ESeason do cliente (ESeason.cs:1-9, só 0-5) — nunca aplicar aqui.
            // ref: PA-01-05 — ignoreCase:true evita que um typo de maiúscula no .json falhe silenciosamente.
            if (!Enum.TryParse(activeSeasonName, ignoreCase: true, out Season season) || season == Season.STORM)
            {
                logger.Warning($"TRL-WeatherSync SeasonCycle: nome de estação inválido '{activeSeasonName}' — ignorando ciclo neste tick.");
                return;
            }

            weatherConfig.OverrideSeason = season;

            foreach (var (seasonKey, presetWeights) in cfg.WeatherPresetWeight)
            {
                weatherConfig.Weather.WeatherPresetWeight[seasonKey] = ParseWeatherPresetWeights(seasonKey, presetWeights);
            }

            // Com estação fixa ativa, GetActiveWeatherSeason() (SeasonalEventService.cs:307-312) retorna sempre
            // `season` (= activeSeasonName) — e é essa mesma chave que GetWeatherPresetWeightsBySeason usa
            // (WeatherGenerator.cs:77-82). Sem este passo, os pesos ficariam travados nos da estação fixa.
            // Sobrescrever a entrada de activeSeasonName com os pesos da estação cíclica corrente faz o clima
            // continuar variando ao longo do ano mesmo com a estética travada.
            if (cfg.FixedSeason != null && cfg.WeatherPresetWeight.TryGetValue(cyclicSeasonName, out var cyclicWeights))
            {
                weatherConfig.Weather.WeatherPresetWeight[activeSeasonName] = ParseWeatherPresetWeights(cyclicSeasonName, cyclicWeights);
            }
        }
        catch (Exception ex)
        {
            // ref: PA-01-01 — airbag: uma exceção aqui não pode propagar até App.cs:68-71 (sem try/catch) e
            // arriscar o boot do servidor inteiro por causa deste mod.
            logger.Error($"TRL-WeatherSync SeasonCycle: falha ao aplicar ciclo de estação — {ex.Message}");
        }
    }

    // ref: CR-01-02 — TryParse por entrada em vez de Enum.Parse dentro do .ToDictionary(): um nome de preset
    // inválido (typo no .json) antes pulava a estação INTEIRA (a exceção escapava do foreach de Apply() e
    // abortava as outras 5), não só a entrada inválida.
    private Dictionary<WeatherPreset, double> ParseWeatherPresetWeights(string seasonKey, Dictionary<string, double> presetWeights)
    {
        var parsed = new Dictionary<WeatherPreset, double>();
        foreach (var (presetName, weight) in presetWeights)
        {
            if (Enum.TryParse<WeatherPreset>(presetName, ignoreCase: true, out var preset))
            {
                parsed[preset] = weight;
            }
            else
            {
                logger.Warning($"TRL-WeatherSync SeasonCycle: preset de clima inválido '{presetName}' em '{seasonKey}' — ignorado.");
            }
        }

        return parsed;
    }

    // Corner case "primeira execução sem estado salvo → Outono": satisfeito por design — o season-cycle.json
    // padrão tem AUTUMN como cycleOrder[0] e referenceEpochUtc = data de criação do config, então dia 0 do
    // ciclo já cai em Outono, sem lógica de "primeira vez" separada.
    private static string ResolveSeasonFromCycle(SeasonCycleConfig cfg)
    {
        var nowUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var elapsedDays = (nowUtc - cfg.ReferenceEpochUtc) / 86400.0;
        var cycleLengthDays = cfg.CycleOrder.Sum(e => e.Days);
        // módulo sempre positivo mesmo se referenceEpochUtc estiver no futuro ou elapsedDays for negativo
        var dayInCycle = ((elapsedDays % cycleLengthDays) + cycleLengthDays) % cycleLengthDays;

        var cursor = 0.0;
        foreach (var entry in cfg.CycleOrder)
        {
            cursor += entry.Days;
            if (dayInCycle < cursor)
            {
                return entry.Season;
            }
        }

        return cfg.CycleOrder[^1].Season; // fallback de arredondamento de ponto flutuante
    }
}
