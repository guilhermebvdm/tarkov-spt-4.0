using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Comfort.Common;
using EFT;
using UnityEngine;

[Serializable]
public class BossLocationSpawn
{
	[CompilerGenerated]
	public class Class287
	{
		public Vector3 closestsPos;

		public int method_0(BotZone x, BotZone y)
		{
			float sqrMagnitude = (x.CenterOfSpawnPoints - closestsPos).sqrMagnitude;
			float sqrMagnitude2 = (y.CenterOfSpawnPoints - closestsPos).sqrMagnitude;
			if (sqrMagnitude < sqrMagnitude2)
			{
				return -1;
			}
			if (sqrMagnitude > sqrMagnitude2)
			{
				return 1;
			}
			return 0;
		}
	}

	public string BossName = "";

	public float BossChance = -1f;

	public string BossZone = "";

	public bool BossPlayer;

	public string BossDifficult;

	public string BossEscortDifficult;

	public string BossEscortType = "";

	public string BossEscortAmount = "";

	public float Time;

	public float Delay;

	public string TriggerId = "";

	public string TriggerName = "";

	public bool IgnoreMaxBots;

	public bool ForceSpawn;

	public bool DependKarma;

	public Vector3? PerfectPos;

	public WildSpawnSupports[] Supports;

	public bool ShowOnTarkovMap;

	public bool ShowOnTarkovMapPvE;

	[NonSerialized]
	public List<BossLocationSpawnSubData> SubDatas;

	[NonSerialized]
	public const float MIN_KARMA_TO_RECALC = 0.5f;

	[NonSerialized]
	public List<BotZone> PossibleShuffledZones = new List<BotZone>();

	[field: NonSerialized]
	public WildSpawnType BossType { get; set; }

	[field: NonSerialized]
	public WildSpawnType EscortType { get; set; }

	[field: NonSerialized]
	public int EscortCount { get; set; }

	[field: NonSerialized]
	public string BornZone { get; set; }

	[field: NonSerialized]
	public bool ShallSpawn { get; set; }

	[field: NonSerialized]
	public BotDifficulty BossDif { get; set; } = BotDifficulty.normal;

	[field: NonSerialized]
	public BotDifficulty EscortDif { get; set; } = BotDifficulty.normal;

	[field: NonSerialized]
	public SpawnTriggerType TriggerType { get; set; }

	[field: NonSerialized]
	public bool Activated { get; set; }

	public bool IsStartWave()
	{
		if (Time < 0f)
		{
			return TriggerType == SpawnTriggerType.none;
		}
		return false;
	}

	public void ParseMainTypesTypes()
	{
		CalculateChance();
		Activated = false;
		method_4();
		method_3();
		method_2();
		method_1();
		method_0();
	}

	public void CalculateChance()
	{
		ShallSpawn = GClass856.IsTrue100(BossChance);
	}

	public void method_0()
	{
		EscortDif = (BotDifficulty)Enum.Parse(typeof(BotDifficulty), BossEscortDifficult);
	}

	public void method_1()
	{
		BossDif = (BotDifficulty)Enum.Parse(typeof(BotDifficulty), BossDifficult);
	}

	public void method_2()
	{
		EscortType = (WildSpawnType)Enum.Parse(typeof(WildSpawnType), BossEscortType);
	}

	public void method_3()
	{
		BossType = (WildSpawnType)Enum.Parse(typeof(WildSpawnType), BossName);
	}

	public void method_4()
	{
		if (!string.IsNullOrEmpty(TriggerName))
		{
			TriggerType = (SpawnTriggerType)Enum.Parse(typeof(SpawnTriggerType), TriggerName);
		}
	}

	public void Init()
	{
		ShallSpawn = GClass856.IsTrue100(BossChance);
		Activated = false;
		ParseMainTypesTypes();
		string[] array = BossEscortAmount.Split(',');
		List<int> list = new List<int>();
		try
		{
			string[] array2 = array;
			foreach (string s in array2)
			{
				list.Add(int.Parse(s));
			}
		}
		catch (Exception)
		{
			list.Add(0);
		}
		int num = GClass856.RandomElement(list);
		BornZone = GClass856.RandomElement(BossZone.Split(','));
		if (Supports != null && Supports.Length != 0)
		{
			SubDatas = new List<BossLocationSpawnSubData>();
			WildSpawnSupports[] supports = Supports;
			foreach (WildSpawnSupports wildSpawnSupports in supports)
			{
				BotDifficulty difficulty = (BotDifficulty)Enum.Parse(typeof(BotDifficulty), GClass856.RandomElement(wildSpawnSupports.BossEscortDifficult));
				SubDatas.Add(new BossLocationSpawnSubData(wildSpawnSupports.BossEscortAmount, wildSpawnSupports.BossEscortType, difficulty));
			}
		}
		if (SubDatas != null)
		{
			foreach (BossLocationSpawnSubData subData in SubDatas)
			{
				num += subData.BossEscortAmount;
			}
		}
		EscortCount = num;
	}

	public List<BossLocationSpawnSubData> GetEscors()
	{
		return SubDatas;
	}

