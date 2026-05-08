using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ReachableClass
{
	public HashSet<NavGraphEditorPoint> Points = new HashSet<NavGraphEditorPoint>();

	public int Id;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	public int Count => Points.Count;

	public bool Deprecate
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public ReachableClass(int id, GClass418 baseReachablEditor)
	{
		Id = id;
		baseReachablEditor.AddReachable(this);
		Debug.Log($"Create group :{Id}");
	}

	public void Add(NavGraphEditorPoint p)
	{
		Points.Add(p);
		p.Reachable = this;
		Debug.Log($"Group :{Id}   Add:{p.Id}");
	}

	public void SetDeprecated()
	{
		Deprecate = true;
		Debug.Log($"Set deprecated  {Id}");
	}

	public bool IsReach(NavGraphEditorPoint finalPoint)
	{
		return finalPoint.Reachable.Id == Id;
	}
}
