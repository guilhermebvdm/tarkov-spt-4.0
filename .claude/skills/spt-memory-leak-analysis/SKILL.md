---
name: spt-memory-leak-analysis
description: Static memory-leak auditing for SPT 4.0 / EFT 0.16.x / Fika mods (client BepInEx plugins and C# server mods). Use during /analyze-memory-leak, and during /review-technical-spec, /code-mod and /code-review whenever the mod allocates raid-scoped state, subscribes to events, spawns Unity objects, or holds static/server-side caches. Classifies leaks by mechanism and by accumulation rate (per-frame/per-raid/per-event/per-boot), ties them to the Fika headless OOM scenario, and gives grep recipes + an in-game confirmation plan. Points to `csharp-mod-best-practices` §1 and `spt-mod-best-practices` §2/§3 for the underlying rules instead of repeating them.
---

# SPT / EFT / Fika Memory-Leak Analysis

Corpo de conhecimento para **auditar vazamento de memória** em mods deste repo. Alvo: SPT 4.0 / EFT 0.16.x, client (BepInEx/Harmony/Unity) e server (C# `[Injectable]`).

> **Esta skill NÃO reescreve as regras de memória** — elas já vivem em:
> - `csharp-mod-best-practices` §1 (memory ownership: disposal, referências que pinam, alocações), §2 (async/threading/Unity), §6 (coleções).
> - `spt-mod-best-practices` §2 (raid lifecycle — "the leak point"), §3 (memory & performance).
> - `docs/technical/spt-antipatterns.md`: **AP-01** (raid lifecycle hooks ausentes → estado estático vaza entre raids), **AP-07** (self-reentry → crash), **AP-08** (estado stale entre contextos).
>
> O que **esta** skill adiciona: (1) a ligação com o **Fika headless** (por que um leak invisível no solo derruba o headless), (2) uma **taxonomia de mecanismos** com greps de detecção, (3) o **eixo de taxa de acúmulo** que define severidade, (4) o **procedimento de auditoria estática** (parear alocação↔release), (5) o **plano de confirmação in-game**, e (6) o que **NÃO** é leak do mod (falsos positivos do ambiente).

---

## 1. Por que leak mata o headless (o contexto que orienta a prioridade)

O sintoma que motiva esta análise: **o Fika headless reinicia/crasha após alguns minutos/raids, mesmo com 32 GB de RAM.**

Cadeia causal:
- O **Fika headless** é uma instância do EFT (Unity) rodando **sem render**, que hospeda a raid coop. Ele carrega **os mesmos mods client** que os peers (precisam dar match).
- Diferente de uma sessão solo (que fecha o processo ao voltar pro menu), o headless **fica de pé por horas**, hospedando **raid após raid no mesmo processo**.
- Logo: um leak **per-raid** que no solo você nunca percebe (uma raid, fecha o jogo) **acumula linearmente** no headless. 30 raids = 30 grafos de raid retidos → OOM muito antes de "ter RAM sobrando" ajudar (o pagefile estoura / o processo é morto).
- O **EFT já vaza por conta própria** (leak conhecido do jogo base — ver `wiki/spt/Performance_Tuning.md` e `Known_EFT_Issues_40.md`). O mod **soma** a esse baseline. Você não conserta o leak da BSG, mas **não pode adicionar mais**.

**Regra de ouro da priorização:** a gravidade de um leak é dominada pela **taxa de acúmulo** (§4), não pelo tamanho de cada alocação. Um `int` vazado por frame é pior que um `Texture2D` vazado uma vez no boot.

---

## 2. O que o ambiente JÁ faz com memória (não sugira o que já existe)

Antes de propor mitigação, saiba o que Fika/SPT já fazem — sugerir isso de novo é ruído, e **contra-atacar** isso é bug.

### Fika headless (`references/fika-headless/Fika.Headless/FikaHeadlessPlugin.cs`)
- **RAM cleaner nativo** (loop no `Update`, cadência `RAMCleanInterval`, default 15 min): **em raid** roda `GarbageCollector.CollectIncremental`; **fora de raid** roda `Resources.UnloadUnusedAssets()` + `MemoryControllerClass.Collect(2, Forced, …)`.
- **Restart preventivo por contagem de raids** (`_restartAfterAmountOfRaids` → `OnReady` chama `Application.Quit()`): o workaround aceito upstream para leak acumulado é **reiniciar após N raids**. Valor vem do server (`/fika/headless/restartafterraidamount`).
- **Watchdog de conexão**: 45 s sem peers → `Application.Quit()`. **Exceção no init de raid** → `Application.Quit(1)`. Ou seja, "o headless reiniciou" pode ser leak (OOM), mas também pode ser watchdog/exceção — a análise de leak é uma hipótese entre outras (o `LogOutput.log` desambigua).
- **Blocklist**: o headless **recusa** hospedar se detectar RAM/VRAM cleaners de terceiros (`com.cactuspie.ramcleanerinterval`, `SPTVRAMCleaner`…) e **desabilita** o `MemoryCollectionPatch` do SPT. → **Não** proponha um "RAM cleaner" como mod; o headless já gerencia isso e bane concorrentes.

### SPT server (C#, deepwiki `sp-tarkov/server-csharp`)
- `NoGCRegionMiddleware` controla GC durante requests. Dados de jogo são **imutáveis, carregados 1× no boot** (`DatabaseService` singleton). Handlers de request são **transient** (por-request). Caches explícitos: `BotLootCacheService`, `ItemBaseClassService`.
- Implicação para leak server-side: o risco não é o dado imutável — é **cache/coleção `static` ou em singleton que cresce por profile/raid sem eviction**, ou handler registrado e nunca removido (§3 SRV).

**Consequência prática:** mitigação de leak no mod = **liberar o que o mod alocou** (parear alocação↔release), **nunca** `GC.Collect()` / `Resources.UnloadUnusedAssets()` a partir do mod (causa hitch e quebra outros mods — `spt-mod-best-practices` §3).

---

## 3. Taxonomia de mecanismos (com grep de detecção e release pareado)

Cada leak tem **um par**: um ponto de **alocação** (nasce) e um ponto de **release** (deveria morrer). Leak = alocação sem release no escopo certo. Os greps abaixo acham a **alocação**; a auditoria (§5) procura o **release pareado**.

### LIFE — estado de raid sem teardown
- **Sintoma:** o mod aloca em raid-start (hook em `GameWorld.OnGameStarted`) mas não libera em raid-end; ou o stop-hook não existe / não é idempotente.
- **Grep (alocação):** `RaidSession|OnGameStarted|AfterGameStarted|_gameWorld =|MainPlayer` no `modded/`.
- **Release pareado:** patch em `GameWorld.OnDestroy` **E** `BaseLocalGame.Stop` (qualquer um dispara primeiro conforme o tipo de saída), com guard `bool _ended` idempotente. Ausência = **LIFE** (AP-01). Ver `spt-mod-best-practices` §2 "Stop hook (the leak point)".
- **Taxa:** per-raid (🟠), sobe para 🔴 se o estado retido for o grafo do raid (`Player`/`GameWorld`).

### EVT — subscription sem `-=` pareado
- **Sintoma:** `publisher.Event += OnFoo;` em publisher longevo (`GameWorld`, `Camera`, `GameUI`, `Singleton<T>.Instance`, evento estático) sem `-=` no teardown. Cada raid adiciona um handler; o handler segura `this` → segura o grafo inteiro. Closures capturam `this` implicitamente.
- **Grep (alocação):** `\+= ` (foco em `+= On`, `.Subscribe(`, `Add.*Listener`, `Action`), depois grep negativo do `-=` correspondente.
- **Release pareado:** `-= OnFoo` no mesmo teardown do LIFE, ou weak-event. `csharp-mod-best-practices` §1 "References that pin objects".
- **Taxa:** per-raid (🟠) se subscrito no raid-start; per-frame (🔴) se subscrito dentro de um patch por frame (raro mas letal).

### STAT — estado estático que retém ou só cresce
- **Sintoma:** `static List<Player>`, `static Dictionary<string, BotOwner>`, cache `static` populado em raid/evento e nunca limpo. Retém refs → **impede GC do raid inteiro** (AP-01). Cache sem limite/eviction cresce indefinidamente.
- **Grep (alocação):** `static .*(List|Dictionary|HashSet|Queue|ConcurrentDictionary|\[\])` + `static .*(Player|BotOwner|GameObject|Profile|Transform)`.
- **Release pareado:** `.Clear()` no raid-end; ou cache com limite (LRU/eviction). Sem clear = **STAT**.
- **Taxa:** per-raid ou per-event (🟠/🔴 conforme o que retém).

### UNITY — objeto Unity não destruído
- **Sintoma:** `new GameObject`, `Instantiate`, `AddComponent`, `.material`/`.materials` (clona!), `Texture2D`/`RenderTexture`/`AudioClip`/`AssetBundle` carregados e não `Destroy`/`Unload(true)`. Objeto parenteado a `null` ou a raiz persistente **sobrevive à raid**. Material clonado via `.material` cria uma instância que precisa de `Destroy`.
- **Grep (alocação):** `new GameObject|Instantiate\(|AddComponent|\.material\b|\.materials\b|LoadAsset|AssetBundle|new Texture2D|new RenderTexture`.
- **Release pareado:** `Destroy(...)`/`DestroyImmediate`/`Unload(true)` no teardown; ou parentear a um objeto que o EFT destrói (`gameWorld.transform`, `mainPlayer.gameObject`). Ver `spt-mod-best-practices` §3 e a lição real de material em `mods/SPT-Menu-Overhaul/memory/sessions.md` (clone explícito + destruição no `DisposeResources`).
- **Taxa:** per-raid/per-event (🟠); per-frame se instanciado em `Update` (🔴).

### DISP — IDisposable / coroutine / timer sem dispose
- **Sintoma:** `CancellationTokenSource`, `Stream`, `HttpClient` (per-request), `SemaphoreSlim`, `Timer`, `StartCoroutine` sem cancel. Coroutine iniciada num `MonoBehaviour` que sobrevive à raid continua rodando; CTS não disposto vaza handles.
- **Grep (alocação):** `new CancellationTokenSource|new SemaphoreSlim|new Timer|StartCoroutine|new HttpClient|new FileStream|new MemoryStream`.
- **Release pareado:** `using var` (escopo de método), ou `.Dispose()`/`.Cancel()` + `StopCoroutine` no raid-end. Um `HttpClient` deve ser **plugin-scope reusado**, nunca per-request; um `CancellationTokenSource` deve ser **fresh por raid**. `csharp-mod-best-practices` §1/§2.
- **Taxa:** per-raid (🟠).

### THRD — thread/async/timer longevo retendo o grafo
- **Sintoma:** `Task.Run`/`new Thread`/`System.Timers.Timer` que captura estado do raid e sobrevive a ele; `async void` (exceção some e o método pode nunca liberar). Toca Unity API fora da main thread (corrompe/lança).
- **Grep (alocação):** `Task\.Run|new Thread|System\.Timers\.Timer|async void|ThreadPool`.
- **Release pareado:** `CancellationToken` amarrado ao tempo de vida da raid, checado nos suspension points; join/cancel no teardown. `csharp-mod-best-practices` §2.
- **Taxa:** per-raid (🟠), 🔴 se o timer dispara trabalho por frame.

### HOT — alocação em hot path (pressão de GC, não retenção)
- **Sintoma:** `new`, LINQ (`Where`/`Select`/`ToList`), `string.Format`/concatenação, boxing dentro de postfix de `Update`/`FixedUpdate`/AI-tick. Não é retenção — mas gera lixo por frame; no headless (que roda GC incremental **em raid**) vira hitch + RAM churn, e multiplica por N bots.
- **Grep (alocação):** achar o patch em `Update|FixedUpdate|LateUpdate|Tick|ManualUpdate` e dentro dele `new |\.Where\(|\.Select\(|\.ToList\(|\.ToArray\(|string\.Format|\$"`.
- **Release pareado:** N/A (não retém) — o fix é **não alocar**: cachear buffers, `for` manual, `StringBuilder`, `ArrayPool<T>`. `spt-mod-best-practices` §3, `csharp` §1 "Allocations".
- **Taxa:** per-frame/tick (🔴 como custo de GC; rebaixar para 🟡 se a alocação é minúscula e rara).

### SRV — retenção server-side
- **Sintoma:** no server C#, cache/coleção `static` ou campo de singleton `[Injectable(InjectionType.Singleton)]` que cresce por profile/raid sem eviction; handler/rota/callback registrado e nunca removido; `event +=` num serviço singleton.
- **Grep (alocação):** `static .*(Dictionary|List|ConcurrentDictionary)` + campos de coleção em classes `[Injectable]` singleton; `+= ` em serviços.
- **Release pareado:** eviction por profile-logout / raid-end, ou limite de tamanho. Lembrar: dado **imutável** carregado 1× (DatabaseService) **não** é leak. Ver §2 (server) e `spt-mod-best-practices` §4.
- **Taxa:** per-raid/per-profile (🟠).

---

## 4. Eixo de taxa de acúmulo (define a severidade base)

| Taxa | Gatilho | Impacto no headless | Severidade base |
|---|---|---|---|
| **per-frame/tick** | `Update`/`FixedUpdate`/AI-tick | mata em **minutos** | 🔴 |
| **per-raid** | 1× por raid (start sem release no end) | acumula em **horas**, raid a raid — **causa clássica do OOM do headless** | 🟠 |
| **per-event** | spawn de bot, tiro, hit, item | escala com **atividade** | 🟡 |
| **per-boot/one-shot** | 1× no Awake/boot | constante, não cresce (headless reinicia) | 🟢 |

Ajuste a severidade base pelo que é **retido** (grafo de raid inteiro sobe; valor escalar desce) e pela **certeza** (confirmado por leitura vs. hipótese). Um `STAT`/`EVT` per-raid que retém `GameWorld` → 🔴.

---

## 5. Procedimento de auditoria estática (parear alocação ↔ release)

1. **Classificar o mod:** client (`BepInEx`, `Plugin.cs`, `[HarmonyPatch]`) / server (`[Injectable]`, sem `UnityEngine`) / combo. Define quais mecanismos se aplicam (UNITY/HOT só client; SRV só server; LIFE/EVT/STAT/DISP/THRD ambos).
2. **Mapear a vida do mod:** achar Awake, o raid-start hook, e — crucial — **o raid-end hook**. Se não há teardown de raid e o mod aloca estado de raid → já é achado LIFE (AP-01). Use o grafo do mod (`graph-code-navigation`) para achar o teardown e os callers.
3. **Varrer superfícies de risco** com os greps do §3. Para cada alocação encontrada, **procurar o release pareado**:
   - `+= X` → existe `-= X`? (grep o mesmo símbolo)
   - `new GameObject`/`Instantiate` → existe `Destroy` do mesmo objeto?
   - `static` collection → existe `.Clear()` no raid-end?
   - `new CancellationTokenSource`/`StartCoroutine` → `Dispose`/`Cancel`/`StopCoroutine`?
   - O release está no **escopo certo para a taxa**? (per-raid → no raid-end; per-frame → não deveria alocar).
4. **Grafo aponta, leitura prova** (`graph-code-navigation`): o grep/grafo localiza o candidato; confirme abrindo `arquivo.cs:linha` e lendo o fluxo. Não reporte um achado sem ler o par alocação↔release.
5. **Cruzar com a memória do mod** (`sessions.md`): leak já resolvido não volta; leak conhecido pendente é reforçado.
6. **Classificar:** mecanismo (§3) × taxa (§4) → severidade. Emitir achado com alocação, release esperado, o grep negativo que prova a ausência, e o fix acionável.

---

## 6. Confirmação in-game (a estática só levanta hipótese)

Retenção **só se prova medindo**. Ordem, priorizando 🔴/🟠:
- **RSS entre menus:** medir a memória residente do processo no menu antes da raid 1, e no menu após cada raid (raid1→exit→menu→raid2→…). **Crescimento monotônico entre menus** = leak **per-raid** (LIFE/EVT/STAT/DISP).
- **RSS dentro de raid longa (>20 min, com bots/tiros):** subida contínua na mesma raid = **per-frame/per-event** (HOT/EVT/UNITY).
- **Teardown sujo:** encerrar por **alt-F4 / morte / MIA** (não só extract) e ler `LogOutput.log`: exceção no stop-hook = teardown não-idempotente (AP-01/LIFE). É o caminho que o headless mais exercita.
- **Headless real:** N raids seguidas no headless com só este mod (+ deps); observar RSS/pagefile entre raids — reproduz o OOM.
- **Heap snapshot** (dnSpy / dotMemory anexado): comparar 2 snapshots em pontos equivalentes (menu pós-raid1 vs. menu pós-raidN). Os **tipos que crescem** apontam o mecanismo — `Player`/`GameObject`/handlers acumulando confirmam EVT/STAT.

Matriz mínima de repro (mesma do `fix.md.tmpl`): **raid1→exit→raid2** (per-raid) + **alt-F4/morte/MIA** (teardown).

---

## 7. O que NÃO é leak do mod (falsos positivos)

- **Baseline do EFT/Unity/Fika:** o jogo base vaza; o RAM cleaner do headless roda a cada 15 min; o restart-após-N-raids é esperado. Crescimento que **também ocorre sem o mod** não é achado do mod — compare com/sem.
- **Dado imutável carregado 1× no boot** (DatabaseService no server; assets carregados no Awake e reusados a raid toda): retenção constante, **não** crescente. Não é leak.
- **Objeto Unity parenteado a algo que o EFT destrói** (`gameWorld.transform`, `mainPlayer.gameObject`): destruído automaticamente no fim da raid. Não precisa de `Destroy` manual.
- **Cache intencional com limite/eviction:** retenção limitada é design, não leak (ex.: `CustomClasses/backlog/037-performance-cache`).
- **Harmony patches são globais e persistem entre raids de propósito** — não registre/desregistre por raid; faça o corpo do patch checar `Singleton<GameWorld>.Instantiated` e sair cedo. Registrar patch por raid é que seria o bug.
- **Não** proponha `GC.Collect()`, `Resources.UnloadUnusedAssets()` ou um "RAM cleaner" no mod — o headless bane cleaners de terceiros e isso causa hitch (§2).

---

## 8. Padrões preventivos de arquitetura (desenhar para não vazar)

Detectar é remediar tarde. Estes padrões **evitam** o leak na origem — cada um responde a um mecanismo do §3. Prefira propô-los na spec técnica (`/create-technical-spec`) a caçá-los depois.

### 8.1 Um `RaidSession` dono de tudo (ownership centralizado) — resolve LIFE/EVT/STAT/DISP
- **Padrão:** todo estado de raid vive num único objeto `RaidSession`, criado no raid-start e `Dispose`ado no raid-end. Um **único** ponto de teardown idempotente, não release espalhado por 10 classes (que é onde um se esquece).
- **Idiom "disposable bag":** na hora de **alocar**, registre o **release** na mesma linha — acumule `Action`s de cleanup e rode todas no `Dispose()`:
  ```csharp
  _cleanup.Add(() => world.OnPersonAdd -= OnPersonAdd);   // registrado ao subscrever
  _cleanup.Add(() => UnityEngine.Object.Destroy(marker)); // registrado ao instanciar
  // no teardown (idempotente com _ended):
  foreach (var c in _cleanup) c(); _cleanup.Clear();
  ```
  Garante que cada alocação nasce com seu release — o par nunca fica órfão. Cobre extract/death/MIA/alt-F4 de uma vez (`spt-mod-best-practices` §2; AP-01).

### 8.2 Subscribe/unsubscribe simétrico e weak-event para publisher estático — resolve EVT
- Subscrever e dessubscrever **no mesmo objeto**, co-localizados (idealmente via 8.1). Todo `+=` tem seu `-=` no teardown.
- Para publisher **estático longevo** (evento em singleton que dura o processo inteiro, comum no headless), o `-=` esquecido segura o assinante para sempre — use **weak-event** (`WeakReference` ao alvo, ou um `WeakEventManager`), que deixa o GC coletar o assinante mesmo sem `-=`. [fonte externa: michaelscodingspot.com "5 techniques to avoid memory leaks by events"]
- Evite closures que capturam `this` em subscriptions longevas — capture só o campo necessário num local. [fonte externa: docs.unity3d.com GC best practices]

### 8.3 Object pooling para objetos recorrentes — resolve UNITY/HOT
- Objeto que nasce/morre por **evento repetido** (marcador de HUD, projétil, ícone, linha de texto, partícula): **pool e reuse** em vez de `Instantiate`/`Destroy` a cada vez. Pooling corta alocações de heap drasticamente e elimina o churn de `Destroy` (que também gera trabalho de GC). [fonte externa: docs.unity3d.com; embrace.io "GC spikes in Unity" — pooling reduz spikes]
- Unity ≥ 2021 traz `UnityEngine.Pool.ObjectPool<T>`; para dados puros, um `Stack<T>`/`Queue<T>` de instâncias reaproveitadas basta. Regra: o mod **empresta** do pool no evento e **devolve** no fim do evento — nunca `Destroy` um objeto pooled.

### 8.4 Zero-alloc no hot path (buffers reusados) — resolve HOT
- Em `Update`/`FixedUpdate`/AI-tick: **nenhum `new`**. Reuse campos de instância (`List<T>`/`T[]` limpos com `.Clear()`, não recriados), `ArrayPool<T>.Shared`, `StringBuilder` cacheado; `for` manual em vez de LINQ; `struct readonly` passado por `in`/`ref`.
- Evite **boxing** (passar `int`/`enum` para API `object`, incluindo `string.Format`) e **closures/lambdas** em código por-frame — ambos são fontes silenciosas de lixo. [fonte externa: docs.unity3d.com; gamedeveloper.com "reducing memory allocations"]
- Cache `MethodInfo`/`FieldInfo`/`AccessTools.*` em `static readonly` e o `Singleton<T>.Instance` num local dentro do método (`csharp-mod-best-practices` §3, `spt-mod-best-practices` §3).

### 8.5 Deixe o EFT destruir por você (parentear ao objeto certo) — resolve UNITY
- `GameObject` do mod parenteado a `gameWorld.transform` ou `mainPlayer.gameObject` é **destruído automaticamente** no fim da raid — menos código de teardown, menos chance de esquecer. Só gerencie `Destroy` manual quando precisar de vida diferente da do pai. Nunca parenteie a `null`/raiz persistente algo com escopo de raid (sobrevive → leak).

### 8.6 DI lifetime correto no server (espelhar a arquitetura do SPT) — resolve SRV
- O SPT trata **dado imutável de jogo como singleton carregado 1× no boot** (`DatabaseService`) e **estado por-request como transient**. Espelhe: `[Injectable(InjectionType.Singleton)]` só para stateless/imutável; **não pendure estado de profile/raid num singleton** sem eviction.
- Cache mutável server-side = **cache com limite e eviction explícita** (por profile-logout / raid-end / tamanho máximo), nunca um `static Dictionary` que só cresce. `NoGCRegionMiddleware` já cuida do GC durante o request — não force GC no mod. [fonte externa: deepwiki sp-tarkov/server-csharp]

### 8.7 Cache só o que dói recomputar (e com limite) — resolve STAT/SRV
- Cache é retenção deliberada; justifique-a. Cachear é certo quando o custo de recomputar é alto e o resultado é reusado (ex.: `CustomClasses/037-performance-cache`, invalidado por `_dataVersion`). Cachear "por via das dúvidas" é só retenção sem dono. Todo cache precisa de **chave de invalidação** e **limite**.

### 8.8 Meça, não adivinhe (feche o loop) — resolve todos
- Padrões acima reduzem risco; **só a medição prova**. Deixe o mod **fácil de medir**: um log opcional (gated por config) de "objetos vivos"/contagem do pool no raid-end torna a regressão de leak visível sem anexar profiler. Rode a matriz do §6 antes de considerar "sem leak". [fonte externa: illogika unity-best-practices; site24x7 ".NET memory leaks"]

> Ao escrever a spec técnica de um mod que aloca estado de raid, **cite qual destes padrões o mod adota** — é a forma barata de o review confirmar que o leak foi desenhado para fora, não deixado para a auditoria achar.

## Checklist de auditoria (usar em `/analyze-memory-leak` e nos reviews)

1. **Teardown existe?** Há hook de raid-end (`GameWorld.OnDestroy` **e** `BaseLocalGame.Stop`), idempotente (`_ended`)? (LIFE / AP-01)
2. **Subscriptions pareadas?** Todo `+=`/`.Subscribe`/`AddListener` tem `-=` no teardown? Closures não capturam `this` sem necessidade? (EVT)
3. **Static limpo?** Toda coleção/cache `static` tem `.Clear()`/eviction? Não retém `Player`/`GameObject`/`Profile` além da raid? (STAT)
4. **Unity destruído?** Todo `new GameObject`/`Instantiate`/`.material`/`AssetBundle`/`Texture` tem `Destroy`/`Unload(true)` — ou está parenteado a objeto que o EFT destrói? (UNITY)
5. **IDisposable disposto?** `CancellationTokenSource`/`Coroutine`/`Timer`/`Stream` com `Dispose`/`Cancel`/`StopCoroutine` no escopo certo? `HttpClient` plugin-scope, CTS fresh por raid? (DISP)
6. **Threads/async amarrados à raid?** `CancellationToken` fluindo, sem `async void` fora de event handler, sem timer órfão? (THRD)
7. **Hot path limpo?** Zero `new`/LINQ/`string.Format`/boxing em `Update`/`FixedUpdate`/AI-tick? Reflection cacheada? (HOT)
8. **Server-side:** caches em singleton com eviction por profile/raid? Sem `static` que só cresce? Dado imutável distinguido de cache mutável? (SRV)
9. **Taxa atribuída:** cada achado tem taxa de acúmulo (per-frame/raid/event/boot) e a severidade reflete isso? (§4)
10. **Não redundante com o ambiente:** a sugestão libera o que o mod alocou, sem `GC.Collect`/`UnloadUnusedAssets`/RAM cleaner? (§2/§7)

Se um item falha, é achado `ML-NN-MM` no relatório (ou 🔴 no review técnico/code-review). Confirmação final é sempre in-game (§6).