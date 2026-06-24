using UnityEngine;
using EFT.Animations;
using SPT.Reflection.Patching;
using System.Reflection;
using EFT;
using HarmonyLib;

namespace TarkovIRL
{
    public class Patch_SetHeadRotation_ApplyDeadzone : ModulePatch
    {
        static FieldInfo _playerField;
        static FieldInfo _fcField;

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(Player.FirearmController), "_player");
            _fcField = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_firearmController");
            return typeof(ProceduralWeaponAnimation).GetMethod("SetHeadRotation", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool Prefix(ProceduralWeaponAnimation __instance, Vector3 headRot)
        {
            if (!PrimeMover.IsWeaponDeadzone.Value)
            {
                return true;
            }

            if ((UnityEngine.Object)(object)__instance == (UnityEngine.Object)null)
            {
                return true;
            }

            Player.FirearmController firearmController = (Player.FirearmController)_fcField.GetValue(__instance);
            if ((UnityEngine.Object)(object)firearmController == (UnityEngine.Object)null)
            {
                return true;
            }

            Player player = (Player)_playerField.GetValue(firearmController);
            if ((UnityEngine.Object)(object)player != (UnityEngine.Object)null && player.IsYourPlayer && player.MovementContext.CurrentState.Name != EPlayerState.Stationary)
            {
                Vector3 headRotThisFrame = DeadzoneController.GetHeadRotationWithDeadzone(player);
                AccessTools.Field(typeof(ProceduralWeaponAnimation), "_headRotationVec").SetValue(__instance, headRotThisFrame);

                return false;
            }
            return true;
        }
    }
}
