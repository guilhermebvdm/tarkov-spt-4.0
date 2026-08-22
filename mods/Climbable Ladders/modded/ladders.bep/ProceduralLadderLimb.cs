using System;
using UnityEngine;
using RootMotion.FinalIK;
using tarkin.ladders.shared;

namespace tarkin.ladders.bep
{
    public abstract class ProceduralLadderLimb
    {
        public readonly LimbIK limb;
        protected readonly bool isRight;
        protected readonly float speed;

        // (fingers forward, knuckles up, 0)
        readonly Vector3 localGripOffset;

        Vector3 currentGripPoint;
        Quaternion currentIKRot;

        int currentRungIndex = -1;
        public bool InArc { get; private set; }
        float arcT;
        float arcDuration;

        const float ArcSpeed = 3.0f;
        const float ArcOutwardAmount = 0.15f;
        Vector3 arcGripFrom;
        Vector3 arcOutwardDir;

        public event Action<Vector3> OnRungReached;
        public event Action OnArcStarted;

        protected ProceduralLadderLimb(LimbIK limb, bool isRight, Vector3 localGripOffset, float speed)
        {
            this.limb = limb;
            this.isRight = isRight;
            this.localGripOffset = localGripOffset;
            this.speed = speed;

            currentIKRot = limb.solver.IKRotation;
            currentGripPoint = limb.solver.IKPosition + currentIKRot * localGripOffset;
        }

        protected abstract Vector3 GetRootPosition();
        protected virtual float HeightOffset => 0f;

        protected virtual bool ShouldSkipUpdate(float relativeHeight) => false;

        protected virtual void ApplyWeights()
        {
            limb.solver.IKPositionWeight = 1f;
            limb.solver.IKRotationWeight = 1f;
        }

        protected void ResetToCurrent()
        {
            currentIKRot = limb.solver.IKRotation;
            currentGripPoint = limb.solver.IKPosition + currentIKRot * localGripOffset;
        }

        public void Update(Ladder ladder)
        {
            Vector3 lowestRungWorldPos = ladder.transform.position;
            float rungSpacing = ladder.RungSpacing;
            int rungCount = ladder.RungCount;
            float ladderWidth = ladder.Width;

            Vector3 ladderUp = ladder.transform.up;
            Vector3 ladderRight = ladder.transform.right;
            Vector3 ladderForward = ladder.transform.forward;

            float relativeHeight = Vector3.Dot(
                GetRootPosition() - lowestRungWorldPos + ladderUp * HeightOffset,
                ladderUp);
            float continuousIndex = relativeHeight / rungSpacing;

            if (ShouldSkipUpdate(relativeHeight))
                return;

            int parity = isRight ? 1 : 0;
            int rungIndex = Mathf.RoundToInt((continuousIndex - parity) / 2f) * 2 + parity;
            rungIndex = Mathf.Clamp(rungIndex, 0, rungCount - 1);

            Vector3 rungCenter = lowestRungWorldPos + ladderUp * (rungIndex * rungSpacing);
            float sideSign = isRight ? 1f : -1f;

            float maxXOffsetFromCenter = 0.2f;
            float handWidth = 0.06f;
            float clampedXOffsetFromCenter = Mathf.Min(maxXOffsetFromCenter, ladderWidth / 2f - handWidth);

            Vector3 desiredGripPoint = rungCenter - ladderRight * (sideSign * clampedXOffsetFromCenter);

            if (rungIndex != currentRungIndex)
            {
                if (currentRungIndex >= 0)
                {
                    InArc = true;
                    arcT = 0f;
                    arcGripFrom = currentGripPoint;
                    arcOutwardDir = ladderForward;

                    float distanceToTravel = Vector3.Distance(arcGripFrom, desiredGripPoint);
                    arcDuration = Mathf.Max(0.01f, distanceToTravel / (ArcSpeed * speed / 3f));

                    OnArcStarted?.Invoke();
                }
                currentRungIndex = rungIndex;
            }

            if (InArc)
            {
                arcT += Time.deltaTime / arcDuration;
                if (arcT >= 1f)
                {
                    arcT = 1f;
                    InArc = false;
                }

                float t = arcT * arcT * (3f - 2f * arcT);
                float outward = 4f * arcT * (1f - arcT) * ArcOutwardAmount;

                currentGripPoint = Vector3.Lerp(arcGripFrom, desiredGripPoint, t)
                                 + arcOutwardDir * outward;

                if (!InArc)
                    OnRungReached?.Invoke(currentGripPoint);
            }
            else
            {
                currentGripPoint = Vector3.MoveTowards(
                    currentGripPoint, desiredGripPoint, Time.deltaTime * speed);
            }

            Vector3 jointPos = limb.solver.bone2.transform.position;
            Vector3 limbDir = (currentGripPoint - jointPos).normalized;

            if (limbDir.sqrMagnitude < 0.01f)
                limbDir = (currentGripPoint - GetRootPosition()).normalized;

            Vector3 fingersDir = Vector3.ProjectOnPlane(limbDir, ladderRight).normalized;

            if (fingersDir.sqrMagnitude < 0.001f)
                fingersDir = -ladderForward;

            Vector3 knuckleDir = Vector3.Cross(fingersDir, -ladderRight).normalized;

            Quaternion targetIKRot = ConvertToTarkovRig(knuckleDir, fingersDir);

            currentIKRot = Quaternion.Slerp(
                currentIKRot, targetIKRot, Time.deltaTime * speed * 5f);

            Vector3 ikPos = currentGripPoint - currentIKRot * localGripOffset;

            ApplyWeights();

            limb.solver.SetIKPosition(ikPos);
            limb.solver.SetIKRotation(currentIKRot);
        }

        // convert from intuitive orientation to tarkov-specific orientation
        public static Quaternion ConvertToTarkovRig(Vector3 knuckleDir, Vector3 fingersDir)
        {
            fingersDir = Vector3.ProjectOnPlane(fingersDir, knuckleDir).normalized;
            if (fingersDir.sqrMagnitude < 0.001f)
                fingersDir = Vector3.up;

            Vector3 rigFwd = Vector3.Cross(knuckleDir, fingersDir);
            return Quaternion.LookRotation(rigFwd, knuckleDir);
        }
    }
}
