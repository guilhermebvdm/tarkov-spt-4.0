using System;
using BitPacking;
using UnityEngine;

public class GClass1368 : GInterface131, ISerializer
{
	[NonSerialized]
	public IMeasureStatistics IMeasureStatistics;

	public bool IsOverflow => IMeasureStatistics.IsOverflow;

	public int BitsWritten => IMeasureStatistics.BitsWritten;

	public int BitsCount => IMeasureStatistics.BitsCount;

	public int BitsRemain => BitsCount - BitsWritten;

	public int BytesWritten => IMeasureStatistics.BytesWritten;

	public byte[] Buffer => IMeasureStatistics.Buffer;

	public EBitStreamMode StreamMode => EBitStreamMode.Writing;

	public GClass1368(byte[] buffer)
	{
		IMeasureStatistics = new GClass1367(buffer);
	}

	public void WriteLimitedInt32(int value, int min, int max, BitPackingTag tag)
	{
		if (min >= max || value < min || value > max)
		{
			Debug.LogErrorFormat("BitWriterStream::WriteLimitedInt32 Data Loss Error. Min {0}, Max {1}, Value {2} tag:{3}", min, max, value, tag);
		}
		int bits = GClass1365.BitRequired(min, max);
		uint value2 = (uint)(value - min);
		IMeasureStatistics.WriteBits(value2, bits);
	}

	public void WriteUInt32UsingBitRange(uint value, int[] rangeBits, BitPackingTag tag)
	{
		int num = 0;
		int num2 = rangeBits.Length;
		int num3 = 0;
		int num4;
		while (true)
		{
			if (num3 < num2 - 1)
			{
				num4 = num + ((1 << rangeBits[num3]) - 1);
				bool flag = value <= num4;
				Write(flag);
				if (flag)
				{
					break;
				}
				num += 1 << rangeBits[num3];
				num3++;
				continue;
			}
			WriteLimitedInt32((int)value, num, num + ((1 << rangeBits[num2 - 1]) - 1), tag);
			return;
		}
		WriteLimitedInt32((int)value, num, num4, tag);
	}

	public void WriteLimitedFloat(float value, float min, float max, float res, BitPackingTag tag)
	{
		if (!(min < max) || !(value >= min) || !(value <= max))
		{
			Debug.LogErrorFormat("BitWriterStream::WriteLimitedFloat Data Loss Error. Min {0}, Max {1}, Value {2}, tag:{3} ", min, max, value, tag);
		}
		GClass1365.CalculateDataForQuantizing(min, max, res, out var delta, out var maxIntegerValue, out var bits);
		uint value2 = GClass1365.QuantizeFloatValue(value, min, delta, maxIntegerValue);
		WriteBits(value2, bits);
	}

	public void WriteLimitedString(string value, char min, char max, BitPackingTag tag, uint? maxSize)
	{
		if (value != null && maxSize.HasValue && value.Length > maxSize.Value)
		{
			throw GClass1369.InvalidMaxSize(value.Length, maxSize.Value);
		}
		if (min > max)
		{
			Debug.LogError("BitWriterStream::WriteLimitedString min > max");
		}
		bool flag = value == null;
		Write(flag);
		if (flag)
		{
			return;
		}
		WriteAlign();
		int length = value.Length;
		Write(length);
		int bits = GClass1365.BitRequired(min, max);
		for (int i = 0; i < length; i++)
		{
			char c = value[i];
			if (c < min || c > max)
			{
				Debug.LogErrorFormat("BitWriterStream::WriteLimitedString Data Loss Error. Min {0}, Max {1}, Value '{2}', tag {3}", min, max, value, tag);
			}
			uint value2 = (uint)(c - min);
			IMeasureStatistics.WriteBits(value2, bits);
		}
	}

	public void WriteBits(uint value, int bits)
	{
		IMeasureStatistics.WriteBits(value, bits);
	}

	public void WriteBytes(byte[] data, int startIndex, int length)
	{
		WriteAlign();
		IMeasureStatistics.WriteBytes(data, startIndex, length);
	}

	public void WriteBytesAndSize(byte[] data)
	{
		Write((ushort)data.Length);
		WriteBytes(data, 0, data.Length);
	}

	public void Write(char value)
	{
		WriteBits(value, 16);
	}

	public void Write(byte value)
	{
		WriteBits(value, 8);
	}

	public void Write(short value)
	{
		WriteBits((uint)value, 16);
	}

	public void Write(ushort value)
	{
		WriteBits(value, 16);
	}

	public void Write(int value)
	{
		WriteBits((uint)value, 32);
	}

	public void Write(uint value)
	{
		WriteBits(value, 32);
	}

	public void Write(long value)
	{
		uint value2 = (uint)(value & 0xFFFFFFFFL);
		uint value3 = (uint)((ulong)value >> 32);
		Write(value2);
		Write(value3);
	}

	public void Write(ulong value)
	{
		uint value2 = (uint)(value & 0xFFFFFFFFL);
		uint value3 = (uint)(value >> 32);
		Write(value2);
		Write(value3);
	}

	public void Write(float value)
	{
		GStruct135 gStruct = new GStruct135
		{
			Float = value
		};
		Write(gStruct.UInt);
	}

