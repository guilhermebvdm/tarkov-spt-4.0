using System;

namespace Audio.SpatialSystem.Data;

[Serializable]
public struct IndoorRoomsData(int startIndex, int[] ids)
{
	public int startIndex = startIndex;

	public int[] ids = ids;
}
