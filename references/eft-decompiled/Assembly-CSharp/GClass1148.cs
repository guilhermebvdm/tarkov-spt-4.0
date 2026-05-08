using Audio.SpatialSystem;
using Audio.SpatialSystem.Data;
using UnityEngine;

public abstract class GClass1148
{
	public static GClass1141 Create(Transform listenerTransform, EOcclusionTest test, LoggerClass logger)
	{
		SpatialAudioSystem instance = MonoBehaviourSingleton<SpatialAudioSystem>.Instance;
		_ = instance.PropagationDepth;
		switch (test)
		{
		default:
			return new GClass1145(instance, listenerTransform, new GClass1132(instance), logger);
		case EOcclusionTest.Regular:
			return new GClass1143(instance, listenerTransform, instance.WithdrawCalculator<GClass1129>(), logger);
		case EOcclusionTest.Continuous:
			return new GClass1144(instance, listenerTransform, new GClass1131(instance), instance.WithdrawCalculator<GClass1129>(), logger);
		case EOcclusionTest.ContinuousPropagated:
			return new GClass1143(instance, listenerTransform, instance.WithdrawCalculator<GClass1129>(), logger);
		case EOcclusionTest.Combined:
		{
			AudioOcclusionSettings occlusionSettings = instance.OcclusionSettings;
			return new GClass1142(instance, listenerTransform, occlusionSettings, instance.WithdrawCalculator<GClass1134>(), instance.WithdrawCalculator<GClass1129>(), logger);
		}
		case EOcclusionTest.OneShotPropagation:
		case EOcclusionTest.OneShotFullOcclusion:
			return new GClass1146(instance, listenerTransform, logger);
		}
	}
}
