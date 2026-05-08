using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace EFT;

public class WavesSpawnScenario : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	public class Class1155
	{
		public static readonly Class1155 class1155_0 = new Class1155();

		public static Func<WildSpawnWave, int> func_0;

		public static Func<BotWaveDataClass, float> func_1;

		public int method_0(WildSpawnWave x)
		{
			return x.slots_max;
		}

		public float method_1(BotWaveDataClass wave)
		{
			return wave.Time;
		}
	}

	[CompilerGenerated]
	public class Class1156
	{
		public WildSpawnType wildSpawnType;

		public bool method_0(MinMaxBots x)
		{
			return x.WildSpawnType == wildSpawnType;
		}
	}

	[CompilerGenerated]
	public class Class1157
	{
		public WildSpawnType type;

		public int method_0(WildSpawnWave x)
		{
			if (x.WildSpawnType != type)
			{
				return 0;
			}
			return x.slots_min;
		}
	}

	[CompilerGenerated]
	public class Class1158
	{
		public BotWaveDataClass spawnWave;

		public GClass641.IBotTimer timer;

		public WavesSpawnScenario wavesSpawnScenario_0;

		public void method_0()
		{
			wavesSpawnScenario_0.list_0.Remove(timer);
			wavesSpawnScenario_0.func_0(spawnWave).HandleExceptions();
		}
	}

	public BotLocationModifier BotLocationModifier;

	private readonly List<GClass641.IBotTimer> list_0 = new List<GClass641.IBotTimer>();

	private Func<BotWaveDataClass, Task> func_0;

	private Dictionary<WildSpawnType, int> dictionary_0 = new Dictionary<WildSpawnType, int>();

	public readonly List<WaveInfoClass> BotsCountProfiles = new List<WaveInfoClass>();

	[CompilerGenerated]
	private BotWaveDataClass[] gclass1880_0;

	[CompilerGenerated]
	private bool bool_0;

	public BotWaveDataClass[] SpawnWaves
	{
		[CompilerGenerated]
		get
		{
			return gclass1880_0;
		}
		[CompilerGenerated]
		set
		{
			gclass1880_0 = value;
		}
	}

	public bool Enabled
	{
		[CompilerGenerated]
		get
		{
			return bool_0;
		}
		[CompilerGenerated]
		set
		{
			bool_0 = value;
		}
	}

	public static WavesSpawnScenario smethod_0(GameObject game, WildSpawnWave[] waves, Func<BotWaveDataClass, Task> spawnAction, LocationSettingsClass.Location location = null)
	{
		MinMaxBots[] minMaxBots = ((location != null) ? location.MinMaxBots : new MinMaxBots[0]);
		BotLocationModifier botLocationModifier = ((location != null) ? location.BotLocationModifier : new BotLocationModifier());
		WavesSpawnScenario wavesSpawnScenario = game.gameObject.AddComponent<WavesSpawnScenario>();
		smethod_1(wavesSpawnScenario.dictionary_0, minMaxBots);
		bool flag = location?.OldSpawn ?? false;
		if (DebugBotData.UseDebugData && DebugBotData.Instance.waves.Count > 0)
		{
			flag = true;
		}
		if (flag)
		{
			wavesSpawnScenario.Init(waves);
		}
		wavesSpawnScenario.func_0 = spawnAction;
		wavesSpawnScenario.BotLocationModifier = botLocationModifier;
		if (waves.Sum((WildSpawnWave x) => x.slots_max) != 0)
		{
		}
		return wavesSpawnScenario;
	}

	public static void smethod_1(Dictionary<WildSpawnType, int> minCounts, MinMaxBots[] minMaxBots)
	{
		WildSpawnType[] allTypes = BotsController.AllTypes;
		foreach (WildSpawnType wildSpawnType in allTypes)
		{
			if (minMaxBots == null)
			{
				minCounts.Add(wildSpawnType, 0);
				continue;
			}
			MinMaxBots minMaxBots2 = minMaxBots.FirstOrDefault((MinMaxBots x) => x.WildSpawnType == wildSpawnType);
			if (minMaxBots2 == null)
			{
				minCounts.Add(wildSpawnType, 0);
			}
			else
			{
				minCounts.Add(wildSpawnType, minMaxBots2.min);
			}
		}
	}

	public void Init(WildSpawnWave[] waves)
	{
		Enabled = true;
		foreach (WildSpawnWave wildSpawnWave in waves)
		{
			if (wildSpawnWave.WildSpawnType == WildSpawnType.marksman || wildSpawnWave.WildSpawnType != WildSpawnType.assault)
			{
			}
		}
		method_0(waves, WildSpawnType.marksman);
		method_0(waves, WildSpawnType.assault);
		SpawnWaves = ((waves == null) ? new BotWaveDataClass[0] : (from wave in waves.Select(delegate(WildSpawnWave wave)
			{
				int botsCount = GClass856.RandomInclude(wave.slots_min, wave.slots_max);
				BotWaveDataClass botWaveDataClass = new BotWaveDataClass
				{
					Time = UnityEngine.Random.Range(wave.time_min, wave.time_max),
					BotsCount = botsCount,
					Difficulty = wave.GetDifficulty(),
					WildSpawnType = wave.WildSpawnType,
					SpawnAreaName = wave.SpawnPoints,
					Side = wave.BotSide,
					IsPlayers = wave.isPlayers,
					ChanceGroup = wave.ChanceGroup,
					KeepZoneOnSpawn = wave.KeepZoneOnSpawn
				};
				WaveInfoClass item = new WaveInfoClass(botWaveDataClass.BotsCount, botWaveDataClass.WildSpawnType, botWaveDataClass.Difficulty);
				BotsCountProfiles.Add(item);
				return botWaveDataClass;
			})
			orderby wave.Time
			select wave).ToArray());
		BotWaveDataClass[] spawnWaves = SpawnWaves;
		for (int i = 0; i < spawnWaves.Length; i++)
		{
		}
	}

	public void method_0(WildSpawnWave[] waves, WildSpawnType type)
	{
		if (DebugBotData.UseDebugData && DebugBotData.Instance.NoMinMax)
		{
			return;
		}
		int num = waves.Sum((WildSpawnWave x) => (x.WildSpawnType == type) ? x.slots_min : 0);
		int num2 = dictionary_0[type];
		if (num >= num2)
		{
			return;
		}
		int num3 = num2 - num;
		if (waves.Length <= num3)
		{
			foreach (WildSpawnWave item in GClass856.RandomElement(waves, num3))
			{
				item.slots_min++;
				if (item.slots_max < item.slots_min)
				{
					item.slots_max = item.slots_min;
				}
			}
			return;
		}
		int num4 = num3 / waves.Length;
		foreach (WildSpawnWave wildSpawnWave in waves)
		{
			wildSpawnWave.slots_min += num4;
			if (wildSpawnWave.slots_max < wildSpawnWave.slots_min)
			{
				wildSpawnWave.slots_max = wildSpawnWave.slots_min;
			}
		}
	}

	public async Task Run(EBotsSpawnMode spawnMode = EBotsSpawnMode.Anyway)
	{
		if (!Enabled)
		{
			return;
		}
		List<Task> list = new List<Task>();
		BotWaveDataClass[] spawnWaves = SpawnWaves;
		foreach (BotWaveDataClass spawnWave in spawnWaves)
		{
			switch (spawnMode)
			{
			case EBotsSpawnMode.AfterGameStarted:
				if (spawnWave.Time < 0f)
				{
					continue;
				}
				break;
			case EBotsSpawnMode.BeforeGameStarted:
				if (spawnWave.Time < 0f)
				{
					list.Add(func_0(spawnWave));
				}
				continue;
			}
			GClass641.IBotTimer timer = StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(spawnWave.Time));
			list_0.Add(timer);
			timer.OnTimer += delegate
			{
				list_0.Remove(timer);
				func_0(spawnWave).HandleExceptions();
			};
		}
		if (list.Count > 0)
		{
			await Task.WhenAll(list.ToArray());
		}
	}

	public void Stop()
	{
		foreach (GClass641.IBotTimer item in list_0)
		{
			item.Stop();
		}
	}

	[CompilerGenerated]
	public BotWaveDataClass method_1(WildSpawnWave wave)
	{
		int botsCount = GClass856.RandomInclude(wave.slots_min, wave.slots_max);
		BotWaveDataClass botWaveDataClass = new BotWaveDataClass
		{
			Time = UnityEngine.Random.Range(wave.time_min, wave.time_max),
			BotsCount = botsCount,
			Difficulty = wave.GetDifficulty(),
			WildSpawnType = wave.WildSpawnType,
			SpawnAreaName = wave.SpawnPoints,
			Side = wave.BotSide,
			IsPlayers = wave.isPlayers,
			ChanceGroup = wave.ChanceGroup,
			KeepZoneOnSpawn = wave.KeepZoneOnSpawn
		};
		WaveInfoClass item = new WaveInfoClass(botWaveDataClass.BotsCount, botWaveDataClass.WildSpawnType, botWaveDataClass.Difficulty);
		BotsCountProfiles.Add(item);
		return botWaveDataClass;
	}
}
