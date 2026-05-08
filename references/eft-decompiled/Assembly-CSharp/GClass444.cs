using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class GClass444 : ABossLogic
{
	[NonSerialized]
	public Dictionary<int, bool> Dictionary_0 = new Dictionary<int, bool>();

	[NonSerialized]
	public Dictionary<int, Vector3> Dictionary_1 = new Dictionary<int, Vector3>();

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public bool Bool_0;

	public BotHalloweenEvent BotHalloweenEvent_0 => BotOwner_0.BotsController.EventsController.BotHalloweenEvent;

	public AIPlaceInfoHalloweenCircle AIPlaceInfoHalloweenCircle_0 => BotHalloweenEvent_0.TargetCircle;

	public Vector3 Vector3_0 => AIPlaceInfoHalloweenCircle_0.CenterPoint.position;

	public bool Boolean_0 => BotHalloweenEvent_0.IsEventRaised;

	public GClass444(BotOwner owner, BotBoss bossLogic)
		: base(owner, bossLogic)
	{
	}

	public void StartRitual()
	{
		BotHalloweenEvent_0.StartRitual();
	}

	public override void SetPatrolMode()
	{
	}

	public override void Activate()
	{
		BotOwner_0.Boss.OnFollowerStatusChange += method_0;
		BotHalloweenEvent_0.SetupGroup(BotOwner_0.BotsGroup.Id);
		method_1();
	}

	public override void BossLogicUpdate()
	{
		if (!(Float_0 > Time.time))
		{
			bool bool_;
			if ((bool_ = Time.time - BotOwner_0.BotsGroup.EnemyLastSeenTimeReal < 5f) != Bool_0)
			{
				Bool_0 = bool_;
				method_1();
			}
			Float_0 = Time.time + (Bool_0 ? 30f : 3f);
		}
	}

	public bool ShouldCast(int id)
	{
		if (Boolean_0)
		{
			return false;
		}
		if (Dictionary_0.TryGetValue(id, out var value))
		{
			return value;
		}
		return false;
	}

	public Vector3 GetSummonCenter()
	{
		return Vector3_0;
	}

	public Vector3 GetPosition(int ownerId)
	{
		return Dictionary_1[ownerId];
	}

	public void method_0(BotOwner bot, FollowerStatusChange status)
	{
		method_1();
		if (status == FollowerStatusChange.Remove)
		{
			BotHalloweenEvent_0.KillFirstSectant();
		}
	}

	public void method_1()
	{
		if (!BotHalloweenEvent_0.IsMyGroup(BotOwner_0.BotsGroup.Id))
		{
			return;
		}
		int num = 0;
		int placeIndex = 0;
		num = method_3(0, BotOwner_0, 0);
		foreach (BotOwner follower in BotOwner_0.Boss.Followers)
		{
			num = method_3(num, follower, placeIndex);
		}
	}

	public int method_2()
	{
		if (Bool_0)
		{
			if (BotOwner_0.BotsGroup.MembersCount == 1)
			{
				return 0;
			}
			return Mathf.Min(BotHalloweenEvent_0.ToCircle, 1);
		}
		return BotHalloweenEvent_0.ToCircle;
	}

	public int method_3(int isHolder, BotOwner bot, int placeIndex)
	{
		bool flag = isHolder < method_2();
		if (Dictionary_0.ContainsKey(bot.Id))
		{
			Dictionary_0[bot.Id] = flag;
		}
		else
		{
			Dictionary_0.Add(bot.Id, flag);
		}
		if (flag)
		{
			Vector3 position = AIPlaceInfoHalloweenCircle_0.CastPoints[isHolder].position;
			if (Dictionary_1.ContainsKey(bot.Id))
			{
				Dictionary_1[bot.Id] = position;
			}
			else
			{
				Dictionary_1.Add(bot.Id, position);
			}
			isHolder++;
		}
		return isHolder;
	}

	public override void Dispose()
	{
		if (!BotOwner_0.HealthController.IsAlive)
		{
			BotHalloweenEvent_0.KillFirstSectant();
		}
		BotOwner_0.Boss.OnFollowerStatusChange -= method_0;
	}
}
