---
title: "Anexo profiling 2026-08-22 — game-systems-delta"
date: 2026-08-22
status: 🟢 Vivo
authors: Claude (investigação de performance 2026-08-22, análise dimensional)
---

# Delta dos sistemas do jogo entre ambientes (conjunto comparável)

> Dimensão: game-systems-delta · Data: 2026-08-22 · Capturas: vanilla-A/B (SPT_2, ModAttribution), modded-A/B (SPT/TRL, UpdateOnly) · Todas ~30s, Customs, main thread (ThreadId=1)

## Metodologia

- Fonte: `methods.csv` (agregados por método), `edges.csv` (cadeia caller→callee), `timeline.csv` (buckets de 250ms), `frames.csv` (frames por bucket) de cada captura. Cálculos em Python 3.14; nenhum número estimado.
- **Normalização**: tudo em ms/frame e calls/frame (frame counts diferem: 2082 / 1766 / 1507 / 1767). Séries por bucket divididas pelos frames reais do bucket (contados de `frames.csv` por faixa de timestamp).
- **Comparação limpa** = método folha nos dois modos (self==inclusive nas 4 capturas) → self diretamente comparável. **Artefato de atribuição** = método com callees instrumentados só no vanilla (ModAttribution) → no modded o self absorve os filhos; nesses casos usa-se o INCLUSIVE (inclusivo é inclusivo independentemente da instrumentação dos filhos — ambos os modos instrumentam o mesmo nó raiz).
- Proxy de população de bots: `AICoreLayerClass.Update` calls/frame (existe nas 4): vanilla-A **24.42**, vanilla-B **22.85**, modded-A **12.50**, modded-B **8.30**. Proxy de players totais (GWUTL): vanilla via `Player.LateUpdate` (25.45 / 32.56); modded via SAIN `PlayerDataExtensions.Update` +1 (≈14.5 / ≈9.3+1≈10.3).
- Último bucket (parcial) descartado nas séries.

### Cadeias de chamada confirmadas em `edges.csv` (vanilla-A)

- `EFTPhysicsClass.Update` → `SyncTransformsClass.Update` (268.6 ms incl / 2082 calls) — idem no modded-A (3090.7 ms / 1508 calls).
- `GameWorldUnityTickListener.Update` → `FikaHostGameWorld.PlayerTick` (1535.5 ms incl = 0.738 ms/f, praticamente todo o subtree) → tick de todos os players.
- Fika `BotStateManager.Update` → `AICoreControllerClass.Update` (1142 ms) → `AICoreAgentClass.Update` → `AICoreStrategyAbstractClass.Update` → `AICoreLayerClass.Update`; e → `AITaskManager.Update` (237 ms). **Todo o cérebro de IA roda DENTRO do BotStateManager.Update do Fika** → o inclusive dele cobre o tick completo de IA + sync Fika.

## 1. Tabela cross-env (self ms/frame · calls/frame)

Colunas: self/f (calls/f). “Limpa?” = folha nos dois modos. Δ = modded−vanilla no par de mesmo sufixo (A−A, B−B).

