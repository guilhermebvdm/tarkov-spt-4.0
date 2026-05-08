using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT.Network;
using UnityEngine;

namespace EFT;

public abstract class AbstractSession : MonoBehaviour
{
	[CompilerGenerated]
	public class Class1108
	{
		public Action listener;

		public void method_0(NetworkChannel channel, short type, byte[] buffer, int bufferCount)
		{
			listener();
		}
	}

	[CompilerGenerated]
	public class Class1109
	{
		public Action<byte[], int> listener;

		public void method_0(NetworkChannel channel, short type, byte[] buffer, int bufferCount)
		{
			listener(buffer, bufferCount);
		}
	}

	[CompilerGenerated]
	public class Class1110
	{
		public Action<GStruct416> listener;

		public AbstractSession abstractSession_0;

		public void method_0(NetworkChannel channel, short type, byte[] buffer, int bufferCount)
		{
			listener(new GStruct416
			{
				Connection = abstractSession_0.Connection,
				Channel = channel,
				MessageType = type,
				buffer = new ArraySegment<byte>(buffer, 0, bufferCount)
			});
		}
	}

	[CompilerGenerated]
	private EMemberCategory ememberCategory_0;

	[CompilerGenerated]
	private string string_0;

	[CompilerGenerated]
	private string string_1;

	private readonly Dictionary<short, Action<NetworkChannel, short, byte[], int>> dictionary_0 = new Dictionary<short, Action<NetworkChannel, short, byte[], int>>();

	private GClass3071 gclass3071_0;

	[CompilerGenerated]
	private GClass3066 gclass3066_0;

	public EMemberCategory MemberCategory
	{
		[CompilerGenerated]
		get
		{
			return ememberCategory_0;
		}
		[CompilerGenerated]
		set
		{
			ememberCategory_0 = value;
		}
	}

	public string ProfileId
	{
		[CompilerGenerated]
		get
		{
			return string_0;
		}
		[CompilerGenerated]
		set
		{
			string_0 = value;
		}
	}

	public string Token
	{
		[CompilerGenerated]
		get
		{
			return string_1;
		}
		[CompilerGenerated]
		set
		{
			string_1 = value;
		}
	}

	public GClass3071 Connection
	{
		get
		{
			return gclass3071_0;
		}
		set
		{
			if (value == null)
			{
				gclass3071_0?.RemoveMessageListener(method_0);
				gclass3071_0 = value;
			}
			else
			{
				gclass3071_0 = value;
				gclass3071_0.AddMessageListener(method_0);
			}
		}
	}

	public GClass3066 NetworkCryptography
	{
		[CompilerGenerated]
		get
		{
			return gclass3066_0;
		}
		[CompilerGenerated]
		set
		{
			gclass3066_0 = value;
		}
	}

	public static T CreateSession<T>(Transform parent, string name, string profileId, string token) where T : AbstractSession
	{
		GameObject obj = new GameObject();
		obj.name = name;
		obj.transform.parent = parent;
		T val = obj.AddComponent<T>();
		val.ProfileId = profileId;
		val.Token = token;
		return val;
	}

	public virtual void ProfileResourcesLoadProgress(int id, float progress)
	{
	}

	public virtual void OnDestroy()
	{
	}

	public void method_0(NetworkChannel channel, short messageType, byte[] buffer, int bufferCount)
	{
		if (dictionary_0.TryGetValue(messageType, out var value))
		{
			ArraySegment<byte> arraySegment = NetworkCryptography.Decrypt(buffer, 0, bufferCount);
			value(channel, messageType, arraySegment.Array, arraySegment.Count);
		}
	}

	public void AddMessageListener(NetworkMessageType messageType, Action listener)
	{
		AddMessageListener(messageType, (Action<NetworkChannel, short, byte[], int>)delegate
		{
			listener();
		});
	}

	public void AddMessageListener(NetworkMessageType messageType, Action<byte[], int> listener)
	{
		AddMessageListener(messageType, delegate(NetworkChannel channel, short type, byte[] buffer, int bufferCount)
		{
			listener(buffer, bufferCount);
		});
	}

	public void AddMessageListener(NetworkMessageType messageType, Action<GStruct416> listener)
	{
		AddMessageListener(messageType, delegate(NetworkChannel channel, short type, byte[] buffer, int bufferCount)
		{
			listener(new GStruct416
			{
				Connection = Connection,
				Channel = channel,
				MessageType = type,
				buffer = new ArraySegment<byte>(buffer, 0, bufferCount)
			});
		});
	}

	public void AddMessageListener(NetworkMessageType messageType, Action<NetworkChannel, short, byte[], int> listener)
	{
		short key = (short)messageType;
		if (dictionary_0.ContainsKey(key))
		{
			throw new Exception("Can't add, listener exists");
		}
		dictionary_0.Add(key, listener);
	}

	public void RemoveMessageListener(NetworkMessageType messageType)
	{
		short num = (short)messageType;
		if (!dictionary_0.ContainsKey(num))
		{
			throw new Exception($"Can't remove, listener '{num}' does not exist");
		}
		dictionary_0.Remove(num);
	}

	public void Send(NetworkChannel channel, NetworkMessageType messageType)
	{
		Send(channel, messageType, Array.Empty<byte>(), 0, 0);
	}

	public void Send(NetworkChannel channel, NetworkMessageType messageType, GInterface382 message)
	{
		using GClass1288 gClass = GClass1291.Get();
		message.Serialize(gClass);
		Send(channel, messageType, gClass);
	}

	public void Send(NetworkChannel channel, NetworkMessageType messageType, ArraySegment<byte> buffer)
	{
		Send(channel, messageType, buffer.Array, buffer.Offset, buffer.Count);
	}

	public void Send(NetworkChannel channel, NetworkMessageType messageType, byte[] buffer, int bufferOffset, int bufferCount)
	{
		if (Connection != null)
		{
			ArraySegment<byte> arraySegment = NetworkCryptography.Encrypt(buffer, bufferOffset, bufferCount);
			Connection.Send(channel, (short)messageType, arraySegment.Array, arraySegment.Offset, arraySegment.Count);
		}
	}

	public AbstractSession()
	{
	}
}
