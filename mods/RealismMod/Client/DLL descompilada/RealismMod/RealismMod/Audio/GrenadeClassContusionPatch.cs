// Decompiled with JetBrains decompiler
// Type: RealismMod.Audio.GrenadeClassContusionPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod.Audio;

public class GrenadeClassContusionPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (ThrowWeapItemClass).GetMethod("get_Contusion");
  }

  [PatchPrefix]
  private static bool PreFix(ThrowWeapItemClass __instance, ref Vector3 __result)
  {
    Vector3 contusion = ((Item) __instance).GetTemplate<ThrowWeapTemplateClass>().Contusion;
    float num1 = contusion.z * (float) (1.0 - (1.0 - (double) DeafenController.EarProtectionFactor) * 1.2999999523162842);
    float num2 = contusion.y * 2f * DeafenController.EarProtectionFactor;
    float num3 = PlayerState.EnviroType == 1 ? num1 * 1.7f : num1;
    float num4 = PlayerState.EnviroType == 1 ? num2 * 1.7f : num2;
    __result = new Vector3(contusion.x, num4, num3);
    return false;
  }
}
