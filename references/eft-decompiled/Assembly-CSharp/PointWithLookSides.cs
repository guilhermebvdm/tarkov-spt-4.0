using System.Collections.Generic;
using UnityEngine;

public class PointWithLookSides : MonoBehaviour
{
	public List<Vector3> Directions = new List<Vector3>();

	public void Refresh()
	{
		if (Directions.Count == 0)
		{
			Debug.LogError("Point look have ZERO directions " + base.transform.parent.name);
		}
	}

	public void OnDrawGizmosSelected()
	{
		for (int i = 0; i < Directions.Count; i++)
		{
			Vector3 vector = Directions[i];
			Gizmos.color = new Color(0f, 1f, 0f, 0.9f);
			Vector3 vector2 = base.transform.position + vector * 2f;
			Gizmos.DrawLine(base.transform.position, vector2);
			GClass850.DrawCube(vector2, base.transform.rotation, Vector3.one * 0.2f);
		}
	}
}
