---
title: "Anexo profiling 2026-08-22 — coverage-gaps"
date: 2026-08-22
status: 🟢 Vivo
authors: Claude (investigação de performance 2026-08-22, análise dimensional)
---

# Coverage gaps — o que estas capturas NÃO conseguem ver

Dimensão: inventário de mods × visibilidade, baseline Harmony vanilla, gap de instrumentação do modded, overhead do profiler, limitações estruturais e desenho da próxima rodada.
Capturas: vanilla-A/B (SPT_2, ModAttribution), modded-A/B (SPT, UpdateOnly). Todos os números calculados via Python sobre os CSVs brutos (script: `analysis/coverage_calc.py`, saída intermediária `analysis/coverage_calc_out.json`).

---

## 0. Metodologia

- Inventário: `D:\SPT\BepInEx\plugins` (top-level), DLLs enumerados recursivamente por entrada. Critério de visibilidade: algum assembly da entrada aparece em `mod-summary.csv` (qualquer thread) de modded-A **ou** modded-B, ou em `methods.csv` com PluginName preenchido.
- Baseline Harmony: `harmony-patches.csv` das duas capturas vanilla, linhas com Calls>0 ("executados"), normalizado por frame (2082 / 1766 frames).
- Gap Update-family: conjunto (Assembly, DeclaringType, Method) com Method ∈ {Update, LateUpdate, FixedUpdate} na main thread; união vanilla-A∪B comparada com união modded-A∪B.
- Verificação de mecanismo: grep binário nos metadados .NET dos DLLs implantados (strings de typeref/atributos ficam em UTF-8 no assembly) + fontes de mods no repo.
- Overhead do profiler: linhas de `methods.csv` com assembly/plugin contendo "profiler".

---

## 1. Inventário de plugins × visibilidade na captura modded

**Contagem: 107 entradas em `D:\SPT\BepInEx\plugins` → 39 visíveis (40 assemblies observados), 66 invisíveis (61,7%), 2 pastas sem DLL** (`DragonDenDevTool` e `WTT-HeadVoiceSelector` — os DLLs correspondentes estão na raiz e foram contados como entradas próprias).

"Visível" significa apenas: **ao menos 1 método da superfície Update/LateUpdate/FixedUpdate observado**. Não significa custo medido por inteiro — patches Harmony, coroutines, OnGUI, handlers de evento e async continuations de mods *visíveis* também estão fora do radar do UpdateOnly (ex.: o ICM aparece com 115,6 ms self em modded-B via MonoBehaviour **e ao mesmo tempo** tem um patch invisível em `Player.LateUpdate` — ver §3).

### 1.1 Visíveis (39 entradas → assemblies, self total main thread A+B somados, em ms/30s×2)

