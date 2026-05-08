using System;
using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT;
using UnityEngine;

public class BotGroupWarnData
{
	[NonSerialized]
	public Dictionary<Player, GClass428> PlayersWarned = new Dictionary<Player, GClass428>();

	[NonSerialized]
	public List<BotSettingsClass> WarnTargets = new List<BotSettingsClass>();

	[NonSerialized]
	public BotsGroup Group;

	[NonSerialized]
	public Dictionary<int, BotSettingsClass> WarnTargetsByPlayerId = new Dictionary<int, BotSettingsClass>();

	[NonSerialized]
	public HashSet<int> WarningPlayers = new HashSet<int>();

	public BotOwner Boss => Group.BossGroup.Boss;

	public float Single_0 => Boss.Settings.FileSettings.Boss.WARN_PLAYER_PERIOD;

	public BotGroupWarnData(BotsGroup group)
	{
		Group = group;
	}

	public virtual void UpdateDistToScavsPlayersAndWarnIntruders()
	{
		List<Player> players2Enemies = new List<Player>();
		WarnTargets = method_10(WarnTargets);
		if (Boss == null)
		{
			return;
		}
		foreach (BotSettingsClass warnTarget in WarnTargets)
		{
			Player player = warnTarget.Player;
			if (player.HealthController.IsAlive && !Boss.PlayerFollowData.IsFollower(player) && Boss.Settings.IsPlayerWarn(player))
			{
				float sqrMagnitude = (player.Position - Boss.Position).sqrMagnitude;
				if (Mathf.Abs(player.Position.y - Boss.Position.y) < method_1(player) && sqrMagnitude < method_2(player))
				{
					players2Enemies.Add(player);
				}
				else if (!WarningPlayers.Contains(player.Id))
				{
					method_0(warnTarget, ref players2Enemies);
				}
			}
		}
		method_6(players2Enemies);
	}

	public void method_0(BotSettingsClass targetToWarn, ref List<Player> players2Enemies)
	{
		Player player = targetToWarn.Player;
		float num = method_2(player);
		float num2 = method_3(player);
		float num3 = 0f;
		num3 = player.Profile.Side switch
		{
			EPlayerSide.Bear => method_4(player), 
			EPlayerSide.Usec => method_5(player), 
			_ => method_3(player), 
		};
		BotOwner botOwner = null;
		float num4 = float.MaxValue;
		int num5 = 0;
		while (true)
		{
			if (num5 < Group.MembersCount)
			{
				BotOwner botOwner2 = Group.Member(num5);
				bool flag = Mathf.Abs(player.Position.y - botOwner2.Position.y) < method_1(player);
				float sqrMagnitude = (botOwner2.Position - player.Position).sqrMagnitude;
				if (flag && (sqrMagnitude < num || (Boss.Boss.Followers.Count < Boss.Settings.FileSettings.Boss.COUNT_FOLLOWERS_TO_WARN && !Boss.BotsController.BotTradersServices.LighthouseKeeperServices.ShallWarn(Boss, player))) && !(sqrMagnitude >= num2))
				{
					break;
				}
				float sqrMagnitude2 = (botOwner2.Position - player.Position).sqrMagnitude;
				GClass428 value2;
				if (sqrMagnitude2 < num3)
				{
					bool flag2 = Mathf.Abs(player.Position.y - botOwner2.Position.y) < Boss.Settings.FileSettings.Patrol.MAX_YDIST_TO_START_WARN_REQUEST_TO_REQUESTER;
					if (Boss.Settings.FileSettings.Boss.SHALL_WARN)
					{
						if (PlayersWarned.TryGetValue(player, out var value))
						{
							if (value.ShallAttack(Boss, player))
							{
								players2Enemies.Add(player);
							}
							else if (value.ComeInsideTime + Single_0 < Time.time)
							{
								PlayersWarned.Remove(player);
							}
						}
						else if (sqrMagnitude2 < num4)
						{
							botOwner = botOwner2;
							num4 = sqrMagnitude2;
						}
					}
					else if (sqrMagnitude < Boss.Settings.FileSettings.Boss.BOSS_DIST_TO_SHOOT_SQRT && flag2)
					{
						players2Enemies.Add(player);
					}
				}
				else if (PlayersWarned.TryGetValue(player, out value2))
				{
					value2.ComeAway();
				}
				num5++;
				continue;
			}
			if (botOwner != null)
			{
				method_7(player, botOwner);
			}
			return;
		}
		players2Enemies.Add(player);
	}

