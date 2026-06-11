// Decompiled with JetBrains decompiler
// Type: RealismMod.StatCalc
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.Animations;
using EFT.InventoryLogic;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace RealismMod;

public static class StatCalc
{
  public const float ConvVRecoilConversion = -1f;
  public const float ErgoWeightMult = 13.5f;
  public const float ErgoTorqueMult = 0.8f;
  public const float PistolErgoWeightMult = 12.6f;
  public const float PistolErgoTorqueMult = 1f;
  public const float VRecoilWeightMult = 2f;
  public const float VRecoilTorqueMult = 0.87f;
  public const float PistolVRecoilWeightMult = 1.5f;
  public const float PistolVRecoilTorqueMult = 1.5f;
  public const float HRecoilWeightMult = 3.55f;
  public const float HRecoilTorqueMult = 0.4f;
  public const float PistolHRecoilWeightMult = 3.5f;
  public const float PistolHRecoilTorqueMult = 0.8f;
  public const float DispersionWeightMult = 1.5f;
  public const float DispersionTorqueMult = 1.2f;
  public const float PistolDispersionWeightMult = 0.8f;
  public const float PistolDispersionTorqueMult = 1.5f;
  public const float CamWeightMult = 5f;
  public const float CamTorqueMult = 0.2f;
  public const float AngleTorqueMult = 0.39f;
  public const float PistolAngleTorqueMult = 0.3f;
  public const float DampingWeightMult = 0.04f;
  public const float DampingTorqueMult = 0.055f;
  public const float DampingMin = 0.2f;
  public const float DampingMax = 0.9f;
  public const float DampingPistolMin = 0.2f;
  public const float DampingPistolMax = 0.9f;
  public const float HandDampingWeightMult = 0.07f;
  public const float HandDampingTorqueMult = 0.04f;
  public const float HandDampingMin = 0.2f;
  public const float HandDampingMax = 0.9f;
  public const float HandDampingPistolMin = 0.2f;
  public const float HandDampingPistolMax = 0.9f;
  public const float MagWeightMult = 16.5f;

  public static void CalcPlayerWeightStats(Player player)
  {
    player.Inventory.UpdateTotalWeight((EventArgs) null);
    float num1 = GClass828<float>.op_Implicit(player.Inventory.TotalWeight);
    Weapon weapon = !Object.op_Inequality((Object) player?.HandsController, (Object) null) || player?.HandsController?.Item == null ? (Weapon) null : player.HandsController.Item as Weapon;
    float num2 = weapon != null ? ((Item) weapon).TotalWeight : 0.0f;
    PlayerState.TotalModifiedWeightMinusWeapon = num1 - num2;
    PlayerState.TotalMousePenalty = (float) (-(double) num1 / 10.0);
    PlayerState.TotalModifiedWeight = num1;
    if (PluginConfig.EnableGeneralLogging.Value)
      Utils.Logger.LogWarning((object) $"Total Mouse Penality {PlayerState.TotalMousePenalty}, Total Modified Weight Minus Weap {PlayerState.TotalModifiedWeightMinusWeapon}, Total Modified Weight {PlayerState.TotalModifiedWeight},");
    if (!PluginConfig.EnableMouseSensPenalty.Value)
      return;
    player.RemoveMouseSensitivityModifier((Player.EMouseSensitivityModifier) 0);
    if ((double) PlayerState.TotalMousePenalty < 0.0)
      player.AddMouseSensitivityModifier((Player.EMouseSensitivityModifier) 0, PlayerState.TotalMousePenalty / 100f);
  }

  public static void CalcSightAccuracy(Mod currentAimingMod, WeaponMod aimingModStats)
  {
    float num1 = 0.0f;
    int num2 = 0;
    if (currentAimingMod != null)
    {
      if (aimingModStats.ModType == "sight")
        num1 += (float) currentAimingMod.Accuracy / 100f;
      foreach (Item allParentItem in GClass3176.GetAllParentItems((Item) currentAimingMod, false))
      {
        if (allParentItem is Mod)
        {
          if (!(TemplateStats.GetDataObj<WeaponMod>(TemplateStats.WeaponModStats, MongoID.op_Implicit(allParentItem.TemplateId)).ModType != "mount"))
          {
            Mod mod = allParentItem as Mod;
            num1 += (float) mod.Accuracy / 100f;
          }
          else
            continue;
        }
        ++num2;
        if (num2 >= 5 || !(allParentItem is Mod))
          break;
      }
    }
    WeaponStats.ScopeAccuracyFactor = num1;
  }

