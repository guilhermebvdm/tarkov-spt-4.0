using System;

public abstract class GClass1515
{
	public static int Write(this byte[] buffer, byte value, int offset = 0)
	{
		buffer[offset] = value;
		return 1;
	}

	public static int Write(this byte[] buffer, ushort value, int offset = 0)
	{
		buffer[offset++] = (byte)value;
		buffer[offset] = (byte)(value >> 8);
		return 2;
	}

	public static int Write(this byte[] buffer, uint value, int offset = 0)
	{
		buffer[offset++] = (byte)value;
		buffer[offset++] = (byte)(value >> 8);
		buffer[offset++] = (byte)(value >> 16);
		buffer[offset] = (byte)(value >> 24);
		return 4;
	}

	public static int WriteUInt64(this byte[] buffer, ulong value, int offset = 0)
	{
		buffer[offset++] = (byte)value;
		buffer[offset++] = (byte)(value >> 8);
		buffer[offset++] = (byte)(value >> 16);
		buffer[offset++] = (byte)(value >> 24);
		buffer[offset++] = (byte)(value >> 32);
		buffer[offset++] = (byte)(value >> 40);
		buffer[offset++] = (byte)(value >> 48);
		buffer[offset] = (byte)(value >> 56);
		return 8;
	}

	public static int WritePackedUInt64(this byte[] buffer, ulong value, int offset = 0)
	{
		if (value <= 240L)
		{
			buffer[offset++] = (byte)value;
		}
		else if (value <= 2287L)
		{
			buffer[offset++] = (byte)((value - 240L) / 256L + 241L);
			buffer[offset++] = (byte)((value - 240L) % 256L);
		}
		else if (value <= 67823L)
		{
			buffer[offset++] = 249;
			buffer[offset++] = (byte)((value - 2288L) / 256L);
			buffer[offset++] = (byte)((value - 2288L) % 256L);
		}
		else if (value <= 16777215L)
		{
			buffer[offset++] = 250;
			buffer[offset++] = (byte)(value & 0xFFL);
			buffer[offset++] = (byte)((value >> 8) & 0xFFL);
			buffer[offset++] = (byte)((value >> 16) & 0xFFL);
		}
		else if (value <= 4294967295L)
		{
			buffer[offset++] = 251;
			buffer[offset++] = (byte)(value & 0xFFL);
			buffer[offset++] = (byte)((value >> 8) & 0xFFL);
			buffer[offset++] = (byte)((value >> 16) & 0xFFL);
			buffer[offset++] = (byte)((value >> 24) & 0xFFL);
		}
		else if (value <= 1099511627775L)
		{
			buffer[offset++] = 252;
			buffer[offset++] = (byte)(value & 0xFFL);
			buffer[offset++] = (byte)((value >> 8) & 0xFFL);
			buffer[offset++] = (byte)((value >> 16) & 0xFFL);
			buffer[offset++] = (byte)((value >> 24) & 0xFFL);
			buffer[offset++] = (byte)((value >> 32) & 0xFFL);
		}
		else if (value <= 281474976710655L)
		{
			buffer[offset++] = 253;
			buffer[offset++] = (byte)(value & 0xFFL);
			buffer[offset++] = (byte)((value >> 8) & 0xFFL);
			buffer[offset++] = (byte)((value >> 16) & 0xFFL);
			buffer[offset++] = (byte)((value >> 24) & 0xFFL);
			buffer[offset++] = (byte)((value >> 32) & 0xFFL);
			buffer[offset++] = (byte)((value >> 40) & 0xFFL);
		}
		else if (value <= 72057594037927935L)
		{
			buffer[offset++] = 254;
			buffer[offset++] = (byte)(value & 0xFFL);
			buffer[offset++] = (byte)((value >> 8) & 0xFFL);
			buffer[offset++] = (byte)((value >> 16) & 0xFFL);
			buffer[offset++] = (byte)((value >> 24) & 0xFFL);
			buffer[offset++] = (byte)((value >> 32) & 0xFFL);
			buffer[offset++] = (byte)((value >> 40) & 0xFFL);
			buffer[offset++] = (byte)((value >> 48) & 0xFFL);
		}
		else
		{
			buffer[offset++] = byte.MaxValue;
			buffer[offset++] = (byte)(value & 0xFFL);
			buffer[offset++] = (byte)((value >> 8) & 0xFFL);
			buffer[offset++] = (byte)((value >> 16) & 0xFFL);
			buffer[offset++] = (byte)((value >> 24) & 0xFFL);
			buffer[offset++] = (byte)((value >> 32) & 0xFFL);
			buffer[offset++] = (byte)((value >> 40) & 0xFFL);
			buffer[offset++] = (byte)((value >> 48) & 0xFFL);
			buffer[offset++] = (byte)((value >> 56) & 0xFFL);
		}
		return offset;
	}

