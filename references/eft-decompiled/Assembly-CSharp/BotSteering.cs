using System;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class BotSteering : GClass429
{
	public float FirstTurnSpeed = 160f;

	public float FirstTurnBigSpeed = 320f;

	public float TurnSpeedSprint = 200f;

	[NonSerialized]
	public Vector3 LookDirection_1 = Vector3.one;

	[NonSerialized]
	public Player Player;

	[NonSerialized]
	public Vector3 CustomPoint;

	[NonSerialized]
	public Vector3 CustomDirection;

	[NonSerialized]
	public BifacialTransform OwnerTransform;

	[NonSerialized]
	public bool BlockSteering;

	[NonSerialized]
	public bool IsErrorLogged;

	[NonSerialized]
	public bool IsErrorDirectionLog;

	[field: NonSerialized]
	public float Speed { get; set; }

	public Vector3 LookDirection => LookDirection_1;

	[field: NonSerialized]
	public EBotSteering SteeringMode { get; set; }

	public static BotSteering Create(BotOwner bot)
	{
		if (bot.Profile.Info.Settings.UseSimpleAnimator)
		{
			return new GClass498(bot);
		}
		return new BotSteering(bot);
	}

	public BotSteering([NotNull] BotOwner owner)
		: base(owner)
	{
		OwnerTransform = owner.Transform;
		Player = owner.GetPlayer;
		BlockSteering = BotOwner_0.Settings.FileSettings.Shoot.BLOCK_STEERING;
	}

	public void Activate()
	{
		FirstTurnSpeed = BotOwner_0.Settings.FileSettings.Move.FIRST_TURN_SPEED;
		FirstTurnBigSpeed = BotOwner_0.Settings.FileSettings.Move.FIRST_TURN_BIG_SPEED;
		TurnSpeedSprint = BotOwner_0.Settings.FileSettings.Move.TURN_SPEED_ON_SPRINT;
	}

	public void ManualFixedUpdate()
	{
		Steering();
	}

	public void SetYAngle(float angle)
	{
		if (!BlockSteering)
		{
			float num = Mathf.DeltaAngle(Player.Rotation.y, angle);
			BotOwner_0.AimingManager.CurrentAiming.RotateY(num);
			Player.Rotate(new Vector2(0f, num));
		}
	}

	public void LookToPoint(Vector3 point)
	{
		LookToPoint(point, BotOwner_0.Settings.FileSettings.Move.BASE_ROTATE_SPEED);
	}

	public void LookToPathDestPoint()
	{
		LookToPathDestPoint(BotOwner_0.Settings.FileSettings.Move.BASE_ROTATE_SPEED);
	}

	public void LookToMovingDirection()
	{
		LookToMovingDirection(BotOwner_0.Settings.FileSettings.Move.BASE_ROTATE_SPEED);
	}

	public void LookToPoint(Vector3 point, float rotateSpeed)
	{
		method_0(rotateSpeed);
		SteeringMode = EBotSteering.ToCustomPoint;
		CustomPoint = point;
	}

	public void LookToMovingDirection(float rotateSpeed)
	{
		method_0(rotateSpeed);
		SteeringMode = EBotSteering.ToMovingDirection;
	}

	public void LookToDirection(Vector3 dir)
	{
		LookToDirection(dir, BotOwner_0.Settings.FileSettings.Move.BASE_ROTATE_SPEED);
	}

	public void LookToDirection(Vector3 dir, float rotateSpeed)
	{
		method_0(rotateSpeed);
		SteeringMode = EBotSteering.Direction;
		if (dir.sqrMagnitude > 0f)
		{
			CustomDirection = dir;
		}
	}

	public void LookToPathDestPoint(float rotateSpeed)
	{
		method_0(rotateSpeed);
		SteeringMode = EBotSteering.ToDestPoint;
	}

	public void SetYByDir(Vector3 dir)
	{
		SetYAngle(CalcYByDir(dir));
	}

	public float CalcYByDir(Vector3 dir)
	{
		float magnitude = dir.magnitude;
		float value = (0f - dir.y) / magnitude;
		value = Mathf.Clamp(value, -1f, 1f);
		value = 57.29578f * Mathf.Asin(value);
		return (0f - Mathf.Abs(value)) * Mathf.Sign(dir.y);
	}

	public void method_0(float rotateSpeed)
	{
		Speed = rotateSpeed;
	}

	public virtual void SetXAngle(float degPerSec)
	{
		if (!BlockSteering)
		{
			float target;
			if (BotOwner_0.LookedTransform != null)
			{
				Vector3 normalized = (BotOwner_0.LookedTransform.position - BotOwner_0.WeaponRoot.position).normalized;
				target = 57.29578f * Mathf.Atan2(normalized.x, normalized.z);
			}
			else
			{
				target = 57.29578f * Mathf.Atan2(LookDirection_1.x, LookDirection_1.z);
			}
			float num = Mathf.DeltaAngle(Player.Rotation.x, target);
			if (BotOwner_0.BotLay.IsLay && num > BotOwner_0.Settings.FileSettings.Look.ANGLE_FOR_GETUP)
			{
				BotOwner_0.BotLay.GetUp(withCheck: true);
			}
			float num2 = degPerSec * Time.deltaTime;
			float num3 = ((!(num > 0f)) ? Mathf.Clamp(num, 0f - num2, 0f) : Mathf.Clamp(num, 0f, num2));
			BotOwner_0.AimingManager.CurrentAiming.RotateX(num3);
			Player.Rotate(new Vector2(num3, 0f), ignoreClamp: true);
		}
	}

	public virtual void Steering()
	{
		Vector3 lookDirection_ = LookDirection_1;
		bool flag = false;
		bool flag2 = false;
		if (BotOwner_0.Mover.Sprinting && BotOwner_0.Mover.HasPathAndNoComplete)
		{
			LookDirection_1 = BotOwner_0.Mover.DirCurPoint;
			if (BotOwner_0.Mover.ShallSlowAtStart && !BotOwner_0.Mover.FirstTurnComplete)
			{
				if (BotOwner_0.Mover.FirstTurnBigSpeed)
				{
					method_0(FirstTurnBigSpeed);
				}
				else
				{
					method_0(FirstTurnSpeed);
				}
			}
			else if (BotOwner_0.Mover.IsMoving)
			{
				method_0(TurnSpeedSprint);
			}
		}
		else
		{
			EBotSteering steeringMode = SteeringMode;
			if (BotOwner_0.Mover.CurrentState == EBotMoverState.NearDoor)
			{
				steeringMode = EBotSteering.ToCustomPoint;
			}
			else
			{
				switch (steeringMode)
				{
				case EBotSteering.ToDestPoint:
					if (BotOwner_0.Destination.HasValue)
					{
						Vector3 lookDirection_2 = BotOwner_0.Destination.Value - OwnerTransform.position;
						if (lookDirection_2.sqrMagnitude > 0f)
						{
							LookDirection_1 = lookDirection_2;
						}
						if (Mathf.Abs(LookDirection_1.y) < 0.001f)
						{
							flag = true;
						}
					}
					goto IL_01e5;
				case EBotSteering.ToMovingDirection:
					if (!CanSteerToMovingDirection())
					{
						return;
					}
					flag = true;
					LookDirection_1 = BotOwner_0.Mover.DirCurPoint;
					goto IL_01e5;
				case EBotSteering.ToCustomPoint:
					break;
				case EBotSteering.Direction:
					LookDirection_1 = CustomDirection;
					if (Mathf.Abs(LookDirection_1.y) < 0.001f)
					{
						flag = true;
					}
					goto IL_01e5;
				default:
					goto IL_01e5;
				}
			}
			flag2 = true;
			LookDirection_1 = CustomPoint - BotOwner_0.WeaponRoot.position;
		}
		goto IL_01e5;
		IL_01e5:
		Vector3 lookDirection_3 = LookDirection_1;
		if (Mathf.Abs(lookDirection_3.x) <= Mathf.Epsilon && Mathf.Abs(lookDirection_3.z) <= Mathf.Epsilon)
		{
			if (!IsErrorLogged)
			{
				IsErrorLogged = true;
			}
			LookDirection_1 = lookDirection_;
		}
		SetXAngle(Speed);
		if (flag)
		{
			SetYAngle(0f);
		}
		else if (flag2)
		{
			SetYByDir(LookDirection_1);
		}
		else
		{
			SetYByDir(LookDirection_1);
		}
	}

	public virtual bool CanSteerToMovingDirection()
	{
		return Player.MovementContext.Velocity.magnitude >= 0.04f;
	}
}
