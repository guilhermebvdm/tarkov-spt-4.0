using System;
using EFT;
using UnityEngine;

public class GClass91 : BaseLogicLayerSimpleAbstractClass
{
	public GClass91(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (!BotOwner_0.Medecine.FirstAid.Have2Do && !BotOwner_0.Medecine.SurgicalKit.HaveWork)
		{
			if (BotOwner_0.Memory.IsInCover)
			{
				Vector3 vector = method_13();
				Vector3 vector2 = BotOwner_0.Memory.CurCustomCoverPoint.Position - vector;
				if (Mathf.Abs(vector2.y) < LocalBotSettingsProviderClass.Core.MAX_Y_DIFF_TO_PROTECT && vector2.sqrMagnitude < LocalBotSettingsProviderClass.Core.COVER_TOOFAR_FROM_BOSS_SQRT)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "holdFine");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "hold!Fine");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCoverRun(BotOwner_0), "holdOrRun");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "first aid");
		}
		if (method_12(20f))
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "goforheal");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "heal now");
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		data.CenterPos = method_13();
		return base.FindPoint(data, p, checkCurrent);
	}

	public override bool ShallUseNow()
	{
		if (BotOwner_0.Memory.HaveGoal)
		{
			return BotOwner_0.BotFollower.HaveBoss;
		}
		return false;
	}

	public override string Name()
	{
		return "HoldNearBoss";
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			if (!BotOwner_0.Memory.IsInCover)
			{
				return new AICoreActionEndStruct("IsInCover");
			}
		}
		else
		{
			if (goalEnemy.IsVisible && goalEnemy.CanShoot)
			{
				return new AICoreActionEndStruct("CanShoot");
			}
			if (goalEnemy.IsVisible && goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Cover.END_HOLD_IF_ENEMY_CLOSE_AND_VISIBLE)
			{
				return new AICoreActionEndStruct("IFENEMYCLOS");
			}
		}
		return AICoreActionEndStruct_1;
	}

	public Vector3 method_13()
	{
		if (BotOwner_0.Boss.IamBoss)
		{
			return BotOwner_0.Position;
		}
		return BotOwner_0.BotFollower.BossToFollow?.PositionIfInCover ?? BotOwner_0.Position;
	}
}
