using System;
using EFT;
using EFT.Game.Spawning;
using UnityEngine;

namespace TRLDynamicSpawn
{
    public class CustomSpawnPoint : ISpawnPoint, IPositionPoint
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "CustomMOARSpawn";
        public bool SpawnBlocked { get; set; } = false;
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; } = Quaternion.identity;
        public EPlayerSideMask Sides => (EPlayerSideMask)int.MaxValue; 
        public ESpawnCategoryMask Categories => (ESpawnCategoryMask)int.MaxValue;
        public string Infiltration { get; set; } = "";
        public string BotZoneName { get; set; } = "";
        public bool IsSnipeZone { get; set; } = false;
        public float DelayToCanSpawnSec { get; set; } = 0f;
        public float NextBornTime { get; set; } = 0f;
        public int CorePointId { get; set; } = 0;
        public ISpawnPointCollider Collider { get; set; } = null;

        public float CalcMultiSpawnDelay(float delay, BotCreationDataClass data) { return delay; }
        public void Dispose() { }
        public bool IsNotCollidedArtillery(ServerShellingControllerClass controller) { return true; }
        public bool IsInPlayersIndividualLimits(BotCreationDataClass data) { return true; }
        public void IncreaseUsedPlayerSpawnsForNearestPlayer(BotCreationDataClass data) { }
    }
}
