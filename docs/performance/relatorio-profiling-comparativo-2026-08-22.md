---
title: "Relatório — Profiling comparativo TRL (SPT) × Vanilla (SPT_2), Customs, 2026-08-22"
date: 2026-08-22
status: 🟢 Vivo
authors: Claude (investigação de performance 2026-08-22, rodada de profiling in-game)
---

# Relatório — Profiling comparativo TRL (SPT) × Vanilla (SPT_2)

Análise comparativa das capturas do **SPT Runtime Profiler 0.1.1** (plugin BepInEx que instrumenta métodos managed no Mono e exporta custo por método/frame/bucket) entre o ambiente **Tarkov Red Line** (`D:\SPT`, ~107 entradas de plugin) e o ambiente **baseline** (`D:\SPT_2`, EFT+SPT 4.0.13+Fika 2.3.8+utilitários). Todas as capturas: raid em **Customs**, ~30 s cada, 2026-08-22 à noite.

Método: 6 análises dimensionais paralelas sobre os dados brutos (CSVs + worst-frames JSON + decompile do EFT + fonte dos mods no repo), seguidas de **verificação adversarial** dos 8 achados principais — cada um re-derivado dos arquivos originais por um verificador independente instruído a refutá-lo. Resultado: 5 CONFIRMADOS, 3 PARCIAIS (claim corrigida), 0 refutados. Detalhes por dimensão nos [anexos](./anexos-profiling-2026-08-22/).

**Escopo desta etapa: diagnóstico.** Nenhuma correção é proposta aqui além de próximos passos de investigação/medição.

---

## 1. O que os arquivos de profiling fornecem

Cada captura (pasta `BepInEx\profiling\<timestamp>`) contém:

| Arquivo | Conteúdo | Uso nesta análise |
|---|---|---|
| `frames.csv` | 1 linha por frame: duração total (`FrameMs`), tempo managed instrumentado (`ManagedProfiledMs`), método de maior custo próprio do frame | distribuições, cauda, gap managed×não-atribuído |
| `methods.csv` | por método×thread: chamadas, custo próprio (self) e inclusivo, percentis, máximos | ranking de ofensores, censo de instâncias (calls/frame de um `Update` de MonoBehaviour = nº de instâncias vivas) |
| `edges.csv` | pares chamador→chamado com custo | cadeias de chamada |
| `mod-summary.csv` | agregado por plugin BepInEx | atribuição por mod |
| `timeline.csv` | custo por método em **buckets de 250 ms** | séries temporais, periodicidade, crescimento de instâncias |
| `harmony-patches.csv` | inventário + timing de patches Harmony | **só populado no vanilla** (ver §6) |
| `worst-frames/*.json` | dump profundo dos piores frames (métodos, edges, roots) | anatomia dos hitches |
| `capture.json` | consolidado + diagnósticos (entradas descartadas etc.) | validação de sanidade |

**Duas ressalvas estruturais descobertas antes de qualquer análise:**

1. **Capturas duplicadas** — no SPT, `205433`, `205500` e `205508` são **byte-idênticas** (mesmo MD5). Das "3 últimas" solicitadas, existem na prática **2 datasets distintos**: `205500` (**modded-A**) e `205604` (**modded-B**). No SPT_2: `203734` (**vanilla-A**) e `203938` (**vanilla-B**).
2. **Modo de captura assimétrico** — o vanilla capturou em `ModAttribution` (todos os métodos de plugins + timing de patches Harmony) e o modded em `UpdateOnly` (só superfícies `Update`/`LateUpdate`/`FixedUpdate` de MonoBehaviours, o modo mais leve). Consequência: no modded, **o custo de patches Harmony e de qualquer código de mod fora de um Update é invisível** — ou aparece diluído no custo de métodos do jogo, ou cai fora do managed medido. Toda a análise abaixo desconta esse viés e ele domina a seção de lacunas (§6).

