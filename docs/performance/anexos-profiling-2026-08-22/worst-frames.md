---
title: "Anexo profiling 2026-08-22 — worst-frames"
date: 2026-08-22
status: 🟢 Vivo
authors: Claude (investigação de performance 2026-08-22, análise dimensional)
---

# Anatomia dos piores frames (worst-frames/*.json) — vanilla × modded

> Dimensão: **worst-frames** · Investigação de profiling SPT vs SPT_2 · 2026-08-22
> Dados: `worst-frames/frame-N.json` + `frames.csv` + `methods.csv` + `timeline.csv` das 4 capturas (ver CONTEXT.md).
> Toda contagem/soma calculada via Python 3.14 sobre os arquivos brutos; nenhum número estimado.

---

## 1. Contagem de deep frames e política de retenção

Threshold do deep capture: `FrameMs > 25` **OU** `ManagedProfiledMs > 10`. Config `WorstFrameCount=20`.

| Captura | Frames | FrameMs>25 | managed>10 | Cruzaram (OR) | DeepRetained=true | Arquivos em disco |
|---|---:|---:|---:|---:|---:|---:|
| vanilla-A | 2082 | 13 (0.6%) | 75 (3.6%) | 75 (3.6%) | 75 | 75 |
| vanilla-B | 1766 | 13 (0.7%) | 33 (1.9%) | 35 (2.0%) | 38 | 38 |
| modded-A | 1507 | **114 (7.6%)** | 68 (4.5%) | **147 (9.8%)** | 100 | **100** |
| modded-B | 1767 | 41 (2.3%) | 59 (3.3%) | 71 (4.0%) | 71 | 71 |

Fatos derivados:

- **O cap efetivo de retenção em disco é 100, não 20.** modded-A saturou: 147 frames cruzaram o threshold, 100 retidos. Os 100 retidos são **exatamente o top-100 por FrameMs** entre os que cruzaram (overlap 100/100; por managed seria 65/100). Os 47 descartados são os mais leves (FrameMs 21.0–25.1ms, mediana 23.7). `WorstFrameCount=20` do config não corresponde ao comportamento observado.
- vanilla-B tem 3 frames retidos que não cruzam o threshold recalculado (23.86/24.81/23.86ms de FrameMs, managed 7.5–8.7ms) — borda de arredondamento/ordem de avaliação no runtime do profiler; irrelevante quantitativamente.
- **A contagem de arquivos NÃO é comparável entre ambientes**: o gatilho `managed>10` dispara muito mais fácil no vanilla (ModAttribution instrumenta Player.LateUpdate, patches, métodos de plugin → managed maior por construção). 62 dos 75 deep frames de vanilla-A têm FrameMs ≤ 25 — são "frames com managed alto", não hitches. **O indicador honesto cross-env é FrameMs>25: vanilla 0.6–0.7% × modded 7.6% (A, 11–12x) / 2.3% (B, 3x).**

## 2. Duas classes de worst frames

Classificando cada worst frame por "existe algum método com self > 10ms?":

| Captura | Spike-class (1 método >10ms) | Chronic-class (sem dominador) | Mediana da fração managed (main, self) |
|---|---:|---:|---:|
| vanilla-A | 3 | 72 | 52% |
| vanilla-B | 1 | 37 | 49% |
| modded-A | **8** | 92 | **34%** |
| modded-B | **6** | 65 | **44%** |

- **Chronic-class modded**: frames de 25–33ms, topo típico `SyncTransformsClass.Update` 2.4–5.1ms + `GameWorldUnityTickListener.Update` ~1ms; managed instrumentado soma só 8–14ms → **mais da metade do frame é invisível ao modo UpdateOnly** (render/física/nativo + Player.LateUpdate não instrumentado + patches Harmony dos ~100 mods). GAP de instrumentação relevante.
- **Spike-class modded**: todos os frames >38ms têm UM dono claro (tabela §3).

### Spike-class completa (método dominante, self ms)