| Entrada | Assembly observado | Self A+B (ms) | Métodos obs. |
|---|---|---:|---:|
| Fika | Fika.Core | 1092,0 | 14 |
| SAIN | SAIN | 532,0 | 25 |
| ORBIT | ORBIT | 335,2 | 19 |
| VisceralCombat | VisceralCombat + VolumetricBloodFX | 288,6 + 191,3 | 7 + 3 |
| TRL-ImmersiveCombatMedicine | TRLImmersiveCombatMedicine | 231,7 | 8 |
| TRL-SpeakFromTarkov | TRL-SpeakFromTarkov | 138,6 | 10 |
| SPTQuestMap | SPTQuestMap.Client | 125,9 | 5 |
| ManimalIcebreaker | ManimalIcebreakerClient | 124,1 | 2 |
| TRL-StancesAndMobility | TRL-StancesAndMobility | 34,4 | 4 |
| FOVFix.dll | FOVFix | 27,5 | 1 |
| SPTRecoilRework.dll | SPTRecoilRework | 26,8 | 3 |
| HollywoodFX | HollywoodFX | 24,6 | 10 |
| SPTProfiler | SPTProfiler | 22,0 | 1 |
| HollywoodGraphics | HollywoodGraphics | 15,9 | 3 |
| TarkovRedLine | RedLineRestart | 11,1 | 1 |
| TRL-ImmersiveOverlays | TRL-ImmersiveOverlays | 10,9 | 1 |
| tarkin | doordash | 8,0 | 1 |
| DebugPlus.dll | DebugPlus | 5,2 | 1 |
| DPX-QuickThrow | DPX-QuickThrow | 4,6 | 1 |
| Terkoiz.Freecam.dll | Terkoiz.Freecam | 4,5 | 2 |
| DynamicMaps | DynamicMaps | 4,4 | 1 |
| Tyfon.UIFixes.dll | Tyfon.UIFixes | 3,8 | 4 |
| ATAKRig | ATAKRig | 3,7 | 2 |
| Drexira.DragonDenDevTool.dll | Drexira.DragonDenDevTool | 3,6 | 2 |
| DrakiaXYZ-QuestTracker | DrakiaXYZ-QuestTracker | 3,6 | 2 |
| spt | ConfigurationManager | 3,5 | 2 |
| HandsAreNotBusy.dll | HandsAreNotBusy | 3,1 | 1 |
| Blackout | Blackout | 2,7 | 1 |
| C11-TN4-Client | C11-TN4-Client | 2,4 | 2 |
| ContinuousLoadAmmo.dll | ContinuousLoadAmmo | 2,0 | 1 |
| WTT-ClientCommonLib | WTT-ClientCommonLib | 1,9 | 2 |
| LoadAmmoAnim | LoadAmmoAnimClient | 1,9 | 1 |
| TRL-DynamicSpawn.dll | TRL-DynamicSpawn | 1,6 | 3 |
| WTT-PackNStrap | WTT-PackNStrap | 1,0 | 1 |
| DrakiaXYZ-BotDebug.dll | DrakiaXYZ-BotDebug | 0,7 | 1 |
| 7Bpencil.WeaponCamoAndStickers | 7Bpencil.WeaponCamoAndStickers | 0,7 | 2 |
| UnderFire.dll | UnderFire | 0,6 | 1 |
| Wara-ModdingStatsHelper.dll | Wara-ModdingStatsHelper | 0,5 | 1 |
| BorkelRNVG | BorkelRNVG | 0,1 | 1 |

Soma da atribuição por mod visível: modded-A 1,137 ms/frame, modded-B 0,896 ms/frame (main thread, só superfície Update) — contra managed médio de 6,95 / 5,42 ms/f. **A atribuição por mod cobre ~16% do managed medido, que por sua vez subconta o managed real (§3).**

### 1.2 Invisíveis (66 entradas — nenhum método observado em nenhuma das 2 capturas modded)

Classificação heurística por função conhecida (**não medida** — serve para priorizar, não para absolver):

**Alta relevância in-raid (rodam patch/evento durante a raid; custo real possível, 100% fora do radar):**

| Entrada | DLL(s) | Por que importa |
|---|---|---|
| DrakiaXYZ-BigBrain.dll | DrakiaXYZ-BigBrain | Framework de layers de IA — **toda a lógica do SAIN roda por dentro do patch dele em `AICoreAgentClass.Update`** (§3) |
| DrakiaXYZ-Waypoints | DrakiaXYZ-Waypoints | Navmesh custom por bot (patch em pathfinding) |
| MoreBotsAPI | MoreBotsPlugin | Referencia `AICoreAgentClass`/`AICoreControllerClass` nos metadados |
| CustomClasses | CustomClasses-Client | Perks por-frame via patches em métodos de Player (mod próprio TRL) |
| SkillsExtended | SkillsExtended(+Common) | Skills com tracking in-raid |
| hazelify.StanceSync.dll | hazelify.StanceSync | Patcha `PlayerCameraController.LateUpdate` (fonte no repo: `mods/StanceSync/modded/Patches/PlayerCameraControllerPatchPrefix.cs:14`) |
| TRL-Fixes.dll | TRL-Fixes | Coleção de patches TRL |
| TRL-FikaSync-ClimbableLadders.dll | TRL-FikaSync-ClimbableLadders | Sync de escadas via patches/eventos Fika |
| SPTVRAMCleaner.dll | SPTVRAMCleaner | Limpeza periódica de VRAM — candidato clássico a **hitch periódico** (Resources.UnloadUnusedAssets/GC), invisível se disparado por timer/coroutine |
| DynamicExternalResolution.dll | DynamicExternalResolution | Escala dinâmica de resolução — decisão por frame, possivelmente via evento de câmera |
| BackdoorBanditClient | DoorBreach | Dano em portas via eventos de hit |
| CSGas | CSGasClient | Efeitos de área por evento |
| tarkin-ladders | tarkin.ladders.bep(+shared) | Patch de escadas (contém string LateUpdate nos metadados) |
| WTT-ContentBackportClient | WTT-ContentBackport* | Conteúdo backportado (patches de item/arma) |
| CWX | CWX_MegaMod | Mega-pack de tweaks via patches |
| Gaylatea-UseLooseLoot.dll / UseItemsFromAnywhere.dll / MergeConsumables(+Fika) / ozen-MagCheckInterrupt(.Net) / ozen-Foldables / BringBackConcussion.dll / inory-agonysfx.dll / BlackDiv / Manimal.WatchAnims / PlayerEncumbranceBar / Softwyx.CareerLog / tacticaltoaster-untargohome / DrakiaXYZ-SearchOpenContainers.dll / UnderFire→(visível) | — | Eventos in-raid diversos (uso de item, som de dor, HUD, log de carreira) |

