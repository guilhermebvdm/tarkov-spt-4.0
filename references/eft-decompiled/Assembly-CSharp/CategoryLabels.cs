using System;
using System.Runtime.CompilerServices;
using ChartAndGraph;
using UnityEngine;

[Serializable]
public class CategoryLabels : AlignedItemLabels
{
	public enum ChartCategoryLabelOptions
	{
		All,
		FirstOnly
	}

	[SerializeField]
	[Tooltip("Determines which labels are visible")]
	private ChartCategoryLabelOptions visibleLabels;

	public ChartCategoryLabelOptions VisibleLabels
	{
		get
		{
			return visibleLabels;
		}
		set
		{
			visibleLabels = value;
			RaiseOnChanged();
		}
	}

	public override Action<IInternalUse, bool> Assign => delegate(IInternalUse x, bool clear)
	{
		if (clear)
		{
			if (x.CategoryLabels == this)
			{
				x.CategoryLabels = null;
			}
		}
		else if (x.CategoryLabels != this)
		{
			x.CategoryLabels = this;
		}
	};

	[CompilerGenerated]
	public void method_4(IInternalUse x, bool clear)
	{
		if (clear)
		{
			if (x.CategoryLabels == this)
			{
				x.CategoryLabels = null;
			}
		}
		else if (x.CategoryLabels != this)
		{
			x.CategoryLabels = this;
		}
	}
}
