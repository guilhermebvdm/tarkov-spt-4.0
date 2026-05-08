using System;
using System.Collections.Generic;
using BitPacking;
using EFT;

public struct GStruct22
{
	public const int OFFSET = 1;

	[NonSerialized]
	public static int Int_0 = 127;

	public int wildTypesCount;

	public int ZonesCount;

	public string[] Names;

	public int[][] Blocks;

	[NonSerialized]
	public static int Int_1 = 123;

	public int Count
	{
		get
		{
			if (Names == null)
			{
				return 0;
			}
			return Names.Length;
		}
	}

	public GStruct22(Dictionary<BotZone, HashSet<WildSpawnType>> data)
	{
		Array values = Enum.GetValues(typeof(WildSpawnType));
		wildTypesCount = values.Length;
		ZonesCount = data.Count;
		Names = new string[ZonesCount];
		Blocks = new int[ZonesCount][];
		for (int i = 0; i < ZonesCount; i++)
		{
			Blocks[i] = new int[wildTypesCount];
			for (int j = 0; j < wildTypesCount; j++)
			{
				Blocks[i][j] = 60;
			}
		}
		int num = 0;
		foreach (KeyValuePair<BotZone, HashSet<WildSpawnType>> datum in data)
		{
			Names[num] = datum.Key.ShortName;
			int num2 = 0;
			foreach (WildSpawnType item in datum.Value)
			{
				Blocks[num][num2] = (int)(1 + item);
				num2++;
			}
			num++;
		}
	}

	public static void Serialize(GStruct22 data, GInterface131 stream)
	{
		stream.WriteLimitedInt32(data.ZonesCount, 0, 63, BitPackingTag.DebugZoneInfoStructZonesCount);
		stream.WriteLimitedInt32(data.wildTypesCount, 0, Int_1, BitPackingTag.DebugZoneInfoStructWildTypesCount);
		for (int i = 0; i < data.Count && i < data.Names.Length; i++)
		{
			string value = data.Names[i];
			stream.WriteLimitedString(value, ' ', 'z', BitPackingTag.DebugZoneInfoStructZoneName, 1600u);
			for (int j = 0; j < data.wildTypesCount; j++)
			{
				int value2 = data.Blocks[i][j];
				stream.WriteLimitedInt32(value2, 0, 255, BitPackingTag.DebugZoneInfoBlocksZones);
			}
		}
	}

	public static GStruct22 Deserialize(IDataReader stream)
	{
		GStruct22 result = default(GStruct22);
		result.ZonesCount = stream.ReadLimitedInt32(0, 63);
		result.wildTypesCount = stream.ReadLimitedInt32(0, Int_1);
		result.Blocks = new int[result.ZonesCount][];
		result.Names = new string[result.ZonesCount];
		try
		{
			for (int i = 0; i < result.ZonesCount; i++)
			{
				result.Names[i] = stream.ReadLimitedString(' ', 'z', 1600u);
				result.Blocks[i] = new int[result.wildTypesCount];
				for (int j = 0; j < result.wildTypesCount; j++)
				{
					int num = stream.ReadLimitedInt32(0, 255);
					result.Blocks[i][j] = num;
				}
			}
		}
		catch (Exception)
		{
		}
		return result;
	}
}
