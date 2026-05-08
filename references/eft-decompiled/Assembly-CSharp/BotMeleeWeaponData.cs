using System;
using EFT;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

public class BotMeleeWeaponData : GClass429
{
	[NonSerialized]
	public const float BIG_PERIOD = 10f;

	[NonSerialized]
	public const float CHANCE_TO_SAY = 100f;

	[NonSerialized]
	public const float SDIST_TOO_CLOSE = 4f;

	[NonSerialized]
	public const float SMAPLE_DIST = 2.6f;

	[NonSerialized]
	public const float TOOFAR_BASE_PATH_DIST = 2f;

	[NonSerialized]
	public float TRY_HIT_PERIOD_FALSE = 0.1f;

	[NonSerialized]
	public bool DebugComboEnabled;

	[NonSerialized]
	public float FAR_DIST = 8f;

	[NonSerialized]
	public float MID_DIST = 5f;

	[NonSerialized]
	public float FarRecalc = 1f;

	[NonSerialized]
	public float MidRecalc = 0.6f;

	[NonSerialized]
	public float CloseRecalc = 0.3f;

	[NonSerialized]
	public float MidRecalcZZ = 0.4f;

	[NonSerialized]
	public float CloseRecalcZZ = 0.2f;

	[NonSerialized]
	public Vector3 LastTarget = Vector3.zero;

	[NonSerialized]
	public const float NO_RECAL_CAUSE_TOO_CLOSE = 0.09f;

	[NonSerialized]
	public bool LastCanHitResult;

	[NonSerialized]
	public Vector3[] PathToRun;

	[NonSerialized]
	public float RunPathCheck;

	[NonSerialized]
	public float NextTryHitTime;

	[NonSerialized]
	public Vector3[] LastCalcPath;

	public bool Running = true;

	[NonSerialized]
	public bool UseZigZag;

	[field: NonSerialized]
	public bool MeleeWeaponEquipped { get; set; }

	[field: NonSerialized]
	public bool HaveMelee { get; set; }

	public bool Boolean_0 => BotOwner_0.Settings.FileSettings.Shoot.USE_MELEE_COMBOS;

	[field: NonSerialized]
	public IKnifeController KnifeController { get; set; }

	public float Single_0 => BotOwner_0.Settings.FileSettings.Shoot.DIST_TO_HIT_MELEE;

	public float Single_1 => BotOwner_0.Settings.FileSettings.Shoot.DIST_TO_STOP_SPRINT_MELEE;

	public float Single_2 => BotOwner_0.Settings.FileSettings.Shoot.TRY_HIT_PERIOD_MELEE;

	public bool meleeOptimize => BotOwner_0.Settings.FileSettings.Move.MELEE_ATTACK_OPTIMIZE;

	[field: NonSerialized]
	public float LastTimeEnemyHit { get; set; } = -100000f;

	[field: NonSerialized]
	public bool ShallEndRun { get; set; }

	public event Action<BotOwner, Player> OnEnemyHitted;

	public event Action<BotOwner> OnTryHit;

	public BotMeleeWeaponData(BotOwner owner)
		: base(owner)
	{
	}

	public void Activate()
	{
		HaveMelee = BotOwner_0.GetPlayer.method_130(EquipmentSlot.Scabbard);
		UseZigZag = BotOwner_0.Settings.FileSettings.Move.MELEE_ATTACK_ZIG_ZAG;
		if (meleeOptimize)
		{
			FarRecalc = 2f;
			MidRecalc = 1f;
			CloseRecalc = 0.7f;
			MidRecalcZZ = 0.8f;
			CloseRecalcZZ = 0.7f;
		}
	}

	public void method_0(float delay)
	{
		NextTryHitTime = Time.time + delay;
	}

