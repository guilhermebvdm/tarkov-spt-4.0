using System;
using System.Collections.Generic;
using System.Linq;
using EFT;
using JetBrains.Annotations;

public class GClass1890 : BotSpawner
{
	public GClass1890([NotNull] IBotCreator botCreator, [NotNull] IBotGame game, [NotNull] BotZone[] botZones, [NotNull] BotsClass bots, [NotNull] ISpawnSystem spawnSystem, int maxBots, bool freeForAll, [NotNull] Dictionary<PatrolPoint, BotZone> allZonesPatrols, string openZones)
		: base(botCreator, game, botZones, bots, spawnSystem, maxBots, freeForAll, allZonesPatrols, openZones)
	{
	}

	public override GClass1884 TrySpawnFreeInner(BotCreationDataClass data, bool newWave, Action<GClass1884> callback)
	{
		if (data.SpawnStopped)
		{
			return null;
		}
		GClass1884 result = null;
		bool flag = data.IsValidSpawnType(WildSpawnType.marksman);
		int totalSum;
		Dictionary<BotZone, int> dictionary = method_14(flag, out totalSum);
		Dictionary<BotZone, float> dictionary2 = new Dictionary<BotZone, float>();
		int count = data.Count;
		Dictionary<PatrolPoint, BotZone> testingZones = (flag ? ZonesPatrolsSnipe : ZonesPatrols);
		if (AllPlayers.Count > 0)
		{
			foreach (Player allPlayer in AllPlayers)
			{
				float dist;
				BotZone closestZone = GetClosestZone(allPlayer.Position, testingZones, out dist);
				if (!(closestZone != null))
				{
					continue;
				}
				if (dictionary2.ContainsKey(closestZone))
				{
					float num = dictionary2[closestZone];
					if (dist < num)
					{
						dictionary2[closestZone] = dist;
					}
				}
				else if (dictionary.ContainsKey(closestZone))
				{
					dictionary2.Add(closestZone, dist);
				}
			}
		}
		else
		{
			BotZone[] allBotZones = AllBotZones;
			foreach (BotZone key in allBotZones)
			{
				if (dictionary.ContainsKey(key))
				{
					dictionary2.Add(key, 1f);
				}
			}
		}
		if (data.SpawnParams != null && data.SpawnParams.ShallBeGroup != null && data.SpawnParams.ShallBeGroup.Group)
		{
			TryToSpawnInZoneAndDelay(dictionary2.FirstOrDefault().Key, data, withCheckMinMax: true, newWave: true);
			return null;
		}
		if (count >= dictionary2.Count && count >= totalSum)
		{
			int num2 = 0;
			foreach (KeyValuePair<BotZone, float> item in dictionary2)
			{
				num2++;
				dictionary[item.Key]--;
				totalSum--;
				BotCreationDataClass data2 = data.Separate(1);
				TryToSpawnInZoneAndDelay(item.Key, data2, withCheckMinMax: true, newWave);
			}
			List<BotZone> list = new List<BotZone>();
			foreach (KeyValuePair<BotZone, int> item2 in dictionary)
			{
				if (item2.Value <= 0)
				{
					list.Add(item2.Key);
				}
			}
			foreach (BotZone item3 in list)
			{
				dictionary.Remove(item3);
				dictionary2.Remove(item3);
			}
			int num3 = count - num2;
			if (num3 > totalSum)
			{
				int count2 = num3 - totalSum;
				result = new GClass1884(null, count2, data, callback);
				num3 = totalSum;
			}
			if (dictionary.Count > 0)
			{
				for (int j = 0; j < num3; j++)
				{
					BotZone botZone = GClass856.RandomElement(dictionary.Keys);
					if (botZone != null)
					{
						num2--;
						dictionary[botZone]--;
						if (dictionary[botZone] <= 0)
						{
							dictionary.Remove(botZone);
						}
					}
					BotCreationDataClass data3 = data.Separate(1);
					TryToSpawnInZoneAndDelay(botZone, data3, withCheckMinMax: true, newWave);
				}
			}
			return result;
		}
		int num4 = count;
		for (int k = 0; k < count; k++)
		{
			if (dictionary.Count <= 0)
			{
				break;
			}
			BotZone botZone2 = GClass856.RandomElement(dictionary.Keys);
			if (botZone2 != null)
			{
				dictionary[botZone2]--;
				if (dictionary[botZone2] <= 0)
				{
					dictionary.Remove(botZone2);
				}
			}
			num4--;
			BotCreationDataClass data4 = data.Separate(1);
			TryToSpawnInZoneAndDelay(botZone2, data4, withCheckMinMax: true, newWave);
		}
		if (num4 > 0)
		{
			return new GClass1884(null, num4, data, callback);
		}
		return null;
	}

	public Dictionary<BotZone, int> method_14(bool isSniper, out int totalSum)
	{
		Dictionary<BotZone, int> dictionary = new Dictionary<BotZone, int>();
		totalSum = 0;
		BotZone[] allBotZones = AllBotZones;
		foreach (BotZone botZone in allBotZones)
		{
			if (isSniper == botZone.SnipeZone)
			{
				int num = ((botZone.MaxPersons > 0) ? botZone.MaxPersons : 10);
				int count = Bots.GetListByZone(botZone).Count;
				int num2 = num - count;
				if (num2 > 0)
				{
					totalSum += num2;
					dictionary.Add(botZone, num2);
				}
			}
		}
		return dictionary;
	}
}
