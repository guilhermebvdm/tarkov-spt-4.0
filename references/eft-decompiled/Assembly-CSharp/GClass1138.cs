using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Audio.SpatialSystem;
using Audio.SpatialSystem.Data;
using Unity.Collections;

public class GClass1138 : GInterface463, IDisposable
{
	public struct Struct207
	{
		public int startIndex;

		public int endExclusive;

		public int Count => endExclusive - startIndex;
	}

	public struct Struct208
	{
		public uint roomPairID;

		public AudioRouteData[] data;
	}

	public LocationBakeSettings bakeSettings;

	public uint roomPairMaxRoutesCapacity;

	public uint roomPairMaxPortalsCapacity;

	[NonSerialized]
	public GClass1124 Gclass1124_0 = new GClass1124();

	[NonSerialized]
	public List<Struct208> List_0 = new List<Struct208>();

	[NonSerialized]
	public NativeArray<AudioRouteData> NativeArray_0;

	[NonSerialized]
	public Dictionary<uint, Struct207> Dictionary_0 = new Dictionary<uint, Struct207>();

	[NonSerialized]
	public Dictionary<short, RoomPair.AudioPortalData> Dictionary_1 = new Dictionary<short, RoomPair.AudioPortalData>();

	[NonSerialized]
	public NativeArray<int> NativeArray_1;

	[NonSerialized]
	public Dictionary<uint, Struct207> Dictionary_2 = new Dictionary<uint, Struct207>();

	[NonSerialized]
	public Dictionary<(short emitterId, short listenerId), RoomPair> Dictionary_3 = new Dictionary<(short, short), RoomPair>();

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public Dictionary<short, BaseSpatialAudioPortal> Dictionary_4 = new Dictionary<short, BaseSpatialAudioPortal>();

	[NonSerialized]
	[CompilerGenerated]
	public Dictionary<string, BaseSpatialAudioPortal> Dictionary_5 = new Dictionary<string, BaseSpatialAudioPortal>();

	[NonSerialized]
	[CompilerGenerated]
	public Dictionary<short, SpatialAudioRoom> Dictionary_6 = new Dictionary<short, SpatialAudioRoom>();

	[NonSerialized]
	[CompilerGenerated]
	public Dictionary<uint, RoomPair> Dictionary_7 = new Dictionary<uint, RoomPair>();

	[NonSerialized]
	[CompilerGenerated]
	public Dictionary<short, RoomPair[]> Dictionary_8 = new Dictionary<short, RoomPair[]>();

	public bool IsDisposed => Bool_0;

	public Dictionary<short, BaseSpatialAudioPortal> PortalsByID
	{
		[CompilerGenerated]
		get
		{
			return Dictionary_4;
		}
	}

	public Dictionary<string, BaseSpatialAudioPortal> InteractivePortalsByID
	{
		[CompilerGenerated]
		get
		{
			return Dictionary_5;
		}
	}

	public Dictionary<short, SpatialAudioRoom> RoomsByID
	{
		[CompilerGenerated]
		get
		{
			return Dictionary_6;
		}
	}

	public Dictionary<uint, RoomPair> RoomPairsByID
	{
		[CompilerGenerated]
		get
		{
			return Dictionary_7;
		}
	}

	public Dictionary<short, RoomPair[]> RelevantRoomPairsByRoomID
	{
		[CompilerGenerated]
		get
		{
			return Dictionary_8;
		}
	}

	public int RoutesDataCount => Dictionary_0.Count;

	public NativeList<RoomPair.AudioPortalData> StaticPortalDataList => Gclass1124_0.GetActiveList();

	public bool HasPendingPortalUpdates => Gclass1124_0.HasPendingUpdates;

	public IReadOnlyDictionary<(short emitterId, short listenerId), RoomPair> RoomPairIndex => Dictionary_3;

