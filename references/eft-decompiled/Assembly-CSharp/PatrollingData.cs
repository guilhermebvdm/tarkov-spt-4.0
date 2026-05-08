using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class PatrollingData : GClass429
{
	public GClass504 PointControl;

	public GClass509 PatrolPathControl;

	public GClass515 MoveByReservWay;

	public BotOwner _lastFriend;

	[NonSerialized]
	public float NextFriendSearch;

	[NonSerialized]
	public float NextTalk;

	[NonSerialized]
	public float NextTimeCanRecalc;

	[NonSerialized]
	public PatrolStatus Status_1;

	[NonSerialized]
	public PatrolStatus PrevStatus;

	[NonSerialized]
	public float ComeTime;

	[NonSerialized]
	public float ReservChoosedTime;

	[NonSerialized]
	public bool NeedUpdateNextPoint;

	[NonSerialized]
	public PatrolLootPointsData LootData_1;

	[NonSerialized]
	public PatrolExfiltrationPointsData ExfiltrationData_1;

	[NonSerialized]
	public GInterface10 PatrolMove;

	[NonSerialized]
	public IProductionUpdate PatrolLook;

	[NonSerialized]
	public BotObserveDataClass BotObserveData;

	[NonSerialized]
	public List<GClass585> SuspetionPlaces = new List<GClass585>(30);

	[NonSerialized]
	public float RecheckPeriod;

	public PatrolLootPointsData LootData => LootData_1;

	public PatrolExfiltrationPointsData ExfiltrationData => ExfiltrationData_1;

	public PatrolStatus Status
	{
		get
		{
			return Status_1;
		}
		set
		{
			if (value == PatrolStatus.pause)
			{
				if (Status_1 == PatrolStatus.pause)
				{
					return;
				}
				BotOwner_0.StopMove();
				PrevStatus = Status_1;
			}
			if (value == PatrolStatus.stay)
			{
				if (Status_1 != PatrolStatus.stay)
				{
					ComeTime = Time.time;
				}
				PatrolLook.SetComeToPoint();
			}
			this.OnStatusChanged?.Invoke(value);
			Status_1 = value;
		}
	}

	[field: NonSerialized]
	public float SpeedPatrol { get; }

	[field: NonSerialized]
	public bool IsActivated { get; set; }

	public PatrolPointContainer CurPatrolPoint => PointControl.PatrolPoint;

	public PatrolWay Way => PointControl.Way;

	[field: NonSerialized]
	public Vector3 CurTargetPoint { get; set; }

	[field: NonSerialized]
	public PatrolPointChooserBasic PointChooser { get; set; }

	[field: NonSerialized]
	public float ComeToPointTime { get; set; }

	public event Action<PatrolPointContainer> OnPatrolPointCome;

	public event Action<PatrolStatus> OnStatusChanged;

	public static PatrolPointChooserBasic GetPointChooser(BotOwner bot, PatrolMode mode, IGetProfileData data = null)
	{
		if (data != null && data.ShallChooseByData())
		{
			return new GClass560(bot, data);
		}
		switch (mode)
		{
		case PatrolMode.groupMoving:
			return new GClass562(bot);
		default:
			return new PatrolPointChooserBasic(bot);
		case PatrolMode.bossRoundProtect:
		case PatrolMode.bossCoverScouts:
			return new GClass557(bot);
		case PatrolMode.follower:
			return new GClass561(bot);
		}
	}

	public PatrollingData(BotOwner owner)
		: base(owner)
	{
		MoveByReservWay = new GClass515(owner);
		SpeedPatrol = GClass856.Random(0.6f, 0.7f);
		BotOwner_0 = owner;
		PatrolPathControl = new GClass509(owner);
		PointControl = new GClass504(owner);
		PointControl.SetPointSetter(PointSetted);
		PointControl.SetWayChanged(method_2);
		LootData_1 = new PatrolLootPointsData(owner);
		ExfiltrationData_1 = new PatrolExfiltrationPointsData(owner);
		BotObserveData = BotOwner_0.Memory.botObserveData;
	}

	public void Activate()
	{
		if (!IsActivated)
		{
			PatrolPointChooserBasic pointChooser = GetPointChooser(BotOwner_0, PatrolMode.simple, BotOwner_0.SpawnProfileData);
			SetMode(PatrolMode.simple, pointChooser);
		}
		BotOwner_0.Memory.OnPeaceChange += method_1;
		PatrolPathControl.Activate();
	}

	public void Unpause()
	{
		if (Status == PatrolStatus.pause)
		{
			Status = PrevStatus;
			if (Status == PatrolStatus.go)
			{
				BotOwner_0.GoToPoint(PointControl.PatrolPoint.Position);
			}
		}
	}

	public void SetMode(PatrolMode mode, PatrolPointChooserBasic pointChooser)
	{
		IsActivated = true;
		PointChooser = pointChooser;
		switch (mode)
		{
		default:
			Debug.LogError("wrong set patrol mode " + mode);
			break;
		case PatrolMode.bossRoundProtect:
			PatrolMove = new GClass514(BotOwner_0, this, PatrolFollowerType.CloseCoverWide);
			PatrolLook = new GClass507(BotOwner_0, this, BotObserveData);
			break;
		case PatrolMode.bossRoundProtectWithStay:
			PatrolMove = new GClass514(BotOwner_0, this, PatrolFollowerType.CloseCoverWithStop);
			PatrolLook = new GClass507(BotOwner_0, this, BotObserveData);
			break;
		case PatrolMode.bossCoverScouts:
			PatrolMove = new GClass511(BotOwner_0, this);
			PatrolLook = new GClass507(BotOwner_0, this, BotObserveData);
			break;
		case PatrolMode.follower:
			PatrolMove = new GClass510(BotOwner_0, this);
			PatrolLook = new GClass507(BotOwner_0, this, BotObserveData);
			break;
		case PatrolMode.simple:
			PatrolMove = new GClass510(BotOwner_0, this);
			PatrolLook = new GClass507(BotOwner_0, this, BotObserveData);
			break;
		case PatrolMode.bossStayAtPlaces:
			PatrolLook = new GClass507(BotOwner_0, this, BotObserveData);
			if (!(BotOwner_0.Boss.BossLogic is IBossLogic bossLogic))
			{
				Debug.LogError("wrong logic of boss IPatrolMoveBossStayAtPlace");
				PatrolMove = new GClass510(BotOwner_0, this);
				return;
			}
			PatrolMove = new GClass513(BotOwner_0, this, bossLogic);
			break;
		case PatrolMode.byNameAndStay:
			PatrolMove = new GClass512(BotOwner_0, this);
			PatrolLook = new GClass508(BotOwner_0, this, BotObserveData);
			break;
		case PatrolMode.bossRoundProtectAndStay:
			PatrolMove = new GClass514(BotOwner_0, this, PatrolFollowerType.CloseCover);
			PatrolLook = new GClass506(BotOwner_0, this, BotObserveData);
			break;
		case PatrolMode.groupMoving:
			PatrolMove = new GClass510(BotOwner_0, this);
			PatrolLook = new GClass507(BotOwner_0, this, BotObserveData);
			break;
		}
		PointChooser.ChooseStartWay();
	}

	public void ManualUpdate(bool canLookAround)
	{
		if (Status == PatrolStatus.stay)
		{
			method_3();
			if (CurPatrolPoint != null && CurPatrolPoint.TargetPoint.PatrolWay != null)
			{
				switch (CurPatrolPoint.TargetPoint.PatrolWay.PatrolType)
				{
				case PatrolType.patrolling:
					PatrolLook.Update();
					TryToTalk(bigDelay: true);
					break;
				case PatrolType.reserved:
					method_5();
					TryToTalk(bigDelay: false);
					ReserveGoNextPoint();
					break;
				case PatrolType.boss:
					PatrolLook.Update();
					method_4();
					break;
				}
			}
		}
		else
		{
			PatrolPathControl.ManualUpdate();
			UpdateObserveWay();
			TryToTalk(bigDelay: true);
		}
	}

	public void ReserveGoNextPoint()
	{
		TryToTalk(bigDelay: true);
		if (Status_1 == PatrolStatus.stay && Time.time - ComeTime > BotOwner_0.Settings.FileSettings.Patrol.GO_TO_NEXT_POINT_DELTA_RESERV_WAY && Time.time - ReservChoosedTime > BotOwner_0.Settings.FileSettings.Patrol.GO_TO_NEXT_POINT_DELTA_RESERV_WAY)
		{
			PointChooser.FindNextPoint(withSetting: true, withoutNext: false, -1, canCut: false);
			ReservChoosedTime = Time.time;
		}
	}

	public void MoveUpdate()
	{
		PatrolMove?.Update();
	}

	public void StopAndLookToPoint(Vector3 sPoint)
	{
		Vector3 vectorToLook = sPoint - BotOwner_0.Transform.position;
		BotObserveData.SetVectorToLook(vectorToLook);
	}

	public void UpdateObserveWay()
	{
		BotObserveData.Update();
	}

	public void ComeToPoint()
	{
		if (Status == PatrolStatus.go)
		{
			ComeToPointTime = Time.time;
		}
		if (CurPatrolPoint != null && !(CurPatrolPoint.TargetPoint == null))
		{
			this.OnPatrolPointCome?.Invoke(CurPatrolPoint);
			PatrolPathControl.ComeToPoint(CurPatrolPoint);
			if (PointControl.Way.PatrolType == PatrolType.reserved)
			{
				if (CurPatrolPoint.TargetPoint.ActionData != null)
				{
					CurPatrolPoint.TargetPoint.ActionData.ComeTo(BotOwner_0);
				}
				Status = PatrolStatus.stay;
			}
			else
			{
				if (Status == PatrolStatus.stay)
				{
					return;
				}
				PatrolPointType patrolPointType = CurPatrolPoint.TargetPoint.PatrolPointType;
				if (patrolPointType != PatrolPointType.checkPoint && patrolPointType == PatrolPointType.stayPoint)
				{
					if (Status != PatrolStatus.go)
					{
						return;
					}
					if (NeedUpdateNextPoint)
					{
						PointChooser.NextPointDecision(delegate
						{
							Status = PatrolStatus.stay;
							PatrolLook.SetDefault(max: false);
						});
						NeedUpdateNextPoint = false;
					}
					else
					{
						Status = PatrolStatus.stay;
						PatrolLook.SetDefault(max: true);
					}
				}
				else
				{
					PointChooser.NextPointDecision(delegate
					{
						Status = PatrolStatus.stay;
						PatrolLook.SetDefault(max: false);
					});
				}
			}
		}
		else
		{
			PointChooser.NextPointDecision(delegate
			{
				Status = PatrolStatus.stay;
				PatrolLook.SetDefault(max: false);
			});
		}
	}

	public void GoToPoint()
	{
		if (Status != PatrolStatus.pause && !BotRun.CalcPathToMoveZigZagAndGo(CurTargetPoint, BotOwner_0, EZigZAgType.percentOffset))
		{
			BotOwner_0.GoToPoint(CurTargetPoint);
		}
	}

	public void Disable()
	{
		PointChooser?.Disable();
	}

	public void FindNextPoint(bool withSetting, bool withoutNext, GDelegate4 pointFilter = null)
	{
		PointChooser.FindNextPoint(withSetting, withoutNext, -1, canCut: true, pointFilter);
	}

	public void Pause()
	{
		Status = PatrolStatus.pause;
	}

	public void DrawGizomsSelectedFromBot()
	{
		PatrolMove.DrawGizmosSelected();
		if (PointControl.PatrolPoint != null)
		{
			Gizmos.color = Color.yellow;
			Vector3 position = PointControl.PatrolPoint.Position;
			Gizmos.DrawWireCube(position, Vector3.one * 0.3f);
			Gizmos.DrawWireCube(position + Vector3.up * 0.5f, Vector3.one * 0.3f);
		}
		switch (Status_1)
		{
		case PatrolStatus.go:
			Gizmos.color = Color.green;
			break;
		case PatrolStatus.pause:
			Gizmos.color = Color.gray;
			break;
		case PatrolStatus.stay:
			Gizmos.color = Color.blue;
			break;
		}
		Gizmos.DrawWireCube(BotOwner_0.Position + Vector3.up * 2.4f, Vector3.one * 0.2f);
		foreach (GClass585 suspetionPlace in SuspetionPlaces)
		{
			if (suspetionPlace.IsAlive())
			{
				Gizmos.color = Color.white;
				Gizmos.DrawSphere(suspetionPlace.Point, 0.1f);
			}
		}
	}

	public void SetTargetMoveSpeed()
	{
		BotOwner_0.SetTargetMoveSpeed(SpeedPatrol);
	}

	public void RefreshStatus()
	{
		Status = PatrolStatus.go;
	}

	public void PointSetted(PatrolPointContainer point, int index)
	{
		if (Status == PatrolStatus.pause)
		{
			return;
		}
		if (PointChooser == null)
		{
			PointChooser = GetPointChooser(BotOwner_0, PatrolMode.simple);
		}
		Vector3 direction = PointControl.PatrolPoint.Position - BotOwner_0.Transform.position;
		BotObserveData.SetVector(direction);
		CurTargetPoint = PointControl.PatrolPoint.Position;
		NavMeshPathStatus navMeshPathStatus = PatrolPathControl.GoToPoint(PointControl.PatrolPoint);
		if (PointChooser.WasCutted)
		{
			if (navMeshPathStatus == NavMeshPathStatus.PathComplete)
			{
				CurTargetPoint = method_0();
				BotOwner_0.GoToPoint(CurTargetPoint);
				Status = PatrolStatus.go;
			}
		}
		else
		{
			Status = PatrolStatus.go;
		}
		PatrolLook?.SetDefault(max: false);
	}

	public Vector3 method_0()
	{
		return BotOwner_0.Mover.LastTargetPoint(PointChooser.PathDistCoef);
	}

	public void method_1(bool obj)
	{
		if (obj)
		{
			Status = PatrolStatus.go;
		}
	}

	public void method_2()
	{
		_lastFriend = null;
	}

	public void method_3()
	{
		if (!(RecheckPeriod > Time.time))
		{
			RecheckPeriod = Time.time + 5f;
			if (!GClass510.IsCome(BotOwner_0, BotOwner_0.PatrollingData.CurPatrolPoint.Position, extraTarget: false, out var _))
			{
				Status = PatrolStatus.go;
				NeedUpdateNextPoint = true;
			}
		}
	}

	public void method_4()
	{
		if (Status_1 == PatrolStatus.stay && Time.time - ComeTime > BotOwner_0.Settings.FileSettings.Patrol.GO_TO_NEXT_POINT_DELTA)
		{
			PointChooser.FindNextPoint(withSetting: true, withoutNext: false, -1, canCut: false);
		}
	}

	public void TryToTalk(bool bigDelay)
	{
		if (NextTalk < Time.time)
		{
			float value = ((!bigDelay) ? ((_lastFriend == null) ? BotOwner_0.Settings.FileSettings.Patrol.TALK_DELAY_BIG : BotOwner_0.Settings.FileSettings.Patrol.TALK_DELAY) : BotOwner_0.Settings.FileSettings.Patrol.TALK_DELAY_BIG);
			NextTalk = Time.time + GClass856.GreateRandom(Mathf.Clamp(value, BotOwner_0.Settings.FileSettings.Patrol.MIN_TALK_DELAY, 100f));
			BotOwner_0.BotTalk.TrySay(EPhraseTrigger.OnMutter, withGroupDelay: true);
		}
	}

	public void method_5()
	{
		if (NextFriendSearch < Time.time)
		{
			float sqrDist;
			BotOwner closestFriend = BotOwner_0.Covers.GetClosestFriend(out sqrDist);
			if (closestFriend == null)
			{
				_lastFriend = null;
			}
			else if (sqrDist < BotOwner_0.Settings.FileSettings.Patrol.MIN_DIST_TO_CLOSE_TALK_SQR)
			{
				_lastFriend = closestFriend;
			}
			else
			{
				_lastFriend = null;
			}
			NextFriendSearch = Time.time + BotOwner_0.Settings.FileSettings.Patrol.FRIEND_SEARCH_SEC;
		}
		if (_lastFriend != null)
		{
			Vector3 dir = _lastFriend.Transform.position - BotOwner_0.Transform.position;
			BotOwner_0.Steering.LookToDirection(dir, 90f);
		}
	}

	public void Dispose()
	{
		if (BotOwner_0.Memory != null)
		{
			BotOwner_0.Memory.OnPeaceChange -= method_1;
		}
		PatrolPathControl?.Dispose();
		PointControl?.Dispose();
		PatrolMove?.Dispose();
		PointChooser?.Dispose();
		this.OnStatusChanged = null;
	}

	[CompilerGenerated]
	public void method_6()
	{
		Status = PatrolStatus.stay;
		PatrolLook.SetDefault(max: false);
	}

	[CompilerGenerated]
	public void method_7()
	{
		Status = PatrolStatus.stay;
		PatrolLook.SetDefault(max: false);
	}

	[CompilerGenerated]
	public void method_8()
	{
		Status = PatrolStatus.stay;
		PatrolLook.SetDefault(max: false);
	}
}
