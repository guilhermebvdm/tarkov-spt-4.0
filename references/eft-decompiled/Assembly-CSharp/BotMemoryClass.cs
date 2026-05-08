using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using JetBrains.Annotations;
using UnityEngine;

public class BotMemoryClass
{
	[CompilerGenerated]
	public class Class1095
	{
		public BotMemoryClass BotMemoryClass;

		public IPlayer enemy;

		public void method_0(EDamageType health)
		{
			BotMemoryClass.DeleteInfoAboutEnemy(enemy);
		}
	}

	[CompilerGenerated]
	public class Class1096
	{
		public BotMemoryClass BotMemoryClass;

		public DamageInfoStruct damageInfo;

		public void method_0()
		{
			if (!BotMemoryClass.BotOwner_0.IsDead)
			{
				BotMemoryClass.method_8(damageInfo);
			}
		}
	}

	public const float SDIST_TO_AFFECT_CLOSE_ENEMY = 625f;

	public const float SDIST_CLOSE_COVER = 1.6f;

	public PlaceForCheck CurrentPlaceForCheck;

	public DangerDataClass DangerData;

	public BotCurrentCoverInfoClass BotCurrentCoverInfo;

	public Vector3 ActivatedPos;

	public BotObserveDataClass botObserveData;

	public GoalTargetClass GoalTarget;

	[NonSerialized]
	public BotsGroup BotsGroup_0;

	[NonSerialized]
	public BotOwner BotOwner_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public EnemyInfo EnemyInfo_0;

	[NonSerialized]
	public PlaceForCheck PlaceForCheck_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public BotLastBlindEffectModifierClass BotLastBlindEffectModifierClass;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public float Float_5;

	[CompilerGenerated]
	private Action<IPlayer> action_0;

	[CompilerGenerated]
	private Action<BotOwner> action_1;

	[CompilerGenerated]
	private Action<bool, CustomNavigationPoint> action_2;

	[CompilerGenerated]
	private Action<bool> action_3;

	[CompilerGenerated]
	private Action<Vector3?> action_4;

	[CompilerGenerated]
	private Action<BotOwner, IPlayer> action_5;

	public float nextTryMoveToEnemyLogTime;

	[NonSerialized]
	[CompilerGenerated]
	public GClass579 Gclass579_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_6;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_7 = -1000f;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_8;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_2;

	[NonSerialized]
	[CompilerGenerated]
	public EnemyInfo EnemyInfo_1;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_9;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_3 = true;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_4;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_10 = -900000f;

	public CustomNavigationPoint CurCustomCoverPoint => BotCurrentCoverInfo.CovPoint;

	public GClass579 LastDamageData
	{
		[CompilerGenerated]
		get
		{
			return Gclass579_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass579_0 = value;
		}
	}

	public bool LastDamageDataActive
	{
		get
		{
			if (LastDamageData != null)
			{
				return LastDamageData.IsActive();
			}
			return false;
		}
	}

	public bool IsDamaged => Float_1 > Time.time;

	public float ComeToCoverTime
	{
		[CompilerGenerated]
		get
		{
			return Float_6;
		}
		[CompilerGenerated]
		set
		{
			Float_6 = value;
		}
	}

	public float LeaveCoverTime => BotCurrentCoverInfo.LeaveCoverTime;

	public float LastTimeHit
	{
		[CompilerGenerated]
		get
		{
			return Float_7;
		}
		[CompilerGenerated]
		set
		{
			Float_7 = value;
		}
	}

	public Vector3 LastHitPos
	{
		[CompilerGenerated]
		get
		{
			return Vector3_0;
		}
		[CompilerGenerated]
		set
		{
			Vector3_0 = value;
		}
	}

	public float EnemySetTime
	{
		[CompilerGenerated]
		get
		{
			return Float_8;
		}
		[CompilerGenerated]
		set
		{
			Float_8 = value;
		}
	}

	public bool ShallRunIfNoAmmo
	{
		[CompilerGenerated]
		get
		{
			return Bool_2;
		}
		[CompilerGenerated]
		set
		{
			Bool_2 = value;
		}
	}

