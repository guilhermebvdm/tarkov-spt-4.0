// Decompiled with JetBrains decompiler
// Type: RealismMod.StanceController
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.Animations.NewRecoil;
using EFT.InventoryLogic;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

#nullable disable
namespace RealismMod;

public static class StanceController
{
  public const float STANCE_WEIGHT_LIMIT_KG = 8f;
  public static Quaternion CurrentRotation;
  public static Quaternion StanceRotation;
  public static Vector3 MountWeapPosition;
  public static Vector3 CurrentVisualRecoil;
  public static Vector3 TargetVisualRecoil;
  public static bool HasResetActiveAim;
  public static bool HasResetLowReady;
  public static bool HasResetHighReady;
  public static bool HasResetShortStock;
  public static bool HasResetPistolPos;
  public static bool HasResetMelee;
  public static bool IsResettingActiveAim;
  public static bool IsResettingLowReady;
  public static bool IsResettingHighReady;
  public static bool IsResettingShortStock;
  public static bool IsResettingPistol;
  public static bool IsResettingMelee;
  public static bool DidHalfMeleeAnim;
  public static float StanceRotationSpeed;
  public static bool HaveSetAiming;
  public static bool HaveSetActiveAim;
  public static bool _isLeftStanceResetState;
  private static float _leftStanceTime;
  private static Vector3 _leftStanceRotaiton;
  private static float _pistolPosSpeed;
  private static float _currentRifleXPos;
  private static float _currentRifleYPos;
  private static float _currentRifleZPos;
  private static float _currentPistolXPos;
  private static float _currentPistolYPos;
  private static float _currentPistolZPos;
  private static float _gunCameraAlignmentTargetX;
  private static float _gunCameraAlignmentTargetY;
  private static float _gunCameraAlignmentTargetZ;
  private static float _gunXTarget;
  private static float _gunYTarget;
  private static float _gunZTarget;
  private static Vector3 _leftStancePistolRotaitonTarget;
  private static Vector3 _leftStancePistolPositionTarget;
  private static Vector3 _leftStanceRifleRotaitonTarget;
  private static Vector3 _leftStanceRiflePositionTarget;
  private static Vector3 _leftStancePosition;
  private static Vector3 _leftStanceVelocity;
  private static float _leftStanceProgress;
  private static float _leftStanceTargetX;
  private static AnimationCurve _leftRotationXCurve;
  private static AnimationCurve _leffPosZCurve;
  private static AnimationCurve _leffPosZCurveReturn;
  public static Vector3 CoverWiggleDirection;
  public static Vector3 BaseWeaponOffsetPosition;
  public static Vector3 StanceTargetPosition;
  private static Vector3 _pistolLocalPosition;
  private static Vector3 _rifleLocalPosition;
  private const float _clickDelay = 0.2f;
  private static float _doubleClickTime;
  private static bool _clickTriggered;
  public static int StanceIndex;
  public static bool MeleeIsToggleable;
  public static bool CanDoMeleeDetection;
  public static bool MeleeHitSomething;
  private static float _meleeTimer;
  private static bool _isHoldingBackMelee;
  public static bool IsFiringFromStance;
  public static float StanceShotTime;
  private static float _manipTime;
  public static float ManipTimer;
  public static float DampingTimer;
  public static bool DoDampingTimer;
  public static bool CanResetDamping;
  public static bool WasAimingBeforeCollision;
  public static bool StopCameraMovement;
  public static float CameraMovmentForCollisionSpeed;
  public static bool IsColliding;
  public static float HighReadyBlackedArmTime;
  public static bool CanDoHighReadyInjuredAnim;
  public static bool CancelPistolStance;
  public static bool PistolIsColliding;
  public static bool CancelHighReady;
  public static bool ModifyHighReady;
  public static bool CancelLowReady;
  public static bool CancelShortStock;
  public static bool CancelActiveAim;
  public static bool ShouldResetStances;
  private static bool _doMeleeReset;
  private static EStance _lastRecordedStanceStamina;
  private static EStance _previousStance;
  private static EStance _currentStance;
  private static EStance _storedStance;
  public static bool FinishedUnPatrolStancing;
  private static bool _SkipPistolWiggle;
  public static bool WasActiveAim;
  private static bool _isLeftShoulder;
  public static bool CancelLeftShoulder;
  public static bool HaveResetLeftShoulder;
  public static bool IsDoingTacSprint;
  public const float TAC_SPRINT_WEIGHT_LIMIT = 5.1f;
  public const float TAC_SPRINT_WEIGHT_BULLPUP = 5.75f;
  public const int TAC_SPRINT_LENGTH_LIMIT = 6;
  public const float TAC_SPRINT_ERGO_LIMIT = 35f;
  public static bool IsInForcedLowReady;
  public static bool IsAiming;
  public static bool DidWeaponSwap;
  public static bool IsBlindFiring;
  public static bool IsInThirdPerson;
  public static bool ToggledLight;
  public static bool DidStanceWiggle;
  public static bool DidLowReadyResetStanceWiggle;
  public static float WiggleReturnSpeed;
  private static bool _regenStam;
  private static bool _drainStamStam;
  private static bool _neutralStam;
  private static bool _wasBracingStam;
  private static bool _wasMountingStam;
  private static bool _wasAimingStam;
  public static bool HaveResetStamDrain;
  public static bool CanResetAimDrain;
  private static Vector3 _posePosOffest;
  private static Vector3 _poseRotOffest;
  private static Vector3 _patrolPos;
  private static Vector3 _patrolRot;
  private static Vector3 _riflePatrolPos;
  private static Vector3 _riflePatrolRot;
  private static Vector3 _pistolPatrolPos;
  private static Vector3 _pistolPatrolRot;
  private static float _tacSprintTime;
  private static bool _canDoTacSprintTimer;
  private static float _mountAimSmoothed;
  public static float _cumulativeMountPitch;
  public static float _cumulativeMountYaw;
  private static Vector2 _lastMountYawPitch;
  public static EBracingDirection BracingDirection;
  public static bool IsBracing;
  public static bool _isRealismMounting;
  public static float BracingSwayBonus;
  public static float BracingRecoilBonus;
  public static WildSpawnType[] _botsToUseTacticalStances;
  public static Player.BetterValueBlender StanceBlender;
  private static readonly Stopwatch aimWatch;

  private static Quaternion _makeQuaternionDelta(Quaternion from, Quaternion to)
  {
    return Quaternion.op_Multiply(to, Quaternion.Inverse(from));
  }

  public static Vector3 MountPos { get; set; }

  public static Vector3 MountDir { get; set; }

  public static bool AllStancesReset
  {
    get
    {
      return StanceController.HasResetActiveAim && StanceController.HasResetLowReady && StanceController.HasResetHighReady && StanceController.HasResetShortStock && StanceController.HasResetPistolPos && StanceController.HaveResetLeftShoulder;
    }
  }

  public static bool ShouldBlockAllStances
  {
    get
    {
      return StanceController.IsMounting && WeaponStats.BipodIsDeployed || !StanceController.MeleeIsToggleable;
    }
  }

  public static bool IsReadyForBayonetCharge => StanceController._isHoldingBackMelee;

  public static bool TreatWeaponAsPistolStance
  {
    get => WeaponStats.IsMachinePistol || WeaponStats.IsStocklessPistol;
  }

  public static bool CanDoTacSprint
  {
    get
    {
      int num;
      if (PluginConfig.EnableTacSprint.Value && PlayerState.IsSprinting)
      {
        switch (StanceController.CurrentStance)
        {
          case EStance.HighReady:
            if ((double) WeaponStats.TotalWeaponWeight <= (WeaponStats.IsBullpup ? 5.75 : 5.0999999046325684) && (double) WeaponStats.TotalWeaponLength <= 6.0 && !PlayerState.IsScav && !Plugin.RealHealthController.HealthConditionPreventsTacSprint)
            {
              num = (double) WeaponStats.TotalErgo > 35.0 ? 1 : 0;
              goto label_6;
            }
            break;
          case EStance.ActiveAiming:
            break;
          default:
            if (StanceController.StoredStance != EStance.HighReady)
              break;
            goto case EStance.HighReady;
        }
      }
      num = 0;
label_6:
      return num != 0;
    }
  }

  public static bool ShouldForceLowReady
  {
    get
    {
      return (Plugin.RealHealthController.HealthConditionForcedLowReady || (double) WeaponStats.TotalWeaponWeight >= 10.0 && !StanceController.IsMounting) && !StanceController.IsAiming && !StanceController.IsFiringFromStance && StanceController.CurrentStance != EStance.PistolCompressed && StanceController.CurrentStance != EStance.PatrolStance && StanceController.CurrentStance != EStance.ShortStock && StanceController.CurrentStance != EStance.ActiveAiming && StanceController.MeleeIsToggleable && !StanceController.IsBracing;
    }
  }

  public static float HighReadyManipBuff
  {
    get => StanceController.CurrentStance == EStance.HighReady ? 1.18f : 1f;
  }

  public static float ActiveAimManipBuff
  {
    get
    {
      return StanceController.CurrentStance != EStance.ActiveAiming || !PluginConfig.ActiveAimReload.Value ? 1f : 1.15f;
    }
  }

  public static float LowReadyManipBuff
  {
    get => StanceController.CurrentStance == EStance.LowReady ? 1.21f : 1f;
  }

  public static EStance StoredStance
  {
    get => StanceController._storedStance;
    set => StanceController._storedStance = value;
  }

  public static EStance CurrentStance
  {
    get => StanceController._currentStance;
    set
    {
      if (value == StanceController._currentStance)
        return;
      StanceController._currentStance = value;
      if (!StanceController.IsAiming)
        Utils.GetYourPlayer().ProceduralWeaponAnimation.method_23(false);
    }
  }

  public static bool IsLeftStanceResetState
  {
    get => StanceController._isLeftStanceResetState;
    private set => StanceController._isLeftStanceResetState = value;
  }

  public static bool IsLeftShoulder
  {
    get => StanceController._isLeftShoulder;
    set
    {
      if (value == StanceController._isLeftShoulder)
        return;
      StanceController._isLeftShoulder = value;
      Utils.GetYourPlayer().ProceduralWeaponAnimation.method_23(false);
    }
  }

  public static bool IsDoingLeftShoulderNotBlocked
  {
    get
    {
      return StanceController.IsLeftShoulder && !StanceController.IsBlindFiring && !StanceController.CancelLeftShoulder;
    }
  }

  public static bool IsMounting
  {
    get => StanceController._isRealismMounting;
    set
    {
      if (value == StanceController._isRealismMounting)
        return;
      Player yourPlayer = Utils.GetYourPlayer();
      Player.FirearmController handsController = yourPlayer.HandsController as Player.FirearmController;
      if (Object.op_Equality((Object) handsController, (Object) null))
      {
        value = false;
      }
      else
      {
        StanceController._isRealismMounting = value;
        if (Object.op_Inequality((Object) yourPlayer.ProceduralWeaponAnimation, (Object) null))
          yourPlayer.ProceduralWeaponAnimation.method_23(false);
        float totalCenterOfImpact = handsController.Item.GetTotalCenterOfImpact(false);
        AccessTools.Field(typeof (Player.FirearmController), "float_3").SetValue((object) handsController, (object) totalCenterOfImpact);
        yourPlayer.ProceduralWeaponAnimation.UpdateTacticalReload();
        ((Player.AbstractHandsController) handsController).FirearmsAnimator.SetMounted(value);
      }
    }
  }

  public static bool IsCantedAiming(ProceduralWeaponAnimation pwa, bool checkifAiming)
  {
    return (double) Mathf.Abs(pwa.CurrentScope.Rotation) >= (double) EFTHardSettings.Instance.SCOPE_ROTATION_THRESHOLD & (!checkifAiming || StanceController.IsAiming);
  }

  public static bool AimingInterrupted { get; set; }

  public static void InterruptAim(Player.FirearmController fc)
  {
    if (!((Player.AbstractHandsController) fc).IsAiming || StanceController.AimingInterrupted)
      return;
    fc.ToggleAim();
    StanceController.AimingInterrupted = true;
  }

  public static void UnInterruptAim(Player.FirearmController fc)
  {
    if (((Player.AbstractHandsController) fc).IsAiming || !StanceController.AimingInterrupted)
      return;
    fc.ToggleAim();
    StanceController.AimingInterrupted = false;
  }

  public static float ChonkerFactor => (double) WeaponStats.TotalWeaponWeight >= 8.0 ? 0.7f : 1f;

  public static Dictionary<string, Vector3> GetWeaponOffsets()
  {
    return new Dictionary<string, Vector3>()
    {
      {
        "5aafa857e5b5b00018480968",
        new Vector3(0.0f, 0.0f, -0.1f)
      },
      {
        "5b0bbe4e5acfc40dc528a72d",
        new Vector3(0.0f, 0.0f, -0.035f)
      },
      {
        "676176d362e0497044079f4c",
        new Vector3(0.0f, -0.0135f, 0.02f)
      },
      {
        "6183afd850224f204c1da514",
        new Vector3(0.0f, -0.0135f, 0.02f)
      },
      {
        "6165ac306ef05c2ce828ef74",
        new Vector3(0.0f, -0.0135f, 0.02f)
      },
      {
        "6184055050224f204c1da540",
        new Vector3(0.0f, -0.0135f, 0.02f)
      },
      {
        "618428466ef05c2ce828f218",
        new Vector3(0.0f, -0.0135f, 0.02f)
      },
      {
        "5ae08f0a5acfc408fb1398a1",
        new Vector3(0.0f, 0.0f, -0.005f)
      },
      {
        "5bfd297f0db834001a669119",
        new Vector3(0.0f, 0.0f, -0.005f)
      },
      {
        "54491c4f4bdc2db1078b4568",
        new Vector3(0.0f, 0.0f, -0.01f)
      },
      {
        "56dee2bdd2720bc8328b4567",
        new Vector3(0.0f, 0.0f, -0.01f)
      },
      {
        "606dae0ab0e443224b421bb7",
        new Vector3(0.0f, 0.0f, -0.01f)
      },
      {
        "6259b864ebedf17603599e88",
        new Vector3(0.0f, 0.0f, -0.02f)
      },
      {
        "6783ae5bb52da6ed912e3d01",
        new Vector3(0.0f, 0.0f, -0.02f)
      }
    };
  }

