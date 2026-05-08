using System;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using EFT.Interactive;
using UnityEngine;

public class BotStationaryWeaponData : GClass429
{
	[CompilerGenerated]
	public class Class200
	{
		public BotStationaryWeaponData botStationaryWeaponData_0;

		public Player.FirearmController fireArms;

		public void method_0(Player.FirearmController controller)
		{
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			botStationaryWeaponData_0.method_4(controller);
			botStationaryWeaponData_0.IsInSpawnState = false;
			fireArms.OnReadyToOperate -= method_0;
		}
	}

	[NonSerialized]
	public const float MAX_STATIONARY_SDIST = 22500f;

	[NonSerialized]
	public const float AFTER_DROP_SEARCH_DELAY = 15f;

	[NonSerialized]
	public const float SIMPLE_SEARCH_DELAY = 3f;

	[NonSerialized]
	public AIStationaryController CurStationaryWeapons;

	[NonSerialized]
	public GClass641.IBotTimer StationaryBlockVisionTimer;

	[NonSerialized]
	public float LastSpottedTime = -9999f;

	[NonSerialized]
	public float NextPosibleSearchTime;

	[NonSerialized]
	public float NextPosibleStartUseTime;

	[NonSerialized]
	public float SitTime;

	[NonSerialized]
	public float ComeCloseTime;

	[NonSerialized]
	public float ShallEndShootFromCover;

	[NonSerialized]
	public float CosToLeave;

	[NonSerialized]
	public int SpottedTimes;

	[NonSerialized]
	public float DropTime;

	[field: NonSerialized]
	public bool WantToTake { get; set; }

	[field: NonSerialized]
	public bool Taken { get; set; }

	[field: NonSerialized]
	public bool CanLeave { get; set; }

	public bool HaveLink => CurLink != null;

	[field: NonSerialized]
	public StationaryWeaponLink CurLink { get; set; }

	[field: NonSerialized]
	public bool IsInSpawnState { get; set; }

	[field: NonSerialized]
	public float LastDistToWeapon { get; set; }

	public event Action<StationaryWeaponLink> OnLinkedWeaponChange;

	public BotStationaryWeaponData(BotOwner owner)
		: base(owner)
	{
	}

	public void StartMove()
	{
		if (BotOwner_0.Brain.LastDecision != BotLogicDecision.runToStationary && Time.time - SitTime > 1f)
		{
			DropCurWeapon(withDelay: false);
		}
	}

	public StationaryWeaponLink CheckWantTakeStationary(float maxRadius, bool igoneEnemy = false, float delatAfterDeathPeriod = -1f, bool needStraightShoot = false)
	{
		if (!(CurStationaryWeapons == null) && CurStationaryWeapons.Weapons != null)
		{
			if (!BotOwner_0.Settings.FileSettings.Cover.STATIONARY_CAN_USE)
			{
				return null;
			}
			if (NextPosibleStartUseTime > Time.time)
			{
				return null;
			}
			if (CurLink != null && Taken && CurLink.IsFree(BotOwner_0.Id))
			{
				return CurLink;
			}
			if (CurLink != null)
			{
				bool flag;
				if ((flag = IsEnemyAtSector(CurLink) && CurLink.HaveAmmo() && CurLink.Weapon.IsAvailable(BotOwner_0.Profile.Id)) && delatAfterDeathPeriod > 0f && !CurLink.CanTakeByPrevDeath(delatAfterDeathPeriod))
				{
					flag = false;
				}
				if (flag && needStraightShoot && method_1())
				{
					flag = false;
				}
				if (flag)
				{
					return CurLink;
				}
			}
			if (NextPosibleSearchTime > Time.time && Time.time - DropTime > 0.5f)
			{
				return null;
			}
			if (method_5())
			{
				return null;
			}
			if (igoneEnemy)
			{
				StationaryWeaponLink stationaryWeaponLink = CurStationaryWeapons.Weapons.FirstOrDefault();
				if (stationaryWeaponLink != null)
				{
					return stationaryWeaponLink;
				}
			}
			if (!BotOwner_0.CanSprintPlayer)
			{
				return null;
			}
			NextPosibleSearchTime = Time.time + 3f;
			int num = 0;
			StationaryWeaponLink stationaryWeaponLink2;
			while (true)
			{
				if (num < CurStationaryWeapons.Weapons.Length)
				{
					stationaryWeaponLink2 = CurStationaryWeapons.Weapons[num];
					if (!(stationaryWeaponLink2.Weapon == null) && !((stationaryWeaponLink2.Weapon.OperatorPosition - BotOwner_0.Position).magnitude > maxRadius) && !(stationaryWeaponLink2.CorePointInGame == null) && stationaryWeaponLink2.CorePointInGame.ConnectionGroupId == BotOwner_0.StartCorePoint.ConnectionGroupId)
					{
						EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
						if (goalEnemy != null)
						{
							Vector3 rhs = BotOwner_0.Position - goalEnemy.CurrPosition;
							Vector3 lhs = BotOwner_0.Position - goalEnemy.CurrPosition;
							float y = 0f;
							lhs.y = 0f;
							rhs.y = y;
							if (Vector3.Dot(lhs, rhs) < 0f || !method_0(rhs.magnitude, stationaryWeaponLink2))
							{
								goto IL_02c6;
							}
						}
						if (stationaryWeaponLink2.IsFree(BotOwner_0.Id) && stationaryWeaponLink2.HaveAmmo() && (!(delatAfterDeathPeriod > 0f) || CurLink.CanTakeByPrevDeath(delatAfterDeathPeriod)) && (!needStraightShoot || goalEnemy == null || !method_1()) && (goalEnemy == null || IsEnemyAtSector(stationaryWeaponLink2)))
						{
							break;
						}
					}
					goto IL_02c6;
				}
				return null;
				IL_02c6:
				num++;
			}
			SetTargetStationary(stationaryWeaponLink2);
			return stationaryWeaponLink2;
		}
		return null;
	}

