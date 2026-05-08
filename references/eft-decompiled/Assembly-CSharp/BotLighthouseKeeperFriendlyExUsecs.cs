using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using EFT.InventoryLogic;
using UnityEngine;

public class BotLighthouseKeeperFriendlyExUsecs : GClass533
{
	[field: NonSerialized]
	public bool Disposed { get; set; }

	public BotLighthouseKeeperFriendlyExUsecs(BotLighthouseKeeperServices services, IPlayersCollection allBots, Player player)
		: base(services, allBots, player)
	{
		BotLighthouseKeeperServices_0 = services;
		IplayersCollection_0 = allBots;
		OnFriendlyExUsecActivated(player, reactivate: false);
	}

	public override void ActivateService(Player playerServiceOwner, bool reactivate)
	{
		OnFriendlyExUsecActivated(playerServiceOwner, reactivate);
	}

	public void OnFriendlyExUsecActivated(Player playerServiceOwner, bool reactivate)
	{
		foreach (IPlayer item in IplayersCollection_0)
		{
			if ((item.AIData.IsAI && item.AIData.BotOwner.BotState != EBotState.Active) || !method_1(item))
			{
				continue;
			}
			item.AIData.BotOwner.BotsGroup.RemoveEnemy(playerServiceOwner, EBotEnemyCause.lighthouseKeeperServices);
			item.AIData.BotOwner.BotsGroup.AddAlly(playerServiceOwner);
			item.AIData.BotOwner.BotsGroup.OnEnemyAdd += base.method_2;
			foreach (Player allAlivePlayers in Singleton<GameWorld>.Instance.AllAlivePlayersList)
			{
				if (!string.IsNullOrEmpty(allAlivePlayers.GroupId) && allAlivePlayers.GroupId == playerServiceOwner.GroupId && (!reactivate || !BotLighthouseKeeperServices_0.MarkedPlayers.Contains(allAlivePlayers.Id)))
				{
					Player player = allAlivePlayers;
					item.AIData.BotOwner.BotsGroup.RemoveEnemy(player, EBotEnemyCause.lighthouseKeeperServices);
					item.AIData.BotOwner.BotsGroup.AddAlly(player);
					List_0.Add(player);
					if (!reactivate)
					{
						BotLighthouseKeeperServices_0.MarkedPlayers.Remove(allAlivePlayers.Id);
					}
				}
			}
			if (item.AIData.BotOwner.BotState == EBotState.Active)
			{
				item.AIData.BotOwner.Brain.BaseBrain.CalcActionNextFrame();
			}
		}
		Player_0 = playerServiceOwner;
		Singleton<BotEventHandler>.Instance.OnBeingHit += method_7;
		Singleton<BotEventHandler>.Instance.OnKill += method_5;
	}

	public void method_4()
	{
		if (Player_0 == null)
		{
			return;
		}
		List<Player> allAlivePlayersList = Singleton<GameWorld>.Instance.AllAlivePlayersList;
		IBotTimer?.Stop();
		foreach (Player item in allAlivePlayersList)
		{
			if ((string.IsNullOrEmpty(item.GroupId) || !(item.GroupId == Player_0.GroupId)) && !(item == Player_0))
			{
				continue;
			}
			foreach (IPlayer item2 in IplayersCollection_0)
			{
				if (method_1(item2))
				{
					item2.AIData.BotOwner.BotsGroup.AddEnemy(item, EBotEnemyCause.lighthouseKeeperServices);
					item2.AIData.BotOwner.BotsGroup.OnEnemyAdd -= base.method_2;
				}
			}
		}
		IBotTimer = null;
	}

	public void method_5(IPlayer killer, IPlayer target)
	{
		if (Player_0 == null)
		{
			return;
		}
		if (BotLighthouseKeeperServices_0.IsPlayerExUsecFriendly(killer) && method_6(target))
		{
			BotLighthouseKeeperServices_0.CancelAllLighthouseKeeperServicesForPlayersTeam(killer);
			return;
		}
		bool flag = killer.Id == Player_0.Id;
		foreach (Player item in List_0)
		{
			flag |= item.Id == killer.Id;
		}
		if (flag && target.AIData != null && target.AIData.IsAI && (method_1(target) || method_0(target)))
		{
			BotLighthouseKeeperServices_0.CancelAllLighthouseKeeperServicesForPlayersTeam(killer);
		}
	}

	public bool method_6(IPlayer player)
	{
		if (player.IsAI)
		{
			return false;
		}
		if (player.AIData.PlaceInfo.InfoLogicAllEnemy is AIPlaceInfoLogicZryachiy)
		{
			RadioTransmitterRecodableComponent radioTransmitterRecodableComponent = player.FindRadioTransmitter();
			if (radioTransmitterRecodableComponent != null && radioTransmitterRecodableComponent.Status == RadioTransmitterStatus.Green)
			{
				return true;
			}
		}
		return false;
	}

