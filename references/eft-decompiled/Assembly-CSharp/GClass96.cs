using EFT;
using UnityEngine;

public class GClass96 : GClass95
{
	public GClass96(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		Vector3 position = base.GClass444_0.GetPosition(BotOwner_0.Id);
		Vector3 vector = position - BotOwner_0.Position;
		if (Mathf.Abs(vector.y) < 0.5f)
		{
			vector.y = 0f;
		}
		if (vector.sqrMagnitude > 1f)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToPoint, "notAtPlace", new GClass30(position));
		}
		BotOwner_0.Steering.LookToPoint(base.GClass444_0.GetSummonCenter());
		base.GClass444_0.StartRitual();
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.summon, "atPlace");
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndSummon()
	{
		if ((base.GClass444_0.GetPosition(BotOwner_0.Id) - BotOwner_0.Position).sqrMagnitude > 1f)
		{
			return new AICoreActionEndStruct("not my pos");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndGoToPoint()
	{
		if (BotOwner_0.GoToSomePointData.IsCome())
		{
			return new AICoreActionEndStruct("Come");
		}
		return AICoreActionEndStruct_1;
	}

	public override bool ShallUseNow()
	{
		if (base.GClass444_0 != null)
		{
			return base.GClass444_0.ShouldCast(BotOwner_0.Id);
		}
		return false;
	}

	public override string Name()
	{
		return "PrstSummon";
	}
}
