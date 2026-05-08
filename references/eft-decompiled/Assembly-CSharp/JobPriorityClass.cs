using System;
using System.Runtime.CompilerServices;
using Diz.Jobs;

public abstract class JobPriorityClass
{
	[Serializable]
	[CompilerGenerated]
	public class Class1033
	{
		public static readonly Class1033 class1033_0 = new Class1033();

		public static GDelegate62 gdelegate62_0;

		public static GDelegate62 gdelegate62_1;

		public static GDelegate62 gdelegate62_2;

		public GInterface150 method_0(JobYieldClass jobYield)
		{
			return JobScheduler.Yield(EJobPriority.General, jobYield);
		}

		public GInterface150 method_1(JobYieldClass jobYield)
		{
			return JobScheduler.Yield(EJobPriority.Low, jobYield);
		}

		public GInterface150 method_2(JobYieldClass jobYield)
		{
			return JobScheduler.Yield(EJobPriority.Immediate, jobYield);
		}
	}

	public static GDelegate62 General => (JobYieldClass jobYield) => JobScheduler.Yield(EJobPriority.General, jobYield);

	public static GDelegate62 Low => (JobYieldClass jobYield) => JobScheduler.Yield(EJobPriority.Low, jobYield);

	public static GDelegate62 Immediate => (JobYieldClass jobYield) => JobScheduler.Yield(EJobPriority.Immediate, jobYield);

	public static EJobPriority Priority(this GDelegate62 priorityDelegate)
	{
		if (priorityDelegate == General)
		{
			return EJobPriority.General;
		}
		if (priorityDelegate == Low)
		{
			return EJobPriority.Low;
		}
		if (priorityDelegate != Immediate)
		{
			throw new NotImplementedException();
		}
		return EJobPriority.Immediate;
	}

	public static GDelegate62 GetYieldDelegate(EJobPriority priority)
	{
		return priority switch
		{
			EJobPriority.Low => Low, 
			EJobPriority.General => General, 
			EJobPriority.Immediate => Immediate, 
			_ => throw new ArgumentOutOfRangeException("priority", priority, null), 
		};
	}
}
