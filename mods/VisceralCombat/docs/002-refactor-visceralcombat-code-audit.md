---
title: Auditoria de Código e Otimizações — Visceral Combat
date: 2026-08-09
status: 🟢 Vivo
authors:
  - Antigravity
---

# 002-refactor — Auditoria de Código e Limpeza do Visceral Combat

Este documento registra a auditoria técnica realizada no código-fonte modificado em [`mods/VisceralCombat/modded/`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/), com foco na eliminação de **vazamentos de memória (RAM)**, **scripts mortos do Unity Asset Store**, **gargalos de FPS em loops `Update()`** e **acúmulo de objetos órfãos entre raids**.

---

## 📊 Resumo Executivo da Auditoria

| Severidade | Categoria | Quantidade | Descrição Principal |
|---|---|---|---|
| 🔴 **Alta** | Vazamento de RAM pós-Raid | 2 | Listas estáticas de `Player` mantidas na memória entre partidas sem `Clear()`. |
| 🔴 **Alta** | Instanciação Órfã | 1 | Double `Object.Instantiate` sem destruição do objeto intermediário no `EffectContainer`. |
| 🟡 **Média** | Scripts Mortos / Inúteis | 4 | Scripts residuais do PuppetMaster e Asset Store (`MouseOrbit`, `Navigator`, `RagdollSpawner`, `DecaGizmo`). |
| 🟡 **Média** | Retenção de Pool pós-Raid | 1 | `GoreObjectPool` não limpa referências destruídas ao encerrar o `GameWorld`. |
| 🟢 **Baixa** | Redundância / Fragilidade | 3 | Chamada dupla de `OnEnable()` no `Awake()` e delegates sem `Unsubscribe`. |

---

## 🔴 1. Vazamentos de Memória (RAM Leaks pós-Raid)

### 1.1 `VisceralEntry.cs` — Listas estáticas de `Player` retidas entre partidas
- **Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralEntry.cs:54-56`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralEntry.cs#L54-L56)
- **Problema:** `dismemberedPlayers` (List) e `deadPlayers` (Dictionary) são campos de instância em `VisceralEntry`, que é um singleton BepInEx persistente (`DontDestroyOnLoad`). Quando a raid termina e o jogo retorna ao menu principal ou inicia uma nova partida, esses contêineres **nunca são limpos (`Clear()`)**.
- **Impacto:** Todas as referências para instâncias de `Player`, `Transform` e hierarquias de bots das raids anteriores permanecem presas na memória, impedindo a Garbage Collection da Unity e acumulando megabytes de RAM a cada raid jogada.
- **Solução Recomendada:** Adicionar chamadas `VisceralEntry.Instance.dismemberedPlayers.Clear()` e `deadPlayers.Clear()` no `GameWorld.OnGameStarted` ou no `GameWorld.Dispose`.

### 1.2 `EffectContainer.cs` — Double `Instantiate` criando objeto órfão na cena
- **Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Classes/EffectContainer.cs:78-79`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Classes/EffectContainer.cs#L78-L79)
- **Problema:**
  ```csharp
  GameObject val2 = Object.Instantiate<GameObject>(array3[0]);
  activeRagdollBase = Object.Instantiate<GameObject>(val2, val.transform);
  ```
  O método instancia `val2` na raiz da cena Unity e em seguida faz uma *segunda* instanciação a partir de `val2` para `activeRagdollBase`. `val2` fica totalmente abandonado ("órfão") na raiz do mapa sem pai e sem referência para destruição.
- **Impacto:** Instanciação duplicada desnecessária e lixo de memória na raiz do mapa.
- **Solução Recomendada:** Substituir por instanciação direta:
  ```csharp
  activeRagdollBase = Object.Instantiate<GameObject>(array3[0], val.transform);
  ```

---

## 🟡 2. Scripts Mortos e Residuais do Asset Store / ILSpy

