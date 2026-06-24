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
        // Advanced ADS Transition (Shouldering) state
        private static bool _isInShoulderingPhase = false;
        private static float _shoulderingStartTime = 0f;
        
        // Stance transition shouldering state
        private static bool _isInStanceShoulderingPhase = false;
        private static float _stanceShoulderingStartTime = 0f;
        private static Stance _previousStance = Stance.Default;

        // Item 009: wiggle por troca INTENCIONAL de stance (request consumível do StanceManager),
        // consumido 1x por frame (o postfix roda para a spring de rotação E de posição no mesmo frame).
        private static int _wiggleFrame = -1;
        private static bool _wiggleThisFrame;
        private static Stance _wiggleFrom = Stance.Default;
        private static Stance _wiggleTo = Stance.Default;
        
        // Double Wiggle state
        private static bool _waitingForDoubleWiggle = false;
        private static float _doubleWiggleExecutionTime = 0f;

        private static bool _isPlayingTransitionCurve = false;
        private static float _transitionCurveTimer = 0f;
        private static Vector3 _currentCurveRotation = Vector3.zero;
        private static Vector3 _currentCurvePosition = Vector3.zero;

        private static float _lastWeaponWeightFactor = 1f;

        // Independent Wiggle Spring
        public static Vector3 _currentWiggleRotation = Vector3.zero;
        private static Vector3 _wiggleRotationVelocity = Vector3.zero;
        private static Vector3 _currentWigglePosition = Vector3.zero;
        private static Vector3 _wigglePositionVelocity = Vector3.zero;

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
        
        private static bool _wasAiming = false;
        private static bool _wasInStance = false;
        private static bool _wasHoldingFirearm = false;
        private static bool _isInitialized = false;

        
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

            // Item 009: consumir o pedido de wiggle 1x por frame (postfix roda p/ rot e pos no mesmo frame).
            if (UnityEngine.Time.frameCount != _wiggleFrame)
            {
                _wiggleThisFrame = StanceManager.ConsumeWiggleRequest(out _wiggleFrom, out _wiggleTo);
                _wiggleFrame = UnityEngine.Time.frameCount;
            }

            // Safe double wiggle check that works for both rot and pos without double-decreasing
            bool executeDoubleWiggle = false;
            if (_waitingForDoubleWiggle && UnityEngine.Time.time >= _doubleWiggleExecutionTime)
            {
                executeDoubleWiggle = true;
            }

            // A transição natural do EFT para mira (ADS) e a nossa transição de Stance 
            // ocorrem ao longo de vários frames. Aplicamos a velocidade/offset de forma contínua.
            bool isTransitioning = _isInStanceShoulderingPhase || 
                                   _isInShoulderingPhase || 
                                   _wiggleThisFrame ||
                                   executeDoubleWiggle;

            // FAST PATH: If we're stable (at target with no active transitions) and no state changed,
            // we can skip all the expensive calculations and just apply cached values directly
            // Use firearm-aware isInStance to match the actual transition logic below
            bool isInStanceFull = isHoldingFirearm && StanceManager.IsInStance;
            bool stateChanged = (isAiming != _wasAiming) ||
                               (isInStanceFull != _wasInStance) ||
                               (currentStance != _previousStance) ||
                               (isHoldingFirearm != _wasHoldingFirearm) ||
                               _wiggleThisFrame;
            
            // Check if wiggle is active so we don't early exit
            const float epsilon = 1e-6f;
            bool isWiggleActive = _currentWiggleRotation.sqrMagnitude > epsilon || _currentWigglePosition.sqrMagnitude > epsilon;

            if (_isStable && _isInitialized && !stateChanged && !executeDoubleWiggle && !isWiggleActive)
            {
                // Apply cached values directly - skip all transition logic
                if (isRotationSpring)
                {
                    __result = _currentRotation + _currentShoulderingRotation + _currentWiggleRotation + __instance.Current;
                }
                else if (isPositionSpring)
                {
                    __result = _currentPosition + __instance.Current;
                }
                return;
            }
            
            // --- Item 009: Stance Wiggle Procedural ---
            float speedMultiplier = isAiming ? (Plugin._WiggleSpeedADS?.Value ?? 15f) : (Plugin._WiggleSpeedHipfire?.Value ?? 15f);
            
            if (_wiggleThisFrame && Plugin._EnableStanceWiggle?.Value == true)
            {
                Plugin.Logger.LogInfo($"[Spy] Executing Wiggle! To={_wiggleTo}, IsRotationSpring={isRotationSpring}");
                float weaponWeight = 3.0f;
                if (gameWorld.MainPlayer != null && gameWorld.MainPlayer.HandsController is Player.FirearmController fcWiggle && fcWiggle.Item != null)
                {
                    weaponWeight = fcWiggle.Item.Weight;
                }

                float weightFactor = Mathf.Clamp(weaponWeight / 4.0f, 0.5f, 3.0f);
                _lastWeaponWeightFactor = weightFactor; // Save for double wiggle

                float dirSign = -1f;

                if (isRotationSpring)
                {
                    float multX = Plugin._StanceWiggleX?.Value ?? 1f;
                    float multY = Plugin._StanceWiggleY?.Value ?? 1f;
                    float multZ = Plugin._StanceWiggleZ?.Value ?? 1f;

                    Vector3 wiggle = new Vector3(
                        dirSign * UnityEngine.Random.Range(0.2f, 0.5f) * multX, // X: Pitch
                        UnityEngine.Random.Range(-0.3f, 0.3f) * multY,          // Y: Yaw
                        UnityEngine.Random.Range(-0.1f, 0.1f) * multZ           // Z: Roll
                    ) * weightFactor;
                    _currentWiggleRotation += wiggle * 0.5f;
                    _wiggleRotationVelocity += wiggle * speedMultiplier;
                }
                if (isPositionSpring)
                {
                    float multX = Plugin._StanceWiggleX?.Value ?? 1f;
                    float multY = Plugin._StanceWiggleY?.Value ?? 1f;
                    float multZ = Plugin._StanceWiggleZ?.Value ?? 1f;

                    Vector3 wigglePos = new Vector3(
                        UnityEngine.Random.Range(-0.01f, 0.01f) * multX,        // X: Esquerda/Direita
                        dirSign * UnityEngine.Random.Range(0.01f, 0.03f) * multY, // Y: Cima/Baixo
                        dirSign * UnityEngine.Random.Range(0.02f, 0.05f) * multZ  // Z: Frente/Trás
                    ) * weightFactor;
                    _currentWigglePosition += wigglePos * 1.0f;
                    _wigglePositionVelocity += wigglePos * speedMultiplier;
                    
                    // Schedule double wiggle
                    if (Plugin._EnableDoubleWiggle?.Value == true)
                    {
                        _waitingForDoubleWiggle = true;
                        _doubleWiggleExecutionTime = UnityEngine.Time.time + 0.12f; // 120ms delay
                    }
                }
            }
            else if (executeDoubleWiggle && Plugin._EnableStanceWiggle?.Value == true)
            {
                // Double Wiggle (inverse direction, reduced strength)
                float dirSign = 1f; // Inverted!
                float doubleWiggleStrength = 0.35f; // 35% of original
                
                if (isRotationSpring)
                {
                    float multX = Plugin._StanceWiggleX?.Value ?? 1f;
                    float multY = Plugin._StanceWiggleY?.Value ?? 1f;
                    float multZ = Plugin._StanceWiggleZ?.Value ?? 1f;

                    Vector3 wiggle = new Vector3(
                        dirSign * 0.35f * multX, // Static pitch bounce
                        0f,                      // No secondary yaw
                        0f                       // No secondary roll
                    ) * _lastWeaponWeightFactor * doubleWiggleStrength;
                    
                    _currentWiggleRotation += wiggle * 0.5f;
                    _wiggleRotationVelocity += wiggle * speedMultiplier;
                }
                if (isPositionSpring)
                {
                    float multY = Plugin._StanceWiggleY?.Value ?? 1f;
                    float multZ = Plugin._StanceWiggleZ?.Value ?? 1f;

                    Vector3 wigglePos = new Vector3(
                        0f,
                        dirSign * 0.02f * multY, 
                        dirSign * 0.035f * multZ  
                    ) * _lastWeaponWeightFactor * doubleWiggleStrength;
                    
                    _currentWigglePosition += wigglePos * 1.0f;
                    _wigglePositionVelocity += wigglePos * speedMultiplier;
                    
                    // Turn it off so it doesn't execute again
                    _waitingForDoubleWiggle = false;
                }
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
                
            // Update target values when state changes
            if (stateChanged)
            {
                _isStable = false;
                
                // Item 009: o wiggle foi MOVIDO para fora deste bloco (ver após o fast-path) — agora
                // dispara por troca intencional de stance (request) em vez de comparação de _previousStance.

                _wasAiming = isAiming;
                _wasInStance = isInStanceFull;
                _wasHoldingFirearm = isHoldingFirearm;
                _previousStance = currentStance;
            }

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

                if (isInStance != _wasInStance || currentStance != _previousStance)
                {
                    // Inicia a curva cinematográfica em qualquer troca de stance
                    _isPlayingTransitionCurve = true;
                    _transitionCurveTimer = 0f;
                }

                _wasAiming = isAiming;
                _wasInStance = isInStance;
                _wasHoldingFirearm = isHoldingFirearm;
            }
            float deltaTime = Time.deltaTime;
            float normalDamping = Plugin._StanceTransitionDamping?.Value ?? 1.0f;
            float angularFreq = SpringMath.SmoothTimeToAngularFrequency(smoothTime);

            _currentRotation = SpringMath.SpringDamp(_currentRotation, _targetRotation, ref _rotationVelocity, normalDamping, angularFreq, deltaTime);
            _currentShoulderingRotation = SpringMath.SpringDamp(_currentShoulderingRotation, _targetShoulderingRotation, ref _shoulderingRotationVelocity, normalDamping, angularFreq, deltaTime);
            _currentPosition = SpringMath.SpringDamp(_currentPosition, _targetPosition, ref _positionVelocity, normalDamping, angularFreq, deltaTime);

            // Avaliação das Curvas Cinemáticas
            if (_isPlayingTransitionCurve)
            {
                _transitionCurveTimer += deltaTime;
                float progress = Mathf.Clamp01(_transitionCurveTimer / 0.35f); // 0.35s fixo de acordo com o design

                _currentCurveRotation = shwngFpsCameraStances.StanceTransitionCurves.EvaluateRotation(progress);
                _currentCurvePosition = shwngFpsCameraStances.StanceTransitionCurves.EvaluatePosition(progress);

                if (progress >= 1.0f)
                {
                    _isPlayingTransitionCurve = false;
                }
            }
            else
            {
                _currentCurveRotation = Vector3.zero;
                _currentCurvePosition = Vector3.zero;
            }

            float wiggleDuration = _wasAiming ? (Plugin._WiggleDurationADS?.Value ?? 0.15f) : (Plugin._WiggleDurationHipfire?.Value ?? 0.15f);
            float wiggleFreq = SpringMath.SmoothTimeToAngularFrequency(wiggleDuration);
            
            _currentWiggleRotation = SpringMath.SpringDamp(_currentWiggleRotation, Vector3.zero, ref _wiggleRotationVelocity, 1.0f, wiggleFreq, deltaTime);
            _currentWigglePosition = SpringMath.SpringDamp(_currentWigglePosition, Vector3.zero, ref _wigglePositionVelocity, 1.0f, wiggleFreq, deltaTime);
            
            // Track stability for potential early exit optimization in future frames
            // Use small epsilon for approximate comparison since SmoothDamp converges asymptotically
            bool atRotationTarget = (_currentRotation - _targetRotation).sqrMagnitude < epsilon;
            bool atPositionTarget = (_currentPosition - _targetPosition).sqrMagnitude < epsilon;
            bool atShoulderingTarget = (_currentShoulderingRotation - _targetShoulderingRotation).sqrMagnitude < epsilon;
            bool atWiggleRotationTarget = _currentWiggleRotation.sqrMagnitude < epsilon;
            bool atWigglePositionTarget = _currentWigglePosition.sqrMagnitude < epsilon;
            
            _wasStable = _isStable;
            _isStable = atRotationTarget && atPositionTarget && atShoulderingTarget && 
                       atWiggleRotationTarget && atWigglePositionTarget &&
                       !_isInShoulderingPhase && !_isInStanceShoulderingPhase;

            // Apply the interpolated values based on which spring this is
            if (isRotationSpring)
            {
                // Add both the stance/ADS rotation AND the shouldering rotation offset AND the wiggle AND the curve
                __result = _currentRotation + _currentShoulderingRotation + _currentWiggleRotation + _currentCurveRotation + __instance.Current;
            }
            else if (isPositionSpring)
            {
                __result = _currentPosition + _currentWigglePosition + _currentCurvePosition + __instance.Current;
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
