using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Comfort.Common;
using EFT.Game.Spawning;
using EFT.Interactive;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

namespace EFT;

public class BotsController : IZones
{
	[Serializable]
	[CompilerGenerated]
	public class Class1152
	{
		public static readonly Class1152 class1152_0 = new Class1152();

		public static Func<BotZone, string> func_0;

		public static Func<BotZone, IEnumerable<ISpawnPoint>> func_1;

		public string method_0(BotZone zone)
		{
			return zone.NameZone;
		}

		public IEnumerable<ISpawnPoint> method_1(BotZone zone)
		{
			return zone.SpawnPoints;
		}
	}

	[CompilerGenerated]
	public class Class1153
	{
		public Vector3 requestedPosition;

		public GroupPoint closetsGroupPoint;

		public List<BotOwner> botsWithSameCG;

		public bool method_0(BotOwner x)
		{
			return x.CoverSearchInfo.ConnectionGroupId == closetsGroupPoint.ConnectionGroup;
		}

		public Vector3 method_1(int i)
		{
			return botsWithSameCG[i].Position - requestedPosition;
		}
	}

	public GClass3597 OnlineDependenceSettings;

	[NonSerialized]
	public static WildSpawnType[] AllTypes_1;

	public readonly BotsClass Bots;

	[NonSerialized]
	public bool CanSpawn;

	[NonSerialized]
	public BotSpawner BotSpawner_1;

	public AICoreControllerClass AICoreController = new AICoreControllerClass();

	public AIStationaryController StationaryWeapons;

	[NonSerialized]
	public GClass412 Connections;

	[NonSerialized]
	public AICoversData CoversData_1;

	public ZoneLeaveControllerClass ZonesLeaveController;

	public GClass1874 ArtilleryZonesController;

	public readonly Dictionary<GameObject, ELookObstacleType> AILayerLookObstaclesCache = new Dictionary<GameObject, ELookObstacleType>();

	[NonSerialized]
	public int MaxCount;

	public BotLocationModifier BotLocationModifier;

	public GClass636 CutController;

	[NonSerialized]
	public GClass678 SpawnControlScenario;

	[NonSerialized]
	public BotPresetClass[] BotPresets;

	[NonSerialized]
	public GClass612[] BotScatterings;

	[NonSerialized]
	public BotTradersServices BotTradersServices_1;

	public static WildSpawnType[] AllTypes => AllTypes_1 ?? (AllTypes_1 = (WildSpawnType[])Enum.GetValues(typeof(WildSpawnType)));

	[field: NonSerialized]
	public AITaskManager AiTaskManager { get; set; }

	[field: NonSerialized]
	public BotsEventsController EventsController { get; set; }

	[field: NonSerialized]
	public IBotGame BotGame { get; set; }

	public AICoversData CoversData => CoversData_1;

	public GameDateTime GameDateTime => BotGame.GameDateTime;

	public bool IsEnable => BotSpawner_1 != null;

	public int AliveAndLoadingBotsCount => BotSpawner_1.AliveAndLoadingBotsCount;

	public int AliveLoadingDelayedBotsCount => BotSpawner_1.AllBotsWithDelayed;

	public BotSpawner BotSpawner => BotSpawner_1;

	[field: NonSerialized]
	public IPlayersCollection Players { get; set; }

	[field: NonSerialized]
	public BotsPlantedMinesController PlantedMines { get; set; }

	public BotTradersServices BotTradersServices => BotTradersServices_1;

	[field: NonSerialized]
	public BotSpawnLimiter BotSpawnLimiter { get; set; }

	[field: NonSerialized]
	public BotsSmokesVisionSystem BotSmokesVisionSystem { get; set; }

	public bool IsPvE
	{
		get
		{
			if (TarkovApplication.Exist(out var tarkovApplication) && (tarkovApplication.CurrentRaidSettings.IsPveOffline || tarkovApplication.CurrentRaidSettings.Local))
			{
				return true;
			}
			return false;
		}
	}

	public BotsController()
	{
		Connections = new GClass412();
		CutController = new GClass636();
		Bots = new BotsClass(Connections);
		BotTradersServices_1 = new BotTradersServices(this);
		BotSpawnLimiter = new BotSpawnLimiter(this);
	}

