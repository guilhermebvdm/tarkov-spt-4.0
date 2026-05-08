using EFT;
using UnityEngine;

public class GClass85 : GClass84
{
	public GClass85(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override bool ShallUseNow()
	{
		if (!base.ShallUseNow())
		{
			return false;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && Time.time - goalEnemy.PersonalLastSeenTime <= 15f)
		{
			return false;
		}
		return true;
	}
}