Contexto de população (proxy medido nos dados): o vanilla rodou com **~25–33 players/bots vivos** e o modded com **~24 Players instanciados mas só ~8–14 cérebros de IA ativos**. Ou seja: **o baseline carregava 2–3× mais IA ativa que o ambiente modded** — qualquer piora do modded é *apesar* de menos bots, o que torna os deltas abaixo conservadores.

## 2. Comparação geral SPT_2 × SPT

### 2.1 Frametime (duração de cada quadro; 16,7 ms = 60 FPS)

| Captura | frames | avg | p50 | p95 | p99 | máx | % do TEMPO em frames >25 ms |
|---|---|---|---|---|---|---|---|
| vanilla-A | 2082 | 14,41 | 13,92 | 19,77 | 23,66 | 51,2 | 1,4% |
| vanilla-B | 1766 | 16,99 | 16,82 | 20,90 | 23,81 | 53,4 | 1,5% |
| modded-A | 1507 | **19,92** | 18,92 | **25,76** | **31,74** | **211,5** | **11,8%** |
| modded-B | 1767 | 16,97 | 16,60 | 22,89 | 26,68 | **110,5** | **4,3%** |

**A piora do modded vive na cauda, não na média.** modded-B empata com vanilla-B na média (16,97×16,99) — mas com 1/3 da IA ativa; essa "igualdade" já é a medida do custo do stack. A régua de ruído natural entre runs do mesmo ambiente é ±2,6–2,9 ms na média — só a cauda (p95/p99/máx) separa os ambientes **sem sobreposição**: o modded passa **3–8× mais tempo de parede** acima de 25 ms, e frames acima de 100 ms **só existem no modded** (teto vanilla: 53,4 ms).

### 2.2 Onde vive a piora (decomposição do excesso médio, referência = média vanilla)

| | modded-A | modded-B |
|---|---|---|
| Δ frametime médio | +4,22 ms/f | +1,27 ms/f |
| … managed instrumentado | +1,15 | −0,38 |
| … gap não-atribuído | +3,07 | +1,65 |
| …… código comum não-instrumentado no modded (projeção, dominada por `Player.LateUpdate`, escalada pela população) | ~0,8–1,1 | ~0,6–0,8 |
| …… **residual: custo nativo extra OU managed invisível dos mods** | **~+2,0 a +2,3** | **~+0,9 a +1,1** |

A cauda tem **duas naturezas distintas**: a banda 25–33 ms (103 frames no modded-A = 9,2% do tempo) é "gap-pesada" — mais da metade do frame invisível ao profiler — enquanto os hitches >50 ms são 80–89% managed e **totalmente identificados** (AsyncWorker, InputManager, JobScheduler — ver §3).

### 2.3 Sistemas do jogo, lado a lado (métodos instrumentados nos dois modos)

Os 12+ sistemas neutros (culling, sombras, flare, decal system, `ComponentSystem`, IA por chamada, IK por chamada) têm custo **idêntico ou menor no modded** — isso calibra a comparação: os deltas abaixo não são artefato de modo. Ranking dos deltas reais (ms/frame, self, par A−A / B−B):

| # | Sistema | Δ(A) | Δ(B) | Natureza |
|---|---|---|---|---|
| 1 | **`SyncTransformsClass.Update`** (o passo de simulação de física do EFT) | **+1,92** | **+1,32** | contínuo, 100% dos frames |
| 2 | `DeferredDecalRenderer.Update` (renderizador de decais — manchas projetadas em superfícies, ex.: sangue) | +0,22 | +0,20 | contínuo, 7–15× o vanilla |
| 3 | `JobScheduler.LateUpdate` (fila de tarefas fatiadas do jogo, orçamento de 16 ms/frame) | +0,16 | +0,17 | rajadas de 91–160 ms sincronizadas com o metrônomo de spawn |
| 4 | `AsyncWorker` (bomba que drena continuações de tarefas async na main thread) | +0,20 | +0,00 | **2 hitches únicos** de 105 e 195 ms, só no modded-A |
| 5 | `InputManager.Update` (despacho de comandos de tecla) | +0,04 | +0,04 | média neutra; hitches de 79–89 ms (vanilla: máx 31,8) |
| 6 | `GameWorldUnityTickListener.Update` (tick por player do mundo), inclusive **por player** | 2,1–3,2× | 2,1–3,2× | contínuo; total mascarado por menos bots |

