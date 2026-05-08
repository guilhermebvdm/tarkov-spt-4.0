using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using CommonAssets.Scripts.Game;
using EFT.AssetsManager;
using EFT.Bots;
using EFT.CameraControl;
using EFT.EnvironmentEffect;
using EFT.Game.Spawning;
using EFT.Interactive;
using EFT.Interactive.SecretExfiltrations;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.Screens;
using EFT.Utilities;
using EFT.Weather;
using JsonType;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace EFT;

public abstract class BaseLocalGame<TPlayerOwner> : AbstractGame, EndByExitTrigerScenario.GInterface146, IGame, EndByTimerScenario.Interface8 where TPlayerOwner : EftGamePlayerOwner
{
	public class Class1630
	{
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1631
	{
		public static readonly Class1631 class1631_0 = new Class1631();

		public static Func<float> func_0;

		public static Func<float> func_1;

		public static Func<float> func_2;

		public static Func<float> func_3;

		public static Func<float> func_4;

		public static Func<LootItemPositionClass, bool> func_5;

		public static Func<LootItemPositionClass, Item> func_6;

		public static Func<Item, bool> func_7;

		public static Func<Item, IEnumerable<ResourceKey>> func_8;

		public static Func<LootItem, Item> func_9;

		public static Func<LootableContainer, Item> func_10;

		public static Func<InsuredItemClass, string> func_11;

		public static Action action_0;

		public float method_0()
		{
			return Time.time;
		}

		public float method_1()
		{
			return Singleton<SharedGameSettingsClass>.Instance.Control.Settings.MouseSensitivity;
		}

		public float method_2()
		{
			return Singleton<SharedGameSettingsClass>.Instance.Control.Settings.MouseAimingSensitivity;
		}

		public float method_3()
		{
			return Singleton<SharedGameSettingsClass>.Instance.Control.Settings.MouseSensitivity;
		}

		public float method_4()
		{
			return Singleton<SharedGameSettingsClass>.Instance.Control.Settings.MouseAimingSensitivity;
		}

		public bool method_5(LootItemPositionClass x)
		{
			return x.Item is LootContainerItemClass;
		}

		public Item method_6(LootItemPositionClass x)
		{
			return x.Item;
		}

		public bool method_7(Item x)
		{
			return !(x is GClass3248);
		}

		public IEnumerable<ResourceKey> method_8(Item x)
		{
			return x.Template.AllResources;
		}

		public Item method_9(LootItem x)
		{
			return x.ItemOwner.RootItem;
		}

		public Item method_10(LootableContainer x)
		{
			return x.ItemOwner.RootItem;
		}

		public string method_11(InsuredItemClass item)
		{
			return item.ItemId;
		}

		public void method_12()
		{
		}
	}

	[CompilerGenerated]
	public class Class1632<U> where U : BaseLocalGame<TPlayerOwner>
	{
		public U game;

		public IInputTree inputTree;

		public InsuranceCompanyClass insurance;

		public ISession backEndSession;

		public GameUI gameUI;

		public LocationSettingsClass.Location location;

		public TPlayerOwner method_0(LocalPlayer player)
		{
			game.LocalPlayer_0 = player;
			TPlayerOwner val = EftGamePlayerOwner.Create<TPlayerOwner>(player, inputTree, insurance, backEndSession, gameUI, game.GameDateTime, location);
			val.OnLeave += game.vmethod_4;
			return val;
		}
	}

	[CompilerGenerated]
	public class Class1633
	{
		public ISpawnSystem spawnSystem;

		public BaseLocalGame<TPlayerOwner> baseLocalGame_0;

		public async Task<LocalPlayer> method_0()
		{
			ISpawnPoint spawnPoint = spawnSystem.SelectSpawnPoint(ESpawnCategory.Player, baseLocalGame_0.Profile_0.Info.Side, null, null, null, null, baseLocalGame_0.Profile_0.Id);
			baseLocalGame_0.string_0 = spawnPoint.Infiltration;
			int playerId = ++baseLocalGame_0.int_0;
			Player.EUpdateMode armsUpdateMode = Player.EUpdateMode.Auto;
			if (BackendConfigAbstractClass.Config.UseHandsFastAnimator)
			{
				armsUpdateMode = Player.EUpdateMode.Manual;
			}
			if (baseLocalGame_0.localRaidSettings_0 != null && baseLocalGame_0.localRaidSettings_0.mode == ELocalMode.PVE_OFFLINE)
			{
				string[] accessKeys = baseLocalGame_0.localRaidSettings_0.selectedLocation?.AccessKeys;
				if (accessKeys != null && accessKeys.Length != 0)
				{
					Item item = baseLocalGame_0.Profile_0.Inventory.GetPlayerItems(EPlayerItems.Equipment)?.FirstOrDefault((Item equippedItem) => accessKeys.Contains(equippedItem.StringTemplateId));
					if (item != null)
					{
						baseLocalGame_0.method_6(baseLocalGame_0.Profile_0, item.Id);
					}
				}
			}
			LocalPlayer obj = await baseLocalGame_0.vmethod_3(baseLocalGame_0.GameWorld_0, playerId, spawnPoint.Position, spawnPoint.Rotation, "Player", "", EPointOfView.FirstPerson, baseLocalGame_0.Profile_0, aiControl: false, baseLocalGame_0.UpdateQueue, armsUpdateMode, Player.EUpdateMode.Auto, BackendConfigAbstractClass.Config.CharacterController.ClientPlayerMode, () => Singleton<SharedGameSettingsClass>.Instance.Control.Settings.MouseSensitivity, () => Singleton<SharedGameSettingsClass>.Instance.Control.Settings.MouseAimingSensitivity, new GClass2268(), baseLocalGame_0.iSession, (baseLocalGame_0.localRaidSettings_0 != null) ? baseLocalGame_0.localRaidSettings_0.mode : ELocalMode.TRAINING);
			obj.Location = baseLocalGame_0.Location_0.Id;
			obj.OnEpInteraction += baseLocalGame_0.OnEpInteraction;
			obj.OnStatisticsShot = (Action<Item, AmmoItemClass>)Delegate.Combine(obj.OnStatisticsShot, new Action<Item, AmmoItemClass>(baseLocalGame_0.method_5));
			return obj;
		}
	}

