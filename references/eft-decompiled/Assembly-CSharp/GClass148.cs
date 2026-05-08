using System;
using EFT;
using UnityEngine;

public class GClass148 : GClass146
{
	[NonSerialized]
	public const float Float_41 = 15f;

	public GClass148(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override bool ShallUseNow()
	{
		if (Ginterface7_0 != null && BotOwner_0.BotFollower.HaveBoss)
		{
			return BotOwner_0.Memory.HaveEnemy;
		}
		return false;
	}

	public override string Name()
	{
		return "FlSanFight";
	}

	public override CustomNavigationPoint FindPointForFight(bool checkCurrent)
	{
		if (CustomNavigationPoint_0 == null)
		{
			checkCurrent = false;
		}
		ShootPointClass shootPointClass = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		if (CustomNavigationPoint_0 != null)
		{
			if (!CustomNavigationPoint_0.IsFreeById(BotOwner_0.Id))
			{
				checkCurrent = false;
			}
			else if (!GClass369.CanShootToTarget(shootPointClass, CustomNavigationPoint_0, BotOwner_0.LookSensor.Mask))
			{
				checkCurrent = false;
			}
		}
		Vector3 vector = ((!BotOwner_0.BotFollower.HaveBoss) ? BotOwner_0.Position : BotOwner_0.BotFollower.BossToFollow.Position);
		CoverShootType shootType = CoverShootType.shoot;
		if (shootPointClass == null)
		{
			shootType = CoverShootType.hide;
		}
		PointsArrayType arrayType = (method_14() ? PointsArrayType.covers : PointsArrayType.both);
		float maxDistSqr = (Bool_5 ? 256f : 900f);
		int value = (Bool_5 ? 6 : 14);
		CoverSearchData coverSearchData = new GClass384(15f, vector, BotOwner_0, shootType, maxDistSqr, 0f, CoverSearchType.distToBot, shootPointClass, null, vector, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(0f), PointsArrayType.both, useSelfFindPoint: true, null, value);
		coverSearchData.UseSelfFindPoint = checkCurrent;
		coverSearchData.ArrayType = arrayType;
		coverSearchData.UseLineCastToCover = true;
		CustomNavigationPoint coverPointMain = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(coverSearchData, checkCurrent);
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(coverPointMain);
		Bool_4 = true;
		return coverPointMain;
	}

	public void SetCorePosition(Vector3 followersPositionValue)
	{
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (Bool_5 && method_17() && method_15())
		{
			Vector3 vector = method_16();
			if ((BotOwner_0.Position - vector).sqrMagnitude > Float_24)
			{
				return new AICoreActionEndStruct("FollwoerEnd");
			}
		}
		return base.EndHoldPosition();
	}
}
