using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Koenigz.PerfectCulling;
using Koenigz.PerfectCulling.EFT;

public abstract class GClass1249
{
	public static GClass1252 Compress(this CullingGroupData data)
	{
		if (data.Indices != null && data.Indices.Length != 0)
		{
			List<int> list = new List<int>();
			ushort[] indices = data.Indices;
			Array.Sort(indices);
			smethod_0(indices[0], list);
			for (int i = 1; i < indices.Length; i++)
			{
				smethod_0(indices[i] - indices[i - 1], list);
			}
			GClass1232 gClass = new GClass1232();
			gClass.Reset();
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
					gClass.Write((uint)num2, 2);
					gClass.Write((uint)list[num], (int)PerfectCullingVolumeBakeData.BITS[num2]);
					num++;
					continue;
				}
				gClass.Flush();
				byte[] array = new byte[gClass.Length];
				Array.Copy(gClass.Buffer, array, gClass.Length);
				return new GClass1252
				{
					numIndices = (ushort)list.Count,
					compressedIndexArray = array
				};
			}
			throw new Exception("Hmm");
		}
		return new GClass1252
		{
			numIndices = 0,
			compressedIndexArray = null
		};
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
