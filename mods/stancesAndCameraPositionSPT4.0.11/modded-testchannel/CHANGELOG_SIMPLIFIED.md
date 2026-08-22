# Changelog

## v1.0.5 (February 2026)

### New Features

#### Stance Transition Shouldering Effect
- **Affect Stance Transition Too** (toggle, default: true) - When enabled, applies the same shouldering throw effect when switching between stances (not just ADS)
- **Stance Change Sound Volume** (0-2, default: 1) - Volume multiplier for the aim rattle sound played when switching stances. 0 = muted, 1 = normal, 2 = louder

#### Rotation Throw Effects
- **Shoulder Throw Yaw** (-15 to 15, default: 6) - Yaw rotation during throw phase. Positive = rotate right. Applied to both ADS and stance transitions
- **Shoulder Throw Pitch** (-15 to 15, default: -3) - Pitch rotation during throw phase. Positive = rotate up. Applied to both ADS and stance transitions
- **Shoulder Throw Roll** (-15 to 15, default: -1.5) - Roll/tilt rotation during throw phase. Positive = tilt right. Applied to both ADS and stance transitions

#### Overall Throw Intensity Multipliers
- **ADS Shoulder Throw Intensity** (0-2, default: 1) - Overall intensity multiplier for ADS throw effect. Multiplies Forward, Up, Yaw, Pitch, Roll amounts. 0 = no throw, 1 = use config values, 2 = double effect
- **Stance Shoulder Throw Intensity** (0-2, default: 0.75) - Overall intensity multiplier for stance switch throw effect. Same behavior as ADS intensity

#### Weapon Stats Scaling for Stance Transitions
- **Advanced Stance Transition Stat Intensity** (0.01-2, default: 1) - How strongly weapon weight/ergonomics affects stance transition speed and shouldering throw amounts. Uses a separate intensity slider from ADS transitions

### Technical Details
- Rotation throw uses the same smooth interpolation system as position throw (SmoothDamp)
- Rotation and position shouldering offsets are tracked separately for proper blending
- Stance transitions have their own weapon stat scaling that is independent from ADS scaling
- Stance change sound uses WeaponSoundPlayer.PlayAimingSound() with ergonomics-based volume calculation
- All new settings are under the Advanced ADS Transitions section
- Most new settings marked as IsAdvanced (require enabling Advanced Settings in ConfigurationManager)

---

## v1.1.0 (February 2026)

### Bug Fixes
- **Fixed stance switching during sprint**: Blocked ability to switch stances while sprinting to prevent instant tactical sprint mode switching glitches

### Performance Optimizations
- **Removed camera throw effect**: Eliminated camera shake/throw feature for performance reasons
- **Camera offset dirty flag**: Only updates camera offset when config values actually change (via SettingChanged callbacks)
- **Config value caching**: Stance rotation/position values cached and rebuilt only when settings change
- **Vector3 caching**: Pre-cached stance Vector3 values in StanceManager to eliminate per-frame allocations
- **GameWorld per-frame caching**: Single Singleton lookup per frame instead of multiple lookups
- **SpringGetPatch fast path**: Early exit when spring state is stable (not transitioning), skips all SmoothDamp calculations
- **Early exit optimizations**: 
  - Skips Advanced ADS transition logic entirely when feature is disabled
  - Skips tac sprint checks when no sprint animations are enabled

### Configuration Reorganization
- Unified stance sections: Each stance (1, 2, 3) now has all its settings (rotation, position, sprint animation) in a single section
- Added explicit Order values to all config entries for consistent display order in ConfigurationManager
- Reorganized sections in logical order:
  1. Positions (camera offsets)
  2. Settings (stance controls, hotkeys, transitions)
  3. Advanced ADS Transitions (shouldering effects)
  4. ADS Default Values (Advanced)
  5. Default Hands/Arms Positions (Advanced)
  6. Stance 1 / Stance 2 / Stance 3 (unified per-stance settings)
  7. Tac Sprint Settings (Advanced)
  8. Field of View

### Default Value Changes
- Advanced ADS Transitions now defaults to `false` (was `true`)
- ADS Transition Speed now defaults to `1` (was `2`)

---

## v1.1.4 (February 2026)

### Performance Optimizations (Plugin.cs)
- **String allocation elimination**: `CanDoTacSprint()` now caches bullpup status per weapon change via `RebuildCachedWeaponProperties()` instead of calling `.ToLower().Contains()` every frame
- **Cell size caching**: Weapon cell size X stored in `_cachedWeaponCellSizeX`, reused in both `CanDoTacSprint()` and `DisableTacSprintImmediate()`
- **Weapon cache invalidation**: Wired into `ResetState()` for proper cleanup

### Code Cleanup (Plugin.cs)
- **`_wasKeyPressed` simplified**: Removed redundant edge-detection guard around `GetKeyDown()` (which is already single-frame)
- **Double `IsHoldingFirearm` removed**: `CanDoTacSprint()` now checks `is Player.FirearmController` once (already implies holding firearm)
- **Unused `fc` variable removed**: `EnableTacSprint()` now uses `is Player.FirearmController` without binding

### SpringGetPatch.cs Fixes
- **Cached spring references**: `_cachedHandsRotation`/`_cachedHandsPosition` set once per GameWorld, allowing non-hands springs to early-exit before any GameWorld lookup
- **Fast-path stability**: `isInStanceFull` now includes `isHoldingFirearm` check, matching the actual transition logic
- **Coupled resets**: GameWorld change now calls both `ResetState()` and `StanceManager.ResetState()` together
- **Empty XML doc fixed**: `/// <summary>` block now has content

### Transit Bug Fix
- **`ValidateSpringCache()`**: Added to `SpringGetPatch`, called once per frame from `Plugin.Update()` before anything else. Compares cached Spring references against the current `HandsContainer`'s actual springs. If they don't match (transit, weapon swap, player recreation), the cache is cleared so the fast-exit path doesn't reject new Spring instances.

### Removed
- **GameWorld change detection in SpringGetPatch**: Removed `_lastGameWorld` field and the `if (_lastGameWorld != gameWorld)` reset block. `ValidateSpringCache()` already handles spring invalidation on transit/raid change.
- **Snap thresholds**: Removed `positionSnapThreshold`/`rotationSnapThreshold` constants and all three snap-to-target blocks. `SmoothDamp` converges naturally. Updated stability tracking to use approximate comparison (`sqrMagnitude < epsilon`) instead of exact equality.