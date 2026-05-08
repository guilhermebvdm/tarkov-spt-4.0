using System;
using BitPacking;
using UnityEngine;

public class GClass1364 : IDataReader, ISerializer
{
	[NonSerialized]
	public GInterface127 Ginterface127_0;

	public byte[] Buffer => Ginterface127_0.Buffer;

	public int BitsRead => Ginterface127_0.BitsRead;

	public int BitsCount => Ginterface127_0.BitsCount;

	public int BitsRemain => BitsCount - BitsRead;

	public int BytesRead => Ginterface127_0.BytesRead;

	public bool IsOverflow => Ginterface127_0.IsOverflow;

	public EBitStreamMode StreamMode => EBitStreamMode.Reading;

	public GClass1364(byte[] buffer)
	{
		Ginterface127_0 = new GClass1363(buffer);
	}

	public int ReadLimitedInt32(int min, int max)
	{
		int bits = GClass1365.BitRequired(min, max);
		return (int)Ginterface127_0.ReadBits(bits) + min;
	}

	public uint ReadUInt32UsingBitRange(int[] rangeBits)
	{
		int num = 0;
		int num2 = rangeBits.Length;
		int num3 = 0;
		int max;
		while (true)
		{
			if (num3 < num2 - 1)
			{
				max = num + ((1 << rangeBits[num3]) - 1);
				if (ReadBool())
				{
					break;
				}
				num += 1 << rangeBits[num3];
				num3++;
				continue;
			}
			return (uint)ReadLimitedInt32(num, num + ((1 << rangeBits[num2 - 1]) - 1));
		}
		return (uint)ReadLimitedInt32(num, max);
	}

	public float ReadLimitedFloat(float min, float max, float res)
	{
		GClass1365.CalculateDataForQuantizing(min, max, res, out var delta, out var maxIntegerValue, out var bits);
		return GClass1365.DequantizeUIntToFloat(ReadBits(bits), min, maxIntegerValue, delta);
	}

	public string ReadLimitedString(char min, char max, uint? maxSize = 1600u)
	{
		if (ReadBool())
		{
			return null;
		}
		ReadAlign();
		int num = ReadInt32();
		if (maxSize.HasValue && num > maxSize)
		{
			throw GClass1369.InvalidMaxSize(num, maxSize.Value, Ginterface127_0);
		}
		char[] array = new char[num];
		int bits = GClass1365.BitRequired(min, max);
		for (int i = 0; i < num; i++)
		{
			uint num2 = Ginterface127_0.ReadBits(bits);
			array[i] = (char)(num2 + min);
		}
		return new string(array);
	}

	public uint ReadBits(int bits)
	{
		return Ginterface127_0.ReadBits(bits);
	}

	public void ReadBytes(byte[] destinationBytes, int destinationStartIndex, int length)
	{
		ReadAlign();
		Ginterface127_0.ReadBytes(destinationBytes, destinationStartIndex, length);
	}

	public byte[] ReadBytesAlloc(uint? maxSize)
	{
		ushort num = ReadUInt16();
		if (maxSize.HasValue && num > maxSize)
		{
			throw GClass1369.InvalidMaxSize(num, maxSize.Value, Ginterface127_0);
		}
		byte[] array = new byte[num];
		ReadBytes(array, 0, array.Length);
		return array;
	}

	public char ReadChar()
	{
		return (char)Ginterface127_0.ReadBits(16);
	}

	public byte ReadByte()
	{
		return (byte)Ginterface127_0.ReadBits(8);
	}

	public short ReadInt16()
	{
		return (short)Ginterface127_0.ReadBits(16);
	}

	public ushort ReadUInt16()
	{
		return (ushort)Ginterface127_0.ReadBits(16);
	}

	public int ReadInt32()
	{
		return (int)Ginterface127_0.ReadBits(32);
	}

	public uint ReadUInt32()
	{
		return Ginterface127_0.ReadBits(32);
	}

	public long ReadInt64()
	{
		uint num = ReadUInt32();
		return (long)(((ulong)ReadUInt32() << 32) | num);
	}

	public ulong ReadUInt64()
	{
		uint num = ReadUInt32();
		return ((ulong)ReadUInt32() << 32) | num;
	}

	public float ReadFloat()
	{
		GStruct135 gStruct = new GStruct135
		{
			UInt = ReadUInt32()
		};
		return gStruct.Float;
	}

	public double ReadDouble()
	{
		GStruct133 gStruct = new GStruct133
		{
			Ulong = ReadUInt64()
		};
		return gStruct.Double;
	}

