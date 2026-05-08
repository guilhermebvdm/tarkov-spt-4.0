using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class GClass1295
{
	[NonSerialized]
	public const float Float_0 = -0.707107f;

	[NonSerialized]
	public const float Float_1 = 0.707107f;

	[NonSerialized]
	public const ushort Ushort_0 = 1023;

	public static bool ScaleToLong(float value, float precision, out long result)
	{
		if (precision == 0f)
		{
			throw new DivideByZeroException("ScaleToLong: precision=0 would cause null division. If rounding isn't wanted, don't call this function.");
		}
		try
		{
			result = Convert.ToInt64(value / precision);
			return true;
		}
		catch (OverflowException)
		{
			result = ((value > 0f) ? long.MaxValue : long.MinValue);
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool ScaleToLong(Vector3 value, float precision, out long x, out long y, out long z)
	{
		return (byte)(1u & (ScaleToLong(value.x, precision, out x) ? 1u : 0u) & (ScaleToLong(value.y, precision, out y) ? 1u : 0u) & (ScaleToLong(value.z, precision, out z) ? 1u : 0u)) != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool ScaleToLong(Vector3 value, float precision, out GStruct126 quantized)
	{
		quantized = GStruct126.zero;
		return ScaleToLong(value, precision, out quantized.x, out quantized.y, out quantized.z);
	}

	public static float ScaleToFloat(long value, float precision)
	{
		if (precision == 0f)
		{
			throw new DivideByZeroException("ScaleToLong: precision=0 would cause null division. If rounding isn't wanted, don't call this function.");
		}
		return (float)value * precision;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 ScaleToFloat(long x, long y, long z, float precision)
	{
		Vector3 result = default(Vector3);
		result.x = ScaleToFloat(x, precision);
		result.y = ScaleToFloat(y, precision);
		result.z = ScaleToFloat(z, precision);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 ScaleToFloat(GStruct126 value, float precision)
	{
		return ScaleToFloat(value.x, value.y, value.z, precision);
	}

	public static ushort ScaleFloatToUShort(float value, float minValue, float maxValue, ushort minTarget, ushort maxTarget)
	{
		int num = maxTarget - minTarget;
		float num2 = maxValue - minValue;
		float num3 = value - minValue;
		return (ushort)(minTarget + (ushort)(num3 / num2 * (float)num));
	}

	public static float ScaleUShortToFloat(ushort value, ushort minValue, ushort maxValue, float minTarget, float maxTarget)
	{
		float num = maxTarget - minTarget;
		ushort num2 = (ushort)(maxValue - minValue);
		ushort num3 = (ushort)(value - minValue);
		return minTarget + (float)(int)num3 / (float)(int)num2 * num;
	}

	public static int LargestAbsoluteComponentIndex(Vector4 value, out float largestAbs, out Vector3 withoutLargest)
	{
		Vector4 vector = new Vector4(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z), Mathf.Abs(value.w));
		largestAbs = vector.x;
		withoutLargest = new Vector3(value.y, value.z, value.w);
		int result = 0;
		if (vector.y > largestAbs)
		{
			result = 1;
			largestAbs = vector.y;
			withoutLargest = new Vector3(value.x, value.z, value.w);
		}
		if (vector.z > largestAbs)
		{
			result = 2;
			largestAbs = vector.z;
			withoutLargest = new Vector3(value.x, value.y, value.w);
		}
		if (vector.w > largestAbs)
		{
			result = 3;
			largestAbs = vector.w;
			withoutLargest = new Vector3(value.x, value.y, value.z);
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float smethod_0(Quaternion q, int element)
	{
		return element switch
		{
			0 => q.x, 
			1 => q.y, 
			2 => q.z, 
			3 => q.w, 
			_ => 0f, 
		};
	}

	public static uint CompressQuaternion(Quaternion q)
	{
		float largestAbs;
		Vector3 withoutLargest;
		int num = LargestAbsoluteComponentIndex(new Vector4(q.x, q.y, q.z, q.w), out largestAbs, out withoutLargest);
		if (smethod_0(q, num) < 0f)
		{
			withoutLargest = -withoutLargest;
		}
		ushort num2 = ScaleFloatToUShort(withoutLargest.x, -0.707107f, 0.707107f, 0, 1023);
		ushort num3 = ScaleFloatToUShort(withoutLargest.y, -0.707107f, 0.707107f, 0, 1023);
		ushort num4 = ScaleFloatToUShort(withoutLargest.z, -0.707107f, 0.707107f, 0, 1023);
		return (uint)((num << 30) | (num2 << 20) | (num3 << 10) | num4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Quaternion smethod_1(Quaternion value)
	{
		Vector4 vector = new Vector4(value.x, value.y, value.z, value.w);
		if (!(Vector4.Dot(vector, vector) > 1.1754944E-38f))
		{
			return Quaternion.identity;
		}
		return value.normalized;
	}

	public static Quaternion DecompressQuaternion(uint data)
	{
		ushort value = (ushort)(data & 0x3FF);
		ushort value2 = (ushort)((data >> 10) & 0x3FF);
		ushort value3 = (ushort)((data >> 20) & 0x3FF);
		int num = (int)(data >> 30);
		float num2 = ScaleUShortToFloat(value3, 0, 1023, -0.707107f, 0.707107f);
		float num3 = ScaleUShortToFloat(value2, 0, 1023, -0.707107f, 0.707107f);
		float num4 = ScaleUShortToFloat(value, 0, 1023, -0.707107f, 0.707107f);
		float num5 = Mathf.Sqrt(1f - num2 * num2 - num3 * num3 - num4 * num4);
		Vector4 vector = num switch
		{
			0 => new Vector4(num5, num2, num3, num4), 
			1 => new Vector4(num2, num5, num3, num4), 
			2 => new Vector4(num2, num3, num5, num4), 
			_ => new Vector4(num2, num3, num4, num5), 
		};
		return smethod_1(new Quaternion(vector.x, vector.y, vector.z, vector.w));
	}

	public static void CompressVarUInt(EFTWriterClass writer, ulong value)
	{
		if (value <= 240L)
		{
			writer.WriteByte((byte)value);
		}
		else if (value <= 2287L)
		{
			writer.WriteByte((byte)((value - 240L >> 8) + 241L));
			writer.WriteByte((byte)((value - 240L) & 0xFFL));
		}
		else if (value <= 67823L)
		{
			writer.WriteByte(249);
			writer.WriteByte((byte)(value - 2288L >> 8));
			writer.WriteByte((byte)((value - 2288L) & 0xFFL));
		}
		else if (value <= 16777215L)
		{
			writer.WriteByte(250);
			writer.WriteByte((byte)(value & 0xFFL));
			writer.WriteByte((byte)((value >> 8) & 0xFFL));
			writer.WriteByte((byte)((value >> 16) & 0xFFL));
		}
		else if (value <= 4294967295L)
		{
			writer.WriteByte(251);
			writer.WriteByte((byte)(value & 0xFFL));
			writer.WriteByte((byte)((value >> 8) & 0xFFL));
			writer.WriteByte((byte)((value >> 16) & 0xFFL));
			writer.WriteByte((byte)((value >> 24) & 0xFFL));
		}
		else if (value <= 1099511627775L)
		{
			writer.WriteByte(252);
			writer.WriteByte((byte)(value & 0xFFL));
			writer.WriteByte((byte)((value >> 8) & 0xFFL));
			writer.WriteByte((byte)((value >> 16) & 0xFFL));
			writer.WriteByte((byte)((value >> 24) & 0xFFL));
			writer.WriteByte((byte)((value >> 32) & 0xFFL));
		}
		else if (value <= 281474976710655L)
		{
			writer.WriteByte(253);
			writer.WriteByte((byte)(value & 0xFFL));
			writer.WriteByte((byte)((value >> 8) & 0xFFL));
			writer.WriteByte((byte)((value >> 16) & 0xFFL));
			writer.WriteByte((byte)((value >> 24) & 0xFFL));
			writer.WriteByte((byte)((value >> 32) & 0xFFL));
			writer.WriteByte((byte)((value >> 40) & 0xFFL));
		}
		else if (value <= 72057594037927935L)
		{
			writer.WriteByte(254);
			writer.WriteByte((byte)(value & 0xFFL));
			writer.WriteByte((byte)((value >> 8) & 0xFFL));
			writer.WriteByte((byte)((value >> 16) & 0xFFL));
			writer.WriteByte((byte)((value >> 24) & 0xFFL));
			writer.WriteByte((byte)((value >> 32) & 0xFFL));
			writer.WriteByte((byte)((value >> 40) & 0xFFL));
			writer.WriteByte((byte)((value >> 48) & 0xFFL));
		}
		else
		{
			writer.WriteByte(byte.MaxValue);
			writer.WriteByte((byte)(value & 0xFFL));
			writer.WriteByte((byte)((value >> 8) & 0xFFL));
			writer.WriteByte((byte)((value >> 16) & 0xFFL));
			writer.WriteByte((byte)((value >> 24) & 0xFFL));
			writer.WriteByte((byte)((value >> 32) & 0xFFL));
			writer.WriteByte((byte)((value >> 40) & 0xFFL));
			writer.WriteByte((byte)((value >> 48) & 0xFFL));
			writer.WriteByte((byte)((value >> 56) & 0xFFL));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void CompressVarInt(EFTWriterClass writer, long i)
	{
		ulong value = (ulong)((i >> 63) ^ (i << 1));
		CompressVarUInt(writer, value);
	}

	public static ulong DecompressVarUInt(EFTReaderClass reader)
	{
		byte b = reader.ReadByte();
		if (b < 241)
		{
			return b;
		}
		byte b2 = reader.ReadByte();
		if (b <= 248)
		{
			return (ulong)(240L + (b - 241L << 8) + b2);
		}
		byte b3 = reader.ReadByte();
		if (b == 249)
		{
			return 2288L + ((ulong)b2 << 8) + b3;
		}
		byte b4 = reader.ReadByte();
		if (b == 250)
		{
			return b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16);
		}
		byte b5 = reader.ReadByte();
		if (b == 251)
		{
			return b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24);
		}
		byte b6 = reader.ReadByte();
		if (b == 252)
		{
			return b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32);
		}
		byte b7 = reader.ReadByte();
		if (b == 253)
		{
			return b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32) + ((ulong)b7 << 40);
		}
		byte b8 = reader.ReadByte();
		if (b == 254)
		{
			return b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32) + ((ulong)b7 << 40) + ((ulong)b8 << 48);
		}
		byte b9 = reader.ReadByte();
		if (b != byte.MaxValue)
		{
			throw new IndexOutOfRangeException($"DecompressVarInt failure: {b}");
		}
		return b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32) + ((ulong)b7 << 40) + ((ulong)b8 << 48) + ((ulong)b9 << 56);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long DecompressVarInt(EFTReaderClass reader)
	{
		ulong num = DecompressVarUInt(reader);
		return (long)((num >> 1) ^ (0L - (num & 1L)));
	}

	public static short ScaleFloatToShort(float value, float minValue, float maxValue)
	{
		return ScaleFloatToShort(value, minValue, maxValue, short.MinValue, short.MaxValue);
	}

	public static short ScaleFloatToShort(float value, float minValue, float maxValue, short minTarget, short maxTarget)
	{
		int num = maxTarget - minTarget;
		float num2 = maxValue - minValue;
		float num3 = Mathf.Clamp(value, minValue, maxValue) - minValue;
		return (short)(minTarget + (short)(num3 / num2 * (float)num));
	}

	public static float ScaleShortToFloat(short value, float minTarget, float maxTarget)
	{
		return ScaleShortToFloat(value, short.MinValue, short.MaxValue, minTarget, maxTarget);
	}

	public static float ScaleShortToFloat(short value, short minValue, short maxValue, float minTarget, float maxTarget)
	{
		float num = maxTarget - minTarget;
		ushort num2 = (ushort)(maxValue - minValue);
		ushort num3 = (ushort)(Mathf.Clamp(value, minValue, maxValue) - minValue);
		return minTarget + (float)(int)num3 / (float)(int)num2 * num;
	}

	public static byte ScaleFloatToByte(float value, float minValue, float maxValue)
	{
		return ScaleFloatToByte(value, minValue, maxValue, 0, byte.MaxValue);
	}

	public static byte ScaleFloatToByte(float value, float minValue, float maxValue, byte minTarget, byte maxTarget)
	{
		int num = maxTarget - minTarget;
		float num2 = maxValue - minValue;
		float num3 = Mathf.Clamp(value, minValue, maxValue) - minValue;
		return (byte)(minTarget + (byte)(num3 / num2 * (float)num));
	}

	public static float ScaleByteToFloat(byte value, float minTarget, float maxTarget)
	{
		return ScaleByteToFloat(value, 0, byte.MaxValue, minTarget, maxTarget);
	}

	public static float ScaleByteToFloat(byte value, byte minValue, byte maxValue, float minTarget, float maxTarget)
	{
		float num = maxTarget - minTarget;
		byte b = (byte)(maxValue - minValue);
		byte b2 = (byte)(Mathf.Clamp(value, minValue, maxValue) - minValue);
		return minTarget + (float)(int)b2 / (float)(int)b * num;
	}

	public static byte ScaleIntToByte(int value, int minValue, int maxValue)
	{
		return ScaleIntToByte(value, minValue, maxValue, 0, byte.MaxValue);
	}

	public static byte ScaleIntToByte(int value, int minValue, int maxValue, byte minTarget, byte maxTarget)
	{
		int num = maxTarget - minTarget;
		float num2 = maxValue - minValue;
		float num3 = Mathf.Clamp(value, minValue, maxValue) - minValue;
		return (byte)(minTarget + (byte)(num3 / num2 * (float)num));
	}

	public static int ScaleByteToInt(byte value, int minTarget, int maxTarget)
	{
		return ScaleByteToInt(value, 0, byte.MaxValue, minTarget, maxTarget);
	}

	public static int ScaleByteToInt(byte value, byte minValue, byte maxValue, int minTarget, int maxTarget)
	{
		float num = maxTarget - minTarget;
		byte b = (byte)(maxValue - minValue);
		byte b2 = (byte)(Mathf.Clamp(value, minValue, maxValue) - minValue);
		return (int)((float)minTarget + (float)(int)b2 / (float)(int)b * num);
	}

	public static GStruct123 ScaleToVector3Short(Vector3 value, float minValue, float maxValue)
	{
		return ScaleToVector3Short(value, minValue, maxValue, short.MinValue, short.MaxValue);
	}

	public static GStruct123 ScaleToVector3Short(Vector3 value, float minValue, float maxValue, short minTarget, short maxTarget)
	{
		return new GStruct123(ScaleFloatToShort(value.x, minValue, maxValue, minTarget, maxTarget), ScaleFloatToShort(value.y, minValue, maxValue, minTarget, maxTarget), ScaleFloatToShort(value.z, minValue, maxValue, minTarget, maxTarget));
	}

	public static Vector3 ScaleFromVector3Short(GStruct123 value, float minTarget, float maxTarget)
	{
		return ScaleFromVector3Short(value, short.MinValue, short.MaxValue, minTarget, maxTarget);
	}

	public static Vector3 ScaleFromVector3Short(GStruct123 value, short minValue, short maxValue, float minTarget, float maxTarget)
	{
		return new Vector3(ScaleShortToFloat(value.x, minValue, maxValue, minTarget, maxTarget), ScaleShortToFloat(value.y, minValue, maxValue, minTarget, maxTarget), ScaleShortToFloat(value.z, minValue, maxValue, minTarget, maxTarget));
	}

	public static GStruct122 ScaleToVector3Byte(Vector3 value, float minValue, float maxValue)
	{
		return ScaleToVector3Byte(value, minValue, maxValue, 0, byte.MaxValue);
	}

	public static GStruct122 ScaleToVector3Byte(Vector3 value, float minValue, float maxValue, byte minTarget, byte maxTarget)
	{
		return new GStruct122(ScaleFloatToByte(value.x, minValue, maxValue, minTarget, maxTarget), ScaleFloatToByte(value.y, minValue, maxValue, minTarget, maxTarget), ScaleFloatToByte(value.z, minValue, maxValue, minTarget, maxTarget));
	}

	public static Vector3 ScaleFromVector3Byte(GStruct122 value, float minTarget, float maxTarget)
	{
		return ScaleFromVector3Byte(value, 0, byte.MaxValue, minTarget, maxTarget);
	}

	public static Vector3 ScaleFromVector3Byte(GStruct122 value, byte minValue, byte maxValue, float minTarget, float maxTarget)
	{
		return new Vector3(ScaleByteToFloat(value.x, minValue, maxValue, minTarget, maxTarget), ScaleByteToFloat(value.y, minValue, maxValue, minTarget, maxTarget), ScaleByteToFloat(value.z, minValue, maxValue, minTarget, maxTarget));
	}

	public static GStruct121 ScaleToVector2Short(Vector2 value, float minValue, float maxValue)
	{
		return ScaleToVector2Short(value, minValue, maxValue, short.MinValue, short.MaxValue);
	}

	public static GStruct121 ScaleToVector2Short(Vector2 value, float minValue, float maxValue, short minTarget, short maxTarget)
	{
		return new GStruct121(ScaleFloatToShort(value.x, minValue, maxValue, minTarget, maxTarget), ScaleFloatToShort(value.y, minValue, maxValue, minTarget, maxTarget));
	}

	public static Vector2 ScaleFromVector2Short(GStruct121 value, short minTarget, short maxTarget)
	{
		return ScaleFromVector2Short(value, short.MinValue, short.MaxValue, minTarget, maxTarget);
	}

	public static Vector2 ScaleFromVector2Short(GStruct121 value, short minValue, short maxValue, float minTarget, float maxTarget)
	{
		return new Vector2(ScaleShortToFloat(value.x, minValue, maxValue, minTarget, maxTarget), ScaleShortToFloat(value.y, minValue, maxValue, minTarget, maxTarget));
	}

	public static GStruct120 ScaleToVector2Byte(Vector2 value, float minValue, float maxValue)
	{
		return ScaleToVector2Byte(value, minValue, maxValue, 0, byte.MaxValue);
	}

	public static GStruct120 ScaleToVector2Byte(Vector2 value, float minValue, float maxValue, byte minTarget, byte maxTarget)
	{
		return new GStruct120(ScaleFloatToByte(value.x, minValue, maxValue, minTarget, maxTarget), ScaleFloatToByte(value.y, minValue, maxValue, minTarget, maxTarget));
	}

	public static Vector2 ScaleFromVector2Byte(GStruct120 value, float minTarget, float maxTarget)
	{
		return ScaleFromVector2Byte(value, 0, byte.MaxValue, minTarget, maxTarget);
	}

	public static Vector2 ScaleFromVector2Byte(GStruct120 value, byte minValue, byte maxValue, float minTarget, float maxTarget)
	{
		return new Vector2(ScaleByteToFloat(value.x, minValue, maxValue, minTarget, maxTarget), ScaleByteToFloat(value.y, minValue, maxValue, minTarget, maxTarget));
	}

	public static void AttachToMask(ref byte mask, byte index, bool value)
	{
		if (value)
		{
			mask |= (byte)(1 << (int)index);
		}
	}

	public static bool AttachedToMask(byte mask, byte index)
	{
		return mask == (mask | (1 << (int)index));
	}
}