	public void Init(IBotGame botGame, IBotCreator botCreator, BotZone[] botZones, ISpawnSystem spawnSystem, BotLocationModifier botLocationModifier, bool botEnable, bool freeForAll, bool enableWaveControl, bool online, bool haveSectants, [NotNull] IPlayersCollection players, string openZones, LocationSettingsClass.Location.EventsDataClass events)
	{
		smethod_0();
		BotGame = botGame;
		Players = players;
		CoversData_1 = AICoversData.CreateOrFind(undestandAtGame: true);
		CoversData_1.RestoreData();
		CoversData_1.CachePoints();
		BotCoverBounds.DisableAllCoilliders();
		BotDoorsController.CreateOrFind(doErrorMsg: false).RefreshData(this);
		BotCreationDataClass.ProfilesLoadingProcess = 0;
		if (Singleton<IBotGame>.Instantiated)
		{
			Singleton<IBotGame>.Release(Singleton<IBotGame>.Instance);
		}
		Singleton<IBotGame>.Create(BotGame);
		PlantedMines = new BotsPlantedMinesController(CoversData_1);
		StationaryWeapons = GClass870.FindUnityObjectOfType<AIStationaryController>();
		StationaryWeapons.Init(CoversData_1.AICorePointsHolder);
		AiTaskManager = new AITaskManager();
		ZonesLeaveController = new ZoneLeaveControllerClass(Bots, botGame.GameDateTime, botZones, haveSectants, MaxCount, players);
		ArtilleryZonesController = new GClass1874(this);
		ArtilleryZonesController.Activate();
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		Singleton<GClass620>.Create(new GClass620());
		Singleton<GClass620>.Instance.SetSettings(BotPresets, BotScatterings, botLocationModifier, IsPvE);
		OnlineDependenceSettings = new GClass3597(online);
		BotSettingsRepoClass.Init();
		if (DebugBotData.UseDebugData)
		{
			DebugBotData.Instance.InitMessage();
			if (!botEnable)
			{
				UnityEngine.Debug.LogError("Really? You are using DebugBotData, but you turned off bots? Why??? ");
				botEnable = true;
				UnityEngine.Debug.LogError("Bots turned ON!!! (If u still want to play without bots but with DBD write code)");
			}
		}
		if (DebugBotData.UseDebugData && DebugBotData.Instance.FreeForAllOverride)
		{
			freeForAll = DebugBotData.Instance.FreeForAll;
		}
		EventsController = new BotsEventsController(BotGame.GameDateTime, Bots, ZonesLeaveController, BotSpawner_1, CoversData.AIPlaceInfoHolder, events, CoversData);
		method_2();
		GClass369.Init();
		CanSpawn = botEnable;
		for (int i = 0; i < botZones.Length; i++)
		{
			botZones[i].Init(botLocationModifier, this);
		}
		BotLocationModifier = botLocationModifier ?? new BotLocationModifier();
		BotLocationModifier.Validate();
		List<BotZone> list = new List<BotZone>();
		BotZone[] array = botZones;
		foreach (BotZone botZone in array)
		{
			if (botZone.SpawnPointMarkers.Count != 0)
			{
				list.Add(botZone);
			}
		}
		if (list.Count <= 0)
		{
			CanSpawn = false;
		}
		Dictionary<PatrolPoint, BotZone> dictionary = method_1(botZones);
		botZones = list.ToArray();
		BotSpawner_1 = new GClass1890(botCreator, botGame, botZones, Bots, spawnSystem, MaxCount, freeForAll, dictionary, openZones);
		BotSpawner_1.SetMaxBots(MaxCount);
		if (DebugBotData.UseDebugData)
		{
			DebugBotData.Instance.StartUseAutoRespawn(BotSpawner_1, Bots, BotSpawner_1.Groups);
		}
		SpawnControlScenario = new GClass678();
		if (enableWaveControl)
		{
			SpawnControlScenario.Init(BotSpawner_1, dictionary);
		}
		if (Singleton<BotEventHandler>.Instantiated)
		{
			if (CanSpawn)
			{
				Singleton<BotEventHandler>.Instance.OnKill += method_11;
				Singleton<BotEventHandler>.Instance.OnBeingHit += method_12;
				Singleton<BotEventHandler>.Instance.OnGrenadeThrow += method_5;
				Singleton<BotEventHandler>.Instance.OnGrenadeExplosive += method_3;
				Singleton<BotEventHandler>.Instance.OnRocketExplosive += method_4;
				Singleton<BotEventHandler>.Instance.OnApplyLighthouseKeeperFriendlyUsecs += method_6;
				Singleton<BotEventHandler>.Instance.OnApplyLighthouseKeeperFriendlyZryachiy += method_7;
			}
			Singleton<BotEventHandler>.Instance.OnApplyTraderServiceBtrSupport += method_8;
			Singleton<BotEventHandler>.Instance.OnStopTraderServiceBtrSupport += method_9;
			Singleton<BotEventHandler>.Instance.OnInterruptTraderServiceBtrSupportByBetrayer += method_10;
		}
		Connections.Activate();
		if (BotGame != null)
		{
			BotGame.UpdateByUnity += method_0;
		}
		stopwatch.Stop();
		ZonesLeaveController.Activate();
		AICoreController.Activate();
		EventsController.Activate();
		CutController.Init(this, botZones);
		PlantedMines.Activate();
		CutController.Init(this, botZones);
		BotSmokesVisionSystem = new BotsSmokesVisionSystem(Bots, CoversData_1);
		if (!(CoversData_1.Patrols != null) || CoversData_1.Patrols.LootPointClusters == null)
		{
			return;
		}
		foreach (AILootPointsCluster lootPointCluster in CoversData_1.Patrols.LootPointClusters)
		{
			lootPointCluster.CollectActualSpawnedLoot(Singleton<GameWorld>.Instance.AllLoot);
		}
	}

