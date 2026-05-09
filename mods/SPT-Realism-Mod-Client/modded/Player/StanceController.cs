using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.Animations.NewRecoil;
using EFT.InventoryLogic;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static EFT.Player;

namespace RealismMod
{
    public enum EBracingDirection 
    {
        Top,
        Left, 
        Right,
        None
    }

    public enum EStance
    {
        None,
        LowReady,
        HighReady,
        ShortStock,
        ActiveAiming,
        PatrolStance,
        Melee,
        PistolCompressed
    }

    public static class StanceController
    {
        public const float STANCE_WEIGHT_LIMIT_KG = 8f;

        public static Quaternion CurrentRotation = Quaternion.identity;
        public static Quaternion StanceRotation = Quaternion.identity;
        public static Vector3 MountWeapPosition = Vector3.zero;
        public static Vector3 CurrentVisualRecoil = Vector3.zero;
        public static Vector3 TargetVisualRecoil = Vector3.zero;

        public static bool HasResetActiveAim = true;
        public static bool HasResetLowReady = true;
        public static bool HasResetHighReady = true;
        public static bool HasResetShortStock = true;
        public static bool HasResetPistolPos = true;
        public static bool HasResetMelee = true;

        public static bool IsResettingActiveAim = false;
        public static bool IsResettingLowReady = false;
        public static bool IsResettingHighReady = false;
        public static bool IsResettingShortStock = false;
        public static bool IsResettingPistol = false;
        public static bool IsResettingMelee = false;
        public static bool DidHalfMeleeAnim = false;

        public static float StanceRotationSpeed = 1f;

        public static bool HaveSetAiming = false;
        public static bool HaveSetActiveAim = false;

        public static bool _isLeftStanceResetState = false;
        private static float _leftStanceTime = 0f;
        private static Vector3 _leftStanceRotaiton;

        private static float _pistolPosSpeed = 1f;

        private static float _currentRifleXPos = 0f;
        private static float _currentRifleYPos = 0f;
        private static float _currentRifleZPos = 0f;
        private static float _currentPistolXPos = 0f;
        private static float _currentPistolYPos = 0f;
        private static float _currentPistolZPos = 0f;

        private static float _gunCameraAlignmentTargetX = 0f;
        private static float _gunCameraAlignmentTargetY = 0f;
        private static float _gunCameraAlignmentTargetZ = 0f;
        private static float _gunXTarget = 0f;
        private static float _gunYTarget = 0f;
        private static float _gunZTarget = 0f;

        private static Vector3 _leftStancePistolRotaitonTarget = new Vector3(0f, -10f, 0f);
        private static Vector3 _leftStancePistolPositionTarget = new Vector3(0f, -0.02f, 0f);
        private static Vector3 _leftStanceRifleRotaitonTarget = new Vector3(0f, -10f, 0f);
        private static Vector3 _leftStanceRiflePositionTarget = new Vector3(0f, 0f, 0f);
        private static Vector3 _leftStancePosition = Vector3.zero;
        private static Vector3 _leftStanceVelocity = Vector3.zero;
        private static float _leftStanceProgress = 0f;
        private static float _leftStanceTargetX;

        private static AnimationCurve _leftRotationXCurve = new AnimationCurve(
            new Keyframe(0, 0f),
            new Keyframe(0.25f, -2f),
            new Keyframe(0.5f, -5f),
            new Keyframe(0.75f, -1.5f),
            new Keyframe(1, 0f)
        );

        private static AnimationCurve _leffPosZCurve = new AnimationCurve(
            new Keyframe(0, 0f),
            new Keyframe(0.15f, 0.1f),
            new Keyframe(0.3f, 0.075f),
            new Keyframe(0.5f, 0.1f),
            new Keyframe(0.65f, 0.05f),
            new Keyframe(0.7f, 0.025f),
            new Keyframe(0.9f, -0.045f),
            new Keyframe(1, 0f)
        );

        private static AnimationCurve _leffPosZCurveReturn = new AnimationCurve(
            new Keyframe(0, 0f),
            new Keyframe(0.15f, -0.05f),
            new Keyframe(0.3f, 0.025f),
            new Keyframe(0.5f, 0.05f),
            new Keyframe(0.65f, 0.075f),
            new Keyframe(0.7f, 0.05f),
            new Keyframe(0.9f, 0.1f),
            new Keyframe(1, 0f)
            );

        public static Vector3 CoverWiggleDirection = Vector3.zero;
        public static Vector3 BaseWeaponOffsetPosition = Vector3.zero;
        public static Vector3 StanceTargetPosition = Vector3.zero;
        private static Vector3 _pistolLocalPosition = Vector3.zero;
        private static Vector3 _rifleLocalPosition = Vector3.zero;

        private const float _clickDelay = 0.2f;
        private static float _doubleClickTime = 0f;
        private static bool _clickTriggered = true;
        public static int StanceIndex = 0;

        public static bool MeleeIsToggleable = true;
        public static bool CanDoMeleeDetection = false;
        public static bool MeleeHitSomething = false;
        private static float _meleeTimer = 0.0f;
        private static bool _isHoldingBackMelee = false;

        public static bool IsFiringFromStance = false;
        public static float StanceShotTime = 0.0f;
        private static float _manipTime = 0.0f;
        public static float ManipTimer = 0.25f;

        public static float DampingTimer = 0.0f;
        public static bool DoDampingTimer = false;
        public static bool CanResetDamping = true;

        public static bool WasAimingBeforeCollision = false;
        public static bool StopCameraMovement = false;
        public static float CameraMovmentForCollisionSpeed = 0.01f;
        public static bool IsColliding = false;

        public static float HighReadyBlackedArmTime = 0.0f;
        public static bool CanDoHighReadyInjuredAnim = false;

        public static bool CancelPistolStance = false;
        public static bool PistolIsColliding = false;
        public static bool CancelHighReady = false;
        public static bool ModifyHighReady = false;
        public static bool CancelLowReady = false;
        public static bool CancelShortStock = false;
        public static bool CancelActiveAim = false;
        public static bool ShouldResetStances = false;
        private static bool _doMeleeReset = false;

        private static EStance _lastRecordedStanceStamina = EStance.None; //used for stamina drate rate updates
        private static EStance _previousStance = EStance.None;
        private static EStance _currentStance = EStance.None;
        private static EStance _storedStance = EStance.None;
        public static bool FinishedUnPatrolStancing = false;
        private static bool _SkipPistolWiggle = false;
        public static bool WasActiveAim = false;

        private static bool _isLeftShoulder = false;
        public static bool CancelLeftShoulder = false;
        public static bool HaveResetLeftShoulder = false;
        public static bool IsDoingTacSprint = false;
        public const float TAC_SPRINT_WEIGHT_LIMIT = 5.1f;
        public const float TAC_SPRINT_WEIGHT_BULLPUP = 5.75f;
        public const int TAC_SPRINT_LENGTH_LIMIT = 6;
        public const float TAC_SPRINT_ERGO_LIMIT = 35f;

        public static bool IsInForcedLowReady = false;
        public static bool IsAiming = false;
        public static bool DidWeaponSwap = false;
        public static bool IsBlindFiring = false;
        public static bool IsInThirdPerson = false;
        public static bool ToggledLight = false;
        public static bool DidStanceWiggle = false;
        public static bool DidLowReadyResetStanceWiggle = false;
        public static float WiggleReturnSpeed = 1f;

        //arm stamina
        private static bool _regenStam = false;
        private static bool _drainStamStam = false;
        private static bool _neutralStam = false;
        private static bool _wasBracingStam = false;
        private static bool _wasMountingStam = false;
        private static bool _wasAimingStam = false;
        public static bool HaveResetStamDrain = false;
        public static bool CanResetAimDrain = false;

        //extra rotaitons
        private static Vector3 _posePosOffest = Vector3.zero;
        private static Vector3 _poseRotOffest = Vector3.zero;
        private static Vector3 _patrolPos = Vector3.zero;
        private static Vector3 _patrolRot = Vector3.zero;

        //patrol
        private static Vector3 _riflePatrolPos = new Vector3(0.2f, 0.025f, 0.1f);
        private static Vector3 _riflePatrolRot = new Vector3(0.05f, -0.05f, -0.5f);
        private static Vector3 _pistolPatrolPos = new Vector3(0.05f, 0f, 0f);
        private static Vector3 _pistolPatrolRot = new Vector3(0.1f, -0.1f, -0.1f);

        //tac sprint
        private static float _tacSprintTime = 0.0f;
        private static bool _canDoTacSprintTimer = false;

        //mounting
        private static Quaternion _makeQuaternionDelta(Quaternion from, Quaternion to) => to * Quaternion.Inverse(from); //yeah I don't know what this is either
        private static float _mountAimSmoothed = 0f;
        public static float _cumulativeMountPitch = 0f;
        public static float _cumulativeMountYaw = 0f;
        static Vector2 _lastMountYawPitch;
        public static EBracingDirection BracingDirection = EBracingDirection.None;
        public static bool IsBracing = false;
        public static bool _isRealismMounting = false;
        public static float BracingSwayBonus = 1f;
        public static float BracingRecoilBonus = 1f;

        //this sucks, don't use in update
        public static WildSpawnType[] _botsToUseTacticalStances = { WildSpawnType.bossKolontay, WildSpawnType.pmcBEAR, WildSpawnType.pmcUSEC, WildSpawnType.exUsec, WildSpawnType.pmcBot, WildSpawnType.bossKnight, WildSpawnType.followerBigPipe, WildSpawnType.followerBirdEye, WildSpawnType.bossGluhar, WildSpawnType.followerGluharAssault, WildSpawnType.followerGluharScout, WildSpawnType.followerGluharSecurity, WildSpawnType.followerGluharSnipe };

        public static Player.BetterValueBlender StanceBlender = new Player.BetterValueBlender
        {
            Speed = 5f,
            Target = 0f
        };

        public static Vector3 MountPos { get; set; }
        public static Vector3 MountDir { get; set; }

        public static bool AllStancesReset
        {
            get
            {
                return HasResetActiveAim && HasResetLowReady && HasResetHighReady && HasResetShortStock && HasResetPistolPos && HaveResetLeftShoulder; //&& HasResetMelee
            }
        }

        public static bool ShouldBlockAllStances
        {
            get
            {
                return (IsMounting && WeaponStats.BipodIsDeployed) || !MeleeIsToggleable;
            }
        }

        public static bool IsReadyForBayonetCharge
        {
            get
            {
                return (_isHoldingBackMelee);
            }
        }

        public static bool TreatWeaponAsPistolStance
        {
            get
            {
                return WeaponStats.IsMachinePistol || WeaponStats.IsStocklessPistol;
            }
        }

        public static bool CanDoTacSprint
        {
            get
            {
                return PluginConfig.EnableTacSprint.Value && PlayerState.IsSprinting && CurrentStance != EStance.ActiveAiming
                && (CurrentStance == EStance.HighReady || StoredStance == EStance.HighReady) &&
                WeaponStats.TotalWeaponWeight <= (WeaponStats.IsBullpup ? TAC_SPRINT_WEIGHT_BULLPUP : TAC_SPRINT_WEIGHT_LIMIT)
                && WeaponStats.TotalWeaponLength <= TAC_SPRINT_LENGTH_LIMIT && !PlayerState.IsScav
                && !Plugin.RealHealthController.HealthConditionPreventsTacSprint && WeaponStats.TotalErgo > TAC_SPRINT_ERGO_LIMIT;
            }
        }

        public static bool ShouldForceLowReady
        {
            get
            {
                return (Plugin.RealHealthController.HealthConditionForcedLowReady || (WeaponStats.TotalWeaponWeight >= 10f && !IsMounting))
                    && !IsAiming && !IsFiringFromStance && CurrentStance != EStance.PistolCompressed
                    && CurrentStance != EStance.PatrolStance && CurrentStance != EStance.ShortStock
                    && CurrentStance != EStance.ActiveAiming && MeleeIsToggleable && !IsBracing;
            }
        }

        public static float HighReadyManipBuff
        {
            get
            {
                return CurrentStance == EStance.HighReady ? 1.18f : 1f;
            }
        }
        public static float ActiveAimManipBuff
        {
            get
            {
                return CurrentStance == EStance.ActiveAiming && PluginConfig.ActiveAimReload.Value ? 1.15f : 1f;
            }
        }
        public static float LowReadyManipBuff
        {
            get
            {
                return CurrentStance == EStance.LowReady ? 1.21f : 1f;
            }
        }

        public static EStance StoredStance
        {
            get { return _storedStance; }
            set { _storedStance = value; }
        }
        public static EStance CurrentStance
        {
            get { return _currentStance; }
            set
            {
                if (value != _currentStance)
                {
                    _currentStance = value;
                    if (!IsAiming) Utils.GetYourPlayer().ProceduralWeaponAnimation.method_23();
                }
            }
        }

        public static bool IsLeftStanceResetState 
        {
            get { return _isLeftStanceResetState; }
            private set  { _isLeftStanceResetState = value; }
        }

