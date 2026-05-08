using System;
using System.Collections.Generic;
using System.Text;
using EFT.Game.Spawning;

public class GClass701
{
	public bool Inited;

	[NonSerialized]
	public List<GClass700> List_0 = new List<GClass700>();

	[NonSerialized]
	public GClass700 Gclass700_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public Dictionary<ISpawnPoint, bool> Dictionary_0 = new Dictionary<ISpawnPoint, bool>();

	[NonSerialized]
	public ISpawnPoint IspawnPoint_0;

	public StringBuilder DebugInfo()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("_collectionsOccupiedPoints");
		foreach (KeyValuePair<ISpawnPoint, bool> item in Dictionary_0)
		{
			stringBuilder.AppendLine($"point:{item.Key.Id} ({item.Key.Position}) val:{item.Value}");
		}
		stringBuilder.AppendLine($"_calcFarestFromPlayer:{Bool_0}");
		foreach (GClass700 item2 in List_0)
		{
			stringBuilder.AppendLine(item2.DebugInfo().ToString());
		}
		if (Gclass700_0 != null)
		{
			stringBuilder.AppendLine("_resultFarset:" + Gclass700_0.DebugInfo().ToString());
		}
		if (IspawnPoint_0 != null)
		{
			stringBuilder.AppendLine($"_finalNoValidRandom:{IspawnPoint_0}");
		}
		return stringBuilder;
	}

	public void StartCalcFarthestFromPlayers()
	{
		Inited = true;
		Bool_0 = true;
	}

	public void AddFarthestFromPlayersResult(ISpawnPoint rnd, float dist)
	{
		Gclass700_0 = new GClass700(rnd, dist);
	}

	public void AddFarthestFromPlayers(ISpawnPoint spawnPoint, float sqrDistance)
	{
		List_0.Add(new GClass700(spawnPoint, sqrDistance));
	}

	public void AddNotOccupied(ISpawnPoint point, bool isNotOccupied)
	{
		GClass856.ForceAddValue(Dictionary_0, point, isNotOccupied);
	}

	public void AddFinalNoValiedRandom(ISpawnPoint spawnPoint)
	{
		IspawnPoint_0 = spawnPoint;
	}
}
