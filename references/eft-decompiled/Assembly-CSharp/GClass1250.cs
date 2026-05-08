using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using ComponentAce.Compression.Libs.zlib;
using Koenigz.PerfectCulling;
using Koenigz.PerfectCulling.EFT;
using UnityEngine;

public class GClass1250 : IDisposable
{
	public class GClass1251 : IDisposable
	{
		[NonSerialized]
		public const int Int_0 = 262140;

		[NonSerialized]
		public static ArrayPool<byte> ArrayPool_0 = ArrayPool<byte>.Shared;

		[NonSerialized]
		public GClass1250 Gclass1250_0;

		[NonSerialized]
		public byte[] Byte_0;

		[NonSerialized]
		public GClass1231 Gclass1231_0;

		[NonSerialized]
		public global::VisibilityIndicesClass<ushort> Gclass1222_0;

		[NonSerialized]
		public ZStream Zstream_0;

		[NonSerialized]
		public byte[] Byte_1;

		[NonSerialized]
		public byte[] Byte_2;

		[NonSerialized]
		public int Int_1;

		public GClass1251(GClass1250 packedCullingGridData)
		{
			Gclass1231_0 = new GClass1231();
			Gclass1250_0 = packedCullingGridData;
			Byte_1 = Gclass1250_0.Byte_0;
			Byte_0 = ArrayPool_0.Rent(262140);
			Gclass1222_0 = GClass1245.AllocateCellArray();
			Int_1 = Gclass1250_0.List_0.Count;
			Byte_2 = ArrayPool_0.Rent(Int_1);
			Zstream_0 = new ZStream();
			Zstream_0.inflateInit();
		}

		public int DecompressCullingCell(int cellIndex, int targetGroupId, global::VisibilityIndicesClass<ushort> outputArray)
		{
			int next_in_index = Gclass1250_0.method_5(cellIndex);
			int num = Gclass1250_0.method_3(cellIndex);
			if (num == 0)
			{
				return 0;
			}
			Zstream_0.inflateReset();
			Zstream_0.next_in = Byte_1;
			Zstream_0.next_in_index = next_in_index;
			Zstream_0.avail_in = num;
			Zstream_0.avail_out = Byte_0.Length;
			Zstream_0.next_out = Byte_0;
			Zstream_0.next_out_index = 0;
			Zstream_0.inflate(0);
			int num2 = Byte_0[0];
			int num3 = 1;
			int num4 = 0;
			ushort num5;
			while (true)
			{
				if (num4 < num2)
				{
					byte b = Byte_0[num3];
					num3++;
					num5 = BitConverter.ToUInt16(Byte_0, num3);
					num3 += 2;
					if (num5 > 0)
					{
						ushort num6 = BitConverter.ToUInt16(Byte_0, num3);
						num3 += 2;
						if (targetGroupId == b)
						{
							break;
						}
						num3 += num6;
					}
					num4++;
					continue;
				}
				return 0;
			}
			method_0(Byte_0, num3, num5, outputArray);
			return num5;
		}

		public void DecompressCullingCell(int cellIndex, GDelegate40 callbackPerCullingGroupInCell)
		{
			int next_in_index = Gclass1250_0.method_5(cellIndex);
			int num = Gclass1250_0.method_3(cellIndex);
			if (num == 0)
			{
				return;
			}
			Zstream_0.inflateReset();
			Zstream_0.next_in = Byte_1;
			Zstream_0.next_in_index = next_in_index;
			Zstream_0.avail_in = num;
			Zstream_0.avail_out = Byte_0.Length;
			Zstream_0.next_out = Byte_0;
			Zstream_0.next_out_index = 0;
			Zstream_0.inflate(0);
			int num2 = Byte_0[0];
			int num3 = 1;
			for (int i = 0; i < Int_1; i++)
			{
				Byte_2[i] = 0;
			}
			for (int j = 0; j < num2; j++)
			{
				byte b = Byte_0[num3];
				num3++;
				ushort num4 = BitConverter.ToUInt16(Byte_0, num3);
				num3 += 2;
				if (num4 > 0)
				{
					Byte_2[b] = 1;
					ushort num5 = BitConverter.ToUInt16(Byte_0, num3);
					num3 += 2;
					method_0(Byte_0, num3, num4, Gclass1222_0);
					callbackPerCullingGroupInCell(new GStruct113(b, Gclass1222_0));
					num3 += num5;
				}
			}
			for (int k = 0; k < Int_1; k++)
			{
				if (Byte_2[k] == 0)
				{
					callbackPerCullingGroupInCell(new GStruct113(k, null));
				}
			}
		}

		public int method_0(byte[] compressedIndices, int readOffset, int numIndices, global::VisibilityIndicesClass<ushort> outIndices)
		{
			outIndices.Clear();
			Gclass1231_0.Reset(compressedIndices);
			Gclass1231_0.SetOffset(readOffset);
			ushort num = 0;
			for (int i = 0; i < numIndices; i++)
			{
				uint num2 = Gclass1231_0.Read(2);
				uint bits = PerfectCullingVolumeBakeData.BITS[num2];
				uint num3 = Gclass1231_0.Read((int)bits);
				num += (ushort)num3;
				if (num3 < 15)
				{
					if (outIndices.Count >= outIndices.Buffer.Length)
					{
						break;
					}
					outIndices.Add(num);
				}
			}
			return Gclass1231_0.Position;
		}

