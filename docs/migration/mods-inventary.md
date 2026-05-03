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

## Base — 🏠 UltraFika-Plugin

Mod fundamental do projeto. Habilita multiplayer no SPT e serve como base sobre a qual os demais mods rodam. **Migração prioritária zero** — sem ele, o restante do ecossistema não tem sentido.

| Item | Detalhe |
|---|---|
| **Origem** | 🏠 Interno (criado pela equipe) |
| **Tipo** | 🖥️ Client (C# / BepInEx) |
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
| [SVM] Server Value Modifier | 🌐 Server | 🔍 | [GhostFenixx/SVM](https://github.com/GhostFenixx/SVM) | — | Modifica valores do servidor (loot, traders, hideout) — último release SPT 3.11 | 🟠 Aguardar upstream | 🔍 |
| AAAArtem-WTT | 🔍 | 🔍 | 🔍 | 🔍 | 🔍 (relacionado ao WTT) | 🟡 Avaliar | 🔍 |
| acidphantasm-DelayedFleaSales | 🌐 Server | 🔍 | 🔍 | [acidphantasm/delayedfleasales-csharp](https://github.com/acidphantasm/delayedfleasales-csharp) | Atrasa vendas no flea market | 🟢 Instalar | 🔍 |
| acidphantasm-moretagcolours | 🔍 | 🔍 | 🔍 | 🔍 | Mais cores para tags de itens | 🟡 Avaliar | 🔍 |
| acidphantasm-previewsizer | 🔍 | 🔍 | 🔍 | [acidphantasm/acidphantasm-previewsizer](https://github.com/acidphantasm/acidphantasm-previewsizer) | Redimensiona preview de itens | 🟡 Avaliar | 🔍 |
| acidphantasm-progressivebotsystem | 🌐 Server | 🔍 | 🔍 | [acidphantasm/progressivebotsystem-csharp](https://github.com/acidphantasm/progressivebotsystem-csharp) | Sistema progressivo de bots | 🟢 Instalar | 🔍 |
| acidphantasm-refsptfriendlyquests | 🌐 Server | 🔍 | 🔍 | [acidphantasm/reffriendlyquests-csharp](https://github.com/acidphantasm/reffriendlyquests-csharp) | Quests amigáveis (compatível com Ref) | 🟢 Instalar | 🔍 |
| acidphantasm-simpleworkoutqte | 🌐 Server | 🔍 | 🔍 | [acidphantasm/acidphantasm-simpleworkoutqte](https://github.com/acidphantasm/acidphantasm-simpleworkoutqte) | QTE de workout no hideout | 🟡 Avaliar | 🔍 |
| AirFilterWarning | 🔍 | 🔍 | 🔍 | 🔍 | Aviso de filtro de ar gerador | 🟡 Avaliar | 🔍 |
| AmandsGraphics | 🖥️ Client | 🔍 | [Amands2Mello/AmandsGraphics](https://github.com/Amands2Mello/AmandsGraphics) | — | Configurações gráficas avançadas — último release SPT 3.10 | 🟠 Aguardar upstream | 🔍 |
| aMoxoPixel-Painter | 🌐 Server | 🔍 | 🔍 | [emilanderss0n/Painter](https://github.com/emilanderss0n/Painter) | Trader que vende mods de armas pintados | 🟡 Avaliar | 🔍 |
| BeltSlot | 🖥️ Client | 🔍 | 🔍 | [Trench-foot/BeltSlot](https://github.com/Trench-foot/BeltSlot) | Slot extra de cinto no inventário | 🟡 Avaliar | 🔍 |
| BetterRearSights | 🔍 | 🔍 | 🔍 | 🔍 | Mira traseira melhorada | 🟡 Avaliar | 🔍 |
| BorkelRNVG | 🖥️ Client | [954](https://forge.sp-tarkov.com/mod/954/borkels-realistic-night-vision-goggles-nvgs-and-t-7) | [Borkel/RealisticNVG-client-2](https://github.com/Borkel/RealisticNVG-client-2) | [Borkel/RealisticNVG-client-2](https://github.com/Borkel/RealisticNVG-client-2) | NVGs realistas com máscaras + luz natural — v2.1.1 (SPT 4.0.13). Mesmo repo serve 3.x e 4.0 (versão por release) | 🟢 Instalar | 🔍 |
| BRNVG_N-15Adapter | 🔍 | 🔍 | 🔍 | 🔍 | Adaptador N-15 para BRNVG | 🟡 Avaliar | 🔍 |
| ChooChoo-TraderModding | 🔍 | 🔍 | 🔍 | 🔍 | Modding via traders | 🟡 Avaliar | 🔍 |
| ContinuousLoadAmmo | 🔍 | 🔍 | 🔍 | 🔍 | Carregamento contínuo de munição | 🟡 Avaliar | 🔍 |
| CWX | 🔍 | 🔍 | 🔍 | [CWXDEV/CWX-Mods](https://github.com/CWXDEV/CWX-Mods) | Coleção de mods do CWX (verificar conteúdo) | 🟡 Avaliar | 🔍 |
| DanW-SPTQuestingBots | 🌐 Server | 🔍 | 🔍 | [dwesterwick/SPTQuestingBots](https://github.com/dwesterwick/SPTQuestingBots) | Bots fazendo quests + spawns PMC mimic live | 🟡 Avaliar | 🔍 |
| DeadzoneMod | 🔍 | 🔍 | 🔍 | 🔍 | 🔍 (deadzone de mira?) | 🟡 Avaliar | 🔍 |
| desze-UnlockHideoutCustomization | 🔍 | 🔍 | 🔍 | 🔍 | Desbloqueia customização do hideout | 🟡 Avaliar | 🔍 |
| DewardianDev-MOAR | 🌐 Server | 🔍 | 🔍 | [Andrewgdewar/MOAR](https://github.com/Andrewgdewar/MOAR) | Bot spawning system | 🟡 Avaliar | 🔍 |
| dk.SeparateHostility | 🔍 | 🔍 | 🔍 | 🔍 | Separa hostilidade entre facções | 🟡 Avaliar | 🔍 |
| doordash | 🔍 | 🔍 | 🔍 | 🔍 | 🔍 | 🟡 Avaliar | 🔍 |
| DrakiaXYZ-BigBrain | 🖥️ Client | [902](https://forge.sp-tarkov.com/mod/902/bigbrain) | [DrakiaXYZ/SPT-BigBrain](https://github.com/DrakiaXYZ/SPT-BigBrain) | [DrakiaXYZ/SPT-BigBrain](https://github.com/DrakiaXYZ/SPT-BigBrain) | Library de combat layers para bots (dep. de SAIN) — v1.4.0 (SPT 4.0.13). Mesmo repo, versionado por tags | 🟢 Instalar | 🔍 |
| DrakiaXYZ-EquipFromWeaponRack | 🖥️ Client | 🔍 | 🔍 | [DrakiaXYZ/SPT-EquipFromWeaponRack](https://github.com/DrakiaXYZ/SPT-EquipFromWeaponRack) | Equipar arma direto do rack do hideout | 🟡 Avaliar | 🔍 |
| DrakiaXYZ-LootRadius | 🔍 | 🔍 | 🔍 | 🔍 | Aumenta raio de loot | 🟡 Avaliar | 🔍 |
| DrakiaXYZ-QuickMoveToContainer | 🖥️ Client | 🔍 | 🔍 | [DrakiaXYZ/SPT-QuickMoveToContainer](https://github.com/DrakiaXYZ/SPT-QuickMoveToContainer) | Ctrl+Click move item para container aberto | 🟡 Avaliar | 🔍 |
| DrakiaXYZ-SearchOpenContainers | 🖥️ Client | 🔍 | 🔍 | [DrakiaXYZ/SPT-SearchOpenContainers](https://github.com/DrakiaXYZ/SPT-SearchOpenContainers) | Buscar dentro de containers abertos | 🟡 Avaliar | 🔍 |
| DrakiaXYZ-Waypoints | 🖥️ Client | [827](https://forge.sp-tarkov.com/mod/827/waypoints-expanded-navmesh) | [DrakiaXYZ/SPT-Waypoints](https://github.com/DrakiaXYZ/SPT-Waypoints) | [DrakiaXYZ/SPT-Waypoints](https://github.com/DrakiaXYZ/SPT-Waypoints) | Expande navmesh dos mapas (dep. de SAIN) — v1.8.2 (SPT 4.0.13). Mesmo repo, versionado por tags | 🟢 Instalar | 🔍 |
| DynamicExternalResolution | 🔍 | 🔍 | 🔍 | 🔍 | Resolução externa dinâmica | 🟡 Avaliar | 🔍 |
| DynamicMaps (SPTDynamicMaps) | 🖥️ Client | 🔍 | 🔍 | [mpstark/SPT-DynamicMaps](https://github.com/mpstark/SPT-DynamicMaps) | UI custom de mapas com tracking de quests | 🟡 Avaliar | 🔍 |
| Eco-Attachment Emporium | 🔍 | 🔍 | 🔍 | 🔍 | Mais attachments para armas | 🟡 Avaliar | 🔍 |
| ExpandedFpsLimit | 🔍 | 🔍 | 🔍 | 🔍 | Aumenta limite de FPS | 🟡 Avaliar | 🔍 |
| Fika | 🔀 Misto | [2326](https://forge.sp-tarkov.com/mod/2326/project-fika) | [project-fika/Fika-Plugin](https://github.com/project-fika/Fika-Plugin) | [project-fika/Fika-Plugin](https://github.com/project-fika/Fika-Plugin) | Multiplayer base (BepInEx + server) — v2.2.5 (SPT 4.0.13). Mesmo repo. Aliases: `fika-server`, `Fika.Core` | 🟢 Instalar | 🔍 |
| flir-betterkeysng | 🔍 | 🔍 | 🔍 | 🔍 | UI melhorada para keys — autor `flir` (mesmo de Tarkov Weather System); provável Client | 🟡 Avaliar | 🔍 |
| FOVFix | 🖥️ Client | 🔍 | 🔍 | [space-commits/SPT-FOV-Fix](https://github.com/space-commits/SPT-FOV-Fix) | Fix de FOV (Fontaine's FOV Fix) — releases para SPT 4.x | 🟢 Instalar | 🔍 |
| gaylatea-deadlyblades | 🔍 | 🔍 | 🔍 | 🔍 | Lâminas mais letais | 🟡 Avaliar | 🔍 |
| Gaylatea-UseLooseLoot | 🖥️ Client | 🔍 | 🔍 | [DrakiaXYZ/SPT-UseLooseLoot](https://github.com/DrakiaXYZ/SPT-UseLooseLoot) | Usa loose loot direto sem entrar no inventário | 🟡 Avaliar | 🔍 |
| HandsAreNotBusy | 🔍 | 🔍 | 🔍 | 🔍 | Mãos não ficam ocupadas (animação) | 🟡 Avaliar | 🔍 |
| hideoutcat | 🔍 | 🔍 | 🔍 | 🔍 | 🔍 (relacionado ao hideout) | 🟡 Avaliar | 🔍 |
| HollywoodFX | 🖥️ Client | [2003](https://forge.sp-tarkov.com/mod/2003/hollywoodfx) | [SleepingPills/HollywoodFX](https://github.com/SleepingPills/HollywoodFX) | [SleepingPills/HollywoodFX](https://github.com/SleepingPills/HollywoodFX) | FX cinematográficos (impactos, blood) — v1.8.4 (SPT 4.0.13). Autor: JankyTheClown / SleepingPills. Mesmo repo | 🟢 Instalar | 🔍 |
| HollywoodGraphics | 🔍 | 🔍 | 🔍 | 🔍 | Gráficos cinematográficos — mesmo autor de HollywoodFX/HollywoodCam (JankyTheClown / SleepingPills) | 🟡 Avaliar | 🔍 |
| IcyClawz.CustomInteractions | 🖥️ Client | 🔍 | 🔍 | 🔍 | Interações customizadas — perfil do autor: hub.sp-tarkov.com/user/34778-icyclawz (autor IgorEisberg) | 🟡 Avaliar | 🔍 |
| IcyClawz.ItemContextMenuExt | 🖥️ Client | [forge/1283](https://forge.sp-tarkov.com/files/file/1283-item-context-menu-extended/) | [IgorEisberg/SPT-ClientMods](https://github.com/IgorEisberg/SPT-ClientMods) | [IgorEisberg/SPT-ClientMods](https://github.com/IgorEisberg/SPT-ClientMods) | Menu de contexto estendido em itens (SPT 4.0.13, 18.1K downloads). Repo monorepo do autor | 🟢 Instalar | 🔍 |
| IcyClawz.ItemSellPrice | 🖥️ Client | [forge/1230](https://forge.sp-tarkov.com/files/file/1230-item-sell-price/) | [IgorEisberg/SPT-ClientMods](https://github.com/IgorEisberg/SPT-ClientMods) | [IgorEisberg/SPT-ClientMods](https://github.com/IgorEisberg/SPT-ClientMods) | Preços de venda em todos os traders (SPT 4.0.13, 81.5K downloads). Repo monorepo do autor | 🟢 Instalar | 🔍 |
| IcyClawz.MunitionsExpert | 🖥️ Client | 🔍 | 🔍 | 🔍 | Info detalhada de munição — perfil do autor: hub.sp-tarkov.com/user/34778-icyclawz (autor IgorEisberg) | 🟡 Avaliar | 🔍 |
| IhanaMies-LootValueBackend | 🖥️ Client | 🔍 | [IhanaMies/LootValue](https://github.com/IhanaMies/LootValue) | — | Mostra valor de loot na UI — último release SPT 3.11 | 🟠 Aguardar upstream | 🔍 |
| inory-agonysfx | 🔍 | 🔍 | 🔍 | 🔍 | SFX de dor/agonia | 🟡 Avaliar | 🔍 |
| JBOBYH_ItemPreviewQoL | 🔍 | 🔍 | 🔍 | 🔍 | QoL de preview de itens | 🟡 Avaliar | 🔍 |
| Jehree-GildedKeyStorage | 🖥️ Client | 🔍 | [Jehree/SPT-Gilded_Key_Storage](https://github.com/Jehree/SPT-Gilded_Key_Storage) | [DrakiaXYZ/SPT-GildedKeyStorage-CSharp](https://github.com/DrakiaXYZ/SPT-GildedKeyStorage-CSharp) | Storage especializado para keys — original do Jehree (3.x), fork C# do DrakiaXYZ é o de 4.0 | 🟢 Instalar | 🔍 |
| Kaeno-TraderScrolling | 🔍 | 🔍 | 🔍 | 🔍 | Scroll na lista de traders | 🟡 Avaliar | 🔍 |
| Kat.BetterAmmoLoadingList | 🔍 | 🔍 | 🔍 | 🔍 | Lista melhorada de loading de munição | 🟡 Avaliar | 🔍 |
| kmyuhkyuk-EnvironmentReplace | 🔍 | 🔍 | 🔍 | 🔍 | Substitui ambientes/mapas | 🟡 Avaliar | 🔍 |
| kmyuhkyuk-KmyTarkovApi | 🖥️ Client | 🔍 | 🔍 | [kmyuhkyuk/KmyTarkovApi](https://github.com/kmyuhkyuk/KmyTarkovApi) | Framework para client mods | 🟡 Avaliar | 🔍 |
| lacyway-mergeconsumables (MergeConsumables) | 🔍 | 🔍 | 🔍 | 🔍 | Merge de consumíveis (médicos, comida) | 🟡 Avaliar | 🔍 |
| MoreCheckmarks | 🔀 Misto | 🔍 | 🔍 | [TommySoucy/MoreCheckmarks](https://github.com/TommySoucy/MoreCheckmarks) | Checkmarks coloridos em itens (quests, hideout, barters) — v2.1.0 (SPT 4.0.11). Alias: `MoreCheckmarksBackend` | 🟢 Instalar | 🔍 |
| MoxoPixel-MagTape | 🌐 Server | 🔍 | 🔍 | [emilanderss0n/MagTape](https://github.com/emilanderss0n/MagTape) | Magazines com tape (visual + tagging) | 🟡 Avaliar | 🔍 |
| MoxoPixel-TacticalGearComponent | 🌐 Server | 🔍 | 🔍 | [emilanderss0n/TGC](https://github.com/emilanderss0n/TGC) | Componente de equipamento tático | 🟡 Avaliar | 🔍 |
| MusicManiac-LessRestrictingHeadwear | 🔍 | 🔍 | 🔍 | 🔍 | Headwear menos restritivo | 🟡 Avaliar | 🔍 |
| platinum-theblacklist | 🔍 | 🔍 | 🔍 | 🔍 | Blacklist de itens | 🟡 Avaliar | 🔍 |
| PlayerEncumbranceBar | 🖥️ Client | 🔍 | [mpstark/SPT-PlayerEncumbranceBar](https://github.com/mpstark/SPT-PlayerEncumbranceBar) | — | Barra de encumbrance no inventário — último release SPT 3.8 | 🟠 Aguardar upstream | 🔍 |
| Pluto! - SPT Battlepass | 🔍 | 🔍 | 🔍 | 🔍 | Battlepass para SPT (não encontrado em busca pública) | 🟡 Avaliar | 🔍 |
| QuickSell | 🔍 | 🔍 | 🔍 | [TadMaj/Tarkov-QuickSell](https://github.com/TadMaj/Tarkov-QuickSell) | Venda rápida de itens (context menu) | 🟡 Avaliar | 🔍 |
| RaiRai.ColorConverterAPI | 🔍 | 🔍 | 🔍 | 🔍 | API utilitária de conversão de cores | 🟡 Avaliar | 🔍 |
| Realism | 🔀 Misto | 🔍 | [space-commits/SPT-Realism-Mod-Client](https://github.com/space-commits/SPT-Realism-Mod-Client) · [SPT-Realism-Mod-Server](https://github.com/space-commits/SPT-Realism-Mod-Server) | — | Overhaul de realismo (balística, médica, hazards) — último release SPT 3.9.x. Aliases: `SPT-Realism`, `RealismMod` | 🟠 Aguardar upstream | 🔍 |
| redlaser42-Better Headset Descriptions | 🔍 | 🔍 | 🔍 | 🔍 | Descrições melhoradas de headsets | 🟡 Avaliar | 🔍 |
| redlaser42-Increase Climb Height | 🔍 | 🔍 | 🔍 | 🔍 | Aumenta altura máxima de escalada | 🟡 Avaliar | 🔍 |
| SAIN | 🖥️ Client | [791](https://forge.sp-tarkov.com/mod/791/sain-solarints-ai-modifications-full-ai-combat-system-replacement) | [Solarint/SAIN](https://github.com/Solarint/SAIN) | [ArchangelWTF/SAIN](https://github.com/ArchangelWTF/SAIN) | Substituição completa de IA dos bots — v4.4.3 (SPT 4.0.13). **3.x: repo Solarint original** · **4.2.0+: fork ArchangelWTF**. Depende de BigBrain + Waypoints | 🟢 Instalar | 🔍 |
| seasoniterator | 🔍 | 🔍 | 🔍 | 🔍 | 🔍 (estações/seasonal?) | 🟡 Avaliar | 🔍 |
| shibdib-NoTransitTasks | 🔍 | 🔍 | 🔍 | 🔍 | Remove tasks de transit | 🟡 Avaliar | 🔍 |
| Skwizzy-LootingBots | 🔀 Misto | 🔍 | 🔍 | [Skwizzy/SPT-LootingBots](https://github.com/Skwizzy/SPT-LootingBots) | Bots fazendo loot (BepInEx + server) | 🟡 Avaliar | 🔍 |
| somtam.NoBush | 🔍 | 🔍 | 🔍 | 🔍 | Remove arbustos densos | 🟡 Avaliar | 🔍 |
| somtam.SimpleDeClutter | 🔍 | 🔍 | 🔍 | 🔍 | Reduz clutter visual | 🟡 Avaliar | 🔍 |
| SPT-FreshContentBackport | 🔍 | 🔍 | 🔍 | 🔍 | Backport de conteúdo novo | 🟡 Avaliar | 🔍 |
| SPT-InsuranceFraud | 🔍 | 🔍 | 🔍 | [ibxccc123/SPT-InsuranceFraud](https://github.com/ibxccc123/SPT-InsuranceFraud) | Fraude no seguro (loot dropado retorna) | 🟡 Avaliar | 🔍 |
| SPTVRAMCleaner | 🔍 | 🔍 | 🔍 | 🔍 | Limpeza de VRAM (talvez relacionado a swiftxp-hub/spt-server-memory-cleaner) | 🟡 Avaliar | 🔍 |
| StashSearch | 🖥️ Client | 🔍 | 🔍 | [DrakiaXYZ/SPT-StashSearch](https://github.com/DrakiaXYZ/SPT-StashSearch) | Busca dentro do stash | 🟡 Avaliar | 🔍 |
| SwiftXP.ShowMeTheMoney | 🔍 | 🔍 | 🔍 | [swiftxp-hub/spt-show-me-the-money](https://github.com/swiftxp-hub/spt-show-me-the-money) | Mostra dinheiro/valores em UI | 🟡 Avaliar | 🔍 |
| TacticalToasterUNTARGH | 🌐 Server | 🔍 | 🔍 | [TacticalToaster/TacticalToasterUNTARGH](https://github.com/TacticalToaster/TacticalToasterUNTARGH) | Adiciona UNTAR como faction com bots customizados | 🟡 Avaliar | 🔍 |
| Tarkov Weather System | 🖥️ Client | [2120](https://forge.sp-tarkov.com/mod/2120/time-weather-changer-ng) | [flir063-spt @ v2.3.3.0](https://gitlab.com/flir063-spt/timeweatherchanger/-/tree/v2.3.3.0) | [flir063-spt/timeweatherchanger](https://gitlab.com/flir063-spt/timeweatherchanger) | Time & Weather Changer NG — v2.4.0 (SPT 4.0.13). Autor: flir. Hospedado no **GitLab** | 🟢 Instalar | 🔍 |
| TellTheTime | 🔍 | 🔍 | 🔍 | 🔍 | Mostra hora atual | 🟡 Avaliar | 🔍 |
| Terkoiz.Freecam | 🖥️ Client | 🔍 | [TerkoizLT/SPT-Freecam](https://github.com/TerkoizLT/SPT-Freecam) | — | Câmera livre (debug/replay) — v1.4.6 (último release SPT 3.11) | 🟠 Aguardar upstream | 🔍 |
| tyfon-hideoutinprogress | 🖥️ Client | 🔍 | 🔍 | [tyfon7/hip](https://github.com/tyfon7/hip) | Botão "Transfer Items" no hideout (SPT 4.0). Alias: `Tyfon.HideoutInProgress` | 🟢 Instalar | 🔍 |
| tyfon-uifixes | 🖥️ Client | 🔍 | 🔍 | [tyfon7/UIFixes](https://github.com/tyfon7/UIFixes) | Coleção de QoL fixes de UI. Aliases: `Tyfon.UIFixes`, `Tyfon.UIFixes.Net` | 🟡 Avaliar | 🔍 |
| tyfon-weaponcustomizer | 🖥️ Client | 🔍 | 🔍 | [tyfon7/WeaponCustomizer](https://github.com/tyfon7/WeaponCustomizer) | Fine tune de attachments. Alias: `Tyfon.WeaponCustomizer` | 🟡 Avaliar | 🔍 |
| Virtual's Custom Quest Loader | 🌐 Server | 🔍 | 🔍 | [VirtualAE/Virtuals-Custom-Quest-Loader](https://github.com/VirtualAE/Virtuals-Custom-Quest-Loader) | Dependência para mods importarem custom quests. Alias: `VCQL`, `VCQLQuestZones` | 🟡 Avaliar | 🔍 |
| VisceralCombat | 🔍 | 🔍 | 🔍 | 🔍 | Efeitos viscerais de combate | 🟡 Avaliar | 🔍 |
| VolumetricBloodFX | 🔍 | 🔍 | 🔍 | 🔍 | FX de sangue volumétrico | 🟡 Avaliar | 🔍 |
| Wara-ModdingStatsHelper | 🔍 | 🔍 | 🔍 | 🔍 | Helper de stats em modding | 🟡 Avaliar | 🔍 |
| WTT-Armory | 🌐 Server | [2246](https://forge.sp-tarkov.com/mod/2246/wtt-armory) | [WelcomeToTarkov/WTT-Armory](https://github.com/WelcomeToTarkov/WTT-Armory) | [WelcomeToTarkov/WTT-Armory @ 4.0](https://github.com/WelcomeToTarkov/WTT-Armory/tree/4.0) | Pack de 50+ armas + quests (WTT team) — v2.0.5 (SPT 4.0.13). 4.0 está em branch separada | 🟢 Instalar | 🔍 |
| WTT-PackNStrap | 🌐 Server | [1278](https://forge.sp-tarkov.com/mod/1278/wtt-pack-n-strap) | [WelcomeToTarkov/PackNStrap](https://github.com/WelcomeToTarkov/PackNStrap) | [WelcomeToTarkov/PackNStrap](https://github.com/WelcomeToTarkov/PackNStrap) | Battle belt + small cases (WTT team) — v2.0.4 (SPT 4.0.13). Mesmo repo | 🟢 Instalar | 🔍 |
| yellowdoge-tarkovrarecollectibles | 🔍 | 🔍 | 🔍 | 🔍 | Itens raros colecionáveis | 🟡 Avaliar | 🔍 |
| zzDrakiaXYZ-LiveFleaPrices | 🌐 Server | 🔍 | 🔍 | [DrakiaXYZ/SPT-LiveFleaPrices-CSharp](https://github.com/DrakiaXYZ/SPT-LiveFleaPrices-CSharp) | Preços do flea ao vivo (live data) — versão C# do mesmo autor | 🟢 Instalar | 🔍 |
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
