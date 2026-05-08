using System;
using EFT;

public class BlindFireStateClass : IdleStateClass
{
	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	public BlindFireStateClass(MovementContext movementContext)
		: base(movementContext)
	{
	}

	public override void Enter(bool isFromSameState)
	{
		base.Enter(isFromSameState);
		Float_1 = 0f;
		Float_2 = ((AdditionalDirectionInfo != EMovementDirection.Right) ? 1 : (-1));
		MovementContext.SetTilt(0f);
		switch (Type)
		{
		case EStateType.In:
			MovementContext.SetBlindFireFloat(0f);
			break;
		case EStateType.None:
		case EStateType.Out:
			MovementContext.SetBlindFireFloat(Float_2);
			break;
		}
	}

	public override void ReEnter()
	{
		base.ReEnter();
		Float_1 = 0f;
		Float_2 = ((AdditionalDirectionInfo != EMovementDirection.Right) ? 1 : (-1));
	}

	public override void Exit(bool toSameState)
	{
		switch (Type)
		{
		case EStateType.Out:
			MovementContext.SetBlindFireFloat(Float_1);
			break;
		case EStateType.In:
			MovementContext.SetBlindFireFloat(toSameState ? Float_2 : Float_1);
			break;
		}
		base.Exit(toSameState);
	}

	public override void ManualAnimatorMoveUpdate(float deltaTime)
	{
		base.ManualAnimatorMoveUpdate(deltaTime);
		int num = ((AdditionalDirectionInfo != EMovementDirection.Right) ? 1 : (-1));
		float blindFireFloat = SmoothLerpAnimCurve.Evaluate(NormalizedTime) * (float)num;
		MovementContext.SetBlindFireFloat(blindFireFloat);
	}
}
