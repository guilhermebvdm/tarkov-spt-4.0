using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class BotLeaveData : GClass429
{
	[NonSerialized]
	public Vector3 PointToLeave;

	[NonSerialized]
	public GClass25 GotoPeriod;

	[NonSerialized]
	public GClass25 CheckPeriod;

	[NonSerialized]
	public bool CanLeave = true;

	[NonSerialized]
	public bool CanLeaveByMyself;

	[NonSerialized]
	public float DistToCanLeave = 300f;

	[NonSerialized]
	public float MinLeaveTime = 59940f;

	[field: NonSerialized]
	public bool WannaLeave { get; set; }

	[field: NonSerialized]
	public bool LeaveComplete { get; set; }

	public event Action<BotOwner> OnLeave;

	public BotLeaveData(BotOwner owner)
		: base(owner)
	{
	}

	public void Activate(float leaveDist)
	{
		DistToCanLeave = leaveDist;
		CanLeaveByMyself = DistToCanLeave > 1f;
		GotoPeriod = new GClass25(5f, method_0);
		CheckPeriod = new GClass25(10f, CheckCanLeave);
		bool flag = false;
		WildSpawnType role = BotOwner_0.Profile.Info.Settings.Role;
		if ((uint)role <= 1u)
		{
			flag = true;
		}
		CanLeaveByMyself = flag && CanLeaveByMyself;
	}

	public void DoLeaveExternal()
	{
		method_1();
	}

	public void CheckCanLeave()
	{
		if (!CanLeaveByMyself || WannaLeave || BotOwner_0.Memory.HaveEnemy || Time.time - BotOwner_0.ActivateTime < MinLeaveTime)
		{
			return;
		}
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in BotOwner_0.EnemiesController.EnemyInfos)
		{
			if (!(enemyInfo.Value.Distance >= DistToCanLeave))
			{
				return;
			}
		}
		foreach (KeyValuePair<IPlayer, BotSettingsClass> neutral in BotOwner_0.BotsGroup.Neutrals)
		{
			if (!((neutral.Key.Position - BotOwner_0.Position).sqrMagnitude >= DistToCanLeave * DistToCanLeave))
			{
				return;
			}
		}
		int num = 0;
		while (true)
		{
			if (num < BotOwner_0.BotsGroup.MembersCount)
			{
				if (!GClass2190.IsBossOrFollower(BotOwner_0.BotsGroup.Member(num).Profile.Info.Settings))
				{
					num++;
					continue;
				}
				break;
			}
			if (BotOwner_0.BotRequestController.CurRequest == null || !(BotOwner_0.BotRequestController.CurRequest is GClass592 gClass) || gClass.Requester.IsAI)
			{
				method_1();
			}
			break;
		}
	}

	public void UpdateByNode()
	{
		if (WannaLeave)
		{
			GotoPeriod.Update();
		}
	}

	public void UpdateFromLayer()
	{
		CheckPeriod.Update();
	}

	public void BlockLeave()
	{
		CanLeave = false;
	}

	public void RemoveFromMap()
	{
		this.OnLeave?.Invoke(BotOwner_0);
		this.OnLeave = null;
		BotOwner_0.Deactivate();
		BotOwner_0.Dispose();
		LeaveComplete = true;
		BotOwner_0.BotsGroup.BotGame.BotDespawn(BotOwner_0);
	}

	public void method_0()
	{
		if ((BotOwner_0.Position - PointToLeave).sqrMagnitude < 4f)
		{
			RemoveFromMap();
			return;
		}
		BotOwner_0.Steering.LookToMovingDirection();
		BotOwner_0.GoToPoint(PointToLeave);
	}

	public void method_1()
	{
		if (!CanLeave)
		{
			return;
		}
		BotZone botZone = BotOwner_0.BotsGroup.BotZone;
		if (botZone.UnSpawnPoints != null && botZone.UnSpawnPoints.Length != 0)
		{
			Vector3 position = BotOwner_0.Position;
			IPositionPoint[] unSpawnPoints = BotOwner_0.BotsGroup.BotZone.UnSpawnPoints;
			int nearEntity = GClass369.GetNearEntity(position, unSpawnPoints);
			PointToLeave = BotOwner_0.BotsGroup.BotZone.UnSpawnPoints[nearEntity].transform.position;
		}
		else
		{
			int nearEntity2 = GClass369.GetNearEntity(botZone.SpawnPointMarkers.Count, (int i) => BotOwner_0.Position - BotOwner_0.BotsGroup.BotZone.SpawnPointMarkers[i].SpawnPoint.Position);
			PointToLeave = BotOwner_0.BotsGroup.BotZone.SpawnPointMarkers[nearEntity2].SpawnPoint.Position;
		}
		BotOwner_0.StandBy.Activate();
		WannaLeave = true;
	}

	public void Dispose()
	{
		CheckPeriod = null;
		this.OnLeave = null;
	}

	[CompilerGenerated]
	public Vector3 method_2(int i)
	{
		return BotOwner_0.Position - BotOwner_0.BotsGroup.BotZone.SpawnPointMarkers[i].SpawnPoint.Position;
	}
}
