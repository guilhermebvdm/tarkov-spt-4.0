using System;
using EFT;
using UnityEngine;

public class BotHalloweenWithZombiesGameEvent : GClass429
{
	[NonSerialized]
	public float HoldPeriod_1;

	[NonSerialized]
	public float NextChangePositionTime;

	[NonSerialized]
	public Vector3 FindPointCenterPosition_1;

	[NonSerialized]
	public BotOwner CachedLeader;

	public float HoldPeriod
	{
		get
		{
			TryRecalcPatrolParams();
			return HoldPeriod_1;
		}
	}

	public Vector3 FindPointCenterPosition
	{
		get
		{
			BotOwner botOwner = method_0();
			if (BotOwner_0 != botOwner)
			{
				return botOwner.GameEventsData.HalloweenGameEvent.FindPointCenterPosition;
			}
			TryRecalcPatrolParams();
			return FindPointCenterPosition_1;
		}
	}

	public event Action<Vector3, float, GClass674> OnCallAction;

	public event Action<IPlayer> OnReportSeenEnemy;

	public BotHalloweenWithZombiesGameEvent(BotOwner owner)
		: base(owner)
	{
	}

	public BotOwner method_0()
	{
		if (CachedLeader != null && CachedLeader.GetPlayer.HealthController.IsAlive)
		{
			return CachedLeader;
		}
		BotOwner cachedLeader = BotOwner_0;
		int num = 0;
		while (true)
		{
			if (num < BotOwner_0.BotsGroup.MembersCount)
			{
				BotOwner botOwner = BotOwner_0.BotsGroup.Member(num);
				if (num % 5 == 0 && num < BotOwner_0.BotsGroup.MembersCount - 1)
				{
					cachedLeader = botOwner;
				}
				if (botOwner == BotOwner_0)
				{
					break;
				}
				num++;
				continue;
			}
			CachedLeader = BotOwner_0.BotsGroup.Member(0);
			return CachedLeader;
		}
		CachedLeader = cachedLeader;
		return CachedLeader;
	}

	public void ReportSeenEnemy(IPlayer target)
	{
		this.OnReportSeenEnemy?.Invoke(target);
	}

	public void Call(Vector3 positionToCheck, float callRadiusSqr, GClass674 pursuit)
	{
		this.OnCallAction?.Invoke(positionToCheck, callRadiusSqr, pursuit);
	}

	public void TryRecalcPatrolParams()
	{
		if (Time.time > NextChangePositionTime - 5f)
		{
			HoldPeriod_1 = GClass856.Random(180f, 600f);
			NextChangePositionTime = Time.time + HoldPeriod_1;
			FindPointCenterPosition_1 = BotOwner_0.Position + GClass856.RandomHorizontal(-100f, 100f);
		}
	}
}