**Baixa relevância in-raid (menu/hideout/flea/load-time — custo em raid improvável, mas não medido):**
acidphantasm-moretagcolours, acidphantasm-previewsizer, acidphantasm-simpleworkoutqte, AirFilterWarning, AnanasCharles-AttachmentCompatHighlight, AnanasCharles-StatDeltaPreview, AutoGym, ClearPrepareScreen, com.swiftxp.showmethemoney(+quicksell), DrakiaXYZ-EquipFromWeaponRack, DrakiaXYZ-GildedKeyStorage-Client, DrakiaXYZ-TaskListFixes, IcyClawz.CustomInteractions/ItemContextMenuExt/MunitionsExpert, JBOBYH, Kaeno-TraderScrolling, Kat.BetterAmmoLoadingList, LetMeRightClick, maschine-WeaponBuilderSearch, MoreCheckmarks, MoxoPixel-HideoutShootout, MoxoPixel.MenuOverhaul, RaiRai.ColorConverterAPI, redlaser42.UI_Refresh, ReduceFakeInteriorShadow, RefinedFleaOfferList, s8_SPT_LoadBundleEvenFaster, s8_SPT_PatchCRC32, Tyfon.AutoDeposit/DebugTooltip/HideoutInProgress/UIFixes.Net/WeaponCustomizer, UnflashbangHideout, WTT-HeadVoiceSelector.dll.

Nota: MoreCheckmarks e Tyfon.WeaponCustomizer são conhecidos por trabalho pesado em eventos de inventário — em raid isso dispara ao lootear. Invisível aqui.

---

## 2. Buraco Harmony: baseline vanilla como régua

`harmony-patches.csv` **modded = 0 linhas** (nem os patches *registrados* são inventariados no modo UpdateOnly). No vanilla (ModAttribution):

| Métrica | vanilla-A | vanilla-B |
|---|---:|---:|
| Patches registrados | 237 | 236 |
| Owners distintos registrados | 103 | — |
| Plugins com patch registrado | 8 (Fika.Core 121, SPT.Custom 45, SPT.Singleplayer 39, SPT.Core 19, SPT.Debugging 4, Freecam 3, DragonDenDevTool 1, +5 s/ plugin) | idem |
| Patches executados na captura | 35 | 32 |
| Self total executados | 6,83 ms/30s = **3,28 µs/frame** | 5,48 ms/30s = **3,10 µs/frame** |
| Inclusive total | 9,35 ms = 4,49 µs/f | 8,41 ms = 4,76 µs/f |
| **Fika (todos os patches)** | 66 calls, 1,45 ms self = **0,70 µs/frame** | 196 calls, 0,37 ms = **0,21 µs/frame** |
| Maior owner | Freecam FallDamagePatch (prefix em `ActiveHealthController.HandleFall`, 25,3 calls/f) = 2,10 µs/f | idem 23,9 calls/f = 2,33 µs/f |

Por plugin executado: média 0,55 µs/f, mediana 0,20 µs/f, máx 2,10-2,33 µs/f (Freecam).

**Extrapolação ILUSTRATIVA (não é medição):** se os ~100 plugins do modded tivessem handlers tão baratos quanto os do stack vanilla:
- 100 × média (0,55 µs/f) ≈ **0,06 ms/frame**
- 100 × pior plugin vanilla (2,1-2,3 µs/f) ≈ **0,21-0,23 ms/frame**

