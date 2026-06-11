// Decompiled with JetBrains decompiler
// Type: RealismMod.ReloadCylinderMagazinePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class ReloadCylinderMagazinePatch : ModulePatch
{
  private static FieldInfo _playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    ReloadCylinderMagazinePatch._playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("ReloadCylinderMagazine", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(Player.FirearmController __instance)
  {
    if (!((Player) ReloadCylinderMagazinePatch._playerField.GetValue((object) __instance)).IsYourPlayer)
      return;
    PlayerState.IsAttemptingToReloadInternalMag = true;
    PlayerState.IsAttemptingRevolverReload = true;
    if (PluginConfig.EnableReloadLogging.Value)
    {
      ModulePatch.Logger.LogWarning((object) "===ReloadCylinderMagazine===");
      ModulePatch.Logger.LogWarning((object) "=============");
    }
  }
}
