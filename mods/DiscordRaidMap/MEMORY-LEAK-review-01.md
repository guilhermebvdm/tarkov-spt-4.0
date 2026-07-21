# DiscordRaidMap — Análise de Memory Leak · 01

**Mod:** DiscordRaidMap · **Tipo:** client (BepInEx/Harmony; roda **só no host** — ver `HostCheck`)
**Escopo analisado:** `modded/` inteiro
**Data:** 2026-07-20

> Auditoria **estática** de vazamento de memória (retenção não liberada + pressão de GC) no código do mod. Cada achado recebe um ID `ML-01-MM` **permanente**. Análise estática **levanta hipóteses priorizadas**; a confirmação é in-game (ver `## Plano de confirmação`). Skill de referência: `spt-memory-leak-analysis`.
>
> 🎯 **Contexto:** este mod entrou na investigação por ter aparecido **por último no `LogOutput.log` de um OOM** do headless, na 1ª raid, com ~100 mods carregados. Pela skill §1.2, o "último no log" costuma ser a **vítima**, não o culpado — mas aqui o mod tem um mecanismo de **consumo agudo intra-raid** que o torna um **suspeito real**, não só um espectador. `HostCheck.CanBroadcast()` ([HostCheck.cs:13](modded/RaidMap/HostCheck.cs#L13)) confirma que ele **só roda no host = headless**.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 2 · 🟡 Médios: 2 · 🟢 Menores: 1 · Total: 5
> Superfícies de risco varridas: subscriptions (7), static (2), IDisposable (4), render CPU (1), hot-path snapshot (1) · **Teardown: correto** (sem leak de subscription — ver Panorama)

## Veredito rápido

- **NÃO é um leak clássico de retenção entre raids.** O teardown do mod é sólido: `Plugin.OnDestroy` desinscreve todos os eventos ([Plugin.cs:43-56](modded/Plugin.cs#L43)), `RaidStateCollector.Dispose` faz `-=` pareado ([RaidStateCollector.cs:346](modded/RaidMap/RaidStateCollector.cs#L346)), `RaidMapLifecycle.OnRaidEnd → StopBroadcaster` para tudo. Um raid1→raid2 não vaza subscriptions.
- **O problema é CONSUMO AGUDO INTRA-RAID:** o mod renderiza a imagem do mapa **na CPU via System.Drawing**, a cada `UpdateIntervalSeconds` (default **5 s**, min 2 s), **durante toda a raid, no headless**, alocando dezenas de MB no **Large Object Heap** por render. Isso bate com o perfil "crash na 1ª raid" (skill §1.1: per-event agudo), e **soma ao agregado** de ~100 mods (skill §1.2). **ML-01-01 é o suspeito nº 1.**

## Mecanismos (categoria) · Taxa · Impacto

Ver escala completa na skill `spt-memory-leak-analysis` §3 (mecanismos) e §4 (taxa de acúmulo).

## Panorama

- **Tipo:** client BepInEx, **host-only** (`HostCheck`). Roda no headless; early-return nos Fika clients.
- **Vida do mod:** Awake registra 4 patches + subscreve eventos de Settings ([Plugin.cs:15-36](modded/Plugin.cs#L15)) · raid-start = `GameWorld.OnGameStarted` postfix → `RaidMapLifecycle.OnGameStarted` cria collector+renderer+discord+broadcaster · raid-end = `GameWorld.OnDestroy` prefix → `StopBroadcaster`.
- **Ponto de teardown:** ✅ presente e correto. `Plugin.OnDestroy` ([Plugin.cs:43](modded/Plugin.cs#L43)) + `RaidMapLifecycle.Dispose`/`StopBroadcaster` + `RaidStateCollector.Dispose` + `Renderer.Dispose` + `DiscordWebhookClient` dispõe forms/CTS. **Não** hooka `BaseLocalGame.Stop`, mas hooka `GameWorld.OnDestroy` — que cobre extract/death/MIA (o objeto morre em todos). ⚠️ ver ML-05 sobre alt-F4.
- **Superfícies → release:** todo `+=` tem `-=` (Plugin 22-25 ↔ 47-50; collector 53-54 ↔ 348-349). `static List Airdrops` limpa no OnDestroy ([RaidPatches.cs:40](modded/Patches/RaidPatches.cs#L40)). `HttpClient` é **plugin-de-raid-scope** reusado (bom, não per-call). CTS é `using` por request (bom).
- **Buffers de mapa (dimensões reais):** customs **3971×2000 = 30,3 MB** por `Color32[]`; woods 14,3 MB; reserve 14,0; labs 12,4; labyrinth 12,3; streets 11,1; groundzero 10,9; shoreline 10,1; lighthouse 9,4; factory 9,3; interchange 7,2.
- **Leaks conhecidos na memória do mod:** sem memória prévia (`sessions.md` inexistente — mod recém-adicionado).

## Índice

| ID | Mec | Taxa | Impacto | Título | Status |
|---|---|---|---|---|---|
| ML-01-01 | HOT | per-event (~5s) | 🟠 | Render CPU aloca 2× buffer grande no LOH por tick (30 MB×2 em Customs) — **suspeito nº1 do OOM** | Pendente |
| ML-01-02 | HOT/DISP | per-event | 🟠 | `Font`/`Bitmap`/`Graphics` GDI+ recriados por label e por chamada (churn de handles não-gerenciados) | Pendente |
| ML-01-03 | THRD | per-event | 🟡 | System.Drawing/GDI+ executado em `Task.Run` (thread de background) — risco de instabilidade no headless | Pendente |
| ML-01-04 | HOT | per-event | 🟡 | LINQ/`ToList`/`Concat`/`Cast` + reflection por-player em `CollectSnapshot`/`RefreshKilledPlayers` a cada tick (× N bots) | Pendente |
| ML-01-05 | STAT/LIFE | per-event | 🟢 | Listas de mortos crescem intra-raid retendo `Player`/corpo; teardown não cobre `BaseLocalGame.Stop` | Pendente |

---

## Achados

### ML-01-01 · HOT — per-event (~5 s) · 🟠 Forte (**suspeito nº 1 do OOM**)

**Render na CPU aloca dois buffers grandes no Large Object Heap por tick de render**

**Local:** [`modded/RaidMap/Renderer.cs:40-69`](modded/RaidMap/Renderer.cs#L40) · [`modded/RaidMap/Renderer.cs:119-124`](modded/RaidMap/Renderer.cs#L119) · [`modded/RaidMap/Renderer.cs:456-490`](modded/RaidMap/Renderer.cs#L456)

**Alocação (onde nasce):** por render, `Render()` chama `CopyBackground` (`new Color32[W*H]`, [Renderer.cs:121](modded/RaidMap/Renderer.cs#L121)) **e** `EncodePng` (`new byte[stride*H]` + `new Bitmap(W,H)` + `MemoryStream` + `ToArray()`, [Renderer.cs:458-489](modded/RaidMap/Renderer.cs#L458)). Disparado por `RaidBroadcaster.Update` a cada `_updateIntervalSeconds` ([RaidBroadcaster.cs:35-53](modded/RaidMap/RaidBroadcaster.cs#L35)).

**Release esperado:** N/A — não é retenção; os arrays são coletáveis após o render. O problema é o **volume e o destino (LOH)**, não a retenção.

**Problema:** cada array > 85 KB vai para o **Large Object Heap**. Em Customs (`bigmap`), `Color32[]` = **30,3 MB** e o `byte[]` do encode = outros **30,3 MB** — **~60 MB de alocações LOH por render**, a cada 5 s (2 s no mínimo), *durante toda a raid*. Numa raid de 20 min a 5 s = ~240 renders = **~14 GB de churn no LOH**. O LOH **não é compactado** pelo GC incremental que o headless roda em raid (`GarbageCollector.CollectIncremental` — ver skill §2) → **fragmentação + crescimento monotônico do working set**.

**Por que importa:** consumo agudo intra-raid (skill §1.1) que **cresce durante a raid** e **soma ao agregado** de ~100 mods (§1.2). Num processo já perto do teto, é um gatilho direto e plausível do `OutOfMemory` na 1ª raid. É o único mecanismo do mod que explica um OOM sozinho na 1ª raid.

**Sugestão:**
1. **Reusar os buffers** (skill §8.4): o tamanho do mapa é fixo durante a raid. Alocar `Color32[] _canvas` e `byte[] _encodeBuffer` **uma vez** por mapa (ou no `Renderer`, por dimensão) e **reusar** a cada render — `Array.Copy` do background para o canvas reusado em vez de `new`. Elimina o churn LOH.
2. **Reusar um único `Bitmap`/`MemoryStream`** de encode em vez de recriar (com `using` só no Dispose do Renderer).
3. **Só renderizar quando muda** e/ou aumentar o default de `UpdateIntervalSeconds` (5 s → 15 s reduz o churn em 3×; combinar com (1)).
4. Confirmar com o Plano de confirmação (medir RSS ao longo de 20 min de raid em Customs).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### ML-01-02 · HOT/DISP — per-event · 🟠 Forte

**`Font`/`Bitmap`/`Graphics` do GDI+ recriados por label e por chamada (handles não-gerenciados)**

**Local:** [`modded/RaidMap/Renderer.cs:292-340`](modded/RaidMap/Renderer.cs#L292) (`MeasureText`, `DrawTrueTypeText`, `CreateConfiguredFont`)

**Alocação (onde nasce):** `CreateConfiguredFont` faz `new System.Drawing.Font(...)` **a cada chamada** ([Renderer.cs:339](modded/RaidMap/Renderer.cs#L339)); `MeasureText` e `DrawTrueTypeText` criam `Bitmap` + `Graphics` de medição **por label** ([Renderer.cs:294-313](modded/RaidMap/Renderer.cs#L294)). Chamado 1–2× por marcador de player/morto, por render.

**Release esperado:** disposto via `using` (presente — bom). O problema é o **churn de objetos GDI+ não-gerenciados** (cada `Font`/`Bitmap`/`Graphics` é um handle GDI nativo).

**Problema:** GDI+ tem limite de handles por processo (~10 000). Criar/destruir `Font` + 2 `Bitmap` + 2 `Graphics` por label, por render, num headless com vários players/mortos e a cada 5 s, é churn alto de recursos nativos. Qualquer caminho de exceção que escape um `using` (ex.: fonte inválida) vaza handles GDI — que **não** aparecem no heap gerenciado (o profiler de heap não os vê), mas derrubam o processo com "GDI+ generic error" / falha de alocação nativa.

**Por que importa:** soma ao consumo do ML-01-01 e adiciona um vetor de instabilidade nativa que um heap snapshot gerenciado não detecta. No headless de longa duração, esgotamento de handles GDI é um modo de falha real.

**Sugestão:** cachear a `Font` como campo do `Renderer` (criar uma vez em `CreateConfiguredFont`, dispor no `Dispose` — §8.4/§8.1); reusar um único `Bitmap(1,1)`+`Graphics` de medição como campos. Idealmente medir texto sem `System.Drawing` (ver ML-01-03).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### ML-01-03 · THRD — per-event · 🟡 Médio

**System.Drawing/GDI+ executado numa thread de background (`Task.Run`)**

**Local:** [`modded/RaidMap/RaidBroadcaster.cs:62-72`](modded/RaidMap/RaidBroadcaster.cs#L62) (`Task.Run(() => _renderer.Render(...))`)

**Problema:** `Render()` usa `System.Drawing`/GDI+ inteiro (`Bitmap`, `Graphics`, `Font`, `LockBits`) dentro de um `Task.Run`, ou seja, numa **thread do thread-pool**, não na main thread. GDI+ não é garantido thread-safe; em Unity Mono (headless), `System.Drawing` depende do backend nativo (libgdiplus/GDI+) e é uma fonte conhecida de instabilidade/vazamento sob concorrência. O `_rendererLock` ([RaidBroadcaster.cs:12](modded/RaidMap/RaidBroadcaster.cs#L12)) serializa os renders (bom), mas não muda o fato de ser cross-thread.

**Por que importa:** não é leak puro, mas um crash/corrupção do renderer no headless também derruba a raid. Relevante para a confiabilidade do host — e a análise de um OOM pode estar mascarando uma falha de GDI+ nativa (que não seria "out of managed memory").

**Sugestão:** manter o render fora da main thread é desejável para não travar o tick do headless, mas então garantir GDI+ estritamente serializado (já está) e, no médio prazo, avaliar substituir `System.Drawing` por composição de pixels 100% gerenciada (o mod **já** faz o blend de markers em `Color32[]` manualmente — só o **texto** depende de GDI+; um atlas de glifos pré-renderizado eliminaria a dependência). Registrar como dívida se não for agora.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### ML-01-04 · HOT — per-event · 🟡 Médio

**LINQ/`ToList`/`Concat`/`Cast` + reflection por-player em `CollectSnapshot` a cada tick**

**Local:** [`modded/RaidMap/RaidStateCollector.cs:211-223`](modded/RaidMap/RaidStateCollector.cs#L211) (`RefreshKilledPlayers`) · [`RaidStateCollector.cs:141-158`](modded/RaidMap/RaidStateCollector.cs#L141) (`AddExtracts` `Concat`/`Cast`) · [`RaidStateCollector.cs:300-320`](modded/RaidMap/RaidStateCollector.cs#L300) (`GetReferencePlayer` `.Where().ToList()`)

**Problema:** a cada snapshot (~5 s), `RefreshKilledPlayers` itera `AllPlayersEverExisted` e chama `HasCorpse` (reflection `FieldInfo.GetValue`) **por player** — no headless com dezenas/centenas de bots, são N reflection calls por tick. `AddExtracts` aloca via `Concat`/`Cast`/`?? []`. Não é o dominante (ML-01-01 domina o volume), mas é alocação e CPU recorrentes que escalam com o nº de bots.

**Por que importa:** pressão de GC secundária + custo de CPU no tick do headless (que precisa manter ≥30 FPS — skill §2). `GetReferencePlayer` cacheia `_headlessReferencePlayer` após a 1ª busca (bom), então o custo maior é o `RefreshKilledPlayers` por-player.

**Sugestão:** manter um índice incremental de mortos (o mod já reage a `PlayerOnDeadPatch.OnDead`) e **não** re-varrer `AllPlayersEverExisted` por reflection a cada tick — usar o evento como fonte primária e reservar a varredura para um fallback esparso. Cachear o resultado de extracts (mudam pouco).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### ML-01-05 · STAT/LIFE — per-event · 🟢 Menor

**Listas de mortos crescem intra-raid retendo `Player`/corpo; teardown não cobre `BaseLocalGame.Stop`**

**Local:** [`modded/RaidMap/RaidStateCollector.cs:44-47`](modded/RaidMap/RaidStateCollector.cs#L44) (`_deadPlayers`/`_killedEnemies`/`_killedBosses`) · [`modded/Patches/RaidPatches.cs:31-33`](modded/Patches/RaidPatches.cs#L31) (raid-end via `GameWorld.OnDestroy` só)

**Problema:** (a) as listas de mortos acumulam refs de `Player` (com o corpo) durante a raid — esperado (precisa desenhá-los), limpas no `Dispose` (per-raid OK). Numa raid muito longa com muitas mortes, retenção cresce, mas é limitada e liberada no fim. (b) O raid-end é hookado só em `GameWorld.OnDestroy`; a skill/`spt-mod-best-practices §2` recomenda **também** `BaseLocalGame.Stop` por robustez. Na prática `OnDestroy` cobre extract/death/MIA/alt-F4 (o `GameWorld` é destruído em todos), então o risco aqui é baixo — mas se um caminho de saída destruir o `GameWorld` sem chamar o prefix, o `static Airdrops` e o collector ficariam até o próximo start (que também limpa via `StopBroadcaster`). 

**Por que importa:** baixo — o teardown é idempotente na prática e um novo raid-start limpa o estado anterior. Registrado para completude.

**Sugestão:** opcional — adicionar hook em `BaseLocalGame.Stop` como segundo gatilho idempotente (guard já existe via `StopBroadcaster` null-check). Não urgente.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## Plano de confirmação (in-game — a análise estática só levanta hipóteses)

> A retenção/consumo só se prova medindo. Priorizar ML-01-01.

- [ ] **Log:** este mod entrou por um `OutOfMemory` já observado. Se reproduzir, confirmar no `LogOutput.log` que é `OutOfMemory`/`bad_alloc` (memória) e não uma exceção de GDI+ (ML-01-03).
- [ ] **RSS dentro de raid longa (>20 min) em Customs (`bigmap`, maior buffer):** só este mod (+ deps) no headless, `UpdateIntervalSeconds=5`. RSS subindo continuamente = confirma ML-01-01 (churn LOH). É o teste decisivo.
- [ ] **A/B do intervalo:** repetir com `UpdateIntervalSeconds=30` — se a subida de RSS cai proporcionalmente, o render é a causa.
- [ ] **A/B do fix:** aplicar ML-01-01 (buffers reusados) e repetir — RSS deve estabilizar.
- [ ] **Heap snapshot (dnSpy/dotMemory):** comparar início vs. 15 min. `Color32[]`/`byte[]` grandes vivos + tamanho do LOH crescente confirmam. ⚠️ handles GDI (ML-01-02) **não** aparecem no heap gerenciado — cruzar com o contador de "GDI objects" do Process Explorer.
- [ ] **raid1 → exit → raid2:** confirmar (como esperado da leitura) que **não** há crescimento entre menus — o mod não vaza entre raids; o problema é intra-raid.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-20 | Análise de memory leak 01 criada via `/analyze-memory-leak` |
