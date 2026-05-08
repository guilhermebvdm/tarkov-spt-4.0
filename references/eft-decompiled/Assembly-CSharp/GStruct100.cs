using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct GStruct100 : IJobParallelFor
{
	[NonSerialized]
	public const float Float_0 = 0.0001f;

	[NonSerialized]
	public const float Float_1 = 0.0001f;

	[NonSerialized]
	public const float Float_2 = 0.001f;

	[NonSerialized]
	public const float Float_3 = 0.1f;

	[NonSerialized]
	public const int Int_0 = -1;

	[ReadOnly]
	public float minEnergyAtMaxDistance;

	[ReadOnly]
	public float minValidReflectionEnergy;

	[ReadOnly]
	public NativeArray<RaycastHit> previousStepSphereHits;

	public NativeArray<GStruct105> rayStates;

	[ReadOnly]
	public float3 listenerPosition;

	[ReadOnly]
	public int maxReflections;

	[ReadOnly]
	public float rayOffset;

	[ReadOnly]
	public QueryParameters queryParams;

	[ReadOnly]
	public int maxCommandsInBuffer;

	[ReadOnly]
	public float reflectionEnergyLossFactor;

	[ReadOnly]
	public float spherecastRadius;

	[ReadOnly]
	public float listenerCheckSpherecastRadius;

	[NativeDisableParallelForRestriction]
	public NativeArray<SpherecastCommand> nextSpherecastCommands;

	[WriteOnly]
	public NativeArray<float> minPathLengths;

	[WriteOnly]
	public NativeArray<float> pathEnergies;

	[WriteOnly]
	public NativeArray<byte> pathEndStatuses;

	[NativeDisableParallelForRestriction]
	public NativeList<GStruct101>.ParallelWriter pathPointInfosWriter;

	[ReadOnly]
	public NativeArray<int2> commandIndices;

	public void Execute(int index)
	{
		GStruct105 gStruct = rayStates[index];
		if (!gStruct.isActive)
		{
			minPathLengths[index] = float.MaxValue;
			pathEnergies[index] = 0f;
			pathEndStatuses[index] = 4;
			GStruct105 value = rayStates[index];
			value.isActive = false;
			value.raySegmentCommandIndex = -1;
			value.listenerCheckSphereCommandIndex = -1;
			rayStates[index] = value;
			return;
		}
		int2 obj = commandIndices[index];
		int x = obj.x;
		int y = obj.y;
		float num = float.MaxValue;
		float num2 = 0f;
		int num3 = -1;
		int num4 = -1;
		byte b = 0;
		bool flag = false;
		byte b2 = 0;
		float num5 = float.MaxValue;
		float num6 = 0f;
		bool num7 = y != -1;
		bool flag2 = false;
		bool flag3 = false;
		RaycastHit raycastHit = default(RaycastHit);
		bool flag4 = false;
		Vector3 vector = gStruct.currentOrigin;
		Vector3 vector2 = gStruct.currentDirection;
		int reflectionCount = gStruct.reflectionCount;
		float distanceTraveled = gStruct.distanceTraveled;
		float num8 = gStruct.remainingDistance;
		float num9 = gStruct.currentEnergy;
		if (num7)
		{
			if (y >= 0 && y < previousStepSphereHits.Length)
			{
				flag = previousStepSphereHits[y].colliderInstanceID == 0;
			}
			if (flag)
			{
				float num10 = math.distance(gStruct.currentOrigin, listenerPosition);
				float num11 = gStruct.distanceTraveled + num10;
				float num12 = GClass1169.CalculateDistanceAttenuation(num10, gStruct.remainingDistance + num10, minEnergyAtMaxDistance);
				num6 = gStruct.currentEnergy * num12;
				if (num6 >= minValidReflectionEnergy)
				{
					b2 = 1;
					num5 = num11;
				}
				else
				{
					b2 = 2;
				}
			}
			else
			{
				b2 = 2;
			}
		}
		if (x != -1)
		{
			if (x >= 0 && x < previousStepSphereHits.Length)
			{
				raycastHit = previousStepSphereHits[x];
				flag2 = raycastHit.colliderInstanceID != 0 && raycastHit.distance > 0.0001f;
			}
			if (flag2)
			{
				float distance = raycastHit.distance;
				float num13 = GClass1169.CalculateDistanceAttenuation(distance, gStruct.remainingDistance, minEnergyAtMaxDistance);
				Vector3 point = raycastHit.point;
				float num14 = gStruct.distanceTraveled + distance;
				float num15 = gStruct.remainingDistance - distance;
				float num16 = gStruct.currentEnergy * num13;
				if (gStruct.reflectionCount < maxReflections && num15 > rayOffset && num16 >= minValidReflectionEnergy * 0.1f)
				{
					Vector3 normal = raycastHit.normal;
					if (math.lengthsq(normal) > 0.0001f)
					{
						Vector3 normalized = Vector3.Reflect(gStruct.currentDirection, normal).normalized;
						if (normalized.sqrMagnitude > 0.0001f && Vector3.Dot(normalized, normal) >= 0.001f)
						{
							vector = point + normalized * rayOffset;
							vector2 = normalized;
							reflectionCount = gStruct.reflectionCount + 1;
							distanceTraveled = num14;
							num8 = num15;
							num9 = num16 * reflectionEnergyLossFactor;
							if (num9 >= minValidReflectionEnergy * 0.1f)
							{
								flag3 = true;
							}
						}
					}
				}
			}
		}
		if (b2 == 1)
		{
			b = 1;
			num = num5;
			num2 = num6;
			pathPointInfosWriter.AddNoResize(new GStruct101
			{
				rayIndex = index,
				point = gStruct.currentOrigin
			});
		}
		else if (!(flag2 && flag3))
		{
			b = (byte)((b2 == 2) ? 2 : ((!flag2) ? 4 : 3));
		}
		else
		{
			b = 0;
			flag4 = true;
			pathPointInfosWriter.AddNoResize(new GStruct101
			{
				rayIndex = index,
				point = raycastHit.point
			});
			float num17 = math.distance(vector, listenerPosition);
			num4 = -1;
			if (num17 > rayOffset && num17 <= num8)
			{
				float3 x2 = listenerPosition - (float3)vector;
				if (math.lengthsq(x2) > 0.0001f)
				{
					float3 float5 = math.normalize(x2);
					float num18 = num17 - rayOffset;
					if (num18 > 0.0001f)
					{
						int num19 = index * 2 + 1;
						if (num19 < maxCommandsInBuffer)
						{
							num4 = num19;
							nextSpherecastCommands[num4] = new SpherecastCommand(vector, listenerCheckSpherecastRadius, float5, queryParams, num18);
						}
					}
				}
			}
			int num20 = index * 2;
			if (num20 < maxCommandsInBuffer)
			{
				num3 = num20;
				nextSpherecastCommands[num3] = new SpherecastCommand(vector, spherecastRadius, vector2, queryParams, num8);
			}
			else
			{
				Debug.LogWarning($"[ProcessHitsJob Ray {index}] Buffer full for segment command. Stopping ray.");
				b = 4;
				flag4 = false;
				num3 = -1;
				num4 = -1;
			}
		}
		minPathLengths[index] = ((b == 1) ? num : float.MaxValue);
		pathEnergies[index] = ((b == 1) ? num2 : 0f);
		pathEndStatuses[index] = b;
		GStruct105 value2 = rayStates[index];
		if (flag4)
		{
			value2.isActive = true;
			value2.currentOrigin = vector;
			value2.currentDirection = vector2;
			value2.distanceTraveled = distanceTraveled;
			value2.remainingDistance = num8;
			value2.reflectionCount = reflectionCount;
			value2.currentEnergy = num9;
			value2.raySegmentCommandIndex = num3;
			value2.listenerCheckSphereCommandIndex = num4;
		}
		else
		{
			value2.isActive = false;
			value2.raySegmentCommandIndex = -1;
			value2.listenerCheckSphereCommandIndex = -1;
		}
		rayStates[index] = value2;
	}
}
