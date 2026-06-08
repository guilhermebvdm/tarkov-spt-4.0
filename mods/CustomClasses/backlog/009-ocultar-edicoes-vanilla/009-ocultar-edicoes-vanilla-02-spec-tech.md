# 009 — Ocultar edições vanilla no launcher · Spec Técnica

**Mod:** CustomClasses
**Slug:** 009-ocultar-edicoes-vanilla
**Criado:** 2026-06-07

> Mod **server-side** puro (SPT 4.0). O launcher monta a lista de edições em `LauncherController.Connect()` filtrando por `CoreConfig.Features.CreateNewProfileTypesBlacklist`. Basta **adicionar as keys das edições vanilla a essa blacklist** no load — config-driven (JSON), sem recompilar.

## 1. Estratégia

`LauncherController.Connect()` faz `GetProfileTemplates().Where(p => !CoreConfig.Features.CreateNewProfileTypesBlacklist.Contains(p.Key))` ([LauncherController.cs:41](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/LauncherController.cs#L41)). A `Key` é o nome da edition (= a chave em `GetProfileTemplates`, a mesma que injetamos pras classes). Então um `IOnLoad` lê uma lista de edições a ocultar (config JSON, default = 7 vanilla) e as **adiciona** ao `HashSet` `CreateNewProfileTypesBlacklist` via `configServer.GetConfig<CoreConfig>()`. `HashSet.Add` = idempotente. Não mexe em template nem em perfil → **perfis já criados continuam carregando** (a blacklist só filtra a lista de criação).

## 2. Pontos de referência (SPT server source)

| Símbolo | Arquivo | Uso |
|---|---|---|
| `LauncherController.Connect()` filtra por blacklist | [LauncherController.cs:41](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/LauncherController.cs#L41) | onde a edition é escondida (por `Key`) |
| `configServer.GetConfig<CoreConfig>()` | [LauncherController.cs:30](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/LauncherController.cs#L30) | obter o CoreConfig (mesma instância singleton) |
| `CoreConfig.Features.CreateNewProfileTypesBlacklist` (`HashSet<string>`) | [CoreConfig.cs:267](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Spt/Config/CoreConfig.cs#L267) | a blacklist a popular |
| `ModHelper.GetJsonDataFromFile<T>` | (ref. SkillDistribution `SkillDisctributionMod.cs`) | ler o JSON de config do mod |
| `IOnLoad` / `OnLoadOrder` | (mod 001/002) | hook de load |

## 3. Novas propriedades F12

Nenhuma (server-side).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Server/config/hidden-editions.jsonc` | CRIAR | `{ "hide": [ "Standard", … ] }` — default com as 7 vanilla. |
| `modded/Server/HiddenEditionsConfig.cs` | CRIAR | DTO record (`Hide` = List<string>). |
| `modded/Server/HiddenEditionsLoader.cs` | CRIAR | `IOnLoad`: lê o config + adiciona as keys à `CreateNewProfileTypesBlacklist`. |
| `scripts/build-class-jsons.js` | (n/a) | — não afeta as classes. |

> Default da config: `Standard`, `Left Behind`, `Prepare To Escape`, `Edge Of Darkness`, `Unheard`, `Tournament`, `SPT Easy start`. **Mantidos** (não na lista): `SPT Developer`, `SPT Zero to hero` + todas as classes do mod (que nunca entram na blacklist).

## 5. Stubs de código

### HiddenEditionsConfig.cs

```csharp
using System.Text.Json.Serialization;

namespace CustomClasses;

/// <summary>Config do item 009: edições (por nome/Key) a ocultar na criação de perfil do launcher.</summary>
public sealed record HiddenEditionsConfig
{
    [JsonPropertyName("hide")]
    public List<string>? Hide { get; init; }
}
```

### HiddenEditionsLoader.cs

```csharp
using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;                 // IOnLoad, OnLoadOrder
using SPTarkov.Server.Core.Helpers;            // ModHelper
using SPTarkov.Server.Core.Servers;            // ConfigServer
using SPTarkov.Server.Core.Models.Spt.Config;  // CoreConfig
using SPTarkov.Server.Core.Models.Utils;       // ISptLogger

namespace CustomClasses;

/// <summary>
///     Item 009: oculta edições vanilla da tela de criação de perfil do launcher, adicionando suas keys
///     a CoreConfig.Features.CreateNewProfileTypesBlacklist (LauncherController.cs:41). Config-driven.
///     Não toca templates/perfis → perfis já criados seguem jogáveis.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class HiddenEditionsLoader(
    ModHelper modHelper,
    ConfigServer configServer,
    ISptLogger<HiddenEditionsLoader> logger
) : IOnLoad
{
    public Task OnLoad()
    {
        var configPath = System.IO.Path.Combine(
            modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()), "config");
        var cfg = modHelper.GetJsonDataFromFile<HiddenEditionsConfig>(configPath, "hidden-editions.jsonc");

        var hide = cfg?.Hide;
        if (hide is null || hide.Count == 0)
        {
            logger.Info("[CustomClasses] hidden-editions: nenhuma edição a ocultar.");
            return Task.CompletedTask;
        }

        var blacklist = configServer.GetConfig<CoreConfig>().Features.CreateNewProfileTypesBlacklist;   // ref: CoreConfig.cs:267
        var added = 0;
        foreach (var key in hide)
        {
            if (!string.IsNullOrWhiteSpace(key) && blacklist.Add(key.Trim()))
            {
                added++;
            }
        }

        logger.Info($"[CustomClasses] {added} edição(ões) vanilla ocultada(s) do launcher.");
        return Task.CompletedTask;
    }
}
```

### config/hidden-editions.jsonc

```jsonc
{
  // Edições (pelo nome exato/Key) ocultadas na criação de perfil do launcher.
  // Mantidos automaticamente: SPT Developer, SPT Zero to hero, e todas as classes do mod.
  "hide": [
    "Standard",
    "Left Behind",
    "Prepare To Escape",
    "Edge Of Darkness",
    "Unheard",
    "Tournament",
    "SPT Easy start"
  ]
}
```

## 6. Fluxo de dados

```
config/hidden-editions.jsonc (.hide[])
  → HiddenEditionsLoader.OnLoad (PostDBModLoader+1)
      → configServer.GetConfig<CoreConfig>().Features.CreateNewProfileTypesBlacklist.Add(key)  (CoreConfig.cs:267)
  → [launcher connect] LauncherController.Connect → Editions = templates.Where(!blacklist.Contains(Key))  (LauncherController.cs:41)
  → launcher mostra só as não-ocultas (+ classes do mod)
```

## 7. Riscos e dependências

- **Ordem:** a blacklist é lida pelo `LauncherController` em runtime (no connect), depois de todo o load → qualquer `IOnLoad` serve. `PostDBModLoader+1` (mesma faixa do nosso loader) é seguro.
- **Keys exatas:** os nomes têm que bater com as Keys de `GetProfileTemplates` (`"Edge Of Darkness"`, `"SPT Easy start"`, etc. — com espaços/caixa exatos, vistos em `templates/profiles.json`). Key errada = no-op silencioso.
- **Não ocultar** `SPT Developer`/`SPT Zero to hero`/classes do mod (não estão na lista default).
- **Perfis existentes:** intactos — a blacklist só filtra a lista de criação (não remove templates nem quebra saves). Cobre o corner case da spec.
- **Launcher v2:** confirmar se `LauncherV2Controller` também respeita a mesma blacklist (provável, mesma CoreConfig) — **TODO confirmar** no review/code-mod.
- **Compartilha o CoreConfig** com o resto do SPT — só **adicionamos** keys (não removemos/limpamos), não-destrutivo.

## 8. Checklist de implementação

- [ ] `HiddenEditionsConfig` (DTO) + `config/hidden-editions.jsonc` (default 7 vanilla).
- [ ] `HiddenEditionsLoader` (IOnLoad) — ler config + `blacklist.Add` por key + log.
- [ ] `/compile-mod` — server builda; config copiada.
- [ ] Confirmar `LauncherV2Controller` respeita a blacklist (senão, achar o ponto equivalente).
- [ ] Playtest: launcher mostra só SPT Developer + SPT Zero to hero + classes do mod; vanilla some; perfil já criado com edition oculta ainda carrega.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Spec técnica criada via `/create-technical-spec` (blacklist `CreateNewProfileTypesBlacklist`; IOnLoad config-driven; refs LauncherController/CoreConfig verificadas) |
