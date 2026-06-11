// Decompiled with JetBrains decompiler
// Type: RealismMod.ReloadMagPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using RealismMod.Weapons;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class ReloadMagPatch : ModulePatch
{
  private static FieldInfo _playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    ReloadMagPatch._playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("ReloadMag", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(Player.FirearmController __instance, MagazineItemClass magazine)
  {
    if (!((Player) ReloadMagPatch._playerField.GetValue((object) __instance)).IsYourPlayer || !Plugin.ServerConfig.reload_changes)
      return;
    ReloadController.SetMagReloadSpeeds(__instance, magazine);
    if (PluginConfig.EnableReloadLogging.Value)
    {
      ModulePatch.Logger.LogWarning((object) "ReloadMag Patch");
      ModulePatch.Logger.LogWarning((object) ("magazine = " + GClass2112.LocalizedName((Item) magazine)));
      ModulePatch.Logger.LogWarning((object) ("magazine weight = " + ((Item) magazine).TotalWeight.ToString()));
    }
  }
}
