using System;
using EFT;

public class BotGameEventsData : GClass429
{
	[field: NonSerialized]
	public KhorovodBotGameEvent KhorovodGameEvent { get; }

	[field: NonSerialized]
	public BotHalloweenWithZombiesGameEvent HalloweenGameEvent { get; }

	public BotGameEventsData(BotOwner owner)
		: base(owner)
	{
		KhorovodGameEvent = new KhorovodBotGameEvent(owner);
		HalloweenGameEvent = new BotHalloweenWithZombiesGameEvent(owner);
	}
}
