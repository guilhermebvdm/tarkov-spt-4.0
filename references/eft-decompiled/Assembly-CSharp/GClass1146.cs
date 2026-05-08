using System;
using System.Collections.Generic;
using Audio.SpatialSystem;
using UnityEngine;

public class GClass1146 : GClass1141
{
	[NonSerialized]
	public GClass1180 Gclass1180_0;

	[NonSerialized]
	public Dictionary<int, GClass1129> Dictionary_0 = new Dictionary<int, GClass1129>();

	[NonSerialized]
	public Dictionary<int, GInterface89> Dictionary_1 = new Dictionary<int, GInterface89>();

	[NonSerialized]
	public Dictionary<int, SourceContainerClass> Dictionary_2 = new Dictionary<int, SourceContainerClass>();

	[NonSerialized]
	public Dictionary<int, bool> Dictionary_3 = new Dictionary<int, bool>();

	[NonSerialized]
	public List<int> List_0 = new List<int>();

	public GClass1146(SpatialAudioSystem audioSystem, Transform listenerTransform, LoggerClass logger)
		: base(audioSystem, listenerTransform, logger)
	{
		Gclass1180_0 = new GClass1180();
	}

	public override void SetAudioSourceContainer(SourceContainerClass sourceContainer)
	{
		method_6();
		if (!sourceContainer.IsValid)
		{
			return;
		}
		ESoundOcclusionType eSoundOcclusionType = method_7(sourceContainer);
		if (eSoundOcclusionType == ESoundOcclusionType.None)
		{
			return;
		}
		BetterSource betterSource = sourceContainer[0];
		betterSource.SetOcclusionVolumeFactor(0f);
		betterSource.UpdateSourceVolume();
		betterSource.SetLowPassFilterParameters(20f, 1f, applyImmediately: true);
		base.SetAudioSourceContainer(sourceContainer);
		int iD = sourceContainer.ID;
		if (Dictionary_2.TryGetValue(iD, out var _))
		{
			if (Dictionary_0.TryGetValue(iD, out var value2))
			{
				SpatialAudioSystem_0.ReturnCalculator(value2);
				Dictionary_0.Remove(iD);
			}
			if (Dictionary_1.TryGetValue(iD, out var value3))
			{
				SpatialAudioSystem_0.ReturnCalculator(value3);
				Dictionary_1.Remove(iD);
			}
			Dictionary_3.Remove(iD);
			Dictionary_2.Remove(iD);
		}
		List_0.Remove(iD);
		if (eSoundOcclusionType == ESoundOcclusionType.Propagation)
		{
			GClass1129 value4 = SpatialAudioSystem_0.WithdrawCalculator<GClass1129>();
			Dictionary_0[iD] = value4;
		}
		else
		{
			GClass1170 value5 = SpatialAudioSystem_0.WithdrawCalculator<GClass1170>();
			Dictionary_1[iD] = value5;
		}
		Dictionary_2[iD] = sourceContainer;
		Dictionary_3[iD] = false;
	}

	public void method_6()
	{
		if (List_0.Count == 0)
		{
			return;
		}
		foreach (int item in List_0)
		{
			if (Dictionary_0.TryGetValue(item, out var value))
			{
				SpatialAudioSystem_0.ReturnCalculator(value);
				Dictionary_0.Remove(item);
			}
			if (Dictionary_1.TryGetValue(item, out var value2))
			{
				SpatialAudioSystem_0.ReturnCalculator(value2);
				Dictionary_1.Remove(item);
			}
			Dictionary_3.Remove(item);
			Dictionary_2.Remove(item);
		}
		List_0.Clear();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		method_6();
		foreach (var (num2, sourceContainerClass2) in Dictionary_2)
		{
			if (!sourceContainerClass2.IsValid)
			{
				List_0.Add(num2);
				continue;
			}
			if (method_7(sourceContainerClass2) == ESoundOcclusionType.Propagation)
			{
				if (!Dictionary_0.TryGetValue(num2, out var value))
				{
					continue;
				}
				value.ManualUpdate(sourceContainerClass2, base.Vector3_0, forceUpdate: true);
			}
			else
			{
				if (!Dictionary_1.TryGetValue(num2, out var value2))
				{
					continue;
				}
				if (!value2.IsCalculationInProgress)
				{
					float sqrMagnitude = (sourceContainerClass2.CurrentPosition - base.Vector3_0).sqrMagnitude;
					float a = Gclass1180_0.CalculateQualityFactorSq(sqrMagnitude, sourceContainerClass2.SqrMaxDistance);
					a = Mathf.Min(a, sourceContainerClass2.MaxOcclusionQualityFactor);
					value2.StartCalculation(base.Vector3_0, sourceContainerClass2.CurrentPosition, sourceContainerClass2.MaxDistance, a);
				}
			}
			Dictionary_3[num2] = true;
		}
	}

