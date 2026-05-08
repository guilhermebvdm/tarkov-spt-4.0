using System;
using EFT;
using UnityEngine;

public class GClass164 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public bool Bool_5;

	public GClass164(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.suppressFire, "TstSuppress");
	}

	public override AICoreActionEndStruct EndSuppressFire()
	{
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		return base.EndGoToCoverPoint();
	}

	public override AICoreActionEndStruct EndAttackMoving()
	{
		return base.EndAttackMoving();
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (BotOwner_0.WeaponManager.Reload.Reloading)
		{
			data = new CoverSearchData(data.CenterPos, data.Bot, CoverShootType.hide, data.MaxDistSqr, 0f, CoverSearchType.distToBot, data.Shoot2Point, null, null, data.CheckShootHide, new CoverSearchDefenceDataClass(BotOwner_0.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL));
			return base.FindPoint(data, p, checkCurrent);
		}
		Vector3 vector = ((BotOwner_0.Memory.GoalEnemy != null) ? BotOwner_0.Memory.GoalEnemy.CurrPosition : BotOwner_0.Position);
		data = new CoverSearchData(vector, BotOwner_0.CoverSearchInfo, CoverShootType.shoot, data.MaxDistSqr, 0f, CoverSearchType.distToToCenter, data.Shoot2Point, null, vector, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(BotOwner_0.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL, 999));
		return base.FindPoint(data, p, checkCurrent);
	}

	public override bool ShallUseNow()
	{
		return true;
	}

	public override string Name()
	{
		return "TestLayer";
	}
}
