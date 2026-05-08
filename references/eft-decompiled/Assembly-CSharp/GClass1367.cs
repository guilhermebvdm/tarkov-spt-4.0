using System;

public class GClass1367 : IMeasureStatistics
{
	[NonSerialized]
	public unsafe uint* PUint_0;

	[NonSerialized]
	public byte[] Byte_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public ulong Ulong_0;

	[NonSerialized]
	public int Int_2;

	[NonSerialized]
	public int Int_3;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public int Int_4;

	public byte[] Buffer => Byte_0;

	public bool IsOverflow => Bool_0;

	public int BitsWritten => Int_4;

	public int BitsCount => Int_1;

	public int BytesWritten => Int_3 * 4;

	public int TotalBytes => Int_0 * 4;

	public unsafe GClass1367(byte[] bufferBytes)
	{
		Byte_0 = bufferBytes;
		int num = Byte_0.Length;
		fixed (byte* pUint_ = &Byte_0[0])
		{
			PUint_0 = (uint*)pUint_;
		}
		Int_0 = num / 4;
		Int_1 = Int_0 * 32;
		Int_4 = 0;
		Ulong_0 = 0uL;
		Int_2 = 0;
		Int_3 = 0;
		Bool_0 = false;
	}

	public unsafe void WriteBits(uint value, int bits)
	{
		if (Int_4 + bits > Int_1)
		{
			Bool_0 = true;
			return;
		}
		value &= (uint)(int)((1L << bits) - 1L);
		Ulong_0 |= (ulong)value << 64 - Int_2 - bits;
		Int_2 += bits;
		if (Int_2 >= 32)
		{
			PUint_0[Int_3] = (uint)(Ulong_0 >> 32);
			Ulong_0 <<= 32;
			Int_2 -= 32;
			Int_3++;
		}
		Int_4 += bits;
	}

	public void WriteBytes(byte[] bytes, int startIndex, int length)
	{
		if (Int_4 + length * 8 > Int_1)
		{
			Bool_0 = true;
			return;
		}
		int num = (4 - Int_2 / 8) % 4;
		if (num > length)
		{
			num = length;
		}
		for (int i = 0; i < num; i++)
		{
			WriteBits(bytes[startIndex + i], 8);
		}
		if (num != length)
		{
			int num2 = (length - num) / 4;
			if (num2 > 0)
			{
				Array.Copy(bytes, startIndex + num, Byte_0, Int_3 * 4, num2 * 4);
				Int_4 += num2 * 32;
				Int_3 += num2;
				Ulong_0 = 0uL;
			}
			int num3 = num + num2 * 4;
			int num4 = length - num3;
			for (int j = 0; j < num4; j++)
			{
				WriteBits(bytes[startIndex + num3 + j], 8);
			}
		}
	}

	public void WriteBytes(byte[] bytes)
	{
		WriteBytes(bytes, 0, bytes.Length);
	}

	public void WriteByteAlign()
	{
		int num = Int_4 % 8;
		if (num != 0)
		{
			WriteBits(0u, 8 - num);
		}
	}

	public void Reset()
	{
		Int_4 = 0;
		Ulong_0 = 0uL;
		Int_2 = 0;
		Int_3 = 0;
		Bool_0 = false;
	}

	public unsafe void FlushBits()
	{
		if (Int_2 != 0)
		{
			if (Int_3 >= Int_0)
			{
				Bool_0 = true;
			}
			else
			{
				PUint_0[Int_3++] = (uint)(Ulong_0 >> 32);
			}
		}
	}

	public int method_0()
	{
		return (8 - Int_4 % 8) % 8;
	}
}
