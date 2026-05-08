using System;
using System.Collections.Generic;
using UnityEngine;

public class AITriangleData : MonoBehaviour
{
	public List<NavPoint> Points;

	public List<NavTriangle> Triangles;

	public List<NavEdge> Edge;

	public TriangleCounters Counters;

	[NonSerialized]
	public bool Restored;

	[NonSerialized]
	public Dictionary<NavEdge, List<NavPoint>> ToCut = new Dictionary<NavEdge, List<NavPoint>>();

	public bool HaveExternalEdges;

	public bool DrawPoints;

	public bool DrawPointsWithEdges;

	public bool DrawEdges;

	public bool DrawTriags;

	public bool DrawCounters;

	public bool DrawWithIndex;

	public int DistDraw = 20;

	public int CountExternalEdges;

	[SerializeField]
	private int _idIndex;

	public int wrongAddsToCutList;

	public int goodAddsToCutList;

	private Dictionary<int, NavPoint> dictionary_0 = new Dictionary<int, NavPoint>();

	private Dictionary<int, NavTriangle> dictionary_1 = new Dictionary<int, NavTriangle>();

	private Dictionary<int, NavEdge> dictionary_2 = new Dictionary<int, NavEdge>();

	private readonly Vector3 vector3_0 = Vector3.up * 0.03f;

	private readonly Vector3 vector3_1 = Vector3.up * 0.2f;

	private readonly Vector3 vector3_2 = Vector3.up * 0.1f;

	private readonly Vector3 vector3_3 = Vector3.up * 0.3f;

	public float SizeToCutOff = 2f;

	public List<GameObject> ObjectsToDelete;

	public int LastUsedId => _idIndex;

	public int GetId()
	{
		_idIndex++;
		return _idIndex;
	}

	public static AITriangleData CreateOrFind(bool withClear)
	{
		AITriangleData aITriangleData = UnityEngine.Object.FindObjectOfType<AITriangleData>();
		if (aITriangleData != null)
		{
			if (withClear)
			{
				aITriangleData.Clear();
			}
		}
		else
		{
			aITriangleData = new GameObject("AITriangleData").AddComponent<AITriangleData>();
			BotZone botZone = UnityEngine.Object.FindObjectOfType<BotZone>();
			aITriangleData.transform.SetParent(botZone.gameObject.transform);
			aITriangleData.transform.SetParent(null);
		}
		aITriangleData.transform.SetAsFirstSibling();
		return aITriangleData;
	}

	public void OnDrawGizmosSelected()
	{
		if (!Restored)
		{
			return;
		}
		int num = DistDraw * DistDraw;
		Vector3 position = Camera.current.transform.position;
		if (DrawPoints)
		{
			int num2 = 0;
			float radius = 0.1f;
			foreach (NavPoint point in Points)
			{
				float sqrMagnitude = (point.Pos - position).sqrMagnitude;
				num2++;
				if (!(sqrMagnitude < (float)num))
				{
					continue;
				}
				Vector3 vector = (DrawWithIndex ? (point.Pos + num2 * vector3_0) : (point.Pos + vector3_0));
				bool flag = false;
				bool flag2 = false;
				if (DrawPointsWithEdges)
				{
					radius = 0.01f;
					flag = point.Edges.Count > 0;
					int num3 = 0;
					foreach (NavEdge edge in point.Edges)
					{
						Vector3 v = edge.GetOther(point).Pos - point.Pos;
						float magnitude = v.magnitude;
						if (!(flag2 = magnitude <= 0f))
						{
							v = GClass855.NormalizeFastSelf(v);
							Gizmos.DrawRay(vector + Vector3.up * num3 * 0.004f, v * 0.4f * magnitude);
							num3++;
						}
					}
				}
				if (flag2)
				{
					Gizmos.color = Color.red;
				}
				else
				{
					Gizmos.color = (flag ? Color.cyan : Color.blue);
				}
				Gizmos.DrawSphere(vector, radius);
			}
		}
		if (DrawEdges)
		{
			int num4 = 0;
			foreach (NavEdge item in Edge)
			{
				float sqrMagnitude2 = ((item.Point1.Pos + item.Point2.Pos) * 0.5f - position).sqrMagnitude;
				num4++;
				if (sqrMagnitude2 < (float)num)
				{
					Gizmos.color = ((item.Pair != null) ? Color.green : Color.yellow);
					if (DrawWithIndex)
					{
						Gizmos.DrawLine(item.Point1.Pos + vector3_1 * num4, item.Point2.Pos + vector3_1 * num4);
					}
					else
					{
						Gizmos.DrawLine(item.Point1.Pos + vector3_1, item.Point2.Pos + vector3_1);
					}
				}
			}
		}
		if (DrawTriags)
		{
			int num5 = 0;
			foreach (NavTriangle triangle in Triangles)
			{
				if (!((triangle.Position - position).sqrMagnitude < (float)num))
				{
					continue;
				}
				num5++;
				foreach (NavEdge navEgde in triangle.NavEgdes)
				{
					Gizmos.color = ((navEgde.Pair != null) ? Color.green : Color.yellow);
					if (DrawWithIndex)
					{
						Gizmos.DrawLine(navEgde.Point1.Pos + vector3_2 * num5, navEgde.Point2.Pos + vector3_2 * num5);
					}
					else
					{
						Gizmos.DrawLine(navEgde.Point1.Pos + vector3_2, navEgde.Point2.Pos + vector3_2);
					}
				}
			}
		}
		if (!DrawCounters || Counters == null || Counters.Counters.Count <= 0)
		{
			return;
		}
		Gizmos.color = Color.red;
		foreach (TriangleCounter counter in Counters.Counters)
		{
			NavPoint navPoint = dictionary_0[counter.Points[0]];
			if (!((navPoint.Pos - position).sqrMagnitude < (float)num))
			{
				continue;
			}
			int num6 = 0;
			NavPoint navPoint2 = dictionary_0[counter.Points[counter.Points.Count - 1]];
			Gizmos.DrawLine(navPoint.Pos + vector3_3, navPoint2.Pos + vector3_3);
			for (int i = 0; i < counter.Points.Count - 1; i++)
			{
				num6++;
				navPoint = dictionary_0[counter.Points[i]];
				navPoint2 = dictionary_0[counter.Points[i + 1]];
				if (DrawWithIndex)
				{
					Vector3 vector2 = Vector3.up * 0.1f;
					Gizmos.DrawLine(navPoint.Pos + vector3_3 + vector2 * (num6 - 1), navPoint2.Pos + vector3_3 + vector2 * num6);
				}
				else
				{
					Gizmos.DrawLine(navPoint.Pos + vector3_3, navPoint2.Pos + vector3_3);
				}
			}
		}
	}

