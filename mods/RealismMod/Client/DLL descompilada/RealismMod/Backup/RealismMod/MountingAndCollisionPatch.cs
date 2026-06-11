// Decompiled with JetBrains decompiler
// Type: RealismMod.MountingAndCollisionPatch
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

public class MountingAndCollisionPatch : ModulePatch
{
  private static FieldInfo _playerField;
  private static FieldInfo _fcField;
  private static FieldInfo _blendField;
  private static FieldInfo _smoothInField;
  private static FieldInfo _smoothOutField;
  private static float _mountClamp = 0.0f;
  private static float _collidingModifier = 1f;
  private static float _finalStateEndTimer = 0.0f;
  private static float _finalStateDelayTimer = 0.0f;
  private static bool _delayFinalState = false;
  private static float _adsResetTimer = 0.0f;
  private static float _collisionOverrideTimer = 0.0f;
  private static float _collisionTimer = 0.0f;
  private static float _collisionResetTimer = 0.0f;
  private static float _previousOverlapValue = 0.0f;
  private static float _currentOverlapValue = 0.0f;
  private static float _smoothedOverlapValue = 0.0f;
  private static float _lastDistance = 0.0f;
  private static bool _isColliding = false;
  private static bool _wasInFinalState = false;
  private static float _stanceFactor = 0.0f;
  private static float _stanceInverseFactor = 0.0f;
  private static Vector3 _initialPos = new Vector3(0.01f, -0.075f, -0.13f);
  private static Vector3 _initialRot = new Vector3(-0.025f, 0.005f, 0.005f);
  private static Vector3 _finalPos = Vector3.zero;
  private static Vector3 _finalRot = Vector3.zero;
  private static Vector3 _collisionPos = Vector3.zero;
  private static Vector3 _collisionRot = Vector3.zero;

  protected virtual MethodBase GetTargetMethod()
  {
    MountingAndCollisionPatch._playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    MountingAndCollisionPatch._fcField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_firearmController");
    MountingAndCollisionPatch._blendField = AccessTools.Field(typeof (TurnAwayEffector), "_blendSpeed");
    MountingAndCollisionPatch._smoothInField = AccessTools.Field(typeof (TurnAwayEffector), "_inSmoothTime");
    MountingAndCollisionPatch._smoothOutField = AccessTools.Field(typeof (TurnAwayEffector), "_outSmoothTime");
    return (MethodBase) typeof (ProceduralWeaponAnimation).GetMethod("AvoidObstacles", BindingFlags.Instance | BindingFlags.Public);
  }

  private static void DoMounting(Player player, ProceduralWeaponAnimation pwa)
  {
    MountingAndCollisionPatch._mountClamp = !StanceController.IsMounting ? Mathf.Lerp(MountingAndCollisionPatch._mountClamp, 0.0f, 0.1f) : Mathf.Lerp(MountingAndCollisionPatch._mountClamp, 2.5f, 0.1f);
    float pivotPoint = WeaponStats.BipodIsDeployed ? 1.5f : 0.75f;
    float aimPivot = WeaponStats.BipodIsDeployed ? 0.15f : 0.25f;
    StanceController.MountingPivotUpdate(player, pwa, MountingAndCollisionPatch._mountClamp, StanceController.GetDeltaTime(), pivotPoint, aimPivot);
  }

