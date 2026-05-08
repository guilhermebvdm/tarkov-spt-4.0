using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;

public class BotLighthouseKeeperServices
{
	[NonSerialized]
	public const float CHECK_FRIENDLY_PERIOD = 5f;

	[NonSerialized]
	public float NextCheckTime;

	[NonSerialized]
	public BotsController BotsController;

	[NonSerialized]
	public List<BotLighthouseKeeperFriendlyExUsecs> FriendlyExUsecs_1 = new List<BotLighthouseKeeperFriendlyExUsecs>();

	[NonSerialized]
	public List<BotLighthouseKeeperFriendlyZryachiy> FriendlyZryachiy_1 = new List<BotLighthouseKeeperFriendlyZryachiy>();

	[NonSerialized]
	public List<int> MarkedPlayers_1 = new List<int>();

	public List<BotLighthouseKeeperFriendlyExUsecs> FriendlyExUsecs => FriendlyExUsecs_1;

	public List<BotLighthouseKeeperFriendlyZryachiy> FriendlyZryachiy => FriendlyZryachiy_1;

	public List<int> MarkedPlayers => MarkedPlayers_1;

	public BotLighthouseKeeperServices(BotsController botsController)
	{
		BotsController = botsController;
	}

	public bool IsPlayerExUsecFriendly(IPlayer player)
	{
		foreach (BotLighthouseKeeperFriendlyExUsecs item in FriendlyExUsecs_1)
		{
			if (string.IsNullOrEmpty(item.PlayerGroupId) || !(item.PlayerGroupId == player.GroupId))
			{
				if (item.PlayerId == player.Id)
				{
					return true;
				}
				continue;
			}
			return true;
		}
		return false;
	}

	public bool IsPlayerZryachiyFriendly(IPlayer player)
	{
		foreach (BotLighthouseKeeperFriendlyZryachiy item in FriendlyZryachiy_1)
		{
			if (string.IsNullOrEmpty(item.PlayerGroupId) || !(item.PlayerGroupId == player.GroupId))
			{
				if (item.PlayerId == player.Id)
				{
					return true;
				}
				continue;
			}
			return true;
		}
		return false;
	}

	public bool ShallWarn(BotOwner owner, Player playerToWarn)
	{
		if (IsExUsecsServiceActive(playerToWarn) && BotSettingsRepoClass.IsExUsec(owner.Profile.Info.Settings.Role))
		{
			return true;
		}
		if (IsPlayerZryachiyFriendly(playerToWarn) && (owner.Profile.Info.Settings.Role == WildSpawnType.bossZryachiy || owner.AIData.Player.Profile.Info.Settings.Role == WildSpawnType.followerZryachiy))
		{
			return true;
		}
		return false;
	}

	public void OnFriendlyExUsecPurchased(Player player)
	{
		BotLighthouseKeeperFriendlyExUsecs item = new BotLighthouseKeeperFriendlyExUsecs(this, BotsController.Players, player);
		FriendlyExUsecs_1.Add(item);
	}

	public void OnFriendlyZryachiyPurchased(Player player)
	{
		BotLighthouseKeeperFriendlyZryachiy item = new BotLighthouseKeeperFriendlyZryachiy(this, BotsController.Players, player);
		FriendlyZryachiy_1.Add(item);
	}

	public void CancelAllLighthouseKeeperServicesForPlayersTeam(IPlayer player)
	{
		foreach (Player allAlivePlayers in Singleton<GameWorld>.Instance.AllAlivePlayersList)
		{
			if (allAlivePlayers != null && !string.IsNullOrEmpty(allAlivePlayers.GroupId) && allAlivePlayers.GroupId == player.GroupId)
			{
				method_1(allAlivePlayers);
			}
		}
		if (player.GroupId == null)
		{
			method_1(player);
		}
	}

	public void method_0(string playerProfileId)
	{
		List<Player> allAlivePlayersList = Singleton<GameWorld>.Instance.AllAlivePlayersList;
		int num = 0;
		Player player;
		while (true)
		{
			if (num < allAlivePlayersList.Count)
			{
				player = allAlivePlayersList[num];
				if (player != null && player.Profile.Id == playerProfileId)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		CancelAllLighthouseKeeperServicesForPlayersTeam(player);
	}

	public bool IsExUsecsServiceActive(IPlayer player)
	{
		foreach (BotLighthouseKeeperFriendlyExUsecs item in FriendlyExUsecs_1)
		{
			if (item.PlayerProfileId == player.Profile.Id)
			{
				return true;
			}
		}
		return false;
	}

	public void method_1(IPlayer player)
	{
		for (int i = 0; i < FriendlyExUsecs_1.Count; i++)
		{
			BotLighthouseKeeperFriendlyExUsecs botLighthouseKeeperFriendlyExUsecs = FriendlyExUsecs_1[i];
			if (botLighthouseKeeperFriendlyExUsecs != null && (botLighthouseKeeperFriendlyExUsecs.Disposed || (!string.IsNullOrEmpty(player.GroupId) && botLighthouseKeeperFriendlyExUsecs.PlayerGroupId == player.GroupId) || botLighthouseKeeperFriendlyExUsecs.PlayerProfileId == player.Profile.Id))
			{
				botLighthouseKeeperFriendlyExUsecs.Dispose();
				FriendlyExUsecs_1.RemoveAt(i);
				i--;
			}
		}
		for (int j = 0; j < FriendlyZryachiy_1.Count; j++)
		{
			BotLighthouseKeeperFriendlyZryachiy botLighthouseKeeperFriendlyZryachiy = FriendlyZryachiy_1[j];
			if (botLighthouseKeeperFriendlyZryachiy != null && (botLighthouseKeeperFriendlyZryachiy.Disposed || (!string.IsNullOrEmpty(player.GroupId) && botLighthouseKeeperFriendlyZryachiy.PlayerGroupId == player.GroupId) || botLighthouseKeeperFriendlyZryachiy.PlayerProfileId == player.Profile.Id))
			{
				botLighthouseKeeperFriendlyZryachiy.Dispose();
				FriendlyZryachiy_1.RemoveAt(j);
				j--;
			}
		}
		if (!player.Profile.TradersInfo.TryGetValue("638f541a29ffd1183d187f57", out var value))
		{
			return;
		}
		RadioTransmitterRecodableComponent recodableComponent = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId).RecodableItemsHandler.GetRecodableComponent<RadioTransmitterRecodableComponent>();
		bool flag = recodableComponent?.Handler != null;
		if (value.Standing > 0.009999999776482582)
		{
			value.SetStanding(0.009999999776482582);
			if (flag && recodableComponent.Status == RadioTransmitterStatus.Green)
			{
				recodableComponent.SetStatus(RadioTransmitterStatus.Yellow);
			}
		}
		else
		{
			value.SetStanding(0.0);
			if (flag)
			{
				recodableComponent.SetEncoded(value: false);
				recodableComponent.SetStatus(RadioTransmitterStatus.Red);
			}
		}
		if (flag)
		{
			recodableComponent.Handler.OnGetRadioTransmitterStatusFromClient();
		}
	}

	public void Dispose()
	{
		foreach (BotLighthouseKeeperFriendlyZryachiy item in FriendlyZryachiy_1)
		{
			item?.Dispose();
		}
		foreach (BotLighthouseKeeperFriendlyExUsecs item2 in FriendlyExUsecs_1)
		{
			item2?.Dispose();
		}
		FriendlyZryachiy_1?.Clear();
		FriendlyExUsecs_1?.Clear();
	}
}
