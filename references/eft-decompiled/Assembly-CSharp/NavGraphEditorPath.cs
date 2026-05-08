using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NavGraphEditorPath
{
	public int FromId;

	public int ToId;

	public float Dist;

	public int Id;

	[NonSerialized]
	public NavGraphEditorPoint From;

	[NonSerialized]
	public NavGraphEditorPoint To;

	[NonSerialized]
	public HashSet<NavGraphEditorPoint> WaysTo = new HashSet<NavGraphEditorPoint>();

	public int DoorLinkId = -1;

	[NonSerialized]
	public float Float_0 = 1f;

	public float Value => Dist * Float_0;

	public static NavGraphEditorPath Create(NavGraphEditorPoint from, NavGraphEditorPoint to, float dist, NavGraphContainer container)
	{
		if (from.Id == to.Id)
		{
			Debug.LogError($"NavGraphEditorPath have same ids:{from.Id}");
		}
		return new NavGraphEditorPath(from, to, dist, container);
	}

	public NavGraphEditorPath(NavGraphEditorPoint newPoint, NavGraphEditorPoint prevPoint, float dist, NavGraphContainer container)
	{
		Id = container.GetNewId();
		To = newPoint;
		From = prevPoint;
		FromId = From.Id;
		ToId = To.Id;
		Dist = dist;
		Debug.DrawRay(From.Position, Vector3.up * 20f, Color.yellow, 5f);
		Debug.DrawRay(To.Position, Vector3.up * 20f, Color.green, 5f);
		if (From.ClosestPointsAsPoints.Contains(To))
		{
			Debug.LogError("contains test");
		}
		else
		{
			NavGraphEditorPointWay navGraphEditorPointWay = new NavGraphEditorPointWay(To, From, this, container.GetNewId());
			NavGraphEditorPointWay navGraphEditorPointWay2 = new NavGraphEditorPointWay(From, To, this, container.GetNewId());
			From.AddWay(navGraphEditorPointWay);
			To.AddWay(navGraphEditorPointWay2);
			navGraphEditorPointWay.Reverse = navGraphEditorPointWay2;
			navGraphEditorPointWay2.Reverse = navGraphEditorPointWay;
			navGraphEditorPointWay.TargetPoint = (navGraphEditorPointWay2.StartPoint = To);
			navGraphEditorPointWay.StartPoint = (navGraphEditorPointWay2.TargetPoint = From);
			navGraphEditorPointWay.Path = (navGraphEditorPointWay2.Path = this);
			navGraphEditorPointWay.ReverseId = navGraphEditorPointWay2.Id;
			navGraphEditorPointWay2.ReverseId = navGraphEditorPointWay.Id;
			container.AddWay(navGraphEditorPointWay);
			container.AddWay(navGraphEditorPointWay2);
		}
		container.AddPath(this);
	}

	public Vector3 MidPos()
	{
		return (To.Position + From.Position) / 2f;
	}

	public void SetBaseCoef()
	{
		Float_0 = 1f;
	}

	public void DropCoef(float coef)
	{
		float float_ = Float_0 / coef;
		Float_0 = float_;
	}

	public void SetCoef(float coef)
	{
		float float_ = Float_0 * coef;
		Float_0 = float_;
	}

	public void SetDoorLinkForOpenState(NavMeshDoorLink link)
	{
		DoorLinkId = link.Id;
		link.ShallTryInteract = true;
		link.ConnectedNavGraphPathOpenStateId = Id;
	}

	public void SetDoorLinkForCloseState(NavMeshDoorLink link)
	{
		DoorLinkId = link.Id;
		link.ConnectedNavGraphPathCloseStateId = Id;
	}

	public void DrawGizmosSelected(Vector3 up)
	{
		float num = 1f;
		float max = 100f;
		float t = (Mathf.Clamp(Float_0, num, max) - num) / 99f;
		Color color = Color.Lerp(Color.green, Color.red, t);
		color.a = 0.7f;
		Gizmos.color = color;
		Gizmos.DrawLine(From.Position + up, To.Position + up);
		if (DoorLinkId > 0)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(From.Position + up * 1.1f, To.Position + up * 1.1f);
		}
	}
}
