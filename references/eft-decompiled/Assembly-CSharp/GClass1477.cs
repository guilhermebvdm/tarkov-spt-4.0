using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FlyingWormConsole3.LiteNetLib;

public class GClass1477
{
	public class Class945
	{
		public Class941[] Fragments;

		public int ReceivedCount;

		public int TotalSize;

		public byte ChannelId;
	}

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public int Int_2;

	[NonSerialized]
	public double Double_0_1 = 27.0;

	[NonSerialized]
	public int Int_3;

	[NonSerialized]
	public int Int_4;

	[NonSerialized]
	public Stopwatch Stopwatch_0 = new Stopwatch();

	[NonSerialized]
	public int Int_5;

	[NonSerialized]
	public long Long_0;

	[NonSerialized]
	public Class944 Class944_0;

	[NonSerialized]
	public object Object_0 = new object();

	[NonSerialized]
	public volatile GClass1477 Gclass1477_0;

	[NonSerialized]
	public GClass1477 Gclass1477_1;

	[NonSerialized]
	public Queue<Class941> Queue_0;

	[NonSerialized]
	public Queue<Class933> Queue_1;

	[NonSerialized]
	public Class933[] Class933_0;

	[NonSerialized]
	public int Int_6;

	[NonSerialized]
	public int Int_7;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public int Int_8;

	[NonSerialized]
	public int Int_9;

	[NonSerialized]
	public const int Int_10 = 1000;

	[NonSerialized]
	public const int Int_11 = 4;

	[NonSerialized]
	public object Object_1 = new object();

	[NonSerialized]
	public int Int_12;

	[NonSerialized]
	public Dictionary<ushort, Class945> Dictionary_0;

	[NonSerialized]
	public Dictionary<ushort, ushort> Dictionary_1;

	[NonSerialized]
	public Class941 Class941_0;

	[NonSerialized]
	public int Int_13;

	[NonSerialized]
	public int Int_14;

	[NonSerialized]
	public int Int_15;

	[NonSerialized]
	public int Int_16;

	[NonSerialized]
	public long Long_1;

	[NonSerialized]
	public byte Byte_0_1;

	[NonSerialized]
	public ConnectionState ConnectionState_0;

	[NonSerialized]
	public Class941 Class941_1;

	[NonSerialized]
	public const int Int_17 = 300;

	[NonSerialized]
	public int Int_18;

	[NonSerialized]
	public Class941 Class941_2;

	[NonSerialized]
	public Class941 Class941_3;

	[NonSerialized]
	public Class941 Class941_4;

	[NonSerialized]
	public Class941 Class941_5;

	public readonly IPEndPoint EndPoint;

	public readonly GClass1476 NetManager;

	public readonly int Id;

	public object Tag;

	public readonly GClass1478 Statistics;

	public byte Byte_0
	{
		get
		{
			return Byte_0_1;
		}
		set
		{
			Byte_0_1 = value;
			Class941_0.ConnectionNumber = value;
			Class941_2.ConnectionNumber = value;
			Class941_3.ConnectionNumber = value;
		}
	}

	public ConnectionState ConnectionState => ConnectionState_0;

	public long Int64_0 => Long_1;

	public int Ping => Int_1 / 2;

	public int Mtu => Int_6;

	public long RemoteTimeDelta => Long_0;

	public DateTime RemoteUtcTime => new DateTime(DateTime.UtcNow.Ticks + Long_0);

	public int TimeSinceLastPacket => Int_5;

	public double Double_0 => Double_0_1;

	public GClass1477(GClass1476 netManager, IPEndPoint remoteEndPoint, int id)
	{
		Id = id;
		Statistics = new GClass1478();
		Class944_0 = netManager.Class944_0;
		NetManager = netManager;
		if (netManager.MtuOverride > 0)
		{
			method_1(netManager.MtuOverride);
		}
		else if (netManager.UseSafeMtu)
		{
			method_0(0);
		}
		else
		{
			method_0(1);
		}
		EndPoint = remoteEndPoint;
		ConnectionState_0 = ConnectionState.Connected;
		Class941_0 = new Class941(PacketProperty.Merged, GClass1474.Int_3);
		Class941_3 = new Class941(PacketProperty.Pong, 0);
		Class941_2 = new Class941(PacketProperty.Ping, 0)
		{
			Sequence = 1
		};
		Queue_0 = new Queue<Class941>(64);
		Dictionary_0 = new Dictionary<ushort, Class945>();
		Dictionary_1 = new Dictionary<ushort, ushort>();
		Class933_0 = new Class933[netManager.ChannelsCount * 4];
		Queue_1 = new Queue<Class933>(netManager.ChannelsCount * 4);
	}

