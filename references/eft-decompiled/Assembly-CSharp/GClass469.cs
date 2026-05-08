using System;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class GClass469 : BotSearchData
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	public GClass469([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	public override void Activate()
	{
	}

	public override void UpdateByNode()
	{
		EnemyInfo enemyInfo = BotOwner_0.Memory.GoalEnemy;
		if (enemyInfo == null)
		{
			enemyInfo = BotOwner_0.Memory.LastEnemy;
		}
		BotOwner_0.Sprint(val: false);
		BotOwner_0.Mover.SetTargetMoveSpeed(1f);
		if (enemyInfo != null && enemyInfo.Person.HealthController.IsAlive)
		{
			if (enemyInfo.Distance < BotOwner_0.Settings.FileSettings.Boss.KILLA_SEARCH_METERS)
			{
				Float_2 = Time.time;
				if (BotOwner_0.Memory.IsInCover)
				{
					BotOwner_0.StopMove();
					BotOwner_0.LookData.SetLookPointByHearing();
				}
				else
				{
					method_6();
				}
				BotOwner_0.LookData.SetLookPointByHearing();
			}
			else
			{
				method_7(enemyInfo.CurrPosition);
				BotOwner_0.LookData.SetLookPointByHearing();
			}
			return;
		}
		if (Float_1 < Time.time)
		{
			Float_1 = Time.time + 10f;
			if (BotOwner_0.Memory.GoalTarget.HavePlaceTarget() && !BotOwner_0.Memory.GoalTarget.GoalTarget.Reacheble)
			{
				BotOwner_0.BotsGroup.PointChecked(BotOwner_0.Memory.GoalTarget.GoalTarget);
				Vector3 vector = GClass856.RandomHorizontal(-3f, 3f);
				if (BotOwner_0.Memory.GoalTarget.Position.HasValue)
				{
					GroupPoint closest = BotOwner_0.Covers.GetClosest(BotOwner_0.Memory.GoalTarget.Position.Value + vector, BotOwner_0.StartCorePoint.ConnectionGroupId);
					if (closest != null)
					{
						BotOwner_0.GoToPoint(closest.Position);
						return;
					}
				}
				Float_1 = Time.time + 2f;
				return;
			}
			if (BotOwner_0.Memory.GoalTarget.GoalTarget.Reacheble)
			{
				BotOwner_0.GoToPoint(BotOwner_0.Memory.GoalTarget.GoalTarget.Position, slowAtTheEnd: true, -1f, getUpWithCheck: false, mustHaveWay: false);
			}
		}
		BotOwner_0.LookData.SetLookPointByHearing();
	}

	public void method_6()
	{
		if (Float_0 < Time.time)
		{
			Float_0 = Time.time + 3f;
			EnemyInfo enemyInfo = BotOwner_0.Memory.GoalEnemy;
			if (enemyInfo == null)
			{
				enemyInfo = BotOwner_0.Memory.LastEnemy;
			}
			Vector3 pos = enemyInfo?.CurrPosition ?? ((!BotOwner_0.Memory.GoalTarget.Position.HasValue) ? BotOwner_0.Position : BotOwner_0.Memory.GoalTarget.Position.Value);
			CustomNavigationPoint freeClosePoint = BotOwner_0.Covers.GetFreeClosePoint(pos, 0f);
			BotOwner_0.GoToPoint(freeClosePoint);
		}
	}

	public void method_7(Vector3 currPosition)
	{
		if (Float_0 < Time.time)
		{
			Float_0 = Time.time + 1f;
			BotOwner_0.GoToPoint(currPosition);
		}
	}
}
