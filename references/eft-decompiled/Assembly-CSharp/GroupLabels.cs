using System;
using System.Runtime.CompilerServices;
using ChartAndGraph;
using UnityEngine;

[Serializable]
public class GroupLabels : ItemLabelsBase
{
	[SerializeField]
	[Tooltip("Detemines the alignment of the group labels")]
	private GroupLabelAlignment alignment;

	public GroupLabelAlignment Alignment
	{
		get
		{
			return alignment;
		}
		set
		{
			alignment = value;
			RaiseOnUpdate();
		}
	}

	public override Action<IInternalUse, bool> Assign => delegate(IInternalUse x, bool clear)
	{
		if (clear)
		{
			if (x.GroupLabels == this)
			{
				x.GroupLabels = null;
			}
		}
		else if (x.GroupLabels != this)
		{
			x.GroupLabels = this;
		}
	};

	[CompilerGenerated]
	public void method_4(IInternalUse x, bool clear)
	{
		if (clear)
		{
			if (x.GroupLabels == this)
			{
				x.GroupLabels = null;
			}
		}
		else if (x.GroupLabels != this)
		{
			x.GroupLabels = this;
		}
	}
}
