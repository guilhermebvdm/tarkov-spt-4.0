using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass170 : GClass168
{
	[CompilerGenerated]
	public class Class229
	{
		public GClass170 gclass170_0;

		public float sD;

		public bool method_0(GroupPoint x)
		{
			if (x.PlaceId != gclass170_0.Nullable_0.Value)
			{
				return false;
			}
			if ((x.Position - gclass170_0.BotOwner_0.Position).sqrMagnitude < sD)
			{
				return false;
			}
			return true;
		}
	}

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public float Float_3 = float.MinValue;

	[NonSerialized]
	public float Float_4 = float.MinValue;

	[NonSerialized]
	public float Float_5 = float.MinValue;

	[NonSerialized]
	public float Float_6 = 0.5f;

	[NonSerialized]
	public int Int_3;

	[NonSerialized]
	public int Int_4 = 10;

	[NonSerialized]
	public bool Bool_6;

	[NonSerialized]
	public List<CustomNavigationPoint> List_0 = new List<CustomNavigationPoint>();

	public float MIN_DIST_TO_TELEPORT => BotOwner_0.Settings.FileSettings.Boss.BOSS_ZRYACHIY_MIN_DIST_TO_TELEPORT;

	public float CHANCE_TELEPORT => BotOwner_0.Settings.FileSettings.Boss.BOSS_ZRYACHIY_TELEPORT_CHANCE;

	public float MIN_DIST_TO_NEXT_COVER => BotOwner_0.Settings.FileSettings.Boss.BOSS_ZRYACHIY_MIN_DIST_TO_NEXT_COVER;

	public float Single_1 => BotOwner_0.ShootData.LastTriggerPressd;

	public GClass170(BotOwner bot, int priority)
		: base(bot, priority)
	{
		BotOwner_0.BotPersonalStats.OnKillTarget += method_23;
		BotOwner_0.BotPersonalStats.OnHitTarget += method_22;
		foreach (AIPlaceInfo place in BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.AIPlaceInfoHolder.Places)
		{
			if (place.InfoLogicAllEnemy is AIPlaceInfoLogicZryachiy { ControllableZone: not false })
			{
				place.OnPlayerEnter += method_19;
				place.OnPlayerExit += method_20;
			}
		}
		List_0 = BotOwner_0.VoxelesPersonalData.Points((CustomNavigationPoint x) => !((x.Position - BotOwner_0.Position).sqrMagnitude > 22500f));
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (!Bool_4 && goalEnemy.CanShoot)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.lay, "CanSh");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			if (BotOwner_0.Medecine.FirstAid.Have2Do)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "heal now");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.lay, "inCover");
		}
		if (Bool_5)
		{
			Bool_5 = false;
			if (method_15())
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.teleportToCover, "tp2");
			}
			if (Time.time - BotOwner_0.ActivateTime < 15f)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "toC1");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.crawl, "crawl_TP");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.crawl, "crawl_1");
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		data.PlaceInfo = base.Nullable_0;
		if (base.Nullable_0.HasValue)
		{
			float sD = MIN_DIST_TO_NEXT_COVER * MIN_DIST_TO_NEXT_COVER;
			CustomNavigationPoint closestPoint = BotOwner_0.Covers.GetClosestPoint(BotOwner_0.Position, delegate(GroupPoint x)
			{
				if (x.PlaceId != base.Nullable_0.Value)
				{
					return false;
				}
				return !((x.Position - BotOwner_0.Position).sqrMagnitude < sD);
			});
			if (closestPoint != null)
			{
				return closestPoint;
			}
			List_0.Clear();
			if (List_0.Count > 0)
			{
				return GClass856.RandomElement(List_0);
			}
		}
		return base.FindPoint(data, p, checkCurrent);
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.Memory.HaveEnemy;
	}

	public override string Name()
	{
		return "ZryachiyFight";
	}

	public override AICoreActionEndStruct EndCrawlNode()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			if (Bool_4)
			{
				Bool_4 = false;
			}
			return new AICoreActionEndStruct("inCover");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndLayNode()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (!goalEnemy.IsVisible && Bool_6 && Time.time - goalEnemy.LastChangeVisionTime > 10f)
		{
			Bool_6 = false;
			method_21();
			return new AICoreActionEndStruct("loseVision");
		}
		if (method_24() && Time.time - Single_1 > 10f)
		{
			method_21();
			return new AICoreActionEndStruct("KillSoon");
		}
		if (BotOwner_0.Medecine.FirstAid.Have2Do && !goalEnemy.CanShoot)
		{
			method_21();
			return new AICoreActionEndStruct("needHeal");
		}
		float num = Time.time - Float_3;
		float num2 = Time.time - Single_1;
		if (num2 < 1f && num > num2 + 5f)
		{
			if (Float_5 < Time.time)
			{
				Float_5 = Time.time + Float_6;
				Int_3++;
				if (Int_3 >= Int_4)
				{
					method_21();
					return new AICoreActionEndStruct("hitChange");
				}
			}
		}
		else
		{
			Int_3 = 0;
		}
		return AICoreActionEndStruct_1;
	}

	public void method_19(Player obj)
	{
	}

	public void method_20(Player obj)
	{
		if (BotOwner_0.Memory.GoalEnemy != null && BotOwner_0.Memory.GoalEnemy.Person.Id == obj.Id)
		{
			Bool_6 = true;
		}
	}

	public void method_21()
	{
		BotOwner_0.Memory.Spotted(byHit: false);
		Bool_4 = true;
		Int_3 = 0;
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		bool flag = method_16();
		if (goalEnemy != null && !(goalEnemy.Distance > MIN_DIST_TO_TELEPORT && flag))
		{
			try
			{
				return;
			}
			catch (Exception)
			{
				return;
			}
		}
		Bool_5 = GClass856.IsTrue100(CHANCE_TELEPORT);
		try
		{
		}
		catch (Exception)
		{
		}
	}

	public void method_22(IPlayer arg1, DamageInfoStruct arg2, EBodyPart arg3)
	{
		Bool_4 = true;
		Float_3 = Time.time;
	}

	public void method_23(string arg1, DamageInfoStruct arg2)
	{
		Float_4 = Time.time;
	}

	public bool method_24()
	{
		return Time.time - Float_4 < 10f;
	}

	public override void Dispose()
	{
		BotOwner_0.BotPersonalStats.OnHitTarget -= method_22;
		BotOwner_0.BotPersonalStats.OnKillTarget -= method_23;
		base.Dispose();
	}

	[CompilerGenerated]
	public bool method_25(CustomNavigationPoint x)
	{
		if ((x.Position - BotOwner_0.Position).sqrMagnitude > 22500f)
		{
			return false;
		}
		return true;
	}
}
