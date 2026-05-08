using System;
using UnityEngine;

[Serializable]
[GAttribute29]
public class WeightedLootPointSpawnPosition
{
	public string Name;

	public float Weight;

	public ClassVector3 Position;

	public ClassVector3 Rotation;

	public WeightedLootPointSpawnPosition()
	{
	}

	public WeightedLootPointSpawnPosition(Vector3 position, Vector3 rotationEuler, float weight, string name)
	{
		Position = position;
		Rotation = rotationEuler;
		Weight = weight;
		Name = name;
	}
}
