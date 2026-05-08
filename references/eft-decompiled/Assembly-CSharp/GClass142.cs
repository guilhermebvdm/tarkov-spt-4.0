using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class GClass142 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public const float Float_3 = 1f;

	[NonSerialized]
	public const float Float_4 = 10f;

	[NonSerialized]
	public float Float_5 = 100f;

	[NonSerialized]
	public float Float_6 = 5f;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public bool Bool_6 = true;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public BotFollower BotFollower_0;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	public float Float_9;

	[NonSerialized]
	public const float Float_10 = 5f;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_7;

	[NonSerialized]
	[CompilerGenerated]
	public CustomNavigationPoint CustomNavigationPoint_1;

	public bool HaveCoverToShoot
	{
		[CompilerGenerated]
		get
		{
			return Bool_7;
		}
		[CompilerGenerated]
		set
		{
			Bool_7 = value;
		}
	}

	public CustomNavigationPoint PointToShoot
	{
		[CompilerGenerated]
		get
		{
			return CustomNavigationPoint_1;
		}
		[CompilerGenerated]
		set
		{
			CustomNavigationPoint_1 = value;
		}
	}

	public GClass142(BotOwner bot, int priority)
		: base(bot, priority)
	{
		BotFollower_0 = bot.BotFollower;
	}

	public override bool ShallUseNow()
	{
		if (BotOwner_0.Memory.HaveEnemy)
		{
			if (!BotOwner_0.BotFollower.HaveBoss)
			{
				return BotOwner_0.Boss.IamBoss;
			}
			return true;
		}
		return false;
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		global::AICoreActionResultStruct<BotLogicDecision, GClass26>? aICoreActionResultStruct = InFightLogic();
		if (method_3())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "kg954");
		}
		if (aICoreActionResultStruct.HasValue)
		{
			if (Bool_6 && Time.time - BotOwner_0.Memory.ComeToCoverTime > 5f)
			{
				if (BotOwner_0.Memory.IsInCover)
				{
					BotOwner_0.Memory.Spotted(byHit: false);
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMovingFlank, "kay3");
			}
			return aICoreActionResultStruct.Value;
		}
		if (method_4())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "shootNow");
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "!haveEnemy");
		}
		try
		{
			bool canShoot = goalEnemy.CanShoot;
			bool flag = ProtectWantKill();
			bool flag2 = ProtectCareKill();
			method_14();
			if (!goalEnemy.IsVisible && BotOwner_0.SmokeGrenade.ShallShoot())
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootToSmoke, "SmokeGrenad");
			}
			if (HaveCoverToShoot && flag)
			{
				bool num = method_5(goalEnemy);
				bool flag3 = method_15();
				if (num && !flag3 && Time.time - goalEnemy.PersonalSeenTime < 3f)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "canShootLas");
				}
				if (!flag3 && goalEnemy.Distance > 10f)
				{
					BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(PointToShoot);
					if (BotOwner_0.CanSprintPlayer)
					{
						return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "goalEnemy.D");
					}
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "goal.D");
				}
				if (BotOwner_0.Memory.IsInCover && BotOwner_0.Memory.CurCustomCoverPoint.Id == PointToShoot.Id)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromCover, ".Memor");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, ".Memor");
			}
			bool flag4 = false;
			if (canShoot)
			{
				float sqrDist;
				BotOwner closestFriend = BotOwner_0.Covers.GetClosestFriend(out sqrDist);
				flag4 = !(sqrDist < LocalBotSettingsProviderClass.Core.MIN_DIST_CLOSE_DEF) || !(closestFriend != null) || closestFriend.Id > BotOwner_0.Id;
			}
			if (flag4)
			{
				if (goalEnemy.IsVisible)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "goalEnemy.V");
				}
				if (!method_12(BotOwner_0.Settings.FileSettings.Boss.IF_I_HITTED_GO_AWAY_SEC_HIT) && !BotOwner_0.Memory.IsUnderFire)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "deltaLastHi");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "deltaLastHi");
			}
			if (flag2)
			{
				if (!method_15() && Time.time - goalEnemy.PersonalSeenTime < 3f)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "goalEnemy.P");
				}
				if (flag)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToEnemy, "wantKill");
				}
			}
			if (BotOwner_0.Memory.IsInCover)
			{
				if (BotOwner_0.Medecine.FirstAid.Have2Do && (BotOwner_0.Memory.LastEnemy == null || Time.time - BotOwner_0.Memory.LastEnemyTimeSeen > BotOwner_0.Settings.FileSettings.Mind.PROTECT_DELTA_HEAL_SEC))
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "PROTECTDELT");
				}
				Vector3 vector = method_17();
				if ((BotOwner_0.Position - vector).sqrMagnitude > Float_5)
				{
					return method_18();
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "distToBoss");
			}
			if (HaveCoverToShoot)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "HaveCoverSh");
			}
			return method_18();
		}
		catch (Exception message)
		{
			if (!Bool_5)
			{
				Debug.LogError(message);
				Bool_5 = true;
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "erorrLoged");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "erorrLoged2");
		}
	}

	public CustomNavigationPoint FollowerCheckData()
	{
		Vector3 vector = ((!BotOwner_0.BotFollower.HaveBoss) ? BotOwner_0.Position : BotOwner_0.BotFollower.BossToFollow.Position);
		ShootPointClass shootPointClass = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		CoverShootType shootType = CoverShootType.shoot;
		if (shootPointClass == null)
		{
			shootType = CoverShootType.hide;
		}
		CoverSearchData data = new CoverSearchData(vector, BotOwner_0.CoverSearchInfo, shootType, LocalBotSettingsProviderClass.Core.START_DIST_TO_COV, 0f, CoverSearchType.closerToSelectedPoint, shootPointClass, null, vector, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(0f));
		return BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: true);
	}

	public override string Name()
	{
		return "Pmc";
	}

	public void method_13()
	{
		if (!(Float_7 < Time.time))
		{
			return;
		}
		Vector3 vector = method_17();
		Float_7 = Time.time + 1f;
		CoverSearchData data = new CoverSearchData(vector, BotOwner_0.CoverSearchInfo, CoverShootType.hide, Float_5, 0f, CoverSearchType.closerToSelectedPoint, null, null, vector, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(0f));
		CustomNavigationPoint_0 = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: true);
		if (CustomNavigationPoint_0 != null)
		{
			if ((vector - CustomNavigationPoint_0.Position).sqrMagnitude < Float_5 && !CustomNavigationPoint_0.IsSpotted)
			{
				Bool_4 = true;
			}
			else
			{
				Bool_4 = false;
			}
		}
		else
		{
			Bool_4 = false;
		}
	}

	public override AICoreActionEndStruct EndShootFromCover()
	{
		if (Bool_6 && Time.time - BotOwner_0.Memory.ComeToCoverTime > 5f)
		{
			return new AICoreActionEndStruct("ghyFlank");
		}
		return base.EndShootFromCover();
	}

	public override AICoreActionEndStruct EndAttackMovingFlank()
	{
		if (method_3())
		{
			return new AICoreActionEndStruct("96jk");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("jh7");
		}
		return AICoreActionEndStruct_1;
	}

	public void method_14()
	{
		if (!(Float_8 < Time.time))
		{
			return;
		}
		Float_8 = 1f + Time.time;
		Vector3 vector = ((!BotFollower_0.HaveBoss) ? BotOwner_0.Position : BotFollower_0.BossToFollow.Position);
		PointToShoot = FollowerCheckData();
		if (PointToShoot != null && PointToShoot.IsFreeById(BotOwner_0.Id) && !PointToShoot.IsSpotted)
		{
			float sqrMagnitude = (vector - PointToShoot.Position).sqrMagnitude;
			_ = BotOwner_0.Memory.GoalEnemy;
			if (sqrMagnitude < BotOwner_0.Settings.FileSettings.Boss.MAX_DIST_COVER_BOSS_SQRT)
			{
				if (ProtectCareKill())
				{
					bool canIShootToEnemy = PointToShoot.CanIShootToEnemy;
					HaveCoverToShoot = canIShootToEnemy;
				}
				else
				{
					HaveCoverToShoot = true;
				}
				if (HaveCoverToShoot && (BotOwner_0.Memory.CurCustomCoverPoint == null || BotOwner_0.Memory.CurCustomCoverPoint.Id != PointToShoot.Id))
				{
					BotOwner_0.Memory.BotCurrentCoverInfo.Spotted();
					BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(PointToShoot);
				}
			}
			else
			{
				HaveCoverToShoot = false;
			}
		}
		else
		{
			HaveCoverToShoot = false;
		}
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return new AICoreActionEndStruct("enemynull");
		}
		if (BotOwner_0.DogFight.ShallStartCauseHavePlace())
		{
			return new AICoreActionEndStruct("StartH");
		}
		if (!goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("!enemy.CanS");
		}
		if (method_4())
		{
			return AICoreActionEndStruct_1;
		}
		if (method_3())
		{
			return new AICoreActionEndStruct("StartD");
		}
		if (goalEnemy.Distance < 1f)
		{
			return new AICoreActionEndStruct("enemy.Dista");
		}
		if (BotOwner_0.WeaponManager.Reload.Reloading)
		{
			return new AICoreActionEndStruct(".Reloa");
		}
		return AICoreActionEndStruct_1;
	}

	public bool method_15()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && goalEnemy.CanShoot)
		{
			return goalEnemy.IsVisible;
		}
		return false;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (method_4())
		{
			return new AICoreActionEndStruct("ShootImmediately");
		}
		method_14();
		return base.EndRunToCover();
	}

	public virtual bool ProtectWantKill()
	{
		return Time.time - BotOwner_0.BotsGroup.EnemyLastSeenTimeReal < BotOwner_0.Settings.FileSettings.Mind.ATTACK_ENEMY_IF_PROTECT_DELTA_LAST_TIME_SEEN;
	}

	public virtual bool ProtectCareKill()
	{
		bool result;
		if (!(result = Time.time - method_19() < BotOwner_0.Settings.FileSettings.Mind.HOLD_IF_PROTECT_DELTA_LAST_TIME_SEEN))
		{
			return false;
		}
		return result;
	}

	public override AICoreActionEndStruct EndGoToEnemy()
	{
		method_14();
		bool num = ProtectWantKill();
		bool flag = ProtectCareKill();
		if (num && flag && !HaveCoverToShoot)
		{
			return base.EndGoToEnemy();
		}
		return new AICoreActionEndStruct("resEndG1");
	}

	public override AICoreActionEndStruct EndAttackMoving()
	{
		method_14();
		if (HaveCoverToShoot && BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("pmcFindCove");
		}
		return base.EndAttackMoving();
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		method_14();
		if (!Bool_4 && !HaveCoverToShoot)
		{
			return new AICoreActionEndStruct("!CoverNearB");
		}
		return base.EndGoToCoverPoint();
	}

	public override AICoreActionEndStruct EndGoToPoint()
	{
		AICoreActionEndStruct result = method_16();
		if (result.Value)
		{
			Float_9 = Time.time;
		}
		return result;
	}

	public AICoreActionEndStruct method_16()
	{
		method_14();
		if (Bool_4)
		{
			return new AICoreActionEndStruct("haveCoverN");
		}
		if ((BotOwner_0.GoToSomePointData.Point - method_17()).sqrMagnitude > Float_6 * Float_6)
		{
			return new AICoreActionEndStruct(">CloseBoss");
		}
		if (BotOwner_0.GoToSomePointData.IsCome())
		{
			return new AICoreActionEndStruct("AtPoint");
		}
		return base.EndGoToPoint();
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		method_14();
		Vector3 vector = method_17();
		if ((BotOwner_0.Position - vector).sqrMagnitude > Float_5)
		{
			return new AICoreActionEndStruct(">CloseBoss");
		}
		if (HaveCoverToShoot && ProtectWantKill() && ProtectCareKill())
		{
			return new AICoreActionEndStruct("havecoverto");
		}
		return base.EndHoldPosition();
	}

	public Vector3 method_17()
	{
		if (BotOwner_0.BotFollower.BossToFollow != null)
		{
			return BotOwner_0.BotFollower.BossToFollow.Position;
		}
		return BotOwner_0.Position;
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_18()
	{
		Vector3 vector = method_17();
		method_13();
		if (Bool_4)
		{
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(CustomNavigationPoint_0);
			if (BotOwner_0.CanSprintPlayer)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "hyt3");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "ph4");
		}
		float b = 2.5f;
		float x = GClass856.Random(0.5f, b) * (float)GClass856.RandomSing();
		float z = GClass856.Random(0.5f, b) * (float)GClass856.RandomSing();
		Vector3 vector2 = vector + new Vector3(x, 0f, z);
		CustomNavigationPoint closestPoint = BotOwner_0.Covers.GetClosestPoint(vector2);
		if (closestPoint != null)
		{
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(closestPoint);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "lkgr3");
		}
		if (Time.time - Float_9 > 10f && NavMesh.SamplePosition(vector2, out var hit, Float_6, -1))
		{
			Vector3_0 = hit.position;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToPoint, "lj6", new GClass30(Vector3_0));
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			HoldFor(4f);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "oyh3");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "ljfe2");
	}

	public float method_19()
	{
		if (BotOwner_0.Settings.FileSettings.Mind.PROTECT_TIME_REAL)
		{
			return BotOwner_0.BotsGroup.EnemyLastSeenTimeReal;
		}
		return BotOwner_0.BotsGroup.EnemyLastSeenTimeSence;
	}
}
