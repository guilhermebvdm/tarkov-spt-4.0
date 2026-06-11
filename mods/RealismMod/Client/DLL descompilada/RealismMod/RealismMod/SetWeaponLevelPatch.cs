// Decompiled with JetBrains decompiler
// Type: RealismMod.SetWeaponLevelPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class SetWeaponLevelPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (FirearmsAnimator).GetMethod("SetWeaponLevel");
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(FirearmsAnimator __instance, float weaponLevel)
  {
    if (!(WeaponStats._WeapClass == "shotgun"))
      return;
    weaponLevel = Mathf.Clamp(weaponLevel + 1f, 1f, 3f);
    WeaponAnimationSpeedControllerClass.SetWeaponLevel(((ObjectInHandsAnimator) __instance).Animator, weaponLevel);
  }
}
