// Decompiled with JetBrains decompiler
// Type: RealismMod.ApplyComplexRotationPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.Animations;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class ApplyComplexRotationPatch : ModulePatch
{
  private static FieldInfo _aimSpeedField;
  private static FieldInfo _compensatoryField;
  private static FieldInfo _displacementStrField;
  private static FieldInfo _scopeRotationField;
  private static FieldInfo _weapTempRotationField;
  private static FieldInfo _weapTempPositionField;
  private static FieldInfo _isAimingField;
  private static FieldInfo _firearmControllerField;
  private static FieldInfo _playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    ApplyComplexRotationPatch._aimSpeedField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_aimingSpeed");
    ApplyComplexRotationPatch._compensatoryField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_compensatoryScale");
    ApplyComplexRotationPatch._displacementStrField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_displacementStr");
    ApplyComplexRotationPatch._scopeRotationField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_targetScopeRotation");
    ApplyComplexRotationPatch._weapTempPositionField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_temporaryPosition");
    ApplyComplexRotationPatch._weapTempRotationField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_temporaryRotation");
    ApplyComplexRotationPatch._isAimingField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_isAiming");
    ApplyComplexRotationPatch._firearmControllerField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_firearmController");
    ApplyComplexRotationPatch._playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    return (MethodBase) typeof (ProceduralWeaponAnimation).GetMethod("ApplyComplexRotation", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPostfix]
  private static void Postfix(
    ProceduralWeaponAnimation __instance,
    float dt,
    Vector3 ____vCameraTarget)
  {
    Player.FirearmController firearmController = (Player.FirearmController) ApplyComplexRotationPatch._firearmControllerField.GetValue((object) __instance);
    if (Object.op_Equality((Object) firearmController, (Object) null))
      return;
    Player player = (Player) ApplyComplexRotationPatch._playerField.GetValue((object) firearmController);
    if (!Object.op_Inequality((Object) player, (Object) null) || !player.IsYourPlayer || player.MovementContext.CurrentState.Name == 21)
      return;
    Player.FirearmController handsController = player.HandsController as Player.FirearmController;
    StanceController.IsInThirdPerson = false;
    float num1 = (float) ApplyComplexRotationPatch._aimSpeedField.GetValue((object) __instance);
    float num2 = (float) ApplyComplexRotationPatch._compensatoryField.GetValue((object) __instance);
    float num3 = (float) ApplyComplexRotationPatch._displacementStrField.GetValue((object) __instance);
    Quaternion quaternion = (Quaternion) ApplyComplexRotationPatch._scopeRotationField.GetValue((object) __instance);
    Vector3 vector3_1 = (Vector3) ApplyComplexRotationPatch._weapTempPositionField.GetValue((object) __instance);
    Quaternion weapRotation = (Quaternion) ApplyComplexRotationPatch._weapTempRotationField.GetValue((object) __instance);
    bool flag1 = (bool) ApplyComplexRotationPatch._isAimingField.GetValue((object) __instance);
    Vector3 vector3_2 = __instance.HandsContainer.HandsRotation.Get();
    Vector3 vector3_3 = __instance.HandsContainer.SwaySpring.Value;
    Vector3.op_Addition(Vector3.op_Addition(vector3_2, Vector3.op_Multiply(num3 * (flag1 ? __instance.AimingDisplacementStr : 1f), new Vector3(vector3_3.x, 0.0f, vector3_3.z))), vector3_3);
    Vector3 vector3_4 = __instance._shouldMoveWeaponCloser ? __instance.HandsContainer.RotationCenterWoStock : __instance.HandsContainer.RotationCenter;
    __instance.HandsContainer.WeaponRootAnim.TransformPoint(vector3_4);
    bool flag2 = StanceController.CurrentStance == EStance.HighReady || StanceController.CurrentStance == EStance.LowReady || StanceController.CurrentStance == EStance.ShortStock || StanceController.CurrentStance == EStance.ActiveAiming || StanceController.CurrentStance == EStance.Melee || StanceController.IsLeftShoulder;
    bool flag3 = StanceController.CurrentStance == EStance.ShortStock || StanceController.CurrentStance == EStance.ActiveAiming || StanceController.TreatWeaponAsPistolStance || StanceController.CurrentStance == EStance.Melee;
    bool flag4 = PluginConfig.RememberStanceFiring.Value && !flag1 && StanceController.IsFiringFromStance && !flag3;
    bool flag5 = (flag2 || !StanceController.AllStancesReset || StanceController.CurrentStance == EStance.PistolCompressed) && !flag4;
    bool flag6 = PluginConfig.ActiveAimReload.Value && PlayerState.IsInReloadOpertation && !PlayerState.IsAttemptingToReloadInternalMag && !PlayerState.IsQuickReloading;
    bool flag7 = StanceController.CancelActiveAim && StanceController.CurrentStance == EStance.ActiveAiming && !flag6 || StanceController.CancelHighReady && StanceController.CurrentStance == EStance.HighReady || StanceController.CancelLowReady && StanceController.CurrentStance == EStance.LowReady || StanceController.CancelShortStock && StanceController.CurrentStance == EStance.ShortStock;
    StanceController.CurrentRotation = Quaternion.Slerp(StanceController.CurrentRotation, !__instance.IsAiming || !StanceController.AllStancesReset ? (flag5 ? StanceController.StanceRotation : Quaternion.identity) : Quaternion.identity, flag5 ? StanceController.StanceRotationSpeed * PluginConfig.StanceRotationSpeedMulti.Value : (__instance.IsAiming ? 7f * num1 * dt : 8f * dt));
    ShootController.DoVisualRecoil(ref weapRotation);
    __instance.HandsContainer.WeaponRootAnim.SetPositionAndRotation(vector3_1, Quaternion.op_Multiply(weapRotation, StanceController.CurrentRotation));
    if (!Plugin.ServerConfig.enable_stances)
      return;
    if (StanceController.TreatWeaponAsPistolStance)
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
      StanceController.HasResetMelee = true;
      StanceController.DoPistolStances(false, __instance, dt, player, handsController, ____vCameraTarget);
    }
    else if (!StanceController.TreatWeaponAsPistolStance || WeaponStats.HasShoulderContact)
    {
      if (((!flag2 && StanceController.AllStancesReset || flag4 && !flag3 ? 1 : (StanceController.IsAiming ? 1 : 0)) | (flag7 ? 1 : 0)) != 0 || StanceController.IsBlindFiring || StanceController.IsLeftShoulder)
        ((Player.ValueBlender) StanceController.StanceBlender).Target = 0.0f;
      else if (flag2)
        ((Player.ValueBlender) StanceController.StanceBlender).Target = 1f;
      if (!flag2 && StanceController.AllStancesReset && !flag4 && !StanceController.IsAiming || StanceController.IsBlindFiring || StanceController.IsLeftShoulder)
        StanceController.StanceTargetPosition = Vector3.Lerp(StanceController.StanceTargetPosition, Vector3.zero, 5f * dt);
      StanceController.HasResetPistolPos = true;
      StanceController.DoRifleStances(player, handsController, false, __instance, dt, ____vCameraTarget);
    }
    if (PluginConfig.EnableExtraProcEffects.Value)
      StanceController.DoExtraPosAndRot(__instance, player);
    StanceController.DoPatrolStance(__instance, player);
  }
}
