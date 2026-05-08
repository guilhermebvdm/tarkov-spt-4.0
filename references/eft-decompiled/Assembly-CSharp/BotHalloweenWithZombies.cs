using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using UnityEngine;

public class BotHalloweenWithZombies
{
	[Serializable]
	[CompilerGenerated]
	public class Class342
	{
		public static readonly Class342 class342_0 = new Class342();

		public static Func<BossLocationSpawn, bool> func_0;

		public bool method_0(BossLocationSpawn x)
		{
			return x.BossType == WildSpawnType.bossPartisan;
		}
	}

	[CompilerGenerated]
	public class Class343
	{
		public BotHalloweenWithZombies botHalloweenWithZombies_0;

		public BotOwner bot;

		public void method_0(DamageInfoStruct di, EBodyPart bp, float t)
		{
			botHalloweenWithZombies_0.method_6(bot, di, bp, t);
		}

		public void method_1(IPlayer trg)
		{
			botHalloweenWithZombies_0.method_7(bot, trg);
		}
	}

	[NonSerialized]
	public const string MINIMAL_INFECTED_SPAWN_TRIGGER = "MinimalInfectedBotOnStart";

	[NonSerialized]
	public BotsClass List;

	[NonSerialized]
	public List<GClass674> ActivePursuits = new List<GClass674>();

	[NonSerialized]
	public LocationSettingsClass.Location.GClass1425 HalloweenConfig;

	[NonSerialized]
	public GClass672 CrowdSpawn;

	[NonSerialized]
	public GClass673 BotHalloweenDistributions;

	[NonSerialized]
	public bool IsInited;

	[NonSerialized]
	public BotSpawner Spawner;

	[NonSerialized]
	public bool MidSessionSpawned;

	public float TargetPointSearchRadiusLimit
	{
		get
		{
			if (HalloweenConfig == null)
			{
				return 0f;
			}
			return HalloweenConfig.TargetPointSearchRadiusLimit;
		}
	}

	public float MinSpawnDistToPlayer
	{
		get
		{
			if (HalloweenConfig == null)
			{
				return 0f;
			}
			return HalloweenConfig.MinSpawnDistToPlayer;
		}
	}

	public int InfectionPercentage
	{
		get
		{
			if (HalloweenConfig == null)
			{
				return 0;
			}
			return HalloweenConfig.InfectionPercentage;
		}
	}

	public float ZombieLookCoeff
	{
		get
		{
			if (HalloweenConfig == null)
			{
				return 1f;
			}
			return HalloweenConfig.InfectedLookCoeff;
		}
	}

	public BotHalloweenWithZombies(BotsEventsController botsEventsController, BotsClass botsList, BotSpawner spawner, LocationSettingsClass.Location.GClass1425 halloweenConfig)
	{
		BotHalloweenDistributions = new GClass673(botsList);
		List = botsList;
		Spawner = spawner;
		HalloweenConfig = halloweenConfig;
		if (HalloweenConfig != null)
		{
			CrowdSpawn = new GClass672(ActivePursuits, spawner, halloweenConfig);
			IsInited = true;
		}
	}

	public bool ShallUseInfectedLookCoeff(BotOwner owner)
	{
		if (BotSettingsRepoClass.IsInfected(owner.Profile.Info.Settings.Role))
		{
			return owner.BotsController.EventsController.BotHalloweenWithZombies != null;
		}
		return false;
	}

	public void Activate()
	{
		if (!IsInited)
		{
			return;
		}
		foreach (BotOwner botOwner in List.BotOwners)
		{
			method_2(botOwner);
		}
		method_1();
		List.OnBotAdd += method_3;
		if (InfectionPercentage > 0)
		{
			method_0();
		}
	}

	public void SpawnAction()
	{
		if (InfectionPercentage > 0)
		{
			Singleton<BotEventHandler>.Instance.AnyEvent("MinimalInfectedBotOnStart");
		}
		for (int i = 0; i <= 100; i += 10)
		{
			if (InfectionPercentage <= i)
			{
				Singleton<BotEventHandler>.Instance.AnyEvent($"UninfectedSpawn{i}");
			}
			if (InfectionPercentage > i - 10)
			{
				Singleton<BotEventHandler>.Instance.AnyEvent($"InfectedSpawn{i}");
			}
		}
	}

	public void method_0()
	{
		foreach (BossLocationSpawn item in Singleton<IBotGame>.Instance.BossSpawnScenario.BossSpawnWaves.Where((BossLocationSpawn x) => x.BossType == WildSpawnType.bossPartisan).ToList())
		{
			item.BossChance = -1f;
			item.CalculateChance();
		}
	}

	public void method_1()
	{
		List<WildSpawnType> list = new List<WildSpawnType>
		{
			WildSpawnType.infectedAssault,
			WildSpawnType.infectedCivil,
			WildSpawnType.infectedPmc,
			WildSpawnType.infectedLaborant
		};
		List<BotDifficulty> obj = new List<BotDifficulty>
		{
			BotDifficulty.easy,
			BotDifficulty.normal,
			BotDifficulty.hard
		};
		int count = 10;
		foreach (BotDifficulty item in obj)
		{
			foreach (WildSpawnType item2 in list)
			{
				Spawner.AddToTargetBackup(item, item2, count);
			}
		}
	}

