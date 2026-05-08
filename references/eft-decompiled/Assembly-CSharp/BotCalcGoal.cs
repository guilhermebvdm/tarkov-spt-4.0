using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class BotCalcGoal : GClass429
{
	[CompilerGenerated]
	public class Class240
	{
		public BotOwner bot;

		public bool method_0(PlaceForCheck check)
		{
			if ((check.CheckingPlayer == null || check.CheckingPlayer == bot) && check.Reacheble)
			{
				return !check.IsCome;
			}
			return false;
		}

		public bool method_1(PlaceForCheck check)
		{
			if (!(check.CheckingPlayer == null) && !(check.CheckingPlayer == bot))
			{
				return false;
			}
			return check.Reacheble;
		}

		public bool method_2(PlaceForCheck check)
		{
			if (!(check.CheckingPlayer == null))
			{
				return check.CheckingPlayer == bot;
			}
			return true;
		}
	}

	[NonSerialized]
	public float NextCheckGoalIfHaveEnemy;

	public List<PlaceForCheck> PlacesForCheck => BotOwner_0.BotsGroup.PlacesForCheck;

	public BotCalcGoal(BotOwner owner)
		: base(owner)
	{
	}

	public void CalcGoalForBot()
	{
		BotOwner botOwner_ = BotOwner_0;
		if (botOwner_.Memory.DangerData.HaveCloseDanger)
		{
			botOwner_.Memory.GoalTarget.Clear();
			botOwner_.Memory.GoalEnemy = null;
			return;
		}
		EnemyInfo enemyInfo = botOwner_.EnemyChooser.FindDangerEnemy();
		if (enemyInfo != null)
		{
			botOwner_.Memory.GoalEnemy = enemyInfo;
			if (NextCheckGoalIfHaveEnemy < Time.time || !BotOwner_0.Memory.HaveGoal)
			{
				NextCheckGoalIfHaveEnemy = Time.time + 3f;
				method_0(withDropEnemy: false);
			}
		}
		else
		{
			method_0(withDropEnemy: true);
		}
	}

	public void method_0(bool withDropEnemy)
	{
		BotOwner botOwner_ = BotOwner_0;
		if (withDropEnemy)
		{
			botOwner_.Memory.GoalEnemy = null;
		}
		PlaceForCheck placeForCheck = method_1(botOwner_);
		if (placeForCheck != null)
		{
			botOwner_.Memory.GoalTarget.SetTarget(placeForCheck);
		}
		else if (PlacesForCheck.Count == 0)
		{
			if (BotOwner_0.BotsGroup.LastSoundsController.ZeroGoalTarget != null && !BotOwner_0.BotsGroup.LastSoundsController.ZeroGoalTarget.IsExpire())
			{
				botOwner_.Memory.GoalTarget.SetZeroGoal();
			}
			else
			{
				botOwner_.Memory.GoalTarget.Clear();
			}
		}
		else
		{
			botOwner_.Memory.GoalTarget.SetZeroGoal();
		}
	}

	public PlaceForCheck method_1(BotOwner bot)
	{
		List<BotSettingsClass> lastSeenPositions = BotOwner_0.BotsGroup.GetLastSeenPositions();
		if (PlacesForCheck.Count == 0 && lastSeenPositions.Count == 0)
		{
			return null;
		}
		int minIndex = 0;
		double minPoint = double.MaxValue;
		method_6(bot, (PlaceForCheck check) => (check.CheckingPlayer == null || check.CheckingPlayer == bot) && check.Reacheble && !check.IsCome, out minIndex, out minPoint);
		if (minIndex < 0)
		{
			method_6(bot, (PlaceForCheck check) => (check.CheckingPlayer == null || check.CheckingPlayer == bot) && check.Reacheble, out minIndex, out minPoint);
		}
		if (PlacesForCheck.Count == 0)
		{
			minPoint = 0.0;
		}
		int index = 0;
		double num = double.MaxValue;
		for (int num2 = 0; num2 < lastSeenPositions.Count; num2++)
		{
			BotSettingsClass botSettingsClass = lastSeenPositions[num2];
			if (botSettingsClass.IsHaveSeen)
			{
				float num3 = Time.time - botSettingsClass.EnemyLastSeenTimeSense;
				float magnitude = (bot.Position - botSettingsClass.EnemyLastPosition).magnitude;
				double num4 = method_2(bot, num3, magnitude, bot.Settings.FileSettings.Mind.LASTSEEN_POINT_CHOOSE_COEF);
				if (num4 < num)
				{
					index = num2;
					num = num4;
				}
			}
		}
		if (minPoint < num && PlacesForCheck.Count != 0)
		{
			return PlacesForCheck[minIndex];
		}
		if (lastSeenPositions.Count != 0)
		{
			BotSettingsClass botSettingsClass2 = lastSeenPositions[index];
			if (method_7(botSettingsClass2.EnemyLastSeenTimeSense))
			{
				return null;
			}
			return method_5(bot, botSettingsClass2.EnemyLastPosition);
		}
		method_6(bot, (PlaceForCheck check) => check.CheckingPlayer == null || check.CheckingPlayer == bot, out minIndex, out minPoint);
		if (minIndex >= 0)
		{
			return PlacesForCheck[minIndex];
		}
		return null;
	}

	public double method_2(BotOwner owner, double sec, float dist, float coef)
	{
		return (sec * sec + (double)(dist * owner.Settings.FileSettings.Mind.COVER_DIST_COEF)) * (double)coef;
	}

	public PlaceForCheck method_3(BotOwner bot, Vector3 pos)
	{
		if (bot.Memory.GoalTarget.HavePlaceTarget())
		{
			if (bot.Memory.GoalTarget.GoalTarget.IsThisPointClose(pos))
			{
				return bot.Memory.GoalTarget.GoalTarget;
			}
			return method_4(pos, PlaceForCheckType.danger, canUseCovrPoints: false);
		}
		return method_4(pos, PlaceForCheckType.danger, canUseCovrPoints: false);
	}

	public PlaceForCheck method_4(Vector3 suspectedPoint, PlaceForCheckType type, bool canUseCovrPoints = true)
	{
		PlaceForCheck placeForCheck = null;
		if (canUseCovrPoints)
		{
			CustomNavigationPoint freeClosePoint = BotOwner_0.Covers.GetFreeClosePoint(suspectedPoint, 0f);
			if (freeClosePoint == null)
			{
				return null;
			}
			Vector3 direction = freeClosePoint.Position - suspectedPoint;
			float magnitude = direction.magnitude;
			bool flag;
			if (flag = magnitude < LocalBotSettingsProviderClass.Core.COVER_DIST_CLOSE)
			{
				flag = !Physics.Raycast(new Ray(suspectedPoint, direction), magnitude);
			}
			if (flag)
			{
				placeForCheck = new PlaceForCheck(freeClosePoint, suspectedPoint, type);
			}
		}
		if (placeForCheck == null)
		{
			placeForCheck = new PlaceForCheck(suspectedPoint, type);
		}
		return placeForCheck;
	}

	public PlaceForCheck method_5(BotOwner bot, Vector3 target)
	{
		NavMeshPath navMeshPath = new NavMeshPath();
		NavMesh.CalculatePath(bot.Transform.position, target, -1, navMeshPath);
		if (navMeshPath.status == NavMeshPathStatus.PathComplete)
		{
			return method_3(bot, target);
		}
		CustomNavigationPoint freeClosePoint = bot.Covers.GetFreeClosePoint(target, 0f);
		if (freeClosePoint == null)
		{
			return null;
		}
		return method_3(bot, freeClosePoint.Position);
	}

	public void method_6(BotOwner bot, Func<PlaceForCheck, bool> condiction, out int minIndex, out double minPoint)
	{
		minIndex = -1;
		minPoint = double.MaxValue;
		for (int i = 0; i < PlacesForCheck.Count; i++)
		{
			PlaceForCheck placeForCheck = PlacesForCheck[i];
			if (condiction(placeForCheck))
			{
				float coef = (placeForCheck.IsDanger ? bot.Settings.FileSettings.Mind.SIMPLE_POINT_CHOOSE_COEF : bot.Settings.FileSettings.Mind.DANGER_POINT_CHOOSE_COEF);
				float num = Time.time - placeForCheck.CreatedTime;
				float magnitude = (bot.Position - placeForCheck.Position).magnitude;
				double num2 = method_2(bot, num, magnitude, coef);
				if (num2 < minPoint)
				{
					minIndex = i;
					minPoint = num2;
				}
			}
		}
	}

	public bool method_7(float newestPosition)
	{
		float num = Time.time - newestPosition;
		if (num < 0f)
		{
			return true;
		}
		return num > LocalBotSettingsProviderClass.Core.LAST_SEEN_POS_LIFETIME;
	}
}
