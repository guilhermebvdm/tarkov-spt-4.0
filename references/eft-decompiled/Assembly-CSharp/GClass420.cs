using System;
using System.Collections.Generic;
using UnityEngine;

public class GClass420
{
	[NonSerialized]
	public static int Int_0;

	[NonSerialized]
	public List<NavGraphEditorPoint> List_0 = new List<NavGraphEditorPoint>();

	[NonSerialized]
	public NavGraphEditorPoint[] NavGraphEditorPoint_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public GClass422<NavGraphEditorPoint> Gclass422_0 = new GClass422<NavGraphEditorPoint>();

	[NonSerialized]
	public int Int_1 = 1;

	[NonSerialized]
	public NavGraphEditorPoint NavGraphEditorPoint_1;

	[NonSerialized]
	public NavGraphEditorPoint NavGraphEditorPoint_2;

	public static void DrawWayGizmos(IPositionPoint[] way, int restrict = 900)
	{
		if (way != null)
		{
			IPositionPoint obj = way[^1];
			IPositionPoint positionPoint = way[0];
			Vector3 vector = Vector3.up * 0.8f;
			Gizmos.color = Color.cyan;
			Vector3 center = obj.Position + vector;
			Gizmos.DrawSphere(center, 0.2f);
			Gizmos.DrawSphere(center, 0.1f);
			Gizmos.color = Color.yellow;
			Vector3 center2 = positionPoint.Position + vector;
			Gizmos.DrawSphere(center2, 0.2f);
			Gizmos.DrawSphere(center2, 0.1f);
			for (int i = 0; i < way.Length - 1 && i <= restrict; i++)
			{
				IPositionPoint obj2 = way[i];
				IPositionPoint positionPoint2 = way[i + 1];
				Gizmos.color = Color.cyan;
				Gizmos.DrawLine(obj2.Position + vector, positionPoint2.Position + vector);
			}
		}
	}

	public static int smethod_0()
	{
		Int_0++;
		return Int_0;
	}

	public bool GetWayPoints(NavGraphEditorPoint beginPoint, NavGraphEditorPoint endPoint, out NavGraphEditorPoint[] wayOut, bool disableLogs = false)
	{
		Int_0 = smethod_0();
		bool flag = GClass398.Instance.IsTraceEnable();
		if (disableLogs)
		{
			flag = false;
		}
		NavGraphEditorPoint_1 = beginPoint;
		NavGraphEditorPoint_2 = endPoint;
		Int_1 = Int_0;
		GClass416 gClass = new GClass416(Int_1);
		Gclass422_0.Enqueue(0f, beginPoint);
		int num = 0;
		while (Gclass422_0.Count > 0)
		{
			num++;
			NavGraphEditorPoint navGraphEditorPoint = Gclass422_0.Dequeue();
			if (flag)
			{
				gClass.AddStep(num, navGraphEditorPoint);
			}
			method_6(navGraphEditorPoint, Int_1);
			float num2 = method_4(navGraphEditorPoint);
			for (int i = 0; i < navGraphEditorPoint.ClosestPointsWaysArray.Count; i++)
			{
				if (flag)
				{
					gClass.TestNeighbour(i);
				}
				NavGraphEditorPointWay navGraphEditorPointWay = navGraphEditorPoint.ClosestPointsWaysArray[i];
				NavGraphEditorPoint targetPoint = navGraphEditorPointWay.TargetPoint;
				bool flag2 = method_5(targetPoint, Int_1);
				if (flag)
				{
					gClass.TestNeighbourVisited(targetPoint, flag2);
				}
				if (!flag2)
				{
					float num3 = 0f - method_2(navGraphEditorPointWay, endPoint.Position);
					Gclass422_0.Enqueue(num3, targetPoint);
					method_6(targetPoint, Int_1);
					targetPoint.nodeFrom = navGraphEditorPoint;
					method_3(targetPoint, num2 + navGraphEditorPointWay.Value);
					if (flag)
					{
						gClass.TestNeighbourHeuristic(targetPoint, num3);
					}
				}
				if (targetPoint.Id == endPoint.Id)
				{
					if (flag)
					{
						gClass.Finish();
					}
					NavGraphEditorPoint[] array = method_0(targetPoint, beginPoint);
					wayOut = array;
					return true;
				}
			}
		}
		wayOut = null;
		return false;
	}

	public void OnDrawGizmosSelected()
	{
		IPositionPoint[] navGraphEditorPoint_ = NavGraphEditorPoint_0;
		DrawWayGizmos(navGraphEditorPoint_);
	}

	public float CalcDist()
	{
		return Float_0;
	}

	public NavGraphEditorPoint[] method_0(NavGraphEditorPoint lastNode, NavGraphEditorPoint firstNode)
	{
		try
		{
			List_0.Clear();
			NavGraphEditorPoint navGraphEditorPoint = lastNode;
			float num = method_4(navGraphEditorPoint);
			while (!(num <= 0f) && navGraphEditorPoint != firstNode)
			{
				List_0.Add(navGraphEditorPoint);
				NavGraphEditorPoint nodeFrom = navGraphEditorPoint.nodeFrom;
				float num2 = method_4(nodeFrom);
				navGraphEditorPoint = nodeFrom;
				num = num2;
				Float_0 += num;
			}
			List_0.Add(navGraphEditorPoint);
			List_0.Reverse();
			NavGraphEditorPoint_0 = List_0.ToArray();
			return NavGraphEditorPoint_0;
		}
		catch (Exception arg)
		{
			Debug.LogError($"can't RestorePath lastNode:{lastNode.Id}  firstNode:{firstNode.Id}   error:{arg}");
		}
		return null;
	}

	public float method_1(NavGraphEditorPointWay node, Vector3 targetPosition)
	{
		return Vector3.Distance(node.TargetPoint.Position, targetPosition);
	}

	public float method_2(NavGraphEditorPointWay node, Vector3 targetPosition)
	{
		float num = Vector3.Distance(node.TargetPoint.Position, targetPosition);
		float value = node.Value;
		return num + value;
	}

	public void method_3(NavGraphEditorPoint neighbour, float length)
	{
		neighbour.pathLength = length;
	}

	public float method_4(NavGraphEditorPoint currentNode)
	{
		return currentNode.pathLength;
	}

	public bool method_5(NavGraphEditorPoint currentNode, int searchId)
	{
		return currentNode.SearchWayId == searchId;
	}

	public void method_6(NavGraphEditorPoint currentNode, int searchId)
	{
		currentNode.SearchWayId = searchId;
	}
}
