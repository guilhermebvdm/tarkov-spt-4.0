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
    float num2 = PrimeMover.Instance.WeightAttenuationCurve.Evaluate(PrimeMover.DeadzoneCustomWeight.Value) * (1f - PrimeMover.Instance.ErgoAttenuationCurve.Evaluate(PrimeMover.DeadzoneCustomErgo.Value));
    if ((double) num2 < (double) PrimeMover.MinimumWeaponSway.Value)
      num2 = PrimeMover.MinimumWeaponSway.Value;
    float num3 = PrimeMover.WeaponDeadzoneMulti.Value * num2;
    if (PlayerMotionController.IsAiming)
    {
      num3 *= PrimeMover.DeadzoneInADS.Value;
    }
    else
    {
      switch (StanceController.CurrentStance)
      {
        case EStance.None:
          num3 *= PrimeMover.DeadzoneInVanilla.Value;
          break;
        case EStance.HighReady:
          num3 *= PrimeMover.DeadzoneInHighReady.Value;
          break;
        case EStance.LowReady:
          num3 *= PrimeMover.DeadzoneInLowReady.Value;
          break;
        case EStance.ShortStock:
          num3 *= PrimeMover.DeadzoneInShortStock.Value;
          break;
        case EStance.ActiveAiming:
          num3 *= PrimeMover.DeadzoneInActiveAim.Value;
          break;
      }
    }
    float num4 = PrimeMover.DeadzoneWeightForEfficiency.Value ? EfficiencyController.EfficiencyModifierInverse : 1f;
    NewDeadzoneController._rotDeltaSmoothedInDeltaTime = Mathf.Lerp(NewDeadzoneController._rotDeltaSmoothedInDeltaTime, NewDeadzoneController._rotDeltaSmoothed * num3, fdt * PrimeMover.DeadzoneHeadFollowSpeedMulti.Value * num4);
  }

  public static Vector3 GetHeadRotWithDeadzone(Vector3 headRotInitial)
  {
    Vector3 headRotWithDeadzone = headRotInitial;
    headRotWithDeadzone.y += NewDeadzoneController._rotDeltaSmoothedInDeltaTime;
    return headRotWithDeadzone;
  }
}

