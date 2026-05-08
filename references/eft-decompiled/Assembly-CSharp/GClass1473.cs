using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using FlyingWormConsole3.LiteNetLib;

public class GClass1473
{
	public struct Struct255
	{
		public IPEndPoint LocalEndPoint;

		public IPEndPoint RemoteEndPoint;

		public string Token;
	}

	public struct Struct256
	{
		public IPEndPoint TargetEndPoint;

		public NatAddressType Type;

		public string Token;
	}

	public class Class936
	{
		[NonSerialized]
		[CompilerGenerated]
		public IPEndPoint IpendPoint_0;

		[NonSerialized]
		[CompilerGenerated]
		public string String_0;

		public IPEndPoint Internal
		{
			[CompilerGenerated]
			get
			{
				return IpendPoint_0;
			}
			[CompilerGenerated]
			set
			{
				IpendPoint_0 = value;
			}
		}

		public string Token
		{
			[CompilerGenerated]
			get
			{
				return String_0;
			}
			[CompilerGenerated]
			set
			{
				String_0 = value;
			}
		}
	}

	public class Class937
	{
		[NonSerialized]
		[CompilerGenerated]
		public IPEndPoint IpendPoint_0;

		[NonSerialized]
		[CompilerGenerated]
		public IPEndPoint IpendPoint_1;

		[NonSerialized]
		[CompilerGenerated]
		public string String_0;

		public IPEndPoint Internal
		{
			[CompilerGenerated]
			get
			{
				return IpendPoint_0;
			}
			[CompilerGenerated]
			set
			{
				IpendPoint_0 = value;
			}
		}

		public IPEndPoint External
		{
			[CompilerGenerated]
			get
			{
				return IpendPoint_1;
			}
			[CompilerGenerated]
			set
			{
				IpendPoint_1 = value;
			}
		}

		public string Token
		{
			[CompilerGenerated]
			get
			{
				return String_0;
			}
			[CompilerGenerated]
			set
			{
				String_0 = value;
			}
		}
	}

	public class Class938
	{
		[NonSerialized]
		[CompilerGenerated]
		public string String_0;

		[NonSerialized]
		[CompilerGenerated]
		public bool Bool_0;

		public string Token
		{
			[CompilerGenerated]
			get
			{
				return String_0;
			}
			[CompilerGenerated]
			set
			{
				String_0 = value;
			}
		}

		public bool IsExternal
		{
			[CompilerGenerated]
			get
			{
				return Bool_0;
			}
			[CompilerGenerated]
			set
			{
				Bool_0 = value;
			}
		}
	}

	[NonSerialized]
	public Class946 Class946_0;

	[NonSerialized]
	public Queue<Struct255> Queue_0 = new Queue<Struct255>();

	[NonSerialized]
	public Queue<Struct256> Queue_1 = new Queue<Struct256>();

	[NonSerialized]
	public GClass1482 Gclass1482_0 = new GClass1482();

	[NonSerialized]
	public GClass1484 Gclass1484_0 = new GClass1484();

	[NonSerialized]
	public GClass1485 Gclass1485_0 = new GClass1485(256);

	[NonSerialized]
	public INATIntroductionListener INATIntroductionListener;

	public const int MaxTokenLength = 256;

	public GClass1473(Class946 socket)
	{
		Class946_0 = socket;
		Gclass1485_0.SubscribeReusable<Class937>(method_3);
		Gclass1485_0.SubscribeReusable<Class936, IPEndPoint>(method_2);
		Gclass1485_0.SubscribeReusable<Class938, IPEndPoint>(method_4);
	}

	public void method_0(IPEndPoint senderEndPoint, Class941 packet)
	{
		lock (Gclass1482_0)
		{
			Gclass1482_0.SetSource(packet.RawData, 1, packet.Size);
			Gclass1485_0.ReadAllPackets(Gclass1482_0, senderEndPoint);
		}
	}

	public void Init(INATIntroductionListener listener)
	{
		INATIntroductionListener = listener;
	}

	public void method_1<T>(T packet, IPEndPoint target) where T : class, new()
	{
		SocketError errorCode = SocketError.Success;
		Gclass1484_0.Reset();
		Gclass1484_0.Put((byte)16);
		Gclass1485_0.Write(Gclass1484_0, packet);
		Class946_0.SendTo(Gclass1484_0.Data, 0, Gclass1484_0.Length, target, ref errorCode);
	}

	public void NatIntroduce(IPEndPoint hostInternal, IPEndPoint hostExternal, IPEndPoint clientInternal, IPEndPoint clientExternal, string additionalInfo)
	{
		Class937 @class = new Class937
		{
			Token = additionalInfo
		};
		@class.Internal = hostInternal;
		@class.External = hostExternal;
		method_1(@class, clientExternal);
		@class.Internal = clientInternal;
		@class.External = clientExternal;
		method_1(@class, hostExternal);
	}

	public void PollEvents()
	{
		if (INATIntroductionListener == null || (Queue_1.Count == 0 && Queue_0.Count == 0))
		{
			return;
		}
		lock (Queue_1)
		{
			while (Queue_1.Count > 0)
			{
				Struct256 @struct = Queue_1.Dequeue();
				INATIntroductionListener.OnNatIntroductionSuccess(@struct.TargetEndPoint, @struct.Type, @struct.Token);
			}
		}
		lock (Queue_0)
		{
			while (Queue_0.Count > 0)
			{
				Struct255 struct2 = Queue_0.Dequeue();
				INATIntroductionListener.OnNatIntroductionRequest(struct2.LocalEndPoint, struct2.RemoteEndPoint, struct2.Token);
			}
		}
	}

	public void SendNatIntroduceRequest(string host, int port, string additionalInfo)
	{
		SendNatIntroduceRequest(GClass1479.MakeEndPoint(host, port), additionalInfo);
	}

	public void SendNatIntroduceRequest(IPEndPoint masterServerEndPoint, string additionalInfo)
	{
		string localIp = GClass1479.GetLocalIp(LocalAddrType.IPv4);
		if (string.IsNullOrEmpty(localIp))
		{
			localIp = GClass1479.GetLocalIp(LocalAddrType.IPv6);
		}
		method_1(new Class936
		{
			Internal = GClass1479.MakeEndPoint(localIp, Class946_0.LocalPort),
			Token = additionalInfo
		}, masterServerEndPoint);
	}

	public void method_2(Class936 req, IPEndPoint senderEndPoint)
	{
		lock (Queue_0)
		{
			Queue_0.Enqueue(new Struct255
			{
				LocalEndPoint = req.Internal,
				RemoteEndPoint = senderEndPoint,
				Token = req.Token
			});
		}
	}

	public void method_3(Class937 req)
	{
		Class938 @class = new Class938
		{
			Token = req.Token
		};
		method_1(@class, req.Internal);
		SocketError errorCode = SocketError.Success;
		Class946_0.Ttl = 2;
		Class946_0.SendTo(new byte[1] { 17 }, 0, 1, req.External, ref errorCode);
		Class946_0.Ttl = 255;
		@class.IsExternal = true;
		method_1(@class, req.External);
	}

	public void method_4(Class938 req, IPEndPoint senderEndPoint)
	{
		lock (Queue_1)
		{
			Queue_1.Enqueue(new Struct256
			{
				TargetEndPoint = senderEndPoint,
				Type = (req.IsExternal ? NatAddressType.External : NatAddressType.Internal),
				Token = req.Token
			});
		}
	}
}