  public static void UpdateAimParameters(
    Player.FirearmController firearmController,
    ProceduralWeaponAnimation pwa)
  {
    Weapon weapon = firearmController.Weapon;
    SkillManager.GClass2017 gclass2017 = (SkillManager.GClass2017) AccessTools.Field(typeof (ProceduralWeaponAnimation), "_buffInfo").GetValue((object) pwa);
    Player.ValueBlender valueBlender = (Player.ValueBlender) AccessTools.Field(typeof (ProceduralWeaponAnimation), "_aimSwayBlender").GetValue((object) pwa);
    float num1 = 0.0f;
    if (weapon.WeapClass != "pistol")
    {
      float num2 = GearController.FSIsActive || GearController.NVGIsActive || GearController.HasGasMask ? 1.45f : 1f;
      float num3 = StatCalc.ProceduralIntensityFactorCalc(((Item) weapon).TotalWeight, 4f);
      num1 = Mathf.InverseLerp(1f, 180f, (float) ((double) WeaponStats.ErgoFactor * (double) num3 * (1.0 + -(double) WeaponStats.Balance / 100.0) * (1.0 - (double) WeaponStats.PureErgoDelta) * (double) num2 * (1.0 - (double) PlayerState.StrengthSkillAimBuff * 1.5) * (1.0 + (1.0 - (double) PlayerState.GearErgoPenalty) * 1.5)));
    }
    AccessTools.Field(typeof (ProceduralWeaponAnimation), "_aimSwayStrength").SetValue((object) pwa, (object) num1);
    float num4 = Mathf.InverseLerp(1f, 80f, WeaponStats.TotalErgo * PlayerState.GearErgoPenalty) * 1.3f;
    float num5 = Mathf.Clamp(num4 * (float) (1.0 + (double) gclass2017.AimSpeed * 0.5), 0.35f, 1.5f);
    valueBlender.Speed = (float) ((double) pwa.SwayFalloff * (double) num5 * 4.3499999046325684);
    pwa.UpdateSwayFactors();
    float num6 = WeaponStats.IsPistol ? num5 * 1.35f : num5;
    WeaponStats.SightlessAimSpeed = num6;
    WeaponStats.ErgoStanceSpeed = (float) ((double) num4 * (1.0 + (double) gclass2017.AimSpeed * 0.5) * (WeaponStats.IsPistol ? 1.3999999761581421 : 1.0));
    AccessTools.Field(typeof (ProceduralWeaponAnimation), "_aimingSpeed").SetValue((object) pwa, (object) num6);
    if (!PluginConfig.EnablePWALogging.Value)
      return;
    Utils.Logger.LogWarning((object) "========UpdateWeaponVariables=======");
    Utils.Logger.LogWarning((object) ("total ergo = " + WeaponStats.TotalErgo.ToString()));
    Utils.Logger.LogWarning((object) ("aimSpeed = " + num6.ToString()));
    Utils.Logger.LogWarning((object) ("base aimSpeed = " + num4.ToString()));
    Utils.Logger.LogWarning((object) ("swayStrength = " + num1.ToString()));
    Utils.Logger.LogWarning((object) ("total ergofactor = " + (WeaponStats.ErgoFactor * (float) (1.0 - (double) PlayerState.StrengthSkillAimBuff * 1.5)).ToString()));
    Utils.Logger.LogWarning((object) ("gear ergo factor = " + PlayerState.GearErgoPenalty.ToString()));
  }

  public static float ErgoWeightCalc(
    float totalWeight,
    float ergoDelta,
    float totalTorque,
    string weapClass)
  {
    if (weapClass == "pistol")
    {
      totalTorque = (double) totalTorque < 0.0 ? totalTorque * 2f : totalTorque;
      float num = (float) (1.0 + ((double) totalTorque > 14.0 ? (double) totalTorque / 100.0 : (double) totalTorque / -100.0));
      return Mathf.Clamp((float) (Math.Pow((double) Math.Max(1f, Math.Max(1f, totalWeight * (1f - ergoDelta)) * num) * 3.2000000476837158, 3.2000000476837158) + 1.0) / 10f, 2.5f, 80f);
    }
    totalTorque = (double) totalTorque < 0.0 ? totalTorque * 3f : ((double) totalTorque > 10.0 ? totalTorque / 2f : ((double) totalTorque > 10.0 || (double) totalTorque < 0.0 ? totalTorque : totalTorque * 1.5f));
    float num1 = (float) (1.0 + (double) totalTorque / -100.0);
    return Mathf.Clamp((float) (Math.Pow((double) Math.Max(1f, Math.Max(1f, totalWeight * (1f - ergoDelta)) * num1) * 2.0999999046325684, 3.7000000476837158) + 1.0) / 750f, 2.5f, 80f);
  }

  public static float ProceduralIntensityFactorCalc(float weapWeight, float idealWeapWeight)
  {
    return (float) (((double) weapWeight - (double) idealWeapWeight) / (double) idealWeapWeight + 1.0);
  }

  public static void SpeedStatCalc(
    Weapon weap,
    Gun weaponStats,
    float ergoWeight,
    float ergonomicWeightLessMag,
    float chamberSpeedMod,
    float reloadSpeedMod,
    ref float totalReloadSpeedLessMag,
    ref float totalChamberSpeed,
    ref float totalAimMoveSpeedFactor,
    ref float totalFiringChamberSpeed,
    ref float totalChamberCheckSpeed,
    ref float totalFixSpeed)
  {
    float baseFixSpeed = weaponStats.BaseFixSpeed;
    float chamberCheckSpeed = weaponStats.BaseChamberCheckSpeed;
    float chamberSpeedMulti = weaponStats.BaseChamberSpeedMulti;
    float reloadSpeedMulti = weaponStats.BaseReloadSpeedMulti;
    float num1 = (float) (1.0 + -1.0 * (double) WeaponStats.PureRecoilDelta);
    float num2 = (float) (1.0 - (double) ergoWeight / 100.0);
    float num3 = (float) (1.0 - (double) ergonomicWeightLessMag / 100.0);
    chamberSpeedMod = (float) (1.0 + (double) chamberSpeedMod / 100.0);
    reloadSpeedMod = (float) (1.0 + (double) reloadSpeedMod / 100.0);
    totalFixSpeed = Mathf.Clamp(baseFixSpeed * num2 * chamberSpeedMod, weaponStats.MinChamberSpeed, weaponStats.MaxChamberSpeed);
    totalFiringChamberSpeed = Mathf.Clamp(chamberSpeedMulti * num2 * chamberSpeedMod * num1, weaponStats.MinChamberSpeed, weaponStats.MaxChamberSpeed);
    totalChamberSpeed = Mathf.Clamp(chamberSpeedMulti * num2 * chamberSpeedMod, weaponStats.MinChamberSpeed, weaponStats.MaxChamberSpeed);
    totalChamberCheckSpeed = Mathf.Clamp(chamberCheckSpeed * num2 * chamberSpeedMod, weaponStats.MinChamberSpeed, weaponStats.MaxChamberSpeed);
    totalReloadSpeedLessMag = Mathf.Clamp(reloadSpeedMulti * num3 * reloadSpeedMod, weaponStats.MinReloadSpeed, weaponStats.MaxReloadSpeed);
    totalAimMoveSpeedFactor = Mathf.Max((float) (1.0 - (double) ergoWeight / 150.0), 0.5f);
  }

