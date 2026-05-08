using System;
using System.Collections.Generic;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

[Serializable]
public class AutocullLODGroupCell : IAutocullAutomated
{
	[SerializeField]
	public List<LODGroup> _lodGroups;

	[SerializeField]
	public Bounds _bounds;

	[NonSerialized]
	public bool IsAutocullVisible_1;

	public bool IsAutocullVisible
	{
		get
		{
			return IsAutocullVisible_1;
		}
		set
		{
			IsAutocullVisible_1 = value;
			foreach (LODGroup lodGroup in _lodGroups)
			{
				lodGroup.enabled = value;
			}
		}
	}

	public Bounds AutocullObjectBounds => _bounds;

	public bool IsDynamicCullingObject => false;

	public AutocullLODGroupCell(List<LODGroup> lodGroups)
	{
		if (lodGroups == null)
		{
			throw new NullReferenceException("lodGroups");
		}
		_lodGroups = lodGroups;
		method_0();
	}

	public void method_0()
	{
		if (_lodGroups.Count > 0)
		{
			Bounds lODGroupBounds = GClass1240.GetLODGroupBounds(_lodGroups[0]);
			foreach (LODGroup lodGroup in _lodGroups)
			{
				lODGroupBounds.Encapsulate(GClass1240.GetLODGroupBounds(lodGroup));
			}
			_bounds = lODGroupBounds;
		}
		else
		{
			_bounds = default(Bounds);
		}
	}

	public void Union(AutocullLODGroupCell other)
	{
		_lodGroups.AddRange(other._lodGroups);
		method_0();
	}
}
