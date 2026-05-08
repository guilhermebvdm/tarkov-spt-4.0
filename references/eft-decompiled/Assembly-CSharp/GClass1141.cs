using System;
using System.Runtime.CompilerServices;
using Audio.Effects;
using Audio.SpatialSystem;
using EFT;
using UnityEngine;

public abstract class GClass1141 : GInterface87, GInterface86
{
	[NonSerialized]
	public const float Float_0 = 5f;

	[NonSerialized]
	public GClass1127 Gclass1127_0 = new GClass1127();

	[NonSerialized]
	public SpatialAudioSystem SpatialAudioSystem_0;

	[NonSerialized]
	public Transform Transform_0;

	[NonSerialized]
	public LoggerClass LoggerClass;

	[NonSerialized]
	public SourceContainerClass SourceContainerClass;

	[NonSerialized]
	public GClass1156 Gclass1156_0;

	[NonSerialized]
	public GClass1154 Gclass1154_0;

	[NonSerialized]
	public GClass1153 Gclass1153_0;

	[NonSerialized]
	public GClass1155 Gclass1155_0;

	[NonSerialized]
	public GClass1161 Gclass1161_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_1;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_2;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_3;

	public Vector3 Vector3_0
	{
		get
		{
			if (!(GamePlayerOwner.MyPlayer == null))
			{
				return GamePlayerOwner.MyPlayer.PlayerColliderPointOnCenterAxis(0.5f);
			}
			return Transform_0.position;
		}
	}

	public float PropagationEffect
	{
		[CompilerGenerated]
		get
		{
			return Float_1;
		}
		[CompilerGenerated]
		set
		{
			Float_1 = value;
		}
	}

	public float ObstructionEffect
	{
		[CompilerGenerated]
		get
		{
			return Float_2;
		}
		[CompilerGenerated]
		set
		{
			Float_2 = value;
		}
	}

	public float OcclusionEffect
	{
		[CompilerGenerated]
		get
		{
			return Float_3;
		}
		[CompilerGenerated]
		set
		{
			Float_3 = value;
		}
	}

	public bool Boolean_0 => PropagationEffect > 0f;

	public static GClass1141 Create(Transform listenerTransform, EOcclusionTest test, LoggerClass logger)
	{
		return GClass1148.Create(listenerTransform, test, logger);
	}

	public GClass1141(SpatialAudioSystem audioSystem, Transform listenerTransform, LoggerClass logger)
	{
		SpatialAudioSystem_0 = audioSystem;
		Transform_0 = listenerTransform;
		LoggerClass = logger;
		SourceContainerClass = new SourceContainerClass();
		Gclass1156_0 = new GClass1156();
		Gclass1154_0 = new GClass1154();
		Gclass1153_0 = new GClass1153(listenerTransform);
		Gclass1155_0 = new GClass1155(Gclass1156_0, LoggerClass);
		Gclass1161_0 = new GClass1161();
	}

	public void ForceUpdate()
	{
		OnForceUpdate();
	}

	public virtual void OnForceUpdate()
	{
	}

	public virtual void SetAudioSourceContainer(SourceContainerClass sourceContainer)
	{
		SourceContainerClass = sourceContainer;
	}

	public void UpdateOcclusionEffects(SourceContainerClass container, bool updateImmediately = false)
	{
		if (!Bool_0 && container.IsValid)
		{
			OnUpdateOcclusionEffects(container, updateImmediately);
		}
	}

	public void ManualUpdate()
	{
		if (!Bool_0)
		{
			OnUpdate();
		}
	}

	public void ManualLateUpdate()
	{
		if (!Bool_0)
		{
			OnLateUpdate();
		}
	}

	public virtual void OnUpdateOcclusionEffects(SourceContainerClass sourceContainer, bool updateImmediately = false)
	{
	}

	public virtual void OnUpdate()
	{
	}

	public virtual void OnLateUpdate()
	{
	}

	public virtual void CalculateFinalOcclusionParams(ISpatialAudioRoom currentSourceRoom, BetterSource source, Vector3 sourcePos)
	{
		AudioGroupOcclusionSettings spatialSettings = source.SpatialSettings;
		Vector3 vector3_ = Vector3_0;
		OcclusionEffect = Mathf.Clamp01(CalculateTotalEffect() * SpatialAudioSystem_0.GetEnvironmentFactor(currentSourceRoom, spatialSettings));
		AnimationCurve curve = (Boolean_0 ? spatialSettings.propagationEQPreset.lpfSettings.distanceCurve : spatialSettings.obstructionEQPreset.lpfSettings.distanceCurve);
		AnimationCurve curve2 = (Boolean_0 ? spatialSettings.propagationEQPreset.hpfSettings.distanceCurve : spatialSettings.obstructionEQPreset.hpfSettings.distanceCurve);
		Gclass1127_0.IsolationFactor = SpatialAudioSystem_0.GetIsolationFactor(currentSourceRoom);
		Gclass1127_0.DistancePercentage = Gclass1154_0.Compute(vector3_, source, LoggerClass);
		Gclass1127_0.DistanceHiCutFrequency = method_2(curve);
		Gclass1127_0.DistanceLowCutFrequency = method_2(curve2);
		Gclass1127_0.ObstructionVolumeFactor = method_3(spatialSettings.obstructionEQPreset.volumeCurve, ObstructionEffect);
		Gclass1127_0.PropagationVolumeFactor = method_3(spatialSettings.propagationEQPreset.volumeCurve, PropagationEffect);
		Gclass1127_0.OcclusionVolumeFactor = method_0(currentSourceRoom, sourcePos, vector3_, spatialSettings);
		Gclass1127_0.OcclusionRolloffScale = method_4(spatialSettings.rolloffScale);
		Gclass1161_0.Compute(vector3_, sourcePos, spatialSettings, Boolean_0, out var lowPassResonance, out var highPassResonance);
		Gclass1127_0.LowPassResonance = lowPassResonance;
		Gclass1127_0.HighPassResonance = highPassResonance;
	}

