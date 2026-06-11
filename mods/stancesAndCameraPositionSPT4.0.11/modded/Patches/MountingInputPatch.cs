using System.Reflection;
using EFT;
using EFT.InputSystem;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CameraRotationMod.Patches
{
    public class MountingInputPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GamePlayerOwner), nameof(GamePlayerOwner.TranslateCommand));
        }

        [PatchPrefix]
        private static bool PatchPrefix(GamePlayerOwner __instance, ECommand command)
        {
            if (command == ECommand.WeaponMounting && Plugin._EnableWeaponMounting.Value)
            {
                if (MountingManager.IsBracing)
                {
                    MountingManager.IsMounting = !MountingManager.IsMounting;
                }
                else if (MountingManager.IsMounting)
                {
                    MountingManager.IsMounting = false;
                }
                return false;
            }
            return true;
        }
    }
}
