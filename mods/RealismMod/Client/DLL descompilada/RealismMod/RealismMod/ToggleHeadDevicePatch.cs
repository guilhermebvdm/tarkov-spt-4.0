// Decompiled with JetBrains decompiler
// Type: RealismMod.ToggleHeadDevicePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class ToggleHeadDevicePatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Player).GetMethod("method_15", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(Player __instance)
  {
    if (!__instance.IsYourPlayer)
      return true;
    Player.FirearmController handsController = __instance.HandsController as Player.FirearmController;
    return Object.op_Inequality((Object) handsController, (Object) null) ? ((Player.AbstractHandsController) handsController).FirearmsAnimator.IsIdling() && !PlayerState.BlockFSWhileConsooming : !PlayerState.BlockFSWhileConsooming;
  }
}
