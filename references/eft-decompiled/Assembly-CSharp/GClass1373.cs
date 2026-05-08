using System;
using BitPacking;
using UnityEngine;

public class GClass1373
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public BitPackingTag BitPackingTag_0;

	public GClass1373(float resolution, BitPackingTag tag)
	{
		Float_0 = resolution;
		BitPackingTag_0 = tag;
	}

	public void Write(GInterface131 writer, Quaternion quaternion)
	{
		quaternion.Normalize();
		int num = -1;
		float num2 = float.MinValue;
		for (int i = 0; i < 4; i++)
		{
			float num3 = Mathf.Abs(quaternion[i]);
			if (num3 > num2)
			{
				num2 = num3;
				num = i;
			}
		}
		writer.WriteLimitedInt32(num, 0, 3, BitPackingTag.QuaternionQuantizer);
		float num4 = ((!(quaternion[num] < 0f)) ? 1 : (-1));
		for (int j = 0; j < 4; j++)
		{
			if (j != num)
			{
				float value = quaternion[j] * num4;
				writer.WriteLimitedFloat(value, -1f, 1f, Float_0, BitPackingTag_0);
			}
		}
	}

	public void Read(IDataReader reader, out Quaternion quaternion)
	{
		quaternion = Quaternion.identity;
		int num = reader.ReadLimitedInt32(0, 3);
		float num2 = 0f;
		for (int i = 0; i < 4; i++)
		{
			if (i != num)
			{
				float num3 = (quaternion[i] = reader.ReadLimitedFloat(-1f, 1f, Float_0));
				num2 += num3 * num3;
			}
		}
		quaternion[num] = Mathf.Sqrt(1f - num2);
	}
}
