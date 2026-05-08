using System.Runtime.InteropServices;

public abstract class GClass1481
{
	[StructLayout(LayoutKind.Explicit)]
	public struct Struct258
	{
		[FieldOffset(0)]
		public ulong Along;

		[FieldOffset(0)]
		public double Adouble;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct Struct259
	{
		[FieldOffset(0)]
		public int Aint;

		[FieldOffset(0)]
		public float Afloat;
	}

	public static void smethod_0(byte[] buffer, int offset, ulong data)
	{
		buffer[offset] = (byte)data;
		buffer[offset + 1] = (byte)(data >> 8);
		buffer[offset + 2] = (byte)(data >> 16);
		buffer[offset + 3] = (byte)(data >> 24);
		buffer[offset + 4] = (byte)(data >> 32);
		buffer[offset + 5] = (byte)(data >> 40);
		buffer[offset + 6] = (byte)(data >> 48);
		buffer[offset + 7] = (byte)(data >> 56);
	}

	public static void smethod_1(byte[] buffer, int offset, int data)
	{
		buffer[offset] = (byte)data;
		buffer[offset + 1] = (byte)(data >> 8);
		buffer[offset + 2] = (byte)(data >> 16);
		buffer[offset + 3] = (byte)(data >> 24);
	}

	public static void WriteLittleEndian(byte[] buffer, int offset, short data)
	{
		buffer[offset] = (byte)data;
		buffer[offset + 1] = (byte)(data >> 8);
	}

	public static void GetBytes(byte[] bytes, int startIndex, double value)
	{
		Struct258 @struct = new Struct258
		{
			Adouble = value
		};
		smethod_0(bytes, startIndex, @struct.Along);
	}

	public static void GetBytes(byte[] bytes, int startIndex, float value)
	{
		Struct259 @struct = new Struct259
		{
			Afloat = value
		};
		smethod_1(bytes, startIndex, @struct.Aint);
	}

	public static void GetBytes(byte[] bytes, int startIndex, short value)
	{
		WriteLittleEndian(bytes, startIndex, value);
	}

	public static void GetBytes(byte[] bytes, int startIndex, ushort value)
	{
		WriteLittleEndian(bytes, startIndex, (short)value);
	}

	public static void GetBytes(byte[] bytes, int startIndex, int value)
	{
		smethod_1(bytes, startIndex, value);
	}

	public static void GetBytes(byte[] bytes, int startIndex, uint value)
	{
		smethod_1(bytes, startIndex, (int)value);
	}

	public static void GetBytes(byte[] bytes, int startIndex, long value)
	{
		smethod_0(bytes, startIndex, (ulong)value);
	}

	public static void GetBytes(byte[] bytes, int startIndex, ulong value)
	{
		smethod_0(bytes, startIndex, value);
	}
}
