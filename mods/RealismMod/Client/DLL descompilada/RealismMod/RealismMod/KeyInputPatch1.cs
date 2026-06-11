// Decompiled with JetBrains decompiler
// Type: RealismMod.KeyInputPatch1
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.InputSystem;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class KeyInputPatch1 : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Class1604).GetMethod("TranslateCommand", BindingFlags.Instance | BindingFlags.Public);
  }

  private static void RechamberRound(Player.FirearmController fc, Player player)
  {
    Plugin.CanLoadChamber = true;
    int currentMagazineCount = fc.Weapon.GetCurrentMagazineCount();
    MagazineItemClass currentMagazine = ((Item) fc.Weapon).GetCurrentMagazine();
    if (currentMagazine == null)
      return;
    ((Player.AbstractHandsController) fc).FirearmsAnimator.SetAmmoInChamber(0.0f);
    ((Player.AbstractHandsController) fc).FirearmsAnimator.SetAmmoOnMag(currentMagazineCount);
    ((Player.AbstractHandsController) fc).FirearmsAnimator.SetAmmoCompatible(true);
    GStruct455<GInterface398> gstruct455 = currentMagazine.Cartridges.PopTo((TraderControllerClass) player.InventoryController, fc.Item.Chambers[0].CreateItemAddress());
    WeaponManagerClass weaponManagerClass = (WeaponManagerClass) AccessTools.Field(typeof (Player.FirearmController), "weaponManagerClass").GetValue((object) fc);
    weaponManagerClass.RemoveAllShells();
    AmmoItemClass resultItem = (AmmoItemClass) gstruct455.Value.ResultItem;
    ((Player.AbstractHandsController) fc).FirearmsAnimator.SetAmmoInChamber(1f);
    ((Player.AbstractHandsController) fc).FirearmsAnimator.SetAmmoOnMag(fc.Weapon.GetCurrentMagazineCount());
    weaponManagerClass.SetRoundIntoWeapon(resultItem, 0);
    ((Player.AbstractHandsController) fc).FirearmsAnimator.Rechamber(true);
    Plugin.StartRechamberTimer = true;
    Plugin.ChamberTimer = 0.0f;
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static bool PatchPrefix(Class1604 __instance, ECommand command)
  {
    if (StanceController.CurrentStance != EStance.PistolCompressed && (command == 40 || command == 41 || command == 43))
    {
      StanceController.DidWeaponSwap = true;
      return true;
    }
    if (command == 31 /*0x1F*/ || command == 32 /*0x20*/ || command == 34 || command == 33)
    {
      StanceController.IsMounting = false;
      return true;
    }
    if (command == 92 && Plugin.ServerConfig.manual_chambering)
    {
      Player yourPlayer = Utils.GetYourPlayer();
      Player.FirearmController handsController = yourPlayer.HandsController as Player.FirearmController;
      if (((Player.AbstractHandsController) handsController).IsAiming)
        handsController.ToggleAim();
      if (yourPlayer.MovementContext.CurrentState.Name == 21 || Plugin.CanLoadChamber || !handsController.Weapon.HasChambers || handsController.Weapon.Chambers.Length != 1 || handsController.Weapon.ChamberAmmoCount != 0 || ((Item) handsController.Weapon).GetCurrentMagazine() == null || ((Item) handsController.Weapon).GetCurrentMagazine().Count <= 0)
        return true;
      KeyInputPatch1.RechamberRound(handsController, yourPlayer);
      return false;
    }
    if (command == 64 /*0x40*/ || command == 5 || command == 6)
    {
      AimController.HeadDeviceStateChanged = true;
      return true;
    }
    if (Plugin.ServerConfig.enable_stances && command == 44)
    {
      if (!StanceController.IsInForcedLowReady && !StanceController.ShouldBlockAllStances)
        StanceController.ToggleLeftShoulder();
      return false;
    }
    if (command == 25 && Plugin.ServerConfig.recoil_attachment_overhaul && StanceController.IsAiming)
    {
      Player yourPlayer = Utils.GetYourPlayer();
      if (yourPlayer.Physical.HoldingBreath)
        return true;
      Player.FirearmController handsController = yourPlayer.HandsController as Player.FirearmController;
      if ((double) WeaponStats.TotalWeaponWeight <= 8.0)
        StanceController.DoWiggleEffects(yourPlayer, yourPlayer.ProceduralWeaponAnimation, handsController.Weapon, new Vector3(0.25f, 0.25f, 0.5f), wiggleFactor: 0.5f);
      return true;
    }
    if (command == 141)
      StanceController.IsMounting = false;
    bool flag1 = Plugin.ServerConfig.enable_stances && PluginConfig.BlockFiring.Value;
    bool flag2 = !Plugin.RealHealthController.ArmsAreIncapacitated && !Plugin.RealHealthController.HasOverdosed;
    bool flag3 = StanceController.CurrentStance != EStance.None && StanceController.CurrentStance != EStance.ActiveAiming && StanceController.CurrentStance != EStance.ShortStock && StanceController.CurrentStance != EStance.PistolCompressed;
    if (!(command == 1 & flag1 & flag2 & flag3))
      return true;
    StanceController.CurrentStance = EStance.None;
    StanceController.StoredStance = EStance.None;
    ((Player.ValueBlender) StanceController.StanceBlender).Target = 0.0f;
    return false;
  }
}
