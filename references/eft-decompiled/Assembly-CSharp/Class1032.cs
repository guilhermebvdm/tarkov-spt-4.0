using System;

public class Class1032 : Interface5
{
	[NonSerialized]
	public JobYieldClass JobYieldClass;

	public bool JobFinished;

	public bool HasExecution;

	public void Init(JobYieldClass jobYield)
	{
		JobYieldClass = jobYield;
		JobFinished = false;
	}

	public bool CanExecute(Struct263 continuation)
	{
		if (JobFinished)
		{
			return false;
		}
		if (continuation.jobYield == JobYieldClass)
		{
			return !continuation.wasExecuted;
		}
		return false;
	}

	public void Execute(ref Struct263 continuation)
	{
		continuation.Execute();
		HasExecution = true;
		if (!JobYieldClass.IsInProgress)
		{
			JobFinished = true;
		}
	}
}
