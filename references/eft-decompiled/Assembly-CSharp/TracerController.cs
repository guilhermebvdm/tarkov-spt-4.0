using UnityEngine;

public class TracerController : MonoBehaviour
{
	private TracerSystem tracerSystem_0;

	public void Start()
	{
		tracerSystem_0 = GClass870.FindUnityObjectOfType<TracerSystem>();
	}

	public void DrawTracers(EftBulletClass shotResults)
	{
		if (shotResults.PositionHistory.Count != 0)
		{
			for (int i = 1; i < shotResults.PositionHistory.Count; i++)
			{
				Vector3 start = shotResults.PositionHistory[i - 1];
				Vector3 end = shotResults.PositionHistory[i];
				tracerSystem_0.Add(start, end, Color.red, 0.01f, 0.01f);
			}
		}
	}
}
