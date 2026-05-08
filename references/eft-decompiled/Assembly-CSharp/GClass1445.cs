using System.Collections.Generic;
using UnityEngine;

public abstract class GClass1445
{
	public static IAnimator CreateAnimator(Animator animator)
	{
		return GClass1446.Create(animator);
	}

	public static IAnimator CreateAnimator(FastAnimatorControllerClass fastAnimatorController, Dictionary<int, GClass1448> computedNodes = null, Transform rootBone = null, GInterface123 playableAnimator = null)
	{
		return new FastAnimatorProcessorClass(fastAnimatorController, computedNodes, rootBone, playableAnimator);
	}
}