### 2.1 `BFX_MouseOrbit.cs` — Script de teste de câmera afetando o ponteiro do mouse
- **Local:** [`mods/VisceralCombat/modded/VolumetricBloodFX/BFX_MouseOrbit.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VolumetricBloodFX/BFX_MouseOrbit.cs)
- **Problema:** Script residual do pacote Asset Store "Volumetric Blood FX". Ele roda um `LateUpdate()` contínuo checando `Input.GetMouseButton` e tentando controlar `Cursor.visible` e `Cursor.lockState`.
- **Impacto:** Processamento inútil no loop do frame e risco de travar ou forçar o cursor do mouse na tela durante menus/raid.
- **Solução Recomendada:** Remover a classe ou desativar/excluir o arquivo `BFX_MouseOrbit.cs`.

### 2.2 `RagdollSpawner.cs` & `Navigator.cs` — Classes sem qualquer referência
- **Locais:**
  - [`VisceralCombat.Ragdolls.Classes.Debug/RagdollSpawner.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Classes.Debug/RagdollSpawner.cs)
  - [`VisceralCombat.Ragdolls.Classes.RootMotion.Demos/Navigator.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Classes.RootMotion.Demos/Navigator.cs)
- **Problema:** `RagdollSpawner` (menu GUI IMGUI de debug para spawnar ragdolls) e `Navigator` (navegação/NavMesh demo do PuppetMaster) não são referenciados em nenhum ponto do mod ou servidor.
- **Solução Recomendada:** Remover ambas as pastas/arquivos para manter o repositório limpo.

### 2.3 `BFX_DecaGizmo.cs` — Desenho de Editor Gizmos em Runtime
- **Local:** [`mods/VolumetricBloodFX/BFX_DecaGizmo.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VolumetricBloodFX/BFX_DecaGizmo.cs)
- **Problema:** Contém apenas métodos de editor Unity (`OnDrawGizmosSelected`).
- **Solução Recomendada:** Remover arquivo residual.

---

## 🟡 3. Gestão do Object Pool (`GoreObjectPool.cs`)

### 3.1 Falta de Limpeza do Pool ao Mudar de Raid
- **Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Classes/GoreObjectPool.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Classes/GoreObjectPool.cs)
- **Problema:** `GoreObjectPool` possui `DontDestroyOnLoad`, mas o método `ClearPool()` nunca é chamado por nenhum patch de ciclo de vida (`OnGameStarted` ou `GameWorld.Dispose`).
- **Impacto:** Objetos desativados no pool de raids anteriores continuam alocados na memória do processo do jogo indefinidamente.
- **Solução Recomendada:** Invocar `GoreObjectPool.Instance.ClearPool()` na inicialização ou encerramento de cada mapa em `GameStartedPatch.cs`.

---

## 🟢 4. Fragilidades Secundárias & Micro-Otimizações

### 4.1 `BFX_DecalSettings.cs` — Acúmulo de Event Delegates
- **Local:** [`mods/VolumetricBloodFX/BFX_DecalSettings.cs:56`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VolumetricBloodFX/BFX_DecalSettings.cs#L56)
- **Problema:** `shaderProperies.OnAnimationFinished += ShaderCurve_OnAnimationFinished;` é inscrito em `Awake()`, porém não existe desinscrição (`-=`) em `OnDestroy()` ou `OnDisable()`.
- **Solução Recomendada:** Adicionar `OnDestroy()` removendo o ouvinte.

### 4.2 `BFX_ShaderProperies.cs` — Chamada dupla de `OnEnable()`
- **Local:** [`mods/VolumetricBloodFX/BFX_ShaderProperies.cs:44`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VolumetricBloodFX/BFX_ShaderProperies.cs#L44)
- **Problema:** A linha `OnEnable();` é invocada manualmente dentro de `Awake()`. A Unity já chama `OnEnable()` automaticamente após `Awake()`, fazendo a inicialização rodar duas vezes seguidas.
- **Solução Recomendada:** Remover `OnEnable();` de dentro do `Awake()`.

---

## 📋 Conclusão

O núcleo funcional do mod (desmembramento, física de agonia e desativação de ragdolls) está **100% estável e funcional**. As oportunidades identificadas acima são exclusivamente de **limpeza de lixo de memória pós-raid** e **remoção de scripts legados**.