| Método | vanilla-A | vanilla-B | modded-A | modded-B | Δ(A) | Δ(B) | Limpa? |
|---|---|---|---|---|---|---|---|
| **SyncTransformsClass.Update** | 0.1290 (1.00) | 0.2714 (1.00) | **2.0509** (1.00) | **1.5884** (1.00) | **+1.922** | **+1.317** | ✅ folha (self=incl nas 4) |
| GameWorldUnityTickListener.Update **(INCLUSIVE)** | 0.7688 (1.00) | 0.7523 (1.00) | 1.0784 (1.00) | 0.6397 (1.00) | +0.310 | −0.113 | ⚠️ usar inclusive; self não comparável (vanilla 0.016 vs modded 0.819/0.500 = artefato) |
| DeferredDecalRenderer.Update | 0.0159 (1.00) | 0.0321 (1.00) | 0.2349 (1.00) | 0.2328 (1.00) | +0.219 | +0.201 | ✅ folha |
| JobScheduler.LateUpdate (Diz.Jobs) | 0.0411 (1.00) | 0.0512 (1.00) | 0.2046 (1.00) | 0.2219 (1.00) | +0.164 | +0.171 | ⚠️ quase-folha; vanilla-B incl 0.1008>self (callee instrumentado só lá) |
| AsyncWorker.Update (Diz.Utils) | 0.0003 (1.00) | 0.0003 (1.00) | 0.0720 (1.00) | 0.0010 (1.00) | +0.072 | +0.001 | ⚠️ pump de continuações; callees invisíveis no modded |
| AsyncWorker.FixedUpdate | 0.0005 (0.86) | 0.0006 (1.02) | 0.1311 (1.19) | 0.0009 (1.02) | +0.131 | +0.000 | ⚠️ idem |
| InputManager.Update | 0.1190 (1.00) | 0.1147 (1.00) | 0.1538 (1.00) | 0.1578 (1.00) | +0.035 | +0.043 | ⚠️ incl≈self nos dois; maxIncl 30.5/3.9 vanilla vs **79.4/89.3** modded |
| EventSystem.Update (uGUI) | 0.0153 (0.05) | ausente (0) | 0.0564 (0.12) | 0.0350 (0.08) | +0.041 | +0.035 | ✅ folha; 0.32–0.46 ms/call nos dois |
| IKSolver.Update (FinalIK) | 0.0500 (2.91) | 0.0524 (3.22) | 0.0667 (4.55) | 0.0658 (4.16) | +0.017 | +0.013 | ✅ folha; per-call igual (0.0147–0.0172 ms) |
| GPUInstancerManager.Update | 0.1112 (4.00) | 0.2129 (4.00) | 0.2119 (4.00) | 0.2297 (4.00) | +0.101 | +0.017 | ✅ folha; vanilla-A é o outlier baixo — vs vanilla-B é neutro |
| AmbientLight.LateUpdate | 0.1650 (1.00) | 0.1867 (1.00) | 0.2055 (1.00) | 0.1796 (1.00) | +0.041 | −0.007 | ✅ folha, neutro |
| StaticManager.Update | 0.1033 (1.00) | 0.1157 (1.00) | 0.1399 (1.00) | 0.1124 (1.00) | +0.037 | −0.003 | ✅ neutro |
| CullingManager.Update | 0.0772 (1.00) | 0.0802 (1.00) | 0.0855 (1.00) | 0.0756 (1.00) | +0.008 | −0.005 | ✅ neutro |
| ComponentSystem`2.Update | 0.1392 (8.00) | 0.1717 (8.00) | 0.1742 (8.00) | 0.1509 (8.00) | +0.035 | −0.021 | ✅ neutro (8 instâncias fixas nos 4) |
| ComponentSystem`2.LateUpdate | 0.1177 (8.00) | 0.1501 (8.00) | 0.1452 (8.00) | 0.1402 (8.00) | +0.028 | −0.010 | ✅ neutro |
| DistantShadow.Update | 0.0480 (1.00) | 0.0500 (1.00) | 0.0514 (1.00) | 0.0491 (1.00) | +0.003 | −0.001 | ✅ neutro |
| FlareScheduler.LateUpdate | 0.0746 (1.00) | 0.0603 (1.00) | 0.0483 (1.00) | 0.0399 (1.00) | **−0.026** | **−0.020** | ✅ NEGATIVO no modded |
| PerfectCullingCrossSceneGroup.Update | 0.0343 (26.99) | 0.0359 (26.98) | 0.0852 (27.00) | 0.0179 (27.00) | +0.051 | **−0.018** | ✅ folha; divergente entre modded-A/B (dependente de câmera), líquido ~neutro |
| DecalSystem.LateUpdate | 0.0011 (9.00) | 0.0010 (8.99) | 0.0011 (9.00) | 0.0010 (9.00) | 0.000 | 0.000 | ✅ idêntico (9 instâncias nas 4) |
| TextureDecalsPainter.Update | 0.0002 (1.00) | 0.0007 (1.00) | 0.0023 (1.00) | 0.0018 (1.00) | +0.002 | +0.001 | ✅ ~neutro |
| GameWorld.Update (EFT) | 0.0009 (1.00) | 0.0010 (1.00) | 0.0018 (1.00) | 0.0018 (1.00) | +0.001 | +0.001 | ✅ ~neutro |
| ClientGameWorld.LateUpdate | 0.0002 (1.00) | 0.0003 (1.00) | 0.0002 (1.00) | 0.0002 (1.00) | 0.000 | 0.000 | ✅ idêntico |
| GameWorldUnityTickListener.FixedUpdate | 0.0038 (0.86) | 0.0051 (1.02) | 0.0038 (1.19) | 0.0031 (1.02) | 0.000 | −0.002 | ✅ neutro |
| SyncTransformsClass.FixedUpdate | 0.0002 (0.86) | 0.0002 (1.02) | 0.0002 (1.19) | 0.0002 (1.02) | 0.000 | 0.000 | ✅ idêntico → UpdateMode nunca é FixedUpdate; sim roda no Update |

