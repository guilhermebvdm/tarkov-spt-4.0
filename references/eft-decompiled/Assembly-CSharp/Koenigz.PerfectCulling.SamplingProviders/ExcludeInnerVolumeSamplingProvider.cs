using System.Collections.Generic;
using Koenigz.PerfectCulling.EFT;
using UnityEngine;

namespace Koenigz.PerfectCulling.SamplingProviders;

[RequireComponent(typeof(PerfectCullingBakingBehaviour))]
[DisallowMultipleComponent]
[ExecuteAlways]
public class ExcludeInnerVolumeSamplingProvider : SamplingProviderBase
{
	[SerializeField]
	private PerfectCullingCrossSceneVolume[] _innerCrossVolumes;

	public override string Name => "Inner Volume";

	public override void InitializeSamplingProvider()
	{
	}

	public void RefreshInnerVolumes()
	{
		List<PerfectCullingCrossSceneVolume> list = new List<PerfectCullingCrossSceneVolume>();
		PerfectCullingCrossSceneVolume component = GetComponent<PerfectCullingCrossSceneVolume>();
		PerfectCullingCrossSceneVolume[] componentsInChildren = base.transform.GetComponentsInChildren<PerfectCullingCrossSceneVolume>();
		foreach (PerfectCullingCrossSceneVolume perfectCullingCrossSceneVolume in componentsInChildren)
		{
			if (!(perfectCullingCrossSceneVolume == component))
			{
				list.Add(perfectCullingCrossSceneVolume);
			}
		}
		_innerCrossVolumes = list.ToArray();
	}

	public override bool IsSamplingPositionActive(PerfectCullingBakingBehaviour bakingBehaviour, Vector3 pos)
	{
		return true;
	}

	public bool IsSamplingPositionActiveMT(PerfectCullingBakingBehaviour bakingBehaviour, Vector3 pos)
	{
		PerfectCullingCrossSceneVolume[] innerCrossVolumes = _innerCrossVolumes;
		int num = 0;
		while (true)
		{
			if (num < innerCrossVolumes.Length)
			{
				if (innerCrossVolumes[num].runtimeVolumeBounds.Contains(pos))
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
