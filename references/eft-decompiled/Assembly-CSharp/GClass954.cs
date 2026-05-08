using System;
using Unity.Jobs;
using UnityEngine;

public class GClass954 : CustomYieldInstruction
{
	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public JobHandle JobHandle_0;

	public override bool keepWaiting
	{
		get
		{
			if (Bool_0 && Time.frameCount >= Int_0)
			{
				JobHandle_0.Complete();
			}
			return !JobHandle_0.IsCompleted;
		}
	}

	public GClass954(JobHandle jobHandle, bool isUsingTempJobAllocator = true)
	{
		JobHandle_0 = jobHandle;
		Bool_0 = isUsingTempJobAllocator;
		Int_0 = Time.frameCount + 4;
	}
}
