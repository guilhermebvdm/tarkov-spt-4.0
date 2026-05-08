using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class BossSpawnScenario
{
	[Serializable]
	[CompilerGenerated]
	public class Class344
	{
		public static readonly Class344 class344_0 = new Class344();

		public static Func<BossLocationSpawn, bool> func_0;

		public bool method_0(BossLocationSpawn wave)
		{
			if (BotSettingsRepoClass.IsPmcBot(wave.BossType) && wave.Time < 0f)
			{
				return GClass856.IsNullOrEmpty(wave.BornZone);
			}
			return false;
		}
	}

	[CompilerGenerated]
	public class Class345
	{
		public BossLocationSpawn wave;

		public BossSpawnScenario bossSpawnScenario_0;

		public GClass641.IBotTimer timer;

		public void method_0()
		{
			if (!wave.Activated)
			{
				bossSpawnScenario_0.Timers.Remove(timer);
				bossSpawnScenario_0.method_5(wave);
			}
		}
	}

	[CompilerGenerated]
	public class Class346
	{
		public BossSpawnScenario bossSpawnScenario_0;

		public BossLocationSpawn wave;

		public GClass641.IBotTimer timer;

		public Action<string> onEvent;

		public BotEventHandler.GDelegate30 delActions;

		public void method_0(string eventId)
		{
			if (bossSpawnScenario_0.method_4(wave, eventId, timer))
			{
				Singleton<BotEventHandler>.Instance.OnEvent -= onEvent;
			}
		}

		public void method_1(string eventId, Vector3 position)
		{
			if (bossSpawnScenario_0.method_4(wave, eventId, timer))
			{
				Singleton<BotEventHandler>.Instance.OnInteractObject -= delActions;
			}
		}
	}

	[CompilerGenerated]
	public class Class347
	{
		public BossSpawnScenario bossSpawnScenario_0;

		public BossLocationSpawn wave;

		public void method_0()
		{
			bossSpawnScenario_0.SpawnBossAction(wave);
		}
	}

	[NonSerialized]
	public List<GClass641.IBotTimer> Timers = new List<GClass641.IBotTimer>();

	[NonSerialized]
	public Action<BossLocationSpawn> SpawnBossAction;

	public readonly List<WaveInfoClass> BotsCountProfiles = new List<WaveInfoClass>();

	[NonSerialized]
	public GClass675 QuestsSpanws;

	[NonSerialized]
	public bool IsSubscribed;

	[field: NonSerialized]
	public BossLocationSpawn[] BossSpawnWaves { get; set; }

	[field: NonSerialized]
	public bool HaveSectants { get; set; }

	public static BossSpawnScenario smethod_0(BossLocationSpawn[] bossWaves, Action<BossLocationSpawn> spawnBossAction)
	{
		BossSpawnScenario bossSpawnScenario = new BossSpawnScenario();
		bossSpawnScenario.method_0(bossWaves);
		bossSpawnScenario.SpawnBossAction = spawnBossAction;
		return bossSpawnScenario;
	}

	public void method_0(BossLocationSpawn[] bossWaves)
	{
		if (bossWaves != null)
		{
			BossLocationSpawn[] array = bossWaves;
			foreach (BossLocationSpawn bossLocationSpawn in array)
			{
				bossLocationSpawn.Init();
				if (bossLocationSpawn.ShallSpawn && BotSettingsRepoClass.IsSectant(bossLocationSpawn.BossType))
				{
					HaveSectants = true;
				}
				List<WaveInfoClass> usingTypes = bossLocationSpawn.GetUsingTypes();
				BotsCountProfiles.AddRange(usingTypes);
			}
		}
		BossSpawnWaves = bossWaves;
		if (BossSpawnWaves != null)
		{
			BossLocationSpawn[] array = BossSpawnWaves;
			for (int i = 0; i < array.Length; i++)
			{
			}
		}
		QuestsSpanws = new GClass675(method_5);
	}

	public void Run(List<BotZone> pmcZones, EBotsSpawnMode spawnMode = EBotsSpawnMode.Anyway)
	{
		QuestsSpanws.Run();
		if (BossSpawnWaves == null)
		{
			return;
		}
		method_1(pmcZones);
		BossLocationSpawn[] bossSpawnWaves;
		if (!IsSubscribed)
		{
			bossSpawnWaves = BossSpawnWaves;
			foreach (BossLocationSpawn bossLocationSpawn in bossSpawnWaves)
			{
				if (bossLocationSpawn.TriggerType != SpawnTriggerType.none)
				{
					method_3(bossLocationSpawn, null);
				}
			}
			IsSubscribed = true;
		}
		if (spawnMode == EBotsSpawnMode.AfterGameStarted)
		{
			bossSpawnWaves = BossSpawnWaves;
			for (int i = 0; i < bossSpawnWaves.Length; i++)
			{
				bossSpawnWaves[i].ShouldSpawnByKarma();
			}
		}
		bossSpawnWaves = BossSpawnWaves;
		foreach (BossLocationSpawn bossLocationSpawn2 in bossSpawnWaves)
		{
			switch (spawnMode)
			{
			case EBotsSpawnMode.Anyway:
				if (bossLocationSpawn2.Time >= 0f)
				{
					method_2(bossLocationSpawn2);
				}
				else if (bossLocationSpawn2.TriggerType == SpawnTriggerType.none)
				{
					method_5(bossLocationSpawn2);
				}
				break;
			case EBotsSpawnMode.AfterGameStarted:
				method_2(bossLocationSpawn2);
				break;
			default:
				if (bossLocationSpawn2.IsStartWave())
				{
					method_5(bossLocationSpawn2);
				}
				break;
			}
		}
	}

	public void method_1(List<BotZone> pmcZones)
	{
		List<string> list = new List<string>();
		int i;
		foreach (BotZone pmcZone in pmcZones)
		{
			for (i = 0; i < pmcZone.SpawnPoints.Length; i++)
			{
				if (GClass3582.ContainBotPmcCategory(pmcZone.SpawnPoints[i].Categories))
				{
					list.Add(pmcZone.name);
				}
			}
		}
		GClass856.Shuffle(list);
		i = 0;
		foreach (BossLocationSpawn item in BossSpawnWaves.Where((BossLocationSpawn wave) => BotSettingsRepoClass.IsPmcBot(wave.BossType) && wave.Time < 0f && GClass856.IsNullOrEmpty(wave.BornZone)))
		{
			if (i < list.Count)
			{
				item.BossZone = list[i];
				i++;
				continue;
			}
			Debug.LogError("Not enough bosszones for all pmc waves. Add more spawn zones with pmc points. Some waves would have empty zone field.");
			break;
		}
	}

	public void method_2(BossLocationSpawn wave)
	{
		if (!(wave.Time > 0f))
		{
			return;
		}
		GClass641.IBotTimer timer = null;
		if (!wave.ShallSpawn)
		{
			return;
		}
		timer = StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(wave.Time));
		Timers.Add(timer);
		timer.OnTimer += delegate
		{
			if (!wave.Activated)
			{
				Timers.Remove(timer);
				method_5(wave);
			}
		};
	}

	public void method_3(BossLocationSpawn wave, [CanBeNull] GClass641.IBotTimer timer)
	{
		switch (wave.TriggerType)
		{
		case SpawnTriggerType.interactObject:
		{
			BotEventHandler.GDelegate30 delActions = null;
			delActions = delegate(string eventId, Vector3 position)
			{
				if (method_4(wave, eventId, timer))
				{
					Singleton<BotEventHandler>.Instance.OnInteractObject -= delActions;
				}
			};
			Singleton<BotEventHandler>.Instance.OnInteractObject += delActions;
			break;
		}
		case SpawnTriggerType.byQuest:
			QuestsSpanws.AddWave(wave);
			break;
		case SpawnTriggerType.botEvent:
		{
			Action<string> onEvent = null;
			onEvent = delegate(string eventId)
			{
				if (method_4(wave, eventId, timer))
				{
					Singleton<BotEventHandler>.Instance.OnEvent -= onEvent;
				}
			};
			Singleton<BotEventHandler>.Instance.OnEvent += onEvent;
			break;
		}
		}
	}

	public bool method_4(BossLocationSpawn wave, string trgId, [CanBeNull] GClass641.IBotTimer timer)
	{
		if ((string.IsNullOrEmpty(wave.TriggerId) || wave.TriggerId == trgId) && !wave.Activated)
		{
			if (timer != null)
			{
				Timers.Remove(timer);
			}
			method_5(wave);
			return true;
		}
		return false;
	}

	public void method_5(BossLocationSpawn wave)
	{
		if (!wave.ShallSpawn)
		{
			return;
		}
		wave.Activated = true;
		if (wave.Delay > 1f)
		{
			StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(wave.Delay)).OnTimer += delegate
			{
				SpawnBossAction(wave);
			};
		}
		else
		{
			SpawnBossAction(wave);
		}
	}

	public void Stop()
	{
		foreach (GClass641.IBotTimer timer in Timers)
		{
			timer.Stop();
		}
	}
}
