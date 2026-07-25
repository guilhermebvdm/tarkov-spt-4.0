# 009 — Coop/bots: hardening do Trauma 2.0 · Review Técnica 02

**Mod:** TRL-ImmersiveCombatMedicine
**Spec técnica revisada:** [009-coop-hardening-02-spec-tech.md](009-coop-hardening-02-spec-tech.md)
**Data:** 2026-07-25

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-02-MM` (review 02, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 1 · Total: 1

**Memória consultada:** topo de `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md` (Sessão 4, entradas P-4.1/P-4.5 e bloco P-3.x). Mesma leitura da rodada 1, sem entradas novas desde então. P-4.1 (débito do boilerplate `Update()`, aberto em 006 code-review-01 CR-01-02 e 008 code-review-01 CR-01-01) é o que este item resolve; nenhuma pendência 🔴 do mod bloqueia esta rodada.

**Confirmação (releitura direta da spec técnica ATUAL, não do rótulo "Resolvido" da rodada 1) dos 3 achados da rodada 1 — todos de fato fechados no texto:**
- ✅ PA-01-01 resolvido na spec — fechado. §4 (cada uma das 4 linhas "MODIFICAR" da tabela) e §8 (itens 2-5 do checklist) instruem explicitamente "remover os campos antigos (`_wasActive`/`_trackedWorld`)" para os 4 consumidores.
- ✅ PA-01-02 resolvido na spec — fechado. §5.3 (nota de Arms) hoje lê "corpo idêntico a `TraumaArmsConsumer.cs:350-351`/`359-360` (só as 2 chamadas), EXCETO o bookkeeping `_trackedWorld`/`_wasActive`/`return`, agora do struct" — reconferido contra o arquivo real nesta rodada (ver §6 abaixo): citação exata, `TearDownLocal("raid-end", worldDead: true)`/`ResetLockout()` estão de fato nas linhas 350/351 e `TearDownLocal("world-swap", worldDead: true)`/`ResetLockout()` nas linhas 359/360.
- ✅ PA-01-03 resolvido na spec — fechado. Aviso "NUNCA marcar `readonly`" presente no comentário do `struct TraumaConsumerLifecycle` (§5.1) e replicado no campo `_lifecycle` de cada um dos 4 consumidores (§5.2/§5.3, comentário `// PA-01-03: NUNCA marcar readonly`).

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-02-01 | A — Gap | 🟢 Menor | `SubscribeWithSnapshot` pode disparar `OnTransition` sincronamente dentro do próprio `Awake()`, antes do cache dos delegates — spec não documenta por que isso é inofensivo | ✅ Resolvido |

---

## Verificação desta rodada (focos além da rodada 1, com releitura independente dos 4 arquivos reais)

### 1. Colisão de nomes `OnWorldGone`/`OnWorldSwap`/`OnToggleOff`/`OnToggleOn`

Grep de `OnWorldGone|OnWorldSwap|OnToggleOff|OnToggleOn` em todo `mods/TRL-ImmersiveCombatMedicine/modded/` (não só os 4 arquivos-alvo): **zero ocorrências**. Os 4 nomes são inteiramente novos — nenhum dos 4 arquivos reais (`TraumaLegsConsumer.cs`, `TraumaFallCycleConsumer.cs`, `TraumaArmsConsumer.cs`, `TraumaStomachConsumer.cs`, todos relidos por inteiro nesta rodada) já declara método, campo ou evento com esses nomes, e nenhum outro arquivo do mod os usa. Também confirmado que nenhum dos 4 nomes coincide com uma mensagem mágica do Unity (`OnEnable`/`OnDisable`/`OnDestroy`/`OnApplicationQuit`/etc. não incluem essas variantes) — sem risco de invocação implícita pelo motor do Unity. **Sem achado.**

### 2. Ordem em `Awake()`: registro/assinatura do motor ANTES do cache dos delegates

