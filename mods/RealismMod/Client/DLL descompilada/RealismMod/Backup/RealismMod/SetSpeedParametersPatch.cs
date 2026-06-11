// Decompiled with JetBrains decompiler
// Type: RealismMod.SetSpeedParametersPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class SetSpeedParametersPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (FirearmsAnimator).GetMethod("SetSpeedParameters", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(FirearmsAnimator __instance)
  {
    Player yourPlayer = Utils.GetYourPlayer();
    if (Object.op_Equality((Object) yourPlayer, (Object) null))
      return;
    Player.FirearmController handsController = yourPlayer.HandsController as Player.FirearmController;
    if (Object.op_Equality((Object) handsController, (Object) null) || ((Player.AbstractHandsController) handsController).FirearmsAnimator != __instance)
      return;
    ((ObjectInHandsAnimator) __instance).SetAnimationSpeed(1f);
    if (PluginConfig.EnableReloadLogging.Value)
      ModulePatch.Logger.LogWarning((object) "===SetSpeedParameters===");
  }
}
