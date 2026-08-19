using System.Reflection;
using ActionPOV.Core;
using EFT;
using EFT.Animations;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

#nullable disable
namespace ActionPOV.Patches
{
    // 1. Interceptação e Divisão Proporcional de Input
    public class Patch_PlayerRotate : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.Rotate));
        }

        [PatchPrefix]
        private static bool Prefix(Player __instance, ref Vector2 deltaRotation, bool ignoreClamp)
        {
            if (__instance == null || !__instance.IsYourPlayer || !Plugin.EnableMod.Value)
                return true;

            // Guards de Estado (Sprint, Inventário, Cura/Itens)
            if (__instance.MovementContext.CurrentState.Name == EPlayerState.Stationary ||
                __instance.MovementContext.IsSprintEnabled ||
                __instance.HandsController is Player.UsableItemController)
            {
                KineticSpringEngine.Reset();
                return true;
            }

            bool isAiming = __instance.HandsController != null && __instance.HandsController.IsAiming;
            KineticSpringEngine.ProcessMouseInput(ref deltaRotation, isAiming);
            return true;
        }
    }

    // 2. Cinética da Visão / Roll Orgânico da Cabeça
    public class Patch_SetHeadRotation : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ProceduralWeaponAnimation), nameof(ProceduralWeaponAnimation.SetHeadRotation));
        }

        [PatchPrefix]
        private static bool Prefix(ProceduralWeaponAnimation __instance, Vector3 headRot)
        {
            if (!Plugin.EnableMod.Value) return true;

            var player = EFTBindings.GetPlayer(__instance);
            if (player == null || !player.IsYourPlayer) return true;

            if (player.MovementContext.CurrentState.Name == EPlayerState.Stationary) return true;

            // Base na rotação nativa calculada pelo PWA, injetando o Roll e Pitch/Yaw lag
            Vector3 finalRot = headRot;
            finalRot.x += KineticSpringEngine.CurrentHeadPitch;
            finalRot.y += KineticSpringEngine.CurrentHeadYaw;
            finalRot.z = KineticSpringEngine.CurrentHeadRoll;

            // Injeção de Offsets de Diagnóstico Manual (F12)
            if (Plugin.EnableDiagnosticOverrides.Value)
            {
                finalRot.x += Plugin.DebugHeadRotX.Value;
                finalRot.y += Plugin.DebugHeadRotY.Value;
                finalRot.z += Plugin.DebugHeadRotZ.Value;
            }

            player.HeadRotation = finalRot;
            EFTBindings.SetHeadRotationVec(__instance, finalRot);

            return false; // Assume o controle da cabeça
        }
    }

    // 3. Aplicação do Pivô Esférico e Rotação das Mãos/Arma
    public class Patch_CalculateCameraPosition : ModulePatch
    {
        private static bool _wasAimingLastFrame = false;
        private static Transform _lastWeaponRoot = null;
        private static Vector3 _originalLocalPosition = Vector3.zero;
        private static Quaternion _originalLocalRotation = Quaternion.identity;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ProceduralWeaponAnimation), nameof(ProceduralWeaponAnimation.CalculateCameraPosition));
        }

        [PatchPostfix]
        private static void Postfix(ProceduralWeaponAnimation __instance)
        {
            if (!Plugin.EnableMod.Value) return;

            var player = EFTBindings.GetPlayer(__instance);
            if (player == null || !player.IsYourPlayer || __instance.HandsContainer == null) return;

            Transform weaponRoot = __instance.HandsContainer.WeaponRoot;
            if (weaponRoot == null) return;

            // Blindagem contra acúmulo infinito de transformações no WeaponRoot
            if (_lastWeaponRoot == weaponRoot)
            {
                weaponRoot.localPosition = _originalLocalPosition;
                weaponRoot.localRotation = _originalLocalRotation;
            }
            else
            {
                _lastWeaponRoot = weaponRoot;
            }
            _originalLocalPosition = weaponRoot.localPosition;
            _originalLocalRotation = weaponRoot.localRotation;

            // Detecção de Transição para ADS
            bool isAiming = __instance.IsAiming;
            if (isAiming && !_wasAimingLastFrame)
            {
                KineticSpringEngine.Reset();
            }
            _wasAimingLastFrame = isAiming;

            // Executa a física de amortecimento, roll e sway orgânico
            KineticSpringEngine.UpdatePhysics(player, Time.deltaTime);

            // Calcula a translação esférica ancorada no ombro
            KineticSpringEngine.CalculateArmOffsets(out Vector3 posOffset, out Quaternion rotOffset);

            // Aplica os offsets finais da física
            weaponRoot.localPosition += posOffset;
            weaponRoot.localRotation *= rotOffset;

            // Injeção de Offsets de Diagnóstico Manual (F12)
            if (Plugin.EnableDiagnosticOverrides.Value)
            {
                Vector3 manualPos = new Vector3(Plugin.DebugWeaponPosX.Value, Plugin.DebugWeaponPosY.Value, Plugin.DebugWeaponPosZ.Value);
                Quaternion manualRot = Quaternion.Euler(Plugin.DebugWeaponRotX.Value, Plugin.DebugWeaponRotY.Value, Plugin.DebugWeaponRotZ.Value);

                weaponRoot.localPosition += manualPos;
                weaponRoot.localRotation *= manualRot;
            }
        }
    }

    // 4. Atenuação do Sway Vanilla do Tarkov (Permite que a nossa mola física atue limpa)
    public class Patch_UpdateSwayFactors : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ProceduralWeaponAnimation), "UpdateSwayFactors");
        }

        [PatchPostfix]
        private static void Postfix(ProceduralWeaponAnimation __instance)
        {
            if (!Plugin.EnableMod.Value || __instance == null || __instance.MotionReact == null)
                return;

            var player = EFTBindings.GetPlayer(__instance);
            if (player == null || !player.IsYourPlayer) return;

            // Limpa interferência dos eixos X e Z nativos do jogo e reduz o Y
            Vector3 vanillaSway = __instance.MotionReact.SwayFactors;
            __instance.MotionReact.SwayFactors = new Vector3(0f, vanillaSway.y * 0.2f, 0f);
        }
    }

    // 5. Impacto e Recuo Visual de Disparo (Camera Punch & Weapon Kick)
    public class Patch_OnMakingShot : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), "OnMakingShot");
        }

        [PatchPostfix]
        private static void Postfix(Player __instance)
        {
            if (!Plugin.EnableMod.Value || __instance == null || !__instance.IsYourPlayer)
                return;

            bool isAiming = __instance.HandsController != null && __instance.HandsController.IsAiming;
            KineticSpringEngine.ApplyShotPunch(isAiming);
        }
    }
}
