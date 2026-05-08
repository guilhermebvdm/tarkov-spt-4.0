using System;
using System.Collections.Generic;
using Audio.SpatialSystem;
using Audio.SpatialSystem.Data;
using UnityEngine;

public class GClass1142 : GClass1141
{
	[NonSerialized]
	public AudioOcclusionSettings AudioOcclusionSettings_0;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public float Float_6 = -1f;

	[NonSerialized]
	public float Float_7 = -1f;

	[NonSerialized]
	public Dictionary<int, GClass1127> Dictionary_0 = new Dictionary<int, GClass1127>(20);

	[NonSerialized]
	public GClass1128 Gclass1128_0;

	[NonSerialized]
	public GClass1128 Gclass1128_1;

	[NonSerialized]
	public SpatialAudioSystem SpatialAudioSystem_1;

	[NonSerialized]
	public bool Bool_3;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public Action Action_0;

	public GClass1142(SpatialAudioSystem spatialSystem, Transform listenerTransform, AudioOcclusionSettings occlusionSettings, GClass1128 obstructionCalculator, GClass1128 propagationCalculator, LoggerClass logger)
		: base(spatialSystem, listenerTransform, logger)
	{
		AudioOcclusionSettings_0 = occlusionSettings;
		SpatialAudioSystem_1 = spatialSystem;
		Gclass1128_0 = obstructionCalculator;
		Gclass1128_1 = propagationCalculator;
		Action_0 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<GClass3575>(method_6);
		Float_4 = 0f;
		Float_5 = 0f;
	}

	public override void SetAudioSourceContainer(SourceContainerClass sourceContainer)
	{
		Bool_1 = base.Boolean_0;
		base.SetAudioSourceContainer(sourceContainer);
	}

	public void method_6(GClass3575 stateChangedEvent)
	{
		ForceUpdate();
	}

	public override void OnForceUpdate()
	{
		Bool_3 = true;
	}

	public override void OnUpdate()
	{
		if (SourceContainerClass.IsValid)
		{
			Vector3 vector3_ = base.Vector3_0;
			Gclass1128_1.ManualUpdate(SourceContainerClass, vector3_, Bool_3);
			if (!base.Boolean_0)
			{
				Gclass1128_0.ManualUpdate(SourceContainerClass, vector3_, Bool_3);
			}
			if (Bool_3)
			{
				UpdateOcclusionEffects(SourceContainerClass, updateImmediately: true);
				Bool_4 = true;
			}
			base.OnUpdate();
		}
	}

	public override void OnLateUpdate()
	{
		if (SourceContainerClass.IsValid)
		{
			Gclass1128_1.ManualLateUpdate();
			if (!base.Boolean_0)
			{
				Gclass1128_0.ManualLateUpdate();
			}
			if (Bool_4)
			{
				UpdateOcclusionEffects(SourceContainerClass, updateImmediately: true);
				Bool_3 = false;
				Bool_4 = false;
			}
			base.OnLateUpdate();
		}
	}

	public override void OnUpdateOcclusionEffects(SourceContainerClass sourceContainer, bool updateImmediately = false)
	{
		if (!method_9())
		{
			ForceUpdate();
		}
		Vector3 currentPosition = sourceContainer.CurrentPosition;
		if (sourceContainer.IsOutOfHearingRangeWithOffset(5f))
		{
			return;
		}
		float score = Gclass1128_1.GetScore();
		float num = AudioOcclusionSettings_0.commonSettings.smoothingFactor * Time.deltaTime;
		Float_4 = ((score <= 0f) ? Mathf.MoveTowards(Float_4, 0f, num) : Mathf.Lerp(Float_4, score, Mathf.Clamp01(num)));
		Float_4 = Mathf.Clamp01(Float_4);
		base.PropagationEffect = Float_4;
		float score2 = Gclass1128_0.GetScore();
		Float_5 = ((score2 <= 0f || score > 0f) ? Mathf.MoveTowards(Float_5, 0f, num) : Mathf.Lerp(Float_5, score2, Mathf.Clamp01(num)));
		Float_5 = Mathf.Clamp01(Float_5);
		base.ObstructionEffect = Float_5;
		if (base.Boolean_0 != Bool_1)
		{
			Vector3 positionOffset = (base.Boolean_0 ? Vector3.zero : new Vector3(0f, AudioOcclusionSettings_0.commonSettings.playerObstructionYOffset, 0f));
			SourceContainerClass.SetPositionOffset(positionOffset);
			Gclass1128_0.Reset();
			Bool_1 = base.Boolean_0;
		}
		bool flag = base.Boolean_0 != Bool_2;
		bool flag2 = (base.Boolean_0 ? (Mathf.Abs(Float_4 - Float_6) > AudioOcclusionSettings_0.commonSettings.effectChangeThreshold) : (Mathf.Abs(Float_5 - Float_7) > AudioOcclusionSettings_0.commonSettings.effectChangeThreshold));
		bool num2 = updateImmediately || flag || flag2;
		Bool_2 = base.Boolean_0;
		Float_6 = Float_4;
		Float_7 = Float_5;
		if (!num2)
		{
			method_8(sourceContainer);
			method_5(sourceContainer);
			return;
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

	public bool method_9()
	{
		return base.Boolean_0 != SpatialAudioSystem_1.IsSourceInListenerRoom(SourceContainerClass);
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
		if (Gclass1128_1 != null)
		{
			SpatialAudioSystem_0.ReturnCalculator(Gclass1128_1);
			Gclass1128_1 = null;
		}
		base.Dispose();
	}
}
