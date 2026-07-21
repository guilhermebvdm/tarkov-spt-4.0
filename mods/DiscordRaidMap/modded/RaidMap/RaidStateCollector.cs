using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using DiscordRaidMap.Patches;
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
        private readonly List<Player> _deadPlayers = [];
        private readonly List<Player> _killedEnemies = [];
        private readonly List<Player> _killedBosses = [];
        private readonly List<AirdropSynchronizableObject> _airdrops = [];
        private Player _headlessReferencePlayer;

        public RaidStateCollector(GameWorld gameWorld)
        {
            _gameWorld = gameWorld;
            PlayerOnDeadPatch.OnDead += OnPlayerDead;
            AirdropLandedPatch.OnAirdropLanded += OnAirdropLanded;

            foreach (var airdrop in AirdropLandedPatch.Airdrops)
            {
                OnAirdropLanded(airdrop);
            }
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
            foreach (var airdrop in _airdrops.Where(a => a != null))
            {
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

        private void OnPlayerDead(Player player)
        {
            var referencePlayer = GetReferencePlayer(_gameWorld?.MainPlayer);
            TryTrackDeadPlayer(player, referencePlayer);
            TryTrackKilledPlayer(player, referencePlayer);
        }

        private void OnAirdropLanded(AirdropSynchronizableObject airdrop)
        {
            if (airdrop != null && !_airdrops.Contains(airdrop))
            {
                _airdrops.Add(airdrop);
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

            if (_headlessReferencePlayer != null)
            {
                return _headlessReferencePlayer;
            }

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
            PlayerOnDeadPatch.OnDead -= OnPlayerDead;
            AirdropLandedPatch.OnAirdropLanded -= OnAirdropLanded;
            _deadPlayers.Clear();
            _killedEnemies.Clear();
            _killedBosses.Clear();
            _airdrops.Clear();
        }
    }
}