	public bool method_0(float magnitude, StationaryWeaponLink stationary)
	{
		if (stationary.IsGrenade())
		{
			return magnitude > BotOwner_0.Settings.FileSettings.Shoot.STATIONARY_GRENADE_MIN_DIST_TO_TAKE;
		}
		return (double)magnitude > BotOwner_0.Settings.FileSettings.Shoot.STATIONARY_SIMPLE_MIN_DIST_TO_TAKE;
	}

	public bool method_1()
	{
		if (BotOwner_0.Memory.GoalEnemy != null && !GClass369.CanShoot(CurLink.Weapon.WeaponTransform.position, BotOwner_0.Memory.GoalEnemy))
		{
			return true;
		}
		return false;
	}

	public bool TryFindClosests(Vector3 pos, bool withSetTarget, out StationaryWeaponLink closest)
	{
		if (CurStationaryWeapons == null)
		{
			if (BotOwner_0 == null || BotOwner_0.BotsGroup == null || BotOwner_0.BotsGroup.BotGame.BotsController.StationaryWeapons == null)
			{
				closest = null;
				return false;
			}
			CurStationaryWeapons = BotOwner_0.BotsGroup.BotGame.BotsController.StationaryWeapons;
		}
		float num = float.MaxValue;
		int lastConnectionId = BotOwner_0.VoxelesPersonalData.GetLastConnectionId();
		closest = null;
		for (int i = 0; i < CurStationaryWeapons.Weapons.Length; i++)
		{
			StationaryWeaponLink stationaryWeaponLink = CurStationaryWeapons.Weapons[i];
			if (!(stationaryWeaponLink == null) && !(stationaryWeaponLink.Weapon == null) && stationaryWeaponLink.HaveAmmo() && stationaryWeaponLink.IsFree(BotOwner_0.Id) && stationaryWeaponLink.CorePointInGame.ConnectionGroupId == lastConnectionId)
			{
				float sqrMagnitude = (stationaryWeaponLink.Weapon.OperatorPosition - BotOwner_0.Position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					closest = stationaryWeaponLink;
				}
			}
		}
		if (closest != null && num < 22500f)
		{
			if (withSetTarget)
			{
				SetTargetStationary(closest);
			}
			return true;
		}
		return false;
	}

