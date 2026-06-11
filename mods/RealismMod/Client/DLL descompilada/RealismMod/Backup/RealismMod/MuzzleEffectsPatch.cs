// Decompiled with JetBrains decompiler
// Type: RealismMod.MuzzleEffectsPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class MuzzleEffectsPatch : ModulePatch
{
  private static FieldInfo _muzzleManagerField;
  private static FieldInfo _muzzleSparksField;
  private static FieldInfo _muzzleJetsField;
  private static FieldInfo _floatField;
  private static FieldInfo _muzzleFumeField;
  private static FieldInfo _muzzleSmokeField;

  protected virtual MethodBase GetTargetMethod()
  {
    MuzzleEffectsPatch._muzzleManagerField = AccessTools.Field(typeof (FirearmsEffects), "_muzzleManager");
    MuzzleEffectsPatch._muzzleSparksField = AccessTools.Field(typeof (MuzzleManager), "muzzleSparks_0");
    MuzzleEffectsPatch._muzzleFumeField = AccessTools.Field(typeof (MuzzleManager), "muzzleFume_0");
    MuzzleEffectsPatch._muzzleSmokeField = AccessTools.Field(typeof (MuzzleManager), "muzzleSmoke_0");
    MuzzleEffectsPatch._muzzleJetsField = AccessTools.Field(typeof (MuzzleManager), "muzzleJet_0");
    MuzzleEffectsPatch._floatField = AccessTools.Field(typeof (MuzzleManager), "float_1");
    return (MethodBase) typeof (WeaponManagerClass).GetMethod("PlayShotEffects", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static void Prefix(WeaponManagerClass __instance)
  {
    if (Object.op_Inequality((Object) __instance.Player, (Object) null) && !__instance.Player.IsYourPlayer || !PluginConfig.EnableMuzzleEffects.Value)
      return;
    MuzzleManager muzzleManager = (MuzzleManager) MuzzleEffectsPatch._muzzleManagerField.GetValue((object) __instance.FirearmsEffects);
    if (Object.op_Equality((Object) muzzleManager, (Object) null))
      return;
    Player.FirearmController handsController = __instance.Player.HandsController as Player.FirearmController;
    float num1 = Mathf.Pow((float) (2.0 - (double) handsController.Weapon.Repairable.Durability / 100.0), 0.5f);
    float num2 = (float) (1.0 + (double) handsController.Weapon.MalfState.LastShotOverheat / 100.0);
    int num3 = (int) ((double) (Mathf.Pow(1f - WeaponStats.VelocityDelta, 3.5f) * (float) StatCalc.CaliberSparks(WeaponStats.Caliber)) * (1.0 + (double) WeaponStats.TotalMuzzleFlash / 100.0) * (double) num2 * (double) num1);
    float num4 = (float) StatCalc.CaliberMuzzleFlash(WeaponStats.Caliber) * 0.8f;
    float num5 = ((double) WeaponStats.VelocityDelta < 0.0 ? Mathf.Pow(1f - WeaponStats.VelocityDelta, 2.5f) * num4 : (1f - WeaponStats.VelocityDelta) * num4) * (float) (1.0 + (double) WeaponStats.TotalMuzzleFlash / 100.0) * (PlayerState.EnviroType == 1 ? 1.19f : 1f);
    Vector2 vector2;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2).\u002Ector(num5 / 2f, num5);
    float num6 = Mathf.Pow(1f - WeaponStats.VelocityDelta, 3f) * StatCalc.CaliberFlame(WeaponStats.Caliber);
    float num7 = Mathf.Clamp((float) (1.0 + (double) WeaponStats.TotalMuzzleFlash / 50.0), 0.1f, 3f);
    float num8 = PlayerState.EnviroType == 1 ? 1.142f : 1f;
    float num9 = num6 * num7 * num2 * num1 * num8;
    float num10 = (float) ((double) Mathf.Pow(1f - WeaponStats.VelocityDelta, 3f) * (double) StatCalc.CaliberSmoke(WeaponStats.Caliber) * 0.64999997615814209) * ((double) WeaponStats.TotalMuzzleFlash > 0.0 ? (float) (1.0 + (double) WeaponStats.TotalMuzzleFlash / 75.0) : (float) (1.0 + (double) WeaponStats.TotalMuzzleFlash / 200.0)) * (float) (1.0 + (double) WeaponStats.TotalGas / 100.0) * (!WeaponStats.IsDirectImpingement || !WeaponStats.HasSuppressor ? 1f : 1.35f) * (PlayerState.EnviroType == 1 ? 1.24f : 0.85f) * num1 * num2;
    float num11 = 1f + num10;
    float num12 = Mathf.Max(1f - num10, 0.1f);
    MuzzleSparks[] muzzleSparksArray = (MuzzleSparks[]) MuzzleEffectsPatch._muzzleSparksField.GetValue((object) muzzleManager);
    if (muzzleSparksArray != null)
    {
      int length = muzzleSparksArray.Length;
      for (int index = 0; index < length; ++index)
      {
        MuzzleSparks muzzleSparks = muzzleSparksArray[index];
        muzzleSparks.CountMin = Mathf.Max(num3 - 5, -4);
        muzzleSparks.CountRange = Mathf.Max(4 + num3, 0);
      }
    }
    MuzzleJet[] muzzleJetArray = (MuzzleJet[]) MuzzleEffectsPatch._muzzleJetsField.GetValue((object) muzzleManager);
    if (muzzleJetArray != null)
    {
      int length1 = muzzleJetArray.Length;
      for (int index1 = 0; index1 < length1; ++index1)
      {
        MuzzleJet muzzleJet = muzzleJetArray[index1];
        muzzleJet.Chance = num9 / (float) length1;
        if (muzzleJet.Particles != null)
        {
          int length2 = muzzleJet.Particles.Length;
          for (int index2 = 0; index2 < length2; ++index2)
            muzzleJet.Particles[index2].Size = num9;
        }
      }
    }
    float num13 = (float) MuzzleEffectsPatch._floatField.GetValue((object) muzzleManager);
    if (Object.op_Inequality((Object) muzzleManager.Light, (Object) null) && (double) num13 > 0.0)
      muzzleManager.Light.Range = vector2;
    __instance.FirearmsEffects.UpdateMuzzle();
    MuzzleFume[] muzzleFumeArray = (MuzzleFume[]) MuzzleEffectsPatch._muzzleFumeField.GetValue((object) muzzleManager);
    if (muzzleFumeArray != null)
    {
      int length = muzzleFumeArray.Length;
      for (int index = 0; index < length; ++index)
      {
        MuzzleFume muzzleFume = muzzleFumeArray[index];
        muzzleFume.Size = Mathf.Max(num10, 0.3f);
        muzzleFume.CountMin = 1;
        muzzleFume.CountRange = 2;
      }
    }
    MuzzleSmoke[] muzzleSmokeArray = (MuzzleSmoke[]) MuzzleEffectsPatch._muzzleSmokeField.GetValue((object) muzzleManager);
    if (muzzleSmokeArray == null || muzzleSmokeArray.Length == 0)
      return;
    MuzzleSmoke muzzleSmoke = muzzleSmokeArray[0];
    muzzleSmoke.SmokeLength = WeaponStats.IsPistol ? 0.0f : 18f * num11;
    muzzleSmoke.MuzzleSpeedMultiplier = 0.39f;
    muzzleSmoke.SmokeVelocity = WeaponStats.IsPistol ? 100f : 0.06f * num12;
    muzzleSmoke.SmokeIncreasingByShot = WeaponStats.IsPistol ? 0.0f : 0.25f * num11;
  }
}
