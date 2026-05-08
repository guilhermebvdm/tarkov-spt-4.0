using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using EFT.GlobalEvents;
using UnityEngine;

public class GClass672
{
	public class Class337
	{
		public int spawnsRemains;

		public float nextSpawnsTime;
	}

	[Serializable]
	[CompilerGenerated]
	public class Class338
	{
		public static readonly Class338 class338_0 = new Class338();

		public static Func<LocationSettingsClass.Location.GClass1427, int> func_0;

		public int method_0(LocationSettingsClass.Location.GClass1427 x)
		{
			return x.Weight;
		}
	}

	[CompilerGenerated]
	public class Class339
	{
		public LocationSettingsClass.Location.GClass1427 spawnParams;

		public GClass672 gclass672_0;

		public void method_0()
		{
			BossLocationSpawn bossLocationSpawn = new BossLocationSpawn();
			bossLocationSpawn.BossChance = 100f;
			bossLocationSpawn.BossDifficult = spawnParams.Difficulty.ToString();
			bossLocationSpawn.BossName = spawnParams.Role.ToString();
			bossLocationSpawn.ForceSpawn = true;
			bossLocationSpawn.BossEscortAmount = "0";
			bossLocationSpawn.BossEscortDifficult = spawnParams.Difficulty.ToString();
			bossLocationSpawn.BossEscortType = spawnParams.Role.ToString();
			bossLocationSpawn.ParseMainTypesTypes();
			BotSpawnParams botSpawnParams = new BotSpawnParams();
			gclass672_0.BotSpawner_0.BossSpawner.Spawn(bossLocationSpawn, botSpawnParams).HandleExceptions();
		}
	}

	[NonSerialized]
	public Dictionary<string, Class337> Dictionary_0 = new Dictionary<string, Class337>();

	[NonSerialized]
	public List<GClass674> List_0 = new List<GClass674>();

	[NonSerialized]
	public BotSpawner BotSpawner_0;

	[NonSerialized]
	public GClass25 Gclass25_0;

	[NonSerialized]
	public LocationSettingsClass.Location.GClass1425 Gclass1425_0;

	public GClass672(List<GClass674> activePursuits, BotSpawner spawner, LocationSettingsClass.Location.GClass1425 halloweenConfig)
	{
		Gclass25_0 = new GClass25(4f, method_1);
		BotSpawner_0 = spawner;
		List_0 = activePursuits;
		Gclass1425_0 = halloweenConfig;
	}

	public void ManualUpdate()
	{
		Gclass25_0.Update();
	}

	public void method_0(Class337 pcsInfo)
	{
		int num = 0;
		while (pcsInfo.spawnsRemains > 0)
		{
			LocationSettingsClass.Location.GClass1427 spawnParams = method_5();
			Action task = delegate
			{
				BossLocationSpawn bossLocationSpawn = new BossLocationSpawn();
				bossLocationSpawn.BossChance = 100f;
				bossLocationSpawn.BossDifficult = spawnParams.Difficulty.ToString();
				bossLocationSpawn.BossName = spawnParams.Role.ToString();
				bossLocationSpawn.ForceSpawn = true;
				bossLocationSpawn.BossEscortAmount = "0";
				bossLocationSpawn.BossEscortDifficult = spawnParams.Difficulty.ToString();
				bossLocationSpawn.BossEscortType = spawnParams.Role.ToString();
				bossLocationSpawn.ParseMainTypesTypes();
				BotSpawnParams spawnParams2 = new BotSpawnParams();
				BotSpawner_0.BossSpawner.Spawn(bossLocationSpawn, spawnParams2).HandleExceptions();
			};
			int num2 = num;
			BotSpawner_0.BotGame.BotsController.AiTaskManager.RegisterDelayedTask(num2, task);
			pcsInfo.spawnsRemains--;
			num++;
		}
	}

	public void method_1()
	{
		int num = 0;
		float num2 = Gclass1425_0.CrowdAttackBlockRadius * Gclass1425_0.CrowdAttackBlockRadius;
		for (int i = 0; i < List_0.Count; i++)
		{
			GClass674 gClass = List_0[i];
			if (!gClass.TargetAllowToSpawnCrowd)
			{
				continue;
			}
			if (gClass.Disposed)
			{
				List_0.RemoveAt(i);
				i--;
			}
			if (!gClass.ReadyToSpawn() || !gClass.TargetAllowToSpawnCrowd)
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < i; j++)
			{
				GClass674 gClass2 = List_0[j];
				if (gClass2.TargetAllowToSpawnCrowd && !(GClass856.SqrDistance(gClass.CurPosition, gClass2.CurPosition) >= num2))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				num++;
				Class337 pcsInfo = method_2(gClass.Target);
				method_0(pcsInfo);
				if (num >= Gclass1425_0.CrowdsLimit)
				{
					break;
				}
			}
		}
	}

	public Class337 method_2(IPlayer player)
	{
		Class337 @class;
		if (!Dictionary_0.ContainsKey(player.ProfileId))
		{
			@class = new Class337();
			@class.spawnsRemains = method_4();
			@class.nextSpawnsTime = 0f;
			Dictionary_0.Add(player.ProfileId, @class);
		}
		else
		{
			@class = Dictionary_0[player.ProfileId];
		}
		if (Time.time > @class.nextSpawnsTime)
		{
			method_3(@class);
			method_7(player);
		}
		return @class;
	}

	public void method_3(Class337 pcsInfo)
	{
		pcsInfo.spawnsRemains = method_4();
		pcsInfo.nextSpawnsTime = Time.time + Gclass1425_0.CrowdCooldownPerPlayerSec;
	}

	public int method_4()
	{
		if (Gclass1425_0.InfectionPercentage <= 0)
		{
			return 0;
		}
		if (Gclass1425_0.InfectionPercentage >= 100)
		{
			return Gclass1425_0.MaxCrowdAttackSpawnLimit;
		}
		return (int)(Gclass1425_0.CalcRealInfectionLevel() * (float)Gclass1425_0.MaxCrowdAttackSpawnLimit);
	}

	public LocationSettingsClass.Location.GClass1427 method_5()
	{
		int b = Gclass1425_0.CrowdAttackSpawnParams.Sum((LocationSettingsClass.Location.GClass1427 x) => x.Weight);
		int num = GClass856.RandomInclude(0, b);
		int num2 = 0;
		while (true)
		{
			if (num2 < Gclass1425_0.CrowdAttackSpawnParams.Count)
			{
				int weight = Gclass1425_0.CrowdAttackSpawnParams[num2].Weight;
				if (num < weight)
				{
					break;
				}
				num -= weight;
				num2++;
				continue;
			}
			return Gclass1425_0.CrowdAttackSpawnParams[Gclass1425_0.CrowdAttackSpawnParams.Count - 1];
		}
		return Gclass1425_0.CrowdAttackSpawnParams[num2];
	}

	public void method_6()
	{
	}

	public void method_7(IPlayer player)
	{
		GlobalEventHandlerClass.CreateEvent<BotsCrowdSpawnEvent>().Invoke(player.ProfileId);
	}
}
