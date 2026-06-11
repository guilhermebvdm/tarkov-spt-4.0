// Decompiled with JetBrains decompiler
// Type: RealismMod.VisualEffects.PrismEffectsEnablePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using RealismMod.Health;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod.VisualEffects;

public class PrismEffectsEnablePatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (PrismEffects).GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPostfix]
  private static void PatchPostFix(PrismEffects __instance)
  {
    if (!(((Object) ((Component) __instance).gameObject).name == "FPS Camera") || Object.op_Equality((Object) ScreenEffectsController.PrismEffects, (Object) __instance))
      return;
    ScreenEffectsController.PrismEffects = __instance;
  }
}
