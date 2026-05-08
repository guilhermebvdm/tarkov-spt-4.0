using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

public abstract class GClass1809
{
	[Serializable]
	[CompilerGenerated]
	public class Class1125
	{
		public static readonly Class1125 class1125_0 = new Class1125();

		public static Func<IPAddress, bool> func_0;

		public bool method_0(IPAddress ip)
		{
			return ip.AddressFamily == AddressFamily.InterNetwork;
		}
	}

	public static string GetHostIP()
	{
		return Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault((IPAddress ip) => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString();
	}
}
