using System;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class GClass565(BotOwner owner, int index) : GClass563(owner, index)
{
	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public bool Bool_4 = true;

	[NonSerialized]
	public bool Bool_5;

	public override bool SetTarget(GStruct25 trg, Vector3? lookSide)
	{
		Gstruct25_0 = trg;
		Nullable_0 = lookSide;
		return true;
	}

	public override void Update()
	{
		bool extraTarget;
		GStruct25 patrolTarget = method_1(out extraTarget);
		bool flag;
		if ((flag = GClass510.IsCome(BotOwner_0, patrolTarget.Position, extraTarget, out var _)) != Bool_5 && BotOwner_0.Settings.FileSettings.Patrol.FOLLOWER_START_MOVE_DELAY > 0f)
		{
			Float_0 = Time.time + BotOwner_0.Settings.FileSettings.Patrol.FOLLOWER_START_MOVE_DELAY;
		}
		Bool_5 = flag;
		if (flag)
		{
			float pose = ((!patrolTarget.HasValue) ? 1f : (patrolTarget.SitAtEnd ? 0f : 1f));
			BotOwner_0.SetPose(pose);
			method_2(patrolTarget, isClose: true);
			BotOwner_0.StopMove();
		}
		else
		{
			if (base.HaveProblems)
			{
				BotObserveDataClass.SetVector(BotOwner_0.Mover.NormDirCurPoint);
			}
			if (Bool_2)
			{
				method_3(patrolTarget.Position, withSprint: false);
			}
			else
			{
				PatrolPoint patrolPoint = BossPatrolPoint();
				if (patrolPoint != null)
				{
					method_3(patrolPoint.Position, withSprint: false);
				}
			}
			BotObserveDataClass.Update();
		}
		if (Bool_1)
		{
			base.HaveProblems = false;
		}
		else
		{
			base.HaveProblems = false;
		}
	}

	public void method_2(GStruct25 patrolTarget, bool isClose)
	{
		if (Float_2 < Time.time)
		{
			if (PatrolPoint_0 == null)
			{
				PatrolPoint_0 = BossPatrolPoint();
			}
			float num = 1.5f;
			if (isClose)
			{
				if (patrolTarget.HasLookSides)
				{
					BotObserveDataClass.SetVectorToLook(GClass856.RandomElement(patrolTarget.LookSides.Directions));
				}
				else if (PatrolPoint_0.HaveLookSide)
				{
					BotObserveDataClass.SetVectorToLook(PatrolPoint_0.LookDir(0));
				}
				else
				{
					Vector3 direction = GClass369.Test4Sides(BotOwner_0.Position - BotOwner_0.BotFollower.BossToFollow.Position, BotOwner_0.GetPlayer.PlayerBones.Head.position);
					BotObserveDataClass.SetVector(direction);
					num *= 2f;
				}
			}
			else
			{
				BotObserveDataClass.SetVector(BotOwner_0.Mover.NormDirCurPoint);
			}
			Float_2 = Time.time + num;
		}
		BotObserveDataClass.Update();
	}

	public void method_3(Vector3 v, bool withSprint)
	{
		if (BotOwner_0.Mover.HasPathAndNoComplete && BotOwner_0.Mover.TargetPoint.HasValue && (BotOwner_0.Mover.TargetPoint.Value - v).sqrMagnitude < 1f)
		{
			return;
		}
		if (!Bool_0)
		{
			BotOwner_0.Mover.SetTargetMoveSpeed(BotOwner_0.BotFollower.BossToFollow.MoveSpeed);
		}
		if (Float_0 < Time.time)
		{
			Float_0 = Time.time + 1.5f;
			NavMeshHit hit;
			if (GClass855.IsOnNavMesh(v, 0.2f))
			{
				NavMeshPathStatus navMeshPathStatus = BotOwner_0.GoToPoint(v);
				Bool_4 = navMeshPathStatus == NavMeshPathStatus.PathComplete;
			}
			else if (NavMesh.SamplePosition(v, out hit, 2f, -1))
			{
				NavMeshPathStatus navMeshPathStatus2 = BotOwner_0.GoToPoint(hit.position);
				Bool_4 = navMeshPathStatus2 == NavMeshPathStatus.PathComplete;
			}
			else
			{
				Bool_4 = false;
			}
		}
	}
}
