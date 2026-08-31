using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;
using EFT;
using EFT.Interactive;

using tarkin.ladders.shared;
using Comfort.Common;

namespace tarkin.ladders.bep
{
    [DefaultExecutionOrder(100)] // after Player
    public class PlayerLadderController : MonoBehaviour
    {
        public Player Player => player;
        public Ladder Ladder => ladder;

        Player player;
        Ladder ladder;

        private const float BaseClimbSpeed = 1.2f;
        private const float BaseArmSpace = 0.49f;
        private const float XOffset = 0f;
        private const float CharacterControllerRadius = 0.25f;

        private const float RungSpeedVariation = 0.25f;
        private const float SideSwayAmount = 0.01f;
        private const float LeanSwayAmount = 0.3f;

        private const float ClimbStaminaDrainRate = 2f;
        private const float HoldStaminaDrainRate = 0.1f; // small drain to prevent regeneration while idle

        private float distanceSinceLastPainCheck = 0f;
        private const float PainDistanceThreshold = 0.5f; // Apply damage every 0.5 meters climbed
        private const float BrokenArmClimbDamage = 2f;

        private readonly CancellationTokenSource _destroyCts = new CancellationTokenSource();
        private bool isTransitioning = true;

        private float originalCapsuleColliderRadius;

        ProceduralLadderBody proceduralBody;

        private float ladderArmSpace;

        private Vector2 inputAxes;

        private float rollAngle = 0f;
        private float rollVelocity = 0f;
        private const float RollSpeed = 333f;
        private const float RollDeceleration = 0.35f;

        private bool IsBarMode => ladder.RungCount == 1;

        public static event Action<PlayerLadderController> OnPlayerLadderControllerInit;
        public event Action OnProceduralBodyCreate;
        public event Action OnProceduralBodyDestroy;
        public event Action<float> OnBarAngleChanged;

        static float CalculateArmSpace(Ladder ladder)
        {
            float tiltY = ladder.transform.forward.y;
            float sensitivity = 1.5f;
            float slopeFactor = 1.0f - (tiltY * sensitivity);
            slopeFactor = Mathf.Clamp(slopeFactor, 0.2f, 3.0f);

            return slopeFactor * BaseArmSpace;
        }

        public void Init(Ladder ladder)
        {
            this.ladder = ladder;
            this.ladderArmSpace = CalculateArmSpace(ladder);

            player = GetComponent<Player>();

            player.MovementContext.IsAxesIgnored = true;

            player.HideWeapon(); // sets IsInBufferZone = true

            player.MovementContext.SetPoseLevel(1f);

            _ = Transition(_destroyCts.Token);

            player.RestoreRibcageScale();

            if (player.CharacterController.GetCollider() is CapsuleCollider capsuleCollider)
            {
                originalCapsuleColliderRadius = capsuleCollider.radius;
                capsuleCollider.radius = CharacterControllerRadius;
            }

            Patch_Physical_CanClimb.OverrideCanClimb = true;
            Patch_Physical_CanVault.OverrideCanVault = true;
            Patch_ObstacleCalculatorModel_DistanceToMainObstacle.OverrideVaultObstacleDistance = true;

            player.OnPlayerDead += OnPlayerDead;

            Patch_MoveInputTranslator_TranslateAxes.Subscribe(player, OnPlayerInput);

            OnPlayerLadderControllerInit?.Invoke(this);
        }

        private void OnPlayerInput(Player player, ref float[] axes)
        {
            if (axes == null || axes.Length < 2) // not sure if this can actually happen
                return;
            inputAxes = new Vector2(axes[0], axes[1]);
        }

        private void OnPlayerDead(Player player, IPlayer lastAggressor, DamageInfo damageInfo, EBodyPart part)
        {
            Destroy(this);
        }

