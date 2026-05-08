using System;
using EFT.GlobalEvents;

namespace EFT;

public class RunddansMessagesEvent : SyncEventFromServer
{
	public enum EType
	{
		NoRequiredItem,
		NonInteractive
	}

	[field: NonSerialized]
	public int PlayerId { get; set; }

	[field: NonSerialized]
	public EType Type { get; set; }

	public void Invoke(int playerId, EType type)
	{
		PlayerId = playerId;
		Type = type;
		base.Invoke();
	}

	public override void Serialize(ref GInterface131 writerStream)
	{
		writerStream.Write(PlayerId);
		writerStream.Write(Type);
	}

	public override void Deserialize(ref IDataReader readerStream)
	{
		PlayerId = readerStream.ReadInt32();
		Type = readerStream.ReadEnum<EType>();
	}

	public override void Reset()
	{
		PlayerId = 0;
		Type = EType.NoRequiredItem;
	}
}
