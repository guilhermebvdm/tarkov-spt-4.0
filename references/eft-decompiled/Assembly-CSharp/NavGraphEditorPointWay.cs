using System;

[Serializable]
public class NavGraphEditorPointWay : IComparable<NavGraphEditorPointWay>
{
	public int TargetPointId;

	public int StartPointId;

	public int PathId;

	public int Id;

	public int ReverseId;

	[NonSerialized]
	public NavGraphEditorPoint StartPoint;

	[NonSerialized]
	public NavGraphEditorPoint TargetPoint;

	[NonSerialized]
	public NavGraphEditorPath Path;

	[NonSerialized]
	public NavGraphEditorPointWay Reverse;

	public float Value => Path.Value;

	public NavGraphEditorPointWay(NavGraphEditorPoint targetPoint, NavGraphEditorPoint startPoint, NavGraphEditorPath path, int id)
	{
		Id = id;
		StartPoint = startPoint;
		StartPointId = StartPoint.Id;
		TargetPointId = targetPoint.Id;
		PathId = path.Id;
	}

	public int CompareTo(NavGraphEditorPointWay other)
	{
		if (other.Path.Dist == Path.Dist)
		{
			return 0;
		}
		if (other.Path.Dist < Path.Dist)
		{
			return 1;
		}
		return -1;
	}
}