	public void method_0(int mtuIdx)
	{
		Int_7 = mtuIdx;
		Int_6 = GClass1474.Int_2[mtuIdx] - NetManager.ExtraPacketSizeForLayer;
	}

	public void method_1(int mtuValue)
	{
		Int_6 = mtuValue;
		Bool_0 = true;
	}

	public int GetPacketsCountInReliableQueue(byte channelNumber, bool ordered)
	{
		int num = channelNumber * 4 + (ordered ? 2 : 0);
		Class933 @class = Class933_0[num];
		if (@class == null)
		{
			return 0;
		}
		return ((Class934)@class).PacketsInQueue;
	}

	public Class933 method_2(byte idx)
	{
		Class933 @class = Class933_0[idx];
		if (@class != null)
		{
			return @class;
		}
		switch ((DeliveryMethod)(byte)(idx % 4))
		{
		case DeliveryMethod.ReliableUnordered:
			@class = new Class934(this, ordered: false, idx);
			break;
		case DeliveryMethod.Sequenced:
			@class = new Class935(this, reliable: false, idx);
			break;
		case DeliveryMethod.ReliableOrdered:
			@class = new Class934(this, ordered: true, idx);
			break;
		case DeliveryMethod.ReliableSequenced:
			@class = new Class935(this, reliable: true, idx);
			break;
		}
		Class933 class2 = Interlocked.CompareExchange(ref Class933_0[idx], @class, null);
		if (class2 != null)
		{
			return class2;
		}
		return @class;
	}

	public GClass1477(GClass1476 netManager, IPEndPoint remoteEndPoint, int id, byte connectNum, GClass1484 connectData)
		: this(netManager, remoteEndPoint, id)
	{
		Long_1 = DateTime.UtcNow.Ticks;
		ConnectionState_0 = ConnectionState.Outgoing;
		Byte_0 = connectNum;
		Class941_4 = Class942.Make(connectData, remoteEndPoint.Serialize(), Long_1);
		Class941_4.ConnectionNumber = connectNum;
		NetManager.method_7(Class941_4, EndPoint);
	}

	public GClass1477(GClass1476 netManager, IPEndPoint remoteEndPoint, int id, long connectId, byte connectNum)
		: this(netManager, remoteEndPoint, id)
	{
		Long_1 = connectId;
		ConnectionState_0 = ConnectionState.Connected;
		Byte_0 = connectNum;
		Class941_5 = Class943.Make(Long_1, connectNum, reusedPeer: false);
		NetManager.method_7(Class941_5, EndPoint);
	}

	public void method_3(long connectionId, byte connectionNumber, byte[] data, int start, int length)
	{
		Long_1 = connectionId;
		Byte_0_1 = connectionNumber;
		method_8(data, start, length, force: false);
	}

	public bool method_4(Class943 packet)
	{
		if (ConnectionState_0 != ConnectionState.Outgoing)
		{
			return false;
		}
		if (packet.ConnectionId != Long_1)
		{
			return false;
		}
		Byte_0 = packet.ConnectionNumber;
		Interlocked.Exchange(ref Int_5, 0);
		ConnectionState_0 = ConnectionState.Connected;
		return true;
	}

	public int GetMaxSinglePacketSize(DeliveryMethod options)
	{
		return Int_6 - Class941.GetHeaderSize((options != DeliveryMethod.Unreliable) ? PacketProperty.Channeled : PacketProperty.Unreliable);
	}

