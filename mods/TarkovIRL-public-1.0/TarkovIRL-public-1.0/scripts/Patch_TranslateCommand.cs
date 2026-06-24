using BepInEx.Configuration;
using EFT.InputSystem;
using EFT;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using static EFT.Player;

namespace TarkovIRL
{
    internal class Patch_TranslateCommand : ModulePatch
    {
        static FieldInfo _playerField;
        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(Class1599), "player_0");
            return typeof(Class1599).GetMethod("TranslateCommand", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Class1599 __instance, ECommand command)
        {

            Player player = (Player)_playerField.GetValue(__instance);
            if ((UnityEngine.Object)(object)player != (UnityEngine.Object)null && player.IsYourPlayer)
            {
                if (command == ECommand.ReloadWeapon)
                {
                    //UtilsTIRL.Log("reload called");
                    AugmentedReloadController.ToggleAugmentedMode();
                }

                if (command == ECommand.SelectFirstPrimaryWeapon || command == ECommand.SelectSecondPrimaryWeapon || command == ECommand.QuickSelectSecondaryWeapon || command == ECommand.SelectSecondaryWeapon)
                {
                    //player.QuickdrawWeaponFast = false;
                    WeaponSelectionController.Process(command, player);
                }

                if (command == ECommand.ToggleBreathing)
                {
                    PlayerMotionController.IsHoldingBreath = true;
                }
                if (command == ECommand.EndBreathing)
                {
                    PlayerMotionController.IsHoldingBreath = false;
                }
                if (command == ECommand.FoldStock)
                {
                    WeaponController.ToggleFolded();
                }

                bool selectionFromSlot = command == ECommand.PressSlot0;
                selectionFromSlot &= command == ECommand.PressSlot4;
                selectionFromSlot &= command == ECommand.PressSlot5;
                selectionFromSlot &= command == ECommand.PressSlot6;
                selectionFromSlot &= command == ECommand.PressSlot7;
                selectionFromSlot &= command == ECommand.PressSlot8;
                selectionFromSlot &= command == ECommand.PressSlot9;
                selectionFromSlot &= command == ECommand.SelectFastSlot0;
                selectionFromSlot &= command == ECommand.SelectFastSlot4;
                selectionFromSlot &= command == ECommand.SelectFastSlot5;
                selectionFromSlot &= command == ECommand.SelectFastSlot6;
                selectionFromSlot &= command == ECommand.SelectFastSlot7;
                selectionFromSlot &= command == ECommand.SelectFastSlot8;
                selectionFromSlot &= command == ECommand.SelectFastSlot9;
                if (selectionFromSlot)
                {
                    WeaponSelectionController.Process(command, player);
                    
                }
            }
        }
    }
}
