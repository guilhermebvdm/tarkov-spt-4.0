// Decompiled with JetBrains decompiler
// Type: RealismMod.ModConstructorPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class ModConstructorPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Mod).GetConstructor(new Type[2]
    {
      typeof (string),
      typeof (ModTemplate)
    });
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(Mod __instance, string id, ModTemplate template)
  {
    WeaponMod dataObj = TemplateStats.GetDataObj<WeaponMod>(TemplateStats.WeaponModStats, MongoID.op_Implicit(((Item) __instance).TemplateId));
    float verticalRecoil = dataObj.VerticalRecoil;
    float horizontalRecoil = dataObj.HorizontalRecoil;
    float dispersion = dataObj.Dispersion;
    float cameraRecoil = dataObj.CameraRecoil;
    float autoRof = dataObj.AutoROF;
    float semiRof = dataObj.SemiROF;
    float malfunctionChance = dataObj.ModMalfunctionChance;
    float recoilAngle = dataObj.RecoilAngle;
    float reloadSpeed = dataObj.ReloadSpeed;
    float chamberSpeed = dataObj.ChamberSpeed;
    float aimSpeed = dataObj.AimSpeed;
    float modShotDispersion = dataObj.ModShotDispersion;
    float convergence = dataObj.Convergence;
    float meleeDamage = dataObj.MeleeDamage;
    float meleePen = dataObj.MeleePen;
    float flash = dataObj.Flash;
    float aimStability = dataObj.AimStability;
    float handling = dataObj.Handling;
    if (Plugin.ServerConfig.malf_changes)
      Utils.AddAttribute((Item) __instance, Attributes.ENewItemAttributeId.MalfunctionChance, malfunctionChance, ModConstructorPatch.getMalfOdds(malfunctionChance) ?? "", new bool?(true));
    if (dataObj.StockAllowADS)
      Utils.AddAttribute((Item) __instance, Attributes.ENewItemAttributeId.CanADS, 1f, "", colored: false);
    if ((double) flash != 0.0 && PluginConfig.EnableMuzzleEffects.Value)
    {
      bool flag = !Utils.IsMuzzleCombo(__instance) && !Utils.IsFlashHider(__instance) && !Utils.IsBarrel(__instance);
      float baseValue = flash * (flag ? 1f : -1f);
      string name = flag ? Attributes.ENewItemAttributeId.Gas.GetName() : Attributes.ENewItemAttributeId.MuzzleFlash.GetName();
      Utils.AddAttribute((Item) __instance, Attributes.ENewItemAttributeId.MuzzleFlash, baseValue, $"{baseValue}%", new bool?(flag), name);
    }
    Utils.AddAttribute((Item) __instance, Attributes.ENewItemAttributeId.HorizontalRecoil, horizontalRecoil, $"{horizontalRecoil}%", new bool?(true));
    Utils.AddAttribute((Item) __instance, Attributes.ENewItemAttributeId.VerticalRecoil, verticalRecoil, $"{verticalRecoil}%", new bool?(true));
    Utils.AddAttribute((Item) __instance, Attributes.ENewItemAttributeId.Dispersion, dispersion, $"{dispersion}%", new bool?(true));
    Utils.AddAttribute((Item) __instance, Attributes.ENewItemAttributeId.CameraRecoil, cameraRecoil, $"{cameraRecoil}%", new bool?(true));
    Utils.AddAttribute((Item) __instance, Attributes.ENewItemAttributeId.AutoROF, autoRof, $"{autoRof}%", new bool?(true));
    Utils.AddAttribute((Item) __instance, Attributes.ENewItemAttributeId.SemiROF, semiRof, $"{semiRof}%", new bool?(false));
    Utils.AddAttribute((Item) __instance, Attributes.ENewItemAttributeId.RecoilAngle, recoilAngle, $"{recoilAngle}%", new bool?(false));
    Utils.AddAttribute((Item) __instance, Attributes.ENewItemAttributeId.ReloadSpeed, reloadSpeed, $"{reloadSpeed}%", new bool?(false));
    Utils.AddAttribute((Item) __instance, Attributes.ENewItemAttributeId.ChamberSpeed, chamberSpeed, $"{chamberSpeed}%", new bool?(false));
    Utils.AddAttribute((Item) __instance, Attributes.ENewItemAttributeId.AimSpeed, aimSpeed, $"{aimSpeed}%", new bool?(false));
    Utils.AddAttribute((Item) __instance, Attributes.ENewItemAttributeId.AimStability, aimStability, $"{aimStability}%", new bool?(false));
    Utils.AddAttribute((Item) __instance, Attributes.ENewItemAttributeId.Handling, handling, $"{handling}%", new bool?(false));
    Utils.AddAttribute((Item) __instance, Attributes.ENewItemAttributeId.ShotDispersion, modShotDispersion, $"{modShotDispersion}%", new bool?(false));
    Utils.AddAttribute((Item) __instance, Attributes.ENewItemAttributeId.Convergence, convergence, $"{convergence}%", new bool?(false));
    Utils.AddAttribute((Item) __instance, Attributes.ENewItemAttributeId.MeleeDamage, meleeDamage, $"{meleeDamage}%", new bool?(false));
    Utils.AddAttribute((Item) __instance, Attributes.ENewItemAttributeId.MeleePen, meleePen, $"{meleePen}%", new bool?(false));
  }

  public static string getMalfOdds(float malfChance)
  {
    float num = malfChance;
    if ((double) num < 0.0)
      return $"{malfChance}%";
    if ((double) num == 0.0)
      return "No Change";
    if ((double) num <= 50.0)
      return $"{malfChance}%";
    if ((double) num <= 100.0)
      return "Moderate Increase";
    if ((double) num <= 500.0)
      return "Significant Increase";
    if ((double) num <= 1000.0)
      return "Large Increase";
    return (double) num <= 5000.0 ? "Critical Increase" : "";
  }
}
