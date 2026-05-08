using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass90 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	[CompilerGenerated]
	public GClass435 Gclass435_0;

	public GClass435 GClass435_0
	{
		[CompilerGenerated]
		get
		{
			return Gclass435_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass435_0 = value;
		}
	}

	public GClass90(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public void SetBoss(GClass435 gluhar)
	{
		GClass435_0 = gluhar;
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.Medecine.FirstAid.Have2Do)
		{
			if (BotOwner_0.Memory.IsInCover)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "heal");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToCoverPoint, "cover_heal");
		}
		if (GClass435_0.TrainSecure)
		{
			if (BotOwner_0.Memory.IsInCover)
			{
				CustomNavigationPoint curCustomCoverPoint = BotOwner_0.Memory.CurCustomCoverPoint;
				if (Float_3 <= 0f)
				{
					float gOOD_DIST_TO_POINT_COEF = BotOwner_0.Settings.FileSettings.Cover.GOOD_DIST_TO_POINT_COEF;
					float num = LocalBotSettingsProviderClass.Core.GOOD_DIST_TO_POINT * gOOD_DIST_TO_POINT_COEF;
					float float_ = num * num;
					Float_3 = float_;
				}
				bool flag = (curCustomCoverPoint.Position - Vector3_0).sqrMagnitude < Float_3;
				bool flag2 = (curCustomCoverPoint.Position - BotOwner_0.Position).sqrMagnitude < 4f;
				if (flag && flag2)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "holdTargetL");
				}
				CustomNavigationPoint closestPoint = BotOwner_0.Covers.GetClosestPoint(Vector3_0);
				if (closestPoint != null)
				{
					BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(closestPoint);
					float sqrMagnitude = (BotOwner_0.Position - closestPoint.Position).sqrMagnitude;
					Float_3 = sqrMagnitude + 1f;
					CustomNavigationPoint_0 = closestPoint;
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "runTargetL");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "v4");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "last");
	}

	public override bool ShallUseNow()
	{
		if (BotOwner_0.Memory.HaveGoal && GClass435_0 != null)
		{
			return BotOwner_0.BotFollower.HaveBoss;
		}
		return false;
	}

	public override string Name()
	{
		return "GlGoal";
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("cover");
		}
		if (goalEnemy != null && goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("VisibleCanS");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		if (goalEnemy != null)
		{
			if (goalEnemy.IsVisible && goalEnemy.CanShoot)
			{
				return new AICoreActionEndStruct("CanShoot");
			}
			if (goalEnemy.IsVisible && goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Cover.END_HOLD_IF_ENEMY_CLOSE_AND_VISIBLE)
			{
				return new AICoreActionEndStruct("ENEMYCLOSE");
			}
		}
		return AICoreActionEndStruct_1;
	}
}
