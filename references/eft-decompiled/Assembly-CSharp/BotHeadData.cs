using System;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class BotHeadData : GClass429
{
	public enum EHeadState
	{
		Disabled,
		Patrol,
		EnemyVisibility
	}

	public enum EBotHeadDirection
	{
		Turn,
		LookStraight
	}

	[NonSerialized]
	public float RotationSpeed;

	[NonSerialized]
	public float TargetHorizontalRotation;

	[NonSerialized]
	public EBotHeadDirection DirType;

	[NonSerialized]
	public float NextHeadChange;

	[NonSerialized]
	public float UpdatePointTimer;

	[field: NonSerialized]
	public EHeadState EHeadState_0 { get; set; }

	public BotGlobalPatrolSettings BotGlobalPatrolSettings_0 => BotOwner_0.Settings.FileSettings.Patrol;

	public float Single_0 => BotOwner_0.GetPlayer.HeadRotation.y;

	public BotHeadData([NotNull] BotOwner owner)
		: base(owner)
	{
		EHeadState_0 = EHeadState.Disabled;
	}

	public void ManualUpdate()
	{
		method_1();
		method_3();
	}

	public void method_0(Vector3 target)
	{
		Vector3 to = target - BotOwner_0.LookSensor.HeadPoint;
		TargetHorizontalRotation = Vector3.SignedAngle(BotOwner_0.LookDirection, to, Vector3.up);
	}

	public void method_1()
	{
		switch (EHeadState_0)
		{
		case EHeadState.Disabled:
			method_4();
			break;
		case EHeadState.Patrol:
			method_8();
			break;
		case EHeadState.EnemyVisibility:
			method_6();
			break;
		}
	}

	public void method_2(EHeadState newState)
	{
		if (EHeadState_0 != newState)
		{
			EHeadState_0 = newState;
			switch (newState)
			{
			case EHeadState.Patrol:
				method_7();
				break;
			case EHeadState.EnemyVisibility:
				method_5();
				break;
			case EHeadState.Disabled:
				break;
			}
		}
	}

	public void method_3()
	{
		float num = Time.deltaTime * RotationSpeed;
		if (!(Math.Abs(TargetHorizontalRotation - Single_0) < num))
		{
			float num2 = ((TargetHorizontalRotation != 0f) ? Mathf.Sign(TargetHorizontalRotation) : Mathf.Sign(Single_0));
			num *= num2;
			BotOwner_0.GetPlayer.Look(num, 0f, withReturn: false);
		}
	}

	public void method_4()
	{
		if (BotOwner_0.Memory.IsPeace)
		{
			method_2(EHeadState.Patrol);
		}
		else if (BotOwner_0.EnemiesController.BestObservedEnemy != null && BotOwner_0.EnemiesController.BestObservedEnemy.VisibilityLevel >= BotOwner_0.Settings.FileSettings.Look.VISIBILITY_LEVEL_TO_TURN_HEAD)
		{
			method_2(EHeadState.EnemyVisibility);
		}
		else
		{
			TargetHorizontalRotation = 0f;
		}
	}

	public void method_5()
	{
		RotationSpeed = BotOwner_0.Settings.FileSettings.Look.HEAD_ROTATE_SPEED;
	}

	public void method_6()
	{
		EnemyInfo bestObservedEnemy = BotOwner_0.EnemiesController.BestObservedEnemy;
		if (bestObservedEnemy == null)
		{
			TargetHorizontalRotation = 0f;
			method_2(BotOwner_0.Memory.IsPeace ? EHeadState.Patrol : EHeadState.Disabled);
		}
		else if (bestObservedEnemy.VisibilityLevel >= BotOwner_0.Settings.FileSettings.Look.VISIBILITY_LEVEL_TO_TURN_HEAD)
		{
			method_0(bestObservedEnemy.CurrPosition);
		}
	}

	public void method_7()
	{
		RotationSpeed = BotOwner_0.Settings.FileSettings.Patrol.HEAD_TURN_SPEED;
		DirType = EBotHeadDirection.Turn;
		NextHeadChange = Time.time + GClass856.GreateRandom(BotOwner_0.Settings.FileSettings.Patrol.HEAD_FRONT_PERIOD_TIME);
	}

	public void method_8()
	{
		if (!BotOwner_0.Memory.IsPeace)
		{
			method_2(EHeadState.Disabled);
			return;
		}
		EnemyInfo bestObservedEnemy = BotOwner_0.EnemiesController.BestObservedEnemy;
		if (bestObservedEnemy != null && bestObservedEnemy.VisibilityLevel >= BotOwner_0.Settings.FileSettings.Look.VISIBILITY_LEVEL_TO_TURN_HEAD)
		{
			method_2(EHeadState.EnemyVisibility);
			return;
		}
		switch (DirType)
		{
		case EBotHeadDirection.LookStraight:
			TargetHorizontalRotation = 0f;
			if (NextHeadChange < Time.time)
			{
				NextHeadChange = Time.time + GClass856.GreateRandom(BotGlobalPatrolSettings_0.HEAD_FRONT_PERIOD_TIME);
				DirType = EBotHeadDirection.Turn;
				Debug.Log("[head] patrol bot go to turn state");
			}
			break;
		case EBotHeadDirection.Turn:
			if (UpdatePointTimer < Time.time)
			{
				UpdatePointTimer = Time.time + GClass856.GreateRandom(BotGlobalPatrolSettings_0.HEAD_TURN_PERIOD_TIME);
				if (!BotOwner_0.AimingManager.CurrentAiming.HardAim)
				{
					float num = (GClass856.IsTrue100(50f) ? GClass856.GreateRandom(BotGlobalPatrolSettings_0.HEAD_ANG_ROTATE) : (0f - GClass856.GreateRandom(BotGlobalPatrolSettings_0.HEAD_ANG_ROTATE)));
					TargetHorizontalRotation = 0f - num;
				}
			}
			if (NextHeadChange < Time.time)
			{
				NextHeadChange = Time.time + GClass856.GreateRandom(BotGlobalPatrolSettings_0.HEAD_FRONT_PERIOD_TIME);
				DirType = EBotHeadDirection.LookStraight;
			}
			break;
		}
	}
}