  private static void ModifyBSGCollisions(
    ProceduralWeaponAnimation pwa,
    Player.FirearmController fc)
  {
    MountingAndCollisionPatch._currentOverlapValue = fc.OverlapValue;
    float num1 = 0.1f;
    MountingAndCollisionPatch._smoothedOverlapValue += num1 * (MountingAndCollisionPatch._currentOverlapValue - MountingAndCollisionPatch._smoothedOverlapValue);
    bool flag1 = (double) MountingAndCollisionPatch._smoothedOverlapValue > (double) MountingAndCollisionPatch._previousOverlapValue;
    bool flag2 = (double) MountingAndCollisionPatch._smoothedOverlapValue < (double) MountingAndCollisionPatch._previousOverlapValue;
    bool flag3 = Utils.AreFloatsEqual(MountingAndCollisionPatch._smoothedOverlapValue, MountingAndCollisionPatch._previousOverlapValue);
    float num2 = 0.1f * Time.deltaTime;
    float num3 = 0.2f * Time.deltaTime;
    float num4 = 0.15f;
    float num5 = 2.5f;
    float num6 = 0.15f;
    if (flag3)
    {
      MountingAndCollisionPatch._collisionTimer = 0.0f;
      MountingAndCollisionPatch._collisionResetTimer = 0.0f;
      MountingAndCollisionPatch._collidingModifier = Mathf.MoveTowards(MountingAndCollisionPatch._collidingModifier, 1f, num2);
    }
    else if (flag1)
    {
      MountingAndCollisionPatch._collisionTimer += Time.deltaTime;
      MountingAndCollisionPatch._collidingModifier = (double) MountingAndCollisionPatch._collisionTimer > (double) num6 ? Mathf.MoveTowards(MountingAndCollisionPatch._collidingModifier, 1f, num2) : Mathf.MoveTowards(MountingAndCollisionPatch._collidingModifier, num4, num3);
      MountingAndCollisionPatch._collisionResetTimer = 0.0f;
    }
    else if (flag2)
    {
      MountingAndCollisionPatch._collisionTimer = 0.0f;
      MountingAndCollisionPatch._collisionResetTimer += Time.deltaTime;
      MountingAndCollisionPatch._collidingModifier = (double) MountingAndCollisionPatch._collisionResetTimer > (double) num5 ? Mathf.MoveTowards(MountingAndCollisionPatch._collidingModifier, 1f, num2) : Mathf.MoveTowards(MountingAndCollisionPatch._collidingModifier, num4, num3);
    }
    MountingAndCollisionPatch._previousOverlapValue = MountingAndCollisionPatch._smoothedOverlapValue;
    MountingAndCollisionPatch._blendField.SetValue((object) pwa.TurnAway, (object) (float) (4.5 * (double) MountingAndCollisionPatch._collidingModifier));
    MountingAndCollisionPatch._smoothInField.SetValue((object) pwa.TurnAway, (object) (float) (14.0 * (double) MountingAndCollisionPatch._collidingModifier));
    MountingAndCollisionPatch._smoothOutField.SetValue((object) pwa.TurnAway, (object) (float) (8.0 * (double) MountingAndCollisionPatch._collidingModifier));
  }

  private static void SetStanceSpeedModi(bool isPistol)
  {
    if (isPistol)
    {
      MountingAndCollisionPatch._stanceFactor = 1f;
      MountingAndCollisionPatch._stanceInverseFactor = 1f;
      StanceController.CameraMovmentForCollisionSpeed = 0.06f;
    }
    else if (StanceController.CurrentStance == EStance.ShortStock || StanceController.StoredStance == EStance.ShortStock)
    {
      MountingAndCollisionPatch._stanceFactor = 1.15f;
      MountingAndCollisionPatch._stanceInverseFactor = 0.85f;
      StanceController.CameraMovmentForCollisionSpeed = 0.1f;
    }
    else if (StanceController.CurrentStance == EStance.HighReady || StanceController.StoredStance == EStance.HighReady)
    {
      MountingAndCollisionPatch._stanceFactor = 1.1f;
      MountingAndCollisionPatch._stanceInverseFactor = 0.89f;
      StanceController.CameraMovmentForCollisionSpeed = 0.2f;
    }
    else if (StanceController.CurrentStance == EStance.LowReady || StanceController.StoredStance == EStance.LowReady)
    {
      MountingAndCollisionPatch._stanceFactor = 1.07f;
      MountingAndCollisionPatch._stanceInverseFactor = 0.92f;
      StanceController.CameraMovmentForCollisionSpeed = 0.16f;
    }
    else if (StanceController.CurrentStance == EStance.ActiveAiming || StanceController.StoredStance == EStance.ActiveAiming)
    {
      MountingAndCollisionPatch._stanceFactor = 1.03f;
      MountingAndCollisionPatch._stanceInverseFactor = 0.95f;
      StanceController.CameraMovmentForCollisionSpeed = 0.08f;
    }
    else if (StanceController.CurrentStance == EStance.PatrolStance)
    {
      MountingAndCollisionPatch._stanceFactor = 1f;
      MountingAndCollisionPatch._stanceInverseFactor = 1f;
      StanceController.CameraMovmentForCollisionSpeed = 0.1f;
    }
    else
    {
      MountingAndCollisionPatch._stanceFactor = 1f;
      MountingAndCollisionPatch._stanceInverseFactor = 1f;
      StanceController.CameraMovmentForCollisionSpeed = 0.07f;
    }
  }

