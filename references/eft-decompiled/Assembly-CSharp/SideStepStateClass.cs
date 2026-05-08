using System;
using EFT;
using UnityEngine;

public class SideStepStateClass : IdleStateClass
{
	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public float Float_3;

	public SideStepStateClass(MovementContext movementContext)
		: base(movementContext)
	{
	}

	public override void Enter(bool isFromSameState)
	{
		base.Enter(isFromSameState);
		if (!isFromSameState)
		{
			float num = MovementContext.Yaw - MovementContext.HandsToBodyAngle;
			Vector2 yawLimit = new Vector2(num - MovementContext.TrunkRotationLimit + 1f, num + MovementContext.TrunkRotationLimit - 1f);
			MovementContext.SetRotationLimit(yawLimit, Player.PlayerMovementConstantsClass.STAND_POSE_ROTATION_PITCH_RANGE);
		}
		Float_2 = 0f;
		Float_1 = 0f;
		Float_3 = ((AdditionalDirectionInfo != EMovementDirection.Left) ? 1 : (-1));
		MovementContext.SetTilt(0f);
		switch (Type)
		{
		case EStateType.In:
			MovementContext.SetSidestep(0f);
			break;
		case EStateType.None:
		case EStateType.Out:
			MovementContext.SetSidestep(Float_3);
			break;
		}
	}

	public override void ReEnter()
	{
		base.ReEnter();
		Float_2 = 0f;
		Float_1 = 0f;
		Float_3 = ((AdditionalDirectionInfo != EMovementDirection.Left) ? 1 : (-1));
	}

	public override void Exit(bool toSameState)
	{
		if (!toSameState)
		{
			MovementContext.SetRotationLimit(Player.PlayerMovementConstantsClass.FULL_YAW_RANGE, Player.PlayerMovementConstantsClass.STAND_POSE_ROTATION_PITCH_RANGE);
			base.SetStep(0);
		}
		switch (Type)
		{
		case EStateType.Out:
			MovementContext.SetSidestep(Float_2);
			break;
		case EStateType.In:
			MovementContext.SetSidestep(toSameState ? Float_3 : Float_2);
			break;
		}
		base.Exit(toSameState);
	}

	public override void ManualAnimatorMoveUpdate(float deltaTime)
	{
		base.ManualAnimatorMoveUpdate(deltaTime);
		method_0(deltaTime);
		switch (Type)
		{
		case EStateType.None:
			MovementContext.SetSidestep(Float_3);
			break;
		case EStateType.In:
			MovementContext.SetSidestep(Mathf.Lerp(Float_2, Float_3, Float_1 / StateLength));
			break;
		case EStateType.Out:
			MovementContext.SetSidestep(Mathf.Lerp(Float_3, Float_2, Float_1 / StateLength));
			break;
		}
		Float_1 += deltaTime;
	}

	public override void SetTilt(float tilt)
	{
	}

	public override void SetStep(int step)
	{
		if (Type != EStateType.Out)
		{
			base.SetStep(step);
		}
	}

	public override void Jump()
	{
		SetStep(0);
		MovementContext.SetSidestep(0f);
	}
}