  public static void WeaponStatCalc(
    Weapon weap,
    Gun weaponStats,
    float currentTorque,
    ref float totalTorque,
    float currentErgo,
    float currentVRecoil,
    float currentHRecoil,
    float currentDispersion,
    float currentCamRecoil,
    float currentRecoilAngle,
    float baseErgo,
    float baseVRecoil,
    float baseHRecoil,
    ref float totalErgo,
    ref float totalVRecoil,
    ref float totalHRecoil,
    ref float totalDispersion,
    ref float totalCamRecoil,
    ref float totalRecoilAngle,
    ref float totalRecoilDamping,
    ref float totalRecoilHandDamping,
    ref float totalErgoDelta,
    ref float totalVRecoilDelta,
    ref float totalHRecoilDelta,
    ref float recoilDamping,
    ref float recoilHandDamping,
    float currentCOI,
    bool hasShoulderContact,
    ref float totalCOI,
    ref float totalCOIDelta,
    float baseCOI,
    float totalPureErgo,
    ref float totalPureErgoDelta,
    ref float totalErgoLessMag,
    float currentErgoLessMag,
    bool isDisplayDelta)
  {
    float num1 = (double) currentTorque > 3.0 ? currentTorque * -1f : currentTorque;
    float num2;
    float num3;
    float statFactor1;
    float num4;
    float statFactor2;
    float num5;
    float statFactor3;
    float num6;
    float statFactor4;
    if (weap.WeapClass == "pistol")
    {
      num2 = 0.3f;
      num3 = 1f;
      statFactor1 = 12.6f;
      num4 = 1.5f;
      statFactor2 = 1.5f;
      num5 = 0.8f;
      statFactor3 = 3.5f;
      num6 = 1.5f;
      statFactor4 = 0.8f;
    }
    else
    {
      num2 = 0.39f;
      num3 = 0.8f;
      statFactor1 = 13.5f;
      num4 = 0.87f;
      statFactor2 = 2f;
      num5 = 0.4f;
      statFactor3 = 3.55f;
      num6 = 1.2f;
      statFactor4 = 1.5f;
    }
    float weight1 = ((Item) weap).Weight;
    float weight2 = StatCalc.FactoredWeight(weight1);
    float num7 = StatCalc.TorqueCalc(weaponStats.BaseTorque, weight2, weap.WeapClass);
    float num8 = StatCalc.WeightStatCalc(statFactor1, weap.IsBeltMachineGun || (double) weight1 >= 10.0 ? weight1 * 0.5f : weight1) / 100f;
    float num9 = StatCalc.WeightStatCalc(statFactor2, weight1) / 100f;
    float num10 = StatCalc.WeightStatCalc(statFactor3, weight1) / 100f;
    float num11 = StatCalc.WeightStatCalc(statFactor4, weight1) / 100f;
    float num12 = StatCalc.WeightStatCalc(5f, weight1) / 100f;
    float totalWeight = ((Item) weap).TotalWeight;
    float num13 = StatCalc.WeightStatCalc(0.04f, totalWeight) / 100f;
    float num14 = StatCalc.WeightStatCalc(0.07f, totalWeight) / 100f;
    totalTorque = num7 + currentTorque;
    float num15 = num7 + num1;
    float num16 = WeaponStats.IsPistol ? num15 / 100f : totalTorque / 100f;
    float num17 = totalTorque / 100f;
    float num18 = totalTorque / -100f;
    totalErgoLessMag = currentErgoLessMag + currentErgoLessMag * (num8 + num16 * num3);
    totalErgo = currentErgo + currentErgo * (num8 + num16 * num3);
    totalVRecoil = currentVRecoil + currentVRecoil * (num9 + num17 * num4);
    totalHRecoil = currentHRecoil + currentHRecoil * (num10 + num18 * num5);
    totalCamRecoil = currentCamRecoil + currentCamRecoil * (num12 + num18 * 0.2f);
    totalDispersion = currentDispersion + currentDispersion * (num11 + num17 * num6);
    totalRecoilAngle = currentRecoilAngle + currentRecoilAngle * (num17 * num2);
    totalCOI = currentCOI + currentCOI * (float) (-(double) weaponStats.WeapAccuracy / 100.0);
    if (!hasShoulderContact && !WeaponStats.IsPistol)
    {
      totalPureErgo *= 0.85f;
      totalErgo *= 0.85f;
      totalErgoLessMag *= 0.85f;
      totalVRecoil *= 1.65f;
      totalHRecoil *= 1.15f;
      totalCamRecoil *= 0.6f;
      totalDispersion *= 1.75f;
      totalRecoilAngle *= 1.15f;
    }
    totalCOIDelta = (totalCOI - baseCOI) / baseCOI;
    totalErgoDelta = (float) (((double) totalErgo - 80.0) / 80.0);
    totalPureErgoDelta = (float) (((double) totalPureErgo - 80.0) / 80.0);
    totalVRecoilDelta = (totalVRecoil - baseVRecoil) / baseVRecoil;
    totalHRecoilDelta = (totalHRecoil - baseHRecoil) / baseHRecoil;
    if (isDisplayDelta)
      return;
    if (weap.WeapClass == "pistol")
    {
      totalRecoilDamping = Mathf.Clamp(recoilDamping + recoilDamping * (num13 + num17 * 0.055f), 0.2f, 0.9f);
      totalRecoilHandDamping = Mathf.Clamp(recoilHandDamping + recoilHandDamping * (num14 + num18 * 0.04f), 0.2f, 0.9f);
    }
    else
    {
      totalRecoilDamping = Mathf.Clamp(recoilDamping + recoilDamping * (num13 + num17 * 0.055f), 0.2f, 0.9f);
      totalRecoilHandDamping = Mathf.Clamp(recoilHandDamping + recoilHandDamping * (num14 + num18 * 0.04f), 0.2f, 0.9f);
    }
  }

