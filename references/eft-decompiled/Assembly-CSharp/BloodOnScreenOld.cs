using System;
using System.Collections.Generic;
using UnityEngine;

[Obsolete]
public class BloodOnScreenOld : Posteffect
{
	[Serializable]
	public struct BloodData
	{
		public Texture DuDv;

		public Texture Blood;
	}

	[Header("Blood")]
	[SerializeField]
	private BloodData[] _bloodData;

	[SerializeField]
	private string _dudvPropertyName = "_DudvMap";

	[SerializeField]
	private string _bloodPropertyName = "_BloodTex";

	[SerializeField]
	private string _bloodAmountPropertyName = "_BloodTextureAmount";

	[SerializeField]
	private string _cutoutPropertyName = "_Cutout";

	[Range(0f, 1f)]
	[SerializeField]
	private float _cutout;

	[Range(0f, 1f)]
	[SerializeField]
	private float _bloodTextureAmount = 0.5f;

	[Header("Refraction")]
	[Range(0f, 1f)]
	[SerializeField]
	private float _refraction = 0.022f;

	[SerializeField]
	private string _refractionPropertyName = "_Refraction";

	[Header("Downsampling")]
	[Range(0f, 4f)]
	[SerializeField]
	private int _downsampling = 4;

	[Header("Color corection")]
	[HideInInspector]
	[SerializeField]
	public int Mode;

	[HideInInspector]
	[SerializeField]
	public float InputMinL;

	[HideInInspector]
	[SerializeField]
	public float InputMaxL = 255f;

	[HideInInspector]
	[SerializeField]
	public float InputGammaL = 1f;

	[HideInInspector]
	[SerializeField]
	public float InputMinR;

	[HideInInspector]
	[SerializeField]
	public float InputMaxR = 255f;

	[HideInInspector]
	[SerializeField]
	public float InputGammaR = 1f;

	[HideInInspector]
	[SerializeField]
	public float InputMinG;

	[HideInInspector]
	[SerializeField]
	public float InputMaxG = 255f;

	[HideInInspector]
	[SerializeField]
	public float InputGammaG = 1f;

	[HideInInspector]
	[SerializeField]
	public float InputMinB;

	[HideInInspector]
	[SerializeField]
	public float InputMaxB = 255f;

	[HideInInspector]
	[SerializeField]
	public float InputGammaB = 1f;

	[HideInInspector]
	[SerializeField]
	public float OutputMinL;

	[HideInInspector]
	[SerializeField]
	public float OutputMaxL = 255f;

	[HideInInspector]
	[SerializeField]
	public float OutputMinR;

	[HideInInspector]
	[SerializeField]
	public float OutputMaxR = 255f;

	[HideInInspector]
	[SerializeField]
	public float OutputMinG;

	[HideInInspector]
	[SerializeField]
	public float OutputMaxG = 255f;

	[HideInInspector]
	[SerializeField]
	public float OutputMinB;

	[HideInInspector]
	[SerializeField]
	public float OutputMaxB = 255f;

	private Dictionary<int, BloodData> dictionary_0;

	private static readonly int int_0 = Shader.PropertyToID("_inputMin");

	private static readonly int int_1 = Shader.PropertyToID("_inputMax");

	private static readonly int int_2 = Shader.PropertyToID("_inputGamma");

	private static readonly int int_3 = Shader.PropertyToID("_outputMin");

	private static readonly int int_4 = Shader.PropertyToID("_outputMax");

	private static readonly int int_5 = Shader.PropertyToID("_Blured");

	public float Cutout
	{
		get
		{
			return _cutout;
		}
		set
		{
			_cutout = Mathf.Clamp01(value);
		}
	}

	public int TextureSetsAmount => _bloodData.Length;

	public override void Start()
	{
		base.Start();
		dictionary_0 = new Dictionary<int, BloodData>(_bloodData.Length);
		for (int i = 0; i < _bloodData.Length; i++)
		{
			dictionary_0.Add(i, _bloodData[i]);
		}
		SetTexture(0);
	}

	public void SetTexture(int setNumber)
	{
		if (dictionary_0 == null)
		{
			dictionary_0 = new Dictionary<int, BloodData>(_bloodData.Length);
			for (int i = 0; i < _bloodData.Length; i++)
			{
				dictionary_0.Add(i, _bloodData[i]);
			}
		}
		setNumber = Mathf.Clamp(setNumber, 0, dictionary_0.Count - 1);
		EffectMaterial.SetTexture(_dudvPropertyName, dictionary_0[setNumber].DuDv);
		EffectMaterial.SetTexture(_bloodPropertyName, dictionary_0[setNumber].Blood);
	}

	public override void OnRenderImage(RenderTexture source, RenderTexture destanation)
	{
		if (Math.Abs(base.EffectAmount) < Mathf.Epsilon)
		{
			GClass860.BlitOrCopy(source, destanation);
			return;
		}
		EffectMaterial.SetFloat(_refractionPropertyName, _refraction);
		EffectMaterial.SetFloat(EffectValueName, base.EffectAmount);
		EffectMaterial.SetFloat(_bloodAmountPropertyName, _bloodTextureAmount);
		EffectMaterial.SetFloat(_cutoutPropertyName, Cutout);
		if (Mode == 0)
		{
			EffectMaterial.SetVector(int_0, new Vector4(InputMinL / 255f, InputMinL / 255f, InputMinL / 255f, 1f));
			EffectMaterial.SetVector(int_1, new Vector4(InputMaxL / 255f, InputMaxL / 255f, InputMaxL / 255f, 1f));
			EffectMaterial.SetVector(int_2, new Vector4(InputGammaL, InputGammaL, InputGammaL, 1f));
			EffectMaterial.SetVector(int_3, new Vector4(OutputMinL / 255f, OutputMinL / 255f, OutputMinL / 255f, 1f));
			EffectMaterial.SetVector(int_4, new Vector4(OutputMaxL / 255f, OutputMaxL / 255f, OutputMaxL / 255f, 1f));
		}
		else
		{
			EffectMaterial.SetVector(int_0, new Vector4(InputMinR / 255f, InputMinG / 255f, InputMinB / 255f, 1f));
			EffectMaterial.SetVector(int_1, new Vector4(InputMaxR / 255f, InputMaxG / 255f, InputMaxB / 255f, 1f));
			EffectMaterial.SetVector(int_2, new Vector4(InputGammaR, InputGammaG, InputGammaB, 1f));
			EffectMaterial.SetVector(int_3, new Vector4(OutputMinR / 255f, OutputMinG / 255f, OutputMinB / 255f, 1f));
			EffectMaterial.SetVector(int_4, new Vector4(OutputMaxR / 255f, OutputMaxG / 255f, OutputMaxB / 255f, 1f));
		}
		RenderTexture temporary = RenderTexture.GetTemporary(512, 512, 0);
		temporary.name = "BloodOnScreen T1";
		RenderTexture temporary2 = RenderTexture.GetTemporary(256, 256, 0);
		temporary2.name = "BloodOnScreen T2";
		Graphics.Blit(source, temporary);
		for (int i = 0; i < _downsampling; i++)
		{
			Graphics.Blit(temporary, temporary2);
			Graphics.Blit(temporary2, temporary);
		}
		EffectMaterial.SetTexture(int_5, temporary);
		Graphics.Blit(source, destanation, EffectMaterial);
		RenderTexture.ReleaseTemporary(temporary);
		RenderTexture.ReleaseTemporary(temporary2);
	}
}