**modded-A** (8): 211.55← AsyncWorker.FixedUpdate 195.11 · 126.46← AsyncWorker.Update 105.46 · 104.86← InputManager.Update 79.38 · 52.92← JobScheduler.LateUpdate 34.25 · 45.53/44.37/38.39← NonWavesSpawnScenario.Update 20.8/20.2/20.9 · 45.23← GameWorldUnityTickListener.Update 15.96
**modded-B** (6): 110.53← InputManager.Update 89.33 · 58.78← JobScheduler.LateUpdate 33.96 · 47.67/46.26/41.99← NonWavesSpawnScenario.Update 25.6/25.3/23.8 · 33.87← JobScheduler.LateUpdate 14.52
**vanilla-A** (3): 51.21← SyncTransformsClass.Update 17.56 · 49.36← InputManager.Update 30.33 · 33.36← FikaPlayer.ApplyDamageInfo 10.21
**vanilla-B** (1): 34.52← GClass890.Update (fila de áudio, alias 4.1 "SuperBetterAudioQueue") 10.95

## 3. Dominadores de self agregados across worst frames

Self somado nos worst frames (main thread), self/WF = média por worst frame, baseC/F e baseS/F = média da captura inteira (methods.csv).

### vanilla-A (75 WF; ΣFrameMs=1778ms; Σself-managed=990ms = 55.7%)

| método | presente | Σself ms | self/WF | calls/WF | base c/f | base self/f |
|---|---:|---:|---:|---:|---:|---:|
| Player.LateUpdate | 75 | 160.7 | 2.143 | 26.0 | 25.45 | 1.7415 |
| AICoreAgentClass.Update | 75 | 144.2 | 1.922 | 24.9 | 24.42 | 0.2382 |
| SyncTransformsClass.Update | 75 | 132.2 | 1.762 | 1.0 | 1.00 | 0.1290 |
| JobScheduler.LateUpdate | 75 | 59.3 | 0.790 | 1.0 | 1.00 | 0.0411 |
| InputManager.Update | 75 | 39.3 | 0.523 | 1.0 | 1.00 | 0.1190 |
| AICoreLayerClass.Update | 75 | 35.5 | 0.474 | 24.9 | 24.42 | 0.1545 |
| FikaPlayer.ManualUpdate | 75 | 33.9 | 0.452 | 26.0 | 25.45 | 0.3212 |

### vanilla-B (38 WF; ΣFrameMs=1015ms; Σself=533ms = 52.5%)

Topo: Player.LateUpdate 70.5ms (1.854/WF) · JobScheduler.LateUpdate 66.0 (1.736/WF, 34x o base) · SyncTransforms 44.4 (1.167/WF) · `<CreateBot>d__8.MoveNext` 17.4ms em **3 frames** + **LZ4HC** (LL64.LZ4HC_InsertAndGetWiderMatch 10.0ms/2699 calls + LL.LZ4HC_hashPtr 9.6ms/11758 calls + LL.LZ4HC_Insert 6.7ms, nos mesmos 3 frames) + BotFirearmController.Create 6.8ms → **assinatura de spawn de bot com descompressão de bundle no vanilla**.

### modded-A (100 WF; ΣFrameMs=3188ms; Σself=1426ms = 44.7%)

| método | presente | Σself ms | self/WF | calls/WF | base c/f | base self/f | ratio self |
|---|---:|---:|---:|---:|---:|---:|---:|
| SyncTransformsClass.Update | 100 | 276.4 | 2.764 | 1.0 | 1.00 | 2.0509 | 1.3x |
| AsyncWorker.FixedUpdate | 100 | 196.5 | 1.965 | 1.7 | 1.19 | 0.1311 | 15.0x |
| GameWorldUnityTickListener.Update | 100 | 129.2 | 1.292 | 1.0 | 1.00 | 0.8191 | 1.6x |
| AsyncWorker.Update | 100 | 105.5 | 1.055 | 1.0 | 1.00 | 0.0720 | 14.7x |
| InputManager.Update | 100 | 88.4 | 0.884 | 1.0 | 1.00 | 0.1538 | 5.7x |
| JobScheduler.LateUpdate | 100 | 73.7 | 0.737 | 1.0 | 1.00 | 0.2046 | 3.6x |
| NonWavesSpawnScenario.Update | 100 | 61.9 | 0.619 | 1.0 | 1.00 | 0.0417 | 14.9x |
| BotStateManager.Update (Fika) | 100 | 51.3 | 0.513 | 1.0 | 1.00 | 0.3565 | 1.4x |
| EventSystem.Update (uGUI) | 62 | 29.6 | 0.296 | 0.6 | 0.12 | 0.0564 | 5.2x |

