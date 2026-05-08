using EFT;
using UnityEngine;

public class LootingStateClass : MovementState
{
	public LootingStateClass(MovementContext movementContext)
		: base(movementContext)
	{
	}

	public override void Enter(bool isFromSameState)
	{
		base.Enter(isFromSameState);
		if (!isFromSameState)
		{
			Vector2 yawLimit = new Vector2(MovementContext.Yaw - 15f, MovementContext.Yaw + 15f);
			Vector2 pitchLimit = new Vector2(MovementContext.Pitch - 15f, MovementContext.Pitch + 3f);
			MovementContext.SetRotationLimit(yawLimit, pitchLimit);
		}
		MovementContext.SetTilt(0f);
	}

	public override void Exit(bool toSameState)
	{
		if (!toSameState)
		{
			MovementContext.SetRotationLimit(Player.PlayerMovementConstantsClass.FULL_YAW_RANGE, Player.PlayerMovementConstantsClass.STAND_POSE_ROTATION_PITCH_RANGE);
		}
		base.Exit(toSameState);
	}

	public override void SetTilt(float tilt)
	{
	}

	public override void Loot(bool enabled)
	{
		MovementContext.PlayerAnimatorEnableLoot(enabled);
	}
}
