using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct Struct228 : IJobParallelFor
{
	[ReadOnly]
	public Vector3 listenerPos;

	[ReadOnly]
	public float calculationRadius;

	[ReadOnly]
	public NativeArray<Vector3> lineStarts;

	[ReadOnly]
	public NativeArray<Vector3> lineEnds;

	[ReadOnly]
	public NativeArray<double> calculatedSqrDistances;

	[NativeDisableParallelForRestriction]
	public NativeArray<double> totalWeight;

	[NativeDisableParallelForRestriction]
	public NativeArray<bool> withinRange;

	[NativeDisableParallelForRestriction]
	public NativeArray<Vector3> totalDirection;

	public void Execute(int index)
	{
		if (calculatedSqrDistances[index] + double.Epsilon < (double)(calculationRadius * calculationRadius))
		{
			totalDirection[0] += Class823.smethod_0(listenerPos, lineStarts[index], lineEnds[index], calculationRadius, out var spread);
			totalWeight[0] += spread;
			withinRange[0] = true;
		}
	}
}
