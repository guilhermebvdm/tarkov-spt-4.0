using System;
using System.Net;
using System.Text;

public class GClass1484
{
	[NonSerialized]
	public byte[] Byte_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public const int Int_1 = 64;

	[NonSerialized]
	public bool Bool_0;

	public int Capacity => Byte_0.Length;

	public byte[] Data => Byte_0;

	public int Length => Int_0;

	public GClass1484()
		: this(autoResize: true, 64)
	{
	}

	public GClass1484(bool autoResize)
		: this(autoResize, 64)
	{
	}

	public GClass1484(bool autoResize, int initialSize)
	{
		Byte_0 = new byte[initialSize];
		Bool_0 = autoResize;
	}

	public static GClass1484 FromBytes(byte[] bytes, bool copy)
	{
		if (copy)
		{
			GClass1484 gClass = new GClass1484(autoResize: true, bytes.Length);
			gClass.Put(bytes);
			return gClass;
		}
		return new GClass1484(autoResize: true, 0)
		{
			Byte_0 = bytes,
			Int_0 = bytes.Length
		};
	}

	public static GClass1484 FromBytes(byte[] bytes, int offset, int length)
	{
		GClass1484 gClass = new GClass1484(autoResize: true, bytes.Length);
		gClass.Put(bytes, offset, length);
		return gClass;
	}

	public static GClass1484 FromString(string value)
	{
		GClass1484 gClass = new GClass1484();
		gClass.Put(value);
		return gClass;
	}

	public void ResizeIfNeed(int newSize)
	{
		int num = Byte_0.Length;
		if (num < newSize)
		{
			while (num < newSize)
			{
				num *= 2;
			}
			Array.Resize(ref Byte_0, num);
		}
	}

	public void Reset(int size)
	{
		ResizeIfNeed(size);
		Int_0 = 0;
	}

	public void Reset()
	{
		Int_0 = 0;
	}

	public byte[] CopyData()
	{
		byte[] array = new byte[Int_0];
		Buffer.BlockCopy(Byte_0, 0, array, 0, Int_0);
		return array;
	}

	public int SetPosition(int position)
	{
		int int_ = Int_0;
		Int_0 = position;
		return int_;
	}

	public void Put(float value)
	{
		if (Bool_0)
		{
			ResizeIfNeed(Int_0 + 4);
		}
		GClass1481.GetBytes(Byte_0, Int_0, value);
		Int_0 += 4;
	}

	public void Put(double value)
	{
		if (Bool_0)
		{
			ResizeIfNeed(Int_0 + 8);
		}
		GClass1481.GetBytes(Byte_0, Int_0, value);
		Int_0 += 8;
	}

	public void Put(long value)
	{
		if (Bool_0)
		{
			ResizeIfNeed(Int_0 + 8);
		}
		GClass1481.GetBytes(Byte_0, Int_0, value);
		Int_0 += 8;
	}

	public void Put(ulong value)
	{
		if (Bool_0)
		{
			ResizeIfNeed(Int_0 + 8);
		}
		GClass1481.GetBytes(Byte_0, Int_0, value);
		Int_0 += 8;
	}

	public void Put(int value)
	{
		if (Bool_0)
		{
			ResizeIfNeed(Int_0 + 4);
		}
		GClass1481.GetBytes(Byte_0, Int_0, value);
		Int_0 += 4;
	}

	public void Put(uint value)
	{
		if (Bool_0)
		{
			ResizeIfNeed(Int_0 + 4);
		}
		GClass1481.GetBytes(Byte_0, Int_0, value);
		Int_0 += 4;
	}

	public void Put(char value)
	{
		if (Bool_0)
		{
			ResizeIfNeed(Int_0 + 2);
		}
		GClass1481.GetBytes(Byte_0, Int_0, value);
		Int_0 += 2;
	}

	public void Put(ushort value)
	{
		if (Bool_0)
		{
			ResizeIfNeed(Int_0 + 2);
		}
		GClass1481.GetBytes(Byte_0, Int_0, value);
		Int_0 += 2;
	}

	public void Put(short value)
	{
		if (Bool_0)
		{
			ResizeIfNeed(Int_0 + 2);
		}
		GClass1481.GetBytes(Byte_0, Int_0, value);
		Int_0 += 2;
	}

	public void Put(sbyte value)
	{
		if (Bool_0)
		{
			ResizeIfNeed(Int_0 + 1);
		}
		Byte_0[Int_0] = (byte)value;
		Int_0++;
	}

	public void Put(byte value)
	{
		if (Bool_0)
		{
			ResizeIfNeed(Int_0 + 1);
		}
		Byte_0[Int_0] = value;
		Int_0++;
	}

	public void Put(byte[] data, int offset, int length)
	{
		if (Bool_0)
		{
			ResizeIfNeed(Int_0 + length);
		}
		Buffer.BlockCopy(data, offset, Byte_0, Int_0, length);
		Int_0 += length;
	}

