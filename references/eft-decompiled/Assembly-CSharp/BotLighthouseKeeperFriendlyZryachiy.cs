using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using EFT.InventoryLogic;
using UnityEngine;

public class BotLighthouseKeeperFriendlyZryachiy : GClass533
{
	[NonSerialized]
	public BotOwner Zryachiy;

	[field: NonSerialized]
	public bool Disposed { get; set; }

	public BotLighthouseKeeperFriendlyZryachiy(BotLighthouseKeeperServices services, IPlayersCollection allBots, Player player)
		: base(services, allBots, player)
	{
		BotLighthouseKeeperServices_0 = services;
		IplayersCollection_0 = allBots;
		foreach (IPlayer item in IplayersCollection_0)
		{
			if (item.Profile.Info.Settings.Role == WildSpawnType.bossZryachiy)
			{
				Zryachiy = item.AIData.BotOwner;
				break;
			}
		}
		OnFriendlyZryachiyActivated(player, reactivate: false);
	}

	public override void ActivateService(Player playerServiceOwner, bool reactivate)
	{
		OnFriendlyZryachiyActivated(playerServiceOwner, reactivate);
	}

	public void OnFriendlyZryachiyActivated(Player playerServiceOwner, bool reactivate)
	{
		foreach (IPlayer item in IplayersCollection_0)
		{
			if (!method_0(item))
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
					List_0.Add(player);
					item.AIData.BotOwner.BotsGroup.RemoveEnemy(player, EBotEnemyCause.lighthouseKeeperServices);
					item.AIData.BotOwner.BotsGroup.AddAlly(player);
					if (!reactivate)
					{
						BotLighthouseKeeperServices_0.MarkedPlayers.Remove(allAlivePlayers.Id);
					}
				}
			}
			item.AIData.BotOwner.Brain.BaseBrain.CalcActionNextFrame();
		}
		Player_0 = playerServiceOwner;
		Singleton<BotEventHandler>.Instance.OnBeingHit += method_6;
		Singleton<BotEventHandler>.Instance.OnKill += method_4;
	}

	public void method_4(IPlayer killer, IPlayer target)
	{
		if (Player_0 == null)
		{
			return;
		}
		if (BotLighthouseKeeperServices_0.IsPlayerZryachiyFriendly(killer) && method_5(target))
		{
			BotLighthouseKeeperServices_0.CancelAllLighthouseKeeperServicesForPlayersTeam(killer);
			return;
		}
		bool flag = killer.Id == Player_0.Id;
		foreach (Player item in List_0)
		{
			flag |= item.Id == killer.Id;
		}
		if (flag && target.AIData != null && target.AIData.IsAI && method_0(target))
		{
			BotLighthouseKeeperServices_0.CancelAllLighthouseKeeperServicesForPlayersTeam(killer);
		}
	}

	public bool method_5(IPlayer player)
	{
		if (player.IsAI)
		{
			return false;
		}
		if (player.AIData.PlaceInfo.InfoLogicAllEnemy is AIPlaceInfoLogicZryachiy aIPlaceInfoLogicZryachiy)
		{
			if (!aIPlaceInfoLogicZryachiy.AttackPossible)
			{
				return false;
			}
			RadioTransmitterRecodableComponent radioTransmitterRecodableComponent = player.FindRadioTransmitter();
			if (radioTransmitterRecodableComponent != null && radioTransmitterRecodableComponent.Status == RadioTransmitterStatus.Green)
			{
				return true;
			}
		}
		return false;
	}

	public void method_6(DamageInfoStruct damageInfo, Player victim)
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
			method_10(damageInfo, victim);
			return;
		}
		if (victim.PlayerId == Player_0.Id)
		{
			method_9(damageInfo, victim);
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
				method_7(damageInfo, victim);
				return;
			}
			return;
		}
		method_8(damageInfo, victim);
	}

	public void method_7(DamageInfoStruct damageInfo, Player victim)
	{
		IPlayer iPlayer = damageInfo.Player.iPlayer;
		if (!method_0(iPlayer) && !BotLighthouseKeeperServices_0.IsPlayerZryachiyFriendly(iPlayer))
		{
			method_11(iPlayer, marked: false);
		}
	}

	public void method_8(DamageInfoStruct damageInfo, Player victim)
	{
		IPlayer iPlayer = damageInfo.Player.iPlayer;
		if (!BotLighthouseKeeperServices_0.IsPlayerZryachiyFriendly(victim))
		{
			if (method_0(victim))
			{
				method_11(iPlayer, marked: false);
				method_2(iPlayer, EBotEnemyCause.lighthouseKeeperServices);
			}
			else
			{
				method_11(victim, marked: true);
			}
		}
	}

	public void method_9(DamageInfoStruct damageInfo, Player victim)
	{
		if (damageInfo.Player.IsAI)
		{
			if (method_0(damageInfo.Player.AIData.BotOwner))
			{
				return;
			}
		}
		else
		{
			int id = damageInfo.Player.iPlayer.Id;
			string groupId = damageInfo.Player.iPlayer.GroupId;
			foreach (BotLighthouseKeeperFriendlyZryachiy item in BotLighthouseKeeperServices_0.FriendlyZryachiy)
			{
				if (item.Player_0 == null || item.Player_0.Id == victim.Id)
				{
					continue;
				}
				if (id == item.Player_0.Id || (item.PlayerGroupId != null && groupId != null && item.PlayerGroupId == groupId))
				{
					return;
				}
				foreach (Player item2 in item.List_0)
				{
					if (item2 != null && id == item2.Id)
					{
						return;
					}
				}
			}
		}
		method_11(damageInfo.Player.iPlayer, marked: false);
	}

	public void method_10(DamageInfoStruct damageInfo, Player victim)
	{
		if (!method_13(damageInfo.Player.iPlayer.Id, victim.Id))
		{
			if (victim.AIData == null || !victim.AIData.IsAI)
			{
				method_11(victim, marked: true);
			}
			else if (method_0(victim))
			{
				method_11(damageInfo.Player.iPlayer, marked: false, EBotEnemyCause.lighthouseKeeperServices);
				method_2(damageInfo.Player.iPlayer, EBotEnemyCause.lighthouseKeeperServices);
			}
			else
			{
				method_11(victim, marked: true);
			}
		}
	}

	public override bool OutOfRange(Vector3 position)
	{
		float currentVisibleDistance = Zryachiy.Settings.Current.CurrentVisibleDistance;
		return GClass856.SqrDistance(Zryachiy.Position, position) > currentVisibleDistance * currentVisibleDistance;
	}

	public void method_11(IPlayer target, bool marked, EBotEnemyCause cause = EBotEnemyCause.lighthouseKeeperServicesTarget)
	{
		foreach (IPlayer item in IplayersCollection_0)
		{
			if (method_0(item))
			{
				item.AIData.BotOwner.BotsGroup.AddEnemy(target, cause);
				if (item.AIData.BotOwner.Boss.BossLogic is ZyriachyBossLogicClass zyriachyBossLogicClass)
				{
					zyriachyBossLogicClass.AddEnemy(target, cause);
				}
			}
		}
		if (marked)
		{
			BotLighthouseKeeperServices_0.MarkedPlayers.Add(target.Id);
		}
	}

	public void method_12()
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
				if (method_0(item2))
				{
					item2.AIData.BotOwner.BotsGroup.OnEnemyAdd -= base.method_2;
					if (item2.AIData.BotOwner.Boss.BossLogic is ZyriachyBossLogicClass zyriachyBossLogicClass)
					{
						zyriachyBossLogicClass.AddEnemy(item);
					}
					item2.AIData.BotOwner.BotsGroup.AddEnemy(item, EBotEnemyCause.lighthouseKeeperServices);
				}
			}
		}
		IBotTimer = null;
	}

	public bool method_13(int aggressorId, int targetId)
	{
		foreach (BotLighthouseKeeperFriendlyZryachiy item in BotLighthouseKeeperServices_0.FriendlyZryachiy)
		{
			if (targetId != item.Player_0.Id)
			{
				if (item == this)
				{
					continue;
				}
				foreach (Player item2 in item.List_0)
				{
					if (item2 != null && targetId == item2.Id && aggressorId != item.PlayerId)
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
		Singleton<BotEventHandler>.Instance.OnBeingHit -= method_6;
		Singleton<BotEventHandler>.Instance.OnKill -= method_4;
		method_12();
		IBotTimer?.Stop();
		IBotTimer = null;
		Player_0 = null;
		Disposed = true;
	}
}
