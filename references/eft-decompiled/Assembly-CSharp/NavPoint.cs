using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NavPoint
{
	public int Id;

	public int AreaId;

	public bool IsValid = true;

	public Vector3 Pos;

	[NonSerialized]
	public HashSet<NavEdge> Edges = new HashSet<NavEdge>();

	public NavPoint(int id, Vector3 pos)
	{
		Id = id;
		Pos = pos;
	}

	public void AddEdge(NavEdge navEdge)
	{
		Edges.Add(navEdge);
	}
}
