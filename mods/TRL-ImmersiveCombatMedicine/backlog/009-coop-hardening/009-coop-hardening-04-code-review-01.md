# 009 — Coop/bots: hardening do Trauma 2.0 · Code Review 01

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [009-coop-hardening-01-spec.md](009-coop-hardening-01-spec.md)
**Spec técnica:** [009-coop-hardening-02-spec-tech.md](009-coop-hardening-02-spec-tech.md)
**Asbuild:** [009-coop-hardening-05-asbuild.md](009-coop-hardening-05-asbuild.md)
**Data:** 2026-07-20

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 1 · Total: 1

**Memória consultada:** topo de `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md` (Sessão 4, P-4.1 — débito do boilerplate `Update()`, aberto em 006 code-review-01 CR-01-02 e 008 code-review-01 CR-01-01; P-4.5 — 009/010 pendentes de início). Nenhuma pendência 🔴 do mod bloqueia esta rodada. P-4.1 é exatamente o que este item (A4) fecha.

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | F — Melhoria opcional | 🟢 Menor | Campo `_onToggleOn` do Stomach, sempre `null`, gera warning `CS0649` evitável | ✅ Aplicado |

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

## Verificação central desta rodada (regressão linha-a-linha dos 4 consumidores + demais focos do Passo 4)

Releitura completa via `git diff` de cada um dos 6 arquivos tocados (5 modificados + 1 criado), cruzada com `009-coop-hardening-02-spec-tech.md` §5 e as 2 rodadas de review técnica (PA-01-01/02/03, PA-02-01).

1. **`Update()`/`Awake()` novo vs. antigo, linha a linha, nos 4 consumidores** — sem regressão em nenhum:
   - **`TraumaLegsConsumer.cs`**: branch `gw==null` (clear `_applied` + `CancelAll` + `ClearBotRestores`) → `OnWorldGone()` idêntico; world-swap (mesmas 3 chamadas) → `OnWorldSwap()` idêntico; toggle ON→OFF (sweep + `RemoveCapGuarded` loop + `CancelKind` + `FlushBotRestores` + log) → `OnToggleOff()` idêntico; toggle OFF→ON (loop `RegisteredPlayers` + `ApplyCap` + log condicional) → `OnToggleOn()` idêntico. `Update()` novo (`mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaLegsConsumer.cs:217-238`) preserva a ordem: detecção → bookkeeping (agora dentro de `Tick()`) → `if (!active) return;` → poda oportunista → `PumpDeferred`/`PumpBotRestores`, exatamente a mesma sequência do `if (active) { ... }` original.
   - **`TraumaFallCycleConsumer.cs`**: os 4 branches mapeiam 1:1 para `OnWorldGone`/`OnWorldSwap`/`OnToggleOff`/`OnToggleOn` (`:229-303`); `Update()` (`:304-313`) chama `Tick()` e, com `active`, roda `TickHumanCycle()` → `PumpDeferred()` → `TraumaBotFall.Pump()` na mesma ordem do original.
   - **`TraumaArmsConsumer.cs`** (o mais complexo — watchdog + timer de ADS + hooks): `OnWorldGone`/`OnWorldSwap` (`:355-367`) contêm **só** `TearDownLocal(...)`+`ResetLockout()`, sem o bookkeeping (`_trackedWorld`/`_wasActive`/`return`) copiado para dentro — confere com a citação corrigida PA-01-02, não com a original (que incluía essas linhas no range citado). `OnToggleOff`/`OnToggleOn` (`:369-387`) idênticos ao original. A poda oportunista + watchdog (`_reestablishPending`) + deadline do timer ADS (`_aimAnchor`), que no original rodavam **depois** de `_wasActive = active; if (!active) return;`, continuam rodando depois do mesmo guard (`:392-429`, comentário "INALTERADO" confere com o diff — zero linha tocada nesse trecho).
   - **`TraumaStomachConsumer.cs`** (o mais simples): `OnWorldGone`/`OnWorldSwap`/`OnToggleOff` (`:133-152`) mapeiam 1:1 para os 3 `CancelKind(...)` originais; não há branch de toggle-on original (nenhum `else if`) e o novo código preserva isso passando `_onToggleOn` (sempre `null`) ao `Tick()` — `?.Invoke()` vira no-op, comportamento idêntico.
   - Nenhuma reordenação, nenhum "enquanto estou aqui" de melhoria em nenhum dos 4 arquivos.

