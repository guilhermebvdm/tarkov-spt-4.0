using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

[CreateAssetMenu(fileName = "PuppetMaster Humanoid Config", menuName = "PuppetMaster/Humanoid Config", order = 1)]
public class PuppetMasterHumanoidConfig : ScriptableObject
{
	[Serializable]
	public class HumanoidMuscle
	{
		[HideInInspector]
		public string name;

		public HumanBodyBones bone;

		public Muscle.Props props;
	}

	[LargeHeader("Simulation")]
	public PuppetMaster.State state;

	public PuppetMaster.StateSettings stateSettings = PuppetMaster.StateSettings.Default;

	public PuppetMaster.Mode mode;

	public float blendTime = 0.1f;

	public bool fixTargetTransforms = true;

	public int solverIterationCount = 6;

	public bool visualizeTargetPose = true;

	[LargeHeader("Master Weights")]
	[Range(0f, 1f)]
	public float mappingWeight = 1f;

	[Range(0f, 1f)]
	public float pinWeight = 1f;

	[Range(0f, 1f)]
	public float muscleWeight = 1f;

	[LargeHeader("Joint and Muscle Settings")]
	public float muscleSpring = 100f;

	public float muscleDamper = 0f;

	[Range(1f, 8f)]
	public float pinPow = 4f;

	[Range(0f, 100f)]
	public float pinDistanceFalloff = 5f;

	public bool angularPinning;

	public bool updateJointAnchors = true;

	public bool supportTranslationAnimation;

	public bool angularLimits;

	public bool internalCollisions;

	[LargeHeader("Individual Muscle Settings")]
	public HumanoidMuscle[] muscles = new HumanoidMuscle[0];

	public void ApplyTo(PuppetMaster p)
	{
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)p.targetRoot == (Object)null || (Object)(object)p.targetAnimator == (Object)null || !p.targetAnimator.isHuman)
		{
			return;
		}
		p.state = state;
		p.stateSettings = stateSettings;
		p.mode = mode;
		p.blendTime = blendTime;
		p.fixTargetTransforms = fixTargetTransforms;
		p.solverIterationCount = solverIterationCount;
		p.visualizeTargetPose = visualizeTargetPose;
		p.mappingWeight = mappingWeight;
		p.pinWeight = pinWeight;
		p.muscleWeight = muscleWeight;
		p.muscleSpring = muscleSpring;
		p.muscleDamper = muscleDamper;
		p.pinPow = pinPow;
		p.pinDistanceFalloff = pinDistanceFalloff;
		p.angularPinning = angularPinning;
		p.updateJointAnchors = updateJointAnchors;
		p.supportTranslationAnimation = supportTranslationAnimation;
		p.angularLimits = angularLimits;
		p.internalCollisions = internalCollisions;
		for (int i = 0; i < muscles.Length; i++)
		{
			Muscle muscle = GetMuscle(muscles[i].bone, p.targetAnimator, p);
			if (muscle == null && i < p.muscles.Length)
			{
				muscle = p.muscles[i];
			}
			if (muscle != null)
			{
				HumanoidMuscle humanoidMuscle = muscles[i];
				muscle.props.group = humanoidMuscle.props.group;
				muscle.props.mappingWeight = humanoidMuscle.props.mappingWeight;
				muscle.props.muscleDamper = humanoidMuscle.props.muscleDamper;
				muscle.props.muscleWeight = humanoidMuscle.props.muscleWeight;
				muscle.props.pinWeight = humanoidMuscle.props.pinWeight;
			}
		}
	}

	private Muscle GetMuscle(HumanBodyBones boneId, Animator animator, PuppetMaster puppetMaster)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Invalid comparison between Unknown and I4
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if ((int)boneId == 55)
		{
			return null;
		}
		Transform boneTransform = animator.GetBoneTransform(boneId);
		if ((Object)(object)boneTransform == (Object)null)
		{
			return null;
		}
		Muscle[] array = puppetMaster.muscles;
		foreach (Muscle muscle in array)
		{
			if ((Object)(object)muscle.target == (Object)(object)boneTransform)
			{
				return muscle;
			}
		}
		return null;
	}
}
