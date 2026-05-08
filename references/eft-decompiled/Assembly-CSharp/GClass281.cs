using System;
using EFT;
using UnityEngine;

public class GClass281 : GClass177<GClass27>
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1 = 0.6f;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public float Float_3 = 0.1f;

	[NonSerialized]
	public GClass187 Gclass187_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	public CustomNavigationPoint CustomNavigationPoint_0 => BotOwner_0.Memory.CurCustomCoverPoint;

	public GClass281(BotOwner bot)
		: base(bot)
	{
		Gclass187_0 = new GClass187(bot);
	}

	public Vector3? GetPoint(GClass27 data)
	{
		if (data != null && data.PointToShoot.HasValue)
		{
			return data.PointToShoot;
		}
		return BotOwner_0.SuppressShoot.GetPoint();
	}

	public override void UpdateNodeByBrain(GClass27 data)
	{
		Vector3? point = GetPoint(data);
		if (!point.HasValue)
		{
			return;
		}
		Vector3 shootResult;
		bool flag = method_2(point.Value, out shootResult);
		point = shootResult;
		BotUnderbarrelLauncherController underbarrelLauncherController = BotOwner_0.WeaponManager.UnderbarrelLauncherController;
		if (underbarrelLauncherController.IsActive)
		{
			if (underbarrelLauncherController.NeedToReload())
			{
				underbarrelLauncherController.TryReload();
				return;
			}
		}
		else
		{
			if (underbarrelLauncherController.CanSwitchInFight(BotOwner_0))
			{
				underbarrelLauncherController.TryEnable();
			}
			if (!BotOwner_0.WeaponManager.HaveBullets)
			{
				BotOwner_0.WeaponManager.Reload.TryReload();
				return;
			}
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			if (BotOwner_0.Memory.CurCustomCoverPoint.CoverLevel != CoverLevel.Stay)
			{
				BotOwner_0.StopMove();
				BotOwner_0.SetPose(1f);
				if (flag)
				{
					Gclass187_0.UpdateNodeByBrain(data);
				}
				return;
			}
			bool num = Vector3.Dot(point.Value - BotOwner_0.Position, CustomNavigationPoint_0.ToWallVector) > 0f;
			if (num && !flag)
			{
				method_0();
			}
			if (!num)
			{
				BotOwner_0.Tilt.Stop();
			}
			Vector3 firePosition = BotOwner_0.Memory.CurCustomCoverPoint.FirePosition;
			Vector3 vector = BotOwner_0.Position - firePosition;
			vector.y = 0f;
			if (vector.sqrMagnitude < 0.4f)
			{
				if (flag)
				{
					Gclass187_0.UpdateNodeByBrain(data);
				}
				return;
			}
		}
		else
		{
			BotOwner_0.DoorOpener.UpdateDoorInteractionStatus();
			if (flag)
			{
				Gclass187_0.UpdateNodeByBrain(data);
				return;
			}
		}
		BotOwner_0.Steering.LookToPoint(point.Value);
	}

	public void method_0()
	{
		if (CustomNavigationPoint_0 == null || Float_0 > Time.time)
		{
			return;
		}
		Float_0 = Time.time + 1f;
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		switch (CustomNavigationPoint_0.CoverLevel)
		{
		case CoverLevel.Sit:
		case CoverLevel.Lay:
		{
			if (method_1())
			{
				BotOwner_0.SetPose(1f);
			}
			Vector3 firePosition2 = CustomNavigationPoint_0.FirePosition;
			firePosition2.y = CustomNavigationPoint_0.Position.y;
			if ((double)(firePosition2 - BotOwner_0.Position).sqrMagnitude > 0.01)
			{
				BotOwner_0.GoToPoint(firePosition2, slowAtTheEnd: true, 0.3f);
				BotOwner_0.Mover.SetTargetMoveSpeed(0.2f);
			}
			break;
		}
		case CoverLevel.Stay:
		{
			BotOwner_0.Mover.SetTargetMoveSpeed(0.001f);
			Vector3 firePosition = CustomNavigationPoint_0.FirePosition;
			if (CustomNavigationPoint_0.StrategyType == PointWithNeighborType.cover && goalEnemy != null)
			{
				Vector3 a = GClass855.NormalizeFastSelf(goalEnemy.CurrPosition - CustomNavigationPoint_0.Position);
				Vector3 toWallVector = CustomNavigationPoint_0.ToWallVector;
				if (GClass855.IsAngLessNormalized(a, toWallVector, 0.7071068f))
				{
					BotOwner_0.Tilt.Set(CustomNavigationPoint_0.TiltType);
				}
			}
			if (Time.time - BotOwner_0.Memory.BotCurrentCoverInfo.LastChangeCoverStatusTime > Float_1)
			{
				BotOwner_0.GoToPoint(firePosition, slowAtTheEnd: true, 0.1f);
			}
			if (method_1())
			{
				BotOwner_0.SetPose(1f);
			}
			break;
		}
		}
		if (BotOwner_0.Memory.BotCurrentCoverInfo.IsOutOfCoverComplete)
		{
			BotOwner_0.Memory.BotCurrentCoverInfo.CoverStatus = ShootCoverStatus.shooting;
			BotOwner_0.StopMove();
		}
	}

	public bool method_1()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return true;
		}
		if (goalEnemy.CanShoot && goalEnemy.IsVisible)
		{
			return false;
		}
		return true;
	}

	public bool method_2(Vector3 toShoot, out Vector3 shootResult)
	{
		if (Float_2 > Time.time)
		{
			shootResult = Vector3_0;
			return Bool_0;
		}
		Float_2 = Time.time + 0.2f;
		float b = 0.4f;
		float num = GClass856.Random(0.1f, b);
		float num2 = GClass856.Random(0.1f, b);
		float num3 = GClass856.Random(0.1f, b);
		toShoot = new Vector3(toShoot.x + num, toShoot.y + num2, toShoot.z + num3);
		Vector3 v = toShoot - BotOwner_0.LookSensor.ShootStartPos;
		Vector3 vector = GClass855.NormalizeFastSelf(v);
		float num4 = v.magnitude;
		if (num4 > 5f)
		{
			num4 *= 0.5f;
		}
		Vector3 start = BotOwner_0.LookSensor.ShootStartPos + Float_3 * vector;
		Vector3 vector2 = BotOwner_0.LookSensor.ShootStartPos + num4 * vector;
		Bool_0 = !Physics.Linecast(start, vector2, LayerMaskClass.HighPolyWithTerrainMask);
		if (Bool_0)
		{
			Vector3_0 = vector2;
		}
		else
		{
			Vector3_0 = toShoot;
		}
		shootResult = Vector3_0;
		if (Bool_0)
		{
			Float_2 = Time.time + 2.5f;
		}
		return Bool_0;
	}
}
