using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT;
using UnityEngine;

public class GClass448 : GClass445<GClass360, GClass357>
{
	[Serializable]
	[CompilerGenerated]
	public class Class174
	{
		public static readonly Class174 class174_0 = new Class174();

		public static Func<BotOwner, bool> func_0;

		public bool method_0(BotOwner x)
		{
			return x.HealthController.IsAlive;
		}
	}

	[CompilerGenerated]
	public class Class175
	{
		public Player arg1Player;

		public bool method_0(BotOwner x)
		{
			return x.Id == arg1Player.Id;
		}
	}

	[CompilerGenerated]
	public class Class176
	{
		public Vector3 pos;

		public float method_0(BotOwner x)
		{
			return (x.Position - pos).sqrMagnitude;
		}
	}

	[NonSerialized]
	public const float Float_0 = 14f;

	[NonSerialized]
	public const float Float_1 = 33f;

	[NonSerialized]
	public const float Float_2 = 30f;

	[NonSerialized]
	public const float Float_3 = 8f;

	[NonSerialized]
	public const float Float_4 = 8f;

	[NonSerialized]
	public const float Float_5 = 1f;

	[NonSerialized]
	public const float Float_6 = 15f;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public Dictionary<int, float> Dictionary_2 = new Dictionary<int, float>();

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	public float Float_9;

	[CompilerGenerated]
	private Action action_0;

	[CompilerGenerated]
	private Action action_1;

	public BotOwner Owner => BotOwner_0;

	public override event Action OnPositionsRecalculated
	{
		[CompilerGenerated]
		add
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnRemoveFollower
	{
		[CompilerGenerated]
		add
		{
			Action action = action_1;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_1;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass448(BotOwner owner, BotBoss bossLogic)
		: base(owner, bossLogic)
	{
		Vector3_0 = new Vector3(GClass856.Random(-14f, 14f), 0f, GClass856.Random(-14f, 14f));
	}

	public void StartAttackWithKnife(BotOwner hitter, Vector3 enemyPos)
	{
		BotOwner botOwner = null;
		foreach (KeyValuePair<BotOwner, Vector3> item in FollowersPositions())
		{
			BotOwner key = item.Key;
			if (key.Id != hitter.Id && !((key.Position - enemyPos).sqrMagnitude <= 900f))
			{
				botOwner = key;
				break;
			}
		}
		if (botOwner != null)
		{
			botOwner.BotTalk.Say(EPhraseTrigger.CheckHim);
		}
		BotOwner_0.Gesture.TryGestus(EInteraction.OkGesture, withAimingDelay: false);
	}

	public void StartRunAway()
	{
		BotOwner_0.Gesture.TryGestus(EInteraction.ComeWithMeGesture, withAimingDelay: false);
	}

	public void StartHitPLayer(Player trg)
	{
		int id = trg.Id;
		if (Dictionary_2.ContainsKey(id))
		{
			Dictionary_2[id] = Time.time;
		}
		else
		{
			Dictionary_2.Add(id, Time.time);
		}
	}

	public override Vector3 GetTargetToLook()
	{
		return BotOwner_0.Position;
	}

	public override void SetPatrolMode()
	{
		PatrolPointChooserBasic pointChooser = PatrollingData.GetPointChooser(BotOwner_0, PatrolMode.bossStayAtPlaces);
		BotOwner_0.PatrollingData.SetMode(PatrolMode.bossStayAtPlaces, pointChooser);
		FindPositionsForFollowers(true);
		SetFollowersPositions();
		method_3();
	}

	public override void BossLogicUpdate()
	{
		if (Float_7 < Time.time)
		{
			Float_7 = Time.time + 5f;
			method_7();
			method_5();
		}
	}

	public void FollowerUnderAttack(BotOwner attackedWarriod)
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return;
		}
		bool flag = goalEnemy.HaveNightVision();
		BotOwner_0.Gesture.TryGestus(EInteraction.ThereGesture, withAimingDelay: false);
		float num = float.MaxValue;
		GClass359 gClass = null;
		foreach (GClass360 item in List_0)
		{
			if (item != null && item.Bot.Id != attackedWarriod.Id)
			{
				float sqrMagnitude = (item.Bot.Position - goalEnemy.CurrPosition).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					gClass = item;
				}
			}
		}
		if (gClass == null)
		{
			return;
		}
		gClass.SetAttackWithDelay(1f);
		foreach (GClass360 item2 in List_0)
		{
			if (item2 != null && item2.Bot.Id != attackedWarriod.Id && item2.Bot.Id != gClass.Bot.Id)
			{
				float attackWithDelay = (flag ? 0f : GClass856.Random(1f, 8f));
				item2.SetAttackWithDelay(attackWithDelay);
			}
		}
	}

	public void TriggerPressedBy(BotOwner owner)
	{
		foreach (GClass360 item in List_0)
		{
			if (item.Bot.Id != owner.Id && item.IsCurShootAndRun())
			{
				item.SetAttackWithDelay(1f);
			}
		}
	}

