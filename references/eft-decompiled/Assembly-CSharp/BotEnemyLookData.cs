using System;
using EFT;
using UnityEngine;

public class BotEnemyLookData : GClass429
{
	[NonSerialized]
	public const float CHECK_PERIOD_SAFE = 0.5f;

	[NonSerialized]
	public const float DIST_TO_CHECK = 20f;

	[NonSerialized]
	public float PrevCheckTime;

	[NonSerialized]
	public float NextCheckPeriod;

	[NonSerialized]
	public float PrevResult;

	[NonSerialized]
	public float CheckPeriod = 1f;

	[NonSerialized]
	public bool OnlyIfVisible;

	public BotEnemyLookData(BotOwner owner, bool onlyIfVisible)
		: base(owner)
	{
		OnlyIfVisible = onlyIfVisible;
	}

	public void DoCheck()
	{
		if (NextCheckPeriod > Time.time)
		{
			return;
		}
		NextCheckPeriod = Time.time + 0.5f;
		bool flag = false;
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && goalEnemy.Distance < 20f && (!OnlyIfVisible || goalEnemy.IsVisible))
		{
			flag = BotOwner_0.IsEnemyLookingAtMe(goalEnemy);
		}
		if (flag)
		{
			float num = Time.time - PrevCheckTime;
			if (num < CheckPeriod)
			{
				PrevResult += num;
			}
			else
			{
				PrevResult = 0f;
			}
		}
		else
		{
			PrevResult = 0f;
		}
		PrevCheckTime = Time.time;
	}

	public bool IsEnemyLookAtMeForPeriod(float period)
	{
		if (Time.time - PrevCheckTime < CheckPeriod * 2f)
		{
			return PrevResult > period;
		}
		return false;
	}
}