	public void Put(byte[] data)
	{
		if (Bool_0)
		{
			ResizeIfNeed(Int_0 + data.Length);
		}
		Buffer.BlockCopy(data, 0, Byte_0, Int_0, data.Length);
		Int_0 += data.Length;
	}

	public void PutSBytesWithLength(sbyte[] data, int offset, int length)
	{
		if (Bool_0)
		{
			ResizeIfNeed(Int_0 + length + 4);
		}
		GClass1481.GetBytes(Byte_0, Int_0, length);
		Buffer.BlockCopy(data, offset, Byte_0, Int_0 + 4, length);
		Int_0 += length + 4;
	}

	public void PutSBytesWithLength(sbyte[] data)
	{
		if (Bool_0)
		{
			ResizeIfNeed(Int_0 + data.Length + 4);
		}
		GClass1481.GetBytes(Byte_0, Int_0, data.Length);
		Buffer.BlockCopy(data, 0, Byte_0, Int_0 + 4, data.Length);
		Int_0 += data.Length + 4;
	}

	public void PutBytesWithLength(byte[] data, int offset, int length)
	{
		if (Bool_0)
		{
			ResizeIfNeed(Int_0 + length + 4);
		}
		GClass1481.GetBytes(Byte_0, Int_0, length);
		Buffer.BlockCopy(data, offset, Byte_0, Int_0 + 4, length);
		Int_0 += length + 4;
	}

	public void PutBytesWithLength(byte[] data)
	{
		if (Bool_0)
		{
			ResizeIfNeed(Int_0 + data.Length + 4);
		}
		GClass1481.GetBytes(Byte_0, Int_0, data.Length);
		Buffer.BlockCopy(data, 0, Byte_0, Int_0 + 4, data.Length);
		Int_0 += data.Length + 4;
	}

	public void Put(bool value)
	{
		if (Bool_0)
		{
			ResizeIfNeed(Int_0 + 1);
		}
		Byte_0[Int_0] = (byte)(value ? 1u : 0u);
		Int_0++;
	}

	public void method_0(Array arr, int sz)
	{
		ushort num = (ushort)((arr != null) ? ((ushort)arr.Length) : 0);
		sz *= num;
		if (Bool_0)
		{
			ResizeIfNeed(Int_0 + sz + 2);
		}
		GClass1481.GetBytes(Byte_0, Int_0, num);
		if (arr != null)
		{
			Buffer.BlockCopy(arr, 0, Byte_0, Int_0 + 2, sz);
		}
		Int_0 += sz + 2;
	}

	public void PutArray(float[] value)
	{
		method_0(value, 4);
	}

	public void PutArray(double[] value)
	{
		method_0(value, 8);
	}

	public void PutArray(long[] value)
	{
		method_0(value, 8);
	}

	public void PutArray(ulong[] value)
	{
		method_0(value, 8);
	}

	public void PutArray(int[] value)
	{
		method_0(value, 4);
	}

	public void PutArray(uint[] value)
	{
		method_0(value, 4);
	}

	public void PutArray(ushort[] value)
	{
		method_0(value, 2);
	}

	public void PutArray(short[] value)
	{
		method_0(value, 2);
	}

	public void PutArray(bool[] value)
	{
		method_0(value, 1);
	}

	public void PutArray(string[] value)
	{
		ushort num = (ushort)((value != null) ? ((ushort)value.Length) : 0);
		Put(num);
		for (int i = 0; i < num; i++)
		{
			Put(value[i]);
		}
	}

	public void PutArray(string[] value, int maxLength)
	{
		ushort num = (ushort)((value != null) ? ((ushort)value.Length) : 0);
		Put(num);
		for (int i = 0; i < num; i++)
		{
			Put(value[i], maxLength);
		}
	}

	public void Put(IPEndPoint endPoint)
	{
		Put(endPoint.Address.ToString());
		Put(endPoint.Port);
	}

	public void Put(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			Put(0);
			return;
		}
		int byteCount = Encoding.UTF8.GetByteCount(value);
		if (Bool_0)
		{
			ResizeIfNeed(Int_0 + byteCount + 4);
		}
		Put(byteCount);
		Encoding.UTF8.GetBytes(value, 0, value.Length, Byte_0, Int_0);
		Int_0 += byteCount;
	}

	public void Put(string value, int maxLength)
	{
		if (string.IsNullOrEmpty(value))
		{
			Put(0);
			return;
		}
		int charCount = ((value.Length > maxLength) ? maxLength : value.Length);
		int byteCount = Encoding.UTF8.GetByteCount(value);
		if (Bool_0)
		{
			ResizeIfNeed(Int_0 + byteCount + 4);
		}
		Put(byteCount);
		Encoding.UTF8.GetBytes(value, 0, charCount, Byte_0, Int_0);
		Int_0 += byteCount;
	}

	public void Put<T>(T obj) where T : GInterface143
	{
		obj.Serialize(this);
	}
}
