// Decompiled with JetBrains decompiler
// Type: RealismMod.UpdateSwayFactorsPatch
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

public class UpdateSwayFactorsPatch : ModulePatch
{
  private static FieldInfo playerField;
  private static FieldInfo fcField;

  protected virtual MethodBase GetTargetMethod()
  {
    UpdateSwayFactorsPatch.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    UpdateSwayFactorsPatch.fcField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_firearmController");
    return (MethodBase) typeof (ProceduralWeaponAnimation).GetMethod("UpdateSwayFactors", BindingFlags.Instance | BindingFlags.Public);
  }

  private static float GetStanceFactor(ProceduralWeaponAnimation pwa, bool forDisplacement = false)
  {
    if (forDisplacement)
    {
      double stanceFactor;
      if (!StanceController.IsMounting)
      {
        if (!StanceController.IsBracing)
        {
          if (!StanceController.IsLeftShoulder)
          {
            switch (StanceController.CurrentStance)
            {
              case EStance.LowReady:
                stanceFactor = 0.87000000476837158;
                break;
              case EStance.HighReady:
                stanceFactor = 0.9100000262260437;
                break;
              case EStance.ShortStock:
                stanceFactor = 0.75;
                break;
              case EStance.ActiveAiming:
                stanceFactor = 0.949999988079071;
                break;
              default:
                stanceFactor = 1.0;
                break;
            }
          }
          else
            stanceFactor = 1.1499999761581421;
        }
        else
          stanceFactor = 0.34999999403953552;
      }
      else
        stanceFactor = 0.20000000298023224;
      return (float) stanceFactor;
    }
    double stanceFactor1;
    if (!StanceController.IsMounting)
    {
      if (!StanceController.IsBracing)
      {
        if (!StanceController.IsLeftShoulder || pwa.IsAiming)
        {
          if (!StanceController.IsLeftShoulder)
          {
            if (!pwa.IsAiming)
            {
              if ((double) WeaponStats.TotalWeaponWeight <= 1.6000000238418579 || StanceController.CurrentStance != EStance.PistolCompressed)
              {
                switch (StanceController.CurrentStance)
                {
                  case EStance.LowReady:
                    stanceFactor1 = 0.800000011920929;
                    break;
                  case EStance.HighReady:
                    stanceFactor1 = 0.85000002384185791;
                    break;
                  case EStance.ShortStock:
                    stanceFactor1 = 0.800000011920929;
                    break;
                  case EStance.ActiveAiming:
                    stanceFactor1 = 0.89999997615814209;
                    break;
                  case EStance.PistolCompressed:
                    stanceFactor1 = 1.1499999761581421;
                    break;
                  default:
                    stanceFactor1 = 1.0;
                    break;
                }
              }
              else
                stanceFactor1 = 0.85000002384185791;
            }
            else
              stanceFactor1 = 0.75;
          }
          else
            stanceFactor1 = 0.87000000476837158;
        }
        else
          stanceFactor1 = 1.1499999761581421;
      }
      else
        stanceFactor1 = 0.10000000149011612;
    }
    else
      stanceFactor1 = 0.05000000074505806;
    return (float) stanceFactor1;
  }

