using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using EFT.Game.Spawning;
using UnityEngine;

public abstract class GClass533
{
	[NonSerialized]
	public Player Player_0;

	[NonSerialized]
	public List<Player> List_0 = new List<Player>();

	[NonSerialized]
	public BotLighthouseKeeperServices BotLighthouseKeeperServices_0;

	[NonSerialized]
	public IPlayersCollection IplayersCollection_0;

	[NonSerialized]
	public GClass641.IBotTimer IBotTimer;

	public string PlayerProfileId
	{
		get
		{
			if (!(Player_0 == null))
			{
				return Player_0.Profile.Id;
			}
			return null;
		}
	}

	public int PlayerId => Player_0.Id;

	public string PlayerGroupId
	{
		get
		{
			if (!(Player_0 == null))
			{
				return Player_0.GroupId;
			}
			return null;
		}
	}

	public GClass533(BotLighthouseKeeperServices services, IPlayersCollection allBots, Player player)
	{
		BotLighthouseKeeperServices_0 = services;
		IplayersCollection_0 = allBots;
	}

	public bool method_0(IPlayer player)
	{
		if (player.IsAI)
		{
			if (player.AIData.Player.Profile.Info.Settings.Role != WildSpawnType.bossZryachiy)
			{
				return player.AIData.Player.Profile.Info.Settings.Role == WildSpawnType.followerZryachiy;
			}
			return true;
		}
		return false;
	}

	public bool method_1(IPlayer player)
	{
		if (player.IsAI)
		{
			return BotSettingsRepoClass.IsExUsec(player.AIData.Player.Profile.Info.Settings.Role);
		}
		return false;
	}

	public void method_2(IPlayer player, EBotEnemyCause cause)
	{
		if (IBotTimer == null && cause != EBotEnemyCause.lighthouseKeeperServicesTarget)
		{
			IBotTimer = StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(5.0));
			IBotTimer.OnTimer += delegate
			{
				Reactivate();
			};
		}
	}

	public void Reactivate()
	{
		if (!(Player_0 == null))
		{
			IBotTimer.Stop();
			IBotTimer = null;
			ActivateService(Player_0, reactivate: true);
		}
	}

	public abstract bool OutOfRange(Vector3 position);

	public abstract void ActivateService(Player playerServiceOwner, bool reactivate);

	[CompilerGenerated]
	public void method_3()
	{
		Reactivate();
	}
}
