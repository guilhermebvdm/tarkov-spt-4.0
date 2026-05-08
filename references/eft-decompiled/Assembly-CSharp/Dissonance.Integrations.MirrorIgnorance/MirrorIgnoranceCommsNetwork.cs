using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Dissonance.Datastructures;
using Dissonance.Extensions;
using Dissonance.Networking;
using JetBrains.Annotations;
using UnityEngine;

namespace Dissonance.Integrations.MirrorIgnorance;

[HelpURL("https://placeholder-software.co.uk/dissonance/docs/Basics/Quick-Start-MirrorIgnorance/")]
public class MirrorIgnoranceCommsNetwork : BaseCommsNetwork<GClass1277, GClass1276, GStruct117, Unit, Unit>
{
	[Serializable]
	[CompilerGenerated]
	public class Class902
	{
		public static readonly Class902 class902_0 = new Class902();

		public static Func<byte[]> func_0;

		public byte[] ctor_003Eb__13_0()
		{
			return new byte[1024];
		}
	}

	public short TypeCode = 18385;

	public bool Activated = true;

	private readonly ConcurrentPool<byte[]> _loopbackBuffers = new ConcurrentPool<byte[]>(8, () => new byte[1024]);

	private readonly List<ArraySegment<byte>> _loopbackQueue = new List<ArraySegment<byte>>();

	public override GClass1277 CreateServer(Unit details)
	{
		return new GClass1277(this);
	}

	public override GClass1276 CreateClient(Unit details)
	{
		return new GClass1276(this);
	}

	public override void Update()
	{
		if (base.IsInitialized)
		{
			if (Activated)
			{
				bool active = GClass3065.Default.Active;
				if (base.Mode.IsServerEnabled() != active || !base.Mode.IsClientEnabled())
				{
					RunAsClient(Unit.None);
				}
			}
			else if (base.Mode != NetworkMode.None)
			{
				Stop();
				_loopbackQueue.Clear();
			}
			for (int i = 0; i < _loopbackQueue.Count; i++)
			{
				base.Client?.NetworkReceivedPacket(_loopbackQueue[i]);
				_loopbackBuffers.Put(_loopbackQueue[i].Array);
			}
			_loopbackQueue.Clear();
		}
		base.Update();
	}

	public override void Initialize()
	{
		GClass3065.Default.AddMessageListener(TypeCode, NullMessageReceivedHandler);
		base.Initialize();
	}

	public bool PreprocessPacketToClient(ArraySegment<byte> packet, GStruct117 destination)
	{
		if (base.Server == null)
		{
			throw Log.CreatePossibleBugException("server packet preprocessing running, but this peer is not a server", "8f9dc0a0-1b48-4a7f-9bb6-f767b2542ab1");
		}
		if (base.Client == null)
		{
			return false;
		}
		if (GClass3064.Default.Connection != destination.Connection)
		{
			return false;
		}
		if (base.Client != null)
		{
			_loopbackQueue.Add(packet.CopyToSegment(_loopbackBuffers.Get()));
		}
		return true;
	}

	public bool PreprocessPacketToServer(ArraySegment<byte> packet)
	{
		if (base.Client == null)
		{
			throw Log.CreatePossibleBugException("client packet processing running, but this peer is not a client", "dd75dce4-e85c-4bb3-96ec-3a3636cc4fbe");
		}
		if (base.Server == null)
		{
			return false;
		}
		base.Server.NetworkReceivedPacket(new GStruct117(GClass3064.Default.Connection), packet);
		return true;
	}

	public static void NullMessageReceivedHandler([NotNull] GStruct416 netmsg)
	{
		if (Logs.GetLogLevel(LogCategory.Network) <= LogLevel.Trace)
		{
			Debug.Log("Discarding Dissonance network message");
		}
		EFTReaderClass eFTReaderClass = new EFTReaderClass(netmsg.buffer);
		int num = (int)GClass1285.ReadUInt(eFTReaderClass);
		for (int i = 0; i < num; i++)
		{
			eFTReaderClass.ReadByte();
		}
	}

	public ArraySegment<byte> CopyToArraySegment([NotNull] EFTReaderClass msg, ArraySegment<byte> segment)
	{
		if (msg == null)
		{
			throw new ArgumentNullException("msg");
		}
		byte[] array = segment.Array;
		if (array == null)
		{
			throw new ArgumentNullException("segment");
		}
		int num = (int)GClass1285.ReadUInt(msg);
		if (num > segment.Count)
		{
			throw Log.CreatePossibleBugException("receive buffer is too small", "A7387195-BF3D-4796-A362-6C64BB546445");
		}
		for (int i = 0; i < num; i++)
		{
			array[segment.Offset + i] = msg.ReadByte();
		}
		return new ArraySegment<byte>(array, segment.Offset, num);
	}

	public int CopyPacketToNetworkWriter(ArraySegment<byte> packet, [NotNull] EFTWriterClass writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		byte[] array = packet.Array;
		if (array == null)
		{
			throw new ArgumentNullException("packet");
		}
		writer.Reset();
		GClass1290.WriteShort(writer, TypeCode);
		GClass1290.WriteUInt(writer, (uint)packet.Count);
		for (int i = 0; i < packet.Count; i++)
		{
			writer.WriteByte(array[packet.Offset + i]);
		}
		return writer.Position;
	}
}
