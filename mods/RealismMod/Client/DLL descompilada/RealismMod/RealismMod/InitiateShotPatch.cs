// Decompiled with JetBrains decompiler
// Type: RealismMod.InitiateShotPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class InitiateShotPatch : ModulePatch
{
  private static FieldInfo _playerfield;
  private static FieldInfo _accField;
  private static FieldInfo _buckFeld;
  private static FieldInfo _prefabField;
  private static FieldInfo _skillField;
  private static FieldInfo _soundField;
  private static FieldInfo _recoilField;

  protected virtual MethodBase GetTargetMethod()
  {
    InitiateShotPatch._playerfield = AccessTools.Field(typeof (Player.FirearmController), "_player");
    InitiateShotPatch._accField = AccessTools.Field(typeof (Player.FirearmController), "float_3");
    InitiateShotPatch._buckFeld = AccessTools.Field(typeof (Player.FirearmController), "float_4");
    InitiateShotPatch._prefabField = AccessTools.Field(typeof (Player.FirearmController), "weaponPrefab_0");
    InitiateShotPatch._skillField = AccessTools.Field(typeof (Player.FirearmController), "gclass2017_0");
    InitiateShotPatch._soundField = AccessTools.Field(typeof (Player.FirearmController), "weaponSoundPlayer_0");
    InitiateShotPatch._recoilField = AccessTools.Field(typeof (Player.FirearmController), "float_5");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("method_58", BindingFlags.Instance | BindingFlags.Public);
  }

  private static Vector3 CalculateRecoilBloom(float overHeatFactor)
  {
    if (!Plugin.ServerConfig.recoil_attachment_overhaul || ShootController.ShotCount <= 0)
      return Vector3.zero;
    float num1 = ShootController.FactoredTotalDispersion * 0.0007f * Mathf.Pow((float) ShootController.ShotCount, 0.2f) * overHeatFactor;
    float num2 = Random.Range(-num1, num1);
    float num3 = Random.Range(0.0f, num1);
    return new Vector3(Mathf.Clamp(num2, -0.02f, 0.02f), Mathf.Clamp(num3, 0.0f, 0.02f), 0.0f);
  }

  [PatchPrefix]
  private static bool Prefix(
    Player.FirearmController __instance,
    Weapon weapon,
    AmmoItemClass ammo,
    int chamberIndex,
    bool multiShot = false)
  {
    Player player = (Player) InitiateShotPatch._playerfield.GetValue((object) __instance);
    if (Object.op_Equality((Object) player, (Object) null) || !player.IsYourPlayer)
      return true;
    float num1 = (float) InitiateShotPatch._accField.GetValue((object) __instance);
    float num2 = (float) InitiateShotPatch._buckFeld.GetValue((object) __instance);
    WeaponPrefab weaponPrefab = (WeaponPrefab) InitiateShotPatch._prefabField.GetValue((object) __instance);
    SkillManager.GClass2017 gclass2017 = (SkillManager.GClass2017) InitiateShotPatch._skillField.GetValue((object) __instance);
    WeaponSoundPlayer weaponSoundPlayer = (WeaponSoundPlayer) InitiateShotPatch._soundField.GetValue((object) __instance);
    Transform original = __instance.CurrentFireport.Original;
    Vector3 position = __instance.CurrentFireport.position;
    Vector3 vector3_1 = __instance.WeaponDirection;
    Vector3 vector3_2 = position;
    float ammoFactor = ammo.AmmoFactor;
    float overHeatFactor = 1f;
    BackendConfigSettingsClass instance = Singleton<BackendConfigSettingsClass>.Instance;
    __instance.AdjustShotVectors(ref vector3_2, ref vector3_1);
    ammo.buckshotDispersion = num2;
    __instance.CurrentChamberIndex = chamberIndex;
    weapon.OnShot(ammo.DurabilityBurnModificator, ammo.HeatFactor, player.Skills.WeaponDurabilityLosOnShotReduce.Value, instance.Overheat, GClass1662.PastTime);
    if ((double) weapon.MalfState.LastShotOverheat >= 1.0)
      overHeatFactor = Mathf.Pow(Mathf.Lerp(1f, instance.Overheat.MaxCOIIncreaseMult, (float) (((double) weapon.MalfState.LastShotOverheat - 1.0) / ((double) instance.Overheat.MaxOverheat - 1.0))), 2f);
    int num3;
    if (multiShot)
    {
      if ((num3 = chamberIndex > 0 ? 1 : 0) != 0)
      {
        float num4 = Random.Range(weaponPrefab.DupletAccuracyPenaltyX.x, weaponPrefab.DupletAccuracyPenaltyX.y);
        float num5 = Random.Range(weaponPrefab.DupletAccuracyPenaltyY.x, weaponPrefab.DupletAccuracyPenaltyY.y);
        Vector3 vector3_3;
        // ISSUE: explicit constructor call
        ((Vector3) ref vector3_3).\u002Ector(num4, num5);
        float num6 = vector3_3.y * -1f;
        Vector3 vector3_4 = Quaternion.op_Multiply(Quaternion.AngleAxis(vector3_3.x, original.forward), vector3_1);
        vector3_1 = Quaternion.op_Multiply(Quaternion.AngleAxis(num6, original.right), vector3_4);
      }
    }
    else
      num3 = 0;
    float num7 = weapon.CylinderHammerClosed ? weapon.DoubleActionAccuracyPenalty * (1f - gclass2017.DoubleActionRecoilReduce) * weapon.StockDoubleActionAccuracyPenaltyMult : 0.0f;
    float num8 = __instance.method_54(weapon);
    double num9 = ((Item) weapon).GetItemComponent<BuffComponent>().WeaponSpread;
    if (GClass834.ApproxEquals(num9, 0.0))
      num9 = 1.0;
    Vector3 vector3_5 = Vector3.op_Multiply(Random.insideUnitSphere, (float) (((double) num1 + (double) num7) * (double) ammoFactor * (double) overHeatFactor * (double) num8 * num9 * 0.012000000104308128));
    Vector3 vector3_6 = Vector3.op_Addition(Vector3.op_Addition(vector3_1, vector3_5), InitiateShotPatch.CalculateRecoilBloom(overHeatFactor));
    __instance.InitiateShot((IWeapon) weapon, ammo, vector3_2, ((Vector3) ref vector3_6).normalized, position, chamberIndex, weapon.MalfState.LastShotOverheat);
    float num10 = num3 != 0 ? 1.5f : 1f;
    InitiateShotPatch._recoilField.SetValue((object) __instance, (object) (float) ((double) num10 + (double) ammo.ammoRec / 100.0));
    __instance.method_59(weaponSoundPlayer, ammo, vector3_2, vector3_6, multiShot);
    if (ammo.AmmoTemplate.IsLightAndSoundShot)
    {
      __instance.method_62(position, vector3_1);
      __instance.LightAndSoundShot(position, vector3_1, ammo.AmmoTemplate);
    }
    return false;
  }
}
