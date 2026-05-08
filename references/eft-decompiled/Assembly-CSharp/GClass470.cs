using System;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class GClass470 : BotSearchData
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public GClass435 Gclass435_0;

	public GClass470([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	public override void Activate()
	{
		Gclass435_0 = BotOwner_0.Boss.BossLogic as GClass435;
	}

	public override void UpdateByNode()
	{
		EnemyInfo enemyInfo = BotOwner_0.Memory.GoalEnemy;
		if (enemyInfo == null)
		{
			enemyInfo = BotOwner_0.Memory.LastEnemy;
		}
		if (enemyInfo == null)
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
			return;
		}
		if (Gclass435_0 == null)
		{
			Gclass435_0 = BotOwner_0.Boss.BossLogic as GClass435;
		}
		if (Gclass435_0 != null && Time.time - enemyInfo.TimeLastSeen < BotOwner_0.Settings.FileSettings.Boss.GLUHAR_TIME_TO_ASSAULT && Gclass435_0.AssaultFollowersShallAttack)
		{
			BotOwner_0.Sprint(val: true);
			method_7(enemyInfo.CurrPosition);
			BotOwner_0.LookData.SetLookPointByHearing();
		}
		else
		{
			method_6();
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
