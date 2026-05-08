using System;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using UnityEngine;

public class GClass110 : GClass109
{
	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public float Float_10;

	[NonSerialized]
	public float Float_11;

	[NonSerialized]
	public GClass453<GClass441> Gclass453_0;

	[NonSerialized]
	public float Float_12;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public float Float_13;

	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public const float Float_14 = 5f;

	[NonSerialized]
	public const float Float_15 = 1f;

	[NonSerialized]
	public float Float_16;

	[NonSerialized]
	public const float Float_17 = 1f;

	public GClass110(BotOwner bot, int priority)
		: base(bot, priority)
	{
		_ = BotOwner_0.WeaponManager.Selector.CanChangeToMeleeWeapons;
		Gclass453_0 = new GClass453<GClass441>(bot);
		Gclass453_0.FindBoss();
		BotOwner_0.Memory.OnGoalEnemyChanged += method_19;
	}

	public void method_19(BotOwner obj)
	{
		if (obj == null)
		{
			Bool_4 = false;
			Gclass453_0.BossLogic.AssaultCanKill = false;
		}
		else if (Float_11 < Time.time)
		{
			Bool_4 = false;
			Gclass453_0.BossLogic.AssaultCanKill = false;
		}
	}

	public override void Dispose()
	{
		BotOwner_0.Memory.OnGoalEnemyChanged -= method_19;
		base.Dispose();
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (Time.time < Float_16 + 1f)
		{
			if (BotOwner_0.Memory.GoalEnemy.CanShoot)
			{
				return method_15("shtFar1");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToEnemy, "gteFar1");
		}
		if (Bool_4)
		{
			return method_16(BotOwner_0.Memory.GoalEnemy, "atkf1");
		}
		global::AICoreActionResultStruct<BotLogicDecision, GClass26>? aICoreActionResultStruct = InFightLogic();
		if (aICoreActionResultStruct.HasValue)
		{
			if (aICoreActionResultStruct.Value.Action == BotLogicDecision.shootFromCover)
			{
				method_24();
			}
			return aICoreActionResultStruct.Value;
		}
		return method_13();
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		data.PlaceInfo = Gclass453_0.BossLogic.AreaId;
		if (CustomNavigationPoint_0 != null && (!CustomNavigationPoint_0.IsFreeById(BotOwner_0.Id) || CustomNavigationPoint_0.IsSpotted))
		{
			CustomNavigationPoint_0 = null;
		}
		if (CustomNavigationPoint_0 != null)
		{
			return CustomNavigationPoint_0;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null)
		{
			Vector3 centerPos = (BotOwner_0.Position + goalEnemy.CurrPosition) * 0.5f;
			data.CenterPos = centerPos;
		}
		else
		{
			data.CenterPos = BotOwner_0.Position;
		}
		data.SearchType = CoverSearchType.distToToCenter;
		CustomNavigationPoint customNavigationPoint = base.FindPoint(data, p, checkCurrent);
		if (customNavigationPoint == null)
		{
			data.CheckShootHide = ECheckSHootHide.hide;
			data.ArrayType = PointsArrayType.covers;
			customNavigationPoint = base.FindPoint(data, p, checkCurrent);
		}
		return customNavigationPoint;
	}

	public void UpdateLastFarShotTime()
	{
		Float_16 = Time.time;
		if (BotOwner_0.Brain.BaseBrain.CurLayerInfo == this)
		{
			BotOwner_0.Brain.BaseBrain.CalcActionNextFrame();
		}
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.Memory.HaveEnemy;
	}

	public override string Name()
	{
		return "KolontayFight";
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		AICoreActionEndStruct result = method_20();
		if (result.Value)
		{
			Gclass453_0.BossLogic.SecutiryMovingClose = false;
		}
		return result;
	}

