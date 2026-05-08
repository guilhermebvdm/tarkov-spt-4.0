using System;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class GClass564 : GClass563
{
	[NonSerialized]
	public float Float_2;

	public GClass564(BotOwner owner, int index)
		: base(owner, index)
	{
	}

	public override void Activate()
	{
		BotOwner_0.BotFollower.BossToFollow.PatrollingData.OnStatusChanged += method_2;
	}

	public override bool SetTarget(GStruct25 trg, Vector3? lookSide)
	{
		NavMeshPath navMeshPath = new NavMeshPath();
		if (NavMesh.CalculatePath(BotOwner_0.Position, trg.Position, -1, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete && (navMeshPath.corners[navMeshPath.corners.Length - 1] - trg.Position).sqrMagnitude < BotOwner_0.Settings.FileSettings.Move.REACH_DIST * BotOwner_0.Settings.FileSettings.Move.REACH_DIST)
		{
			Gstruct25_0 = trg;
			return true;
		}
		return false;
	}

	public override void Update()
	{
		GStruct25 gStruct = (base.HaveProblems ? Gstruct25_0 : ((BotOwner_0.BotFollower.BossToFollow.PatrollingData.Status == PatrolStatus.pause) ? default(GStruct25) : new GStruct25(BossPatrolPoint().Position)));
		if (gStruct.HasValue)
		{
			if (GClass510.IsCome(BotOwner_0, gStruct.Position, extraTarget: false, out var _))
			{
				if (Float_2 < Time.time)
				{
					float num = 1.5f;
					if (Bool_1)
					{
						Vector3 direction = GClass369.Test4Sides(BotOwner_0.Position - BotOwner_0.BotFollower.BossToFollow.Position, BotOwner_0.GetPlayer.PlayerBones.Head.position);
						BotObserveDataClass.SetVector(direction);
						num *= 2f;
					}
					else
					{
						BotObserveDataClass.SetVector(BotOwner_0.Mover.NormDirCurPoint);
					}
					Float_2 = Time.time + num;
				}
				if (base.HaveProblems)
				{
					base.HaveProblems = false;
				}
				BotOwner_0.StopMove();
			}
			else if (base.HaveProblems || !Bool_2)
			{
				BotObserveDataClass.SetVector(BotOwner_0.Mover.NormDirCurPoint);
				if (Float_0 < Time.time)
				{
					Float_0 = Time.time + 1.5f;
					method_3(gStruct.Position);
				}
			}
		}
		BotObserveDataClass.Update();
	}

	public void method_2(PatrolStatus obj)
	{
		if (obj == PatrolStatus.go)
		{
			if (Bool_0)
			{
				PatrolPoint patrolPoint = BossPatrolPoint();
				method_3(patrolPoint.Position);
				Float_0 = Time.time + 1.5f;
			}
			else
			{
				Float_0 = Time.time + 0.5f;
			}
		}
	}

	public void method_3(Vector3 v)
	{
		BotOwner_0.GoToPoint(v);
	}

	public override void Dispose()
	{
		BotOwner_0.BotFollower.BossToFollow.PatrollingData.OnStatusChanged -= method_2;
	}
}