  [PatchPrefix]
  private static bool Prefix(ProceduralWeaponAnimation __instance)
  {
    Player.FirearmController firearmController = (Player.FirearmController) UpdateSwayFactorsPatch.fcField.GetValue((object) __instance);
    if (Object.op_Equality((Object) firearmController, (Object) null))
      return false;
    Player player = (Player) UpdateSwayFactorsPatch.playerField.GetValue((object) firearmController);
    if (!Object.op_Inequality((Object) player, (Object) null) || !player.IsYourPlayer || player.MovementContext.CurrentState.Name == 21)
      return true;
    Weapon weapon = firearmController.Weapon;
    bool flag1 = WeaponStats.IsStocklessPistol || WeaponStats.IsMachinePistol;
    float stanceFactor1 = UpdateSwayFactorsPatch.GetStanceFactor(__instance, true);
    float stanceFactor2 = UpdateSwayFactorsPatch.GetStanceFactor(__instance);
    float totalWeight = ((Item) weapon).TotalWeight;
    float num1 = WeaponStats.IsBullpup ? 0.75f : 1f;
    float num2 = (float) (1.0 + (double) (PlayerState.TotalModifiedWeight - totalWeight) / 200.0);
    bool flag2 = !WeaponStats.HasShoulderContact;
    float num3 = (float) ((double) WeaponStats.ErgoFactor * (double) PlayerState.ErgoDeltaInjuryMulti * (1.0 - (double) PlayerState.StrengthSkillAimBuff) * (1.0 + (1.0 - (double) PlayerState.GearErgoPenalty)));
    float num4 = StatCalc.ProceduralIntensityFactorCalc(totalWeight, flag1 ? 1.3f : 2.4f);
    float num5 = flag2 ? PluginConfig.ProceduralIntensity.Value * 0.95f : PluginConfig.ProceduralIntensity.Value * 0.48f;
    float num6 = flag2 ? PluginConfig.ProceduralIntensity.Value * 0.86f : PluginConfig.ProceduralIntensity.Value * 0.51f;
    float num7 = flag1 ? 17.5f : 15.8f;
    float num8 = flag1 ? 0.95f : 0.88f;
    float num9 = flag1 ? 2.1f : 5.9f;
    float num10 = flag1 ? 42f : 169f;
    float num11 = flag1 ? 0.44f : 0.53f;
    float num12 = flag1 ? 0.84f : 1.09f;
    float num13 = num3 * num4 * num2 * num1;
    float num14 = Mathf.Clamp(num13 / num7, num8, num9) * (float) ((double) stanceFactor1 * (double) WeaponStats.TotalWeaponHandlingModi * (__instance.IsAiming ? 1.0900000333786011 : 1.0));
    float num15 = Mathf.Clamp(num13 / num10, num11, num12) * (stanceFactor1 * WeaponStats.TotalWeaponHandlingModi);
    AccessTools.Field(typeof (ProceduralWeaponAnimation), "_displacementStr").SetValue((object) __instance, (object) (float) ((double) num14 * (double) num5 * (double) num2));
    AccessTools.Field(typeof (ProceduralWeaponAnimation), "_swayStrength").SetValue((object) __instance, (object) num15);
    __instance.MotionReact.SwayFactors = Vector3.op_Multiply(new Vector3(num15, __instance.IsAiming ? num15 * 0.3f : num15, num15), Mathf.Clamp(num6 * num4 * num2, num6, 1f));
    float num16 = WeaponStats.IsStocklessPistol || WeaponStats.IsMachinePistol || !WeaponStats.HasShoulderContact ? 1.55f : (WeaponStats.IsBullpup ? 0.8f : 1f);
    float num17 = flag1 ? 1.45f : 2.55f;
    float num18 = flag1 ? 1.35f : 1.35f;
    WeaponStats.BaseWeaponMotionIntensity = Mathf.Clamp(0.065f * stanceFactor2 * num3 * num2 * num16 * WeaponStats.TotalWeaponHandlingModi, num18, num17) * 0.33f;
    float num19 = WeaponStats.IsBullpup ? 0.8f : (WeaponStats.IsMachinePistol || WeaponStats.IsStocklessPistol ? 1.4f : (!WeaponStats.HasShoulderContact ? 1.45f : 1f));
    WeaponStats.WalkMotionIntensity = (float) ((double) Mathf.Pow((float) (2.0999999046325684 * ((double) WeaponStats.ErgoFactor / 100.0)) * WeaponStats.TotalWeaponHandlingModi, 0.87f) * (double) num19 * (double) num2 * (1.0 + (1.0 - (double) PlayerState.GearErgoPenalty)));
    if (PluginConfig.EnablePWALogging.Value)
    {
      ModulePatch.Logger.LogWarning((object) "=====UpdateSwayFactors====");
      ModulePatch.Logger.LogWarning((object) ("ergoWeight = " + num3.ToString()));
      ModulePatch.Logger.LogWarning((object) ("weightFactor = " + num4.ToString()));
      ModulePatch.Logger.LogWarning((object) ("swayStrength = " + num15.ToString()));
      ModulePatch.Logger.LogWarning((object) ("displacementStrength = " + num14.ToString()));
      ModulePatch.Logger.LogWarning((object) ("displacementModifier = " + num5.ToString()));
      ModulePatch.Logger.LogWarning((object) ("aimIntensity = " + num6.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Sway Factors = " + __instance.MotionReact.SwayFactors.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Motion Intensity = " + WeaponStats.BaseWeaponMotionIntensity.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Walk Motion Intensity = " + WeaponStats.WalkMotionIntensity.ToString()));
      ModulePatch.Logger.LogWarning((object) ("PlayerState.GearErgoPenalty = " + PlayerState.GearErgoPenalty.ToString()));
      ModulePatch.Logger.LogWarning((object) ("ergoWeightFactor = " + num4.ToString()));
      ModulePatch.Logger.LogWarning((object) ("stanceFactor = " + stanceFactor1.ToString()));
      ModulePatch.Logger.LogWarning((object) ("has bipod " + (firearmController.HasBipod && firearmController.BipodState).ToString()));
    }
    return false;
  }
}
