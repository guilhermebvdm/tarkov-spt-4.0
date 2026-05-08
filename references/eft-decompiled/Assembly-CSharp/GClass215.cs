using System;
using EFT;
using UnityEngine;

public class GClass215(BotOwner bot) : GClass200<GClass26>(bot)
{
	public bool UseBaseLook = true;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public GClass592 Gclass592_0;

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		Gclass592_0 = BotOwner_0.BotRequestController.CurRequest as GClass592;
		if (Gclass592_0 != null)
		{
			BotOwner_0.Sprint(val: false);
			if (Float_0 < Time.time)
			{
				method_6();
			}
			if (UseBaseLook)
			{
				BotOwner_0.LookData.SetLookPointByHearing();
			}
			BotOwner_0.SetPose(1f);
			BotOwner_0.SetTargetMoveSpeed(1f);
			if (BotOwner_0.Mover.IsComeTo(BotOwner_0.Settings.FileSettings.Move.REACH_DIST, onCover: true))
			{
				method_5();
			}
		}
	}

	public void method_5()
	{
		BotOwner_0.Memory.ComeToPoint();
		BotOwner_0.SetPose(0.01f);
		BotOwner_0.StopMove();
	}

	public void method_6()
	{
		Float_0 = Time.time + BotOwner_0.Settings.FileSettings.Move.UPDATE_TIME_RECAL_WAY;
		BotOwner_0.BotLight.TurnOff();
		if (Gclass592_0.IsExecuterFar())
		{
			Vector3 position = Gclass592_0.Requester.Position;
			BotOwner_0.BotLight.TurnOff();
			CustomNavigationPoint freeClosePoint = BotOwner_0.Covers.GetFreeClosePoint(position, 1f);
			if ((position - freeClosePoint.Position).sqrMagnitude < 64f)
			{
				BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(freeClosePoint);
				BotOwner_0.GoToPoint(freeClosePoint);
			}
			else
			{
				Vector3 vector = GClass855.NormalizeFastSelf(position - BotOwner_0.Position);
				Vector3 targetPoint = position - vector * 2f;
				BotOwner_0.MoveToEnemyData.TryMoveToEnemy(targetPoint);
			}
		}
	}
}
