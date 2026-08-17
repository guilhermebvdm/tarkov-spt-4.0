using UnityEngine;
using RootMotion.FinalIK;

namespace tarkin.ladders.bep
{
    public class ProceduralLadderArm : ProceduralLadderLimb
    {
        readonly Transform shoulder;

        public ProceduralLadderArm(LimbIK limb, bool isRight)
            : base(limb, isRight,
                   localGripOffset: new Vector3(-0.10f, -0.03f, 0f),
                   speed: 2.4f)
        {
            shoulder = limb.transform.GetChild(0);
        }

        protected override Vector3 GetRootPosition() => shoulder.position;
    }
}
