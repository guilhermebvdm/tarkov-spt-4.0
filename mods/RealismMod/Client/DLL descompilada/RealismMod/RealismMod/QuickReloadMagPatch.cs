// Decompiled with JetBrains decompiler
// Type: RealismMod.QuickReloadMagPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using HarmonyLib;
using RealismMod.Weapons;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class QuickReloadMagPatch : ModulePatch
{
  private static FieldInfo _playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    QuickReloadMagPatch._playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("QuickReloadMag", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(Player.FirearmController __instance, MagazineItemClass magazine)
  {
    if (!((Player) QuickReloadMagPatch._playerField.GetValue((object) __instance)).IsYourPlayer)
      return;
    if (Plugin.ServerConfig.reload_changes)
      ReloadController.SetMagReloadSpeeds(__instance, magazine, true);
    PlayerState.IsQuickReloading = true;
    if (PluginConfig.EnableReloadLogging.Value)
    {
      ModulePatch.Logger.LogWarning((object) "===QuickReloadMag===");
      ModulePatch.Logger.LogWarning((object) "=============");
    }
  }
}
