using System;
using EFT;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

public class BotSearchData : GClass429
{
	[NonSerialized]
	public bool Going;

	[NonSerialized]
	public float UpdatePointTimer;

	[NonSerialized]
	public bool IsReachableLast = true;

	[NonSerialized]
	public CustomNavigationPoint ClosePoint;

	[NonSerialized]
	public float NextPosibleCheckTime;

	[NonSerialized]
	public float NextPosibleGoRefresh;

	[NonSerialized]
	public float LastCheckEnemyPos;

	[NonSerialized]
	public BotSearchPoint LastSearchPoint;

	[field: NonSerialized]
	public BotSearchPoint SearchPoint { get; set; }

	public static BotSearchData Create(BotOwner bot)
	{
		switch (bot.Profile.Info.Settings.Role)
		{
		case WildSpawnType.followerGluharAssault:
		case WildSpawnType.followerGluharScout:
			return new GClass470(bot);
		case WildSpawnType.bossKilla:
		case WildSpawnType.cursedAssault:
			return new GClass471(bot);
		default:
			return new BotSearchData(bot);
		case WildSpawnType.bossTagillaAgro:
		case WildSpawnType.tagillaHelperAgro:
			return new GClass469(bot);
		}
	}

	public BotSearchData([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	public virtual void Activate()
	{
	}

	public void RefreshSearchPoint()
	{
		if (!(NextPosibleCheckTime > Time.time))
		{
			BotOwner_0.Mover.Sprint(val: false);
			NextPosibleCheckTime = Time.time + 10f;
			BotSearchPoint nextSearchPoint = method_1();
			method_0(nextSearchPoint);
		}
	}

	public virtual void UpdateByNode()
	{
		RefreshSearchPoint();
		BotOwner_0.LookData.SetLookPointByHearing();
		if (SearchPoint == null)
		{
			BotOwner_0.StopMove();
			return;
		}
		bool flag = true;
		float sqrMagnitude = (BotOwner_0.Transform.position - SearchPoint.Position).sqrMagnitude;
		float sqrDist;
		BotOwner closestFriend = BotOwner_0.Covers.GetClosestFriend(out sqrDist);
		if (sqrMagnitude < 1f)
		{
			method_4();
			return;
		}
		if (closestFriend != null && sqrDist < LocalBotSettingsProviderClass.Core.MIN_DIST_CLOSE_DEF)
		{
			float sqrMagnitude2 = (closestFriend.Transform.position - SearchPoint.Position).sqrMagnitude;
			flag = sqrMagnitude < sqrMagnitude2;
		}
		if (flag)
		{
			Vector3 vector = BotOwner_0.Position - SearchPoint.Position;
			if (vector.y < BotOwner_0.Settings.FileSettings.Move.Y_APPROXIMATION)
			{
				vector.y = 0f;
			}
			if (!(vector.sqrMagnitude < 4f))
			{
				if (NextPosibleGoRefresh < Time.time)
				{
					NextPosibleGoRefresh = Time.time + 4f;
					GoStraightCoverPoint(SearchPoint.Position);
				}
			}
			else
			{
				method_4();
			}
		}
		else
		{
			BotOwner_0.Mover.Sprint(val: false);
			BotOwner_0.StopMove();
		}
	}

	public void GoStraightCoverPoint(Vector3 goTo)
	{
		BotOwner_0.Sprint(val: false);
		BotOwner_0.SetTargetMoveSpeed(1f);
		NavMeshPathStatus navMeshPathStatus = BotOwner_0.GoToPoint(goTo, slowAtTheEnd: false, -1f, getUpWithCheck: true, mustHaveWay: false);
		IsReachableLast = navMeshPathStatus == NavMeshPathStatus.PathComplete;
		Going = true;
		if (!IsReachableLast)
		{
			BotOwner_0.StopMove();
		}
		CustomNavigationPoint closestPoint = BotOwner_0.Covers.GetClosestPoint(BotOwner_0.Position);
		if (closestPoint != null)
		{
			if ((BotOwner_0.Position - closestPoint.Position).sqrMagnitude < BotOwner_0.Settings.FileSettings.Cover.CLOSE_DIST_POINT_SQRT)
			{
				ClosePoint = closestPoint;
			}
			BotOwner_0.LookData.SetLookPointByHearing(ClosePoint);
		}
	}

	public void method_0(BotSearchPoint nextSearchPoint)
	{
		if (nextSearchPoint == null)
		{
			SearchPoint = null;
		}
		else if (LastSearchPoint == null)
		{
			SearchPoint = nextSearchPoint;
		}
		else if (LastSearchPoint.TypeSearch != nextSearchPoint.TypeSearch)
		{
			SearchPoint = nextSearchPoint;
		}
		else if ((LastSearchPoint.Position - nextSearchPoint.Position).sqrMagnitude < 1f)
		{
			SearchPoint = null;
		}
		else
		{
			SearchPoint = nextSearchPoint;
		}
	}

	public BotSearchPoint method_1()
	{
		BotSearchPoint botSearchPoint = method_3();
		if (botSearchPoint != null)
		{
			return botSearchPoint;
		}
		return method_2();
	}

	public BotSearchPoint method_2()
	{
		GoalTargetClass goalTarget = BotOwner_0.Memory.GoalTarget;
		if (!goalTarget.HavePlaceTarget())
		{
			return null;
		}
		if (goalTarget.GoalTarget.IsCome)
		{
			return null;
		}
		method_5();
		return new GClass539(goalTarget);
	}

	public BotSearchPoint method_3()
	{
		Vector3? vector = null;
		if (BotOwner_0.Memory.GoalEnemy != null)
		{
			if (BotOwner_0.Memory.GoalEnemy.ShallKnowEnemyLate())
			{
				vector = BotOwner_0.Memory.GoalEnemy.EnemyLastPosition;
			}
		}
		else if (BotOwner_0.Memory.LastEnemy != null && Time.time - LastCheckEnemyPos > 50f)
		{
			LastCheckEnemyPos = Time.time;
			vector = BotOwner_0.Memory.LastEnemy.EnemyLastPosition;
		}
		if (vector.HasValue)
		{
			return new BotSearchPoint(vector.Value, EBotSearchPoint.playerPosition);
		}
		return null;
	}

	public void method_4()
	{
		SearchPoint?.Come();
		BotOwner_0.SetPose(0.01f);
		BotOwner_0.StopMove();
		LastSearchPoint = SearchPoint;
		NextPosibleCheckTime = Time.time + 1f;
		SearchPoint = null;
	}

	public void method_5()
	{
		if (UpdatePointTimer < Time.time)
		{
			UpdatePointTimer = Time.time + BotOwner_0.Settings.FileSettings.Move.UPDATE_TIME_RECAL_WAY;
			Going = false;
		}
	}

	public void Dispose()
	{
		LastSearchPoint = null;
		SearchPoint = null;
		ClosePoint = null;
	}
}
