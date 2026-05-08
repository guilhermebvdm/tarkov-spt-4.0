using System;
using EFT;
using UnityEngine;

public class GClass45(BotOwner bot, int priority) : BaseLogicLayerSimpleAbstractClass(bot, priority)
{
	[NonSerialized]
	public float Float_3 = -1f;

	[NonSerialized]
	public float Float_4;

	public override bool ShallUseNow()
	{
		if (BotOwner_0.Memory.GoalEnemy != null)
		{
			return BotOwner_0.Memory.GoalEnemy.Distance > method_13();
		}
		return false;
	}

	public float method_13()
	{
		if (BotOwner_0.WeaponManager.CurrentWeapon is ShotgunItemClass)
		{
			return BotOwner_0.Settings.FileSettings.Shoot.FAR_DISTANCE_SHOTGUNS;
		}
		if (!(BotOwner_0.WeaponManager.CurrentWeapon is PistolItemClass) && !(BotOwner_0.WeaponManager.CurrentWeapon is RevolverItemClass))
		{
			return BotOwner_0.Settings.FileSettings.Shoot.FAR_DISTANCE_ALL_WEAPONS;
		}
		return BotOwner_0.Settings.FileSettings.Shoot.FAR_DISTANCE_PISTOLS;
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		data.coverSearchDefenceData.minDefenceLevel = 22f;
		data.coverSearchDefenceData.maxDangerLevel = 0;
		data.CheckShootHide = ECheckSHootHide.hide;
		data.shootType = CoverShootType.hide;
		return base.FindPoint(data, p, checkCurrent: false);
	}

	public override string Name()
	{
		return "AssaultEnemyFar";
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.Memory.IsInCover && !BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "holdfar");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToCoverPoint, "coverfar");
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (BotOwner_0.Memory.GoalEnemy.IsVisible)
		{
			if (Float_3 < 0f)
			{
				Float_3 = Time.time + BotOwner_0.Settings.FileSettings.Shoot.FAR_DIST_EYE_CONTACT_TIME_TO_CHANGE_COVER;
				return AICoreActionEndStruct_1;
			}
			if (Time.time > Float_3)
			{
				BotOwner_0.Memory.Spotted(byHit: true, null, 10f);
				Float_3 = -1f;
				return new AICoreActionEndStruct("SeenPersonal");
			}
			return AICoreActionEndStruct_1;
		}
		Float_3 = -1f;
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		if (goalEnemy == null && CanSearchEnemy())
		{
			return new AICoreActionEndStruct("CanSearchEn");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		_ = BotOwner_0.Memory.GoalEnemy;
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("InCover");
		}
		if (method_3())
		{
			return new AICoreActionEndStruct("StartD");
		}
		if (BotOwner_0.Memory.CurCustomCoverPoint != null && BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
		{
			return new AICoreActionEndStruct("IsSpotted");
		}
		return AICoreActionEndStruct_1;
	}
}