	[CompilerGenerated]
	public class Class1634
	{
		public string[] accessKeys;

		public bool method_0(Item equippedItem)
		{
			return accessKeys.Contains(equippedItem.StringTemplateId);
		}
	}

	[CompilerGenerated]
	public class Class1635
	{
		public string keyId;

		public bool method_0(Item item)
		{
			return item.Id == keyId;
		}
	}

	[CompilerGenerated]
	public class Class1636
	{
		public BaseLocalGame<TPlayerOwner> baseLocalGame_0;

		public string profileId;

		public ExitStatus exitStatus;

		public string exitName;

		public float delay;

		public void method_0()
		{
			baseLocalGame_0.gameUI_0.TimerPanel.Close();
			if (baseLocalGame_0.gparam_0 != null)
			{
				baseLocalGame_0.gparam_0.vmethod_1();
			}
			CurrentScreenSingletonClass.Instance.CloseAllScreensForced();
			baseLocalGame_0.method_15(profileId, exitStatus, exitName, delay).HandleExceptions();
		}
	}

	[CompilerGenerated]
	public class Class1637
	{
		public BaseLocalGame<TPlayerOwner> baseLocalGame_0;

		public ExitStatus exitStatus;

		public TimeSpan duration;

		public void method_0()
		{
			baseLocalGame_0.callback_0(new Result<ExitStatus, TimeSpan, MetricsClass>(exitStatus, duration, baseLocalGame_0.vmethod_7()));
			UIEventSystem.Instance.Enable();
			if (baseLocalGame_0.gclass24_0 != null)
			{
				baseLocalGame_0.gclass24_0.Dispose();
				baseLocalGame_0.gclass24_0 = null;
			}
		}
	}

	private bool bool_2;

	[CompilerGenerated]
	private GameDateTime gameDateTime_0;

	[CompilerGenerated]
	private GameWorld gameWorld_0;

	[CompilerGenerated]
	private LocalPlayer localPlayer_0;

	protected DateTime dateTime_0;

	private GameDateTime gameDateTime_1;

	private GameUI gameUI_0;

	protected ISession iSession;

	private Callback<ExitStatus, TimeSpan, MetricsClass> callback_0;

	private EndByExitTrigerScenario endByExitTrigerScenario_0;

	private EndByTimerScenario endByTimerScenario_0;

	private Func<Task<LocalPlayer>> func_0;

	private Func<LocalPlayer, TPlayerOwner> func_1;

	protected TPlayerOwner gparam_0;

	protected GClass24 gclass24_0;

	protected MetricsCollectorClass metricsCollectorClass;

	protected MetricsEventsClass metricsEventsClass;

	protected LocalRaidSettings localRaidSettings_0;

	private Action action_2;

	private DateTime dateTime_1;

	private EDateTime edateTime_0;

	private string string_0;

	protected readonly Dictionary<string, Player> dictionary_0 = new Dictionary<string, Player>();

	protected LocalGameLoggerClass localGameLoggerClass;

	private int int_0;

	private readonly Dictionary<string, DateTime> dictionary_1 = new Dictionary<string, DateTime>
	{
		{
			"factory4_day",
			new DateTime(2016, 8, 4, 15, 28, 0, DateTimeKind.Utc)
		},
		{
			"factory4_night",
			new DateTime(2016, 8, 4, 3, 28, 0, DateTimeKind.Utc)
		}
	};

	protected readonly BotsController botsController_0 = new BotsController();

	[CompilerGenerated]
	private Profile profile_0;

	[CompilerGenerated]
	private LocationSettingsClass.Location location_0;

	[CompilerGenerated]
	private Action action_3;

	public GameDateTime GameDateTime
	{
		[CompilerGenerated]
		get
		{
			return gameDateTime_0;
		}
		[CompilerGenerated]
		set
		{
			gameDateTime_0 = value;
		}
	}

	public GameWorld GameWorld_0
	{
		[CompilerGenerated]
		get
		{
			return gameWorld_0;
		}
		[CompilerGenerated]
		set
		{
			gameWorld_0 = value;
		}
	}

	public LocalPlayer LocalPlayer_0
	{
		[CompilerGenerated]
		get
		{
			return localPlayer_0;
		}
		[CompilerGenerated]
		set
		{
			localPlayer_0 = value;
		}
	}

	public TPlayerOwner PlayerOwner => gparam_0;

	public Profile Profile_0
	{
		[CompilerGenerated]
		get
		{
			return profile_0;
		}
		[CompilerGenerated]
		set
		{
			profile_0 = value;
		}
	}

	public LocationSettingsClass.Location Location_0
	{
		[CompilerGenerated]
		get
		{
			return location_0;
		}
		[CompilerGenerated]
		set
		{
			location_0 = value;
		}
	}

	public override string LocationObjectId => Location_0._Id;

	public override GameUI GameUi => gameUI_0;

	public override string ProfileId => Profile_0.Id;

	public List<Player> AllPlayers
	{
		get
		{
			if (gparam_0 != null && gparam_0.Player != null)
			{
				return new List<Player> { gparam_0.Player };
			}
			return new List<Player>();
		}
	}

	GameObject IGame.gameObject => base.gameObject;

