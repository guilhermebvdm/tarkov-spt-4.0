using System;
using EFT;
using UnityEngine;

public class GClass212(BotOwner bot) : GClass200<GClass31>(bot)
{
	public bool UseBaseLook = true;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	public override void UpdateNodeByBrain(GClass31 data)
	{
		method_0();
		if (Bool_1)
		{
			Bool_1 = false;
			return;
		}
		BotOwner_0.Sprint(val: false);
		if (Float_0 < Time.time)
		{
			method_6(data);
		}
		if (UseBaseLook)
		{
			BotOwner_0.LookData.SetLookPointByHearing();
		}
		if (!Bool_0)
		{
			BotOwner_0.SetPose(1f);
			BotOwner_0.SetTargetMoveSpeed(1f);
			CustomNavigationPoint customNavigationPoint = ((data == null) ? BotOwner_0.Memory.CurCustomCoverPoint : data.Point);
			if (customNavigationPoint != null)
			{
				if (customNavigationPoint.IsFreeById(BotOwner_0.Id))
				{
					BotOwner_0.GoToPoint(customNavigationPoint);
					Bool_0 = true;
				}
				else
				{
					Float_0 = 0f;
				}
			}
		}
		if (data != null)
		{
			BotOwner_0.Memory.SetCoverPoints(data.Point);
		}
		if (BotOwner_0.Mover.IsComeTo(BotOwner_0.Settings.FileSettings.Move.REACH_DIST, onCover: true, data?.Point))
		{
			method_5();
		}
	}

	public void method_5()
	{
		BotOwner_0.Memory.ComeToPoint();
		BotOwner_0.SetPose(0.01f);
		BotOwner_0.StopMove();
	}

	public void method_6(GClass31 data)
	{
		BotOwner_0.BotLight.TurnOff();
		Float_0 = Time.time + BotOwner_0.Settings.FileSettings.Move.UPDATE_TIME_RECAL_WAY;
		Bool_0 = false;
		CustomNavigationPoint customNavigationPoint = BotOwner_0.BotAttackManager.PointToGo();
		if (data != null && data.Point.IsFreeById(BotOwner_0.Id))
		{
			BotOwner_0.GoToPoint(data.Point);
		}
		else if (customNavigationPoint != null)
		{
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(customNavigationPoint);
			BotOwner_0.GoToPoint(customNavigationPoint);
		}
	}
}
