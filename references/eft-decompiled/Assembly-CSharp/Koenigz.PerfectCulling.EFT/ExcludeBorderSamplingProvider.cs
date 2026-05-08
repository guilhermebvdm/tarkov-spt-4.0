using System;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

[RequireComponent(typeof(PerfectCullingBakingBehaviour))]
[DisallowMultipleComponent]
[ExecuteAlways]
public class ExcludeBorderSamplingProvider : SamplingProviderBase
{
	[SerializeField]
	private LevelBorder _border;

	[NonSerialized]
	private GClass916 gclass916_0;

	public override string Name => "Border";

	public override void InitializeSamplingProvider()
	{
		if (_border == null)
		{
			throw new NullReferenceException("Border is not set");
		}
		gclass916_0 = _border.CreateConcaveBorder();
	}

	public override bool IsSamplingPositionActive(PerfectCullingBakingBehaviour bakingBehaviour, Vector3 pos)
	{
		return gclass916_0.Contains(pos.x, pos.z);
	}
}
