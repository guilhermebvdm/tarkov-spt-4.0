using BitPacking;
using EFT;

public struct GStruct215
{
	public bool HasInteraction;

	public EInteractionType EInteractionType;

	public EInteractionStage Execute;

	public string Id;

	public string ItemId;

	public bool Equals(GStruct215 other)
	{
		if (EInteractionType == other.EInteractionType && string.Equals(Id, other.Id))
		{
			return Execute == other.Execute;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is GStruct215)
		{
			return Equals((GStruct215)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((int)EInteractionType * 397) ^ ((Id != null) ? Id.GetHashCode() : 0);
	}

	public void Serialize(GInterface131 writer)
	{
		writer.Write(HasInteraction);
		if (HasInteraction)
		{
			writer.WriteLimitedInt32((int)EInteractionType, 0, 4, BitPackingTag.InteractWithDoorPacket0);
			writer.Write(Id);
			writer.WriteLimitedInt32((int)Execute, 0, 2, BitPackingTag.InteractWithDoorPacket1);
		}
		if (EInteractionType == EInteractionType.Unlock)
		{
			writer.WriteLimitedString(ItemId, ' ', 'z', BitPackingTag.EInteractionTypeItemId, 1600u);
		}
	}

	public void Deserialize(IDataReader reader)
	{
		if (reader.ReadBool())
		{
			HasInteraction = true;
			EInteractionType = (EInteractionType)reader.ReadLimitedInt32(0, 4);
			Id = reader.ReadString(1600u);
			Execute = (EInteractionStage)reader.ReadLimitedInt32(0, 2);
			if (EInteractionType == EInteractionType.Unlock)
			{
				ItemId = reader.ReadLimitedString(' ', 'z', 1600u);
			}
		}
	}
}
