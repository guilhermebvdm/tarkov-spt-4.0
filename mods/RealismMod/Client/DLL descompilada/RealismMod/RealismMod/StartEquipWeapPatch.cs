// Decompiled with JetBrains decompiler
// Type: RealismMod.StartEquipWeapPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class StartEquipWeapPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) ((IEnumerable<MethodInfo>) typeof (Player.FirearmController.GClass1824).GetMethods(BindingFlags.Instance | BindingFlags.Public)).First<MethodInfo>((Func<MethodInfo, bool>) (m => m.GetParameters().Length == 1 && m.GetParameters()[0].Name == "onWeaponAppear"));
  }

  [PatchPrefix]
  private static bool Prefix(Player.FirearmController.GClass1824 __instance, Action onWeaponAppear)
  {
    Player.FirearmController firearmController = (Player.FirearmController) AccessTools.Field(typeof (Player.FirearmController.GClass1824), "firearmController_0").GetValue((object) __instance);
    Player player = (Player) AccessTools.Field(typeof (Player.FirearmController), "_player").GetValue((object) firearmController);
    if (!player.IsYourPlayer || player.MovementContext.CurrentState.Name == 21 || !firearmController.Weapon.HasChambers || firearmController.Weapon.Chambers.Length != 1)
      return true;
    MagazineItemClass magazineItemClass = (MagazineItemClass) AccessTools.Field(typeof (Player.FirearmController.GClass1824), "magazineItemClass").GetValue((object) __instance);
    bool flag1 = (bool) AccessTools.Field(typeof (Player.FirearmController.GClass1824), "bool_1").GetValue((object) __instance);
    AmmoItemClass ammoItemClass = (AmmoItemClass) AccessTools.Field(typeof (Player.FirearmController.GClass1824), "ammoItemClass").GetValue((object) __instance);
    WeaponManagerClass weaponManagerClass = (WeaponManagerClass) AccessTools.Field(typeof (Player.FirearmController.GClass1824), "weaponManagerClass").GetValue((object) __instance);
    AccessTools.Field(typeof (Player.FirearmController.GClass1824), "action_0").SetValue((object) __instance, (object) onWeaponAppear);
    ((Player.BaseAnimationOperationClass) __instance).Start();
    ((ObjectInHandsAnimator) ((Player.AbstractHandsController) firearmController).FirearmsAnimator).SetActiveParam(true, true);
    ((ObjectInHandsAnimator) ((Player.AbstractHandsController) firearmController).FirearmsAnimator).SetLayerWeight(((Player.AbstractHandsController) firearmController).FirearmsAnimator.LACTIONS_LAYER_INDEX, 0);
    player.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, (float) ((Item) firearmController.Weapon).CalculateCellSize().X);
    int chamberAmmoCount = firearmController.Weapon.ChamberAmmoCount;
    int currentMagazineCount = firearmController.Weapon.GetCurrentMagazineCount();
    MagazineItemClass currentMagazine = ((Item) firearmController.Weapon).GetCurrentMagazine();
    AccessTools.Field(typeof (Player.FirearmController.GClass1824), "magazineItemClass").SetValue((object) __instance, (object) currentMagazine);
    firearmController.AmmoInChamberOnSpawn = chamberAmmoCount;
    if (firearmController.Weapon.ChamberAmmoCount == 0)
    {
      Plugin.CanLoadChamber = false;
      Plugin.BlockChambering = true;
    }
    if (firearmController.Weapon.HasChambers)
      ((Player.AbstractHandsController) firearmController).FirearmsAnimator.SetAmmoInChamber((float) chamberAmmoCount);
    ((Player.AbstractHandsController) firearmController).FirearmsAnimator.SetAmmoOnMag(currentMagazineCount);
    player.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
    player.Skills.OnWeaponDraw((Item) firearmController.Weapon);
    bool flag2 = currentMagazine == null || currentMagazine.IsAmmoCompatible(firearmController.Weapon.Chambers);
    AccessTools.Field(typeof (Player.FirearmController.GClass1824), "bool_1").SetValue((object) __instance, (object) flag2);
    ((Player.AbstractHandsController) firearmController).FirearmsAnimator.SetAmmoCompatible(flag2);
    if (flag2 && currentMagazine != null && currentMagazine.Count > 0 && firearmController.Weapon.Chambers.Length != 0 && firearmController.Weapon.MalfState.State == 1)
      ((ObjectInHandsAnimator) ((Player.AbstractHandsController) firearmController).FirearmsAnimator).SetLayerWeight(((Player.AbstractHandsController) firearmController).FirearmsAnimator.MALFUNCTION_LAYER_INDEX, 0);
    if (((!Plugin.CanLoadChamber || currentMagazine == null || chamberAmmoCount != 0 ? 0 : (currentMagazineCount > 0 ? 1 : 0)) & (flag2 ? 1 : 0)) != 0 && firearmController.Item.Chambers.Length != 0)
    {
      Weapon.EMalfunctionState state = firearmController.Item.MalfState.State;
      if (state == 1)
        firearmController.Weapon.MalfState.ChangeStateSilent((Weapon.EMalfunctionState) 0);
      GStruct455<GInterface398> gstruct455 = currentMagazine.Cartridges.PopTo((TraderControllerClass) player.InventoryController, firearmController.Item.Chambers[0].CreateItemAddress());
      firearmController.Item.MalfState.ChangeStateSilent(state);
      if (gstruct455.Value == null)
        return false;
      weaponManagerClass.RemoveAllShells();
      player.UpdatePhones();
      AmmoItemClass resultItem = (AmmoItemClass) gstruct455.Value.ResultItem;
      AccessTools.Field(typeof (Player.FirearmController.GClass1824), "bulletClass").SetValue((object) __instance, (object) resultItem);
    }
    return false;
  }
}
