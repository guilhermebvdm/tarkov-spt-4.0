using UnityEngine;
using RootMotion.FinalIK;
using System;

namespace tarkin.ladders.bep
{
    public class ProceduralLadderLeg : ProceduralLadderLimb, IDisposable
    {
        readonly Transform hips;

        public ProceduralLadderLeg(Transform thigh, Transform calf, Transform foot, Transform hips, bool isRight)
            : base(CreateLimbIK(thigh, calf, foot, hips), isRight,
                   localGripOffset: new Vector3(-0.2f, -0.02f, 0f),
                   speed: 3f)
        {
            this.hips = hips;
        }

        static LimbIK CreateLimbIK(Transform thigh, Transform calf, Transform foot, Transform root)
        {
            var limb = thigh.gameObject.AddComponent<LimbIK>();
            limb.solver.SetChain(thigh, calf, foot, root);
            return limb;
        }

        protected override Vector3 GetRootPosition() => hips.position;
        protected override float HeightOffset => -0.55f;

        protected override bool ShouldSkipUpdate(float relativeHeight)
        {
            if (relativeHeight >= (isRight ? -0.55f : -0.45f))
                return false;

            ResetToCurrent();
            limb.solver.IKPositionWeight = 0f;
            limb.solver.IKRotationWeight = 0f;
            return true;
        }

        public void Update(Vector3 pos, Quaternion rot)
        {
            limb.solver.SetIKPosition(pos);
            limb.solver.SetIKRotation(rot);
        }

        protected override void ApplyWeights()
        {
            float delta = Time.deltaTime * speed;
            limb.solver.IKPositionWeight = Mathf.MoveTowards(limb.solver.IKPositionWeight, 1f, delta);
            limb.solver.IKRotationWeight = Mathf.MoveTowards(limb.solver.IKRotationWeight, 1f, delta);
        }

        public void Dispose()
        {
            if (limb != null)
                Component.Destroy(limb);
        }
    }
}
