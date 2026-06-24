// Decompiled with JetBrains decompiler
// Type: TarkovIRL.Patch_ReloadMag
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace TarkovIRL;

public class Patch_ReloadMag : ModulePatch
{
  private static FieldInfo playerField;

  protected override MethodBase GetTargetMethod()
  {
    Patch_ReloadMag.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("ReloadMag", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(Player.FirearmController __instance)
  {
    Player player = (Player) Patch_ReloadMag.playerField.GetValue((object) __instance);
    if (!player.IsYourPlayer)
      return;
    AugmentedReloadController.RefreshAnimator(player.HandsAnimator);
  }
}
