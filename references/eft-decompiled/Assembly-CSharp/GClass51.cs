using System;
using EFT;
using UnityEngine;

public abstract class GClass51 : GClass50
{
	[NonSerialized]
	public bool Bool_5 = true;

	[NonSerialized]
	public float Float_3 = 3600f;

	[NonSerialized]
	public float Float_4 = 14f;

	[NonSerialized]
	public float Float_5 = 15f;

	[NonSerialized]
	public float Float_6 = 4f;

	[NonSerialized]
	public float Float_7 = 9f;

	[NonSerialized]
	public Vector3 Vector3_1 = Vector3.zero;

	[NonSerialized]
	public StationaryWeaponLink StationaryWeaponLink_0;

	[NonSerialized]
	public bool Bool_6;

	[NonSerialized]
	public bool Bool_7;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	public float Float_9;

	public GClass51(BotOwner bot, int priority, bool canUseStationary)
		: base(bot, priority)
	{
		method_15(Float_6, Float_7);
		Bool_5 = canUseStationary;
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.Medecine.FirstAid.Have2Do)
		{
			if (BotOwner_0.Memory.IsInCover)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "first aid");
			}
			if (method_12(20f))
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "goforheal");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "heal now");
		}
		if (Bool_5 && !BotOwner_0.Boss.IamBoss && Bool_6 && Float_9 < Time.time && StationaryWeaponLink_0 != null && BotOwner_0.WeaponManager.Stationary.CurLink != null && method_16(StationaryWeaponLink_0))
		{
			if (!BotOwner_0.WeaponManager.Stationary.IsClose())
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToStationary, "runToSt");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromStationary, "atStationary");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "holdOrCover");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "toC");
	}

	public override bool ShallUseNow()
	{
		return true;
	}

	public void method_15(float minDistToBoss, float maxDistToBoss)
	{
		if (!Bool_7)
		{
			if (BotOwner_0.Boss.IamBoss && minDistToBoss > 0f && maxDistToBoss > 0f)
			{
				float x = GClass856.Random(minDistToBoss, maxDistToBoss) * (float)GClass856.RandomSing();
				float z = GClass856.Random(minDistToBoss, maxDistToBoss) * (float)GClass856.RandomSing();
				Vector3_1 = new Vector3(x, 0f, z);
				Bool_7 = true;
			}
			else if (BotOwner_0.BotFollower.HaveBoss)
			{
				int index = BotOwner_0.BotFollower.Index;
				int followersTargetCount = BotOwner_0.BotFollower.FollowersTargetCount;
				float angDegree = (float)index / (float)followersTargetCount * 360f;
				Vector3 b = new Vector3(1f, 0f, 0f);
				Vector3_1 = GClass855.RotateOnAngUp(b, angDegree) * GClass856.GreateRandom(Float_5);
				Bool_7 = true;
			}
		}
	}

	public override void OnActivate()
	{
		method_17();
		base.OnActivate();
	}

	public override AICoreActionEndStruct EndShootFromStationary()
	{
		if (BotOwner_0.Medecine.FirstAid.Have2Do)
		{
			return new AICoreActionEndStruct("needHeal");
		}
		StationaryWeaponLink curLink = BotOwner_0.WeaponManager.Stationary.CurLink;
		if (curLink == null)
		{
			return new AICoreActionEndStruct("linkNull");
		}
		if (!curLink.HaveAmmo())
		{
			return new AICoreActionEndStruct("no ammo");
		}
		if (method_12(4f))
		{
			Float_9 = Time.time + 360f;
			return new AICoreActionEndStruct("get hit");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (BotOwner_0.Boss.IamBoss && method_12(4f))
		{
			BotOwner_0.Memory.Spotted(byHit: false);
			return new AICoreActionEndStruct("get hit");
		}
		if (BotOwner_0.Medecine.FirstAid.Have2Do)
		{
			return new AICoreActionEndStruct("needHeal");
		}
		if (method_7())
		{
			return new AICoreActionEndStruct("EndHol");
		}
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		if (BotOwner_0.WeaponManager.Stationary.HaveLink && method_16(StationaryWeaponLink_0))
		{
			return new AICoreActionEndStruct("stationary");
		}
		method_17();
		return AICoreActionEndStruct_1;
	}

	public bool method_16(StationaryWeaponLink link)
	{
		if (!Bool_5)
		{
			return false;
		}
		bool result = true;
		if (link != null && !link.CanTakeByPrevDeath(9999f))
		{
			result = false;
			if ((BotOwner_0.Memory.GoalEnemy != null && Time.time - BotOwner_0.Memory.GoalEnemy.GroupInfo.LastShootTime < 3f) || (BotOwner_0.Memory.LastEnemy != null && Time.time - BotOwner_0.Memory.LastEnemy.GroupInfo.LastShootTime < 3f))
			{
				result = true;
			}
		}
		return result;
	}

	public void method_17()
	{
		if (BotOwner_0.Boss.IamBoss || !Bool_5 || Float_8 > Time.time)
		{
			return;
		}
		Float_8 = Time.time + 14f;
		if (BotOwner_0.WeaponManager.Stationary.HaveLink || !BotOwner_0.WeaponManager.Stationary.TryFindClosests(Vector3_0, withSetTarget: false, out StationaryWeaponLink_0) || (Vector3_0 - StationaryWeaponLink_0.Position).sqrMagnitude > Float_3)
		{
			return;
		}
		if (BotOwner_0.WeaponManager.Stationary.CurLink == null)
		{
			if (!StationaryWeaponLink_0.IsFree(BotOwner_0.Id))
			{
				StationaryWeaponLink_0 = null;
				return;
			}
			BotOwner_0.WeaponManager.Stationary.SetTargetStationary(StationaryWeaponLink_0);
		}
		Bool_6 = true;
	}

	public override void ManualUpdate()
	{
		BotOwner_0.PatrollingData.TryToTalk(bigDelay: true);
		base.ManualUpdate();
	}
}
