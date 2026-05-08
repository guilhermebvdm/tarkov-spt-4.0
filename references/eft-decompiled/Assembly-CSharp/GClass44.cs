using EFT;
using UnityEngine;

public class GClass44 : GClass43
{
	public const float PERIOD_WAIT_LASTENEMY_SEC = 900f;

	public override Vector3? TargetPosition => BotOwner_0.Memory.GoalTarget.Position;

	public override int TargetEnvironmentId => BotOwner_0.Memory.GoalTarget.EnvironmentId;

	public GClass44(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override bool ShallUseNow()
	{
		EnemyInfo lastEnemy = BotOwner_0.Memory.LastEnemy;
		if (lastEnemy != null && lastEnemy.Person.AIData.EnvironmentId > 0 && Time.time - lastEnemy.GroupInfo.EnemyLastSeenTimeReal < 900f)
		{
			return true;
		}
		if (!BotOwner_0.Memory.HaveGoal)
		{
			return false;
		}
		if (BotOwner_0.Memory.GoalTarget.EnvironmentId > 0)
		{
			return true;
		}
		return false;
	}

	public override string Name()
	{
		return "Goal Building";
	}
}
