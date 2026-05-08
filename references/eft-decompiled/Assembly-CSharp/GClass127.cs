using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using UnityEngine;

public class GClass127 : GClass125
{
	[CompilerGenerated]
	public class Class222
	{
		public Vector3 enemy;

		public GClass127 gclass127_0;

		public bool method_0(GroupPoint x)
		{
			if ((x.Position - enemy).sqrMagnitude > 2500f)
			{
				return false;
			}
			if (!x.IsSpotted)
			{
				return x.IsFreeById(gclass127_0.BotOwner_0.Id);
			}
			return false;
		}
	}

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public float Float_7 = 400f;

	[NonSerialized]
	public float Float_8 = 2500f;

	[NonSerialized]
	public float Float_9 = 4900f;

	[NonSerialized]
	public const float Float_10 = 4f;

	[NonSerialized]
	public const float Float_11 = 5f;

	[NonSerialized]
	public const float Float_12 = 5f;

	[NonSerialized]
	public const float Float_13 = 50f;

	public const float IF_SEEN_ENEMY_TOO_LATE_NO_MINES = 40f;

	[NonSerialized]
	public const float Float_14 = 2500f;

	[NonSerialized]
	public const float Float_15 = 70f;

	[NonSerialized]
	public const float Float_16 = 60f;

