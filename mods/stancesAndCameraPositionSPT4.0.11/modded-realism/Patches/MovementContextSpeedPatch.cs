using System.Reflection;
using SPT.Reflection.Patching;
using EFT;
using HarmonyLib;

namespace CameraRotationMod.Patches
{
    public class MovementContextSpeedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(MovementContext), nameof(MovementContext.MaxSpeed));
        }

        [PatchPostfix]
        private static void Postfix(ref float __result)
        {
            if (Plugin._WalkSpeedMultiplier != null)
            {
                __result *= Plugin._WalkSpeedMultiplier.Value;
            }
        }
    }

    public class MovementContextSprintSpeedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(MovementContext), nameof(MovementContext.SprintSpeed));
        }

        [PatchPostfix]
        private static void Postfix(ref float __result)
        {
            if (Plugin._SprintSpeedMultiplier != null)
            {
                __result *= Plugin._SprintSpeedMultiplier.Value;
            }
        }
    }

    public class PlayerChangeSpeedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.ChangeSpeed));
        }

        [PatchPrefix]
        private static bool Prefix(Player __instance, float speedDelta)
        {
            // Se o mouse scroll com o modificador (Ctrl) estiver sendo usado para mudar de stance,
            // bloqueamos a mudança de velocidade no jogo original.
            if (__instance.IsYourPlayer && Plugin._EnableMouseWheelCycle != null && Plugin._EnableMouseWheelCycle.Value)
            {
                if (Plugin._MouseWheelModifierKey != null && UnityEngine.Input.GetKey(Plugin._MouseWheelModifierKey.Value))
                {
                    return false; // Ignora o método original (não altera a velocidade)
                }
            }
            return true; // Executa o método original normalmente
        }
    }
}
