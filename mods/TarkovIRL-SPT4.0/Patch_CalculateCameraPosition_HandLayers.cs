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
  private static System.Runtime.CompilerServices.ConditionalWeakTable<ProceduralWeaponAnimation, Player> _playerCache = new System.Runtime.CompilerServices.ConditionalWeakTable<ProceduralWeaponAnimation, Player>();
  private static Transform _lastWeaponRoot = null;
  private static Vector3 _lastTotalPosOffset = Vector3.zero;
  private static Quaternion _lastTotalRotOffset = Quaternion.identity;

  protected override MethodBase GetTargetMethod()
  {
    Patch_CalculateCameraPosition_HandLayers.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    Patch_CalculateCameraPosition_HandLayers.fcField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_firearmController");
    return (MethodBase) typeof (ProceduralWeaponAnimation).GetMethod("CalculateCameraPosition", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(ProceduralWeaponAnimation __instance)
  {
    if (__instance == null)
      return;
    Player player;
    if (!_playerCache.TryGetValue(__instance, out player))
    {
        Player.FirearmController firearmController = (Player.FirearmController) Patch_CalculateCameraPosition_HandLayers.fcField.GetValue((object) __instance);
        if (firearmController != null)
            player = (Player) Patch_CalculateCameraPosition_HandLayers.playerField.GetValue((object) firearmController);
        
        if (player != null)
            _playerCache.Add(__instance, player);
    }

    if (player == null || !player.IsYourPlayer)
      return;
    WeaponController.IsUsingMounted = __instance.IsMountedState;

    Transform currentWeaponRoot = __instance.HandsContainer.WeaponRoot;
    if (Patch_CalculateCameraPosition_HandLayers._lastWeaponRoot == currentWeaponRoot)
    {
        currentWeaponRoot.localPosition -= Patch_CalculateCameraPosition_HandLayers._lastTotalPosOffset;
        currentWeaponRoot.localRotation *= Quaternion.Inverse(Patch_CalculateCameraPosition_HandLayers._lastTotalRotOffset);
    }
    Patch_CalculateCameraPosition_HandLayers._lastWeaponRoot = currentWeaponRoot;

    if (AnimStateController.IsBlindfire || WeaponController.IsUsingMounted)
    {
        Patch_CalculateCameraPosition_HandLayers._lastTotalPosOffset = Vector3.zero;
        Patch_CalculateCameraPosition_HandLayers._lastTotalRotOffset = Quaternion.identity;
        return;
    }
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
    Vector3 totalPosOffset = Vector3.zero;
    Quaternion totalRotOffset = Quaternion.identity;

    if (flag1) totalPosOffset += handPosForBreath;
    if (flag2) totalPosOffset += modifiedHandPosWithPose;
    if (flag3)
    {
      totalPosOffset += posWithPoseChange;
      totalRotOffset *= rotWithPoseChange;
    }
    if (flag4) totalPosOffset += handsShakePosition;
    if (flag5)
    {
      totalPosOffset += handPosZmovement;
      totalPosOffset += forLoweredWeapon;
    }
    if (flag7)
    {
      totalPosOffset += localPosition;
      totalRotOffset *= localRotation;
    }
    if (flag6)
    {
      totalPosOffset += modifiedHandPosFootstep;
      totalPosOffset += sideToSidePosition;
      totalRotOffset *= sideToSideRotation;
    }
    if (flag8)
    {
      totalPosOffset += newSwayPosition;
      totalRotOffset *= newSwayRotation;
    }

    Vector3 position;
    Quaternion rotation;
    DirectionalSwayController.GetDirectionalSway(out position, out rotation);
    totalPosOffset += position;
    totalRotOffset *= rotation;

    Vector3 pos;
    Quaternion rot;
    WeaponSelectionController.GetWeaponSelectionTransforms(out pos, out rot);
    totalPosOffset += pos;
    totalRotOffset *= rot;

    currentWeaponRoot.localPosition += totalPosOffset;
    currentWeaponRoot.localRotation *= totalRotOffset;

    Patch_CalculateCameraPosition_HandLayers._lastTotalPosOffset = totalPosOffset;
    Patch_CalculateCameraPosition_HandLayers._lastTotalRotOffset = totalRotOffset;
  }
}

