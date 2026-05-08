using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class GClass678
{
	[NonSerialized]
	public Dictionary<BotZone, GClass1888> Dictionary_0 = new Dictionary<BotZone, GClass1888>();

	[NonSerialized]
	public BotSpawner BotSpawner_0;

	[NonSerialized]
	public Dictionary<PatrolPoint, BotZone> Dictionary_1 = new Dictionary<PatrolPoint, BotZone>();

	[NonSerialized]
	public bool Bool_0;

	public void Init(BotSpawner botSpawner, Dictionary<PatrolPoint, BotZone> zonePatrols)
	{
		Bool_0 = true;
		BotSpawner_0 = botSpawner;
		BotSpawner_0.OnBotCreated += method_3;
		BotSpawner_0.OnBotRemoved += method_4;
		BotSpawner_0.OnSpawnedWave += method_0;
		Dictionary_1 = zonePatrols;
	}

	public void method_0(GClass1888 obj)
	{
		if (Dictionary_0.ContainsKey(obj.BotZone))
		{
			Dictionary_0[obj.BotZone] = obj;
		}
		else
		{
			Dictionary_0.Add(obj.BotZone, obj);
		}
	}

	public BotZone method_1()
	{
		float num = float.MaxValue;
		PatrolPoint key = null;
		for (int i = 0; i < BotSpawner_0.PlayersCount; i++)
		{
			Player player = BotSpawner_0.GetPlayer(i);
			float dist;
			PatrolPoint patrolPoint = method_2(player, out dist);
			if (dist < num)
			{
				num = dist;
				key = patrolPoint;
			}
		}
		return Dictionary_1[key];
	}

	public PatrolPoint method_2(Player testedPlayer, out float dist)
	{
		float num = float.MaxValue;
		PatrolPoint result = null;
		foreach (KeyValuePair<PatrolPoint, BotZone> item in Dictionary_1)
		{
			float sqrMagnitude = (item.Key.transform.position - testedPlayer.Position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = item.Key;
			}
		}
		dist = num;
		return result;
	}

	public void method_3(BotOwner obj)
	{
	}

	public void method_4(BotOwner obj)
	{
		BotZone botZone = method_1();
		if (Dictionary_0.ContainsKey(botZone))
		{
			GClass1888 gClass = Dictionary_0[botZone];
			int count = gClass.Count;
			if (!BotSpawner_0.Groups.ContainsKey(botZone))
			{
				return;
			}
			HashSet<BotsGroup> groups = BotSpawner_0.Groups[botZone].GetGroups(notNull: true);
			int num = 0;
			foreach (BotsGroup item in groups)
			{
				num += method_5(item);
			}
			num += BotSpawner_0.BotsDelayed;
			if (num < count)
			{
				int spawnCount = count - num;
				BotCreationDataClass data = gClass.Data.Separate(spawnCount);
				BotSpawner_0.TryToSpawnInZoneAndDelay(botZone, data, withCheckMinMax: true, newWave: false);
			}
		}
		else
		{
			Debug.LogError("Error can't find zone wave to check");
		}
	}

	public int method_5(BotsGroup group)
	{
		int num = 0;
		for (int i = 0; i < group.MembersCount; i++)
		{
			if (!group.Member(i).IsDead)
			{
				num++;
			}
		}
		return num;
	}

	public void Dispose()
	{
		if (Bool_0)
		{
			BotSpawner_0.OnBotCreated -= method_3;
			BotSpawner_0.OnBotRemoved -= method_4;
			BotSpawner_0.OnSpawnedWave -= method_0;
		}
	}
}
