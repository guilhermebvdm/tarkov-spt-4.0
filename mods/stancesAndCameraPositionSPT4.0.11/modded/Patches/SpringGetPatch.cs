using SPT.Reflection.Patching;
using EFT.Animations;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using Comfort.Common;
using EFT;

namespace CameraRotationMod.Patches
{
    /// <summary>
    /// Patch Spring.Get() to add our custom offset to the return value with smooth transitions
    /// Handles both ADS transitions and Stance toggling with framerate-independent interpolation
    /// Uses Unity's SmoothDamp for guaranteed stability at any frame rate
    /// </summary>
    public class SpringGetPatch : ModulePatch
    {
        // Track transition states
        private static bool _wasAiming = false;
        private static bool _wasInStance = false;
        private static bool _wasHoldingFirearm = false;
        private static bool _isInitialized = false;
        
        // Advanced ADS Transition (Shouldering) state
        private static bool _isInShoulderingPhase = false;
        private static float _shoulderingStartTime = 0f;
        
        // Stance transition shouldering state
        private static bool _isInStanceShoulderingPhase = false;
        private static float _stanceShoulderingStartTime = 0f;
        private static Stance _previousStance = Stance.Default;
        
        // Smooth interpolation for transitions using SmoothDamp
        private static Vector3 _currentRotation = Vector3.zero;
        private static Vector3 _targetRotation = Vector3.zero;
        private static Vector3 _rotationVelocity = Vector3.zero;
        
        private static Vector3 _currentPosition = Vector3.zero;
        private static Vector3 _targetPosition = Vector3.zero;
        private static Vector3 _positionVelocity = Vector3.zero;
        
        // Shouldering rotation offset (separate from the main rotation target)
        private static Vector3 _currentShoulderingRotation = Vector3.zero;
        private static Vector3 _targetShoulderingRotation = Vector3.zero;
        private static Vector3 _shoulderingRotationVelocity = Vector3.zero;
        
        // Track if we're in a stable state (at target with no transitions)
        private static bool _isStable = false;
        private static bool _wasStable = false;
        
        // Cached spring references to avoid property lookups on every Spring.Get() call
        private static Spring _cachedHandsRotation = null;
        private static Spring _cachedHandsPosition = null;
        
        /// <summary>
        /// Reset all state - called when entering new raid or GameWorld changes
        /// </summary>
        public static void ResetState()
        {
            _wasAiming = false;
            _wasInStance = false;
            _wasHoldingFirearm = false;
            _isInitialized = false;
            _isInShoulderingPhase = false;
            _shoulderingStartTime = 0f;
            _isInStanceShoulderingPhase = false;
            _stanceShoulderingStartTime = 0f;
            _previousStance = Stance.Default;
            _currentRotation = Vector3.zero;
            _targetRotation = Vector3.zero;
            _rotationVelocity = Vector3.zero;
            _currentPosition = Vector3.zero;
            _targetPosition = Vector3.zero;
            _positionVelocity = Vector3.zero;
            _currentShoulderingRotation = Vector3.zero;
            _targetShoulderingRotation = Vector3.zero;
            _shoulderingRotationVelocity = Vector3.zero;
            _isStable = false;
            _wasStable = false;
            _cachedHandsRotation = null;
            _cachedHandsPosition = null;
        }
        
