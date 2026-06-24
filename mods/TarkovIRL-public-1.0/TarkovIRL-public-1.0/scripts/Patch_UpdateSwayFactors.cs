using UnityEngine;
using SPT.Reflection.Patching;
using System.Reflection;
using HarmonyLib;
using EFT;
using EFT.Animations;

namespace TarkovIRL
{
    internal class Patch_UpdateSwayFactors : ModulePatch
    {

        private static FieldInfo playerField;
        private static FieldInfo fcField;

        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(Player.FirearmController), "_player");
            fcField = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_firearmController");
            return typeof(ProceduralWeaponAnimation).GetMethod("UpdateSwayFactors", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void Postfix(ProceduralWeaponAnimation __instance)
        {

            if ((Object)(object)__instance == (Object)null)
            {
                return;
            }

            if (!PrimeMover.IsWeaponSway.Value)
            {
                return;
            }

            if (SwayController.IsSwayUpdatedThisFrame)
            {
                return;
            }

            Player.FirearmController firearmController = (Player.FirearmController)fcField.GetValue(__instance);
            if (firearmController == null)
            {
                return;
            }

            Player player = (Player)playerField.GetValue(firearmController);
            if (player != null && player.IsYourPlayer && player.MovementContext.CurrentState.Name != EPlayerState.Stationary)
            {
                Vector3 newSwayFactors = SwayController.GetNewSway(__instance.MotionReact.SwayFactors, __instance.IsAiming);
                __instance.MotionReact.SwayFactors = newSwayFactors;
                SwayController.IsSwayUpdatedThisFrame = true;
            }
            else
            {
                return;
            }
        }
    }
}