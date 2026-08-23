---
title: "Anexo profiling 2026-08-22 — mod-census"
date: 2026-08-22
status: 🟢 Vivo
authors: Claude (investigação de performance 2026-08-22, análise dimensional)
---

# Atribuição por mod + census de instâncias + crescimento (mod-census)

> Dimensão: atribuição direta por mod no ambiente modded (UpdateOnly), census de instâncias MonoBehaviour vivas via calls/frame, crescimento ao longo dos 30s, e perspectiva vs delta total de frametime.
> Capturas: modded-A (`D:\SPT\BepInEx\profiling\2026-08-22_205500`, 1507 frames), modded-B (`D:\SPT\BepInEx\profiling\2026-08-22_205604`, 1767 frames); vanilla-A/B (`D:\SPT_2\...\203734` / `203938`) como referência para Fika e tipos de jogo.
> Todos os números calculados via Python direto dos CSVs (nunca estimados).

## 0. Viés de instrumentação (leia antes de qualquer tabela)

1. **UpdateOnly instrumenta POR NOME, não por herança de MonoBehaviour.** Prova: `SAIN.Extensions.PlayerDataExtensions` aparece 4× em methods.csv — são 4 **overloads estáticos** de `Update(...)` (PlayerWeaponData / PlayerVelocityData / PlayerHeadData / PlayerNavData — StableKeys conferidos). `EnemyPartDataClass`, `EnemyPartsClass`, `RaycastResult` (SAIN) e `MovementSystem.Update(List<Agent>)` (ORBIT) também são classes comuns, não MonoBehaviours. Consequência: **calls/frame = nº de instâncias vivas SÓ vale para MonoBehaviours reais** (DismemberedLimbScaler, BFX_*, componentes 1/frame); para os demais é contagem de invocação.
2. **Só superfícies `Update`/`LateUpdate`/`FixedUpdate` são visíveis.** Harmony patches dos ~100 mods, corrotinas, handlers de evento e métodos com outros nomes são invisíveis (harmony-patches.csv do modded está vazio). Exemplo concreto: `DismemberedLimbScaler.OnAnimatorMove()` faz o MESMO trabalho de Update/LateUpdate e **não aparece** — o custo real do componente é ~1,5× o medido. O custo real de cada mod pode ser MUITO maior que o self listado.
3. **Self no modded absorve callees não instrumentados.** Ex.: `BotStateManager.Update` (Fika) no modded contém o custo de `SendBatchStates`/`_controller.Update` inteiro; no vanilla (ModAttribution) esses callees foram instrumentados à parte. **Comparação cross-env justa: usar Inclusive, não Self.**
4. Frames diferentes por captura → tudo normalizado por frame.

## 1. Tabela completa por mod (mod-summary, main thread, self)

Ordenado por modded-B. `m` = métodos observados. Self = SÓ superfície Update (viés §0.2).

| Mod (PluginName) | A: self ms/f | A: calls | A: m | B: self ms/f | B: calls | B: m | B: maxIncl 1 call (ms) |
|---|---|---|---|---|---|---|---|
| Fika.Core | 0.3795 | 20 301 | 14 | 0.2944 | 23 035 | 13 | 7.88 |
| ORBIT | 0.0962 | 31 935 | 19 | 0.1077 | 42 353 | 19 | 7.42 |
| SAIN | 0.2304 | 715 858 | 25 | 0.1046 | 351 820 | 19 | 0.17 |
| Visceral Combat | 0.1035 | 493 923 | 7 | 0.0751 | 656 793 | 2 | 0.16 |
| SSH EFT Volumetric Blood FX | 0.0495 | 94 941 | 3 | 0.0661 | 158 551 | 3 | 0.18 |
| TRL-ImmersiveCombatMedicine | 0.0771 | 12 056 | 8 | 0.0654 | 14 136 | 8 | 1.07 |
| TRL-SpeakFromTarkov | 0.0440 | 15 070 | 10 | 0.0409 | 17 670 | 10 | 0.14 |
| Manimal-Icebreaker | 0.0389 | 3 015 | 2 | 0.0370 | 3 535 | 2 | 0.12 |
| SPT-QuestMap Client | 0.0404 | 7 540 | 5 | 0.0368 | 8 840 | 5 | 0.80 |
| TRL-StancesAndMobility | 0.0107 | 5 656 | 4 | 0.0104 | 7 068 | 4 | 0.66 |
| Fontaine-FOVFix | 0.0088 | 1 508 | 1 | 0.0080 | 1 768 | 1 | 0.06 |
| SPTRecoilRework | 0.0086 | 7 817 | 3 | 0.0079 | 8 867 | 3 | 0.15 |
| Janky-HollywoodFX | 0.0079 | 18 213 | 10 | 0.0071 | 21 287 | 10 | 0.04 |
| SPT Runtime Profiler (overhead) | 0.0069 | 1 507 | 1 | 0.0065 | 1 767 | 1 | 0.02 |
| Janky-HollywoodGraphics | 0.0047 | 4 521 | 3 | 0.0050 | 5 301 | 3 | 0.09 |
| TRL-ImmersiveOverlays | 0.0032 | 1 507 | 1 | 0.0034 | 1 767 | 1 | 0.04 |
| RedLine Restarter | 0.0037 | 1 507 | 1 | 0.0031 | 1 767 | 1 | 0.15 |
| (26 mods restantes, cada ≤0.0015) | ~0.021 | — | — | ~0.016 | — | — | — |
| **TOTAL mods main thread** | **1.137** | | | **0.896** | | | |

