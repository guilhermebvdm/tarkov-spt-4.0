using System;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class GClass509 : GClass429
{
	[NonSerialized]
	public PatrolPointContainer PatrolPointContainer_0;

	[NonSerialized]
	public PatrolPointContainer PatrolPointContainer_1;

	[NonSerialized]
	public Vector3[] Vector3_0;

	[NonSerialized]
	public bool Bool_0;

	public GClass509(BotOwner owner)
		: base(owner)
	{
	}

	public void ComeToPoint(PatrolPointContainer point)
	{
		PatrolPointContainer_0 = point;
	}

	public void Activate()
	{
	}

	public void RefreshGoToPoint()
	{
		BotOwner_0.PatrollingData.GoToPoint();
	}

	public NavMeshPathStatus GoToPoint(PatrolPointContainer point)
	{
		Bool_0 = GClass856.IsTrue100(BotOwner_0.Settings.FileSettings.Patrol.SPRINT_BETWEEN_CACHED_POINTS);
		PatrolPointContainer_1 = point;
		NavMeshPathStatus result = BotOwner_0.GoToPoint(point.Position);
		if (Bool_0)
		{
			Bool_0 = BotOwner_0.Mover.DistDestination > 50f;
		}
		return result;
	}

	public void ManualUpdate()
	{
		if (Bool_0)
		{
			if (PatrolPointContainer_1 != null)
			{
				bool val = (PatrolPointContainer_1.Position - BotOwner_0.Position).sqrMagnitude > 2f;
				BotOwner_0.Sprint(val);
			}
		}
		else
		{
			BotOwner_0.Sprint(val: false);
		}
	}

	public void Dispose()
	{
	}
}