Os 4 `Awake()` reais seguem `_instance = this;` → `TraumaConsumerRegistry.Register(...)` → `TraumaEngine.SubscribeWithSnapshot(OnTransition)` [+ `TraumaEngine.OneShotPublished += OnOneShot;` só em Legs/FallCycle] — e a spec insere o cache dos 5 delegates (`_isActiveDelegate`/`_onWorldGone`/`_onWorldSwap`/`_onToggleOff`/`_onToggleOn`) DEPOIS dessas chamadas, nunca antes, nos 4 casos.

`TraumaConsumerRegistry.Register` (`TraumaEngineState.cs:132-135`) só grava num `Dictionary` — nenhuma invocação síncrona. Mas **`TraumaEngine.SubscribeWithSnapshot` (`TraumaEngine.cs:72-96`) FAZ replay síncrono**: se o motor (`_instance` do `TraumaEngine`) já existe com registros ativos, o método invoca `handler(...)` (= `OnTransition` do consumidor) uma vez por linha ativa de cada registro, dentro do próprio loop, ANTES de retornar (`TraumaEngine.cs:89`, dentro do `for` que começa em `:78`). Ou seja: `OnTransition`/`OnTransitionCore` PODEM rodar no meio do `Awake()` do consumidor, antes das linhas de cache dos delegates (que vêm depois na mesma função) — a hipótese "improvável" levantada pelo orquestrador na verdade **acontece por construção**, não é só uma possibilidade teórica.

Isso não é um bug: confirmado por leitura completa dos 4 `OnTransitionCore`/`OnOneShotCore` que nenhum deles lê ou escreve `_lifecycle`, `_isActiveDelegate`, `_onWorldGone`, `_onWorldSwap`, `_onToggleOff` ou `_onToggleOn` — a "reação a transição" (dict `_applied`, FSM `_phase`/`_local`, `_localPlayer`/`_localLine`+hooks, roll do estômago) e a "detecção de lifecycle" (o helper) são trilhas de estado totalmente independentes. A ordem realmente não importa hoje. Mas a spec (§7 "Ordem de inicialização") só documenta a garantia Awake()→Update() do Unity — um ponto diferente e mais óbvio — sem mencionar esse replay síncrono nem por que ele é seguro em relação aos campos do helper. Vira o achado **PA-02-01** abaixo (🟢 — documentação/blindagem para manutenção futura, não bug atual).

### 3. `TraumaArmsConsumer` (consumidor com mais estado) — campos per-tick lidos/escritos DENTRO dos 4 callbacks

Releitura completa do `Update()` real (`TraumaArmsConsumer.cs:343-430`) confirma:
- `TearDownLocal(reason, worldDead)` — chamada de dentro do futuro `OnWorldGone`/`OnWorldSwap`/`OnToggleOff` — ESCREVE `_localPlayer = null; _localLine = TraumaLine.None; _reestablishPending = false; _reestablishCount = 0; _aimAnchor = -1f;` e desmonta os hooks (`TearDownAimHooks()`/`TearDownWatchdog()`, que por sua vez zeram `_hookedFc`/`_handsHookedPlayer`/`_hookedHc`).
- `ResetLockout()` — idem — ESCREVE `_lockoutUntil`/`_lockoutProfileId`/`_lockoutVoicePlayed`/`_nextVoiceTryAt`/`_lockoutVoiceSkipLogged`/`_lockoutIncapacitatedLogged`.
- Essas MESMAS variáveis são LIDAS logo depois, na lógica per-tick que sobra em `Update()` após `if (!active) return;`: a poda oportunista lê `_localPlayer`; o bloco do watchdog lê `_reestablishPending`/`_localPlayer`; o deadline do timer de ADS lê `_aimAnchor`/`_localPlayer`/`_localLine` (linhas 392-429 do arquivo real, citadas pela própria spec como "100% fora do helper").

