using System;
using EFT;
using EFT.Vehicle;
using UnityEngine;

public class BotBewareBTR : GClass429
{
	public Action<Grenade> OnBewareGrenade;

	[NonSerialized]
	public float EndPeriodOfIgnore;

	[NonSerialized]
	public float NextPossibleRunTime;

	[NonSerialized]
	public float DELTA_CALC_TIME = 2f;

	[NonSerialized]
	public float DeltaGoCheck = 2f;

	public BTRVehicle BTRVehicle_0
	{
		get
		{
			if (BTRControllerClass.Instance != null)
			{
				return BTRControllerClass.Instance.BtrVehicle;
			}
			return null;
		}
	}

	public BotBewareBTR(BotOwner owner)
		: base(owner)
	{
	}

	public bool ShallRunAway()
	{
		if (BTRVehicle_0 == null)
		{
			return false;
		}
		if (GClass856.SqrDistance(BotOwner_0.Position, BTRVehicle_0.transform.position) < BotOwner_0.Settings.FileSettings.Mind.AVOID_BTR_RADIUS_SQR)
		{
			return true;
		}
		return false;
	}

	public void UpdateByNode()
	{
		method_0();
		BotOwner_0.BotRun.RunAwayGrenade();
	}

	public void method_0()
	{
		bool flag = false;
		Vector3 position = BTRVehicle_0.transform.position;
		if ((((NextPossibleRunTime < Time.time) ? 1u : 0u) | 0u) != 0)
		{
			CoverSearchData data = new CoverSearchData(BotOwner_0.Position, BotOwner_0.CoverSearchInfo, CoverShootType.hide, 100f, 0f, CoverSearchType.distToBot, null, null, null, ECheckSHootHide.hide, new CoverSearchDefenceDataClass(0f), PointsArrayType.allWithBush);
			NextPossibleRunTime = Time.time + (flag ? 0.5f : DELTA_CALC_TIME);
			CustomNavigationPoint customNavigationPoint = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: false);
			if (customNavigationPoint == null || (customNavigationPoint.Position - position).sqrMagnitude < LocalBotSettingsProviderClass.Core.DELTA_GRENADE_BEWARE_DIST_SQRT || !customNavigationPoint.IsFreeById(BotOwner_0.Id))
			{
				customNavigationPoint = method_1();
			}
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(customNavigationPoint);
			BotOwner_0.GoToPoint(customNavigationPoint);
		}
	}

	public CustomNavigationPoint method_1()
	{
		return BotOwner_0.Covers.FindHidePoint(BotOwner_0.Position, -1f);
	}
}
