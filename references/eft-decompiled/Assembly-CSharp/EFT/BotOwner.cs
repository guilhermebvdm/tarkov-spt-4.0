using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT.Animations;
using EFT.CameraControl;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

namespace EFT;

public class BotOwner : MonoBehaviour, IPlayer
{
	public const float DIST_CHECK_NAVMESH = 0.6f;

	public static readonly Vector3 STAY_HEIGHT = new Vector3(0f, 1.15f, 0f);

	public static readonly Vector3 SIT_HEIGHT = new Vector3(0f, 0.55f, 0f);

	public const string PATH_TO_AI = "AI";

	public const string PATH_TO_AI_DEBUG = "AIDebug";

	public static int BotCount;

	private Action<BotOwner> _botDiedCallback;

	private EBotState _botState;

	private bool _lastHitEvent;

	private float _nextGetGoalTime = 2f;

	private float _preActivateTime;

	private float _activateTime;

	private float _nextTimeCheckBorn;

	public GClass410 DecisionProxy;

	public GClass402 DebugMemory;

	public Transform LookedTransform;

	public BotMemoryClass Memory;

	public BotDifficultySettingsClass Settings;

	private bool _isLocalGame;

	public float InertiaonDownSpeedTEMP = 3f;

	public float CollectedAngDownSpeedTEMP = 38f;

	public bool KeepZoneOnSpawn => SpawnProfileData.KeepZoneOnSpawn;

	public StandartBotBrain Brain { get; set; }

	public float ENEMY_LOOK_AT_ME { get; set; }

	public float ActivateTime => _activateTime;

	public LookSensor LookSensor { get; set; }

	public DeadBodyData DeadBodyData { get; set; }

	public BotLay BotLay { get; set; }

	public BotTilt Tilt { get; set; }

	public BotExfiltrationData Exfiltration { get; set; }

	public BotCalcGoal GoalCulculator { get; set; }

	public BotGoToPointData GoToSomePointData { get; set; }

	public LookData LookData { get; set; }

	public BotLight BotLight { get; set; }

	public BotTurnAwayLight BotTurnAwayLight { get; set; }

	public BotCoverSearchInfo CoverSearchInfo { get; set; }

	public BotBoss Boss { get; set; }

	public BotLeaveData LeaveData { get; set; }

	public BotFollower BotFollower { get; set; }

	public BotNeutralsCheskData NeutralsCheskData { get; set; }

	public BotNightVisionData NightVision { get; set; }

	public BotLoyaltyData LoyaltyData { get; set; }

	public BotMinesData MinesData { get; set; }

	public BotMinesRealtimePlaceFinder MinesRealtimePlaceFinder { get; set; }

	public BotVoxelesPersonalData VoxelesPersonalData { get; set; }

	public BotAssaultBuildingData AssaultBuildingData { get; set; }

	public BotRun BotRun { get; set; }

	public BotSteering Steering { get; set; }

	public BotEnemyLookData EnemyLookData { get; set; }

	public BotGrenadeToPortal GrenadeToPortal { get; set; }

	public FindPlaceToShootManager FindPlaceToShoot { get; set; }

	public BotHealAnotherTarget HealAnotherTarget { get; set; }

	public BotNearDoorData NearDoorData { get; set; }

	public BotRandomPlanItemDropper RandomPlanItemDropper { get; set; }

	public BotPeaceHardAim PeaceHardAim { get; set; }

	public BotPeaceLook PeaceLook { get; set; }

	public BotUnityEditorRunChecker UnityEditorRunChecker { get; set; }

	public BotDogFight DogFight { get; set; }

	public BotPriorityAxeTarget PriorityAxeTarget { get; set; }

	public BotFriendChecker FriendChecker { get; set; }

	public BotNavMeshCutterController NavMeshCutterController { get; set; }

	public BotAvoidDangerPlaces BotAvoidDangerPlaces { get; set; }

	public BotBewarePlantedMine BewarePlantedMine { get; set; }

	public BotBewareGrenade BewareGrenade { get; set; }

	public BotBewareBTR BewareBTR { get; set; }

	public BotPlayerFollowData PlayerFollowData { get; set; }

	public BotMoveToEnemyData MoveToEnemyData { get; set; }

	public ArtilleryDangerPlace ArtilleryDangerPlace { get; set; }

	public BewareArtillery BewareArtillery { get; set; }

	public BotFlashGrenade FlashGrenade { get; set; }

	public GrenadeSuicideBotData GrenadeSuicide { get; set; }

	public BotHealingBySomebody HealingBySomebody { get; set; }

	public BotPeacefulActions PeacefulActions { get; set; }

	public BotDeadBodyWork DeadBodyWork { get; set; }

	public BotMagazineChecker MagazineChecker { get; set; }

	public BotSmokeGrenade SmokeGrenade { get; set; }

	public BotSuppressShoot SuppressShoot { get; set; }

	public BotSuppressStationary SuppressStationary { get; set; }

	public BotEnemyChooser EnemyChooser { get; set; }

	public BotGiftData GiftData { get; set; }

	public BotDangerPointsData DangerPointsData { get; set; }

