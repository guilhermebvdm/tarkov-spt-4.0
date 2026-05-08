using System;
using UnityEngine;

public class GClass585
{
	public Vector3 Point;

	[NonSerialized]
	public float Float_0;

	public GClass585(Vector3 p, float lifeTime)
	{
		Point = p;
		Float_0 = Time.time + lifeTime;
	}

	public bool IsAlive()
	{
		return Time.time < Float_0;
	}
}
