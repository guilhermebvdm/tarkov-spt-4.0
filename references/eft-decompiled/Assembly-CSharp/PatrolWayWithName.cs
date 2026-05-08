using EFT;

public class PatrolWayWithName : PatrolWay
{
	public string NameId;

	public override bool Suitable(BotOwner bot, IGetProfileData data)
	{
		return bot.PatrollingData.PointChooser.IsWaySuitableByNameId(NameId);
	}

	public override string SuitableData()
	{
		return "NameId:" + NameId;
	}
}
