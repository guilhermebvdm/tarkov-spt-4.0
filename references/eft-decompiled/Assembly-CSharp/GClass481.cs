using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using JetBrains.Annotations;

public class GClass481 : BotEnemyChooser
{
	[Serializable]
	[CompilerGenerated]
	public class Class241
	{
		public static readonly Class241 class241_0 = new Class241();

		public static Func<EnemyInfo, bool> func_0;

		public bool method_0(EnemyInfo x)
		{
			if (x.Person.HealthController.IsAlive)
			{
				return x.Person.Side != EPlayerSide.Savage;
			}
			return false;
		}
	}

	public GClass481([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	public override EnemyInfo FindDangerEnemy()
	{
		if (!BotOwner_0.Memory.HaveEnemy && BotOwner_0.BotFollower.HaveBoss)
		{
			EnemyInfo enemyInfo = method_0();
			if (enemyInfo != null)
			{
				return enemyInfo;
			}
		}
		if (BotOwner_0.Memory.IsInCover && BotOwner_0.Brain.BaseBrain.CurLayerInfo is GClass160 && (!BotOwner_0.Memory.HaveEnemy || BotOwner_0.Memory.GoalEnemy.Owner == null))
		{
			BotEnemiesController enemiesController = BotOwner_0.EnemiesController;
			List<EnemyInfo> list = enemiesController.EnemyInfos.Values.Where((EnemyInfo x) => x.Person.HealthController.IsAlive && x.Person.Side != EPlayerSide.Savage).ToList();
			if (list.Count == 0)
			{
				return base.FindDangerEnemy();
			}
			float num = float.MaxValue;
			IPlayer player = null;
			foreach (EnemyInfo item in list)
			{
				float num2 = 0f;
				float sqrMagnitude = (item.CurrPosition - BotOwner_0.Transform.position).sqrMagnitude;
				if ((!(sqrMagnitude > BotOwner_0.Settings.FileSettings.Boss.TAGILLA_FEEL_ENEMY_DIST_SQR) || item.IsVisible) && !item.Person.IsAI)
				{
					num2 += sqrMagnitude;
					if (num2 < num)
					{
						num = num2;
						player = item.Person;
					}
				}
			}
			if (player == null)
			{
				return base.FindDangerEnemy();
			}
			EnemyInfo enemyInfo2 = enemiesController.EnemyInfos[player];
			if (!BotOwner_0.BotsGroup.Enemies.ContainsKey(enemyInfo2.Person))
			{
				BotOwner_0.BotsGroup.AddEnemy(enemyInfo2.Person, EBotEnemyCause.tagillaFindENemy);
			}
			return enemyInfo2;
		}
		return base.FindDangerEnemy();
	}

	public EnemyInfo method_0()
	{
		if (BotOwner_0.IsFollower() && BotOwner_0.BotFollower.BossToFollow != null)
		{
			BotOwner botOwner = BotOwner_0.BotFollower.BossToFollow.Player().AIData.BotOwner;
			if (BotOwner_0.Memory.GoalEnemy == null && botOwner != null && botOwner.Memory.HaveEnemy)
			{
				BotOwner_0.BotsGroup.AddEnemy(botOwner.Memory.GoalEnemy.Person, EBotEnemyCause.synWithKilla);
				foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in BotOwner_0.EnemiesController.EnemyInfos)
				{
					if (Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(enemyInfo.Key.ProfileId) == Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(botOwner.Memory.GoalEnemy.Person.ProfileId))
					{
						BotOwner_0.BotsGroup.Enemies[enemyInfo.Key].Sync(botOwner.Memory.GoalEnemy);
						return enemyInfo.Value;
					}
				}
			}
			return null;
		}
		return null;
	}
}
