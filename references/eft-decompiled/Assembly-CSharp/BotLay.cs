using System;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class BotLay : GClass429
{
	public const float LAY_SHOOT_HEIGHT = 0.14f;

	[NonSerialized]
	public int DamageTimes;

	[NonSerialized]
	public float LayTime;

	[NonSerialized]
	public float NextAttackCheckCanLay;

	[NonSerialized]
	public float NextPosibleCheckCanLay;

	[NonSerialized]
	public float NextPosibleGetUp;

	[NonSerialized]
	public float Tan;

	[NonSerialized]
	public MovementContext MovementContext;

	[NonSerialized]
	public float EndPeriodNoCheck;

	public bool IsLay
	{
		get
		{
			return MovementContext.IsInPronePose;
		}
		set
		{
			if (IsLay == value)
			{
				return;
			}
			if (value)
			{
				LayTime = Time.time;
				BotOwner_0.Mover.SetPose(0f);
				BotOwner_0.Mover.DoProne(val: true);
				BotOwner_0.GetPlayer.MovementContext.OnCantRotate += method_0;
				BotOwner_0.ShootData.NullShootsBetween();
				BotOwner_0.BotRun.StopRunAnyway();
				NextPosibleGetUp = Time.time + BotOwner_0.Settings.FileSettings.Lay.DELTA_GETUP;
			}
			else
			{
				BotOwner_0.GetPlayer.MovementContext.OnCantRotate -= method_0;
				if (MovementContext.IsInPronePose)
				{
					NextPosibleCheckCanLay = Time.time + BotOwner_0.Settings.FileSettings.Lay.DELTA_AFTER_GETUP;
					NextPosibleGetUp = Time.time + BotOwner_0.Settings.FileSettings.Lay.DELTA_GETUP;
				}
				if (BotOwner_0.Mover.TargetPose <= 0f)
				{
					BotOwner_0.Mover.SetPose(0.4f);
				}
				BotOwner_0.Mover.DoProne(val: false);
			}
			this.OnLay?.Invoke(value);
		}
	}

	public bool CanProne
	{
		get
		{
			if (!BotOwner_0.Settings.FileSettings.Lay.SHALL_LAY_WITHOUT_CHECK)
			{
				return BotOwner_0.GetPlayer.MovementContext.CanProne;
			}
			return true;
		}
	}

	public event Action<bool> OnLay;

	public BotLay(BotOwner owner)
		: base(owner)
	{
		MovementContext = BotOwner_0.GetPlayer.MovementContext;
		Tan = Mathf.Tan(Player.PlayerMovementConstantsClass.PRONE_POSE_ROTATION_PITCH_RANGE.y * (MathF.PI / 180f));
		NextPosibleCheckCanLay = Time.time + 10f;
	}

	public void Activate()
	{
		NextPosibleCheckCanLay = Time.time + 8f;
	}

	public void DelayPosibleLayFor(float period)
	{
		NextPosibleCheckCanLay = Time.time + period;
	}

	public bool CanLayByPeriod()
	{
		return NextPosibleCheckCanLay < Time.time;
	}

	public bool TryLay()
	{
		if (BotOwner_0.ArtilleryDangerPlace.IsActive)
		{
			IsLay = true;
			BotOwner_0.BotsGroup.GroupLaying.StartLay();
			return true;
		}
		bool haveEnemy;
		if (!(haveEnemy = BotOwner_0.Memory.HaveEnemy) && !BotOwner_0.Settings.FileSettings.Lay.IF_NO_ENEMY)
		{
			return false;
		}
		if (haveEnemy && BotOwner_0.Memory.GoalEnemy.Distance < BotOwner_0.Settings.FileSettings.Lay.DIST_LAY_CHECK)
		{
			return false;
		}
		if (CanLayByPeriod())
		{
			DelayPosibleLayFor(BotOwner_0.Settings.FileSettings.Lay.DELTA_LAY_CHECK);
			if (CanProne && CheckDistGood())
			{
				IsLay = true;
				BotOwner_0.BotsGroup.GroupLaying.StartLay();
				return true;
			}
		}
		return false;
	}

	public bool CanShootPos([CanBeNull] EnemyInfo goalEnemy, bool withCheckShoot, bool withFriendlyFire)
	{
		if (goalEnemy == null)
		{
			return false;
		}
		if (!withCheckShoot)
		{
			return true;
		}
		Vector3 partToShoot = goalEnemy.GetPartToShoot();
		return CanShootPos(partToShoot, withCheckShoot: true, withFriendlyFire);
	}

	public bool CanShootPos(Vector3 goalEnemy, bool withCheckShoot, bool withFriendlyFire)
	{
		if (!withCheckShoot)
		{
			return true;
		}
		Vector3 vector = BotOwner_0.Transform.position + Vector3.up * 0.14f;
		Vector3 vector2 = goalEnemy + Vector3.up - vector;
		Vector3 vector3 = vector2;
		vector3.y = vector.y;
		float num = Vector3.Angle(vector3, vector2);
		float lAY_DOWN_ANG_SHOOT = LocalBotSettingsProviderClass.Core.LAY_DOWN_ANG_SHOOT;
		if (num > Mathf.Abs(lAY_DOWN_ANG_SHOOT))
		{
			return false;
		}
		if (!GClass369.CanShootToTarget(new ShootPointClass(goalEnemy), vector, BotOwner_0.LookSensor.Mask, doubleSide: true))
		{
			return false;
		}
		if (withFriendlyFire)
		{
			return !BotOwner_0.ShootData.CheckFriendlyFire(vector, goalEnemy);
		}
		return true;
	}

	public void GetUp(bool withCheck)
	{
		if (!(NextPosibleGetUp > Time.time))
		{
			IsLay = false;
		}
	}

	public void CheckGetUp()
	{
		if (method_1())
		{
			GetUp(withCheck: true);
		}
	}

	public void Damaged()
	{
		DamageTimes++;
		if (DamageTimes > BotOwner_0.Settings.FileSettings.Lay.DAMAGE_TIME_TO_GETUP)
		{
			GetUp(withCheck: true);
		}
	}

	public bool CheckDistGood()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null)
		{
			if (goalEnemy.Distance > BotOwner_0.Settings.FileSettings.Lay.DIST_ENEMY_GETUP_LAY)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public bool CanLayWithoutCheck()
	{
		return BotOwner_0.Settings.FileSettings.Lay.SHALL_LAY_WITHOUT_CHECK;
	}

	public void method_0(ECantRotate cause)
	{
		if (BotOwner_0.Settings.FileSettings.Lay.SHALL_GETUP_ON_ROTATE && cause == ECantRotate.NotGround)
		{
			GetUp(withCheck: false);
		}
	}

	public bool method_1()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null)
		{
			Vector3 lhs = BotOwner_0.Memory.GoalEnemy.CurrPosition - BotOwner_0.Transform.position;
			float sqrMagnitude = lhs.sqrMagnitude;
			if (goalEnemy.IsVisible)
			{
				Vector3 lookDirection = BotOwner_0.LookDirection;
				float num = Vector3.Dot(lhs, lookDirection);
				float num2 = Mathf.Sqrt(lhs.x * lhs.x + lhs.z * lhs.z);
				if (Mathf.Abs(lhs.y) / num2 > Tan)
				{
					GetUp(withCheck: false);
					return false;
				}
				if (num < 0f)
				{
					BotOwner_0.DangerPointsData.SetAllDangerNull();
					GetUp(withCheck: false);
					return false;
				}
				if (sqrMagnitude < BotOwner_0.Settings.FileSettings.Lay.DIST_ENEMY_NULL_DANGER_LAY_SQRT)
				{
					BotOwner_0.DangerPointsData.SetAllDangerNull();
				}
			}
			if (sqrMagnitude < BotOwner_0.Settings.FileSettings.Lay.DIST_ENEMY_GETUP_LAY_SQRT)
			{
				GetUp(withCheck: false);
				return false;
			}
		}
		float num3 = Time.time - LayTime;
		if (num3 > BotOwner_0.Settings.FileSettings.Lay.CLEAR_POINTS_OF_SCARE_SEC)
		{
			BotOwner_0.DangerPointsData.SetAllDangerNull();
		}
		return num3 > BotOwner_0.Settings.FileSettings.Lay.MAX_LAY_TIME;
	}

	public void Dispose()
	{
		BotOwner_0.GetPlayer.MovementContext.OnCantRotate -= method_0;
	}
}