Fora da main thread: só SAIN aparece com custo relevante (≈2.7 ms totais em 30 s na thread 2 — desprezível).

## 2. Census por método (methods.csv, PluginName preenchido)

### 2.1 VisceralCombat — tipo real identificado: `VisceralCombat.Ragdolls.Classes.DismemberedLimbScaler`

| Método | A: calls (c/f) | B: calls (c/f) | self ms/f A→B | avg self/call |
|---|---|---|---|---|
| DismemberedLimbScaler.Update | 242 627 (161.0) | 328 390 (185.9) | 0.0445 → 0.0462 | 0.2–0.3 µs |
| DismemberedLimbScaler.LateUpdate | 242 627 (161.0) | 328 403 (185.9) | 0.0305 → 0.0289 | 0.2 µs |
| PuppetMaster.LateUpdate (RootMotion.Dynamics) | 382 (0.25) | — | 0.0128 → 0 | 50.6 µs |
| Muscle.Update (RootMotion.Dynamics) | 7 280 (4.83) | — | 0.0088 → 0 | 1.8 µs |
| LivingDismembermentController.Update | 170 (0.11) | — | 0.0034 → 0 | 30.2 µs |
| PuppetMaster.FixedUpdate | 455 (0.30) | — | 0.0024 → 0 | 8.0 µs |

- **161 → 186 instâncias vivas** de `DismemberedLimbScaler` (MonoBehaviour real ⇒ c/f = instâncias). Cada uma escreve `transform.localScale` **3×/frame** (Update + LateUpdate + OnAnimatorMove — o 3º invisível ao profiler ⇒ custo real ≈ medido × 1,5 ≈ **0.11–0.19 ms/f** hoje).
- Fonte no repo: `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Classes/RagdollHelperClass.cs:561` (classe) e `VisceralCombat.Combined.Patches/KillPatch.cs:267,359` (criação). Mecanismo: em cada desmembramento, `AddComponent<DismemberedLimbScaler>()` em **TODO transform filho** do membro (`GetComponentsInChildren<Transform>(true)`) + transforms de rigidbody do ragdoll — um membro adiciona vários componentes. **Nenhum `Destroy()` do componente existe no código** (grep: só AddComponent/GetComponent). Vive enquanto o corpo existir.
- PuppetMaster/Muscle: transiente — ativo só nos segundos 4–11 de modded-A (1 puppet, ~25 muscles), desativado após a animação de morte (`DisableLiveActiveRagdoll`). Caro por call (FixedUpdate incl máx 16.8 ms), mas de vida curta.

### 2.2 VolumetricBloodFX (assembly vendorado no VisceralCombat; namespace vazio)

