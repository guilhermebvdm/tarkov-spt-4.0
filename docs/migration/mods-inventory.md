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

- **#:** contador sequencial (apenas para referência cruzada).
- **Origem:** mods criados pela equipe são prefixados com `🏠` no nome (ex: `🏠 IdleSprintFix`). Demais são da comunidade.
- **Tipo:** 🖥️ Client (C# / BepInEx) · 🌐 Server (TypeScript/JS) · 🔀 Misto (ambos) · 🔍 a classificar
- **Atuação:** ver enum "Atuação" abaixo (onde o mod atua: hideout, raid, geral, ambos)
- **Categoria:** intenção dominante do mod — ver enum "Categoria" abaixo (escolher 1).
- **Escopo:** sistemas do jogo afetados — ver enum "Escopo" abaixo (1–2 valores separados por `·`; se 3+ usar 🧩 **Framework**).
- **Forge:** URL no [forge.sp-tarkov.com](https://forge.sp-tarkov.com/mods) (1 página por mod, lista todas as versões). Formato: `[id](url)` · `🔍` (a buscar) · `—` (confirmado ausente)
- **Repo 3.x:** URL do repositório fonte na versão SPT 3.x (último release pré-4.0). Aceita GitHub, GitLab ou similar. Formato: `[autor/repo](url)` · `🔍` (a buscar) · `—` (não há repo público)
- **Repo 4.0:** URL do repositório fonte na versão SPT 4.0+. Pode ser repo separado (autor manteve nomes diferentes), branch/tag específica, ou mesmo repo do 3.x se autor migrou no mesmo. Aceita GitHub, GitLab, etc. Formato: `[autor/repo](url)` · `🔍` · `—` (autor não publicou 4.0)
- **Status:** ver enum "Status disponíveis" abaixo. **Notas e aliases** (forks, autor, dependências) vão na coluna **Função**, não em outras colunas.
- **Prioridade:** ver enum "Prioridades" abaixo.

## Status disponíveis

- 🟡 **Avaliar** — ainda não decidido (default ao adicionar mod novo)
- 🟢 **À Instalar** — versão 4.0 **já existe** publicamente; basta baixar e instalar (sem trabalho de código)
- ⬆️ **Evoluir p/ 4.0** — adaptar/refatorar código 3.x existente para 4.0 (envolve refactor e evolução, não criação do zero). Padrão para mods internos `🏠` que temos código.
- 🔧 **Desenvolver** — criar do zero no 4.0 (autor original não lançou; decidimos antecipar fazendo do nosso lado)
- 🟠 **Aguardar upstream** — autor original ainda não lançou 4.0; default quando 4.x = ❌. Pode virar `🔧 Desenvolver` se decidirmos antecipar.
- 🔴 **Bloqueado** — incompatibilidade arquitetural sem workaround conhecido
- ⚫ **Não incluir** — fora do escopo do projeto

### Fluxo de decisão

```
4.x existe publicamente?
├─ ✅ Sim                     → 🟢 À Instalar
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

## Atuação

Onde o mod atua dentro do jogo:

- 🏚️ **Hideout** — específico do hideout (workout, customization, geradores, racks)
- ⚔️ **Raid** — específico de gameplay em raid (IA, FOV, FX, NVG, weather, mapas, animações)
- 🔀 **Ambos** — afeta tanto hideout quanto raid (ex: inventário/stash usado em ambos, multiplayer base)
- 🌐 **Geral** — não-específico de hideout ou raid (UI global, traders, flea, perfil, conteúdo de itens, frameworks)
- 🔍 — a categorizar (default)

## Categoria

Intenção dominante do mod (escolher **1**):

- 🛋️ **Quality of Life** — conveniência, reduz fricção (QuickMove, SearchOpenContainers, MoreTagColours)
- 🩸 **Realismo** — aproxima de comportamento real (balística, médica, IA, NVG, deadzone)
- 🔥 **Hardcore** — aumenta dificuldade/punição (DelayedFleaSales, SeparateHostility)
- ⚖️ **Balanceamento** — ajusta valores/economia sem mudar sistemas (SVM, LiveFleaPrices, blacklist)
- 📈 **Progressão** — altera ritmo de evolução do personagem/hideout (battlepass, skill rate)
- ➕ **Conteúdo** — adiciona itens/quests/traders/mapas novos (WTT, Eco-Attachment, Painter)
- 🧩 **Framework/Base** — infra para outros mods (BigBrain, Waypoints, Fika, KmyTarkovApi)
- 🎨 **Cosmético/Visual** — só estética, sem gameplay (HollywoodFX, hideoutcat, MagTape)
- 🔍 — a categorizar (default)

## Escopo

Sistemas do jogo afetados (1–2 valores separados por `·`; se 3+ usar **🧩 Framework**):

- 🤖 **IA/Bots** — comportamento, spawns, looting de bots
- 🎒 **Loot** — loot tables, loose loot, container loot
- 🖼️ **UI** — interface, menus, tooltips, ícones
- 🗺️ **Mapas** — geometria, navmesh, weather, ambiente
- 🔫 **Armas** — comportamento de armas, attachments, customização
- 🎯 **Munições** — balística, info, loading
- 🛡️ **Equipamentos** — armor, rig, headwear, NVG, médico vestível
- 💰 **Mercado** — traders, flea market, preços, vendas
- 📜 **Quests** — tarefas, transit, conteúdo de questline
- 🏚️ **Hideout** — sistemas do hideout (workout, customization, racks, geradores)
- 📊 **Progressão** — skills, XP, leveling
- ✨ **Gráficos/FX** — gráficos, pós-processamento, FOV, FX, áudio ambiental
- 🌐 **Multiplayer** — sincronização, rede, multiplayer base
- 📦 **Inventário** — stash, slots, organização de itens
- 🎬 **Animações** — animações de personagem (sprint, climb, mãos)
- 🧩 **Framework** — afeta múltiplos sistemas ou serve de base para outros mods
- 🔍 — a categorizar (default)

## Inventário completo

| # | Mod | Tipo | Atuação | Categoria | Escopo | Forge | Repo 3.x | Repo 4.0 | SPT 4.0? | Função | Status | Prioridade | TRL 3.0? | Instalado |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | [SVM] Server Value Modifier | 🌐 Server | 🌐 Geral | ⚖️ Balanceamento | 🧩 Framework | [236](https://forge.sp-tarkov.com/mod/236/server-value-modifier-svm) | [GhostFenixx/SVM](https://github.com/GhostFenixx/SVM) | [GhostFenixx/svm-csharp](https://github.com/GhostFenixx/svm-csharp) | ✅ | Modifica valores do servidor (loot, traders, hideout) — último release SPT 3.11 | 🟠 Aguardar upstream | 🔍 | Sim | ✓ |
| 2 | AAAArtem-WTT | 🖥️ Client | 🌐 Geral | 🔍 | 🔍 | [1023](https://forge.sp-tarkov.com/mod/1023/wtt-artem) | [WelcomeToTarkov/WTT-Artem](https://github.com/WelcomeToTarkov/WTT-Artem) | [WelcomeToTarkov/WTT-Artem](https://github.com/WelcomeToTarkov/WTT-Artem) | ✅ | 🔍 (relacionado ao WTT) | 🟡 Avaliar | 🔍 | Sim | — |
| 3 | acidphantasm-DelayedFleaSales | 🖥️ Client | 🌐 Geral | 🔥 Hardcore | 💰 Mercado | [2016](https://forge.sp-tarkov.com/mod/2016/delayed-flea-sales) | [acidphantasm/acidphantasm-delayedfleasales](https://github.com/acidphantasm/acidphantasm-delayedfleasales) | [acidphantasm/delayedfleasales-csharp](https://github.com/acidphantasm/delayedfleasales-csharp) | ✅ | Atrasa vendas no flea market (era Server/TS em 3.x, reescrito como BepInEx C# para 4.0) | ⚫ Não incluir | 🔍 | Sim | — |
| 4 | acidphantasm-moretagcolours | 🖥️ Client | 🌐 Geral | 🛋️ QoL | 🖼️ UI | [1533](https://forge.sp-tarkov.com/mod/1533/more-tag-colours) | [acidphantasm/acidphantasm-moretagcolours](https://github.com/acidphantasm/acidphantasm-moretagcolours) | [acidphantasm/acidphantasm-moretagcolours](https://github.com/acidphantasm/acidphantasm-moretagcolours) | ✅ | Mais cores para tags de itens | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 5 | acidphantasm-previewsizer | 🖥️ Client | 🌐 Geral | 🛋️ QoL | 🖼️ UI | [2339](https://forge.sp-tarkov.com/mod/2339/preview-sizer) | [acidphantasm/acidphantasm-previewsizer](https://github.com/acidphantasm/acidphantasm-previewsizer) | [acidphantasm/acidphantasm-previewsizer](https://github.com/acidphantasm/acidphantasm-previewsizer) | ✅ | Redimensiona preview de itens | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 6 | acidphantasm-progressivebotsystem | 🖥️ Client | ⚔️ Raid | ⚖️ Balanceamento | 🤖 IA | [1594](https://forge.sp-tarkov.com/mod/1594/apbs-acids-progressive-bot-system) | [acidphantasm/acidphantasm-apbs](https://github.com/acidphantasm/acidphantasm-apbs) | [acidphantasm/progressivebotsystem-csharp](https://github.com/acidphantasm/progressivebotsystem-csharp) | ✅ | Sistema progressivo de bots (era Server/TS em 3.x, reescrito como BepInEx C# para 4.0) | 🟢 À Instalar | 🔍 | Sim | ✓ |
| 7 | acidphantasm-refsptfriendlyquests | 🖥️ Client | 🌐 Geral | ⚖️ Balanceamento | 📜 Quests | [1538](https://forge.sp-tarkov.com/mod/1538/ref-spt-friendly-quests) | [acidphantasm/acidphantasm-refchanges](https://github.com/acidphantasm/acidphantasm-refchanges) | [acidphantasm/reffriendlyquests-csharp](https://github.com/acidphantasm/reffriendlyquests-csharp) | ✅ | Quests amigáveis (compatível com Ref — era Server/TS em 3.x, reescrito como BepInEx C# para 4.0) | 🟢 À Instalar | 🔍 | Sim | ✓ |
| 8 | acidphantasm-simpleworkoutqte | 🌐 Server | 🏚️ Hideout | 🛋️ QoL | 🏚️ Hideout | [1437](https://forge.sp-tarkov.com/mod/1437/simple-workout-qte) | [acidphantasm/acidphantasm-simpleworkoutqte](https://github.com/acidphantasm/acidphantasm-simpleworkoutqte) | [acidphantasm/acidphantasm-simpleworkoutqte](https://github.com/acidphantasm/acidphantasm-simpleworkoutqte) | ✅ | QTE de workout no hideout | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 9 | AirFilterWarning | 🖥️ Client | 🏚️ Hideout | 🛋️ QoL | 🏚️ Hideout | [2129](https://forge.sp-tarkov.com/mod/2129/air-filter-warning) | [danx91/AirFilterWarning](https://github.com/danx91/AirFilterWarning) | [danx91/AirFilterWarning](https://github.com/danx91/AirFilterWarning) | ✅ | Aviso de filtro de ar gerador | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 10 | AmandsGraphics | 🖥️ Client | ⚔️ Raid | 🎨 Cosmético | ✨ Gráficos | [592](https://forge.sp-tarkov.com/mod/592/amandss-graphics) | [Amands2Mello/AmandsGraphics](https://github.com/Amands2Mello/AmandsGraphics) | — | ❌ | Configurações gráficas avançadas — último release SPT 3.10 | 🟠 Aguardar upstream | 🔍 | Sim | — |
| 11 | aMoxoPixel-Painter | 🌐 Server | 🌐 Geral | ➕ Conteúdo | 💰 Mercado · 🔫 Armas | [1025](https://forge.sp-tarkov.com/mod/1025/painter) | [emilanderss0n/Painter](https://github.com/emilanderss0n/Painter) | [emilanderss0n/Painter](https://github.com/emilanderss0n/Painter) | ✅ | Trader que vende mods de armas pintados | 🟢 À Instalar | 🔍 | Sim | — |
| 12 | BeltSlot | 🖥️ Client | 🔀 Ambos | 🛋️ QoL | 📦 Inventário · 🛡️ Equipamentos | [2181](https://forge.sp-tarkov.com/mod/2181/belt-slot) | [Trench-foot/BeltSlot](https://github.com/Trench-foot/BeltSlot) | [Trench-foot/BeltSlot](https://github.com/Trench-foot/BeltSlot) | ❌ | Slot extra de cinto no inventário — último release v1.0.1 (SPT 3.11.4, jul/2025); sem build 4.0 | ⚫ Não incluir | 🔍 | Sim | — |
| 13 | BetterRearSights | 🖥️ Client | ⚔️ Raid | 🩸 Realismo | 🔫 Armas | [1591](https://forge.sp-tarkov.com/mod/1591/better-rear-sights) | [peinwastaken/PeinBetterRearSights](https://github.com/peinwastaken/PeinBetterRearSights) | [peinwastaken/PeinBetterRearSights](https://github.com/peinwastaken/PeinBetterRearSights) | ✅ | Mira traseira melhorada | ⚫ Não incluir | 🔍 | Sim | — |
| 14 | BorkelRNVG | 🖥️ Client | ⚔️ Raid | 🩸 Realismo | 🛡️ Equipamentos | [954](https://forge.sp-tarkov.com/mod/954/borkels-realistic-night-vision-goggles-nvgs-and-t-7) | [Borkel/RealisticNVG-client-2](https://github.com/Borkel/RealisticNVG-client-2) | [Borkel/RealisticNVG-client-2](https://github.com/Borkel/RealisticNVG-client-2) | ✅ | NVGs realistas com máscaras + luz natural — v2.1.1 (SPT 4.0.13). Mesmo repo serve 3.x e 4.0 (versão por release) | 🟢 À Instalar | 🔍 | Sim | ✓ |
| 15 | BRNVG_N-15Adapter | 🖥️ Client | ⚔️ Raid | 🩸 Realismo | 🛡️ Equipamentos | [954](https://forge.sp-tarkov.com/mod/954/borkels-realistic-night-vision-goggles-nvgs-and-t-7) | — | [Borkel/RealisticNVG-client-2](https://github.com/Borkel/RealisticNVG-client-2) | ✅ | Adaptador N-15 para BRNVG — incluído no pacote BorkelRNVG v2.1.1 (#14), SPT 4.0.13 | 🟢 Instalar | 🔍 | Sim | ✓ |
| 16 | ChooChoo-TraderModding | 🔀 Misto | 🌐 Geral | 🛋️ QoL | 💰 Mercado · 🔫 Armas | [1283](https://forge.sp-tarkov.com/mod/1283/trader-modding-and-improved-weapon-building) | [Soulztorm/ChooChoo-TraderModding](https://github.com/Soulztorm/ChooChoo-TraderModding) | [Soulztorm/ChooChoo-TraderModding](https://github.com/Soulztorm/ChooChoo-TraderModding) | ✅ | Modding via traders | ⚫ Não incluir | 🔍 | Sim | — |
| 17 | ContinuousLoadAmmo | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | 🎯 Munições | [2112](https://forge.sp-tarkov.com/mod/2112/continuous-load-ammo) | [ozen-m/SPT-ContinuousLoadAmmo](https://github.com/ozen-m/SPT-ContinuousLoadAmmo) | [ozen-m/SPT-ContinuousLoadAmmo](https://github.com/ozen-m/SPT-ContinuousLoadAmmo) | ✅ | Carregamento contínuo de munição | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 18 | CWX | 🖥️ Client | 🌐 Geral | ➕ Conteúdo | 🧩 Framework | [1454](https://forge.sp-tarkov.com/mod/1454/cwx-megamod) | [CWXDEV/CWX-Mods](https://github.com/CWXDEV/CWX-Mods) | [CWXDEV/CWX-Mods](https://github.com/CWXDEV/CWX-Mods) | ✅ | Coleção de mods do CWX (CWX-MegaMod) | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 19 | DanW-SPTQuestingBots | 🌐 Server | ⚔️ Raid | 🩸 Realismo | 🤖 IA | [1109](https://forge.sp-tarkov.com/mod/1109/questing-bots) | [dwesterwick/SPTQuestingBots](https://github.com/dwesterwick/SPTQuestingBots) | [dwesterwick/SPTQuestingBots](https://github.com/dwesterwick/SPTQuestingBots) | ✅ | Bots fazendo quests + spawns PMC mimic live | ⚫ Não incluir | 🔍 | Sim | — |
| 20 | DeadzoneMod | 🖥️ Client | ⚔️ Raid | 🩸 Realismo | 🔫 Armas | [1001](https://forge.sp-tarkov.com/mod/1001/deadzone) | [lualeet/sptarkov-deadzone](https://github.com/lualeet/sptarkov-deadzone) | — | ❌ | Deadzone/free aim style insurgency | ⚫ Não incluir | 🔍 | Sim | — |
| 21 | desze-UnlockHideoutCustomization | 🌐 Server | 🏚️ Hideout | 🛋️ QoL | 🏚️ Hideout | [2053](https://forge.sp-tarkov.com/mod/2053/unlock-all-hideout-customization) | [desze7/hideout-unlock-all](https://github.com/desze7/hideout-unlock-all) | — | ❌ | Desbloqueia customização do hideout | 🟠 Aguardar upstream | 🔍 | Sim | — |
| 22 | DewardianDev-MOAR | 🌐 Server | ⚔️ Raid | ⚖️ Balanceamento | 🤖 IA | [789](https://forge.sp-tarkov.com/mod/789/moar-bagels-ultra-lite-spawn-mod) | [Andrewgdewar/MOAR](https://github.com/Andrewgdewar/MOAR) | [Andrewgdewar/MOAR](https://github.com/Andrewgdewar/MOAR) | ✅ | Bot spawning system | 🟠 Aguardar upstream | 🔍 | Sim | — |
| 23 | dk.SeparateHostility | 🖥️ Client | ⚔️ Raid | 🩸 Realismo | 🤖 IA | [2248](https://forge.sp-tarkov.com/mod/2248/separate-hostility) | [gottyduke/SeparateHostility](https://github.com/gottyduke/SeparateHostility) | — | ❌ | Separa hostilidade entre facções | ⚫ Não incluir | 🔍 | Sim | — |
| 24 | doordash | 🖥️ Client | ⚔️ Raid | 🩸 Realismo | 🗺️ Mapas | [2214](https://forge.sp-tarkov.com/mod/2214/doordash) | [bmpq/spt-doordash](https://github.com/bmpq/spt-doordash) | [bmpq/spt-doordash](https://github.com/bmpq/spt-doordash) | ✅ | Breaching de portas trancadas (SPT 4.0) | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 25 | DrakiaXYZ-BigBrain | 🖥️ Client | ⚔️ Raid | 🧩 Framework | 🤖 IA | [902](https://forge.sp-tarkov.com/mod/902/bigbrain) | [DrakiaXYZ/SPT-BigBrain](https://github.com/DrakiaXYZ/SPT-BigBrain) | [DrakiaXYZ/SPT-BigBrain](https://github.com/DrakiaXYZ/SPT-BigBrain) | ✅ | Library de combat layers para bots (dep. de SAIN) — v1.4.0 (SPT 4.0.13). Mesmo repo, versionado por tags | 🟢 À Instalar | 🔍 | Sim | ✓ |
| 26 | DrakiaXYZ-EquipFromWeaponRack | 🖥️ Client | 🏚️ Hideout | 🛋️ QoL | 🏚️ Hideout · 🔫 Armas | [1136](https://forge.sp-tarkov.com/mod/1136/equip-from-weapon-rack) | [DrakiaXYZ/SPT-EquipFromWeaponRack](https://github.com/DrakiaXYZ/SPT-EquipFromWeaponRack) | [DrakiaXYZ/SPT-EquipFromWeaponRack](https://github.com/DrakiaXYZ/SPT-EquipFromWeaponRack) | ✅ | Equipar arma direto do rack do hideout | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 27 | DrakiaXYZ-LootRadius | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | 🎒 Loot | [1349](https://forge.sp-tarkov.com/mod/1349/loot-radius) | [DrakiaXYZ/SPT-LootRadius](https://github.com/DrakiaXYZ/SPT-LootRadius) | — | ❌ | Aumenta raio de loot | 🟠 Aguardar upstream | 🔍 | Sim | — |
| 28 | DrakiaXYZ-QuickMoveToContainer | 🖥️ Client | 🔀 Ambos | 🛋️ QoL | 📦 Inventário | [1341](https://forge.sp-tarkov.com/mod/1341/quick-move-to-containers) | [DrakiaXYZ/SPT-QuickMoveToContainer](https://github.com/DrakiaXYZ/SPT-QuickMoveToContainer) | [DrakiaXYZ/SPT-QuickMoveToContainer](https://github.com/DrakiaXYZ/SPT-QuickMoveToContainer) | ✅ | Ctrl+Click move item para container aberto | ⚫ Não incluir | 🔍 | Sim | — |
| 29 | DrakiaXYZ-SearchOpenContainers | 🖥️ Client | 🔀 Ambos | 🛋️ QoL | 📦 Inventário | [934](https://forge.sp-tarkov.com/mod/934/search-open-containers) | [DrakiaXYZ/SPT-SearchOpenContainers](https://github.com/DrakiaXYZ/SPT-SearchOpenContainers) | [DrakiaXYZ/SPT-SearchOpenContainers](https://github.com/DrakiaXYZ/SPT-SearchOpenContainers) | ✅ | Buscar dentro de containers abertos | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 30 | DrakiaXYZ-Waypoints | 🖥️ Client | ⚔️ Raid | 🧩 Framework | 🤖 IA · 🗺️ Mapas | [827](https://forge.sp-tarkov.com/mod/827/waypoints-expanded-navmesh) | [DrakiaXYZ/SPT-Waypoints](https://github.com/DrakiaXYZ/SPT-Waypoints) | [DrakiaXYZ/SPT-Waypoints](https://github.com/DrakiaXYZ/SPT-Waypoints) | ✅ | Expande navmesh dos mapas (dep. de SAIN) — v1.8.2 (SPT 4.0.13). Mesmo repo, versionado por tags | 🟢 À Instalar | 🔍 | Sim | ✓ |
| 31 | DynamicExternalResolution | 🖥️ Client | 🌐 Geral | 🛋️ QoL | ✨ Gráficos | [929](https://forge.sp-tarkov.com/mod/929/dynamic-external-resolution-patch-derp) | [MrFlashMode/SPT-Dynamic-External-Resolution](https://github.com/MrFlashMode/SPT-Dynamic-External-Resolution) | — | ❌ | Resolução externa dinâmica (DERP) | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 32 | DynamicMaps (SPTDynamicMaps) | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | 🖼️ UI · 🗺️ Mapas | [1431](https://forge.sp-tarkov.com/mod/1431/dynamic-maps) | [mpstark/SPT-DynamicMaps](https://github.com/mpstark/SPT-DynamicMaps) | [mpstark/SPT-DynamicMaps](https://github.com/mpstark/SPT-DynamicMaps) | ✅ | UI custom de mapas com tracking de quests | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 33 | Eco-Attachment Emporium | 🌐 Server | 🌐 Geral | ➕ Conteúdo | 🔫 Armas | [2288](https://forge.sp-tarkov.com/mod/2288/ecos-attachment-emporium) | [Eco9341/tarkovMods](https://github.com/Eco9341/tarkovMods) | [Eco9341/tarkovMods](https://github.com/Eco9341/tarkovMods) | ✅ | Mais attachments para armas | 🟢 À Instalar | 🔍 | Sim | — |
| 34 | ExpandedFpsLimit | 🖥️ Client | 🌐 Geral | 🛋️ QoL | ✨ Gráficos | [2066](https://forge.sp-tarkov.com/mod/2066/expanded-fps-limit) | [Mugnum/SPT_ExpandedFpsLimit](https://github.com/Mugnum/SPT_ExpandedFpsLimit) | [Mugnum/SPT_ExpandedFpsLimit](https://github.com/Mugnum/SPT_ExpandedFpsLimit) | ✅ | Aumenta limite de FPS (até 360) | ⚫ Não incluir | 🔍 | Sim | — |
| 35 | Fika | 🔀 Misto | 🔀 Ambos | 🧩 Framework | 🌐 Multiplayer | [2326](https://forge.sp-tarkov.com/mod/2326/project-fika) | [project-fika/Fika-Plugin](https://github.com/project-fika/Fika-Plugin) | [project-fika/Fika-Plugin](https://github.com/project-fika/Fika-Plugin) | ✅ | Multiplayer base (BepInEx + server) — v2.2.5 (SPT 4.0.13). Mesmo repo. Aliases: `fika-server`, `Fika.Core` | 🟢 À Instalar | 🔍 | Sim | ✓ |
| 36 | flir-betterkeysng | 🖥️ Client | 🌐 Geral | 🛋️ QoL | 🖼️ UI | [1888](https://forge.sp-tarkov.com/mod/1888/better-keys-ng) | [flir063-spt/betterkeys-ng](https://gitlab.com/flir063-spt/betterkeys-ng) | [flir063-spt/betterkeys-ng](https://gitlab.com/flir063-spt/betterkeys-ng) | ✅ | UI melhorada para keys — autor `flir` (mesmo de Tarkov Weather System); provável Client | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 37 | FOVFix | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | ✨ Gráficos | [701](https://forge.sp-tarkov.com/mod/701/fontaines-fov-fix) | [space-commits/SPT-FOV-Fix](https://github.com/space-commits/SPT-FOV-Fix) | [space-commits/SPT-FOV-Fix](https://github.com/space-commits/SPT-FOV-Fix) | ✅ | Fix de FOV (Fontaine's FOV Fix) — releases para SPT 4.x | 🟢 À Instalar | 🔍 | Sim | ✓ |
| 38 | gaylatea-deadlyblades | 🌐 Server | ⚔️ Raid | ⚖️ Balanceamento | 🔫 Armas | [819](https://forge.sp-tarkov.com/mod/819/deadly-blades) | [silversupreme/SPT-DeadlyBlades](https://github.com/silversupreme/SPT-DeadlyBlades) | — | ❌ | Lâminas mais letais | ⬆️ Evoluir p/ 4.0 | 🔍 | Sim | — |
| 39 | Gaylatea-UseLooseLoot | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | 🎒 Loot | [933](https://forge.sp-tarkov.com/mod/933/use-loose-loot) | [DrakiaXYZ/SPT-UseLooseLoot](https://github.com/DrakiaXYZ/SPT-UseLooseLoot) | [DrakiaXYZ/SPT-UseLooseLoot](https://github.com/DrakiaXYZ/SPT-UseLooseLoot) | ✅ | Usa loose loot direto sem entrar no inventário | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 40 | HandsAreNotBusy | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | 🎬 Animações | [1298](https://forge.sp-tarkov.com/mod/1298/handsarenotbusy) | [Lacyway/HandsAreNotBusy](https://github.com/Lacyway/HandsAreNotBusy) | [Lacyway/HandsAreNotBusy](https://github.com/Lacyway/HandsAreNotBusy) | ✅ | Mãos não ficam ocupadas (animação) | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 41 | hideoutcat | 🖥️ Client | 🏚️ Hideout | 🎨 Cosmético | 🏚️ Hideout | [2038](https://forge.sp-tarkov.com/mod/2038/hideout-cat) | [bmpq/spt-hideoutcat](https://github.com/bmpq/spt-hideoutcat) | — | ❌ | Gato no hideout (5 texturas configuráveis) | 🟠 Aguardar upstream | 🔍 | Sim | — |
| 42 | HollywoodFX | 🖥️ Client | ⚔️ Raid | 🎨 Cosmético | ✨ Gráficos | [2003](https://forge.sp-tarkov.com/mod/2003/hollywoodfx) | [SleepingPills/HollywoodFX](https://github.com/SleepingPills/HollywoodFX) | [SleepingPills/HollywoodFX](https://github.com/SleepingPills/HollywoodFX) | ✅ | FX cinematográficos (impactos, blood) — v1.8.4 (SPT 4.0.13). Autor: JankyTheClown / SleepingPills. Mesmo repo | 🟢 À Instalar | 🔍 | Sim | ✓ |
| 43 | HollywoodGraphics | 🖥️ Client | ⚔️ Raid | 🎨 Cosmético | ✨ Gráficos | [2003](https://forge.sp-tarkov.com/mod/2003/hollywoodfx) | — | [SleepingPills/HollywoodFX](https://github.com/SleepingPills/HollywoodFX) | ✅ | Gráficos cinematográficos — incluído no pacote HollywoodFX v1.8.4 (#42), SPT 4.0.13 | 🟢 Instalar | 🔍 | Sim | ✓ |
| 44 | IcyClawz.CustomInteractions | 🖥️ Client | 🔀 Ambos | 🛋️ QoL | 🖼️ UI | [938](https://forge.sp-tarkov.com/mod/938/custom-interactions) | [IgorEisberg/SPT-ClientMods](https://github.com/IgorEisberg/SPT-ClientMods) | [IgorEisberg/SPT-ClientMods](https://github.com/IgorEisberg/SPT-ClientMods) | ✅ | Interações customizadas — perfil do autor: hub.sp-tarkov.com/user/34778-icyclawz (autor IgorEisberg) | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 45 | IcyClawz.ItemContextMenuExt | 🖥️ Client | 🔀 Ambos | 🛋️ QoL | 🖼️ UI | [940](https://forge.sp-tarkov.com/mod/940/item-context-menu-extended) | [IgorEisberg/SPT-ClientMods](https://github.com/IgorEisberg/SPT-ClientMods) | [IgorEisberg/SPT-ClientMods](https://github.com/IgorEisberg/SPT-ClientMods) | ✅ | Menu de contexto estendido em itens (SPT 4.0.13, 18.1K downloads). Repo monorepo do autor | 🟢 À Instalar | 🔍 | Sim | ✓ |
| 46 | IcyClawz.ItemSellPrice | 🖥️ Client | 🌐 Geral | 🛋️ QoL | 💰 Mercado · 🖼️ UI | [909](https://forge.sp-tarkov.com/mod/909/item-sell-price) | [IgorEisberg/SPT-ClientMods](https://github.com/IgorEisberg/SPT-ClientMods) | [IgorEisberg/SPT-ClientMods](https://github.com/IgorEisberg/SPT-ClientMods) | ✅ | Preços de venda em todos os traders (SPT 4.0.13, 81.5K downloads). Repo monorepo do autor | ⚫ Não incluir | 🔍 | Sim | — |
| 47 | IcyClawz.MunitionsExpert | 🖥️ Client | 🌐 Geral | 🛋️ QoL | 🎯 Munições · 🖼️ UI | [972](https://forge.sp-tarkov.com/mod/972/munitions-expert-reboot) | [IgorEisberg/SPT-ClientMods](https://github.com/IgorEisberg/SPT-ClientMods) | [IgorEisberg/SPT-ClientMods](https://github.com/IgorEisberg/SPT-ClientMods) | ✅ | Info detalhada de munição — perfil do autor: hub.sp-tarkov.com/user/34778-icyclawz (autor IgorEisberg) | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 48 | IhanaMies-LootValueBackend | 🖥️ Client | 🔀 Ambos | 🛋️ QoL | 🎒 Loot · 🖼️ UI | [1155](https://forge.sp-tarkov.com/mod/1155/lootvalue) | [IhanaMies/LootValue](https://github.com/IhanaMies/LootValue) | — | ❌ | Mostra valor de loot na UI — último release SPT 3.11 | ⚫ Não incluir | 🔍 | Sim | — |
| 49 | inory-agonysfx | 🖥️ Client | ⚔️ Raid | 🩸 Realismo | ✨ Gráficos | [1831](https://forge.sp-tarkov.com/mod/1831/agony-sfx) | [ppowa/agonysfx](https://github.com/ppowa/agonysfx) | [ppowa/agonysfx](https://github.com/ppowa/agonysfx) | ✅ | SFX de dor/agonia | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 50 | JBOBYH_ItemPreviewQoL | 🖥️ Client | 🌐 Geral | 🛋️ QoL | 🖼️ UI | [2206](https://forge.sp-tarkov.com/mod/2206/item-preview-qol-screenshots) | [jbobyh/JBOBYH_ItemPreviewQoL](https://github.com/jbobyh/JBOBYH_ItemPreviewQoL) | [jbobyh/JBOBYH_ItemPreviewQoL](https://github.com/jbobyh/JBOBYH_ItemPreviewQoL) | ✅ | QoL de preview de itens | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 51 | Jehree-GildedKeyStorage | 🖥️ Client | 🔀 Ambos | 🛋️ QoL | 📦 Inventário | [865](https://forge.sp-tarkov.com/mod/865/gilded-key-storage) | [Jehree/SPT-Gilded_Key_Storage](https://github.com/Jehree/SPT-Gilded_Key_Storage) | [DrakiaXYZ/SPT-GildedKeyStorage-CSharp](https://github.com/DrakiaXYZ/SPT-GildedKeyStorage-CSharp) | ✅ | Storage especializado para keys — original do Jehree (3.x), fork C# do DrakiaXYZ é o de 4.0 | 🟢 À Instalar | 🔍 | Sim | ✓ |
| 52 | Kaeno-TraderScrolling | 🖥️ Client | 🌐 Geral | 🛋️ QoL | 💰 Mercado · 🖼️ UI | [1089](https://forge.sp-tarkov.com/mod/1089/kaeno-traderscrolling) | [CWXDEV/Kaeno-TraderScrolling](https://github.com/CWXDEV/Kaeno-TraderScrolling) | [CWXDEV/Kaeno-TraderScrolling](https://github.com/CWXDEV/Kaeno-TraderScrolling) | ✅ | Scroll na lista de traders | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 53 | Kat.BetterAmmoLoadingList | 🖥️ Client | 🔀 Ambos | 🛋️ QoL | 🎯 Munições · 🖼️ UI | [2221](https://forge.sp-tarkov.com/mod/2221/ball-better-ammo-loading-list) | [Katrin0522/SPT-BetterAmmoLoadingList](https://github.com/Katrin0522/SPT-BetterAmmoLoadingList) | [Katrin0522/SPT-BetterAmmoLoadingList](https://github.com/Katrin0522/SPT-BetterAmmoLoadingList) | ✅ | Lista melhorada de loading de munição | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 54 | kmyuhkyuk-EnvironmentReplace | 🖥️ Client | ⚔️ Raid | ➕ Conteúdo | 🗺️ Mapas | [1371](https://forge.sp-tarkov.com/mod/1371/environment-replace) | [minihazel/EnvironmentReplace](https://github.com/minihazel/EnvironmentReplace) | [minihazel/EnvironmentReplace](https://github.com/minihazel/EnvironmentReplace) | ✅ | Substitui ambientes/mapas | 🟡 Avaliar | 🔍 | Sim | — |
| 55 | kmyuhkyuk-KmyTarkovApi | 🖥️ Client | 🌐 Geral | 🧩 Framework | 🧩 Framework | [898](https://forge.sp-tarkov.com/mod/898/kmy-tarkov-api) | [kmyuhkyuk/KmyTarkovApi](https://github.com/kmyuhkyuk/KmyTarkovApi) | [kmyuhkyuk/KmyTarkovApi](https://github.com/kmyuhkyuk/KmyTarkovApi) | ✅ | Framework para client mods | 🟡 Avaliar | 🔍 | Sim | — |
| 56 | lacyway-mergeconsumables (MergeConsumables) | 🔀 Misto | 🔀 Ambos | 🛋️ QoL | 📦 Inventário | [1657](https://forge.sp-tarkov.com/mod/1657/mergeconsumables) | [Lacyway/MergeConsumables](https://github.com/Lacyway/MergeConsumables) | [Lacyway/MergeConsumables](https://github.com/Lacyway/MergeConsumables) | ✅ | Merge de consumíveis (médicos, comida) | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 57 | MoreCheckmarks | 🔀 Misto | 🔀 Ambos | 🛋️ QoL | 🖼️ UI | [861](https://forge.sp-tarkov.com/mod/861/morecheckmarks) | [TommySoucy/MoreCheckmarks](https://github.com/TommySoucy/MoreCheckmarks) | [TommySoucy/MoreCheckmarks](https://github.com/TommySoucy/MoreCheckmarks) | ✅ | Checkmarks coloridos em itens (quests, hideout, barters) — v2.1.0 (SPT 4.0.11). Alias: `MoreCheckmarksBackend` | 🟢 À Instalar | 🔍 | Sim | ✓ |
| 58 | MoxoPixel-MagTape | 🌐 Server | 🌐 Geral | 🎨 Cosmético | 🎯 Munições | [1018](https://forge.sp-tarkov.com/mod/1018/mag-tape) | [emilanderss0n/MagTape](https://github.com/emilanderss0n/MagTape) | [emilanderss0n/MagTape](https://github.com/emilanderss0n/MagTape) | ✅ | Magazines com tape (visual + tagging) | 🟠 Aguardar upstream | 🔍 | Sim | — |
| 59 | MoxoPixel-TacticalGearComponent | 🌐 Server | 🌐 Geral | ➕ Conteúdo | 🛡️ Equipamentos | [1125](https://forge.sp-tarkov.com/mod/1125/tactical-gear-component) | [emilanderss0n/TGC](https://github.com/emilanderss0n/TGC) | [emilanderss0n/TGC](https://github.com/emilanderss0n/TGC) | ✅ | Componente de equipamento tático | 🟠 Aguardar upstream | 🔍 | Sim | — |
| 60 | MusicManiac-LessRestrictingHeadwear | 🌐 Server | 🌐 Geral | ⚖️ Balanceamento | 🛡️ Equipamentos | [922](https://forge.sp-tarkov.com/mod/922/less-restricting-headwear) | [MusicManiac/LessRestrictingHeadwear](https://github.com/MusicManiac/LessRestrictingHeadwear) | [MusicManiac/LessRestrictingHeadwear](https://github.com/MusicManiac/LessRestrictingHeadwear) | ✅ | Headwear menos restritivo — v4.0.13 disponível | 🟢 À Instalar | 🔍 | Sim | ✓ |
| 61 | platinum-theblacklist | 🔀 Misto | 🌐 Geral | ⚖️ Balanceamento | 💰 Mercado | [755](https://forge.sp-tarkov.com/mod/755/the-blacklist-flea-market-enhancements) | [gndworks/spt-the-blacklist](https://github.com/gndworks/spt-the-blacklist) | [ArchangelWTF/spt-the-blacklist](https://github.com/ArchangelWTF/spt-the-blacklist) | ✅ | Blacklist de itens (flea market enhancements) | ⚫ Não incluir | 🔍 | Sim | — |
| 62 | PlayerEncumbranceBar | 🖥️ Client | 🔀 Ambos | 🛋️ QoL | 🖼️ UI · 📦 Inventário | [1374](https://forge.sp-tarkov.com/mod/1374/player-encumbrance-bar) | [mpstark/SPT-PlayerEncumbranceBar](https://github.com/mpstark/SPT-PlayerEncumbranceBar) | [Lacyway/SPT-PlayerEncumbranceBar](https://github.com/Lacyway/SPT-PlayerEncumbranceBar) | ✅ | Barra de encumbrance no inventário — v1.2.2 (SPT 4.0.13, maintainer 4.0: Lacyway) | 🟢 À Instalar | 🔍 | Sim | ✓ |
| 63 | Pluto! - SPT Battlepass | 🌐 Server | 🌐 Geral | 📈 Progressão | 📜 Quests · 📊 Progressão | [2098](https://forge.sp-tarkov.com/mod/2098/spt-battlepass) | Não encontrado | — | ❌ | Battlepass para SPT (Arena Season 0) | 🟠 Aguardar upstream | 🔍 | Sim | — |
| 64 | QuickSell | 🖥️ Client | 🌐 Geral | 🛋️ QoL | 💰 Mercado | [1732](https://forge.sp-tarkov.com/mod/1732/quicksell) | [TadMaj/Tarkov-QuickSell](https://github.com/TadMaj/Tarkov-QuickSell) | [TadMaj/Tarkov-QuickSell](https://github.com/TadMaj/Tarkov-QuickSell) | ✅ | Venda rápida de itens (context menu) | ⚫ Não incluir | 🔍 | Sim | — |
| 65 | RaiRai.ColorConverterAPI | 🖥️ Client | 🌐 Geral | 🧩 Framework | 🖼️ UI | [1090](https://forge.sp-tarkov.com/mod/1090/color-converter-api) | [RaiRaiTheRaichu/ColorConverterAPI](https://github.com/RaiRaiTheRaichu/ColorConverterAPI) | [RaiRaiTheRaichu/ColorConverterAPI](https://github.com/RaiRaiTheRaichu/ColorConverterAPI) | ✅ | API utilitária de conversão de cores | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 66 | Realism | 🔀 Misto | 🔀 Ambos | 🩸 Realismo | 🧩 Framework | [416](https://forge.sp-tarkov.com/mod/416/spt-realism-mod) | [space-commits/SPT-Realism-Mod-Client](https://github.com/space-commits/SPT-Realism-Mod-Client) · [SPT-Realism-Mod-Server](https://github.com/space-commits/SPT-Realism-Mod-Server) | — | ❌ | Overhaul de realismo (balística, médica, hazards) — último release SPT 3.9.x. Aliases: `SPT-Realism`, `RealismMod` | ⬆️ Evoluir p/ 4.0 | 🔍 | Sim | — |
| 67 | redlaser42-Better Headset Descriptions | 🖥️ Client | 🌐 Geral | 🛋️ QoL | 🖼️ UI · 🛡️ Equipamentos | [2199](https://forge.sp-tarkov.com/mod/2199/better-headset-descriptions) | Não encontrado | — | ❌ | Adiciona stats de headsets às descrições dos itens — sem repo público | ⚫ Não incluir | 🔍 | Sim | — |
| 68 | redlaser42-Increase Climb Height | 🌐 Server | ⚔️ Raid | ⚖️ Balanceamento | 🎬 Animações | [1575](https://forge.sp-tarkov.com/mod/1575/increase-climb-height) | Não encontrado | Não encontrado | ✅ | Aumenta altura máxima de escalada | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 69 | SAIN | 🖥️ Client | ⚔️ Raid | 🩸 Realismo | 🤖 IA | [791](https://forge.sp-tarkov.com/mod/791/sain-solarints-ai-modifications-full-ai-combat-system-replacement) | [Solarint/SAIN](https://github.com/Solarint/SAIN) | [ArchangelWTF/SAIN](https://github.com/ArchangelWTF/SAIN) | ✅ | Substituição completa de IA dos bots — v4.4.3 (SPT 4.0.13). **3.x: repo Solarint original** · **4.2.0+: fork ArchangelWTF**. Depende de BigBrain + Waypoints | 🟢 À Instalar | 🔍 | Sim | ✓ |
| 70 | seasoniterator | 🔍 | 🔍 | 🔍 | 🔍 | 🔍 | Não encontrado | Não encontrado | ❌ | 🔍 (estações/seasonal?) — não confirmado no Forge | ⬆️ Evoluir p/ 4.0 | 🔍 | Sim | — |
| 71 | shibdib-NoTransitTasks | 🌐 Server | ⚔️ Raid | ⚖️ Balanceamento | 📜 Quests | [1944](https://forge.sp-tarkov.com/mod/1944/no-transit-tasks) | [shibdib/SPT-NoTransitTasks](https://github.com/shibdib/SPT-NoTransitTasks) | — | ❌ | Remove tasks de transit | 🟡 Avaliar | 🔍 | Sim | — |
| 72 | Skwizzy-LootingBots | 🔀 Misto | ⚔️ Raid | 🩸 Realismo | 🤖 IA · 🎒 Loot | [812](https://forge.sp-tarkov.com/mod/812/looting-bots) | [Skwizzy/SPT-LootingBots](https://github.com/Skwizzy/SPT-LootingBots) | [Skwizzy/SPT-LootingBots](https://github.com/Skwizzy/SPT-LootingBots) | ✅ | Bots fazendo loot (BepInEx + server) | ⚫ Não incluir | 🔍 | Sim | — |
| 73 | somtam.NoBush | 🖥️ Client | ⚔️ Raid | 🩸 Realismo | 🤖 IA | [2123](https://forge.sp-tarkov.com/mod/2123/no-bush-updated-for-311) | [gitTerebi/NoBush](https://github.com/gitTerebi/NoBush) | — | ❌ | Para AI atirar em quem está na bush | 🟠 Aguardar upstream | 🔍 | Sim | — |
| 74 | somtam.SimpleDeClutter | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | ✨ Gráficos | [2139](https://forge.sp-tarkov.com/mod/2139/simple-declutter) | [gitTerebi/Simple-Declutter](https://github.com/gitTerebi/Simple-Declutter) | — | ❌ | Reduz clutter visual | 🟠 Aguardar upstream | 🔍 | Sim | — |
| 75 | SPT-FreshContentBackport | 🌐 Server | 🌐 Geral | ➕ Conteúdo | 🧩 Framework | [2187](https://forge.sp-tarkov.com/mod/2187/fresh-content-backport) | [DragonX86-dev/SPT-FreshContentBackport](https://github.com/DragonX86-dev/SPT-FreshContentBackport) | — | ❌ | Backport de conteúdo novo | ⚫ Não incluir | 🔍 | Sim | — |
| 76 | SPT-InsuranceFraud | 🌐 Server | 🌐 Geral | ⚖️ Balanceamento | 💰 Mercado | [1792](https://forge.sp-tarkov.com/mod/1792/insurance-fraud) | [ibxccc/SPT-InsuranceFraud](https://github.com/ibxccc/SPT-InsuranceFraud) | [ibxccc123/SPT-InsuranceFraud](https://github.com/ibxccc123/SPT-InsuranceFraud) | ❌ | Fraude no seguro (loot dropado retorna) — único release v1.0.0 (SPT 3.9.8, out/2024); repo original removido; sem build 4.0 | 🟡 Avaliar | 🔍 | Sim | — |
| 77 | SPTVRAMCleaner | 🖥️ Client | 🌐 Geral | 🛋️ QoL | ✨ Gráficos | [2173](https://forge.sp-tarkov.com/mod/2173/vram-cleaner) | [matsixx/SPTVRAMCleaner](https://github.com/matsixx/SPTVRAMCleaner) | [matsixx/SPTVRAMCleaner](https://github.com/matsixx/SPTVRAMCleaner) | ✅ | Limpeza de VRAM (autor matsixx, não swiftxp) | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 78 | StashSearch | 🖥️ Client | 🏚️ Hideout | 🛋️ QoL | 📦 Inventário | [2148](https://forge.sp-tarkov.com/mod/2148/stash-search) | [ArchangelWTF/StashSearch](https://github.com/ArchangelWTF/StashSearch) | [DrakiaXYZ/SPT-StashSearch](https://github.com/DrakiaXYZ/SPT-StashSearch) | ❌ | Busca dentro do stash — último release v1.4.2 (SPT 3.11.4, mai/2025); DrakiaXYZ fork sem commits desde 2024-03 | 🟠 Aguardar upstream | 🔍 | Sim | — |
| 79 | SwiftXP.ShowMeTheMoney | 🔀 Misto | 🌐 Geral | 🛋️ QoL | 🖼️ UI · 💰 Mercado | [2299](https://forge.sp-tarkov.com/mod/2299/show-me-the-money) | [swiftxp-hub/spt-show-me-the-money](https://github.com/swiftxp-hub/spt-show-me-the-money) | [swiftxp-hub/spt-show-me-the-money](https://github.com/swiftxp-hub/spt-show-me-the-money) | ✅ | Mostra dinheiro/valores em UI | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 80 | TacticalToasterUNTARGH | 🌐 Server | ⚔️ Raid | ➕ Conteúdo | 🤖 IA | [2342](https://forge.sp-tarkov.com/mod/2342/untar-go-home) | [TacticalToaster/TacticalToasterUNTARGH](https://github.com/TacticalToaster/TacticalToasterUNTARGH) | [TacticalToaster/TacticalToasterUNTARGH](https://github.com/TacticalToaster/TacticalToasterUNTARGH) | ✅ | Adiciona UNTAR como faction com bots customizados | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 81 | Tarkov Weather System | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | 🗺️ Mapas | [2120](https://forge.sp-tarkov.com/mod/2120/time-weather-changer-ng) | [flir063-spt @ v2.3.3.0](https://gitlab.com/flir063-spt/timeweatherchanger/-/tree/v2.3.3.0) | [flir063-spt/timeweatherchanger](https://gitlab.com/flir063-spt/timeweatherchanger) | ✅ | Time & Weather Changer NG — v2.4.0 (SPT 4.0.13). Autor: flir. Hospedado no **GitLab** | 🟢 À Instalar | 🔍 | Sim | — |
| 82 | TellTheTime | 🖥️ Client | 🌐 Geral | 🛋️ QoL | 🖼️ UI | [2202](https://forge.sp-tarkov.com/mod/2202/tell-the-time) | [ReplayDEVYT/TellTheTime](https://github.com/ReplayDEVYT/TellTheTime) | — | ❌ | Mostra hora atual | ⚫ Não incluir | 🔍 | Sim | — |
| 83 | Terkoiz.Freecam | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | 🖼️ UI | [164](https://forge.sp-tarkov.com/mod/164/freecam) | [TerkoizLT/SPT-Freecam](https://github.com/TerkoizLT/SPT-Freecam) | [acidphantasm/SPT-Freecam](https://github.com/acidphantasm/SPT-Freecam) | ✅ | Câmera livre (debug/replay) — v1.4.6 (último release SPT 3.11). Mantido por acidphantasm | 🟠 Aguardar upstream | 🔍 | Sim | ✓ |
| 84 | tyfon-hideoutinprogress | 🖥️ Client | 🏚️ Hideout | 🛋️ QoL | 🏚️ Hideout · 📦 Inventário | [2076](https://forge.sp-tarkov.com/mod/2076/hideout-in-progress) | [tyfon7/hip](https://github.com/tyfon7/hip) | [tyfon7/hip](https://github.com/tyfon7/hip) | ✅ | Botão "Transfer Items" no hideout (SPT 4.0). Alias: `Tyfon.HideoutInProgress` | 🟢 À Instalar | 🔍 | Sim | ✓ |
| 85 | tyfon-uifixes | 🖥️ Client | 🌐 Geral | 🛋️ QoL | 🖼️ UI | [1342](https://forge.sp-tarkov.com/mod/1342/ui-fixes) | [tyfon7/UIFixes](https://github.com/tyfon7/UIFixes) | [tyfon7/UIFixes](https://github.com/tyfon7/UIFixes) | ✅ | Coleção de QoL fixes de UI. Aliases: `Tyfon.UIFixes`, `Tyfon.UIFixes.Net` | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 86 | tyfon-weaponcustomizer | 🖥️ Client | 🌐 Geral | 🛋️ QoL | 🔫 Armas | [1950](https://forge.sp-tarkov.com/mod/1950/weapon-customizer) | [tyfon7/WeaponCustomizer](https://github.com/tyfon7/WeaponCustomizer) | [tyfon7/WeaponCustomizer](https://github.com/tyfon7/WeaponCustomizer) | ✅ | Fine tune de attachments. Alias: `Tyfon.WeaponCustomizer` | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 87 | Virtual's Custom Quest Loader | 🌐 Server | 🌐 Geral | 🧩 Framework | 📜 Quests | [649](https://forge.sp-tarkov.com/mod/649/virtuals-custom-quest-loader) | [VirtualAE/Virtuals-Custom-Quest-Loader](https://github.com/VirtualAE/Virtuals-Custom-Quest-Loader) | [VirtualAE/Virtuals-Custom-Quest-Loader](https://github.com/VirtualAE/Virtuals-Custom-Quest-Loader) | ✅ | Dependência para mods importarem custom quests. Alias: `VCQL`, `VCQLQuestZones` | 🟠 Aguardar upstream | 🔍 | Sim | — |
| 88 | VisceralCombat | 🔍 | ⚔️ Raid | 🩸 Realismo | ✨ Gráficos | — | Não encontrado | Não encontrado | ❌ | Efeitos viscerais de combate — distribuído via Patreon/Discord (Valentin The Mad), não publicado no Forge | 🟠 Aguardar upstream | 🔍 | Sim | — |
| 89 | VolumetricBloodFX | 🔍 | ⚔️ Raid | 🎨 Cosmético | ✨ Gráficos | — | Não encontrado | Não encontrado | ❌ | FX de sangue volumétrico — distribuído via Patreon/Discord (Valentin The Mad), não publicado no Forge | 🟠 Aguardar upstream | 🔍 | Sim | — |
| 90 | Wara-ModdingStatsHelper | 🖥️ Client | 🌐 Geral | 🛋️ QoL | 🖼️ UI · 🔫 Armas | [1300](https://forge.sp-tarkov.com/mod/1300/modding-stats-helper-by-wara) | [Soulztorm/Wara-ModdingStatsHelper](https://github.com/Soulztorm/Wara-ModdingStatsHelper) | [Soulztorm/Wara-ModdingStatsHelper](https://github.com/Soulztorm/Wara-ModdingStatsHelper) | ✅ | Helper de stats em modding | 🟡 Avaliar | 🔍 | Sim | ✓ |
| 91 | WTT-Armory | 🌐 Server | 🌐 Geral | ➕ Conteúdo | 🔫 Armas · 📜 Quests | [2246](https://forge.sp-tarkov.com/mod/2246/wtt-armory) | [WelcomeToTarkov/WTT-Armory](https://github.com/WelcomeToTarkov/WTT-Armory) | [WelcomeToTarkov/WTT-Armory @ 4.0](https://github.com/WelcomeToTarkov/WTT-Armory/tree/4.0) | ✅ | Pack de 50+ armas + quests (WTT team) — v2.0.5 (SPT 4.0.13). 4.0 está em branch separada | 🟢 À Instalar | 🔍 | Sim | — |
| 92 | WTT-PackNStrap | 🌐 Server | 🔀 Ambos | ➕ Conteúdo | 🛡️ Equipamentos | [1278](https://forge.sp-tarkov.com/mod/1278/wtt-pack-n-strap) | [WelcomeToTarkov/PackNStrap](https://github.com/WelcomeToTarkov/PackNStrap) | [WelcomeToTarkov/PackNStrap](https://github.com/WelcomeToTarkov/PackNStrap) | ✅ | Battle belt + small cases (WTT team) — v2.0.4 (SPT 4.0.13). Mesmo repo | 🟢 À Instalar | 🔍 | Sim | ✓ |
| 93 | yellowdoge-tarkovrarecollectibles | 🌐 Server | 🌐 Geral | ➕ Conteúdo | 🎒 Loot | [2318](https://forge.sp-tarkov.com/mod/2318/tarkov-rare-collectibles) | [TheYellowDoge/YellowDoge-TarkovRareCollectibles](https://github.com/TheYellowDoge/YellowDoge-TarkovRareCollectibles) | [TheYellowDoge/YellowDoge-TarkovRareCollectibles](https://github.com/TheYellowDoge/YellowDoge-TarkovRareCollectibles) | ✅ | Itens raros colecionáveis | 🟡 Avaliar | 🔍 | Sim | — |
| 94 | zzDrakiaXYZ-LiveFleaPrices | 🌐 Server | 🌐 Geral | ⚖️ Balanceamento | 💰 Mercado | [1131](https://forge.sp-tarkov.com/mod/1131/live-flea-prices) | [DrakiaXYZ/SPT-LiveFleaPrices](https://github.com/DrakiaXYZ/SPT-LiveFleaPrices) | [DrakiaXYZ/SPT-LiveFleaPrices-CSharp](https://github.com/DrakiaXYZ/SPT-LiveFleaPrices-CSharp) | ✅ | Preços do flea ao vivo (live data) — versão C# do mesmo autor | 🟢 À Instalar | 🔍 | Sim | ✓ |
| 95 | 🏠 Band-Aid | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | 🧩 Framework | — | — | — | ❌ | Fix/patch interno do projeto | ⬆️ Evoluir p/ 4.0 | 🔍 | Sim | — |
| 96 | 🏠 CoordLogger | 🖥️ Client | ⚔️ Raid | 🧩 Framework | 🧩 Framework | — | — | — | ❌ | Logger de coordenadas (utilitário interno) | ⚫ Não incluir | 🔍 | Sim | — |
| 97 | 🏠 FikaTransitFix (FikaTransitFixServer) | 🌐 Server | ⚔️ Raid | 🧩 Framework | 🌐 Multiplayer | — | — | — | ❌ | Fix de transit em raids do Fika (interno) | 🟡 Avaliar | 🔍 | Sim | — |
| 98 | 🏠 FixReloadUltraFika | 🖥️ Client | ⚔️ Raid | 🧩 Framework | 🌐 Multiplayer · 🎬 Animações | — | — | — | ❌ | Fix de reload no UltraFika (interno) | ⬆️ Evoluir p/ 4.0 | 🔍 | Sim | — |
| 99 | 🏠 ForceSync | 🔀 Misto | 🔀 Ambos | 🧩 Framework | 🌐 Multiplayer | — | — | — | ❌ | Força sincronização (interno, relacionado ao UltraFika) | 🟡 Avaliar | 🔍 | Sim | — |
| 100 | 🏠 GhostMercenaries | 🌐 Server | ⚔️ Raid | ➕ Conteúdo | 🤖 IA | — | — | — | ❌ | Mercenários customizados (interno) | ⬆️ Evoluir p/ 4.0 | 🔍 | Sim | — |
| 101 | 🏠 IdleSprintFix | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | 🎬 Animações | — | — | — | ❌ | Fix do bug de sprint travado (interno, v1.2.2) | ⚫ Não incluir | 🔍 | Sim | — |
| 102 | 🏠 TarkovRedLine (TarkovRedLine-ServerMod) | 🌐 Server | 🌐 Geral | ⚖️ Balanceamento | 🧩 Framework | — | — | — | ❌ | Mod do servidor RedLine — customização proprietária do projeto | ⬆️ Evoluir p/ 4.0 | 🔍 | Sim | — |
| 103 | 🏠 UmbigoPreto-Face the Knight - Mask Fix | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | 🛡️ Equipamentos | — | — | — | ❌ | Fix da máscara do Knight (interno) | ⚫ Não incluir | 🔍 | Sim | — |
| 104 | 🏠 UmbigoPreto-TrueTrauma | 🔀 Misto | ⚔️ Raid | 🩸 Realismo | 🧩 Framework | — | — | — | ❌ | Sistema de trauma realista (interno) | ⬆️ Evoluir p/ 4.0 | 🔍 | Sim | — |
| 105 | Climbable Ladders | 🖥️ Client | ⚔️ Raid | 🩸 Realismo | 🗺️ Mapas · 🎬 Animações | [2649](https://forge.sp-tarkov.com/mod/2649/climbable-ladders) | — | [bmpq/spt-ladders](https://github.com/bmpq/spt-ladders) | ✅ | Habilita escalada de escadas em vários mapas (woods bunker, customs watchtower etc.) — SPT 4.0.13 | 🟢 À Instalar | 🔍 | New | ✓ |
| 106 | Magazine Check Interrupt | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | 🎯 Munições · 🎬 Animações | [2643](https://forge.sp-tarkov.com/mod/2643/magazine-check-interrupt) | — | [ozen-m/SPT-MagCheckInterrupt](https://github.com/ozen-m/SPT-MagCheckInterrupt) | ✅ | Transição contínua de magazine check para reload sem sair da animação — SPT 4.0.13 | 🟢 À Instalar | 🔍 | New | ✓ |
| 107 | Stance Sync | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | 🎬 Animações · 🔫 Armas | [2639](https://forge.sp-tarkov.com/mod/2639/stance-sync) | — | [minihazel/StanceSync](https://github.com/minihazel/StanceSync) | ✅ | Sincroniza lean e shoulder swap; opção para desabilitar lean sincronizado ao mirar — SPT 4.0.13 | 🟢 À Instalar | 🔍 | New | ✓ |
| 108 | Stat Rewards | 🖥️ Client | 🌐 Geral | 📈 Progressão | 📊 Progressão | [2655](https://forge.sp-tarkov.com/mod/2655/stat-rewards) | — | [Chazut/StatRewards](https://github.com/Chazut/StatRewards) | ✅ | Recompensas aleatórias ao atingir marcos de stats (kills, dano, loot etc.) — 48 milestones repetíveis, configurável — SPT 4.0.13 | 🟢 À Instalar | 🔍 | New | — |
| 109 | Brighter Interiors | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | ✨ Gráficos | [2613](https://forge.sp-tarkov.com/mod/2613/brighter-interiors) | — | [7Bpencil/SPT-ReduceFakeInteriorShadow](https://github.com/7Bpencil/SPT-ReduceFakeInteriorShadow) | ✅ | Reduz intensidade de sombras internas para melhorar visibilidade em ambientes fechados — SPT 4.0.13 | 🟢 À Instalar | 🔍 | New | ✓ |
| 110 | RZCustomProfiles | 🖥️ Client | 🌐 Geral | 🛋️ QoL | 🖼️ UI | [2614](https://forge.sp-tarkov.com/mod/2614/rzcustomprofiles) | — | [remzdnb/RZ-SPTMods](https://github.com/remzdnb/RZ-SPTMods) | ✅ | Gerenciamento de templates de perfil de personagem — v1.1.0 (SPT 4.0.13). Autor: RemzDNB | 🟢 À Instalar | 🔍 | New | ✓ |
| 111 | LoadBundleEvenFaster | 🖥️ Client | 🌐 Geral | 🛋️ QoL | ✨ Gráficos | [2599](https://forge.sp-tarkov.com/mod/2599/loadbundleevenfaster) | — | [s8ga/SPT_LoadBundleEvenFaster](https://github.com/s8ga/SPT_LoadBundleEvenFaster) | ✅ | Reduz tempo de carregamento de bundles — camada extra sobre LoadBundleFaster. **Dep:** LoadBundleFaster ≥ v1.0.0. SPT 4.0.13 | 🟢 À Instalar | 🔍 | New | ✓ |
| 112 | Recoil Rework (Legacy) | 🌐 Server | ⚔️ Raid | 🩸 Realismo | 🔫 Armas | [2190](https://forge.sp-tarkov.com/mod/2190/recoil-rework-legacy) | — | [peinwastaken/SPTRecoilRework](https://github.com/peinwastaken/SPTRecoilRework) | ✅ | Reescreve mecânica de recoil das armas (autor: pein) — v1.10.0 recompilada para SPT 4.0.13 | 🟢 À Instalar | 🔍 | New | ✓ |
| 113 | Gunsmith Barters | 🌐 Server | 🌐 Geral | 🔥 Hardcore | 💰 Mercado · 🔫 Armas | [1963](https://forge.sp-tarkov.com/mod/1963/gunsmith-barters) | — | [Solethia/Kipperworks.GunsmithBarters](https://github.com/Solethia/Kipperworks.GunsmithBarters) | ✅ | Adiciona barters no Mecânico voltados a runs hardcore — v2.0.2 (SPT 4.0.13). ⚠️ pode fazer alterações permanentes no perfil | 🟢 À Instalar | 🔍 | New | — |
| 114 | Camera Position Control & Custom Weapon Stances | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | 🎬 Animações · 🔫 Armas | [2572](https://forge.sp-tarkov.com/mod/2572/camera-position-control-custom-weapon-stances) | — | [shengzhanzhe/stancesAndCameraPositionSPT4.0.11](https://github.com/shengzhanzhe/stancesAndCameraPositionSPT4.0.11) | ✅ | Controle customizável de posição da câmera FPV e stances de arma — v1.1.5 (SPT 4.0.13). ⚠️ ao atualizar de ≤0.9.7 remover `CameraRotationMod.dll` antigo | 🟢 À Instalar | 🔍 | New | ✓ |
| 115 | Manimal's Ammo Loading Animations | 🖥️ Client | ⚔️ Raid | 🩸 Realismo | 🎯 Munições · 🎬 Animações | [2681](https://forge.sp-tarkov.com/mod/2681/manimals-ammo-loading-animations) | — | [danauraborealis/ManimalAmmoLoadingAnimations](https://github.com/danauraborealis/ManimalAmmoLoadingAnimations) | ✅ | Animações em primeira pessoa para carregar munição em magazines — v1.5.0 (SPT 4.0.13). **Dep:** WTT-CommonLib ≥ v2.0.20. ⚠️ incompatível com Fika | 🟢 À Instalar | 🔍 | New | ✓ |
| 116 | Refined Flea Offer List | 🖥️ Client | 🌐 Geral | 🛋️ QoL | 💰 Mercado · 🖼️ UI | [2623](https://forge.sp-tarkov.com/mod/2623/refined-flea-offer-list) | — | [Klamist/RefinedFleaOfferList](https://github.com/Klamist/RefinedFleaOfferList) | ✅ | Exibe apenas a oferta mais barata por item no flea market; display inteligente de rublos e variantes de arma/armadura — v1.0.1 (SPT 4.0.13). Autor: ciallomako | 🟢 À Instalar | 🔍 | New | ✓ |
| 117 | Knight Mask Fix (4.0+) | 🖥️ Client | ⚔️ Raid | 🎨 Cosmético | 🛡️ Equipamentos | [2685](https://forge.sp-tarkov.com/mod/2685/knight-mask-fix-40) | — | [TheOfficialSkull/FW---Knight-Mask-Fix---4.0.13](https://github.com/TheOfficialSkull/FW---Knight-Mask-Fix---4.0.13) | ✅ | Corrige textura ausente da máscara 'Death Knight' no PMC Knight e em modelos de jogador — SPT 4.0.13+. Autor: Flex Wayne | 🟢 À Instalar | 🔍 | New | ✓ |
| 118 | Peltor TEP-300 earplugs backport and fixes | 🖥️ Client | ⚔️ Raid | ➕ Conteúdo | 🛡️ Equipamentos | [2420](https://forge.sp-tarkov.com/mod/2420/peltor-tep-300-earplugs-backport-and-fixes) | — | [Dight67/TEP-300-Backport-Fixes](https://github.com/Dight67/TEP-300-Backport-Fixes) | ✅ | Restaura os fones militares TEP-300 do EFT Live no SPT, com correções — SPT 4.0.13. Autor: nem | 🟢 À Instalar | 🔍 | New | ✓ |
| 119 | Skills Extended | 🌐 Server | 🌐 Geral | ⚙️ Mecânicas | 🔍 | [2383](https://forge.sp-tarkov.com/mod/2383/skills-extended) | — | [CJ-SPT/Skills-Extended](https://github.com/CJ-SPT/Skills-Extended) | ✅ | Expande e aprimora o sistema de skills do personagem com mecânicas mais impactantes — compatível com Fika, SPT 4.0.13 | 🟢 Instalar | 🔍 | New | — |
| 120 | Wolfik's Heavy Trooper Masks - Reupload | 🖥️ Client | ⚔️ Raid | ➕ Conteúdo | 🛡️ Equipamentos | [1569](https://forge.sp-tarkov.com/mod/1569/wolfiks-heavy-trooper-masks-reupload) | — | [Hood26/WolfiksHeavyTroopers](https://github.com/Hood26/WolfiksHeavyTroopers) | ✅ | Adiciona cinco máscaras pesadas em variantes de cor (Black, Tan, Black/Coyote, Wolf, Cult) com proteção real de armadura e desbloqueio por quests — SPT 4.0.13 | 🟢 Instalar | 🔍 | New | ✓ |
| 121 | Couturier - gear and clothing pack | 🔀 Misto | ⚔️ Raid | ➕ Conteúdo | 🛡️ Equipamentos · 🎽 Vestuário | [2239](https://forge.sp-tarkov.com/mod/2239/couturier-gear-and-clothing-pack) | — | [turbodestroyer1337/spt_couturier](https://github.com/turbodestroyer1337/spt_couturier) | ✅ | Pack de equipamento e vestuário: mochilas, capacetes, armaduras, coletes táticos retexturizados e roupas adicionais — SPT 4.0.13 | ⚫ Não incluir | 🔍 | New | — |
| 122 | Foldables | 🖥️ Client | 🌐 Geral | ⚙️ Mecânicas | 🎒 Inventário · 🛡️ Equipamentos | [2422](https://forge.sp-tarkov.com/mod/2422/foldables) | — | [ozen-m/SPT-Foldables](https://github.com/ozen-m/SPT-Foldables) | ✅ | Permite dobrar mochilas e coletes, reduzindo seu tamanho quando guardados no inventário — v1.0.3, SPT 4.0.13. Autor: ozen | 🟢 Instalar | 🔍 | New | ✓ |
| 123 | Change Helmet Visor | 🔀 Misto | ⚔️ Raid | 🛋️ QoL | 🛡️ Equipamentos · 🖼️ UI | [2554](https://forge.sp-tarkov.com/mod/2554/change-helmet-visor) | — | [Trinagan/ChangeHelmetVisor](https://github.com/Trinagan/ChangeHelmetVisor) | ✅ | Ajusta a renderização do visor de capacete com controle de escala de textura via plugin BepInEx — v2.0.0, SPT 4.0.13 | 🟢 Instalar | 🔍 | New | ✓ |
| 124 | WTT - CommonLib | 🌐 Server | 🌐 Geral | 📚 Biblioteca | 🧰 Dev/Lib | [2310](https://forge.sp-tarkov.com/mod/2310/wtt-commonlib) | — | [WelcomeToTarkov/WTT-CommonLib](https://github.com/WelcomeToTarkov/WTT-CommonLib) | ✅ | Biblioteca utilitária de modding: reduz boilerplate ao adicionar itens, missões e personagens personalizados — v2.0.20, SPT 4.0.13 | 🟢 Instalar | 🔍 | New | ✓ |
| 125 | Easy Ammo Names | 🌐 Server | 🌐 Geral | 🛋️ QoL | 🖼️ UI · 🎒 Inventário | [1262](https://forge.sp-tarkov.com/mod/1262/easy-ammo-names) | — | [DrakiaXYZ/SPT-EasyAmmoNames-CSharp](https://github.com/DrakiaXYZ/SPT-EasyAmmoNames-CSharp) | ✅ | Renomeia munições no SPT para identificação mais fácil dos tipos de projéteis — SPT 4.0.13 | 🟢 Instalar | 🔍 | New | ✓ |
| 126 | Quest Tracker | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | 🖼️ UI · 🗺️ Quests | [1140](https://forge.sp-tarkov.com/mod/1140/quest-tracker) | — | [DrakiaXYZ/SPT-QuestTracker](https://github.com/DrakiaXYZ/SPT-QuestTracker) | ✅ | Overlay alternável para acompanhar progresso de missões em tempo real durante incursões — SPT 4.0.13 | 🟢 Instalar | 🔍 | New | ✓ |
| 127 | Use Items Anywhere | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | 🎒 Inventário | [2386](https://forge.sp-tarkov.com/mod/2386/use-items-anywhere) | — | [CJ-SPT/UseItemsAnywhere](https://github.com/CJ-SPT/UseItemsAnywhere) | ✅ | Permite usar e vincular itens de qualquer lugar do inventário durante a raid — SPT 4.0.13 | 🟢 Instalar | 🔍 | New | ✓ |
| 128 | Hideout Recipe Framework | 🌐 Server | 🏚️ Hideout | 📚 Biblioteca | 🧰 Dev/Lib · 🏚️ Hideout | [2520](https://forge.sp-tarkov.com/mod/2520/hideout-recipe-framework) | — | [arcustarkov-sys/hideout-recipe-framework](https://github.com/arcustarkov-sys/hideout-recipe-framework) | ✅ | Framework para criar facilmente receitas personalizadas do Esconderijo, expandindo opções de produção — SPT 4.0.13 | 🟢 Instalar | 🔍 | New | ✓ |
| 129 | Dynamic External Resolution Patch (DERP) | 🖥️ Client | 🌐 Geral | 🛋️ QoL | ✨ Gráficos | [2200](https://forge.sp-tarkov.com/mod/2200/dynamic-external-resolution-patch-derp) | [MrFlashMode/SPT-Dynamic-External-Resolution](https://github.com/MrFlashMode/SPT-Dynamic-External-Resolution) | [Shibatsui/SPT-Dynamic-External-Resolution](https://github.com/Shibatsui/SPT-Dynamic-External-Resolution) | ✅ | Altera resolução dinamicamente ao usar miras ópticas, aumentando FPS — v1.1.1, SPT 4.0.13. Autor: Sh1ba (port 4.0 do DERP original, ver #31) | 🟢 Instalar | 🔍 | New | ✓ |
| 130 | AutoDeposit | 🖥️ Client | 🏚️ Hideout | 🛋️ QoL | 🎒 Inventário | [1469](https://forge.sp-tarkov.com/mod/1469/autodeposit) | — | [tyfon7/AutoDeposit](https://github.com/tyfon7/AutoDeposit) | ✅ | Transfere itens para contêineres do stash que já contêm itens correspondentes (estilo Quick Stack do Terraria) — SPT 4.0.13 | 🟢 Instalar | 🔍 | New | ✓ |
| 131 | Bring Back Concussion | 🖥️ Client | ⚔️ Raid | 🩸 Realismo | 🔊 Áudio | [2550](https://forge.sp-tarkov.com/mod/2550/bring-back-concussion) | — | [harmonyzt/BringBackConcussion](https://github.com/harmonyzt/BringBackConcussion) | ✅ | Restaura efeitos sonoros de concussão e sons pós-morte do EFT (tinido, cegueira temporária) — v1.0.3, SPT 4.0.13 | 🟢 Instalar | 🔍 | New | ✓ |
| 132 | LetMeRightClick | 🖥️ Client | 🌐 Geral | 🛋️ QoL | 🖼️ UI · 🎒 Inventário | [2405](https://forge.sp-tarkov.com/mod/2405/letmerightclick) | — | [Lacyway/LetMeRightClick](https://github.com/Lacyway/LetMeRightClick) | ✅ | Permite usar botão direito do mouse ao buscar itens, eliminando a restrição anterior — v1.0.0, SPT 4.0.13 | 🟢 Instalar | 🔍 | New | ✓ |
| 133 | Un-flashbang Hideout | 🖥️ Client | 🏚️ Hideout | 🛋️ QoL | ✨ Gráficos · 🏚️ Hideout | [1425](https://forge.sp-tarkov.com/mod/1425/un-flashbang-hideout) | — | [ArchangelWTF/Un-flashbangHideout](https://github.com/ArchangelWTF/Un-flashbangHideout) | ✅ | Remove o efeito de flashbang no hideout ao retornar de sessões noturnas com super sampling ativo — v1.0.4, SPT 4.0.13 | 🟢 Instalar | 🔍 | New | ✓ |
| 134 | Task List Fixes | 🖥️ Client | 🌐 Geral | 🛋️ QoL | 🖼️ UI · 🗺️ Quests | [824](https://forge.sp-tarkov.com/mod/824/task-list-fixes) | — | [DrakiaXYZ/SPT-TaskListFixes](https://github.com/DrakiaXYZ/SPT-TaskListFixes) | ✅ | Corrige classificação de tarefas e melhora desempenho/organização da lista de missões — SPT 4.0.13. Autor: DrakiaXYZ | 🟢 Instalar | 🔍 | New | ✓ |
| 135 | Dragon Den - Dev Tool | 🖥️ Client | ⚔️ Raid | 🛠️ Dev | 🧰 Dev/Lib | [2336](https://forge.sp-tarkov.com/mod/2336/dragon-den-dev-tool) | — | [Drexira/DragonDen-DevTool](https://github.com/Drexira/DragonDen-DevTool) | ✅ | Ferramenta dev para testes em raid: heal, modo deus, spawn de itens, gestão de bots e teleporte (225 locais) — SPT 4.0.13. Autor: Drexira | 🟢 Instalar | 🔍 | New | ✓ |
| 136 | Camo And Stickers | 🖥️ Client | 🌐 Geral | 🎨 Cosmético | 🔫 Armas | [2658](https://forge.sp-tarkov.com/mod/2658/camo-and-stickers) | — | [7Bpencil/SPT.WeaponCamoAndStickers](https://github.com/7Bpencil/SPT.WeaponCamoAndStickers) | ✅ | Personaliza armas com camuflagem e adesivos, oferecendo opções estéticas para equipamentos — SPT 4.0.13 | 🟢 Instalar | 🔍 | New | ✓ |
| 137 | SPTarkovSpeedLoader | 🖥️ Client | 🌐 Geral | 🛋️ QoL | 🎒 Inventário | [2440](https://forge.sp-tarkov.com/mod/2440/sptarkovspeedloader) | — | [ragnaroks/SPTarkovSpeedLoader](https://github.com/ragnaroks/SPTarkovSpeedLoader) | ✅ | Acelera o carregamento de munição em magazines, abastecendo equipamentos mais rápido — SPT 4.0.13 | 🟢 Instalar | 🔍 | New | — |
| 138 | WTT - Menu Overhaul | 🖥️ Client | 🌐 Geral | 🎨 Cosmético | 🖼️ UI | [1775](https://forge.sp-tarkov.com/mod/1775/wtt-menu-overhaul) | — | [emilanderss0n/SPT-Menu-Overhaul](https://github.com/emilanderss0n/SPT-Menu-Overhaul) | ✅ | Novo layout de menu com o personagem na tela inicial (modelo rotacionável) — SPT 4.0.13 | 🟢 Instalar | 🔍 | New | ✓ |
| 139 | Discord Raid Map | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | 🌐 Multiplayer | [2714](https://forge.sp-tarkov.com/mod/2714/discord-raid-map) | — | [Fiodorwellfme/DiscordRaidMap](https://github.com/Fiodorwellfme/DiscordRaidMap) | ✅ | Posta mapa da raid no Discord via webhook (jogadores, kills, bosses, airdrop, extrações, tempo) — SPT 4.0.13 | 🟢 Instalar | 🔍 | New | ✓ |
| 140 | UnderFire - An Adrenaline Effect | 🖥️ Client | ⚔️ Raid | 🩸 Realismo | ✨ Gráficos/FX | [2063](https://forge.sp-tarkov.com/mod/2063/underfire-an-adrenaline-effect) | — | [rpmwpm/UnderFire](https://github.com/rpmwpm/UnderFire) | ✅ | Efeito de adrenalina (visão/FX) ao levar tiros ou ser atingido em combate — SPT 4.0.13 | 🟢 Instalar | 🔍 | New | ✓ |
| 141 | Career Log | 🖥️ Client | 🌐 Geral | 🛋️ QoL | 🖼️ UI | [2713](https://forge.sp-tarkov.com/mod/2713/career-log) | — | [FallegaHQ/SPT-CareerLog](https://github.com/FallegaHQ/SPT-CareerLog) | ✅ | Histórico de carreira: log de cada raid, stats vitalícios, finanças e stash, acessível no menu — SPT 4.0.13 | 🟢 Instalar | 🔍 | New | ✓ |
| 142 | ABPS - Acid's Bot Placement System | 🖥️ Client | ⚔️ Raid | ⚖️ Balanceamento | 🤖 IA/Bots | [2097](https://forge.sp-tarkov.com/mod/2097/abps-acids-bot-placement-system) | — | [acidphantasm/botplacementsystem-csharp](https://github.com/acidphantasm/botplacementsystem-csharp) | ✅ | Sistema configurável de spawn de bots (waves, bosses, chances) | 🟢 Instalar | 🔍 | New | ✓ |
| 143 | MoreBotsAPI | 🖥️ Client | ⚔️ Raid | 🧩 Framework/Base | 🤖 IA/Bots | [2426](https://forge.sp-tarkov.com/mod/2426/morebotsapi) | — | [TacticalToaster/MoreBotsAPI](https://github.com/TacticalToaster/MoreBotsAPI) | ✅ | API para facilitar a implementação de bots customizados | 🟢 Instalar | 🔍 | New | ✓ |
| 144 | LoadBundleFaster (Crc32 Patch) | 🖥️ Client | 🌐 Geral | 🛋️ QoL | 🧩 Framework | [2563](https://forge.sp-tarkov.com/mod/2563/loadbundlefaster-crc32-patch) | — | [s8ga/SPT_PatchCrc32](https://github.com/s8ga/SPT_PatchCrc32) | ✅ | Patch para acelerar o carregamento de bundles (BepinEx plugin) | 🟢 Instalar | 🔍 | New | ✓ |
| 145 | ORBIT | 🖥️ Client | ⚔️ Raid | ⚖️ Balanceamento | 🤖 IA/Bots | [2706](https://forge.sp-tarkov.com/mod/2706/orbit) | — | [Chazut/ORBIT](https://github.com/Chazut/ORBIT) | ✅ | Bots com missões: looteiam, lutam e extraem como players reais | 🟢 Instalar | 🔍 | New | ✓ |
| 146 | Manimal's Hacker Mod | 🖥️ Client | ⚔️ Raid | ➕ Conteúdo | 🎒 Loot | [2703](https://forge.sp-tarkov.com/mod/2703/manimals-hacker-mod) | — | [danauraborealis/ManimalHackerMod](https://github.com/danauraborealis/ManimalHackerMod) | ✅ | Itens que permitem hackear portas de keycards e caixas eletrônicos (ATMs) | 🟢 Instalar | 🔍 | New | ✓ |
| 147 | Picture in Picture Disabler | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | ✨ Gráficos/FX | [2667](https://forge.sp-tarkov.com/mod/2667/picture-in-picture-disabler) | — | [Fiodorwellfme/PiP-Disabler](https://github.com/Fiodorwellfme/PiP-Disabler) | ✅ | Desativa a renderização PiP em miras telescópicas para maximizar FPS | 🟢 Instalar | 🔍 | New | ✓ |
| 148 | All The Clothes | 🌐 Server | 🌐 Geral | 🎨 Cosmético | 🛡️ Equipamentos | [526](https://forge.sp-tarkov.com/mod/526/all-the-clothes) | — | [RaiRaiTheRaichu/SPT_All-The-Clothes-Mod](https://github.com/RaiRaiTheRaichu/SPT_All-The-Clothes-Mod) | ✅ | Libera opções extras de vestuário - reescrito para SPT 4.0+ | 🟢 Instalar | 🔍 | New | ✓ |
| 149 | Hideout Shootout | 🖥️ Client | 🏚️ Hideout | 🛋️ QoL | 🔫 Armas | [2705](https://forge.sp-tarkov.com/mod/2705/hideout-shootout) | — | [emilanderss0n/spt-hideout-shootout](https://github.com/emilanderss0n/spt-hideout-shootout) | ✅ | Permite usar a arma livremente no Hideout fora do stand de tiro | 🟢 Instalar | 🔍 | New | ✓ |
| 150 | AutoGymRAT | 🖥️ Client | 🏚️ Hideout | 🛋️ QoL | 🏚️ Hideout | [2720](https://forge.sp-tarkov.com/mod/2720/autogymrat) | — | [Sweetloldude/AutoGym](https://github.com/Sweetloldude/AutoGym) | ✅ | Completa automaticamente o QTE do círculo do treino na academia do hideout — SPT 4.0.13 | 🟢 Instalar | 🔍 | New | ✓ |
| 151 | More Energy Drinks | 🌐 Server | 🌐 Geral | ➕ Conteúdo | 📊 Progressão · 💰 Mercado | [1688](https://forge.sp-tarkov.com/mod/1688/more-energy-drinks) | — | [Hood26/HoodsEnergyDrinks-CSharp](https://github.com/Hood26/HoodsEnergyDrinks-CSharp) | ✅ | Adiciona 38 energéticos com efeitos (regen de stamina, buffs de skill e drawbacks) — SPT 4.0.13 | 🟢 Instalar | 🔍 | New | — |
| 152 | Raid Review | 🔀 Misto | ⚔️ Raid | 🛋️ QoL | 🗺️ Mapas · 🖼️ UI | [1479](https://forge.sp-tarkov.com/mod/1479/raid-review) | — | [Chazut/SPT-RaidReview](https://github.com/Chazut/SPT-RaidReview) | ✅ | Sistema web de replay de raids com heatmap, kills, loots e dados posicionais — SPT 4.0.13 | 🟢 Instalar | 🔍 | New | — |
| 153 | Franchi SPAS-12 | 🌐 Server | 🌐 Geral | ➕ Conteúdo | 🔫 Armas | [2721](https://forge.sp-tarkov.com/mod/2721/franchi-spas-12) | — | [Eco9341/Franchi-SPAS-12](https://github.com/Eco9341/Franchi-SPAS-12) | ✅ | Adiciona a escopeta Franchi SPAS-12 (requer WTT-CommonLib v2.0.20+) — SPT 4.0.13. Autor: Eco | 🟢 Instalar | 🔍 | New | — |

## Próximos passos

1. **Mods ainda com campos `🔍` críticos** (Tipo, Forge ou Repo 4.0 desconhecido):
   - `BRNVG_N-15Adapter`, `HollywoodGraphics`, `VisceralCombat`, `VolumetricBloodFX` — sem presença pública no Forge; confirmar se são standalone ou sub-pastas de outros mods
   - `seasoniterator` — não localizado; pode ser mod privado/Discord-only
   - Mods internos `🏠`: Band-Aid, CoordLogger, FixReloadUltraFika, ForceSync, GhostMercenaries, UmbigoPreto-* — preencher Prioridade quando stack UltraFika for definida

2. **Decidir destino dos `🟠 Aguardar upstream`** (5 mods pendentes):

   | Mod | Último SPT | Decisão sugerida |
   |---|---|---|
   | SVM | 3.11 | Aguardar — comunidade depende |
   | AmandsGraphics | 3.10 | Aguardar ou ⚫ Não incluir |
   | Realism | 3.9.x | Aguardar — scope grande |
   | IhanaMies-LootValue | 3.11 | Aguardar |
   | Terkoiz.Freecam | 3.11 | Aguardar (acidphantasm mantém) |

3. **Criar specs individuais** em `docs/migration/<mod-name>/` para os 10 mods `🏠` com status `⬆️ Evoluir p/ 4.0` — começar por UltraFika-Plugin (bloqueia tudo)

4. **Mapear dependências entre mods** — dependências conhecidas:
   - SAIN → BigBrain + Waypoints
   - Skwizzy-LootingBots → BigBrain
   - UltraFika-Plugin → Fika (upstream)

5. **Definir Prioridade** — coluna ainda `🔍` em todos os 104 mods; priorizar pelo menos os `🟢 À Instalar` que serão instalados na primeira build

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
| 2026-05-03 | Guilherme | docs(migration): research forge URLs for ~80 mods + scenario taxonomy |
| 2026-05-03 | Guilherme | docs(migration): add Cenário column (Hideout/Raid/Ambos/Geral) to inventory |
| 2026-05-03 | Guilherme | docs(migration): rename Cenário column to Atuação |
| 2026-05-03 | Guilherme | docs(migration): add # counter + Categoria/Escopo taxonomy classifying all 104 mods |
| 2026-05-03 | Guilherme | docs(migration): classify Tipo for ~40 mods using Forge metadata and GitHub repo inspection |
| 2026-05-03 | Guilherme | docs(migration): review pass — fix acidphantasm Tipo (Server→Client 4.0), internal mods Tipo, wrong Forge ID #930→#922 (LessRestrictingHeadwear), PlayerEncumbranceBar Repo 4.0, Próximos passos |
| 2026-05-03 | Guilherme | docs(migration): classify Tipo for 41 mods + add Atuação/Categoria/Escopo columns |
| 2026-05-03 | Guilherme | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-03 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-03 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-03 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-03 | Guilherme | docs(migration): review pass — fix 7 issue categories |
| 2026-05-04 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-03 | Guilherme | refactor(migration): rename inventary → inventory across all files |
| 2026-05-04 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-03 | Guilherme | docs(migration): fill Repo 3.x / 4.0 for 76 mods — replace 🔍 with links or Não encontrado |
| 2026-05-04 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-03 | Guilherme | feat(migration): add SPT 4.0? column to mods inventory |
| 2026-05-09 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-09 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-09 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-09 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-10 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-10 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-11 | Guilherme | chore(backlog): standardize naming convention to NNN-{slug}-NN-{type}.md |
| 2026-05-12 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-12 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-13 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-13 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-13 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-13 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-13 | Guilherme | feat(workflow+mods): backlog 002/003 + workflow infra + vendored mods |
| 2026-05-13 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-14 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-14 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-14 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-16 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-16 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-17 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-17 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-17 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-17 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-17 | Guilherme | docs(migration): add 6 mods, correct SPT 4.0 status for StashSearch/BeltSlot, gray out NaoIncluir rows |
| 2026-05-20 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-05-20 | Guilherme | docs(tarkov-itemdb): consolidate flea price formula investigation + override-only plan |
| 2026-06-02 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-06-02 | Guilherme | chore: commit pending work across tarkov-itemdb viewer, pipeline and RZCustomProfiles |
| 2026-06-03 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-06-03 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-06-03 | Guilherme | docs(migration): add mods 138 (WTT Menu Overhaul) + 139 (Discord Raid Map) |
| 2026-06-04 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-06-04 | Guilherme | chore(claude): allow inventory scripts and chrome-devtools MCP without prompts |
| 2026-06-04 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-06-04 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-06-04 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-06-04 | Guilherme | feat: add /extract-discord-mods-topic command |
| 2026-06-05 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-06-05 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-06-05 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-06-05 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-06-06 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-06-06 | Guilherme | docs: add Discord dev-channel analysis for Realism Mod |
| 2026-06-07 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-06-11 | sync-script | docs(migration): sync mods-inventory.html from markdown |
| 2026-06-10 | Guilherme | feat(AutoGym): vendor mod + workout body skin swap (item 001) |
| 2026-06-22 | Guilherme | wip(customclasses): rename Fantasma->Furtivo + custom skills/signature-patches backlog + launcher icons |