A ORDEM entre a escrita (dentro do callback, executada de dentro de `Tick()`) e a leitura (no corpo de `Update()`, após `if (!active) return;`) é **idêntica à do original**: hoje, os mesmos `TearDownLocal`/`ResetLockout` já rodam ANTES da poda/watchdog/deadline, sequencialmente dentro do mesmo `Update()`. A extração não inverte nada — `Tick()` executa a mesma sequência de branches (null → swap → toggle) internamente, na mesma ordem, e só devolve o controle ao corpo per-tick de `Update()` depois, exatamente onde ele já rodava. Também conferido: o único ponto que precisa de `gw` fora do que `Tick()` já resolveu é `OnToggleOn` (bloco do snapshot, `TraumaArmsConsumer.cs:378-387` — `gw.MainPlayer`), e ele re-obtém `Singleton<GameWorld>.Instance` dentro do próprio callback, mesmo padrão já documentado e usado em Legs/FallCycle (§5.2/§5.3) — sem risco de leitura divergente entre a chamada em `Tick()` e a nova busca dentro do callback (Unity é single-thread; nada re-atribui `Singleton<GameWorld>.Instance` no meio do mesmo frame). **Nenhum achado novo aqui** — a extração preserva a ordem escrita→leitura em todos os pontos verificados, incluindo os que a rodada 1 não teria cruzado (leituras/escritas do MESMO campo nos dois lados do guard).

### 4. Comparação dos 4 `Update()` "depois" — a duplicação de DETECÇÃO realmente some?

Reconstruídos os 4 corpos pós-extração a partir do código completo de §5.2 (Legs) e §5.3 (FallCycle), mais a prosa + linhas citadas para Arms/Stomach (§5.3, últimos 2 parágrafos), cruzados contra os 4 arquivos reais:

| Consumidor | Linha(s) de detecção | Lógica per-tick após o guard (inalterada) |
|---|---|---|
| Legs | `bool active = _lifecycle.Tick(_isActiveDelegate, _onWorldGone, _onWorldSwap, _onToggleOff, _onToggleOn); if (!active) return;` | poda oportunista + `TraumaPose.PumpDeferred()`/`PumpBotRestores()` |
| FallCycle | idêntica, mesma assinatura de 5 argumentos | `TickHumanCycle()` + `TraumaPose.PumpDeferred()` + `TraumaBotFall.Pump()` |
| Arms | idêntica | poda oportunista + bloco do watchdog (`_reestablishPending`) + deadline do timer ADS |
| Stomach | idêntica (`_onToggleOn` cacheado como `null` — nunca atribuído no `Awake()` de Stomach — `?.Invoke()` no-op) | `TraumaPose.PumpDeferred()` + `PumpBotRestores()` |

A parte de DETECÇÃO fica **byte-a-byte igual** nos 4 casos — mesma chamada, mesmos 5 parâmetros na mesma ordem, mesmo guard `if (!active) return;` logo em seguida. Só a AÇÃO de cada evento (já era específica por consumidor antes da extração) e a lógica per-tick após o guard mudam de um consumidor para outro — exatamente o que A4 promete ("elimina a duplicação da DETECÇÃO, nunca da AÇÃO"). **Objetivo declarado de A4 confirmado cumprido — sem achado.**

### 5. `PROPRIEDADES.md` / `mod-backlog.md`

Nenhuma `ConfigEntry` nova em qualquer stub de §5 (grep confirma: zero `Config.Bind`/`ConfigEntry` novo em qualquer bloco criado ou modificado por A3/A4) — `PROPRIEDADES.md` não tem nada a atualizar, consistente com §3 da spec ("N/A"). `mod-backlog.md` (linha do item 009) permanece ⚪ Backlog, o que é correto para a fase atual: a transição para 🟡 é responsabilidade do `/code-mod`, não da spec técnica (`repo-workflow-best-practices` §6). **Nada a apontar.**

### 6. Ângulo adicional — evidências do Assembly de A3 reconferidas de forma independente

Reli `references/eft-decompiled/Assembly-CSharp/PhraseSpeakerClass.cs` sem me apoiar na rodada 1: assinatura `public TagBank Play(EPhraseTrigger trigger, ETagStatus tags, bool demand = false, int? importance = null)` está exatamente na linha 176; o predicado `Busy && importance <= Int_0` está exatamente nas linhas 207-211; o método fecha na linha 239 — bate 100% com a citação da spec (`PhraseSpeakerClass.cs:176-239`). Em `TraumaVoice.cs`, a chamada `p.Speaker?.Play(...)` de `PlayStrong` está na linha 21, a de `TryPlayStrong` na linha 31, e `Allowed()` (o carimbo otimista do cooldown de 2s) ocupa exatamente as linhas 43-50 — todas as citações batem. **Nenhum erro de citação em A3, confirmado de forma independente.**

