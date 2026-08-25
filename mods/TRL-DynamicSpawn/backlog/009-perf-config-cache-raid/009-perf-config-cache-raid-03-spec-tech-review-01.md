# 009 — perf-config-cache-raid · Review Técnica 01

**Mod:** TRL-DynamicSpawn
**Spec técnica revisada:** [009-perf-config-cache-raid-02-spec-tech.md](009-perf-config-cache-raid-02-spec-tech.md)
**Data:** 2026-08-22

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 2 · 🟢 Menores: 3 · ✅ Resolvidos: 5 · Total: 5

**Contexto consultado:** memória do mod — snapshot de 2026-08-16 (v3.2.9); pendências P-ROADMAP-01/04 não afetam este item. Docs canônicos: `spt-antipatterns.md` (AP-01/03/05/09 aplicáveis — todos endereçados na §9). Grafo do mod ausente (`references/graphs/mods/TRL-DynamicSpawn/` não existe) — overrides auditados por grep no dump: `GameWorld.OnDestroy` (único override `ClientGameWorld.cs:219`, chama base em `:222` ✓), `BaseLocalGame<>.Stop` (`LocalGame.cs:357` → base `:362` ✓; `CoopGame.cs:718` do Fika, não verificado — a spec declara isso e usa `OnDestroy` como primário ✓), `OnGameStarted` (`:2584` ✓). Linhas citadas conferem com o dump. Sem `modded/` — fonte é `Client/`; conflitos de patch checados em `Client/Patches/` (só `DynamicSpawnManagerPatch` no mesmo alvo, postfix aditivo).

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | B — Edge Case | 🟡 | `Stop` invalida o cache cedo demais — leitor entre `Stop` e `OnDestroy` refaz HTTP | ✅ Resolvido |
| PA-01-02 | B — Edge Case | 🟡 | Backoff de 30 s pode engolir o fetch one-shot do `DynamicSpawnManager` → raid inteira com `new TRLConfig()` | ✅ Resolvido |
| PA-01-03 | B — Edge Case | 🟢 | Guest Fika inicia o `DespawnLoop` e faz 1 HTTP sem uso | ✅ Resolvido |
| PA-01-04 | A — Gap | 🟢 | Bump `3.3.0` vs. regra do `/compile-mod`; cabeçalho de `PROPRIEDADES.md` está em v3.1.2 | ✅ Resolvido |
| PA-01-05 | C — Lógica | 🟢 | Patch em genérico fechado (`BaseLocalGame<EftGamePlayerOwner>.Stop`) precisa de confirmação in-game | ✅ Resolvido |

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

### PA-01-01 · B — Edge Case · 🟡 Importante — ✅ Resolvido em 2026-08-22

**`BaseLocalGame.Stop` invalida o cache enquanto o mundo ainda existe**

**Problema:** na spec §5, `RaidLifecycle.OnRaidEnd()` faz `StopLoop()` **e** `ForceRefresh()` — e é chamado tanto pelo prefix de `Stop` (`BaseLocalGame-1.cs:1018`) quanto pelo de `OnDestroy` (`GameWorld.cs:2111`). `Stop(..., float delay)` dispara no início do encerramento (extract/morte/MIA) **antes** da cena cair: o `GameWorld` continua vivo por segundos (tela de extração, `delay`). Qualquer leitor de `ServerConfigProvider.Config` nessa janela — overlay com o mapa aberto (`TRLMapBubbleOverlay.cs:209`, por frame), um spawn em voo passando por `Patches.cs:436/564/640` — encontra cache vazio e faz um fetch **novo**, no pior momento (teardown). Como `_raidActive` já virou `false`, o segundo hook (`OnDestroy`) é no-op e não limpa esse fetch tardio: a raid seguinte parte de cache "fresco" do fim da anterior (edições do painel entre raids ainda chegam, porque esse fetch é posterior a qualquer edição feita *durante* a raid — mas AC-M1 conta 2 em vez de 1).

**Por que importa:** AC-M1 (= 1 `getConfig`/raid) e AC-M2 (sem stutter no fim) ficam frágeis justamente no cenário que a V1 vai medir; e o HTTP bloqueante no teardown é o mesmo mecanismo que o item quer eliminar.