  public static void ModStatCalc(
    Mod mod,
    bool isDisplayDelta,
    bool isChonker,
    float modWeight,
    ref float currentTorque,
    string position,
    float modWeightFactored,
    float modAutoROF,
    ref float currentAutoROF,
    float modSemiROF,
    ref float currentSemiROF,
    float modCamRecoil,
    ref float currentCamRecoil,
    float modDispersion,
    ref float currentDispersion,
    float modAngle,
    ref float currentRecoilAngle,
    float modAccuracy,
    ref float currentCOI,
    float modErgo,
    ref float currentErgo,
    float modVRecoil,
    ref float currentVRecoil,
    float modHRecoil,
    ref float currentHRecoil,
    ref float pureErgo,
    float modShotDisp,
    ref float currentShotDisp,
    ref float currentMalfChance,
    float modMalfChance,
    ref float pureRecoil,
    ref float currentConv,
    float modConv,
    ref float currentCamReturnSpeed)
  {
    float num1 = StatCalc.WeightStatCalc(13.5f, isChonker ? modWeight * 0.5f : modWeight) / 100f;
    float num2 = StatCalc.WeightStatCalc(2f, modWeight) / 100f;
    float num3 = StatCalc.WeightStatCalc(3.55f, modWeight) / 100f;
    float num4 = StatCalc.WeightStatCalc(1.5f, modWeight) / 100f;
    float num5 = StatCalc.WeightStatCalc(5f, modWeight) / 100f;
    currentTorque += StatCalc.GetTorque(position, modWeightFactored);
    currentErgo += currentErgo * (modErgo / 100f + num1);
    currentVRecoil += currentVRecoil * (modVRecoil / 100f + num2);
    currentHRecoil += currentHRecoil * (modHRecoil / 100f + num3);
    currentCamRecoil += currentCamRecoil * (modCamRecoil / 100f + num5);
    currentDispersion += currentDispersion * (modDispersion / 100f + num4);
    currentRecoilAngle += currentRecoilAngle * (modAngle / 100f);
    currentCOI += currentCOI * (float) (-(double) modAccuracy / 100.0);
    currentAutoROF += currentAutoROF * (modAutoROF / 100f);
    currentSemiROF += currentSemiROF * (modSemiROF / 100f);
    pureErgo += pureErgo * (modErgo / 100f);
    currentCamReturnSpeed = Mathf.Clamp(currentCamReturnSpeed + currentCamReturnSpeed * (modCamRecoil / 100f), 0.1f, 0.5f);
    currentConv += currentConv * (modConv / 100f);
    if (isDisplayDelta)
      return;
    currentShotDisp += currentShotDisp * (float) (-1.0 * (double) modShotDisp / 100.0);
    currentMalfChance += currentMalfChance * (modMalfChance / 100f);
    if (Utils.IsSilencer(mod))
      pureRecoil += (float) ((double) pureRecoil * ((double) modHRecoil * 0.5 / 100.0) + (double) modCamRecoil * 0.5 / 100.0);
    else
      pureRecoil += pureRecoil * (float) ((double) modVRecoil / 100.0 + (double) modHRecoil / 100.0 + (double) modCamRecoil / 100.0 + -(double) modConv / 100.0 + (double) modDispersion / 100.0);
  }

