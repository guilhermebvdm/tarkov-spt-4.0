using System;
using System.Net;
using System.Text;

public class GClass1482
{
	[NonSerialized]
	public byte[] Byte_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public int Int_2;

	public byte[] RawData => Byte_0;

	public int RawDataSize => Int_1;

	public int UserDataOffset => Int_2;

	public int UserDataSize => Int_1 - Int_2;

	public bool IsNull => Byte_0 == null;

	public int Position => Int_0;

	public bool EndOfData => Int_0 == Int_1;

	public int AvailableBytes => Int_1 - Int_0;

	public void SkipBytes(int count)
	{
		Int_0 += count;
	}

	public void SetSource(GClass1484 dataWriter)
	{
		Byte_0 = dataWriter.Data;
		Int_0 = 0;
		Int_2 = 0;
		Int_1 = dataWriter.Length;
	}

	public void SetSource(byte[] source)
	{
		Byte_0 = source;
		Int_0 = 0;
		Int_2 = 0;
		Int_1 = source.Length;
	}

	public void SetSource(byte[] source, int offset)
	{
		Byte_0 = source;
		Int_0 = offset;
		Int_2 = offset;
		Int_1 = source.Length;
	}

	public void SetSource(byte[] source, int offset, int maxSize)
	{
		Byte_0 = source;
		Int_0 = offset;
		Int_2 = offset;
		Int_1 = maxSize;
	}

	public GClass1482()
	{
	}

	public GClass1482(GClass1484 writer)
	{
		SetSource(writer);
	}

	public GClass1482(byte[] source)
	{
		SetSource(source);
	}

	public GClass1482(byte[] source, int offset)
	{
		SetSource(source, offset);
	}

	public GClass1482(byte[] source, int offset, int maxSize)
	{
		SetSource(source, offset, maxSize);
	}

	public IPEndPoint GetNetEndPoint()
	{
		string hostStr = GetString(1000);
		int port = GetInt();
		return GClass1479.MakeEndPoint(hostStr, port);
	}

	public byte GetByte()
	{
		byte result = Byte_0[Int_0];
		Int_0++;
		return result;
	}

	public sbyte GetSByte()
	{
		sbyte result = (sbyte)Byte_0[Int_0];
		Int_0++;
		return result;
	}

	public bool[] GetBoolArray()
	{
		ushort num = BitConverter.ToUInt16(Byte_0, Int_0);
		Int_0 += 2;
		bool[] array = new bool[num];
		Buffer.BlockCopy(Byte_0, Int_0, array, 0, num);
		Int_0 += num;
		return array;
	}

	public ushort[] GetUShortArray()
	{
		ushort num = BitConverter.ToUInt16(Byte_0, Int_0);
		Int_0 += 2;
		ushort[] array = new ushort[num];
		Buffer.BlockCopy(Byte_0, Int_0, array, 0, num * 2);
		Int_0 += num * 2;
		return array;
	}

	public short[] GetShortArray()
	{
		ushort num = BitConverter.ToUInt16(Byte_0, Int_0);
		Int_0 += 2;
		short[] array = new short[num];
		Buffer.BlockCopy(Byte_0, Int_0, array, 0, num * 2);
		Int_0 += num * 2;
		return array;
	}

	public long[] GetLongArray()
	{
		ushort num = BitConverter.ToUInt16(Byte_0, Int_0);
		Int_0 += 2;
		long[] array = new long[num];
		Buffer.BlockCopy(Byte_0, Int_0, array, 0, num * 8);
		Int_0 += num * 8;
		return array;
	}

	public ulong[] GetULongArray()
	{
		ushort num = BitConverter.ToUInt16(Byte_0, Int_0);
		Int_0 += 2;
		ulong[] array = new ulong[num];
		Buffer.BlockCopy(Byte_0, Int_0, array, 0, num * 8);
		Int_0 += num * 8;
		return array;
	}

	public int[] GetIntArray()
	{
		ushort num = BitConverter.ToUInt16(Byte_0, Int_0);
		Int_0 += 2;
		int[] array = new int[num];
		Buffer.BlockCopy(Byte_0, Int_0, array, 0, num * 4);
		Int_0 += num * 4;
		return array;
	}

	public uint[] GetUIntArray()
	{
		ushort num = BitConverter.ToUInt16(Byte_0, Int_0);
		Int_0 += 2;
		uint[] array = new uint[num];
		Buffer.BlockCopy(Byte_0, Int_0, array, 0, num * 4);
		Int_0 += num * 4;
		return array;
	}

	public float[] GetFloatArray()
	{
		ushort num = BitConverter.ToUInt16(Byte_0, Int_0);
		Int_0 += 2;
		float[] array = new float[num];
		Buffer.BlockCopy(Byte_0, Int_0, array, 0, num * 4);
		Int_0 += num * 4;
		return array;
	}

