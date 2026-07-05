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

    if (flag1) totalPosOffset += HandBreathController.GetModifiedHandPosForBreath(player);
    if (flag2) totalPosOffset += HandPoseController.GetModifiedHandPosWithPose(player);
    if (flag3)
    {
      totalPosOffset += HandPoseController.GetModifiedHandPosWithPoseChange(player);
      totalRotOffset *= HandPoseController.GetModifiedHandRotWithPoseChange();
    }
    if (flag4) totalPosOffset += HandShakeController.GetHandsShakePosition(player);
    if (flag5)
    {
      totalPosOffset += HandMovWithRotController.GetModifiedHandPosZMovement(player);
      totalPosOffset += HandMovWithRotController.GetModifiedHandPosForLoweredWeapon(player);
    }
    if (flag7)
    {
      Vector3 localPosition = __instance.HandsContainer.WeaponRoot.localPosition;
      Quaternion localRotation = __instance.HandsContainer.WeaponRoot.localRotation;
      ParallaxController.GetModifiedHandPosRotParallax(player, ref localPosition, ref localRotation);
      totalPosOffset += localPosition;
      totalRotOffset *= localRotation;
    }
    if (flag6)
    {
      totalPosOffset += FootstepController.GetModifiedHandPosFootstep;
      totalPosOffset += FootstepController.GetSideToSidePosition();
      totalRotOffset *= FootstepController.GetSideToSideRotation();
    }
    if (flag8)
    {
      totalPosOffset += NewSwayController.GetNewSwayPosition();
      totalRotOffset *= NewSwayController.GetNewSwayRotation();
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

