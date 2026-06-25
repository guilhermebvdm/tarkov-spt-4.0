// Decompiled with JetBrains decompiler
// Type: TarkovIRL.DeadzoneController
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using EFT;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal static class DeadzoneController
{
  private static readonly float _LerpRate = 10f;
  private static readonly float _RotDeltaThresh = 0.0002f;
  private static float _deadZoneLerp = 0.0f;
  private static bool _updateDZ = true;
  public static bool DeadzoneUpdatedThisFrame = false;

  private static float ProcessHeadDelta(float rawHeadDelta)
  {
    return (float) ((double) rawHeadDelta / (double) WeaponController.CurrentWeaponErgoNorm / 10.0 * (double) WeaponController.CurrentWeaponWeight * 0.10000000149011612);
  }

  public static Vector3 GetHeadRotationWithDeadzone(
    Player player,
    float deadzoneSetting,
    Vector3 headRotInitial)
  {
    return Vector3.zero;
  }
}
