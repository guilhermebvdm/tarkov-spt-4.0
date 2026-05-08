using System;
using EFT;
using UnityEngine;

public abstract class GClass76 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public const float Float_3 = 30f;

	[NonSerialized]
	public int Int_1 = 40;

	[NonSerialized]
	public int Int_2 = 60;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public ShootPointClass ShootPointClass;

	public GClass76(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_13(bool replaceShoot, GClass483 findPlaceToShoot, ShootPointClass shootToPoint)
	{
		if (CustomNavigationPoint_0 == null || !CustomNavigationPoint_0.CanIShootToEnemy)
		{
			method_15(GetShootPoint());
		}
		_ = BotOwner_0.Memory.GoalEnemy;
		if (BotOwner_0.Memory.IsInCover)
		{
			if (BotOwner_0.Memory.CurCustomCoverPoint == CustomNavigationPoint_0 && BotOwner_0.Memory.CurCustomCoverPoint.CanIShootToEnemy)
			{
				if ((CustomNavigationPoint_0.Position - BotOwner_0.Position).sqrMagnitude > 2f)
				{
					BotOwner_0.Memory.CheckIsInCover2();
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "еtoCov");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "waitFroEnemy");
			}
			if (CustomNavigationPoint_0 != null && CustomNavigationPoint_0.CanIShootToEnemy)
			{
				float sqrMagnitude = (CustomNavigationPoint_0.Position - BotOwner_0.Position).sqrMagnitude;
				BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(CustomNavigationPoint_0);
				if (sqrMagnitude > 2f)
				{
					BotOwner_0.Memory.CheckIsInCover2();
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "run2Hold");
				}
			}
			if (findPlaceToShoot.LastGoodPoint.HasValue)
			{
				Vector3 firePos = findPlaceToShoot.LastGoodPoint.Value + BotOwner_0.ShootData.WeaponRootOffset;
				if (!GClass369.CanShootToTarget(shootToPoint, firePos, BotOwner_0.LookSensor.Mask))
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "wait4point");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToPoint, "2some", new GClass30(findPlaceToShoot.LastGoodPoint.Value));
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "attck1");
		}
		if (findPlaceToShoot.LastGoodPoint.HasValue)
		{
			float sqrMagnitude2 = (findPlaceToShoot.LastGoodPoint.Value - BotOwner_0.Position).sqrMagnitude;
			Vector3 firePos2 = findPlaceToShoot.LastGoodPoint.Value + BotOwner_0.ShootData.WeaponRootOffset;
			if (GClass369.CanShootToTarget(shootToPoint, firePos2, BotOwner_0.LookSensor.Mask))
			{
				if (sqrMagnitude2 < 2f)
				{
					if (replaceShoot)
					{
						return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "2some");
					}
					return method_14("2some");
				}
			}
			else
			{
				findPlaceToShoot.Drop();
			}
		}
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(CustomNavigationPoint_0);
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "noInCover");
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_14(string cause)
	{
		Float_6 = Time.time + 5f;
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, cause);
	}

	public void method_15(ShootPointClass curShootPoint)
	{
		if (CustomNavigationPoint_0 != null && ShootPointClass != null && (ShootPointClass.Point - curShootPoint.Point).sqrMagnitude > 1f)
		{
			ShootPointClass = null;
			CustomNavigationPoint_0 = null;
		}
		if (!(Float_4 > Time.time))
		{
			Float_4 = Time.time + 3f;
			method_16(curShootPoint);
		}
	}

	public abstract ShootPointClass GetShootPoint();

	public void method_16(ShootPointClass posibleTarget)
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return;
		}
		Vector3 vector = goalEnemy.CurrPosition - Int_2 * GClass855.NormalizeFast(goalEnemy.Direction);
		_ = BotOwner_0.Memory.IsInCover;
		CoverSearchData coverSearchData = new CoverSearchData(vector, BotOwner_0.CoverSearchInfo, CoverShootType.shoot, 2500f, 0f, CoverSearchType.distToBot, posibleTarget, null, null, ECheckSHootHide.shoot, new CoverSearchDefenceDataClass(0f));
		coverSearchData.ArrayType = PointsArrayType.allWithBush;
		coverSearchData.MinSDistToCarePos = 1600f;
		CustomNavigationPoint_0 = null;
		CustomNavigationPoint customNavigationPoint = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(coverSearchData, checkCurrent: false);
		if (customNavigationPoint != null)
		{
			bool num = (goalEnemy.CurrPosition - customNavigationPoint.Position).sqrMagnitude < (float)(Int_1 * Int_1);
			Debug.DrawRay(customNavigationPoint.Position, Vector3.up * 10f, Color.green, 2f);
			if (num)
			{
				customNavigationPoint = BotOwner_0.Covers.GetFreeClosePoint(vector, Int_1);
				Debug.DrawRay(customNavigationPoint.Position, Vector3.up * 10f, Color.yellow, 2f);
			}
			else
			{
				Debug.DrawRay(customNavigationPoint.Position, Vector3.up * 20f, Color.green, 2f);
			}
			CustomNavigationPoint_0 = customNavigationPoint;
			if (CustomNavigationPoint_0 != null && CustomNavigationPoint_0.CanIShootToEnemy)
			{
				ShootPointClass = posibleTarget;
			}
		}
	}

	public override AICoreActionEndStruct EndLayNode()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (method_3())
		{
			return new AICoreActionEndStruct("StartD");
		}
		if (!BotOwner_0.BotLay.IsLay)
		{
			return new AICoreActionEndStruct("!Lay");
		}
		if (goalEnemy != null && !goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("!CanShoot");
		}
		return AICoreActionEndStruct_1;
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (CustomNavigationPoint_0 != null)
		{
			return CustomNavigationPoint_0;
		}
		return base.FindPoint(data, p, checkCurrent);
	}
}
