using System;
using System.Reflection;
using EFT;
using Fika.Core.Main.Players;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CameraRotationMod.Patches
{
    /// <summary>
    /// Item 014 (fix-03): aplica o offset de stance do jogador OBSERVADO no transform FINAL da arma de 3a
    /// pessoa (PlayerBones.Weapon_Root_Anim), num POSTFIX de ObservedPlayer.ObservedVisualPass — o ÚNICO
    /// ponto não-sobrescrito. A investigação (3 sub-agents) mostrou que Kinematics (ObservedPlayer.cs:1889)
    /// reescreve o Weapon_Root_Anim DEPOIS do ShiftWeaponRoot, e que o thirdPersonAuthority pode atenuar o
    /// DeltaRotation — por isso ApplyComplexRotation / ProcessEffectors / ShiftWeaponRoot-Prefix não funcionaram
    /// (o log nunca aparecia OU o offset era apagado). Aqui rodamos depois de TUDO (ShiftWeaponRoot + Kinematics
    /// + IK + ManualLateUpdate), então nada sobrescreve.
    /// </summary>
    public class ObservedStanceVisualPatch : ModulePatch
    {
        private static bool _loggedHook;

        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(ObservedPlayer), "ObservedVisualPass");

        [PatchPostfix]
        private static void Postfix(ObservedPlayer __instance)
        {
            try
            {
                if (!_loggedHook)
                {
                    _loggedHook = true;
                    Plugin.Logger.LogInfo("[StanceSync-014] ObservedVisualPass Postfix RODOU para observado.");
                }
                __instance.gameObject.GetComponent<Networking.ObservedStanceAnimator>()?.ApplyToWeaponRoot(__instance.PlayerBones);
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[StanceSync-014] VisualPatch {ex.Message}"); }
        }
    }
}
