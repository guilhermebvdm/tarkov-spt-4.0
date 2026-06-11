// Decompiled with JetBrains decompiler
// Type: RealismMod.RecalcWeaponParametersPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using BepInEx.Logging;
using EFT;
using EFT.Animations;
using EFT.Animations.NewRecoil;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class RecalcWeaponParametersPatch : ModulePatch
{
  private static FieldInfo playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    RecalcWeaponParametersPatch.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    return (MethodBase) typeof (NewRecoilShotEffect).GetMethod("RecalculateRecoilParamsOnChangeWeapon", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(
    NewRecoilShotEffect __instance,
    WeaponTemplate template,
    BackendConfigSettingsClass.AimingConfiguration AimingConfig,
    Player.FirearmController firearmController,
    float recoilSuppressionX,
    float recoilSuppressionY,
    float recoilSuppressionFactor,
    float modsFactorRecoil)
  {
    Player player = (Player) RecalcWeaponParametersPatch.playerField.GetValue((object) firearmController);
    if (!Object.op_Inequality((Object) player, (Object) null) || !player.IsYourPlayer)
      return true;
    float ergonomicsDelta = firearmController.Weapon.ErgonomicsDelta;
    float num1 = WeaponStats.IsStockedPistol ? 0.75f : 1f;
    __instance._firearmController = firearmController;
    __instance.RecoilStableShotIndex = 1;
    __instance.HandRotationRecoil.RecoilReturnTrajectoryOffset = template.RecoilReturnPathOffsetHandRotation * PluginConfig.AfterRecoilRandomness.Value;
    __instance.HandRotationRecoil.StableAngleIncreaseStep = template.RecoilStableAngleIncreaseStep;
    __instance.HandRotationRecoil.AfterRecoilOffsetVerticalRange = Vector2.zero;
    __instance.HandRotationRecoil.AfterRecoilOffsetHorizontalRange = Vector2.zero;
    __instance.HandRotationRecoil.ProgressRecoilAngleOnStable = new Vector2(ShootController.FactoredTotalDispersion * PluginConfig.RecoilRandomness.Value, ShootController.FactoredTotalDispersion * PluginConfig.RecoilRandomness.Value);
    __instance.HandRotationRecoil.ReturnTrajectoryDumping = template.RecoilReturnPathDampingHandRotation;
    float num2 = WeaponStats.IsPistol ? PluginConfig.PistolRecoilDampingMulti.Value : PluginConfig.RiflRecoilDampingMulti.Value;
    ((RecoilProcessBase) __instance.HandRotationRecoilEffect).Damping = template.RecoilDampingHandRotation * num2;
    __instance.HandRotationRecoil.CategoryIntensityMultiplier = template.RecoilCategoryMultiplierHandRotation * PluginConfig.RecoilIntensity.Value * num1;
    float num3 = Mathf.Max(0.0f, (float) ((1.0 + (double) WeaponStats.VRecoilDelta) * (1.0 - (double) recoilSuppressionX - (double) recoilSuppressionY * (double) recoilSuppressionFactor)));
    float num4 = Mathf.Max(0.0f, (float) ((1.0 + (double) WeaponStats.HRecoilDelta) * (1.0 - (double) recoilSuppressionX - (double) recoilSuppressionY * (double) recoilSuppressionFactor)));
    __instance.BasicRecoilRotationStrengthRange = new Vector2(0.95f, 1.05f);
    __instance.BasicRecoilPositionStrengthRange = new Vector2(0.95f, 1.05f);
    __instance.BasicPlayerRecoilRotationStrength = Vector2.op_Multiply(__instance.BasicRecoilRotationStrengthRange, (template.RecoilForceUp * num3 + AimingConfig.RecoilVertBonus) * __instance.IncomingRotationStrengthMultiplier);
    __instance.BasicPlayerRecoilPositionStrength = Vector2.op_Multiply(__instance.BasicRecoilPositionStrengthRange, (template.RecoilForceBack * num4 + AimingConfig.RecoilBackBonus) * __instance.IncomingRotationStrengthMultiplier);
    float totalCamRecoil = WeaponStats.TotalCamRecoil;
    __instance.ShotRecoilProcessValues[3].IntensityMultiplicator = totalCamRecoil;
    __instance.ShotRecoilProcessValues[4].IntensityMultiplicator = -totalCamRecoil;
    ShootController.BaseTotalCamRecoil = totalCamRecoil;
    ShootController.BaseTotalVRecoil = (float) Math.Round((double) __instance.BasicPlayerRecoilRotationStrength.y, 3);
    ShootController.BaseTotalHRecoil = (float) Math.Round((double) __instance.BasicPlayerRecoilPositionStrength.y, 3);
    ShootController.BaseTotalConvergence = WeaponStats.TotalModdedConv * (WeaponStats.IsPistol ? PluginConfig.PistolConvergenceMulti.Value : PluginConfig.ConvergenceMulti.Value);
    ShootController.BaseTotalRecoilDamping = (float) Math.Round((double) WeaponStats.TotalRecoilDamping * (double) num2, 3);
    ShootController.BaseTotalHandDamping = (float) Math.Round((double) WeaponStats.TotalRecoilHandDamping, 3);
    WeaponStats.TotalWeaponWeight = ((Item) firearmController.Weapon).TotalWeight;
    WeaponStats.TotalWeaponLength = (float) ((Item) firearmController.Item).CalculateCellSize().X;
    __instance.HandRotationRecoil.MountVerticalRecoilMultiplier = template.MountVerticalRecoilMultiplier;
    __instance.HandRotationRecoil.MountHorizontalRecoilMultiplier = template.MountHorizontalRecoilMultiplier;
    if (Object.op_Inequality((Object) firearmController, (Object) null) && firearmController.HasBipod)
      __instance.HandRotationRecoil.BipodRecoilMultiplier = firearmController.Bipod.BipodRecoilMultiplier;
    if (MongoID.op_Inequality(WeaponStats.WeapID, ((ItemTemplate) template)._id))
      StanceController.DidWeaponSwap = true;
    WeaponStats.WeapID = ((ItemTemplate) template)._id;
    if (PluginConfig.EnableRecoilLogging.Value)
    {
      ModulePatch.Logger.LogWarning((object) "============RecalcWeapParams========");
      ManualLogSource logger1 = ModulePatch.Logger;
      Vector2 vector2 = __instance.BasicPlayerRecoilRotationStrength;
      string str1 = "vert recoil " + vector2.ToString();
      logger1.LogWarning((object) str1);
      ManualLogSource logger2 = ModulePatch.Logger;
      vector2 = __instance.BasicPlayerRecoilPositionStrength;
      string str2 = "horz recoil " + vector2.ToString();
      logger2.LogWarning((object) str2);
      ModulePatch.Logger.LogWarning((object) ("cam recoil " + totalCamRecoil.ToString()));
      ModulePatch.Logger.LogWarning((object) "====================");
    }
    return false;
  }
}
