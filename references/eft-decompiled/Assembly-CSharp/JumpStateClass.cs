using System;
using EFT;
using UnityEngine;

public class JumpStateClass : MovementState
{
	public enum EJumpState
	{
		PushingFromTheGround,
		Jump,
		Bumbped
	}

	[NonSerialized]
	public Vector2 Vector2_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public EJumpState EjumpState_0;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public float Float_5 = 1f;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public const float Float_6 = 0.1f;

	[NonSerialized]
	public const int Int_1 = 3;

	[NonSerialized]
	public LayerMask LayerMask_0;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public float Float_9;

	[NonSerialized]
	public float Float_10;

	[NonSerialized]
	public Vector3 Vector3_2;

	public JumpStateClass(MovementContext movementContext)
		: base(movementContext)
	{
		StickToGround = false;
	}

	public override void Enter(bool isFromSameState)
	{
		Vector2_0 = Vector2.zero;
		Vector3_2 = Vector3.zero;
		Float_5 = (float)MovementContext.SkillManager.StrengthBuffJumpHeightInc + 1f;
		MovementContext.HoldBreath(enable: false);
		MovementContext.OnJump();
		MovementContext.PlayerAnimator.SetDoClimb(doClimb: false);
		MovementContext.PlayerAnimator.SetDoVault(doVaulting: false);
		Float_2 = 0f;
		Int_0 = 0;
		Float_7 = MovementContext.SmoothedCharacterMovementSpeed;
		Bool_0 = MovementContext.PreviousState is SprintStateClass;
		EjumpState_0 = EJumpState.PushingFromTheGround;
		Vector3 vector = MovementContext.InputMotionBeforeLimit;
		Vector3 vector2 = ((MovementContext.PreviousState is RunStateClass) ? MovementContext.AbsoluteMovementDirection.normalized : Vector3.zero);
		vector = new Vector3((Mathf.Abs(vector.x) > Mathf.Abs(vector2.x)) ? vector.x : vector2.x, 0f, (Mathf.Abs(vector.z) > Mathf.Abs(vector2.z)) ? vector.z : vector2.z);
		Float_1 = vector.magnitude;
		Float_0 = Float_1;
		Vector3_0 = ((Float_1 > 0.1f) ? vector.normalized : MovementContext.TransformForwardVector);
		Float_9 = MovementContext.TransformPosition.y;
		Float_10 = 0f;
		float num = ((!MovementContext.PhysicalConditionContainsAny(EPhysicalCondition.LeftLegDamaged | EPhysicalCondition.RightLegDamaged) || MovementContext.PhysicalConditionIs(EPhysicalCondition.OnPainkillers)) ? Mathf.Lerp(1f, 0.66f, MovementContext.Overweight) : 0.66f);
		Vector3_1 = EFTHardSettings.Instance.LIFT_VELOCITY_BY_SPEED.Evaluate(Float_1) * num * Vector3.up;
		Float_3 = EFTHardSettings.Instance.JUMP_DELAY_BY_SPEED.Evaluate(Float_1);
		MovementContext.PlayerAnimatorEnableLanding(enabled: false);
		base.Enter(isFromSameState);
		MovementContext.SetTilt(0f);
		MovementContext.ObstacleCollisionFacade.Reset();
		MovementContext.LeftStanceController.DisableLeftStanceAnimFromBodyAction();
	}

	public override void Exit(bool toSameState)
	{
		base.Exit(toSameState);
		if (MovementContext.IsInPronePose)
		{
			MovementContext.IsInPronePose = MovementContext.CanProne;
		}
		MovementContext.SetTilt(0f);
		MovementContext.OnJumpEnd();
		MovementContext.LeftStanceController.SetAnimatorLeftStanceToCacheFromBodyAction();
	}

