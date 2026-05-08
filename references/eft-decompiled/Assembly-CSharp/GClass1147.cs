using System;
using System.Collections.Generic;
using Audio.SpatialSystem;
using UnityEngine;

public class GClass1147 : GClass1141
{
	[NonSerialized]
	public Dictionary<int, GClass1127> Dictionary_0 = new Dictionary<int, GClass1127>(20);

	[NonSerialized]
	public GClass1128 Gclass1128_0;

	[NonSerialized]
	public GClass1128 Gclass1128_1;

	[NonSerialized]
	public new Vector3 Vector3_0 = Vector3.zero;

	[NonSerialized]
	public Vector3 Vector3_1 = Vector3.zero;

	public GClass1147(SpatialAudioSystem audioSystem, Transform listenerTransform, GClass1128 obstructionCalculator, GClass1128 propagationCalculator, LoggerClass logger)
		: base(audioSystem, listenerTransform, logger)
	{
		Gclass1128_0 = obstructionCalculator;
		Gclass1128_1 = propagationCalculator;
	}

	public override void OnUpdateOcclusionEffects(SourceContainerClass sourceContainer, bool updateImmediately = false)
	{
		method_8(sourceContainer);
		method_5(sourceContainer);
		if (!sourceContainer.IsPlaying && !updateImmediately)
		{
			return;
		}
		Vector3 currentPosition = sourceContainer.CurrentPosition;
		Vector3 vector3_ = base.Vector3_0;
		if (method_6(currentPosition, vector3_) && !updateImmediately)
		{
			return;
		}
		Vector3_0 = currentPosition;
		Vector3_1 = vector3_;
		if (!sourceContainer.IsOutOfHearingRangeWithOffset(5f))
		{
			base.PropagationEffect = Gclass1128_1.CalculateScore(vector3_, sourceContainer);
			if (base.PropagationEffect <= 0f)
			{
				base.ObstructionEffect = Gclass1128_0.CalculateScore(vector3_, sourceContainer);
			}
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

	public bool method_6(Vector3 soundPos, Vector3 listenerPos)
	{
		if (GClass940.IsPositionIdentical(soundPos, Vector3_0, 1f))
		{
			return GClass940.IsPositionIdentical(listenerPos, Vector3_1, 1f);
		}
		return false;
	}

	public override void ApplyOcclusionToSource(BetterSource source, bool updateImmediately = false)
	{
		method_7(source);
		if (Dictionary_0.TryGetValue(source.GetInstanceID(), out var value))
		{
			Gclass1127_0.Apply(value);
		}
		base.ApplyOcclusionToSource(source, updateImmediately);
	}

	public void method_7(BetterSource source)
	{
		int instanceID = source.GetInstanceID();
		if (!Dictionary_0.TryGetValue(instanceID, out var value))
		{
			value = new GClass1127();
			Dictionary_0.Add(instanceID, value);
		}
		value.Apply(Gclass1127_0);
	}

	public void method_8(SourceContainerClass container)
	{
		if (!container.IsValid)
		{
			return;
		}
		for (int i = 0; i < container.Count; i++)
		{
			BetterSource betterSource = container[i];
			if (!Dictionary_0.TryGetValue(betterSource.GetInstanceID(), out var value))
			{
				method_7(betterSource);
				continue;
			}
			Gclass1127_0.Apply(value);
			ApplyOcclusionToSource(betterSource, updateImmediately: true);
		}
	}

	public override void Dispose()
	{
		Dictionary_0.Clear();
		Gclass1128_0.Dispose();
		Gclass1128_1.Dispose();
		base.Dispose();
	}
}
