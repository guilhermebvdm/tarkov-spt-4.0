---
title: 001 — fika-poolmanager-nre · As-Built
date: 2026-07-22
status: 🟢 Vivo
authors: [Antigravity]
---

# 001 — fika-poolmanager-nre · As-Built

## Arquivos Modificados/Criados

### [Plugin.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-Fixes/modded/Plugin.cs) [MODIFY]
* Habilita a inicialização do patch `Patch_PoolManagerCreateItem` no Awake.

### [Patch_PoolManagerCreateItem.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-Fixes/modded/Patches/Patch_PoolManagerCreateItem.cs) [NEW]
* Implementa o prefixo Harmony para interceptar `PoolManagerClass.CreateItem` de 4 parâmetros para jogadores remotos.
