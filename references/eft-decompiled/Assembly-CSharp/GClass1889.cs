using System;
using System.Collections.Generic;
using EFT;
using JetBrains.Annotations;

public class GClass1889 : BotSpawner
{
	public GClass1889([NotNull] IBotCreator botCreator, [NotNull] IBotGame game, [NotNull] BotZone[] botZones, [NotNull] BotsClass bots, [NotNull] ISpawnSystem spawnSystem, int maxBots, bool freeForAll, [NotNull] Dictionary<PatrolPoint, BotZone> allZonesPatrols, string openZones)
		: base(botCreator, game, botZones, bots, spawnSystem, maxBots, freeForAll, allZonesPatrols, openZones)
	{
	}

	public override GClass1884 TrySpawnFreeInner(BotCreationDataClass data, bool newWave, Action<GClass1884> callback)
	{
		int count = data.Count;
		Dictionary<BotZone, float> dictionary = new Dictionary<BotZone, float>();
		if (AllPlayers.Count > 0)
		{
			foreach (Player allPlayer in AllPlayers)
			{
				Dictionary<PatrolPoint, BotZone> testingZones = (data.IsValidSpawnType(WildSpawnType.marksman) ? ZonesPatrolsSnipe : ZonesPatrols);
				float dist;
				BotZone closestZone = GetClosestZone(allPlayer.Position, testingZones, out dist);
				if (!(closestZone != null))
				{
					continue;
				}
				if (dictionary.ContainsKey(closestZone))
				{
					float num = dictionary[closestZone];
					if (dist < num)
					{
						dictionary[closestZone] = dist;
					}
				}
				else
				{
					dictionary.Add(closestZone, dist);
				}
			}
		}
		else
		{
			BotZone[] openedZones = OpenedZones;
			foreach (BotZone key in openedZones)
			{
				dictionary.Add(key, 1f);
			}
		}
		int num2 = 0;
		if (count >= dictionary.Count)
		{
			_ = dictionary.Count;
			int num3 = count / dictionary.Count;
			int num4 = count - num3;
			foreach (KeyValuePair<BotZone, float> item in dictionary)
			{
				int num5 = num3;
				if (num4 > 0)
				{
					num4--;
					num5++;
				}
				BotCreationDataClass botCreationDataClass = data.Separate(num5);
				GClass1884 gClass = TryToSpawnInZoneInner(item.Key, botCreationDataClass, botCreationDataClass.Count, withCheckMinMax: true, newWave);
				if (gClass != null && gClass.Count > 0)
				{
					num2 += gClass.Count;
				}
			}
		}
		else
		{
			int num4 = count;
			foreach (KeyValuePair<BotZone, float> item2 in dictionary)
			{
				if (num4 > 0)
				{
					num4--;
					BotCreationDataClass botCreationDataClass2 = data.Separate(1);
					GClass1884 gClass2 = TryToSpawnInZoneInner(item2.Key, botCreationDataClass2, botCreationDataClass2.Count, withCheckMinMax: true, newWave);
					if (gClass2 != null && gClass2.Count > 0)
					{
						num2 += gClass2.Count;
					}
				}
			}
		}
		if (num2 > 0)
		{
			return new GClass1884(null, num2, data, callback);
		}
		return null;
	}
}
