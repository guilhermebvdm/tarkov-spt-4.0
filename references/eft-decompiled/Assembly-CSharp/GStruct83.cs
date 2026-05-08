using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct GStruct83
{
	public struct Struct183 : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<SpherecastCommand> previousCommands;

		[ReadOnly]
		public NativeArray<RaycastHit> spherecastHits;

		[WriteOnly]
		public NativeArray<SpherecastCommand> spherecastCommands;

		public void Execute(int index)
		{
			RaycastHit raycastHit = spherecastHits[index];
			if (raycastHit.point != default(Vector3))
			{
				SpherecastCommand spherecastCommand = previousCommands[index];
				Vector3 point = raycastHit.point;
				float distance = spherecastCommand.distance - (point - spherecastCommand.origin).magnitude;
				spherecastCommands[index] = new SpherecastCommand(point, spherecastCommand.radius, spherecastCommand.direction, distance, spherecastCommand.layerMask);
			}
			else
			{
				spherecastCommands[index] = default(SpherecastCommand);
			}
		}
	}

	public struct Struct184 : IJob
	{
		public int maxHits;

		public int offset;

		[ReadOnly]
		public NativeArray<RaycastHit> semiResults;

		[WriteOnly]
		public NativeArray<RaycastHit> results;

		public void Execute()
		{
			int num = results.Length / maxHits;
			for (int i = 0; i < num; i++)
			{
				results[i * maxHits + offset] = semiResults[i];
			}
		}
	}

	public struct Struct185 : IJob
	{
		public int maxHits;

		public NativeArray<SpherecastCommand> spherecastCommands;

		public NativeArray<RaycastHit> spherecastHits;

		public void Execute()
		{
			int num = spherecastHits.Length / maxHits;
			for (int i = 1; i < num; i++)
			{
				for (int j = i * maxHits; j < (i + 1) * maxHits; j++)
				{
					RaycastHit value = spherecastHits[j];
					if (value.point == default(Vector3))
					{
						break;
					}
					SpherecastCommand spherecastCommand = spherecastCommands[i];
					float distance = (spherecastCommand.origin - value.point).magnitude - spherecastCommand.radius;
					value.distance = distance;
					spherecastHits[j] = value;
				}
			}
		}
	}

	[NonSerialized]
	public NativeArray<RaycastHit> NativeArray_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public NativeArray<RaycastHit>[] NativeArray_1;

	[NonSerialized]
	public NativeArray<SpherecastCommand>[] NativeArray_2;

	public int MaxHits => Int_0;

	public GStruct83(NativeArray<SpherecastCommand> commands, NativeArray<RaycastHit> results, int maxHits)
	{
		if (maxHits <= 0)
		{
			throw new ArgumentException("maxHits should be greater than zero");
		}
		if (results.Length < commands.Length * maxHits)
		{
			throw new ArgumentException("Results array length does not match maxHits count");
		}
		if (maxHits <= 1)
		{
			Debug.LogWarning("Using SpherecastAllCommand with maxHits = 1 will cause unnecessary overhead comparing to SpherecastCommand, please use that instead");
		}
		NativeArray_0 = results;
		Int_0 = maxHits;
		NativeArray_1 = new NativeArray<RaycastHit>[maxHits];
		for (int i = 0; i < maxHits; i++)
		{
			NativeArray_1[i] = new NativeArray<RaycastHit>(commands.Length, Allocator.TempJob);
		}
		NativeArray_2 = new NativeArray<SpherecastCommand>[maxHits];
		NativeArray_2[0] = commands;
		for (int j = 1; j < maxHits; j++)
		{
			NativeArray_2[j] = new NativeArray<SpherecastCommand>(commands.Length, Allocator.TempJob);
		}
	}

	public JobHandle Schedule(JobHandle dependency)
	{
		for (int i = 0; i < Int_0; i++)
		{
			dependency = SpherecastCommand.ScheduleBatch(NativeArray_2[i], NativeArray_1[i], 16, dependency);
			if (i < Int_0 - 1)
			{
				dependency = IJobParallelForExtensions.Schedule(new Struct183
				{
					previousCommands = NativeArray_2[i],
					spherecastHits = NativeArray_1[i],
					spherecastCommands = NativeArray_2[i + 1]
				}, NativeArray_2[i].Length, 32, dependency);
			}
			dependency = new Struct184
			{
				maxHits = Int_0,
				semiResults = NativeArray_1[i],
				offset = i,
				results = NativeArray_0
			}.Schedule(dependency);
		}
		dependency = new Struct185
		{
			spherecastHits = NativeArray_0,
			maxHits = Int_0,
			spherecastCommands = NativeArray_2[0]
		}.Schedule(dependency);
		return dependency;
	}

	public void Dispose()
	{
		for (int i = 0; i < Int_0; i++)
		{
			NativeArray_1[i].Dispose();
		}
		for (int j = 1; j < Int_0; j++)
		{
			NativeArray_2[j].Dispose();
		}
		Int_0 = 0;
	}
}