        public static bool IsLeftShoulder
        {
            get { return _isLeftShoulder; }
            set
            {
                if (value != _isLeftShoulder)
                {
                    _isLeftShoulder = value;
                    Utils.GetYourPlayer().ProceduralWeaponAnimation.method_23(); //to recalculate sway (I think)
                }
            }
        }

        public static bool IsDoingLeftShoulderNotBlocked
        {
            get 
            {
                return IsLeftShoulder && !IsBlindFiring && !CancelLeftShoulder; 
            }
        }

        public static bool IsMounting
        {
            get
            {
                return _isRealismMounting;
            }
            set
            {
                if (value != _isRealismMounting)
                {
                    Player player = Utils.GetYourPlayer();
                    FirearmController fc = player.HandsController as FirearmController;
                    if (fc == null)
                    {
                        value = false;
                        return;
                    }
                    _isRealismMounting = value;
                    if (player.ProceduralWeaponAnimation != null) player.ProceduralWeaponAnimation.method_23();
                    float accuracy = fc.Item.GetTotalCenterOfImpact(false); //forces accuracy to update
                    AccessTools.Field(typeof(Player.FirearmController), "float_3").SetValue(fc, accuracy); //update weapon accuracy
                    player.ProceduralWeaponAnimation.UpdateTacticalReload(); //gives better chamber animations
                    //player.MovementContext.PlayerAnimator.SetProneBipodMount(player.MovementContext.IsInPronePose && WeaponStats.BipodIsDeployed && value); //this causes camera to detatch from weapon, could be a good effect if I could get camera to follow it.
                    fc.FirearmsAnimator.SetMounted(value);
                    //player.ProceduralWeaponAnimation.SetMountingData(value, BracingDirection != EBracingDirection.Top);
                }
            }
        }


        public static bool IsCantedAiming(ProceduralWeaponAnimation pwa, bool checkifAiming)
        {
            bool isCanted = Mathf.Abs(pwa.CurrentScope.Rotation) >= EFTHardSettings.Instance.SCOPE_ROTATION_THRESHOLD;
            bool isAimingOk = !checkifAiming || StanceController.IsAiming;
            return isCanted && isAimingOk;
        }

        public static bool AimingInterrupted { get; set; }

        public static void InterruptAim(FirearmController fc)
        {
            if (fc.IsAiming && !AimingInterrupted)
            {
                fc.ToggleAim();
                AimingInterrupted = true;
            }
        }

        public static void UnInterruptAim(FirearmController fc) 
        {
            if (!fc.IsAiming && AimingInterrupted)
            {
                fc.ToggleAim();
                AimingInterrupted = false;
            }
        }

        public static float ChonkerFactor 
        {
            get 
            {
                return WeaponStats.TotalWeaponWeight >= STANCE_WEIGHT_LIMIT_KG ? 0.7f : 1f;
            }
        }

        public static Dictionary<string, Vector3> GetWeaponOffsets()
        {
            return new Dictionary<string, Vector3>{
            { "5aafa857e5b5b00018480968", new Vector3(0f, 0f, -0.1f)}, //m1a
            { "5b0bbe4e5acfc40dc528a72d", new Vector3(0f, 0f, -0.035f)}, //sa58
            { "676176d362e0497044079f4c", new Vector3(0f, -0.0135f, 0.02f)}, //x17
            { "6183afd850224f204c1da514", new Vector3(0f, -0.0135f, 0.02f)}, //mk17
            { "6165ac306ef05c2ce828ef74", new Vector3(0f, -0.0135f, 0.02f)}, //mk17 fde
            { "6184055050224f204c1da540", new Vector3(0f, -0.0135f, 0.02f)}, //mk16
            { "618428466ef05c2ce828f218", new Vector3(0f, -0.0135f, 0.02f)}, //mk16 fde
            { "5ae08f0a5acfc408fb1398a1", new Vector3(0f, 0f, -0.005f)}, //mosin 
            { "5bfd297f0db834001a669119", new Vector3(0f, 0f, -0.005f)}, //mosin s
            { "54491c4f4bdc2db1078b4568", new Vector3(0f, 0f, -0.01f)}, //mp133
            { "56dee2bdd2720bc8328b4567", new Vector3(0f, 0f, -0.01f)}, //mp153
            { "606dae0ab0e443224b421bb7", new Vector3(0f, 0f, -0.01f)}, //mp155
            { "6259b864ebedf17603599e88", new Vector3(0f, 0f, -0.02f)}, //M3
            { "6783ae5bb52da6ed912e3d01", new Vector3(0f, 0f, -0.02f)}, //M3 mechanic
            };
        }

        private static float GetRestoreRate()
        {
            float baseRestoreRate = 0f;
            if (IsMounting && WeaponStats.BipodIsDeployed)
            {
                baseRestoreRate = 5f;
            }
            if (CurrentStance == EStance.PatrolStance || IsMounting)
            {
                baseRestoreRate = 4f;
            }
            else if (CurrentStance == EStance.LowReady || CurrentStance == EStance.PistolCompressed || IsBracing)
            {
                baseRestoreRate = 2.4f;
            }
            else if (CurrentStance == EStance.HighReady)
            {
                baseRestoreRate = 1.85f;
            }
            else if (CurrentStance == EStance.ShortStock)
            {
                baseRestoreRate = 1.3f;
            }
            else if (IsIdle() && !PluginConfig.EnableIdleStamDrain.Value)
            {
                baseRestoreRate = 1f;
            }
            else
            {
                baseRestoreRate = 1f;
            }
            float formfactor = WeaponStats.IsBullpup ? 1.05f : 1f;
            return (1f - ((WeaponStats.ErgoFactor * formfactor) / 100f)) * baseRestoreRate * PlayerState.HealthStamRegenFactor;
        }

        private static float GetDrainRate(Player player)
        {
            float baseDrainRate = 0f;
            if (player.Physical.HoldingBreath)
            {
                baseDrainRate = IsMounting && WeaponStats.BipodIsDeployed ? 0.025f : IsMounting ? 0.05f : IsBracing ? 0.1f : 0.5f;
            }
            else if (IsAiming)
            {
                baseDrainRate = 0.15f;
            }
            else if (IsDoingTacSprint)
            {
                baseDrainRate = 0.15f;
            }
            else if (CurrentStance == EStance.ActiveAiming)
            {
                baseDrainRate = 0.075f;
            }
            else
            {
                baseDrainRate = 0.1f;
            }
            float formfactor = WeaponStats.IsBullpup ? 0.4f : 1f;
            return WeaponStats.ErgoFactor * formfactor * baseDrainRate * ((1f - PlayerState.HealthStamRegenFactor) + 1f) * (1f - (PlayerState.StrengthSkillAimBuff)) * PluginConfig.IdleStamDrainModi.Value;
        }

        //this method makes baby Jesus cry
        public static void SetStanceStamina(Player player)
        {
            bool isUsingStationaryWeapon = player.MovementContext.CurrentState.Name == EPlayerState.Stationary;
            bool isInRegenableStance = CurrentStance == EStance.HighReady || CurrentStance == EStance.LowReady || CurrentStance == EStance.PatrolStance || CurrentStance == EStance.ShortStock || (IsIdle() && !PluginConfig.EnableIdleStamDrain.Value);
            bool isInRegenableState = (!player.Physical.HoldingBreath && (IsMounting || IsBracing)) || player.IsInPronePose || CurrentStance == EStance.PistolCompressed || isUsingStationaryWeapon;
            bool doRegen = ((isInRegenableStance && !IsAiming && !IsFiringFromStance) || isInRegenableState) && !PlayerState.IsSprinting;
            bool shouldDoIdleDrain = IsIdle() && PluginConfig.EnableIdleStamDrain.Value;
            bool shouldInterruptRegen = isInRegenableStance && (IsAiming || IsFiringFromStance);
            bool doNeutral = PlayerState.IsSprinting || player.IsInventoryOpened || (CurrentStance == EStance.ActiveAiming && player.Pose == EPlayerPose.Duck);
            bool doDrain = ((shouldInterruptRegen || !isInRegenableStance || shouldDoIdleDrain) && !isInRegenableState && !doNeutral) || (IsDoingTacSprint && PluginConfig.EnableIdleStamDrain.Value);
            EStance stance = CurrentStance;

            if (HaveResetStamDrain || DidWeaponSwap || IsAiming != _wasAimingStam || _regenStam != doRegen || _drainStamStam != doDrain || _neutralStam != doNeutral || _lastRecordedStanceStamina != CurrentStance || IsMounting != _wasMountingStam || IsBracing != _wasBracingStam)
            {
                if (doDrain)
                {
                    player.Physical.Aim(1f);
                }
                else if (doRegen)
                {
                    player.Physical.Aim(0f);
                }
                else if (doNeutral)
                {
                    player.Physical.Aim(1f);
                }
                HaveResetStamDrain = false;
            }

            //drain
            if (doDrain)
            {
                player.Physical.HandsStamina.Multiplier = GetDrainRate(player);
            }
            //regen
            else if (doRegen)
            {
                player.Physical.HandsStamina.Multiplier = GetRestoreRate();
            }
            //no drain or regen
            else if (doNeutral)
            {
                player.Physical.HandsStamina.Multiplier = 0f;
            }

            _regenStam = doRegen;
            _drainStamStam = doDrain;
            _neutralStam = doNeutral;
            _wasBracingStam = IsBracing;
            _wasMountingStam = IsMounting;
            _wasAimingStam = IsAiming;
            _lastRecordedStanceStamina = CurrentStance;
        }

        public static void ResetStanceStamina() 
        {
            _regenStam = false;
            _drainStamStam = false;
            _neutralStam = false;
            _wasBracingStam = false;
            _wasMountingStam = false;
            _wasAimingStam = false;
            _lastRecordedStanceStamina = EStance.None;
        }

        public static void UnarmedStanceStamina(Player player)
        {
            player.Physical.Aim(0f);
            player.Physical.HandsStamina.Multiplier = 1f;
            ResetStanceStamina();
        }

        public static bool IsIdle()
        {
            return CurrentStance == EStance.None && StoredStance == EStance.None && HasResetActiveAim && HasResetHighReady && HasResetLowReady && HasResetShortStock && HasResetPistolPos && HasResetMelee ? true : false;
        }

        public static void CancelAllStances()
        {
            StanceBlender.Target = 0f;
            CurrentStance = EStance.None;
            StoredStance = EStance.None;
            DidStanceWiggle = false;
            WasActiveAim = false;
            IsLeftShoulder = false;
        }

        private static void StanceManipCancelTimer()
        {
            _manipTime += Time.deltaTime;

            if (_manipTime >= ManipTimer)
            {
                CancelHighReady = false;
                ModifyHighReady = false;
                CancelLowReady = false;
                CancelShortStock = false;
                CancelPistolStance = false;
                CancelActiveAim = false;
                ShouldResetStances = false;
                CancelLeftShoulder = false;
                ManipTimer = 0.25f;
                _manipTime = 0f;
            }
        }

        private static void StanceDampingTimer()
        {
            DampingTimer += Time.deltaTime;

            if (DampingTimer >= 0.01f) //0.05f
            {
                CanResetDamping = true;
                DoDampingTimer = false;
                DampingTimer = 0f;
            }
        }

        public static void StanceShotTimer()
        {
            StanceShotTime += Time.deltaTime;

            if (StanceShotTime >= 0.55f)
            {
                IsFiringFromStance = false;
                StanceShotTime = 0f;
            }
        }

        private static void MeleeCooldownTimer()
        {
            _meleeTimer += Time.deltaTime;

            if (_meleeTimer >= 0.25f)
            {
                _doMeleeReset = false;
                MeleeIsToggleable = true;
                _meleeTimer = 0f;
            }
        }

        private static void DoMeleeEffect()
        {
            Player player = Singleton<GameWorld>.Instance.MainPlayer;
            Player.FirearmController fc = player.HandsController as Player.FirearmController;
            if (WeaponStats.HasBayonet)
            {

                int rndNum = UnityEngine.Random.Range(1, 11);
                string track = rndNum <= 5 ? "knife_1.wav" : "knife_2.wav";
                Singleton<BetterAudio>.Instance.PlayAtPoint(player.ProceduralWeaponAnimation.HandsContainer.WeaponRootAnim.position, Plugin.RealismAudioController.HitAudioClips[track], 2, BetterAudio.AudioSourceGroupType.Distant, 100, 2, EOcclusionTest.Continuous);
            }
            player.Physical.ConsumeAsMelee(2f + (WeaponStats.ErgoFactor / 100f));
        }

        private static void ToggleStance(EStance targetStance, bool setPrevious = false, bool setPrevisousAsCurrent = false)
        {
            _previousStance = _currentStance;
            if (IsLeftShoulder) IsLeftShoulder = false;
            if (setPrevious) StoredStance = CurrentStance;
            if (CurrentStance == targetStance) CurrentStance = EStance.None;
            else CurrentStance = targetStance;
            if (setPrevisousAsCurrent) StoredStance = CurrentStance;
        }

