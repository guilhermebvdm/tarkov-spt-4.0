using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FlyingWormConsole3.LiteNetLib;

public class GClass1485
{
	public abstract class Class947<T>
	{
		public static readonly ulong Id;

		static Class947()
		{
			ulong num = 14695981039346656037uL;
			string text = typeof(T).ToString();
			for (int i = 0; i < text.Length; i++)
			{
				num ^= text[i];
				num *= 1099511628211L;
			}
			Id = num;
		}
	}

	public delegate void SubscribeDelegate(GClass1482 reader, object userData);

	[CompilerGenerated]
	public class Class948<T> where T : class, new()
	{
		public Func<T> packetConstructor;

		public GClass1485 gclass1485_0;

		public Action<T> onReceive;

		public void method_0(GClass1482 reader, object userData)
		{
			T val = packetConstructor();
			gclass1485_0.Gclass1486_0.Deserialize(reader, val);
			onReceive(val);
		}
	}

	[CompilerGenerated]
	public class Class949<T, U> where T : class, new()
	{
		public Func<T> packetConstructor;

		public GClass1485 gclass1485_0;

		public Action<T, U> onReceive;

		public void method_0(GClass1482 reader, object userData)
		{
			T val = packetConstructor();
			gclass1485_0.Gclass1486_0.Deserialize(reader, val);
			onReceive(val, (U)userData);
		}
	}

	[CompilerGenerated]
	public class Class950<T> where T : class, new()
	{
		public GClass1485 gclass1485_0;

		public T reference;

		public Action<T> onReceive;

		public void method_0(GClass1482 reader, object userData)
		{
			gclass1485_0.Gclass1486_0.Deserialize(reader, reference);
			onReceive(reference);
		}
	}

	[CompilerGenerated]
	public class Class951<T, U> where T : class, new()
	{
		public GClass1485 gclass1485_0;

		public T reference;

		public Action<T, U> onReceive;

		public void method_0(GClass1482 reader, object userData)
		{
			gclass1485_0.Gclass1486_0.Deserialize(reader, reference);
			onReceive(reference, (U)userData);
		}
	}

	[CompilerGenerated]
	public class Class952<T, U> where T : GInterface143
	{
		public Func<T> packetConstructor;

		public Action<T, U> onReceive;

		public void method_0(GClass1482 reader, object userData)
		{
			T arg = packetConstructor();
			arg.Deserialize(reader);
			onReceive(arg, (U)userData);
		}
	}

	[CompilerGenerated]
	public class Class953<T> where T : GInterface143
	{
		public Func<T> packetConstructor;

		public Action<T> onReceive;

		public void method_0(GClass1482 reader, object userData)
		{
			T obj = packetConstructor();
			obj.Deserialize(reader);
			onReceive(obj);
		}
	}

	[CompilerGenerated]
	public class Class954<T, U> where T : GInterface143, new()
	{
		public T reference;

		public Action<T, U> onReceive;

		public void method_0(GClass1482 reader, object userData)
		{
			reference.Deserialize(reader);
			onReceive(reference, (U)userData);
		}
	}

	[CompilerGenerated]
	public class Class955<T> where T : GInterface143, new()
	{
		public T reference;

		public Action<T> onReceive;

		public void method_0(GClass1482 reader, object userData)
		{
			reference.Deserialize(reader);
			onReceive(reference);
		}
	}

	[NonSerialized]
	public GClass1486 Gclass1486_0;

	[NonSerialized]
	public Dictionary<ulong, SubscribeDelegate> Dictionary_0 = new Dictionary<ulong, SubscribeDelegate>();

	[NonSerialized]
	public GClass1484 Gclass1484_0 = new GClass1484();

	public GClass1485()
	{
		Gclass1486_0 = new GClass1486();
	}

	public GClass1485(int maxStringLength)
	{
		Gclass1486_0 = new GClass1486(maxStringLength);
	}

	public virtual ulong GetHash<T>()
	{
		return Class947<T>.Id;
	}

	public virtual SubscribeDelegate GetCallbackFromData(GClass1482 reader)
	{
		ulong uLong = reader.GetULong();
		if (!Dictionary_0.TryGetValue(uLong, out var value))
		{
			throw new GException14("Undefined packet in NetDataReader");
		}
		return value;
	}

	public virtual void WriteHash<T>(GClass1484 writer)
	{
		writer.Put(GetHash<T>());
	}

	public void RegisterNestedType<T>() where T : struct, GInterface143
	{
		Gclass1486_0.RegisterNestedType<T>();
	}

	public void RegisterNestedType<T>(Action<GClass1484, T> writeDelegate, Func<GClass1482, T> readDelegate)
	{
		Gclass1486_0.RegisterNestedType(writeDelegate, readDelegate);
	}

	public void RegisterNestedType<T>(Func<T> constructor) where T : class, GInterface143
	{
		Gclass1486_0.RegisterNestedType(constructor);
	}

	public void ReadAllPackets(GClass1482 reader)
	{
		while (reader.AvailableBytes > 0)
		{
			ReadPacket(reader);
		}
	}

	public void ReadAllPackets(GClass1482 reader, object userData)
	{
		while (reader.AvailableBytes > 0)
		{
			ReadPacket(reader, userData);
		}
	}

