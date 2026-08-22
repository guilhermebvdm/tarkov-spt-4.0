using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CameraRotationMod.Patches
{
    // Item 012: o StaminaController é a autoridade única da stamina de braço. Estes dois Prefixes NEUTRALIZAM
    // o loop vanilla (Process) e os consumos pontuais (Consume) SOMENTE para a instância de braço do MainPlayer
    // enquanto ControllingHands. A Stamina de PERNA (mesma classe GClass774, instância diferente) fica intacta
    // graças ao gate por instância. Substitui os 3 patches do 06-fix-01 (Restoration/Consume-prone/Process-prone).

    public class HandsStaminaNeutralizePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(GClass774), nameof(GClass774.Process));   // ref: GClass774.cs:303

        [PatchPrefix]
        private static bool Prefix(GClass774 __instance)
        {
            try
            {
                var h = Singleton<GameWorld>.Instance?.MainPlayer?.Physical?.HandsStamina;
                // PA-01-01: gate por flag do StaminaController; defasagem de <=1 frame na borda é segura
                // (nunca neutraliza a perna — __instance == HandsStamina garante o braço).
                if (StaminaController.ControllingHands && __instance == h) return false;  // o controller escreve
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[HandsNeutralize.Process] {ex}"); }
            return true;
        }
    }

    public class HandsConsumeNeutralizePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(GClass774), nameof(GClass774.Consume));   // ref: GClass774.cs:241

        [PatchPrefix]
        private static bool Prefix(GClass774 __instance, ref float __result)
        {
            try
            {
                var h = Singleton<GameWorld>.Instance?.MainPlayer?.Physical?.HandsStamina;
                if (StaminaController.ControllingHands && __instance == h)
                {
                    __result = __instance.Current;   // ref: GClass774.cs:23 — retorno neutro
                    return false;
                }
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[HandsNeutralize.Consume] {ex}"); }
            return true;
        }
    }
}
