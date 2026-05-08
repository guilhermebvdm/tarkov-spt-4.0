using System;
using System.Net;
using System.Net.Sockets;

public class Class983
{
	[NonSerialized]
	public const int Int_0 = 1000;

	[NonSerialized]
	public const int Int_1 = 10000;

	public const int DefaultPort = 123;

	[NonSerialized]
	public IPEndPoint IpendPoint_0;

	[NonSerialized]
	public int Int_2 = 1000;

	[NonSerialized]
	public int Int_3;

	public bool NeedToKill => Int_3 >= 10000;

	public Class983(IPEndPoint endPoint)
	{
		IpendPoint_0 = endPoint;
	}

	public bool Send(Class946 socket, int time)
	{
		Int_2 += time;
		Int_3 += time;
		if (Int_2 < 1000)
		{
			return false;
		}
		SocketError errorCode = SocketError.Success;
		GClass1487 gClass = new GClass1487();
		int num = socket.SendTo(gClass.Bytes, 0, gClass.Bytes.Length, IpendPoint_0, ref errorCode);
		if (errorCode == SocketError.Success)
		{
			return num == gClass.Bytes.Length;
		}
		return false;
	}
}
