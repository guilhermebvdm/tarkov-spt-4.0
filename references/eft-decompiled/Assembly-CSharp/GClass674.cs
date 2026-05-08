using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass674
{
	[NonSerialized]
	public IPlayer Iplayer_0;

	[NonSerialized]
	public IPlayer Iplayer_1;

	[NonSerialized]
	public List<BotOwner> List_0 = new List<BotOwner>();

	[NonSerialized]
	public GClass25 Gclass25_0;

	[NonSerialized]
	public Action<GClass674> Action_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public LocationSettingsClass.Location.GClass1425 Gclass1425_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	public IPlayer Target => Iplayer_0;

	public Vector3 CallerPosition => Iplayer_1.Position;

	public Vector3 CurPosition => Iplayer_0.Position;

	public Vector3 TargetLastSeenPosition
	{
		get
		{
			if (Iplayer_1.AIData.BotOwner.Memory.HaveEnemy)
			{
				return Iplayer_1.AIData.BotOwner.Memory.GoalEnemy.EnemyLastPositionReal;
			}
			return Iplayer_0.Position;
		}
	}

	public float Single_0 => Gclass1425_0.ZombieCallRadiusLimit;

	public float Single_1 => Gclass1425_0.ZombieCallDeltaRadius;

	public bool Disposed
	{
		[CompilerGenerated]
		get
		{
			return Bool_1;
		}
		[CompilerGenerated]
		set
		{
			Bool_1 = value;
		}
	}

	public bool TargetAllowToSpawnCrowd => Bool_0;

	public GClass674(IPlayer curTarget, BotOwner zombieCaller, Action<GClass674> OnStop, LocationSettingsClass.Location.GClass1425 halloweenParams)
	{
		Iplayer_0 = curTarget;
		Iplayer_1 = zombieCaller.GetPlayer;
		Iplayer_1.OnIPlayerDeadOrUnspawn += FindNextCallerOrDisposePursuit;
		Action_0 = OnStop;
		Gclass1425_0 = halloweenParams;
		Bool_0 = method_0(curTarget);
		Iplayer_0.OnIPlayerDeadOrUnspawn += method_3;
		Gclass25_0 = new GClass25(Gclass1425_0.ZombieCallPeriodSec, method_1);
	}

	public bool method_0(IPlayer trg)
	{
		if (trg.IsAI && trg.AIData.BotOwner.Profile.Info.Settings.Role != WildSpawnType.pmcBEAR)
		{
			return trg.AIData.BotOwner.Profile.Info.Settings.Role == WildSpawnType.pmcUSEC;
		}
		return true;
	}

	public bool Contains(IPlayer bot)
	{
		foreach (BotOwner item in List_0)
		{
			if (item.GetPlayer.ProfileId == bot.ProfileId)
			{
				return true;
			}
		}
		return false;
	}

	public void method_1()
	{
		Float_0 += Single_1;
		if (Float_0 >= Single_0)
		{
			Float_0 = Single_0;
		}
		float num = Float_0 * Float_0;
		if (!Iplayer_1.HealthController.IsAlive || !Iplayer_1.AIData.BotOwner.Memory.HaveEnemy || Iplayer_1.AIData.BotOwner.Memory.GoalEnemy.Person != Iplayer_0)
		{
			FindNextCallerOrDisposePursuit(Iplayer_1);
			if (Disposed)
			{
				return;
			}
		}
		foreach (BotOwner item in List_0)
		{
			if (item.HealthController.IsAlive && !(GClass856.SqrDistance(Iplayer_0.Position, item.Position) > num))
			{
				item.GameEventsData.HalloweenGameEvent.Call(TargetLastSeenPosition, num, this);
			}
		}
	}

	public void FindNextCallerOrDisposePursuit(IPlayer obj)
	{
		foreach (BotOwner item in List_0)
		{
			if (item != null && item.GetPlayer.HealthController.IsAlive && item.Memory.GoalEnemy != null && item.Memory.GoalEnemy.Person == Target)
			{
				Iplayer_1 = item;
				return;
			}
		}
		method_5();
	}

	public void ManualUpdate()
	{
		Gclass25_0.Update();
	}

	public void AddZombie(BotOwner bot)
	{
		List_0.Add(bot);
		bot.OnIPlayerDeadOrUnspawn += method_2;
	}

	public void method_2(IPlayer obj)
	{
		if (obj.AIData.BotOwner != null)
		{
			BotOwner botOwner = obj.AIData.BotOwner;
			List_0.Remove(botOwner);
			botOwner.OnIPlayerDeadOrUnspawn -= method_2;
		}
	}

	public void method_3(IPlayer obj)
	{
		if (!method_4())
		{
			method_5();
		}
	}

	public bool method_4()
	{
		return false;
	}

	public void method_5()
	{
		foreach (BotOwner item in List_0)
		{
			item.OnIPlayerDeadOrUnspawn -= method_2;
		}
		Action_0?.Invoke(this);
		Disposed = true;
	}

	public bool ReadyToSpawn()
	{
		return Float_0 >= Single_0;
	}
}
