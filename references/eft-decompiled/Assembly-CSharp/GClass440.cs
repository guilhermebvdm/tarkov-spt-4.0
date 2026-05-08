using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using UnityEngine;

public class GClass440 : ABossLogic
{
	[CompilerGenerated]
	public class Class166
	{
		public int shootPosKey;

		public bool method_0(GClass333 x)
		{
			return x.Id == shootPosKey;
		}
	}

	[NonSerialized]
	public const float Float_0 = 160f;

	[NonSerialized]
	public const float Float_1 = 15f;

	[NonSerialized]
	public const string String_0 = "590c678286f77426c9660122";

	[NonSerialized]
	public const float Float_2 = 100f;

	[NonSerialized]
	public const float Float_3 = 60f;

	[NonSerialized]
	public const float Float_4 = 10f;

	public Dictionary<BotOwner, Vector3> FollowersPositions = new Dictionary<BotOwner, Vector3>();

	public Action OnThrowRequest;

	[NonSerialized]
	public List<MedsItemClass> List_0 = new List<MedsItemClass>();

	[NonSerialized]
	public List<GClass333> List_1 = new List<GClass333>();

	[NonSerialized]
	public BossKnightBrainClass BossKnightBrainClass;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public Vector3 Vector3_2;

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public Dictionary<int, bool> Dictionary_0 = new Dictionary<int, bool>();

	[NonSerialized]
	public float Float_8;

	[CompilerGenerated]
	private Action action_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	public bool EnoughtHaveGoodCovers
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

	public Vector3? PointForBoss => BotOwner_0.Position;

	public event Action OnPositionsRecalculated
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

	public GClass440([NotNull] BotOwner owner, [NotNull] BotBoss bossLogic)
		: base(owner, bossLogic)
	{
		EnoughtHaveGoodCovers = true;
		owner.Boss.OnFollowerStatusChange += method_8;
		owner.Memory.OnGoalEnemyChanged += method_7;
	}

	public bool CanStartUseStimulator()
	{
		return Time.time > Float_7;
	}

	public void method_0(GClass413 obj)
	{
		if (!(Time.time - Float_6 < 60f) && GClass856.IsTrue100(160f))
		{
			MedsItemClass medsItemClass = method_6();
			if (medsItemClass != null)
			{
				BotOwner_0.ItemDropper.SetItemToDropNext(medsItemClass);
				BotOwner_0.ItemDropper.TryDoDrop();
			}
		}
	}

	public void SomebodyUseStimulator()
	{
		Float_7 = Time.time + 10f;
	}

	public override void SetPatrolMode()
	{
		PatrolPointChooserBasic pointChooser = PatrollingData.GetPointChooser(BotOwner_0, PatrolMode.groupMoving, BotOwner_0.SpawnProfileData);
		BotOwner_0.PatrollingData.SetMode(PatrolMode.groupMoving, pointChooser);
	}

	public override void Activate()
	{
		Float_6 = Time.time;
		BotOwner_0.PeacefulActions.OnStartPeacefulMove += method_0;
		if (BotOwner_0.Brain.BaseBrain == null)
		{
			BotOwner_0.Brain.OnSetStrategy += method_1;
		}
		else
		{
			method_2();
		}
	}

	public void method_1(BaseBrain obj)
	{
		BotOwner_0.Brain.OnSetStrategy -= method_1;
		method_2();
	}

	public void method_2()
	{
		BossKnightBrainClass = BotOwner_0.Brain.BaseBrain as BossKnightBrainClass;
		if (BossKnightBrainClass == null)
		{
			Debug.LogError("Sanitar boss can't implement sanitar strategy");
		}
	}

	public override void BossLogicUpdate()
	{
		if (Float_5 < Time.time)
		{
			Float_5 = Time.time + 1f;
			method_12();
		}
	}

	public void WannHeal(BotOwner healTarget)
	{
		BotOwner_0.HealAnotherTarget.HealAsk(healTarget);
	}

	public void SetFightPosition(bool isGood, BotOwner bot)
	{
		if (isGood && !EnoughtHaveGoodCovers && Float_8 < Time.time)
		{
			Float_8 = Time.time + 3f;
			foreach (KeyValuePair<int, bool> item in method_4(Dictionary_0))
			{
				if (item.Key != bot.Id)
				{
					method_5(item.Key);
				}
			}
		}
		Dictionary_0[bot.Id] = isGood;
		EnoughtHaveGoodCovers = method_3();
	}

	public override void OnDrawGizmosSelected()
	{
		foreach (KeyValuePair<BotOwner, Vector3> followersPosition in FollowersPositions)
		{
			Vector3 vector = followersPosition.Value + Vector3.up;
			Gizmos.color = Color.yellow;
			Gizmos.DrawRay(followersPosition.Value, Vector3.up * 2f);
			Vector3 to = followersPosition.Key.Position + Vector3.up;
			Gizmos.color = Color.green;
			Gizmos.DrawLine(vector, to);
		}
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(Vector3_0, new Vector3(0.5f, 10f, 0.5f));
		Gizmos.DrawWireCube(Vector3_1, new Vector3(0.5f, 10f, 0.5f));
		Gizmos.color = Color.green;
		if (PointForBoss.HasValue)
		{
			Gizmos.DrawWireCube(PointForBoss.Value, new Vector3(0.5f, 10f, 0.5f));
		}
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(Vector3_2 + Vector3.up * 2f, new Vector3(0.5f, 10f, 0.5f));
		Gizmos.DrawWireCube(Vector3_2 + Vector3.up * 2.5f, new Vector3(0.5f, 10f, 0.5f));
		Gizmos.DrawWireCube(Vector3_2 + Vector3.up * 3f, new Vector3(0.5f, 10f, 0.5f));
	}

