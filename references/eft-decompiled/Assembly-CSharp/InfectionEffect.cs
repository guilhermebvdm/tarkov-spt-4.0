using UnityEngine;

public class InfectionEffect : MonoBehaviour
{
	private static int int_0 = Shader.PropertyToID("_MaskArea");

	[SerializeField]
	private Material _effectMaterial;

	[SerializeField]
	private AnimationCurve _refractCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve _infectCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private SSAAPropagator ssaapropagator_0;

	[SerializeField]
	[Range(0f, 1f)]
	private float _effectValue;

	private Vector4 vector4_0;

	public float EffectValue
	{
		get
		{
			return _effectValue;
		}
		set
		{
			_effectValue = value;
		}
	}

	public void Awake()
	{
		ssaapropagator_0 = GetComponent<SSAAPropagator>();
		vector4_0 = _effectMaterial.GetVector(int_0);
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.GetSourceDestination(out source, out destination);
		}
		method_0(source, destination);
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.ReleaseSourceDestination(source, destination);
		}
	}

	public void method_0(RenderTexture source, RenderTexture destination)
	{
		_effectValue = Mathf.Clamp01(_effectValue);
		if (_effectValue > 0f)
		{
			vector4_0.z = _infectCurve.Evaluate(_effectValue);
			vector4_0.w = _refractCurve.Evaluate(_effectValue);
			_effectMaterial.SetVector(int_0, vector4_0);
			Graphics.Blit(source, destination, _effectMaterial);
		}
		else
		{
			Graphics.Blit(source, destination);
		}
	}
}
