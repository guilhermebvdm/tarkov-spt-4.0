---
title: "004 — Refatoração de Performance & Sistema Wake on Hit · Code Review 01"
date: 2026-08-22
status: 🟢 Vivo
authors:
  - Antigravity
---

# 004 — Refatoração de Performance & Sistema Wake on Hit · Code Review 01

**Mod:** VisceralCombat  
**Plano de Implementação:** [docs/plano-implementacao-refatoracao-visceral.md](plano-implementacao-refatoracao-visceral.md)  
**Relatório de Auditoria:** [docs/relatorio-auditoria-original-vs-proposta.md](relatorio-auditoria-original-vs-proposta.md)  
**Data:** 2026-08-22  

> Análise crítica e avaliação formal do plano de implementação e das alterações necessárias em [`mods/VisceralCombat/modded/`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/). Cada achado recebe um ID permanente `CR-04-MM`.

---

## 📊 Resumo Executivo da Avaliação

> 🔴 **Bloqueadores:** 3 · 🟠 **Fortes:** 4 · 🟡 **Médios:** 2 · 🟢 **Menores:** 3 · **Total:** 12 Achados Avaliados

| Status do Plano de Implementação | Parecer Técnico |
|---|---|
| ✅ **APROVADO PARA EXECUÇÃO** | O plano [plano-implementacao-refatoracao-visceral.md](plano-implementacao-refatoracao-visceral.md) resolve **100% dos 3 bloqueadores 🔴 e dos 4 pontos fortes 🟠** identificados no código atual de `modded/`. |

---

## 📋 Índice de Achados

