using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FastBlur : MonoBehaviour
{
	[Serializable]
	public enum Dimensions
	{
		_128 = 0x80,
		_256 = 0x100,
		_512 = 0x200,
		_1024 = 0x400,
		_2048 = 0x800
	}

	[Header("Blur Setup")]
	[SerializeField]
	[Range(0f, 1f)]
	private float _value;

	[SerializeField]
	private Material _material;

	[SerializeField]
	private Dimensions _upsampleTexDimension = Dimensions._512;

	[SerializeField]
	private Dimensions _downsampleTexDimension = Dimensions._256;

	[SerializeField]
	[Range(1f, 8f)]
	private int _blurCount = 4;

	[Header("Hit")]
	[SerializeField]
	private float _hitTime = 0.5f;

	[SerializeField]
	private AnimationCurve _hitCurve;

	[Header("Death")]
	[SerializeField]
	private float _deathTime = 1.5f;

	[SerializeField]
	private AnimationCurve _deathCurve;

	private SSAAPropagator ssaapropagator_0;

	private float float_0 = 1f;

	private float float_1 = 1f;

	private AnimationCurve animationCurve_0;

	private bool bool_0;

	private float float_2;

	private static readonly int int_0 = Shader.PropertyToID("_BlurTex");

	private static readonly int int_1 = Shader.PropertyToID("_Value");

	public void Awake()
	{
		ssaapropagator_0 = GetComponent<SSAAPropagator>();
	}

	public void Start()
	{
		animationCurve_0 = _hitCurve;
	}

	public void Hit(float power)
	{
		if (!bool_0)
		{
			float_1 = 0f;
			float_0 = _hitTime;
			animationCurve_0 = _hitCurve;
			float_2 = power;
		}
	}

	public void Die()
	{
		if (!bool_0)
		{
			float_1 = 0f;
			float_0 = _deathTime;
			animationCurve_0 = _deathCurve;
			bool_0 = true;
		}
	}

	public void Reset()
	{
		float_1 = 1f;
		_value = 0f;
		bool_0 = false;
		animationCurve_0 = _hitCurve;
	}

	public void Update()
	{
		float_1 += Time.deltaTime;
		float num = Mathf.Clamp01(float_1 / float_0);
		_value = animationCurve_0.Evaluate(num) * float_2;
		_value = Mathf.Clamp01(_value);
		if (num >= 1f)
		{
			base.enabled = false;
		}
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.GetSourceDestination(out source, out destination);
		}
		OnRenderImageImpl(source, destination);
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.ReleaseSourceDestination(source, destination);
		}
	}

	public void OnRenderImageImpl(RenderTexture source, RenderTexture destanation)
	{
		if (Mathf.Abs(_value) < Mathf.Epsilon)
		{
			GClass860.BlitOrCopy(source, destanation);
			return;
		}
		RenderTexture temporary = RenderTexture.GetTemporary((int)_upsampleTexDimension, (int)_upsampleTexDimension, 0);
		RenderTexture temporary2 = RenderTexture.GetTemporary((int)_downsampleTexDimension, (int)_downsampleTexDimension, 0);
		Graphics.Blit(source, temporary);
		for (int i = 0; i < _blurCount; i++)
		{
			Graphics.Blit(temporary, temporary2);
			Graphics.Blit(temporary2, temporary);
		}
		_material.SetTexture(int_0, temporary);
		_material.SetFloat(int_1, _value);
		Graphics.Blit(source, destanation, _material);
		RenderTexture.ReleaseTemporary(temporary);
		RenderTexture.ReleaseTemporary(temporary2);
	}
}
