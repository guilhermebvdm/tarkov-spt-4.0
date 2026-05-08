using System;
using System.Collections.Generic;
using Audio.SpatialSystem;
using UnityEngine;

public class GClass1143 : GClass1141
{
	[NonSerialized]
	public Dictionary<int, GClass1127> Dictionary_0 = new Dictionary<int, GClass1127>(20);

	[NonSerialized]
	public GClass1128 Gclass1128_0;

	[NonSerialized]
	public new Vector3 Vector3_0 = Vector3.zero;

	[NonSerialized]
	public Vector3 Vector3_1 = Vector3.zero;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public bool Bool_3;

	[NonSerialized]
	public Action Action_0;

	public GClass1143(SpatialAudioSystem spatialSystem, Transform listenerTransform, GClass1128 propagationCalculator, LoggerClass logger)
		: base(spatialSystem, listenerTransform, logger)
	{
		Gclass1128_0 = propagationCalculator;
		Action_0 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<GClass3575>(method_6);
		base.ObstructionEffect = 0f;
	}

	public void method_6(GClass3575 stateChangedEvent)
	{
		ForceUpdate();
	}

	public override void OnForceUpdate()
	{
		Bool_2 = true;
	}

	public override void OnUpdate()
	{
		if (!SourceContainerClass.IsValid)
		{
			return;
		}
		Vector3 vector3_ = base.Vector3_0;
		Bool_1 = method_7(SourceContainerClass.CurrentPosition, vector3_);
		if (!Bool_1 || Bool_2)
		{
			Gclass1128_0.ManualUpdate(SourceContainerClass, vector3_, Bool_2);
			if (Bool_2)
			{
				UpdateOcclusionEffects(SourceContainerClass, updateImmediately: true);
				Bool_3 = true;
			}
			base.OnUpdate();
		}
	}

	public override void OnLateUpdate()
	{
		if (SourceContainerClass.IsValid && (!Bool_1 || Bool_2))
		{
			Gclass1128_0.ManualLateUpdate();
			if (Bool_3)
			{
				UpdateOcclusionEffects(SourceContainerClass, updateImmediately: true);
				Bool_2 = false;
				Bool_3 = false;
			}
			base.OnLateUpdate();
		}
	}

	public override void OnUpdateOcclusionEffects(SourceContainerClass sourceContainer, bool updateImmediately = false)
	{
		if (!method_10())
		{
			ForceUpdate();
		}
		method_9(sourceContainer);
		method_5(sourceContainer);
		Vector3 currentPosition = sourceContainer.CurrentPosition;
		if ((!Bool_1 || updateImmediately || Bool_2) && !sourceContainer.IsOutOfHearingRangeWithOffset(5f))
		{
			base.PropagationEffect = Gclass1128_0.GetScore();
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

	public bool method_7(Vector3 soundPos, Vector3 listenerPos)
	{
		if (GClass940.IsPositionIdentical(soundPos, Vector3_0, 1f) && GClass940.IsPositionIdentical(listenerPos, Vector3_1, 1f))
		{
			return true;
		}
		Vector3_0 = soundPos;
		Vector3_1 = listenerPos;
		return false;
	}

	public override void ApplyOcclusionToSource(BetterSource source, bool updateImmediately = false)
	{
		method_8(source);
		if (Dictionary_0.TryGetValue(source.GetInstanceID(), out var value))
		{
			Gclass1127_0.Apply(value);
		}
		base.ApplyOcclusionToSource(source, updateImmediately);
	}

	public void method_8(BetterSource source)
	{
		int instanceID = source.GetInstanceID();
		if (!Dictionary_0.TryGetValue(instanceID, out var value))
		{
			value = new GClass1127();
			Dictionary_0.Add(instanceID, value);
		}
		value.Apply(Gclass1127_0);
	}

	public void method_9(SourceContainerClass container)
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
				method_8(betterSource);
				continue;
			}
			Gclass1127_0.Apply(value);
			ApplyOcclusionToSource(betterSource, updateImmediately: true);
		}
	}

	public bool method_10()
	{
		return base.PropagationEffect > 0f != SpatialAudioSystem_0.IsSourceInListenerRoom(SourceContainerClass);
	}

	public override void Dispose()
	{
		Action_0?.Invoke();
		Action_0 = null;
		Dictionary_0?.Clear();
		if (Gclass1128_0 != null)
		{
			SpatialAudioSystem_0.ReturnCalculator(Gclass1128_0);
			Gclass1128_0 = null;
		}
		base.Dispose();
	}
}
