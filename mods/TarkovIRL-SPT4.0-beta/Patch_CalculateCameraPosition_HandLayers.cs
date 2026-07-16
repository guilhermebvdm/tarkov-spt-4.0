using EFT;
using EFT.Animations;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal class Patch_CalculateCameraPosition_HandLayers : ModulePatch
{
  private static FieldInfo playerField;
  private static FieldInfo fcField;
  private static ConditionalWeakTable<ProceduralWeaponAnimation, Player> _playerCache = new ConditionalWeakTable<ProceduralWeaponAnimation, Player>();
  private static Transform _lastWeaponRoot = (Transform) null;
  private static Vector3 _originalLocalPosition = Vector3.zero;
  private static Quaternion _originalLocalRotation = Quaternion.identity;
  private static float _rightShoulderTimer = 0f;
  private static bool _wasLeftShoulder = false;

  protected override MethodBase GetTargetMethod()
  {
    Patch_CalculateCameraPosition_HandLayers.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    Patch_CalculateCameraPosition_HandLayers.fcField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_firearmController");
    return (MethodBase) typeof (ProceduralWeaponAnimation).GetMethod("CalculateCameraPosition", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(ProceduralWeaponAnimation __instance)
  {
    if ((__instance == null))
      return;
    Player player;
    if (!Patch_CalculateCameraPosition_HandLayers._playerCache.TryGetValue(__instance, out player))
    {
      Player.FirearmController firearmController = (Player.FirearmController) Patch_CalculateCameraPosition_HandLayers.fcField.GetValue((object) __instance);
      if ((firearmController != null))
        player = (Player) Patch_CalculateCameraPosition_HandLayers.playerField.GetValue((object) firearmController);
      if ((player != null))
        Patch_CalculateCameraPosition_HandLayers._playerCache.Add(__instance, player);
    }
    if ((player == null) || !player.IsYourPlayer)
      return;
    WeaponController.IsUsingMounted = __instance.IsMountedState;
    Transform weaponRoot = __instance.HandsContainer.WeaponRoot;
    if ((Patch_CalculateCameraPosition_HandLayers._lastWeaponRoot == weaponRoot))
    {
      weaponRoot.localPosition = Patch_CalculateCameraPosition_HandLayers._originalLocalPosition;
      weaponRoot.localRotation = Patch_CalculateCameraPosition_HandLayers._originalLocalRotation;
    }
    else
      Patch_CalculateCameraPosition_HandLayers._lastWeaponRoot = weaponRoot;
    Patch_CalculateCameraPosition_HandLayers._originalLocalPosition = weaponRoot.localPosition;
    Patch_CalculateCameraPosition_HandLayers._originalLocalRotation = weaponRoot.localRotation;
    if (AnimStateController.IsBlindfire || WeaponController.IsUsingMounted || !PrimeMover.EnableMod.Value)
    {
      Patch_CalculateCameraPosition_HandLayers._originalLocalPosition = Vector3.zero;
      Patch_CalculateCameraPosition_HandLayers._originalLocalRotation = Quaternion.identity;
    }
    else
    {
      EfficiencyController.UpdateEfficiency(player);
      bool flag1 = PrimeMover.IsBreathingEffect.Value;
      bool flag2 = PrimeMover.IsPoseEffect.Value;
      bool flag3 = PrimeMover.IsPoseChangeEffect.Value;
      bool flag4 = PrimeMover.IsArmShakeEffect.Value;
      bool flag5 = PrimeMover.IsSmallMovementsEffect.Value;
      bool flag6 = PrimeMover.IsFootstepEffect.Value;
      bool flag7 = PrimeMover.IsParallaxEffect.Value;
      bool flag8 = PrimeMover.IsWeaponSway.Value;

      bool isLeftShoulder = StanceController.IsLeftShoulder;
      if (isLeftShoulder)
      {
        _wasLeftShoulder = true;
      }
      else if (_wasLeftShoulder)
      {
        _wasLeftShoulder = false;
        _rightShoulderTimer = Time.time + 0.5f;
      }
      bool blockSwayAndFreeAim = isLeftShoulder || (Time.time < _rightShoulderTimer);

      if (blockSwayAndFreeAim)
      {
        flag1 = false;
        flag2 = false;
        flag3 = false;
        flag4 = false;
        flag5 = false;
        flag6 = false;
        flag7 = false;
        flag8 = false;
      }

      Vector3 vector3_1 = Vector3.zero;
      Quaternion quaternion1 = Quaternion.identity;
      if (flag1)
        vector3_1 = (vector3_1 + HandBreathController.GetModifiedHandPosForBreath(player));
      if (flag2)
        vector3_1 = (vector3_1 + HandPoseController.GetModifiedHandPosWithPose(player));
      if (flag3)
      {
        vector3_1 = (vector3_1 + HandPoseController.GetModifiedHandPosWithPoseChange(player));
        quaternion1 = (quaternion1 * HandPoseController.GetModifiedHandRotWithPoseChange());
      }
      if (flag4)
        vector3_1 = (vector3_1 + HandShakeController.GetHandsShakePosition(player));
      if (flag5)
        vector3_1 = vector3_1 + HandMovWithRotController.GetModifiedHandPosZMovement(player) + HandMovWithRotController.GetModifiedHandPosForLoweredWeapon(player);
      if (flag7)
      {
        Vector3 localPosition = __instance.HandsContainer.WeaponRoot.localPosition;
        Quaternion localRotation = __instance.HandsContainer.WeaponRoot.localRotation;
        ParallaxController.GetModifiedHandPosRotParallax(player, ref localPosition, ref localRotation);
        vector3_1 = (vector3_1 + localPosition);
        quaternion1 = (quaternion1 * localRotation);
      }
      if (flag6)
      {
        vector3_1 = vector3_1 + FootstepController.GetModifiedHandPosFootstep + FootstepController.GetSideToSidePosition();
        quaternion1 = (quaternion1 * FootstepController.GetSideToSideRotation());
      }
      if (flag8)
      {
        vector3_1 = (vector3_1 + NewSwayController.GetNewSwayPosition());
        quaternion1 = (quaternion1 * NewSwayController.GetNewSwayRotation());
      }
      Vector3 position = Vector3.zero;
      Quaternion rotation = Quaternion.identity;
      if (!blockSwayAndFreeAim)
      {
        DirectionalSwayController.GetDirectionalSway(out position, out rotation);
      }
      Vector3 vector3_2 = (vector3_1 + position);
      Quaternion quaternion2 = (quaternion1 * rotation);
      Vector3 pos;
      Quaternion rot;
      WeaponSelectionController.GetWeaponSelectionTransforms(out pos, out rot);
      Vector3 vector3_3 = (vector3_2 + pos);
      Quaternion quaternion3 = (quaternion2 * rot);
      Vector3 vector3_4 = (vector3_3 + new Vector3(PrimeMover.DebugPosX.Value, PrimeMover.DebugPosY.Value, PrimeMover.DebugPosZ.Value));
      Quaternion quaternion4 = (quaternion3 * Quaternion.Euler(PrimeMover.DebugRotX.Value, PrimeMover.DebugRotY.Value, PrimeMover.DebugRotZ.Value));
      Vector3 posOffset = Vector3.zero;
      Quaternion rotOffset = Quaternion.identity;
      if (!blockSwayAndFreeAim)
      {
        FreeAimController.GetOffsets(out posOffset, out rotOffset);
      }
      Vector3 vector3_5 = (vector3_4 + posOffset);
      Quaternion quaternion5 = (quaternion4 * rotOffset);
      Transform transform1 = weaponRoot;
      transform1.localPosition = (transform1.localPosition + vector3_5);
      Transform transform2 = weaponRoot;
      transform2.localRotation = (transform2.localRotation * quaternion5);
    }
  }
}


