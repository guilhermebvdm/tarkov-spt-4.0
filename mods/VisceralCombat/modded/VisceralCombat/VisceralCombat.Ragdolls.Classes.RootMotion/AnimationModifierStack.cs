using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion;

public class AnimationModifierStack : MonoBehaviour
{
	public AnimationModifier[] modifiers = new AnimationModifier[0];

	private Animator animator;

	private Baker baker;

	private void Start()
	{
		animator = ((Component)this).GetComponent<Animator>();
		baker = ((Component)this).GetComponent<Baker>();
		Baker obj = baker;
		obj.OnStartClip = (Baker.BakerDelegate)Delegate.Combine(obj.OnStartClip, new Baker.BakerDelegate(OnBakerStartClip));
		Baker obj2 = baker;
		obj2.OnUpdateClip = (Baker.BakerDelegate)Delegate.Combine(obj2.OnUpdateClip, new Baker.BakerDelegate(OnBakerUpdateClip));
		AnimationModifier[] array = modifiers;
		foreach (AnimationModifier animationModifier in array)
		{
			animationModifier.OnInitiate(baker, animator);
		}
	}

	private void OnBakerStartClip(AnimationClip clip, float normalizedTime)
	{
		AnimationModifier[] array = modifiers;
		foreach (AnimationModifier animationModifier in array)
		{
			animationModifier.OnStartClip(clip);
		}
	}

	private void OnBakerUpdateClip(AnimationClip clip, float normalizedTime)
	{
		AnimationModifier[] array = modifiers;
		foreach (AnimationModifier animationModifier in array)
		{
			if (((Behaviour)animationModifier).enabled)
			{
				animationModifier.OnBakerUpdate(normalizedTime);
			}
		}
	}

	private void LateUpdate()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if ((!((Behaviour)animator).enabled && !baker.isBaking) || (baker.isBaking && baker.mode == Baker.Mode.AnimationClips) || (Object)(object)animator.runtimeAnimatorController == (Object)null)
		{
			return;
		}
		AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
		float normalizedTime = currentAnimatorStateInfo.normalizedTime;
		AnimationModifier[] array = modifiers;
		foreach (AnimationModifier animationModifier in array)
		{
			if (((Behaviour)animationModifier).enabled)
			{
				animationModifier.OnBakerUpdate(normalizedTime);
			}
		}
	}
}