---

## Pontos

### PA-02-01 · A — Gap · 🟢 Menor · Resolvido em 2026-07-20

**`SubscribeWithSnapshot` pode disparar `OnTransition` sincronamente dentro do próprio `Awake()`, antes do cache dos delegates — spec não documenta por que isso é inofensivo**

**Problema:** a ordem proposta em §5.2/§5.3 para os 4 `Awake()` é `_instance = this;` → `TraumaConsumerRegistry.Register(...)` → `TraumaEngine.SubscribeWithSnapshot(OnTransition)` [+ `OneShotPublished += OnOneShot` em Legs/FallCycle] → cache dos 5 delegates (`_isActiveDelegate`, `_onWorldGone`, `_onWorldSwap`, `_onToggleOff`, `_onToggleOn`). Só que `TraumaEngine.SubscribeWithSnapshot` (`TraumaEngine.cs:72-96`) não é uma simples assinatura de evento: se o motor (`_instance` do `TraumaEngine`) já existe com registros ativos, o método chama `handler(...)` SINCRONAMENTE dentro do próprio loop (`TraumaEngine.cs:89`, dentro do `for` iniciado em `:78`) antes de retornar — isto é, `OnTransition`/`OnTransitionCore` do consumidor pode rodar NO MEIO do `Awake()`, antes das linhas de cache dos delegates que vêm depois na mesma função. A spec §7 ("Ordem de inicialização") só documenta a garantia Awake()→Update() do Unity — não menciona esse replay síncrono nem por que ele é seguro em relação a `_lifecycle`/aos delegates cacheados.

**Por que importa:** hoje é inofensivo — confirmado por leitura completa dos 4 `OnTransitionCore`/`OnOneShotCore`: nenhum deles lê ou escreve `_lifecycle`, `_isActiveDelegate`, `_onWorldGone`, `_onWorldSwap`, `_onToggleOff` ou `_onToggleOn` (a "reação a transição" e a "detecção de lifecycle" são trilhas de estado totalmente separadas). Mas essa segurança é IMPLÍCITA, não documentada. Se uma sessão futura adicionar a um `OnTransitionCore`/`OnOneShotCore` uma leitura de algo equivalente ao estado do `_lifecycle` (ex.: checar se o mundo já foi processado neste frame), reintroduziria silenciosamente um bug de ordem de inicialização — o helper ainda não teria cacheado nada / rodado seu primeiro `Tick()` nesse ponto do `Awake()`. É o mesmo tipo de invariante não-óbvio que motivou o aviso de `readonly` da PA-01-03 — vale o mesmo cuidado de documentação.

**Sugestão:** adicionar a §7 ("Ordem de inicialização") uma frase explícita: *"`TraumaEngine.SubscribeWithSnapshot` (chamada em `Awake()` antes do cache dos delegates) pode invocar `OnTransition` sincronamente durante o próprio `Awake()`, se o motor já tiver registros ativos (`TraumaEngine.cs:72-96`, replay em `:89`). Isso é seguro porque `OnTransitionCore`/`OnOneShotCore` nunca leem/escrevem `_lifecycle` nem os delegates cacheados (`_isActiveDelegate`/`_onWorldGone`/`_onWorldSwap`/`_onToggleOff`/`_onToggleOn`) — são trilhas de estado independentes. Qualquer mudança futura que acople as duas trilhas precisa mover o cache dos delegates para ANTES de `SubscribeWithSnapshot`."* Opcionalmente, replicar como comentário inline em cada um dos 4 `Awake()` reais, junto da linha `TraumaEngine.SubscribeWithSnapshot(OnTransition)`.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** §7 ("Ordem de inicialização") ganhou o parágrafo explicando o replay síncrono de `SubscribeWithSnapshot` e por que é seguro; comentário inline replicado nos 4 stubs de `Awake()` (§5.2/§5.3).
