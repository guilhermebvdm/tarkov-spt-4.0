using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CameraRotationMod.Patches
{
    /// <summary>
    /// Item 014 (fix-03): aplica o offset de stance do jogador OBSERVADO na janela PRÉ-IK — num POSTFIX de
    /// <c>PlayerBones.ShiftWeaponRoot</c>, que roda em ObservedPlayer.ObservedVisualPass na linha ~1876, ANTES
    /// do alvo da IK das mãos (<c>method_20</c>, 1884) e do solve (<c>method_19</c>, 1886).
    ///
    /// Por que aqui: a IK das mãos do observado mira nos markers <c>weapon_L/R_IK_marker</c>, que são FILHOS do
    /// <c>Weapon_Root_Anim</c>. Movendo o Weapon_Root_Anim nesta janela, os markers se deslocam → a LimbIK leva o
    /// BRAÇO até a stance → o <c>Kinematics</c> (1889) mantém a ARMA colada na mão. Braço E arma acompanham.
    ///
    /// A tentativa anterior (06-fix-02, Postfix de ObservedVisualPass) rodava DEPOIS da IK/Kinematics → a IK já tinha solveado o braço na
    /// pose sem offset, então só a arma (filho do Weapon_Root_Anim) se movia e descolava da mão. Investigação por
    /// 2 sub-agents (Assembly + Fika) confirmou a janela correta = entre ShiftWeaponRoot e method_20. Postfix de
    /// ShiftWeaponRoot é o hook robusto: nome estável (não obfuscado), força total, sem a atenuação do
    /// thirdPersonAuthority que penalizaria um Prefix via DeltaRotation.
    /// Ref: ObservedPlayer.cs:1876-1889 · PlayerBones.cs:328-419 (ShiftWeaponRoot) · Player.method_20/19.
    /// </summary>
    public class ObservedStanceShiftPatch : ModulePatch
    {
        private static bool _loggedHook;

        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(PlayerBones), "ShiftWeaponRoot");

        [PatchPostfix]
        private static void Postfix(PlayerBones __instance, EPointOfView pv)
        {
            try
            {
                // Só 3ª pessoa (observados/TP). O branch de 1ª pessoa é do jogador local — tratado pelo
                // ApplyComplexRotationPatch; aqui não tocamos.
                if (pv == EPointOfView.FirstPerson) return;

                Player p = __instance.Player;
                if (p == null || p.IsYourPlayer) return;   // AP-02: observado apenas, nunca o MainPlayer local

                var animator = p.gameObject.GetComponent<Networking.ObservedStanceAnimator>();
                if (animator == null) return;

                if (!_loggedHook)
                {
                    _loggedHook = true;
                    Plugin.Logger.LogInfo("[StanceSync-014] ShiftWeaponRoot Postfix RODOU para observado (pré-IK).");
                }
                animator.ApplyToWeaponRoot(__instance);
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[StanceSync-014] ShiftPatch {ex.Message}"); }
        }
    }
}
