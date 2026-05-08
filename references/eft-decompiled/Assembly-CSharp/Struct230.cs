using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct Struct230 : IJobParallelFor
{
	[WriteOnly]
	public NativeArray<Vector3> calculatedClosestPoints;

	[WriteOnly]
	public NativeArray<double> calculatedSqrDistances;

	[NativeDisableParallelForRestriction]
	public NativeArray<double> totalWeight;

	[NativeDisableParallelForRestriction]
	public NativeArray<bool> withinRange;

	[NativeDisableParallelForRestriction]
	public NativeArray<Vector3> totalDirection;

	[NativeDisableParallelForRestriction]
	public NativeArray<int> crossingNumber;

	public void Execute(int index)
	{
		calculatedClosestPoints[index] = Vector3.zero;
		calculatedSqrDistances[index] = double.MaxValue;
		totalWeight[0] = 0.0;
		withinRange[0] = false;
		totalDirection[0] = Vector3.zero;
		crossingNumber[0] = 0;
	}
}
