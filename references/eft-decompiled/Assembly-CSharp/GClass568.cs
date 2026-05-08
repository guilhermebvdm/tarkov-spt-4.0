using System;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class GClass568 : GClass563
{
	[NonSerialized]
	public float Float_2;

	public GClass568(BotOwner owner, int index)
		: base(owner, index)
	{
	}

	public override bool SetTarget(GStruct25 trg, Vector3? lookSide)
	{
		return true;
	}

	public override void Update()
	{
		if (PatrolPoint_0 == null)
		{
			PatrolPoint_0 = BossPatrolPoint();
		}
		BotOwner_0.Mover.SetTargetMoveSpeed(0.7f);
		Vector3? vector = null;
		switch (BotOwner_0.BotFollower.BossToFollow.PatrollingData.Status)
		{
		case PatrolStatus.go:
			base.HaveProblems = !method_0();
			if (PatrolPoint_0.HaveLookSide)
			{
				vector = PatrolPoint_0.LookDir(0);
			}
			break;
		case PatrolStatus.pause:
			base.HaveProblems = !method_0();
			break;
		case PatrolStatus.stay:
			method_0();
			if (PatrolPoint_0.HaveLookSide)
			{
				vector = PatrolPoint_0.LookDir(0);
			}
			base.HaveProblems = false;
			break;
		}
		if (Float_2 < Time.time)
		{
			if (vector.HasValue)
			{
				Float_2 = Time.time + 1f;
				BotObserveDataClass.SetVectorToLook(vector.Value);
			}
			else
			{
				BotObserveDataClass.SetVector();
			}
		}
		BotObserveDataClass.Update();
	}

	[CanBeNull]
	public override PatrolPoint BossPatrolPoint()
	{
		if (BotOwner_0.BotFollower != null && BotOwner_0.BotFollower.BossToFollow != null)
		{
			PatrolPointContainer curPatrolPoint = BotOwner_0.BotFollower.BossToFollow.PatrollingData.CurPatrolPoint;
			if (curPatrolPoint != null)
			{
				int index = Mathf.Clamp(curPatrolPoint.TargetPoint.SubPointsCount - Int_0, 1, 99);
				PatrolPoint_0 = curPatrolPoint.TargetPoint.GetSubPoint(index);
				return PatrolPoint_0;
			}
		}
		return BotOwner_0.PatrollingData.CurPatrolPoint.TargetPoint;
	}
}
