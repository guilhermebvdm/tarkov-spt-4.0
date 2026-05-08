using System;
using Unity.Jobs;
using UnityEngine;

public interface GInterface89 : IDisposable
{
	bool IsCalculationInProgress { get; }

	JobHandle LastJobHandle { get; }

	JobHandle StartCalculation(Vector3 listenerPosition, Vector3 sourcePosition, float maxDistance, float qualityFactor = 1f, JobHandle inputDeps = default(JobHandle));

	JobHandle UpdateCalculationStep(JobHandle inputDeps = default(JobHandle));

	void CompleteCalculation();

	float GetNormalizedPathScore();
}