Obs.: AsyncWorker.FixedUpdate/Update e NonWaves concentram quase todo o Σself em poucos frames (§4-§5); os ratios 15x refletem isso.

### modded-B (71 WF; ΣFrameMs=1995ms; Σself=934ms = 46.8%)

Topo: **JobScheduler.LateUpdate 274.8ms (3.871/WF, 17.4x o base)** · SyncTransforms 142.7 (2.010/WF, 1.3x) · InputManager 99.2 (1.398/WF, 8.9x) · NonWavesSpawnScenario 74.8 (1.054/WF, 24.3x) · GameWorldUnityTickListener 70.6 (0.994/WF, 2.0x).

### Comparação de composição vanilla × modded

- **Vanilla**: worst frames = custo por-bot legítimo (Player.LateUpdate + AICore com 25–33 bots) + evento raro de spawn (CreateBot + LZ4 de bundle). Nenhum método passa de 17.6ms self na pior ocorrência (fora InputManager 30.3ms 1x).
- **Modded**: worst frames = **drenos de fila** (AsyncWorker/JobScheduler/InputManager/NonWaves — nenhum deles relevante no vanilla) sobre uma base crônica 2–3x mais cara (SyncTransforms 2.0–2.8ms/WF vs 1.2–1.8 no vanilla; GameWorldUnityTickListener ~1ms/WF) e com metade do frame não-atribuída.

## 4. Decomposição dos hitches extremos

### modded-A frame 28154 — 211.55ms (t=28.8s), `frame-28154.json`

| componente | valor |
|---|---|
| FrameMs | 211.548 |
| managed (header) | 200.12ms (94.6%) |
| Σself main thread | 200.10ms |
| Σself outras threads | 0.01ms |
| não-atribuído | **11.45ms (5.4%)** |
| **AsyncWorker.FixedUpdate** | **self = incl = 195.114ms, callCount = 1** → **92.2% do frame num único drain** |
| 2º maior self | SyncTransformsClass.Update 1.422ms |
| 3º | GameWorldUnityTickListener.Update 0.536ms (incl 0.753) |

Todo o resto do frame soma ~3.5ms e é a carga normal (GPUInstancer 0.274, BotStateManager 0.271, decal 0.215, SAIN EnemyPartDataClass 245 calls/0.055ms, Visceral DismemberedLimbScaler 161 calls/0.033ms...).

**Edges/roots**: `AsyncWorker.FixedUpdate` aparece nos roots com `[no managed caller observed]` (é superfície MonoBehaviour chamada pela engine) e **não tem NENHUM edge de saída** — self==inclusive porque, no modo UpdateOnly, nada do que ele invoca é instrumentado. Os 94 edges do frame são triviais (GameWorldUnityTickListener→SAIN/PlayerPhysical/EnemyParts, BotStateManager→AICoreLayer, EFTPhysicsClass→SyncTransforms 1.42ms). Ou seja: o JSON **prova onde** o tempo foi gasto (dentro do drain) e **não pode provar o quê** foi drenado — limite do modo.

### modded-A frame 27935 — 126.46ms (t=25.0s): AsyncWorker.**Update** 105.46ms self, mesmo padrão.
### modded-A frame 26883 — 104.86ms (t=4.6s): InputManager.Update 79.38ms self (§4b).

### modded-B frame 33426 — 110.53ms (t=2.6s), `frame-33426.json`

