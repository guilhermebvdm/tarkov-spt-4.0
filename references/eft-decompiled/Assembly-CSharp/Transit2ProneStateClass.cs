using System;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class Transit2ProneStateClass : IdleStateClass
{
	[NonSerialized]
	public float Float_1;

	public Transit2ProneStateClass(MovementContext movementContext)
		: base(movementContext)
	{
	}

	public override void Enter(bool isFromSameState)
	{
		base.Enter(isFromSameState);
		if (!isFromSameState)
		{
			Float_1 = 0f;
			MovementContext.PlayerAnimatorTransitionSpeed = MovementContext.TransitionSpeed;
			MovementContext.TrunkRotationLimit = 25f;
			MovementContext.SetPitchSmoothly(Player.PlayerMovementConstantsClass.PRONE_POSE_ROTATION_PITCH_RANGE);
			MovementContext.GrounderSetActive(grounderIsOn: false);
			MovementContext.AdjustCharacterController(prone: true);
			MovementContext.SetPOMCollider(PlayerOverlapManager.EExtrusionCollider.Prone);
			MovementContext.LeftStanceController.DisableLeftStanceAnimFromBodyAction();
		}
	}

	public override void ManualAnimatorMoveUpdate(float deltaTime)
	{
		ProcessRotation(deltaTime);
		ProcessAnimatorMovement(deltaTime);
		MovementContext.AlignToSurface(deltaTime);
	}

	public override void Pickup(bool enabled, [CanBeNull] Action action)
	{
	}

	public override void Jump()
	{
	}

	public override void ProcessAnimatorMovement(float deltaTime)
	{
		Vector3 motion = MovementContext.PlayerAnimatorDeltaPosition;
		ApplyGravity(ref motion, deltaTime);
		Vector3 forwardProjectionOnSurface = MovementContext.ForwardProjectionOnSurface;
		if (MovementContext.OverlapOrHasNoGround(0.75f, forwardProjectionOnSurface, 0f, 4f, 0.2f))
		{
			MovementContext.RestorePreviousYaw();
			Vector3 vector = MovementContext.InverseTransformVector(motion);
			Float_1 = Mathf.Lerp(Float_1, Mathf.Max(0.01f, MovementContext.GetPenetrationDepth(forwardProjectionOnSurface, 0.35f)), deltaTime * 10f);
			vector.z = 0f - Float_1;
			motion = MovementContext.TransformVector(vector);
		}
		else if (MovementContext.OverlapOrHasNoGround(0.75f, -forwardProjectionOnSurface, 0.25f, 4f, 0.15f))
		{
			MovementContext.RestorePreviousYaw();
			Vector3 vector2 = MovementContext.InverseTransformVector(motion);
			Float_1 = Mathf.Lerp(Float_1, Mathf.Max(0.01f, MovementContext.GetPenetrationDepth(-forwardProjectionOnSurface, 0.4f)), deltaTime * 7f);
			vector2.z = Float_1;
			motion = MovementContext.TransformVector(vector2);
		}
		MovementContext.ApplyMotion(motion, deltaTime);
		MovementContext.ApplyRotation(MovementContext.TransformRotation * MovementContext.AnimatorDeltaRotation);
	}
}
