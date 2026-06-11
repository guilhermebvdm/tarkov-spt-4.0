// Decompiled with JetBrains decompiler
// Type: RealismMod.AimController
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.InventoryLogic;
using UnityEngine;

#nullable disable
namespace RealismMod;

public static class AimController
{
  private static bool hasSetCanAds;
  private static bool hasSetActiveAimADS;
  private static bool wasToggled;
  public static bool AimStateChanged;
  public static bool HeadDeviceStateChanged;
  public static float consoomTimer;

  public static void ADSCheck(Player player, Player.FirearmController fc)
  {
    if (!player.IsYourPlayer || fc.Item == null)
      return;
    FaceShieldComponent component1 = player.FaceShieldObserver.Component;
    NightVisionComponent component2 = player.NightVisionObserver.Component;
    ThermalVisionComponent component3 = player.ThermalVisionObserver.Component;
    bool flag1 = component1 != null && (component1?.Togglable == null || component1.Togglable.On);
    bool flag2 = component2 != null && (component2?.Togglable == null || component2.Togglable.On);
    bool flag3 = component3 != null && (component3?.Togglable == null || component3.Togglable.On);
    bool flag4 = !WeaponStats.WeaponCanFSADS && !GearController.GearAllowsADS;
    bool flag5 = PluginConfig.EnableFSPatch.Value && (flag1 & flag4 || flag4 && (component1 == null || component1?.Togglable == null));
    bool flag6 = PluginConfig.EnableNVGPatch.Value && ((!flag2 ? 0 : (player.ProceduralWeaponAnimation.CurrentScope.IsOptic ? 1 : 0)) | (flag3 ? 1 : 0)) != 0;
    GearController.FSIsActive = flag1;
    GearController.NVGIsActive = flag2 | flag3;
    if (AimController.HeadDeviceStateChanged)
    {
      GearController.CheckGear(Utils.GetYourPlayer());
      AimController.HeadDeviceStateChanged = false;
    }
    fc.UpdateHipInaccuracy();
    if (Plugin.ServerConfig.enable_stances)
    {
      if (flag6 | flag5)
      {
        if (!AimController.hasSetCanAds)
        {
          if (((Player.AbstractHandsController) fc).IsAiming)
            fc.ToggleAim();
          PlayerState.IsAllowedADS = false;
          AimController.hasSetCanAds = true;
        }
      }
      else
      {
        PlayerState.IsAllowedADS = true;
        AimController.hasSetCanAds = false;
      }
      if (!AimController.wasToggled && flag1 | flag2)
        AimController.wasToggled = true;
      if (AimController.wasToggled && !flag1 && !flag2 && StanceController.CurrentStance == EStance.ActiveAiming)
      {
        StanceController.WasActiveAim = false;
        if (PluginConfig.ToggleActiveAim.Value)
        {
          ((Player.ValueBlender) StanceController.StanceBlender).Target = 0.0f;
          StanceController.CurrentStance = EStance.None;
        }
        AimController.wasToggled = false;
      }
      AimController.AimStateChanged = false;
      if (StanceController.CurrentStance == EStance.ActiveAiming && !AimController.hasSetActiveAimADS)
      {
        player.MovementContext.SetAimingSlowdown(true, 0.33f);
        AimController.hasSetActiveAimADS = true;
      }
      else if (StanceController.CurrentStance != EStance.ActiveAiming && AimController.hasSetActiveAimADS)
      {
        player.MovementContext.SetAimingSlowdown(false, 0.33f);
        if (((Player.AbstractHandsController) fc).IsAiming)
          player.MovementContext.SetAimingSlowdown(true, 0.33f);
        AimController.hasSetActiveAimADS = false;
      }
      if (((Player.AbstractHandsController) fc).IsAiming && StanceController.CurrentStance == EStance.PatrolStance)
        StanceController.CurrentStance = EStance.None;
    }
    if (player.ProceduralWeaponAnimation.OverlappingAllowsBlindfire)
    {
      StanceController.IsAiming = ((Player.AbstractHandsController) fc).IsAiming;
      StanceController.PistolIsColliding = false;
    }
    else if (WeaponStats.IsStocklessPistol)
      StanceController.PistolIsColliding = true;
    if (PlayerState.BlockFSWhileConsooming)
    {
      AimController.consoomTimer += Time.deltaTime;
      if ((double) AimController.consoomTimer >= 1.0)
      {
        PlayerState.BlockFSWhileConsooming = false;
        AimController.consoomTimer = 0.0f;
      }
    }
  }
}
