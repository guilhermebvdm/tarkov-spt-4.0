---
title: "Item 004 — Gerenciador de GC Anti-Stutter (Filtro de Sobrevivência) — As-Built"
date: "2026-09-16"
status: "🔵 Concluído"
authors: ["Antigravity"]
---

# Item 004 — Gerenciador de GC Anti-Stutter (Filtro de Sobrevivência) — As-Built

## 1. Resumo Executivo

O Item 004 foi reformulado na versão **0.3.8** do mod **TRL-CoreSight** para eliminar completamente o "super-stutter" de 4 a 6 segundos relatado em raid real ao abrir o inventário (causado por invocações manuais de `MemoryControllerClass.Collect(force: true)`).

A nova arquitetura transforma o componente em um **filtro inibidor estrito de momentos de sobrevivência**:
1. **Inibição Cirúrgica:** O coletor de lixo da Unity é desativado (`GCMode = Disabled`) exclusivamente quando o jogador está com a mira acionada (**ADS**) ou em **combate ativo** (janela recente de disparos).
2. **Delegação Nativa:** Fora de combate e fora de mira, o GC permanece ativo de forma nativa (`GCMode = Enabled`), permitindo que a Unity e o Tarkov gerenciem a limpeza de memória de forma suave e incremental, sem sobressaltos nem travamentos de tela.
3. **Airbag contra OOM:** Se a memória alocada do heap atingir o limite crítico configurado (`CriticalMemoryLimitMB`), o GC nativo é liberado imediatamente para evitar crash por falta de memória RAM.

---

## 2. Componentes e Funcionalidades Implementadas

### 2.1. Filtro de Sobrevivência (`Core/GCOptimizerManager.cs`)
- **Zero Coletas Forçadas:** Eliminadas todas as chamadas a `MemoryControllerClass.Collect` e `GC.Collect`. O mod nunca mais força faxinas síncronas bloqueantes.
- **Inibição em ADS e Combate:**
  - `isAiming`: Verificação de mira ativa via `ProceduralWeaponAnimation.IsAiming`.
  - `inCombat`: Janela de proteção em segundos pós-disparos (`Time.time - _lastShotTime < CombatGracePeriod`).
  - Quando qualquer condição for verdadeira, define `GarbageCollector.GCMode = GarbageCollector.Mode.Disabled`.
- **Modo Nativo Fora de Perigo:**
  - Assim que o jogador encerra a visada e a janela de combate expira, `GarbageCollector.GCMode` é restaurado para `Enabled`.
- **Salvaguarda de Emergência:**
  - Se `AllocatedMemoryMB >= CriticalMemoryLimitMB`, o GC permanece em `Enabled` mesmo em combate, prevenindo crashes de Out-Of-Memory.
- **Encerramento e Morte:**
  - Se o jogador for eliminado na partida ou ao sair da raid (`Cleanup()`), o GC é imediatamente restaurado para `Enabled`.

### 2.2. Hook de Disparos Balísticos (`Patches/WeaponShotGCPatch.cs`)
- Intercepta `WeaponManagerClass.StartSpawnShell`.
- Quando o jogador local ou um bot a menos de 50 metros efetua um disparo, o timestamp `_lastShotTime` é atualizado, estendendo a janela de inibição de GC.

### 2.3. Menu BepInEx F12 (`Configuration/ModConfig.cs` e `PROPRIEDADES.md`)
Seção **`11. Anti-Stutter (Garbage Collection)`**:
- `EnableGCOptimizer` (`bool`, padrão `true`): Inibe a coleta de lixo da Unity durante combates e mira (ADS) para eliminar stutters, mantendo o GC nativo fora de risco.
- `CombatGracePeriod` (`float`, padrão `5.0s`, faixa `2.0s` a `15.0s`): Tempo em segundos após o último disparo de arma para manter a inibição de GC ativa.
- `CriticalMemoryLimitMB` (`float`, padrão `3500.0MB`, faixa `2000.0MB` a `6000.0MB`): Limite de segurança de memória alocada para forçar a liberação do GC nativo e evitar crash por falta de RAM (OOM).

---

## 3. Arquivos Modificados

| Arquivo | Ação | Descrição |
|---|---|---|
| `modded/Core/GCOptimizerManager.cs` | MODIFICAR | Remoção de coletas forçadas e redefinição para filtro inibidor estrito em ADS/Combate com fallback de RAM. |
| `modded/Configuration/ModConfig.cs` | MODIFICAR | Remoção de `ForceCollectOnInventory` e `SafeIntervalSeconds`. |
| `PROPRIEDADES.md` | MODIFICAR | Atualização da Seção 11 com as propriedades ativas em pt-BR. |
| `modded/Plugin.cs` | MODIFICAR | Bump SemVer para `0.3.8`. |
| `modded/TRL-CoreSight.csproj` | MODIFICAR | Bump SemVer para `0.3.8`. |
| `mod.json` | MODIFICAR | Bump de versão para `0.3.8`. |

---

## 4. Verificação e Compilação

- **Compilação:** `dotnet build mods/TRL-CoreSight/modded/TRL-CoreSight.csproj -c Release`
- **Resultado:** 0 Erros, 0 Avisos.
- **Binário isolado gerado:** `mods/TRL-CoreSight/builds/TRL-CoreSight.dll` (75.264 bytes).
- **Regra de isolamento cumprida:** Nada foi copiado para a pasta do jogo.
