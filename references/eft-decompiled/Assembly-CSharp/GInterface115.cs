using Koenigz.PerfectCulling;
using UnityEngine;

public interface GInterface115
{
	string Name { get; }

	void InitializeSamplingProvider();

	bool IsSamplingPositionActive(PerfectCullingBakingBehaviour bakingBehaviour, Vector3 pos);
}
