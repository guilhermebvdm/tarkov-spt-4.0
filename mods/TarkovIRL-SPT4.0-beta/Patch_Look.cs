using EFT;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal class Patch_Look : ModulePatch
{
  protected override MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Player).GetMethod("Look", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPostfix]
  private static void Postfix(Player __instance)
  {
    if ((__instance == null) || !__instance.IsYourPlayer || (int)__instance.MovementContext.CurrentState.Name == 21 || !PrimeMover.IsSmallMovementsEffect.Value || !PrimeMover.EnableMod.Value)
      return;
    Vector3 headRotThisFrame = HeadRotController.GetHeadRotThisFrame(__instance.HeadRotation);
    __instance.HeadRotation = headRotThisFrame;
    __instance.ProceduralWeaponAnimation.SetHeadRotation(__instance.HeadRotation);
  }
}



