using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass597 : BotRequest
{
	[CompilerGenerated]
	public class Class282
	{
		public BotOwner requester;

		public bool method_0(KeyValuePair<IPlayer, BotSettingsClass> x)
		{
			if (x.Value.IsHaveSeen)
			{
				return (x.Value.EnemyLastPosition - requester.Transform.position).sqrMagnitude < 2500f;
			}
			return false;
		}
	}

	[NonSerialized]
	public const int Int_0 = 15;

	[NonSerialized]
	public const int Int_1 = 2500;

	[NonSerialized]
	public GClass602 Gclass602_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public int Int_2;

	[NonSerialized]
	public int Int_3;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public float Float_0;

	public override EBotRequestMode RequestMode => EBotRequestMode.Fight;

	public GClass597(Player requester, EnemyInfo info)
		: base(requester, BotRequestType.suppressionFire)
	{
		DisposeOtherRequestsWhenTaken = true;
		Gclass602_0 = new GClass602(info);
	}

	public GClass597(Player requester, Vector3 point)
		: base(requester, BotRequestType.suppressionFire)
	{
		Gclass602_0 = new GClass602(point);
	}

	public override bool CanStartExecute(BotOwner executor)
	{
		if (Gclass602_0.EnemyByRequester == null)
		{
			return false;
		}
		return base.CanStartExecute(executor);
	}

	public override void Activate()
	{
		base.Activate();
		if (Requester.AIData.IsAI)
		{
			Requester.AIData.BotOwner.Memory.OnGoalEnemyChanged += method_0;
		}
	}

	public override bool CanRequest(BotOwner requester)
	{
		if (requester.Memory.GoalEnemy == null)
		{
			return false;
		}
		if (!requester.Memory.IsInCover && !requester.BotLay.IsLay)
		{
			return false;
		}
		if (requester.BotsGroup.Enemies.Count((KeyValuePair<IPlayer, BotSettingsClass> x) => x.Value.IsHaveSeen && (x.Value.EnemyLastPosition - requester.Transform.position).sqrMagnitude < 2500f) > 1)
		{
			return false;
		}
		return true;
	}

	public override bool CanProceed()
	{
		EnemyInfo goalEnemy = Executor.Memory.GoalEnemy;
		if (goalEnemy != null && goalEnemy.CanShoot && goalEnemy.IsVisible)
		{
			return false;
		}
		return true;
	}

	public override void PlayerDestroy(Player player)
	{
		if (Gclass602_0.IsTarget(player))
		{
			Dispose();
		}
	}

	public override void Take(BotOwner executor)
	{
		base.Take(executor);
		executor.SuppressShoot.Init(Gclass602_0.EnemyByRequester);
		Int_3 = (int)GClass856.GreateRandom(15);
		executor.ShootData.OnTriggerPressed += method_1;
	}

	public ShootPointClass GetShootPos()
	{
		return new ShootPointClass(Gclass602_0.ShootPos(), 0.8f);
	}

	public bool CanShootNow()
	{
		if (Float_0 < Time.time)
		{
			Float_0 = Time.time + 1f;
			Bool_1 = GClass369.CanShootToTarget(GetShootPos(), Executor.WeaponRoot.position, Executor.LookSensor.Mask);
		}
		return Bool_1;
	}

	public void method_0(BotOwner obj)
	{
		if (obj.Memory.GoalEnemy == null)
		{
			Complete();
		}
	}

	public void method_1()
	{
		if (!Bool_0)
		{
			Bool_0 = true;
			Executor.BotTalk.TrySay(EPhraseTrigger.Covering, withGroupDelay: true);
		}
		Int_2++;
		if (Int_2 > Int_3)
		{
			Complete();
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		if (Requester.AIData.IsAI)
		{
			Requester.AIData.BotOwner.Memory.OnGoalEnemyChanged -= method_0;
		}
		if (Executor != null)
		{
			Executor.ShootData.OnTriggerPressed -= method_1;
		}
	}
}
