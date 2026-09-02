using UnityEngine;

namespace SAIN.Models.PlayerData;

public sealed class PlayerWeaponData
{
    /// <summary>
    /// The place the bullet shoots from
    /// </summary>
    public Vector3 FirePort { get; set; }

    /// <summary>
    /// The direction a player's weapon is actually pointing
    /// </summary>
    public Vector3 PointDirection { get; set; }

    /// <summary>
    /// Player's Weapon Root Position
    /// </summary>
    public Vector3 WeaponRoot { get; set; }

    /// <summary>
    /// Direction from a player's position to their weaponroot.
    /// </summary>
    public Vector3 WeaponRootOffset { get; set; }

    /// <summary>
    /// A Player's position, but the y value is set to their weaponroot's y.
    /// </summary>
    public Vector3 WeaponRootHeightOffset { get; set; }
}
