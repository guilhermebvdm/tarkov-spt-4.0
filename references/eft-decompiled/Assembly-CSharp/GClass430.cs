using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class GClass430 : ABossLogic, GInterface8
{
	[CompilerGenerated]
	public class Class163
	{
		public BotOwner bot;

		public GClass430 gclass430_0;

		public void method_0()
		{
			bot.CalledData.TryCall(new CallEnemyTargetInfo(), gclass430_0.BotOwner_0, helpOnlyWithEnemy: false);
		}
	}

	[CompilerGenerated]
	public class Class164
	{
		public Vector3 posToSuppress;

		public Func<Vector3> func_0;

		public Vector3 method_0()
		{
			return posToSuppress;
		}
	}

	[NonSerialized]
	public const float Float_0 = 10f;

	[NonSerialized]
	public const float Float_1 = 4f;

	[NonSerialized]
	public const float Float_2 = 30f;

	[NonSerialized]
	public const string String_0 = "BossBoarBorn";

	[NonSerialized]
	public const int Int_0 = 2;

	[NonSerialized]
	public const float Float_3 = 5f;

	[NonSerialized]
	public const float Float_4 = 5f;

	[NonSerialized]
	public const float Float_5 = 6400f;

	public float _lastTimeHit;

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	public float Float_9;

	[NonSerialized]
	public float Float_10 = -1f;

	[NonSerialized]
	public float Float_11;

	[NonSerialized]
	public float Float_12;

	public bool IsHitted => Time.time - _lastTimeHit < 10f;

	public GClass430(BotOwner owner, BotBoss bossLogic)
		: base(owner, bossLogic)
	{
		Float_8 = GClass856.GreateRandom(5f);
	}

	public void StartMoveToAttackPoint(int id)
	{
		Float_9 = Time.time;
	}

	public bool CanStartMoveToAttackPoint(BotOwner askingBot)
	{
		float num = Time.time - Float_10;
		float num2 = Time.time - Float_9;
		if (num > 0.5f)
		{
			return num2 > Float_8;
		}
		return false;
	}

	public override void Activate()
	{
		BotOwner_0.Memory.OnBulletNear += method_3;
		BotOwner_0.GetPlayer.BeingHitAction += method_0;
		BotOwner_0.Memory.OnGoalEnemyChanged += method_1;
		foreach (BotOwner follower in BotOwner_0.Boss.Followers)
		{
			follower.Memory.OnBulletNear += method_3;
		}
		BotOwner_0.Boss.OnFollowerStatusChange += method_4;
		BotOwner_0.BotsController.Bots.OnBotAdd += method_2;
		foreach (BotOwner botOwner in BotOwner_0.BotsController.Bots.BotOwners)
		{
			method_2(botOwner);
		}
		Singleton<BotEventHandler>.Instance.AnyEvent("BossBoarBorn");
	}

	public override void BossLogicUpdate()
	{
		method_5();
	}

	public override void SetPatrolMode()
	{
		PatrolPointChooserBasic pointChooser = PatrollingData.GetPointChooser(BotOwner_0, PatrolMode.bossCoverScouts, BotOwner_0.SpawnProfileData);
		BotOwner_0.PatrollingData.SetMode(PatrolMode.bossCoverScouts, pointChooser);
	}

	public bool WantSuppress()
	{
		return Float_11 > Time.time;
	}

	public void SetWantSuppress()
	{
		if (!(Float_12 > Time.time))
		{
			Float_12 = Time.time + 15f;
			Float_11 = Time.time + 5f;
		}
	}

	public void ReportEnemy(IPlayer enemy, Vector3 enemypos, EEnemyPartVisibleType visibleType)
	{
		BotOwner_0.BotsGroup.SetEnemyPos(enemy, enemy.Transform.position, enemy.WeaponRoot.position, visibleType);
	}

	public void method_0(DamageInfoStruct arg1, EBodyPart arg2, float arg3)
	{
		_lastTimeHit = Time.time;
	}

	public void method_1(BotOwner obj)
	{
		if (BotOwner_0.Memory.GoalEnemy != null)
		{
			if (Float_10 < 0f)
			{
				Float_10 = Time.time;
			}
		}
		else
		{
			Float_10 = -1f;
		}
	}

	public void method_2(BotOwner bot)
	{
		if (!(bot.BotsGroup.BotZone != BotOwner_0.BotsGroup.BotZone) && bot.Settings.FileSettings.Mind.MAY_BE_CALLED_FOR_HELP)
		{
			BotOwner_0.AITaskManager.RegisterDelayedTask(BotOwner_0, 5f, delegate
			{
				bot.CalledData.TryCall(new CallEnemyTargetInfo(), BotOwner_0, helpOnlyWithEnemy: false);
			});
		}
	}

	public void method_3(BotOwner bot, IPlayer source)
	{
		if ((source.Position - bot.Position).sqrMagnitude < 6400f || Float_6 > Time.time)
		{
			return;
		}
		Vector3 posToSuppress = source.Position;
		foreach (BotOwner follower in BotOwner_0.Boss.Followers)
		{
			follower.SuppressStationary.SetTargetGetter(() => posToSuppress);
		}
	}

	public void method_4(BotOwner follower, FollowerStatusChange status)
	{
		switch (status)
		{
		case FollowerStatusChange.Remove:
			follower.Memory.OnBulletNear -= method_3;
			break;
		case FollowerStatusChange.Add:
			follower.Memory.OnBulletNear += method_3;
			break;
		}
	}

	public void method_5()
	{
		if (Float_7 > Time.time)
		{
			return;
		}
		Float_7 = Time.time + 4f;
		float num = float.MinValue;
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			return;
		}
		int num2 = 0;
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in BotOwner_0.EnemiesController.EnemyInfos)
		{
			if (Time.time - enemyInfo.Value.GroupInfo.EnemyLastSeenTimeReal < 15f)
			{
				num2++;
			}
		}
		if (num2 < 2)
		{
			return;
		}
		BotOwner botOwner = null;
		for (int i = 0; i < BotOwner_0.BotsGroup.MembersCount; i++)
		{
			BotOwner botOwner2 = BotOwner_0.BotsGroup.Member(i);
			if (botOwner2.Memory.HaveEnemy)
			{
				float distance = botOwner2.Memory.GoalEnemy.Distance;
				if (distance > num)
				{
					num = distance;
					botOwner = botOwner2;
				}
			}
		}
		if (botOwner == null || !botOwner.Memory.IsInCover || botOwner.Memory.CurCustomCoverPoint.CanIShootToEnemy || botOwner.Boss.IamBoss || !method_6(botOwner))
		{
			return;
		}
		Vector3 vector = -botOwner.Memory.GoalEnemy.Direction;
		Vector3 vector2 = botOwner.Position + Vector3.up;
		float num3 = 2f;
		if (!Physics.Raycast(vector2, vector, num3, BotOwner_0.LookSensor.Mask))
		{
			Vector3 placeToThrow = vector2 + GClass855.NormalizeFastSelf(vector) * num3;
			if (BotOwner_0.BotRequestController.TryActivateThrowGrenadeRequest(placeToThrow, ThrowWeapType.smoke_grenade, out var request))
			{
				request.AddPossibleExecutors(botOwner);
				Float_7 = Time.time + 60f;
			}
		}
	}

	public bool method_6(BotOwner mem)
	{
		return mem.WeaponManager.Grenades.HaveGrenadeOfType(ThrowWeapType.smoke_grenade);
	}

	public override void Dispose()
	{
		BotOwner_0.Memory.OnBulletNear -= method_3;
		BotOwner_0.GetPlayer.BeingHitAction -= method_0;
		BotOwner_0.Memory.OnGoalEnemyChanged -= method_1;
		foreach (BotOwner follower in BotOwner_0.Boss.Followers)
		{
			follower.Memory.OnBulletNear -= method_3;
		}
		BotOwner_0.BotsController.Bots.OnBotAdd -= method_2;
	}
}
