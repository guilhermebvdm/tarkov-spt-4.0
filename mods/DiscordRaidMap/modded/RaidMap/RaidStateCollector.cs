using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.SynchronizableObjects;
using HarmonyLib;
using UnityEngine;

namespace DiscordRaidMap.RaidMap
{
    internal sealed class RaidStateCollector : IDisposable
    {
        private static readonly FieldInfo PlayerCorpseField = AccessTools.Field(typeof(Player), "Corpse");
        private static readonly FieldInfo PlayerLastAggressorField = AccessTools.Field(typeof(Player), "LastAggressor");

        private static readonly HashSet<WildSpawnType> BossRoles =
        [
            WildSpawnType.bossBoar,
            WildSpawnType.bossBully,
            WildSpawnType.bossGluhar,
            WildSpawnType.bossKilla,
            WildSpawnType.bossKnight,
            WildSpawnType.followerBigPipe,
            WildSpawnType.followerBirdEye,
            WildSpawnType.bossKolontay,
            WildSpawnType.bossKojaniy,
            WildSpawnType.bossSanitar,
            WildSpawnType.bossTagilla,
            WildSpawnType.bossPartisan,
            WildSpawnType.bossZryachiy,
            WildSpawnType.gifter,
            WildSpawnType.arenaFighterEvent,
            WildSpawnType.sectantPriest,
            WildSpawnType.bossTagillaAgro,
            WildSpawnType.bossKillaAgro,
            WildSpawnType.tagillaHelperAgro
        ];

        private readonly GameWorld _gameWorld;

        // Accumulated across the raid (deduped by Contains), cleared on Dispose. Fed only by the
        // interval corpse scan (no per-tick/per-event tracking) — kept permanent so a despawned corpse
        // does not drop its marker (CR-01-02).
        private readonly List<Player> _deadPlayers = [];
        private readonly List<Player> _killedEnemies = [];
        private readonly List<Player> _killedBosses = [];
        private Player _headlessReferencePlayer;

        public RaidStateCollector(GameWorld gameWorld)
        {
            _gameWorld = gameWorld;
        }

        public RaidSnapshot CollectSnapshot()
        {
            var mainPlayer = _gameWorld?.MainPlayer;
            var mapLocation = GetMapLocation(mainPlayer);
            if (string.IsNullOrWhiteSpace(mapLocation) || !MapRegistry.TryGet(mapLocation, out var map))
            {
                return null;
            }

            var referencePlayer = GetReferencePlayer(mainPlayer);

            var snapshot = new RaidSnapshot
            {
                Map = map,
                TimeRemaining = GetRaidTimeRemaining()
            };

            RefreshKilledPlayers(referencePlayer);

            AddPlayers(snapshot, referencePlayer);
            AddKilled(snapshot, _deadPlayers, RaidMarkerType.DeadPlayer);
            AddKilled(snapshot, _killedEnemies, RaidMarkerType.KilledEnemy);
            AddKilled(snapshot, _killedBosses, RaidMarkerType.KilledBoss);
            AddAirdrops(snapshot);
            AddExtracts(snapshot, referencePlayer, mainPlayer == null);

            return snapshot;
        }

        private void AddPlayers(RaidSnapshot snapshot, IPlayer referencePlayer)
        {
            foreach (var player in _gameWorld.AllPlayersEverExisted ?? [])
            {
                if (player == null
                    || player.HealthController == null
                    || !player.HealthController.IsAlive
                    || IsHeadlessClient(player)
                    || referencePlayer == null
                    || !IsTrackedHumanPlayer(player, referencePlayer))
                {
                    continue;
                }

                snapshot.Markers.Add(new RaidMarker
                {
                    Type = RaidMarkerType.Player,
                    MapPosition = ToMapPosition(player.Position),
                    RotationDegrees = -player.Rotation.x,
                    Label = player.Profile?.GetCorrectedNickname() ?? "Player"
                });
            }
        }

