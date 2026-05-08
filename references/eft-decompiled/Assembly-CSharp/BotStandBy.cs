using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class BotStandBy : GClass429
{
	[NonSerialized]
	public float DIST_TO_SLEEP = 130f;

	[NonSerialized]
	public float DIST_TO_ACTIVATE = 110f;

	[NonSerialized]
	public float MIN_TIME_AFTER_HIT = 30f;

	[NonSerialized]
	public float NextCheckTime;

	[NonSerialized]
	public float NextTimePosibleGoTo;

	[NonSerialized]
	public BotStandByType StandByType_1 = BotStandByType.active;

	[NonSerialized]
	public Vector3? CurPoint;

	public BotStandByType StandByType
	{
		get
		{
			return StandByType_1;
		}
		set
		{
			BotOwner_0.BotPersonalStats.SetStandByState(StandByType_1);
			StandByType_1 = value;
		}
	}

	[field: NonSerialized]
	public bool CanDoStandBy { get; set; }

	public BotStandBy(BotOwner owner)
		: base(owner)
	{
	}

	public void InitPoints(float distToActivate, float distToSleep)
	{
		if (DebugBotData.UseDebugData && DebugBotData.Instance.NoSleepMode)
		{
			CanDoStandBy = false;
			return;
		}
		DIST_TO_SLEEP = distToSleep;
		DIST_TO_ACTIVATE = distToActivate;
		if (!(DIST_TO_SLEEP < 5f) && DIST_TO_ACTIVATE >= 5f)
		{
			if (!BotOwner_0.Settings.FileSettings.Mind.CAN_STAND_BY)
			{
				CanDoStandBy = false;
			}
			else
			{
				CanDoStandBy = true;
			}
		}
		else
		{
			CanDoStandBy = false;
		}
	}

	public void Update()
	{
		if (!CanDoStandBy || !(NextCheckTime < Time.time))
		{
			return;
		}
		NextCheckTime = Time.time + 10f;
		float num = float.MaxValue;
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in BotOwner_0.EnemiesController.EnemyInfos)
		{
			if (enemyInfo.Value.Distance < num)
			{
				num = enemyInfo.Value.Distance;
			}
		}
		foreach (KeyValuePair<IPlayer, BotSettingsClass> neutral in BotOwner_0.BotsGroup.Neutrals)
		{
			try
			{
				float magnitude = (neutral.Key.Transform.position - BotOwner_0.Position).magnitude;
				if (magnitude < num)
				{
					num = magnitude;
				}
			}
			catch (Exception)
			{
				BotOwner_0.BotsGroup.Neutrals.Remove(neutral.Key);
				return;
			}
		}
		switch (StandByType_1)
		{
		case BotStandByType.active:
			if (num > DIST_TO_SLEEP)
			{
				method_0();
			}
			break;
		case BotStandByType.paused:
		case BotStandByType.goToSave:
			if (num < DIST_TO_ACTIVATE)
			{
				Activate();
			}
			break;
		}
	}

	public void Activate()
	{
		if (StandByType_1 != BotStandByType.active)
		{
			CurPoint = null;
			StandByType = BotStandByType.active;
			BotOwner_0.GetPlayer.Sleep(value: false);
		}
	}

	public void UpdateNode()
	{
		if (!BotOwner_0.Settings.FileSettings.Mind.CAN_STAND_BY || !CanDoStandBy || StandByType_1 != BotStandByType.goToSave)
		{
			return;
		}
		if (!CurPoint.HasValue)
		{
			CurPoint = BotOwner_0.Covers.GetClosestPoint(BotOwner_0.Position).Position;
		}
		if ((CurPoint.Value - BotOwner_0.Position).sqrMagnitude < 1f)
		{
			method_1();
		}
		else if (NextTimePosibleGoTo < Time.time)
		{
			NextTimePosibleGoTo = Time.time + 15f;
			if (BotOwner_0.BotLay.IsLay)
			{
				BotOwner_0.BotLay.GetUp(withCheck: false);
			}
			BotOwner_0.Mover.SetPose(1f);
			if (BotOwner_0.GoToPoint(CurPoint.Value) != NavMeshPathStatus.PathComplete)
			{
				BotOwner_0.Mover.Teleport(CurPoint.Value);
			}
		}
	}

	public void GetHit()
	{
		NextCheckTime = Time.time + MIN_TIME_AFTER_HIT;
		Activate();
	}

	public void DebugActivate(bool val)
	{
	}

	public string DistancesInfo()
	{
		return "(" + DIST_TO_ACTIVATE.ToString("0") + "/" + DIST_TO_SLEEP.ToString("0") + ")";
	}

	public void method_0()
	{
		if (BotOwner_0.Settings.FileSettings.Mind.CAN_STAND_BY && !BotOwner_0.Medecine.FirstAid.Have2Do && StandByType_1 != BotStandByType.paused && StandByType_1 != BotStandByType.goToSave)
		{
			CurPoint = null;
			StandByType = BotStandByType.goToSave;
		}
	}

	public void method_1()
	{
		if (BotOwner_0.Settings.FileSettings.Mind.CAN_STAND_BY)
		{
			CurPoint = null;
			StandByType = BotStandByType.paused;
			BotOwner_0.Mover.SetPose(0f);
			BotOwner_0.GetPlayer.Sleep(value: true);
		}
	}
}
