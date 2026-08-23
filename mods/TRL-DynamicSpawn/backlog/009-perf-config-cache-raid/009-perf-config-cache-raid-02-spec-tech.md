# 009 — perf-config-cache-raid · Spec Técnica

**Mod:** TRL-DynamicSpawn
**Spec funcional:** [009-perf-config-cache-raid-01-spec.md](009-perf-config-cache-raid-01-spec.md)
**Criado:** 2026-08-22T22:26:51-03:00

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.
>
> **Perfil: plano de otimização** (`/optimize-mod-performance` Fase 2). Cada mudança cita o achado `AUD-01-MM` do [relatório de auditoria 01](../../docs/relatorio-auditoria-codigo-01.md) e leva os eixos de custo da skill `spt-performance-analysis` §1. Código-fonte editável deste mod: `Client/` (mod próprio, sem `modded/`). **Os logs de debug existentes não são tocados** (observabilidade da validação V1).

## 1. Estratégia

Três achados, uma causa comum: **trabalho periódico com ciclo de vida global** (skill §4 — "quando esse processamento deveria deixar de existir, e ele realmente deixa?"). A ordem de preferência do harness (corrigir lifecycle → eliminar trabalho desnecessário → configuração → cache → reduzir frequência) se aplica inteira:

| Achado | Problema (resumo) | Mecanismo de impacto | Solução | Classe de fix |
|---|---|---|---|---|
| **AUD-01-01** | `ServerConfigProvider.Config` refaz HTTP **síncrono** + desserialização completa a cada 5 s de TTL, em qualquer call site ([ServerConfigProvider.cs:18-26](../../Client/Helpers/ServerConfigProvider.cs#L18-L26)) | cada miss = round-trip HTTP parado na main thread (server remoto → latência de rede vira frametime) + `TRLConfig` inteiro realocado (churn de GC). Eixos: frequência **~0,1/s agregada** × 1 entidade × **raid inteira** × custo unitário **alto** (I/O + JSON) | cache com **escopo de raid**: fetch-on-miss, sem TTL; invalidado só no fim da raid e sob demanda | cache/reutilização + eliminar trabalho |
| **AUD-01-02** | `DespawnLoop` é `while(true)` num GameObject `DontDestroyOnLoad`, e o primeiro passo do loop é o fetch ([BotDespawnManager.cs:50-54](../../Client/Components/BotDespawnManager.cs#L50-L54)) | polling HTTP + scan fora de raid, **para sempre** (menu/hideout/entre raids). Eixos: 0,2/s × 1 × **vida do processo** | lifecycle de raid: loop **começa** no start hook e **para** nos stop hooks; gate de `GameWorld` como rede de segurança | corrigir lifecycle |
| **AUD-01-03** | `_lastFetchTime` só avança no sucesso ([ServerConfigProvider.cs:26](../../Client/Helpers/ServerConfigProvider.cs#L26)) | com a rota falhando, **toda** leitura vira HTTP — ×call sites por-frame (overlay) e por-spawn. No headless é martelo contínuo | registrar a tentativa também na falha; retry mínimo de 30 s | eliminar trabalho desnecessário |

**Sem Harmony novo para o cache em si** — é refactor de helper estático. O único patch novo é o **par de hooks de fim de raid** (o mod não tem nenhum hoje — AP-01): prefix em `GameWorld.OnDestroy` + prefix em `BaseLocalGame<EftGamePlayerOwner>.Stop`, ambos chamando um `RaidLifecycle.OnRaidEnd()` idempotente. O start hook reaproveita o alvo já patcheado pelo mod (`GameWorld.OnGameStarted`, [DynamicSpawnManagerPatch.cs:16](../../Client/Patches/DynamicSpawnManagerPatch.cs#L16)) num patch **separado**, porque o existente faz `return` cedo para guest Fika e o poller de despawn precisa do start em qualquer papel (ele decide por `IsHostOrSolo()` por dentro, como hoje).

**Alternativas descartadas:**
- *Fetch assíncrono (`Task`/coroutine) mantendo o TTL:* tira o bloqueio mas mantém 111 requisições e o churn de desserialização por raid; obriga a tornar todos os consumidores tolerantes a "config chegando depois". O ganho vem da **frequência**, não do mecanismo.
- *Rota push/websocket do painel web:* mudança server-side (fora do escopo da rodada 1, 100% client) para preservar uma conveniência que custa 111 HTTP/raid. Substituída pelo refresh manual (§3).
- *Matar o GameObject do `BotDespawnManager` a cada raid:* mais invasivo (estáticos `_teleportCooldowns`, `_lastTeleportPositions`, cache de reflection) sem ganho sobre "coroutine com escopo de raid".
- *Gate `Instantiated` a cada 5 s no topo do loop sem hooks:* resolve o HTTP fora de raid, mas mantém polling zumbi (skill §2 FREQ: polling onde existe evento é desperdício por construção) e não cria o stop hook que AUD-01-01 precisa de qualquer forma.

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`EFT/GameWorld.cs:2584`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L2584) `public virtual void OnGameStarted()` | Postfix | Start hook: `RaidLifecycle.OnRaidStart(__instance)` — ignora hideout (`MainPlayer is HideoutPlayer`), inicia o `DespawnLoop` da raid. Alvo **já patcheado** pelo mod (`DynamicSpawnManagerPatch`) — patch novo separado, sem `return` cedo para guest. |
| [`EFT/GameWorld.cs:2111`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L2111) `public virtual void OnDestroy()` | Prefix | Stop hook primário: `RaidLifecycle.OnRaidEnd()`. Override em [`EFT/ClientGameWorld.cs:219`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/ClientGameWorld.cs#L219) **chama `base.OnDestroy()`** (`:222`) — o patch na base dispara para `ClientLocalGameWorld` e para o mundo do Fika. Precedente no repo: DynamicMaps, DiscordRaidMap, stances, Realism patcheiam este mesmo alvo. |
| [`EFT/BaseLocalGame-1.cs:1018`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/BaseLocalGame-1.cs#L1018) `public virtual void Stop(string profileId, ExitStatus exitStatus, string exitName, float delay = 0f)` | Prefix | Stop hook secundário (cobre `Left`/`Killed`/`MIA` antes da cena cair). Tipo é genérico aberto — resolver pelo **fechado** `typeof(BaseLocalGame<EftGamePlayerOwner>)`, que é a base concreta de [`EFT/LocalGame.cs:24`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/LocalGame.cs#L24) (override `:357` chama `base.Stop` em `:362`) e de `Fika.Core/Main/GameMode/CoopGame.cs:42`. Idempotente: se já rodou, no-op. |

Nenhum dos três alvos é hot path (per-raid — skill §1.1 🟢). Os patches têm `try/catch` com log de erro (nunca quebrar o teardown do jogo).

## 3. Novas propriedades F12 (BepInEx)

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `Server Config` | `Reload Server Config` | bool | `false` | — | — | Marque para recarregar agora a configuração do painel web (aplica as edições feitas durante a raid). Desmarca sozinho após recarregar. |

- Semântica: **botão**, não estado. O handler `SettingChanged` só age quando `Value == true`: chama `ServerConfigProvider.ForceRefresh()`, loga `LogInfo` e volta `Value = false` (o segundo `SettingChanged` com `false` é ignorado — sem reentrância). Estado neutro = `false`; nunca persiste `true` no `.cfg` (AP-05).
- **Zero custo por frame** (event-driven) — alternativa `KeyboardShortcut` polled em `Plugin.Update()` foi descartada por ser uma leitura por frame para um evento raro.
- Efeito: o cache é zerado; a **próxima leitura** de `Config` (próximo ciclo do `DespawnLoop`, próximo spawn, próximo frame do overlay com mapa aberto) faz 1 fetch. Fora de raid não há leitor → nenhum HTTP até a próxima raid (que buscaria de qualquer forma).
- Documentar em `PROPRIEDADES.md` (seção nova `Server Config`) e na nota de "edição ao vivo" do README do painel (AC-X1).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `Client/Helpers/ServerConfigProvider.cs` | MODIFICAR | Cache por raid (sem TTL), `ConfigJson` bruto cacheado, retry mínimo 30 s em falha, `ForceRefresh()` como única invalidação. `// ref: AUD-01-01`, `// ref: AUD-01-03` |
| `Client/Helpers/RaidLifecycle.cs` | CRIAR | Estado `_raidActive` + `OnRaidStart(GameWorld)` / `OnRaidEnd()` idempotentes; orquestra provider + poller. `// ref: AUD-01-01/02` |
| `Client/Patches/RaidLifecyclePatches.cs` | CRIAR | 3 `ModulePatch`: `OnGameStarted` postfix, `GameWorld.OnDestroy` prefix, `BaseLocalGame<EftGamePlayerOwner>.Stop` prefix |
| `Client/Components/BotDespawnManager.cs` | MODIFICAR | `Start()` deixa de iniciar o loop; `StartLoop()`/`StopLoop()` estáticos idempotentes; `DespawnLoop` ganha `yield break` quando `GameWorld` some. Corpo do scan/teleporte **intocado**. `// ref: AUD-01-02` |
| `Client/Components/DynamicSpawnManager.cs` | MODIFICAR | `FetchServerConfigAndStart` (`:67`) desserializa a **cópia privada** a partir de `ServerConfigProvider.ConfigJson` em vez de fazer o próprio `GetJson` → 1 HTTP por raid (AC-M1 = 1). Resto do método intocado. `// ref: AUD-01-01` |
| `Client/Helpers/Settings.cs` | MODIFICAR | `ConfigEntry<bool> reloadServerConfig` + handler `SettingChanged` |
| `Client/Plugin.cs` | MODIFICAR | Registrar os 3 patches novos; bump `3.2.9 → 3.3.0` (minor: comportamento de edição ao vivo muda — AC-X1) |
| `PROPRIEDADES.md` | MODIFICAR | Seção `Server Config` + cabeçalho de versão |

## 5. Stubs de código

```csharp
// Client/Helpers/ServerConfigProvider.cs
using System;
using Newtonsoft.Json;
using SPT.Common.Http;
using TRLDynamicSpawn.Models;
using UnityEngine;

namespace TRLDynamicSpawn.Helpers
{
    /// <summary>
    /// Config do servidor com escopo de raid: 1 fetch por raid (fetch-on-miss), invalidação só por
    /// ForceRefresh() — chamada pelos hooks de fim de raid (RaidLifecycle) e pelo toggle F12.
    /// ref: AUD-01-01 (TTL de 5 s removido) · ref: AUD-01-03 (retry mínimo em falha)
    /// </summary>
    public static class ServerConfigProvider
    {
        private const string ConfigRoute = "/trldynamicspawn/getConfig";
        private const float FailedFetchRetrySeconds = 30f;   // ref: AUD-01-03

        private static TRLConfig _cachedConfig;
        private static string _cachedJson;
        private static float _lastAttemptTime = -1000f;      // avança em sucesso E em falha

        /// <summary>JSON bruto da última resposta bem-sucedida (para quem precisa de cópia própria).</summary>
        public static string ConfigJson
        {
            get { EnsureFetched(); return _cachedJson; }
        }

        public static TRLConfig Config
        {
            get { EnsureFetched(); return _cachedConfig; }
        }

        private static void EnsureFetched()
        {
            if (_cachedConfig != null) return;                                    // hit: zero custo além do branch
            if (Time.realtimeSinceStartup - _lastAttemptTime < FailedFetchRetrySeconds) return; // backoff
            _lastAttemptTime = Time.realtimeSinceStartup;                         // registra a tentativa ANTES do I/O

            try
            {
                string json = RequestHandler.GetJson(ConfigRoute);               // síncrono (SPT.Common) — 1× por raid
                if (!string.IsNullOrEmpty(json))
                {
                    _cachedConfig = JsonConvert.DeserializeObject<TRLConfig>(json);
                    _cachedJson = json;
                    Plugin.LogSource?.LogInfo("[TRL-DynamicSpawn] Server config fetched (raid-scoped cache).");
                }
                else
                {
                    Plugin.LogSource?.LogWarning($"[TRL-DynamicSpawn] Empty ServerConfig response; retry in {FailedFetchRetrySeconds}s.");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogWarning($"[TRL-DynamicSpawn] Failed to fetch ServerConfig: {ex.Message} — retry in {FailedFetchRetrySeconds}s.");
            }
        }

        /// <summary>Invalida o cache. Próxima leitura de Config/ConfigJson faz 1 fetch.</summary>
        public static void ForceRefresh()
        {
            _cachedConfig = null;
            _cachedJson = null;
            _lastAttemptTime = -1000f;
        }
    }
}
```

```csharp
// Client/Helpers/RaidLifecycle.cs
using EFT;
using TRLDynamicSpawn.Components;

namespace TRLDynamicSpawn.Helpers
{
    /// <summary>Start/stop de raid idempotentes — AP-01. ref: AUD-01-01, AUD-01-02</summary>
    public static class RaidLifecycle
    {
        private static bool _raidActive;

        // ref: Assembly-CSharp/EFT/GameWorld.cs:2584 (OnGameStarted postfix)
        public static void OnRaidStart(GameWorld gameWorld)
        {
            if (gameWorld == null || gameWorld.MainPlayer == null) return;
            if (gameWorld.MainPlayer is HideoutPlayer) return;   // hideout não é raid
            if (_raidActive) return;                             // Fika pode re-entrar em OnGameStarted
            _raidActive = true;
            BotDespawnManager.StartLoop();
        }

        // ref: Assembly-CSharp/EFT/GameWorld.cs:2111 (OnDestroy) e EFT/BaseLocalGame-1.cs:1018 (Stop)
        public static void OnRaidEnd()
        {
            if (!_raidActive) return;                            // segundo hook = no-op
            _raidActive = false;
            BotDespawnManager.StopLoop();
            ServerConfigProvider.ForceRefresh();                 // próxima raid busca config nova
        }
    }
}
```

```csharp
// Client/Patches/RaidLifecyclePatches.cs
using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using TRLDynamicSpawn.Helpers;

namespace TRLDynamicSpawn.Patches
{
    /// <summary>ref: Assembly-CSharp/EFT/GameWorld.cs:2584 — alvo já patcheado por DynamicSpawnManagerPatch (postfix independente).</summary>
    public class RaidStartPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));

        [PatchPostfix]
        private static void Postfix(GameWorld __instance)
        {
            try { RaidLifecycle.OnRaidStart(__instance); }
            catch (Exception ex) { Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] RaidStartPatch: {ex}"); }
        }
    }

    /// <summary>ref: Assembly-CSharp/EFT/GameWorld.cs:2111 — ClientGameWorld.OnDestroy (:219) chama base (:222).</summary>
    public class GameWorldOnDestroyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnDestroy));

        [PatchPrefix]
        private static void Prefix()
        {
            try { RaidLifecycle.OnRaidEnd(); }
            catch (Exception ex) { Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] GameWorldOnDestroyPatch: {ex}"); }
        }
    }

    /// <summary>ref: Assembly-CSharp/EFT/BaseLocalGame-1.cs:1018 — genérico fechado em EftGamePlayerOwner (LocalGame.cs:24, CoopGame.cs:42).</summary>
    public class BaseLocalGameStopPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(BaseLocalGame<EftGamePlayerOwner>), nameof(BaseLocalGame<EftGamePlayerOwner>.Stop));

        [PatchPrefix]
        private static void Prefix()
        {
            try { RaidLifecycle.OnRaidEnd(); }
            catch (Exception ex) { Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] BaseLocalGameStopPatch: {ex}"); }
        }
    }
}
```

```csharp
// Client/Components/BotDespawnManager.cs — trechos que mudam (ref: AUD-01-02)
public static void Enable()            // inalterado: singleton DontDestroyOnLoad criado em Plugin.Start()
{ /* ... */ }

private void Start()
{
    // AUD-01-02: o loop não nasce mais com o processo — nasce com a raid (RaidLifecycle.OnRaidStart).
}

/// <summary>Idempotente: uma coroutine por raid, nunca duas.</summary>
public static void StartLoop()
{
    if (_instance == null) return;
    if (_instance._despawnRoutine != null) return;
    _teleportCooldowns.Clear();                 // NR-4: era feito em Start() 1× por processo
    _instance._currentLocation = null;
    _instance._despawnRoutine = _instance.StartCoroutine(_instance.DespawnLoop());
}

public static void StopLoop()
{
    if (_instance == null || _instance._despawnRoutine == null) return;
    _instance.StopCoroutine(_instance._despawnRoutine);
    _instance._despawnRoutine = null;
}

private IEnumerator DespawnLoop()
{
    while (true)
    {
        // Rede de segurança: se a raid acabou sem o hook (não deveria), sai sem tocar em HTTP.
        if (!Singleton<GameWorld>.Instantiated) { _despawnRoutine = null; yield break; }

        _serverConfig = ServerConfigProvider.Config;   // agora: hit de cache (branch), não HTTP
        // ... resto do corpo INALTERADO (gates, intervalo, scan, teleporte) ...
    }
}
```

```csharp
// Client/Components/DynamicSpawnManager.cs — FetchServerConfigAndStart, só a linha do fetch (ref: AUD-01-01)
// antes:  string json = RequestHandler.GetJson("/trldynamicspawn/getConfig");
// depois: mesma resposta HTTP, cópia privada (os modificadores de preset abaixo mutam _serverConfig — NR-6)
string json = ServerConfigProvider.ConfigJson;
if (string.IsNullOrEmpty(json)) throw new InvalidOperationException("ServerConfig unavailable (see provider warning).");
_serverConfig = JsonConvert.DeserializeObject<TRLConfig>(json);
// ... resto INALTERADO (o catch existente já faz o fallback `new TRLConfig()`)
```

```csharp
// Client/Helpers/Settings.cs — adição
public static ConfigEntry<bool> reloadServerConfig;

// em Init(), após a seção de overlay:
string serverSection = "Server Config";
reloadServerConfig = config.Bind(serverSection, "Reload Server Config", false,
    new ConfigDescription("Tick to reload the web panel configuration now (applies edits made during the raid). Unticks itself after reloading."));
reloadServerConfig.SettingChanged += (_, __) =>
{
    if (!reloadServerConfig.Value) return;          // o reset abaixo dispara SettingChanged com false → ignorado
    ServerConfigProvider.ForceRefresh();
    Plugin.LogSource?.LogInfo("[TRL-DynamicSpawn] Server config cache cleared by user (F12). Next read will fetch.");
    reloadServerConfig.Value = false;
};
```

```csharp
// Client/Plugin.cs — registro (após os patches existentes)
new RaidStartPatch().Enable();
new GameWorldOnDestroyPatch().Enable();
new BaseLocalGameStopPatch().Enable();
// BepInPlugin version: "3.3.0"
```

## 6. Fluxo de dados

```
[boot]  Plugin.Start → Settings.Init (toggle F12) → BotDespawnManager.Enable() (GameObject vivo, SEM loop)

[raid start]  GameWorld.OnGameStarted (GameWorld.cs:2584)
   ├─ DynamicSpawnManagerPatch (existente) → DynamicSpawnManager.Init → FetchServerConfigAndStart
   │     └─ ServerConfigProvider.ConfigJson ──miss──► RequestHandler.GetJson ──► cache {TRLConfig, json}   ← o ÚNICO HTTP da raid
   │        └─ cópia privada _serverConfig (presets mutam só ela)
   └─ RaidStartPatch (novo) → RaidLifecycle.OnRaidStart → BotDespawnManager.StartLoop()

[raid]  leitores de ServerConfigProvider.Config (DespawnLoop :54, Patches :436/:564/:640, overlay :209, ServerConfig :33)
           → hit de cache: retorno imediato (sem HTTP, sem JSON)
        F12 "Reload Server Config" = true → ForceRefresh() → próxima leitura faz 1 fetch (+1 em AC-M1, manual)
        falha de fetch → _lastAttemptTime avança → próxima tentativa ≥30 s depois (AC-M4 ≤2/min)

[raid end]  BaseLocalGame<EftGamePlayerOwner>.Stop (BaseLocalGame-1.cs:1018)  ─┐
            GameWorld.OnDestroy (GameWorld.cs:2111)                            ─┴─► RaidLifecycle.OnRaidEnd (1ª chamada age, 2ª no-op)
                                                                                     ├─ BotDespawnManager.StopLoop()
                                                                                     └─ ServerConfigProvider.ForceRefresh()
[menu/hideout]  nenhum leitor de Config → zero HTTP (AC-M3)
```

Ordem dentro de `OnGameStarted`: os dois postfixes (existente e novo) são independentes — o cache é fetch-on-miss, então não importa qual roda antes; a primeira leitura (seja do `DisableVanillaWavesPatch` em `:436`, ainda durante o setup do `BotsController`, seja do `FetchServerConfigAndStart`) popula o cache para todos.

## 7. Riscos e dependências

- **Patches existentes em `Client/Patches/`** que tocam o mesmo alvo: `DynamicSpawnManagerPatch` (`OnGameStarted` postfix) e `OnGameStartedPatch` em `OnGameStartPatches.cs` (**não registrado** no `Plugin.cs` — código morto, não conflita). Postfixes múltiplos no mesmo alvo são suportados pelo Harmony; os dois não compartilham estado.
- **Compatibilidade com outros mods** que patcheiam `GameWorld.OnDestroy`: DynamicMaps, DiscordRaidMap, stances, Realism, TRL-PvpMode — todos prefix/postfix aditivos; nenhum cancela o original.
- **Fika:** guest continua sem `DynamicSpawnManager`; `RaidStartPatch` inicia o `DespawnLoop` no guest também, que bate em `IsHostOrSolo()` e dorme (como hoje, mas **sem** o HTTP antes). Headless (`Application.isBatchMode`): `IsClient()` = false → comporta-se como host; o ganho de AUD-01-03 é **maior** lá (rota falhando → ≤2/min). `BaseLocalGame<EftGamePlayerOwner>.Stop` cobre `CoopGame`; não confirmado se `CoopGame.Stop` (`CoopGame.cs:718`) chama `base.Stop` — irrelevante porque `OnDestroy` é o hook primário.
- **Regressão funcional (por achado):**
  - AUD-01-01 — edição ao vivo do painel web deixa de ser automática (AC-X1, declarada). Mitigação: toggle F12 + doc.
  - AUD-01-02 — se `OnGameStarted` não disparar (injeção falha), o poller não nasce: **hoje** ele também não funciona nesse caso (cai em `botsController == null`), só que gastando HTTP. Sem regressão.
  - AUD-01-03 — se a primeira tentativa falhar por latência de boot do server, a config demora até 30 s para chegar (hoje: retentativa imediata a cada leitura). Os consumidores já tratam `null` (NR-5); o `FetchServerConfigAndStart` cai no fallback `new TRLConfig()` como hoje quando o fetch próprio falhava.
- **Ordem de inicialização:** `Settings.Init` antes dos patches (já é assim em `Plugin.Start`); `BotDespawnManager.Enable()` antes da primeira raid (idem). `StartLoop()` com `_instance == null` é no-op defensivo.
- **Estado estático que sobrevive entre raids (inventário):** `ServerConfigProvider._cachedConfig/_cachedJson` (invalidados no stop), `BotDespawnManager._teleportCooldowns` (limpo no start — NR-4), `_lastTeleportPositions`/`_lastTeleportZone` (**já** sobrevivem hoje; fora de escopo — são 6 `Vector3` e uma referência a `BotZone` da raid anterior; registrar como dívida para a rodada 2, não muda custo).

## 8. Checklist de implementação

- [ ] `ServerConfigProvider`: remover TTL; `EnsureFetched()` com `_lastAttemptTime` em sucesso e falha; `ConfigJson`; `ForceRefresh()` limpa os dois. Comentários `// ref: AUD-01-01` / `// ref: AUD-01-03`.
- [ ] `RaidLifecycle.cs` novo (idempotente, ignora hideout).
- [ ] `RaidLifecyclePatches.cs` novo (3 patches, `try/catch`, refs `arquivo.cs:linha`).
- [ ] `BotDespawnManager`: `Start()` vazio, `StartLoop()`/`StopLoop()`, `yield break` no topo do loop; **nenhuma outra linha do loop muda**.
- [ ] `DynamicSpawnManager.FetchServerConfigAndStart`: trocar o `GetJson` por `ServerConfigProvider.ConfigJson` + throw em vazio (catch existente faz o fallback).
- [ ] `Settings.cs`: `reloadServerConfig` + handler. `PROPRIEDADES.md`: seção `Server Config`.
- [ ] `Plugin.cs`: registrar 3 patches; versão `3.3.0`.
- [ ] Grep de sanidade: nenhum `RequestHandler.GetJson("/trldynamicspawn/getConfig")` fora do provider.
- [ ] **Logs de debug existentes intocados** (conferir diff: nenhuma linha `Log*` removida/rebaixada).
- [ ] `/compile-mod` (client only) → deploy com rollback (`TRL-DynamicSpawn.dll` → `.bak-3.2.9`) → validação V1 (Fase 4).

## 9. Conformidade com skills (auto-checklist)

> Preenchido pelo `/create-technical-spec` ANTES de salvar. Cada linha: ✅ com evidência (seção desta spec ou `arquivo:linha`), ou **N/A + razão**. Linha ❌ → a spec não está pronta. Validado pelo `/review-technical-spec`. Taxonomia: [docs/technical/spt-antipatterns.md](../../../../docs/technical/spt-antipatterns.md).

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes (`GameWorld.OnDestroy` + `BaseLocalGame.Stop`) — AP-01 | ✅ | §2 (3 alvos com linha), §5 `RaidLifecycle` (`_raidActive` guard). `OnDestroy` da base disparado via `ClientGameWorld.cs:222` (`base.OnDestroy()`); `Stop` fechado em `EftGamePlayerOwner` (`LocalGame.cs:24`, `:362`). |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | Nenhum patch reage a ação de player; `OnRaidStart` filtra `HideoutPlayer` (§5) e o poller mantém `FikaHelper.IsHostOrSolo()` (`BotDespawnManager.cs:114`, intocado). |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | ✅ | Alvos nomeados (não ofuscados). Overrides: `OnDestroy` → só `ClientGameWorld.cs:219` (chama base). `Stop` → `LocalGame.cs:357` (chama base `:362`) e `CoopGame.cs:718` (Fika; coberto pelo `OnDestroy` primário — §7). `OnGameStarted` → alvo já usado pelo mod. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Nenhum estado do EFT é alterado; só estado estático do mod (`ServerConfigProvider`, `RaidLifecycle`, coroutine própria). §7 inventário de estáticos. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | §6 fluxo; `OnRaidEnd` em `OnDestroy` + `Stop` (extract/morte/MIA); alt-F4 = fim do processo. `StartLoop` idempotente (Fika re-entrada). 01-spec AC "Estado entre raids". |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | ✅ | §3: `Reload Server Config` bool, default `false`, auto-reset, estado neutro `false`, sem reentrância. |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` (sem recursão infinita) — AP-07 | N/A | Nenhum patch re-invoca o alvo; o handler `SettingChanged` tem guard explícito (`if (!Value) return`) para o reset. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca (arma/operação/tela) — AP-08 | ✅ | O cache é **a** mudança: validade = raid atual; invalidado em `OnRaidEnd` (2 hooks) e por `ForceRefresh()`; `DespawnLoop` re-resolve `_currentLocation` por ciclo (código existente `:63-74`). |
| 9 | Todo patch-point reconfirmado no `.cs` do dump (não só no recon); "não existe" conferido no `types-index.json`, nunca num grep vazio — AP-09 | ✅ | Reconfirmados em 2026-08-22 no dump: `GameWorld.cs:2111` (`OnDestroy`), `:2584` (`OnGameStarted`), `BaseLocalGame-1.cs:1018` (`Stop`), `ClientGameWorld.cs:219/222`, `LocalGame.cs:24/357/362`. `EFT.GameWorld` e `EFT.BaseLocalGame\`1` presentes no `types-index.json`. |
| 10 | Skill EFT usada como lever confirmada **não-inerte** — AP-10 | N/A | Item não usa skills do EFT. |
| 11 | Pacote FIKA próprio — AP-11 | N/A | Nenhum `INetSerializable`; nada trafega pela rede do Fika. |

## Histórico

| Data | Evento |
|---|---|
| 2026-08-22 | Spec técnica criada via `/optimize-mod-performance --fase 2` (plano de otimização AUD-01-01/02/03) |