## 3. Principais ofensores (ordenados pela relevância observada)

### 🥇 O1 — Física simulando 100% do tempo (`SyncTransformsClass.Update`) — CONFIRMADO

O maior delta contínuo medido: **2,05 / 1,59 ms por frame no modded vs 0,13 / 0,27 no vanilla** (+1,92/+1,32 ms/f), método de maior custo em **~94% dos frames** do modded. O decompile prova que essa classe executa o **passo de simulação PhysX por script** do EFT (`Physics.Simulate` em modo `SmoothSimulate`), gateado por um flag `UpdateEnabled` que liga quando existe rigidbody (corpo físico) registrado, ativo e visível.

O mecanismo do delta **não é passo mais caro — é duty cycle** (fração do tempo em que o sistema fica ligado): o vanilla simula em só **7,4–14% dos buckets de 250 ms** — rajadas de ~2 s após evento de ragdoll/granada, custando 2,2–3,8 ms/f *durante a rajada*, e volta a ~0,0005 ms/f — enquanto o modded simula em **100% dos buckets, do primeiro ao último**, sem nunca desligar, com custo por passo igual ou *menor* que a rajada vanilla. População de bots foi refutada como causa (vanilla tinha 2–3× mais bots e sync ~0 fora de rajada).

### 🥈 O2 — Metrônomo de spawn de 10 s + rajadas do JobScheduler — CONFIRMADO (mecanismo) / PARCIAL (quantificação)

`NonWavesSpawnScenario.Update` (o cenário de spawn contínuo do EFT) tem cadência **hard-coded vanilla de 10 s** — a anomalia do modded é o **custo por tick e a recorrência**:

- Ticks de **20,2–25,6 ms** a cada **10,00 s cravados** nas duas capturas modded (gaps medidos: 10,002–10,016 s), cada um gerando frame de 38–48 ms. No vanilla: 0,65–2,6 ms *totais* em 30 s — um tick vanilla **que efetivamente ativa bots** custou 1,85 ms, ~11–14× menos.
- Cada tick dispara nos ~2 s seguintes uma **rajada do `JobScheduler`** de 91–160 ms (98,6–98,8% de todo o custo do JobScheduler no modded está nessas janelas). O vanilla tem 1 rajada dessas por 30 s, ligada a spawn real; o modded tem 3.
- As janelas [tick, +2 s] ocupam ~20% do tempo e concentram **45–66% dos frames >25 ms** do modded.
- **O agravante central: o spawn não produz nada.** O proxy de população de IA fica estável ou **cai** após cada tick (modded-A 14→12; modded-B 10→7), enquanto no vanilla ele sobe após o tick real. O modded vive em **déficit permanente de bots** (vivos < BotMax), então o branch caro roda em *todo* tick, paga ~120–190 ms por ciclo, e o déficit nunca fecha — trabalho 100% desperdiçado, repetido a cada 10 s, a raid inteira. (O vanilla, com 25+ bots ≥ teto, faz early-return barato.)
- O que os 20–26 ms do tick contêm por dentro (loop do EFT × patches de mod dentro de `TrySpawn`/`ActivateBotsWithoutWave` × tamanho do déficit) é **inseparável nestes dados** (modo UpdateOnly) — é o alvo nº 1 da próxima rodada de captura.