### Sistemas de IA — normalizados POR BOT (proxy = AICoreLayer calls/frame)

| Métrica | vanilla-A (24.42) | vanilla-B (22.85) | modded-A (12.50) | modded-B (8.30) | Veredito |
|---|---|---|---|---|---|
| AICoreLayerClass.Update self **ms/call** | 0.00633 | 0.00444 | 0.00497 | 0.00581 | igual (dentro da variância vanilla) |
| AITaskManager.Update self/f **por bot** | 0.00460 | 0.00463 | 0.00378 | 0.00370 | **−20% no modded** (neutro/negativo) |
| AICoreAgentClass.Update ms/call | 0.00975 | 0.00456 | — | — | GAP no modded |
| AICoreStrategyAbstractClass.Update ms/call | 0.00309 | 0.00368 | — | — | GAP no modded |
| BotStateManager.Update (Fika) **INCLUSIVE**/f | 0.9419 | 0.8252 | 0.5919 | 0.4688 | total MENOR no modded (menos bots) |
| BotStateManager incl/f **por bot** | 0.0386 | 0.0361 | 0.0473 | 0.0565 | **+23–56% por bot** no modded |
| GWUTL.Update INCLUSIVE/f **por player** (players: 25.45/32.56/≈14.5/≈10.3) | 0.0302 | 0.0231 | 0.0744 | 0.0621 | **2.1–3.2x por player** no modded |

GAP: `AICoreAgentClass.Update`, `AICoreControllerClass.Update` e `AICoreStrategyAbstractClass.Update` **não existem** no `methods.csv` do modded (nenhuma thread), embora sejam métodos chamados `Update` e o modo UpdateOnly capture `AICoreLayerClass.Update` e `AITaskManager.Update`. No modded o edge é `BotStateManager.Update` → `AICoreLayerClass.Update` direto (os intermediários somem da cadeia). Hipótese: repatch do BigBrain (DrakiaXYZ, dependência do SAIN, que troca a seleção de layers do cérebro) invalida a instrumentação desses métodos. Consequência: o custo dos intermediários cai no self de `BotStateManager.Update` no modded → o **+23–56%/bot do BotStateManager self é parcialmente artefato**; o inclusive por bot (acima) é a comparação válida.

## 2. Ranking por delta (ms/frame, par a par)