	public static void smethod_0()
	{
		if (MonoBehaviourSingleton<ServerSpatialController>.Instantiated)
		{
			ServerSpatialController instance = MonoBehaviourSingleton<ServerSpatialController>.Instance;
			if (!instance.Restored)
			{
				instance.gameObject.SetActive(value: false);
			}
		}
	}

	public void Disable()
	{
		CanSpawn = false;
	}

	public void DebugSpawnErrorCase()
	{
	}

	public void method_0()
	{
		ArtilleryZonesController.ManualUpdate();
		AICoreController.Update();
		BotSmokesVisionSystem.Update();
		AiTaskManager.Update();
		Bots.UpdateByUnity();
		EventsController.ManualUpdate();
	}

	public BotZoneGroupsDictionary Groups()
	{
		return BotSpawner_1.Groups;
	}

	public Dictionary<PatrolPoint, BotZone> method_1(BotZone[] zones)
	{
		Dictionary<PatrolPoint, BotZone> dictionary = new Dictionary<PatrolPoint, BotZone>();
		foreach (BotZone botZone in zones)
		{
			PatrolWay[] patrolWays = botZone.PatrolWays;
			for (int j = 0; j < patrolWays.Length; j++)
			{
				foreach (PatrolPoint point in patrolWays[j].Points)
				{
					dictionary.Add(point, botZone);
				}
			}
		}
		return dictionary;
	}

	public void method_2()
	{
		IEnumerable<AIPlaceInfo> allObjects = LocationScene.GetAllObjects<AIPlaceInfo>();
		Door[] allDoors = GClass870.FindUnityObjectsOfType<Door>();
		foreach (AIPlaceInfo item in allObjects)
		{
			item.Init(allDoors, this);
		}
	}

	public void method_3(Vector3 explosionPosition, string playerProfileID, bool isSmoke, float smokeRadius, float smokeLifeTime, int throwableId)
	{
		if (playerProfileID == null)
		{
			UnityEngine.Debug.LogError("player is null");
			return;
		}
		IPlayerOwner alivePlayerBridgeByProfileID = Singleton<GameWorld>.Instance.GetAlivePlayerBridgeByProfileID(playerProfileID);
		if (alivePlayerBridgeByProfileID == null)
		{
			return;
		}
		Singleton<BotEventHandler>.Instance.PlaySound(alivePlayerBridgeByProfileID.iPlayer, explosionPosition, 190f, AISoundType.gun);
		Vector3 position = alivePlayerBridgeByProfileID.iPlayer.Position;
		if (!isSmoke)
		{
			return;
		}
		float radius = smokeRadius * LocalBotSettingsProviderClass.Core.SMOKE_GRENADE_RADIUS_COEF;
		foreach (KeyValuePair<BotZone, GClass575> item in Groups())
		{
			foreach (BotsGroup group in item.Value.GetGroups(notNull: true))
			{
				group.AddSmokePlace(explosionPosition, smokeLifeTime, radius, position);
			}
		}
		BotSmokesVisionSystem.TryAddGrenade(throwableId, explosionPosition);
	}

