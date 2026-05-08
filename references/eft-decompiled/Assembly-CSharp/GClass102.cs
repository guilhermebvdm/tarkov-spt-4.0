using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public abstract class GClass102 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public Vector3? Nullable_1;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	[CompilerGenerated]
	public GInterface6 Ginterface6_0;

	public GInterface6 GInterface6_0
	{
		[CompilerGenerated]
		get
		{
			return Ginterface6_0;
		}
		[CompilerGenerated]
		set
		{
			Ginterface6_0 = value;
		}
	}

	public GClass102(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public void SetCarePosition(Vector3 lootPosition)
	{
		Vector3_0 = lootPosition;
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		data.CarePositions = GetCarePos(data.CarePositions);
		return base.FindPoint(data, p, checkCurrent);
	}

	public HashSet<Vector3> GetCarePos(HashSet<Vector3> prevPositions)
	{
		HashSet<Vector3> hashSet = prevPositions;
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (!ShallAttack())
		{
			if (goalEnemy == null)
			{
				hashSet.Add(Vector3_0);
			}
			else if (Nullable_1.HasValue)
			{
				hashSet = new HashSet<Vector3> { Nullable_1.Value };
			}
		}
		else if (goalEnemy != null && Time.time - goalEnemy.TimeLastSeen > BotOwner_0.Settings.FileSettings.Boss.KOJANIY_TAKE_CARE_ABOULT_ENEMY_DELTA && !method_14())
		{
			hashSet.Clear();
			hashSet.Add(Vector3_0);
			hashSet.Add(goalEnemy.EnemyLastPositionReal);
		}
		return hashSet;
	}

	public void method_13(GInterface6 bossData)
	{
		GInterface6_0 = bossData;
		_ = GInterface6_0;
		GInterface6_0.OnThrowRequest += method_15;
	}

	public abstract bool ShallAttack();

	public bool method_14()
	{
		bool result;
		if (!(result = GInterface6_0.ISeeEnemy()))
		{
			foreach (BotOwner key in GInterface6_0.FollowersPositions().Keys)
			{
				if (key.Memory.GoalEnemy != null && key.Memory.GoalEnemy.IsVisible)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	public void method_15()
	{
		if (GInterface6_0.CanStartSuppress() && BotOwner_0.Memory.GoalEnemy != null)
		{
			GInterface6_0.SupressRequestTaken();
			BotOwner_0.SuppressShoot.Init(BotOwner_0.Memory.GoalEnemy);
			CalcActionNextFrame(BotLogicDecision.suppressFire);
		}
	}

	public override void Dispose()
	{
		if (GInterface6_0 != null)
		{
			GInterface6_0.OnThrowRequest -= method_15;
		}
		base.Dispose();
	}
}