	public void SendWithDeliveryEvent(byte[] data, byte channelNumber, DeliveryMethod deliveryMethod, object userData)
	{
		if (deliveryMethod != DeliveryMethod.ReliableOrdered && deliveryMethod != DeliveryMethod.ReliableUnordered)
		{
			throw new ArgumentException("Delivery event will work only for ReliableOrdered/Unordered packets");
		}
		method_5(data, 0, data.Length, channelNumber, deliveryMethod, userData);
	}

	public void SendWithDeliveryEvent(byte[] data, int start, int length, byte channelNumber, DeliveryMethod deliveryMethod, object userData)
	{
		if (deliveryMethod != DeliveryMethod.ReliableOrdered && deliveryMethod != DeliveryMethod.ReliableUnordered)
		{
			throw new ArgumentException("Delivery event will work only for ReliableOrdered/Unordered packets");
		}
		method_5(data, start, length, channelNumber, deliveryMethod, userData);
	}

	public void SendWithDeliveryEvent(GClass1484 dataWriter, byte channelNumber, DeliveryMethod deliveryMethod, object userData)
	{
		if (deliveryMethod != DeliveryMethod.ReliableOrdered && deliveryMethod != DeliveryMethod.ReliableUnordered)
		{
			throw new ArgumentException("Delivery event will work only for ReliableOrdered/Unordered packets");
		}
		method_5(dataWriter.Data, 0, dataWriter.Length, channelNumber, deliveryMethod, userData);
	}

	public void Send(byte[] data, DeliveryMethod deliveryMethod)
	{
		method_5(data, 0, data.Length, 0, deliveryMethod, null);
	}

	public void Send(GClass1484 dataWriter, DeliveryMethod deliveryMethod)
	{
		method_5(dataWriter.Data, 0, dataWriter.Length, 0, deliveryMethod, null);
	}

	public void Send(byte[] data, int start, int length, DeliveryMethod options)
	{
		method_5(data, start, length, 0, options, null);
	}

	public void Send(byte[] data, byte channelNumber, DeliveryMethod deliveryMethod)
	{
		method_5(data, 0, data.Length, channelNumber, deliveryMethod, null);
	}

	public void Send(GClass1484 dataWriter, byte channelNumber, DeliveryMethod deliveryMethod)
	{
		method_5(dataWriter.Data, 0, dataWriter.Length, channelNumber, deliveryMethod, null);
	}

	public void Send(byte[] data, int start, int length, byte channelNumber, DeliveryMethod deliveryMethod)
	{
		method_5(data, start, length, channelNumber, deliveryMethod, null);
	}

	public void method_5(byte[] data, int start, int length, byte channelNumber, DeliveryMethod deliveryMethod, object userData)
	{
		if (ConnectionState_0 != ConnectionState.Connected || channelNumber >= Class933_0.Length)
		{
			return;
		}
		Class933 @class = null;
		PacketProperty property;
		if (deliveryMethod == DeliveryMethod.Unreliable)
		{
			property = PacketProperty.Unreliable;
		}
		else
		{
			property = PacketProperty.Channeled;
			@class = method_2((byte)((uint)(channelNumber * 4) + (uint)deliveryMethod));
		}
		int headerSize = Class941.GetHeaderSize(property);
		int int_ = Int_6;
		if (length + headerSize > int_)
		{
			if (deliveryMethod != DeliveryMethod.ReliableOrdered && deliveryMethod != DeliveryMethod.ReliableUnordered)
			{
				throw new GException12("Unreliable or ReliableSequenced packet size exceeded maximum of " + (int_ - headerSize) + " bytes, Check allowed size by GetMaxSinglePacketSize()");
			}
			int num = int_ - headerSize - 6;
			int num2 = length / num + ((length % num != 0) ? 1 : 0);
			if (num2 > 65535)
			{
				throw new GException12("Data was split in " + num2 + " fragments, which exceeds " + ushort.MaxValue);
			}
			ushort fragmentId = (ushort)Interlocked.Increment(ref Int_12);
			for (ushort num3 = 0; num3 < num2; num3++)
			{
				int num4 = ((length > num) ? num : length);
				Class941 packet = Class944_0.GetPacket(headerSize + num4 + 6);
				packet.Property = property;
				packet.UserData = userData;
				packet.FragmentId = fragmentId;
				packet.FragmentPart = num3;
				packet.FragmentsTotal = (ushort)num2;
				packet.MarkFragmented();
				Buffer.BlockCopy(data, start + num3 * num, packet.RawData, 10, num4);
				@class.AddToQueue(packet);
				length -= num4;
			}
			return;
		}
		Class941 packet2 = Class944_0.GetPacket(headerSize + length);
		packet2.Property = property;
		Buffer.BlockCopy(data, start, packet2.RawData, headerSize, length);
		packet2.UserData = userData;
		if (@class == null)
		{
			lock (Queue_0)
			{
				Queue_0.Enqueue(packet2);
				return;
			}
		}
		@class.AddToQueue(packet2);
	}

