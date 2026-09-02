using System;
using System.Collections.Generic;
using System.Linq;
using EFT;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Layers;
using SAIN.Models.Enums;
using SAIN.Preset.GlobalSettings;
using SAIN.Preset.GlobalSettings.Categories;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.Classes.Debug;
using SAIN.SAINComponent.Classes.Decision;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.Classes.Info;
using SAIN.SAINComponent.Classes.Memory;
using SAIN.SAINComponent.Classes.Mover;
using SAIN.SAINComponent.Classes.Search;
using SAIN.SAINComponent.Classes.Talk;
using SAIN.SAINComponent.Classes.WeaponFunction;
using UnityEngine;

namespace SAIN.Components;

public class BotComponent : BotComponentBase, ISPlayer
{
    public Vector3 NavMeshPosition
    {
        get { return Transform.NavData.Position; }
    }

    public float GetDistanceToPlayer(string ProfileId)
    {
        return PlayerComponent.GetDistanceToPlayer(ProfileId);
    }

    public bool IsPlayerInRange(string ProfileId, float maxDistance, out float playerDistance)
    {
        return PlayerComponent.IsPlayerInRange(ProfileId, maxDistance, out playerDistance);
    }

    public void ActivateIfBotActive(BotOwner botOwner)
    {
        if (botOwner.BotState == EBotState.Active)
        {
            Activate(botOwner);
        }
    }

    public void Activate(BotOwner botOwner)
    {
        if (_activated)
        {
            return;
        }
        var playerComponent = botOwner.GetComponent<PlayerComponent>();
        if (playerComponent == null)
        {
#if DEBUG
            Logger.LogError("Person Null");
#endif
            return;
        }
        if (botOwner.BotState == EBotState.Active)
        {
            if (InitializeBot(playerComponent, botOwner))
            {
                _activated = true;
                OnBotActivated?.Invoke(this);
                return;
            }
            Dispose();
        }
    }

    public IBotAction CurrentAction
    {
        get { return BotActivation.CurrentAction; }
    }

    public bool IsInCombat
    {
        get { return BotActivation.BotInCombat; }
    }

    private bool _activated;

    public event Action<BotComponent> OnBotActivated;

    public bool IsCheater { get; private set; }

    public bool BotActive
    {
        get { return BotActivation.BotActive; }
    }

    public bool BotInStandBy
    {
        get { return BotActivation.BotInStandBy; }
    }

    public AILimitSetting CurrentAILimit
    {
        get { return AILimit.CurrentAILimit; }
    }

    public bool HasEnemy
    {
        get { return Enemy.IsEnemyActive(EnemyController.GoalEnemy); }
    }

    public Enemy GoalEnemy
    {
        get { return HasEnemy ? EnemyController.GoalEnemy : null; }
    }

    public BotGlobalEventsClass GlobalEvents { get; private set; }
    public BotBusyHandsDetector BusyHandsDetector { get; private set; }
    public SAINShootData Shoot { get; private set; }
    public BotWeightManagement WeightManagement { get; private set; }
    public SAINBotMedicalClass Medical { get; private set; }
    public SAINActivationClass BotActivation { get; private set; }
    public DoorOpener DoorOpener { get; private set; }
    public ManualShootClass ManualShoot { get; private set; }
    public CurrentTargetClass CurrentTarget { get; private set; }
    public BotBackpackDropClass BackpackDropper { get; private set; }
    public BotLightController BotLight { get; private set; }
    public SAINBotSpaceAwareness SpaceAwareness { get; private set; }
    public AimDownSightsController AimDownSightsController { get; private set; }
    public SAINAILimit AILimit { get; private set; }
    public SAINBotSuppressClass Suppression { get; private set; }
    public SAINVaultClass Vault { get; private set; }
    public SAINSearchClass Search { get; private set; }
    public SAINMemoryClass Memory { get; private set; }
    public SAINEnemyController EnemyController { get; private set; }
    public SAINFriendlyFireClass FriendlyFire { get; private set; }
    public SAINVisionClass Vision { get; private set; }
    public SAINMoverClass Mover { get; private set; }
    public SAINBotUnstuckClass BotStuck { get; private set; }
    public SAINHearingSensorClass Hearing { get; private set; }
    public SAINBotTalkClass Talk { get; private set; }
    public SAINDecisionClass Decision { get; private set; }
    public SAINCoverClass Cover { get; private set; }
    public SAINBotInfoClass Info { get; private set; }
    public BotSquadContainer Squad { get; private set; }
    public SAINSelfActionClass SelfActions { get; private set; }
    public BotGrenadeManager Grenade { get; private set; }
    public SAINSteeringClass Steering { get; private set; }
    public AimClass Aim { get; private set; }