Ou seja: **overhead de handler bem-comportado não explica o delta do modded** (que é da ordem de +3-5 ms/f no médio e +80-160 ms nos hitches). O que a régua NÃO limita:
1. **Um único patch mal escrito em método quente** — o FallDamagePatch mostra o formato: 25 calls/frame porque o alvo roda por player por frame. Um prefix com alocação/lookup pesado nesse mesmo alvo custaria 100-1000× mais. O review CR-01 do ICM documentou exatamente esse padrão no postfix de `Player.LateUpdate` (IsPartDestroyed×2 + lookups duplos por chave string, por player, por frame).
2. **Trabalho que o patch dispara dentro do código do jogo** — SAIN/BigBrain *substituem* a lógica de IA: o custo aparece como tempo de método do jogo (ou nem aparece, §3), nunca como self do patch.
3. Cauda: `StandartBotBrain.Activate` prefix com maxIncl 0,97-0,98 ms mostra que até patch raro pode dar spike de ~1 ms.

---

## 3. Gap de instrumentação Update-family no modded — e o mecanismo identificado

### 3.1 O que falta

Linhas em `methods.csv`: vanilla 975/926 (900/846 main) vs modded **354/352** (351/338 main). União Update-family: vanilla 241, modded 346 (o modded tem mais tipos porque os MonoBehaviours dos mods entram).

**27 métodos Update-family do jogo presentes no vanilla e AUSENTES nas duas capturas modded** — soma 2,163 ms/frame self (3,237 incl) a população vanilla. Os 6 com custo relevante:

| Método ausente no modded | Vanilla self ms/f | incl ms/f | calls/f | Mod(s) implantado(s) no modded que referencia(m) o tipo* |
|---|---:|---:|---:|---|
| `Player.LateUpdate` | 1,742 (A) / 1,559 (B) | 1,969 / 1,814 | 25,45 / 32,56 | **TRLImmersiveCombatMedicine.dll** (blob de atributo `[EFT.Player…]+"LateUpdate"` confirmado no binário implantado) |
| `AICoreAgentClass.Update` | 0,238 | 0,541 | 24,42 | **DrakiaXYZ-BigBrain.dll**, ManimalIcebreakerClient, MoreBotsPlugin |
| `AICoreStrategyAbstractClass.Update` | 0,084 | 0,186 | 22,85 | BigBrain, MoreBotsPlugin, **ORBIT**, **SAIN** |
| `LaserBeam.LateUpdate` | 0,057 | 0,057 | 2,90 | **BorkelRNVG.dll** |
| `ToDController.Update` | 0,013 | 0,013 | 1,00 | HollywoodGraphics, ManimalIcebreakerClient |
| `AICoreControllerClass.Update` | 0,010 | 0,413 | 1,00 | MoreBotsPlugin, ORBIT |

*Referência de tipo nos metadados .NET ≠ prova de patch naquele método exato — exceto ICM, onde o blob do atributo HarmonyPatch foi lido byte a byte: `REFT.Player, Assembly-CSharp…\nLateUpdate` (o `\n` é o length-byte 0x0A de "LateUpdate").

Os 21 restantes são baratos (≤0,005 ms/f) e a ausência deles pode ser **comportamental** (nunca chamados na janela de 30s do modded — ex.: `ItemView.Update` = inventário aberto, `BotGlobals*.Update` calls/f 0,01), não instrumental.

Contraprova de que players/bots existiam no modded: `PlayerAIDataClass.LateUpdate` (79.104 / 85.584 calls) e `GamePlayerOwner.LateUpdate` estão lá — **irmãos de família LateUpdate visíveis, só os alvos de patch sumiram**.

### 3.2 Mecanismo (causa mais provável) — detour Harmony mata a instrumentação Mono

Cadeia de evidência:
1. O config do profiler declara: instrumentação é **process-lifetime**, decidida no startup ("cannot add methods that Mono did not instrument at startup") — os callbacks enter/leave são inseridos no código nativo do método na hora do JIT.
2. Quando o Harmony patcha um método, ele instala um **detour** (salto) no início do código nativo original para um método dinâmico recompilado — os callbacks do profiler, que vêm depois do ponto do salto, **nunca mais executam**; o método some das estatísticas.
3. Todos os 6 ausentes "caros" são alvos clássicos de patch de mods presentes SÓ no modded (BigBrain patcheia `AICoreAgentClass.Update` — é o mecanismo central dele; ICM confirmado no binário; SAIN/ORBIT referenciam os AICore).
4. No vanilla, nenhum dos 237 patches registrados mira família Update — e lá tudo aparece.

