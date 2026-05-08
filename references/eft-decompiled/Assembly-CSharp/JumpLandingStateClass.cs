using System;
using Comfort.Common;
using EFT;
using UnityEngine;

public class JumpLandingStateClass : MovementState
{
	[NonSerialized]
	public const float Float_0 = 0.35f;

	[NonSerialized]
	public Vector2 Vector2_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public bool Bool_0;

	public JumpLandingStateClass(MovementContext movementContext)
		: base(movementContext)
	{
	}

	public override void Enter(bool isFromSameState)
	{
		base.Enter(isFromSameState);
		Float_1 = 0f;
		Bool_0 = false;
		Vector2_0 = Vector2.zero;
		Float_2 = GClass2298.Evaluate(Singleton<BackendConfigSettingsClass>.Instance.Inertia.MaxTimeWithoutInput, MovementContext.Inertia) / 2f;
	}

	public override void ManualAnimatorMoveUpdate(float deltaTime)
	{
		if (Float_1 > Float_2)
		{
			Bool_0 = true;
		}
		if (GClass855.IsZero(Vector2_0))
		{
			Float_1 += deltaTime;
			if (MovementContext.SmoothedCharacterMovementSpeed > 0.35f)
			{
				MovementContext.SmoothedCharacterMovementSpeed = Mathf.Lerp(MovementContext.SmoothedCharacterMovementSpeed, 0.35f, Singleton<BackendConfigSettingsClass>.Instance.Inertia.SuddenChangesSmoothness * deltaTime);
			}
		}
		ProcessRotation(deltaTime);
		if (!Bool_0)
		{
			ProcessAnimatorMovement(deltaTime);
		}
	}

	public override void Move(Vector2 direction)
	{
		base.Move(direction);
		Vector2_0 = direction;
	}
}