	public void Disconnect(byte[] data)
	{
		NetManager.DisconnectPeer(this, data);
	}

	public void Disconnect(GClass1484 writer)
	{
		NetManager.DisconnectPeer(this, writer);
	}

	public void Disconnect(byte[] data, int start, int count)
	{
		NetManager.DisconnectPeer(this, data, start, count);
	}

	public void Disconnect()
	{
		NetManager.DisconnectPeer(this);
	}

	public DisconnectResult method_6(Class941 packet)
	{
		if ((ConnectionState_0 == ConnectionState.Connected || ConnectionState_0 == ConnectionState.Outgoing) && packet.Size >= 9 && BitConverter.ToInt64(packet.RawData, 1) == Long_1 && packet.ConnectionNumber == Byte_0_1)
		{
			if (ConnectionState_0 != ConnectionState.Connected)
			{
				return DisconnectResult.Reject;
			}
			return DisconnectResult.Disconnect;
		}
		return DisconnectResult.None;
	}

	public void method_7(Class933 channel)
	{
		lock (Queue_1)
		{
			Queue_1.Enqueue(channel);
		}
	}

	public ShutdownResult method_8(byte[] data, int start, int length, bool force)
	{
		lock (Object_0)
		{
			if (ConnectionState_0 != ConnectionState.Disconnected && ConnectionState_0 != ConnectionState.ShutdownRequested)
			{
				ShutdownResult result = ((ConnectionState_0 != ConnectionState.Connected) ? ShutdownResult.Success : ShutdownResult.WasConnected);
				if (force)
				{
					ConnectionState_0 = ConnectionState.Disconnected;
					return result;
				}
				Interlocked.Exchange(ref Int_5, 0);
				Class941_1 = new Class941(PacketProperty.Disconnect, length)
				{
					ConnectionNumber = Byte_0_1
				};
				GClass1481.GetBytes(Class941_1.RawData, 1, Long_1);
				if (Class941_1.Size >= Int_6)
				{
					GClass1475.smethod_5("[Peer] Disconnect additional data size more than MTU - 8!");
				}
				else if (data != null && length > 0)
				{
					Buffer.BlockCopy(data, start, Class941_1.RawData, 9, length);
				}
				ConnectionState_0 = ConnectionState.ShutdownRequested;
				NetManager.method_7(Class941_1, EndPoint);
				return result;
			}
			return ShutdownResult.None;
		}
	}

	public void method_9(int roundTripTime)
	{
		Int_0 += roundTripTime;
		Int_2++;
		Int_1 = Int_0 / Int_2;
		Double_0_1 = 25.0 + (double)Int_1 * 2.1;
	}