Hipóteses alternativas **descartadas ou improváveis**:
- **Cap MaxMethods=50000:** descartado — modded observou 352-354 linhas; vanilla, com superfície muito maior, 926-975. Nenhum diagnóstico de drop (droppedEntries=0 nas 4).
- **JIT tardio / ordem de load:** improvável para métodos que rodam 25×/frame — se tivessem sido JITados depois do hook, seriam instrumentados normalmente (o hook é no JIT); se JITados antes do hook durante o boot, `Player.FixedUpdate` (visível no modded) teria sumido junto.
- Ressalva honesta (por isso "Forte" no mecanismo agregado, mas cada atribuição individual além do ICM é "Suspeita"): não decompilamos BigBrain/BorkelRNVG para ver o `[HarmonyPatch]` exato.

**Corolário importante para TODA a investigação:** no modo UpdateOnly, *qualquer* método Update-family que algum mod patcheia fica invisível — e mods patcheiam exatamente os métodos quentes. O buraco não é aleatório: é **correlacionado com os piores suspeitos**. O mesmo vale dentro do vanilla se algum patch mirasse Update-family (não é o caso).

### 3.3 Estimativa do managed subcontado no modded (extrapolação por população)

Custo por call no vanilla (incl, que é o que o modded veria já que os callees não são instrumentados lá):
- `Player.LateUpdate`: 0,0557-0,0774 ms/call (B/A)
- `AICoreAgentClass.Update`: 0,0222 ms/call; `AICoreStrategyAbstractClass.Update`: 0,0081 ms/call

Escalando pela população do modded (proxies: ~12,5-13,5 entidades em modded-A; ~8,3-9,3 em modded-B):

| Componente invisível | modded-A (ms/f) | modded-B (ms/f) |
|---|---:|---:|
| Player.LateUpdate | 0,70-1,04 | 0,46-0,72 |
| AICoreAgentClass.Update | ~0,28 | ~0,18 |
| AICoreStrategyAbstractClass.Update | ~0,10 | ~0,07 |
| **Total estimado (piso)** | **~1,1-1,4** | **~0,7-1,0** |

Isso é **~16-20% do managed medido** (6,95 / 5,42 ms/f) e ~5-7% do frame — **como PISO**, porque: (a) assume custo por call vanilla, mas no modded o `AICoreAgentClass.Update` executa a lógica SAIN/BigBrain inteira (muito mais cara que a IA vanilla); (b) não inclui o tempo dos handlers de prefix/postfix de nenhum patch; (c) não inclui os 66 mods invisíveis. Classificação: **Suspeita** (derivação, não medição).

---

## 4. Overhead do próprio profiler

| | vanilla-A | vanilla-B | modded-A | modded-B |
|---|---:|---:|---:|---:|
| Linhas SPTProfiler em methods.csv | 34 | 32 | 1 | 1 |
| Self total | 261,4 ms | 241,1 ms | 10,5 ms | 11,5 ms |
| **Self/frame** | **0,126 ms** | **0,137 ms** | **0,0069 ms** | **0,0065 ms** |
| Maior item | DrawCaptureBadge 0,076 ms/f (5486 calls, OnGUI) | DrawCaptureBadge 0,080 ms/f | Plugin.Update | Plugin.Update |

Leitura:
- No vanilla (ModAttribution) o profiler se auto-instrumenta por inteiro: ~0,13 ms/f, 60% disso é o **badge de captura desenhado via OnGUI** (2-3 chamadas OnGUI por frame).
- No modded (UpdateOnly) só `Plugin.Update` é visível (0,007 ms/f) — **mas o badge OnGUI roda do mesmo jeito, sem ser medido**. Ou seja: os dois ambientes pagam ~0,13 ms/f de UI do profiler no frametime; no vanilla isso entra no ManagedProfiledMs (inflando-o ~0,1 ms/f), no modded não.
- Overhead de callback de instrumentação: volume de calls instrumentadas por frame é comparável (vanilla 1794/1971, modded 1854/1564 calls/frame) → custo de enter/leave similar nos dois; a diferença de modo não cria assimetria relevante aqui. O vanilla paga a mais o timing de patches (35 executados) — desprezível.
- Distorção total: <1% do frame nos dois. Direção: infla levemente o vanilla → **o gap real modded-vanilla é ligeiramente MAIOR que o medido**. Não muda nenhuma conclusão. Evidência: Forte.

