using System;
using System.Collections.Generic;
using Audio.SpatialSystem;
using Unity.Collections;

public class GClass1124 : IDisposable
{
	public struct Struct203
	{
		public short portalID;

		public RoomPair.AudioPortalData data;
	}

	[NonSerialized]
	public NativeList<RoomPair.AudioPortalData> NativeList_0;

	[NonSerialized]
	public NativeList<RoomPair.AudioPortalData> NativeList_1;

	[NonSerialized]
	public Dictionary<short, int> Dictionary_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public List<Struct203> List_0 = new List<Struct203>();

	[NonSerialized]
	public List<Struct203> List_1 = new List<Struct203>();

	public bool HasPendingUpdates
	{
		get
		{
			if (List_0.Count <= 0)
			{
				return List_1.Count > 0;
			}
			return true;
		}
	}

	public GClass1124(int initialCapacity = 0)
	{
		NativeList_0 = new NativeList<RoomPair.AudioPortalData>(initialCapacity, Allocator.Persistent);
		NativeList_1 = new NativeList<RoomPair.AudioPortalData>(initialCapacity, Allocator.Persistent);
		Dictionary_0 = new Dictionary<short, int>(initialCapacity);
		Bool_0 = true;
	}

	public void AddPortal(short portalID, RoomPair.AudioPortalData data)
	{
		if (!Dictionary_0.ContainsKey(portalID))
		{
			int length = NativeList_0.Length;
			NativeList_0.Add(in data);
			NativeList_1.Add(in data);
			Dictionary_0[portalID] = length;
		}
	}

	public void UpdatePortalData(short portalID, RoomPair.AudioPortalData data)
	{
		Struct203 item = new Struct203
		{
			portalID = portalID,
			data = data
		};
		List_0.Add(item);
		List_1.Add(item);
	}

	public NativeList<RoomPair.AudioPortalData> GetActiveList()
	{
		if (!Bool_0)
		{
			return NativeList_1;
		}
		return NativeList_0;
	}

	public void SwapBuffers()
	{
		NativeList<RoomPair.AudioPortalData> nativeList = (Bool_0 ? NativeList_1 : NativeList_0);
		List<Struct203> list = (Bool_0 ? List_1 : List_0);
		foreach (Struct203 item in list)
		{
			if (Dictionary_0.TryGetValue(item.portalID, out var value))
			{
				nativeList[value] = item.data;
			}
		}
		list.Clear();
		Bool_0 = !Bool_0;
	}

	public void Clear()
	{
		if (NativeList_0.IsCreated)
		{
			NativeList_0.Clear();
		}
		if (NativeList_1.IsCreated)
		{
			NativeList_1.Clear();
		}
		Dictionary_0.Clear();
		List_0.Clear();
		List_1.Clear();
		Bool_0 = true;
	}

	public void Dispose()
	{
		if (NativeList_0.IsCreated)
		{
			NativeList_0.Dispose();
		}
		if (NativeList_1.IsCreated)
		{
			NativeList_1.Dispose();
		}
	}

	public int GetIndex(short portalID)
	{
		return Dictionary_0.GetValueOrDefault(portalID, -1);
	}
}
