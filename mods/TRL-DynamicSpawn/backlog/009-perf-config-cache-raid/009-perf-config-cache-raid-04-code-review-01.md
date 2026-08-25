# 009 — perf-config-cache-raid · Code Review 01

**Mod:** TRL-DynamicSpawn
**Spec funcional:** [009-perf-config-cache-raid-01-spec.md](009-perf-config-cache-raid-01-spec.md)
**Spec técnica:** [009-perf-config-cache-raid-02-spec-tech.md](009-perf-config-cache-raid-02-spec-tech.md)
**Asbuild:** [009-perf-config-cache-raid-05-asbuild.md](009-perf-config-cache-raid-05-asbuild.md)
**Data:** 2026-08-22

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · ⏭️ Rejeitados: 1 · Total: 4

**Contexto consultado:** memória do mod (snapshot 2026-08-16, v3.2.9; pendências P-ROADMAP-01/04 não afetam). `spt-antipatterns.md` (AP-01/02/03/05/06/09). Review técnica 01 — PA-01-01..05 aplicados no build (conferidos no código). Commit revisado: `3c16960f`. Grafo do mod ausente — impacto mapeado por grep: os únicos leitores de `ServerConfigProvider.Config` continuam sendo `BotDespawnManager.cs:81`, `DynamicSpawnManager.cs:33`, `TRLMapBubbleOverlay.cs:209`, `Patches.cs:436/564/640`; nenhum `RequestHandler.GetJson("/trldynamicspawn/getConfig")` sobrou fora do provider.

**Conferências de performance (skill `spt-performance-analysis`, checklist):** hit de cache = 1 branch (`ServerConfigProvider.cs:48`); backoff registra a tentativa antes do I/O (`:55`); `DespawnLoop` sai com `yield break` sem `GameWorld` (`BotDespawnManager.cs:73-77`); um `DespawnLoop` por raid (`StartLoop` idempotente `:51-58`); os 3 patches novos são per-raid (🟢 na classe de frequência) com `try/catch`; toggle F12 é event-driven (`Settings.cs:119-125`), sem custo por frame; **nenhuma linha `Log*` pré-existente removida ou rebaixada** (diff conferido) — o fix não trocou custo por bug de lifecycle. Linhas do Assembly citadas nos patches batem com o dump (`GameWorld.cs:2111/:2584`, `BaseLocalGame-1.cs:1018` — única sobrecarga de `Stop`, sem `AmbiguousMatchException`).

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟡 | `_raidActive` preso em `true` impede o poller de nascer nas raids seguintes | ✅ Aplicado |
| CR-01-02 | C — Gap vs. spec | 🟡 | AC-X1 pede a nota de "edição ao vivo" também no README do painel — só `PROPRIEDADES.md` foi atualizado | ✅ Aplicado |
| CR-01-03 | E — Legibilidade | 🟢 | Log "Raid end hook fired" dispara ao sair do hideout (não é raid) | ✅ Aplicado |
| CR-01-04 | F — Melhoria opcional | 🟢 | `OnGameStartPatches.cs` é código morto pré-existente (nunca registrado) | ⏭️ Rejeitado (deferido) |

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

### CR-01-01 · B — Bug latente · 🟡 Médio · ✅ Aplicado em 2026-08-22

**`_raidActive` preso em `true` impede o poller de nascer nas raids seguintes**

