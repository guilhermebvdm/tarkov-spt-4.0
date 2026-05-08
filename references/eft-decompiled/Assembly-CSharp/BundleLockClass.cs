using System;
using System.Runtime.CompilerServices;

public class BundleLockClass : IBundleLock
{
	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	[NonSerialized]
	public int Int_1;

	public int MaxConcurrentOperations
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
		[CompilerGenerated]
		set
		{
			Int_0 = value;
		}
	}

	public bool IsLocked => Int_1 >= MaxConcurrentOperations;

	public BundleLockClass(int maxConcurrentOperations)
	{
		MaxConcurrentOperations = maxConcurrentOperations;
	}

	public void Lock()
	{
		Int_1++;
	}

	public void Unlock()
	{
		Int_1--;
	}
}
