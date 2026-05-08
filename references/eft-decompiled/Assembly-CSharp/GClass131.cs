using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass131 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public GClass453<GClass442> Gclass453_0;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public bool Bool_5;

	public const int MORE_ENEMIES_THAN = 1;

	[NonSerialized]
	public const int Int_1 = 61;

	public GClass131(BotOwner bot, int priority)
		: base(bot, priority)
	{
		Gclass453_0 = new GClass453<GClass442>(BotOwner_0);
		BotOwner_0.MinesData.OnMinesStartCache += method_15;
		BotOwner_0.MinesData.OnMinesCacheCompleted += method_14;
	}

	public override void Dispose()
	{
		BotOwner_0.MinesData.OnMinesStartCache -= method_15;
		BotOwner_0.MinesData.OnMinesCacheCompleted -= method_14;
		base.Dispose();
	}

	public override AICoreActionEndStruct EndMoveStealthy()
	{
		if (CustomNavigationPoint_0 != null && (CustomNavigationPoint_0.Position - BotOwner_0.Position).sqrMagnitude < 1f)
		{
			return new AICoreActionEndStruct("cclt");
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		if (CustomNavigationPoint_0 != null && (CustomNavigationPoint_0.Position - BotOwner_0.Position).sqrMagnitude < 1f)
		{
			return new AICoreActionEndStruct("hloeq1");
		}
		return base.EndGoToCoverPoint();
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (BotOwner_0.Medecine.FirstAid.Have2Do && BotOwner_0.Memory.IsInCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "heal");
		}
		method_13();
		if (!BotOwner_0.Memory.IsInCover && CustomNavigationPoint_0 != null)
		{
			BotOwner_0.Memory.Spotted(byHit: false);
			BotOwner_0.Memory.SetCoverPoints(CustomNavigationPoint_0);
			if (goalEnemy.Distance > 50f)
			{
				Bool_5 = !Bool_5;
				if (Bool_5)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.moveStealthy, "jh77");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToCoverPoint, "mk89");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.moveStealthy, "moveTl");
		}
		if (goalEnemy.CanShoot && goalEnemy.IsVisible)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "gg");
		}
		float num = BotOwner_0.SuppressShoot.DeltaLastSupperss();
		BotOwner_0.BotTalk.Say(EPhraseTrigger.Toxic, sayImmediately: true);
		if (num > 61f)
		{
			BotOwner_0.SuppressShoot.Init(goalEnemy);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.suppressFire, "spr");
		}
		HoldFor(61f);
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "hld");
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (method_7())
		{
			return new AICoreActionEndStruct("EndHol");
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("CanShoot");
		}
		if (goalEnemy.IsVisible && goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Cover.END_HOLD_IF_ENEMY_CLOSE_AND_VISIBLE)
		{
			return new AICoreActionEndStruct("CLOSEANDVIS");
		}
		return AICoreActionEndStruct_1;
	}

	public void method_13()
	{
		if (!(Time.time - Float_4 < 6f))
		{
			AIMinePoint closestsPlanted = BotOwner_0.MinesData.GetClosestsPlanted(BotOwner_0.Position);
			Vector3 pos;
			if (BotOwner_0.Memory.HaveEnemy && closestsPlanted != null)
			{
				Vector3 vector = GClass855.NormalizeFastSelf(closestsPlanted.Position - BotOwner_0.Memory.GoalEnemy.CurrPosition);
				pos = closestsPlanted.Position + vector * 20f;
			}
			else
			{
				pos = BotOwner_0.Position;
			}
			CustomNavigationPoint customNavigationPoint = null;
			customNavigationPoint = BotOwner_0.Covers.GetClosestPoint(pos, (GroupPoint x) => !x.IsSpotted && x.IsFreeById(BotOwner_0.Id));
			CustomNavigationPoint_0 = customNavigationPoint;
			Float_4 = Time.time;
		}
	}

	public void method_14()
	{
		Bool_4 = true;
	}

	public void method_15()
	{
		Bool_4 = false;
	}

	public override bool ShallUseNow()
	{
		if (!Bool_4)
		{
			return false;
		}
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			return false;
		}
		if (BotOwner_0.Memory.GoalEnemy.Person.BtrState == EPlayerBtrState.Inside)
		{
			return false;
		}
		if (!Gclass453_0.HaveLogic())
		{
			return false;
		}
		if (Gclass453_0.BossLogic.EnemiesAtClosestsZone() > 1)
		{
			Float_3 = Time.time + 20f;
			return true;
		}
		if (Float_3 > Time.time)
		{
			return true;
		}
		return false;
	}

	public override string Name()
	{
		return "PrtMany";
	}

	[CompilerGenerated]
	public bool method_16(GroupPoint x)
	{
		if (!x.IsSpotted)
		{
			return x.IsFreeById(BotOwner_0.Id);
		}
		return false;
	}
}
