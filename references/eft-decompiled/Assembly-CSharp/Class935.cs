using System;
using FlyingWormConsole3.LiteNetLib;

public class Class935 : Class933
{
	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public ushort Ushort_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public Class941 Class941_0;

	[NonSerialized]
	public Class941 Class941_1;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public byte Byte_0;

	[NonSerialized]
	public long Long_0;

	public Class935(GClass1477 peer, bool reliable, byte id)
		: base(peer)
	{
		Byte_0 = id;
		Bool_0 = reliable;
		if (Bool_0)
		{
			Class941_1 = new Class941(PacketProperty.Ack, 0)
			{
				ChannelId = id
			};
		}
	}

	public override bool vmethod_0()
	{
		if (Bool_0 && Queue_0.Count == 0)
		{
			long ticks = DateTime.UtcNow.Ticks;
			if ((double)(ticks - Long_0) >= Gclass1477_0.Double_0 * 10000.0)
			{
				Class941 class941_ = Class941_0;
				if (class941_ != null)
				{
					Long_0 = ticks;
					Gclass1477_0.method_16(class941_);
				}
			}
		}
		else
		{
			lock (Queue_0)
			{
				while (Queue_0.Count > 0)
				{
					Class941 @class = Queue_0.Dequeue();
					Int_1 = (Int_1 + 1) % 32768;
					@class.Sequence = (ushort)Int_1;
					@class.ChannelId = Byte_0;
					Gclass1477_0.method_16(@class);
					if (Bool_0 && Queue_0.Count == 0)
					{
						Long_0 = DateTime.UtcNow.Ticks;
						Class941_0 = @class;
					}
					else
					{
						Gclass1477_0.NetManager.Class944_0.Recycle(@class);
					}
				}
			}
		}
		if (Bool_0 && Bool_1)
		{
			Bool_1 = false;
			Class941_1.Sequence = Ushort_0;
			Gclass1477_0.method_16(Class941_1);
		}
		return Class941_0 != null;
	}

	public override bool ProcessPacket(Class941 packet)
	{
		if (packet.IsFragmented)
		{
			return false;
		}
		if (packet.Property == PacketProperty.Ack)
		{
			if (Bool_0 && Class941_0 != null && packet.Sequence == Class941_0.Sequence)
			{
				Class941_0 = null;
			}
			return false;
		}
		int num = GClass1479.smethod_1(packet.Sequence, Ushort_0);
		bool result = false;
		if (packet.Sequence < 32768 && num > 0)
		{
			if (Gclass1477_0.NetManager.EnableStatistics)
			{
				Gclass1477_0.Statistics.AddPacketLoss(num - 1);
				Gclass1477_0.NetManager.Statistics.AddPacketLoss(num - 1);
			}
			Ushort_0 = packet.Sequence;
			Gclass1477_0.NetManager.method_22(packet, (!Bool_0) ? DeliveryMethod.Sequenced : DeliveryMethod.ReliableSequenced, 4, Gclass1477_0);
			result = true;
		}
		if (Bool_0)
		{
			Bool_1 = true;
			method_0();
		}
		return result;
	}
}
