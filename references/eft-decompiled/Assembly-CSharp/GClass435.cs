using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT;
using EFT.MovingPlatforms;
using UnityEngine;

public class GClass435 : ABossLogic
{
	[NonSerialized]
	public const int Int_0 = 4;

	[NonSerialized]
	public const string String_0 = "BossGluhar";

	[NonSerialized]
	public const string String_1 = "BossGluharAlt";

	[NonSerialized]
	public const string String_2 = "TrainBounds";

	public bool TrainSecure;

	public bool _assaultFollowersShallAttack;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public Dictionary<int, bool> Dictionary_0 = new Dictionary<int, bool>();

	[CompilerGenerated]
	private Action<BotOwner> action_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_2;

	[NonSerialized]
	[CompilerGenerated]
	public AIPlaceInfo AiplaceInfo_0;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_1;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_3;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_2;

	public bool HaveTrainInfo
	{
		[CompilerGenerated]
		get
		{
			return Bool_2;
		}
		[CompilerGenerated]
		set
		{
			Bool_2 = value;
		}
	}

	public AIPlaceInfo TrainBounds
	{
		[CompilerGenerated]
		get
		{
			return AiplaceInfo_0;
		}
		[CompilerGenerated]
		set
		{
			AiplaceInfo_0 = value;
		}
	}

	public int GoodCovers
	{
		[CompilerGenerated]
		get
		{
			return Int_1;
		}
		[CompilerGenerated]
		set
		{
			Int_1 = value;
		}
	}

	public AIPlaceInfo CorePlace => BotOwner_0.AIData.PlaceInfo;

	public bool FightAtZone
	{
		get
		{
			if (!TrainSecure)
			{
				return CorePlace != null;
			}
			return false;
		}
	}

	public bool BossShallAttack
	{
		[CompilerGenerated]
		get
		{
			return Bool_3;
		}
		[CompilerGenerated]
		set
		{
			Bool_3 = value;
		}
	}

	public bool AssaultFollowersShallAttack
	{
		get
		{
			return _assaultFollowersShallAttack;
		}
		set
		{
			_assaultFollowersShallAttack = value;
		}
	}

	public bool SecurityFightAtHomeEnought => GoodCovers >= 2;

	public bool IsInSector => BotOwner_0.AIData.PlaceInfo != null;

	public bool SecurityWannaAttack
	{
		get
		{
			if (FightAtZone)
			{
				EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
				if (goalEnemy != null)
				{
					return goalEnemy.Person.AIData.PlaceInfo == CorePlace;
				}
			}
			return Bool_1;
		}
		set
		{
			if (value != Bool_1)
			{
				Bool_1 = value;
				if (Bool_1)
				{
					AssaultFollowersShallAttack = true;
				}
			}
		}
	}

	public int SecurityCount
	{
		[CompilerGenerated]
		get
		{
			return Int_2;
		}
		[CompilerGenerated]
		set
		{
			Int_2 = value;
		}
	}