	public BotSuppressGrenade SuppressGrenade { get; set; }

	public BotBtrData BotBtrData { get; set; }

	public ShootData ShootData { get; set; }

	public BotEnemiesController EnemiesController { get; set; }

	public AimingManager AimingManager { get; set; }

	public BotPlanDropItem PlanDropItem { get; set; }

	public BotStandBy StandBy { get; set; }

	public BotHeadData HeadData { get; set; }

	public BotEatDrinkData EatDrinkData { get; set; }

	public BotSecondWeaponData SecondWeaponData { get; set; }

	public BotMedecine Medecine { get; set; }

	public RecoilData RecoilData { get; set; }

	public BotCallForHelp CallForHelp { get; set; }

	public BotCalledData CalledData { get; set; }

	public BotAmbushData Ambush { get; set; }

	public BotFriendlyTilt FriendlyTilt { get; set; }

	public BotLootOpener LootOpener { get; set; }

	public BotCoversData Covers { get; set; }

	public BotWeaponManager WeaponManager { get; set; }

	public BotTacticData Tactic { get; set; }

	public BotDoorOpener DoorOpener { get; set; }

	public BotGesture Gesture { get; set; }

	public BotDangerArea DangerArea { get; set; }

	public BotAssaultDangerArea AssaultDangerArea { get; set; }

	public BotReceiver Receiver { get; set; }

	public BotItemTaker ItemTaker { get; set; }

	public BotExternalItemsController ExternalItemsController { get; set; }

	public BotItemDropper ItemDropper { get; set; }

	public BotSearchData SearchData { get; set; }

	public BotPersonalStats BotPersonalStats { get; set; }

	public BotShootFromPlace ShootFromPlace { get; set; }

	public BotWarnData WarnData { get; set; }

	public BotHearingSensor HearingSensor { get; set; }

	public BotRequestController BotRequestController { get; set; }

	public BotsController BotsController { get; set; }

	public BifacialTransform MyHead { get; set; }

	public DecisionQueue DecisionQueue { get; set; }

	public BotGameEventsData GameEventsData { get; set; }

	public AICorePoint StartCorePoint { get; set; }

	public GameDateTime GameDateTime { get; set; }

	public Vector2 Lean { get; set; }

	public Vector3? Destination => Mover.TargetPoint;

	public BotAttackManager BotAttackManager { get; set; }

	public BotMover Mover { get; set; }

	public BotTalk BotTalk { get; set; }

	public IGetProfileData SpawnProfileData { get; set; }

	public AITaskManager AITaskManager => BotsController.AiTaskManager;

	public BotsGroup BotsGroup { get; set; }

	public EBotState BotState
	{
		get
		{
			return _botState;
		}
		set
		{
			_botState = value;
			if (this.OnBotStateChange != null)
			{
				this.OnBotStateChange(_botState);
			}
		}
	}

	public BifacialTransform Fireport
	{
		get
		{
			if (GetPlayer.MultiBarrelFireports != null && GetPlayer.MultiBarrelFireports.Length != 0)
			{
				return GetPlayer.MultiBarrelFireports[0];
			}
			if (GetPlayer.Fireport != null)
			{
				return GetPlayer.Fireport;
			}
			return WeaponRoot;
		}
	}

	public bool CanSprintPlayer => GetPlayer.Physical.CanSprint;

	[Obsolete("Use Player.Transform instead!", true)]
	public new Transform transform => base.transform;

	public PatrollingData PatrollingData { get; set; }

	public Vector3 Position => Transform.position;

	public string GroupId => Profile.Info.GroupId;

	public string TeamId => Profile.Info.TeamId;

	public string Infiltration => Profile.Info.EntryPoint;

	public string AccountId => Profile.AccountId;

	public string ProfileId { get; set; }

	public int Id { get; set; }

	public EPlayerSide Side => GetPlayer.Profile.Info.Side;

	public BifacialTransform Transform => GetPlayer.PlayerBones.BodyTransform;

	public BifacialTransform WeaponRoot => GetPlayer.PlayerBones.WeaponRoot;

	public IHealthController HealthController => GetPlayer.HealthController;

	public PlayerBones PlayerBones => GetPlayer.PlayerBones;

	public Profile Profile => GetPlayer.Profile;

	public InventoryController InventoryController => GetPlayer.InventoryController;

	public IPlayerSearchController SearchController => GetPlayer.SearchController;

	public BotZone SpawnBotZone { get; set; }

	public Player GetPlayer { get; set; }

	public IAIData AIData => GetPlayer.AIData;

	public bool IsAI
	{
		get
		{
			if (AIData != null)
			{
				return AIData.IsAI;
			}
			return false;
		}
	}

	public bool IsInBufferZone { get; set; }

	public EPlayerBtrState BtrState { get; set; }

	public bool StateIsSuitableForHandInput => GetPlayer.StateIsSuitableForHandInput;

	public Vector2 Rotation => GetPlayer.Rotation;

	public Vector3 Velocity => GetPlayer.Velocity;

	public byte ChannelIndex { get; }

	public bool IsYourPlayer => false;

