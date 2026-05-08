using UnityEngine;

public class DebugCoverResultDrawler : MonoBehaviour
{
	public NavGraphFindDebug Debug;

	public NavGraphContainer Container;

	public bool DrawWay;

	public int stepWayConstrain;

	public bool DrawCover;

	public int stepConstrain;

	public float radiusConstrain;

	public void DrawGizmos(int stepConstrain, float radiusConstrain, NavGraphContainer container, int coverSelectId)
	{
		foreach (NavGraphFindDebugStepInfo item in Debug.StepsInfo)
		{
			if (item._step <= stepConstrain && item.Rad <= radiusConstrain)
			{
				item.DrawGizmos(container, coverSelectId);
				continue;
			}
			break;
		}
	}

	public void OnDrawGizmos()
	{
		if (DrawWay && Debug.ResultWay != null)
		{
			IPositionPoint[] resultWay = Debug.ResultWay;
			GClass420.DrawWayGizmos(resultWay, stepWayConstrain);
		}
		if (DrawCover && Container != null && Container.IsInited)
		{
			DrawGizmos(stepConstrain, radiusConstrain, Container, -1);
		}
	}
}