| Método | A: c/f | B: c/f (início→fim) | self ms/f A→B | avg/call |
|---|---|---|---|---|
| BFX_DecalSettings.Update | 20.0 | 21 → 32 | 0.0209 → 0.0283 | 1.0 µs |
| BFX_ShaderProperies.Update | 20.0 | 21 → 32 | 0.0205 → 0.0272 | 0.9–1.0 µs |
| BFX_ManualAnimationUpdate.Update | 23.0 | 24 → 35 | 0.0081 → 0.0105 | 0.3–0.4 µs |

- São MonoBehaviours de decal de sangue (asset "Volumetric Blood FX"), ~1 trio por decal ⇒ **~20 decals em A, 21→32 em B**. Fonte: `mods/VisceralCombat/modded/VolumetricBloodFX/*.cs`. Após a animação terminar, o componente **continua enabled**: `BFX_ShaderProperies` seta `canUpdate=false` (Update vira no-op chamado todo frame) e `BFX_ManualAnimationUpdate` continua fazendo `GetPropertyBlock/SetPropertyBlock` até `TimeLimit` e depois early-return por frame. Instanciados em `KillPatch.cs` (Instantiate); **sem pooling/Destroy** dos objetos de decal no código do mod (único Destroy é do `limbSquirter`).

### 2.3 SAIN — padrão N×M×7

| Método (classe comum, não MonoBehaviour) | A: c/f | B: c/f | self ms/f A→B |
|---|---|---|---|
| EnemyPartDataClass.Update | 257.9 | 86.1 | 0.0643 → 0.0218 |
| EnemyPartsClass.Update | 36.9 | 12.3 | 0.0362 → 0.0130 |
| PlayerDataExtensions.Update (4 overloads somados) | 54.0 | 37.2 | 0.0895 → 0.0580 |
| RaycastResult.Update | 80.0 | 27.1 | 0.0104 → 0.0043 |
| SeekCoverAction.Update | 1.94 | — | 0.0172 |

Decomposição exata do N×M (confirmada na fonte `mods/SAIN/modded/SAIN/Classes/Bot/EnemyClasses/Vision/EnemyPartsClass.cs`):
- `EnemyPartsClass.Update` roda 1×/frame por **par (bot SAIN, inimigo conhecido)**; internamente chama `part.Update()` para **7 partes do corpo** (`PartsArray`). Razão medida EnemyPartData/EnemyParts = 308/44 = 245/35 = 98/14 = **7.000 exato** nas duas capturas.
- Bots ativos no SAIN (overload de PlayerDataExtensions ÷ 4): A = 15→13; B = 11→8. Pares/bot ≈ 44/15 ≈ 2.9 inimigos por bot.
- `EnemyPartDataClass.Update` é barato por call (0.25 µs — 2 lookups de dict + comparações de timestamp; fonte `EnemyPartDataClass.cs:40`), o custo vem da **frequência**: bots × inimigos × 7 × 60 fps = 15 480 calls/s em A.
- Não é crescimento: série declina com mortes (308→245 em A, 98→77 em B).

### 2.4 TRL-ImmersiveCombatMedicine — "onde estão os 115 ms?"

Os 115.6 ms (modded-B, 30 s) são o TOTAL do mod, espalhado por 8 superfícies; nenhum método sozinho custa isso:

| Método | B: selfTot (ms/30s) | self ms/f | maxIncl 1 call |
|---|---|---|---|
| TraumaEngine.Update | 46.6 | 0.0264 | 1.07 (1.34 em A) |
| TRLImmersiveCombatMedicinePlugin.Update | 35.1 | 0.0199 | 0.09 |
| BandAidController.Update | 26.5 | 0.0150 | 0.32 (3.11 em A) |
| TraumaLegsConsumer + 3 consumers | 7.3 | 0.0041 | ≤0.04 |

Mecanismo (fonte `mods/TRL-ImmersiveCombatMedicine/modded-V3(review)/`): `TraumaEngine.Update` faz guards + `ConsolidateDirty()` todo frame e `Reconcile()` num polling de 1–4 Hz — o max de 1.07–1.34 ms é o tick de reconcile, não o caminho comum (p99Incl 0.41 ms). `BandAidController.Update` roda todo frame `CheckInit()` + `TickPendingConsumes()` + `EnsureMedicInteractables()` (varre players) + `UpdateNativePrompt()` (scan de distância) — o max de 3.11 ms em A é compatível com EnsureMedicInteractables num frame de spawn. Plugin.Update: lookups + 2 Lerp + escrita em `AudioListener.volume` por frame. Total do mod: **0.065–0.077 ms/f contínuos** — médio baixo, sem cauda relevante.

