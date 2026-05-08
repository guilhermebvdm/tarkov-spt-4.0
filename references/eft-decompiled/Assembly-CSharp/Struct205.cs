using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile(OptimizeFor = OptimizeFor.Performance, FloatMode = FloatMode.Fast)]
public struct Struct205 : IJob
{
	[ReadOnly]
	public NativeArray<float> lowerCosts;

	[ReadOnly]
	public NativeArray<float> lowerDistances;

	public NativeArray<float> bestCost;

	public NativeArray<float> bestDistance;

	[ReadOnly]
	public int lowerCostsLength;

	public void Execute()
	{
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		for (int i = 0; i < lowerCostsLength; i++)
		{
			float num3 = lowerCosts[i];
			float num4 = lowerDistances[i];
			if (num3 < num - 0.01f)
			{
				num = num3;
				num2 = num4;
			}
			else if (math.abs(num3 - num) <= 0.01f && num4 < num2)
			{
				num2 = num4;
			}
		}
		bestCost[0] = num;
		bestDistance[0] = num2;
	}
}
