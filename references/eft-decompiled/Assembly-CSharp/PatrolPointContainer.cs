using UnityEngine;

public class PatrolPointContainer
{
	public PatrolPoint TargetPoint;

	public AReserveWayAction ActionData => TargetPoint.ActionData;

	public Vector3 Position => TargetPoint.Position;

	public int Id => TargetPoint.Id;

	public PatrolPoint GetSubPoint(int index)
	{
		return TargetPoint.GetSubPoint(index);
	}

	public PatrolPointContainer(PatrolPoint TargetPoint)
	{
		this.TargetPoint = TargetPoint;
	}
}
