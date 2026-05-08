using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;

public class BotBTRService
{
	[NonSerialized]
	public BotsController BotsController;

	[NonSerialized]
	public List<Player> FriendlyPlayersByBtrSupport = new List<Player>();

	[NonSerialized]
	public List<Player> Betrayers = new List<Player>();

	[NonSerialized]
	public BotOwner BtrOwner;

	public BotBTRService(BotsController botsController)
	{
		BotsController = botsController;
	}

	public void Reset()
	{
		FriendlyPlayersByBtrSupport.Clear();
		Betrayers.Clear();
		OnBTRSupportStop();
	}

	public void RegisterBotBTR(BotOwner owner)
	{
		BtrOwner = owner;
		BotsController.BotSpawner.OnBotCreated += method_0;
	}

	public void OnBTRSupportPurchased(List<Player> passengers)
	{
		List<Player> allAlivePlayersList = Singleton<GameWorld>.Instance.AllAlivePlayersList;
		foreach (Player passenger in passengers)
		{
			method_5(passenger);
		}
		foreach (Player item in allAlivePlayersList)
		{
			foreach (Player passenger2 in passengers)
			{
				if (!string.IsNullOrEmpty(item.GroupId) && item.GroupId == passenger2.GroupId)
				{
					method_5(item);
				}
			}
		}
		method_3();
	}

	public void method_0(BotOwner botOwner)
	{
		method_1(botOwner.GetPlayer);
	}

	public void method_1(IPlayer player)
	{
		if (FriendlyPlayersByBtrSupport.Count > 0 && player != BtrOwner.GetPlayer)
		{
			BtrOwner.BotsGroup.AddEnemy(player, EBotEnemyCause.serviceBTR);
		}
	}

	public void method_2(IPlayer player)
	{
		BtrOwner.BotsGroup.RemoveEnemy(player);
		BtrOwner.BotsGroup.AddNeutral(player);
	}

	public void method_3()
	{
		foreach (Player allAlivePlayers in Singleton<GameWorld>.Instance.AllAlivePlayersList)
		{
			if (!FriendlyPlayersByBtrSupport.Contains(allAlivePlayers))
			{
				method_1(allAlivePlayers);
			}
		}
	}

	public void method_4()
	{
		foreach (Player allAlivePlayers in Singleton<GameWorld>.Instance.AllAlivePlayersList)
		{
			if (!Betrayers.Contains(allAlivePlayers))
			{
				method_2(allAlivePlayers);
			}
		}
	}

	public void method_5(Player player)
	{
		if (!Betrayers.Contains(player))
		{
			FriendlyPlayersByBtrSupport.Add(player);
		}
	}

	public void method_6(Player player)
	{
		FriendlyPlayersByBtrSupport.Remove(player);
		Betrayers.Add(player);
		BtrOwner.BotsGroup.AddEnemy(player, EBotEnemyCause.attackBTR);
	}

	public void OnBTRSupportStop()
	{
		FriendlyPlayersByBtrSupport.Clear();
		method_4();
	}

	public void OnBTRSupportInterruptedByBetrayer(IPlayer betrayer)
	{
		foreach (Player allAlivePlayers in Singleton<GameWorld>.Instance.AllAlivePlayersList)
		{
			if ((!string.IsNullOrEmpty(allAlivePlayers.GroupId) && allAlivePlayers.GroupId == betrayer.GroupId) || betrayer.Id == allAlivePlayers.Id)
			{
				method_6(allAlivePlayers);
			}
		}
	}

	public void Dispose()
	{
		FriendlyPlayersByBtrSupport.Clear();
		Betrayers.Clear();
		BotsController.BotSpawner.OnBotCreated -= method_0;
	}
}
