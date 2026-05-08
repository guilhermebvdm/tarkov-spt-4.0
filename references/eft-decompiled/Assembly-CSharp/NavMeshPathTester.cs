using UnityEngine;
using UnityEngine.AI;

public class NavMeshPathTester : MonoBehaviour
{
	[SerializeField]
	private Transform pointA;

	[SerializeField]
	private Transform pointB;

	public void BuildPath()
	{
		NavMeshPath navMeshPath = new NavMeshPath();
		NavMesh.CalculatePath(pointA.position, pointB.position, -1, navMeshPath);
		BotZoneDebug botZoneDebug = Object.FindObjectOfType<BotZoneDebug>();
		if (botZoneDebug != null)
		{
			botZoneDebug.Clear();
			for (int i = 0; i < navMeshPath.corners.Length - 1; i++)
			{
				botZoneDebug.AddLine("Path", navMeshPath.corners[i], navMeshPath.corners[i + 1]);
				botZoneDebug.Add("Path", navMeshPath.corners[i]);
			}
			botZoneDebug.Add("Path", navMeshPath.corners[navMeshPath.corners.Length - 1]);
			botZoneDebug.Add("Points", pointA.position);
			botZoneDebug.Add("Points", pointB.position);
		}
		Debug.LogError("Path Status: " + navMeshPath.status);
	}
}