        private static void AddKilled(RaidSnapshot snapshot, IEnumerable<Player> players, RaidMarkerType type)
        {
            foreach (var player in players.Where(p => p != null))
            {
                snapshot.Markers.Add(new RaidMarker
                {
                    Type = type,
                    MapPosition = ToMapPosition(player.Position),
                    Label = type == RaidMarkerType.DeadPlayer
                        ? player.Profile?.GetCorrectedNickname() ?? "Player"
                        : ""
                });
            }
        }

        private void AddAirdrops(RaidSnapshot snapshot)
        {
            // Pull active airdrops from the world at snapshot time instead of a per-tick Harmony patch.
            var processor = _gameWorld?.SynchronizableObjectLogicProcessor;
            if (processor == null)
            {
                return;
            }

            foreach (var syncObject in processor.GetSynchronizableObjects())
            {
                // GetSynchronizableObjects() yields all pooled objects unfiltered; only draw an
                // airdrop that is actually initialized and active (CR-01-01) — pooled/uninited
                // instances would render as phantom markers.
                if (syncObject is not AirdropSynchronizableObject airdrop
                    || airdrop == null
                    || !airdrop.IsInited
                    || !airdrop.IsActive)
                {
                    continue;
                }

                snapshot.Markers.Add(new RaidMarker
                {
                    Type = RaidMarkerType.Airdrop,
                    MapPosition = ToMapPosition(airdrop.transform.position)
                });
            }
        }

        private void AddExtracts(RaidSnapshot snapshot, Player referencePlayer, bool skipInfiltrationMatch)
        {
            var controller = _gameWorld.ExfiltrationController;
            if (controller == null || referencePlayer == null)
            {
                return;
            }

            IEnumerable<ExfiltrationPoint> extracts = referencePlayer.Side == EPlayerSide.Savage
                ? controller.ScavExfiltrationPoints
                : controller.ExfiltrationPoints;

            if (controller.SecretExfiltrationPoints != null)
            {
                extracts = (extracts ?? []).Concat(controller.SecretExfiltrationPoints.Cast<ExfiltrationPoint>());
            }

            foreach (var extract in extracts ?? [])
            {
                if (extract == null
                    || !extract.isActiveAndEnabled
                    || (!skipInfiltrationMatch && !extract.InfiltrationMatch(referencePlayer)))
                {
                    continue;
                }

                var markerType = extract.Status switch
                {
                    EExfiltrationStatus.RegularMode => RaidMarkerType.Extract,
                    EExfiltrationStatus.Countdown => RaidMarkerType.Extract,
                    EExfiltrationStatus.Pending => RaidMarkerType.Extract,
                    EExfiltrationStatus.Hidden => RaidMarkerType.ExtractRequirements,
                    EExfiltrationStatus.UncompleteRequirements => RaidMarkerType.ExtractRequirements,
                    _ => (RaidMarkerType?)null
                };

                if (markerType == null)
                {
                    continue;
                }

                snapshot.Markers.Add(new RaidMarker
                {
                    Type = markerType.Value,
                    MapPosition = ToMapPosition(extract.transform.position)
                });
            }
        }

        private static bool IsBoss(IPlayer player)
        {
            return player?.Profile?.Side == EPlayerSide.Savage
                && BossRoles.Contains(player.Profile.Info.Settings.Role);
        }

        private void RefreshKilledPlayers(IPlayer mainPlayer)
        {
            foreach (var player in _gameWorld.AllPlayersEverExisted ?? [])
            {
                if (player == null || !HasCorpse(player))
                {
                    continue;
                }

                TryTrackDeadPlayer(player, mainPlayer);
                TryTrackKilledPlayer(player, mainPlayer);
            }
        }