  private static float GetRestoreRate()
  {
    float num1 = 0.0f;
    if (StanceController.IsMounting && WeaponStats.BipodIsDeployed)
      num1 = 5f;
    float num2;
    if (StanceController.CurrentStance == EStance.PatrolStance || StanceController.IsMounting)
      num2 = 4f;
    else if (StanceController.CurrentStance == EStance.LowReady || StanceController.CurrentStance == EStance.PistolCompressed || StanceController.IsBracing)
    {
      num2 = 2.4f;
    }
    else
    {
      switch (StanceController.CurrentStance)
      {
        case EStance.HighReady:
          num2 = 1.85f;
          break;
        case EStance.ShortStock:
          num2 = 1.3f;
          break;
        default:
          num2 = !StanceController.IsIdle() || PluginConfig.EnableIdleStamDrain.Value ? 1f : 1f;
          break;
      }
    }
    float num3 = WeaponStats.IsBullpup ? 1.05f : 1f;
    return (float) (1.0 - (double) WeaponStats.ErgoFactor * (double) num3 / 100.0) * num2 * PlayerState.HealthStamRegenFactor;
  }

  private static float GetDrainRate(Player player)
  {
    float num1 = !player.Physical.HoldingBreath ? (!StanceController.IsAiming ? (!StanceController.IsDoingTacSprint ? (StanceController.CurrentStance != EStance.ActiveAiming ? 0.1f : 0.075f) : 0.15f) : 0.15f) : (!StanceController.IsMounting || !WeaponStats.BipodIsDeployed ? (StanceController.IsMounting ? 0.05f : (StanceController.IsBracing ? 0.1f : 0.5f)) : 0.025f);
    float num2 = WeaponStats.IsBullpup ? 0.4f : 1f;
    return (float) ((double) WeaponStats.ErgoFactor * (double) num2 * (double) num1 * (1.0 - (double) PlayerState.HealthStamRegenFactor + 1.0) * (1.0 - (double) PlayerState.StrengthSkillAimBuff)) * PluginConfig.IdleStamDrainModi.Value;
  }

  public static void SetStanceStamina(Player player)
  {
    bool flag1 = player.MovementContext.CurrentState.Name == 21;
    bool flag2 = StanceController.CurrentStance == EStance.HighReady || StanceController.CurrentStance == EStance.LowReady || StanceController.CurrentStance == EStance.PatrolStance || StanceController.CurrentStance == EStance.ShortStock || StanceController.IsIdle() && !PluginConfig.EnableIdleStamDrain.Value;
    bool flag3 = ((!player.Physical.HoldingBreath && (StanceController.IsMounting || StanceController.IsBracing) || player.IsInPronePose ? 1 : (StanceController.CurrentStance == EStance.PistolCompressed ? 1 : 0)) | (flag1 ? 1 : 0)) != 0;
    bool flag4 = ((!flag2 || StanceController.IsAiming ? 0 : (!StanceController.IsFiringFromStance ? 1 : 0)) | (flag3 ? 1 : 0)) != 0 && !PlayerState.IsSprinting;
    bool flag5 = StanceController.IsIdle() && PluginConfig.EnableIdleStamDrain.Value;
    bool flag6 = flag2 && (StanceController.IsAiming || StanceController.IsFiringFromStance);
    bool flag7 = PlayerState.IsSprinting || player.IsInventoryOpened || StanceController.CurrentStance == EStance.ActiveAiming && player.Pose == 1;
    bool flag8 = ((flag6 ? 1 : (!flag2 ? 1 : 0)) | (flag5 ? 1 : 0)) != 0 && !flag3 && !flag7 || StanceController.IsDoingTacSprint && PluginConfig.EnableIdleStamDrain.Value;
    EStance currentStance = StanceController.CurrentStance;
    if (StanceController.HaveResetStamDrain || StanceController.DidWeaponSwap || StanceController.IsAiming != StanceController._wasAimingStam || StanceController._regenStam != flag4 || StanceController._drainStamStam != flag8 || StanceController._neutralStam != flag7 || StanceController._lastRecordedStanceStamina != StanceController.CurrentStance || StanceController.IsMounting != StanceController._wasMountingStam || StanceController.IsBracing != StanceController._wasBracingStam)
    {
      if (flag8)
        player.Physical.Aim(1f);
      else if (flag4)
        player.Physical.Aim(0.0f);
      else if (flag7)
        player.Physical.Aim(1f);
      StanceController.HaveResetStamDrain = false;
    }
    if (flag8)
      player.Physical.HandsStamina.Multiplier = StanceController.GetDrainRate(player);
    else if (flag4)
      player.Physical.HandsStamina.Multiplier = StanceController.GetRestoreRate();
    else if (flag7)
      player.Physical.HandsStamina.Multiplier = 0.0f;
    StanceController._regenStam = flag4;
    StanceController._drainStamStam = flag8;
    StanceController._neutralStam = flag7;
    StanceController._wasBracingStam = StanceController.IsBracing;
    StanceController._wasMountingStam = StanceController.IsMounting;
    StanceController._wasAimingStam = StanceController.IsAiming;
    StanceController._lastRecordedStanceStamina = StanceController.CurrentStance;
  }

  public static void ResetStanceStamina()
  {
    StanceController._regenStam = false;
    StanceController._drainStamStam = false;
    StanceController._neutralStam = false;
    StanceController._wasBracingStam = false;
    StanceController._wasMountingStam = false;
    StanceController._wasAimingStam = false;
    StanceController._lastRecordedStanceStamina = EStance.None;
  }

  public static void UnarmedStanceStamina(Player player)
  {
    player.Physical.Aim(0.0f);
    player.Physical.HandsStamina.Multiplier = 1f;
    StanceController.ResetStanceStamina();
  }

  public static bool IsIdle()
  {
    return StanceController.CurrentStance == EStance.None && StanceController.StoredStance == EStance.None && StanceController.HasResetActiveAim && StanceController.HasResetHighReady && StanceController.HasResetLowReady && StanceController.HasResetShortStock && StanceController.HasResetPistolPos && StanceController.HasResetMelee;
  }

  public static void CancelAllStances()
  {
    ((Player.ValueBlender) StanceController.StanceBlender).Target = 0.0f;
    StanceController.CurrentStance = EStance.None;
    StanceController.StoredStance = EStance.None;
    StanceController.DidStanceWiggle = false;
    StanceController.WasActiveAim = false;
    StanceController.IsLeftShoulder = false;
  }

  private static void StanceManipCancelTimer()
  {
    StanceController._manipTime += Time.deltaTime;
    if ((double) StanceController._manipTime < (double) StanceController.ManipTimer)
      return;
    StanceController.CancelHighReady = false;
    StanceController.ModifyHighReady = false;
    StanceController.CancelLowReady = false;
    StanceController.CancelShortStock = false;
    StanceController.CancelPistolStance = false;
    StanceController.CancelActiveAim = false;
    StanceController.ShouldResetStances = false;
    StanceController.CancelLeftShoulder = false;
    StanceController.ManipTimer = 0.25f;
    StanceController._manipTime = 0.0f;
  }

  private static void StanceDampingTimer()
  {
    StanceController.DampingTimer += Time.deltaTime;
    if ((double) StanceController.DampingTimer < 0.0099999997764825821)
      return;
    StanceController.CanResetDamping = true;
    StanceController.DoDampingTimer = false;
    StanceController.DampingTimer = 0.0f;
  }

  public static void StanceShotTimer()
  {
    StanceController.StanceShotTime += Time.deltaTime;
    if ((double) StanceController.StanceShotTime < 0.550000011920929)
      return;
    StanceController.IsFiringFromStance = false;
    StanceController.StanceShotTime = 0.0f;
  }

  private static void MeleeCooldownTimer()
  {
    StanceController._meleeTimer += Time.deltaTime;
    if ((double) StanceController._meleeTimer < 0.25)
      return;
    StanceController._doMeleeReset = false;
    StanceController.MeleeIsToggleable = true;
    StanceController._meleeTimer = 0.0f;
  }

  private static void DoMeleeEffect()
  {
    Player mainPlayer = Singleton<GameWorld>.Instance.MainPlayer;
    Player.FirearmController handsController = mainPlayer.HandsController as Player.FirearmController;
    if (WeaponStats.HasBayonet)
    {
      string key = Random.Range(1, 11) <= 5 ? "knife_1.wav" : "knife_2.wav";
      Singleton<BetterAudio>.Instance.PlayAtPoint(mainPlayer.ProceduralWeaponAnimation.HandsContainer.WeaponRootAnim.position, Plugin.RealismAudioController.HitAudioClips[key], 2f, (BetterAudio.AudioSourceGroupType) 7, 100, 2f, (EOcclusionTest) 3, (AudioMixerGroup) null, false);
    }
    mainPlayer.Physical.ConsumeAsMelee((float) (2.0 + (double) WeaponStats.ErgoFactor / 100.0));
  }

  private static void ToggleStance(
    EStance targetStance,
    bool setPrevious = false,
    bool setPrevisousAsCurrent = false)
  {
    StanceController._previousStance = StanceController._currentStance;
    if (StanceController.IsLeftShoulder)
      StanceController.IsLeftShoulder = false;
    if (setPrevious)
      StanceController.StoredStance = StanceController.CurrentStance;
    StanceController.CurrentStance = StanceController.CurrentStance != targetStance ? targetStance : EStance.None;
    if (!setPrevisousAsCurrent)
      return;
    StanceController.StoredStance = StanceController.CurrentStance;
  }

  private static void ToggleHighReady()
  {
    ((Player.ValueBlender) StanceController.StanceBlender).Target = (double) ((Player.ValueBlender) StanceController.StanceBlender).Target == 0.0 ? 1f : 0.0f;
    StanceController.ToggleStance(EStance.HighReady, setPrevisousAsCurrent: true);
    StanceController.WasActiveAim = false;
    StanceController.DidStanceWiggle = false;
    if (StanceController.CurrentStance != EStance.HighReady || !Plugin.RealHealthController.HealthConditionForcedLowReady)
      return;
    StanceController.CanDoHighReadyInjuredAnim = true;
  }

  private static void ToggleLowReady()
  {
    ((Player.ValueBlender) StanceController.StanceBlender).Target = (double) ((Player.ValueBlender) StanceController.StanceBlender).Target == 0.0 ? 1f : 0.0f;
    StanceController.ToggleStance(EStance.LowReady, setPrevisousAsCurrent: true);
    StanceController.WasActiveAim = false;
    StanceController.DidStanceWiggle = false;
  }

  private static void HandleScrollInput(float scrollIncrement)
  {
    if ((double) scrollIncrement == -1.0)
    {
      int num;
      switch (StanceController.CurrentStance)
      {
        case EStance.LowReady:
          num = 0;
          break;
        case EStance.HighReady:
          StanceController.ToggleHighReady();
          goto label_7;
        default:
          num = StanceController.HasResetHighReady ? 1 : 0;
          break;
      }
      if (num != 0)
        StanceController.ToggleLowReady();
label_7:;
    }
    if ((double) scrollIncrement != 1.0 || StanceController.CurrentStance == EStance.HighReady)
      return;
    if (StanceController.CurrentStance == EStance.LowReady && !Plugin.RealHealthController.HealthConditionForcedLowReady)
      StanceController.ToggleLowReady();
    else if (StanceController.CurrentStance != EStance.HighReady && StanceController.HasResetLowReady)
      StanceController.ToggleHighReady();
  }

  public static void ToggleLeftShoulder()
  {
    Utils.GetYourPlayer().method_58(5f, false);
    StanceController.IsLeftShoulder = !StanceController.IsLeftShoulder;
    if (StanceController.TreatWeaponAsPistolStance)
      return;
    StanceController.CurrentStance = EStance.None;
    StanceController.StoredStance = EStance.None;
    StanceController.WasActiveAim = false;
    StanceController.HaveSetActiveAim = false;
    StanceController.DidStanceWiggle = false;
    ((Player.ValueBlender) StanceController.StanceBlender).Target = 0.0f;
  }