	public void method_4(Vector3 explosionPosition, string playerProfileID)
	{
		if (playerProfileID == null)
		{
			UnityEngine.Debug.LogError("player is null");
			return;
		}
		IPlayerOwner alivePlayerBridgeByProfileID = Singleton<GameWorld>.Instance.GetAlivePlayerBridgeByProfileID(playerProfileID);
		if (alivePlayerBridgeByProfileID != null)
		{
			Singleton<BotEventHandler>.Instance.PlaySound(alivePlayerBridgeByProfileID.iPlayer, explosionPosition, 190f, AISoundType.gun);
		}
	}

	public void method_5(Grenade grenade, Vector3 position, Vector3 force, float mass)
	{
		Vector3 danger = GClass577.FindDangerPoint(position, force, mass);
		foreach (BotOwner botOwner in Bots.BotOwners)
		{
			botOwner.BewareGrenade.AddGrenadeDanger(danger, grenade);
		}
	}

	public void method_6(Player player)
	{
		BotTradersServices_1.LighthouseKeeperServices.OnFriendlyExUsecPurchased(player);
	}

	public void method_7(Player player)
	{
		BotTradersServices_1.LighthouseKeeperServices.OnFriendlyZryachiyPurchased(player);
	}

	public void method_8(List<Player> passengers)
	{
		BotTradersServices_1.BTRServices.OnBTRSupportPurchased(passengers);
	}

	public void method_9()
	{
		BotTradersServices_1.BTRServices.OnBTRSupportStop();
	}

	public void method_10(Player player)
	{
		BotTradersServices_1.BTRServices.OnBTRSupportInterruptedByBetrayer(player);
	}

	public Vector3? GetBossPosition(BotZone zone, Vector3? closestBossPos)
	{
		if (closestBossPos.HasValue)
		{
			return closestBossPos.Value;
		}
		BotZoneGroupsDictionary botZoneGroupsDictionary = Groups();
		if (botZoneGroupsDictionary != null && botZoneGroupsDictionary.TryGetValue(zone, out var value))
		{
			BotsGroup botsGroup = null;
			WildSpawnType[] allTypes = AllTypes;
			foreach (WildSpawnType spawnType in allTypes)
			{
				botsGroup = value.Group(isBossOrFollower: true, spawnType);
				if (botsGroup != null && botsGroup.MembersCount > 0)
				{
					break;
				}
			}
			if (botsGroup != null && botsGroup.MembersCount > 0)
			{
				int num = 0;
				BotOwner botOwner;
				while (true)
				{
					if (num < botsGroup.MembersCount)
					{
						botOwner = botsGroup.Member(num);
						if (BotSettingsRepoClass.IsBoss(botOwner.Profile.Info.Settings.Role))
						{
							break;
						}
						num++;
						continue;
					}
					return botsGroup.Member(0).Position;
				}
				return botOwner.Position;
			}
			return null;
		}
		return null;
	}

	Vector3? IZones.GetBossPosition(BotZone zone, Vector3? closestBossPos)
	{
		//ILSpy generated this explicit interface implementation from .override directive in GetBossPosition
		return this.GetBossPosition(zone, closestBossPos);
	}

	public ISpawnPoint[] ZoneSpawnPoints(string zoneName)
	{
		if (BotSpawner_1 != null && CanSpawn)
		{
			BotZone zoneByName = BotSpawner_1.GetZoneByName(zoneName);
			if (!(zoneByName != null))
			{
				return Array.Empty<ISpawnPoint>();
			}
			return zoneByName.SpawnPoints;
		}
		return Array.Empty<ISpawnPoint>();
	}

	ISpawnPoint[] IZones.ZoneSpawnPoints(string zoneName)
	{
		//ILSpy generated this explicit interface implementation from .override directive in ZoneSpawnPoints
		return this.ZoneSpawnPoints(zoneName);
	}

