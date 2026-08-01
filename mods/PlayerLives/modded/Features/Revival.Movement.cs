using EFT;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerLives.Features
{
    // Standing the player back up, holding the stand pose for a few frames,
    // the downed movement lock, and the downed screen visual effect.
    internal partial class RevivalFeatures
    {
        // How many more frames to keep forcing the stand pose after revive.
        // One write loses the race to the animator blend / downed state, so we
        // re-assert for a short window until the stand actually lands.
        private static Dictionary<string, int> _standAssertFrames = new Dictionary<string, int>();
        private const int STAND_ASSERT_FRAMES = 20; // ~0.3s @60fps

        // Death fade pulses once for 1s on entering critical, then auto-clears.
        // Timer ticked every frame; fade off the frame it hits zero.
        private static float _deathFadeTimer = 0f;
        private const float DEATH_FADE_DURATION = 1.5f;

        // Drive an animated stand: clear prone, hard-set the pose target to full,
        // and request the pose change so the animator actually plays it. Then
        // arm the re-assert window so a stale downed tick can't drag us back down.
        private static void StandPlayerUp(Player player)
        {
            var mc = player?.MovementContext;
            if (mc == null) return;

            mc.IsInPronePose = false;
            mc.SetPoseLevel(1f);   // full stand

            _standAssertFrames[player.ProfileId] = STAND_ASSERT_FRAMES;
        }

        // Called every frame from the patch. While armed, keep re-asserting the
        // stand pose so nothing overrides it for the first few frames.
        private static void TickStandAssert(Player player)
        {
            if (player == null) return;
            string playerId = player.ProfileId;

            if (!_standAssertFrames.TryGetValue(playerId, out int left) || left <= 0)
                return;

            var mc = player.MovementContext;
            if (mc != null)
            {
                mc.IsInPronePose = false;
                mc.SetPoseLevel(1f);
            }

            _standAssertFrames[playerId] = left - 1;
        }

        private static void ArmDownedMovementBlock(string playerId)
        {
            _playerMovementBlockTimers[playerId] = DOWNED_MOVEMENT_BLOCK_DURATION;
        }

        // Cancel the downed block early (revive / give up / clear):
        // zero the timer AND lift the lock + visual right now, so nothing is
        // left stuck if the player dies this frame and Update stops running.
        private static void CancelDownedMovementBlock(Player player)
        {
            string playerId = player.ProfileId;
            _playerMovementBlockTimers[playerId] = 0f;
            _playerMovementLocked[playerId] = false;
            SetDownedMovementLock(player, false); // free movement

            // Kill the fade pulse, in case revive/give-up landed inside the 1s.
            _deathFadeTimer = 0f;
            ToggleDeathFade(false);
        }

        // Runs every frame. Counts the timer down and turns the lock + visual
        // ON the frame it starts and OFF the frame it hits zero. Nothing in between.
        private static void UpdateDownedMovementBlock(Player player)
        {
            if (player == null)
                return;

            string playerId = player.ProfileId;

            // How much block time is left (0 = not blocked).
            _playerMovementBlockTimers.TryGetValue(playerId, out float timeLeft);
            timeLeft = Mathf.Max(0f, timeLeft - Time.deltaTime);
            _playerMovementBlockTimers[playerId] = timeLeft;

            bool shouldLock = timeLeft > 0f;
            bool isLocked = _playerMovementLocked.TryGetValue(playerId, out bool locked) && locked;

            // Only act when the state actually changes, so we lock/unlock once each.
            if (shouldLock == isLocked)
                return;

            _playerMovementLocked[playerId] = shouldLock;
            SetDownedMovementLock(player, shouldLock); // also toggles the downed visual
        }

        // Applies the actual lock + downed visual together. Called only by
        // UpdateDownedMovementBlock: true = down, false = free.
        private static void SetDownedMovementLock(Player player, bool locked)
        {
            try
            {
                if (player?.MovementContext != null)
                {
                    player.MovementContext.IsAxesIgnored = locked;
                }

                if (player?.CharacterController is SimpleCharacterController simpleCharacterController)
                {
                    simpleCharacterController.IsMoveIgnored = locked;
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"Error toggling downed movement lock: {ex.Message}");
            }
        }

        // Pulse the death fade once for 1s. Called on entering critical state.
        private static void PulseDeathFade()
        {
            _deathFadeTimer = DEATH_FADE_DURATION;
            ToggleDeathFade(true);
        }

        // Ticked every frame. Drains the pulse timer and clears the fade once it
        // hits zero. No-op while the timer is already 0.
        private static void TickDeathFade()
        {
            if (_deathFadeTimer <= 0f)
                return;

            _deathFadeTimer = Mathf.Max(0f, _deathFadeTimer - Time.deltaTime);
            if (_deathFadeTimer <= 0f)
                ToggleDeathFade(false);
        }

        // Screen fade to dark. Independent of the blur.
        private static void ToggleDeathFade(bool enabled)
        {
            try
            {
                if (!CameraClass.Exist || CameraClass.Instance?.Camera == null)
                    return;

                var deathEffect = CameraClass.Instance.Camera.GetComponent<DeathFade>();
                if (deathEffect == null)
                    return;

                deathEffect.enabled = true;
                if (enabled)
                    deathEffect.EnableEffect();
                else
                    deathEffect.DisableEffect();
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"Error toggling death fade: {ex.Message}");
            }
        }
    }
}
