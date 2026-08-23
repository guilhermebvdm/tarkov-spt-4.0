---
title: "Anexo profiling 2026-08-22 — timeline-spikes"
date: 2026-08-22
status: 🟢 Vivo
authors: Claude (investigação de performance 2026-08-22, análise dimensional)
---

# Timeline: spikes e periodicidade (buckets de 250ms) — 4 capturas

> Dimensão: timeline-spikes · Dados: `timeline.csv`, `frames.csv`, `worst-frames/*.json`, `methods.csv` das 4 capturas · Data: 2026-08-22
> Capturas: vanilla-A (`D:\SPT_2\...\203734`, ModAttribution), vanilla-B (`203938`, ModAttribution), modded-A (`D:\SPT\...\205500`, UpdateOnly), modded-B (`205604`, UpdateOnly).
> Regras do CONTEXT.md aplicadas: normalização por frame/bucket, atenção à assimetria de modo (no modded, self do método inclui callees não instrumentados e patches Harmony — invisíveis).

## 1. Metodologia

- Série temporal = soma de `SelfMs` (main thread, ThreadId=1) por bucket de 250ms, 121 buckets (~30.25s).
- Spike de bucket = valor >2× mediana da série.
- Picos por método: bucket com self >max(1ms, 5× mediana do método); picos adjacentes (≤2 buckets) colapsados no maior; períodos = gaps entre picos.
- Scan de periodicidade: TODOS os métodos com ≥5ms totais, buscando ≥2 gaps entre picos na faixa 9.0–11.0s.
- Autocorrelação da série total em lags 4–60 buckets (1–15s), excluindo o último bucket parcial.
- Worst-frames: `frames.csv` (>25ms) mapeados a buckets via `floor(t/0.25)`; deep captures `worst-frames/frame-N.json` resolvidos por `methods.csv` da mesma captura.
- Todos os números calculados via Python 3.14 sobre os CSVs brutos; nenhum estimado.

## 2. Série total por bucket — textura vanilla × modded

| Captura | mediana (ms/bucket) | média | CV | p95/mediana | buckets >2× mediana | frames >25ms | frames >40ms |
|---|---|---|---|---|---|---|---|
| vanilla-A | 95.97 | 98.67 | 0.156 | 1.274 | 0 | 13 (0.6%) | 3 |
| vanilla-B | 83.52 | 85.72 | 0.155 | 1.284 | 0 | 13 (0.7%) | 4 |
| modded-A | 84.09 | 86.45 | 0.213 | 1.318 | 1 (b115 = 213ms) | 114 (7.6%) | 7 |
| modded-B | 78.00 | 79.72 | 0.170 | 1.281 | 0 | 41 (2.3%) | 6 |

Leitura: a **carga média managed por janela de 250ms é flat nos dois ambientes** (CV 0.15–0.21) — o que diferencia o modded não é o "nível" da série e sim hitches concentrados que aparecem no `maxFrameMs` por bucket e na contagem de frames >25ms (114 vs 13, com o vanilla rodando ~2× mais bots).

Autocorrelação da série total:

| Captura | r no lag 40 (=10s) | melhores lags |
|---|---|---|
| vanilla-A | −0.071 | 4 (0.677), 5, 11, 12 |
| vanilla-B | −0.206 | 4 (0.219), 5, 50 |
| modded-A | −0.006 | 15 (0.226), 18, 4 |
| modded-B | **+0.361 (máximo global)** | **40 (0.361), 39, 41, 38** |

No modded-B o período de 10s é visível até no agregado. No modded-A ele é mascarado pelos one-offs gigantes (AsyncWorker 195ms, InputManager 105ms).

## 3. O metrônomo de 10s — CONFIRMADO, e é `EFT.NonWavesSpawnScenario.Update`

Scan de periodicidade em **todos** os métodos das 4 capturas (≥2 gaps em 9–11s): o ÚNICO método que passa é `EFT.NonWavesSpawnScenario.Update`, e só no modded.

