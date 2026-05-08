using System;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class IdleStateClass : MovementState
{
	[NonSerialized]
	public const float Float_0 = 1E-05f;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public GClass777 Gclass777_0;

	public static bool smethod_0(Vector2 direction)
	{
		if (!(direction.x > 1E-05f) && !(direction.y > 1E-05f) && !(direction.x < -1E-05f))
		{
			return direction.y < -1E-05f;
		}
		return true;
	}

	public IdleStateClass(MovementContext movementContext)
		: base(movementContext)
	{
		if (!MovementContext.IsAI)
		{
			Gclass777_0 = new GClass777();
			Gclass777_0.Init(movementContext);
		}
	}

	public override void Exit(bool toSameState)
	{
		base.Exit(toSameState);
		MovementContext.HoldBreath(enable: false);
		Bool_1 = false;
		Bool_0 = false;
	}

	public override void Enter(bool isFromSameState)
	{
		base.Enter(isFromSameState);
		MovementContext.EnableSprint(enable: false);
		MovementContext.LeftStanceController.SetAnimatorLeftStanceToCacheFromBodyAction();
		Gclass777_0?.Enter();
	}

	public override void BlindFire(int b)
	{
		MovementContext.SetBlindFire(b);
	}

	public override void BlendMotion(ref Vector3 motion, float deltaTime)
	{
		motion = Vector3.Lerp(motion, MovementContext.LastBlendMotionDelta * deltaTime, EFTHardSettings.Instance.IdleStateMotionPreservation);
	}

	public override void Pickup(bool enabled, [CanBeNull] Action action)
	{
		MovementContext.OverrideState(MovementContext.PickUpState);
		MovementContext.PickupAction = action;
	}

	public override void Plant(bool enabled, bool multitool, float plantTime, Action<bool> action)
	{
		if (MovementContext.PlantState is PlantStateClass plantStateClass)
		{
			plantStateClass.PlantMultitool = multitool;
			plantStateClass.PlantTime = plantTime;
		}
		MovementContext.OverrideState(MovementContext.PlantState);
		MovementContext.PlantAction = action;
	}

	public override void Examine(bool enabled, [CanBeNull] Action action)
	{
		MovementContext.OverrideState(MovementContext.PickUpState);
		MovementContext.PickupAction = action;
	}

	public override void Move(Vector2 direction)
	{
		if (smethod_0(direction))
		{
			direction.x = Math.Sign(direction.x);
			direction.y = Math.Sign(direction.y);
			MovementContext.MovementDirection = direction * 0.01f;
			if (!MovementContext.CanWalk)
			{
				return;
			}
			MovementContext.EnableSprint(Bool_0 && direction.y > 0.1f);
			if (MovementContext.IsSprintEnabled)
			{
				MovementContext.SetPoseLevel(1f);
				if (MovementContext.PoseLevel > 0.9f && MovementContext.SmoothedCharacterMovementSpeed >= 1f)
				{
					MovementContext.PlayerAnimatorEnableSprint(enabled: true);
				}
			}
			MovementContext.PlayerAnimatorEnableInert(enabled: true);
		}
		else
		{
			MovementContext.MovementDirection = Vector2.zero;
		}
	}

	public override void Jump()
	{
		if (MovementContext.PoseLevel > 0.6f && MovementContext.IsGrounded)
		{
			MovementContext.TryJump();
		}
		else
		{
			ChangePose(1f - MovementContext.PoseLevel);
		}
	}

	public override void Vaulting()
	{
		MovementContext.TryVaulting();
	}

	public override void EnableSprint(bool enable, bool isToggle = false)
	{
		if (!isToggle)
		{
			Bool_0 = enable && MovementContext.CanSprint;
		}
	}

	public override void EnableBreath(bool enable)
	{
		MovementContext.HoldBreath(enable);
	}

	public override void Kick()
	{
		MovementContext.PlayerAnimatorEnableKick(enabled: true);
	}

	public override void SetStep(int step)
	{
		if (Mathf.Abs(step) > 0)
		{
			Vector3 vector = new Vector3(MovementContext.TransformForwardVector.z, 0f, 0f - MovementContext.TransformForwardVector.x);
			if (MovementContext.OverlapOrHasNoGround(0.3f, vector * Mathf.Sign(step), 0.2f, 3f))
			{
				MovementContext.Step = 0;
				return;
			}
		}
		MovementContext.Step = step;
	}

	public void method_0(float deltaTime)
	{
		Gclass777_0?.ProcessAnimatorStep(deltaTime, Type);
	}
}