  private static void AssignFinalTransforms(bool isPistol, float length)
  {
    if (isPistol)
    {
      MountingAndCollisionPatch._finalPos = new Vector3(0.15f, -0.6f, 0.1f);
      MountingAndCollisionPatch._finalRot = new Vector3(-0.9f, -0.01f, -0.01f);
    }
    else if (StanceController.CurrentStance == EStance.ShortStock || StanceController.StoredStance == EStance.ShortStock)
    {
      MountingAndCollisionPatch._finalPos = new Vector3(0.0f, 0.0f, -0.5f);
      MountingAndCollisionPatch._finalRot = new Vector3(0.01f, 0.1f, -0.05f);
    }
    else if (StanceController.CurrentStance == EStance.HighReady || StanceController.StoredStance == EStance.HighReady)
    {
      MountingAndCollisionPatch._finalPos = new Vector3(0.08f, -0.34f, -0.4f);
      MountingAndCollisionPatch._finalRot = new Vector3(-0.25f, -0.05f, -0.025f);
    }
    else if (StanceController.CurrentStance == EStance.LowReady || StanceController.StoredStance == EStance.LowReady)
    {
      MountingAndCollisionPatch._finalPos = new Vector3(0.0f, 0.0f, -0.15f);
      MountingAndCollisionPatch._finalRot = new Vector3(0.15f, -0.4f, 0.0f);
    }
    else if (StanceController.CurrentStance == EStance.ActiveAiming || StanceController.StoredStance == EStance.ActiveAiming)
    {
      MountingAndCollisionPatch._finalPos = new Vector3(0.05f, -0.2f, 0.1f);
      MountingAndCollisionPatch._finalRot = new Vector3(-0.5f, -0.5f, -0.5f);
    }
    else if (StanceController.CurrentStance == EStance.PatrolStance)
    {
      MountingAndCollisionPatch._finalPos = Vector3.zero;
      MountingAndCollisionPatch._finalRot = Vector3.zero;
    }
    else
    {
      MountingAndCollisionPatch._finalPos = new Vector3(0.0f, 0.05f, -0.15f);
      MountingAndCollisionPatch._finalRot = new Vector3(0.2f, -0.1f, -0.1f);
    }
  }

