using System;
using Audio.SpatialSystem.Data;

namespace Audio.SpatialSystem;

[Serializable]
public class SerializableAudioRoutesBakeData
{
	public RoomPairItem[] RoomPairsItems;

	public RoutesAwareRoomItem[] RoutesAwareRoomItems;

	public RoutesByRoomPairIDItem[] RoutesByRoomPairIDItems;

	public LocationBakeSettings BakeSettings = new LocationBakeSettings();

	public uint RoomPairMaxRoutesCapacity;

	public uint RoomPairMaxPortalsCapacity;
}
