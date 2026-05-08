using System;
using EFT;
using UnityEngine;

public class GClass277 : GClass276
{
	public const float WAIT_FOR_TILT = 0.6f;

	public const float BOT_CLOSE = 0.14f;

	public const float LAY_CHECK_DELTA = 6f;

	[NonSerialized]
	public const float Float_6 = 32f;

	[NonSerialized]
	public const float Float_7 = 0.5f;

	[NonSerialized]
	public const float Float_8 = 3f;

	public ShootFromCoverType ShootType;

	[NonSerialized]
	public float Float_9;

	[NonSerialized]
	public bool Bool_0;

	public CustomNavigationPoint CustomNavigationPoint_0 => BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint;

	public GClass277(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass28 data)
	{
		if (CustomNavigationPoint_0 == null)
		{
			method_1(data);
			return;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			UpdateUseCover(data);
		}
		else if (Vector3.Dot(goalEnemy.Direction, CustomNavigationPoint_0.ToWallVector) > 0f)
		{
			UpdateUseCover(data);
		}
		else
		{
			method_1(data);
		}
	}

	public void UpdateUseCover(GClass26 data)
	{
		if (BotOwner_0.WeaponManager.UnderbarrelLauncherController.CanSwitchInFight(BotOwner_0))
		{
			BotOwner_0.WeaponManager.UnderbarrelLauncherController.TryEnable();
		}
		bool num = method_4(updateVisibility: true);
		method_10();
		if (num)
		{
			Gclass178_0.UpdateNodeByBrain(data as GClass27);
		}
	}

	public bool method_4(bool updateVisibility)
	{
		if (CustomNavigationPoint_0 == null)
		{
			return false;
		}
		BotOwner_0.BotLight.TurnOn();
		BotOwner_0.Sprint(val: false);
		BotOwner_0.Memory.BotCurrentCoverInfo.TryCheckSafe();
		if (updateVisibility)
		{
			if (BotOwner_0.Memory.GoalEnemy != null)
			{
				if (BotOwner_0.Memory.GoalEnemy.IsVisible)
				{
					if (BotOwner_0.Memory.GoalEnemy.LastPartToShoot != null)
					{
						BotOwner_0.Steering.LookToPoint(BotOwner_0.Memory.GoalEnemy.LastPartToShoot.Position);
					}
					else
					{
						BotOwner_0.Steering.LookToPoint(BotOwner_0.Memory.GoalEnemy.GetBodyPartPosition());
					}
				}
				else
				{
					BotOwner_0.LookData.SetLookPointByHearing();
				}
			}
			else
			{
				BotOwner_0.Steering.LookToDirection(BotOwner_0.Memory.CurCustomCoverPoint.ToWallVector);
			}
		}
		switch (BotOwner_0.Memory.BotCurrentCoverInfo.CoverStatus)
		{
		case ShootCoverStatus.covered:
			method_9();
			if (BotOwner_0.WeaponManager.UnderbarrelLauncherController.IsActive)
			{
				if (BotOwner_0.WeaponManager.UnderbarrelLauncherController.NeedToReload())
				{
					BotOwner_0.WeaponManager.UnderbarrelLauncherController.TryReload();
				}
			}
			else if (!BotOwner_0.WeaponManager.HaveBullets)
			{
				BotOwner_0.WeaponManager.Reload.TryReload();
			}
			if (method_11())
			{
				BotOwner_0.Memory.BotCurrentCoverInfo.CoverStatus = ShootCoverStatus.changingToShot;
				Bool_0 = false;
			}
			break;
		case ShootCoverStatus.changingToShot:
			if (method_8())
			{
				BotOwner_0.Memory.BotCurrentCoverInfo.CoverStatus = ShootCoverStatus.shooting;
				return false;
			}
			method_6();
			break;
		case ShootCoverStatus.changingToCover:
			method_5();
			break;
		case ShootCoverStatus.shooting:
			method_9();
			if (Gclass274_0.UpdateTryThrow())
			{
				return false;
			}
			if (BotOwner_0.Memory.BotCurrentCoverInfo.ShouldGoToCover() || BotOwner_0.WeaponManager.Reload.Reloading)
			{
				Bool_0 = false;
			}
			if (method_8())
			{
				return true;
			}
			if (BotOwner_0.Memory.GoalEnemy != null)
			{
				BotOwner_0.Steering.LookToPoint(BotOwner_0.Memory.GoalEnemy.CurrPosition);
			}
			break;
		}
		return false;
	}

	public void method_5()
	{
		if (CustomNavigationPoint_0 == null)
		{
			return;
		}
		switch (CustomNavigationPoint_0.CoverLevel)
		{
		case CoverLevel.Sit:
		case CoverLevel.Lay:
		{
			float pose = BotOwner_0.Memory.BotCurrentCoverInfo.CalcSit();
			BotOwner_0.SetPose(pose);
			break;
		}
		case CoverLevel.Stay:
			if (BotOwner_0.Memory.BotCurrentCoverInfo.CoverStatus != ShootCoverStatus.covered)
			{
				if (BotOwner_0.WeaponManager.Grenades.ThrowindNow)
				{
					BotOwner_0.StopMove();
					return;
				}
				BotOwner_0.Tilt.Stop();
				Vector3 position = CustomNavigationPoint_0.Position;
				if (!Bool_0)
				{
					Bool_0 = true;
					BotOwner_0.GoToPoint(position, slowAtTheEnd: true, 0.1f);
				}
			}
			break;
		}
		if (BotOwner_0.Memory.BotCurrentCoverInfo.IsCoveringComplete)
		{
			BotOwner_0.Memory.BotCurrentCoverInfo.CoverStatus = ShootCoverStatus.covered;
		}
	}

