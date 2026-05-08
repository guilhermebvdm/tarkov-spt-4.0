using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public abstract class GClass379
{
	[NonSerialized]
	public List<CustomNavigationPoint> List_0 = new List<CustomNavigationPoint>(130);

	public bool method_0(IBot bot, CoverShootType withShoot, GroupPointSearchData point, ShootPointClass shootToPoint, HashSet<Vector3> carePositions, EditorsCoverPointDebugDataClass dataDebug, CoverSearchData data)
	{
		bool flag = false;
		if (GClass394.IsDangerPositionFarEnough(point.Point.Position, carePositions, data.MinSDistToCarePos))
		{
			point.Cause = ECoverCheckCause.TooClose;
			return false;
		}
		switch (withShoot)
		{
		case CoverShootType.grenade:
			flag = GClass577.CanThrowGrenade2(point.FirePosition, shootToPoint.Point, bot.BotGrenadeController, AIGreandeAng.ang45, bot.Settings.Grenade.MIN_THROW_GRENADE_DIST_SQRT).CanThrow;
			break;
		case CoverShootType.shoot:
			flag = GClass369.CanShootToTarget(shootToPoint, point, data.Bot.LookSensorMask);
			data.AddPointCheckRay();
			break;
		}
		point.CanIShootToEnemy = flag;
		if (!flag)
		{
			point.Cause = ECoverCheckCause.CantShoot;
			return false;
		}
		return true;
	}

	public bool method_1(IBot bot, CoverShootType withShoot, GroupPointSearchData point, ShootPointClass shootToPoint, HashSet<Vector3> carePositions, EditorsCoverPointDebugDataClass dataDebug, CoverSearchData data)
	{
		bool flag = data.Bot.Type == WildSpawnType.marksman;
		WildSpawnType type = data.Bot.Type;
		if ((type == WildSpawnType.assault || type == WildSpawnType.assaultGroup) && point.Point.CoverType == CoverType.Foliage)
		{
			return false;
		}
		if (!point.Point.AlwaysGood)
		{
			if (!GClass394.CanIHide(point.Point.Position, point.Point.WallDirection, carePositions, data.MinSDistToCarePos, data.UseLineCastToCover, data.UseAngCastToCover))
			{
				point.Cause = ECoverCheckCause.CantHide;
				point.CanIShootToEnemy = false;
				return false;
			}
			bool flag2 = false;
			switch (withShoot)
			{
			case CoverShootType.shoot:
				flag2 = GClass369.CanShootToTarget(shootToPoint, point, data.Bot.LookSensorMask);
				data.AddPointCheckRay();
				goto default;
			case CoverShootType.grenade:
				flag2 = GClass577.CanThrowGrenade2(point.Point.FirePosition, shootToPoint.Point, bot.BotGrenadeController, AIGreandeAng.ang45, bot.Settings.Grenade.MIN_THROW_GRENADE_DIST_SQRT).CanThrow;
				goto default;
			default:
				if (flag2)
				{
					point.CanIShootToEnemy = true;
					return true;
				}
				point.Cause = ECoverCheckCause.CantShoot;
				return false;
			case CoverShootType.hide:
				point.CanIShootToEnemy = false;
				return true;
			}
		}
		if (!flag && !GClass394.IsDangerPositionFarEnough(point.Point.Position, carePositions, data.MinSDistToCarePos))
		{
			point.Cause = ECoverCheckCause.CantHide;
			return false;
		}
		if (bot.GoalEnemy != null)
		{
			bool flag3 = false;
			Vector3 vector = bot.GoalEnemy.CurrPosition + BotOwner.STAY_HEIGHT;
			Vector3 firePosition = point.Point.FirePosition;
			Vector3 direction = vector - firePosition;
			if (!Physics.Raycast(new Ray(firePosition, direction), out var _, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
			{
				flag3 = true;
			}
			if (!flag3)
			{
				point.Cause = ECoverCheckCause.CantShoot;
				return false;
			}
		}
		return true;
	}

	public bool method_2(Vector3 centerPos, HashSet<Vector3> carePositions, GroupPoint point, EditorsCoverPointDebugDataClass editorsCoverPointDebugData, CoverSearchData searchData)
	{
		List_0.Clear();
		searchData.IncreasePointCheckCounter();
		foreach (Vector3 carePosition in carePositions)
		{
			if ((centerPos - carePosition).sqrMagnitude > 0.001f)
			{
				Vector3 rhs = centerPos - carePosition;
				Vector3 lhs = point.Position - carePosition;
				float y = 0f;
				lhs.y = 0f;
				rhs.y = y;
				if (!(Vector3.Dot(lhs, rhs) >= 0f))
				{
					return false;
				}
			}
		}
		return true;
	}

	public CustomNavigationPoint method_3(IEnumerable<CustomNavigationPoint> list)
	{
		CustomNavigationPoint result = null;
		float num = float.MinValue;
		foreach (CustomNavigationPoint item in list)
		{
			if (item.CoveringWeight > num)
			{
				num = item.CoveringWeight;
				result = item;
			}
		}
		return result;
	}

	public bool method_4(GroupPointSearchData point, IEnumerable<Vector3> carePositions, EditorsCoverPointDebugDataClass dataDebug, CoverSearchData data)
	{
		if (!point.AlwaysGood)
		{
			if (!GClass394.CanIHide(point.Point.Position, point.Point.WallDirection, carePositions, data.MinSDistToCarePos, data.UseLineCastToCover, data.UseAngCastToCover))
			{
				point.CanIShootToEnemy = false;
				point.Cause = ECoverCheckCause.CantHide;
				return false;
			}
		}
		else if (!GClass394.IsDangerPositionFarEnough(point.Point.Position, carePositions, data.MinSDistToCarePos))
		{
			point.Cause = ECoverCheckCause.CantHide;
			point.CanIShootToEnemy = false;
			return false;
		}
		return true;
	}

	public GClass379()
	{
	}
}
