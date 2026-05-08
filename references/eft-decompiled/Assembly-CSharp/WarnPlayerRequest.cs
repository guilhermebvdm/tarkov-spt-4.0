using System;
using EFT;
using UnityEngine;

public class WarnPlayerRequest
{
	[NonSerialized]
	public const float TIME_TO_SHOOT = 3f;

	[NonSerialized]
	public const float TIME_TO_SHOW_AND_SAY = 3f;

	[NonSerialized]
	public const int TRIGGER_DOWN_TO_COMPLETE = 3;

	[NonSerialized]
	public float ComeToPointTime;

	[NonSerialized]
	public float NextUpdTimer;

	[NonSerialized]
	public float SayTime;

	[NonSerialized]
	public float SayTime2;

	[NonSerialized]
	public int ActionsDone;

	[NonSerialized]
	public int ShootsDone;

	[NonSerialized]
	public Vector3 CachedPointToShoot;

	[NonSerialized]
	public float StayEndTime;

	[NonSerialized]
	public bool IsStopSays;

	[NonSerialized]
	public bool IsStopSays2;

	[NonSerialized]
	public EnemyInfo WarnTargetEnemyInfo;

	[NonSerialized]
	public float WARN_TARGET_VISION_CHECK_PERIOD = 1.17f;

	[NonSerialized]
	public float SDIST_TO_POSSIBLE_START_WARNING = 225f;

	[NonSerialized]
	public float LastWarnTargetVisionCheck;

	[NonSerialized]
	public bool CachedCheckLookToWarnTarget;

	[NonSerialized]
	public float OutDist;

	[NonSerialized]
	public float NextCloseCheck;

	[NonSerialized]
	public bool IsClose;

	[NonSerialized]
	public GClass428 DataWarn;

	[NonSerialized]
	public int PreferableExecuter = -1;

	[NonSerialized]
	public float EndPreferableExecuterPeriod;

	[NonSerialized]
	public bool CanExecuteByMyself;

	[NonSerialized]
	public bool EndIfCantExecute;

	[NonSerialized]
	public BotOwner Executor;

	[NonSerialized]
	public float TakenTime;

	[field: NonSerialized]
	public Player playerToWarn { get; }

	[field: NonSerialized]
	public StateWarnPlayer StateWarnPlayer { get; set; }

	public float EndTime => TakenTime + 25f;

	public WarnPlayerRequest(BotOwner executor, Player player, GClass428 dataWarn)
	{
		DataWarn = dataWarn;
		Executor = executor;
		StateWarnPlayer = StateWarnPlayer.goTo;
		CanExecuteByMyself = true;
		playerToWarn = player;
		EndIfCantExecute = false;
		TakenTime = Time.time;
		executor.ShootData.OnTriggerPressed += method_6;
		OutDist = playerToWarn.Profile.Side switch
		{
			EPlayerSide.Bear => method_2(executor, playerToWarn), 
			EPlayerSide.Usec => method_1(executor, playerToWarn), 
			_ => method_3(executor, playerToWarn), 
		} + method_0(executor, playerToWarn);
	}

	public void SetWarnStarted()
	{
		DataWarn.SetStartWarnTime();
	}

	public Vector3 PointToShoot()
	{
		return CachedPointToShoot;
	}

	public float method_0(BotOwner executor, Player playerToWarn)
	{
		if (executor.BotsGroup.IsAlly(playerToWarn))
		{
			return executor.Settings.FileSettings.Boss.BOSS_DIST_TO_WARNING_OUT_DELTA_ALLY;
		}
		return executor.Settings.FileSettings.Boss.BOSS_DIST_TO_WARNING_OUT_DELTA;
	}

	public float method_1(BotOwner executor, Player playerToWarn)
	{
		if (executor.BotsGroup.IsAlly(playerToWarn))
		{
			return executor.Settings.FileSettings.Boss.BOSS_DIST_TO_WARNING_USEC_ALLY;
		}
		return executor.Settings.FileSettings.Boss.BOSS_DIST_TO_WARNING_USEC;
	}

	public float method_2(BotOwner executor, Player playerToWarn)
	{
		if (executor.BotsGroup.IsAlly(playerToWarn))
		{
			return executor.Settings.FileSettings.Boss.BOSS_DIST_TO_WARNING_BEAR_ALLY;
		}
		return executor.Settings.FileSettings.Boss.BOSS_DIST_TO_WARNING_BEAR;
	}

	public float method_3(BotOwner executor, Player playerToWarn)
	{
		if (executor.BotsGroup.IsAlly(playerToWarn))
		{
			return executor.Settings.FileSettings.Boss.BOSS_DIST_TO_WARNING_ALLY;
		}
		return executor.Settings.FileSettings.Boss.BOSS_DIST_TO_WARNING;
	}

	public bool ShouldWarnFromCurrentPosition()
	{
		if (Time.time - TakenTime > 4f && !Executor.Mover.HasPathAndNoComplete)
		{
			return true;
		}
		if (LastWarnTargetVisionCheck + WARN_TARGET_VISION_CHECK_PERIOD < Time.time && (Executor.Position - playerToWarn.Position).sqrMagnitude < SDIST_TO_POSSIBLE_START_WARNING)
		{
			CachedCheckLookToWarnTarget = Executor.LookSensor.CheckLookSimple(Executor.GetPlayer, playerToWarn);
			return CachedCheckLookToWarnTarget;
		}
		return CachedCheckLookToWarnTarget;
	}

	public bool ShallEndStay()
	{
		Executor.Steering.LookToPoint(playerToWarn.Position);
		return StayEndTime < Time.time;
	}