	public void method_7(DamageInfoStruct damageInfo, Player victim)
	{
		if (Player_0 == null)
		{
			return;
		}
		IPlayerOwner player = damageInfo.Player;
		if (player == null || player.iPlayer == null || OutOfRange(victim.Position))
		{
			return;
		}
		if (player.iPlayer.Id == Player_0.Id)
		{
			method_11(damageInfo, victim);
			return;
		}
		if (victim.PlayerId == Player_0.Id)
		{
			method_10(damageInfo, victim);
			return;
		}
		int num = 0;
		while (true)
		{
			if (num < List_0.Count)
			{
				Player player2 = List_0[num];
				if (victim.PlayerId != player2.Id)
				{
					if (player.iPlayer.Id == player2.Id)
					{
						break;
					}
					num++;
					continue;
				}
				method_8(damageInfo, victim);
				return;
			}
			return;
		}
		method_9(damageInfo, victim);
	}

	public void method_8(DamageInfoStruct damageInfo, Player victim)
	{
		IPlayer iPlayer = damageInfo.Player.iPlayer;
		if (!method_1(iPlayer) && !BotLighthouseKeeperServices_0.IsPlayerExUsecFriendly(iPlayer))
		{
			method_12(iPlayer, marked: false);
		}
	}

	public void method_9(DamageInfoStruct damageInfo, Player victim)
	{
		IPlayer iPlayer = damageInfo.Player.iPlayer;
		if (!BotLighthouseKeeperServices_0.IsPlayerExUsecFriendly(victim))
		{
			if (method_1(victim))
			{
				method_12(iPlayer, marked: false);
				method_2(iPlayer, EBotEnemyCause.lighthouseKeeperServices);
			}
			else
			{
				method_12(victim, marked: true);
			}
		}
	}

	public void method_10(DamageInfoStruct damageInfo, Player victim)
	{
		IPlayerOwner player = damageInfo.Player;
		if (player.IsAI)
		{
			if (method_1(damageInfo.Player.AIData.BotOwner))
			{
				return;
			}
		}
		else
		{
			int id = damageInfo.Player.iPlayer.Id;
			string groupId = damageInfo.Player.iPlayer.GroupId;
			foreach (BotLighthouseKeeperFriendlyExUsecs friendlyExUsec in BotLighthouseKeeperServices_0.FriendlyExUsecs)
			{
				if (friendlyExUsec.Player_0 == null || friendlyExUsec.Player_0.Id == victim.Id)
				{
					continue;
				}
				if (id == friendlyExUsec.Player_0.Id || (friendlyExUsec.PlayerGroupId != null && groupId != null && friendlyExUsec.PlayerGroupId == groupId))
				{
					return;
				}
				foreach (Player item in friendlyExUsec.List_0)
				{
					if (item != null && id == item.Id)
					{
						return;
					}
				}
			}
		}
		method_12(player.iPlayer, marked: false);
	}

	public void method_11(DamageInfoStruct damageInfo, Player victim)
	{
		if (!IgnoreTargetCauseExUsecFriendly(damageInfo.Player.iPlayer.Id, victim.Id))
		{
			if (victim.AIData == null || !victim.AIData.IsAI)
			{
				method_12(victim, marked: true);
			}
			else if (BotSettingsRepoClass.IsExUsec(victim.Profile.Info.Settings.Role))
			{
				method_12(damageInfo.Player.iPlayer, marked: false, EBotEnemyCause.lighthouseKeeperServices);
				method_2(damageInfo.Player.iPlayer, EBotEnemyCause.lighthouseKeeperServices);
			}
			else
			{
				method_12(victim, marked: true);
			}
		}
	}

	public override bool OutOfRange(Vector3 position)
	{
		foreach (IPlayer item in IplayersCollection_0)
		{
			if (method_1(item))
			{
				float visibleDist = item.AIData.BotOwner.LookSensor.VisibleDist;
				if (!(GClass856.SqrDistance(item.Position, position) >= visibleDist * visibleDist))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void method_12(IPlayer target, bool marked, EBotEnemyCause cause = EBotEnemyCause.lighthouseKeeperServicesTarget)
	{
		foreach (IPlayer item in IplayersCollection_0)
		{
			if (method_1(item))
			{
				item.AIData.BotOwner.BotsGroup.AddEnemy(target, cause);
			}
		}
		if (marked)
		{
			BotLighthouseKeeperServices_0.MarkedPlayers.Add(target.Id);
		}
	}

	public bool IgnoreTargetCauseExUsecFriendly(int aggressorId, int targetId)
	{
		foreach (BotLighthouseKeeperFriendlyExUsecs friendlyExUsec in BotLighthouseKeeperServices_0.FriendlyExUsecs)
		{
			if (targetId != friendlyExUsec.Player_0.Id)
			{
				if (friendlyExUsec == this)
				{
					continue;
				}
				foreach (Player item in friendlyExUsec.List_0)
				{
					if (item != null && targetId == item.Id && aggressorId != friendlyExUsec.PlayerId)
					{
						return true;
					}
				}
				continue;
			}
			return true;
		}
		return false;
	}

	public void Dispose()
	{
		Singleton<BotEventHandler>.Instance.OnBeingHit -= method_7;
		Singleton<BotEventHandler>.Instance.OnKill -= method_5;
		method_4();
		IBotTimer?.Stop();
		IBotTimer = null;
		Player_0 = null;
		Disposed = true;
	}
}
