using System;
using System.Linq;

public class RollingMedian
{
	[NonSerialized]
	public int BufferSize;

	[NonSerialized]
	public int BufferCount;

	[NonSerialized]
	public int Counter;

	[NonSerialized]
	public float[] Buffer;

	[NonSerialized]
	public float[] SortedBuffer;

	[NonSerialized]
	public bool Dirty = true;

	[NonSerialized]
	public float CachedAvarage;

	public float Median
	{
		get
		{
			Array.Copy(Buffer, SortedBuffer, Buffer.Length);
			Array.Sort(SortedBuffer);
			if (BufferSize % 2 == 1)
			{
				return SortedBuffer[BufferSize / 2];
			}
			return (SortedBuffer[BufferSize / 2 - 1] + SortedBuffer[BufferSize / 2]) * 0.5f;
		}
	}

	public float Avarage
	{
		get
		{
			if (BufferCount == 0)
			{
				return 0f;
			}
			if (Dirty)
			{
				Dirty = false;
				CachedAvarage = Buffer.Sum() / (float)BufferCount;
				return CachedAvarage;
			}
			return CachedAvarage;
		}
	}

	public RollingMedian(int bufferSize)
	{
		BufferSize = bufferSize;
		Buffer = new float[bufferSize];
		SortedBuffer = new float[bufferSize];
	}

	public void AddValue(float value)
	{
		Dirty = true;
		if (BufferCount < BufferSize)
		{
			BufferCount++;
		}
		Counter = (Counter + 1) % BufferSize;
		Buffer[Counter] = value;
	}
}
