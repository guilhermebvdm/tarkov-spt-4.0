using UnityEngine;
using UnityEngine.Playables;

namespace VisceralCombat.Ragdolls.Classes.RootMotion;

public class HumanoidBaker : Baker
{
	[Tooltip("Should the hand IK curves be added to the animation? Disable this if the original hand positions are not important when using the clip on another character via Humanoid retargeting.")]
	public bool bakeHandIK = true;

	[Tooltip("Max keyframe reduction error for the Root.Q/T, LeftFoot IK and RightFoot IK channels. Having a larger error value for 'Key Reduction Error' and a smaller one for this enables you to optimize clip data size without the floating feet effect by enabling 'Foot IK' in the Animator.")]
	[Range(0f, 0.1f)]
	public float IKKeyReductionError;

	[Tooltip("Frame rate divider for the muscle curves. If you have 'Frame Rate' set to 30, and this value set to 3, the muscle curves will be baked at 10 fps. Only the Root Q/T and Hand and Foot IK curves will be baked at 30. This enables you to optimize clip data size without the floating feet effect by enabling 'Foot IK' in the Animator.")]
	[Range(1f, 9f)]
	public int muscleFrameRateDiv = 1;

	private BakerMuscle[] bakerMuscles;

	private BakerHumanoidQT rootQT;

	private BakerHumanoidQT leftFootQT;

	private BakerHumanoidQT rightFootQT;

	private BakerHumanoidQT leftHandQT;

	private BakerHumanoidQT rightHandQT;

	private float[] muscles = new float[0];

	private HumanPose pose = default(HumanPose);

	private HumanPoseHandler handler;

	private Vector3 bodyPosition;

	private Quaternion bodyRotation = Quaternion.identity;

	private int mN = 0;

	private Quaternion lastBodyRotation = Quaternion.identity;

	private void Awake()
	{
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Expected O, but got Unknown
		animator = ((Component)this).GetComponent<Animator>();
		director = ((Component)this).GetComponent<PlayableDirector>();
		if (mode == Mode.AnimationStates || mode == Mode.AnimationClips)
		{
			if ((Object)(object)animator == (Object)null || !animator.isHuman)
			{
				((Behaviour)this).enabled = false;
				return;
			}
			animator.cullingMode = (AnimatorCullingMode)0;
		}
		else if (mode == Mode.PlayableDirector && !((Object)(object)director == (Object)null))
		{
		}
		muscles = new float[HumanTrait.MuscleCount];
		bakerMuscles = new BakerMuscle[HumanTrait.MuscleCount];
		for (int i = 0; i < bakerMuscles.Length; i++)
		{
			bakerMuscles[i] = new BakerMuscle(i);
		}
		rootQT = new BakerHumanoidQT("Root");
		leftFootQT = new BakerHumanoidQT(animator.GetBoneTransform((HumanBodyBones)5), (AvatarIKGoal)0, "LeftFoot");
		rightFootQT = new BakerHumanoidQT(animator.GetBoneTransform((HumanBodyBones)6), (AvatarIKGoal)1, "RightFoot");
		leftHandQT = new BakerHumanoidQT(animator.GetBoneTransform((HumanBodyBones)17), (AvatarIKGoal)2, "LeftHand");
		rightHandQT = new BakerHumanoidQT(animator.GetBoneTransform((HumanBodyBones)18), (AvatarIKGoal)3, "RightHand");
		handler = new HumanPoseHandler(animator.avatar, ((Component)animator).transform);
	}

	protected override Transform GetCharacterRoot()
	{
		return ((Component)animator).transform;
	}

	protected override void OnStartBaking()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		rootQT.Reset();
		leftFootQT.Reset();
		rightFootQT.Reset();
		leftHandQT.Reset();
		rightHandQT.Reset();
		for (int i = 0; i < bakerMuscles.Length; i++)
		{
			bakerMuscles[i].Reset();
		}
		mN = muscleFrameRateDiv;
		lastBodyRotation = Quaternion.identity;
	}

	protected override void OnSetLoopFrame(float time)
	{
		for (int i = 0; i < bakerMuscles.Length; i++)
		{
			bakerMuscles[i].SetLoopFrame(time);
		}
		rootQT.MoveLastKeyframes(time);
		leftFootQT.SetLoopFrame(time);
		rightFootQT.SetLoopFrame(time);
		leftHandQT.SetLoopFrame(time);
		rightHandQT.SetLoopFrame(time);
	}

	protected override void OnSetCurves(ref AnimationClip clip)
	{
		float time = ((Keyframe)(ref bakerMuscles[0].curve.keys[bakerMuscles[0].curve.keys.Length - 1])).time;
		float lengthMlp = ((mode != Mode.Realtime) ? (base.clipLength / time) : 1f);
		for (int i = 0; i < bakerMuscles.Length; i++)
		{
			bakerMuscles[i].SetCurves(ref clip, keyReductionError, lengthMlp);
		}
		rootQT.SetCurves(ref clip, IKKeyReductionError, lengthMlp);
		leftFootQT.SetCurves(ref clip, IKKeyReductionError, lengthMlp);
		rightFootQT.SetCurves(ref clip, IKKeyReductionError, lengthMlp);
		if (bakeHandIK)
		{
			leftHandQT.SetCurves(ref clip, IKKeyReductionError, lengthMlp);
			rightHandQT.SetCurves(ref clip, IKKeyReductionError, lengthMlp);
		}
	}

	protected override void OnSetKeyframes(float time, bool lastFrame)
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		mN++;
		bool flag = true;
		if (mN < muscleFrameRateDiv && !lastFrame)
		{
			flag = false;
		}
		if (mN >= muscleFrameRateDiv)
		{
			mN = 0;
		}
		UpdateHumanPose();
		if (flag)
		{
			for (int i = 0; i < bakerMuscles.Length; i++)
			{
				bakerMuscles[i].SetKeyframe(time, muscles);
			}
		}
		rootQT.SetKeyframes(time, bodyPosition, bodyRotation);
		Vector3 val = bodyPosition * animator.humanScale;
		leftFootQT.SetIKKeyframes(time, animator.avatar, ((Component)animator).transform, animator.humanScale, val, bodyRotation);
		rightFootQT.SetIKKeyframes(time, animator.avatar, ((Component)animator).transform, animator.humanScale, val, bodyRotation);
		leftHandQT.SetIKKeyframes(time, animator.avatar, ((Component)animator).transform, animator.humanScale, val, bodyRotation);
		rightHandQT.SetIKKeyframes(time, animator.avatar, ((Component)animator).transform, animator.humanScale, val, bodyRotation);
	}

	private void UpdateHumanPose()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		handler.GetHumanPose(ref pose);
		bodyPosition = pose.bodyPosition;
		bodyRotation = pose.bodyRotation;
		bodyRotation = BakerUtilities.EnsureQuaternionContinuity(lastBodyRotation, bodyRotation);
		lastBodyRotation = bodyRotation;
		for (int i = 0; i < pose.muscles.Length; i++)
		{
			muscles[i] = pose.muscles[i];
		}
	}
}
