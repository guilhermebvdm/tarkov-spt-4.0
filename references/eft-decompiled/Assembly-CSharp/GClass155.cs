using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass155 : GClass150
{
	public const float DIST_CHANGE_TO_MEELE_FROM_FIRE = 5f;

	public const float PERIOD_SEEN_TOATTACK = 4f;

	[NonSerialized]
	public const string String_0 = "SupShootSect_IN";

	[NonSerialized]
	public const string String_1 = "SupShootSect_OUT";

	[NonSerialized]
	public const float Float_3 = 30f;

	[NonSerialized]
	public GClass448 Gclass448_0;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public string String_2 = "SupShootSect_IN";

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public GClass360 Gclass360_0;

	[NonSerialized]
	public bool Bool_6 = true;

	[NonSerialized]
	[CompilerGenerated]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_7;

	public CustomNavigationPoint PointToShoot
	{
		[CompilerGenerated]
		get
		{
			return CustomNavigationPoint_0;
		}
		[CompilerGenerated]
		set
		{
			CustomNavigationPoint_0 = value;
		}
	}

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

	public GClass155(BotOwner bot, int priority, GClass360 strategy)
		: base(bot, priority)
	{
		Gclass360_0 = strategy;
	}

	public override bool ShallUseNow()
	{
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			return false;
		}
		if (!method_14())
		{
			return false;
		}
		return true;
	}

	public override string Name()
	{
		return String_2;
	}

	public override void OnActivate()
	{
		base.OnActivate();
	}

	public void SetInside(bool isInside)
	{
		Bool_5 = isInside;
		String_2 = (Bool_5 ? "SupShootSect_IN" : "SupShootSect_OUT");
	}

	public void SetDelay(float delay)
	{
		Bool_4 = true;
		Float_5 = Time.time + delay;
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		global::AICoreActionResultStruct<BotLogicDecision, GClass26>? aICoreActionResultStruct = InFightLogic();
		if (aICoreActionResultStruct.HasValue)
		{
			return aICoreActionResultStruct.Value;
		}
		method_13();
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "enemynull");
		}
		if (goalEnemy.Distance < 5f && Gclass448_0.CanChangeToMeleeFromSupport() && Gclass360_0.CanRunToEnemyWithShortPath())
		{
			Gclass448_0.ChangeToMeleeFromSUpport();
			if (Gclass360_0.TryChangeToMelee())
			{
				Gclass360_0.ReturnToSupportShoot(20f);
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.oneMeleeAttack, "ReturnSuppo");
			}
		}
		if (method_14())
		{
			if (HaveCoverToShoot)
			{
				if (goalEnemy.Distance > 30f)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "DistEnemyCa");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "DistEnemyCa");
			}
			if (Bool_5)
			{
				if (goalEnemy.CanShoot)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "canshootnow");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToEnemy, "can'tshoot");
			}
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "last");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "last");
	}

	public CustomNavigationPoint FollowerCheckData()
	{
		Vector3 position = BotOwner_0.Position;
		ShootPointClass shootPointClass = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		CoverShootType shootType = CoverShootType.shoot;
		if (shootPointClass == null)
		{
			shootType = CoverShootType.hide;
		}
		CoverSearchData data = new CoverSearchData(position, BotOwner_0.CoverSearchInfo, shootType, 4f, 0f, CoverSearchType.distToBot, shootPointClass, null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(0f), PointsArrayType.byShootType, useSelfFindPoint: false, null, 15);
		return BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: false);
	}

	public void SetBoss(GClass448 sectantPriest)
	{
		Gclass448_0 = sectantPriest;
	}

	public void SetCorePosition(Vector3 corePoint)
	{
		Vector3_0 = corePoint;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (!BotOwner_0.Memory.IsInCover && BotOwner_0.CanSprintPlayer && (BotOwner_0.Memory.CurCustomCoverPoint == null || !BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted))
		{
			return AICoreActionEndStruct_1;
		}
		BotOwner_0.BotRun.EndMove();
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndOneMeleeAttack()
	{
		return AICoreActionEndStruct;
	}

	public void method_13()
	{
		if (!(Float_4 < Time.time))
		{
			return;
		}
		Float_4 = 1f + Time.time;
		PointToShoot = FollowerCheckData();
		bool flag;
		if (flag = PointToShoot != null && PointToShoot.CanIShootToEnemy)
		{
			if (Bool_6)
			{
				float sqrMagnitude = (PointToShoot.Position - BotOwner_0.Position).sqrMagnitude;
				HaveCoverToShoot = sqrMagnitude < 900f;
			}
			else
			{
				HaveCoverToShoot = true;
			}
		}
		else
		{
			HaveCoverToShoot = false;
		}
		if (HaveCoverToShoot && flag)
		{
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(PointToShoot);
		}
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (method_14())
		{
			method_13();
			EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
			if (HaveCoverToShoot)
			{
				if (goalEnemy != null && Time.time - goalEnemy.TimeLastSeen < 4f)
				{
					return new AICoreActionEndStruct("HaveCoverSh");
				}
			}
			else if (Bool_5)
			{
				return new AICoreActionEndStruct("isInsidenoc");
			}
		}
		return base.EndHoldPosition();
	}

	public bool method_14()
	{
		if (Bool_4)
		{
			return Time.time > Float_5;
		}
		return false;
	}

	public override void Dispose()
	{
		base.Dispose();
	}
}
