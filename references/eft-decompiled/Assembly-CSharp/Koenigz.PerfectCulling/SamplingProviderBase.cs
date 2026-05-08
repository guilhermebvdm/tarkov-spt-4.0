using UnityEngine;

namespace Koenigz.PerfectCulling;

public abstract class SamplingProviderBase : MonoBehaviour, GInterface115
{
	private GInterface111 ginterface111_0;

	public abstract string Name { get; }

	public virtual void OnEnable()
	{
		if (ginterface111_0 == null)
		{
			ginterface111_0 = GetComponent<GInterface111>();
		}
		ginterface111_0.AddSamplingProvider(this);
	}

	public virtual void OnDisable()
	{
		if (ginterface111_0 == null)
		{
			ginterface111_0 = GetComponent<GInterface111>();
		}
		ginterface111_0.RemoveSamplingProvider(this);
	}

	public abstract void InitializeSamplingProvider();

	public abstract bool IsSamplingPositionActive(PerfectCullingBakingBehaviour bakingBehaviour, Vector3 pos);

	public SamplingProviderBase()
	{
	}
}
