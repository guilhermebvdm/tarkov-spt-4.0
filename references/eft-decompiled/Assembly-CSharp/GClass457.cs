using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class GClass457 : GClass456
{
	public const string COMEZONE_TO_REINFORSMENTS_ID = "TrainZone";

	public const string EVENT_TO_REINFORSMENTS_ID = "455";

	public const int EXIT_TO_REINFORSMENTS_ID = 435;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public Dictionary<WildSpawnType, List<BotOwner>> Dictionary_0 = new Dictionary<WildSpawnType, List<BotOwner>>
	{
		{
			WildSpawnType.followerGluharSecurity,
			new List<BotOwner>()
		},
		{
			WildSpawnType.followerGluharAssault,
			new List<BotOwner>()
		},
		{
			WildSpawnType.followerGluharScout,
			new List<BotOwner>()
		},
		{
			WildSpawnType.followerGluharSnipe,
			new List<BotOwner>()
		}
	};

	[NonSerialized]
	public Dictionary<WildSpawnType, int> Dictionary_1;

	public int SECURITY => BotOwner_0.Settings.FileSettings.Boss.GLUHAR_FOLLOWERS_SECURITY;

	public int ASSAULT => BotOwner_0.Settings.FileSettings.Boss.GLUHAR_FOLLOWERS_ASSAULT;

	public int SCOUT => BotOwner_0.Settings.FileSettings.Boss.GLUHAR_FOLLOWERS_SCOUT;

	public int SNIPE => BotOwner_0.Settings.FileSettings.Boss.GLUHAR_FOLLOWERS_SNIPE;

	public override int TargetFollowersCount => SECURITY + ASSAULT + SCOUT + SNIPE;

	public GClass457([NotNull] BotOwner owner)
		: base(owner)
	{
		Dictionary_1 = new Dictionary<WildSpawnType, int>
		{
			{
				WildSpawnType.followerGluharSecurity,
				SECURITY
			},
			{
				WildSpawnType.followerGluharAssault,
				ASSAULT
			},
			{
				WildSpawnType.followerGluharScout,
				SCOUT
			},
			{
				WildSpawnType.followerGluharSnipe,
				SNIPE
			}
		};
	}

	public override void Activate()
	{
		if (BotOwner_0.Settings.FileSettings.Boss.GLUHAR_SEC_TO_REINFORSMENTS > 0f)
		{
			StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromMilliseconds(BotOwner_0.Settings.FileSettings.Boss.GLUHAR_SEC_TO_REINFORSMENTS)).OnTimer += method_3;
		}
		BotEventHandler instance = Singleton<BotEventHandler>.Instance;
		if (instance != null)
		{
			if (BotOwner_0.Settings.FileSettings.Boss.GLUHAR_REINFORSMENTS_BY_EXIT)
			{
				instance.OnExitActivated += method_2;
			}
			if (BotOwner_0.Settings.FileSettings.Boss.GLUHAR_REINFORSMENTS_BY_EVENT)
			{
				instance.OnEvent += method_1;
			}
			if (BotOwner_0.Settings.FileSettings.Boss.GLUHAR_REINFORSMENTS_BY_PLAYER_COME_TO_ZONE)
			{
				instance.OnPlayerComeToPlace += method_0;
			}
		}
	}

	public override void AddFollower(BotOwner botFollower)
	{
		WildSpawnType role = botFollower.Profile.Info.Settings.Role;
		if ((uint)(role - 12) <= 3u)
		{
			List<BotOwner> list = Dictionary_0[role];
			int num = Dictionary_1[role];
			bool flag = false;
			if (list.Count < num)
			{
				list.Add(botFollower);
				List_0.Add(botFollower);
				flag = true;
			}
			else
			{
				foreach (KeyValuePair<WildSpawnType, List<BotOwner>> item in Dictionary_0)
				{
					List<BotOwner> value = item.Value;
					int num2 = Dictionary_1[item.Key];
					if (value.Count < num2)
					{
						value.Add(botFollower);
						List_0.Add(botFollower);
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				Debug.LogError("Gluhar wrong add follower. 1No free place. " + role.ToString() + "   cur:" + Dictionary_0[role].Count + "   target:" + Dictionary_1[role]);
			}
		}
		else
		{
			bool flag2 = false;
			foreach (KeyValuePair<WildSpawnType, List<BotOwner>> item2 in Dictionary_0)
			{
				List<BotOwner> value2 = item2.Value;
				int num3 = Dictionary_1[item2.Key];
				if (value2.Count < num3)
				{
					value2.Add(botFollower);
					List_0.Add(botFollower);
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				Debug.LogError("Gluhar wrong add follower. 2No free place." + role);
			}
		}
		if (!Bool_1)
		{
			int count = List_0.Count;
			Bool_1 = count >= TargetFollowersCount;
		}
	}

	public override List<string> DebugInfo()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<WildSpawnType, List<BotOwner>> item in Dictionary_0)
		{
			list.Add(item.Key.ToString() + ":: " + item.Value.Count + "/" + Dictionary_1[item.Key]);
		}
		return list;
	}

	public override void Remove(BotOwner botFollower)
	{
		if (List_0.Remove(botFollower))
		{
			using Dictionary<WildSpawnType, List<BotOwner>>.Enumerator enumerator = Dictionary_0.GetEnumerator();
			while (enumerator.MoveNext() && !enumerator.Current.Value.Remove(botFollower))
			{
			}
		}
		if (Bool_0)
		{
			return;
		}
		int num = 0;
		foreach (KeyValuePair<WildSpawnType, List<BotOwner>> item in Dictionary_0)
		{
			num += item.Value.Count;
		}
		if (num <= BotOwner_0.Settings.FileSettings.Boss.GLUHAR_FOLLOWERS_TO_REINFORSMENTS)
		{
			Bool_0 = true;
			method_4();
		}
	}

	public override void CheckFollowers()
	{
		if (Float_3 < Time.time)
		{
			Float_3 = Time.time + 0.97f;
			BotOwner_0.BotsGroup.BotGroupWarnData.UpdateDistToScavsPlayersAndWarnIntruders();
		}
	}

	public override void Clear()
	{
		List_0.Clear();
		foreach (KeyValuePair<WildSpawnType, List<BotOwner>> item in Dictionary_0)
		{
			item.Value.Clear();
		}
	}

	public void method_0(string obj)
	{
		if (obj == "TrainZone")
		{
			Singleton<BotEventHandler>.Instance.OnPlayerComeToPlace -= method_0;
			method_4();
		}
	}

	public void method_1(string obj)
	{
		if (obj == "455")
		{
			Singleton<BotEventHandler>.Instance.OnEvent -= method_1;
			method_4();
		}
	}

	public void method_2(int obj)
	{
		if (obj == 435)
		{
			Singleton<BotEventHandler>.Instance.OnExitActivated -= method_2;
			method_4();
		}
	}

	public void method_3()
	{
		method_4();
	}

	public void method_4()
	{
		if (!Bool_1)
		{
			return;
		}
		WildSpawnType? wildSpawnType = null;
		int botsCount = 0;
		foreach (KeyValuePair<WildSpawnType, int> item in Dictionary_1)
		{
			int count = Dictionary_0[item.Key].Count;
			if (count < item.Value)
			{
				botsCount = item.Value - count;
				wildSpawnType = item.Key;
				break;
			}
		}
		if (wildSpawnType.HasValue)
		{
			float time = Time.time;
			BotWaveDataClass wave = new BotWaveDataClass
			{
				BotsCount = botsCount,
				Time = time,
				Difficulty = BotDifficulty.normal,
				IsPlayers = false,
				Side = EPlayerSide.Savage,
				SpawnAreaName = BotOwner_0.BotsGroup.BotZone.NameZone,
				WildSpawnType = wildSpawnType.Value
			};
			BotOwner_0.BotsGroup.BotGame.BotsController.ActivateBotsByWave(wave).HandleExceptions();
		}
	}

	public override void Dispose()
	{
		BotEventHandler instance = Singleton<BotEventHandler>.Instance;
		if (instance != null)
		{
			instance.OnExitActivated -= method_2;
			instance.OnEvent -= method_1;
			instance.OnPlayerComeToPlace -= method_0;
		}
	}
}
