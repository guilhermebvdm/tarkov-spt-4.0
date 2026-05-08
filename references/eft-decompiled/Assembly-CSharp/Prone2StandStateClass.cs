using System;
using EFT;
using JetBrains.Annotations;

public class Prone2StandStateClass(MovementContext movementContext) : IdleStateClass(movementContext)
{
	[NonSerialized]
	public float Float_1 = 0.3f;

	public override void ManualAnimatorMoveUpdate(float deltaTime)
	{
		float normalizedTime = NormalizedTime;
		if (normalizedTime > Float_1)
		{
			float normalTime = (normalizedTime - Float_1) / (1f - Float_1);
			MovementContext.RestoreDefaultAlignment(normalTime);
			MovementContext.GrounderSetActive(grounderIsOn: true);
		}
		base.ManualAnimatorMoveUpdate(deltaTime);
	}

	public override void Enter(bool isFromSameState)
	{
		base.Enter(isFromSameState);
		MovementContext.AdjustCharacterController(prone: false);
		MovementContext.PlayerAnimatorTransitionSpeed = MovementContext.TransitionSpeed;
		MovementContext.LeftStanceController.SetAnimatorLeftStanceToCacheFromBodyAction();
	}

	public override void Pickup(bool enabled, [CanBeNull] Action action)
	{
	}

	public override void Jump()
	{
	}

	public override void Exit(bool toSameState)
	{
		base.Exit(toSameState);
		MovementContext.RestoreDefaultAlignment(1f);
		MovementContext.GrounderSetActive(grounderIsOn: true);
		MovementContext.SetRotationLimit(Player.PlayerMovementConstantsClass.FULL_YAW_RANGE, Player.PlayerMovementConstantsClass.STAND_POSE_ROTATION_PITCH_RANGE);
		MovementContext.TrunkRotationLimit = EFTHardSettings.Instance.HANDS_TO_BODY_MAX_ANGLE;
		MovementContext.SetPOMCollider(PlayerOverlapManager.EExtrusionCollider.Default);
	}
}