	public ICharacterController CharacterController => null;

	public PlayerBody PlayerBody => null;

	public PlayerLoyaltyData Loyalty => GetPlayer.Loyalty;

	public Dictionary<BodyPartType, EnemyPart> MainParts => GetPlayer.MainParts;

	public Vector3 LookDirection => GetPlayer.LookDirection;

	public bool IsDead { get; set; }

	public bool HasPathAndNotComplete => Mover.HasPathAndNoComplete;

	public Player.EUpdateMode ArmsUpdateMode => GetPlayer.ArmsUpdateMode;

	public EUpdateQueue ArmsUpdateQueue => GetPlayer.ArmsUpdateQueue;

	public ECameraType VisibleToCameraType { get; }

	public bool IsVisibleToCamera { get; } = true;

	public event Action<EBotState> OnBotStateChange;

	public event Action<IPlayer> OnIPlayerDeadOrUnspawn;

	public void OnDeserializeFromServer(byte channelId, IDataReader reader)
	{
	}

	public RadioTransmitterRecodableComponent FindRadioTransmitter()
	{
		return null;
	}

	public CultistAmuletItemClass FindCultistAmulet()
	{
		return null;
	}

	public bool HasMarkOfUnknown(out MarkOfUnknownItemClass markOfUnknown)
	{
		markOfUnknown = null;
		return false;
	}

	public void SetInteractInHands(EInteraction interaction)
	{
		GetPlayer.MovementContext.SetInteractInHands(interaction);
	}

	public void PlantItemLocalOnly(Item item, string zone)
	{
		GetPlayer.PlantItemLocalOnly(item, zone);
	}

	public void UpdateInteractionCast()
	{
		GetPlayer.UpdateInteractionCast();
	}

	public void HandleFlareSuccessEvent(Vector3 position, AmmoTemplate ammoTemplate)
	{
	}

	public Vector3 PlayerColliderPointOnCenterAxis(float relativeHeight)
	{
		return GetPlayer.PlayerColliderPointOnCenterAxis(relativeHeight);
	}

	public void SayGroupAboutEnemy(IPlayer person, Vector3? partPos = null)
	{
	}

	public void GoToPoint(CustomNavigationPoint targetPoint)
	{
		Mover.GoToPoint(targetPoint);
	}

