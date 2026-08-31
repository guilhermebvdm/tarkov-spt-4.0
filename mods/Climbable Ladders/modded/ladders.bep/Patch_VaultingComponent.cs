using EFT;
using EFT.Vaulting;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace tarkin.ladders.bep
{
#if DEBUG
    internal class Patch_VaultingComponent : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(VaultingComponent), nameof(VaultingComponent.method_4));
        }

        [PatchPostfix]
        private static void PatchPostfix(VaultingComponent __instance, bool __result)
        {
            if (!__result)
            {
                NotificationManagerClass.DisplayWarningNotification("vault denied early");
                Plugin.Logger.LogWarning($"vault denied early");
                Plugin.Logger.LogInfo($"IvaultingSettings_0: {__instance.IvaultingSettings_0.IsActive}");
                Plugin.Logger.LogInfo($"Already vaulting: {__instance.IsVaulting()}");
                Plugin.Logger.LogInfo($"input movement dir: {__instance.Ginterface283_0.MovementDirection.y}");
                Plugin.Logger.LogInfo($"is grounded: {__instance.Ginterface283_0.IsGrounded}");
            }
        }
    }
#endif


    internal class Patch_ObstacleCalculatorModel_DistanceToMainObstacle : ModulePatch
    {
        public static bool OverrideVaultObstacleDistance;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(ObstacleCalculatorModel), nameof(ObstacleCalculatorModel.DistanceToMainObstacle));
        }

        [PatchPostfix]
        private static void PatchPostfix(ObstacleCalculatorModel __instance, ref float __result)
        {
            if (!OverrideVaultObstacleDistance)
                return;

            // default min valid distance is 0.5
            __result = Mathf.Min(__result, 0.499f);
        }
    }
}
