# 002 — Gerenciador Ciclo Natural Estações · Spec Técnica

**Mod:** TRL-WeatherSync
**Spec funcional:** [002-gerenciador-ciclo-natural-estacoes-01-spec.md](002-gerenciador-ciclo-natural-estacoes-01-spec.md)
**Criado:** 2026-09-10

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/) (cliente) e `references/spt-source/` (servidor). Toda referência ao código cita `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

## 1. Estratégia

Este item é **100% server-side**. Não há Harmony, não há patch de cliente, não há novo `ConfigEntry` — decisão já travada na spec funcional (`.json` do servidor SPT, não F12).

O mecanismo real: `SeasonalEventService.GetActiveWeatherSeason()` ([`SeasonalEventService.cs:307-312`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/SeasonalEventService.cs#L307)) checa `WeatherConfig.OverrideSeason` **antes** de qualquer cálculo por data real de calendário:

```csharp
public Season GetActiveWeatherSeason()
{
    if (WeatherConfig.OverrideSeason.HasValue)
    {
        return WeatherConfig.OverrideSeason.Value;
    }
    // ... fallback por SeasonDates (data real) ...
}
```

Isso significa que o mod não precisa reimplementar nem patchear `SeasonalEventService`, `WeatherController` ou `WeatherGenerator` — basta **escrever** em `WeatherConfig.OverrideSeason` ([`WeatherConfig.cs:23`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Spt/Config/WeatherConfig.cs#L23)) e em `WeatherConfig.Weather.WeatherPresetWeight` ([`WeatherConfig.cs:69`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Spt/Config/WeatherConfig.cs#L69)) a cada ciclo. O SPT 4.0 expõe exatamente os hooks de DI que um mod de servidor precisa para isso — `IOnLoad.OnLoad()` (chamado uma vez no boot, [`App.cs:68-71`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/App.cs#L68)) e `IOnUpdate.OnUpdate(long secondsSinceLastRun)` (chamado a cada ~5s em loop, [`App.cs:92-122`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/App.cs#L92)).

**Nenhum código de cliente é necessário.** A estação calculada pelo servidor chega ao client pelo handshake nativo já documentado na investigação do item 001: `HostGameController.GenerateWeathers()` chama `_backendSession.WeatherRequest()` e depois `Season = _backendSession.Season` ([`HostGameController.cs:530-538`](../../../../references/fika-plugin/Fika.Core/Main/GameMode/HostGameController.cs#L530), FIKA) — o mesmo endpoint que `WeatherController.Generate()` do servidor preenche via `GetActiveWeatherSeason()` ([`WeatherController.cs:33-58`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/WeatherController.cs#L33)). Não há nada pra sincronizar em raid — a estação já é resolvida antes da raid começar, no processo de servidor.

**Precedente real no repo.** Dois mods já shippam server mods C# 4.0 funcionais sob a convenção `modded/Client/` + `modded/Server/`: `mods/Skills-Extended/modded/Server/` e `mods/CustomClasses/modded/Server/`. O design abaixo (metadata, `IOnLoad`, leitura de `.json` próprio em `Resources/Configs/`) é copiado desse padrão real, não inventado — ver `mods/Skills-Extended/modded/Server/Core/ConfigController.cs:11-46` e `Metadata.cs:10-24`.

**Decisão estrutural que precisa de confirmação antes do `/code-mod` (ver §7):** para caber na convenção acima, os arquivos atuais de `modded/*.cs`/`.csproj` do item 001 (client) precisam mover para `modded/Client/`. Isso toca paths já publicados (csproj, README, PROPRIEDADES). Está documentado como risco, não como fato consumado.

## 2. Pontos de integração (servidor, sem Harmony)

> Não existe Prefix/Postfix/Transpiler aqui — mods de servidor SPT 4.0 não usam Harmony. A "integração" é mutação direta de propriedades via DI, na mesma API que o próprio `WeatherController` do servidor usa.

| Alvo | Tipo | Motivo |
|---|---|---|
| [`WeatherConfig.OverrideSeason`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Spt/Config/WeatherConfig.cs#L23) | Mutação via `ConfigServer.GetConfig<WeatherConfig>()` | Força a sub-fase ativa (`Season`), lida por `GetActiveWeatherSeason()` antes do calendário real. |
| [`WeatherConfig.Weather.WeatherPresetWeight`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Spt/Config/WeatherConfig.cs#L69) | Mutação via `ConfigServer.GetConfig<WeatherConfig>()` | Pesos de sol/chuva/nublado por estação, lidos por [`WeatherGenerator.GetWeatherPresetWeightsBySeason(Season)`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Generators/WeatherGenerator.cs#L77) via `currentSeason.ToString()`. |

**Confirmação da API canônica:** o próprio `WeatherController` do servidor (não um patch nosso — código nativo) resolve `WeatherConfig` da mesma forma: `protected readonly WeatherConfig WeatherConfig = configServer.GetConfig<WeatherConfig>();` ([`WeatherController.cs:27`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/WeatherController.cs#L27)). `ConfigServer.GetConfig<T>()` está marcado `[Obsolete]` ("será removido no 4.1 em favor de injeção direta da configuração", [`ConfigServer.cs:34`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Servers/ConfigServer.cs#L34)) — mas é o padrão **vivo** em 4.0, usado pelo código nativo citado acima. Ver risco em §7.

**Chave correta do dicionário de pesos.** `WeatherGenerator.cs:79`:
```csharp
return WeatherConfig.Weather.WeatherPresetWeight.TryGetValue(currentSeason.ToString(), out var weights)
    ? weights
    : WeatherConfig.Weather.WeatherPresetWeight.GetValueOrDefault("default")!;
```
`currentSeason.ToString()` para `Season.WINTER` é literalmente `"WINTER"`. O `weather.json` vanilla usa chaves `"WINTER_START"`/`"WINTER_END"`/`"default"` (suspeita de bug documentada em `docs/investigacao-fika-eft-2026-09-10.md` seção I) — nosso mod usa as chaves corretas (`"SUMMER"`, `"AUTUMN"`, `"WINTER"`, `"SPRING"`, `"AUTUMN_LATE"`, `"SPRING_EARLY"`) e não reproduz esse problema.

**Enum real do servidor** ([`Season.cs`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Enums/Season.cs)):
```csharp
public enum Season { SUMMER = 0, AUTUMN = 1, WINTER = 2, SPRING = 3, AUTUMN_LATE = 4, SPRING_EARLY = 5, STORM = 6 }
```
Mapeia 1:1 (nomes diferentes, valores iguais) com o `ESeason` do cliente ([`ESeason.cs:1-9`](../../../../references/eft-decompiled/Assembly-CSharp/ESeason.cs#L1), byte 0-5: `Summer, Autumn, Winter, Spring, AutumnLate, SpringEarly`) — **exceto `STORM = 6`, que não existe no `ESeason` do cliente.** Nunca setar `OverrideSeason = Season.STORM` (ver checklist de risco §7 e §9 check 4).

## 3. Novas propriedades F12 (BepInEx)

**N/A.** Decisão explícita da spec funcional (§"Comportamento desejado", `001-...` já estabelece o mesmo padrão pro item 001 onde aplicável): duração de estação, modo de progressão e pesos de clima vêm de um `.json` que o servidor SPT lê, não de `ConfigEntry` no client. Nenhuma propriedade nova em `PROPRIEDADES.md`.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/*.cs`, `modded/*.csproj`, `modded/References/`, `modded/Networking/`, `modded/Patches/` | **MOVER → `modded/Client/`** | Alinha com a convenção do repo (`docs/technical/spt4-mod-creation.md` §1) agora que o mod passa a ter os dois lados. **Precisa confirmação do usuário antes do `/code-mod`** — ver §7. |
| `modded/Server/Server.csproj` | CRIAR | Projeto `net9.0`, `SPTarkov.Reflection` + `SPTarkov.Server.Core` (mesmas versões usadas em `mods/Skills-Extended/modded/Server/Server.csproj:15-17`, `4.0.2`). **Sem** target `CopyToServer`/`PostBuildEvent` — ver risco §7. |
| `modded/Server/SeasonCycleModMetadata.cs` | CRIAR | `AbstractModMetadata` (`ModGuid`, `Name`, `Version`, `SptVersion`) — obrigatório pro servidor aceitar o mod. |
| `modded/Server/Models/SeasonCycleConfig.cs` | CRIAR | Records do `season-cycle.json` próprio do mod (`ReferenceEpochUtc`, `CycleOrder`, `FixedSeason`, `WeatherPresetWeight`). |
| `modded/Server/SeasonCycleConfigController.cs` | CRIAR | `IOnLoad` — lê `Resources/Configs/season-cycle.json` ao lado do assembly. |
| `modded/Server/SeasonCycleUpdater.cs` | CRIAR | `IOnLoad` + `IOnUpdate` — calcula a sub-fase ativa por tempo real e escreve em `WeatherConfig`. |
| `modded/Server/Resources/Configs/season-cycle.json` | CRIAR | Config padrão (ver §5, valores default derivados da Sessão 1 da memória do mod). |
| `README.md` | MODIFICAR | Nota: todas as instalações FIKA da mesma raid precisam do **mesmo** `season-cycle.json` (mesma exigência que "mod instalado em todos" do item 001); editar o arquivo exige reiniciar o SPT Server pra pegar efeito (`season-cycle.json` é lido só uma vez no boot, `PA-01-02`). |
| `PROPRIEDADES.md` | MODIFICAR | Nota explicando por que este item não adiciona `ConfigEntry` (config 100% server `.json`). |

## 5. Stubs de código

> Blocos compiláveis com assinatura completa e corpo mínimo plausível. Padrão de DI/config copiado de `mods/Skills-Extended/modded/Server/` (precedente real já compilando no repo).

```csharp
// modded/Server/SeasonCycleModMetadata.cs
using SPTarkov.Server.Core.Models.Spt.Mod;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace TrlWeatherSync.SeasonCycle;

// ref: mods/Skills-Extended/modded/Server/Metadata.cs:10-24 (padrão real já usado no repo)
public record SeasonCycleModMetadata : AbstractModMetadata
{
    public static SeasonCycleModMetadata Instance { get; } = new();
    public override string ModGuid { get; init; } = "trl.weathersync.seasoncycle";
    public override string Name { get; init; } = "TRL-WeatherSync — Season Cycle";
    public override string Author { get; init; } = "TRL";
    public override Version Version { get; init; } = new("1.0.0");
    public override Range SptVersion { get; init; } = new("~4.0.0");
    public override string License { get; init; } = "All Rights Reserved";
}
```

```csharp
// modded/Server/Models/SeasonCycleConfig.cs
using System.Text.Json.Serialization;

namespace TrlWeatherSync.SeasonCycle.Models;

public record SeasonCycleEntry
{
    // Um dos 6 nomes de Season.cs (SUMMER/AUTUMN/WINTER/SPRING/AUTUMN_LATE/SPRING_EARLY).
    // ref: references/spt-source/Libraries/SPTarkov.Server.Core/Models/Enums/Season.cs — nunca "STORM" aqui.
    [JsonPropertyName("season")]
    public required string Season { get; set; }

    [JsonPropertyName("days")]
    public required double Days { get; set; }
}

public record SeasonCycleConfig
{
    // Timestamp Unix (UTC, segundos) fixo — imune a relógio do jogador alterado (corner case da spec funcional).
    [JsonPropertyName("referenceEpochUtc")]
    public required long ReferenceEpochUtc { get; set; }

    // Ordem + duração (dias) de cada sub-fase. Soma = duração do ciclo anual completo.
    [JsonPropertyName("cycleOrder")]
    public required List<SeasonCycleEntry> CycleOrder { get; set; }

    // null = segue o ciclo; senão trava nessa sub-fase (nome de Season). Corner case "modo estação fixa".
    [JsonPropertyName("fixedSeason")]
    public string? FixedSeason { get; set; }

    // Chave externa = nome de Season ("WINTER"); interna = nome de WeatherPreset ("SUNNY"/"RAINY"/"CLOUDY").
    // ref: references/spt-source/Libraries/SPTarkov.Server.Core/Generators/WeatherGenerator.cs:79 (chave = currentSeason.ToString())
    [JsonPropertyName("weatherPresetWeight")]
    public required Dictionary<string, Dictionary<string, double>> WeatherPresetWeight { get; set; }
}
```

```csharp
// modded/Server/SeasonCycleConfigController.cs
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

    private static readonly string ResourcesDirectory =
        Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Resources");

    public async Task OnLoad()
    {
        // ref: PA-01-01 — App.cs:68-71 não embrulha IOnLoad.OnLoad() em try/catch; uma exceção aqui
        // (arquivo ausente/malformado) propagaria e poderia abortar o boot do servidor inteiro, não só deste mod.
        try
        {
            var path = Path.Combine(ResourcesDirectory, "Configs", "season-cycle.json");
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
```

> **Reload:** `SeasonCycleConfigController` lê `season-cycle.json` só uma vez, no boot (`IOnLoad`, não `IOnUpdate`) — decisão consciente (PA-01-02, ver Histórico): o usuário reinicia o SPT Server manualmente após editar `.json`, mesmo fluxo que já usa pros outros mods do repo. Nenhum hot-reload é necessário. A progressão da estação em si (§ `ResolveSeasonFromCycle` abaixo) **não** depende de reload — ela já é recalculada a cada tick de `OnUpdate` a partir do tempo real decorrido, então a estação avança normalmente entre raids sem precisar reiniciar nada; só uma **edição do `.json`** exige restart.

```csharp
// modded/Server/SeasonCycleUpdater.cs
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using TrlWeatherSync.SeasonCycle.Models;

namespace TrlWeatherSync.SeasonCycle;

// Roda depois do SeasonCycleConfigController (ordem padrão de Injectable, sem prioridade = após PreSptModLoader).
[Injectable(InjectionType.Singleton)]
public class SeasonCycleUpdater(
    ISptLogger<SeasonCycleUpdater> logger,
    SeasonCycleConfigController configController,
    ConfigServer configServer
) : IOnLoad, IOnUpdate
{
    // ref: references/spt-source/Libraries/SPTarkov.Server.Core/Services/SeasonalEventService.cs:307-312
    //      GetActiveWeatherSeason() lê OverrideSeason ANTES do calendário real — único gancho necessário.
    // ref: references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/WeatherController.cs:27
    //      mesma API (GetConfig<WeatherConfig>()) usada pelo código nativo do servidor, hoje [Obsolete] p/ 4.1 (risco §7).
    private WeatherConfig WeatherConfig => configServer.GetConfig<WeatherConfig>();

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

            WeatherConfig.OverrideSeason = season;

            foreach (var (seasonKey, presetWeights) in cfg.WeatherPresetWeight)
            {
                WeatherConfig.Weather.WeatherPresetWeight[seasonKey] = presetWeights.ToDictionary(
                    kv => Enum.Parse<WeatherPreset>(kv.Key, ignoreCase: true),
                    kv => kv.Value);
            }

            // Com estação fixa ativa, GetActiveWeatherSeason() (SeasonalEventService.cs:307-312) retorna sempre
            // `season` (= activeSeasonName) — e é essa mesma chave que GetWeatherPresetWeightsBySeason usa
            // (WeatherGenerator.cs:77-82). Sem este passo, os pesos ficariam travados nos da estação fixa.
            // Sobrescrever a entrada de activeSeasonName com os pesos da estação cíclica corrente faz o clima
            // continuar variando ao longo do ano mesmo com a estética travada.
            if (cfg.FixedSeason != null && cfg.WeatherPresetWeight.TryGetValue(cyclicSeasonName, out var cyclicWeights))
            {
                WeatherConfig.Weather.WeatherPresetWeight[activeSeasonName] = cyclicWeights.ToDictionary(
                    kv => Enum.Parse<WeatherPreset>(kv.Key, ignoreCase: true),
                    kv => kv.Value);
            }
        }
        catch (Exception ex)
        {
            // ref: PA-01-01 — airbag: uma exceção aqui não pode propagar até App.cs:68-71 (sem try/catch) e
            // arriscar o boot do servidor inteiro por causa deste mod.
            logger.Error($"TRL-WeatherSync SeasonCycle: falha ao aplicar ciclo de estação — {ex.Message}");
        }
    }

    // Corner case "primeira execução sem estado salvo → Outono": satisfeito por design — o `season-cycle.json`
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
```

```jsonc
// modded/Server/Resources/Configs/season-cycle.json (default de fábrica)
// Ordem e durações conforme decisão da Sessão 1 (memory/sessions.md): 7 dias para Verão/Inverno (fase única),
// 3.5+3.5 dias para as sub-fases de transição de Primavera e Outono. Total: 28 dias (ciclo anual completo).
// AUTUMN é a primeira entrada de propósito — corner case "primeira execução → Outono" (spec funcional).
{
  "referenceEpochUtc": 1757462400,
  "cycleOrder": [
    { "season": "AUTUMN",       "days": 3.5 },
    { "season": "AUTUMN_LATE",  "days": 3.5 },
    { "season": "WINTER",       "days": 7 },
    { "season": "SPRING_EARLY", "days": 3.5 },
    { "season": "SPRING",       "days": 3.5 },
    { "season": "SUMMER",       "days": 7 }
  ],
  "fixedSeason": null,
  "weatherPresetWeight": {
    "SUMMER":       { "SUNNY": 5, "RAINY": 1, "CLOUDY": 3 },
    "AUTUMN":       { "SUNNY": 3, "RAINY": 2, "CLOUDY": 4 },
    "AUTUMN_LATE":  { "SUNNY": 2, "RAINY": 3, "CLOUDY": 4 },
    "WINTER":       { "SUNNY": 1, "RAINY": 4, "CLOUDY": 4 },
    "SPRING_EARLY": { "SUNNY": 3, "RAINY": 3, "CLOUDY": 3 },
    "SPRING":       { "SUNNY": 4, "RAINY": 2, "CLOUDY": 3 }
  }
}
```

> **TODO confirmar (usuário):** os valores de `weatherPresetWeight` acima são um ponto de partida inspirado na proporção ~2:1 do `weather.json` vanilla (seção I da investigação), não uma cópia exata — a spec funcional pede "inspirado", não "idêntico". Ajustar depois de observar in-game.

> **Correção pós-entrega (2026-09-11, pedido do usuário):** todo `Resources/Configs/` acima (stub, path resolvido em `SeasonCycleConfigController`, `<None Include>` do `.csproj`, arquivo físico) foi simplificado pra `Config/` (singular, sem o segmento `Resources/` intermediário — o usuário pediu `Configs/` primeiro e corrigiu pra singular logo em seguida). Instalado, fica `SPT/user/mods/TRL-WeatherSync/Config/season-cycle.json`, não `.../Resources/Configs/...`. O `05-asbuild.md` reflete o estado atual; os trechos acima (`§4`, `§5`, `§8`) ficam como registro histórico do que foi codado no `/code-mod` original — não foram reescritos (ver regra de imutabilidade de reviews; aqui é só a spec, então a correção é registrada como nota em vez de silenciosamente editada).

## 6. Fluxo de dados

```
[A] boot do servidor SPT → SeasonCycleConfigController.OnLoad() lê season-cycle.json
      ↓
[B] a cada ~5s (App.cs:92-122) → SeasonCycleUpdater.OnUpdate() → ResolveSeasonFromCycle()
      ↓ escreve
[C] WeatherConfig.OverrideSeason (WeatherConfig.cs:23) + WeatherConfig.Weather.WeatherPresetWeight (WeatherConfig.cs:69)
      ↓ (algum tempo depois, jogador entra em raid)
[D] HostGameController.GenerateWeathers() (HostGameController.cs:530-538, FIKA) → _backendSession.WeatherRequest()
      ↓ HTTP client/weather
[E] WeatherController.Generate() (WeatherController.cs:33-58) → SeasonalEventService.GetActiveWeatherSeason() (SeasonalEventService.cs:307-312)
      → lê OverrideSeason PRIMEIRO (nunca cai no fallback de calendário real enquanto nosso mod estiver ativo)
      ↓
[F] WeatherGenerator.GetWeatherPresetWeightsBySeason(currentSeason) (WeatherGenerator.cs:77-82) usa os pesos do passo [C]
      ↓ resposta HTTP
[G] Season = _backendSession.Season (HostGameController.cs:537) — aplicado nativamente pelo cliente
```

**Exceção:** mapa `laboratory` — `HostGameController.cs:391-396` / `ClientGameController.cs:504-509` forçam `Season = ESeason.Summer` e `UseCustomWeather = false` incondicionalmente, ignorando qualquer coisa vinda do passo [G]. Nosso `OverrideSeason` é escrito normalmente no servidor, mas nesse mapa específico o cliente descarta o valor antes de aplicar — mesma exclusão que o item 001 já documenta pro clima de curto prazo. Nenhuma ação nossa necessária.

**Labyrinth não tem exceção de clima/estação** — a única menção a `"labyrinth"` no código do FIKA é em `ClientGameController.cs:209` (`CheckSpawnTogether`, decide se o spawn é agrupado ou aleatório), sem relação com `GenerateWeathers()`/`Season`. Segue o fluxo `[A]`→`[G]` normal.

## 7. Riscos e dependências

- ~~**🔴 `/compile-mod` não builda `server-csharp` ainda**~~ — **CORRIGIDO em 2026-09-11 (pós-`/code-mod`):** essa afirmação, baseada em [`docs/technical/spt4-mod-creation.md:142`](../../../../docs/technical/spt4-mod-creation.md#L142), estava **errada** — a doc está desatualizada. Rodar `/compile-mod TRL-WeatherSync` de fato detectou e compilou os dois `.csproj` (`csproj_kind()` reconhece `SPTarkov.` como `server`). **Efeito colateral não previsto:** como não há flag `--no-install`, o script instalou automaticamente os dois `.dll` em `E:/Tarkov Red Line` (client em `BepInEx/plugins/`, server em `SPT/user/mods/`) — violando a instrução permanente do usuário na primeira execução real. Técnica correta daqui pra frente: `--spt-path <inválido>` pula a instalação dos dois lados sem quebrar o build do server (que usa pacotes NuGet, não DLLs do `SPT_PATH`) — registrado na memória pessoal `feedback_compile_mod_install.md`.
- **🔴 Nunca replicar o padrão `CopyToServer`/`PostBuildEvent`** que `mods/Skills-Extended/modded/Server/Server.csproj:29-67` usa (MSBuild target que copia o `.dll` direto pra `SPT/user/mods/` a cada build, fora do fluxo do `/compile-mod`). Isso violaria a instrução permanente do usuário registrada na memória pessoal (`feedback_compile_mod_install.md`) de nunca instalar automaticamente na pasta real do jogo/servidor. O `Server.csproj` deste item **não deve ter esse target**.
- **🟡 Restructuring `modded/` → `Client/`+`Server/`** toca paths já publicados do item 001 (`.csproj`, `README.md`, `PROPRIEDADES.md`, links relativos nos artefatos de backlog do item 001). Confirmar com o usuário antes do `/code-mod` — não é decisão unilateral segura o suficiente pra tomar sem aviso, mesmo tendo precedente real no repo (Skills-Extended, CustomClasses).
- **🟡 `ConfigServer.GetConfig<T>()` está `[Obsolete]`** (remoção anunciada pro SPT 4.1, [`ConfigServer.cs:13,34`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Servers/ConfigServer.cs#L13)). Aceitável agora — é o padrão vivo usado pelo próprio `WeatherController.cs:27` nativo do 4.0 — mas revisitar quando o 4.1 chegar (mesma cautela de não pinar nome/API projetada, AP-09).
- **🟢 Cascata com item 001 (tempestade):** pesos de `RAINY`/`CLOUDY` mais altos no Inverno aumentam indiretamente a chance de `RollForStorm` (item 001) disparar, já que `IWeatherCurve.LightningThunderProbability` deriva de `Cloudiness`. Comportamento emergente esperado e coerente com a intenção do mod (inverno mais tempestuoso); não recalibrar `RollForStorm` como parte deste item — reavaliar depois de observar in-game se necessário.
- **Compatibilidade:** nenhum patch Harmony, nenhuma classe do cliente tocada — risco de conflito com outros mods de servidor é baixo; único ponto de atenção é outro mod de servidor que também escreva `WeatherConfig.OverrideSeason`/`WeatherPresetWeight` (nenhum identificado no repo hoje).
- **Ordem de inicialização:** `SeasonCycleUpdater` depende de `SeasonCycleConfigController.Config` já carregado. `TypePriority = OnLoadOrder.PreSptModLoader` no controller é uma tentativa de rodar cedo, mas **não está provado** pelo código vendorizado deste repo que a ordem entre `IOnLoad` de componentes diferentes é respeitada (`App.cs:68-71`, `PA-01-01`) — por isso `Apply()` nunca assume `Config != null`: checa e retorna cedo com log se ainda não carregou, tentando de novo no próximo `OnUpdate` (~5s depois).
- **`IOnLoad`/`IOnUpdate` sem try/catch no harness nativo (`App.cs:68-71`, `SptServerStartupService.cs:25`):** uma exceção não tratada em qualquer componente pode abortar o boot do servidor inteiro — não é um risco exclusivo deste item, mas o stub (§5) trata isso com try/catch próprio em ambos os `OnLoad` (`PA-01-01`, resolvido).
- **Relógio do sistema manipulado ao vivo — fora de escopo por decisão do usuário.** `ResolveSeasonFromCycle` usa `DateTimeOffset.UtcNow`, então adiantar/atrasar o relógio do Windows em tempo real ainda afeta o cálculo (`referenceEpochUtc` só resolve o ponto de partida, não protege contra manipulação contínua). Mesma limitação do vanilla (`SeasonalEventService.cs:314`, sem proteção). Decisão explícita: não tratar (`PA-01-03`, rejeitado — usuário não espera que isso aconteça e aceita o resultado se acontecer).

## 8. Checklist de implementação

- [x] Confirmar com o usuário a reestruturação `modded/*` → `modded/Client/*` — usuário aprovou (2026-09-11, `AskUserQuestion`).
- [x] Mover arquivos client existentes (`Plugin.cs`, `WeatherSyncSession.cs`, `Networking/`, `Patches/`, `TRL-WeatherSync.csproj`, `References/`) para `modded/Client/`. Verificado com `dotnet build` local — compila 0 erros/0 avisos após a mudança de path (`HintPath` relativo preservado).
- [x] Criar `modded/Server/Server.csproj` (net9.0, `SPTarkov.Reflection` + `SPTarkov.Server.Core` v4.0.2, **sem** target de auto-cópia). Ajuste sobre o stub: adicionado `<None Include="Resources\**\*.*" CopyToOutputDirectory="PreserveNewest" />` — sem isso, `season-cycle.json` não chega no `bin/` local (o stub original só cobria a cópia pro servidor real via um target que foi deliberadamente removido).
- [x] Criar `modded/Server/SeasonCycleModMetadata.cs`. Ajuste sobre o stub: `Contributors`/`Incompatibilities`/`ModDependencies`/`Url`/`IsBundleMod` são **abstratos** em `AbstractModMetadata` (confirmado por erro `CS0534` numa build sem eles), não opcionais como o stub assumia — adicionados com os mesmos defaults nulos/vazios que `mods/Skills-Extended/modded/Server/Metadata.cs` usa.
- [x] Criar `modded/Server/Models/SeasonCycleConfig.cs` (+ `SeasonCycleEntry`).
- [x] Criar `modded/Server/SeasonCycleConfigController.cs`.
- [x] Criar `modded/Server/Resources/Configs/season-cycle.json` com os valores default de fábrica (§5). Ajuste sobre o stub: comentário de topo reescrito de um campo JSON `_comment` (que teria quebrado o deserializer — `JsonUtil.cs:24`, `UnmappedMemberHandling.Disallow` rejeita chaves desconhecidas) para comentários `//` de linha (permitidos, `JsonUtil.cs:20`, `ReadCommentHandling.Skip`).
- [x] Criar `modded/Server/SeasonCycleUpdater.cs`.
- [x] Atualizar `README.md` com a exigência de `season-cycle.json` idêntico entre todas as instalações FIKA da raid + aviso de restart.
- [x] Atualizar `PROPRIEDADES.md` com nota explicando a ausência de novos `ConfigEntry`.
- [x] Decidir e documentar o caminho de build manual (`dotnet build`) — usuário aprovou (2026-09-11, `AskUserQuestion`). `dotnet build Server.csproj -c Release` validado localmente: 0 erros, 0 avisos, `season-cycle.json` copiado corretamente pro `bin/Release/net9.0/Resources/Configs/`. Nenhuma instalação automática em `SPT/user/mods/` real — build fica em `bin/`/`obj/` (gitignored), cópia manual do usuário.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | N/A | Item é 100% server-side, sem estado raid-scoped. `SeasonCycleUpdater` é um singleton de processo de servidor (`IOnLoad`/`IOnUpdate`), independente de início/fim de raid — `App.cs:68-122` mostra que esse ciclo é do servidor, não da raid. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | N/A | Nenhum patch de cliente; nenhuma lógica reage a ação de player. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | N/A | Nenhum Harmony/AccessTools. Toda API usada (`WeatherConfig`, `ConfigServer`, `SeasonalEventService`) é pública, não-ofuscada, do `SPTarkov.Server.Core`. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | `ConfigServer.GetConfig<WeatherConfig>()` é a mesma API que `WeatherController.cs:27` (nativo) usa. Side-effect mapeado em §6: `SeasonalEventService.GetActiveWeatherSeason()` consome exatamente `OverrideSeason`; `WeatherGenerator.GetWeatherPresetWeightsBySeason` consome exatamente `WeatherPresetWeight`. Cuidado adicional documentado: nunca escrever `Season.STORM` (§1, §5). |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | Estado é função pura de tempo real (`ReferenceEpochUtc` + `CycleOrder`) recalculada a cada `OnUpdate`, sem cache que precise sobreviver a nada — trivialmente correto entre raids e até entre reinícios do servidor. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | N/A | Sem `ConfigEntry` (config é `.json` server-side, decisão da spec funcional). |
| 7 | Re-invocação de método patcheado tem reentry-guard — AP-07 | N/A | Não há patch de método. `Apply()` é idempotente por natureza (recalcula do zero a cada chamada, sem estado acumulado que possa entrar em loop). |
| 8 | Flags/caches de intercept validados contra o contexto atual — AP-08 | N/A | Não há cache de contexto de arma/operação/tela; único estado mutável é o próprio `WeatherConfig`, resolvido via DI a cada tick. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | Todas as citações desta spec (`WeatherConfig.cs:23,69`; `SeasonalEventService.cs:307-312,314`; `WeatherGenerator.cs:77-82`; `ConfigServer.cs:22-45`; `WeatherController.cs:27,33-58`; `Season.cs`; `ESeason.cs`; `HostGameController.cs:391-396,530-538`; `ClientGameController.cs:209,504-509`; `App.cs:68-71,92-122`; `SptServerStartupService.cs:25`) foram lidas nesta sessão com linha exata, não recon. |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Não usa skill de personagem como lever. |
| 11 | Pacote FIKA próprio: envelope/registro/airbag — AP-11 | N/A | Item não introduz nenhum pacote de rede — é puramente server-side. A consistência entre peers FIKA vem de cada instalação ler o **mesmo** `season-cycle.json`, não de um pacote em raid (contraste explícito com o design do item 001). |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-10 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-10 | Review técnica 01 (5 achados: 1 🔴, 2 🟡, 2 🟢) — decisões do usuário: PA-01-01 aceito (try/catch + null-guard em `Apply()`/`OnLoad()`); PA-01-02 caminho alternativo (sem hot-reload — usuário reinicia o SPT Server manualmente ao editar `.json`, mesmo fluxo dos outros mods); PA-01-03 rejeitado (manipulação de relógio fora de escopo, decisão consciente do usuário); PA-01-04 aceito com modificação (pesos de clima **não** travam com estação fixa — `Apply()` agora sobrescreve a entrada da estação fixa com os pesos da estação cíclica corrente a cada tick); PA-01-05 aceito (`Enum.TryParse`/`Enum.Parse` com `ignoreCase: true`). Todos os 5 achados aplicados diretamente na spec técnica (§5, §7, §4, §9).
| 2026-09-11 | `/code-mod` executado — checklist §8 100% concluído. `modded/*` reestruturado em `Client/`+`Server/`; 6 arquivos novos do lado servidor; `dotnet build` manual validado (0 erros/0 avisos nos dois projetos). 3 ajustes descobertos durante o build além dos stubs da spec (documentados inline no checklist §8): `CopyToOutputDirectory` no `.csproj`, membros abstratos faltando em `SeasonCycleModMetadata`, e sintaxe de comentário do `season-cycle.json`. Ver `002-...-05-asbuild.md`. |
| 2026-09-11 | `/code-review` 01 (3 achados: 1 🔴, 1 🟡, 1 🟢) + `/apply-code-review` — todos aplicados: CR-01-01 (bug real — `TypePriority = OnLoadOrder.Database` evita que a primeira raid após restart use a estação errada por causa do cache de previsão de clima do SPT), CR-01-02 (parse defensivo por preset), CR-01-03 (captura local de `WeatherConfig`). Server v1.0.0 → 1.0.1. |
| 2026-09-11 | Correção pós-entrega a pedido do usuário: `Resources/Configs/` → `Config/` (singular, sem o segmento `Resources/`) — ver nota após o stub JSON em §5. `dotnet build` revalidado, 0 erros/0 avisos. |
