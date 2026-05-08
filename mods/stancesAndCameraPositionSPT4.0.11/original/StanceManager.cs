using System;
using UnityEngine;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;

namespace CameraRotationMod
{
    public enum Stance
    {
        Default,
        Stance1,
        Stance2,
        Stance3
    }

    /// <summary>
    /// Manages stance toggling between Default, Stance1, and Stance2 positions/rotations
    /// </summary>
    public static class StanceManager
    {
        public static Stance CurrentStance { get; private set; } = Stance.Default;
        public static bool IsInStance => CurrentStance != Stance.Default;
        
        private static ConfigEntry<KeyCode> _stanceToggleKeyConfig;
        private static ConfigEntry<bool> _enableMouseWheelCycleConfig;
        private static ConfigEntry<KeyCode> _mouseWheelModifierKeyConfig;
        private static float _lastScrollTime = 0f;
        private const float ScrollCooldown = 0.15f; // Prevent scroll spam

        // Tac Sprint variables - track state to avoid setting animator every frame
        private static bool _isTacSprintActive = false;
        private static bool _wasAiming = false;
        
        // Tac Sprint reset delay variables
        private static bool _isWaitingToResetTacSprint = false;
        private static float _tacSprintResetTimer = 0f;
        
        // Track GameWorld to detect raid changes and reset state
        private static GameWorld _lastGameWorld = null;
        
        // Cached GameWorld reference for this frame (avoids multiple Singleton lookups)
        private static GameWorld _cachedGameWorld = null;
        private static int _cachedGameWorldFrame = -1;
        
        // Cached weapon properties - rebuilt when weapon changes
        private static Player.FirearmController _lastFirearmController = null;
        private static bool _cachedIsBullpup = false;
        private static int _cachedWeaponCellSizeX = 1;
        
        // Cached stance vectors - rebuilt when config changes
        private static bool _stanceValuesDirty = true;
        private static Vector3 _cachedADSRotation;
        private static Vector3 _cachedADSPosition;
        private static Vector3 _cachedStance1Rotation;
        private static Vector3 _cachedStance1Position;
        private static Vector3 _cachedStance2Rotation;
        private static Vector3 _cachedStance2Position;
        private static Vector3 _cachedStance3Rotation;
        private static Vector3 _cachedStance3Position;
        private static Vector3 _cachedDefaultPosition;
        
        // Cached sprint enabled flag - rebuilt when config changes
        private static bool _sprintEnabledDirty = true;
        private static bool _cachedAnySprintEnabled = false;
        
        /// <summary>
        /// Mark stance values as needing recalculation (called when config changes)
        /// </summary>
        public static void MarkStanceValuesDirty() => _stanceValuesDirty = true;
        
        /// <summary>
        /// Mark sprint enabled flag as needing recalculation (called when sprint config changes)
        /// </summary>
        public static void MarkSprintEnabledDirty() => _sprintEnabledDirty = true;

        public static void Initialize(ConfigEntry<KeyCode> stanceToggleKeyConfig)
        {
            _stanceToggleKeyConfig = stanceToggleKeyConfig;
            _enableMouseWheelCycleConfig = Plugin._EnableMouseWheelCycle;
            _mouseWheelModifierKeyConfig = Plugin._MouseWheelModifierKey;
        }

        public static void Update()
        {
            // Block stance switching while sprinting
            var gameWorld = GetCachedGameWorld();
            if (gameWorld?.MainPlayer?.IsSprintEnabled == true)
                return;
            
            // GetKeyDown already returns true for only one frame, no manual edge-detect needed
            if (UnityEngine.Input.GetKeyDown(_stanceToggleKeyConfig.Value))
            {
                CurrentStance = GetNextStance(CurrentStance);
            }

            // Mouse wheel cycling
            if (_enableMouseWheelCycleConfig?.Value == true)
            {
                // Check if modifier key is held
                if (UnityEngine.Input.GetKey(_mouseWheelModifierKeyConfig.Value))
                {
                    float scrollDelta = UnityEngine.Input.GetAxis("Mouse ScrollWheel");
                    
                    // Check cooldown to prevent rapid cycling
                    if (scrollDelta != 0 && Time.time - _lastScrollTime > ScrollCooldown)
                    {
                        if (scrollDelta > 0)
                        {
                            // Scroll up - cycle forward
                            CurrentStance = GetNextStance(CurrentStance);
                        }
                        else
                        {
                            // Scroll down - cycle backward
                            CurrentStance = GetPreviousStance(CurrentStance);
                        }
                        _lastScrollTime = Time.time;
                    }
                }
            }
        }

