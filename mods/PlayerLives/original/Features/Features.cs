using EFT;
using EFT.UI.Screens;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using PlayerLives.Helpers;

namespace PlayerLives.Features
{
    /// <summary>
    /// Enhanced revival feature with manual activation and temporary invulnerability with restrictions.
    /// Split across partial files by concern:
    ///   Revival.CriticalState.cs   - entering/leaving downed state + notifications
    ///   Revival.Invulnerability.cs - invuln timer, buffs, countdown
    ///   Revival.Movement.cs        - stand up, pose assert, movement lock, downed visual
    ///   Revival.Weapon.cs          - hide/reveal weapon while downed
    ///   Revival.Revive.cs          - manual revive, give up, heal, revive items
    /// </summary>
    internal partial class RevivalFeatures : ModulePatch
    {
        // States
        private static Dictionary<string, long> _lastRevivalTimesByPlayer = new Dictionary<string, long>();
        private static Dictionary<string, bool> _playerInCriticalState = new Dictionary<string, bool>();
        private static Dictionary<string, bool> _playerIsInvulnerable = new Dictionary<string, bool>();
        private static Dictionary<string, float> _playerInvulnerabilityTimers = new Dictionary<string, float>();
        private static Dictionary<string, int> _playerLastInvulnerabilityCountdownSeconds = new Dictionary<string, int>();
        private static Dictionary<string, float> _playerDamageCoeff = new Dictionary<string, float>();
        private static Dictionary<string, float> _nextCriticalNotificationTimes = new Dictionary<string, float>();
        private static Dictionary<string, float> _playerMovementBlockTimers = new Dictionary<string, float>();
        private static Dictionary<string, bool> _playerMovementLocked = new Dictionary<string, bool>();
        // Damage type of the blow that downed the player. Replayed on GiveUp so
        // the profile's LethalDamage records the real cause of death. Value 0 has
        // no EDamageType name and breaks server JSON parsing, so sanitize it.
        private static Dictionary<string, EDamageType> _lastLethalDamageTypeByPlayer = new Dictionary<string, EDamageType>();
        private static Player PlayerClient { get; set; } = null;
        private const float DOWNED_MOVEMENT_BLOCK_DURATION = 2f;

        // Wipe all per-player state. Called on raid start so entries from a
        // previous raid can't leak forward and so the dictionaries don't grow
        // for the life of the process.
        public static void ResetState()
        {
            _lastRevivalTimesByPlayer.Clear();
            _playerInCriticalState.Clear();
            _playerIsInvulnerable.Clear();
            _playerInvulnerabilityTimers.Clear();
            _playerLastInvulnerabilityCountdownSeconds.Clear();
            _playerDamageCoeff.Clear();
            _nextCriticalNotificationTimes.Clear();
            _playerMovementBlockTimers.Clear();
            _playerMovementLocked.Clear();
            _lastLethalDamageTypeByPlayer.Clear();
            _standAssertFrames.Clear();
            _deathFadeTimer = 0f;
            PlayerClient = null;
        }

        public static void RecordLethalDamage(string playerId, EDamageType damageType)
        {
            if (!Enum.IsDefined(typeof(EDamageType), damageType))
            {
                damageType = EDamageType.Undefined;
            }
            _lastLethalDamageTypeByPlayer[playerId] = damageType;
        }

        public static EDamageType GetLastLethalDamageType(string playerId)
        {
            return _lastLethalDamageTypeByPlayer.TryGetValue(playerId, out EDamageType type)
                ? type
                : EDamageType.Undefined;
        }

        protected override MethodBase GetTargetMethod()
        {
            // We're patching the Update method of Player to constantly check for revival key press
            return AccessTools.Method(typeof(Player), nameof(Player.UpdateTick));
        }

        [PatchPostfix]
        static void Postfix(Player __instance)
        {
            try
            {
                Player player = __instance;

                // Bail before touching anything - UpdateTick runs for every player
                // on the map, so all work below must stay behind this guard.
                if (!player.IsYourPlayer) return;

                string playerId = player.ProfileId;
                PlayerClient = player;

                // Invuln timer must count down EVERY frame to stay accurate.
                float invulnTimer = 0f;
                bool inInvuln = _playerIsInvulnerable.TryGetValue(playerId, out bool inv) && inv;
                if (inInvuln && _playerInvulnerabilityTimers.TryGetValue(playerId, out invulnTimer))
                {
                    invulnTimer -= Time.deltaTime;
                    _playerInvulnerabilityTimers[playerId] = invulnTimer;

                    if (invulnTimer <= 0)
                    {
                        EndInvulnerability(player);
                        inInvuln = false;
                    }
                }

                // Drive the downed movement lock off the timer every frame,
                // even after critical state clears, so the OFF flip always lands.
                UpdateDownedMovementBlock(player);

                // Drain the 1s death-fade pulse and clear it when it expires.
                TickDeathFade();

                // Keep re-asserting the stand pose for a few frames after revive.
                TickStandAssert(player);

                bool inCritical = _playerInCriticalState.TryGetValue(playerId, out bool crit) && crit;

                // Keypresses must run EVERY frame - GetKeyDown is true for one frame only,
                // a throttle would drop presses.
                if (inCritical)
                {
                    if (Input.GetKeyDown(Settings.REVIVAL_KEY.Value))
                    {
                        TryPerformManualRevival(player);
                    }

                    if (Input.GetKeyDown(Settings.GIVE_UP_KEY.Value))
                    {
                        GiveUp(player);
                    }
                }

                // Upkeep for downed AND invuln - run every frame (no throttle).
                if (inCritical || inInvuln)
                {

                    HideWeapon(player);

                    if (inCritical)
                    {
                        ShowCriticalStateNotification(player);
                        player.MovementContext.SetPoseLevel(0);
                        player.MovementContext.IsInPronePose = true;
                    }

                    if (inInvuln)
                    {
                        ShowInvulnerabilityCountdown(playerId, invulnTimer);
                        RefillStamina(player);
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"Error in RevivalFeatureExtension patch: {ex.Message}");
            }
        }
    }
}