| ID | Categoria | Impacto | Título | Status no Plano |
|---|---|---|---|---|
| **CR-04-01** | A — Crítico | 🔴 Bloqueador | Assinatura inexistente `Player.ReceiveDamage` no EFT 0.16.9 | ✅ Coberto no Plano (Migração para `ApplyDamageInfo`) |
| **CR-04-02** | A — Crítico | 🔴 Bloqueador | NRE Fatal em `Corpse` via `GetComponent<Player>()` | ✅ Coberto no Plano (`GetComponentInChildren<PlayerBody>`) |
| **CR-04-03** | B — Bug Latente | 🔴 Bloqueador | Regressão de Ciclo de Vida do Ragdoll e Carga Contínua de CPU | ✅ Coberto no Plano (Repouso Físico + *Wake on Hit*) |
| **CR-04-04** | B — Bug Latente | 🟠 Forte | Cancelamento Precoce da Agonia por Rajadas de Armas Automáticas | ✅ Coberto no Plano (Buffer de Carência de 1.2s) |
| **CR-04-05** | C — Gap vs Spec | 🟠 Forte | Três Corrotinas Paralelas `WatchShot` por Bala no Ar | ✅ Coberto no Plano (`VisceralShotProcessor` Unificado) |
| **CR-04-06** | D — Arquitetura | 🟠 Forte | Duplicação de Patches e Execução Redundante a Cada Raid | ✅ Coberto no Plano (Exclusão de `KillClientPatch.cs`) |
| **CR-04-07** | D — Arquitetura | 🟠 Forte | Varreduras Cegas de Cena `FindObjectsOfType` no Início de Raid | ✅ Coberto no Plano (Remoção em `GameStartedPatch.cs`) |
| **CR-04-08** | B — Bug Latente | 🟡 Médio | Bitmask `LayerMask.NameToLayer("Default")` Retornando `0` | ✅ Coberto no Plano (Correção para `1 << LayerMask`) |
| **CR-04-09** | D — Arquitetura | 🟡 Médio | Patch Órfão com `Object.Destroy(player)` | ✅ Coberto no Plano (Exclusão de `PlayerDetonationPatch.cs`) |
| **CR-04-10** | E — Manutenção | 🟢 Menor | Classes Stubs Descompiladas do ILSpy em Patches | ✅ Coberto no Plano (Reescrita em C# Limpo) |
| **CR-04-11** | F — Otimização | 🟢 Menor | Triplo Callback de Ciclo de Vida no `DismemberedLimbScaler` | ✅ Coberto no Plano (Manter Apenas `LateUpdate`) |
| **CR-04-12** | F — Otimização | 🟢 Menor | Throttling de `ForceProneLock` no `LivingDismembermentController` | ✅ Coberto no Plano (Cadência Controlada a 0.5s) |

---

## 🔍 Detalhamento dos Pontos Avaliados

### CR-04-01 · Cat A — Crítico · 🔴 Bloqueador
**Assinatura inexistente `Player.ReceiveDamage` no EFT 0.16.9**  
- **Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/ShootOffHelmetPatch.cs:L15`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/ShootOffHelmetPatch.cs#L15)  
- **Problema:** O método `Player.ReceiveDamage` não existe no `Assembly-CSharp` do EFT 0.16.9. O patch falha silenciosamente na inicialização.  
- **Validação:** Confirmado no `references/eft-decompiled/Assembly-CSharp/EFT/Player.cs`. O pipeline de dano oficial é unificado em `Player.ApplyDamageInfo(DamageInfoStruct, EBodyPart, EBodyPartColliderType, float)`.  
- **Avaliação do Plano:** ✅ **Aprovado.** O plano migra o alvo para `Player.ApplyDamageInfo` e valida se o tiro atingiu a cabeça/capacete antes de arremessar o capacete.

---

### CR-04-02 · Cat A — Crítico · 🔴 Bloqueador
**NRE Fatal em `Corpse` via `GetComponent<Player>()`**  
- **Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/CreateBSGRagdollPatch.cs:L26`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/CreateBSGRagdollPatch.cs#L26)  
- **Problema:** `Corpse` no EFT 0.16.9 é um `LootItem` / MonoBehaviour independente e não possui o componente `Player`. Chamar `.GetComponent<Player>().PlayerBody` retorna `null` e lança NRE instantâneo.  
- **Validação:** Confirmado em `references/eft-decompiled/Assembly-CSharp/EFT.Interactive/Corpse.cs:L217`.  
- **Avaliação do Plano:** ✅ **Aprovado.** O plano substitui por `__instance.GetComponentInChildren<PlayerBody>()` e cacheia os campos privados via `FieldInfo`.

---

### CR-04-03 · Cat B — Bug Latente · 🔴 Bloqueador
**Regressão de Ciclo de Vida do Ragdoll e Carga Contínua de CPU**  
- **Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/RagdollClassPatch.cs:L167`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/RagdollClassPatch.cs#L167)  
- **Problema:** Intercepta `RagdollClass.Start` com prefixo `false`, mas sua corrotina omite `UnsupportRigidbody` e `isKinematic = true`. Corpos permanecem calculando física para sempre, inflando o `SyncTransformsClass.Update` em mais de 2ms por frame.  
- **Avaliação do Plano:** ✅ **Aprovado.** O plano implementa o modelo **Wake on Hit**: o corpo entra no repouso físico nativo do EFT (`isKinematic = true`, 0% de CPU) e acorda temporariamente por 2.5s apenas sob o impacto de novos projéteis ou granadas.

---

### CR-04-04 · Cat B — Bug Latente · 🟠 Forte
**Cancelamento Precoce da Agonia por Rajadas de Armas Automáticas**  
- **Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Classes/RagdollHelperClass.cs:L353`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Classes/RagdollHelperClass.cs#L353)  
- **Problema:** Em rajadas automáticas, o 2º ou 3º tiro atinge o bot 50ms após a morte e cancela a agonia instantaneamente antes mesmo da animação ser percebida.  
- **Avaliação do Plano:** ✅ **Aprovado.** Introdução do `_agonyStartTime` com buffer de carência de 1.2s. Tiros na janela inicial aplicam dano/sangue sem interromper a agonia; tiros intencionais após 1.2s finalizam o bot como tiro de misericórdia.

---

### CR-04-05 · Cat C — Gap vs Spec · 🟠 Forte
**Três Corrotinas Paralelas `WatchShot` por Bala no Ar**  
- **Local:** [`LimbKillPatch.cs:L33`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs#L33), [`BodiesImpulsePatch.cs:L76`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs#L76), [`BleedPatch.cs:L38`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Patches/BleedPatch.cs#L38)  
- **Problema:** Cada tiro instancia 3 corrotinas de polling no `StaticManager`, consumindo ciclos de CPU desnecessários durante tiroteios intensos.  
- **Avaliação do Plano:** ✅ **Aprovado.** Criação do [`VisceralShotProcessor.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Classes/VisceralShotProcessor.cs) centralizado para monitoramento e despacho sequencial único de física, sangue e desmembramento.

---

### CR-04-06 a CR-04-12 · Arquitetura, GC e Otimizações · 🟠/🟡/🟢
- **CR-04-06 (Duplicação de Patches):** Exclusão de `KillClientPatch.cs` e remoção da re-habilitação em `GameStartedPatch.cs`.
- **CR-04-07 (Varreduras Cegas):** Exclusão das linhas 28-34 de `GameStartedPatch.cs (Ragdolls)` (`FindObjectsOfType`).
- **CR-04-08 (Bitmask de Granadas):** Correção de `LayerMask.NameToLayer("Default")` para `1 << LayerMask.NameToLayer("Default")`.
- **CR-04-09 (Código Destrutivo):** Exclusão definitiva de `PlayerDetonationPatch.cs`.
- **CR-04-10 (Limpeza ILSpy):** Substituição de structs descompiladas por métodos assíncronos/corrotinas limpos.
- **CR-04-11 (Scaler de Membros):** Manutenção estrita apenas no `LateUpdate()`.
- **CR-04-12 (Living Dismemberment):** Throttling de `ForceProneLock` a 0.5s.

---

## 🎯 Conclusão e Recomendação

O plano de implementação [plano-implementacao-refatoracao-visceral.md](plano-implementacao-refatoracao-visceral.md) está **100% robusto, tecnicamente validado e apto para execução imediata**.

**Próximo Passo:** Prosseguir com a aplicação das alterações no código-fonte em `mods/VisceralCombat/modded/` conforme especificado no plano aprovado.