	public bool RunToEnemyUpdate()
	{
		ShallEndRun = false;
		if (!BotOwner_0.WeaponManager.IsMelee)
		{
			if (!BotOwner_0.WeaponManager.Selector.CanChangeToMeleeWeapons)
			{
				ShallEndRun = true;
				return false;
			}
			BotOwner_0.WeaponManager.Selector.ChangeToMelee();
		}
		if (BotOwner_0.BotLay.IsLay)
		{
			BotOwner_0.BotLay.GetUp(withCheck: false);
		}
		BotOwner_0.SetPose(1f);
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		bool flag;
		if (flag = goalEnemy.Distance < Single_0)
		{
			BotOwner_0.Steering.LookToPoint(goalEnemy.GetBodyPartPosition());
			if (goalEnemy.Person.AIData.Player.MovementContext.IsInPronePose)
			{
				BotOwner_0.SetPose(0f);
			}
		}
		else
		{
			BotOwner_0.SetPose(1f);
			BotOwner_0.Steering.LookToMovingDirection();
		}
		BotOwner_0.Sprint(Running && goalEnemy.Distance > Single_1);
		if (NextTryHitTime < Time.time)
		{
			bool flag2 = flag && method_2(goalEnemy);
			method_0(flag2 ? 10f : TRY_HIT_PERIOD_FALSE);
		}
		if (BotOwner_0.Mover.HasPathAndNoComplete)
		{
			if (RunPathCheck < Time.time)
			{
				float num = ((!UseZigZag) ? ((goalEnemy.Distance > FAR_DIST) ? FarRecalc : ((goalEnemy.Distance > MID_DIST) ? MidRecalc : CloseRecalc)) : ((goalEnemy.Distance > FAR_DIST) ? FarRecalc : ((goalEnemy.Distance > MID_DIST) ? MidRecalcZZ : CloseRecalcZZ)));
				RunPathCheck = Time.time + num;
				if (!CanRunToEnemyToHit(goalEnemy, out var pathToRun))
				{
					ShallEndRun = true;
					return false;
				}
				if (goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Shoot.MELEE_STOP_MOVE_DISTANCE)
				{
					BotOwner_0.StopMove();
				}
				else
				{
					BotOwner_0.GoToByWay(pathToRun);
				}
			}
		}
		else
		{
			if (!CanRunToEnemyToHit(goalEnemy, out var pathToRun2))
			{
				ShallEndRun = true;
				return false;
			}
			if (goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Shoot.MELEE_STOP_MOVE_DISTANCE)
			{
				BotOwner_0.StopMove();
			}
			else
			{
				BotOwner_0.GoToByWay(pathToRun2);
			}
		}
		return true;
	}

	public bool CanRunToEnemyToHit([NotNull] EnemyInfo enemy, out Vector3[] pathToRun)
	{
		Vector3 targetPoint;
		if (enemy.Distance < 5f && enemy.Distance > 0f)
		{
			Vector3 vector = GClass855.NormalizeFastSelf(enemy.Direction) * BotOwner_0.Settings.FileSettings.Shoot.MELEE_STOP_DIST;
			targetPoint = enemy.CurrPosition + vector;
			if (method_1(targetPoint, out pathToRun))
			{
				return true;
			}
		}
		targetPoint = enemy.CurrPosition;
		return method_1(targetPoint, out pathToRun);
	}

	public void HitCurrentEnemy(DamageInfoStruct damageInfo, float val, Player target)
	{
		LastTimeEnemyHit = Time.time;
		this.OnEnemyHitted?.Invoke(BotOwner_0, target);
	}

	public bool method_1(Vector3 targetPoint, out Vector3[] pathToRun)
	{
		if ((LastTarget - targetPoint).sqrMagnitude < 0.09f && PathToRun != null)
		{
			pathToRun = LastCalcPath;
			return LastCanHitResult;
		}
		NavMeshPath navMeshPath = new NavMeshPath();
		NavMesh.CalculatePath(BotOwner_0.Position, targetPoint, -1, navMeshPath);
		bool flag;
		if (flag = navMeshPath.status == NavMeshPathStatus.PathComplete)
		{
			Vector3 vector = navMeshPath.corners[navMeshPath.corners.Length - 1];
			if ((targetPoint - vector).magnitude > 2f)
			{
				flag = false;
			}
		}
		if (!flag && NavMesh.SamplePosition(targetPoint, out var hit, 2.6f, -1))
		{
			navMeshPath = new NavMeshPath();
			NavMesh.CalculatePath(BotOwner_0.Position, hit.position, -1, navMeshPath);
			if (navMeshPath.status == NavMeshPathStatus.PathComplete)
			{
				flag = true;
			}
		}
		if (flag)
		{
			if (UseZigZag && GClass635.ModifyZigZagPercentOffset(navMeshPath.corners, 1.2f, 100f, 1.4f, 0.5f, out var modifedWay))
			{
				pathToRun = (LastCalcPath = modifedWay.ToArray());
				LastCanHitResult = true;
				return LastCanHitResult;
			}
			pathToRun = (LastCalcPath = navMeshPath.corners);
			LastCanHitResult = true;
			return LastCanHitResult;
		}
		pathToRun = null;
		LastCanHitResult = false;
		return LastCanHitResult;
	}