	public static int Write(this byte[] buffer, short value, int offset = 0)
	{
		return Write(buffer, (ushort)value, offset);
	}

	public static int Write(this byte[] buffer, int value, int offset = 0)
	{
		return Write(buffer, (uint)value, offset);
	}

	public static int Write(this byte[] buffer, long value, int offset = 0)
	{
		return Write(buffer, (ulong)value, offset);
	}

	public unsafe static int Write(this byte[] buffer, float value, int offset = 0)
	{
		uint value2 = *(uint*)(&value);
		return Write(buffer, value2, offset);
	}

	public static int Write(this byte[] buffer, DateTime value, int offset = 0)
	{
		return Write(buffer, value.ToBinary(), offset);
	}

	public static byte ReadByte(this byte[] buffer, ref int offset)
	{
		return buffer[offset++];
	}

	public static ushort ReadUInt16(this byte[] buffer, ref int offset)
	{
		return (ushort)(buffer[offset++] | (buffer[offset++] << 8));
	}

	public static uint ReadUInt32(this byte[] buffer, ref int offset)
	{
		return (uint)(buffer[offset++] | (buffer[offset++] << 8) | (buffer[offset++] << 16) | (buffer[offset++] << 24));
	}

	public static ulong ReadUInt64(this byte[] buffer, ref int offset)
	{
		uint num = ReadUInt32(buffer, ref offset);
		return ((ulong)ReadUInt32(buffer, ref offset) << 32) | num;
	}

	public static short ReadInt16(this byte[] buffer, ref int offset)
	{
		return (short)(buffer[offset++] | (buffer[offset++] << 8));
	}

	public static int ReadInt32(this byte[] buffer, ref int offset)
	{
		return (int)ReadUInt32(buffer, ref offset);
	}

	public static long ReadInt64(this byte[] buffer, ref int offset)
	{
		uint num = ReadUInt32(buffer, ref offset);
		return (long)(((ulong)ReadUInt32(buffer, ref offset) << 32) | num);
	}

	public unsafe static float ReadSingle(this byte[] buffer, ref int offset)
	{
		uint num = ReadUInt32(buffer, ref offset);
		return *(float*)(&num);
	}

	public static DateTime ReadDateTime(this byte[] buffer, ref int offset)
	{
		return DateTime.FromBinary(ReadInt64(buffer, ref offset));
	}

	public static ulong ReadPackedUInt64(this byte[] buffer, ref int offset)
	{
		byte b = ReadByte(buffer, ref offset);
		if (b < 241)
		{
			return b;
		}
		byte b2 = ReadByte(buffer, ref offset);
		if (b >= 241 && b <= 248)
		{
			return (ulong)(240L + 256L * (b - 241L) + b2);
		}
		byte b3 = ReadByte(buffer, ref offset);
		if (b == 249)
		{
			return (ulong)(2288L + 256L * b2 + b3);
		}
		byte b4 = ReadByte(buffer, ref offset);
		if (b == 250)
		{
			return b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16);
		}
		byte b5 = ReadByte(buffer, ref offset);
		if (b == 251)
		{
			return b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24);
		}
		byte b6 = ReadByte(buffer, ref offset);
		if (b == 252)
		{
			return b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32);
		}
		byte b7 = ReadByte(buffer, ref offset);
		if (b == 253)
		{
			return b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32) + ((ulong)b7 << 40);
		}
		byte b8 = ReadByte(buffer, ref offset);
		if (b == 254)
		{
			return b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32) + ((ulong)b7 << 40) + ((ulong)b8 << 48);
		}
		byte b9 = ReadByte(buffer, ref offset);
		if (b != byte.MaxValue)
		{
			throw new IndexOutOfRangeException("ReadPackedUInt64() failure: " + b);
		}
		return b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32) + ((ulong)b7 << 40) + ((ulong)b8 << 48) + ((ulong)b9 << 56);
	}
}