	public void AddStaticPortalData(BaseSpatialAudioPortal portal)
	{
		RoomPair.AudioPortalData audioPortalData = method_0(portal);
		Dictionary_1[portal.ID] = audioPortalData;
		Gclass1124_0.AddPortal(portal.ID, audioPortalData);
	}

	public bool TryGetPortalById(short id, out BaseSpatialAudioPortal portal)
	{
		return PortalsByID.TryGetValue(id, out portal);
	}

	public bool TryGetInteractivePortalById(string id, out BaseSpatialAudioPortal portal)
	{
		return InteractivePortalsByID.TryGetValue(id, out portal);
	}

	public bool TryGetRoutesData(uint roomPairID, out NativeArray<AudioRouteData> data)
	{
		if (Dictionary_0.TryGetValue(roomPairID, out var value))
		{
			data = NativeArray_0.GetSubArray(value.startIndex, value.Count);
			return true;
		}
		data = default(NativeArray<AudioRouteData>);
		return false;
	}

	public void AddRoutesData(uint roomPairID, AudioRouteData[] data)
	{
		List_0.Add(new Struct208
		{
			roomPairID = roomPairID,
			data = data
		});
	}

	public void UpdatePortalData(short portalID, BaseSpatialAudioPortal.PortalState portalState, float closureLevel, float depth, float traversalCost)
	{
		method_1(portalID, closureLevel, depth, traversalCost);
	}

	public void UpdateInitialPortalsData()
	{
		foreach (var (portalID, baseSpatialAudioPortal2) in PortalsByID)
		{
			UpdatePortalData(portalID, baseSpatialAudioPortal2.state, baseSpatialAudioPortal2.PortalClosureLevel, baseSpatialAudioPortal2.Depth, baseSpatialAudioPortal2.traversalMaxCost);
		}
		SwapPortalBuffers();
	}

	public RoomPair.AudioPortalData method_0(BaseSpatialAudioPortal portal)
	{
		return new RoomPair.AudioPortalData
		{
			portalNormal = portal.transform.forward,
			portalRight = portal.transform.right,
			portalUp = portal.transform.up,
			portalHalfSize = portal.GetPortalHalfSize(),
			closureLevel = portal.PortalClosureLevel,
			traversalMaxCost = portal.traversalMaxCost,
			depth = portal.Depth,
			center = portal.portalCollider.bounds.center,
			frontRoomWallOcclusion = portal.FrontRoom.WallOcclusion,
			backRoomWallOcclusion = portal.BackRoom.WallOcclusion,
			isFrontRoomOutdoor = GClass1118.IsOutdoor(portal.FrontRoom.Type),
			isBackRoomOutdoor = GClass1118.IsOutdoor(portal.BackRoom.Type)
		};
	}

	public void method_1(short portalID, float closureLevel, float depth, float traversalCost)
	{
		BaseSpatialAudioPortal value2;
		if (Dictionary_1.TryGetValue(portalID, out var value))
		{
			value.closureLevel = closureLevel;
			value.depth = depth;
			value.traversalMaxCost = traversalCost;
			Dictionary_1[portalID] = value;
			Gclass1124_0.UpdatePortalData(portalID, value);
		}
		else if (PortalsByID.TryGetValue(portalID, out value2))
		{
			RoomPair.AudioPortalData audioPortalData = method_0(value2);
			Dictionary_1[portalID] = audioPortalData;
			Gclass1124_0.AddPortal(portalID, audioPortalData);
		}
		else
		{
			GClass722.Instance.LogError($"Can't find portal with ID : {portalID}");
		}
	}

	public void FinalizeRoutesData()
	{
		int num = 0;
		foreach (Struct208 item in List_0)
		{
			num += item.data.Length;
		}
		NativeArray_0 = new NativeArray<AudioRouteData>(num, Allocator.Persistent);
		int num2 = 0;
		foreach (Struct208 item2 in List_0)
		{
			for (int i = 0; i < item2.data.Length; i++)
			{
				NativeArray_0[num2 + i] = item2.data[i];
			}
			Dictionary_0[item2.roomPairID] = new Struct207
			{
				startIndex = num2,
				endExclusive = num2 + item2.data.Length
			};
			num2 += item2.data.Length;
		}
		List_0.Clear();
	}

