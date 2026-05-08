using System;
using Audio.SpatialSystem.Data;
using Unity.Jobs;
using UnityEngine;

public class GClass1171 : GInterface89, IDisposable
{
	public bool IsCalculationInProgress => false;

	public JobHandle LastJobHandle => default(JobHandle);

	public GClass1171(TransmissionSettings settings, QueryParameters queryParams, float maxDistance)
	{
	}

	public JobHandle StartCalculation(Vector3 listenerPosition, Vector3 sourcePosition, float maxDistance, float qualityFactor = 1f, JobHandle inputDeps = default(JobHandle))
	{
		return default(JobHandle);
	}

	public JobHandle UpdateCalculationStep(JobHandle inputDeps = default(JobHandle))
	{
		return default(JobHandle);
	}

	public void CompleteCalculation()
	{
	}

	public float GetNormalizedPathScore()
	{
		return 0f;
	}

	public void Dispose()
	{
	}
}
