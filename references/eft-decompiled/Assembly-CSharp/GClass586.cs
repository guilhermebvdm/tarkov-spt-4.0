using System;
using System.Collections.Generic;

public class GClass586 : IAIDataRoomLogic
{
	[NonSerialized]
	public ISpatialRoom IspatialRoom_0;

	public void RemoveRoom(ISpatialRoom room)
	{
		if (IspatialRoom_0 != null && room != null && room == IspatialRoom_0)
		{
			IspatialRoom_0 = null;
		}
	}

	public void AddRoom(ISpatialRoom room)
	{
		IspatialRoom_0 = room;
	}

	public List<ISpatialPortal> GetPortals()
	{
		if (IspatialRoom_0 == null)
		{
			return null;
		}
		return IspatialRoom_0.GetPortals();
	}

	public bool HaveRoom()
	{
		return IspatialRoom_0 != null;
	}
}
