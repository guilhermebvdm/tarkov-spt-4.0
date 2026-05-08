using System;

[Serializable]
public class GroupPointPath : IComparable<GroupPointPath>
{
	public float Dist;

	public int IdPath;

	public int IdB;

	public int IdA;

	public GroupPointWay WayA;

	public GroupPointWay WayB;

	public GroupPointPath(float dist, int idPath, GroupPointWay wayA, GroupPointWay wayB)
	{
		IdPath = idPath;
		Dist = dist;
		IdB = wayA.Id;
		IdA = wayB.Id;
		WayB = wayA;
		WayA = wayB;
	}

	public int CompareTo(GroupPointPath other)
	{
		if (other.Dist > Dist)
		{
			return 1;
		}
		if (other.Dist < Dist)
		{
			return -1;
		}
		return 0;
	}
}
