using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiFlare;

[ExecuteInEditMode]
[HelpURL("https://eodproject.atlassian.net/wiki/spaces/FRONT/pages/1254981646/Unity.#Flare")]
public class FlareLight : MonoBehaviour
{
	private bool bool_0;

	[Tooltip("Общий множитель размера всех флар для этого FlareLight")]
	[FormerlySerializedAs("_scale")]
	[FormerlySerializedAs("Scale")]
	[SerializeField]
	private float _totalScale = 1f;

	[Tooltip("Общий множитель прозрачности всех флар для этого FlareLight")]
	[FormerlySerializedAs("_alpha")]
	[FormerlySerializedAs("Alpha")]
	[SerializeField]
	[Range(0f, 1f)]
	private float _totalAlpha = 1f;

	[SerializeField]
	[FormerlySerializedAs("Flares")]
	private Flare[] _flares;

	public static string ScalePropertyName => "_totalScale";

	public static string AlphaPropertyName => "_totalAlpha";

	public static string FlaresPropertyName => "_flares";

	public IReadOnlyList<Flare> Flares => _flares;

	public float Alpha => _totalAlpha;

	public float Scale => _totalScale;

	public bool Enabled => base.isActiveAndEnabled;

	public void OnDestroy()
	{
		if (GClass1023.Instance.IsInitialized)
		{
			GClass1023.Instance.RemoveLight(this);
			bool_0 = true;
		}
	}

	public void SetAlpha(float value)
	{
		if (GClass1023.Instance.IsInitialized)
		{
			_totalAlpha = value;
			GClass1023.Instance.AccessLightData(this).Alpha = _totalAlpha;
		}
	}

	public void SetScale(float value)
	{
		if (GClass1023.Instance.IsInitialized)
		{
			_totalScale = value;
			GClass1023.Instance.AccessLightData(this).Scale = _totalScale;
		}
	}

	public void OnEnable()
	{
		if (GClass1023.Instance.IsInitialized)
		{
			GClass1023.Instance.AddLight(this);
			GClass1023.Instance.AccessLightData(this).Enabled = true;
		}
	}

	public void OnDisable()
	{
		if (GClass1023.Instance.IsInitialized && !bool_0)
		{
			GClass1023.Instance.AccessLightData(this).Enabled = false;
		}
	}

	public void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.transform.position, "LensFlare Gizmo");
	}
}
