using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class CoverSearchData
{
	public readonly IBot Bot;

	public float MinDistSqr;

	public float MaxDistSqr;

	public float MinSDistToCarePos;

	public CoverSearchType SearchType;

	public ShootPointClass Shoot2Point;

	public HashSet<Vector3> CarePositions;

	public Vector3 CenterPos;

	public ECheckSHootHide CheckShootHide;

	public Vector3? PointToBeClose;

	public bool UseLineCastToCover;

	public bool UseAngCastToCover = true;

	public CoverShootType shootType;

	public PointsArrayType ArrayType;

	public bool UseSelfFindPoint;

	public EditorsCoverPointDebugDataClass EditorsCoverPointDebugData;

	public GClass388 DebugCoverPointsDataCollector;

	[CanBeNull]
	public int? PlaceInfo;

	[CanBeNull]
	public int? EnvironmentId;

	public ECoverPointSpecial Special;

	public Vector3? CloseFriendCover;

	[Obsolete]
	public CoverSearchDefenceDataClass coverSearchDefenceData;

	public string searchLabel;

	[NonSerialized]
	public bool ExtraPlacementsDropped;

	[NonSerialized]
	public int StepToDropExtraPlacements = 50;

	[field: NonSerialized]
	public int PointsCheckedTotal { get; set; }

	[field: NonSerialized]
	public int PointsCheckedRay { get; set; }

	[field: NonSerialized]
	public int MaxIterations { get; set; }

	public CoverSearchData(Vector3 centerPos, IBot bot, CoverShootType shootType, float maxDistSqr, float minDistSqr, CoverSearchType searchType, ShootPointClass shoot2point, Vector3? closeFriendCover, Vector3? pointToBeClose, ECheckSHootHide shootHide, CoverSearchDefenceDataClass coverSearchDefenceData, PointsArrayType arrayType = PointsArrayType.byShootType, bool useSelfFindPoint = true, int? placeInfo = null, int? maxIterations = null, string label = "Default")
	{
		PlaceInfo = placeInfo;
		CarePositions = bot.CarePositions();
		CenterPos = centerPos;
		this.coverSearchDefenceData = coverSearchDefenceData;
		Bot = bot;
		this.shootType = shootType;
		ArrayType = arrayType;
		MaxDistSqr = maxDistSqr;
		MinDistSqr = minDistSqr;
		SearchType = searchType;
		Shoot2Point = shoot2point;
		CloseFriendCover = closeFriendCover;
		PointToBeClose = pointToBeClose;
		CheckShootHide = shootHide;
		UseSelfFindPoint = useSelfFindPoint;
		MinSDistToCarePos = bot.Settings.Cover.MIN_TO_ENEMY_TO_BE_NOT_SAFE_SQRT;
		MaxIterations = (maxIterations.HasValue ? maxIterations.Value : Bot.Settings.Cover.MAX_ITERATIONS);
		searchLabel = label;
		RefreshCollectionInfo();
	}

	public CoverSearchData(Vector3 centerPos, IBot bot, CoverShootType shootType, float maxDistSqr, float minDistSqr, CoverSearchType searchType, ShootPointClass shoot2point, Vector3? closeFriendCover, Vector3? pointToBeClose, HashSet<Vector3> carePositions, ECheckSHootHide CheckShootHide, bool useLineCastToCover, CoverSearchDefenceDataClass coverSearchDefenceData, float minSDistToCarePos, int? placeInfo, EditorsCoverPointDebugDataClass editorsCoverPointDebugData, int maxIterations, string label)
	{
		PlaceInfo = placeInfo;
		this.coverSearchDefenceData = coverSearchDefenceData;
		CarePositions = carePositions;
		CenterPos = centerPos;
		MinSDistToCarePos = minSDistToCarePos;
		Bot = bot;
		this.shootType = shootType;
		MaxDistSqr = maxDistSqr;
		MinDistSqr = minDistSqr;
		SearchType = searchType;
		Shoot2Point = shoot2point;
		CloseFriendCover = closeFriendCover;
		PointToBeClose = pointToBeClose;
		this.CheckShootHide = CheckShootHide;
		MaxIterations = maxIterations;
		EditorsCoverPointDebugData = editorsCoverPointDebugData;
		ArrayType = PointsArrayType.byShootType;
		UseLineCastToCover = useLineCastToCover;
		searchLabel = label;
		RefreshCollectionInfo();
	}

	public void RefreshCollectionInfo()
	{
		DebugCoverPointsDataCollector = new GClass388(Bot, CenterPos);
	}

	public void SetStepToDropsExtraPlacements(int count)
	{
		StepToDropExtraPlacements = count;
	}

	public void IncreasePointCheckCounter()
	{
		PointsCheckedTotal++;
	}

	public void AddPointCheckRay()
	{
		PointsCheckedRay++;
	}

	public float CalcPowerPoint(GroupPoint point)
	{
		float distToPoint = DistToBot(point);
		float num = 1f;
		return method_3(distToPoint) * num + method_1(point);
	}

	public float PowerDistToCenter(IPositionPoint point, Vector3 p)
	{
		float sqrMagnitude = (point.Position - CenterPos).sqrMagnitude;
		return 1f / sqrMagnitude + method_1(point);
	}

	public float PowerDistToBot(GroupPoint point)
	{
		bool num = shootType == CoverShootType.hide;
		float distToPos = DistToBot(point);
		float defenceLevel = (num ? point.DefenceInfo.DefenceLevel : 1f);
		return method_2(distToPos, defenceLevel) + method_1(point);
	}

	public float PowerDistToBotAndToTarget(GroupPoint point)
	{
		float num = DistToBot(point);
		float magnitude = (point.Position - CenterPos).magnitude;
		return 50.3f / magnitude + 10f / num + method_1(point);
	}

	public float PowerDistToPoint(GroupPoint point)
	{
		float num = 0f;
		float num2 = 0f;
		if (PointToBeClose.HasValue)
		{
			Vector3 vector = PointToBeClose.Value - point.Position;
			if (Mathf.Abs(vector.y) > LocalBotSettingsProviderClass.Core.MAX_Y_DIFF_TO_PROTECT)
			{
				num2 = -55150f;
			}
			vector.y = 0f;
			num = vector.magnitude;
		}
		return num2 + 350f / Mathf.Abs(num - LocalBotSettingsProviderClass.Core.GOOD_DIST_TO_POINT);
	}

	public virtual float CalcPowerOnlyDistToBot(GroupPoint point)
	{
		float preferedDist = DistToBot(point);
		return CalcDist(point, preferedDist);
	}

	public float GetPower(GroupPoint point)
	{
		return SearchType switch
		{
			CoverSearchType.shoot_toCover_toBot_Distances => CalcPowerPoint(point), 
			CoverSearchType.distToBot => CalcPowerOnlyDistToBot(point), 
			CoverSearchType.distToBotAndToCenter => PowerDistToBotAndToTarget(point), 
			CoverSearchType.distToToCenter => PowerDistToCenter(point, CenterPos), 
			CoverSearchType.closerToSelectedPoint => PowerDistToPoint(point), 
			_ => CalcPowerOnlyDistToBot(point), 
		};
	}

	public float CalcDist(GroupPoint point, float preferedDist)
	{
		float num = 1f;
		float defenceLevel = ((shootType == CoverShootType.hide) ? point.DefenceLevel : 1f);
		float num2 = method_2(preferedDist, defenceLevel) * num;
		if (Bot.Settings.Cover.DEPENDS_Y_DIST_TO_BOT && Mathf.Abs(point.Position.y - Bot.Position.y) > 1f)
		{
			num2 *= 0.1f;
		}
		return num2 + method_1(point);
	}

	public void TryDropExtraPlacements(int currentPointChecksCount)
	{
		if (!ExtraPlacementsDropped && currentPointChecksCount > StepToDropExtraPlacements && StepToDropExtraPlacements > 0)
		{
			EnvironmentId = null;
			PlaceInfo = null;
			Special = (ECoverPointSpecial)0;
			ExtraPlacementsDropped = true;
		}
	}

	public float DistToBot(GroupPoint point)
	{
		return method_0(point);
	}

	public float method_0(IPositionPoint point)
	{
		return (point.Position - Bot.Position).magnitude;
	}

	public float method_1(IPositionPoint point)
	{
		float result = 0f;
		if (Bot.Settings.Cover.CHECK_CLOSEST_FRIEND && CloseFriendCover.HasValue && (point.Position - CloseFriendCover.Value).magnitude < Bot.Settings.Cover.CHECK_CLOSEST_FRIEND_DIST)
		{
			result = -550f;
		}
		return result;
	}

	public float method_2(float distToPos, float defenceLevel)
	{
		EnemyInfo goalEnemy = Bot.GoalEnemy;
		if (goalEnemy != null && Time.time - goalEnemy.TimeLastSeen < 6f)
		{
			return 1f / distToPos;
		}
		float num = defenceLevel - LocalBotSettingsProviderClass.Core.DEFENCE_LEVEL_SHIFT;
		if (num <= 1f)
		{
			num = 1f;
		}
		return num / distToPos;
	}

	public float method_3(float distToPoint)
	{
		return 1f / distToPoint;
	}

	public float method_4(float distToPoint, float deltaShoot, float preferedDist, float coefLevel, float closeFriendCover)
	{
		float num = preferedDist / 2f - deltaShoot * deltaShoot / preferedDist;
		float num2 = 100f - distToPoint * distToPoint / preferedDist;
		float num3 = 0f;
		if (closeFriendCover > 0f)
		{
			if (closeFriendCover < 10f)
			{
				num3 = -100f;
			}
			else
			{
				num3 = closeFriendCover * 50f / LocalBotSettingsProviderClass.Core.FORMUL_COEF_DELTA_FRIEND_COVER;
				num3 = ((num3 < 0f) ? 0f : num3);
			}
		}
		num2 = ((num2 < 0f) ? 0f : num2);
		return (num * LocalBotSettingsProviderClass.Core.FORMUL_COEF_DELTA_SHOOT + num2 * LocalBotSettingsProviderClass.Core.FORMUL_COEF_DELTA_DIST + num3) * coefLevel;
	}
}
