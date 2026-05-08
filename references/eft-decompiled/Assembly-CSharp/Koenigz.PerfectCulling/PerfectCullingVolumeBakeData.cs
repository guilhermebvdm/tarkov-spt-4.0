using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Koenigz.PerfectCulling;

[CreateAssetMenu]
[PreferBinarySerialization]
public class PerfectCullingVolumeBakeData : PerfectCullingBakeData
{
	[Serializable]
	public struct VisibilitySet
	{
		public byte[] compressed;

		public ushort numDeltaValues;
	}

	[Serializable]
	public struct RawData
	{
		public ushort[] uncompressed;
	}

	[Serializable]
	[CompilerGenerated]
	public class Class835
	{
		public static readonly Class835 class835_0 = new Class835();

		public static Func<int, int> func_0;

		public int method_0(int val)
		{
			return val % 32;
		}
	}

	[CompilerGenerated]
	public class Class836
	{
		public PerfectCullingVolumeBakeData perfectCullingVolumeBakeData_0;

		public Vector3Int OptimizedCellSize;

		public Vector3Int optDim;

		public PerfectCullingVolumeBakeData tmpBakeData;

		public int processedElementCount;

		public Task method_0(IGrouping<int, int> groups)
		{
			return Task.Run(async delegate
			{
				HashSet<ushort> hashSet = new HashSet<ushort>();
				int num = 0;
				foreach (int group in groups)
				{
					GClass1225.UnflattenToXYZ(group, out var x, out var y, out var z, perfectCullingVolumeBakeData_0.cellCount);
					int x2 = x / OptimizedCellSize.x;
					int y2 = y / OptimizedCellSize.y;
					int z2 = z / OptimizedCellSize.z;
					int num2 = GClass1225.FlattenXYZDouble(x2, y2, z2, optDim);
					hashSet.Clear();
					for (int i = -1; i <= 1; i++)
					{
						for (int j = -1; j <= 1; j++)
						{
							for (int k = -1; k <= 1; k++)
							{
								if (GClass1225.IsXYZInBounds(x + i, y + j, z + k, perfectCullingVolumeBakeData_0.cellCount))
								{
									int num3 = GClass1225.FlattenXYZDouble(x + i, y + j, z + k, perfectCullingVolumeBakeData_0.cellCount);
									if (perfectCullingVolumeBakeData_0.rawData[num3].uncompressed != null)
									{
										ushort[] uncompressed = perfectCullingVolumeBakeData_0.rawData[num3].uncompressed;
										foreach (ushort item in uncompressed)
										{
											hashSet.Add(item);
										}
									}
								}
							}
						}
					}
					if (tmpBakeData.rawData[num2].uncompressed != null)
					{
						ushort[] uncompressed = tmpBakeData.rawData[num2].uncompressed;
						foreach (ushort item2 in uncompressed)
						{
							hashSet.Add(item2);
						}
					}
					tmpBakeData.rawData[num2].uncompressed = hashSet.ToArray();
					num++;
				}
				Interlocked.Add(ref processedElementCount, num);
			});
		}
	}

	[CompilerGenerated]
	public class Class837
	{
		public IGrouping<int, int> groups;

		public Class836 class836_0;

