using UnityEngine;

public struct MountingPacketStruct
{
	public enum EMountingCommand : byte
	{
		Enter,
		Exit,
		Update,
		StartLeaving
	}

	public bool HasCriticalData;

	public bool IsMounted;

	public Vector3 MountingPoint;

	public float CurrentMountingPointVerticalOffset;

	public short MountingDirection;

	public EMountingCommand MountingCommand;

	public float TransitionTime;

	public bool Equals(MountingPacketStruct other)
	{
		if (object.Equals(IsMounted, other.IsMounted) && object.Equals(MountingPoint, other.MountingPoint) && object.Equals(CurrentMountingPointVerticalOffset, other.CurrentMountingPointVerticalOffset) && object.Equals(MountingDirection, other.MountingDirection) && object.Equals(MountingCommand, other.MountingCommand))
		{
			return object.Equals(TransitionTime, other.TransitionTime);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is MountingPacketStruct other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (IsMounted.GetHashCode() * 1973) ^ (MountingPoint.GetHashCode() * 1973) ^ (CurrentMountingPointVerticalOffset.GetHashCode() * 1973) ^ (MountingDirection.GetHashCode() * 1973) ^ (MountingCommand.GetHashCode() * 1973) ^ (TransitionTime.GetHashCode() * 1973);
	}

	public void Serialize(GInterface131 writer)
	{
		writer.Write((byte)MountingCommand);
		if (MountingCommand == EMountingCommand.Update)
		{
			writer.Write(CurrentMountingPointVerticalOffset);
		}
		if (MountingCommand == EMountingCommand.Enter || MountingCommand == EMountingCommand.Exit)
		{
			writer.Write(IsMounted);
		}
		if (MountingCommand == EMountingCommand.Enter)
		{
			writer.Write(MountingPoint);
			writer.Write(MountingDirection);
			writer.Write(TransitionTime);
		}
	}

	public void Deserialize(IDataReader reader)
	{
		MountingCommand = (EMountingCommand)reader.ReadByte();
		if (MountingCommand == EMountingCommand.Update)
		{
			CurrentMountingPointVerticalOffset = reader.ReadFloat();
		}
		if (MountingCommand == EMountingCommand.Enter || MountingCommand == EMountingCommand.Exit)
		{
			IsMounted = reader.ReadBool();
		}
		if (MountingCommand == EMountingCommand.Enter)
		{
			MountingPoint = reader.ReadVector3();
			MountingDirection = reader.ReadInt16();
			TransitionTime = reader.ReadFloat();
		}
	}
}