Converge com — mas foi derivado independentemente de — o metrônomo de 10 s já medido via CapFrameX na auditoria do TRL-DynamicSpawn ([relatorio-auditoria-codigo-01](../../mods/TRL-DynamicSpawn/docs/relatorio-auditoria-codigo-01.md), AUD-01-01/05/06).

### 🥉 O3 — Hitches de fila async na main thread (`AsyncWorker`) — CONFIRMADO (mecanismo; gatilho não identificado)

Os **2 piores frames de toda a investigação** são um único dreno do `AsyncWorker` do jogo: frame de **211,5 ms** (um `FixedUpdate` de 195,1 ms, 92% do frame) e frame de **126,5 ms** (um `Update` de 105,5 ms), ambos no modded-A. O decompile mostra o porquê estrutural: `CheckForFinishedTasks()` drena a fila de continuações com `while(true)` **sem orçamento de tempo**, e as tarefas são criadas sem `RunContinuationsAsynchronously` — a conclusão de uma tarefa em background roda a cadeia async inteira inline no dreno. Vanilla nas mesmas janelas: máx 0,02 ms. **Quem enfileirou ~300 ms de trabalho é invisível no UpdateOnly** — episódico (não ocorre no modded-B), ~4,8 s após um tick de spawn. Suspeita natural: pipeline de geração de perfil/loadout de bot (converge com os bursts de `bot/generate` da investigação DynamicSpawn), não confirmável nestes dados.

### O4 — Cauda crônica 25–33 ms sem dominador — Suspeita (majoritariamente invisível)

103 frames do modded-A (9,2% do tempo) na banda 25–33 ms **sem método dominante**: SyncTransforms no topo com 2–5 ms e >50% do frame no gap não-atribuído. É a manifestação do **residual de ~+0,9 a +2,3 ms/f** da decomposição (§2.2): patches Harmony dos ~100 mods, corrotinas, callbacks de render e custo nativo extra — tudo fora do radar do UpdateOnly. Não fecha sem a próxima rodada de captura.

### O5 — População permanente de gore do Visceral — hipótese líder para O1 + acúmulo confirmado

Dois fatos independentes e um vínculo em aberto:

- **Fato (GROW/LIFE confirmado no dado e no fonte):** `VisceralCombat.Ragdolls.Classes.DismemberedLimbScaler` — componente que o Visceral adiciona em **todo transform filho** de um membro desmembrado (`KillPatch.cs:267,359` → `RagdollHelperClass.cs:561`) — tinha **161 instâncias vivas** no modded-A e **180→193 (só sobe)** no modded-B, cada uma escrevendo `transform.localScale` 3×/frame (Update + LateUpdate + OnAnimatorMove). **Não existe `Destroy()` desse componente no código** — vive enquanto o corpo existir. Entre as duas capturas (64 s), a população foi 161→193 (~+25/min em combate). O mesmo padrão vale para os decais do **VolumetricBloodFX** (BFX_*: 20→35 instâncias, sem pooling, Update ativo após a animação).
- **Fato:** o ragdoll do Visceral registra rigidbodies no sistema de física do EFT **sem o checker de visibilidade** que o jogo usa para desligar corpos fora de vista (`RagdollClass.cs:125`, `SupportRigidbody(rb, 0f)`).
- **Vínculo (Suspeita — verificação adversarial rebaixou de Forte):** essa população parada explica a **existência** do piso permanente do O1 (algo mantém `UpdateEnabled` ligado o tempo todo, e o gore está presente do bucket 0 ao fim nas duas capturas), mas a **magnitude** do piso não é função só do nº de membros (modded-A com 160 membros paga 1,93 ms/f pós-janela; modded-B com 186 paga 1,63) — há um co-driver adicional modded-only (~0,3 ms/f) correlacionado à atividade de bots. Puppets ativos do PuppetMaster (física de morte animada) adicionam ~+0,4–0,65 ms/f mas são transientes (7 s no modded-A, zero no modded-B). **Teste decisivo barato: captura A/B com Visceral desligado** (ver §7).

