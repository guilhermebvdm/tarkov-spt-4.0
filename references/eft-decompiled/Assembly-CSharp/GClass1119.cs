using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Audio.SpatialSystem;
using Audio.SpatialSystem.Data;

public class GClass1119
{
	[NonSerialized]
	public const short Short_0 = 300;

	[NonSerialized]
	public int Int_0;

	public bool Boolean_0 => Int_0 % 300 == 0;

	public IEnumerator ReadAndWriteToDataContainerCoroutine(string pathToLoad, GClass1138 container, CancellationToken cancellationToken, Action onDone = null, Action<Exception> onError = null)
	{
		if (!File.Exists(pathToLoad))
		{
			GClass722.Instance.LogError("The file does not exist. " + pathToLoad);
		}
		else if (container != null && !container.IsDisposed)
		{
			HashSet<uint> hashSet = new HashSet<uint>();
			using (FileStream input = new FileStream(pathToLoad, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using BinaryReader binaryReader = new BinaryReader(input);
				if (cancellationToken.IsCancellationRequested)
				{
					yield break;
				}
				yield return method_0(binaryReader, container, cancellationToken);
				if (container.IsDisposed || cancellationToken.IsCancellationRequested)
				{
					yield break;
				}
				yield return method_1(binaryReader, container, cancellationToken, hashSet);
				if (container.IsDisposed || cancellationToken.IsCancellationRequested)
				{
					yield break;
				}
				yield return method_2(binaryReader, container, cancellationToken);
				if (container.IsDisposed || cancellationToken.IsCancellationRequested)
				{
					yield break;
				}
				container.bakeSettings = new LocationBakeSettings();
				container.bakeSettings.Read(binaryReader);
				container.roomPairMaxRoutesCapacity = binaryReader.ReadUInt32();
				container.roomPairMaxPortalsCapacity = binaryReader.ReadUInt32();
				container.FinalizeRoutesData();
				yield return null;
				if (container.IsDisposed || cancellationToken.IsCancellationRequested)
				{
					yield break;
				}
				container.FinalizePortalIndicesData();
			}
			if (hashSet.Count > 0)
			{
				string arg = string.Join(",", hashSet.Take(10));
				GClass722.Instance.LogError($"Missing {hashSet.Count} RoomPair entries while reading bake data. Examples: {arg}");
			}
			onDone?.Invoke();
		}
		else
		{
			GClass722.Instance.LogWarn("ReadAndWriteToDataContainerCoroutine: container is null or disposed, aborting read.");
		}
	}

	public IEnumerator method_0(BinaryReader reader, GClass1138 container, CancellationToken cancellationToken)
	{
		Int_0 = 0;
		if (container == null || container.IsDisposed || cancellationToken.IsCancellationRequested)
		{
			yield break;
		}
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}
			if (container.IsDisposed)
			{
				break;
			}
			uint key = reader.ReadUInt32();
			RoomPair roomPair = new RoomPair();
			roomPair.Read(reader);
			container.RoomPairsByID[key] = roomPair;
			Int_0++;
			if (Boolean_0)
			{
				yield return null;
			}
		}
	}

	public IEnumerator method_1(BinaryReader reader, GClass1138 container, CancellationToken cancellationToken, HashSet<uint> missingRoomPairIds)
	{
		Int_0 = 0;
		if (container == null || container.IsDisposed || cancellationToken.IsCancellationRequested)
		{
			yield break;
		}
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			if (container.IsDisposed)
			{
				break;
			}
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}
			short key = reader.ReadInt16();
			int num2 = reader.ReadInt32();
			container.RelevantRoomPairsByRoomID[key] = new RoomPair[num2];
			for (int j = 0; j < num2; j++)
			{
				if (!container.IsDisposed && !cancellationToken.IsCancellationRequested)
				{
					uint num3 = reader.ReadUInt32();
					if (container.RoomPairsByID.TryGetValue(num3, out var value))
					{
						container.RelevantRoomPairsByRoomID[key][j] = value;
					}
					else
					{
						missingRoomPairIds.Add(num3);
						container.RelevantRoomPairsByRoomID[key][j] = new RoomPair();
					}
					Int_0++;
					if (Boolean_0)
					{
						yield return null;
					}
					continue;
				}
				yield break;
			}
		}
	}

	public IEnumerator method_2(BinaryReader reader, GClass1138 container, CancellationToken cancellationToken)
	{
		Int_0 = 0;
		if (container == null || container.IsDisposed || cancellationToken.IsCancellationRequested)
		{
			yield break;
		}
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			if (container.IsDisposed)
			{
				break;
			}
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}
			uint roomPairID = reader.ReadUInt32();
			AudioRouteData[] data = method_3(reader);
			if (!container.IsDisposed && !cancellationToken.IsCancellationRequested)
			{
				container.AddRoutesData(roomPairID, data);
				Int_0++;
				if (Boolean_0)
				{
					yield return null;
				}
				continue;
			}
			break;
		}
	}

	public AudioRouteData[] method_3(BinaryReader reader)
	{
		int num = reader.ReadInt32();
		AudioRouteData[] array = new AudioRouteData[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = default(AudioRouteData);
			array[i].Read(reader);
		}
		return array;
	}
}