---

## 5. Outras limitações estruturais desta rodada

| # | Limitação | Números |
|---|---|---|
| L1 | **Sem dados de GC/alocação** — frames.csv só tem FrameMs/ManagedProfiledMs/TopSelf; nenhuma coluna de GC, alloc ou heap. Spikes de GC aparecem como "frame gordo sem managed correspondente", indistinguíveis de espera de engine/render | colunas: FrameNumber, TimestampSeconds, FrameMs, ManagedProfiledMs, ManualDeepCapture, DeepRetained, TopSelfMethod, TopSelfMs |
| L2 | **Sem GPU** — CPU frametime somente; managed cobre 32-40% do frame (vanilla-A 39,8%, vanilla-B 34,6%, modded-A 34,9%, modded-B 32,0%). Os outros ~60-65% (render thread, GPU wait, physics interno, código não instrumentado) são caixa-preta | — |
| L3 | **Capturas de 30s** (30,00/30,01/30,00/29,96s) — não capturam a degradação conhecida de raid longa (RAM 10→33GB, memória do projeto) nem acúmulo de instâncias além da janela (o crescimento Visceral 161→186 instâncias entre modded-A e B é o único vislumbre) | durações medidas |
| L4 | **Cenários não pareados** — vanilla com ~25-33 players+bots, modded com ~8-14. O vanilla rodou com ~2× mais bots e frametime melhor: os deltas do modded NÃO são população — mas qualquer comparação por-entidade exige normalização e mesmo ela é frágil (composição de bots, distância, LOS diferentes) | proxies: 25,45/32,56 vs ~12,5-13,5/~8,3-9,3 calls/f |
| L5 | **1 mapa só (Customs), 1 momento de raid** — sem spawn wave pesada capturada, sem interior denso, sem evento de boss | — |
| L6 | **Captura "triplicada"** — 205433 = 205500 = 205508 byte-idênticos (MD5 frames.csv `46a46f3ee4d06247cf596d45ae41397f` nos 3). Só existem 2 datasets modded reais | MD5 verificado |
| L7 | **modded-A estourou o teto de deep-frames**: 147 frames qualificantes (>25ms ou managed>10ms) vs MaxDeepFrames=100 → **47 frames ruins (32%) sem detalhe profundo**. vanilla-A 75/75, vanilla-B 35/38, modded-B 71/71 ok | contagem por frames.csv |
| L8 | **F10 (deep frame manual) nunca usado** — ManualDeepCapture=0 nas 4 capturas; nenhum frame profundo dirigido a evento (spawn, hitch percebido) | — |
| L9 | **harmony-patches.csv do modded 100% vazio** — nem inventário de patches registrados (o vanilla registra 237 mesmo sem executar). Não sabemos nem QUANTOS patches o stack modded instala | 0 linhas |
| L10 | **Sem contagem direta de bots** — população inferida por proxies de calls/frame; no modded nem Player.LateUpdate existe para contar (proxies secundários: AICoreLayerClass, SAIN PlayerDataExtensions) | — |

---

## 6. Desenho da PRÓXIMA rodada de captura

Objetivo: fechar os 3 buracos (atribuição por mod no modded, patches Harmony, pareamento) sem perder comparabilidade.

1. **Modded em ModAttribution** (mudança de 1 linha no `com.spt.runtimeprofiler.cfg` + restart do EFT — instrumentação é process-lifetime).
   - Ganho: mod-summary real dos 107 mods, harmony-patches.csv populado (inventário + timing dos executados), métodos de plugin com self/incl.
   - Custo/risco: overhead maior de instrumentação (mais métodos com enter/leave; no vanilla o modo custou ~0,13 ms/f de profiler + callbacks; com 107 plugins o volume de métodos instrumentados cresce muito — aceitar até ~1-2 ms/f de overhead e validar com `droppedEntries`/`lateFramePackets` no diagnostics). Risco de estouro de MaxMethods=50000 com 107 plugins: monitorar diagnostics; se estourar, subir MaxMethods (até 1M) ou rodar em 2 sessões com metade dos mods.
   - **Atenção: mesmo em ModAttribution o alvo patchado pode continuar invisível** (o detour não desfaz) — mas os *handlers* dos patches passam a ser medidos e atribuídos, que é o que falta.
