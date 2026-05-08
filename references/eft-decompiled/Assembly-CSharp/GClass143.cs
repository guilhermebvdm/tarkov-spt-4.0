using EFT;
using UnityEngine;

public class GClass143 : GClass142
{
	public GClass143(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override string Name()
	{
		return "Follower bully";
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		AICoreActionEndStruct result = base.EndHoldPosition();
		if (result.Value)
		{
			return result;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && Time.time - goalEnemy.GroupInfo.LastChangeVisionTime > 1.5f && (ProtectWantKill() & ProtectCareKill()))
		{
			return new AICoreActionEndStruct("shall kill");
		}
		return AICoreActionEndStruct_1;
	}
}