        /// <summary>
        /// Get the next enabled stance in the cycle, skipping disabled stances
        /// </summary>
        private static Stance GetNextStance(Stance current)
        {
            bool useOnlyStances = Plugin._UseOnlyStances?.Value ?? false;
            int maxAttempts = 5; // Prevent infinite loops
            int attempts = 0;
            
            Stance next = current;
            
            do
            {
                // Move to next stance in sequence
                next = next switch
                {
                    Stance.Default => Stance.Stance1,
                    Stance.Stance1 => Stance.Stance2,
                    Stance.Stance2 => Stance.Stance3,
                    Stance.Stance3 => Stance.Default,
                    _ => Stance.Default
                };
                
                attempts++;
                
                // Check if this stance should be included in cycle
                if (IsStanceEnabled(next, useOnlyStances))
                    return next;
                    
            } while (attempts < maxAttempts && next != current);
            
            // If we cycled through everything and nothing is enabled, return current
            return current;
        }

        /// <summary>
        /// Get the previous enabled stance in the cycle, skipping disabled stances (for scroll down)
        /// </summary>
        private static Stance GetPreviousStance(Stance current)
        {
            bool useOnlyStances = Plugin._UseOnlyStances?.Value ?? false;
            int maxAttempts = 5; // Prevent infinite loops
            int attempts = 0;
            
            Stance prev = current;
            
            do
            {
                // Move to previous stance in sequence (reverse order)
                prev = prev switch
                {
                    Stance.Default => Stance.Stance3,
                    Stance.Stance1 => Stance.Default,
                    Stance.Stance2 => Stance.Stance1,
                    Stance.Stance3 => Stance.Stance2,
                    _ => Stance.Default
                };
                
                attempts++;
                
                // Check if this stance should be included in cycle
                if (IsStanceEnabled(prev, useOnlyStances))
                    return prev;
                    
            } while (attempts < maxAttempts && prev != current);
            
            // If we cycled through everything and nothing is enabled, return current
            return current;
        }
        