		public async Task method_0()
		{
			HashSet<ushort> hashSet = new HashSet<ushort>();
			int num = 0;
			foreach (int item3 in groups)
			{
				GClass1225.UnflattenToXYZ(item3, out var x, out var y, out var z, class836_0.perfectCullingVolumeBakeData_0.cellCount);
				int x2 = x / class836_0.OptimizedCellSize.x;
				int y2 = y / class836_0.OptimizedCellSize.y;
				int z2 = z / class836_0.OptimizedCellSize.z;
				int num2 = GClass1225.FlattenXYZDouble(x2, y2, z2, class836_0.optDim);
				hashSet.Clear();
				for (int i = -1; i <= 1; i++)
				{
					for (int j = -1; j <= 1; j++)
					{
						for (int k = -1; k <= 1; k++)
						{
							if (!GClass1225.IsXYZInBounds(x + i, y + j, z + k, class836_0.perfectCullingVolumeBakeData_0.cellCount))
							{
								continue;
							}
							int num3 = GClass1225.FlattenXYZDouble(x + i, y + j, z + k, class836_0.perfectCullingVolumeBakeData_0.cellCount);
							if (class836_0.perfectCullingVolumeBakeData_0.rawData[num3].uncompressed != null)
							{
								ushort[] uncompressed = class836_0.perfectCullingVolumeBakeData_0.rawData[num3].uncompressed;
								foreach (ushort item in uncompressed)
								{
									hashSet.Add(item);
								}
							}
						}
					}
				}
				if (class836_0.tmpBakeData.rawData[num2].uncompressed != null)
				{
					ushort[] uncompressed = class836_0.tmpBakeData.rawData[num2].uncompressed;
					foreach (ushort item2 in uncompressed)
					{
						hashSet.Add(item2);
					}
				}
				class836_0.tmpBakeData.rawData[num2].uncompressed = hashSet.ToArray();
				num++;
			}
			Interlocked.Add(ref class836_0.processedElementCount, num);
		}
	}

	public Vector3 cellCount;

	public Vector3 cellSize;

	public Vector3 originalCellSize;

	public Quaternion orientation;

	public VisibilitySet[] data;

	public RawData[] rawData;

	[NonSerialized]
	public int runtimeAssetSize = -1;

	public int maxStoredIndex = -1;

	public int numberOfGroups = -1;

	public string volumeGuid = "";

	public string groupGuid = "";

	public const int MaxValue = 15;

	public const int HeaderBitSize = 2;

	public static uint[] BITS = new uint[4] { 1u, 2u, 3u, 4u };

	private static GClass1231 gclass1231_0 = new GClass1231();

	private static GClass1232 gclass1232_0 = new GClass1232();

	public int RuntimeAssetSize
	{
		get
		{
			if (runtimeAssetSize == -1)
			{
				int num = 0;
				VisibilitySet[] array = data;
				for (int i = 0; i < array.Length; i++)
				{
					VisibilitySet visibilitySet = array[i];
					num += visibilitySet.compressed.Length;
					num += 2;
				}
				runtimeAssetSize = num;
			}
			return runtimeAssetSize;
		}
	}

	public void SetVolumeBakeData(Vector3 cellSize, Vector3 cellCount)
	{
		originalCellSize = cellSize;
		this.cellSize = cellSize;
		this.cellCount = cellCount;
	}

	public override void PrepareForBake(PerfectCullingBakingBehaviour bakingBehaviour)
	{
		orientation = bakingBehaviour.transform.rotation;
		data = new VisibilitySet[bakingBehaviour.GetSamplingPositions().Count];
		rawData = new RawData[data.Length];
		maxStoredIndex = -1;
		numberOfGroups = bakingBehaviour.bakeGroups.Length;
	}

	public override ushort[] GetRawData(int index)
	{
		if (rawData[index].uncompressed == null)
		{
			return Array.Empty<ushort>();
		}
		return rawData[index].uncompressed;
	}

	public override void SetRawData(int index, ushort[] indices)
	{
		if (indices != null && indices.Length != 0)
		{
			rawData[index] = new RawData
			{
				uncompressed = indices
			};
			ushort[] uncompressed = rawData[index].uncompressed;
			foreach (ushort a in uncompressed)
			{
				maxStoredIndex = Mathf.Max(a, maxStoredIndex);
			}
		}
		else
		{
			rawData[index] = new RawData
			{
				uncompressed = Array.Empty<ushort>()
			};
			maxStoredIndex = 0;
		}
	}

	public unsafe override void SetRawDataMT(int index, int* buffer, int count)
	{
		if (count == 0)
		{
			rawData[index] = new RawData
			{
				uncompressed = Array.Empty<ushort>()
			};
			maxStoredIndex = 0;
			return;
		}
		ushort[] array = new ushort[count];
		for (int i = 0; i < count; i++)
		{
			maxStoredIndex = Mathf.Max(array[i] = (ushort)buffer[i], maxStoredIndex);
		}
		rawData[index] = new RawData
		{
			uncompressed = array
		};
	}

