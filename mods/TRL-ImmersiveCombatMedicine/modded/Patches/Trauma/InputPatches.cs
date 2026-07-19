using HarmonyLib;
using EFT;
using EFT.InputSystem;
using UnityEngine;
using TRLImmersiveCombatMedicine;

namespace TrueTrauma
{
    [HarmonyPatch(typeof(EFT.GamePlayerOwner), "TranslateAxes")]
    class FreezeAxesPatch
    {
        static void Prefix(EFT.GamePlayerOwner __instance, ref float[] axes)
        {
            if (!TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value || !TRLImmersiveCombatMedicinePlugin.ConfigBlackoutEnabled.Value) return;

            if (__instance.Player != null && TraumaState.BlackoutTimers.ContainsKey(__instance.Player.ProfileId))
                if (axes != null) for (int i = 0; i < axes.Length; i++) axes[i] = 0f;
        }
    }

    [HarmonyPatch(typeof(EFT.GamePlayerOwner), "TranslateCommand")]
    class FreezeCommandPatch
    {
        static bool Prefix(EFT.GamePlayerOwner __instance, ECommand command, ref InputNode.ETranslateResult __result)
        {
            if (!TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value || !TRLImmersiveCombatMedicinePlugin.ConfigBlackoutEnabled.Value) return true;

            if (__instance.Player != null && TraumaState.BlackoutTimers.ContainsKey(__instance.Player.ProfileId))
            {
                if (command == ECommand.Escape) return true;
                __result = InputNode.ETranslateResult.BlockAll;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MovementContext), "CanStandAt")]
    class CantStandUpPatch
    {
        static bool Prefix(MovementContext __instance, ref bool __result)
        {
            if (!TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value) return true;
            if (TraumaState.PlayerField == null) return true;

            try
            {
                Player player = TraumaState.PlayerField.GetValue(__instance) as Player;

                if (player != null && player.HealthController.IsAlive)
                {
                    string id = player.ProfileId;

                    if (TRLImmersiveCombatMedicinePlugin.ConfigBlackoutEnabled.Value && TraumaState.BlackoutTimers.ContainsKey(id))
                    {
                        __result = false;
                        return false;
                    }

                    // ref: spec 003 §4 (D10) — branches legados de pernas removidos (ImpactTimers 1 s;
                    // bloqueio de levantar 10 s humano / 90 s bot com 2 pernas zeradas): levantar não é mais
                    // travado pelo sistema legado — o ciclo de queda real chega no item 004 via Trauma 2.0.
                }
            }
            catch { }
            return true;
        }
    }
}
