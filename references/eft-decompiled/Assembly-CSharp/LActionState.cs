using System;
using AnimationEventSystem;
using UnityEngine;

[GAttribute18]
public class LActionState : StateMachineBehaviour, IStateBehaviour
{
	public class GClass915
	{
		[NonSerialized]
		public bool Bool_0;

		[NonSerialized]
		public bool Bool_1;

		[NonSerialized]
		public int Int_0 = -1;

		[NonSerialized]
		public bool Bool_2;

		[NonSerialized]
		public AnimationCurve AnimationCurve_0;

		[NonSerialized]
		public float Float_0 = 0.9f;

		public GClass915(LActionSetup setup)
		{
			AnimationCurve_0 = setup.LayerWeight;
			Float_0 = setup.StartThreshold;
		}

		public GClass915(AnimationCurve layerWeight, float startTreshold)
		{
			AnimationCurve_0 = layerWeight;
			Float_0 = startTreshold;
		}

		public void OnStateEnter(IAnimator animator, AnimatorStateInfoWrapper stateInfo, int layerIndex)
		{
			Bool_0 = false;
			Bool_2 = false;
			if (Int_0 < 0)
			{
				Int_0 = animator.GetLayerIndex("LActions");
			}
			if (Int_0 < 0)
			{
				Debug.LogWarning("LActions not found");
			}
			Bool_1 = AnimationCurve_0[0].value < AnimationCurve_0.keys[AnimationCurve_0.length - 1].value;
		}

		public void OnStateUpdate(IAnimator animator, AnimatorStateInfoWrapper stateInfo, int layerIndex)
		{
			if (Bool_0)
			{
				return;
			}
			int shortNameHash = animator.GetNextAnimatorStateInfo(0).shortNameHash;
			float num = 0f;
			if ((shortNameHash != 0 && shortNameHash != stateInfo.shortNameHash) || stateInfo.normalizedTime >= 1f)
			{
				Bool_0 = true;
				num = AnimationCurve_0[AnimationCurve_0.length - 1].value;
			}
			else
			{
				num = AnimationCurve_0.Evaluate(stateInfo.normalizedTime);
			}
			if (!Bool_2)
			{
				if (Bool_1 && num >= Float_0)
				{
					Bool_2 = true;
					WeaponAnimationSpeedControllerClass.TriggerHandReady(animator);
				}
				else if (!Bool_1 && num <= Float_0)
				{
					Bool_2 = true;
					WeaponAnimationSpeedControllerClass.SetUseLeftHand(animator, useLeftHand: false);
				}
			}
		}

		public void OnStateExit(IAnimator animator, AnimatorStateInfoWrapper stateInfo, int layerIndex)
		{
			if (Bool_1)
			{
				if (!Bool_2)
				{
					WeaponAnimationSpeedControllerClass.TriggerHandReady(animator);
				}
			}
			else
			{
				animator.SetLayerWeight(Int_0, 0f);
				WeaponAnimationSpeedControllerClass.SetUseLeftHand(animator, useLeftHand: false);
			}
		}
	}

	private GClass1446 gclass1446_0;

	public AnimatorControllerStaticData StaticData;

	public int LActionSetupId = -1;

	private GClass915 gclass915_0;

	public LActionSetup Setup => StaticData.GetLActionSetupById(LActionSetupId);

	public GClass915 GClass915_0
	{
		get
		{
			if (gclass915_0 == null)
			{
				gclass915_0 = new GClass915(Setup);
			}
			return gclass915_0;
		}
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		method_0(animator);
		GClass915_0.OnStateEnter(gclass1446_0, GClass1446.CreateAnimatorStateInfoWrapper(stateInfo), layerIndex);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		method_0(animator);
		GClass915_0.OnStateUpdate(gclass1446_0, GClass1446.CreateAnimatorStateInfoWrapper(stateInfo), layerIndex);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		method_0(animator);
		GClass915_0.OnStateExit(gclass1446_0, GClass1446.CreateAnimatorStateInfoWrapper(stateInfo), layerIndex);
	}

	public void method_0(Animator animator)
	{
		if (gclass1446_0 == null)
		{
			gclass1446_0 = GClass1446.Create();
		}
		gclass1446_0.SetAnimator(animator);
	}
}
