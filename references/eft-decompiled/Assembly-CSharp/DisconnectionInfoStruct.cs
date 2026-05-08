using System.Net.Sockets;
using FlyingWormConsole3.LiteNetLib;

public struct DisconnectionInfoStruct
{
	public DisconnectReason Reason;

	public SocketError SocketErrorCode;

	public GClass1483 AdditionalData;
}
