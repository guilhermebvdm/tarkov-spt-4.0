using System;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using UnityEngine;

public class GClass133 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public float Float_3;

	public GClass133(BotOwner bot, int priority)
		: base(bot, priority)
	{
		Float_3 = Time.time + 5f;
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (method_13(out var aiCoreActionResult))
		{
			return aiCoreActionResult;
		}
		BotOwner_0.PatrollingData.SetTargetMoveSpeed();
		BotOwner_0.PatrollingData.PointChooser.ShallChangeWay();
		PatrolWay way;
		if (BotOwner_0.BotFollower.HaveBoss)
		{
			way = BotOwner_0.BotFollower.BossToFollow.PatrollingData.Way;
		}
		else
		{
			if (BotOwner_0.PatrollingData.Way == null)
			{
				BotOwner_0.PatrollingData.PointChooser.ChooseStartWay();
			}
			way = BotOwner_0.PatrollingData.Way;
		}
		if (!BotOwner_0.Boss.IamBoss && BotOwner_0.BotFollower.HaveBoss)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.followerPatrol, "BossFollow");
		}
		if (way != null && way.PatrolType == PatrolType.reserved && BotOwner_0.Settings.FileSettings.Patrol.CAN_CHOOSE_RESERV)
		{
			BotOwner_0.PatrollingData.ComeToPoint();
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.alternativePatrol, "RESER");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.simplePatrol, "Basic");
	}

	public bool method_13(out global::AICoreActionResultStruct<BotLogicDecision, GClass26> aiCoreActionResult)
	{
		if (!BotOwner_0.Medecine.FirstAid.Have2Do && !BotOwner_0.Medecine.SurgicalKit.HaveWork)
		{
			if (BotOwner_0.SmokeGrenade.IsInSmoke)
			{
				aiCoreActionResult = new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToCoverPoint, "PeaceSmoke");
				return true;
			}
			if (BotOwner_0.PeaceHardAim.HaveActions())
			{
				aiCoreActionResult = new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.peaceHardAim, "PeaceHardAi");
				return true;
			}
			if (BotOwner_0.PeaceLook.HaveActions())
			{
				aiCoreActionResult = new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.peaceLook, "PeaceLook");
				return true;
			}
			if (BotOwner_0.SecondWeaponData.HaveActions())
			{
				aiCoreActionResult = new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.watchSecondWeapon, "Look2ndWeap");
				return true;
			}
			if (!method_15())
			{
				if (BotOwner_0.FriendlyTilt.HaveActions())
				{
					aiCoreActionResult = new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.friendlyTilt, "FriendlyTil");
					return true;
				}
				if (BotOwner_0.EatDrinkData.HaveActions())
				{
					aiCoreActionResult = new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.eatDrink, "EatDrinkDat");
					return true;
				}
				if (BotOwner_0.PatrollingData.LootData.HaveActions() && BotOwner_0.Mover.HasPathAndNoComplete)
				{
					aiCoreActionResult = new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToLootPointNode, "LootTime");
					return true;
				}
				if (BotOwner_0.SecondWeaponData.HaveActions())
				{
					aiCoreActionResult = new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.watchSecondWeapon, "Look2ndWeap");
					return true;
				}
				if (BotOwner_0.Gesture.HaveRequest())
				{
					aiCoreActionResult = new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.gesture, "Gesture");
					return true;
				}
				if (BotOwner_0.PeacefulActions.HaveActions())
				{
					aiCoreActionResult = new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.peaceful, "Peaceful");
					return true;
				}
			}
			aiCoreActionResult = Gstruct8_0;
			return false;
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			aiCoreActionResult = new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "first aid");
			return true;
		}
		if (method_12(20f))
		{
			aiCoreActionResult = new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "goforheal");
			return true;
		}
		aiCoreActionResult = new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "heal now");
		return true;
	}

	public override void ManualUpdate()
	{
		if (Float_3 >= Time.time)
		{
			return;
		}
		BotUnderbarrelLauncherController underbarrelLauncherController = BotOwner_0.WeaponManager.UnderbarrelLauncherController;
		if (underbarrelLauncherController.NeedToReload())
		{
			Float_3 = Time.time + GClass856.Random(4f, 6f);
			if (underbarrelLauncherController.IsActive)
			{
				underbarrelLauncherController.TryReload(delegate
				{
					method_14();
				});
			}
			else
			{
				underbarrelLauncherController.TryEnableReloadDisable(method_14);
			}
		}
		else
		{
			method_14();
		}
	}

	public void method_14()
	{
		if ((float)BotOwner_0.WeaponManager.Reload.BulletCount / (float)BotOwner_0.WeaponManager.Reload.MaxBulletCount < 0.6f)
		{
			Float_3 = Time.time + GClass856.Random(4f, 6f);
			BotOwner_0.WeaponManager.Reload.TryReload();
		}
	}

	public bool method_15()
	{
		if (!BotOwner_0.Boss.IamBoss)
		{
			return BotOwner_0.BotFollower.HaveBoss;
		}
		return true;
	}

	public override AICoreActionEndStruct EndLayNode()
	{
		return AICoreActionEndStruct;
	}

	public override string Name()
	{
		return "PatrolAssault";
	}

	public override AICoreActionEndStruct EndWatchSecondWeapon()
	{
		if (BotOwner_0.SecondWeaponData.HaveActions())
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override bool ShallUseNow()
	{
		BotOwner_0.PriorityAxeTarget.FindTarget();
		return true;
	}

	public override AICoreActionEndStruct EndHeal()
	{
		if (!BotOwner_0.Medecine.FirstAid.Have2Do)
		{
			return new AICoreActionEndStruct("EndHeal");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndSimplePatrol()
	{
		if (method_16(out var reason))
		{
			return new AICoreActionEndStruct(reason);
		}
		if (BotOwner_0.PatrollingData.Way.PatrolType == PatrolType.reserved)
		{
			if (!BotOwner_0.Settings.FileSettings.Patrol.CAN_CHOOSE_RESERV)
			{
				BotOwner_0.Settings.FileSettings.Patrol.CAN_CHOOSE_RESERV = true;
			}
			return new AICoreActionEndStruct("way is alt");
		}
		if (BotOwner_0.BotFollower.HaveBoss && !BotOwner_0.Boss.IamBoss)
		{
			return new AICoreActionEndStruct("new boss");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndPeaceful()
	{
		if (BotOwner_0.PeacefulActions.HaveActions())
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndAlternativePatrol()
	{
		if (method_16(out var reason))
		{
			return new AICoreActionEndStruct(reason);
		}
		if (BotOwner_0.PatrollingData.Way.PatrolType == PatrolType.reserved)
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("no alt way");
	}

	public override AICoreActionEndStruct EndFollowerPatrolItem()
	{
		if (BotOwner_0.Boss.IamBoss)
		{
			return new AICoreActionEndStruct("IamBoss");
		}
		if (!BotOwner_0.BotFollower.HaveBoss)
		{
			return new AICoreActionEndStruct("no boss");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndSearch()
	{
		return AICoreActionEndStruct;
	}

	public bool method_16(out string reason)
	{
		if (!method_15())
		{
			if (BotOwner_0.EatDrinkData.HaveActions())
			{
				reason = "eatDrink";
				return true;
			}
			if (BotOwner_0.FriendlyTilt.HaveActions())
			{
				reason = "FriendlyTil";
				return true;
			}
			if (BotOwner_0.Gesture.HaveRequest())
			{
				reason = "Gesture";
				return true;
			}
			if (BotOwner_0.SecondWeaponData.HaveActions())
			{
				reason = "Watch2ndWeap";
				return true;
			}
			if (BotOwner_0.PeacefulActions.HaveActions())
			{
				reason = "PeacefulAction";
				return true;
			}
			if (BotOwner_0.PatrollingData.LootData.HaveActions() && BotOwner_0.Mover.HasPathAndNoComplete)
			{
				reason = "LootData";
				return true;
			}
		}
		if (BotOwner_0.PeaceLook.HaveActions())
		{
			reason = "peaceLook";
			return true;
		}
		if (!BotOwner_0.Medecine.FirstAid.Have2Do && !BotOwner_0.Medecine.SurgicalKit.HaveWork)
		{
			reason = null;
			return false;
		}
		reason = "mustHeal";
		return true;
	}

	public override AICoreActionEndStruct EndGoToLootPointNode()
	{
		if (!BotOwner_0.PatrollingData.LootData.HaveActions())
		{
			return new AICoreActionEndStruct("noLootD");
		}
		return AICoreActionEndStruct_1;
	}

	[CompilerGenerated]
	public void method_17(IResult _)
	{
		method_14();
	}
}
