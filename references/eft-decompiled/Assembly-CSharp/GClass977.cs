using System;
using UnityEngine;
using UnityEngine.Rendering;

public class GClass977
{
	[NonSerialized]
	public ThermalVision ThermalVision_0;

	[NonSerialized]
	public Material Material_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1 = 0.05f;

	[NonSerialized]
	public float Float_2 = 0.05f;

	[NonSerialized]
	public float Float_3 = 0.05f;

	[NonSerialized]
	public float Float_4 = 0.05f;

	[NonSerialized]
	public float Float_5 = 0.05f;

	[NonSerialized]
	public float Float_6 = 0.5f;

	[NonSerialized]
	public static int Int_0 = Shader.PropertyToID("_VerticalJump");

	[NonSerialized]
	public static int Int_1 = Shader.PropertyToID("_ScanLineJitter");

	[NonSerialized]
	public static int Int_2 = Shader.PropertyToID("_HorizontalShake");

	[NonSerialized]
	public static int Int_3 = Shader.PropertyToID("_ColorDrift");

	[NonSerialized]
	public static int Int_4 = Shader.PropertyToID("_DigitalMistakeIntensity");

	[NonSerialized]
	public static int Int_5 = Shader.PropertyToID("_ColorSwitchIntensity");

	[NonSerialized]
	public static int Int_6 = Shader.PropertyToID("_DispTex");

	[NonSerialized]
	public static int Int_7 = Shader.PropertyToID("_FilterRadius");

	[NonSerialized]
	public static int Int_8 = Shader.PropertyToID("_Direction");

	[NonSerialized]
	public static int Int_9 = Shader.PropertyToID("_Flip_up");

	[NonSerialized]
	public static int Int_10 = Shader.PropertyToID("_Flip_down");

	[NonSerialized]
	public static int Int_11 = Shader.PropertyToID("_Displace");

	[NonSerialized]
	public static int Int_12 = Shader.PropertyToID("_Scale");

	[NonSerialized]
	public static int Int_13 = Shader.PropertyToID("_GlitchTempRT");

	public GClass977(ThermalVision thermalVision)
	{
		ThermalVision_0 = thermalVision;
	}

	public void UpdateTexture(CommandBuffer commandBuffer)
	{
		if (!(ThermalVision_0 == null))
		{
			if (Material_0 == null)
			{
				Material_0 = new Material(ThermalVision_0.GlitchUtilities.Shader);
				Material_0.hideFlags = HideFlags.DontSave;
			}
			method_0();
			method_1();
			commandBuffer.GetTemporaryRT(Int_13, -1, -1);
			commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, Int_13, Material_0, 0);
			commandBuffer.Blit(Int_13, BuiltinRenderTextureType.CameraTarget, Material_0, 1);
			commandBuffer.ReleaseTemporaryRT(Int_13);
		}
	}

	public void method_0()
	{
		GlitchUtilities glitchUtilities = ThermalVision_0.GlitchUtilities;
		Vector2 vector = new Vector2(glitchUtilities.VerticalJumpCoef * method_2(glitchUtilities.VerticalJumpFrequency), glitchUtilities.VerticalJumpTimeScale);
		Material_0.SetVector(Int_0, vector);
		float num = glitchUtilities.ScanLineJitterCoef * method_2(glitchUtilities.ScanLineJitterFrequency);
		float y = Mathf.Clamp01(1f - num * 1.2f);
		float x = 0.002f + Mathf.Pow(num, 3f) * 0.05f;
		Material_0.SetVector(Int_1, new Vector2(x, y));
		Material_0.SetFloat(Int_2, glitchUtilities.HorizontalShake * method_2(glitchUtilities.HorizontalShakeFrequency));
		float value = glitchUtilities.ColorDriftCoef * glitchUtilities.MaxColorDrift * method_2(glitchUtilities.ColorDriftFrequency);
		Material_0.SetFloat(Int_3, value);
		Material_0.SetFloat(Int_4, glitchUtilities.DigitalMistakeIntensity * method_2(glitchUtilities.DigitalMistakeFrequency));
		Float_0 = glitchUtilities.ColorSwitchIntensity * method_2(glitchUtilities.ColorSwitchFrequency);
		Material_0.SetFloat(Int_5, Float_0);
		Material_0.SetTexture(Int_6, glitchUtilities.DisplacementMap);
	}

	public void method_1()
	{
		GlitchUtilities glitchUtilities = ThermalVision_0.GlitchUtilities;
		Float_3 += Time.deltaTime * Float_0;
		if (Float_3 > Float_6)
		{
			Material_0.SetFloat(Int_7, UnityEngine.Random.Range(-3f, 3f) * Float_0);
			Material_0.SetVector(Int_8, Quaternion.AngleAxis((float)UnityEngine.Random.Range(0, 360) * Float_0, Vector3.forward) * Vector4.one);
			Float_3 = 0f;
			Float_6 = UnityEngine.Random.value;
		}
		if (Float_0 == 0f)
		{
			Material_0.SetFloat(Int_7, 0f);
		}
		float num = glitchUtilities.VerticalFlipIntensity * method_2(glitchUtilities.VerticalFlipFrequency);
		Float_1 += Time.deltaTime * num;
		if (Float_1 > Float_4)
		{
			if (UnityEngine.Random.value < 0.1f * num)
			{
				Material_0.SetFloat(Int_9, UnityEngine.Random.Range(0f, 1f) * num);
			}
			else
			{
				Material_0.SetFloat(Int_9, 0f);
			}
			Float_1 = 0f;
			Float_4 = UnityEngine.Random.value / 10f;
		}
		if (num == 0f)
		{
			Material_0.SetFloat(Int_9, 0f);
		}
		Float_2 += Time.deltaTime * num;
		if (Float_2 > Float_5)
		{
			if (UnityEngine.Random.value < 0.1f * num)
			{
				Material_0.SetFloat(Int_10, 1f - UnityEngine.Random.Range(0f, 1f) * num);
			}
			else
			{
				Material_0.SetFloat(Int_10, 1f);
			}
			Float_2 = 0f;
			Float_5 = UnityEngine.Random.value / 10f;
		}
		if (num == 0f)
		{
			Material_0.SetFloat(Int_10, 1f);
		}
		if ((double)UnityEngine.Random.value < 0.05 * (double)glitchUtilities.DigitalMistakeIntensity)
		{
			Material_0.SetFloat(Int_11, UnityEngine.Random.value * glitchUtilities.DigitalMistakeIntensity);
			Material_0.SetFloat(Int_12, 1f - UnityEngine.Random.value * glitchUtilities.DigitalMistakeIntensity);
		}
		else
		{
			Material_0.SetFloat(Int_11, 0f);
		}
	}

	public float method_2(float frequency)
	{
		return (UnityEngine.Random.Range(0f, 1f) < frequency) ? 1 : 0;
	}
}