	public event Action UpdateByUnity
	{
		[CompilerGenerated]
		add
		{
			Action action = action_3;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_3;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public static T smethod_0<T>(IInputTree inputTree, Profile profile, GameWorld gameWorld, GameDateTime backendDateTime, InsuranceCompanyClass insurance, GameUI gameUI, LocationSettingsClass.Location location, TimeAndWeatherSettings timeAndWeather, WavesSettings wavesSettings, EDateTime dateTime, Callback<ExitStatus, TimeSpan, MetricsClass> callback, float fixedDeltaTime, EUpdateQueue updateQueue, ISession backEndSession, TimeSpan? sessionTime, MetricsEventsClass metricsEvents, MetricsCollectorClass metricsCollector, LocalRaidSettings raidSettings) where T : BaseLocalGame<TPlayerOwner>
	{
		float num = 1.5f;
		WildSpawnWave[] waves = location.waves;
		foreach (WildSpawnWave obj in waves)
		{
			obj.slots_min = (int)((float)obj.slots_min * num);
			obj.slots_max = (int)((float)obj.slots_max * num);
		}
		T game = AbstractGame.Create<T>(updateQueue, sessionTime);
		game.GameWorld_0 = gameWorld;
		game.bool_2 = BackendConfigAbstractClass.Config.UseSpiritPlayer;
		BackendConfigAbstractClass.Config.UseSpiritPlayer = false;
		game.iSession = backEndSession;
		game.gameUI_0 = gameUI;
		game.callback_0 = callback;
		game.Profile_0 = profile;
		game.Location_0 = location;
		game.edateTime_0 = dateTime;
		game.FixedDeltaTime = fixedDeltaTime;
		smethod_1<T>(location, wavesSettings);
		if (!Singleton<BotEventHandler>.Instantiated)
		{
			Singleton<BotEventHandler>.Create(new BotEventHandler());
		}
		game.endByExitTrigerScenario_0 = EndByExitTrigerScenario.Create(game);
		game.endByTimerScenario_0 = EndByTimerScenario.smethod_0(game);
		game.gameDateTime_1 = backendDateTime;
		game.metricsEventsClass = metricsEvents;
		game.metricsCollectorClass = metricsCollector;
		game.localRaidSettings_0 = raidSettings;
		game.method_3(timeAndWeather);
		game.func_1 = delegate(LocalPlayer player)
		{
			game.LocalPlayer_0 = player;
			TPlayerOwner val = EftGamePlayerOwner.Create<TPlayerOwner>(player, inputTree, insurance, backEndSession, gameUI, game.GameDateTime, location);
			val.OnLeave += game.vmethod_4;
			return val;
		};
		WorldInteractiveObject.InteractionShouldBeConfirmed = false;
		game.vmethod_0();
		return game;
	}

	public static void smethod_1<T>(LocationSettingsClass.Location location, WavesSettings wavesSettings) where T : BaseLocalGame<TPlayerOwner>
	{
		location.OldSpawn = location.OfflineOldSpawn;
		location.NewSpawn = location.OfflineNewSpawn;
		float num = 1f;
		switch (wavesSettings.BotAmount)
		{
		case EBotAmount.NoBots:
		case EBotAmount.Low:
			num = ((Singleton<BackendConfigSettingsClass>.Instance != null) ? Singleton<BackendConfigSettingsClass>.Instance.WAVE_COEF_LOW : LocalBotSettingsProviderClass.Core.WAVE_COEF_LOW);
			break;
		case EBotAmount.Medium:
			num = ((Singleton<BackendConfigSettingsClass>.Instance != null) ? Singleton<BackendConfigSettingsClass>.Instance.WAVE_COEF_MID : LocalBotSettingsProviderClass.Core.WAVE_COEF_MID);
			break;
		case EBotAmount.High:
			num = ((Singleton<BackendConfigSettingsClass>.Instance != null) ? Singleton<BackendConfigSettingsClass>.Instance.WAVE_COEF_HIGH : LocalBotSettingsProviderClass.Core.WAVE_COEF_HIGH);
			break;
		case EBotAmount.Horde:
			num = ((Singleton<BackendConfigSettingsClass>.Instance != null) ? Singleton<BackendConfigSettingsClass>.Instance.WAVE_COEF_HORDE : LocalBotSettingsProviderClass.Core.WAVE_COEF_HORDE);
			break;
		}
		location.BotMax = (int)((float)location.BotMax * num);
	}

	public void Update()
	{
		action_3?.Invoke();
	}

	public virtual void vmethod_0()
	{
	}

	public void method_3(TimeAndWeatherSettings timeAndWeather)
	{
		System.Random random = new System.Random();
		if (timeAndWeather.IsRandomTime)
		{
			dateTime_1 = new DateTime(2016, 4, 30, random.Next(1, 24), random.Next(1, 59), 0, DateTimeKind.Utc);
		}
		else if (!dictionary_1.TryGetValue(Location_0.Id, out dateTime_1))
		{
			dateTime_1 = ((edateTime_0 == EDateTime.CURR) ? gameDateTime_1.Calculate() : gameDateTime_1.Calculate().AddHours(12.0));
		}
		GameDateTime = new GameDateTime(gameDateTime_1.DateTime_0, dateTime_1, gameDateTime_1.TimeFactor, gameDateTime_1.Boolean_0);
		GameWorld_0.GameDateTime = GameDateTime;
		if (WeatherController.Instance != null || MonoBehaviourSingleton<TODSkySimple>.Instance != null)
		{
			GClass4.Instance.CurrentTime.GameDateTime = GameDateTime;
			WeatherClass[] randomTestWeatherNodes = WeatherClass.GetRandomTestWeatherNodes();
			if (!timeAndWeather.IsRandomWeather)
			{
				long time = randomTestWeatherNodes[0].Time;
				randomTestWeatherNodes[0] = iSession.Weather;
				randomTestWeatherNodes[0].Time = time;
			}
			if (WeatherController.Instance != null)
			{
				WeatherController.Instance.method_0(randomTestWeatherNodes);
			}
		}
	}

	public async Task method_4(BotControllerSettings botsSettings, string backendUrl, InventoryController inventoryController)
	{
		base.Status = GameStatus.Running;
		Singleton<GameWorld>.Instance.RegisterRestrictableZones();
		UnityEngine.Random.InitState((int)EFTDateTimeClass.Now.Ticks);
		LocationSettingsClass.Location location = (Location_0.IsHideout ? Location_0 : localRaidSettings_0.selectedLocation);
		Singleton<GameWorld>.Instance.LocationId = Location_0.Id;
		SpawnPointManagerClass spawnPoints = SpawnPointManagerClass.CreateFromScene(EFTDateTimeClass.LocalDateTimeFromUnixTime(location.UnixDateTime), location.SpawnPointParams);
		int spawnSafeDistance = ((location.SpawnSafeDistanceMeters > 0) ? location.SpawnSafeDistanceMeters : 100);
		SpawnSettingsStruct settings = new SpawnSettingsStruct(location.MinDistToFreePoint, location.MaxDistToFreePoint, location.MaxBotPerZone, spawnSafeDistance, location.NoGroupSpawn, location.OneTimeSpawn);
		ISpawnSystem spawnSystem = SpawnSystemCreatorClass.CreateSpawnSystem(settings, () => Time.time, GameWorld_0, botsController_0, spawnPoints);
		BackendConfigSettingsClass instance = Singleton<BackendConfigSettingsClass>.Instance;
		if (instance != null && instance.ArtilleryShelling != null && instance.ArtilleryShelling.ArtilleryMapsConfigs != null && instance.ArtilleryShelling.ArtilleryMapsConfigs.Keys.Contains(location.Id))
		{
			Singleton<GameWorld>.Instance.ServerShellingController = new ServerShellingControllerClass();
			Singleton<GameWorld>.Instance.ClientShellingController = new ClientShellingControllerClass(hasAuthority: true);
		}
		if (instance != null && instance.EventSettings.EventActive && !instance.EventSettings.LocationsToIgnore.Contains(location._Id))
		{
			GameObject gameObject = (GameObject)Resources.Load("Prefabs/HALLOWEEN_CONTROLLER");
			if (gameObject != null)
			{
				GClass6.InstantiatePrefab(base.transform, gameObject);
			}
			else
			{
				UnityEngine.Debug.LogError("Can't find event prefab in resources. Path : Prefabs/HALLOWEEN_CONTROLLER");
			}
		}
		if (instance != null && instance.BTRSettings.LocationsWithBTR.Contains(location.Id))
		{
			Singleton<GameWorld>.Instance.BtrController = new BTRControllerClass(Singleton<GameWorld>.Instance);
		}
		if (!Location_0.IsHideout && instance?.transitSettings?.active == true)
		{
			Singleton<GameWorld>.Instance.TransitController = new LocalGameTransitControllerClass(instance.transitSettings, location.transitParameters, Profile_0, localRaidSettings_0);
		}
		else
		{
			TransitControllerAbstractClass.DisableTransitPoints();
		}
		if (!Location_0.IsHideout && instance?.runddansSettings?.active == true)
		{
			Singleton<GameWorld>.Instance.RunddansController = new LocalGameRunddansControllerClass(instance.runddansSettings, location);
		}
		else
		{
			RunddansControllerAbstractClass.ToggleEventEnvironment(isOn: false);
		}
		Singleton<GameWorld>.Instance.ClientBroadcastSyncController = new ClientBroadcastSyncControllerClass();
		ApplicationConfigClass config = BackendConfigAbstractClass.Config;
		if (config.FixedFrameRate > 0f)
		{
			base.FixedDeltaTime = 1f / config.FixedFrameRate;
		}
		func_0 = async delegate
		{
			ISpawnPoint spawnPoint = spawnSystem.SelectSpawnPoint(ESpawnCategory.Player, Profile_0.Info.Side, null, null, null, null, Profile_0.Id);
			string_0 = spawnPoint.Infiltration;
			int playerId = ++int_0;
			Player.EUpdateMode armsUpdateMode = Player.EUpdateMode.Auto;
			if (BackendConfigAbstractClass.Config.UseHandsFastAnimator)
			{
				armsUpdateMode = Player.EUpdateMode.Manual;
			}
			if (localRaidSettings_0 != null && localRaidSettings_0.mode == ELocalMode.PVE_OFFLINE)
			{
				string[] accessKeys = localRaidSettings_0.selectedLocation?.AccessKeys;
				if (accessKeys != null && accessKeys.Length != 0)
				{
					Item item = Profile_0.Inventory.GetPlayerItems(EPlayerItems.Equipment)?.FirstOrDefault((Item equippedItem) => accessKeys.Contains(equippedItem.StringTemplateId));
					if (item != null)
					{
						method_6(Profile_0, item.Id);
					}
				}
			}
			LocalPlayer obj = await vmethod_3(GameWorld_0, playerId, spawnPoint.Position, spawnPoint.Rotation, "Player", "", EPointOfView.FirstPerson, Profile_0, aiControl: false, base.UpdateQueue, armsUpdateMode, Player.EUpdateMode.Auto, BackendConfigAbstractClass.Config.CharacterController.ClientPlayerMode, () => Singleton<SharedGameSettingsClass>.Instance.Control.Settings.MouseSensitivity, () => Singleton<SharedGameSettingsClass>.Instance.Control.Settings.MouseAimingSensitivity, new GClass2268(), iSession, (localRaidSettings_0 != null) ? localRaidSettings_0.mode : ELocalMode.TRAINING);
			obj.Location = Location_0.Id;
			obj.OnEpInteraction += base.OnEpInteraction;
			obj.OnStatisticsShot = (Action<Item, AmmoItemClass>)Delegate.Combine(obj.OnStatisticsShot, new Action<Item, AmmoItemClass>(method_5));
			return obj;
		};
		using (CounterCreatorAbstractClass.StartWithToken("player create"))
		{
			metricsEventsClass?.SetPlayerSpawnEvent();
			LocalPlayer localPlayer = await func_0();
			dictionary_0.Add(localPlayer.ProfileId, localPlayer);
			gparam_0 = func_1(localPlayer);
			PlayerCameraController.Create(gparam_0.Player);
			CameraClass.Instance.SetOcclusionCullingEnabled(Location_0.OcculsionCullingEnabled);
			CameraClass.Instance.IsActive = false;
		}
		await method_12(location);
		await vmethod_1(botsSettings, spawnSystem);
		if (Singleton<IBotGame>.Instantiated)
		{
			Singleton<IBotGame>.Instance.BotsController.CoversData.Patrols.RestoreLoot(location.Loot, LocationScene.GetAllObjects<LootableContainer>());
		}
		AirdropEventClass airdropEventClass = new AirdropEventClass();
		airdropEventClass.AirdropParameters = Location_0.airdropParameters;
		airdropEventClass.Init(Singleton<AbstractGame>.Instance.GameType == EGameType.Offline);
		(Singleton<GameWorld>.Instance as ClientGameWorld).ClientSynchronizableObjectLogicProcessor.ServerAirdropManager = airdropEventClass;
		await method_7();
	}

	public void method_5(Item item, AmmoItemClass ammo)
	{
		if (item is Weapon weapon)
		{
			gparam_0.Player.StatisticsManager.OnShot(weapon, ammo);
		}
	}

	public void method_6(Profile profile, string keyId)
	{
		if (string.IsNullOrEmpty(keyId))
		{
			return;
		}
		Item item = null;
		Item item2 = profile.Inventory.GetPlayerItems(EPlayerItems.Equipment).FirstOrDefault((Item item3) => item3.Id == keyId);
		if (item2 != null)
		{
			KeyComponent itemComponent = item2.GetItemComponent<KeyComponent>();
			if (itemComponent != null)
			{
				if (itemComponent.Template.MaximumNumberOfUsage != 0 && ++itemComponent.NumberOfUsages == itemComponent.Template.MaximumNumberOfUsage)
				{
					item = item2;
				}
			}
			else
			{
				item = item2;
			}
		}
		if (item != null && item.Parent.Container is StashGridClass stashGridClass)
		{
			GStruct154<GClass3413> gStruct = stashGridClass.Remove(item, simulate: false);
			if (gStruct.Failed)
			{
				UnityEngine.Debug.LogError(gStruct.Error);
			}
		}
	}

	public virtual async Task vmethod_1(BotControllerSettings controllerSettings, ISpawnSystem spawnSystem)
	{
	}

	public Task method_7()
	{
		MemoryControllerClass.RunHeapPreAllocation();
		MemoryControllerClass.Collect(force: true);
		if (MemoryControllerClass.Settings.OverrideRamCleanerSettings ? MemoryControllerClass.Settings.RamCleanerEnabled : ((bool)Singleton<SharedGameSettingsClass>.Instance.Game.Settings.AutoEmptyWorkingSet))
		{
			MemoryControllerClass.EmptyWorkingSet();
		}
		MemoryControllerClass.GCEnabled = false;
		Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
		CameraClass.Instance.IsActive = true;
		TaskCompletionSource taskCompletionSource = new TaskCompletionSource();
		StartCoroutine(vmethod_5(taskCompletionSource.Complete));
		return taskCompletionSource.Task;
	}

	public abstract IEnumerator vmethod_2();

	public virtual async Task<LocalPlayer> vmethod_3(GameWorld gameWorld, int playerId, Vector3 position, Quaternion rotation, string layerName, string prefix, EPointOfView pointOfView, Profile profile, bool aiControl, EUpdateQueue updateQueue, Player.EUpdateMode armsUpdateMode, Player.EUpdateMode bodyUpdateMode, CharacterControllerSpawner.Mode characterControllerMode, Func<float> getSensitivity, Func<float> getAimingSensitivity, IStatisticsManager statisticsManager, ISession session, ELocalMode localMode)
	{
		if (!TransitControllerAbstractClass.IsTransit(profile.Id, out int _))
		{
			profile.SetSpawnedInSession(value: false);
		}
		return await LocalPlayer.Create(gameWorld, playerId, position, rotation, "Player", "", EPointOfView.FirstPerson, profile, aiControl: false, base.UpdateQueue, armsUpdateMode, Player.EUpdateMode.Auto, BackendConfigAbstractClass.Config.CharacterController.ClientPlayerMode, () => Singleton<SharedGameSettingsClass>.Instance.Control.Settings.MouseSensitivity, () => Singleton<SharedGameSettingsClass>.Instance.Control.Settings.MouseAimingSensitivity, new GClass2268(), new GClass1855(), session, localMode, isYourPlayer: true, isBot: false);
	}

	public void method_8(string backendUrl, string locationId, int variantId)
	{
		GInterface17 cache = iSession.Cache;
		if (cache == null)
		{
			return;
		}
		string text = backendUrl + $"/client/location/getLocalloot?locationId={locationId}&variantId={variantId}";
		if (cache.Exists(text))
		{
			return;
		}
		string path = "LocalLoot/" + Location_0.Id + variantId;
		LocationSettingsClass.GClass1419 gClass;
		try
		{
			gClass = JsonParserClass.ParseJsonTo<LocationSettingsClass.GClass1419>(GClass861.Load<TextAsset>(path).text, Array.Empty<JsonConverter>());
			if (gClass.BackendUrl != backendUrl)
			{
				return;
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
			return;
		}
		GClass648 obj = new GClass648
		{
			data = gClass.Location,
			crc = gClass.crc
		};
		try
		{
			string data = JsonParserClass.ToPrettyJson(obj);
			cache.Save(text, data);
			UnityEngine.Debug.Log($"location {locationId} variantId:{variantId} crc:{gClass.crc} uploaded to backend cache url:{text}");
		}
		catch (Exception exception2)
		{
			UnityEngine.Debug.LogException(exception2);
		}
	}

	public Task<LocationSettingsClass.Location> method_9(string backendUrl)
	{
		int num = UnityEngine.Random.Range(1, 6);
		return smethod_5(JsonParserClass.ParseJsonTo<LocationSettingsClass.GClass1419>(GClass861.Load<TextAsset>("LocalLoot/" + Location_0.Id + num).text, Array.Empty<JsonConverter>()).Location);
	}

	public void method_10(ExfiltrationPoint point, EExfiltrationStatus prevStatus)
	{
		UpdateExfiltrationUi(point, point.Entered.Any((Player x) => x.ProfileId == Profile_0.Id));
	}

	public virtual void vmethod_4()
	{
		ReconnectionScreen.GClass3885 gClass = new ReconnectionScreen.GClass3885(Profile_0, Location_0, ESideType.Pmc, returnAllowed: true, nextScreenAllowed: false, iSession);
		gClass.OnLeave += delegate
		{
			Stop(Profile_0.Id, ExitStatus.Left, null);
		};
		gClass.ShowScreen(EScreenState.Queued);
	}

	public virtual IEnumerator vmethod_5(Action runCallback)
	{
		yield return vmethod_2();
		using (CounterCreatorAbstractClass.StartWithToken("SessionRun"))
		{
			vmethod_6();
		}
		runCallback?.Invoke();
		if (metricsEventsClass != null)
		{
			metricsEventsClass.SetGameRunned();
			metricsEventsClass.SetGameSpawn();
			metricsEventsClass.SetGameSpawned();
			metricsEventsClass.SetGameStarting();
			metricsEventsClass.SetGameStarted();
		}
		metricsCollectorClass?.Start();
	}

	public virtual void vmethod_6()
	{
		base.GameTimer.Start();
		Spawn();
		SkillClass[] skills = Profile_0.Skills.Skills;
		for (int i = 0; i < skills.Length; i++)
		{
			skills[i].SetPointsEarnedInSession(0f);
		}
		if (string_0 != null)
		{
			Profile_0.Info.EntryPoint = string_0;
			ExfiltrationControllerClass.Instance.InitAllExfiltrationPoints(Location_0._Id, Location_0.exits, Location_0.SecretExits, justLoadSettings: false, Location_0.DisabledScavExits);
			ExfiltrationControllerClass.Instance.InitSecretExfils(gparam_0.Player);
			ExfiltrationPoint[] array = ExfiltrationControllerClass.Instance.EligiblePoints(Profile_0);
			SecretExfiltrationPoint[] array2 = ExfiltrationControllerClass.Instance.SecretEligiblePoints();
			gameUI_0.TimerPanel.SetTime(EFTDateTimeClass.UtcNow, Profile_0.Info.Side, GClass1893.SessionSeconds(base.GameTimer), array, array2);
			ExfiltrationPoint[] array3 = array;
			foreach (ExfiltrationPoint exfiltrationPoint in array3)
			{
				exfiltrationPoint.OnStatusChanged += method_10;
				UpdateExfiltrationUi(exfiltrationPoint, contains: false, initial: true);
			}
			SecretExfiltrationPoint[] array4 = array2;
			foreach (SecretExfiltrationPoint secretExfiltrationPoint in array4)
			{
				secretExfiltrationPoint.OnStatusChanged += method_10;
				secretExfiltrationPoint.OnStatusChanged += base.ShowNewSecretExit;
				UpdateExfiltrationUi(secretExfiltrationPoint, contains: false, initial: true);
			}
			if (TransitControllerAbstractClass.Exist<LocalGameTransitControllerClass>(out var transitController))
			{
				transitController.EnablePoints();
				transitController.UpdateTimers();
				transitController.HandleExits();
			}
		}
		endByExitTrigerScenario_0.Run();
		if (Location_0.EventTrapsData != null)
		{
			LabyrinthSyncableTrapClass.InitLabyrinthSyncableTraps(Location_0.EventTrapsData);
		}
		dateTime_0 = EFTDateTimeClass.Now;
		base.Status = GameStatus.Started;
		if (Singleton<IBotGame>.Instantiated)
		{
			Singleton<IBotGame>.Instance.BotsController.Bots.CheckActivation();
		}
		ConsoleScreen.ApplyStartCommands();
	}

	public virtual void Spawn()
	{
		LocalPlayer_0.HealthController.DiedEvent += delegate
		{
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			gparam_0.vmethod_1();
			LocalPlayer_0.HealthController.DiedEvent -= method_19;
			method_11();
		};
		gparam_0.vmethod_0();
	}

	public void method_11()
	{
		gameUI_0.BattleUiPanelDeath.Show(Profile_0, ExitStatus.Killed, EFTDateTimeClass.Now - dateTime_0);
		Stop(Profile_0.Id, ExitStatus.Killed, null, 5f);
	}

	public static bool smethod_2<T>(ref PlayerLoopSystem system, PlayerLoopSystem replacement)
	{
		if (system.type == typeof(T))
		{
			system = replacement;
			return true;
		}
		if (system.subSystemList != null)
		{
			for (int i = 0; i < system.subSystemList.Length; i++)
			{
				if (smethod_2<T>(ref system.subSystemList[i], replacement))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void smethod_3()
	{
	}

	public async Task method_12(LocationSettingsClass.Location location)
	{
		using (CounterCreatorAbstractClass.StartWithToken("SpawnLoot"))
		{
			if (BackendConfigAbstractClass.Config.NoLootForLocalGame)
			{
				foreach (LootItemPositionClass item in location.Loot.Where((LootItemPositionClass x) => x.Item is LootContainerItemClass).ToList())
				{
					LootContainerItemClass lootContainerItemClass = item.Item as LootContainerItemClass;
					StashGridClass[] grids = lootContainerItemClass.Grids;
					for (int num = 0; num < grids.Length; num++)
					{
						grids[num].RemoveAll();
					}
					Slot[] slots = lootContainerItemClass.Slots;
					for (int num = 0; num < slots.Length; num++)
					{
						slots[num].RemoveItem();
					}
				}
			}
			Item[] source = location.Loot.Select((LootItemPositionClass x) => x.Item).ToArray();
			ResourceKey[] array = GClass3380.GetAllItemsFromCollections(source.OfType<GClass3248>()).Concat(source.Where((Item x) => !(x is GClass3248))).SelectMany((Item x) => x.Template.AllResources)
				.ToArray();
			if (array.Length != 0)
			{
				PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
				GClass660.FindParentPlayerLoopSystem(currentPlayerLoop, typeof(EarlyUpdate.UpdateTextureStreamingManager), out var playerLoopSystem, out var index);
				PlayerLoopSystem[] array2 = new PlayerLoopSystem[playerLoopSystem.subSystemList.Length];
				if (index != -1)
				{
					Array.Copy(playerLoopSystem.subSystemList, array2, playerLoopSystem.subSystemList.Length);
					PlayerLoopSystem playerLoopSystem2 = new PlayerLoopSystem
					{
						updateDelegate = smethod_3,
						type = typeof(Class1630)
					};
					playerLoopSystem.subSystemList[index] = playerLoopSystem2;
					PlayerLoop.SetPlayerLoop(currentPlayerLoop);
				}
				await Singleton<PoolManagerClass>.Instance.LoadBundlesAndCreatePools(PoolManagerClass.PoolsCategory.Raid, PoolManagerClass.AssemblyType.Local, array, JobPriorityClass.General, new GClass1519<LoadingProgressStruct>(delegate(LoadingProgressStruct p)
				{
					SetMatchmakerStatus("Loading loot... " + p.Stage, p.Progress);
				}));
				if (index != -1)
				{
					Array.Copy(array2, playerLoopSystem.subSystemList, playerLoopSystem.subSystemList.Length);
					PlayerLoop.SetPlayerLoop(currentPlayerLoop);
				}
			}
			metricsEventsClass?.SetGamePooled();
			GClass1404 lootItems = GameWorld_0.method_4(location.Loot);
			GameWorld_0.method_5(lootItems, initial: true);
			gparam_0.Player.ManageGameQuests();
		}
	}

	public void StopGame()
	{
		Stop(Profile_0.Id, ExitStatus.MissingInAction, null);
	}

	void EndByTimerScenario.Interface8.StopGame()
	{
		//ILSpy generated this explicit interface implementation from .override directive in StopGame
		this.StopGame();
	}

	public void ItemPlaced(GClass2185 droppedItem, string profileId)
	{
	}

	public void BotDespawn(BotOwner botOwner)
	{
		Player getPlayer = botOwner.GetPlayer;
		botsController_0.BotDied(botOwner);
		botsController_0.DestroyInfo(getPlayer);
		AssetPoolObject.ReturnToPool(botOwner.gameObject);
	}

	public void ItemRemoved(GClass2185 droppedItem)
	{
	}

	public void StopSession(string profileId, ExitStatus exitStatus, string exitName)
	{
		Stop(profileId, exitStatus, exitName);
	}

	void EndByExitTrigerScenario.GInterface146.StopSession(string profileId, ExitStatus exitStatus, string exitName)
	{
		//ILSpy generated this explicit interface implementation from .override directive in StopSession
		this.StopSession(profileId, exitStatus, exitName);
	}

	public virtual void Stop(string profileId, ExitStatus exitStatus, string exitName, float delay = 0f)
	{
		if (profileId != Profile_0.Id || base.Status == GameStatus.Stopped || base.Status == GameStatus.Stopping)
		{
			return;
		}
		if (base.Status == GameStatus.Starting || base.Status == GameStatus.Started)
		{
			endByTimerScenario_0.GameStatus_0 = GameStatus.SoftStopping;
		}
		base.Status = GameStatus.Stopping;
		GClass1893.TryStop(base.GameTimer);
		endByExitTrigerScenario_0.Stop();
		gameUI_0.TimerPanel.Close();
		botsController_0.StopGettingInfo();
		botsController_0.DestroyInfo(gparam_0.Player);
		if (EnvironmentManager.Instance != null)
		{
			EnvironmentManager.Instance.Stop();
		}
		MonoBehaviourSingleton<PreloaderUI>.Instance.StartBlackScreenShow(1f, 1f, delegate
		{
			gameUI_0.TimerPanel.Close();
			if (gparam_0 != null)
			{
				gparam_0.vmethod_1();
			}
			CurrentScreenSingletonClass.Instance.CloseAllScreensForced();
			method_15(profileId, exitStatus, exitName, delay).HandleExceptions();
		});
		BackendConfigAbstractClass.Config.UseSpiritPlayer = bool_2;
	}

	public FlatItemsDataClass[] method_13()
	{
		GameWorld instance = Singleton<GameWorld>.Instance;
		Item[] array = (from x in instance.LootList.OfType<LootItem>()
			select x.ItemOwner.RootItem).Concat(from x in instance.LootList.OfType<LootableContainer>()
			select x.ItemOwner.RootItem).ToArray();
		Item item = Singleton<ItemFactoryClass>.Instance.CreateItem("FakeStash", "566abbc34bdc2d92178b4576", null);
		TraderControllerClass itemController = new TraderControllerClass(item, item.Name, item.Name);
		foreach (Corpse item2 in instance.LootList.OfType<Corpse>())
		{
			Slot[] slots = ((InventoryEquipment)item2.ItemOwner.RootItem).Slots;
			for (int num = 0; num < slots.Length; num++)
			{
				Item containedItem = slots[num].ContainedItem;
				if (containedItem != null && !containedItem.Template.NotShownInSlot && InteractionsHandlerClass.Remove(containedItem, itemController, simulate: true).Failed)
				{
					InteractionsHandlerClass.RemoveWithoutRestrictions(containedItem, item2.ItemOwner);
				}
			}
		}
		Item[] second = GClass3380.GetAllItemsFromCollections(array.OfType<GClass3248>()).ToArray();
		FlatItemsDataClass[] array2 = Singleton<ItemFactoryClass>.Instance.TreeToFlatItems(array.Concat(second).Distinct());
		HashSet<string> hashSet = new HashSet<string>(Profile_0.InsuredItems.Select((InsuredItemClass insuredItemClass) => insuredItemClass.ItemId));
		Dictionary<string, FlatItemsDataClass> dictionary = new Dictionary<string, FlatItemsDataClass>();
		FlatItemsDataClass[] array3 = array2;
		foreach (FlatItemsDataClass flatItemsDataClass in array3)
		{
			MongoID id = flatItemsDataClass._id;
			if (!hashSet.Contains(id))
			{
				continue;
			}
			if (dictionary.TryGetValue(id, out var value))
			{
				MongoID? parentId = value.parentId;
				if (!string.IsNullOrEmpty(parentId.HasValue ? ((string)parentId.GetValueOrDefault()) : null))
				{
					continue;
				}
			}
			dictionary[id] = flatItemsDataClass;
		}
		return dictionary.Values.ToArray();
	}

	public Dictionary<string, FlatItemsDataClass[]> method_14()
	{
		GameWorld instance = Singleton<GameWorld>.Instance;
		Dictionary<string, FlatItemsDataClass[]> dictionary = new Dictionary<string, FlatItemsDataClass[]>();
		BTRControllerClass btrController = instance.BtrController;
		if (btrController?.TransferItemsController.Stash != null)
		{
			StashItemClass stash = btrController.TransferItemsController.Stash;
			dictionary.Add(stash.Id + "_btr", Singleton<ItemFactoryClass>.Instance.TreeToFlatItems(stash));
		}
		if (TransitControllerAbstractClass.Exist<LocalGameTransitControllerClass>(out var transitController) && transitController?.TransferItemsController?.Stash != null)
		{
			StashItemClass stash2 = transitController.TransferItemsController.Stash;
			dictionary.Add(stash2.Id + "_transit", Singleton<ItemFactoryClass>.Instance.TreeToFlatItems(stash2));
		}
		return dictionary;
	}

	public async Task method_15(string profileId, ExitStatus exitStatus, string exitName, float delay)
	{
		CurrentScreenSingletonClass instance = CurrentScreenSingletonClass.Instance;
		if (instance.CheckCurrentScreen(EEftScreenType.Reconnect))
		{
			instance.CloseAllScreensForced();
		}
		Player player = gparam_0.Player;
		player.OnStatisticsShot = (Action<Item, AmmoItemClass>)Delegate.Remove(player.OnStatisticsShot, new Action<Item, AmmoItemClass>(method_5));
		gparam_0.Player.OnGameSessionEnd(exitStatus, base.PastTime, Location_0.Id, exitName);
		CleanUp();
		base.Status = GameStatus.Stopped;
		TimeSpan duration = EFTDateTimeClass.Now - dateTime_0;
		if (localRaidSettings_0.mode == ELocalMode.PVE_OFFLINE)
		{
			Profile_0.Health = ((exitStatus == ExitStatus.Transit) ? gparam_0.Player.ActiveHealthController.Store(Singleton<BackendConfigSettingsClass>.Instance.transitSettings) : gparam_0.Player.ActiveHealthController.Store());
			iSession.LastPlayerState = gparam_0.Player.Profile.GetVisualEquipmentState();
		}
		CompleteProfileDescriptorClass obj = new CompleteProfileDescriptorClass(Profile_0, GClass2240.Instance);
		RaidEndDescriptorClass results = new RaidEndDescriptorClass
		{
			profile = JsonParserClass.ToUnparsedData(obj),
			result = exitStatus,
			killerId = gparam_0.Player.KillerId,
			killerAid = gparam_0.Player.KillerAccountId,
			exitName = exitName,
			inSession = true,
			favorite = (Profile_0.Info.Side == EPlayerSide.Savage),
			playTime = (int)duration.TotalSeconds,
			ProfileId = Profile_0.Id
		};
		FlatItemsDataClass[] lostInsuredItems = method_13();
		Dictionary<string, FlatItemsDataClass[]> transferItems = method_14();
		try
		{
			await iSession.LocalRaidEnded(localRaidSettings_0, results, lostInsuredItems, transferItems);
		}
		catch (Exception exception)
		{
			MonoBehaviourSingleton<PreloaderUI>.Instance.ShowErrorScreen("Error", exception, delegate
			{
			});
		}
		MonoBehaviourSingleton<BetterAudio>.Instance.FadeOutVolumeAfterRaid();
		GClass855.WaitSeconds(StaticManager.Instance, delay, delegate
		{
			callback_0(new Result<ExitStatus, TimeSpan, MetricsClass>(exitStatus, duration, vmethod_7()));
			UIEventSystem.Instance.Enable();
			if (gclass24_0 != null)
			{
				gclass24_0.Dispose();
				gclass24_0 = null;
			}
		});
	}

	public virtual MetricsClass vmethod_7()
	{
		using (CounterCreatorAbstractClass.StartWithToken("CollectMetrics"))
		{
			metricsCollectorClass.Stop();
			MetricsClass metricsClass = new MetricsClass();
			GClass2611 metrics = metricsCollectorClass.Metrics;
			if (metrics != null)
			{
				metricsClass.sid = localRaidSettings_0.serverId;
				metricsClass.HardwareDescription = Class1467.smethod_1();
				metricsClass.Location = localRaidSettings_0.selectedLocation.Name;
				metricsClass.Metrics = metrics;
				metricsClass.ClientEvents = metricsEventsClass;
				metricsClass.mode = ((localRaidSettings_0.mode == ELocalMode.PVE_OFFLINE) ? EClientMetrics.OfflinePVE : EClientMetrics.Training);
				if (Singleton<SharedGameSettingsClass>.Instantiated)
				{
					SharedGameSettingsClass instance = Singleton<SharedGameSettingsClass>.Instance;
					metricsClass.Settings = instance.Graphics.Settings.Clone();
					metricsClass.SharedSettings = new GClass2639(instance.Game);
				}
			}
			if (gclass24_0 != null)
			{
				metricsClass.SpikeSamples = gclass24_0.SpikeSamples;
			}
			return metricsClass;
		}
	}

	public override void SetMatchmakerStatus(string status, float? progress = null)
	{
		InvokeMatchingStatusChanged(status, progress);
	}

	public virtual void CleanUp()
	{
		smethod_4(dictionary_0);
	}

	public static void smethod_4(IDictionary<string, Player> players)
	{
		foreach (Player value in players.Values)
		{
			try
			{
				value.Dispose();
				AssetPoolObject.ReturnToPool(value.gameObject);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}
		players.Clear();
	}

	public int method_16()
	{
		int_0++;
		return int_0;
	}

	public BaseLocalGame()
	{
	}

	[CompilerGenerated]
	public static Task<LocationSettingsClass.Location> smethod_5(GClass846 unparsedData)
	{
		return Task.FromResult(JsonParserClass.ParseJsonTo<LocationSettingsClass.Location>(unparsedData, Array.Empty<JsonConverter>()));
	}

	[CompilerGenerated]
	public bool method_17(Player x)
	{
		return x.ProfileId == Profile_0.Id;
	}

	[CompilerGenerated]
	public void method_18()
	{
		Stop(Profile_0.Id, ExitStatus.Left, null);
	}

	[CompilerGenerated]
	public void method_19(EDamageType controller)
	{
		// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
		gparam_0.vmethod_1();
		LocalPlayer_0.HealthController.DiedEvent -= method_19;
		method_11();
	}

	[CompilerGenerated]
	public void method_20(LoadingProgressStruct p)
	{
		SetMatchmakerStatus("Loading loot... " + p.Stage, p.Progress);
	}
}
