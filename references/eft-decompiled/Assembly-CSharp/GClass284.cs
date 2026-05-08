using System;
using EFT;

public class GClass284 : GClass177<GClass26>
{
	[NonSerialized]
	public GClass186 Gclass186_0;

	public GClass284(BotOwner bot)
		: base(bot)
	{
		Gclass186_0 = new GClass186(bot);
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if (!(BotOwner_0.WeaponManager.Stationary.CurLink == null))
		{
			if (!BotOwner_0.WeaponManager.Stationary.Taken)
			{
				BotOwner_0.WeaponManager.Stationary.Take();
			}
			if (BotOwner_0.WeaponManager.Stationary.CheckAmmonProcess())
			{
				BotOwner_0.StopMove();
				Gclass186_0.UpdateNodeByBrain(data as GClass27);
			}
		}
	}
}
