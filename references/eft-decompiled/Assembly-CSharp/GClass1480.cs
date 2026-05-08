using System;

public abstract class GClass1480
{
	public const int ChecksumSize = 4;

	[NonSerialized]
	public const uint Uint_0 = 2197175160u;

	[NonSerialized]
	public static uint[] Uint_1;

	static GClass1480()
	{
		Uint_1 = new uint[4096];
		for (uint num = 0u; num < 256; num++)
		{
			uint num2 = num;
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					num2 = (((num2 & 1) == 1) ? (0x82F63B78u ^ (num2 >> 1)) : (num2 >> 1));
				}
				Uint_1[i * 256 + num] = num2;
			}
		}
	}

	public static uint Compute(byte[] input, int offset, int length)
	{
		uint num = uint.MaxValue;
		while (length >= 16)
		{
			uint num2 = Uint_1[768 + input[offset + 12]] ^ Uint_1[512 + input[offset + 13]] ^ Uint_1[256 + input[offset + 14]] ^ Uint_1[input[offset + 15]];
			uint num3 = Uint_1[1792 + input[offset + 8]] ^ Uint_1[1536 + input[offset + 9]] ^ Uint_1[1280 + input[offset + 10]] ^ Uint_1[1024 + input[offset + 11]];
			uint num4 = Uint_1[2816 + input[offset + 4]] ^ Uint_1[2560 + input[offset + 5]] ^ Uint_1[2304 + input[offset + 6]] ^ Uint_1[2048 + input[offset + 7]];
			num = Uint_1[3840 + ((byte)num ^ input[offset])] ^ Uint_1[3584 + ((byte)(num >> 8) ^ input[offset + 1])] ^ Uint_1[3328 + ((byte)(num >> 16) ^ input[offset + 2])] ^ Uint_1[3072 + ((num >> 24) ^ input[offset + 3])] ^ num4 ^ num3 ^ num2;
			offset += 16;
			length -= 16;
		}
		while (--length >= 0)
		{
			num = Uint_1[(byte)(num ^ input[offset++])] ^ (num >> 8);
		}
		return num ^ 0xFFFFFFFFu;
	}
}
