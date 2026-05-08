using System;
using Comfort.Common;
using EFT;
using UnityEngine;

public class GClass78 : GClass76
{
	[NonSerialized]
	public new ShootPointClass ShootPointClass;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public const float Float_7 = 60f;

	[NonSerialized]
	public const float Float_8 = 40f;

	[NonSerialized]
	public EnemyInfo EnemyInfo_0;

	[NonSerialized]
	public float Float_9;

	[NonSerialized]
	public GClass483 Gclass483_0;

	[NonSerialized]
	public GClass483 Gclass483_1;

	[NonSerialized]
	public GClass483 Gclass483_2;

	public GClass78(BotOwner bot, int priority)
		: base(bot, priority)
	{
		BotOwner_0.BotsGroup.OnMemberRemove += method_20;
		BotOwner_0.Memory.OnGoalEnemyChanged += method_18;
		EnemyInfo_0 = BotOwner_0.Memory.GoalEnemy;
		Gclass483_1 = BotOwner_0.FindPlaceToShoot.Register(Int_2, Int_1, 0.5f);
		Gclass483_2 = BotOwner_0.FindPlaceToShoot.Register((int)((float)Int_2 * 0.7f), (int)((float)Int_1 * 0.5f), 0.65f);
		Gclass483_0 = Gclass483_1;
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		return method_13(replaceShoot: true, Gclass483_0, ShootPointClass);
	}

	public void Init(Vector3 pointToControl)
	{
		Bool_4 = true;
		if (!Bool_5)
		{
			Bool_5 = true;
			BotOwner_0.Memory.OnBulletNear += method_21;
		}
		ShootPointClass = new ShootPointClass(pointToControl);
		if (!Gclass483_1.IsShootToThis(ShootPointClass))
		{
			Gclass483_1.Drop();
		}
		if (!Gclass483_2.IsShootToThis(ShootPointClass))
		{
			Gclass483_2.Drop();
		}
		Float_9 = Time.time;
		ShootPointClass = new ShootPointClass(pointToControl);
	}

	public override bool ShallUseNow()
	{
		if (BotOwner_0.Memory.HaveEnemy)
		{
			return Bool_4;
		}
		return false;
	}

	public override string Name()
	{
		return "BirdHold";
	}

	public override AICoreActionEndStruct EndGoToPoint()
	{
		if (BotOwner_0.GoToSomePointData.IsCome())
		{
			return new AICoreActionEndStruct("Come");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndShootFromCover()
	{
		return new AICoreActionEndStruct("now1");
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		return new AICoreActionEndStruct("now");
	}

	public float method_17()
	{
		if (base.ShootPointClass != null)
		{
			return (base.ShootPointClass.Point - ShootPointClass.Point).sqrMagnitude;
		}
		return 0f;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		float num = Time.time - goalEnemy.GroupInfo.EnemyLastSeenTimeReal;
		if (goalEnemy.Distance < 40f)
		{
			method_22();
			return new AICoreActionEndStruct("endDist");
		}
		if (num > 60f)
		{
			return AICoreActionEndStruct_1;
		}
		if (Gclass483_0.LastGoodPoint.HasValue)
		{
			if ((Gclass483_0.LastGoodPoint.Value - BotOwner_0.Position).sqrMagnitude < 4f)
			{
				return AICoreActionEndStruct_1;
			}
			return new AICoreActionEndStruct("notClose7");
		}
		method_15(GetShootPoint());
		if (CustomNavigationPoint_0 != null && CustomNavigationPoint_0.CanIShootToEnemy)
		{
			if (method_17() > 9f)
			{
				CustomNavigationPoint_0 = null;
				return new AICoreActionEndStruct("tooFar");
			}
			if ((CustomNavigationPoint_0.Position - BotOwner_0.Position).sqrMagnitude > 2f)
			{
				return new AICoreActionEndStruct("wrongC");
			}
		}
		if (CustomNavigationPoint_0 == null || (CustomNavigationPoint_0 != null && !CustomNavigationPoint_0.CanIShootToEnemy))
		{
			if (Gclass483_1.ManualUpdateSearch(GetShootPoint(), Int_1, out var point))
			{
				Gclass483_0 = Gclass483_1;
				return new AICoreActionEndStruct("havePointFr");
			}
			if (Gclass483_2.ManualUpdateSearch(GetShootPoint(), (float)Int_1 * 0.5f, out point))
			{
				Gclass483_0 = Gclass483_2;
				return new AICoreActionEndStruct("havePointCl");
			}
		}
		return AICoreActionEndStruct_1;
	}

	public override void ManualUpdate()
	{
		base.ManualUpdate();
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		return base.EndRunToCover();
	}

	public override ShootPointClass GetShootPoint()
	{
		return ShootPointClass;
	}

	public void method_18(BotOwner obj)
	{
		if (EnemyInfo_0 != null)
		{
			Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(EnemyInfo_0.Person.ProfileId).OnInventoryOpened -= method_19;
		}
		EnemyInfo_0 = BotOwner_0.Memory.GoalEnemy;
		if (EnemyInfo_0 != null)
		{
			Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(EnemyInfo_0.Person.ProfileId).OnInventoryOpened += method_19;
		}
	}

	public void method_19(Player player, bool obj)
	{
		if (Bool_4 && obj && (ShootPointClass.Point - player.Position).magnitude < 2f)
		{
			method_22();
		}
	}

	public void method_20(BotOwner obj)
	{
		if (BotOwner_0.BotsGroup.MembersCount == 1)
		{
			Init(obj.Position);
		}
	}

	public void method_21(BotOwner bot, IPlayer source)
	{
		method_22();
	}

	public void method_22()
	{
		if (Bool_4)
		{
			Bool_4 = false;
		}
	}

	public override void Dispose()
	{
		if (EnemyInfo_0 != null)
		{
			Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(EnemyInfo_0.Person.ProfileId).OnInventoryOpened -= method_19;
		}
		BotOwner_0.BotsGroup.OnMemberRemove -= method_20;
		BotOwner_0.Memory.OnBulletNear -= method_21;
		BotOwner_0.Memory.OnGoalEnemyChanged -= method_18;
		base.Dispose();
	}
}