        private static void ToggleHighReady()
        {
            StanceBlender.Target = StanceBlender.Target == 0f ? 1f : 0f;
            ToggleStance(EStance.HighReady, false, true);
            WasActiveAim = false;
            DidStanceWiggle = false;

            if (CurrentStance == EStance.HighReady && (Plugin.RealHealthController.HealthConditionForcedLowReady))
            {
                CanDoHighReadyInjuredAnim = true;
            }
        }

        private static void ToggleLowReady()
        {
            StanceBlender.Target = StanceBlender.Target == 0f ? 1f : 0f;
            ToggleStance(EStance.LowReady, false, true);
            WasActiveAim = false;
            DidStanceWiggle = false;
        }

        private static void HandleScrollInput(float scrollIncrement)
        {
            if (scrollIncrement == -1)
            {
                if (CurrentStance == EStance.HighReady)
                {
                    ToggleHighReady();
                }
                else if (CurrentStance != EStance.LowReady && HasResetHighReady)
                {
                    ToggleLowReady();
                }
            }
            if (scrollIncrement == 1 && CurrentStance != EStance.HighReady)
            {
                if (CurrentStance == EStance.LowReady && !Plugin.RealHealthController.HealthConditionForcedLowReady)
                {
                    ToggleLowReady();
                }
                else if (CurrentStance != EStance.HighReady && HasResetLowReady)
                {
                    ToggleHighReady();
                }
            }
        }

        public static void ToggleLeftShoulder()
        {
            Utils.GetYourPlayer().method_58(5f, false);
            IsLeftShoulder = !IsLeftShoulder;
            if (!TreatWeaponAsPistolStance)
            {
                CurrentStance = EStance.None;
                StoredStance = EStance.None;
                WasActiveAim = false;
                HaveSetActiveAim = false;
                DidStanceWiggle = false;
                StanceBlender.Target = 0f;
            }
        }

        public static void StanceUpdate()
        {
            if (Utils.WeaponIsReady && Utils.GetYourPlayer().MovementContext.CurrentState.Name != EPlayerState.Stationary)
            {
                if (DoDampingTimer)
                {
                    StanceDampingTimer();
                }

                if (_doMeleeReset)
                {
                    MeleeCooldownTimer();
                }

                //patrol
                if (!ShouldBlockAllStances && Input.GetKeyDown(PluginConfig.PatrolKeybind.Value.MainKey) && PluginConfig.PatrolKeybind.Value.Modifiers.All(Input.GetKey))
                {
                    Utils.GetYourPlayer().method_58(5f, false);
                    ToggleStance(EStance.PatrolStance);
                    StoredStance = EStance.None;
                    StanceBlender.Target = 0f;
                    DidStanceWiggle = false;
                }

                if (!PlayerState.IsSprinting && !PlayerState.IsInInventory && !TreatWeaponAsPistolStance)
                {
                    //cycle stances
                    if (!ShouldBlockAllStances && Input.GetKeyUp(PluginConfig.CycleStancesKeybind.Value.MainKey))
                    {
                        if (Time.time <= _doubleClickTime)
                        {
                            _clickTriggered = true;
                            StanceBlender.Target = 0f;
                            StanceIndex = 0;
                            CancelAllStances();
                            DidStanceWiggle = false;
                        }
                        else
                        {
                            _clickTriggered = false;
                            _doubleClickTime = Time.time + _clickDelay;
                        }
                    }
                    else if (!_clickTriggered)
                    {
                        if (Time.time > _doubleClickTime)
                        {
                            IsLeftShoulder = false;
                            StanceBlender.Target = 1f;
                            _clickTriggered = true;
                            StanceIndex++;
                            StanceIndex = StanceIndex > 3 ? 1 : StanceIndex;
                            CurrentStance = (EStance)StanceIndex;
                            StoredStance = CurrentStance;
                            DidStanceWiggle = false;
                            if (CurrentStance == EStance.HighReady && Plugin.RealHealthController.HealthConditionForcedLowReady)
                            {
                                CanDoHighReadyInjuredAnim = true;
                            }
                        }
                    }

                    //active aim
                    if (!PluginConfig.ToggleActiveAim.Value)
                    {
                        if ((!IsAiming && !ShouldBlockAllStances && Input.GetKey(PluginConfig.ActiveAimKeybind.Value.MainKey) && PluginConfig.ActiveAimKeybind.Value.Modifiers.All(Input.GetKey)) || (Input.GetKey(KeyCode.Mouse1) && !PlayerState.IsAllowedADS))
                        {
                            if (!HaveSetActiveAim)
                            {
                                DidStanceWiggle = false;
                            }
                            IsLeftShoulder = false;
                            StanceBlender.Target = 1f;
                            CurrentStance = EStance.ActiveAiming;
                            WasActiveAim = true;
                            HaveSetActiveAim = true;
                        }
                        else if (HaveSetActiveAim)
                        {
                            StanceBlender.Target = 0f;
                            CurrentStance = StoredStance;
                            WasActiveAim = false;
                            HaveSetActiveAim = false;
                            DidStanceWiggle = false;
                        }
                    }
                    else
                    {
                        if ((!IsAiming && !ShouldBlockAllStances && Input.GetKeyDown(PluginConfig.ActiveAimKeybind.Value.MainKey) && PluginConfig.ActiveAimKeybind.Value.Modifiers.All(Input.GetKey)) || (Input.GetKeyDown(KeyCode.Mouse1) && !PlayerState.IsAllowedADS))
                        {
                            StanceBlender.Target = StanceBlender.Target == 0f ? 1f : 0f;
                            ToggleStance(EStance.ActiveAiming);
                            WasActiveAim = CurrentStance == EStance.ActiveAiming ? true : false;
                            DidStanceWiggle = false;
                            if (CurrentStance != EStance.ActiveAiming)
                            {
                                CurrentStance = StoredStance;
                            }
                        }
                    }

                    if (!ShouldBlockAllStances && PluginConfig.UseMouseWheelStance.Value && !IsAiming)
                    {
                        if ((Input.GetKey(PluginConfig.StanceWheelComboKeyBind.Value.MainKey) && PluginConfig.UseMouseWheelPlusKey.Value) || (!PluginConfig.UseMouseWheelPlusKey.Value && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.R) && !Input.GetKey(KeyCode.C)))
                        {
                            float scrollDelta = Input.mouseScrollDelta.y;
                            if (scrollDelta != 0f)
                            {
                                HandleScrollInput(scrollDelta);
                            }
                        }
                    }

                    //Melee
                    if (!IsAiming && MeleeIsToggleable && Input.GetKeyDown(PluginConfig.MeleeKeybind.Value.MainKey) && PluginConfig.MeleeKeybind.Value.Modifiers.All(Input.GetKey))
                    {
                        IsMounting = false;
                        IsLeftShoulder = false;
                        CurrentStance = EStance.Melee;
                        StoredStance = EStance.None;
                        WasActiveAim = false;
                        DidStanceWiggle = false;
                        StanceBlender.Target = 1f;
                        MeleeIsToggleable = false;
                        MeleeHitSomething = false;
                    }

                    //short-stock
                    if (!ShouldBlockAllStances && Input.GetKeyDown(PluginConfig.ShortStockKeybind.Value.MainKey) && PluginConfig.ShortStockKeybind.Value.Modifiers.All(Input.GetKey))
                    {
                        StanceBlender.Target = StanceBlender.Target == 0f ? 1f : 0f;
                        ToggleStance(EStance.ShortStock, false, true);
                        WasActiveAim = false;
                        DidStanceWiggle = false;
                    }

                    //high ready
                    if (!ShouldBlockAllStances && !IsInForcedLowReady && Input.GetKeyDown(PluginConfig.HighReadyKeybind.Value.MainKey) && PluginConfig.HighReadyKeybind.Value.Modifiers.All(Input.GetKey))
                    {
                        ToggleHighReady();
                    }

                    //low ready
                    if (!ShouldBlockAllStances && !IsInForcedLowReady && Input.GetKeyDown(PluginConfig.LowReadyKeybind.Value.MainKey) && PluginConfig.LowReadyKeybind.Value.Modifiers.All(Input.GetKey))
                    {
                        ToggleLowReady();
                    }

                    //cancel if aiming
                    if (IsAiming)
                    {
                        if (CurrentStance == EStance.ActiveAiming || WasActiveAim)
                        {
                            StoredStance = EStance.None;
                        }
                        CurrentStance = EStance.None;
                        HaveSetAiming = true;
                    }
                    else if (HaveSetAiming)
                    {
                        CurrentStance = WasActiveAim ? EStance.ActiveAiming : StoredStance;
                        HaveSetAiming = false;
                    }
                }


                if (ShootController.IsFiring) //stnace specific firing check is too slow
                {
                    bool rememberStance = PluginConfig.RememberStanceFiring.Value && IsAiming;
                    bool isActiveAim = CurrentStance == EStance.ActiveAiming && !IsAiming;
                    bool keepStance = rememberStance || (isActiveAim || CurrentStance == EStance.ShortStock || CurrentStance == EStance.PistolCompressed);

                    if (!keepStance)
                    {
                        CurrentStance = EStance.None;
                        StoredStance = EStance.None;
                        StanceBlender.Target = 0f;
                    }
                }

                if (CanDoHighReadyInjuredAnim)
                {
                    HighReadyBlackedArmTime += Time.deltaTime;
                    if (HighReadyBlackedArmTime >= 0.35f)
                    {
                        CanDoHighReadyInjuredAnim = false;
                        CurrentStance = EStance.LowReady;
                        StoredStance = EStance.LowReady;
                        HighReadyBlackedArmTime = 0f;
                    }
                }

                if (ShouldForceLowReady)
                {
                    StanceBlender.Target = 1f;
                    CurrentStance = EStance.LowReady;
                    StoredStance = EStance.LowReady;
                    WasActiveAim = false;
                    IsLeftShoulder = false;
                    IsInForcedLowReady = true;
                }
                else IsInForcedLowReady = false;
            }

            if (ShouldResetStances)
            {
                StanceManipCancelTimer();
            }

            if (DidWeaponSwap || (!PluginConfig.RememberStanceItem.Value && !Utils.WeaponIsReady) || !Utils.PlayerIsReady)
            {
                IsLeftShoulder = false;
                IsMounting = false;
                CurrentStance = EStance.None;
                StoredStance = EStance.None;
                StanceBlender.Target = 0f;
                StanceIndex = 0;
                WasActiveAim = false;
                DidWeaponSwap = false;
                AimingInterrupted = false;
                ResetStanceStamina();
            }
        }

