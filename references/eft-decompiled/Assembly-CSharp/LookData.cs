using System;
using EFT;
using UnityEngine;

public class LookData : GClass429
{
	[NonSerialized]
	public int LastSideSign = 1;

	[NonSerialized]
	public float NextCheckTimeSetLookAnotherSide;

	[NonSerialized]
	public float LOOK_TO_ENEMY_TIME = 10f;

	[NonSerialized]
	public float NextCheckTimeSetLookHearing;

	public static LookData Create(BotOwner owner)
	{
		if (BotSettingsRepoClass.IsInfected(owner.Profile.Info.Settings.Role))
		{
			return new GClass499(owner);
		}
		return new LookData(owner);
	}

	public LookData(BotOwner owner)
		: base(owner)
	{
		LOOK_TO_ENEMY_TIME = BotOwner_0.Settings.FileSettings.Look.LOOK_TO_ENEMY_TIME;
	}

	public void ResetUpdateTime()
	{
		if (!(NextCheckTimeSetLookHearing < 0.3f))
		{
			NextCheckTimeSetLookHearing = 0.3f;
		}
	}

	public virtual bool IsLookPointExistAndValid(CustomNavigationPoint point)
	{
		return point != null;
	}

	public virtual void SetLookPointByHearing(CustomNavigationPoint closestPoint = null)
	{
		if ((BotOwner_0.Mover.CurrentState == EBotMoverState.NearDoor || BotOwner_0.DoorOpener.Interacting) && BotOwner_0.DoorOpener.HaveDoorToOpen)
		{
			Vector3 dir = BotOwner_0.DoorOpener.LookPoint - BotOwner_0.Position;
			dir.y = 0f;
			BotOwner_0.Steering.LookToDirection(dir);
			BotOwner_0.Steering.SetYAngle(0f);
		}
		else
		{
			if (!(Time.time > NextCheckTimeSetLookHearing))
			{
				return;
			}
			bool flag = true;
			NextCheckTimeSetLookHearing = Time.time + BotOwner_0.Settings.FileSettings.Look.WAIT_NEW_SENSOR * (UnityEngine.Random.value + 0.2f);
			Vector3? vector = null;
			float num = -1f;
			PlaceForCheck placeForCheck = BotOwner_0.BotsGroup.YoungestPlace(BotOwner_0, BotOwner_0.Settings.FileSettings.Hearing.DIST_PLACE_TO_FIND_POINT, priorityDanger: true);
			EnemyInfo enemyInfo = null;
			if (BotOwner_0.Memory.GoalEnemy != null && Time.time - BotOwner_0.Memory.GoalEnemy.TimeLastSeenReal < LOOK_TO_ENEMY_TIME)
			{
				enemyInfo = BotOwner_0.Memory.GoalEnemy;
				if (BotOwner_0.Memory.LastEnemy != null && Time.time - BotOwner_0.Memory.LastEnemy.TimeLastSeenReal < LOOK_TO_ENEMY_TIME)
				{
					enemyInfo = BotOwner_0.Memory.LastEnemy;
				}
			}
			if (enemyInfo != null)
			{
				num = Time.time - enemyInfo.TimeLastSeen;
				if (num < BotOwner_0.Settings.FileSettings.Cover.LOOK_LAST_ENEMY_POS_LONG && enemyInfo.Distance < BotOwner_0.Settings.FileSettings.Cover.LOOK_LAST_ENEMY_POS_DIST)
				{
					vector = enemyInfo.CurrPosition - BotOwner_0.Position;
				}
				else if (num < BotOwner_0.Settings.FileSettings.Cover.LOOK_LAST_ENEMY_POS_MOVING)
				{
					vector = ((num < 1f || !BotOwner_0.Memory.LastDamageDataActive) ? new Vector3?(enemyInfo.EnemyLastPosition - BotOwner_0.Position) : new Vector3?(BotOwner_0.Memory.LastDamageData.Postion - BotOwner_0.Position));
				}
				else if (BotOwner_0.Memory.LastDamageDataActive)
				{
					vector = BotOwner_0.Memory.LastDamageData.Postion - BotOwner_0.Position;
				}
				else if (placeForCheck != null)
				{
					bool flag2 = false;
					if (!(Time.time - placeForCheck.CreatedTime < BotOwner_0.Settings.FileSettings.Hearing.LOOK_ONLY_DANGER_DELTA) || placeForCheck.Type == PlaceForCheckType.danger)
					{
						flag2 = true;
					}
					if (flag2)
					{
						flag = false;
						vector = UsePlaceForCheck(placeForCheck);
					}
				}
				else if (num < BotOwner_0.Settings.FileSettings.Look.LOOK_LAST_POSENEMY_IF_NO_DANGER_SEC)
				{
					vector = enemyInfo.EnemyLastPosition - BotOwner_0.Position;
				}
			}
			if (!vector.HasValue && BotOwner_0.Memory.LastDamageDataActive && placeForCheck != null)
			{
				vector = UsePlaceForCheck(placeForCheck);
			}
			bool flag3 = enemyInfo != null && Time.time - enemyInfo.TimeLastSeen < BotOwner_0.Settings.FileSettings.Hearing.LOOK_ONLY_DANGER_DELTA;
			if (!vector.HasValue)
			{
				if (placeForCheck != null && flag3 && !placeForCheck.IsDanger)
				{
					placeForCheck = null;
				}
				if (placeForCheck != null && Time.time - placeForCheck.CreatedTime < BotOwner_0.Settings.FileSettings.Look.OLD_TIME_POINT)
				{
					vector = UsePlaceForCheck(placeForCheck);
				}
			}
			if (closestPoint == null)
			{
				closestPoint = BotOwner_0.Memory.CurCustomCoverPoint;
			}
			if (!vector.HasValue)
			{
				if (BotOwner_0.Mover.IsMoving)
				{
					BotOwner_0.Steering.LookToMovingDirection();
				}
				else
				{
					int num2 = LastSideSign;
					if (Time.time > NextCheckTimeSetLookAnotherSide && closestPoint != null)
					{
						float num3 = BotOwner_0.Settings.FileSettings.Look.WAIT_NEW__LOOK_SENSOR * (UnityEngine.Random.value + 0.5f);
						NextCheckTimeSetLookAnotherSide = Time.time + num3;
						num2 = ((num2 <= 0) ? 1 : (-1));
					}
					if (closestPoint != null && BotOwner_0.Memory.IsInCover)
					{
						switch (closestPoint.CoverLevel)
						{
						case CoverLevel.Sit:
						case CoverLevel.Lay:
							vector = RotateWallBySide(closestPoint, num2);
							break;
						case CoverLevel.Stay:
						{
							Vector3 b = -closestPoint.ToWallVector;
							b.y = 0f;
							float num4 = GClass856.Random(30f, 80f);
							if (GClass856.IsTrue100(50f))
							{
								num4 = 0f - num4;
							}
							vector = GClass855.RotateOnAngUp(b, num4);
							flag = false;
							break;
						}
						}
					}
				}
			}
			if (!vector.HasValue && BotOwner_0.Memory.LastEnemy != null && Time.time - BotOwner_0.Memory.LastEnemy.TimeLastSeen < BotOwner_0.Settings.FileSettings.Mind.LAST_ENEMY_LOOK_TO)
			{
				vector = BotOwner_0.Memory.LastEnemy.EnemyLastPosition - BotOwner_0.Transform.position;
			}
			if (vector.HasValue && vector.Value.sqrMagnitude > 0.001f)
			{
				CustomNavigationPoint customNavigationPoint = closestPoint;
				if (enemyInfo == null && placeForCheck != null && placeForCheck.IsDanger)
				{
					flag = false;
				}
				bool flag4 = enemyInfo == null || Time.time - enemyInfo.TimeLastSeen > 20f;
				if (flag && customNavigationPoint != null && flag4 && (BotOwner_0.Transform.position - customNavigationPoint.Position).sqrMagnitude < LocalBotSettingsProviderClass.Core.CLOSE_TO_WALL_ROTATE_BY_WALL_SQRT && (customNavigationPoint.CoverLevel == CoverLevel.Stay || customNavigationPoint.StrategyType == PointWithNeighborType.both || customNavigationPoint.StrategyType == PointWithNeighborType.ambush))
				{
					Vector3 vector2 = GClass855.NormalizeFastSelf(vector.Value);
					Vector3 normalized = customNavigationPoint.ToWallVector.normalized;
					if (GClass855.IsAngLessNormalized(vector2, normalized, 0.5f) && customNavigationPoint.BordersLightHave)
					{
						float num5 = Vector3.Angle(customNavigationPoint.LeftBorderLight, vector2);
						float num6 = Vector3.Angle(customNavigationPoint.RightBorderLight, vector2);
						bool flag5 = num > 0f && num < LocalBotSettingsProviderClass.Core.LOOK_ANYSIDE_BY_WALL_SEC_OF_ENEMY;
						vector = ((num5 < num6 && (customNavigationPoint.CanLookLeft || flag5)) ? new Vector3?(RotateWallBySide(customNavigationPoint, 1)) : ((!(customNavigationPoint.CanLookRight || flag5)) ? new Vector3?(GClass855.RotateOnAngUp(-customNavigationPoint.ToWallVector, GClass856.Random(-10f, 10f))) : new Vector3?(RotateWallBySide(customNavigationPoint, -1))));
					}
				}
				BotOwner_0.Steering.LookToDirection(vector.Value);
				return;
			}
			CustomNavigationPoint customNavigationPoint2 = closestPoint;
			if (IsLookPointExistAndValid(customNavigationPoint2))
			{
				if ((BotOwner_0.Transform.position - customNavigationPoint2.Position).sqrMagnitude < LocalBotSettingsProviderClass.Core.CLOSE_TO_WALL_ROTATE_BY_WALL_SQRT)
				{
					int side = GClass856.RandomSing();
					vector = RotateWallBySide(customNavigationPoint2, side);
					BotOwner_0.Steering.LookToDirection(vector.Value);
				}
				else if (customNavigationPoint2.CoverLevel == CoverLevel.Stay)
				{
					vector = GClass855.RotateOnAngUp(-customNavigationPoint2.ToWallVector, GClass856.Random(-10f, 10f));
					BotOwner_0.Steering.LookToDirection(vector.Value);
				}
				else
				{
					BotOwner_0.Steering.LookToDirection(GClass856.RandomHorizontal(-1f, 1f));
				}
			}
			else
			{
				BotOwner_0.Steering.LookToMovingDirection();
			}
		}
	}

