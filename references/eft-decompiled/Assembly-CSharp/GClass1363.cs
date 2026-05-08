using System;

public class GClass1363 : GInterface127
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
	public int Int_2;

	[NonSerialized]
	public ulong Ulong_0;

	[NonSerialized]
	public int Int_3;

	[NonSerialized]
	public int Int_4;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	public byte[] Buffer => Byte_0;

	public bool IsOverflow => Bool_0;

	public int BitsRead => Int_2;

	public int BitsCount => Int_1;

	public int BytesRead => (Int_4 + ((Int_3 > 0) ? 1 : 0)) * 4;

	public int TotalBytes => Int_1 * 8;

	public unsafe GClass1363(byte[] bufferBytes)
	{
		Byte_0 = bufferBytes;
		int num = Byte_0.Length;
		fixed (byte* pUint_ = &Byte_0[0])
		{
			PUint_0 = (uint*)pUint_;
		}
		Int_0 = num / 4;
		Int_1 = Int_0 * 32;
		Int_2 = 0;
		Int_3 = 0;
		Int_4 = 0;
		Bool_1 = true;
		Bool_0 = false;
	}

	public unsafe uint ReadBits(int bits)
	{
		if (Bool_1)
		{
			Ulong_0 = *PUint_0;
			Bool_1 = false;
		}
		if (Int_2 + bits > Int_1)
		{
			Bool_0 = true;
			return 0u;
		}
		Int_2 += bits;
		if (Int_3 + bits < 32)
		{
			Ulong_0 <<= bits;
			Int_3 += bits;
		}
		else
		{
			Int_4++;
			int num = 32 - Int_3;
			int num2 = bits - num;
			Ulong_0 <<= num;
			Ulong_0 |= PUint_0[Int_4];
			Ulong_0 <<= num2;
			Int_3 = num2;
		}
		int result = (int)(Ulong_0 >> 32);
		Ulong_0 &= 4294967295uL;
		return (uint)result;
	}

	public unsafe void ReadBytes(byte[] destination, int destinationStartIndex, int bytesCount)
	{
		if (Bool_1)
		{
			Ulong_0 = *PUint_0;
			Bool_1 = false;
		}
		if (Int_2 + bytesCount * 8 > Int_1)
		{
			Array.Clear(destination, destinationStartIndex, bytesCount);
			Bool_0 = true;
			return;
		}
		int num = (4 - Int_3 / 8) % 4;
		if (num > bytesCount)
		{
			num = bytesCount;
		}
		for (int i = 0; i < num; i++)
		{
			destination[destinationStartIndex + i] = (byte)ReadBits(8);
		}
		if (num != bytesCount)
		{
			int num2 = (bytesCount - num) / 4;
			if (num2 > 0)
			{
				int length = num2 * 4;
				int sourceIndex = Int_4 * 4;
				Array.Copy(Byte_0, sourceIndex, destination, destinationStartIndex + num, length);
				Int_2 += num2 * 32;
				Int_4 += num2;
				Ulong_0 = PUint_0[Int_4];
			}
			int num3 = num + num2 * 4;
			int num4 = bytesCount - num3;
			for (int j = 0; j < num4; j++)
			{
				destination[destinationStartIndex + num3 + j] = (byte)ReadBits(8);
			}
		}
	}

	public int GetAlignBits()
	{
		return (8 - Int_2 % 8) % 8;
	}

	public void ReadAlign()
	{
		int num = Int_2 % 8;
		if (num != 0)
		{
			ReadBits(8 - num);
		}
	}

	public void Reset()
	{
		Int_2 = 0;
		Int_3 = 0;
		Int_4 = 0;
		Bool_1 = true;
		Bool_0 = false;
	}
}