	public void method_10(DeliveryMethod method, Class941 p)
	{
		if (p.IsFragmented)
		{
			ushort fragmentId = p.FragmentId;
			if (!Dictionary_0.TryGetValue(fragmentId, out var value))
			{
				value = new Class945
				{
					Fragments = new Class941[p.FragmentsTotal],
					ChannelId = p.ChannelId
				};
				Dictionary_0.Add(fragmentId, value);
			}
			Class941[] fragments = value.Fragments;
			if (p.FragmentPart < fragments.Length && fragments[p.FragmentPart] == null && p.ChannelId == value.ChannelId)
			{
				fragments[p.FragmentPart] = p;
				value.ReceivedCount++;
				value.TotalSize += p.Size - 10;
				if (value.ReceivedCount != fragments.Length)
				{
					return;
				}
				Class941 packet = Class944_0.GetPacket(value.TotalSize);
				int num = 0;
				int num2 = 0;
				int num3;
				while (true)
				{
					if (num2 < value.ReceivedCount)
					{
						Class941 @class = fragments[num2];
						num3 = @class.Size - 10;
						if (num + num3 > packet.RawData.Length)
						{
							break;
						}
						Buffer.BlockCopy(@class.RawData, 10, packet.RawData, num, num3);
						num += num3;
						Class944_0.Recycle(@class);
						fragments[num2] = null;
						num2++;
						continue;
					}
					Dictionary_0.Remove(fragmentId);
					NetManager.method_22(packet, method, 0, this);
					return;
				}
				Dictionary_0.Remove(fragmentId);
				GClass1475.smethod_5("Fragment error pos: {0} >= resultPacketSize: {1} , totalSize: {2}", num + num3, packet.RawData.Length, value.TotalSize);
			}
			else
			{
				Class944_0.Recycle(p);
				GClass1475.smethod_5("Invalid fragment packet");
			}
		}
		else
		{
			NetManager.method_22(p, method, 4, this);
		}
	}

	public void method_11(Class941 packet)
	{
		if (packet.Size < GClass1474.Int_2[0])
		{
			return;
		}
		int num = BitConverter.ToInt32(packet.RawData, 1);
		int num2 = BitConverter.ToInt32(packet.RawData, packet.Size - 4);
		if (num == packet.Size && num == num2 && num <= GClass1474.Int_3)
		{
			if (packet.Property == PacketProperty.MtuCheck)
			{
				Int_9 = 0;
				packet.Property = PacketProperty.MtuOk;
				NetManager.method_6(packet, EndPoint);
			}
			else if (num > Int_6 && !Bool_0 && num == GClass1474.Int_2[Int_7 + 1])
			{
				lock (Object_1)
				{
					method_0(Int_7 + 1);
				}
				if (Int_7 == GClass1474.Int_2.Length - 1)
				{
					Bool_0 = true;
				}
			}
		}
		else
		{
			GClass1475.smethod_5("[MTU] Broken packet. RMTU {0}, EMTU {1}, PSIZE {2}", num, num2, packet.Size);
		}
	}

	public void method_12(int deltaTime)
	{
		if (Bool_0)
		{
			return;
		}
		Int_8 += deltaTime;
		if (Int_8 < 1000)
		{
			return;
		}
		Int_8 = 0;
		Int_9++;
		if (Int_9 >= 4)
		{
			Bool_0 = true;
			return;
		}
		lock (Object_1)
		{
			if (Int_7 < GClass1474.Int_2.Length - 1)
			{
				int num = GClass1474.Int_2[Int_7 + 1] - NetManager.ExtraPacketSizeForLayer;
				Class941 packet = Class944_0.GetPacket(num);
				packet.Property = PacketProperty.MtuCheck;
				GClass1481.GetBytes(packet.RawData, 1, num);
				GClass1481.GetBytes(packet.RawData, packet.Size - 4, num);
				if (NetManager.method_6(packet, EndPoint) <= 0)
				{
					Bool_0 = true;
				}
			}
		}
	}

	public ConnectRequestResult method_13(Class942 connRequest)
	{
		switch (ConnectionState_0)
		{
		case ConnectionState.Connected:
			if (connRequest.ConnectionTime == Long_1)
			{
				NetManager.method_7(Class941_5, EndPoint);
			}
			else if (connRequest.ConnectionTime > Long_1)
			{
				return ConnectRequestResult.Reconnection;
			}
			break;
		case ConnectionState.Outgoing:
		{
			if (connRequest.ConnectionTime < Long_1)
			{
				return ConnectRequestResult.P2PLose;
			}
			if (connRequest.ConnectionTime != Long_1)
			{
				break;
			}
			SocketAddress socketAddress = EndPoint.Serialize();
			byte[] targetAddress = connRequest.TargetAddress;
			int num = socketAddress.Size - 1;
			while (num >= 0)
			{
				byte b = socketAddress[num];
				if (b == targetAddress[num] || b >= targetAddress[num])
				{
					num--;
					continue;
				}
				return ConnectRequestResult.P2PLose;
			}
			break;
		}
		case ConnectionState.ShutdownRequested:
		case ConnectionState.Disconnected:
			if (connRequest.ConnectionTime >= Long_1)
			{
				return ConnectRequestResult.NewConnection;
			}
			break;
		}
		return ConnectRequestResult.None;
	}

