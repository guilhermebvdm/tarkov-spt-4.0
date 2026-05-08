using System.Collections.Generic;
using Comfort.Common;
using EFT;
using JetBrains.Annotations;

public class GClass480 : BotEnemyChooser
{
	public GClass480([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	public override EnemyInfo FindDangerEnemy()
	{
		if (BotOwner_0.Memory.GoalEnemy == null)
		{
			EnemyInfo enemyInfo = method_0();
			if (enemyInfo != null)
			{
				return enemyInfo;
			}
		}
		return base.FindDangerEnemy();
	}

	public EnemyInfo method_0()
	{
		if (BotOwner_0.Memory.GoalEnemy == null && BotOwner_0.Boss.Followers.Count > 0)
		{
			BotOwner botOwner = BotOwner_0.Boss.Followers[0];
			if (botOwner.Memory.HaveEnemy && !BotOwner_0.Memory.HaveEnemy)
			{
				BotOwner_0.BotsGroup.AddEnemy(botOwner.Memory.GoalEnemy.Person, EBotEnemyCause.KillaSyncTagilla);
				foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in BotOwner_0.EnemiesController.EnemyInfos)
				{
					if (Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(enemyInfo.Key.ProfileId) == Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(botOwner.Memory.GoalEnemy.Person.ProfileId))
					{
						BotOwner_0.BotsGroup.Enemies[enemyInfo.Key].Sync(botOwner.Memory.GoalEnemy);
						return enemyInfo.Value;
					}
				}
			}
		}
		return null;
	}
}
