using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass82 : GClass81
{
	[CompilerGenerated]
	public class Class212
	{
		public EnemyInfo enemy;

		public Vector3 method_0()
		{
			return enemy.CurrPosition;
		}
	}

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public GInterface8 Ginterface8_0;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public bool Bool_5;

	public GClass82(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public void method_15()
	{
		if (Float_5 > Time.time)
		{
			return;
		}
		Float_5 = Time.time + 2f;
		EnemyInfo enemy = BotOwner_0.Memory.GoalEnemy;
		if (enemy == null || (enemy.CanShoot && enemy.IsVisible) || base.AbstractSuppressStationary_0.IsReady() || !BotOwner_0.WeaponManager.Stationary.IsEnemyAtSector(BotOwner_0.WeaponManager.Stationary.CurLink, enemy.CurrPosition))
		{
			return;
		}
		float num = Time.time - enemy.TimeLastSeen;
		float endSuppressTime = BotOwner_0.SuppressStationary.CurUsingLogic.EndSuppressTime;
		float num2 = Time.time - endSuppressTime;
		if ((num < 20f && num > 4f && num2 > 6f) || method_17())
		{
			base.AbstractSuppressStationary_0.SetPeriod(GClass856.Random(3f, 5f));
			BotOwner_0.SuppressStationary.SetTargetGetter(() => enemy.CurrPosition);
		}
	}

	public override bool ShallUseNow()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return false;
		}
		if (goalEnemy.IsVisible)
		{
			return false;
		}
		StationaryWeaponLink curLink = BotOwner_0.WeaponManager.Stationary.CurLink;
		if (curLink == null)
		{
			return false;
		}
		if (!curLink.CanTakeByPrevDeath(9999f) && !BotOwner_0.WeaponManager.Stationary.Taken)
		{
			return false;
		}
		if (!base.AbstractSuppressStationary_0.IsInProcess && Vector3.Dot(goalEnemy.Direction, BotOwner_0.LookDirection) < 0f)
		{
			return false;
		}
		if (Time.time - goalEnemy.GroupInfo.EnemyLastSeenTimeReal < 20f && !method_17())
		{
			return false;
		}
		return base.ShallUseNow();
	}

	public override string Name()
	{
		return "BoarStationary";
	}

	public override void OnActivate()
	{
		base.OnActivate();
		if (!Bool_4 && Ginterface8_0 == null)
		{
			if (BotOwner_0.BotFollower.HaveBoss)
			{
				Ginterface8_0 = BotOwner_0.BotFollower.BossToFollow.GetBossLogic() as GInterface8;
				Bool_4 = true;
			}
			else if (!Bool_5)
			{
				Bool_5 = true;
				BotOwner_0.BotFollower.OnBossFinded += method_16;
			}
		}
	}

	public override void ManualUpdate()
	{
		method_15();
		base.ManualUpdate();
	}

	public override AICoreActionEndStruct EndSuppressStationary()
	{
		if (base.AbstractSuppressStationary_0.IsReady())
		{
			if (Time.time - base.AbstractSuppressStationary_0.StartArtSupress > 4f)
			{
				base.AbstractSuppressStationary_0.StopExternal(BotOwner_0.SuppressStationary.PointToArtSuppress());
				return new AICoreActionEndStruct("timeOut");
			}
			EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
			if (goalEnemy != null && goalEnemy.CanShoot)
			{
				return new AICoreActionEndStruct("seeEnemy");
			}
			return AICoreActionEndStruct_1;
		}
		return base.EndSuppressStationary();
	}

	public override AICoreActionEndStruct EndRunToEnemy()
	{
		return AICoreActionEndStruct;
	}

	public void method_16(IBossToFollow obj)
	{
		Ginterface8_0 = BotOwner_0.BotFollower.BossToFollow.GetBossLogic() as GInterface8;
		Bool_4 = true;
		BotOwner_0.BotFollower.OnBossFinded -= method_16;
	}

	public bool method_17()
	{
		if (Ginterface8_0 == null)
		{
			return false;
		}
		return Ginterface8_0.WantSuppress();
	}

	public override void Dispose()
	{
		BotOwner_0.BotFollower.OnBossFinded -= method_16;
		base.Dispose();
	}
}
