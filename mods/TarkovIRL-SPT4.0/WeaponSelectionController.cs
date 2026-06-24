// Decompiled with JetBrains decompiler
// Type: TarkovIRL.WeaponSelectionController
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using EFT;
using EFT.InputSystem;
using EFT.InventoryLogic;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal class WeaponSelectionController
{
  private static WeaponSelectionController.EWeaponSelection _lastSelectedWeapon;
  private static Vector3 _posLerp = Vector3.zero;
  private static Vector3 _orderEndPos = Vector3.zero;
  private static Vector3 _presentStartPos = Vector3.zero;
  private static Vector3 _rotLerp = Vector3.zero;
  private static Vector3 _orderEndRot = Vector3.zero;
  private static Vector3 _presentStartRot = Vector3.zero;
  private static Vector3 _posLerpHistory = Vector3.zero;
  private static Vector3 _rotLerpHistory = Vector3.zero;
  private static Vector3 _posLerpSmoothed = Vector3.zero;
  private static Vector3 _rotLerpSmoothed = Vector3.zero;
  private static readonly Vector3 Pistol_Start_Pos = new Vector3(0.05f, 0.1f, -0.2f);
  private static readonly Vector3 Pistol_Start_Rot = new Vector3(0.1f, -0.2f, 0.05f);
  private static readonly Vector3 Pistol_End_Pos = Vector3.zero;
  private static readonly Vector3 Pistol_End_Rot = Vector3.zero;
  private static readonly Vector3 Shoulder_Start_Pos = new Vector3(0.27f, -0.5f, -0.12f);
  private static readonly Vector3 Shoulder_Start_Rot = new Vector3(-0.4f, -0.56f, 0.0f);
  private static readonly Vector3 Shoulder_End_Pos = new Vector3(0.25f, -0.5f, -0.19f);
  private static readonly Vector3 Shoulder_End_Rot = new Vector3(-0.4f, -0.56f, 0.0f);
  private static readonly Vector3 Sling_Start_Pos = new Vector3(0.06f, -0.16f, -0.1f);
  private static readonly Vector3 Sling_Start_Rot = new Vector3(0.0f, -0.75f, -0.24f);
  private static readonly Vector3 Sling_End_Pos = new Vector3(0.37f, -0.16f, -0.46f);
  private static readonly Vector3 Sling_End_Rot = new Vector3(0.0f, 1f, -0.24f);
  private static readonly float TransformSmoothingDTMulti = 12f;
  private static Player _player = (Player) null;
  private static AnimationCurve _animSpeedCurvePhase1 = new AnimationCurve();
  private static AnimationCurve _animSpeedCurvePhase2 = new AnimationCurve();
  private static AnimationCurve _Flatcurve = new AnimationCurve(new Keyframe[2]
  {
    new Keyframe(0.0f, 1f),
    new Keyframe(1f, 1f)
  });
  private static bool _transitionFirstFrame = false;
  private static bool _transitionWindowOpen = false;
  private static AnimStateController.EWeaponState _previousState;

  public static void UpdateAnimationPump(float dt)
  {
    if (!PrimeMover.IsWeaponTrans.Value)
      return;
    WeaponSelectionController._posLerpHistory.x += WeaponSelectionController._posLerp.x;
    WeaponSelectionController._posLerpHistory.y += WeaponSelectionController._posLerp.y;
    WeaponSelectionController._posLerpHistory.z += WeaponSelectionController._posLerp.z;
    WeaponSelectionController._posLerpHistory.x -= WeaponSelectionController._posLerpSmoothed.x;
    WeaponSelectionController._posLerpHistory.y -= WeaponSelectionController._posLerpSmoothed.y;
    WeaponSelectionController._posLerpHistory.z -= WeaponSelectionController._posLerpSmoothed.z;
    WeaponSelectionController._rotLerpHistory.x += WeaponSelectionController._rotLerp.x;
    WeaponSelectionController._rotLerpHistory.y += WeaponSelectionController._rotLerp.y;
    WeaponSelectionController._rotLerpHistory.z += WeaponSelectionController._rotLerp.z;
    WeaponSelectionController._rotLerpHistory.x -= WeaponSelectionController._rotLerpSmoothed.x;
    WeaponSelectionController._rotLerpHistory.y -= WeaponSelectionController._rotLerpSmoothed.y;
    WeaponSelectionController._rotLerpHistory.z -= WeaponSelectionController._rotLerpSmoothed.z;
    WeaponSelectionController._posLerpSmoothed.x = WeaponSelectionController._posLerpHistory.x * dt * WeaponSelectionController.TransformSmoothingDTMulti;
    WeaponSelectionController._posLerpSmoothed.y = WeaponSelectionController._posLerpHistory.y * dt * WeaponSelectionController.TransformSmoothingDTMulti;
    WeaponSelectionController._posLerpSmoothed.z = WeaponSelectionController._posLerpHistory.z * dt * WeaponSelectionController.TransformSmoothingDTMulti;
    WeaponSelectionController._rotLerpSmoothed.x = WeaponSelectionController._rotLerpHistory.x * dt * WeaponSelectionController.TransformSmoothingDTMulti;
    WeaponSelectionController._rotLerpSmoothed.y = WeaponSelectionController._rotLerpHistory.y * dt * WeaponSelectionController.TransformSmoothingDTMulti;
    WeaponSelectionController._rotLerpSmoothed.z = WeaponSelectionController._rotLerpHistory.z * dt * WeaponSelectionController.TransformSmoothingDTMulti;
    if (WeaponSelectionController._transitionFirstFrame)
    {
      WeaponSelectionController._transitionFirstFrame = false;
      WeaponSelectionController._transitionWindowOpen = true;
    }
    if (!WeaponSelectionController._transitionWindowOpen)
      return;
    AnimStateController.EWeaponState weaponState = AnimStateController.WeaponState;
    float normalizedTime = WeaponSelectionController._player.HandsAnimator.Animator.GetCurrentAnimatorStateInfo(1).normalizedTime;
    float num1 = Mathf.Clamp(EfficiencyController.EfficiencyModifierInverse, 0.5f, 1.5f);
    float num2 = PlayerMotionController.IsProne ? 0.5f : 1f;
    float num3 = WeaponController.IsStockFolded ? 1.5f : 1f;
    float num4 = num1 * num2 * WeaponController.GetWeaponMulti(true) * PrimeMover.TransitionSpeedMulti.Value * num3;
    switch (weaponState)
    {
      case AnimStateController.EWeaponState.ORDER_ARM:
        float speed1 = WeaponSelectionController._animSpeedCurvePhase1.Evaluate(normalizedTime) * num4;
        WeaponSelectionController.SetAnimSpeed(WeaponSelectionController._player, speed1);
        WeaponSelectionController._posLerp = Vector3.Lerp(Vector3.zero, WeaponSelectionController._orderEndPos, normalizedTime);
        WeaponSelectionController._rotLerp = Vector3.Lerp(Vector3.zero, WeaponSelectionController._orderEndRot, normalizedTime);
        break;
      case AnimStateController.EWeaponState.PRESENT_ARM:
        float speed2 = WeaponSelectionController._animSpeedCurvePhase2.Evaluate(normalizedTime) * num4;
        WeaponSelectionController.SetAnimSpeed(WeaponSelectionController._player, speed2);
        WeaponSelectionController._posLerp = Vector3.Lerp(WeaponSelectionController._presentStartPos, Vector3.zero, normalizedTime);
        WeaponSelectionController._rotLerp = Vector3.Lerp(WeaponSelectionController._presentStartRot, Vector3.zero, normalizedTime);
        break;
    }
    if (WeaponSelectionController._previousState == AnimStateController.EWeaponState.PRESENT_ARM && weaponState == AnimStateController.EWeaponState.IDLE)
    {
      WeaponSelectionController.SetAnimSpeed(WeaponSelectionController._player, 1f);
      WeaponSelectionController._transitionWindowOpen = false;
      WeaponSelectionController._posLerp = Vector3.zero;
      WeaponSelectionController._rotLerp = Vector3.zero;
    }
    WeaponSelectionController._previousState = weaponState;
  }

  public static void GetWeaponSelectionTransforms(out Vector3 pos, out Quaternion rot)
  {
    pos = WeaponSelectionController._posLerpSmoothed;
    rot = TIRLUtils.GetQuatFromV3(WeaponSelectionController._rotLerpSmoothed);
  }

  public static void Process(ECommand command, Player player)
  {
    if (Object.op_Equality((Object) player, (Object) null))
      return;
    WeaponSelectionController._player = player;
    if (AnimStateController.WeaponState == AnimStateController.EWeaponState.UNKNOWN_STATE)
      return;
    Item weaponOrKnifeItem = player.LastEquippedWeaponOrKnifeItem;
    Item containedItem1 = player.Inventory.Equipment.GetSlot((EquipmentSlot) 0).ContainedItem;
    Item containedItem2 = player.Inventory.Equipment.GetSlot((EquipmentSlot) 1).ContainedItem;
    Item containedItem3 = player.Inventory.Equipment.GetSlot((EquipmentSlot) 2).ContainedItem;
    if (weaponOrKnifeItem == null)
    {
      player.TrySetLastEquippedWeapon(true);
      if (weaponOrKnifeItem == null)
        return;
    }
    if (containedItem1 != null && weaponOrKnifeItem.Equals((object) containedItem1))
    {
      if (command == 40)
        return;
      WeaponSelectionController._lastSelectedWeapon = WeaponSelectionController.EWeaponSelection.SLING;
    }
    if (containedItem2 != null && weaponOrKnifeItem.Equals((object) containedItem2))
    {
      if (command == 41)
        return;
      WeaponSelectionController._lastSelectedWeapon = WeaponSelectionController.EWeaponSelection.SHOULDER;
    }
    if (containedItem3 != null)
    {
      if (weaponOrKnifeItem.Equals((object) containedItem3))
      {
        if (command == 42)
          return;
        WeaponSelectionController._lastSelectedWeapon = WeaponSelectionController.EWeaponSelection.PISTOL;
      }
      if (weaponOrKnifeItem.Equals((object) containedItem3))
      {
        if (command == 43 && containedItem1 == null)
          return;
        WeaponSelectionController._lastSelectedWeapon = WeaponSelectionController.EWeaponSelection.PISTOL;
      }
    }
    if (command == 40 && containedItem1 == null || command == 41 && containedItem2 == null || command == 42 && containedItem3 == null)
      return;
    WeaponSelectionController._transitionFirstFrame = true;
    if (command == 40 && WeaponSelectionController._lastSelectedWeapon == WeaponSelectionController.EWeaponSelection.SHOULDER)
      WeaponSelectionController.ProcessShoulderToSling();
    if (command == 41 && WeaponSelectionController._lastSelectedWeapon == WeaponSelectionController.EWeaponSelection.SLING)
      WeaponSelectionController.ProcessSlingToShoulder();
    if (command == 42 && WeaponSelectionController._lastSelectedWeapon == WeaponSelectionController.EWeaponSelection.SLING)
      WeaponSelectionController.ProcessSlingToPistol();
    if (command == 43 && WeaponSelectionController._lastSelectedWeapon == WeaponSelectionController.EWeaponSelection.SLING)
      WeaponSelectionController.ProcessQuickSlingToPistol();
    if (command == 42 && WeaponSelectionController._lastSelectedWeapon == WeaponSelectionController.EWeaponSelection.SHOULDER)
      WeaponSelectionController.ProcessShoulderToPistol();
    if (command == 43 && WeaponSelectionController._lastSelectedWeapon == WeaponSelectionController.EWeaponSelection.SHOULDER)
      WeaponSelectionController.ProcessShoulderToPistol();
    if (command == 41 && WeaponSelectionController._lastSelectedWeapon == WeaponSelectionController.EWeaponSelection.PISTOL)
      WeaponSelectionController.ProcessPistolToShoulder();
    if (command == 40 && WeaponSelectionController._lastSelectedWeapon == WeaponSelectionController.EWeaponSelection.PISTOL)
      WeaponSelectionController.ProcessPistolToSling();
    if (!(command == 133 & command == (int) sbyte.MaxValue & command == 128 /*0x80*/ & command == 129 & command == 130 & command == 131 & command == 132 & command == 52 & command == 46 & command == 47 & command == 48 /*0x30*/ & command == 49 & command == 50 & command == 51) || WeaponSelectionController._lastSelectedWeapon != WeaponSelectionController.EWeaponSelection.SHOULDER)
      return;
    WeaponSelectionController.ProcessShoulderToSlot();
  }

  private static void ProcessShoulderToSling()
  {
    WeaponSelectionController._animSpeedCurvePhase1 = PrimeMover.Instance.OrderShoulderCurve;
    WeaponSelectionController._animSpeedCurvePhase2 = WeaponSelectionController._Flatcurve;
    WeaponSelectionController._orderEndPos = WeaponSelectionController.Shoulder_End_Pos;
    WeaponSelectionController._orderEndRot = WeaponSelectionController.Shoulder_End_Rot;
    WeaponSelectionController._presentStartPos = WeaponSelectionController.Sling_Start_Pos;
    WeaponSelectionController._presentStartRot = WeaponSelectionController.Sling_Start_Rot;
  }

  private static void ProcessSlingToShoulder()
  {
    WeaponSelectionController._animSpeedCurvePhase1 = WeaponSelectionController._Flatcurve;
    WeaponSelectionController._animSpeedCurvePhase2 = PrimeMover.Instance.PresentShoulderCurve;
    WeaponSelectionController._orderEndPos = WeaponSelectionController.Sling_End_Pos;
    WeaponSelectionController._orderEndRot = WeaponSelectionController.Sling_End_Rot;
    WeaponSelectionController._presentStartPos = WeaponSelectionController.Shoulder_Start_Pos;
    WeaponSelectionController._presentStartRot = WeaponSelectionController.Shoulder_Start_Rot;
  }

  private static void ProcessSlingToPistol()
  {
    WeaponSelectionController._animSpeedCurvePhase1 = new AnimationCurve(new Keyframe[2]
    {
      new Keyframe(0.0f, 1f),
      new Keyframe(1f, 1f)
    });
    WeaponSelectionController._animSpeedCurvePhase2 = new AnimationCurve(new Keyframe[2]
    {
      new Keyframe(0.0f, 0.3f),
      new Keyframe(1f, 0.3f)
    });
    WeaponSelectionController._orderEndPos = WeaponSelectionController.Sling_End_Pos;
    WeaponSelectionController._orderEndRot = WeaponSelectionController.Sling_End_Rot;
    WeaponSelectionController._presentStartPos = WeaponSelectionController.Pistol_Start_Pos;
    WeaponSelectionController._presentStartRot = WeaponSelectionController.Pistol_Start_Rot;
  }

  private static void ProcessQuickSlingToPistol()
  {
    WeaponSelectionController._animSpeedCurvePhase1 = new AnimationCurve(new Keyframe[2]
    {
      new Keyframe(0.0f, 1.25f),
      new Keyframe(1f, 0.75f)
    });
    WeaponSelectionController._animSpeedCurvePhase2 = new AnimationCurve(new Keyframe[2]
    {
      new Keyframe(0.0f, 1.2f),
      new Keyframe(1f, 2f)
    });
    WeaponSelectionController._orderEndPos = WeaponSelectionController.Sling_End_Pos;
    WeaponSelectionController._orderEndRot = WeaponSelectionController.Sling_End_Rot;
    WeaponSelectionController._presentStartPos = WeaponSelectionController.Pistol_Start_Pos;
    WeaponSelectionController._presentStartRot = WeaponSelectionController.Pistol_Start_Rot;
  }

  private static void ProcessShoulderToPistol()
  {
    WeaponSelectionController._animSpeedCurvePhase1 = PrimeMover.Instance.OrderShoulderCurve;
    WeaponSelectionController._animSpeedCurvePhase2 = new AnimationCurve(new Keyframe[2]
    {
      new Keyframe(0.0f, 0.3f),
      new Keyframe(1f, 0.3f)
    });
    WeaponSelectionController._orderEndPos = WeaponSelectionController.Shoulder_End_Pos;
    WeaponSelectionController._orderEndRot = WeaponSelectionController.Shoulder_End_Rot;
    WeaponSelectionController._presentStartPos = WeaponSelectionController.Pistol_Start_Pos;
    WeaponSelectionController._presentStartRot = WeaponSelectionController.Pistol_Start_Rot;
  }

  private static void ProcessPistolToShoulder()
  {
    WeaponSelectionController._animSpeedCurvePhase1 = new AnimationCurve(new Keyframe[2]
    {
      new Keyframe(0.0f, 0.25f),
      new Keyframe(1f, 0.25f)
    });
    WeaponSelectionController._animSpeedCurvePhase2 = PrimeMover.Instance.PresentShoulderCurve;
    WeaponSelectionController._orderEndPos = WeaponSelectionController.Pistol_End_Pos;
    WeaponSelectionController._orderEndRot = WeaponSelectionController.Pistol_End_Pos;
    WeaponSelectionController._presentStartPos = WeaponSelectionController.Shoulder_Start_Pos;
    WeaponSelectionController._presentStartRot = WeaponSelectionController.Shoulder_Start_Rot;
  }

  private static void ProcessPistolToSling()
  {
    WeaponSelectionController._animSpeedCurvePhase1 = new AnimationCurve(new Keyframe[2]
    {
      new Keyframe(0.0f, 0.35f),
      new Keyframe(1f, 0.25f)
    });
    WeaponSelectionController._animSpeedCurvePhase2 = new AnimationCurve(new Keyframe[2]
    {
      new Keyframe(0.0f, 1.75f),
      new Keyframe(1f, 1.75f)
    });
    WeaponSelectionController._orderEndPos = WeaponSelectionController.Pistol_End_Pos;
    WeaponSelectionController._orderEndRot = WeaponSelectionController.Pistol_End_Pos;
    WeaponSelectionController._presentStartPos = WeaponSelectionController.Sling_Start_Pos;
    WeaponSelectionController._presentStartRot = WeaponSelectionController.Sling_Start_Rot;
  }

  private static void ProcessShoulderToSlot()
  {
    WeaponSelectionController._animSpeedCurvePhase1 = PrimeMover.Instance.OrderShoulderCurve;
    WeaponSelectionController._animSpeedCurvePhase2 = WeaponSelectionController._Flatcurve;
    WeaponSelectionController._orderEndPos = WeaponSelectionController.Shoulder_End_Pos;
    WeaponSelectionController._orderEndRot = WeaponSelectionController.Shoulder_End_Rot;
    WeaponSelectionController._presentStartPos = Vector3.zero;
    WeaponSelectionController._presentStartRot = Vector3.zero;
  }

  private static void SetAnimSpeed(Player player, float speed)
  {
    if (Object.op_Equality((Object) player, (Object) null))
      return;
    player.HandsAnimator.SetAnimationSpeed(speed);
  }

  public enum EWeaponSelection
  {
    SLING,
    SHOULDER,
    PISTOL,
    OTHER,
  }
}
