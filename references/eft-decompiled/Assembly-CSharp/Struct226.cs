using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct Struct226 : IJobParallelFor
{
	[ReadOnly]
	public Vector3 targetPoint;

	[ReadOnly]
	public NativeArray<Vector3> points;

	[NativeDisableParallelForRestriction]
	public NativeArray<int> crossingNumber;

	public void Execute(int index)
	{
		Vector3 vector = points[index];
		Vector3 vector2 = points[(index + 1) % points.Length];
		if (vector.z > targetPoint.z != vector2.z > targetPoint.z && targetPoint.x < (vector2.x - vector.x) * (targetPoint.z - vector.z) / (vector2.z - vector.z) + vector.x)
		{
			crossingNumber[0]++;
		}
	}
}
