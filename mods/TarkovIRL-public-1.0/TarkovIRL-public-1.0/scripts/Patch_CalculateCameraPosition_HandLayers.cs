using SPT.Reflection.Patching;
using EFT;
using EFT.Animations;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TarkovIRL
{
    internal class Patch_CalculateCameraPosition_HandLayers : ModulePatch
    {
        private static FieldInfo playerField;
        private static FieldInfo fcField;

        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(Player.FirearmController), "_player");
            fcField = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_firearmController");
            return typeof(ProceduralWeaponAnimation).GetMethod("CalculateCameraPosition", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(ProceduralWeaponAnimation __instance)
        {
            if ((UnityEngine.Object)(object)__instance == (UnityEngine.Object)null)
            {
                return;
            }


            Player.FirearmController firearmController = (Player.FirearmController)fcField.GetValue(__instance);
            if ((UnityEngine.Object)(object)firearmController == (UnityEngine.Object)null)
            {
                return;
            }
            Player player = (Player)playerField.GetValue(firearmController);
            if ((UnityEngine.Object)(object)player != (UnityEngine.Object)null && player.IsYourPlayer)
            {
                WeaponController.IsUsingMounted = __instance.IsMountedState;

                if (AnimStateController.IsBlindfire)
                {
                    return;
                }

                if (WeaponController.IsUsingMounted)
                {
                    return;
                }

                EfficiencyController.UpdateEfficiency(player);

                Vector3 breathOffset = HandBreathController.GetModifiedHandPosForBreath(player);
                Vector3 armStamOffset = HandShakeController.GetHandsShakePosition(player);
                Vector3 poseOffset = HandPoseController.GetModifiedHandPosWithPose(player);
                Quaternion poseOffsetRot = HandPoseController.GetModifiedHandRotWithPoseChange();
                Vector3 changePoseOffset = HandPoseController.GetModifiedHandPosWithPoseChange(player);
                Vector3 movementZOffsets = HandMovWithRotController.GetModifiedHandPosZMovement(player);
                Vector3 unstockedOffset = HandMovWithRotController.GetModifiedHandPosForLoweredWeapon(player);
                Vector3 footstepOffset = FootstepController.GetModifiedHandPosFootstep;
                Vector3 parallaxPosition = __instance.HandsContainer.WeaponRoot.localPosition;
                Quaternion parallaxRotation = __instance.HandsContainer.WeaponRoot.localRotation;
                ParallaxController.GetModifiedHandPosRotParallax(player, ref parallaxPosition, ref parallaxRotation);
                Vector3 addedSwayPosition = NewSwayController.GetNewSwayPosition();
                Quaternion addedSwayRotation = NewSwayController.GetNewSwayRotation();
                Vector3 sideToSidePosition = FootstepController.GetSideToSidePosition();
                Quaternion sideToSideRotation = FootstepController.GetSideToSideRotation();

                bool isBreathPos = PrimeMover.IsBreathingEffect.Value;
                bool isPosePos = PrimeMover.IsPoseEffect.Value;
                bool isPoseChangePos = PrimeMover.IsPoseChangeEffect.Value;
                bool isArmShake = PrimeMover.IsArmShakeEffect.Value;
                bool isSmallEffects = PrimeMover.IsSmallMovementsEffect.Value;
                bool isFootstep = PrimeMover.IsFootstepEffect.Value;
                bool isParallax = PrimeMover.IsParallaxEffect.Value;
                bool isNewSway = PrimeMover.IsWeaponSway.Value;

                if (isBreathPos) __instance.HandsContainer.WeaponRoot.localPosition += breathOffset;
                if (isPosePos)
                {
                    __instance.HandsContainer.WeaponRoot.localPosition += poseOffset;
                }
                if (isPoseChangePos)
                {
                    __instance.HandsContainer.WeaponRoot.localPosition += changePoseOffset;
                    __instance.HandsContainer.WeaponRoot.localRotation *= poseOffsetRot;
                }
                if (isArmShake) __instance.HandsContainer.WeaponRoot.localPosition += armStamOffset;
                if (isSmallEffects)
                {
                    __instance.HandsContainer.WeaponRoot.localPosition += movementZOffsets;
                    __instance.HandsContainer.WeaponRoot.localPosition += unstockedOffset;
                }
                if (isParallax)
                {
                    __instance.HandsContainer.WeaponRoot.localPosition += parallaxPosition;
                    __instance.HandsContainer.WeaponRoot.localRotation *= parallaxRotation;

                }
                if (isFootstep)
                {   
                    __instance.HandsContainer.WeaponRoot.localPosition += footstepOffset;
                    __instance.HandsContainer.WeaponRoot.localPosition += sideToSidePosition;
                    __instance.HandsContainer.WeaponRoot.localRotation *= sideToSideRotation;
                }
                if (isNewSway)
                {
                    __instance.HandsContainer.WeaponRoot.localPosition += addedSwayPosition;
                    __instance.HandsContainer.WeaponRoot.localRotation *= addedSwayRotation;
                }

                DirectionalSwayController.GetDirectionalSway(out Vector3 directionalSwayPos, out Quaternion directionalSwayRot);
                __instance.HandsContainer.WeaponRoot.localPosition += directionalSwayPos;
                __instance.HandsContainer.WeaponRoot.localRotation *= directionalSwayRot;

                // weapon selection transitions
                WeaponSelectionController.GetWeaponSelectionTransforms(out Vector3 WeaponTransitionPos, out Quaternion WeaponTransitionRot);
                __instance.HandsContainer.WeaponRoot.localPosition += WeaponTransitionPos;
                __instance.HandsContainer.WeaponRoot.localRotation *= WeaponTransitionRot;
            }
        }
    }
}