	public float method_1(Player player)
	{
		if (Boss.BotsGroup.IsAlly(player))
		{
			return Boss.Settings.FileSettings.Patrol.MAX_YDIST_TO_START_WARN_REQUEST_TO_REQUESTER_ALLY;
		}
		return Boss.Settings.FileSettings.Patrol.MAX_YDIST_TO_START_WARN_REQUEST_TO_REQUESTER;
	}

	public float method_2(Player player)
	{
		if (Boss.BotsGroup.IsAlly(player))
		{
			return Boss.Settings.FileSettings.Boss.BOSS_DIST_TO_SHOOT_SQRT_ALLY;
		}
		return Boss.Settings.FileSettings.Boss.BOSS_DIST_TO_SHOOT_SQRT;
	}

	public float method_3(Player player)
	{
		if (Boss.BotsGroup.IsAlly(player))
		{
			return Boss.Settings.FileSettings.Boss.BOSS_DIST_TO_WARNING_SQRT_ALLY;
		}
		return Boss.Settings.FileSettings.Boss.BOSS_DIST_TO_WARNING_SQRT;
	}

	public float method_4(Player player)
	{
		if (Boss.BotsGroup.IsAlly(player))
		{
			return Boss.Settings.FileSettings.Boss.BOSS_DIST_TO_WARNING_SQRT_BEAR_ALLY;
		}
		return Boss.Settings.FileSettings.Boss.BOSS_DIST_TO_WARNING_SQRT_BEAR;
	}

	public float method_5(Player player)
	{
		if (Boss.BotsGroup.IsAlly(player))
		{
			return Boss.Settings.FileSettings.Boss.BOSS_DIST_TO_WARNING_SQRT_USEC_ALLY;
		}
		return Boss.Settings.FileSettings.Boss.BOSS_DIST_TO_WARNING_SQRT_USEC;
	}

	public void PlayerDead(Player player)
	{
		PlayersWarned.Remove(player);
	}