| Captura | ticks (timestamp do frame) | gaps | custo self por tick | calls no frame | frame resultante |
|---|---|---|---|---|---|
| modded-A | t=4.099 / 14.104 / 24.106s (b16/56/96) | **10.005s, 10.002s** | 20.23 / 20.77 / 20.89 ms | 1 | 44.4 / 45.5 / 38.4 ms |
| modded-B | t=3.941 / 13.943 / 23.959s (b15/55/95) | **10.002s, 10.016s** | 25.60 / 23.85 / 25.33 ms | 1 | 47.7 / 42.0 / 46.3 ms |
| vanilla-A | nenhum tick visível | — | total 0.65ms em 30s | — | — |
| vanilla-B | 1 tick real em t≈23.8s (b95) | — | **1.85 ms** | 1 | não entra nos worst |

- Nos deep captures dos frames de tick (modded-B #34757, modded-A #27889): `NonWavesSpawnScenario.Update` self=incl (25.32/20.88ms), calls=1 → **todo o custo está em callees não instrumentados** (modo UpdateOnly): a cadeia `BotsController.ActivateBotsWithoutWave` + quaisquer patches Harmony sobre ela.
- Decompile (`references/eft-decompiled/Assembly-CSharp/EFT/NonWavesSpawnScenario.cs`): MonoBehaviour vanilla do cenário de spawn "sem waves". O timer de 10s é **do jogo**: `const float float_1 = 10f`, e `BotSpawnPeriodCheck` é clampado para ≥10s. A cada tick: `num = BotMax − AliveLoadingDelayedBotsCount`; se `num ≤ 0` → **early-return barato** (caso vanilla, 25–32 vivos ≥ BotMax); se `num > 0` → `TrySpawn` do cenário de grupo + loop de `num` chamadas `ActivateBotsWithoutWave(1, BotProfileDataClass)` (caso modded, 7–14 vivos).
- Conclusão: **a cadência de 10s é vanilla; o custo por tick é a anomalia modded** — 20–26ms vs 1.85ms no único tick real do vanilla-B (≈11–14×). Mods presentes só no modded que tocam spawn: `TRL-DynamicSpawn.dll`, `MoreBotsAPI/`, `SAIN/`, `DrakiaXYZ-Waypoints` (inventário de `D:\SPT\BepInEx\plugins`). No vanilla o Fika patcheia `NonWavesSpawnScenario.Run` (só Run, não Update) e o SPT patcheia `BotSpawner.*`/`SpawnPoint.*` com custo medido trivial (0.03–0.20ms totais em 30s, harmony-patches.csv vanilla-B).

## 4. Rajadas de `Diz.Jobs.JobScheduler.LateUpdate` — 99% do custo dentro de [tick, +2s]

JobScheduler (decompile `Diz.Jobs/JobScheduler.cs`): executor de continuations do jogo com **budget por frame** (`FrameTicks` default 160000 ticks = 16ms; force mode ×3). `LateUpdate` drena a fila; rajada = fila cheia (jobs de criação/streaming de bots).

| Captura | total 30s | dentro das janelas [tick NonWaves, +2s] | rajadas individuais |
|---|---|---|---|
| modded-A | 308.3 ms | **304.0 ms (99%)** | b16–24: 91.1ms · b56–64: 110.9ms · b96–104: 102.1ms |
| modded-B | 392.1 ms | **387.3 ms (99%)** | b15–23: 121.1ms · b55–63: 105.9ms · b95–103: 160.3ms |
| vanilla-A | 85.5 ms | (1 rajada única) | b60–71: 84.1ms |
| vanilla-B | 90.4 ms | (1 rajada única) | b94–99: 89.4ms |

- Frames únicos com `JobScheduler.LateUpdate` de 34.0/34.3ms (modded-B #34767 t=24.23s = 58.8ms de frame; modded-A #27899 t=24.34s = 52.9ms) — o budget de 16ms é rompido (>2×).
- **Comportamento existe no vanilla** (rajada de 84–89ms ligada ao spawn/streaming real de bots, 1× por captura — em vanilla-B imediatamente após o tick de 1.85ms do NonWaves em b95). A diferença modded: **3 rajadas/30s em vez de 1**, e 6% (A) a 78% (B, janela 3) maiores.

## 5. Spawns sem efeito visível na população

Proxy de instâncias de IA = calls/frame por bucket (`AICoreLayerClass.Update` no modded, `EFT.Player.LateUpdate` no vanilla):

- modded-A: 14.0 → 13.0 (b~17) → 12.0 (b~50) → 12.0 no fim. **Nunca sobe** — nem em b16/56/96.
- modded-B: 10.0 → 9.0 → 8.0 (b~34) → 7.0 no fim. **Monotônico decrescente**, sem degrau positivo nos ticks.
- vanilla-A: 25 → 26 (degrau em b~60, coincide com a rajada JobScheduler b60–71).
- vanilla-B: 32 → 35–37 (degrau em b~96, coincide com a rajada b94–99).

No vanilla a rajada corresponde a bots que **entram de fato**. No modded o pipeline paga tick (20–26ms) + rajada (91–160ms) **a cada 10s sem nenhum bot novo aparecer no proxy de IA** — spawn abortado/falho, ou bot imediatamente limitado/removido por algum sistema (limiter/despawn). É custo recorrente sem efeito de gameplay observável na janela.

## 6. Concentração dos frames ruins nas janelas do metrônomo

Janelas [tick−0.05s, tick+2s] = ~6.15s de 30s (~20% do tempo):

| Captura | frames >25ms | dentro das janelas | excesso total acima de 16.7ms | excesso nas janelas |
|---|---|---|---|---|
| modded-A | 114 | 51 (**45%**) | 1635 ms | 824 ms (**50%**) |
| modded-B | 41 | 27 (**66%**) | 611 ms | 362 ms (**59%**) |

(No modded-A as janelas também contêm o one-off de InputManager t=4.55s e o AsyncWorker.Update t=25.02s — ver §7; a concentração não é 100% metrônomo puro, mas a âncora temporal é ele.)

Worst-frames >40ms completos:

| Captura | frame | t (s) | bucket | FrameMs | top self |
|---|---|---|---|---|---|
| modded-A | #28154 | 28.79 | 115 | 211.5 | AsyncWorker.FixedUpdate 195.1 |
| modded-A | #27935 | 25.02 | 100 | 126.5 | AsyncWorker.Update 105.5 |
| modded-A | #26883 | 4.55 | 18 | 104.9 | InputManager.Update 79.4 |
| modded-A | #27899 | 24.34 | 97 | 52.9 | JobScheduler.LateUpdate 34.3 |
| modded-A | #27369 | 14.10 | 56 | 45.5 | NonWavesSpawnScenario.Update 20.8 |
| modded-A | #26869 | 4.14 | 16 | 45.2 | GameWorldUnityTickListener.Update 16.0 |
| modded-A | #26868 | 4.10 | 16 | 44.4 | NonWavesSpawnScenario.Update 20.2 |
| modded-B | #33426 | 2.56 | 10 | 110.5 | InputManager.Update 89.3 |
| modded-B | #34767 | 24.23 | 96 | 58.8 | JobScheduler.LateUpdate 34.0 |
| modded-B | #34246 | 16.07 | 64 | 47.9 | (frame gordo difuso; top SyncTransforms 2.0) |
| modded-B | #33495 | 3.94 | 15 | 47.7 | NonWavesSpawnScenario.Update 25.6 |
| modded-B | #34757 | 23.96 | 95 | 46.3 | NonWavesSpawnScenario.Update 25.3 |
| modded-B | #34122 | 13.94 | 55 | 42.0 | NonWavesSpawnScenario.Update 23.8 |

Vanilla >40ms (7 no total das duas): 51.2/50.0/49.4ms (vanilla-A b4/b60/b64) e 53.4/48.4/40.3ms (vanilla-B b97/b96/b46) — os dois últimos são a rajada de spawn/streaming.

## 7. One-offs gigantes (não periódicos)

**`Diz.Utils.AsyncWorker`** (decompile `Diz.Utils/AsyncWorker.cs`): bomba main-thread de tasks de background — `Update`/`FixedUpdate` chamam `CheckForFinishedTasks()`, que executa no main thread as continuations de tasks concluídas (`RunOnBackgroundThread`/`RunInMainTread`).

- modded-A: 1 call de **195.1ms** (FixedUpdate, t=28.79s, frame 211.5ms — o pior da investigação) e 1 call de **105.5ms** (Update, t=25.02s, 0.9s após o 3º tick de spawn). self=incl, calls=1 → uma única continuation opaca engoliu o frame.
- modded-B: total 3.3ms em 30s (nada). vanilla: ~1ms total. **Hitch exclusivo do modded-A**, não periódico; proximidade temporal com o 3º tick de spawn sugere continuation do pipeline assíncrono de bot/asset, mas sem prova no dado (UpdateOnly não vê o delegate).

**`EFT.InputSystem.InputManager.Update`**: 1 spike único por captura, sempre no início — vanilla-A 31.8ms (t=1.25s), modded-A 79.9ms (t=4.55s), modded-B 89.3ms (t=2.56s); vanilla-B sem spike (max 2.75ms). `ManualDeepCapture=False` nos 3 (não é o hotkey de deep-capture do profiler). Padrão de dispatch de evento de input que dispara handler caro (abertura de tela/ação). Existe no vanilla; no modded é 2.5–2.8× maior.

## 8. Comportamento espinhoso que é NORMAL do jogo (visto no vanilla)

- **Rajada de spawn/streaming**: vanilla-A b60–71 (JobScheduler 84ms + SyncTransformsClass com buckets de até 42.2ms + AICoreAgentClass até 36.1ms — bots novos entrando, degrau 25→26); vanilla-B b46–55 e b94–99 (idem, degrau 32→35). Espinhas dessa família não são, por si, evidência de mod.
- **`GClass890.Update`** (alias41 "SuperBetterAudioQueue" — fila de áudio agendada em par de BetterSource; rótulo comunitário, não verificado): picos de 2–6ms nos dois ambientes, e o MAIOR single-call é do vanilla-B (10.95ms, b67). Modded tem 2.2× mais calls (2760–2832 vs 1204–1292 em 30s) com custo/call igual (8–16µs) — mais eventos de áudio, custo total pequeno (23–45ms/30s). Não é fonte de hitch relevante.
- **`Fika.Core.Main.Components.BotStateManager.Update`**: suave nas 4 capturas (mediana 3.2–4.2ms/bucket, max 6.1–8.1ms), escala com nº de bots, **sem picos** — descartado como fonte de spike.
- **`Diz.Jobs.JobScheduler`**: as rajadas em si são mecanismo vanilla (fila com budget); o problema modded é a frequência (3×/30s) e o tamanho.
- **`EFT.GameWorldUnityTickListener`** (decompile: trampoline `GameWorld.DoWorldTick`): elevado no 1º tick modded-A (b16, self 23.95ms no bucket, frame #26869 com 16.0ms) — registro de mundo pós-spawn; nos ticks seguintes não sobe. Secundário.

## 9. Limitações

- Modo UpdateOnly no modded: os 20–26ms do tick são um agregado opaco (método + callees + patches). A atribuição a mod específico exige recaptura em ModAttribution.
- 30s por captura = só 3 ticks do metrônomo por captura; período estimado com 2 gaps por captura (4 no total, todos 10.002–10.016s).
- Population proxies diferentes entre ambientes (AICoreLayerClass vs Player.LateUpdate) — comparo tendência, não nível.
- b18 modded-A (InputManager 79.9ms) cai 0.45s após o 1º tick; tratado como one-off independente (mesmo padrão existe no vanilla-A sem tick).

## 10. Próximos passos de investigação (sem fix)

1. **Recapturar modded com Mode=ModAttribution** (30–60s): preenche `harmony-patches.csv` e atribui os 20–26ms do tick e as continuations do AsyncWorker a plugin/patch específico.
2. **Auditar patches da cadeia de spawn** nos mods presentes só no modded (`TRL-DynamicSpawn`, `MoreBotsAPI`, `SAIN`, Fika) sobre `NonWavesSpawnScenario`, `BotsController.ActivateBotsWithoutWave`, `BotSpawner.*`, `SpawnPoint.*` — via grafos do repo (`graph-code-navigation`).
3. **Conferir configs de location servidas pelo server TRL** (BotMax, BotSpawnPeriodCheck, BotSpawnTimeOn/Off, NewSpawn, NonWaveGroupScenario) vs vanilla — explica por que o branch caro (`num>0`) roda a cada 10s no modded e quase nunca no vanilla.
4. **Instrumentação leve num tick**: logar `BotMax`, `AliveLoadingDelayedBotsCount`, `AliveAndLoadingBotsCount` e o `num` efetivo por tick → prova se spawns são pedidos e falham (custo sem efeito, §5).
5. **Captura longa (2–3min)** para medir se o custo por tick cresce com o tempo de raid (ligação com RAM 10→33GB da investigação anterior) e se as rajadas de JobScheduler se alongam.

## Histórico

| Data | Autor | Descrição |
|---|---|---|
| 2026-08-22 | Guilherme | docs(perf): add DynamicSpawn audit report + ICM/Stances optimization handoffs |
