using System;
using UnityEngine;

public class GroupPointSearchData : IComparable<GroupPointSearchData>
{
	public GroupPoint Point;

	public float Power;

	public ECoverCheckCause Cause;

	[field: NonSerialized]
	public bool CanIShootToEnemy { get; set; }

	public bool IsSpotted => Point.IsSpotted;

	public Vector3 FirePosition => Point.FirePosition;

	public bool AlwaysGood => Point.AlwaysGood;

	public GroupPointSearchData(GroupPoint p, float power)
	{
		Point = p;
		Power = power;
	}

	public int CompareTo(GroupPointSearchData other)
	{
		if (Power > other.Power)
		{
			return -1;
		}
		if (Power < other.Power)
		{
			return 1;
		}
		return 0;
	}
}
