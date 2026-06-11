// Decompiled with JetBrains decompiler
// Type: RealismMod.GetDurabilityLossOnShotPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class GetDurabilityLossOnShotPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Weapon).GetMethod("GetDurabilityLossOnShot", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(
    Weapon __instance,
    ref float __result,
    float ammoBurnRatio,
    float overheatFactor,
    float skillWeaponTreatmentFactor,
    out float modsBurnRatio)
  {
    if (__instance != null && ((IContainer) ((Item) __instance).Owner)?.ID != null && ((IContainer) ((Item) __instance).Owner).ID == Singleton<GameWorld>.Instance.MainPlayer.ProfileId)
    {
      modsBurnRatio = WeaponStats.TotalModDuraBurn;
      __result = (float) ((double) __instance.Repairable.TemplateDurability / (double) __instance.Template.OperatingResource * (double) __instance.DurabilityBurnRatio * ((double) modsBurnRatio * (double) ammoBurnRatio) * (double) overheatFactor * (1.0 - (double) skillWeaponTreatmentFactor));
      return false;
    }
    modsBurnRatio = 1f;
    return true;
  }
}
