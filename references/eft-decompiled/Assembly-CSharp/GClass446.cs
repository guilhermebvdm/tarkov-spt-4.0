using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class GClass446 : GClass445<BossKojaniyBrainClass, GClass357>, GInterface6
{
	[Serializable]
	[CompilerGenerated]
	public class Class167
	{
		public static readonly Class167 class167_0 = new Class167();

		public static Func<AIPlaceInfo, bool> func_0;

		public static Func<AIPlaceInfo, bool> func_1;

		public static Func<PatrolWay, bool> func_2;

		public bool method_0(AIPlaceInfo x)
		{
			return x.InfoLogicAllEnemy is KojaniyTargetLoot;
		}

		public bool method_1(AIPlaceInfo x)
		{
			return x.name == "Lootable_kojaniy";
		}

		public bool method_2(PatrolWay x)
		{
			return x.PatrolType == PatrolType.boss;
		}
	}

	[CompilerGenerated]
	public class Class168
	{
		public KeyValuePair<IPlayer, EnemyInfo> enemyInfo;

		public GClass446 gclass446_0;

		public void method_0()
		{
			gclass446_0.BotOwner_0.BotsGroup.ReportAboutEnemy(enemyInfo.Key, EEnemyPartVisibleType.Visible, gclass446_0.BotOwner_0);
			enemyInfo.Value.SetVisible(value: true);
		}
	}

	[NonSerialized]
	public const float Float_0 = 40f;

	[NonSerialized]
	public const string String_0 = "Lootable_kojaniy";

	[NonSerialized]
	public const float Float_1 = 225f;

	[NonSerialized]
	public const float Float_2 = 100f;

	[NonSerialized]
	public const float Float_3 = 10f;

	[NonSerialized]
	public const float Float_4 = 3f;

	[CompilerGenerated]
	private Action action_0;

	[NonSerialized]
	public List<Vector3> List_1 = new List<Vector3>();

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public Vector3 Vector3_2;

	[NonSerialized]
	public Vector3 Vector3_3;

	[NonSerialized]
	public Vector3 Vector3_4;

	[NonSerialized]
	public float Float_7 = -200f;

	[NonSerialized]
	public Vector3 Vector3_5;

	[NonSerialized]
	public AIPlaceInfo AiplaceInfo_0;

	[CompilerGenerated]
	private Action action_1;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_6;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	public Vector3 LootPosition
	{
		[CompilerGenerated]
		get
		{
			return Vector3_6;
		}
		[CompilerGenerated]
		set
		{
			Vector3_6 = value;
		}
	}

	public bool ShallAttack
	{
		[CompilerGenerated]
		get
		{
			return Bool_1;
		}
		[CompilerGenerated]
		set
		{
			Bool_1 = value;
		}
	}

	public event Action OnThrowRequest
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

	public override event Action OnPositionsRecalculated
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

	[CanBeNull]
	public static Dictionary<BotOwner, Vector3> CalcOptimalPositions2(List<Vector3> positions, List<BotOwner> bots)
	{
		if (positions.Count != bots.Count)
		{
			return null;
		}
		Dictionary<BotOwner, Vector3> dictionary = new Dictionary<BotOwner, Vector3>();
		foreach (BotOwner bot in bots)
		{
			float num = float.MaxValue;
			Vector3 value = Vector3.zero;
			int num2 = -1;
			for (int i = 0; i < positions.Count; i++)
			{
				Vector3 vector = positions[i];
				float sqrMagnitude = (vector - bot.Position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					value = vector;
					num2 = i;
				}
			}
			if (num2 >= 0)
			{
				dictionary.Add(bot, value);
				positions.RemoveAt(num2);
			}
		}
		return dictionary;
	}

	public static Dictionary<BotOwner, Vector3> CalcOptimalPositions(List<Vector3> positions, List<BotOwner> bots)
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
				if (botOwner.BotState == EBotState.Active)
				{
					float magnitude = (botOwner.Position - vector).magnitude;
					num2 += magnitude;
					dictionary.Add(botOwner, vector);
				}
			}
			if (num2 < num)
			{
				result = dictionary;
				num = num2;
			}
		}
		return result;
	}

	public GClass446([NotNull] BotOwner owner, [NotNull] BotBoss bossLogic)
		: base(owner, bossLogic)
	{
		owner.Memory.OnGoalEnemyChanged += method_4;
		BotOwner_0.Memory.OnBulletNear += method_3;
		float x = GClass856.Random(-10f, 10f);
		float z = GClass856.Random(-10f, 10f);
		Vector3_5 = new Vector3(x, 0f, z);
	}

	public bool CanStartSuppress()
	{
		return Time.time - Float_7 > BotOwner_0.Settings.FileSettings.Boss.KOJANIY_START_NEXT_SUPPERS_SHOOTS_SEC;
	}

	public bool ISeeEnemy()
	{
		if (BotOwner_0.Memory.GoalEnemy != null)
		{
			return BotOwner_0.Memory.GoalEnemy.IsVisible;
		}
		return false;
	}

	public override Vector3 GetTargetToLook()
	{
		return Vector3_0;
	}

	public void SetWannaFight()
	{
		if (!ShallAttack)
		{
			ShallAttack = true;
		}
	}

	public bool IsRunning()
	{
		if (!(BotOwner_0 == null) && !BotOwner_0.IsDead)
		{
			if (BotOwner_0.Memory.IsInCover)
			{
				return false;
			}
			if (BotOwner_0.Mover.Sprinting)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public override void SetPatrolMode()
	{
		foreach (LootItemPositionClass item in Singleton<GameWorld>.Instance.AllLoot)
		{
			if (item.Id.Contains("Lootable_kojaniy"))
			{
				LootPosition = item.Position;
				Vector3_0 = LootPosition;
				break;
			}
		}
		AiplaceInfo_0 = BotOwner_0.BotsController.CoversData.AIPlaceInfoHolder.Places.FirstOrDefault((AIPlaceInfo x) => x.InfoLogicAllEnemy is KojaniyTargetLoot);
		if (AiplaceInfo_0 == null)
		{
			IEnumerable<AIPlaceInfo> allObjects = LocationScene.GetAllObjects<AIPlaceInfo>();
			AiplaceInfo_0 = allObjects.FirstOrDefault((AIPlaceInfo x) => x.name == "Lootable_kojaniy");
		}
		if (AiplaceInfo_0 == null)
		{
			return;
		}
		LootPosition = AiplaceInfo_0.transform.position;
		Vector3_0 = LootPosition;
		AiplaceInfo_0.OnPlayerEnter += method_2;
		Vector3_0 += new Vector3(0f, 3f, 0f);
		PatrolPointChooserBasic pointChooser = PatrollingData.GetPointChooser(BotOwner_0, PatrolMode.bossStayAtPlaces);
		BotOwner_0.PatrollingData.SetMode(PatrolMode.bossStayAtPlaces, pointChooser);
		foreach (PatrolWay item2 in BotOwner_0.BotsGroup.BotZone.PatrolWays.Where((PatrolWay x) => x.PatrolType == PatrolType.boss))
		{
			foreach (PatrolPoint point in item2.Points)
			{
				List_1.Add(point.Position);
			}
		}
		_ = List_1.Count;
		FindPositionsForFollowers(true);
		SetFollowersPositions();
	}

	public bool TooManyEnemies()
	{
		return BotOwner_0.BotsGroup.CurrentEnemies.ActiveEnemies > BotOwner_0.Settings.FileSettings.Boss.KOJANIY_SAFE_ENEMIES;
	}

	public override void Activate()
	{
		base.Activate();
	}

	public override void BossLogicUpdate()
	{
		if (Float_5 < Time.time)
		{
			Float_5 = Time.time + 5f;
			method_5();
			method_11();
			method_10();
		}
	}

	public void SupressRequestTaken()
	{
		Float_7 = Time.time;
	}

	public override void OnDrawGizmosSelected()
	{
		foreach (KeyValuePair<BotOwner, Vector3> item in Dictionary_0)
		{
			Vector3 vector = item.Value + Vector3.up;
			Gizmos.color = Color.yellow;
			Gizmos.DrawRay(item.Value, Vector3.up * 2f);
			Vector3 to = item.Key.Position + Vector3.up;
			Gizmos.color = Color.green;
			Gizmos.DrawLine(vector, to);
		}
		Gizmos.color = Color.blue;
		Gizmos.DrawRay(Vector3_0, Vector3.up * 20f);
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(Vector3_2, new Vector3(0.5f, 10f, 0.5f));
		Gizmos.DrawWireCube(Vector3_3, new Vector3(0.5f, 10f, 0.5f));
		Gizmos.color = Color.green;
		if (base.PointForBoss.HasValue)
		{
			Gizmos.DrawWireCube(base.PointForBoss.Value, new Vector3(0.5f, 10f, 0.5f));
		}
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(Vector3_4 + Vector3.up * 2f, new Vector3(0.5f, 10f, 0.5f));
		Gizmos.DrawWireCube(Vector3_4 + Vector3.up * 2.5f, new Vector3(0.5f, 10f, 0.5f));
		Gizmos.DrawWireCube(Vector3_4 + Vector3.up * 3f, new Vector3(0.5f, 10f, 0.5f));
	}

	public override void SetLogicToFollower(BossKojaniyBrainClass followerLogic)
	{
		followerLogic.SetBossKojaniy(this);
	}

	public override void FindPositionsForFollowers(bool anyway)
	{
		Vector3 vector = ((BotOwner_0.Memory.GoalEnemy == null) ? LootPosition : BotOwner_0.Memory.GoalEnemy.CurrPosition);
		if (!anyway && (vector - Vector3_4).sqrMagnitude < BotOwner_0.Settings.FileSettings.Boss.KOJANIY_DIST_CORE_SPOS_RECALC_SQRT)
		{
			return;
		}
		Vector3_4 = vector;
		float num = float.MaxValue;
		Vector3? vector2 = null;
		foreach (Vector3 item in List_1)
		{
			float sqrMagnitude = (item - BotOwner_0.Position).sqrMagnitude;
			float sqrMagnitude2 = (item - Vector3_4).sqrMagnitude;
			if (sqrMagnitude < num && (sqrMagnitude2 > 100f || !vector2.HasValue))
			{
				num = sqrMagnitude;
				vector2 = item;
			}
		}
		if (!vector2.HasValue)
		{
			return;
		}
		base.PointForBoss = vector2 + Vector3_5;
		if (Dictionary_0.Count == 0)
		{
			return;
		}
		Vector3 n = GClass855.NormalizeFastSelf(Vector3_4 - base.PointForBoss.Value);
		Vector3 vector3 = GClass855.Rotate90(n, GClass855.SideTurn.left);
		Vector3 vector4 = GClass855.Rotate90(n, GClass855.SideTurn.right);
		Vector3_2 = Vector3_4 + vector3 * 40f;
		Vector3_3 = Vector3_4 + vector4 * 40f;
		int count = Dictionary_0.Count;
		int num2 = count / 2;
		int count2 = num2;
		int count3 = num2;
		if (num2 * 2 != count)
		{
			count3 = num2 + 1;
		}
		List<Vector3> posiblePositions = List_1.ToList();
		List<Vector3> list = method_9(count2, posiblePositions, Vector3_2, Vector3_4);
		List<Vector3> collection = method_9(count3, posiblePositions, Vector3_3, Vector3_4);
		list.AddRange(collection);
		if (list.Count == 0)
		{
			return;
		}
		Dictionary<BotOwner, Vector3> dictionary = CalcOptimalPositions(list, BotOwner_0.Boss.Followers);
		if (dictionary == null)
		{
			return;
		}
		foreach (KeyValuePair<BotOwner, Vector3> item2 in dictionary)
		{
			Dictionary_0[item2.Key] = item2.Value;
		}
		action_1?.Invoke();
	}

	public void method_2(Player player)
	{
		if (player.AIData.IsAI)
		{
			return;
		}
		for (int i = 0; i < BotOwner_0.BotsGroup.MembersCount; i++)
		{
			foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in BotOwner_0.BotsGroup.Member(i).EnemiesController.EnemyInfos)
			{
				if (enemyInfo.Key.Id == player.Id)
				{
					BotOwner_0.AITaskManager.RegisterDelayedTask(BotOwner_0, 10f, delegate
					{
						BotOwner_0.BotsGroup.ReportAboutEnemy(enemyInfo.Key, EEnemyPartVisibleType.Visible, BotOwner_0);
						enemyInfo.Value.SetVisible(value: true);
					});
					BotOwner_0.AITaskManager.RegisterDelayedTask(BotOwner_0, 15f, delegate
					{
						BotOwner_0.BotsGroup.ReportAboutEnemy(enemyInfo.Key, EEnemyPartVisibleType.Visible, BotOwner_0);
						enemyInfo.Value.SetVisible(value: true);
					});
					return;
				}
			}
		}
	}

	public void method_3(BotOwner bot, IPlayer source)
	{
		SetWannaFight();
	}

	public void method_4(BotOwner obj)
	{
		method_8(anyway: true);
	}

	public void method_5()
	{
		if (!ShallAttack)
		{
			int activeEnemies = BotOwner_0.BotsGroup.CurrentEnemies.ActiveEnemies;
			bool flag = false;
			float num = BotOwner_0.Settings.FileSettings.Boss.KOJANIY_MIN_DIST_TO_LOOT_SQRT;
			if (activeEnemies > 1)
			{
				num *= BotOwner_0.Settings.FileSettings.Boss.KOJANIY_MANY_ENEMIES_COEF;
			}
			foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in BotOwner_0.EnemiesController.EnemyInfos)
			{
				if (enemyInfo.Value.Distance < BotOwner_0.Settings.FileSettings.Boss.KOJANIY_DIST_TO_BE_ENEMY && (enemyInfo.Value.CurrPosition - LootPosition).sqrMagnitude < num)
				{
					flag = true;
				}
			}
			if (flag)
			{
				SetWannaFight();
			}
			else
			{
				if (List_0.Count <= 0)
				{
					return;
				}
				bool flag2 = true;
				foreach (BossKojaniyBrainClass item in List_0)
				{
					if (!item.IAmReady)
					{
						flag2 = false;
						break;
					}
				}
				if (flag2)
				{
					flag2 = false;
					foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo2 in BotOwner_0.EnemiesController.EnemyInfos)
					{
						if ((enemyInfo2.Value.CurrPosition - LootPosition).magnitude < BotOwner_0.Settings.FileSettings.Boss.KOJANIY_DIST_WHEN_READY)
						{
							flag2 = true;
						}
					}
					if (flag2)
					{
						SetWannaFight();
						return;
					}
				}
				if (BotOwner_0.Memory.GoalEnemy == null || !(BotOwner_0.Memory.GoalEnemy.Distance < BotOwner_0.Settings.FileSettings.Boss.KOJANIY_DIST_ENEMY_TOO_CLOSE))
				{
					foreach (BotOwner follower in BotOwner_0.Boss.Followers)
					{
						if (follower.Memory.GoalEnemy != null && !(follower.Memory.GoalEnemy.Distance >= follower.Settings.FileSettings.Boss.KOJANIY_DIST_ENEMY_TOO_CLOSE))
						{
							SetWannaFight();
							break;
						}
					}
					return;
				}
				SetWannaFight();
			}
		}
		else
		{
			method_6();
		}
	}

	public void method_6()
	{
		if (BotOwner_0.EnemiesController.EnemyInfos.Count == 0)
		{
			method_7();
			return;
		}
		float num = BotOwner_0.Settings.FileSettings.Boss.KOJANIY_DIST_TO_BE_ENEMY * 1.1f;
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in BotOwner_0.EnemiesController.EnemyInfos)
		{
			if (!(enemyInfo.Value.Distance >= num))
			{
				return;
			}
		}
		method_7();
	}

	public void method_7()
	{
		ShallAttack = false;
	}

	public void method_8(bool anyway)
	{
		FindPositionsForFollowers(anyway);
		SetFollowersPositions();
	}

	public List<Vector3> method_9(int count, List<Vector3> posiblePositions, Vector3 closestTo, Vector3 enemyPos)
	{
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < count; i++)
		{
			Vector3? vector = null;
			float num = float.MaxValue;
			foreach (Vector3 posiblePosition in posiblePositions)
			{
				if ((posiblePosition - enemyPos).sqrMagnitude > 100f)
				{
					float sqrMagnitude = (posiblePosition - closestTo).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						vector = posiblePosition;
					}
				}
			}
			if (!vector.HasValue)
			{
				vector = GClass856.RandomElement(posiblePositions);
			}
			list.Add(vector.Value);
			posiblePositions.Remove(vector.Value);
		}
		return list;
	}

	public void method_10()
	{
		method_8(anyway: false);
	}

	public void method_11()
	{
		if (!(Float_6 < Time.time))
		{
			return;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && CanStartSuppress() && Time.time - goalEnemy.TimeLastSeen > BotOwner_0.Settings.FileSettings.Boss.KOJANIY_START_SUPPERS_SHOOTS_SEC)
		{
			Float_6 = Time.time + 30f;
			if (!TooManyEnemies() && action_0 != null)
			{
				action_0();
			}
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		action_0 = null;
		BotOwner_0.Memory.OnGoalEnemyChanged -= method_4;
		BotOwner_0.Memory.OnBulletNear -= method_3;
		if (AiplaceInfo_0 != null)
		{
			AiplaceInfo_0.OnPlayerEnter -= method_2;
		}
	}

	public bool IsNearLoot(EnemyInfo enemy)
	{
		if ((enemy.CurrPosition - Vector3_0).sqrMagnitude < 225f)
		{
			return true;
		}
		return false;
	}
}