	public void method_6(List<Player> players2Enemies)
	{
		for (int i = 0; i < players2Enemies.Count; i++)
		{
			Player player = players2Enemies[i];
			if (player == null)
			{
				continue;
			}
			for (int j = 0; j < Boss.BotsGroup.MembersCount; j++)
			{
				Boss.BotsGroup.AddPointToSearch(player.Position, 150f, Boss);
			}
			PlayersWarned.Remove(player);
			Boss.BotsGroup.AddEnemy(player, EBotEnemyCause.warn);
			if (!Boss.Settings.FileSettings.Boss.SET_CHEAT_VISIBLE_WHEN_ADD_TO_ENEMY)
			{
				continue;
			}
			for (int k = 0; k < Boss.BotsGroup.MembersCount; k++)
			{
				foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in Boss.BotsGroup.Member(k).EnemiesController.EnemyInfos)
				{
					if (enemyInfo.Key.Id == player.Id)
					{
						enemyInfo.Value.SetVisible(value: true);
					}
				}
			}
		}
	}

	public void method_7(Player playerToWarn, BotOwner closestBot)
	{
		closestBot.BotTalk.Say(EPhraseTrigger.Scav);
		if (PlayersWarned.TryGetValue(playerToWarn, out var value))
		{
			value.Return();
			return;
		}
		GClass428 gClass = new GClass428();
		PlayersWarned.Add(playerToWarn, gClass);
		if (closestBot.WarnData.ThrowWarnPlayerRequest(playerToWarn, gClass))
		{
			WarningPlayers.Add(playerToWarn.Id);
		}
	}

	public void method_8(Dictionary<int, BotSettingsClass> warnTargets)
	{
		foreach (Player allAlivePlayers in Singleton<GameWorld>.Instance.AllAlivePlayersList)
		{
			if ((object)allAlivePlayers == null)
			{
				continue;
			}
			Player player = allAlivePlayers;
			if (!method_9(player) && !Boss.EnemiesController.IsEnemy(allAlivePlayers) && !warnTargets.ContainsKey(player.Id))
			{
				if (!player.AIData.IsAI && ((player.Profile.Info.Side == EPlayerSide.Bear && Boss.Settings.FileSettings.Mind.DEFAULT_BEAR_BEHAVIOUR.HasFlag(EWarnBehaviour.Warn)) || (player.Profile.Info.Side == EPlayerSide.Usec && Boss.Settings.FileSettings.Mind.DEFAULT_USEC_BEHAVIOUR.HasFlag(EWarnBehaviour.Warn)) || (player.Profile.Info.Side == EPlayerSide.Savage && Boss.Settings.FileSettings.Mind.DEFAULT_SAVAGE_BEHAVIOUR.HasFlag(EWarnBehaviour.Warn))))
				{
					warnTargets.Add(player.Id, new BotSettingsClass(player, Boss.BotsGroup));
				}
				else if (player.AIData.IsAI && Boss.Settings.FileSettings.Mind.WARN_BOT_TYPES.Contains(player.Profile.Info.Settings.Role) && player.AIData.BotOwner.BotState == EBotState.Active && !Boss.BotsGroup.Contains(player.AIData.BotOwner))
				{
					warnTargets.Add(player.Id, new BotSettingsClass(player, Boss.BotsGroup));
				}
			}
		}
	}

	public bool method_9(Player enemyInfo)
	{
		switch (Boss.Profile.Info.Settings.Role)
		{
		default:
			return enemyInfo.Loyalty.BossNoAttack;
		case WildSpawnType.pmcBot:
		case WildSpawnType.sectantWarrior:
		case WildSpawnType.sectantPriest:
		case WildSpawnType.exUsec:
		case WildSpawnType.bossKnight:
		case WildSpawnType.followerBigPipe:
		case WildSpawnType.followerBirdEye:
		case WildSpawnType.arenaFighterEvent:
		case WildSpawnType.peacefullZryachiyEvent:
		case WildSpawnType.sectactPriestEvent:
		case WildSpawnType.ravangeZryachiyEvent:
			return false;
		}
	}

	public List<BotSettingsClass> method_10(List<BotSettingsClass> warnTargets)
	{
		WarnTargetsByPlayerId.Clear();
		method_8(WarnTargetsByPlayerId);
		warnTargets.Clear();
		foreach (KeyValuePair<int, BotSettingsClass> item in WarnTargetsByPlayerId)
		{
			BotSettingsClass value = item.Value;
			if (!value.Player.HealthController.IsAlive)
			{
				continue;
			}
			if (Boss.PlayerFollowData.HaveToFollow)
			{
				if (!value.Player.AIData.IsAI && !Boss.PlayerFollowData.IsFollower(value.Player))
				{
					string followGroupId = Boss.PlayerFollowData.FollowGroupId;
					if (GClass856.IsNullOrEmpty(followGroupId) || GClass856.IsNullOrEmpty(value.Player.GroupId) || value.Player.GroupId != followGroupId)
					{
						warnTargets.Add(value);
					}
				}
			}
			else if (method_11(value))
			{
				warnTargets.Add(value);
			}
		}
		return warnTargets;
	}

	public bool method_11(BotSettingsClass enemyInfo)
	{
		if (enemyInfo.Player.IsAI)
		{
			if ((enemyInfo.Player.GroupId != Boss.GroupId || enemyInfo.Player.GroupId == null) && enemyInfo.Player.AIData.BotOwner != null)
			{
				return !enemyInfo.Player.AIData.BotOwner.IsDead;
			}
			return false;
		}
		return true;
	}

	public void WarnComplete(Player player)
	{
		WarningPlayers.Remove(player.Id);
		if (PlayersWarned.TryGetValue(player, out var value))
		{
			value.WarnedTime = Time.time;
		}
	}

	public void DisposeCallback(Player player)
	{
		WarningPlayers.Remove(player.Id);
	}
}
