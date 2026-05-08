using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;

public class MiniAssaultBotsGroup
{
	[CompilerGenerated]
	public class Class276
	{
		public MiniAssaultBotsGroup miniAssaultBotsGroup_0;

		public BotOwner bot;

		public void method_0(Player player)
		{
			miniAssaultBotsGroup_0.BotForAssault.Remove(bot);
		}
	}

	public EBotAssaultAreaStatus Status;

	[NonSerialized]
	public Dictionary<BotOwner, EBotAssaultAreaStatus> BotForAssault = new Dictionary<BotOwner, EBotAssaultAreaStatus>();

	[field: NonSerialized]
	public AIDangerArea AreaToAssaut { get; set; }

	public MiniAssaultBotsGroup(AIDangerArea area, List<BotOwner> botForAssault)
	{
		AreaToAssaut = area;
		foreach (BotOwner item in botForAssault)
		{
			BotForAssault.Add(item, EBotAssaultAreaStatus.shallGoClose);
		}
		foreach (KeyValuePair<BotOwner, EBotAssaultAreaStatus> item2 in BotForAssault)
		{
			item2.Key.AssaultDangerArea.SetActivaArea(this);
			method_1(item2.Key);
		}
	}

	public void BotStartAssault(BotOwner bot)
	{
		if (method_0(bot, EBotAssaultAreaStatus.completed))
		{
			bot.BotsGroup.BotZone.ZoneDangerAreas.Complete(AreaToAssaut);
		}
	}

	public void BotIsReady(BotOwner bot)
	{
		if (method_0(bot, EBotAssaultAreaStatus.assault))
		{
			Status = EBotAssaultAreaStatus.assault;
		}
	}

	public bool method_0(BotOwner bot, EBotAssaultAreaStatus status)
	{
		if (BotForAssault.ContainsKey(bot))
		{
			BotForAssault[bot] = status;
		}
		bool result = true;
		foreach (KeyValuePair<BotOwner, EBotAssaultAreaStatus> item in BotForAssault)
		{
			if (item.Value != status)
			{
				result = false;
			}
		}
		return result;
	}

	public void method_1(BotOwner bot)
	{
		bot.GetPlayer.OnPlayerDeadOrUnspawn += delegate
		{
			BotForAssault.Remove(bot);
		};
	}

	public void Dispose()
	{
		AreaToAssaut = null;
		BotForAssault.Clear();
	}
}