	public double[] GetDoubleArray()
	{
		ushort num = BitConverter.ToUInt16(Byte_0, Int_0);
		Int_0 += 2;
		double[] array = new double[num];
		Buffer.BlockCopy(Byte_0, Int_0, array, 0, num * 8);
		Int_0 += num * 8;
		return array;
	}

	public string[] GetStringArray()
	{
		ushort num = BitConverter.ToUInt16(Byte_0, Int_0);
		Int_0 += 2;
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = GetString();
		}
		return array;
	}

	public string[] GetStringArray(int maxStringLength)
	{
		ushort num = BitConverter.ToUInt16(Byte_0, Int_0);
		Int_0 += 2;
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = GetString(maxStringLength);
		}
		return array;
	}

	public bool GetBool()
	{
		bool result = Byte_0[Int_0] > 0;
		Int_0++;
		return result;
	}

	public char GetChar()
	{
		char result = BitConverter.ToChar(Byte_0, Int_0);
		Int_0 += 2;
		return result;
	}

	public ushort GetUShort()
	{
		ushort result = BitConverter.ToUInt16(Byte_0, Int_0);
		Int_0 += 2;
		return result;
	}

	public short GetShort()
	{
		short result = BitConverter.ToInt16(Byte_0, Int_0);
		Int_0 += 2;
		return result;
	}

	public long GetLong()
	{
		long result = BitConverter.ToInt64(Byte_0, Int_0);
		Int_0 += 8;
		return result;
	}

	public ulong GetULong()
	{
		ulong result = BitConverter.ToUInt64(Byte_0, Int_0);
		Int_0 += 8;
		return result;
	}

	public int GetInt()
	{
		int result = BitConverter.ToInt32(Byte_0, Int_0);
		Int_0 += 4;
		return result;
	}

	public uint GetUInt()
	{
		uint result = BitConverter.ToUInt32(Byte_0, Int_0);
		Int_0 += 4;
		return result;
	}

	public float GetFloat()
	{
		float result = BitConverter.ToSingle(Byte_0, Int_0);
		Int_0 += 4;
		return result;
	}

	public double GetDouble()
	{
		double result = BitConverter.ToDouble(Byte_0, Int_0);
		Int_0 += 8;
		return result;
	}

	public string GetString(int maxLength)
	{
		int num = GetInt();
		if (num > 0 && num <= maxLength * 2)
		{
			if (Encoding.UTF8.GetCharCount(Byte_0, Int_0, num) > maxLength)
			{
				return string.Empty;
			}
			string result = Encoding.UTF8.GetString(Byte_0, Int_0, num);
			Int_0 += num;
			return result;
		}
		return string.Empty;
	}

	public string GetString()
	{
		int num = GetInt();
		if (num <= 0)
		{
			return string.Empty;
		}
		string result = Encoding.UTF8.GetString(Byte_0, Int_0, num);
		Int_0 += num;
		return result;
	}

	public ArraySegment<byte> GetRemainingBytesSegment()
	{
		ArraySegment<byte> result = new ArraySegment<byte>(Byte_0, Int_0, AvailableBytes);
		Int_0 = Byte_0.Length;
		return result;
	}

	public T Get<T>() where T : GInterface143, new()
	{
		T result = new T();
		result.Deserialize(this);
		return result;
	}

	public byte[] GetRemainingBytes()
	{
		byte[] array = new byte[AvailableBytes];
		Buffer.BlockCopy(Byte_0, Int_0, array, 0, AvailableBytes);
		Int_0 = Byte_0.Length;
		return array;
	}

	public void GetBytes(byte[] destination, int start, int count)
	{
		Buffer.BlockCopy(Byte_0, Int_0, destination, start, count);
		Int_0 += count;
	}

	public void GetBytes(byte[] destination, int count)
	{
		Buffer.BlockCopy(Byte_0, Int_0, destination, 0, count);
		Int_0 += count;
	}

	public sbyte[] GetSBytesWithLength()
	{
		int num = GetInt();
		sbyte[] array = new sbyte[num];
		Buffer.BlockCopy(Byte_0, Int_0, array, 0, num);
		Int_0 += num;
		return array;
	}

	public byte[] GetBytesWithLength()
	{
		int num = GetInt();
		byte[] array = new byte[num];
		Buffer.BlockCopy(Byte_0, Int_0, array, 0, num);
		Int_0 += num;
		return array;
	}

	public byte PeekByte()
	{
		return Byte_0[Int_0];
	}

	public sbyte PeekSByte()
	{
		return (sbyte)Byte_0[Int_0];
	}

	public bool PeekBool()
	{
		return Byte_0[Int_0] > 0;
	}

	public char PeekChar()
	{
		return BitConverter.ToChar(Byte_0, Int_0);
	}

	public ushort PeekUShort()
	{
		return BitConverter.ToUInt16(Byte_0, Int_0);
	}

	public short PeekShort()
	{
		return BitConverter.ToInt16(Byte_0, Int_0);
	}

	public long PeekLong()
	{
		return BitConverter.ToInt64(Byte_0, Int_0);
	}

	public ulong PeekULong()
	{
		return BitConverter.ToUInt64(Byte_0, Int_0);
	}

	public int PeekInt()
	{
		return BitConverter.ToInt32(Byte_0, Int_0);
	}

	public uint PeekUInt()
	{
		return BitConverter.ToUInt32(Byte_0, Int_0);
	}

	public float PeekFloat()
	{
		return BitConverter.ToSingle(Byte_0, Int_0);
	}

	public double PeekDouble()
	{
		return BitConverter.ToDouble(Byte_0, Int_0);
	}

	public string PeekString(int maxLength)
	{
		int num = BitConverter.ToInt32(Byte_0, Int_0);
		if (num > 0 && num <= maxLength * 2)
		{
			if (Encoding.UTF8.GetCharCount(Byte_0, Int_0 + 4, num) > maxLength)
			{
				return string.Empty;
			}
			return Encoding.UTF8.GetString(Byte_0, Int_0 + 4, num);
		}
		return string.Empty;
	}

	public string PeekString()
	{
		int num = BitConverter.ToInt32(Byte_0, Int_0);
		if (num <= 0)
		{
			return string.Empty;
		}
		return Encoding.UTF8.GetString(Byte_0, Int_0 + 4, num);
	}

	public bool TryGetByte(out byte result)
	{
		if (AvailableBytes >= 1)
		{
			result = GetByte();
			return true;
		}
		result = 0;
		return false;
	}

	public bool TryGetSByte(out sbyte result)
	{
		if (AvailableBytes >= 1)
		{
			result = GetSByte();
			return true;
		}
		result = 0;
		return false;
	}

	public bool TryGetBool(out bool result)
	{
		if (AvailableBytes >= 1)
		{
			result = GetBool();
			return true;
		}
		result = false;
		return false;
	}

	public bool TryGetChar(out char result)
	{
		if (AvailableBytes >= 2)
		{
			result = GetChar();
			return true;
		}
		result = '\0';
		return false;
	}

	public bool TryGetShort(out short result)
	{
		if (AvailableBytes >= 2)
		{
			result = GetShort();
			return true;
		}
		result = 0;
		return false;
	}

	public bool TryGetUShort(out ushort result)
	{
		if (AvailableBytes >= 2)
		{
			result = GetUShort();
			return true;
		}
		result = 0;
		return false;
	}

	public bool TryGetInt(out int result)
	{
		if (AvailableBytes >= 4)
		{
			result = GetInt();
			return true;
		}
		result = 0;
		return false;
	}

	public bool TryGetUInt(out uint result)
	{
		if (AvailableBytes >= 4)
		{
			result = GetUInt();
			return true;
		}
		result = 0u;
		return false;
	}

	public bool TryGetLong(out long result)
	{
		if (AvailableBytes >= 8)
		{
			result = GetLong();
			return true;
		}
		result = 0L;
		return false;
	}

	public bool TryGetULong(out ulong result)
	{
		if (AvailableBytes >= 8)
		{
			result = GetULong();
			return true;
		}
		result = 0uL;
		return false;
	}

	public bool TryGetFloat(out float result)
	{
		if (AvailableBytes >= 4)
		{
			result = GetFloat();
			return true;
		}
		result = 0f;
		return false;
	}

	public bool TryGetDouble(out double result)
	{
		if (AvailableBytes >= 8)
		{
			result = GetDouble();
			return true;
		}
		result = 0.0;
		return false;
	}

	public bool TryGetString(out string result)
	{
		if (AvailableBytes >= 4)
		{
			int num = PeekInt();
			if (AvailableBytes >= num + 4)
			{
				result = GetString();
				return true;
			}
		}
		result = null;
		return false;
	}

	public bool TryGetStringArray(out string[] result)
	{
		if (!TryGetUShort(out var result2))
		{
			result = null;
			return false;
		}
		result = new string[result2];
		int num = 0;
		while (true)
		{
			if (num < result2)
			{
				if (!TryGetString(out result[num]))
				{
					break;
				}
				num++;
				continue;
			}
			return true;
		}
		result = null;
		return false;
	}

	public bool TryGetBytesWithLength(out byte[] result)
	{
		if (AvailableBytes >= 4)
		{
			int num = PeekInt();
			if (num >= 0 && AvailableBytes >= num + 4)
			{
				result = GetBytesWithLength();
				return true;
			}
		}
		result = null;
		return false;
	}

	public void Clear()
	{
		Int_0 = 0;
		Int_1 = 0;
		Byte_0 = null;
	}
}