	public IEnumerable<string> ZoneNames(bool canBeSnipe)
	{
		if (BotSpawner_1 != null && CanSpawn)
		{
			return (from zone in BotSpawner_1.SpawnZones(canBeSnipe)
				select zone.NameZone).ToArray();
		}
		return Enumerable.Empty<string>();
	}

	IEnumerable<string> IZones.ZoneNames(bool canBeSnipe)
	{
		//ILSpy generated this explicit interface implementation from .override directive in ZoneNames
		return this.ZoneNames(canBeSnipe);
	}

	public ISpawnPoint[] AllZonesSpawnPoints(bool canBeSnipe)
	{
		return BotSpawner_1.SpawnZones(canBeSnipe).SelectMany((BotZone zone) => zone.SpawnPoints).ToArray();
	}

	ISpawnPoint[] IZones.AllZonesSpawnPoints(bool canBeSnipe)
	{
		//ILSpy generated this explicit interface implementation from .override directive in AllZonesSpawnPoints
		return this.AllZonesSpawnPoints(canBeSnipe);
	}

	public async Task ActivateBotsByWave(BotWaveDataClass wave)
	{
		if (BotSpawner_1 != null && CanSpawn)
		{
			await BotSpawner_1.ActivateBotsByWave(wave);
		}
	}

	public void ActivateBotsByWave(BossLocationSpawn wave)
	{
		if (BotSpawner_1 != null && CanSpawn)
		{
			BotSpawner_1.ActivateBotsByWave(wave);
		}
	}

	public void ActivateBotsWithoutWave(int count, IGetProfileData data)
	{
		if (BotSpawner_1 != null && CanSpawn)
		{
			BotSpawner_1.ActivateBotsWithoutWave(count, data).HandleExceptions();
		}
	}

	public void method_11(IPlayer killer, IPlayer target)
	{
		if (!target.AIData.IsBossOrFollowerRequireRevenge() && killer.Loyalty.WasAttackedBy(target))
		{
			return;
		}
		if (target.AIData.IsAI && killer.AIData.IsAI)
		{
			WildSpawnType role = killer.AIData.BotOwner.Profile.Info.Settings.Role;
			if (target.AIData.BotOwner.Settings.GetAlwaysFriendlyBotTypes().Contains(role))
			{
				return;
			}
		}
		float num = LocalBotSettingsProviderClass.Core.DEAD_AGR_DIST * LocalBotSettingsProviderClass.Core.DEAD_AGR_DIST;
		foreach (BotOwner botOwner in Bots.BotOwners)
		{
			if (botOwner.Side != target.Side && (botOwner.Transform.position - target.Transform.position).sqrMagnitude < num)
			{
				botOwner.Tactic.AggressionChange(botOwner.Settings.FileSettings.Mind.FRIEND_AGR_KILL);
			}
		}
		if (!killer.AIData.IsAI && !target.Loyalty.CanBeFreeKilled && killer.Profile.Info.Side == EPlayerSide.Savage && target.Profile.Info.Side == EPlayerSide.Savage && target.Profile.Info.Settings.Role != WildSpawnType.pmcBEAR && target.Profile.Info.Settings.Role != WildSpawnType.pmcUSEC && target.Profile.Info.Settings.Role != WildSpawnType.pmcBot && target.Profile.Info.Settings.Role != WildSpawnType.exUsec && target.Profile.Info.Settings.Role != WildSpawnType.arenaFighter)
		{
			foreach (BotOwner item in BotSpawner_1.GetAllBotsNearTarget(killer.Transform.position, LocalBotSettingsProviderClass.Core.SAVAGE_KILL_DIST))
			{
				BotsGroup botsGroup = item.BotsGroup;
				if (!botsGroup.Enemies.ContainsKey(target) && !botsGroup.Enemies.ContainsKey(killer) && !botsGroup.InitialFileSettings.Boss.NOT_ADD_TO_ENEMY_ON_KILLS)
				{
					botsGroup.AddEnemy(killer, EBotEnemyCause.byKill);
				}
			}
		}
		if (target.AIData.IsBossOrFollowerRequireRevenge() && !killer.AIData.IsAI && !target.Loyalty.CanBeFreeKilled)
		{
			AddEnemyToAllGroupsInBotZone(killer, target, target);
		}
	}

