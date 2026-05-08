using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct Struct229 : IJob
{
	[ReadOnly]
	public NativeArray<Vector3> calculatedClosestPoints;

	[ReadOnly]
	public NativeArray<double> calculatedSqrDistances;

	[ReadOnly]
	public NativeArray<Vector3> totalDirection;

	[ReadOnly]
	public NativeArray<double> totalWeight;

	[ReadOnly]
	public float calculationRadius;

	[ReadOnly]
	public NativeArray<bool> withinRange;

	[ReadOnly]
	public NativeArray<int> crossingNumber;

	[WriteOnly]
	public NativeArray<Struct225> result;

	public void Execute()
	{
		double num = double.MaxValue;
		Vector3 vector = Vector3.zero;
		float num2 = calculationRadius * calculationRadius;
		for (int i = 0; i < calculatedClosestPoints.Length; i++)
		{
			if (calculatedSqrDistances[i] < num)
			{
				num = calculatedSqrDistances[i];
				vector = calculatedClosestPoints[i];
			}
		}
		double num3 = Math.Sqrt(Vector3.SqrMagnitude(totalDirection[0]));
		double closestDistance = Math.Sqrt(num);
		double averageSpread;
		Vector3 averageDirection;
		if (num3 > double.Epsilon)
		{
			averageSpread = num3 / totalWeight[0];
			averageDirection = totalDirection[0] / (float)num3;
		}
		else if (num < (double)num2)
		{
			averageSpread = 1.0;
			averageDirection = vector;
		}
		else
		{
			averageSpread = 0.0;
			averageDirection = vector;
		}
		result[0] = new Struct225
		{
			closestPoint = vector,
			averageDirection = averageDirection,
			closestDistance = closestDistance,
			averageSpread = averageSpread,
			withinRange = withinRange[0],
			isInside = (crossingNumber[0] % 2 == 1)
		};
	}
}