	public Vector3 RotateWallBySide(CustomNavigationPoint covPoint, int side = 0)
	{
		Vector3 toWallVector = covPoint.ToWallVector;
		if (side == 0)
		{
			LastSideSign = -LastSideSign;
			side = LastSideSign;
		}
		int oFFSET_LOOK_ALONG_WALL_ANG = BotOwner_0.Settings.FileSettings.Cover.OFFSET_LOOK_ALONG_WALL_ANG;
		int num = GClass856.RandomInclude(90 - oFFSET_LOOK_ALONG_WALL_ANG, 90 + oFFSET_LOOK_ALONG_WALL_ANG);
		return GClass855.RotateOnAngUp(toWallVector, side * num);
	}

	public Vector3 UsePlaceForCheck(PlaceForCheck palce)
	{
		Vector3 vector = palce.Position - BotOwner_0.MyHead.position;
		if (vector.magnitude < BotOwner_0.Settings.FileSettings.Look.DIST_NOT_TO_IGNORE_WALL)
		{
			return vector;
		}
		if (Physics.Raycast(new Ray(BotOwner_0.MyHead.position, vector), out var _, BotOwner_0.Settings.FileSettings.Look.DIST_CHECK_WALL, LayerMaskClass.HighPolyWithTerrainMask))
		{
			return GClass369.Test4Sides(vector, BotOwner_0.MyHead.position);
		}
		return vector;
	}
}
