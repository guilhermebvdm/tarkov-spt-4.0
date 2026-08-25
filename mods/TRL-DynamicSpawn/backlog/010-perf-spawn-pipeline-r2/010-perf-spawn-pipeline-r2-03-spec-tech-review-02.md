# 010 — perf-spawn-pipeline-r2 · Review Técnica 02

**Mod:** TRL-DynamicSpawn
**Spec técnica revisada:** [010-perf-spawn-pipeline-r2-02-spec-tech.md](010-perf-spawn-pipeline-r2-02-spec-tech.md)
**Data:** 2026-08-23

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-02-MM` (review 02, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3

**Contexto consultado:** [review 01](010-perf-spawn-pipeline-r2-03-spec-tech-review-01.md) (9 pontos aceitos e aplicados em 2026-08-23); memória do mod inalterada (snapshot 2026-08-16; nenhuma pendência afeta). Dump do EFT no checkout principal (`C:\Repos\spt\tarkov-spt-4.0\references\eft-decompiled\Assembly-CSharp\`) + Fika (`references/fika-plugin/Fika.Core/`). Esta rodada **reverifica cada resolução** da review 01 contra o dump e registra só o que restou.

## Pontos da review 01 fechados

- ✅ PA-01-01 resolvido na spec — fechado. `PmcMatches(InfoClass, ProfileInfoSettingsClass, WildSpawnType)` bate com `EFT/Profile.cs:632` (`readonly InfoClass Info`), `InfoClass.cs:123` (`ProfileInfoSettingsClass Settings`), `ProfileInfoSettingsClass.cs:7/:9` (`Role`, `BotDifficulty`). Compila.
- ✅ PA-01-02 resolvido na spec — fechado. §2/§9 check 3 listam os três callers; AC-X6 cobre o Halloween.
- ✅ PA-01-03 resolvido na spec — fechado. Verificado a fundo (era o ponto com mais risco de resolução errada):
  - `LocalGameStopPatch`: `AccessTools.Method(typeof(LocalGame), nameof(LocalGame.Stop))` sem lista de tipos é **inequívoco** — `LocalGame` (`EFT/LocalGame.cs:24`, classe concreta) declara um único `Stop` (`:357`, override do virtual `BaseLocalGame-1.cs:1018`); `AbstractGame` não tem `Stop(` (grep: 0); reflection devolve só o override mais derivado, sem `AmbiguousMatchException`.
  - `CoopGameStopPatch`: `Fika.Core.Main.GameMode.CoopGame` é o namespace real (`CoopGame.cs:37`); classe `public sealed` (`:42`) — Harmony patcheia método de classe `sealed` normalmente (o que importa é o IL do método, não a herança). A assinatura `Stop(string, ExitStatus, string, float delay = 0f)` (`:718`) casa com `new[] { typeof(string), typeof(ExitStatus), typeof(string), typeof(float) }` — o parâmetro com default continua sendo `float` na lista de tipos; `ExitStatus` é `EFT.ExitStatus` (`EFT/ExitStatus.cs:1-3`), coberto pelo `using EFT;` já presente em `RaidLifecyclePatches.cs`. `AccessTools.TypeByName` varre os assemblies carregados; `Fika.Core` carrega antes do mod pela `BepInDependency("com.fika.core", SoftDependency)` (`Plugin.cs:13`).
  - **No setup Fika (host, headless e solo-com-Fika), `LocalGame` nunca é instanciado:** `TarkovApplication_LocalGameCreator_Patch.cs:192` substitui a criação por `CoopGame.Create(...)` (`CoopGame.cs:107`, `smethod_0<CoopGame>`). E `CoopGame.Stop` (`:718-836`) **não chama `base.Stop`** — encerra via `ExitManager` (`:811-818`) e `HostGameController.StopBotsSystem(false)` (`:776`). Isso explica definitivamente por que o patch no genérico `BaseLocalGame<>.Stop` foi inerte na V1 (não é só o problema de genérico fechado: o override do Fika nunca passa pela base). O prefix no próprio `CoopGame.Stop` dispara independentemente disso ✓. Ver PA-02-03 para a expectativa da V2.
  - O prefix de `ActivateBotsWithoutWave` continua válido no Fika host: `HostGameController.cs:55` cria o `NonWavesSpawnScenario` com o mesmo `BotsController`, `:650` chama `Run()`, e o `NonWaveSpawnScenario_Patch` do Fika só habilita o cenário no server (`IsServer`) — exatamente o lado em que o mod atua.
- ✅ PA-01-04 resolvido na spec — fechado. AC-X5 declarado; `PmcMatches` simplificado. Ressalva de comentário em PA-02-01.
- ✅ PA-01-05 resolvido na spec — fechado. `StopCoroutine(_activeWaveCoroutine)` chamado de dentro de `SpawnHordeLoop` (outra coroutine do mesmo `MonoBehaviour`) é padrão Unity válido — a coroutine chamadora não é afetada; chamar `StopCoroutine` com um `Coroutine` já concluído é no-op (o campo nunca é zerado ao terminar, `:413` só sobrescreve), então o `if != null` não precisa de guarda extra. O mod já faz exatamente isso em `:411`.
- ✅ PA-01-06 resolvido na spec — fechado. Ordem "flags primeiro, depois `Instance == null`" correta; `Instance.StopAllCoroutines()` em componente vivo ✓; no componente destruído o early-return evita `MissingReferenceException`.
- ✅ PA-01-07 resolvido na spec — fechado. `_sptQueueClearedThisRaid` como campo de instância é por raid de fato: `DynamicSpawnManagerPatch.cs:32-35` evita duplicata no mesmo `GameWorld` e `:60` faz `gameWorld.gameObject.AddComponent<DynamicSpawnManager>()` a cada raid (o `GameWorld` é destruído no fim — `GameWorld.cs:2111`). Limpeza após o check de humano ✓.
- ✅ PA-01-08 resolvido na spec — fechado nas §1/§2/§9. Sobras em comentários de stub e na §6 → PA-02-02.
- ✅ PA-01-09 resolvido na spec — fechado.

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-02-01 | C — Lógica | 🟢 | Comentário de `PmcMatches` cita `sptUsec`/`sptBear`, que não existem em `WildSpawnType` (0.16.9) | ✅ Resolvido |
| PA-02-02 | A — Gap | 🟢 | Sobras das resoluções: `:157`, "public property", "1×/warmup", "finally não roda" ainda aparecem em stubs, §4/§6/§8 e corner cases da 01-spec | ✅ Resolvido |
| PA-02-03 | A — Gap | 🟢 | Expectativa da V2 no setup Fika: a fonte logada será `CoopGame.Stop`, nunca `LocalGame.Stop` (`LocalGame` não é instanciado com Fika) | ✅ Resolvido |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-02-01 · C — Lógica · 🟢 Menor · ✅ Resolvido em 2026-08-23

**Comentário de `PmcMatches` cita `sptUsec`/`sptBear`, que não existem em `WildSpawnType`**

**Problema:** o stub da §5 diz `return info.Side == wantedSide || st.Role == requested;   // cobre sptUsec/sptBear (Side) e pmcUSEC/pmcBEAR (Role)`. Em `EFT/WildSpawnType.cs` (0.16.9) os membros PMC são `pmcBEAR = 51` e `pmcUSEC = 52`; não há **nenhum** membro com prefixo `spt` (grep case-insensitive: 0). `sptUsec`/`sptBear` eram os nomes do SPT 3.x — o código atual do mod já não os usa (a heurística `ToString().Contains("usec")` de `Patches.cs:829-830` era o resquício, e cai com este item). O código compila (é só comentário), mas o comentário documenta um caso que não existe e pode induzir o `/code-mod` a "restaurar" a heurística de string.

**Por que importa:** AP-09 (membro inexistente citado como fato) e rastreabilidade: o comentário vai para o código-fonte.

**Sugestão:** trocar o comentário por `// Side covers any USEC/BEAR profile regardless of Role; Role covers pmcUSEC/pmcBEAR (WildSpawnType.cs:55-56). No spt* roles exist in 0.16.9 — the old ToString().Contains() heuristic is dropped on purpose.`

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** comentário do stub trocado (cita `WildSpawnType.cs`, `pmcUSEC(52)/pmcBEAR(51)`, sem `spt*`).

### PA-02-02 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-08-23

**Sobras das resoluções da review 01 em stubs, §4/§6/§8 e corner cases da 01-spec**

**Problema:** as correções entraram nas §1/§2/§9, mas trechos que o `/code-mod` copia literalmente ainda carregam o texto antigo:
- §5 `SpawnGatePatches.cs`, comentário XML: "(NonWavesSpawnScenario.cs:157)" → `EFT/NonWavesSpawnScenario.cs:160`; linha `var role = bp.WildSpawnType_0; // ref: BotProfileDataClass.cs (public property, …)` → é campo público (`:16`).
- §5 `ChooseProfilePatch`: `var side = __instance.Side; // ref: BotProfileDataClass.cs:87 (Side / WildSpawnType_0 / BotDifficulty_0 usados pelo vanilla)` → citar `:16/:19/:43` e a nota "lifted ==, sem `.Value`" que a §2 já tem.
- §4 (`DynamicSpawnManager.cs`: "`ClearSptQueue` 1×/warmup (`:392` → antes do `while`)"), §6 ("[1ª vez por warmup] ClearSptQueue()" e ":157") e §8 ("`ClearSptQueue` 1×/warmup") contradizem a §1/§5 (c), que agora dizem **1× por raid, depois do check de humano, dentro do `while`**.
- §6 "[fim de raid]" só mostra o caminho `GameWorld.OnDestroy`; falta a linha `LocalGame.Stop / CoopGame.Stop → OnRaidEnd(...) → StopLoop() · StopSpawnLoops()` (sem `ForceRefresh`, que é exclusivo do `OnDestroy` — PA-01-01 do 009).
- 01-spec, corner cases: "passa a acontecer 1× por aquecimento" → 1× por raid; "`IsGeneratingDynamicWave` forçado para `false` (o `finally` da coroutine não roda quando ela é parada)" → a razão correta (PA-01-06) é o reset defensivo com `try/finally` sem `yield`.

**Por que importa:** o `/code-mod` segue os stubs e o checklist §8; um dev que leia "1×/warmup" no §8 implementa a semântica rejeitada na review 01.

**Sugestão:** aplicar as cinco substituições acima literalmente (texto já dado), mantendo §1/§2/§5 (c)/§9 como estão.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** as cinco substituições aplicadas (§4, §5 ×2, §6 incl. caminho `LocalGame.Stop | CoopGame.Stop`, §8; 01-spec corner cases ×2).

### PA-02-03 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-08-23

**Expectativa da V2 no setup Fika: a fonte logada será `CoopGame.Stop`, nunca `LocalGame.Stop`**

**Problema:** §9 check 1 diz "a V2 confere no log a fonte `LocalGame.Stop`/`CoopGame.Stop`" e a 01-spec AC-X4 "a V2 confere a fonte no log". Com o Fika instalado (o setup medido no baseline/V1, cf. `IsFikaClient`/headless no código), **`LocalGame` nunca é criado**: `TarkovApplication_LocalGameCreator_Patch.cs:192` → `CoopGame.Create` (`CoopGame.cs:107`). Logo `LocalGameStopPatch` fica registrado mas inerte nesse setup (inofensivo), e o único hook cedo que pode disparar é `CoopGameStopPatch`. Além disso, `CoopGame.Stop` não chama `base.Stop` (`:718-836`, encerra via `ExitManager` `:811-818`) — evidência de que qualquer patch na base (genérico ou não) seria inerte no Fika; a spec ainda atribui a inércia da V1 só ao "genérico fechado".

**Por que importa:** sem a expectativa explícita, um log com `Raid end hook fired (CoopGame.Stop)` e **zero** `LocalGame.Stop` pode ser lido como "o patch `LocalGame` não dispara → remover" — decisão errada (ele é o caminho do SPT sem Fika). E `Enable()` do SPT lança `PatchException` se `GetTargetMethod()` devolver `null` (`AbstractPatch.cs:110-113`) — o guard `if (CoopGameStopPatch.TargetType != null)` do stub é obrigatório, não opcional; vale marcá-lo assim no §8.

**Sugestão:** §9 check 1 e §2 (linha do `LocalGame.cs:357`): acrescentar "com Fika instalado, `LocalGame` não é instanciado (`TarkovApplication_LocalGameCreator_Patch.cs:192` → `CoopGame.Create`) e `CoopGame.Stop` não chama `base.Stop` (`CoopGame.cs:718-836`) — fonte esperada na V2: `CoopGame.Stop` (host/headless) ou `LocalGame.Stop` (SPT puro); ausência de **ambas** é que indica patch inerte". §8: "`CoopGameStopPatch` só com `TargetType != null` (senão `Enable()` lança `PatchException`, `AbstractPatch.cs:110-113`)". 01-spec AC-X4: mesma expectativa.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** §5 (comentário do registro no `Plugin.cs`: guard obrigatório + `PatchException`), §6 e 01-spec AC-X4 registram a expectativa "fonte `CoopGame.Stop` com Fika; `LocalGame.Stop` só em SPT puro; ausência de ambas = inerte".

---

## Verificações sem achado (registro desta rodada)

- (a) `CoopGame.Stop` assinatura/namespace/`sealed`/default param — ✓ (detalhes em PA-01-03 acima).
- (b) `_sptQueueClearedThisRaid` por raid — ✓ (`DynamicSpawnManagerPatch.cs:32-35/:60`).
- (c) `StopCoroutine` de dentro de coroutine do mesmo componente — ✓ sem pitfall; no-op em coroutine concluída.
- (d) `StopSpawnLoops` flags-first — ✓.
- (e) `PmcMatches` vs `:825-832`: semântica preservada para todos os perfis PMC reais (Side Usec/Bear **ou** Role pmcUSEC/pmcBEAR); o que cai é (i) o degrau "qualquer perfil" (AC-X5, declarado) e (ii) a heurística de string para roles `spt*` que **não existem** em 0.16.9 (PA-02-01, só comentário).
- `IsGeneratingDynamicWave = false` e `_nextWaveTime = 0f` no ramo de pausa — inofensivos; HUD mostra `0s` (`Mathf.Max(0, …)`, `:1536`).
- Guest Fika: `NonWaveSpawnScenario_Patch` do Fika desliga o cenário no cliente (`IsServer == false`) — o prefix novo nem é alcançado; `FikaHelper.IsClient()` no topo é redundante e correto.
