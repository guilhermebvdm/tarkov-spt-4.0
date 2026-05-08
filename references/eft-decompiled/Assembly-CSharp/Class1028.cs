using System;
using Diz.Jobs;

public class Class1028 : GInterface150
{
	[NonSerialized]
	public JobYieldClass JobYieldClass;

	[NonSerialized]
	public bool Bool_0;

	public Class1028 Init(JobYieldClass jobYield, bool runSynchronously)
	{
		JobYieldClass = jobYield;
		Bool_0 = runSynchronously;
		return this;
	}

	public GInterface151 GetAwaiter()
	{
		return JobScheduler.class1029_0.Init(JobYieldClass, Bool_0);
	}
}
