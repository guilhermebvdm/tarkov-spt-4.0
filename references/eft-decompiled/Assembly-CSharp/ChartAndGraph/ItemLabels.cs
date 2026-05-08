using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ChartAndGraph;

public class ItemLabels : AlignedItemLabels
{
	[Range(0f, 7f)]
	[SerializeField]
	private int fractionDigits;

	public int FractionDigits
	{
		get
		{
			return fractionDigits;
		}
		set
		{
			fractionDigits = value;
			RaiseOnUpdate();
		}
	}

	public override Action<IInternalUse, bool> Assign => delegate(IInternalUse x, bool clear)
	{
		if (clear)
		{
			if (x.ItemLabels == this)
			{
				x.ItemLabels = null;
			}
		}
		else if (x.ItemLabels != this)
		{
			x.ItemLabels = this;
		}
	};

	[CompilerGenerated]
	public void method_4(IInternalUse x, bool clear)
	{
		if (clear)
		{
			if (x.ItemLabels == this)
			{
				x.ItemLabels = null;
			}
		}
		else if (x.ItemLabels != this)
		{
			x.ItemLabels = this;
		}
	}
}
