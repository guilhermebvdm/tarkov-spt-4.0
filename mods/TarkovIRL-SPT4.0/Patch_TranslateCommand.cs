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

    protected override MethodBase GetTargetMethod()
    {
        return typeof(GamePlayerOwner).GetMethod("TranslateCommand", BindingFlags.Instance | BindingFlags.Public);
    }

    [SPT.Reflection.Patching.PatchPostfix]
    private static void PatchPostfix(GamePlayerOwner __instance, ECommand command)
    {
        Player player = __instance.Player;
        if (player == null || !player.IsYourPlayer)
            return;
            
        if (command == (ECommand)16 /*0x10*/)
            AugmentedReloadController.ToggleAugmentedMode();
        if (command == (ECommand)40 || command == (ECommand)41 || command == (ECommand)43 || command == (ECommand)42)
            WeaponSelectionController.Process(command, player);
        if (command == (ECommand)25)
            PlayerMotionController.IsHoldingBreath = true;
        if (command == (ECommand)26)
            PlayerMotionController.IsHoldingBreath = false;
        if (command == (ECommand)53)
            WeaponController.ToggleFolded();
            
        // Cleaned up the weird bitwise condition from decompilation
        if (command == (ECommand)133 || command == (ECommand)127 || command == (ECommand)128 || command == (ECommand)129 || command == (ECommand)130 || command == (ECommand)131 || command == (ECommand)132 || command == (ECommand)52 || command == (ECommand)46 || command == (ECommand)47 || command == (ECommand)48 || command == (ECommand)49 || command == (ECommand)50 || command == (ECommand)51)
            WeaponSelectionController.Process(command, player);
    }
}
