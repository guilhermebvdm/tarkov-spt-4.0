using System;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class GClass72(BotOwner bot, int priority) : BaseLogicLayerSimpleAbstractClass(bot, priority)
{
	[NonSerialized]
	public float Float_3 = 40000f;

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.Memory.HaveEnemy && !BotOwner_0.Memory.GoalEnemy.IsVisible)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToEnemy, "FindE");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.followPlayer, "follow");
	}

	public override void OnActivate()
	{
		Singleton<BotEventHandler>.Instance.OnFlareDelegate += method_13;
		base.OnActivate();
	}

	public void method_13(IPlayer player, Vector3 position, AmmoTemplate ammoTemplate)
	{
		if (!BotOwner_0.PlayerFollowData.HaveFound && !player.IsAI && player.Id != BotOwner_0.AIData.Player.Id && ammoTemplate.ContainsType(FlareEventType.AIFollowEvent) && (player.Position - BotOwner_0.Position).sqrMagnitude < Float_3)
		{
			BotOwner_0.PlayerFollowData.SetToFollow(player);
		}
	}

	public override void Dispose()
	{
		Singleton<BotEventHandler>.Instance.OnFlareDelegate -= method_13;
		base.Dispose();
	}

	public override AICoreActionEndStruct EndPlayerFollow()
	{
		if (BotOwner_0.Memory.HaveEnemy && !BotOwner_0.Memory.GoalEnemy.IsVisible)
		{
			return new AICoreActionEndStruct("needAttack");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndAttackMoving()
	{
		return AICoreActionEndStruct;
	}

	public override bool ShallUseNow()
	{
		if (!BotOwner_0.PlayerFollowData.HaveFound)
		{
			return false;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && Time.time - goalEnemy.PersonalLastSeenTime < 20f)
		{
			return false;
		}
		if (!BotOwner_0.PlayerFollowData.HaveToFollow)
		{
			return false;
		}
		if (!BotOwner_0.PlayerFollowData.IsEnoughtDist())
		{
			return false;
		}
		return true;
	}

	public override string Name()
	{
		return "Follow Player";
	}
}
