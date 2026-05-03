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
- **Forge:** URL no [forge.sp-tarkov.com](https://forge.sp-tarkov.com/mods) (página única lista todas as versões) · `🔍 buscar` · `—` (confirmado ausente)
- **GitHub:** URL do repositório fonte · `🔍 buscar` · `—` (privado/não-existe)
- **4.x?:** ✅ tem versão SPT 4.0+ · ❌ só tem 3.x · 🔍 verificar
- **Status:** ver enum abaixo

## Base — 🏠 UltraFika-Plugin

Mod fundamental do projeto. Habilita multiplayer no SPT e serve como base sobre a qual os demais mods rodam. **Migração prioritária zero** — sem ele, o restante do ecossistema não tem sentido.

| Item | Detalhe |
|---|---|
| **Origem** | 🏠 Interno (criado pela equipe) |
| **Tipo** | 🖥️ Client (C# / BepInEx) |
| **Função** | Cliente multiplayer (fork de Fika) |
| **Forge** | — (não publicado) |
| **GitHub** | — (não público — fork privado) |
| **4.x?** | ❌ |
| **Upstream** | [Project Fika](https://forge.sp-tarkov.com/mod/2326/project-fika) — base de onde foi forkado |
| **Prioridade** | 🔥 Crítica — primeiro mod a ser migrado |
| **Status migração** | 🔄 Migrar (adaptar código 3.x existente para 4.0) |
| **Bloqueia** | Todos os demais mods do projeto dependem desta base estar funcional |

## Inventário completo

| Mod | Tipo | Forge | GitHub | 4.x? | Função | Status |
|---|---|---|---|---|---|---|
| [SVM] Server Value Modifier | 🌐 Server | 🔍 | [GhostFenixx/SVM](https://github.com/GhostFenixx/SVM) | ❌ (latest 3.11) | Modifica valores do servidor (loot, traders, hideout) | 🟠 Aguardar upstream |
| AAAArtem-WTT | 🔍 | 🔍 | 🔍 | 🔍 | 🔍 (relacionado ao WTT) | 🔵 Avaliar |
| acidphantasm-DelayedFleaSales | 🌐 Server | 🔍 | [acidphantasm/delayedfleasales-csharp](https://github.com/acidphantasm/delayedfleasales-csharp) | ✅ | Atrasa vendas no flea market | 🆕 Atualizar |
| acidphantasm-moretagcolours | 🔍 | 🔍 | 🔍 | 🔍 | Mais cores para tags de itens | 🔵 Avaliar |
| acidphantasm-previewsizer | 🔍 | 🔍 | [acidphantasm/acidphantasm-previewsizer](https://github.com/acidphantasm/acidphantasm-previewsizer) | 🔍 | Redimensiona preview de itens | 🔵 Avaliar |
| acidphantasm-progressivebotsystem | 🌐 Server | 🔍 | [acidphantasm/progressivebotsystem-csharp](https://github.com/acidphantasm/progressivebotsystem-csharp) | ✅ | Sistema progressivo de bots | 🆕 Atualizar |
| acidphantasm-refsptfriendlyquests | 🌐 Server | 🔍 | [acidphantasm/reffriendlyquests-csharp](https://github.com/acidphantasm/reffriendlyquests-csharp) | ✅ | Quests amigáveis (compatível com Ref) | 🆕 Atualizar |
| acidphantasm-simpleworkoutqte | 🌐 Server | 🔍 | [acidphantasm/acidphantasm-simpleworkoutqte](https://github.com/acidphantasm/acidphantasm-simpleworkoutqte) | 🔍 | QTE de workout no hideout | 🔵 Avaliar |
| AirFilterWarning | 🔍 | 🔍 | 🔍 | 🔍 | Aviso de filtro de ar gerador | 🔵 Avaliar |
| AmandsGraphics | 🖥️ Client | 🔍 | [Amands2Mello/AmandsGraphics](https://github.com/Amands2Mello/AmandsGraphics) | ❌ (3.10) | Configurações gráficas avançadas | 🟠 Aguardar upstream |
| aMoxoPixel-Painter | 🌐 Server | 🔍 | [emilanderss0n/Painter](https://github.com/emilanderss0n/Painter) | 🔍 | Trader que vende mods de armas pintados | 🔵 Avaliar |
| 🏠 Band-Aid | 🔍 | — | — | ❌ | Fix/patch interno do projeto | 🔄 Migrar |
| BeltSlot | 🖥️ Client | 🔍 | [Trench-foot/BeltSlot](https://github.com/Trench-foot/BeltSlot) | 🔍 | Slot extra de cinto no inventário | 🔵 Avaliar |
| BetterRearSights | 🔍 | 🔍 | 🔍 | 🔍 | Mira traseira melhorada | 🔵 Avaliar |
| BorkelRNVG | 🖥️ Client | [954/borkels-realistic-nvgs](https://forge.sp-tarkov.com/mod/954/borkels-realistic-night-vision-goggles-nvgs-and-t-7) | 🔍 | ✅ 4.0.13 (v2.1.1) | NVGs realistas com máscaras + luz natural | 🆕 Atualizar |
| BRNVG_N-15Adapter | 🔍 | 🔍 | 🔍 | 🔍 | Adaptador N-15 para BRNVG | 🔵 Avaliar |
| ChooChoo-TraderModding | 🔍 | 🔍 | 🔍 | 🔍 | Modding via traders | 🔵 Avaliar |
| ContinuousLoadAmmo | 🔍 | 🔍 | 🔍 | 🔍 | Carregamento contínuo de munição | 🔵 Avaliar |
| 🏠 CoordLogger | 🔍 | — | — | ❌ | Logger de coordenadas (utilitário interno) | 🔄 Migrar |
| CWX | 🔍 | 🔍 | [CWXDEV/CWX-Mods](https://github.com/CWXDEV/CWX-Mods) | 🔍 | Coleção de mods do CWX (verificar conteúdo) | 🔵 Avaliar |
| DanW-SPTQuestingBots | 🌐 Server | 🔍 | [dwesterwick/SPTQuestingBots](https://github.com/dwesterwick/SPTQuestingBots) | 🔍 | Bots fazendo quests + spawns PMC mimic live | 🔵 Avaliar |
| DeadzoneMod | 🔍 | 🔍 | 🔍 | 🔍 | 🔍 (deadzone de mira?) | 🔵 Avaliar |
| desze-UnlockHideoutCustomization | 🔍 | 🔍 | 🔍 | 🔍 | Desbloqueia customização do hideout | 🔵 Avaliar |
| DewardianDev-MOAR | 🌐 Server | 🔍 | [Andrewgdewar/MOAR](https://github.com/Andrewgdewar/MOAR) | 🔍 | Bot spawning system | 🔵 Avaliar |
| dk.SeparateHostility | 🔍 | 🔍 | 🔍 | 🔍 | Separa hostilidade entre facções | 🔵 Avaliar |
| doordash | 🔍 | 🔍 | 🔍 | 🔍 | 🔍 | 🔵 Avaliar |
| DrakiaXYZ-BigBrain | 🖥️ Client | [902/bigbrain](https://forge.sp-tarkov.com/mod/902/bigbrain) | [DrakiaXYZ/SPT-BigBrain](https://github.com/DrakiaXYZ/SPT-BigBrain) | ✅ 4.0.13 (v1.4.0) | Library de combat layers para bots (dep. de SAIN) | 🆕 Atualizar |
| DrakiaXYZ-EquipFromWeaponRack | 🖥️ Client | 🔍 | [DrakiaXYZ/SPT-EquipFromWeaponRack](https://github.com/DrakiaXYZ/SPT-EquipFromWeaponRack) | 🔍 | Equipar arma direto do rack do hideout | 🔵 Avaliar |
| DrakiaXYZ-LootRadius | 🔍 | 🔍 | 🔍 | 🔍 | Aumenta raio de loot | 🔵 Avaliar |
| DrakiaXYZ-QuickMoveToContainer | 🖥️ Client | 🔍 | [DrakiaXYZ/SPT-QuickMoveToContainer](https://github.com/DrakiaXYZ/SPT-QuickMoveToContainer) | 🔍 | Ctrl+Click move item para container aberto | 🔵 Avaliar |
| DrakiaXYZ-SearchOpenContainers | 🖥️ Client | 🔍 | [DrakiaXYZ/SPT-SearchOpenContainers](https://github.com/DrakiaXYZ/SPT-SearchOpenContainers) | 🔍 | Buscar dentro de containers abertos | 🔵 Avaliar |
| DrakiaXYZ-Waypoints | 🖥️ Client | [827/waypoints-expanded-navmesh](https://forge.sp-tarkov.com/mod/827/waypoints-expanded-navmesh) | [DrakiaXYZ/SPT-Waypoints](https://github.com/DrakiaXYZ/SPT-Waypoints) | ✅ 4.0.13 (v1.8.2) | Expande navmesh dos mapas (dep. de SAIN) | 🆕 Atualizar |
| DynamicExternalResolution | 🔍 | 🔍 | 🔍 | 🔍 | Resolução externa dinâmica | 🔵 Avaliar |
| DynamicMaps (SPTDynamicMaps) | 🖥️ Client | 🔍 | [mpstark/SPT-DynamicMaps](https://github.com/mpstark/SPT-DynamicMaps) | 🔍 | UI custom de mapas com tracking de quests | 🔵 Avaliar |
| Eco-Attachment Emporium | 🔍 | 🔍 | 🔍 | 🔍 | Mais attachments para armas | 🔵 Avaliar |
| ExpandedFpsLimit | 🔍 | 🔍 | 🔍 | 🔍 | Aumenta limite de FPS | 🔵 Avaliar |
| Fika (fika-server / Fika.Core) | 🔀 Misto | [2326/project-fika](https://forge.sp-tarkov.com/mod/2326/project-fika) | [project-fika/Fika-Plugin](https://github.com/project-fika/Fika-Plugin) | ✅ 4.0.13 (v2.2.5) | Multiplayer base (BepInEx + server) | 🆕 Atualizar |
| 🏠 FikaTransitFix (FikaTransitFixServer) | 🌐 Server | — | — | ❌ | Fix de transit em raids do Fika (interno) | 🔄 Migrar |
| 🏠 FixReloadUltraFika | 🔍 | — | — | ❌ | Fix de reload no UltraFika (interno) | 🔄 Migrar |
| flir-betterkeysng | 🔍 (provável Client, mesmo autor de Time & Weather) | 🔍 | 🔍 | 🔍 | UI melhorada para keys (autor: flir, mesmo de Tarkov Weather) | 🔵 Avaliar |
| 🏠 ForceSync | 🔍 | — | — | ❌ | Força sincronização (interno, relacionado ao UltraFika) | 🔄 Migrar |
| FOVFix | 🖥️ Client | 🔍 | [space-commits/SPT-FOV-Fix](https://github.com/space-commits/SPT-FOV-Fix) | ✅ 4.x.x | Fix de FOV (Fontaine's FOV Fix) | 🆕 Atualizar |
| gaylatea-deadlyblades | 🔍 | 🔍 | 🔍 | 🔍 | Lâminas mais letais | 🔵 Avaliar |
| Gaylatea-UseLooseLoot | 🖥️ Client | 🔍 | [DrakiaXYZ/SPT-UseLooseLoot](https://github.com/DrakiaXYZ/SPT-UseLooseLoot) | 🔍 | Usa loose loot direto sem entrar no inventário | 🔵 Avaliar |
| 🏠 GhostMercenaries | 🔍 | — | — | ❌ | Mercenários customizados (interno) | 🔄 Migrar |
| HandsAreNotBusy | 🔍 | 🔍 | 🔍 | 🔍 | Mãos não ficam ocupadas (animação) | 🔵 Avaliar |
| HollywoodFX | 🖥️ Client | [2003/hollywoodfx](https://forge.sp-tarkov.com/mod/2003/hollywoodfx) | 🔍 | ✅ 4.0.13 (v1.8.4) | FX cinematográficos (impactos, blood) por JankyTheClown | 🆕 Atualizar |
| HollywoodGraphics | 🔍 (provável JankyTheClown) | 🔍 | 🔍 | 🔍 | Gráficos cinematográficos (mesmo autor de HollywoodFX/HollywoodCam) | 🔵 Avaliar |
| IcyClawz.CustomInteractions | 🖥️ Client | 🔍 (na hub.sp-tarkov.com/user/34778-icyclawz/) | — | 🔍 | Interações customizadas | 🔵 Avaliar |
| IcyClawz.ItemContextMenuExt | 🖥️ Client | [hub: 1283](https://hub.sp-tarkov.com/files/file/1283-item-context-menu-extended/) | — | ✅ 4.0.13 | Menu de contexto estendido em itens (18.1K downloads) | 🆕 Atualizar |
| IcyClawz.ItemSellPrice | 🖥️ Client | [hub: 1230](https://hub.sp-tarkov.com/files/file/1230-item-sell-price/) | — | ✅ 4.0.13 | Mostra preços de venda em todos os traders (81.5K downloads) | 🆕 Atualizar |
| IcyClawz.MunitionsExpert | 🖥️ Client | 🔍 (na hub.sp-tarkov.com/user/34778-icyclawz/) | — | 🔍 | Info detalhada de munição | 🔵 Avaliar |
| 🏠 IdleSprintFix | 🖥️ Client | — | — | ❌ | Fix do bug de sprint travado (interno, v1.2.2) | 🔄 Migrar |
| IhanaMies-LootValueBackend | 🖥️ Client | 🔍 | [IhanaMies/LootValue](https://github.com/IhanaMies/LootValue) | ❌ (3.11) | Mostra valor de loot na UI | 🟠 Aguardar upstream |
| inory-agonysfx | 🔍 | 🔍 | 🔍 | 🔍 | SFX de dor/agonia | 🔵 Avaliar |
| JBOBYH_ItemPreviewQoL | 🔍 | 🔍 | 🔍 | 🔍 | QoL de preview de itens | 🔵 Avaliar |
| Jehree-GildedKeyStorage (DrakiaXYZ-GildedKeyStorage) | 🖥️ Client | 🔍 | [Jehree/SPT-Gilded_Key_Storage](https://github.com/Jehree/SPT-Gilded_Key_Storage) (orig) · [DrakiaXYZ/SPT-GildedKeyStorage-CSharp](https://github.com/DrakiaXYZ/SPT-GildedKeyStorage-CSharp) (4.0 fork) | ✅ (fork C#) | Storage especializado para keys | 🆕 Atualizar |
| Kaeno-TraderScrolling | 🔍 | 🔍 | 🔍 | 🔍 | Scroll na lista de traders | 🔵 Avaliar |
| Kat.BetterAmmoLoadingList | 🔍 | 🔍 | 🔍 | 🔍 | Lista melhorada de loading de munição | 🔵 Avaliar |
| kmyuhkyuk-EnvironmentReplace | 🔍 | 🔍 | 🔍 | 🔍 | Substitui ambientes/mapas | 🔵 Avaliar |
| kmyuhkyuk-KmyTarkovApi | 🖥️ Client | 🔍 | [kmyuhkyuk/KmyTarkovApi](https://github.com/kmyuhkyuk/KmyTarkovApi) | 🔍 | Framework para client mods | 🔵 Avaliar |
| lacyway-mergeconsumables (MergeConsumables) | 🔍 | 🔍 | 🔍 | 🔍 | Merge de consumíveis (médicos, comida) | 🔵 Avaliar |
| MoreCheckmarks (MoreCheckmarksBackend) | 🔀 Misto | 🔍 | [TommySoucy/MoreCheckmarks](https://github.com/TommySoucy/MoreCheckmarks) | ✅ 4.0.11 (v2.1.0) | Checkmarks coloridos em itens (quests, hideout, barters) | 🆕 Atualizar |
| MoxoPixel-MagTape | 🌐 Server | 🔍 | [emilanderss0n/MagTape](https://github.com/emilanderss0n/MagTape) | 🔍 | Magazines com tape (visual + tagging) | 🔵 Avaliar |
| MoxoPixel-TacticalGearComponent | 🌐 Server | 🔍 | [emilanderss0n/TGC](https://github.com/emilanderss0n/TGC) | 🔍 | Componente de equipamento tático | 🔵 Avaliar |
| MusicManiac-LessRestrictingHeadwear | 🔍 | 🔍 | 🔍 | 🔍 | Headwear menos restritivo | 🔵 Avaliar |
| platinum-theblacklist | 🔍 | 🔍 | 🔍 | 🔍 | Blacklist de itens | 🔵 Avaliar |
| PlayerEncumbranceBar | 🖥️ Client | 🔍 | [mpstark/SPT-PlayerEncumbranceBar](https://github.com/mpstark/SPT-PlayerEncumbranceBar) | ❌ (3.8) | Barra de encumbrance no inventário | 🟠 Aguardar upstream |
| Pluto! - SPT Battlepass | 🔍 | 🔍 | 🔍 (não encontrado em busca) | 🔍 | Battlepass para SPT | 🔵 Avaliar |
| QuickSell | 🔍 | 🔍 | [TadMaj/Tarkov-QuickSell](https://github.com/TadMaj/Tarkov-QuickSell) | 🔍 | Venda rápida de itens (context menu) | 🔵 Avaliar |
| RaiRai.ColorConverterAPI | 🔍 | 🔍 | 🔍 | 🔍 | API utilitária de conversão de cores | 🔵 Avaliar |
| Realism (SPT-Realism / RealismMod) | 🔀 Misto | 🔍 | [space-commits/SPT-Realism-Mod-Client](https://github.com/space-commits/SPT-Realism-Mod-Client) · [SPT-Realism-Mod-Server](https://github.com/space-commits/SPT-Realism-Mod-Server) | ❌ (3.9.x) | Overhaul de realismo (balística, médica, hazards) | 🟠 Aguardar upstream |
| redlaser42-Better Headset Descriptions | 🔍 | 🔍 | 🔍 | 🔍 | Descrições melhoradas de headsets | 🔵 Avaliar |
| redlaser42-Increase Climb Height | 🔍 | 🔍 | 🔍 | 🔍 | Aumenta altura máxima de escalada | 🔵 Avaliar |
| SAIN | 🖥️ Client | [791/sain-solarints-ai](https://forge.sp-tarkov.com/mod/791/sain-solarints-ai-modifications-full-ai-combat-system-replacement) | [Solarint/SAIN](https://github.com/Solarint/SAIN) | ✅ 4.0.13 (v4.4.3) | Substituição completa de IA dos bots | 🆕 Atualizar |
| seasoniterator | 🔍 | 🔍 | 🔍 | 🔍 | 🔍 (estações/seasonal?) | 🔵 Avaliar |
| shibdib-NoTransitTasks | 🔍 | 🔍 | 🔍 | 🔍 | Remove tasks de transit | 🔵 Avaliar |
| Skwizzy-LootingBots | 🔀 Misto | 🔍 | [Skwizzy/SPT-LootingBots](https://github.com/Skwizzy/SPT-LootingBots) | 🔍 | Bots fazendo loot (BepInEx + server) | 🔵 Avaliar |
| somtam.NoBush | 🔍 | 🔍 | 🔍 | 🔍 | Remove arbustos densos | 🔵 Avaliar |
| somtam.SimpleDeClutter | 🔍 | 🔍 | 🔍 | 🔍 | Reduz clutter visual | 🔵 Avaliar |
| SPT-FreshContentBackport | 🔍 | 🔍 | 🔍 | 🔍 | Backport de conteúdo novo | 🔵 Avaliar |
| SPT-InsuranceFraud | 🔍 | 🔍 | [ibxccc123/SPT-InsuranceFraud](https://github.com/ibxccc123/SPT-InsuranceFraud) | 🔍 | Fraude no seguro (loot dropado retorna) | 🔵 Avaliar |
| SPTVRAMCleaner | 🔍 | 🔍 | 🔍 | 🔍 | Limpeza de VRAM (talvez relacionado a swiftxp-hub/spt-server-memory-cleaner) | 🔵 Avaliar |
| StashSearch | 🖥️ Client | 🔍 | [DrakiaXYZ/SPT-StashSearch](https://github.com/DrakiaXYZ/SPT-StashSearch) | 🔍 | Busca dentro do stash | 🔵 Avaliar |
| SwiftXP.ShowMeTheMoney | 🔍 | 🔍 | [swiftxp-hub/spt-show-me-the-money](https://github.com/swiftxp-hub/spt-show-me-the-money) | 🔍 | Mostra dinheiro/valores em UI | 🔵 Avaliar |
| TacticalToasterUNTARGH | 🌐 Server | 🔍 | [TacticalToaster/TacticalToasterUNTARGH](https://github.com/TacticalToaster/TacticalToasterUNTARGH) | 🔍 | Adiciona UNTAR como faction com bots customizados | 🔵 Avaliar |
| Tarkov Weather System | 🖥️ Client | [2120/time-weather-changer-ng](https://forge.sp-tarkov.com/mod/2120/time-weather-changer-ng) | 🔍 | ✅ 4.0.13 (v2.4.0) | Time & Weather Changer NG (autor: flir) | 🆕 Atualizar |
| 🏠 TarkovRedLine (TarkovRedLine-ServerMod) | 🌐 Server | — | — | ❌ | Mod do servidor RedLine — customização proprietária do projeto | 🔄 Migrar |
| TellTheTime | 🔍 | 🔍 | 🔍 | 🔍 | Mostra hora atual | 🔵 Avaliar |
| Terkoiz.Freecam | 🖥️ Client | 🔍 | [TerkoizLT/SPT-Freecam](https://github.com/TerkoizLT/SPT-Freecam) | ❌ (3.11, v1.4.6) | Câmera livre (debug/replay) | 🟠 Aguardar upstream |
| tyfon-hideoutinprogress (Tyfon.HideoutInProgress) | 🖥️ Client | 🔍 | [tyfon7/hip](https://github.com/tyfon7/hip) | ✅ 4.0 | Botão "Transfer Items" no hideout | 🆕 Atualizar |
| tyfon-uifixes (Tyfon.UIFixes) | 🖥️ Client | 🔍 | [tyfon7/UIFixes](https://github.com/tyfon7/UIFixes) | 🔍 | Coleção de QoL fixes de UI | 🔵 Avaliar |
| tyfon-weaponcustomizer (Tyfon.WeaponCustomizer) | 🖥️ Client | 🔍 | [tyfon7/WeaponCustomizer](https://github.com/tyfon7/WeaponCustomizer) | 🔍 | Fine tune de attachments | 🔵 Avaliar |
| 🏠 UmbigoPreto-Face the Knight - Mask Fix | 🔍 | — | — | ❌ | Fix da máscara do Knight (interno) | 🔄 Migrar |
| 🏠 UmbigoPreto-TrueTrauma | 🔍 | — | — | ❌ | Sistema de trauma realista (interno) | 🔄 Migrar |
| Virtual's Custom Quest Loader (VCQL) | 🌐 Server | 🔍 | [VirtualAE/Virtuals-Custom-Quest-Loader](https://github.com/VirtualAE/Virtuals-Custom-Quest-Loader) | 🔍 | Dependência para mods importarem custom quests | 🔵 Avaliar |
| VisceralCombat | 🔍 | 🔍 | 🔍 | 🔍 | Efeitos viscerais de combate | 🔵 Avaliar |
| VolumetricBloodFX | 🔍 | 🔍 | 🔍 | 🔍 | FX de sangue volumétrico | 🔵 Avaliar |
| Wara-ModdingStatsHelper | 🔍 | 🔍 | 🔍 | 🔍 | Helper de stats em modding | 🔵 Avaliar |
| WTT-Armory | 🌐 Server | [2246/wtt-armory](https://forge.sp-tarkov.com/mod/2246/wtt-armory) | [WelcomeToTarkov](https://github.com/WelcomeToTarkov) | 🔍 | Pack de 50+ armas + quests (WTT team) | 🔵 Avaliar |
| WTT-PackNStrap | 🌐 Server | [1278/wtt-pack-n-strap](https://forge.sp-tarkov.com/mod/1278/wtt-pack-n-strap) | [WelcomeToTarkov](https://github.com/WelcomeToTarkov) | 🔍 | Battle belt + small cases (WTT team) | 🔵 Avaliar |
| yellowdoge-tarkovrarecollectibles | 🔍 | 🔍 | 🔍 | 🔍 | Itens raros colecionáveis | 🔵 Avaliar |
| zzDrakiaXYZ-LiveFleaPrices | 🌐 Server | 🔍 | [DrakiaXYZ/SPT-LiveFleaPrices-CSharp](https://github.com/DrakiaXYZ/SPT-LiveFleaPrices-CSharp) | ✅ (versão C#) | Preços do flea ao vivo (live data) | 🆕 Atualizar |

## Utilitários / pastas

Itens listados que **não são mods** propriamente ditos — são pastas ou utilitários de suporte. Avaliar caso a caso se fazem sentido no novo repo ou se viram parte da infra (`.agents/`, scripts, etc.).

| Item | Tipo | Observação |
|---|---|---|
| `spt` | 📁 Pasta | Avaliar conteúdo — pode ser config ou scripts |
| `ssh` | 📁 Pasta | Provavelmente chaves/config SSH — **NÃO versionar segredos** |
| `tarkin` | 📁 Pasta | Avaliar conteúdo |

## Status disponíveis

- 🔵 **Avaliar** — ainda não decidido (default ao adicionar mod novo)
- 🆕 **Atualizar** — versão 4.0 **já existe** publicamente; basta usar/baixar a nova versão (sem trabalho de código)
- 🔄 **Migrar** — adaptar/refatorar código 3.x existente para 4.0 (envolve refactor e evolução, não criação do zero). Padrão para mods internos `🏠` que temos código.
- 🔧 **Desenvolver** — criar do zero no 4.0 (autor original não lançou; decidimos antecipar fazendo do nosso lado)
- 🟠 **Aguardar upstream** — autor original ainda não lançou 4.0; default quando 4.x = ❌. Pode virar `🔧 Desenvolver` se decidirmos antecipar.
- 🔴 **Bloqueado** — incompatibilidade arquitetural sem workaround conhecido
- ⚫ **Não incluir** — fora do escopo do projeto

### Fluxo de decisão

```
4.x existe publicamente?
├─ ✅ Sim                     → 🆕 Atualizar
├─ ❌ Não, mas é mod interno  → 🔄 Migrar (temos o código 3.x)
└─ ❌ Não, é da comunidade    → 🟠 Aguardar upstream
                                  └─ Decidiu antecipar? → 🔧 Desenvolver
```

## Progresso da pesquisa

- **Forge URLs encontrados:** ~12 (SAIN, BigBrain, Waypoints, Fika, WTT-Armory, WTT-PackNStrap, HollywoodFX, BorkelRNVG, Tarkov Weather, IcyClawz mods)
- **GitHub URLs encontrados:** ~40+
- **Tipo classificado (não 🔍):** ~40 mods
- **Restante** (`🔍` em todas ou maioria das colunas): ~40 mods — autores menores ou nomes ambíguos

## Mods 🏠 Internos (criados pela equipe)

11 mods são proprietários do projeto. Todos com status `🔄 Migrar` (temos código 3.x e adaptamos para 4.0):

| # | Mod | Tipo | Notas |
|---|---|---|---|
| 1 | 🏠 UltraFika-Plugin | 🖥️ Client | **Base** do projeto — fork do Project Fika upstream |
| 2 | 🏠 IdleSprintFix | 🖥️ Client | v1.2.2 — fix do bug de sprint travado |
| 3 | 🏠 UmbigoPreto-Face the Knight - Mask Fix | 🔍 | Fix da máscara do boss Knight |
| 4 | 🏠 UmbigoPreto-TrueTrauma | 🔍 | Sistema de trauma realista |
| 5 | 🏠 TarkovRedLine | 🌐 Server | Customizações server-side da brand "Red Line" |
| 6 | 🏠 GhostMercenaries | 🔍 | Mercenários customizados |
| 7 | 🏠 ForceSync | 🔍 | Forçar sincronização (relacionado ao UltraFika) |
| 8 | 🏠 FixReloadUltraFika | 🔍 | Fix de reload no UltraFika |
| 9 | 🏠 FikaTransitFix | 🌐 Server | Fix de transit em raids do Fika |
| 10 | 🏠 CoordLogger | 🔍 | Logger de coordenadas (utilitário) |
| 11 | 🏠 Band-Aid | 🔍 | Fix/patch interno |

**Decisão arquitetural:** mods internos podem virar pastas em `mods/client/<mod>/` ou `mods/server/<mod>/` neste repositório (já que temos o código fonte). Avaliar caso a caso se cada um justifica vida própria ou pode ser consolidado.

## Próximos passos

1. **Continuar pesquisa para mods 🔍 restantes** (próxima rodada de research):
   - IcyClawz.* (4 mods sem repo público — buscar no forge)
   - HollywoodFX, HollywoodGraphics, VolumetricBloodFX (FX/visuais)
   - desze-*, somtam.*, UmbigoPreto-*, redlaser42-*, MusicManiac-*
   - ContinuousLoadAmmo, BorkelRNVG, BetterRearSights, AirFilterWarning, etc.

2. **Mods sem 4.x confirmado (status `🟠 Aguardar upstream`):**
   - SVM (3.11 latest), AmandsGraphics (3.10), PlayerEncumbranceBar (3.8), Realism (3.9.x), IhanaMies-LootValue (3.11), Terkoiz.Freecam (3.11)
   - Decidir por mod: aguardar autor original OU portar manualmente OU descartar

3. **TarkovRedLine + UltraFika-Plugin:** mods proprietários do projeto — `🔧 Desenvolver` confirmado, criar specs

4. **Identificar dependências entre mods** (ex: SAIN → BigBrain + Waypoints) — adicionar coluna ou notas
5. Criar specs individuais em `docs/migration/<mod-name>/` para os mods com status `🔄 Migrar` ou `🔧 Desenvolver`
6. Atualizar este inventário conforme decisões forem tomadas

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
