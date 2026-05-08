using Audio.SpatialSystem.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct Struct218 : IJobParallelFor
{
	[ReadOnly]
	public int currentIndoorRoomID;

	[ReadOnly]
	public float listenerHeightWeight;

	[ReadOnly]
	public Vector3 listenerPosition;

	[ReadOnly]
	public NativeArray<PortalData> portalsData;

	[ReadOnly]
	public NativeArray<float> distancesToListener;

	[WriteOnly]
	public NativeArray<GStruct108> calculatedPortalData;

	[ReadOnly]
	public NativeArray<short> indoorRoomsIDs;

	public void Execute(int index)
	{
		PortalData portalData = portalsData[index];
		if (!smethod_0(portalData.state))
		{
			return;
		}
		float num = distancesToListener[index];
		float depth = portalData.depth;
		if (!smethod_1(num, depth))
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < portalData.indoorRoomsCount; i++)
		{
			if (indoorRoomsIDs[portalData.startIndex + i] == currentIndoorRoomID)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			float num2 = Mathf.Abs(listenerPosition.y - portalData.position.y);
			float num3 = num / depth;
			float b = GClass1162.CalculateHorizontalDiffraction(listenerPosition, portalData.position);
			b = Mathf.Lerp(0f, b, num3);
			float num4 = num2 * listenerHeightWeight;
			float score = num3 + b + num4;
			calculatedPortalData[index] = new GStruct108(score, distancesToListener[index], portalsData[index]);
		}
	}

	public static bool smethod_0(byte state)
	{
		if (state != 1)
		{
			return state != 3;
		}
		return false;
	}

	public static bool smethod_1(float distanceToPortal, float depth)
	{
		return distanceToPortal <= depth;
	}
}
