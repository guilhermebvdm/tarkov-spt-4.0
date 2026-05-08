using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class BotEnemiesController : GClass429
{
	[Serializable]
	[CompilerGenerated]
	public class Class238
	{
		public static readonly Class238 class238_0 = new Class238();

		public static Func<IPlayer, bool> func_0;

		public bool method_0(IPlayer x)
		{
			return x.AIData.ShallPursuit;
		}
	}

	public const int CLOSE = 6;

	public const int MID = 8;

	[NonSerialized]
	public EnemyInfo[] SortedInfos = new EnemyInfo[100];

	[NonSerialized]
	public int CountEnemies;

	[NonSerialized]
	public bool HavePersuitableEnemy;

	[NonSerialized]
	public bool CanPersueAxeman;

	public EnemyInfo BestObservedEnemy;

	public GClass474 MissController;

	[field: NonSerialized]
	public Dictionary<IPlayer, EnemyInfo> EnemyInfos { get; }

	public bool CanPursueAxeman
	{
		get
		{
			if (!CanPersueAxeman)
			{
				return BotOwner_0.PriorityAxeTarget.AllPursuit;
			}
			return true;
		}
	}

	public bool HavePursuitableEnemy
	{
		get
		{
			if (!HavePersuitableEnemy)
			{
				return BotOwner_0.PriorityAxeTarget.AllPursuit;
			}
			return true;
		}
		set
		{
			if (CanPursueAxeman)
			{
				HavePersuitableEnemy = value;
			}
		}
	}

	public static BotEnemiesController Create([NotNull] BotOwner owner)
	{
		switch (owner.Profile.Info.Settings.Role)
		{
		default:
			return new BotEnemiesController(owner);
		case WildSpawnType.bossBoar:
			return new GClass472(owner);
		case WildSpawnType.bossZryachiy:
		case WildSpawnType.followerZryachiy:
			return new GClass473(owner);
		}
	}

	public BotEnemiesController([NotNull] BotOwner owner)
		: base(owner)
	{
		EnemyInfos = new Dictionary<IPlayer, EnemyInfo>();
		MissController = new GClass474(this, owner);
	}

	public void Remove(IPlayer info)
	{
		EnemyInfo enemyInfo = EnemyInfos[info];
		enemyInfo.Dispose();
		EnemyInfos.Remove(info);
		CountEnemies--;
		bool flag = false;
		for (int i = 0; i < CountEnemies + 1; i++)
		{
			if (!flag && SortedInfos[i] == enemyInfo)
			{
				flag = true;
			}
			if (flag)
			{
				EnemyInfo enemyInfo2 = SortedInfos[i + 1];
				SortedInfos[i] = enemyInfo2;
				if (enemyInfo2 == null)
				{
					break;
				}
				enemyInfo2.PriorityIndex = i;
			}
		}
		bool havePursuitableEnemy = EnemyInfos.Keys.Any((IPlayer x) => x.AIData.ShallPursuit);
		HavePursuitableEnemy = havePursuitableEnemy;
	}

	public virtual EnemyInfo AddNew(BotsGroup botsGroup, IPlayer enemy, BotSettingsClass groupInfo)
	{
		return new EnemyInfo(botsGroup, enemy, BotOwner_0, groupInfo);
	}

	public void Activate(bool globalPossibility)
	{
		CanPersueAxeman = globalPossibility && BotOwner_0.Settings.FileSettings.Mind.WILL_PERSUE_AXEMAN;
		MissController.Activate();
	}

	public void SetInfo(IPlayer enemy, EnemyInfo info)
	{
		EnemyInfos[enemy] = info;
		if (info.Person.AIData.ShallPursuit || BotOwner_0.PriorityAxeTarget.AllPursuit)
		{
			HavePursuitableEnemy = true;
		}
		SortedInfos[CountEnemies] = info;
		info.PriorityIndex = CountEnemies;
		CountEnemies++;
	}

	public void UpdateFor(EnemyInfo info)
	{
		int priorityIndex = info.PriorityIndex;
		if (priorityIndex > 0)
		{
			EnemyInfo enemyInfo = SortedInfos[priorityIndex - 1];
			if (enemyInfo != null && enemyInfo.Distance > info.Distance)
			{
				method_0(priorityIndex, priorityIndex - 1);
			}
		}
	}

	public void UpdateFor(EnemyInfo info, bool mainEnemy)
	{
	}

	public void DrawGizmos()
	{
		Vector3 up = Vector3.up;
		for (int i = 0; i < CountEnemies; i++)
		{
			EnemyInfo enemyInfo = SortedInfos[i];
			int priorityIndex = enemyInfo.PriorityIndex;
			Color color = Color.red;
			if (priorityIndex > 6)
			{
				color = Color.yellow;
				if (priorityIndex > 8)
				{
					color = Color.green;
				}
			}
			Gizmos.color = color;
			Gizmos.DrawLine(up + BotOwner_0.Position, up + enemyInfo.CurrPosition);
		}
	}

	public void CheckEnemyPursuit(PlayerAIDataClass aiData)
	{
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in EnemyInfos)
		{
			if (enemyInfo.Key.AIData == aiData)
			{
				HavePursuitableEnemy = true;
				break;
			}
		}
	}

	public bool IsEnemy(IPlayer player)
	{
		return EnemyInfos.ContainsKey(player);
	}

	public void SetSameEnemy(EnemyInfo enemyInfo)
	{
		if (enemyInfo == null)
		{
			return;
		}
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo2 in EnemyInfos)
		{
			if (enemyInfo2.Value.Person.Id == enemyInfo.Person.Id)
			{
				BotOwner_0.Memory.GoalEnemy = enemyInfo2.Value;
				break;
			}
		}
	}

	public void HitTarget(Player target, DamageInfoStruct damageInfo, EBodyPart bodyPart)
	{
		if (EnemyInfos.TryGetValue(target, out var value))
		{
			value.LastDoHitTime = Time.time;
		}
	}

	public void method_0(int a, int b)
	{
		EnemyInfo enemyInfo = SortedInfos[a];
		SortedInfos[a] = SortedInfos[b];
		SortedInfos[a].PriorityIndex = a;
		SortedInfos[b] = enemyInfo;
		enemyInfo.PriorityIndex = b;
	}

	public void ShootDone()
	{
		if (BotOwner_0.Memory.HaveEnemy && BotOwner_0.Memory.GoalEnemy.FirstTimeShoot < 0f)
		{
			BotOwner_0.Memory.GoalEnemy.FirstTimeShoot = Time.time;
		}
	}

	public void Dispose()
	{
		MissController.Dispose();
	}
}