| # | Sistema | Δ(A) | Δ(B) | Natureza |
|---|---|---|---|---|
| 1 | **SyncTransformsClass.Update** | **+1.922** | **+1.317** | contínuo, 100% dos frames |
| 2 | GWUTL.Update (inclusive, por player 2.1–3.2x) | +0.310 | −0.113 | contínuo; total mascarado por menos bots |
| 3 | DeferredDecalRenderer.Update | +0.219 | +0.201 | contínuo, série plana |
| 4 | AsyncWorker Update+FixedUpdate | +0.203 | +0.001 | **2 hitches únicos** (105.5 + 195.1 ms) só no modded-A |
| 5 | JobScheduler.LateUpdate | +0.164 | +0.171 | rajadas de 16–76 ms/bucket, frequentes no modded |
| 6 | InputManager.Update | +0.035 | +0.043 | média pequena; maxIncl 79–89 ms (hitch) |
| 7 | EventSystem.Update | +0.041 | +0.035 | contínuo pequeno (mais UI viva no modded) |
| 8 | IKSolver.Update | +0.017 | +0.013 | mais solvers ativos (4.2–4.6/f vs 2.9–3.2/f com 2–3x MENOS bots); per-call igual |
| — | FlareScheduler.LateUpdate | −0.026 | −0.020 | negativo |
| — | BotStateManager (Fika) inclusive | −0.350 | −0.356 | negativo em total (menos bots), positivo por bot |
| — | AICoreLayer+AITaskManager | −0.157 | −0.129 | negativo (menos bots; per-call igual) |

Contexto de magnitude: frametime médio modded-A 19.92 ms vs vanilla-A 14.41 (Δ +5.51); só o SyncTransforms explica **35%** do Δ do par A. No par B os médios empatam (16.97 vs 16.99) — mas o vanilla-B carregava **22.85 bots vs 8.30** do modded-B; a igualdade de frametime com 1/3 da população de IA é em si a medida do custo do stack TRL.

## 3. SyncTransformsClass.Update — o nº 1, mecanismo e hipóteses

### O que a classe é (decompile)

`references/eft-decompiled/Assembly-CSharp/EFTPhysicsClass.cs` — `SyncTransformsClass` é **tipo aninhado de `EFTPhysicsClass`** (alias 4.1: `PhysicsExtensions`). Apesar do nome, o `Update()` dela **não** é `Physics.SyncTransforms()` — é o **passo inteiro de simulação do PhysX**:

- EFT roda com `Physics.simulationMode = Script` e um dos modos `Update/QualityControl/SmoothSimulate`; `Update()` chama **`Physics.Simulate(deltaTime)`** (modo SmoothSimulate: média móvel de deltaTime via `GClass828`, teto `MaxSmoothPhysicsDeltaTime`). `FixedUpdate()` só simularia no modo FixedUpdate — e mede 0.0002 ms/f nas 4 capturas → o modo em raid é o do Update. (`SyncTransforms()`/`ForceSyncTransforms()` existem na classe mas são estáticos à parte, fora do caminho do Update.)
- **Gate de liga/desliga**: `Boolean_0 = (simulationMode==Script) && UpdateEnabled`. `UpdateEnabled` é controlado por `EFTPhysicsClass.GClass745` — um registro de rigidbodies "suportados" (`SupportRigidbody(rb, quality, visibilityChecker)`). A cada `GClass745.Update()` (chamado por `EFTPhysicsClass.Update` junto com o Simulate), a lista é varrida: rigidbody nulo/inativo sai; conta quantos estão **ativos na hierarquia e visíveis** (checker null = sempre conta). **Se a contagem é 0, `UpdateEnabled=false` e o Simulate para de rodar.**
- Quem registra rigidbodies (grep no decompile): `RagdollClass` (ragdoll de cadáver, quality 0, **sem** visibility checker), `LootItem` (com checker), `Grenade`/`GrenadeCartridge`/`FlareCartridge`, `SmallPhysicsObject`, `BrokenDoor`, `TripwireLogicClass`, `MgBelt`, `BeltDetachablePart`, `VehicleSuspension`/`WheelDrive`, airdrop.

Ou seja: **no design do EFT a simulação física por script só roda enquanto existe prop físico ativo (ragdoll caindo, granada voando, loot solto); quando tudo assenta/é desregistrado, o custo é zero.**

### Quantificação precisa

self==inclusive nas 4 capturas (folha; caller único `EFTPhysicsClass.Update`) → comparação limpa.

