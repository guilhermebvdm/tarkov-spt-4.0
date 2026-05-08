using System;
using System.Collections.Generic;
using UnityEngine;

public class GizmoDrawer : MonoBehaviour
{
	private static Action action_0;

	private static List<Bounds> list_0;

	public static void AddGizmo(Action drawGizmo)
	{
		action_0 = (Action)Delegate.Combine(action_0, drawGizmo);
	}

	public static void RemoveGizmos()
	{
		action_0 = null;
	}

	public void OnDrawGizmos()
	{
		if (action_0 != null)
		{
			action_0();
		}
		if (list_0 == null)
		{
			return;
		}
		foreach (Bounds item in list_0)
		{
			Gizmos.DrawWireCube(item.center, item.size);
		}
	}

	public static void DrawBoxes(List<Bounds> boxes)
	{
		list_0 = boxes;
	}

	public static void DrawWireCapsule(Vector3 pos, Vector3 pos2, float radius, Color color = default(Color))
	{
	}

	public static void smethod_0(float arg1, float arg2, float forward)
	{
	}
}
