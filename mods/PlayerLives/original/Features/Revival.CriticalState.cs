using EFT;
using EFT.Communications;
using System;
using UnityEngine;
using PlayerLives.Helpers;
using EFT.UI.Screens;

namespace PlayerLives.Features
{
    // Entering / leaving the downed (critical) state and the related notifications.
    internal partial class RevivalFeatures
    {
        public static void SetPlayerCriticalState(Player player, bool criticalState)
        {
            if (player == null)
                return;

            string playerId = player.ProfileId;

            // Update critical state
            _playerInCriticalState[playerId] = criticalState;

            if (criticalState)
            {
                ApplyCriticalStatePlayer(player);
            }
            else
            {
                _nextCriticalNotificationTimes.Remove(playerId);
                CancelDownedMovementBlock(player);

                // If player is leaving critical state without revival (e.g., revival failed),
                // make sure to remove stealth from player and disable invulnerability
                if (!_playerInvulnerabilityTimers.ContainsKey(playerId))
                {
                    _playerIsInvulnerable.Remove(playerId);
                }
            }
        }

        private static void ApplyCriticalStatePlayer(Player player)
        {
            try
            {
                string playerId = player.ProfileId;

                // Misnomer: ToggleScreen does not toggle - it forces the screen to
                // the given type, so this closes the inventory (and keeps it closed)
                // rather than flipping it open when it was already shut.
                CurrentScreenSingletonClass.Instance.ToggleScreen(EEftScreenType.Inventory);

                ArmDownedMovementBlock(playerId);

                PulseDeathFade();

                SetDamageCoeffZero(player);

                player.Speaker.Play(EPhraseTrigger.OnAgony, player.HealthStatus, true);

            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"Error applying critical mode: {ex.Message}");
            }
        }

        private static void ShowCriticalStateNotification(Player player, bool force = false)
        {
            if (player == null || !player.IsYourPlayer) return;

            string playerId = player.ProfileId;
            float now = Time.time;

            if (!force &&
                _nextCriticalNotificationTimes.TryGetValue(playerId, out float nextNotificationTime) &&
                now < nextNotificationTime)
            {
                return;
            }

            _nextCriticalNotificationTimes[playerId] = now + 4f;
            ShowClientNotification(
                $"You died. Press {Settings.REVIVAL_KEY.Value} to revive or {Settings.GIVE_UP_KEY.Value} to give up.",
                ENotificationDurationType.Long,
                ENotificationIconType.Alert,
                Color.red);
        }

        private static void ShowClientNotification(
            string message,
            ENotificationDurationType duration,
            ENotificationIconType icon,
            Color color)
        {
            try
            {
                NotificationManagerClass.DisplayMessageNotification(message, duration, icon, color);
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"Error displaying client notification '{message}': {ex.Message}");
            }
        }

        public static bool IsPlayerInCriticalState(string playerId)
        {
            return _playerInCriticalState.TryGetValue(playerId, out bool inCritical) && inCritical;
        }
    }
}
