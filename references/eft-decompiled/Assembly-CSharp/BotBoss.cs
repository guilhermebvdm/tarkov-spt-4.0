using System;
using System.Collections.Generic;
using System.Linq;
using EFT;
using UnityEngine;

public class BotBoss : GClass429, IBossToFollow
{
	[NonSerialized]
	public GClass456 Followers_1;

	[NonSerialized]
	public GInterface10 PatrolMove;

	[field: NonSerialized]
	public bool IamBoss { get; set; }

	public bool AllowRequestSelf => BotOwner_0.Settings.FileSettings.Boss.ALLOW_REQUEST_SELF;

	[field: NonSerialized]
	public ABossLogic BossLogic { get; set; }

	[field: NonSerialized]
	public bool NeedProtection { get; set; } = true;

	public PatrollingData PatrollingData => BotOwner_0.PatrollingData;

	public bool IsAI => true;

	public BotOwner Owner => BotOwner_0;

	public Vector3 PositionOrTargetCover
	{
		get
		{
			if (BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint != null)
			{
				return BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint.Position;
			}
			return BotOwner_0.Position;
		}
	}

	public Vector3 PositionIfInCover
	{
		get
		{
			if (BotOwner_0.Memory.IsInCover)
			{
				return BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint.Position;
			}
			return BotOwner_0.Position;
		}
	}

	public Vector3 Position => BotOwner_0.Position;

	public float MoveSpeed => BotOwner_0.Mover.DestMoveSpeed;

	public List<string> DebugInfoFollower => Followers_1.DebugInfo();

	public List<BotOwner> Followers => Followers_1.Followers;

	public int FollowersTargetCount => Followers_1.TargetFollowersCount;

	public bool IsAlive => BotOwner_0.HealthController.IsAlive;

	public int Id => BotOwner_0.Id;

	public int TargetFollowersCount => Followers_1.TargetFollowersCount;

	public event Action<BotOwner, List<BotOwner>> OnBossDead;

	public event Action<BotOwner> OnBecomeBoss;

	public event Action<BotOwner, FollowerStatusChange> OnFollowerStatusChange;

	public static bool IsFollowerSuitableForBoss(WildSpawnType follower, WildSpawnType boss)
	{
		return BotSettingsRepoClass.IsSuitable(boss, follower);
	}

	public BotBoss(BotOwner owner)
		: base(owner)
	{
		WildSpawnType role = BotOwner_0.Profile.Info.Settings.Role;
		if (role != WildSpawnType.bossTest && role != WildSpawnType.bossGluhar)
		{
			Followers_1 = new GClass456(BotOwner_0);
		}
		else
		{
			Followers_1 = new GClass457(BotOwner_0);
		}
	}

	public PatrolPoint GetPatrolPosByIndex(int botFollowerIndex)
	{
		if (PatrollingData.CurPatrolPoint != null)
		{
			return PatrollingData.CurPatrolPoint.TargetPoint.GetSubPoint(botFollowerIndex);
		}
		return null;
	}

	public ABossLogic GetBossLogic()
	{
		return BotOwner_0.Boss.BossLogic;
	}

	public void ManualUpdate()
	{
		if (IamBoss)
		{
			Followers_1.CheckFollowers();
			BossLogic.BossLogicUpdate();
		}
	}

	public IPlayer Player()
	{
		return BotOwner_0;
	}

	public EnemyInfo CurEnemy()
	{
		return BotOwner_0.Memory.GoalEnemy;
	}