	public static BotOwner Create(Player player, GameObject behaviourTreePrefab, GameDateTime gameDataTime, BotsController botsController, bool isLocalGame, AICorePoint corePointId)
	{
		player.ProceduralWeaponAnimation.Mask = EProceduralAnimationMask.DrawDown;
		BotDifficulty difficulty;
		WildSpawnType role;
		if (player.Profile.Info != null && player.Profile.Info.Settings != null)
		{
			difficulty = player.Profile.Info.Settings.BotDifficulty;
			role = player.Profile.Info.Settings.Role;
		}
		else
		{
			difficulty = BotDifficulty.normal;
			role = WildSpawnType.assault;
		}
		BotDifficultySettingsClass settings = Singleton<GClass620>.Instance.GetSettings(difficulty, role, botsController.IsPvE);
		BotOwner botOwner = player.gameObject.AddComponent<BotOwner>();
		botOwner._isLocalGame = isLocalGame;
		botOwner.Settings = settings;
		botOwner.StartCorePoint = corePointId;
		botOwner.BotTalk = new BotTalk(botOwner);
		botOwner.Tactic = new BotTacticData(botOwner);
		botOwner.name = $"Bot{++BotCount}";
		player.SetOwnerToAIData(botOwner);
		botOwner.ENEMY_LOOK_AT_ME = Mathf.Cos(botOwner.Settings.FileSettings.Mind.ENEMY_LOOK_AT_ME_ANG * (MathF.PI / 180f));
		botOwner.BotsController = botsController;
		botOwner.GetPlayer = player;
		botOwner.Id = player.Id;
		botOwner.ProfileId = player.Profile.Id;
		botOwner.GetPlayer.ActiveHealthController.SetDamageCoeff(botOwner.Settings.FileSettings.Core.DamageCoeff);
		botOwner.MyHead = player.PlayerBones.Head;
		botOwner.Brain = new StandartBotBrain(botOwner);
		botOwner.DecisionProxy = new GClass410(botOwner);
		botOwner.DecisionQueue = new DecisionQueue(botOwner);
		botOwner.BotLight = new BotLight(botOwner);
		botOwner.BotTurnAwayLight = new BotTurnAwayLight(botOwner);
		botOwner.LookData = LookData.Create(botOwner);
		botOwner.HeadData = new BotHeadData(botOwner);
		botOwner.WarnData = new BotWarnData(botOwner);
		botOwner.NavMeshCutterController = new BotNavMeshCutterController(botOwner);
		botOwner.GrenadeSuicide = new GrenadeSuicideBotData(botOwner);
		botOwner.EatDrinkData = new BotEatDrinkData(botOwner);
		botOwner.SecondWeaponData = new BotSecondWeaponData(botOwner);
		botOwner.MagazineChecker = new BotMagazineChecker(botOwner);
		botOwner.VoxelesPersonalData = new BotVoxelesPersonalData(botOwner);
		botOwner.FriendlyTilt = new BotFriendlyTilt(botOwner);
		botOwner.Exfiltration = new BotExfiltrationData(botOwner);
		botOwner.GoalCulculator = new BotCalcGoal(botOwner);
		botOwner.PlanDropItem = new BotPlanDropItem(botOwner);
		botOwner.ItemTaker = new BotItemTaker(botOwner);
		botOwner.NeutralsCheskData = new BotNeutralsCheskData(botOwner);
		botOwner.CoverSearchInfo = new BotCoverSearchInfo(botOwner);
		botOwner.ExternalItemsController = new BotExternalItemsController(botOwner);
		botOwner.PeaceHardAim = new BotPeaceHardAim(botOwner);
		botOwner.PeaceLook = new BotPeaceLook(botOwner);
		botOwner.MoveToEnemyData = new BotMoveToEnemyData(botOwner);
		botOwner.DangerArea = new BotDangerArea(botOwner);
		botOwner.BewareArtillery = new BewareArtillery(botOwner);
		botOwner.AssaultDangerArea = new BotAssaultDangerArea(botOwner);
		botOwner.ItemDropper = new BotItemDropper(botOwner);
		botOwner.RandomPlanItemDropper = BotRandomPlanItemDropper.Create(botOwner);
		botOwner.PlayerFollowData = new BotPlayerFollowData(botOwner);
		botOwner.LoyaltyData = new BotLoyaltyData(botOwner);
		botOwner.MinesData = new BotMinesData(botOwner);
		botOwner.AssaultBuildingData = new BotAssaultBuildingData(botOwner);
		botOwner.FindPlaceToShoot = new FindPlaceToShootManager(botOwner);
		botOwner.GoToSomePointData = new BotGoToPointData(botOwner);
		botOwner.FriendChecker = new BotFriendChecker(botOwner, botsController.Bots.GetConnector());
		botOwner.PeacefulActions = new BotPeacefulActions(botOwner);
		botOwner.Covers = new BotCoversData(botOwner);
		botOwner.StandBy = new BotStandBy(botOwner);
		botOwner.EnemyLookData = new BotEnemyLookData(botOwner, onlyIfVisible: true);
		botOwner.HealingBySomebody = new BotHealingBySomebody(botOwner);
		botOwner.Medecine = new BotMedecine(botOwner);
		botOwner.LeaveData = new BotLeaveData(botOwner);
		botOwner.BotFollower = BotFollower.Create(botOwner);
		botOwner.UnityEditorRunChecker = new BotUnityEditorRunChecker(botOwner);
		botOwner.EnemiesController = BotEnemiesController.Create(botOwner);
		botOwner.Boss = new BotBoss(botOwner);
		botOwner.DoorOpener = new BotDoorOpener(botOwner);
		botOwner.RecoilData = new RecoilData(botOwner);
		botOwner.LootOpener = new BotLootOpener(botOwner);
		botOwner.DeadBodyWork = new BotDeadBodyWork(botOwner);
		botOwner.WeaponManager = new BotWeaponManager(botOwner);
		botOwner.AimingManager = new AimingManager(botOwner);
		botOwner.BotRun = new BotRun(botOwner);
		botOwner.HealAnotherTarget = new BotHealAnotherTarget(botOwner);
		botOwner.Steering = BotSteering.Create(botOwner);
		botOwner.ShootData = new ShootData(botOwner, botOwner.RecoilData);
		botOwner.DeadBodyData = new DeadBodyData(botOwner);
		botOwner.BotLay = new BotLay(botOwner);
		botOwner.Tilt = new BotTilt(botOwner);
		botOwner.GoToSomePointData = new BotGoToPointData(botOwner);
		botOwner.Receiver = new BotReceiver(botOwner);
		botOwner.NightVision = new BotNightVisionData(botOwner);
		botOwner.SearchData = BotSearchData.Create(botOwner);
		botOwner.GoToSomePointData = new BotGoToPointData(botOwner);
		botOwner.Gesture = new BotGesture(botOwner);
		botOwner.GameDateTime = gameDataTime;
		botOwner.LookSensor = new LookSensor(botOwner);
		botOwner.BotAttackManager = new BotAttackManager(botOwner);
		botOwner.HearingSensor = new BotHearingSensor(botOwner);
		botOwner.BotRequestController = new BotRequestController(botOwner);
		botOwner.BotPersonalStats = new BotPersonalStats();
		botOwner.ShootFromPlace = new BotShootFromPlace(botOwner);
		botOwner.DebugMemory = new GClass402(botOwner);
		botOwner.BewarePlantedMine = new BotBewarePlantedMine(botOwner);
		botOwner.BotAvoidDangerPlaces = new BotAvoidDangerPlaces(botOwner);
		botOwner.BewareGrenade = new BotBewareGrenade(botOwner);
		botOwner.BewareBTR = new BotBewareBTR(botOwner);
		botOwner.ArtilleryDangerPlace = new ArtilleryDangerPlace(botOwner);
		botOwner.FlashGrenade = new BotFlashGrenade(botOwner);
		botOwner.NearDoorData = new BotNearDoorData(botOwner);
		botOwner.DogFight = new BotDogFight(botOwner);
		botOwner.GrenadeToPortal = new BotGrenadeToPortal(botOwner);
		botOwner.CallForHelp = new BotCallForHelp(botOwner);
		botOwner.CalledData = new BotCalledData(botOwner);
		botOwner.Ambush = new BotAmbushData(botOwner);
		botOwner.PriorityAxeTarget = new BotPriorityAxeTarget(botOwner);
		botOwner.SmokeGrenade = new BotSmokeGrenade(botOwner);
		botOwner.SuppressShoot = new BotSuppressShoot(botOwner);
		botOwner.SuppressGrenade = new BotSuppressGrenade(botOwner);
		botOwner.BotBtrData = new BotBtrData(botOwner);
		botOwner.SuppressStationary = new BotSuppressStationary(botOwner);
		botOwner.DangerPointsData = new BotDangerPointsData(botOwner);
		botOwner.EnemyChooser = BotEnemyChooser.Create(botOwner);
		botOwner.GiftData = new BotGiftData(botOwner);
		if (botOwner.Settings.FileSettings.Move.ETERNITY_STAMINA)
		{
			botOwner.GetPlayer.Physical.Stamina.ForceMode = true;
			botOwner.GetPlayer.Physical.HandsStamina.ForceMode = true;
		}
		return botOwner;
	}

