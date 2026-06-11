// Decompiled with JetBrains decompiler
// Type: RealismMod.GetTotalCenterOfImpactPatch
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

public class GetTotalCenterOfImpactPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Weapon).GetMethod("GetTotalCenterOfImpact", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(Weapon __instance, ref float __result, bool includeAmmo)
  {
    if (!Utils.PlayerIsReady || ((Item) __instance)?.Owner == null || __instance == null || ((IContainer) ((Item) __instance).Owner)?.ID == null || !(((IContainer) ((Item) __instance).Owner).ID == Singleton<GameWorld>.Instance.MainPlayer.ProfileId))
      return true;
    bool flag1 = StanceController.BracingDirection == EBracingDirection.Top;
    bool flag2 = StanceController.IsMounting & flag1;
    float num1 = !flag2 || !WeaponStats.BipodIsDeployed ? (flag2 ? 0.85f : (!StanceController.IsMounting || flag1 ? (StanceController.IsBracing & flag1 ? 0.95f : (!StanceController.IsBracing || flag1 ? 1f : 0.975f)) : 0.9f)) : 0.75f;
    float num2 = !WeaponStats.HasShoulderContact ? 2f : 1f;
    float num3 = (float) ((double) (__instance.CenterOfImpactBase * (1f + __instance.CenterOfImpactDelta)) * (1.0 - (double) WeaponStats.ScopeAccuracyFactor) * (double) num1 * (double) num2 * (PluginConfig.IncreaseCOI.Value ? 1.5 : 1.0));
    if (!includeAmmo)
    {
      __result = num3;
      return false;
    }
    AmmoTemplate currentAmmoTemplate = __instance.CurrentAmmoTemplate;
    __result = num3 * (currentAmmoTemplate != null ? currentAmmoTemplate.AmmoFactor : 1f);
    return false;
  }
}