2. **Campos antigos `_wasActive`/`_trackedWorld` realmente removidos** — grep `_wasActive|_trackedWorld` nos 4 consumidores retorna **só ocorrências em comentário** (`TraumaLegsConsumer.cs:31,195`; `TraumaFallCycleConsumer.cs:28,231`; `TraumaArmsConsumer.cs:39,357,367`; `TraumaStomachConsumer.cs:19,132`) — nenhuma declaração de campo nem leitura/escrita real. Confirmado: não foram só comentados, foram de fato apagados.

3. **Nenhum campo `_lifecycle` marcado `readonly`** — grep `TraumaConsumerLifecycle _lifecycle` nos 4 arquivos mostra `private TraumaConsumerLifecycle _lifecycle;` (sem `readonly`) em todos: `TraumaLegsConsumer.cs:32`, `TraumaFallCycleConsumer.cs:29`, `TraumaArmsConsumer.cs:40`, `TraumaStomachConsumer.cs:20`. Todos carregam o comentário `// PA-01-03: NUNCA marcar readonly`.

4. **Bookkeeping NÃO copiado para dentro dos callbacks do Arms (PA-01-02)** — confirmado acima (item 1): `OnWorldGone()`/`OnWorldSwap()` do `TraumaArmsConsumer.cs` (`:355-367`) contêm só as 2 chamadas de negócio; `_trackedWorld = null`/`_wasActive = IsActive()`/`_trackedWorld = gw`/`return` não aparecem em lugar nenhum fora de `TraumaConsumerLifecycle.Tick()`. A implementação seguiu a citação **corrigida** da review técnica, não a original.

5. **`TraumaStomachConsumer._onToggleOn` nunca atribuído** — confirmado (`TraumaStomachConsumer.cs:25,44-45`): campo declarado, nunca recebe valor em `Awake()`, permanece `null`; `Tick()` invoca via `?.Invoke()`, que é no-op para delegate nulo. Reproduz fielmente o original (que não tinha `else if` de toggle-on nenhum). O warning `CS0649` (campo nunca atribuído) é a consequência esperada e documentada dessa escolha — não é sintoma de bug, é o comportamento intencional. Ver CR-01-01 abaixo para uma simplificação opcional que eliminaria o próprio warning sem mudar o comportamento.

6. **Fix do erro de compilação (`UnityEngine.Random.value` explícito)** — `TraumaStomachConsumer.cs:88`: `bool success = chance >= 100f || (chance > 0f && UnityEngine.Random.value * 100f < chance);`. Único uso de `Random` no arquivo (grep confirma). A ambiguidade nasceu porque `using System;` (novo, para `Func<bool>`/`Action`) entrou em conflito com `using UnityEngine;` (pré-existente) ao resolver o identificador `Random` — `System.Random` e `UnityEngine.Random` são dois tipos com o mesmo nome simples, e o C# reporta CS0104 na simples menção ambígua, antes mesmo de resolver o membro `.value`. A qualificação total resolve só a ambiguidade de namespace; a fórmula do roll (`chance >= 100f || (chance > 0f && valor*100 < chance)`) é byte-a-byte a mesma. Grep confirma que `Legs`/`FallCycle` não usam `Random` (sem ambiguidade lá) e `Arms` já tinha `using System;` antes desta mudança (sem novo risco).

7. **`TraumaVoice.cs` — só o comentário mudou** — `git diff` mostra exclusivamente a adição do bloco de comentário XML "DECISÃO A3 (009, 2026-07-20)" acima de `PlayStrong`; a assinatura (`internal static void PlayStrong(Player p)`) e o corpo (`if (!Allowed(p, strong: true)) return; p.Speaker?.Play(...)`) são idênticos linha a linha ao `HEAD`. `TryPlayStrong` (005) não foi tocado. Confirmado.

8. **Ordem em `Awake()` — cache dos delegates DEPOIS de `Register`/`SubscribeWithSnapshot`/`OneShotPublished`** — confirmado nos 4 arquivos: `TraumaLegsConsumer.cs:45-58` (`Register` → `SubscribeWithSnapshot` → `OneShotPublished +=` → 5 delegates), `TraumaFallCycleConsumer.cs:41-53` (idem), `TraumaArmsConsumer.cs:52-63` (`Register` → `SubscribeWithSnapshot` → delegates; sem `OneShotPublished`, consistente com o original), `TraumaStomachConsumer.cs:35-45` (idem, com o comentário explícito "SEM `OneShotPublished`"). O comentário sobre o replay síncrono (PA-02-01 — *"pode invocar OnTransition sincronamente AQUI... seguro pois OnTransitionCore nunca toca `_lifecycle`/delegates"*) está presente, com a mesma redação, nos 4 pontos de chamada de `SubscribeWithSnapshot`.

