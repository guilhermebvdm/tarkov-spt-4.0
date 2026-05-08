using System.Collections.Generic;
using Koenigz.PerfectCulling;

public class GClass1213 : IEqualityComparer<PerfectCullingBakeGroup>
{
	public bool Equals(PerfectCullingBakeGroup x, PerfectCullingBakeGroup y)
	{
		if (x != null && y != null)
		{
			if (x.renderers?.Length != y.renderers?.Length)
			{
				return false;
			}
			if (x.groupType != y.groupType)
			{
				return false;
			}
			if (x.renderers != null && y.renderers != null)
			{
				for (int i = 0; i < x.renderers.Length; i++)
				{
					if (x.renderers[i] != y.renderers[i])
					{
						return false;
					}
				}
			}
			return true;
		}
		return false;
	}

	public int GetHashCode(PerfectCullingBakeGroup obj)
	{
		if (obj == null)
		{
			return 0;
		}
		int num = 17;
		num = (int)(221 + obj.groupType);
		for (int i = 0; i < obj.renderers?.Length; i++)
		{
			num = num * 13 + obj.renderers[i].GetInstanceID();
		}
		return num;
	}
}
