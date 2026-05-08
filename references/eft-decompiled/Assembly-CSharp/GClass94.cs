using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using EFT.HealthSystem;
using UnityEngine;

public class GClass94(BotOwner bot, int priority) : BaseLogicLayerSimpleAbstractClass(bot, priority)
{
	[CompilerGenerated]
	public class Class218
	{
		public Vector3 posEnemy;

		public bool method_0(GroupPoint point)
		{
			float magnitude = (point.Position - posEnemy).magnitude;
			if (magnitude > 50f)
			{
				return false;
			}
			if (magnitude < 15f)
			{
				return false;
			}
			return true;
		}

		public bool method_1(GroupPoint point)
		{
			float magnitude = (point.Position - posEnemy).magnitude;
			if (magnitude > 60f)
			{
				return false;
			}
			if (magnitude < 15f)
			{
				return false;
			}
			return true;
		}
	}

	public const float DIST_TO_MISFIRE = 60f;

	public const float DELTA_START_ATTACK = 18f;

	public const float DELTA_HIT_ATTACK = 12f;

	public const float DIST_TO_RUN_TO_ENEMY = 15f;

	public const float DIST_TO_TELEPORT = 60f;

	public const float PERFECT_DIST = 50f;

	public const int BUFF_UPDATE_TIME = 5;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public float Float_4 = -1f;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public HashSet<IPlayer> HashSet_0 = new HashSet<IPlayer>();

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.Distance > 60f && !goalEnemy.IsVisible)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "telep");
		}
		bool flag = method_16();
		if (goalEnemy.IsVisible && !flag)
		{
			if (goalEnemy.CanShoot)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "nows");
			}
			if (goalEnemy.Distance > 60f)
			{
				method_15();
			}
		}
		if (flag)
		{
			Float_4 = Time.time;
			BotOwner_0.WeaponManager.Melee.Running = false;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.oneMeleeAttack, "cut");
		}
		if (method_13())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "csat");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToEnemy, "runTo");
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.Memory.HaveEnemy;
	}

	public override string Name()
	{
		return "RavangeZryachiy";
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		return AICoreActionEndStruct;
	}

	public override void ManualUpdate()
	{
		base.ManualUpdate();
		method_14();
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		return AICoreActionEndStruct;
	}

	public bool method_13()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (!goalEnemy.IsVisible)
		{
			return false;
		}
		if (!goalEnemy.CanShoot)
		{
			return false;
		}
		return true;
	}

	public override AICoreActionEndStruct EndRunToEnemy()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.Distance > 60f)
		{
			return new AICoreActionEndStruct("tooFar");
		}
		if (method_13())
		{
			return new AICoreActionEndStruct("nShoot");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndOneMeleeAttack()
	{
		if (BotOwner_0.Memory.GoalEnemy.Distance > 60f)
		{
			return new AICoreActionEndStruct("tooFar2");
		}
		if (!method_16())
		{
			return new AICoreActionEndStruct("cantMelee");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		method_15();
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (!(goalEnemy.Distance < 60f) && !goalEnemy.IsVisible)
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("goodStart");
	}

	public void method_14()
	{
		if (Float_5 > Time.time)
		{
			return;
		}
		Float_5 = Time.time + 5f;
		if ((Vector3_0 - BotOwner_0.Position).sqrMagnitude < 4f)
		{
			return;
		}
		Vector3_0 = BotOwner_0.Position;
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in BotOwner_0.EnemiesController.EnemyInfos)
		{
			if (enemyInfo.Value.Distance < 60f && enemyInfo.Key.HealthController is ActiveHealthController activeHealthController && !HashSet_0.Contains(enemyInfo.Key))
			{
				HashSet_0.Add(enemyInfo.Key);
				activeHealthController.DoEventEffect();
			}
		}
	}

	public void method_15()
	{
		if (Float_3 > Time.time)
		{
			return;
		}
		Float_3 = Time.time + 10f;
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		Vector3 vector = GClass855.NormalizeFastSelf(goalEnemy.Direction);
		Vector3 pos = goalEnemy.CurrPosition - vector * 60f * 0.25f;
		Vector3 posEnemy = goalEnemy.CurrPosition;
		CustomNavigationPoint closestPoint = BotOwner_0.Covers.GetClosestPoint(pos, delegate(GroupPoint point)
		{
			float magnitude = (point.Position - posEnemy).magnitude;
			if (magnitude > 50f)
			{
				return false;
			}
			return !(magnitude < 15f);
		});
		if (closestPoint != null)
		{
			BotOwner_0.Mover.Teleport(closestPoint.Position);
			return;
		}
		closestPoint = BotOwner_0.Covers.GetClosestPoint(pos, delegate(GroupPoint point)
		{
			float magnitude = (point.Position - posEnemy).magnitude;
			if (magnitude > 60f)
			{
				return false;
			}
			return !(magnitude < 15f);
		});
		if (closestPoint != null)
		{
			BotOwner_0.Mover.Teleport(closestPoint.Position);
		}
	}

	public bool method_16()
	{
		if (Float_4 < 0f)
		{
			return true;
		}
		if (Time.time - Float_4 < 18f)
		{
			return true;
		}
		if (Time.time - BotOwner_0.WeaponManager.Melee.LastTimeEnemyHit < 12f)
		{
			return true;
		}
		return false;
	}

	public override void Dispose()
	{
		base.Dispose();
	}
}
