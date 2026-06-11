// Decompiled with JetBrains decompiler
// Type: RealismMod.CenterOfImpactMOAPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class CenterOfImpactMOAPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (BarrelItemClass).GetMethod("get_CenterOfImpactMOA", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(BarrelItemClass __instance, ref float __result)
  {
    BarrelComponent itemComponent = ((Item) __instance).GetItemComponent<BarrelComponent>();
    __result = itemComponent != null ? (float) Math.Round(100.0 * (double) itemComponent.Template.CenterOfImpact / 2.9089000225067139 * 2.0, 2) : 0.0f;
    return false;
  }
}
