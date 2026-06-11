// Decompiled with JetBrains decompiler
// Type: RealismMod.AddRecoilForcePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Bsg.GameSettings;
using Comfort.Common;
using EFT;
using EFT.Animations.NewRecoil;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class AddRecoilForcePatch : ModulePatch
{
  private static FieldInfo _shotIndexField;
  private static FieldInfo _fcField;
  private static FieldInfo _playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    AddRecoilForcePatch._fcField = AccessTools.Field(typeof (NewRecoilShotEffect), "_firearmController");
    AddRecoilForcePatch._playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    AddRecoilForcePatch._shotIndexField = AccessTools.Field(typeof (NewRecoilShotEffect), "_autoFireShotIndex");
    return (MethodBase) typeof (NewRecoilShotEffect).GetMethod("AddRecoilForce");
  }

  private static float ZeroShiftGunFactor(string weapType, string id)
  {
    string str = id;
    if (str == "6165ac306ef05c2ce828ef74" || str == "6183afd850224f204c1da514")
      return 1.35f;
    switch (weapType)
    {
      case "grenadeLauncher":
      case "machinegun":
      case "marksmanRifle":
      case "sniperRifle":
        return 1.15f;
      case "pistol":
        return 0.15f;
      case "shotgun":
        return 1f;
      case "smg":
        return 0.5f;
      default:
        return 1f;
    }
  }

  private static float PistolShotFactor(int shot)
  {
    int num = shot;
    if (num >= 8)
      return 0.01f;
    switch (num)
    {
      case 0:
        return 0.25f;
      case 1:
        return 0.01f;
      case 2:
        return 0.01f;
      case 3:
        return 0.01f;
      case 4:
        return 0.01f;
      case 5:
        return 0.05f;
      case 6:
        return 0.05f;
      case 7:
        return 0.05f;
      default:
        return 1f;
    }
  }

  private static float RifleShotModifier(int shotCount)
  {
    return (float) (0.949999988079071 + (double) shotCount * 0.05000000074505806);
  }

  private static void DoZeroSift(Player.FirearmController firearmController, float totalCamRecoil)
  {
    float num1 = AddRecoilForcePatch.ZeroShiftGunFactor(WeaponStats._WeapClass, MongoID.op_Implicit(((Item) firearmController.Weapon).TemplateId));
    float num2 = (float) (((double) ShootController.FactoredTotalVRecoil + (double) ShootController.FactoredTotalHRecoil) * (1.0 + (double) totalCamRecoil)) * num1;
    float num3 = (float) ((1.0 - (double) WeaponStats.ScopeAccuracyFactor) * 2.0 + (double) num2 * 0.10000000149011612 * 0.15000000596046448);
    int num4 = Random.Range(1, 21);
    if ((double) num3 <= (double) num4)
      return;
    float num5 = num3 * 0.015f;
    WeaponStats.ZeroRecoilOffset = new Vector2(Random.Range(-num5, num5), Random.Range(-num5, num5));
    if (WeaponStats.ScopeID != null && WeaponStats.ScopeID != "" && WeaponStats.ZeroOffsetDict.ContainsKey(WeaponStats.ScopeID))
      WeaponStats.ZeroOffsetDict[WeaponStats.ScopeID] = WeaponStats.ZeroRecoilOffset;
  }

  [PatchPrefix]
  public static bool Prefix(NewRecoilShotEffect __instance, float incomingForce)
  {
    Player.FirearmController firearmController = (Player.FirearmController) AddRecoilForcePatch._fcField.GetValue((object) __instance);
    Player player = (Player) AddRecoilForcePatch._playerField.GetValue((object) firearmController);
    if (!Object.op_Inequality((Object) player, (Object) null) || !player.IsYourPlayer)
      return true;
    bool flag = WeaponStats.IsPistol && WeaponStats.FireMode == null && firearmController.autoFireOn;
    float num1 = WeaponStats.IsStocklessPistol || !WeaponStats.HasShoulderContact && !WeaponStats.IsPistol ? 0.0f : PlayerState.TotalModifiedWeightMinusWeapon;
    float num2 = (float) (1.0 - (double) num1 / 650.0);
    float num3 = (float) (1.0 + (double) num1 / 200.0);
    float num4 = StanceController.IsLeftShoulder ? 1.14f : 1f;
    float num5 = StanceController.CurrentStance == EStance.ActiveAiming ? 0.9f : 1f;
    float num6 = StanceController.CurrentStance == EStance.ActiveAiming || !StanceController.IsAiming ? 0.8f : 1f;
    float num7 = StanceController.CurrentStance == EStance.ShortStock ? 1.15f : 1f;
    float num8 = StanceController.CurrentStance == EStance.ShortStock ? 0.6f : 1f;
    float num9 = Mathf.Clamp(Mathf.Pow(StanceController.BracingRecoilBonus, 0.75f), 0.8f, 1f);
    float totalRecoilAngle = ShootController.BaseTotalRecoilAngle;
    float num10 = (float) GameSetting<int>.op_Implicit(((SharedGameSettingsClass.GClass2153<GClass1053>) Singleton<SharedGameSettingsClass>.Instance.Game).Settings.FieldOfView) / 70f;
    float num11 = flag ? AddRecoilForcePatch.PistolShotFactor(ShootController.ShotCount) : 1f;
    float num12 = !WeaponStats.IsPistol ? Mathf.Clamp(AddRecoilForcePatch.RifleShotModifier(ShootController.ShotCount), 0.95f, 1.15f) : 1f;
    int num13 = (int) AddRecoilForcePatch._shotIndexField.GetValue((object) __instance) + 1;
    AddRecoilForcePatch._shotIndexField.SetValue((object) __instance, (object) num13);
    if ((int) AddRecoilForcePatch._shotIndexField.GetValue((object) __instance) == __instance.RecoilStableShotIndex)
      __instance.HandRotationRecoil.SetStableMode(true);
    __instance.HandRotationRecoil.MultiplayerByShotIndex = __instance.method_0();
    float num14;
    float num15;
    __instance.method_2(incomingForce, ref num14, ref num15);
    float num16 = Mathf.Clamp(PlayerState.RecoilInjuryMulti * num5 * num7 * num2 * StanceController.BracingRecoilBonus * num11 * num4, 0.25f, 1.25f) * (WeaponStats.IsPistol ? PluginConfig.PistolVertMulti.Value : PluginConfig.VertMulti.Value);
    float num17 = Mathf.Clamp(PlayerState.RecoilInjuryMulti * num7 * num2 * num11, 0.25f, 1.25f) * PluginConfig.HorzMulti.Value;
    ShootController.FactoredTotalVRecoil = num16 * ShootController.BaseTotalVRecoil;
    ShootController.FactoredTotalHRecoil = num17 * ShootController.BaseTotalHRecoil;
    float num18 = num17 * num10;
    float num19 = num14 * (num16 * num12);
    float num20 = num15 * num18;
    float num21 = WeaponStats.IsPistol ? PluginConfig.PistolDispMulti.Value : PluginConfig.RifleDispMulti.Value;
    float num22 = incomingForce * PlayerState.RecoilInjuryMulti * num7 * num3 * num9 * num4 * num12 * PluginConfig.RifleDispMulti.Value;
    ShootController.FactoredTotalDispersion = ShootController.BaseTotalDispersion * num21;
    __instance.HandRotationRecoil.ProgressRecoilAngleOnStable = new Vector2(ShootController.FactoredTotalDispersion * PluginConfig.RecoilRandomness.Value, ShootController.FactoredTotalDispersion * PluginConfig.RecoilRandomness.Value);
    __instance.BasicPlayerRecoilDegreeRange = new Vector2(ShootController.BaseTotalRecoilAngle, ShootController.BaseTotalRecoilAngle);
    __instance.BasicRecoilRadian = Vector2.op_Multiply(__instance.BasicPlayerRecoilDegreeRange, (float) Math.PI / 180f);
    Vector2 vector2;
    __instance.method_3(ref vector2);
    float num23 = WeaponStats.IsPistol ? PluginConfig.PistolCamMulti.Value : PluginConfig.RifleCamMulti.Value;
    float num24 = ShootController.ShotCount > 1 ? Mathf.Min((float) ((double) ShootController.ShotCount * 0.10000000149011612 + 1.0), 1.55f) : 1f;
    float totalCamRecoil = ShootController.BaseTotalCamRecoil * incomingForce * PlayerState.RecoilInjuryMulti * num8 * num6 * num2 * num24 * num23;
    ShootController.FactoredTotalCamRecoil = totalCamRecoil;
    __instance.ShotRecoilProcessValues[3].IntensityMultiplicator = totalCamRecoil;
    __instance.ShotRecoilProcessValues[4].IntensityMultiplicator = -totalCamRecoil;
    __instance.method_1(vector2, num19, num20);
    foreach (ShotEffector.RecoilShotVal recoilProcessValue in __instance.ShotRecoilProcessValues)
      recoilProcessValue.Process(__instance.RecoilDirection);
    __instance.WeaponRecoil.OnShoot();
    if (PluginConfig.EnableRecoilLogging.Value)
    {
      ModulePatch.Logger.LogWarning((object) "==========shoot==========");
      ModulePatch.Logger.LogWarning((object) ("camFactor " + (incomingForce * PlayerState.RecoilInjuryMulti * num8 * num6 * num2).ToString()));
      ModulePatch.Logger.LogWarning((object) ("vertFactor " + num16.ToString()));
      ModulePatch.Logger.LogWarning((object) ("horzFactor " + num18.ToString()));
      ModulePatch.Logger.LogWarning((object) ("dispFactor " + num22.ToString()));
      ModulePatch.Logger.LogWarning((object) ("mounting " + StanceController.BracingRecoilBonus.ToString()));
      ModulePatch.Logger.LogWarning((object) "====================");
    }
    if ((double) WeaponStats.ScopeAccuracyFactor < 0.0)
      AddRecoilForcePatch.DoZeroSift(firearmController, totalCamRecoil);
    return false;
  }
}
