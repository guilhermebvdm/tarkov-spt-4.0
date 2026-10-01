using EFT;
using SAIN.Extensions;
using SAIN.Models.PlayerData;
using UnityEngine;

namespace SAIN.Classes;

public sealed class PlayerTransformClass
{
    public Vector3 Position { get; private set; }
    public Vector3 EyePosition { get; private set; }
    public Vector3 BodyPosition { get; private set; }
    public Vector3 LookDirection { get; private set; }

    public PlayerHeadData HeadData { get; private set; } = new();
    public PlayerNavData NavData { get; private set; } = new();
    public PlayerVelocityData VelocityData { get; private set; } = new();
    public PlayerWeaponData WeaponData { get; private set; } = new();

    public Vector3 WeaponRoot
    {
        get { return WeaponData.WeaponRoot; }
    }

    public Vector3 Right()
    {
        return AngledLookDirection(0f, 90f, 0f);
    }

    public Vector3 Left()
    {
        return AngledLookDirection(0f, -90f, 0f);
    }

    public Vector3 AngledLookDirection(float x, float y, float z)
    {
        return Quaternion.Euler(x, y, z) * LookDirection;
    }

    public void ManualUpdate(Player player, bool isAI)
    {
        Vector3 playerPosition = player.Position;
        Vector3 playerLookDir = player.LookDirection;
        BifacialTransform headTransform = player.PlayerBones.Head;
        BifacialTransform bodyTransform = player.PlayerBones.Ribcage;

        // zombies don't have eye part
        EBodyPartColliderType eyePart = player.PlayerBones.BodyPartCollidersDictionary.ContainsKey(EBodyPartColliderType.Eyes)
            ? EBodyPartColliderType.Eyes
            : EBodyPartColliderType.HeadCommon;
        BodyPartCollider eyeCollider = player.PlayerBones.BodyPartCollidersDictionary[eyePart];

        Position = playerPosition;
        LookDirection = playerLookDir;
        EyePosition = eyeCollider.Center;
        BodyPosition = bodyTransform.position;

        WeaponData = WeaponData.Update(playerPosition, player);
        NavData = NavData.Update(playerPosition, isAI);
        VelocityData = VelocityData.Update(player.MovementContext.Velocity);
        HeadData = HeadData.Update(headTransform.position, headTransform.rotation, headTransform.up);
    }
}
