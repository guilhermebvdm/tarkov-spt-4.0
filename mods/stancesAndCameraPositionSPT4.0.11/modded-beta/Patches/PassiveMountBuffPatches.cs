using System;
using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.Animations.NewRecoil;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace CameraRotationMod.Patches
{
    // Buffs do mount PASSIVO (item 011). Só agem quando PassiveMountState.IsBracing — que só fica true
    // para o jogador local (gravado sob IsYourPlayer no PassiveMountDetectPatch). Cede ao mount ativo do
    // vanilla (IsBracing é limpo quando IsMountedState). Magnitudes são pontos de partida — calibrar
    // in-game para ficarem MAIS FRACAS que o ativo (PA-01-02).

    // Recoil reduzido. ref: AddRecoilForceMountPatch (item 004, validado em runtime).
    public class PassiveRecoilPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(NewRecoilShotEffect), "AddRecoilForce");

        [PatchPrefix]
        private static void Prefix(ref float incomingForce)
        {
            try
            {
                // NewRecoilShotEffect é o recoil de câmera/arma em 1ª pessoa (local). IsBracing só é true
                // para o jogador local (detecção com gate IsYourPlayer) — CR-01-01.
                if (Plugin._EnablePassiveMount.Value && PassiveMountState.IsBracing)
                    incomingForce *= Plugin._PassiveRecoilMultiplier.Value;
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[PassiveRecoil] {ex.Message}"); }
        }
    }

    // Sway (respiração) reduzido. ref: WeaponMountingPatch (item 004, validado em runtime).
    public class PassiveSwayPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(ProceduralWeaponAnimation),
                                  nameof(ProceduralWeaponAnimation.ProcessEffectors));

        [PatchPostfix]
        private static void Postfix(ProceduralWeaponAnimation __instance)
        {
            try
            {
                if (!Plugin._EnablePassiveMount.Value || !PassiveMountState.IsBracing) return;
                // AP-02 (CR-01-01): ProcessEffectors roda para TODA PWA (peers/bots em Fika). Aplicar só
                // ao MainPlayer — o IsBracing global não distingue por player.
                if (__instance != Singleton<GameWorld>.Instance?.MainPlayer?.ProceduralWeaponAnimation) return;
                if (__instance.Breath != null)
                    __instance.Breath.Intensity *= Plugin._PassiveSwayMultiplier.Value;
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[PassiveSway] {ex.Message}"); }
        }
    }
}
