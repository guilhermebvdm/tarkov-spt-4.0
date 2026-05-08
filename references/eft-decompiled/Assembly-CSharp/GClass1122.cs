using System;
using System.Collections.Generic;
using Audio.SpatialSystem;
using Audio.SpatialSystem.Data;
using EFT;
using EFT.DataProviding;
using EFT.EnvironmentEffect;
using UnityEngine;

public class GClass1122
{
	[NonSerialized]
	public const short Short_0 = 5;

	[NonSerialized]
	public const float Float_0 = 0.1f;

	[NonSerialized]
	public List<SpatialAudioRoom> List_0 = new List<SpatialAudioRoom>();

	[NonSerialized]
	public List<SpatialAudioRoom> List_1 = new List<SpatialAudioRoom>();

	[NonSerialized]
	public Dictionary<ISpatialAudioRoom, List<ISpatialAudioRoom>> Dictionary_0 = new Dictionary<ISpatialAudioRoom, List<ISpatialAudioRoom>>();

	[NonSerialized]
	public List<ISpatialAudioRoom> List_2 = new List<ISpatialAudioRoom>();

	[NonSerialized]
	public List<ISpatialAudioRoom> List_3 = new List<ISpatialAudioRoom>(20);

	[NonSerialized]
	public List<ISpatialAudioRoom> List_4 = new List<ISpatialAudioRoom>(20);

	[NonSerialized]
	public Dictionary<int, List<ISpatialAudioRoom>> Dictionary_1 = new Dictionary<int, List<ISpatialAudioRoom>>();

	[NonSerialized]
	public Dictionary<int, List<ISpatialAudioRoom>> Dictionary_2 = new Dictionary<int, List<ISpatialAudioRoom>>();

	[NonSerialized]
	public Dictionary<int, ISpatialAudioRoom> Dictionary_3 = new Dictionary<int, ISpatialAudioRoom>();

	[NonSerialized]
	public ISpatialAudioRoom IspatialAudioRoom_0 = new GClass1149();

	[NonSerialized]
	public EAudioRoomTypeMask EaudioRoomTypeMask_0 = EAudioRoomTypeMask.Outdoor;

	[NonSerialized]
	public GClass1159<ISpatialAudioRoom> Gclass1159_0;

	[NonSerialized]
	public List<ISpatialAudioRoom> List_5 = new List<ISpatialAudioRoom>();

	[NonSerialized]
	public Dictionary<(int emitterId, int listenerId), RoomPair> Dictionary_4 = new Dictionary<(int, int), RoomPair>();

	public ISpatialAudioRoom ListenerCurrentRoom
	{
		get
		{
			if (List_3.Count > 0)
			{
				return List_3[0];
			}
			if (List_4.Count > 0)
			{
				return List_4[0];
			}
			return IspatialAudioRoom_0;
		}
	}

	public int ListenerCurrentRoomID
	{
		get
		{
			if (ListenerCurrentRoom != null)
			{
				return ListenerCurrentRoom.ID;
			}
			return -1;
		}
	}

	public int ListenerCurrentOutdoorRoomID
	{
		get
		{
			if (List_4.Count <= 0)
			{
				return -1;
			}
			return List_4[0].ID;
		}
	}

	public GClass1122()
	{
		method_0();
		method_1();
	}

	public void method_0()
	{
		foreach (SpatialAudioCrossSceneGroup allCrossGroup in SpatialAudioCrossSceneGroup.AllCrossGroups)
		{
			method_11(allCrossGroup.AudioRooms);
		}
		List_1.Clear();
		foreach (SpatialAudioRoom item in List_0)
		{
			if (GClass1118.IsOutdoor(item.Type))
			{
				List_1.Add(item);
			}
		}
		method_12();
		GClass3670.CreateData<GClass1123>(EDataLifeTime.Raid);
		GClass3670.GetData<GClass1123>().FillData(List_0);
	}

