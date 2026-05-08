using System;
using Audio.SpatialSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile(OptimizeFor = OptimizeFor.Performance, FloatMode = FloatMode.Fast)]
public struct Struct204 : IJobParallelFor
{
	[NonSerialized]
	public const float Float_0 = 0.05f;

	[NonSerialized]
	public const byte Byte_0 = 2;

	[NonSerialized]
	public const float Float_1 = 1E-06f;

	[ReadOnly]
	public float minPortalCostPercent;

	[ReadOnly]
	public float diffractionExponent;

	[ReadOnly]
	public float distanceWeight;

	[ReadOnly]
	public float heightExponent;

	[ReadOnly]
	public float segmentHeightWeightUp;

	[ReadOnly]
	public float segmentHeightWeightDown;

	[ReadOnly]
	public float absoluteHeightWeight;

	[ReadOnly]
	public float typicalRoomHeight;

	[ReadOnly]
	public float maxSegmentLength;

	[ReadOnly]
	public NativeArray<AudioRouteData> RoutesData;

	[ReadOnly]
	public NativeArray<int> RoutePortalIndices;

	[ReadOnly]
	public NativeList<RoomPair.AudioPortalData> StaticPortalData;

	[NativeDisableParallelForRestriction]
	public NativeArray<GStruct88> RouteNodes;

	[NativeDisableParallelForRestriction]
	public NativeArray<float> LowestCosts;

	[NativeDisableParallelForRestriction]
	public NativeArray<float> LowestDistances;

	[ReadOnly]
	public float3 SourcePosition;

	[ReadOnly]
	public float3 ListenerPosition;

	public void Execute(int index)
	{
		short portalDataLength = RoutesData[index].PortalDataLength;
		ushort startIndex = RoutesData[index].StartIndex;
		ushort nodeStartIndex = RoutesData[index].NodeStartIndex;
		RouteNodes[nodeStartIndex] = new GStruct88(GStruct88.ENodeType.Emitter, SourcePosition);
		float3 entryPoint = SourcePosition;
		int num = nodeStartIndex + 1;
		for (int i = 0; i < portalDataLength; i++)
		{
			int index2 = startIndex + i;
			int index3 = RoutePortalIndices[index2];
			RoomPair.AudioPortalData portal = StaticPortalData[index3];
			float3 float5 = method_0(in portal, entryPoint, ListenerPosition);
			if (!math.all(float5 == float3.zero))
			{
				RouteNodes[num] = new GStruct88(GStruct88.ENodeType.Portal, float5, portal.closureLevel, portal.traversalMaxCost, portal.depth, portal.frontRoomWallOcclusion, portal.backRoomWallOcclusion, portal.isFrontRoomOutdoor, portal.isBackRoomOutdoor);
				num++;
				entryPoint = float5;
			}
		}
		RouteNodes[num] = new GStruct88(GStruct88.ENodeType.Listener, ListenerPosition);
		int nodesCount = num - nodeStartIndex + 1;
		method_1(nodeStartIndex, nodesCount, out var cost, out var distance);
		LowestCosts[index] = cost;
		LowestDistances[index] = distance;
	}

	public float3 method_0(in RoomPair.AudioPortalData portal, float3 entryPoint, float3 exitPoint)
	{
		float3 center = portal.center;
		float3 portalRight = portal.portalRight;
		float3 portalUp = portal.portalUp;
		float2 portalHalfSize = portal.portalHalfSize;
		float num = math.dot(entryPoint - center, portalRight);
		float num2 = math.dot(entryPoint - center, portalUp);
		float num3 = math.dot(exitPoint - center, portalRight);
		float num4 = math.dot(exitPoint - center, portalUp);
		float3 portalNormal = portal.portalNormal;
		float num5 = math.abs(math.dot(entryPoint - center, portalNormal));
		float num6 = math.abs(math.dot(exitPoint - center, portalNormal));
		float num7 = num5 + num6;
		float valueToClamp = ((num7 < 1E-06f) ? ((num + num3) * 0.5f) : ((num6 * num + num5 * num3) / num7));
		float valueToClamp2 = ((num7 < 1E-06f) ? ((num2 + num4) * 0.5f) : ((num6 * num2 + num5 * num4) / num7));
		float num8 = math.clamp(valueToClamp, 0f - portalHalfSize.x, portalHalfSize.x);
		float num9 = math.clamp(valueToClamp2, 0f - portalHalfSize.y, portalHalfSize.y);
		return center + portalRight * num8 + portalUp * num9;
	}