        void UpdateBarRoll()
        {
            float input = inputAxes.x;

            Transform centerOfMass = player.PlayerBones.Pelvis.Original;
            Transform pivotPoint = ladder.transform;
            Vector3 rotationAxis = ladder.transform.right;

            Vector3 currentComDirection = centerOfMass.position - pivotPoint.position;
            float currentMassAngle = Vector3.SignedAngle(Vector3.down, currentComDirection, rotationAxis);
            float leverArmDistance = currentComDirection.magnitude;

            float gravityAcceleration = Mathf.Sin(currentMassAngle * Mathf.Deg2Rad) * (Physics.gravity.y * 130f * leverArmDistance);

            float inputAcceleration = input * RollSpeed;

            rollVelocity += (gravityAcceleration + inputAcceleration) * Time.deltaTime;

            float friction = 0.8f;
            rollVelocity = Mathf.Lerp(rollVelocity, 0f, Time.deltaTime * friction);

            rollAngle += rollVelocity * Time.deltaTime;

            OnBarAngleChanged?.Invoke(rollAngle);
        }

        void LateUpdate()
        {
            if (!player.HandsIsEmpty)
                return;

            if (proceduralBody == null)
            {
                proceduralBody = new ProceduralLadderBody(player, ladder);
                OnProceduralBodyCreate?.Invoke();
            }

            proceduralBody.Update(rollAngle);
        }

        async Task Transition(CancellationToken cToken)
        {
            try
            {
                isTransitioning = true;
                player.MovementContext.SetCharacterMovementSpeed(0, true);

                Vector3 targetWorldPos = GetWorldPositionAtHeight(GetHeightOnLadder(player.Position));

                targetWorldPos.y = Mathf.Min(player.Position.y, ladder.transform.position.y + ladder.MaxHeight - 1.5f);

                InteractionParameters interactionParameters = new InteractionParameters
                {
                    InteractionPosition = targetWorldPos,
                    Grip = null,
                    AnimationId = 0,
                    ViewTarget = targetWorldPos - ladder.transform.forward,
                    Snap = true,
                    RotationMode = WorldInteractiveObject.ERotationInterpolationMode.ViewTargetWithZeroPitch,
                    BlockChangePosLevel = true
                };
                player.MovementContext.InteractionParameters = interactionParameters;


                ApproachState approachStateClass = null;
                IPlayerStateContainerBehaviour[] getInitedMovementState = player.MovementContext.GetInitedMovementState;
                for (int i = 0; i < getInitedMovementState.Length; i++)
                {
                    if (getInitedMovementState[i].EncapsulatedState is ApproachState approachStateClass2)
                    {
                        approachStateClass = approachStateClass2;
                        player.MovementContext.OverrideState(approachStateClass);
                        break;
                    }
                }

                if (approachStateClass == null)
                    return;

                while (!approachStateClass.ApproachDone)
                {
                    cToken.ThrowIfCancellationRequested();
                    await Task.Yield();
                }

                player.MovementContext.ExitOverridenState();

                while (player.HandsController.IsInInteraction())
                {
                    cToken.ThrowIfCancellationRequested();
                    await Task.Yield();
                }

                if (player.Position.y < ladder.transform.position.y - 2f)
                {
                    player.MovementContext.InputMotionBeforeLimit = Vector2.zero;
                    player.MovementContext.InputMotion = Vector2.zero;
                    player.Jump();
                    await Task.Yield();
                }

                while (!player.HandsIsEmpty)
                {
                    cToken.ThrowIfCancellationRequested();
                    await Task.Yield();
                }
                player.ChangeSpeed(1000);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error during Transition: {ex}");
                Destroy(this);
            }
            finally
            {
                isTransitioning = false;

                if (player.MovementContext.OverridenState is ApproachState)
                    player.MovementContext.ExitOverridenState();
            }
        }

        private float GetHeightOnLadder(Vector3 referencePosition)
        {
            Vector3 vectorFromBase = referencePosition - ladder.transform.position;
            return Vector3.Dot(vectorFromBase, ladder.transform.up);
        }

        private Vector3 GetWorldPositionAtHeight(float height)
        {
            float clampedHeight = Mathf.Min(height, ladder.MaxHeight - 1f);

            float sway = Mathf.Sin(clampedHeight / ladder.RungSpacing * Mathf.PI) * SideSwayAmount;

            Vector3 result = ladder.transform.position
                 + (ladder.transform.right * (XOffset + sway))
                 + (ladder.transform.up * clampedHeight)
                 + (ladder.transform.forward * ladderArmSpace);

            result.y = Mathf.Max(ladder.transform.position.y - 1.8f, result.y);

            return result;
        }

