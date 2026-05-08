using System;
using System.IO;
using UnityEngine;

namespace Audio.SpatialSystem.Data;

[Serializable]
public class LocationBakeSettings
{
	[NonSerialized]
	public float MaxDistance = 1000f;

	[Space]
	[Tooltip("Maximum propagation depth for indoor→indoor room pairs")]
	[Min(1f)]
	public int maxDepthIndoorToIndoor = 7;

	[Tooltip("Maximum propagation depth for indoor→outdoor room pairs")]
	[Min(1f)]
	public int maxDepthIndoorToOutdoor = 3;

	[Tooltip("Maximum propagation depth for outdoor→outdoor room pairs")]
	[Min(1f)]
	public int maxDepthOutdoorToOutdoor = 3;

	[Tooltip("Maximum distance for outdoor→outdoor room pairs")]
	[Min(1f)]
	public int maxDistanceOutdoorToOutdoor = 350;

	[Tooltip("Maximum distance for indoor→outdoor room pairs")]
	[Min(1f)]
	public int maxDistanceIndoorToOutdoor = 300;

	[Tooltip("Maximum distance for indoor→indoor room pairs")]
	[Min(1f)]
	public int maxDistanceIndoorToIndoor = 200;

	public void Write(BinaryWriter writer)
	{
		writer.Write(maxDepthIndoorToIndoor);
		writer.Write(maxDepthIndoorToOutdoor);
		writer.Write(maxDepthOutdoorToOutdoor);
		writer.Write(maxDistanceOutdoorToOutdoor);
		writer.Write(maxDistanceIndoorToOutdoor);
		writer.Write(maxDistanceIndoorToIndoor);
	}

	public void Read(BinaryReader reader)
	{
		maxDepthIndoorToIndoor = reader.ReadInt32();
		maxDepthIndoorToOutdoor = reader.ReadInt32();
		maxDepthOutdoorToOutdoor = reader.ReadInt32();
		maxDistanceOutdoorToOutdoor = reader.ReadInt32();
		maxDistanceIndoorToOutdoor = reader.ReadInt32();
		maxDistanceIndoorToIndoor = reader.ReadInt32();
	}
}
