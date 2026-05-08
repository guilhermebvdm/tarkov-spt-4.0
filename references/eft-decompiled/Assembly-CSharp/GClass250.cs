using System;
using EFT;

public class GClass250(BotOwner bot) : GClass200<GClass26>(bot)
{
	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public GClass395 Gclass395_0 = new GClass395();

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		BotOwner_0.SetPose(1f);
		BotOwner_0.BotLay.GetUp(withCheck: true);
		if (BotOwner_0.PatrollingData.CurPatrolPoint == null)
		{
			BotOwner_0.PatrollingData.FindNextPoint(withSetting: true, withoutNext: false);
			return;
		}
		BotOwner_0.PatrollingData.ManualUpdate(Bool_0);
		BotOwner_0.PatrollingData.MoveUpdate();
		Gclass395_0.Update(BotOwner_0);
	}
}
