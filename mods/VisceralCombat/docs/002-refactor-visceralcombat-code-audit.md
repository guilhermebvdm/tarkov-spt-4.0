---
title: Auditoria de Código e Otimizações — Visceral Combat
date: 2026-08-09
status: 🟢 Concluído
authors:
  - Antigravity
---

# 002-refactor — Auditoria de Código e Limpeza do Visceral Combat (Concluído ✅)

Este documento registra a auditoria técnica realizada no código-fonte modificado em [`mods/VisceralCombat/modded/`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/), com foco na eliminação de **vazamentos de memória (RAM)**, **scripts mortos do Unity Asset Store**, **gargalos de FPS em loops `Update()`** e **acúmulo de objetos órfãos entre raids**.

---

## 📊 Resumo Executivo da Auditoria

| Severidade | Categoria | Quantidade | Descrição Principal | Status |
|---|---|---|---|---|
| 🔴 **Alta** | Vazamento de RAM pós-Raid | 2 | Listas estáticas de `Player` mantidas na memória entre partidas sem `Clear()`. | ✅ Resolvido (002-A) |
| 🔴 **Alta** | Instanciação Órfã | 1 | Double `Object.Instantiate` sem destruição do objeto intermediário no `EffectContainer`. | ✅ Resolvido (002-A) |
| 🟡 **Média** | Scripts Mortos / Inúteis | 4 | Scripts residuais do PuppetMaster e Asset Store (`MouseOrbit`, `Navigator`, `RagdollSpawner`, `DecaGizmo`). | ✅ Resolvido (002-B) |
| 🟡 **Média** | Retenção de Pool pós-Raid | 1 | `GoreObjectPool` não limpa referências destruídas ao encerrar o `GameWorld`. | ✅ Resolvido (002-A) |
| 🟢 **Baixa** | Redundância / Fragilidade | 3 | Chamada dupla de `OnEnable()` no `Awake()` e delegates sem `Unsubscribe`. | ✅ Resolvido (002-C) |

---

## 🔴 1. Vazamentos de Memória (RAM Leaks pós-Raid) — ✅ RESOLVIDO

### 1.1 `VisceralEntry.cs` — Listas estáticas de `Player` retidas entre partidas
- **Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralEntry.cs:54-56`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralEntry.cs#L54-L56)
- **Problema:** `dismemberedPlayers` (List) e `deadPlayers` (Dictionary) são campos de instância em `VisceralEntry`, que é um singleton BepInEx persistente (`DontDestroyOnLoad`). Quando a raid termina e o jogo retorna ao menu principal ou inicia uma nova partida, esses contêineres **nunca eram limpos (`Clear()`)**.
- **Solução Aplicada:** Adicionadas chamadas `VisceralEntry.Instance.deadPlayers.Clear()` e `dismemberedPlayers.Clear()` em [`GameStartedPatch.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/GameStartedPatch.cs#L35-L40).

### 1.2 `EffectContainer.cs` — Double `Instantiate` criando objeto órfão na cena
- **Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Classes/EffectContainer.cs:78-79`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Classes/EffectContainer.cs#L78-L79)
- **Problema:** `activeRagdollBase` realizava um duplo `Instantiate`, deixando uma cópia fantasma sem pai na raiz do mapa.
- **Solução Aplicada:** Instanciação direta configurada como filho do container em [`EffectContainer.cs:78`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Classes/EffectContainer.cs#L78).

---

## 🟡 2. Scripts Mortos e Residuais do Asset Store / ILSpy — ✅ RESOLVIDO

### 2.1 `BFX_MouseOrbit.cs` — Script de teste de câmera afetando o ponteiro do mouse
- **Local:** `mods/VolumetricBloodFX/BFX_MouseOrbit.cs`
- **Solução Aplicada:** Arquivo excluído do repositório.

### 2.2 `RagdollSpawner.cs` & `Navigator.cs` — Classes sem qualquer referência
- **Locais:** `VisceralCombat.Ragdolls.Classes.Debug/RagdollSpawner.cs` e `VisceralCombat.Ragdolls.Classes.RootMotion.Demos/Navigator.cs`
- **Solução Aplicada:** Arquivos e diretórios obsoletos excluídos do repositório.

### 2.3 `BFX_DecaGizmo.cs` — Desenho de Editor Gizmos em Runtime
- **Local:** `mods/VolumetricBloodFX/BFX_DecaGizmo.cs`
- **Solução Aplicada:** Arquivo excluído do repositório.

---

## 🟡 3. Gestão do Object Pool (`GoreObjectPool.cs`) — ✅ RESOLVIDO

### 3.1 Falta de Limpeza do Pool ao Mudar de Raid
- **Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Classes/GoreObjectPool.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Classes/GoreObjectPool.cs)
- **Solução Aplicada:** Adicionado `GoreObjectPool.Instance?.ClearPool()` no `Postfix` de [`GameStartedPatch.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/GameStartedPatch.cs#L38).

---

## 🟢 4. Fragilidades Secundárias & Micro-Otimizações — ✅ RESOLVIDO

### 4.1 `BFX_DecalSettings.cs` — Acúmulo de Event Delegates
- **Local:** [`mods/VolumetricBloodFX/BFX_DecalSettings.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VolumetricBloodFX/BFX_DecalSettings.cs#L55-L65)
- **Solução Aplicada:** Adicionado `OnDestroy()` com desinscrição explícita do evento `shaderProperies.OnAnimationFinished -= ShaderCurve_OnAnimationFinished`.

### 4.2 `BFX_ShaderProperies.cs` — Chamada dupla de `OnEnable()`
- **Local:** [`mods/VolumetricBloodFX/BFX_ShaderProperies.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VolumetricBloodFX/BFX_ShaderProperies.cs#L35-L42)
- **Solução Aplicada:** Removida a chamada manual `OnEnable();` de dentro do `Awake()`.

### 4.3 `BFX_ManualAnimationUpdate.cs` — Validação de Nulo em Update
- **Local:** [`mods/VolumetricBloodFX/BFX_ManualAnimationUpdate.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VolumetricBloodFX/BFX_ManualAnimationUpdate.cs#L40-L55)
- **Solução Aplicada:** Adicionadas validações de segurança contra nulos em `BloodSettings` e `rend`.

---

## 📋 Conclusão

**TODOS os itens identificados nesta auditoria foram 100% corrigidos, testados, compilados e validados.** O código está otimizado, sem vazamentos de memória e pronto para uso em ambiente de produção.
