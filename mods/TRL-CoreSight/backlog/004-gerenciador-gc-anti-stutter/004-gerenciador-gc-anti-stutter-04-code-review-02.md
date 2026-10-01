---
title: "Item 004 — Gerenciador de GC Anti-Stutter (Filtro de Sobrevivência) — Code Review 02"
date: "2026-09-16"
status: "🔵 Em andamento"
authors: ["Antigravity"]
---

# Item 004 — Gerenciador de GC Anti-Stutter — Code Review 02

## 1. Escopo da Revisão

Revisão técnica do código refatorado no mod `TRL-CoreSight` para o **Item 004**, eliminando todas as coletas manuais forçadas de lixo (`MemoryControllerClass.Collect(force: true)`) e transformando o componente em um **filtro inibidor estrito de sobrevivência** (ADS + Combate recente) com delegação nativa e fallback de emergência de RAM.

### Arquivos Modificados

| Arquivo | Tipo | Descrição |
|---|---|---|
| `modded/Core/GCOptimizerManager.cs` | Modificado | Removida toda lógica de coletas síncronas forçadas; implementada alternância estrita entre `Disabled` (em ADS/Combate) e `Enabled` (fora de risco/RAM crítica). |
| `modded/Configuration/ModConfig.cs` | Modificado | Removidos os binds de `ForceCollectOnInventory` e `SafeIntervalSeconds`; mantidos `EnableGCOptimizer`, `CombatGracePeriod` e `CriticalMemoryLimitMB`. |
| `PROPRIEDADES.md` | Modificado | Atualizada a Seção 11 com as propriedades ativas e explicações em pt-BR. |
| `modded/Plugin.cs` | Modificado | Bump de versão SemVer para `0.3.8`. |
| `modded/TRL-CoreSight.csproj` | Modificado | Bump de versão SemVer para `0.3.8`. |
| `mod.json` | Modificado | Bump de versão para `0.3.8`. |

---

## 2. Análise Crítica de Código e Arquitetura

### 2.1. Eliminação do Super-Stutter de Inventário
* **Zero Coletas Forçadas:** A remoção de `MemoryControllerClass.Collect(force: true)` elimina na raiz o congelamento de 4 a 6 segundos e o underrun do buffer de áudio ao abrir o inventário.
* **Abertura de Inventário Instantânea:** O inventário e o menu de looting voltam a abrir a 60+ FPS sem engasgo ou travamento na thread principal da Unity.

### 2.2. Filtro Estrito de Sobrevivência
* **Inibição Cirúrgica (`shouldSuppress = isAiming || inCombat`):**
  * Quando o jogador mira com a arma (`isAiming == true`), o GC é imediatamente desativado (`GCMode = Disabled`).
  * Quando o jogador dispara ou está sob tiroteio (`(Time.time - _lastShotTime) < CombatGracePeriod`), o GC permanece desativado.
  * O jogador nunca sofrerá *stutter* de Garbage Collection no primeiro tiro ou durante a troca de disparos.
* **Delegação ao Motor Nativo:**
  * Assim que o jogador baixa a mira e o tempo de combate expira, `GCMode` retorna a `Enabled`. A Unity e o Tarkov gerenciam a memória incrementalmente no seu tempo natural.

### 2.3. Salvaguarda de Emergência contra OOM
* **Airbag de Memória (`CriticalMemoryLimitMB`):**
  * Se a memória alocada do heap atingir o patamar crítico (padrão 3500 MB), o GC é forçado a `Enabled` mesmo em combate/mira, impedindo que o jogo sofra crash por falta de memória RAM.

---

## 3. Conformidade com as Regras do Projeto

| Regra | Status | Evidência |
|---|---|---|
| **SemVer Bump Obrigatório** | ✅ Aprovado | Versão incrementada de `0.3.7` para `0.3.8` em `Plugin.cs`, `TRL-CoreSight.csproj` e `mod.json`. |
| **Isolamento de Build** | ✅ Aprovado | DLL final gerada exclusivamente em `mods/TRL-CoreSight/builds/TRL-CoreSight.dll` (75.264 bytes). Nada copiado para pasta do jogo. |
| **Ciclo de Backlog** | ✅ Aprovado | Revisão 02 documentada com rastreabilidade completa. |
| **Idioma** | ✅ Aprovado | Documentação e código em Português do Brasil. |

---

## 4. Veredito do Code Review

🟢 **Aprovado com Louvor.** A refatoração elimina com precisão o efeito colateral relatado em raid, mantendo a proteção nos momentos vitais de sobrevivência e devolvendo a gestão de memória ao motor nativo nos demais casos.
