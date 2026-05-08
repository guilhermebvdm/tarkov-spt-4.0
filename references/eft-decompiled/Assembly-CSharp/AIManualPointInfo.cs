using System.Collections.Generic;
using UnityEngine;

public class AIManualPointInfo : MonoBehaviour
{
	public const int MANUAL_ZONE_START_ID = 100000;

	public bool _alwaysDraw;

	public bool DrawWithEdges;

	public bool DrawWithCore;

	public float MinDefenceLevelToDraw;

	public BoxCollider[] ExcludableColliders;

	public List<GroupPoint> Points = new List<GroupPoint>(100);

	public void Init()
	{
		SetCllidersToTrigers();
	}

	public void Add(GroupPoint c)
	{
		c.Id = Points.Count;
		Points.Add(c);
	}

	public void SetCllidersToTrigers()
	{
		BoxCollider[] excludableColliders = ExcludableColliders;
		for (int i = 0; i < excludableColliders.Length; i++)
		{
			excludableColliders[i].enabled = false;
		}
	}

	public void SetAlwaysDraw(bool alwaysDraw)
	{
		_alwaysDraw = alwaysDraw;
	}

	public void RemoveByExcutableColiders<T>(List<T> coreList, BoxCollider[] colliders) where T : IPositionPoint
	{
		List<T> list = new List<T>();
		for (int i = 0; i < coreList.Count; i++)
		{
			T item = coreList[i];
			bool flag = false;
			foreach (BoxCollider box in colliders)
			{
				if (GClass369.PointInOABB(item.Position, box))
				{
					flag = true;
				}
			}
			if (flag)
			{
				list.Remove(item);
				i--;
			}
		}
	}

	public void OnDrawGizmosSelected()
	{
		if (!_alwaysDraw)
		{
			method_0();
		}
	}

	public void OnDrawGizmos()
	{
		if (_alwaysDraw)
		{
			method_0();
		}
	}

	public void method_0()
	{
		foreach (GroupPoint point in Points)
		{
			if (MinDefenceLevelToDraw > 0f)
			{
				Gizmos.color = new Color(0.9f, 0.2f, 0.2f, 0.5f);
			}
			point.DrawGizmos(DrawWithEdges, DrawWithCore);
		}
	}

	public void method_1()
	{
	}

	public void method_2(List<GroupPoint> coreList, List<GroupPoint> mergedList)
	{
		List<GroupPoint> list = new List<GroupPoint>();
		foreach (GroupPoint core in coreList)
		{
			foreach (GroupPoint merged in mergedList)
			{
				if ((core.Position - merged.Position).sqrMagnitude < 0.3f)
				{
					list.Add(merged);
				}
			}
		}
		if (list.Count > 0)
		{
			Debug.LogError("Optimized merged" + list.Count + "  points");
			GroupPoint[] array = list.ToArray();
			foreach (GroupPoint item in array)
			{
				mergedList.Remove(item);
			}
		}
		else
		{
			Debug.Log("All fine. NOthing to optimize");
		}
	}

	public void method_3()
	{
		List<GroupPoint> list = new List<GroupPoint>();
		for (int i = 0; i < Points.Count; i++)
		{
			GroupPoint groupPoint = Points[i];
			for (int j = i + 1; j < Points.Count; j++)
			{
				GroupPoint groupPoint2 = Points[j];
				if ((groupPoint2.Position - groupPoint.Position).sqrMagnitude < 0.3f)
				{
					list.Add(groupPoint2);
				}
			}
		}
		if (list.Count > 0)
		{
			Debug.LogError("Optimized self " + list.Count + "  points");
			GroupPoint[] array = list.ToArray();
			foreach (GroupPoint item in array)
			{
				Points.Remove(item);
			}
		}
		else
		{
			Debug.Log("All fine. NOthing to optimize");
		}
	}
}