### O6 — Decais em renderização contínua (`DeferredDecalRenderer`) — CONFIRMADO

+0,20–0,22 ms/frame constante (7–15× o vanilla), série plana — população estável de decais (sangue nas superfícies) sendo re-renderizada todo frame. Consistente com Visceral/VolumetricBloodFX/HollywoodFX como fontes; atribuição individual não fecha nestes dados.

### O7 — `InputManager` como ímã de hitch — Suspeita

1 hitch por captura modded de **79–89 ms** dentro do despacho de comando de tecla (vanilla: máx 31,8 no mesmo ponto). O modo UpdateOnly não mostra qual handler de tecla (mod) estava pendurado na cadeia. Média neutra — só cauda.

### O8 — Custos contínuos por mod (superfície Update medida) — pequenos, nomeados

Soma de **todos** os mods na main thread: **1,14 (A) / 0,90 (B) ms/frame** — só **13–26% do delta médio** tem atribuição direta a Update de mod; nenhum mod individual é ofensor dominante nesta superfície. Os maiores: Fika 0,29–0,38 ms/f (BotStateManager; por bot fica *mais barato* que no vanilla — não é regressão), SAIN 0,10–0,23 (padrão N pares × 7 partes de corpo, decresce com mortes — não é leak), ORBIT 0,10–0,11 (perfil *hitchy*: spikes de 4,6–7,4 ms em frames isolados de path/loot), Visceral+BloodFX 0,14–0,15, **TRL-ICM 0,065–0,077 contínuos** (maior superfície: `TraumaEngine.Update` com reconcile de até 1,3 ms), TRL-SpeakFromTarkov 0,04 (polling de microfone + **9 leituras de `ConfigEntry.Value` re-aplicadas ao filtro todo frame**), Manimal-Icebreaker (`RenderEnvProbe`, 1×/frame) e SPT-QuestMap ~0,03–0,04 cada.

## 4. Evidências (síntese por ofensor)

Cada linha abaixo sobreviveu à verificação adversarial (re-derivação independente dos arquivos brutos):

| Ofensor | Evidência-chave | Veredito |
|---|---|---|
| O1 SyncTransforms | self/f 2,0509/1,5884 (modded) vs 0,1290/0,2714 (vanilla), 1 call/f nas 4; TopSelfMethod em 93,8%/93,3% dos frames; duty cycle 100% vs 7,4–14%; decompile: `Physics.Simulate` gateado por `UpdateEnabled` | **CONFIRMADO** |
| O2 Metrônomo | ticks de 20,2–25,6 ms em t=4,10/14,10/24,11 s (A) e 3,94/13,94/23,96 s (B) — gaps de 10,00 s; rajadas JobScheduler 91–160 ms nas janelas pós-tick (98,6–98,8% do custo); população de IA cai após os ticks; const `10f` no decompile | **CONFIRMADO** (metrônomo) / **PARCIAL** (fator 11–14× apoia-se em 1 tick vanilla; composição interna do tick não separável) |
| O3 AsyncWorker | frame 28154: 211,548 ms total, 195,114 ms num único call de FixedUpdate (92,2%); frame 27935: 105,46 ms; decompile: dreno `while(true)` sem orçamento; vanilla máx 0,02 ms | **CONFIRMADO** (gatilho em aberto) |
| O4 Cauda crônica | 103 frames 25–33 ms no modded-A; managed 43,6–47,3% nesses frames; residual +0,9–2,3 ms/f na decomposição | Suspeita (invisível por construção do modo) |
| O5 Gore Visceral | 242.627→656.793 callbacks/30 s; 161→193 instâncias monotônico; fonte sem Destroy; `SupportRigidbody` sem visibility checker; modded-B sem puppets mantém piso | GROW **CONFIRMADO**; vínculo com O1 **PARCIAL** (hipótese líder, co-driver residual) |
| O6 DeferredDecal | 0,2349/0,2328 vs 0,0159/0,0321 ms/f, série plana | **CONFIRMADO** |
| O7 InputManager | maxIncl 79,4/89,3 ms (modded) vs 30,5/3,9 (vanilla); média neutra | Suspeita |
| O8 Atribuição por mod | Σ mods 1,137/0,896 ms/f; nos 171 worst-frames modded nenhum método de mod é contribuinte material | **CONFIRMADO** |

