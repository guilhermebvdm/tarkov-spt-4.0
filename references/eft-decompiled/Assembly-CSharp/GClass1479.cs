using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using FlyingWormConsole3.LiteNetLib;

public abstract class GClass1479
{
	[NonSerialized]
	public static List<string> List_0 = new List<string>();

	public static IPEndPoint MakeEndPoint(string hostStr, int port)
	{
		return new IPEndPoint(ResolveAddress(hostStr), port);
	}

	public static IPAddress ResolveAddress(string hostStr)
	{
		if (hostStr == "localhost")
		{
			return IPAddress.Loopback;
		}
		if (!IPAddress.TryParse(hostStr, out var address))
		{
			if (Class946.Bool_0)
			{
				address = ResolveAddress(hostStr, AddressFamily.InterNetworkV6);
			}
			if (address == null)
			{
				address = ResolveAddress(hostStr, AddressFamily.InterNetwork);
			}
		}
		if (address == null)
		{
			throw new ArgumentException("Invalid address: " + hostStr);
		}
		return address;
	}

	public static IPAddress ResolveAddress(string hostStr, AddressFamily addressFamily)
	{
		IPAddress[] addressList = Dns.GetHostEntry(hostStr).AddressList;
		int num = 0;
		IPAddress iPAddress;
		while (true)
		{
			if (num < addressList.Length)
			{
				iPAddress = addressList[num];
				if (iPAddress.AddressFamily == addressFamily)
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return iPAddress;
	}

	public static List<string> GetLocalIpList(LocalAddrType addrType)
	{
		List<string> list = new List<string>();
		GetLocalIpList(list, addrType);
		return list;
	}

	public static void GetLocalIpList(IList<string> targetList, LocalAddrType addrType)
	{
		bool flag = (addrType & LocalAddrType.IPv4) == LocalAddrType.IPv4;
		bool flag2 = (addrType & LocalAddrType.IPv6) == LocalAddrType.IPv6;
		try
		{
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface networkInterface in allNetworkInterfaces)
			{
				if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback || networkInterface.OperationalStatus != OperationalStatus.Up)
				{
					continue;
				}
				IPInterfaceProperties iPProperties = networkInterface.GetIPProperties();
				if (iPProperties.GatewayAddresses.Count == 0)
				{
					continue;
				}
				foreach (UnicastIPAddressInformation unicastAddress in iPProperties.UnicastAddresses)
				{
					IPAddress address = unicastAddress.Address;
					if ((flag && address.AddressFamily == AddressFamily.InterNetwork) || (flag2 && address.AddressFamily == AddressFamily.InterNetworkV6))
					{
						targetList.Add(address.ToString());
					}
				}
			}
		}
		catch
		{
		}
		if (targetList.Count == 0)
		{
			IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
			foreach (IPAddress iPAddress in addressList)
			{
				if ((flag && iPAddress.AddressFamily == AddressFamily.InterNetwork) || (flag2 && iPAddress.AddressFamily == AddressFamily.InterNetworkV6))
				{
					targetList.Add(iPAddress.ToString());
				}
			}
		}
		if (targetList.Count == 0)
		{
			if (flag)
			{
				targetList.Add("127.0.0.1");
			}
			if (flag2)
			{
				targetList.Add("::1");
			}
		}
	}

	public static string GetLocalIp(LocalAddrType addrType)
	{
		lock (List_0)
		{
			List_0.Clear();
			GetLocalIpList(List_0, addrType);
			return (List_0.Count == 0) ? string.Empty : List_0[0];
		}
	}

	public static void smethod_0()
	{
		try
		{
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			for (int i = 0; i < allNetworkInterfaces.Length; i++)
			{
				foreach (UnicastIPAddressInformation unicastAddress in allNetworkInterfaces[i].GetIPProperties().UnicastAddresses)
				{
					if (unicastAddress.Address.AddressFamily != AddressFamily.InterNetwork)
					{
						_ = unicastAddress.Address.AddressFamily;
					}
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public static int smethod_1(int number, int expected)
	{
		return (number - expected + 32768 + 16384) % 32768 - 16384;
	}
}