	public void method_14(Class941 packet)
	{
		if (ConnectionState_0 != ConnectionState.Outgoing && ConnectionState_0 != ConnectionState.Disconnected)
		{
			if (packet.Property == PacketProperty.ShutdownOk)
			{
				if (ConnectionState_0 == ConnectionState.ShutdownRequested)
				{
					ConnectionState_0 = ConnectionState.Disconnected;
				}
				Class944_0.Recycle(packet);
				return;
			}
			if (packet.ConnectionNumber != Byte_0_1)
			{
				Class944_0.Recycle(packet);
				return;
			}
			Interlocked.Exchange(ref Int_5, 0);
			switch (packet.Property)
			{
			case PacketProperty.Unreliable:
				NetManager.method_22(packet, DeliveryMethod.Unreliable, 1, this);
				break;
			case PacketProperty.Channeled:
			case PacketProperty.Ack:
			{
				if (packet.ChannelId > Class933_0.Length)
				{
					Class944_0.Recycle(packet);
					break;
				}
				Class933 @class = Class933_0[packet.ChannelId] ?? ((packet.Property == PacketProperty.Ack) ? null : method_2(packet.ChannelId));
				if (@class != null && !@class.ProcessPacket(packet))
				{
					Class944_0.Recycle(packet);
				}
				break;
			}
			case PacketProperty.Ping:
				if (GClass1479.smethod_1(packet.Sequence, Class941_3.Sequence) > 0)
				{
					GClass1481.GetBytes(Class941_3.RawData, 3, DateTime.UtcNow.Ticks);
					Class941_3.Sequence = packet.Sequence;
					NetManager.method_7(Class941_3, EndPoint);
				}
				Class944_0.Recycle(packet);
				break;
			case PacketProperty.Pong:
				if (packet.Sequence == Class941_2.Sequence)
				{
					Stopwatch_0.Stop();
					int num3 = (int)Stopwatch_0.ElapsedMilliseconds;
					Long_0 = BitConverter.ToInt64(packet.RawData, 3) + num3 * 10000L / 2L - DateTime.UtcNow.Ticks;
					method_9(num3);
					NetManager.method_4(this, num3 / 2);
				}
				Class944_0.Recycle(packet);
				break;
			case PacketProperty.MtuCheck:
			case PacketProperty.MtuOk:
				method_11(packet);
				break;
			default:
				GClass1475.smethod_5("Error! Unexpected packet type: " + packet.Property);
				break;
			case PacketProperty.Merged:
			{
				int num = 1;
				while (num < packet.Size)
				{
					ushort num2 = BitConverter.ToUInt16(packet.RawData, num);
					num += 2;
					if (packet.RawData.Length - num < num2)
					{
						break;
					}
					Class941 packet2 = Class944_0.GetPacket(num2);
					Buffer.BlockCopy(packet.RawData, num, packet2.RawData, 0, num2);
					packet2.Size = num2;
					if (!packet2.Verify())
					{
						break;
					}
					num += num2;
					method_14(packet2);
				}
				Class944_0.Recycle(packet);
				break;
			}
			}
		}
		else
		{
			Class944_0.Recycle(packet);
		}
	}

	public void method_15()
	{
		if (Int_14 != 0)
		{
			int num = ((Int_14 <= 1) ? NetManager.method_8(Class941_0.RawData, 3, Int_13 - 2, EndPoint) : NetManager.method_8(Class941_0.RawData, 0, 1 + Int_13, EndPoint));
			if (NetManager.EnableStatistics)
			{
				Statistics.IncrementPacketsSent();
				Statistics.AddBytesSent(num);
			}
			Int_13 = 0;
			Int_14 = 0;
		}
	}

