---
title: Novos mods — SPT 4.0
date: 2026-05-03
status: ⚫ Arquivado
authors: Guilherme
---

# Novos mods — SPT 4.0

> **⚫ Arquivado (2026-06-03).** Lista obsoleta — estes mods foram absorvidos no inventário único [`mods-inventory.md`](../mods-inventory.md). Novos mods agora entram direto lá (via `/add-mod-inventory-list` ou edição manual + `node scripts/sync-mods-html.js`). Mantido só por histórico.

Mods selecionados para entrar na stack do SPT 4.0 que **não estavam** no inventário original de migração (`mods-inventory.md`). Todos já têm versão 4.0 disponível — status padrão: 🟢 Instalar.

Para convenções de colunas (Status, Prioridade, Tipo, Atuação, Categoria, Escopo), ver [mods-inventory.md](mods-inventory.md).

## Inventário

| # | Mod | Tipo | Atuação | Categoria | Escopo | Forge | Repo 4.0 | Função | Status | Prioridade |
|---|---|---|---|---|---|---|---|---|---|---|
| 1 | WTT-PackNStrap | 🔀 Misto | 🔀 Ambos | ➕ Conteúdo | 🛡️ Equipamentos · 📦 Inventário | [1278](https://forge.sp-tarkov.com/mod/1278/wtt-pack-n-strap) | [WelcomeToTarkov/PackNStrap](https://github.com/WelcomeToTarkov/PackNStrap) | Battle belt + small cases (WTT team) — v2.0.4 (SPT 4.0.13). Mesmo repo | 🟢 Instalar | 🔍 |
| 2 | Climbable Ladders | 🖥️ Client | ⚔️ Raid | 🩸 Realismo | 🗺️ Mapas · 🎬 Animações | [2649](https://forge.sp-tarkov.com/mod/2649/climbable-ladders) | [bmpq/spt-ladders](https://github.com/bmpq/spt-ladders) | Habilita escalada de escadas em vários mapas (woods bunker, customs watchtower etc.) — SPT 4.0.13 | 🟢 Instalar | 🔍 |
| 3 | Magazine Check Interrupt | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | 🎯 Munições · 🎬 Animações | [2643](https://forge.sp-tarkov.com/mod/2643/magazine-check-interrupt) | [ozen-m/SPT-MagCheckInterrupt](https://github.com/ozen-m/SPT-MagCheckInterrupt) | Transição contínua de magazine check para reload sem sair da animação — SPT 4.0.13 | 🟢 Instalar | 🔍 |
| 4 | Stance Sync | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | 🎬 Animações · 🔫 Armas | [2639](https://forge.sp-tarkov.com/mod/2639/stance-sync) | [minihazel/StanceSync](https://github.com/minihazel/StanceSync) | Sincroniza lean e shoulder swap; opção para desabilitar lean sincronizado ao mirar — SPT 4.0.13 | 🟢 Instalar | 🔍 |
| 5 | Stat Rewards | 🖥️ Client | 🌐 Geral | 📈 Progressão | 📊 Progressão | [2655](https://forge.sp-tarkov.com/mod/2655/stat-rewards) | [Chazut/StatRewards](https://github.com/Chazut/StatRewards) | Recompensas aleatórias ao atingir marcos de stats (kills, dano, loot etc.) — 48 milestones repetíveis, configurável — SPT 4.0.13 | 🟢 Instalar | 🔍 |
| 6 | Brighter Interiors | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | ✨ Gráficos | [2613](https://forge.sp-tarkov.com/mod/2613/brighter-interiors) | [7Bpencil/SPT-ReduceFakeInteriorShadow](https://github.com/7Bpencil/SPT-ReduceFakeInteriorShadow) | Reduz intensidade de sombras internas para melhorar visibilidade em ambientes fechados — SPT 4.0.13 | 🟢 Instalar | 🔍 |
| 7 | RZCustomProfiles | 🖥️ Client | 🌐 Geral | 🛋️ QoL | 🖼️ UI | [2614](https://forge.sp-tarkov.com/mod/2614/rzcustomprofiles) | [remzdnb/RZ-SPTMods](https://github.com/remzdnb/RZ-SPTMods) | Gerenciamento de templates de perfil de personagem — v1.1.0 (SPT 4.0.13). Autor: RemzDNB | 🟢 Instalar | 🔍 |
| 8 | LoadBundleEvenFaster | 🖥️ Client | 🌐 Geral | 🛋️ QoL | ✨ Gráficos | [2599](https://forge.sp-tarkov.com/mod/2599/loadbundleevenfaster) | [s8ga/SPT_LoadBundleEvenFaster](https://github.com/s8ga/SPT_LoadBundleEvenFaster) | Reduz tempo de carregamento de bundles — camada extra sobre LoadBundleFaster. **Dep:** LoadBundleFaster (Crc32 Patch) ≥ v1.0.0. SPT 4.0.13 | 🟢 Instalar | 🔍 |
| 9 | Recoil Rework (Legacy) | 🌐 Server | ⚔️ Raid | 🩸 Realismo | 🔫 Armas | [2190](https://forge.sp-tarkov.com/mod/2190/recoil-rework-legacy) | [peinwastaken/SPTRecoilRework](https://github.com/peinwastaken/SPTRecoilRework) | Reescreve mecânica de recoil das armas (autor: pein) — v1.10.0 recompilada para SPT 4.0.13 | 🟢 Instalar | 🔍 |
| 10 | Gunsmith Barters | 🌐 Server | 🌐 Geral | 🔥 Hardcore | 💰 Mercado · 🔫 Armas | [1963](https://forge.sp-tarkov.com/mod/1963/gunsmith-barters) | [Solethia/Kipperworks.GunsmithBarters](https://github.com/Solethia/Kipperworks.GunsmithBarters) | Adiciona barters no Mecânico voltados a runs hardcore — autor: Solethia, v2.0.2 (SPT 4.0.13). ⚠️ pode fazer alterações permanentes no perfil | 🟢 Instalar | 🔍 |
| 11 | Camera Position Control & Custom Weapon Stances | 🖥️ Client | ⚔️ Raid | 🛋️ QoL | 🎬 Animações · 🔫 Armas | [2572](https://forge.sp-tarkov.com/mod/2572/camera-position-control-custom-weapon-stances) | [shengzhanzhe/stancesAndCameraPositionSPT4.0.11](https://github.com/shengzhanzhe/stancesAndCameraPositionSPT4.0.11) | Controle customizável de posição da câmera FPV e stances de arma — autor: shenghanzhe, v1.1.5 (SPT 4.0.13). ⚠️ ao atualizar de ≤0.9.7 remover `CameraRotationMod.dll` antigo | 🟢 Instalar | 🔍 |
| 12 | Manimal's Ammo Loading Animations | 🖥️ Client | ⚔️ Raid | 🩸 Realismo | 🎯 Munições · 🎬 Animações | [2681](https://forge.sp-tarkov.com/mod/2681/manimals-ammo-loading-animations) | [danauraborealis/ManimalAmmoLoadingAnimations](https://github.com/danauraborealis/ManimalAmmoLoadingAnimations) | Animações em primeira pessoa para carregar munição em magazines — autores: manimal1920 / GrooveypenguinX, v1.5.0 (SPT 4.0.13). **Dep:** WTT-CommonLib ≥ v2.0.20. ⚠️ pode fazer alterações permanentes no perfil; incompatível com Fika | 🟢 Instalar | 🔍 |

## Histórico

| Data | Autor | Descrição |
|---|---|---|
| 2026-05-04 | Guilherme | feat(profiles): RZCustomProfiles 10 perfis customizados com loadouts calibrados |
| 2026-05-08 | Guilherme | feat(mods): add stancesAndCameraPositionSPT4.0.11 with stamina/speed by stance |
