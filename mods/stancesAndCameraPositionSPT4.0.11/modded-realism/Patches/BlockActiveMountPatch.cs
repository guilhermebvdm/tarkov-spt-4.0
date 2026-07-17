using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CameraRotationMod.Patches
{
    /// <summary>
    /// Item 015: bloqueia a ATIVAÇÃO do mount de superfície (mount vanilla) quando o jogador LOCAL está em
    /// Stance 1/2/3, sem mirar e sem estar em prone. Prefix em <c>Player.TryMountWeapon</c> — ponto único de
    /// ativação (input WeaponMounting), antes da detecção de ponto do componente de mounting. O bipé NÃO passa
    /// por aqui (é <c>FirearmController.Class1270</c>/<c>BipodState</c>), logo fica de fora naturalmente
    /// (decisão "bipé é exceção"). Prone também é exceção — o mount deitado é legítimo (igual ao item 011).
    /// Ref: Assembly-CSharp/EFT/Player.cs:26218.
    /// </summary>
    public class BlockActiveMountPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            // ref: Assembly-CSharp/EFT/Player.cs:26218 (public void TryMountWeapon())
            => AccessTools.Method(typeof(Player), "TryMountWeapon");

        [PatchPrefix]
        private static bool Prefix(Player __instance)
        {
            try
            {
                if (Plugin._BlockActiveMountInStance != null && !Plugin._BlockActiveMountInStance.Value)
                    return true;                                       // feature desligada no F12 → vanilla
                if (__instance == null || !__instance.IsYourPlayer) return true;   // AP-02: só o MainPlayer local

                var pwa = __instance.ProceduralWeaponAnimation;
                bool isAiming = pwa != null && pwa.IsAiming;           // ref: ProceduralWeaponAnimation.IsAiming
                // Stance 1/2/3, sem ADS e sem prone (o 011 também cede ao vanilla em prone — PassiveMountDetectPatch.cs:56).
                if (StanceManager.CurrentStance != Stance.Default && !isAiming && !__instance.IsInPronePose)
                    return false;                                      // pula o TryMountWeapon original → não monta
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[Mount015] BlockActiveMount {ex.Message}"); }
            return true;
        }
    }
}