	public bool IsEnemyAtSector(StationaryWeaponLink stationary)
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return false;
		}
		float num = CurDistToLeave();
		if (num > 0f && goalEnemy.Distance < num)
		{
			return false;
		}
		return IsEnemyAtSector(stationary, goalEnemy.CurrPosition);
	}

	public bool IsEnemyAtSector(StationaryWeaponLink stationary, Vector3 pos)
	{
		Vector3 operatorPosition = stationary.Weapon.OperatorPosition;
		Vector3 v = pos - operatorPosition;
		v.y = 0f;
		return GClass855.IsAngLessNormalized(GClass855.NormalizeFastSelf(v), stationary.InitialDir, stationary.CosAngleBase);
	}

	public void SetTargetStationary(StationaryWeaponLink link)
	{
		method_2(link);
	}

	public void DropCurWeapon(bool withDelay, bool withLeavingAsPlayer = true)
	{
		if (BotOwner_0.GetPlayer.InventoryController.IsInventoryBlocked() || !Taken || !CanLeave)
		{
			return;
		}
		if (CurLink == null)
		{
			WantToTake = false;
			Taken = false;
			return;
		}
		SpottedTimes = 0;
		if (withDelay)
		{
			NextPosibleStartUseTime = Time.time + 15f;
		}
		if (CurLink.Weapon.IsOperator(BotOwner_0.Profile.Id) && withLeavingAsPlayer)
		{
			BotOwner_0.GetPlayer.OperateStationaryWeapon(CurLink.Weapon, StationaryPacketStruct.EStationaryCommand.Leave);
		}
		WantToTake = false;
		Taken = false;
		method_2(null);
		DropTime = Time.time;
		NextPosibleSearchTime = Time.time + 15f;
	}

	public void method_2(StationaryWeaponLink link)
	{
		if (!(CurLink == link))
		{
			if (link == null)
			{
				BotOwner_0.Mover.AllowTeleport();
			}
			if (CurLink != null)
			{
				CurLink.ClearOwner();
				CurLink = link;
			}
			else
			{
				WantToTake = true;
				CurLink = link;
				CurLink.SetOwner(BotOwner_0);
			}
			this.OnLinkedWeaponChange?.Invoke(CurLink);
		}
	}

	public void Activate()
	{
		CurStationaryWeapons = BotOwner_0.BotsGroup.BotGame.BotsController.StationaryWeapons;
		BotOwner_0.GetPlayer.MovementContext.OnHandsControllerChanged += method_3;
		CosToLeave = Mathf.Cos(BotOwner_0.Settings.FileSettings.Aiming.STATIONARY_LEAVE_HALF_DEGREE * (MathF.PI / 180f));
	}

	public void Take()
	{
		if (BotOwner_0.GetPlayer.HandsController.CanRemove())
		{
			if (BotOwner_0.BotLay.IsLay)
			{
				BotOwner_0.BotLay.GetUp(withCheck: false);
				return;
			}
			CanLeave = false;
			BotOwner_0.GetPlayer.OperateStationaryWeapon(CurLink.Weapon, StationaryPacketStruct.EStationaryCommand.Occupy);
			Taken = true;
			SitTime = Time.time;
		}
	}

	public BotLogicDecision? GetCurrentDecision()
	{
		if (IsClose())
		{
			if (CurLink == null)
			{
				return null;
			}
			if (!CurLink.HaveAmmo())
			{
				return null;
			}
			if (IsEnemyAtSector(CurLink) && CurLink.Weapon.IsAvailable(BotOwner_0.Profile.Id))
			{
				return BotLogicDecision.shootFromStationary;
			}
			return null;
		}
		return BotLogicDecision.runToStationary;
	}

	public bool IsClose()
	{
		if (CurLink == null)
		{
			return false;
		}
		if (CurLink.BadLoad)
		{
			return false;
		}
		Vector3 vector = CurLink.Weapon.OperatorPosition - BotOwner_0.Position;
		LastDistToWeapon = vector.magnitude;
		if (Mathf.Abs(vector.y) > BotOwner_0.Settings.FileSettings.Move.Y_APPROXIMATION)
		{
			return false;
		}
		if (LastDistToWeapon > 0.8f)
		{
			return false;
		}
		ComeCloseTime = Time.time;
		return true;
	}

	public void Spotted()
	{
		SpottedTimes++;
		LastSpottedTime = Time.time;
	}

	public bool CheckEndShootFromStationary()
	{
		if (CurLink == null)
		{
			return true;
		}
		if (!CurLink.HaveAmmo())
		{
			return true;
		}
		if (IsEnemyAtSector(CurLink))
		{
			return false;
		}
		if (method_5())
		{
			return true;
		}
		return false;
	}

	public bool CheckAmmonProcess()
	{
		if (!CurLink.HaveAmmo())
		{
			return false;
		}
		return true;
	}

	public bool CheckIsCloseToTarget()
	{
		return IsClose();
	}

	public float DeltaSitTime()
	{
		return Time.time - SitTime;
	}

	public bool CheckeEnemyOutOfSector(EnemyInfo enemy)
	{
		if (CurLink == null)
		{
			return true;
		}
		if (enemy == null)
		{
			return true;
		}
		if (!GClass855.IsAngLessNormalized(GClass855.NormalizeFastSelf(enemy.Direction), CurLink.InitialDir, CosToLeave))
		{
			return true;
		}
		return false;
	}

	public bool IsPointAtSector(Vector3 point)
	{
		return GClass855.IsAngLessNormalized(GClass855.NormalizeFastSelf(point - BotOwner_0.Position), CurLink.InitialDir, CosToLeave);
	}

	public bool LeaveCauseNoAmmo()
	{
		if (CurLink == null)
		{
			return true;
		}
		if (CurLink.HaveAmmo())
		{
			return false;
		}
		return true;
	}

	public string GetAmmoInfo()
	{
		if (CurLink == null)
		{
			return "Cur link is null";
		}
		return CurLink.AmmoInfo();
	}

	public bool ShallEndShootFromCurrent()
	{
		if (BotOwner_0.Settings.FileSettings.Cover.CAN_END_SHOOT_FROM_COVER_CAUSE_STATIONARY && ShallEndShootFromCover < Time.time)
		{
			ShallEndShootFromCover = Time.time + BotOwner_0.Settings.FileSettings.Cover.CAN_END_SHOOT_FROM_COVER_CAUSE_STATIONARY_DELTA;
			if (CheckWantTakeStationary(BotOwner_0.Settings.FileSettings.Cover.CAN_END_SHOOT_FROM_COVER_CAUSE_STATIONARY_RADIUS) != null)
			{
				BotOwner_0.Memory.Spotted(byHit: false);
				return true;
			}
		}
		return false;
	}

	public float CurDistToLeave()
	{
		if (CurLink == null)
		{
			return -1f;
		}
		if (CurLink.Weapon.Animation == StationaryWeapon.EStationaryAnimationType.AGS_17)
		{
			return BotOwner_0.Settings.FileSettings.Shoot.AGS_17_DIST_TO_LEAVE;
		}
		return -1f;
	}

	public void method_3(Player.AbstractHandsController arg1, Player.AbstractHandsController arg2)
	{
		Class200 CS_0024_003C_003E8__locals10 = new Class200();
		CS_0024_003C_003E8__locals10.botStationaryWeaponData_0 = this;
		if (!(arg2 != null))
		{
			return;
		}
		CS_0024_003C_003E8__locals10.fireArms = BotOwner_0.GetPlayer.HandsController as Player.FirearmController;
		if (!(CS_0024_003C_003E8__locals10.fireArms != null))
		{
			return;
		}
		if (CS_0024_003C_003E8__locals10.fireArms.IsInSpawnOperation())
		{
			IsInSpawnState = true;
			CS_0024_003C_003E8__locals10.fireArms.OnReadyToOperate += delegate(Player.FirearmController controller)
			{
				// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
				CS_0024_003C_003E8__locals10.botStationaryWeaponData_0.method_4(controller);
				CS_0024_003C_003E8__locals10.botStationaryWeaponData_0.IsInSpawnState = false;
				CS_0024_003C_003E8__locals10.fireArms.OnReadyToOperate -= CS_0024_003C_003E8__locals10.method_0;
			};
		}
		else
		{
			method_4(CS_0024_003C_003E8__locals10.fireArms);
		}
	}

	public void method_4(Player.FirearmController fireArms)
	{
		if (BotOwner_0.WeaponManager.Stationary != null)
		{
			if (BotOwner_0.WeaponManager.Stationary.CurLink != null && fireArms.Item == BotOwner_0.WeaponManager.Stationary.CurLink.Weapon.Item)
			{
				BotOwner_0.WeaponManager.StationaryTaken(fireArms, BotOwner_0.WeaponManager.Stationary.CurLink.Weapon);
				CanLeave = true;
			}
			else
			{
				BotOwner_0.WeaponManager.CheckCurMainWeapon();
				DropCurWeapon(withDelay: false);
			}
		}
	}

	public bool method_5()
	{
		if (SpottedTimes > BotOwner_0.Settings.FileSettings.Cover.STATIONARY_SPOTTED_TIMES_TO_LEAVE && Time.time - LastSpottedTime < 1f)
		{
			return true;
		}
		return false;
	}

	public void Dispose()
	{
		DropCurWeapon(withDelay: false, withLeavingAsPlayer: false);
		if (CurLink != null && CurLink.CurOwnerId == BotOwner_0.Id)
		{
			CurLink.ClearOwner();
		}
		BotOwner_0.GetPlayer.MovementContext.OnHandsControllerChanged -= method_3;
	}
}
