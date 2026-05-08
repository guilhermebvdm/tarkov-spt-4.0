using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT.SynchronizableObjects;
using UnityEngine;

public class BotsPlantedMinesController
{
	[NonSerialized]
	public AICoversData Data;

	[NonSerialized]
	public Dictionary<TripwireSynchronizableObject, PlantedMineAIInfo> Planted = new Dictionary<TripwireSynchronizableObject, PlantedMineAIInfo>();

	public event GDelegate3 OnAdd;

	public event GDelegate3 OnRemove;

	public BotsPlantedMinesController(AICoversData data)
	{
		Data = data;
	}

	public void Activate()
	{
		Singleton<BotEventHandler>.Instance.OnPlantTripwire += method_2;
		Singleton<BotEventHandler>.Instance.OnDeactivateTripwire += method_0;
	}

	public void method_0(TripwireSynchronizableObject throwwear)
	{
		if (Planted.TryGetValue(throwwear, out var value))
		{
			Planted.Remove(throwwear);
			value.Deativate();
			this.OnRemove?.Invoke(throwwear, value);
		}
		throwwear.OnDestroyed -= method_1;
	}

	public void method_1(TripwireSynchronizableObject throwwear)
	{
		if (Planted.TryGetValue(throwwear, out var value))
		{
			Planted.Remove(throwwear);
			value.Deativate();
			this.OnRemove?.Invoke(throwwear, value);
		}
		throwwear.OnDestroyed -= method_1;
	}

	public void method_2(TripwireSynchronizableObject throwweap, Vector3 position)
	{
		if (!(throwweap == null) && !Planted.ContainsKey(throwweap))
		{
			NavGraphVoxelSimple voxelSafe = Data.GetVoxelSafe(position);
			List<NavGraphVoxelSimple> voxelesExtended = Data.GetVoxelesExtended(voxelSafe.IndexX, voxelSafe.IndexY, voxelSafe.IndexZ, 1, onlyWithPoints: false);
			PlantedMineAIInfo plantedMineAIInfo = new PlantedMineAIInfo(throwweap, position);
			throwweap.OnDestroyed += method_1;
			Planted.Add(throwweap, plantedMineAIInfo);
			plantedMineAIInfo.Init(voxelesExtended);
			this.OnAdd?.Invoke(throwweap, plantedMineAIInfo);
		}
	}

	public void Dispose()
	{
		Singleton<BotEventHandler>.Instance.OnDeactivateTripwire -= method_0;
		Singleton<BotEventHandler>.Instance.OnPlantTripwire -= method_2;
	}
}
