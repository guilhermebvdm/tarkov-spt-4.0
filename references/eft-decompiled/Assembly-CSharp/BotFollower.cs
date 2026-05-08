using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using UnityEngine;

public class BotFollower : GClass429
{
	[NonSerialized]
	public PatrolDataFollower PatrolDataFollower_1;

	[NonSerialized]
	public BotFollowerFight BotFollowerFight_1;

	[field: NonSerialized]
	public IBossToFollow BossToFollow { get; set; }

	public PatrolDataFollower PatrolDataFollower
	{
		get
		{
			if (PatrolDataFollower_1 == null)
			{
				PatrolDataFollower_1 = new PatrolDataFollower(BotOwner_0, Index);
			}
			return PatrolDataFollower_1;
		}
		set
		{
			PatrolDataFollower_1 = value;
		}
	}

	public BotFollowerFight BotFollowerFight
	{
		get
		{
			if (BotFollowerFight_1 == null)
			{
				BotFollowerFight_1 = new BotFollowerFight(BotOwner_0, this);
			}
			return BotFollowerFight_1;
		}
		set
		{
			BotFollowerFight_1 = value;
		}
	}

	public bool HaveBoss => BossToFollow != null;

	[field: NonSerialized]
	public int Index { get; set; }

	public virtual bool NeedToProtectBoss
	{
		get
		{
			if (HaveBoss)
			{
				return BossToFollow.NeedProtection;
			}
			return false;
		}
	}

	public int FollowersTargetCount => BossToFollow.FollowersTargetCount;

	public event Action<IBossToFollow> OnBossFinded;

	public event Action<Player> OnBossDead;

	public static BotFollower Create(BotOwner bot)
	{
		WildSpawnType role = bot.Profile.Info.Settings.Role;
		if (role != WildSpawnType.followerTest && (uint)(role - 12) > 3u)
		{
			return new BotFollower(bot);
		}
		return new GClass454(bot);
	}

	public BotFollower(BotOwner owner)
		: base(owner)
	{
	}

	public void method_0(float period)
	{
		PatrolDataFollower.StopFor(Mathf.Clamp(period, 1f, 60f));
	}

	public virtual void Activate()
	{
		BotFollowerFight = new BotFollowerFight(BotOwner_0, this);
		PatrolDataFollower = new PatrolDataFollower(BotOwner_0, Index);
	}

	public void TryFindBoss()
	{
		if (BotOwner_0.IsFollower())
		{
			method_2();
			method_1();
		}
	}

	public virtual void SetToFollow(IBossToFollow boss, int index, bool changeLogicMode = false)
	{
		Index = index;
		if (!(BossToFollow == null || changeLogicMode))
		{
			return;
		}
		BossToFollow = boss;
		PatrolDataFollower.Activate();
		PatrolDataFollower.SetIndex(Index);
		PatrolMode mode = PatrolMode.follower;
		PatrolMode mode2 = PatrolMode.simple;
		if (BotOwner_0.Profile.Info.Settings.Role == WildSpawnType.followerBigPipe || BotOwner_0.Profile.Info.Settings.Role == WildSpawnType.followerBirdEye)
		{
			mode2 = PatrolMode.groupMoving;
		}
		PatrolPointChooserBasic pointChooser = PatrollingData.GetPointChooser(BotOwner_0, mode2, BotOwner_0.SpawnProfileData);
		BotOwner_0.PatrollingData.SetMode(mode, pointChooser);
		BotOwner_0.Tactic.SetTactic(BotsGroup.BotCurrentTactic.Protect);
		BossFindAction();
		if (boss.IsAI)
		{
			BotOwner botOwner = boss.Player().AIData.BotOwner;
			if (changeLogicMode)
			{
				botOwner.PeacefulActions.OnStartPeacefulMove -= method_4;
				botOwner.DeadBodyWork.OnStartLookToBody -= method_0;
			}
			botOwner.PeacefulActions.OnStartPeacefulMove += method_4;
			botOwner.DeadBodyWork.OnStartLookToBody += method_0;
		}
	}