	public void AllFollowersDoAttack()
	{
		BotOwner_0.Gesture.TryGestus(EInteraction.ThereGesture, withAimingDelay: false);
		foreach (GClass360 item in List_0)
		{
			float attackWithDelay = GClass856.Random(0.5f, 1f);
			item.SetAttackWithDelay(attackWithDelay);
		}
	}

	public bool HaveFollower(Player arg1Player)
	{
		return BossLogic.Followers.FirstOrDefault((BotOwner x) => x.Id == arg1Player.Id) != null;
	}

	public bool ShallAttack(Player arg1Player)
	{
		if (arg1Player.IsAI)
		{
			return false;
		}
		return true;
	}

	public bool CanHit(Player trg)
	{
		int id = trg.Id;
		if (Dictionary_2.TryGetValue(id, out var value))
		{
			return Time.time - value > 8f;
		}
		return true;
	}

	public void FollowerTalkCheck()
	{
		BotOwner botOwner = method_6();
		if (botOwner != null)
		{
			botOwner.BotTalk.Say(EPhraseTrigger.CheckHim);
		}
	}

	public void ChangeToMeleeFromSUpport()
	{
		Float_9 = Time.time;
	}

	public bool CanChangeToMeleeFromSupport()
	{
		if (Time.time - Float_9 > 15f)
		{
			return true;
		}
		return false;
	}

	public override void SetLogicToFollower(GClass360 followerLogic)
	{
		followerLogic.SetBoss(this);
	}

	public override void SubRemoveFollower(BotOwner botOwner)
	{
		botOwner.HealthController.DiedEvent -= method_2;
	}

	public override void SubAddFollower(BotOwner botOwner)
	{
		botOwner.HealthController.DiedEvent += method_2;
		botOwner.WeaponManager.Grenades.OnGrenadeThrowComplete += method_4;
	}

	public override void FindPositionsForFollowers(bool anyway)
	{
		method_3();
		int count = BotOwner_0.Boss.Followers.Count;
		List<Vector3> list = new List<Vector3>(count);
		float num = 360f / (float)count;
		Vector3 vector = ((BotOwner_0.Memory.CurCustomCoverPoint == null) ? (BotOwner_0.Position + Vector3_0) : (BotOwner_0.Memory.CurCustomCoverPoint.Position + Vector3_0));
		for (int i = 0; i < count; i++)
		{
			Vector3 vector2 = GClass855.RotateOnAngUp(Vector3.right, num * (float)i);
			Vector3 item = vector + vector2 * 33f;
			list.Add(item);
		}
		Dictionary<BotOwner, Vector3> dictionary = GClass446.CalcOptimalPositions(list, BotOwner_0.Boss.Followers);
		if (dictionary == null)
		{
			return;
		}
		foreach (KeyValuePair<BotOwner, Vector3> item2 in dictionary)
		{
			Dictionary_0[item2.Key] = item2.Value;
		}
		action_0?.Invoke();
	}

	public void method_2(EDamageType damageType)
	{
		AllFollowersDoAttack();
	}

	public void method_3()
	{
		if (BotOwner_0.PatrollingData.CurPatrolPoint != null)
		{
			base.PointForBoss = BotOwner_0.PatrollingData.CurPatrolPoint.Position + Vector3_0;
		}
	}

	public void method_4(ThrowWeapItemClass obj)
	{
		BotOwner_0.Gesture.TryGestus(EInteraction.HoldGesture, withAimingDelay: false);
	}

	public void method_5()
	{
		if (BotOwner_0.Memory.GoalEnemy == null)
		{
			return;
		}
		if (BotOwner_0.Memory.GoalEnemy.Distance < 30f)
		{
			if (!Bool_1 && Float_8 < Time.time)
			{
				Float_8 = Time.time + 20f;
				FollowerTalkCheck();
			}
			Bool_1 = true;
		}
		else
		{
			Bool_1 = false;
		}
	}

	public BotOwner method_6()
	{
		if (BotOwner_0.Memory.GoalEnemy != null && BotOwner_0.Memory.GoalEnemy.Person.HealthController.IsAlive)
		{
			Vector3 pos = BotOwner_0.Memory.GoalEnemy.CurrPosition;
			List<BotOwner> list = (from x in BossLogic.Followers
				where x.HealthController.IsAlive
				orderby (x.Position - pos).sqrMagnitude
				select x).ToList();
			if (list.Count > 1)
			{
				return list[1];
			}
			return null;
		}
		return null;
	}

	public void method_7()
	{
		method_8();
		FindPositionsForFollowers(false);
		SetFollowersPositions();
	}

	public void method_8()
	{
		CustomNavigationPoint curCustomCoverPoint = BotOwner_0.Memory.CurCustomCoverPoint;
		if (BotOwner_0.Memory.GoalEnemy != null || curCustomCoverPoint == null || curCustomCoverPoint.PlaceId <= 0)
		{
			return;
		}
		foreach (KeyValuePair<BotOwner, Vector3> item in Dictionary_0)
		{
			BotOwner key = item.Key;
			if (key.Memory.CurCustomCoverPoint != null && key.Memory.CurCustomCoverPoint.PlaceId != curCustomCoverPoint.PlaceId)
			{
				key.Memory.Spotted(byHit: false);
			}
		}
	}
}
