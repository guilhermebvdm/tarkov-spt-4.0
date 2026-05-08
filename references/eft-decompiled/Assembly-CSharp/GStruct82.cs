using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct GStruct82
{
	public struct Struct180 : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<RaycastCommand> PreviousCommands;

		[ReadOnly]
		public NativeArray<RaycastHit> RaycastHits;

		[WriteOnly]
		public NativeArray<RaycastCommand> RaycastCommands;

		[ReadOnly]
		public float MINStep;

		public void Execute(int index)
		{
			RaycastHit raycastHit = RaycastHits[index];
			if (raycastHit.point != default(Vector3))
			{
				RaycastCommand raycastCommand = PreviousCommands[index];
				Vector3 vector = raycastHit.point + raycastCommand.direction.normalized * MINStep;
				float distance = raycastCommand.distance - (vector - raycastCommand.from).magnitude;
				RaycastCommands[index] = new RaycastCommand(vector, raycastCommand.direction, distance, raycastCommand.layerMask);
			}
			else
			{
				RaycastCommands[index] = default(RaycastCommand);
			}
		}
	}

	public struct Struct181 : IJob
	{
		public int MAXHits;

		public int Offset;

		[ReadOnly]
		public NativeArray<RaycastHit> SemiResults;

		[WriteOnly]
		public NativeArray<RaycastHit> Results;

		public void Execute()
		{
			int num = Results.Length / MAXHits;
			for (int i = 0; i < num; i++)
			{
				Results[i * MAXHits + Offset] = SemiResults[i];
			}
		}
	}

	public struct Struct182 : IJob
	{
		public int MAXHits;

		public NativeArray<RaycastCommand> RaycastCommands;

		public NativeArray<RaycastHit> RaycastHits;

		public void Execute()
		{
			int num = RaycastHits.Length / MAXHits;
			for (int i = 1; i < num; i++)
			{
				for (int j = i * MAXHits; j < (i + 1) * MAXHits; j++)
				{
					RaycastHit value = RaycastHits[j];
					if (value.point == default(Vector3))
					{
						break;
					}
					float magnitude = (RaycastCommands[i].from - value.point).magnitude;
					value.distance = magnitude;
					RaycastHits[j] = value;
				}
			}
		}
	}

	[NonSerialized]
	public NativeArray<RaycastHit> NativeArray_0;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	public readonly float MINStep;

	[NonSerialized]
	public NativeArray<RaycastHit>[] NativeArray_1;

	[NonSerialized]
	public NativeArray<RaycastCommand>[] NativeArray_2;

	public int MaxHits
	{
		[CompilerGenerated]
		readonly get
		{
			return Int_0;
		}
		[CompilerGenerated]
		set
		{
			Int_0 = value;
		}
	}

	public GStruct82(NativeArray<RaycastCommand> commands, NativeArray<RaycastHit> results, int maxHits, float minStep = 0.0001f)
	{
		if (maxHits <= 0)
		{
			throw new ArgumentException("maxHits should be greater than zero");
		}
		if (results.Length < commands.Length * maxHits)
		{
			throw new ArgumentException("Results array length does not match maxHits count");
		}
		if (minStep < 0f)
		{
			throw new ArgumentException("minStep should be more or equal to zero");
		}
		if (maxHits <= 1)
		{
			Debug.LogWarning("Using RaycastAllCommand with maxHits = 1 will cause unnecessary overhead comparing to RaycastCommand, please use that instead");
		}
		NativeArray_0 = results;
		MaxHits = maxHits;
		MINStep = minStep;
		NativeArray_1 = new NativeArray<RaycastHit>[maxHits];
		for (int i = 0; i < maxHits; i++)
		{
			NativeArray_1[i] = new NativeArray<RaycastHit>(commands.Length, Allocator.TempJob);
		}
		NativeArray_2 = new NativeArray<RaycastCommand>[maxHits];
		NativeArray_2[0] = commands;
		for (int j = 1; j < maxHits; j++)
		{
			NativeArray_2[j] = new NativeArray<RaycastCommand>(commands.Length, Allocator.TempJob);
		}
	}

	public JobHandle Schedule(JobHandle dependency)
	{
		for (int i = 0; i < MaxHits; i++)
		{
			dependency = RaycastCommand.ScheduleBatch(NativeArray_2[i], NativeArray_1[i], 16, dependency);
			if (i < MaxHits - 1)
			{
				dependency = IJobParallelForExtensions.Schedule(new Struct180
				{
					PreviousCommands = NativeArray_2[i],
					RaycastHits = NativeArray_1[i],
					RaycastCommands = NativeArray_2[i + 1],
					MINStep = MINStep
				}, NativeArray_2[i].Length, 32, dependency);
			}
			dependency = new Struct181
			{
				MAXHits = MaxHits,
				SemiResults = NativeArray_1[i],
				Offset = i,
				Results = NativeArray_0
			}.Schedule(dependency);
		}
		dependency = new Struct182
		{
			RaycastHits = NativeArray_0,
			MAXHits = MaxHits,
			RaycastCommands = NativeArray_2[0]
		}.Schedule(dependency);
		return dependency;
	}

	public void Dispose()
	{
		for (int i = 0; i < MaxHits; i++)
		{
			NativeArray_1[i].Dispose();
		}
		for (int j = 1; j < MaxHits; j++)
		{
			NativeArray_2[j].Dispose();
		}
		MaxHits = 0;
	}
}