	public bool ShallChangeCoverToFindEnemy => Time.time - ComeToCoverTime > BotOwner_0.Settings.FileSettings.Cover.WAIT_INT_COVER_FINDING_ENEMY;

	public EnemyInfo LastEnemy
	{
		[CompilerGenerated]
		get
		{
			return EnemyInfo_1;
		}
		[CompilerGenerated]
		set
		{
			EnemyInfo_1 = value;
		}
	}

	public bool IsInCover
	{
		get
		{
			if (CurCustomCoverPoint == null)
			{
				return false;
			}
			return Bool_0;
		}
		set
		{
			if (value == Bool_0)
			{
				return;
			}
			if (value)
			{
				if (CurCustomCoverPoint == null)
				{
					return;
				}
				ComeToCoverTime = Time.time;
				BotOwner_0.DangerPointsData.SetAllDangerNull();
				BotOwner_0.Sprint(val: false);
				BotCurrentCoverInfo.ComeToCover();
			}
			else if (Bool_0)
			{
				BotOwner_0.BotPersonalStats.LeaveCover();
				BotCurrentCoverInfo.Leaved();
			}
			Bool_0 = value;
			if (Bool_0)
			{
				action_2?.Invoke(Bool_0, CurCustomCoverPoint);
			}
		}
	}

	public EnemyInfo GoalEnemy
	{
		get
		{
			return EnemyInfo_0;
		}
		set
		{
			if (EnemyInfo_0 == value)
			{
				return;
			}
			if (value == null || (EnemyInfo_0 != value && BotOwner_0.HealthController.IsAlive))
			{
				BotOwner_0.AimingManager.CurrentAiming.LoseTarget();
			}
			if (EnemyInfo_0 != null)
			{
				Singleton<GameWorld>.Instance.GetEverExistedPlayerByID(EnemyInfo_0.Person.ProfileId).BeingHitAction -= method_4;
				LastEnemy = EnemyInfo_0;
			}
			bool flag = EnemyInfo_0 != value;
			EnemyInfo_0 = value;
			if (EnemyInfo_0 != null)
			{
				Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(EnemyInfo_0.Person.ProfileId).BeingHitAction += method_4;
				method_0();
			}
			if (action_1 != null && flag)
			{
				action_1(BotOwner_0);
			}
			if (EnemyInfo_0 != null)
			{
				EnemySetTime = Time.time;
				if (!EnemyInfo_0.IsVisible)
				{
					BotOwner_0.AimingManager.CurrentAiming.LoseTarget();
				}
			}
			if (value != null)
			{
				DangerData.TargetNull();
			}
			else
			{
				method_6();
			}
		}
	}

	public float LastEnemyTimeSeen
	{
		[CompilerGenerated]
		get
		{
			return Float_9;
		}
		[CompilerGenerated]
		set
		{
			Float_9 = value;
		}
	}

	public bool IsPeace
	{
		[CompilerGenerated]
		get
		{
			return Bool_3;
		}
		[CompilerGenerated]
		set
		{
			Bool_3 = value;
		}
	}

	public bool AttackImmediately
	{
		[CompilerGenerated]
		get
		{
			return Bool_4;
		}
		[CompilerGenerated]
		set
		{
			Bool_4 = value;
		}
	}

	public float UnderFireTime
	{
		[CompilerGenerated]
		get
		{
			return Float_10;
		}
		[CompilerGenerated]
		set
		{
			Float_10 = value;
		}
	}

	public bool IsUnderFire => Float_4 > Time.time;

	public bool HaveEnemy => GoalEnemy != null;

	public bool HaveGoal => GoalTarget.HaveMainTarget();

