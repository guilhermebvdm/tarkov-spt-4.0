using System;

[Serializable]
public class GroupPointWay : IComparable<GroupPointWay>
{
	[NonSerialized]
	public GroupPoint Target;

	public float Dist;

	public int IdTarget;

	public int Id;

	public GroupPointWay(int id, GroupPoint target, float dist)
	{
		Id = id;
		Target = target;
		Dist = dist;
		IdTarget = target.Id;
	}

	public void Restore(GroupPoint target)
	{
		Target = target;
	}

	public int CompareTo(GroupPointWay other)
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