  public static void ModConditionalStatCalc(
    Weapon weap,
    Gun weaponStats,
    Mod mod,
    WeaponMod weaponModStats,
    bool folded,
    string weapType,
    string weapOpType,
    ref bool hasShoulderContact,
    ref float modAutoROF,
    ref float modSemiROF,
    ref bool stockAllowsFSADS,
    ref float modVRecoil,
    ref float modHRecoil,
    ref float modCamRecoil,
    ref float modAngle,
    ref float modDispersion,
    ref float modErgo,
    ref float modAccuracy,
    ref string modType,
    ref string position,
    ref float modChamber,
    ref float modLoudness,
    ref float modMalfChance,
    ref float modDuraBurn,
    ref float modConv,
    ref float modFlash,
    ref float modStability,
    ref float modHandling,
    ref float modAimSpeed)
  {
    Mod mod1 = (Mod) null;
    WeaponStats.StockPosition = 0;
    if (((Item) mod)?.Parent?.Container?.ParentItem != null)
      mod1 = ((Item) mod).Parent.Container.ParentItem as Mod;
    if (Utils.IsStock(mod))
    {
      if (folded)
      {
        modDuraBurn = 1f;
        modMalfChance = 0.0f;
        modAutoROF = 0.0f;
        modSemiROF = 0.0f;
        modConv = 0.0f;
        modVRecoil = 0.0f;
        modHRecoil = 0.0f;
        modCamRecoil = 0.0f;
        modAngle = 0.0f;
        modDispersion = 0.0f;
        modErgo = 0.0f;
        modAccuracy = 0.0f;
        modStability = 0.0f;
        modAimSpeed = 0.0f;
        modHandling = 0.0f;
        modType = "folded_stock";
        position = "neutral";
        hasShoulderContact = false;
        return;
      }
      if (!folded)
      {
        if (modType.StartsWith("Stock") || modType == "buffer_stock")
        {
          hasShoulderContact = mod.Template.HasShoulderContact;
          stockAllowsFSADS = weaponModStats.StockAllowADS;
        }
        if (weapOpType != "buffer")
        {
          if (modType == "buffer")
          {
            modConv = 0.0f;
            modVRecoil = 0.0f;
            modHRecoil = 0.0f;
            modDispersion = 0.0f;
            modCamRecoil = 0.0f;
            modAutoROF = 0.0f;
            modSemiROF = 0.0f;
            modDuraBurn = 1f;
            modMalfChance = 0.0f;
            return;
          }
          if (modType == "buffer_stock")
          {
            modConv = 0.0f;
            modAutoROF = 0.0f;
            modSemiROF = 0.0f;
            modDuraBurn = 1f;
            modMalfChance = 0.0f;
            return;
          }
        }
        StatCalc.StockPositionChecker(mod, ref modVRecoil, ref modHRecoil, ref modDispersion, ref modCamRecoil, ref modErgo, ref modHandling, ref modStability);
        if (modType == "buffer_adapter" || modType == "stock_adapter")
        {
          int num1;
          if (mod != null)
          {
            int? length = ((CompoundItem) mod).Slots?.Length;
            int num2 = 1;
            if (length.GetValueOrDefault() > num2 & length.HasValue)
            {
              num1 = ((CompoundItem) mod)?.Slots[1]?.ContainedItem != null ? 1 : 0;
              goto label_18;
            }
          }
          num1 = 0;
label_18:
          if (num1 != 0)
          {
            modVRecoil += -1f;
            modHRecoil += -2f;
            modDispersion += -1f;
            modErgo += 2f;
            modChamber += 20f;
          }
          if (((CompoundItem) mod)?.Slots[0]?.ContainedItem != null)
          {
            Mod containedItem = ((CompoundItem) mod).Slots[0].ContainedItem as Mod;
            if (weaponModStats.ModType != "buffer")
              return;
            for (int index = 0; index < ((CompoundItem) containedItem).Slots.Length; ++index)
            {
              if (((CompoundItem) containedItem).Slots[index].ContainedItem != null)
                return;
            }
          }
          modVRecoil = 0.0f;
          modHRecoil = 0.0f;
          modDispersion = 0.0f;
          modCamRecoil = 0.0f;
          modErgo = 0.0f;
          modStability = 0.0f;
          modHandling = 0.0f;
          return;
        }
        if (modType == "hydraulic_buffer")
        {
          if (weaponStats.IsManuallyOperated)
            modMalfChance = 0.0f;
          if (!(weap.WeapClass != "shotgun") && !(weap.WeapClass != "sniperRifle") && !(weap.WeapClass != "assaultCarbine") && !(weapOpType == "buffer"))
            return;
          modConv = 0.0f;
          modVRecoil = 0.0f;
          modHRecoil = 0.0f;
          modDispersion = 0.0f;
          modCamRecoil = 0.0f;
          return;
        }
      }
    }
    if (modType == "mount" || modType == "sight")
      modAccuracy = 0.0f;
    if ((modType == "booster" || Utils.IsSilencer(mod)) && (weapType == "short_AK" || mod1 != null && weaponModStats.ModType == "short_barrel"))
    {
      if (Utils.IsSilencer(mod))
      {
        modAutoROF *= 2f;
        modSemiROF *= 2f;
        modMalfChance *= 1.2f;
        modDuraBurn = (float) (((double) modDuraBurn - 1.0) * 1.2000000476837158 + 1.0);
      }
      else
      {
        modAutoROF *= 2.5f;
        modSemiROF *= 2.5f;
        modMalfChance *= 2f;
        modDuraBurn = (float) (((double) modDuraBurn - 1.0) * 1.75 + 1.0);
      }
    }
    else if (modType == "foregrip_adapter" && ((CompoundItem) mod).Slots[0].ContainedItem != null)
    {
      modErgo = 0.0f;
    }
    else
    {
      if (Utils.IsSilencer(mod) || Utils.IsFlashHider(mod) || Utils.IsMuzzleCombo(mod))
      {
        if (weaponStats.IsManuallyOperated)
        {
          modMalfChance = 0.0f;
          modDuraBurn = (float) (((double) modDuraBurn - 1.0) * 0.25 + 1.0);
        }
        if (weaponStats.WeapType == "DI")
          modDuraBurn *= 1.25f;
      }
      if (modType == "muzzle_supp_adapter" && ((CompoundItem) mod).Slots != null && ((IEnumerable<Slot>) ((CompoundItem) mod).Slots).Count<Slot>() > 0 && ((CompoundItem) mod).Slots[0].ContainedItem != null)
      {
        if (!Utils.IsSilencer(((CompoundItem) mod).Slots[0].ContainedItem as Mod))
          return;
        modConv *= 0.1f;
        modVRecoil *= 0.1f;
        modHRecoil *= 0.1f;
        modCamRecoil *= 0.1f;
        modDispersion *= 0.1f;
        modAngle *= 0.1f;
        modLoudness = 0.0f;
        modFlash = 0.0f;
      }
      else if (modType == "shot_pump_grip_adapt" && ((CompoundItem) mod).Slots[0].ContainedItem != null)
      {
        Mod containedItem = ((CompoundItem) mod).Slots[0].ContainedItem as Mod;
        if (!Utils.IsForegrip(containedItem) && (!(weaponModStats.ModType == "foregrip_adapter") || ((CompoundItem) containedItem).Slots[0].ContainedItem == null))
          return;
        modChamber += 20f;
      }
      else if (modType == "grip_stock_adapter")
      {
        if (((CompoundItem) mod).Slots[0].ContainedItem != null)
        {
          Mod containedItem = ((CompoundItem) mod).Slots[0].ContainedItem as Mod;
          if (weaponModStats.ModType.StartsWith("Stock") || (double) ((CompoundItem) containedItem).Slots.Length > 0.0 && ((CompoundItem) containedItem).Slots[0].ContainedItem != null)
            return;
        }
        modVRecoil = 0.0f;
        modHRecoil = 0.0f;
        modDispersion = 0.0f;
        modCamRecoil = 0.0f;
        modStability = 0.0f;
        modHandling = 0.0f;
      }
      else if (modType == "sig_taper_brake")
      {
        if (mod1 == null || ((Item) mod).Parent.Container == null || ((CompoundItem) mod1).Slots[1].ContainedItem == null)
          return;
        modConv *= 0.1f;
        modVRecoil *= 0.1f;
        modHRecoil *= 0.1f;
        modCamRecoil *= 0.1f;
        modDispersion *= 0.1f;
        modAngle *= 0.1f;
        modLoudness = 0.0f;
        modFlash = 0.0f;
      }
      else
      {
        if (!(modType == "barrel_2slot") || mod1 == null || ((Item) mod).Parent.Container == null || ((CompoundItem) mod1).Slots[1].ContainedItem == null)
          return;
        modConv *= 0.1f;
        modVRecoil *= 0.1f;
        modHRecoil *= 0.1f;
        modCamRecoil *= 0.1f;
        modDispersion *= 0.1f;
        modAngle *= 0.1f;
        modLoudness = 0.0f;
        modFlash = 0.0f;
      }
    }
  }

