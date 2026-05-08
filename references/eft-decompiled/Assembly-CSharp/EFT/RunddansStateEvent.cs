using System;
using System.Collections.Generic;
using EFT.GlobalEvents;

namespace EFT;

public class RunddansStateEvent : SyncEventFromServer
{
	[field: NonSerialized]
	public int PlayerId { get; set; }

	[field: NonSerialized]
	public Dictionary<int, EventObject.EState> Objects { get; set; } = new Dictionary<int, EventObject.EState>();

	public void Invoke(int playerId, Dictionary<int, EventObject> objects)
	{
		PlayerId = playerId;
		Objects.Clear();
		foreach (var (key, eventObject2) in objects)
		{
			Objects.Add(key, eventObject2.State);
		}
		base.Invoke();
	}

	public override void Serialize(ref GInterface131 writerStream)
	{
		writerStream.Write(PlayerId);
		writerStream.Write((ushort)Objects.Count);
		foreach (var (value, value2) in Objects)
		{
			writerStream.Write(value);
			writerStream.Write(value2);
		}
	}

	public override void Deserialize(ref IDataReader readerStream)
	{
		PlayerId = readerStream.ReadInt32();
		Objects.Clear();
		ushort num = readerStream.ReadUInt16();
		for (ushort num2 = 0; num2 < num; num2++)
		{
			int key = readerStream.ReadInt32();
			EventObject.EState value = readerStream.ReadEnum<EventObject.EState>();
			Objects.Add(key, value);
		}
	}

	public override void Reset()
	{
		PlayerId = 0;
		Objects.Clear();
	}
}
