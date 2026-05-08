using System;
using System.Runtime.CompilerServices;
using Diz.Jobs;
using UnityEngine;

public class Class1029 : GInterface151, INotifyCompletion
{
	[NonSerialized]
	public JobYieldClass JobYieldClass;

	[NonSerialized]
	public bool Bool_0;

	public bool IsCompleted => Bool_0;

	public Class1029 Init(JobYieldClass jobYield, bool runSynchronously)
	{
		JobYieldClass = jobYield;
		Bool_0 = runSynchronously;
		return this;
	}

	public void OnCompleted(Action continuation)
	{
		if (Application.isPlaying)
		{
			JobScheduler.smethod_1(continuation, JobYieldClass);
		}
	}

	public void GetResult()
	{
	}
}