	public void method_12(DamageInfoStruct damageInfo, Player target)
	{
		try
		{
			IPlayerOwner player = damageInfo.Player;
			if (player == null || target == null || player.IsAI || target.Loyalty.CanBeFreeKilled)
			{
				return;
			}
			AddEnemyToAllGroupsInBotZone(player.iPlayer, target, target);
			if (!target.IsAI)
			{
				Player alivePlayerByProfileID = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.iPlayer.ProfileId);
				if (alivePlayerByProfileID != null)
				{
					alivePlayerByProfileID.Loyalty.MarkAsCanBeFreeKilled();
				}
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
	}

	public void AddEnemyToAllGroupsInBotZone(IPlayer aggressor, IPlayer groupOwner, IPlayer target)
	{
		if (!groupOwner.IsAI)
		{
			return;
		}
		BotZone botZone = groupOwner.AIData.BotOwner.BotsGroup.BotZone;
		foreach (KeyValuePair<BotZone, GClass575> item in Groups())
		{
			if (item.Key != botZone)
			{
				continue;
			}
			foreach (BotsGroup group in item.Value.GetGroups(notNull: true))
			{
				if (!group.HaveFollowTarget(aggressor) && !group.Enemies.ContainsKey(aggressor) && !group.HaveMemberWithRole(WildSpawnType.gifter) && !BotSettingsRepoClass.IsSectant(group.InitialBotType) && !group.InitialFileSettings.Boss.NOT_ADD_TO_ENEMY_ON_KILLS && group.ShallRevengeFor(target))
				{
					group.AddEnemy(aggressor, EBotEnemyCause.AddEnemyToAllGroupsInBotZone);
				}
			}
		}
	}

	public void AddEnemyToAllGroups(IPlayer aggressor, IPlayer groupOwner, IPlayer target)
	{
		foreach (KeyValuePair<BotZone, GClass575> item in Groups())
		{
			foreach (BotsGroup group in item.Value.GetGroups(notNull: true))
			{
				if (!group.HaveFollowTarget(aggressor) && !group.Enemies.ContainsKey(aggressor) && !group.InitialFileSettings.Boss.NOT_ADD_TO_ENEMY_ON_KILLS && group.ShallRevengeFor(target))
				{
					group.AddEnemy(aggressor, EBotEnemyCause.AddEnemyToAllGroups);
				}
			}
		}
	}

	public BotZone GetClosestZone(Vector3 position, out float dist)
	{
		return BotSpawner_1.GetClosestZone(position, out dist);
	}

	public void StopGettingInfo()
	{
		Connections.Stop();
		SpawnControlScenario?.Dispose();
		BotSpawner_1?.Stop();
		if (BotGame != null)
		{
			BotGame.UpdateByUnity -= method_0;
		}
		Singleton<BotEventHandler>.Instance?.Stop();
		ArtilleryZonesController.Dispose();
		PlantedMines.Dispose();
		CutController.Dispose();
		Bots.Stop();
		EventsController.Dispose();
		AICoreController.Stop();
		AiTaskManager.Dispose();
		BotTradersServices_1.Dispose();
		StaticManager.Instance.TimerManager.StopAllTimers();
	}

	public void DestroyInfo(Player player)
	{
		if (BotSpawner_1 != null)
		{
			BotSpawner_1.DeletePlayer(player);
			player.AIData.Dispose();
		}
	}

	public void AddActivePLayer(Player player)
	{
		if (BotSpawner_1 != null)
		{
			BotSpawner_1.AddPlayer(player);
		}
	}

	[CanBeNull]
	public BotOwner ClosestBotToPoint(Vector3 p)
	{
		return BotSpawner_1.ClosestBotToPoint(p);
	}

	public void SetSettings(int maxCount, BotPresetClass[] botPresets, GClass612[] botScatterings)
	{
		BotPresets = botPresets;
		BotScatterings = botScatterings;
		if (DebugBotData.UseDebugData)
		{
			MaxCount = DebugBotData.Instance.MaxBotsCount;
		}
		else
		{
			MaxCount = maxCount;
		}
		if (BotSpawner_1 != null)
		{
			BotSpawner_1.SetMaxBots(MaxCount);
			ZonesLeaveController.SetMaxBots(MaxCount);
		}
	}

	public static BotsController FindBotControllerEditorOnly()
	{
		return Singleton<IBotGame>.Instance?.BotsController;
	}

	public bool DebugUpdateSettingsToAllBots()
	{
		if (Bots.Count > 0 && !LocalBotSettingsProviderClass.LoadExternal())
		{
			UnityEngine.Debug.LogError("can't load external files");
			return false;
		}
		foreach (BotOwner botOwner in Bots.BotOwners)
		{
			botOwner.Settings.DebugUpdateSettingsExternal(IsPvE);
		}
		return true;
	}

	public bool DebugChangeParameter(string cls, string prm, object val)
	{
		foreach (BotOwner botOwner in Bots.BotOwners)
		{
			if (!botOwner.Settings.DebugChangeParameter(cls, prm, val))
			{
				return false;
			}
		}
		return true;
	}

	public void SpawnBossDebug()
	{
		if (BotSpawner_1 != null)
		{
			BotSpawner_1.BossSpawner.InitBossSpawnDebug();
		}
	}

	public void ResetGameDateTime(int hour)
	{
		DateTime dateTime = BotSpawner_1.BotGame.GameDateTime.Calculate();
		DateTime gameDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, Mathf.Min(hour, 23), dateTime.Minute, dateTime.Second, DateTimeKind.Utc);
		BotSpawner_1.BotGame.GameDateTime.Reset(gameDateTime);
	}