        /// <summary>
        /// Validate that cached Spring references still match the current player's HandsContainer.
        /// Must be called once per frame (from Plugin.Update) BEFORE any Spring.Get() calls,
        /// so the fast-exit path in PatchPostfix doesn't silently reject new Spring instances
        /// after transit, weapon swap, or player recreation.
        /// </summary>
        public static void ValidateSpringCache()
        {
            // Nothing to validate if cache is already empty
            if (_cachedHandsRotation == null)
                return;
            
            var gameWorld = StanceManager.GetCachedGameWorld();
            if (gameWorld?.MainPlayer?.ProceduralWeaponAnimation?.HandsContainer == null)
            {
                // Player/PWA not available — clear cache so fast-exit won't block new springs
                _cachedHandsRotation = null;
                _cachedHandsPosition = null;
                return;
            }
            
            var container = gameWorld.MainPlayer.ProceduralWeaponAnimation.HandsContainer;
            
            // If the actual springs no longer match our cache, invalidate it.
            // PatchPostfix will re-cache on its next eligible call and detect
            // any GameWorld change that requires a full ResetState().
            if (_cachedHandsRotation != container.HandsRotation ||
                _cachedHandsPosition != container.HandsPosition)
            {
                _cachedHandsRotation = null;
                _cachedHandsPosition = null;
            }
        }
        
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Spring), nameof(Spring.Get));
        }
        
        /// <summary>
        /// Convert transition speed (user-facing) to SmoothDamp smoothTime (seconds)
        /// Higher speed = faster = lower smoothTime
        /// </summary>
        private static float SpeedToSmoothTime(float speed)
        {
            // Clamp speed to avoid division by zero and extreme values
            speed = Mathf.Clamp(speed, 0.5f, 20f);
            // Convert: speed 1 → 0.25s, speed 4 → 0.0625s, speed 12 → 0.02s
            return 0.25f / speed;
        }

        /// <summary>
        /// One-of-a-kind Spring.Get() hack
        /// </summary>
        [PatchPostfix]
        private static void PatchPostfix(Spring __instance, ref Vector3 __result)
        {
            // FAST early exit: if we have cached spring refs, reject non-hands springs immediately
            // This avoids GameWorld lookups for the majority of Spring.Get() calls
            if (_cachedHandsRotation != null && __instance != _cachedHandsRotation && __instance != _cachedHandsPosition)
                return;
            
            // Use cached GameWorld from StanceManager to avoid multiple Singleton lookups
            var gameWorld = StanceManager.GetCachedGameWorld();
            if (gameWorld?.MainPlayer?.ProceduralWeaponAnimation?.HandsContainer == null)
                return;

            var pwa = gameWorld.MainPlayer.ProceduralWeaponAnimation;
            
            // Cache the spring references so future calls can early-exit without GameWorld lookup
            if (_cachedHandsRotation == null)
            {
                _cachedHandsRotation = pwa.HandsContainer.HandsRotation;
                _cachedHandsPosition = pwa.HandsContainer.HandsPosition;
            }

            // Early exit if this spring is not one we care about (hands only, camera handled in PlayerSpringPatch)
            if (__instance != _cachedHandsRotation && __instance != _cachedHandsPosition)
                return;

            bool isRotationSpring = __instance == _cachedHandsRotation;
            bool isPositionSpring = __instance == _cachedHandsPosition;
            
            bool isAiming = pwa.IsAiming;
            bool isHoldingFirearm = StanceManager.IsHoldingFirearm();
            Stance currentStance = StanceManager.CurrentStance;
            
            // FAST PATH: If we're stable (at target with no active transitions) and no state changed,
            // we can skip all the expensive calculations and just apply cached values directly
            // Use firearm-aware isInStance to match the actual transition logic below
            bool isInStanceFull = isHoldingFirearm && StanceManager.IsInStance;
            bool stateChanged = (isAiming != _wasAiming) || 
                               (isInStanceFull != _wasInStance) || 
                               (currentStance != _previousStance) ||
                               (isHoldingFirearm != _wasHoldingFirearm);
            
            if (_isStable && _isInitialized && !stateChanged)
            {
                // Apply cached values directly - skip all transition logic
                if (isRotationSpring)
                {
                    __result = _currentRotation + _currentShoulderingRotation + __instance.Current;
                }
                else if (isPositionSpring)
                {
                    __result = _currentPosition + __instance.Current;
                }
                return;
            }
            
            // Check if any features are actually enabled
            bool resetOnADSEnabled = Plugin._ResetOnADS?.Value ?? false;
            bool defaultPositionEnabled = Plugin._DefaultHandsPositionEnabled?.Value ?? false;
            
            // Check if ANY feature is enabled that could potentially affect this spring
            // Stances are always enabled when in stance mode, so check if we're in a stance
            bool isInAnyStance = StanceManager.IsInStance;
            bool anyRotationFeatureEnabled = isInAnyStance || resetOnADSEnabled;
            bool anyPositionFeatureEnabled = isInAnyStance || defaultPositionEnabled || resetOnADSEnabled;
            
            // Early exit only if NO features are enabled at all
            if (isRotationSpring && !anyRotationFeatureEnabled)
                return;
            if (isPositionSpring && !anyPositionFeatureEnabled)
                return;

            // Check if player is holding a firearm - if not, force Default stance
            bool isInStance = isHoldingFirearm && StanceManager.IsInStance;

            // Use StanceManager to get target values based on current state
            // StanceManager handles: Default <-> Stance <-> ADS transitions
            // If not holding firearm, always use Default (zero) values
            Vector3 desiredRotation = isHoldingFirearm ? StanceManager.GetTargetRotation(isAiming) : Vector3.zero;
            Vector3 desiredPosition = isHoldingFirearm ? StanceManager.GetTargetPosition(isAiming) : Vector3.zero;

            // justStartedAiming is used for shouldering trigger
            bool justStartedAiming = isAiming && !_wasAiming;
            
            // Detect stance change (different from just entering/exiting stance)
            bool stanceChanged = currentStance != _previousStance;
            
            // Check if advanced ADS transitions are enabled EARLY to skip expensive calculations
            bool advancedADSEnabled = Plugin._EnableAdvancedADSTransitions?.Value ?? false;
            
            // Variables for transition speed and shouldering
            float transitionSpeed;
            Vector3 shoulderingPositionOffset = Vector3.zero;
            Vector3 shoulderingRotationOffset = Vector3.zero;
            
            // If advanced ADS is disabled, use simple transition speeds and skip all shouldering
            if (!advancedADSEnabled)
            {
                // Clear any active shouldering phases
                _isInShoulderingPhase = false;
                _isInStanceShoulderingPhase = false;
                
                // Simple transition speed - just use base config values
                if (isAiming)
                {
                    transitionSpeed = Plugin._ADSTransitionSpeed?.Value ?? 2f;
                }
                else
                {
                    transitionSpeed = Plugin._StanceTransitionSpeed?.Value ?? 1f;
                }
                
                // Play stance change sound (independent of Advanced ADS)
                if (stanceChanged && isHoldingFirearm && !isAiming)
                {
                    PlayStanceChangeSound(gameWorld.MainPlayer);
                }
                
                // Update previous stance tracker
                _previousStance = currentStance;
            }
            else
            {
                // ADVANCED ADS ENABLED - do full calculations
                bool scaleByWeaponStats = Plugin._ScaleByWeaponStats?.Value ?? true;
                
                // Get EFT's calculated AimingSpeed (based on weapon weight + ergonomics)
                float eftAimingSpeed = pwa.AimingSpeed;
                float scaleIntensity = Plugin._WeaponStatsScaleIntensity?.Value ?? 1f;
                
                // Calculate raw multipliers
                float rawAimingSpeedMultiplier = eftAimingSpeed;
                float rawInverseAimingSpeed = 1f / Mathf.Max(eftAimingSpeed, 0.5f);
                
                // Lerp between 1 (no effect) and full effect based on intensity
                float aimingSpeedMultiplier = scaleByWeaponStats ? Mathf.LerpUnclamped(1f, rawAimingSpeedMultiplier, scaleIntensity) : 1f;
                float inverseAimingSpeed = scaleByWeaponStats ? Mathf.LerpUnclamped(1f, rawInverseAimingSpeed, scaleIntensity) : 1f;
                
                // Get base config values
                float baseThrowDuration = Plugin._ADSShoulderThrowDuration?.Value ?? 0.15f;
                float baseThrowSpeed = Plugin._ADSShoulderThrowSpeed?.Value ?? 2f;
                float baseSettleSpeed = Plugin._ADSShoulderSettleSpeed?.Value ?? 1.5f;
                float baseThrowForward = Plugin._ADSShoulderThrowForward?.Value ?? 0.02f;
                float baseThrowUp = Plugin._ADSShoulderThrowUp?.Value ?? -0.015f;
                
                // Apply weapon stat scaling
                float throwDuration = baseThrowDuration * inverseAimingSpeed;
                float throwSpeed = baseThrowSpeed * aimingSpeedMultiplier;
                float settleSpeed = baseSettleSpeed * aimingSpeedMultiplier;
                float throwForward = baseThrowForward * inverseAimingSpeed;
                float throwUp = baseThrowUp * inverseAimingSpeed;
                
                // Start shouldering phase when entering ADS
                if (justStartedAiming && isHoldingFirearm)
                {
                    _isInShoulderingPhase = true;
                    _shoulderingStartTime = Time.time;
                }
                
                // End shouldering phase when no longer aiming or duration exceeded
                if (!isAiming)
                {
                    _isInShoulderingPhase = false;
                }
                else if (_isInShoulderingPhase && Time.time - _shoulderingStartTime >= throwDuration)
                {
                    _isInShoulderingPhase = false;
                }
                
                // Stance transition shouldering effect
                bool affectStanceTransition = Plugin._AffectStanceTransitionToo?.Value ?? true;
                float stanceTransitionIntensity = Plugin._AdvancedStanceTransitionIntensity?.Value ?? 1f;
                
                // Calculate stance-specific values only if stance transitions are affected
                float stanceAimingSpeedMultiplier = 1f;
                float stanceInverseAimingSpeed = 1f;
                float stanceThrowDuration = baseThrowDuration;
                float stanceThrowSpeed = baseThrowSpeed;
                float stanceThrowForward = baseThrowForward;
                float stanceThrowUp = baseThrowUp;
                
                if (affectStanceTransition)
                {
                    stanceAimingSpeedMultiplier = Mathf.LerpUnclamped(1f, rawAimingSpeedMultiplier, stanceTransitionIntensity);
                    stanceInverseAimingSpeed = Mathf.LerpUnclamped(1f, rawInverseAimingSpeed, stanceTransitionIntensity);
                    stanceThrowDuration = baseThrowDuration * stanceInverseAimingSpeed;
                    stanceThrowSpeed = baseThrowSpeed * stanceAimingSpeedMultiplier;
                    stanceThrowForward = baseThrowForward * stanceInverseAimingSpeed;
                    stanceThrowUp = baseThrowUp * stanceInverseAimingSpeed;
                }
                
                // Get rotation throw config values (shared between ADS and stance)
                float baseThrowYaw = Plugin._ADSShoulderThrowYaw?.Value ?? 0f;
                float baseThrowPitch = Plugin._ADSShoulderThrowPitch?.Value ?? 0f;
                float baseThrowRoll = Plugin._ADSShoulderThrowRoll?.Value ?? 0f;
                
                // Calculate rotation throws
                float throwYaw = baseThrowYaw * inverseAimingSpeed;
                float throwPitch = baseThrowPitch * inverseAimingSpeed;
                float throwRoll = baseThrowRoll * inverseAimingSpeed;
                float stanceThrowYaw = affectStanceTransition ? baseThrowYaw * stanceInverseAimingSpeed : 0f;
                float stanceThrowPitch = affectStanceTransition ? baseThrowPitch * stanceInverseAimingSpeed : 0f;
                float stanceThrowRoll = affectStanceTransition ? baseThrowRoll * stanceInverseAimingSpeed : 0f;
                
                // Start stance shouldering phase when changing stances (and not aiming)
                if (stanceChanged && affectStanceTransition && isHoldingFirearm && !isAiming)
                {
                    _isInStanceShoulderingPhase = true;
                    _stanceShoulderingStartTime = Time.time;
                }
                
                // Play stance change sound (independent of Advanced ADS - but only in this branch to avoid duplicate)
                if (stanceChanged && isHoldingFirearm && !isAiming)
                {
                    PlayStanceChangeSound(gameWorld.MainPlayer);
                }
                
                // End stance shouldering phase when duration exceeded or started aiming
                if (isAiming)
                {
                    _isInStanceShoulderingPhase = false;
                }
                else if (_isInStanceShoulderingPhase && Time.time - _stanceShoulderingStartTime >= stanceThrowDuration)
                {
                    _isInStanceShoulderingPhase = false;
                }
                
                // Update previous stance tracker
                _previousStance = currentStance;
                
                // Calculate transition speed
                if (_isInShoulderingPhase)
                {
                    transitionSpeed = throwSpeed;
                }
                else if (_isInStanceShoulderingPhase)
                {
                    transitionSpeed = stanceThrowSpeed;
                }
                else if (isAiming)
                {
                    transitionSpeed = settleSpeed;
                }
                else
                {
                    float baseStanceSpeed = Plugin._StanceTransitionSpeed?.Value ?? 1f;
                    transitionSpeed = affectStanceTransition ? (baseStanceSpeed * stanceAimingSpeedMultiplier) : baseStanceSpeed;
                }
                
                // Get overall throw intensity multipliers
                float adsThrowIntensity = Plugin._ADSShoulderThrowIntensity?.Value ?? 1f;
                float stanceThrowIntensity = Plugin._StanceShoulderThrowIntensity?.Value ?? 1f;
                
                // Apply shouldering offsets
                if (_isInShoulderingPhase)
                {
                    shoulderingPositionOffset = new Vector3(0f, throwUp * adsThrowIntensity, throwForward * adsThrowIntensity);
                    shoulderingRotationOffset = new Vector3(throwPitch * adsThrowIntensity, throwYaw * adsThrowIntensity, throwRoll * adsThrowIntensity);
                }
                else if (_isInStanceShoulderingPhase)
                {
                    shoulderingPositionOffset = new Vector3(0f, stanceThrowUp * stanceThrowIntensity, stanceThrowForward * stanceThrowIntensity);
                    shoulderingRotationOffset = new Vector3(stanceThrowPitch * stanceThrowIntensity, stanceThrowYaw * stanceThrowIntensity, stanceThrowRoll * stanceThrowIntensity);
                }
            }
            
            // Convert transition speed to SmoothDamp smoothTime
            float smoothTime = SpeedToSmoothTime(transitionSpeed);
            
            // ALWAYS update targets to match desired state (for real-time slider adjustments)
            _targetRotation = desiredRotation;
            _targetPosition = desiredPosition + shoulderingPositionOffset;
            _targetShoulderingRotation = shoulderingRotationOffset;
            
            // Recompute stateChanged with proper isInStance (includes firearm check)
            bool stateChangedFull = (isAiming != _wasAiming) || (isInStance != _wasInStance) || (isHoldingFirearm != _wasHoldingFirearm);
            
            if (stateChangedFull)
            {
                // First time initialization - start at target
                if (!_isInitialized)
                {
                    _currentRotation = desiredRotation;
                    _currentPosition = desiredPosition;
                    _isInitialized = true;
                }
                // else: State changed but already initialized
                // SmoothDamp will smoothly transition, reset velocities for cleaner start
                _rotationVelocity = Vector3.zero;
                _positionVelocity = Vector3.zero;
                
                _wasAiming = isAiming;
                _wasInStance = isInStance;
                _wasHoldingFirearm = isHoldingFirearm;
            }
            
            float deltaTime = Time.deltaTime;
            
            // Use Unity's SmoothDamp - mathematically stable at any frame rate
            _currentRotation = Vector3.SmoothDamp(_currentRotation, _targetRotation, ref _rotationVelocity, smoothTime, Mathf.Infinity, deltaTime);
            _currentPosition = Vector3.SmoothDamp(_currentPosition, _targetPosition, ref _positionVelocity, smoothTime, Mathf.Infinity, deltaTime);
            _currentShoulderingRotation = Vector3.SmoothDamp(_currentShoulderingRotation, _targetShoulderingRotation, ref _shoulderingRotationVelocity, smoothTime, Mathf.Infinity, deltaTime);
            
            // Track stability for potential early exit optimization in future frames
            // Use small epsilon for approximate comparison since SmoothDamp converges asymptotically
            const float epsilon = 1e-6f;
            bool atRotationTarget = (_currentRotation - _targetRotation).sqrMagnitude < epsilon;
            bool atPositionTarget = (_currentPosition - _targetPosition).sqrMagnitude < epsilon;
            bool atShoulderingTarget = (_currentShoulderingRotation - _targetShoulderingRotation).sqrMagnitude < epsilon;
            _wasStable = _isStable;
            _isStable = atRotationTarget && atPositionTarget && atShoulderingTarget && 
                       !_isInShoulderingPhase && !_isInStanceShoulderingPhase;

            // Apply the interpolated values based on which spring this is
            if (isRotationSpring)
            {
                // Add both the stance/ADS rotation AND the shouldering rotation offset
                __result = _currentRotation + _currentShoulderingRotation + __instance.Current;
            }
            else if (isPositionSpring)
            {
                __result = _currentPosition + __instance.Current;
            }
        }
        
        /// <summary>
        /// Play the aim rattle sound when changing stances (same sound as when ADS)
        /// </summary>
        private static void PlayStanceChangeSound(Player player)
        {
            if (player?.HandsController is Player.FirearmController fc)
            {
                var soundPlayer = fc.WeaponSoundPlayer;
                if (soundPlayer != null)
                {
                    // Get user-configured volume multiplier (0 = mute, 1 = normal, 2 = louder)
                    float volumeMultiplier = Plugin._StanceChangeSoundVolume?.Value ?? 1f;
                    if (volumeMultiplier <= 0f) return; // Skip if muted
                    
                    // Calculate volume similar to EFT's CalculateAimingSoundVolume
                    // TotalErgonomics / 100 - 1, clamped and scaled
                    float ergo = fc.TotalErgonomics / 100f - 1f;
                    float volume = Mathf.Clamp(ergo * ergo, 0.1f, 0.2f);
                    
                    // Apply covert movement modifier if available
                    if (player.MovementContext != null)
                    {
                        volume *= player.MovementContext.CovertEquipmentNoise;
                    }
                    
                    // Apply user volume multiplier
                    volume *= volumeMultiplier;
                    
                    soundPlayer.PlayAimingSound(volume);
                }
            }
        }
        
        /// <summary>
        /// Plays the aim rattle sound effect when switching stances.
        /// </summary>
    }
}
