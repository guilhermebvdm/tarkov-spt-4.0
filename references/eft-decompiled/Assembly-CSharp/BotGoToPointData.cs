using System;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class BotGoToPointData : GClass429
{
	[NonSerialized]
	public bool PointhRefreshed;

	[NonSerialized]
	public float DistToPoint;

	[NonSerialized]
	public float LastPosibleRecalc;

	[NonSerialized]
	public Vector3 LastRecalc;

	[NonSerialized]
	public const float SDIST_TO_RECALC = 9f;

	[NonSerialized]
	public NavMeshPath Path = new NavMeshPath();

	[field: NonSerialized]
	public Vector3 Point { get; set; }

	public BotGoToPointData(BotOwner owner)
		: base(owner)
	{
	}

	public void SetPoint(Vector3 p)
	{
		if ((Point - p).sqrMagnitude > 1f)
		{
			NavMesh.CalculatePath(BotOwner_0.Position, p, -1, Path);
			if (Path.status != NavMeshPathStatus.PathComplete)
			{
				CustomNavigationPoint closestPoint = BotOwner_0.Covers.GetClosestPoint(p);
				if (closestPoint != null)
				{
					Point = closestPoint.Position;
				}
			}
		}
		Point = p;
		LastPosibleRecalc = Time.time + 0.1f;
		PointhRefreshed = true;
	}

	public bool HaveTarget()
	{
		return Point.sqrMagnitude >= Mathf.Epsilon;
	}

	public void UpdateToGo(bool sprint, float speed = 1f, float pose = 1f)
	{
		if (HaveTarget())
		{
			if (BotOwner_0.Memory.HaveEnemy && BotOwner_0.Memory.GoalEnemy.Distance < 10f)
			{
				BotOwner_0.Steering.LookToPoint(BotOwner_0.Memory.GoalEnemy.GetBodyPartPosition());
			}
			else
			{
				BotOwner_0.LookData.SetLookPointByHearing();
			}
			if (sprint && DistToPoint < BotOwner_0.Settings.FileSettings.Move.DIST_SPRINT_GO_TO_SOME_POINT)
			{
				sprint = false;
			}
			BotOwner_0.Sprint(sprint);
			BotOwner_0.SetPose(pose);
			BotOwner_0.SetTargetMoveSpeed(speed);
			float num = (((LastRecalc - Point).sqrMagnitude > 9f) ? 20f : 3f);
			if (Time.time - LastPosibleRecalc > num)
			{
				LastPosibleRecalc = Time.time;
				PointhRefreshed = true;
			}
			if (PointhRefreshed)
			{
				LastRecalc = Point;
				PointhRefreshed = false;
				BotOwner_0.GoToPoint(Point, slowAtTheEnd: true, -1f, getUpWithCheck: false, mustHaveWay: false);
			}
		}
	}

	public bool IsCome()
	{
		if (!HaveTarget())
		{
			return true;
		}
		Vector3 vector = Point - BotOwner_0.Transform.position;
		vector.y = ((Mathf.Abs(vector.y) > BotOwner_0.Settings.FileSettings.Move.Y_APPROXIMATION) ? vector.y : 0f);
		DistToPoint = vector.magnitude;
		return DistToPoint < BotOwner_0.Settings.FileSettings.Move.REACH_DIST;
	}
}