	public void ReadPacket(GClass1482 reader)
	{
		ReadPacket(reader, null);
	}

	public void Send<T>(GClass1477 peer, T packet, DeliveryMethod options) where T : class, new()
	{
		Gclass1484_0.Reset();
		Write(Gclass1484_0, packet);
		peer.Send(Gclass1484_0, options);
	}

	public void SendNetSerializable<T>(GClass1477 peer, T packet, DeliveryMethod options) where T : GInterface143
	{
		Gclass1484_0.Reset();
		WriteNetSerializable(Gclass1484_0, packet);
		peer.Send(Gclass1484_0, options);
	}

	public void Send<T>(GClass1476 manager, T packet, DeliveryMethod options) where T : class, new()
	{
		Gclass1484_0.Reset();
		Write(Gclass1484_0, packet);
		manager.SendToAll(Gclass1484_0, options);
	}

	public void SendNetSerializable<T>(GClass1476 manager, T packet, DeliveryMethod options) where T : GInterface143
	{
		Gclass1484_0.Reset();
		WriteNetSerializable(Gclass1484_0, packet);
		manager.SendToAll(Gclass1484_0, options);
	}

	public void Write<T>(GClass1484 writer, T packet) where T : class, new()
	{
		WriteHash<T>(writer);
		Gclass1486_0.Serialize(writer, packet);
	}

	public void WriteNetSerializable<T>(GClass1484 writer, T packet) where T : GInterface143
	{
		WriteHash<T>(writer);
		packet.Serialize(writer);
	}

	public byte[] Write<T>(T packet) where T : class, new()
	{
		Gclass1484_0.Reset();
		WriteHash<T>(Gclass1484_0);
		Gclass1486_0.Serialize(Gclass1484_0, packet);
		return Gclass1484_0.CopyData();
	}

	public byte[] WriteNetSerializable<T>(T packet) where T : GInterface143
	{
		Gclass1484_0.Reset();
		WriteHash<T>(Gclass1484_0);
		packet.Serialize(Gclass1484_0);
		return Gclass1484_0.CopyData();
	}

	public void ReadPacket(GClass1482 reader, object userData)
	{
		GetCallbackFromData(reader)(reader, userData);
	}

	public void Subscribe<T>(Action<T> onReceive, Func<T> packetConstructor) where T : class, new()
	{
		Gclass1486_0.Register<T>();
		Dictionary_0[GetHash<T>()] = delegate(GClass1482 reader, object userData)
		{
			T val = packetConstructor();
			Gclass1486_0.Deserialize(reader, val);
			onReceive(val);
		};
	}

	public void Subscribe<T, TUserData>(Action<T, TUserData> onReceive, Func<T> packetConstructor) where T : class, new()
	{
		Gclass1486_0.Register<T>();
		Dictionary_0[GetHash<T>()] = delegate(GClass1482 reader, object userData)
		{
			T val = packetConstructor();
			Gclass1486_0.Deserialize(reader, val);
			onReceive(val, (TUserData)userData);
		};
	}

	public void SubscribeReusable<T>(Action<T> onReceive) where T : class, new()
	{
		Gclass1486_0.Register<T>();
		T reference = new T();
		Dictionary_0[GetHash<T>()] = delegate(GClass1482 reader, object userData)
		{
			Gclass1486_0.Deserialize(reader, reference);
			onReceive(reference);
		};
	}

	public void SubscribeReusable<T, TUserData>(Action<T, TUserData> onReceive) where T : class, new()
	{
		Gclass1486_0.Register<T>();
		T reference = new T();
		Dictionary_0[GetHash<T>()] = delegate(GClass1482 reader, object userData)
		{
			Gclass1486_0.Deserialize(reader, reference);
			onReceive(reference, (TUserData)userData);
		};
	}

	public void SubscribeNetSerializable<T, TUserData>(Action<T, TUserData> onReceive, Func<T> packetConstructor) where T : GInterface143
	{
		Dictionary_0[GetHash<T>()] = delegate(GClass1482 reader, object userData)
		{
			T arg = packetConstructor();
			arg.Deserialize(reader);
			onReceive(arg, (TUserData)userData);
		};
	}

	public void SubscribeNetSerializable<T>(Action<T> onReceive, Func<T> packetConstructor) where T : GInterface143
	{
		Dictionary_0[GetHash<T>()] = delegate(GClass1482 reader, object userData)
		{
			T obj = packetConstructor();
			obj.Deserialize(reader);
			onReceive(obj);
		};
	}

	public void SubscribeNetSerializable<T, TUserData>(Action<T, TUserData> onReceive) where T : GInterface143, new()
	{
		T reference = new T();
		Dictionary_0[GetHash<T>()] = delegate(GClass1482 reader, object userData)
		{
			reference.Deserialize(reader);
			onReceive(reference, (TUserData)userData);
		};
	}

	public void SubscribeNetSerializable<T>(Action<T> onReceive) where T : GInterface143, new()
	{
		T reference = new T();
		Dictionary_0[GetHash<T>()] = delegate(GClass1482 reader, object userData)
		{
			reference.Deserialize(reader);
			onReceive(reference);
		};
	}

	public bool RemoveSubscription<T>()
	{
		return Dictionary_0.Remove(GetHash<T>());
	}
}
