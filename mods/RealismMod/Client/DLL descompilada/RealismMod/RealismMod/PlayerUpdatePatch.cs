// Decompiled with JetBrains decompiler
// Type: RealismMod.PlayerUpdatePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using BepInEx.Configuration;
using CommonAssets.Scripts.Audio;
using EFT;
using EFT.Animations;
using EFT.Animations.NewRecoil;
using HarmonyLib;
using RealismMod.Weapons;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class PlayerUpdatePatch : ModulePatch
{
  private static FieldInfo surfaceField;
  private static float _sprintCooldownTimer = 0.0f;
  private static bool _doSwayReset = false;
  private static float _sprintTimer = 0.0f;
  private static bool _didSprintPenalties = false;
  private static bool _resetSwayAfterFiring = false;
  private static float _layer20AnimWeight;
  private static int[] _doorHashes = new int[4]
  {
    1682425115,
    2067175453,
    235030625,
    1710112953
  };

  private static bool SkipSprintPenalty => ShootController.IsFiring && !StanceController.IsAiming;

  private static void DoSprintTimer(
    Player player,
    ProceduralWeaponAnimation pwa,
    Player.FirearmController fc,
    float mountingBonus)
  {
    PlayerUpdatePatch._sprintCooldownTimer += Time.deltaTime;
    if (!PlayerUpdatePatch._didSprintPenalties)
    {
      bool skipSprintPenalty = PlayerUpdatePatch.SkipSprintPenalty;
      float num1 = (float) (1.0 + (double) PlayerUpdatePatch._sprintTimer / 7.0);
      float num2 = (float) (1.0 + (double) (WeaponStats.ErgoFactor * (float) (1.0 + (1.0 - (double) PlayerState.GearErgoPenalty)) * WeaponStats.TotalWeaponHandlingModi) / 200.0);
      float num3 = Mathf.Min(pwa.Breath.Intensity * num1 * num2, skipSprintPenalty ? 1f : 3f);
      float num4 = Mathf.Min(pwa.HandsContainer.HandsRotation.InputIntensity * num1 * num2, skipSprintPenalty ? 1f : 1.05f);
      pwa.Breath.Intensity = num3 * mountingBonus;
      pwa.HandsContainer.HandsRotation.InputIntensity = num4 * mountingBonus;
      PlayerState.SprintTotalBreathIntensity = num3;
      PlayerState.SprintTotalHandsIntensity = num4;
      PlayerState.SprintHipfirePenalty = Mathf.Min((float) (1.0 + (double) PlayerUpdatePatch._sprintTimer / 100.0), 1.25f);
      PlayerState.ADSSprintMulti = Mathf.Max((float) (1.0 - (double) PlayerUpdatePatch._sprintTimer / 10.0), 0.45f);
      PlayerUpdatePatch._didSprintPenalties = true;
      PlayerUpdatePatch._doSwayReset = false;
    }
    if ((double) PlayerUpdatePatch._sprintCooldownTimer >= 0.34999999403953552)
    {
      PlayerState.SprintBlockADS = false;
      if (PlayerState.TriedToADSFromSprint)
        fc.ToggleAim();
    }
    if ((double) PlayerUpdatePatch._sprintCooldownTimer < 4.0)
      return;
    PlayerState.WasSprinting = false;
    PlayerUpdatePatch._doSwayReset = true;
    PlayerUpdatePatch._sprintCooldownTimer = 0.0f;
    PlayerUpdatePatch._sprintTimer = 0.0f;
  }

  private static void ResetSwayParams(ProceduralWeaponAnimation pwa, float mountingBonus)
  {
    bool skipSprintPenalty = PlayerUpdatePatch.SkipSprintPenalty;
    float num1 = 0.035f;
    float num2 = 0.4f;
    PlayerState.SprintTotalBreathIntensity = Mathf.Lerp(PlayerState.SprintTotalBreathIntensity, PlayerState.TotalBreathIntensity, num1);
    PlayerState.SprintTotalHandsIntensity = Mathf.Lerp(PlayerState.SprintTotalHandsIntensity, PlayerState.TotalHandsIntensity, num1);
    PlayerState.ADSSprintMulti = Mathf.Lerp(PlayerState.ADSSprintMulti, 1f, num2);
    PlayerState.SprintHipfirePenalty = Mathf.Lerp(PlayerState.SprintHipfirePenalty, 1f, num2);
    pwa.Breath.Intensity = Mathf.Clamp(PlayerState.SprintTotalBreathIntensity * mountingBonus, 0.1f, skipSprintPenalty ? 1f : 3f);
    pwa.HandsContainer.HandsRotation.InputIntensity = Mathf.Clamp(PlayerState.SprintTotalHandsIntensity * mountingBonus, 0.1f, skipSprintPenalty ? 1f : 1.05f);
    if (!Utils.AreFloatsEqual(1f, PlayerState.ADSSprintMulti) || !Utils.AreFloatsEqual(pwa.Breath.Intensity, PlayerState.TotalBreathIntensity) || !Utils.AreFloatsEqual(pwa.HandsContainer.HandsRotation.InputIntensity, PlayerState.TotalHandsIntensity))
      return;
    PlayerUpdatePatch._doSwayReset = false;
  }

  private static void DoSprintPenalty(
    Player player,
    Player.FirearmController fc,
    float mountingBonus)
  {
    if (player.IsSprintEnabled || !player.MovementContext.IsGrounded || player.MovementContext.PlayerAnimatorIsJumpSetted())
    {
      float num1 = !player.MovementContext.IsGrounded ? 2.5f : 1f;
      float num2 = player.MovementContext.PlayerAnimatorIsJumpSetted() ? 4f : 1f;
      PlayerUpdatePatch._sprintTimer += Time.deltaTime * num1 * num2;
      if ((double) PlayerUpdatePatch._sprintTimer >= 1.0)
      {
        PlayerState.SprintBlockADS = true;
        PlayerState.WasSprinting = true;
        PlayerUpdatePatch._didSprintPenalties = false;
      }
    }
    else
    {
      if (PlayerState.WasSprinting)
        PlayerUpdatePatch.DoSprintTimer(player, player.ProceduralWeaponAnimation, fc, mountingBonus);
      if (PlayerUpdatePatch._doSwayReset)
        PlayerUpdatePatch.ResetSwayParams(player.ProceduralWeaponAnimation, mountingBonus);
    }
    PlayerState.HasFullyResetSprintADSPenalties = !PlayerUpdatePatch._doSwayReset && !PlayerState.WasSprinting;
    if (ShootController.IsFiring)
    {
      PlayerUpdatePatch._doSwayReset = false;
      PlayerUpdatePatch._resetSwayAfterFiring = false;
    }
    else
    {
      if (PlayerUpdatePatch._resetSwayAfterFiring)
        return;
      PlayerUpdatePatch._resetSwayAfterFiring = true;
      PlayerUpdatePatch._doSwayReset = true;
    }
  }

  private static void GetStaminaPerc(Player player)
  {
    float num = Mathf.Min((float) ((double) player.Physical.HandsStamina.Current / (double) GClass827<float>.op_Implicit(player.Physical.HandsStamina.TotalCapacity) * (1.0 + (double) PlayerState.StrengthSkillAimBuff)), 1f);
    PlayerState.BaseStaminaPerc = player.Physical.Stamina.Current / GClass827<float>.op_Implicit(player.Physical.Stamina.TotalCapacity);
    PlayerState.RemainingArmStamFactor = Mathf.Pow(num, 0.5f);
    PlayerState.RemainingArmStamReloadFactor = Mathf.Clamp(Mathf.Pow(num, 0.25f), 0.8f, 1f);
    PlayerState.CombinedStaminaPerc = Mathf.Pow(num * PlayerState.BaseStaminaPerc, 0.35f);
  }

  private static void CalcBaseHipfireAccuracy(Player player)
  {
    float num1 = 0.5f;
    float num2 = WeaponStats.IsStocklessPistol || WeaponStats.IsMachinePistol || !WeaponStats.HasShoulderContact ? 1.25f : 1f;
    float num3 = (float) (1.0 - (double) ShootController.BaseTotalConvergence / 100.0);
    float num4 = (float) (1.0 + (double) ShootController.BaseTotalDispersion / 100.0);
    float num5 = (float) (1.0 + ((double) ShootController.BaseTotalVRecoil + (double) ShootController.BaseTotalHRecoil) / 100.0);
    float num6 = (float) (1.0 + (double) PlayerState.TotalModifiedWeightMinusWeapon / 200.0);
    float num7 = (float) ((Plugin.RealHealthController.IsCoughingInGas ? 2.0 : 1.0) * (double) PlayerState.ErgoDeltaInjuryMulti * (Plugin.RealHealthController.HasOverdosed ? 1.5 : 1.0));
    float num8 = Mathf.Max(2f - PlayerState.CombinedStaminaPerc, 0.5f);
    float num9 = (float) (1.0 + (double) (WeaponStats.ErgoFactor * (float) (1.0 + (1.0 - (double) PlayerState.GearErgoPenalty))) / 200.0);
    double num10;
    switch (StanceController.CurrentStance)
    {
      case EStance.ShortStock:
        num10 = 1.3500000238418579;
        break;
      case EStance.ActiveAiming:
        num10 = 0.699999988079071;
        break;
      default:
        num10 = 1.0;
        break;
    }
    float num11 = (float) num10;
    float num12 = num3 * num4 * num5;
    WeaponStats.BaseHipfireInaccuracy = Mathf.Clamp(num1 * num9 * num2 * PlayerState.DeviceBonus * num8 * num11 * Mathf.Pow(WeaponStats.TotalWeaponHandlingModi, 0.45f) * num7 * num12 * num6, 0.2f, 1f);
  }

  private static void ModifyWalkRelatedValues(Player player)
  {
    float num1 = Mathf.Max(2f - PlayerState.CombinedStaminaPerc, 0.5f);
    float num2 = Plugin.RealHealthController.IsCoughingInGas ? 2f : 1f;
    float num3 = WeaponStats.WalkMotionIntensity * PlayerState.ErgoDeltaInjuryMulti * num1 * num2;
    double num4;
    if (!StanceController.IsMounting)
    {
      if (!StanceController.IsBracing)
      {
        if (!StanceController.IsLeftShoulder || StanceController.IsAiming)
        {
          if (!StanceController.IsLeftShoulder)
          {
            if (!StanceController.IsAiming)
            {
              switch (StanceController.CurrentStance)
              {
                case EStance.LowReady:
                  num4 = 0.85000002384185791;
                  break;
                case EStance.HighReady:
                  num4 = 0.89999997615814209;
                  break;
                case EStance.ShortStock:
                  num4 = 0.89999997615814209;
                  break;
                case EStance.ActiveAiming:
                  num4 = 0.89999997615814209;
                  break;
                case EStance.PistolCompressed:
                  num4 = 1.2000000476837158;
                  break;
                default:
                  num4 = 1.0;
                  break;
              }
            }
            else
              num4 = 0.85000002384185791;
          }
          else
            num4 = 0.949999988079071;
        }
        else
          num4 = 1.1499999761581421;
      }
      else
        num4 = 0.5;
    }
    else
      num4 = 0.25;
    float num5 = (float) num4;
    double num6;
    if (!StanceController.IsMounting)
    {
      if (!StanceController.IsBracing)
      {
        if (!StanceController.IsLeftShoulder || StanceController.IsAiming)
        {
          if (!StanceController.IsLeftShoulder)
          {
            if (!StanceController.IsAiming)
            {
              switch (StanceController.CurrentStance)
              {
                case EStance.LowReady:
                  num6 = 0.699999988079071;
                  break;
                case EStance.HighReady:
                  num6 = 0.800000011920929;
                  break;
                case EStance.ShortStock:
                  num6 = 0.800000011920929;
                  break;
                case EStance.ActiveAiming:
                  num6 = 0.85000002384185791;
                  break;
                case EStance.PistolCompressed:
                  num6 = 1.1499999761581421;
                  break;
                default:
                  num6 = 1.0;
                  break;
              }
            }
            else
              num6 = 0.89999997615814209;
          }
          else
            num6 = 0.949999988079071;
        }
        else
          num6 = 1.0499999523162842;
      }
      else
        num6 = 0.5;
    }
    else
      num6 = 0.25;
    float num7 = (float) num6;
    player.ProceduralWeaponAnimation.Walk.StepFrequency = Mathf.Min(player.ProceduralWeaponAnimation.Walk.StepFrequency, 1.1f);
    player.ProceduralWeaponAnimation.Walk.IntensityMinMax[0] = new Vector2(0.5f, 1f);
    player.ProceduralWeaponAnimation.HandsContainer.HandsPosition.ReturnSpeed = 0.1f;
    player.ProceduralWeaponAnimation.HandsContainer.HandsPosition.InputIntensity = Mathf.Clamp(0.99f * num7 * num3, 0.55f, 0.85f) * PluginConfig.ProceduralIntensity.Value;
    player.ProceduralWeaponAnimation.HandsContainer.HandsRotation.ReturnSpeed = 0.05f;
    player.ProceduralWeaponAnimation.HandsContainer.HandsRotation.InputIntensity = Mathf.Clamp(0.98f * num5 * num3, 0.6f, 0.95f * PluginConfig.ProceduralIntensity.Value);
    player.ProceduralWeaponAnimation.MotionReact.Intensity = WeaponStats.BaseWeaponMotionIntensity * num1 * PlayerState.DeviceBonus * PluginConfig.ProceduralIntensity.Value;
  }

  private static void SetStancePWAValues(Player player, Player.FirearmController fc)
  {
    PlayerUpdatePatch.ModifyWalkRelatedValues(player);
    if (StanceController.CanResetDamping)
    {
      float num1 = WeaponStats.IsPistol ? PluginConfig.PistolRecoilDampingMulti.Value : PluginConfig.RiflRecoilDampingMulti.Value;
      float num2 = WeaponStats.IsStockedPistol ? 0.75f : 1f;
      NewRecoilShotEffect currentRecoilEffect = player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect as NewRecoilShotEffect;
      currentRecoilEffect.HandRotationRecoil.CategoryIntensityMultiplier = Mathf.Lerp(currentRecoilEffect.HandRotationRecoil.CategoryIntensityMultiplier, fc.Weapon.Template.RecoilCategoryMultiplierHandRotation * PluginConfig.RecoilIntensity.Value * num2, 0.01f);
      currentRecoilEffect.HandRotationRecoil.ReturnTrajectoryDumping = Mathf.Lerp(currentRecoilEffect.HandRotationRecoil.ReturnTrajectoryDumping, fc.Weapon.Template.RecoilReturnPathDampingHandRotation, 0.01f);
      ((RecoilProcessBase) player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect.HandRotationRecoilEffect).Damping = Mathf.Lerp(((RecoilProcessBase) player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect.HandRotationRecoilEffect).Damping, fc.Weapon.Template.RecoilDampingHandRotation * num1, 0.01f);
      player.ProceduralWeaponAnimation.HandsContainer.HandsPosition.Damping = Mathf.Lerp(player.ProceduralWeaponAnimation.HandsContainer.HandsPosition.Damping, 0.41f, 0.01f);
      ((RecoilProcessBase) player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect.HandRotationRecoilEffect).ReturnSpeed = Mathf.Lerp(((RecoilProcessBase) player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect.HandRotationRecoilEffect).ReturnSpeed, ShootController.BaseTotalConvergence, 0.01f);
    }
    else
    {
      player.ProceduralWeaponAnimation.HandsContainer.HandsPosition.Damping = 0.75f;
      NewRecoilShotEffect currentRecoilEffect = player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect as NewRecoilShotEffect;
      currentRecoilEffect.HandRotationRecoil.CategoryIntensityMultiplier = WeaponStats._WeapClass == "pistol" ? 0.4f : 0.3f;
      currentRecoilEffect.HandRotationRecoil.ReturnTrajectoryDumping = 0.8f;
      ((RecoilProcessBase) player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect.HandRotationRecoilEffect).Damping = 0.8f;
      ((RecoilProcessBase) player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect.HandRotationRecoilEffect).ReturnSpeed = 10f * StanceController.WiggleReturnSpeed;
    }
    player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect.HandPositionRecoilEffect.Damping = 0.5f;
    player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect.HandPositionRecoilEffect.ReturnSpeed = 0.08f;
    player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect.RecoilProcessValues[3].IntensityMultiplicator = 0.0f;
    player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect.RecoilProcessValues[4].IntensityMultiplicator = 0.0f;
  }

  private static void ChamberTimer(Player.FirearmController fc)
  {
    Plugin.ChamberTimer += Time.deltaTime;
    if ((double) Plugin.ChamberTimer < 0.5)
      return;
    ((Player.AbstractHandsController) fc).FirearmsAnimator.Rechamber(false);
    fc.SetAnimatorAndProceduralValues();
    Plugin.StartRechamberTimer = false;
    Plugin.ChamberTimer = 0.0f;
  }

  private static void PlaySprintSfx(
    Player player,
    ref float ____lastStepTime,
    BetterSource ___NestedStepSoundSource,
    float ____sprintSurfaceCheck,
    ref bool ____playedAtLeastOneStep,
    GInterface86 ____specificStepAudioController,
    ref float ____sign)
  {
    if ((double) Math.Abs(____sign - player.Single_0) < 1.4012984643248171E-45)
      return;
    ____sign = player.Single_0;
    float num1 = Time.time - ____lastStepTime;
    if ((double) num1 > 0.20000000298023224 && (double) player.MovementContext.FreefallTime < 0.60000002384185791)
    {
      ____playedAtLeastOneStep = true;
      ____lastStepTime = Time.time;
      player.method_63(___NestedStepSoundSource);
      if (player.CheckSurface(____sprintSurfaceCheck))
      {
        SurfaceSet surfaceSet = (SurfaceSet) PlayerUpdatePatch.surfaceField.GetValue((object) player);
        player.UpdateMuffledState();
        SoundBank sprintSoundBank = surfaceSet.SprintSoundBank;
        float num2 = player.FirstPersonPointOfView ? sprintSoundBank.BaseVolume : 1f;
        float num3 = (float) (0.5 + 3.0 * (double) player.Physical.Overweight);
        float num4 = player.method_62((EAudioMovementState) 2) * num2 * num3;
        player.method_64((EAudioMovementState) 2, false);
        surfaceSet.SprintSoundBank.Play(___NestedStepSoundSource, (EnvironmentType) 0, player.Distance, num4, player.Distance, player.FirstPersonPointOfView, true);
        ____specificStepAudioController.Play((EAudioMovementState) 2, player.Environment, player.Distance, num4, player.Distance, player.FirstPersonPointOfView);
        player.method_58(1.1f, false);
        if ((double) num1 < 1.2000000476837158 && player.FirstPersonPointOfView)
          player.ProceduralWeaponAnimation.Walk.StepFrequency = 0.5f / Mathf.Clamp(num1, 0.3f, 0.8f);
      }
    }
  }

  private static void PWAUpdate(Player player, Player.FirearmController fc)
  {
    if (Object.op_Inequality((Object) fc, (Object) null))
    {
      if (Plugin.ServerConfig.recoil_attachment_overhaul)
        ShootController.ShootUpdate(player, fc);
      WeaponStats.BipodIsDeployed = fc.HasBipod && fc.BipodState;
      WeaponStats.FireMode = fc.Item.SelectedFireMode;
      if (Plugin.StartRechamberTimer)
        PlayerUpdatePatch.ChamberTimer(fc);
      if (Plugin.ServerConfig.enable_stances)
        StanceController.ToggleMounting(player, player.ProceduralWeaponAnimation, fc);
      if (ShootController.IsFiring)
      {
        ShootController.SetRecoilParams(player.ProceduralWeaponAnimation, fc.Item, player);
        if (StanceController.CurrentStance == EStance.PatrolStance)
          StanceController.CurrentStance = EStance.None;
      }
      ReloadController.ReloadStateCheck(player, fc);
      AimController.ADSCheck(player, fc);
      if (PluginConfig.EnableStanceStamChanges.Value && Plugin.ServerConfig.enable_stances)
        StanceController.SetStanceStamina(player);
      PlayerUpdatePatch.GetStaminaPerc(player);
      if (!ShootController.IsFiringMovement && Plugin.ServerConfig.enable_stances)
        PlayerUpdatePatch.SetStancePWAValues(player, fc);
      player.MovementContext.SetPatrol(StanceController.IsInThirdPerson && StanceController.CurrentStance == EStance.PatrolStance);
    }
    else if (Plugin.ServerConfig.enable_stances && PluginConfig.EnableStanceStamChanges.Value && !StanceController.HaveResetStamDrain)
      StanceController.UnarmedStanceStamina(player);
    else
      StanceController.IsMounting = false;
    PlayerUpdatePatch.CalcBaseHipfireAccuracy(player);
    player.ProceduralWeaponAnimation.Breath.HipPenalty = Mathf.Clamp(WeaponStats.BaseHipfireInaccuracy * PlayerState.SprintHipfirePenalty, 0.1f, 0.5f);
  }

  private static void SmoothenAnimations(Player player)
  {
    int nameHash = player._animators[0].GetCurrentAnimatorStateInfo(20).nameHash;
    bool flag = ((IEnumerable<int>) PlayerUpdatePatch._doorHashes).Contains<int>(nameHash);
    if (PlayerState.IsInInventory | flag)
    {
      float num = flag ? 0.9f : 0.15f;
      PlayerUpdatePatch._layer20AnimWeight = Mathf.MoveTowards(PlayerUpdatePatch._layer20AnimWeight, num, 1.9f * Time.deltaTime);
      player._animators[0].SetLayerWeight(20, PlayerUpdatePatch._layer20AnimWeight);
    }
    else
    {
      PlayerUpdatePatch._layer20AnimWeight = Mathf.MoveTowards(PlayerUpdatePatch._layer20AnimWeight, 1f, 1f * Time.deltaTime);
      player._animators[0].SetLayerWeight(20, PlayerUpdatePatch._layer20AnimWeight);
    }
  }

  protected virtual MethodBase GetTargetMethod()
  {
    PlayerUpdatePatch.surfaceField = AccessTools.Field(typeof (Player), "_currentSet");
    return (MethodBase) typeof (Player).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(
    Player __instance,
    ref float ____lastStepTime,
    BetterSource ___NestedStepSoundSource,
    float ____sprintSurfaceCheck,
    ref bool ____playedAtLeastOneStep,
    GInterface86 ____specificStepAudioController,
    ref float ____sign)
  {
    if (Plugin.ServerConfig.headset_changes)
    {
      SurfaceSet surfaceSet = (SurfaceSet) PlayerUpdatePatch.surfaceField.GetValue((object) __instance);
      surfaceSet.SprintSoundBank.BaseVolume = PluginConfig.SharedMovementVolume.Value;
      surfaceSet.StopSoundBank.BaseVolume = PluginConfig.SharedMovementVolume.Value;
      surfaceSet.JumpSoundBank.BaseVolume = PluginConfig.SharedMovementVolume.Value;
      surfaceSet.LandingSoundBank.BaseVolume = PluginConfig.SharedMovementVolume.Value;
    }
    if (!Utils.PlayerIsReady || !__instance.IsYourPlayer)
      return;
    if (PlayerState.IsSprinting && StanceController.CanDoMeleeDetection && WeaponStats.HasBayonet && StanceController.IsReadyForBayonetCharge)
      PlayerUpdatePatch.PlaySprintSfx(__instance, ref ____lastStepTime, ___NestedStepSoundSource, ____sprintSurfaceCheck, ref ____playedAtLeastOneStep, ____specificStepAudioController, ref ____sign);
    if (PluginConfig.EnableAnimationFixes.Value)
      PlayerUpdatePatch.SmoothenAnimations(__instance);
    GameWorldController.TimeInRaid += Time.deltaTime;
    Player.FirearmController handsController = __instance.HandsController as Player.FirearmController;
    PlayerState.IsSprinting = __instance.IsSprintEnabled;
    PlayerState.EnviroType = __instance.Environment;
    PlayerState.BtrState = __instance.BtrState;
    PlayerState.IsMoving = __instance.IsSprintEnabled || (__instance.ProceduralWeaponAnimation.Mask & 2) > 0;
    KeyboardShortcut keyboardShortcut1 = PluginConfig.ToggleGasMaskKey.Value;
    int num;
    if (Input.GetKeyDown(((KeyboardShortcut) ref keyboardShortcut1).MainKey))
    {
      KeyboardShortcut keyboardShortcut2 = PluginConfig.ToggleGasMaskKey.Value;
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      num = ((KeyboardShortcut) ref keyboardShortcut2).Modifiers.All<KeyCode>(PlayerUpdatePatch.\u003C\u003EO.\u003C0\u003E__GetKey ?? (PlayerUpdatePatch.\u003C\u003EO.\u003C0\u003E__GetKey = new Func<KeyCode, bool>(Input.GetKey))) ? 1 : 0;
    }
    else
      num = 0;
    if (num != 0)
      GearController.ToggleGasMask(__instance);
    if (PluginConfig.EnableSprintPenalty.Value)
      PlayerUpdatePatch.DoSprintPenalty(__instance, handsController, StanceController.BracingSwayBonus);
    else
      PlayerState.HasFullyResetSprintADSPenalties = true;
    if (PlayerState.HasFullyResetSprintADSPenalties)
    {
      __instance.ProceduralWeaponAnimation.Breath.Intensity = PlayerState.TotalBreathIntensity * StanceController.BracingSwayBonus;
      __instance.ProceduralWeaponAnimation.HandsContainer.HandsRotation.InputIntensity = PlayerState.TotalHandsIntensity * StanceController.BracingSwayBonus;
    }
    if (Plugin.ServerConfig.recoil_attachment_overhaul)
      PlayerUpdatePatch.PWAUpdate(__instance, handsController);
  }
}
