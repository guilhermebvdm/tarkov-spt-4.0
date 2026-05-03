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

- **Tipo:** 🖥️ Client (C# / BepInEx) · 🌐 Server (TypeScript) · 🔀 Misto (ambos) · 🔍 a classificar
- **Link 3.x** e **Link 4.x:** `[texto](url)` quando encontrado · `🔍 buscar` quando pendente · `—` quando confirmado ausente
- **Status:** ver enum abaixo

Quando o link 4.x for `—`, decidir entre `🔧 Desenvolver`, `🟠 Aguardar upstream` ou `⚫ Não incluir`.

Tipos pré-classificados aqui são **chutes** baseados no nome (`Server`, `ServerMod`, `Backend` no nome → 🌐). Sempre validar consultando o repositório real antes de portar.

## Base — UltraFika-Plugin

Mod fundamental do projeto. Habilita multiplayer no SPT e serve como base sobre a qual os demais mods rodam. **Migração prioritária zero** — sem ele, o restante do ecossistema não tem sentido.

| Item | Detalhe |
|---|---|
| **Tipo** | Client (C# / BepInEx) |
| **Função** | Cliente multiplayer (Fika) |
| **Link 3.x** | 🔍 buscar |
| **Link 4.x** | 🔍 buscar |
| **Prioridade** | 🔥 Crítica — primeiro mod a ser migrado |
| **Status migração** | 🔵 Avaliar |
| **Bloqueia** | Todos os demais mods do projeto dependem desta base estar funcional |

## Inventário completo

| Mod | Tipo | Link 3.x | Link 4.x | Função | Status |
|---|---|---|---|---|---|
| [SVM] Server Value Modifier | 🌐 Server | 🔍 buscar | 🔍 buscar | Modifica valores do servidor (loot, traders, etc.) | 🔵 Avaliar |
| AAAArtem-WTT | 🔍 | 🔍 buscar | 🔍 buscar | 🔍 (relacionado ao WTT) | 🔵 Avaliar |
| acidphantasm-DelayedFleaSales | 🔍 | 🔍 buscar | 🔍 buscar | Atrasa vendas no flea market | 🔵 Avaliar |
| acidphantasm-moretagcolours | 🔍 | 🔍 buscar | 🔍 buscar | Mais cores para tags de itens | 🔵 Avaliar |
| acidphantasm-previewsizer | 🔍 | 🔍 buscar | 🔍 buscar | Redimensiona preview de itens | 🔵 Avaliar |
| acidphantasm-progressivebotsystem | 🔍 | 🔍 buscar | 🔍 buscar | Sistema progressivo de bots | 🔵 Avaliar |
| acidphantasm-refsptfriendlyquests | 🔍 | 🔍 buscar | 🔍 buscar | Quests amigáveis ao SPT | 🔵 Avaliar |
| acidphantasm-simpleworkoutqte | 🔍 | 🔍 buscar | 🔍 buscar | QTE de workout no hideout | 🔵 Avaliar |
| AirFilterWarning | 🔍 | 🔍 buscar | 🔍 buscar | Aviso de filtro de ar gerador | 🔵 Avaliar |
| AmandsGraphics | 🔍 | 🔍 buscar | 🔍 buscar | Configurações gráficas avançadas | 🔵 Avaliar |
| aMoxoPixel-Painter | 🔍 | 🔍 buscar | 🔍 buscar | Customização visual (skins/pintura) | 🔵 Avaliar |
| Band-Aid | 🔍 | 🔍 buscar | 🔍 buscar | 🔍 (fix/patch) | 🔵 Avaliar |
| BeltSlot | 🔍 | 🔍 buscar | 🔍 buscar | Slot extra de cinto no inventário | 🔵 Avaliar |
| BetterRearSights | 🔍 | 🔍 buscar | 🔍 buscar | Mira traseira melhorada | 🔵 Avaliar |
| BorkelRNVG | 🔍 | 🔍 buscar | 🔍 buscar | RNVG (night vision) | 🔵 Avaliar |
| BRNVG_N-15Adapter | 🔍 | 🔍 buscar | 🔍 buscar | Adaptador N-15 para BRNVG | 🔵 Avaliar |
| ChooChoo-TraderModding | 🔍 | 🔍 buscar | 🔍 buscar | Modding via traders | 🔵 Avaliar |
| ContinuousLoadAmmo | 🔍 | 🔍 buscar | 🔍 buscar | Carregamento contínuo de munição | 🔵 Avaliar |
| CoordLogger | 🔍 | 🔍 buscar | 🔍 buscar | Logger de coordenadas | 🔵 Avaliar |
| CWX | 🔍 | 🔍 buscar | 🔍 buscar | 🔍 | 🔵 Avaliar |
| DanW-SPTQuestingBots | 🔍 | 🔍 buscar | 🔍 buscar | Bots fazendo quests | 🔵 Avaliar |
| DeadzoneMod | 🔍 | 🔍 buscar | 🔍 buscar | 🔍 (deadzone de mira?) | 🔵 Avaliar |
| desze-UnlockHideoutCustomization | 🔍 | 🔍 buscar | 🔍 buscar | Desbloqueia customização do hideout | 🔵 Avaliar |
| DewardianDev-MOAR | 🔍 | 🔍 buscar | 🔍 buscar | Mais bots/spawns (MOAR) | 🔵 Avaliar |
| dk.SeparateHostility | 🔍 | 🔍 buscar | 🔍 buscar | Separa hostilidade entre facções | 🔵 Avaliar |
| doordash | 🔍 | 🔍 buscar | 🔍 buscar | 🔍 | 🔵 Avaliar |
| DrakiaXYZ-BigBrain | 🔍 | 🔍 buscar (v1.3.2) | 🔍 buscar | Sistema de combat layers (dependência de SAIN) | 🔵 Avaliar |
| DrakiaXYZ-EquipFromWeaponRack | 🔍 | 🔍 buscar | 🔍 buscar | Equipar arma do rack do hideout | 🔵 Avaliar |
| DrakiaXYZ-LootRadius | 🔍 | 🔍 buscar | 🔍 buscar | Aumenta raio de loot | 🔵 Avaliar |
| DrakiaXYZ-QuickMoveToContainer | 🔍 | 🔍 buscar | 🔍 buscar | Mover itens rápido para container | 🔵 Avaliar |
| DrakiaXYZ-SearchOpenContainers | 🔍 | 🔍 buscar | 🔍 buscar | Buscar containers abertos | 🔵 Avaliar |
| DrakiaXYZ-Waypoints | 🔍 | 🔍 buscar (v1.7.1) | 🔍 buscar | Waypoints de patrulha (dependência de SAIN) | 🔵 Avaliar |
| DynamicExternalResolution | 🔍 | 🔍 buscar | 🔍 buscar | Resolução externa dinâmica | 🔵 Avaliar |
| DynamicMaps (SPTDynamicMaps) | 🖥️ Client | 🔍 buscar (v0.5.7) | 🔍 buscar | UI de mapas dinâmicos com tracking de quests | 🔵 Avaliar |
| Eco-Attachment Emporium | 🔍 | 🔍 buscar | 🔍 buscar | Mais attachments para armas | 🔵 Avaliar |
| ExpandedFpsLimit | 🔍 | 🔍 buscar | 🔍 buscar | Aumenta limite de FPS | 🔵 Avaliar |
| Fika (fika-server / Fika.Core) | 🔀 Misto | 🔍 buscar | 🔍 buscar | Multiplayer base (upstream) | 🔵 Avaliar |
| FikaTransitFix (FikaTransitFixServer) | 🌐 Server | 🔍 buscar | 🔍 buscar | Fix de transit em raids do Fika | 🔵 Avaliar |
| FixReloadUltraFika | 🔍 | 🔍 buscar | 🔍 buscar | Fix de reload no UltraFika | 🔵 Avaliar |
| flir-betterkeysng | 🔍 | 🔍 buscar | 🔍 buscar | UI melhorada para keys (next-gen) | 🔵 Avaliar |
| ForceSync | 🔍 | 🔍 buscar | 🔍 buscar | Força sincronização (Fika?) | 🔵 Avaliar |
| FOVFix | 🔍 | 🔍 buscar | 🔍 buscar | Fix de FOV | 🔵 Avaliar |
| gaylatea-deadlyblades | 🔍 | 🔍 buscar | 🔍 buscar | Lâminas mais letais | 🔵 Avaliar |
| Gaylatea-UseLooseLoot | 🔍 | 🔍 buscar | 🔍 buscar | Usa loose loot (loot solto no chão) | 🔵 Avaliar |
| GhostMercenaries | 🔍 | 🔍 buscar | 🔍 buscar | 🔍 (mercenários invisíveis?) | 🔵 Avaliar |
| HandsAreNotBusy | 🔍 | 🔍 buscar | 🔍 buscar | Mãos não ficam ocupadas (animação) | 🔵 Avaliar |
| HollywoodFX | 🔍 | 🔍 buscar | 🔍 buscar | FX visuais cinematográficos | 🔵 Avaliar |
| HollywoodGraphics | 🔍 | 🔍 buscar | 🔍 buscar | Gráficos cinematográficos | 🔵 Avaliar |
| IcyClawz.CustomInteractions | 🔍 | 🔍 buscar | 🔍 buscar | Interações customizadas | 🔵 Avaliar |
| IcyClawz.ItemContextMenuExt | 🔍 | 🔍 buscar | 🔍 buscar | Menu de contexto estendido em itens | 🔵 Avaliar |
| IcyClawz.ItemSellPrice | 🔍 | 🔍 buscar | 🔍 buscar | Mostra preço de venda dos itens | 🔵 Avaliar |
| IcyClawz.MunitionsExpert | 🔍 | 🔍 buscar | 🔍 buscar | Info detalhada de munição | 🔵 Avaliar |
| IdleSprintFix | 🔍 | 🔍 buscar (v1.2.2) | 🔍 buscar | Fix do bug de sprint travado | 🔵 Avaliar |
| IhanaMies-LootValueBackend | 🌐 Server | 🔍 buscar | 🔍 buscar | Backend para cálculo de valor de loot | 🔵 Avaliar |
| inory-agonysfx | 🔍 | 🔍 buscar | 🔍 buscar | SFX de dor/agonia | 🔵 Avaliar |
| JBOBYH_ItemPreviewQoL | 🔍 | 🔍 buscar | 🔍 buscar | QoL de preview de itens | 🔵 Avaliar |
| Jehree-GildedKeyStorage (DrakiaXYZ-GildedKeyStorage) | 🔍 | 🔍 buscar | 🔍 buscar | Storage especializado para keys | 🔵 Avaliar |
| Kaeno-TraderScrolling | 🔍 | 🔍 buscar | 🔍 buscar | Scroll na lista de traders | 🔵 Avaliar |
| Kat.BetterAmmoLoadingList | 🔍 | 🔍 buscar | 🔍 buscar | Lista melhorada de loading de munição | 🔵 Avaliar |
| kmyuhkyuk-EnvironmentReplace | 🔍 | 🔍 buscar | 🔍 buscar | Substitui ambientes/mapas | 🔵 Avaliar |
| kmyuhkyuk-KmyTarkovApi | 🔍 | 🔍 buscar | 🔍 buscar | API utilitária | 🔵 Avaliar |
| lacyway-mergeconsumables (MergeConsumables) | 🔍 | 🔍 buscar | 🔍 buscar | Merge de consumíveis (médicos, comida) | 🔵 Avaliar |
| MoreCheckmarks (MoreCheckmarksBackend) | 🔀 Misto | 🔍 buscar | 🔍 buscar | Mais checkmarks no inventário | 🔵 Avaliar |
| MoxoPixel-MagTape | 🔍 | 🔍 buscar | 🔍 buscar | Visual de tape em magazines | 🔵 Avaliar |
| MoxoPixel-TacticalGearComponent | 🔍 | 🔍 buscar | 🔍 buscar | Componente de equipamento tático | 🔵 Avaliar |
| MusicManiac-LessRestrictingHeadwear | 🔍 | 🔍 buscar | 🔍 buscar | Headwear menos restritivo | 🔵 Avaliar |
| platinum-theblacklist | 🔍 | 🔍 buscar | 🔍 buscar | Blacklist de itens | 🔵 Avaliar |
| PlayerEncumbranceBar | 🔍 | 🔍 buscar | 🔍 buscar | Barra de encumbrance (peso) do player | 🔵 Avaliar |
| Pluto! - SPT Battlepass | 🔍 | 🔍 buscar | 🔍 buscar | Battlepass para SPT | 🔵 Avaliar |
| QuickSell | 🔍 | 🔍 buscar | 🔍 buscar | Venda rápida de itens | 🔵 Avaliar |
| RaiRai.ColorConverterAPI | 🔍 | 🔍 buscar | 🔍 buscar | API utilitária de conversão de cores | 🔵 Avaliar |
| Realism (SPT-Realism / RealismMod) | 🔀 Misto | 🔍 buscar | 🔍 buscar | Overhaul de realismo (balística, médica, etc.) | 🔵 Avaliar |
| redlaser42-Better Headset Descriptions | 🔍 | 🔍 buscar | 🔍 buscar | Descrições melhoradas de headsets | 🔵 Avaliar |
| redlaser42-Increase Climb Height | 🔍 | 🔍 buscar | 🔍 buscar | Aumenta altura máxima de escalada | 🔵 Avaliar |
| SAIN (zSolarint-SAIN-ServerMod) | 🌐 Server | 🔍 buscar | 🔍 buscar | Substituição do sistema de IA dos bots | 🔵 Avaliar |
| seasoniterator | 🔍 | 🔍 buscar | 🔍 buscar | 🔍 (estações/seasonal?) | 🔵 Avaliar |
| shibdib-NoTransitTasks | 🔍 | 🔍 buscar | 🔍 buscar | Remove tasks de transit | 🔵 Avaliar |
| Skwizzy-LootingBots (Skwizzy-LootingBots-ServerMod) | 🌐 Server | 🔍 buscar | 🔍 buscar | Bots fazendo loot | 🔵 Avaliar |
| somtam.NoBush | 🔍 | 🔍 buscar | 🔍 buscar | Remove arbustos densos | 🔵 Avaliar |
| somtam.SimpleDeClutter | 🔍 | 🔍 buscar | 🔍 buscar | Reduz clutter visual | 🔵 Avaliar |
| SPT-FreshContentBackport | 🔍 | 🔍 buscar | 🔍 buscar | Backport de conteúdo novo | 🔵 Avaliar |
| SPT-InsuranceFraud | 🔍 | 🔍 buscar | 🔍 buscar | Mecânica de fraude no seguro | 🔵 Avaliar |
| SPTVRAMCleaner | 🔍 | 🔍 buscar | 🔍 buscar | Limpeza de VRAM | 🔵 Avaliar |
| StashSearch | 🔍 | 🔍 buscar | 🔍 buscar | Busca dentro do stash | 🔵 Avaliar |
| SwiftXP.ShowMeTheMoney | 🔍 | 🔍 buscar | 🔍 buscar | Mostra dinheiro/valores em UI | 🔵 Avaliar |
| TacticalToasterUNTARGH | 🔍 | 🔍 buscar | 🔍 buscar | 🔍 (relacionado a UNTAR/GO HOME?) | 🔵 Avaliar |
| Tarkov Weather System | 🔍 | 🔍 buscar | 🔍 buscar | Sistema de clima dinâmico | 🔵 Avaliar |
| TarkovRedLine (TarkovRedLine-ServerMod) | 🌐 Server | 🔍 buscar | 🔍 buscar | Mod do servidor RedLine (privado?) | 🔵 Avaliar |
| TellTheTime | 🔍 | 🔍 buscar | 🔍 buscar | Mostra hora atual | 🔵 Avaliar |
| Terkoiz.Freecam | 🔍 | 🔍 buscar | 🔍 buscar | Câmera livre (debug/replay) | 🔵 Avaliar |
| tyfon-hideoutinprogress (Tyfon.HideoutInProgress) | 🔍 | 🔍 buscar | 🔍 buscar | Indica progresso no hideout | 🔵 Avaliar |
| tyfon-uifixes (Tyfon.UIFixes / Tyfon.UIFixes.Net) | 🔍 | 🔍 buscar | 🔍 buscar | Coleção de fixes de UI | 🔵 Avaliar |
| tyfon-weaponcustomizer (Tyfon.WeaponCustomizer) | 🔍 | 🔍 buscar | 🔍 buscar | Customizador de armas | 🔵 Avaliar |
| UmbigoPreto-Face the Knight - Mask Fix | 🔍 | 🔍 buscar | 🔍 buscar | Fix da máscara do Knight | 🔵 Avaliar |
| UmbigoPreto-TrueTrauma | 🔍 | 🔍 buscar | 🔍 buscar | Sistema de trauma realista | 🔵 Avaliar |
| Virtual's Custom Quest Loader (VCQLQuestZones) | 🌐 Server | 🔍 buscar | 🔍 buscar | Loader de quests customizadas (VCQL) | 🔵 Avaliar |
| VisceralCombat | 🔍 | 🔍 buscar | 🔍 buscar | Efeitos viscerais de combate | 🔵 Avaliar |
| VolumetricBloodFX | 🔍 | 🔍 buscar | 🔍 buscar | FX de sangue volumétrico | 🔵 Avaliar |
| Wara-ModdingStatsHelper | 🔍 | 🔍 buscar | 🔍 buscar | Helper de stats em modding | 🔵 Avaliar |
| WTT-Armory | 🔍 | 🔍 buscar | 🔍 buscar | Pack de armas (WTT) | 🔵 Avaliar |
| WTT-PackNStrap | 🔍 | 🔍 buscar | 🔍 buscar | Pack de equipamento (WTT) | 🔵 Avaliar |
| yellowdoge-tarkovrarecollectibles | 🔍 | 🔍 buscar | 🔍 buscar | Itens raros colecionáveis | 🔵 Avaliar |
| zzDrakiaXYZ-LiveFleaPrices | 🔍 | 🔍 buscar | 🔍 buscar | Preços do flea ao vivo (live data) | 🔵 Avaliar |

## Utilitários / pastas

Itens listados que **não são mods** propriamente ditos — são pastas ou utilitários de suporte. Avaliar caso a caso se fazem sentido no novo repo ou se viram parte da infra (`.agents/`, scripts, etc.).

| Item | Tipo | Observação |
|---|---|---|
| `spt` | 📁 Pasta | Avaliar conteúdo — pode ser config ou scripts |
| `ssh` | 📁 Pasta | Provavelmente chaves/config SSH — **NÃO versionar segredos** |
| `tarkin` | 📁 Pasta | Avaliar conteúdo |

## Status disponíveis

- 🔵 **Avaliar** — ainda não decidido
- 🟢 **Portar** — adaptar código existente do 3.x para 4.0
- 🔧 **Desenvolver** — criar do zero no 4.0 (autor não lançou e não dá pra portar)
- 🟠 **Aguardar upstream** — esperando autor original lançar versão 4.0
- 🔴 **Bloqueado** — incompatibilidade arquitetural sem workaround conhecido
- ⚫ **Não incluir** — fora do escopo do projeto

## Próximos passos

1. **Classificar tipo (🔍 → 🖥️ / 🌐 / 🔀):** abrir cada repo e identificar se é Client (C#/BepInEx), Server (TypeScript/JS) ou Misto
2. **Preencher Link 3.x** de cada mod (SPT Hub, GitHub, etc.)
3. **Preencher Link 4.x** — quando ausente, decidir entre `🔧 Desenvolver`, `🟠 Aguardar upstream` ou `⚫ Não incluir`
4. **Validar funções:** corrigir os `🔍` na coluna Função após inspecionar o repo de cada mod
5. **Identificar dependências entre mods** (ex: SAIN ↔ BigBrain ↔ Waypoints) e adicionar coluna ou notas
6. **Identificar dependências de Assembly-CSharp** que mudaram entre 3.x e 4.0
7. Criar specs individuais em `docs/migration/<mod-name>/` para os que serão portados ou desenvolvidos
8. Atualizar este inventário conforme decisões forem tomadas

## Histórico

| Data | Autor | Descrição |
|---|---|---|
| 2026-05-02 | Guilherme | +49 / -0 linhas |
| 2026-05-02 | Guilherme | +15 / -7 linhas |
| 2026-05-03 | Guilherme | +32 / -17 linhas |
| 2026-05-03 | Guilherme | +130 / -26 linhas |
