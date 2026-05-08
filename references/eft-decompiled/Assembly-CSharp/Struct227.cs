using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct Struct227 : IJobParallelFor
{
	[ReadOnly]
	public Vector3 listenerPos;

	[ReadOnly]
	public NativeArray<Vector3> lineStarts;

	[ReadOnly]
	public NativeArray<Vector3> lineEnds;

	[WriteOnly]
	public NativeArray<Vector3> calculatedClosestPoints;

	[WriteOnly]
	public NativeArray<double> calculatedSqrDistances;

	public void Execute(int index)
	{
		Vector3 vector = Class823.smethod_2(listenerPos, lineStarts[index], lineEnds[index]);
		calculatedClosestPoints[index] = vector;
		calculatedSqrDistances[index] = Vector3.SqrMagnitude(vector - listenerPos);
	}
}
