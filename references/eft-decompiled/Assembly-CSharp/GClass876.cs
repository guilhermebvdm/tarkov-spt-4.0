using System.Collections.Generic;

public class GClass876 : IEqualityComparer<GuidReference>
{
	public bool Equals(GuidReference x, GuidReference y)
	{
		return x.ObjectGuid == y.ObjectGuid;
	}

	public int GetHashCode(GuidReference obj)
	{
		return obj.ObjectGuid.GetHashCode();
	}
}
