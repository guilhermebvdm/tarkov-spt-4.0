using System.Collections.Generic;
using EFT;

public class FollowPlayerEvent : GClass670
{
	public override List<int> LayersToActivate => new List<int> { 504 };

	public override List<int> LayersToDeActivate => new List<int>();

	public FollowPlayerEvent(BotsClass botsList)
		: base(botsList)
	{
	}

	public override void Activate()
	{
		BotsClass.OnBotAdd += method_1;
		base.Activate();
		if (LocalBotSettingsProviderClass.Core.START_ACTIVE_FOLLOW_PLAYER_EVENT)
		{
			Start("OnStart");
		}
	}

	public void method_1(BotOwner bot)
	{
		if (Bool_0)
		{
			method_0(bot);
		}
	}

	public override void Dispose()
	{
		BotsClass.OnBotAdd -= method_1;
		base.Dispose();
	}

	public override bool CanActivate(BotOwner bot)
	{
		return bot.Settings.FileSettings.Mind.ACTIVE_FOLLOW_PLAYER_EVENTS;
	}

	public void DebugActivate()
	{
		Start("debug");
	}
}