	public void method_16(Class941 packet)
	{
		packet.ConnectionNumber = Byte_0_1;
		int num = 1 + packet.Size + 2;
		if (num + 20 >= Int_6)
		{
			int num2 = NetManager.method_7(packet, EndPoint);
			if (NetManager.EnableStatistics)
			{
				Statistics.IncrementPacketsSent();
				Statistics.AddBytesSent(num2);
			}
			return;
		}
		if (Int_13 + num > Int_6)
		{
			method_15();
		}
		GClass1481.GetBytes(Class941_0.RawData, Int_13 + 1, (ushort)packet.Size);
		Buffer.BlockCopy(packet.RawData, 0, Class941_0.RawData, Int_13 + 1 + 2, packet.Size);
		Int_13 += packet.Size + 2;
		Int_14++;
	}

	public void Update(int deltaTime)
	{
		Interlocked.Add(ref Int_5, deltaTime);
		ConnectionState connectionState_ = ConnectionState_0;
		if (connectionState_ <= ConnectionState.Connected)
		{
			switch (connectionState_)
			{
			case ConnectionState.Connected:
				if (Int_5 > NetManager.DisconnectTimeout)
				{
					NetManager.method_9(this, DisconnectReason.Timeout, SocketError.Success, null);
					return;
				}
				break;
			case ConnectionState.Outgoing:
				Int_16 += deltaTime;
				if (Int_16 > NetManager.ReconnectDelay)
				{
					Int_16 = 0;
					Int_15++;
					if (Int_15 > NetManager.MaxConnectAttempts)
					{
						NetManager.method_9(this, DisconnectReason.ConnectionFailed, SocketError.Success, null);
					}
					else
					{
						NetManager.method_7(Class941_4, EndPoint);
					}
				}
				return;
			}
		}
		else
		{
			switch (connectionState_)
			{
			case ConnectionState.Disconnected:
				return;
			case ConnectionState.ShutdownRequested:
				if (Int_5 > NetManager.DisconnectTimeout)
				{
					ConnectionState_0 = ConnectionState.Disconnected;
					return;
				}
				Int_18 += deltaTime;
				if (Int_18 >= 300)
				{
					Int_18 = 0;
					NetManager.method_7(Class941_1, EndPoint);
				}
				return;
			}
		}
		Int_3 += deltaTime;
		if (Int_3 >= NetManager.PingInterval)
		{
			Int_3 = 0;
			Class941_2.Sequence++;
			if (Stopwatch_0.IsRunning)
			{
				method_9((int)Stopwatch_0.ElapsedMilliseconds);
			}
			Stopwatch_0.Reset();
			Stopwatch_0.Start();
			NetManager.method_7(Class941_2, EndPoint);
		}
		Int_4 += deltaTime;
		if (Int_4 >= NetManager.PingInterval * 3)
		{
			Int_4 = 0;
			Int_0 = Int_1;
			Int_2 = 1;
		}
		method_12(deltaTime);
		if (Queue_1.Count > 0)
		{
			lock (Queue_1)
			{
				int count = Queue_1.Count;
				while (count-- > 0)
				{
					Class933 @class = Queue_1.Dequeue();
					if (@class.SendAndCheckQueue())
					{
						Queue_1.Enqueue(@class);
					}
				}
			}
		}
		if (Queue_0.Count > 0)
		{
			lock (Queue_0)
			{
				while (Queue_0.Count > 0)
				{
					Class941 packet = Queue_0.Dequeue();
					method_16(packet);
					NetManager.Class944_0.Recycle(packet);
				}
			}
		}
		method_15();
	}

	public void method_17(Class941 packet)
	{
		if (packet.UserData != null)
		{
			if (packet.IsFragmented)
			{
				Dictionary_1.TryGetValue(packet.FragmentId, out var value);
				value++;
				if (value == packet.FragmentsTotal)
				{
					NetManager.method_5(this, packet.UserData);
					Dictionary_1.Remove(packet.FragmentId);
				}
				else
				{
					Dictionary_1[packet.FragmentId] = value;
				}
			}
			else
			{
				NetManager.method_5(this, packet.UserData);
			}
			packet.UserData = null;
		}
		Class944_0.Recycle(packet);
	}
}
