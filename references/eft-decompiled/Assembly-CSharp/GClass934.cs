using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public abstract class GClass934
{
	public static Dictionary<string, Material> RoadsMaterials = new Dictionary<string, Material>();

	public static List<Renderer> LodRoads = new List<Renderer>();

	public static Material Get(string materialName, Renderer renderer)
	{
		if (!RoadsMaterials.ContainsKey(materialName))
		{
			Material value = new Material(renderer.sharedMaterial);
			RoadsMaterials.Add(materialName, value);
		}
		return RoadsMaterials[materialName];
	}

	public static void Add(Renderer renderer)
	{
		LodRoads.Add(renderer);
	}

	public static void Remove(string materialName, Renderer renderer)
	{
		if (RoadsMaterials.ContainsKey(materialName))
		{
			RoadsMaterials.Remove(materialName);
		}
		if (LodRoads.Contains(renderer))
		{
			LodRoads.Remove(renderer);
		}
	}
}