	public string ReadString(uint? maxSize)
	{
		if (ReadBool())
		{
			return null;
		}
		ReadAlign();
		int num = ReadInt32();
		if (num >= 0 && (!maxSize.HasValue || !(num > maxSize)))
		{
			if (num > 1000000)
			{
				throw GClass1369.InvalidMaxSize(num, 1000000u, Ginterface127_0);
			}
			char[] array = new char[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = ReadChar();
			}
			return new string(array);
		}
		throw GClass1369.InvalidMaxSize(num, maxSize.GetValueOrDefault(), Ginterface127_0);
	}

	public Vector3 ReadVector3()
	{
		float x = ReadFloat();
		float y = ReadFloat();
		float z = ReadFloat();
		return new Vector3(x, y, z);
	}

	public Vector2 ReadVector2()
	{
		float x = ReadFloat();
		float y = ReadFloat();
		return new Vector2(x, y);
	}

	public Quaternion ReadQuaternion()
	{
		float x = ReadFloat();
		float y = ReadFloat();
		float z = ReadFloat();
		float w = ReadFloat();
		return new Quaternion(x, y, z, w);
	}

	public bool ReadBool()
	{
		return ReadBits(1) != 0;
	}

	public TEnum ReadEnum<[Attribute1] TEnum>() where TEnum : struct, Enum, IConvertible
	{
		long min = GClass1365.GClass1366<TEnum>.Min;
		long max = GClass1365.GClass1366<TEnum>.Max;
		int bits = GClass1365.BitRequired(min, max);
		return GClass1365.GClass1366<TEnum>.ToEnum((int)Ginterface127_0.ReadBits(bits) + min);
	}

	public bool ReadCheck(uint magic, string message = null)
	{
		ReadAlign();
		uint num = ReadBits(32);
		return magic == num;
	}

	public void ReadAlign()
	{
		Ginterface127_0.ReadAlign();
	}

	public void Reset()
	{
		Ginterface127_0.Reset();
	}

	public void SerializeLimitedInt32(ref int value, int min, int max, BitPackingTag tag)
	{
		value = ReadLimitedInt32(min, max);
	}

	public void SerializeUInt32UsingBitRange(ref uint value, int[] rangeBits, BitPackingTag tag)
	{
		value = ReadUInt32UsingBitRange(rangeBits);
	}

	public void SerializeLimitedFloat(ref float value, float min, float max, float res, BitPackingTag tag)
	{
		value = ReadLimitedFloat(min, max, res);
	}

	public void SerializeLimitedString(ref string value, char min, char max, BitPackingTag tag, uint? maxSize = 1600u)
	{
		value = ReadLimitedString(min, max, maxSize);
	}

	public void SerializeBits(ref uint value, int bits)
	{
		value = ReadBits(bits);
	}

	public void SerializeBytes(ref byte[] data, int startIndex, int length)
	{
		ReadBytes(data, startIndex, length);
	}

	public void SerializeBytesAndSize(ref byte[] data, uint? maxSize)
	{
		data = ReadBytesAlloc(maxSize);
	}

	public void Serialize(ref char value)
	{
		value = ReadChar();
	}

	public void Serialize(ref byte value)
	{
		value = ReadByte();
	}

	public void Serialize(ref short value)
	{
		value = ReadInt16();
	}

	public void Serialize(ref ushort value)
	{
		value = ReadUInt16();
	}

	public void Serialize(ref int value)
	{
		value = ReadInt32();
	}

	public void Serialize(ref uint value)
	{
		value = ReadUInt32();
	}

	public void Serialize(ref long value)
	{
		value = ReadInt64();
	}

	public void Serialize(ref ulong value)
	{
		value = ReadUInt64();
	}

	public void Serialize(ref float value)
	{
		value = ReadFloat();
	}

	public void Serialize(ref double value)
	{
		value = ReadDouble();
	}

	public void Serialize(ref string value, uint? maxSize)
	{
		value = ReadString(maxSize);
	}

	public void Serialize(ref Vector3 value)
	{
		value = ReadVector3();
	}

	public void Serialize(ref Vector2 value)
	{
		value = ReadVector2();
	}

	public void Serialize(ref Quaternion value)
	{
		value = ReadQuaternion();
	}

	public void Serialize(ref bool value)
	{
		value = ReadBool();
	}

	public void Serialize<[Attribute1] TEnum>(ref TEnum value) where TEnum : struct, Enum, IConvertible
	{
		value = ReadEnum<TEnum>();
	}

	public bool SerializeCheck(uint checkNumber, string message = null)
	{
		return ReadCheck(checkNumber, message);
	}

	public void SerializeAlign()
	{
		ReadAlign();
	}
}
