using UnityEngine;

#nullable disable
namespace TarkovIRL;

public static class SwayController
{
  private static readonly float _BaseSwayLerpSpeed = 1f;
  public static bool IsSwayUpdatedThisFrame = false;
  private static float _addedSwayTarget = 0.0f;
  private static float _addedSwayLerp = 0.0f;

  public static void UpdateLerp(float dt)
  {
    SwayController._addedSwayLerp = Mathf.Lerp(SwayController._addedSwayLerp, SwayController._addedSwayTarget, dt * SwayController._BaseSwayLerpSpeed);
  }

  public static Vector3 GetNewSway(Vector3 newSwayFactors, bool isAiming)
  {
    if (!PrimeMover.IsWeaponSway.Value || AnimStateController.IsBlindfire)
      return Vector3.zero;
    float weaponMulti = WeaponController.GetWeaponMulti(false);
    newSwayFactors.x = 0.0f;
    newSwayFactors.y *= -0.2f * weaponMulti;
    newSwayFactors.z = 0.0f;
    return newSwayFactors;
  }
}