| | vanilla-A | vanilla-B | modded-A | modded-B |
|---|---|---|---|---|
| self ms/frame (média da captura) | 0.1290 | 0.2714 | **2.0509** | **1.5884** |
| Δ vs vanilla mesmo sufixo | — | — | **+1.922** | **+1.317** |
| **Duty cycle** (buckets 250ms com >0.1 ms/f) | **7.5%** (9/120) | **14.2%** (17/120) | **100%** (120/120) | **100%** (120/120) |
| ms/f médio nos buckets ATIVOS | 2.49 | 2.17 | 2.10 | 1.63 |
| ms/f médio nos buckets ociosos | 0.0005 | 0.0005 | — (não há) | — (não há) |
| mínimo de bucket (ms/f) | 0.000 | 0.000 | **1.272** | **0.956** |
| máximo de bucket (ms/f) | 3.84 | 3.02 | 3.40 | 2.33 |
| TopSelfMethod do frame (histograma frames.csv) | raro | raro | 94% dos frames | 93% dos frames |
| maxIncl 1 frame | 17.56 | 8.96 | 4.84 | 3.20 |

**O mecanismo do delta não é "passo mais caro" — é "passo nunca desliga".** O custo por frame QUANDO ATIVO é igual ou até menor no modded (2.10/1.63) que nos bursts do vanilla (2.49/2.17, janelas de 2–2.5s quando um ragdoll/granada está vivo: vanilla-A buckets 64–72 ≈ t 16.0–18.25s; vanilla-B buckets 46–55 e 73–79). O vanilla paga esse preço 7.5–14.2% do tempo; o modded paga 100% do tempo, do bucket 0 ao último, sem nunca retornar a zero (piso 0.96–1.27 ms/f).

### Séries e correlações (timeline 250ms, modded)

- População Visceral: `VisceralCombat.Ragdolls.Classes.DismemberedLimbScaler` (Update+LateUpdate todo frame) — **161 instâncias constantes** no modded-A (146–193 por ruído de borda de bucket), **180→186→193** no modded-B. Presentes desde o bucket 0 (captura começou no meio da raid) até o fim. São **~322–386 callbacks/frame** só de membros desmembrados, e cada instância escreve transform (o self delas é barato: 0.067+0.046 ms/f somados no A) — o custo caro aparece do lado do motor.
- Decals BFX (VolumetricBloodFX `BFX_DecalSettings`): 18–24 instâncias (A), 21–35 (B), constantes.
- PuppetMaster (RootMotion.Dynamics DENTRO do namespace VisceralCombat): **só no modded-A**, janela buckets 16–47 (t≈4–12s) com 12–35 `Muscle.Update`/frame (pico 35 ≈ 1–2 puppets ativos), `PuppetMaster.FixedUpdate` 455 calls (incl máx 16.8ms). **modded-B: zero PuppetMaster/Muscle na captura inteira.**

Correlações (Pearson, sync self/f por bucket × série, 120 buckets):

| Série | modded-A | modded-B | detrended (1ª diferença) A / B |
|---|---|---|---|
| instâncias LimbScaler | 0.158 | 0.037 | 0.158 / 0.043 |
| bots (AICoreLayer c/f) | 0.541 | −0.019 | 0.124 / 0.019 |
| instâncias BFX decal | 0.159 | −0.018 | 0.163 / 0.033 |
| muscles ativos (só A) | 0.261 | — | 0.238 / — |

Leitura honesta: as correlações são fracas **porque as séries explicativas são quase constantes** (membros ~161/~186 fixos, decals ~20/~30 fixos) — sem variância não há correlação, e o que sobra no modded-A é uma tendência lenta de queda do sync (2.5→1.4 ms/f) que acompanha a queda de bots (14→12; corr 0.54 espúria de tendência, cai para 0.12 detrended). Teste de janela do PuppetMaster (modded-A): sync médio DENTRO da janela de muscles = 2.23 ms/f, ANTES = 2.59, DEPOIS = 1.93 — o pico absoluto da captura (3.10–3.40 ms/f, buckets 16–18) coincide com a ENTRADA dos muscles (12→28→35), mas o nível base de ~2.5 já existia antes sem nenhum muscle ativo. **Puppets ativos adicionam ~+0.5–0.9 ms/f transitórios; não explicam o piso.**

