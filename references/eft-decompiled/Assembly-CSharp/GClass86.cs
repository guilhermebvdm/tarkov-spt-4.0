using System;
using EFT;
using UnityEngine;

public class GClass86 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public float Float_3;

	public GClass86(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		BotOwner_0.DeadBodyWork.UpdateCheck();
		if (BotOwner_0.DeadBodyWork.ShallUse)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.deadBody, "DeadBodyWor");
		}
		if (BotOwner_0.PeaceHardAim.HaveActions())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.peaceHardAim, "PeaceHardAi");
		}
		if (BotOwner_0.PeaceLook.HaveActions())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.peaceLook, "PeaceLook");
		}
		if (BotOwner_0.SecondWeaponData.HaveActions())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.watchSecondWeapon, "Look2ndWeap");
		}
		BotOwner_0.ItemTaker.RefreshClosestItems();
		if (BotOwner_0.ItemTaker.HaveItemToTake())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.botTakeItem, "PlanDropIte");
		}
		if ((float)BotOwner_0.WeaponManager.Reload.BulletCount / (float)BotOwner_0.WeaponManager.Reload.MaxBulletCount < 0.6f && Float_3 < Time.time)
		{
			Float_3 = Time.time + 30f;
			BotOwner_0.WeaponManager.Reload.TryReload();
		}
		BotOwner_0.PatrollingData.SetTargetMoveSpeed();
		BotOwner_0.PatrollingData.PointChooser.ShallChangeWay();
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.followerPatrol, "Basic");
	}

	public override string Name()
	{
		return "PatrolFollower";
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.BotFollower.HaveBoss;
	}

	public override AICoreActionEndStruct EndSimplePatrol()
	{
		if (method_13(out var reason))
		{
			return new AICoreActionEndStruct(reason);
		}
		if (BotOwner_0.PatrollingData.Way.PatrolType == PatrolType.reserved)
		{
			return new AICoreActionEndStruct("way is alt");
		}
		if (BotOwner_0.BotFollower.HaveBoss && !BotOwner_0.Boss.IamBoss)
		{
			return new AICoreActionEndStruct("new boss");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndWatchSecondWeapon()
	{
		if (BotOwner_0.SecondWeaponData.HaveActions())
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndAlternativePatrol()
	{
		if (method_13(out var reason))
		{
			return new AICoreActionEndStruct(reason);
		}
		if (BotOwner_0.PatrollingData.Way.PatrolType == PatrolType.reserved)
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("no alt way");
	}

	public override AICoreActionEndStruct EndFollowerPatrolItem()
	{
		if (BotOwner_0.Boss.IamBoss)
		{
			return new AICoreActionEndStruct("IamBoss");
		}
		if (!BotOwner_0.BotFollower.HaveBoss)
		{
			return new AICoreActionEndStruct("no boss");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndSearch()
	{
		return AICoreActionEndStruct;
	}

	public bool method_13(out string reason)
	{
		if (BotOwner_0.EatDrinkData.HaveActions())
		{
			reason = "eatDrink";
			return true;
		}
		if (BotOwner_0.FriendlyTilt.HaveActions())
		{
			reason = "FriendlyTilt";
			return true;
		}
		if (BotOwner_0.Gesture.HaveRequest())
		{
			reason = "Gesture";
			return true;
		}
		reason = null;
		return false;
	}
}
