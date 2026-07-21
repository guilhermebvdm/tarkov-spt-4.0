# DiscordRaidMap — Análise de Memory Leak · 02

**Mod:** DiscordRaidMap · **Tipo:** client (host-only via `HostCheck`)
**Escopo analisado:** `modded/` inteiro, **estado v1.1.2** (pós MEMORY-LEAK-review-01 + CODE-review-01 aplicados)
**Data:** 2026-07-21

> Re-auditoria completa (skill `spt-memory-leak-analysis`) após todos os fixes. Foco: confirmar que os leaks foram fechados e varrer o mod inteiro por retenção/pressão residual + gestão de dependências. IDs `ML-02-MM`. Referência anterior: [MEMORY-LEAK-review-01.md](MEMORY-LEAK-review-01.md).

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 1 · 🟢 Menores: 3 · Total: 4
> Veredito: **sem leak acionável.** O driver do OOM (churn de LOH) foi eliminado; resta 1 dívida de estabilidade (GDI+ em thread de fundo) e 3 minudências.

## Panorama — verificado LIMPO (por mecanismo)

| Mec | Situação | Evidência |
|---|---|---|
| **LIFE** | ✅ Teardown idempotente cobrindo todas as saídas. `GameWorld.OnDestroy` → `StopBroadcaster` (null-check). O renderer é disposto **mesmo com upload em voo** (finally de `RenderAndUploadAsync` quando `!_started`), e `Stop()` só dispõe direto se `!_uploading` — sem use-after-dispose nem double-dispose. | [RaidBroadcaster.cs:80-123](modded/RaidMap/RaidBroadcaster.cs#L80) |
| **EVT** | ✅ Todo `+=` pareado com `-=` no `Plugin.OnDestroy` (7 configs + 2 eventos de patch). O `RaidStateCollector` **não assina mais nada**. | [Plugin.cs:46-61](modded/Plugin.cs#L46) |
| **STAT** | ✅ A `static List Airdrops` foi eliminada. Estáticos restantes são imutáveis (`JpegEncoder`, `BossRoles`, `FieldInfo`s) ou process-scope por design (`Http`). | [RaidPatches.cs](modded/Patches/RaidPatches.cs) · [Renderer.cs:33](modded/RaidMap/Renderer.cs#L33) |
| **UNITY** | ✅ Zero objetos Unity criados (render é CPU/System.Drawing, não Texture/GameObject). Referências a `Player`/`AirdropSynchronizableObject` são **lidas**, não possuídas. | — |
| **DISP** | ✅ `Renderer`/`RaidStateCollector`/`RaidMapLifecycle` implementam e propagam `Dispose`. Descartáveis por render (`MemoryStream`, `EncoderParameters`, `textBitmap`, CTS/HttpRequestMessage) todos em `using`/`finally`. | [Renderer.cs Dispose](modded/RaidMap/Renderer.cs) · [DiscordWebhookClient.cs](modded/RaidMap/DiscordWebhookClient.cs) |
| **THRD** | ⚠️ Render roda em `Task.Run` (fundo), serializado por `_rendererLock`; fire-and-forget guardado por `_uploading` (só 1 em voo). Sem thread órfã. Ressalva GDI+ = ML-02-01. | [RaidBroadcaster.cs:62-93](modded/RaidMap/RaidBroadcaster.cs#L62) |
| **HOT** | ✅ Churn de LOH eliminado: downscale único do fundo + reuso de `_canvas`/`_encodeBuffer`/`_encodeBitmap`/`Font`. Alocações restantes são **por intervalo (~15 s)**, não por frame. | [Renderer.cs GetCanvas/EncodeImage](modded/RaidMap/Renderer.cs) |
| **SRV** | N/A (mod client). | — |

**Dependências (gestão):** `Fika.Core` é **soft dependency** resolvida por reflection (`HostCheck` via `Chainloader.PluginInfos` + `AppDomain` assemblies, com `Lazy` cacheando o `PropertyInfo`); degrada graciosamente se ausente (broadcast liberado sem Fika). Sem NuGet novo. `System.Drawing`/`System.Net.Http` são do framework (net472). **Ganho colateral do refactor:** `HostCheck.CanBroadcast()` agora é chamado **1×/raid** (no start) — as chamadas per-evento sumiram com os patches removidos.

## Índice

| ID | Mec | Taxa | Impacto | Título | Status |
|---|---|---|---|---|---|
| ML-02-01 | THRD | per-event | 🟡 | System.Drawing/GDI+ executa em thread de fundo (Mono) | Dívida (deferido) |
| ML-02-02 | HOT | per-event | 🟢 | Alocações por intervalo remanescentes (MemoryStream/ToArray/snapshot/reflection) | Aceito |
| ML-02-03 | LIFE | per-raid | 🟢 | Raid-end só em `GameWorld.OnDestroy`, não `BaseLocalGame.Stop` | Aceito |
| ML-02-04 | STAT | per-event | 🟢 | Listas de mortos acumulam refs de `Player` na raid (by design) | Aceito |

---

## Achados

### ML-02-01 · THRD — per-event · 🟡 Médio (dívida conhecida, carryover ML-01-03)

**System.Drawing/GDI+ executa numa thread de fundo (`Task.Run`)**

**Local:** [`modded/RaidMap/RaidBroadcaster.cs:66-72`](modded/RaidMap/RaidBroadcaster.cs#L66) · todo o `Renderer` (GDI+).

**Problema:** o render usa `System.Drawing`/GDI+ (`Bitmap`, `Graphics`, `Font`, `LockBits`) dentro de `Task.Run` — thread do pool, não a main. Com o reuso de buffers, os objetos GDI (`_encodeBitmap`, `_cachedFont`, `_measureGraphics`) agora são **criados e reusados na thread de fundo**. `_rendererLock` garante que nunca há concorrência (render vs. Dispose/Replace são mutuamente exclusivos), mas GDI+ em Mono fora da main thread é uma fonte conhecida de instabilidade.

**Por que importa:** não é leak de memória gerenciada — é risco de **estabilidade nativa** (um erro de GDI+ derruba o render/raid no headless). Um OOM aparente pode até mascarar uma falha de GDI+.

**Sugestão (deferir):** manter como dívida. Solução definitiva: eliminar a dependência de GDI+ para texto (único uso que exige `Graphics`/`Font`) via **atlas de glifos pré-renderizado** compondo direto em `Color32[]` (o resto do desenho já é 100% gerenciado). Aí o render inteiro fica managed e thread-safe.

**Decisão:** `[x]` Rejeitar (aceitar como dívida) — reavaliar se aparecer erro de GDI+ no `LogOutput.log`.

---

### ML-02-02 · HOT — per-event · 🟢 Menor (aceito)

**Alocações por intervalo remanescentes**

**Local:** [`modded/RaidMap/Renderer.cs:498`](modded/RaidMap/Renderer.cs#L498) (`MemoryStream` + `ToArray`) · [`RaidStateCollector.cs:62`](modded/RaidMap/RaidStateCollector.cs#L62) (`RaidSnapshot` + markers) · `RefreshKilledPlayers` (reflection por player).

**Problema:** por intervalo (~15 s) ainda se aloca: o `MemoryStream` + `stream.ToArray()` (o payload comprimido — inevitável como saída), `EncoderParameters`, um `RaidSnapshot` + `RaidMarker`s, e reflection (`GetValue`) por player em `RefreshKilledPlayers`. Tudo **por intervalo**, não por frame — pressão de GC desprezível.

**Por que importa:** baixo. O `ToArray()` é a saída obrigatória; o resto é pequeno e cadenciado. Não justifica complexidade adicional.

**Decisão:** `[x]` Aceitar como está.

---

### ML-02-03 · LIFE — per-raid · 🟢 Menor (aceito)

**Raid-end só via `GameWorld.OnDestroy`**

**Local:** [`modded/Patches/RaidPatches.cs:27-39`](modded/Patches/RaidPatches.cs#L27)

**Problema:** o teardown é disparado só por `GameWorld.OnDestroy`. `spt-mod-best-practices` §2 recomenda também `BaseLocalGame.Stop` por robustez. Na prática o `GameWorld` é destruído em todas as saídas (extract/death/MIA/alt-F4), então `OnDestroy` cobre — e o `StopBroadcaster` é idempotente, e um novo raid-start também limpa o anterior.

**Por que importa:** baixo. Registrado para completude.

**Decisão:** `[x]` Aceitar (deferir).

---

### ML-02-04 · STAT — per-event · 🟢 Menor (by design, aceito)

**Listas de mortos acumulam refs de `Player` durante a raid**

**Local:** [`modded/RaidMap/RaidStateCollector.cs:47-52`](modded/RaidMap/RaidStateCollector.cs#L47)

**Problema:** desde o CR-01-02, `_deadPlayers`/`_killedEnemies`/`_killedBosses` acumulam refs de `Player` mortos durante a raid (para manter o marcador permanente), limpas no `Dispose` (raid-end).

**Por que importa:** **não é leak.** É retenção per-raid, bounded pelo nº de mortes, liberada no fim da raid. E **não pina** nada além do que o jogo já retém — esses `Player` vivem em `AllPlayersEverExisted` independentemente do mod. Trade-off aceito (permanência dos marcadores vs. re-derivar por intervalo).

**Decisão:** `[x]` Aceitar (by design).

---

## Comparação com a review 01 (o que fechou)

| Achado 01 | Status agora |
|---|---|
| ML-01-01 (LOH churn 30 MB×2/render — driver do OOM) | ✅ **Fechado** (downscale + reuso de buffers) |
| ML-01-02 (Font/GDI churn por label) | ✅ **Fechado** (Font/Graphics cacheados) |
| ML-01-03 (GDI+ em Task.Run) | ⚠️ Rebatizado ML-02-01 — dívida aceita |
| ML-01-04 (LINQ/reflection por tick) | ✅ **Melhorado** (agora só por intervalo; `GetReferencePlayer` cacheia/revalida) |
| ML-01-05 (listas crescem + só OnDestroy) | ✅ Coberto por ML-02-03/04 (aceitos) |

## Plano de confirmação (in-game)

- [ ] RSS **estável** ao longo de 20 min em Customs (o teste decisivo do OOM) — só este mod + deps no headless.
- [ ] `LogOutput.log` sem `OutOfMemory` **nem** erro de GDI+ (ML-02-01) numa sessão longa.
- [ ] raid1 → exit → raid2: sem crescimento entre menus (esperado — sem retenção entre raids).

## Histórico

| Data | Evento |
|---|---|
| 2026-07-21 | Análise de memory leak 02 (re-auditoria pós-fixes v1.1.2) — sem leak acionável |
