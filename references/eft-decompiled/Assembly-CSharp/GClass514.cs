using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class GClass514 : GClass510
{
	[NonSerialized]
	public const float Float_2 = 8.6f;

	[NonSerialized]
	public WildSpawnType? Nullable_0;

	[NonSerialized]
	public List<BotOwner> List_0 = new List<BotOwner>();

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public PatrolFollowerType PatrolFollowerType_0;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public float Float_4;

	public GClass514(BotOwner owner, PatrollingData data, PatrolFollowerType patrolFollowerType)
		: base(owner, data)
	{
		PatrolFollowerType_0 = patrolFollowerType;
		if (owner.Profile.Info.Settings.Role == WildSpawnType.bossGluhar || owner.Profile.Info.Settings.Role == WildSpawnType.bossTest)
		{
			Nullable_0 = WildSpawnType.followerGluharSecurity;
		}
		List<BotOwner> followers = owner.Boss.Followers;
		owner.Boss.OnFollowerStatusChange += method_2;
		foreach (BotOwner item in followers)
		{
			method_1(item);
		}
	}

	public override void Update()
	{
		BotOwner_0.Mover.SetTargetMoveSpeed(1f);
		if (BotOwner_0.PatrollingData.Way.PatrolType == PatrolType.reserved)
		{
			return;
		}
		bool flag = PatrollingData_0.Status == PatrolStatus.pause;
		method_3();
		bool flag2 = method_4(List_0);
		int num = 0;
		foreach (BotOwner item in List_0)
		{
			PatrolPoint subPoint = BotOwner_0.PatrollingData.CurPatrolPoint.GetSubPoint(num);
			GStruct25 trg = new GStruct25(subPoint.Position, subPoint.ShallSit, subPoint.PointWithLookSides);
			num++;
			item.BotFollower.PatrolDataFollower.SetTarget(trg, null);
			item.BotFollower.PatrolDataFollower.SetProblemsSomebody(!flag2);
		}
		if (flag)
		{
			PatrollingData_0.Unpause();
		}
		base.Update();
	}

	public override void DrawGizmosSelected()
	{
		int num = 0;
		foreach (BotOwner item in List_0)
		{
			_ = item;
			num++;
		}
	}

	public void method_1(BotOwner bot)
	{
		if (!Nullable_0.HasValue || Nullable_0.Value == bot.Profile.Info.Settings.Role)
		{
			PatrolFollowerType patrolFollowerType_ = PatrolFollowerType_0;
			bot.BotFollower.PatrolDataFollower.Init(patrolFollowerType_, this);
			List_0.Add(bot);
			if (BotOwner_0.PatrollingData.Way == null)
			{
				BotOwner_0.PatrollingData.PointChooser.ChooseStartWay();
			}
			BotOwner_0.PatrollingData.PointChooser.FindPointForFollower(bot, BotOwner_0.PatrollingData.Way);
		}
	}

	public void method_2(BotOwner bot, FollowerStatusChange val)
	{
		switch (val)
		{
		case FollowerStatusChange.Remove:
			List_0.Remove(bot);
			break;
		case FollowerStatusChange.Add:
			method_1(bot);
			break;
		}
	}

	public void method_3()
	{
		if (!(Float_4 < Time.time))
		{
			return;
		}
		Float_4 = Time.time + 2f;
		bool flag = PatrollingData_0.Status == PatrolStatus.pause;
		bool flag2;
		if ((flag2 = Mathf.Abs(PatrollingData_0.CurPatrolPoint.Position.y - BotOwner_0.Transform.position.y) < 1f && Float_1 < 4f) == Bool_0)
		{
			return;
		}
		Bool_0 = flag2;
		foreach (BotOwner item in List_0)
		{
			item.BotFollower.PatrolDataFollower.SetCloseToTarget(Bool_0);
		}
		if (flag2 && flag)
		{
			PatrollingData_0.Unpause();
		}
	}

	public bool method_4(List<BotOwner> followers)
	{
		foreach (BotOwner follower in followers)
		{
			if (follower.BotFollower.PatrolDataFollower.HaveProblems)
			{
				return false;
			}
		}
		return true;
	}

	public override void Dispose()
	{
		BotOwner_0.Boss.OnFollowerStatusChange -= method_2;
	}
}