  public static void StanceUpdate()
  {
    if (Utils.WeaponIsReady && Utils.GetYourPlayer().MovementContext.CurrentState.Name != 21)
    {
      if (StanceController.DoDampingTimer)
        StanceController.StanceDampingTimer();
      if (StanceController._doMeleeReset)
        StanceController.MeleeCooldownTimer();
      KeyboardShortcut keyboardShortcut;
      int num1;
      if (!StanceController.ShouldBlockAllStances)
      {
        keyboardShortcut = PluginConfig.PatrolKeybind.Value;
        if (Input.GetKeyDown(((KeyboardShortcut) ref keyboardShortcut).MainKey))
        {
          keyboardShortcut = PluginConfig.PatrolKeybind.Value;
          // ISSUE: reference to a compiler-generated field
          // ISSUE: reference to a compiler-generated field
          num1 = ((KeyboardShortcut) ref keyboardShortcut).Modifiers.All<KeyCode>(StanceController.\u003C\u003EO.\u003C0\u003E__GetKey ?? (StanceController.\u003C\u003EO.\u003C0\u003E__GetKey = new Func<KeyCode, bool>(Input.GetKey))) ? 1 : 0;
          goto label_9;
        }
      }
      num1 = 0;
label_9:
      if (num1 != 0)
      {
        Utils.GetYourPlayer().method_58(5f, false);
        StanceController.ToggleStance(EStance.PatrolStance);
        StanceController.StoredStance = EStance.None;
        ((Player.ValueBlender) StanceController.StanceBlender).Target = 0.0f;
        StanceController.DidStanceWiggle = false;
      }
      if (!PlayerState.IsSprinting && !PlayerState.IsInInventory && !StanceController.TreatWeaponAsPistolStance)
      {
        int num2;
        if (!StanceController.ShouldBlockAllStances)
        {
          keyboardShortcut = PluginConfig.CycleStancesKeybind.Value;
          num2 = Input.GetKeyUp(((KeyboardShortcut) ref keyboardShortcut).MainKey) ? 1 : 0;
        }
        else
          num2 = 0;
        if (num2 != 0)
        {
          if ((double) Time.time <= (double) StanceController._doubleClickTime)
          {
            StanceController._clickTriggered = true;
            ((Player.ValueBlender) StanceController.StanceBlender).Target = 0.0f;
            StanceController.StanceIndex = 0;
            StanceController.CancelAllStances();
            StanceController.DidStanceWiggle = false;
          }
          else
          {
            StanceController._clickTriggered = false;
            StanceController._doubleClickTime = Time.time + 0.2f;
          }
        }
        else if (!StanceController._clickTriggered && (double) Time.time > (double) StanceController._doubleClickTime)
        {
          StanceController.IsLeftShoulder = false;
          ((Player.ValueBlender) StanceController.StanceBlender).Target = 1f;
          StanceController._clickTriggered = true;
          ++StanceController.StanceIndex;
          StanceController.StanceIndex = StanceController.StanceIndex > 3 ? 1 : StanceController.StanceIndex;
          StanceController.CurrentStance = (EStance) StanceController.StanceIndex;
          StanceController.StoredStance = StanceController.CurrentStance;
          StanceController.DidStanceWiggle = false;
          if (StanceController.CurrentStance == EStance.HighReady && Plugin.RealHealthController.HealthConditionForcedLowReady)
            StanceController.CanDoHighReadyInjuredAnim = true;
        }
        if (!PluginConfig.ToggleActiveAim.Value)
        {
          int num3;
          if (!StanceController.IsAiming && !StanceController.ShouldBlockAllStances)
          {
            keyboardShortcut = PluginConfig.ActiveAimKeybind.Value;
            if (Input.GetKey(((KeyboardShortcut) ref keyboardShortcut).MainKey))
            {
              keyboardShortcut = PluginConfig.ActiveAimKeybind.Value;
              // ISSUE: reference to a compiler-generated field
              // ISSUE: reference to a compiler-generated field
              if (((KeyboardShortcut) ref keyboardShortcut).Modifiers.All<KeyCode>(StanceController.\u003C\u003EO.\u003C0\u003E__GetKey ?? (StanceController.\u003C\u003EO.\u003C0\u003E__GetKey = new Func<KeyCode, bool>(Input.GetKey))))
              {
                num3 = 1;
                goto label_29;
              }
            }
          }
          num3 = !Input.GetKey((KeyCode) 324) ? 0 : (!PlayerState.IsAllowedADS ? 1 : 0);
label_29:
          if (num3 != 0)
          {
            if (!StanceController.HaveSetActiveAim)
              StanceController.DidStanceWiggle = false;
            StanceController.IsLeftShoulder = false;
            ((Player.ValueBlender) StanceController.StanceBlender).Target = 1f;
            StanceController.CurrentStance = EStance.ActiveAiming;
            StanceController.WasActiveAim = true;
            StanceController.HaveSetActiveAim = true;
          }
          else if (StanceController.HaveSetActiveAim)
          {
            ((Player.ValueBlender) StanceController.StanceBlender).Target = 0.0f;
            StanceController.CurrentStance = StanceController.StoredStance;
            StanceController.WasActiveAim = false;
            StanceController.HaveSetActiveAim = false;
            StanceController.DidStanceWiggle = false;
          }
        }
        else
        {
          int num4;
          if (!StanceController.IsAiming && !StanceController.ShouldBlockAllStances)
          {
            keyboardShortcut = PluginConfig.ActiveAimKeybind.Value;
            if (Input.GetKeyDown(((KeyboardShortcut) ref keyboardShortcut).MainKey))
            {
              keyboardShortcut = PluginConfig.ActiveAimKeybind.Value;
              // ISSUE: reference to a compiler-generated field
              // ISSUE: reference to a compiler-generated field
              if (((KeyboardShortcut) ref keyboardShortcut).Modifiers.All<KeyCode>(StanceController.\u003C\u003EO.\u003C0\u003E__GetKey ?? (StanceController.\u003C\u003EO.\u003C0\u003E__GetKey = new Func<KeyCode, bool>(Input.GetKey))))
              {
                num4 = 1;
                goto label_40;
              }
            }
          }
          num4 = !Input.GetKeyDown((KeyCode) 324) ? 0 : (!PlayerState.IsAllowedADS ? 1 : 0);
label_40:
          if (num4 != 0)
          {
            ((Player.ValueBlender) StanceController.StanceBlender).Target = (double) ((Player.ValueBlender) StanceController.StanceBlender).Target == 0.0 ? 1f : 0.0f;
            StanceController.ToggleStance(EStance.ActiveAiming);
            StanceController.WasActiveAim = StanceController.CurrentStance == EStance.ActiveAiming;
            StanceController.DidStanceWiggle = false;
            if (StanceController.CurrentStance != EStance.ActiveAiming)
              StanceController.CurrentStance = StanceController.StoredStance;
          }
        }
        if (!StanceController.ShouldBlockAllStances && PluginConfig.UseMouseWheelStance.Value && !StanceController.IsAiming)
        {
          keyboardShortcut = PluginConfig.StanceWheelComboKeyBind.Value;
          if (Input.GetKey(((KeyboardShortcut) ref keyboardShortcut).MainKey) && PluginConfig.UseMouseWheelPlusKey.Value || !PluginConfig.UseMouseWheelPlusKey.Value && !Input.GetKey((KeyCode) 306) && !Input.GetKey((KeyCode) 308) && !Input.GetKey((KeyCode) 114) && !Input.GetKey((KeyCode) 99))
          {
            float y = Input.mouseScrollDelta.y;
            if ((double) y != 0.0)
              StanceController.HandleScrollInput(y);
          }
        }
        int num5;
        if (!StanceController.IsAiming && StanceController.MeleeIsToggleable)
        {
          keyboardShortcut = PluginConfig.MeleeKeybind.Value;
          if (Input.GetKeyDown(((KeyboardShortcut) ref keyboardShortcut).MainKey))
          {
            keyboardShortcut = PluginConfig.MeleeKeybind.Value;
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            num5 = ((KeyboardShortcut) ref keyboardShortcut).Modifiers.All<KeyCode>(StanceController.\u003C\u003EO.\u003C0\u003E__GetKey ?? (StanceController.\u003C\u003EO.\u003C0\u003E__GetKey = new Func<KeyCode, bool>(Input.GetKey))) ? 1 : 0;
            goto label_55;
          }
        }
        num5 = 0;
label_55:
        if (num5 != 0)
        {
          StanceController.IsMounting = false;
          StanceController.IsLeftShoulder = false;
          StanceController.CurrentStance = EStance.Melee;
          StanceController.StoredStance = EStance.None;
          StanceController.WasActiveAim = false;
          StanceController.DidStanceWiggle = false;
          ((Player.ValueBlender) StanceController.StanceBlender).Target = 1f;
          StanceController.MeleeIsToggleable = false;
          StanceController.MeleeHitSomething = false;
        }
        int num6;
        if (!StanceController.ShouldBlockAllStances)
        {
          keyboardShortcut = PluginConfig.ShortStockKeybind.Value;
          if (Input.GetKeyDown(((KeyboardShortcut) ref keyboardShortcut).MainKey))
          {
            keyboardShortcut = PluginConfig.ShortStockKeybind.Value;
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            num6 = ((KeyboardShortcut) ref keyboardShortcut).Modifiers.All<KeyCode>(StanceController.\u003C\u003EO.\u003C0\u003E__GetKey ?? (StanceController.\u003C\u003EO.\u003C0\u003E__GetKey = new Func<KeyCode, bool>(Input.GetKey))) ? 1 : 0;
            goto label_61;
          }
        }
        num6 = 0;
label_61:
        if (num6 != 0)
        {
          ((Player.ValueBlender) StanceController.StanceBlender).Target = (double) ((Player.ValueBlender) StanceController.StanceBlender).Target == 0.0 ? 1f : 0.0f;
          StanceController.ToggleStance(EStance.ShortStock, setPrevisousAsCurrent: true);
          StanceController.WasActiveAim = false;
          StanceController.DidStanceWiggle = false;
        }
        int num7;
        if (!StanceController.ShouldBlockAllStances && !StanceController.IsInForcedLowReady)
        {
          keyboardShortcut = PluginConfig.HighReadyKeybind.Value;
          if (Input.GetKeyDown(((KeyboardShortcut) ref keyboardShortcut).MainKey))
          {
            keyboardShortcut = PluginConfig.HighReadyKeybind.Value;
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            num7 = ((KeyboardShortcut) ref keyboardShortcut).Modifiers.All<KeyCode>(StanceController.\u003C\u003EO.\u003C0\u003E__GetKey ?? (StanceController.\u003C\u003EO.\u003C0\u003E__GetKey = new Func<KeyCode, bool>(Input.GetKey))) ? 1 : 0;
            goto label_67;
          }
        }
        num7 = 0;
label_67:
        if (num7 != 0)
          StanceController.ToggleHighReady();
        int num8;
        if (!StanceController.ShouldBlockAllStances && !StanceController.IsInForcedLowReady)
        {
          keyboardShortcut = PluginConfig.LowReadyKeybind.Value;
          if (Input.GetKeyDown(((KeyboardShortcut) ref keyboardShortcut).MainKey))
          {
            keyboardShortcut = PluginConfig.LowReadyKeybind.Value;
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            num8 = ((KeyboardShortcut) ref keyboardShortcut).Modifiers.All<KeyCode>(StanceController.\u003C\u003EO.\u003C0\u003E__GetKey ?? (StanceController.\u003C\u003EO.\u003C0\u003E__GetKey = new Func<KeyCode, bool>(Input.GetKey))) ? 1 : 0;
            goto label_73;
          }
        }
        num8 = 0;
label_73:
        if (num8 != 0)
          StanceController.ToggleLowReady();
        if (StanceController.IsAiming)
        {
          if (StanceController.CurrentStance == EStance.ActiveAiming || StanceController.WasActiveAim)
            StanceController.StoredStance = EStance.None;
          StanceController.CurrentStance = EStance.None;
          StanceController.HaveSetAiming = true;
        }
        else if (StanceController.HaveSetAiming)
        {
          StanceController.CurrentStance = StanceController.WasActiveAim ? EStance.ActiveAiming : StanceController.StoredStance;
          StanceController.HaveSetAiming = false;
        }
      }
      if (ShootController.IsFiring)
      {
        bool flag1 = PluginConfig.RememberStanceFiring.Value && StanceController.IsAiming;
        bool flag2 = StanceController.CurrentStance == EStance.ActiveAiming && !StanceController.IsAiming;
        if (!flag1 && !flag2 && StanceController.CurrentStance != EStance.ShortStock && StanceController.CurrentStance != EStance.PistolCompressed)
        {
          StanceController.CurrentStance = EStance.None;
          StanceController.StoredStance = EStance.None;
          ((Player.ValueBlender) StanceController.StanceBlender).Target = 0.0f;
        }
      }
      if (StanceController.CanDoHighReadyInjuredAnim)
      {
        StanceController.HighReadyBlackedArmTime += Time.deltaTime;
        if ((double) StanceController.HighReadyBlackedArmTime >= 0.34999999403953552)
        {
          StanceController.CanDoHighReadyInjuredAnim = false;
          StanceController.CurrentStance = EStance.LowReady;
          StanceController.StoredStance = EStance.LowReady;
          StanceController.HighReadyBlackedArmTime = 0.0f;
        }
      }
      if (StanceController.ShouldForceLowReady)
      {
        ((Player.ValueBlender) StanceController.StanceBlender).Target = 1f;
        StanceController.CurrentStance = EStance.LowReady;
        StanceController.StoredStance = EStance.LowReady;
        StanceController.WasActiveAim = false;
        StanceController.IsLeftShoulder = false;
        StanceController.IsInForcedLowReady = true;
      }
      else
        StanceController.IsInForcedLowReady = false;
    }
    if (StanceController.ShouldResetStances)
      StanceController.StanceManipCancelTimer();
    if (!StanceController.DidWeaponSwap && (PluginConfig.RememberStanceItem.Value || Utils.WeaponIsReady) && Utils.PlayerIsReady)
      return;
    StanceController.IsLeftShoulder = false;
    StanceController.IsMounting = false;
    StanceController.CurrentStance = EStance.None;
    StanceController.StoredStance = EStance.None;
    ((Player.ValueBlender) StanceController.StanceBlender).Target = 0.0f;
    StanceController.StanceIndex = 0;
    StanceController.WasActiveAim = false;
    StanceController.DidWeaponSwap = false;
    StanceController.AimingInterrupted = false;
    StanceController.ResetStanceStamina();
  }

