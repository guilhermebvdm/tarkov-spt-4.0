// Decompiled with JetBrains decompiler
// Type: TarkovIRL.NewDeadzoneController
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using RealismMod;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal class NewDeadzoneController
{
  private static float _rotDeltaHistory;
  private static float _rotDeltaSmoothed;
  private static float _rotDeltaSmoothedInDeltaTime;

  public static void Update(float fdt)
  {
    float num1 = PlayerMotionController.HorizontalRotationDelta * 100f;
    NewDeadzoneController._rotDeltaHistory += num1;
    NewDeadzoneController._rotDeltaHistory -= NewDeadzoneController._rotDeltaSmoothed;
    NewDeadzoneController._rotDeltaSmoothed = (float) ((double) NewDeadzoneController._rotDeltaHistory * (double) fdt * 9.0);
    float num2 = PrimeMover.WeaponDeadzoneMulti.Value * WeaponController.GetWeaponMulti(false);
    if (WeaponController.HasCheekWeld() && PlayerMotionController.IsAiming)
    {
      num2 *= PrimeMover.DeadzoneInADS.Value;
    }
    else
    {
      switch (StanceController.CurrentStance)
      {
        case EStance.None:
          num2 *= PrimeMover.DeadzoneInVanilla.Value;
          break;
        case EStance.LowReady:
          num2 *= PrimeMover.DeadzoneInLowReady.Value;
          break;
        case EStance.HighReady:
          num2 *= PrimeMover.DeadzoneInHighReady.Value;
          break;
        case EStance.ShortStock:
          num2 *= PrimeMover.DeadzoneInShortStock.Value;
          break;
        case EStance.ActiveAiming:
          num2 *= PrimeMover.DeadzoneInActiveAim.Value;
          break;
      }
    }
    float num3 = PrimeMover.DeadzoneWeightForEfficiency.Value ? EfficiencyController.EfficiencyModifierInverse : 1f;
    NewDeadzoneController._rotDeltaSmoothedInDeltaTime = Mathf.Lerp(NewDeadzoneController._rotDeltaSmoothedInDeltaTime, NewDeadzoneController._rotDeltaSmoothed * num2, fdt * PrimeMover.DeadzoneHeadFollowSpeedMulti.Value * num3);
  }

  public static Vector3 GetHeadRotWithDeadzone(Vector3 headRotInitial)
  {
    Vector3 headRotWithDeadzone = headRotInitial;
    headRotWithDeadzone.y += NewDeadzoneController._rotDeltaSmoothedInDeltaTime;
    return headRotWithDeadzone;
  }
}