### 2.5 Outros mods 1-instância (custo por frame puro)

| Mod.Método (1 c/f) | self µs/frame A → B | Caracterização |
|---|---|---|
| Fika BotStateManager.Update | 356.5 → 274.0 | ver §4 |
| ORBIT MovementSystem.Update | 51.1 → 54.1 | itera `liveAgents` 1×/frame: re-ancora `BotMover` (4 writes/bot), rescue de ilha, jobs de movimento (fonte `mods/ORBIT/modded/Orbit/Systems/MovementSystem.cs:44`) |
| TRL-SpeakFromTarkov MicrophoneCapturer.Update | 33.4 → 30.5 | polling contínuo do mic: `Microphone.IsRecording` (nativo) + `PollMicrophoneData()` + **9 leituras de ConfigEntry.Value re-aplicadas ao filtro TODO frame** (fonte `mods/TRL-SpeakFromTarkov/modded/Audio/MicrophoneCapturer.cs:211`) |
| Manimal-Icebreaker RenderEnvProbe.Update | 38.6 → 36.7 | 1 instância, todo frame; **sem fonte no repo** — nome sugere re-render de reflection probe |
| SPT-QuestMap RaidQuestRuntime.Update | 31.6 → 28.1 | 1 instância; p99Incl 0.41 ms (tick periódico interno); **sem fonte no repo** |
| Fontaine-FOVFix Plugin.Update | 8.8 → 8.0 | 1 instância |

ORBIT tem perfil hitchy, não contínuo: `OrbitManager.Update` maxIncl 7.42 ms, `GotoObjectiveStrategy.Update` p99Incl 6.55 ms/maxIncl 7.33 (B), `LootContainerAction.Update` maxIncl 4.56 — spikes de path/loot em frames isolados. Nos worst-frames de modded-B: GotoObjectiveStrategy aparece em 5 dos 71 (10.1 ms somados), LootContainerAction em 2 (8.2 ms).

## 3. Crescimento (timeline.csv, calls/bucket ÷ frames/bucket, resolução 1 s)

| Método | modded-A (30 s) | modded-B (30 s) | Veredito |
|---|---|---|---|
| DismemberedLimbScaler.Update | **161 constante** | **180 → 186 → 193 (+13, só sobe)** | **GROW/LIFE monotônico** — nunca decresce; A→B (64 s depois) foi 161→193 |
| BFX_DecalSettings/ShaderProperies.Update | 20 constante | **21 → 30 → 32 (+11, só sobe)** | **GROW/LIFE monotônico** |
| BFX_ManualAnimationUpdate.Update | 23 constante | 24 → 33 → 35 | idem |
| EnemyPartDataClass.Update (SAIN) | 308 → 245 (decresce em degraus) | 98 → 77 | não é acúmulo — segue mortes de inimigos |
| PlayerDataExtensions (SAIN, ÷4 = bots) | 15 → 13 | 11 → 8 | decresce (mortes) |
| Muscle/PuppetMaster (Visceral) | 0 → ativo s4–11 → 0 | 0 | transiente correto |

Padrão dos dois GROW: platôs com degraus PARA CIMA a cada evento (kill/desmembramento/sangue), sem nunca descer — assinatura de objeto criado por evento e nunca destruído. Extrapolação linear simples: se a taxa observada entre capturas (~+25 scalers/min em combate) persistir numa raid de 40+ min com combate, chega a casa de milhares de componentes (custo linear: ~0.7 µs/frame por scaler contando as 3 superfícies, mais pressão no Animator/Transform system fora do managed).

## 4. Fika BotStateManager — normalização por bot vs vanilla

População (proxy `Player.FixedUpdate ÷ FikaHostWorld.FixedUpdate` = steps físicos/frame):