	public void method_1()
	{
		if (List_0.Count == 0)
		{
			return;
		}
		Bounds bounds = List_0[0].Bounds;
		foreach (SpatialAudioRoom item in List_0)
		{
			bounds.Encapsulate(item.Bounds);
		}
		Gclass1159_0 = new GClass1159<ISpatialAudioRoom>(bounds, 4);
		foreach (SpatialAudioRoom item2 in List_0)
		{
			Gclass1159_0.Insert(item2, item2.Bounds);
		}
	}

	public void AddPlayerCurrentRoom(SpatialAudioRoom room, IPlayer player)
	{
		if (player.IsYourPlayer)
		{
			method_2(room);
			return;
		}
		if (player.AIData != null)
		{
			player.AIData.roomLogicLogic.AddRoom(room);
		}
		Dictionary<int, List<ISpatialAudioRoom>> dictionary = (room.IsOutdoor ? Dictionary_2 : Dictionary_1);
		int id = player.Id;
		Dictionary_3[id] = room;
		if (dictionary.TryGetValue(id, out var value))
		{
			value.Remove(room);
			value.Insert(0, room);
		}
		else
		{
			dictionary.Add(id, new List<ISpatialAudioRoom> { room });
		}
	}

	public void method_2(SpatialAudioRoom room)
	{
		if (!ListenerCurrentRoom.IsValid || ListenerCurrentRoom.ID != room.ID)
		{
			List<ISpatialAudioRoom> list = (room.IsOutdoor ? List_4 : List_3);
			if (list.Contains(room))
			{
				list.Remove(room);
				list.Insert(0, room);
			}
			else
			{
				list.Insert(0, room);
			}
			EaudioRoomTypeMask_0 = ListenerCurrentRoom?.Type ?? EAudioRoomTypeMask.Outdoor;
			GlobalEventHandlerClass.CreateEvent<GClass3573>().Invoke(ListenerCurrentRoom, ListenerCurrentRoomID, EaudioRoomTypeMask_0, ListenerCurrentOutdoorRoomID, EPlayerRoomInteractionState.Enter);
		}
	}

	public void RemovePlayerCurrentRoom(SpatialAudioRoom room, IPlayer player)
	{
		try
		{
			player.AIData?.roomLogicLogic.RemoveRoom(room);
		}
		catch (Exception arg)
		{
			Debug.LogError($"RemovePlayerCurrentRoom:{arg}");
		}
		if (player.IsYourPlayer)
		{
			method_3(room);
			return;
		}
		Dictionary<int, List<ISpatialAudioRoom>> obj = (room.IsOutdoor ? Dictionary_2 : Dictionary_1);
		int id = player.Id;
		if (obj.TryGetValue(id, out var value))
		{
			if (value.Count == 1)
			{
				Dictionary_3[id] = room;
			}
			value.Remove(room);
		}
	}

	public void method_3(SpatialAudioRoom room)
	{
		(room.IsOutdoor ? List_4 : List_3).Remove(room);
		EaudioRoomTypeMask_0 = ((ListenerCurrentRoom == null) ? EAudioRoomTypeMask.Outdoor : ListenerCurrentRoom.Type);
		GlobalEventHandlerClass.CreateEvent<GClass3573>().Invoke(ListenerCurrentRoom, ListenerCurrentRoomID, EaudioRoomTypeMask_0, ListenerCurrentOutdoorRoomID, EPlayerRoomInteractionState.Exit);
	}

	public ISpatialAudioRoom GetOtherPlayerCurrentRoom(int playerId)
	{
		ISpatialAudioRoom ispatialAudioRoom_ = IspatialAudioRoom_0;
		if (Dictionary_1.TryGetValue(playerId, out var value) && value.Count > 0)
		{
			return value[0];
		}
		if (Dictionary_2.TryGetValue(playerId, out var value2) && value2.Count > 0)
		{
			return value2[0];
		}
		return ispatialAudioRoom_;
	}

	public ISpatialAudioRoom method_4<T>(List<T> roomsToCheck, Vector3 position) where T : class, ISpatialAudioRoom
	{
		List_2.Clear();
		foreach (T item in roomsToCheck)
		{
			if (method_6(item, position))
			{
				List_2.Add(item);
			}
			if (List_2.Count >= 5)
			{
				break;
			}
		}
		return method_9(List_2, List_2.Count);
	}

