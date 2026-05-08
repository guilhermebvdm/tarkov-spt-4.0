using System;
using EFT;
using UnityEngine;

public class GClass515 : GClass429
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public bool Bool_0;

	public GClass515(BotOwner owner)
		: base(owner)
	{
	}

	public bool Update()
	{
		if (Float_0 < Time.time)
		{
			method_1();
		}
		BotOwner_0.SetPose(1f);
		BotOwner_0.SetTargetMoveSpeed(0.6f);
		BotOwner_0.LookData.SetLookPointByHearing();
		bool flag = false;
		if (!Bool_0 && BotOwner_0.Settings.FileSettings.Patrol.USE_PATROL_POINT_ACTION_MOVE_BY_RESERVE_WAY)
		{
			flag = true;
			Vector3 goTo = BotOwner_0.PatrollingData.CurPatrolPoint.ActionData.GoTo;
			BotOwner_0.GoToPoint(goTo);
			Bool_0 = true;
		}
		if (flag)
		{
			return false;
		}
		if (!BotOwner_0.Mover.IsComeTo(BotOwner_0.Settings.FileSettings.Move.REACH_DIST, onCover: false))
		{
			return false;
		}
		method_0();
		return true;
	}

	public void method_0()
	{
		BotOwner_0.PatrollingData.ComeToPoint();
		BotOwner_0.StopMove();
	}

	public void method_1()
	{
		Float_0 = Time.time + BotOwner_0.Settings.FileSettings.Move.UPDATE_TIME_RECAL_WAY;
		Bool_0 = false;
	}
}