### Hipóteses de quem mantém a simulação ligada (classificadas)

| Hipótese | Plausibilidade | Evidência a favor | Evidência contra |
|---|---|---|---|
| **H1 — População permanente de rigidbodies registrados e "vivos" do stack de gore** (161–193 membros do Visceral + cadáveres PuppetMaster + gibs HollywoodFX): pelo menos 1 rigidbody ativo+visível existe SEMPRE → `UpdateEnabled` nunca desliga; e o `DismemberedLimbScaler` escreve transform TODO frame (Update+LateUpdate, 484k–656k escritas/30s) mantendo os corpos físicos sujos/acordados, o que encarece cada `Physics.Simulate` | **ALTA** | Duty cycle 100% desde bucket 0 com membros presentes desde bucket 0 e contagem constante; vanilla com o mesmo código de jogo desliga em segundos; piso modded (0.96–1.27) alto demais para cena dormindo; modded-B tem MAIS membros (186–193 vs 161) e piso ativo, mesmo sem nenhum puppet | Correlação bucket-a-bucket nula — mas esperada nula com população constante; não dá para ver do profiling QUEM registrou os rigidbodies |
| **H2 — Puppets PuppetMaster ativos (ragdoll vivo simulando)** | MÉDIA (contribuição transitória, não o piso) | Pico global do sync (3.40) na entrada da janela de muscles no modded-A; +0.5–0.9 ms/f na janela | modded-B: zero muscles a captura inteira e ainda assim 1.59 ms/f constante; nível pré-janela (2.59) > nível na janela (2.23) |
| **H3 — Decals de sangue físicos (BFX/decals)** | BAIXA para o SyncTransforms | 18–35 instâncias vivas constantes | corr nula; BFX_Decal é componente visual (shader/settings), sem indício de rigidbody; o custo de decal aparece em DeferredDecalRenderer, não na física |
| **H4 — População de bots/players movendo transforms** | **REFUTADA** | — | vanilla com 22.85–24.42 bots (2–3x mais) tem sync ≈ 0.0005 ms/f fora de burst → personagem (kinematic) não dispara esse custo; modded-B com 8.3 bots paga 1.59 |

### Próximos passos de medição (SyncTransforms)

1. Instrumentação dirigida: patch temporário em `EFTPhysicsClass.GClass745.smethod_0` logando `List_0.Count` e a contagem ativa+visível a cada 1s no modded → confirma H1 diretamente (quem está na lista: dump de `rb.name`/hierarchy).
2. `Physics.autoSyncTransforms`/`simulationMode` no modded vs vanilla via BepInEx console (descarta mod que muda o modo global).
3. Contar rigidbodies não-dormindo: `FindObjectsOfType<Rigidbody>().Count(rb => !rb.IsSleeping())` amostrado — separa "muitos corpos acordados" (H1 forte) de "poucos acordados mas Simulate caro por broadphase gigante".
4. Teste A/B de config: Visceral com dismemberment/persistência de membros OFF (ou limite de membros baixo) → repetir captura UpdateOnly; expectativa H1: sync/f cai para perto do padrão burst do vanilla.

## 4. DeferredDecalRenderer.Update — 0.233/0.236 vs 0.016/0.032

Decompile (`DeferredDecals/DeferredDecalRenderer.cs`): `Update()` itera `list_0` (decals registrados) chamando `ManualUpdate()` em cada um; se algum mudou, `method_13()` (rebuild do command buffer). Custo ∝ nº de decals registrados. Folha nos dois modos → comparação limpa.

