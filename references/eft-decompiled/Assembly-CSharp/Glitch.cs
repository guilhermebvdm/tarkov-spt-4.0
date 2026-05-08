using EFT.BlitDebug;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Glitch : MonoBehaviour
{
	[SerializeField]
	private Shader _shader;

	[SerializeField]
	private Texture2D _displacementMap;

	[Header("Digital Mistake")]
	[SerializeField]
	[Range(0f, 1f)]
	private float _digitalMistakeIntensity;

	[SerializeField]
	[Range(0f, 1f)]
	private float _digitalMistakeFrequency = 1f;

	[Header("Vertical Flip")]
	[SerializeField]
	[Range(0f, 1f)]
	private float _verticalFlipIntensity;

	[SerializeField]
	[Range(0f, 1f)]
	private float _verticalFlipFrequency = 1f;

	[Header("Color Switch")]
	[SerializeField]
	[Range(0f, 1f)]
	private float _colorSwitchIntensity;

	[SerializeField]
	[Range(0f, 1f)]
	private float _colorSwitchFrequency = 1f;

	private float float_0;

	[Header("Vertical Jump")]
	[SerializeField]
	[Range(0f, 1f)]
	private float _verticalJumpCoef;

	[SerializeField]
	[Range(0f, 1f)]
	private float _verticalJumpFrequency = 1f;

	[SerializeField]
	private float _verticalJumpTimeScale = 11.3f;

	[Header("Scan Line Jitter")]
	[SerializeField]
	[Range(0f, 1f)]
	private float _scanLineJitterCoef;

	[SerializeField]
	[Range(0f, 1f)]
	private float _scanLineJitterFrequency = 1f;

	[Header("Horizontal Shake")]
	[SerializeField]
	[Range(0f, 1f)]
	private float _horizontalShake;

	[SerializeField]
	[Range(0f, 1f)]
	private float _horizontalShakeFrequency = 1f;

	[Header("Color Drift")]
	[SerializeField]
	[Range(0f, 1f)]
	private float _colorDriftCoef;

	[SerializeField]
	[Range(0f, 1f)]
	private float _colorDriftFrequency = 1f;

	[SerializeField]
	private float _maxColorDrift = 0.06f;

	private Material material_0;

	private float float_1 = 0.05f;

	private float float_2 = 0.05f;

	private float float_3 = 0.05f;

	private float float_4 = 0.05f;

	private float float_5 = 0.05f;

	private float float_6 = 0.5f;

	private static readonly int int_0 = Shader.PropertyToID("_VerticalJump");

	private static readonly int int_1 = Shader.PropertyToID("_ScanLineJitter");

	private static readonly int int_2 = Shader.PropertyToID("_HorizontalShake");

	private static readonly int int_3 = Shader.PropertyToID("_ColorDrift");

	private static readonly int int_4 = Shader.PropertyToID("_DigitalMistakeIntensity");

	private static readonly int int_5 = Shader.PropertyToID("_ColorSwitchIntensity");

	private static readonly int int_6 = Shader.PropertyToID("_DispTex");

	private static readonly int int_7 = Shader.PropertyToID("_FilterRadius");

	private static readonly int int_8 = Shader.PropertyToID("_Direction");

	private static readonly int int_9 = Shader.PropertyToID("_Flip_up");

	private static readonly int int_10 = Shader.PropertyToID("_Flip_down");

	private static readonly int int_11 = Shader.PropertyToID("_Displace");

	private static readonly int int_12 = Shader.PropertyToID("_Scale");

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (material_0 == null)
		{
			material_0 = new Material(_shader);
			material_0.hideFlags = HideFlags.DontSave;
		}
		method_0();
		method_1();
		RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height);
		DebugGraphics.Blit(source, temporary, material_0, 0);
		DebugGraphics.Blit(temporary, destination, material_0, 1);
		RenderTexture.ReleaseTemporary(temporary);
	}

	public void method_0()
	{
		Vector2 vector = new Vector2(_verticalJumpCoef * method_2(_verticalJumpFrequency), _verticalJumpTimeScale);
		material_0.SetVector(int_0, vector);
		float num = _scanLineJitterCoef * method_2(_scanLineJitterFrequency);
		float y = Mathf.Clamp01(1f - num * 1.2f);
		float x = 0.002f + Mathf.Pow(num, 3f) * 0.05f;
		material_0.SetVector(int_1, new Vector2(x, y));
		material_0.SetFloat(int_2, _horizontalShake * method_2(_horizontalShakeFrequency));
		float value = _colorDriftCoef * _maxColorDrift * method_2(_colorDriftFrequency);
		material_0.SetFloat(int_3, value);
		material_0.SetFloat(int_4, _digitalMistakeIntensity * method_2(_digitalMistakeFrequency));
		float_0 = _colorSwitchIntensity * method_2(_colorSwitchFrequency);
		material_0.SetFloat(int_5, float_0);
		material_0.SetTexture(int_6, _displacementMap);
	}

	public void method_1()
	{
		float_3 += Time.deltaTime * float_0;
		if (float_3 > float_6)
		{
			material_0.SetFloat(int_7, Random.Range(-3f, 3f) * float_0);
			material_0.SetVector(int_8, Quaternion.AngleAxis((float)Random.Range(0, 360) * float_0, Vector3.forward) * Vector4.one);
			float_3 = 0f;
			float_6 = Random.value;
		}
		if (float_0 == 0f)
		{
			material_0.SetFloat(int_7, 0f);
		}
		float num = _verticalFlipIntensity * method_2(_verticalFlipFrequency);
		float_1 += Time.deltaTime * num;
		if (float_1 > float_4)
		{
			if (Random.value < 0.1f * num)
			{
				material_0.SetFloat(int_9, Random.Range(0f, 1f) * num);
			}
			else
			{
				material_0.SetFloat(int_9, 0f);
			}
			float_1 = 0f;
			float_4 = Random.value / 10f;
		}
		if (num == 0f)
		{
			material_0.SetFloat(int_9, 0f);
		}
		float_2 += Time.deltaTime * num;
		if (float_2 > float_5)
		{
			if (Random.value < 0.1f * num)
			{
				material_0.SetFloat(int_10, 1f - Random.Range(0f, 1f) * num);
			}
			else
			{
				material_0.SetFloat(int_10, 1f);
			}
			float_2 = 0f;
			float_5 = Random.value / 10f;
		}
		if (num == 0f)
		{
			material_0.SetFloat(int_10, 1f);
		}
		if ((double)Random.value < 0.05 * (double)_digitalMistakeIntensity)
		{
			material_0.SetFloat(int_11, Random.value * _digitalMistakeIntensity);
			material_0.SetFloat(int_12, 1f - Random.value * _digitalMistakeIntensity);
		}
		else
		{
			material_0.SetFloat(int_11, 0f);
		}
	}

	public float method_2(float frequency)
	{
		return (Random.Range(0f, 1f) < frequency) ? 1 : 0;
	}
}
