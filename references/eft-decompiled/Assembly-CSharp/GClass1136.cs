using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class GClass1136
{
	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public NativeArray<RaycastCommand> NativeArray_0;

	[NonSerialized]
	public NativeArray<RaycastHit> NativeArray_1;

	[NonSerialized]
	public GStruct82 Gstruct82_0;

	[NonSerialized]
	public JobHandle JobHandle_0;

	[NonSerialized]
	[CompilerGenerated]
	public Dictionary<int, RaycastHit[]> Dictionary_0 = new Dictionary<int, RaycastHit[]>();

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public QueryParameters QueryParameters_0;

	public Dictionary<int, RaycastHit[]> HitsData
	{
		[CompilerGenerated]
		get
		{
			return Dictionary_0;
		}
	}

	public bool IsCompleted
	{
		get
		{
			if (JobHandle_0.IsCompleted)
			{
				return Bool_0;
			}
			return false;
		}
	}

	public GClass1136(int maxBufferSize, int maxHits = 1, QueryParameters queryParameters = default(QueryParameters))
	{
		Bool_0 = true;
		Int_0 = maxBufferSize;
		Int_1 = maxHits;
		QueryParameters_0 = queryParameters;
		NativeArray_0 = new NativeArray<RaycastCommand>(maxBufferSize, Allocator.Persistent);
		NativeArray_1 = new NativeArray<RaycastHit>(maxBufferSize * maxHits, Allocator.Persistent);
		Bool_1 = maxHits > 1;
		for (int i = 0; i < Int_0; i++)
		{
			HitsData[i] = new RaycastHit[Int_1];
		}
	}

	public void AddRaycastCommand(Vector3 startPoint, Vector3 direction, float distance, int index)
	{
		if (Bool_0)
		{
			NativeArray_0[index] = new RaycastCommand(startPoint, direction, QueryParameters_0, distance);
		}
	}

	public void Execute()
	{
		if (NativeArray_0.IsCreated && NativeArray_1.IsCreated)
		{
			Bool_0 = false;
			if (Bool_1)
			{
				Gstruct82_0 = new GStruct82(NativeArray_0, NativeArray_1, Int_1, 0.05f);
				JobHandle_0 = Gstruct82_0.Schedule(JobHandle_0);
			}
			else
			{
				JobHandle_0 = RaycastCommand.ScheduleBatch(NativeArray_0, NativeArray_1, 1, JobHandle_0);
			}
		}
		else
		{
			Debug.LogError("can't schedule");
		}
	}

	public void StoreResults()
	{
		JobHandle_0.Complete();
		Gstruct82_0.Dispose();
		if (!NativeArray_1.IsCreated)
		{
			return;
		}
		for (int i = 0; i < Int_0; i++)
		{
			for (int j = 0; j < Int_1; j++)
			{
				RaycastHit raycastHit = NativeArray_1[i * Int_1 + j];
				HitsData[i][j] = raycastHit;
				NativeArray_1[i * Int_1 + j] = default(RaycastHit);
			}
		}
		Bool_0 = true;
	}

	public void Dispose()
	{
		JobHandle_0.Complete();
		if (NativeArray_1.IsCreated && NativeArray_0.IsCreated)
		{
			NativeArray_1.Dispose();
			NativeArray_0.Dispose();
			Gstruct82_0.Dispose();
			Bool_0 = true;
		}
	}

	public void ClearHitData()
	{
		HitsData.Clear();
	}

	~GClass1136()
	{
		ClearHitData();
		Dispose();
	}
}
