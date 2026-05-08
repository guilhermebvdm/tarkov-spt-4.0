using Audio.SpatialSystem.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct Struct217 : IJobParallelFor
{
	[ReadOnly]
	public Vector3 ListenerPosition;

	[ReadOnly]
	public NativeArray<PortalData> PortalsData;

	[WriteOnly]
	public NativeArray<float> DistancesToListener;

	public void Execute(int index)
	{
		DistancesToListener[index] = Vector3.Distance(ListenerPosition, PortalsData[index].position);
	}
}
