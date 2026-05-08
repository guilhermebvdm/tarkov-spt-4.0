using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class ServerSpatialController : MonoBehaviourSingleton<ServerSpatialController>
{
	public List<BaseSpatialRoom> Rooms;

	public List<ServerSpatialPortal> Portals;

	[CompilerGenerated]
	private bool bool_0;

	public bool Restored
	{
		[CompilerGenerated]
		get
		{
			return bool_0;
		}
		[CompilerGenerated]
		set
		{
			bool_0 = value;
		}
	}

	public void Restore()
	{
		Dictionary<int, BaseSpatialRoom> dictionary = new Dictionary<int, BaseSpatialRoom>();
		foreach (BaseSpatialRoom room in Rooms)
		{
			dictionary.Add(room.ID, room);
			room.Init();
		}
		foreach (ServerSpatialPortal portal in Portals)
		{
			foreach (int item in portal.ConnectedRoomsId)
			{
				dictionary[item].AddPortal(portal);
			}
		}
		Restored = true;
	}

	public void ClearInfo()
	{
		Restored = false;
		Rooms = new List<BaseSpatialRoom>();
		Portals = new List<ServerSpatialPortal>();
	}
}
