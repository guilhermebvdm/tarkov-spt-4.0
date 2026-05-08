using System.Collections.Generic;

public class GClass974 : IComparer<StencilShadow>
{
	public int Compare(StencilShadow x, StencilShadow y)
	{
		if (x.ActualDrawPriority == y.ActualDrawPriority)
		{
			if (x == y)
			{
				return 0;
			}
			if (x.GetInstanceID() >= y.GetInstanceID())
			{
				return 1;
			}
			return -1;
		}
		if (x.ActualDrawPriority >= y.ActualDrawPriority)
		{
			return 1;
		}
		return -1;
	}
}