2. **Pareamento de cenário**: mesma janela de raid (primeiros 5 min), mesmo mapa, e idealmente população comparável — usar o próprio proxy (`AICoreLayerClass.Update` calls/f) como critério de validade da comparação; registrar contagem de bots por fora (F12 debug ou log do spawner) para eliminar a inferência.
3. **Capturas dirigidas a evento**: iniciar captura (F8) ~10s antes de momento de spawn conhecido (metrônomo de 10s do pipeline TRL) e usar **F10** no instante de hitch percebido — hoje os worst-frames são só os retidos por threshold e modded-A perdeu 32% deles. Subir `MaxDeepFrames` para 300-500 e `WorstFrameCount` para 50 no modded.
4. **Duração**: manter 30s para as comparáveis + adicionar 1 captura longa (300-600s, `CaptureDurationSeconds` até 600) no modded para ver crescimento (instâncias Visceral por bucket, tendência de FrameMs ao longo de minutos — proxy da degradação de RAM). Aceitar que a longa não é comparável com as curtas.
5. **Par de controle para o buraco Harmony**: 1 captura modded-UpdateOnly + 1 modded-ModAttribution no mesmo cenário → a diferença de ManagedProfiledMs entre as duas mede na prática o "managed invisível" que hoje só estimamos (~1,1-1,4 ms/f piso).
6. **O que validar ao receber os dados**: harmony-patches.csv >0 linhas; presença de handlers de ICM/SAIN/BigBrain; droppedEntries=0; badge/overlay do profiler idêntico nos dois; população via proxy ±20%.
7. **Fora do escopo do profiler** (aceitar ou complementar com outra ferramenta): GC/alloc (usar Unity PlayerConnection/profiler nativo ou contadores de `GC.CollectionCount` logados por um mod mínimo), GPU (CapFrameX já em uso no projeto — casar timestamps), RAM (log periódico de WorkingSet).

---

## 7. Resumo dos achados desta dimensão

| ID | Achado | Evidência |
|---|---|---|
| CG-01 | 66 de 107 mods (61,7%) invisíveis na captura modded; atribuição por mod cobre só ~1,0-1,1 ms/f de 6,9-5,4 ms/f de managed | Forte |
| CG-02 | Mecanismo do buraco: detour Harmony elimina instrumentação dos alvos Update-family — os 6 métodos ausentes mais caros são todos alvos de patch de mods modded-only; ICM confirmado no binário | Forte (mecanismo) / Suspeita (atribuições individuais exceto ICM) |
| CG-03 | Managed do modded subconta ≥1,1-1,4 ms/f (modded-A) / 0,7-1,0 (modded-B) — piso, por extrapolação de população | Suspeita |
| CG-04 | Baseline Harmony vanilla: 3,1-3,3 µs/frame self (35 patches); Fika 0,2-0,7 µs/f; extrapolação ilustrativa p/ 100 mods bem-comportados: 0,06-0,23 ms/f → handler barato não explica o delta; o risco é patch ruim em método quente + trabalho disparado dentro do jogo | Forte (baseline) |
| CG-05 | Overhead do profiler ~0,13 ms/f nos dois ambientes (badge OnGUI), visível só no vanilla; distorção <1% do frame, direção conservadora | Forte |
| CG-06 | modded-A perdeu detalhe profundo de 47/147 frames ruins (cap MaxDeepFrames=100) | Forte |
| CG-07 | Estruturais: sem GC/GPU/RAM, 30s, não pareado (~2× bots no vanilla), 1 mapa, captura triplicada, harmony csv modded vazio, F10 não usado | Forte (factual) |

Arquivos-fonte desta análise: `analysis/coverage_calc.py`, `analysis/coverage_calc_out.json` (mesma pasta deste .md).

## Histórico

| Data | Autor | Descrição |
|---|---|---|
| 2026-08-22 | Guilherme | docs(perf): add DynamicSpawn audit report + ICM/Stances optimization handoffs |