	public event Action<BotOwner> OnScoutWannaAttack
	{
		[CompilerGenerated]
		add
		{
			Action<BotOwner> action = action_0;
			Action<BotOwner> action2;
			do
			{
				action2 = action;
				Action<BotOwner> value2 = (Action<BotOwner>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<BotOwner> action = action_0;
			Action<BotOwner> action2;
			do
			{
				action2 = action;
				Action<BotOwner> value2 = (Action<BotOwner>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass435(BotOwner owner, BotBoss bossLogic)
		: base(owner, bossLogic)
	{
		BossLogic.OnFollowerStatusChange += method_1;
		Bool_0 = GClass856.IsTrue100(BotOwner_0.Settings.FileSettings.Boss.GLUHAR_BOSS_WANNA_ATTACK_CHANCE_0_100);
		BotEventHandler instance = Singleton<BotEventHandler>.Instance;
		SecurityCount = 0;
		if (instance != null)
		{
			instance.OnTrainCome += method_0;
		}
	}

	public override void SetPatrolMode()
	{
		GClass558 pointChooser = new GClass558(BotOwner_0, BotOwner_0.SpawnProfileData, "BossGluhar", "BossGluharAlt");
		BotOwner_0.PatrollingData.SetMode(PatrolMode.bossRoundProtectAndStay, pointChooser);
	}

	public override void Activate()
	{
		BotOwner_0.BotsGroup.CurrentEnemies.OnCurrentCheck += method_3;
		AIPlaceInfo trainBounds = BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.AIPlaceInfoHolder.FindPlace("TrainBounds");
		TrainBounds = trainBounds;
		HaveTrainInfo = TrainBounds != null;
	}

	public override void BossLogicUpdate()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (AssaultFollowersShallAttack)
		{
			if (goalEnemy == null || goalEnemy.Distance > BotOwner_0.Settings.FileSettings.Boss.GLUHAR_STOP_ASSAULT_ATTACK_DIST)
			{
				AssaultFollowersShallAttack = false;
			}
		}
		else if (goalEnemy != null && goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Boss.GLUHAR_ASSAULT_ATTACK_DIST)
		{
			AssaultFollowersShallAttack = true;
		}
	}

	public EnemyInfo FindDangerEnemyFor(BotOwner owner)
	{
		return null;
	}

	public bool SeenLessTimeBoss(float delta)
	{
		if (BotOwner_0.Memory.GoalEnemy != null && Time.time - BotOwner_0.Memory.GoalEnemy.PersonalLastSeenTime < delta)
		{
			return true;
		}
		return false;
	}

	public bool SeenLessTimeFollowersSecurity(float delta, out int count)
	{
		count = 0;
		bool result = false;
		foreach (BotOwner follower in BossLogic.Followers)
		{
			if (follower.Profile.Info.Settings.Role == WildSpawnType.followerGluharSecurity && follower.Memory.GoalEnemy != null && Time.time - follower.Memory.GoalEnemy.PersonalLastSeenTime < delta)
			{
				result = true;
				count++;
			}
		}
		return result;
	}

	public void OneScoutDoAttack(BotOwner scoutAttack)
	{
		if (action_0 != null)
		{
			action_0(scoutAttack);
		}
	}

	public void SetHaveCover(int id, bool haveCover)
	{
		if (!Dictionary_0.ContainsKey(id))
		{
			Dictionary_0.Add(id, value: false);
		}
		if (Dictionary_0[id] != haveCover)
		{
			Dictionary_0[id] = haveCover;
			if (haveCover)
			{
				GoodCovers++;
			}
			else
			{
				GoodCovers--;
			}
		}
	}

	public void method_0(Locomotive.ETravelState obj)
	{
		if (HaveTrainInfo)
		{
			switch (obj)
			{
			case Locomotive.ETravelState.OnRouteBack:
				TrainSecure = false;
				break;
			case Locomotive.ETravelState.OnRouteToDestination:
				TrainSecure = true;
				break;
			}
		}
	}

	public void method_1(BotOwner follower, FollowerStatusChange status)
	{
		switch (status)
		{
		case FollowerStatusChange.Add:
			if (follower.Profile.Info.Settings.Role == WildSpawnType.followerGluharSecurity)
			{
				SecurityCount++;
			}
			break;
		case FollowerStatusChange.Remove:
			switch (follower.Profile.Info.Settings.Role)
			{
			case WildSpawnType.followerGluharSecurity:
			{
				SecurityCount--;
				if (SecurityCount < 0)
				{
					SecurityCount = 0;
				}
				int num = 0;
				while (true)
				{
					if (num < BossLogic.Followers.Count)
					{
						BotOwner botOwner = BossLogic.Followers[num];
						if (botOwner.Profile.Info.Settings.Role != WildSpawnType.followerGluharSecurity && botOwner.Profile.Info.Settings.Role != WildSpawnType.followerGluharSnipe)
						{
							break;
						}
						num++;
						continue;
					}
					return;
				}
				SecurityCount++;
				method_2();
				break;
			}
			case WildSpawnType.followerGluharAssault:
			{
				foreach (BotOwner follower2 in BossLogic.Followers)
				{
					if (follower2.Profile.Info.Settings.Role == WildSpawnType.followerGluharAssault)
					{
						follower2.Tactic.SetTactic(BotsGroup.BotCurrentTactic.Protect);
					}
				}
				break;
			}
			}
			break;
		}
	}

	public void method_2()
	{
		for (int i = 0; i < BossLogic.Followers.Count; i++)
		{
			BotOwner botOwner = BossLogic.Followers[i];
			botOwner.BotFollower.SetToFollow(BossLogic, i, changeLogicMode: true);
			botOwner.Memory.GoalTarget.Clear();
		}
	}

	public void method_3(int obj)
	{
		if (!Bool_0)
		{
			return;
		}
		if (obj < 4)
		{
			if (BossShallAttack)
			{
				BotOwner_0.Tactic.SetTactic(BotsGroup.BotCurrentTactic.Protect);
			}
			BossShallAttack = false;
		}
		else
		{
			if (!BossShallAttack)
			{
				BotOwner_0.Tactic.SetTactic(BotsGroup.BotCurrentTactic.Attack);
			}
			BossShallAttack = true;
		}
	}

	public override void Dispose()
	{
		BotEventHandler instance = Singleton<BotEventHandler>.Instance;
		if (instance != null)
		{
			instance.OnTrainCome -= method_0;
		}
		action_0 = null;
		BossLogic.OnFollowerStatusChange -= method_1;
		if (BotOwner_0.BotsGroup != null)
		{
			BotOwner_0.BotsGroup.CurrentEnemies.OnCurrentCheck -= method_3;
		}
	}
}
