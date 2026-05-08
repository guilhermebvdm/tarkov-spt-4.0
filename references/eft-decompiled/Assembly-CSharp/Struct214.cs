using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct Struct214 : IJob
{
	[NonSerialized]
	public const float Float_0 = 0f;

	[NonSerialized]
	public const float Float_1 = 1f;

	[ReadOnly]
	public NativeArray<GStruct106> rayResults;

	[WriteOnly]
	public NativeArray<GStruct106> aggregatedResultBuffer;

	public void Execute()
	{
		int length = rayResults.Length;
		float num = 0f;
		float num2 = 0f;
		bool pathPotentiallyValid = false;
		for (int i = 0; i < length; i++)
		{
			GStruct106 gStruct = rayResults[i];
			if (gStruct.PathPotentiallyValid)
			{
				pathPotentiallyValid = true;
			}
			float num3 = (gStruct.PathPotentiallyValid ? gStruct.RemainingEnergy : 0f);
			num += num3;
			num2 += num3 * gStruct.TotalThickness;
		}
		float remainingEnergy = math.clamp(num / (float)length, 0f, 1f);
		float totalThickness = ((num > 0f) ? (num2 / num) : float.MaxValue);
		aggregatedResultBuffer[0] = new GStruct106
		{
			RemainingEnergy = remainingEnergy,
			TotalThickness = totalThickness,
			PathPotentiallyValid = pathPotentiallyValid
		};
	}
}
