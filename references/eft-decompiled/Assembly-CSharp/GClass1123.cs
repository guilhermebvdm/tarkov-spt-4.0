using System;
using System.Collections.Generic;
using Audio.SpatialSystem;
using Audio.SpatialSystem.Data;
using UnityEngine;

public class GClass1123 : GInterface463
{
	[NonSerialized]
	public Dictionary<int, List<BaseSpatialAudioPortal>> Dictionary_0 = new Dictionary<int, List<BaseSpatialAudioPortal>>();

	[NonSerialized]
	public Dictionary<int, Dictionary<int, BaseSpatialAudioPortal>> Dictionary_1 = new Dictionary<int, Dictionary<int, BaseSpatialAudioPortal>>();

	[NonSerialized]
	public Dictionary<int, PortalData[]> Dictionary_2 = new Dictionary<int, PortalData[]>();

	[NonSerialized]
	public Dictionary<int, short[]> Dictionary_3 = new Dictionary<int, short[]>();

	[NonSerialized]
	public HashSet<short> HashSet_0;

	[NonSerialized]
	public Dictionary<int, Dictionary<int, int>> Dictionary_4 = new Dictionary<int, Dictionary<int, int>>();

	public void FillData(List<SpatialAudioRoom> audioRooms)
	{
		foreach (SpatialAudioRoom audioRoom in audioRooms)
		{
			method_0(audioRoom);
		}
		method_3();
	}

	public bool TryGetPortalsByRelativeOutdoorRoomID(int roomID, out List<BaseSpatialAudioPortal> portals)
	{
		return Dictionary_0.TryGetValue(roomID, out portals);
	}

	public bool TryGetOutdoorPortalData(int roomID, out PortalData[] data)
	{
		return Dictionary_2.TryGetValue(roomID, out data);
	}

	public bool TryGetIndoorRoomsIdsByOutRoomID(int roomID, out short[] ids)
	{
		return Dictionary_3.TryGetValue(roomID, out ids);
	}

	public bool TryUpdateConcretePortalData(int portalID)
	{
		bool result = false;
		if (Dictionary_4.TryGetValue(portalID, out var value))
		{
			foreach (var (roomID, num3) in value)
			{
				if (TryGetPortalsByRelativeOutdoorRoomID(roomID, out var portals) && TryGetOutdoorPortalData(roomID, out var data) && data.Length > num3 && portals.Count > num3)
				{
					data[num3].state = (byte)portals[num3].state;
					result = true;
				}
			}
		}
		return result;
	}

	public void FullUpdatePortalsData(int outRoomID)
	{
		if (!TryGetPortalsByRelativeOutdoorRoomID(outRoomID, out var portals) || !TryGetOutdoorPortalData(outRoomID, out var data))
		{
			return;
		}
		if (data.Length != portals.Count)
		{
			Debug.LogError("portal data not identical with portals");
			return;
		}
		for (int i = 0; i < portals.Count; i++)
		{
			data[i].state = (byte)portals[i].state;
		}
	}

	public void method_0(SpatialAudioRoom room)
	{
		if (!GClass1118.IsOutdoor(room.Type))
		{
			return;
		}
		int portalIndex = 0;
		short iD = room.ID;
		foreach (SpatialAudioRoom.RoomConnection roomConnection in room.roomConnections)
		{
			SpatialAudioRoom connectedRoom = roomConnection.connectedRoom;
			if (GClass1118.IsOutdoor(connectedRoom.Type) && connectedRoom.priority < room.priority)
			{
				foreach (SpatialAudioRoom.RoomConnection roomConnection2 in connectedRoom.roomConnections)
				{
					SpatialAudioRoom connectedRoom2 = roomConnection2.connectedRoom;
					if (GClass1118.IsOutdoor(connectedRoom2.Type))
					{
						foreach (SpatialAudioRoom.RoomConnection roomConnection3 in connectedRoom2.roomConnections)
						{
							method_1(iD, roomConnection3, ref portalIndex);
						}
					}
					method_1(iD, roomConnection2, ref portalIndex);
				}
			}
			method_1(iD, roomConnection, ref portalIndex);
		}
	}

