using System;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class GClass471 : BotSearchData
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	public GClass471([NotNull] BotOwner owner)
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
		if (Time.time - Float_1 < BotOwner_0.Settings.FileSettings.Boss.KILLA_SEARCH_SEC_STOP_AFTER_COMING)
		{
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
		else if (enemyInfo != null && enemyInfo.Person.HealthController.IsAlive)
		{
			if (enemyInfo.Distance < BotOwner_0.Settings.FileSettings.Boss.KILLA_SEARCH_METERS)
			{
				Float_1 = Time.time;
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
		}
		else
		{
			BotOwner_0.LookData.SetLookPointByHearing();
		}
	}

	public void method_6()
	{
		if (Float_0 < Time.time)
		{
			Float_0 = Time.time + 1f;
			CustomNavigationPoint freeClosePoint = BotOwner_0.Covers.GetFreeClosePoint(BotOwner_0.Position, 0f);
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
