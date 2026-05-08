using System;

namespace Audio.SpatialSystem;

[Serializable]
public struct RoutesByRoomPairIDItem
{
	public uint roomPairID;

	public AudioRouteData[] routesData;
}