  private static void DoTacSprint(Player.FirearmController fc, Player player)
  {
    if (StanceController.CanDoTacSprint)
    {
      StanceController.IsDoingTacSprint = true;
      player.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, 2f);
      StanceController._tacSprintTime = 0.0f;
      StanceController._canDoTacSprintTimer = true;
    }
    else if (PluginConfig.EnableTacSprint.Value && StanceController._canDoTacSprintTimer)
    {
      StanceController._tacSprintTime += Time.deltaTime;
      if ((double) StanceController._tacSprintTime >= 0.5)
      {
        player.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, WeaponStats.TotalWeaponLength);
        StanceController._tacSprintTime = 0.0f;
        StanceController._canDoTacSprintTimer = false;
      }
      StanceController.IsDoingTacSprint = false;
    }
    else
      StanceController.IsDoingTacSprint = false;
  }

  public static void DoWiggleEffects(
    Player player,
    ProceduralWeaponAnimation pwa,
    Weapon weapon,
    Vector3 wiggleDirection,
    bool playSound = false,
    float volume = 4f,
    float wiggleFactor = 1f,
    bool isADS = false,
    bool useGearSound = false)
  {
    if (playSound)
      player.method_58(volume * PluginConfig.StanceSfxModifier.Value, useGearSound);
    NewRecoilShotEffect currentRecoilEffect = pwa.Shootingg.CurrentRecoilEffect as NewRecoilShotEffect;
    if (isADS)
    {
      currentRecoilEffect.HandRotationRecoil.ReturnTrajectoryDumping = 0.3f * wiggleFactor;
      ((RecoilProcessBase) pwa.Shootingg.CurrentRecoilEffect.HandRotationRecoilEffect).Damping = 0.3f * wiggleFactor;
    }
    player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect.RecoilProcessValues[3].IntensityMultiplicator = 0.0f;
    player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect.RecoilProcessValues[4].IntensityMultiplicator = 0.0f;
    float length = (float) pwa.Shootingg.CurrentRecoilEffect.RecoilProcessValues.Length;
    for (int index = 0; (double) index < (double) length; ++index)
      pwa.Shootingg.CurrentRecoilEffect.RecoilProcessValues[index].Process(wiggleDirection);
    player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect.RecoilProcessValues[3].IntensityMultiplicator = 0.0f;
    player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect.RecoilProcessValues[4].IntensityMultiplicator = 0.0f;
  }

  private static void MoveGunToCameraPID(
    ProceduralWeaponAnimation pwa,
    float dt,
    float stanceMulti,
    ref float gunAxesTarget,
    ref float gunCameraAlignmentTarget,
    float camTargetAxes,
    float speedModifer,
    float tolerance = 0.001f,
    bool ignoreLeftShoulder = false)
  {
    if (!StanceController.IsAiming)
      gunCameraAlignmentTarget = camTargetAxes;
    if (StanceController.IsColliding || StanceController.PistolIsColliding || !pwa.OverlappingAllowsBlindfire || StanceController.StopCameraMovement || StanceController.IsDoingLeftShoulderNotBlocked && !ignoreLeftShoulder)
      return;
    bool flag1 = ShootController.IsFiringMovement && !PluginConfig.EnableAltRifleRecoil.Value && !StanceController.TreatWeaponAsPistolStance;
    bool flag2 = ShootController.IsFiringMovement && StanceController.TreatWeaponAsPistolStance;
    if (!StanceController.IsAiming || flag1 || flag2)
      return;
    float num1 = speedModifer * stanceMulti;
    float num2 = gunCameraAlignmentTarget - camTargetAxes;
    if ((double) Mathf.Abs(num2) > (double) tolerance)
    {
      float num3 = num2 * num1 * dt;
      gunAxesTarget += num3;
    }
  }

  private static Vector3 GetRifleStancePIDModifier()
  {
    int num;
    switch (StanceController.StoredStance)
    {
      case EStance.LowReady:
        return new Vector3(0.8f, 0.7f, 1f);
      case EStance.HighReady:
        return new Vector3(0.6f, 0.35f, 1f);
      case EStance.ShortStock:
        return new Vector3(0.5f, 0.3f, 1f);
      case EStance.ActiveAiming:
        num = 1;
        break;
      default:
        num = StanceController.WasActiveAim ? 1 : 0;
        break;
    }
    return num != 0 ? new Vector3(1.5f, 0.75f, 1f) : Vector3.one;
  }

  public static void DoExtraPosAndRot(ProceduralWeaponAnimation pwa, Player player)
  {
    float num1 = WeaponStats.IsPistol || WeaponStats.HasShoulderContact ? 0.0f : -0.04f;
    float num2 = (float) WeaponStats.StockPosition * 0.01f;
    float num3 = WeaponStats.HasShoulderContact ? -0.04f : 0.04f;
    float num4 = (1f - player.MovementContext.PoseLevel) * num3;
    float num5 = pwa.IsAiming ? 0.0f : 0.0f;
    float num6 = pwa.IsAiming ? 0.0f : 0.0f;
    float num7 = pwa.IsAiming ? 0.0f : Mathf.Clamp(num4 + num1 + num2, -0.05f, 0.05f);
    Vector3 vector3_1;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3_1).\u002Ector(num5, num6, num7);
    StanceController._posePosOffest = Vector3.Lerp(StanceController._posePosOffest, vector3_1, 5f * Time.deltaTime);
    Transform weaponRoot1 = pwa.HandsContainer.WeaponRoot;
    weaponRoot1.localPosition = Vector3.op_Addition(weaponRoot1.localPosition, StanceController._posePosOffest);
    bool flag1 = WeaponStats.BipodIsDeployed && StanceController.IsMounting;
    bool flag2 = StanceController.IsCantedAiming(pwa, true);
    bool flag3 = !flag2 && !flag1 && (GearController.HasGasMask || GearController.FSIsActive && GearController.GearBlocksMouth) && !WeaponStats.WeaponCanFSADS && pwa.IsAiming && WeaponStats.HasShoulderContact && !WeaponStats.IsStocklessPistol && !WeaponStats.IsMachinePistol;
    bool flag4 = WeaponStats.HasLongMag && player.IsInPronePose && !flag1;
    float num8 = -0.41f;
    float num9 = flag2 ? 0.0f : (!flag4 || pwa.IsAiming ? (!flag4 || !pwa.IsAiming ? 0.0f : -0.12f) : -0.35f);
    float num10 = WeaponStats.ErgoFactor * (-1f / 1000f);
    float num11 = (float) ((1.0 - (double) player.MovementContext.PoseLevel) * -0.029999999329447746 + (player.IsInPronePose ? -0.029999999329447746 : 0.0));
    float num12 = flag3 ? num10 - 0.025f : 0.0f;
    float num13 = pwa.IsAiming || StanceController.IsMounting || StanceController.IsBracing ? 0.0f : num11 + num10;
    float num14 = flag2 ? num8 : 0.0f;
    float num15 = 0.0f;
    float num16 = Mathf.Clamp(num13 + num12 + num9, -0.5f, 0.0f) + num14;
    float num17 = 0.0f;
    Vector3 vector3_2;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3_2).\u002Ector(num15, num16, num17);
    StanceController._poseRotOffest = Vector3.Lerp(StanceController._poseRotOffest, vector3_2, 5f * Time.deltaTime);
    Quaternion identity = Quaternion.identity;
    identity.x = StanceController._poseRotOffest.x;
    identity.y = StanceController._poseRotOffest.y;
    identity.z = StanceController._poseRotOffest.z;
    Transform weaponRoot2 = pwa.HandsContainer.WeaponRoot;
    weaponRoot2.localRotation = Quaternion.op_Multiply(weaponRoot2.localRotation, identity);
  }

  private static void CheckLeftShoulder(
    Player player,
    Player.FirearmController fc,
    ProceduralWeaponAnimation pwa,
    float stanceMulti,
    float dt,
    Vector3 posTarget,
    Vector3 rotTarget,
    float rotSpeed,
    float curveModifier = 1f)
  {
    float num1 = Mathf.Clamp((float) (1.0 - (double) stanceMulti + 1.0), 0.05f, 1.5f);
    float num2 = StanceController.IsAiming ? num1 * 0.22f : num1 * 0.22f;
    float num3 = posTarget.x + PluginConfig.LeftShoulderOffset.Value;
    Vector3 vector3_1 = StanceController.IsDoingLeftShoulderNotBlocked ? new Vector3(num3, posTarget.y, posTarget.z + StanceController._leffPosZCurve.Evaluate(StanceController._leftStanceProgress) * curveModifier) : new Vector3(0.0f, 0.0f, StanceController._leffPosZCurveReturn.Evaluate(StanceController._leftStanceProgress) * curveModifier);
    if (StanceController.IsDoingLeftShoulderNotBlocked)
    {
      StanceController._leftStanceTargetX = num3;
      StanceController._leftStanceTime = 0.0f;
      StanceController._isLeftStanceResetState = false;
    }
    else
    {
      StanceController._leftStanceTime += dt;
      StanceController._isLeftStanceResetState = (double) StanceController._leftStanceTime <= 0.5;
    }
    StanceController._leftStancePosition = Vector3.SmoothDamp(StanceController._leftStancePosition, vector3_1, ref StanceController._leftStanceVelocity, num2, 0.55f, dt);
    StanceController._leftStanceProgress = Mathf.InverseLerp(0.0f, StanceController._leftStanceTargetX, StanceController._leftStancePosition.x);
    StanceController.HaveResetLeftShoulder = Utils.AreFloatsEqual(StanceController._leftStanceProgress, 0.0f) && !StanceController.IsLeftShoulder;
    bool flag1 = StanceController.IsLeftShoulder && Utils.IsLessThan(StanceController._leftStanceProgress, 0.99f);
    bool flag2 = (StanceController._isLeftStanceResetState || !StanceController.IsLeftShoulder) && Utils.IsGreaterThan(StanceController._leftStanceProgress, 0.01f);
    if (StanceController.IsAiming && flag1 | flag2)
      StanceController.InterruptAim(fc);
    if (!flag1 && !flag2)
      StanceController.UnInterruptAim(fc);
    Vector3 vector3_2 = !StanceController.IsDoingLeftShoulderNotBlocked || StanceController.IsAiming ? Vector3.zero : rotTarget;
    vector3_2.x += StanceController._leftRotationXCurve.Evaluate(StanceController._leftStanceProgress);
    StanceController._leftStanceRotaiton = Vector3.Lerp(StanceController._leftStanceRotaiton, vector3_2, rotSpeed * dt);
    Quaternion quaternion = Quaternion.Euler(StanceController._leftStanceRotaiton);
    Transform weaponRoot = pwa.HandsContainer.WeaponRoot;
    weaponRoot.localRotation = Quaternion.op_Multiply(weaponRoot.localRotation, quaternion);
  }

  private static void HandleAltPistolPosition(
    Player player,
    Player.FirearmController fc,
    ProceduralWeaponAnimation pwa,
    float stanceMulti,
    float dt,
    Vector3 camTarget)
  {
    float num1 = StanceController._isLeftStanceResetState ? 0.2f : 1f;
    float num2 = Mathf.Pow(Utils.GetFPSFactor(), 0.25f);
    float num3 = StanceController.IsAiming ? PluginConfig.PistolPosResetSpeedMulti.Value * stanceMulti : PluginConfig.PistolPosSpeedMulti.Value * stanceMulti;
    float num4 = num2 * num1 * PluginConfig.PistolPosResetSpeedMulti.Value;
    StanceController._pistolPosSpeed = Mathf.Lerp(StanceController._pistolPosSpeed, num3, dt * 10f);
    if (!StanceController.IsAiming)
    {
      StanceController._gunXTarget = !StanceController.IsBlindFiring ? 0.038f : 0.0f;
      StanceController._gunYTarget = -0.0385f;
      StanceController._gunZTarget = 0.0f;
    }
    StanceController.CheckLeftShoulder(player, fc, pwa, StanceController._pistolPosSpeed, dt, StanceController._leftStancePistolPositionTarget, StanceController._leftStancePistolRotaitonTarget, stanceMulti * 2.5f, 0.05f);
    if (Plugin.FOVFixPresent)
    {
      StanceController.MoveGunToCameraPID(pwa, dt, stanceMulti, ref StanceController._gunXTarget, ref StanceController._gunCameraAlignmentTargetX, camTarget.x, 0.15f * num4, 0.0001f);
      StanceController.MoveGunToCameraPID(pwa, dt, stanceMulti, ref StanceController._gunYTarget, ref StanceController._gunCameraAlignmentTargetY, camTarget.y, 0.3f * num4, ignoreLeftShoulder: true);
      StanceController.MoveGunToCameraPID(pwa, dt, stanceMulti, ref StanceController._gunZTarget, ref StanceController._gunCameraAlignmentTargetZ, camTarget.z, 0.4f * num4, ignoreLeftShoulder: true);
    }
    StanceController._currentPistolXPos = Mathf.Lerp(StanceController._currentPistolXPos, StanceController._gunXTarget, dt * StanceController._pistolPosSpeed);
    StanceController._currentPistolYPos = Mathf.Lerp(StanceController._currentPistolYPos, StanceController._gunYTarget, dt * StanceController._pistolPosSpeed);
    StanceController._currentPistolZPos = Mathf.Lerp(StanceController._currentPistolZPos, StanceController._gunZTarget, dt * StanceController._pistolPosSpeed);
    StanceController._pistolLocalPosition.x = StanceController._currentPistolXPos + StanceController._leftStancePosition.x;
    StanceController._pistolLocalPosition.y = StanceController._currentPistolYPos + StanceController._leftStancePosition.y;
    StanceController._pistolLocalPosition.z = StanceController._currentPistolZPos + StanceController._leftStancePosition.z;
    pwa.HandsContainer.WeaponRoot.localPosition = StanceController._pistolLocalPosition;
  }

  private static void HandleRiflePosition(
    Player player,
    Player.FirearmController fc,
    ProceduralWeaponAnimation pwa,
    float stanceMulti,
    float movementFactor,
    float dt,
    Vector3 camTarget)
  {
    float num1 = StanceController._isLeftStanceResetState ? 0.0f : 1f;
    float num2 = Mathf.Pow(Utils.GetFPSFactor(), 0.25f);
    float num3 = StanceController.IsAiming ? 30f * WeaponStats.TotalFinalAimSpeed : 6f * WeaponStats.TotalFinalAimSpeed;
    float num4 = 30f * num2 * num1;
    Vector3 stancePidModifier = StanceController.GetRifleStancePIDModifier();
    bool flag = StanceController.IsCantedAiming(pwa, false) && StanceController.WasActiveAim;
    if (!StanceController.IsAiming)
    {
      StanceController._gunXTarget = StanceController.BaseWeaponOffsetPosition.x + PluginConfig.WeapOffset.Value.x;
      StanceController._gunYTarget = StanceController.BaseWeaponOffsetPosition.y + PluginConfig.WeapOffset.Value.y;
      StanceController._gunZTarget = StanceController.BaseWeaponOffsetPosition.z + PluginConfig.WeapOffset.Value.z;
    }
    StanceController.CheckLeftShoulder(player, fc, pwa, stanceMulti, dt, StanceController._leftStanceRiflePositionTarget, StanceController._leftStanceRifleRotaitonTarget, stanceMulti * 4.5f);
    if (PluginConfig.EnableAltRifle.Value && Plugin.FOVFixPresent)
    {
      StanceController.MoveGunToCameraPID(pwa, dt, WeaponStats.TotalFinalAimSpeed, ref StanceController._gunXTarget, ref StanceController._gunCameraAlignmentTargetX, camTarget.x, 0.3f * num4 * stancePidModifier.x, 0.0001f);
      StanceController.MoveGunToCameraPID(pwa, dt, WeaponStats.TotalFinalAimSpeed, ref StanceController._gunYTarget, ref StanceController._gunCameraAlignmentTargetY, camTarget.y, 0.3f * num4 * stancePidModifier.y, 0.0001f, true);
      StanceController.MoveGunToCameraPID(pwa, dt, WeaponStats.TotalFinalAimSpeed, ref StanceController._gunZTarget, ref StanceController._gunCameraAlignmentTargetZ, camTarget.z, 0.3f * num4 * stancePidModifier.z, 0.0001f, true);
    }
    StanceController._currentRifleXPos = Mathf.Lerp(StanceController._currentRifleXPos, StanceController._gunXTarget, dt * num3);
    StanceController._currentRifleYPos = Mathf.Lerp(StanceController._currentRifleYPos, StanceController._gunYTarget, dt * num3);
    StanceController._currentRifleZPos = Mathf.Lerp(StanceController._currentRifleZPos, StanceController._gunZTarget, dt * num3);
    StanceController._rifleLocalPosition.x = StanceController._currentRifleXPos + StanceController._leftStancePosition.x;
    StanceController._rifleLocalPosition.y = StanceController._currentRifleYPos + StanceController._leftStancePosition.y;
    StanceController._rifleLocalPosition.z = StanceController._currentRifleZPos + StanceController._leftStancePosition.z;
    pwa.HandsContainer.WeaponRoot.localPosition = StanceController._rifleLocalPosition;
  }

  public static void DoPistolStances(
    bool isThirdPerson,
    ProceduralWeaponAnimation pwa,
    float dt,
    Player player,
    Player.FirearmController fc,
    Vector3 camTarget)
  {
    bool flag = isThirdPerson;
    float num1 = (float) (1.0 + (double) PlayerState.TotalModifiedWeightMinusWeapon / 100.0);
    float stanceMulti = Mathf.Clamp(Mathf.Clamp(WeaponStats.ErgoStanceSpeed * Mathf.Pow(WeaponStats.TotalWeaponHandlingModi, 0.5f), 0.65f, 1.45f) * PlayerState.StanceInjuryMulti * Plugin.RealHealthController.AdrenalineStanceBonus * Mathf.Max(PlayerState.RemainingArmStamFactor, 0.55f), 0.5f, 1.45f);
    float num2 = (float) (1.0 - (double) stanceMulti + 1.0);
    float num3 = Mathf.Clamp(WeaponStats.ErgoStanceSpeed * 0.25f, 0.1f, 1f);
    StanceController.WiggleReturnSpeed = (float) (1.0 - (double) PlayerState.AimSkillADSBuff * 0.5) * num3 * PlayerState.StanceInjuryMulti * num1 * Mathf.Max(PlayerState.RemainingArmStamFactor, 0.65f);
    float num4 = PlayerState.IsMoving ? 0.8f : 1f;
    Quaternion quaternion1 = Quaternion.Euler(PluginConfig.PistolResetRotation.Value);
    Vector3 vector3_1 = flag ? PluginConfig.PistolThirdPersonPosition.Value : PluginConfig.PistolOffset.Value;
    Vector3 vector3_2 = flag ? new Vector3(0.01f, 0.025f, -0.015f) : new Vector3(0.01f, 0.025f, -0.015f);
    Vector3 vector3_3 = PlayerState.IsScav ? vector3_2 : vector3_1;
    Vector3 vector3_4 = flag ? PluginConfig.PistolThirdPersonRotation.Value : PluginConfig.PistolRotation.Value;
    Vector3 vector3_5 = flag ? new Vector3(2f, -10f, 0.0f) : new Vector3(2f, -10f, 0.0f);
    Quaternion quaternion2 = Quaternion.Euler(PlayerState.IsScav ? vector3_5 : vector3_4);
    Quaternion quaternion3 = Quaternion.Euler(PluginConfig.PistolAdditionalRotation.Value);
    StanceController.HandleAltPistolPosition(player, fc, pwa, stanceMulti, dt, camTarget);
    if (StanceController.CurrentStance == EStance.PatrolStance)
      return;
    if (!pwa.IsAiming && !StanceController.IsBlindFiring && !StanceController.PistolIsColliding && !WeaponStats.HasShoulderContact && PluginConfig.EnableAltPistol.Value)
    {
      if (StanceController.CurrentStance == EStance.PatrolStance || StanceController._previousStance == EStance.PatrolStance)
        StanceController._SkipPistolWiggle = true;
      StanceController.CurrentStance = EStance.PistolCompressed;
      StanceController.StoredStance = EStance.None;
      StanceController.IsResettingPistol = false;
      StanceController.HasResetPistolPos = false;
      ((Player.ValueBlender) StanceController.StanceBlender).Speed = PluginConfig.PistolPosSpeedMulti.Value * stanceMulti;
      StanceController.StanceTargetPosition = Vector3.Lerp(StanceController.StanceTargetPosition, vector3_3, PluginConfig.StanceTransitionSpeedMulti.Value * stanceMulti * dt);
      if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value < 1.0)
      {
        StanceController.StanceRotationSpeed = 4f * stanceMulti * dt * PluginConfig.PistolAdditionalRotationSpeedMulti.Value * stanceMulti;
        StanceController.StanceRotation = quaternion3;
      }
      else
      {
        StanceController.StanceRotationSpeed = (float) (4.0 * (double) stanceMulti * (double) dt * (double) PluginConfig.PistolRotationSpeedMulti.Value * (double) stanceMulti * (flag ? (double) PluginConfig.ThirdPersonRotationSpeed.Value : 1.0));
        StanceController.StanceRotation = quaternion2;
      }
      if (Vector3.op_Equality(StanceController.StanceTargetPosition, vector3_3) && (double) ((Player.ValueBlender) StanceController.StanceBlender).Value >= 1.0 && !StanceController.CanResetDamping)
        StanceController.DoDampingTimer = true;
      else if (Vector3.op_Inequality(StanceController.StanceTargetPosition, vector3_3) || (double) ((Player.ValueBlender) StanceController.StanceBlender).Value < 1.0)
        StanceController.CanResetDamping = false;
      if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value < 0.949999988079071 || StanceController.CancelPistolStance)
        StanceController.DidStanceWiggle = false;
      if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value < 1.0 || !Vector3.op_Equality(StanceController.StanceTargetPosition, vector3_3) || StanceController.DidStanceWiggle)
        return;
      if (!StanceController._SkipPistolWiggle && !StanceController.IsLeftShoulder)
        StanceController.DoWiggleEffects(player, pwa, fc.Weapon, Vector3.op_Multiply(new Vector3(-12.5f, 5f, 1f), num4));
      StanceController.DidStanceWiggle = true;
      StanceController.CancelPistolStance = false;
      StanceController._SkipPistolWiggle = false;
    }
    else if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value > 0.0 && !StanceController.HasResetPistolPos && !StanceController.PistolIsColliding)
    {
      StanceController.CanResetDamping = false;
      StanceController.IsResettingPistol = true;
      StanceController.StanceRotationSpeed = (float) (4.0 * (double) stanceMulti * (double) dt * (double) PluginConfig.PistolResetRotationSpeedMulti.Value * (double) stanceMulti * (flag ? (double) PluginConfig.ThirdPersonRotationSpeed.Value : 1.0));
      StanceController.StanceRotation = quaternion1;
      ((Player.ValueBlender) StanceController.StanceBlender).Speed = (float) ((double) PluginConfig.PistolPosResetSpeedMulti.Value * (double) stanceMulti * (flag ? (double) PluginConfig.ThirdPersonPositionSpeed.Value : 1.0));
    }
    else
    {
      if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value != 0.0 || StanceController.HasResetPistolPos || StanceController.PistolIsColliding)
        return;
      if (!StanceController.CanResetDamping)
        StanceController.DoDampingTimer = true;
      if (!StanceController.IsLeftShoulder)
        StanceController.DoWiggleEffects(player, pwa, fc.Weapon, Vector3.op_Multiply(new Vector3(-10f, 0.0f, -20f), num4));
      StanceController.IsResettingPistol = false;
      StanceController.CurrentStance = EStance.None;
      StanceController.StanceRotation = Quaternion.identity;
      StanceController.HasResetPistolPos = true;
    }
  }

  public static void DoRifleStances(
    Player player,
    Player.FirearmController fc,
    bool isThirdPerson,
    ProceduralWeaponAnimation pwa,
    float dt,
    Vector3 camTarget)
  {
    float movementFactor = PlayerState.IsMoving ? 1.1f : 1f;
    bool useThirdPersonStance = isThirdPerson;
    float num1 = (float) (1.0 + (double) PlayerState.TotalModifiedWeightMinusWeapon / 150.0);
    float num2 = (double) WeaponStats.TotalWeaponWeight >= 8.0 ? 0.45f : 0.55f;
    float num3 = Mathf.Clamp(1.15f * WeaponStats.ErgoStanceSpeed * Mathf.Pow(WeaponStats.TotalWeaponHandlingModi, 0.4f), num2, 1.2f);
    float num4 = (double) WeaponStats.TotalWeaponWeight >= 8.0 ? 0.3f : 0.4f;
    float stanceMulti = Mathf.Clamp(num3 * PlayerState.StanceInjuryMulti * Plugin.RealHealthController.AdrenalineStanceBonus * Mathf.Max(PlayerState.RemainingArmStamFactor, 0.65f), num4, 1.18f);
    float resetErgoMulti = (float) (1.0 - (double) stanceMulti + 1.0);
    bool pauseStance = PlayerState.IsInInventory || StanceController.IsBlindFiring || StanceController.IsLeftShoulder;
    float num5 = Mathf.Clamp(WeaponStats.ErgoStanceSpeed * 0.5f, 0.1f, 1f);
    float num6 = WeaponStats.HasShoulderContact ? 1f : 0.5f;
    StanceController.WiggleReturnSpeed = (float) (1.0 - (double) PlayerState.AimSkillADSBuff * 0.5) * num5 * PlayerState.StanceInjuryMulti * num6 * num1 * Mathf.Max(PlayerState.RemainingArmStamFactor, 0.55f);
    if (!isThirdPerson)
      StanceController.HandleRiflePosition(player, fc, pwa, stanceMulti, movementFactor, dt, camTarget);
    StanceController.DoTacSprint(fc, player);
    StanceController.DoShortStock(player, fc, isThirdPerson, pwa, dt, useThirdPersonStance, stanceMulti, resetErgoMulti, pauseStance, movementFactor);
    StanceController.DoHighReady(player, fc, isThirdPerson, pwa, dt, useThirdPersonStance, stanceMulti, resetErgoMulti, pauseStance, movementFactor);
    StanceController.DoLowReady(player, fc, isThirdPerson, pwa, dt, useThirdPersonStance, stanceMulti, resetErgoMulti, pauseStance, movementFactor);
    StanceController.DoActiveAim(player, fc, isThirdPerson, pwa, dt, useThirdPersonStance, stanceMulti, resetErgoMulti, pauseStance, movementFactor);
    StanceController.DoMeleeStance(player, fc, isThirdPerson, pwa, dt, useThirdPersonStance, stanceMulti, resetErgoMulti, pauseStance, movementFactor);
  }

  public static void DoShortStock(
    Player player,
    Player.FirearmController fc,
    bool isThirdPerson,
    ProceduralWeaponAnimation pwa,
    float dt,
    bool useThirdPersonStance,
    float stanceMulti,
    float resetErgoMulti,
    bool pauseStance,
    float movementFactor)
  {
    float num1 = Mathf.Clamp(stanceMulti, 0.65f, 1.5f);
    Quaternion quaternion1 = Quaternion.Euler(useThirdPersonStance ? PluginConfig.ShortStockThirdPersonRotation.Value : Vector3.op_Multiply(PluginConfig.ShortStockRotation.Value, num1));
    Quaternion quaternion2 = Quaternion.Euler(Vector3.op_Multiply(PluginConfig.ShortStockAdditionalRotation.Value, resetErgoMulti));
    Quaternion quaternion3 = Quaternion.Euler(Vector3.op_Multiply(PluginConfig.ShortStockResetRotation.Value, resetErgoMulti));
    Vector3 vector3 = useThirdPersonStance ? PluginConfig.ShortStockThirdPersonPosition.Value : PluginConfig.ShortStockOffset.Value;
    if (StanceController.CurrentStance == EStance.ShortStock && !pwa.IsAiming && !StanceController.CancelShortStock && !StanceController.IsBlindFiring && !pwa.LeftStance && !PlayerState.IsSprinting && !pauseStance)
    {
      float num2 = 1f;
      float num3 = 1f;
      float num4 = 1f;
      StanceController.IsResettingShortStock = false;
      StanceController.HasResetShortStock = false;
      StanceController.HasResetMelee = true;
      if (Vector3.op_Inequality(StanceController.StanceTargetPosition, vector3))
      {
        if (!StanceController.HasResetActiveAim)
          num2 = 0.55f;
        if (!StanceController.HasResetHighReady)
          num3 = 0.78f;
        if (!StanceController.HasResetLowReady)
          num4 = 0.55f;
      }
      else
      {
        StanceController.HasResetActiveAim = true;
        StanceController.HasResetHighReady = true;
        StanceController.HasResetLowReady = true;
      }
      if (Vector3.op_Equality(StanceController.StanceTargetPosition, vector3) && (double) ((Player.ValueBlender) StanceController.StanceBlender).Value >= 1.0 && !StanceController.CanResetDamping)
        StanceController.DoDampingTimer = true;
      else if (Vector3.op_Inequality(StanceController.StanceTargetPosition, vector3) || (double) ((Player.ValueBlender) StanceController.StanceBlender).Value < 1.0)
        StanceController.CanResetDamping = false;
      float num5 = num2 * num3 * num4;
      float num6 = num2 * num3 * num4;
      if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value < 1.0)
      {
        StanceController.StanceRotationSpeed = (float) (4.0 * (double) num1 * (double) dt * (double) PluginConfig.ShortStockAdditionalRotationSpeedMulti.Value * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonRotationSpeed.Value : 1.0)) * num6;
        StanceController.StanceRotation = quaternion2;
      }
      else
      {
        StanceController.StanceRotationSpeed = (float) (4.0 * (double) num1 * (double) dt * (double) PluginConfig.ShortStockRotationMulti.Value * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonRotationSpeed.Value : 1.0)) * num6;
        StanceController.StanceRotation = quaternion1;
      }
      ((Player.ValueBlender) StanceController.StanceBlender).Speed = (float) ((double) PluginConfig.ShortStockSpeedMulti.Value * (double) num1 * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonPositionSpeed.Value : 1.0));
      StanceController.StanceTargetPosition = Vector3.Lerp(StanceController.StanceTargetPosition, vector3, PluginConfig.StanceTransitionSpeedMulti.Value * num1 * num5 * dt);
      if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value < 0.89999997615814209 && !Vector3.op_Equality(StanceController.StanceTargetPosition, vector3) || StanceController.DidStanceWiggle || useThirdPersonStance)
        return;
      StanceController.DoWiggleEffects(player, pwa, fc.Weapon, Vector3.op_Multiply(new Vector3(5f, -2.5f, 30f), movementFactor), true);
      StanceController.DidStanceWiggle = true;
    }
    else if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value > 0.0 && !StanceController.HasResetShortStock && StanceController.CurrentStance != EStance.LowReady && StanceController.CurrentStance != EStance.ActiveAiming && StanceController.CurrentStance != EStance.HighReady && !StanceController.IsResettingActiveAim && !StanceController.IsResettingHighReady && !StanceController.IsResettingLowReady && !StanceController.IsResettingMelee)
    {
      StanceController.CanResetDamping = false;
      StanceController.IsResettingShortStock = true;
      StanceController.StanceRotationSpeed = 4f * num1 * dt * PluginConfig.ShortStockResetRotationSpeedMulti.Value;
      StanceController.StanceRotation = quaternion3;
      ((Player.ValueBlender) StanceController.StanceBlender).Speed = (float) ((double) PluginConfig.ShortStockResetSpeedMulti.Value * (double) num1 * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonPositionSpeed.Value : 1.0));
    }
    else
    {
      if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value != 0.0 || StanceController.HasResetShortStock)
        return;
      if (!StanceController.CanResetDamping)
        StanceController.DoDampingTimer = true;
      if (!useThirdPersonStance)
        StanceController.DoWiggleEffects(player, pwa, fc.Weapon, Vector3.op_Multiply(new Vector3(-4f, -2f, -30f), movementFactor), true);
      StanceController.DidStanceWiggle = false;
      StanceController.StanceRotation = Quaternion.identity;
      StanceController.IsResettingShortStock = false;
      StanceController.HasResetShortStock = true;
    }
  }

  public static void DoHighReady(
    Player player,
    Player.FirearmController fc,
    bool isThirdPerson,
    ProceduralWeaponAnimation pwa,
    float dt,
    bool useThirdPersonStance,
    float stanceMulti,
    float resetErgoMulti,
    bool pauseStance,
    float movementFactor)
  {
    float num1 = Mathf.Clamp(stanceMulti, 0.5f, 0.98f);
    float num2 = (double) WeaponStats.TotalErgo <= 49.0 ? -1f : 1f;
    float num3 = (double) WeaponStats.TotalErgo <= 40.0 ? 1f : 2f;
    Vector3 vector3_1 = useThirdPersonStance ? PluginConfig.HighReadyThirdPersonRotation.Value : new Vector3(PluginConfig.HighReadyRotation.Value.x * stanceMulti, (float) ((double) PluginConfig.HighReadyRotation.Value.y * (double) stanceMulti * (StanceController.ModifyHighReady ? -1.0 : 1.0)), PluginConfig.HighReadyRotation.Value.z * stanceMulti);
    Vector3 vector3_2 = useThirdPersonStance ? PluginConfig.HighReadyThirdPersonPosition.Value : new Vector3(PluginConfig.HighReadyOffset.Value.x, PluginConfig.HighReadyOffset.Value.y * (StanceController.ModifyHighReady ? 0.25f : 1f), PluginConfig.HighReadyOffset.Value.z);
    Quaternion quaternion1 = Quaternion.Euler(vector3_1);
    Quaternion quaternion2 = Quaternion.Euler(Vector3.op_Multiply(PluginConfig.HighReadyAdditionalRotation.Value, resetErgoMulti));
    Quaternion quaternion3 = Quaternion.Euler(Vector3.op_Multiply(PluginConfig.HighReadyResetRotation.Value, resetErgoMulti));
    if (StanceController.CurrentStance == EStance.HighReady && !pwa.IsAiming && !StanceController.IsFiringFromStance && !StanceController.CancelHighReady && !pauseStance)
    {
      float num4 = 1f;
      float num5 = 1f;
      float num6 = 1f;
      StanceController.IsResettingHighReady = false;
      StanceController.HasResetHighReady = false;
      StanceController.HasResetMelee = true;
      if (Vector3.op_Inequality(StanceController.StanceTargetPosition, vector3_2))
      {
        if (!StanceController.HasResetShortStock)
          num4 = 0.82f;
        if (!StanceController.HasResetActiveAim)
          num6 = 1f;
        if (!StanceController.HasResetLowReady)
          num5 = 1f;
      }
      else
      {
        StanceController.HasResetActiveAim = true;
        StanceController.HasResetLowReady = true;
        StanceController.HasResetShortStock = true;
      }
      if (Vector3.op_Equality(StanceController.StanceTargetPosition, vector3_2) && (double) ((Player.ValueBlender) StanceController.StanceBlender).Value == 1.0 && !StanceController.CanResetDamping)
        StanceController.DoDampingTimer = true;
      else if (Vector3.op_Inequality(StanceController.StanceTargetPosition, vector3_2) || (double) ((Player.ValueBlender) StanceController.StanceBlender).Value < 1.0)
        StanceController.CanResetDamping = false;
      float num7 = num4 * num5 * num6;
      float num8 = (float) ((double) num4 * (double) num5 * (double) num6 * ((double) num7 != 1.0 ? 0.89999997615814209 : 1.0));
      if (StanceController.CanDoHighReadyInjuredAnim)
      {
        if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value < 0.30000001192092896)
        {
          Quaternion quaternion4 = Quaternion.Euler(useThirdPersonStance ? PluginConfig.LowReadyThirdPersonRotation.Value : new Vector3(PluginConfig.LowReadyRotation.Value.x * resetErgoMulti, PluginConfig.LowReadyRotation.Value.y, PluginConfig.LowReadyRotation.Value.z));
          StanceController.StanceRotationSpeed = (float) (3.0 * (double) num1 * (double) dt * (double) PluginConfig.HighReadyRotationMulti.Value * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonRotationSpeed.Value * 0.699999988079071 : 1.0) * (WeaponStats.IsPistol ? 0.5 : 1.0));
          StanceController.StanceRotation = quaternion4;
        }
        else
        {
          StanceController.StanceRotationSpeed = (float) (3.0 * (double) num1 * (double) dt * (double) PluginConfig.HighReadyAdditionalRotationSpeedMulti.Value * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonRotationSpeed.Value * 0.20000000298023224 : 1.0) * (WeaponStats.IsPistol ? 0.5 : 1.0));
          StanceController.StanceRotation = quaternion2;
        }
      }
      else if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value < 0.30000001192092896)
      {
        StanceController.StanceRotationSpeed = (float) (4.0 * (double) num1 * (double) dt * (double) PluginConfig.HighReadyAdditionalRotationSpeedMulti.Value * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonRotationSpeed.Value * 0.20000000298023224 : 1.0) * (double) num8 * (WeaponStats.IsPistol ? 0.5 : 1.0));
        StanceController.StanceRotation = quaternion2;
      }
      else
      {
        StanceController.StanceRotationSpeed = (float) (4.0 * (double) num1 * (double) dt * (double) PluginConfig.HighReadyRotationMulti.Value * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonRotationSpeed.Value * 0.699999988079071 : 1.0) * (double) num8 * (WeaponStats.IsPistol ? 0.5 : 1.0));
        StanceController.StanceRotation = quaternion1;
      }
      ((Player.ValueBlender) StanceController.StanceBlender).Speed = (float) ((double) PluginConfig.HighReadySpeedMulti.Value * (double) num1 * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonPositionSpeed.Value : 1.0));
      StanceController.StanceTargetPosition = Vector3.Lerp(StanceController.StanceTargetPosition, vector3_2, PluginConfig.StanceTransitionSpeedMulti.Value * num1 * num7 * dt);
      if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value < 1.0 && !Vector3.op_Equality(StanceController.StanceTargetPosition, vector3_2) || StanceController.DidStanceWiggle || useThirdPersonStance)
        return;
      if (!WeaponStats.IsPistol)
        StanceController.DoWiggleEffects(player, pwa, fc.Weapon, Vector3.op_Multiply(new Vector3(5f, 5f, 5f), movementFactor), true);
      StanceController.DidStanceWiggle = true;
    }
    else if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value > 0.0 && !StanceController.HasResetHighReady && StanceController.CurrentStance != EStance.LowReady && StanceController.CurrentStance != EStance.ActiveAiming && StanceController.CurrentStance != EStance.ShortStock && !StanceController.IsResettingActiveAim && !StanceController.IsResettingLowReady && !StanceController.IsResettingShortStock && !StanceController.IsResettingMelee)
    {
      StanceController.CanResetDamping = false;
      StanceController.IsResettingHighReady = true;
      StanceController.StanceRotationSpeed = (float) (4.0 * (double) num1 * (double) dt * (double) PluginConfig.HighReadyResetRotationMulti.Value * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonRotationSpeed.Value : 1.0));
      StanceController.StanceRotation = quaternion3;
      ((Player.ValueBlender) StanceController.StanceBlender).Speed = (float) ((double) PluginConfig.HighReadyResetSpeedMulti.Value * (double) num1 * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonPositionSpeed.Value : 1.0));
    }
    else
    {
      if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value > 0.0 || StanceController.HasResetHighReady)
        return;
      if (!StanceController.CanResetDamping)
        StanceController.DoDampingTimer = true;
      if (!useThirdPersonStance && !WeaponStats.IsPistol)
        StanceController.DoWiggleEffects(player, pwa, fc.Weapon, Vector3.op_Multiply(new Vector3(num2 * 10f, num2 * 1f, num3 * -10f), movementFactor), true);
      StanceController.DidStanceWiggle = false;
      StanceController.StanceRotation = Quaternion.identity;
      StanceController.IsResettingHighReady = false;
      StanceController.HasResetHighReady = true;
    }
  }

  public static void DoLowReady(
    Player player,
    Player.FirearmController fc,
    bool isThirdPerson,
    ProceduralWeaponAnimation pwa,
    float dt,
    bool useThirdPersonStance,
    float stanceMulti,
    float resetErgoMulti,
    bool pauseStance,
    float movementFactor)
  {
    float num1 = Mathf.Clamp(stanceMulti, 0.5f, 0.98f);
    Quaternion quaternion1 = Quaternion.Euler(useThirdPersonStance ? PluginConfig.LowReadyThirdPersonRotation.Value : new Vector3(PluginConfig.LowReadyRotation.Value.x * resetErgoMulti, PluginConfig.LowReadyRotation.Value.y, PluginConfig.LowReadyRotation.Value.z));
    Quaternion quaternion2 = Quaternion.Euler(Vector3.op_Multiply(PluginConfig.LowReadyAdditionalRotation.Value, resetErgoMulti));
    Quaternion quaternion3 = Quaternion.Euler(Vector3.op_Multiply(PluginConfig.LowReadyResetRotation.Value, resetErgoMulti));
    Vector3 vector3 = useThirdPersonStance ? PluginConfig.LowReadyThirdPersonPosition.Value : PluginConfig.LowReadyOffset.Value;
    if (StanceController.CurrentStance == EStance.LowReady && !pwa.IsAiming && !StanceController.IsFiringFromStance && !StanceController.CancelLowReady && !pauseStance)
    {
      float num2 = 1f;
      float num3 = 1f;
      float num4 = 1f;
      StanceController.IsResettingLowReady = false;
      StanceController.HasResetLowReady = false;
      StanceController.HasResetMelee = true;
      if (Vector3.op_Inequality(StanceController.StanceTargetPosition, vector3))
      {
        if (!StanceController.HasResetHighReady)
          num2 = 0.95f;
        if (!StanceController.HasResetShortStock)
          num3 = 0.7f;
        if (!StanceController.HasResetActiveAim)
          num4 = 0.87f;
      }
      else
      {
        StanceController.HasResetHighReady = true;
        StanceController.HasResetShortStock = true;
        StanceController.HasResetActiveAim = true;
      }
      if (Vector3.op_Equality(StanceController.StanceTargetPosition, vector3) && (double) ((Player.ValueBlender) StanceController.StanceBlender).Value >= 1.0 && !StanceController.CanResetDamping)
        StanceController.DoDampingTimer = true;
      else if (Vector3.op_Inequality(StanceController.StanceTargetPosition, vector3) || (double) ((Player.ValueBlender) StanceController.StanceBlender).Value < 1.0)
        StanceController.CanResetDamping = false;
      float num5 = num2 * num3 * num4;
      float num6 = (float) ((double) num2 * (double) num3 * (double) num4 * ((double) num5 != 1.0 ? 1.0249999761581421 : 1.0));
      if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value < 1.0)
      {
        StanceController.StanceRotationSpeed = (float) (4.0 * (double) num1 * (double) dt * (double) PluginConfig.LowReadyAdditionalRotationSpeedMulti.Value * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonRotationSpeed.Value * 0.800000011920929 : 1.0)) * num6;
        StanceController.StanceRotation = quaternion2;
      }
      else
      {
        StanceController.StanceRotationSpeed = (float) (4.0 * (double) num1 * (double) dt * (double) PluginConfig.LowReadyRotationMulti.Value * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonRotationSpeed.Value * 0.800000011920929 : 1.0)) * num6;
        StanceController.StanceRotation = quaternion1;
      }
      ((Player.ValueBlender) StanceController.StanceBlender).Speed = (float) ((double) PluginConfig.LowReadySpeedMulti.Value * (double) num1 * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonPositionSpeed.Value * 0.800000011920929 : 1.0));
      StanceController.StanceTargetPosition = Vector3.Lerp(StanceController.StanceTargetPosition, vector3, PluginConfig.StanceTransitionSpeedMulti.Value * num1 * num5 * dt);
      if (((double) ((Player.ValueBlender) StanceController.StanceBlender).Value >= 0.5 || Vector3.op_Equality(StanceController.StanceTargetPosition, vector3)) && !StanceController.DidStanceWiggle && !useThirdPersonStance)
      {
        StanceController.DoWiggleEffects(player, pwa, fc.Weapon, Vector3.op_Multiply(new Vector3(7f, 7f, 0.0f), movementFactor), true);
        StanceController.DidStanceWiggle = true;
      }
      StanceController.DidLowReadyResetStanceWiggle = false;
    }
    else if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value > 0.0 && !StanceController.HasResetLowReady && StanceController.CurrentStance != EStance.ActiveAiming && StanceController.CurrentStance != EStance.HighReady && StanceController.CurrentStance != EStance.ShortStock && !StanceController.IsResettingActiveAim && !StanceController.IsResettingHighReady && !StanceController.IsResettingShortStock && !StanceController.IsResettingMelee)
    {
      StanceController.CanResetDamping = false;
      StanceController.IsResettingLowReady = true;
      StanceController.StanceRotationSpeed = (float) (4.0 * (double) num1 * (double) dt * (double) PluginConfig.LowReadyResetRotationMulti.Value * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonRotationSpeed.Value * 0.800000011920929 : 1.0));
      StanceController.StanceRotation = quaternion3;
      ((Player.ValueBlender) StanceController.StanceBlender).Speed = (float) ((double) PluginConfig.LowReadyResetSpeedMulti.Value * (double) num1 * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonPositionSpeed.Value * 0.800000011920929 : 1.0));
      if (useThirdPersonStance || (double) ((Player.ValueBlender) StanceController.StanceBlender).Value > 0.64999997615814209 || StanceController.DidLowReadyResetStanceWiggle)
        return;
      StanceController.DoWiggleEffects(player, pwa, fc.Weapon, Vector3.op_Multiply(new Vector3(-10f, 4f, 10f), movementFactor), true);
      StanceController.DidLowReadyResetStanceWiggle = true;
    }
    else
    {
      if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value != 0.0 || StanceController.HasResetLowReady)
        return;
      if (!StanceController.CanResetDamping)
        StanceController.DoDampingTimer = true;
      StanceController.StanceRotation = Quaternion.identity;
      StanceController.IsResettingLowReady = false;
      StanceController.HasResetLowReady = true;
    }
  }

  public static void DoActiveAim(
    Player player,
    Player.FirearmController fc,
    bool isThirdPerson,
    ProceduralWeaponAnimation pwa,
    float dt,
    bool useThirdPersonStance,
    float stanceMulti,
    float resetErgoMulti,
    bool pauseStance,
    float movementFactor)
  {
    Vector3 vector3_1 = useThirdPersonStance ? PluginConfig.ActiveThirdPersonRotation.Value : PluginConfig.ActiveAimRotation.Value;
    Quaternion.Euler(Vector3.op_Multiply(PluginConfig.ActiveAimAdditionalRotation.Value, resetErgoMulti));
    Quaternion quaternion1 = StanceController.IsCantedAiming(pwa, true) ? Quaternion.Euler(Vector3.op_Multiply(new Vector3(0.0f, 10f, -1f), resetErgoMulti)) : Quaternion.Euler(Vector3.op_Multiply(PluginConfig.ActiveAimResetRotation.Value, resetErgoMulti));
    Vector3 vector3_2 = useThirdPersonStance ? PluginConfig.ActiveThirdPersonPosition.Value : PluginConfig.ActiveAimOffset.Value;
    Quaternion quaternion2 = Quaternion.Euler(vector3_1);
    if (StanceController.CurrentStance == EStance.ActiveAiming && !StanceController.CancelActiveAim && !pauseStance)
    {
      float num1 = (double) WeaponStats.TotalErgo <= 40.0 ? 0.75f : 1f;
      float num2 = 1f;
      float num3 = 1f;
      float num4 = 1f;
      float num5 = 1f;
      float num6 = 1f;
      float num7 = 1f;
      StanceController.IsResettingActiveAim = false;
      StanceController.HasResetActiveAim = false;
      StanceController.HasResetMelee = true;
      if (Vector3.op_Inequality(StanceController.StanceTargetPosition, vector3_2))
      {
        if (!StanceController.HasResetShortStock)
        {
          num2 = 0.45f;
          num3 = 0.9f;
        }
        if (!StanceController.HasResetHighReady)
        {
          num4 = 1.15f;
          num6 = 1.15f;
        }
        if (!StanceController.HasResetLowReady)
        {
          num5 = 1.29f;
          num7 = 1.37f;
        }
      }
      else
      {
        StanceController.HasResetShortStock = true;
        StanceController.HasResetHighReady = true;
        StanceController.HasResetLowReady = true;
      }
      if (Vector3.op_Equality(StanceController.StanceTargetPosition, vector3_2) && (double) ((Player.ValueBlender) StanceController.StanceBlender).Value == 1.0 && !StanceController.CanResetDamping)
        StanceController.DoDampingTimer = true;
      else if (Vector3.op_Inequality(StanceController.StanceTargetPosition, vector3_2) || (double) ((Player.ValueBlender) StanceController.StanceBlender).Value < 1.0)
        StanceController.CanResetDamping = false;
      float num8 = num2 * num4 * num5;
      float num9 = num3 * num6 * num7;
      StanceController.StanceTargetPosition = Vector3.Lerp(StanceController.StanceTargetPosition, vector3_2, PluginConfig.StanceTransitionSpeedMulti.Value * stanceMulti * num8 * dt);
      StanceController.StanceRotationSpeed = (float) (4.0 * (double) stanceMulti * (double) dt * (double) num1 * (double) PluginConfig.ActiveAimRotationSpeedMulti.Value * (double) StanceController.ChonkerFactor * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonRotationSpeed.Value : 1.0)) * num9;
      StanceController.StanceRotation = quaternion2;
      ((Player.ValueBlender) StanceController.StanceBlender).Speed = (float) ((double) PluginConfig.ActiveAimPosSpeedMulti.Value * (double) stanceMulti * (double) num1 * (double) StanceController.ChonkerFactor * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonPositionSpeed.Value : 1.0));
      if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value < 1.0 && !Vector3.op_Equality(StanceController.StanceTargetPosition, vector3_2) || StanceController.DidStanceWiggle || useThirdPersonStance)
        return;
      StanceController.DoWiggleEffects(player, pwa, fc.Weapon, new Vector3(-10f, -10f, 0.0f), true, 3f);
      StanceController.DidStanceWiggle = true;
    }
    else if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value > 0.0 && !StanceController.HasResetActiveAim && StanceController.CurrentStance != EStance.LowReady && StanceController.CurrentStance != EStance.HighReady && StanceController.CurrentStance != EStance.ShortStock && !StanceController.IsResettingLowReady && !StanceController.IsResettingHighReady && !StanceController.IsResettingShortStock && !StanceController.IsResettingMelee)
    {
      StanceController.CanResetDamping = false;
      StanceController.IsResettingActiveAim = true;
      StanceController.StanceRotationSpeed = (float) ((double) stanceMulti * (double) dt * (double) PluginConfig.ActiveAimResetRotationSpeedMulti.Value * (double) StanceController.ChonkerFactor * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonRotationSpeed.Value : 1.0));
      StanceController.StanceRotation = quaternion1;
      ((Player.ValueBlender) StanceController.StanceBlender).Speed = (float) ((double) PluginConfig.ActiveAimResetSpeedMulti.Value * (double) stanceMulti * (double) StanceController.ChonkerFactor * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonPositionSpeed.Value : 1.0));
    }
    else
    {
      if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value != 0.0 || StanceController.HasResetActiveAim)
        return;
      if (!StanceController.CanResetDamping)
        StanceController.DoDampingTimer = true;
      if (!useThirdPersonStance)
        StanceController.DoWiggleEffects(player, pwa, fc.Weapon, Vector3.op_Multiply(new Vector3(-5f, 1.5f, 0.0f), movementFactor), true, 3f);
      StanceController.DidStanceWiggle = false;
      StanceController.StanceRotation = Quaternion.identity;
      StanceController.IsResettingActiveAim = false;
      StanceController.HasResetActiveAim = true;
    }
  }

  public static void DoMeleeStance(
    Player player,
    Player.FirearmController fc,
    bool isThirdPerson,
    ProceduralWeaponAnimation pwa,
    float dt,
    bool useThirdPersonStance,
    float stanceMulti,
    float resetErgoMulti,
    bool pauseStance,
    float movementFactor)
  {
    if (WeaponStats.HasBayonet)
    {
      StanceController.DoMeleeStanceBayonet(player, fc, isThirdPerson, pwa, dt, useThirdPersonStance, stanceMulti, resetErgoMulti, pauseStance, movementFactor);
    }
    else
    {
      bool flag = StanceController.CurrentStance == EStance.Melee && !pwa.IsAiming && !pauseStance;
      Quaternion quaternion1 = Quaternion.Euler(new Vector3(2.5f * resetErgoMulti, -15f * resetErgoMulti, -1f));
      Quaternion quaternion2 = Quaternion.Euler(new Vector3(-1.5f * resetErgoMulti, -7.5f * resetErgoMulti, -0.5f));
      Vector3 vector3_1;
      // ISSUE: explicit constructor call
      ((Vector3) ref vector3_1).\u002Ector(0.0f, 0.06f, 0.0f);
      Vector3 vector3_2;
      // ISSUE: explicit constructor call
      ((Vector3) ref vector3_2).\u002Ector(0.0f, -0.0275f, 0.0f);
      if (flag && !PlayerState.IsSprinting)
      {
        StanceController.IsResettingMelee = false;
        StanceController.HasResetMelee = false;
        StanceController.HasResetActiveAim = true;
        StanceController.HasResetHighReady = true;
        StanceController.HasResetLowReady = true;
        StanceController.HasResetShortStock = true;
        if (Vector3.op_Equality(StanceController.StanceTargetPosition, vector3_2) && (double) ((Player.ValueBlender) StanceController.StanceBlender).Value >= 1.0 && !StanceController.CanResetDamping)
          StanceController.DoDampingTimer = true;
        else if (Vector3.op_Inequality(StanceController.StanceTargetPosition, vector3_2) || (double) ((Player.ValueBlender) StanceController.StanceBlender).Value < 1.0)
          StanceController.CanResetDamping = false;
        StanceController.StanceRotationSpeed = (float) (10.0 * (double) Mathf.Clamp(stanceMulti, 0.8f, 1f) * (double) dt * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonRotationSpeed.Value : 1.0));
        float num1 = Vector3.Distance(StanceController.StanceTargetPosition, vector3_1);
        float num2 = Vector3.Distance(StanceController.StanceTargetPosition, vector3_2);
        if ((double) num1 > 1.0 / 1000.0 && !StanceController.DidHalfMeleeAnim)
        {
          StanceController.StanceRotation = quaternion1;
          StanceController.StanceTargetPosition = Vector3.Lerp(StanceController.StanceTargetPosition, vector3_1, (float) ((double) PluginConfig.StanceTransitionSpeedMulti.Value * (double) Mathf.Clamp(stanceMulti, 0.75f, 1f) * (double) dt * 1.5) * StanceController.ChonkerFactor);
        }
        else
        {
          StanceController.DidHalfMeleeAnim = true;
          StanceController.StanceRotation = quaternion2;
          StanceController.StanceTargetPosition = Vector3.Lerp(StanceController.StanceTargetPosition, vector3_2, (float) ((double) PluginConfig.StanceTransitionSpeedMulti.Value * (double) Mathf.Clamp(stanceMulti, 0.75f, 1f) * (double) dt * 2.0) * StanceController.ChonkerFactor);
        }
        if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value >= 1.0 && (double) num2 <= 1.0 / 1000.0 && !StanceController.DidStanceWiggle)
        {
          StanceController.DoMeleeEffect();
          StanceController.DoWiggleEffects(player, pwa, fc.Weapon, Vector3.op_Multiply(new Vector3(-20f, -10f, -90f), movementFactor), true, 1f, useGearSound: true);
          StanceController.DidStanceWiggle = true;
        }
        if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value >= 0.89999997615814209 && StanceController.DidHalfMeleeAnim)
          StanceController.CanDoMeleeDetection = true;
        if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value < 1.0 || (double) num2 > 1.0 / 1000.0)
          return;
        StanceController.CurrentStance = StanceController.StoredStance;
        ((Player.ValueBlender) StanceController.StanceBlender).Target = 0.0f;
      }
      else if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value > 0.0 && !StanceController.HasResetMelee)
      {
        StanceController.CanDoMeleeDetection = false;
        StanceController.CanResetDamping = false;
        StanceController.IsResettingMelee = true;
        StanceController.StanceRotationSpeed = 10f * stanceMulti * dt;
        StanceController.StanceRotation = Quaternion.identity;
        ((Player.ValueBlender) StanceController.StanceBlender).Speed = (float) (15.0 * (double) stanceMulti * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonPositionSpeed.Value : 1.0));
      }
      else
      {
        if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value != 0.0 || StanceController.HasResetMelee)
          return;
        StanceController._doMeleeReset = true;
        if (!StanceController.CanResetDamping)
          StanceController.DoDampingTimer = true;
        StanceController.StanceRotation = Quaternion.identity;
        StanceController.IsResettingMelee = false;
        StanceController.HasResetMelee = true;
        StanceController.DidHalfMeleeAnim = false;
      }
    }
  }

  public static void DoMeleeStanceBayonet(
    Player player,
    Player.FirearmController fc,
    bool isThirdPerson,
    ProceduralWeaponAnimation pwa,
    float dt,
    bool useThirdPersonStance,
    float stanceMulti,
    float resetErgoMulti,
    bool pauseStance,
    float movementFactor)
  {
    bool flag = StanceController.CurrentStance == EStance.Melee && !pwa.IsAiming && !pauseStance;
    KeyboardShortcut keyboardShortcut = PluginConfig.MeleeKeybind.Value;
    StanceController._isHoldingBackMelee = ((!Input.GetKey(((KeyboardShortcut) ref keyboardShortcut).MainKey) ? 0 : (!StanceController.MeleeHitSomething ? 1 : 0)) & (flag ? 1 : 0)) != 0;
    Quaternion quaternion1 = Quaternion.Euler(new Vector3(2.5f * resetErgoMulti, -15f * resetErgoMulti, -1f));
    Quaternion quaternion2 = Quaternion.Euler(new Vector3(-1.5f * resetErgoMulti, -7.5f * resetErgoMulti, -0.5f));
    Vector3 vector3_1;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3_1).\u002Ector(0.0f, 0.06f, 0.0f);
    Vector3 vector3_2;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3_2).\u002Ector(0.0f, -0.0275f, 0.0f);
    if (flag)
    {
      StanceController.IsResettingMelee = false;
      StanceController.HasResetMelee = false;
      StanceController.HasResetActiveAim = true;
      StanceController.HasResetHighReady = true;
      StanceController.HasResetLowReady = true;
      StanceController.HasResetShortStock = true;
      if (Vector3.op_Equality(StanceController.StanceTargetPosition, vector3_2) && (double) ((Player.ValueBlender) StanceController.StanceBlender).Value >= 1.0 && !StanceController.CanResetDamping)
        StanceController.DoDampingTimer = true;
      else if (Vector3.op_Inequality(StanceController.StanceTargetPosition, vector3_2) || (double) ((Player.ValueBlender) StanceController.StanceBlender).Value < 1.0)
        StanceController.CanResetDamping = false;
      StanceController.StanceRotationSpeed = (float) (10.0 * (double) Mathf.Clamp(stanceMulti, 0.8f, 1f) * (double) dt * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonRotationSpeed.Value : 1.0));
      float num1 = Vector3.Distance(StanceController.StanceTargetPosition, vector3_1);
      float num2 = Vector3.Distance(StanceController.StanceTargetPosition, vector3_2);
      if ((double) num1 > 1.0 / 1000.0 && !StanceController.DidHalfMeleeAnim)
      {
        StanceController.StanceRotation = quaternion1;
        StanceController.StanceTargetPosition = Vector3.Lerp(StanceController.StanceTargetPosition, vector3_1, (float) ((double) PluginConfig.StanceTransitionSpeedMulti.Value * (double) Mathf.Clamp(stanceMulti, 0.75f, 1f) * (double) dt * 1.5) * StanceController.ChonkerFactor);
      }
      else
      {
        StanceController.DidHalfMeleeAnim = true;
        if (!StanceController._isHoldingBackMelee)
        {
          StanceController.StanceRotation = quaternion2;
          StanceController.StanceTargetPosition = Vector3.Lerp(StanceController.StanceTargetPosition, vector3_2, (float) ((double) PluginConfig.StanceTransitionSpeedMulti.Value * (double) Mathf.Clamp(stanceMulti, 0.75f, 1f) * (double) dt * 2.0) * StanceController.ChonkerFactor);
        }
      }
      ((Player.ValueBlender) StanceController.StanceBlender).Speed = (float) (50.0 * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonPositionSpeed.Value : 1.0));
      if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value >= 0.89999997615814209 && !StanceController.DidStanceWiggle && !StanceController.MeleeHitSomething && !StanceController._isHoldingBackMelee)
      {
        StanceController.DoMeleeEffect();
        StanceController.DoWiggleEffects(player, pwa, fc.Weapon, Vector3.op_Multiply(new Vector3(-20f, -10f, -90f), movementFactor), true, 1f, useGearSound: true);
        StanceController.DidStanceWiggle = true;
      }
      if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value >= 0.89999997615814209 && StanceController.DidHalfMeleeAnim)
        StanceController.CanDoMeleeDetection = true;
      if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value < 1.0 || (double) num2 > 1.0 / 1000.0)
        return;
      StanceController.CurrentStance = StanceController.StoredStance;
      ((Player.ValueBlender) StanceController.StanceBlender).Target = 0.0f;
    }
    else if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value > 0.0 && !StanceController.HasResetMelee)
    {
      StanceController.CanDoMeleeDetection = false;
      StanceController.CanResetDamping = false;
      StanceController.IsResettingMelee = true;
      StanceController.StanceRotationSpeed = 10f * stanceMulti * dt;
      StanceController.StanceRotation = Quaternion.identity;
      ((Player.ValueBlender) StanceController.StanceBlender).Speed = (float) (15.0 * (double) stanceMulti * (useThirdPersonStance ? (double) PluginConfig.ThirdPersonPositionSpeed.Value : 1.0));
    }
    else
    {
      if ((double) ((Player.ValueBlender) StanceController.StanceBlender).Value != 0.0 || StanceController.HasResetMelee)
        return;
      StanceController._doMeleeReset = true;
      if (!StanceController.CanResetDamping)
        StanceController.DoDampingTimer = true;
      StanceController.StanceRotation = Quaternion.identity;
      StanceController.IsResettingMelee = false;
      StanceController.HasResetMelee = true;
      StanceController.DidHalfMeleeAnim = false;
    }
  }

  public static void DoPatrolStance(ProceduralWeaponAnimation pwa, Player player)
  {
    Vector3 vector3_1 = StanceController.CurrentStance != EStance.PatrolStance ? Vector3.zero : (WeaponStats.IsStocklessPistol || WeaponStats.IsMachinePistol ? StanceController._pistolPatrolPos : StanceController._riflePatrolPos);
    StanceController._patrolPos = Vector3.Lerp(StanceController._patrolPos, vector3_1, 5.5f * Time.deltaTime);
    Transform weaponRoot1 = pwa.HandsContainer.WeaponRoot;
    weaponRoot1.localPosition = Vector3.op_Addition(weaponRoot1.localPosition, StanceController._patrolPos);
    Vector3 vector3_2 = StanceController.CurrentStance != EStance.PatrolStance ? Vector3.zero : (WeaponStats.IsStocklessPistol || WeaponStats.IsMachinePistol ? StanceController._pistolPatrolRot : StanceController._riflePatrolRot);
    StanceController._patrolRot = Vector3.Lerp(StanceController._patrolRot, vector3_2, 5.5f * Time.deltaTime);
    Quaternion identity = Quaternion.identity;
    identity.x = StanceController._patrolRot.x;
    identity.y = StanceController._patrolRot.y;
    identity.z = StanceController._patrolRot.z;
    Transform weaponRoot2 = pwa.HandsContainer.WeaponRoot;
    weaponRoot2.localRotation = Quaternion.op_Multiply(weaponRoot2.localRotation, identity);
    if ((double) Vector3.Distance(StanceController._patrolPos, Vector3.zero) <= 0.05000000074505806)
      StanceController.FinishedUnPatrolStancing = true;
    else
      StanceController.FinishedUnPatrolStancing = false;
  }

  private static void SetRotationWrapped(ref float yaw, ref float pitch)
  {
    if ((double) yaw < 0.0)
      yaw += 360f;
    if ((double) pitch < 0.0)
      pitch += 360f;
    pitch %= 360f;
    yaw %= 360f;
    if ((double) yaw > 180.0)
      yaw -= 360f;
    if ((double) pitch <= 180.0)
      return;
    pitch -= 360f;
  }

  private static void SetRotationClamped(ref float yaw, ref float pitch, float maxAngle)
  {
    Vector2 vector2 = Vector2.ClampMagnitude(new Vector2(yaw, pitch), maxAngle);
    yaw = vector2.x;
    pitch = vector2.y;
  }

  private static void UpdateAimSmoothed(ProceduralWeaponAnimation pwa, float deltaTime)
  {
    StanceController._mountAimSmoothed = Mathf.Lerp(StanceController._mountAimSmoothed, pwa.IsAiming ? 1f : 0.0f, deltaTime * 6f);
  }

  private static void UpdateMountRotation(Vector2 currentYawPitch, float clamp)
  {
    Quaternion quaternion1 = Quaternion.Euler(StanceController._lastMountYawPitch.x, StanceController._lastMountYawPitch.y, 0.0f);
    Quaternion to = Quaternion.Euler(currentYawPitch.x, currentYawPitch.y, 0.0f);
    StanceController._lastMountYawPitch = currentYawPitch;
    Quaternion quaternion2 = StanceController._makeQuaternionDelta(Quaternion.SlerpUnclamped(to, quaternion1, 0.115f), to);
    Vector3 eulerAngles = ((Quaternion) ref quaternion2).eulerAngles;
    StanceController._cumulativeMountYaw += eulerAngles.x;
    StanceController._cumulativeMountPitch += eulerAngles.y;
    StanceController.SetRotationWrapped(ref StanceController._cumulativeMountYaw, ref StanceController._cumulativeMountPitch);
    StanceController.SetRotationClamped(ref StanceController._cumulativeMountYaw, ref StanceController._cumulativeMountPitch, clamp);
  }

  private static void ApplyPivotPoint(
    ProceduralWeaponAnimation pwa,
    Player player,
    float pivotPoint,
    float aimPivot)
  {
    float num = (float) (1.0 - (1.0 - (double) aimPivot) * (double) StanceController._mountAimSmoothed);
    Transform weaponRootAnim = pwa.HandsContainer.WeaponRootAnim;
    if (Object.op_Equality((Object) weaponRootAnim, (Object) null))
      return;
    GClass819.LocalRotateAround(weaponRootAnim, Vector3.op_Multiply(Vector3.up, -pivotPoint), new Vector3(StanceController._cumulativeMountPitch * num, 0.0f, StanceController._cumulativeMountYaw * num));
    GClass819.LocalRotateAround(weaponRootAnim, Vector3.op_Multiply(Vector3.up, pivotPoint), Vector3.zero);
  }

  public static void MountingPivotUpdate(
    Player player,
    ProceduralWeaponAnimation pwa,
    float clamp,
    float deltaTime,
    float pivotPoint = 0.75f,
    float aimPivot = 0.25f)
  {
    Vector2 currentYawPitch;
    // ISSUE: explicit constructor call
    ((Vector2) ref currentYawPitch).\u002Ector(player.MovementContext.Yaw, player.MovementContext.Pitch);
    StanceController.UpdateMountRotation(currentYawPitch, clamp);
    StanceController.UpdateAimSmoothed(pwa, deltaTime);
    StanceController.ApplyPivotPoint(pwa, player, pivotPoint, aimPivot);
  }

  public static float GetDeltaTime()
  {
    float deltaTime = (float) StanceController.aimWatch.Elapsed.Milliseconds / 1000f;
    StanceController.aimWatch.Reset();
    StanceController.aimWatch.Start();
    return deltaTime;
  }

  public static void ToggleMounting(
    Player player,
    ProceduralWeaponAnimation pwa,
    Player.FirearmController fc)
  {
    if (!StanceController.IsMounting || !PlayerState.IsMoving)
      return;
    StanceController.IsMounting = false;
  }

  static StanceController()
  {
    // ISSUE: unable to decompile the method.
  }
}