	public BossLocationSpawn Copy()
	{
		BossLocationSpawn bossLocationSpawn = new BossLocationSpawn
		{
			BossName = BossName,
			BossChance = BossChance,
			BossZone = BossZone,
			BossPlayer = BossPlayer,
			BossDifficult = BossDifficult,
			BossEscortDifficult = BossEscortDifficult,
			BossEscortType = BossEscortType,
			BossEscortAmount = BossEscortAmount,
			TriggerName = TriggerName,
			TriggerId = TriggerId,
			Time = Time,
			Delay = Delay,
			IgnoreMaxBots = IgnoreMaxBots,
			ForceSpawn = ForceSpawn,
			DependKarma = DependKarma
		};
		if (Supports != null && Supports.Length != 0)
		{
			WildSpawnSupports[] array = new WildSpawnSupports[Supports.Length];
			for (int i = 0; i < Supports.Length; i++)
			{
				WildSpawnSupports wildSpawnSupports = Supports[i];
				array[i] = wildSpawnSupports.Copy();
			}
			bossLocationSpawn.Supports = array;
		}
		return bossLocationSpawn;
	}

	public List<WaveInfoClass> GetUsingTypes()
	{
		List<WaveInfoClass> list = new List<WaveInfoClass>();
		list.Add(new WaveInfoClass(1, BossType, BossDif));
		if (SubDatas != null)
		{
			foreach (BossLocationSpawnSubData subData in SubDatas)
			{
				list.Add(subData.GetTypesBotWave());
			}
		}
		else
		{
			int num = EscortCount;
			if (EscortType == WildSpawnType.followerZryachiy)
			{
				num += 3;
			}
			list.Add(new WaveInfoClass(num, EscortType, EscortDif));
		}
		return list;
	}

	public string DebugInfo()
	{
		return $"BOSS_WAVE info: BossName: {BossName}, BossChance: {BossChance}, BossZone: {BossZone}, BossPlayer: {BossPlayer}, BossDifficult: {BossDifficult}, BossEscortDifficult: {BossEscortDifficult}, BossEscortType: {BossEscortType}, BossEscortAmount: {BossEscortAmount} DependKarma:{DependKarma} TriggerId:{TriggerId} TriggerName:{TriggerName}";
	}

	public List<BotZone> GetPossibleZones(BotZone[] allZones, List<BotZone> markedBossZone, Vector3 closestsPos)
	{
		if (PossibleShuffledZones.Count > 0)
		{
			return PossibleShuffledZones;
		}
		if (BornZone != null && BornZone.Length > 1)
		{
			method_5(allZones, markedBossZone);
		}
		else
		{
			markedBossZone.Sort(delegate(BotZone x, BotZone y)
			{
				float sqrMagnitude = (x.CenterOfSpawnPoints - closestsPos).sqrMagnitude;
				float sqrMagnitude2 = (y.CenterOfSpawnPoints - closestsPos).sqrMagnitude;
				if (sqrMagnitude < sqrMagnitude2)
				{
					return -1;
				}
				return (sqrMagnitude > sqrMagnitude2) ? 1 : 0;
			});
			PossibleShuffledZones = markedBossZone.ToList();
			StringBuilder stringBuilder = new StringBuilder("sorted");
			foreach (BotZone possibleShuffledZone in PossibleShuffledZones)
			{
				stringBuilder.Append("    zone:" + possibleShuffledZone.name);
			}
		}
		return PossibleShuffledZones;
	}

	public List<BotZone> GetPossibleZones(BotZone[] allZones, List<BotZone> markedBossZone)
	{
		if (PossibleShuffledZones.Count > 0)
		{
			return PossibleShuffledZones;
		}
		PossibleShuffledZones.Clear();
		if (BornZone != null && BornZone.Length > 1)
		{
			method_5(allZones, markedBossZone);
		}
		else
		{
			PossibleShuffledZones = GClass856.Shuffle(markedBossZone);
		}
		return PossibleShuffledZones;
	}

	public void method_5(BotZone[] allZones, List<BotZone> markedBossZone)
	{
		string bornZone = BornZone;
		BotZone botZone = null;
		foreach (BotZone botZone2 in allZones)
		{
			if (botZone2.NameZone.Equals(bornZone))
			{
				botZone = botZone2;
			}
		}
		if (botZone == null)
		{
			PossibleShuffledZones = new List<BotZone> { GClass856.RandomElement(markedBossZone) };
		}
		else
		{
			PossibleShuffledZones = new List<BotZone> { botZone };
		}
	}

	public bool ShouldSpawnByKarma()
	{
		if (!DependKarma)
		{
			return ShallSpawn;
		}
		List<Player> allAlivePlayersList = Singleton<GameWorld>.Instance.AllAlivePlayersList;
		float num = float.MaxValue;
		int num2 = 0;
		foreach (Player item in allAlivePlayersList)
		{
			if (!item.IsAI)
			{
				num2++;
				float num3 = item.Profile.KarmaValue;
				if (num3 < num)
				{
					num = num3;
				}
			}
		}
		if (num < 0.5f)
		{
			float v = BossChance / Mathf.Pow(1000f, num);
			ShallSpawn = GClass856.IsTrue100(v);
			return ShallSpawn;
		}
		ShallSpawn = false;
		return ShallSpawn;
	}
}
