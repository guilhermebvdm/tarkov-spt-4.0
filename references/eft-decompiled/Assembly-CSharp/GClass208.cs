using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public abstract class GClass208 : GClass200<GClass29>
{
	[CompilerGenerated]
	public class Class117
	{
		public GClass208 gclass208_0;

		public EnemyInfo enemy;

		public bool method_0(GroupPoint arg)
		{
			if ((arg.Position - enemy.CurrPosition).sqrMagnitude > gclass208_0.Float_2)
			{
				return true;
			}
			return false;
		}
	}

	[NonSerialized]
	public NavMeshPath NavMeshPath_0 = new NavMeshPath();

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public EStealthMove EstealthMove_0;

	[NonSerialized]
	public float Float_2 = 400f;

	[NonSerialized]
	public GClass25 Gclass25_0;

	public GClass208(BotOwner bot, float enemyMinDist)
		: base(bot)
	{
		Gclass25_0 = new GClass25(5f, method_6);
		Float_2 = enemyMinDist * enemyMinDist;
	}

	public override void UpdateNodeByBrain(GClass29 data)
	{
		method_0();
		BotOwner_0.Mover.SetTargetMoveSpeed(1f);
		BotOwner_0.SetPose(1f);
		method_5(data);
	}

	public void method_5(GClass29 data)
	{
		if (BotOwner_0.Memory.CurCustomCoverPoint != null)
		{
			GClass369.DebugDrawArc(BotOwner_0.Memory.CurCustomCoverPoint.Position, BotOwner_0.Position, 4f, Time.deltaTime, Color.magenta);
		}
		switch (EstealthMove_0)
		{
		case EStealthMove.toSide:
			if (BotOwner_0.SDistTo(CustomNavigationPoint_0.Position) < 1f)
			{
				method_7(EStealthMove.normal);
			}
			GClass369.DebugDrawArc(CustomNavigationPoint_0.Position, BotOwner_0.Position, 4f, Time.deltaTime, Color.yellow);
			break;
		case EStealthMove.normal:
			if (BotOwner_0.Memory.CurCustomCoverPoint == null)
			{
				Gclass25_0.Update();
			}
			else if (!method_9(data))
			{
				if (Float_1 < Time.time)
				{
					Float_1 = Time.time + 4f;
					BotOwner_0.GoToPoint(BotOwner_0.Memory.CurCustomCoverPoint);
				}
				if (BotOwner_0.SDistTo(BotOwner_0.Memory.CurCustomCoverPoint.Position) < 1f)
				{
					BotOwner_0.Memory.ComeToPoint();
					BotOwner_0.StopMove();
				}
			}
			break;
		}
	}

	public void method_6()
	{
		BotOwner_0.BotLight.TurnOff();
		CustomNavigationPoint customNavigationPoint = BotOwner_0.BotAttackManager.PointToGo();
		if (customNavigationPoint != null)
		{
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(customNavigationPoint);
			BotOwner_0.GoToPoint(customNavigationPoint);
		}
	}

	public void method_7(EStealthMove eStealthMove)
	{
		if (EstealthMove_0 != eStealthMove)
		{
			EstealthMove_0 = eStealthMove;
			switch (eStealthMove)
			{
			case EStealthMove.toSide:
				BotOwner_0.GoToPoint(CustomNavigationPoint_0.Position);
				break;
			case EStealthMove.normal:
				method_8();
				break;
			}
		}
	}

	public void method_8()
	{
		Float_0 = Time.time + 4f;
		BotOwner_0.SetPose(1f);
		if (!BotOwner_0.HasPathAndNotComplete && BotOwner_0.Memory.CurCustomCoverPoint != null && BotOwner_0.Memory.CurCustomCoverPoint.Owner == BotOwner_0.Id)
		{
			if ((BotOwner_0.Position - BotOwner_0.Memory.CurCustomCoverPoint.Position).sqrMagnitude > 1f)
			{
				BotOwner_0.GoToPoint(BotOwner_0.Memory.CurCustomCoverPoint);
			}
			else if (!BotOwner_0.Memory.ComeToPoint())
			{
				BotOwner_0.BotAttackManager.UpdateNextTick();
			}
		}
	}

	public bool method_9(GClass29 data)
	{
		Vector3 position = BotOwner_0.Memory.CurCustomCoverPoint.Position;
		if (Float_0 < Time.time)
		{
			Float_0 = Time.time + 3f;
			float num = BotOwner_0.DistTo(position);
			EnemyInfo enemy = BotOwner_0.Memory.GoalEnemy;
			if (num < enemy.Distance)
			{
				return false;
			}
			Vector3 pos;
			if (data != null)
			{
				pos = data.FlankPoint;
			}
			else
			{
				Vector3 projectionPoint = GClass369.GetProjectionPoint(enemy.CurrPosition, BotOwner_0.Position, position);
				float magnitude = (projectionPoint - BotOwner_0.Position).magnitude;
				Vector3 vector = GClass855.NormalizeFastSelf(enemy.CurrPosition - projectionPoint);
				pos = projectionPoint - vector * magnitude;
				float magnitude2 = (projectionPoint - position).magnitude;
				if (num < magnitude2)
				{
					return false;
				}
			}
			CustomNavigationPoint closestPoint = BotOwner_0.Covers.GetClosestPoint(pos, (GroupPoint arg) => ((arg.Position - enemy.CurrPosition).sqrMagnitude > Float_2) ? true : false);
			if (closestPoint != null)
			{
				float magnitude3 = (closestPoint.Position - BotOwner_0.Position).magnitude;
				if (num < magnitude3)
				{
					return false;
				}
				NavMeshPath navMeshPath = new NavMeshPath();
				NavMesh.CalculatePath(closestPoint.Position, BotOwner_0.Position, -1, navMeshPath);
				if (navMeshPath.status != NavMeshPathStatus.PathComplete)
				{
					return false;
				}
				if (GClass371.CalculatePathLength(navMeshPath) > magnitude3 * 2f)
				{
					return false;
				}
				float magnitude4 = (position - closestPoint.Position).magnitude;
				if (magnitude4 > num * 1.5f)
				{
					return false;
				}
				NavMesh.CalculatePath(position, closestPoint.Position, -1, navMeshPath);
				if (navMeshPath.status != NavMeshPathStatus.PathComplete)
				{
					return false;
				}
				if (GClass371.CalculatePathLength(navMeshPath) > magnitude4 * 2f)
				{
					return false;
				}
				CustomNavigationPoint_0 = closestPoint;
				Debug.DrawRay(closestPoint.Position, Vector3.up * 30f, Color.red, 10f);
				Debug.DrawRay(position, Vector3.up * 30f, Color.magenta, 10f);
				method_7(EStealthMove.toSide);
				Float_0 = Time.time + 13f;
				return true;
			}
		}
		return false;
	}
}
