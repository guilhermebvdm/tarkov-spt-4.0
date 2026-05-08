using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public abstract class GClass383
{
	[NonSerialized]
	public static List<CustomNavigationPoint> List_0 = new List<CustomNavigationPoint>(130);

	public static void CheckCanShotHideAtGroup(HashSet<CustomNavigationPoint> nearGroups, EditorsCoverPointDebugDataClass dataDebug, CoverSearchData data)
	{
		List_0.Clear();
		bool isMarksman = data.Bot.Type == WildSpawnType.marksman;
		foreach (CustomNavigationPoint nearGroup in nearGroups)
		{
			if (!CheckPointOnShootHide(data, nearGroup, isMarksman))
			{
				List_0.Add(nearGroup);
			}
		}
		nearGroups.ExceptWith(List_0);
	}

	public static bool CheckPointOnShootHide(CoverSearchData data, CustomNavigationPoint testedPoint, bool isMarksman)
	{
		testedPoint.CanIShootToEnemy = false;
		IBot bot = data.Bot;
		HashSet<Vector3> carePositions = data.CarePositions;
		if (testedPoint.IsSpotted)
		{
			return false;
		}
		testedPoint.SetWeight(float.MaxValue, withBaseWeight: false);
		if (!testedPoint.AlwaysGood)
		{
			if (!smethod_0(data, testedPoint))
			{
				return false;
			}
			return true;
		}
		if (isMarksman || testedPoint.IsDangerPositionFarEnough(carePositions, data.MinSDistToCarePos))
		{
			if (bot.GoalEnemy != null)
			{
				bool flag = false;
				Vector3 vector = bot.GoalEnemy.CurrPosition + BotOwner.STAY_HEIGHT;
				Vector3 firePosition = testedPoint.FirePosition;
				Vector3 direction = vector - firePosition;
				if (!Physics.Raycast(new Ray(firePosition, direction), out var _, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
				{
					testedPoint.CanIShootToEnemy = true;
					flag = true;
				}
				if (!flag)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public static bool smethod_0(CoverSearchData data, CustomNavigationPoint point)
	{
		IBot bot = data.Bot;
		CoverShootType shootType = data.shootType;
		ShootPointClass shoot2Point = data.Shoot2Point;
		HashSet<Vector3> carePositions = data.CarePositions;
		if (point.CanIHide(carePositions, data.MinSDistToCarePos, data.UseLineCastToCover, data.UseAngCastToCover))
		{
			bool flag = false;
			switch (shootType)
			{
			case CoverShootType.grenade:
				flag = GClass577.CanThrowGrenade2(point.FirePosition, shoot2Point.Point, bot.BotGrenadeController, AIGreandeAng.ang45, bot.Settings.Grenade.MIN_THROW_GRENADE_DIST_SQRT).CanThrow;
				break;
			case CoverShootType.shoot:
				flag = GClass369.CanShootToTarget(shoot2Point, point, data.Bot.LookSensorMask);
				data.AddPointCheckRay();
				break;
			}
			if (shootType != CoverShootType.hide)
			{
				if (!flag)
				{
					point.CanIShootToEnemy = false;
					return false;
				}
				point.CanIShootToEnemy = true;
			}
			return true;
		}
		point.CanIShootToEnemy = false;
		return false;
	}
}
