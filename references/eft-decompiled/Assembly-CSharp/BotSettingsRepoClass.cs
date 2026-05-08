using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public abstract class BotSettingsRepoClass
{
	[NonSerialized]
	public const string String_0_1 = "ScavRole/Boss";

	[NonSerialized]
	public const string String_1 = "ScavRole/Follower";

	[NonSerialized]
	public const string String_2 = "ScavRole/Sectant";

	[NonSerialized]
	public static bool Bool_0 = false;

	[NonSerialized]
	public static Dictionary<WildSpawnType, GClass790> Dictionary_0 = new Dictionary<WildSpawnType, GClass790>
	{
		{
			WildSpawnType.crazyAssaultEvent,
			new GClass790(isBoss: false, isFollower: false, isHostileToEverybody: false, String_0)
		},
		{
			WildSpawnType.assault,
			new GClass790(isBoss: false, isFollower: false, isHostileToEverybody: false, String_0)
		},
		{
			WildSpawnType.tagillaHelperAgro,
			new GClass790(isBoss: false, isFollower: false, isHostileToEverybody: false, String_0)
		},
		{
			WildSpawnType.skier,
			new GClass790(isBoss: false, isFollower: false, isHostileToEverybody: false, String_0)
		},
		{
			WildSpawnType.peacemaker,
			new GClass790(isBoss: false, isFollower: false, isHostileToEverybody: false, String_0)
		},
		{
			WildSpawnType.cursedAssault,
			new GClass790(isBoss: false, isFollower: false, isHostileToEverybody: false, String_0)
		},
		{
			WildSpawnType.test,
			new GClass790(isBoss: false, isFollower: false, isHostileToEverybody: false, String_0)
		},
		{
			WildSpawnType.spiritWinter,
			new GClass790(isBoss: false, isFollower: false, isHostileToEverybody: false, String_0)
		},
		{
			WildSpawnType.spiritSpring,
			new GClass790(isBoss: false, isFollower: false, isHostileToEverybody: false, String_0)
		},
		{
			WildSpawnType.marksman,
			new GClass790(isBoss: false, isFollower: false, isHostileToEverybody: false, "ScavRole/Marksman")
		},
		{
			WildSpawnType.assaultGroup,
			new GClass790(isBoss: true, isFollower: true, isHostileToEverybody: false, String_0)
		},
		{
			WildSpawnType.sectantPriest,
			new GClass790(isBoss: true, isFollower: false, isHostileToEverybody: true, "ScavRole/Sectant")
		},
		{
			WildSpawnType.sectantOni,
			new GClass790(isBoss: true, isFollower: false, isHostileToEverybody: true, "ScavRole/Sectant")
		},
		{
			WildSpawnType.sectantPrizrak,
			new GClass790(isBoss: true, isFollower: false, isHostileToEverybody: true, "ScavRole/Sectant")
		},
		{
			WildSpawnType.sectantPredvestnik,
			new GClass790(isBoss: true, isFollower: false, isHostileToEverybody: true, "ScavRole/Sectant")
		},
		{
			WildSpawnType.sectantWarrior,
			new GClass790(isBoss: false, isFollower: true, isHostileToEverybody: true, "ScavRole/Sectant")
		},
		{
			WildSpawnType.arenaFighterEvent,
			new GClass790(isBoss: true, isFollower: true, isHostileToEverybody: true, "ScavRole/ArenaFighterEvent")
		},
		{
			WildSpawnType.pmcUSEC,
			new GClass790(isBoss: true, isFollower: true, isHostileToEverybody: true, "ScavRole/PmcBot")
		},
		{
			WildSpawnType.pmcBEAR,
			new GClass790(isBoss: true, isFollower: true, isHostileToEverybody: true, "ScavRole/PmcBot")
		},
		{
			WildSpawnType.pmcBot,
			new GClass790(isBoss: true, isFollower: true, isHostileToEverybody: true, "ScavRole/PmcBot")
		},
		{
			WildSpawnType.arenaFighter,
			new GClass790(isBoss: true, isFollower: true, isHostileToEverybody: true, "ScavRole/ArenaFighter")
		},
		{
			WildSpawnType.exUsec,
			new GClass790(isBoss: true, isFollower: true, isHostileToEverybody: true, "ScavRole/ExUsec")
		},
		{
			WildSpawnType.shooterBTR,
			new GClass790(isBoss: true, isFollower: true, isHostileToEverybody: true, "ScavRole/ShooterBTR")
		},
		{
			WildSpawnType.infectedAssault,
			new GClass790(isBoss: false, isFollower: false, isHostileToEverybody: true, "ScavRole/infectedAssault")
		},
		{
			WildSpawnType.infectedCivil,
			new GClass790(isBoss: false, isFollower: false, isHostileToEverybody: true, "ScavRole/infectedCivil")
		},
		{
			WildSpawnType.infectedLaborant,
			new GClass790(isBoss: false, isFollower: false, isHostileToEverybody: true, "ScavRole/infectedLaborant")
		},
		{
			WildSpawnType.infectedPmc,
			new GClass790(isBoss: false, isFollower: false, isHostileToEverybody: true, "ScavRole/infectedPmc")
		},
		{
			WildSpawnType.bossBoarSniper,
			new GClass790(isBoss: true, isFollower: false, isHostileToEverybody: false, "ScavRole/Follower")
		},
		{
			WildSpawnType.bossBoar,
			new GClass790(isBoss: true, isFollower: false, isHostileToEverybody: false, "ScavRole/Boss")
		},
		{
			WildSpawnType.bossKolontay,
			new GClass790(isBoss: true, isFollower: false, isHostileToEverybody: false, "ScavRole/Boss")
		},
		{
			WildSpawnType.bossBully,
			new GClass790(isBoss: true, isFollower: false, isHostileToEverybody: false, "ScavRole/Boss")
		},
		{
			WildSpawnType.bossGluhar,
			new GClass790(isBoss: true, isFollower: false, isHostileToEverybody: false, "ScavRole/Boss")
		},
		{
			WildSpawnType.bossKilla,
			new GClass790(isBoss: true, isFollower: false, isHostileToEverybody: false, "ScavRole/Boss")
		},
		{
			WildSpawnType.bossPartisan,
			new GClass790(isBoss: true, isFollower: false, isHostileToEverybody: false, "ScavRole/Boss")
		},
		{
			WildSpawnType.bossKnight,
			new GClass790(isBoss: true, isFollower: false, isHostileToEverybody: false, "ScavRole/Boss", ETagStatus.Knight)
		},
		{
			WildSpawnType.bossKojaniy,
			new GClass790(isBoss: true, isFollower: false, isHostileToEverybody: false, "ScavRole/Boss")
		},
		{
			WildSpawnType.bossSanitar,
			new GClass790(isBoss: true, isFollower: false, isHostileToEverybody: false, "ScavRole/Boss")
		},
		{
			WildSpawnType.bossTagilla,
			new GClass790(isBoss: true, isFollower: false, isHostileToEverybody: false, "ScavRole/Boss")
		},
		{
			WildSpawnType.bossTagillaAgro,
			new GClass790(isBoss: true, isFollower: false, isHostileToEverybody: false, "ScavRole/Boss")
		},
		{
			WildSpawnType.bossKillaAgro,
			new GClass790(isBoss: true, isFollower: false, isHostileToEverybody: false, "ScavRole/Boss")
		},
		{
			WildSpawnType.infectedTagilla,
			new GClass790(isBoss: true, isFollower: false, isHostileToEverybody: false, "ScavRole/infectedTagilla")
		},
		{
			WildSpawnType.bossZryachiy,
			new GClass790(isBoss: true, isFollower: false, isHostileToEverybody: true, "ScavRole/Boss")
		},
		{
			WildSpawnType.peacefullZryachiyEvent,
			new GClass790(isBoss: true, isFollower: true, isHostileToEverybody: true, "ScavRole/Boss")
		},
		{
			WildSpawnType.ravangeZryachiyEvent,
			new GClass790(isBoss: true, isFollower: true, isHostileToEverybody: true, "ScavRole/Boss")
		},
		{
			WildSpawnType.sectactPriestEvent,
			new GClass790(isBoss: true, isFollower: true, isHostileToEverybody: true, "ScavRole/Boss")
		},
		{
			WildSpawnType.bossTest,
			new GClass790(isBoss: true, isFollower: false, isHostileToEverybody: false, "ScavRole/Boss")
		},
		{
			WildSpawnType.gifter,
			new GClass790(isBoss: true, isFollower: false, isHostileToEverybody: false, "ScavRole/Boss")
		},
		{
			WildSpawnType.followerBoarClose1,
			new GClass790(isBoss: false, isFollower: true, isHostileToEverybody: false, "ScavRole/Follower")
		},
		{
			WildSpawnType.followerBoarClose2,
			new GClass790(isBoss: false, isFollower: true, isHostileToEverybody: false, "ScavRole/Follower")
		},
		{
			WildSpawnType.followerBoar,
			new GClass790(isBoss: false, isFollower: true, isHostileToEverybody: false, "ScavRole/Follower")
		},
		{
			WildSpawnType.followerZryachiy,
			new GClass790(isBoss: true, isFollower: true, isHostileToEverybody: true, "ScavRole/Boss")
		},
		{
			WildSpawnType.followerBigPipe,
			new GClass790(isBoss: true, isFollower: true, isHostileToEverybody: false, "ScavRole/Boss", ETagStatus.BigPipe)
		},
		{
			WildSpawnType.followerBirdEye,
			new GClass790(isBoss: true, isFollower: true, isHostileToEverybody: false, "ScavRole/Boss", ETagStatus.Birdeye)
		},
		{
			WildSpawnType.followerBully,
			new GClass790(isBoss: false, isFollower: true, isHostileToEverybody: false, "ScavRole/Follower")
		},
		{
			WildSpawnType.followerKolontaySecurity,
			new GClass790(isBoss: false, isFollower: true, isHostileToEverybody: false, "ScavRole/Follower")
		},
		{
			WildSpawnType.followerKolontayAssault,
			new GClass790(isBoss: false, isFollower: true, isHostileToEverybody: false, "ScavRole/Follower")
		},
		{
			WildSpawnType.followerGluharAssault,
			new GClass790(isBoss: false, isFollower: true, isHostileToEverybody: false, "ScavRole/Follower")
		},
		{
			WildSpawnType.followerGluharScout,
			new GClass790(isBoss: false, isFollower: true, isHostileToEverybody: false, "ScavRole/Follower")
		},
		{
			WildSpawnType.followerGluharSecurity,
			new GClass790(isBoss: false, isFollower: true, isHostileToEverybody: false, "ScavRole/Follower")
		},
		{
			WildSpawnType.followerGluharSnipe,
			new GClass790(isBoss: false, isFollower: true, isHostileToEverybody: false, "ScavRole/Follower")
		},
		{
			WildSpawnType.followerKojaniy,
			new GClass790(isBoss: false, isFollower: true, isHostileToEverybody: false, "ScavRole/Follower")
		},
		{
			WildSpawnType.followerSanitar,
			new GClass790(isBoss: false, isFollower: true, isHostileToEverybody: false, "ScavRole/Follower")
		},
		{
			WildSpawnType.followerTagilla,
			new GClass790(isBoss: false, isFollower: true, isHostileToEverybody: false, "ScavRole/Boss")
		},
		{
			WildSpawnType.followerTest,
			new GClass790(isBoss: false, isFollower: true, isHostileToEverybody: false, "ScavRole/Follower")
		}
	};

	[NonSerialized]
	public static Dictionary<WildSpawnType, HashSet<WildSpawnType>> Dictionary_1 = new Dictionary<WildSpawnType, HashSet<WildSpawnType>>();

	[NonSerialized]
	public static List<WildSpawnType> List_0 = new List<WildSpawnType>
	{
		WildSpawnType.bossKolontay,
		WildSpawnType.followerKolontayAssault,
		WildSpawnType.followerKolontaySecurity
	};

	[NonSerialized]
	public static List<WildSpawnType> List_1 = new List<WildSpawnType>
	{
		WildSpawnType.bossBoar,
		WildSpawnType.followerBoar,
		WildSpawnType.followerBoarClose1,
		WildSpawnType.followerBoarClose2
	};

	[NonSerialized]
	public static List<WildSpawnType> List_2 = new List<WildSpawnType>
	{
		WildSpawnType.bossGluhar,
		WildSpawnType.followerGluharAssault,
		WildSpawnType.followerGluharScout,
		WildSpawnType.followerGluharSecurity,
		WildSpawnType.followerGluharSnipe
	};

	[NonSerialized]
	public static List<WildSpawnType> List_3 = new List<WildSpawnType>
	{
		WildSpawnType.bossBully,
		WildSpawnType.followerBully
	};

	[NonSerialized]
	public static List<WildSpawnType> List_4 = new List<WildSpawnType>
	{
		WildSpawnType.bossZryachiy,
		WildSpawnType.followerZryachiy
	};

	[NonSerialized]
	public static List<WildSpawnType> List_5 = new List<WildSpawnType>
	{
		WildSpawnType.bossKojaniy,
		WildSpawnType.followerKojaniy
	};

	[NonSerialized]
	public static List<WildSpawnType> List_6 = new List<WildSpawnType>
	{
		WildSpawnType.bossKnight,
		WildSpawnType.followerBirdEye,
		WildSpawnType.followerBigPipe
	};

	[NonSerialized]
	public static List<WildSpawnType> List_7 = new List<WildSpawnType>
	{
		WildSpawnType.infectedCivil,
		WildSpawnType.infectedLaborant,
		WildSpawnType.infectedAssault
	};

	[NonSerialized]
	public static List<WildSpawnType> List_8 = new List<WildSpawnType>
	{
		WildSpawnType.bossSanitar,
		WildSpawnType.followerSanitar
	};

	[NonSerialized]
	public static List<WildSpawnType> List_9 = new List<WildSpawnType>
	{
		WildSpawnType.sectantWarrior,
		WildSpawnType.sectantPriest
	};

	[NonSerialized]
	public static List<WildSpawnType> List_10 = new List<WildSpawnType>
	{
		WildSpawnType.sectantPredvestnik,
		WildSpawnType.sectantOni,
		WildSpawnType.sectantPrizrak
	};

	[NonSerialized]
	public static List<WildSpawnType> List_11 = new List<WildSpawnType>
	{
		WildSpawnType.bossTest,
		WildSpawnType.followerTest
	};

	public static string String_0 => GClass867.ToStringNoBox(EPlayerSide.Savage);

	public static string GetScavRoleKey(this WildSpawnType role)
	{
		return Dictionary_0[role].ScavRoleKey;
	}

	public static bool IsHostileToEverybody(this WildSpawnType role)
	{
		return Dictionary_0[role].IsHostileToEverybody;
	}

	public static bool IsMidPriorityBossOrFollower(this WildSpawnType role)
	{
		if (role != WildSpawnType.pmcBot && role != WildSpawnType.exUsec)
		{
			return role == WildSpawnType.arenaFighter;
		}
		return true;
	}

	public static bool ShallUseFastAnimator(this WildSpawnType role)
	{
		if ((uint)(role - 22) > 1u && (uint)(role - 64) > 1u)
		{
			return true;
		}
		return false;
	}

	public static void Init()
	{
		if (Bool_0)
		{
			return;
		}
		WildSpawnType[] array = (WildSpawnType[])Enum.GetValues(typeof(WildSpawnType));
		for (int i = 0; i < array.Length; i++)
		{
			WildSpawnType key = array[i];
			if (!Dictionary_0.ContainsKey(key))
			{
				Debug.LogError("BLOCKER ERROR. Add settings for role:" + key);
			}
		}
		Dictionary_0[WildSpawnType.followerTagilla].CountAsBossForStatistics = true;
		Dictionary_0[WildSpawnType.followerBigPipe].CountAsBossForStatistics = true;
		Dictionary_0[WildSpawnType.followerBirdEye].CountAsBossForStatistics = true;
		Dictionary_0[WildSpawnType.followerZryachiy].CountAsBossForStatistics = true;
		Dictionary_0[WildSpawnType.bossZryachiy].CountAsBossForStatistics = true;
		Dictionary_0[WildSpawnType.assaultGroup].CountAsBossForStatistics = false;
		Bool_0 = true;
		smethod_0(List_0);
		smethod_0(List_1);
		smethod_0(List_2);
		smethod_0(List_3);
		smethod_0(List_4);
		smethod_0(List_5);
		smethod_0(List_8);
		smethod_0(List_9);
		smethod_0(List_11);
		smethod_0(List_6);
		smethod_0(List_7);
		smethod_0(List_10);
	}

	public static bool CountAsBossForStatistics(this WildSpawnType role)
	{
		bool? countAsBossForStatistics = Dictionary_0[role].CountAsBossForStatistics;
		if (countAsBossForStatistics.HasValue)
		{
			return countAsBossForStatistics.Value;
		}
		return IsBoss(role);
	}

	public static bool IsSuitable(WildSpawnType test1, WildSpawnType test2)
	{
		if (test1 == test2)
		{
			return true;
		}
		if (Dictionary_1.TryGetValue(test1, out var value))
		{
			return value.Contains(test2);
		}
		return false;
	}

	public static ETagStatus GetPhraseTagFromRole(WildSpawnType role)
	{
		if (Dictionary_0.TryGetValue(role, out var value))
		{
			return value.PhraseTag;
		}
		return (ETagStatus)0;
	}

	public static void smethod_0(List<WildSpawnType> p0)
	{
		foreach (WildSpawnType item in p0)
		{
			HashSet<WildSpawnType> hashSet = new HashSet<WildSpawnType>();
			foreach (WildSpawnType item2 in p0)
			{
				if (item2 != item)
				{
					hashSet.Add(item2);
				}
			}
			Dictionary_1.Add(item, hashSet);
		}
	}

	public static bool IsBoss(this WildSpawnType role)
	{
		if (!Dictionary_0.TryGetValue(role, out var value))
		{
			return false;
		}
		return value.IsBoss;
	}

	public static bool IsPureBoss(this WildSpawnType role)
	{
		if (!Dictionary_0.TryGetValue(role, out var value))
		{
			return false;
		}
		if (value.IsBoss && !IsFollower(role))
		{
			return !IsSectant(role);
		}
		return false;
	}

	public static bool IsExUsec(this WildSpawnType role)
	{
		if (role != WildSpawnType.exUsec && role != WildSpawnType.bossKnight && role != WildSpawnType.followerBigPipe)
		{
			return role == WildSpawnType.followerBirdEye;
		}
		return true;
	}

	public static bool IsSectant(this WildSpawnType role)
	{
		if (role != WildSpawnType.sectantPriest && role != WildSpawnType.sectantWarrior && role != WildSpawnType.sectantOni && role != WildSpawnType.sectantPredvestnik)
		{
			return role == WildSpawnType.sectantPrizrak;
		}
		return true;
	}

	public static bool IsPmcBot(this WildSpawnType role)
	{
		if (role != WildSpawnType.pmcBEAR)
		{
			return role == WildSpawnType.pmcUSEC;
		}
		return true;
	}

	public static bool IsInfected(this WildSpawnType role)
	{
		if (role != WildSpawnType.infectedCivil && role != WildSpawnType.infectedLaborant && role != WildSpawnType.infectedAssault && role != WildSpawnType.infectedPmc)
		{
			return role == WildSpawnType.infectedTagilla;
		}
		return true;
	}

	public static bool IsFollower(this WildSpawnType role)
	{
		return Dictionary_0[role].IsFollower;
	}

	public static bool IsBossOrFollower(this WildSpawnType role)
	{
		if (!IsBoss(role))
		{
			return IsFollower(role);
		}
		return true;
	}
}
