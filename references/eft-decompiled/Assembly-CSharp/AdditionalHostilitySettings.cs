using System;
using EFT;

[Serializable]
public class AdditionalHostilitySettings
{
	[Serializable]
	public class ChancedEnemy
	{
		public int EnemyChance;

		public WildSpawnType Role;
	}

	public WildSpawnType BotRole;

	public WildSpawnType[] AlwaysEnemies = new WildSpawnType[0];

	public ChancedEnemy[] ChancedEnemies = new ChancedEnemy[0];

	public WildSpawnType[] Warn = new WildSpawnType[0];

	public WildSpawnType[] Neutral = new WildSpawnType[0];

	public WildSpawnType[] AlwaysFriends = new WildSpawnType[0];

	public EWarnBehaviour SavagePlayerBehaviour;

	public int SavageEnemyChance;

	public EWarnBehaviour BearPlayerBehaviour;

	public int BearEnemyChance;

	public EWarnBehaviour UsecPlayerBehaviour;

	public int UsecEnemyChance;
}
