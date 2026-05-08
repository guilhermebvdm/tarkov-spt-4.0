using System;
using EFT;
using UnityEngine;

public class GClass251 : GClass200<GClass26>
{
	[NonSerialized]
	public const float Float_0 = 15f;

	[NonSerialized]
	public const float Float_1 = 3f;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public float Float_3;

	public WarnPlayerRequest WarnPlayerRequest_0 => BotOwner_0.WarnData.WarnPlayerRequest;

	public GClass251(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if (BotOwner_0.CanSprintPlayer)
		{
			method_6();
		}
		else
		{
			method_5();
		}
	}

	public void method_5()
	{
		method_0();
		method_7();
		BotOwner_0.Mover.Sprint(val: false);
		if (method_10())
		{
			BotOwner_0.Mover.IsComeTo(3f, onCover: false);
		}
		else
		{
			method_8();
		}
	}

	public void method_6()
	{
		method_0();
		BotOwner_0.SetPose(1f);
		bool num = BotOwner_0.MoveToEnemyData.TryMoveToEnemy(WarnPlayerRequest_0.playerToWarn.Position);
		if (!Bool_0)
		{
			Bool_0 = true;
			Float_3 = Time.time;
		}
		if (num)
		{
			if (BotOwner_0.Mover.IsComeTo(3f, onCover: false))
			{
				method_8();
			}
		}
		else if (Time.time - Float_3 > 15f)
		{
			WarnPlayerRequest_0.Dispose();
		}
	}

	public void method_7()
	{
		BotOwner_0.Steering.LookToPoint(WarnPlayerRequest_0.playerToWarn.Position);
	}

	public void method_8()
	{
		method_9();
		BotOwner_0.StopMove();
	}

	public void method_9()
	{
		WarnPlayerRequest_0.NextState();
	}

	public bool method_10()
	{
		if (Float_2 < Time.time)
		{
			Float_2 = Time.time + BotOwner_0.Settings.FileSettings.Move.UPDATE_TIME_RECAL_WAY;
			BotOwner_0.Mover.SetTargetMoveSpeed(1f);
			BotOwner_0.Mover.SetPose(1f);
			if (BotOwner_0.MoveToEnemyData.TryMoveToEnemy(WarnPlayerRequest_0.playerToWarn.Position))
			{
				return false;
			}
		}
		return true;
	}
}
