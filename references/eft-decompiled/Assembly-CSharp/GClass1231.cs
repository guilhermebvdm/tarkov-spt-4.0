using System;
using System.Runtime.CompilerServices;

public class GClass1231
{
	[NonSerialized]
	public const int Int_0 = 8;

	[NonSerialized]
	public byte[] Byte_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public int Int_2;

	public int Position => Int_2;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint Read(int bits)
	{
		uint num = 0u;
		switch (bits)
		{
		case 1:
			num |= method_0();
			break;
		case 2:
			num |= method_0() | (method_0() << 1);
			break;
		case 3:
			num |= method_0() | (method_0() << 1) | (method_0() << 2);
			break;
		case 4:
			num |= method_0() | (method_0() << 1) | (method_0() << 2) | (method_0() << 3);
			break;
		case 5:
			num |= method_0() | (method_0() << 1) | (method_0() << 2) | (method_0() << 3) | (method_0() << 4);
			break;
		case 6:
			num |= method_0() | (method_0() << 1) | (method_0() << 2) | (method_0() << 3) | (method_0() << 4) | (method_0() << 5);
			break;
		case 7:
			num |= method_0() | (method_0() << 1) | (method_0() << 2) | (method_0() << 3) | (method_0() << 4) | (method_0() << 5) | (method_0() << 6);
			break;
		case 8:
			num |= method_0() | (method_0() << 1) | (method_0() << 2) | (method_0() << 3) | (method_0() << 4) | (method_0() << 5) | (method_0() << 6) | (method_0() << 7);
			break;
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetOffset(int offset)
	{
		Int_2 = offset;
		Int_1 = 8;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Reset(byte[] buffer)
	{
		Byte_0 = buffer;
		Int_2 = 0;
		Int_1 = 8;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint method_0()
	{
		if (Int_1 == 0)
		{
			Int_2++;
			Int_1 = 7;
			return (uint)(Byte_0[Int_2] & 1);
		}
		int result = (Byte_0[Int_2] >> 8 - Int_1) & 1;
		Int_1--;
		return (uint)result;
	}
}
