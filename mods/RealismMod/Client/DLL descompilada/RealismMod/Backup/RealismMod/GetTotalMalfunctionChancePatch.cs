// Decompiled with JetBrains decompiler
// Type: RealismMod.GetTotalMalfunctionChancePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class GetTotalMalfunctionChancePatch : ModulePatch
{
  private static FieldInfo playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    GetTotalMalfunctionChancePatch.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("GetTotalMalfunctionChance", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(
    ref float __result,
    AmmoItemClass ammoToFire,
    Player.FirearmController __instance,
    float overheat,
    out double durabilityMalfChance,
    out float magMalfChance,
    out float ammoMalfChance,
    out float overheatMalfChance,
    out float weaponDurability)
  {
    if (((Player) GetTotalMalfunctionChancePatch.playerField.GetValue((object) __instance)).IsYourPlayer)
    {
      durabilityMalfChance = 0.0;
      magMalfChance = 0.0f;
      ammoMalfChance = 0.0f;
      overheatMalfChance = 0.0f;
      weaponDurability = 0.0f;
      if (!__instance.Item.AllowMalfunction)
      {
        __result = 0.0f;
        return false;
      }
      if (__instance.Weapon.AmmoCaliber == "762x35" && ammoToFire.Caliber == "556x45NATO")
      {
        __result = 10000f;
        return false;
      }
      BackendConfigSettingsClass instance = Singleton<BackendConfigSettingsClass>.Instance;
      BackendConfigSettingsClass.GClass1501 malfunction = instance.Malfunction;
      BackendConfigSettingsClass.GClass1502 overheat1 = instance.Overheat;
      float totalMalfChance = WeaponStats.TotalMalfChance;
      Mathf.Min(WeaponStats.MalfChanceDelta * 3f, 0.99f);
      float ammoMalfChanceMult = malfunction.AmmoMalfChanceMult;
      ammoMalfChance = ammoToFire != null ? (float) (1.0 + ((double) ammoToFire.MalfMisfireChance + (double) ammoToFire.MalfFeedChance) * (double) ammoMalfChanceMult) : 1f;
      MagazineItemClass currentMagazine = ((Item) __instance.Item).GetCurrentMagazine();
      magMalfChance = currentMagazine == null ? 1f : (float) (1.0 + (double) currentMagazine.MalfunctionChance * (double) malfunction.MagazineMalfChanceMult);
      durabilityMalfChance = 2.0 - (double) __instance.Item.Repairable.Durability / (double) __instance.Item.Repairable.TemplateDurability;
      weaponDurability = (float) ((double) __instance.Item.Repairable.Durability / (double) __instance.Item.Repairable.TemplateDurability * 100.0);
      overheatMalfChance = 1f + Mathf.Clamp01(overheat / 100f);
      overheatMalfChance = Mathf.Pow(overheatMalfChance, 3f);
      float num1 = (float) (1.0 + (double) ShootController.ShotCount / 200.0);
      float num2 = ShootController.ShotCount > 2 ? Mathf.Max(WeaponStats.AutoFireRateDelta, 1f) : 1f;
      bool flag1 = !WeaponStats.CanCycleSubs && ammoToFire.ammoHear == 1;
      bool flag2 = __instance.IsSilenced || WeaponStats.HasBooster;
      bool flag3 = (((double) weaponDurability < (double) PluginConfig.DuraMalfThreshold.Value || (double) overheatMalfChance > 1.5 || (double) ShootController.ShotCount > 6.0 || (double) magMalfChance > 2.0 || (double) ammoMalfChance > 1.5 || (double) WeaponStats.MalfChanceDelta < -0.5 ? 1 : ((double) totalMalfChance > 0.0040000001899898052 ? 1 : 0)) | (flag1 ? 1 : 0)) != 0;
      durabilityMalfChance = (double) weaponDurability < (double) PluginConfig.DuraMalfReductionThreshold.Value ? ((double) weaponDurability < 60.0 ? Math.Pow(durabilityMalfChance, 5.0) : Math.Pow(durabilityMalfChance, 3.0)) : Math.Pow(durabilityMalfChance, 1.5);
      float num3 = 0.0f;
      if (flag1)
      {
        float num4 = flag2 ? 0.25f : 1f;
        num3 = !(ammoToFire.Caliber == "762x39") ? 0.055f * num4 : 0.04f * num4;
      }
      float num5 = (float) durabilityMalfChance * overheatMalfChance * ammoMalfChance * magMalfChance * num1 * num2 * (float) __instance.Item.Buff.MalfunctionProtections;
      float num6 = (totalMalfChance + num3) * num5;
      float num7 = flag3 ? num6 : num6 * 0.01f;
      __result = num7 * PluginConfig.MalfMulti.Value;
      if (PluginConfig.EnableBallisticsLogging.Value)
      {
        ModulePatch.Logger.LogWarning((object) "=============");
        ModulePatch.Logger.LogWarning((object) ("canDoMalfChance " + flag3.ToString()));
        ModulePatch.Logger.LogWarning((object) ("weapon base chance " + totalMalfChance.ToString()));
        ModulePatch.Logger.LogWarning((object) ("weapon malf delta " + WeaponStats.MalfChanceDelta.ToString()));
        ModulePatch.Logger.LogWarning((object) ("ammo malf chance " + ammoMalfChance.ToString()));
        ModulePatch.Logger.LogWarning((object) ("heat malf chance " + overheatMalfChance.ToString()));
        ModulePatch.Logger.LogWarning((object) ("durability " + weaponDurability.ToString()));
        ModulePatch.Logger.LogWarning((object) ("durability malf chance " + durabilityMalfChance.ToString()));
        ModulePatch.Logger.LogWarning((object) $"mag malf chance {magMalfChance}");
        ModulePatch.Logger.LogWarning((object) ("WeaponStats.FireRateDelta " + WeaponStats.AutoFireRateDelta.ToString()));
        ModulePatch.Logger.LogWarning((object) ("subFactor " + num3.ToString()));
        ModulePatch.Logger.LogWarning((object) ("total malf chance " + __result.ToString()));
      }
      return false;
    }
    durabilityMalfChance = 0.0;
    magMalfChance = 0.0f;
    ammoMalfChance = 0.0f;
    overheatMalfChance = 0.0f;
    weaponDurability = 0.0f;
    return true;
  }
}
