// Decompiled with JetBrains decompiler
// Type: RealismMod.COIDisplayDeltaPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class COIDisplayDeltaPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Weapon).GetMethod("method_17", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(Weapon __instance, ref float __result)
  {
    float num1 = __instance.method_9((float) __instance.Repairable.TemplateDurability);
    float num2 = (float) (((double) __instance.GetBarrelDeviation() - (double) num1) / ((double) __instance.Single_0 - (double) num1));
    __result = UIWeaponStats.COIDelta + num2;
    return false;
  }
}
