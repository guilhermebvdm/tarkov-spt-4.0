using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using FlyingWormConsole3.LiteNetLib;

public class GClass1471 : IOnNetworkEvent, GInterface139, GInterface140
{
	public delegate void GDelegate51(GClass1477 peer);

	public delegate void GDelegate52(GClass1477 peer, DisconnectionInfoStruct disconnectInfo);

	public delegate void GDelegate53(IPEndPoint endPoint, SocketError socketError);

	public delegate void GDelegate54(GClass1477 peer, GClass1483 reader, DeliveryMethod deliveryMethod);

	public delegate void GDelegate55(IPEndPoint remoteEndPoint, GClass1483 reader, UnconnectedMessageType messageType);

	public delegate void GDelegate56(GClass1477 peer, int latency);

	public delegate void GDelegate57(GClass1470 request);

	public delegate void GDelegate58(GClass1477 peer, object userData);

	public delegate void GDelegate59(GClass1487 packet);

	[CompilerGenerated]
	private GDelegate51 gdelegate51_0;

	[CompilerGenerated]
	private GDelegate52 gdelegate52_0;

	[CompilerGenerated]
	private GDelegate53 gdelegate53_0;

	[CompilerGenerated]
	private GDelegate54 gdelegate54_0;

	[CompilerGenerated]
	private GDelegate55 gdelegate55_0;

	[CompilerGenerated]
	private GDelegate56 gdelegate56_0;

	[CompilerGenerated]
	private GDelegate57 gdelegate57_0;

	[CompilerGenerated]
	private GDelegate58 gdelegate58_0;

	[CompilerGenerated]
	private GDelegate59 gdelegate59_0;