  public static string GetModPosition(Mod mod, string weapType, string opType, string modType)
  {
    int num;
    switch (modType)
    {
      case "StockN":
        return "neutral";
      case "StockF":
        return "front";
      case "StockR":
        return "rearHalf";
      case "counterWeight":
        return "frontFar";
      case "shotTube":
        num = 1;
        break;
      default:
        if (!Utils.IsTacticalCombo(mod))
        {
          num = Utils.IsFlashlight(mod) ? 1 : 0;
          break;
        }
        goto case "shotTube";
    }
    if (num != 0)
      return "frontHalf";
    if (weapType == "pistol" || weapType == "bullpup")
    {
      if (weapType == "pistol" && Utils.IsMount(mod))
        return "front";
      if (Utils.IsStock(mod) || opType != "revolver" && Utils.IsMagazine(mod))
        return "rear";
      return Utils.IsUBGL(mod) || Utils.IsHandguard(mod) || Utils.IsGasblock(mod) || Utils.IsFlashHider(mod) || Utils.IsForegrip(mod) || Utils.IsMuzzleCombo(mod) || Utils.IsSilencer(mod) || Utils.IsBipod(mod) || Utils.IsBarrel(mod) ? "front" : "neutral";
    }
    if (opType == "p90" || opType.Contains("tubefed") || opType == "magForward")
    {
      if (Utils.IsStock(mod))
        return "rear";
      return Utils.IsUBGL(mod) || Utils.IsMagazine(mod) || Utils.IsHandguard(mod) || Utils.IsGasblock(mod) || Utils.IsFlashHider(mod) || Utils.IsForegrip(mod) || Utils.IsMuzzleCombo(mod) || Utils.IsSilencer(mod) || Utils.IsTacticalCombo(mod) || Utils.IsFlashlight(mod) || Utils.IsBipod(mod) || Utils.IsBarrel(mod) ? "front" : "neutral";
    }
    if (Utils.IsStock(mod) || Utils.IsPistolGrip(mod))
      return "rear";
    return Utils.IsUBGL(mod) || Utils.IsTacticalCombo(mod) || Utils.IsFlashlight(mod) || Utils.IsBipod(mod) || Utils.IsHandguard(mod) || Utils.IsGasblock(mod) || Utils.IsForegrip(mod) || Utils.IsMuzzleCombo(mod) || Utils.IsFlashHider(mod) || Utils.IsSilencer(mod) || Utils.IsBarrel(mod) ? "front" : "neutral";
  }