	public AICoreActionEndStruct method_20()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("InCover");
		}
		if (!BotOwner_0.CanSprintPlayer)
		{
			return new AICoreActionEndStruct("CanSprintPl");
		}
		if (method_14())
		{
			Float_4 = Time.time;
			return new AICoreActionEndStruct("CvrNtFnd");
		}
		return AICoreActionEndStruct_1;
	}

	public void method_21()
	{
		if (!(Float_13 > Time.time) && BotOwner_0.Memory.IsInCover && !(Time.time - BotOwner_0.Memory.ComeToCoverTime < 2f))
		{
			ShootPointClass shootPointClass = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
			Vector3 pos = ((shootPointClass != null) ? ((shootPointClass.Point + BotOwner_0.Position) * 0.5f) : BotOwner_0.Position);
			Float_13 = 3f + Time.time;
			CustomNavigationPoint_0 = BotOwner_0.Covers.GetClosestPoint(pos, (GroupPoint point) => !point.IsSpotted && point.IsFreeById(BotOwner_0.Id) && ((!Gclass453_0.BossLogic.AreaId.HasValue || point.PlaceId == Gclass453_0.BossLogic.AreaId.Value) ? true : false));
			if (CustomNavigationPoint_0 != null && (!CustomNavigationPoint_0.IsFreeById(BotOwner_0.Id) || CustomNavigationPoint_0.IsSpotted))
			{
				CustomNavigationPoint_0 = null;
			}
		}
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		BotOwner_0.BotLight.Stroboscope.EnableFor(1f);
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return new AICoreActionEndStruct("enemynull");
		}
		if (method_23(fromDesicion: false, out var info) && Float_8 < Time.time)
		{
			return new AICoreActionEndStruct(info + "2");
		}
		if (!goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("!enemy.CanS");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndShootFromCover()
	{
		BotOwner_0.BotLight.Stroboscope.EnableFor(1f);
		if (method_23(fromDesicion: false, out var info) && Float_8 < Time.time)
		{
			return new AICoreActionEndStruct(info + "1");
		}
		return base.EndShootFromCover();
	}

	public CoverSearchData method_22()
	{
		int num = 75;
		ShootPointClass shootPointClass = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		Vector3 centerPos = ((shootPointClass != null) ? ((shootPointClass.Point + BotOwner_0.Position) * 0.5f) : BotOwner_0.Position);
		int num2 = num * num;
		return new CoverSearchData(centerPos, BotOwner_0.CoverSearchInfo, CoverShootType.hide, num2, 0f, CoverSearchType.distToToCenter, shootPointClass, null, null, ECheckSHootHide.hide, new CoverSearchDefenceDataClass(-1f));
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.CanShoot && goalEnemy.IsVisible)
		{
			return new AICoreActionEndStruct("sgt");
		}
		method_21();
		float num = 999f;
		if (BotOwner_0.Memory.IsInCover)
		{
			num = Time.time - BotOwner_0.Memory.ComeToCoverTime;
			if (!BotOwner_0.Memory.CurCustomCoverPoint.IsFreeById(BotOwner_0.Id))
			{
				return new AICoreActionEndStruct("notFree");
			}
		}
		if (!BotOwner_0.Memory.IsInCover && method_12(5f))
		{
			return new AICoreActionEndStruct("hitted");
		}
		if (num > 6f)
		{
			bool flag = Vector3.Dot(goalEnemy.Person.LookDirection, goalEnemy.Direction) > 0f;
			float num2 = Mathf.Abs(BotOwner_0.Position.y - goalEnemy.CurrPosition.y);
			if (method_23(fromDesicion: false, out var info))
			{
				return new AICoreActionEndStruct(info + "3");
			}
			if (flag && num2 < 1f && CustomNavigationPoint_0 != BotOwner_0.Memory.CurCustomCoverPoint && Time.time - goalEnemy.GroupInfo.EnemyLastSeenTimeReal < 5f)
			{
				Gclass453_0.BossLogic.SecutiryMovingClose = true;
				BotOwner_0.Memory.Spotted(byHit: false);
				return new AICoreActionEndStruct("moveToNext");
			}
		}
		return AICoreActionEndStruct_1;
	}

	public bool method_23(bool fromDesicion, out string info)
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.Distance > 15f)
		{
			info = null;
			return false;
		}
		if (method_17())
		{
			info = null;
			return false;
		}
		if (goalEnemy.Distance > 5f && Time.time - BotOwner_0.Memory.LastTimeHit < 1f)
		{
			info = null;
			return false;
		}
		if (Mathf.Abs(BotOwner_0.Position.y - goalEnemy.CurrPosition.y) > 1f)
		{
			info = null;
			return false;
		}
		bool num = Vector3.Dot(goalEnemy.Person.LookDirection, goalEnemy.Direction) > 0f && Time.time - goalEnemy.GroupInfo.EnemyLastSeenTimeReal < 5f;
		bool flag = false;
		string info2 = null;
		if (num)
		{
			info2 = "Dot";
			flag = true;
		}
		if (!flag && WantMeleeAssault(out info2))
		{
			flag = true;
		}
		if (flag)
		{
			if (!fromDesicion)
			{
				method_24();
				method_25();
			}
			info = info2;
			return true;
		}
		info = null;
		return false;
	}

	public override void ManualUpdate()
	{
		base.ManualUpdate();
	}

	public void method_24()
	{
		Gclass453_0.BossLogic.AssaultCanKill = true;
	}

	public override AICoreActionEndStruct EndOneMeleeAttack()
	{
		return base.EndOneMeleeAttack();
	}

	public void method_25()
	{
		Gclass453_0.BossLogic.AssaultCanKill = true;
		Bool_4 = true;
		Float_11 = Time.time + 60f;
	}

	public bool WantMeleeAssault(out string info)
	{
		if (Float_12 > Time.time)
		{
			info = String_0;
			return Bool_5;
		}
		Float_12 = Time.time + 1f;
		if (BotOwner_0.WeaponManager.Reload.Reloading)
		{
			String_0 = (info = "MeReload");
			return false;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && !Gclass459_0.EnemySprintingNow())
		{
			if (Time.time - goalEnemy.GroupInfo.EnemyLastSeenTimeReal > 5f)
			{
				String_0 = (info = "tooLate");
				return false;
			}
			Player alivePlayerByProfileID = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(goalEnemy.Person.ProfileId);
			Player.FirearmController firearmController = alivePlayerByProfileID.HandsController as Player.FirearmController;
			if (firearmController != null && firearmController.IsInReloadOperation())
			{
				Bool_5 = true;
				String_0 = (info = "ReloadOp");
				return Bool_5;
			}
			if (alivePlayerByProfileID.HandsController as Player.MedsController != null)
			{
				Bool_5 = true;
				String_0 = (info = "Meds");
				return Bool_5;
			}
			if (alivePlayerByProfileID.HandsController.IsInInteraction())
			{
				Bool_5 = true;
				info = "Interact";
				return Bool_5;
			}
			if (firearmController != null && firearmController.IsInventoryOpen())
			{
				Bool_5 = true;
				String_0 = (info = "Interact");
				return Bool_5;
			}
			String_0 = (info = "noVal2");
			return false;
		}
		String_0 = (info = "sprint");
		return false;
	}

	[CompilerGenerated]
	public bool method_26(GroupPoint point)
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
