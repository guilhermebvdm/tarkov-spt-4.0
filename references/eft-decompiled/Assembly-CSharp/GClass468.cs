using EFT;
using JetBrains.Annotations;

public abstract class GClass468 : GClass429
{
	public static BotLogicDecision HoldOrCover(BotOwner bot)
	{
		if (bot.Memory.IsInCover && !bot.Memory.CurCustomCoverPoint.IsSpotted)
		{
			return BotLogicDecision.holdPosition;
		}
		return BotLogicDecision.goToCoverPoint;
	}

	public GClass468([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	public BotLogicDecision HoldOrCover()
	{
		return HoldOrCover(BotOwner_0);
	}
}
