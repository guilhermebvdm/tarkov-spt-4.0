using System;
using EFT;

public class GClass289 : GClass177<GClass26>
{
	[NonSerialized]
	public GClass251 Gclass251_0;

	[NonSerialized]
	public GClass189 Gclass189_0;

	[NonSerialized]
	public GClass290 Gclass290_0;

	public WarnPlayerRequest WarnPlayerRequest_0 => BotOwner_0.WarnData.WarnPlayerRequest;

	public GClass289(BotOwner bot)
		: base(bot)
	{
		Gclass251_0 = new GClass251(bot);
		Gclass189_0 = new GClass189(bot);
		Gclass290_0 = new GClass290(bot);
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		WarnPlayerRequest warnPlayerRequest_ = WarnPlayerRequest_0;
		if (warnPlayerRequest_ == null)
		{
			return;
		}
		switch (warnPlayerRequest_.StateWarnPlayer)
		{
		case StateWarnPlayer.goTo:
			if (warnPlayerRequest_.ShouldWarnFromCurrentPosition())
			{
				warnPlayerRequest_.SetWarnStarted();
				warnPlayerRequest_.StateWarnPlayer = StateWarnPlayer.stay;
			}
			else
			{
				Gclass251_0.UpdateNodeByBrain(data);
			}
			break;
		case StateWarnPlayer.say1:
			warnPlayerRequest_.SetWarnStarted();
			Gclass290_0.UpdateNodeByBrain(data);
			warnPlayerRequest_.UpdateSay(onlyRoger: true);
			break;
		case StateWarnPlayer.shoot:
			warnPlayerRequest_.SetWarnStarted();
			Gclass189_0.UpdateNodeByBrain(data as GClass27);
			warnPlayerRequest_.UpdateSay(onlyRoger: true);
			break;
		case StateWarnPlayer.stay:
			if (WarnPlayerRequest_0.ShallEndStay())
			{
				WarnPlayerRequest_0.NextState();
			}
			if (Gclass189_0.IsReady)
			{
				warnPlayerRequest_.UpdateSay();
			}
			warnPlayerRequest_.UpdateSay(onlyRoger: true);
			break;
		case StateWarnPlayer.wait:
			break;
		}
	}
}
