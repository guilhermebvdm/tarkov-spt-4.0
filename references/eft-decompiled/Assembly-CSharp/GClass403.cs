using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public abstract class GClass403
{
	public static GizmosConfig Config => DebugBotData.Instance.Gizmos;

	public static void DrawBotOwnerGizmos(BotOwner owner)
	{
	}

	public static void DrawBotOwnerGizmosSelected(BotOwner owner)
	{
		if (Config.DrawDestinationPoint)
		{
			Gizmos.color = Color.yellow;
			smethod_0(owner, 0.11f);
		}
		if (owner == null || owner.Mover == null)
		{
			return;
		}
		owner.Mover.OnDrawGizmosPrevWay();
		if (Config.DrawVisionArrowSelected)
		{
			owner.EnemiesController.DrawGizmos();
		}
		if (Config.LookSensorGizmoSelected)
		{
			owner.LookSensor.DrawGizmosSelected();
		}
		if (Config.DrawVisionArrowSelected)
		{
			owner.Mover.DrawVisionLines();
		}
		if (Config.DrawEnemiesLastPosSelected)
		{
			smethod_1(owner);
		}
		if (Config.DrawCellDataSelected)
		{
			owner.NearDoorData.DrawGizmosSelected();
		}
		if (Config.DrawVoxel)
		{
			owner.AIData.DrawGizmosSelectedVoxel();
		}
		if (owner.Memory == null)
		{
			return;
		}
		owner.CoverSearchInfo.TryDrawGizmosLastSearchDebugByFull();
		owner.CoverSearchInfo.TryDrawGizmosLastSearchDebugClosest();
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in owner.EnemiesController.EnemyInfos)
		{
			Gizmos.color = Color.red;
			if (Config.DrawCanShootLineSelected && enemyInfo.Value.CanShoot)
			{
				Gizmos.DrawLine(owner.Transform.position + Vector3.up * 1.5f, enemyInfo.Key.Transform.position + Vector3.up * 1.5f);
			}
			Gizmos.color = Color.yellow;
			if (Config.DrawVisibleLineSelected && enemyInfo.Value.IsVisible)
			{
				Gizmos.DrawLine(owner.Transform.position + Vector3.up * 1.4f, enemyInfo.Key.Transform.position + Vector3.up * 1.4f);
			}
		}
		if (Config.DrawPatrollingDataSelected)
		{
			owner.PatrollingData.DrawGizomsSelectedFromBot();
		}
		if (Config.DrawBewareGrenadeSelected)
		{
			owner.BewareGrenade.DrawGizmosSelected();
		}
		if (owner.Transform == null || owner.Mover == null)
		{
			return;
		}
		Vector3 vector = owner.Transform.position + Vector3.up * 1f;
		Vector3 to = vector + owner.Mover.NormDirCurPoint;
		Gizmos.color = Color.blue;
		if (Config.DrawNormDirCurPointSelected)
		{
			Gizmos.DrawLine(vector, to);
		}
		Gizmos.color = Color.yellow;
		Vector3 realDestPoint = owner.Mover.RealDestPoint;
		Vector3 to2 = realDestPoint + Vector3.up * 2f;
		if (Config.DrawRealDestinationPointSelected)
		{
			Gizmos.DrawLine(realDestPoint, to2);
		}
		if (Config.DrawMoverWaySelected)
		{
			owner.Mover.DrawSelectedGizmosWay();
		}
		if (owner.Memory.GoalTarget.HavePlaceTarget())
		{
			Gizmos.color = Color.green;
			if (Config.DrawGoalTargetPositionSelected)
			{
				Gizmos.DrawSphere(owner.Memory.GoalTarget.GoalTarget.Position, 0.3f);
			}
			if (Config.DrawGoalTargetBasePointSelected)
			{
				Gizmos.DrawSphere(owner.Memory.GoalTarget.GoalTarget.BasePoint, 0.15f);
			}
		}
		if (owner.BotsGroup == null || !Config.DrawBotsGroupSelected)
		{
			return;
		}
		foreach (PlaceForCheck item in owner.BotsGroup.PlacesForCheck)
		{
			switch (item.Type)
			{
			case PlaceForCheckType.simple:
				Gizmos.color = Color.magenta;
				Gizmos.DrawSphere(item.BasePoint, 0.2f);
				Gizmos.DrawSphere(item.Position, 0.07f);
				break;
			case PlaceForCheckType.danger:
				Gizmos.color = Color.red;
				Gizmos.DrawSphere(item.BasePoint, 0.2f);
				Gizmos.DrawSphere(item.Position, 0.07f);
				break;
			case PlaceForCheckType.suspicious:
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere(item.BasePoint, 0.2f);
				Gizmos.DrawSphere(item.Position, 0.07f);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public static void smethod_0(BotOwner owner, float rad)
	{
		if (owner.Destination.HasValue)
		{
			Gizmos.DrawSphere(owner.Destination.Value + Vector3.up * 0.15f, rad);
			Gizmos.DrawSphere(owner.Destination.Value + Vector3.up * 0.35f, rad);
			Gizmos.DrawSphere(owner.Destination.Value + Vector3.up * 0.55f, rad);
		}
	}

	public static void smethod_1(BotOwner owner)
	{
		if (!DebugBotData.UseDebugData || !Config.DrawEnemiesLastPosSelected)
		{
			return;
		}
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in owner.EnemiesController.EnemyInfos)
		{
			Vector3 enemyLastPositionReal = enemyInfo.Value.EnemyLastPositionReal;
			Vector3 enemyLastPosition = enemyInfo.Value.EnemyLastPosition;
			Gizmos.color = new Color(1f, 0.3f, 0f);
			Gizmos.DrawSphere(enemyLastPositionReal, 0.4f);
			Gizmos.color = new Color(0f, 0.3f, 1f);
			Gizmos.DrawSphere(enemyLastPosition, 0.4f);
		}
	}
}
