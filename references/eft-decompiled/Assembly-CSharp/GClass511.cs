using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class GClass511 : GClass510
{
	[NonSerialized]
	public List<BotOwner> List_0 = new List<BotOwner>();

	[NonSerialized]
	public List<BotOwner> List_1 = new List<BotOwner>();

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public float Float_3;

	public GClass511(BotOwner owner, PatrollingData data)
		: base(owner, data)
	{
		List<BotOwner> followers = owner.Boss.Followers;
		owner.Boss.OnFollowerStatusChange += method_5;
		foreach (BotOwner item in followers)
		{
			method_4(item);
		}
	}

	public override void Update()
	{
		method_2(base.Update);
	}

	public void method_1()
	{
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
		foreach (BotOwner item2 in List_1)
		{
			item2.BotFollower.PatrolDataFollower.SetCloseToTarget(Bool_0);
		}
		if (flag2 && flag)
		{
			PatrollingData_0.Unpause();
		}
	}

	public void method_2(Action baseUpdate)
	{
		BotOwner_0.Mover.SetTargetMoveSpeed(1f);
		if (BotOwner_0.PatrollingData.Way.PatrolType == PatrolType.reserved)
		{
			if (!(Float_3 < Time.time))
			{
				return;
			}
			Float_3 = Time.time + 25f;
			for (int i = 0; i < BotOwner_0.Boss.Followers.Count; i++)
			{
				BotOwner botOwner = BotOwner_0.Boss.Followers[i];
				PatrolPointContainer point = new PatrolPointContainer(BotOwner_0.PatrollingData.CurPatrolPoint.TargetPoint.GetSubPoint(i));
				if (botOwner != null && botOwner.PatrollingData != null && botOwner.PatrollingData.PointControl != null)
				{
					botOwner.PatrollingData.PointControl.SetTarget(point);
				}
			}
			return;
		}
		bool flag = PatrollingData_0.Status == PatrolStatus.pause;
		method_1();
		bool flag2 = method_3(List_1);
		bool flag3 = method_3(List_0);
		int num = 0;
		bool flag4 = flag2;
		bool isFront = true;
		foreach (BotOwner item in List_0)
		{
			GStruct25 trg;
			if (flag4)
			{
				PatrolPoint subPoint = BotOwner_0.PatrollingData.CurPatrolPoint.GetSubPoint(num);
				trg = new GStruct25(subPoint.Position, subPoint.ShallSit, subPoint.PointWithLookSides);
			}
			else if ((item.Position - BotOwner_0.Position).sqrMagnitude < 100f)
			{
				trg = new GStruct25(item.Position);
			}
			else
			{
				PatrolPoint subPoint2 = BotOwner_0.PatrollingData.CurPatrolPoint.GetSubPoint(num);
				trg = new GStruct25(subPoint2.Position, subPoint2.ShallSit, subPoint2.PointWithLookSides);
				GClass510.FrontPos(BotOwner_0, isFront, List_0.Count, num);
			}
			num++;
			item.BotFollower.PatrolDataFollower.SetTarget(trg, null);
			item.BotFollower.PatrolDataFollower.SetProblemsSomebody(!flag2 || !flag3);
		}
		if (flag4)
		{
			if (flag)
			{
				PatrollingData_0.Unpause();
			}
		}
		else if (!flag)
		{
			PatrollingData_0.Pause();
		}
		baseUpdate();
	}

	public bool method_3(List<BotOwner> followers)
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

	public void method_4(BotOwner bot)
	{
		bool flag = List_1.Count <= List_0.Count;
		switch (bot.Profile.Info.Settings.Role)
		{
		case WildSpawnType.followerKolontaySecurity:
			flag = false;
			break;
		case WildSpawnType.followerKolontayAssault:
			flag = true;
			break;
		}
		if (flag)
		{
			bot.BotFollower.PatrolDataFollower.Init(PatrolFollowerType.Scout, this);
			List_1.Add(bot);
		}
		else
		{
			bot.BotFollower.PatrolDataFollower.Init(PatrolFollowerType.CloseCover, this);
			List_0.Add(bot);
		}
		if (BotOwner_0.PatrollingData.Way == null)
		{
			BotOwner_0.PatrollingData.PointChooser.ChooseStartWay();
		}
		BotOwner_0.PatrollingData.PointChooser.FindPointForFollower(bot, BotOwner_0.PatrollingData.Way);
	}

	public void method_5(BotOwner bot, FollowerStatusChange status)
	{
		switch (status)
		{
		case FollowerStatusChange.Remove:
			List_0.Remove(bot);
			List_1.Remove(bot);
			break;
		case FollowerStatusChange.Add:
			method_4(bot);
			break;
		}
	}

	public override void Dispose()
	{
		BotOwner_0.Boss.OnFollowerStatusChange -= method_5;
	}
}