		public void Dispose()
		{
			ArrayPool_0.Return(Byte_2);
			Byte_2 = null;
			Zstream_0.free();
			Zstream_0 = null;
			GClass1245.ReuseCellArray(Gclass1222_0);
			Gclass1222_0 = null;
			ArrayPool_0.Return(Byte_0);
			Byte_0 = null;
			Gclass1231_0 = null;
		}
	}

	public readonly struct GStruct112
	{
		public readonly Vector3 center;

		public readonly Vector3 size;

		public readonly Quaternion rotation;

		public override string ToString()
		{
			return $"{center} | {size} | {rotation}";
		}
	}

	public const string PACKED_FILE_EXT = "_packed_cull.bytes";

	[NonSerialized]
	public GClass1231 Gclass1231_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public byte[] Byte_0;

	[NonSerialized]
	public ReadOnlyMemory<byte> ReadOnlyMemory_0;

	[NonSerialized]
	public MemoryHandle MemoryHandle_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public ReadOnlyMemory<int> ReadOnlyMemory_1;

	[NonSerialized]
	public List<GuidReference> List_0;

	public int NumCells => Int_0;

	public List<GuidReference> GroupMapping => List_0;

	public bool IsValid => Byte_0 != null;

	public MemoryHandle Handle => MemoryHandle_0;

	public int GetGroupIndex(GuidReference groupGuid)
	{
		int num = 0;
		while (true)
		{
			if (num < GroupMapping.Count)
			{
				if (GroupMapping[num] == groupGuid)
				{
					break;
				}
				num++;
				continue;
			}
			return -1;
		}
		return num;
	}

	public GClass1251 AllocateDecompressor()
	{
		return new GClass1251(this);
	}

	public void LoadSync(string filePath)
	{
		Byte_0 = File.ReadAllBytes(filePath);
		if (Byte_0 != null)
		{
			GClass712.Debug($"Culling data loaded, size={Byte_0.Length}");
		}
		else
		{
			GClass712.Error("Culling data file failed to load " + filePath + ". Probably it was not exported properly.");
		}
	}

	public void PostLoadInitialize()
	{
		try
		{
			Gclass1231_0 = new GClass1231();
			ReadOnlyMemory_0 = new ReadOnlyMemory<byte>(Byte_0);
			MemoryHandle_0 = ReadOnlyMemory_0.Pin();
			Int_0 = method_2();
			Int_1 = Byte_0.Length - (4 + 4 * Int_0);
			method_0();
		}
		catch (Exception ex)
		{
			throw new Exception(ex.Message + "\n" + ex.StackTrace);
		}
	}

	public void method_0()
	{
		List_0 = new List<GuidReference>();
		int num = method_1();
		int num2 = 4;
		for (int i = 0; i < num; i++)
		{
			int num3 = BitConverter.ToInt32(Byte_0, num2);
			num2 += 4;
			GuidReference item = new GuidReference(new ReadOnlySpan<byte>(Byte_0, num2, num3).ToArray());
			List_0.Add(item);
			num2 += num3;
		}
	}

	public List<OrientedBounds> GetCellsBounds()
	{
		List<OrientedBounds> list = new List<OrientedBounds>();
		for (int i = 0; i < Int_0; i++)
		{
			GStruct112 cellBounds = GetCellBounds(i);
			list.Add(new OrientedBounds
			{
				bounds = new Bounds(cellBounds.center, cellBounds.size),
				rotation = cellBounds.rotation
			});
		}
		return list;
	}

	public unsafe int method_1()
	{
		return *(int*)MemoryHandle_0.Pointer;
	}

	public unsafe int method_2()
	{
		return *((int*)((byte*)MemoryHandle_0.Pointer + Byte_0.Length) - 1);
	}

	public unsafe int method_3(int cellIndex)
	{
		return *(int*)((byte*)MemoryHandle_0.Pointer + method_4(cellIndex) + sizeof(GStruct112));
	}

	public unsafe int method_4(int cellIndex)
	{
		return *(int*)((byte*)MemoryHandle_0.Pointer + Int_1 + 4 * cellIndex);
	}

	public unsafe int method_5(int cellIndex)
	{
		return method_4(cellIndex) + sizeof(GStruct112) + 4;
	}

	public unsafe GStruct112 GetCellBounds(int cellIndex)
	{
		return *(GStruct112*)((byte*)MemoryHandle_0.Pointer + method_4(cellIndex));
	}

	public static string GetPackedFilePathForGrid(PerfectCullingAdaptiveGrid grid)
	{
		return Path.Combine(Application.streamingAssetsPath, "Culling_Data", grid.GridHash + "_packed_cull.bytes");
	}

	public void Dispose()
	{
		MemoryHandle_0.Dispose();
		Byte_0 = null;
		Gclass1231_0 = null;
	}
}
