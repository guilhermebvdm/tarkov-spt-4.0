using System;
using System.Runtime.CompilerServices;

public class GException10 : Exception
{
	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public uint Uint_0;

	[NonSerialized]
	[CompilerGenerated]
	public byte[] Byte_0;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_1;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_2;

	public int Size
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
		[CompilerGenerated]
		set
		{
			Int_0 = value;
		}
	}

	public uint MaxSize
	{
		[CompilerGenerated]
		get
		{
			return Uint_0;
		}
		[CompilerGenerated]
		set
		{
			Uint_0 = value;
		}
	}

	public byte[] Bytes
	{
		[CompilerGenerated]
		get
		{
			return Byte_0;
		}
		[CompilerGenerated]
		set
		{
			Byte_0 = value;
		}
	}

	public int BitsRead
	{
		[CompilerGenerated]
		get
		{
			return Int_1;
		}
		[CompilerGenerated]
		set
		{
			Int_1 = value;
		}
	}

	public int BytesRead
	{
		[CompilerGenerated]
		get
		{
			return Int_2;
		}
		[CompilerGenerated]
		set
		{
			Int_2 = value;
		}
	}

	public GException10(int size, uint maxSize)
	{
		Size = size;
		MaxSize = maxSize;
	}

	public GException10(int size, uint maxSize, GInterface127 bitReader)
	{
		Size = size;
		MaxSize = maxSize;
		Bytes = bitReader.Buffer;
		BitsRead = bitReader.BitsRead;
		BytesRead = bitReader.BytesRead;
	}
}