	[NonSerialized]
	public GClass25 Gclass25_0;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	public GClass127(BotOwner bot, int priority)
		: base(bot, priority)
	{
		Gclass25_0 = new GClass25(5f, method_16);
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		bool flag = Time.time - BotOwner_0.Memory.UnderFireTime < 5f;
		bool flag2 = goalEnemy.Distance < 70f;
		bool flag3 = Time.time - BotOwner_0.ShootData.LastTriggerPressd < 4f;
		if (method_15())
		{
			method_14();
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "clsSht");
		}
		if (flag2 && flag && BotOwner_0.Memory.IsInCover && Time.time - BotOwner_0.Memory.ComeToCoverTime > 2f)
		{
			method_18();
			if (CustomNavigationPoint_0 != null)
			{
				BotOwner_0.Memory.Spotted(byHit: false);
				BotOwner_0.Memory.SetCoverPoints(CustomNavigationPoint_0);
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "hitNear");
			}
		}
		global::AICoreActionResultStruct<BotLogicDecision, GClass26>? aICoreActionResultStruct = InFightLogic();
		if (aICoreActionResultStruct.HasValue)
		{
			if (flag2 && BotOwner_0.Memory.IsInCover && Time.time - BotOwner_0.Memory.ComeToCoverTime > 4f && (flag3 || flag))
			{
				method_18();
				if (CustomNavigationPoint_0 != null)
				{
					BotOwner_0.Memory.Spotted(byHit: false);
					BotOwner_0.Memory.SetCoverPoints(CustomNavigationPoint_0);
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "goNear");
				}
			}
			method_14();
			return aICoreActionResultStruct.Value;
		}
		if (goalEnemy.IsVisible)
		{
			if (goalEnemy.Distance < 70f && (flag || Time.time - BotOwner_0.Brain.Agent.LastPeriod < 5f))
			{
				method_18();
				if (CustomNavigationPoint_0 != null)
				{
					BotOwner_0.Memory.Spotted(byHit: false);
					BotOwner_0.Memory.SetCoverPoints(CustomNavigationPoint_0);
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "hitNear");
				}
			}
			method_14();
			return method_19("uk87");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			if (goalEnemy.IsVisible && goalEnemy.CanShoot)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "lspf");
			}
			method_13();
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "hkd4");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "dfgh4");
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		return base.FindPoint(data, p, checkCurrent);
	}

	public override AICoreActionEndStruct EndShootFromCover()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (Time.time - BotOwner_0.ShootData.LastTriggerPressd < 4f && goalEnemy.Distance < 50f)
		{
			method_18();
			if (CustomNavigationPoint_0 != null)
			{
				return new AICoreActionEndStruct("goBetter");
			}
		}
		return base.EndShootFromCover();
	}

	public void method_16()
	{
		BotOwner_0.MinesData.MinesRealtimePlaceFinder.TryFindAndPlant();
	}

	public void method_17()
	{
		AIMinePoint aIMinePoint = BotOwner_0.MinesData.FindClosestsUnplanted(BotOwner_0.Position);
		if (aIMinePoint == null || !((aIMinePoint.Position - BotOwner_0.Position).sqrMagnitude < Float_7))
		{
			return;
		}
		bool flag = true;
		foreach (Player allAlivePlayers in Singleton<GameWorld>.Instance.AllAlivePlayersList)
		{
			if (!allAlivePlayers.IsAI && !((aIMinePoint.Position - allAlivePlayers.Position).sqrMagnitude >= 81f))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			BotOwner_0.MinesData.PlantHereNow(aIMinePoint);
		}
	}

	public override void ManualUpdate()
	{
		base.ManualUpdate();
		Gclass25_0.Update();
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.Distance < 50f)
		{
			method_18();
			if (BotOwner_0.Memory.CurCustomCoverPoint == null)
			{
				return new AICoreActionEndStruct("noC");
			}
			if (CustomNavigationPoint_0 != null && CustomNavigationPoint_0.Id != BotOwner_0.Memory.CurCustomCoverPoint.Id)
			{
				BotOwner_0.Memory.Spotted(byHit: false);
				BotOwner_0.Memory.SetCoverPoints(CustomNavigationPoint_0);
				return new AICoreActionEndStruct("tCl");
			}
		}
		method_13();
		if (method_7())
		{
			return new AICoreActionEndStruct("EndHol");
		}
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("CanShoot");
		}
		if (goalEnemy.IsVisible && goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Cover.END_HOLD_IF_ENEMY_CLOSE_AND_VISIBLE)
		{
			return new AICoreActionEndStruct("CLOSEANDVIS");
		}
		return AICoreActionEndStruct_1;
	}

	public void method_18()
	{
		if (Time.time - Float_4 < 4f)
		{
			return;
		}
		Float_4 = Time.time;
		Vector3 vector2;
		if (BotOwner_0.Memory.HaveEnemy)
		{
			Vector3 vector = GClass855.NormalizeFastSelf(BotOwner_0.Memory.GoalEnemy.Direction);
			vector2 = BotOwner_0.Memory.GoalEnemy.CurrPosition - vector * 60f;
		}
		else
		{
			vector2 = BotOwner_0.Position;
		}
		Vector3 enemy = BotOwner_0.Memory.GoalEnemy.CurrPosition;
		CustomNavigationPoint customNavigationPoint = null;
		customNavigationPoint = BotOwner_0.Covers.GetClosestPoint(vector2, delegate(GroupPoint x)
		{
			if ((x.Position - enemy).sqrMagnitude > 2500f)
			{
				return false;
			}
			return !x.IsSpotted && x.IsFreeById(BotOwner_0.Id);
		}, printErrorLogsIfFail: false, 60);
		CustomNavigationPoint_0 = customNavigationPoint;
		if (CustomNavigationPoint_0 != null)
		{
			Debug.DrawRay(vector2, Vector3.up * 20f, Color.yellow, 10f);
			Debug.DrawRay(CustomNavigationPoint_0.Position, Vector3.up * 30f, Color.red, 10f);
		}
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		if (Time.time - BotOwner_0.Brain.Agent.LastPeriod > 5f)
		{
			return new AICoreActionEndStruct("dlt");
		}
		return base.EndShootFromPlace();
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_19(string info)
	{
		if (BotOwner_0.Memory.GoalEnemy.CanShoot)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, info);
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, info);
	}

	public override AICoreActionEndStruct EndAttackMoving()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDogFight()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return new AICoreActionEndStruct("enemynull");
		}
		if (!goalEnemy.IsVisible)
		{
			return new AICoreActionEndStruct("!visible");
		}
		if (goalEnemy.VisibleType == EEnemyPartVisibleType.Visible)
		{
			return AICoreActionEndStruct_1;
		}
		if (Time.time - goalEnemy.LastChangeVisionTypeTime < 1f)
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("!enemy.V");
	}

	public override bool ShallUseNow()
	{
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			return false;
		}
		method_20();
		if (Bool_4)
		{
			return true;
		}
		return false;
	}

	public void method_20()
	{
		if (Float_6 > Time.time)
		{
			return;
		}
		Float_6 = Time.time + 3f;
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			Bool_4 = false;
			return;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (Time.time - goalEnemy.PersonalLastSeenTime > 40f)
		{
			Bool_4 = false;
			return;
		}
		Vector3 currPosition = goalEnemy.CurrPosition;
		int num = 0;
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in BotOwner_0.EnemiesController.EnemyInfos)
		{
			if ((enemyInfo.Value.CurrPosition - currPosition).sqrMagnitude < Float_8 && (enemyInfo.Value.CurrPosition - BotOwner_0.Position).sqrMagnitude < Float_9)
			{
				num++;
			}
		}
		bool flag = num > 1;
		if (Bool_4 != flag)
		{
			Bool_4 = flag;
			if (Bool_4)
			{
				Float_6 = Time.time + 35f;
			}
		}
	}

	public override string Name()
	{
		return "PrtFMN";
	}
}
