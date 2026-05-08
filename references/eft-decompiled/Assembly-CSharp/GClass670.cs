using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;

public abstract class GClass670
{
	[CompilerGenerated]
	public class Class336
	{
		public GClass670 gclass670_0;

		public BotOwner bot;

		public void method_0(BaseBrain state)
		{
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			gclass670_0.BotEventActive(bot);
			bot.Brain.OnSetStrategy -= method_0;
		}
	}

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public BotsClass BotsClass;

	public const int FORCE_ATTACK_EVENT = 501;

	public const int PURSUIT_LAYER = 502;

	public const int KHOROVOD_CHRISTMAS_EVENT_LAYER = 503;

	public const int FOLLOW_PLAYER_LAYER = 504;

	public const int HALLOWEEN_HIDE_LAYER = 506;

	public const int GO_TO_GENERATOR_EVENT_LAYER = 507;

	public abstract List<int> LayersToActivate { get; }

	public abstract List<int> LayersToDeActivate { get; }

	public GClass670(BotsClass botsList)
	{
		BotsClass = botsList;
	}

	public virtual void Activate()
	{
	}

	public virtual void Dispose()
	{
	}

	public virtual bool Start(string cause)
	{
		if (Bool_0)
		{
			return false;
		}
		Bool_0 = true;
		foreach (BotOwner botOwner in BotsClass.BotOwners)
		{
			method_0(botOwner);
		}
		return true;
	}

	public void method_0(BotOwner bot)
	{
		Class336 CS_0024_003C_003E8__locals11 = new Class336();
		CS_0024_003C_003E8__locals11.gclass670_0 = this;
		CS_0024_003C_003E8__locals11.bot = bot;
		if (!CanActivate(CS_0024_003C_003E8__locals11.bot) || !CS_0024_003C_003E8__locals11.bot.Settings.FileSettings.Mind.ACTIVE_FORCE_ATTACK_EVENTS)
		{
			return;
		}
		if (CS_0024_003C_003E8__locals11.bot.BotState == EBotState.Active)
		{
			BotEventActive(CS_0024_003C_003E8__locals11.bot);
			return;
		}
		CS_0024_003C_003E8__locals11.bot.Brain.OnSetStrategy += delegate
		{
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			CS_0024_003C_003E8__locals11.gclass670_0.BotEventActive(CS_0024_003C_003E8__locals11.bot);
			CS_0024_003C_003E8__locals11.bot.Brain.OnSetStrategy -= CS_0024_003C_003E8__locals11.method_0;
		};
	}

	public abstract bool CanActivate(BotOwner bot);

	public virtual void BotEventActive(BotOwner bot)
	{
		bot.Brain.BaseBrain.ActivateLayers(LayersToActivate);
	}
}
