// Decompiled with JetBrains decompiler
// Type: RealismMod.Weapons.ReloadController
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using UnityEngine;

#nullable disable
namespace RealismMod.Weapons;

public static class ReloadController
{
  public static float MinimumReloadSpeed = 0.7f;
  public static float MaxInternalReloadSpeed = 1.5f;

  public static float MaxReloadSpeed => PlayerState.IsQuickReloading ? 1.65f : 1.4f;

  public static void SetMagReloadSpeeds(
    Player.FirearmController __instance,
    MagazineItemClass magazine,
    bool isQuickReload = false)
  {
    PlayerState.IsMagReloading = true;
    StanceController.CancelLowReady = true;
    StanceController.CancelLeftShoulder = true;
    Weapon weapon = __instance.Item;
    if (PlayerState.NoCurrentMagazineReload)
    {
      Player player = (Player) AccessTools.Field(typeof (Player.FirearmController), "_player").GetValue((object) __instance);
      ReloadController.MagReloadSpeedModifier(weapon, magazine, false, true);
      player.HandsAnimator.SetAnimationSpeed(Mathf.Clamp(WeaponStats.CurrentMagReloadSpeed * PlayerState.ReloadInjuryMulti * PlayerState.ReloadSkillMulti * GearController.GearReloadMulti * StanceController.HighReadyManipBuff * StanceController.ActiveAimManipBuff * Plugin.RealHealthController.AdrenalineReloadBonus * Mathf.Max(PlayerState.RemainingArmStamFactor, 0.8f), 0.65f, 1.35f));
    }
    else
      ReloadController.MagReloadSpeedModifier(weapon, magazine, true, false, isQuickReload);
  }

  public static void MagReloadSpeedModifier(
    Weapon weapon,
    MagazineItemClass magazine,
    bool isNewMag,
    bool reloadFromNoMag,
    bool isQuickReload = false)
  {
    WeaponMod dataObj = TemplateStats.GetDataObj<WeaponMod>(TemplateStats.WeaponModStats, MongoID.op_Implicit(((Item) magazine).TemplateId));
    float num1 = (float) ((weapon.IsBeltMachineGun ? (double) ((Item) magazine).TotalWeight * 16.5 * 0.5 : (double) ((Item) magazine).TotalWeight * 16.5) / -100.0 + 1.0);
    float reloadSpeed = dataObj.ReloadSpeed;
    float reloadSpeedLessMag = WeaponStats.TotalReloadSpeedLessMag;
    float num2 = WeaponStats.IsPistol || WeaponStats.HasShoulderContact ? 1f : 0.8f;
    float num3 = (float) (1.0 - (double) PlayerState.TotalModifiedWeightMinusWeapon * (1.0 / 1000.0));
    float num4 = (float) ((double) reloadSpeed / 100.0 + 1.0) * num1 * reloadSpeedLessMag * num2 * num3;
    if (reloadFromNoMag)
    {
      WeaponStats.NewMagReloadSpeed = num4;
      WeaponStats.CurrentMagReloadSpeed = num4;
    }
    else if (isNewMag)
      WeaponStats.NewMagReloadSpeed = num4;
    else
      WeaponStats.CurrentMagReloadSpeed = num4;
    if (isQuickReload)
    {
      WeaponStats.NewMagReloadSpeed *= PluginConfig.QuickReloadSpeedMulti.Value;
      WeaponStats.CurrentMagReloadSpeed *= PluginConfig.QuickReloadSpeedMulti.Value;
    }
    WeaponStats.NewMagReloadSpeed *= PluginConfig.GlobalReloadSpeedMulti.Value;
    WeaponStats.CurrentMagReloadSpeed *= PluginConfig.GlobalReloadSpeedMulti.Value;
  }

  public static void ReloadStateCheck(Player player, Player.FirearmController fc)
  {
    PlayerState.IsInReloadOpertation = fc.IsInReloadOperation();
    if (PlayerState.IsInReloadOpertation)
    {
      if (StanceController.CurrentStance == EStance.PatrolStance)
        StanceController.CurrentStance = EStance.None;
      StanceController.ModifyHighReady = true;
      StanceController.CancelShortStock = true;
      StanceController.CancelActiveAim = true;
      StanceController.CancelLeftShoulder = true;
      if (!PlayerState.IsAttemptingToReloadInternalMag || !Plugin.ServerConfig.reload_changes)
        return;
      bool flag = fc.Item.WeapClass == "shotgun";
      StanceController.CancelHighReady = !flag;
      StanceController.CancelLowReady = flag || WeaponStats.IsPistol;
      float num1 = !flag || StanceController.CurrentStance != EStance.HighReady ? 1f : StanceController.HighReadyManipBuff;
      float num2 = flag || StanceController.CurrentStance != EStance.LowReady ? 1f : StanceController.LowReadyManipBuff;
      float num3 = Mathf.Clamp(WeaponStats.CurrentMagReloadSpeed * PluginConfig.InternalMagReloadMulti.Value * PlayerState.ReloadSkillMulti * PlayerState.ReloadInjuryMulti * num1 * num2 * PlayerState.RemainingArmStamReloadFactor, ReloadController.MinimumReloadSpeed, ReloadController.MaxInternalReloadSpeed);
      player.HandsAnimator.SetAnimationSpeed(num3);
      if (PluginConfig.EnableReloadLogging.Value)
        Utils.Logger.LogWarning((object) ("IsAttemptingToReloadInternalMag = " + num3.ToString()));
    }
    else
    {
      PlayerState.IsAttemptingToReloadInternalMag = false;
      PlayerState.IsAttemptingRevolverReload = false;
    }
  }
}
