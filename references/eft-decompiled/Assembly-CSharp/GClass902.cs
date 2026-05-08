using System.Collections.Generic;

public class GClass902 : IEqualityComparer<SpatialAudioRoom>
{
	public bool Equals(SpatialAudioRoom x, SpatialAudioRoom y)
	{
		if (!(x == null) && !(y == null))
		{
			if (x.ID != y.ID)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public int GetHashCode(SpatialAudioRoom obj)
	{
		if (obj == null)
		{
			return 0;
		}
		return 221 + obj.ID;
	}
}
