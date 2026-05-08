using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class GClass116 : GClass108
{
	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public GClass453<GClass441> Gclass453_0;

	public GClass116(BotOwner bot, int priority)
		: base(bot, priority)
	{
		Gclass453_0 = new GClass453<GClass441>(bot);
		Gclass453_0.FindBoss();
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		return method_13();
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.Memory.HaveGoal;
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		data.PlaceInfo = Gclass453_0.BossLogic.AreaId;
		return base.FindPoint(data, p, checkCurrent);
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (method_7())
		{
			return new AICoreActionEndStruct("EndHol");
		}
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		if (!BotOwner_0.Memory.CurCustomCoverPoint.IsFreeById(BotOwner_0.Id))
		{
			return new AICoreActionEndStruct("notFree");
		}
		return AICoreActionEndStruct_1;
	}

	public override void ManualUpdate()
	{
		if (Float_6 < Time.time)
		{
			Float_6 = Time.time + 3f;
			foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in BotOwner_0.EnemiesController.EnemyInfos)
			{
				if (enemyInfo.Value.Distance < 30f)
				{
					BotOwner_0.BotsGroup.SetEnemyPos(enemyInfo.Key, enemyInfo.Value.CurrPosition, enemyInfo.Key.WeaponRoot.position, EEnemyPartVisibleType.Sence);
				}
			}
		}
		base.ManualUpdate();
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (method_14())
		{
			Float_4 = Time.time;
			return new AICoreActionEndStruct("CvrNtFnd");
		}
		return base.EndRunToCover();
	}

	public override void Dispose()
	{
		Gclass453_0.Dispose();
		base.Dispose();
	}

	public override string Name()
	{
		return "KlnTrg";
	}
}