	public List<Player> GetAllBossPlayers()
	{
		return BotSpawner_1.GetAllBossPLayers();
	}

	public void BotDied(BotOwner botOwner)
	{
		if (BotSpawner_1 != null)
		{
			BotSpawner_1.BotDied(botOwner);
		}
	}

	public void DebugSpawnServerAnyway()
	{
		BotSpawner_1.DebugSpawnAnyway();
	}

	public void SetAllPlayersPursuitDebug()
	{
		BotSpawner_1.SetAllPlayersPursuitDebug();
	}

	public void DebugLogsAboultRemoveEnemies()
	{
		BotSpawner_1.DebugLogsAboultRemoveEnemies();
	}

	public string FullBotsCountInfo()
	{
		return BotSpawner_1.FullBotsCountInfo();
	}

	public BotSpawner GetSpawner()
	{
		return BotSpawner_1;
	}

	public void DevelopmentTeleportBot(Vector3 requestedPosition)
	{
		List<BotOwner> list = Bots.BotOwners.ToList();
		if (list.Count <= 0 || !NavMesh.SamplePosition(requestedPosition, out var hit, 20f, -1))
		{
			return;
		}
		NavGraphVoxelSimple voxelSafe = CoversData.GetVoxelSafe(requestedPosition);
		GroupPoint closetsGroupPoint = CoversData.GetClosestsPointInVoxelesExtended(voxelSafe.IndexX, voxelSafe.IndexY, voxelSafe.IndexZ, 10, requestedPosition, null);
		if (closetsGroupPoint == null)
		{
			return;
		}
		List<BotOwner> botsWithSameCG = list.Where((BotOwner x) => x.CoverSearchInfo.ConnectionGroupId == closetsGroupPoint.ConnectionGroup).ToList();
		if (botsWithSameCG.Count > 0)
		{
			int nearEntity = GClass369.GetNearEntity(botsWithSameCG.Count, (int i) => botsWithSameCG[i].Position - requestedPosition);
			BotOwner botOwner = botsWithSameCG[nearEntity];
			NavMeshPath navMeshPath = new NavMeshPath();
			if (NavMesh.CalculatePath(hit.position, closetsGroupPoint.Position, -1, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
			{
				botOwner.Memory.Spotted(byHit: false);
				botOwner.Memory.BotCurrentCoverInfo.SetCover(closetsGroupPoint.GetById(botOwner.Id));
				botOwner.Mover.Stop();
				botOwner.Mover.Teleport(hit.position);
				botOwner.Transform.position = hit.position;
			}
		}
	}

	public void DevelopmentTeleportBot(BotOwner ownerToMove, Vector3 requestedPosition)
	{
		if (NavMesh.SamplePosition(requestedPosition, out var hit, 20f, -1))
		{
			ownerToMove.Memory.Spotted(byHit: false);
			ownerToMove.Mover.Stop();
			ownerToMove.GetPlayer.Teleport(hit.position);
		}
	}
}
