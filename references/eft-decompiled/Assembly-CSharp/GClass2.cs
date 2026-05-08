using System;
using System.Collections.Generic;
using UnityEngine;

public class GClass2
{
	[NonSerialized]
	public List<GClass0> List_0;

	[NonSerialized]
	public GClass0 Gclass0_0;

	public GClass2()
	{
		List_0 = new List<GClass0>();
	}

	public GClass0 AddPoint(Vector3 position, Vector3 upVector, Vector3 dirVector)
	{
		GClass0 gClass = new GClass0(position, upVector, dirVector);
		List_0.Add(gClass);
		Gclass0_0 = gClass;
		return Gclass0_0;
	}

	public List<GClass0> GetPoints()
	{
		return List_0;
	}

	public GClass0 GetCurPoint()
	{
		return Gclass0_0;
	}
}
