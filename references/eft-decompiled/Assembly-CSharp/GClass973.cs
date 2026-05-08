using System.Collections.Generic;

public class GClass973 : IComparer<AnalyticSource>
{
	public int Compare(AnalyticSource x, AnalyticSource y)
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