	public override void ManualAnimatorMoveUpdate(float deltaTime)
	{
		MovementContext.PlayerAnimatorEnableInert(Vector2_0.magnitude > 0.1f);
		if (EjumpState_0 == EJumpState.PushingFromTheGround)
		{
			if (!MovementContext.HeadBump(Vector3_2.y * deltaTime) && Float_2 <= 2f + Float_3)
			{
				if (!MovementContext.IsGrounded && Float_2 > Float_3)
				{
					EjumpState_0 = EJumpState.Jump;
				}
			}
			else
			{
				method_0();
			}
		}
		else if (EjumpState_0 == EJumpState.Jump)
		{
			if (Float_10 > EFTHardSettings.Instance.JumpTimeDescendingForStateExit && MovementContext.IsGrounded)
			{
				MovementContext.PlayerAnimatorEnableJump(enabled: false);
				MovementContext.PlayerAnimatorEnableLanding(enabled: true);
				if (Vector2_0.sqrMagnitude > 0.1f && MovementContext.CanWalk)
				{
					MovementContext.EnableSprint(Bool_0 && Vector2_0.y > 0.1f);
					MovementContext.PlayerAnimatorEnableSprint(MovementContext.IsSprintEnabled, dontChangeLeftStanceValue: true);
					MovementContext.MovementDirection = Vector2_0;
					MovementContext.PlayerAnimatorEnableInert(enabled: true);
				}
				else
				{
					MovementContext.PlayerAnimatorEnableInert(enabled: false);
				}
			}
			if (MovementContext.HeadBump(Vector3_2.y * deltaTime))
			{
				method_0();
			}
			if (Bool_1)
			{
				if (Mathf.Abs(MovementContext.TransformPosition.y - Float_8) < 0.0001f)
				{
					Int_0++;
					base.FreefallTime = deltaTime;
					if (Int_0 > 3)
					{
						method_0();
					}
				}
				else
				{
					Int_0 = 0;
				}
			}
		}
		if (!MovementContext.IsGrounded)
		{
			base.FreefallTime += deltaTime;
			Float_8 = MovementContext.TransformPosition.y;
			Bool_1 = true;
		}
		else
		{
			base.FreefallTime = deltaTime;
			Bool_1 = false;
		}
		Float_2 += deltaTime;
		ApplyMovementAndRotation(deltaTime);
	}

	public void method_0()
	{
		MovementContext.PlayerAnimatorEnableJump(enabled: false);
		MovementContext.PlayerAnimatorEnableLanding(enabled: true);
		EjumpState_0 = EJumpState.Bumbped;
	}

	public virtual void ApplyMovementAndRotation(float deltaTime)
	{
		Quaternion rotation = Quaternion.Lerp(MovementContext.TransformRotation, Quaternion.AngleAxis(MovementContext.Yaw, Vector3.up), EFTHardSettings.Instance.TRANSFORM_ROTATION_LERP_SPEED * deltaTime);
		MovementContext.ApplyRotation(rotation);
		method_1(deltaTime);
		MovementContext.PlayerAnimatorSetAimAngle(MovementContext.Pitch);
	}

	public override void EnableSprint(bool enable, bool isToggle = false)
	{
		Bool_0 &= enable && MovementContext.CanSprint;
	}

	public void method_1(float deltaTime)
	{
		float num = Float_2 - Float_3;
		Vector3_2 = ((Float_2 < Float_3) ? Vector3.zero : (Vector3_1 * Float_5 + Physics.gravity * num));
		Vector3 rhs = (GClass855.IsZero(Vector2_0) ? Vector3.zero : MovementContext.TransformVector(new Vector3(Vector2_0.x, 0f, Vector2_0.y).normalized));
		float num2 = Vector3.Dot(Vector3_0, rhs);
		if (MovementContext.TransformPosition.y < Float_9)
		{
			Vector3_2.y += (Float_9 - MovementContext.TransformPosition.y) / deltaTime * 0.9f;
		}
		float b = ((num2 < -0.966f) ? (Float_1 * EFTHardSettings.Instance.AIR_CONTROL_BACK_DIR) : ((!(num2 > 0.966f)) ? (Float_1 * EFTHardSettings.Instance.AIR_CONTROL_NONE_OR_ORT_DIR) : Mathf.Max(EFTHardSettings.Instance.AIR_MIN_SPEED, Float_1 * EFTHardSettings.Instance.AIR_CONTROL_SAME_DIR)));
		if (EjumpState_0 == EJumpState.Jump)
		{
			Float_0 = Mathf.Lerp(Float_0, b, deltaTime * EFTHardSettings.Instance.AIR_LERP);
		}
		Vector3 motion = (Vector3_0 * Float_0 + Vector3_2) * deltaTime;
		if (EjumpState_0 == EJumpState.Bumbped)
		{
			motion.y = Mathf.Min(motion.y, 0f);
		}
		float y = MovementContext.TransformPosition.y;
		Float_9 = MovementContext.TransformPosition.y + Vector3_2.y * deltaTime;
		LimitMotion(ref motion, deltaTime);
		MovementContext.ApplyMotion(motion, deltaTime);
		if (MovementContext.TransformPosition.y - y < 0.001f || Vector3_2.y < 0f)
		{
			Float_10 += deltaTime;
		}
	}

	public override void Move(Vector2 direction)
	{
		Vector2_0 = direction;
	}

	public override void SetTilt(float tilt)
	{
	}
}
