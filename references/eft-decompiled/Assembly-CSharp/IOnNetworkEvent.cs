using System.Net;
using System.Net.Sockets;
using FlyingWormConsole3.LiteNetLib;

public interface IOnNetworkEvent
{
	void OnPeerConnected(GClass1477 peer);

	void OnPeerDisconnected(GClass1477 peer, DisconnectionInfoStruct disconnectInfo);

	void OnNetworkError(IPEndPoint endPoint, SocketError socketError);

	void OnNetworkReceive(GClass1477 peer, GClass1483 reader, DeliveryMethod deliveryMethod);

	void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, GClass1483 reader, UnconnectedMessageType messageType);

	void OnNetworkLatencyUpdate(GClass1477 peer, int latency);

	void OnConnectionRequest(GClass1470 request);
}
