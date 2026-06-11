// Decompiled with JetBrains decompiler
// Type: RealismMod.ToggleAimPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class ToggleAimPatch : ModulePatch
{
  private static FieldInfo playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    ToggleAimPatch.playerField = AccessTools.Field(typeof (Player.ItemHandsController), "_player");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("ToggleAim", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(Player.FirearmController __instance)
  {
    if (!((Player) ToggleAimPatch.playerField.GetValue((object) __instance)).IsYourPlayer)
      return true;
    AimController.AimStateChanged = true;
    bool flag = PluginConfig.EnableFSPatch.Value || PluginConfig.EnableNVGPatch.Value;
    StanceController.CanResetAimDrain = true;
    if (PlayerState.SprintBlockADS && !PlayerState.TriedToADSFromSprint)
    {
      PlayerState.TriedToADSFromSprint = true;
      return false;
    }
    PlayerState.TriedToADSFromSprint = false;
    return !flag || PlayerState.IsAllowedADS;
  }
}