| componente | valor |
|---|---|
| FrameMs | 110.535 |
| managed (header) | 95.40ms (86.3%) |
| não-atribuído | **15.14ms (13.7%)** |
| **InputManager.Update** | **self 89.329 / incl 89.334ms — 80.8% do frame** |
| 2º | SyncTransforms 1.607 · 3º GameWorldUnityTickListener 0.635 (incl 0.878) |

Edges de InputManager.Update no frame: GClass2404.Update (3 calls, 0.001ms), Class1863/1865.Update, GClass2407.Update — tudo ≤0.002ms. O custo é interno/não instrumentado.

**Leitura pelo decompile** (`EFT.InputSystem.InputManager`): `Update()` faz `inputBindingsDataClass.UpdateInput(...)` (polling de teclas → lista de `ECommand`) e, com `DeliverInputOnUpdates`, `method_2()→method_3()→TranslateDelegate(commands, ...)` — **despacha os comandos pela cadeia de InputNode (GamePlayerOwner→Player→UI)**. Um hitch de 89ms aqui = algum consumidor de comando executou trabalho síncrono pesado em resposta a uma tecla naquele frame. O vanilla exibe o mesmo fenômeno menor (vanilla-A: 30.33ms em t=1.2s; vanilla-B max 3.85ms) — o modded amplia ~3x (79–89ms), presumivelmente por handlers/patches de mods na cadeia de input. Os dados não identificam a tecla/comando.

## 5. O metrônomo de spawn e a cascata

Série temporal (buckets de 250ms, self na main thread):

| método | vanilla-A | vanilla-B | modded-A | modded-B |
|---|---|---|---|---|
| NonWavesSpawnScenario.Update (Σ 30s) | 0.7ms | 2.6ms | 62.8ms | 76.5ms |
| — picos | nenhum | nenhum | **t=4.0(20ms), 14.0(21), 24.0(21) — gaps 10.00s exatos** | **t=3.8(26), 13.8(24), 23.8(25) — gaps 10.00s** |
| JobScheduler.LateUpdate picos | t=16.2–17.2s (24/15/19/14ms) | t=24.0–24.2 (42/33) | t=5.5(35), 15.5(27), 24.2(69) | t=4.2–4.8(35+44), 14.0–14.2(58+42), 24.0–24.5(76+37+41) |
| AsyncWorker picos | — | — | **FixedUpdate t=28.8 (195ms); Update t=25.0 (105ms)** | — (Σ30s: 1.5+1.8ms) |
| InputManager pico | t=1.2s (32ms) | — | t=4.5s (80ms) | t=2.5s (90ms) |

- `EFT.NonWavesSpawnScenario.Update` (decompile): gate temporal `PastTime - float_0 < float_2` com `const float float_1 = 10f` e `float_2 = max(BotSpawnPeriodCheck, 10f)` → **checagem de spawn no máximo a cada 10s — o metrônomo é hardcoded do EFT**. Quando abre e há déficit (`num = BotMax − AliveLoadingDelayedBotsCount > 0`): `GClass1876.TrySpawn(...)` + loop `botsController.ActivateBotsWithoutWave(1, data)` **síncrono no Update** → os 20–26ms de self por tick.
- No vanilla o MESMO componente roda e custa ~0 (0.7–2.6ms em 30s): população no cap (~25–32 vivos) → early-return no déficit. **No modded (8–14 bots vivos) o déficit nunca fecha → todo tick de 10s ativa spawns.** A causa do déficit permanente (despawn do TRL-DynamicSpawn? BotMax elevado? bots morrendo?) fica para a dimensão de spawn/população.
- `Diz.Jobs.JobScheduler` (decompile): agendador cooperativo de continuations com orçamento por frame (FrameTicks=16ms default), **mas** com válvula de escape — após `SlowFrames` (6) drains consecutivos acima do orçamento, `Boolean_0` retorna true incondicionalmente e ele executa backlog mesmo estourado (`int_1 > SlowFrames` → executa e rebaixa `int_1 = SlowFrames/2`). Sob a rajada de jobs de um spawn wave isso produz os LateUpdate de 14–34ms e buckets de 27–76ms, **co-temporizados com o metrônomo** nos dois modded.
- Cascata em modded-A: tick de spawn t=24 → JobScheduler 69ms bucket t=24.2 → AsyncWorker.Update 105ms t=25.0 → AsyncWorker.FixedUpdate 195ms t=28.8 (~4.8s depois do tick — janela compatível com round-trip ao servidor SPT para gerar perfis + montagem). Correlação temporal, não prova de causa.

