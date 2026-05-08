using System;
using UnityEngine;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class DeathFade : MonoBehaviour
{
	[SerializeField]
	private Material _deathFade;

	[SerializeField]
	private Material _deathColorFade;

	[SerializeField]
	private Color _color = Color.black;

	[SerializeField]
	private float _closeEyesTime = 0.1f;

	[SerializeField]
	private float _closeEyesDelay = 1.8f;

	[SerializeField]
	private float _disableTime = 0.001f;

	[SerializeField]
	private AnimationCurve _enableCurve;

	[SerializeField]
	private AnimationCurve _disableCurve;

	[SerializeField]
	private bool _debug;

	private float float_0;

	[SerializeField]
	[Range(0f, 1f)]
	private float _closeEyesValue;

	[SerializeField]
	[Range(0f, 1f)]
	private float _fadeValue;

	private bool bool_0;

	private float float_1;

	private float float_2;

	private AnimationCurve animationCurve_0;

	private Material material_0;

	private Material material_1;

	private SSAAPropagator ssaapropagator_0;

	private float float_3;

	private static readonly int int_0 = Shader.PropertyToID("_Value");

	private static readonly int int_1 = Shader.PropertyToID("_Color");

	public void EnableEffect()
	{
		float_0 = _closeEyesTime;
		bool_0 = true;
		animationCurve_0 = _enableCurve;
	}

	public void DisableEffect()
	{
		float_0 = _disableTime;
		_closeEyesValue = 0f;
		_fadeValue = 0f;
		float_2 = 0f;
		float_3 = 0f;
		bool_0 = false;
		animationCurve_0 = _disableCurve;
	}

	public void Awake()
	{
		material_0 = GClass6.CopyToPreventMaterialChangeInEditor(_deathFade);
		material_1 = GClass6.CopyToPreventMaterialChangeInEditor(_deathColorFade);
		ssaapropagator_0 = GetComponent<SSAAPropagator>();
	}

	public void Update()
	{
		if (_debug || animationCurve_0 == null)
		{
			return;
		}
		if (bool_0)
		{
			float_3 += Time.deltaTime;
			float_2 = Mathf.Clamp01(float_2 + Time.deltaTime / 2f);
			if (float_3 >= _closeEyesDelay)
			{
				float_1 = Mathf.Clamp01(float_1 + Time.deltaTime / float_0);
			}
		}
		else
		{
			float_2 = Mathf.Clamp01(float_2 - Time.deltaTime / 2f);
			float_1 = Mathf.Clamp01(float_1 - Time.deltaTime / float_0);
		}
		_closeEyesValue = animationCurve_0.Evaluate(float_1);
		_fadeValue = animationCurve_0.Evaluate(float_2);
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
		if (Math.Abs(_closeEyesValue) < Mathf.Epsilon && Math.Abs(_fadeValue) < Mathf.Epsilon)
		{
			GClass860.BlitOrCopy(source, destination);
			return;
		}
		material_1.SetFloat(int_0, _fadeValue / 1.5f);
		Graphics.Blit(source, destination, material_1);
		if (!(Math.Abs(_closeEyesValue) < Mathf.Epsilon))
		{
			material_0.SetFloat(int_0, _closeEyesValue / 2f);
			material_0.SetColor(int_1, _color);
			Graphics.Blit(source, destination, material_0);
		}
	}
}
