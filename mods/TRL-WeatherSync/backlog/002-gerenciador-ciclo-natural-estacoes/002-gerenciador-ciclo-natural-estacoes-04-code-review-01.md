# 002 — Gerenciador Ciclo Natural Estações · Code Review 01

**Mod:** TRL-WeatherSync
**Spec funcional:** [002-gerenciador-ciclo-natural-estacoes-01-spec.md](002-gerenciador-ciclo-natural-estacoes-01-spec.md)
**Spec técnica:** [002-gerenciador-ciclo-natural-estacoes-02-spec-tech.md](002-gerenciador-ciclo-natural-estacoes-02-spec-tech.md)
**Asbuild:** [002-gerenciador-ciclo-natural-estacoes-05-asbuild.md](002-gerenciador-ciclo-natural-estacoes-05-asbuild.md)
**Data:** 2026-09-11

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🔴 Bloqueador | Cache de previsão de clima primed no boot ANTES do nosso `OverrideSeason` — 1ª(s) raid(s) após restart usam a estação errada | ✅ Aplicado |
| CR-01-02 | B — Bug latente | 🟡 Médio | Um nome de `WeatherPreset` inválido no `.json` aborta a escrita de TODOS os pesos, não só o inválido | ✅ Aplicado |
| CR-01-03 | F — Melhoria opcional | 🟢 Menor | `WeatherConfig` (propriedade `=>`) reavalia `GetConfig<T>()` várias vezes por `Apply()` | ✅ Aplicado |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### CR-01-01 · B — Bug latente · 🔴 Bloqueador · ✅ Aplicado em 2026-09-11

**Cache de previsão de clima do servidor é gerado no boot ANTES do nosso `OverrideSeason` ser aplicado — a(s) primeira(s) raid(s) após cada restart do SPT Server usam a estação real do calendário, não a do ciclo do mod**