        void Update()
        {
            if (ladder == null)
            {
                Destroy(this);
                return;
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                player.Move(inputAxes);
                Destroy(this);
                return;
            }

            if (isTransitioning)
                return; 
            
            if (IsBarMode)
                UpdateBarRoll();
            else
                rollAngle = 0f;

            player.MovementContext.IsGrounded = true;

            if (player.MovementContext.CurrentState is JumpStateClass jumpState)
                jumpState.Float_10 = 999f;
            else
                player.MovementContext.ResetFlying();

            player.MovementContext.SetPoseLevel(1f);

            float currentHeight = GetHeightOnLadder(player.Position);

            float inputDir = inputAxes.y;

            if (TryExit(currentHeight, inputDir))
            {
                Destroy(this);
                return;
            }

            float rungPhase = (currentHeight % ladder.RungSpacing) / ladder.RungSpacing;
            float rungSpeedFactor = 1f - RungSpeedVariation * Mathf.Cos(rungPhase * Mathf.PI * 2f);

            float inventoryWeightFactor = Mathf.Clamp01(Mathf.InverseLerp(20, 60, player.InventoryController.TotalWeight()));

            bool isMoving = Mathf.Abs(inputDir) > 0.05f;
            float drainRate = isMoving ? ClimbStaminaDrainRate * Mathf.Lerp(1f, 2f, inventoryWeightFactor) : HoldStaminaDrainRate;
            player.Physical.ConsumeAsMelee(drainRate * Time.deltaTime);

            float currentSpeed = BaseClimbSpeed * Mathf.Lerp(1f, 0.5f, inventoryWeightFactor);

            if (Input.GetKey(KeyCode.LeftShift))
            {
                player.ChangeSpeed(1000);
#if DEBUG
                currentSpeed *= 3f;
#endif
            }

            if (player.Physical.Exhausted)
            {
                currentSpeed *= 0.5f;
            }

            currentSpeed *= inputDir;

            if (!IsBarMode)
                currentSpeed *= rungSpeedFactor;

            float scrollSpeedFactor = Mathf.Lerp(0.2f, 1f, player.MovementContext.RelativeSpeed);
            currentSpeed *= scrollSpeedFactor;

            float moveDelta = currentSpeed * Time.deltaTime;

            float desiredHeight = currentHeight + moveDelta;

            player.Position = GetWorldPositionAtHeight(desiredHeight);

            if (isMoving)
            {
                distanceSinceLastPainCheck += Mathf.Abs(moveDelta);
                if (distanceSinceLastPainCheck >= PainDistanceThreshold)
                {
                    ApplyBrokenArmPenalty();
                    distanceSinceLastPainCheck = 0f;
                }

                float targetTilt = -Mathf.Sin((desiredHeight / ladder.RungSpacing) * Mathf.PI + ladder.RungSpacing * 0.3f) * LeanSwayAmount;
                player.MovementContext.SetTilt(Mathf.MoveTowards(player.MovementContext.SmoothedTilt, targetTilt, Time.deltaTime * 5f), true);
            }
        }

        bool TryVaultingFakeForwardInput()
        {
            // bypass vaulting conditions of forward input
            player.InputDirection = new Vector2(0, 1f); // checked separately
            player.MovementContext.MovementDirection_1 = new Vector2(0, 1f); // checked separately (not using public setter because that does something to animator)

            if (player.MovementContext.TryVaulting())
            {
                return true;
            }
            return false;
        }

