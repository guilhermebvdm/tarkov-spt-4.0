using EFT;
using UnityEngine;

public class GClass594 : BotRequest
{
	public override EBotRequestMode RequestMode => EBotRequestMode.Fight;

	public GClass594(Player requester)
		: base(requester, BotRequestType.hide)
	{
	}

	public override void PlayerDestroy(Player player)
	{
	}

	public override void Take(BotOwner executor)
	{
		base.Take(executor);
		SetEndTimeOnTake(LocalBotSettingsProviderClass.Core.HOLD_REQUEST_TIME_SEC);
	}

	public override bool CanProceed()
	{
		if (Executor.Memory.GoalEnemy != null)
		{
			if (Executor.Memory.GoalEnemy.IsVisible)
			{
				return false;
			}
			if (Time.time - Executor.Memory.GoalEnemy.TimeLastSeen < 10f)
			{
				return false;
			}
			return true;
		}
		return true;
	}

	public override bool CanRequest(BotOwner requester)
	{
		return true;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		return ContinueNodeLogic;
	}
}
