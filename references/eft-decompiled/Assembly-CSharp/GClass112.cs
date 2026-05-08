using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass112 : GClass108
{
	[NonSerialized]
	public GClass453<GClass441> Gclass453_0;

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	public GClass1881<BotLogicDecision> Gclass1881_0 = new GClass1881<BotLogicDecision>(new KeyValuePair<BotLogicDecision, float>(BotLogicDecision.suppressFire, 1f), new KeyValuePair<BotLogicDecision, float>(BotLogicDecision.attackMovingWithSuppress, 1f));

	[NonSerialized]
	public GClass1881<BotLogicDecision> Gclass1881_1 = new GClass1881<BotLogicDecision>(new KeyValuePair<BotLogicDecision, float>(BotLogicDecision.suppressFire, 1f), new KeyValuePair<BotLogicDecision, float>(BotLogicDecision.attackMovingWithSuppress, 1f), new KeyValuePair<BotLogicDecision, float>(BotLogicDecision.runToEnemy, 1f));

	[NonSerialized]
	public GClass1881<BotLogicDecision> Gclass1881_2 = new GClass1881<BotLogicDecision>(new KeyValuePair<BotLogicDecision, float>(BotLogicDecision.suppressFire, 1f), new KeyValuePair<BotLogicDecision, float>(BotLogicDecision.runToEnemy, 1f));

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_4;

	[NonSerialized]
	[CompilerGenerated]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	[CompilerGenerated]
	public CustomNavigationPoint CustomNavigationPoint_1;

	public bool HaveCoverToShoot
	{
		[CompilerGenerated]
		get
		{
			return Bool_4;
		}
		[CompilerGenerated]
		set
		{
			Bool_4 = value;
		}
	}

	public CustomNavigationPoint _coverInMiddle
	{
		[CompilerGenerated]
		get
		{
			return CustomNavigationPoint_0;
		}
		[CompilerGenerated]
		set
		{
			CustomNavigationPoint_0 = value;
		}
	}

	public CustomNavigationPoint _coverForAttack
	{
		[CompilerGenerated]
		get
		{
			return CustomNavigationPoint_1;
		}
		[CompilerGenerated]
		set
		{
			CustomNavigationPoint_1 = value;
		}
	}

	public GClass112(BotOwner bot, int priority)
		: base(bot, priority)
	{
		Gclass453_0 = new GClass453<GClass441>(bot);
	}

	public override string Name()
	{
		return "SecurityKln";
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		global::AICoreActionResultStruct<BotLogicDecision, GClass26>? aICoreActionResultStruct = InFightLogic();
		if (aICoreActionResultStruct.HasValue)
		{
			return aICoreActionResultStruct.Value;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (Gclass453_0.BossLogic.SecutiryMovingClose)
		{
			method_18();
			BotLogicDecision botLogicDecision = ((Gclass453_0.BossLogic.DistanceToEnemy > 20f) ? Gclass1881_0.Random() : ((_coverInMiddle == BotOwner_0.Memory.CurCustomCoverPoint) ? Gclass1881_2.Random() : Gclass1881_1.Random()));
			if (botLogicDecision == BotLogicDecision.runToEnemy && goalEnemy.IsVisible)
			{
				return method_15("jf1");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(botLogicDecision, "rnd");
		}
		if (Gclass453_0.BossLogic.AssaultCanKill)
		{
			if (goalEnemy.IsVisible)
			{
				return method_15("jf2");
			}
			method_16();
			if (_coverForAttack != null && _coverForAttack.IsFreeById(BotOwner_0.Id) && !BotOwner_0.Memory.IsInCover)
			{
				BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(_coverForAttack);
				if (GClass856.IsTrue100(40f) && goalEnemy.Distance > (_coverForAttack.Position - BotOwner_0.Position).magnitude)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "a56TC");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "accRTC");
			}
			if (GClass856.IsTrue100(50f))
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToEnemy, "spr1");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToEnemy, "spr2");
		}
		if ((BotOwner_0.Medecine.FirstAid.Have2Do || BotOwner_0.Medecine.SurgicalKit.HaveWork) && BotOwner_0.Memory.IsInCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "heal");
		}
		return method_13();
	}

	public override AICoreActionEndStruct EndGoToEnemy()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && (!goalEnemy.IsVisible || !goalEnemy.CanShoot))
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("endNN");
	}

	public override AICoreActionEndStruct EndDogFight()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return new AICoreActionEndStruct("enemynull");
		}
		if (!goalEnemy.IsVisible)
		{
			return new AICoreActionEndStruct("!visible");
		}
		return AICoreActionEndStruct_1;
	}

	public void method_16()
	{
		if (Float_7 > Time.time)
		{
			return;
		}
		float num = Time.time - BotOwner_0.Memory.ComeToCoverTime;
		if (!BotOwner_0.Memory.IsInCover || !(num < 1f))
		{
			CoverSearchData coverSearchData = method_17();
			coverSearchData.UseSelfFindPoint = false;
			Float_7 = 3f + Time.time;
			_coverForAttack = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(coverSearchData, checkCurrent: false);
			if (_coverForAttack != null && (!_coverForAttack.IsFreeById(BotOwner_0.Id) || _coverForAttack.IsSpotted))
			{
				_coverForAttack = null;
			}
		}
	}

	public CoverSearchData method_17()
	{
		int num = 75;
		ShootPointClass shootPointClass = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		Vector3 centerPos = ((shootPointClass != null) ? ((shootPointClass.Point + BotOwner_0.Position) * 0.5f) : BotOwner_0.Position);
		int num2 = num * num;
		return new CoverSearchData(centerPos, BotOwner_0.CoverSearchInfo, CoverShootType.shoot, num2, 0f, CoverSearchType.distToBot, shootPointClass, null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(-1f))
		{
			PlaceInfo = Gclass453_0.BossLogic.AreaId
		};
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (Gclass453_0.BossLogic.AssaultCanKill)
		{
			if (_coverForAttack != null && (!_coverForAttack.IsFreeById(BotOwner_0.Id) || _coverForAttack.IsSpotted))
			{
				_coverForAttack = null;
			}
			if (_coverForAttack != null)
			{
				return _coverForAttack;
			}
		}
		if (_coverInMiddle != null && (!_coverInMiddle.IsFreeById(BotOwner_0.Id) || _coverInMiddle.IsSpotted))
		{
			_coverInMiddle = null;
		}
		if (_coverInMiddle != null)
		{
			return _coverInMiddle;
		}
		data.CenterPos = method_20();
		CustomNavigationPoint closestPoint = BotOwner_0.Covers.GetClosestPoint(data.CenterPos, (GroupPoint point) => !point.IsSpotted && point.IsFreeById(BotOwner_0.Id) && ((!Gclass453_0.BossLogic.AreaId.HasValue || point.PlaceId == Gclass453_0.BossLogic.AreaId.Value) ? true : false));
		if (closestPoint != null)
		{
			return closestPoint;
		}
		return base.FindPoint(data, p, checkCurrent);
	}

	public override bool ShallUseNow()
	{
		Gclass453_0.FindBoss();
		if (BotOwner_0.Memory.HaveEnemy)
		{
			return Gclass453_0.HaveLogic();
		}
		return false;
	}

	public void method_18()
	{
		if (!(Float_8 < Time.time) || !BotOwner_0.BotFollower.HaveBoss)
		{
			return;
		}
		if (_coverInMiddle != null && (!_coverInMiddle.IsFreeById(BotOwner_0.Id) || _coverInMiddle.IsSpotted))
		{
			_coverInMiddle = null;
		}
		CoverSearchData data = method_19();
		Float_8 = 3f + Time.time;
		CustomNavigationPoint coverPointMain = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: false);
		if (BotOwner_0.Memory.CurCustomCoverPoint == null)
		{
			_coverInMiddle = coverPointMain;
		}
		else if (coverPointMain != null)
		{
			Vector3 positionOrTargetCover = BotOwner_0.BotFollower.BossToFollow.PositionOrTargetCover;
			float sqrMagnitude = (BotOwner_0.Memory.CurCustomCoverPoint.Position - positionOrTargetCover).sqrMagnitude;
			if ((coverPointMain.Position - positionOrTargetCover).sqrMagnitude < sqrMagnitude)
			{
				_coverInMiddle = coverPointMain;
			}
		}
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return new AICoreActionEndStruct("enemynull");
		}
		if (!goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("!Cs");
		}
		if (!goalEnemy.IsVisible)
		{
			return new AICoreActionEndStruct("!Vs");
		}
		return AICoreActionEndStruct_1;
	}

	public CoverSearchData method_19()
	{
		ShootPointClass shoot2point = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		return new CoverSearchData(method_20(), BotOwner_0.CoverSearchInfo, CoverShootType.hide, 5625f, 0f, CoverSearchType.distToToCenter, shoot2point, null, null, ECheckSHootHide.hide, new CoverSearchDefenceDataClass(-1f))
		{
			PlaceInfo = Gclass453_0.BossLogic.AreaId
		};
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("InCover");
		}
		if (method_14())
		{
			Float_4 = Time.time;
			return new AICoreActionEndStruct("CvrNtFnd");
		}
		return AICoreActionEndStruct_1;
	}

	public override void OnActivate()
	{
		Gclass453_0.FindBoss();
		base.OnActivate();
	}

	public override AICoreActionEndStruct EndSuppressFire()
	{
		if (!Gclass453_0.BossLogic.SecutiryMovingClose)
		{
			return new AICoreActionEndStruct("stp");
		}
		return base.EndSuppressFire();
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		method_18();
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		if (!BotOwner_0.Memory.CurCustomCoverPoint.IsFreeById(BotOwner_0.Id))
		{
			return new AICoreActionEndStruct("notFree");
		}
		if (_coverInMiddle != null && _coverInMiddle != BotOwner_0.Memory.CurCustomCoverPoint)
		{
			BotOwner_0.Memory.Spotted(byHit: false);
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(_coverInMiddle);
			return new AICoreActionEndStruct("haveCOver");
		}
		if (Gclass453_0.BossLogic.SecutiryMovingClose)
		{
			return new AICoreActionEndStruct("SMC1");
		}
		if (Gclass453_0.BossLogic.AssaultCanKill)
		{
			return new AICoreActionEndStruct("ACC1");
		}
		if (goalEnemy != null && goalEnemy.IsVisible)
		{
			return new AICoreActionEndStruct("CanShoot");
		}
		if (!BotOwner_0.Medecine.FirstAid.Have2Do && !BotOwner_0.Medecine.SurgicalKit.HaveWork)
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("ShallHeal");
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		return AICoreActionEndStruct;
	}

	public Vector3 method_20()
	{
		if (BotOwner_0.BotFollower.HaveBoss)
		{
			return BotOwner_0.BotFollower.BossToFollow.PositionOrTargetCover;
		}
		return BotOwner_0.Position;
	}

	public override void Dispose()
	{
		Gclass453_0.Dispose();
		base.Dispose();
	}

	[CompilerGenerated]
	public bool method_21(GroupPoint point)
	{
		if (!point.IsSpotted && point.IsFreeById(BotOwner_0.Id))
		{
			if (Gclass453_0.BossLogic.AreaId.HasValue && point.PlaceId != Gclass453_0.BossLogic.AreaId.Value)
			{
				return false;
			}
			return true;
		}
		return false;
	}
}
