using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct GStruct98 : IJob
{
	[ReadOnly]
	public NativeArray<float> rayMinPathLengths;

	[ReadOnly]
	public NativeArray<float> rayEnergies;

	[ReadOnly]
	public NativeList<GStruct101> rayPathPointInfos;

	[ReadOnly]
	public NativeArray<byte> rayEndStatuses;

	[ReadOnly]
	public GStruct104 initialBestOverallResult;

	[ReadOnly]
	public GStruct104 initialBestReflectedResult;

	[ReadOnly]
	public NativeArray<RaycastHit> previousStepSphereHits;

	[ReadOnly]
	public int directSpherecastCommandIndex;

	[ReadOnly]
	public float3 sourcePosition;

	[ReadOnly]
	public float3 listenerPosition;

	[ReadOnly]
	public float minReflectionEnergyThreshold;

	[ReadOnly]
	public NativeArray<SpherecastCommand> scheduledSpherecastCommands;

	[ReadOnly]
	public NativeArray<GStruct105> rayStates;

	[ReadOnly]
	public int maxCommandBufferSize;

	[WriteOnly]
	public NativeArray<SpherecastCommand> outputSpherecastCommands;

	[WriteOnly]
	public NativeArray<int2> outputCommandIndicesMap;

	[WriteOnly]
	public NativeArray<GStruct103> outputBatchProcessing;

	public void Execute()
	{
		float num = float.MaxValue;
		int num2 = -1;
		for (int i = 0; i < rayMinPathLengths.Length; i++)
		{
			if (rayEndStatuses[i] == 1)
			{
				float num3 = rayMinPathLengths[i];
				if (num3 < num)
				{
					num = num3;
					num2 = i;
				}
			}
		}
		GStruct104 gStruct = new GStruct104
		{
			found = false,
			pathPoints = default(FixedList128Bytes<GStruct101>)
		};
		if (num2 != -1)
		{
			gStruct.found = true;
			gStruct.pathLength = num;
			gStruct.energy = rayEnergies[num2];
			gStruct.rayIndex = num2;
			for (int j = 0; j < rayPathPointInfos.Length; j++)
			{
				GStruct101 item = rayPathPointInfos[j];
				if (item.rayIndex == num2 && gStruct.pathPoints.Length < gStruct.pathPoints.Capacity)
				{
					gStruct.pathPoints.Add(in item);
				}
			}
		}
		GStruct103 value = default(GStruct103);
		bool foundDirectPathClearThisBatch = false;
		if (directSpherecastCommandIndex >= 0 && directSpherecastCommandIndex < previousStepSphereHits.Length)
		{
			foundDirectPathClearThisBatch = previousStepSphereHits[directSpherecastCommandIndex].colliderInstanceID == 0;
		}
		value.foundDirectPathClearThisBatch = foundDirectPathClearThisBatch;
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
		outputBatchProcessing[0] = value;
		int num4 = 0;
		for (int k = 0; k < rayStates.Length; k++)
		{
			GStruct105 gStruct2 = rayStates[k];
			int x = -1;
			int y = -1;
			if (gStruct2.isActive)
			{
				int raySegmentCommandIndex = gStruct2.raySegmentCommandIndex;
				if (raySegmentCommandIndex >= 0 && num4 < maxCommandBufferSize && raySegmentCommandIndex < scheduledSpherecastCommands.Length)
				{
					outputSpherecastCommands[num4] = scheduledSpherecastCommands[raySegmentCommandIndex];
					x = num4++;
				}
				int listenerCheckSphereCommandIndex = gStruct2.listenerCheckSphereCommandIndex;
				if (listenerCheckSphereCommandIndex >= 0 && num4 < maxCommandBufferSize && listenerCheckSphereCommandIndex < scheduledSpherecastCommands.Length)
				{
					outputSpherecastCommands[num4] = scheduledSpherecastCommands[listenerCheckSphereCommandIndex];
					y = num4++;
				}
			}
			outputCommandIndicesMap[k] = new int2(x, y);
		}
		for (int l = num4; l < maxCommandBufferSize; l++)
		{
			outputSpherecastCommands[l] = default(SpherecastCommand);
		}
	}
}