	public Vector3 GetNearestPursuitCenter(BotOwner owner)
	{
		int num = 0;
		GClass674 gClass;
		while (true)
		{
			if (num < ActivePursuits.Count)
			{
				gClass = ActivePursuits[num];
				if (gClass.TargetAllowToSpawnCrowd && gClass.Contains(owner))
				{
					break;
				}
				num++;
				continue;
			}
			float num2 = float.MaxValue;
			GClass674 gClass2 = null;
			for (int i = 0; i < ActivePursuits.Count; i++)
			{
				GClass674 gClass3 = ActivePursuits[i];
				if (gClass3.TargetAllowToSpawnCrowd)
				{
					float num3 = GClass856.SqrDistance(gClass3.CurPosition, owner.Position);
					if (num3 < num2)
					{
						gClass2 = gClass3;
						num2 = num3;
					}
				}
			}
			return gClass2?.CurPosition ?? owner.Position;
		}
		return gClass.CurPosition;
	}

	public void method_2(BotOwner bot)
	{
		WildSpawnType role = bot.Profile.Info.Settings.Role;
		if ((uint)(role - 60) <= 4u)
		{
			bot.GetPlayer.BeingHitAction += delegate(DamageInfoStruct di, EBodyPart bp, float t)
			{
				method_6(bot, di, bp, t);
			};
			bot.GameEventsData.HalloweenGameEvent.OnReportSeenEnemy += delegate(IPlayer trg)
			{
				method_7(bot, trg);
			};
		}
	}

	public void method_3(BotOwner bot)
	{
		method_2(bot);
		method_4(bot);
	}

	public void method_4(BotOwner bot)
	{
		GClass674 gClass = null;
		float num = float.MaxValue;
		Vector3 position = bot.Position;
		foreach (GClass674 activePursuit in ActivePursuits)
		{
			if (activePursuit.TargetAllowToSpawnCrowd)
			{
				float num2 = GClass856.SqrDistance(position, activePursuit.CallerPosition);
				if (gClass == null || num2 < num)
				{
					num = num2;
					gClass = activePursuit;
				}
			}
		}
		if (gClass == null)
		{
			foreach (GClass674 activePursuit2 in ActivePursuits)
			{
				float num3 = GClass856.SqrDistance(position, activePursuit2.CallerPosition);
				if (gClass == null || num3 < num)
				{
					num = num3;
					gClass = activePursuit2;
				}
			}
		}
		gClass?.AddZombie(bot);
	}

	public void ManualUpdate()
	{
		BotHalloweenDistributions.ManualUpdate();
		if (!IsInited || ActivePursuits.Count == 0)
		{
			return;
		}
		for (int i = 0; i < ActivePursuits.Count; i++)
		{
			GClass674 gClass = ActivePursuits[i];
			if (gClass.Disposed)
			{
				ActivePursuits.RemoveAt(i);
				i--;
			}
			else
			{
				gClass.ManualUpdate();
			}
		}
		CrowdSpawn.ManualUpdate();
	}

	public void method_5()
	{
		if (!MidSessionSpawned)
		{
			GameTimerClass gameTimer = Singleton<AbstractGame>.Instance.GameTimer;
			TimeSpan timeSpan = gameTimer.SessionTime.Value.Multiply(0.5);
			if (gameTimer.PastTime >= timeSpan)
			{
				SpawnAction();
				MidSessionSpawned = true;
			}
		}
	}

	public void Dispose()
	{
		if (IsInited)
		{
			List.OnBotAdd -= method_3;
		}
	}

	public void method_6(BotOwner zombieCaller, DamageInfoStruct damageInfo, EBodyPart bodyPart, float arg3)
	{
		if (damageInfo.Player != null)
		{
			IPlayer iPlayer = damageInfo.Player.iPlayer;
			method_8(zombieCaller, iPlayer);
		}
	}

	public void method_7(BotOwner zombieCaller, IPlayer target)
	{
		method_8(zombieCaller, target);
	}

	public void method_8(BotOwner zombieCaller, IPlayer target)
	{
		for (int i = 0; i < ActivePursuits.Count; i++)
		{
			GClass674 gClass = ActivePursuits[i];
			if (target == gClass.Target)
			{
				return;
			}
		}
		if (target.IsAI && BotSettingsRepoClass.IsInfected(target.AIData.BotOwner.Profile.Info.Settings.Role))
		{
			return;
		}
		GClass674 gClass2 = new GClass674(target, zombieCaller, method_9, HalloweenConfig);
		ActivePursuits.Add(gClass2);
		foreach (BotOwner botOwner in List.BotOwners)
		{
			if (BotSettingsRepoClass.IsInfected(botOwner.Profile.Info.Settings.Role))
			{
				gClass2.AddZombie(botOwner);
			}
		}
	}

	public void method_9(GClass674 obj)
	{
		ActivePursuits.Remove(obj);
	}

	public static IEnumerable<WaveInfoClass> GetProfilesOnStart()
	{
		return new List<WaveInfoClass>
		{
			new WaveInfoClass(26, WildSpawnType.infectedAssault, BotDifficulty.easy),
			new WaveInfoClass(100, WildSpawnType.infectedAssault, BotDifficulty.normal),
			new WaveInfoClass(51, WildSpawnType.infectedAssault, BotDifficulty.hard),
			new WaveInfoClass(13, WildSpawnType.infectedCivil, BotDifficulty.easy),
			new WaveInfoClass(50, WildSpawnType.infectedCivil, BotDifficulty.normal),
			new WaveInfoClass(20, WildSpawnType.infectedLaborant, BotDifficulty.easy),
			new WaveInfoClass(64, WildSpawnType.infectedLaborant, BotDifficulty.normal),
			new WaveInfoClass(20, WildSpawnType.infectedPmc, BotDifficulty.easy),
			new WaveInfoClass(77, WildSpawnType.infectedPmc, BotDifficulty.normal),
			new WaveInfoClass(77, WildSpawnType.infectedPmc, BotDifficulty.hard)
		};
	}
}
