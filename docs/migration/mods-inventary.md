---
title: Inventário de mods para migração SPT 3.x → 4.0
date: 2026-05-02
status: 🔵 Em andamento
authors: Guilherme
---

# Inventário de mods — Migração SPT 3.x → 4.0

Catálogo dos mods atualmente em uso no `tarkov-spt-3.0` que precisam ser avaliados para portar/recriar no SPT 4.0.

> **Atenção:** SPT 4.0 tem arquitetura incompatível com 3.x. Este documento serve como ponto de partida para decidir o que migrar, refazer ou descartar.

## Convenções de preenchimento

Cada mod tem:

- **Origem:** mods criados pela equipe são prefixados com `🏠` no nome (ex: `🏠 IdleSprintFix`). Demais são da comunidade.
- **Tipo:** 🖥️ Client (C# / BepInEx) · 🌐 Server (TypeScript/JS) · 🔀 Misto (ambos) · 🔍 a classificar
- **Cenário:** ver enum "Cenários" abaixo (onde o mod atua: hideout, raid, geral, ambos)
- **Forge:** URL no [forge.sp-tarkov.com](https://forge.sp-tarkov.com/mods) (1 página por mod, lista todas as versões). Formato: `[id](url)` · `🔍` (a buscar) · `—` (confirmado ausente)
- **Repo 3.x:** URL do repositório fonte na versão SPT 3.x (último release pré-4.0). Aceita GitHub, GitLab ou similar. Formato: `[autor/repo](url)` · `🔍` (a buscar) · `—` (não há repo público)
- **Repo 4.0:** URL do repositório fonte na versão SPT 4.0+. Pode ser repo separado (autor manteve nomes diferentes), branch/tag específica, ou mesmo repo do 3.x se autor migrou no mesmo. Aceita GitHub, GitLab, etc. Formato: `[autor/repo](url)` · `🔍` · `—` (autor não publicou 4.0)
- **Status:** ver enum "Status disponíveis" abaixo. **Notas e aliases** (forks, autor, dependências) vão na coluna **Função**, não em outras colunas.
- **Prioridade:** ver enum "Prioridades" abaixo.

## Status disponíveis

- 🟡 **Avaliar** — ainda não decidido (default ao adicionar mod novo)
- 🟢 **Instalar** — versão 4.0 **já existe** publicamente; basta baixar e instalar (sem trabalho de código)
- ⬆️ **Evoluir p/ 4.0** — adaptar/refatorar código 3.x existente para 4.0 (envolve refactor e evolução, não criação do zero). Padrão para mods internos `🏠` que temos código.
- 🔧 **Desenvolver** — criar do zero no 4.0 (autor original não lançou; decidimos antecipar fazendo do nosso lado)
- 🟠 **Aguardar upstream** — autor original ainda não lançou 4.0; default quando 4.x = ❌. Pode virar `🔧 Desenvolver` se decidirmos antecipar.
- 🔴 **Bloqueado** — incompatibilidade arquitetural sem workaround conhecido
- ⚫ **Não incluir** — fora do escopo do projeto

### Fluxo de decisão

```
4.x existe publicamente?
├─ ✅ Sim                     → 🟢 Instalar
├─ ❌ Não, mas é mod interno  → ⬆️ Evoluir p/ 4.0 (temos o código 3.x)
└─ ❌ Não, é da comunidade    → 🟠 Aguardar upstream
                                  └─ Decidiu antecipar? → 🔧 Desenvolver
```

## Prioridades

- 🔥 **Crítica** — bloqueia outros mods ou é base do projeto (ex: UltraFika)
- 🔝 **Alta** — central pra experiência do projeto; sem ele a stack perde valor
- ➖ **Média** — útil mas substituível ou opcional
- 🔻 **Baixa** — nice-to-have, pouco impacto se ausente
- 🔍 — a definir (default)

## Cenários

Onde o mod atua dentro do jogo:

- 🏚️ **Hideout** — específico do hideout (workout, customization, geradores, racks)
- ⚔️ **Raid** — específico de gameplay em raid (IA, FOV, FX, NVG, weather, mapas, animações)
- 🔀 **Ambos** — afeta tanto hideout quanto raid (ex: inventário/stash usado em ambos, multiplayer base)
- 🌐 **Geral** — não-específico de hideout ou raid (UI global, traders, flea, perfil, conteúdo de itens, frameworks)
- 🔍 — a categorizar (default)

## Base — 🏠 UltraFika-Plugin

Mod fundamental do projeto. Habilita multiplayer no SPT e serve como base sobre a qual os demais mods rodam. **Migração prioritária zero** — sem ele, o restante do ecossistema não tem sentido.

| Item | Detalhe |
|---|---|
| **Origem** | 🏠 Interno (criado pela equipe) |
| **Tipo** | 🖥️ Client (C# / BepInEx) |
| **Cenário** | 🔀 Ambos (multiplayer afeta hideout + raid) |
| **Função** | Cliente multiplayer (fork de Fika) |
| **Forge** | — (não publicado) |
| **Repo 3.x** | — (não público — fork privado) |
| **Repo 4.0** | — (a desenvolver) |
| **Upstream** | [Project Fika](https://forge.sp-tarkov.com/mod/2326/project-fika) — base de onde foi forkado |
| **Prioridade** | 🔥 Crítica — primeiro mod a ser migrado |
| **Status** | ⬆️ Evoluir p/ 4.0 (adaptar código 3.x existente para 4.0) |
| **Bloqueia** | Todos os demais mods do projeto dependem desta base estar funcional |

## Inventário completo

| Mod | Tipo | Forge | Repo 3.x | Repo 4.0 | Função | Status | Prioridade |
|---|---|---|---|---|---|---|---|
| [SVM] Server Value Modifier | 🌐 Server | [236](https://forge.sp-tarkov.com/mod/236/server-value-modifier-svm) | [GhostFenixx/SVM](https://github.com/GhostFenixx/SVM) | — | Modifica valores do servidor (loot, traders, hideout) — último release SPT 3.11 | 🟠 Aguardar upstream | 🔍 |
| AAAArtem-WTT | 🔍 | [1023](https://forge.sp-tarkov.com/mod/1023/wtt-artem) | 🔍 | 🔍 | 🔍 (relacionado ao WTT) | 🟡 Avaliar | 🔍 |
| acidphantasm-DelayedFleaSales | 🌐 Server | [2016](https://forge.sp-tarkov.com/mod/2016/delayed-flea-sales) | 🔍 | [acidphantasm/delayedfleasales-csharp](https://github.com/acidphantasm/delayedfleasales-csharp) | Atrasa vendas no flea market | 🟢 Instalar | 🔍 |
| acidphantasm-moretagcolours | 🔍 | [1533](https://forge.sp-tarkov.com/mod/1533/more-tag-colours) | 🔍 | 🔍 | Mais cores para tags de itens | 🟡 Avaliar | 🔍 |
| acidphantasm-previewsizer | 🔍 | [2339](https://forge.sp-tarkov.com/mod/2339/preview-sizer) | 🔍 | [acidphantasm/acidphantasm-previewsizer](https://github.com/acidphantasm/acidphantasm-previewsizer) | Redimensiona preview de itens | 🟡 Avaliar | 🔍 |
| acidphantasm-progressivebotsystem | 🌐 Server | [1594](https://forge.sp-tarkov.com/mod/1594/apbs-acids-progressive-bot-system) | 🔍 | [acidphantasm/progressivebotsystem-csharp](https://github.com/acidphantasm/progressivebotsystem-csharp) | Sistema progressivo de bots | 🟢 Instalar | 🔍 |
| acidphantasm-refsptfriendlyquests | 🌐 Server | [1538](https://forge.sp-tarkov.com/mod/1538/ref-spt-friendly-quests) | 🔍 | [acidphantasm/reffriendlyquests-csharp](https://github.com/acidphantasm/reffriendlyquests-csharp) | Quests amigáveis (compatível com Ref) | 🟢 Instalar | 🔍 |
| acidphantasm-simpleworkoutqte | 🌐 Server | [1437](https://forge.sp-tarkov.com/mod/1437/simple-workout-qte) | 🔍 | [acidphantasm/acidphantasm-simpleworkoutqte](https://github.com/acidphantasm/acidphantasm-simpleworkoutqte) | QTE de workout no hideout | 🟡 Avaliar | 🔍 |
| AirFilterWarning | 🔍 | [2129](https://forge.sp-tarkov.com/mod/2129/air-filter-warning) | 🔍 | 🔍 | Aviso de filtro de ar gerador | 🟡 Avaliar | 🔍 |
| AmandsGraphics | 🖥️ Client | [592](https://forge.sp-tarkov.com/mod/592/amandss-graphics) | [Amands2Mello/AmandsGraphics](https://github.com/Amands2Mello/AmandsGraphics) | — | Configurações gráficas avançadas — último release SPT 3.10 | 🟠 Aguardar upstream | 🔍 |
| aMoxoPixel-Painter | 🌐 Server | [1025](https://forge.sp-tarkov.com/mod/1025/painter) | 🔍 | [emilanderss0n/Painter](https://github.com/emilanderss0n/Painter) | Trader que vende mods de armas pintados | 🟡 Avaliar | 🔍 |
| BeltSlot | 🖥️ Client | [2181](https://forge.sp-tarkov.com/mod/2181/belt-slot) | 🔍 | [Trench-foot/BeltSlot](https://github.com/Trench-foot/BeltSlot) | Slot extra de cinto no inventário | 🟡 Avaliar | 🔍 |
| BetterRearSights | 🔍 | [1591](https://forge.sp-tarkov.com/mod/1591/better-rear-sights) | 🔍 | 🔍 | Mira traseira melhorada | 🟡 Avaliar | 🔍 |
| BorkelRNVG | 🖥️ Client | [954](https://forge.sp-tarkov.com/mod/954/borkels-realistic-night-vision-goggles-nvgs-and-t-7) | [Borkel/RealisticNVG-client-2](https://github.com/Borkel/RealisticNVG-client-2) | [Borkel/RealisticNVG-client-2](https://github.com/Borkel/RealisticNVG-client-2) | NVGs realistas com máscaras + luz natural — v2.1.1 (SPT 4.0.13). Mesmo repo serve 3.x e 4.0 (versão por release) | 🟢 Instalar | 🔍 |
| BRNVG_N-15Adapter | 🔍 | — | 🔍 | 🔍 | Adaptador N-15 para BRNVG (sub-pasta interna do mod BorkelRNVG, não publicado separado) | 🟡 Avaliar | 🔍 |
| ChooChoo-TraderModding | 🔍 | [1283](https://forge.sp-tarkov.com/mod/1283/trader-modding-and-improved-weapon-building) | 🔍 | 🔍 | Modding via traders | 🟡 Avaliar | 🔍 |
| ContinuousLoadAmmo | 🔍 | [2112](https://forge.sp-tarkov.com/mod/2112/continuous-load-ammo) | 🔍 | 🔍 | Carregamento contínuo de munição | 🟡 Avaliar | 🔍 |
| CWX | 🔍 | [1454](https://forge.sp-tarkov.com/mod/1454/cwx-megamod) | 🔍 | [CWXDEV/CWX-Mods](https://github.com/CWXDEV/CWX-Mods) | Coleção de mods do CWX (CWX-MegaMod) | 🟡 Avaliar | 🔍 |
| DanW-SPTQuestingBots | 🌐 Server | [1109](https://forge.sp-tarkov.com/mod/1109/questing-bots) | 🔍 | [dwesterwick/SPTQuestingBots](https://github.com/dwesterwick/SPTQuestingBots) | Bots fazendo quests + spawns PMC mimic live | 🟡 Avaliar | 🔍 |
| DeadzoneMod | 🔍 | [1001](https://forge.sp-tarkov.com/mod/1001/deadzone) | 🔍 | 🔍 | Deadzone/free aim style insurgency | 🟡 Avaliar | 🔍 |
| desze-UnlockHideoutCustomization | 🔍 | [2053](https://forge.sp-tarkov.com/mod/2053/unlock-all-hideout-customization) | 🔍 | 🔍 | Desbloqueia customização do hideout | 🟡 Avaliar | 🔍 |
| DewardianDev-MOAR | 🌐 Server | [789](https://forge.sp-tarkov.com/mod/789/moar-bagels-ultra-lite-spawn-mod) | 🔍 | [Andrewgdewar/MOAR](https://github.com/Andrewgdewar/MOAR) | Bot spawning system | 🟡 Avaliar | 🔍 |
| dk.SeparateHostility | 🔍 | [2248](https://forge.sp-tarkov.com/mod/2248/separate-hostility) | 🔍 | 🔍 | Separa hostilidade entre facções | 🟡 Avaliar | 🔍 |
| doordash | 🔍 | [2214](https://forge.sp-tarkov.com/mod/2214/doordash) | 🔍 | 🔍 | Breaching de portas trancadas (SPT 4.0) | 🟡 Avaliar | 🔍 |
| DrakiaXYZ-BigBrain | 🖥️ Client | [902](https://forge.sp-tarkov.com/mod/902/bigbrain) | [DrakiaXYZ/SPT-BigBrain](https://github.com/DrakiaXYZ/SPT-BigBrain) | [DrakiaXYZ/SPT-BigBrain](https://github.com/DrakiaXYZ/SPT-BigBrain) | Library de combat layers para bots (dep. de SAIN) — v1.4.0 (SPT 4.0.13). Mesmo repo, versionado por tags | 🟢 Instalar | 🔍 |
| DrakiaXYZ-EquipFromWeaponRack | 🖥️ Client | [1136](https://forge.sp-tarkov.com/mod/1136/equip-from-weapon-rack) | 🔍 | [DrakiaXYZ/SPT-EquipFromWeaponRack](https://github.com/DrakiaXYZ/SPT-EquipFromWeaponRack) | Equipar arma direto do rack do hideout | 🟡 Avaliar | 🔍 |
| DrakiaXYZ-LootRadius | 🔍 | [1349](https://forge.sp-tarkov.com/mod/1349/loot-radius) | 🔍 | 🔍 | Aumenta raio de loot | 🟡 Avaliar | 🔍 |
| DrakiaXYZ-QuickMoveToContainer | 🖥️ Client | [1341](https://forge.sp-tarkov.com/mod/1341/quick-move-to-containers) | 🔍 | [DrakiaXYZ/SPT-QuickMoveToContainer](https://github.com/DrakiaXYZ/SPT-QuickMoveToContainer) | Ctrl+Click move item para container aberto | 🟡 Avaliar | 🔍 |
| DrakiaXYZ-SearchOpenContainers | 🖥️ Client | [934](https://forge.sp-tarkov.com/mod/934/search-open-containers) | 🔍 | [DrakiaXYZ/SPT-SearchOpenContainers](https://github.com/DrakiaXYZ/SPT-SearchOpenContainers) | Buscar dentro de containers abertos | 🟡 Avaliar | 🔍 |
| DrakiaXYZ-Waypoints | 🖥️ Client | [827](https://forge.sp-tarkov.com/mod/827/waypoints-expanded-navmesh) | [DrakiaXYZ/SPT-Waypoints](https://github.com/DrakiaXYZ/SPT-Waypoints) | [DrakiaXYZ/SPT-Waypoints](https://github.com/DrakiaXYZ/SPT-Waypoints) | Expande navmesh dos mapas (dep. de SAIN) — v1.8.2 (SPT 4.0.13). Mesmo repo, versionado por tags | 🟢 Instalar | 🔍 |
| DynamicExternalResolution | 🔍 | [929](https://forge.sp-tarkov.com/mod/929/dynamic-external-resolution-patch-derp) | 🔍 | 🔍 | Resolução externa dinâmica (DERP) | 🟡 Avaliar | 🔍 |
| DynamicMaps (SPTDynamicMaps) | 🖥️ Client | [1431](https://forge.sp-tarkov.com/mod/1431/dynamic-maps) | 🔍 | [mpstark/SPT-DynamicMaps](https://github.com/mpstark/SPT-DynamicMaps) | UI custom de mapas com tracking de quests | 🟡 Avaliar | 🔍 |
| Eco-Attachment Emporium | 🔍 | [2288](https://forge.sp-tarkov.com/mod/2288/ecos-attachment-emporium) | 🔍 | 🔍 | Mais attachments para armas | 🟡 Avaliar | 🔍 |
| ExpandedFpsLimit | 🔍 | [2066](https://forge.sp-tarkov.com/mod/2066/expanded-fps-limit) | 🔍 | 🔍 | Aumenta limite de FPS (até 360) | 🟡 Avaliar | 🔍 |
| Fika | 🔀 Misto | [2326](https://forge.sp-tarkov.com/mod/2326/project-fika) | [project-fika/Fika-Plugin](https://github.com/project-fika/Fika-Plugin) | [project-fika/Fika-Plugin](https://github.com/project-fika/Fika-Plugin) | Multiplayer base (BepInEx + server) — v2.2.5 (SPT 4.0.13). Mesmo repo. Aliases: `fika-server`, `Fika.Core` | 🟢 Instalar | 🔍 |
| flir-betterkeysng | 🔍 | [1888](https://forge.sp-tarkov.com/mod/1888/better-keys-ng) | 🔍 | 🔍 | UI melhorada para keys — autor `flir` (mesmo de Tarkov Weather System); provável Client | 🟡 Avaliar | 🔍 |
| FOVFix | 🖥️ Client | [701](https://forge.sp-tarkov.com/mod/701/fontaines-fov-fix) | 🔍 | [space-commits/SPT-FOV-Fix](https://github.com/space-commits/SPT-FOV-Fix) | Fix de FOV (Fontaine's FOV Fix) — releases para SPT 4.x | 🟢 Instalar | 🔍 |
| gaylatea-deadlyblades | 🔍 | [819](https://forge.sp-tarkov.com/mod/819/deadly-blades) | 🔍 | 🔍 | Lâminas mais letais | 🟡 Avaliar | 🔍 |
| Gaylatea-UseLooseLoot | 🖥️ Client | [933](https://forge.sp-tarkov.com/mod/933/use-loose-loot) | 🔍 | [DrakiaXYZ/SPT-UseLooseLoot](https://github.com/DrakiaXYZ/SPT-UseLooseLoot) | Usa loose loot direto sem entrar no inventário | 🟡 Avaliar | 🔍 |
| HandsAreNotBusy | 🔍 | [1298](https://forge.sp-tarkov.com/mod/1298/handsarenotbusy) | 🔍 | 🔍 | Mãos não ficam ocupadas (animação) | 🟡 Avaliar | 🔍 |
| hideoutcat | 🔍 | [2038](https://forge.sp-tarkov.com/mod/2038/hideout-cat) | 🔍 | 🔍 | Gato no hideout (5 texturas configuráveis) | 🟡 Avaliar | 🔍 |
| HollywoodFX | 🖥️ Client | [2003](https://forge.sp-tarkov.com/mod/2003/hollywoodfx) | [SleepingPills/HollywoodFX](https://github.com/SleepingPills/HollywoodFX) | [SleepingPills/HollywoodFX](https://github.com/SleepingPills/HollywoodFX) | FX cinematográficos (impactos, blood) — v1.8.4 (SPT 4.0.13). Autor: JankyTheClown / SleepingPills. Mesmo repo | 🟢 Instalar | 🔍 |
| HollywoodGraphics | 🔍 | — | 🔍 | 🔍 | Gráficos cinematográficos — sem mod separado no Forge (provavelmente pasta interna do HollywoodFX) | 🟡 Avaliar | 🔍 |
| IcyClawz.CustomInteractions | 🖥️ Client | [938](https://forge.sp-tarkov.com/mod/938/custom-interactions) | 🔍 | 🔍 | Interações customizadas — perfil do autor: hub.sp-tarkov.com/user/34778-icyclawz (autor IgorEisberg) | 🟡 Avaliar | 🔍 |
| IcyClawz.ItemContextMenuExt | 🖥️ Client | [940](https://forge.sp-tarkov.com/mod/940/item-context-menu-extended) | [IgorEisberg/SPT-ClientMods](https://github.com/IgorEisberg/SPT-ClientMods) | [IgorEisberg/SPT-ClientMods](https://github.com/IgorEisberg/SPT-ClientMods) | Menu de contexto estendido em itens (SPT 4.0.13, 18.1K downloads). Repo monorepo do autor | 🟢 Instalar | 🔍 |
| IcyClawz.ItemSellPrice | 🖥️ Client | [909](https://forge.sp-tarkov.com/mod/909/item-sell-price) | [IgorEisberg/SPT-ClientMods](https://github.com/IgorEisberg/SPT-ClientMods) | [IgorEisberg/SPT-ClientMods](https://github.com/IgorEisberg/SPT-ClientMods) | Preços de venda em todos os traders (SPT 4.0.13, 81.5K downloads). Repo monorepo do autor | 🟢 Instalar | 🔍 |
| IcyClawz.MunitionsExpert | 🖥️ Client | [972](https://forge.sp-tarkov.com/mod/972/munitions-expert-reboot) | 🔍 | 🔍 | Info detalhada de munição — perfil do autor: hub.sp-tarkov.com/user/34778-icyclawz (autor IgorEisberg) | 🟡 Avaliar | 🔍 |
| IhanaMies-LootValueBackend | 🖥️ Client | [1155](https://forge.sp-tarkov.com/mod/1155/lootvalue) | [IhanaMies/LootValue](https://github.com/IhanaMies/LootValue) | — | Mostra valor de loot na UI — último release SPT 3.11 | 🟠 Aguardar upstream | 🔍 |
| inory-agonysfx | 🔍 | [1831](https://forge.sp-tarkov.com/mod/1831/agony-sfx) | 🔍 | 🔍 | SFX de dor/agonia | 🟡 Avaliar | 🔍 |
| JBOBYH_ItemPreviewQoL | 🔍 | [2206](https://forge.sp-tarkov.com/mod/2206/item-preview-qol-screenshots) | 🔍 | 🔍 | QoL de preview de itens | 🟡 Avaliar | 🔍 |
| Jehree-GildedKeyStorage | 🖥️ Client | [865](https://forge.sp-tarkov.com/mod/865/gilded-key-storage) | [Jehree/SPT-Gilded_Key_Storage](https://github.com/Jehree/SPT-Gilded_Key_Storage) | [DrakiaXYZ/SPT-GildedKeyStorage-CSharp](https://github.com/DrakiaXYZ/SPT-GildedKeyStorage-CSharp) | Storage especializado para keys — original do Jehree (3.x), fork C# do DrakiaXYZ é o de 4.0 | 🟢 Instalar | 🔍 |
| Kaeno-TraderScrolling | 🔍 | [1089](https://forge.sp-tarkov.com/mod/1089/kaeno-traderscrolling) | 🔍 | 🔍 | Scroll na lista de traders | 🟡 Avaliar | 🔍 |
| Kat.BetterAmmoLoadingList | 🔍 | [2221](https://forge.sp-tarkov.com/mod/2221/ball-better-ammo-loading-list) | 🔍 | 🔍 | Lista melhorada de loading de munição | 🟡 Avaliar | 🔍 |
| kmyuhkyuk-EnvironmentReplace | 🔍 | [1371](https://forge.sp-tarkov.com/mod/1371/environment-replace) | 🔍 | 🔍 | Substitui ambientes/mapas | 🟡 Avaliar | 🔍 |
| kmyuhkyuk-KmyTarkovApi | 🖥️ Client | [898](https://forge.sp-tarkov.com/mod/898/kmy-tarkov-api) | 🔍 | [kmyuhkyuk/KmyTarkovApi](https://github.com/kmyuhkyuk/KmyTarkovApi) | Framework para client mods | 🟡 Avaliar | 🔍 |
| lacyway-mergeconsumables (MergeConsumables) | 🔍 | [1657](https://forge.sp-tarkov.com/mod/1657/mergeconsumables) | 🔍 | 🔍 | Merge de consumíveis (médicos, comida) | 🟡 Avaliar | 🔍 |
| MoreCheckmarks | 🔀 Misto | [861](https://forge.sp-tarkov.com/mod/861/morecheckmarks) | 🔍 | [TommySoucy/MoreCheckmarks](https://github.com/TommySoucy/MoreCheckmarks) | Checkmarks coloridos em itens (quests, hideout, barters) — v2.1.0 (SPT 4.0.11). Alias: `MoreCheckmarksBackend` | 🟢 Instalar | 🔍 |
| MoxoPixel-MagTape | 🌐 Server | [1018](https://forge.sp-tarkov.com/mod/1018/mag-tape) | 🔍 | [emilanderss0n/MagTape](https://github.com/emilanderss0n/MagTape) | Magazines com tape (visual + tagging) | 🟡 Avaliar | 🔍 |
| MoxoPixel-TacticalGearComponent | 🌐 Server | [1125](https://forge.sp-tarkov.com/mod/1125/tactical-gear-component) | 🔍 | [emilanderss0n/TGC](https://github.com/emilanderss0n/TGC) | Componente de equipamento tático | 🟡 Avaliar | 🔍 |
| MusicManiac-LessRestrictingHeadwear | 🔍 | [930](https://forge.sp-tarkov.com/mod/930/less-restricting-headwear) | 🔍 | 🔍 | Headwear menos restritivo | 🟡 Avaliar | 🔍 |
| platinum-theblacklist | 🔍 | [755](https://forge.sp-tarkov.com/mod/755/the-blacklist-flea-market-enhancements) | 🔍 | 🔍 | Blacklist de itens (flea market enhancements) | 🟡 Avaliar | 🔍 |
| PlayerEncumbranceBar | 🖥️ Client | [1374](https://forge.sp-tarkov.com/mod/1374/player-encumbrance-bar) | [mpstark/SPT-PlayerEncumbranceBar](https://github.com/mpstark/SPT-PlayerEncumbranceBar) | — | Barra de encumbrance no inventário — v1.2.2 (SPT 4.0.13, mantido por Lacyway) | 🟢 Instalar | 🔍 |
| Pluto! - SPT Battlepass | 🔍 | [2098](https://forge.sp-tarkov.com/mod/2098/spt-battlepass) | 🔍 | 🔍 | Battlepass para SPT (Arena Season 0) | 🟡 Avaliar | 🔍 |
| QuickSell | 🔍 | [1732](https://forge.sp-tarkov.com/mod/1732/quicksell) | 🔍 | [TadMaj/Tarkov-QuickSell](https://github.com/TadMaj/Tarkov-QuickSell) | Venda rápida de itens (context menu) | 🟡 Avaliar | 🔍 |
| RaiRai.ColorConverterAPI | 🔍 | [1090](https://forge.sp-tarkov.com/mod/1090/color-converter-api) | 🔍 | 🔍 | API utilitária de conversão de cores | 🟡 Avaliar | 🔍 |
| Realism | 🔀 Misto | [416](https://forge.sp-tarkov.com/mod/416/spt-realism-mod) | [space-commits/SPT-Realism-Mod-Client](https://github.com/space-commits/SPT-Realism-Mod-Client) · [SPT-Realism-Mod-Server](https://github.com/space-commits/SPT-Realism-Mod-Server) | — | Overhaul de realismo (balística, médica, hazards) — último release SPT 3.9.x. Aliases: `SPT-Realism`, `RealismMod` | 🟠 Aguardar upstream | 🔍 |
| redlaser42-Better Headset Descriptions | 🔍 | [2199](https://forge.sp-tarkov.com/mod/2199/better-headset-descriptions) | 🔍 | 🔍 | Descrições melhoradas de headsets | 🟡 Avaliar | 🔍 |
| redlaser42-Increase Climb Height | 🔍 | [1575](https://forge.sp-tarkov.com/mod/1575/increase-climb-height) | 🔍 | 🔍 | Aumenta altura máxima de escalada | 🟡 Avaliar | 🔍 |
| SAIN | 🖥️ Client | [791](https://forge.sp-tarkov.com/mod/791/sain-solarints-ai-modifications-full-ai-combat-system-replacement) | [Solarint/SAIN](https://github.com/Solarint/SAIN) | [ArchangelWTF/SAIN](https://github.com/ArchangelWTF/SAIN) | Substituição completa de IA dos bots — v4.4.3 (SPT 4.0.13). **3.x: repo Solarint original** · **4.2.0+: fork ArchangelWTF**. Depende de BigBrain + Waypoints | 🟢 Instalar | 🔍 |
| seasoniterator | 🔍 | 🔍 | 🔍 | 🔍 | 🔍 (estações/seasonal?) — não confirmado no Forge | 🟡 Avaliar | 🔍 |
| shibdib-NoTransitTasks | 🔍 | [1944](https://forge.sp-tarkov.com/mod/1944/no-transit-tasks) | 🔍 | 🔍 | Remove tasks de transit | 🟡 Avaliar | 🔍 |
| Skwizzy-LootingBots | 🔀 Misto | [812](https://forge.sp-tarkov.com/mod/812/looting-bots) | 🔍 | [Skwizzy/SPT-LootingBots](https://github.com/Skwizzy/SPT-LootingBots) | Bots fazendo loot (BepInEx + server) | 🟡 Avaliar | 🔍 |
| somtam.NoBush | 🔍 | [2123](https://forge.sp-tarkov.com/mod/2123/no-bush-updated-for-311) | 🔍 | 🔍 | Para AI atirar em quem está na bush | 🟡 Avaliar | 🔍 |
| somtam.SimpleDeClutter | 🔍 | [2139](https://forge.sp-tarkov.com/mod/2139/simple-declutter) | 🔍 | 🔍 | Reduz clutter visual | 🟡 Avaliar | 🔍 |
| SPT-FreshContentBackport | 🔍 | [2187](https://forge.sp-tarkov.com/mod/2187/fresh-content-backport) | 🔍 | 🔍 | Backport de conteúdo novo | 🟡 Avaliar | 🔍 |
| SPT-InsuranceFraud | 🔍 | [1792](https://forge.sp-tarkov.com/mod/1792/insurance-fraud) | 🔍 | [ibxccc123/SPT-InsuranceFraud](https://github.com/ibxccc123/SPT-InsuranceFraud) | Fraude no seguro (loot dropado retorna) | 🟡 Avaliar | 🔍 |
| SPTVRAMCleaner | 🔍 | [2173](https://forge.sp-tarkov.com/mod/2173/vram-cleaner) | 🔍 | 🔍 | Limpeza de VRAM (autor matsixx, não swiftxp) | 🟡 Avaliar | 🔍 |
| StashSearch | 🖥️ Client | [2148](https://forge.sp-tarkov.com/mod/2148/stash-search) | 🔍 | [DrakiaXYZ/SPT-StashSearch](https://github.com/DrakiaXYZ/SPT-StashSearch) | Busca dentro do stash | 🟡 Avaliar | 🔍 |
| SwiftXP.ShowMeTheMoney | 🔍 | [2299](https://forge.sp-tarkov.com/mod/2299/show-me-the-money) | 🔍 | [swiftxp-hub/spt-show-me-the-money](https://github.com/swiftxp-hub/spt-show-me-the-money) | Mostra dinheiro/valores em UI | 🟡 Avaliar | 🔍 |
| TacticalToasterUNTARGH | 🌐 Server | [2342](https://forge.sp-tarkov.com/mod/2342/untar-go-home) | 🔍 | [TacticalToaster/TacticalToasterUNTARGH](https://github.com/TacticalToaster/TacticalToasterUNTARGH) | Adiciona UNTAR como faction com bots customizados | 🟡 Avaliar | 🔍 |
| Tarkov Weather System | 🖥️ Client | [2120](https://forge.sp-tarkov.com/mod/2120/time-weather-changer-ng) | [flir063-spt @ v2.3.3.0](https://gitlab.com/flir063-spt/timeweatherchanger/-/tree/v2.3.3.0) | [flir063-spt/timeweatherchanger](https://gitlab.com/flir063-spt/timeweatherchanger) | Time & Weather Changer NG — v2.4.0 (SPT 4.0.13). Autor: flir. Hospedado no **GitLab** | 🟢 Instalar | 🔍 |
| TellTheTime | 🔍 | [2202](https://forge.sp-tarkov.com/mod/2202/tell-the-time) | 🔍 | 🔍 | Mostra hora atual | 🟡 Avaliar | 🔍 |
| Terkoiz.Freecam | 🖥️ Client | [164](https://forge.sp-tarkov.com/mod/164/freecam) | [TerkoizLT/SPT-Freecam](https://github.com/TerkoizLT/SPT-Freecam) | — | Câmera livre (debug/replay) — v1.4.6 (último release SPT 3.11). Mantido por acidphantasm | 🟠 Aguardar upstream | 🔍 |
| tyfon-hideoutinprogress | 🖥️ Client | [2076](https://forge.sp-tarkov.com/mod/2076/hideout-in-progress) | 🔍 | [tyfon7/hip](https://github.com/tyfon7/hip) | Botão "Transfer Items" no hideout (SPT 4.0). Alias: `Tyfon.HideoutInProgress` | 🟢 Instalar | 🔍 |
| tyfon-uifixes | 🖥️ Client | [1342](https://forge.sp-tarkov.com/mod/1342/ui-fixes) | 🔍 | [tyfon7/UIFixes](https://github.com/tyfon7/UIFixes) | Coleção de QoL fixes de UI. Aliases: `Tyfon.UIFixes`, `Tyfon.UIFixes.Net` | 🟡 Avaliar | 🔍 |
| tyfon-weaponcustomizer | 🖥️ Client | [1950](https://forge.sp-tarkov.com/mod/1950/weapon-customizer) | 🔍 | [tyfon7/WeaponCustomizer](https://github.com/tyfon7/WeaponCustomizer) | Fine tune de attachments. Alias: `Tyfon.WeaponCustomizer` | 🟡 Avaliar | 🔍 |
| Virtual's Custom Quest Loader | 🌐 Server | [649](https://forge.sp-tarkov.com/mod/649/virtuals-custom-quest-loader) | 🔍 | [VirtualAE/Virtuals-Custom-Quest-Loader](https://github.com/VirtualAE/Virtuals-Custom-Quest-Loader) | Dependência para mods importarem custom quests. Alias: `VCQL`, `VCQLQuestZones` | 🟡 Avaliar | 🔍 |
| VisceralCombat | 🔍 | — | 🔍 | 🔍 | Efeitos viscerais de combate — distribuído via Patreon/Discord (Valentin The Mad), não publicado no Forge | 🟡 Avaliar | 🔍 |
| VolumetricBloodFX | 🔍 | — | 🔍 | 🔍 | FX de sangue volumétrico — distribuído via Patreon/Discord (Valentin The Mad), não publicado no Forge | 🟡 Avaliar | 🔍 |
| Wara-ModdingStatsHelper | 🔍 | [1300](https://forge.sp-tarkov.com/mod/1300/modding-stats-helper-by-wara) | 🔍 | 🔍 | Helper de stats em modding | 🟡 Avaliar | 🔍 |
| WTT-Armory | 🌐 Server | [2246](https://forge.sp-tarkov.com/mod/2246/wtt-armory) | [WelcomeToTarkov/WTT-Armory](https://github.com/WelcomeToTarkov/WTT-Armory) | [WelcomeToTarkov/WTT-Armory @ 4.0](https://github.com/WelcomeToTarkov/WTT-Armory/tree/4.0) | Pack de 50+ armas + quests (WTT team) — v2.0.5 (SPT 4.0.13). 4.0 está em branch separada | 🟢 Instalar | 🔍 |
| WTT-PackNStrap | 🌐 Server | [1278](https://forge.sp-tarkov.com/mod/1278/wtt-pack-n-strap) | [WelcomeToTarkov/PackNStrap](https://github.com/WelcomeToTarkov/PackNStrap) | [WelcomeToTarkov/PackNStrap](https://github.com/WelcomeToTarkov/PackNStrap) | Battle belt + small cases (WTT team) — v2.0.4 (SPT 4.0.13). Mesmo repo | 🟢 Instalar | 🔍 |
| yellowdoge-tarkovrarecollectibles | 🔍 | [2318](https://forge.sp-tarkov.com/mod/2318/tarkov-rare-collectibles) | 🔍 | 🔍 | Itens raros colecionáveis | 🟡 Avaliar | 🔍 |
| zzDrakiaXYZ-LiveFleaPrices | 🌐 Server | [1131](https://forge.sp-tarkov.com/mod/1131/live-flea-prices) | 🔍 | [DrakiaXYZ/SPT-LiveFleaPrices-CSharp](https://github.com/DrakiaXYZ/SPT-LiveFleaPrices-CSharp) | Preços do flea ao vivo (live data) — versão C# do mesmo autor | 🟢 Instalar | 🔍 |
| 🏠 Band-Aid | 🔍 | — | — | — | Fix/patch interno do projeto | ⬆️ Evoluir p/ 4.0 | 🔍 |
| 🏠 CoordLogger | 🔍 | — | — | — | Logger de coordenadas (utilitário interno) | ⬆️ Evoluir p/ 4.0 | 🔍 |
| 🏠 FikaTransitFix (FikaTransitFixServer) | 🌐 Server | — | — | — | Fix de transit em raids do Fika (interno) | ⬆️ Evoluir p/ 4.0 | 🔍 |
| 🏠 FixReloadUltraFika | 🔍 | — | — | — | Fix de reload no UltraFika (interno) | ⬆️ Evoluir p/ 4.0 | 🔍 |
| 🏠 ForceSync | 🔍 | — | — | — | Força sincronização (interno, relacionado ao UltraFika) | ⬆️ Evoluir p/ 4.0 | 🔍 |
| 🏠 GhostMercenaries | 🔍 | — | — | — | Mercenários customizados (interno) | ⬆️ Evoluir p/ 4.0 | 🔍 |
| 🏠 IdleSprintFix | 🖥️ Client | — | — | — | Fix do bug de sprint travado (interno, v1.2.2) | ⬆️ Evoluir p/ 4.0 | 🔍 |
| 🏠 TarkovRedLine (TarkovRedLine-ServerMod) | 🌐 Server | — | — | — | Mod do servidor RedLine — customização proprietária do projeto | ⬆️ Evoluir p/ 4.0 | 🔍 |
| 🏠 UmbigoPreto-Face the Knight - Mask Fix | 🔍 | — | — | — | Fix da máscara do Knight (interno) | ⬆️ Evoluir p/ 4.0 | 🔍 |
| 🏠 UmbigoPreto-TrueTrauma | 🔍 | — | — | — | Sistema de trauma realista (interno) | ⬆️ Evoluir p/ 4.0 | 🔍 |

## Próximos passos

1. **Continuar pesquisa para mods 🔍 restantes** (~40 mods — autores menores, mods sem presença pública clara):
   - IcyClawz.CustomInteractions, IcyClawz.MunitionsExpert (sem repo público — buscar no forge)
   - HollywoodGraphics, VolumetricBloodFX, VisceralCombat (FX/visuais)
   - desze-*, somtam.*, redlaser42-*, MusicManiac-*, Wara-*, yellowdoge-*
   - ContinuousLoadAmmo, BetterRearSights, AirFilterWarning, BRNVG_N-15Adapter
   - Pluto Battlepass, RaiRai.ColorConverterAPI, lacyway-mergeconsumables, etc.

2. **Decidir destino dos `🟠 Aguardar upstream`:** SVM (3.11), AmandsGraphics (3.10), PlayerEncumbranceBar (3.8), Realism (3.9.x), IhanaMies-LootValue (3.11), Terkoiz.Freecam (3.11). Por mod: aguardar autor OU mudar para `🔧 Desenvolver` OU `⚫ Não incluir`.

3. **Criar specs individuais** em `docs/migration/<mod-name>/` para os 11 mods `🏠` com status `⬆️ Evoluir p/ 4.0` (ver tabela acima)

4. **Mapear dependências entre mods** (ex: SAIN → BigBrain + Waypoints; Skwizzy-LootingBots → BigBrain) — adicionar coluna `Depende de` ou notas estruturadas

5. **Validar tipos `🔍`** abrindo cada repo encontrado (priorizar mods com GitHub/GitLab identificado)

## Histórico

| Data | Autor | Descrição |
|---|---|---|
| 2026-05-02 | Guilherme | +49 / -0 linhas |
| 2026-05-02 | Guilherme | +15 / -7 linhas |
| 2026-05-03 | Guilherme | +32 / -17 linhas |
| 2026-05-03 | Guilherme | +130 / -26 linhas |
| 2026-05-03 | Guilherme | docs(wiki): add sp-tarkov/wiki snapshot under wiki/spt/ |
| 2026-05-03 | Guilherme | docs(migration): add Forge+GitHub research findings for ~50 mods (1st batch) |
| 2026-05-03 | Guilherme | docs(migration): expand research with Forge URLs (HollywoodFX, BorkelRNVG, IcyClawz, Weather) |
| 2026-05-03 | Guilherme | docs(migration): revise status enum (Atualizar/Migrar/Desenvolver/Aguardar) + mark 11 internal mods |
| 2026-05-03 | Guilherme | docs(migration): rename Atualizar→Instalar, fix contradictions, standardize column formats |
| 2026-05-03 | Guilherme | docs(migration): split GitHub into 3.x/4.0 cols + add Prioridade + hideoutcat + research 11 forge mods |
| 2026-05-03 | Guilherme | docs(migration): rename GitHub cols to Repo (accept GitLab) + add Tarkov Weather 3.x v2.3.3.0 link |
| 2026-05-03 | Guilherme | docs(migration): research forge URLs for ~80 mods with 🔍 status |
| 2026-05-03 | Guilherme | docs(migration): move internal mods (🏠) to end of inventory table in alphabetical order |
