using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using FlyingWormConsole3.LiteNetLib;

public class Class946
{
	public const int ReceivePollingTime = 500000;

	[NonSerialized]
	public Socket Socket_0;

	[NonSerialized]
	public Socket Socket_1;

	[NonSerialized]
	public Thread Thread_0;

	[NonSerialized]
	public Thread Thread_1;

	[NonSerialized]
	public IPEndPoint IpendPoint_0;

	[NonSerialized]
	public IPEndPoint IpendPoint_1;

	[NonSerialized]
	public GClass1476 Gclass1476_0;

	[NonSerialized]
	public const int Int_0 = -1744830452;

	[NonSerialized]
	public static IPAddress Ipaddress_0;

	[NonSerialized]
	public static bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_1;

	public volatile bool IsRunning;

	public int LocalPort
	{
		[CompilerGenerated]
		get
		{
			return Int_1;
		}
		[CompilerGenerated]
		set
		{
			Int_1 = value;
		}
	}

	public short Ttl
	{
		get
		{
			if (Socket_0.AddressFamily == AddressFamily.InterNetworkV6)
			{
				return (short)Socket_0.GetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.HopLimit);
			}
			return Socket_0.Ttl;
		}
		set
		{
			if (Socket_0.AddressFamily == AddressFamily.InterNetworkV6)
			{
				Socket_0.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.HopLimit, value);
			}
			else
			{
				Socket_0.Ttl = value;
			}
		}
	}

	static Class946()
	{
		Ipaddress_0 = IPAddress.Parse("ff02::1");
		Bool_0 = Socket.OSSupportsIPv6;
	}

	public Class946(GClass1476 listener)
	{
		Gclass1476_0 = listener;
	}

	public bool method_0()
	{
		return IsRunning;
	}

	public bool method_1(SocketException ex, EndPoint bufferEndPoint)
	{
		switch (ex.SocketErrorCode)
		{
		case SocketError.Interrupted:
		case SocketError.NotSocket:
			return true;
		default:
			GClass1475.smethod_5("[R]Error code: {0} - {1}", (int)ex.SocketErrorCode, ex.ToString());
			Gclass1476_0.method_17(null, ex.SocketErrorCode, (IPEndPoint)bufferEndPoint);
			break;
		case SocketError.MessageSize:
		case SocketError.ConnectionReset:
		case SocketError.TimedOut:
			break;
		}
		return false;
	}

	public void ManualReceive()
	{
		method_2(Socket_0, IpendPoint_0);
		if (Socket_1 != null && Socket_1 != Socket_0)
		{
			method_2(Socket_1, IpendPoint_1);
		}
	}

	public bool method_2(Socket socket, EndPoint bufferEndPoint)
	{
		try
		{
			int num = socket.Available;
			if (num == 0)
			{
				return false;
			}
			while (num > 0)
			{
				Class941 packet = Gclass1476_0.Class944_0.GetPacket(GClass1474.Int_3);
				packet.Size = socket.ReceiveFrom(packet.RawData, 0, GClass1474.Int_3, SocketFlags.None, ref bufferEndPoint);
				Gclass1476_0.method_17(packet, SocketError.Success, (IPEndPoint)bufferEndPoint);
				num -= packet.Size;
			}
		}
		catch (SocketException ex)
		{
			return method_1(ex, bufferEndPoint);
		}
		catch (ObjectDisposedException)
		{
			return true;
		}
		return false;
	}

	public void method_3(object state)
	{
		Socket socket = (Socket)state;
		EndPoint remoteEP = new IPEndPoint((socket.AddressFamily == AddressFamily.InterNetwork) ? IPAddress.Any : IPAddress.IPv6Any, 0);
		while (method_0())
		{
			Class941 packet;
			try
			{
				if (socket.Available == 0 && !socket.Poll(500000, SelectMode.SelectRead))
				{
					continue;
				}
				packet = Gclass1476_0.Class944_0.GetPacket(GClass1474.Int_3);
				packet.Size = socket.ReceiveFrom(packet.RawData, 0, GClass1474.Int_3, SocketFlags.None, ref remoteEP);
				goto IL_0083;
			}
			catch (SocketException ex)
			{
				if (method_1(ex, remoteEP))
				{
					break;
				}
			}
			catch (ObjectDisposedException)
			{
				break;
			}
			continue;
			IL_0083:
			Gclass1476_0.method_17(packet, SocketError.Success, (IPEndPoint)remoteEP);
		}
	}

	public bool Bind(IPAddress addressIPv4, IPAddress addressIPv6, int port, bool reuseAddress, IPv6Mode ipv6Mode, bool manualMode)
	{
		if (method_0())
		{
			return false;
		}
		bool flag = ipv6Mode == IPv6Mode.DualMode && Bool_0;
		Socket_0 = new Socket(flag ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		if (!method_4(Socket_0, new IPEndPoint(flag ? addressIPv6 : addressIPv4, port), reuseAddress, ipv6Mode))
		{
			return false;
		}
		LocalPort = ((IPEndPoint)Socket_0.LocalEndPoint).Port;
		if (flag)
		{
			Socket_1 = Socket_0;
		}
		IsRunning = true;
		if (!manualMode)
		{
			Thread_0 = new Thread(method_3)
			{
				Name = "SocketThreadv4(" + LocalPort + ")",
				IsBackground = true
			};
			Thread_0.Start(Socket_0);
		}
		else
		{
			IpendPoint_0 = new IPEndPoint(IPAddress.Any, 0);
		}
		if (Bool_0 && ipv6Mode == IPv6Mode.SeparateSocket)
		{
			Socket_1 = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
			if (method_4(Socket_1, new IPEndPoint(addressIPv6, LocalPort), reuseAddress, ipv6Mode))
			{
				if (manualMode)
				{
					IpendPoint_1 = new IPEndPoint(IPAddress.IPv6Any, 0);
				}
				else
				{
					Thread_1 = new Thread(method_3)
					{
						Name = "SocketThreadv6(" + LocalPort + ")",
						IsBackground = true
					};
					Thread_1.Start(Socket_1);
				}
			}
			return true;
		}
		return true;
	}

	public bool method_4(Socket socket, IPEndPoint ep, bool reuseAddress, IPv6Mode ipv6Mode)
	{
		socket.ReceiveTimeout = 500;
		socket.SendTimeout = 500;
		socket.ReceiveBufferSize = 1048576;
		socket.SendBufferSize = 1048576;
		try
		{
			socket.IOControl(-1744830452, new byte[1], null);
		}
		catch
		{
		}
		try
		{
			socket.ExclusiveAddressUse = !reuseAddress;
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, reuseAddress);
		}
		catch
		{
		}
		if (socket.AddressFamily == AddressFamily.InterNetwork)
		{
			Ttl = 255;
			try
			{
				socket.DontFragment = true;
			}
			catch (SocketException ex)
			{
				GClass1475.smethod_5("[B]DontFragment error: {0}", ex.SocketErrorCode);
			}
			try
			{
				socket.EnableBroadcast = true;
			}
			catch (SocketException ex2)
			{
				GClass1475.smethod_5("[B]Broadcast error: {0}", ex2.SocketErrorCode);
			}
		}
		else if (ipv6Mode == IPv6Mode.DualMode)
		{
			try
			{
				socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, optionValue: false);
			}
			catch (Exception ex3)
			{
				GClass1475.smethod_5("[B]Bind exception (dualmode setting): {0}", ex3.ToString());
			}
		}
		try
		{
			socket.Bind(ep);
			_ = socket.AddressFamily;
		}
		catch (SocketException ex4)
		{
			SocketError socketErrorCode = ex4.SocketErrorCode;
			if (socketErrorCode != SocketError.AddressFamilyNotSupported)
			{
				if (socketErrorCode == SocketError.AddressAlreadyInUse && socket.AddressFamily == AddressFamily.InterNetworkV6 && ipv6Mode != IPv6Mode.DualMode)
				{
					try
					{
						socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, optionValue: true);
						socket.Bind(ep);
					}
					catch (SocketException ex5)
					{
						GClass1475.smethod_5("[B]Bind exception: {0}, errorCode: {1}", ex5.ToString(), ex5.SocketErrorCode);
						return false;
					}
					return true;
				}
				GClass1475.smethod_5("[B]Bind exception: {0}, errorCode: {1}", ex4.ToString(), ex4.SocketErrorCode);
				return false;
			}
			return true;
		}
		return true;
	}

	public bool SendBroadcast(byte[] data, int offset, int size, int port)
	{
		if (!method_0())
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		try
		{
			flag = Socket_0.SendTo(data, offset, size, SocketFlags.None, new IPEndPoint(IPAddress.Broadcast, port)) > 0;
			if (Socket_1 != null)
			{
				flag2 = Socket_1.SendTo(data, offset, size, SocketFlags.None, new IPEndPoint(Ipaddress_0, port)) > 0;
			}
		}
		catch (Exception ex)
		{
			GClass1475.smethod_5("[S][MCAST]" + ex);
			return flag;
		}
		return flag || flag2;
	}

	public int SendTo(byte[] data, int offset, int size, IPEndPoint remoteEndPoint, ref SocketError errorCode)
	{
		if (!method_0())
		{
			return 0;
		}
		try
		{
			Socket socket = Socket_0;
			if (remoteEndPoint.AddressFamily == AddressFamily.InterNetworkV6 && Bool_0)
			{
				socket = Socket_1;
			}
			return socket.SendTo(data, offset, size, SocketFlags.None, remoteEndPoint);
		}
		catch (SocketException ex)
		{
			SocketError socketErrorCode = ex.SocketErrorCode;
			if (socketErrorCode != SocketError.Interrupted)
			{
				if (socketErrorCode != SocketError.MessageSize)
				{
					if (socketErrorCode == SocketError.NoBufferSpaceAvailable)
					{
						goto IL_008a;
					}
					GClass1475.smethod_5("[S]" + ex);
				}
				errorCode = ex.SocketErrorCode;
				return -1;
			}
			goto IL_008a;
			IL_008a:
			return 0;
		}
		catch (Exception ex2)
		{
			GClass1475.smethod_5("[S]" + ex2);
			return -1;
		}
	}

	public void Close(bool suspend)
	{
		if (!suspend)
		{
			IsRunning = false;
		}
		if (Socket_0 == Socket_1)
		{
			Socket_1 = null;
		}
		if (Socket_0 != null)
		{
			Socket_0.Close();
		}
		if (Socket_1 != null)
		{
			Socket_1.Close();
		}
		Socket_0 = null;
		Socket_1 = null;
		if (Thread_0 != null && Thread_0 != Thread.CurrentThread)
		{
			Thread_0.Join();
		}
		if (Thread_1 != null && Thread_1 != Thread.CurrentThread)
		{
			Thread_1.Join();
		}
		Thread_0 = null;
		Thread_1 = null;
	}
}