        private static void DoTacSprint(Player.FirearmController fc, Player player)
        {
            if (CanDoTacSprint)
            {
                IsDoingTacSprint = true;
                player.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, 2f);
                _tacSprintTime = 0f;
                _canDoTacSprintTimer = true;
            }
            else if (PluginConfig.EnableTacSprint.Value && _canDoTacSprintTimer)
            {
                _tacSprintTime += Time.deltaTime;
                if (_tacSprintTime >= 0.5f)
                {
                    player.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, WeaponStats.TotalWeaponLength);
                    _tacSprintTime = 0f;
                    _canDoTacSprintTimer = false;
                }
                IsDoingTacSprint = false;
            }
            else
            {
                IsDoingTacSprint = false;
            }
        }

        public static void DoWiggleEffects(Player player, ProceduralWeaponAnimation pwa, Weapon weapon, Vector3 wiggleDirection, bool playSound = false, float volume = 4f, float wiggleFactor = 1f, bool isADS = false, bool useGearSound = false)
        {
            if (playSound)
            {
                player.method_58(volume * PluginConfig.StanceSfxModifier.Value, useGearSound);
            }

            NewRecoilShotEffect newRecoil = pwa.Shootingg.CurrentRecoilEffect as NewRecoilShotEffect;
            if (isADS)
            {
                newRecoil.HandRotationRecoil.ReturnTrajectoryDumping = 0.3f * wiggleFactor;
                pwa.Shootingg.CurrentRecoilEffect.HandRotationRecoilEffect.Damping = 0.3f * wiggleFactor;
            }
            player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect.RecoilProcessValues[3].IntensityMultiplicator = 0;
            player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect.RecoilProcessValues[4].IntensityMultiplicator = 0;
            float count = pwa.Shootingg.CurrentRecoilEffect.RecoilProcessValues.Length;
            for (int i = 0; i < count; i++)
            {
                pwa.Shootingg.CurrentRecoilEffect.RecoilProcessValues[i].Process(wiggleDirection);
            }
            player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect.RecoilProcessValues[3].IntensityMultiplicator = 0;
            player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect.RecoilProcessValues[4].IntensityMultiplicator = 0;
        }

        private static void MoveGunToCameraPID(ProceduralWeaponAnimation pwa, float dt, float stanceMulti, ref float gunAxesTarget, ref float gunCameraAlignmentTarget, float camTargetAxes, float speedModifer, float tolerance = 0.001f, bool ignoreLeftShoulder = false)
        {
            if (!IsAiming)
            {
                gunCameraAlignmentTarget = camTargetAxes;
            }

            if (IsColliding || PistolIsColliding || !pwa.OverlappingAllowsBlindfire || StopCameraMovement || (IsDoingLeftShoulderNotBlocked && !ignoreLeftShoulder)) return;

            bool skipPIDForRifle = ShootController.IsFiringMovement && !PluginConfig.EnableAltRifleRecoil.Value && !TreatWeaponAsPistolStance;
            bool skipPIDForPistol = ShootController.IsFiringMovement && TreatWeaponAsPistolStance;
            if (IsAiming && !skipPIDForRifle && !skipPIDForPistol)
            {
                float speed = speedModifer * stanceMulti;

                // Calculate difference
                float error = gunCameraAlignmentTarget - camTargetAxes;

                if (Mathf.Abs(error) > tolerance)
                {
                    // Convert error into a vertical offset
                    // (positive error = move weapon upward, negative = downward)
                    float adjustment = error * speed * dt;

                    gunAxesTarget += adjustment;
                }
            }
        }

        private static Vector3 GetRifleStancePIDModifier()
        {
            if (StoredStance == EStance.HighReady)
                return new Vector3(0.6f, 0.35f, 1f);
            if (StoredStance == EStance.LowReady)
                return new Vector3(0.8f, 0.7f, 1f);
            if (StoredStance == EStance.ShortStock)
                return new Vector3(0.5f, 0.3f, 1f);
            if (StoredStance == EStance.ActiveAiming || WasActiveAim)
                return new Vector3(1.5f, 0.75f, 1f);

            return Vector3.one;
        }

        //non-stance related rotational and postion changes for immersion
        public static void DoExtraPosAndRot(ProceduralWeaponAnimation pwa, Player player)
        {
            //position
            float stockOffset = !WeaponStats.IsPistol && !WeaponStats.HasShoulderContact ? -0.04f : 0f;
            float stockPosOffset = WeaponStats.StockPosition * 0.01f;
            float posOffsetMulti = WeaponStats.HasShoulderContact ? -0.04f : 0.04f;
            float posePosOffset = (1f - player.MovementContext.PoseLevel) * posOffsetMulti;

            float targetPosXOffset = pwa.IsAiming ? 0f : 0f;
            float targetPosYOffset = pwa.IsAiming ? 0f : 0f;
            float targetPosZOffset = pwa.IsAiming ? 0f : Mathf.Clamp(posePosOffset + stockOffset + stockPosOffset, -0.05f, 0.05f);
            Vector3 targetPos = new Vector3(targetPosXOffset, targetPosYOffset, targetPosZOffset);

            _posePosOffest = Vector3.Lerp(_posePosOffest, targetPos, 5f * Time.deltaTime);
            pwa.HandsContainer.WeaponRoot.localPosition += _posePosOffest;

            //rotation
            bool isMountedWithBipod = WeaponStats.BipodIsDeployed && StanceController.IsMounting;
            bool doCantedSightOffset = IsCantedAiming(pwa, true);
            bool doMaskOffset = !doCantedSightOffset && !isMountedWithBipod && (GearController.HasGasMask || (GearController.FSIsActive && GearController.GearBlocksMouth)) && !WeaponStats.WeaponCanFSADS && pwa.IsAiming && WeaponStats.HasShoulderContact && !WeaponStats.IsStocklessPistol && !WeaponStats.IsMachinePistol;
            bool doLongMagOffset = WeaponStats.HasLongMag && player.IsInPronePose && !isMountedWithBipod;
            float cantedOffsetBase = -0.41f;
            float magOffset = doCantedSightOffset ? 0f : doLongMagOffset && !pwa.IsAiming ? -0.35f : doLongMagOffset && pwa.IsAiming ? -0.12f : 0f;
            float ergoOffset = WeaponStats.ErgoFactor * -0.001f;
            float poseRotOffset = (1f - player.MovementContext.PoseLevel) * -0.03f;
            poseRotOffset += player.IsInPronePose ? -0.03f : 0f;
            float maskFactor = doMaskOffset ? -0.025f + ergoOffset : 0f;
            float baseRotOffset = pwa.IsAiming || StanceController.IsMounting || StanceController.IsBracing ? 0f : poseRotOffset + ergoOffset;
            float cantedSightOffset = doCantedSightOffset ? cantedOffsetBase : 0f;

            float rotX = 0f;
            float rotY = Mathf.Clamp(baseRotOffset + maskFactor + magOffset, -0.5f, 0f) + cantedSightOffset;
            float rotZ = 0f;
            Vector3 targetRot = new Vector3(rotX, rotY, rotZ);

            _poseRotOffest = Vector3.Lerp(_poseRotOffest, targetRot, 5f * Time.deltaTime); //speeds should be affected by stance multi? or player crouch speed?

            Quaternion newRot = Quaternion.identity;
            newRot.x = _poseRotOffest.x;
            newRot.y = _poseRotOffest.y;
            newRot.z = _poseRotOffest.z;
            pwa.HandsContainer.WeaponRoot.localRotation *= newRot;
        }

        private static void CheckLeftShoulder(Player player, Player.FirearmController fc, ProceduralWeaponAnimation pwa, float stanceMulti, float dt, Vector3 posTarget, Vector3 rotTarget, float rotSpeed, float curveModifier = 1f)
        {
            float baseSpeed = Mathf.Clamp((1f - stanceMulti) + 1f, 0.05f, 1.5f);
            float speed = IsAiming ? baseSpeed * 0.22f : baseSpeed * 0.22f;

            //position

            var xTarget = posTarget.x + PluginConfig.LeftShoulderOffset.Value;
            var position = IsDoingLeftShoulderNotBlocked
                ? new Vector3(xTarget, posTarget.y, posTarget.z + (_leffPosZCurve.Evaluate(_leftStanceProgress) * curveModifier))
                : new Vector3(0f, 0f, _leffPosZCurveReturn.Evaluate(_leftStanceProgress) * curveModifier);

            if (IsDoingLeftShoulderNotBlocked)
            {
                _leftStanceTargetX = xTarget;
                _leftStanceTime = 0f;
                _isLeftStanceResetState = false;
            }
            else
            {        
                _leftStanceTime += dt;
                if (_leftStanceTime <= 0.5f)
                {
                    _isLeftStanceResetState = true;
                }
                else
                {
                    _isLeftStanceResetState = false;
                }
            }

            _leftStancePosition = Vector3.SmoothDamp(_leftStancePosition, position, ref _leftStanceVelocity, speed, 0.55f, dt);

            _leftStanceProgress = Mathf.InverseLerp(0f, _leftStanceTargetX, _leftStancePosition.x);

            if (Utils.AreFloatsEqual(_leftStanceProgress, 0f) && !IsLeftShoulder) HaveResetLeftShoulder = true;
            else HaveResetLeftShoulder = false;

            //moving towards 1, and is left shoulder
            bool isTransitionignLeft = IsLeftShoulder && Utils.IsLessThan(_leftStanceProgress, 0.99f);
            bool isTransitioningRight = (_isLeftStanceResetState || !IsLeftShoulder) && Utils.IsGreaterThan(_leftStanceProgress, 0.01f);

            if (IsAiming && (isTransitionignLeft || isTransitioningRight))
            {
                InterruptAim(fc);
            } 
            if (!isTransitionignLeft && !isTransitioningRight)
            {
                UnInterruptAim(fc);
            }

            //rotation
            var rotation = IsDoingLeftShoulderNotBlocked && !IsAiming ? rotTarget : Vector3.zero;
            rotation.x += _leftRotationXCurve.Evaluate(_leftStanceProgress);

            _leftStanceRotaiton = Vector3.Lerp(_leftStanceRotaiton, rotation, rotSpeed * dt);
            Quaternion newRot = Quaternion.Euler(_leftStanceRotaiton);

            pwa.HandsContainer.WeaponRoot.localRotation *= newRot;
        }

        //I've no idea wtf is going on here but it sort of works
        private static void HandleAltPistolPosition(Player player, Player.FirearmController fc, ProceduralWeaponAnimation pwa, float stanceMulti, float dt, Vector3 camTarget)
        {
            //left stance speed
            float leftResetSpeedModi = _isLeftStanceResetState ? 0.2f : 1f;

            //speed
            float fpsFactor = Mathf.Pow(Utils.GetFPSFactor(), 0.25f);
            float speedFactorTarget = IsAiming ? PluginConfig.PistolPosResetSpeedMulti.Value * stanceMulti : PluginConfig.PistolPosSpeedMulti.Value * stanceMulti;
            float pidSpeed = fpsFactor * leftResetSpeedModi * PluginConfig.PistolPosResetSpeedMulti.Value;
            _pistolPosSpeed = Mathf.Lerp(_pistolPosSpeed, speedFactorTarget, dt * 10f);

            if (!IsAiming)
            {
                _gunXTarget = !IsBlindFiring ? 0.038f : 0f;
                _gunYTarget = -0.0385f;
                _gunZTarget =  0f;
            }

            CheckLeftShoulder(player, fc, pwa, _pistolPosSpeed, dt, _leftStancePistolPositionTarget, _leftStancePistolRotaitonTarget, stanceMulti * 2.5f, 0.05f);

            if (Plugin.FOVFixPresent) 
            {
                MoveGunToCameraPID(pwa, dt, stanceMulti, ref _gunXTarget, ref _gunCameraAlignmentTargetX, camTarget.x, 0.15f * pidSpeed, 0.0001f);
                MoveGunToCameraPID(pwa, dt, stanceMulti, ref _gunYTarget, ref _gunCameraAlignmentTargetY, camTarget.y, 0.3f * pidSpeed, ignoreLeftShoulder: true);
                MoveGunToCameraPID(pwa, dt, stanceMulti, ref _gunZTarget, ref _gunCameraAlignmentTargetZ, camTarget.z, 0.4f * pidSpeed, ignoreLeftShoulder: true);
            }

            _currentPistolXPos = Mathf.Lerp(_currentPistolXPos, _gunXTarget, dt * _pistolPosSpeed);
            _currentPistolYPos = Mathf.Lerp(_currentPistolYPos, _gunYTarget, dt * _pistolPosSpeed);
            _currentPistolZPos = Mathf.Lerp(_currentPistolZPos, _gunZTarget, dt * _pistolPosSpeed); 

            _pistolLocalPosition.x = _currentPistolXPos + _leftStancePosition.x;
            _pistolLocalPosition.y = _currentPistolYPos + _leftStancePosition.y;
            _pistolLocalPosition.z = _currentPistolZPos + _leftStancePosition.z;

            pwa.HandsContainer.WeaponRoot.localPosition = _pistolLocalPosition;

        }

        private static void HandleRiflePosition(Player player, Player.FirearmController fc, ProceduralWeaponAnimation pwa, float stanceMulti, float movementFactor, float dt, Vector3 camTarget)
        {
            //left stance speeds
            float leftResetPidModi = _isLeftStanceResetState ? 0f : 1f;

            //speeds
            float fpsFactor = Mathf.Pow(Utils.GetFPSFactor(), 0.25f);   
            float posSpeed = IsAiming ? 30f * WeaponStats.TotalFinalAimSpeed : 6f * WeaponStats.TotalFinalAimSpeed;
            float pidSpeed = 30f * fpsFactor * leftResetPidModi;
            Vector3 stanceModifer = GetRifleStancePIDModifier();

            bool isCantedAiming = IsCantedAiming(pwa, false);
            bool adjustSpeedForCant = isCantedAiming && WasActiveAim;

            if (!IsAiming) 
            {
                _gunXTarget = BaseWeaponOffsetPosition.x + PluginConfig.WeapOffset.Value.x;
                _gunYTarget = BaseWeaponOffsetPosition.y + PluginConfig.WeapOffset.Value.y;
                _gunZTarget = BaseWeaponOffsetPosition.z + PluginConfig.WeapOffset.Value.z;
            }

            CheckLeftShoulder(player, fc, pwa, stanceMulti, dt, _leftStanceRiflePositionTarget, _leftStanceRifleRotaitonTarget, stanceMulti * 4.5f);

            if (PluginConfig.EnableAltRifle.Value && Plugin.FOVFixPresent)
            {
                MoveGunToCameraPID(pwa, dt, WeaponStats.TotalFinalAimSpeed, ref _gunXTarget, ref _gunCameraAlignmentTargetX, camTarget.x, 0.3f * pidSpeed * stanceModifer.x, 0.0001f);
                MoveGunToCameraPID(pwa, dt, WeaponStats.TotalFinalAimSpeed, ref _gunYTarget, ref _gunCameraAlignmentTargetY, camTarget.y, 0.3f * pidSpeed * stanceModifer.y, 0.0001f, true);
                MoveGunToCameraPID(pwa, dt, WeaponStats.TotalFinalAimSpeed, ref _gunZTarget, ref _gunCameraAlignmentTargetZ, camTarget.z, 0.3f * pidSpeed * stanceModifer.z, 0.0001f, true);
            }
          
            _currentRifleXPos = Mathf.Lerp(_currentRifleXPos, _gunXTarget, dt * posSpeed);
            _currentRifleYPos = Mathf.Lerp(_currentRifleYPos, _gunYTarget, dt * posSpeed); //if trying to fix stance ADS, animspeed might be fucking with things
            _currentRifleZPos = Mathf.Lerp(_currentRifleZPos, _gunZTarget, dt * posSpeed);

            _rifleLocalPosition.x = _currentRifleXPos + _leftStancePosition.x;
            _rifleLocalPosition.y = _currentRifleYPos + _leftStancePosition.y;
            _rifleLocalPosition.z = _currentRifleZPos + _leftStancePosition.z;

            pwa.HandsContainer.WeaponRoot.localPosition = _rifleLocalPosition;
        }

        public static void DoPistolStances(bool isThirdPerson, EFT.Animations.ProceduralWeaponAnimation pwa, float dt, Player player, Player.FirearmController fc, Vector3 camTarget)
        {
            bool useThirdPersonStance = isThirdPerson;//  || Plugin.IsUsingFika
            float totalPlayerWeight = PlayerState.TotalModifiedWeightMinusWeapon;
            float playerWeightFactor = 1f + (totalPlayerWeight / 100f);
            float ergoMulti = Mathf.Clamp(WeaponStats.ErgoStanceSpeed * Mathf.Pow(WeaponStats.TotalWeaponHandlingModi, 0.5f), 0.65f, 1.45f);
            float stanceMulti = Mathf.Clamp(ergoMulti * PlayerState.StanceInjuryMulti * Plugin.RealHealthController.AdrenalineStanceBonus * (Mathf.Max(PlayerState.RemainingArmStamFactor, 0.55f)), 0.5f, 1.45f);

            //float balanceFactor = 1f + (WeaponStats.Balance / 100f);
            // float rotationBalanceFactor = WeaponStats.Balance <= -9f ? -balanceFactor : balanceFactor;
            //float wiggleBalanceFactor = Mathf.Abs(WeaponStats.Balance) > 4f ? balanceFactor : Mathf.Abs(WeaponStats.Balance) <= 4f ? 0.75f : Mathf.Abs(WeaponStats.Balance) <= 3f ? 0.5f : 0.25f;
            float resetErgoMulti = (1f - stanceMulti) + 1f;

            float wiggleErgoMulti = Mathf.Clamp((WeaponStats.ErgoStanceSpeed * 0.25f), 0.1f, 1f);
            WiggleReturnSpeed = (1f - (PlayerState.AimSkillADSBuff * 0.5f)) * wiggleErgoMulti * PlayerState.StanceInjuryMulti * playerWeightFactor * (Mathf.Max(PlayerState.RemainingArmStamFactor, 0.65f));

            float movementFactor = PlayerState.IsMoving ? 0.8f : 1f;

            Quaternion pistolRevertQuaternion = Quaternion.Euler(PluginConfig.PistolResetRotation.Value); // * rotationBalanceFactor
            Vector3 pistolPMCTargetPosition = useThirdPersonStance ? PluginConfig.PistolThirdPersonPosition.Value : PluginConfig.PistolOffset.Value;
            Vector3 pistolScavTargetPosition = useThirdPersonStance ? new Vector3(0.01f, 0.025f, -0.015f) : new Vector3(0.01f, 0.025f, -0.015f);
            Vector3 pistolTargetPosition = PlayerState.IsScav ? pistolScavTargetPosition : pistolPMCTargetPosition;
            Vector3 pistolPMCTargetRotation = useThirdPersonStance ? PluginConfig.PistolThirdPersonRotation.Value : PluginConfig.PistolRotation.Value;
            Vector3 pistolScavTargetRotation = useThirdPersonStance ? new Vector3(2f, -10f, 0f) : new Vector3(2f, -10f, 0f);
            Vector3 pistolTargetRotation = PlayerState.IsScav ? pistolScavTargetRotation : pistolPMCTargetRotation;
            Quaternion pistolTargetQuaternion = Quaternion.Euler(pistolTargetRotation);
            Quaternion pistolMiniTargetQuaternion = Quaternion.Euler(PluginConfig.PistolAdditionalRotation.Value);

            //I've no idea wtf is going on here but it sort of works
            HandleAltPistolPosition(player, fc, pwa, stanceMulti, dt, camTarget);

            if (CurrentStance == EStance.PatrolStance) return;

            if (!pwa.IsAiming && !IsBlindFiring && !PistolIsColliding && !WeaponStats.HasShoulderContact && PluginConfig.EnableAltPistol.Value) //!CancelPistolStance && !pwa.LeftStance
            {
                if (CurrentStance == EStance.PatrolStance || _previousStance == EStance.PatrolStance) _SkipPistolWiggle = true;
                CurrentStance = EStance.PistolCompressed;
                StoredStance = EStance.None;
                IsResettingPistol = false;
                HasResetPistolPos = false;

                StanceBlender.Speed = PluginConfig.PistolPosSpeedMulti.Value * stanceMulti;
                StanceTargetPosition = Vector3.Lerp(StanceTargetPosition, pistolTargetPosition, PluginConfig.StanceTransitionSpeedMulti.Value * stanceMulti * dt);

                if (StanceBlender.Value < 1f)
                {
                    StanceRotationSpeed = 4f * stanceMulti * dt * PluginConfig.PistolAdditionalRotationSpeedMulti.Value * stanceMulti;
                    StanceRotation = pistolMiniTargetQuaternion;
                }
                else
                {
                    StanceRotationSpeed = 4f * stanceMulti * dt * PluginConfig.PistolRotationSpeedMulti.Value * stanceMulti * (useThirdPersonStance ? PluginConfig.ThirdPersonRotationSpeed.Value : 1f);
                    StanceRotation = pistolTargetQuaternion;
                }

                if (StanceTargetPosition == pistolTargetPosition && StanceBlender.Value >= 1f && !CanResetDamping)
                {
                    DoDampingTimer = true;
                }
                else if (StanceTargetPosition != pistolTargetPosition || StanceBlender.Value < 1)
                {
                    CanResetDamping = false;
                }

                if (StanceBlender.Value < 0.95f || CancelPistolStance)
                {
                    DidStanceWiggle = false;
                }
                if ((StanceBlender.Value >= 1f && StanceTargetPosition == pistolTargetPosition) && !DidStanceWiggle)
                {
                    if (!_SkipPistolWiggle && !IsLeftShoulder) DoWiggleEffects(player, pwa, fc.Weapon, new Vector3(-12.5f, 5f, 1f) * movementFactor);
                    DidStanceWiggle = true;
                    CancelPistolStance = false;
                    _SkipPistolWiggle = false;
                }

            }
            else if (StanceBlender.Value > 0f && !HasResetPistolPos && !PistolIsColliding)
            {
                CanResetDamping = false;

                IsResettingPistol = true;
                StanceRotationSpeed = 4f * stanceMulti * dt * PluginConfig.PistolResetRotationSpeedMulti.Value * stanceMulti * (useThirdPersonStance ? PluginConfig.ThirdPersonRotationSpeed.Value : 1f);
                StanceRotation = pistolRevertQuaternion;
                StanceBlender.Speed = PluginConfig.PistolPosResetSpeedMulti.Value * stanceMulti * (useThirdPersonStance ? PluginConfig.ThirdPersonPositionSpeed.Value : 1f);
            }
            else if (StanceBlender.Value == 0f && !HasResetPistolPos && !PistolIsColliding)
            {
                if (!CanResetDamping)
                {
                    DoDampingTimer = true;
                }

                if (!IsLeftShoulder) DoWiggleEffects(player, pwa, fc.Weapon, new Vector3(-10f, 0f, -20f) * movementFactor); //new Vector3(10f, 1f, -30f) * wiggleBalanceFactor * rotationBalanceFactor  * wiggleBalanceFactor

                IsResettingPistol = false;
                CurrentStance = EStance.None;
                StanceRotation = Quaternion.identity;
                HasResetPistolPos = true;
            }
        }

        public static void DoRifleStances(Player player, Player.FirearmController fc, bool isThirdPerson, EFT.Animations.ProceduralWeaponAnimation pwa, float dt, Vector3 camTarget)
        {
            float movementFactor = PlayerState.IsMoving ? 1.1f : 1f;
            bool useThirdPersonStance = isThirdPerson; // || Plugin.IsUsingFika
            float totalPlayerWeight = PlayerState.TotalModifiedWeightMinusWeapon;
            float playerWeightFactor = 1f + (totalPlayerWeight / 150f);
            float lowerBaseLimit = WeaponStats.TotalWeaponWeight >= STANCE_WEIGHT_LIMIT_KG ? 0.45f : 0.55f;
            float ergoMulti = Mathf.Clamp(1.15f * WeaponStats.ErgoStanceSpeed * Mathf.Pow(WeaponStats.TotalWeaponHandlingModi, 0.4f), lowerBaseLimit, 1.2f);
            float lowerSpeedLimit = WeaponStats.TotalWeaponWeight >= STANCE_WEIGHT_LIMIT_KG ? 0.3f : 0.4f;
            float stanceMulti = Mathf.Clamp(ergoMulti * PlayerState.StanceInjuryMulti * Plugin.RealHealthController.AdrenalineStanceBonus * (Mathf.Max(PlayerState.RemainingArmStamFactor, 0.65f)), lowerSpeedLimit, 1.18f);
            float resetErgoMulti = (1f - stanceMulti) + 1f;

            bool pauseStance = PlayerState.IsInInventory || IsBlindFiring || IsLeftShoulder;
             
            float wiggleErgoMulti = Mathf.Clamp((WeaponStats.ErgoStanceSpeed * 0.5f), 0.1f, 1f);
            float stocklessModifier = WeaponStats.HasShoulderContact ? 1f : 0.5f;
            WiggleReturnSpeed = (1f - (PlayerState.AimSkillADSBuff * 0.5f)) * wiggleErgoMulti * PlayerState.StanceInjuryMulti * stocklessModifier * playerWeightFactor * (Mathf.Max(PlayerState.RemainingArmStamFactor, 0.55f));
          
            //for setting baseline position
            if (!isThirdPerson)
            {
                HandleRiflePosition(player, fc, pwa, stanceMulti, movementFactor, dt, camTarget);
            }

            DoTacSprint(fc, player);

            ////short-stock////
            DoShortStock(player, fc, isThirdPerson, pwa, dt, useThirdPersonStance, stanceMulti, resetErgoMulti, pauseStance, movementFactor);

            ////high ready////
            DoHighReady(player, fc, isThirdPerson, pwa, dt, useThirdPersonStance, stanceMulti, resetErgoMulti, pauseStance, movementFactor);

            ////low ready////
            DoLowReady(player, fc, isThirdPerson, pwa, dt, useThirdPersonStance, stanceMulti, resetErgoMulti, pauseStance, movementFactor);

            ////active aiming////
            DoActiveAim(player, fc, isThirdPerson, pwa, dt, useThirdPersonStance, stanceMulti, resetErgoMulti, pauseStance, movementFactor);

            ////Melee////
            DoMeleeStance(player, fc, isThirdPerson, pwa, dt, useThirdPersonStance, stanceMulti, resetErgoMulti, pauseStance, movementFactor);

        }

        public static void DoShortStock(Player player, Player.FirearmController fc, bool isThirdPerson, EFT.Animations.ProceduralWeaponAnimation pwa, float dt, bool useThirdPersonStance, float stanceMulti, float resetErgoMulti, bool pauseStance, float movementFactor)
        {
            float shortStockStanceMulti = Mathf.Clamp(stanceMulti, 0.65f, 1.5f);

            Vector3 shortTargetRotation = useThirdPersonStance ?
                PluginConfig.ShortStockThirdPersonRotation.Value :
                PluginConfig.ShortStockRotation.Value * shortStockStanceMulti;
            Quaternion shortStockTargetQuaternion = Quaternion.Euler(shortTargetRotation);
            Quaternion shortStockMiniTargetQuaternion = Quaternion.Euler(PluginConfig.ShortStockAdditionalRotation.Value * resetErgoMulti);
            Quaternion shortStockRevertQuaternion = Quaternion.Euler(PluginConfig.ShortStockResetRotation.Value * resetErgoMulti);
            Vector3 shortStockTargetPosition = useThirdPersonStance ?
                PluginConfig.ShortStockThirdPersonPosition.Value :
                PluginConfig.ShortStockOffset.Value;

            if (CurrentStance == EStance.ShortStock && !pwa.IsAiming && !CancelShortStock && !IsBlindFiring && !pwa.LeftStance && !PlayerState.IsSprinting && !pauseStance)
            {
                float activeToShort = 1f;
                float highToShort = 1f;
                float lowToShort = 1f;
                IsResettingShortStock = false;
                HasResetShortStock = false;
                HasResetMelee = true;

                if (StanceTargetPosition != shortStockTargetPosition)
                {
                    if (!HasResetActiveAim)
                    {
                        activeToShort = 0.55f;
                    }
                    if (!HasResetHighReady)
                    {
                        highToShort = 0.78f;
                    }
                    if (!HasResetLowReady)
                    {
                        lowToShort = 0.55f;
                    }
                }
                else
                {
                    HasResetActiveAim = true;
                    HasResetHighReady = true;
                    HasResetLowReady = true;
                }

                if (StanceTargetPosition == shortStockTargetPosition && StanceBlender.Value >= 1f && !CanResetDamping)
                {
                    DoDampingTimer = true;
                }
                else if (StanceTargetPosition != shortStockTargetPosition || StanceBlender.Value < 1)
                {
                    CanResetDamping = false;
                }

                float transitionPositionFactor = activeToShort * highToShort * lowToShort;
                float transitionRotationFactor = activeToShort * highToShort * lowToShort;

                if (StanceBlender.Value < 1f)
                {
                    StanceRotationSpeed = 4f * shortStockStanceMulti * dt * PluginConfig.ShortStockAdditionalRotationSpeedMulti.Value * (useThirdPersonStance ? PluginConfig.ThirdPersonRotationSpeed.Value : 1f) * transitionRotationFactor;
                    StanceRotation = shortStockMiniTargetQuaternion;
                }
                else
                {
                    StanceRotationSpeed = 4f * shortStockStanceMulti * dt * PluginConfig.ShortStockRotationMulti.Value * (useThirdPersonStance ? PluginConfig.ThirdPersonRotationSpeed.Value : 1f) * transitionRotationFactor;
                    StanceRotation = shortStockTargetQuaternion;
                }

                StanceBlender.Speed = PluginConfig.ShortStockSpeedMulti.Value * shortStockStanceMulti * (useThirdPersonStance ? PluginConfig.ThirdPersonPositionSpeed.Value : 1f);
                StanceTargetPosition = Vector3.Lerp(StanceTargetPosition, shortStockTargetPosition, PluginConfig.StanceTransitionSpeedMulti.Value * shortStockStanceMulti * transitionPositionFactor * dt);

                if ((StanceBlender.Value >= 0.9f || StanceTargetPosition == shortStockTargetPosition) && !DidStanceWiggle && !useThirdPersonStance)
                {
                    DoWiggleEffects(player, pwa, fc.Weapon, new Vector3(5f, -2.5f, 30f) * movementFactor, true);
                    DidStanceWiggle = true;
                }
            }
            else if (StanceBlender.Value > 0f && !HasResetShortStock && CurrentStance != EStance.LowReady && CurrentStance != EStance.ActiveAiming && CurrentStance != EStance.HighReady && !IsResettingActiveAim && !IsResettingHighReady && !IsResettingLowReady && !IsResettingMelee)
            {
                CanResetDamping = false;
                IsResettingShortStock = true;
                StanceRotationSpeed = 4f * shortStockStanceMulti * dt * PluginConfig.ShortStockResetRotationSpeedMulti.Value;
                StanceRotation = shortStockRevertQuaternion;
                StanceBlender.Speed = PluginConfig.ShortStockResetSpeedMulti.Value * shortStockStanceMulti * (useThirdPersonStance ? PluginConfig.ThirdPersonPositionSpeed.Value : 1f);
            }
            else if (StanceBlender.Value == 0f && !HasResetShortStock)
            {
                if (!CanResetDamping)
                {
                    DoDampingTimer = true;
                }

                if (!useThirdPersonStance) DoWiggleEffects(player, pwa, fc.Weapon, new Vector3(-4f, -2f, -30f) * movementFactor, true);
                DidStanceWiggle = false;
                StanceRotation = Quaternion.identity;
                IsResettingShortStock = false;
                HasResetShortStock = true;
            }
        }

        public static void DoHighReady(Player player, Player.FirearmController fc, bool isThirdPerson, EFT.Animations.ProceduralWeaponAnimation pwa, float dt, bool useThirdPersonStance, float stanceMulti, float resetErgoMulti, bool pauseStance, float movementFactor)
        {
            float highReadyStanceMulti = Mathf.Clamp(stanceMulti, 0.5f, 0.98f);
            float highReadyXWiggleFactor = WeaponStats.TotalErgo <= 49f ? -1f : 1f;
            float highReadyZWiggleFactor = WeaponStats.TotalErgo <= 40f ? 1f : 2f;

            Vector3 highTargetRotation = useThirdPersonStance ?
                PluginConfig.HighReadyThirdPersonRotation.Value :
                new Vector3(
                    PluginConfig.HighReadyRotation.Value.x * stanceMulti,
                    PluginConfig.HighReadyRotation.Value.y * stanceMulti * (ModifyHighReady ? -1f : 1f),
                    PluginConfig.HighReadyRotation.Value.z * stanceMulti);

            Vector3 highReadyTargetPosition = useThirdPersonStance ?
                PluginConfig.HighReadyThirdPersonPosition.Value :
                new Vector3(
                    PluginConfig.HighReadyOffset.Value.x,
                    PluginConfig.HighReadyOffset.Value.y * (ModifyHighReady ? 0.25f : 1f),
                    PluginConfig.HighReadyOffset.Value.z);

            Quaternion highReadyTargetQuaternion = Quaternion.Euler(highTargetRotation);
            Quaternion highReadyMiniTargetQuaternion = Quaternion.Euler(PluginConfig.HighReadyAdditionalRotation.Value * resetErgoMulti);
            Quaternion highReadyRevertQuaternion = Quaternion.Euler(PluginConfig.HighReadyResetRotation.Value * resetErgoMulti);

            if (CurrentStance == EStance.HighReady && !pwa.IsAiming && !IsFiringFromStance && !CancelHighReady && !pauseStance)
            {
                float shortToHighMulti = 1.0f;
                float lowToHighMulti = 1.0f;
                float activeToHighMulti = 1.0f;
                IsResettingHighReady = false;
                HasResetHighReady = false;
                HasResetMelee = true;

                if (StanceTargetPosition != highReadyTargetPosition)
                {
                    if (!HasResetShortStock)
                    {
                        shortToHighMulti = 0.82f;
                    }
                    if (!HasResetActiveAim)
                    {
                        activeToHighMulti = 1f;
                    }
                    if (!HasResetLowReady)
                    {
                        lowToHighMulti = 1f;
                    }
                }
                else
                {
                    HasResetActiveAim = true;
                    HasResetLowReady = true;
                    HasResetShortStock = true;
                }

                if (StanceTargetPosition == highReadyTargetPosition && StanceBlender.Value == 1 && !CanResetDamping)
                {
                    DoDampingTimer = true;
                }
                else if (StanceTargetPosition != highReadyTargetPosition || StanceBlender.Value < 1)
                {
                    CanResetDamping = false;
                }

                float transitionPositionFactor = shortToHighMulti * lowToHighMulti * activeToHighMulti;
                float transitionRotationFactor = shortToHighMulti * lowToHighMulti * activeToHighMulti * (transitionPositionFactor != 1f ? 0.9f : 1f);

                if (CanDoHighReadyInjuredAnim)
                {
                    if (StanceBlender.Value < 0.3f)
                    {
#warning replace this with bespoke rotation target and speed
                        Vector3 lowTargetRotation = useThirdPersonStance ?
                            PluginConfig.LowReadyThirdPersonRotation.Value :
                            new Vector3(
                                PluginConfig.LowReadyRotation.Value.x * resetErgoMulti,
                                PluginConfig.LowReadyRotation.Value.y,
                                PluginConfig.LowReadyRotation.Value.z);

                        Quaternion lowReadyTargetQuaternion = Quaternion.Euler(lowTargetRotation);

                        StanceRotationSpeed = 3f * highReadyStanceMulti * dt * PluginConfig.HighReadyRotationMulti.Value * (useThirdPersonStance ? PluginConfig.ThirdPersonRotationSpeed.Value * 0.7f : 1f) * (WeaponStats.IsPistol ? 0.5f : 1f);
                        StanceRotation = lowReadyTargetQuaternion;
                    }
                    else
                    {
                        StanceRotationSpeed = 3f * highReadyStanceMulti * dt * PluginConfig.HighReadyAdditionalRotationSpeedMulti.Value * (useThirdPersonStance ? PluginConfig.ThirdPersonRotationSpeed.Value * 0.2f : 1f) * (WeaponStats.IsPistol ? 0.5f : 1f);
                        StanceRotation = highReadyMiniTargetQuaternion;
                    }
                }
                else
                {
                    if (StanceBlender.Value < 0.3f)
                    {
                        StanceRotationSpeed = 4f * highReadyStanceMulti * dt * PluginConfig.HighReadyAdditionalRotationSpeedMulti.Value * (useThirdPersonStance ? PluginConfig.ThirdPersonRotationSpeed.Value * 0.2f : 1f) * transitionRotationFactor * (WeaponStats.IsPistol ? 0.5f : 1f);
                        StanceRotation = highReadyMiniTargetQuaternion;
                    }
                    else
                    {
                        StanceRotationSpeed = 4f * highReadyStanceMulti * dt * PluginConfig.HighReadyRotationMulti.Value * (useThirdPersonStance ? PluginConfig.ThirdPersonRotationSpeed.Value * 0.7f : 1f) * transitionRotationFactor * (WeaponStats.IsPistol ? 0.5f : 1f);
                        StanceRotation = highReadyTargetQuaternion;
                    }
                }

                StanceBlender.Speed = PluginConfig.HighReadySpeedMulti.Value * highReadyStanceMulti * (useThirdPersonStance ? PluginConfig.ThirdPersonPositionSpeed.Value : 1f);
                StanceTargetPosition = Vector3.Lerp(StanceTargetPosition, highReadyTargetPosition, PluginConfig.StanceTransitionSpeedMulti.Value * highReadyStanceMulti * transitionPositionFactor * dt);

                if ((StanceBlender.Value >= 1f || StanceTargetPosition == highReadyTargetPosition) && !DidStanceWiggle && !useThirdPersonStance)
                {
                    if (!WeaponStats.IsPistol) DoWiggleEffects(player, pwa, fc.Weapon, new Vector3(5f, 5f, 5f) * movementFactor, true);//new Vector3(11f, 5.5f, 50f)
                    DidStanceWiggle = true;
                }
            }
            else if (StanceBlender.Value > 0f && !HasResetHighReady && CurrentStance != EStance.LowReady && CurrentStance != EStance.ActiveAiming && CurrentStance != EStance.ShortStock && !IsResettingActiveAim && !IsResettingLowReady && !IsResettingShortStock && !IsResettingMelee)
            {
                CanResetDamping = false;
                IsResettingHighReady = true;
                StanceRotationSpeed = 4f * highReadyStanceMulti * dt * PluginConfig.HighReadyResetRotationMulti.Value * (useThirdPersonStance ? PluginConfig.ThirdPersonRotationSpeed.Value : 1f);
                StanceRotation = highReadyRevertQuaternion;
                StanceBlender.Speed = PluginConfig.HighReadyResetSpeedMulti.Value * highReadyStanceMulti * (useThirdPersonStance ? PluginConfig.ThirdPersonPositionSpeed.Value : 1f);
            }
            else if (StanceBlender.Value <= 0f && !HasResetHighReady)
            {
                if (!CanResetDamping)
                {
                    DoDampingTimer = true;
                }

                if (!useThirdPersonStance && !WeaponStats.IsPistol) DoWiggleEffects(player, pwa, fc.Weapon, new Vector3(highReadyXWiggleFactor * 10f, highReadyXWiggleFactor * 1f, highReadyZWiggleFactor * -10f) * movementFactor, true); //(1.5f, 3.75f, -30)
                DidStanceWiggle = false;
                StanceRotation = Quaternion.identity;
                IsResettingHighReady = false;
                HasResetHighReady = true;
            }
        }

        public static void DoLowReady(Player player, Player.FirearmController fc, bool isThirdPerson, EFT.Animations.ProceduralWeaponAnimation pwa, float dt, bool useThirdPersonStance, float stanceMulti, float resetErgoMulti, bool pauseStance, float movementFactor)
        {
            float lowReadyStanceMulti = Mathf.Clamp(stanceMulti, 0.5f, 0.98f);

            Vector3 lowTargetRotation = useThirdPersonStance ?
                PluginConfig.LowReadyThirdPersonRotation.Value :
                new Vector3(
                    PluginConfig.LowReadyRotation.Value.x * resetErgoMulti,
                    PluginConfig.LowReadyRotation.Value.y,
                    PluginConfig.LowReadyRotation.Value.z);

            Quaternion lowReadyTargetQuaternion = Quaternion.Euler(lowTargetRotation);
            Quaternion lowReadyMiniTargetQuaternion = Quaternion.Euler(PluginConfig.LowReadyAdditionalRotation.Value * resetErgoMulti);
            Quaternion lowReadyRevertQuaternion = Quaternion.Euler(PluginConfig.LowReadyResetRotation.Value * resetErgoMulti);

            Vector3 lowReadyTargetPosition = useThirdPersonStance ?
                PluginConfig.LowReadyThirdPersonPosition.Value :
                PluginConfig.LowReadyOffset.Value;

            if (CurrentStance == EStance.LowReady && !pwa.IsAiming && !IsFiringFromStance && !CancelLowReady && !pauseStance)
            {
                float highToLow = 1.0f;
                float shortToLow = 1.0f;
                float activeToLow = 1.0f;
                IsResettingLowReady = false;
                HasResetLowReady = false;
                HasResetMelee = true;

                if (StanceTargetPosition != lowReadyTargetPosition)
                {
                    if (!HasResetHighReady)
                    {
                        highToLow = 0.95f;
                    }
                    if (!HasResetShortStock)
                    {
                        shortToLow = 0.7f;
                    }
                    if (!HasResetActiveAim)
                    {
                        activeToLow = 0.87f;
                    }
                }
                else
                {
                    HasResetHighReady = true;
                    HasResetShortStock = true;
                    HasResetActiveAim = true;
                }

                if (StanceTargetPosition == lowReadyTargetPosition && StanceBlender.Value >= 1f && !CanResetDamping)
                {
                    DoDampingTimer = true;
                }
                else if (StanceTargetPosition != lowReadyTargetPosition || StanceBlender.Value < 1)
                {
                    CanResetDamping = false;
                }

                float transitionPositionFactor = highToLow * shortToLow * activeToLow;
                float transitionRotationFactor = highToLow * shortToLow * activeToLow * (transitionPositionFactor != 1f ? 1.025f : 1f);

                if (StanceBlender.Value < 1f)
                {
                    StanceRotationSpeed = 4f * lowReadyStanceMulti * dt * PluginConfig.LowReadyAdditionalRotationSpeedMulti.Value * (useThirdPersonStance ? PluginConfig.ThirdPersonRotationSpeed.Value * 0.8f : 1f) * transitionRotationFactor;
                    StanceRotation = lowReadyMiniTargetQuaternion;
                }
                else
                {
                    StanceRotationSpeed = 4f * lowReadyStanceMulti * dt * PluginConfig.LowReadyRotationMulti.Value * (useThirdPersonStance ? PluginConfig.ThirdPersonRotationSpeed.Value * 0.8f : 1f) * transitionRotationFactor;
                    StanceRotation = lowReadyTargetQuaternion;
                }

                StanceBlender.Speed = PluginConfig.LowReadySpeedMulti.Value * lowReadyStanceMulti * (useThirdPersonStance ? PluginConfig.ThirdPersonPositionSpeed.Value * 0.8f : 1f);
                StanceTargetPosition = Vector3.Lerp(StanceTargetPosition, lowReadyTargetPosition, PluginConfig.StanceTransitionSpeedMulti.Value * lowReadyStanceMulti * transitionPositionFactor * dt);

                if ((StanceBlender.Value >= 0.5f || StanceTargetPosition == lowReadyTargetPosition) && !DidStanceWiggle && !useThirdPersonStance)
                {
                    DoWiggleEffects(player, pwa, fc.Weapon, new Vector3(7f, 7f, 0f) * movementFactor, true);
                    DidStanceWiggle = true;
                }
                DidLowReadyResetStanceWiggle = false;
            }
            else if (StanceBlender.Value > 0f && !HasResetLowReady && CurrentStance != EStance.ActiveAiming && CurrentStance != EStance.HighReady && CurrentStance != EStance.ShortStock && !IsResettingActiveAim && !IsResettingHighReady && !IsResettingShortStock && !IsResettingMelee)
            {
                CanResetDamping = false;

                IsResettingLowReady = true;
                StanceRotationSpeed = 4f * lowReadyStanceMulti * dt * PluginConfig.LowReadyResetRotationMulti.Value * (useThirdPersonStance ? PluginConfig.ThirdPersonRotationSpeed.Value * 0.8f : 1f);
                StanceRotation = lowReadyRevertQuaternion;

                StanceBlender.Speed = PluginConfig.LowReadyResetSpeedMulti.Value * lowReadyStanceMulti * (useThirdPersonStance ? PluginConfig.ThirdPersonPositionSpeed.Value * 0.8f : 1f);

                if (!useThirdPersonStance && StanceBlender.Value <= 0.65f && !DidLowReadyResetStanceWiggle)
                {
                    DoWiggleEffects(player, pwa, fc.Weapon, new Vector3(-10f, 4f, 10f) * movementFactor, true); //new Vector3(-4f, 2.5f, 10f)
                    DidLowReadyResetStanceWiggle = true;
                }
            }
            else if (StanceBlender.Value == 0f && !HasResetLowReady)
            {
                if (!CanResetDamping)
                {
                    DoDampingTimer = true;
                }
                StanceRotation = Quaternion.identity;
                IsResettingLowReady = false;
                HasResetLowReady = true;
            }
        }

        public static void DoActiveAim(Player player, Player.FirearmController fc, bool isThirdPerson, EFT.Animations.ProceduralWeaponAnimation pwa, float dt, bool useThirdPersonStance, float stanceMulti, float resetErgoMulti, bool pauseStance, float movementFactor)
        {
            Vector3 activeTargetRoation =
                useThirdPersonStance ? PluginConfig.ActiveThirdPersonRotation.Value :
                PluginConfig.ActiveAimRotation.Value;

            Quaternion activeAimMiniTargetQuaternion =
                Quaternion.Euler(PluginConfig.ActiveAimAdditionalRotation.Value * resetErgoMulti);

            Quaternion activeAimRevertQuaternion =
                 IsCantedAiming(pwa, true) ? Quaternion.Euler(new Vector3(0f, 10f, -1f) * resetErgoMulti) :
                 Quaternion.Euler(PluginConfig.ActiveAimResetRotation.Value * resetErgoMulti);

            Vector3 activeAimTargetPosition = useThirdPersonStance ?
                PluginConfig.ActiveThirdPersonPosition.Value :
                PluginConfig.ActiveAimOffset.Value;

            Quaternion activeAimTargetQuaternion = Quaternion.Euler(activeTargetRoation);

            if (CurrentStance == EStance.ActiveAiming && !CancelActiveAim && !pauseStance)
            {
                float ergoFactor = WeaponStats.TotalErgo <= 40f ? 0.75f : 1f;
                float shortToActive = 1f;
                float shortToActiveRotation = 1f;
                float highToActive = 1f;
                float lowToActive = 1f;
                float highToActiveRotation = 1f;
                float lowToActiveRotation = 1f;
                IsResettingActiveAim = false;
                HasResetActiveAim = false;
                HasResetMelee = true;

                if (StanceTargetPosition != activeAimTargetPosition)
                {
                    if (!HasResetShortStock)
                    {
                        shortToActive = 0.45f;
                        shortToActiveRotation = 0.9f;
                    }
                    if (!HasResetHighReady)
                    {
                        highToActive = 1.15f;
                        highToActiveRotation = 1.15f;
                    }
                    if (!HasResetLowReady)
                    {
                        lowToActive = 1.29f;
                        lowToActiveRotation = 1.37f;
                    }
                }
                else
                {
                    HasResetShortStock = true;
                    HasResetHighReady = true;
                    HasResetLowReady = true;
                }

                if (StanceTargetPosition == activeAimTargetPosition && StanceBlender.Value == 1 && !CanResetDamping)
                {
                    DoDampingTimer = true;
                }
                else if (StanceTargetPosition != activeAimTargetPosition || StanceBlender.Value < 1)
                {
                    CanResetDamping = false;
                }

                float transitionPositionFactor = shortToActive * highToActive * lowToActive;
                float transitionRotationFactor = shortToActiveRotation * highToActiveRotation * lowToActiveRotation; //(transitionPositionFactor != 1f ? 0.9f : 1f)

                //additonal rotation makes ADS janky
                /*     if (StanceBlender.Value < 1f)
                     {

                         StanceTargetPosition = Vector3.Lerp(StanceTargetPosition, activeAimTargetPosition, PluginConfig.StanceTransitionSpeedMulti.Value * stanceMulti * transitionPositionFactor * dt);
                         rotationSpeed = 4f * stanceMulti * dt * ergoFactor * PluginConfig.ActiveAimAdditionalRotationSpeedMulti.Value * ChonkerFactor * (useThirdPersonStance ? PluginConfig.ThirdPersonRotationSpeed.Value : 1f) * transitionRotationFactor;
                         stanceRotation = activeAimMiniTargetQuaternion;
                     }
                     else
                     {
                         StanceTargetPosition = Vector3.Lerp(StanceTargetPosition, activeAimTargetPosition, PluginConfig.StanceTransitionSpeedMulti.Value * stanceMulti * transitionPositionFactor * dt);
                         rotationSpeed = 4f * stanceMulti * dt * ergoFactor * PluginConfig.ActiveAimRotationMulti.Value * ChonkerFactor * (useThirdPersonStance ? PluginConfig.ThirdPersonRotationSpeed.Value : 1f) * transitionRotationFactor;
                         stanceRotation = activeAimTargetQuaternion;
                     }*/

                StanceTargetPosition = Vector3.Lerp(StanceTargetPosition, activeAimTargetPosition, PluginConfig.StanceTransitionSpeedMulti.Value * stanceMulti * transitionPositionFactor * dt);
                StanceRotationSpeed = 4f * stanceMulti * dt * ergoFactor * PluginConfig.ActiveAimRotationSpeedMulti.Value * ChonkerFactor * (useThirdPersonStance ? PluginConfig.ThirdPersonRotationSpeed.Value : 1f) * transitionRotationFactor;
                StanceRotation = activeAimTargetQuaternion;

                StanceBlender.Speed = PluginConfig.ActiveAimPosSpeedMulti.Value * stanceMulti * ergoFactor * ChonkerFactor * (useThirdPersonStance ? PluginConfig.ThirdPersonPositionSpeed.Value : 1f);

                if ((StanceBlender.Value >= 1f || StanceTargetPosition == activeAimTargetPosition) && !DidStanceWiggle && !useThirdPersonStance)
                {
                    DoWiggleEffects(player, pwa, fc.Weapon, new Vector3(-10f, -10f, 0f), true, 3f);
                    DidStanceWiggle = true;
                }
            }
            else if (StanceBlender.Value > 0f && !HasResetActiveAim && CurrentStance != EStance.LowReady && CurrentStance != EStance.HighReady && CurrentStance != EStance.ShortStock && !IsResettingLowReady && !IsResettingHighReady && !IsResettingShortStock && !IsResettingMelee)
            {
                CanResetDamping = false;

                IsResettingActiveAim = true;
                StanceRotationSpeed = stanceMulti * dt * PluginConfig.ActiveAimResetRotationSpeedMulti.Value * ChonkerFactor * (useThirdPersonStance ? PluginConfig.ThirdPersonRotationSpeed.Value : 1f);
                StanceRotation = activeAimRevertQuaternion;
                StanceBlender.Speed = PluginConfig.ActiveAimResetSpeedMulti.Value * stanceMulti * ChonkerFactor * (useThirdPersonStance ? PluginConfig.ThirdPersonPositionSpeed.Value : 1f);
            }
            else if (StanceBlender.Value == 0f && !HasResetActiveAim)
            {
                if (!CanResetDamping)
                {
                    DoDampingTimer = true;
                }

                if (!useThirdPersonStance) DoWiggleEffects(player, pwa, fc.Weapon, new Vector3(-5f, 1.5f, 0f) * movementFactor, true, 3f);
                DidStanceWiggle = false;

                StanceRotation = Quaternion.identity;

                IsResettingActiveAim = false;
                HasResetActiveAim = true;
            }
        }

        public static void DoMeleeStance(Player player, Player.FirearmController fc, bool isThirdPerson, EFT.Animations.ProceduralWeaponAnimation pwa, float dt, bool useThirdPersonStance, float stanceMulti, float resetErgoMulti, bool pauseStance, float movementFactor)
        {
            if (WeaponStats.HasBayonet)
            {
                DoMeleeStanceBayonet(player, fc, isThirdPerson, pwa, dt, useThirdPersonStance, stanceMulti, resetErgoMulti, pauseStance, movementFactor);
                return;
            }

            bool isDoingMelee = CurrentStance == EStance.Melee && !pwa.IsAiming && !pauseStance;

            Quaternion meleeInitialQuaternion = Quaternion.Euler(new Vector3(2.5f * resetErgoMulti, -15f * resetErgoMulti, -1f));
            Quaternion meleeFinalQuaternion = Quaternion.Euler(new Vector3(-1.5f * resetErgoMulti, -7.5f * resetErgoMulti, -0.5f));
            Vector3 meleeInitialPos = new Vector3(0f, 0.06f, 0f);
            Vector3 meleeFinalPos = new Vector3(0f, -0.0275f, 0f);

            ////Melee////
            if (isDoingMelee && !PlayerState.IsSprinting)
            {
                IsResettingMelee = false;
                HasResetMelee = false;
                HasResetActiveAim = true;
                HasResetHighReady = true;
                HasResetLowReady = true;
                HasResetShortStock = true;

                if (StanceTargetPosition == meleeFinalPos && StanceBlender.Value >= 1f && !CanResetDamping)
                {
                    DoDampingTimer = true;
                }
                else if (StanceTargetPosition != meleeFinalPos || StanceBlender.Value < 1)
                {
                    CanResetDamping = false;
                }

                StanceRotationSpeed = 10f * Mathf.Clamp(stanceMulti, 0.8f, 1f) * dt * (useThirdPersonStance ? PluginConfig.ThirdPersonRotationSpeed.Value : 1f);

                float initialPosDistance = Vector3.Distance(StanceTargetPosition, meleeInitialPos);
                float finalPosDistance = Vector3.Distance(StanceTargetPosition, meleeFinalPos);

                if (initialPosDistance > 0.001f && !DidHalfMeleeAnim)
                {
                    StanceRotation = meleeInitialQuaternion;
                    StanceTargetPosition = Vector3.Lerp(StanceTargetPosition, meleeInitialPos, PluginConfig.StanceTransitionSpeedMulti.Value * Mathf.Clamp(stanceMulti, 0.75f, 1f) * dt * 1.5f * ChonkerFactor);
                }
                else
                {
                    DidHalfMeleeAnim = true;
                    StanceRotation = meleeFinalQuaternion;
                    StanceTargetPosition = Vector3.Lerp(StanceTargetPosition, meleeFinalPos, PluginConfig.StanceTransitionSpeedMulti.Value * Mathf.Clamp(stanceMulti, 0.75f, 1f) * dt * 2f * ChonkerFactor);
                }
                if (StanceBlender.Value >= 1f && finalPosDistance <= 0.001f && !DidStanceWiggle)
                {
                    DoMeleeEffect();
                    DoWiggleEffects(player, pwa, fc.Weapon, new Vector3(-20f, -10f, -90f) * movementFactor, true, 1f, useGearSound: true);
                    DidStanceWiggle = true;
                }

                if (StanceBlender.Value >= 0.9f && DidHalfMeleeAnim)
                {
                    CanDoMeleeDetection = true;
                }

                if (StanceBlender.Value >= 1f && finalPosDistance <= 0.001f)
                {
                    CurrentStance = StoredStance;
                    StanceBlender.Target = 0f;
                }
            }
            else if (StanceBlender.Value > 0f && !HasResetMelee) //&& !IsLowReady && !IsActiveAiming && !IsHighReady && !IsShortStock && !isResettingActiveAim && !isResettingHighReady && !isResettingLowReady && !isResettingShortStock
            {
                CanDoMeleeDetection = false;
                CanResetDamping = false;
                IsResettingMelee = true;
                StanceRotationSpeed = 10f * stanceMulti * dt;
                StanceRotation = Quaternion.identity;
                StanceBlender.Speed = 15f * stanceMulti * (useThirdPersonStance ? PluginConfig.ThirdPersonPositionSpeed.Value : 1f);
            }
            else if (StanceBlender.Value == 0f && !HasResetMelee)
            {
                _doMeleeReset = true;
                if (!CanResetDamping)
                {
                    DoDampingTimer = true;
                }
                StanceRotation = Quaternion.identity;
                IsResettingMelee = false;
                HasResetMelee = true;
                DidHalfMeleeAnim = false;
            }
        }

        public static void DoMeleeStanceBayonet(Player player, Player.FirearmController fc, bool isThirdPerson, EFT.Animations.ProceduralWeaponAnimation pwa, float dt, bool useThirdPersonStance, float stanceMulti, float resetErgoMulti, bool pauseStance, float movementFactor)
        {
            bool isDoingMelee = CurrentStance == EStance.Melee && !pwa.IsAiming && !pauseStance;
            _isHoldingBackMelee = Input.GetKey(PluginConfig.MeleeKeybind.Value.MainKey) && !MeleeHitSomething && isDoingMelee;

            Quaternion meleeInitialQuaternion = Quaternion.Euler(new Vector3(2.5f * resetErgoMulti, -15f * resetErgoMulti, -1f));
            Quaternion meleeFinalQuaternion = Quaternion.Euler(new Vector3(-1.5f * resetErgoMulti, -7.5f * resetErgoMulti, -0.5f));
            Vector3 meleeInitialPos = new Vector3(0f, 0.06f, 0f);
            Vector3 meleeFinalPos = new Vector3(0f, -0.0275f, 0f);

            if (isDoingMelee)
            {
                IsResettingMelee = false;
                HasResetMelee = false;
                HasResetActiveAim = true;
                HasResetHighReady = true;
                HasResetLowReady = true;
                HasResetShortStock = true;

                if (StanceTargetPosition == meleeFinalPos && StanceBlender.Value >= 1f && !CanResetDamping)
                {
                    DoDampingTimer = true;
                }
                else if (StanceTargetPosition != meleeFinalPos || StanceBlender.Value < 1)
                {
                    CanResetDamping = false;
                }

                StanceRotationSpeed = 10f * Mathf.Clamp(stanceMulti, 0.8f, 1f) * dt * (useThirdPersonStance ? PluginConfig.ThirdPersonRotationSpeed.Value : 1f);

                float initialPosDistance = Vector3.Distance(StanceTargetPosition, meleeInitialPos);
                float finalPosDistance = Vector3.Distance(StanceTargetPosition, meleeFinalPos);

                if ((initialPosDistance > 0.001f && !DidHalfMeleeAnim))
                {
                    StanceRotation = meleeInitialQuaternion;
                    StanceTargetPosition = Vector3.Lerp(StanceTargetPosition, meleeInitialPos, PluginConfig.StanceTransitionSpeedMulti.Value * Mathf.Clamp(stanceMulti, 0.75f, 1f) * dt * 1.5f * ChonkerFactor);
                }
                else
                {
                    DidHalfMeleeAnim = true;
                    if (!_isHoldingBackMelee)
                    {
                        StanceRotation = meleeFinalQuaternion;
                        StanceTargetPosition = Vector3.Lerp(StanceTargetPosition, meleeFinalPos, PluginConfig.StanceTransitionSpeedMulti.Value * Mathf.Clamp(stanceMulti, 0.75f, 1f) * dt * 2f * ChonkerFactor);
                    }
                }

                StanceBlender.Speed = 50f * (useThirdPersonStance ? PluginConfig.ThirdPersonPositionSpeed.Value : 1f);

                if (StanceBlender.Value >= 0.9f && !DidStanceWiggle && !MeleeHitSomething && !_isHoldingBackMelee) // && finalPosDistance <= 0.001f
                {
                    DoMeleeEffect();
                    DoWiggleEffects(player, pwa, fc.Weapon, new Vector3(-20f, -10f, -90f) * movementFactor, true, 1f, useGearSound: true);
                    DidStanceWiggle = true;
                }

                if (StanceBlender.Value >= 0.9f && DidHalfMeleeAnim)
                {
                    CanDoMeleeDetection = true;
                }

                if (StanceBlender.Value >= 1f && finalPosDistance <= 0.001f)
                {
                    CurrentStance = StoredStance;
                    StanceBlender.Target = 0f;
                }
            }
            else if (StanceBlender.Value > 0f && !HasResetMelee) 
            {
                CanDoMeleeDetection = false;
                CanResetDamping = false;
                IsResettingMelee = true;
                StanceRotationSpeed = 10f * stanceMulti * dt;
                StanceRotation = Quaternion.identity;
                StanceBlender.Speed = 15f * stanceMulti * (useThirdPersonStance ? PluginConfig.ThirdPersonPositionSpeed.Value : 1f);
            }
            else if (StanceBlender.Value == 0f && !HasResetMelee)
            {
                _doMeleeReset = true;
                if (!CanResetDamping)
                {
                    DoDampingTimer = true;
                }
                StanceRotation = Quaternion.identity;
                IsResettingMelee = false;
                HasResetMelee = true;
                DidHalfMeleeAnim = false;
            }
        }

        public static void DoPatrolStance(ProceduralWeaponAnimation pwa, Player player)
        {
            Vector3 patrolPos = StanceController.CurrentStance != EStance.PatrolStance ? Vector3.zero : WeaponStats.IsStocklessPistol || WeaponStats.IsMachinePistol ? _pistolPatrolPos : _riflePatrolPos;
            _patrolPos = Vector3.Lerp(_patrolPos, patrolPos, 5.5f * Time.deltaTime);
            pwa.HandsContainer.WeaponRoot.localPosition += _patrolPos;

            Vector3 patrolRot = StanceController.CurrentStance != EStance.PatrolStance ? Vector3.zero : WeaponStats.IsStocklessPistol || WeaponStats.IsMachinePistol ? _pistolPatrolRot : _riflePatrolRot;
            _patrolRot = Vector3.Lerp(_patrolRot, patrolRot, 5.5f * Time.deltaTime);

            Quaternion newRot = Quaternion.identity;
            newRot.x = _patrolRot.x;
            newRot.y = _patrolRot.y;
            newRot.z = _patrolRot.z;
            pwa.HandsContainer.WeaponRoot.localRotation *= newRot;

            if (Vector3.Distance(_patrolPos, Vector3.zero) <= 0.05f) StanceController.FinishedUnPatrolStancing = true;
            else StanceController.FinishedUnPatrolStancing = false;
        }

        ///
        //thanks and credit to lualeet's deadzone mod for this code, 0 jank compared to Realism's previous mounting system
        ///
        static void SetRotationWrapped(ref float yaw, ref float pitch)
        {
            // I prefer using (-180; 180) euler angle range over (0; 360)
            // However, wrapping the angles is easier with (0; 360), so temporarily cast it
            if (yaw < 0) yaw += 360;
            if (pitch < 0) pitch += 360;

            pitch %= 360;
            yaw %= 360;

            // Now cast it back
            if (yaw > 180) yaw -= 360;
            if (pitch > 180) pitch -= 360;
        }

        static void SetRotationClamped(ref float yaw, ref float pitch, float maxAngle)
        {
            Vector2 clampedVector
                = Vector2.ClampMagnitude(
                    new Vector2(yaw, pitch),
                    maxAngle
                );

            yaw = clampedVector.x;
            pitch = clampedVector.y;
        }

        static void UpdateAimSmoothed(ProceduralWeaponAnimation pwa, float deltaTime)
        {
            _mountAimSmoothed = Mathf.Lerp(_mountAimSmoothed, pwa.IsAiming ? 1f : 0f, deltaTime * 6f);
        }

        static void UpdateMountRotation(Vector2 currentYawPitch, float clamp)
        {
            Quaternion lastRotation = Quaternion.Euler(_lastMountYawPitch.x, _lastMountYawPitch.y, 0);
            Quaternion currentRotation = Quaternion.Euler(currentYawPitch.x, currentYawPitch.y, 0);

            _lastMountYawPitch = currentYawPitch;
            lastRotation = Quaternion.SlerpUnclamped(currentRotation, lastRotation, 0.115f);

            Vector3 delta = _makeQuaternionDelta(lastRotation, currentRotation).eulerAngles;

            _cumulativeMountYaw += delta.x;
            _cumulativeMountPitch += delta.y;

            SetRotationWrapped(ref _cumulativeMountYaw, ref _cumulativeMountPitch);
            SetRotationClamped(ref _cumulativeMountYaw, ref _cumulativeMountPitch, clamp);
        }

        static void ApplyPivotPoint(ProceduralWeaponAnimation pwa, Player player, float pivotPoint, float aimPivot)
        {
            float aimMultiplier = 1f - ((1f - aimPivot) * _mountAimSmoothed);

            Transform weaponRootAnim = pwa.HandsContainer.WeaponRootAnim;

            if (weaponRootAnim == null) return;

            weaponRootAnim.LocalRotateAround(Vector3.up * -pivotPoint, new Vector3( _cumulativeMountPitch * aimMultiplier, 0, _cumulativeMountYaw * aimMultiplier));

            // Not doing this messes up pivot for all offsets after this
            weaponRootAnim.LocalRotateAround(
                Vector3.up * pivotPoint,
                Vector3.zero
            );
        }

        public static void MountingPivotUpdate(Player player, ProceduralWeaponAnimation pwa, float clamp, float deltaTime, float pivotPoint = 0.75f, float aimPivot = 0.25f)
        {
            Vector2 currentYawPitch = new(player.MovementContext.Yaw, player.MovementContext.Pitch);

            UpdateMountRotation(currentYawPitch, clamp);
            UpdateAimSmoothed(pwa, deltaTime);
            ApplyPivotPoint(pwa, player, pivotPoint, aimPivot);
        }

        static readonly System.Diagnostics.Stopwatch aimWatch = new();
        public static float GetDeltaTime()
        {
            float deltaTime = aimWatch.Elapsed.Milliseconds / 1000f;
            aimWatch.Reset();
            aimWatch.Start();
            return deltaTime;
        }

        public static void ToggleMounting(Player player, ProceduralWeaponAnimation pwa, Player.FirearmController fc)
        {
           /* if (player.IsInPronePose && WeaponStats.BipodIsDeployed)
            {
                IsMounting = true;
            }*/
            if (IsMounting && PlayerState.IsMoving)
            {
                IsMounting = false;
            }
        }
    }
}

