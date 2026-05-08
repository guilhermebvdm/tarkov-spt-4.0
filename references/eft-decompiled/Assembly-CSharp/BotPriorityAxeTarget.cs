using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class BotPriorityAxeTarget : GClass429
{
	[CompilerGenerated]
	public class Class250
	{
		public EnemyInfo closestPerson;

		public bool method_0(GroupPoint point)
		{
			Vector3 vector = point.Position - closestPerson.CurrPosition;
			float num = Mathf.Abs(vector.y);
			vector.y = 0f;
			if (num > 1.5f)
			{
				return false;
			}
			return true;
		}
	}

	[NonSerialized]
	public const float CHECK_PERIOD = 2f;

	[NonSerialized]
	public const float Y_DELTA = 1.5f;

	[NonSerialized]
	public float NextCheckPeriod;

	[NonSerialized]
	public float NextGo;

	[NonSerialized]
	public float RunDist;

	[NonSerialized]
	public float RunSDist;

	[NonSerialized]
	public CustomNavigationPoint ClosestPoint;

	[NonSerialized]
	public EnemyInfo ClosestPerson;

	[NonSerialized]
	public bool AllPursuit_1;

	[NonSerialized]
	public float PrevDistPursuit;

	[NonSerialized]
	public float PossibleRadiusLostTime;

	public bool HavePointToGo => ClosestPoint != null;

	public bool AllPursuit
	{
		get
		{
			return AllPursuit_1;
		}
		set
		{
			AllPursuit_1 = value;
			if (AllPursuit_1)
			{
				PrevDistPursuit = BotOwner_0.Settings.FileSettings.Mind.MAX_DIST_TO_PERSUE_AXEMAN;
				BotOwner_0.Settings.FileSettings.Mind.MAX_DIST_TO_PERSUE_AXEMAN = 200f;
				RunDist = 200f;
				RunSDist = RunDist * RunDist;
			}
			else
			{
				BotOwner_0.Settings.FileSettings.Mind.MAX_DIST_TO_PERSUE_AXEMAN = PrevDistPursuit;
				RunDist = BotOwner_0.Settings.FileSettings.Mind.MAX_DIST_TO_RUN_PERSUE_AXEMAN;
				RunSDist = RunDist * RunDist;
			}
		}
	}

	public BotPriorityAxeTarget(BotOwner owner)
		: base(owner)
	{
		RunDist = BotOwner_0.Settings.FileSettings.Mind.MAX_DIST_TO_RUN_PERSUE_AXEMAN;
		RunSDist = RunDist * RunDist;
	}

	public void FindTarget()
	{
		if (!BotOwner_0.EnemiesController.CanPursueAxeman || !(NextCheckPeriod < Time.time))
		{
			return;
		}
		NextCheckPeriod = Time.time + 2f;
		float num = float.MaxValue;
		EnemyInfo closestPerson = null;
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in BotOwner_0.EnemiesController.EnemyInfos)
		{
			bool flag = enemyInfo.Key.AIData.ShallPursuit || AllPursuit;
			Player getPlayer = enemyInfo.Value.Owner.GetPlayer;
			bool flag2 = false;
			if (AllPursuit)
			{
				flag2 = true;
			}
			else
			{
				foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo2 in BotOwner_0.EnemiesController.EnemyInfos)
				{
					Player getPlayer2 = enemyInfo2.Value.Owner.GetPlayer;
					if (getPlayer.GroupId == getPlayer2.GroupId && !getPlayer2.AIData.ShallPursuit)
					{
						flag2 = true;
						break;
					}
				}
			}
			if (flag2 && flag && enemyInfo.Value.Distance < BotOwner_0.Settings.FileSettings.Mind.MAX_DIST_TO_PERSUE_AXEMAN && enemyInfo.Value.Distance < num)
			{
				num = enemyInfo.Value.Distance;
				closestPerson = enemyInfo.Value;
			}
		}
		ClosestPerson = closestPerson;
		CustomNavigationPoint customNavigationPoint = null;
		if (closestPerson != null)
		{
			customNavigationPoint = BotOwner_0.Covers.GetClosestPoint(closestPerson.CurrPosition, delegate(GroupPoint point)
			{
				Vector3 vector = point.Position - closestPerson.CurrPosition;
				float num2 = Mathf.Abs(vector.y);
				vector.y = 0f;
				return !(num2 > 1.5f);
			});
			if (customNavigationPoint == null)
			{
				customNavigationPoint = BotOwner_0.Covers.GetClosestPoint(BotOwner_0.Position);
			}
		}
		if (customNavigationPoint != null)
		{
			ClosestPoint = customNavigationPoint;
		}
		else if (closestPerson == null)
		{
			ClosestPoint = null;
		}
	}

	public bool IsPointInsideDangerZone()
	{
		if (ClosestPoint != null && GClass369.IsPointInsideDangerZone(BotOwner_0, ClosestPoint.BasePosition))
		{
			return true;
		}
		return false;
	}

	public void ManualUpdate()
	{
		method_0();
	}

	public bool IsInPossibleRadius()
	{
		if (ClosestPerson == null)
		{
			return false;
		}
		bool result;
		if (result = ClosestPerson.Distance < RunDist)
		{
			PossibleRadiusLostTime = Time.time + 15f;
		}
		if (PossibleRadiusLostTime > Time.time)
		{
			return true;
		}
		return result;
	}

	public void method_0()
	{
		if (ClosestPoint == null)
		{
			return;
		}
		float sqrMagnitude = (ClosestPoint.Position - BotOwner_0.Position).sqrMagnitude;
		BotOwner_0.LookData.SetLookPointByHearing();
		if (sqrMagnitude < 1f)
		{
			BotOwner_0.StopMove();
		}
		else
		{
			BotOwner_0.SetTargetMoveSpeed(1f);
			if (NextGo < Time.time)
			{
				NextGo = Time.time + 2f;
				BotOwner_0.GoToPoint(ClosestPoint);
			}
		}
		bool val = sqrMagnitude < RunSDist && sqrMagnitude > 9f && BotOwner_0.CanSprintPlayer;
		BotOwner_0.Sprint(val);
	}
}