	public void PreActivate(BotZone zone, GameDateTime time, BotsGroup group, AICoversData covers, bool autoActivate = true)
	{
		_preActivateTime = Time.time;
		GameDateTime = time;
		BotsGroup = group;
		LookSensor.UpdateZoneValue(zone);
		Covers.Init();
		Mover = BotMover.Create(this, covers);
		WeaponManager.PreActivate();
		Memory = new BotMemoryClass(this, BotsGroup);
		PatrollingData = new PatrollingData(this);
		GameEventsData = new BotGameEventsData(this);
		method_4();
		DebugMemory.Init();
		method_0();
		if (autoActivate)
		{
			BotState = EBotState.PreActive;
		}
		SpawnBotZone = zone;
	}

	public void PostActivate()
	{
		if (BotState == EBotState.NonActive)
		{
			BotState = EBotState.PreActive;
		}
	}

	public void method_0()
	{
		Collider collider = GetPlayer.CharacterController.GetCollider();
		foreach (BotOwner botOwner in BotsGroup.BotGame.BotsController.Bots.BotOwners)
		{
			EFTPhysicsClass.IgnoreCollision(botOwner.GetPlayer.CharacterController.GetCollider(), collider);
		}
	}

	[CanBeNull]
	public ShootPointClass CurrentEnemyTargetPosition(bool sensPosition)
	{
		if (Memory.GoalEnemy == null)
		{
			return null;
		}
		Vector3 point = ((!sensPosition) ? Memory.GoalEnemy.GetBodyPartPosition() : (Memory.GoalEnemy.EnemyLastPosition + STAY_HEIGHT));
		return new ShootPointClass(point);
	}

	public void method_1(Func<int, bool> condition, IPlayer person)
	{
		BotsGroup.RemoveInfo(person);
		if (Memory.GoalEnemy.Person.Id == person.Id)
		{
			Memory.GoalEnemy = null;
		}
	}

	public void Sprint(bool val, bool withDebugCallback = true)
	{
		if (val)
		{
			SetPose(1f);
			AimingManager.CurrentAiming.LoseTarget();
		}
		Mover.Sprint(val, withDebugCallback);
	}

	public void method_2()
	{
		GetPlayer.OnPlayerDead += method_3;
		GetPlayer.BeingHitAction += method_9;
	}

	public void method_3(Player player, IPlayer lastAggressor, DamageInfoStruct lastDamageInfo, EBodyPart lastBodyPart)
	{
		if (lastAggressor != null)
		{
			BotsGroup.ReportAboutEnemy(lastAggressor, EEnemyPartVisibleType.Visible, this);
			method_9(lastDamageInfo, EBodyPart.Chest, 0f);
		}
	}

	public void method_4()
	{
		GetPlayer.HealthController.ApplyDamageEvent += method_7;
		GetPlayer.HealthController.DiedEvent += method_6;
		if (Singleton<BotEventHandler>.Instantiated)
		{
			Singleton<BotEventHandler>.Instance.OnBodyBotDead += method_5;
		}
	}

	public void method_5(Vector3 obj)
	{
		if (HealthController.IsAlive && Memory.IsPeace)
		{
			float sqrMagnitude = (obj - Transform.position).sqrMagnitude;
			if (!(sqrMagnitude < 1f) && sqrMagnitude < Settings.FileSettings.Hearing.DEAD_BODY_SOUND_RAD)
			{
				BotsGroup.AddPointToSearch(obj, 80f, this);
			}
		}
	}