	public bool CanProceed()
	{
		if (!(Executor != null))
		{
			return false;
		}
		if (method_4())
		{
			return false;
		}
		if (Executor.Memory.GoalEnemy != null && Executor.Memory.GoalEnemy.IsVisible)
		{
			return false;
		}
		return true;
	}

	public bool UpdateSay(bool onlyRoger = false)
	{
		Executor.StopMove();
		Executor.Steering.LookToPoint(playerToWarn.Position);
		if (SayTime2 > 0f && Time.time - SayTime2 > 3f)
		{
			return true;
		}
		if (SayTime > 0f && SayTime2 <= 0f)
		{
			if (Time.time - SayTime > 3f)
			{
				if (!IsStopSays)
				{
					IsStopSays = true;
					SayTime2 = Time.time;
					method_7(onlyRoger);
					Executor.Gesture.TryGestus(EInteraction.ThereGesture, withAimingDelay: false);
					method_8();
				}
				return false;
			}
		}
		else if (!IsStopSays2)
		{
			IsStopSays2 = true;
			SayTime = Time.time;
			method_7();
			Executor.Gesture.TryGestus(EInteraction.GetOffGesture, withAimingDelay: false);
			method_8();
		}
		return false;
	}

	public void Complete()
	{
		Executor.AIData.BotOwner.BotsGroup.BotGroupWarnData.WarnComplete(playerToWarn);
		ActionsDone = 0;
	}

	public void NextState()
	{
		switch (StateWarnPlayer)
		{
		case StateWarnPlayer.goTo:
			method_11();
			break;
		case StateWarnPlayer.wait:
			method_11();
			break;
		case StateWarnPlayer.say1:
		case StateWarnPlayer.shoot:
		case StateWarnPlayer.stay:
			if (method_10())
			{
				Complete();
			}
			else
			{
				method_11();
			}
			break;
		}
	}

	public bool method_4()
	{
		if (NextCloseCheck > Time.time)
		{
			return IsClose;
		}
		NextCloseCheck = Time.time + 2f;
		float num = float.MaxValue;
		bool flag = false;
		for (int i = 0; i < Executor.BotsGroup.MembersCount; i++)
		{
			float sqrMagnitude = (Executor.BotsGroup.Member(i).Position - playerToWarn.Position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				flag = true;
				num = sqrMagnitude;
			}
		}
		if (!flag)
		{
			return false;
		}
		IsClose = num > OutDist * OutDist;
		return IsClose;
	}

	public void method_5()
	{
		Vector3 vector = new Vector3(GClass856.Random(-0.5f, 0.5f), GClass856.Random(0.3f, 1f), GClass856.Random(-0.5f, 0.5f));
		CachedPointToShoot = playerToWarn.PlayerBones.Head.position + vector;
	}

	public void method_6()
	{
		method_5();
		if (StateWarnPlayer == StateWarnPlayer.shoot)
		{
			ShootsDone++;
			if (ShootsDone > 3)
			{
				NextState();
			}
		}
	}

	public void method_7(bool onlyRoger = false)
	{
		if (!onlyRoger && !GClass856.RandomBool())
		{
			Executor.BotTalk.TrySay(EPhraseTrigger.OnMutter, withGroupDelay: true);
		}
		else
		{
			Executor.BotTalk.TrySay(EPhraseTrigger.Roger, withGroupDelay: true);
		}
	}

	public void method_8()
	{
		if (playerToWarn != null && playerToWarn.IsAI && playerToWarn.AIData.BotOwner.BotState == EBotState.Active)
		{
			playerToWarn.AIData.BotOwner.PatrollingData.FindNextPoint(withSetting: true, withoutNext: false, method_9);
			playerToWarn.AIData.BotOwner.Gesture.TryGestus(EInteraction.OkGesture, withAimingDelay: true);
		}
	}

	public bool method_9(PatrolPoint point)
	{
		float num = GClass856.SqrDistance(Executor.Boss.Position, point.Position);
		if (Executor.BotsGroup.IsAlly(playerToWarn))
		{
			return num > Executor.Boss.Owner.Settings.FileSettings.Boss.BOSS_DIST_TO_WARNING_OUT_SQRT_ALLY;
		}
		return num > Executor.Boss.Owner.Settings.FileSettings.Boss.BOSS_DIST_TO_WARNING_OUT_SQRT * 1.25f;
	}

	public bool method_10()
	{
		return IsClose;
	}

	public void method_11()
	{
		ActionsDone++;
		if (ActionsDone > LocalBotSettingsProviderClass.Core.MAX_WARNS_BEFORE_KILL)
		{
			if (Executor.BotsGroup.Enemies.ContainsKey(playerToWarn))
			{
				Complete();
				return;
			}
			Executor.BotsGroup.AddEnemy(playerToWarn, EBotEnemyCause.rndWanrRequest);
			Complete();
		}
		else if (ActionsDone != 1 && GClass856.IsTrue100(Executor.Settings.FileSettings.Mind.CHANCE_TO_STAY_WHEN_WARN_PLAYER_100))
		{
			StayEndTime = Time.time + 4f;
			StateWarnPlayer = StateWarnPlayer.stay;
		}
		else if (GClass856.IsTrue100(Executor.Settings.FileSettings.Mind.CHANCE_SHOOT_WHEN_WARN_PLAYER_100))
		{
			StateWarnPlayer = StateWarnPlayer.shoot;
			method_5();
		}
		else
		{
			StateWarnPlayer = StateWarnPlayer.say1;
		}
	}

	public void Dispose()
	{
		if (Executor != null)
		{
			Executor.ShootData.OnTriggerPressed -= method_6;
		}
		Executor.AIData.BotOwner.BotsGroup.BotGroupWarnData.DisposeCallback(playerToWarn);
	}
}
