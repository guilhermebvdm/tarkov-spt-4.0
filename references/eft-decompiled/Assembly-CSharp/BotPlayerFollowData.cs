using System;
using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using UnityEngine;

public class BotPlayerFollowData : GClass429
{
	[NonSerialized]
	public Vector3 Offset;

	[NonSerialized]
	public Vector3 LastPlayerConnectionPlace;

	[NonSerialized]
	public float LastPlayerConnectionTime;

	[NonSerialized]
	public float SetSprintTime;

	[NonSerialized]
	public float NextRefreshGoTo;

	[NonSerialized]
	public IPlayer ToFollow;

	[NonSerialized]
	public bool SprintNow;

	[NonSerialized]
	public float SDIST_NOT_TO_FOLLOW = 10000f;

	[NonSerialized]
	public float SDIST_CHECK = 25f;

	[NonSerialized]
	public float SDIST_SPRINT = 25f;

	[NonSerialized]
	public float PERIOD_CHECK = 5.5f;

	[field: NonSerialized]
	public bool HaveFound { get; set; }

	public bool HaveToFollow => ToFollow != null;

	public string FollowGroupId => ToFollow.GroupId;

	public BotPlayerFollowData(BotOwner owner)
		: base(owner)
	{
	}

	public void UpdateFromNode()
	{
		Vector3 vector = ToFollow.Position + GClass855.NormalizeFastSelf(ToFollow.LookDirection) * 1f + Offset;
		float sqrMagnitude = (vector - LastPlayerConnectionPlace).sqrMagnitude;
		float num = Time.time - LastPlayerConnectionTime;
		if (sqrMagnitude < SDIST_CHECK && num < PERIOD_CHECK)
		{
			if (!(NextRefreshGoTo > Time.time))
			{
				NextRefreshGoTo = Time.time + 4f;
				if (BotOwner_0.Memory.CurCustomCoverPoint != null && (BotOwner_0.Memory.CurCustomCoverPoint.Position - BotOwner_0.Position).sqrMagnitude > 2f)
				{
					BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(BotOwner_0.Memory.CurCustomCoverPoint);
					BotOwner_0.Mover.GoToPoint(BotOwner_0.Memory.CurCustomCoverPoint);
				}
			}
			return;
		}
		BotOwner_0.SetTargetMoveSpeed(1f);
		if (BotOwner_0.Mover.IsComeTo(BotOwner_0.Settings.FileSettings.Move.REACH_DIST, onCover: true))
		{
			if (BotOwner_0.Memory.CurCustomCoverPoint != null && BotOwner_0.Memory.CurCustomCoverPoint.StrategyType == PointWithNeighborType.ambush)
			{
				BotOwner_0.SetPose(0.1f);
			}
			else
			{
				BotOwner_0.SetPose(1f);
			}
			BotOwner_0.StopMove();
		}
		else
		{
			BotOwner_0.SetPose(1f);
		}
		method_4();
		if (!SprintNow || !BotOwner_0.HasPathAndNotComplete)
		{
			BotOwner_0.LookData.SetLookPointByHearing();
		}
		BotOwner_0.Sprint(SprintNow);
		LastPlayerConnectionTime = Time.time;
		LastPlayerConnectionPlace = vector;
		CustomNavigationPoint closestPoint = BotOwner_0.Covers.GetClosestPoint(vector);
		if (closestPoint != null)
		{
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(closestPoint);
			if (!(NextRefreshGoTo > Time.time))
			{
				NextRefreshGoTo = Time.time + 4f;
				BotOwner_0.Mover.GoToPoint(closestPoint);
			}
		}
	}

	public void SetToFollow(IPlayer player)
	{
		if (HaveFound)
		{
			return;
		}
		HaveFound = true;
		ToFollow = player;
		BotOwner_0.BotsGroup.RemoveEnemy(ToFollow, EBotEnemyCause.doFollow);
		IPlayersCollection players = BotOwner_0.BotsGroup.BotGame.BotsController.Players;
		if (!GClass856.IsNullOrEmpty(player.GroupId))
		{
			foreach (IPlayer item in players)
			{
				if (item.GroupId == player.GroupId)
				{
					BotOwner_0.BotsGroup.RemoveEnemy(item, EBotEnemyCause.doFollow2);
				}
			}
		}
		BotOwner_0.Memory.OnAddEnemy += method_2;
		Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(ToFollow.ProfileId).GetPlayer.BeingHitAction += method_3;
		ToFollow.HealthController.DiedEvent += method_5;
		BotOwner_0.BotsGroup.BotGame.BotsController.Bots.OnPlayerRemove += method_0;
		BotOwner_0.BotsGroup.Lock();
	}

	public bool IsEnoughtDist()
	{
		return (BotOwner_0.Position - ToFollow.Position).sqrMagnitude < SDIST_NOT_TO_FOLLOW;
	}

	public bool IsFollower(IPlayer player)
	{
		if (!HaveToFollow)
		{
			return false;
		}
		return player.Id == ToFollow.Id;
	}

	public void method_0(Player person)
	{
		if (HaveFound)
		{
			method_1(person);
		}
	}

	public void method_1(IPlayer person)
	{
		if (ToFollow != null && person != null && ToFollow.Id == person.Id)
		{
			BotOwner_0.Memory.OnAddEnemy -= method_2;
			HaveFound = false;
			method_6();
		}
	}

	public void method_2(IPlayer person)
	{
		if (HaveFound && ToFollow != null && ToFollow.Id == person.Id)
		{
			BotOwner_0.Memory.OnAddEnemy -= method_2;
			HaveFound = false;
			method_6();
		}
	}

	public void method_3(DamageInfoStruct damageInfo, EBodyPart eBodyPart, float val)
	{
		if (damageInfo.Player != null && (ToFollow == null || damageInfo.Player.iPlayer.Id != ToFollow.Id))
		{
			BotOwner_0.BotsGroup.AddEnemy(damageInfo.Player.iPlayer, EBotEnemyCause.followGetHit);
			if (BotOwner_0.EnemiesController.EnemyInfos.TryGetValue(damageInfo.Player.iPlayer, out var value))
			{
				value.SetVisible(value: true);
			}
		}
	}

	public void method_4()
	{
		if (!(Time.time - SetSprintTime > 4f))
		{
			return;
		}
		if (!SprintNow)
		{
			if ((ToFollow.Position - BotOwner_0.Position).sqrMagnitude > SDIST_SPRINT)
			{
				SetSprintTime = Time.time;
				SprintNow = true;
			}
		}
		else if ((ToFollow.Position - BotOwner_0.Position).sqrMagnitude < SDIST_SPRINT)
		{
			SetSprintTime = Time.time;
			SprintNow = true;
		}
		else
		{
			SetSprintTime = Time.time;
			SprintNow = false;
		}
	}

	public void method_5(EDamageType obj)
	{
		method_1(ToFollow);
	}

	public void method_6()
	{
		if (ToFollow != null)
		{
			Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(ToFollow.ProfileId).GetPlayer.BeingHitAction -= method_3;
			ToFollow.HealthController.DiedEvent -= method_5;
			ToFollow = null;
		}
	}

	public void Dispose()
	{
		method_1(ToFollow);
	}
}
