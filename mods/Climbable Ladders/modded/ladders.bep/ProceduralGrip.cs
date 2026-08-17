using System.Collections.Generic;
using UnityEngine;

#if SPT_4_0
using IWeaponGripPose = GInterface26;
#endif

namespace tarkin.ladders.bep
{
    public class FingerJoint
    {
        public Transform Bone;
        public int Index;
        public Quaternion RestRotation;

        public float CurrentAngle;
        public float TargetAngle;
        public Vector3 EndPositionOffset;
    }

    public class Finger
    {
        public FingerJoint Base;
        public FingerJoint Mid;
        public FingerJoint Tip;

        public bool IsThumb;
        public Vector3 BendAxis;
        public float MinCurl;
        public float MaxCurl;

        public IEnumerable<FingerJoint> Joints
        {
            get
            {
                if (Base != null) yield return Base;
                if (Mid != null) yield return Mid;
                if (Tip != null) yield return Tip;
            }
        }
    }

    public class ProceduralGrip : IWeaponGripPose
    {
        #region Interface implementation
        public Quaternion this[int index]
        {
            get
            {
                if (index >= 0 && index < _fingerRotations.Length)
                    return _fingerRotations[index];
                return Quaternion.identity;
            }
            set { throw new System.NotImplementedException(); }
        }

        public bool IsAlternative => false;
        public bool IsCached => true;

        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
        #endregion

        private readonly Quaternion[] _fingerRotations;
        private readonly Transform[] _allBones;

        private readonly Vector3 _bendAxis = Vector3.forward;
        private readonly float _minCurlAngle = -45f;
        private readonly float _maxCurlAngle = 70f;

        private readonly Vector3 _thumbBendAxis = new Vector3(0, 0, 1f).normalized;
        private readonly float _thumbMinCurlAngle = -45f;
        private readonly float _thumbMaxCurlAngle = 20f;

        private readonly float _curlSpeed = 4f;
        private readonly float _uncurlSpeed = 3f;

        private List<Finger> _fingers;
        private SkinnedMeshRenderer _handRenderer;

        private float currentCurl;

        public ProceduralGrip(Transform palm)
        {
            Position = palm.position;
            Rotation = palm.rotation;

            _allBones = palm.GetComponentsInChildren<Transform>();
            _fingerRotations = new Quaternion[_allBones.Length];

            _handRenderer = FindRendererForBone(palm);

            _fingers = ExtractFingers();
            CalculateBoneLengths();

            for (int i = 0; i < _allBones.Length; i++)
            {
                _fingerRotations[i] = GetRestPoseLocalRotation(_allBones[i]) ?? _allBones[i].localRotation;
            }
        }

        private static SkinnedMeshRenderer FindRendererForBone(Transform bone)
        {
            foreach (var smr in bone.root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (System.Array.IndexOf(smr.bones, bone) != -1)
                    return smr;
            }
            return null;
        }

        private List<Finger> ExtractFingers()
        {
            List<Finger> fingersList = new List<Finger>();

            for (int digit = 1; digit <= 5; digit++)
            {
                FingerJoint baseJoint = null, midJoint = null, tipJoint = null;

                for (int i = 0; i < _allBones.Length; i++)
                {
                    Transform bone = _allBones[i];

                    if (bone.name.EndsWith($"Digit{digit}1"))
                        baseJoint = CreateJoint(bone, i);

                    if (bone.name.EndsWith($"Digit{digit}2"))
                        midJoint = CreateJoint(bone, i);

                    if (bone.name.EndsWith($"Digit{digit}3"))
                        tipJoint = CreateJoint(bone, i);
                }

                if (baseJoint != null && midJoint != null && tipJoint != null)
                {
                    bool isThumb = (digit == 1);

                    fingersList.Add(new Finger
                    {
                        Base = baseJoint,
                        Mid = midJoint,
                        Tip = tipJoint,

                        IsThumb = isThumb,
                        BendAxis = isThumb ? _thumbBendAxis : _bendAxis,
                        MinCurl = isThumb ? _thumbMinCurlAngle : _minCurlAngle,
                        MaxCurl = isThumb ? _thumbMaxCurlAngle : _maxCurlAngle
                    });
                }
            }

            return fingersList;
        }

        private FingerJoint CreateJoint(Transform bone, int index)
        {
            Quaternion restPose = GetRestPoseLocalRotation(bone) ?? bone.localRotation;

            return new FingerJoint
            {
                Bone = bone,
                Index = index,
                RestRotation = restPose
            };
        }

        private Quaternion? GetRestPoseLocalRotation(Transform bone)
        {
            if (_handRenderer == null || bone == null || bone.parent == null)
                return null;

            int boneIndex = System.Array.IndexOf(_handRenderer.bones, bone);
            int parentIndex = System.Array.IndexOf(_handRenderer.bones, bone.parent);

            if (boneIndex == -1 || parentIndex == -1)
                return null;

            Matrix4x4 boneBindPose = _handRenderer.sharedMesh.bindposes[boneIndex];
            Matrix4x4 parentBindPose = _handRenderer.sharedMesh.bindposes[parentIndex];

            Matrix4x4 localMatrix = parentBindPose * boneBindPose.inverse;

            return localMatrix.rotation;
        }

        public void Update(bool shouldCurl)
        {
            float curlAmountTarget = shouldCurl ? 0.7f : 0.2f;
            float changeSpeed = shouldCurl ? _curlSpeed : _uncurlSpeed;
            float newCurlAmount = Mathf.MoveTowards(currentCurl, curlAmountTarget, changeSpeed * Time.deltaTime);

            SetCurl(newCurlAmount);
        }

        public void TestSinAnimation()
        {
            float t = Time.time;

            float normalizedTime = (Mathf.Sin(t * 3f) + 1f) * 0.5f;

            SetCurl(normalizedTime);
        }

        void SetCurl(float t)
        {
            currentCurl = t;

            for (int i = 0; i < _fingers.Count; i++)
            {
                Finger finger = _fingers[i];
                float curlAngle = Mathf.Lerp(finger.MinCurl, finger.MaxCurl, t);

                foreach (FingerJoint joint in finger.Joints)
                {
                    Quaternion curlRotation = Quaternion.AngleAxis(curlAngle, _bendAxis);
                    _fingerRotations[joint.Index] = joint.RestRotation * curlRotation;
                }
            }
        }

        private void CalculateBoneLengths()
        {
            foreach (var finger in _fingers)
            {
                finger.Base.EndPositionOffset = finger.Base.Bone.InverseTransformPoint(finger.Mid.Bone.position);
                finger.Mid.EndPositionOffset = finger.Mid.Bone.InverseTransformPoint(finger.Tip.Bone.position);

                finger.Tip.EndPositionOffset = finger.Mid.EndPositionOffset * 0.8f;
            }
        }
    }
}
