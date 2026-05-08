using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass221 : GClass200<GClass26>
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public GClass178 Gclass178_0;

	public GClass597 GClass597_0 => BotOwner_0.BotRequestController.CurRequest as GClass597;

	public GClass221(BotOwner bot)
		: base(bot)
	{
		Gclass178_0 = new GClass183(bot);
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		BotOwner_0.Mover.Sprint(val: false);
		if (Float_0 < Time.time)
		{
			method_6();
		}
		BotOwner_0.LookData.SetLookPointByHearing();
		if (!BotOwner_0.Mover.IsComeTo(BotOwner_0.Settings.FileSettings.Move.REACH_DIST, onCover: true))
		{
			if (GClass597_0.CanShootNow())
			{
				BotOwner_0.StopMove();
				Gclass178_0.UpdateNodeByBrain(data as GClass27);
			}
			else
			{
				Gclass178_0.UpdateNodeByBrain(data as GClass27);
			}
		}
		else
		{
			method_5();
		}
	}

	public void method_5()
	{
		BotOwner_0.Memory.ComeToPoint();
		BotOwner_0.StopMove();
	}

	public void method_6()
	{
		Float_0 = Time.time + BotOwner_0.Settings.FileSettings.Move.UPDATE_TIME_RECAL_WAY;
		ShootPointClass shootPos = GClass597_0.GetShootPos();
		BotOwner_0.BotAttackManager.TryPointGetting(withShoot: true, CoverSearchType.shoot_toCover_toBot_Distances, shootPos, LocalBotSettingsProviderClass.Core.START_DIST_TO_COV, delegate(CustomNavigationPoint point)
		{
			BotOwner_0.Mover.SetTargetMoveSpeed(1f);
			BotOwner_0.GoToPoint(point);
		}, delegate
		{
			BotOwner_0.BotTalk.TrySay(EPhraseTrigger.BadWork);
			BotOwner_0.BotRequestController.DoNotAcceptRequests(GClass597_0);
			GClass597_0.Dispose();
		});
	}

	[CompilerGenerated]
	public void method_7(CustomNavigationPoint point)
	{
		BotOwner_0.Mover.SetTargetMoveSpeed(1f);
		BotOwner_0.GoToPoint(point);
	}

	[CompilerGenerated]
	public void method_8()
	{
		BotOwner_0.BotTalk.TrySay(EPhraseTrigger.BadWork);
		BotOwner_0.BotRequestController.DoNotAcceptRequests(GClass597_0);
		GClass597_0.Dispose();
	}
}