        /// <summary>
        /// Check if a stance is enabled and should be included in the cycle
        /// </summary>
        private static bool IsStanceEnabled(Stance stance, bool useOnlyStances)
        {
            switch (stance)
            {
                case Stance.Default:
                    // Default is included unless "use only stances" is enabled
                    return !useOnlyStances;
                    
                case Stance.Stance1:
                    return Plugin._EnableStance1?.Value ?? true;
                    
                case Stance.Stance2:
                    return Plugin._EnableStance2?.Value ?? true;
                    
                case Stance.Stance3:
                    return Plugin._EnableStance3?.Value ?? true;
                    
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Check if player is holding a firearm (not melee, empty hands, or using items)
        /// </summary>
        public static bool IsHoldingFirearm()
        {
            var gameWorld = GetCachedGameWorld();
            if (gameWorld?.MainPlayer == null)
                return false;
            
            // HandsController will be FirearmController when holding a gun
            // It will be different controller types for melee, meds, food, grenades, empty hands
            return gameWorld.MainPlayer.HandsController is Player.FirearmController;
        }
        
        /// <summary>
        /// Get cached GameWorld reference to avoid multiple Singleton lookups per frame
        /// </summary>
        public static GameWorld GetCachedGameWorld()
        {
            int currentFrame = Time.frameCount;
            if (_cachedGameWorldFrame != currentFrame)
            {
                _cachedGameWorld = Singleton<GameWorld>.Instance;
                _cachedGameWorldFrame = currentFrame;
            }
            return _cachedGameWorld;
        }
        
        /// <summary>
        /// Rebuild cached stance vectors from config values
        /// </summary>
        private static void RebuildCachedStanceValues()
        {
            if (!_stanceValuesDirty)
                return;
                
            _cachedADSRotation = new Vector3(
                Plugin._ADSHandsPitchRotation?.Value ?? 0f,
                Plugin._ADSHandsYawRotation?.Value ?? 0f,
                Plugin._ADSHandsRollRotation?.Value ?? 0f
            );
            
            _cachedADSPosition = new Vector3(
                Plugin._ADSHandsSidewaysOffset?.Value ?? 0f,
                Plugin._ADSHandsUpDownOffset?.Value ?? 0f,
                Plugin._ADSHandsForwardBackwardOffset?.Value ?? 0f
            );
            
            _cachedStance1Rotation = new Vector3(
                Plugin._Stance1HandsPitchRotation?.Value ?? 0f,
                Plugin._Stance1HandsYawRotation?.Value ?? 0f,
                Plugin._Stance1HandsRollRotation?.Value ?? 0f
            );
            
            _cachedStance1Position = new Vector3(
                Plugin._Stance1HandsSidewaysOffset?.Value ?? 0f,
                Plugin._Stance1HandsUpDownOffset?.Value ?? 0f,
                Plugin._Stance1HandsForwardBackwardOffset?.Value ?? 0f
            );
            
            _cachedStance2Rotation = new Vector3(
                Plugin._Stance2HandsPitchRotation?.Value ?? 0f,
                Plugin._Stance2HandsYawRotation?.Value ?? 0f,
                Plugin._Stance2HandsRollRotation?.Value ?? 0f
            );
            
            _cachedStance2Position = new Vector3(
                Plugin._Stance2HandsSidewaysOffset?.Value ?? 0f,
                Plugin._Stance2HandsUpDownOffset?.Value ?? 0f,
                Plugin._Stance2HandsForwardBackwardOffset?.Value ?? 0f
            );
            
            _cachedStance3Rotation = new Vector3(
                Plugin._Stance3HandsPitchRotation?.Value ?? 0f,
                Plugin._Stance3HandsYawRotation?.Value ?? 0f,
                Plugin._Stance3HandsRollRotation?.Value ?? 0f
            );
            
            _cachedStance3Position = new Vector3(
                Plugin._Stance3HandsSidewaysOffset?.Value ?? 0f,
                Plugin._Stance3HandsUpDownOffset?.Value ?? 0f,
                Plugin._Stance3HandsForwardBackwardOffset?.Value ?? 0f
            );
            
            _cachedDefaultPosition = (Plugin._DefaultHandsPositionEnabled?.Value ?? false)
                ? new Vector3(
                    Plugin._DefaultHandsSidewaysOffset?.Value ?? 0f,
                    Plugin._DefaultHandsUpDownOffset?.Value ?? 0f,
                    Plugin._DefaultHandsForwardBackwardOffset?.Value ?? 0f
                )
                : Vector3.zero;
            
            _stanceValuesDirty = false;
        }

        /// <summary>
        /// Get the current target rotation based on stance state and ADS state
        /// </summary>
        public static Vector3 GetTargetRotation(bool isAiming)
        {
            // Ensure cached values are up to date
            RebuildCachedStanceValues();
            
            // If ADS and reset rotation is enabled, return ADS rotation
            if (isAiming && (Plugin._ResetOnADS?.Value ?? false))
            {
                return _cachedADSRotation;
            }

            // Return cached rotation based on current stance
            return CurrentStance switch
            {
                Stance.Stance1 => _cachedStance1Rotation,
                Stance.Stance2 => _cachedStance2Rotation,
                Stance.Stance3 => _cachedStance3Rotation,
                _ => Vector3.zero
            };
        }

        /// <summary>
        /// Get the current target position based on stance state and ADS state
        /// </summary>
        public static Vector3 GetTargetPosition(bool isAiming)
        {
            // Ensure cached values are up to date
            RebuildCachedStanceValues();
            
            // If ADS and reset is enabled, return ADS position
            if (isAiming && (Plugin._ResetOnADS?.Value ?? false))
            {
                return _cachedADSPosition;
            }

            // Return cached position based on current stance
            return CurrentStance switch
            {
                Stance.Stance1 => _cachedStance1Position,
                Stance.Stance2 => _cachedStance2Position,
                Stance.Stance3 => _cachedStance3Position,
                _ => _cachedDefaultPosition
            };
        }

        /// <summary>
        /// Resets all stance state - called when entering new raid or GameWorld changes
        /// </summary>
        public static void ResetState()
        {
            CurrentStance = Stance.Default;
            _isTacSprintActive = false;
            _wasAiming = false;
            _isWaitingToResetTacSprint = false;
            _tacSprintResetTimer = 0f;
            _lastGameWorld = null;
            _lastScrollTime = 0f;
            _cachedGameWorld = null;
            _cachedGameWorldFrame = -1;
            _stanceValuesDirty = true;
            _sprintEnabledDirty = true;
            _lastFirearmController = null;
            _cachedIsBullpup = false;
            _cachedWeaponCellSizeX = 1;
        }
        
        /// <summary>
        /// Rebuild cached 'any sprint enabled' flag from config values
        /// </summary>
        private static void RebuildCachedSprintEnabled()
        {
            if (!_sprintEnabledDirty)
                return;
            
            _cachedAnySprintEnabled = (Plugin._Stance1SprintAnimationEnabled?.Value ?? false) ||
                                      (Plugin._Stance2SprintAnimationEnabled?.Value ?? false) ||
                                      (Plugin._Stance3SprintAnimationEnabled?.Value ?? false);
            _sprintEnabledDirty = false;
        }
        
        /// <summary>
        /// Updates the tac sprint animation state based on stance and sprint status
        /// Uses state tracking to avoid setting animator parameters every frame
        /// </summary>
        public static void UpdateTacSprint()
        {
            // Early exit: if no stances have sprint animation enabled, skip all processing
            // Only check this if tac sprint is not currently active (need to disable it if active)
            if (!_isTacSprintActive && !_isWaitingToResetTacSprint)
            {
                RebuildCachedSprintEnabled();
                if (!_cachedAnySprintEnabled)
                    return;
            }
            
            var gameWorld = GetCachedGameWorld();
            if (gameWorld?.MainPlayer == null)
            {
                // Reset state when no player
                ResetState();
                return;
            }
            
            // Detect GameWorld change (new raid) and reset state
            if (_lastGameWorld != gameWorld)
            {
                // Reset but don't change stance preference - user may want to keep their stance
                _isTacSprintActive = false;
                _isWaitingToResetTacSprint = false;
                _tacSprintResetTimer = 0f;
                _wasAiming = false;
                _lastGameWorld = gameWorld;
            }

            var player = gameWorld.MainPlayer;
            
            // Handle delayed reset timer
            if (_isWaitingToResetTacSprint)
            {
                _tacSprintResetTimer -= Time.deltaTime;
                
                // If player starts sprinting again during delay, cancel the reset
                if (player.IsSprintEnabled && IsHoldingFirearm() && CanDoTacSprint(player))
                {
                    _isWaitingToResetTacSprint = false;
                    _tacSprintResetTimer = 0f;
                    _isTacSprintActive = true; // Re-enable without calling EnableTacSprint (already set)
                    return;
                }
                
                // Timer expired - complete the reset
                if (_tacSprintResetTimer <= 0f)
                {
                    DisableTacSprintImmediate(player);
                }
                return;
            }
            
            // CRITICAL: If player switched away from firearm (e.g., consumables, melee), 
            // immediately disable tac sprint to prevent stuck animation glitch
            if (_isTacSprintActive && !IsHoldingFirearm())
            {
                DisableTacSprintImmediate(player);
                return;
            }
            
            // Check if player is aiming - CRITICAL for preventing stuck sprint
            bool isAiming = player.ProceduralWeaponAnimation?.IsAiming ?? false;
            
            // If we just entered ADS, immediately disable tac sprint (no delay)
            if (isAiming && !_wasAiming && _isTacSprintActive)
            {
                DisableTacSprintImmediate(player);
                _wasAiming = isAiming;
                return;
            }
            
            _wasAiming = isAiming;
            
            // Don't allow tac sprint while aiming
            if (isAiming)
            {
                if (_isTacSprintActive)
                {
                    DisableTacSprintImmediate(player);
                }
                return;
            }

            // Check if sprint animation is enabled for current stance
            bool sprintEnabled = CurrentStance switch
            {
                Stance.Stance1 => Plugin._Stance1SprintAnimationEnabled?.Value ?? false,
                Stance.Stance2 => Plugin._Stance2SprintAnimationEnabled?.Value ?? false,
                Stance.Stance3 => Plugin._Stance3SprintAnimationEnabled?.Value ?? false,
                _ => false // Default stance - no special sprint animation
            };

            // If feature disabled for this stance, make sure tac sprint is off
            if (!sprintEnabled)
            {
                if (_isTacSprintActive)
                {
                    DisableTacSprintImmediate(player);
                }
                return;
            }

            // Check if we should enable tac sprint
            bool shouldEnableTacSprint = CanDoTacSprint(player);

            // State change: enable tac sprint
            if (shouldEnableTacSprint && !_isTacSprintActive)
            {
                EnableTacSprint(player);
            }
            // State change: disable tac sprint (with delay)
            else if (!shouldEnableTacSprint && _isTacSprintActive)
            {
                StartTacSprintReset(player);
            }
        }

        /// <summary>
        /// Enable tac sprint animation - only called once when entering state
        /// </summary>
        private static void EnableTacSprint(Player player)
        {
            if (player.HandsController is Player.FirearmController)
            {
                player.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, 2f);
                _isTacSprintActive = true;
            }
        }

        /// <summary>
        /// Start delayed tac sprint reset - weapon stays in compact position for configured time
        /// </summary>
        private static void StartTacSprintReset(Player player)
        {
            float delay = Plugin._TacSprintResetDelay?.Value ?? 0.35f;
            
            if (delay <= 0f)
            {
                // No delay - instant reset
                DisableTacSprintImmediate(player);
                return;
            }
            
            // Start the delay timer
            _isTacSprintActive = false;
            _isWaitingToResetTacSprint = true;
            _tacSprintResetTimer = delay;
        }
        
        /// <summary>
        /// Immediately disable tac sprint animation - reset to actual weapon size
        /// Used for ADS, weapon swap, and other instant-reset scenarios
        /// </summary>
        private static void DisableTacSprintImmediate(Player player)
        {
            // Always mark as inactive first
            _isTacSprintActive = false;
            _isWaitingToResetTacSprint = false;
            _tacSprintResetTimer = 0f;
            
            // If holding a firearm, reset to actual weapon size (use cached cell size)
            if (player.HandsController is Player.FirearmController)
            {
                player.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, (float)_cachedWeaponCellSizeX);
            }
            else
            {
                // Not holding a firearm - reset to neutral (1 = default for non-weapons)
                // This prevents the stuck animation glitch when switching to consumables
                player.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, 1f);
            }
        }

