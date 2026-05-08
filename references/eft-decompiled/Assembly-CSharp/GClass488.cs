using System;
using EFT;

public class GClass488 : BotFirstAidClass
{
	public GClass488(BotOwner owner, Action<bool> callback)
		: base(owner, callback)
	{
	}

	public override void FirstAidApplied()
	{
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			BotOwner_0.GetPlayer.ActiveHealthController.RestoreFullHealth();
		}
	}
}