**Sugestão:** separar as responsabilidades dos dois hooks. `RaidLifecycle.OnRaidEnd()` passa a só **parar o poller** e marcar `_raidActive = false` (idempotente como está); a **invalidação do cache** sai dele e vai para um `RaidLifecycle.OnWorldDestroyed()` chamado **apenas** pelo prefix de `GameWorld.OnDestroy` (que é o último evento — depois dele não há leitor até a próxima raid, AC-M3). `OnDestroy` chama os dois (`OnRaidEnd()` + `OnWorldDestroyed()`); `Stop` chama só `OnRaidEnd()`. Cada um com seu guard (`_raidActive` / `_cachedConfig != null` já serve como guard do segundo). Atualizar §5 (`RaidLifecycle`, `RaidLifecyclePatches`) e §6 (fluxo).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão *(aplicado autonomamente na spec — correção de engenharia sem mudança de escopo; reverter se discordar)*
- `[ ]` Caminho alternativo: _________________

**Resolução:** spec §5/§6 atualizadas — `OnRaidEnd()` (Stop + OnDestroy) só para o poller; `OnWorldDestroyed()` (só OnDestroy) invalida o cache.

### PA-01-02 · B — Edge Case · 🟡 Importante — ✅ Resolvido em 2026-08-22

**Backoff de 30 s pode engolir o fetch one-shot do `DynamicSpawnManager`**

**Problema:** a spec §5 troca `RequestHandler.GetJson` em `FetchServerConfigAndStart` (`DynamicSpawnManager.cs:67`) por `ServerConfigProvider.ConfigJson`. `ConfigJson` passa por `EnsureFetched()`, que **respeita o backoff**: se um leitor anterior (ex.: `DisableVanillaWavesPatch`, `Patches.cs:436`, que roda durante o setup do `BotsController` — antes de `OnGameStarted`) tentou e falhou há < 30 s (server acordando, latência de boot, rota temporariamente indisponível), o manager recebe `null` **sem tentar**, lança, cai no `catch` e fica com `_serverConfig = new TRLConfig()` **pela raid inteira** — não há retry no manager (é one-shot). Hoje, os dois fetches são independentes: uma falha no patch não contamina o manager.

**Por que importa:** regressão silenciosa de NR-1/NR-6 num cenário realista (server remoto em produção com latência; headless subindo raid logo após boot): preset, timers por mapa, EliteConfig e custom spawns viram defaults vazios por 30+ minutos. O backoff existe para proteger leitores **periódicos** (AUD-01-03), não para negar a **única** tentativa de um consumidor one-shot.

**Sugestão:** em `ServerConfigProvider`, trocar a propriedade `ConfigJson` por `GetConfigJson(bool bypassBackoff)`: com `true`, `EnsureFetched` ignora a janela de 30 s (ainda registra `_lastAttemptTime` e ainda **não** refaz HTTP se o cache já está populado — hit continua sem custo). `FetchServerConfigAndStart` chama `GetConfigJson(bypassBackoff: true)`; todos os outros leitores ficam na propriedade `Config` (com backoff). Custo máximo: 1 tentativa extra por raid, só no caso de falha anterior — AC-M4 (≤2/min na rota falhando) continua válido porque o manager roda 1× por raid. Atualizar §5 (provider + trecho do manager) e §8.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão *(aplicado autonomamente na spec — idem PA-01-01)*
- `[ ]` Caminho alternativo: _________________

**Resolução:** spec §5 — `GetConfigJson(bool bypassBackoff)` no provider; manager usa `bypassBackoff: true`.

### PA-01-03 · B — Edge Case · 🟢 Menor — ✅ Resolvido em 2026-08-22

**Guest Fika inicia o `DespawnLoop` e paga 1 HTTP por raid sem uso**

**Problema:** §7 admite que no guest o `RaidStartPatch` inicia o loop, que lê `Config` (1 fetch) e só então bate em `IsHostOrSolo()` (`BotDespawnManager.cs:114`) a cada ciclo. No guest nenhum consumidor usa a config (sem `DynamicSpawnManager`, sem spawn) — exceto o overlay do mapa, que lê sob demanda e buscaria sozinho.

