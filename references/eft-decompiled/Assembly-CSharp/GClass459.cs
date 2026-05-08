using System;
using Comfort.Common;
using EFT;
using UnityEngine;

public class GClass459 : GClass429
{
	[NonSerialized]
	public const float Float_0 = 0.35f;

	[NonSerialized]
	public const float Float_1 = 0.1f;

	[NonSerialized]
	public AICoreActionEndStruct AICoreActionEndStruct;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public float Float_3 = -1f;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public float Float_5 = 999f;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public bool Bool_3;

	[NonSerialized]
	public GClass25 Gclass25_0;

	public BotGlobalsBossSettings BotGlobalsBossSettings_0 => BotOwner_0.Settings.FileSettings.Boss;

	public static GClass459 Create(BotOwner bot)
	{
		return new GClass459(bot);
	}

	public GClass459(BotOwner owner)
		: base(owner)
	{
		Gclass25_0 = new GClass25(10f, method_0);
	}

	public BotLogicDecision DoMeleeAssault()
	{
		if (method_5())
		{
			Float_4 = 0f;
			Bool_0 = true;
			Float_5 = PathToEnemyLengthByNavMesh();
			return BotLogicDecision.oneMeleeAttack;
		}
		return BotLogicDecision.goToEnemy;
	}

	public float PathToEnemyLengthByNavMesh()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			Float_3 = float.MaxValue;
			return Float_3;
		}
		if (Float_3 < 0f || Float_2 + 0.35f < Time.time)
		{
			Float_3 = BotOwner_0.Mover.ComputePathLengthToPoint(goalEnemy.CurrPosition);
			Float_2 = Time.time;
		}
		return Float_3;
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> AttackOrHoldOrShoot()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			if (BotOwner_0.Memory.GoalEnemy != null && BotOwner_0.Memory.GoalEnemy.CanShoot)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromCover, "AttackOrHol");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "AttackOrHol");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "AttackOrHol");
	}

	public bool WantMeleeAssault(bool cachedDecision = true)
	{
		bool num = method_1(cachedDecision);
		if (num)
		{
			SetLastStartAttack();
		}
		return num;
	}

	public void method_0()
	{
		Bool_3 = GClass856.RandomBool(BotGlobalsBossSettings_0.TAGILLA_MELEE_CHANCE_FORCED);
	}

	public bool method_1(bool cachedDecision = true)
	{
		if (cachedDecision && Float_4 + BotGlobalsBossSettings_0.TAGILLA_MELEE_ATTACK_NEXT_DECISION_PERIOD > Time.time)
		{
			return Bool_1;
		}
		if (!BotOwner_0.WeaponManager.CanChangeHands())
		{
			Bool_1 = false;
			return Bool_1;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return false;
		}
		if (!method_6())
		{
			Bool_1 = false;
			return Bool_1;
		}
		if (Float_6 + BotGlobalsBossSettings_0.TAGILLA_MIN_TIME_TO_REPEAT_MELEE_ASSAULT > Time.time)
		{
			Bool_1 = false;
			return Bool_1;
		}
		if (BotOwner_0.WeaponManager.Reload.Reloading)
		{
			Bool_1 = false;
			return Bool_1;
		}
		if (method_7())
		{
			Gclass25_0.Update();
			Bool_1 = Bool_3;
			return Bool_1;
		}
		if (EnemySprintingNow())
		{
			return false;
		}
		if (method_2(goalEnemy))
		{
			Bool_1 = true;
			return Bool_1;
		}
		Player alivePlayerByProfileID = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(goalEnemy.Person.ProfileId);
		Player.FirearmController firearmController = alivePlayerByProfileID.HandsController as Player.FirearmController;
		if (firearmController != null && firearmController.IsInReloadOperation())
		{
			Bool_1 = GClass856.RandomBool(BotGlobalsBossSettings_0.TAGILLA_MELEE_CHANCE_RELOAD);
			return Bool_1;
		}
		if (alivePlayerByProfileID.HandsController as Player.MedsController != null)
		{
			Bool_1 = GClass856.RandomBool(BotGlobalsBossSettings_0.TAGILLA_MELEE_CHANCE_MEDS);
			return Bool_1;
		}
		if (alivePlayerByProfileID.HandsController.IsInInteraction())
		{
			Bool_1 = GClass856.RandomBool(BotGlobalsBossSettings_0.TAGILLA_MELEE_CHANCE_INTERACTION);
			SetLastStartAttack();
			return Bool_1;
		}
		if (method_3(alivePlayerByProfileID))
		{
			Bool_1 = true;
			return Bool_1;
		}
		if (firearmController != null && firearmController.IsInventoryOpen())
		{
			Bool_1 = GClass856.RandomBool(BotGlobalsBossSettings_0.TAGILLA_MELEE_CHANCE_INVENTORY);
			return Bool_1;
		}
		return false;
	}

	public bool method_2(EnemyInfo enemy)
	{
		return Vector3.Dot(enemy.Person.LookDirection, enemy.Direction) > 0f;
	}

	public bool method_3(Player enemyPlayer)
	{
		float tAGILLA_MELEE_ATTACK_IF_NO_SHOOT = BotGlobalsBossSettings_0.TAGILLA_MELEE_ATTACK_IF_NO_SHOOT;
		if (tAGILLA_MELEE_ATTACK_IF_NO_SHOOT < 0f)
		{
			return false;
		}
		return Time.time - enemyPlayer.AIData.LastShotTime > tAGILLA_MELEE_ATTACK_IF_NO_SHOOT;
	}

	public void SetLastStartAttack()
	{
		Float_4 = Time.time;
	}

	public void method_4()
	{
		Bool_0 = false;
		Float_5 = 999f;
		Float_6 = Time.time;
		Bool_1 = false;
	}

	public AICoreActionEndStruct EndOneMeleeAttack()
	{
		if (Float_4 < 0.1f)
		{
			Float_4 = Time.time;
		}
		float num = Mathf.Max(BotOwner_0.WeaponManager.Melee.LastTimeEnemyHit, Float_4);
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			Float_4 = 0f;
			method_4();
			return new AICoreActionEndStruct("enemynull");
		}
		if (Float_3 < 0f || Float_2 + 0.35f < Time.time)
		{
			Float_3 = BotOwner_0.Mover.ComputePathLengthToPoint(goalEnemy.CurrPosition);
			Float_2 = Time.time;
		}
		if (num > 0f && num + BotGlobalsBossSettings_0.TAGILLA_TIME_TO_PURSUIT_WITHOUT_HITS < Time.time)
		{
			method_4();
			return new AICoreActionEndStruct("pursuitTime");
		}
		if (Float_3 >= Float_5 * 1.35f && Float_3 >= 10f)
		{
			method_4();
			return new AICoreActionEndStruct("pursuitPath");
		}
		if (Float_3 > BotGlobalsBossSettings_0.TAGILLA_SECOND_ASSAULT_RADIUS)
		{
			method_4();
			BotOwner_0.DecisionQueue.Clear();
			BotOwner_0.DecisionQueue.Enqueue(new GClass537(BotLogicDecision.attackMoving));
			return new AICoreActionEndStruct("lostsecondr");
		}
		float num2 = Mathf.Abs(BotOwner_0.Memory.GoalEnemy.CurrPosition.y - BotOwner_0.Position.y);
		if (!goalEnemy.IsVisible && num2 > BotOwner_0.Settings.FileSettings.Boss.TAGILLA_Y_DELTA_TO_BE_ENEMY_BOSS)
		{
			method_4();
			return new AICoreActionEndStruct("AnotherFloo");
		}
		if (!goalEnemy.IsVisible && goalEnemy.Distance > BotGlobalsBossSettings_0.TAGILLA_SECOND_ASSAULT_RADIUS)
		{
			method_4();
			return new AICoreActionEndStruct("lostsecondr");
		}
		Float_5 = Float_3;
		return AICoreActionEndStruct;
	}

	public bool method_5()
	{
		return Time.time - BotOwner_0.WeaponManager.Melee.LastTimeEnemyHit >= 0.1f;
	}

	public bool method_6()
	{
		if (BotOwner_0.Memory.GoalEnemy == null)
		{
			return false;
		}
		if (PathToEnemyLengthByNavMesh() < BotGlobalsBossSettings_0.TAGILLA_FORCED_CLOSE_ATTACK_DIST)
		{
			return true;
		}
		if (PathToEnemyLengthByNavMesh() > BotGlobalsBossSettings_0.TAGILLA_SECOND_ASSAULT_RADIUS)
		{
			return false;
		}
		float v = ((PathToEnemyLengthByNavMesh() < BotGlobalsBossSettings_0.TAGILLA_FIRST_ASSAULT_RADIUS) ? BotGlobalsBossSettings_0.TAGILLA_FIRST_ASSAULT_CHANCE : BotGlobalsBossSettings_0.TAGILLA_SECOND_ASSAULT_CHANCE);
		if (Time.time > Float_7 + 4f)
		{
			Bool_2 = GClass856.IsTrue100(v);
			Float_7 = Time.time;
		}
		if (Mathf.Abs(BotOwner_0.Memory.GoalEnemy.CurrPosition.y - BotOwner_0.Position.y) < BotGlobalsBossSettings_0.TAGILLA_Y_DELTA_TO_BE_ENEMY_BOSS)
		{
			return Bool_2;
		}
		return false;
	}

	public bool method_7()
	{
		float tAGILLA_FORCED_CLOSE_ATTACK_DIST = BotOwner_0.Settings.FileSettings.Boss.TAGILLA_FORCED_CLOSE_ATTACK_DIST;
		if (tAGILLA_FORCED_CLOSE_ATTACK_DIST < 0f)
		{
			return false;
		}
		if (Mathf.Abs(BotOwner_0.Memory.GoalEnemy.CurrPosition.y - BotOwner_0.Position.y) < BotOwner_0.Settings.FileSettings.Boss.TAGILLA_Y_DELTA_TO_BE_ENEMY_BOSS)
		{
			return PathToEnemyLengthByNavMesh() < tAGILLA_FORCED_CLOSE_ATTACK_DIST;
		}
		return false;
	}

	public bool EnemySprintingNow()
	{
		return Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(BotOwner_0.Memory.GoalEnemy.Person.ProfileId).Velocity.magnitude > 2.4f;
	}
}
