using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class TestSetNavMeshCutter : MonoBehaviour
{
	public Transform From;

	public Transform To;

	public Transform CutterPlace;

	public NavMeshObstacle LastObstacle;

	public void DoTest()
	{
		DoTestAsync().HandleExceptions();
	}

	public async Task DoTestAsync()
	{
		NavMeshPath navMeshPath = new NavMeshPath();
		LastObstacle.gameObject.SetActive(value: true);
		LastObstacle.transform.position = CutterPlace.position;
		await Task.Yield();
		await Task.Yield();
		await Task.Yield();
		NavMesh.CalculatePath(From.position, To.position, -1, navMeshPath);
		if (navMeshPath.status == NavMeshPathStatus.PathComplete)
		{
			GClass369.DebugDrawWay(navMeshPath.corners, Vector3.zero, Color.blue, 20f);
		}
		LastObstacle.gameObject.SetActive(value: false);
		Debug.LogError($"calc result:{navMeshPath.status}");
	}
}