        bool TryExit(float currentHeight, float moveDir)
        {
            if (IsBarMode)
                return false;

            if (moveDir < -0.1f)
            {
                if (currentHeight + 0.5f < 0f)
                    return true;

                // ref: AUD-01-06 Gating de proximidade: só consulta a física se estiver próximo à base
                if (currentHeight < 1.0f && Physics.Raycast(player.Position + ladder.transform.forward * 0.35f, Vector3.down, 0.1f, LayerMaskController.TerrainLowPoly))
                {
                    return true;
                }
            }
            else if (moveDir > 0.1f)
            {
                if (currentHeight > ladder.MaxHeight - 1.2f)
                {
                    if (TryVaultingFakeForwardInput())
                        return true;
                }
            }

            return false;
        }

        private void ApplyBrokenArmPenalty()
        {
            bool leftArmDamaged = player.MovementContext.PhysicalConditionIs(EPhysicalCondition.LeftArmDamaged);
            bool rightArmDamaged = player.MovementContext.PhysicalConditionIs(EPhysicalCondition.RightArmDamaged);

            if (!leftArmDamaged && !rightArmDamaged)
                return;

            bool dealtDamage = false;

            if (leftArmDamaged)
            {
                player.ActiveHealthController.ApplyDamage(EBodyPart.LeftArm, BrokenArmClimbDamage, DamageHelper.FallDamage);
                player.ActiveHealthController?.DoWoundRelapse(PainDistanceThreshold, EBodyPart.LeftArm);
                dealtDamage = true;
            }

            if (rightArmDamaged)
            {
                player.ActiveHealthController.ApplyDamage(EBodyPart.RightArm, BrokenArmClimbDamage, DamageHelper.FallDamage);
                player.ActiveHealthController?.DoWoundRelapse(PainDistanceThreshold, EBodyPart.RightArm);
                dealtDamage = true;
            }

            if (dealtDamage && !player.MovementContext.PhysicalConditionIs(EPhysicalCondition.OnPainkillers))
            {
                player.Say(EPhraseTrigger.OnBeingHurt, demand: true);
            }
        }

        private static void SafeRestoreWeapon(Player player)
        {
            if (player == null || player.HealthController == null || !player.HealthController.IsAlive)
                return;

            player.StartCoroutine(RestoreWeaponWhenReady(player));
        }

        private static IEnumerator RestoreWeaponWhenReady(Player player)
        {
            float timeout = 3.0f;
            float elapsed = 0f;

            // ref: AUD-01-09 Aguarda o término de qualquer vaulting, desarmamento prévio (HandsIsEmpty) e animações pendentes
            while (player != null && elapsed < timeout)
            {
                bool isVaulting = player.MovementContext != null && player.MovementContext.PlayerAnimatorGetIsVaulting();
                bool isHandsInteracting = player.HandsController != null && player.HandsController.IsInInteraction();
                bool isWaitingEmptyHands = !player.HandsIsEmpty && player.IsInBufferZone;

                if (!isVaulting && !isHandsInteracting && !isWaitingEmptyHands)
                    break;

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Respiro adicional de 1 frame para garantir que a fila de operações do servidor Fika processou o ack do desarmamento
            yield return null;

            if (player == null || player.HealthController == null || !player.HealthController.IsAlive)
                yield break;

            player.IsInBufferZone = false;
            player.TrySetLastEquippedWeapon();
        }

        void OnDestroy()
        {
            _destroyCts.Cancel();
            _destroyCts.Dispose();

            player.OnPlayerDead -= OnPlayerDead;
            Patch_MoveInputTranslator_TranslateAxes.Unsubscribe(player, OnPlayerInput);

            player.MovementContext.IsAxesIgnored = false;

            Patch_Physical_CanClimb.OverrideCanClimb = false;
            Patch_Physical_CanVault.OverrideCanVault = false;
            Patch_ObstacleCalculatorModel_DistanceToMainObstacle.OverrideVaultObstacleDistance = false;

            proceduralBody?.Dispose();

            try { OnProceduralBodyDestroy?.Invoke(); } catch (Exception ex){ Plugin.Logger.LogError(ex); }
            
            if (!player.HealthController.IsAlive)
                return;

            if (player.CharacterController.GetCollider() is CapsuleCollider capsuleCollider)
            {
                capsuleCollider.radius = originalCapsuleColliderRadius;
            }

            SafeRestoreWeapon(player);
            player.SetCompensationScale();
        }
    }
}