	public event Action<IPlayer> OnAddEnemy
	{
		[CompilerGenerated]
		add
		{
			Action<IPlayer> action = action_0;
			Action<IPlayer> action2;
			do
			{
				action2 = action;
				Action<IPlayer> value2 = (Action<IPlayer>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IPlayer> action = action_0;
			Action<IPlayer> action2;
			do
			{
				action2 = action;
				Action<IPlayer> value2 = (Action<IPlayer>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<BotOwner> OnGoalEnemyChanged
	{
		[CompilerGenerated]
		add
		{
			Action<BotOwner> action = action_1;
			Action<BotOwner> action2;
			do
			{
				action2 = action;
				Action<BotOwner> value2 = (Action<BotOwner>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<BotOwner> action = action_1;
			Action<BotOwner> action2;
			do
			{
				action2 = action;
				Action<BotOwner> value2 = (Action<BotOwner>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<bool, CustomNavigationPoint> OnInCoverChange
	{
		[CompilerGenerated]
		add
		{
			Action<bool, CustomNavigationPoint> action = action_2;
			Action<bool, CustomNavigationPoint> action2;
			do
			{
				action2 = action;
				Action<bool, CustomNavigationPoint> value2 = (Action<bool, CustomNavigationPoint>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<bool, CustomNavigationPoint> action = action_2;
			Action<bool, CustomNavigationPoint> action2;
			do
			{
				action2 = action;
				Action<bool, CustomNavigationPoint> value2 = (Action<bool, CustomNavigationPoint>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<bool> OnPeaceChange
	{
		[CompilerGenerated]
		add
		{
			Action<bool> action = action_3;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<bool> action = action_3;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<Vector3?> OnSpottedByHit
	{
		[CompilerGenerated]
		add
		{
			Action<Vector3?> action = action_4;
			Action<Vector3?> action2;
			do
			{
				action2 = action;
				Action<Vector3?> value2 = (Action<Vector3?>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_4, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<Vector3?> action = action_4;
			Action<Vector3?> action2;
			do
			{
				action2 = action;
				Action<Vector3?> value2 = (Action<Vector3?>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_4, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<BotOwner, IPlayer> OnBulletNear
	{
		[CompilerGenerated]
		add
		{
			Action<BotOwner, IPlayer> action = action_5;
			Action<BotOwner, IPlayer> action2;
			do
			{
				action2 = action;
				Action<BotOwner, IPlayer> value2 = (Action<BotOwner, IPlayer>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_5, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<BotOwner, IPlayer> action = action_5;
			Action<BotOwner, IPlayer> action2;
			do
			{
				action2 = action;
				Action<BotOwner, IPlayer> value2 = (Action<BotOwner, IPlayer>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_5, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void method_0()
	{
		if (EnemyInfo_0 == null || !(Time.time - EnemyInfo_0.PersonalLastSeenTime < 5f))
		{
			return;
		}
		foreach (BotOwner botOwner in BotOwner_0.BotsGroup.BotGame.BotsController.Bots.BotOwners)
		{
			if (BotOwner_0.SDistTo(botOwner.Position) < BotOwner_0.Settings.FileSettings.Mind.SDIST_TO_DELIVER_INFO_WHEN_ENEMY)
			{
				botOwner.BotsGroup.ReportAboutEnemy(EnemyInfo_0.Person, EEnemyPartVisibleType.Visible, botOwner);
			}
		}
	}

	public BotMemoryClass(BotOwner owner, BotsGroup botsGroup)
	{
		GoalTarget = new GoalTargetClass(owner);
		BotLastBlindEffectModifierClass = new BotLastBlindEffectModifierClass(1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f);
		botObserveData = new BotObserveDataClass(owner);
		DangerData = new DangerDataClass();
		BotCurrentCoverInfo = new BotCurrentCoverInfoClass(owner);
		BotOwner_0 = owner;
		BotsGroup_0 = botsGroup;
	}

	public void method_1(IEffect obj)
	{
		if (obj is GInterface342)
		{
			switch (obj.BodyPart)
			{
			case EBodyPart.LeftLeg:
			case EBodyPart.RightLeg:
				BotOwner_0.BotTalk.TrySay(EPhraseTrigger.LegBroken);
				break;
			case EBodyPart.LeftArm:
			case EBodyPart.RightArm:
				BotOwner_0.BotTalk.TrySay(EPhraseTrigger.HandBroken);
				break;
			}
		}
	}

	public void Spotted(bool byHit, Vector3? from = null, float? secToBeSpotted = null)
	{
		if (IsInCover)
		{
			if (CurCustomCoverPoint.AlwaysGood && BotOwner_0.Profile.Info.Settings.Role == WildSpawnType.marksman)
			{
				return;
			}
			float period = secToBeSpotted ?? BotOwner_0.Settings.FileSettings.Cover.MAX_SPOTTED_TIME_SEC;
			foreach (CustomNavigationPoint closePoint in BotOwner_0.BotsGroup.CoverPointMaster.GetClosePoints(BotOwner_0.Transform.position, BotOwner_0, BotOwner_0.Settings.FileSettings.Cover.SPOTTED_COVERS_RADIUS))
			{
				closePoint.Spotted(period);
			}
			IsInCover = false;
			BotCurrentCoverInfo.Spotted();
			if (byHit && from.HasValue && (GoalEnemy == null || GoalEnemy.Distance > BotOwner_0.LookSensor.MaxShootDist))
			{
				BotOwner_0.DangerPointsData.AddPointOfDanger(new PlaceForCheck(from.Value, PlaceForCheckType.simple), realDanger: true);
			}
			if (byHit)
			{
				action_4?.Invoke(from);
			}
		}
		BotOwner_0.WeaponManager.Stationary.Spotted();
	}

	public bool LastEnemyVisionOld(float delta)
	{
		return Time.time - Float_0 > delta;
	}

	public void SetCoverPoints(CustomNavigationPoint value, string debugdata = "")
	{
		if (value == null || value != CurCustomCoverPoint)
		{
			BotCurrentCoverInfo.SetCover(value);
			if (CurCustomCoverPoint != null)
			{
				CurCustomCoverPoint.SetOwner(BotOwner_0);
				float sqrMagnitude = (CurCustomCoverPoint.Position - BotOwner_0.Transform.position).sqrMagnitude;
				IsInCover = sqrMagnitude < 1f;
			}
		}
	}

	public void LoseVisionCurrentEnemy()
	{
		Float_0 = Time.time;
	}

	public void CheckIsInCover2()
	{
		if (CurCustomCoverPoint != null)
		{
			float sqrMagnitude = (CurCustomCoverPoint.Position - BotOwner_0.Transform.position).sqrMagnitude;
			IsInCover = sqrMagnitude < 1.6f;
		}
		else
		{
			IsInCover = false;
		}
	}

	public void ManualUpdate(float deltaTime)
	{
		method_7();
		method_2();
	}

	public void method_2()
	{
		if (!(Float_2 > Time.time))
		{
			Float_2 = Time.time + 2f;
			CheckIsPeace();
		}
	}

	public void DeleteInfoAboutEnemy(IPlayer gamePerson)
	{
		if (gamePerson != null && BotOwner_0.EnemiesController.EnemyInfos.TryGetValue(gamePerson, out var value))
		{
			if (value != null)
			{
				BotOwner_0.EnemiesController.Remove(gamePerson);
			}
			if (GoalEnemy != null && GoalEnemy.Person == gamePerson)
			{
				GoalEnemy = null;
			}
			if (LastEnemy != null && LastEnemy.Person == gamePerson)
			{
				LastEnemy = null;
			}
			Player everExistedPlayerByID = Singleton<GameWorld>.Instance.GetEverExistedPlayerByID(gamePerson.ProfileId);
			BotOwner_0.Boss.DeletePlayer(everExistedPlayerByID);
			BotOwner_0.BotRequestController.CurRequest?.PlayerDestroy(everExistedPlayerByID);
			BotOwner_0.PeacefulActions.RemovePlayer(gamePerson);
			if (everExistedPlayerByID != null)
			{
				_ = everExistedPlayerByID.Profile.Nickname;
			}
			if (BotOwner_0 != null)
			{
				BotOwner_0.Id.ToString();
			}
		}
	}

	public void AddEnemy([NotNull] IPlayer enemy, BotSettingsClass groupInfo, bool onActivation)
	{
		if (enemy.Id == BotOwner_0.GetPlayer.Id)
		{
			return;
		}
		for (int i = 0; i < BotOwner_0.BotsGroup.MembersCount; i++)
		{
			if (BotOwner_0.BotsGroup.Member(i).GetPlayer.Id == enemy.Id)
			{
				return;
			}
		}
		if (!BotOwner_0.EnemiesController.EnemyInfos.ContainsKey(enemy) && enemy.Transform != null && enemy.HealthController.IsAlive)
		{
			EnemyInfo enemyInfo = BotOwner_0.EnemiesController.AddNew(BotsGroup_0, enemy, groupInfo);
			BotOwner_0.EnemiesController.SetInfo(enemy, enemyInfo);
			BotOwner_0.BotRequestController.RemoveAllRequestByRequester(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(enemy.ProfileId));
			enemy.HealthController.DiedEvent += delegate
			{
				DeleteInfoAboutEnemy(enemy);
			};
			float sqrMagnitude = (BotOwner_0.Position - enemy.Position).sqrMagnitude;
			if (!onActivation && sqrMagnitude < 625f && !BotOwner_0.Memory.HaveEnemy && GClass369.CanShoot(BotOwner_0, enemyInfo))
			{
				enemyInfo.SetVisible(value: true);
				BotOwner_0.Memory.GoalEnemy = enemyInfo;
			}
			action_0?.Invoke(enemy);
		}
	}

	public string MessageInfo(bool withN = true)
	{
		string text = "";
		if (GoalTarget.HavePlaceTarget() && GoalEnemy == null)
		{
			text = "T: " + ((double)Time.time - GoalTarget.CreatedTime).ToString("0.00") + "  " + GoalTarget.Type;
		}
		if (GoalEnemy != null)
		{
			text = "E: " + GoalEnemy.CanShoot + " Visible:" + GoalEnemy.IsVisible;
		}
		return text + "\n ComeTo:" + IsInCover;
	}

	public void SetLastTimeSeeEnemy()
	{
		LastEnemyTimeSeen = Time.time;
	}

	public void RemovePlaceForCheck(List<PlaceForCheck> list)
	{
		if (list.Contains(CurrentPlaceForCheck))
		{
			CurrentPlaceForCheck = null;
		}
	}

	public void Activate()
	{
		ActivatedPos = BotOwner_0.Transform.position;
		BotOwner_0.Memory.CheckIsPeace();
		ShallRunIfNoAmmo = GClass856.IsTrue100(BotOwner_0.Settings.FileSettings.Move.CHANCE_TO_RUN_IF_NO_AMMO_0_100);
		BotsGroup_0.AddMember(BotOwner_0, onActivation: true);
		BotOwner_0.GetPlayer.HealthController.EffectStartedEvent += method_1;
	}

	public bool ComeToPoint()
	{
		IsInCover = true;
		return true;
	}

	public void GetHit(DamageInfoStruct damageInfo)
	{
		if (BotOwner_0.Memory.GoalEnemy != null)
		{
			method_8(damageInfo);
			return;
		}
		float num = ((!BotOwner_0.Memory.GoalTarget.HavePlaceTarget()) ? BotOwner_0.Settings.FileSettings.Mind.HIT_DELAY_WHEN_PEACE : BotOwner_0.Settings.FileSettings.Mind.HIT_DELAY_WHEN_HAVE_SMT);
		if (num > 0f)
		{
			StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(num)).OnTimer += delegate
			{
				if (!BotOwner_0.IsDead)
				{
					method_8(damageInfo);
				}
			};
		}
		else
		{
			method_8(damageInfo);
		}
	}

	public void SetPanicPoint(PlaceForCheck placeForCheck, bool realDanger)
	{
		if (BotOwner_0.Boss.IamBoss || GoalEnemy != null)
		{
			return;
		}
		float magnitude = (BotOwner_0.Transform.position - placeForCheck.BasePoint).magnitude;
		if (magnitude < BotOwner_0.Settings.FileSettings.Hearing.BOT_CLOSE_PANIC_DIST)
		{
			if (!Physics.Raycast(new Ray(BotOwner_0.MyHead.position, placeForCheck.BasePoint), magnitude, LayerMaskClass.HighPolyWithTerrainMask))
			{
				DangerData.SetTarget(placeForCheck, BotOwner_0);
			}
			return;
		}
		float num = Time.time - LastEnemyTimeSeen;
		if ((LastEnemyTimeSeen <= 60f || num > 60f) && Time.time - LastEnemyTimeSeen > BotOwner_0.Settings.FileSettings.Mind.TIME_TO_RUN_TO_COVER_CAUSE_SHOOT_SEC)
		{
			BotOwner_0.DangerPointsData.AddPointOfDanger(placeForCheck, realDanger);
		}
	}

	public void TryGetInCoverSay()
	{
		if (Float_3 < Time.time)
		{
			Float_3 = Time.time + 20f;
			BotOwner_0.BotTalk.TrySay(EPhraseTrigger.GetInCover);
		}
	}

	public void BodyFound(GClass386 body)
	{
		BotOwner_0.BotTalk.TrySay(EPhraseTrigger.LootBody, withGroupDelay: true);
		if (body.IsEnemiesBody(BotOwner_0.Side))
		{
			BotOwner_0.DeadBodyData.SetBody(body);
			return;
		}
		ETagStatus value = (ETagStatus)0;
		if (body.Player != null && body.Player.AIData.IsAI)
		{
			value = BotSettingsRepoClass.GetPhraseTagFromRole(body.Player.AIData.BotOwner.Profile.Info.Settings.Role);
		}
		BotOwner_0.BotTalk.TrySay(EPhraseTrigger.OnFriendlyDown, value, withGroupDelay: true);
	}

	public void CheckIsPeace()
	{
		bool flag = !BotOwner_0.BewareGrenade.ShallRunAway() && !DangerData.HaveCloseDanger && BotOwner_0.ArtilleryDangerPlace.ShallEnd() && GoalEnemy == null && !BotOwner_0.DangerPointsData.HaveDangePoints && !GoalTarget.HaveMainTarget() && !BotOwner_0.DeadBodyData.HaveBodyToCheck() && !BotOwner_0.BotRequestController.HaveActivatedRequests();
		if (IsPeace != flag)
		{
			if (flag)
			{
				BotOwner_0.Settings.Current.Dismiss(BotLastBlindEffectModifierClass);
				BotLastBlindEffectModifierClass = new BotLastBlindEffectModifierClass(1f, 1f, 1f, BotOwner_0.Settings.FileSettings.Patrol.VISION_DIST_COEF_PEACE, 1f, 1f, 1f, 1f, 1f);
				BotOwner_0.Settings.Current.Apply(BotLastBlindEffectModifierClass);
			}
			else
			{
				BotOwner_0.Settings.Current.Dismiss(BotLastBlindEffectModifierClass);
			}
		}
		method_3(flag);
	}

	public void SetUnderFire(IPlayer source)
	{
		BotOwner_0.BotTalk.Say(EPhraseTrigger.UnderFire);
		float uNDER_FIRE_PERIOD = BotOwner_0.Settings.FileSettings.Mind.UNDER_FIRE_PERIOD;
		action_5?.Invoke(BotOwner_0, source);
		UnderFireTime = Time.time;
		Float_4 = UnderFireTime + uNDER_FIRE_PERIOD;
		if (BotOwner_0.Settings.FileSettings.Mind.AMBUSH_WHEN_UNDER_FIRE && Float_5 < Time.time)
		{
			Float_5 = Time.time + BotOwner_0.Settings.FileSettings.Mind.AMBUSH_WHEN_UNDER_FIRE_TIME_RESIST;
			BotOwner_0.Tactic.SetTactic(BotsGroup.BotCurrentTactic.Ambush, shallAutoReturnToAttack: true, uNDER_FIRE_PERIOD);
		}
	}

	public void UseDogFightOut()
	{
		BotCurrentCoverInfo.method_0();
	}

	public void method_3(bool toChange)
	{
		if (!Bool_1 && IsPeace != toChange)
		{
			IsPeace = toChange;
			action_3?.Invoke(IsPeace);
			if (IsPeace)
			{
				BotOwner_0.PatrollingData.RefreshStatus();
				BotOwner_0.BotTalk.TrySay(EPhraseTrigger.Clear, withGroupDelay: true);
				BotOwner_0.BotLight.TurnOn();
			}
		}
	}

	public void method_4(DamageInfoStruct damageInfo, EBodyPart eBodyPart, float val)
	{
		if (damageInfo.Player != null && damageInfo.Player.iPlayer.Id == BotOwner_0.Id && damageInfo.DamageType == EDamageType.Melee)
		{
			BotOwner_0.WeaponManager.Melee.HitCurrentEnemy(damageInfo, val, Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(GoalEnemy.Person.ProfileId));
		}
	}

	public void method_5()
	{
		if (CurrentPlaceForCheck != null && (CurrentPlaceForCheck.Position - BotOwner_0.Transform.position).magnitude < BotOwner_0.Settings.FileSettings.Move.REACH_DIST)
		{
			BotsGroup_0.PlaceChecked(CurrentPlaceForCheck);
		}
	}

	public void method_6()
	{
		if (LastEnemy != null)
		{
			if (GClass856.IsTrue100(BotOwner_0.Settings.FileSettings.Mind.ATTACK_IMMEDIATLY_CHANCE_0_100))
			{
				AttackImmediately = true;
				return;
			}
			float powerOfEquipment = LastEnemy.Person.AIData.PowerOfEquipment;
			if (BotOwner_0.AIData.PowerOfEquipment * BotOwner_0.Tactic.AggressionCoef > powerOfEquipment)
			{
				AttackImmediately = true;
			}
			else
			{
				AttackImmediately = false;
			}
		}
		else
		{
			AttackImmediately = true;
		}
	}

	public void method_7()
	{
		if (IsInCover)
		{
			if (CurCustomCoverPoint == null)
			{
				IsInCover = false;
			}
			else if ((CurCustomCoverPoint.Position - BotOwner_0.Transform.position).sqrMagnitude > BotOwner_0.Settings.FileSettings.Cover.MAX_DIST_OF_COVER_SQR)
			{
				IsInCover = false;
			}
		}
	}

	public void method_8(DamageInfoStruct damageInfo)
	{
		IPlayerOwner player = damageInfo.Player;
		SetUnderFire(player?.iPlayer);
		bool flag = true;
		bool flag2 = false;
		if (player != null && player.IsAI)
		{
			flag2 = player.IsAI;
			if (!BotOwner_0.EnemiesController.EnemyInfos.ContainsKey(player.iPlayer))
			{
				flag = false;
			}
		}
		Vector3 vector = player?.iPlayer.Position ?? damageInfo.MasterOrigin;
		Vector3 lhs = BotOwner_0.Transform.position - vector;
		float magnitude = lhs.magnitude;
		if (flag)
		{
			BotOwner_0.Memory.BotCurrentCoverInfo.HitInCoverByEnemy(vector);
		}
		else
		{
			BotOwner_0.Memory.BotCurrentCoverInfo.HitInCoverByUnknown(vector);
		}
		bool flag3 = damageInfo.Damage > BotOwner_0.Settings.FileSettings.Aiming.MIN_DAMAGE_TO_GET_HIT_AFFETS;
		if (BotOwner_0.Settings.FileSettings.Mind.IGNORE_ANOTHER_BOTS_BEING_HIT && player != null && player.IsAI)
		{
			return;
		}
		if (flag3)
		{
			LastTimeHit = Time.time;
			LastHitPos = damageInfo.Player.iPlayer.Position;
			BotOwner_0.Medecine.GetDamaged();
			BotOwner_0.AimingManager.CurrentAiming.GetHit(damageInfo);
			if (BotOwner_0.BotLay.IsLay)
			{
				BotOwner_0.BotLay.Damaged();
			}
			if (Vector3.Dot(lhs, BotOwner_0.LookDirection) > 0f)
			{
				BotOwner_0.BotTalk.TrySay(EPhraseTrigger.OnSix);
			}
			ValueStruct bodyPartHealth = BotOwner_0.GetPlayer.HealthController.GetBodyPartHealth(EBodyPart.Common);
			if (bodyPartHealth.Current / bodyPartHealth.Maximum < 0.2f)
			{
				BotOwner_0.BotTalk.TrySay(EPhraseTrigger.HurtNearDeath);
			}
			BotOwner_0.BotTalk.TrySay(EPhraseTrigger.OnBeingHurt);
		}
		if ((!IsDamaged || BotOwner_0.Memory.GoalEnemy == null) && flag3)
		{
			Float_1 = Time.time + BotOwner_0.Settings.FileSettings.Aiming.DAMAGE_PANIC_TIME;
			if (damageInfo.Damage > BotOwner_0.Settings.FileSettings.Mind.MIN_DAMAGE_SCARE)
			{
				PlaceForCheck placeForCheck = new PlaceForCheck(vector, PlaceForCheckType.danger);
				BotOwner_0.DangerPointsData.AddPointOfDanger(placeForCheck, realDanger: true);
				Vector3 suspectedPoint = (vector + BotOwner_0.Position) / 2f;
				BotOwner_0.BotsGroup.AddPointToSearch(suspectedPoint, 160f, BotOwner_0, baseReacheble: false);
			}
		}
		if (player != null)
		{
			if (BotOwner_0.Settings.FileSettings.Mind.REVENGE_TO_GROUP && !string.IsNullOrEmpty(player.iPlayer.GroupId))
			{
				foreach (IPlayer item in BotsGroup_0.Neutrals.Keys.ToList())
				{
					if (item.GroupId == player.iPlayer.GroupId)
					{
						BotsGroup_0.CheckAndAddEnemy(item, ignoreAI: true);
					}
				}
				int num = 0;
				for (int i = 0; i < BotsGroup_0.Allies.Count; i++)
				{
					IPlayer player2 = BotsGroup_0.Allies[i];
					if (player2.GroupId == player.iPlayer.GroupId)
					{
						if (BotsGroup_0.CheckAndAddEnemy(player2, ignoreAI: true))
						{
							i--;
							num++;
						}
						if (num > 1000)
						{
							break;
						}
					}
				}
			}
			else
			{
				BotsGroup_0.CheckAndAddEnemy(player.iPlayer, ignoreAI: true);
			}
			Player alivePlayerByProfileID = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.iPlayer.ProfileId);
			if ((BotOwner_0.IsAI && BotOwner_0.AIData.IsBossOrFollowerRequireRevenge() && !flag2 && (alivePlayerByProfileID == null || !alivePlayerByProfileID.Loyalty.CanBeFreeKilled)) || (!BotOwner_0.IsAI && !flag2 && (alivePlayerByProfileID == null || !alivePlayerByProfileID.Loyalty.CanBeFreeKilled)))
			{
				BotOwner_0.BotsController.AddEnemyToAllGroupsInBotZone(player.iPlayer, BotOwner_0, BotOwner_0);
			}
			if (!(damageInfo.Weapon is ThrowWeapItemClass) && BotOwner_0.BotsGroup.IsEnemy(player.iPlayer))
			{
				LastDamageData = new GClass579(vector);
			}
			bool flag4;
			if ((flag4 = GoalEnemy != null && GoalEnemy.IsVisible) && Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(GoalEnemy.Person.ProfileId) == player)
			{
				return;
			}
			if (flag4 && magnitude < BotOwner_0.Settings.FileSettings.Mind.DIST_TO_ENEMY_SPOTTED_ON_HIT)
			{
				BotOwner_0.BotsGroup.ReportAboutEnemy(player.iPlayer, EEnemyPartVisibleType.Visible, BotOwner_0);
			}
		}
		float num2 = magnitude / BotOwner_0.Settings.FileSettings.Mind.HIT_POINT_DETECTION;
		float num3 = GClass856.Random(0f - num2, num2);
		float num4 = GClass856.Random(0f - num2, num2);
		vector.x += num3;
		vector.z += num4;
		PlaceForCheck placeForCheck2 = BotsGroup_0.AddPointToSearch(vector, 5550f, BotOwner_0);
		if (placeForCheck2 != null)
		{
			SetPanicPoint(placeForCheck2, realDanger: true);
			BotOwner_0.CalcGoal();
		}
	}

	public void Dispose()
	{
		action_5 = null;
		action_2 = null;
		SetCoverPoints(null);
	}
}