	public override void SetRawData(int index, GClass1210 indices)
	{
		if (indices == null)
		{
			rawData[index] = new RawData
			{
				uncompressed = Array.Empty<ushort>()
			};
			maxStoredIndex = 0;
			return;
		}
		ushort[] bufferCopy = indices.BufferCopy;
		rawData[index] = new RawData
		{
			uncompressed = bufferCopy
		};
		ushort[] array = bufferCopy;
		foreach (ushort a in array)
		{
			maxStoredIndex = Mathf.Max(a, maxStoredIndex);
		}
	}

	public void method_0(int index, ushort[] indices)
	{
		if (indices.Length == 0)
		{
			return;
		}
		List<int> list = new List<int>();
		smethod_0(indices[0], list);
		for (int i = 1; i < indices.Length; i++)
		{
			smethod_0(indices[i] - indices[i - 1], list);
		}
		gclass1232_0.Reset();
		int num = 0;
		while (true)
		{
			if (num < list.Count)
			{
				int num2;
				if (list[num] <= 1)
				{
					num2 = 0;
				}
				else if (list[num] <= 3)
				{
					num2 = 1;
				}
				else if (list[num] <= 7)
				{
					num2 = 2;
				}
				else
				{
					if (list[num] > 15)
					{
						break;
					}
					num2 = 3;
				}
				gclass1232_0.Write((uint)num2, 2);
				gclass1232_0.Write((uint)list[num], (int)BITS[num2]);
				num++;
				continue;
			}
			gclass1232_0.Flush();
			byte[] array = new byte[gclass1232_0.Length];
			Array.Copy(gclass1232_0.Buffer, array, gclass1232_0.Length);
			data[index] = new VisibilitySet
			{
				compressed = array,
				numDeltaValues = (ushort)list.Count
			};
			List<ushort> list2 = new List<ushort>();
			SampleAtIndex(index, list2);
			if (indices.Length != list2.Count)
			{
				throw new Exception("Length mismatch: " + indices.Length + " vs " + list2.Count);
			}
			int num3 = 0;
			while (true)
			{
				if (num3 < indices.Length)
				{
					if (indices[num3] != list2[num3])
					{
						break;
					}
					num3++;
					continue;
				}
				return;
			}
			throw new Exception("Mismatch " + indices[num3] + " " + list2[num3] + " @ index " + index + " : " + num3 + " - Maybe the array was unsorted?");
		}
		throw new Exception("Hmm");
	}

	public override void CompleteBake()
	{
		if (rawData != null && rawData.Length != 0)
		{
			data = new VisibilitySet[rawData.Length];
			for (int i = 0; i < rawData.Length; i++)
			{
				GClass1227.ListUshort.Clear();
				GClass1227.ListUshort.AddRange(rawData[i].uncompressed);
				GClass1227.ListUshort.Sort();
				method_0(i, GClass1227.ListUshort.ToArray());
			}
			rawData = null;
		}
	}

