using System;
using System.Runtime.CompilerServices;

namespace ChartAndGraph;

public class VerticalAxis : AxisBase
{
	public override Action<IInternalUse, bool> Assign => delegate(IInternalUse x, bool clear)
	{
		if (clear)
		{
			if (x.VerticalAxis == this)
			{
				x.VerticalAxis = null;
			}
		}
		else if (x.VerticalAxis != this)
		{
			x.VerticalAxis = this;
		}
	};

	[CompilerGenerated]
	public void method_11(IInternalUse x, bool clear)
	{
		if (clear)
		{
			if (x.VerticalAxis == this)
			{
				x.VerticalAxis = null;
			}
		}
		else if (x.VerticalAxis != this)
		{
			x.VerticalAxis = this;
		}
	}
}
