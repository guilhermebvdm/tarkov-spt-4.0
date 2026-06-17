using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using EFT.Animations;
using HarmonyLib;
using Comfort.Common;
using EFT;

namespace CameraRotationMod.Patches
{
    /// <summary>
    /// Patch Spring.Get() to add our custom offset to the return value with smooth transitions
    /// Handles both ADS transitions and Stance toggling with framerate-independent interpolation
    /// Uses Unity's SmoothDamp    /// Permite adicionar offsets dinâmicos gerados pela transição das Stances para todos os jogadores.
    /// </summary>
    public class SpringGetPatch : ModulePatch
    {
        // Cache dictionary para mapear Springs -> Controladores dos jogadores (para Bots e Fika)
        private static readonly System.Collections.Generic.Dictionary<Spring, PlayerStanceController> _playerSprings = 
            new System.Collections.Generic.Dictionary<Spring, PlayerStanceController>();

        // Fallback for MainPlayer
        private static Spring _cachedHandsRotation;
        private static Spring _cachedHandsPosition;

        public static void RegisterPlayerSprings(EFT.Player player, Spring rot, Spring pos, PlayerStanceController controller)
        {
            if (rot != null) _playerSprings[rot] = controller;
            if (pos != null) _playerSprings[pos] = controller;
        }

        public static void UnregisterPlayerSprings(Spring rot, Spring pos)
        {
            if (rot != null) _playerSprings.Remove(rot);
            if (pos != null) _playerSprings.Remove(pos);
        }

        public static void ClearCache()
        {
            _cachedHandsRotation = null;
            _cachedHandsPosition = null;
            _playerSprings.Clear();
        }
        private static Stance _previousStance = Stance.Default;
        
        // ADS Transition Flags
        private static bool _isAdsTransition = false;
        private static bool _isEnteringAds = false;

        private static bool _isPlayingTransitionCurve = false;
        private static float _transitionCurveTimer = 0f;
        public static Vector3 CurrentCurveRotation = Vector3.zero;
        public static Vector3 CurrentCurvePosition = Vector3.zero;

        private static Vector3 _startTransitionRotation = Vector3.zero;
        private static Vector3 _startTransitionPosition = Vector3.zero;

        public static void TriggerTransitionCurve()
        {
            _isPlayingTransitionCurve = true;
            _transitionCurveTimer = 0f;
            _startTransitionRotation = _currentRotation;
            _startTransitionPosition = _currentPosition;
        }

        // Smooth interpolation for transitions using SmoothDamp
        private static Vector3 _currentRotation = Vector3.zero;
        private static Vector3 _targetRotation = Vector3.zero;
        private static Vector3 _rotationVelocity = Vector3.zero;
        
        private static Vector3 _currentPosition = Vector3.zero;
        private static Vector3 _targetPosition = Vector3.zero;
        private static Vector3 _positionVelocity = Vector3.zero;
        
        // Track if we're in a stable state (at target with no transitions)
        private static bool _isStable = false;
        private static bool _wasStable = false;
        