  private static void CollisionOverride(ProceduralWeaponAnimation pwa, Player.FirearmController fc)
  {
    MountingAndCollisionPatch._blendField.SetValue((object) pwa.TurnAway, (object) 0.0f);
    MountingAndCollisionPatch._smoothInField.SetValue((object) pwa.TurnAway, (object) 0.0f);
    MountingAndCollisionPatch._smoothOutField.SetValue((object) pwa.TurnAway, (object) 0.0f);
    Vector3 position = pwa.HandsContainer.WeaponRoot.position;
    Vector3 vector3_1 = Vector3.op_UnaryNegation(((Component) pwa.HandsContainer.WeaponRoot).transform.up);
    bool isPistol = WeaponStats.IsStocklessPistol || WeaponStats.IsMachinePistol;
    float newWeaponLength = WeaponStats.NewWeaponLength;
    float num1 = newWeaponLength * (isPistol ? 1.05f : 1.25f);
    MountingAndCollisionPatch._isColliding = false;
    RaycastHit raycastHit;
    if (!StanceController.IsMounting && EFTPhysicsClass.Raycast(new Ray(position, vector3_1), ref raycastHit, num1, LayerMask.op_Implicit(LayerMaskClass.HighPolyWithTerrainMask)))
    {
      MountingAndCollisionPatch._lastDistance = ((RaycastHit) ref raycastHit).distance;
      MountingAndCollisionPatch._isColliding = true;
    }
    MountingAndCollisionPatch.SetStanceSpeedModi(isPistol);
    bool flag1 = !WeaponStats.IsPistol && !WeaponStats.HasShoulderContact;
    float num2 = (flag1 ? 1.15f : 1f) * MountingAndCollisionPatch._stanceFactor;
    float num3 = (flag1 ? 0.85f : 1f) * MountingAndCollisionPatch._stanceInverseFactor;
    float num4 = 0.015f * WeaponStats.ErgoFactor * num3;
    float num5 = 0.03f * WeaponStats.ErgoFactor * MountingAndCollisionPatch._stanceInverseFactor;
    float num6 = 0.5f * MountingAndCollisionPatch._stanceFactor;
    if (MountingAndCollisionPatch._isColliding)
    {
      MountingAndCollisionPatch._collisionOverrideTimer = 0.0f;
      StanceController.IsColliding = true;
    }
    else
    {
      MountingAndCollisionPatch._collisionOverrideTimer += Time.deltaTime;
      StanceController.IsColliding = (double) MountingAndCollisionPatch._collisionOverrideTimer < (double) num4;
    }
    StanceController.StopCameraMovement = false;
    if (MountingAndCollisionPatch._isColliding)
    {
      MountingAndCollisionPatch._adsResetTimer = 0.0f;
      StanceController.StopCameraMovement = true;
    }
    else
    {
      MountingAndCollisionPatch._adsResetTimer += Time.deltaTime;
      StanceController.StopCameraMovement = (double) MountingAndCollisionPatch._adsResetTimer < (double) num5;
    }
    MountingAndCollisionPatch.AssignFinalTransforms(isPistol, newWeaponLength);
    float num7 = 0.15f * WeaponStats.TotalErgo * num2;
    float num8 = (isPistol ? 0.45f : 0.5f) * Mathf.Pow(newWeaponLength, 1.15f);
    bool flag2 = (double) MountingAndCollisionPatch._lastDistance >= (double) num8 || MountingAndCollisionPatch._delayFinalState;
    float num9 = Mathf.Pow(Mathf.InverseLerp(newWeaponLength, 0.0f, MountingAndCollisionPatch._lastDistance), 0.45f);
    float num10 = (float) ((double) newWeaponLength * -0.40000000596046448 * (1.0 - (double) MountingAndCollisionPatch._lastDistance / (double) num1));
    float num11 = (float) ((double) newWeaponLength * -0.64999997615814209 * (1.0 - (double) MountingAndCollisionPatch._lastDistance / (double) num1));
    Vector3 vector3_2 = Vector3.op_Multiply(new Vector3(0.025f, 0.0f, 0.0f), num9);
    vector3_2.y = num10;
    vector3_2.z = num11;
    Vector3 vector3_3 = Vector3.op_Multiply(MountingAndCollisionPatch._finalPos, num9);
    Vector3 vector3_4 = Vector3.op_Multiply(MountingAndCollisionPatch._initialRot, num9);
    Vector3 vector3_5 = Vector3.op_Multiply(MountingAndCollisionPatch._finalRot, num9);
    Vector3 vector3_6 = !StanceController.IsColliding ? Vector3.zero : (flag2 ? vector3_4 : vector3_5);
    Vector3 zero = Vector3.zero;
    bool flag3;
    if (!StanceController.IsColliding)
      flag3 = false;
    else if (flag2)
    {
      flag3 = false;
    }
    else
    {
      flag3 = true;
      MountingAndCollisionPatch._wasInFinalState = true;
    }
    bool flag4 = !StanceController.IsColliding && !MountingAndCollisionPatch._wasInFinalState;
    bool flag5 = flag2 && !MountingAndCollisionPatch._wasInFinalState;
    Vector3 vector3_7 = flag4 ? Vector3.zero : (flag5 ? vector3_2 : vector3_3);
    Vector3 vector3_8 = flag4 ? Vector3.zero : (flag5 ? vector3_4 : vector3_5);
    MountingAndCollisionPatch._collisionPos = Vector3.Lerp(MountingAndCollisionPatch._collisionPos, vector3_7, num7 * Time.deltaTime);
    Transform weaponRoot1 = pwa.HandsContainer.WeaponRoot;
    weaponRoot1.localPosition = Vector3.op_Addition(weaponRoot1.localPosition, MountingAndCollisionPatch._collisionPos);
    MountingAndCollisionPatch._collisionRot = Vector3.Lerp(MountingAndCollisionPatch._collisionRot, vector3_8, num7 * Time.deltaTime);
    Quaternion identity = Quaternion.identity;
    identity.x = MountingAndCollisionPatch._collisionRot.x;
    identity.y = MountingAndCollisionPatch._collisionRot.y;
    identity.z = MountingAndCollisionPatch._collisionRot.z;
    Transform weaponRoot2 = pwa.HandsContainer.WeaponRoot;
    weaponRoot2.localRotation = Quaternion.op_Multiply(weaponRoot2.localRotation, identity);
    if (MountingAndCollisionPatch._isColliding)
    {
      MountingAndCollisionPatch._finalStateDelayTimer += Time.deltaTime;
      MountingAndCollisionPatch._delayFinalState = (double) MountingAndCollisionPatch._finalStateDelayTimer < 0.05000000074505806;
    }
    else
    {
      MountingAndCollisionPatch._finalStateDelayTimer = 0.0f;
      MountingAndCollisionPatch._delayFinalState = true;
    }
    if (MountingAndCollisionPatch._wasInFinalState && !flag3)
    {
      MountingAndCollisionPatch._finalStateEndTimer += Time.deltaTime;
      if ((double) MountingAndCollisionPatch._finalStateEndTimer < (double) num6)
        return;
      MountingAndCollisionPatch._wasInFinalState = false;
    }
    else
      MountingAndCollisionPatch._finalStateEndTimer = 0.0f;
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(ProceduralWeaponAnimation __instance)
  {
    Player.FirearmController fc = (Player.FirearmController) MountingAndCollisionPatch._fcField.GetValue((object) __instance);
    if (Object.op_Equality((Object) fc, (Object) null))
      return;
    Player player = (Player) MountingAndCollisionPatch._playerField.GetValue((object) fc);
    if (!Object.op_Inequality((Object) player, (Object) null) || !player.IsYourPlayer || player.MovementContext.CurrentState.Name == 21)
      return;
    if (PluginConfig.OverrideCollision.Value && Plugin.FOVFixPresent)
      MountingAndCollisionPatch.CollisionOverride(__instance, fc);
    else if (PluginConfig.ModifyBSGCollision.Value)
      MountingAndCollisionPatch.ModifyBSGCollisions(__instance, fc);
    MountingAndCollisionPatch.DoMounting(player, __instance);
  }
}