        /// <summary>
        /// Check if player meets all conditions for tac sprint animation
        /// </summary>
        /// <summary>
        /// Rebuild cached weapon properties when the equipped firearm changes.
        /// Caches bullpup status and cell size to avoid per-frame string/calc overhead.
        /// </summary>
        private static void RebuildCachedWeaponProperties(Player.FirearmController fc)
        {
            if (fc == _lastFirearmController)
                return;
            
            _lastFirearmController = fc;
            
            var weapon = fc.Item;
            _cachedWeaponCellSizeX = weapon.CalculateCellSize().X;
            
            // Zero-allocation bullpup check using OrdinalIgnoreCase
            string templateName = weapon.Template.Name;
            string shortNameKey = weapon.Template.ShortNameLocalizationKey.ToString();
            
            _cachedIsBullpup = templateName.IndexOf("bullpup", StringComparison.OrdinalIgnoreCase) >= 0 ||
                               shortNameKey.IndexOf("aug", StringComparison.OrdinalIgnoreCase) >= 0 ||
                               shortNameKey.IndexOf("p90", StringComparison.OrdinalIgnoreCase) >= 0 ||
                               shortNameKey.IndexOf("mdr", StringComparison.OrdinalIgnoreCase) >= 0 ||
                               shortNameKey.IndexOf("rfb", StringComparison.OrdinalIgnoreCase) >= 0;
        }
        
        private static bool CanDoTacSprint(Player player)
        {
            // Must be in stance and sprinting
            if (!IsInStance || !player.IsSprintEnabled)
                return false;

            if (!(player.HandsController is Player.FirearmController fc))
                return false;

            // Rebuild cached weapon properties if weapon changed
            RebuildCachedWeaponProperties(fc);
            
            var weapon = fc.Item;
            
            // Use cached values instead of per-frame recalculation
            float weightLimit = _cachedIsBullpup ? Plugin._TacSprintWeightLimitBullpup.Value : Plugin._TacSprintWeightLimit.Value;
            
            return weapon.TotalWeight <= weightLimit &&
                   _cachedWeaponCellSizeX <= Plugin._TacSprintLengthLimit.Value &&
                   weapon.ErgonomicsTotal > Plugin._TacSprintErgoLimit.Value;
        }
    }
}