        private static bool _wasAiming = false;
        private static bool _wasInStance = false;
        private static bool _wasHoldingFirearm = false;
        private static bool _isInitialized = false;
        private static Stance _stanceBeforeAds = Stance.Default;

        
        /// <summary>
        /// Reset all state - called when entering new raid or GameWorld changes
        /// </summary>
        public static void ResetState()
        {
            _wasAiming = false;
            _wasInStance = false;
            _wasHoldingFirearm = false;
            _isInitialized = false;
            _previousStance = Stance.Default;
            _stanceBeforeAds = Stance.Default;
            _isAdsTransition = false;
            _isEnteringAds = false;
            _currentRotation = Vector3.zero;
            _targetRotation = Vector3.zero;
            _rotationVelocity = Vector3.zero;
            _currentPosition = Vector3.zero;
            _targetPosition = Vector3.zero;
            _positionVelocity = Vector3.zero;
            _startTransitionRotation = Vector3.zero;
            _startTransitionPosition = Vector3.zero;
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
            // Tenta pegar o controlador a partir do dicionário (multiplayer/bots)
            if (_playerSprings.TryGetValue(__instance, out var controller))
            {
                if (controller != null)
                {
                    // Determinar se é posição ou rotação checando a velocidade da spring.
                    // Isso é uma aproximação segura para os controladores extras,
                    // já que registramos pos e rot em PlayerStanceController.
                    bool isControllerRot = controller.IsRotationSpring(__instance);
                    
                    if (isControllerRot)
                    {
                        __result = controller.GetRotationOffset() + __instance.Current;
                    }
                    else
                    {
                        __result = controller.GetPositionOffset() + __instance.Current;
                    }
                    return; // Retorno antecipado para não processar a lógica do MainPlayer
                }
            }

            // FAST early exit: se não estiver no dicionário e nem no cache do MainPlayer
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

            bool isMainPlayerRot = (__instance == _cachedHandsRotation);
            bool isMainPlayerPos = (__instance == _cachedHandsPosition);

            if (!isMainPlayerRot && !isMainPlayerPos)
                return;

            bool isRotationSpring = isMainPlayerRot;
            bool isPositionSpring = isMainPlayerPos;

            
            bool isAiming = pwa.IsAiming;
            bool isHoldingFirearm = StanceManager.IsHoldingFirearm();
            Stance currentStance = StanceManager.CurrentStance;

            // A transição natural do EFT para mira (ADS) e a nossa transição de Stance 
            // ocorrem ao longo de vários frames. Aplicamos a velocidade/offset de forma contínua.
            bool isTransitioning = _isPlayingTransitionCurve;

            // FAST PATH: If we're stable (at target with no active transitions) and no state changed,
            // we can skip all the expensive calculations and just apply cached values directly
            // Use firearm-aware isInStance to match the actual transition logic below
            bool isInStanceFull = isHoldingFirearm && StanceManager.IsInStance;
            bool stateChanged = (isAiming != _wasAiming) ||
                               (isInStanceFull != _wasInStance) ||
                               (currentStance != _previousStance) ||
                               (isHoldingFirearm != _wasHoldingFirearm);
            
            const float epsilon = 1e-6f;

            if (_isStable && _isInitialized && !stateChanged && !_isPlayingTransitionCurve)
            {
                // Apply cached values directly - skip all transition logic
                if (isRotationSpring)
                {
                    __result = _currentRotation + __instance.Current;
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
            bool advancedAdsEnabled = Plugin._EnableAdvancedADSTransitions?.Value ?? true;
            
            // Check if ANY feature is enabled that could potentially affect this spring
            // Stances are always enabled when in stance mode, so check if we're in a stance
            bool isInAnyStance = StanceManager.IsInStance;
            bool anyRotationFeatureEnabled = isInAnyStance || resetOnADSEnabled || advancedAdsEnabled || _isPlayingTransitionCurve || stateChanged;
            bool anyPositionFeatureEnabled = isInAnyStance || defaultPositionEnabled || resetOnADSEnabled || advancedAdsEnabled || _isPlayingTransitionCurve || stateChanged;
            
            // Early exit only if NO features are enabled at all
            if (isRotationSpring && !anyRotationFeatureEnabled)
                return;
            if (isPositionSpring && !anyPositionFeatureEnabled)
                return;
                
            // Update target values when state changes
            if (stateChanged)
            {
                _isStable = false;
            }

            // Check if player is holding a firearm - if not, force Default stance
            bool isInStance = isHoldingFirearm && StanceManager.IsInStance;

            // Use StanceManager to get target values based on current state
            // StanceManager handles: Default <-> Stance <-> ADS transitions
            // If not holding firearm, always use Default (zero) values
            Vector3 desiredRotation = isHoldingFirearm ? StanceManager.GetTargetRotation(isAiming) : Vector3.zero;
            Vector3 desiredPosition = isHoldingFirearm ? StanceManager.GetTargetPosition(isAiming) : Vector3.zero;

            float transitionSpeed;
            if (isAiming)
            {
                transitionSpeed = Plugin._ADSShoulderSettleSpeed?.Value ?? 0.15f;
            }
            else
            {
                transitionSpeed = Plugin._StanceTransitionSpeed?.Value ?? 1f;
            }
            
            // Recompute stateChanged
            bool stateChangedFull = (isAiming != _wasAiming) || (isInStance != _wasInStance) || (isHoldingFirearm != _wasHoldingFirearm) || (currentStance != _previousStance);
            
            if (stateChangedFull)
            {
                // Play stance change sound
                if ((currentStance != _previousStance || isInStance != _wasInStance) && isHoldingFirearm && !isAiming)
                {
                    PlayStanceChangeSound(gameWorld.MainPlayer);
                }
                
                // Track ads transition vs stance transition
                if (isAiming != _wasAiming)
                {
                    if (isAiming) 
                    {
                        // Se o player apertou mirar, gravamos qual Stance ele estava ANTES de mirar
                        _stanceBeforeAds = _wasInStance ? _previousStance : Stance.Default;
                    }

                    _isAdsTransition = true;
                    _isEnteringAds = isAiming;
                    _isPlayingTransitionCurve = true;
                    _transitionCurveTimer = 0f;
                    _startTransitionRotation = _currentRotation;
                    _startTransitionPosition = _currentPosition;
                }
                else if (isInStance != _wasInStance || currentStance != _previousStance)
                {
                    _isAdsTransition = false;
                    _isPlayingTransitionCurve = true;
                    _transitionCurveTimer = 0f;
                    _startTransitionRotation = _currentRotation;
                    _startTransitionPosition = _currentPosition;
                }
                
                _previousStance = currentStance;
            }
            
            // Convert transition speed to SmoothDamp smoothTime
            float smoothTime = SpeedToSmoothTime(transitionSpeed);
            
            // ALWAYS update targets to match desired state
            _targetRotation = desiredRotation;
            _targetPosition = desiredPosition;
            
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

                // Variaveis de tracking atualizadas acima

                _wasAiming = isAiming;
                _wasInStance = isInStance;
                _wasHoldingFirearm = isHoldingFirearm;
            }
            float deltaTime = Time.deltaTime;
            float normalDamping = Plugin._StanceTransitionDamping?.Value ?? 1.0f;
            float angularFreq = SpringMath.SmoothTimeToAngularFrequency(smoothTime);

            if (_isPlayingTransitionCurve)
            {
                _transitionCurveTimer += deltaTime;
                float duration = Plugin._CurveDuration?.Value ?? 0.35f;
                float progress = Mathf.Clamp01(_transitionCurveTimer / duration);

                // Base offset Lerp
                float damping;
                if (_isAdsTransition) {
                    if (_stanceBeforeAds == Stance.Stance1) damping = Plugin._Stance1ADSOvershootDamping?.Value ?? 1.0f;
                    else if (_stanceBeforeAds == Stance.Stance2) damping = Plugin._Stance2ADSOvershootDamping?.Value ?? 1.0f;
                    else damping = Plugin._Stance0ADSOvershootDamping?.Value ?? 1.0f;
                } else {
                    damping = Plugin._StanceOvershootDamping?.Value ?? 1.0f;
                }
                // If damping is 1.0, amplitude is 0. If damping is 0.4, amplitude is 0.6.
                float amplitude = 1.0f - damping;
                float frequency = 3f;
                float easedProgress;
                float proceduralYKick = 0f;

                // Base EaseInOut for the whole transition
                float baseEase = progress * progress * (3f - 2f * progress);

                if (amplitude > 0f && progress >= 0.5f)
                {
                    // Map 0.5->1.0 to 0.0->1.0 for the overshoot wave
                    float tailProgress = (progress - 0.5f) * 2f;
                    
                    // Starts at 0 (sin(0)=0), oscillates, decays to 0 at tailProgress=1.0
                    float decay = 1f - tailProgress;
                    float wave = Mathf.Sin(tailProgress * Mathf.PI * frequency);
                    
                    float overshoot = amplitude * decay * wave;
                    
                    // Add overshoot to the base Lerp progress
                    easedProgress = baseEase + overshoot;
                    
                    // Extract this wave to physically kick the Y axis
                    proceduralYKick = overshoot;
                }
                else
                {
                    easedProgress = baseEase;
                }

                _currentRotation = Vector3.LerpUnclamped(_startTransitionRotation, _targetRotation, easedProgress);
                _currentPosition = Vector3.LerpUnclamped(_startTransitionPosition, _targetPosition, easedProgress);

                Vector3 impactRot;
                Vector3 impactPos;

                float pitchMult = 1.0f;
                float rollMult = 1.0f;
                float yawMult = 1.0f;
                float posYMult = 1.0f;
                float posZMult = 1.0f;
                float posXMult = 1.0f;

                if (_isAdsTransition)
                {
                    impactRot = shwngFpsCameraStances.StanceTransitionCurves.EvaluateADSRotation(progress, _isEnteringAds, _stanceBeforeAds);
                    impactPos = shwngFpsCameraStances.StanceTransitionCurves.EvaluateADSPosition(progress, _isEnteringAds, _stanceBeforeAds);
                    
                    if (_stanceBeforeAds == Stance.Stance1) {
                        pitchMult = Plugin._Stance1ADSPitchMultiplier?.Value ?? 1.0f;
                        rollMult = Plugin._Stance1ADSRollMultiplier?.Value ?? 1.0f;
                        yawMult = Plugin._Stance1ADSYawMultiplier?.Value ?? 1.0f;
                        posYMult = Plugin._Stance1ADSPosYMultiplier?.Value ?? 1.0f;
                        posZMult = Plugin._Stance1ADSPosZMultiplier?.Value ?? 1.0f;
                    } else if (_stanceBeforeAds == Stance.Stance2) {
                        pitchMult = Plugin._Stance2ADSPitchMultiplier?.Value ?? 1.0f;
                        rollMult = Plugin._Stance2ADSRollMultiplier?.Value ?? 1.0f;
                        yawMult = Plugin._Stance2ADSYawMultiplier?.Value ?? 1.0f;
                        posYMult = Plugin._Stance2ADSPosYMultiplier?.Value ?? 1.0f;
                        posZMult = Plugin._Stance2ADSPosZMultiplier?.Value ?? 1.0f;
                    } else {
                        pitchMult = Plugin._Stance0ADSPitchMultiplier?.Value ?? 1.0f;
                        rollMult = Plugin._Stance0ADSRollMultiplier?.Value ?? 1.0f;
                        yawMult = Plugin._Stance0ADSYawMultiplier?.Value ?? 1.0f;
                        posYMult = Plugin._Stance0ADSPosYMultiplier?.Value ?? 1.0f;
                        posZMult = Plugin._Stance0ADSPosZMultiplier?.Value ?? 1.0f;
                    }
                }
                else
                {
                    impactRot = shwngFpsCameraStances.StanceTransitionCurves.EvaluateRotation(progress, _previousStance, currentStance);
                    impactPos = shwngFpsCameraStances.StanceTransitionCurves.EvaluatePosition(progress, _previousStance, currentStance);
                    
                    pitchMult = Plugin._StanceCurvePitchMultiplier?.Value ?? 1.0f;
                    rollMult = Plugin._StanceCurveRollMultiplier?.Value ?? 1.0f;
                    yawMult = Plugin._StanceCurveYawMultiplier?.Value ?? 1.0f;
                    posYMult = Plugin._StanceCurvePositionMultiplier?.Value ?? 1.0f;
                    posZMult = Plugin._StanceCurvePositionMultiplier?.Value ?? 1.0f;
                    posXMult = Plugin._StanceCurvePositionMultiplier?.Value ?? 1.0f;
                }

                // Em impactRot: X é Pitch, Y é o Roll, Z é Yaw.
                CurrentCurveRotation = new Vector3(impactRot.x * pitchMult, impactRot.y * rollMult, impactRot.z * yawMult);
                CurrentCurvePosition = new Vector3(impactPos.x * posXMult, impactPos.y * posYMult, impactPos.z * posZMult);

                if (progress >= 1.0f)
                {
                    _isPlayingTransitionCurve = false;
                    _currentRotation = _targetRotation;
                    _currentPosition = _targetPosition;
                }
            }
            else
            {
                _currentRotation = SpringMath.SpringDamp(_currentRotation, _targetRotation, ref _rotationVelocity, normalDamping, angularFreq, deltaTime);
                _currentPosition = SpringMath.SpringDamp(_currentPosition, _targetPosition, ref _positionVelocity, normalDamping, angularFreq, deltaTime);
                CurrentCurveRotation = Vector3.zero;
                CurrentCurvePosition = Vector3.zero;
            }
            
            // Track stability for potential early exit optimization in future frames
            // Use small epsilon for approximate comparison since SmoothDamp converges asymptotically
            bool atRotationTarget = (_currentRotation - _targetRotation).sqrMagnitude < epsilon;
            bool atPositionTarget = (_currentPosition - _targetPosition).sqrMagnitude < epsilon;
            
            _wasStable = _isStable;
            _isStable = atRotationTarget && atPositionTarget && !_isPlayingTransitionCurve;

            // Apply the interpolated values based on which spring this is
            if (isRotationSpring)
            {
                __result = _currentRotation + CurrentCurveRotation + __instance.Current;
            }
            else if (isPositionSpring)
            {
                __result = _currentPosition + CurrentCurvePosition + __instance.Current;
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