	public BotOwner GetFirstFollower(bool withGrenade)
	{
		int num = 0;
		BotOwner botOwner;
		while (true)
		{
			if (num < Followers_1.Followers.Count)
			{
				botOwner = Followers_1.Followers[num];
				if (botOwner.HealthController.IsAlive && (!withGrenade || botOwner.WeaponManager.Grenades.HaveGrenade))
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return botOwner;
	}

	public void SetBoss(int followersCount)
	{
		method_0();
		Followers_1.SetTargetFollowersCount(followersCount);
		IamBoss = true;
		Followers_1.UpdateFollowers();
		method_1();
		this.OnBecomeBoss?.Invoke(BotOwner_0);
	}

	public bool HaveFollowers()
	{
		return Followers_1.Followers.Any();
	}

	public void RemoveFollower(BotOwner botFollower)
	{
		Followers_1.Remove(botFollower);
		this.OnFollowerStatusChange?.Invoke(botFollower, FollowerStatusChange.Remove);
	}

	public bool IsMe(IPlayer player)
	{
		return false;
	}

	public void DeletePlayer(Player getPlayer)
	{
		if (IamBoss)
		{
			BotOwner_0.BotsGroup.BotGroupWarnData.PlayerDead(getPlayer);
		}
	}

	public bool OfferSelf(BotOwner offer)
	{
		if (Followers_1.Followers.Count >= Followers_1.TargetFollowersCount)
		{
			return false;
		}
		if (!IsFollowerSuitableForBoss(offer.Profile.Info.Settings.Role, BotOwner_0.Profile.Info.Settings.Role))
		{
			return false;
		}
		if (!offer.BotFollower.HaveBoss && !offer.Boss.IamBoss)
		{
			Followers_1.AddFollower(offer);
			this.OnFollowerStatusChange?.Invoke(offer, FollowerStatusChange.Add);
			offer.BotFollower.SetToFollow(this, Followers_1.Followers.Count - 1);
		}
		return true;
	}

	public void method_0()
	{
		switch (BotOwner_0.Profile.Info.Settings.Role)
		{
		case WildSpawnType.bossBully:
			BossLogic = new GClass432(BotOwner_0, this);
			break;
		case WildSpawnType.bossKojaniy:
		case WildSpawnType.followerKojaniy:
			BossLogic = new GClass446(BotOwner_0, this);
			NeedProtection = false;
			break;
		case WildSpawnType.bossGluhar:
			BossLogic = new GClass435(BotOwner_0, this);
			NeedProtection = true;
			break;
		case WildSpawnType.bossSanitar:
			BossLogic = new BossLogicClass(BotOwner_0, this);
			break;
		case WildSpawnType.sectantPriest:
			BossLogic = new GClass448(BotOwner_0, this);
			NeedProtection = false;
			break;
		case WildSpawnType.bossTest:
		case WildSpawnType.gifter:
			BossLogic = new GClass452(BotOwner_0, this);
			break;
		case WildSpawnType.bossKnight:
			BossLogic = new GClass440(BotOwner_0, this);
			NeedProtection = false;
			break;
		case WildSpawnType.bossZryachiy:
		case WildSpawnType.followerZryachiy:
			BossLogic = new ZyriachyBossLogicClass(BotOwner_0, this);
			NeedProtection = false;
			break;
		case WildSpawnType.bossBoar:
			BossLogic = new GClass430(BotOwner_0, this);
			NeedProtection = false;
			break;
		case WildSpawnType.bossBoarSniper:
			BossLogic = new GClass431(BotOwner_0, this);
			NeedProtection = false;
			break;
		case WildSpawnType.peacefullZryachiyEvent:
			BossLogic = new GClass450(BotOwner_0, this);
			break;
		case WildSpawnType.sectactPriestEvent:
			BossLogic = new GClass444(BotOwner_0, this);
			break;
		case WildSpawnType.ravangeZryachiyEvent:
			BossLogic = new GClass451(BotOwner_0, this);
			break;
		case WildSpawnType.bossKolontay:
			BossLogic = new GClass441(BotOwner_0, this);
			NeedProtection = false;
			break;
		case WildSpawnType.bossPartisan:
			BossLogic = new GClass442(BotOwner_0, this);
			NeedProtection = false;
			break;
		case WildSpawnType.sectantPredvestnik:
			BossLogic = new GClass447(BotOwner_0, this);
			NeedProtection = false;
			break;
		default:
			BossLogic = new GClass433(BotOwner_0, this);
			break;
		case WildSpawnType.followerTest:
		case WildSpawnType.followerBully:
		case WildSpawnType.pmcBot:
		case WildSpawnType.assaultGroup:
		case WildSpawnType.exUsec:
		case WildSpawnType.arenaFighter:
		case WildSpawnType.arenaFighterEvent:
		case WildSpawnType.crazyAssaultEvent:
		case WildSpawnType.shooterBTR:
		case WildSpawnType.peacemaker:
		case WildSpawnType.pmcBEAR:
		case WildSpawnType.pmcUSEC:
		case WildSpawnType.skier:
		case WildSpawnType.infectedAssault:
		case WildSpawnType.infectedPmc:
		case WildSpawnType.infectedCivil:
		case WildSpawnType.infectedLaborant:
			BossLogic = new GClass433(BotOwner_0, this);
			break;
		case WildSpawnType.bossTagilla:
		case WildSpawnType.infectedTagilla:
			BossLogic = new GClass437(BotOwner_0, this);
			break;
		case WildSpawnType.bossTagillaAgro:
			BossLogic = new GClass438(BotOwner_0, this);
			break;
		case WildSpawnType.bossKilla:
		case WildSpawnType.cursedAssault:
		case WildSpawnType.bossKillaAgro:
			BossLogic = new GClass436(BotOwner_0, this);
			break;
		case WildSpawnType.tagillaHelperAgro:
			BossLogic = new GClass434(BotOwner_0, this);
			NeedProtection = false;
			break;
		}
		BossLogic.Activate();
		BotOwner_0.BotsGroup.BossAppear(BotOwner_0);
	}

	public void method_1()
	{
		BossLogic.SetPatrolMode();
	}

	public void Dispose()
	{
		this.OnBecomeBoss = null;
		this.OnFollowerStatusChange = null;
		if (IamBoss)
		{
			BossLogic.Dispose();
			List<BotOwner> arg = Followers_1.Followers.ToList();
			BotOwner[] array = Followers_1.Followers.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].BotFollower.Dispose();
			}
			this.OnBossDead?.Invoke(BotOwner_0, arg);
			this.OnBossDead = null;
			Followers_1.Dispose();
			Followers_1.Clear();
		}
	}
}