	public virtual void ApplyOcclusionToSource(BetterSource source, bool updateImmediately = false)
	{
		AudioGroupOcclusionSettings spatialSettings = source.SpatialSettings;
		GStruct89 w = Gclass1153_0.Calculate(source.Position);
		float heightDistance = Mathf.Abs(source.Position.y - Transform_0.position.y);
		bool isInDiffEvn = SpatialAudioSystem_0.IsSourceAndListenerInDiffEnvironment(SourceContainerClass.CurrentAudioRoom);
		AudioOcclusionEQPreset audioOcclusionEQPreset = (Boolean_0 ? spatialSettings.propagationEQPreset : spatialSettings.obstructionEQPreset);
		GStruct89 influencedWeights = Gclass1153_0.Influence(w, audioOcclusionEQPreset.rotationCoefficient, audioOcclusionEQPreset.distanceCoefficient, Gclass1127_0.DistancePercentage);
		Gclass1155_0.Compute(audioOcclusionEQPreset, OcclusionEffect, influencedWeights, heightDistance, Boolean_0, SpatialAudioSystem_0.OcclusionSettings.commonSettings.floorHeight, Gclass1127_0.IsolationFactor, isInDiffEvn, out var lowPassFreq, out var highPassFreq);
		source.SetOcclusionVolumeFactor(Gclass1127_0.OcclusionVolumeFactor);
		source.SetOcclusionRolloffScale(Gclass1127_0.OcclusionRolloffScale);
		source.SetLowPassFilterParameters(lowPassFreq, Gclass1127_0.LowPassResonance, updateImmediately);
		source.SetHighPassFilterParameters(highPassFreq, Gclass1127_0.HighPassResonance, updateImmediately);
	}

	public virtual float CalculateTotalEffect()
	{
		return Mathf.Clamp01(Boolean_0 ? PropagationEffect : ObstructionEffect);
	}

	public float method_0(ISpatialAudioRoom sourceRoom, Vector3 sourcePos, Vector3 listenerPos, AudioGroupOcclusionSettings settings)
	{
		bool isInDiffEnv = SpatialAudioSystem_0.IsSourceAndListenerInDiffEnvironment(sourceRoom);
		if (Boolean_0)
		{
			Gclass1127_0.PropagationVolumeFactor = method_1(sourceRoom, settings.propagationEQPreset, Gclass1127_0.PropagationVolumeFactor, isInDiffEnv, Gclass1127_0.IsolationFactor);
		}
		float heightDiff = Mathf.Abs(sourcePos.y - listenerPos.y);
		float time = Gclass1156_0.Compute(heightDiff, SpatialAudioSystem_0.OcclusionSettings.commonSettings.floorHeight, LoggerClass);
		float num = (Boolean_0 ? settings.propagationEQPreset.heightVolumeCurve.Evaluate(time) : settings.obstructionEQPreset.heightVolumeCurve.Evaluate(time));
		if (Boolean_0)
		{
			num = Mathf.Lerp(1f, num, GClass2313.LogarithmicInterpolation(OcclusionEffect));
			Gclass1127_0.PropagationVolumeFactor = Mathf.Clamp01(Gclass1127_0.PropagationVolumeFactor * num);
		}
		else
		{
			Gclass1127_0.ObstructionVolumeFactor = Mathf.Clamp01(Gclass1127_0.ObstructionVolumeFactor * num);
		}
		if (!Boolean_0)
		{
			return Gclass1127_0.ObstructionVolumeFactor;
		}
		return Gclass1127_0.PropagationVolumeFactor;
	}

	public float method_1(ISpatialAudioRoom sourceRoom, AudioOcclusionEQPreset preset, float baseVol, bool isInDiffEnv, float isolationFactor)
	{
		baseVol = Mathf.Max(baseVol, preset.environmentVolumeThresholds.baseValue);
		if (!isInDiffEnv)
		{
			baseVol = ((sourceRoom.IsIsolated == SpatialAudioSystem_0.ListenerCurrentRoom.IsIsolated) ? Mathf.Lerp(baseVol, preset.environmentVolumeThresholds.indoorIsolated, isolationFactor) : Mathf.Lerp(baseVol, preset.environmentVolumeThresholds.diffRoomsTypeIsolated, isolationFactor));
		}
		else
		{
			float b = (GClass1118.IsOutdoor(sourceRoom.Type) ? preset.environmentVolumeThresholds.outdoorToIndoor : preset.environmentVolumeThresholds.indoorToOutdoor);
			baseVol = Mathf.Max(baseVol, b);
			baseVol = Mathf.Lerp(baseVol, preset.environmentVolumeThresholds.diffEnvironmentIsolated, isolationFactor);
		}
		return baseVol;
	}

	public float method_2(AnimationCurve curve)
	{
		return curve.Evaluate(Gclass1127_0.DistancePercentage);
	}

	public float method_3(AnimationCurve volumeCurve, float value)
	{
		if (value <= 0f)
		{
			return 1f;
		}
		return volumeCurve.Evaluate(value);
	}

	public float method_4(float maxScale)
	{
		if (Boolean_0 && maxScale < 1f)
		{
			return Mathf.Lerp(maxScale, 1f, OcclusionEffect);
		}
		return 1f;
	}

	public void method_5(SourceContainerClass container)
	{
		if (container.IsValid)
		{
			container.UpdateOcclusionData(this);
		}
	}

	public virtual void Dispose()
	{
		Gclass1127_0.Dispose();
		Bool_0 = true;
	}
}
