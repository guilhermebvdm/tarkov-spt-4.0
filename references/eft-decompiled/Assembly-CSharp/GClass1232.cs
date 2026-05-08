using System;
using System.Runtime.CompilerServices;

public class GClass1232
{
	[NonSerialized]
	public const uint Uint_0 = 1u;

	[NonSerialized]
	public byte[] Byte_0 = new byte[65536];

	[NonSerialized]
	public byte Byte_1;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public int Int_1;

	public byte[] Buffer => Byte_0;

	public int Length => Int_1;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Reset()
	{
		Byte_1 = 0;
		Int_0 = 0;
		Int_1 = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int Write(uint value, int bits)
	{
		int num = 0;
		for (int i = 0; i < bits; i++)
		{
			uint singleBit = (value >> i) & 1;
			method_0(singleBit);
			num++;
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Flush()
	{
		if (Int_0 != 0)
		{
			method_1();
		}
		Byte_1 = 0;
		Int_0 = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void method_0(uint singleBit)
	{
		if (Int_0 == 8)
		{
			method_1();
			Byte_1 = 0;
			Int_0 = 0;
		}
		if (singleBit != 0)
		{
			Byte_1 |= (byte)(singleBit << Int_0);
		}
		Int_0++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void method_1()
	{
		Byte_0[Int_1] = Byte_1;
		Int_1++;
	}
}