  private static void StockPositionChecker(
    Mod mod,
    ref float modVRecoil,
    ref float modHRecoil,
    ref float modDispersion,
    ref float modCamRecoil,
    ref float modErgo,
    ref float modHandling,
    ref float modStability)
  {
    if (((Item) mod).Parent.Container == null)
      return;
    string modType = TemplateStats.GetDataObj<WeaponMod>(TemplateStats.WeaponModStats, MongoID.op_Implicit(((Item) mod).Parent.Container.ParentItem.TemplateId)).ModType;
    if (modType == "buffer" || modType == "buffer_adapter")
    {
      Mod parentItem = ((Item) mod).Parent.Container.ParentItem as Mod;
      for (int position = 0; position < ((CompoundItem) parentItem).Slots.Length; ++position)
      {
        if (((CompoundItem) parentItem).Slots[position].ContainedItem != null)
        {
          StatCalc.BufferSlotModifier(position, ref modVRecoil, ref modHRecoil, ref modDispersion, ref modCamRecoil, ref modErgo, ref modHandling, ref modStability);
          break;
        }
      }
    }
  }

  public static float WeightStatCalc(float statFactor, float itemWeight)
  {
    return itemWeight * -statFactor;
  }

  public static float FactoredWeight(float modWeight)
  {
    return Mathf.Clamp((float) Math.Pow((double) modWeight * 1.5, 1.1000000238418579) / 1.1f, 1f / 1000f, 5f);
  }

  private static float TorqueCalc(float distance, float weight, string weapClass)
  {
    if (weapClass == "pistol")
      distance *= 2.5f;
    return (distance - 0.0f) * weight;
  }

  public static float GetTorque(string position, float weight)
  {
    float torque;
    switch (position)
    {
      case "front":
        torque = StatCalc.TorqueCalc(-10f, weight, WeaponStats._WeapClass);
        break;
      case "rear":
        torque = StatCalc.TorqueCalc(10f, weight, WeaponStats._WeapClass);
        break;
      case "rearHalf":
        torque = StatCalc.TorqueCalc(2.5f, weight, WeaponStats._WeapClass);
        break;
      case "frontFar":
        torque = StatCalc.TorqueCalc(-15f, weight, WeaponStats._WeapClass);
        break;
      case "frontHalf":
        torque = StatCalc.TorqueCalc(-5f, weight, WeaponStats._WeapClass);
        break;
      default:
        torque = 0.0f;
        break;
    }
    return torque;
  }

  private static void BufferSlotModifier(
    int position,
    ref float modVRecoil,
    ref float modHRecoil,
    ref float modDispersion,
    ref float modCamRecoil,
    ref float modErgo,
    ref float modHandling,
    ref float modStability)
  {
    switch (position)
    {
      case 0:
        modVRecoil *= 0.5f;
        modHRecoil *= 0.5f;
        modDispersion *= 0.5f;
        modCamRecoil *= 0.5f;
        modErgo *= 1.5f;
        WeaponStats.StockPosition = -1;
        break;
      case 1:
        modVRecoil *= 0.75f;
        modHRecoil *= 0.75f;
        modDispersion *= 0.75f;
        modCamRecoil *= 0.75f;
        modErgo *= 1.25f;
        modHandling += 0.1f;
        modStability += 0.05f;
        WeaponStats.StockPosition = 0;
        break;
      case 2:
        modVRecoil *= 1f;
        modHRecoil *= 1f;
        modDispersion *= 1f;
        modCamRecoil *= 1f;
        modErgo *= 1f;
        modHandling += 0.075f;
        modStability += 0.075f;
        WeaponStats.StockPosition = 1;
        break;
      case 3:
        modVRecoil *= 1.25f;
        modHRecoil *= 1.25f;
        modDispersion *= 1.25f;
        modCamRecoil *= 1.25f;
        modErgo *= 0.75f;
        modHandling += 0.05f;
        modStability += 0.1f;
        WeaponStats.StockPosition = 2;
        break;
      default:
        modVRecoil *= 1f;
        modHRecoil *= 1f;
        modDispersion *= 1f;
        modCamRecoil *= 1f;
        modErgo *= 1f;
        WeaponStats.StockPosition = 0;
        break;
    }
  }

  public static float CaliberSmoke(string caliber)
  {
    switch (caliber)
    {
      case "1143x23ACP":
        return 0.45f;
      case "127x108":
        return 1.75f;
      case "127x33":
        return 1.2f;
      case "127x55":
        return 1.2f;
      case "127x99":
        return 1.65f;
      case "12g":
        return 1f;
      case "20g":
        return 0.8f;
      case "23x75":
        return 1.2f;
      case "30x29":
        return 1f;
      case "366TKM":
        return 0.9f;
      case "40x46":
        return 1f;
      case "40x53":
        return 1f;
      case "46x30":
        return 0.35f;
      case "545x39":
        return 0.5f;
      case "556x45NATO":
        return 0.5f;
      case "57x28":
        return 0.4f;
      case "68x51":
        return 0.85f;
      case "762x25TT":
        return 0.5f;
      case "762x35":
        return 0.5f;
      case "762x39":
        return 0.65f;
      case "762x51":
        return 0.8f;
      case "762x54R":
        return 0.9f;
      case "86x70":
        return 1.35f;
      case "9x18PM":
        return 0.4f;
      case "9x19PARA":
        return 0.425f;
      case "9x21":
        return 0.45f;
      case "9x33R":
        return 1f;
      case "9x39":
        return 0.45f;
      default:
        return 0.5f;
    }
  }

