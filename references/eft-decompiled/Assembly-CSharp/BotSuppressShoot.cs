using System;
using System.Collections.Generic;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class BotSuppressShoot : GClass519
{
	[NonSerialized]
	public int Shoots;

	[NonSerialized]
	public bool CanShootLastTime;

	[NonSerialized]
	public float NextShootTimeCheck;

	[NonSerialized]
	public List<Vector3> Points;

	[NonSerialized]
	public int ShootIndex;

	[NonSerialized]
	public int LimitShots;

	public BotSuppressShoot([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	public bool Init(EnemyInfo enemyToSupress, CustomNavigationPoint pos = null)
	{
		Shoots = 0;
		bool result = method_0(enemyToSupress, pos);
		Points = null;
		return result;
	}

	public bool InitToPoint(Vector3 point, CustomNavigationPoint pos = null)
	{
		Shoots = 0;
		bool result = method_1(point, pos);
		Points = null;
		return result;
	}

	public bool InitToPoints(List<Vector3> point, CustomNavigationPoint pos = null)
	{
		if (point.Count == 0)
		{
			return false;
		}
		LimitShots = Mathf.Min(BotOwner_0.Settings.FileSettings.Shoot.SUPPRESS_TRIGGERS_DOWN_AS_LIST, point.Count * 2);
		ShootIndex = 0;
		Points = point;
		return InitToPoint(point[ShootIndex], pos);
	}

	public override void Activate()
	{
		BotOwner_0.ShootData.OnTriggerPressed += method_3;
	}

	public void ManualUpdate()
	{
		if (Bool_0 && Float_1 < Time.time)
		{
			method_2(BotOwner_0.Settings.FileSettings.Shoot.SUPPRESS_BY_SHOOT_TIME);
		}
	}

	public void method_3()
	{
		if (!Bool_0)
		{
			return;
		}
		Shoots++;
		if (Points != null)
		{
			ShootIndex++;
			if (ShootIndex >= Points.Count)
			{
				ShootIndex = 0;
			}
			InitToPoint(Points[ShootIndex]);
			if (ShootIndex > LimitShots)
			{
				method_2(BotOwner_0.Settings.FileSettings.Shoot.SUPPRESS_BY_SHOOT_TIME);
			}
		}
		else if (Shoots > BotOwner_0.Settings.FileSettings.Shoot.SUPPRESS_TRIGGERS_DOWN)
		{
			method_2(BotOwner_0.Settings.FileSettings.Shoot.SUPPRESS_BY_SHOOT_TIME);
		}
	}

	public override void Dispose()
	{
		BotOwner_0.ShootData.OnTriggerPressed -= method_3;
	}
}
