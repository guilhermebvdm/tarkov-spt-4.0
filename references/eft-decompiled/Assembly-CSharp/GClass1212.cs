using System;
using System.Collections.Generic;

public abstract class GClass1212
{
	public const int BAKE_INDEX_BUFFER_SIZE = 65535;

	[NonSerialized]
	public static Queue<GClass1210> Queue_0;

	[NonSerialized]
	public static Queue<GClass1211> Queue_1;

	public static GClass1210 AllocateBakeBuffer()
	{
		if (Queue_0 == null)
		{
			Queue_0 = new Queue<GClass1210>();
		}
		if (Queue_0.Count == 0)
		{
			return new GClass1210(65535);
		}
		GClass1210 gClass = Queue_0.Dequeue();
		gClass.Reset();
		return gClass;
	}

	public static void ReuseBakeBuffer(GClass1210 buffer)
	{
		if (Queue_0 == null)
		{
			throw new InvalidOperationException("No buffers were allocated yet");
		}
		Queue_0.Enqueue(buffer);
	}

	public static GClass1211 AllocateExchangeBuffer()
	{
		if (Queue_1 == null)
		{
			Queue_1 = new Queue<GClass1211>();
		}
		if (Queue_1.Count == 0)
		{
			return new GClass1211(65535);
		}
		GClass1211 gClass = Queue_1.Dequeue();
		gClass.Reset();
		return gClass;
	}

	public static void ReuseExchangeBuffer(GClass1211 buffer)
	{
		if (Queue_1 == null)
		{
			throw new InvalidOperationException("No buffers were allocated yet");
		}
		Queue_1.Enqueue(buffer);
	}
}