	public ISpatialAudioRoom method_5(List<SpatialAudioRoom.RoomConnection> connectionsToCheck, Vector3 position)
	{
		List_2.Clear();
		foreach (SpatialAudioRoom.RoomConnection item in connectionsToCheck)
		{
			if (method_6(item.connectedRoom, position))
			{
				List_2.Add(item.connectedRoom);
			}
			if (List_2.Count >= 5)
			{
				break;
			}
		}
		return method_9(List_2, List_2.Count);
	}

	public bool method_6(ISpatialAudioRoom room, Vector3 position)
	{
		if (!room.Bounds.Contains(position))
		{
			return false;
		}
		BoxCollider[] colliders = room.Colliders;
		int num = 0;
		while (true)
		{
			if (num < colliders.Length)
			{
				if (colliders[num].bounds.Contains(position))
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return true;
	}

	public ISpatialAudioRoom FindInitialRoomOptimized(Vector3 position)
	{
		if (Gclass1159_0 != null)
		{
			List_5.Clear();
			Gclass1159_0.Query(position, List_5);
			List_2.Clear();
			foreach (ISpatialAudioRoom item in List_5)
			{
				if (method_6(item, position))
				{
					List_2.Add(item);
				}
			}
			if (List_2.Count > 0)
			{
				ISpatialAudioRoom spatialAudioRoom = List_2[0];
				for (int i = 1; i < List_2.Count; i++)
				{
					if (List_2[i].RoomSize < spatialAudioRoom.RoomSize)
					{
						spatialAudioRoom = List_2[i];
					}
				}
				return spatialAudioRoom;
			}
		}
		List_2.Clear();
		if (List_1.Count == 0)
		{
			return method_4(List_0, position);
		}
		foreach (SpatialAudioRoom item2 in List_1)
		{
			if (method_6(item2, position))
			{
				ISpatialAudioRoom spatialAudioRoom2 = method_5(item2.roomConnections, position);
				if (spatialAudioRoom2.IsValid)
				{
					List_2.Clear();
					List_2.Add(spatialAudioRoom2);
					List_2.Add(item2);
					break;
				}
			}
		}
		if (!method_7(position))
		{
			return method_4(List_0, position);
		}
		ISpatialAudioRoom spatialAudioRoom3 = method_9(List_2, List_2.Count);
		if (!spatialAudioRoom3.IsValid)
		{
			return method_4(List_0, position);
		}
		return spatialAudioRoom3;
	}

	public bool method_7(Vector3 sourcePos)
	{
		if (List_2.Count == 1 && GClass1118.IsOutdoor(List_2[0].Type))
		{
			return EnvironmentManager.Instance.GetEnvironmentByPos(sourcePos) == EnvironmentType.Outdoor;
		}
		return true;
	}

	public ISpatialAudioRoom FindActualCurrentRoom(ISpatialAudioRoom previouslyKnownRoom, Vector3 currentPosition)
	{
		Bounds sourceBounds = new Bounds(currentPosition, Vector3.zero);
		return FindActualCurrentRoom(previouslyKnownRoom, sourceBounds);
	}

	public ISpatialAudioRoom FindActualCurrentRoom(ISpatialAudioRoom previouslyKnownRoom, Bounds sourceBounds)
	{
		if (previouslyKnownRoom.IsValid && Dictionary_0.TryGetValue(previouslyKnownRoom, out var value) && value.Count > 0)
		{
			foreach (ISpatialAudioRoom item in value)
			{
				if (method_8(item, sourceBounds))
				{
					return item;
				}
			}
		}
		List_5.Clear();
		Gclass1159_0.Query(sourceBounds, List_5);
		List_2.Clear();
		foreach (ISpatialAudioRoom item2 in List_5)
		{
			if (method_8(item2, sourceBounds))
			{
				List_2.Add(item2);
			}
		}
		if (List_2.Count > 0)
		{
			ISpatialAudioRoom spatialAudioRoom = List_2[0];
			for (int i = 1; i < List_2.Count; i++)
			{
				if (List_2[i].RoomSize < spatialAudioRoom.RoomSize)
				{
					spatialAudioRoom = List_2[i];
				}
			}
			return spatialAudioRoom;
		}
		return FindInitialRoomOptimized(sourceBounds.center);
	}

	public bool method_8(ISpatialAudioRoom room, Bounds sourceBounds)
	{
		Bounds bounds = room.Bounds;
		bounds.Expand(0.1f);
		if (!bounds.Intersects(sourceBounds))
		{
			return false;
		}
		BoxCollider[] colliders = room.Colliders;
		int num = 0;
		while (true)
		{
			if (num < colliders.Length)
			{
				Bounds bounds2 = colliders[num].bounds;
				bounds2.Expand(0.1f);
				if (bounds2.Intersects(sourceBounds))
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return true;
	}

	public ISpatialAudioRoom method_9(IReadOnlyList<ISpatialAudioRoom> proposedRooms, int roomsCount)
	{
		if (proposedRooms.Count == 0)
		{
			return IspatialAudioRoom_0;
		}
		if (roomsCount == 1)
		{
			return proposedRooms[0];
		}
		return method_10(proposedRooms[roomsCount - 1], method_9(proposedRooms, roomsCount - 1));
	}

	public ISpatialAudioRoom method_10(ISpatialAudioRoom roomA, ISpatialAudioRoom roomB)
	{
		if (!(roomA.RoomSize < roomB.RoomSize))
		{
			return roomB;
		}
		return roomA;
	}

	public bool TryGetMatchingRoomPair(ISpatialAudioRoom emitterRoom, ISpatialAudioRoom listenerRoom, out RoomPair roomPair, out bool isReversed)
	{
		roomPair = null;
		isReversed = false;
		if (emitterRoom.IsValid && listenerRoom.IsValid)
		{
			short iD = emitterRoom.ID;
			short iD2 = listenerRoom.ID;
			(short, short) key = ((iD < iD2) ? (iD, iD2) : (iD2, iD));
			if (GClass3670.TryGetData<GClass1138>(out var dataContainer) && dataContainer.RoomPairIndex.TryGetValue(key, out roomPair))
			{
				isReversed = emitterRoom.ID > listenerRoom.ID;
				return true;
			}
			return false;
		}
		return false;
	}

	public void method_11(List<SpatialAudioRoom> audioRooms)
	{
		foreach (SpatialAudioRoom audioRoom in audioRooms)
		{
			if ((object)audioRoom != null)
			{
				audioRoom.Initialize();
				if (audioRoom.IsInitialized && !List_0.Contains(audioRoom))
				{
					List_0.Add(audioRoom);
				}
			}
		}
	}

	public void method_12()
	{
		if (List_0.Count == 0)
		{
			return;
		}
		foreach (SpatialAudioRoom item in List_0)
		{
			method_13(item);
		}
	}

	public void method_13(SpatialAudioRoom room)
	{
		List<ISpatialAudioRoom> list = smethod_0(room);
		if (list != null && list.Count > 0)
		{
			Dictionary_0[room] = list;
		}
	}

	public static List<ISpatialAudioRoom> smethod_0(SpatialAudioRoom startingRoom)
	{
		if ((object)startingRoom == null)
		{
			return null;
		}
		List<ISpatialAudioRoom> list = new List<ISpatialAudioRoom> { startingRoom };
		foreach (SpatialAudioRoom.RoomConnection roomConnection in startingRoom.roomConnections)
		{
			list.Add(roomConnection.connectedRoom);
			foreach (SpatialAudioRoom.RoomConnection roomConnection2 in roomConnection.connectedRoom.roomConnections)
			{
				if (!list.Contains(roomConnection2.connectedRoom))
				{
					list.Add(roomConnection2.connectedRoom);
				}
			}
		}
		return list;
	}

	public void Clear()
	{
		Dictionary_3?.Clear();
		List_0?.Clear();
		Dictionary_0?.Clear();
	}
}
