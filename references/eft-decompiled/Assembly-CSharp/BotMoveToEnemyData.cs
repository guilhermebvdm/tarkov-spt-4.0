using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class BotMoveToEnemyData(BotOwner owner) : GClass429(owner)
{
	[NonSerialized]
	public const float SDIST_TOO_CLOSE = 4f;

	[NonSerialized]
	public const float SMAPLE_DIST = 2.6f;

	[NonSerialized]
	public const float TOOFAR_BASE_PATH_DIST = 2f;

	[NonSerialized]
	public NavMeshPath Path = new NavMeshPath();

	public bool TryMoveToEnemy(Vector3 targetPoint)
	{
		if (method_0(targetPoint, out var curTargetPos) && BotOwner_0.GoToPoint(curTargetPos, slowAtTheEnd: true, -1f, getUpWithCheck: false, mustHaveWay: false) == NavMeshPathStatus.PathComplete)
		{
			return true;
		}
		if (!ShallRecalWay(out var _) && Time.time - BotOwner_0.Mover.LastPathSetTime < 10f)
		{
			return true;
		}
		if (BotOwner_0.GoToPoint(targetPoint, slowAtTheEnd: true, -1f, getUpWithCheck: false, mustHaveWay: false) == NavMeshPathStatus.PathComplete && BotOwner_0.Mover.TargetPoint.HasValue)
		{
			Vector3 value = BotOwner_0.Mover.TargetPoint.Value;
			if ((targetPoint - value).magnitude < 2f)
			{
				return true;
			}
		}
		if (NavMesh.SamplePosition(targetPoint, out var hit, 2.6f, -1) && BotOwner_0.GoToPoint(hit.position, slowAtTheEnd: false, -1f, getUpWithCheck: false, mustHaveWay: false) == NavMeshPathStatus.PathComplete)
		{
			return true;
		}
		List<CustomNavigationPoint> closePoints = BotOwner_0.BotsGroup.CoverPointMaster.GetClosePoints(targetPoint, BotOwner_0, 25f);
		if (closePoints.Count > 0)
		{
			CustomNavigationPoint customNavigationPoint = GClass856.RandomElement(closePoints);
			if (customNavigationPoint != null && Mathf.Abs(customNavigationPoint.Position.y - targetPoint.y) < 1f && BotOwner_0.GoToPoint(customNavigationPoint.Position, slowAtTheEnd: true, -1f, getUpWithCheck: false, mustHaveWay: false) == NavMeshPathStatus.PathComplete)
			{
				return true;
			}
			customNavigationPoint = GClass856.RandomElement(closePoints);
			if (customNavigationPoint != null && BotOwner_0.GoToPoint(customNavigationPoint.Position, slowAtTheEnd: true, -1f, getUpWithCheck: false, mustHaveWay: false) == NavMeshPathStatus.PathComplete)
			{
				return true;
			}
		}
		CustomNavigationPoint freeClosePoint = BotOwner_0.Covers.GetFreeClosePoint(targetPoint, 0f);
		if (freeClosePoint != null && BotOwner_0.GoToPoint(freeClosePoint.Position, slowAtTheEnd: true, -1f, getUpWithCheck: false, mustHaveWay: false) == NavMeshPathStatus.PathComplete)
		{
			return true;
		}
		return false;
	}

	public bool ShallRecalWay(out Vector3 lastPos)
	{
		if (!BotOwner_0.Mover.HasPathAndNoComplete)
		{
			lastPos = Vector3.zero;
			return true;
		}
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			lastPos = Vector3.zero;
			return true;
		}
		Vector3? targetPoint = BotOwner_0.Mover.TargetPoint;
		if (!targetPoint.HasValue)
		{
			lastPos = Vector3.zero;
			return false;
		}
		Vector3 currPosition = BotOwner_0.Memory.GoalEnemy.CurrPosition;
		float sqrMagnitude = (targetPoint.Value - currPosition).sqrMagnitude;
		lastPos = targetPoint.Value;
		if (sqrMagnitude > 4f)
		{
			return true;
		}
		return false;
	}

	public bool method_0(Vector3 shootTargetPoint, out Vector3 curTargetPos)
	{
		if (!BotOwner_0.Mover.HasPathAndNoComplete)
		{
			curTargetPos = Vector3.zero;
			return false;
		}
		if (BotOwner_0.Mover.TargetPoint.HasValue)
		{
			curTargetPos = BotOwner_0.Mover.TargetPoint.Value;
			if (BotOwner_0.Memory.HaveEnemy)
			{
				return GClass369.CanShoot(curTargetPos, BotOwner_0.Memory.GoalEnemy);
			}
			return GClass369.CanShootToTarget(new ShootPointClass(shootTargetPoint + Vector3.up), curTargetPos, BotOwner_0.LookSensor.Mask);
		}
		curTargetPos = Vector3.zero;
		return false;
	}

	public bool CanMoveTo(Vector3 position)
	{
		if (NavMesh.CalculatePath(BotOwner_0.Position, position, -1, Path))
		{
			switch (Path.status)
			{
			case NavMeshPathStatus.PathPartial:
			{
				if (Path.corners.Length < 2)
				{
					return false;
				}
				Vector3 vector = Path.corners[Path.corners.Length - 1];
				if ((position - vector).magnitude < 2f)
				{
					return true;
				}
				break;
			}
			case NavMeshPathStatus.PathComplete:
				return true;
			}
		}
		return false;
	}
}
