using EFT;
using HarmonyLib;
using RootMotion.FinalIK;
using System;
using System.Reflection;
using tarkin.ladders.shared;
using UnityEngine;

namespace tarkin.ladders.bep
{
    public class ProceduralLadderBody : IDisposable
    {
        private readonly static FieldInfo Field_Player__limbs = AccessTools.Field(typeof(Player), "_limbs");
        private ProceduralLadderArm proceduralArmLeft;
        private ProceduralLadderArm proceduralArmRight;
        private ProceduralGrip proceduralGripLeft;
        private ProceduralGrip proceduralGripRight;

        private ProceduralLadderLeg proceduralLegLeft;
        private ProceduralLadderLeg proceduralLegRight;

        private readonly static FieldInfo Field_Player__vaultAudioController = AccessTools.Field(typeof(Player), "_vaultAudioController");

        private readonly Player player;
        private readonly Ladder ladder;

        private readonly Vector3 enterPelvisPosition;
        private readonly Quaternion enterPelvisRotation;
        private readonly Quaternion enterCamContainerRotation;
        private float transitionCurrentProgress;
        private float transitionCurrentVelocity;
        private const float PelvisOrbitRadius = 0.95f;

        public ProceduralLadderBody(Player player, Ladder ladder)
        {
            this.ladder = ladder;
            this.player = player;

            enterPelvisPosition = player.PlayerBones.Pelvis.position;
            enterPelvisRotation = player.PlayerBones.Pelvis.rotation;
            enterCamContainerRotation = player.CameraContainer.transform.rotation;

            InitHands();
            InitLegs();
        }

        void InitHands()
        {
            LimbIK[] limbs = Field_Player__limbs.GetValue(player) as LimbIK[];

            proceduralArmLeft = new ProceduralLadderArm(limbs[0], false);
            proceduralArmRight = new ProceduralLadderArm(limbs[1], true);

            proceduralGripLeft = new ProceduralGrip(player.HandPosers[0].transform);
            proceduralGripRight = new ProceduralGrip(player.HandPosers[1].transform);

            proceduralArmLeft.OnRungReached += PlayRungReachSound;
            proceduralArmRight.OnRungReached += PlayRungReachSound;

            proceduralArmLeft.OnArcStarted += PlayArcStartedSound;
            proceduralArmRight.OnArcStarted += PlayArcStartedSound;
        }

        void InitLegs()
        {
            Transform leftThigh = player.PlayerBones.LeftThigh1.Original;
            proceduralLegLeft = new ProceduralLadderLeg(
                leftThigh,
                leftThigh.GetChild(0).GetChild(0),
                leftThigh.GetChild(0).GetChild(0).GetChild(0),
                player.PlayerBones.Pelvis.Original,
                isRight: false
                );

            Transform rightThigh = player.PlayerBones.RightThigh1.Original;
            proceduralLegRight = new ProceduralLadderLeg(
                rightThigh,
                rightThigh.GetChild(0).GetChild(0),
                rightThigh.GetChild(0).GetChild(0).GetChild(0),
                player.PlayerBones.Pelvis.Original,
                isRight: true
                );

            proceduralLegLeft.OnRungReached += PlayRungReachSound;
            proceduralLegRight.OnRungReached += PlayRungReachSound;
        }

        void PlayArcStartedSound()
        {
            (Field_Player__vaultAudioController.GetValue(player) as BaseVaultingAudioController)?.PlayGearSound();
        }

        void PlayRungReachSound(Vector3 contactPoint)
        {
            if (!ladder.IsSurfaceSoundIdentified)
                ladder.TryIdentifySurfaceSound(contactPoint);

            player.method_76(hit: true, ladder.SurfaceSound); // Player.UpdateSurfaceData()

            player.PlayStepSound();
        }

        // pull up curves
        private readonly AnimationCurve pelvisForwardCurve = new AnimationCurve(
            new Keyframe(0.0f, 0.20f),
            new Keyframe(0.4f, 0.40f),
            new Keyframe(0.8f, 0.20f),
            new Keyframe(1.0f, 0.20f)
        );

        private readonly AnimationCurve footDownCurve = new AnimationCurve(
            new Keyframe(0.0f, 0.90f),
            new Keyframe(0.8f, 0.30f),
            new Keyframe(1.0f, 0.90f)
        );

