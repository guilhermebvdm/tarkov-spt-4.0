using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

public struct Struct263
{
	public Action action;

	public JobYieldClass jobYield;

	public string jobStageName;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	public bool wasExecuted
	{
		[CompilerGenerated]
		readonly get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public Struct263(Action action, [CanBeNull] JobYieldClass jobYield)
	{
		this.action = action;
		this.jobYield = jobYield;
		wasExecuted = false;
		jobStageName = jobYield?.CurrentStageName;
	}

	public void Execute()
	{
		wasExecuted = true;
		action();
	}
}
