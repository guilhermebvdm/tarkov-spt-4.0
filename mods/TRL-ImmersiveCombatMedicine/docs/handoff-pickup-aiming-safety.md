# 📋 Handoff Técnico — Correção de Trava de Entrada (Pickup/Equip NRE Guard)

**Data:** 25 de Julho de 2026  
**Módulo Alvo:** `TRL-ImmersiveCombatMedicine`  
**Autor:** Antigravity AI Pair Programmer  

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
