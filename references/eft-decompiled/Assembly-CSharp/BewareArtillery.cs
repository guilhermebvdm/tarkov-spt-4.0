using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class BewareArtillery : GClass429
{
	[NonSerialized]
	public (float nextCheck, bool lastResult) LineCastResult;

	[NonSerialized]
	public bool LastSafeResult;

	[NonSerialized]
	public float CoverSaveAndHasPathLastCheckTime;

	[NonSerialized]
	public float LastFindNewCoverTime;

	public BewareArtillery(BotOwner owner)
		: base(owner)
	{
	}

	public void UpdateByNode()
	{
		method_0();
		BotOwner_0.BotRun.RunAwayGrenade();
	}

	public void method_0()
	{
		bool flag = false;
		if (BotOwner_0.Memory.BotCurrentCoverInfo.HaveCover)
		{
			if (CoverSaveAndHasPathLastCheckTime < Time.time)
			{
				CustomNavigationPoint covPoint = BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint;
				if (IsCoverSafe(covPoint))
				{
					CoverSaveAndHasPathLastCheckTime = Time.time + 0.5f;
					if (!covPoint.IsFreeById(BotOwner_0.Id) && !BotOwner_0.Mover.HasPathAndNoComplete)
					{
						BotOwner_0.GoToPoint(covPoint);
					}
				}
				else
				{
					flag = true;
				}
			}
			if (!flag)
			{
				return;
			}
		}
		if (LastFindNewCoverTime >= Time.time)
		{
			return;
		}
		LastFindNewCoverTime = Time.time + UnityEngine.Random.Range(10f, 15f);
		(string, GClass1875) closestZone = BotOwner_0.ArtilleryDangerPlace.GetClosestZone();
		if (!string.IsNullOrEmpty(closestZone.Item1) && closestZone.Item2 != null)
		{
			CoverSearchData data = new CoverSearchData(closestZone.Item2.Position, BotOwner_0.CoverSearchInfo, CoverShootType.hide, closestZone.Item2.RadiusSqr + 100f, closestZone.Item2.RadiusSqr, CoverSearchType.distToToCenter, null, null, null, ECheckSHootHide.hide, new CoverSearchDefenceDataClass(0f));
			CustomNavigationPoint customNavigationPoint = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: false);
			if (customNavigationPoint == null || !customNavigationPoint.IsFreeById(BotOwner_0.Id) || !IsCoverSafe(customNavigationPoint))
			{
				Vector3 vector = GClass855.NormalizeFastSelf(BotOwner_0.Position - BotOwner_0.ArtilleryDangerPlace.GetAveragePositionOfAllZonesCenters());
				customNavigationPoint = BotOwner_0.Covers.FindHidePoint(BotOwner_0.Position + vector * BotOwner_0.ArtilleryDangerPlace.MaxDangerRadius, -1f);
			}
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(customNavigationPoint);
			BotOwner_0.GoToPoint(customNavigationPoint);
		}
	}

	public bool IsCoverSafe(CustomNavigationPoint cover)
	{
		if (LineCastResult.nextCheck > Time.time)
		{
			return LineCastResult.lastResult;
		}
		LineCastResult.nextCheck = Time.time + 2f;
		foreach (KeyValuePair<string, List<CustomNavigationPoint>> activeZonesSpottedCover in BotOwner_0.ArtilleryDangerPlace.ActiveZonesSpottedCovers)
		{
			if (BotOwner_0.BotsController.ArtilleryZonesController.TryGetZoneById(activeZonesSpottedCover.Key, out var zoneData))
			{
				LineCastResult.lastResult = Physics.Linecast(zoneData.Position, cover.Position + Vector3.up, LayerMaskClass.HighPolyWithTerrainMask);
				if (!LineCastResult.lastResult)
				{
					return false;
				}
			}
		}
		return LineCastResult.lastResult;
	}
}