	public void method_1(int rootRoomID, SpatialAudioRoom.RoomConnection connection, ref int portalIndex)
	{
		BaseSpatialAudioPortal[] connectingPortals = connection.connectingPortals;
		foreach (BaseSpatialAudioPortal baseSpatialAudioPortal in connectingPortals)
		{
			if (baseSpatialAudioPortal.ToOutdoor && (!GClass1118.IsOutdoor(baseSpatialAudioPortal.FrontRoom.Type) || !GClass1118.IsOutdoor(baseSpatialAudioPortal.BackRoom.Type)) && method_2(rootRoomID, baseSpatialAudioPortal, portalIndex))
			{
				portalIndex++;
			}
		}
	}

	public bool method_2(int roomID, BaseSpatialAudioPortal portal, int portalIndex)
	{
		if (Dictionary_0.TryGetValue(roomID, out var value) && value.Contains(portal))
		{
			return false;
		}
		if (!Dictionary_0.TryGetValue(roomID, out var value2))
		{
			Dictionary_0[roomID] = new List<BaseSpatialAudioPortal> { portal };
		}
		else
		{
			value2.Add(portal);
		}
		if (Dictionary_1.TryGetValue(roomID, out var value3))
		{
			value3[portal.ID] = portal;
		}
		else
		{
			Dictionary_1[roomID] = new Dictionary<int, BaseSpatialAudioPortal> { { portal.ID, portal } };
		}
		if (!Dictionary_4.TryGetValue(portal.ID, out var value4))
		{
			Dictionary_4[portal.ID] = new Dictionary<int, int> { { roomID, portalIndex } };
		}
		else
		{
			value4[roomID] = portalIndex;
		}
		return true;
	}

	public void method_3()
	{
		foreach (KeyValuePair<int, List<BaseSpatialAudioPortal>> item in Dictionary_0)
		{
			item.Deconstruct(out var key, out var value);
			int num = key;
			List<BaseSpatialAudioPortal> list = value;
			int count = list.Count;
			Dictionary_2[num] = new PortalData[count];
			Dictionary_3[num] = Array.Empty<short>();
			short num2 = 0;
			for (short num3 = 0; num3 < count; num3++)
			{
				BaseSpatialAudioPortal baseSpatialAudioPortal = list[num3];
				if (!GClass1118.IsOutdoor(baseSpatialAudioPortal.FrontRoom.Type) || !GClass1118.IsOutdoor(baseSpatialAudioPortal.BackRoom.Type))
				{
					method_4(baseSpatialAudioPortal, num2, num, out var indoorRoomsCount);
					Dictionary_2[num][num3] = method_6(baseSpatialAudioPortal, num2, indoorRoomsCount);
					num2 += indoorRoomsCount;
				}
			}
		}
	}

	public void method_4(BaseSpatialAudioPortal portal, short startIndex, int outRoomID, out short indoorRoomsCount)
	{
		indoorRoomsCount = 0;
		SpatialAudioRoom spatialAudioRoom = (GClass1118.IsIndoor(portal.FrontRoom.Type) ? portal.FrontRoom : portal.BackRoom);
		short[] array = Dictionary_3[outRoomID];
		indoorRoomsCount = (short)(spatialAudioRoom.roomConnections.Count + 1);
		int num = array.Length + indoorRoomsCount;
		Dictionary_3[outRoomID] = method_5(outRoomID, num);
		Dictionary_3[outRoomID][startIndex] = spatialAudioRoom.ID;
		if (indoorRoomsCount <= 1)
		{
			return;
		}
		int num2 = startIndex + 1;
		int num3 = 0;
		foreach (SpatialAudioRoom.RoomConnection roomConnection in spatialAudioRoom.roomConnections)
		{
			if (GClass1118.IsOutdoor(roomConnection.connectedRoom.Type))
			{
				num--;
				Dictionary_3[outRoomID] = method_5(outRoomID, num);
				indoorRoomsCount--;
			}
			else
			{
				Dictionary_3[outRoomID][num2 + num3] = roomConnection.connectedRoom.ID;
				num3++;
			}
		}
	}

	public short[] method_5(int outRoomID, int newSize)
	{
		short[] array = Dictionary_3[outRoomID];
		Array.Resize(ref array, newSize);
		return array;
	}

	public PortalData method_6(BaseSpatialAudioPortal portal, short startIndex, short indoorRoomsCount)
	{
		return new PortalData(portal.ID, portal.transform.position, portal.transform.up, portal.traversalMaxCost, (byte)portal.state, portal.Depth, startIndex, indoorRoomsCount);
	}
}
