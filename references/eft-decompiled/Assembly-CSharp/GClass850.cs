using System;
using UnityEngine;

public abstract class GClass850
{
	[NonSerialized]
	public static Quaternion Quaternion_0 = Quaternion.Euler(0f, 90f, 0f);

	public static void DrawCube(Vector3 position, Quaternion rotation, Vector3 scale)
	{
		Matrix4x4 matrix = Gizmos.matrix;
		Gizmos.matrix *= Matrix4x4.TRS(position, rotation, scale);
		Gizmos.DrawCube(Vector3.zero, Vector3.one);
		Gizmos.matrix = matrix;
	}

	public static void DrawWireCube(Vector3 position, Quaternion rotation, Vector3 scale)
	{
		Matrix4x4 matrix = Gizmos.matrix;
		Gizmos.matrix *= Matrix4x4.TRS(position, rotation, scale);
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		Gizmos.matrix = matrix;
	}

	public static void DrawArrow(Vector3 origin, Vector3 direction)
	{
		Vector3 vector = origin + direction;
		Vector3 vector2 = Quaternion_0 * vector * 0.1f;
		vector2 = Vector3.ClampMagnitude(vector2, 0.03f);
		Gizmos.DrawLine(origin, vector);
		Gizmos.DrawLine(vector, origin + vector2 + direction * 0.7f);
		Gizmos.DrawLine(vector, origin - vector2 + direction * 0.7f);
	}
}
