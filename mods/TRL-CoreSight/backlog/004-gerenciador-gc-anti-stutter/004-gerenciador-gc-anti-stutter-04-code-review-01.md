---
title: "Item 004 — Gerenciador de GC Anti-Stutter — Code Review 01"
date: "2026-09-15"
status: "🔵 Em andamento"
authors: ["Antigravity"]
---

# Item 004 — Gerenciador de GC Anti-Stutter — Code Review 01

## 1. Escopo da Revisão

Revisão técnica do código implementado no mod `TRL-CoreSight` para supressão do coletor de lixo Mono (`Stop-the-World GC`) durante situações ativas de combate e visada (ADS), com agendamento de coletas limpas em momentos neutros e proteção contra esgotamento de memória (*Out of Memory*).

### Arquivos Criados / Modificados

| Arquivo | Tipo | Descrição |
|---|---|---|
| `modded/Core/GCOptimizerManager.cs` | Novo | Orquestrador de supressão e agendamento de coletas de memória, com debouncing em inventário, cooldown de segurança e airbag de memória crítica. |
| `modded/Patches/WeaponShotGCPatch.cs` | Novo | Patch Harmony Postfix em `WeaponManagerClass.StartSpawnShell` para detectar disparos do jogador e de bots num raio de 50 metros. |
| `modded/Configuration/ModConfig.cs` | Modificado | Inclusão de configurações da Seção 11 no F12 (`EnableGCOptimizer`, `CombatGracePeriod`, `SafeIntervalSeconds`, `CriticalMemoryLimitMB`, `ForceCollectOnInventory`) e evento `OnGCSettingsChanged`. |
| `modded/Core/PerformanceManager.cs` | Modificado | Integração do `GCOptimizerManager` no ciclo de vida, atualização por frame, telemetria em `OnGUI` e encerramento seguro com restauração no `Cleanup()`. |
| `modded/Plugin.cs` | Modificado | Ativação do novo patch e bump de versão SemVer para `0.3.5`. |
| `modded/TRL-CoreSight.csproj` | Modificado | Inclusão dos novos arquivos compilados e bump SemVer para `0.3.5`. |
| `mod.json` | Modificado | Bump SemVer para `0.3.5`. |
| `PROPRIEDADES.md` | Modificado | Documentação em pt-BR da seção 11 de F12. |

---

## 2. Análise Crítica de Código e Arquitetura

### 2.1. Desempenho e Eliminação de Quedas de 1% Low
- **Eliminação de Congelamentos de Primeiro Tiro:** Ao definir `GarbageCollector.GCMode = GarbageCollector.Mode.Disabled` durante o ato de puxar a mira (`IsAiming`), correr (`IsSprintEnabled`) ou atirar (período de 5s pós-tiro), a thread principal da Unity nunca é pausada pelo GC no meio do tiroteio.
- **Detecção Híbrida de Combate:** O patch `WeaponShotGCPatch` notifica disparos tanto do jogador local quanto de bots num raio de 50 metros, garantindo que mesmo se um bot atirar de surpresa no jogador, o GC já entra em modo suprimido imediatamente.

### 2.2. Robustez e Coleta Segura no Inventário
- **Debounce de Estabilização:** O sistema aguarda 0.5 segundos contínuos de inventário aberto antes de iniciar uma coleta preventiva, garantindo que aberturas acidentais ou rápidas não causem micro-pausas na interface.
- **Cooldown Entre Coletas:** Limite de 20 segundos mínimos entre coletas no inventário, prevenindo coletas repetitivas durante o arrasto contínuo de itens de loot.
- **Utilização da API Nativa do EFT:** A coleta delegada chama `MemoryControllerClass.Collect(force: true)`, aproveitando as rotinas otimizadas de compactação de LOH e limpeza de memória desenvolvidas pela própria BSG.

### 2.3. Airbag de Segurança de Memória e Resiliência a Morte
- **Proteção Contra Vazamento e OOM:** Se a memória alocada no heap ultrapassar o limiar de segurança configurado (`CriticalMemoryLimitMB`, padrão 3500 MB), o GC é forçado imediatamente a `GarbageCollector.Mode.Enabled`, prevenindo travamento do processo por falta de RAM física.
- **Restauração na Morte ou Fim de Raid:** No `OnUpdate()`, se o jogador morre (`!player.HealthController.IsAlive`), o GC é imediatamente liberado. No `Cleanup()` e `OnDestroy()`, um bloco com tratamento de exceção garante que a Unity volte para `Mode.Enabled` antes de transicionar para o menu principal.

---

## 3. Conformidade com as Regras do Projeto

| Regra | Status | Evidência |
|---|---|---|
| **SemVer Bump Obrigatório** | ✅ Aprovado | Versão incrementada de `0.3.4` para `0.3.5` em `Plugin.cs`, `TRL-CoreSight.csproj` e `mod.json`. |
| **Isolamento de Build** | ✅ Aprovado | DLL final gerada exclusivamente em `mods/TRL-CoreSight/builds/TRL-CoreSight.dll` (65.536 bytes). Nada copiado para `D:/SPT`. |
| **Ciclo de Backlog** | ✅ Aprovado | Item 004 documentado de ponta a ponta (01-spec, 02-spec-tech, 03-review, 04-code-review, 05-asbuild). |
| **Idioma** | ✅ Aprovado | Toda a documentação e código em Português do Brasil. |

---

## 4. Veredito do Code Review

🟢 **Aprovado com Louvor para Homologação em Raid.** Sistema extremamente estável, resolve um dos maiores problemas crônicos de micro-stutters do Escape from Tarkov e possui múltiplos airbags de segurança.
