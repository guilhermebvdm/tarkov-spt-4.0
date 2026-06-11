using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CameraRotationMod.Patches
{
    /// <summary>
    /// Postfix em PlayerPhysicalClass.GetHandsRestorationFunc — quando StaminaMultiplier ≠ 1.0
    /// (mod ativo), zera o resultado vanilla para o tick do StanceManager controlar o delta.
    /// Preserva comportamento vanilla em ADS, prone-suspenso e bots (Player_0 check).
    /// ref: Assembly-CSharp/PlayerPhysicalClass.cs:1022
    /// ref: fix-01 — substitui Mode/Intensity pelo StaminaMultiplier unificado.
    /// </summary>
    public class StanceStaminaRecoveryPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(PlayerPhysicalClass),
                nameof(PlayerPhysicalClass.GetHandsRestorationFunc));
        }

        [PatchPostfix]
        [HarmonyPriority(Priority.Low)] // rodar depois de outros mods de stamina
        private static void Postfix(PlayerPhysicalClass __instance, ref float __result)
        {
            try
            {
                var gw = Singleton<GameWorld>.Instance;
                if (gw?.MainPlayer == null) return;
                if (gw.MainPlayer is HideoutPlayer && Plugin._DebugApplyInHideout?.Value != true) return;
                if (__instance.Player_0 != gw.MainPlayer) return;    // só MainPlayer (ignorar bots)

                // Quando Multiplier ≠ 1.0 o tick controla o delta de stamina diretamente.
                // Zeramos o vanilla aqui para evitar que ele interfira (soma indesejada).
                // Em ADS e prone-suspenso preservamos o comportamento vanilla do EFT.
                // ref: fix-01 — StaminaMode+Intensity unificados em StaminaMultiplier.
                float mult = StanceStaminaState.Multiplier;
                if (System.Math.Abs(mult - 1.0f) <= 1e-5f) return; // vanilla — não interferir

                if (MountingManager.IsMounting || gw.MainPlayer.ProceduralWeaponAnimation?.IsMountedState == true)
                {
                    __result = 5f; // Recupera 5 de estamina do braço por segundo quando montado
                    return;
                }

                if (!StanceStaminaState.IsSuspendedByProne &&
                    gw.MainPlayer.ProceduralWeaponAnimation?.IsAiming != true)
                    __result = 0f;
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[StanceStaminaRecoveryPatch] {ex}");
            }
        }
    }

    public class HandsStaminaConsumePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass774), nameof(GClass774.Consume));
        }

        [PatchPrefix]
        private static bool Prefix(GClass774 __instance, ref float __result)
        {
            try
            {
                var gw = Singleton<GameWorld>.Instance;
                if (gw?.MainPlayer?.Physical?.HandsStamina == __instance)
                {
                    if (gw.MainPlayer.IsInPronePose)
                    {
                        __result = 0f;
                        return false; // Ignora o método original
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[HandsStaminaConsumePatch] {ex}");
            }
            return true;
        }
    }

    public class HandsStaminaProcessPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass774), nameof(GClass774.Process));
        }

        [PatchPrefix]
        private static void Prefix(GClass774 __instance, ref bool __state)
        {
            try
            {
                var gw = Singleton<GameWorld>.Instance;
                if (gw?.MainPlayer?.Physical?.HandsStamina == __instance && gw.MainPlayer.IsInPronePose)
                {
                    __state = __instance.ForceMode;
                    __instance.ForceMode = true;
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[HandsStaminaProcessPatch Prefix] {ex}");
            }
        }

        [PatchPostfix]
        private static void Postfix(GClass774 __instance, bool __state)
        {
            try
            {
                var gw = Singleton<GameWorld>.Instance;
                if (gw?.MainPlayer?.Physical?.HandsStamina == __instance && gw.MainPlayer.IsInPronePose)
                {
                    __instance.ForceMode = __state;
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[HandsStaminaProcessPatch Postfix] {ex}");
            }
        }
    }
}
