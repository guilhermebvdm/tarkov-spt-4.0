using UnityEngine;
using EFT.Animations;
using SPT.Reflection.Patching;
using System.Reflection;
using EFT;
using HarmonyLib;

namespace TarkovIRL
{
    public class Patch_SetHeadRotation : ModulePatch
    {
        static FieldInfo _playerField;
        static FieldInfo _fcField;

        static Vector3 _dzLerp = Vector3.zero;
        static Vector3 _dzLerpTarget = Vector3.zero;

        protected override MethodBase GetTargetMethod()
        {
            /*
            resetLookField = AccessTools.Field(typeof(Player), "_resetLook");
            mouseLookControlField = AccessTools.Field(typeof(Player), "_mouseLookControl");
            isResettingLookField = AccessTools.Field(typeof(Player), "_isResettingLook");
            setResetedLookNextFrameField = AccessTools.Field(typeof(Player), "_setResetedLookNextFrame");
            isLookingField = AccessTools.Field(typeof(Player), "_isLooking");
            horizontalField = AccessTools.Field(typeof(Player), "_horizontal");
            verticalField = AccessTools.Field(typeof(Player), "_vertical");
            */
            _playerField = AccessTools.Field(typeof(Player.FirearmController), "_player");
            _fcField = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_firearmController");
            return typeof(ProceduralWeaponAnimation).GetMethod("SetHeadRotation", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool Prefix(ProceduralWeaponAnimation __instance, Vector3 headRot)
        {
            if ((UnityEngine.Object)(object)__instance == (UnityEngine.Object)null)
            {
                return true;
            }

            Player.FirearmController firearmController = (Player.FirearmController)_fcField.GetValue(__instance);
            if ((UnityEngine.Object)(object)firearmController == (UnityEngine.Object)null)
            {
                _dzLerpTarget = headRot;
                return true;

                // ^^
                // This needs to return true in order for Patch_Look to be able to play,
                // which it must do because of the conflict with FOV fix mod.
                // Before, I had this false and deployed the throw code below. In that case,
                // the return must be commented out because indeed there is no firearm to be found,
                // but rather something else(?) that inherets from ItemHandsController.

                // Ultimately the problem remains of snapping between the throw and the
                // re-presentation of the weapon after (and before!), but at least the full throw animation
                // is allowed to play before the snap back -- and if the player is moving, the rupture is
                // anyway attenuated and hard to notice.
            }

            Player player = (Player)_playerField.GetValue(firearmController);
            if ((UnityEngine.Object)(object)player != (UnityEngine.Object)null && player.IsYourPlayer && player.MovementContext.CurrentState.Name != EPlayerState.Stationary)
            {
                Vector3 headRotModified = headRot;
                if (PrimeMover.IsWeaponDeadzone.Value)
                {
                    //headRotModified = DeadzoneController.GetHeadRotationWithDeadzone(player, PrimeMover.WeaponDeadzoneMulti.Value, headRot);
                    headRotModified = NewDeadzoneController.GetHeadRotWithDeadzone(headRotModified);
                }

                // ^^
                // for some reason I have to inject the config value here, if I try to do it in the deadzone controller 
                // it gets a wild error. Shrug.

                headRotModified.y *= 1.5f;
                player.HeadRotation = headRotModified;
                AccessTools.Field(typeof(ProceduralWeaponAnimation), "_headRotationVec").SetValue(__instance, headRotModified);

                return false;
            }
            return true;
        }
    }
}
