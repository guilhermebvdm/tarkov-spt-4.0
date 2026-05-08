using System;
using System.Collections.Generic;
using System.Threading;

public abstract class Class933
{
	[NonSerialized]
	public GClass1477 Gclass1477_0;

	[NonSerialized]
	public Queue<Class941> Queue_0;

	[NonSerialized]
	public int Int_0;

	public int PacketsInQueue => Queue_0.Count;

	public Class933(GClass1477 peer)
	{
		Gclass1477_0 = peer;
		Queue_0 = new Queue<Class941>(64);
	}

	public void AddToQueue(Class941 packet)
	{
		lock (Queue_0)
		{
			Queue_0.Enqueue(packet);
		}
		method_0();
	}

	public void method_0()
	{
		if (Interlocked.CompareExchange(ref Int_0, 1, 0) == 0)
		{
			Gclass1477_0.method_7(this);
		}
	}

	public bool SendAndCheckQueue()
	{
		bool num = vmethod_0();
		if (!num)
		{
			Interlocked.Exchange(ref Int_0, 0);
		}
		return num;
	}

	public abstract bool vmethod_0();

	public abstract bool ProcessPacket(Class941 packet);
}
