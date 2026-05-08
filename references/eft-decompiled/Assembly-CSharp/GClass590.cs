using System;
using System.Collections.Generic;

public class GClass590 : IAIDataRoomLogic
{
	[NonSerialized]
	public List<ISpatialPortal> List_0 = new List<ISpatialPortal>();

	public void AddRoom(ISpatialRoom room)
	{
	}

	public List<ISpatialPortal> GetPortals()
	{
		return List_0;
	}

	public bool HaveRoom()
	{
		return false;
	}

	public void RemoveRoom(ISpatialRoom room)
	{
	}
}