| Captura | Players vivos | BotOwners | BSM self ms/f | BSM **incl** ms/f | incl µs/bot | self µs/bot |
|---|---|---|---|---|---|---|
| vanilla-A | 25.5 | 24.5 | 0.2206 | 0.9419 | 38.4 | 9.0 |
| vanilla-B | 32.6 | 31.6 | 0.2400 | 0.8252 | 26.1 | 7.6 |
| modded-A | 24.0 | 23.0 | 0.3565 | 0.5919 | 25.7 | 15.5 |
| modded-B | 24.0 | 23.0 | 0.2740 | 0.4688 | 20.4 | 11.9 |

- **Correção importante ao proxy do CONTEXT.md**: o modded tinha ~23 BotOwners vivos (não 8–14). `AICoreLayerClass` 8–12 c/f mede bots com IA ATIVA (limiter/ORBIT assume bots), não população.
- Fonte (`references/fika-plugin/Fika.Core/Main/Components/BotStateManager.cs:68`): Update = `_controller.Update?.Invoke()` (delegate do HostGameController inteiro) + `_botsController.method_0()` + `SendBatchStates()` a SendRate Hz (600 calls/30s = 20 Hz nas 4 capturas). No modded (UpdateOnly) o self engole esses callees; no vanilla eles foram instrumentados à parte — por isso o self "sobe" 0.22→0.36.
- **Pelo inclusive (comparável entre modos), o modded é MAIS BARATO por bot** (20–26 vs 26–38 µs/bot). O aparente delta de self 0.27–0.36 vs 0.22–0.24 é artefato da assimetria de instrumentação. Fika não é regressão aqui.

## 5. Linhas com namespace vazio e calls altíssimos (30–56k em 30 s)

Identificação via `references/eft-decompiled/types-index.json` (modded-B, main thread):

| Tipo (ns vazio) | c/f modded | c/f vanilla-A/B | Identidade (alias SPT 4.1) | self ms/f modded-B |
|---|---|---|---|---|
| GClass2091.Update | 51.75 | 36.5 / 44.6 | **EFT.SpringMagazineVisual** (visual de mola de carregador) | 0.0229 |
| MagazineInHandsVisualController.Update (ns EFT) | 52.75 | 37.5 / 45.6 | idem função — 1 por carregador em mãos/mundo | 0.0194 |
| PlayerAIDataClass.LateUpdate | 48.43 | 47.5 / 65.8 | **AIData** (dados de IA por player) | 0.0108 |
| GClass1679.LateUpdate | 48.43 | 47.5 / 65.8 | **EFT.FoliageIntersectionSystem** (folhagem/intersecção, casado 1:1 com AIData) | 0.0056 |
| BaseSoundPlayer.Update / WeaponSoundPlayer.Update | 42.9 / 40.9 | 35.5 / 43.6 | players de som por arma | 0.0037 / 0.0126 |
| BFX_ManualAnimationUpdate/DecalSettings/ShaderProperies | 32 / 29 / 28.7 | — (não existem no vanilla) | **VolumetricBloodFX (mod)** — únicos ns-vazio exclusivos do modded | 0.066 (soma) |
| MetaXRAcousticGeometry.LateUpdate (asm Meta.XR.Audio) | 28.0 | 28.0 / 28.0 | geometria acústica Meta XR (idêntico nos 2 envs) | 0.0201 |
| PerfectCullingCrossSceneGroup.Update | 27.0 | 27.0 / 27.0 | culling estático do mapa (idêntico) | 0.0179 |
| AmmoPoolObject.Update (ns EFT.AssetsManager) | 20.9 | 0.13 / 0.18 | **pool de munição — 20.9 c/f no modded vs ~0 no vanilla** | 0.0012 |

Leitura: quase tudo é infraestrutura do jogo com contagens comparáveis ao vanilla. Exceções modded: (a) BFX_* é mod; (b) contagem de carregadores/armas ~53 vs 37–46 do vanilla com MENOS players — mais armas no mundo por loadouts TRL; (c) `AmmoPoolObject` 20.9 c/f vs ~0.15 no vanilla — ~21 objetos de pool de munição ativos permanentemente no modded (efeito colateral provável de mods de balística/efeitos; custo self hoje ínfimo, 0.0012 ms/f, mas é census divergente).

## 6. Fonte no repo (verificado)