	public void method_6()
	{
		if (CustomNavigationPoint_0 == null)
		{
			return;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		switch (CustomNavigationPoint_0.CoverLevel)
		{
		case CoverLevel.Sit:
		case CoverLevel.Lay:
		{
			if (method_2())
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
			firePosition.y = CustomNavigationPoint_0.Position.y;
			if (CustomNavigationPoint_0.StrategyType == PointWithNeighborType.cover && goalEnemy != null)
			{
				Vector3 a = GClass855.NormalizeFastSelf(goalEnemy.CurrPosition - CustomNavigationPoint_0.Position);
				Vector3 toWallVector = CustomNavigationPoint_0.ToWallVector;
				if (GClass855.IsAngLessNormalized(a, toWallVector, 0.7071068f))
				{
					BotOwner_0.Tilt.Set(CustomNavigationPoint_0.TiltType);
				}
			}
			if (Time.time - BotOwner_0.Memory.BotCurrentCoverInfo.LastChangeCoverStatusTime > 0.6f)
			{
				BotOwner_0.GoToPoint(firePosition, slowAtTheEnd: true, 0.1f);
			}
			if (method_2())
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

	public bool method_7()
	{
		if (!CustomNavigationPoint_0.AlwaysGood)
		{
			return false;
		}
		if (BotOwner_0.BotLay.IsLay)
		{
			return false;
		}
		if (Float_9 < Time.time)
		{
			Float_9 = Time.time + 6f;
			if (BotOwner_0.Memory.GoalEnemy != null && BotOwner_0.BotLay.CanShootPos(BotOwner_0.Memory.GoalEnemy.EnemyLastPosition, CustomNavigationPoint_0.AlwaysGood, withFriendlyFire: false))
			{
				BotOwner_0.BotLay.TryLay();
				return true;
			}
		}
		return false;
	}

	public bool method_8()
	{
		switch (ShootType)
		{
		default:
			return true;
		case ShootFromCoverType.ToPoint:
			return BotOwner_0.Memory.BotCurrentCoverInfo.IsOutOfCoverComplete;
		case ShootFromCoverType.ToPlayer:
			if (BotOwner_0.Memory.GoalEnemy == null)
			{
				return false;
			}
			if (BotOwner_0.WeaponManager.UnderbarrelLauncherController.IsActive)
			{
				return true;
			}
			if (BotOwner_0.Memory.GoalEnemy.IsVisible)
			{
				return BotOwner_0.Memory.GoalEnemy.CanShoot;
			}
			return false;
		}
	}

	public void method_9()
	{
		BotOwner_0.Mover.Stop();
		BotOwner_0.SetTargetMoveSpeed(0f);
	}

	public void method_10()
	{
		if (BotOwner_0.Settings.FileSettings.Cover.SHALL_CHANGE_COVER_IF_CAN_SHOOT && BotOwner_0.Memory.BotCurrentCoverInfo.CoverStatus == ShootCoverStatus.shooting)
		{
			EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
			if (goalEnemy != null && Time.time - goalEnemy.PersonalLastSeenTime < 3f && Time.time - BotOwner_0.Memory.BotCurrentCoverInfo.LastChangeCoverStatusTime > 0.5f && BotOwner_0.ShootData.LastTriggerPressd - BotOwner_0.Memory.ComeToCoverTime > BotOwner_0.Settings.FileSettings.Cover.CHANGE_COVER_IF_CANT_SHOOT_SEC)
			{
				BotOwner_0.Memory.Spotted(byHit: false);
			}
		}
	}

	public bool method_11()
	{
		if (BotOwner_0.Memory.BotCurrentCoverInfo == null)
		{
			return false;
		}
		if (BotOwner_0.WeaponManager.Reload.Reloading)
		{
			EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
			if (goalEnemy.IsVisible && goalEnemy.CanShoot && goalEnemy.Distance < 32f)
			{
				BotOwner_0.WeaponManager.Reload.TryStopReload();
			}
			return false;
		}
		BotUnderbarrelLauncherController underbarrelLauncherController = BotOwner_0.WeaponManager.UnderbarrelLauncherController;
		if (BotOwner_0.WeaponManager.Reload.BulletCount <= 0 && underbarrelLauncherController.TryGetUnderbarrelWeapon(out var underbarrelWeapon) && underbarrelLauncherController.AmmoQuery(underbarrelWeapon).Count == 0)
		{
			return false;
		}
		return BotOwner_0.Memory.BotCurrentCoverInfo.nextToLookTime < Time.time;
	}
}
