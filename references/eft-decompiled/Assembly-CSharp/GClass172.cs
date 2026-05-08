using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass172 : GClass168
{
	[Serializable]
	[CompilerGenerated]
	public class Class230
	{
		public static readonly Class230 class230_0 = new Class230();

		public static Action action_0;

		public void method_0()
		{
		}
	}

	[NonSerialized]
	public const int Int_3 = 6;

	[NonSerialized]
	public const int Int_4 = 8;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public AIPlaceInfo AiplaceInfo_0;

	[NonSerialized]
	public List<CustomNavigationPoint> List_0;

	public GClass172(BotOwner bot, int priority)
		: base(bot, priority)
	{
		Float_3 = Time.time;
		List_0 = BotOwner_0.VoxelesPersonalData.Points((CustomNavigationPoint x) => x.PlaceId == base.Nullable_0);
		_ = List_0.Count;
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.Medecine.FirstAid.Have2Do && !method_12(8f))
		{
			if (BotOwner_0.Memory.IsInCover)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "first aid ptr");
			}
			if (method_12(30f))
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "goforheal");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "heal now");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			if (AiplaceInfo_0 != null)
			{
				Vector3 position = AiplaceInfo_0.Collider.transform.position;
				Debug.DrawRay(position, Vector3.up * 200f, Color.red, 2f);
				BotOwner_0.BotsGroup.AddPointToSearch(position, 210f, BotOwner_0, baseReacheble: false, canUseCovers: false);
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.lay, "holdOrCover");
		}
		if (method_15())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.teleportToCover, "tp1");
		}
		if (!BotOwner_0.Memory.BotCurrentCoverInfo.HaveCover)
		{
			BotOwner_0.BotAttackManager.TryPointGetting(method_19(), delegate(CustomNavigationPoint point)
			{
				BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(point);
			}, delegate
			{
			});
		}
		if (BotOwner_0.Memory.BotCurrentCoverInfo.HaveCover)
		{
			if ((BotOwner_0.Position - BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint.Position).magnitude > 8f)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "RtoC");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.crawl, "CToc");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "RNC");
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (base.Nullable_0.HasValue && Time.time - Float_3 < 20f)
		{
			if (checkCurrent && data.Bot.IsCurrentPointGood(data.SearchType, data, out var point))
			{
				return point;
			}
			if (List_0.Count > 0)
			{
				return GClass856.RandomElement(List_0);
			}
		}
		return base.FindPoint(data, p, checkCurrent);
	}

	public override bool ShallUseNow()
	{
		return true;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		return AICoreActionEndStruct;
	}

	public override string Name()
	{
		return "LayingPatrol";
	}

	public override void OnActivate()
	{
		base.OnActivate();
		foreach (AIPlaceInfo place in BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.AIPlaceInfoHolder.Places)
		{
			if (place.InfoLogicAllEnemy is AIPlaceInfoLogicZryachiy { LookAtCenter: not false })
			{
				AiplaceInfo_0 = place;
				break;
			}
		}
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (BotOwner_0.Mover.DistDestination < 6f)
		{
			return new AICoreActionEndStruct("Close");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			if (!BotOwner_0.Medecine.FirstAid.Have2Do)
			{
				CustomNavigationPoint curCustomCoverPoint = BotOwner_0.Memory.CurCustomCoverPoint;
				BotOwner_0.Memory.CheckIsInCover2();
				if (!BotOwner_0.Memory.BotCurrentCoverInfo.HaveCover)
				{
					BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(curCustomCoverPoint);
				}
			}
			return new AICoreActionEndStruct("InCover");
		}
		if (BotOwner_0.Memory.CurCustomCoverPoint != null && BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
		{
			return new AICoreActionEndStruct("IsSpotted");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndCrawlNode()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("inCover");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndLayNode()
	{
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("layNIC");
		}
		if (base.Nullable_0.HasValue && BotOwner_0.Memory.CurCustomCoverPoint.PlaceId != base.Nullable_0.Value)
		{
			BotOwner_0.Memory.Spotted(byHit: false);
			return new AICoreActionEndStruct("wrong cover");
		}
		if (BotOwner_0.Medecine.FirstAid.Have2Do && !method_12(8f))
		{
			return new AICoreActionEndStruct("mdc1");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndHeal()
	{
		if (!BotOwner_0.Medecine.FirstAid.Have2Do)
		{
			return new AICoreActionEndStruct("EndHeal");
		}
		return AICoreActionEndStruct_1;
	}

	public CoverSearchData method_19()
	{
		return new CoverSearchData(BotOwner_0.Position, BotOwner_0.CoverSearchInfo, CoverShootType.shoot, 10000f, 0f, CoverSearchType.distToBot, null, null, null, ECheckSHootHide.shoot, new CoverSearchDefenceDataClass(0f), PointsArrayType.allWithBush, useSelfFindPoint: true, base.Nullable_0);
	}

	[CompilerGenerated]
	public bool method_20(CustomNavigationPoint x)
	{
		return x.PlaceId == base.Nullable_0;
	}

	[CompilerGenerated]
	public void method_21(CustomNavigationPoint point)
	{
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(point);
	}
}
