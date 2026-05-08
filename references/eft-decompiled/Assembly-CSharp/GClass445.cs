using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public abstract class GClass445<T, U> : ABossLogic, IBossLogic where T : GClass356 where U : GClass356
{
	[NonSerialized]
	public Dictionary<BotOwner, Vector3> Dictionary_0 = new Dictionary<BotOwner, Vector3>();

	[NonSerialized]
	public List<T> List_0 = new List<T>();

	[NonSerialized]
	public Dictionary<BotOwner, bool> Dictionary_1 = new Dictionary<BotOwner, bool>();

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3? Nullable_0;

	public bool EnoughtHaveGoodCovers
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public Vector3? PointForBoss
	{
		[CompilerGenerated]
		get
		{
			return Nullable_0;
		}
		[CompilerGenerated]
		set
		{
			Nullable_0 = value;
		}
	}

	public abstract event Action OnPositionsRecalculated;

	public GClass445(BotOwner owner, BotBoss bossLogic)
		: base(owner, bossLogic)
	{
		EnoughtHaveGoodCovers = true;
	}

	public override void Activate()
	{
		BotOwner_0.Boss.OnFollowerStatusChange += method_1;
		Dictionary_1.Add(BotOwner_0, value: true);
		method_0();
	}

	public void SetHaveGoodCover(BotOwner id, bool noGoodCover)
	{
		Dictionary_1[id] = noGoodCover;
		int num = 0;
		foreach (KeyValuePair<BotOwner, bool> item in Dictionary_1)
		{
			if (item.Value)
			{
				num++;
			}
		}
		EnoughtHaveGoodCovers = (float)num > (float)Dictionary_1.Count / 2f;
	}

	public Dictionary<BotOwner, Vector3> FollowersPositions()
	{
		return Dictionary_0;
	}

	public void SetFollowersPositions()
	{
		if (BotOwner_0.Brain.BaseBrain is U val && PointForBoss.HasValue)
		{
			val.SetCorePosition(PointForBoss.Value);
		}
		foreach (KeyValuePair<BotOwner, Vector3> item in Dictionary_0)
		{
			if (item.Key.Brain.BaseBrain is T val2)
			{
				val2.SetCorePosition(item.Value);
			}
		}
	}

	public abstract Vector3 GetTargetToLook();

	public abstract void SetLogicToFollower(T followerLogic);

	public abstract void FindPositionsForFollowers(bool b);

	public virtual void SubAddFollower(BotOwner botOwner)
	{
	}

	public virtual void SubRemoveFollower(BotOwner botOwner)
	{
	}

	public void method_0()
	{
		foreach (BotOwner follower in BotOwner_0.Boss.Followers)
		{
			if (follower.Brain is T logicToFollower)
			{
				SetLogicToFollower(logicToFollower);
			}
		}
	}

	public void method_1(BotOwner botFollower, FollowerStatusChange arg2)
	{
		T val = botFollower.Brain.BaseBrain as T;
		if (arg2 == FollowerStatusChange.Add)
		{
			if (!Dictionary_0.ContainsKey(botFollower))
			{
				Dictionary_1.Add(botFollower, value: true);
				if (val != null)
				{
					SetLogicToFollower(val);
					List_0.Add(val);
				}
				Dictionary_0.Add(botFollower, Vector3.zero);
				FindPositionsForFollowers(b: true);
				SetFollowersPositions();
				SubAddFollower(botFollower);
			}
		}
		else
		{
			if (val != null)
			{
				SetLogicToFollower(val);
				List_0.Remove(val);
			}
			Dictionary_0.Remove(botFollower);
			SetFollowersPositions();
			SubRemoveFollower(botFollower);
		}
		FindPositionsForFollowers(b: true);
	}

	public override void Dispose()
	{
		if (BotOwner_0.Boss != null)
		{
			BotOwner_0.Boss.OnFollowerStatusChange -= method_1;
		}
	}
}
