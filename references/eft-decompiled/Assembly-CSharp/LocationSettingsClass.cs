using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EFT;
using EFT.Game.Spawning;
using JsonType;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class LocationSettingsClass
{
	public class GClass1419
	{
		public string BackendUrl;

		public GClass846 Location;

		public uint crc;
	}

	public class Location
	{
		public class GClass1420
		{
			public object[] items;

			public int min;

			public int max;
		}

		public class GClass1421
		{
			public bool enabled;

			public bool @static;

			public ELootRarity rarity;

			public string[] filter;
		}

		public class GClass1422
		{
			public string name;

			public string description;
		}

		public class GClass1423
		{
			public int PlaneAirdropStartMin = 250;

			public int PlaneAirdropStartMax = 550;

			public int PlaneAirdropEnd = 2200;

			public float PlaneAirdropChance;

			public int PlaneAirdropMax;

			public int PlaneAirdropCooldownMin = 550;

			public int PlaneAirdropCooldownMax = 850;

			public int AirdropPointDeactivateDistance = 60;

			public int MinPlayersCountToSpawnAirdrop = 6;

			public int UnsuccessfulTryPenalty = 120;
		}

		public class EventsDataClass
		{
			public GClass1425 Halloween2024;

			public GClass1426 Khorovod;

			public IEnumerable<WaveInfoClass> GetEventsProfilesOnStart()
			{
				GClass1425 halloween = Halloween2024;
				if (halloween != null && halloween.InfectionPercentage > 0)
				{
					return BotHalloweenWithZombies.GetProfilesOnStart();
				}
				return null;
			}
		}

		public class GClass1425
		{
			public int InfectionPercentage;

			public int MinInfectionPercentage;

			public List<GClass1427> CrowdAttackSpawnParams;

			public float ZombieMultiplier;

			public int MaxCrowdAttackSpawnLimit;

			public int CrowdsLimit;

			public float InfectedLookCoeff;

			public float CrowdCooldownPerPlayerSec;

			public float ZombieCallPeriodSec;

			public float ZombieCallDeltaRadius;

			public float ZombieCallRadiusLimit;

			public float CrowdAttackBlockRadius;

			public float MinSpawnDistToPlayer = 50f;

			public float TargetPointSearchRadiusLimit = 200f;

			public float CalcRealInfectionLevel()
			{
				return Mathf.Lerp((float)MinInfectionPercentage / 100f, 1f, (float)InfectionPercentage / 100f);
			}
		}

		public class GClass1426
		{
			public float Chance;
		}

		public class GClass1427
		{
			public BotDifficulty Difficulty;

			public WildSpawnType Role;

			public int Weight;
		}

		public class GClass1428
		{
			public string id;

			public ResourceKey pic;

			[JsonIgnore]
			public string String_0 => "/files/" + pic.path;

			public async Task<Sprite> LoadBannerSprite(GInterface221 session)
			{
				return await GClass855.LoadIconSprite(session, String_0);
			}

			public void GetAndAssignSprite(GInterface221 session, Image image, bool resetImage, CancellationToken cancellationToken)
			{
				GClass855.LoadAndAssignSprite(session, String_0, image, resetImage, cancellationToken).HandleExceptions();
			}
		}

		[Serializable]
		public class TransitParameters
		{
			public int id = 1;

			public bool active = true;

			public string name = string.Empty;

			public string description = string.Empty;

			public string conditions = string.Empty;

			public int activateAfterSec = 999;

			public ushort time = 30;

			public string target = string.Empty;

			public string location = string.Empty;

			public bool events;

			public bool hideIfNoKey;

			[NonSerialized]
			public bool eventsEnable;
		}

		public enum LocationRules
		{
			Normal,
			AvoidOwnPmc,
			AvoidAllPmc
		}

		[Serializable]
		[CompilerGenerated]
		public class Class923
		{
			public static readonly Class923 class923_0 = new Class923();

			public static Func<string, bool> func_0;

			public bool method_0(string x)
			{
				return x != "laboratory";
			}
		}

		public const string FACTORY_DAY_ID = "factory4_day";

		public const string FACTORY_NIGHT_ID = "factory4_night";

		public const string FACTORY_ID = "factory4";

		public const string LABORATORY_ID = "laboratory";

		public const string SANDBOX_ID = "Sandbox";

		public const string SANDBOX_HIGH_ID = "Sandbox_high";

		[NonSerialized]
		public const string HIDEOUT_ID = "hideout";

		public static readonly string[] AvailableMaps = new string[13]
		{
			"develop", "Woods", "factory4_day", "factory4_night", "bigmap", "Shoreline", "Interchange", "RezervBase", "laboratory", "Lighthouse",
			"TarkovStreets", "Sandbox", "Sandbox_high"
		};

		public bool IsSecret;

		public float IconX;

		public float IconY;

		public string _Id;

		public string Id;

		public bool Enabled;

		public bool EnableCoop;

		public bool ForceOnlineRaidInPVE;

		public bool Locked;

		public string Name;

		public ResourceKey Scene;

		public string Description;

		public float Area;

		public int RequiredPlayerLevelMin;

		public int RequiredPlayerLevelMax;

		public int matching_min_seconds;

		public int users_gather_seconds;

		public int users_spawn_seconds_n;

		public int users_spawn_seconds_n2;

		public int users_summon_seconds;

		public int sav_summon_seconds;

		public int session_duration_minutes;

		public int exit_count;

		public int exit_access_time;

		public int exit_time;

		public int EscapeTimeLimit;

		public int EscapeTimeLimitCoop;

		public int MinDistToExitPoint;

		public int MaxBotPerZone;

		public int MaxDistToFreePoint;

		public int MinDistToFreePoint;

		public int SpawnSafeDistanceMeters;

		public bool NoGroupSpawn;

		public bool OneTimeSpawn;

		public ResourceKey Preview;

		public string[] filter_ex;

		public WildSpawnWave[] waves;

		public MinMaxBots[] MinMaxBots;

		public BotLocationModifier BotLocationModifier;

		public NonWaveGroupScenarioData NonWaveGroupScenario;

		public BossLocationSpawn[] BossLocationSpawn;

		public GClass1420[] limits;

		public Dictionary<string, GClass1421> containers;

		public GClass1428[] Banners;

		public EventsDataClass Events;

		public LabyrinthSyncableTrapDataClass EventTrapsData;

		[JsonProperty("transits")]
		public TransitParameters[] transitParameters;

		public GClass1423[] airdropParameters = new GClass1423[1]
		{
			new GClass1423()
		};

		public int PmcMaxPlayersInGroup = 5;

		public int ScavMaxPlayersInGroup = 4;

		public int MinGroupSize = 2;

		public int MinPlayers;

		public int MaxPlayers;

		public int PlayersRequestCount = -1;

		public int MaxCoopGroup;

		public int AveragePlayerLevel;

		public int AveragePlayTime;

		public LocationRules Rules;

		public long UnixDateTime;

		public SpawnPointParams[] SpawnPointParams;

		public LocationExitClass[] exits;

		[JsonProperty("secretExits")]
		public GClass1432[] SecretExits;

		public GClass1404 Loot;

		public string OpenZones;

		public bool OcculsionCullingEnabled;

		public bool DisabledForScav;

		public bool OldSpawn = true;

		public bool OfflineOldSpawn;

		public bool NewSpawn;

		public bool OfflineNewSpawn;

		public bool NewSpawnForPlayers;

		public int BotMax;

		public int BotMaxPlayer;

		public int BotMaxTimePlayer;

		public int BotStart;

		public int BotStartPlayer;

		public int BotStop;

		public int BotSpawnPeriodCheck;

		public int BotSpawnCountStep;

		public int BotSpawnTimeOnMin;

		public int BotSpawnTimeOnMax;

		public int BotSpawnTimeOffMin;

		public int BotSpawnTimeOffMax;

		public int BotEasy;

		public int BotNormal;

		public int BotHard;

		public int BotImpossible;

		public int BotAssault;

		public int BotMarksman;

		public string DisabledScavExits;

		public string[] AccessKeys;

		public int MinPlayerLvlAccessKeys;

		public static IEnumerable<string> NightTimeAllowedLocations => AvailableMaps.Where((string x) => x != "laboratory");

		public Vector2 RelativeMapPos => new Vector2(IconX / 1000f, IconY / 1000f);

		public bool IsHideout => Id == "hideout";

		[JsonIgnore]
		public string LocalizedName => GClass2348.Localized(_Id + " Name");
	}

	public class GClass1429
	{
		public string Source;

		public string Destination;

		public bool Event;
	}

	[NonSerialized]
	public const float Float_0 = 1000f;

	public Dictionary<string, Location> locations;

	public GClass1429[] paths;
}
