using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct GStruct97 : IJob
{
	[NonSerialized]
	public const float Float_0 = 0.01f;

	[NonSerialized]
	public const int Int_0 = -1;

	[ReadOnly]
	public NativeArray<GStruct104> findShortestResult;

	[ReadOnly]
	public NativeArray<RaycastHit> previousStepSphereHits;

	[ReadOnly]
	public int directSpherecastCommandIndex;

	[ReadOnly]
	public float3 sourcePosition;

	[ReadOnly]
	public float3 listenerPosition;

	[ReadOnly]
	public float MaxDistance;

	[ReadOnly]
	public NativeArray<GStruct105> rayStates;

	[ReadOnly]
	public int maxCommandBufferSize;

	[ReadOnly]
	public float minReflectionEnergyThreshold;

	[ReadOnly]
	public GStruct104 initialBestOverallResult;

	[ReadOnly]
	public GStruct104 initialBestReflectedResult;

	[ReadOnly]
	public NativeArray<SpherecastCommand> scheduledSpherecastCommands;

	[WriteOnly]
	public NativeArray<SpherecastCommand> outputSpherecastCommands;

	[WriteOnly]
	public NativeArray<int2> outputCommandIndicesMap;

	[WriteOnly]
	public NativeArray<GStruct103> outputBatchProcessing;

	public void Execute()
	{
		GStruct103 value = default(GStruct103);
		bool foundDirectPathClearThisBatch = false;
		if (directSpherecastCommandIndex != -1 && directSpherecastCommandIndex < previousStepSphereHits.Length)
		{
			foundDirectPathClearThisBatch = previousStepSphereHits[directSpherecastCommandIndex].colliderInstanceID == 0;
		}
		value.foundDirectPathClearThisBatch = foundDirectPathClearThisBatch;
		GStruct104 gStruct = findShortestResult[0];
		GStruct104 bestOverallDataSoFar = initialBestOverallResult;
		GStruct104 bestReflectedDataSoFar = initialBestReflectedResult;
		if (gStruct.found)
		{
			if (gStruct.pathLength < bestOverallDataSoFar.pathLength && gStruct.energy >= minReflectionEnergyThreshold)
			{
				bestOverallDataSoFar = gStruct;
			}
			if (math.abs(gStruct.pathLength - math.distance(sourcePosition, listenerPosition)) >= 0.01f && gStruct.pathLength < bestReflectedDataSoFar.pathLength && gStruct.energy >= minReflectionEnergyThreshold)
			{
				bestReflectedDataSoFar = gStruct;
			}
		}
		value.bestOverallDataSoFar = bestOverallDataSoFar;
		value.bestReflectedDataSoFar = bestReflectedDataSoFar;
		int num = 0;
		for (int i = 0; i < rayStates.Length; i++)
		{
			GStruct105 gStruct2 = rayStates[i];
			int x = -1;
			int y = -1;
			if (gStruct2.isActive)
			{
				int raySegmentCommandIndex = gStruct2.raySegmentCommandIndex;
				if (raySegmentCommandIndex != -1 && num < maxCommandBufferSize && raySegmentCommandIndex < scheduledSpherecastCommands.Length)
				{
					outputSpherecastCommands[num] = scheduledSpherecastCommands[raySegmentCommandIndex];
					x = num++;
				}
				int listenerCheckSphereCommandIndex = gStruct2.listenerCheckSphereCommandIndex;
				if (listenerCheckSphereCommandIndex != -1 && num < maxCommandBufferSize && listenerCheckSphereCommandIndex < scheduledSpherecastCommands.Length)
				{
					outputSpherecastCommands[num] = scheduledSpherecastCommands[listenerCheckSphereCommandIndex];
					y = num++;
				}
			}
			outputCommandIndicesMap[i] = new int2(x, y);
		}
		for (int j = num; j < maxCommandBufferSize; j++)
		{
			outputSpherecastCommands[j] = default(SpherecastCommand);
		}
		outputBatchProcessing[0] = value;
	}
}
