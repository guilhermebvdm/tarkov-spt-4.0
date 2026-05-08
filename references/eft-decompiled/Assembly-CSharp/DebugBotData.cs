using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class DebugBotData : ScriptableObject
{
	public class Class128
	{
		public BotZone Zone;

		public int PosibleSlots;
	}

	[Serializable]
	[CompilerGenerated]
	public class Class129
	{
		public static readonly Class129 class129_0 = new Class129();

		public static Func<Class128, int> func_0;

		public static Func<WildSpawnWave, int> func_1;

		public int method_0(Class128 x)
		{
			return x.PosibleSlots;
		}

		public int method_1(WildSpawnWave x)
		{
			return x.slots_max;
		}
	}

	[CompilerGenerated]
	public class Class130
	{
		public BotZone botZone;

		public bool method_0(Class128 x)
		{
			return x.Zone == botZone;
		}
	}

	private const string string_0 = "DebugBotData.txt";

	private const float float_0 = 1f;

	private static DebugBotData debugBotData_0;

	private static bool bool_0;

	[SerializeField]
	public List<WildSpawnWave> StartWaves = new List<WildSpawnWave>();

	[SerializeField]
	public List<BossLocationSpawn> BossStartWaves = new List<BossLocationSpawn>();

	public GizmosConfig Gizmos;

	public bool autoRespawn;

	public bool evenlyDistribute;

	public bool spawnInstantly;

	public bool localProfiles;

	public bool AnySideBornUse;

	public bool DebugBrain;

	public DebugBotDesition DebugBotDesition;

	public DebugBotTactic DebugBotTactic;

	public bool BadShooting;

	public bool UseDebugWawes;

	public int respawnCount;

	public int MaxBotsCount;

	public int SummonSavages;

	public bool BadVision;

	public bool BadHearing;

	public bool NoShoot;

	public bool BadShootingAsCount;

	public bool MaxAgression;

	public bool NoMinMax;

	public bool DebugCoverLogs;

	public bool NoReciol;

	public bool FreeForAll;

	public bool FreeForAllOverride;

	public bool DrawColorCanShoot;

	public bool DrawColorLinesWeight;

	public bool NoArmorToPower;

	public bool GoodShooting;

	public bool NoSleepMode;

	public bool LoadSettingsFromCode;

	public bool DebugMalfunctions;

	public bool AlwaysSprint;

	public bool SprintOverride;

	public bool CheckStuck;

	public float SprintSpeed = 1f;

	public bool IgnoreBackendHostilitySettings;

	public bool useThisData;

	[HideInInspector]
	public List<WildSpawnWave> waves = new List<WildSpawnWave>();

	[HideInInspector]
	public List<BossLocationSpawn> bossWaves = new List<BossLocationSpawn>();

	public bool NoZoneBlocks;

	public bool NoRoleBlocks = true;

	public bool NoGrenadeOffset;

	public bool NoGrenadeThrow;

	public bool TrueAim;

	public DebugLook DebugRaysSystem;

	public bool NoStopServer;

	public bool AlwaysLayWhenDanger;

	public bool IgnoreBotLimits;

	public bool UseDebugMoverObject;

	public bool ActivatedStatus = true;

	public bool PhraseRequestAlwaysGood;

	private BotSpawner botSpawner_0;

	private BotZoneGroupsDictionary botZoneGroupsDictionary_0;

	private float float_1;

	public bool StartWithNoPlayers;

	public float BadShootOffset => 3f;

	public static DebugBotData Instance => debugBotData_0 ?? (debugBotData_0 = Load());

	public static bool UseDebugData
	{
		get
		{
			if (bool_0)
			{
				return false;
			}
			DebugBotData instance = Instance;
			int num;
			if (instance != null)
			{
				num = (instance.useThisData ? 1 : 0);
				if (num != 0)
				{
					goto IL_002a;
				}
			}
			else
			{
				num = 0;
			}
			bool_0 = true;
			goto IL_002a;
			IL_002a:
			return (byte)num != 0;
		}
	}

	public static DebugBotData Load()
	{
		try
		{
			if (File.Exists("DebugBotData.txt"))
			{
				GClass405 data = JsonUtility.FromJson<GClass405>(File.ReadAllText("DebugBotData.txt"));
				DebugBotData debugBotData = ScriptableObject.CreateInstance<DebugBotData>();
				debugBotData.SetData(data);
				return debugBotData;
			}
		}
		catch (Exception ex)
		{
			Debug.Log("Can't load Data " + ex);
		}
		return null;
	}

	public void StartUseAutoRespawn(BotSpawner botSpawner, BotsClass bots, BotZoneGroupsDictionary groups)
	{
		botSpawner_0 = botSpawner;
		botZoneGroupsDictionary_0 = groups;
	}

	public void InitMessage()
	{
		if (useThisData)
		{
			Debug.LogWarning("BE CARE!! USING DEBUG DATA. ЗАПУЩЕН ДЕБАЖНЫЙ РЕЖИМ БОТОВ. DEBUG MODE BOTS IS ON \n Если ты это видишь и не знаешь что это то скажи об этом комунить кто знает)  \n Если надо убрать в едиторе найди файл DebugBotData и нажми там UseThisData\n Если это видишь в билде то что бы отключить надо удалить файлик DebugBotData.txt\n To remove this in build delete file: DebugBotData.txt");
			Debug.LogError("BE CARE!! USING DEBUG DATA. ЗАПУЩЕН ДЕБАЖНЫЙ РЕЖИМ БОТОВ. DEBUG MODE BOTS IS ON \n Если ты это видишь и не знаешь что это то скажи об этом комунить кто знает)  \n Если надо убрать в едиторе найди файл DebugBotData и нажми там UseThisData\n Если это видишь в билде то что бы отключить надо удалить файлик DebugBotData.txt\n To remove this in build delete file: DebugBotData.txt");
			Debug.Log("BE CARE!! USING DEBUG DATA. ЗАПУЩЕН ДЕБАЖНЫЙ РЕЖИМ БОТОВ. DEBUG MODE BOTS IS ON \n Если ты это видишь и не знаешь что это то скажи об этом комунить кто знает)  \n Если надо убрать в едиторе найди файл DebugBotData и нажми там UseThisData\n Если это видишь в билде то что бы отключить надо удалить файлик DebugBotData.txt\n To remove this in build delete file: DebugBotData.txt");
		}
	}

	public void Init(BotZone[] botZones = null)
	{
		if (!useThisData)
		{
			return;
		}
		waves = new List<WildSpawnWave>();
		bossWaves = new List<BossLocationSpawn>();
		if (autoRespawn)
		{
			foreach (WildSpawnWave startWave in StartWaves)
			{
				WildSpawnWave wildSpawnWave = startWave.Copy();
				waves.Add(wildSpawnWave);
				wildSpawnWave.slots_min = respawnCount;
				wildSpawnWave.slots_max = respawnCount;
			}
			StaticManager.Instance.StaticUpdate += method_1;
		}
		else
		{
			foreach (WildSpawnWave startWave2 in StartWaves)
			{
				WildSpawnWave item = startWave2.Copy();
				waves.Add(item);
			}
			foreach (BossLocationSpawn bossStartWave in BossStartWaves)
			{
				BossLocationSpawn item2 = bossStartWave.Copy();
				bossWaves.Add(item2);
			}
		}
		if (!evenlyDistribute)
		{
			return;
		}
		if (botZones != null && botZones.Length != 0)
		{
			List<Class128> list = new List<Class128>();
			foreach (BotZone botZone in botZones)
			{
				Class128 item3 = new Class128
				{
					Zone = botZone,
					PosibleSlots = ((botZone.MaxPersons > 0) ? botZone.MaxPersons : 10)
				};
				list.Add(item3);
			}
			waves = method_0(list, waves);
		}
		else
		{
			Debug.LogError("Can't find zone to born!");
		}
	}

	public void Serialize(string path = "DebugBotData.txt")
	{
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		File.Create(path).Dispose();
		string value = JsonUtility.ToJson(new GClass405(this), prettyPrint: true);
		StreamWriter streamWriter = new StreamWriter(path);
		streamWriter.Write(value);
		streamWriter.Flush();
		streamWriter.Close();
	}

	public void DebugActivateAllBots(bool val = true)
	{
		BotsController botsController = BotsController.FindBotControllerEditorOnly();
		ActivatedStatus = val;
		if (botsController == null)
		{
			return;
		}
		foreach (BotOwner botOwner in botsController.Bots.BotOwners)
		{
			botOwner.StandBy.DebugActivate(ActivatedStatus);
		}
	}

	public void SetData(GClass405 result)
	{
		autoRespawn = result.autoRespawn;
		respawnCount = result.respawnCount;
		StartWaves = result.StartWaves;
		BossStartWaves = result.BossStartWaves;
		useThisData = result.useThisData;
		evenlyDistribute = result.evenlyDistribute;
		UseDebugWawes = result.useStartWaves;
		BadHearing = result.badHearing;
		BadVision = result.badVision;
		spawnInstantly = result.spawnInstantly;
		NoMinMax = result.NoMinMax;
		NoSleepMode = result.NoSleepMode;
		TrueAim = result.TrueAim;
		FreeForAllOverride = result.FreeForAllOverride;
		FreeForAll = result.FreeForAll;
		DebugBrain = result.UseDebugBrain;
		DebugBotDesition = result.DebugBotDesition;
		SummonSavages = result.SummonSavages;
		PhraseRequestAlwaysGood = result.PhraseRequestAlwaysGood;
	}

	public List<WildSpawnWave> method_0(List<Class128> zonesSpace, List<WildSpawnWave> wavesToCheck)
	{
		List<WildSpawnWave> list = new List<WildSpawnWave>();
		int num = zonesSpace.Sum((Class128 x) => x.PosibleSlots);
		foreach (WildSpawnWave item in wavesToCheck)
		{
			int slots_max = item.slots_max;
			Class128 @class = null;
			foreach (Class128 item2 in zonesSpace)
			{
				if (item2.PosibleSlots > slots_max)
				{
					@class = item2;
					break;
				}
			}
			if (@class != null)
			{
				@class.PosibleSlots -= slots_max;
				item.SpawnPoints = @class.Zone.name;
				list.Add(item);
				continue;
			}
			int num2 = item.slots_max;
			List<BotZone> list2 = new List<BotZone>();
			foreach (Class128 item3 in zonesSpace)
			{
				if (num2 != 0)
				{
					WildSpawnWave wildSpawnWave = item.Copy();
					list.Add(wildSpawnWave);
					wildSpawnWave.SpawnPoints = item3.Zone.name;
					if (item3.PosibleSlots <= num2)
					{
						num2 -= item3.PosibleSlots;
						wildSpawnWave.slots_max = (wildSpawnWave.slots_min = item3.PosibleSlots);
						item3.PosibleSlots = 0;
						list2.Add(item3.Zone);
						continue;
					}
					wildSpawnWave.slots_min = (wildSpawnWave.slots_max = num2);
					item3.PosibleSlots -= num2;
					num2 = 0;
					break;
				}
				break;
			}
			foreach (BotZone botZone in list2)
			{
				zonesSpace.RemoveAll((Class128 x) => x.Zone == botZone);
			}
			if (num2 > 0)
			{
				for (int num3 = 0; num3 < num2; num3++)
				{
					WildSpawnWave wildSpawnWave2 = GClass856.RandomElement(list);
					wildSpawnWave2.slots_min = ++wildSpawnWave2.slots_max;
				}
				int num4 = wavesToCheck.Sum((WildSpawnWave x) => x.slots_max);
				Debug.LogError($"CAN SPAWN THIS COUNT OF BOTS AT THIS MAP. THIS IS IMPOSIBLE!!!!! HOW TO FIX: add patrols || remove bots || change map || don't do this   MaxBots:{num}  YourCount:{num4}   " + $" Bots without place:{num2}. this map does not support this number of bots. ");
			}
		}
		return list;
	}

	public void method_1()
	{
		if (!(float_1 > Time.time))
		{
			return;
		}
		float_1 = 1f + Time.time;
		foreach (KeyValuePair<BotZone, GClass575> item in botZoneGroupsDictionary_0)
		{
			foreach (BotsGroup item2 in item.Value.GetAllGroupsDebug())
			{
				if (item2 != null && item2.MembersCount < respawnCount && item2.MembersCount > 0)
				{
					botSpawner_0.method_2(item2.Side, item.Key).HandleExceptions();
				}
			}
		}
	}
}
