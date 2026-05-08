using UnityEngine;

public class LevelPhysicsSettings : MonoBehaviour
{
	private const int int_0 = 16;

	[SerializeField]
	private Bounds _mbpBounds;

	[Range(1f, 16f)]
	[SerializeField]
	private int _mbpSubdivisions = 1;

	private static LevelPhysicsSettings levelPhysicsSettings_0;

	public void Awake()
	{
		if (levelPhysicsSettings_0 != null)
		{
			Debug.LogError("multiple LevelPhysicsSetup on one level", this);
			return;
		}
		method_0();
		levelPhysicsSettings_0 = this;
	}

	public void OnDestroy()
	{
		if (this == levelPhysicsSettings_0)
		{
			levelPhysicsSettings_0 = null;
		}
	}

	public Bounds GetGlobalBounds()
	{
		Bounds mbpBounds = _mbpBounds;
		mbpBounds.center = base.transform.TransformPoint(_mbpBounds.center);
		return mbpBounds;
	}

	public void method_0()
	{
		Bounds bounds = GetGlobalBounds();
		int num = _mbpSubdivisions;
		if (BackendConfigAbstractClass.Config.Physics.OverrideMBPSettings)
		{
			bounds = BackendConfigAbstractClass.Config.Physics.OverrideMBPBounds;
			num = BackendConfigAbstractClass.Config.Physics.OverrideMBPSubdivisions;
		}
		Physics.RebuildBroadphaseRegions(bounds, num);
		Debug.LogFormat("physics mbp settings applied: {0} {1}", bounds, num);
	}
}
