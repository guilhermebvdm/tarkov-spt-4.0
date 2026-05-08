using UnityEngine;
using UnityEngine.AI;

public abstract class GClass371
{
	public static void DrawPathGizmo(this NavMeshPath path, Color color)
	{
		Gizmos.color = color;
		Vector3[] corners = path.corners;
		for (int i = 0; i < corners.Length - 1; i++)
		{
			Gizmos.DrawLine(corners[i], corners[i + 1]);
		}
	}

	public static void DrawPathDebug(Vector3[] path, Color color, float time, float offsetUp = 0.1f)
	{
		Vector3 vector = Vector3.up * offsetUp;
		for (int i = 0; i < path.Length - 1; i++)
		{
			Debug.DrawLine(path[i] + vector, path[i + 1] + vector, color, time);
		}
	}

	public static float CalculatePathLength(this NavMeshPath path)
	{
		return CalculatePathLength(path.corners);
	}

	public static float CalculatePathLength(this Vector3[] corners)
	{
		if (corners == null)
		{
			return float.MaxValue;
		}
		if (corners.Length < 2)
		{
			return 0f;
		}
		Vector3 vector = corners[0];
		float num = 0f;
		for (int i = 1; i < corners.Length; i++)
		{
			Vector3 vector2 = corners[i];
			Vector3 vector3 = vector - vector2;
			num += Mathf.Sqrt(vector3.x * vector3.x + vector3.y * vector3.y + vector3.z * vector3.z);
			vector = vector2;
		}
		return num;
	}
}
