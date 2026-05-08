public struct StationaryPacketStruct
{
	public enum EStationaryCommand : byte
	{
		Occupy,
		Leave,
		Denied
	}

	public bool HasInteraction;

	public string Id;

	public EStationaryCommand StationaryCommand;

	public bool FinalizeObserverCrutch;

	public bool IsStationaryFinal;

	public void Serialize(GInterface131 writer)
	{
		writer.Write(HasInteraction);
		if (HasInteraction)
		{
			writer.Write((byte)StationaryCommand);
			if (StationaryCommand == EStationaryCommand.Occupy)
			{
				writer.Write(Id);
			}
		}
		writer.Write(FinalizeObserverCrutch);
		if (FinalizeObserverCrutch)
		{
			writer.Write(IsStationaryFinal);
		}
	}

	public void Deserialize(IDataReader reader)
	{
		if (reader.ReadBool())
		{
			HasInteraction = true;
			StationaryCommand = (EStationaryCommand)reader.ReadByte();
			if (StationaryCommand == EStationaryCommand.Occupy)
			{
				Id = reader.ReadString(1600u);
			}
		}
		FinalizeObserverCrutch = reader.ReadBool();
		if (FinalizeObserverCrutch)
		{
			IsStationaryFinal = reader.ReadBool();
		}
	}

	public bool Equals(StationaryPacketStruct other)
	{
		if (HasInteraction == other.HasInteraction && string.Equals(Id, other.Id))
		{
			return StationaryCommand == other.StationaryCommand;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is StationaryPacketStruct other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((HasInteraction.GetHashCode() * 397) ^ ((Id != null) ? Id.GetHashCode() : 0)) * 397) ^ StationaryCommand.GetHashCode();
	}

	public static bool operator ==(StationaryPacketStruct left, StationaryPacketStruct right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(StationaryPacketStruct left, StationaryPacketStruct right)
	{
		return !left.Equals(right);
	}
}
