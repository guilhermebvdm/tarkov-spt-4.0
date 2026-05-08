using EFT;

public class AIPlaceInfoLogicZryachiy : AIPlaceInfoLogic
{
	public bool CloseZone;

	public bool AttackPossible;

	public bool PermanentVision;

	public bool ControllableZone;

	public bool FollowersPositions;

	public bool BossPositions;

	public bool LookAtCenter;

	private int int_0 = -1;

	public bool CanTakeBy(BotOwner bot)
	{
		if (bot.Id != int_0)
		{
			return int_0 <= 0;
		}
		return true;
	}

	public void TakeBy(BotOwner bot)
	{
		int_0 = bot.Id;
	}

	public void ReleaseBy(BotOwner bot)
	{
		if (int_0 == bot.Id)
		{
			int_0 = -1;
		}
	}
}