	public event GDelegate51 PeerConnectedEvent
	{
		[CompilerGenerated]
		add
		{
			GDelegate51 gDelegate = gdelegate51_0;
			GDelegate51 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate51 value2 = (GDelegate51)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate51_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate51 gDelegate = gdelegate51_0;
			GDelegate51 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate51 value2 = (GDelegate51)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate51_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate52 PeerDisconnectedEvent
	{
		[CompilerGenerated]
		add
		{
			GDelegate52 gDelegate = gdelegate52_0;
			GDelegate52 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate52 value2 = (GDelegate52)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate52_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate52 gDelegate = gdelegate52_0;
			GDelegate52 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate52 value2 = (GDelegate52)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate52_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate53 NetworkErrorEvent
	{
		[CompilerGenerated]
		add
		{
			GDelegate53 gDelegate = gdelegate53_0;
			GDelegate53 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate53 value2 = (GDelegate53)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate53_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate53 gDelegate = gdelegate53_0;
			GDelegate53 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate53 value2 = (GDelegate53)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate53_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate54 NetworkReceiveEvent
	{
		[CompilerGenerated]
		add
		{
			GDelegate54 gDelegate = gdelegate54_0;
			GDelegate54 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate54 value2 = (GDelegate54)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate54_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate54 gDelegate = gdelegate54_0;
			GDelegate54 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate54 value2 = (GDelegate54)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate54_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate55 NetworkReceiveUnconnectedEvent
	{
		[CompilerGenerated]
		add
		{
			GDelegate55 gDelegate = gdelegate55_0;
			GDelegate55 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate55 value2 = (GDelegate55)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate55_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate55 gDelegate = gdelegate55_0;
			GDelegate55 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate55 value2 = (GDelegate55)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate55_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate56 NetworkLatencyUpdateEvent
	{
		[CompilerGenerated]
		add
		{
			GDelegate56 gDelegate = gdelegate56_0;
			GDelegate56 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate56 value2 = (GDelegate56)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate56_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate56 gDelegate = gdelegate56_0;
			GDelegate56 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate56 value2 = (GDelegate56)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate56_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate57 ConnectionRequestEvent
	{
		[CompilerGenerated]
		add
		{
			GDelegate57 gDelegate = gdelegate57_0;
			GDelegate57 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate57 value2 = (GDelegate57)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate57_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate57 gDelegate = gdelegate57_0;
			GDelegate57 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate57 value2 = (GDelegate57)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate57_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate58 DeliveryEvent
	{
		[CompilerGenerated]
		add
		{
			GDelegate58 gDelegate = gdelegate58_0;
			GDelegate58 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate58 value2 = (GDelegate58)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate58_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate58 gDelegate = gdelegate58_0;
			GDelegate58 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate58 value2 = (GDelegate58)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate58_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate59 NtpResponseEvent
	{
		[CompilerGenerated]
		add
		{
			GDelegate59 gDelegate = gdelegate59_0;
			GDelegate59 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate59 value2 = (GDelegate59)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate59_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate59 gDelegate = gdelegate59_0;
			GDelegate59 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate59 value2 = (GDelegate59)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate59_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public void ClearPeerConnectedEvent()
	{
		gdelegate51_0 = null;
	}

	public void ClearPeerDisconnectedEvent()
	{
		gdelegate52_0 = null;
	}

	public void ClearNetworkErrorEvent()
	{
		gdelegate53_0 = null;
	}

	public void ClearNetworkReceiveEvent()
	{
		gdelegate54_0 = null;
	}

	public void ClearNetworkReceiveUnconnectedEvent()
	{
		gdelegate55_0 = null;
	}

	public void ClearNetworkLatencyUpdateEvent()
	{
		gdelegate56_0 = null;
	}

	public void ClearConnectionRequestEvent()
	{
		gdelegate57_0 = null;
	}

	public void ClearDeliveryEvent()
	{
		gdelegate58_0 = null;
	}

	public void ClearNtpResponseEvent()
	{
		gdelegate59_0 = null;
	}

	public void OnPeerConnected(GClass1477 peer)
	{
		if (gdelegate51_0 != null)
		{
			gdelegate51_0(peer);
		}
	}

	void IOnNetworkEvent.OnPeerConnected(GClass1477 peer)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnPeerConnected
		this.OnPeerConnected(peer);
	}

	public void OnPeerDisconnected(GClass1477 peer, DisconnectionInfoStruct disconnectInfo)
	{
		if (gdelegate52_0 != null)
		{
			gdelegate52_0(peer, disconnectInfo);
		}
	}

	void IOnNetworkEvent.OnPeerDisconnected(GClass1477 peer, DisconnectionInfoStruct disconnectInfo)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnPeerDisconnected
		this.OnPeerDisconnected(peer, disconnectInfo);
	}

	public void OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
	{
		if (gdelegate53_0 != null)
		{
			gdelegate53_0(endPoint, socketErrorCode);
		}
	}

	void IOnNetworkEvent.OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnNetworkError
		this.OnNetworkError(endPoint, socketErrorCode);
	}

	public void OnNetworkReceive(GClass1477 peer, GClass1483 reader, DeliveryMethod deliveryMethod)
	{
		if (gdelegate54_0 != null)
		{
			gdelegate54_0(peer, reader, deliveryMethod);
		}
	}

	void IOnNetworkEvent.OnNetworkReceive(GClass1477 peer, GClass1483 reader, DeliveryMethod deliveryMethod)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnNetworkReceive
		this.OnNetworkReceive(peer, reader, deliveryMethod);
	}

	public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, GClass1483 reader, UnconnectedMessageType messageType)
	{
		if (gdelegate55_0 != null)
		{
			gdelegate55_0(remoteEndPoint, reader, messageType);
		}
	}

	void IOnNetworkEvent.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, GClass1483 reader, UnconnectedMessageType messageType)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnNetworkReceiveUnconnected
		this.OnNetworkReceiveUnconnected(remoteEndPoint, reader, messageType);
	}

	public void OnNetworkLatencyUpdate(GClass1477 peer, int latency)
	{
		if (gdelegate56_0 != null)
		{
			gdelegate56_0(peer, latency);
		}
	}

	void IOnNetworkEvent.OnNetworkLatencyUpdate(GClass1477 peer, int latency)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnNetworkLatencyUpdate
		this.OnNetworkLatencyUpdate(peer, latency);
	}

	public void OnConnectionRequest(GClass1470 request)
	{
		if (gdelegate57_0 != null)
		{
			gdelegate57_0(request);
		}
	}

	void IOnNetworkEvent.OnConnectionRequest(GClass1470 request)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnConnectionRequest
		this.OnConnectionRequest(request);
	}

	public void OnMessageDelivered(GClass1477 peer, object userData)
	{
		if (gdelegate58_0 != null)
		{
			gdelegate58_0(peer, userData);
		}
	}

	void GInterface139.OnMessageDelivered(GClass1477 peer, object userData)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnMessageDelivered
		this.OnMessageDelivered(peer, userData);
	}

	public void OnNtpResponse(GClass1487 packet)
	{
		if (gdelegate59_0 != null)
		{
			gdelegate59_0(packet);
		}
	}

	void GInterface140.OnNtpResponse(GClass1487 packet)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnNtpResponse
		this.OnNtpResponse(packet);
	}
}