        private void TryTrackDeadPlayer(Player player, IPlayer mainPlayer)
        {
            if (player != null && HasCorpse(player) && IsTrackedHumanPlayer(player, mainPlayer) && !_deadPlayers.Contains(player))
            {
                _deadPlayers.Add(player);
            }
        }

        private void TryTrackKilledPlayer(Player player, IPlayer mainPlayer)
        {
            if (player == null || IsTrackedHumanPlayer(player, mainPlayer))
            {
                return;
            }

            var aggressor = PlayerLastAggressorField.GetValue(player) as IPlayer;
            if (!IsTrackedHumanPlayer(aggressor, mainPlayer))
            {
                return;
            }

            var list = IsBoss(player) ? _killedBosses : _killedEnemies;
            if (!list.Contains(player))
            {
                list.Add(player);
            }
        }

        private static bool HasCorpse(Player player)
        {
            return PlayerCorpseField.GetValue(player) != null;
        }

        private static bool IsHeadlessClient(IPlayer player)
        {
            return player?.Profile?.Info?.MemberCategory == EMemberCategory.UnitTest;
        }

        private static bool IsTrackedHumanPlayer(IPlayer player, IPlayer mainPlayer)
        {
            if (player == null || mainPlayer == null)
            {
                return false;
            }

            if (player.ProfileId == mainPlayer.ProfileId)
            {
                return true;
            }

            return !string.IsNullOrEmpty(mainPlayer.GroupId) && player.GroupId == mainPlayer.GroupId;
        }

        private string GetMapLocation(Player mainPlayer)
        {
            if (!string.IsNullOrWhiteSpace(mainPlayer?.Location))
            {
                return mainPlayer.Location;
            }

            return _gameWorld?.LocationId;
        }

        private Player GetReferencePlayer(Player mainPlayer)
        {
            if (mainPlayer != null && !IsHeadlessClient(mainPlayer))
            {
                return mainPlayer;
            }

            // Revalidate the cached reference: a disconnected/dead peer would give stale side/extract
            // filtering, so drop it and re-resolve to a live candidate (CR-01-07).
            if (_headlessReferencePlayer != null
                && _headlessReferencePlayer.HealthController != null
                && _headlessReferencePlayer.HealthController.IsAlive)
            {
                return _headlessReferencePlayer;
            }

            _headlessReferencePlayer = null;

            var fikaCandidates = (_gameWorld?.AllPlayersEverExisted ?? [])
                .Where(player => player != null
                    && !IsHeadlessClient(player)
                    && string.Equals(player.GroupId, "Fika", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (fikaCandidates.Count > 0)
            {
                _headlessReferencePlayer = fikaCandidates.First();
                return _headlessReferencePlayer;
            }

            var aliveCandidates = (_gameWorld?.AllPlayersEverExisted ?? [])
                .Where(player => player != null
                    && player.HealthController != null
                    && player.HealthController.IsAlive
                    && !IsHeadlessClient(player))
                .ToList();

            return aliveCandidates.FirstOrDefault(player => !string.IsNullOrEmpty(player.GroupId))
                ?? aliveCandidates.FirstOrDefault();
        }

        private static Vector3 ToMapPosition(Vector3 unityPosition)
        {
            return new Vector3(unityPosition.x, unityPosition.z, unityPosition.y);
        }

        private static string GetRaidTimeRemaining()
        {
            var game = Singleton<AbstractGame>.Instance;
            var timer = game?.GameTimer;
            if (timer?.EscapeDateTime == null)
            {
                return "";
            }

            var remaining = timer.EscapeDateTime.Value - EFTDateTimeClass.UtcNow;
            if (remaining < TimeSpan.Zero)
            {
                remaining = TimeSpan.Zero;
            }

            return $"{(int)remaining.TotalMinutes:00}:{remaining.Seconds:00}";
        }

        public void Dispose()
        {
            _deadPlayers.Clear();
            _killedEnemies.Clear();
            _killedBosses.Clear();
            _headlessReferencePlayer = null;
        }
    }
}