**Por que importa:** custo pequeno (1 HTTP + 1 coroutine dormindo por raid), mas é trabalho zumbi por construção (skill `spt-performance-analysis` §4 — "roda no contexto certo?").

**Sugestão:** em `RaidLifecycle.OnRaidStart`, `if (FikaHelper.IsClient()) return;` antes de `StartLoop()` (mesmo critério que `DynamicSpawnManagerPatch.cs:25` já usa). Registrar em §5 e na 01-spec AC "Fika/multiplayer" (guest: poller nem nasce).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão *(usuário: "Prossiga" — aplicado no `/code-mod`)*
- `[ ]` Caminho alternativo: _________________

**Resolução:** `RaidLifecycle.OnRaidStart` retorna cedo em guest Fika.

### PA-01-04 · A — Gap · 🟢 Menor — ✅ Resolvido em 2026-08-22

**Bump de versão e cabeçalho de `PROPRIEDADES.md`**

**Problema:** §4 fixa `3.2.9 → 3.3.0` (minor). A regra do `/compile-mod` (§2 do command) é: patch por default, minor para "feature nova visível". O toggle F12 + a mudança de edição ao vivo (AC-X1) justificam minor — mas o bump é feito **pelo `/compile-mod`**, não pelo `/code-mod`; a spec deve dizer "critério: minor" e deixar o número para o compile (evita dupla contagem se houver build de instrumentação antes). Além disso, [PROPRIEDADES.md:3](../../PROPRIEDADES.md#L3) ainda diz `v3.1.2` — já está stale antes deste item.

**Por que importa:** clareza de processo; cabeçalho errado confunde a validação V1 ("qual build estou medindo?").

**Sugestão:** em §4/§8 trocar "bump 3.2.9 → 3.3.0" por "critério de bump: **minor** (AC-X1 + propriedade F12 nova) — número definido no `/compile-mod`"; no `/code-mod`, ao adicionar a seção `Server Config`, atualizar também o cabeçalho de `PROPRIEDADES.md` para a versão corrente.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão *(usuário: "Prossiga")*
- `[ ]` Caminho alternativo: _________________

**Resolução:** critério minor fica registrado; número aplicado no `/compile-mod`. Cabeçalho do `PROPRIEDADES.md` atualizado no `/code-mod`.

### PA-01-05 · C — Lógica · 🟢 Menor — ✅ Resolvido em 2026-08-22

**Patch em genérico fechado precisa de confirmação in-game (AP-06)**

**Problema:** §2 resolve `Stop` via `typeof(BaseLocalGame<EftGamePlayerOwner>)`. Harmony patcheia métodos de tipo genérico **fechado**, mas com argumento de tipo por referência o código nativo é compartilhado entre instanciações e há relatos de patch que não dispara em alguns runtimes (Mono do Unity incluso). O repo tem precedente apenas do caminho `GameWorld.OnDestroy` (stances removeu o patch de `Stop` por esse motivo — `RaidLifecyclePatches.cs:60-62` do stances). A spec já trata `OnDestroy` como primário, então o risco é de **cobertura**, não de bug.

**Por que importa:** se o patch de `Stop` nunca disparar, o poller só para em `OnDestroy` (segundos depois) — sem impacto funcional, mas a §9 check 1 estaria "✅" com um hook inerte.

**Sugestão:** manter o patch, e na Fase 4 (validação) incluir **1 linha de log `LogInfo` no prefix de `Stop`** (`[TRL-DynamicSpawn] Raid stop hook fired (BaseLocalGame.Stop)`) — 1× por raid, custo zero — e conferir no log da V1 que ela aparece. Se não aparecer, remover o patch e anotar na §9 check 1 que `OnDestroy` é o único hook efetivo (com a evidência de `ClientGameWorld.cs:222`). Adicionar ao checklist de validação V1 do relatório.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão *(usuário: "Prossiga")*
- `[ ]` Caminho alternativo: _________________

**Resolução:** `LogInfo` 1×/raid nos dois prefixes de fim de raid; item adicionado ao checklist V1.
