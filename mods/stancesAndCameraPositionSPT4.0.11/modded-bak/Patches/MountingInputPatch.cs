using System.Reflection;
using EFT;
using EFT.InputSystem;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CameraRotationMod.Patches
{
    // Item 004 (06-fix-01): mount ATIVO via comando nativo WeaponMounting (ECommand 140), espelhando
    // RealismMod KeyInputPatch2. Suprime o mount nativo do EFT (return false) para que o estado de mount
    // seja 100% do mod — EXCETO com bipé, onde o sistema nativo é mantido. Toggle: Active -> None; senão,
    // se há superfície detectada (Passive) -> Active. Usa a tecla nativa de mount (rebindável no EFT).
    public class MountingInputPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GamePlayerOwner), nameof(GamePlayerOwner.TranslateCommand));
        }

        [PatchPrefix]
        private static bool PatchPrefix(GamePlayerOwner __instance, ECommand command)
        {
            if (!Plugin._EnableWeaponMounting.Value) return true;
            if (command != ECommand.WeaponMounting) return true;

            var player = __instance.Player;
            if (player == null || !player.IsYourPlayer) return true;

            // Bipé: deixa o sistema nativo cuidar (não suprime).
            var pwa = player.ProceduralWeaponAnimation;
            if (pwa != null && pwa.IsBipodUsed) return true;

            MountingManager.ToggleActiveMount();
            // Sem superfície detectada, ToggleActiveMount é no-op — mas ainda suprimimos o comando nativo
            // para evitar o mount do EFT numa superfície que o nosso raycast não validou (critério unificado).

            return false; // suprime o mount nativo
        }
    }
}