**Local:** [`mods/TRL-DynamicSpawn/Client/Helpers/RaidLifecycle.cs:21-23`](../../Client/Helpers/RaidLifecycle.cs#L21)

**Problema:**
```csharp
if (_raidActive) return;                             // Fika may re-enter OnGameStarted
_raidActive = true;
BotDespawnManager.StartLoop();
```
Se uma raid terminar sem **nenhum** dos dois hooks de fim rodar até o fim (exceção dentro de `OnRaidEnd` antes de `_raidActive = false` é impossível hoje, mas um patch que não aplicou + um `OnDestroy` que não disparou por crash parcial da cena deixa o estado preso), `_raidActive` fica `true` e **toda raid seguinte** pula `StartLoop()` — o despawn/teleporte some silenciosamente até reiniciar o jogo. O guard é redundante: `StartLoop()` já é idempotente (`BotDespawnManager.cs:53-54`), e o `DespawnLoop` já se auto-encerra sem `GameWorld` (`:73-77`).

**Por que importa:** falha silenciosa e persistente (NR-3 quebra sem log) num caminho que a spec declarou auto-recuperável ("hook perdido atrasa, não cancela").

**Sugestão:** remover o early-return e deixar a idempotência com quem a tem:
```csharp
_raidActive = true;            // sempre re-arma; StartLoop é idempotente (uma coroutine por raid)
BotDespawnManager.StartLoop();
```
Manter o guard de `OnRaidEnd` (`:31`) como está — ele é o que garante o no-op do segundo hook.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão *(decisão autônoma sob "Prossiga")*
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `Client/Helpers/RaidLifecycle.cs` — removido o early-return `if (_raidActive) return;` em `OnRaidStart`; `_raidActive = true` sempre re-arma e `StartLoop()` garante a idempotência. `// ref: CR-01-01`.

### CR-01-02 · C — Gap vs. spec · 🟡 Médio · ✅ Aplicado em 2026-08-22

**AC-X1 pede a nota de "edição ao vivo" também no README do painel**

**Local:** [`mods/TRL-DynamicSpawn/README.md:3`](../../README.md#L3) (ausência) · spec funcional AC-X1: *"O caminho manual tem de estar documentado em `PROPRIEDADES.md` **e no README do painel**"*.

**Problema:** só `PROPRIEDADES.md` ganhou a seção `Server Config`. O `README.md` do mod (que descreve o painel web) não diz que edições feitas **durante** a raid passaram a valer só na próxima raid ou via o toggle F12.

**Por que importa:** é a única mudança perceptível do item; quem edita o painel no meio da raid e não vê efeito vai abrir bug.

**Sugestão:** adicionar ao `README.md`, na parte que descreve o painel web, um parágrafo curto: *"A configuração do painel é lida pelo cliente **uma vez por raid**. Edições feitas durante a raid entram na próxima raid — ou imediatamente, marcando `F12 → Server Config → Reload Server Config`."*

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão *(decisão autônoma sob "Prossiga")*
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `README.md` — nova seção "Painel web e aplicação da configuração" (1×/raid + toggle F12).

### CR-01-03 · E — Legibilidade · 🟢 Menor · ✅ Aplicado em 2026-08-22

**Log "Raid end hook fired" dispara ao sair do hideout**

**Local:** [`mods/TRL-DynamicSpawn/Client/Patches/RaidLifecyclePatches.cs:45`](../../Client/Patches/RaidLifecyclePatches.cs#L45)

**Problema:** o hideout também tem um `GameWorld`; sair dele dispara `OnDestroy` → a linha `Raid end hook fired (GameWorld.OnDestroy)` aparece sem ter havido raid (o `OnWorldDestroyed` em si é inofensivo: `OnRaidEnd` é no-op e `ForceRefresh` limpa um cache já vazio). Custo zero, mas confunde a leitura do log na validação V1 (PA-01-05 conta nesses logs para provar que os hooks disparam).

**Sugestão:** mover os dois `LogInfo` dos prefixes para dentro de `RaidLifecycle.OnRaidEnd(string source)`, logando **só na primeira chamada efetiva** (`if (!_raidActive) return;` antes do log). Como `Stop` dispara antes de `OnDestroy`, a fonte logada confirma se o patch de `Stop` está vivo — objetivo do PA-01-05 preservado, sem ruído de hideout.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão *(decisão autônoma sob "Prossiga")*
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `Client/Helpers/RaidLifecycle.cs` — `OnRaidEnd(string source)` loga `Raid end hook fired (<source>)` só na primeira chamada efetiva; `Client/Patches/RaidLifecyclePatches.cs` — os dois `LogInfo` dos prefixes removidos, fonte passada por parâmetro. `// ref: CR-01-03`.

### CR-01-04 · F — Melhoria opcional · 🟢 Menor · ⏭️ Rejeitado em 2026-08-22

**`OnGameStartPatches.cs` é código morto pré-existente**

**Local:** [`mods/TRL-DynamicSpawn/Client/Patches/OnGameStartPatches.cs:9-35`](../../Client/Patches/OnGameStartPatches.cs#L9)

**Problema:** `OnGameStartedPatch` e `OnGameStartedPatch2` nunca são registrados no `Plugin.cs`; o segundo patcheia `BotZone.Awake` com um parâmetro `GameWorld __instance` que não existe nesse alvo (falharia no `Enable()`). Não é deste item, mas confundiu a auditoria de "quem patcheia `OnGameStarted`" (§7 da spec precisou explicar que é morto).

**Por que importa:** manutenção — próximo leitor assume que está ativo.

**Sugestão:** deferir para a rodada 1.5/2 (fora do escopo de não-regressão): remover o arquivo ou anotar `// DEAD CODE — not registered` no topo.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[x]` Rejeitar (deferir / aceitar como dívida): fora do escopo de não-regressão da rodada 1 — tratar na rodada 1.5 junto com o gating de logs (AUD-01-07)

**Resolução:** Rejeitado — fora do escopo de não-regressão da rodada 1; tratar na rodada 1.5 junto com o gating de logs (AUD-01-07).

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-08-22 | Code review 01 criada via `/code-review` |
| 2026-08-22 | Aplicação automática de 3 achados via `/apply-code-review` — IDs aplicados: CR-01-01, CR-01-02, CR-01-03; rejeitados (deferidos): CR-01-04 |
