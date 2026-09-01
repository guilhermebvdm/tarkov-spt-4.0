---
title: "TRL-Fixes — Handoff Técnico: Correção de Trava de Entrada (Pickup Aiming Safety)"
date: 2026-08-01
status: 🟢 Vivo
authors: Antigravity
---

# 📋 Handoff Técnico — Correção de Trava de Entrada (Pickup/Equip NRE Guard)

**Data:** 25 de Julho de 2026  
**Módulo Alvo:** `TRL-Fixes` (ver histórico abaixo)  
**Autor:** Antigravity AI Pair Programmer  

---

## 📌 Histórico de dono (2026-08-01)

Este documento foi escrito endereçado ao `TRL-ImmersiveCombatMedicine`, **que nunca recebeu o patch**. O código
foi implementado no `TRL-Fixes`, movido para `stancesAndCameraPositionSPT4.0.11` no commit `19aa6499` sem
registro em changelog ou memória de nenhum dos três mods, e **devolvido ao `TRL-Fixes` em 2026-08-01**.

Motivo da devolução: é um remendo sobre bug do **jogo base**, sem relação com posturas/mobilidade. Verificado
que o mod de stances não participa do caminho do erro — nenhum mod TRL escreve `FirearmController.IsAiming`, e
os enxertos de stances no fluxo de mira ficam no pipeline de animação (`ProceduralWeaponAnimation`), que roda
depois e em outro objeto. Como o mod de stances está sendo preparado para publicação pública, um patch que
engole exceções alheias ao seu tema não pode viajar junto.

## 🔍 Estado da causa raiz

O mecanismo descrito abaixo é **coerente com o decompilado** — `method_63`/`method_64` (`Player.cs:14569-14588`)
acessam `FirearmsAnimator` sem checagem de nulo, e o campo é preenchido só no equip da arma — mas **nunca foi
confirmado com uma captura real em raid**. Por isso a implementação atual registra a **primeira ocorrência com
a pilha de chamadas completa**. Se essa pilha apontar origem diferente, o remendo está mascarando outra coisa.

---

## 🎯 Contexto e Diagnóstico do Bug

### Sintoma em Raid:
Ao saquear um item do chão (especialmente Rigs / Coletes Táticos / Armaduras) usando o menu de ação nativo **"Equipar" / "Pickup"**, o corpo do personagem fica **completamente congelado/preso** (não anda, não se agacha, não vira a visão), porém a interface (menu TAB) e as teclas de troca de arma continuam funcionando.

### Causa Raiz Técnico:
1. Ao acionar o `Pickup` nativo do EFT (`GetActionsClass.smethod_10` -> `IdleStateClass.Pickup`), o jogo inicia a transição da máquina de estados do movimento do jogador (`MovementContext.OverrideState`).
2. Para iniciar a animação de abaixar/pegar, o jogo desativa temporariamente a mira (`OnAimingDisabled` -> `SetAiming(false)` -> `set_IsAiming(false)`).
3. No setter `set_IsAiming(false)`, o EFT chama o método interno `method_64()`.
4. **Race Condition**: No exato mesmo frame em que o `method_64()` tenta ler o estado visual das armas/lunetas/animador, o `InventoryController` está reconstruindo a hierarquia de slots do equipamento equipado. O `method_64()` lê uma referência nula temporária e dispara `NullReferenceException`.
5. **Travamento de Input**: Como a exceção não é tratada dentro de `set_IsAiming`, a transição do `MovementContext` é **abortada no meio**. O `MovementContext` fica permanentemente no estado de *Input Lock* (bloqueio de controles).

---

## 🛠️ Solução Recomendada para o `TRL-ImmersiveCombatMedicine`

Implementar um `ModulePatch` com um `[PatchFinalizer]` em `Player.FirearmController.set_IsAiming` (ou `IsAiming` setter).

### Implementação Sugerida (C# / Harmony):

```csharp
using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>
    /// Previne a trava do MovementContext quando ocorre exceção de nulo durante a desativação de mira
    /// ao equipar/pegar itens do chão (Pickup race condition).
    /// </summary>
    public class PickupAimingSafetyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.PropertySetter(typeof(Player.FirearmController), "IsAiming");

        [PatchFinalizer]
        private static Exception Finalizer(Exception __exception)
        {
            if (__exception is NullReferenceException)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning(
                    $"[ICM-Safety] Preveniu trava do MovementContext no FirearmController.IsAiming (pickup/equip race condition): {__exception.Message}");
                
                // Retorna null para engolir a NRE e permitir que o MovementContext conclua a transição de estado sem travar o jogador
                return null;
            }
            return __exception;
        }
    }
}
```

### Onde Registrar:
Adicionar no método de inicialização do plugin em `TRLImmersiveCombatMedicinePlugin.cs`:

```csharp
new PickupAimingSafetyPatch().Enable();
```

---

## ✅ Comportamento Esperado Após Aplicação
- Se ocorrer a colisão de frames entre a troca de inventário e a desativação de mira, o `[PatchFinalizer]` intercepta o `NullReferenceException` e retorna `null`.
- A transição do `MovementContext` conclui normalmente.
- O personagem **nunca mais trava os controles**, permitindo usar a função nativa de equipar do chão sem stutters nem travamentos.
