using System;
using EFT;
using UnityEngine;

public class GClass118 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public const float Float_3 = 25f;

	[NonSerialized]
	public const float Float_4 = 0.25f;

	[NonSerialized]
	public float Float_5;

	public GClass118(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.WeaponManager.Reload.Reloading)
		{
			BotOwner_0.WeaponManager.Reload.Reload();
		}
		if (Float_5 + 25f < Time.time)
		{
			Float_5 = Time.time;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "hold");
		}
		if (!BotOwner_0.Memory.IsInCover && !GClass856.IsTrue100(BotOwner_0.Settings.FileSettings.Shoot.REPAIR_MALFUNCTION_IMMEDIATE_CHANCE))
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "gotocover");
		}
		Float_5 = 0f;
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.repairMalfunction, "repair");
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.WeaponManager.Malfunctions.HaveMalfunction();
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (Float_5 + 0.25f < Time.time)
		{
			return new AICoreActionEndStruct("HoldExpired");
		}
		return AICoreActionEndStruct_1;
	}

	public override string GetCustomData()
	{
		return BotOwner_0.WeaponManager.Malfunctions.GetCustomBotInfoData();
	}

	public override string Name()
	{
		return "Malfunction";
	}

	public override AICoreActionEndStruct EndDogFight()
	{
		if (BotOwner_0.WeaponManager.Malfunctions.HaveMalfunction())
		{
			return new AICoreActionEndStruct("HaveMalf");
		}
		return base.EndDogFight();
	}

	public override AICoreActionEndStruct EndAttackMoving()
	{
		if (BotOwner_0.WeaponManager.Malfunctions.HaveMalfunction())
		{
			return new AICoreActionEndStruct("HaveMalf");
		}
		return base.EndAttackMoving();
	}

	public override AICoreActionEndStruct EndShootFromCover()
	{
		if (BotOwner_0.WeaponManager.Malfunctions.HaveMalfunction())
		{
			return new AICoreActionEndStruct("HaveMalf");
		}
		return base.EndShootFromCover();
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		if (BotOwner_0.WeaponManager.Malfunctions.HaveMalfunction())
		{
			return new AICoreActionEndStruct("HaveMalf");
		}
		return base.EndShootFromPlace();
	}

	public override AICoreActionEndStruct EndShootToSmoke()
	{
		if (BotOwner_0.WeaponManager.Malfunctions.HaveMalfunction())
		{
			return new AICoreActionEndStruct("HaveMalf");
		}
		return base.EndShootToSmoke();
	}

	public override AICoreActionEndStruct EndSuppressFire()
	{
		if (BotOwner_0.WeaponManager.Malfunctions.HaveMalfunction())
		{
			return new AICoreActionEndStruct("HaveMalf");
		}
		return base.EndSuppressFire();
	}

	public override AICoreActionEndStruct EndRepairMalfunction()
	{
		if (!BotOwner_0.WeaponManager.Malfunctions.HaveMalfunction())
		{
			return new AICoreActionEndStruct("!HaveMalf");
		}
		return base.EndRepairMalfunction();
	}

	public override AICoreActionEndStruct EndGoToEnemy()
	{
		if (BotOwner_0.WeaponManager.Malfunctions.HaveMalfunction())
		{
			return new AICoreActionEndStruct("HaveMalf");
		}
		return base.EndGoToEnemy();
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("InCover");
		}
		if (!BotOwner_0.CanSprintPlayer)
		{
			return new AICoreActionEndStruct("CanSprintPl");
		}
		if (BotOwner_0.Memory.CurCustomCoverPoint != null && BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
		{
			return new AICoreActionEndStruct("IsSpotted");
		}
		return AICoreActionEndStruct_1;
	}
}
