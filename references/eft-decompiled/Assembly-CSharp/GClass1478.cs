using System;
using System.Threading;

public class GClass1478
{
	[NonSerialized]
	public long Long_0;

	[NonSerialized]
	public long Long_1;

	[NonSerialized]
	public long Long_2;

	[NonSerialized]
	public long Long_3;

	[NonSerialized]
	public long Long_4;

	public long PacketsSent => Interlocked.Read(ref Long_0);

	public long PacketsReceived => Interlocked.Read(ref Long_1);

	public long BytesSent => Interlocked.Read(ref Long_2);

	public long BytesReceived => Interlocked.Read(ref Long_3);

	public long PacketLoss => Interlocked.Read(ref Long_4);

	public long PacketLossPercent
	{
		get
		{
			long packetsSent = PacketsSent;
			long packetLoss = PacketLoss;
			if (packetsSent != 0L)
			{
				return packetLoss * 100L / packetsSent;
			}
			return 0L;
		}
	}

	public void Reset()
	{
		Interlocked.Exchange(ref Long_0, 0L);
		Interlocked.Exchange(ref Long_1, 0L);
		Interlocked.Exchange(ref Long_2, 0L);
		Interlocked.Exchange(ref Long_3, 0L);
		Interlocked.Exchange(ref Long_4, 0L);
	}

	public void IncrementPacketsSent()
	{
		Interlocked.Increment(ref Long_0);
	}

	public void IncrementPacketsReceived()
	{
		Interlocked.Increment(ref Long_1);
	}

	public void AddBytesSent(long bytesSent)
	{
		Interlocked.Add(ref Long_2, bytesSent);
	}

	public void AddBytesReceived(long bytesReceived)
	{
		Interlocked.Add(ref Long_3, bytesReceived);
	}

	public void IncrementPacketLoss()
	{
		Interlocked.Increment(ref Long_4);
	}

	public void AddPacketLoss(long packetLoss)
	{
		Interlocked.Add(ref Long_4, packetLoss);
	}

	public override string ToString()
	{
		return $"BytesReceived: {BytesReceived}\nPacketsReceived: {PacketsReceived}\nBytesSent: {BytesSent}\nPacketsSent: {PacketsSent}\nPacketLoss: {PacketLoss}\nPacketLossPercent: {PacketLossPercent}\n";
	}
}
