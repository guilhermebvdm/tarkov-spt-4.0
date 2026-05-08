using UnityEngine;
using UnityEngine.AI;

public abstract class GClass373
{
	public static Vector2 ScreenCenter => new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);

	public static void DrawPathDebug(this NavMeshPath path, Color color, float time)
	{
		GClass371.DrawPathDebug(path.corners, color, time);
	}
}