  public static float CaliberFlame(string caliber)
  {
    switch (caliber)
    {
      case "1143x23ACP":
        return 0.05f;
      case "127x108":
        return 0.25f;
      case "127x33":
        return 0.3f;
      case "127x55":
        return 0.13f;
      case "127x99":
        return 0.24f;
      case "12g":
        return 0.12f;
      case "20g":
        return 0.1f;
      case "23x75":
        return 0.14f;
      case "30x29":
        return 0.06f;
      case "366TKM":
        return 0.1f;
      case "40x46":
        return 0.06f;
      case "40x53":
        return 0.06f;
      case "46x30":
        return 0.055f;
      case "545x39":
        return 0.09f;
      case "556x45NATO":
        return 0.09f;
      case "57x28":
        return 0.065f;
      case "68x51":
        return 0.12f;
      case "762x25TT":
        return 0.065f;
      case "762x35":
        return 0.07f;
      case "762x39":
        return 0.1f;
      case "762x51":
        return 0.15f;
      case "762x54R":
        return 0.14f;
      case "86x70":
        return 0.19f;
      case "9x18PM":
        return 0.05f;
      case "9x19PARA":
        return 0.06f;
      case "9x21":
        return 0.055f;
      case "9x33R":
        return 0.2f;
      case "9x39":
        return 0.065f;
      default:
        return 0.05f;
    }
  }

  public static int CaliberSparks(string caliber)
  {
    switch (caliber)
    {
      case "1143x23ACP":
        return 1;
      case "127x108":
        return 8;
      case "127x33":
        return 15;
      case "127x55":
        return 4;
      case "127x99":
        return 7;
      case "12g":
        return 2;
      case "20g":
        return 1;
      case "23x75":
        return 3;
      case "30x29":
        return 0;
      case "366TKM":
        return 2;
      case "40x46":
        return 0;
      case "40x53":
        return 0;
      case "46x30":
        return 0;
      case "545x39":
        return 3;
      case "556x45NATO":
        return 3;
      case "57x28":
        return 1;
      case "68x51":
        return 4;
      case "762x25TT":
        return 3;
      case "762x35":
        return 0;
      case "762x39":
        return 4;
      case "762x51":
        return 5;
      case "762x54R":
        return 5;
      case "86x70":
        return 6;
      case "9x18PM":
        return 0;
      case "9x19PARA":
        return 2;
      case "9x21":
        return 2;
      case "9x33R":
        return 10;
      case "9x39":
        return 0;
      default:
        return 0;
    }
  }

  public static int CaliberMuzzleFlash(string caliber)
  {
    switch (caliber)
    {
      case "1143x23ACP":
        return 12;
      case "127x108":
        return 40;
      case "127x33":
        return 40;
      case "127x55":
        return 25;
      case "127x99":
        return 37;
      case "12g":
        return 14;
      case "20g":
        return 12;
      case "23x75":
        return 19;
      case "30x29":
        return 8;
      case "366TKM":
        return 17;
      case "40x46":
        return 8;
      case "40x53":
        return 8;
      case "46x30":
        return 11;
      case "545x39":
        return 15;
      case "556x45NATO":
        return 15;
      case "57x28":
        return 12;
      case "68x51":
        return 22;
      case "762x25TT":
        return 14;
      case "762x35":
        return 12;
      case "762x39":
        return 16 /*0x10*/;
      case "762x51":
        return 23;
      case "762x54R":
        return 23;
      case "86x70":
        return 30;
      case "9x18PM":
        return 10;
      case "9x19PARA":
        return 13;
      case "9x21":
        return 13;
      case "9x33R":
        return 30;
      case "9x39":
        return 11;
      default:
        return 12;
    }
  }

  public static float CaliberLoudnessFactor(string caliber)
  {
    switch (caliber)
    {
      case "1143x23ACP":
        return 2.3f;
      case "127x108":
        return 7f;
      case "127x33":
        return 5f;
      case "127x55":
        return 3.8f;
      case "127x99":
        return 6.5f;
      case "12g":
        return 3f;
      case "20g":
        return 2.9f;
      case "23x75":
        return 3.35f;
      case "30x29":
        return 2.4f;
      case "366TKM":
        return 2.68f;
      case "40x46":
        return 2.5f;
      case "40x53":
        return 2.5f;
      case "46x30":
        return 2.3f;
      case "545x39":
        return 2.63f;
      case "556x45NATO":
        return 2.65f;
      case "57x28":
        return 3f;
      case "68x51":
        return 3f;
      case "762x25TT":
        return 2.55f;
      case "762x35":
        return 2f;
      case "762x39":
        return 2.6f;
      case "762x51":
        return 2.8f;
      case "762x54R":
        return 2.82f;
      case "86x70":
        return 5f;
      case "9x18PM":
        return 2.2f;
      case "9x19PARA":
        return 2.4f;
      case "9x21":
        return 2.35f;
      case "9x33R":
        return 4f;
      case "9x39":
        return 1.9f;
      default:
        return 1f;
    }
  }
}