	public override void OnLateUpdate()
	{
		base.OnLateUpdate();
		method_6();
		foreach (var (num2, sourceContainerClass2) in Dictionary_2)
		{
			if (!sourceContainerClass2.IsValid)
			{
				List_0.Add(num2);
			}
			else
			{
				if (Dictionary_3.TryGetValue(num2, out var value) && !value)
				{
					continue;
				}
				if (method_7(sourceContainerClass2) == ESoundOcclusionType.Propagation)
				{
					if (!Dictionary_0.TryGetValue(num2, out var value2))
					{
						continue;
					}
					value2.ManualLateUpdate();
					if (!value2.LastHandle.IsCompleted)
					{
						continue;
					}
					float occlusionEffect = (base.PropagationEffect = value2.GetScore());
					base.OcclusionEffect = occlusionEffect;
				}
				else
				{
					if (!Dictionary_1.TryGetValue(num2, out var value3))
					{
						continue;
					}
					value3.UpdateCalculationStep();
					if (!value3.LastJobHandle.IsCompleted)
					{
						continue;
					}
					value3.CompleteCalculation();
					base.ObstructionEffect = value3.GetNormalizedPathScore();
				}
				if (sourceContainerClass2.IsOutOfHearingRangeWithOffset(5f))
				{
					List_0.Add(num2);
				}
				else
				{
					UpdateOcclusionEffects(sourceContainerClass2, updateImmediately: true);
				}
			}
		}
	}

	public override void OnUpdateOcclusionEffects(SourceContainerClass sourceContainer, bool updateImmediately = false)
	{
		BetterSource source = sourceContainer[0];
		CalculateFinalOcclusionParams(sourceContainer.CurrentAudioRoom, source, sourceContainer.CurrentPosition);
		ApplyOcclusionToSource(source, updateImmediately);
		List_0.Add(sourceContainer.ID);
	}

	public override void ApplyOcclusionToSource(BetterSource source, bool updateImmediately = false)
	{
		if (Mathf.Approximately(Gclass1127_0.OcclusionVolumeFactor, 0f))
		{
			source.Release();
			return;
		}
		base.ApplyOcclusionToSource(source, updateImmediately);
		source.UpdateSourceVolume();
	}

	public ESoundOcclusionType method_7(SourceContainerClass container)
	{
		if (!SpatialAudioSystem_0.IsSourceAndListenerOutdoor(container.CurrentAudioRoom) && SpatialAudioSystem_0.IsSourceInListenerRoom(container))
		{
			return ESoundOcclusionType.None;
		}
		if (container.OcclusionTest == EOcclusionTest.OneShotPropagation)
		{
			return ESoundOcclusionType.Propagation;
		}
		if (container.OcclusionTest != EOcclusionTest.OneShotFullOcclusion)
		{
			throw new Exception($"Can't handle not proposed occlusion test : {container.OcclusionTest}");
		}
		if (!SpatialAudioSystem_0.IsSourceAndListenerInCommonOutdoor(container.CurrentAudioRoom) && (!SpatialAudioSystem_0.IsSourceInListenerRoom(container) || !SpatialAudioSystem_0.IsSourceAndListenerOutdoor(container.CurrentAudioRoom)))
		{
			return ESoundOcclusionType.Propagation;
		}
		return ESoundOcclusionType.Obstruction;
	}

	public override void Dispose()
	{
		int key;
		foreach (KeyValuePair<int, GClass1129> item in Dictionary_0)
		{
			item.Deconstruct(out key, out var value);
			GClass1129 calculator = value;
			SpatialAudioSystem_0.ReturnCalculator(calculator);
		}
		foreach (KeyValuePair<int, GInterface89> item2 in Dictionary_1)
		{
			item2.Deconstruct(out key, out var value2);
			GInterface89 calculator2 = value2;
			SpatialAudioSystem_0.ReturnCalculator(calculator2);
		}
		Dictionary_0.Clear();
		Dictionary_1.Clear();
		base.Dispose();
	}
}
