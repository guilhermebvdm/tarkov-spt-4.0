using System;
using EFT;
using UnityEngine;

public class BotWarnData : GClass429
{
	[NonSerialized]
	public WarnPlayerRequest WarnPlayerRequest_1;

	public WarnPlayerRequest WarnPlayerRequest => WarnPlayerRequest_1;

	public BotWarnData(BotOwner owner)
		: base(owner)
	{
	}

	public bool ThrowWarnPlayerRequest(Player playerToWarn, GClass428 dataWarn)
	{
		if (BotOwner_0.BotsGroup.Enemies.ContainsKey(playerToWarn))
		{
			return false;
		}
		WarnPlayerRequest_1 = new WarnPlayerRequest(BotOwner_0, playerToWarn, dataWarn);
		return true;
	}

	public void ManualUpdate()
	{
		if (WarnPlayerRequest_1 != null && (WarnPlayerRequest_1.EndTime < Time.time || BotOwner_0.BotsGroup.IsEnemy(WarnPlayerRequest_1.playerToWarn)))
		{
			WarnPlayerRequest_1?.Dispose();
			WarnPlayerRequest_1 = null;
		}
	}

	public void StopWarn()
	{
		WarnPlayerRequest_1?.Dispose();
		WarnPlayerRequest_1 = null;
	}

	public void Dispose()
	{
		StopWarn();
	}
}
