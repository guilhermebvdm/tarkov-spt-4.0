using System;
using UnityEngine;

[Serializable]
public class FastLActionStateBehaviour : GClass1328
{
	public float StartTreshold;

	public AnimationCurve LayerWeight;

	[NonSerialized]
	public LActionState.GClass915 Context;

	public LActionState.GClass915 GClass915_0
	{
		get
		{
			if (Context == null)
			{
				Context = new LActionState.GClass915(LayerWeight, StartTreshold);
			}
			return Context;
		}
	}

	public override GClass1328 Clone()
	{
		FastLActionStateBehaviour fastLActionStateBehaviour = new FastLActionStateBehaviour();
		fastLActionStateBehaviour.StartTreshold = StartTreshold;
		fastLActionStateBehaviour.LayerWeight = new AnimationCurve(LayerWeight.keys);
		fastLActionStateBehaviour.LayerWeight.postWrapMode = LayerWeight.postWrapMode;
		fastLActionStateBehaviour.LayerWeight.preWrapMode = LayerWeight.preWrapMode;
		return fastLActionStateBehaviour;
	}

	public override void OnStateEnter(FastAnimatorProcessorClass fastAnimatorProcessor, in AnimatorStateInfoWrapper layerInfo, int layerIndex)
	{
		GClass915_0.OnStateEnter(fastAnimatorProcessor, layerInfo, layerIndex);
	}

	public override void OnStateUpdate(FastAnimatorProcessorClass fastAnimatorProcessor, in AnimatorStateInfoWrapper layerInfo, int layerIndex)
	{
		GClass915_0.OnStateUpdate(fastAnimatorProcessor, layerInfo, layerIndex);
	}

	public override void OnStateExit(FastAnimatorProcessorClass fastAnimatorProcessor, in AnimatorStateInfoWrapper layerInfo, int layerIndex)
	{
		GClass915_0.OnStateExit(fastAnimatorProcessor, layerInfo, layerIndex);
	}
}
