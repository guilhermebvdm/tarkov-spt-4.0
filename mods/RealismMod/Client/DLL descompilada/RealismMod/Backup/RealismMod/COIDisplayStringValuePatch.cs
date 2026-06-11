// Decompiled with JetBrains decompiler
// Type: RealismMod.COIDisplayStringValuePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class COIDisplayStringValuePatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Weapon).GetMethod("method_18", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(Weapon __instance, ref string __result)
  {
    __result = $"{((float) ((double) COIDisplayStringValuePatch.GetTotalCOI(__instance, true) * (double) __instance.GetBarrelDeviation() * 100.0 / 2.9089000225067139 * 2.0)).ToString("0.0#")} {GClass2112.Localized("moa", (string) null)}";
    return false;
  }

  private static float GetTotalCOI(Weapon __instance, bool includeAmmo)
  {
    float totalCoi = __instance.CenterOfImpactBase * (1f + UIWeaponStats.COIDelta);
    if (!includeAmmo)
      return totalCoi;
    AmmoTemplate currentAmmoTemplate = __instance.CurrentAmmoTemplate;
    return totalCoi * (currentAmmoTemplate != null ? currentAmmoTemplate.AmmoFactor : 1f);
  }
}