- vanilla-A 0.0159 ms/f (p10–p90 por bucket 0.014–0.019), vanilla-B 0.0321 (0.029–0.036) — o próprio vanilla dobra com mais combate (mais decals de impacto).
- modded-A 0.2349 (p10–p90 **0.221–0.254**), modded-B 0.2328 (0.217–0.254) — **série plana a captura inteira**, sem crescimento: população ESTÁVEL de decals ~7–15x maior que a vanilla, já estabelecida no início da captura.
- Δ = **+0.219 / +0.201 ms/frame**. Atribuível a decals de sangue? Consistente mas não provado: BFX_DecalSettings mantém 18–35 decals de sangue vivos constantes (corr ddr×bfx = 0.52 no A / −0.04 no B — inconclusiva porque ambas as séries são planas), e mods de gore (Visceral/HollywoodFX/BFX) são os únicos produtores novos de decal do stack. Alternativa: acúmulo de decals de impacto de combate anterior à captura que não expiram. Próximo passo: dump de `list_0.Count` (e tipos/materiais dos decals) via patch temporário; teste A/B com BFX/gore decals off.

## 5. GameWorldUnityTickListener.Update (INCLUSIVE nos dois — self não comparável)

Self no modded (0.819/0.500) é artefato puro: no vanilla os callees (FikaHostGameWorld.PlayerTick etc.) estão instrumentados e drenam o self para 0.016. Inclusive é comparável (mesmo nó raiz instrumentado nas 4):

| | vanilla-A | vanilla-B | modded-A | modded-B |
|---|---|---|---|---|
| incl/f | 0.7688 | 0.7523 | 1.0784 | 0.6397 |
| players estimados | 25.45 | 32.56 | ≈14.5 | ≈10.3 |
| **incl/f por player** | 0.0302 | 0.0231 | **0.0744** | **0.0621** |
| p50 / p90 / max por bucket (ms/f) | 0.750/0.974/1.81 | 0.735/0.875/1.88 | 1.021/1.404/3.55 | 0.640/0.804/2.00 |

O tick de mundo por player custa **2.1–3.2x** no modded. Aqui dentro vivem os patches Harmony invisíveis do modo UpdateOnly (SAIN, Fika, TRL e ~100 mods patcham métodos do caminho PlayerTick/DoWorldTick). Cautela: (i) players do modded são estimativa por proxy SAIN; (ii) parte do custo do GWUTL é fixa (não escala com player) — o vanilla A→B quase não muda de 25→32 players, então com 10–14 players o "por player" infla o denominador contra o modded... e mesmo assim o modded-A fica +0.31 ms/f ACIMA do vanilla em termos absolutos com metade dos players. Evidência: Suspeita (direção clara, magnitude exata contaminada pelo custo fixo).

## 6. Hitches: AsyncWorker, JobScheduler, InputManager

- **AsyncWorker (Diz.Utils)** — `Update()/FixedUpdate()` = `GClass1516_0.CheckForFinishedTasks()`: pump que executa no main thread as continuações de tasks async terminadas. Vanilla: 0.0003–0.0008 ms/f, max 0.02. Modded-B: idem (max 0.88). **Modded-A: dois eventos únicos — Update maxIncl 105.46 ms em t≈25.0s e FixedUpdate maxIncl 195.11 ms em t≈28.75s** (este É o frame de 211.5 ms da captura). Ou seja: alguma task async (carga de asset/bundle, spawn?) entregou uma continuação gigantesca no main thread, 2x em 4s. Callees invisíveis no UpdateOnly → próximo passo: repetir captura em modo com atribuição, ou patch de log em CheckForFinishedTasks cronometrando cada continuação com stack da task.
- **JobScheduler (Diz.Jobs) LateUpdate** — processa fila de jobs com orçamento por frame. Média 4–5x maior no modded (0.205/0.222 vs 0.041/0.051) e rajadas maiores e mais frequentes: buckets >16ms — modded-A: 21–23 (20–35ms), 57–62 (16–27ms), 96–97 (17–**69**ms); modded-B: 16–19 (21–44ms), 56–57 (58/42ms), 96–98 (**76**/37/41ms); vanilla-A: um episódio (buckets 64–69, até 24ms — coincidindo com a janela de ragdoll do sync!); vanilla-B: um episódio (96–98, até 42ms). Os buckets ~14s, ~24s de gap no modded merecem cruzamento com o "metrônomo de spawn" de outra dimensão. maxIncl de 1 frame: 34.2/34.0 modded ≈ 34.4 vanilla-B — o pior frame individual é igual; a FREQUÊNCIA é o delta.
- **InputManager.Update** — média +0.035/+0.043 (0.154/0.158 vs 0.119/0.115), mas maxIncl **79.4 (modded-A) / 89.3 (modded-B)** vs 30.5/3.9 vanilla. incl≈self nos dois modos; o InputManager processa a fila de comandos de input → algum handler de comando caro (menu/mapa de mod?) ou GC no caminho. Pontual, 1–2 frames por captura.

