using System;
using System.Runtime.CompilerServices;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public abstract class GClass563
{
	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public BotObserveDataClass BotObserveDataClass;

	[NonSerialized]
	public BotOwner BotOwner_0;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public GStruct25 Gstruct25_0;

	[NonSerialized]
	public Vector3? Nullable_0;

	[NonSerialized]
	public PatrolPoint PatrolPoint_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_3;

	public bool HaveProblems
	{
		[CompilerGenerated]
		get
		{
			return Bool_3;
		}
		[CompilerGenerated]
		set
		{
			Bool_3 = value;
		}
	}

	public GClass563(BotOwner owner, int index)
	{
		BotOwner_0 = owner;
		Int_0 = index;
		BotObserveDataClass = BotOwner_0.Memory.botObserveData;
	}

	public virtual void Activate()
	{
	}

	public abstract bool SetTarget(GStruct25 trg, Vector3? lookSide);

	public abstract void Update();

	public void SetIndex(int index)
	{
		Int_0 = index;
	}

	public void SomeBodyhaveProblems(bool someBodyhaveProblems)
	{
		Bool_2 = someBodyhaveProblems;
	}

	public void SetFolloweType(bool front)
	{
		Bool_0 = front;
	}

	public void SetClseToTarget(bool closeToPoint)
	{
		Bool_1 = closeToPoint;
	}

	public virtual void OnDrawSelectedGizmos()
	{
	}

	public bool method_0()
	{
		PatrolPoint patrolPoint = BossPatrolPoint();
		if (patrolPoint != null)
		{
			if ((patrolPoint.Position - BotOwner_0.Position).sqrMagnitude < 1f)
			{
				BotOwner_0.StopMove();
				return true;
			}
			if (Float_1 < Time.time)
			{
				Float_1 = Time.time + 1f;
				BotOwner_0.GoToPoint(patrolPoint.Position);
			}
		}
		return false;
	}

	public GStruct25 method_1(out bool extraTarget)
	{
		extraTarget = false;
		if (!Bool_1 && Gstruct25_0.HasValue)
		{
			if (GClass855.IsOnNavMesh(Gstruct25_0.Position, 0.2f))
			{
				return new GStruct25(Gstruct25_0.Position);
			}
			extraTarget = true;
			return new GStruct25(BotOwner_0.BotFollower.BossToFollow.Position);
		}
		PatrolPoint patrolPoint = BossPatrolPoint();
		if (patrolPoint != null)
		{
			return new GStruct25(patrolPoint.Position, patrolPoint.ShallSit, patrolPoint.PointWithLookSides);
		}
		return new GStruct25(BotOwner_0.PatrollingData.CurTargetPoint);
	}

	[CanBeNull]
	public virtual PatrolPoint BossPatrolPoint()
	{
		if (BotOwner_0.BotFollower != null && BotOwner_0.BotFollower.BossToFollow != null)
		{
			PatrolPointContainer patrolPointContainer = BotOwner_0.BotFollower.BossToFollow.PatrollingData?.CurPatrolPoint;
			if (patrolPointContainer != null)
			{
				PatrolPoint_0 = patrolPointContainer.TargetPoint.GetSubPoint(Int_0);
				return PatrolPoint_0;
			}
		}
		return BotOwner_0.PatrollingData.CurPatrolPoint?.TargetPoint;
	}

	public virtual void Dispose()
	{
	}
}