	public void Clear()
	{
		Restored = false;
		goodAddsToCutList = 0;
		wrongAddsToCutList = 0;
		_idIndex = 0;
		dictionary_1.Clear();
		dictionary_2.Clear();
		dictionary_0.Clear();
		Triangles.Clear();
		Points.Clear();
		Edge.Clear();
		Counters?.Clear();
	}

	public void SetDictionaries(Dictionary<int, NavPoint> dcPoints, Dictionary<int, NavEdge> dcEdge, Dictionary<int, NavTriangle> dcTriangles)
	{
		dictionary_1 = dcTriangles;
		dictionary_0 = dcPoints;
		dictionary_2 = dcEdge;
		Restored = true;
	}

	public void DeleteTriangle(NavTriangle triagToDel)
	{
		Triangles.Remove(triagToDel);
		dictionary_1.Remove(triagToDel.Id);
		foreach (NavEdge navEgde in triagToDel.NavEgdes)
		{
			dictionary_2.Remove(navEgde.Id);
			Edge.Remove(navEgde);
			if (navEgde.Pair != null)
			{
				navEgde.Pair.Pair = null;
			}
			navEgde.MarkDeleted();
			navEgde.Point1.Edges?.Remove(navEgde);
			navEgde.Point2.Edges?.Remove(navEgde);
		}
	}

	public void AddTriag(NavTriangle trianlge)
	{
		Triangles.Add(trianlge);
		Edge.AddRange(trianlge.NavEgdes);
		foreach (NavEdge navEgde in trianlge.NavEgdes)
		{
			dictionary_2.Add(navEgde.Id, navEgde);
		}
	}

	public NavPoint GetPointNew(Vector3 pos)
	{
		return new NavPoint(GetId(), pos);
	}

	public NavPoint GetPoint(int id)
	{
		if (dictionary_0.TryGetValue(id, out var value))
		{
			return value;
		}
		Debug.LogError($"can't find nav point id:{id}.   _dcPoints:{dictionary_0.Count}");
		return dictionary_0[id];
	}

	public NavEdge GetEdge(int index)
	{
		if (dictionary_2.TryGetValue(index, out var value))
		{
			return value;
		}
		Debug.LogError($"can't find edge with id:{index} _dcEdge:{dictionary_2.Count}");
		return null;
	}

	public bool AddCut(GClass392 point, NavEdge edge)
	{
		NavPoint point2 = point.Point;
		if (point2 == null)
		{
			Debug.LogError("point with null navpoint");
			return false;
		}
		if (!ToCut.TryGetValue(edge, out var value))
		{
			value = new List<NavPoint>();
			ToCut.Add(edge, value);
		}
		bool flag = true;
		for (int i = 0; i < value.Count; i++)
		{
			NavPoint navPoint = value[i];
			if (!((point2.Pos - navPoint.Pos).magnitude > 0f))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			goodAddsToCutList++;
			value.Add(point2);
			return true;
		}
		wrongAddsToCutList++;
		return false;
	}

	public void RemoveEdge(NavEdge navEgde)
	{
		Edge.Remove(navEgde);
		dictionary_2.Remove(navEgde.Id);
		navEgde.MarkDeleted();
		navEgde.Point1.Edges?.Remove(navEgde);
		navEgde.Point2.Edges?.Remove(navEgde);
	}

	public void AddEdgeExternal(NavEdge nEdge)
	{
		CountExternalEdges++;
		Edge.Add(nEdge);
		dictionary_2.Add(nEdge.Id, nEdge);
		HaveExternalEdges = true;
	}

	public string EdgeDebug()
	{
		return $"List.Count:{Edge.Count} dcEdge:{dictionary_2.Count}";
	}

	public int CalcZeroDistEdges()
	{
		int num = 0;
		foreach (NavEdge item in Edge)
		{
			if (item.CalcDist() <= 0f)
			{
				num++;
			}
		}
		return num;
	}

	public void TryAddPoint(NavPoint p)
	{
		if (!dictionary_0.ContainsKey(p.Id))
		{
			dictionary_0.Add(p.Id, p);
			Points.Add(p);
		}
	}
}
