using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class PatrolPoint : MonoBehaviour, IPositionPoint, IAICorePointLink
{
	[SerializeField]
	private int _corePointId;

	public int Id;

	public bool CanUseByBoss = true;

	public PatrolWay PatrolWay;

	public bool ShallSit;

	public PatrolPointType PatrolPointType;

	public AReserveWayAction ActionData;

	public PointWithLookSides PointWithLookSides;

	public bool SubManual;

	[SerializeField]
	private List<PatrolPoint> subPoints;

	private bool _errorPrinted;

	public int IdPoint => Id;

	public BotOwner Owner { get; set; }

	public Vector3 position => base.transform.position;

	public Vector3 Position => base.transform.position;

	public bool HaveLookSide => LookIndexes > 0;

	public int LookIndexes
	{
		get
		{
			if (PointWithLookSides == null)
			{
				return -1;
			}
			return PointWithLookSides.Directions.Count;
		}
	}

	public AICorePoint CorePointInGame { get; set; }

	public int SubPointsCount => subPoints.Count;

	public void Awake()
	{
		subPoints = method_0();
	}

	public Vector3 PositionForTest()
	{
		if (ActionData != null)
		{
			return ActionData.GoTo;
		}
		return Position;
	}

	public Vector3 LookDir(int index)
	{
		return PointWithLookSides.Directions[index];
	}

	public void SetOwner(BotOwner owner)
	{
		if (owner != null)
		{
			if (subPoints.Count > 0 && owner.BotFollower.HaveBoss)
			{
				return;
			}
			foreach (PatrolPoint point in owner.PatrollingData.Way.Points)
			{
				if (point.Owner == Owner)
				{
					point.SetOwner(null);
				}
			}
		}
		if (ActionData != null && Owner != null && owner == null)
		{
			ActionData.SetLeaveUser(Owner);
		}
		Owner = owner;
		if (ActionData != null)
		{
			ActionData.SetCurrentUser(Owner);
		}
	}

	public int GetOwnerId()
	{
		if (Owner == null)
		{
			return -1;
		}
		return Owner.Id;
	}

	public void CreateSubPoints(PatrolWay way)
	{
		int num = 6;
		method_1();
		List<PatrolPoint> list = method_0();
		if (SubManual)
		{
			PointWithLookSides component = GetComponent<PointWithLookSides>();
			if (component != null)
			{
				PointWithLookSides = component;
				component.Refresh();
			}
			subPoints = new List<PatrolPoint>();
			foreach (Transform item in base.transform)
			{
				if (NavMesh.SamplePosition(item.position + Vector3.up, out var hit, 1.5f, -1))
				{
					item.position = hit.position;
				}
				else
				{
					string text = ((item.parent != null) ? item.parent.name : "No parent");
					Debug.LogError("Can't sample to to navmesh " + item.name + "  with SubManual == true  parent:" + text);
				}
				PatrolPoint component2 = item.GetComponent<PatrolPoint>();
				NavMeshPath navMeshPath = new NavMeshPath();
				if (NavMesh.CalculatePath(Position, item.position, -1, navMeshPath))
				{
					if (navMeshPath.status != NavMeshPathStatus.PathComplete)
					{
						Debug.LogError("Can't cal path:" + item.gameObject.name + " with some of sub points " + base.gameObject.name + ". This points setted manually. SO FIX IT!!!!222");
					}
					else
					{
						float magnitude = (navMeshPath.corners[0] - Position).magnitude;
						float magnitude2 = (navMeshPath.corners[navMeshPath.corners.Length - 1] - item.position).magnitude;
						if (magnitude > 0.7f)
						{
							Debug.LogError($"distToStart ERROR  {magnitude}  {item.gameObject.name}   {base.gameObject.name}");
						}
						if (magnitude2 > 0.7f)
						{
							Debug.LogError($"distToEnd ERROR  {magnitude2} {item.gameObject.name}   {base.gameObject.name}");
						}
					}
				}
				else
				{
					Debug.LogError("Can't cal path with some of sub points " + base.gameObject.name + ". This points setted manually. SO FIX IT!!!!111");
				}
				if (component2 != null)
				{
					subPoints.Add(component2);
					PointWithLookSides componentInChildren = component2.GetComponentInChildren<PointWithLookSides>();
					bool flag = false;
					bool flag2 = false;
					if (componentInChildren != null)
					{
						flag = true;
						component2.PointWithLookSides = componentInChildren;
					}
					PointWithLookSides component3 = component2.GetComponent<PointWithLookSides>();
					if (component3 != null)
					{
						flag2 = true;
						component2.PointWithLookSides = component3;
					}
					if (flag && flag2)
					{
						Debug.LogError("Patrol point " + base.gameObject.name + "  have 2 PointWithLookSides");
					}
					if (component2.PointWithLookSides != null)
					{
						component2.PointWithLookSides.Refresh();
					}
				}
			}
		}
		else
		{
			foreach (PatrolPoint item2 in list)
			{
				UnityEngine.Object.DestroyImmediate(item2.gameObject);
			}
			subPoints = new List<PatrolPoint>();
			method_2(1.7f * way.CoefSubPoints);
			method_2(2.5f * way.CoefSubPoints);
			method_2(3.4f * way.CoefSubPoints);
			if (subPoints.Count <= num)
			{
				method_2(1.7f * way.CoefSubPoints, 1f);
				method_2(2.5f * way.CoefSubPoints, 1f);
			}
		}
		int num2 = num + 1;
		if (subPoints.Count <= num2)
		{
			int num3 = num2 - subPoints.Count;
			while (num3 > 0)
			{
				num3--;
				PatrolPoint patrolPoint = ((subPoints.Count <= 0) ? this : GClass856.RandomElement(subPoints));
				Vector3 vector = patrolPoint.transform.position;
				float num4 = 1f;
				float x = GClass856.Random(-1f, num4);
				float z = GClass856.Random(-1f, num4);
				Vector3 vector2 = new Vector3(x, 0f, z);
				if (!method_3(vector + vector2, num4, 2f, num4, checkDdist: false))
				{
					num4 = 0.1f;
					x = GClass856.Random(-0.1f, num4);
					z = GClass856.Random(-0.1f, num4);
					vector2 = new Vector3(x, 0f, z);
					method_3(vector + vector2, num4, 2f, num4, checkDdist: false);
				}
			}
			num3 = num - subPoints.Count;
			if (num3 > 0)
			{
				Debug.LogWarning("Auto fixed subpoints of:" + base.gameObject.name);
			}
		}
		if (subPoints.Count <= num)
		{
			int num5 = 4;
			if (subPoints.Count <= 4)
			{
				Debug.LogError($"Под точек слишком мало Меньше:{num5}. !!!!BLOCKER!!!!! Правь сейчас! name:{base.gameObject.name}. ");
			}
			else
			{
				Debug.LogWarning($"Под точек слишком мало Меньше:{num}. name:{base.gameObject.name}. This is not BLOCKER.Под точек нехватает. Это не полный пиздец, но ты лучше присмотрись!");
			}
		}
		int num6 = 25;
		if (base.transform.childCount > 25)
		{
			Debug.LogError($"1Под точек as Transform Больше {num6} < {base.transform.childCount} name:{base.gameObject.name}. This is not BLOCKER. Но лучше посмотреть, явно их многовато!");
		}
		if (subPoints.Count > num6)
		{
			Debug.LogError($"1Под точек as subPoints Больше {num6} < {subPoints.Count} name:{base.gameObject.name}. This is not BLOCKER. Но лучше посмотреть, явно их многовато!");
		}
	}

	public Vector3? GetLookDirection(int index, bool wasCutted)
	{
		if (index >= 0 && LookIndexes >= 0)
		{
			if (LookIndexes <= index)
			{
				return null;
			}
			return PointWithLookSides.Directions[index];
		}
		return null;
	}

	public void SetWay(PatrolWay patrolWay)
	{
		PatrolWay = patrolWay;
	}

	public void CheckData(PatrolWay way)
	{
		if (way.PatrolType == PatrolType.reserved)
		{
			AReserveWayAction componentInChildren = GetComponentInChildren<AReserveWayAction>();
			if (componentInChildren == null)
			{
				Debug.LogError("Patrol point at reserv way have no data to use:" + base.gameObject.name);
				return;
			}
			ActionData = componentInChildren;
			ActionData.RefreshData();
		}
	}

	public bool IsFreeFor(BotOwner botOwner = null)
	{
		if (Owner == null)
		{
			return true;
		}
		if (botOwner == null)
		{
			return false;
		}
		return botOwner == Owner;
	}

	public void TryAutoFixActionData()
	{
		if (ActionData != null)
		{
			ActionData.AutoFix();
		}
	}

	public PatrolPoint GetSubPoint(int index)
	{
		return subPoints[Mathf.Clamp(index, 0, subPoints.Count - 1)];
	}

	public void SetCorePoint(AICorePoint point)
	{
		_corePointId = point.Id;
	}

	public void Restore(AICorePointHolder holder)
	{
		CorePointInGame = holder.GetCorePoint(_corePointId);
	}

	public List<PatrolPoint> method_0()
	{
		PointWithLookSides = null;
		ActionData = null;
		List<PatrolPoint> list = new List<PatrolPoint>();
		List<GameObject> list2 = new List<GameObject>();
		foreach (Transform item in base.transform)
		{
			PatrolPoint component = item.GetComponent<PatrolPoint>();
			if (component != null)
			{
				list.Add(component);
				continue;
			}
			PointWithLookSides component2 = item.GetComponent<PointWithLookSides>();
			if (component2 != null)
			{
				PointWithLookSides = component2;
				PointWithLookSides.Refresh();
				continue;
			}
			AReserveWayAction component3 = item.GetComponent<AReserveWayAction>();
			if (component3 != null)
			{
				ActionData = component3;
			}
			else
			{
				list2.Add(item.gameObject);
			}
		}
		if (list2.Count > 0)
		{
			Debug.LogError("Patrol point have child without any good data:" + base.gameObject.name + "    FIXED " + list2.Count);
			GameObject gameObject;
			PointWithLookSides pointWithLookSides;
			if (PointWithLookSides == null)
			{
				gameObject = new GameObject("Looks");
				pointWithLookSides = gameObject.AddComponent<PointWithLookSides>();
				gameObject.transform.SetParent(base.transform, worldPositionStays: false);
				gameObject.transform.localPosition = Vector3.zero;
			}
			else
			{
				pointWithLookSides = PointWithLookSides;
				gameObject = PointWithLookSides.gameObject;
			}
			foreach (GameObject item2 in list2)
			{
				item2.transform.SetParent(gameObject.transform, worldPositionStays: true);
				Vector3 v = item2.transform.position - base.transform.position;
				if (v.sqrMagnitude <= 1E-06f)
				{
					Debug.LogError("WRONG DIST " + item2.gameObject.name);
				}
				v.y = 0f;
				v = GClass855.NormalizeFastSelf(v);
				pointWithLookSides.Directions.Add(v);
			}
			PointWithLookSides = pointWithLookSides;
		}
		return list;
	}

	public void method_1()
	{
		LookDirections[] componentsInChildren = GetComponentsInChildren<LookDirections>();
		foreach (LookDirections lookDirections in componentsInChildren)
		{
			float magnitude = (lookDirections.Position - base.transform.position).magnitude;
			if (magnitude > 12f)
			{
				Debug.LogError("SubManual: distToCore > 12:" + magnitude + "    atpoint: " + base.gameObject.name + "   fix it MANUALLY");
			}
			if (NavMesh.SamplePosition(lookDirections.Position, out var hit, 5f, -1))
			{
				if (hit.distance > 0.5f)
				{
					Debug.LogError("SubManual: hit.distance too long:" + hit.distance + "    atpoint: " + base.gameObject.name + "    autofix");
				}
				NavMeshPath path = new NavMeshPath();
				if (NavMesh.CalculatePath(lookDirections.Position, hit.position, -1, path))
				{
					lookDirections.transform.position = hit.position;
					lookDirections.NormalizeLookSide();
					if (lookDirections.GetComponent<PatrolPoint>() == null)
					{
						lookDirections.gameObject.AddComponent<PatrolPoint>();
						GameObject obj = new GameObject("Look sides");
						PointWithLookSides pointWithLookSides = obj.AddComponent<PointWithLookSides>();
						obj.transform.SetParent(lookDirections.gameObject.transform, worldPositionStays: false);
						pointWithLookSides.Directions = new List<Vector3>();
						pointWithLookSides.Directions.Add(lookDirections.dir);
					}
				}
				else
				{
					Debug.LogError("SubManual: NO PATH AT SUB POINT:    at point: " + base.gameObject.name + "   fix it MANUALLY");
				}
			}
			else
			{
				Debug.LogError("SubManual: Can't find navmesh near:" + base.gameObject.name + "   fix it MANUALLY");
			}
		}
		componentsInChildren = GetComponentsInChildren<LookDirections>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.DestroyImmediate(componentsInChildren[i]);
		}
	}

	public void method_2(float offset, float yOffset = 0f)
	{
		Vector3 v = new Vector3(GClass856.Random(-1f, 1f), 0f, GClass856.Random(-1f, 1f));
		v = GClass855.NormalizeFastSelf(v) * offset;
		for (int i = 0; i < 4; i++)
		{
			v = GClass855.RotateOnAngUp(v, GClass856.Random(70f, 110f));
			Vector3 pos = position + v;
			method_3(pos, offset, yOffset, 0.3f, checkDdist: true);
		}
	}

	public bool method_3(Vector3 pos, float offset, float yOffset, float sampleOffset, bool checkDdist)
	{
		NavMeshHit hit = default(NavMeshHit);
		if (NavMesh.SamplePosition(pos, out hit, sampleOffset + yOffset, -1))
		{
			NavMeshPath navMeshPath = new NavMeshPath();
			if (NavMesh.CalculatePath(position, pos, -1, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
			{
				float num = GClass371.CalculatePathLength(navMeshPath);
				if (!checkDdist || num < offset * 4f)
				{
					Debug.DrawRay(hit.position, Vector3.up * 2f, Color.green, 5f);
					GameObject obj = new GameObject("SubPoint_" + (base.transform.childCount + 1));
					PatrolPoint item = obj.AddComponent<PatrolPoint>();
					obj.transform.SetParent(base.transform);
					obj.transform.position = hit.position;
					subPoints.Add(item);
					return true;
				}
			}
		}
		return false;
	}

	public void method_4()
	{
		Gizmos.color = new Color(0f, 1f, 1f, 0.9f);
		GClass850.DrawCube(base.transform.position, base.transform.rotation, new Vector3(1f, 4f, 1f) * 0.2f);
	}

	public void method_5()
	{
		if (GClass806.NoDrawSatelites)
		{
			return;
		}
		Gizmos.color = new Color(1f, 0.9215686f, 0.01568628f, 0.9f);
		if (subPoints == null)
		{
			return;
		}
		foreach (PatrolPoint subPoint in subPoints)
		{
			if (subPoint != null)
			{
				GClass850.DrawCube(subPoint.transform.position, base.transform.rotation, new Vector3(1f, 4f, 1f) * 0.14f);
			}
		}
	}

	public void OnDrawGizmos()
	{
		if (GClass806.AlwaysDrawPatrolPoints)
		{
			method_7();
		}
	}

	public void OnDrawGizmosSelected()
	{
		if (!GClass806.AlwaysDrawPatrolPoints)
		{
			method_7();
		}
		method_6();
	}

	public void method_6()
	{
		if (Owner != null)
		{
			Gizmos.color = new Color(0.8f, 0.2f, 0.4f, 0.7f);
			for (int i = 0; i < Owner.Id; i++)
			{
				Gizmos.DrawSphere(base.transform.position + Vector3.up * (1f + 0.2f * (float)i), 0.1f);
			}
		}
	}

	public void method_7()
	{
		if (ActionData != null)
		{
			ActionData.DrawGizmos();
		}
		method_4();
		method_5();
		if (Physics.Raycast(base.transform.position, Vector3.down, out var hitInfo, 50f))
		{
			Gizmos.DrawLine(base.transform.position, hitInfo.point);
		}
	}
}