	public bool method_3()
	{
		if (Dictionary_0.Count > 2)
		{
			int num = 0;
			foreach (KeyValuePair<int, bool> item in Dictionary_0)
			{
				if (item.Value)
				{
					num++;
				}
			}
			return num >= 2;
		}
		return true;
	}

	public Dictionary<int, bool> method_4(Dictionary<int, bool> shooterPosGood)
	{
		Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
		foreach (KeyValuePair<int, bool> item in shooterPosGood)
		{
			dictionary.Add(item.Key, item.Value);
		}
		return dictionary;
	}

	public void method_5(int shootPosKey)
	{
		if (BossLogic.Id == shootPosKey)
		{
			BossKnightBrainClass?.ForceRecalcShootPos();
			return;
		}
		List_1.FirstOrDefault((GClass333 x) => x.Id == shootPosKey)?.ForceRecalcShootPos();
	}

	public MedsItemClass method_6()
	{
		Player getPlayer = BotOwner_0.GetPlayer;
		EquipmentSlot[] secureSlots = BotMedecine.secureSlots;
		List_0.Clear();
		getPlayer.InventoryController.GetAcceptableItemsNonAlloc(secureSlots, List_0);
		foreach (MedsItemClass item in List_0)
		{
			if (item is MedKitItemClass && item.TemplateId == (MongoID)"590c678286f77426c9660122")
			{
				return item;
			}
		}
		return null;
	}

	public void method_7(BotOwner obj)
	{
		method_10(anyway: true);
	}

	public void method_8(BotOwner botFollower, FollowerStatusChange status)
	{
	}

	[CanBeNull]
	public Dictionary<BotOwner, Vector3> method_9(List<Vector3> positions, List<BotOwner> bots)
	{
		if (positions.Count != bots.Count)
		{
			return null;
		}
		int count = positions.Count;
		float num = float.MaxValue;
		Dictionary<BotOwner, Vector3> result = new Dictionary<BotOwner, Vector3>();
		for (int i = 0; i < count; i++)
		{
			Dictionary<BotOwner, Vector3> dictionary = new Dictionary<BotOwner, Vector3>();
			float num2 = 0f;
			for (int j = 0; j < count; j++)
			{
				BotOwner botOwner = bots[j];
				int num3 = i + j;
				if (num3 >= count)
				{
					num3 -= count;
				}
				Vector3 vector = positions[num3];
				float magnitude = (botOwner.Position - vector).magnitude;
				num2 += magnitude;
				dictionary.Add(botOwner, vector);
			}
			if (num2 < num)
			{
				result = dictionary;
				num = num2;
			}
		}
		return result;
	}

	public void method_10(bool anyway)
	{
		method_11(anyway);
	}

	public void method_11(bool anyway)
	{
		Vector3 vector = ((BotOwner_0.Memory.GoalEnemy != null) ? BotOwner_0.Memory.GoalEnemy.CurrPosition : ((BotOwner_0.Memory.LastEnemy == null) ? BotOwner_0.PatrollingData.CurTargetPoint : BotOwner_0.Memory.LastEnemy.EnemyLastPositionReal));
		if (!anyway && (vector - Vector3_2).sqrMagnitude < BotOwner_0.Settings.FileSettings.Boss.KOJANIY_DIST_CORE_SPOS_RECALC_SQRT)
		{
			return;
		}
		Vector3_2 = vector;
		if ((BotOwner_0.Position - Vector3_2).sqrMagnitude > 100f || FollowersPositions.Count == 0)
		{
			return;
		}
		Vector3 n = GClass855.NormalizeFastSelf(Vector3_2 - PointForBoss.Value);
		Vector3 vector2 = GClass855.Rotate90(n, GClass855.SideTurn.left);
		Vector3 vector3 = GClass855.Rotate90(n, GClass855.SideTurn.right);
		Vector3_0 = Vector3_2 + vector2 * 15f;
		Vector3_1 = Vector3_2 + vector3 * 15f;
		List<Vector3> list = new List<Vector3>();
		list.Add(Vector3_0);
		list.Add(Vector3_1);
		if (list.Count == 0)
		{
			return;
		}
		Dictionary<BotOwner, Vector3> dictionary = method_9(list, BotOwner_0.Boss.Followers);
		if (dictionary == null)
		{
			return;
		}
		foreach (KeyValuePair<BotOwner, Vector3> item in dictionary)
		{
			FollowersPositions[item.Key] = item.Value;
		}
		if (action_0 != null)
		{
			action_0();
		}
	}

	public void method_12()
	{
		method_10(anyway: false);
	}

	public override void Dispose()
	{
		OnThrowRequest = null;
		if (BotOwner_0.Brain != null)
		{
			BotOwner_0.Brain.OnSetStrategy -= method_1;
		}
		if (BotOwner_0.Boss != null)
		{
			BotOwner_0.Boss.OnFollowerStatusChange -= method_8;
		}
		if (BotOwner_0.PeacefulActions != null)
		{
			BotOwner_0.PeacefulActions.OnStartPeacefulMove -= method_0;
		}
		if (BotOwner_0.Memory != null)
		{
			BotOwner_0.Memory.OnGoalEnemyChanged -= method_7;
		}
	}
}