	public void method_6(EDamageType damageType)
	{
		if (Singleton<BotEventHandler>.Instantiated)
		{
			Vector3 position = Transform.position;
			Singleton<BotEventHandler>.Instance.DeadBodySound(position);
		}
		if (WeaponManager.Stationary.CurLink != null)
		{
			WeaponManager.Stationary.CurLink.DeathAtStationary();
		}
		BotsController.BotDied(this);
		BotPersonalStats.Death(damageType);
		Dispose();
		IsDead = true;
		BotsGroup.BotZone.ZoneDangerAreas.BotDied(Position);
		if (_botDiedCallback != null)
		{
			_botDiedCallback(this);
		}
		else
		{
			Debug.LogError("bot die but have problems: _botState:" + _botState);
		}
	}

	public void Dispose()
	{
		this.OnBotStateChange = null;
		_ = Time.time;
		if (_botState == EBotState.PreActive)
		{
			return;
		}
		BotState = EBotState.Disposed;
		try
		{
			Brain?.Dispose();
			ArtilleryDangerPlace?.Dispose();
			AIData?.AskRequests?.DisposeAll();
			Mover?.Dispose();
			SuppressGrenade?.Dispose();
			SuppressShoot?.Dispose();
			SuppressStationary?.Dispose();
			BotLay?.Dispose();
			AssaultBuildingData?.Dispose();
			LoyaltyData?.Dispose();
			Boss?.Dispose();
			Tactic?.Dispose();
			FriendlyTilt?.Dispose();
			ExternalItemsController?.Dispose();
			DangerArea?.Dispose();
			LeaveData?.Dispose();
			EnemyChooser?.Dispose();
			Covers?.Dispose();
			DebugMemory?.Dispose();
			SearchData?.Dispose();
			PlayerFollowData?.Dispose();
			LookSensor?.Dispose();
			WarnData?.Dispose();
		}
		catch (Exception)
		{
		}
		try
		{
			WeaponManager?.Dispose();
			UnityEditorRunChecker?.Dispose();
			Medecine?.Dispose();
			NavMeshCutterController?.Dispose();
			FlashGrenade?.Dispose();
			PatrollingData?.Dispose();
			PeacefulActions?.Dispose();
			EnemiesController?.Dispose();
			BotFollower?.Dispose();
			ShootData?.Dispose();
			MinesData?.Dispose();
			BotRequestController?.Dispose();
			EatDrinkData?.Dispose();
			PeaceHardAim?.Dispose();
			ItemTaker?.Dispose();
			GiftData?.Dispose();
			RandomPlanItemDropper?.Dispose();
			GiftData?.Dispose();
			CalledData?.Dispose();
			BewarePlantedMine?.Dispose();
			NeutralsCheskData?.Dispose();
		}
		catch (Exception)
		{
		}
		try
		{
			method_8();
		}
		catch (Exception)
		{
		}
		try
		{
			PatrollingData?.Disable();
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].isTrigger = false;
			}
			BotState = EBotState.NonActive;
			HearingSensor?.Dispose();
			Receiver?.Dispose();
		}
		catch (Exception)
		{
		}
		try
		{
			Memory?.Dispose();
			BotPersonalStats?.Dispose();
		}
		catch (Exception)
		{
		}
		PeaceLook = null;
		EnemyChooser = null;
		Brain = null;
		ArtilleryDangerPlace = null;
		Mover = null;
		SuppressGrenade = null;
		AssaultBuildingData = null;
		SuppressShoot = null;
		BotLay = null;
		LoyaltyData = null;
		FriendlyTilt = null;
		ExternalItemsController = null;
		DangerArea = null;
		LeaveData = null;
		Covers = null;
		DebugMemory = null;
		SearchData = null;
		WarnData = null;
		NeutralsCheskData = null;
		UnityEditorRunChecker = null;
		Medecine = null;
		PatrollingData = null;
		BotFollower = null;
		EatDrinkData = null;
		PeaceHardAim = null;
		ItemTaker = null;
		PatrollingData = null;
		HearingSensor = null;
		Receiver = null;
		GiftData = null;
		Exfiltration = null;
	}

	public bool IsRole(WildSpawnType role)
	{
		if (Profile != null && Profile.Info != null && Profile.Info.Settings != null)
		{
			return Profile.Info.Settings.Role == role;
		}
		return false;
	}

	public bool IsFollower()
	{
		if (Profile != null && Profile.Info != null && Profile.Info.Settings != null)
		{
			return GClass2190.IsFollower(Profile.Info.Settings);
		}
		return false;
	}

	public float DistTo(Vector3 v)
	{
		return (Transform.position - v).magnitude;
	}

	public float SDistTo(Vector3 v)
	{
		return (Transform.position - v).sqrMagnitude;
	}

	public void method_7(EBodyPart bodyPart, float damage, DamageInfoStruct damageInfo)
	{
		GClass3051.IsSelfInflicted(damageInfo.DamageType);
	}

	public bool IsEnemyLookingAtMe(EnemyInfo goalEnemy)
	{
		if (goalEnemy == null)
		{
			return false;
		}
		return IsEnemyLookingAtMe(goalEnemy.Person);
	}

	public bool IsEnemyLookingAtMe(IPlayer gamePerson)
	{
		Vector3 position = WeaponRoot.position;
		BifacialTransform weaponRoot = gamePerson.WeaponRoot;
		return GClass855.IsAngLessNormalized(GClass855.NormalizeFastSelf(position - weaponRoot.position), gamePerson.LookDirection, 0.9659258f);
	}

	public void method_8()
	{
		_lastHitEvent = true;
		GetPlayer.OnPlayerDead -= method_3;
		GetPlayer.BeingHitAction -= method_9;
		GetPlayer.HealthController.ApplyDamageEvent -= method_7;
		GetPlayer.HealthController.DiedEvent -= method_6;
		if (Singleton<BotEventHandler>.Instantiated)
		{
			Singleton<BotEventHandler>.Instance.OnBodyBotDead -= method_5;
		}
	}

	public void method_9(DamageInfoStruct damageInfo, EBodyPart bodyType, float damageReducedByArmor)
	{
		StandBy.GetHit();
		if (damageInfo.Player == null)
		{
			return;
		}
		Player alivePlayerByProfileID = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(damageInfo.Player.iPlayer.ProfileId);
		if (alivePlayerByProfileID != null && !damageInfo.Player.IsAI && damageInfo.Player.iPlayer.Side == EPlayerSide.Savage && !BotSettingsRepoClass.IsHostileToEverybody(Profile.Info.Settings.Role) && !alivePlayerByProfileID.Loyalty.CanBeFreeKilled)
		{
			alivePlayerByProfileID.Loyalty.MarkAsCanBeFreeKilled();
		}
		BotPersonalStats.GetHit(damageInfo, bodyType);
		Memory.GetHit(damageInfo);
		if (damageInfo.Player.iPlayer != null)
		{
			if (EnemiesController.EnemyInfos.TryGetValue(damageInfo.Player.iPlayer, out var value))
			{
				value.LastGetHitTime = Time.time;
			}
			if (damageInfo.Player.iPlayer.Side == Side)
			{
				BotTalk.TrySay(EPhraseTrigger.FriendlyFire);
			}
		}
	}

	public void method_10()
	{
		try
		{
			VoxelesPersonalData.Activate(BotsGroup.BotGame.BotsController.CoversData);
			LookSensor.Activate();
			Settings.Activate();
			ExternalItemsController.Activate();
			ItemTaker.Activate();
			BewarePlantedMine.Activate();
			EnemyChooser.Activate();
			PlanDropItem.Activate();
			MinesData.Activate();
			ItemDropper.Activate();
			SuppressStationary.Activate();
			NavMeshCutterController.Activate();
			BotFollower.Activate();
			FriendlyTilt.Activate();
			RandomPlanItemDropper.Activate();
			Tactic.Activate();
			EnemiesController.Activate(BotsGroup.BotGame.BotsController.OnlineDependenceSettings.CanPersueAxeman);
			HearingSensor.Init();
			LeaveData.Activate(BotsGroup.BotZone.Modifier.LeaveDist);
			Receiver.Init();
			Mover.Activate();
			BotTalk.Activate();
			LoyaltyData.Activate();
			AssaultDangerArea.Activate();
			DangerArea.Activate();
			BotPersonalStats.Init(this, BotsGroup.BotZone.name);
			StandBy.InitPoints(BotsGroup.BotZone.Modifier.DistToActivate, BotsGroup.BotZone.Modifier.DistToSleep);
			method_2();
			FlashGrenade.Activate();
			PeaceHardAim.Activate();
			ShootData.Activate();
			PeaceLook.Activate();
			NearDoorData.Activate();
			AIData.Activate();
			UnityEditorRunChecker.Activate();
			NightVision.Activate();
			SearchData.Activate();
			Medecine.Activate();
			BotState = EBotState.Active;
			Memory.Activate();
			SuppressShoot.Activate();
			EatDrinkData.Activate();
			SecondWeaponData.Activate();
			BotLay.Activate();
			SuppressGrenade.Activate();
			method_11();
			Brain.Activate();
			PatrollingData.Activate();
			WeaponManager.Activate();
			BotFollower.TryFindBoss();
			_activateTime = Time.time;
		}
		catch (Exception)
		{
			BotState = EBotState.ActiveFail;
		}
	}

	public void method_11()
	{
		if (Settings.FileSettings.Boss.EFFECT_PAINKILLER)
		{
			GetPlayer.ActiveHealthController.DoPainKiller();
		}
		if (Settings.FileSettings.Boss.DISABLE_METABOLISM)
		{
			GetPlayer.HealthController.DisableMetabolism();
		}
		if (Settings.FileSettings.Boss.EFFECT_REGENERATION_PER_MIN > 0f)
		{
			GetPlayer.ActiveHealthController.DoScavRegeneration(Settings.FileSettings.Boss.EFFECT_REGENERATION_PER_MIN);
		}
	}

	public void Disable()
	{
		BotState = EBotState.NonActive;
	}

	public void UpdateManual()
	{
		if (BotState == EBotState.Active && GetPlayer.HealthController.IsAlive)
		{
			StandBy.Update();
			LookSensor.ManualUpdate();
			if (StandBy.StandByType != BotStandByType.paused)
			{
				if (_nextGetGoalTime < Time.time)
				{
					CalcGoal();
				}
				SuppressShoot.ManualUpdate();
				HeadData.ManualUpdate();
				ShootData.ManualUpdate();
				Tilt.ManualUpdate();
				NightVision.ManualUpdate();
				NearDoorData.Update();
				DogFight.ManualUpdate();
				FriendChecker.ManualUpdate();
				RecoilData.LosingRecoil();
				Mover.ManualUpdate();
				AimingManager.ManualUpdate();
				DoorOpener.ManualUpdate();
				Medecine.ManualUpdate();
				Boss.ManualUpdate();
				BotTalk.ManualUpdate();
				WeaponManager.ManualUpdate();
				BotRequestController.Update();
				GrenadeToPortal.ManualUpdate();
				Tactic.UpdateChangeTactics();
				Memory.ManualUpdate(Time.deltaTime);
				Settings.UpdateManual();
				BotRequestController.TryToFind();
				WarnData.ManualUpdate();
				ArtilleryDangerPlace.ManualUpdate();
				if (GetPlayer.UpdateQueue == EUpdateQueue.Update)
				{
					Mover.ManualFixedUpdate();
					Steering.ManualFixedUpdate();
				}
				UnityEditorRunChecker.ManualLateUpdate();
			}
		}
		else if (BotState == EBotState.PreActive && WeaponManager.IsReady)
		{
			if (NavMesh.SamplePosition(GetPlayer.Position, out var _, 0.6f, -1))
			{
				method_10();
			}
			else if (_nextTimeCheckBorn < Time.time)
			{
				_nextTimeCheckBorn = Time.time + 1f;
				Transform.position = GClass856.RandomElement(BotsGroup.BotZone.SpawnPoints).Position + Vector3.up * 0.5f;
				method_10();
			}
		}
	}

	public void Deactivate()
	{
		BotState = EBotState.NonActive;
	}

	public void CalcGoal()
	{
		_nextGetGoalTime = LocalBotSettingsProviderClass.Core.UPDATE_GOAL_TIMER_SEC + Time.time;
		BotsGroup.CalcGoalForBot(this);
	}

	public void FixedUpdate()
	{
		if (BotState == EBotState.Active && GetPlayer.UpdateQueue == EUpdateQueue.FixedUpdate)
		{
			Steering.ManualFixedUpdate();
			Mover.ManualFixedUpdate();
		}
	}

	public void SetLean(Vector2 lean)
	{
		Lean = lean;
	}

	public void StopMove()
	{
		Mover.Stop();
	}

	public void SetTargetMoveSpeed(float speed)
	{
		Mover.SetTargetMoveSpeed(speed);
	}

	public void MovementResume()
	{
		Mover.MovementResume();
	}

	public void SetPose(float targetPose)
	{
		Mover.SetPose(targetPose);
	}

	public void MovementPause(float pauseTime)
	{
		Mover.MovementPause(pauseTime);
	}

	public void SetDieCallback(Action<BotOwner> botDied)
	{
		_botDiedCallback = botDied;
	}

	public void GoToByWay(Vector3[] way, float reachDist = -1f)
	{
		if (reachDist < 0f)
		{
			reachDist = Settings.FileSettings.Move.REACH_DIST;
		}
		Mover.GoToByWay(way, reachDist);
	}

	public NavMeshPathStatus GoToPoint(Vector3 position, bool slowAtTheEnd = true, float reachDist = -1f, bool getUpWithCheck = false, bool mustHaveWay = true, bool mustGetUp = true, bool onlyShortTrie = false, bool force = false)
	{
		if (reachDist < 0f)
		{
			reachDist = Settings.FileSettings.Move.REACH_DIST;
		}
		return Mover.GoToPoint(position, slowAtTheEnd, reachDist, getUpWithCheck: true, mustHaveWay, onlyShortTrie, force);
	}

	public void OnDrawGizmos()
	{
	}

	public void OnDrawGizmosSelected()
	{
		GClass403.DrawBotOwnerGizmosSelected(this);
	}

	public void SetHandle(string toString)
	{
	}

	public HashSet<Vector3> CarePositions()
	{
		return Covers.CarePositions();
	}

	public IAnimator GetArmsAnimatorCommon()
	{
		return GetPlayer.GetArmsAnimatorCommon();
	}

	public void SetArmsAnimatorCommon(IAnimator animator)
	{
		GetPlayer.SetArmsAnimatorCommon(animator);
	}

	public GStruct156<Item> FindItemById(MongoID itemId, bool checkDistance = true, bool checkOwnership = true)
	{
		return GetPlayer.FindItemById(itemId, checkDistance, checkOwnership);
	}

	public float SqrDistHorizontal(Vector3 trg)
	{
		return GClass855.SqrDistHorizontal(Position, trg);
	}

	public bool ShouldApplyDamage(Player target, DamageInfoStruct damage, EBodyPart bodyPartType)
	{
		return EnemiesController.MissController.ShouldApplyDamage(target, damage, bodyPartType);
	}
}
