// Decompiled with JetBrains decompiler
// Type: RealismMod.ChamberCheckUIPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.Screens;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class ChamberCheckUIPatch : ModulePatch
{
  private static FieldInfo ammoCountPanelField;
  private static FieldInfo _playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    ChamberCheckUIPatch._playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    ChamberCheckUIPatch.ammoCountPanelField = AccessTools.Field(typeof (EftBattleUIScreen), "_ammoCountPanel");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("CheckChamber", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(Player.FirearmController __instance)
  {
    if (!((Player) ChamberCheckUIPatch._playerField.GetValue((object) __instance)).IsYourPlayer)
      return;
    Slot slot = ((IEnumerable<Slot>) __instance.Weapon.Chambers).FirstOrDefault<Slot>();
    AmmoItemClass containedItem = slot == null ? (AmmoItemClass) null : slot.ContainedItem as AmmoItemClass;
    if (containedItem != null)
      ((BattleUIScreen<EftBattleUIScreen.GClass3575, EEftScreenType>) Singleton<CommonUI>.Instance.EftBattleUIScreen).ShowAmmoDetails(1, 10, 10, GClass2112.LocalizedName((Item) containedItem), false);
    else if (__instance.Weapon.Chambers.Length == 1)
      ((BattleUIScreen<EftBattleUIScreen.GClass3575, EEftScreenType>) Singleton<CommonUI>.Instance.EftBattleUIScreen).ShowAmmoDetails(0, 10, 10, (string) null, false);
  }
}
