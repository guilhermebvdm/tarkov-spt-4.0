using EFT.Vaulting;
using UnityEngine;

public struct GStruct223
{
	public bool HasCriticalData;

	public EVaultingStrategy VaultingStrategy;

	public Vector3 VaultingPoint;

	public float VaultingSpeed;

	public float VaultingHeight;

	public float VaultingLength;

	public float BehindObstacleHeight;

	public float AbsoluteForwardVelocity;

	public bool Equals(GStruct223 other)
	{
		if (object.Equals(VaultingStrategy, other.VaultingStrategy) && object.Equals(VaultingPoint, other.VaultingPoint) && object.Equals(VaultingHeight, other.VaultingHeight) && object.Equals(VaultingLength, other.VaultingLength) && object.Equals(BehindObstacleHeight, other.BehindObstacleHeight))
		{
			return object.Equals(AbsoluteForwardVelocity, other.AbsoluteForwardVelocity);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is GStruct223)
		{
			return Equals((GStruct223)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (VaultingStrategy.GetHashCode() * 1973) ^ (VaultingPoint.GetHashCode() * 1973) ^ (VaultingHeight.GetHashCode() * 1973) ^ (VaultingLength.GetHashCode() * 1973) ^ (BehindObstacleHeight.GetHashCode() * 1973) ^ (AbsoluteForwardVelocity.GetHashCode() * 1973);
	}

	public void Serialize(GInterface131 writer)
	{
		writer.Write(VaultingStrategy);
		writer.Write(VaultingPoint);
		writer.Write(VaultingHeight);
		writer.Write(VaultingLength);
		writer.Write(BehindObstacleHeight);
		writer.Write(AbsoluteForwardVelocity);
	}

	public void Deserialize(IDataReader reader)
	{
		VaultingStrategy = reader.ReadEnum<EVaultingStrategy>();
		VaultingPoint = reader.ReadVector3();
		VaultingHeight = reader.ReadFloat();
		VaultingLength = reader.ReadFloat();
		BehindObstacleHeight = reader.ReadFloat();
		AbsoluteForwardVelocity = reader.ReadFloat();
	}
}
