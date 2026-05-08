using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class GClass456 : GClass429
{
	[NonSerialized]
	public const float Float_0 = 15f;

	[NonSerialized]
	public const float Float_1 = 0.97f;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public List<BotOwner> List_0 = new List<BotOwner>();

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	public virtual int TargetFollowersCount
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
		[CompilerGenerated]
		set
		{
			Int_0 = value;
		}
	}

	public virtual List<BotOwner> Followers => List_0;

	public GClass456([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	public virtual void Activate()
	{
	}

	public virtual void Clear()
	{
		List_0.Clear();
	}

	public virtual void AddFollower(BotOwner botFollower)
	{
		List_0.Add(botFollower);
	}

	public virtual void Remove(BotOwner botFollower)
	{
		List_0.Remove(botFollower);
	}

	public virtual List<string> DebugInfo()
	{
		return new List<string> { List_0.Count + "/" + TargetFollowersCount };
	}

	public void SetTargetFollowersCount(int followersCount)
	{
		TargetFollowersCount = followersCount;
	}

	public virtual void CheckFollowers()
	{
		if (Followers.Count < TargetFollowersCount && Float_2 < Time.time)
		{
			Float_2 = Time.time + 15f;
			UpdateFollowers();
		}
		if (Float_3 < Time.time)
		{
			Float_3 = Time.time + 0.97f;
			BotOwner_0.BotsGroup.BotGroupWarnData.UpdateDistToScavsPlayersAndWarnIntruders();
		}
	}

	public void UpdateFollowers()
	{
		BotsGroup botsGroup = BotOwner_0.BotsGroup;
		for (int i = 0; i < botsGroup.MembersCount; i++)
		{
			BotOwner botOwner = botsGroup.Member(i);
			if (botOwner.BotState == EBotState.Active && !botOwner.IsDead && !BotOwner_0.Boss.OfferSelf(botOwner))
			{
				break;
			}
		}
	}

	public virtual void Dispose()
	{
	}
}
