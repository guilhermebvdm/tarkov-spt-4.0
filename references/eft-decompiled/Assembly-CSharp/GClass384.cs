using System;
using EFT;
using UnityEngine;

public class GClass384 : CoverSearchData
{
	[NonSerialized]
	public float Float_0;

	public GClass384(float prefDist, Vector3 centerPos, BotOwner bot, CoverShootType shootType, float maxDistSqr, float minDistSqr, CoverSearchType searchType, ShootPointClass shoot2point, Vector3? closeFriendCover, Vector3? pointToBeClose, ECheckSHootHide CheckShootHide, CoverSearchDefenceDataClass coverSearchDefenceData, PointsArrayType arrayType = PointsArrayType.byShootType, bool useSelfFindPoint = true, int? placeInfo = null, int? maxIterations = null)
		: base(centerPos, bot.CoverSearchInfo, shootType, maxDistSqr, minDistSqr, searchType, shoot2point, closeFriendCover, pointToBeClose, CheckShootHide, coverSearchDefenceData, arrayType, useSelfFindPoint, placeInfo, maxIterations)
	{
		Float_0 = prefDist;
	}

	public override float CalcPowerOnlyDistToBot(GroupPoint point)
	{
		float preferedDist = Mathf.Abs(DistToBot(point) - Float_0);
		return CalcDist(point, preferedDist);
	}
}
