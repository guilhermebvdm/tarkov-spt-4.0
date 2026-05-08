using BitPacking;
using EFT;

public struct PlayerInteractPacket
{
	public bool HasInteraction;

	public EInteractionType InteractionType;

	public byte SideId;

	public byte SlotId;

	public bool Fast;

	public bool Equals(PlayerInteractPacket other)
	{
		if (InteractionType == other.InteractionType && SideId == other.SideId && SlotId == other.SlotId)
		{
			return Fast == other.Fast;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is PlayerInteractPacket other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((int)InteractionType * 397) ^ SideId.GetHashCode() ^ SlotId.GetHashCode() ^ Fast.GetHashCode();
	}

	public void Serialize(GInterface131 writer)
	{
		writer.Write(HasInteraction);
		if (HasInteraction)
		{
			writer.WriteLimitedInt32((int)InteractionType, 5, 6, BitPackingTag.InteractWithBtr);
			writer.Write(SideId);
			writer.Write(SlotId);
			writer.Write(Fast);
		}
	}

	public void Deserialize(IDataReader reader)
	{
		if (reader.ReadBool())
		{
			HasInteraction = true;
			InteractionType = (EInteractionType)reader.ReadLimitedInt32(5, 6);
			SideId = reader.ReadByte();
			SlotId = reader.ReadByte();
			Fast = reader.ReadBool();
		}
	}
}
