// Decompiled with JetBrains decompiler
// Type: RealismMod.CamRecoilPatch
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

public class CamRecoilPatch : ModulePatch
{
  private static FieldInfo _cameraRecoilField;
  private static FieldInfo _cameraRecoilRotateField;
  private static FieldInfo _prevCameraTargetField;
  private static FieldInfo _headRotationField;
  private static FieldInfo _playerField;
  private static FieldInfo _fcField;
  private static float _camRecoilLerpTempSpeed = 1f;

  protected virtual MethodBase GetTargetMethod()
  {
    CamRecoilPatch._cameraRecoilField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_cameraRecoilLerpTempSpeed");
    CamRecoilPatch._cameraRecoilRotateField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_currentRecoilCameraRotate");
    CamRecoilPatch._prevCameraTargetField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_previousCameraTargetRotation");
    CamRecoilPatch._headRotationField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_headRotationVec");
    CamRecoilPatch._playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    CamRecoilPatch._fcField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_firearmController");
    return (MethodBase) typeof (ProceduralWeaponAnimation).GetMethod("method_19", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static bool PatchPrefix(ProceduralWeaponAnimation __instance, float deltaTime)
  {
    Player.FirearmController firearmController = (Player.FirearmController) CamRecoilPatch._fcField.GetValue((object) __instance);
    if (Object.op_Equality((Object) firearmController, (Object) null))
      return false;
    Player player = (Player) CamRecoilPatch._playerField.GetValue((object) firearmController);
    if (!Object.op_Inequality((Object) player, (Object) null) || !player.IsYourPlayer || player.MovementContext.CurrentState.Name == 21)
      return false;
    float num = (float) CamRecoilPatch._cameraRecoilField.GetValue((object) __instance);
    Quaternion quaternion1 = (Quaternion) CamRecoilPatch._cameraRecoilRotateField.GetValue((object) __instance);
    Quaternion quaternion2 = (Quaternion) CamRecoilPatch._prevCameraTargetField.GetValue((object) __instance);
    __instance.HandsContainer.CameraRotation.ReturnSpeed = 0.2f;
    __instance.HandsContainer.CameraRotation.Damping = 0.55f;
    if (Vector3.op_Inequality((Vector3) CamRecoilPatch._headRotationField.GetValue((object) __instance), Vector3.zero))
      return false;
    Vector3 current = ((RecoilProcessBase) __instance.Shootingg.CurrentRecoilEffect.HandRotationRecoilEffect).Current;
    bool autoFireOn;
    if ((autoFireOn = (__instance.Shootingg.CurrentRecoilEffect.HandRotationRecoilEffect as NewRotationRecoilProcess).AutoFireOn) & __instance.IsAiming && Vector3.op_Inequality(current, Vector3.zero))
    {
      if (!((RecoilProcessBase) __instance.Shootingg.CurrentRecoilEffect.HandRotationRecoilEffect).StableOn)
      {
        Quaternion localRotation = __instance.HandsContainer.CameraTransform.localRotation;
        localRotation.y = 0.0f;
        Quaternion quaternion3 = localRotation;
        CamRecoilPatch._camRecoilLerpTempSpeed = Mathf.Clamp(CamRecoilPatch._camRecoilLerpTempSpeed + __instance.CameraToWeaponAngleStep * deltaTime, __instance.CameraToWeaponAngleSpeedRange.x, __instance.CameraToWeaponAngleSpeedRange.y) * 2f;
        Quaternion quaternion4 = Quaternion.Lerp(quaternion1, quaternion3, CamRecoilPatch._camRecoilLerpTempSpeed);
        __instance.HandsContainer.CameraTransform.localRotation = quaternion4;
        CamRecoilPatch._prevCameraTargetField.SetValue((object) __instance, (object) quaternion3);
        CamRecoilPatch._cameraRecoilRotateField.SetValue((object) __instance, (object) quaternion4);
        return false;
      }
      Quaternion quaternion5 = quaternion2;
      quaternion5.z = __instance.HandsContainer.CameraTransform.localRotation.z;
      CamRecoilPatch._camRecoilLerpTempSpeed = Mathf.Clamp(CamRecoilPatch._camRecoilLerpTempSpeed + __instance.CameraToWeaponAngleStep * deltaTime, __instance.CameraToWeaponAngleSpeedRange.x, __instance.CameraToWeaponAngleSpeedRange.y) * 2f;
      Quaternion quaternion6 = Quaternion.Lerp(quaternion1, quaternion5, CamRecoilPatch._camRecoilLerpTempSpeed);
      __instance.HandsContainer.CameraTransform.localRotation = quaternion6;
      CamRecoilPatch._cameraRecoilRotateField.SetValue((object) __instance, (object) quaternion6);
      return false;
    }
    if (!autoFireOn & __instance.IsAiming)
    {
      Quaternion localRotation = __instance.HandsContainer.CameraTransform.localRotation;
      localRotation.y = 0.0f;
      Quaternion quaternion7 = localRotation;
      CamRecoilPatch._camRecoilLerpTempSpeed = Mathf.Clamp(CamRecoilPatch._camRecoilLerpTempSpeed - __instance.CameraToWeaponAngleStep * deltaTime, __instance.CameraToWeaponAngleSpeedRange.x, __instance.CameraToWeaponAngleSpeedRange.y);
      Quaternion quaternion8 = Quaternion.Lerp(quaternion1, quaternion7, CamRecoilPatch._camRecoilLerpTempSpeed * 2f);
      __instance.HandsContainer.CameraTransform.localRotation = quaternion8;
      CamRecoilPatch._prevCameraTargetField.SetValue((object) __instance, (object) quaternion7);
      CamRecoilPatch._cameraRecoilRotateField.SetValue((object) __instance, (object) quaternion8);
      return false;
    }
    if (!__instance.IsAiming)
    {
      Quaternion localRotation = __instance.HandsContainer.CameraTransform.localRotation;
      CamRecoilPatch._camRecoilLerpTempSpeed = Mathf.Clamp(CamRecoilPatch._camRecoilLerpTempSpeed - __instance.CameraToWeaponAngleStep * deltaTime, __instance.CameraToWeaponAngleSpeedRange.x, __instance.CameraToWeaponAngleSpeedRange.y) * 2f;
      CamRecoilPatch._cameraRecoilField.SetValue((object) __instance, (object) CamRecoilPatch._camRecoilLerpTempSpeed);
      Quaternion quaternion9 = Quaternion.Lerp(quaternion1, localRotation, CamRecoilPatch._camRecoilLerpTempSpeed);
      __instance.HandsContainer.CameraTransform.localRotation = quaternion9;
      CamRecoilPatch._prevCameraTargetField.SetValue((object) __instance, (object) localRotation);
      CamRecoilPatch._cameraRecoilRotateField.SetValue((object) __instance, (object) quaternion9);
    }
    return false;
  }
}
