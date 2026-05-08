using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CameraRotationMod.Patches
{
    /// <summary>
    /// Postfix em PlayerPhysicalClass.GetHandsRestorationFunc — multiplica o resultado
    /// pela Intensity da stance ativa quando Mode = Recovery, fora de ADS, fora de
    /// prone-suspenso e somente para o MainPlayer.
    /// ref: Assembly-CSharp/PlayerPhysicalClass.cs:1022
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
                if (gw.MainPlayer is HideoutPlayer) return;          // hideout — feature inerte
                if (__instance.Player_0 != gw.MainPlayer) return;    // só MainPlayer (ignorar bots)

                if (StanceStaminaState.Mode != EStanceStaminaMode.Recovery) return;
                if (StanceStaminaState.IsSuspendedByProne) return;

                // Recovery não aplica em ADS — EFT já zera regen ali
                if (gw.MainPlayer.ProceduralWeaponAnimation?.IsAiming == true) return;

                float intensity = StanceStaminaState.Intensity;
                if (float.IsNaN(intensity) || float.IsInfinity(intensity)) return;
                __result *= intensity;
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[StanceStaminaRecoveryPatch] {ex}");
            }
        }
    }
}
