using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class GClass341 : GClass337
{
	[NonSerialized]
	public const int Int_0 = 1;

	[NonSerialized]
	public const int Int_1 = 2;

	[NonSerialized]
	public const int Int_2 = 5;

	[NonSerialized]
	public const int Int_3 = 6;

	[NonSerialized]
	public const int Int_4 = 7;

	[NonSerialized]
	public const int Int_5 = 9;

	[NonSerialized]
	public const int Int_6 = 10;

	[NonSerialized]
	public const int Int_7 = 11;

	[NonSerialized]
	public const int Int_8 = 12;

	[NonSerialized]
	public GClass68 Gclass68_0;

	[NonSerialized]
	public float Float_0 = 900f;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	public GClass341(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(5, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 78);
		method_0(12, layer2, activeOnStart: true);
		GClass166 layer3 = new GClass166(owner, 75);
		method_0(11, layer3, activeOnStart: true);
		GClass84 layer4 = new GClass84(owner, 70);
		method_0(10, layer4, activeOnStart: true);
		Gclass68_0 = new GClass68(owner, 65);
		method_0(1, Gclass68_0, activeOnStart: true);
		GClass39 layer5 = new GClass39(owner, 50);
		method_0(7, layer5, activeOnStart: true);
		GClass139 layer6 = new GClass139(owner, 30);
		method_0(9, layer6, activeOnStart: true);
		GClass46 layer7 = new GClass46(owner, 9);
		method_0(6, layer7, activeOnStart: true);
		GClass133 layer8 = new GClass133(owner, 0);
		method_0(2, layer8, activeOnStart: true);
		method_6();
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 75, 55, 76);
	}

	public override void ManualUpdate()
	{
		if (Float_2 < Time.time)
		{
			Float_2 = Time.time + 2f;
			method_11();
		}
	}

	public override string ShortName()
	{
		return "FollowerGluharScout";
	}

	public void method_6()
	{
		if (Gclass435_0 == null)
		{
			if (Owner.BotFollower.BossToFollow != null)
			{
				method_7(Owner.BotFollower.BossToFollow);
			}
			else
			{
				Owner.BotFollower.OnBossFinded += method_9;
			}
		}
	}

	public void method_7(IBossToFollow bossToFollow)
	{
		if (bossToFollow.Player().AIData.BotOwner.Boss.BossLogic is GClass435 gClass)
		{
			SetBoss(gClass);
			gClass.OnScoutWannaAttack += method_8;
		}
		else
		{
			UnityEngine.Debug.LogError("Can't find boss gluhar");
		}
	}

	public void method_8(BotOwner obj)
	{
		if (obj.Id != Owner.Id)
		{
			method_10();
		}
	}

	public void method_9(IBossToFollow bossToFollow)
	{
		method_7(bossToFollow);
		Owner.BotFollower.OnBossFinded -= method_9;
	}

	public void method_10()
	{
		Bool_0 = true;
		Bool_1 = false;
		Owner.Tactic.SetTactic(BotsGroup.BotCurrentTactic.Protect);
		Gclass68_0.ShallAttack(Bool_1);
	}

	public void method_11()
	{
		if (Gclass435_0 == null)
		{
			return;
		}
		EnemyInfo goalEnemy = Owner.Memory.GoalEnemy;
		if (Bool_1)
		{
			if (Float_1 < Time.time)
			{
				if (goalEnemy != null)
				{
					bool bool_ = goalEnemy.Distance > Owner.Settings.FileSettings.Boss.GLUHAR_FOLLOWER_SCOUT_DIST_END_ATTACK;
					Bool_1 = bool_;
				}
				else
				{
					Bool_1 = false;
				}
				Float_1 = Time.time + 3f;
			}
		}
		else
		{
			if (!(Float_1 < Time.time) || goalEnemy == null)
			{
				return;
			}
			Bool_1 = goalEnemy.Distance < Owner.Settings.FileSettings.Boss.GLUHAR_FOLLOWER_SCOUT_DIST_START_ATTACK && goalEnemy.IsVisible;
			Float_1 = Time.time + 3f;
			if (Bool_1 || Owner.BotFollower.BossToFollow == null)
			{
				return;
			}
			List<BotOwner> followers = Owner.BotFollower.BossToFollow.Followers;
			float num = float.MaxValue;
			foreach (BotOwner item in followers)
			{
				if (item.Profile.Info.Settings.Role == WildSpawnType.followerGluharAssault || item.Profile.Info.Settings.Role == WildSpawnType.bossGluhar)
				{
					float sqrMagnitude = (item.Position - Owner.Position).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
					}
				}
			}
			if (num < Float_0)
			{
				Bool_1 = true;
				Gclass435_0.OneScoutDoAttack(Owner);
			}
		}
	}
}
