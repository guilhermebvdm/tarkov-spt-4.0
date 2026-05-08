using System;
using Comfort.Common;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class ShootData
{
	[NonSerialized]
	public BotOwner Owner;

	[NonSerialized]
	public Player Player;

	[NonSerialized]
	public float NextFingerUpTime;

	[NonSerialized]
	public float TimeFingerDown;

	[NonSerialized]
	public float TriggersTimeToRun;

	[NonSerialized]
	public float TriggersDownBetweenStops;

	[NonSerialized]
	public float NextFingerDownCan;

	[NonSerialized]
	public RecoilData RecoilData;

	[NonSerialized]
	public GameWorld GameWorld;

	[NonSerialized]
	public Vector3 WeaponRootOffset_1;

	[NonSerialized]
	public float NextLongWaitBegin = -1f;

	public Vector3 WeaponRootOffset => WeaponRootOffset_1;

	[field: NonSerialized]
	public float LastTriggerPressd { get; set; }

	public IFirearmHandsController ShootController => Owner.WeaponManager.ShootController;

	[field: NonSerialized]
	public bool Shooting { get; set; }

	public BotGlobalAimingSettings BotGlobalAimingSettings_0 => Owner.Settings.FileSettings.Aiming;

	[field: NonSerialized]
	public bool CanShootByState { get; set; }

	public event Action OnTriggerPressed;

	public ShootData(BotOwner owner, RecoilData recoilData)
	{
		TriggersTimeToRun = GClass856.GreateRandom(LocalBotSettingsProviderClass.Core.TRIGGERS_DOWN_TO_RUN_WHEN_MOVE);
		RecoilData = recoilData;
		Owner = owner;
		Player = Owner.GetPlayer;
		GameWorld = Singleton<GameWorld>.Instance;
	}

	public void Activate()
	{
		if (Owner.Settings.FileSettings.Shoot.CAN_STOP_SHOOT_CAUSE_ANIMATOR)
		{
			Owner.GetPlayer.MovementContext.OnStateChanged += method_1;
		}
		else
		{
			CanShootByState = true;
		}
		WeaponRootOffset_1 = Vector3.up * 1.4f;
		Owner.GetPlayer.MovementContext.OnStateChanged += method_1;
		Owner.AITaskManager.RegisterDelayedTask(Owner, 1f, method_0);
	}

	public void SetCanShootByState(bool state)
	{
		CanShootByState = state;
	}

	public void method_0()
	{
		WeaponRootOffset_1 = Owner.WeaponRoot.position - Owner.Position;
	}

	public void method_1(EPlayerState previousstate, EPlayerState nextstate)
	{
		switch (nextstate)
		{
		case EPlayerState.Sprint:
		case EPlayerState.Jump:
		case EPlayerState.FallDown:
		case EPlayerState.Transition:
		case EPlayerState.BreachDoor:
		case EPlayerState.Loot:
		case EPlayerState.Pickup:
		case EPlayerState.Open:
		case EPlayerState.Close:
		case EPlayerState.Unlock:
		case EPlayerState.DoorInteraction:
		case EPlayerState.Approach:
		case EPlayerState.Prone2Stand:
		case EPlayerState.Transit2Prone:
		case EPlayerState.Plant:
			ShootController.SetTriggerPressed(pressed: false);
			CanShootByState = false;
			break;
		default:
			CanShootByState = true;
			break;
		}
	}

	public void NullShootsBetween()
	{
		TriggersDownBetweenStops = 0f;
	}

	public void ManualUpdate()
	{
		if (Shooting)
		{
			method_2();
			if (NextFingerUpTime < Time.time)
			{
				EndShoot();
			}
			else if (!Owner.WeaponManager.HaveBullets)
			{
				EndShoot();
			}
		}
	}

	public bool CheckFriendlyFire(Vector3 from, Vector3 to)
	{
		Player player = method_3(from, to);
		if (player != null)
		{
			if (player.Id == Owner.Id)
			{
				return false;
			}
			if (Owner.Memory.GoalEnemy != null && player.Id == Owner.Memory.GoalEnemy.Person.Id)
			{
				return false;
			}
			if (player.Profile.Info.Side != Player.Side)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public bool Shoot()
	{
		BotWeaponManager weaponManager = Owner.WeaponManager;
		if (Owner.Settings.FileSettings.Shoot.USE_BTR_CANSHOOT && !Owner.BotBtrData.CanShoot())
		{
			return false;
		}
		if (weaponManager.IsMelee)
		{
			weaponManager.Selector.TryChangeWeapon();
			return false;
		}
		if (!weaponManager.IsWeaponReady)
		{
			return false;
		}
		if (Owner.WeaponManager.Reload.Reloading)
		{
			return false;
		}
		if (!CanShootByState)
		{
			return false;
		}
		BotUnderbarrelLauncherController underbarrelLauncherController = Owner.WeaponManager.UnderbarrelLauncherController;
		if (underbarrelLauncherController.IsActive)
		{
			if (underbarrelLauncherController.NeedToReload() && !underbarrelLauncherController.TryReload())
			{
				underbarrelLauncherController.TryDisable();
				return false;
			}
			if (!underbarrelLauncherController.CheckShootAttemptAndDisableIfNeeded())
			{
				return false;
			}
			NextFingerDownCan = Time.time - 0.1f;
		}
		else if (!Owner.WeaponManager.HaveBullets)
		{
			Owner.WeaponManager.Reload.TryReload();
		}
		if (!Shooting && NextFingerDownCan < Time.time)
		{
			Owner.Memory.DangerData.ShootDone();
			Owner.Memory.BotCurrentCoverInfo.ShootsFromCover++;
			method_6();
			if (!Owner.Memory.IsInCover && !Owner.BotLay.IsLay)
			{
				TriggersDownBetweenStops += 1f;
			}
			Shooting = true;
			TimeFingerDown = Time.time;
			LastTriggerPressd = Time.time;
			if (ShootController != null)
			{
				ShootController.IsInLauncherMode();
				ShootController.SetTriggerPressed(pressed: true);
				Owner.AimingManager.CurrentAiming.TriggerPressedDone();
				this.OnTriggerPressed?.Invoke();
				return true;
			}
		}
		return false;
	}

	public void ShootDoneWeapon()
	{
		Owner.EnemiesController.ShootDone();
		Owner.BotPersonalStats?.ShootToTarget(Owner.Memory?.GoalEnemy, Owner.AimingManager.CurrentAiming?.EndTargetPoint);
		Owner.AimingManager.CurrentAiming?.ShootDone(Owner.WeaponManager.CurrentWeapon);
	}

	public void EndShoot()
	{
		if (Shooting)
		{
			method_5();
			Shooting = false;
			ShootController?.SetTriggerPressed(pressed: false);
		}
	}

	public bool IsManySHootsOnAttack()
	{
		return TriggersDownBetweenStops > TriggersTimeToRun;
	}

	public void method_2()
	{
		if (Time.time - TimeFingerDown > Owner.Settings.FileSettings.Shoot.RECOIL_DELTA_PRESS)
		{
			TimeFingerDown = Time.time;
			RecoilData.Recoil();
		}
	}

	[CanBeNull]
	public Player method_3(Vector3 weaponRootPos, Vector3 aimTo)
	{
		Player player = null;
		float sHPERE_FRIENDY_FIRE_SIZE = Owner.Settings.FileSettings.Aiming.SHPERE_FRIENDY_FIRE_SIZE;
		float num = 0.6f;
		LayerMask playerMask = LayerMaskClass.PlayerMask;
		if (sHPERE_FRIENDY_FIRE_SIZE > 0f)
		{
			num += sHPERE_FRIENDY_FIRE_SIZE;
		}
		Vector3 v = aimTo - weaponRootPos;
		Vector3 vector = GClass855.NormalizeFastSelf(v);
		weaponRootPos += vector * num;
		RaycastHit hitInfo;
		if (sHPERE_FRIENDY_FIRE_SIZE > 0f)
		{
			RaycastHit[] array = new RaycastHit[1];
			if (Physics.SphereCastNonAlloc(new Ray(weaponRootPos, vector), sHPERE_FRIENDY_FIRE_SIZE, array, v.magnitude, playerMask) > 0)
			{
				RaycastHit[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					RaycastHit raycastHit = array2[i];
					if (raycastHit.collider != null)
					{
						player = method_4(raycastHit.collider);
						if (player != null)
						{
							break;
						}
					}
				}
			}
		}
		else if (Physics.Linecast(weaponRootPos, aimTo, out hitInfo, playerMask))
		{
			player = method_4(hitInfo.collider);
		}
		return player;
	}

	[CanBeNull]
	public Player method_4(Collider collider)
	{
		return GameWorld.GetPlayerByCollider(collider);
	}

	public void method_5()
	{
		float num = Owner.WeaponManager.WeaponAIPreset.HoldTriggerUpTime();
		if (Owner.IsRole(WildSpawnType.marksman))
		{
			num = Mathf.Clamp((Owner.AimingManager.CurrentAiming.RealTargetPoint - Owner.Transform.position).magnitude / Owner.Settings.FileSettings.Shoot.MARKSMAN_DIST_SEK_COEF, Owner.Settings.FileSettings.Shoot.WAIT_NEXT_SINGLE_SHOT_LONG_MIN, Owner.Settings.FileSettings.Shoot.WAIT_NEXT_SINGLE_SHOT_LONG_MAX);
		}
		if (Owner.Settings.FileSettings.Shoot.USE_SINGLE_SHOT_SERIES && Time.time > NextLongWaitBegin)
		{
			if (NextLongWaitBegin > 0f)
			{
				num += Owner.Settings.FileSettings.Shoot.NEXT_SINGLE_SHOT_PAUSE;
			}
			NextLongWaitBegin = Time.time + Owner.Settings.FileSettings.Shoot.NEXT_SINGLE_SHOT_PAUSE + GClass856.Random(Owner.Settings.FileSettings.Shoot.SINGLE_SHOT_SERIES_TIME_MIN, Owner.Settings.FileSettings.Shoot.SINGLE_SHOT_SERIES_TIME_MAX);
		}
		float num2 = num * UnityEngine.Random.Range(0.5f, 1.5f);
		NextFingerDownCan = Time.time + num2;
	}

	public void Dispose()
	{
		Owner.GetPlayer.MovementContext.OnStateChanged -= method_1;
	}

	public void method_6()
	{
		float num = Owner.WeaponManager.WeaponAIPreset.TriggerDownTime();
		NextFingerUpTime = Time.time + num;
	}

	public void BlockFor(float period)
	{
		NextFingerDownCan = Mathf.Max(NextFingerDownCan, Time.time + period);
		NextFingerUpTime = Time.time;
	}
}