	public void MergeDownsample()
	{
		if (cellCount.x == 1f && cellCount.y == 1f && cellCount.z == 1f)
		{
			Debug.LogWarning("Unable to downsample any further.");
			return;
		}
		PerfectCullingVolumeBakeData tmpBakeData = ScriptableObject.CreateInstance<PerfectCullingVolumeBakeData>();
		Vector3Int OptimizedCellSize = new Vector3Int(2, 2, 2);
		Vector3Int vector3Int = new Vector3Int((int)((cellCount.x % (float)OptimizedCellSize.x == 0f) ? cellCount.x : (cellCount.x + (float)OptimizedCellSize.x - cellCount.x % (float)OptimizedCellSize.x)), (int)((cellCount.y % (float)OptimizedCellSize.y == 0f) ? cellCount.y : (cellCount.y + (float)OptimizedCellSize.y - cellCount.y % (float)OptimizedCellSize.y)), (int)((cellCount.z % (float)OptimizedCellSize.z == 0f) ? cellCount.z : (cellCount.z + (float)OptimizedCellSize.z - cellCount.z % (float)OptimizedCellSize.z)));
		Vector3Int optDim = new Vector3Int(vector3Int.x / OptimizedCellSize.x, vector3Int.y / OptimizedCellSize.y, vector3Int.z / OptimizedCellSize.z);
		tmpBakeData.rawData = new RawData[optDim.x * optDim.y * optDim.z];
		tmpBakeData.cellCount = optDim;
		tmpBakeData.cellSize = new Vector3(cellSize.x * (float)OptimizedCellSize.x, cellSize.y * (float)OptimizedCellSize.y, cellSize.z * (float)OptimizedCellSize.z);
		int num = Mathf.CeilToInt(cellCount.x * cellCount.y * cellCount.z);
		IEnumerable<IGrouping<int, int>> source = from val in Enumerable.Range(0, num)
			group val by val % 32;
		int processedElementCount = 0;
		Task task = Task.WhenAll(source.Select((IGrouping<int, int> groups) => Task.Run(async delegate
		{
			HashSet<ushort> hashSet = new HashSet<ushort>();
			int num3 = 0;
			foreach (int group in groups)
			{
				GClass1225.UnflattenToXYZ(group, out var x, out var y, out var z, cellCount);
				int x2 = x / OptimizedCellSize.x;
				int y2 = y / OptimizedCellSize.y;
				int z2 = z / OptimizedCellSize.z;
				int num4 = GClass1225.FlattenXYZDouble(x2, y2, z2, optDim);
				hashSet.Clear();
				for (int i = -1; i <= 1; i++)
				{
					for (int j = -1; j <= 1; j++)
					{
						for (int k = -1; k <= 1; k++)
						{
							if (GClass1225.IsXYZInBounds(x + i, y + j, z + k, cellCount))
							{
								int num5 = GClass1225.FlattenXYZDouble(x + i, y + j, z + k, cellCount);
								if (rawData[num5].uncompressed != null)
								{
									ushort[] uncompressed = rawData[num5].uncompressed;
									foreach (ushort item in uncompressed)
									{
										hashSet.Add(item);
									}
								}
							}
						}
					}
				}
				if (tmpBakeData.rawData[num4].uncompressed != null)
				{
					ushort[] uncompressed = tmpBakeData.rawData[num4].uncompressed;
					foreach (ushort item2 in uncompressed)
					{
						hashSet.Add(item2);
					}
				}
				tmpBakeData.rawData[num4].uncompressed = hashSet.ToArray();
				num3++;
			}
			Interlocked.Add(ref processedElementCount, num3);
		})));
		TaskAwaiter awaiter = task.GetAwaiter();
		int num2 = 0;
		while (num2 != num && !task.Wait(100))
		{
			num2 = Interlocked.CompareExchange(ref processedElementCount, 0, 0);
		}
		awaiter.GetResult();
		rawData = tmpBakeData.rawData;
		cellCount = tmpBakeData.cellCount;
		cellSize = tmpBakeData.cellSize;
		UnityEngine.Object.DestroyImmediate(tmpBakeData);
	}

	public override void SampleAtIndex(int index, List<ushort> indices)
	{
		gclass1231_0.Reset(data[index].compressed);
		ushort num = 0;
		int numDeltaValues = data[index].numDeltaValues;
		for (int i = 0; i < numDeltaValues; i++)
		{
			uint num2 = gclass1231_0.Read(2);
			uint bits = BITS[num2];
			uint num3 = gclass1231_0.Read((int)bits);
			num += (ushort)num3;
			if (num3 < 15)
			{
				indices.Add(num);
			}
		}
	}

	public void SampleAtIndexFast(int index, global::VisibilityIndicesClass<ushort> indices)
	{
		gclass1231_0.Reset(data[index].compressed);
		ushort num = 0;
		int numDeltaValues = data[index].numDeltaValues;
		for (int i = 0; i < numDeltaValues; i++)
		{
			uint num2 = gclass1231_0.Read(2);
			uint bits = BITS[num2];
			uint num3 = gclass1231_0.Read((int)bits);
			num += (ushort)num3;
			if (num3 < 15)
			{
				indices.Add(num);
			}
		}
	}

	public override void DrawInspectorGUI()
	{
	}

	[CompilerGenerated]
	public static void smethod_0(int value, List<int> values)
	{
		while (value >= 15)
		{
			values.Add(15);
			value -= 15;
		}
		values.Add(value);
	}
}
