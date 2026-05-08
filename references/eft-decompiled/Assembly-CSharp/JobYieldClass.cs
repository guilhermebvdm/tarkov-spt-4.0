using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Diz.Jobs;

public class JobYieldClass
{
	public struct GStruct157(JobYieldClass jobYield) : IDisposable
	{
		[NonSerialized]
		public JobYieldClass JobYieldClass = jobYield;

		public void Dispose()
		{
			JobYieldClass.End();
		}
	}

	[NonSerialized]
	public GDelegate62 Gdelegate62_0;

	[NonSerialized]
	[CompilerGenerated]
	public EJobPriority EjobPriority_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	[NonSerialized]
	[CompilerGenerated]
	public string String_0;

	[NonSerialized]
	[CompilerGenerated]
	public string String_1;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public Stack<Action> Stack_0 = new Stack<Action>();

	public EJobPriority Priority
	{
		[CompilerGenerated]
		get
		{
			return EjobPriority_0;
		}
		[CompilerGenerated]
		set
		{
			EjobPriority_0 = value;
		}
	}

	public bool IsInProgress
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public bool IsCanceled
	{
		[CompilerGenerated]
		get
		{
			return Bool_1;
		}
		[CompilerGenerated]
		set
		{
			Bool_1 = value;
		}
	}

	public string Name
	{
		[CompilerGenerated]
		get
		{
			return String_0;
		}
		[CompilerGenerated]
		set
		{
			String_0 = value;
		}
	}

	public string CurrentStageName
	{
		[CompilerGenerated]
		get
		{
			return String_1;
		}
		[CompilerGenerated]
		set
		{
			String_1 = value;
		}
	}

	public JobYieldClass(string name = null)
	{
		Name = name;
	}

	public JobYieldClass WithPriority(EJobPriority priority)
	{
		Priority = priority;
		Gdelegate62_0 = JobPriorityClass.GetYieldDelegate(priority);
		return this;
	}

	public async Task Yield(string jobStageName = null)
	{
		CurrentStageName = jobStageName;
		method_0();
		await Gdelegate62_0(this);
		method_0();
	}

	public GStruct157 Begin()
	{
		method_2();
		method_0();
		IsInProgress = true;
		return new GStruct157(this);
	}

	public void End()
	{
		IsInProgress = false;
		method_2();
	}

	public bool ForceFinish()
	{
		if (IsInProgress)
		{
			return JobScheduler.ForceExecuteContinuations(this);
		}
		return false;
	}

	public void Cancel(bool once = false)
	{
		IsCanceled = true;
		Bool_2 = once;
	}

	public void Reset()
	{
		IsCanceled = false;
		Bool_2 = false;
		IsInProgress = false;
		CurrentStageName = null;
	}

	public void method_0()
	{
		if (IsCanceled)
		{
			if (Bool_2)
			{
				IsCanceled = false;
			}
			method_1();
			throw new GException15();
		}
	}

	public void AddCancelAction(Action action)
	{
		Stack_0.Push(action);
	}

	public void method_1()
	{
		while (Stack_0.Count > 0)
		{
			Stack_0.Pop()();
		}
	}

	public void method_2()
	{
		Stack_0.Clear();
	}
}
