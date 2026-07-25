using HarmonyLib;
using EFT;
using UnityEngine;
using System.Reflection;
using System;

namespace TRLFixes.Patches
{
    public class PickupAimingSafetyPatch
    {
        public void Enable()
        {
            var harmony = new Harmony("com.trl.fixes.pickupaiming");

            try
            {
                var setAimingMethod = AccessTools.PropertySetter(typeof(Player.FirearmController), "IsAiming");
                var finalizerMethod = AccessTools.Method(typeof(PickupAimingSafetyPatch), nameof(Finalizer));

                if (setAimingMethod != null && finalizerMethod != null)
                {
                    harmony.Patch(setAimingMethod, finalizer: new HarmonyMethod(finalizerMethod));
                    Debug.Log("[TRL-Fixes] Safety patch em FirearmController.IsAiming aplicado com sucesso!");
                }
                else
                {
                    Debug.LogError("[TRL-Fixes] Não foi possível encontrar FirearmController.set_IsAiming!");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TRL-Fixes] Falha ao aplicar PickupAimingSafetyPatch: {ex}");
            }
        }

        public static Exception Finalizer(Exception __exception)
        {
            if (__exception is NullReferenceException)
            {
                Debug.LogWarning($"[TRL-Fixes] Preveniu trava do boneco em FirearmController.IsAiming (pickup/equip race condition): {__exception.Message}");
                return null; // Engole a NRE para permitir que o MovementContext conclua a transição de estado sem travar o jogador
            }
            return __exception;
        }
    }
}
