using System;

public class GClass1483 : GClass1482
{
	[NonSerialized]
	public Class941 Class941_0;

	[NonSerialized]
	public GClass1476 Gclass1476_0;

	[NonSerialized]
	public Class939 Class939_0;

	public GClass1483(GClass1476 manager, Class939 evt)
	{
		Gclass1476_0 = manager;
		Class939_0 = evt;
	}

	public void method_0(Class941 packet, int headerSize)
	{
		if (packet != null)
		{
			Class941_0 = packet;
			SetSource(packet.RawData, headerSize, packet.Size);
		}
	}

	public void method_1()
	{
		Clear();
		if (Class941_0 != null)
		{
			Gclass1476_0.Class944_0.Recycle(Class941_0);
		}
		Class941_0 = null;
		Gclass1476_0.method_13(Class939_0);
	}

	public void Recycle()
	{
		if (Gclass1476_0.AutoRecycle)
		{
			throw new Exception("Recycle called with AutoRecycle enabled");
		}
		method_1();
	}
}
