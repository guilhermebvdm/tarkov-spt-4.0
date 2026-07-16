using EFT;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal class HandBreathController
{
  private static readonly float _BreathSpeedModifier = 0.55f;
  private static readonly float _BreathVerticalOffsetModifier = 0.0075f;
  private static float _breathUpdateTimer = 0.0f;

  public static Vector3 GetModifiedHandPosForBreath(Player player)
  {
    AnimationCurve breathCurve = PrimeMover.Instance.BreathCurve;
    float num1 = Mathf.Clamp((float) (1.0 - (double) player.Physical.Stamina.Current / 104.0), 0.15f, 1f);
    float num2 = RealismWrapper.IsAdrenaline ? 1.25f : 1f;
    float num3 = (float) (1.0 + (double) num1 * (double) num2);
    int num4 = (double) HandBreathController._breathUpdateTimer >= 0.550000011920929 || (double) HandBreathController._breathUpdateTimer <= 0.25999999046325684 ? 0 : (PlayerMotionController.IsHoldingBreath ? 1 : 0);
    bool flag = PlayerMotionController.IsAugmentedBreath = num4 != 0 & PlayerMotionController.IsAiming & (double) player.Physical.Stamina.NormalValue > 0.0099999997764825821;
    float num5 = RealismWrapper.IsAdrenaline ? 0.2f : 0.4f;
    float num6 = flag ? num5 : 1f;
    HandBreathController._breathUpdateTimer += player.DeltaTime * num3 * HandBreathController._BreathSpeedModifier * num6;
    if ((double) HandBreathController._breathUpdateTimer >= 1.0)
      --HandBreathController._breathUpdateTimer;
    float num7 = breathCurve.Evaluate(HandBreathController._breathUpdateTimer);
    float num8 = Mathf.Clamp(num1, 0.025f, 1f);
    return new Vector3(0.0f, num7 * HandBreathController._BreathVerticalOffsetModifier * num8 * PrimeMover.BreathingEffectMulti.Value, 0.0f);
  }
}

