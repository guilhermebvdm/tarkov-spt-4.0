# 004 — gerenciador-gc-anti-stutter · Review Técnica 02

**Mod:** TRL-CoreSight  
**Data:** 2026-09-16T02:10:00Z  
**Origem:** Teste empírico em raid real (Relato de "Super-Stutter" de 4–6s ao abrir o inventário)

> Análise crítica e reformulação arquitetural do gerenciador de GC. Identificadores de pontos: `PA-02-MM`.

---

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 2 · 🟢 Menores: 1 · ✅ Resolvidos: 3 · Total: 3

---

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-02-01 | C — Erro de Premissa | 🟡 Importante | Efeito adverso e travamento de 4–6s causado por `MemoryControllerClass.Collect(force: true)` | ✅ Resolvido em 2026-09-16 |
| PA-02-02 | B — Design | 🟡 Importante | Redefinição para filtro inibidor estrito de sobrevivência (ADS + Combate recente) | ✅ Resolvido em 2026-09-16 |
| PA-02-03 | A — Limpeza | 🟢 Menor | Remoção de configurações de coleta forçada no F12 (`ForceCollectOnInventory`, `SafeIntervalSeconds`) | ✅ Resolvido em 2026-09-16 |

---

## Pontos e Resoluções

### PA-02-01 · C — Erro de Premissa · 🟡 Importante

**Efeito adverso e travamento de 4–6s causado por `MemoryControllerClass.Collect(force: true)`**

**Problema:** A premissa original assumia que chamar `MemoryControllerClass.Collect(force: true)` ao abrir o inventário seria uma faxina limpa e imperceptível. Em testes em raid real, essa chamada forçou descarregamento síncrono massivo de unmanaged assets e varredura total de heap, provocando congelamento severo da engine de 4 a 6 segundos e distorção com estalos no subsistema de áudio (underrun de buffer DSP).

**Resolução:** Eliminação absoluta de qualquer invocação a `MemoryControllerClass.Collect` e `GC.Collect`. O mod nunca mais forçará coletas síncronas de lixo.

---

### PA-02-02 · B — Design · 🟡 Importante

**Redefinição para filtro inibidor estrito de sobrevivência (ADS + Combate recente)**

**Problema:** O gerenciamento contínuo de janelas de coleta pelo mod gerava complexidade desnecessária e atrito com o motor.

**Resolução:** O mod passa a agir estritamente como um **filtro inibidor de momentos críticos**:
- **Sobrevivência:** Se o jogador estiver com ADS acionada (`IsAiming == true`) OU tiver disparado armas recentemente (`Time.time - _lastShotTime < CombatGracePeriod`), o GC da Unity é suspenso (`GarbageCollector.GCMode = Mode.Disabled`).
- **Nativo:** Fora de combate e fora de ADS, o GC é mantido em `Mode.Enabled`, permitindo que a Unity e o Tarkov gerenciem a limpeza de memória de forma incremental e natural.
- **Fallback de Emergência:** Se o heap atingir o limite de segurança `CriticalMemoryLimitMB`, o GC é forçado para `Mode.Enabled` mesmo durante combate/mira para evitar crash por falta de memória (OOM).

---

### PA-02-03 · A — Limpeza · 🟢 Menor

**Remoção de configurações de coleta forçada no F12 (`ForceCollectOnInventory`, `SafeIntervalSeconds`)**

**Problema:** As opções `ForceCollectOnInventory` e `SafeIntervalSeconds` tornaram-se obsoletas e induzem o usuário ao erro.

**Resolução:** Propriedades removidas de `ModConfig.cs` e `PROPRIEDADES.md`, mantendo apenas os controles essenciais do filtro (`EnableGCOptimizer`, `CombatGracePeriod`, `CriticalMemoryLimitMB`).

---

## Conclusão

Review técnica 02 aprovada para implementação imediata no código-fonte.
