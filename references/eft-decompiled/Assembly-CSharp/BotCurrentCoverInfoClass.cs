using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class BotCurrentCoverInfoClass
{
	[NonSerialized]
	public const float Float_0 = 2f;

	[NonSerialized]
	public const float Float_1 = 1.2f;

	public float nextToLookTime;

	public int ShootsFromCover;

	public GClass423 CoverWithPath = new GClass423();

	[NonSerialized]
	public ShootCoverStatus ShootCoverStatus_0;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public AttackFromCoverType AttackFromCoverType_0;

	[NonSerialized]
	public BotOwner BotOwner_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public int Int_2;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_7;

	[NonSerialized]
	[CompilerGenerated]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_8;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_9;

	[NonSerialized]
	[CompilerGenerated]
	public CustomNavigationPoint CustomNavigationPoint_1;

	public float ComeToCoverTime
	{
		[CompilerGenerated]
		get
		{
			return Float_7;
		}
		[CompilerGenerated]
		set
		{
			Float_7 = value;
		}
	}

	public CustomNavigationPoint LastCover
	{
		[CompilerGenerated]
		get
		{
			return CustomNavigationPoint_0;
		}
		[CompilerGenerated]
		set
		{
			CustomNavigationPoint_0 = value;
		}
	}

	public float LeaveCoverTime
	{
		[CompilerGenerated]
		get
		{
			return Float_8;
		}
		[CompilerGenerated]
		set
		{
			Float_8 = value;
		}
	}

	public float LastChangeCoverStatusTime
	{
		[CompilerGenerated]
		get
		{
			return Float_9;
		}
		[CompilerGenerated]
		set
		{
			Float_9 = value;
		}
	}

	public ShootCoverStatus CoverStatus
	{
		get
		{
			return ShootCoverStatus_0;
		}
		set
		{
			if (ShootCoverStatus_0 == value)
			{
				return;
			}
			ShootsFromCover = 0;
			LastChangeCoverStatusTime = Time.time;
			ShootCoverStatus_0 = value;
			switch (ShootCoverStatus_0)
			{
			case ShootCoverStatus.changingToShot:
				Float_5 = Time.time + BotOwner_0.Settings.FileSettings.Cover.HIDE_TO_COVER_TIME;
				break;
			case ShootCoverStatus.covered:
				break;
			case ShootCoverStatus.changingToCover:
				Float_4 = Time.time + BotOwner_0.Settings.FileSettings.Cover.HIDE_TO_COVER_TIME;
				break;
			case ShootCoverStatus.shooting:
				AttackFromCoverType_0 = AttackFromCoverType.shoot;
				if (BotOwner_0.WeaponManager.Grenades.HaveGrenade)
				{
					AttackFromCoverType_0 = AttackFromCoverType.grenade;
				}
				nextToLookTime = BotOwner_0.Settings.Current.WaitInCoverBetweenShotsSecCurrent + Time.time;
				break;
			}
		}
	}

	public CustomNavigationPoint CovPoint
	{
		[CompilerGenerated]
		get
		{
			return CustomNavigationPoint_1;
		}
		[CompilerGenerated]
		set
		{
			CustomNavigationPoint_1 = value;
		}
	}

	public bool IsCoveringComplete => Float_4 < Time.time;

	public bool IsOutOfCoverComplete => Float_5 < Time.time;

	public bool HaveCover => CovPoint != null;

	public BotCurrentCoverInfoClass(BotOwner player)
	{
		BotOwner_0 = player;
	}

	public float CalcSit()
	{
		if (Float_6 > 0f)
		{
			Float_6 -= Time.deltaTime * 2f;
		}
		return Mathf.Clamp(Float_6, 0f, 1.2f);
	}

	public void TryCheckSafe()
	{
		if (Float_2 < Time.time)
		{
			Float_2 = Time.time + BotOwner_0.Settings.FileSettings.Cover.TIME_CHECK_SAFE;
			method_1();
		}
	}

	public void ComeToCover()
	{
		if (CovPoint != null)
		{
			BotOwner_0.BotLight.TurnOff();
			BotOwner_0.ShootData.NullShootsBetween();
			BotOwner_0.BotRun.StopRunAnyway();
			CoverStatus = ShootCoverStatus.shooting;
			Float_4 = 0f;
			Float_5 = 0f;
			ComeToCoverTime = Time.time;
			BotOwner_0.BotPersonalStats.CoverTaken();
		}
	}

	public void Leaved()
	{
		Int_0 = 0;
		Int_1 = 0;
		Int_2 = 0;
		LeaveCoverTime = Time.time;
		SetCover(null);
		ShootCoverStatus_0 = ShootCoverStatus.shooting;
	}

	public bool ShouldGoToCover()
	{
		bool flag = false;
		switch (AttackFromCoverType_0)
		{
		case AttackFromCoverType.grenade:
			flag = nextToLookTime < Time.time || BotOwner_0.WeaponManager.Grenades.NearLastThrow;
			break;
		case AttackFromCoverType.shoot:
			flag = ShootsFromCover > BotOwner_0.Settings.FileSettings.Shoot.SHOOT_FROM_COVER || nextToLookTime < Time.time || BotOwner_0.WeaponManager.Reload.BulletCount == 0;
			break;
		}
		if (flag)
		{
			CoverStatus = ShootCoverStatus.changingToCover;
			Float_6 = 1.2f;
			return true;
		}
		return false;
	}

	public bool CanChangeWayByDist()
	{
		if (CovPoint == null)
		{
			return true;
		}
		return (CovPoint.Position - BotOwner_0.Transform.position).sqrMagnitude > BotOwner_0.Settings.FileSettings.Cover.DIST_CANT_CHANGE_WAY_SQR;
	}

	public void Spotted()
	{
		SetCover(null);
	}

	public void SetCover(CustomNavigationPoint value, bool withSetOwner = true)
	{
		if (CovPoint != null)
		{
			LastCover = CovPoint;
		}
		if (CovPoint != null)
		{
			if (CovPoint == value)
			{
				return;
			}
			CovPoint.SetFree();
		}
		if (value != null && !value.IsFreeById(BotOwner_0.Id))
		{
			bool num = GClass398.Instance.IsTraceEnable();
			bool flag = false;
			if (num)
			{
				string message = string.Format("Set target point to bot{0}.  pointId:{1}   prev:{2}  withSetOwner:{3} Layer:{4} Decision:{5}", BotOwner_0.Id, value.Id, (CovPoint != null) ? CovPoint.Id.ToString() : "null", withSetOwner, BotOwner_0.Brain.ActiveLayerName(), BotOwner_0.Brain.LastDecision?.ToString());
				if (flag)
				{
					Debug.LogError(message);
				}
			}
			_ = value.CorePointInGame.ConnectionGroupId;
			_ = BotOwner_0.StartCorePoint.ConnectionGroupId;
		}
		CovPoint = value;
		if (withSetOwner)
		{
			CovPoint?.SetOwner(BotOwner_0);
		}
	}

	public void HitInCoverByEnemy(Vector3 position)
	{
		Int_1++;
		if (Int_1 >= BotOwner_0.Settings.FileSettings.Cover.HITS_TO_LEAVE_COVER)
		{
			BotOwner_0.Memory.Spotted(byHit: true, position);
		}
	}

	public void HitInCoverByUnknown(Vector3 position)
	{
		Int_2++;
		if (Int_2 >= BotOwner_0.Settings.FileSettings.Cover.HITS_TO_LEAVE_COVER_UNKNOWN)
		{
			BotOwner_0.Memory.Spotted(byHit: true, position);
		}
	}

	public void AddShootDirection()
	{
		if (Float_3 < Time.time)
		{
			Float_3 = Time.time + BotOwner_0.Settings.FileSettings.Cover.SHOOT_NEAR_SEC_PERIOD;
			Int_0++;
			if (Int_0 >= BotOwner_0.Settings.FileSettings.Cover.SHOOT_NEAR_TO_LEAVE)
			{
				BotOwner_0.Memory.Spotted(byHit: false);
			}
		}
	}

	public bool UseDogFight(float delta)
	{
		if (Bool_0)
		{
			bool num = Time.time - LeaveCoverTime < delta;
			if (!num)
			{
				Bool_0 = false;
			}
			return num;
		}
		return false;
	}

	public void SetPathToCover(CustomNavigationPoint resultPoint)
	{
		CoverWithPath.ResultPoint = resultPoint;
		CoverWithPath.SetTime = Time.time;
	}

	public void method_0()
	{
		Bool_0 = true;
	}

	public void method_1()
	{
		if (!BotOwner_0.Settings.FileSettings.Cover.CHECK_COVER_ENEMY_LOOK)
		{
			return;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null || !goalEnemy.IsVisible)
		{
			return;
		}
		if (goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Cover.ENEMY_DIST_TO_GO_OUT)
		{
			BotOwner_0.Memory.Spotted(byHit: false);
			BotOwner_0.Memory.UseDogFightOut();
		}
		else if (BotOwner_0.IsEnemyLookingAtMe(goalEnemy))
		{
			Vector3 direction = BotOwner_0.Transform.position - goalEnemy.CurrPosition;
			float magnitude = direction.magnitude;
			if (magnitude < BotOwner_0.Settings.FileSettings.Cover.DIST_CHECK_SFETY && !Physics.Raycast(new Ray(goalEnemy.Person.WeaponRoot.position, direction), magnitude, LayerMaskClass.HighPolyWithTerrainMask))
			{
				BotOwner_0.Memory.Spotted(byHit: false);
				BotOwner_0.Memory.UseDogFightOut();
			}
		}
	}
}
