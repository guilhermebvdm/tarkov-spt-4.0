using System;
using System.Collections.Generic;
using System.Text;
using EFT;
using JetBrains.Annotations;

public class BotEnemyChooser : GClass429
{
	[NonSerialized]
	public StringBuilder LoggerBuilder;

	[NonSerialized]
	public int OldHashBetterLog;

	[NonSerialized]
	public int OldHashPrepareLog;

	public static BotEnemyChooser Create(BotOwner owner)
	{
		switch (owner.Profile.Info.Settings.Role)
		{
		case WildSpawnType.bossTest:
			return new GClass482(owner);
		case WildSpawnType.followerBigPipe:
			return new GClass479(owner);
		case WildSpawnType.bossGluhar:
		case WildSpawnType.followerGluharAssault:
		case WildSpawnType.followerGluharSecurity:
		case WildSpawnType.followerGluharScout:
		case WildSpawnType.followerGluharSnipe:
			return new GClass476(owner);
		case WildSpawnType.arenaFighterEvent:
			return new GClass478(owner);
		case WildSpawnType.followerZryachiy:
			return new GClass475(owner);
		default:
			return new BotEnemyChooser(owner);
		case WildSpawnType.bossKilla:
		case WildSpawnType.bossKillaAgro:
			return new GClass480(owner);
		case WildSpawnType.bossTagilla:
		case WildSpawnType.followerTagilla:
		case WildSpawnType.infectedTagilla:
		case WildSpawnType.bossTagillaAgro:
			return new GClass481(owner);
		case WildSpawnType.ravangeZryachiyEvent:
			return new GClass477(owner);
		}
	}

	public BotEnemyChooser([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	[CanBeNull]
	public virtual EnemyInfo FindDangerEnemy()
	{
		bool flag;
		if (flag = GClass398.Instance.IsTraceEnable())
		{
			LoggerBuilder = new StringBuilder();
			LoggerBuilder.Append($"prepare find enemy standart:{BotOwner_0.Id} ");
		}
		List<EnemyInfo> list = new List<EnemyInfo>();
		foreach (EnemyInfo value in BotOwner_0.EnemiesController.EnemyInfos.Values)
		{
			if (flag)
			{
				LoggerBuilder.AppendLine();
				LoggerBuilder.Append(" Enemy " + value.Person.Profile.Nickname + " ");
			}
			if (!value.Person.HealthController.IsAlive)
			{
				if (flag)
				{
					LoggerBuilder.Append("isDead");
				}
			}
			else if (!value.HaveSeen)
			{
				if (flag)
				{
					LoggerBuilder.Append("not HaveSeen");
				}
			}
			else if (!value.ShallKnowEnemy())
			{
				if (flag)
				{
					LoggerBuilder.Append("not ShallKnowEnemy");
				}
			}
			else
			{
				list.Add(value);
			}
		}
		if (flag)
		{
			int hashCode = LoggerBuilder.ToString().GetHashCode();
			OldHashPrepareLog = hashCode;
		}
		if (list.Count == 0)
		{
			return null;
		}
		return BetterEnemy(list, flag);
	}

	public virtual void Activate()
	{
	}

	public EnemyInfo BetterEnemy(List<EnemyInfo> enemiesInfos, bool isLogging)
	{
		if (enemiesInfos.Count == 0)
		{
			return null;
		}
		BotEnemiesController enemiesController = BotOwner_0.EnemiesController;
		float num = float.MaxValue;
		IPlayer player = null;
		if (isLogging)
		{
			LoggerBuilder = new StringBuilder();
			LoggerBuilder.Append($"find better enemy for bot:{BotOwner_0.Id}");
		}
		foreach (EnemyInfo enemiesInfo in enemiesInfos)
		{
			if (isLogging)
			{
				LoggerBuilder.AppendLine();
				LoggerBuilder.Append("enemy " + enemiesInfo.Person.Profile.Nickname + " == ");
			}
			if (enemiesInfo.IgnoreUntilAggression)
			{
				if (isLogging)
				{
					LoggerBuilder.Append("IgnoreUntilAggression");
				}
				continue;
			}
			float num2 = 0f;
			float distance = enemiesInfo.Distance;
			if (!(distance < BotOwner_0.Settings.FileSettings.Mind.MAX_AGGRO_BOT_DIST) && enemiesInfo.IsVisible && distance > BotOwner_0.Settings.FileSettings.Mind.MAX_AGGRO_BOT_DIST_UPPER_LIMIT)
			{
				if (isLogging)
				{
					LoggerBuilder.Append($"too close distance:{(int)distance}");
				}
				continue;
			}
			num2 = CalcWeight(distance, enemiesInfo);
			if (num2 < num)
			{
				num = num2;
				player = enemiesInfo.Person;
			}
			if (isLogging)
			{
				LoggerBuilder.Append($" weight:{(int)num2} {enemiesInfo.GetDebugInfo()}  addw:{enemiesInfo.AdditionalWeightCauseHit()}");
			}
		}
		if (player == null)
		{
			return null;
		}
		if (isLogging)
		{
			int hashCode = LoggerBuilder.ToString().GetHashCode();
			OldHashBetterLog = hashCode;
		}
		return enemiesController.EnemyInfos[player];
	}

	public virtual float CalcWeight(float dist, EnemyInfo enemyInfo)
	{
		float num = dist;
		if (!enemyInfo.IsVisible)
		{
			switch (enemyInfo.VisibleType)
			{
			case EEnemyPartVisibleType.NotVisible:
				num += 100000f;
				break;
			case EEnemyPartVisibleType.GreenSence:
				num += 1000f;
				break;
			case EEnemyPartVisibleType.Sence:
				num += 1000f;
				break;
			}
		}
		if (!enemyInfo.CanShoot)
		{
			num += 20000f;
		}
		return num + enemyInfo.AdditionalWeightCauseHit();
	}

	public virtual void Dispose()
	{
	}
}
