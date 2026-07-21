using System.Collections.Generic;
using UnityEngine;

namespace DiscordRaidMap.RaidMap
{
    internal enum RaidMarkerType
    {
        Player,
        DeadPlayer,
        KilledEnemy,
        KilledBoss,
        Airdrop,
        Extract,
        ExtractRequirements
    }

    internal sealed class RaidSnapshot
    {
        public MapDefinition Map { get; set; }
        public string TimeRemaining { get; set; } = "";
        public List<RaidMarker> Markers { get; } = [];
    }

    internal sealed class RaidMarker
    {
        public RaidMarkerType Type { get; set; }
        public Vector3 MapPosition { get; set; }
        public float RotationDegrees { get; set; }
        public string Label { get; set; } = "";
    }
}