	public void Write(double value)
	{
		GStruct133 gStruct = new GStruct133
		{
			Double = value
		};
		Write(gStruct.Ulong);
	}

	public void Write(string value)
	{
		bool flag = value == null;
		Write(flag);
		if (!flag)
		{
			WriteAlign();
			int length = value.Length;
			Write(length);
			for (int i = 0; i < length; i++)
			{
				char value2 = value[i];
				Write(value2);
			}
		}
	}

	public void Write(Vector3 value)
	{
		Write(value.x);
		Write(value.y);
		Write(value.z);
	}

	public void Write(Vector2 value)
	{
		Write(value.x);
		Write(value.y);
	}

	public void Write(Quaternion value)
	{
		Write(value.x);
		Write(value.y);
		Write(value.z);
		Write(value.w);
	}

	public void Write(bool value)
	{
		WriteBits(value ? 1u : 0u, 1);
	}

	public void Write(GInterface131 value)
	{
		int num = value.BitsWritten / 8;
		int num2 = value.BitsWritten % 8;
		for (int i = 0; i < num; i++)
		{
			WriteBits(value.Buffer[4 * (i / 4) + (4 - i % 4 - 1)], 8);
		}
		if (num2 > 0)
		{
			WriteBits(value.Buffer[4 * (num / 4) + (4 - num % 4 - 1)], num2);
		}
	}

	public void WriteCheck(uint checkNumber)
	{
		WriteAlign();
		WriteBits(checkNumber, 32);
	}

	public void WriteAlign()
	{
		IMeasureStatistics.WriteByteAlign();
	}

	public void Write<[Attribute1] TEnum>(TEnum value) where TEnum : struct, Enum, IConvertible
	{
		long min = GClass1365.GClass1366<TEnum>.Min;
		long max = GClass1365.GClass1366<TEnum>.Max;
		long num = GClass1365.GClass1366<TEnum>.ToInt(value);
		int bits = GClass1365.BitRequired(min, max);
		uint value2 = (uint)(num - min);
		IMeasureStatistics.WriteBits(value2, bits);
	}

	public void Flush()
	{
		IMeasureStatistics.FlushBits();
	}

	public void Reset()
	{
		IMeasureStatistics.Reset();
	}

	public void SerializeLimitedInt32(ref int value, int min, int max, BitPackingTag tag)
	{
		WriteLimitedInt32(value, min, max, tag);
	}

	public void SerializeUInt32UsingBitRange(ref uint value, int[] rangeBits, BitPackingTag tag)
	{
		WriteUInt32UsingBitRange(value, rangeBits, tag);
	}

	public void SerializeLimitedFloat(ref float value, float min, float max, float res, BitPackingTag tag)
	{
		WriteLimitedFloat(value, min, max, res, tag);
	}

	public void SerializeLimitedString(ref string value, char min, char max, BitPackingTag tag, uint? maxSize = 1600u)
	{
		WriteLimitedString(value, min, max, tag, maxSize);
	}

	public void SerializeBits(ref uint value, int bits)
	{
		WriteBits(value, bits);
	}

	public void SerializeBytes(ref byte[] data, int startIndex, int length)
	{
		WriteBytes(data, startIndex, length);
	}

	public void SerializeBytesAndSize(ref byte[] data, uint? maxSize)
	{
		if (data != null && maxSize.HasValue && data.Length > maxSize.Value)
		{
			throw GClass1369.InvalidMaxSize(data.Length, maxSize.Value);
		}
		WriteBytesAndSize(data);
	}

	public void Serialize(ref char value)
	{
		Write(value);
	}

	public void Serialize(ref byte value)
	{
		Write(value);
	}

	public void Serialize(ref short value)
	{
		Write(value);
	}

	public void Serialize(ref ushort value)
	{
		Write(value);
	}

	public void Serialize(ref int value)
	{
		Write(value);
	}

	public void Serialize(ref uint value)
	{
		Write(value);
	}

	public void Serialize(ref long value)
	{
		Write(value);
	}

	public void Serialize(ref ulong value)
	{
		Write(value);
	}

	public void Serialize(ref float value)
	{
		Write(value);
	}

	public void Serialize(ref double value)
	{
		Write(value);
	}

	public void Serialize(ref string value, uint? maxSize)
	{
		if (value != null && maxSize.HasValue && value.Length > maxSize.Value)
		{
			throw GClass1369.InvalidMaxSize(value.Length, maxSize.Value);
		}
		Write(value);
	}

	public void Serialize(ref Vector3 value)
	{
		Write(value);
	}

	public void Serialize(ref Vector2 value)
	{
		Write(value);
	}

	public void Serialize(ref Quaternion value)
	{
		Write(value);
	}

	public void Serialize(ref bool value)
	{
		Write(value);
	}

	public void Serialize<[Attribute1] TEnum>(ref TEnum value) where TEnum : struct, Enum, IConvertible
	{
		Write(value);
	}

	public bool SerializeCheck(uint checkNumber, string message = null)
	{
		WriteCheck(checkNumber);
		return true;
	}

	public void SerializeAlign()
	{
		WriteAlign();
	}
}