| Mod | Fonte? | Onde |
|---|---|---|
| VisceralCombat | SIM | `mods/VisceralCombat/modded/VisceralCombat/` |
| VolumetricBloodFX | SIM (vendorado) | `mods/VisceralCombat/modded/VolumetricBloodFX/` |
| SAIN | SIM | `mods/SAIN/modded/SAIN/` |
| ORBIT | SIM | `mods/ORBIT/modded/Orbit/` |
| TRL-ImmersiveCombatMedicine | SIM | `mods/TRL-ImmersiveCombatMedicine/modded-V3(review)/` |
| TRL-SpeakFromTarkov | SIM | `mods/TRL-SpeakFromTarkov/modded/` |
| Fika.Core | SIM (referência) | `references/fika-plugin/` |
| Manimal-Icebreaker | **NÃO** | — (só DLL em plugins) |
| SPT-QuestMap | **NÃO** | — |
| Janky-HollywoodFX/Graphics, FOVFix, SPTRecoilRework | NÃO (irrelevantes: ≤0.008 ms/f) | — |

## 7. Perspectiva: quanto o UpdateOnly explica por atribuição direta?

| Métrica | modded-A | modded-B |
|---|---|---|
| Frametime médio | 19.918 ms | 16.969 ms |
| Managed profiled médio | 6.946 ms | 5.422 ms |
| Soma self de TODOS os mods (main) | **1.137 ms/f** | **0.896 ms/f** |
| — sem Fika, profiler e mods compartilhados c/ vanilla | **0.746 ms/f** | **0.593 ms/f** |
| Delta vs vanilla-A (14.411 ms, ~25 players) | +5.507 ms | +2.558 ms |
| Delta vs vanilla-B (16.993 ms, ~33 players) | +2.925 ms | −0.024 ms |
| **% do delta explicado por atribuição direta (stack TRL)** | **13.5% (vs vA) / 25.5% (vs vB)** | **23.2% (vs vA) / n/a (vs vB≈0)** |

- No vanilla, a superfície Update dos mods (Fika+profiler+devtools) soma 0.276–0.299 ms/f — o modded adiciona ~0.6–0.75 ms/f de Update de mods novos. Isso é **~4% do frametime** do modded e **~1/8 a 1/4 do delta médio**.
- O resto do delta NÃO está nas superfícies Update dos mods: está (i) nos métodos do jogo inflados no modded (`SyncTransformsClass.Update` +1.3–1.9 ms/f — maior delta isolado; `JobScheduler`, `AsyncWorker`, `NonWavesSpawnScenario` nos hitches), (ii) no custo invisível de Harmony patches dos ~100 mods (harmony-patches.csv vazio no UpdateOnly), e (iii) fora do managed (render/física/GC). Nos deep worst-frames, os únicos métodos de mod recorrentes são BotStateManager (todo frame, pequeno) e spikes ocasionais do ORBIT (GotoObjectiveStrategy/LootContainerAction, 2–5 de 71 frames).
- Nota de método: os worst-frames retidos são 100 (modded-A) e 71 (modded-B) — acima do WorstFrameCount=20 configurado (fato 10 do CONTEXT confirmado e quantificado).

## 8. Próximos passos de medição sugeridos

1. Recapturar o modded em **ModAttribution** (mesmo modo do vanilla) para expor Harmony patches e fechar o gap de atribuição (~75–85% do delta hoje sem dono direto).
2. Raid longa (15–30 min) com kills e captura periódica: confirmar taxa de crescimento de `DismemberedLimbScaler` (161→193 em ~1 min de combate) e BFX_* e correlacionar com degradação de frametime/RAM.
3. Investigar `SyncTransformsClass.Update` (delta +1.3–1.9 ms/f) na dimensão própria — candidato a efeito indireto dos ragdolls/rigidbodies do Visceral (rb.isKinematic, joints destruídos) e da contagem extra de armas no mundo.
4. `AmmoPoolObject` 20.9 c/f vs ~0.15 no vanilla: identificar qual mod mantém 21 pools de munição ativos.
5. ORBIT: perfil hitchy (maxIncl 7.3–7.4 ms) — instrumentação dedicada nos jobs de path/loot para dimensionar contribuição aos p99.

## Histórico

| Data | Autor | Descrição |
|---|---|---|
| 2026-08-22 | Guilherme | docs(perf): add DynamicSpawn audit report + ICM/Stances optimization handoffs |
