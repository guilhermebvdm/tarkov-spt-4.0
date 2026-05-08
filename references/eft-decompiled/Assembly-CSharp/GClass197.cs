using EFT;

public class GClass197 : GClass177<GClass26>
{
	public GClass197(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		BotOwner_0.SetPose(0f);
		BotOwner_0.Sprint(val: false);
		if (!BotOwner_0.Medecine.Using)
		{
			if (BotOwner_0.WeaponManager.Reload.Reloading)
			{
				BotOwner_0.WeaponManager.Reload.TryStopReload();
			}
			BotOwner_0.LookData.SetLookPointByHearing();
			bool flag;
			if ((flag = BotOwner_0.Medecine.FirstAid.ShallStartUse()) && BotOwner_0.Medecine.FirstAid.IsBleeding)
			{
				BotOwner_0.Medecine.FirstAid.TryApplyToCurrentPart();
			}
			else if (BotOwner_0.Medecine.SurgicalKit.ShallStartUse())
			{
				BotOwner_0.Medecine.SurgicalKit.ApplyToCurrentPart();
			}
			else if (flag)
			{
				BotOwner_0.Medecine.FirstAid.TryApplyToCurrentPart();
			}
		}
	}
}