## 5. O que é comportamento normal do jogo (não perseguir)

- **Rajadas do JobScheduler em spawn real** — existem no vanilla (1 rajada de 84–89 ms por 30 s ligada a `CreateBot` + descompressão LZ4 de bundle). O mecanismo é vanilla; o problema do modded é a **frequência** (3×) e o gatilho vazio (O2).
- **Custo por chamada da IA** (`AICoreLayer`, `AITaskManager`) e do IK — igual ou **menor** no modded; SAIN não encarece o tick por bot nesta superfície.
- **Fika `BotStateManager`** — por bot, *mais barato* no modded pelo inclusive (20–26 vs 26–38 µs/bot); o "aumento" aparente de self era artefato da assimetria de modo.
- **Padrão N×M×7 do SAIN** (pares bot×inimigo × 7 partes de corpo) — decresce com mortes; alto em contagem, barato em custo, sem acúmulo.
- **Fila de áudio (`GClass890`) e streaming de cena** — spikes equivalentes nos dois ambientes.
- **Rajada única do `InputManager` no início de captura** — presente nos 4 datasets (a diferença modded é só a magnitude, O7).
- **Overhead do profiler**: ~0,13 ms/f nos dois ambientes, direção conservadora (não fabrica os deltas).

## 6. Lacunas e limitações dos dados atuais

1. **Assimetria de modo (a limitação-mãe):** `harmony-patches.csv` do modded vem **vazio** — nem inventário de patches há. **66 dos 107 mods (61,7%) são completamente invisíveis** na captura modded, incluindo suspeitos de peso: BigBrain (toda a lógica do SAIN roda por dentro do patch dele), Waypoints, CustomClasses, SPTVRAMCleaner (candidato clássico a hitch periódico), TRL-Fixes, MoreCheckmarks.
2. **Detour Harmony mata a instrumentação do alvo** (achado de mecanismo, CONFIRMADO): um patch Harmony recompila o método-alvo, descartando o prólogo de instrumentação do profiler. Os 6 métodos Update-family mais caros do vanilla **ausentes do modded** (`Player.LateUpdate` 1,56–1,74 ms/f, cadeia `AICoreAgent/Strategy/Controller`, `LaserBeam.LateUpdate`, `ToDController.Update`) são **exatamente alvos de patch de mods presentes só no modded** (ICM confirmado por leitura binária do DLL implantado; prova de execução oculta: identidade de calls com `PlayerAIDataClass.LateUpdate` mostra `Player.LateUpdate` rodando 24,0×/frame sem ser visto). O managed do modded **subconta ≥1,1–1,4 ms/f** (piso). Corolário importante: *mesmo em ModAttribution os alvos patchados continuarão invisíveis — mas os handlers dos patches passam a ser medidos, que é o que falta.*
3. **A régua Harmony do vanilla** (35 patches executados: 3,1–3,3 µs/frame no total) mostra que *handler bem-comportado* não explica o delta — o risco está em patch caro em método quente e no trabalho disparado *dentro* do jogo, ambos não medidos.
4. **Cenários não pareados:** vanilla com 2–3× mais IA ativa; modded-A aparenta janela de combate (puppets, rajada de gore), modded-B pós-combate. Não há registro externo de contagem de bots/eventos por captura.
5. **Janela de 30 s:** longa o bastante para 3 ciclos do metrônomo, curta demais para degradação de raid longa (a curva RAM 10→33 GB da investigação anterior não é observável aqui; nenhuma degradação intra-captura foi detectada).
6. **Retenção de worst-frames saturada:** modded-A estourou o cap real de 100 deep frames — **47 de 147 frames ruins ficaram sem detalhe** (o próprio nº 147 vs 13–19 do vanilla é um indicador). `WorstFrameCount=20`/`MaxDeepFrames=100` de default são baixos para este ambiente.
7. **Fora do escopo do profiler:** GC (coletas/alocações), GPU, RAM, contagem direta de bots — exigem complemento (CapFrameX já em uso, contadores logados).
8. **Captura triplicada** (205433=205500=205508) reduziu o n efetivo do modded a 2.

