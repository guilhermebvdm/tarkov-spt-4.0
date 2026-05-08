using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class InventoryBlur : MonoBehaviour
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

	private float float_0;

	private bool bool_0;

	private static readonly int int_0 = Shader.PropertyToID("_BlurTex");

	private static readonly int int_1 = Shader.PropertyToID("_Value");

	private SSAAPropagator ssaapropagator_0;

	public void Awake()
	{
		_value = 0f;
		bool_0 = false;
		ssaapropagator_0 = GetComponent<SSAAPropagator>();
	}

	public void Enable(float time = 0.5f)
	{
		bool_0 = true;
		float_0 = time;
	}

	public void Disable()
	{
		bool_0 = false;
	}

	public void Update()
	{
		if (bool_0)
		{
			_value = Mathf.Clamp01(_value + Time.deltaTime / float_0);
			return;
		}
		_value = Mathf.Clamp01(_value - Time.deltaTime / float_0);
		if (_value < Mathf.Epsilon)
		{
			_value = 0f;
			base.enabled = false;
		}
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.GetSourceDestination(out source, out destination);
		}
		if (_value < Mathf.Epsilon)
		{
			if (ssaapropagator_0 != null)
			{
				ssaapropagator_0.ReleaseSourceDestination(source, destination);
			}
			Graphics.Blit(source, destination);
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
		Graphics.Blit(source, destination, _material);
		RenderTexture.ReleaseTemporary(temporary);
		RenderTexture.ReleaseTemporary(temporary2);
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.ReleaseSourceDestination(source, destination);
		}
	}
}
