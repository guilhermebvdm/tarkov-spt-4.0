using System;
using FlyingWormConsole3.LiteNetLib;

public class Class934 : Class933
{
	public struct Struct257
	{
		[NonSerialized]
		public Class941 Class941_0;

		[NonSerialized]
		public long Long_0;

		[NonSerialized]
		public bool Bool_0;

		public override string ToString()
		{
			if (Class941_0 != null)
			{
				return Class941_0.Sequence.ToString();
			}
			return "Empty";
		}

		public void Init(Class941 packet)
		{
			Class941_0 = packet;
			Bool_0 = false;
		}

		public void TrySend(long currentTime, GClass1477 peer, out bool hasPacket)
		{
			if (Class941_0 == null)
			{
				hasPacket = false;
				return;
			}
			hasPacket = true;
			if (Bool_0)
			{
				double num = peer.Double_0 * 10000.0;
				if ((double)(currentTime - Long_0) < num)
				{
					return;
				}
			}
			Long_0 = currentTime;
			Bool_0 = true;
			peer.method_16(Class941_0);
		}

		public bool Clear(GClass1477 peer)
		{
			if (Class941_0 != null)
			{
				peer.method_17(Class941_0);
				Class941_0 = null;
				return true;
			}
			return false;
		}
	}

	[NonSerialized]
	public Class941 Class941_0;

	[NonSerialized]
	public Struct257[] Struct257_0;

	[NonSerialized]
	public Class941[] Class941_1;

	[NonSerialized]
	public bool[] Bool_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public int Int_2;

	[NonSerialized]
	public int Int_3;

	[NonSerialized]
	public int Int_4;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public DeliveryMethod DeliveryMethod_0;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public int Int_5;

	[NonSerialized]
	public const int Int_6 = 8;

	[NonSerialized]
	public byte Byte_0;

	public Class934(GClass1477 peer, bool ordered, byte id)
		: base(peer)
	{
		Byte_0 = id;
		Int_5 = 64;
		Bool_2 = ordered;
		Struct257_0 = new Struct257[Int_5];
		for (int i = 0; i < Struct257_0.Length; i++)
		{
			Struct257_0[i] = default(Struct257);
		}
		if (Bool_2)
		{
			DeliveryMethod_0 = DeliveryMethod.ReliableOrdered;
			Class941_1 = new Class941[Int_5];
		}
		else
		{
			DeliveryMethod_0 = DeliveryMethod.ReliableUnordered;
			Bool_0 = new bool[Int_5];
		}
		Int_3 = 0;
		Int_1 = 0;
		Int_2 = 0;
		Int_4 = 0;
		Class941_0 = new Class941(PacketProperty.Ack, (Int_5 - 1) / 8 + 2)
		{
			ChannelId = id
		};
	}

	public void method_1(Class941 packet)
	{
		if (packet.Size != Class941_0.Size)
		{
			return;
		}
		ushort sequence = packet.Sequence;
		int num = GClass1479.smethod_1(Int_3, sequence);
		if (sequence >= 32768 || num < 0 || num >= Int_5)
		{
			return;
		}
		byte[] rawData = packet.RawData;
		lock (Struct257_0)
		{
			int num2 = Int_3;
			while (num2 != Int_1 && GClass1479.smethod_1(num2, sequence) < Int_5)
			{
				int num3 = num2 % Int_5;
				int num4 = 4 + num3 / 8;
				int num5 = num3 % 8;
				if ((rawData[num4] & (1 << num5)) == 0)
				{
					if (Gclass1477_0.NetManager.EnableStatistics)
					{
						Gclass1477_0.Statistics.IncrementPacketLoss();
						Gclass1477_0.NetManager.Statistics.IncrementPacketLoss();
					}
				}
				else
				{
					if (num2 == Int_3)
					{
						Int_3 = (Int_3 + 1) % 32768;
					}
					Struct257_0[num3].Clear(Gclass1477_0);
				}
				num2 = (num2 + 1) % 32768;
			}
		}
	}

	public override bool vmethod_0()
	{
		if (Bool_1)
		{
			Bool_1 = false;
			lock (Class941_0)
			{
				Gclass1477_0.method_16(Class941_0);
			}
		}
		long ticks = DateTime.UtcNow.Ticks;
		bool flag = false;
		lock (Struct257_0)
		{
			lock (Queue_0)
			{
				while (Queue_0.Count > 0 && GClass1479.smethod_1(Int_1, Int_3) < Int_5)
				{
					Class941 @class = Queue_0.Dequeue();
					@class.Sequence = (ushort)Int_1;
					@class.ChannelId = Byte_0;
					Struct257_0[Int_1 % Int_5].Init(@class);
					Int_1 = (Int_1 + 1) % 32768;
				}
			}
			for (int num = Int_3; num != Int_1; num = (num + 1) % 32768)
			{
				Struct257_0[num % Int_5].TrySend(ticks, Gclass1477_0, out var hasPacket);
				if (hasPacket)
				{
					flag = true;
				}
			}
		}
		if (!flag && !Bool_1)
		{
			return Queue_0.Count > 0;
		}
		return true;
	}

	public override bool ProcessPacket(Class941 packet)
	{
		if (packet.Property == PacketProperty.Ack)
		{
			method_1(packet);
			return false;
		}
		int sequence = packet.Sequence;
		if (sequence >= 32768)
		{
			return false;
		}
		int num = GClass1479.smethod_1(sequence, Int_4);
		if (GClass1479.smethod_1(sequence, Int_2) > Int_5)
		{
			return false;
		}
		if (num < 0)
		{
			return false;
		}
		if (num >= Int_5 * 2)
		{
			return false;
		}
		int num3;
		lock (Class941_0)
		{
			int num4;
			int num5;
			if (num >= Int_5)
			{
				int num2 = (Int_4 + num - Int_5 + 1) % 32768;
				Class941_0.Sequence = (ushort)num2;
				while (Int_4 != num2)
				{
					num3 = Int_4 % Int_5;
					num4 = 4 + num3 / 8;
					num5 = num3 % 8;
					Class941_0.RawData[num4] &= (byte)(~(1 << num5));
					Int_4 = (Int_4 + 1) % 32768;
				}
			}
			Bool_1 = true;
			num3 = sequence % Int_5;
			num4 = 4 + num3 / 8;
			num5 = num3 % 8;
			if ((Class941_0.RawData[num4] & (1 << num5)) != 0)
			{
				return false;
			}
			Class941_0.RawData[num4] |= (byte)(1 << num5);
		}
		method_0();
		if (sequence == Int_2)
		{
			Gclass1477_0.method_10(DeliveryMethod_0, packet);
			Int_2 = (Int_2 + 1) % 32768;
			if (Bool_2)
			{
				Class941 p;
				while ((p = Class941_1[Int_2 % Int_5]) != null)
				{
					Class941_1[Int_2 % Int_5] = null;
					Gclass1477_0.method_10(DeliveryMethod_0, p);
					Int_2 = (Int_2 + 1) % 32768;
				}
			}
			else
			{
				while (Bool_0[Int_2 % Int_5])
				{
					Bool_0[Int_2 % Int_5] = false;
					Int_2 = (Int_2 + 1) % 32768;
				}
			}
			return true;
		}
		if (Bool_2)
		{
			Class941_1[num3] = packet;
		}
		else
		{
			Bool_0[num3] = true;
			Gclass1477_0.method_10(DeliveryMethod_0, packet);
		}
		return true;
	}
}