	public void DrawGizmos()
	{
		if (HaveBoss)
		{
			Vector3 center = BotOwner_0.Position + Vector3.up * 1.5f;
			Gizmos.color = new Color(1f, 0.64705884f, 0f);
			Gizmos.DrawWireSphere(center, 0.3f);
			if (PatrolDataFollower.HaveProblems)
			{
				Gizmos.color = new Color(1f, 13f / 51f, 0.2f);
			}
			Gizmos.DrawWireSphere(center, 0.4f);
		}
	}

	public void DrawGizmosSelected()
	{
		if (PatrolDataFollower != null)
		{
			PatrolDataFollower.OnDrawGizmosSelected();
		}
	}

	public virtual void Update(BotOwner bot)
	{
	}

	public void BossFindAction()
	{
		this.OnBossFinded?.Invoke(BossToFollow);
		Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(BossToFollow.Player().ProfileId).OnPlayerDead += method_3;
	}

	public void method_1(float? maxDist = null)
	{
		float num = float.MaxValue;
		if (maxDist.HasValue)
		{
			num = maxDist.Value * maxDist.Value;
		}
		List<Player> allBossPlayers = BotOwner_0.BotsGroup.BotGame.BotsController.GetAllBossPlayers();
		Player player = null;
		float num2 = float.MaxValue;
		for (int i = 0; i < allBossPlayers.Count; i++)
		{
			Player player2 = allBossPlayers[i];
			if (player2.HealthController.IsAlive)
			{
				float sqrMagnitude = (player2.Position - BotOwner_0.Position).sqrMagnitude;
				if (sqrMagnitude < num2 && sqrMagnitude < num)
				{
					num2 = sqrMagnitude;
					player = player2;
				}
			}
		}
		if (player != null)
		{
			player.AIData.AIBossPlayer.OfferBot(BotOwner_0);
		}
	}

	public void method_2(float? maxDist = null)
	{
		float num = float.MaxValue;
		if (maxDist.HasValue)
		{
			num = maxDist.Value * maxDist.Value;
		}
		IEnumerable<BotOwner> botOwners = BotOwner_0.BotsGroup.BotGame.BotsController.Bots.BotOwners;
		BotBoss botBoss = null;
		float num2 = float.MaxValue;
		foreach (BotOwner item in botOwners)
		{
			if (item.BotState != EBotState.Active || !item.HealthController.IsAlive || !item.Boss.IamBoss || item.Id == BotOwner_0.Id || item.Boss.Followers.Count >= item.Boss.TargetFollowersCount || item.BotsGroup.BotZone != BotOwner_0.BotsGroup.BotZone)
			{
				continue;
			}
			float sqrMagnitude = (item.Position - BotOwner_0.Position).sqrMagnitude;
			if (sqrMagnitude < num2 && sqrMagnitude < num)
			{
				num2 = sqrMagnitude;
				if (BotBoss.IsFollowerSuitableForBoss(BotOwner_0.Profile.Info.Settings.Role, item.Profile.Info.Settings.Role))
				{
					botBoss = item.Boss;
				}
			}
		}
		botBoss?.OfferSelf(BotOwner_0);
	}

	public void method_3(Player player, IPlayer lastAggressor, DamageInfoStruct damageInfo, EBodyPart part)
	{
		this.OnBossDead?.Invoke(player);
	}

	public void method_4(GClass413 pairData)
	{
		float value = pairData.EndTime - Time.time;
		if (pairData.ShallStop)
		{
			PatrolDataFollower.StopFor(Mathf.Clamp(value, 1f, 60f));
		}
	}

	public virtual bool Dispose()
	{
		PatrolDataFollower.Dispose();
		if (BossToFollow != null && BossToFollow.IsAlive)
		{
			if (BotOwner_0.HealthController.IsAlive && BotOwner_0.BotState == EBotState.Active)
			{
				PatrolPointChooserBasic pointChooser = PatrollingData.GetPointChooser(BotOwner_0, PatrolMode.simple, BotOwner_0.SpawnProfileData);
				BotOwner_0.PatrollingData?.SetMode(PatrolMode.simple, pointChooser);
			}
			BossToFollow.RemoveFollower(BotOwner_0);
			BossToFollow = null;
			return true;
		}
		return false;
	}
}
