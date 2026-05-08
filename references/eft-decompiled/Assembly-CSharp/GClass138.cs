using System;
using EFT;
using UnityEngine;

public class GClass138(BotOwner bot, int priority, bool canUseGreenPoints) : GClass135(bot, priority, canUseGreenPoints)
{
	[NonSerialized]
	public string String_0 = "StayAtPosOpt";

	[NonSerialized]
	public bool Bool_7;

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (BotOwner_0.Memory.IsInCover && BotOwner_0.Memory.CurCustomCoverPoint != null)
		{
			return BotOwner_0.Memory.CurCustomCoverPoint;
		}
		if (CustomNavigationPoint_0 == null)
		{
			CustomNavigationPoint_0 = method_17(data);
		}
		return CustomNavigationPoint_0;
	}

	public override bool ShallUseNow()
	{
		return true;
	}

	public override string Name()
	{
		return String_0;
	}

	public void SetInside(bool isIndoorZone)
	{
		Bool_7 = isIndoorZone;
	}

	public override bool IsPointGood(CustomNavigationPoint cachedPoint)
	{
		int? placeId = GClass150.GetPlaceId(BotOwner_0);
		if (placeId.HasValue && (cachedPoint == null || cachedPoint.PlaceId != placeId.Value))
		{
			return false;
		}
		return base.IsPointGood(cachedPoint);
	}

	public CustomNavigationPoint method_17(CoverSearchData data)
	{
		float num = (Bool_7 ? 10f : 25f);
		float minDistToFriend = (Bool_7 ? 9f : (data.Bot.Settings.Cover.CHECK_CLOSEST_FRIEND_DIST * data.Bot.Settings.Cover.CHECK_CLOSEST_FRIEND_DIST));
		Vector3? closeFriendCover = BotOwner_0.Covers.ClosestFriendCoverPoint(Vector3_0);
		CoverPointMaster.GStruct10? gStruct = null;
		if (closeFriendCover.HasValue)
		{
			gStruct = new CoverPointMaster.GStruct10
			{
				MinDistToFriend = minDistToFriend,
				CoverPos = closeFriendCover.Value
			};
		}
		int? placeId = GClass150.GetPlaceId(BotOwner_0);
		if (!Bool_7)
		{
			float sDist;
			CustomNavigationPoint customNavigationPoint = BotOwner_0.BotsGroup.CoverPointMaster.ClosestGreenPoint(Vector3_0, BotOwner_0, gStruct, placeId, out sDist);
			if (customNavigationPoint != null && sDist < 900f)
			{
				return customNavigationPoint;
			}
		}
		CustomNavigationPoint curCustomCoverPoint = BotOwner_0.Memory.CurCustomCoverPoint;
		if (curCustomCoverPoint != null && curCustomCoverPoint.IsGoodInsideBuilding && (!placeId.HasValue || curCustomCoverPoint.PlaceId == placeId.Value))
		{
			return curCustomCoverPoint;
		}
		CustomNavigationPoint customNavigationPoint2 = BotOwner_0.Covers.FindHidePoint(Vector3_0, 0f, gStruct, onlyWithInsideCover: true, placeId);
		if (customNavigationPoint2 != null)
		{
			return customNavigationPoint2;
		}
		customNavigationPoint2 = BotOwner_0.Covers.FindHidePoint(Vector3_0, 0f, gStruct, onlyWithInsideCover: false, placeId);
		if (customNavigationPoint2 != null)
		{
			return customNavigationPoint2;
		}
		Vector3 position = BotOwner_0.Position;
		CoverSearchData coverSearchData = new CoverSearchData(position, BotOwner_0.CoverSearchInfo, CoverShootType.hide, 16f, 0f, CoverSearchType.distToBot, null, closeFriendCover, position, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(0f));
		coverSearchData.CenterPos = position;
		if (Bool_7)
		{
			coverSearchData.UseAngCastToCover = true;
			coverSearchData.UseLineCastToCover = true;
		}
		coverSearchData.MinSDistToCarePos = num * num;
		coverSearchData.UseSelfFindPoint = false;
		return BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(coverSearchData, checkCurrent: true);
	}
}
