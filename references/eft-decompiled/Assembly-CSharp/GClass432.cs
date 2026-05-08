using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using UnityEngine;

public class GClass432(BotOwner owner, BotBoss bossLogic) : ABossLogic(owner, bossLogic)
{
	[Serializable]
	[CompilerGenerated]
	public class Class165
	{
		public static readonly Class165 class165_0 = new Class165();

		public static Func<BotOwner, bool> func_0;

		public static Func<BotSettingsClass, float> func_1;

		public static Func<PlaceForCheck, bool> func_2;

		public static Func<PlaceForCheck, float> func_3;

		public bool method_0(BotOwner x)
		{
			return x.WeaponManager.Grenades.HaveGrenade;
		}

		public float method_1(BotSettingsClass x)
		{
			return x.EnemyLastSeenTimeSense;
		}

		public bool method_2(PlaceForCheck x)
		{
			return x.Reacheble;
		}

		public float method_3(PlaceForCheck x)
		{
			return x.CreatedTime;
		}
	}

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public bool Bool_0 = true;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public int Int_1;

	public override void SetPatrolMode()
	{
		PatrolPointChooserBasic pointChooser = PatrollingData.GetPointChooser(BotOwner_0, PatrolMode.bossCoverScouts, BotOwner_0.SpawnProfileData);
		BotOwner_0.PatrollingData.SetMode(PatrolMode.bossCoverScouts, pointChooser);
	}

	public override void Activate()
	{
		BotOwner_0.Boss.OnFollowerStatusChange += method_0;
	}

	public override void BossLogicUpdate()
	{
		if (BotOwner_0.Memory.IsPeace || !(Float_1 < Time.time))
		{
			return;
		}
		Float_1 = Time.time + 1f;
		if (BotOwner_0.Memory.LastEnemy == null && BotOwner_0.Memory.GoalEnemy == null && !BotOwner_0.Settings.FileSettings.Boss.COVER_TO_SEND)
		{
			return;
		}
		float num = Time.time - BotOwner_0.BotsGroup.EnemyLastSeenTimeSence;
		bool flag = num < BotOwner_0.Settings.FileSettings.Boss.TIME_AFTER_LOSE_DELTA && BotOwner_0.BotsGroup.EnemyLastSeenTimeSence > 1f;
		bool flag2 = num > BotOwner_0.Settings.FileSettings.Boss.TIME_AFTER_LOSE;
		bool flag3 = num > BotOwner_0.Settings.FileSettings.Boss.TIME_AFTER_LOSE + 7f;
		if (!(flag2 || flag3))
		{
			return;
		}
		bool flag4 = method_1();
		bool flag5 = false;
		flag5 = (flag2 && flag4 && flag) || (flag3 && flag);
		if (Bool_0)
		{
			if ((BotOwner_0.BotsGroup.EnemyLastSeenPositionSence - BotOwner_0.Position).sqrMagnitude < BotOwner_0.Settings.FileSettings.Boss.MAX_DIST_DECIDER_TO_SEND_SQRT && flag5)
			{
				method_2();
				Float_0 = Time.time + BotOwner_0.Settings.FileSettings.Boss.DELTA_SEARCH_TIME;
				Bool_0 = false;
			}
		}
		else
		{
			Bool_0 = Float_0 < Time.time;
		}
	}

	public void method_0(BotOwner arg1, FollowerStatusChange status)
	{
		if (status != FollowerStatusChange.Remove)
		{
			return;
		}
		Int_1++;
		if (Int_1 >= 2)
		{
			for (int i = 0; i < BossLogic.Followers.Count; i++)
			{
				BossLogic.Followers[i].Memory.Spotted(byHit: false);
			}
		}
	}

