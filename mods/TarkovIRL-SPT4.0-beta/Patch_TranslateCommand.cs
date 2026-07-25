using EFT;
using EFT.InputSystem;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal class Patch_TranslateCommand : ModulePatch
{
  protected override MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (GamePlayerOwner).GetMethod("TranslateCommand", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(GamePlayerOwner __instance, ECommand command)
  {
    Player player = ((PlayerOwner) __instance).Player;
    if ((player == null) || !player.IsYourPlayer)
      return;
    if (((int)command == 40) || ((int)command == 41) || ((int)command == 43) || ((int)command == 42))
      WeaponSelectionController.Process(command, player);
    if (((int)command == 25))
      PlayerMotionController.IsHoldingBreath = true;
    if (((int)command == 26))
      PlayerMotionController.IsHoldingBreath = false;
    if (((int)command == 53))
      WeaponController.ToggleFolded();
    if (((int)command != 133) && ((int)command != (int)sbyte.MaxValue) && ((int)command != 128) /*0x80*/ && ((int)command != 129) && ((int)command != 130) && ((int)command != 131) && ((int)command != 132) && ((int)command != 52) && ((int)command != 46) && ((int)command != 47) && ((int)command != 48) /*0x30*/ && ((int)command != 49) && ((int)command != 50) && ((int)command != 51))
      return;
    WeaponSelectionController.Process(command, player);
  }
}

