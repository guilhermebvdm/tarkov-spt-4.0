using System;
using UnityEngine;

public class GClass1493
{
	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public int Int_1;

	public bool IsThresholdReached => Int_1 >= Int_0;

	public void SetTargetCount(int targetCount)
	{
		if (targetCount <= 0)
		{
			Debug.LogError("Target count must be positive! Setting to 1.");
		}
		Int_0 = Mathf.Max(1, targetCount);
	}

	public bool TryReachThreshold()
	{
		Int_1++;
		if (Int_1 >= Int_0)
		{
			ResetCounter();
			return true;
		}
		return false;
	}

	public void ResetCounter()
	{
		Int_1 = 0;
	}
}
