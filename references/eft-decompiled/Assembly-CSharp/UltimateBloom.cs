using System;
using System.Collections.Generic;
using BSG.CameraEffects;
using EFT.BlitDebug;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class UltimateBloom : MonoBehaviour
{
	public enum BloomQualityPreset
	{
		Optimized,
		Standard,
		HighVisuals,
		Custom
	}

	public enum BloomSamplingQuality
	{
		VerySmallKernel,
		SmallKernel,
		MediumKernel,
		LargeKernel,
		LargerKernel,
		VeryLargeKernel
	}

	public enum BloomScreenBlendMode
	{
		Screen,
		Add
	}

	public enum HDRBloomMode
	{
		Auto,
		On,
		Off
	}

	public enum BlurSampleCount
	{
		Nine,
		Seventeen,
		Thirteen,
		TwentyThree,
		TwentySeven,
		ThrirtyOne,
		NineCurve,
		FourSimple
	}

	public enum FlareRendering
	{
		Sharp,
		Blurred,
		MoreBlurred
	}

	public enum SimpleSampleCount
	{
		Four,
		Nine,
		FourCurve,
		ThirteenTemporal,
		ThirteenTemporalCurve
	}

	public enum FlareType
	{
		Single,
		Double
	}

	public enum BloomIntensityManagement
	{
		FilmicCurve,
		Threshold
	}

	public enum FlareStripeType
	{
		Anamorphic,
		Star,
		DiagonalUpright,
		DiagonalUpleft
	}

	public enum AnamorphicDirection
	{
		Horizontal,
		Vertical
	}

	public enum BokehFlareQuality
	{
		Low,
		Medium,
		High,
		VeryHigh
	}

	public enum BlendMode
	{
		ADD,
		SCREEN
	}

	public enum SamplingMode
	{
		Fixed,
		HeightRelative
	}

	public enum FlareBlurQuality
	{
		Fast,
		Normal,
		High
	}

	public enum FlarePresets
	{
		ChoosePreset,
		GhostFast,
		Ghost1,
		Ghost2,
		Ghost3,
		Bokeh1,
		Bokeh2,
		Bokeh3
	}

	public delegate void Delegate2(RenderTexture source, RenderTexture destination, float horizontalBlur, float verticalBlur, RenderTexture additiveTexture, BlurSampleCount sampleCount, Color tint, float intensity);

	public float m_SamplingMinHeight = 400f;

	public float[] m_ResSamplingPixelCount = new float[6];

	public SamplingMode m_SamplingMode;

	public BlendMode m_BlendMode;

	public float m_ScreenMaxIntensity;

	public BloomQualityPreset m_QualityPreset;

	public HDRBloomMode m_HDR;

	public BloomScreenBlendMode m_ScreenBlendMode = BloomScreenBlendMode.Add;

	public float m_BloomIntensity = 1f;

	public float m_BloomThreshhold = 0.5f;

	public Color m_BloomThreshholdColor = Color.white;

	public int m_DownscaleCount = 5;

	public BloomIntensityManagement m_IntensityManagement;

	public float[] m_BloomIntensities;

	public Color[] m_BloomColors;

	public bool[] m_BloomUsages;

	[SerializeField]
	public DeluxeFilmicCurve m_BloomCurve = new DeluxeFilmicCurve();

	private int int_0 = 5;

	public bool useTriangleBlit = true;

	private CommandBuffer commandBuffer_0;

	private List<RenderTexture> list_0 = new List<RenderTexture>();

	private List<MaterialPropertyBlock> list_1 = new List<MaterialPropertyBlock>();

	private int int_1;

	private SSAAPropagator ssaapropagator_0;

	public bool m_UseLensFlare;

	public float m_FlareTreshold = 0.8f;

	public float m_FlareIntensity = 0.25f;

	public Color m_FlareTint0 = new Color(0.5372549f, 0.32156864f, 0f);

	public Color m_FlareTint1 = new Color(0f, 21f / 85f, 42f / 85f);

	public Color m_FlareTint2 = new Color(24f / 85f, 0.5921569f, 0f);

	public Color m_FlareTint3 = new Color(38f / 85f, 7f / 51f, 0f);

	public Color m_FlareTint4 = new Color(0.47843137f, 0.34509805f, 0f);

	public Color m_FlareTint5 = new Color(0.5372549f, 0.2784314f, 0f);

	public Color m_FlareTint6 = new Color(0.38039216f, 0.54509807f, 0f);

	public Color m_FlareTint7 = new Color(8f / 51f, 0.5568628f, 0f);

	public float m_FlareGlobalScale = 1f;

	public Vector4 m_FlareScales = new Vector4(1f, 0.6f, 0.5f, 0.4f);

	public Vector4 m_FlareScalesNear = new Vector4(1f, 0.8f, 0.6f, 0.5f);

	public Texture2D m_FlareMask;

	public FlareRendering m_FlareRendering = FlareRendering.Blurred;

	public FlareType m_FlareType = FlareType.Double;

	public Texture2D m_FlareShape;

	public FlareBlurQuality m_FlareBlurQuality = FlareBlurQuality.High;

	private Class500 class500_0;

	private Mesh[] mesh_0;

	public bool m_UseBokehFlare;

	public float m_BokehScale = 0.4f;

	public BokehFlareQuality m_BokehFlareQuality = BokehFlareQuality.Medium;

	public bool m_UseAnamorphicFlare;

	public float m_AnamorphicFlareTreshold = 0.8f;

	public float m_AnamorphicFlareIntensity = 1f;

	public int m_AnamorphicDownscaleCount = 3;

	public int m_AnamorphicBlurPass = 2;

	private int int_2;

	private RenderTexture[] renderTexture_0;

	public float[] m_AnamorphicBloomIntensities;

	public Color[] m_AnamorphicBloomColors;

	public bool[] m_AnamorphicBloomUsages;

	public bool m_AnamorphicSmallVerticalBlur = true;

	public AnamorphicDirection m_AnamorphicDirection;

	public float m_AnamorphicScale = 3f;

	public bool m_UseStarFlare;

	public float m_StarFlareTreshol = 0.8f;

	public float m_StarFlareIntensity = 1f;

	public float m_StarScale = 2f;

	public int m_StarDownscaleCount = 3;

	public int m_StarBlurPass = 2;

	private int int_3;

	private RenderTexture[] renderTexture_1;

	public float[] m_StarBloomIntensities;

	public Color[] m_StarBloomColors;

	public bool[] m_StarBloomUsages;

	public bool m_UseLensDust;

	public float m_DustIntensity = 1f;

	public Texture2D m_DustTexture;

	public float m_DirtLightIntensity = 5f;

	public BloomSamplingQuality m_DownsamplingQuality;

	public BloomSamplingQuality m_UpsamplingQuality;

	public bool m_TemporalStableDownsampling = true;

	public bool m_InvertImage;

	private Material material_0;

	private Shader shader_0;

	private Material material_1;

	private Shader shader_1;

	private Material material_2;

	private Shader shader_2;

	private Material material_3;

	private Material material_4;

	private Shader shader_3;

	private Shader shader_4;

	private Material material_5;

	private Shader shader_5;

	private Material material_6;

	private Shader shader_6;

	private Material material_7;

	private Shader shader_7;

	public bool m_DirectDownSample;

	public bool m_DirectUpsample;

	public bool m_UiShowBloomScales;

	public bool m_UiShowAnamorphicBloomScales;

	public bool m_UiShowStarBloomScales;

	public bool m_UiShowHeightSampling;

	public bool m_UiShowBloomSettings;

	public bool m_UiShowSampling;

	public bool m_UiShowIntensity;

	public bool m_UiShowOptimizations;

	public bool m_UiShowLensDirt;

	public bool m_UiShowLensFlare;

	public bool m_UiShowAnamorphic;

	public bool m_UiShowStar;

	private NightVision nightVision_0;

	private static readonly int int_4 = Shader.PropertyToID("_MaskTex");

	private static readonly int int_5 = Shader.PropertyToID("_Intensity");

	private static readonly int int_6 = Shader.PropertyToID("_FlareIntensity");

	private static readonly int int_7 = Shader.PropertyToID("_ColorBuffer");

	private static readonly int int_8 = Shader.PropertyToID("_FlareTexture");

	private static readonly int int_9 = Shader.PropertyToID("_AdditiveTexture");

	private static readonly int int_10 = Shader.PropertyToID("_brightTexture");

	private static readonly int int_11 = Shader.PropertyToID("_DirtIntensity");

	private static readonly int int_12 = Shader.PropertyToID("_DirtLightIntensity");

	private static readonly int int_13 = Shader.PropertyToID("_ScreenMaxIntensity");

	private static readonly int int_14 = Shader.PropertyToID("_FlareScales");

	private static readonly int int_15 = Shader.PropertyToID("_FlareScalesNear");

	private static readonly int int_16 = Shader.PropertyToID("_FlareTint0");

	private static readonly int int_17 = Shader.PropertyToID("_FlareTint1");

	private static readonly int int_18 = Shader.PropertyToID("_FlareTint2");

	private static readonly int int_19 = Shader.PropertyToID("_FlareTint3");

	private static readonly int int_20 = Shader.PropertyToID("_FlareTint4");

	private static readonly int int_21 = Shader.PropertyToID("_FlareTint5");

	private static readonly int int_22 = Shader.PropertyToID("_FlareTint6");

	private static readonly int int_23 = Shader.PropertyToID("_FlareTint7");

	private static readonly int int_24 = Shader.PropertyToID("_CurveExposure");

	private static readonly int int_25 = Shader.PropertyToID("_K");

	private static readonly int int_26 = Shader.PropertyToID("_Crossover");

	private static readonly int int_27 = Shader.PropertyToID("_Toe");

	private static readonly int int_28 = Shader.PropertyToID("_Shoulder");

	private static readonly int int_29 = Shader.PropertyToID("_MaxValue");

	private static readonly int int_30 = Shader.PropertyToID("_Threshhold");

	private static readonly int int_31 = Shader.PropertyToID("_OffsetInfos");

	private static readonly int int_32 = Shader.PropertyToID("_Tint");

	private static readonly int int_33 = Shader.PropertyToID("_Intensity0");

	private static readonly int int_34 = Shader.PropertyToID("_Intensity1");

	private RenderTexture[] renderTexture_2;

	private RenderTexture[] renderTexture_3;

	private RenderTextureFormat renderTextureFormat_0;

	private bool[] bool_0;

	private RenderTexture renderTexture_4;

	public bool Boolean_0
	{
		get
		{
			if (nightVision_0 != null)
			{
				return nightVision_0.On;
			}
			return false;
		}
	}

	public MaterialPropertyBlock method_0()
	{
		if (int_1 >= list_1.Count)
		{
			list_1.Add(new MaterialPropertyBlock());
		}
		MaterialPropertyBlock materialPropertyBlock = list_1[int_1];
		int_1++;
		materialPropertyBlock.Clear();
		return materialPropertyBlock;
	}

	public void method_1()
	{
		int_1 = 0;
	}

	public void Start()
	{
		nightVision_0 = base.gameObject.GetComponent<NightVision>();
	}

	public void Awake()
	{
		ssaapropagator_0 = GetComponent<SSAAPropagator>();
	}

	public void method_2(Material mat)
	{
		if ((bool)mat)
		{
			UnityEngine.Object.DestroyImmediate(mat);
			mat = null;
		}
	}

	public void method_3(ref Material material, ref Shader shader, string shaderPath)
	{
		if (!(shader != null))
		{
			shader = GClass872.Find(shaderPath);
			if (shader == null)
			{
				Debug.LogError("Shader not found: " + shaderPath);
			}
			else if (!shader.isSupported)
			{
				Debug.LogError("Shader contains error: " + shaderPath + "\n Maybe include path? Try rebuilding the shader.");
			}
			else
			{
				material = method_4(shader);
			}
		}
	}

	public void CreateMaterials()
	{
		int num = 8;
		if (m_BloomIntensities == null || m_BloomIntensities.Length < num)
		{
			m_BloomIntensities = new float[num];
			for (int i = 0; i < 8; i++)
			{
				m_BloomIntensities[i] = 1f;
			}
		}
		if (m_BloomColors == null || m_BloomColors.Length < num)
		{
			m_BloomColors = new Color[num];
			for (int j = 0; j < 8; j++)
			{
				m_BloomColors[j] = Color.white;
			}
		}
		if (m_BloomUsages == null || m_BloomUsages.Length < num)
		{
			m_BloomUsages = new bool[num];
			for (int k = 0; k < 8; k++)
			{
				m_BloomUsages[k] = true;
			}
		}
		if (m_AnamorphicBloomIntensities == null || m_AnamorphicBloomIntensities.Length < num)
		{
			m_AnamorphicBloomIntensities = new float[num];
			for (int l = 0; l < 8; l++)
			{
				m_AnamorphicBloomIntensities[l] = 1f;
			}
		}
		if (m_AnamorphicBloomColors == null || m_AnamorphicBloomColors.Length < num)
		{
			m_AnamorphicBloomColors = new Color[num];
			for (int m = 0; m < 8; m++)
			{
				m_AnamorphicBloomColors[m] = Color.white;
			}
		}
		if (m_AnamorphicBloomUsages == null || m_AnamorphicBloomUsages.Length < num)
		{
			m_AnamorphicBloomUsages = new bool[num];
			for (int n = 0; n < 8; n++)
			{
				m_AnamorphicBloomUsages[n] = true;
			}
		}
		if (m_StarBloomIntensities == null || m_StarBloomIntensities.Length < num)
		{
			m_StarBloomIntensities = new float[num];
			for (int num2 = 0; num2 < 8; num2++)
			{
				m_StarBloomIntensities[num2] = 1f;
			}
		}
		if (m_StarBloomColors == null || m_StarBloomColors.Length < num)
		{
			m_StarBloomColors = new Color[num];
			for (int num3 = 0; num3 < 8; num3++)
			{
				m_StarBloomColors[num3] = Color.white;
			}
		}
		if (m_StarBloomUsages == null || m_StarBloomUsages.Length < num)
		{
			m_StarBloomUsages = new bool[num];
			for (int num4 = 0; num4 < 8; num4++)
			{
				m_StarBloomUsages[num4] = true;
			}
		}
		if (class500_0 == null && m_FlareShape != null && m_UseBokehFlare)
		{
			if (class500_0 != null)
			{
				class500_0.Clear(ref mesh_0);
			}
			class500_0 = new Class500();
		}
		if (material_1 == null)
		{
			renderTexture_2 = new RenderTexture[method_5()];
			renderTexture_3 = new RenderTexture[m_DownscaleCount];
			renderTexture_0 = new RenderTexture[m_AnamorphicDownscaleCount];
			renderTexture_1 = new RenderTexture[m_StarDownscaleCount];
		}
		string shaderPath = ((m_FlareType == FlareType.Single) ? "Hidden/Ultimate/FlareSingle" : "Hidden/Ultimate/FlareDouble");
		method_3(ref material_0, ref shader_0, shaderPath);
		method_3(ref material_1, ref shader_1, "Hidden/Ultimate/Sampling");
		method_3(ref material_3, ref shader_3, "Hidden/Ultimate/BrightpassMask");
		method_3(ref material_4, ref shader_4, "Hidden/Ultimate/BrightpassMask");
		method_3(ref material_5, ref shader_5, "Hidden/Ultimate/FlareMask");
		method_3(ref material_6, ref shader_6, "Hidden/Ultimate/BloomMixer");
		method_3(ref material_7, ref shader_7, "Hidden/Ultimate/FlareMesh");
		bool num5 = m_UseLensDust || m_UseLensFlare || m_UseAnamorphicFlare || m_UseStarFlare;
		string shaderPath2 = "Hidden/Ultimate/BloomCombine";
		if (num5)
		{
			shaderPath2 = "Hidden/Ultimate/BloomCombineFlareDirt";
		}
		method_3(ref material_2, ref shader_2, shaderPath2);
	}

	public Material method_4(Shader shader)
	{
		if (!shader)
		{
			return null;
		}
		return new Material(shader)
		{
			hideFlags = HideFlags.HideAndDontSave
		};
	}

	public void OnDisable()
	{
		if (base.gameObject.activeInHierarchy)
		{
			ForceShadersReload();
			if (class500_0 != null)
			{
				class500_0.Clear(ref mesh_0);
				class500_0 = null;
			}
		}
	}

	public void ForceShadersReload()
	{
		method_2(material_0);
		material_0 = null;
		shader_0 = null;
		method_2(material_1);
		material_1 = null;
		shader_1 = null;
		method_2(material_2);
		material_2 = null;
		shader_2 = null;
		method_2(material_3);
		material_3 = null;
		shader_3 = null;
		method_2(material_4);
		material_4 = null;
		shader_4 = null;
		method_2(material_7);
		material_7 = null;
		shader_7 = null;
		method_2(material_5);
		material_5 = null;
		shader_5 = null;
		method_2(material_6);
		material_6 = null;
		shader_6 = null;
	}

	public int method_5()
	{
		return Mathf.Max(Mathf.Max(Mathf.Max(m_DownscaleCount, m_UseAnamorphicFlare ? m_AnamorphicDownscaleCount : 0), m_UseLensFlare ? (method_7() + 1) : 0), m_UseStarFlare ? m_StarDownscaleCount : 0);
	}

	public void method_6()
	{
		if (bool_0 == null)
		{
			bool_0 = new bool[renderTexture_2.Length];
		}
		if (bool_0.Length != renderTexture_2.Length)
		{
			bool_0 = new bool[renderTexture_2.Length];
		}
		for (int i = 0; i < bool_0.Length; i++)
		{
			bool_0[i] = false;
		}
		for (int j = 0; j < bool_0.Length; j++)
		{
			bool_0[j] = m_BloomUsages[j] || bool_0[j];
		}
		if (m_UseAnamorphicFlare)
		{
			for (int k = 0; k < bool_0.Length; k++)
			{
				bool_0[k] = m_AnamorphicBloomUsages[k] || bool_0[k];
			}
		}
		if (m_UseStarFlare)
		{
			for (int l = 0; l < bool_0.Length; l++)
			{
				bool_0[l] = m_StarBloomUsages[l] || bool_0[l];
			}
		}
	}

	public int method_7()
	{
		if (m_UseBokehFlare && m_FlareShape != null)
		{
			if (m_BokehFlareQuality == BokehFlareQuality.VeryHigh)
			{
				return 1;
			}
			if (m_BokehFlareQuality == BokehFlareQuality.High)
			{
				return 2;
			}
			if (m_BokehFlareQuality == BokehFlareQuality.Medium)
			{
				return 3;
			}
			if (m_BokehFlareQuality == BokehFlareQuality.Low)
			{
				return 4;
			}
		}
		return 0;
	}

	public BlurSampleCount method_8()
	{
		if (m_SamplingMode == SamplingMode.Fixed)
		{
			BlurSampleCount result = BlurSampleCount.ThrirtyOne;
			if (m_UpsamplingQuality == BloomSamplingQuality.VerySmallKernel)
			{
				result = BlurSampleCount.Nine;
			}
			else if (m_UpsamplingQuality == BloomSamplingQuality.SmallKernel)
			{
				result = BlurSampleCount.Thirteen;
			}
			else if (m_UpsamplingQuality == BloomSamplingQuality.MediumKernel)
			{
				result = BlurSampleCount.Seventeen;
			}
			else if (m_UpsamplingQuality == BloomSamplingQuality.LargeKernel)
			{
				result = BlurSampleCount.TwentyThree;
			}
			else if (m_UpsamplingQuality == BloomSamplingQuality.LargerKernel)
			{
				result = BlurSampleCount.TwentySeven;
			}
			return result;
		}
		float num = Screen.height;
		int num2 = 0;
		float num3 = float.MaxValue;
		for (int i = 0; i < m_ResSamplingPixelCount.Length; i++)
		{
			float num4 = Math.Abs(num - m_ResSamplingPixelCount[i]);
			if (num4 < num3)
			{
				num3 = num4;
				num2 = i;
			}
		}
		return num2 switch
		{
			0 => BlurSampleCount.Nine, 
			1 => BlurSampleCount.Thirteen, 
			2 => BlurSampleCount.Seventeen, 
			3 => BlurSampleCount.TwentyThree, 
			4 => BlurSampleCount.TwentySeven, 
			_ => BlurSampleCount.ThrirtyOne, 
		};
	}

	public void ComputeResolutionRelativeData()
	{
		float num = m_SamplingMinHeight;
		float num2 = 9f;
		for (int i = 0; i < m_ResSamplingPixelCount.Length; i++)
		{
			m_ResSamplingPixelCount[i] = num;
			float num3 = num2 + 4f;
			float num4 = num3 / num2;
			num *= num4;
			num2 = num3;
		}
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.GetSourceDestination(out source, out destination);
		}
		method_9(source, destination);
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.ReleaseSourceDestination(source, destination);
		}
	}

	public void method_9(RenderTexture source, RenderTexture destination)
	{
		if (commandBuffer_0 == null)
		{
			commandBuffer_0 = new CommandBuffer();
			commandBuffer_0.name = "UltimateBloomCB";
		}
		commandBuffer_0.Clear();
		bool flag = false;
		flag = ((m_HDR != HDRBloomMode.Auto) ? (m_HDR == HDRBloomMode.On) : (source.format == RuntimeUtilities.defaultHDRRenderTextureFormat && GetComponent<Camera>().allowHDR));
		renderTextureFormat_0 = (flag ? RuntimeUtilities.defaultHDRRenderTextureFormat : RenderTextureFormat.ARGB32);
		if (renderTexture_2 != null && renderTexture_2.Length != method_5())
		{
			OnDisable();
		}
		if (int_0 != m_DownscaleCount || int_2 != m_AnamorphicDownscaleCount || int_3 != m_StarDownscaleCount)
		{
			OnDisable();
		}
		int_0 = m_DownscaleCount;
		int_2 = m_AnamorphicDownscaleCount;
		int_3 = m_StarDownscaleCount;
		CreateMaterials();
		if (m_DirectDownSample || m_DirectUpsample)
		{
			method_6();
		}
		bool flag2 = false;
		if (m_SamplingMode == SamplingMode.HeightRelative)
		{
			ComputeResolutionRelativeData();
		}
		RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, renderTextureFormat_0);
		temporary.filterMode = FilterMode.Bilinear;
		if (m_IntensityManagement == BloomIntensityManagement.Threshold)
		{
			float num = (Boolean_0 ? 0.5f : 1f);
			method_16(source, temporary, m_BloomThreshhold * m_BloomThreshholdColor * num, useTriangleBlit ? method_0() : null);
		}
		else
		{
			m_BloomCurve.UpdateCoefficients();
			if (useTriangleBlit)
			{
				GClass860.BlitOrCopy(commandBuffer_0, source, temporary);
			}
			else
			{
				GClass860.BlitOrCopy(source, temporary);
			}
		}
		if (m_IntensityManagement == BloomIntensityManagement.Threshold)
		{
			method_15(temporary, renderTexture_2, null, flag);
		}
		else
		{
			method_15(temporary, renderTexture_2, m_BloomCurve, flag);
		}
		BlurSampleCount upsamplingCount = method_8();
		method_14(renderTexture_2, renderTexture_3, source.width, source.height, upsamplingCount);
		Texture flareRT = Texture2D.blackTexture;
		RenderTexture renderTexture = null;
		if (m_UseLensFlare)
		{
			int num2 = method_7();
			int num3 = source.width / (int)Mathf.Pow(2f, num2);
			int num4 = source.height / (int)Mathf.Pow(2f, num2);
			if (m_FlareShape != null && m_UseBokehFlare)
			{
				float num5 = 15f;
				if (m_BokehFlareQuality == BokehFlareQuality.Medium)
				{
					num5 *= 2f;
				}
				if (m_BokehFlareQuality == BokehFlareQuality.High)
				{
					num5 *= 4f;
				}
				if (m_BokehFlareQuality == BokehFlareQuality.VeryHigh)
				{
					num5 *= 8f;
				}
				num5 *= m_BokehScale;
				class500_0.SetMaterial(material_7);
				class500_0.RebuildMeshIfNeeded(num3, num4, 1f / (float)num3 * num5, 1f / (float)num4 * num5, ref mesh_0);
				class500_0.SetTexture(m_FlareShape);
				renderTexture = RenderTexture.GetTemporary(source.width / 4, source.height / 4, 0, renderTextureFormat_0);
				int num6 = num2;
				RenderTexture temporary2 = RenderTexture.GetTemporary(source.width / (int)Mathf.Pow(2f, num6 + 1), source.height / (int)Mathf.Pow(2f, num6 + 1), 0, renderTextureFormat_0);
				method_16(renderTexture_2[num2], temporary2, m_FlareTreshold * Vector4.one, useTriangleBlit ? method_0() : null);
				class500_0.RenderFlare(useTriangleBlit ? commandBuffer_0 : null, temporary2, renderTexture, m_UseBokehFlare ? 1f : m_FlareIntensity, ref mesh_0);
				list_0.Add(temporary2);
				RenderTexture temporary3 = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, 0, renderTextureFormat_0);
				material_5.SetTexture(int_4, m_FlareMask);
				if (useTriangleBlit)
				{
					commandBuffer_0.BlitFullscreenTriangle(renderTexture, temporary3, material_5);
				}
				else
				{
					DebugGraphics.Blit(renderTexture, temporary3, material_5, 0);
				}
				list_0.Add(renderTexture);
				renderTexture = null;
				method_13(temporary3, source, ref flareRT);
				list_0.Add(temporary3);
			}
			else
			{
				int num7 = method_7();
				RenderTexture renderTexture2 = renderTexture_2[num7];
				RenderTexture temporary4 = RenderTexture.GetTemporary(renderTexture2.width, renderTexture2.height, 0, renderTextureFormat_0);
				method_17(renderTexture_2[num7], temporary4, m_FlareTreshold * Vector4.one, m_FlareMask, useTriangleBlit ? method_0() : null);
				method_13(temporary4, source, ref flareRT);
				list_0.Add(temporary4);
			}
		}
		if (m_UseAnamorphicFlare)
		{
			RenderTexture renderTexture3 = method_12(renderTexture_2, upsamplingCount, source.width, source.height, FlareStripeType.Anamorphic);
			if (renderTexture3 != null)
			{
				if (m_UseLensFlare)
				{
					method_22(renderTexture3, (RenderTexture)flareRT, 1f, useTriangleBlit ? method_0() : null);
					list_0.Add(renderTexture3);
				}
				else
				{
					flareRT = renderTexture3;
				}
			}
		}
		if (m_UseStarFlare)
		{
			RenderTexture renderTexture4 = null;
			if (m_StarBlurPass == 1)
			{
				renderTexture4 = method_12(renderTexture_2, upsamplingCount, source.width, source.height, FlareStripeType.Star);
				if (renderTexture4 != null)
				{
					if (!m_UseLensFlare && !m_UseAnamorphicFlare)
					{
						flareRT = RenderTexture.GetTemporary(source.width, source.height, 0, renderTextureFormat_0);
						method_23(renderTexture4, (RenderTexture)flareRT, m_StarFlareIntensity);
					}
					else
					{
						method_22(renderTexture4, (RenderTexture)flareRT, m_StarFlareIntensity, useTriangleBlit ? method_0() : null);
					}
					list_0.Add(renderTexture4);
				}
			}
			else if (!m_UseLensFlare && !m_UseAnamorphicFlare)
			{
				renderTexture4 = method_12(renderTexture_2, upsamplingCount, source.width, source.height, FlareStripeType.DiagonalUpleft);
				if (renderTexture4 != null)
				{
					RenderTexture renderTexture5 = method_12(renderTexture_2, upsamplingCount, source.width, source.height, FlareStripeType.DiagonalUpright);
					method_24(renderTexture5, renderTexture4, m_StarFlareIntensity, m_StarFlareIntensity);
					list_0.Add(renderTexture5);
					flareRT = renderTexture4;
				}
			}
			else
			{
				renderTexture4 = method_12(renderTexture_2, upsamplingCount, source.width, source.height, FlareStripeType.DiagonalUpright);
				if (renderTexture4 != null)
				{
					method_22(renderTexture4, (RenderTexture)flareRT, m_StarFlareIntensity, useTriangleBlit ? method_0() : null);
					list_0.Add(renderTexture4);
					renderTexture4 = method_12(renderTexture_2, upsamplingCount, source.width, source.height, FlareStripeType.DiagonalUpleft);
					method_22(renderTexture4, (RenderTexture)flareRT, m_StarFlareIntensity, useTriangleBlit ? method_0() : null);
					list_0.Add(renderTexture4);
				}
			}
		}
		if (m_DirectDownSample)
		{
			for (int i = 0; i < renderTexture_2.Length; i++)
			{
				if (bool_0[i])
				{
					list_0.Add(renderTexture_2[i]);
				}
			}
		}
		else
		{
			for (int j = 0; j < renderTexture_2.Length; j++)
			{
				list_0.Add(renderTexture_2[j]);
			}
		}
		MaterialPropertyBlock materialPropertyBlock = null;
		if (useTriangleBlit)
		{
			materialPropertyBlock = method_0();
			materialPropertyBlock.SetFloat(int_5, m_BloomIntensity);
			materialPropertyBlock.SetFloat(int_6, m_FlareIntensity);
			materialPropertyBlock.SetTexture(int_7, source);
			materialPropertyBlock.SetTexture(int_8, flareRT);
			materialPropertyBlock.SetTexture(int_9, m_UseLensDust ? m_DustTexture : Texture2D.whiteTexture);
			materialPropertyBlock.SetTexture(int_10, temporary);
			if (m_UseLensDust)
			{
				materialPropertyBlock.SetFloat(int_11, m_DustIntensity);
				materialPropertyBlock.SetFloat(int_12, m_DirtLightIntensity);
			}
			else
			{
				materialPropertyBlock.SetFloat(int_11, 1f);
				materialPropertyBlock.SetFloat(int_12, 0f);
			}
			if (m_BlendMode == BlendMode.SCREEN)
			{
				materialPropertyBlock.SetFloat(int_13, m_ScreenMaxIntensity);
			}
		}
		else
		{
			material_2.SetFloat(int_5, m_BloomIntensity);
			material_2.SetFloat(int_6, m_FlareIntensity);
			material_2.SetTexture(int_7, source);
			material_2.SetTexture(int_8, flareRT);
			material_2.SetTexture(int_9, m_UseLensDust ? m_DustTexture : Texture2D.whiteTexture);
			material_2.SetTexture(int_10, temporary);
			if (m_UseLensDust)
			{
				material_2.SetFloat(int_11, m_DustIntensity);
				material_2.SetFloat(int_12, m_DirtLightIntensity);
			}
			else
			{
				material_2.SetFloat(int_11, 1f);
				material_2.SetFloat(int_12, 0f);
			}
			if (m_BlendMode == BlendMode.SCREEN)
			{
				material_2.SetFloat(int_13, m_ScreenMaxIntensity);
			}
		}
		if (useTriangleBlit)
		{
			if (m_InvertImage)
			{
				commandBuffer_0.BlitFullscreenTriangle(renderTexture_4, destination, material_2, 1, materialPropertyBlock);
			}
			else
			{
				commandBuffer_0.BlitFullscreenTriangle(renderTexture_4, destination, material_2, 0, materialPropertyBlock);
			}
		}
		else if (m_InvertImage)
		{
			DebugGraphics.Blit(renderTexture_4, destination, material_2, 1);
		}
		else
		{
			DebugGraphics.Blit(renderTexture_4, destination, material_2, 0);
		}
		for (int k = 0; k < renderTexture_3.Length; k++)
		{
			if (renderTexture_3[k] != null)
			{
				list_0.Add(renderTexture_3[k]);
			}
		}
		if (useTriangleBlit)
		{
			Graphics.ExecuteCommandBuffer(commandBuffer_0);
		}
		if (flag2)
		{
			Graphics.Blit(renderTexture, destination);
		}
		if ((m_UseLensFlare || m_UseAnamorphicFlare || m_UseStarFlare) && flareRT != null && flareRT is RenderTexture)
		{
			list_0.Add((RenderTexture)flareRT);
		}
		list_0.Add(temporary);
		if (m_FlareShape != null && m_UseBokehFlare && renderTexture != null)
		{
			list_0.Add(renderTexture);
		}
		if (!m_UseLensFlare && class500_0 != null)
		{
			class500_0.Clear(ref mesh_0);
		}
		foreach (RenderTexture item in list_0)
		{
			RenderTexture.ReleaseTemporary(item);
		}
		list_0.Clear();
		method_1();
	}

	public RenderTexture method_10(RenderTexture[] sources, BlurSampleCount upsamplingCount, int sourceWidth, int sourceHeight)
	{
		for (int num = renderTexture_1.Length - 1; num >= 0; num--)
		{
			renderTexture_1[num] = RenderTexture.GetTemporary(sourceWidth / (int)Mathf.Pow(2f, num), sourceHeight / (int)Mathf.Pow(2f, num), 0, renderTextureFormat_0);
			renderTexture_1[num].filterMode = FilterMode.Bilinear;
			float num2 = 1f / (float)sources[num].width;
			float num3 = 1f / (float)sources[num].height;
			if (num < m_StarDownscaleCount - 1)
			{
				method_20(sources[num], renderTexture_1[num], num2 * m_StarScale, num3 * m_StarScale, renderTexture_1[num + 1], upsamplingCount, Color.white, 1f);
			}
			else
			{
				method_20(sources[num], renderTexture_1[num], num2 * m_StarScale, num3 * m_StarScale, null, upsamplingCount, Color.white, 1f);
			}
		}
		for (int i = 1; i < renderTexture_1.Length; i++)
		{
			if (renderTexture_1[i] != null)
			{
				list_0.Add(renderTexture_1[i]);
			}
		}
		return renderTexture_1[0];
	}

	public void method_11(Texture source, RenderTexture destination)
	{
		if (useTriangleBlit)
		{
			commandBuffer_0.BlitFullscreenTriangle(source, destination);
		}
		else
		{
			Graphics.Blit(source, destination);
		}
	}

	public RenderTexture method_12(RenderTexture[] sources, BlurSampleCount upsamplingCount, int sourceWidth, int sourceHeight, FlareStripeType type)
	{
		RenderTexture[] array = renderTexture_0;
		bool[] array2 = m_AnamorphicBloomUsages;
		float[] array3 = m_AnamorphicBloomIntensities;
		Color[] array4 = m_AnamorphicBloomColors;
		bool flag = m_AnamorphicSmallVerticalBlur;
		float num = m_AnamorphicBlurPass;
		float num2 = m_AnamorphicScale;
		float num3 = m_AnamorphicFlareIntensity;
		float num4 = 1f;
		float num5 = 0f;
		if (m_AnamorphicDirection == AnamorphicDirection.Vertical)
		{
			num4 = 0f;
			num5 = 1f;
		}
		if (type != FlareStripeType.Anamorphic)
		{
			array = renderTexture_1;
			array2 = m_StarBloomUsages;
			array3 = m_StarBloomIntensities;
			array4 = m_StarBloomColors;
			flag = false;
			num = m_StarBlurPass;
			num2 = m_StarScale;
			num3 = m_StarFlareIntensity;
			num5 = ((type != FlareStripeType.DiagonalUpleft) ? 1f : (-1f));
		}
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = null;
		}
		RenderTexture renderTexture = null;
		for (int num6 = array.Length - 1; num6 >= 0; num6--)
		{
			if ((!(sources[num6] == null) || !m_DirectUpsample) && (array2[num6] || !m_DirectUpsample))
			{
				array[num6] = RenderTexture.GetTemporary(sourceWidth / (int)Mathf.Pow(2f, num6), sourceHeight / (int)Mathf.Pow(2f, num6), 0, renderTextureFormat_0);
				array[num6].filterMode = FilterMode.Bilinear;
				float num7 = 1f / (float)array[num6].width;
				float num8 = 1f / (float)array[num6].height;
				RenderTexture source = sources[num6];
				RenderTexture renderTexture2 = array[num6];
				if (!array2[num6])
				{
					if (renderTexture != null)
					{
						if (flag)
						{
							method_19(renderTexture, renderTexture2, (m_AnamorphicDirection == AnamorphicDirection.Vertical) ? num7 : 0f, (m_AnamorphicDirection == AnamorphicDirection.Horizontal) ? num8 : 0f, null, BlurSampleCount.FourSimple, Color.white, 1f);
						}
						else
						{
							method_11(renderTexture, renderTexture2);
						}
					}
					else
					{
						method_11(Texture2D.blackTexture, renderTexture2);
					}
					renderTexture = array[num6];
				}
				else
				{
					RenderTexture renderTexture3 = null;
					if (flag && renderTexture != null)
					{
						renderTexture3 = RenderTexture.GetTemporary(renderTexture2.width, renderTexture2.height, 0, renderTextureFormat_0);
						method_19(renderTexture, renderTexture3, (m_AnamorphicDirection == AnamorphicDirection.Vertical) ? num7 : 0f, (m_AnamorphicDirection == AnamorphicDirection.Horizontal) ? num8 : 0f, null, BlurSampleCount.FourSimple, Color.white, 1f);
						renderTexture = renderTexture3;
					}
					if (num == 1f)
					{
						if (type != FlareStripeType.Anamorphic)
						{
							method_20(source, renderTexture2, num7 * num2 * num4, num8 * num2 * num5, renderTexture, upsamplingCount, array4[num6], array3[num6] * num3);
						}
						else
						{
							method_19(source, renderTexture2, num7 * num2 * num4, num8 * num2 * num5, renderTexture, upsamplingCount, array4[num6], array3[num6] * num3);
						}
					}
					else
					{
						RenderTexture temporary = RenderTexture.GetTemporary(renderTexture2.width, renderTexture2.height, 0, renderTextureFormat_0);
						bool flag2 = false;
						for (int j = 0; (float)j < num; j++)
						{
							RenderTexture additiveTexture = (((float)j == num - 1f) ? renderTexture : null);
							if (j == 0)
							{
								if (type != FlareStripeType.Anamorphic)
								{
									method_20(source, temporary, num7 * num2 * num4, num8 * num2 * num5, additiveTexture, upsamplingCount, array4[num6], array3[num6] * num3);
								}
								else
								{
									method_19(source, temporary, num7 * num2 * num4, num8 * num2 * num5, additiveTexture, upsamplingCount, array4[num6], array3[num6] * num3);
								}
								continue;
							}
							num7 = 1f / (float)renderTexture2.width;
							num8 = 1f / (float)renderTexture2.height;
							if (j % 2 == 1)
							{
								if (type != FlareStripeType.Anamorphic)
								{
									method_20(temporary, renderTexture2, num7 * num2 * num4 * 1.5f, num8 * num2 * num5 * 1.5f, additiveTexture, upsamplingCount, array4[num6], array3[num6] * num3);
								}
								else
								{
									method_19(temporary, renderTexture2, num7 * num2 * num4 * 1.5f, num8 * num2 * num5 * 1.5f, additiveTexture, upsamplingCount, array4[num6], array3[num6] * num3);
								}
								flag2 = false;
							}
							else
							{
								if (type != FlareStripeType.Anamorphic)
								{
									method_20(renderTexture2, temporary, num7 * num2 * num4 * 1.5f, num8 * num2 * num5 * 1.5f, additiveTexture, upsamplingCount, array4[num6], array3[num6] * num3);
								}
								else
								{
									method_19(renderTexture2, temporary, num7 * num2 * num4 * 1.5f, num8 * num2 * num5 * 1.5f, additiveTexture, upsamplingCount, array4[num6], array3[num6] * num3);
								}
								flag2 = true;
							}
						}
						if (flag2)
						{
							method_11(temporary, renderTexture2);
						}
						if (renderTexture3 != null)
						{
							list_0.Add(renderTexture3);
						}
						list_0.Add(temporary);
					}
					renderTexture = array[num6];
				}
			}
		}
		RenderTexture renderTexture4 = null;
		for (int k = 0; k < array.Length; k++)
		{
			if (array[k] != null)
			{
				if (renderTexture4 == null)
				{
					renderTexture4 = array[k];
				}
				else
				{
					list_0.Add(array[k]);
				}
			}
		}
		return renderTexture4;
	}

	public void method_13(RenderTexture brightTexture, RenderTexture source, ref Texture flareRT)
	{
		flareRT = RenderTexture.GetTemporary(source.width, source.height, 0, renderTextureFormat_0);
		flareRT.filterMode = FilterMode.Bilinear;
		MaterialPropertyBlock materialPropertyBlock = null;
		if (useTriangleBlit)
		{
			materialPropertyBlock = method_0();
			materialPropertyBlock.Clear();
			materialPropertyBlock.SetVector(int_14, m_FlareScales * m_FlareGlobalScale);
			materialPropertyBlock.SetVector(int_15, m_FlareScalesNear * m_FlareGlobalScale);
			materialPropertyBlock.SetVector(int_16, m_FlareTint0);
			materialPropertyBlock.SetVector(int_17, m_FlareTint1);
			materialPropertyBlock.SetVector(int_18, m_FlareTint2);
			materialPropertyBlock.SetVector(int_19, m_FlareTint3);
			materialPropertyBlock.SetVector(int_20, m_FlareTint4);
			materialPropertyBlock.SetVector(int_21, m_FlareTint5);
			materialPropertyBlock.SetVector(int_22, m_FlareTint6);
			materialPropertyBlock.SetVector(int_23, m_FlareTint7);
			materialPropertyBlock.SetFloat(int_5, m_FlareIntensity);
		}
		else
		{
			material_0.SetVector(int_14, m_FlareScales * m_FlareGlobalScale);
			material_0.SetVector(int_15, m_FlareScalesNear * m_FlareGlobalScale);
			material_0.SetVector(int_16, m_FlareTint0);
			material_0.SetVector(int_17, m_FlareTint1);
			material_0.SetVector(int_18, m_FlareTint2);
			material_0.SetVector(int_19, m_FlareTint3);
			material_0.SetVector(int_20, m_FlareTint4);
			material_0.SetVector(int_21, m_FlareTint5);
			material_0.SetVector(int_22, m_FlareTint6);
			material_0.SetVector(int_23, m_FlareTint7);
			material_0.SetFloat(int_5, m_FlareIntensity);
		}
		if (m_FlareRendering == FlareRendering.Sharp)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0, renderTextureFormat_0);
			temporary.filterMode = FilterMode.Bilinear;
			method_18(brightTexture, temporary, 1f / (float)brightTexture.width, 1f / (float)brightTexture.height, SimpleSampleCount.Four, useTriangleBlit ? materialPropertyBlock : null);
			if (useTriangleBlit)
			{
				commandBuffer_0.BlitFullscreenTriangle(temporary, (RenderTexture)flareRT, material_5, 0, materialPropertyBlock);
			}
			else
			{
				DebugGraphics.Blit(temporary, (RenderTexture)flareRT, material_0, 0);
			}
			list_0.Add(temporary);
		}
		else if (m_FlareBlurQuality == FlareBlurQuality.Fast)
		{
			RenderTexture temporary2 = RenderTexture.GetTemporary(brightTexture.width / 2, brightTexture.height / 2, 0, renderTextureFormat_0);
			temporary2.filterMode = FilterMode.Bilinear;
			RenderTexture temporary3 = RenderTexture.GetTemporary(brightTexture.width / 4, brightTexture.height / 4, 0, renderTextureFormat_0);
			temporary3.filterMode = FilterMode.Bilinear;
			DebugGraphics.Blit(brightTexture, temporary2, material_0, 0);
			if (m_FlareRendering == FlareRendering.Blurred)
			{
				method_21(temporary2, temporary3, 1f / (float)temporary2.width, 1f / (float)temporary2.height, null, BlurSampleCount.Thirteen, Color.white, 1f, useTriangleBlit ? method_0() : null);
				method_18(temporary3, (RenderTexture)flareRT, 1f / (float)temporary3.width, 1f / (float)temporary3.height, SimpleSampleCount.Four, useTriangleBlit ? method_0() : null);
			}
			else if (m_FlareRendering == FlareRendering.MoreBlurred)
			{
				method_21(temporary2, temporary3, 1f / (float)temporary2.width, 1f / (float)temporary2.height, null, BlurSampleCount.ThrirtyOne, Color.white, 1f, useTriangleBlit ? method_0() : null);
				method_18(temporary3, (RenderTexture)flareRT, 1f / (float)temporary3.width, 1f / (float)temporary3.height, SimpleSampleCount.Four, useTriangleBlit ? method_0() : null);
			}
			list_0.Add(temporary2);
			list_0.Add(temporary3);
		}
		else if (m_FlareBlurQuality == FlareBlurQuality.Normal)
		{
			RenderTexture temporary4 = RenderTexture.GetTemporary(brightTexture.width / 2, brightTexture.height / 2, 0, renderTextureFormat_0);
			temporary4.filterMode = FilterMode.Bilinear;
			RenderTexture temporary5 = RenderTexture.GetTemporary(brightTexture.width / 4, brightTexture.height / 4, 0, renderTextureFormat_0);
			temporary5.filterMode = FilterMode.Bilinear;
			RenderTexture temporary6 = RenderTexture.GetTemporary(brightTexture.width / 4, brightTexture.height / 4, 0, renderTextureFormat_0);
			temporary6.filterMode = FilterMode.Bilinear;
			method_18(brightTexture, temporary4, 1f / (float)brightTexture.width, 1f / (float)brightTexture.height, SimpleSampleCount.Four, useTriangleBlit ? method_0() : null);
			method_18(temporary4, temporary5, 1f / (float)temporary4.width, 1f / (float)temporary4.height, SimpleSampleCount.Four, useTriangleBlit ? method_0() : null);
			DebugGraphics.Blit(temporary5, temporary6, material_0, 0);
			if (m_FlareRendering == FlareRendering.Blurred)
			{
				method_21(temporary6, temporary5, 1f / (float)temporary5.width, 1f / (float)temporary5.height, null, BlurSampleCount.Thirteen, Color.white, 1f, useTriangleBlit ? method_0() : null);
				method_18(temporary5, (RenderTexture)flareRT, 1f / (float)temporary5.width, 1f / (float)temporary5.height, SimpleSampleCount.Four, useTriangleBlit ? method_0() : null);
			}
			else if (m_FlareRendering == FlareRendering.MoreBlurred)
			{
				method_21(temporary6, temporary5, 1f / (float)temporary5.width, 1f / (float)temporary5.height, null, BlurSampleCount.ThrirtyOne, Color.white, 1f, useTriangleBlit ? method_0() : null);
				method_18(temporary5, (RenderTexture)flareRT, 1f / (float)temporary5.width, 1f / (float)temporary5.height, SimpleSampleCount.Four, useTriangleBlit ? method_0() : null);
			}
			list_0.Add(temporary4);
			list_0.Add(temporary5);
			list_0.Add(temporary6);
		}
		else if (m_FlareBlurQuality == FlareBlurQuality.High)
		{
			RenderTexture temporary7 = RenderTexture.GetTemporary(brightTexture.width / 2, brightTexture.height / 2, 0, renderTextureFormat_0);
			temporary7.filterMode = FilterMode.Bilinear;
			RenderTexture temporary8 = RenderTexture.GetTemporary(temporary7.width / 2, temporary7.height / 2, 0, renderTextureFormat_0);
			temporary8.filterMode = FilterMode.Bilinear;
			RenderTexture temporary9 = RenderTexture.GetTemporary(temporary8.width / 2, temporary8.height / 2, 0, renderTextureFormat_0);
			temporary9.filterMode = FilterMode.Bilinear;
			RenderTexture temporary10 = RenderTexture.GetTemporary(temporary8.width / 2, temporary8.height / 2, 0, renderTextureFormat_0);
			temporary10.filterMode = FilterMode.Bilinear;
			method_18(brightTexture, temporary7, 1f / (float)brightTexture.width, 1f / (float)brightTexture.height, SimpleSampleCount.Four, useTriangleBlit ? method_0() : null);
			method_18(temporary7, temporary8, 1f / (float)temporary7.width, 1f / (float)temporary7.height, SimpleSampleCount.Four, useTriangleBlit ? method_0() : null);
			method_18(temporary8, temporary9, 1f / (float)temporary8.width, 1f / (float)temporary8.height, SimpleSampleCount.Four, useTriangleBlit ? method_0() : null);
			DebugGraphics.Blit(temporary9, temporary10, material_0, 0);
			if (m_FlareRendering == FlareRendering.Blurred)
			{
				method_21(temporary10, temporary9, 1f / (float)temporary9.width, 1f / (float)temporary9.height, null, BlurSampleCount.Thirteen, Color.white, 1f, useTriangleBlit ? method_0() : null);
				method_18(temporary9, (RenderTexture)flareRT, 1f / (float)temporary9.width, 1f / (float)temporary9.height, SimpleSampleCount.Four, useTriangleBlit ? method_0() : null);
			}
			else if (m_FlareRendering == FlareRendering.MoreBlurred)
			{
				method_21(temporary10, temporary9, 1f / (float)temporary9.width, 1f / (float)temporary9.height, null, BlurSampleCount.ThrirtyOne, Color.white, 1f, useTriangleBlit ? method_0() : null);
				method_18(temporary9, (RenderTexture)flareRT, 1f / (float)temporary9.width, 1f / (float)temporary9.height, SimpleSampleCount.Four, useTriangleBlit ? method_0() : null);
			}
			list_0.Add(temporary7);
			list_0.Add(temporary8);
			list_0.Add(temporary9);
			list_0.Add(temporary10);
		}
	}

	public void method_14(RenderTexture[] sources, RenderTexture[] destinations, int originalWidth, int originalHeight, BlurSampleCount upsamplingCount)
	{
		RenderTexture renderTexture = null;
		for (int i = 0; i < renderTexture_3.Length; i++)
		{
			renderTexture_3[i] = null;
		}
		for (int num = destinations.Length - 1; num >= 0; num--)
		{
			if (m_BloomUsages[num] || !m_DirectUpsample)
			{
				renderTexture_3[num] = RenderTexture.GetTemporary(originalWidth / (int)Mathf.Pow(2f, num), originalHeight / (int)Mathf.Pow(2f, num), 0, renderTextureFormat_0);
				renderTexture_3[num].filterMode = FilterMode.Bilinear;
			}
			float num2 = 1f;
			if (m_BloomUsages[num])
			{
				float num3 = 1f / (float)sources[num].width;
				float verticalBlur = 1f / (float)sources[num].height;
				method_21(renderTexture_2[num], renderTexture_3[num], num3 * num2, verticalBlur, renderTexture, upsamplingCount, m_BloomColors[num], m_BloomIntensities[num], useTriangleBlit ? method_0() : null);
			}
			else if (num < m_DownscaleCount - 1)
			{
				if (!m_DirectUpsample)
				{
					method_18(renderTexture, renderTexture_3[num], 1f / (float)renderTexture_3[num].width, 1f / (float)renderTexture_3[num].height, SimpleSampleCount.Four, useTriangleBlit ? method_0() : null);
				}
			}
			else if (useTriangleBlit)
			{
				commandBuffer_0.BlitFullscreenTriangle(Texture2D.blackTexture, renderTexture_3[num]);
			}
			else
			{
				Graphics.Blit(Texture2D.blackTexture, renderTexture_3[num]);
			}
			if (m_BloomUsages[num] || !m_DirectUpsample)
			{
				renderTexture = renderTexture_3[num];
			}
		}
		renderTexture_4 = renderTexture;
	}

	public void method_15(RenderTexture source, RenderTexture[] destinations, DeluxeFilmicCurve intensityCurve, bool hdr)
	{
		int num = destinations.Length;
		RenderTexture renderTexture = source;
		bool flag = false;
		for (int i = 0; i < num; i++)
		{
			if (m_DirectDownSample && !bool_0[i])
			{
				continue;
			}
			destinations[i] = RenderTexture.GetTemporary(source.width / (int)Mathf.Pow(2f, i + 1), source.height / (int)Mathf.Pow(2f, i + 1), 0, renderTextureFormat_0);
			destinations[i].filterMode = FilterMode.Bilinear;
			RenderTexture destination = destinations[i];
			float num2 = 1f;
			float num3 = 1f / (float)renderTexture.width;
			float num4 = 1f / (float)renderTexture.height;
			MaterialPropertyBlock materialPropertyBlock = method_0();
			if (intensityCurve != null && !flag)
			{
				intensityCurve.StoreK();
				if (useTriangleBlit)
				{
					materialPropertyBlock.SetFloat(int_24, intensityCurve.GetExposure());
					materialPropertyBlock.SetFloat(int_25, intensityCurve.m_k);
					materialPropertyBlock.SetFloat(int_26, intensityCurve.m_CrossOverPoint);
					materialPropertyBlock.SetVector(int_27, intensityCurve.m_ToeCoef);
					materialPropertyBlock.SetVector(int_28, intensityCurve.m_ShoulderCoef);
				}
				else
				{
					material_1.SetFloat(int_24, intensityCurve.GetExposure());
					material_1.SetFloat(int_25, intensityCurve.m_k);
					material_1.SetFloat(int_26, intensityCurve.m_CrossOverPoint);
					material_1.SetVector(int_27, intensityCurve.m_ToeCoef);
					material_1.SetVector(int_28, intensityCurve.m_ShoulderCoef);
				}
				float value = (hdr ? 2f : 1f);
				if (useTriangleBlit)
				{
					materialPropertyBlock.SetFloat(int_29, value);
				}
				else
				{
					material_1.SetFloat(int_29, value);
				}
				num3 = 1f / (float)renderTexture.width;
				num4 = 1f / (float)renderTexture.height;
				if (m_TemporalStableDownsampling)
				{
					method_18(renderTexture, destination, num3 * num2, num4 * num2, SimpleSampleCount.ThirteenTemporalCurve, useTriangleBlit ? materialPropertyBlock : null);
				}
				else
				{
					method_18(renderTexture, destination, num3 * num2, num4 * num2, SimpleSampleCount.FourCurve, useTriangleBlit ? materialPropertyBlock : null);
				}
				flag = true;
			}
			else if (m_TemporalStableDownsampling)
			{
				method_18(renderTexture, destination, num3 * num2, num4 * num2, SimpleSampleCount.ThirteenTemporal, useTriangleBlit ? materialPropertyBlock : null);
			}
			else
			{
				method_18(renderTexture, destination, num3 * num2, num4 * num2, SimpleSampleCount.Four, useTriangleBlit ? materialPropertyBlock : null);
			}
			renderTexture = destinations[i];
		}
	}

	public void method_16(RenderTexture source, RenderTexture destination, Vector4 treshold, MaterialPropertyBlock props)
	{
		if (props != null)
		{
			props.SetTexture(int_4, Texture2D.whiteTexture);
			props.SetVector(int_30, treshold);
		}
		else
		{
			material_3.SetTexture(int_4, Texture2D.whiteTexture);
			material_3.SetVector(int_30, treshold);
		}
		if (useTriangleBlit)
		{
			commandBuffer_0.BlitFullscreenTriangle(source, destination, material_3, 0, props);
		}
		else
		{
			DebugGraphics.Blit(source, destination, material_3, 0);
		}
	}

	public void method_17(RenderTexture source, RenderTexture destination, Vector4 treshold, Texture mask, MaterialPropertyBlock props)
	{
		if (props != null)
		{
			props.SetTexture(int_4, mask);
			props.SetVector(int_30, treshold);
		}
		else
		{
			material_4.SetTexture(int_4, mask);
			material_4.SetVector(int_30, treshold);
		}
		if (useTriangleBlit)
		{
			commandBuffer_0.BlitFullscreenTriangle(source, destination, material_4, 0, props);
		}
		else
		{
			DebugGraphics.Blit(source, destination, material_4, 0);
		}
	}

	public void method_18(RenderTexture source, RenderTexture destination, float horizontalBlur, float verticalBlur, SimpleSampleCount sampleCount, MaterialPropertyBlock matBlock)
	{
		if (matBlock != null)
		{
			matBlock.SetVector(int_31, new Vector4(horizontalBlur, verticalBlur, 0f, 0f));
		}
		else
		{
			material_1.SetVector(int_31, new Vector4(horizontalBlur, verticalBlur, 0f, 0f));
		}
		if (matBlock != null)
		{
			switch (sampleCount)
			{
			case SimpleSampleCount.Four:
				commandBuffer_0.BlitFullscreenTriangle(source, destination, material_1, 0, matBlock);
				break;
			case SimpleSampleCount.Nine:
				commandBuffer_0.BlitFullscreenTriangle(source, destination, material_1, 1, matBlock);
				break;
			case SimpleSampleCount.FourCurve:
				commandBuffer_0.BlitFullscreenTriangle(source, destination, material_1, 5, matBlock);
				break;
			case SimpleSampleCount.ThirteenTemporal:
				commandBuffer_0.BlitFullscreenTriangle(source, destination, material_1, 11, matBlock);
				break;
			case SimpleSampleCount.ThirteenTemporalCurve:
				commandBuffer_0.BlitFullscreenTriangle(source, destination, material_1, 12, matBlock);
				break;
			}
		}
		else
		{
			switch (sampleCount)
			{
			case SimpleSampleCount.Four:
				DebugGraphics.Blit(source, destination, material_1, 0);
				break;
			case SimpleSampleCount.Nine:
				DebugGraphics.Blit(source, destination, material_1, 1);
				break;
			case SimpleSampleCount.FourCurve:
				DebugGraphics.Blit(source, destination, material_1, 5);
				break;
			case SimpleSampleCount.ThirteenTemporal:
				DebugGraphics.Blit(source, destination, material_1, 11);
				break;
			case SimpleSampleCount.ThirteenTemporalCurve:
				DebugGraphics.Blit(source, destination, material_1, 12);
				break;
			}
		}
	}

	public void method_19(RenderTexture source, RenderTexture destination, float horizontalBlur, float verticalBlur, RenderTexture additiveTexture, BlurSampleCount sampleCount, Color tint, float intensity)
	{
		int num = 2;
		if (sampleCount == BlurSampleCount.Seventeen)
		{
			num = 3;
		}
		if (sampleCount == BlurSampleCount.Nine)
		{
			num = 4;
		}
		if (sampleCount == BlurSampleCount.NineCurve)
		{
			num = 6;
		}
		if (sampleCount == BlurSampleCount.FourSimple)
		{
			num = 7;
		}
		if (sampleCount == BlurSampleCount.Thirteen)
		{
			num = 8;
		}
		if (sampleCount == BlurSampleCount.TwentyThree)
		{
			num = 9;
		}
		if (sampleCount == BlurSampleCount.TwentySeven)
		{
			num = 10;
		}
		Texture texture = null;
		texture = ((!(additiveTexture == null)) ? ((Texture)additiveTexture) : ((Texture)Texture2D.blackTexture));
		if (useTriangleBlit)
		{
			MaterialPropertyBlock materialPropertyBlock = method_0();
			materialPropertyBlock.SetTexture(int_9, texture);
			materialPropertyBlock.SetVector(int_31, new Vector4(horizontalBlur, verticalBlur, 0f, 0f));
			materialPropertyBlock.SetVector(int_32, tint);
			materialPropertyBlock.SetFloat(int_5, intensity);
			commandBuffer_0.BlitFullscreenTriangle(source, destination, material_1, num, materialPropertyBlock);
		}
		else
		{
			material_1.SetTexture(int_9, texture);
			material_1.SetVector(int_31, new Vector4(horizontalBlur, verticalBlur, 0f, 0f));
			material_1.SetVector(int_32, tint);
			material_1.SetFloat(int_5, intensity);
			DebugGraphics.Blit(source, destination, material_1, num);
		}
	}

	public void method_20(RenderTexture source, RenderTexture destination, float horizontalBlur, float verticalBlur, RenderTexture additiveTexture, BlurSampleCount sampleCount, Color tint, float intensity)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(destination.width, destination.height, destination.depth, destination.format);
		temporary.filterMode = FilterMode.Bilinear;
		int num = 2;
		if (sampleCount == BlurSampleCount.Seventeen)
		{
			num = 3;
		}
		if (sampleCount == BlurSampleCount.Nine)
		{
			num = 4;
		}
		if (sampleCount == BlurSampleCount.NineCurve)
		{
			num = 6;
		}
		if (sampleCount == BlurSampleCount.FourSimple)
		{
			num = 7;
		}
		if (sampleCount == BlurSampleCount.Thirteen)
		{
			num = 8;
		}
		if (sampleCount == BlurSampleCount.TwentyThree)
		{
			num = 9;
		}
		if (sampleCount == BlurSampleCount.TwentySeven)
		{
			num = 10;
		}
		Texture texture = null;
		texture = ((!(additiveTexture == null)) ? ((Texture)additiveTexture) : ((Texture)Texture2D.blackTexture));
		if (useTriangleBlit)
		{
			MaterialPropertyBlock materialPropertyBlock = method_0();
			materialPropertyBlock.SetTexture(int_9, texture);
			materialPropertyBlock.SetVector(int_31, new Vector4(horizontalBlur, verticalBlur, 0f, 0f));
			materialPropertyBlock.SetVector(int_32, tint);
			materialPropertyBlock.SetFloat(int_5, intensity);
			commandBuffer_0.BlitFullscreenTriangle(source, temporary, material_1, num, materialPropertyBlock);
		}
		else
		{
			material_1.SetTexture(int_9, texture);
			material_1.SetVector(int_31, new Vector4(horizontalBlur, verticalBlur, 0f, 0f));
			material_1.SetVector(int_32, tint);
			material_1.SetFloat(int_5, intensity);
			DebugGraphics.Blit(source, temporary, material_1, num);
		}
		texture = temporary;
		if (useTriangleBlit)
		{
			MaterialPropertyBlock materialPropertyBlock2 = method_0();
			materialPropertyBlock2.SetTexture(int_9, texture);
			materialPropertyBlock2.SetVector(int_31, new Vector4(0f - horizontalBlur, verticalBlur, 0f, 0f));
			materialPropertyBlock2.SetVector(int_32, tint);
			materialPropertyBlock2.SetFloat(int_5, intensity);
			commandBuffer_0.BlitFullscreenTriangle(source, destination, material_1, num, materialPropertyBlock2);
		}
		else
		{
			material_1.SetTexture(int_9, texture);
			material_1.SetVector(int_31, new Vector4(0f - horizontalBlur, verticalBlur, 0f, 0f));
			material_1.SetVector(int_32, tint);
			material_1.SetFloat(int_5, intensity);
			DebugGraphics.Blit(source, destination, material_1, num);
		}
		list_0.Add(temporary);
	}

	public void method_21(RenderTexture source, RenderTexture destination, float horizontalBlur, float verticalBlur, RenderTexture additiveTexture, BlurSampleCount sampleCount, Color tint, float intensity, MaterialPropertyBlock props)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(destination.width, destination.height, destination.depth, destination.format);
		temporary.filterMode = FilterMode.Bilinear;
		int num = 2;
		if (sampleCount == BlurSampleCount.Seventeen)
		{
			num = 3;
		}
		if (sampleCount == BlurSampleCount.Nine)
		{
			num = 4;
		}
		if (sampleCount == BlurSampleCount.NineCurve)
		{
			num = 6;
		}
		if (sampleCount == BlurSampleCount.FourSimple)
		{
			num = 7;
		}
		if (sampleCount == BlurSampleCount.Thirteen)
		{
			num = 8;
		}
		if (sampleCount == BlurSampleCount.TwentyThree)
		{
			num = 9;
		}
		if (sampleCount == BlurSampleCount.TwentySeven)
		{
			num = 10;
		}
		if (props != null)
		{
			props.SetTexture(int_9, Texture2D.blackTexture);
			props.SetVector(int_31, new Vector4(0f, verticalBlur, 0f, 0f));
			props.SetVector(int_32, tint);
			props.SetFloat(int_5, intensity);
			commandBuffer_0.BlitFullscreenTriangle(source, temporary, material_1, num, props);
		}
		else
		{
			material_1.SetTexture(int_9, Texture2D.blackTexture);
			material_1.SetVector(int_31, new Vector4(0f, verticalBlur, 0f, 0f));
			material_1.SetVector(int_32, tint);
			material_1.SetFloat(int_5, intensity);
			DebugGraphics.Blit(source, temporary, material_1, num);
		}
		Texture texture = null;
		texture = ((!(additiveTexture == null)) ? ((Texture)additiveTexture) : ((Texture)Texture2D.blackTexture));
		if (props != null)
		{
			props.SetTexture(int_9, texture);
			props.SetVector(int_31, new Vector4(horizontalBlur, 0f, 1f / (float)destination.width, 1f / (float)destination.height));
			props.SetVector(int_32, Color.white);
			props.SetFloat(int_5, 1f);
			commandBuffer_0.BlitFullscreenTriangle(temporary, destination, material_1, num, props);
		}
		else
		{
			material_1.SetTexture(int_9, texture);
			material_1.SetVector(int_31, new Vector4(horizontalBlur, 0f, 1f / (float)destination.width, 1f / (float)destination.height));
			material_1.SetVector(int_32, Color.white);
			material_1.SetFloat(int_5, 1f);
			DebugGraphics.Blit(temporary, destination, material_1, num);
		}
		list_0.Add(temporary);
	}

	public void method_22(RenderTexture source, RenderTexture destination, float intensity, MaterialPropertyBlock prop)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, source.depth, source.format);
		if (useTriangleBlit)
		{
			commandBuffer_0.BlitFullscreenTriangle(destination, temporary);
		}
		else
		{
			Graphics.Blit(destination, temporary);
		}
		if (prop != null)
		{
			prop.SetTexture(int_7, temporary);
			prop.SetFloat(int_5, intensity);
		}
		else
		{
			material_6.SetTexture(int_7, temporary);
			material_6.SetFloat(int_5, intensity);
		}
		if (useTriangleBlit)
		{
			commandBuffer_0.BlitFullscreenTriangle(source, destination, material_6, 0, prop);
		}
		else
		{
			DebugGraphics.Blit(source, destination, material_6, 0);
		}
		list_0.Add(temporary);
	}

	public void method_23(RenderTexture source, RenderTexture destination, float intensity)
	{
		material_6.SetFloat(int_5, intensity);
		DebugGraphics.Blit(source, destination, material_6, 2);
	}

	public void method_24(RenderTexture source, RenderTexture destination, float intensitySource, float intensityDestination)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, source.depth, source.format);
		method_11(destination, temporary);
		if (useTriangleBlit)
		{
			MaterialPropertyBlock materialPropertyBlock = method_0();
			materialPropertyBlock.SetTexture(int_7, temporary);
			materialPropertyBlock.SetFloat(int_33, intensitySource);
			materialPropertyBlock.SetFloat(int_34, intensityDestination);
			commandBuffer_0.BlitFullscreenTriangle(source, destination, material_6, 1, materialPropertyBlock);
		}
		else
		{
			material_6.SetTexture(int_7, temporary);
			material_6.SetFloat(int_33, intensitySource);
			material_6.SetFloat(int_34, intensityDestination);
			DebugGraphics.Blit(source, destination, material_6, 1);
		}
		list_0.Add(temporary);
	}

	public void SetFilmicCurveParameters(float middle, float dark, float bright, float highlights)
	{
		m_BloomCurve.m_ToeStrength = -1f * dark;
		m_BloomCurve.m_ShoulderStrength = bright;
		m_BloomCurve.m_Highlights = highlights;
		m_BloomCurve.m_CrossOverPoint = middle;
		m_BloomCurve.UpdateCoefficients();
	}

	public void OnDestroy()
	{
		commandBuffer_0?.Dispose();
		commandBuffer_0 = null;
	}
}
