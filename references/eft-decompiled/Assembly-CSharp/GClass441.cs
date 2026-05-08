using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT;
using EFT.HealthSystem;
using UnityEngine;

public class GClass441 : ABossLogic
{
	[Serializable]
	[CompilerGenerated]
	public class Class169
	{
		public static readonly Class169 class169_0 = new Class169();

		public static Func<AIPlaceInfo, bool> func_0;

		public bool method_0(AIPlaceInfo x)
		{
			return x.InfoLogicAllEnemy is AIPlaceInfoLogicKolontay;
		}
	}

	[CompilerGenerated]
	public class Class170
	{
		public GClass441 gclass441_0;

		public List<AIPlaceInfo> possibleAreas;

		public Vector3 method_0(int i)
		{
			return possibleAreas[i].transform.position - gclass441_0.BotOwner_0.Position;
		}
	}

	[CompilerGenerated]
	public class Class171
	{
		public BotOwner bot;

		public GClass441 gclass441_0;

		public void method_0()
		{
			bot.CalledData.TryCall(new CallEnemyTargetInfo(), gclass441_0.BotOwner_0, helpOnlyWithEnemy: false);
		}
	}

	public bool _secutiryMovingClose;

	[NonSerialized]
	public Vector3 Vector3_0 = new Vector3(1f, 0f, 0f);

	[CompilerGenerated]
	private Action<PatrolPointContainer> action_0;

	[NonSerialized]
	public const float Float_0 = 10000f;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public PatrolPointContainer PatrolPointContainer_0;

	[NonSerialized]
	[CompilerGenerated]
	public int? Nullable_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public bool Bool_1;

	public bool AssaultCanKill
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public bool SecutiryMovingClose
	{
		get
		{
			return _secutiryMovingClose;
		}
		set
		{
			_secutiryMovingClose = value;
		}
	}

	public float DistanceToEnemy
	{
		get
		{
			if (!BotOwner_0.Memory.HaveEnemy)
			{
				return 999f;
			}
			return BotOwner_0.Memory.GoalEnemy.Distance;
		}
	}

	public PatrolPointContainer LastPatrol
	{
		[CompilerGenerated]
		get
		{
			return PatrolPointContainer_0;
		}
		[CompilerGenerated]
		set
		{
			PatrolPointContainer_0 = value;
		}
	}

	public int? AreaId
	{
		[CompilerGenerated]
		get
		{
			return Nullable_0;
		}
		[CompilerGenerated]
		set
		{
			Nullable_0 = value;
		}
	}

	public event Action<PatrolPointContainer> OnPatrolChanged
	{
		[CompilerGenerated]
		add
		{
			Action<PatrolPointContainer> action = action_0;
			Action<PatrolPointContainer> action2;
			do
			{
				action2 = action;
				Action<PatrolPointContainer> value2 = (Action<PatrolPointContainer>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<PatrolPointContainer> action = action_0;
			Action<PatrolPointContainer> action2;
			do
			{
				action2 = action;
				Action<PatrolPointContainer> value2 = (Action<PatrolPointContainer>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass441(BotOwner owner, BotBoss bossLogic)
		: base(owner, bossLogic)
	{
		GClass441 gclass441_0 = this;
		BotOwner_0.PatrollingData.PointControl.OnTargetPatrolPointSetted += method_3;
		List<AIPlaceInfo> possibleAreas = BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.AIPlaceInfoHolder.Places.Where((AIPlaceInfo x) => x.InfoLogicAllEnemy is AIPlaceInfoLogicKolontay).ToList();
		if (possibleAreas.Count > 0)
		{
			int nearEntity = GClass369.GetNearEntity(possibleAreas.Count, (int i) => possibleAreas[i].transform.position - gclass441_0.BotOwner_0.Position);
			AreaId = possibleAreas[nearEntity].AreaId;
		}
		BotOwner_0.GetPlayer.BeingHitAction += method_2;
		BotOwner_0.WeaponManager.Melee.OnEnemyHitted += method_1;
		BotOwner_0.Memory.OnGoalEnemyChanged += method_0;
	}

	public void method_0(BotOwner obj)
	{
		if (Float_2 > Time.time)
		{
			return;
		}
		Float_2 = Time.time + 180f;
		foreach (BotOwner bot in BotOwner_0.BotsController.Bots.BotOwners)
		{
			if ((bot.Position - BotOwner_0.Position).sqrMagnitude < 10000f && bot.Settings.FileSettings.Mind.MAY_BE_CALLED_FOR_HELP)
			{
				BotOwner_0.AITaskManager.RegisterDelayedTask(BotOwner_0, 2f, delegate
				{
					bot.CalledData.TryCall(new CallEnemyTargetInfo(), BotOwner_0, helpOnlyWithEnemy: false);
				});
			}
		}
	}

	public void method_1(BotOwner arg1, Player person)
	{
		if (person.HealthController is ActiveHealthController activeHealthController)
		{
			activeHealthController.AddMisfireEffect(5f);
			activeHealthController.AddStaminaZeroffect(0.2f);
		}
	}

	public void method_2(DamageInfoStruct info, EBodyPart arg2, float arg3)
	{
		if (info.HaveOwner && info.Player.IsAI)
		{
			info.Player.AIData.BotOwner.ShootData.BlockFor(1f);
		}
	}

	public override void SetPatrolMode()
	{
		PatrolPointChooserBasic pointChooser = PatrollingData.GetPointChooser(BotOwner_0, PatrolMode.bossCoverScouts, BotOwner_0.SpawnProfileData);
		BotOwner_0.PatrollingData.SetMode(PatrolMode.bossCoverScouts, pointChooser);
	}

	public override void Activate()
	{
	}

	public override void BossLogicUpdate()
	{
	}

	public Vector3 GetOffset()
	{
		Vector3_0 = GClass855.Rotate90(Vector3_0, GClass855.SideTurn.right);
		return Vector3_0;
	}

	public void method_3(PatrolPointContainer point)
	{
		LastPatrol = point;
		action_0?.Invoke(point);
	}

	public override void Dispose()
	{
		if (!(BotOwner_0 == null))
		{
			if (BotOwner_0.GetPlayer != null)
			{
				BotOwner_0.GetPlayer.BeingHitAction -= method_2;
			}
			if (BotOwner_0.WeaponManager != null && BotOwner_0.WeaponManager.Melee != null)
			{
				BotOwner_0.WeaponManager.Melee.OnEnemyHitted -= method_1;
			}
			if (BotOwner_0.Memory != null)
			{
				BotOwner_0.Memory.OnGoalEnemyChanged -= method_0;
			}
			if (BotOwner_0.PatrollingData?.PointControl != null)
			{
				BotOwner_0.PatrollingData.PointControl.OnTargetPatrolPointSetted -= method_3;
			}
		}
	}

	public void MyAssaultCheckProtect(bool protectBoss)
	{
		if (protectBoss == Bool_1 || Float_1 > Time.time)
		{
			return;
		}
		Bool_1 = protectBoss;
		Float_1 = Time.time + 20f;
		for (int i = 0; i < BotOwner_0.BotsGroup.MembersCount; i++)
		{
			if (BotOwner_0.BotsGroup.Member(i).Brain.BaseBrain is GClass344 gClass)
			{
				gClass.SetProtect(Bool_1);
			}
		}
	}
}
