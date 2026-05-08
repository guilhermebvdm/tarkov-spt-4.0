using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass401
{
	public GClass367 MaxPoint;

	public List<GClass367> AllSavePoints;

	public Vector3 CenterSearch;

	[NonSerialized]
	[CompilerGenerated]
	public HashSet<GClass367> HashSet_0;

	public HashSet<GClass367> _searchBeforeHide
	{
		[CompilerGenerated]
		get
		{
			return HashSet_0;
		}
		[CompilerGenerated]
		set
		{
			HashSet_0 = value;
		}
	}

	public GClass401(GClass367 MaxPoint, List<GClass367> AllSavePoints, Vector3 CenterSearch, HashSet<GClass367> SearchBeforeHide)
	{
		this.MaxPoint = MaxPoint;
		this.AllSavePoints = AllSavePoints;
		this.CenterSearch = CenterSearch;
		_searchBeforeHide = SearchBeforeHide;
	}
}