9. **Ângulo novo (achado, ver CR-01-01):** `_onToggleOn` do Stomach é um campo que existe só para ficar sempre `null` — o warning `CS0649` que ele gera é evitável sem mudar comportamento (ver abaixo). Nenhum outro ângulo novo encontrado: visibilidade (`internal struct`) consistente com outros helpers do mod (mistura de `public`/`internal`/`static` já presente em `TraumaEngine`/`TraumaConsumerRegistry`/`TraumaPose`); `struct` mutável acessado só via campo de instância (nunca via propriedade/indexador), então não há risco de cópia defensiva; primeiro `Update()` de cada consumidor já disparava o branch de "world-swap" antes da extração (`_trackedWorld` nasce `null` por default, `!ReferenceEquals(gw, null)` é sempre `true` no primeiro tick) — comportamento pré-existente, não introduzido por A4.

---

## Pontos

### CR-01-01 · F — Melhoria opcional · 🟢 Menor · ✅ Aplicado em 2026-07-25

**Campo `_onToggleOn` do Stomach, sempre `null`, gera warning `CS0649` evitável sem mudar comportamento**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaStomachConsumer.cs:25`](../../modded/Patches/Trauma/TraumaStomachConsumer.cs#L25) (declaração) e [`:158`](../../modded/Patches/Trauma/TraumaStomachConsumer.cs#L158) (call site)

**Problema:** `TraumaStomachConsumer` declara `private Action _onToggleOn;` (linha 25) e nunca o atribui em `Awake()` — por design, comentado explicitamente nas linhas 44-45 ("Stomach não tem ação de religar"). O único uso do campo é o call site do `Update()`:

```csharp
bool active = _lifecycle.Tick(_isActiveDelegate, _onWorldGone, _onWorldSwap, _onToggleOff, _onToggleOn);
```

Como o campo nunca recebe valor, o compilador emite `CS0649` ("Field is never assigned to, and will always have its default value null") — o único warning novo introduzido por este item (contra 10 `Harmony003` pré-existentes, por build). O comportamento está correto (confirmado no item 5 da verificação acima), mas o campo existe só para carregar um `null` constante.

**Por que importa:** é cosmético — não afeta comportamento nem falha o build — mas adiciona ruído permanente ao output de compilação deste arquivo especificamente por causa de uma decisão de design (Stomach não tem ação de toggle-on) que já está bem documentada em comentário. Um novo warning nesse arquivo no futuro (de uma mudança real) fica mais fácil de notar se o warning de hoje não estiver lá.

**Sugestão:** eliminar o campo `_onToggleOn` de `TraumaStomachConsumer` e passar o literal `null` diretamente no call site do `Update()`:

```csharp
bool active = _lifecycle.Tick(_isActiveDelegate, _onWorldGone, _onWorldSwap, _onToggleOff, null);
```

Remover a declaração da linha 25 e o bloco de comentário associado (linhas 44-45 de `Awake()`), preservando a explicação ("Stomach não tem ação de religar") como comentário inline no próprio call site do `Update()`. Zero mudança de comportamento — `Tick()` já trata `Action` nula via `?.Invoke()`.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** aceita como sugerida — campo `_onToggleOn` eliminado, `null` literal passado direto no call site.

**Aplicação:** `TraumaStomachConsumer.cs` — removida a declaração do campo (linha 25, substituída por comentário explicando a ausência) e o comentário de não-atribuição em `Awake()`; call site do `Update()` (linha 157) agora passa `null` literal com comentário atualizado explicando a paridade com o original. Recompilado (`compile-mod.sh TRL-ImmersiveCombatMedicine --allow-same-version`): 0 erros, 10 warnings (só `Harmony003` pré-existentes) — o `CS0649` sumiu, confirmando a correção sem regressão.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-20 | Code review 01 criada via `/code-review` — verificação linha-a-linha de regressão nos 4 consumidores (003/004/005/006), confirmação dos 9 pontos do checklist de risco (campos antigos removidos, `_lifecycle` sem `readonly`, bookkeeping fora dos callbacks do Arms, `_onToggleOn` do Stomach fiel ao original, fix de `Random` só de namespace, `TraumaVoice.cs` só com comentário, ordem em `Awake()` preservada). 0 bloqueadores; 1 achado 🟢 opcional (CR-01-01, warning `CS0649` evitável). |
| 2026-07-25 | CR-01-01 aplicado via `/apply-code-review` — campo `_onToggleOn` removido de `TraumaStomachConsumer.cs`, `null` literal no call site; recompilado (0 erros, 10 warnings pré-existentes, `CS0649` eliminado). Achado fechado; 0 pendências nesta rodada. |