## 6. AsyncWorker (Diz.Utils) — o que ele drena

Decompile (`Diz.Utils/AsyncWorker.cs` + `GClass1516.cs`, alias 4.1 "Diz.Utils.TaskWorker"):

- `AsyncWorker` é um MonoBehaviour singleton; `Start()` cria **2 threads de fundo** ("AWorker Thread 1/2") que executam tarefas de `Queue_0` (alimentada por `AsyncWorker.RunOnBackgroundThread(...)`).
- Cada tarefa devolve uma **continuation `Action`** que entra em `Queue_1`; `AsyncWorker.RunInMainTread(action)` chamado de thread de fundo também enfileira em `Queue_1`.
- `Update()` **e** `FixedUpdate()` chamam `GClass1516.CheckForFinishedTasks()`: **`while(true) { dequeue; action(); }` até esvaziar — sem orçamento de tempo nenhum.**
- As continuations fazem `TaskCompletionSource.SetResult(...)` **sem** `RunContinuationsAsynchronously` → **as máquinas de estado async que aguardavam (`await RunOnBackgroundThread(...)`) continuam INLINE dentro do drain**. Um drain de 195ms = cadeia(s) de continuação inteiras executando (parse → criação → montagem...), não "só um callback".
- **Quem alimenta a fila no EFT** (grep no decompile, 16 arquivos): `DataHandlerClass` (alias "HTTPTransportManager" — toda resposta HTTP do backend: parse JSON off-thread + continuation main-thread), `GClass654/WsWebSender` (WebSocket), `Class304` ("Backend"), `GClass1812` ("ItemFactoryCreateOperation"), `GClass3470` ("WeaponAssembler"), `Corpse`, `ObservedPlayerControllerClass`, `ClientPlayer`, `ClientWorld`, `NetworkGameSession`, `BaseInventoryOperationClass`, `TraderAssortmentControllerClass`, `MatchmakerPlayerControllerClass`, `GClass2656` ("AirdropDataReceiver"), `VOIPBanDataClass`. Ou seja: **respostas do servidor SPT (perfis de bot, inventários), fabricação de itens/armas e operações de inventário** — exatamente o pipeline de criação de bot pós-spawn-wave. Mods também podem usar (`AsyncWorker.RunInMainTread` é público) — não verificado nos binários dos mods.
- modded-B não teve drain grande (Σ 3.3ms em 30s) → o evento é **episódico**, dependente de quando o pipeline de spawn/backend descarrega — não é custo contínuo.

## 7. Census de calls nos worst frames (ENT estável durante hitches)

calls/WF (média nos worst frames) × base c/f (captura inteira):

| método (proxy de instâncias) | modded-A WF | modded-A base | modded-B WF | modded-B base |
|---|---:|---:|---:|---:|
| DismemberedLimbScaler.Update (Visceral) | 161.0 | 161.00 | 184.4 | 185.85 |
| DismemberedLimbScaler.LateUpdate | 161.0 | 161.00 | 184.6 | 185.85 |
| EnemyPartDataClass.Update (SAIN) | 282.4 | 257.93 | 87.7 | 86.06 |
| PlayerDataExtensions.Update (SAIN) | 57.1 | 54.01 | 38.3 | 37.18 |
| AICoreLayerClass.Update | 13.3 | 12.50 | 8.6 | 8.30 |
| self dessas 5 linhas somado/WF | ~0.35ms | ~0.29ms | ~0.25ms | ~0.20ms |

**Os worst frames NÃO têm mais instâncias rodando** — Visceral idêntico ao baseline, SAIN +2–9%. A carga por-entidade (161–186 limbs Visceral, 87–282 partes SAIN) é um **piso crônico** (~0.2–0.35ms/frame de self direto) presente em todo frame, não o gatilho dos hitches. (Nota: `PlayerDataExtensions.Update` resolve para 4 MethodIds distintos de SAIN — sobrecargas — agregadas aqui por nome.)

