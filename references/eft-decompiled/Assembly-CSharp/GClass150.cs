using EFT;

public abstract class GClass150 : BaseLogicLayerSimpleAbstractClass
{
	public static int? GetPlaceId(BotOwner owner)
	{
		if (owner.BotFollower.HaveBoss && owner.BotFollower.BossToFollow.IsAlive)
		{
			BotOwner botOwner = owner.BotFollower.BossToFollow.Player().AIData.BotOwner;
			if (botOwner.Memory.CurCustomCoverPoint != null)
			{
				return botOwner.Memory.CurCustomCoverPoint.PlaceId;
			}
			return botOwner.AIData.PlaceInfo?.AreaId;
		}
		if (owner.AIData.PlaceInfo != null)
		{
			return owner.AIData.PlaceInfo.AreaId;
		}
		return null;
	}

	public static void UpdatePlaceInfo(BotOwner owner, CoverSearchData data)
	{
		if (owner.BotFollower.HaveBoss)
		{
			BotOwner botOwner = owner.BotFollower.BossToFollow.Player().AIData.BotOwner;
			int? placeInfo = ((botOwner.Memory.CurCustomCoverPoint == null) ? new int?(botOwner.AIData.PlaceInfo.AreaId) : new int?(botOwner.Memory.CurCustomCoverPoint.PlaceId));
			data.PlaceInfo = placeInfo;
		}
		else if (owner.AIData.PlaceInfo != null)
		{
			data.PlaceInfo = owner.AIData.PlaceInfo.AreaId;
		}
	}

	public GClass150(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}
}