## 7. Recomendações — o que investigar primeiro

Em ordem de retorno por esforço:

1. **Refazer a captura modded em `ModAttribution`** (1 linha no `com.spt.runtimeprofiler.cfg` + restart) **+ 1 par UpdateOnly de controle no mesmo cenário** — fecha de uma vez: atribuição dos 107 mods, inventário+timing Harmony, e a diferença de `ManagedProfiledMs` entre os dois modos mede na prática o managed hoje invisível. Validar `droppedEntries`/`MaxMethods` no diagnostics. Subir `MaxDeepFrames` (300–500) e `WorstFrameCount` (50). Registrar população de bots por fora e parear cenário pelo proxy `AICoreLayerClass.Update` calls/f.
2. **Teste A/B com Visceral desligado** (30 s, mesmo protocolo): se o piso do `SyncTransforms` cair para o padrão vanilla (~0 fora de rajada), o vínculo O5→O1 fecha e o maior delta contínuo da investigação fica explicado. Barato e decisivo. (Variante intermediária: limpar corpos/gore via config, se houver, antes de capturar.)
3. **Instrumentar o tick de spawn** (O2): logar por tick do `NonWavesSpawnScenario` o nº de tentativas/ativações e o resultado — confirmar por que o déficit nunca fecha (suspeita: cap/bloqueio no pipeline TRL-DynamicSpawn, AUD-01-05/06 da auditoria já aprovada) e repartir os 20–26 ms entre loop EFT × patches × déficit. A rodada 1 do DynamicSpawn já aprovada ataca causas prováveis — **re-capturar depois dela** para medir o efeito no metrônomo.
4. **Caçar o gatilho do AsyncWorker** (O3): com ModAttribution + F10 (deep frame manual) num hitch percebido; correlacionar timestamps com requests de backend (`bot/generate`) e logs. É o maior hitch individual e hoje não tem dono.
5. **Captura longa (5–10 min) no modded** — curva de crescimento das populações GROW (gore, decais BFX) e tendência de frametime; casar com curva de RAM/CapFrameX para conectar com a degradação de raid longa já documentada.
6. **Depois dos itens 1–5**, decidir sobre os custos contínuos menores (O8: ICM, SpeakFromTarkov, Icebreaker, decais) — hoje somam <1 ms/f e nenhum justifica refactor antes de fechar O1–O3.

---

**Anexos** (análises dimensionais completas, com metodologia e tabelas integrais): [frames-baseline](./anexos-profiling-2026-08-22/frames-baseline.md) · [timeline-spikes](./anexos-profiling-2026-08-22/timeline-spikes.md) · [worst-frames](./anexos-profiling-2026-08-22/worst-frames.md) · [mod-census](./anexos-profiling-2026-08-22/mod-census.md) · [game-systems-delta](./anexos-profiling-2026-08-22/game-systems-delta.md) · [coverage-gaps](./anexos-profiling-2026-08-22/coverage-gaps.md)

## Histórico

| Data | Autor | Descrição |
|---|---|---|
| 2026-08-22 | Guilherme | docs(perf): add DynamicSpawn audit report + ICM/Stances optimization handoffs |
