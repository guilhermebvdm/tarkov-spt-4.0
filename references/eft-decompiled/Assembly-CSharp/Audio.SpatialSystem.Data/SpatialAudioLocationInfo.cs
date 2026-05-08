using UnityEngine;

namespace Audio.SpatialSystem.Data;

public class SpatialAudioLocationInfo : ScriptableObject
{
	public string relativeBakeDataPath;

	public LocationBakeSettings bakeSettings = new LocationBakeSettings();

	public uint roomsCount;

	public uint portalsCount;

	public uint routesCount;

	public uint roomPairsCount;

	public uint maxRoutesCount;

	public uint maxRoutePortalsCount;
}
