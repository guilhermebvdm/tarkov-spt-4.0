using System;
using Audio.SpatialSystem;
using UnityEngine;

public class GClass1145 : GClass1141
{
	[NonSerialized]
	public GClass1128 Gclass1128_0;

	public GClass1145(SpatialAudioSystem audioSystem, Transform listenerTransform, GClass1128 obstructionCalculator, LoggerClass logger)
		: base(audioSystem, listenerTransform, logger)
	{
		Gclass1128_0 = obstructionCalculator;
	}

	public override void OnUpdateOcclusionEffects(SourceContainerClass sourceContainer, bool updateImmediately = false)
	{
		Vector3 currentPosition = sourceContainer.CurrentPosition;
		Vector3 vector3_ = base.Vector3_0;
		if (!sourceContainer.IsOutOfHearingRangeWithOffset(5f))
		{
			float propagationEffect = (base.ObstructionEffect = Gclass1128_0.CalculateScore(vector3_, sourceContainer));
			base.PropagationEffect = propagationEffect;
			for (int i = 0; i < sourceContainer.Count; i++)
			{
				BetterSource source = sourceContainer[i];
				CalculateFinalOcclusionParams(sourceContainer.CurrentAudioRoom, source, currentPosition);
				ApplyOcclusionToSource(source, updateImmediately);
			}
			method_5(sourceContainer);
			base.OnUpdateOcclusionEffects(sourceContainer, updateImmediately);
		}
	}

	public override void Dispose()
	{
		Gclass1128_0.Dispose();
		base.Dispose();
	}
}