	public bool method_2(EnemyInfo enemy)
	{
		if (MeleeWeaponEquipped && enemy.IsVisible && Time.time - enemy.PersonalLastSeenTime < 0.2f && KnifeController != null)
		{
			if (GClass856.IsTrue100(100f))
			{
				BotOwner_0.BotTalk.DropNextSayPeriod();
				BotOwner_0.BotTalk.Say(EPhraseTrigger.KnifeKill, sayImmediately: true);
			}
			bool result = ((!BotOwner_0.Settings.FileSettings.Shoot.ALTERNATIVE_KNIFE_KICK) ? KnifeController.MakeKnifeKick() : KnifeController.MakeAlternativeKick());
			this.OnTryHit?.Invoke(BotOwner_0);
			return result;
		}
		return false;
	}

	public void UpdateKnifeController(IKnifeController knifeHandsController)
	{
		if (knifeHandsController != null)
		{
			if (KnifeController != null)
			{
				IKnifeController knifeController = KnifeController;
				knifeController.ComboPlanning = (Action)Delegate.Remove(knifeController.ComboPlanning, new Action(method_3));
				IKnifeController knifeController2 = KnifeController;
				knifeController2.OnAttackEnd = (Action)Delegate.Remove(knifeController2.OnAttackEnd, new Action(method_4));
			}
			KnifeController = knifeHandsController;
			if (Boolean_0)
			{
				KnifeController.SetBotParameters();
				IKnifeController knifeController3 = KnifeController;
				knifeController3.ComboPlanning = (Action)Delegate.Combine(knifeController3.ComboPlanning, new Action(method_3));
			}
			MeleeWeaponEquipped = true;
			BotOwner_0.WeaponManager.Selector.IsWeaponReady = true;
			IKnifeController knifeController4 = KnifeController;
			knifeController4.OnAttackEnd = (Action)Delegate.Combine(knifeController4.OnAttackEnd, new Action(method_4));
		}
		else
		{
			MeleeWeaponEquipped = false;
		}
	}

	public void SetDebugCombo(bool state)
	{
		DebugComboEnabled = state;
	}

	public void method_3()
	{
		if (DebugComboEnabled)
		{
			if (UnityEngine.Random.Range(0, 100) > 50)
			{
				KnifeController.ContinueCombo();
			}
			else
			{
				KnifeController.BrakeCombo();
			}
		}
		else if (BotOwner_0.Memory.GoalEnemy != null && BotOwner_0.Memory.GoalEnemy.Owner != null && GClass856.SqrDistance(BotOwner_0.Memory.GoalEnemy.Person.Position, BotOwner_0.Position) < BotOwner_0.Settings.FileSettings.Shoot.DIST_TO_HIT_MELEE_CONTINUE_COMBO)
		{
			KnifeController.ContinueCombo();
		}
		else
		{
			KnifeController.BrakeCombo();
		}
	}

	public void method_4()
	{
		method_0(BotOwner_0.Settings.FileSettings.Shoot.MELEE_RESET_HIT_TIME);
	}

	public void Dispose()
	{
		if (KnifeController != null)
		{
			IKnifeController knifeController = KnifeController;
			knifeController.OnAttackEnd = (Action)Delegate.Remove(knifeController.OnAttackEnd, new Action(method_4));
		}
	}
}
