using UnityEngine;

public class GClass888
{
	public BetterSource[] AudioSources;

	public GClass888()
	{
	}

	public GClass888(BetterSource[] audioSources)
	{
		AudioSources = audioSources;
	}

	public void Pose(Vector3 worldPosition)
	{
		BetterSource[] audioSources = AudioSources;
		foreach (BetterSource betterSource in audioSources)
		{
			if (betterSource != null)
			{
				betterSource.transform.position = worldPosition;
			}
		}
	}

	public virtual void Update()
	{
	}

	public double method_0(double startTime)
	{
		return startTime;
	}

	public double method_1(float clipTime, double calculatedStart)
	{
		if (clipTime != 0f)
		{
			return calculatedStart + (double)clipTime;
		}
		return 0.0;
	}
}
