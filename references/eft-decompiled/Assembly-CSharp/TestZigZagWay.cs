using UnityEngine;
using UnityEngine.AI;

public class TestZigZagWay : MonoBehaviour
{
	public Transform P1;

	public Transform P2;

	public float offsetLength = 6f;

	public bool TestSlowBuyPercent = true;

	public float PercentOffset = 0.05f;

	public float ChanceAtStep = 100f;

	public bool TestAllSteps = true;

	public bool DrawBaseWay = true;

	public void BuildPath()
	{
		NavMeshPath navMeshPath = new NavMeshPath();
		if (NavMesh.CalculatePath(P1.position, P2.position, -1, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
		{
			if (DrawBaseWay)
			{
				GClass371.DrawPathDebug(navMeshPath.corners, Color.yellow, 5f, 0.5f);
			}
			if (TestSlowBuyPercent)
			{
				GClass635.ModifyZigZagPercentOffset(navMeshPath.corners, offsetLength, ChanceAtStep, 3f, PercentOffset, out var modifedWay);
				GClass371.DrawPathDebug(modifedWay.ToArray(), Color.red, 5f, 1f);
			}
			if (TestAllSteps)
			{
				GClass635.ModifyZigZagAllSteps(navMeshPath.corners, offsetLength, 2.2f, out var modifedWay2);
				GClass371.DrawPathDebug(modifedWay2.ToArray(), Color.magenta, 5f, 1.2f);
			}
		}
	}
}