	public void method_1(int nodeStartIndex, int nodesCount, out float cost, out float distance)
	{
		float num = maxSegmentLength;
		float num2 = 1f / num;
		float num3 = distanceWeight;
		float y = heightExponent;
		float num4 = segmentHeightWeightUp;
		float num5 = segmentHeightWeightDown;
		float num6 = absoluteHeightWeight;
		float num7 = typicalRoomHeight;
		float y2 = diffractionExponent;
		int num8 = nodesCount - 2;
		float num9 = 0f;
		float num10 = 0f;
		float num11 = 0f;
		float num12 = math.distance(SourcePosition, ListenerPosition);
		if (num12 < 1E-06f)
		{
			num12 = 1E-06f;
		}
		for (int i = 0; i < num8; i++)
		{
			GStruct88 gStruct = RouteNodes[nodeStartIndex + i];
			GStruct88 node = RouteNodes[nodeStartIndex + i + 1];
			GStruct88 gStruct2 = RouteNodes[nodeStartIndex + i + 2];
			if (!math.all(gStruct.Position == float3.zero) && !math.all(node.Position == float3.zero) && !math.all(gStruct2.Position == float3.zero))
			{
				float3 x = node.Position - gStruct.Position;
				float3 float5 = gStruct2.Position - node.Position;
				float num13 = math.length(x);
				float num14 = math.length(float5);
				num10 += num13;
				float num15 = method_2(SourcePosition, node);
				float num16 = method_2(ListenerPosition, node);
				float num17 = (num15 + num16) * 0.5f;
				float num18 = 0f;
				if (num13 >= 0.05f && num14 >= 0.05f)
				{
					float num19 = 1f / (num13 * num14);
					float num20 = math.dot(x, float5) * num19;
					num20 = ((num20 < -1f) ? (-1f) : ((num20 > 1f) ? 1f : num20));
					num18 = math.pow((1f - num20) * 0.5f, y2);
				}
				float num21 = num13 + num14;
				float t = ((num21 > 0.05f) ? (num13 / num21) : 0.5f);
				float num22 = math.min(math.max(math.lerp(num13, num14, t) * num2, 0f), 1f);
				float num23 = num22 * (num18 + num17) + node.PortalClosureLevel;
				float num24 = (((!node.IsFrontRoomOutdoor || node.IsBackRoomOutdoor) && (node.IsFrontRoomOutdoor || !node.IsBackRoomOutdoor)) ? ((node.FrontRoomWallOcclusion + node.BackRoomWallOcclusion) * 0.5f) : (node.IsFrontRoomOutdoor ? node.BackRoomWallOcclusion : node.FrontRoomWallOcclusion));
				num23 *= num24;
				float num25 = math.length(new float2(x.x, x.z));
				float num26 = math.clamp(math.abs(x.y) / num7, 0f, 1f) * num6;
				float num27 = 0f;
				if (num25 > 0f)
				{
					float x2 = math.min(math.max(math.atan2(math.abs(x.y), num25) / (MathF.PI / 2f), 0f), 1f);
					float num28 = ((x.y > 0f) ? num4 : num5);
					num27 = math.pow(x2, y) * num28;
				}
				float num29 = num22 * (num27 + num26);
				num11 += num29;
				num9 += num23;
			}
		}
		num9 = math.min(math.max(num9, 0f), 1f);
		num11 = math.min(math.max(num11, 0f), 1f);
		GStruct88 gStruct3 = RouteNodes[nodeStartIndex + num8];
		float num30 = num10;
		float num31 = ((!math.all(gStruct3.Position == float3.zero)) ? math.distance(gStruct3.Position, ListenerPosition) : 0f);
		num10 = (distance = num30 + num31);
		float num32 = ((num10 > 0f) ? math.clamp(num12 / num10, 0f, 1f) : 0f);
		float num33 = 1f - num32;
		float num34 = ((num8 > 0) ? (num33 * num3 * (num30 / num10)) : 0f);
		cost = math.clamp(num9 + num11 + num34, 0f, 1f);
	}

	public float method_2(float3 startPoint, GStruct88 node)
	{
		float t = math.clamp(math.distance(startPoint, node.Position) / node.PortalDepth, 0f, 1f);
		return math.lerp(node.PortalTraversalMaxCost * minPortalCostPercent, node.PortalTraversalMaxCost, t);
	}
}