    public bool IsDead
    {
        get { return Player?.HealthController?.IsAlive != true; }
    }

    public bool GameEnding
    {
        get { return BotActivation.GameEnding; }
    }

    public bool SAINLayersActive
    {
        get { return BotActivation.SAINLayersActive; }
    }

    public float DistanceToAimTarget
    {
        get
        {
            //if (BotOwner.AimingManager.CurrentAiming != null)
            //{
            //    return BotOwner.AimingManager.CurrentAiming.LastDist2Target;
            //}
            if (EnemyController.GoalEnemy != null)
            {
                return EnemyController.GoalEnemy.KnownPlaces.BotDistanceFromLastKnown;
            }
            return float.MaxValue;
        }
    }

    public float LastCheckVisibleTime;

    public ESAINLayer ActiveLayer
    {
        get { return BotActivation.ActiveLayer; }
        set { BotActivation.SetActiveLayer(value); }
    }

    // ref: AUD-LOD-01 - Sistema de LOD de IA Adaptativo com Despertar Instantâneo (modded-multithread)
    private float _nextLODTickTime;
    private float _nextDistCheckTime;
    private float _cachedDistToHuman = float.MaxValue;
    private float _combatLockoutTime;
    private const float COMBAT_LOCKOUT_DURATION = 15f;
    private const float LOD_CLOSE_DIST = 50f;
    private const float LOD_MID_DIST = 150f;
    private const float LOD_MID_INTERVAL = 0.04f; // ~25 Hz
    private const float LOD_FAR_INTERVAL = 0.12f; // ~8 Hz

    public float DistanceToClosestHuman { get; private set; } = float.MaxValue;
    public bool IsLODTier0 { get; private set; } = true;

    public void ForceInstantWakeup(bool alertSquad = true)
    {
        _combatLockoutTime = Time.time + COMBAT_LOCKOUT_DURATION;
        _nextLODTickTime = 0f;
        IsLODTier0 = true;

        if (alertSquad)
        {
            var members = Squad?.Members;
            if (members != null && members.Count > 1)
            {
                foreach (var member in members.Values)
                {
                    if (member != null && member != this)
                    {
                        member.ForceInstantWakeup(false);
                    }
                }
            }
        }
    }

