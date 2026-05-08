using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIPlaceInfoHalloweenCircle : AIPlaceInfoLogic
{
	public Transform CenterPoint;

	public List<Transform> CastPoints;

	public List<Vector3> CoverPoints;

	public bool AlwaysDraw;

	public override void EditorCheck()
	{
		if (CenterPoint == null)
		{
			Debug.LogError("AIPlaceInfoHalloweenCircle error. CenterPoint == null. Holder: " + base.transform.name + " ");
		}
		else if (CastPoints != null && CastPoints.Count > 0)
		{
			method_0(CenterPoint);
			foreach (Transform castPoint in CastPoints)
			{
				method_0(castPoint);
			}
			foreach (Transform castPoint2 in CastPoints)
			{
				if (!BotPathFinderClass.FindPath(castPoint2.position, CenterPoint.position, out var _))
				{
					Debug.LogError("AIPlaceInfoHalloweenCircle error. WRONG WAY FROM " + method_1(castPoint2) + " to center. Holder: " + base.transform.name);
				}
				if (AICorePointHolder.GetAnyPointToConnect(castPoint2.position) == null)
				{
					Debug.LogError("AIPlaceInfoHalloweenCircle error. WRONG WAY FROM (corePoint == null) " + method_1(castPoint2) + " to ANY core point. Holder: " + base.transform.name);
				}
			}
			base.EditorCheck();
		}
		else
		{
			Debug.LogError("AIPlaceInfoHalloweenCircle error.CastPoints == null || CastPoints.Count <= 0. Holder: " + method_1(base.transform) + " ");
		}
	}

	public void method_0(Transform castPoint)
	{
		if (NavMesh.SamplePosition(castPoint.position, out var hit, 1f, -1))
		{
			castPoint.position = hit.position;
			return;
		}
		Debug.LogError("AIPlaceInfoHalloweenCircle error. " + method_1(castPoint) + " of " + base.transform.name + " NOT at navmesh ");
	}

	public string method_1(Transform go)
	{
		string text = ((base.transform.parent != null) ? base.transform.parent.name : "no parent");
		return go.name + " parent:" + text;
	}

	public void OnDrawGizmosSelected()
	{
		if (!AlwaysDraw)
		{
			method_2();
		}
	}

	public void OnDrawGizmos()
	{
		if (AlwaysDraw)
		{
			method_2();
		}
	}

	public void method_2()
	{
		Gizmos.color = Color.yellow;
		if (CenterPoint != null)
		{
			Gizmos.DrawSphere(CenterPoint.position, 0.3f);
		}
		Gizmos.color = Color.blue;
		foreach (Transform castPoint in CastPoints)
		{
			if (castPoint != null)
			{
				Gizmos.DrawLine(castPoint.position, CenterPoint.position);
				Gizmos.DrawSphere(castPoint.position, 0.3f);
			}
		}
	}
}
