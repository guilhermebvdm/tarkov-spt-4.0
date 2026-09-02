using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Models.PlayerData;

public sealed class PlayerNavData
{
    public EPlayerNavMeshDistance Status { get; set; }

    /// <summary>
    /// Player Position with y Value set to the navmesh they are on.
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// Last Successful NavMesh Sample Position.
    /// </summary>
    public NavMeshHit LastValidHit { get; set; }

    public float TimeLastOnNavMesh { get; set; }
    public bool IsOnNavMesh { get; set; }
    public float NextCheckTime { get; set; }
}
