---
title: "Relatório de Auditoria Técnica de Código — FIKA Modded (Partição 02: Replicação de Jogadores & Movimento)"
date: 2026-09-02
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — FIKA Modded (Partição 02: Replicação de Jogadores & Movimento)

## 1. Panorama da Partição & Diagnóstico da 2ª Rodada

Esta auditoria técnica da **2ª Rodada** reavalia os componentes em [`mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Players/`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Players/) e [`ObservedClasses/`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/).

### Quadro Comparativo de Diagnóstico

| Severidade | 1ª Rodada (Original) | Pós-Correção (Fase B) | 2ª Rodada (Novos Achados de Refino) |
| :--- | :---: | :---: | :---: |
| 🔴 **Crítico (Memory Leaks / Crashes)** | 2 | 0 | **0** |
| 🟠 **Alto (Desync / Multi-Rail Crash)** | 1 | 0 | **0** |
| 🟡 **Médio (GC Pressure / Alocações em Tiro)** | 3 | 0 | **1** |
| 🔵 **Baixo (SemVer / C# 13 preview)** | 1 | 0 | **0** |
| 💡 **Sugestão de Otimização** | 1 | 0 | **1** |

---

## 2. Novos Achados Identificados na 2ª Rodada

### `AUD-MOD-02-01` — Alocação Excessiva de GC em Disparos Replicados
- **Arquivo:** [`ObservedFirearmController.cs:L549, L609`](../../modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/HandsControllers/ObservedFirearmController.cs#L549)
- **Severidade:** 🟡 Médio (GC Churn / Performance)
- **Descrição:** Em `HandleRevolverShot` e `HandleShot`, cada bala disparada por um jogador remoto invoca `(AmmoItemClass)Singleton<ItemFactoryClass>.Instance.CreateItem(MongoID.Generate(), packet.AmmoTemplate, null)`. Em combates intensos com rajadas contínuas de submetralhadoras ou rifles automáticos, isso aloca dezenas de instâncias completas de Item no heap do Tarkov por segundo.
- **Correção Proposta:** Reusar instâncias temporárias de projétil ou validar cache de templates para efeitos visuais/sonoros de disparo.

### `AUD-MOD-02-02` — Micro-alocação em `UpdatePose`
- **Arquivo:** [`ObservedPlayer.cs:L870-890`](../../modded/Fika-Plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L870-L890)
- **Severidade:** 💡 Sugestão de Otimização
- **Descrição:** Chamadas de transição de pose no `ObservedPlayer` realizam checagens com conversões desnecessárias de structs a cada frame.

---

## 3. Status dos Achados da 1ª Rodada

- `AUD-02-01` (Desinscrição de `_armorUnsubcribes` em `FikaPlayer.OnDestroy`): ✅ **Resolvido**
- `AUD-02-02` (Liberação de `VoipEftSource.Release` em `ObservedPlayer.OnDestroy`): ✅ **Resolvido**
- `AUD-02-03` (Guardas `deltaTime > 0.0001f` contra `NaN` em `ObservedMovementContext`): ✅ **Resolvido**
- `AUD-02-04` (Guarda Singleton em `ObservedPlayer.OnDestroy`): ✅ **Resolvido**
- `TRL-Fixes #4` (Solução multi-trilho `RefreshSlotViews`): ✅ **Integrado**