## 7. Sistemas neutros e negativos — calibração da comparação

A régua da credibilidade: se o delta fosse artefato de modo/overhead do profiler, TODAS as linhas subiriam no modded. Não sobem:

- **Idênticos** (Δ<0.003): DecalSystem.LateUpdate (9 inst., 0.0010–0.0011 nas 4), ClientGameWorld.LateUpdate, SyncTransformsClass.FixedUpdate, GameWorldUnityTickListener.FixedUpdate, GameWorld.Update, DistantShadow.Update, CullingManager.Update.
- **Neutros** (±0.04, dentro da variância vanilla-A↔B): StaticManager, AmbientLight.LateUpdate, ComponentSystem`2 (Update e LateUpdate, 8 instâncias fixas nos 4), GPUInstancerManager (vs vanilla-B; vanilla-A é outlier baixo), PerfectCulling (divergente A/B, líquido ~0), TextureDecalsPainter.
- **Negativos no modded**: FlareScheduler.LateUpdate (−0.026/−0.020, −35%), AITaskManager por bot (−20%), BotStateManager e AICoreLayer em total absoluto (menos bots).
- **Per-call idênticos entre ambientes**: IKSolver.Update 0.0147–0.0172 ms/call nas 4; AICoreLayerClass.Update 0.0044–0.0063 ms/call nas 4 — o custo unitário do mesmo código é o mesmo nos dois lados → overhead de instrumentação equivalente, deltas restantes são reais.

## 8. Gaps de instrumentação relevantes a esta dimensão

1. `AICoreAgentClass/AICoreControllerClass/AICoreStrategyAbstractClass.Update` ausentes do modded (hipótese: repatch BigBrain) → custo deles diluído no self do BotStateManager.
2. `EFT.Player.LateUpdate` (1.63/1.66 ms/f no vanilla, maior método) não instrumentado no modded → invisível, nem no self de ninguém (callback Unity direto). O managed profiled do modded subconta ~1.0–1.6 ms/f só aí.
3. Harmony patches do modded 100% invisíveis (harmony-patches.csv vazio) → deltas "inclusive" (GWUTL, BotStateManager) são o único lugar onde esse custo aparece, somado.
4. Séries explicativas constantes (membros, decals) impedem atribuição por correlação — só experimento A/B ou instrumentação dirigida decide.

## 9. Síntese

O conjunto comparável tem UM ofensor dominante e contínuo — a simulação física por script do EFT (`SyncTransformsClass.Update` = `Physics.Simulate`) rodando com duty cycle 100% no modded (piso 0.96–1.27 ms/f, média 1.59–2.05) contra 7.5–14.2% no vanilla (média 0.13–0.27), delta +1.32 a +1.92 ms/frame, TopSelfMethod em 93–94% dos frames do modded — com o mecanismo apontando para a população permanente de ~161–193 membros desmembrados do Visceral (+cadáveres PuppetMaster) que mantém o gate `UpdateEnabled` do EFT permanentemente ligado. Segundo escalão: decals persistentes (+0.20–0.22), tick de mundo 2.1–3.2x por player (patches invisíveis dentro), fila de jobs 4–5x com rajadas de até 76ms, e dois hitches de 105/195ms de continuações async só no modded-A. Os sistemas de IA por bot, culling, decal-system e flare são iguais ou mais baratos no modded — a comparação está calibrada.

## Histórico

| Data | Autor | Descrição |
|---|---|---|
| 2026-08-22 | Guilherme | docs(perf): add DynamicSpawn audit report + ICM/Stances optimization handoffs |
