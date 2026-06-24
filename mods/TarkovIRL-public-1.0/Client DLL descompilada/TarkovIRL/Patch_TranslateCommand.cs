// Decompiled with JetBrains decompiler
// Type: TarkovIRL.Patch_TranslateCommand
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using EFT;
using EFT.InputSystem;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal class Patch_TranslateCommand : ModulePatch
{
  private static FieldInfo _playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    Patch_TranslateCommand._playerField = AccessTools.Field(typeof (Class1599), "player_0");
    return (MethodBase) typeof (Class1599).GetMethod("TranslateCommand", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(Class1599 __instance, ECommand command)
  {
    Player player = (Player) Patch_TranslateCommand._playerField.GetValue((object) __instance);
    if (!Object.op_Inequality((Object) player, (Object) null) || !player.IsYourPlayer)
      return;
    if (command == 16 /*0x10*/)
      AugmentedReloadController.ToggleAugmentedMode();
    if (command == 40 || command == 41 || command == 43 || command == 42)
      WeaponSelectionController.Process(command, player);
    if (command == 25)
      PlayerMotionController.IsHoldingBreath = true;
    if (command == 26)
      PlayerMotionController.IsHoldingBreath = false;
    if (command == 53)
      WeaponController.ToggleFolded();
    if (command == 133 & command == (int) sbyte.MaxValue & command == 128 /*0x80*/ & command == 129 & command == 130 & command == 131 & command == 132 & command == 52 & command == 46 & command == 47 & command == 48 /*0x30*/ & command == 49 & command == 50 & command == 51)
      WeaponSelectionController.Process(command, player);
  }
}