    private float GetMinDistanceToHumanPlayer(float currentTime)
    {
        if (_nextDistCheckTime > currentTime)
        {
            return _cachedDistToHuman;
        }
        _nextDistCheckTime = currentTime + 0.5f;

        float minDistance = float.MaxValue;
        var otherPlayers = PlayerComponent?.OtherPlayersData?.DataList;
        if (otherPlayers != null)
        {
            for (int i = 0; i < otherPlayers.Count; i++)
            {
                var other = otherPlayers[i];
                var otherPlayer = other?.OtherPlayerComponent?.Player;
                if (otherPlayer != null && !otherPlayer.IsAI && other.DistanceData != null)
                {
                    float dist = other.DistanceData.Distance;
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                    }
                }
            }
        }
        _cachedDistToHuman = minDistance;
        DistanceToClosestHuman = minDistance;
        return minDistance;
    }

    public void ManualUpdate(float currentTime, float deltaTime)
    {
        BotOwner botOwner = BotOwner;
        if (botOwner != null)
        {
            Player player = botOwner.GetPlayer;
            if (player != null)
            {
                TickClassGroup(_alwaysTickClasses, currentTime);

                bool active = BotActive;
                bool inStandBy = !active || BotInStandBy;
                bool inCombat = active && !inStandBy && SAINLayersActive && GoalEnemy != null;
                BotActivation.SetInCombat(inCombat);

                // Determinar se o bot está em Tier 0 (Full Tick - 60+ Hz)
                bool isUnderFire = botOwner.Memory?.IsUnderFire == true;
                bool isLockoutActive = _combatLockoutTime > currentTime;
                bool isRecentlyShot = Medical != null && Medical.TimeSinceShot < 10f;
                float distToHuman = GetMinDistanceToHumanPlayer(currentTime);
                bool isCloseToHuman = distToHuman <= LOD_CLOSE_DIST || distToHuman == float.MaxValue;

                bool isTier0 = inCombat || isUnderFire || isLockoutActive || isRecentlyShot || isCloseToHuman;
                IsLODTier0 = isTier0;

                if (isTier0)
                {
                    _nextLODTickTime = 0f;
                }

                bool shouldTickLOD = isTier0;
                if (!shouldTickLOD)
                {
                    float interval = distToHuman <= LOD_MID_DIST ? LOD_MID_INTERVAL : LOD_FAR_INTERVAL;
                    if (currentTime >= _nextLODTickTime)
                    {
                        shouldTickLOD = true;
                        _nextLODTickTime = currentTime + interval;
                    }
                }

                if (active)
                {
                    if (shouldTickLOD)
                    {
                        TickClassGroup(_tickWhenActiveClasses, currentTime);
                    }
                }

                if (!inStandBy)
                {
                    if (shouldTickLOD)
                    {
                        TickClassGroup(_tickWhenNoSleepClasses, currentTime);
                        HandleDumbShit();
                    }
                }

                if (inCombat)
                {
                    TickClassGroup(_tickWhenCombatClasses, currentTime);
                }
            }
        }
    }

    private static void TickClassGroup(List<IBotClass> List, float CurrentTime)
    {
        for (int i = 0; i < List.Count; i++)
        {
            List[i]?.ManualUpdate();
        }
    }

    public bool InitializeBot(PlayerComponent playerComponent, BotOwner botOwner)
    {
        base.Init(playerComponent, botOwner);
        if (!CreateClasses())
        {
            return false;
        }
        if (!AddToSquad())
        {
            return false;
        }
        if (!InitClasses())
        {
            return false;
        }
        if (!FinishInit(playerComponent))
        {
            return false;
        }
        return true;
    }

    private bool CreateClasses()
    {
        try
        {
            // Must be first, other classes use it
            Info = new SAINBotInfoClass(this);

            Squad = new BotSquadContainer(this);
            BusyHandsDetector = new BotBusyHandsDetector(this);
            GlobalEvents = new BotGlobalEventsClass(this);
            Shoot = new SAINShootData(this);
            WeightManagement = new BotWeightManagement(this);
            Memory = new SAINMemoryClass(this);
            BotStuck = new SAINBotUnstuckClass(this);
            Hearing = new SAINHearingSensorClass(this);
            Talk = new SAINBotTalkClass(this);
            Decision = new SAINDecisionClass(this);
            Cover = new SAINCoverClass(this);
            SelfActions = new SAINSelfActionClass(this);
            Steering = new SAINSteeringClass(this);
            Grenade = new BotGrenadeManager(this);
            Mover = new SAINMoverClass(this);
            EnemyController = new SAINEnemyController(this);
            FriendlyFire = new SAINFriendlyFireClass(this);
            Vision = new SAINVisionClass(this);
            Search = new SAINSearchClass(this);
            Vault = new SAINVaultClass(this);
            Suppression = new SAINBotSuppressClass(this);
            AILimit = new SAINAILimit(this);
            AimDownSightsController = new AimDownSightsController(this);
            SpaceAwareness = new SAINBotSpaceAwareness(this);
            DoorOpener = new DoorOpener(this);
            Medical = new SAINBotMedicalClass(this);
            BotLight = new BotLightController(this);
            BackpackDropper = new BotBackpackDropClass(this);
            CurrentTarget = new CurrentTargetClass(this);
            ManualShoot = new ManualShootClass(this);
            BotActivation = new SAINActivationClass(this);
            Aim = new AimClass(this);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error When Creating Classes, Disposing... : {ex}");
            return false;
        }
        return true;
    }

    public void AddBotClass(IBotClass Class)
    {
        if (Class == null)
        {
            Logger.LogError($"Bot Class of is null, cannot add it to list!");
            return;
        }
        _botClasses.Add(Class);
    }

    public void AddBotTickClass(IBotClass Class)
    {
        if (Class.CanEverTick)
        {
            switch (Class.TickRequirement)
            {
                case ESAINTickState.AlwaysUpdate:
                    _alwaysTickClasses.Add(Class);
                    break;

                case ESAINTickState.OnlyBotActive:
                    _tickWhenActiveClasses.Add(Class);
                    break;

                case ESAINTickState.OnlyNoSleep:
                    _tickWhenNoSleepClasses.Add(Class);
                    break;

                case ESAINTickState.OnlyBotInCombat:
                    _tickWhenCombatClasses.Add(Class);
                    break;

                default:
                    break;
            }
        }
    }

    private bool AddToSquad()
    {
        try
        {
            Squad.SquadInfo.AddMember(this);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error adding member to squad!: {ex}");
            return false;
        }
        return true;
    }

    private bool InitClasses()
    {
        foreach (var botClass in _botClasses)
        {
            try
            {
                botClass.Init();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error When Initializing Class [{botClass}], Disposing... : {ex}");
                return false;
            }
        }
        return true;
    }

    private bool FinishInit(PlayerComponent playerComponent)
    {
        try
        {
            if (!VerifyBrain(playerComponent))
            {
                Logger.LogError("Init SAIN ERROR, Disposing...");
                return false;
            }

            try
            {
                BotOwner.LookSensor.MaxShootDist = float.MaxValue;
                if (BotOwner.AIData is PlayerAIDataClass aiData)
                {
                    aiData.IsNoOffsetShooting = false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error setting MaxShootDist during init, but continuing with initialization...: {ex}");
            }

            try
            {
                var settings = GlobalSettingsClass.Instance.General.Jokes;
                if (
                    settings.RandomCheaters
                    && (
                        EFTMath.RandomBool(settings.RandomCheaterChance)
                        || Player.Profile.Nickname.Contains("solarint", StringComparison.OrdinalIgnoreCase)
                    )
                )
                {
                    IsCheater = true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(
                    $"Error when initializing dumb shit for this bot, continuing anyways since its some dumb shit. Error: {ex}"
                );
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error When Finishing Bot Initialization, Disposing... : {ex}");
            return false;
        }
        return true;
    }

    private bool VerifyBrain(PlayerComponent playerComp)
    {
        if (Info.Profile.IsBoss)
        {
            return true;
        }

        string assignedBrainName = playerComp.BotOwner?.Brain?.BaseBrain?.ShortName();

        if (Info.Profile.IsPMC)
        {
            return IsAssignedBrainAllowed(assignedBrainName, AIBrains.AllowedPMCBrains, "PMC");
        }

        if (Info.Profile.IsPlayerScav)
        {
            return IsAssignedBrainAllowed(assignedBrainName, AIBrains.AllowedPlayerScavBrains, "PlayerScav");
        }

        if (Info.Profile.IsScav)
        {
            return IsAssignedBrainAllowed(assignedBrainName, AIBrains.AllowedScavBrains, "Scav");
        }

        return true;
    }

    private bool IsAssignedBrainAllowed(string assignedBrainName, IReadOnlyCollection<string> allowedBrainNames, string botCategory)
    {
        if (allowedBrainNames.Contains(assignedBrainName))
        {
            return true;
        }

        Logger.LogAndNotifyError(
            $"{BotOwner.name} is a {botCategory} but does not have any of these BaseBrains: ${string.Join(", ", allowedBrainNames)}! Current Brain Assignment: [{assignedBrainName}] : Destroying SAIN for this bot..."
        );

        return false;
    }

    private void OnDisable()
    {
        BotActivation.SetActive(false);
        StopAllCoroutines();
    }

    private void OnEnable() { }

    public void LateUpdate()
    {
        //BotActivation?.LateUpdate();
        //EnemyController?.LateUpdate();
    }

    private void HandleDumbShit()
    {
        if (IsCheater)
        {
            if (_defaultMoveSpeed == 0)
            {
                _defaultMoveSpeed = Player.MovementContext.MaxSpeed;
                _defaultSprintSpeed = Player.MovementContext.SprintSpeed;
            }
            Player.Grounder.enabled = GoalEnemy == null;
            if (GoalEnemy != null)
            {
                Player.MovementContext.SetCharacterMovementSpeed(350, true);
                Player.MovementContext.SprintSpeed = 50f;
                Player.ChangeSpeed(100f);
                Player.UpdateSpeedLimit(100f, Player.ESpeedLimit.SurfaceNormal);
                Player.MovementContext.ChangeSpeedLimit(100f, Player.ESpeedLimit.SurfaceNormal);
                BotOwner.SetTargetMoveSpeed(100f);
            }
            else
            {
                Player.MovementContext.SetCharacterMovementSpeed(_defaultMoveSpeed, false);
                Player.MovementContext.SprintSpeed = _defaultSprintSpeed;
            }
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        BotActivation?.SetActive(false);
        StopAllCoroutines();

        foreach (var botClass in _botClasses)
        {
            try
            {
                botClass.Dispose();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Dispose Class [{botClass}] Error: {ex}");
            }
        }

        if (BotOwner != null)
        {
            BotOwner.OnBotStateChange -= ResetBot;
        }

        Destroy(this);
    }

    private void ResetBot(EBotState state)
    {
        Decision.ResetDecisions(false);
    }

    /// <summary>
    /// All Bot Component classes.
    /// </summary>
    private readonly List<IBotClass> _botClasses = [];

    /// <summary>
    /// Bot classes that should tick no matter what.
    /// </summary>
    private readonly List<IBotClass> _alwaysTickClasses = [];

    /// <summary>
    /// Bot classes that should tick when a bot is active.
    /// </summary>
    private readonly List<IBotClass> _tickWhenActiveClasses = [];

    /// <summary>
    /// Bot classes that should tick when a bot not sleeping.
    /// </summary>
    private readonly List<IBotClass> _tickWhenNoSleepClasses = [];

    /// <summary>
    /// Bot classes that should tick when a bot is in combat.
    /// </summary>
    private readonly List<IBotClass> _tickWhenCombatClasses = [];

    private float _defaultMoveSpeed;
    private float _defaultSprintSpeed;
}
