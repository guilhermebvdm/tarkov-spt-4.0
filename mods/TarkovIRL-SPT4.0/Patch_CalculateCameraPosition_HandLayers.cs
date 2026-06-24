// Decompiled with JetBrains decompiler
// Type: TarkovIRL.Patch_CalculateCameraPosition_HandLayers
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using EFT;
using EFT.Animations;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal class Patch_CalculateCameraPosition_HandLayers : ModulePatch
{
  private static FieldInfo playerField;
  private static FieldInfo fcField;

  protected virtual MethodBase GetTargetMethod()
  {
    Patch_CalculateCameraPosition_HandLayers.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    Patch_CalculateCameraPosition_HandLayers.fcField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_firearmController");
    return (MethodBase) typeof (ProceduralWeaponAnimation).GetMethod("CalculateCameraPosition", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(ProceduralWeaponAnimation __instance)
  {
    if (Object.op_Equality((Object) __instance, (Object) null))
      return;
    Player.FirearmController firearmController = (Player.FirearmController) Patch_CalculateCameraPosition_HandLayers.fcField.GetValue((object) __instance);
    if (Object.op_Equality((Object) firearmController, (Object) null))
      return;
    Player player = (Player) Patch_CalculateCameraPosition_HandLayers.playerField.GetValue((object) firearmController);
    if (!Object.op_Inequality((Object) player, (Object) null) || !player.IsYourPlayer)
      return;
    WeaponController.IsUsingMounted = __instance.IsMountedState;
    if (AnimStateController.IsBlindfire || WeaponController.IsUsingMounted)
      return;
    EfficiencyController.UpdateEfficiency(player);
    Vector3 handPosForBreath = HandBreathController.GetModifiedHandPosForBreath(player);
    Vector3 handsShakePosition = HandShakeController.GetHandsShakePosition(player);
    Vector3 modifiedHandPosWithPose = HandPoseController.GetModifiedHandPosWithPose(player);
    Quaternion rotWithPoseChange = HandPoseController.GetModifiedHandRotWithPoseChange();
    Vector3 posWithPoseChange = HandPoseController.GetModifiedHandPosWithPoseChange(player);
    Vector3 handPosZmovement = HandMovWithRotController.GetModifiedHandPosZMovement(player);
    Vector3 forLoweredWeapon = HandMovWithRotController.GetModifiedHandPosForLoweredWeapon(player);
    Vector3 modifiedHandPosFootstep = FootstepController.GetModifiedHandPosFootstep;
    Vector3 localPosition = __instance.HandsContainer.WeaponRoot.localPosition;
    Quaternion localRotation = __instance.HandsContainer.WeaponRoot.localRotation;
    ParallaxController.GetModifiedHandPosRotParallax(player, ref localPosition, ref localRotation);
    Vector3 newSwayPosition = NewSwayController.GetNewSwayPosition();
    Quaternion newSwayRotation = NewSwayController.GetNewSwayRotation();
    Vector3 sideToSidePosition = FootstepController.GetSideToSidePosition();
    Quaternion sideToSideRotation = FootstepController.GetSideToSideRotation();
    bool flag1 = PrimeMover.IsBreathingEffect.Value;
    bool flag2 = PrimeMover.IsPoseEffect.Value;
    bool flag3 = PrimeMover.IsPoseChangeEffect.Value;
    bool flag4 = PrimeMover.IsArmShakeEffect.Value;
    bool flag5 = PrimeMover.IsSmallMovementsEffect.Value;
    bool flag6 = PrimeMover.IsFootstepEffect.Value;
    bool flag7 = PrimeMover.IsParallaxEffect.Value;
    bool flag8 = PrimeMover.IsWeaponSway.Value;
    if (flag1)
    {
      Transform weaponRoot = __instance.HandsContainer.WeaponRoot;
      weaponRoot.localPosition = Vector3.op_Addition(weaponRoot.localPosition, handPosForBreath);
    }
    if (flag2)
    {
      Transform weaponRoot = __instance.HandsContainer.WeaponRoot;
      weaponRoot.localPosition = Vector3.op_Addition(weaponRoot.localPosition, modifiedHandPosWithPose);
    }
    if (flag3)
    {
      Transform weaponRoot1 = __instance.HandsContainer.WeaponRoot;
      weaponRoot1.localPosition = Vector3.op_Addition(weaponRoot1.localPosition, posWithPoseChange);
      Transform weaponRoot2 = __instance.HandsContainer.WeaponRoot;
      weaponRoot2.localRotation = Quaternion.op_Multiply(weaponRoot2.localRotation, rotWithPoseChange);
    }
    if (flag4)
    {
      Transform weaponRoot = __instance.HandsContainer.WeaponRoot;
      weaponRoot.localPosition = Vector3.op_Addition(weaponRoot.localPosition, handsShakePosition);
    }
    if (flag5)
    {
      Transform weaponRoot3 = __instance.HandsContainer.WeaponRoot;
      weaponRoot3.localPosition = Vector3.op_Addition(weaponRoot3.localPosition, handPosZmovement);
      Transform weaponRoot4 = __instance.HandsContainer.WeaponRoot;
      weaponRoot4.localPosition = Vector3.op_Addition(weaponRoot4.localPosition, forLoweredWeapon);
    }
    if (flag7)
    {
      Transform weaponRoot5 = __instance.HandsContainer.WeaponRoot;
      weaponRoot5.localPosition = Vector3.op_Addition(weaponRoot5.localPosition, localPosition);
      Transform weaponRoot6 = __instance.HandsContainer.WeaponRoot;
      weaponRoot6.localRotation = Quaternion.op_Multiply(weaponRoot6.localRotation, localRotation);
    }
    if (flag6)
    {
      Transform weaponRoot7 = __instance.HandsContainer.WeaponRoot;
      weaponRoot7.localPosition = Vector3.op_Addition(weaponRoot7.localPosition, modifiedHandPosFootstep);
      Transform weaponRoot8 = __instance.HandsContainer.WeaponRoot;
      weaponRoot8.localPosition = Vector3.op_Addition(weaponRoot8.localPosition, sideToSidePosition);
      Transform weaponRoot9 = __instance.HandsContainer.WeaponRoot;
      weaponRoot9.localRotation = Quaternion.op_Multiply(weaponRoot9.localRotation, sideToSideRotation);
    }
    if (flag8)
    {
      Transform weaponRoot10 = __instance.HandsContainer.WeaponRoot;
      weaponRoot10.localPosition = Vector3.op_Addition(weaponRoot10.localPosition, newSwayPosition);
      Transform weaponRoot11 = __instance.HandsContainer.WeaponRoot;
      weaponRoot11.localRotation = Quaternion.op_Multiply(weaponRoot11.localRotation, newSwayRotation);
    }
    Vector3 position;
    Quaternion rotation;
    DirectionalSwayController.GetDirectionalSway(out position, out rotation);
    Transform weaponRoot12 = __instance.HandsContainer.WeaponRoot;
    weaponRoot12.localPosition = Vector3.op_Addition(weaponRoot12.localPosition, position);
    Transform weaponRoot13 = __instance.HandsContainer.WeaponRoot;
    weaponRoot13.localRotation = Quaternion.op_Multiply(weaponRoot13.localRotation, rotation);
    Vector3 pos;
    Quaternion rot;
    WeaponSelectionController.GetWeaponSelectionTransforms(out pos, out rot);
    Transform weaponRoot14 = __instance.HandsContainer.WeaponRoot;
    weaponRoot14.localPosition = Vector3.op_Addition(weaponRoot14.localPosition, pos);
    Transform weaponRoot15 = __instance.HandsContainer.WeaponRoot;
    weaponRoot15.localRotation = Quaternion.op_Multiply(weaponRoot15.localRotation, rot);
  }
}
