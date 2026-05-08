using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class AIBossPlayer : IBossToFollow
{
	[NonSerialized]
	public Player Player;

	[field: NonSerialized]
	public bool IAmBoos { get; }

	public bool NeedProtection => true;

	[field: NonSerialized]
	public PatrollingData PatrollingData { get; set; }

	public Vector3 Position => this.Player.Position;

	public Vector3 PositionOrTargetCover => this.Player.Position;

	[field: NonSerialized]
	public List<BotOwner> Followers { get; } = new List<BotOwner>();

	public int FollowersTargetCount => 1;

	public bool IsAlive => this.Player.HealthController.IsAlive;

	public Vector3 PositionIfInCover => this.Player.Position;

	public float MoveSpeed => 0.7f;

	public bool IsAI => false;

	public AIBossPlayer(Player player)
	{
		this.Player = player;
		if (player.Profile.Info.Settings == null)
		{
			IAmBoos = false;
		}
		else
		{
			IAmBoos = GClass2190.IsBoss(player.Profile.Info.Settings);
		}
	}

	public PatrolPoint GetPatrolPosByIndex(int botFollowerIndex)
	{
		return null;
	}

	public ABossLogic GetBossLogic()
	{
		return null;
	}

	public void RemoveFollower(BotOwner owner)
	{
		Followers.Remove(owner);
	}

	public bool IsMe(IPlayer player)
	{
		return player == this.Player;
	}

	public IPlayer Player()
	{
		return this.Player;
	}

	public EnemyInfo CurEnemy()
	{
		return null;
	}

	public BotOwner GetFirstFollower(bool withGrenade)
	{
		int num = 0;
		BotOwner botOwner;
		while (true)
		{
			if (num < Followers.Count)
			{
				botOwner = Followers[num];
				if (botOwner.HealthController.IsAlive && (!withGrenade || botOwner.WeaponManager.Grenades.HaveGrenade))
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return botOwner;
	}

	public void OfferBot(BotOwner bot)
	{
		Followers.Add(bot);
		bot.BotFollower.PatrolDataFollower.InitPlayer(this.Player);
		bot.BotFollower.SetToFollow(this, Followers.Count - 1);
	}

	public void Dispose()
	{
		if (IAmBoos)
		{
			BotOwner[] array = Followers.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].BotFollower.Dispose();
			}
			Followers.Clear();
		}
	}
}