## 8. Distribuição temporal dos worst frames (bins de 2s)

- **modded-A**: 76/100 em t=0–6s (frente de spawn t≈4 + início da captura), 11 em t=14–16s, 7 em t=24–29s. Os 24 frames de t=0–2s são chronic-class (25–31ms, SyncTransforms no topo, managed 7–14ms) — **possível contaminação de warm-up do profiler/captura recém-iniciada** (Artefato-possível), mas consistentes com o padrão crônico do resto.
- **modded-B**: 53/71 dentro das janelas do metrônomo (t≈4, 14, 24s).
- **vanilla-A**: 63/75 em t=16–22s (evento único de spawn — JobScheduler picos t=16.2–17.2); **vanilla-B**: 13/38 em t=24s (CreateBot+LZ4).
- Conclusão: **spawn de bots gera os worst frames nos DOIS ambientes**; a diferença do modded é (a) frequência — metrônomo de 10s sempre ativo por déficit permanente vs 1 evento por captura no vanilla — e (b) magnitude — 104–211ms vs máx 53.4ms.

## 9. Limites de evidência

- Modo UpdateOnly: self dos drenos (AsyncWorker, InputManager, JobScheduler, NonWaves) **inclui** todos os callees não instrumentados — o conteúdo real do trabalho é invisível; harmony-patches.csv vazio.
- Populações de bots diferentes (~2x mais no vanilla) tornam os drenos modded ainda mais anômalos (menos bots, hitches maiores), mas impedem comparação fina de custos por-bot.
- Frames chronic-class do modded têm >50% do tempo não atribuído — pode ser render/física/GC **ou** patches Harmony dos mods; indistinguível nesta captura.
- Atribuição spawn→AsyncWorker/JobScheduler é correlação temporal (co-timing 10s + janela pós-tick), não cadeia de chamadas comprovada.

## 10. Próximos passos de medição

1. **Recapturar o modded em ModAttribution** (mesmo raid/Customs): abre o conteúdo dos drenos (o que roda dentro de CheckForFinishedTasks e da cadeia do InputManager) e popula harmony-patches.csv. É o passo que converte 3 "Suspeitas" em veredito.
2. Instrumentação dirigida barata: patch temporário em `GClass1516.CheckForFinishedTasks` logando `Queue_1.Count` e o tipo do delegate quando o drain passa de ~5ms (identifica o alimentador do hitch de 195ms).
3. Logar `JobScheduler.QueueLength` por frame nas janelas do metrônomo — dimensiona o backlog que a válvula de escape força.
4. Na dimensão spawn/população: explicar o déficit permanente (`BotMax − AliveLoadingDelayedBotsCount > 0` a cada tick de 10s) — config de BotMax vs despawn do TRL-DynamicSpawn.
5. Repetir captura modded sem interação de teclado nos primeiros 10s para isolar o hitch de InputManager (se sumir, é reação a comando do usuário; identificar o handler via ModAttribution).

## Anexo — números de referência dos 4 piores frames modded

| frame | captura | t | FrameMs | managed | não-atrib. | dominador (self) | % do frame |
|---|---|---:|---:|---:|---:|---|---:|
| 28154 | modded-A | 28.8s | 211.55 | 200.12 (94.6%) | 11.45 | AsyncWorker.FixedUpdate 195.11 | 92.2% |
| 27935 | modded-A | 25.0s | 126.46 | — | — | AsyncWorker.Update 105.46 | 83.4% |
| 33426 | modded-B | 2.6s | 110.53 | 95.40 (86.3%) | 15.14 | InputManager.Update 89.33 | 80.8% |
| 26883 | modded-A | 4.6s | 104.86 | — | — | InputManager.Update 79.38 | 75.7% |

## Histórico

| Data | Autor | Descrição |
|---|---|---|
| 2026-08-22 | Guilherme | docs(perf): add DynamicSpawn audit report + ICM/Stances optimization handoffs |
