using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace CameraRotationMod.Patches
{
    public class ApplyComplexRotationPatch : ModulePatch
    {
        private static FieldInfo _scopeRotationField;
        private static FieldInfo _weapTempPositionField;
        private static FieldInfo _weapTempRotationField;
        private static FieldInfo _isAimingField;
        private static FieldInfo _currentRotationField;
        private static FieldInfo _firearmControllerField;

        private static bool _wasAiming = false;
        private static Stance _lastStance = Stance.Default;
        private static float _adsKickTimer = 0f;
        private static bool _waitingForAdsKick = false;
        private static float _pendingKickAmount = 0f;
        private static float _kickSustainTimer = 0f;
        private static float _kickSustainAmountPerSec = 0f;

        private static void ApplyKick(float totalKickVelocity)
        {
            _kickSustainTimer = 0.1f; // Spread the kick over 100ms
            _kickSustainAmountPerSec = totalKickVelocity / 0.1f;
        }

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

            return typeof(EFT.Animations.ProceduralWeaponAnimation).GetMethod("ApplyComplexRotation", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void Postfix(EFT.Animations.ProceduralWeaponAnimation __instance, float dt)
        {
            Player.FirearmController firearmController = (Player.FirearmController)_firearmControllerField.GetValue(__instance);
            if (firearmController == null)
            {
                return;
            }

            Player player = Traverse.Create(firearmController).Field<Player>("_player").Value;
            if (player == null || !player.IsYourPlayer)
                return;

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
                Plugin.Logger.LogError($"[SPY-CRASH-PREVENTED] ApplyComplex: weapRotation (Tarkov _temporaryRotation) is (0,0,0,0)! Skipped frame to save camera.");
                return;
            }
            if (float.IsNaN(dt) || float.IsInfinity(dt) || dt <= 0f || dt > 1f)
            {
                Plugin.Logger.LogError($"[SPY-CRASH-PREVENTED] ApplyComplex: deltaTime is invalid ({dt})! Skipped frame.");
                return;
            }
            if (float.IsNaN(scopeRotation.x) || float.IsNaN(scopeRotation.y) || float.IsNaN(scopeRotation.z) || float.IsNaN(scopeRotation.w))
            {
                Plugin.Logger.LogError($"[SPY-CRASH-PREVENTED] ApplyComplex: scopeRotation has NaN components! Skipped frame.");
                return;
            }
            // ==========================================

            bool isInStance = StanceManager.IsInStance;
            Stance currentStance = StanceManager.CurrentStance;
            float kick = Plugin._StanceKickIntensity?.Value ?? -0.05f;
            float kickVelocity = kick * 30f; // Convert positional intensity to velocity impulse

            // Hold Breath Arm Stamina & Oxygen Drain
            if (player.Physical != null && player.Physical.HoldingBreath)
            {
                // Arm Stamina
                float drainAmount = Plugin._HoldBreathArmStaminaDrain.Value * dt;
                player.Physical.HandsStamina.Current -= drainAmount;
                if (player.Physical.HandsStamina.Current < 0f) 
                {
                    player.Physical.HandsStamina.Current = 0f;
                }

                // Oxygen
                if (player.Physical.Oxygen != null)
                {
                    float oxDrain = Plugin._HoldBreathOxygenDrain.Value * dt;
                    player.Physical.Oxygen.Current -= oxDrain;
                    if (player.Physical.Oxygen.Current < 0f)
                    {
                        player.Physical.Oxygen.Current = 0f;
                    }
                }
            }

            // Do not apply kick if player is prone
            bool isProne = player.IsInPronePose;

            if (!isProne)
            {
                if (isAiming != _wasAiming)
                {
                    if (isAiming) // Entering ADS
                    {
                        _waitingForAdsKick = true;
                        _adsKickTimer = Plugin._ADSKickDelay?.Value ?? 0.15f;
                        _pendingKickAmount = kickVelocity;
                    }
                    else // Exiting ADS
                    {
                        _waitingForAdsKick = false;
                        ApplyKick(kickVelocity); // Opposite visual effect when exiting since it's moving forward
                    }

                    _wasAiming = isAiming;
                }

                if (currentStance != _lastStance)
                {
                    // Stance kick is immediate
                    ApplyKick(kickVelocity);
                    _lastStance = currentStance;
                }

                if (_waitingForAdsKick)
                {
                    _adsKickTimer -= dt;
                    if (_adsKickTimer <= 0f)
                    {
                        _waitingForAdsKick = false;
                        ApplyKick(_pendingKickAmount);
                    }
                }

                // Apply sustained kick over time to avoid snap/teleport
                if (_kickSustainTimer > 0f)
                {
                    float t = Mathf.Min(_kickSustainTimer, dt);
                    _posVelocity.y += _kickSustainAmountPerSec * t;
                    _kickSustainTimer -= t;
                }
            }
            else
            {
                // If prone, just sync state without kicking
                _wasAiming = isAiming;
                _lastStance = currentStance;
                _waitingForAdsKick = false;
                _kickSustainTimer = 0f;
            }

            // Targets (Nunca usar scopeRotation.eulerAngles diretamente, pois algumas miras possuem valores extremos fixup que viram a câmera)
            Vector3 targetEuler = isInStance ? StanceManager.GetTargetRotation(isAiming) : Vector3.zero;
            Vector3 targetPosition = isAiming && !isInStance ? Vector3.zero : isInStance ? StanceManager.GetTargetPosition(isAiming) : Vector3.zero;

            // Spring Interpolation (Overshoot / Quicada)
            float speedMult = Plugin._StanceTransitionSpeed?.Value ?? 1f;
            float stiffness = 150f * speedMult;
            float damping = Plugin._StanceOvershootDamping?.Value ?? 12f; // Low damping = more quicada (overshoot)
            
            CurrentEuler = SpringLerpAngle(CurrentEuler, targetEuler, ref _rotVelocity, stiffness, damping, dt);
            CurrentPosition = SpringLerp(CurrentPosition, targetPosition, ref _posVelocity, stiffness, damping, dt);
            
            CurrentRotation = Quaternion.Euler(CurrentEuler);

            // [AUTO-SPY] Check if CurrentRotation got corrupted before applying
            if (float.IsNaN(CurrentRotation.x) || float.IsNaN(CurrentRotation.y) || float.IsNaN(CurrentRotation.z) || float.IsNaN(CurrentRotation.w) || 
               (CurrentRotation.x == 0 && CurrentRotation.y == 0 && CurrentRotation.z == 0 && CurrentRotation.w == 0))
            {
                Plugin.Logger.LogError($"[SPY-CRASH-PREVENTED] ApplyComplex: CurrentRotation generated an invalid Quaternion {CurrentRotation} from Euler {CurrentEuler}! Skipped apply.");
                CurrentEuler = Vector3.zero; // Reset to safe state
                return;
            }

            // Apply directly to WeaponRootAnim, ensuring the position offset is oriented correctly in the weapon's local space
            Vector3 orientedPositionOffset = weapRotation * CurrentPosition;
            __instance.HandsContainer.WeaponRootAnim.SetPositionAndRotation(weaponPosition + orientedPositionOffset, weapRotation * CurrentRotation);
            
            if (LogNextFrame)
            {
                Plugin.Logger.LogInfo($"[Spy-Complex] ApplyComplexRotation: isAiming={isAiming}, isInStance={isInStance}");
                Plugin.Logger.LogInfo($"[Spy-Complex] ApplyComplexRotation: targetEuler={targetEuler}, CurrentEuler={CurrentEuler}, rotVelocity={_rotVelocity}");
                Plugin.Logger.LogInfo($"[Spy-Complex] ApplyComplexRotation: targetPosition={targetPosition}, CurrentPosition={CurrentPosition}, posVelocity={_posVelocity}");
                Plugin.Logger.LogInfo($"[Spy-Complex] ApplyComplexRotation: speedMult={speedMult}, stiffness={stiffness}, damping={damping}, dt={dt}");
                LogNextFrame = false;
            }

        }
    }
}
