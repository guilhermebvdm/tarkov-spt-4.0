using System;
using Koenigz.PerfectCulling;
using UnityEngine;

public class GClass1230 : GInterface115
{
	[NonSerialized]
	public PerfectCullingExcludeVolume[] PerfectCullingExcludeVolume_0;

	public static string DefaultActiveSamplingProviderName => "DefaultActiveSamplingProvider";

	public string Name => DefaultActiveSamplingProviderName;

	public void InitializeSamplingProvider()
	{
		PerfectCullingExcludeVolume_0 = UnityEngine.Object.FindObjectsOfType<PerfectCullingExcludeVolume>();
	}

	public bool IsSamplingPositionActive(PerfectCullingBakingBehaviour bakingBehaviour, Vector3 pos)
	{
		PerfectCullingExcludeVolume[] perfectCullingExcludeVolume_ = PerfectCullingExcludeVolume_0;
		int num = 0;
		while (true)
		{
			if (num < perfectCullingExcludeVolume_.Length)
			{
				if (perfectCullingExcludeVolume_[num].IsPositionActive(bakingBehaviour, pos))
				{
					break;
				}
				num++;
				continue;
			}
			return true;
		}
		return false;
	}
}
