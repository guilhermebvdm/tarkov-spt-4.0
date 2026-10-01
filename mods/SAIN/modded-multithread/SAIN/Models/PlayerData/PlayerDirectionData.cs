using SAIN.Models.Direction;
using UnityEngine;

namespace SAIN.Models.PlayerData;

// This has to be a struct due to it being used in a unity struct
public struct PlayerDirectionData
{
    public Vector3 OwnerViewPosition { get; set; }
    public Vector3 OwnerPosition { get; set; }
    public Vector3 OwnerLookDirection { get; set; }

    public DirectionData MainDirectionData { get; set; }
}
