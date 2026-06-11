// Decompiled with JetBrains decompiler
// Type: RealismMod.ApplySimpleRotationPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.Animations;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class ApplySimpleRotationPatch : ModulePatch
{
  private static FieldInfo aimSpeedField;
  private static FieldInfo blindFireStrength;
  private static FieldInfo scopeRotationField;
  private static FieldInfo weapRotationField;
  private static FieldInfo isAimingField;
  private static FieldInfo weaponPositionField;
  private static FieldInfo currentRotationField;
  private static FieldInfo firearmControllerField;
  private static FieldInfo playerField;
  private static Vector3 mountWeapPosition = Vector3.zero;
  private static Vector3 lowReadyTargetRotation = new Vector3(18f, 5f, -1f);
  private static Quaternion lowReadyTargetQuaternion = Quaternion.Euler(ApplySimpleRotationPatch.lowReadyTargetRotation);
  private static Vector3 lowReadyTargetPostion = new Vector3(0.06f, 0.04f, 0.0f);
  private static Vector3 highReadyTargetRotation = new Vector3(-15f, 3f, 3f);
  private static Quaternion highReadyTargetQuaternion = Quaternion.Euler(ApplySimpleRotationPatch.highReadyTargetRotation);
  private static Vector3 highReadyTargetPostion = new Vector3(0.05f, 0.1f, -0.12f);
  private static Vector3 activeAimTargetRotation = new Vector3(0.0f, -40f, 0.0f);
  private static Quaternion activeAimTargetQuaternion = Quaternion.Euler(ApplySimpleRotationPatch.activeAimTargetRotation);
  private static Vector3 activeAimTargetPostion = new Vector3(0.0f, 0.0f, 0.0f);
  private static Vector3 shortStockTargetRotation = new Vector3(0.0f, -28f, 0.0f);
  private static Quaternion shortStockTargetQuaternion = Quaternion.Euler(ApplySimpleRotationPatch.shortStockTargetRotation);
  private static Vector3 shortStockTargetPostion = new Vector3(0.05f, 0.18f, -0.2f);
  private static Vector3 tacPistolTargetRotation = new Vector3(0.0f, -20f, 0.0f);
  private static Quaternion tacPistolTargetQuaternion = Quaternion.Euler(ApplySimpleRotationPatch.tacPistolTargetRotation);
  private static Vector3 tacPistolTargetPosition = new Vector3(-0.1f, 0.1f, -0.05f);
  private static Vector3 normalPistolTargetRotation = new Vector3(0.0f, -5f, 0.0f);
  private static Quaternion normalPistolTargetQuaternion = Quaternion.Euler(ApplySimpleRotationPatch.normalPistolTargetRotation);
  private static Vector3 normalPistolTargetPosition = new Vector3(-0.05f, 0.0f, 0.0f);

  protected virtual MethodBase GetTargetMethod()
  {
    ApplySimpleRotationPatch.aimSpeedField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_aimingSpeed");
    ApplySimpleRotationPatch.blindFireStrength = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_blindfireStrength");
    ApplySimpleRotationPatch.weaponPositionField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_temporaryPosition");
    ApplySimpleRotationPatch.scopeRotationField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_targetScopeRotation");
    ApplySimpleRotationPatch.weapRotationField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_temporaryRotation");
    ApplySimpleRotationPatch.isAimingField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_isAiming");
    ApplySimpleRotationPatch.currentRotationField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_cameraIdenity");
    ApplySimpleRotationPatch.firearmControllerField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_firearmController");
    ApplySimpleRotationPatch.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    return (MethodBase) typeof (ProceduralWeaponAnimation).GetMethod("ApplySimpleRotation", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPostfix]
  private static void Postfix(ProceduralWeaponAnimation __instance, float dt)
  {
    Player.FirearmController fc = (Player.FirearmController) ApplySimpleRotationPatch.firearmControllerField.GetValue((object) __instance);
    if (Object.op_Equality((Object) fc, (Object) null))
      return;
    Player player = (Player) ApplySimpleRotationPatch.playerField.GetValue((object) fc);
    if (!Object.op_Inequality((Object) player, (Object) null) || player.MovementContext.CurrentState.Name == 21)
      return;
    float num1 = (float) ApplySimpleRotationPatch.blindFireStrength.GetValue((object) __instance);
    Quaternion quaternion1 = (Quaternion) ApplySimpleRotationPatch.scopeRotationField.GetValue((object) __instance);
    Vector3 vector3_1 = (Vector3) ApplySimpleRotationPatch.weaponPositionField.GetValue((object) __instance);
    Quaternion quaternion2 = (Quaternion) ApplySimpleRotationPatch.weapRotationField.GetValue((object) __instance);
    if (player.IsYourPlayer)
    {
      StanceController.IsInThirdPerson = true;
      float num2 = (float) ApplySimpleRotationPatch.aimSpeedField.GetValue((object) __instance);
      bool flag1 = (bool) ApplySimpleRotationPatch.isAimingField.GetValue((object) __instance);
      bool flag2 = StanceController.CurrentStance == EStance.HighReady || StanceController.CurrentStance == EStance.LowReady || StanceController.CurrentStance == EStance.ShortStock || StanceController.CurrentStance == EStance.ActiveAiming || StanceController.CurrentStance == EStance.Melee;
      bool flag3 = StanceController.CurrentStance == EStance.ShortStock || StanceController.CurrentStance == EStance.ActiveAiming || StanceController.TreatWeaponAsPistolStance || StanceController.CurrentStance == EStance.Melee;
      bool flag4 = !(PluginConfig.RememberStanceFiring.Value & flag1) && StanceController.IsFiringFromStance && !flag3;
      bool flag5 = (flag2 || !StanceController.AllStancesReset || StanceController.CurrentStance == EStance.PistolCompressed) && !flag4;
      bool flag6 = PluginConfig.ActiveAimReload.Value && PlayerState.IsInReloadOpertation && !PlayerState.IsAttemptingToReloadInternalMag && !PlayerState.IsQuickReloading;
      bool flag7 = StanceController.CancelActiveAim && StanceController.CurrentStance == EStance.ActiveAiming && !flag6 || StanceController.CancelHighReady && StanceController.CurrentStance == EStance.HighReady || StanceController.CancelLowReady && StanceController.CurrentStance == EStance.LowReady || StanceController.CancelShortStock && StanceController.CurrentStance == EStance.ShortStock;
      StanceController.CurrentRotation = Quaternion.Slerp(StanceController.CurrentRotation, !__instance.IsAiming || !StanceController.AllStancesReset ? (flag5 ? StanceController.StanceRotation : Quaternion.identity) : quaternion1, flag5 ? StanceController.StanceRotationSpeed * PluginConfig.StanceRotationSpeedMulti.Value : (__instance.IsAiming ? 8f * num2 * dt : 8f * dt));
      __instance.HandsContainer.WeaponRootAnim.SetPositionAndRotation(vector3_1, Quaternion.op_Multiply(quaternion2, StanceController.CurrentRotation));
      if (StanceController.TreatWeaponAsPistolStance && PluginConfig.EnableAltPistol.Value)
      {
        if (StanceController.CurrentStance == EStance.PistolCompressed && !StanceController.IsAiming && !StanceController.IsResettingPistol && !StanceController.IsBlindFiring)
          ((Player.ValueBlender) StanceController.StanceBlender).Target = 1f;
        else
          ((Player.ValueBlender) StanceController.StanceBlender).Target = 0.0f;
        if (StanceController.CurrentStance != EStance.PistolCompressed && !StanceController.IsAiming && !StanceController.IsResettingPistol || StanceController.IsBlindFiring)
          StanceController.StanceTargetPosition = Vector3.Lerp(StanceController.StanceTargetPosition, Vector3.zero, 5f * dt);
        StanceController.HasResetActiveAim = true;
        StanceController.HasResetHighReady = true;
        StanceController.HasResetLowReady = true;
        StanceController.HasResetShortStock = true;
        StanceController.DoPistolStances(true, __instance, dt, player, fc, Vector3.zero);
      }
      else if (!StanceController.TreatWeaponAsPistolStance || WeaponStats.HasShoulderContact)
      {
        if (((!flag2 && StanceController.AllStancesReset || flag4 && !flag3 ? 1 : (StanceController.IsAiming ? 1 : 0)) | (flag7 ? 1 : 0)) != 0 || StanceController.IsBlindFiring)
          ((Player.ValueBlender) StanceController.StanceBlender).Target = 0.0f;
        else if (flag2)
          ((Player.ValueBlender) StanceController.StanceBlender).Target = 1f;
        if (!flag2 && StanceController.AllStancesReset && !flag4 && !StanceController.IsAiming || StanceController.IsBlindFiring)
          StanceController.StanceTargetPosition = Vector3.Lerp(StanceController.StanceTargetPosition, Vector3.zero, 5f * dt);
        StanceController.HasResetPistolPos = true;
        StanceController.DoRifleStances(player, fc, true, __instance, dt, Vector3.zero);
      }
    }
    else if (player.IsAI && !player.AIData.UseZombieSimpleAnimator)
    {
      Quaternion quaternion3 = Quaternion.identity;
      Quaternion quaternion4 = (Quaternion) ApplySimpleRotationPatch.currentRotationField.GetValue((object) __instance);
      ApplySimpleRotationPatch.aimSpeedField.SetValue((object) __instance, (object) 1f);
      FaceShieldComponent component1 = player.FaceShieldObserver.Component;
      NightVisionComponent component2 = player.NightVisionObserver.Component;
      bool flag8 = component2 != null && (component2.Togglable == null || component2.Togglable.On);
      bool flag9 = component1 != null && (component1.Togglable == null || component1.Togglable.On);
      float lastDist2Target = player.AIData.BotOwner.AimingManager.CurrentAiming.LastDist2Target;
      Vector3 vector3_2 = Vector3.op_Subtraction(player.AIData.BotOwner.AimingManager.CurrentAiming.RealTargetPoint, player.AIData.BotOwner.MyHead.position);
      float magnitude = ((Vector3) ref vector3_2).magnitude;
      bool flag10 = GClass3759.IndexOf<WildSpawnType>((IEnumerable<WildSpawnType>) StanceController._botsToUseTacticalStances, player.AIData.BotOwner.Profile.Info.Settings.Role) != -1;
      bool isPeace = player.AIData.BotOwner.Memory.IsPeace;
      bool flag11 = !player.AIData.BotOwner.ShootData.Shooting && (double) Time.time - (double) player.AIData.BotOwner.ShootData.LastTriggerPressd > 15.0;
      bool flag12 = false;
      float num3 = 1f;
      if (player.MovementContext.BlindFire == 0 && Object.op_Equality((Object) player.MovementContext.StationaryWeapon, (Object) null))
      {
        if (isPeace && !player.IsSprintEnabled && !__instance.IsAiming && !fc.IsInReloadOperation() && !((Player.AbstractHandsController) fc).IsInventoryOpen() && !((Player.AbstractHandsController) fc).IsInInteractionStrictCheck() && !fc.IsInSpawnOperation() && !((Player.AbstractHandsController) fc).IsHandsProcessing())
        {
          flag12 = true;
          player.MovementContext.SetPatrol(true);
        }
        else
        {
          player.MovementContext.SetPatrol(false);
          if (fc.Weapon.WeapClass != "pistol")
          {
            if (((flag10 || fc.IsInReloadOperation() || player.IsSprintEnabled ? 0 : (!__instance.IsAiming ? 1 : 0)) & (flag11 ? 1 : 0)) != 0 && ((double) lastDist2Target >= 25.0 || (double) lastDist2Target == 0.0))
            {
              flag12 = true;
              num3 = 12f * dt;
              quaternion3 = ApplySimpleRotationPatch.lowReadyTargetQuaternion;
              __instance.HandsContainer.HandsPosition.Zero = Vector3.op_Addition(__instance.PositionZeroSum, Vector3.op_Multiply(num1, ApplySimpleRotationPatch.lowReadyTargetPostion));
            }
            if (((!flag10 || fc.IsInReloadOperation() ? 0 : (!__instance.IsAiming ? 1 : 0)) & (flag11 ? 1 : 0)) != 0 && ((double) lastDist2Target >= 25.0 || (double) lastDist2Target == 0.0))
            {
              flag12 = true;
              player.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, 2f);
              num3 = 10.8f * dt;
              quaternion3 = ApplySimpleRotationPatch.highReadyTargetQuaternion;
              __instance.HandsContainer.HandsPosition.Zero = Vector3.op_Addition(__instance.PositionZeroSum, Vector3.op_Multiply(num1, ApplySimpleRotationPatch.highReadyTargetPostion));
            }
            else
              player.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, (float) ((Item) fc.Item).CalculateCellSize().X);
            if (flag10 && (flag8 | flag9 && !player.IsSprintEnabled && !fc.IsInReloadOperation() && (double) lastDist2Target < 25.0 && (double) lastDist2Target > 2.0 && (double) lastDist2Target != 0.0 || __instance.IsAiming && ((!flag8 ? 0 : (__instance.CurrentScope.IsOptic ? 1 : 0)) | (flag9 ? 1 : 0)) != 0))
            {
              flag12 = true;
              num3 = 6f * dt;
              quaternion3 = ApplySimpleRotationPatch.activeAimTargetQuaternion;
              __instance.HandsContainer.HandsPosition.Zero = Vector3.op_Addition(__instance.PositionZeroSum, Vector3.op_Multiply(num1, ApplySimpleRotationPatch.activeAimTargetPostion));
            }
            if (flag10 && !player.IsSprintEnabled && !fc.IsInReloadOperation() && (double) lastDist2Target <= 2.0 && (double) lastDist2Target != 0.0)
            {
              flag12 = true;
              num3 = 12f * dt;
              quaternion3 = ApplySimpleRotationPatch.shortStockTargetQuaternion;
              __instance.HandsContainer.HandsPosition.Zero = Vector3.op_Addition(__instance.PositionZeroSum, Vector3.op_Multiply(num1, ApplySimpleRotationPatch.shortStockTargetPostion));
            }
          }
          else
          {
            if (((flag10 || player.IsSprintEnabled ? 0 : (!__instance.IsAiming ? 1 : 0)) & (flag11 ? 1 : 0)) != 0)
            {
              flag12 = true;
              num3 = 6f * dt;
              quaternion3 = ApplySimpleRotationPatch.normalPistolTargetQuaternion;
              __instance.HandsContainer.HandsPosition.Zero = Vector3.op_Addition(__instance.PositionZeroSum, Vector3.op_Multiply(num1, ApplySimpleRotationPatch.normalPistolTargetPosition));
            }
            if (((!flag10 || player.IsSprintEnabled ? 0 : (!__instance.IsAiming ? 1 : 0)) & (flag11 ? 1 : 0)) != 0)
            {
              flag12 = true;
              num3 = 6f * dt;
              quaternion3 = ApplySimpleRotationPatch.tacPistolTargetQuaternion;
              __instance.HandsContainer.HandsPosition.Zero = Vector3.op_Addition(__instance.PositionZeroSum, Vector3.op_Multiply(num1, ApplySimpleRotationPatch.tacPistolTargetPosition));
            }
          }
        }
      }
      Quaternion quaternion5 = Quaternion.Slerp(quaternion4, !__instance.IsAiming || flag12 ? (flag12 ? quaternion3 : Quaternion.identity) : quaternion1, flag12 ? num3 : 8f * dt);
      __instance.HandsContainer.WeaponRootAnim.SetPositionAndRotation(vector3_1, Quaternion.op_Multiply(quaternion2, quaternion5));
      ApplySimpleRotationPatch.currentRotationField.SetValue((object) __instance, (object) quaternion5);
    }
  }
}
