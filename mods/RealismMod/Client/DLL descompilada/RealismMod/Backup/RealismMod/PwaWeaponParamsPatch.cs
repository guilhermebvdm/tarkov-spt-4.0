// Decompiled with JetBrains decompiler
// Type: RealismMod.PwaWeaponParamsPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.Animations;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class PwaWeaponParamsPatch : ModulePatch
{
  private static FieldInfo playerField;
  private static FieldInfo fcField;
  private static FieldInfo float3Field;
  private static bool didAimWiggle;

  private static void DoADSWiggle(
    ProceduralWeaponAnimation pwa,
    Player player,
    Player.FirearmController fc,
    float factor)
  {
    if (!StanceController.IsIdle() || (double) WeaponStats.TotalWeaponWeight > 8.0 || !(WeaponStats._WeapClass.ToLower() != "pistol"))
      return;
    StanceController.CanResetDamping = false;
    float num1 = StanceController.IsMounting ? 0.1f : (StanceController.IsBracing ? 0.25f : 1f);
    float num2 = GearController.FSIsActive || GearController.NVGIsActive || GearController.HasGasMask ? 3f : 1f;
    float num3 = Mathf.Clamp(3.5f * factor * num2 * num1 * WeaponStats.TotalAimStabilityModi, 0.1f, 17f);
    float num4 = Random.Range(num3 * 0.9f, num3);
    float num5 = Random.Range(num3 * 0.9f, num3);
    Vector3 wiggleDirection;
    // ISSUE: explicit constructor call
    ((Vector3) ref wiggleDirection).\u002Ector(-num4, -num5, 0.0f);
    if (pwa.IsAiming && !PwaWeaponParamsPatch.didAimWiggle)
    {
      if (!StanceController.IsFiringFromStance && !StanceController.IsLeftShoulder)
        StanceController.DoWiggleEffects(player, pwa, fc.Weapon, wiggleDirection, wiggleFactor: factor, isADS: true);
      PwaWeaponParamsPatch.didAimWiggle = true;
    }
    else if (!pwa.IsAiming && PwaWeaponParamsPatch.didAimWiggle)
      PwaWeaponParamsPatch.didAimWiggle = false;
    StanceController.DoDampingTimer = true;
  }

  protected virtual MethodBase GetTargetMethod()
  {
    PwaWeaponParamsPatch.float3Field = AccessTools.Field(typeof (Player.FirearmController), "float_3");
    PwaWeaponParamsPatch.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    PwaWeaponParamsPatch.fcField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_firearmController");
    return (MethodBase) typeof (ProceduralWeaponAnimation).GetMethod("method_23", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(ProceduralWeaponAnimation __instance)
  {
    Player.FirearmController fc = (Player.FirearmController) PwaWeaponParamsPatch.fcField.GetValue((object) __instance);
    if (Object.op_Equality((Object) fc, (Object) null))
      return;
    Player player = (Player) PwaWeaponParamsPatch.playerField.GetValue((object) fc);
    if (!Object.op_Inequality((Object) player, (Object) null) || !player.IsYourPlayer || player.MovementContext.CurrentState.Name == 21)
      return;
    Weapon weapon = fc.Weapon;
    if (weapon != null)
    {
      __instance.Overweight = 0.0f;
      __instance.CrankRecoil = PluginConfig.EnableCrank.Value;
      Mod currentAimingMod = __instance.CurrentAimingMod != null ? ((GClass3175) __instance.CurrentAimingMod).Item as Mod : (Mod) null;
      WeaponMod dataObj = currentAimingMod == null ? (WeaponMod) null : TemplateStats.GetDataObj<WeaponMod>(TemplateStats.WeaponModStats, MongoID.op_Implicit(((Item) currentAimingMod).TemplateId));
      WeaponStats.IsOptic = __instance.CurrentScope.IsOptic;
      StatCalc.CalcSightAccuracy(currentAimingMod, dataObj);
      float totalCenterOfImpact = weapon.GetTotalCenterOfImpact(false);
      PwaWeaponParamsPatch.float3Field.SetValue((object) fc, (object) totalCenterOfImpact);
      float weightMinusWeapon = PlayerState.TotalModifiedWeightMinusWeapon;
      float num1 = (float) (1.0 - (double) weightMinusWeapon / 200.0);
      float num2 = !StanceController.IsIdle() || StanceController.IsLeftShoulder ? (StanceController.WasActiveAim || StanceController.CurrentStance == EStance.ActiveAiming ? 1.35f : (StanceController.CurrentStance == EStance.HighReady || StanceController.CurrentStance == EStance.HighReady ? 1.25f : (StanceController.StoredStance == EStance.LowReady || StanceController.CurrentStance == EStance.LowReady ? 1.25f : (StanceController.IsLeftShoulder ? 0.85f : 1f)))) : 1.5f;
      float num3 = WeaponStats.IsPistol || WeaponStats.HasShoulderContact ? 1f : 0.75f;
      float num4 = WeaponStats.SightlessAimSpeed * PlayerState.ADSInjuryMulti * Mathf.Max(PlayerState.RemainingArmStamFactor, 0.35f);
      float num5 = currentAimingMod != null ? dataObj.AimSpeed : 1f;
      float num6 = currentAimingMod == null || !MongoID.op_Equality(((Item) currentAimingMod).TemplateId, MongoID.op_Implicit("5c07dd120db834001c39092d")) && !MongoID.op_Equality(((Item) currentAimingMod).TemplateId, MongoID.op_Implicit("5c0a2cec0db834001b7ce47d")) || !__instance.CurrentScope.IsOptic ? num5 : 1f;
      float num7 = Mathf.Clamp(num4 * (float) (1.0 + (double) num6 / 100.0) * num2 * num3 * num1, 0.4f, 1.5f);
      float num8 = Mathf.Max((float) ((double) num7 * (double) PlayerState.ADSSprintMulti * (double) Plugin.RealHealthController.AdrenalineADSBonus * (1.0 + (double) WeaponStats.ModAimSpeedModifier)), 0.28f) * (WeaponStats.IsPistol ? PluginConfig.PistolGlobalAimSpeedModifier.Value : PluginConfig.GlobalAimSpeedModifier.Value);
      WeaponStats.TotalFinalAimSpeed = num8;
      AccessTools.Field(typeof (ProceduralWeaponAnimation), "_aimingSpeed").SetValue((object) __instance, (object) num8);
      float num9 = StanceController.IsLeftShoulder ? 1.3f : 1f;
      float num10 = WeaponStats.IsBullpup ? 0.7f : 1f;
      float num11 = (float) ((double) WeaponStats.ErgoFactor * (double) PlayerState.ErgoDeltaInjuryMulti * (1.0 - (double) PlayerState.StrengthSkillAimBuff * 1.75) * (1.0 + (1.0 - (double) PlayerState.GearErgoPenalty)));
      float num12 = StatCalc.ProceduralIntensityFactorCalc(((Item) weapon).TotalWeight, WeaponStats.IsPistol ? 1f : 4f);
      float num13 = (float) (1.0 + (double) weightMinusWeapon / 200.0);
      float factor = (float) (1.0 + (double) num11 * (double) num12 * (double) num13 * (double) num9 / 100.0);
      float num14;
      float num15;
      if (!WeaponStats.HasShoulderContact && !WeaponStats.IsPistol)
      {
        num14 = Mathf.Clamp(0.6f * factor, 0.47f, 1.01f);
        num15 = Mathf.Clamp(0.6f * factor, 0.47f, 1.05f);
      }
      else if (!WeaponStats.HasShoulderContact && (WeaponStats.IsStocklessPistol || WeaponStats.IsMachinePistol))
      {
        num14 = Mathf.Clamp(0.55f * factor, 0.4f, 0.9f);
        num15 = Mathf.Clamp(0.55f * factor, 0.4f, 0.95f);
      }
      else
      {
        num14 = Mathf.Clamp(0.4f * factor * num10, 0.33f, 0.85f);
        num15 = Mathf.Clamp(0.4f * factor * num10, 0.33f, 0.89f);
      }
      float num16 = (double) ((Item) weapon).Weight >= 9.0 ? 1.45f : 1f;
      float num17 = num14 * __instance.IntensityByPoseLevel * PluginConfig.ProceduralIntensity.Value * num16;
      float num18 = num15 * num15 * PluginConfig.ProceduralIntensity.Value * num16;
      PlayerState.TotalBreathIntensity = num17;
      PlayerState.TotalHandsIntensity = num18;
      if (PlayerState.HasFullyResetSprintADSPenalties)
      {
        __instance.Breath.Intensity = PlayerState.TotalBreathIntensity * StanceController.BracingSwayBonus;
        __instance.HandsContainer.HandsRotation.InputIntensity = PlayerState.TotalHandsIntensity * StanceController.BracingSwayBonus;
      }
      else
      {
        __instance.Breath.Intensity = PlayerState.SprintTotalBreathIntensity;
        __instance.HandsContainer.HandsRotation.InputIntensity = PlayerState.SprintTotalHandsIntensity;
      }
      if (__instance.CurrentAimingMod != null)
      {
        string id = ((GClass3175) __instance.CurrentAimingMod)?.Item?.Id != null ? ((GClass3175) __instance.CurrentAimingMod).Item.Id : "";
        WeaponStats.ScopeID = id;
        if (id != null)
        {
          Vector2 vector2;
          if (WeaponStats.ZeroOffsetDict.TryGetValue(id, out vector2))
          {
            WeaponStats.ZeroRecoilOffset = vector2;
          }
          else
          {
            WeaponStats.ZeroRecoilOffset = Vector2.zero;
            WeaponStats.ZeroOffsetDict.Add(id, WeaponStats.ZeroRecoilOffset);
          }
        }
      }
      PwaWeaponParamsPatch.DoADSWiggle(__instance, player, fc, factor);
      if (PluginConfig.EnablePWALogging.Value)
      {
        ModulePatch.Logger.LogWarning((object) "=====method_23========");
        ModulePatch.Logger.LogWarning((object) ("ADSInjuryMulti = " + PlayerState.ADSInjuryMulti.ToString()));
        ModulePatch.Logger.LogWarning((object) ("remaining stam percentage = " + PlayerState.RemainingArmStamFactor.ToString()));
        ModulePatch.Logger.LogWarning((object) ("strength = " + PlayerState.StrengthSkillAimBuff.ToString()));
        ModulePatch.Logger.LogWarning((object) ("player weight = " + num1.ToString()));
        ModulePatch.Logger.LogWarning((object) ("sightSpeedModi = " + num6.ToString()));
        ModulePatch.Logger.LogWarning((object) ("totalSightlessAimSpeed = " + num4.ToString()));
        ModulePatch.Logger.LogWarning((object) ("totalSightedAimSpeed = " + num7.ToString()));
        ModulePatch.Logger.LogWarning((object) ("newAimSpeed = " + num8.ToString()));
        ModulePatch.Logger.LogWarning((object) ("totalBreathIntensity = " + num17.ToString()));
        ModulePatch.Logger.LogWarning((object) ("totalInputIntensitry = " + num18.ToString()));
        ModulePatch.Logger.LogWarning((object) ("ergoWeight = " + num11.ToString()));
        ModulePatch.Logger.LogWarning((object) ("ergoWeightFactor = " + num12.ToString()));
        ModulePatch.Logger.LogWarning((object) ("totalErgoFactor = " + factor.ToString()));
        ModulePatch.Logger.LogWarning((object) ("player weight factor = " + num13.ToString()));
      }
    }
    else if (__instance.PointOfView == 0)
    {
      int num = (int) AccessTools.Property(typeof (ProceduralWeaponAnimation), "AimIndex").GetValue((object) __instance);
      if (!__instance.Sprint && num < __instance.ScopeAimTransforms.Count)
      {
        __instance.Breath.Intensity = 0.5f * __instance.IntensityByPoseLevel;
        __instance.HandsContainer.HandsRotation.InputIntensity = 0.25f;
      }
    }
  }
}
