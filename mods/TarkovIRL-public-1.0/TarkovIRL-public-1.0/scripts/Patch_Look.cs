using EFT;
using EFT.Animations;
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
    internal class Patch_Look : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player).GetMethod("Look", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void Postfix(Player __instance)
        {
            if ((UnityEngine.Object)(object)__instance != (UnityEngine.Object)null && __instance.IsYourPlayer && __instance.MovementContext.CurrentState.Name != EPlayerState.Stationary)
            {
                if (PrimeMover.IsSmallMovementsEffect.Value)
                {
                    Vector3 headRotThisFrame = HeadRotController.GetHeadRotThisFrame(__instance.HeadRotation);
                    __instance.HeadRotation = headRotThisFrame;
                    __instance.ProceduralWeaponAnimation.SetHeadRotation(__instance.HeadRotation);
                }
            }
        }
    }
}
