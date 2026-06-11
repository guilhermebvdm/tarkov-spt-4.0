// Decompiled with JetBrains decompiler
// Type: RealismMod.ShootController
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.Animations;
using EFT.Animations.NewRecoil;
using EFT.InventoryLogic;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class ShootController
{
  private const float FPS_SMOOTHING_FACTOR = 0.42f;
  public static bool IsFiring = false;
  public static bool IsFiringDeafen = false;
  public static bool IsFiringMovement = false;
  public static bool DoFiringWiggle = false;
  public static int ShotCount = 0;
  public static float FiringResetTimer = 0.0f;
  public static float FiringDuration = 0.0f;
  public static float ShotCountResetTimer = 0.0f;
  public static float DeafenResetTimer = 0.0f;
  public static float WiggleResetTimer = 0.0f;
  public static float MovementSpeedResetTimer = 0.0f;
  public static float BaseTotalHRecoil;
  public static float BaseTotalVRecoil;
  public static float BaseTotalDispersion;
  public static float BaseTotalCamRecoil;
  public static float BaseTotalRecoilDamping;
  public static float BaseTotalHandDamping;
  public static float BaseTotalConvergence;
  public static float FactoredTotalConvergence;
  public static float BaseTotalRecoilAngle;
  public static float FactoredTotalHRecoil;
  public static float FactoredTotalVRecoil;
  public static float FactoredTotalDispersion;
  public static float FactoredTotalCamRecoil;
  private static Vector2 _targetRotation = Vector2.zero;
  private static Vector2 _currentRotation = Vector2.zero;
  private static float _recoilAngle;
  private static float _dispersionSpeed;
  private static float _convergenceMulti = 1f;
  private static AnimationCurve _convergenceCurve = new AnimationCurve(new Keyframe[9]
  {
    new Keyframe(0.0f, 1f),
    new Keyframe(0.25f, 0.95f),
    new Keyframe(0.5f, 0.9f),
    new Keyframe(0.75f, 0.85f),
    new Keyframe(1f, 0.8f),
    new Keyframe(1.25f, 0.75f),
    new Keyframe(1.5f, 0.7f),
    new Keyframe(1.75f, 0.65f),
    new Keyframe(2f, 0.5f)
  });
  private static AnimationCurve _autoPistolIntensityCurve = new AnimationCurve(new Keyframe[6]
  {
    new Keyframe(0.0f, 1f),
    new Keyframe(0.05f, 2f),
    new Keyframe(0.1f, 2.5f),
    new Keyframe(0.3f, 2.5f),
    new Keyframe(0.5f, 2.5f),
    new Keyframe(0.7f, 2f)
  });

  public static Vector2 RecoilRotation => ShootController._currentRotation;

  public static void ShootUpdate(Player player, Player.FirearmController fc)
  {
    bool flag = player.ProceduralWeaponAnimation.method_18();
    bool isFiring = ShootController.IsFiring | flag;
    if (isFiring)
    {
      DeafenController.IncreaseDeafeningShooting();
      ShootController.RecoilController(player);
      ShootController.FiringDuration += Time.deltaTime;
    }
    ShootController.DeafenResetTimer += Time.deltaTime;
    ShootController.WiggleResetTimer += Time.deltaTime;
    ShootController.FiringResetTimer += Time.deltaTime;
    ShootController.MovementSpeedResetTimer += Time.deltaTime;
    ShootController.ShotCountResetTimer += Time.deltaTime;
    if (!flag)
      ShootController.FiringDuration = 0.0f;
    if ((double) ShootController.FiringResetTimer >= (double) PluginConfig.ShotResetDelay.Value && !flag)
    {
      ShootController.FiringResetTimer = 0.0f;
      ShootController._targetRotation = Vector2.zero;
      ShootController.ShotCount = 0;
      ShootController.IsFiring = false;
    }
    if ((double) ShootController.DeafenResetTimer >= (double) PluginConfig.DeafenResetDelay.Value)
    {
      ShootController.IsFiringDeafen = false;
      ShootController.DeafenResetTimer = 0.0f;
    }
    if ((double) ShootController.WiggleResetTimer >= 0.11999999731779099)
    {
      ShootController.DoFiringWiggle = false;
      ShootController.WiggleResetTimer = 0.0f;
    }
    if ((double) ShootController.MovementSpeedResetTimer >= 0.5)
    {
      ShootController.IsFiringMovement = false;
      ShootController.MovementSpeedResetTimer = 0.0f;
    }
    StanceController.StanceShotTimer();
    ShootController.UpdateConvergence(fc, player);
    ShootController.LerpRecoilRotation(player, isFiring);
  }

  private static float ShotModifier()
  {
    float num = 0.3f;
    return Mathf.Clamp(num + (float) ShootController.ShotCount * 0.3f, num, 1.2f);
  }

  public static void RecoilController(Player player)
  {
    bool isPistol = WeaponStats.IsPistol;
    float num1 = ShootController.ShotModifier();
    float rotationMultiplier = player.GetRotationMultiplier();
    float num2 = !StanceController.IsAiming ? 1.1f : 1f;
    float num3 = ((WeaponStats.IsVector ? 90f : ShootController.BaseTotalRecoilAngle) + (!StanceController.IsMounting || !WeaponStats.BipodIsDeployed ? (StanceController.IsMounting ? 5f : (StanceController.IsBracing ? 2.5f : 1f)) : 7.5f)) * (!isPistol ? (float) (1.0 + -(double) WeaponStats.TotalDispersionDelta * 0.035000000149011612) : 1f);
    float num4 = Mathf.Clamp(!isPistol ? num3 : num3 - 5f, 60f, 110f);
    float num5 = !isPistol ? (float) (1.0 + -(double) WeaponStats.TotalDispersionDelta) : 1f;
    float a = PluginConfig.RecoilDispersionFactor.Value * (1f / 1000f);
    ShootController._recoilAngle = Utils.AreFloatsEqual(a, 0.0f) ? 0.0f : (float) ((90.0 - (double) num4) / 250.0);
    float num6 = 90f / num4;
    ShootController._dispersionSpeed = PluginConfig.RecoilDispersionSpeed.Value * num5;
    float num7 = Mathf.Max(ShootController.FactoredTotalDispersion * a * num1 * num6 * num2, 0.0f);
    float num8 = (float) ((isPistol ? (double) PluginConfig.PistolRecClimbFactor.Value : (double) PluginConfig.RecoilClimbFactor.Value) * (1.0 / 1000.0));
    float num9 = num7 / rotationMultiplier;
    float num10 = -ShootController.FactoredTotalVRecoil * num8 * num1 / rotationMultiplier;
    float num11 = Mathf.Clamp(num9, -1f, 1f);
    float num12 = Mathf.Clamp(num10, -1f, 0.0f);
    ShootController._targetRotation.x = num11;
    ShootController._targetRotation.y = num12;
  }

  public static void LerpRecoilRotation(Player player, bool isFiring)
  {
    float num1 = 1f;
    if (PluginConfig.UseFpsRecoilFactor.Value)
      num1 = Utils.GetFPSFactor();
    float num2 = !ShootController.IsFiring ? 0.0f : Mathf.Lerp(-ShootController._targetRotation.x, ShootController._targetRotation.x, Mathf.PingPong(Time.time * ShootController._dispersionSpeed, 1f)) + ShootController._recoilAngle;
    Vector2 vector2;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2).\u002Ector(num2, ShootController._targetRotation.y);
    float num3 = !isFiring ? 30f : Mathf.InverseLerp(0.0f, 9f, ShootController.FactoredTotalConvergence) * PluginConfig.RecoilClimbSpeed.Value;
    ShootController._currentRotation = Vector2.MoveTowards(ShootController._currentRotation, Vector2.op_Multiply(vector2, num1), num3 * Time.deltaTime);
    player.Rotate(ShootController._currentRotation, false);
  }

  private static float ShotConvergenceFactor(bool isAutoPistol)
  {
    return isAutoPistol ? ShootController._autoPistolIntensityCurve.Evaluate(ShootController.FiringDuration) : ShootController._convergenceCurve.Evaluate(ShootController.FiringDuration);
  }

  public static void UpdateConvergence(Player.FirearmController fc, Player player)
  {
    bool isAutoPistol = WeaponStats.IsPistol && WeaponStats.FireMode == 0;
    ShootController._convergenceMulti = fc.autoFireOn ? ShootController.ShotConvergenceFactor(isAutoPistol) : 1f;
    float num = player.IsInPronePose ? 0.75f : 1f;
    ShootController.FactoredTotalConvergence = ShootController.BaseTotalConvergence * num * ShootController._convergenceMulti;
    ((RecoilProcessBase) player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect.HandRotationRecoilEffect).ReturnSpeed = ShootController.FactoredTotalConvergence;
  }

  public static void DoVisualRecoil(ref Quaternion weapRotation)
  {
    float num1 = Mathf.Clamp(ShootController.BaseTotalConvergence * 0.95f, 9f, 16f);
    if (ShootController.DoFiringWiggle)
    {
      float num2 = ShootController.FactoredTotalHRecoil / 31f;
      float num3 = Mathf.Lerp(-num2, num2, Mathf.PingPong((float) ((double) Time.time * (double) num1 * 1.0499999523162842), 1f));
      float num4 = ShootController.FactoredTotalDispersion / 16f;
      float num5 = Mathf.Lerp(-num4, num4, Mathf.PingPong(Time.time * num1, 1f)) * 0.05f;
      StanceController.TargetVisualRecoil = Vector3.op_Multiply(Vector3.op_Multiply(new Vector3(Mathf.Lerp(-num4, num4, Mathf.PingPong((float) ((double) Time.time * (double) num1 * 1.5), 1f)) * 0.1f * 0.95f, num3, num5 * 0.89f), PluginConfig.VisRecoilMulti.Value), WeaponStats.CurrentVisualRecoilMulti);
    }
    else
      StanceController.TargetVisualRecoil = Vector3.Lerp(StanceController.TargetVisualRecoil, Vector3.zero, 0.1f);
    StanceController.CurrentVisualRecoil = Vector3.Lerp(StanceController.CurrentVisualRecoil, StanceController.TargetVisualRecoil, 1f);
    Quaternion quaternion = Quaternion.Euler(StanceController.CurrentVisualRecoil);
    weapRotation = Quaternion.op_Multiply(weapRotation, quaternion);
  }

  public static void ResetFiringState(ProceduralWeaponAnimation pwa, Weapon weapon, Player player)
  {
    ShootController.SetRecoilParams(pwa, weapon, player);
    ++ShootController.ShotCount;
    ShootController.FiringResetTimer = 0.0f;
    ShootController.ShotCountResetTimer = 0.0f;
    ShootController.DeafenResetTimer = 0.0f;
    ShootController.WiggleResetTimer = 0.0f;
    ShootController.MovementSpeedResetTimer = 0.0f;
    StanceController.StanceShotTime = 0.0f;
    ShootController.IsFiring = true;
    ShootController.IsFiringDeafen = true;
    ShootController.DoFiringWiggle = true;
    ShootController.IsFiringMovement = true;
    StanceController.IsFiringFromStance = true;
  }

  public static void SetRecoilParams(ProceduralWeaponAnimation pwa, Weapon weapon, Player player)
  {
    if (!Plugin.ServerConfig.recoil_attachment_overhaul)
      return;
    NewRecoilShotEffect currentRecoilEffect = pwa.Shootingg.CurrentRecoilEffect as NewRecoilShotEffect;
    bool flag = WeaponStats.IsOptic && StanceController.IsAiming;
    float num1 = WeaponStats.IsPistol || WeaponStats.HasShoulderContact ? (WeaponStats.IsStockedPistol ? 0.85f : 1f) : 1.25f;
    float num2 = StanceController.IsAiming & flag ? 0.9f : 1f;
    float num3 = StanceController.IsAiming & flag ? 0.95f : 1f;
    currentRecoilEffect.HandRotationRecoil.CategoryIntensityMultiplier = weapon.Template.RecoilCategoryMultiplierHandRotation * PluginConfig.RecoilIntensity.Value * num1;
    currentRecoilEffect.HandRotationRecoil.ReturnTrajectoryDumping = weapon.Template.RecoilReturnPathDampingHandRotation * num2;
    float num4 = WeaponStats.IsPistol ? PluginConfig.PistolRecoilDampingMulti.Value : PluginConfig.RiflRecoilDampingMulti.Value;
    ((RecoilProcessBase) pwa.Shootingg.CurrentRecoilEffect.HandRotationRecoilEffect).Damping = weapon.Template.RecoilDampingHandRotation * num4 * num3;
    pwa.Shootingg.CurrentRecoilEffect.CameraRotationRecoilEffect.Damping = PluginConfig.CamWiggle.Value;
    pwa.Shootingg.CurrentRecoilEffect.CameraRotationRecoilEffect.ReturnSpeed = PluginConfig.CamReturn.Value;
    pwa.Shootingg.CurrentRecoilEffect.CameraRotationRecoilEffect.Intensity = 1f;
    pwa.Shootingg.CurrentRecoilEffect.HandPositionRecoilEffect.Damping = 0.748f * PluginConfig.HandsDampingMulti.Value;
    pwa.Shootingg.CurrentRecoilEffect.HandPositionRecoilEffect.ReturnSpeed = 0.154f;
    currentRecoilEffect.HandRotationRecoil.NextStablePointDistanceRange.x = 1f;
    currentRecoilEffect.HandRotationRecoil.NextStablePointDistanceRange.y = 4f;
  }
}
