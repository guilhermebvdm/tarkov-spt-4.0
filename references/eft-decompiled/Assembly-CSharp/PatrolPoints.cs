using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PatrolPoints : MonoBehaviour
{
	public bool ShowGizmo;

	private readonly List<Transform> list_0 = new List<Transform>();

	public void OnDrawGizmos()
	{
		if (!ShowGizmo)
		{
			return;
		}
		if (base.transform.childCount != list_0.Count)
		{
			list_0.Clear();
			list_0.AddRange(from x in base.transform.GetComponentsInChildren<Transform>()
				where x.transform != base.transform
				select x);
			return;
		}
		Gizmos.color = Color.cyan;
		for (int num = 0; num < list_0.Count; num++)
		{
			Gizmos.DrawWireCube(list_0[num].position, Vector3.one * 0.2f);
		}
	}

	[CompilerGenerated]
	public bool method_0(Transform x)
	{
		return x.transform != base.transform;
	}
}
