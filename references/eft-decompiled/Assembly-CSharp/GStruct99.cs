using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct GStruct99 : IJob
{
	[NonSerialized]
	public const float Float_0 = float.MaxValue;

	[NonSerialized]
	public const int Int_0 = -1;

	[NonSerialized]
	public const byte Byte_0 = 1;

	[ReadOnly]
	public NativeArray<float> rayMinPathLengths;

	[ReadOnly]
	public NativeArray<float> rayEnergies;

	[ReadOnly]
	public NativeList<GStruct101> rayPathPointInfos;

	[ReadOnly]
	public NativeArray<byte> rayEndStatuses;

	[WriteOnly]
	public NativeArray<GStruct104> outputResult;

	public void Execute()
	{
		float num = float.MaxValue;
		int num2 = -1;
		for (int i = 0; i < rayMinPathLengths.Length; i++)
		{
			if (rayEndStatuses[i] == 1 && rayMinPathLengths[i] < num)
			{
				num = rayMinPathLengths[i];
				num2 = i;
			}
		}
		GStruct104 value = new GStruct104
		{
			found = false,
			pathPoints = default(FixedList128Bytes<GStruct101>)
		};
		if (num2 != -1)
		{
			value.pathLength = num;
			value.energy = rayEnergies[num2];
			value.rayIndex = num2;
			value.found = true;
			for (int j = 0; j < rayPathPointInfos.Length; j++)
			{
				if (rayPathPointInfos[j].rayIndex == num2 && value.pathPoints.Length < value.pathPoints.Capacity)
				{
					value.pathPoints.Add(rayPathPointInfos[j]);
				}
			}
		}
		outputResult[0] = value;
	}
}