	public void FinalizePortalIndicesData()
	{
		int num = 0;
		foreach (RoomPair value2 in RoomPairsByID.Values)
		{
			RoomPair.Route[] routes = value2.Routes;
			for (int i = 0; i < routes.Length; i++)
			{
				RoomPair.Route route = routes[i];
				num += route.PortalIDs.Length;
			}
		}
		NativeArray_1 = new NativeArray<int>(num, Allocator.Persistent);
		int num2 = 0;
		foreach (KeyValuePair<uint, RoomPair> item in RoomPairsByID)
		{
			RoomPair value = item.Value;
			int num3 = 0;
			RoomPair.Route[] routes = value.Routes;
			for (int i = 0; i < routes.Length; i++)
			{
				RoomPair.Route route2 = routes[i];
				num3 += route2.PortalIDs.Length;
			}
			Dictionary_2[item.Key] = new Struct207
			{
				startIndex = num2,
				endExclusive = num2 + num3
			};
			routes = value.Routes;
			for (int i = 0; i < routes.Length; i++)
			{
				short[] portalIDs = routes[i].PortalIDs;
				foreach (short portalID in portalIDs)
				{
					int index = Gclass1124_0.GetIndex(portalID);
					NativeArray_1[num2++] = index;
				}
			}
		}
	}

	public bool TryGetPortalIndicesData(uint roomPairID, out NativeArray<int> data)
	{
		if (Dictionary_2.TryGetValue(roomPairID, out var value))
		{
			data = NativeArray_1.GetSubArray(value.startIndex, value.Count);
			return true;
		}
		data = default(NativeArray<int>);
		return false;
	}

	public void SwapPortalBuffers()
	{
		Gclass1124_0.SwapBuffers();
		GlobalEventHandlerClass.CreateEvent<GClass3575>().Invoke();
	}

	public void Clear()
	{
		PortalsByID?.Clear();
		RoomPairsByID?.Clear();
		RelevantRoomPairsByRoomID?.Clear();
		if (Dictionary_3 != null)
		{
			Dictionary_3.Clear();
			Dictionary_3 = null;
		}
		if (NativeArray_0.IsCreated)
		{
			NativeArray_0.Dispose();
		}
		List_0?.Clear();
		Dictionary_0?.Clear();
		if (NativeArray_1.IsCreated)
		{
			NativeArray_1.Dispose();
		}
		Dictionary_2?.Clear();
		Gclass1124_0?.Clear();
	}

	public void BuildRoomPairIndex()
	{
		Dictionary_3 = new Dictionary<(short, short), RoomPair>();
		if (RelevantRoomPairsByRoomID == null)
		{
			return;
		}
		foreach (KeyValuePair<short, RoomPair[]> item in RelevantRoomPairsByRoomID)
		{
			RoomPair[] value = item.Value;
			if (value != null && value.Length != 0)
			{
				RoomPair[] array = value;
				foreach (RoomPair roomPair in array)
				{
					if (roomPair == null)
					{
						GClass722.Instance.LogError($"Can't find room pair with ID {item.Key}");
						continue;
					}
					short firstRoomID = roomPair.FirstRoomID;
					short secondRoomID = roomPair.SecondRoomID;
					(short, short) key = ((firstRoomID < secondRoomID) ? (firstRoomID, secondRoomID) : (secondRoomID, firstRoomID));
					Dictionary_3.TryAdd(key, roomPair);
				}
			}
			else
			{
				GClass722.Instance.LogError($"Can't build room pair index ID: {item.Key}");
			}
		}
	}

	public void Dispose()
	{
		if (!Bool_0)
		{
			Bool_0 = true;
			Clear();
			Gclass1124_0.Dispose();
		}
	}
}
