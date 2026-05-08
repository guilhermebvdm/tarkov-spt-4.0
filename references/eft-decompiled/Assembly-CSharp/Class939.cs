using System.Net;
using System.Net.Sockets;
using FlyingWormConsole3.LiteNetLib;

public class Class939
{
	public enum EType
	{
		Connect,
		Disconnect,
		Receive,
		ReceiveUnconnected,
		Error,
		ConnectionLatencyUpdated,
		Broadcast,
		ConnectionRequest,
		MessageDelivered
	}

	public Class939 Next;

	public EType Type;

	public GClass1477 Peer;

	public IPEndPoint RemoteEndPoint;

	public object UserData;

	public int Latency;

	public SocketError ErrorCode;

	public DisconnectReason DisconnectReason;

	public GClass1470 ConnectionRequest;

	public DeliveryMethod DeliveryMethod;

	public readonly GClass1483 DataReader;

	public Class939(GClass1476 manager)
	{
		DataReader = new GClass1483(manager, this);
	}
}
