using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static EFT.Player;

namespace TarkovIRL
{
    public class Patch_ProcessRotation : ModulePatch
    {
        static FieldInfo movementContextField;

        protected override MethodBase GetTargetMethod()
        {
            movementContextField = AccessTools.Field(typeof(MovementState), "MovementContext");
            //playerField = AccessTools.Field(typeof(MovementState), "_player");
            return typeof(MovementState).GetMethod("ProcessRotation", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool Prefix(MovementState __instance, float deltaTime)
        {
            MovementContext movementContext = (MovementContext)movementContextField.GetValue(__instance);
            //Player player = (Player)movementContextField.GetValue(MContext);
            // if not your player return true

            TIRLUtils.LogError($"turn called, HandsToBodyAngle {movementContext.HandsToBodyAngle}, TrunkRotationLimit {movementContext.TrunkRotationLimit}");
            if (Mathf.Abs(movementContext.HandsToBodyAngle) > movementContext.TrunkRotationLimit * 0.2f)
            {
                __instance.ProcessUpperbodyRotation(deltaTime);
            }
            __instance.UpdateRotationSpeed(deltaTime);

            return false;

            //return true;
        }
    }
}