using System;
using EFT;

public class GClass147 : GClass146
{
	[NonSerialized]
	public bool Bool_12;

	public GClass147(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override bool ShallUseNow()
	{
		if (Ginterface7_0 != null)
		{
			return BotOwner_0.Memory.HaveEnemy;
		}
		return false;
	}

	public override string Name()
	{
		return "BossSanitarFight";
	}

	public override void OnActivate()
	{
		if (BotOwner_0.Boss.BossLogic is GInterface7 bossData)
		{
			SetBossData(bossData);
		}
		BotOwner_0.HealAnotherTarget.OnHealAsked += method_38;
		base.OnActivate();
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (Bool_12)
		{
			Bool_12 = false;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.healAnotherTarget, "_healAsked");
		}
		return base.GetDecision();
	}

	public override CustomNavigationPoint FindPointForFight(bool checkCurrent)
	{
		if (CustomNavigationPoint_0 == null)
		{
			checkCurrent = false;
		}
		ShootPointClass shootPointClass = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		CoverShootType shootType = CoverShootType.shoot;
		if (shootPointClass == null)
		{
			shootType = CoverShootType.hide;
		}
		PointsArrayType arrayType = (method_14() ? PointsArrayType.covers : PointsArrayType.both);
		float maxDistSqr = (Bool_5 ? 256f : 900f);
		int value = (Bool_5 ? 6 : 14);
		CoverSearchData coverSearchData = new CoverSearchData(BotOwner_0.Position, BotOwner_0.CoverSearchInfo, shootType, maxDistSqr, 0f, CoverSearchType.closerToSelectedPoint, shootPointClass, null, BotOwner_0.Position, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(0f), PointsArrayType.both, useSelfFindPoint: true, null, value);
		coverSearchData.UseSelfFindPoint = checkCurrent;
		coverSearchData.ArrayType = arrayType;
		coverSearchData.UseLineCastToCover = true;
		CustomNavigationPoint coverPointMain = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(coverSearchData, checkCurrent);
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(coverPointMain);
		Bool_4 = true;
		return coverPointMain;
	}

	public void method_38(IPlayer obj)
	{
		Bool_12 = true;
	}

	public override void Dispose()
	{
		BotOwner_0.HealAnotherTarget.OnHealAsked -= method_38;
		base.Dispose();
	}
}
