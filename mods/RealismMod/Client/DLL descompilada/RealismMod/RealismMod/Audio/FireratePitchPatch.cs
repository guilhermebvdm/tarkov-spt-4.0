// Decompiled with JetBrains decompiler
// Type: RealismMod.Audio.FireratePitchPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod.Audio;

public class FireratePitchPatch : ModulePatch
{
  private static FieldInfo _playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    FireratePitchPatch._playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("method_61", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static bool PatchPrefix(Player.FirearmController __instance, ref float __result)
  {
    if (!((Player) FireratePitchPatch._playerField.GetValue((object) __instance)).IsYourPlayer)
      return true;
    if (__instance.Weapon == null)
      __result = 1f;
    float num = __instance.Weapon.FireMode.FireMode != null && __instance.Weapon.FireMode.FireMode != 3 ? 1f + Random.Range(-0.03f, 0.03f) : Mathf.Clamp(Mathf.Pow(WeaponStats.AutoFireRateDelta, 3f) * (1f + Mathf.Pow(__instance.Weapon.MalfState.LastShotOverheat / 100f, 2f)) + Random.Range(-0.015f, 0.015f), 0.75f, 1.25f);
    __result = num;
    return false;
  }
}
