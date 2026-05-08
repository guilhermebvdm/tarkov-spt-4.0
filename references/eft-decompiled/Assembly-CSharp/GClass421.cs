using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GClass421
{
	[NonSerialized]
	public const float Float_0 = 1f;

	[NonSerialized]
	public NavGraphEditorPoint NavGraphEditorPoint_0;

	[NonSerialized]
	public NavGraphEditorPoint NavGraphEditorPoint_1;

	[NonSerialized]
	public HashSet<int> HashSet_0 = new HashSet<int>();

	[NonSerialized]
	public SortedSet<FloatContainer> SortedSet_0 = new SortedSet<FloatContainer>();

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public NavGraphContainer NavGraphContainer_0;

	[NonSerialized]
	public List<FloatContainer> List_0;

	[NonSerialized]
	public bool Bool_0;

	public GClass421(NavGraphEditorPoint from, NavGraphEditorPoint to, NavGraphContainer container, bool calcNow)
	{
		if (!container.IsInited)
		{
			container.RefreshData();
		}
		NavGraphContainer_0 = container;
		NavGraphEditorPoint_0 = from;
		NavGraphEditorPoint_1 = to;
		if (from.ClosestPointsWaysArray == null)
		{
			Debug.LogError("wrong data ways   from.ClosestPointsHash == null");
		}
		else if (calcNow)
		{
			FloatContainer floatContainer = new FloatContainer(0f, null, NavGraphEditorPoint_0);
			method_1(floatContainer);
		}
	}

	public float CalcDist()
	{
		if (!Bool_0)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < List_0.Count - 1; i++)
		{
			FloatContainer floatContainer = List_0[i];
			float num2 = Vector3.Distance(b: List_0[i + 1].Position, a: floatContainer.Position);
			num += num2;
		}
		return num;
	}

	public bool Calc(out List<NavGraphEditorPoint> resWay)
	{
		FloatContainer floatContainer = new FloatContainer(0f, null, NavGraphEditorPoint_0);
		method_1(floatContainer);
		resWay = new List<NavGraphEditorPoint>();
		if (Bool_0)
		{
			foreach (FloatContainer item in List_0)
			{
				resWay.Add(item.Owner);
			}
		}
		return Bool_0;
	}

	public void OnDrawGizmosSelected()
	{
		if (List_0 != null)
		{
			Gizmos.color = Color.green;
			Vector3 center = NavGraphEditorPoint_1.Position + Vector3.up;
			Gizmos.DrawSphere(center, 0.2f);
			Gizmos.DrawSphere(center, 0.1f);
			Gizmos.color = Color.yellow;
			Vector3 center2 = NavGraphEditorPoint_0.Position + Vector3.up;
			Gizmos.DrawSphere(center2, 0.2f);
			Gizmos.DrawSphere(center2, 0.1f);
			for (int i = 0; i < List_0.Count - 1; i++)
			{
				FloatContainer floatContainer = List_0[i];
				FloatContainer floatContainer2 = List_0[i + 1];
				Gizmos.color = Color.red;
				Gizmos.DrawLine(floatContainer.Owner.Position + Vector3.up, floatContainer2.Owner.Position + Vector3.up);
			}
		}
	}

	public int CountCorners()
	{
		if (List_0 == null)
		{
			return 0;
		}
		return List_0.Count;
	}

	public void method_0(bool arg1, FloatContainer lastPoint)
	{
		Bool_0 = arg1;
		if (!arg1)
		{
			return;
		}
		bool flag = false;
		int num = 0;
		FloatContainer floatContainer = lastPoint;
		List_0 = new List<FloatContainer>();
		List_0.Add(floatContainer);
		while (!flag && num < 1000)
		{
			if (floatContainer.PrevPoint == null)
			{
				flag = true;
				continue;
			}
			floatContainer = floatContainer.PrevPoint;
			List_0.Add(floatContainer);
			num++;
		}
		for (int i = 0; i < List_0.Count - 1; i++)
		{
			_ = List_0[i];
			_ = List_0[i + 1];
		}
	}

	public void method_1(FloatContainer from)
	{
		if (Int_0 > 300)
		{
			method_0(arg1: false, from);
			return;
		}
		Int_0++;
		if (!SortedSet_0.Remove(from) && Int_0 > 2)
		{
			FloatContainer floatContainer = SortedSet_0.First();
			Debug.LogError($"EXTRA EXIT   first:{floatContainer.GetHashCode()}  from:{from.GetHashCode()}");
			method_0(arg1: false, from);
		}
		else if (!method_2(from))
		{
			if (SortedSet_0.Count == 0)
			{
				method_0(arg1: false, from);
				return;
			}
			FloatContainer floatContainer2 = SortedSet_0.First();
			method_1(floatContainer2);
		}
	}

	public bool method_2(FloatContainer from)
	{
		foreach (NavGraphEditorPointWay item in from.Owner.ClosestPointsWaysArray)
		{
			NavGraphEditorPoint targetPoint = item.TargetPoint;
			NavGraphEditorPath path = item.Path;
			if (targetPoint != NavGraphEditorPoint_1)
			{
				float sqrMagnitude = (targetPoint.Position - NavGraphEditorPoint_1.Position).sqrMagnitude;
				if (!HashSet_0.Contains(targetPoint.Id))
				{
					float val = sqrMagnitude + path.Dist * 1f;
					targetPoint.FloatContainer = new FloatContainer(val, from, targetPoint);
					SortedSet_0.Add(targetPoint.FloatContainer);
					HashSet_0.Add(targetPoint.Id);
				}
				continue;
			}
			method_0(arg1: true, new FloatContainer(path.Dist, from, targetPoint));
			return true;
		}
		return false;
	}
}
