---
title: Auditoria de Código, FPS Thief & Configurações F12 — Visceral Combat
date: 2026-08-09
status: 🟢 Vivo
authors:
  - Antigravity
---

# 003-refactor — Auditoria de Desempenho (FPS Thief), Corrotinas e Menu F12

Este documento registra a terceira fase de auditoria técnica no repositório [`mods/VisceralCombat/modded/`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/), com foco na eliminação de **gargalos de FPS durante tiroteios intensos (Corrotinas `WatchShot`)**, **conexão real das propriedades placebo do menu F12 (BepInEx Config)** e **limpeza de classes descompiladas do ILSpy**.

---

## 📊 Resumo Executivo da Auditoria

| Severidade | Categoria | Quantidade | Descrição Principal |
|---|---|---|---|
| 🔴 **Alta** | Desempenho / CPU (FPS Thief) | 3 | Corrotinas `WatchShot` disparadas no `StaticManager` a cada bala no ar em `LimbKillPatch`, `BodiesImpulsePatch` e `BleedPatch`. |
| 🟡 **Média** | Configurações Placebo (F12) | 5 | Propriedades BepInEx (`headForceIntensity`, `TorsoForceIntensity`, `MappingWeightDuration`, etc.) ignoradas ou desconectadas da física real. |
| 🟢 **Baixa** | Legibilidade / ILSpy Residue | 3 | Classes geradas de iterador ILSpy (`_003CWatchShot_003E...`) e sliders abandonados (`x`, `y`, `z`, `timer`). |

---

## 🔴 1. Otimização do Sistema de Balística e Tiros (FPS Thief em Tiroteios)

### 1.1 `LimbKillPatch.cs` — Corrotina `WatchShot` disparada a cada tiro
- **Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs:180-189`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs#L180-L189)
- **Problema:** O método `Postfix` do `BallisticsCalculator.Shoot` instancia uma corrotina `WatchShot` em `StaticManager.Instance` para **cada** projétil disparado no jogo. Essa corrotina roda um `while (!shot.IsShotFinished) { yield return null; }` a cada frame para testar se o tiro atingiu um osso morto.
- **Impacto:** Em tiroteios com armas automáticas ou escopetas, dezenas de corrotinas acumulam executando loops a cada frame na Main Thread da Unity.
- **Solução Recomendada:** Executar o ajuste de peso muscular somente se `shot.IsShotFinished` for `true`, eliminando o agendamento de corrotinas desnecessárias no ar.

### 1.2 `BodiesImpulsePatch.cs` — Polling de finalização de disparo via corrotinas
- **Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs:203-214`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs#L203-L214)
- **Problema:** Quando `shot.IsShotFinished` é `false` no disparo inicial, inicia a corrotina `WatchShot(shot)` no `StaticManager`.
- **Solução Recomendada:** Refatorar o listener para processar o impulso direto nos eventos de impacto do jogador/ragdoll ou otimizar o manipulador de término de projétil sem pooling por frame.

### 1.3 `BleedPatch.cs` — Corrotina paralela para efeito visual de sangramento
- **Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Patches/BleedPatch.cs:240-250`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Patches/BleedPatch.cs#L240-L250)
- **Problema:** Terceira corrotina idêntica agendada a cada disparo.
- **Solução Recomendada:** Unificar o tratamento de disparo em um único ponto ou invocar `ProcessWatchShot` de forma limpa sem overhead de corrotinas persistentes.

---

## 🟡 2. Conexão Real das Opções do Menu F12 (BepInEx ConfigurationManager)

### 2.1 Multiplicadores Anatômicos de Impulso (`BodiesImpulsePatch.cs`)
- **Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs:234`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs#L234)
- **Problema:** A força da bala utiliza apenas `VisceralEntry.Instance.ShotIntensity.Value`. As opções de configuração `headForceIntensity`, `TorsoForceIntensity`, `ArmsForceIntensity` e `LegsForceIntensity` existem na interface do menu F12, mas são **completamente ignoradas** na fórmula de impulso.
- **Solução Recomendada:** Multiplicar a força pelo configurável correspondente da parte do corpo atingida (`headForceIntensity.Value`, `TorsoForceIntensity.Value`, etc.).

### 2.2 Duração de Lerp do PuppetMaster (`MappingWeightDuration`)
- **Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs:313`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L313)
- **Problema:** `LerpMappingWeight` usa a constante hardcoded `0.8f`, ignorando a opção `MappingWeightDuration.Value` configurável pelo usuário.
- **Solução Recomendada:** Substituir `0.8f` por `VisceralEntry.Instance.MappingWeightDuration.Value`.

### 2.3 Remoção de Sliders de Teste Abandonados (`VisceralEntry.cs`)
- **Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralEntry.cs:145-151`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralEntry.cs#L145-L151)
- **Problema:** Campos `timer`, `x`, `y` e `z` aparecem no painel F12 como controles numéricos abandonados sem uso.
- **Solução Recomendada:** Remover essas entradas do `Bind()` do BepInEx e das declarações de propriedades.

---

## 🟢 3. Limpeza de Classes Descompiladas do ILSpy

### 3.1 Subclasses `_003CWatchShot_003E...`
- **Locais:** `LimbKillPatch.cs`, `BodiesImpulsePatch.cs`, `BleedPatch.cs`
- **Problema:** Presença de classes internas geradas pelo descompilador com nomes obfuscados (`_003CWatchShot_003Ed__2`).
- **Solução Recomendada:** Reescrever os iteradores/métodos com sintaxe C# limpa `IEnumerator` sem classes stub geradas.

---

## 📋 Conclusão

Esta auditoria define as ações necessárias para tornar o Visceral Combat leve em tiroteios e totalmente configurável através do menu F12 do BepInEx.
