using System;
using EFT;
using UnityEngine;

public class GClass81 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public string String_0 = "StationaryWS";

	[NonSerialized]
	public const float Float_3 = 3f;

	[NonSerialized]
	public float Float_4;

	public AbstractSuppressStationary AbstractSuppressStationary_0 => BotOwner_0.SuppressStationary.CurUsingLogic;

	public GClass81(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (!BotOwner_0.WeaponManager.Stationary.IsClose())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToStationary, "runTo");
		}
		if (AbstractSuppressStationary_0.IsReady())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.suppressStationary, "suppStat");
		}
		if (BotOwner_0.WeaponManager.Stationary.Taken)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromStationary, "haveEnemy1");
		}
		BotOwner_0.WeaponManager.Stationary.Take();
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromStationary, "TakeSt");
	}

	public override bool ShallUseNow()
	{
		if (AbstractSuppressStationary_0.Usable && method_14())
		{
			return method_13();
		}
		return false;
	}

	public bool method_13()
	{
		if (Float_4 > Time.time)
		{
			return false;
		}
		if (!method_12(5f))
		{
			return true;
		}
		if (BotOwner_0.Memory.GoalEnemy == null)
		{
			return false;
		}
		if (Time.time - BotOwner_0.ShootData.LastTriggerPressd > 6f)
		{
			Float_4 = Time.time + 120f;
			return false;
		}
		return true;
	}

	public bool method_14()
	{
		if (BotOwner_0.WeaponManager.Stationary.CurLink == null)
		{
			return false;
		}
		if (!BotOwner_0.WeaponManager.Stationary.CurLink.IsFree(BotOwner_0.Id))
		{
			return false;
		}
		if (!BotOwner_0.WeaponManager.Stationary.CheckAmmonProcess())
		{
			return false;
		}
		return true;
	}

	public override string Name()
	{
		return String_0;
	}

	public override AICoreActionEndStruct EndRunToStationary()
	{
		return base.EndRunToStationary();
	}

	public override AICoreActionEndStruct EndSuppressStationary()
	{
		StationaryWeaponLink curLink = BotOwner_0.WeaponManager.Stationary.CurLink;
		if (curLink == null)
		{
			return new AICoreActionEndStruct("linkNull");
		}
		if (!curLink.HaveAmmo())
		{
			return new AICoreActionEndStruct("no ammo");
		}
		if (curLink.IsFree(BotOwner_0.Id))
		{
			return new AICoreActionEndStruct("not free");
		}
		if (AbstractSuppressStationary_0.IsReady())
		{
			if (Time.time - AbstractSuppressStationary_0.StartArtSupress > 3f && BotOwner_0.SuppressStationary.HaveEnemy() && Time.time - BotOwner_0.SuppressStationary.LastHitTargetTime > 3f)
			{
				AbstractSuppressStationary_0.StopExternal(BotOwner_0.SuppressStationary.PointToArtSuppress());
				return new AICoreActionEndStruct("noHit");
			}
			EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
			if (goalEnemy != null && goalEnemy.CanShoot)
			{
				return new AICoreActionEndStruct("seeEnemy");
			}
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("stopSup");
	}

	public override AICoreActionEndStruct EndShootFromStationary()
	{
		StationaryWeaponLink curLink = BotOwner_0.WeaponManager.Stationary.CurLink;
		if (curLink == null)
		{
			return new AICoreActionEndStruct("linkNull");
		}
		if (!curLink.HaveAmmo())
		{
			return new AICoreActionEndStruct("no ammo");
		}
		if (!curLink.IsFree(BotOwner_0.Id))
		{
			return new AICoreActionEndStruct("notMy");
		}
		if (AbstractSuppressStationary_0.IsReady() && AbstractSuppressStationary_0.CanStartSupressEnemy(BotOwner_0.Memory.GoalEnemy))
		{
			return AICoreActionEndStruct;
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return new AICoreActionEndStruct("no enemy");
		}
		if (AbstractSuppressStationary_0.IsReady() && AbstractSuppressStationary_0.CanStartSupressEnemy(BotOwner_0.Memory.GoalEnemy))
		{
			return new AICoreActionEndStruct("want take stationary");
		}
		if (goalEnemy.CanShoot && goalEnemy.IsVisible)
		{
			return new AICoreActionEndStruct("seeEnemy");
		}
		return AICoreActionEndStruct_1;
	}
}