	public bool method_1()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			int count = BossLogic.Followers.Count;
			int num = 0;
			while (true)
			{
				if (num < count)
				{
					if (!BossLogic.Followers[num].Memory.IsInCover)
					{
						break;
					}
					num++;
					continue;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public void method_2()
	{
		if (BossLogic.Followers.Count < BotOwner_0.Settings.FileSettings.Boss.PERSONS_SEND)
		{
			return;
		}
		Int_0 = BotOwner_0.Settings.FileSettings.Boss.PERSONS_SEND;
		List<Vector3> list = method_5(BotOwner_0.Settings.FileSettings.Boss.PERSONS_SEND);
		int num = Mathf.Min(BotOwner_0.Settings.FileSettings.Boss.PERSONS_SEND, list.Count);
		if (num != 0)
		{
			bool flag = BossLogic.Followers.Any((BotOwner x) => x.WeaponManager.Grenades.HaveGrenade) || BotOwner_0.WeaponManager.Grenades.HaveGrenade;
			IList<BotOwner> list2 = GClass856.RandomElement(BossLogic.Followers, num);
			int num2 = 0;
			bool flag2 = false;
			{
				foreach (BotOwner item in list2)
				{
					_ = item;
					if (flag2)
					{
						break;
					}
					if (GClass856.IsTrue100(BotOwner_0.Settings.FileSettings.Boss.CHANCE_TO_SEND_GRENADE_100) && BotOwner_0.Memory.LastEnemy != null && flag && BotOwner_0.BotRequestController.TryActivateThrowGrenadeRequest(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(BotOwner_0.Memory.LastEnemy.Person.ProfileId), onlyCached: true))
					{
						flag2 = true;
					}
					if (!flag2)
					{
						Vector3 vector = list[num2];
						if (BotOwner_0.BotsGroup.IsInSmoke(vector))
						{
							break;
						}
						BotOwner_0.BotRequestController.TryActivateGoToPointRequest(vector, method_3, method_6);
					}
					num2++;
				}
				return;
			}
		}
		Debug.LogError("wrong count places for check");
	}

	public void method_3()
	{
		Int_0--;
		method_4();
	}

	public void method_4()
	{
		if (Int_0 <= 0 && BotOwner_0.Memory.GoalEnemy == null && BotOwner_0.Memory.LastEnemy != null)
		{
			Vector3 currPosition = BotOwner_0.Memory.LastEnemy.CurrPosition;
			BotOwner_0.BotRequestController.TryActivateThrowGrenadeRequest(currPosition, null, out var _);
		}
	}

	public List<Vector3> method_5(int needElements)
	{
		int num = needElements;
		List<Vector3> list = new List<Vector3>();
		BotSettingsClass[] array = BotOwner_0.BotsGroup.Enemies.Values.OrderByDescending((BotSettingsClass x) => x.EnemyLastSeenTimeSense).ToArray();
		int num2 = Mathf.Min(array.Length, needElements);
		for (int num3 = 0; num3 < num2; num3++)
		{
			BotSettingsClass botSettingsClass = array[num3];
			if (botSettingsClass.EnemyLastSeenTimeSense > 0f)
			{
				list.Add(botSettingsClass.EnemyLastPosition);
			}
		}
		if (list.Count >= needElements)
		{
			return list;
		}
		needElements -= list.Count;
		PlaceForCheck[] array2 = (from x in BotOwner_0.BotsGroup.PlacesForCheck
			where x.Reacheble
			orderby x.CreatedTime descending
			select x).ToArray();
		int num4 = Mathf.Min(needElements, array2.Length);
		for (int num5 = 0; num5 < num4; num5++)
		{
			list.Add(array2[num5].Position);
		}
		if (list.Count < num && list.Count != 0)
		{
			needElements -= list.Count;
			for (int num6 = 0; num6 < needElements; num6++)
			{
				list.Add(list[0]);
			}
			return list;
		}
		return list;
	}

	public override void Dispose()
	{
		if (BotOwner_0.Boss != null)
		{
			BotOwner_0.Boss.OnFollowerStatusChange -= method_0;
		}
	}

	public void method_6()
	{
		Int_0--;
		method_4();
	}
}
