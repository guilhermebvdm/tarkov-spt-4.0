// Decompiled with JetBrains decompiler
// Type: RealismMod.ErgoDeltaPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using RealismMod.Weapons;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class ErgoDeltaPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Weapon).GetMethod("get_ErgonomicsDelta", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(Weapon __instance, ref float __result)
  {
    if (!Utils.PlayerIsReady || __instance == null || ((Item) __instance)?.Owner == null || __instance == null || ((IContainer) ((Item) __instance).Owner)?.ID == null || !(((IContainer) ((Item) __instance)?.Owner)?.ID == Singleton<GameWorld>.Instance?.MainPlayer?.ProfileId))
      return true;
    Gun dataObj = TemplateStats.GetDataObj<Gun>(TemplateStats.GunStats, MongoID.op_Implicit(((Item) __instance).TemplateId));
    if (PlayerState.IsInReloadOpertation)
    {
      __result = ErgoDeltaPatch.FinalStatCalc(__instance, dataObj);
    }
    else
    {
      ErgoDeltaPatch.InitialStaCalc(__instance, dataObj);
      __result = ErgoDeltaPatch.FinalStatCalc(__instance, dataObj);
    }
    return false;
  }

  public static float FinalStatCalc(Weapon __instance, Gun gunStats)
  {
    WeaponStats.IsPistol = __instance.WeapClass == "pistol";
    WeaponStats._WeapClass = __instance.WeapClass;
    WeaponStats._IsManuallyOperated = gunStats.IsManuallyOperated;
    WeaponStats.EnableBSGVisRecoil = gunStats.EnableBSGVisRecoil;
    WeaponStats.ReduceBSGVisRecoil = gunStats.ReduceBSGVisRecoil;
    float totalWeight1 = ((Item) __instance).TotalWeight;
    string weapType = gunStats.WeapType;
    string operationType = gunStats.OperationType;
    Mod currentMagazine = (Mod) ((Item) __instance).GetCurrentMagazine();
    float num1 = 0.0f;
    float num2 = 0.0f;
    float num3 = 0.0f;
    bool flag = currentMagazine != null;
    WeaponStats.HasLongMag = false;
    if (flag)
    {
      WeaponMod dataObj = TemplateStats.GetDataObj<WeaponMod>(TemplateStats.WeaponModStats, MongoID.op_Implicit(((Item) currentMagazine).TemplateId));
      float weight = StatCalc.FactoredWeight(num2);
      string modPosition = StatCalc.GetModPosition(currentMagazine, weapType, operationType, "");
      num2 = ((Item) currentMagazine).TotalWeight;
      num1 = currentMagazine.Ergonomics;
      num3 = StatCalc.GetTorque(modPosition, weight);
      WeaponStats.HasLongMag = dataObj.ModType == "long_mag";
    }
    float totalWeight2 = totalWeight1 - num2;
    float reloadSpeedModifier = WeaponStats.SDReloadSpeedModifier;
    float chamberSpeedModifier = WeaponStats.SDChamberSpeedModifier;
    float recoilDamping = gunStats.RecoilDamping;
    float recoilHandDamping = gunStats.RecoilHandDamping;
    float ergonomics = __instance.Template.Ergonomics;
    float num4 = StatCalc.WeightStatCalc(13.5f, __instance.IsBeltMachineGun ? num2 * 0.5f : num2) / 100f;
    float currentErgo = WeaponStats.InitTotalErgo + WeaponStats.InitTotalErgo * (num1 / 100f + num4);
    float totalPureErgo = WeaponStats.InitPureErgo + WeaponStats.InitPureErgo * (num1 / 100f);
    float recoilForceUp = __instance.Template.RecoilForceUp;
    float num5 = StatCalc.WeightStatCalc(2f, num2) / 100f;
    float currentVRecoil = WeaponStats.InitTotalVRecoil + WeaponStats.InitTotalVRecoil * num5;
    float recoilForceBack = __instance.Template.RecoilForceBack;
    float num6 = StatCalc.WeightStatCalc(3.55f, num2) / 100f;
    float currentHRecoil = WeaponStats.InitTotalHRecoil + WeaponStats.InitTotalHRecoil * num6;
    float num7 = StatCalc.WeightStatCalc(1.5f, num2) / 100f;
    float currentDispersion = WeaponStats.InitDispersion + WeaponStats.InitDispersion * num7;
    float initCamRecoil = WeaponStats.InitCamRecoil;
    float initRecoilAngle = WeaponStats.InitRecoilAngle;
    float currentTorque = WeaponStats.InitBalance + num3;
    float totalTorque = 0.0f;
    float totalErgo = 0.0f;
    float totalErgoLessMag = 0.0f;
    float totalVRecoil = 0.0f;
    float totalHRecoil = 0.0f;
    float totalDispersion = 0.0f;
    float totalCamRecoil = 0.0f;
    float totalRecoilAngle = 0.0f;
    float totalRecoilDamping = 0.0f;
    float totalRecoilHandDamping = 0.0f;
    float totalErgoDelta = 0.0f;
    float totalPureErgoDelta = 0.0f;
    float totalVRecoilDelta = 0.0f;
    float totalHRecoilDelta = 0.0f;
    float totalCOI = 0.0f;
    float totalCOIDelta = 0.0f;
    StatCalc.WeaponStatCalc(__instance, gunStats, currentTorque, ref totalTorque, currentErgo, currentVRecoil, currentHRecoil, currentDispersion, initCamRecoil, initRecoilAngle, ergonomics, recoilForceUp, recoilForceBack, ref totalErgo, ref totalVRecoil, ref totalHRecoil, ref totalDispersion, ref totalCamRecoil, ref totalRecoilAngle, ref totalRecoilDamping, ref totalRecoilHandDamping, ref totalErgoDelta, ref totalVRecoilDelta, ref totalHRecoilDelta, ref recoilDamping, ref recoilHandDamping, WeaponStats.InitTotalCOI, WeaponStats.HasShoulderContact, ref totalCOI, ref totalCOIDelta, __instance.CenterOfImpactBase, totalPureErgo, ref totalPureErgoDelta, ref totalErgoLessMag, WeaponStats.InitTotalErgo, false);
    float num8 = StatCalc.ErgoWeightCalc(totalWeight1, totalPureErgoDelta, totalTorque, __instance.WeapClass);
    StatCalc.ErgoWeightCalc(totalWeight2, totalPureErgoDelta, totalTorque, __instance.WeapClass);
    float ergoWeight = Mathf.Max(1f, 80f - totalErgo);
    float ergonomicWeightLessMag = Mathf.Max(1f, 80f - WeaponStats.InitTotalErgo);
    float totalAimMoveSpeedFactor = 0.0f;
    float totalReloadSpeedLessMag = 0.0f;
    float totalChamberSpeed = 0.0f;
    float totalFiringChamberSpeed = 0.0f;
    float totalChamberCheckSpeed = 0.0f;
    float totalFixSpeed = 0.0f;
    StatCalc.SpeedStatCalc(__instance, gunStats, ergoWeight, ergonomicWeightLessMag, chamberSpeedModifier, reloadSpeedModifier, ref totalReloadSpeedLessMag, ref totalChamberSpeed, ref totalAimMoveSpeedFactor, ref totalFiringChamberSpeed, ref totalChamberCheckSpeed, ref totalFixSpeed);
    WeaponStats.TotalFixSpeed = totalFixSpeed;
    WeaponStats.TotalChamberCheckSpeed = totalChamberCheckSpeed;
    WeaponStats.TotalReloadSpeedLessMag = totalReloadSpeedLessMag;
    WeaponStats.TotalChamberSpeed = totalChamberSpeed;
    WeaponStats.TotalFiringChamberSpeed = totalFiringChamberSpeed;
    WeaponStats.AimMoveSpeedWeapModifier = totalAimMoveSpeedFactor;
    if (flag)
      ReloadController.MagReloadSpeedModifier(__instance, (MagazineItemClass) currentMagazine, false, false);
    if (PluginConfig.EnableGeneralLogging.Value)
    {
      ModulePatch.Logger.LogWarning((object) ("Shoulder = " + WeaponStats.HasShoulderContact.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Total Ergo = " + totalErgo.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Total Ergo D = " + totalErgoDelta.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Ergo factor = " + ergoWeight.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Pure Ergo = " + totalPureErgo.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Pure Ergo D = " + totalPureErgoDelta.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Dispersion = " + totalDispersion.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Dispersion Delta = " + (totalDispersion - (float) __instance.Template.RecolDispersion).ToString()));
      ModulePatch.Logger.LogWarning((object) ("Cam Recoil = " + totalCamRecoil.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Total V Recoil = " + totalVRecoil.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Total H Recoil = " + totalHRecoil.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Balance = " + totalTorque.ToString()));
      ModulePatch.Logger.LogWarning((object) ("COIDelta = " + totalCOIDelta.ToString()));
    }
    WeaponStats.TotalDispersion = totalDispersion;
    WeaponStats.TotalDispersionDelta = (totalDispersion - (float) __instance.Template.RecolDispersion) / (float) __instance.Template.RecolDispersion;
    WeaponStats.TotalCamRecoil = totalCamRecoil;
    WeaponStats.TotalRecoilAngle = PluginConfig.EnableAngle.Value ? Mathf.Max(totalRecoilAngle, 65f) : 90f;
    WeaponStats.TotalVRecoil = totalVRecoil;
    WeaponStats.TotalHRecoil = totalHRecoil;
    WeaponStats.Balance = totalTorque;
    WeaponStats.TotalErgo = Mathf.Clamp(totalErgo, 1f, 80f);
    WeaponStats.ErgoDelta = Mathf.Clamp(totalErgoDelta, -0.99f, 2f);
    WeaponStats.VRecoilDelta = totalVRecoilDelta;
    WeaponStats.HRecoilDelta = totalHRecoilDelta;
    WeaponStats.ErgoFactor = Mathf.Clamp(80f - totalErgo, 1f, 80f);
    WeaponStats.ErgonomicWeight = num8;
    WeaponStats.TotalRecoilDamping = totalRecoilDamping;
    WeaponStats.TotalRecoilHandDamping = totalRecoilHandDamping;
    WeaponStats.COIDelta = totalCOIDelta;
    WeaponStats.PureErgoDelta = totalPureErgoDelta;
    WeaponStats.CurrentVisualRecoilMulti = gunStats.VisualMulti;
    return totalErgoDelta;
  }

  public static void InitialStaCalc(Weapon __instance, Gun weapStats)
  {
    WeaponStats.IsPistol = __instance.WeapClass == "pistol";
    WeaponStats._WeapClass = __instance.WeapClass;
    WeaponStats._IsManuallyOperated = weapStats.IsManuallyOperated;
    bool isChonker = __instance.IsBeltMachineGun || (double) ((Item) __instance).TotalWeight >= 10.0;
    WeaponStats.ShouldGetSemiIncrease = false;
    if (!WeaponStats.IsPistol || __instance.WeapClass != "shotgun" || __instance.WeapClass != "sniperRifle" || __instance.WeapClass != "smg")
      WeaponStats.ShouldGetSemiIncrease = true;
    float centerOfImpactBase = __instance.CenterOfImpactBase;
    float bFirerate = (float) __instance.Template.bFirerate;
    float currentSemiROF = (float) Mathf.Max(__instance.Template.SingleFireRate, 240 /*0xF0*/);
    float recoilCamera = __instance.Template.RecoilCamera;
    float visualMulti = weapStats.VisualMulti;
    float speedHandRotation = __instance.Template.RecoilReturnSpeedHandRotation;
    float recolDispersion = (float) __instance.Template.RecolDispersion;
    float recoilAngle1 = (float) __instance.Template.RecoilAngle;
    float recoilForceUp = __instance.Template.RecoilForceUp;
    float currentVRecoil = recoilForceUp;
    float recoilForceBack = __instance.Template.RecoilForceBack;
    float currentHRecoil = recoilForceBack;
    float ergonomics1 = __instance.Template.Ergonomics;
    float currentErgo = ergonomics1;
    float pureErgo = ergonomics1;
    float pureRecoil = recoilForceUp + recoilForceBack;
    float shotgunDispersionBase = __instance.ShotgunDispersionBase;
    float currentShotDisp = shotgunDispersionBase;
    float currentTorque = 0.0f;
    float num1 = 0.0f;
    float num2 = 0.0f;
    float num3 = 0.0f;
    float num4 = 0.0f;
    float num5 = 0.0f;
    float num6 = 1f;
    float malfunctionChance1 = __instance.BaseMalfunctionChance;
    float currentMalfChance = malfunctionChance1;
    string operationType = weapStats.OperationType;
    string weapType = weapStats.WeapType;
    string ammoCaliber = __instance.AmmoCaliber;
    float num7 = 0.0f;
    bool weaponAllowAds = weapStats.WeaponAllowADS;
    bool stockAllowsFSADS = false;
    bool flag = false;
    float num8 = 0.0f;
    float num9 = 0.0f;
    float num10 = 0.0f;
    bool folded = __instance.Folded;
    bool hasShoulderContact = weapStats.HasShoulderContact;
    WeaponStats.BaseMeleeDamage = 0.0f;
    WeaponStats.BaseMeleePen = 0.0f;
    WeaponStats.IsVector = weapType == "vector";
    WeaponStats.HasBayonet = false;
    WeaponStats.HasBooster = false;
    WeaponStats.HasMuzzleDevice = false;
    WeaponStats.HasSuppressor = false;
    foreach (Mod mod in __instance.Mods)
    {
      if (!Utils.IsMagazine(mod))
      {
        WeaponMod dataObj = TemplateStats.GetDataObj<WeaponMod>(TemplateStats.WeaponModStats, MongoID.op_Implicit(((Item) mod).TemplateId));
        string modType = dataObj.ModType;
        float weight = ((Item) mod).Weight;
        float modWeightFactored = StatCalc.FactoredWeight(weight);
        float ergonomics2 = mod.Ergonomics;
        float verticalRecoil = dataObj.VerticalRecoil;
        float convergence = dataObj.Convergence;
        verticalRecoil += (double) convergence > 0.0 ? convergence * -1f : 0.0f;
        float horizontalRecoil = dataObj.HorizontalRecoil;
        float autoRof = dataObj.AutoROF;
        float semiRof = dataObj.SemiROF;
        float cameraRecoil = dataObj.CameraRecoil;
        float dispersion = dataObj.Dispersion;
        float recoilAngle2 = dataObj.RecoilAngle;
        float accuracy = (float) mod.Accuracy;
        float reloadSpeed = dataObj.ReloadSpeed;
        float chamberSpeed = dataObj.ChamberSpeed;
        float aimSpeed = dataObj.AimSpeed;
        float modShotDispersion = dataObj.ModShotDispersion;
        string modPosition = StatCalc.GetModPosition(mod, weapType, operationType, modType);
        float loudness = (float) mod.Loudness;
        float malfunctionChance2 = dataObj.ModMalfunctionChance;
        float durabilityBurnModificator = mod.DurabilityBurnModificator;
        float fixSpeed = dataObj.FixSpeed;
        float flash = dataObj.Flash;
        float handling = dataObj.Handling;
        float aimStability = dataObj.AimStability;
        if (Utils.IsMuzzleDevice(mod))
        {
          if (modType == "bayonet")
            WeaponStats.HasBayonet = true;
          if (modType == "booster")
            WeaponStats.HasBooster = true;
          if (Utils.IsSilencer(mod))
            WeaponStats.HasSuppressor = true;
          WeaponStats.BaseMeleeDamage = dataObj.MeleeDamage;
          WeaponStats.BaseMeleePen = dataObj.MeleePen;
          WeaponStats.HasMuzzleDevice = true;
        }
        if (dataObj.CanCycleSubs)
          flag = true;
        StatCalc.ModConditionalStatCalc(__instance, weapStats, mod, dataObj, folded, weapType, operationType, ref hasShoulderContact, ref autoRof, ref semiRof, ref stockAllowsFSADS, ref verticalRecoil, ref horizontalRecoil, ref cameraRecoil, ref recoilAngle2, ref dispersion, ref ergonomics2, ref accuracy, ref modType, ref modPosition, ref chamberSpeed, ref loudness, ref malfunctionChance2, ref durabilityBurnModificator, ref convergence, ref flash, ref aimStability, ref handling, ref aimSpeed);
        StatCalc.ModStatCalc(mod, false, isChonker, weight, ref currentTorque, modPosition, modWeightFactored, autoRof, ref bFirerate, semiRof, ref currentSemiROF, cameraRecoil, ref recoilCamera, dispersion, ref recolDispersion, recoilAngle2, ref recoilAngle1, accuracy, ref centerOfImpactBase, ergonomics2, ref currentErgo, verticalRecoil, ref currentVRecoil, horizontalRecoil, ref currentHRecoil, ref pureErgo, modShotDispersion, ref currentShotDisp, ref currentMalfChance, malfunctionChance2, ref pureRecoil, ref speedHandRotation, convergence, ref visualMulti);
        if (!Utils.IsMuzzleCombo(mod) && !Utils.IsFlashHider(mod) && !Utils.IsBarrel(mod))
          num10 += flash;
        else
          num9 += flash;
        if (!Utils.IsSight(mod))
          num2 += aimSpeed;
        num3 += chamberSpeed;
        num4 += aimStability;
        num1 += reloadSpeed;
        num7 += loudness;
        num5 += handling;
        num8 += fixSpeed;
        num6 *= durabilityBurnModificator;
      }
    }
    WeaponStats.WeaponCanFSADS = weaponAllowAds || stockAllowsFSADS || !hasShoulderContact;
    WeaponStats.IsMachinePistol = weapType == "smg_pistol" && !hasShoulderContact;
    WeaponStats.IsStocklessPistol = !hasShoulderContact && WeaponStats.IsPistol;
    WeaponStats.IsStockedPistol = hasShoulderContact && WeaponStats.IsPistol;
    float num11 = (float) ((double) num7 / 80.0 + 1.0) * StatCalc.CaliberLoudnessFactor(ammoCaliber);
    if (weapType == "bullpup" || operationType == "p90")
    {
      num11 *= 1.18f;
      WeaponStats.IsBullpup = true;
    }
    else
      WeaponStats.IsBullpup = false;
    float num12 = (float) (((double) recoilForceUp + (double) recoilForceBack - (double) pureRecoil) / (((double) recoilForceUp + (double) recoilForceBack) * -1.0));
    WeaponStats.TotalModDuraBurn = num6;
    WeaponStats.TotalMalfChance = Mathf.Max(currentMalfChance, malfunctionChance1);
    WeaponStats.MalfChanceDelta = (malfunctionChance1 - WeaponStats.TotalMalfChance) / malfunctionChance1;
    DeafenController.GunDeafFactor = num11;
    WeaponStats.CanCycleSubs = flag;
    WeaponStats.HasShoulderContact = hasShoulderContact;
    WeaponStats.InitTotalErgo = currentErgo;
    WeaponStats.InitTotalVRecoil = currentVRecoil;
    WeaponStats.InitTotalHRecoil = currentHRecoil;
    WeaponStats.InitBalance = currentTorque;
    WeaponStats.InitCamRecoil = recoilCamera;
    WeaponStats.InitDispersion = recolDispersion;
    WeaponStats.InitRecoilAngle = recoilAngle1;
    WeaponStats.SDReloadSpeedModifier = num1;
    WeaponStats.SDChamberSpeedModifier = num3;
    WeaponStats.SDFixSpeedModifier = num8;
    WeaponStats.ModAimSpeedModifier = num2 / 100f;
    WeaponStats.AutoFireRate = Mathf.Max(400, (int) bFirerate);
    WeaponStats.SemiFireRate = Mathf.Max(300, (int) currentSemiROF);
    WeaponStats.FireRateDelta = (float) ((double) WeaponStats.AutoFireRate / (double) __instance.Template.bFirerate * ((double) WeaponStats.SemiFireRate / (double) __instance.Template.SingleFireRate));
    WeaponStats.AutoFireRateDelta = (float) WeaponStats.AutoFireRate / (float) __instance.Template.bFirerate;
    WeaponStats.SemiFireRateDelta = (float) WeaponStats.SemiFireRate / (float) __instance.Template.SingleFireRate;
    WeaponStats.InitTotalCOI = centerOfImpactBase;
    WeaponStats.InitPureErgo = pureErgo;
    WeaponStats.PureRecoilDelta = num12;
    WeaponStats.ShotDispDelta = (float) (((double) shotgunDispersionBase - (double) currentShotDisp) / ((double) shotgunDispersionBase * -1.0));
    WeaponStats.TotalCameraReturnSpeed = visualMulti;
    WeaponStats.TotalModdedConv = speedHandRotation * (!hasShoulderContact ? 0.7f : 1f);
    WeaponStats.ConvergenceDelta = speedHandRotation / __instance.Template.RecoilReturnSpeedHandRotation;
    WeaponStats.VelocityDelta = __instance.VelocityDelta;
    WeaponStats.MuzzleLoudness = num7;
    WeaponStats.Caliber = ammoCaliber;
    WeaponStats.TotalMuzzleFlash = num9;
    WeaponStats.TotalGas = num10;
    WeaponStats.IsDirectImpingement = weapType == "DI";
    WeaponStats.TotalAimStabilityModi = Mathf.Clamp((float) (1.0 - (double) num4 / 100.0), 0.25f, 2f);
    WeaponStats.TotalWeaponHandlingModi = Mathf.Clamp((float) (1.0 - (double) num5 / 100.0), 0.25f, 2f);
  }
}
