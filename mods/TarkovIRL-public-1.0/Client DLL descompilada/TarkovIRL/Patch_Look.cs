// Decompiled with JetBrains decompiler
// Type: TarkovIRL.Patch_Look
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using EFT;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal class Patch_Look : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Player).GetMethod("Look", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPostfix]
  private static void Postfix(Player __instance)
  {
    if (!Object.op_Inequality((Object) __instance, (Object) null) || !__instance.IsYourPlayer || __instance.MovementContext.CurrentState.Name == 21 || !PrimeMover.IsSmallMovementsEffect.Value)
      return;
    Vector3 headRotThisFrame = HeadRotController.GetHeadRotThisFrame(__instance.HeadRotation);
    __instance.HeadRotation = headRotThisFrame;
    __instance.ProceduralWeaponAnimation.SetHeadRotation(__instance.HeadRotation);
  }
}
