using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FlyingWormConsole3.LiteNetLib;

public class GClass1476 : IEnumerable<GClass1477>, IEnumerable
{
	public class Class940 : IEqualityComparer<IPEndPoint>
	{
		public bool Equals(IPEndPoint x, IPEndPoint y)
		{
			if (x.Address.Equals(y.Address))
			{
				return x.Port == y.Port;
			}
			return false;
		}

		public int GetHashCode(IPEndPoint obj)
		{
			return obj.GetHashCode();
		}
	}

	public struct GStruct149(GClass1477 p) : IEnumerator<GClass1477>, IEnumerator, IDisposable
	{
		[NonSerialized]
		public GClass1477 Gclass1477_0 = p;

		[NonSerialized]
		public GClass1477 Gclass1477_1 = null;

		public GClass1477 Current => Gclass1477_1;

		object IEnumerator.Current => Gclass1477_1;

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			Gclass1477_1 = ((Gclass1477_1 == null) ? Gclass1477_0 : Gclass1477_1.Gclass1477_0);
			return Gclass1477_1 != null;
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}
	}

	[NonSerialized]
	public Class946 Class946_0;

	[NonSerialized]
	public Thread Thread_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public AutoResetEvent AutoResetEvent_0 = new AutoResetEvent(initialState: true);

	[NonSerialized]
	public Queue<Class939> Queue_0;

	[NonSerialized]
	public Class939 Class939_0;

	[NonSerialized]
	public IOnNetworkEvent IOnNetworkEvent;

	[NonSerialized]
	public GInterface139 Ginterface139_0;

	[NonSerialized]
	public GInterface140 Ginterface140_0;

	[NonSerialized]
	public Dictionary<IPEndPoint, GClass1477> Dictionary_0;

	[NonSerialized]
	public Dictionary<IPEndPoint, GClass1470> Dictionary_1;

	[NonSerialized]
	public Dictionary<IPEndPoint, Class983> Dictionary_2;

	[NonSerialized]
	public ReaderWriterLockSlim ReaderWriterLockSlim_0;

	[NonSerialized]
	public volatile GClass1477 Gclass1477_0;

	[NonSerialized]
	public volatile int Int_0;

	[NonSerialized]
	public List<GClass1477> List_0;

	[NonSerialized]
	public GClass1477[] Gclass1477_1;

	[NonSerialized]
	public GClass1488 Gclass1488_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public Queue<int> Queue_1;

	[NonSerialized]
	public byte Byte_0 = 1;

	[NonSerialized]
	public object Object_0 = new object();

	[NonSerialized]
	public Class944 Class944_0;

	public bool UnconnectedMessagesEnabled;

	public bool NatPunchEnabled;

	public int UpdateTime = 15;

	public int PingInterval = 1000;

	public int DisconnectTimeout = 5000;

	public bool SimulatePacketLoss;

	public bool SimulateLatency;

	public int SimulationPacketLossChance = 10;

	public int SimulationMinLatency = 30;

	public int SimulationMaxLatency = 100;

	public bool UnsyncedEvents;

	public bool UnsyncedReceiveEvent;

	public bool UnsyncedDeliveryEvent;

	public bool BroadcastReceiveEnabled;

	public int ReconnectDelay = 500;

	public int MaxConnectAttempts = 10;

	public bool ReuseAddress;

	public readonly GClass1478 Statistics;

	public bool EnableStatistics;

	public readonly GClass1473 NatPunchModule;

	public bool AutoRecycle;

	public IPv6Mode IPv6Enabled = IPv6Mode.SeparateSocket;

	public int MtuOverride;

	public bool UseSafeMtu;

	public bool DisconnectOnUnreachable;

	public bool IsRunning => Class946_0.IsRunning;

	public int LocalPort => Class946_0.LocalPort;

	public GClass1477 FirstPeer => Gclass1477_0;

	public byte ChannelsCount
	{
		get
		{
			return Byte_0;
		}
		set
		{
			if (value < 1 || value > 64)
			{
				throw new ArgumentException("Channels count must be between 1 and 64");
			}
			Byte_0 = value;
		}
	}

	public List<GClass1477> ConnectedPeerList
	{
		get
		{
			GetPeersNonAlloc(List_0, ConnectionState.Connected);
			return List_0;
		}
	}

	public int ConnectedPeersCount => Int_0;

	public int ExtraPacketSizeForLayer
	{
		get
		{
			if (Gclass1488_0 == null)
			{
				return 0;
			}
			return Gclass1488_0.ExtraPacketSizeForLayer;
		}
	}

	public GClass1477 GetPeerById(int id)
	{
		return Gclass1477_1[id];
	}

	public bool method_0(IPEndPoint endPoint, out GClass1477 peer)
	{
		ReaderWriterLockSlim_0.EnterReadLock();
		bool result = Dictionary_0.TryGetValue(endPoint, out peer);
		ReaderWriterLockSlim_0.ExitReadLock();
		return result;
	}

	public void method_1(GClass1477 peer)
	{
		ReaderWriterLockSlim_0.EnterWriteLock();
		if (Gclass1477_0 != null)
		{
			peer.Gclass1477_0 = Gclass1477_0;
			Gclass1477_0.Gclass1477_1 = peer;
		}
		Gclass1477_0 = peer;
		Dictionary_0.Add(peer.EndPoint, peer);
		if (peer.Id >= Gclass1477_1.Length)
		{
			int num = Gclass1477_1.Length * 2;
			while (peer.Id >= num)
			{
				num *= 2;
			}
			Array.Resize(ref Gclass1477_1, num);
		}
		Gclass1477_1[peer.Id] = peer;
		ReaderWriterLockSlim_0.ExitWriteLock();
	}

	public void method_2(GClass1477 peer)
	{
		ReaderWriterLockSlim_0.EnterWriteLock();
		method_3(peer);
		ReaderWriterLockSlim_0.ExitWriteLock();
	}

	public void method_3(GClass1477 peer)
	{
		if (!Dictionary_0.Remove(peer.EndPoint))
		{
			return;
		}
		if (peer == Gclass1477_0)
		{
			Gclass1477_0 = peer.Gclass1477_0;
		}
		if (peer.Gclass1477_1 != null)
		{
			peer.Gclass1477_1.Gclass1477_0 = peer.Gclass1477_0;
		}
		if (peer.Gclass1477_0 != null)
		{
			peer.Gclass1477_0.Gclass1477_1 = peer.Gclass1477_1;
		}
		peer.Gclass1477_1 = null;
		Gclass1477_1[peer.Id] = null;
		lock (Queue_1)
		{
			Queue_1.Enqueue(peer.Id);
		}
	}

	public GClass1476(IOnNetworkEvent listener, GClass1488 extraPacketLayer = null)
	{
		Class946_0 = new Class946(this);
		IOnNetworkEvent = listener;
		Ginterface139_0 = listener as GInterface139;
		Ginterface140_0 = listener as GInterface140;
		Queue_0 = new Queue<Class939>();
		Class944_0 = new Class944();
		NatPunchModule = new GClass1473(Class946_0);
		Statistics = new GClass1478();
		List_0 = new List<GClass1477>();
		Dictionary_0 = new Dictionary<IPEndPoint, GClass1477>(new Class940());
		Dictionary_1 = new Dictionary<IPEndPoint, GClass1470>(new Class940());
		Dictionary_2 = new Dictionary<IPEndPoint, Class983>(new Class940());
		ReaderWriterLockSlim_0 = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
		Queue_1 = new Queue<int>();
		Gclass1477_1 = new GClass1477[32];
		Gclass1488_0 = extraPacketLayer;
	}

	public void method_4(GClass1477 fromPeer, int latency)
	{
		method_11(Class939.EType.ConnectionLatencyUpdated, fromPeer, null, SocketError.Success, latency);
	}

	public void method_5(GClass1477 fromPeer, object userData)
	{
		if (Ginterface139_0 != null)
		{
			method_11(Class939.EType.MessageDelivered, fromPeer, null, SocketError.Success, 0, DisconnectReason.ConnectionFailed, null, DeliveryMethod.Unreliable, null, userData);
		}
	}

	public int method_6(Class941 packet, IPEndPoint remoteEndPoint)
	{
		int result = method_8(packet.RawData, 0, packet.Size, remoteEndPoint);
		Class944_0.Recycle(packet);
		return result;
	}

	public int method_7(Class941 packet, IPEndPoint remoteEndPoint)
	{
		return method_8(packet.RawData, 0, packet.Size, remoteEndPoint);
	}

	public int method_8(byte[] message, int start, int length, IPEndPoint remoteEndPoint)
	{
		if (!Class946_0.IsRunning)
		{
			return 0;
		}
		SocketError errorCode = SocketError.Success;
		int num;
		if (Gclass1488_0 != null)
		{
			Class941 packet = Class944_0.GetPacket(length + Gclass1488_0.ExtraPacketSizeForLayer);
			Buffer.BlockCopy(message, start, packet.RawData, 0, length);
			int offset = 0;
			Gclass1488_0.ProcessOutBoundPacket(remoteEndPoint, ref packet.RawData, ref offset, ref length);
			num = Class946_0.SendTo(packet.RawData, offset, length, remoteEndPoint, ref errorCode);
			Class944_0.Recycle(packet);
		}
		else
		{
			num = Class946_0.SendTo(message, start, length, remoteEndPoint, ref errorCode);
		}
		switch (errorCode)
		{
		default:
			if (num <= 0)
			{
				return 0;
			}
			if (EnableStatistics)
			{
				Statistics.IncrementPacketsSent();
				Statistics.AddBytesSent(length);
			}
			return num;
		case SocketError.NetworkUnreachable:
		case SocketError.HostUnreachable:
		{
			if (DisconnectOnUnreachable && method_0(remoteEndPoint, out var peer))
			{
				method_9(peer, (errorCode == SocketError.HostUnreachable) ? DisconnectReason.HostUnreachable : DisconnectReason.NetworkUnreachable, errorCode, null);
			}
			method_11(Class939.EType.Error, null, remoteEndPoint, errorCode);
			return -1;
		}
		case SocketError.MessageSize:
			return -1;
		}
	}

	public void method_9(GClass1477 peer, DisconnectReason reason, SocketError socketErrorCode, Class941 eventData)
	{
		method_10(peer, reason, socketErrorCode, force: true, null, 0, 0, eventData);
	}

	public void method_10(GClass1477 peer, DisconnectReason reason, SocketError socketErrorCode, bool force, byte[] data, int start, int count, Class941 eventData)
	{
		switch (peer.method_8(data, start, count, force))
		{
		case ShutdownResult.None:
			return;
		case ShutdownResult.WasConnected:
			Interlocked.Decrement(ref Int_0);
			break;
		}
		Thread.MemoryBarrier();
		method_11(Class939.EType.Disconnect, peer, null, socketErrorCode, 0, reason, null, DeliveryMethod.Unreliable, eventData);
	}

	public void method_11(Class939.EType type, GClass1477 peer = null, IPEndPoint remoteEndPoint = null, SocketError errorCode = SocketError.Success, int latency = 0, DisconnectReason disconnectReason = DisconnectReason.ConnectionFailed, GClass1470 connectionRequest = null, DeliveryMethod deliveryMethod = DeliveryMethod.Unreliable, Class941 readerSource = null, object userData = null)
	{
		bool flag = UnsyncedEvents;
		switch (type)
		{
		case Class939.EType.Connect:
			Interlocked.Increment(ref Int_0);
			break;
		case Class939.EType.MessageDelivered:
			flag = UnsyncedDeliveryEvent;
			break;
		}
		Class939 @class;
		lock (Object_0)
		{
			@class = Class939_0;
			if (@class == null)
			{
				@class = new Class939(this);
			}
			else
			{
				Class939_0 = @class.Next;
			}
		}
		@class.Type = type;
		@class.DataReader.method_0(readerSource, readerSource?.GetHeaderSize() ?? 0);
		@class.Peer = peer;
		@class.RemoteEndPoint = remoteEndPoint;
		@class.Latency = latency;
		@class.ErrorCode = errorCode;
		@class.DisconnectReason = disconnectReason;
		@class.ConnectionRequest = connectionRequest;
		@class.DeliveryMethod = deliveryMethod;
		@class.UserData = userData;
		if (!flag && !Bool_0)
		{
			lock (Queue_0)
			{
				Queue_0.Enqueue(@class);
				return;
			}
		}
		method_12(@class);
	}

	public void method_12(Class939 evt)
	{
		bool isNull = evt.DataReader.IsNull;
		switch (evt.Type)
		{
		case Class939.EType.Connect:
			IOnNetworkEvent.OnPeerConnected(evt.Peer);
			break;
		case Class939.EType.Disconnect:
		{
			DisconnectionInfoStruct disconnectInfo = new DisconnectionInfoStruct
			{
				Reason = evt.DisconnectReason,
				AdditionalData = evt.DataReader,
				SocketErrorCode = evt.ErrorCode
			};
			IOnNetworkEvent.OnPeerDisconnected(evt.Peer, disconnectInfo);
			break;
		}
		case Class939.EType.Receive:
			IOnNetworkEvent.OnNetworkReceive(evt.Peer, evt.DataReader, evt.DeliveryMethod);
			break;
		case Class939.EType.ReceiveUnconnected:
			IOnNetworkEvent.OnNetworkReceiveUnconnected(evt.RemoteEndPoint, evt.DataReader, UnconnectedMessageType.BasicMessage);
			break;
		case Class939.EType.Error:
			IOnNetworkEvent.OnNetworkError(evt.RemoteEndPoint, evt.ErrorCode);
			break;
		case Class939.EType.ConnectionLatencyUpdated:
			IOnNetworkEvent.OnNetworkLatencyUpdate(evt.Peer, evt.Latency);
			break;
		case Class939.EType.Broadcast:
			IOnNetworkEvent.OnNetworkReceiveUnconnected(evt.RemoteEndPoint, evt.DataReader, UnconnectedMessageType.Broadcast);
			break;
		case Class939.EType.ConnectionRequest:
			IOnNetworkEvent.OnConnectionRequest(evt.ConnectionRequest);
			break;
		case Class939.EType.MessageDelivered:
			Ginterface139_0.OnMessageDelivered(evt.Peer, evt.UserData);
			break;
		}
		if (isNull)
		{
			method_13(evt);
		}
		else if (AutoRecycle)
		{
			evt.DataReader.method_1();
		}
	}

	public void method_13(Class939 evt)
	{
		evt.Peer = null;
		evt.ErrorCode = SocketError.Success;
		evt.RemoteEndPoint = null;
		evt.ConnectionRequest = null;
		lock (Object_0)
		{
			evt.Next = Class939_0;
			Class939_0 = evt;
		}
	}

	public void method_14()
	{
		List<GClass1477> list = new List<GClass1477>();
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		while (Class946_0.IsRunning)
		{
			int num = (int)stopwatch.ElapsedMilliseconds;
			num = ((num <= 0) ? 1 : num);
			stopwatch.Reset();
			stopwatch.Start();
			for (GClass1477 gclass1477_ = Gclass1477_0; gclass1477_ != null; gclass1477_ = gclass1477_.Gclass1477_0)
			{
				if (gclass1477_.ConnectionState == ConnectionState.Disconnected && gclass1477_.TimeSinceLastPacket > DisconnectTimeout)
				{
					list.Add(gclass1477_);
				}
				else
				{
					gclass1477_.Update(num);
				}
			}
			if (list.Count > 0)
			{
				ReaderWriterLockSlim_0.EnterWriteLock();
				for (int i = 0; i < list.Count; i++)
				{
					method_3(list[i]);
				}
				ReaderWriterLockSlim_0.ExitWriteLock();
				list.Clear();
			}
			method_16(num);
			int num2 = UpdateTime - (int)stopwatch.ElapsedMilliseconds;
			if (num2 > 0)
			{
				AutoResetEvent_0.WaitOne(num2);
			}
		}
		stopwatch.Stop();
	}

	[Conditional("DEBUG")]
	public void method_15()
	{
	}

	public void method_16(int elapsedMilliseconds)
	{
		List<IPEndPoint> list = null;
		foreach (KeyValuePair<IPEndPoint, Class983> item in Dictionary_2)
		{
			item.Value.Send(Class946_0, elapsedMilliseconds);
			if (item.Value.NeedToKill)
			{
				if (list == null)
				{
					list = new List<IPEndPoint>();
				}
				list.Add(item.Key);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (IPEndPoint item2 in list)
		{
			Dictionary_2.Remove(item2);
		}
	}

	public void ManualUpdate(int elapsedMilliseconds)
	{
		if (!Bool_0)
		{
			return;
		}
		for (GClass1477 gclass1477_ = Gclass1477_0; gclass1477_ != null; gclass1477_ = gclass1477_.Gclass1477_0)
		{
			if (gclass1477_.ConnectionState == ConnectionState.Disconnected && gclass1477_.TimeSinceLastPacket > DisconnectTimeout)
			{
				method_3(gclass1477_);
			}
			else
			{
				gclass1477_.Update(elapsedMilliseconds);
			}
		}
		method_16(elapsedMilliseconds);
	}

	public void ManualReceive()
	{
		if (Bool_0)
		{
			Class946_0.ManualReceive();
		}
	}

	public void method_17(Class941 packet, SocketError errorCode, IPEndPoint remoteEndPoint)
	{
		if (errorCode != SocketError.Success)
		{
			method_11(Class939.EType.Error, null, null, errorCode);
			GClass1475.smethod_5("[NM] Receive error: {0}", errorCode);
			return;
		}
		try
		{
			method_21(packet, remoteEndPoint);
		}
		catch (Exception ex)
		{
			GClass1475.smethod_5("[NM] SocketReceiveThread error: " + ex);
		}
	}

	public GClass1477 method_18(GClass1470 request, byte[] rejectData, int start, int length)
	{
		GClass1477 value = null;
		if (request.ConnectionRequestResult_0 == ConnectionRequestResult.RejectForce)
		{
			if (rejectData != null && length > 0)
			{
				Class941 withProperty = Class944_0.GetWithProperty(PacketProperty.Disconnect, length);
				withProperty.ConnectionNumber = request.Byte_0;
				GClass1481.GetBytes(withProperty.RawData, 1, request.Long_0);
				if (withProperty.Size >= GClass1474.Int_2[0])
				{
					GClass1475.smethod_5("[Peer] Disconnect additional data size more than MTU!");
				}
				else
				{
					Buffer.BlockCopy(rejectData, start, withProperty.RawData, 9, length);
				}
				method_6(withProperty, request.RemoteEndPoint);
			}
		}
		else
		{
			ReaderWriterLockSlim_0.EnterUpgradeableReadLock();
			if (Dictionary_0.TryGetValue(request.RemoteEndPoint, out value))
			{
				ReaderWriterLockSlim_0.ExitUpgradeableReadLock();
			}
			else if (request.ConnectionRequestResult_0 == ConnectionRequestResult.Reject)
			{
				value = new GClass1477(this, request.RemoteEndPoint, method_19());
				value.method_3(request.Long_0, request.Byte_0, rejectData, start, length);
				method_1(value);
				ReaderWriterLockSlim_0.ExitUpgradeableReadLock();
			}
			else
			{
				value = new GClass1477(this, request.RemoteEndPoint, method_19(), request.Long_0, request.Byte_0);
				method_1(value);
				ReaderWriterLockSlim_0.ExitUpgradeableReadLock();
				method_11(Class939.EType.Connect, value);
			}
		}
		lock (Dictionary_1)
		{
			Dictionary_1.Remove(request.RemoteEndPoint);
			return value;
		}
	}

	public int method_19()
	{
		lock (Queue_1)
		{
			return (Queue_1.Count == 0) ? Int_1++ : Queue_1.Dequeue();
		}
	}

	public void method_20(IPEndPoint remoteEndPoint, GClass1477 netPeer, Class942 connRequest)
	{
		byte connectionNumber = connRequest.ConnectionNumber;
		if (netPeer != null)
		{
			ConnectRequestResult connectRequestResult = netPeer.method_13(connRequest);
			switch (connectRequestResult)
			{
			default:
				return;
			case ConnectRequestResult.P2PLose:
				method_9(netPeer, DisconnectReason.PeerToPeerConnection, SocketError.Success, null);
				method_2(netPeer);
				break;
			case ConnectRequestResult.Reconnection:
				method_9(netPeer, DisconnectReason.Reconnect, SocketError.Success, null);
				method_2(netPeer);
				break;
			case ConnectRequestResult.NewConnection:
				method_2(netPeer);
				break;
			}
			if (connectRequestResult != ConnectRequestResult.P2PLose)
			{
				connectionNumber = (byte)((netPeer.Byte_0 + 1) % 4);
			}
		}
		GClass1470 value;
		lock (Dictionary_1)
		{
			if (Dictionary_1.TryGetValue(remoteEndPoint, out value))
			{
				value.method_1(connRequest);
				return;
			}
			value = new GClass1470(connRequest.ConnectionTime, connectionNumber, connRequest.Data, remoteEndPoint, this);
			Dictionary_1.Add(remoteEndPoint, value);
		}
		method_11(Class939.EType.ConnectionRequest, null, null, SocketError.Success, 0, DisconnectReason.ConnectionFailed, value);
	}

	public void method_21(Class941 packet, IPEndPoint remoteEndPoint)
	{
		if (EnableStatistics)
		{
			Statistics.IncrementPacketsReceived();
			Statistics.AddBytesReceived(packet.Size);
		}
		if (Dictionary_2.Count > 0 && Dictionary_2.TryGetValue(remoteEndPoint, out var _))
		{
			if (packet.Size < 48)
			{
				return;
			}
			byte[] array = new byte[packet.Size];
			Buffer.BlockCopy(packet.RawData, 0, array, 0, packet.Size);
			GClass1487 gClass = GClass1487.FromServerResponse(array, DateTime.UtcNow);
			try
			{
				gClass.method_1();
			}
			catch (InvalidOperationException)
			{
				gClass = null;
			}
			if (gClass != null)
			{
				Dictionary_2.Remove(remoteEndPoint);
				if (Ginterface140_0 != null)
				{
					Ginterface140_0.OnNtpResponse(gClass);
				}
			}
			return;
		}
		if (Gclass1488_0 != null)
		{
			int offset = 0;
			Gclass1488_0.ProcessInboundPacket(remoteEndPoint, ref packet.RawData, ref offset, ref packet.Size);
			if (packet.Size == 0)
			{
				return;
			}
		}
		if (!packet.Verify())
		{
			GClass1475.smethod_5("[NM] DataReceived: bad!");
			Class944_0.Recycle(packet);
			return;
		}
		PacketProperty property = packet.Property;
		if (property <= PacketProperty.UnconnectedMessage)
		{
			switch (property)
			{
			case PacketProperty.UnconnectedMessage:
				if (UnconnectedMessagesEnabled)
				{
					method_11(Class939.EType.ReceiveUnconnected, null, remoteEndPoint, SocketError.Success, 0, DisconnectReason.ConnectionFailed, null, DeliveryMethod.Unreliable, packet);
				}
				return;
			case PacketProperty.ConnectRequest:
				if (Class942.GetProtocolId(packet) != 11)
				{
					method_6(Class944_0.GetWithProperty(PacketProperty.InvalidProtocol), remoteEndPoint);
					return;
				}
				break;
			}
		}
		else
		{
			switch (property)
			{
			case PacketProperty.NatMessage:
				if (NatPunchEnabled)
				{
					NatPunchModule.method_0(remoteEndPoint, packet);
				}
				return;
			case PacketProperty.Broadcast:
				if (BroadcastReceiveEnabled)
				{
					method_11(Class939.EType.Broadcast, null, remoteEndPoint, SocketError.Success, 0, DisconnectReason.ConnectionFailed, null, DeliveryMethod.Unreliable, packet);
				}
				return;
			}
		}
		ReaderWriterLockSlim_0.EnterReadLock();
		GClass1477 value2;
		bool flag = Dictionary_0.TryGetValue(remoteEndPoint, out value2);
		ReaderWriterLockSlim_0.ExitReadLock();
		switch (packet.Property)
		{
		default:
			if (flag)
			{
				value2.method_14(packet);
			}
			else
			{
				method_6(Class944_0.GetWithProperty(PacketProperty.PeerNotFound), remoteEndPoint);
			}
			break;
		case PacketProperty.InvalidProtocol:
			if (flag && value2.ConnectionState == ConnectionState.Outgoing)
			{
				method_9(value2, DisconnectReason.InvalidProtocol, SocketError.Success, null);
			}
			break;
		case PacketProperty.PeerNotFound:
			if (flag)
			{
				if (value2.ConnectionState == ConnectionState.Connected)
				{
					if (packet.Size == 1)
					{
						Class941 withProperty = Class944_0.GetWithProperty(PacketProperty.PeerNotFound, 9);
						withProperty.RawData[1] = 0;
						GClass1481.GetBytes(withProperty.RawData, 2, value2.Int64_0);
						method_6(withProperty, remoteEndPoint);
					}
					else if (packet.Size == 10 && packet.RawData[1] == 1 && BitConverter.ToInt64(packet.RawData, 2) == value2.Int64_0)
					{
						method_9(value2, DisconnectReason.RemoteConnectionClose, SocketError.Success, null);
					}
				}
			}
			else if (packet.Size == 10 && packet.RawData[1] == 0)
			{
				packet.RawData[1] = 1;
				method_6(packet, remoteEndPoint);
			}
			break;
		case PacketProperty.ConnectRequest:
		{
			Class942 class2 = Class942.FromData(packet);
			if (class2 != null)
			{
				method_20(remoteEndPoint, value2, class2);
			}
			break;
		}
		case PacketProperty.ConnectAccept:
			if (flag)
			{
				Class943 @class = Class943.FromData(packet);
				if (@class != null && value2.method_4(@class))
				{
					method_11(Class939.EType.Connect, value2);
				}
			}
			break;
		case PacketProperty.Disconnect:
			if (flag)
			{
				DisconnectResult disconnectResult = value2.method_6(packet);
				if (disconnectResult == DisconnectResult.None)
				{
					Class944_0.Recycle(packet);
					break;
				}
				method_9(value2, (disconnectResult == DisconnectResult.Disconnect) ? DisconnectReason.RemoteConnectionClose : DisconnectReason.ConnectionRejected, SocketError.Success, packet);
			}
			else
			{
				Class944_0.Recycle(packet);
			}
			method_6(Class944_0.GetWithProperty(PacketProperty.ShutdownOk), remoteEndPoint);
			break;
		}
	}

	public void method_22(Class941 packet, DeliveryMethod method, int headerSize, GClass1477 fromPeer)
	{
		Class939 @class;
		lock (Object_0)
		{
			@class = Class939_0;
			if (@class == null)
			{
				@class = new Class939(this);
			}
			else
			{
				Class939_0 = @class.Next;
			}
		}
		@class.Type = Class939.EType.Receive;
		@class.DataReader.method_0(packet, headerSize);
		@class.Peer = fromPeer;
		@class.DeliveryMethod = method;
		if (!UnsyncedEvents && !UnsyncedReceiveEvent && !Bool_0)
		{
			lock (Queue_0)
			{
				Queue_0.Enqueue(@class);
				return;
			}
		}
		method_12(@class);
	}

	public void SendToAll(GClass1484 writer, DeliveryMethod options)
	{
		SendToAll(writer.Data, 0, writer.Length, options);
	}

	public void SendToAll(byte[] data, DeliveryMethod options)
	{
		SendToAll(data, 0, data.Length, options);
	}

	public void SendToAll(byte[] data, int start, int length, DeliveryMethod options)
	{
		SendToAll(data, start, length, 0, options);
	}

	public void SendToAll(GClass1484 writer, byte channelNumber, DeliveryMethod options)
	{
		SendToAll(writer.Data, 0, writer.Length, channelNumber, options);
	}

	public void SendToAll(byte[] data, byte channelNumber, DeliveryMethod options)
	{
		SendToAll(data, 0, data.Length, channelNumber, options);
	}

	public void SendToAll(byte[] data, int start, int length, byte channelNumber, DeliveryMethod options)
	{
		try
		{
			ReaderWriterLockSlim_0.EnterReadLock();
			for (GClass1477 gclass1477_ = Gclass1477_0; gclass1477_ != null; gclass1477_ = gclass1477_.Gclass1477_0)
			{
				gclass1477_.Send(data, start, length, channelNumber, options);
			}
		}
		finally
		{
			ReaderWriterLockSlim_0.ExitReadLock();
		}
	}

	public void SendToAll(GClass1484 writer, DeliveryMethod options, GClass1477 excludePeer)
	{
		SendToAll(writer.Data, 0, writer.Length, 0, options, excludePeer);
	}

	public void SendToAll(byte[] data, DeliveryMethod options, GClass1477 excludePeer)
	{
		SendToAll(data, 0, data.Length, 0, options, excludePeer);
	}

	public void SendToAll(byte[] data, int start, int length, DeliveryMethod options, GClass1477 excludePeer)
	{
		SendToAll(data, start, length, 0, options, excludePeer);
	}

	public void SendToAll(GClass1484 writer, byte channelNumber, DeliveryMethod options, GClass1477 excludePeer)
	{
		SendToAll(writer.Data, 0, writer.Length, channelNumber, options, excludePeer);
	}

	public void SendToAll(byte[] data, byte channelNumber, DeliveryMethod options, GClass1477 excludePeer)
	{
		SendToAll(data, 0, data.Length, channelNumber, options, excludePeer);
	}

	public void SendToAll(byte[] data, int start, int length, byte channelNumber, DeliveryMethod options, GClass1477 excludePeer)
	{
		try
		{
			ReaderWriterLockSlim_0.EnterReadLock();
			for (GClass1477 gclass1477_ = Gclass1477_0; gclass1477_ != null; gclass1477_ = gclass1477_.Gclass1477_0)
			{
				if (gclass1477_ != excludePeer)
				{
					gclass1477_.Send(data, start, length, channelNumber, options);
				}
			}
		}
		finally
		{
			ReaderWriterLockSlim_0.ExitReadLock();
		}
	}

	public bool Start()
	{
		return Start(0);
	}

	public bool Start(IPAddress addressIPv4, IPAddress addressIPv6, int port)
	{
		Bool_0 = false;
		if (!Class946_0.Bind(addressIPv4, addressIPv6, port, ReuseAddress, IPv6Enabled, manualMode: false))
		{
			return false;
		}
		Thread_0 = new Thread(method_14)
		{
			Name = "LogicThread",
			IsBackground = true
		};
		Thread_0.Start();
		return true;
	}

	public bool Start(string addressIPv4, string addressIPv6, int port)
	{
		IPAddress addressIPv7 = GClass1479.ResolveAddress(addressIPv4);
		IPAddress addressIPv8 = GClass1479.ResolveAddress(addressIPv6);
		return Start(addressIPv7, addressIPv8, port);
	}

	public bool Start(int port)
	{
		return Start(IPAddress.Any, IPAddress.IPv6Any, port);
	}

	public bool StartInManualMode(IPAddress addressIPv4, IPAddress addressIPv6, int port)
	{
		Bool_0 = true;
		if (!Class946_0.Bind(addressIPv4, addressIPv6, port, ReuseAddress, IPv6Enabled, manualMode: true))
		{
			return false;
		}
		return true;
	}

	public bool StartInManualMode(string addressIPv4, string addressIPv6, int port)
	{
		IPAddress addressIPv7 = GClass1479.ResolveAddress(addressIPv4);
		IPAddress addressIPv8 = GClass1479.ResolveAddress(addressIPv6);
		return StartInManualMode(addressIPv7, addressIPv8, port);
	}

	public bool StartInManualMode(int port)
	{
		return StartInManualMode(IPAddress.Any, IPAddress.IPv6Any, port);
	}

	public bool SendUnconnectedMessage(byte[] message, IPEndPoint remoteEndPoint)
	{
		return SendUnconnectedMessage(message, 0, message.Length, remoteEndPoint);
	}

	public bool SendUnconnectedMessage(GClass1484 writer, IPEndPoint remoteEndPoint)
	{
		return SendUnconnectedMessage(writer.Data, 0, writer.Length, remoteEndPoint);
	}

	public bool SendUnconnectedMessage(byte[] message, int start, int length, IPEndPoint remoteEndPoint)
	{
		Class941 withData = Class944_0.GetWithData(PacketProperty.UnconnectedMessage, message, start, length);
		return method_6(withData, remoteEndPoint) > 0;
	}

	public bool SendBroadcast(GClass1484 writer, int port)
	{
		return SendBroadcast(writer.Data, 0, writer.Length, port);
	}

	public bool SendBroadcast(byte[] data, int port)
	{
		return SendBroadcast(data, 0, data.Length, port);
	}

	public bool SendBroadcast(byte[] data, int start, int length, int port)
	{
		Class941 @class;
		if (Gclass1488_0 != null)
		{
			int headerSize = Class941.GetHeaderSize(PacketProperty.Broadcast);
			@class = Class944_0.GetPacket(headerSize + length + Gclass1488_0.ExtraPacketSizeForLayer);
			@class.Property = PacketProperty.Broadcast;
			Buffer.BlockCopy(data, start, @class.RawData, headerSize, length);
			int offset = 0;
			int length2 = length + headerSize;
			Gclass1488_0.ProcessOutBoundPacket(null, ref @class.RawData, ref offset, ref length2);
		}
		else
		{
			@class = Class944_0.GetWithData(PacketProperty.Broadcast, data, start, length);
		}
		bool result = Class946_0.SendBroadcast(@class.RawData, 0, @class.Size, port);
		Class944_0.Recycle(@class);
		return result;
	}

	public void TriggerUpdate()
	{
		AutoResetEvent_0.Set();
	}

	public void PollEvents()
	{
		if (UnsyncedEvents)
		{
			return;
		}
		int count;
		lock (Queue_0)
		{
			count = Queue_0.Count;
		}
		for (int i = 0; i < count; i++)
		{
			Class939 evt;
			lock (Queue_0)
			{
				evt = Queue_0.Dequeue();
			}
			method_12(evt);
		}
	}

	public GClass1477 Connect(string address, int port, string key)
	{
		return Connect(address, port, GClass1484.FromString(key));
	}

	public GClass1477 Connect(string address, int port, GClass1484 connectionData)
	{
		IPEndPoint target;
		try
		{
			target = GClass1479.MakeEndPoint(address, port);
		}
		catch
		{
			method_11(Class939.EType.Disconnect, null, null, SocketError.Success, 0, DisconnectReason.UnknownHost);
			return null;
		}
		return Connect(target, connectionData);
	}

	public GClass1477 Connect(IPEndPoint target, string key)
	{
		return Connect(target, GClass1484.FromString(key));
	}

	public GClass1477 Connect(IPEndPoint target, GClass1484 connectionData)
	{
		if (!Class946_0.IsRunning)
		{
			throw new InvalidOperationException("Client is not running");
		}
		byte connectNum = 0;
		lock (Dictionary_1)
		{
			if (Dictionary_1.ContainsKey(target))
			{
				return null;
			}
		}
		ReaderWriterLockSlim_0.EnterUpgradeableReadLock();
		if (Dictionary_0.TryGetValue(target, out var value))
		{
			ConnectionState connectionState = value.ConnectionState;
			if (connectionState == ConnectionState.Outgoing || connectionState == ConnectionState.Connected)
			{
				ReaderWriterLockSlim_0.ExitUpgradeableReadLock();
				return value;
			}
			connectNum = (byte)((value.Byte_0 + 1) % 4);
			method_2(value);
		}
		value = new GClass1477(this, target, method_19(), connectNum, connectionData);
		method_1(value);
		ReaderWriterLockSlim_0.ExitUpgradeableReadLock();
		return value;
	}

	public void Stop()
	{
		Stop(sendDisconnectMessages: true);
	}

	public void Stop(bool sendDisconnectMessages)
	{
		if (!Class946_0.IsRunning)
		{
			return;
		}
		for (GClass1477 gclass1477_ = Gclass1477_0; gclass1477_ != null; gclass1477_ = gclass1477_.Gclass1477_0)
		{
			gclass1477_.method_8(null, 0, 0, !sendDisconnectMessages);
		}
		Class946_0.Close(suspend: false);
		AutoResetEvent_0.Set();
		if (!Bool_0)
		{
			Thread_0.Join();
			Thread_0 = null;
		}
		ReaderWriterLockSlim_0.EnterWriteLock();
		Gclass1477_0 = null;
		Dictionary_0.Clear();
		Gclass1477_1 = new GClass1477[32];
		ReaderWriterLockSlim_0.ExitWriteLock();
		lock (Queue_1)
		{
			Queue_1.Clear();
		}
		Int_0 = 0;
		lock (Queue_0)
		{
			Queue_0.Clear();
		}
	}

	public int GetPeersCount(ConnectionState peerState)
	{
		int num = 0;
		ReaderWriterLockSlim_0.EnterReadLock();
		for (GClass1477 gclass1477_ = Gclass1477_0; gclass1477_ != null; gclass1477_ = gclass1477_.Gclass1477_0)
		{
			if ((gclass1477_.ConnectionState & peerState) != 0)
			{
				num++;
			}
		}
		ReaderWriterLockSlim_0.ExitReadLock();
		return num;
	}

	public void GetPeersNonAlloc(List<GClass1477> peers, ConnectionState peerState)
	{
		peers.Clear();
		ReaderWriterLockSlim_0.EnterReadLock();
		for (GClass1477 gclass1477_ = Gclass1477_0; gclass1477_ != null; gclass1477_ = gclass1477_.Gclass1477_0)
		{
			if ((gclass1477_.ConnectionState & peerState) != 0)
			{
				peers.Add(gclass1477_);
			}
		}
		ReaderWriterLockSlim_0.ExitReadLock();
	}

	public void DisconnectAll()
	{
		DisconnectAll(null, 0, 0);
	}

	public void DisconnectAll(byte[] data, int start, int count)
	{
		ReaderWriterLockSlim_0.EnterReadLock();
		for (GClass1477 gclass1477_ = Gclass1477_0; gclass1477_ != null; gclass1477_ = gclass1477_.Gclass1477_0)
		{
			method_10(gclass1477_, DisconnectReason.DisconnectPeerCalled, SocketError.Success, force: false, data, start, count, null);
		}
		ReaderWriterLockSlim_0.ExitReadLock();
	}

	public void DisconnectPeerForce(GClass1477 peer)
	{
		method_9(peer, DisconnectReason.DisconnectPeerCalled, SocketError.Success, null);
	}

	public void DisconnectPeer(GClass1477 peer)
	{
		DisconnectPeer(peer, null, 0, 0);
	}

	public void DisconnectPeer(GClass1477 peer, byte[] data)
	{
		DisconnectPeer(peer, data, 0, data.Length);
	}

	public void DisconnectPeer(GClass1477 peer, GClass1484 writer)
	{
		DisconnectPeer(peer, writer.Data, 0, writer.Length);
	}

	public void DisconnectPeer(GClass1477 peer, byte[] data, int start, int count)
	{
		method_10(peer, DisconnectReason.DisconnectPeerCalled, SocketError.Success, force: false, data, start, count, null);
	}

	public void CreateNtpRequest(IPEndPoint endPoint)
	{
		Dictionary_2.Add(endPoint, new Class983(endPoint));
	}

	public void CreateNtpRequest(string ntpServerAddress, int port)
	{
		IPEndPoint iPEndPoint = GClass1479.MakeEndPoint(ntpServerAddress, port);
		Dictionary_2.Add(iPEndPoint, new Class983(iPEndPoint));
	}

	public void CreateNtpRequest(string ntpServerAddress)
	{
		IPEndPoint iPEndPoint = GClass1479.MakeEndPoint(ntpServerAddress, 123);
		Dictionary_2.Add(iPEndPoint, new Class983(iPEndPoint));
	}

	public GStruct149 GetEnumerator()
	{
		return new GStruct149(Gclass1477_0);
	}

	public IEnumerator<GClass1477> GetEnumerator_1()
	{
		return new GStruct149(Gclass1477_0);
	}

	IEnumerator<GClass1477> IEnumerable<GClass1477>.GetEnumerator()
	{
		//ILSpy generated this explicit interface implementation from .override directive in GetEnumerator_1
		return this.GetEnumerator_1();
	}

	public IEnumerator GetEnumerator_1()
	{
		return new GStruct149(Gclass1477_0);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		//ILSpy generated this explicit interface implementation from .override directive in GetEnumerator_1
		return this.GetEnumerator_1();
	}
}