**Local:** [`mods/TRL-WeatherSync/modded/Server/SeasonCycleUpdater.cs:13-18`](../../modded/Server/SeasonCycleUpdater.cs#L13)

**Problema:** `SeasonCycleUpdater` não declara `TypePriority` no `[Injectable(...)]` (`SeasonCycleUpdater.cs:13`):

```csharp
[Injectable(InjectionType.Singleton)]
public class SeasonCycleUpdater(...) : IOnLoad, IOnUpdate
```

Rastreei o caminho real de quem consome `WeatherConfig.OverrideSeason` no boot do servidor SPT:

1. `GameCallbacks` é `[Injectable(TypePriority = OnLoadOrder.GameCallbacks)]` (valor `300000`) e seu `OnLoad()` chama `gameController.Load()` ([`GameCallbacks.cs:14-28`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Callbacks/GameCallbacks.cs#L14)).
2. `GameController.Load()` chama `postDbLoadService.PerformPostDbLoadActions()` ([`GameController.cs:505-508`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/GameController.cs#L505)).
3. `PerformPostDbLoadActions()` chama `seasonalEventService.GetActiveWeatherSeason()` e imediatamente `raidWeatherService.GenerateFutureWeatherAndCache(currentSeason)` ([`PostDbLoadService.cs:126-127`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/PostDbLoadService.cs#L126)) — isso **pré-gera e armazena em cache** (`RaidWeatherService.WeatherForecast`, campo de instância) até `WeatherConfig.Weather.GenerateWeatherAmountHours` horas de clima futuro, usando a estação resolvida **naquele exato momento**.
4. O endpoint HTTP que o FIKA realmente chama pra pedir clima de raid (`_backendSession.WeatherRequest()`, [`HostGameController.cs:536`](../../../../references/fika-plugin/Fika.Core/Main/GameMode/HostGameController.cs#L536)) bate em `/client/localGame/weather` (confirmado em [`Class308.cs:1022-1034`](../../../../references/eft-decompiled/Assembly-CSharp/Class308.cs#L1022)) — que no servidor é `WeatherController.GenerateLocal()`, que chama `raidWeatherService.GetUpcomingWeather()` ([`WeatherController.cs:65-72`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/WeatherController.cs#L65)).
5. `GetUpcomingWeather()` só regenera o cache se ele estiver **totalmente vazio** para o horário atual (`ValidateWeatherDataExists`, [`RaidWeatherService.cs:97-108`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/RaidWeatherService.cs#L97)) — enquanto o cache do passo 3 ainda tiver entradas futuras, ele é reaproveitado **sem reconsultar a estação**.

Ou seja: se `SeasonCycleUpdater.OnLoad()` (sem prioridade explícita) rodar DEPOIS de `GameCallbacks.OnLoad()` (prioridade `300000`) — o que o código vendorizado deste repo não garante que NÃO aconteça (mesma incerteza de ordem já documentada em `PA-01-01` da spec técnica, mas aqui o consumidor é outro) — o passo 3 gera o cache de clima da(s) primeira(s) raid(s) usando a estação real do calendário (`SeasonDates`), não a do `season-cycle.json`. Esse cache só se corrige sozinho quando as `GenerateWeatherAmountHours` horas expirarem.

**Por que importa:** isso é exatamente o cenário que o usuário vai testar primeiro — reiniciar o SPT Server (que ele já faz rotineiramente, inclusive toda vez que edita `season-cycle.json`, `PA-01-02`) e entrar numa raid logo em seguida. Nessa janela, o critério de aceite central do item ("a chuva/neve observada... no inverno é perceptivelmente maior... proporção configurável no `.json`") **não se cumpre** — os pesos de clima usados vêm do calendário real do EFT, não do ciclo do mod, apesar de `WeatherConfig.OverrideSeason` já estar correto (a estação **visual** nativa, que é lida ao vivo em toda chamada, não sofre esse problema — só os PESOS de sol/chuva/nublado, que é o cache afetado).

**Sugestão:** dar a `SeasonCycleUpdater` (e, por segurança, manter `SeasonCycleConfigController`) uma `TypePriority` explícita **menor que `OnLoadOrder.GameCallbacks` (300000)** — por exemplo `OnLoadOrder.Database` (200000, constante já existente em `OnLoadOrder.cs`, evita inventar um número mágico):

```csharp
[Injectable(InjectionType.Singleton, null, OnLoadOrder.Database)]
public class SeasonCycleUpdater(...) : IOnLoad, IOnUpdate
```

Isso garante `SeasonCycleUpdater.OnLoad()` (200000) roda antes de `GameCallbacks.OnLoad()` (300000), que é quem dispara o priming do cache de `RaidWeatherService`. O null-guard de `Apply()` (`PA-01-01`) continua como rede de segurança pro caso (não comprovado, mas também não descartado) de `SeasonCycleConfigController` (100000) não ter terminado antes de `SeasonCycleUpdater` (200000) — mesma prioridade relativa já correta hoje entre os dois.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `mods/TRL-WeatherSync/modded/Server/SeasonCycleUpdater.cs:19` — `[Injectable(InjectionType.Singleton, null, OnLoadOrder.Database)]` (era `[Injectable(InjectionType.Singleton)]`). `dotnet build Server.csproj -c Release` validado: 0 erros/0 avisos.

---

### CR-01-02 · B — Bug latente · 🟡 Médio · ✅ Aplicado em 2026-09-11

**Um nome de `WeatherPreset` inválido em UMA entrada do `.json` aborta a escrita de TODAS as entradas de peso naquele tick**

**Local:** [`mods/TRL-WeatherSync/modded/Server/SeasonCycleUpdater.cs:68-73`](../../modded/Server/SeasonCycleUpdater.cs#L68)

**Problema:**

```csharp
foreach (var (seasonKey, presetWeights) in cfg.WeatherPresetWeight)
{
    WeatherConfig.Weather.WeatherPresetWeight[seasonKey] = presetWeights.ToDictionary(
        kv => Enum.Parse<WeatherPreset>(kv.Key, ignoreCase: true),
        kv => kv.Value);
}
```

`Enum.Parse<WeatherPreset>` (não `TryParse`) lança exceção se `kv.Key` não for `"SUNNY"`/`"RAINY"`/`"CLOUDY"`. Essa exceção escapa do `foreach` inteiro e é pega só pelo `try/catch` externo de `Apply()` ([`SeasonCycleUpdater.cs:50-92`](../../modded/Server/SeasonCycleUpdater.cs#L50)) — então um único typo (ex.: `"SUNY"`) numa única estação faz **nenhuma** das 6 estações ter seus pesos atualizados naquele tick, não só a estação com o typo. `OverrideSeason` já foi setado antes do loop, então a estação visual fica certa, mas os pesos de clima ficam permanentemente presos no valor anterior (o do `weather.json` vanilla, com as chaves `WINTER_START`/`WINTER_END`/`default`) enquanto o `.json` continuar malformado — todo tick repete o mesmo erro, sem nunca se corrigir sozinho (ao contrário de `CR-01-01`, que se corrige com o tempo).

**Por que importa:** o `season-cycle.json` é editado à mão por cada admin de instalação FIKA (exigência de consistência entre peers, spec funcional §"Comportamento desejado") — um typo de digitação é um erro plausível, e o sintoma (pesos de clima nunca aplicam) só aparece no log do servidor, não em nenhum lugar visível ao jogador.

**Sugestão:** trocar `Enum.Parse` por `Enum.TryParse` dentro do loop e pular só a entrada inválida, sem abortar as demais:

```csharp
foreach (var (seasonKey, presetWeights) in cfg.WeatherPresetWeight)
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
    WeatherConfig.Weather.WeatherPresetWeight[seasonKey] = parsed;
}
```

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada, com a interpretação também estendida ao bloco de estação fixa (que tinha o mesmo padrão `Enum.Parse` dentro de `.ToDictionary()`) — extraída pro método `ParseWeatherPresetWeights(seasonKey, presetWeights)`, reusado nos dois pontos em vez de duplicar o loop.
**Aplicação:** `mods/TRL-WeatherSync/modded/Server/SeasonCycleUpdater.cs` — novo método privado `ParseWeatherPresetWeights` (usa `Enum.TryParse` por entrada + log de warning por preset inválido); os dois usos de `.ToDictionary(kv => Enum.Parse<WeatherPreset>(...))` substituídos por chamadas a ele. `dotnet build` validado: 0 erros/0 avisos.

---

### CR-01-03 · F — Melhoria opcional · 🟢 Menor · ✅ Aplicado em 2026-09-11

**`WeatherConfig` (propriedade `=>`) reavalia `configServer.GetConfig<WeatherConfig>()` a cada acesso — ~8 chamadas por `Apply()`**

**Local:** [`mods/TRL-WeatherSync/modded/Server/SeasonCycleUpdater.cs:24`](../../modded/Server/SeasonCycleUpdater.cs#L24)

**Problema:**

```csharp
private WeatherConfig WeatherConfig => configServer.GetConfig<WeatherConfig>();
```

É uma propriedade calculada (`=>`), então cada leitura de `WeatherConfig` dentro de `Apply()` (uma vez em `WeatherConfig.OverrideSeason = season;`, mais uma vez por iteração do `foreach` de pesos — tipicamente 6 — mais uma vez no bloco de estação fixa) reexecuta `ConfigServer.GetConfig<T>()`, que faz um scan LINQ (`Enum.GetValues<ConfigTypes>().Where(...)`) mais um lookup em dicionário a cada chamada ([`ConfigServer.cs:35-45`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Servers/ConfigServer.cs#L35)), mesmo a referência retornada sendo sempre a mesma instância (cache estático interno do `ConfigServer`).

**Por que importa:** não é um bug — `Apply()` roda só a cada ~5s (`App.cs:92-122`), não é hot path — mas é trabalho redundante fácil de evitar, e a re-leitura repetida dificulta ligeiramente a leitura do método (não fica óbvio que é sempre o mesmo objeto).

**Sugestão:** capturar `WeatherConfig` numa variável local no topo do bloco `try` de `Apply()`:

```csharp
var weatherConfig = configServer.GetConfig<WeatherConfig>();
// ... usar weatherConfig.OverrideSeason / weatherConfig.Weather.WeatherPresetWeight no resto do método
```

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada conforme proposto. `WeatherConfig` capturado como variável local `weatherConfig` no topo do bloco `try` de `Apply()`; a propriedade `private WeatherConfig WeatherConfig => configServer.GetConfig<WeatherConfig>();` foi removida (não é mais usada em nenhum outro lugar da classe).
**Aplicação:** `mods/TRL-WeatherSync/modded/Server/SeasonCycleUpdater.cs` — `var weatherConfig = configServer.GetConfig<WeatherConfig>();` no topo do `try`; todas as referências a `WeatherConfig.*` dentro de `Apply()` trocadas por `weatherConfig.*`. `dotnet build` validado: 0 erros/0 avisos.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-11 | Code review 01 criada via `/code-review` |
| 2026-09-11 | Aplicação de 3 achados via `/apply-code-review` — IDs aplicados: CR-01-01, CR-01-02, CR-01-03 (nenhum rejeitado/pulado). `dotnet build Server.csproj -c Release` validado após cada mudança: 0 erros/0 avisos. |
