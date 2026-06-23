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

namespace TarkovIRL
{
    internal class Patch_ProcessUpperbodyRotation : ModulePatch
    {
        static FieldInfo movementContextField;
        protected override MethodBase GetTargetMethod()
        {
            movementContextField = AccessTools.Field(typeof(MovementState), "MovementContext");
            return typeof(MovementState).GetMethod("ProcessUpperbodyRotation", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool Prefix(MovementState __instance, float deltaTime)
        {
            MovementContext MContext = (MovementContext)movementContextField.GetValue(__instance);
            //
            float y = Mathf.DeltaAngle((float)Math.Sign(MContext.HandsToBodyAngle) * MContext.TrunkRotationLimit, MContext.HandsToBodyAngle);
            //y *= 0.5f;
            MContext.ApplyRotation(Quaternion.Lerp(MContext.TransformRotation, MContext.TransformRotation * Quaternion.Euler(0f, y, 0f), 30f * PrimeMover.Instance.DeltaTime));
            //
            return false;
        }
    }
}