        private readonly AnimationCurve footForwardCurve = new AnimationCurve(
            new Keyframe(0.0f, 0.00f),
            new Keyframe(0.8f, 0.20f),
            new Keyframe(1.0f, 0.00f)
        );

        public void Update(float rollAngle = 0f)
        {
            transitionCurrentProgress = Mathf.SmoothDamp(transitionCurrentProgress, 1f, ref transitionCurrentVelocity, 0.1f);

            Quaternion rollRot = Quaternion.AngleAxis(rollAngle, ladder.transform.right);

            bool isBarMode = ladder.RungCount == 1;

            Vector3 barCenter = ladder.transform.position;
            float playerPosDelta = barCenter.y - player.Position.y;
            float normalizedPullUp = Mathf.InverseLerp(1.8f, 0.5f, playerPosDelta);

            if (isBarMode)
            {
                float forwardOffset = pelvisForwardCurve.Evaluate(normalizedPullUp);


                Vector3 hangDir = rollRot * (ladder.transform.forward * forwardOffset - Vector3.up * (playerPosDelta - 0.6f));
                Vector3 targetPelvisPos = barCenter + hangDir * PelvisOrbitRadius;

                player.PlayerBones.Pelvis.position = Vector3.Lerp(enterPelvisPosition, targetPelvisPos, transitionCurrentProgress);
            }

            Quaternion basePelvisRot = ProceduralLadderLimb.ConvertToTarkovRig(-ladder.transform.forward, Vector3.up);
            Quaternion targetPelvisRot = rollRot * basePelvisRot;

            player.PlayerBones.Pelvis.rotation = Quaternion.Slerp(enterPelvisRotation, targetPelvisRot, transitionCurrentProgress);

            Quaternion baseCamRot = Quaternion.LookRotation(rollRot * -ladder.transform.forward, rollRot * Vector3.up);
            player.CameraContainer.transform.rotation = Quaternion.Slerp(enterCamContainerRotation, baseCamRot, transitionCurrentProgress);

            proceduralArmLeft.Update(ladder);
            proceduralArmRight.Update(ladder);

            UpdateGrip();

#if SPT_4_0
            player.method_19(d: 0f); // Player.IkApply(float distance)
#endif
            if (isBarMode)
            {
                Vector3 characterForward = rollRot * -ladder.transform.forward;
                Vector3 characterDown = rollRot * Vector3.down;
                Vector3 characterUp = rollRot * Vector3.up;

                float legExtension = footDownCurve.Evaluate(normalizedPullUp);
                float forwardBend = footForwardCurve.Evaluate(normalizedPullUp);
                float stanceWidth = 0.12f;

                Vector3 baseFeetPos = player.PlayerBones.Pelvis.position
                                      + (characterDown * legExtension)
                                      + (characterForward * forwardBend);

                Vector3 leftFootPos = baseFeetPos + ladder.transform.right * stanceWidth;
                Vector3 rightFootPos = baseFeetPos + ladder.transform.right * -stanceWidth;

                Quaternion stableBodyRot = Quaternion.LookRotation(characterForward, characterUp);

                Quaternion footRelaxedOffset = Quaternion.Euler(0, 90f, 30f);

                Quaternion feetTargetRot = stableBodyRot * footRelaxedOffset;

                proceduralLegLeft.Update(leftFootPos, feetTargetRot);
                proceduralLegRight.Update(rightFootPos, feetTargetRot);
            }
            else
            {
                proceduralLegLeft.Update(ladder);
                proceduralLegRight.Update(ladder);
            }
        }

        void UpdateGrip()
        {
            var leftPoser = player.HandPosers[0];
            var rightPoser = player.HandPosers[1];

            leftPoser.weight = 1f;
            rightPoser.weight = 1f;

            leftPoser.GripWeight = 1f;
            rightPoser.GripWeight = 1f;

            rightPoser.IgnoreIndexFinger = false;

            proceduralGripLeft.Update(shouldCurl: !proceduralArmLeft.InArc);
            proceduralGripRight.Update(shouldCurl: !proceduralArmRight.InArc);

            leftPoser.SetGrip(proceduralGripLeft);
            rightPoser.SetGrip(proceduralGripRight);
        }

        public void Dispose()
        {
            proceduralLegLeft.Dispose();
            proceduralLegRight.Dispose();
        }
    }
}
