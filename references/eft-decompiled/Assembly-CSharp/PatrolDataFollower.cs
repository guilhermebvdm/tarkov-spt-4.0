using System;
using EFT;
using UnityEngine;

public class PatrolDataFollower : GClass429
{
	[NonSerialized]
	public GClass563 FollowerAIBase;

	[NonSerialized]
	public GClass505 FollowerPlayerBase;

	[NonSerialized]
	public int Index;

	[NonSerialized]
	public Vector3? SafeTarget;

	[NonSerialized]
	public float EndStopTime;

	[field: NonSerialized]
	public bool IsInited { get; set; }

	[field: NonSerialized]
	public bool IsMainWay { get; }

	public bool HaveProblems
	{
		get
		{
			if (!IsInited)
			{
				return false;
			}
			return FollowerAIBase.HaveProblems;
		}
		set
		{
			FollowerAIBase.HaveProblems = value;
		}
	}

	public PatrolDataFollower(BotOwner owner, int index)
		: base(owner)
	{
		IsMainWay = true;
		Index = index;
	}

	public void Init(PatrolFollowerType followerType, GClass513 simple)
	{
		if (FollowerAIBase != null)
		{
			FollowerAIBase.Dispose();
		}
		FollowerAIBase = new GClass569(BotOwner_0, Index, simple);
		IsInited = true;
		method_0();
	}

	public void Init(PatrolFollowerType followerType, GClass510 bossPatrolMoveSimple)
	{
		FollowerAIBase?.Dispose();
		switch (followerType)
		{
		case PatrolFollowerType.Close:
			FollowerAIBase = new GClass564(BotOwner_0, Index);
			break;
		case PatrolFollowerType.CloseCover:
			FollowerAIBase = new GClass565(BotOwner_0, Index);
			break;
		case PatrolFollowerType.CloseCoverWide:
			FollowerAIBase = new GClass566(BotOwner_0, Index, bossPatrolMoveSimple);
			break;
		default:
			FollowerAIBase = new GClass568(BotOwner_0, Index);
			break;
		case PatrolFollowerType.CloseCoverWithStop:
			FollowerAIBase = new GClass567(BotOwner_0, Index);
			break;
		}
		IsInited = true;
		method_0();
	}

	public void InitPlayer(Player player)
	{
		if (FollowerAIBase != null)
		{
			FollowerAIBase.Dispose();
		}
		FollowerPlayerBase = new GClass505(player, BotOwner_0);
	}

	public void Activate()
	{
		if (FollowerAIBase != null)
		{
			FollowerAIBase.Activate();
		}
	}

	public bool SetTarget(GStruct25 trg, Vector3? lookSide)
	{
		if (!IsInited)
		{
			SafeTarget = trg.Position;
			return true;
		}
		return FollowerAIBase.SetTarget(trg, lookSide);
	}

	public void SetProblemsSomebody(bool trg)
	{
		FollowerAIBase.SomeBodyhaveProblems(trg);
	}

	public void StopFor(float period)
	{
		EndStopTime = Time.time + period;
	}

	public void ManualUpdate()
	{
		if (EndStopTime > Time.time)
		{
			BotOwner_0.StopMove();
		}
		else if (BotOwner_0.BotFollower.BossToFollow.IsAI)
		{
			FollowerAIBase.Update();
		}
		else
		{
			FollowerPlayerBase.Update();
		}
	}

	public void SetIndex(int index)
	{
		if (FollowerAIBase != null)
		{
			FollowerAIBase.SetIndex(index);
		}
	}

	public void SetFolloweType(bool front)
	{
		FollowerAIBase.SetFolloweType(front);
	}

	public void SetCloseToTarget(bool closeToPoint)
	{
		FollowerAIBase.SetClseToTarget(closeToPoint);
	}

	public void OnDrawGizmosSelected()
	{
		if (FollowerAIBase != null)
		{
			FollowerAIBase.OnDrawSelectedGizmos();
		}
	}

	public void method_0()
	{
		if (SafeTarget.HasValue)
		{
			FollowerAIBase.SetTarget(new GStruct25(SafeTarget.Value), null);
			SafeTarget = null;
		}
	}

	public void Dispose()
	{
		if (IsInited)
		{
			FollowerAIBase?.Dispose();
		}
	}
}
