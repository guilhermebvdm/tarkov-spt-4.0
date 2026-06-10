using System;
using System.ComponentModel;
using System.IO;
using BitPacking;
using Newtonsoft.Json;
using UnityEngine;

namespace EFT;

[Serializable]
[JsonConverter(typeof(GClass2062))]
[TypeConverter(typeof(GClass2063))]
[GAttribute28("ReadMongoId", "WriteMongoId")]
public readonly struct MongoID : IComparable<MongoID>, IEquatable<MongoID>
{
	[NonSerialized]
	public static System.Random Random = new System.Random();

	[NonSerialized]
	public static ulong ProcessId = UInt64_0;

	[NonSerialized]
	public static uint NewIdCounter;

	[NonSerialized]
	public uint TimeStamp;

	[NonSerialized]
	public ulong Counter;

	[NonSerialized]
	public string StringID;

	public static uint UInt32_0 => Convert.ToUInt32(EFTDateTimeClass.UtcNowUnixInt);

	public static ulong UInt64_0 => (ulong)(((long)Random.Next(0, int.MaxValue) << 8) ^ Random.Next(0, int.MaxValue));

	public static MongoID Generate(bool newProcessId = true)
	{
		return new MongoID(newProcessId);
	}

	public static string smethod_0(uint timeStamp, ulong counter)
	{
		return timeStamp.ToString("X8").ToLower() + counter.ToString("X16").ToLower();
	}

	public static uint smethod_1(string id)
	{
		return Convert.ToUInt32(id.Substring(0, 8), 16);
	}

	public static ulong smethod_2(string id)
	{
		return Convert.ToUInt64(id.Substring(8, 16), 16);
	}

	[JsonConstructor]
	public MongoID([JsonProperty("$value")] string id)
	{
		if (id == null || id.Length != 24)
		{
			throw new ArgumentOutOfRangeException("Critical MongoId error: incorrect length. Id: " + id);
		}
		TimeStamp = smethod_1(id);
		Counter = smethod_2(id);
		StringID = id;
		method_0();
	}

	public MongoID(uint timeStamp, ulong counter)
	{
		TimeStamp = timeStamp;
		Counter = counter;
		StringID = null;
		StringID = method_1();
		method_0();
	}

	public MongoID(EFTReaderClass reader)
	{
		TimeStamp = GClass1285.ReadUInt(reader);
		Counter = GClass1285.ReadULong(reader);
		StringID = null;
		StringID = method_1();
		method_0();
	}

	public MongoID(BinaryReader reader)
	{
		TimeStamp = reader.ReadUInt32();
		Counter = reader.ReadUInt64();
		StringID = null;
		StringID = method_1();
		method_0();
	}

	public MongoID(bool newProcessId)
	{
		TimeStamp = 0u;
		StringID = null;
		if (newProcessId)
		{
			Counter = UInt64_0 << 24;
		}
		else
		{
			NewIdCounter++;
			Counter = (ProcessId << 24) + NewIdCounter;
		}
		TimeStamp = UInt32_0;
		StringID = method_1();
	}

	public MongoID(Profile profile)
	{
		StringID = null;
		TimeStamp = UInt32_0;
		uint num = Convert.ToUInt32(profile.AccountId);
		uint num2 = Convert.ToUInt32(Random.Next(0, 16777215));
		Counter = 0x100000000uL | (ulong)num;
		Counter <<= 24;
		Counter |= num2;
		StringID = method_1();
	}

	public MongoID(MongoID source, int increment, bool newTimestamp)
	{
		TimeStamp = (newTimestamp ? UInt32_0 : source.TimeStamp);
		Counter = ((increment > 0) ? (source.Counter + Convert.ToUInt32(increment)) : (source.Counter - Convert.ToUInt32(Mathf.Abs(increment))));
		StringID = null;
		StringID = method_1();
	}

	public MongoID(ISerializer stream)
	{
		if (stream.StreamMode == EBitStreamMode.Writing)
		{
			throw new Exception("Trying to read from a writing stream.");
		}
		TimeStamp = 0u;
		Counter = 0uL;
		stream.Serialize(ref TimeStamp);
		stream.Serialize(ref Counter);
		StringID = null;
		StringID = method_1();
		method_0();
	}

	public void method_0()
	{
		ulong num = Convert.ToUInt64(Counter >> 24);
		if (ProcessId == num)
		{
			uint val = Convert.ToUInt32(Counter << 40 >> 40);
			NewIdCounter = Math.Max(NewIdCounter, val);
		}
	}

	public MongoID Next()
	{
		return new MongoID(this, 1, newTimestamp: true);
	}

	public MongoID Add(int increment)
	{
		return new MongoID(this, increment, newTimestamp: false);
	}

	public MongoID Combine(MongoID id)
	{
		uint timeStamp = TimeStamp ^ id.TimeStamp;
		ulong counter = Counter ^ id.Counter;
		return new MongoID(timeStamp, counter);
	}

	public static MongoID Read(ISerializer stream)
	{
		return new MongoID(stream);
	}

	public static MongoID Read(EFTReaderClass reader)
	{
		return new MongoID(reader);
	}

	public static MongoID Read(BinaryReader reader)
	{
		return new MongoID(reader);
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(TimeStamp);
		writer.Write(Counter);
	}

	public void Write(ISerializer stream)
	{
		if (stream.StreamMode == EBitStreamMode.Reading)
		{
			throw new Exception("Trying to write into a reading stream.");
		}
		uint value = TimeStamp;
		ulong value2 = Counter;
		stream.Serialize(ref value);
		stream.Serialize(ref value2);
	}

	public void Write(EFTWriterClass writer)
	{
		GClass1290.WriteUInt(writer, TimeStamp);
		GClass1290.WriteULong(writer, Counter);
	}

	public bool Equals(MongoID other)
	{
		if (TimeStamp == other.TimeStamp)
		{
			return Counter == other.Counter;
		}
		return false;
	}

	public int CompareTo(MongoID other)
	{
		if (this == other)
		{
			return 0;
		}
		if (this > other)
		{
			return 1;
		}
		return -1;
	}

	public string method_1()
	{
		return smethod_0(TimeStamp, Counter);
	}

	public override string ToString()
	{
		return StringID ?? string.Empty;
	}

	public byte[] ToBytes()
	{
		byte[] array = new byte[12];
		for (int i = 0; i < 4; i++)
		{
			array[i] = (byte)(TimeStamp >> (3 - i) * 8);
		}
		for (int j = 0; j < 8; j++)
		{
			array[j + 4] = (byte)(Counter >> (7 - j) * 8);
		}
		return array;
	}

	public override bool Equals(object obj)
	{
		if (obj is MongoID mongoID)
		{
			return mongoID == this;
		}
		if (!(obj is string str))
		{
			return false;
		}
		return EqualsToString(str);
	}

	public bool EqualsToString(string str)
	{
		return str.Equals(ToString(), StringComparison.InvariantCultureIgnoreCase);
	}

	public override int GetHashCode()
	{
		uint num = Convert.ToUInt32(Counter >> 32) * 3637;
		uint num2 = Convert.ToUInt32(Counter << 32 >> 32) * 5807;
		return (int)(TimeStamp ^ num ^ num2);
	}

	public static implicit operator string(MongoID mongoId)
	{
		return mongoId.ToString();
	}

	public static implicit operator MongoID(string id)
	{
		return new MongoID(id);
	}

	public static implicit operator MongoID?(string id)
	{
		return string.IsNullOrEmpty(id) ? ((MongoID)null) : new MongoID(id);
	}

	public static MongoID operator ++(MongoID id)
	{
		return new MongoID(id, 1, newTimestamp: false);
	}

	public static MongoID operator --(MongoID id)
	{
		return new MongoID(id, -1, newTimestamp: false);
	}

	public static bool operator ==(MongoID a, MongoID b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(MongoID a, MongoID b)
	{
		return !a.Equals(b);
	}

	public static bool operator >(MongoID a, MongoID b)
	{
		if (a.TimeStamp <= b.TimeStamp)
		{
			if (a.TimeStamp == b.TimeStamp)
			{
				return a.Counter > b.Counter;
			}
			return false;
		}
		return true;
	}

	public static bool operator <(MongoID a, MongoID b)
	{
		if (a.TimeStamp >= b.TimeStamp)
		{
			if (a.TimeStamp == b.TimeStamp)
			{
				return a.Counter < b.Counter;
			}
			return false;
		}
		return true;
	}

	public static bool operator >=(MongoID a, MongoID b)
	{
		if (!(a == b))
		{
			return a > b;
		}
		return true;
	}

	public static bool operator <=(MongoID a, MongoID b)
	{
		if (!(a == b))
		{
			return a < b;
		}
		return true;
	}
}
You are not using the latest version of the tool, please update.
Latest version is '10.1.0.8386' (yours is '10.0.1.8346')
