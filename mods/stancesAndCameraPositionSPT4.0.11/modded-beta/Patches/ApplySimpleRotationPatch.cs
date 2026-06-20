using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace CameraRotationMod.Patches
{
    public class ApplySimpleRotationPatch : ModulePatch
    {
        private static FieldInfo _scopeRotationField;
        private static FieldInfo _weapTempPositionField;
        private static FieldInfo _weapTempRotationField;
        private static FieldInfo _isAimingField;
        private static FieldInfo _currentRotationField;
        private static FieldInfo _firearmControllerField;

        public static Quaternion CurrentRotation = Quaternion.identity;
        public static Vector3 CurrentPosition = Vector3.zero;
        public static Vector3 CurrentEuler = Vector3.zero;
        public static bool LogNextFrame = false;
        private static Vector3 _rotVelocity;
        private static Vector3 _posVelocity;

        private static Vector3 SpringLerpAngle(Vector3 current, Vector3 target, ref Vector3 velocity, float stiffness, float damping, float dt)
        {
            if (float.IsNaN(target.x) || float.IsNaN(target.y) || float.IsNaN(target.z) || float.IsNaN(current.x)) return Vector3.zero;
            Vector3 diff = new Vector3(
                Mathf.DeltaAngle(current.x, target.x),
                Mathf.DeltaAngle(current.y, target.y),
                Mathf.DeltaAngle(current.z, target.z)
            );
            Vector3 force = diff * stiffness;
            velocity += force * dt;
            velocity *= Mathf.Clamp01(1f - damping * dt);
            Vector3 result = current + velocity * dt;
            if (float.IsNaN(result.x)) return Vector3.zero;
            return result;
        }

        private static Vector3 SpringLerp(Vector3 current, Vector3 target, ref Vector3 velocity, float stiffness, float damping, float dt)
        {
            if (float.IsNaN(target.x) || float.IsNaN(target.y) || float.IsNaN(target.z) || float.IsNaN(current.x)) return Vector3.zero;
            Vector3 force = (target - current) * stiffness;
            velocity += force * dt;
            velocity *= Mathf.Clamp01(1f - damping * dt);
            Vector3 result = current + velocity * dt;
            if (float.IsNaN(result.x)) return Vector3.zero;
            return result;
        }

        protected override MethodBase GetTargetMethod()
        {
            _scopeRotationField = AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "_targetScopeRotation");
            _weapTempPositionField = AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "_temporaryPosition");
            _weapTempRotationField = AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "_temporaryRotation");
            _isAimingField = AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "_isAiming");
            _currentRotationField = AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "_cameraIdenity");
            _firearmControllerField = AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "_firearmController");

            return typeof(EFT.Animations.ProceduralWeaponAnimation).GetMethod("ApplySimpleRotation", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void Postfix(EFT.Animations.ProceduralWeaponAnimation __instance, float dt)
        {
            if (LogNextFrame)
            {
                Plugin.Logger.LogInfo($"[Spy-Top] Postfix reached! Checking early returns...");
            }

            Player.FirearmController firearmController = (Player.FirearmController)_firearmControllerField.GetValue(__instance);
            if (firearmController == null)
            {
                if (LogNextFrame) Plugin.Logger.LogInfo($"[Spy-EarlyReturn] firearmController == null");
                return;
            }

            Player player = Traverse.Create(firearmController).Field<Player>("_player").Value;
            if (player == null || !player.IsYourPlayer)
            {
                if (LogNextFrame) Plugin.Logger.LogInfo($"[Spy-EarlyReturn] Not local player");
                return;
            }

            Quaternion scopeRotation = (Quaternion)_scopeRotationField.GetValue(__instance);
            Vector3 weaponPosition = (Vector3)_weapTempPositionField.GetValue(__instance);
            Quaternion weapRotation = (Quaternion)_weapTempRotationField.GetValue(__instance);
            bool isAiming = (bool)_isAimingField.GetValue(__instance);

            // ==========================================
            // [AUTO-SPY & SAFEGUARD]
            // Previne a câmera de virar de ponta cabeça
            // ==========================================
            if (weapRotation.w == 0 && weapRotation.x == 0 && weapRotation.y == 0 && weapRotation.z == 0)
            {
                Plugin.Logger.LogError($"[SPY-CRASH-PREVENTED] ApplySimple: weapRotation (Tarkov _temporaryRotation) is (0,0,0,0)! Skipped frame to save camera.");
                return;
            }
            if (float.IsNaN(dt) || float.IsInfinity(dt) || dt <= 0f || dt > 1f)
            {
                Plugin.Logger.LogError($"[SPY-CRASH-PREVENTED] ApplySimple: deltaTime is invalid ({dt})! Skipped frame.");
                return;
            }
            if (float.IsNaN(scopeRotation.x) || float.IsNaN(scopeRotation.y) || float.IsNaN(scopeRotation.z) || float.IsNaN(scopeRotation.w))
            {
                Plugin.Logger.LogError($"[SPY-CRASH-PREVENTED] ApplySimple: scopeRotation has NaN components! Skipped frame.");
                return;
            }
            // ==========================================

            bool isInStance = StanceManager.IsInStance;

            // Targets (Nunca usar scopeRotation.eulerAngles diretamente, pois algumas miras possuem valores extremos fixup que viram a câmera)
            Vector3 targetEuler = isInStance ? StanceManager.GetTargetRotation(isAiming) : Vector3.zero;
            Vector3 targetPosition = isAiming && !isInStance ? Vector3.zero : isInStance ? StanceManager.GetTargetPosition(isAiming) : Vector3.zero;

            // Spring Interpolation (Overshoot / Quicada)
            float speedMult = Plugin._StanceTransitionSpeed?.Value ?? 1f;
            float stiffness = 150f * speedMult;
            float damping = 12f; // Low damping = more quicada (overshoot)
            
            CurrentEuler = SpringLerpAngle(CurrentEuler, targetEuler, ref _rotVelocity, stiffness, damping, dt);
            CurrentPosition = SpringLerp(CurrentPosition, targetPosition, ref _posVelocity, stiffness, damping, dt);
            
            CurrentRotation = Quaternion.Euler(CurrentEuler);

            // [AUTO-SPY] Check if CurrentRotation got corrupted before applying
            if (float.IsNaN(CurrentRotation.x) || float.IsNaN(CurrentRotation.y) || float.IsNaN(CurrentRotation.z) || float.IsNaN(CurrentRotation.w) || 
               (CurrentRotation.x == 0 && CurrentRotation.y == 0 && CurrentRotation.z == 0 && CurrentRotation.w == 0))
            {
                Plugin.Logger.LogError($"[SPY-CRASH-PREVENTED] ApplySimple: CurrentRotation generated an invalid Quaternion {CurrentRotation} from Euler {CurrentEuler}! Skipped apply.");
                CurrentEuler = Vector3.zero; // Reset to safe state
                return;
            }

            // Apply directly to WeaponRootAnim, ensuring the position offset is oriented correctly in the weapon's local space
            Vector3 orientedPositionOffset = weapRotation * CurrentPosition;
            __instance.HandsContainer.WeaponRootAnim.SetPositionAndRotation(weaponPosition + orientedPositionOffset, weapRotation * CurrentRotation);
            
            if (LogNextFrame)
            {
                Plugin.Logger.LogInfo($"[Spy] ApplySimpleRotation: isAiming={isAiming}, isInStance={isInStance}");
                Plugin.Logger.LogInfo($"[Spy] ApplySimpleRotation: targetEuler={targetEuler}, CurrentEuler={CurrentEuler}, rotVelocity={_rotVelocity}");
                Plugin.Logger.LogInfo($"[Spy] ApplySimpleRotation: targetPosition={targetPosition}, CurrentPosition={CurrentPosition}, posVelocity={_posVelocity}");
                Plugin.Logger.LogInfo($"[Spy] ApplySimpleRotation: speedMult={speedMult}, stiffness={stiffness}, damping={damping}, dt={dt}");
                LogNextFrame = false;
            }

        }
    }
}
