using System;
using UnityEngine;

public class AIDangerArea
{
	public Vector3 Point;

	public float Rad;

	public float Period;

	public float EndTime;

	[field: NonSerialized]
	public bool IsAssaultIsReady { get; set; }

	[field: NonSerialized]
	public float SRad { get; }

	[field: NonSerialized]
	public int Id { get; }

	[field: NonSerialized]
	public bool IsActive { get; }

	public AIDangerArea(Vector3 point, float rad, float period)
	{
		IsActive = false;
		Period = period;
		if (Period > 0f)
		{
			EndTime = Period + Time.time;
		}
		else
		{
			EndTime = -1f;
		}
		Point = point;
		Rad = rad;
		SRad = rad * rad;
		Id = Guid.NewGuid().GetHashCode();
	}

	public bool IsPointInside(Vector3 point)
	{
		return (point - Point).sqrMagnitude < SRad;
	}

	public void Complete()
	{
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(Point, 0.4f);
		Gizmos.DrawWireSphere(Point, Rad);
	}
}
