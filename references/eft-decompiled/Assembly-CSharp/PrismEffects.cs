using System.Collections.Generic;
using EFT.BlitDebug;
using EFT.EnvironmentEffect;
using Prism.Utils;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("PRISM/Prism Effects")]
public class PrismEffects : MonoBehaviour
{
	public PrismPreset currentPrismPreset;

	private bool bool_0;

	public bool isParentPrism;

	public bool isChildPrism;

	private RenderTexture renderTexture_0;

	public Material m_Material;

	public Shader m_Shader;

	public Material m_Material2;

	public Shader m_Shader2;

	public PrismEffects m_MasterEffectExposure;

	private List<PrismEffects> list_0;

	public Material m_AOMaterial;

	public Shader m_AOShader;

	public Material m_Material3;

	public Shader m_Shader3;

	private Camera camera_0;

	private SSAA ssaa_0;

	public bool doBigPass = true;

	public bool useSeparableBlur = true;

	public Texture2D lensDirtTexture;

	public bool useLensDirt = true;

	[Space(10f)]
	public bool useBloom;

	public bool bloomUseScreenBlend;

	public BloomType bloomType = BloomType.HDR;

	public bool debugBloomTex;

	[Range(1f, 12f)]
	public int bloomDownsample = 2;

	[Range(0f, 12f)]
	public int bloomBlurPasses = 4;

	public float bloomIntensity = 0.15f;

	[Range(-2f, 2f)]
	public float bloomThreshold = 0.01f;

	public float dirtIntensity = 1f;

	public bool useBloomStability = true;

	private RenderTexture renderTexture_1;

	public bool useUIBlur;

	public int uiBlurGrabTextureFromPassNumber = 2;

	private RenderTexture renderTexture_2;

	[Space(10f)]
	public bool useVignette;

	public float vignetteStart = 0.9f;

	public float vignetteEnd = 0.4f;

	public float vignetteStrength = 1f;

	public Color vignetteColor = Color.black;

	[Space(10f)]
	public bool useNightVision;

	[SerializeField]
	[Tooltip("The main color of the NV effect")]
	public Color m_NVColor = new Color(0f, 1f, 0.1724138f, 0f);

	[SerializeField]
	[Tooltip("The color that the NV effect will 'bleach' towards (white = default)")]
	public Color m_TargetBleachColor = new Color(1f, 1f, 1f, 0f);

	[Range(0f, 0.1f)]
	[Tooltip("How much base lighting does the NV effect pick up")]
	public float m_baseLightingContribution = 0.025f;

	[Range(0f, 128f)]
	[Tooltip("The higher this value, the more bright areas will get 'bleached out'")]
	public float m_LightSensitivityMultiplier = 100f;

	[Space(10f)]
	public bool useNoise;

	public Texture2D noiseTexture;

	public float noiseScale = 1f;

	public float noiseIntensity = 0.2f;

	public NoiseType noiseType = NoiseType.RandomTimeNoise;

	[Space(10f)]
	public bool useChromaticAberration;

	public AberrationType aberrationType = AberrationType.Vertical;

	[Range(0f, 1f)]
	public float chromaticDistanceOne = 0.29f;

	[Range(0f, 1f)]
	public float chromaticDistanceTwo = 0.599f;

	public float chromaticIntensity = 0.03f;

	public float chromaticBlurWidth = 1f;

	public bool useChromaticBlur;

	[Space(10f)]
	public bool useTonemap;

	public TonemapType tonemapType = TonemapType.RomB;

	public Vector3 toneValues = new Vector3(-1f, 2.72f, 0.15f);

	public Vector3 secondaryToneValues = new Vector3(0.59f, 0.14f, 0.14f);

	public bool useExposure;

	public bool debugViewExposure;

	private RenderTexture renderTexture_3;

	public float exposureMiddleGrey = 0.12f;

	public float exposureLowerLimit = -6f;

	public float exposureUpperLimit = 6f;

	public float exposureSpeed = 6f;

	public int histWidth = 1;

	public int histHeight = 1;

	private bool bool_1 = true;

	public bool useGammaCorrection;

	public float gammaValue = 1f;

	[Space(10f)]
	public bool useDof;

	public bool dofForceEnableMedian;

	public bool useNearDofBlur;

	public bool useFullScreenBlur;

	public float dofNearFocusDistance = 15f;

	public float dofFocusPoint = 5f;

	public float dofFocusDistance = 15f;

	public float dofRadius = 0.6f;

	public float dofBokehFactor = 60f;

	public DoFSamples dofSampleAmount = DoFSamples.Medium;

	public bool dofBlurSkybox = true;

	public bool debugDofPass;

	public Transform dofFocusTransform;

	[Space(10f)]
	public bool useLut;

	public Texture2D twoDLookupTex;

	public Texture3D threeDLookupTex;

	public string basedOnTempTex = "";

	public float lutLerpAmount = 0.6f;

	public bool useSecondLut;

	public Texture2D secondaryTwoDLookupTex;

	public Texture3D secondaryThreeDLookupTex;

	public string secondaryBasedOnTempTex = "";

	public float secondaryLutLerpAmount;

	[Space(10f)]
	public bool useSharpen;

	public float sharpenAmount = 1f;

	public bool useFog;

	public bool fogAffectSkybox;

	public float fogIntensity;

	public float fogStartPoint = 50f;

	public float fogDistance = 170f;

	public Color fogColor = Color.white;

	public Color fogEndColor = Color.gray;

	public float fogHeight = 2f;

	public bool useAmbientObscurance;

	public SampleCount aoSampleCount = SampleCount.Low;

	public bool useAODistanceCutoff;

	public float aoDistanceCutoffLength = 50f;

	public float aoDistanceCutoffStart = 500f;

	public float aoIntensity = 0.7f;

	public float aoMinIntensity;

	public float aoRadius = 1f;

	public bool aoDownsample;

	public AOBlurType aoBlurType = AOBlurType.Fast;

	[Range(0f, 3f)]
	public int aoBlurIterations = 1;

	public float aoBias = 0.1f;

	public float aoBlurFilterDistance = 1.25f;

	public float aoLightingContribution = 1f;

	public bool aoShowDebug;

	public bool useRays;

	public Transform rayTransform;

	public float rayWeight = 0.58767f;

	public Color rayColor = Color.white;

	public Color rayThreshold = new Color(0.87f, 0.74f, 0.65f);

	public bool raysShowDebug;

	[Space(10f)]
	public bool advancedVignette;

	public bool advancedAO;

	private static readonly int int_0 = Shader.PropertyToID("_BloomIntensity");

	private static readonly int int_1 = Shader.PropertyToID("_BloomThreshold");

	private static readonly int int_2 = Shader.PropertyToID("_DirtIntensity");

	private static readonly int int_3 = Shader.PropertyToID("_FogIntensity");

	private static readonly int int_4 = Shader.PropertyToID("_VignetteStart");

	private static readonly int int_5 = Shader.PropertyToID("_VignetteEnd");

	private static readonly int int_6 = Shader.PropertyToID("_VignetteIntensity");

	private static readonly int int_7 = Shader.PropertyToID("_VignetteColor");

	private static readonly int int_8 = Shader.PropertyToID("_NVColor");

	private static readonly int int_9 = Shader.PropertyToID("_TargetWhiteColor");

	private static readonly int int_10 = Shader.PropertyToID("_BaseLightingContribution");

	private static readonly int int_11 = Shader.PropertyToID("_LightSensitivityMultiplier");

	private static readonly int int_12 = Shader.PropertyToID("_GrainIntensity");

	private static readonly int int_13 = Shader.PropertyToID("_GrainTex");

	private static readonly int int_14 = Shader.PropertyToID("_RandomInts");

	private static readonly int int_15 = Shader.PropertyToID("_ChromaticIntensity");

	private static readonly int int_16 = Shader.PropertyToID("_ChromaticParams");

	private static readonly int int_17 = Shader.PropertyToID("_ToneParams");

	private static readonly int int_18 = Shader.PropertyToID("_SecondaryToneParams");

	private static readonly int int_19 = Shader.PropertyToID("_Gamma");

	private static readonly int int_20 = Shader.PropertyToID("_SharpenAmount");

	private static readonly int int_21 = Shader.PropertyToID("_DirtTex");

	private static readonly int int_22 = Shader.PropertyToID("_ExposureLowerLimit");

	private static readonly int int_23 = Shader.PropertyToID("_ExposureUpperLimit");

	private static readonly int int_24 = Shader.PropertyToID("_ExposureMiddleGrey");

	private static readonly int int_25 = Shader.PropertyToID("_ExposureSpeed");

	private static readonly int int_26 = Shader.PropertyToID("useNoise");

	private static readonly int int_27 = Shader.PropertyToID("useNightVision");

	private static readonly int int_28 = Shader.PropertyToID("_SunWeight");

	private static readonly int int_29 = Shader.PropertyToID("_AOIntensity");

	private static readonly int int_30 = Shader.PropertyToID("_AOLuminanceWeighting");

	private static readonly int int_31 = Shader.PropertyToID("_AOSampleCount");

	private static readonly int int_32 = Shader.PropertyToID("_AOSpiralTurns");

	private static readonly int int_33 = Shader.PropertyToID("_SunColor");

	private static readonly int int_34 = Shader.PropertyToID("_SunThreshold");

	private static readonly int int_35 = Shader.PropertyToID("_SunPosition");

	private static readonly int int_36 = Shader.PropertyToID("_FogHeight");

	private static readonly int int_37 = Shader.PropertyToID("_FogDistance");

	private static readonly int int_38 = Shader.PropertyToID("_FogStart");

	private static readonly int int_39 = Shader.PropertyToID("_FogColor");

	private static readonly int int_40 = Shader.PropertyToID("_FogEndColor");

	private static readonly int int_41 = Shader.PropertyToID("_FogBlurSkybox");

	private static readonly int int_42 = Shader.PropertyToID("_InverseView");

	private static readonly int int_43 = Shader.PropertyToID("_AORadius");

	private static readonly int int_44 = Shader.PropertyToID("_AOBias");

	private static readonly int int_45 = Shader.PropertyToID("_AOTargetScale");

	private static readonly int int_46 = Shader.PropertyToID("_AOCutoff");

	private static readonly int int_47 = Shader.PropertyToID("_AOCutoffRange");

	private static readonly int int_48 = Shader.PropertyToID("_AOCameraModelView");

	private static readonly int int_49 = Shader.PropertyToID("_AOProjInfo");

	private static readonly int int_50 = Shader.PropertyToID("_LutScale");

	private static readonly int int_51 = Shader.PropertyToID("_LutOffset");

	private static readonly int int_52 = Shader.PropertyToID("_LutTex");

	private static readonly int int_53 = Shader.PropertyToID("_LutAmount");

	private static readonly int int_54 = Shader.PropertyToID("_SecondLutScale");

	private static readonly int int_55 = Shader.PropertyToID("_SecondLutOffset");

	private static readonly int int_56 = Shader.PropertyToID("_SecondLutTex");

	private static readonly int int_57 = Shader.PropertyToID("_SecondLutAmount");

	private static readonly int int_58 = Shader.PropertyToID("_DofFocusPoint");

	private static readonly int int_59 = Shader.PropertyToID("_DofFocusDistance");

	private static readonly int int_60 = Shader.PropertyToID("_DofRadius");

	private static readonly int int_61 = Shader.PropertyToID("_DofFactor");

	private static readonly int int_62 = Shader.PropertyToID("_DofUseNearBlur");

	private static readonly int int_63 = Shader.PropertyToID("_DofNearFocusDistance");

	private static readonly int int_64 = Shader.PropertyToID("_DofBlurSkybox");

	private static readonly int int_65 = Shader.PropertyToID("_BrightnessTexture");

	private static readonly int int_66 = Shader.PropertyToID("_BlurVector");

	private static readonly int int_67 = Shader.PropertyToID("_LastCameraDepthTexture1");

	private static readonly int int_68 = Shader.PropertyToID("_DoF1");

	private static readonly int int_69 = Shader.PropertyToID("_DofUseLerp");

	private static readonly int int_70 = Shader.PropertyToID("currentIteration");

	private static readonly int int_71 = Shader.PropertyToID("_BlurredScreenTex");

	private static readonly int int_72 = Shader.PropertyToID("_Bloom1");

	private static readonly int int_73 = Shader.PropertyToID("_BloomAcc");

	private static readonly int int_74 = Shader.PropertyToID("_Bloom2");

	private static readonly int int_75 = Shader.PropertyToID("_Bloom3");

	private static readonly int int_76 = Shader.PropertyToID("_Bloom4");

	private static readonly int int_77 = Shader.PropertyToID("_AOBlurVector");

	private static readonly int int_78 = Shader.PropertyToID("_AOTex");

	private static readonly int int_79 = Shader.PropertyToID("_RaysTexture");

	private bool bool_2;

	private SSAAPropagator ssaapropagator_0;

	public bool forceSecondChromaticPass
	{
		get
		{
			if ((useDof && useChromaticAberration) || useChromaticBlur)
			{
				return useChromaticAberration;
			}
			return false;
		}
	}

	public RenderTexture AdaptationTexture => renderTexture_3;

	public bool useMedianDoF
	{
		get
		{
			if (!useBloom && !useExposure && !useDof)
			{
				return false;
			}
			return dofForceEnableMedian;
		}
	}

	public RenderTextureFormat RenderTextureFormat_0
	{
		get
		{
			if (bool_2)
			{
				return RenderTextureFormat.R8;
			}
			return RenderTextureFormat.ARGB32;
		}
	}

	public int aoSampleCountValue
	{
		get
		{
			return aoSampleCount switch
			{
				SampleCount.High => 18, 
				SampleCount.Medium => 14, 
				SampleCount.Low => 10, 
				_ => Mathf.Clamp((int)aoSampleCount, 1, 256), 
			};
		}
		set
		{
			aoSampleCount = (SampleCount)value;
		}
	}

	public bool UsingTerrain
	{
		get
		{
			if ((bool)Terrain.activeTerrain)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsGBufferAvailable => camera_0.actualRenderingPath == RenderingPath.DeferredShading;

	public void Awake()
	{
		ssaapropagator_0 = GetComponent<SSAAPropagator>();
	}

	public void DontRenderPrismThisFrame()
	{
		bool_0 = true;
	}

	public Camera GetPrismCamera()
	{
		if (camera_0 == null)
		{
			camera_0 = GetComponent<Camera>();
			ssaa_0 = GetComponent<SSAA>();
		}
		return camera_0;
	}

	public void ResetToneParamsRomB()
	{
		toneValues = new Vector3(-1f, 2.72f, 0.15f);
		secondaryToneValues = new Vector3(0f, 0f, 0f);
	}

	public void ResetToneParamsFilmic()
	{
		toneValues = new Vector3(6.2f, 0.5f, 1.7f);
		secondaryToneValues = new Vector3(0.004f, 0.06f, 0f);
	}

	public void ResetToneParamsACES()
	{
		toneValues = new Vector3(2.51f, 0.03f, 2.43f);
		secondaryToneValues = new Vector3(0.59f, 0.14f, 0f);
	}

	public void SetGamma(float value)
	{
		gammaValue = value;
	}

	public void SetChromaticIntensity(float value)
	{
		chromaticIntensity = value;
	}

	public void SetNoiseIntensity(float value)
	{
		noiseIntensity = value;
	}

	public void SetVignetteStrength(float value)
	{
		vignetteStrength = value;
	}

	public void SetPrimaryLutStrength(float value)
	{
		lutLerpAmount = value;
	}

	public void SetSecondaryLutStrength(float value)
	{
		secondaryLutLerpAmount = value;
	}

	public void SetPrismPreset(PrismPreset preset)
	{
		if (!preset)
		{
			useBloom = false;
			useDof = false;
			useChromaticAberration = false;
			useVignette = false;
			useNoise = false;
			useTonemap = false;
			useFog = false;
			useNightVision = false;
			useExposure = false;
			useLut = false;
			useSecondLut = false;
			useGammaCorrection = false;
			useAmbientObscurance = false;
			useRays = false;
			useUIBlur = false;
			return;
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.Bloom)
		{
			useBloom = preset.useBloom;
			bloomType = preset.bloomType;
			bloomDownsample = preset.bloomDownsample;
			bloomBlurPasses = preset.bloomBlurPasses;
			bloomIntensity = preset.bloomIntensity;
			bloomThreshold = preset.bloomThreshold;
			useBloomStability = preset.useBloomStability;
			bloomUseScreenBlend = preset.bloomUseScreenBlend;
			useLensDirt = preset.useBloomLensdirt;
			dirtIntensity = preset.bloomLensdirtIntensity;
			lensDirtTexture = preset.bloomLensdirtTexture;
			useFullScreenBlur = preset.useFullScreenBlur;
			useUIBlur = preset.useUIBlur;
			uiBlurGrabTextureFromPassNumber = preset.uiBlurGrabTextureFromPassNumber;
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.DepthOfField)
		{
			useDof = preset.useDoF;
			dofRadius = preset.dofRadius;
			dofSampleAmount = preset.dofSampleCount;
			dofBokehFactor = preset.dofBokehFactor;
			dofFocusPoint = preset.dofFocusPoint;
			dofFocusDistance = preset.dofFocusDistance;
			useNearDofBlur = preset.useNearBlur;
			dofBlurSkybox = preset.dofBlurSkybox;
			dofNearFocusDistance = preset.dofNearFocusDistance;
			dofForceEnableMedian = preset.dofForceEnableMedian;
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.ChromaticAberration)
		{
			useChromaticAberration = preset.useChromaticAb;
			aberrationType = preset.aberrationType;
			chromaticIntensity = preset.chromIntensity;
			chromaticDistanceOne = preset.chromStart;
			chromaticDistanceTwo = preset.chromEnd;
			useChromaticBlur = preset.useChromaticBlur;
			chromaticBlurWidth = preset.chromaticBlurWidth;
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.Vignette)
		{
			useVignette = preset.useVignette;
			vignetteStrength = preset.vignetteIntensity;
			vignetteEnd = preset.vignetteEnd;
			vignetteStart = preset.vignetteStart;
			vignetteColor = preset.vignetteColor;
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.Noise)
		{
			useNoise = preset.useNoise;
			noiseIntensity = preset.noiseIntensity;
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.Tonemap)
		{
			useTonemap = preset.useTonemap;
			tonemapType = preset.toneType;
			toneValues = preset.toneValues;
			secondaryToneValues = preset.secondaryToneValues;
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.Exposure)
		{
			useExposure = preset.useExposure;
			exposureMiddleGrey = preset.exposureMiddleGrey;
			exposureLowerLimit = preset.exposureLowerLimit;
			exposureUpperLimit = preset.exposureUpperLimit;
			exposureSpeed = preset.exposureSpeed;
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.Gamma)
		{
			useGammaCorrection = preset.useGammaCorrection;
			gammaValue = preset.gammaValue;
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.ColorCorrection)
		{
			useLut = preset.useLUT;
			lutLerpAmount = preset.lutIntensity;
			basedOnTempTex = preset.lutPath;
			twoDLookupTex = preset.twoDLookupTex;
			useSecondLut = preset.useSecondLut;
			secondaryLutLerpAmount = preset.secondaryLutLerpAmount;
			secondaryBasedOnTempTex = preset.secondaryLutPath;
			secondaryTwoDLookupTex = preset.secondaryTwoDLookupTex;
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.Nightvision)
		{
			useNightVision = preset.useNV;
			m_NVColor = preset.nvColor;
			m_TargetBleachColor = preset.nvBleachColor;
			m_baseLightingContribution = preset.nvLightingContrib;
			m_LightSensitivityMultiplier = preset.nvLightSensitivity;
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.Fog)
		{
			useFog = preset.useFog;
			fogIntensity = preset.fogIntensity;
			fogStartPoint = preset.fogStartPoint;
			fogDistance = preset.fogDistance;
			fogColor = preset.fogColor;
			fogEndColor = preset.fogEndColor;
			fogHeight = preset.fogHeight;
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.AmbientObscurance)
		{
			useAmbientObscurance = preset.useAmbientObscurance;
			useAODistanceCutoff = preset.useAODistanceCutoff;
			aoIntensity = preset.aoIntensity;
			aoRadius = preset.aoRadius;
			aoDistanceCutoffStart = preset.aoDistanceCutoffStart;
			aoDownsample = preset.aoDownsample;
			aoBlurIterations = preset.aoBlurIterations;
			aoDistanceCutoffLength = preset.aoDistanceCutoffLength;
			aoSampleCount = preset.aoSampleCount;
			aoBias = preset.aoBias;
			aoBlurFilterDistance = preset.aoBlurFilterDistance;
			aoBlurType = preset.aoBlurType;
			aoLightingContribution = preset.aoLightingContribution;
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.Godrays)
		{
			useRays = preset.useRays;
			rayWeight = preset.rayWeight;
			rayColor = preset.rayColor;
			rayThreshold = preset.rayThreshold;
		}
		if (preset.presetType == PrismPresetType.Full || !currentPrismPreset)
		{
			currentPrismPreset = preset;
		}
		Reset();
	}

	public void LerpToPreset(PrismPreset preset, float t)
	{
		t = Mathf.Clamp(t, 0f, 1f);
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.Bloom)
		{
			bloomDownsample = (int)Mathf.Lerp(currentPrismPreset.bloomDownsample, preset.bloomDownsample, t);
			bloomBlurPasses = (int)Mathf.Lerp(currentPrismPreset.bloomBlurPasses, preset.bloomBlurPasses, t);
			bloomIntensity = Mathf.Lerp(currentPrismPreset.bloomIntensity, preset.bloomIntensity, t);
			bloomThreshold = Mathf.Lerp(currentPrismPreset.bloomThreshold, preset.bloomThreshold, t);
			dirtIntensity = Mathf.Lerp(currentPrismPreset.bloomLensdirtIntensity, preset.bloomLensdirtIntensity, t);
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.DepthOfField)
		{
			dofRadius = Mathf.Lerp(currentPrismPreset.dofRadius, preset.dofRadius, t);
			dofBokehFactor = Mathf.Lerp(currentPrismPreset.dofBokehFactor, preset.dofBokehFactor, t);
			dofFocusPoint = Mathf.Lerp(currentPrismPreset.dofFocusPoint, preset.dofFocusPoint, t);
			dofFocusDistance = Mathf.Lerp(currentPrismPreset.dofFocusDistance, preset.dofFocusDistance, t);
			dofNearFocusDistance = Mathf.Lerp(currentPrismPreset.dofNearFocusDistance, preset.dofNearFocusDistance, t);
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.ChromaticAberration)
		{
			chromaticIntensity = Mathf.Lerp(currentPrismPreset.chromIntensity, preset.chromIntensity, t);
			chromaticDistanceOne = Mathf.Lerp(currentPrismPreset.chromStart, preset.chromStart, t);
			chromaticDistanceTwo = Mathf.Lerp(currentPrismPreset.chromEnd, preset.chromEnd, t);
			chromaticBlurWidth = Mathf.Lerp(currentPrismPreset.chromaticBlurWidth, preset.chromaticBlurWidth, t);
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.Vignette)
		{
			vignetteStrength = Mathf.Lerp(currentPrismPreset.vignetteIntensity, preset.vignetteIntensity, t);
			vignetteEnd = Mathf.Lerp(currentPrismPreset.vignetteEnd, preset.vignetteEnd, t);
			vignetteStart = Mathf.Lerp(currentPrismPreset.vignetteStart, preset.vignetteStart, t);
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.Noise)
		{
			noiseIntensity = Mathf.Lerp(currentPrismPreset.noiseIntensity, preset.noiseIntensity, t);
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.Tonemap)
		{
			toneValues = Vector3.Lerp(currentPrismPreset.toneValues, preset.toneValues, t);
			secondaryToneValues = Vector3.Lerp(currentPrismPreset.secondaryToneValues, preset.secondaryToneValues, t);
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.Exposure)
		{
			exposureMiddleGrey = Mathf.Lerp(currentPrismPreset.exposureMiddleGrey, preset.exposureMiddleGrey, t);
			exposureLowerLimit = Mathf.Lerp(currentPrismPreset.exposureLowerLimit, preset.exposureLowerLimit, t);
			exposureUpperLimit = Mathf.Lerp(currentPrismPreset.exposureUpperLimit, preset.exposureUpperLimit, t);
			exposureSpeed = Mathf.Lerp(currentPrismPreset.exposureSpeed, preset.exposureSpeed, t);
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.Gamma)
		{
			gammaValue = Mathf.Lerp(currentPrismPreset.gammaValue, preset.gammaValue, t);
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.ColorCorrection)
		{
			lutLerpAmount = Mathf.Lerp(currentPrismPreset.lutIntensity, preset.lutIntensity, t);
			secondaryLutLerpAmount = Mathf.Lerp(currentPrismPreset.secondaryLutLerpAmount, preset.secondaryLutLerpAmount, t);
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.Fog)
		{
			fogIntensity = Mathf.Lerp(currentPrismPreset.fogIntensity, preset.fogIntensity, t);
			fogStartPoint = Mathf.Lerp(currentPrismPreset.fogStartPoint, preset.fogStartPoint, t);
			fogDistance = Mathf.Lerp(currentPrismPreset.fogDistance, preset.fogDistance, t);
			fogColor = Color.Lerp(currentPrismPreset.fogColor, preset.fogColor, t);
			fogEndColor = Color.Lerp(currentPrismPreset.fogEndColor, preset.fogEndColor, t);
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.AmbientObscurance)
		{
			aoDistanceCutoffStart = Mathf.Lerp(currentPrismPreset.aoDistanceCutoffStart, preset.aoDistanceCutoffStart, t);
			aoIntensity = Mathf.Lerp(currentPrismPreset.aoIntensity, preset.aoIntensity, t);
			aoRadius = Mathf.Lerp(currentPrismPreset.aoRadius, preset.aoRadius, t);
			aoBias = Mathf.Lerp(currentPrismPreset.aoBias, preset.aoBias, t);
			aoDistanceCutoffLength = Mathf.Lerp(currentPrismPreset.aoDistanceCutoffLength, preset.aoDistanceCutoffLength, t);
			aoBlurIterations = (int)Mathf.Lerp(currentPrismPreset.aoBlurIterations, preset.aoBlurIterations, t);
			aoLightingContribution = (int)Mathf.Lerp(currentPrismPreset.aoLightingContribution, preset.aoLightingContribution, t);
		}
		if (preset.presetType == PrismPresetType.Full || preset.presetType == PrismPresetType.Godrays)
		{
			rayWeight = Mathf.Lerp(currentPrismPreset.rayWeight, preset.rayWeight, t);
			rayColor = Color.Lerp(currentPrismPreset.rayColor, preset.rayColor, t);
			rayThreshold = Color.Lerp(currentPrismPreset.rayThreshold, preset.rayThreshold, t);
		}
	}

	public void OnEnable()
	{
		bool_2 = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8);
		camera_0 = GetComponent<Camera>();
		ssaa_0 = GetComponent<SSAA>();
		if (!m_Shader)
		{
			m_Shader = GClass872.Find("Hidden/PrismEffects");
			if (!m_Shader)
			{
				Debug.LogError("Couldn't find shader for PRISM! You shouldn't see this error.");
			}
		}
		if (!m_Shader2)
		{
			m_Shader2 = GClass872.Find("Hidden/PrismEffectsSecondary");
			if (!m_Shader2)
			{
				Debug.LogError("Couldn't find secondary shader for PRISM! You shouldn't see this error.");
			}
		}
		if (!m_Shader3)
		{
			m_Shader3 = GClass872.Find("Hidden/PrismEffectsTertiary");
			if (!m_Shader3)
			{
				Debug.LogError("Couldn't find tertiary shader for PRISM! You shouldn't see this error.");
			}
		}
		if (!m_AOShader)
		{
			m_AOShader = GClass872.Find("Hidden/PrismKinoObscurance");
			if (!m_AOShader)
			{
				Debug.LogError("Couldn't find ao shader for PRISM! You shouldn't see this error.");
			}
		}
		if (!renderTexture_3)
		{
			renderTexture_3 = new RenderTexture(histWidth, histHeight, 0, RenderTextureFormat.ARGB32)
			{
				name = "Current Adaptation Tex"
			};
			renderTexture_3.filterMode = FilterMode.Bilinear;
			renderTexture_3.autoGenerateMips = false;
		}
		if (useUIBlur)
		{
			int num = (ssaa_0 ? ssaa_0.GetInputWidth() : Screen.width);
			int num2 = (ssaa_0 ? ssaa_0.GetInputHeight() : Screen.height);
			renderTexture_2 = new RenderTexture(num / bloomDownsample, num2 / bloomDownsample, 0, RenderTextureFormat.ARGB32);
			renderTexture_2.name = "UI Blur Tex";
			renderTexture_3.filterMode = FilterMode.Bilinear;
			renderTexture_3.autoGenerateMips = false;
		}
		if (useDof || useFog)
		{
			camera_0.depthTextureMode |= DepthTextureMode.Depth;
		}
		if (useAmbientObscurance && (!IsGBufferAvailable || UsingTerrain))
		{
			camera_0.depthTextureMode |= DepthTextureMode.Depth;
			camera_0.depthTextureMode |= DepthTextureMode.DepthNormals;
		}
		bool_1 = true;
	}

	[ContextMenu("DontRenderDepthTexture")]
	public void method_0()
	{
		camera_0.depthTextureMode = DepthTextureMode.None;
	}

	public void OnDestroy()
	{
		if (list_0 != null)
		{
			foreach (PrismEffects item in list_0)
			{
				item.m_MasterEffectExposure = null;
			}
		}
		if ((bool)threeDLookupTex)
		{
			Object.DestroyImmediate(threeDLookupTex);
		}
		threeDLookupTex = null;
		basedOnTempTex = "";
		twoDLookupTex = null;
		if ((bool)secondaryThreeDLookupTex)
		{
			Object.DestroyImmediate(secondaryThreeDLookupTex);
		}
		secondaryThreeDLookupTex = null;
		secondaryBasedOnTempTex = "";
		secondaryTwoDLookupTex = null;
		ssaa_0 = null;
	}

	public void OnDisable()
	{
		if ((bool)m_Material)
		{
			Object.DestroyImmediate(m_Material);
			m_Material = null;
		}
		if ((bool)m_Material2)
		{
			Object.DestroyImmediate(m_Material2);
			m_Material2 = null;
		}
		if ((bool)m_Material3)
		{
			Object.DestroyImmediate(m_Material3);
			m_Material3 = null;
		}
		if ((bool)m_AOMaterial)
		{
			Object.DestroyImmediate(m_AOMaterial);
			m_AOMaterial = null;
		}
		if (m_AOShader == m_Shader || m_Shader2 == m_Shader || m_Shader3 == m_Shader)
		{
			m_AOShader = null;
			m_Shader2 = null;
			m_Shader3 = null;
		}
		if ((bool)threeDLookupTex)
		{
			Object.DestroyImmediate(threeDLookupTex);
			basedOnTempTex = "";
		}
		if ((bool)secondaryThreeDLookupTex)
		{
			Object.DestroyImmediate(secondaryThreeDLookupTex);
			secondaryBasedOnTempTex = "";
		}
		if ((bool)renderTexture_3)
		{
			Object.DestroyImmediate(renderTexture_3);
			renderTexture_3 = null;
		}
		if ((bool)renderTexture_2)
		{
			Object.DestroyImmediate(renderTexture_2);
			renderTexture_2 = null;
		}
	}

	public Material method_1(Shader shader)
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

	public void Reset()
	{
		OnDisable();
		OnEnable();
	}

	public bool method_2()
	{
		if (m_Material == null && m_Shader != null && m_Shader.isSupported)
		{
			m_Material = method_1(m_Shader);
		}
		if (m_Material2 == null && m_Shader2 != null && m_Shader2.isSupported)
		{
			m_Material2 = method_1(m_Shader2);
		}
		if (m_Material3 == null && m_Shader3 != null && m_Shader3.isSupported)
		{
			m_Material3 = method_1(m_Shader3);
		}
		if (m_AOMaterial == null && m_AOShader != null && m_AOShader.isSupported)
		{
			m_AOMaterial = method_1(m_AOShader);
		}
		if (!m_Shader.isSupported)
		{
			Debug.LogError("Prism is not supported on this platform, or you have a shader compilation error somewhere. Disabling.");
			base.enabled = false;
			return false;
		}
		if (!m_Shader2.isSupported)
		{
			Debug.LogError("Prism (secondary shader) is not supported on this platform. Disabling.");
			base.enabled = false;
			return false;
		}
		if (!m_Shader3.isSupported)
		{
			Debug.LogError("Prism (tertiary shader) is not supported on this platform. Disabling some effects.");
			m_Shader3 = m_Shader;
			useRays = false;
		}
		if (!m_AOShader.isSupported)
		{
			Debug.LogError("Prism (AO) is not supported on this platform, or you have a shader compilation error somewhere. Disabling AO.");
			m_AOShader = m_Shader;
			useAmbientObscurance = false;
		}
		if (useLut && threeDLookupTex == null && (bool)twoDLookupTex)
		{
			Convert(twoDLookupTex);
			if (threeDLookupTex == null)
			{
				useLut = false;
				Debug.LogWarning("No LUT found - disabling LookupTexture");
			}
		}
		if (useLut && useSecondLut && secondaryThreeDLookupTex == null && (bool)secondaryTwoDLookupTex)
		{
			Convert(secondaryTwoDLookupTex, secondaryLut: true);
			if (secondaryThreeDLookupTex == null)
			{
				useSecondLut = false;
				Debug.LogWarning("No secondary LUT found - disabling secondary LookupTexture");
			}
		}
		return true;
	}

	public void UpdateShaderValues()
	{
		if (m_Material == null)
		{
			return;
		}
		m_Material.shaderKeywords = null;
		m_Material2.shaderKeywords = null;
		m_AOMaterial.shaderKeywords = null;
		if (useBloom)
		{
			if (bloomType == BloomType.Simple)
			{
				Shader.DisableKeyword("PRISM_HDR_BLOOM");
				Shader.EnableKeyword("PRISM_SIMPLE_BLOOM");
				m_Material2.SetFloat(int_0, bloomIntensity);
				m_Material2.SetFloat(int_1, bloomThreshold);
				m_Material.SetFloat(int_0, bloomIntensity);
				m_Material.SetFloat(int_1, bloomThreshold);
				if (!camera_0.allowHDR)
				{
					Shader.EnableKeyword("PRISM_BLOOM_SCREENBLEND");
				}
				else
				{
					Shader.DisableKeyword("PRISM_BLOOM_SCREENBLEND");
				}
			}
			else if (bloomType == BloomType.HDR)
			{
				Shader.DisableKeyword("PRISM_SIMPLE_BLOOM");
				Shader.EnableKeyword("PRISM_HDR_BLOOM");
				Shader.DisableKeyword("PRISM_BLOOM_SCREENBLEND");
				m_Material.SetFloat(int_1, bloomThreshold);
				m_Material2.SetFloat(int_1, bloomThreshold);
				m_Material2.SetFloat(int_0, bloomIntensity);
				m_Material.SetFloat(int_0, bloomIntensity);
			}
			float value = dirtIntensity * dirtIntensity;
			m_Material.SetFloat(int_2, value);
			m_Material2.SetFloat(int_2, value);
			if (useUIBlur)
			{
				Shader.EnableKeyword("PRISM_UIBLUR");
			}
			else
			{
				Shader.DisableKeyword("PRISM_UIBLUR");
			}
		}
		else
		{
			Shader.DisableKeyword("PRISM_HDR_BLOOM");
			Shader.EnableKeyword("PRISM_SIMPLE_BLOOM");
			Shader.DisableKeyword("PRISM_BLOOM_SCREENBLEND");
			Shader.DisableKeyword("PRISM_USE_STABLEBLOOM");
			Shader.DisableKeyword("PRISM_USE_BLOOM");
			Shader.DisableKeyword("PRISM_UIBLUR");
		}
		method_3();
		if (useFog)
		{
			method_5(m_Material);
		}
		else
		{
			m_Material.SetFloat(int_3, 0f);
		}
		if (useVignette)
		{
			m_Material.SetFloat(int_4, vignetteStart);
			m_Material.SetFloat(int_5, vignetteEnd);
			m_Material.SetFloat(int_6, vignetteStrength);
			m_Material.SetColor(int_7, vignetteColor);
		}
		else
		{
			m_Material.SetFloat(int_6, 0f);
		}
		if (useNightVision)
		{
			m_Material.SetVector(int_8, m_NVColor);
			m_Material.SetVector(int_9, m_TargetBleachColor);
			m_Material.SetFloat(int_10, m_baseLightingContribution);
			m_Material.SetFloat(int_11, m_LightSensitivityMultiplier);
		}
		if (useNoise)
		{
			new Vector2(noiseIntensity * 0.01f, noiseIntensity);
			m_Material.SetVector(int_12, new Vector2(noiseIntensity * 0.01f, noiseIntensity));
			m_Material.SetTexture(int_13, noiseTexture);
			Vector2 vector = new Vector2(0.5f - Random.value, 0.5f - Random.value);
			m_Material.SetVector(int_14, vector);
		}
		if (useChromaticAberration)
		{
			float num = 0f;
			float value2 = chromaticIntensity * chromaticIntensity;
			num = ((aberrationType != AberrationType.Vertical) ? 0f : 1f);
			m_Material.SetFloat(int_15, value2);
			m_Material2.SetFloat(int_15, value2);
			m_Material.SetVector(int_16, new Vector4(chromaticDistanceOne, chromaticDistanceTwo, num, 0f));
			m_Material2.SetVector(int_16, new Vector4(chromaticDistanceOne, chromaticDistanceTwo, num, 0f));
		}
		else
		{
			m_Material.SetFloat(int_15, 0f);
		}
		if (useTonemap)
		{
			m_Material.SetVector(int_17, new Vector4(toneValues.x, toneValues.y, toneValues.z, toneValues.z));
			m_Material.SetVector(int_18, new Vector4(secondaryToneValues.x, secondaryToneValues.y, secondaryToneValues.z, secondaryToneValues.z));
		}
		if (useGammaCorrection)
		{
			m_Material.SetFloat(int_19, gammaValue);
		}
		else
		{
			m_Material.SetFloat(int_19, 0f);
		}
		if (useDof)
		{
			method_9(m_Material);
		}
		if (useSharpen)
		{
			m_Material2.SetFloat(int_20, sharpenAmount);
		}
		if (useLensDirt)
		{
			m_Material.SetTexture(int_21, lensDirtTexture);
			m_Material2.SetTexture(int_21, lensDirtTexture);
		}
		else
		{
			m_Material.SetFloat(int_2, 0f);
		}
		if (useExposure)
		{
			m_Material.SetFloat(int_22, exposureLowerLimit);
			m_Material.SetFloat(int_23, exposureUpperLimit);
			m_Material.SetFloat(int_24, exposureMiddleGrey);
			m_Material2.SetFloat(int_25, exposureSpeed);
			m_Material2.SetFloat(int_22, exposureLowerLimit);
			m_Material2.SetFloat(int_23, exposureUpperLimit);
			m_Material2.SetFloat(int_24, exposureMiddleGrey);
			Shader.EnableKeyword("PRISM_USE_EXPOSURE");
		}
		else
		{
			Shader.DisableKeyword("PRISM_USE_EXPOSURE");
		}
		Shader.DisableKeyword("PRISM_DOF_LOWSAMPLE");
		Shader.DisableKeyword("PRISM_DOF_MEDSAMPLE");
		if (useDof)
		{
			if (dofSampleAmount == DoFSamples.Low)
			{
				Shader.EnableKeyword("PRISM_DOF_LOWSAMPLE");
			}
			else if (dofSampleAmount == DoFSamples.Medium)
			{
				Shader.EnableKeyword("PRISM_DOF_MEDSAMPLE");
			}
			else
			{
				Shader.EnableKeyword("PRISM_DOF_MEDSAMPLE");
				Debug.LogWarning("High sample DoF is currently deprecated. If you want to increase your DoF samples, change around line 81 ('DOF_SAMPLES') in PRISM.cginc to a higher number");
				dofSampleAmount = DoFSamples.Medium;
			}
			if (useNearDofBlur)
			{
				Shader.EnableKeyword("PRISM_DOF_USENEARBLUR");
			}
			else
			{
				Shader.DisableKeyword("PRISM_DOF_USENEARBLUR");
			}
		}
		else
		{
			Shader.DisableKeyword("PRISM_DOF_USENEARBLUR");
			if (useFullScreenBlur)
			{
				m_Material.EnableKeyword("PRISM_DOF_USENEARBLUR");
				m_Material2.EnableKeyword("PRISM_DOF_USENEARBLUR");
			}
			else
			{
				m_Material.DisableKeyword("PRISM_DOF_USENEARBLUR");
				m_Material2.DisableKeyword("PRISM_DOF_USENEARBLUR");
			}
		}
		method_7();
		method_8();
		if (!useLut)
		{
			m_Material.DisableKeyword("PRISM_GAMMA_LOOKUP");
			m_Material.DisableKeyword("PRISM_LINEAR_LOOKUP");
		}
		else if (camera_0.allowHDR)
		{
			m_Material.EnableKeyword("PRISM_LINEAR_LOOKUP");
			m_Material.DisableKeyword("PRISM_GAMMA_LOOKUP");
		}
		else
		{
			m_Material.EnableKeyword("PRISM_GAMMA_LOOKUP");
			m_Material.DisableKeyword("PRISM_LINEAR_LOOKUP");
		}
		if (useTonemap && tonemapType == TonemapType.Filmic)
		{
			m_Material.EnableKeyword("PRISM_FILMIC_TONEMAP");
			Shader.EnableKeyword("PRISM_FILMIC_TONEMAP");
			Shader.DisableKeyword("PRISM_ROMB_TONEMAP");
			Shader.DisableKeyword("PRISM_ACES_TONEMAP");
		}
		else if (useTonemap && tonemapType == TonemapType.RomB)
		{
			m_Material.EnableKeyword("PRISM_ROMB_TONEMAP");
			Shader.EnableKeyword("PRISM_ROMB_TONEMAP");
			Shader.DisableKeyword("PRISM_FILMIC_TONEMAP");
			Shader.DisableKeyword("PRISM_ACES_TONEMAP");
		}
		else if (useTonemap && tonemapType == TonemapType.ACES)
		{
			m_Material.EnableKeyword("PRISM_ACES_TONEMAP");
			Shader.EnableKeyword("PRISM_ACES_TONEMAP");
			Shader.DisableKeyword("PRISM_FILMIC_TONEMAP");
			Shader.DisableKeyword("PRISM_ROMB_TONEMAP");
		}
		else
		{
			Shader.DisableKeyword("PRISM_FILMIC_TONEMAP");
			Shader.DisableKeyword("PRISM_ROMB_TONEMAP");
			Shader.DisableKeyword("PRISM_ACES_TONEMAP");
		}
		if (useNoise)
		{
			if (ssaa_0 != null && ssaa_0.UsesDLSSUpscaler())
			{
				m_Material.SetFloat(int_26, 0f);
			}
			else
			{
				m_Material.SetFloat(int_26, 1f);
			}
		}
		else
		{
			m_Material.SetFloat(int_26, 0f);
		}
		if (useNightVision)
		{
			m_Material.SetFloat(int_27, 1f);
			Shader.EnableKeyword("PRISM_USE_NIGHTVISION");
		}
		else
		{
			m_Material.SetFloat(int_27, 0f);
			Shader.DisableKeyword("PRISM_USE_NIGHTVISION");
		}
		if (useRays)
		{
			method_4(m_Material3);
			method_4(m_Material);
		}
		else
		{
			m_Material.SetFloat(int_28, 0f);
		}
		if (useAmbientObscurance)
		{
			if (!IsGBufferAvailable || UsingTerrain)
			{
				camera_0.depthTextureMode |= DepthTextureMode.Depth;
				camera_0.depthTextureMode |= DepthTextureMode.DepthNormals;
			}
			m_Material.SetFloat(int_29, aoIntensity);
			m_Material.SetFloat(int_30, aoLightingContribution);
			if (!useAODistanceCutoff && !useDof)
			{
				m_AOMaterial.DisableKeyword("_AOCUTOFF_ON");
			}
			else
			{
				m_AOMaterial.EnableKeyword("_AOCUTOFF_ON");
			}
			if (IsGBufferAvailable && !UsingTerrain)
			{
				m_AOMaterial.EnableKeyword("_SOURCE_GBUFFER");
			}
			else
			{
				m_AOMaterial.DisableKeyword("_SOURCE_GBUFFER");
			}
			if (aoSampleCount == SampleCount.Low)
			{
				m_AOMaterial.EnableKeyword("_AOSAMPLECOUNT_LOWEST");
				m_AOMaterial.DisableKeyword("_AOSAMPLECOUNT_CUSTOM");
				m_AOMaterial.SetInt(int_31, aoSampleCountValue);
			}
			else
			{
				m_AOMaterial.EnableKeyword("_AOSAMPLECOUNT_CUSTOM");
				m_AOMaterial.DisableKeyword("_AOSAMPLECOUNT_LOWEST");
				m_AOMaterial.SetInt(int_31, aoSampleCountValue);
			}
			m_AOMaterial.SetInt(int_32, aoSampleCountValue);
		}
		else
		{
			m_Material.SetFloat(int_29, aoMinIntensity);
		}
	}

	public void method_3()
	{
		if ((useBloom && !useBloomStability) || (useBloom && !renderTexture_1))
		{
			Shader.EnableKeyword("PRISM_USE_BLOOM");
			Shader.DisableKeyword("PRISM_USE_STABLEBLOOM");
		}
		else if (useBloom && useBloomStability)
		{
			Shader.EnableKeyword("PRISM_USE_STABLEBLOOM");
			Shader.DisableKeyword("PRISM_USE_BLOOM");
		}
		else
		{
			Shader.DisableKeyword("PRISM_USE_STABLEBLOOM");
			Shader.DisableKeyword("PRISM_USE_BLOOM");
		}
		if (bloomUseScreenBlend)
		{
			Shader.EnableKeyword("PRISM_BLOOM_SCREENBLEND");
		}
		else
		{
			Shader.DisableKeyword("PRISM_BLOOM_SCREENBLEND");
		}
	}

	public void method_4(Material raysMaterial)
	{
		raysMaterial.SetFloat(int_28, rayWeight);
		raysMaterial.SetColor(int_33, rayColor);
		raysMaterial.SetColor(int_34, rayThreshold);
		if (!rayTransform)
		{
			raysMaterial.SetVector(int_35, new Vector4(base.transform.position.x, base.transform.position.y, base.transform.position.z, 1f));
			return;
		}
		Vector3 vector = camera_0.WorldToViewportPoint(rayTransform.position);
		raysMaterial.SetVector(int_35, new Vector4(vector.x, vector.y, vector.z, rayWeight));
		if (vector.z >= 0f)
		{
			raysMaterial.SetColor(int_33, rayColor);
		}
		else
		{
			raysMaterial.SetColor(int_33, Color.black);
		}
	}

	public void method_5(Material fogMaterial)
	{
		fogMaterial.SetFloat(int_36, fogHeight);
		fogMaterial.SetFloat(int_3, 1f);
		fogMaterial.SetFloat(int_37, fogDistance);
		fogMaterial.SetFloat(int_38, fogStartPoint);
		fogMaterial.SetColor(int_39, fogColor);
		fogMaterial.SetColor(int_40, fogEndColor);
		if (fogAffectSkybox)
		{
			fogMaterial.SetFloat(int_41, 1f);
		}
		else
		{
			fogMaterial.SetFloat(int_41, 0.9999999f);
		}
		fogMaterial.SetMatrix(int_42, camera_0.cameraToWorldMatrix);
	}

	public void method_6(Material aoMaterial)
	{
		m_Material.SetFloat(int_30, aoLightingContribution);
		aoMaterial.SetFloat(int_29, aoIntensity);
		aoMaterial.SetFloat(int_43, aoRadius);
		aoMaterial.SetFloat(int_44, aoBias * 0.02f);
		aoMaterial.SetFloat(int_45, aoDownsample ? 0.5f : 1f);
		if (useDof)
		{
			aoMaterial.SetFloat(int_46, dofFocusPoint);
			aoMaterial.SetFloat(int_47, dofFocusDistance);
		}
		else
		{
			aoMaterial.SetFloat(int_46, aoDistanceCutoffStart);
			aoMaterial.SetFloat(int_47, aoDistanceCutoffLength);
		}
		aoMaterial.SetMatrix(int_48, camera_0.cameraToWorldMatrix);
		int num = (ssaa_0 ? ssaa_0.GetInputWidth() : Screen.width);
		int num2 = (ssaa_0 ? ssaa_0.GetInputHeight() : Screen.height);
		Matrix4x4 projectionMatrix = camera_0.projectionMatrix;
		aoMaterial.SetVector(value: new Vector4(-2f / ((float)num * projectionMatrix[0]), -2f / ((float)num2 * projectionMatrix[5]), (1f - projectionMatrix[2]) / projectionMatrix[0], (1f + projectionMatrix[6]) / projectionMatrix[5]), nameID: int_49);
	}

	public void method_7()
	{
		if (useLut && (bool)twoDLookupTex && (bool)threeDLookupTex)
		{
			int width = threeDLookupTex.width;
			threeDLookupTex.wrapMode = TextureWrapMode.Clamp;
			m_Material.SetFloat(int_50, (float)(width - 1) / (1f * (float)width));
			m_Material.SetFloat(int_51, 1f / (2f * (float)width));
			m_Material.SetTexture(int_52, threeDLookupTex);
			m_Material.SetFloat(int_53, lutLerpAmount);
		}
		else
		{
			m_Material.SetFloat(int_53, 0f);
		}
	}

	public void method_8()
	{
		if (useSecondLut && (bool)secondaryTwoDLookupTex && (bool)secondaryThreeDLookupTex)
		{
			int width = secondaryThreeDLookupTex.width;
			secondaryThreeDLookupTex.wrapMode = TextureWrapMode.Clamp;
			m_Material.SetFloat(int_54, (float)(width - 1) / (1f * (float)width));
			m_Material.SetFloat(int_55, 1f / (2f * (float)width));
			m_Material.SetTexture(int_56, secondaryThreeDLookupTex);
			m_Material.SetFloat(int_57, secondaryLutLerpAmount);
		}
		else
		{
			m_Material.SetFloat(int_57, 0f);
		}
	}

	public void SetDofTransform(Transform target)
	{
		dofFocusTransform = target;
	}

	public void SetDofPoint(Vector3 point)
	{
		dofFocusPoint = Vector3.Distance(camera_0.transform.position, point);
	}

	public void ResetDofTransform()
	{
		dofFocusTransform = null;
	}

	public void method_9(Material targetMat)
	{
		if ((bool)dofFocusTransform)
		{
			targetMat.SetFloat(int_58, Vector3.Distance(base.transform.position, dofFocusTransform.position));
		}
		else
		{
			targetMat.SetFloat(int_58, dofFocusPoint);
		}
		targetMat.SetFloat(int_59, dofFocusDistance);
		targetMat.SetFloat(int_60, dofRadius);
		targetMat.SetFloat(int_61, dofBokehFactor);
		if (useNearDofBlur)
		{
			targetMat.SetFloat(int_62, 1f);
			targetMat.SetFloat(int_63, dofNearFocusDistance);
		}
		else
		{
			targetMat.SetFloat(int_62, 0f);
		}
		if (dofBlurSkybox)
		{
			targetMat.SetFloat(int_64, 1f);
		}
		else
		{
			targetMat.SetFloat(int_64, 0.9999999f);
		}
	}

	public void method_10(RenderTexture source, RenderTextureFormat rtFormat)
	{
		RenderTexture renderTexture = RenderTexture.GetTemporary(source.width / 2, source.width / 2, 0, rtFormat);
		renderTexture.name = "PeismEffects DownSampledTex";
		renderTexture.filterMode = FilterMode.Bilinear;
		DebugGraphics.Blit(source, renderTexture, m_Material2, 3);
		int num = histWidth;
		while (renderTexture.height > num || renderTexture.width > num)
		{
			int num2 = renderTexture.width / 2;
			if (num2 < num)
			{
				num2 = num;
			}
			int num3 = renderTexture.height / 2;
			if (num3 < num)
			{
				num3 = num;
			}
			RenderTexture temporary = RenderTexture.GetTemporary(num2, num3, 0, rtFormat);
			temporary.name = "Prism Effects rtTempDst";
			DebugGraphics.Blit(renderTexture, temporary, m_Material2, 2);
			RenderTexture.ReleaseTemporary(renderTexture);
			renderTexture = temporary;
		}
		renderTexture_3.MarkRestoreExpected();
		int pass = 4;
		if (bool_1)
		{
			Graphics.Blit(renderTexture, renderTexture_3);
		}
		else
		{
			DebugGraphics.Blit(renderTexture, renderTexture_3, m_Material2, pass);
		}
		m_Material.SetTexture(int_65, renderTexture_3);
		m_Material2.SetTexture(int_65, renderTexture_3);
		RenderTexture.ReleaseTemporary(renderTexture);
	}

	public RenderTexture method_11(RenderTexture tex, int iterations = 1)
	{
		for (int i = 0; i < iterations; i++)
		{
			Vector2 vector = new Vector2(0f, 1f);
			RenderTexture temporary = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32);
			temporary.name = "Prism Effects BlurOne";
			m_Material2.SetVector(int_66, vector);
			DebugGraphics.Blit(tex, temporary, m_Material2, 8);
			vector = new Vector2(1f, 0f);
			m_Material2.SetVector(int_66, vector);
			DebugGraphics.Blit(temporary, tex, m_Material2, 8);
			RenderTexture.ReleaseTemporary(temporary);
		}
		RenderTexture.ReleaseTemporary(tex);
		return tex;
	}

	[ImageEffectTransformsToLDR]
	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.GetSourceDestinationHDRToLDR(out source, out destination);
		}
		method_12(source, destination);
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.ReleaseSourceDestinationHDRToLDR(source, destination);
		}
	}

	public void method_12(RenderTexture source, RenderTexture destination)
	{
		bool flag = true;
		if (bool_0)
		{
			Graphics.CopyTexture(source, destination);
			bool_0 = false;
			return;
		}
		RenderTexture temporary;
		RenderTexture temporary2;
		RenderTexture temporary3;
		RenderTexture temporary4;
		RenderTexture temporary5;
		RenderTexture temporary6;
		if (method_2() && flag)
		{
			EnvironmentManager instance = EnvironmentManager.Instance;
			if (instance != null)
			{
				exposureSpeed = instance.PrismExposureSpeed;
				exposureMiddleGrey = instance.PrismExposureOffset;
			}
			UpdateShaderValues();
			if (threeDLookupTex == null && useLut)
			{
				SetIdentityLut();
			}
			if (secondaryThreeDLookupTex == null && useSecondLut)
			{
				SetIdentityLut(secondary: true);
			}
			RenderTextureFormat defaultHDRRenderTextureFormat = RuntimeUtilities.defaultHDRRenderTextureFormat;
			defaultHDRRenderTextureFormat = (camera_0.allowHDR ? RuntimeUtilities.defaultHDRRenderTextureFormat : RenderTextureFormat.ARGB32);
			if (isParentPrism)
			{
				if (!renderTexture_0 || renderTexture_0.width != source.width || renderTexture_0.height != source.height)
				{
					renderTexture_0 = new RenderTexture(source.width, source.height, 16, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
					renderTexture_0.name = "PrismEffects DepthTex";
				}
				Graphics.SetRenderTarget(renderTexture_0);
				GL.Clear(clearDepth: true, clearColor: true, Color.white);
				DebugGraphics.Blit(source, renderTexture_0, m_Material3, 5);
				Shader.SetGlobalTexture(int_67, renderTexture_0);
				if (debugDofPass)
				{
					DebugGraphics.Blit(renderTexture_0, destination, m_Material3, 6);
					return;
				}
			}
			int num = 1;
			if (aoDownsample)
			{
				num = 2;
			}
			int width = source.width / num;
			int height = source.height / num;
			int width2 = source.width / 4;
			int height2 = source.height / 4;
			temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat_0, RenderTextureReadWrite.Linear);
			temporary.name = "Prism Effects AO RT";
			temporary2 = RenderTexture.GetTemporary(source.width, source.height, 0, defaultHDRRenderTextureFormat);
			temporary2.name = "Prism Effects TempDofDest";
			temporary3 = RenderTexture.GetTemporary(source.width, source.height, 0, defaultHDRRenderTextureFormat);
			temporary3.name = "Prism Effects TempDest";
			temporary4 = RenderTexture.GetTemporary(width2, height2, 0, RenderTextureFormat.ARGB32);
			temporary4.name = "Prism Effects Rays Tex";
			temporary5 = RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0, defaultHDRRenderTextureFormat);
			temporary5.name = "Prism Effects FullSizeMedian";
			temporary6 = RenderTexture.GetTemporary(source.width, source.height, 0, defaultHDRRenderTextureFormat);
			temporary6.name = "Prism Effects DofTex";
			if (useAmbientObscurance)
			{
				method_6(m_AOMaterial);
				DebugGraphics.Blit(null, temporary, m_AOMaterial, 0);
				if (aoBlurType == AOBlurType.Fast)
				{
					for (int i = 0; i < aoBlurIterations; i++)
					{
						m_AOMaterial.SetVector(int_77, new Vector4(-1f, 0f, 0f, 0f));
						RenderTexture temporary7 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat_0, RenderTextureReadWrite.Linear);
						temporary7.name = "PrismEffects TmpRT3";
						DebugGraphics.Blit(temporary, temporary7, m_AOMaterial, (int)aoBlurType);
						RenderTexture.ReleaseTemporary(temporary);
						m_AOMaterial.SetVector(int_77, new Vector4(0f, 1f, 0f, 0f));
						temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat_0, RenderTextureReadWrite.Linear);
						temporary.name = "PrismEffects AOTex";
						DebugGraphics.Blit(temporary7, temporary, m_AOMaterial, (int)aoBlurType);
						RenderTexture.ReleaseTemporary(temporary7);
					}
				}
				else
				{
					for (int j = 0; j < aoBlurIterations; j++)
					{
						for (int k = 0; k < 2; k++)
						{
							m_AOMaterial.SetVector(int_77, new Vector4(-1f, 0f, 0f, 0f));
							RenderTexture temporary7 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat_0, RenderTextureReadWrite.Linear);
							temporary7.name = "PrismEffects TmpRT3";
							DebugGraphics.Blit(temporary, temporary7, m_AOMaterial, (int)(aoBlurType + k));
							RenderTexture.ReleaseTemporary(temporary);
							m_AOMaterial.SetVector(int_77, new Vector4(0f, 1f, 0f, 0f));
							temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat_0, RenderTextureReadWrite.Linear);
							temporary.name = "PrismEffects AOTex";
							DebugGraphics.Blit(temporary7, temporary, m_AOMaterial, (int)(aoBlurType + k));
							RenderTexture.ReleaseTemporary(temporary7);
						}
					}
				}
				if (aoShowDebug)
				{
					DebugGraphics.Blit(temporary, destination, m_AOMaterial, 2);
					goto IL_0fc9;
				}
				m_Material.SetTexture(int_78, temporary);
				if (isParentPrism)
				{
					Shader.SetGlobalFloat(int_29, aoIntensity);
					Shader.SetGlobalTexture(int_78, temporary);
				}
			}
			if (useMedianDoF)
			{
				DebugGraphics.Blit(source, temporary5, m_Material2, 6);
				m_Material.SetTexture(int_68, temporary5);
				m_Material.SetFloat(int_69, 1f);
			}
			else
			{
				m_Material.SetTexture(int_68, source);
				m_Material.SetFloat(int_69, 0f);
			}
			if (debugDofPass && useDof)
			{
				method_9(m_Material2);
				DebugGraphics.Blit(source, destination, m_Material2, 9);
			}
			else
			{
				if (useExposure)
				{
					bool flag2 = false;
					if (m_MasterEffectExposure != null)
					{
						flag2 = true;
						Graphics.Blit(m_MasterEffectExposure.AdaptationTexture, renderTexture_3);
						m_Material.SetTexture(int_65, renderTexture_3);
						m_Material2.SetTexture(int_65, renderTexture_3);
					}
					if (!flag2)
					{
						if (useMedianDoF)
						{
							method_10(temporary5, defaultHDRRenderTextureFormat);
						}
						else
						{
							method_10(source, defaultHDRRenderTextureFormat);
						}
					}
					if (debugViewExposure)
					{
						DebugGraphics.Blit(source, destination, m_Material2, 11);
						goto IL_0fc9;
					}
				}
				if (useBloom)
				{
					if (bloomType == BloomType.HDR)
					{
						bloomDownsample = 2;
					}
					int num2 = source.width / bloomDownsample;
					int num3 = source.height / bloomDownsample;
					RenderTexture temporary8 = RenderTexture.GetTemporary(num2, num3, 0, defaultHDRRenderTextureFormat);
					temporary8.name = "Prism Effects HaldRezColorDown";
					temporary8.filterMode = FilterMode.Bilinear;
					if (useMedianDoF)
					{
						if (useBloomStability)
						{
							DebugGraphics.Blit(temporary5, temporary8, m_Material2, 6);
						}
						else
						{
							DebugGraphics.Blit(temporary5, temporary8, m_Material2, 6);
						}
					}
					else
					{
						DebugGraphics.Blit(source, temporary8, m_Material2, 6);
					}
					if (useRays)
					{
						DebugGraphics.Blit(temporary8, temporary4, m_Material2, 2);
						method_13(temporary4);
						if (raysShowDebug)
						{
							Graphics.Blit(temporary4, destination);
							RenderTexture.ReleaseTemporary(temporary8);
							goto IL_0fc9;
						}
					}
					RenderTexture renderTexture = RenderTexture.GetTemporary(num2, num3, 0, defaultHDRRenderTextureFormat);
					renderTexture.name = "Prism Effects Median Colror";
					if (bloomType == BloomType.Simple)
					{
						int num4 = 0;
						DebugGraphics.Blit(temporary8, renderTexture, m_Material2, 5);
						for (int l = 0; l < bloomBlurPasses; l++)
						{
							RenderTexture temporary9 = RenderTexture.GetTemporary(num2, num3, 0, defaultHDRRenderTextureFormat);
							temporary9.name = "Prism Effects Blur4";
							m_Material2.SetInt(int_70, l + num4);
							DebugGraphics.Blit(renderTexture, temporary9, m_Material2, 7);
							if (useUIBlur && uiBlurGrabTextureFromPassNumber == l && renderTexture_2.width > 0 && renderTexture_2.height > 0)
							{
								Graphics.Blit(temporary9, renderTexture_2);
								Shader.SetGlobalTexture(int_71, renderTexture_2);
							}
							RenderTexture.ReleaseTemporary(renderTexture);
							renderTexture = temporary9;
						}
						m_Material.SetTexture(int_72, renderTexture);
						if (debugBloomTex)
						{
							m_Material3.SetFloat(int_1, bloomThreshold);
							m_Material3.SetFloat(int_0, bloomIntensity);
							DebugGraphics.Blit(renderTexture, destination, m_Material3, 4);
							RenderTexture.ReleaseTemporary(temporary8);
							RenderTexture.ReleaseTemporary(renderTexture);
							goto IL_0fc9;
						}
						if ((bool)renderTexture_1 && useBloomStability)
						{
							m_Material.SetTexture(int_73, renderTexture_1);
						}
						if ((bool)renderTexture_1)
						{
							RenderTexture.ReleaseTemporary(renderTexture_1);
							renderTexture_1 = null;
						}
						if (useBloomStability)
						{
							renderTexture_1 = RenderTexture.GetTemporary(num2, num3, 0, defaultHDRRenderTextureFormat);
							renderTexture_1.name = "Prism Effects AccumulationBuffer";
							renderTexture_1.filterMode = FilterMode.Bilinear;
							Graphics.Blit(renderTexture, renderTexture_1);
						}
						RenderTexture.ReleaseTemporary(temporary8);
					}
					else if (bloomType == BloomType.HDR)
					{
						RenderTexture temporary10 = RenderTexture.GetTemporary(num2 / 2, num3 / 2, 0, defaultHDRRenderTextureFormat);
						temporary10.name = "Prism Effects QuarterRezColorDown";
						temporary10.filterMode = FilterMode.Bilinear;
						RenderTexture temporary11 = RenderTexture.GetTemporary(num2 / 4, num3 / 4, 0, defaultHDRRenderTextureFormat);
						temporary11.name = "PrismEffects EighthRezColorDown";
						temporary11.filterMode = FilterMode.Bilinear;
						RenderTexture temporary12 = RenderTexture.GetTemporary(num2 / 8, num3 / 8, 0, defaultHDRRenderTextureFormat);
						temporary12.name = "PrismEffects SixteenthRezColorDown";
						temporary12.filterMode = FilterMode.Bilinear;
						DebugGraphics.Blit(source, temporary8, m_Material2, 10);
						DebugGraphics.Blit(temporary8, temporary10, m_Material2, 2);
						DebugGraphics.Blit(temporary10, temporary11, m_Material2, 2);
						DebugGraphics.Blit(temporary11, temporary12, m_Material2, 6);
						temporary8 = method_11(temporary8);
						temporary10 = method_11(temporary10);
						temporary11 = method_11(temporary11, 2);
						temporary12 = method_11(temporary12, 4);
						m_Material.SetTexture(int_72, temporary8);
						m_Material.SetTexture(int_74, temporary10);
						m_Material.SetTexture(int_75, temporary11);
						m_Material.SetTexture(int_76, temporary12);
						if ((bool)renderTexture_1 && useBloomStability)
						{
							m_Material.SetTexture(int_73, renderTexture_1);
						}
						if ((bool)renderTexture_1)
						{
							RenderTexture.ReleaseTemporary(renderTexture_1);
							renderTexture_1 = null;
						}
						if (useBloomStability)
						{
							renderTexture_1 = RenderTexture.GetTemporary(num2, num3, 0, defaultHDRRenderTextureFormat);
							renderTexture_1.name = "PrismEffects AccumulationBuffer";
							renderTexture_1.filterMode = FilterMode.Bilinear;
							Graphics.Blit(temporary8, renderTexture_1);
						}
						if (useUIBlur && renderTexture_2.width > 0 && renderTexture_2.height > 0)
						{
							Graphics.Blit(temporary8, renderTexture_2);
							Shader.SetGlobalTexture(int_71, renderTexture_2);
						}
						if (debugBloomTex)
						{
							m_Material3.EnableKeyword("PRISM_HDR_BLOOM");
							m_Material3.EnableKeyword("PRISM_USE_BLOOM");
							m_Material3.SetTexture(int_72, temporary8);
							m_Material3.SetTexture(int_74, temporary10);
							m_Material3.SetTexture(int_75, temporary11);
							m_Material3.SetTexture(int_76, temporary12);
							m_Material3.SetFloat(int_0, bloomIntensity);
							DebugGraphics.Blit(temporary8, destination, m_Material3, 4);
							goto IL_0fc9;
						}
					}
					if (useSharpen)
					{
						DebugGraphics.Blit(source, temporary3, m_Material, 0);
						RenderTexture temporary13 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
						temporary13.name = "PrismEffects ChromDest";
						DebugGraphics.Blit(temporary3, temporary13, m_Material2, 0);
						Graphics.CopyTexture(temporary13, destination);
						RenderTexture.ReleaseTemporary(temporary13);
					}
					else if (forceSecondChromaticPass)
					{
						DebugGraphics.Blit(source, temporary3, m_Material, 0);
						if (useChromaticBlur)
						{
							Vector2 vector = new Vector2(0f, 1f * chromaticBlurWidth);
							RenderTexture temporary14 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
							temporary14.name = "PrismEffects BlurChrom";
							m_Material2.SetVector(int_66, vector);
							DebugGraphics.Blit(temporary3, temporary14, m_Material2, 12);
							vector = new Vector2(1f * chromaticBlurWidth, 0f);
							RenderTexture temporary15 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
							temporary15.name = "PrismEffects BlurChrom2";
							m_Material2.SetVector(int_66, vector);
							DebugGraphics.Blit(temporary14, temporary15, m_Material2, 12);
							RenderTexture temporary16 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
							temporary16.name = "PrismEffects ChromDest";
							DebugGraphics.Blit(temporary15, temporary16, m_Material2, 1);
							Graphics.CopyTexture(temporary16, destination);
							RenderTexture.ReleaseTemporary(temporary14);
							RenderTexture.ReleaseTemporary(temporary15);
							RenderTexture.ReleaseTemporary(temporary16);
						}
						else
						{
							RenderTexture temporary17 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
							temporary17.name = "PrismEffects ChromDest";
							DebugGraphics.Blit(temporary3, temporary17, m_Material2, 1);
							Graphics.CopyTexture(temporary17, destination);
							RenderTexture.ReleaseTemporary(temporary17);
						}
					}
					else
					{
						DebugGraphics.Blit(source, destination, m_Material, 0);
					}
					RenderTexture.ReleaseTemporary(renderTexture);
				}
				else
				{
					if (useRays)
					{
						int width3 = source.width / 2;
						int height3 = source.height / 2;
						RenderTexture temporary18 = RenderTexture.GetTemporary(width3, height3, 0, defaultHDRRenderTextureFormat);
						temporary18.name = "PrismEffects HalfRezColorDown";
						temporary18.filterMode = FilterMode.Bilinear;
						if (useMedianDoF)
						{
							DebugGraphics.Blit(temporary5, temporary18, m_Material2, 2);
						}
						else
						{
							DebugGraphics.Blit(source, temporary18, m_Material2, 2);
						}
						DebugGraphics.Blit(temporary18, temporary4, m_Material2, 2);
						method_13(temporary4);
						if (raysShowDebug)
						{
							Graphics.Blit(temporary4, destination);
							RenderTexture.ReleaseTemporary(temporary18);
							goto IL_0fc9;
						}
						RenderTexture.ReleaseTemporary(temporary18);
					}
					if (useSharpen)
					{
						DebugGraphics.Blit(source, temporary3, m_Material, 0);
						RenderTexture temporary19 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
						temporary19.name = "PrismEffects ChromDest";
						DebugGraphics.Blit(temporary3, temporary19, m_Material2, 0);
						Graphics.CopyTexture(temporary19, destination);
						RenderTexture.ReleaseTemporary(temporary19);
					}
					else if (forceSecondChromaticPass)
					{
						DebugGraphics.Blit(source, temporary3, m_Material, 0);
						if (useChromaticBlur)
						{
							Vector2 vector2 = new Vector2(0f, 1f * chromaticBlurWidth);
							RenderTexture temporary20 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
							temporary20.name = "PrismEffects BlurChrom";
							m_Material2.SetVector(int_66, vector2);
							DebugGraphics.Blit(temporary3, temporary20, m_Material2, 12);
							vector2 = new Vector2(1f * chromaticBlurWidth, 0f);
							RenderTexture temporary21 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
							temporary21.name = "PrismEffects BlurChrom2";
							m_Material2.SetVector(int_66, vector2);
							DebugGraphics.Blit(temporary20, temporary21, m_Material2, 12);
							RenderTexture temporary22 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
							temporary22.name = "PrismEffects ChromDest";
							DebugGraphics.Blit(temporary21, temporary22, m_Material2, 1);
							Graphics.CopyTexture(temporary22, destination);
							RenderTexture.ReleaseTemporary(temporary20);
							RenderTexture.ReleaseTemporary(temporary21);
							RenderTexture.ReleaseTemporary(temporary22);
						}
						else
						{
							RenderTexture temporary23 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
							temporary23.name = "PrismEffects ChromDest";
							DebugGraphics.Blit(temporary3, temporary23, m_Material2, 1);
							Graphics.CopyTexture(temporary23, destination);
							RenderTexture.ReleaseTemporary(temporary23);
						}
					}
					else
					{
						DebugGraphics.Blit(source, destination, m_Material, 0);
					}
				}
			}
			goto IL_0fc9;
		}
		Graphics.CopyTexture(source, destination);
		return;
		IL_0fc9:
		RenderTexture.ReleaseTemporary(temporary4);
		RenderTexture.ReleaseTemporary(temporary3);
		RenderTexture.ReleaseTemporary(temporary2);
		RenderTexture.ReleaseTemporary(temporary5);
		RenderTexture.ReleaseTemporary(temporary6);
		RenderTexture.ReleaseTemporary(temporary);
		bool_1 = false;
	}

	public void method_13(RenderTexture quarterResMain)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(quarterResMain.width, quarterResMain.height, 0, RenderTextureFormat.ARGB32);
		temporary.name = "PrismEffects PreBlurTex";
		RenderTexture temporary2 = RenderTexture.GetTemporary(quarterResMain.width, quarterResMain.height, 0, RenderTextureFormat.ARGB32);
		temporary2.name = "PrismEffects QuaterResSecond";
		DebugGraphics.Blit(quarterResMain, temporary, m_Material3, 0);
		DebugGraphics.Blit(temporary, temporary2, m_Material3, 1);
		m_Material.SetTexture(int_79, temporary2);
		RenderTexture.ReleaseTemporary(temporary);
		RenderTexture.ReleaseTemporary(temporary2);
	}

	public void SetIdentityLut(bool secondary = false)
	{
		int num = 16;
		Color[] array = new Color[4096];
		float num2 = 1f / 15f;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				for (int k = 0; k < num; k++)
				{
					array[i + j * num + k * num * num] = new Color((float)i * 1f * num2, (float)j * 1f * num2, (float)k * 1f * num2, 1f);
				}
			}
		}
		if (!secondary)
		{
			if ((bool)threeDLookupTex)
			{
				Object.DestroyImmediate(threeDLookupTex);
			}
			threeDLookupTex = new Texture3D(num, num, num, TextureFormat.ARGB32, mipChain: false);
			threeDLookupTex.SetPixels(array);
			threeDLookupTex.Apply();
			basedOnTempTex = "";
		}
		else
		{
			if ((bool)secondaryThreeDLookupTex)
			{
				Object.DestroyImmediate(secondaryThreeDLookupTex);
			}
			secondaryThreeDLookupTex = new Texture3D(num, num, num, TextureFormat.ARGB32, mipChain: false);
			secondaryThreeDLookupTex.SetPixels(array);
			secondaryThreeDLookupTex.Apply();
			secondaryBasedOnTempTex = "";
		}
	}

	public bool ValidDimensions(Texture2D tex2d)
	{
		if (!tex2d)
		{
			return false;
		}
		if (tex2d.height != Mathf.FloorToInt(Mathf.Sqrt(tex2d.width)))
		{
			return false;
		}
		return true;
	}

	public void AddDependantEffectExposure(PrismEffects pe)
	{
		if (list_0 == null)
		{
			list_0 = new List<PrismEffects>();
		}
		list_0.Add(pe);
		pe.m_MasterEffectExposure = this;
	}

	public void Convert(Texture2D temp2DTex, bool secondaryLut = false)
	{
		if ((bool)temp2DTex)
		{
			int num = temp2DTex.width * temp2DTex.height;
			num = temp2DTex.height;
			if (!ValidDimensions(temp2DTex))
			{
				Debug.LogWarning("The given 2D texture " + temp2DTex.name + " cannot be used as a 3D LUT.");
				if (!secondaryLut)
				{
					secondaryBasedOnTempTex = "";
				}
				else
				{
					basedOnTempTex = "";
				}
				return;
			}
			Color[] pixels = temp2DTex.GetPixels();
			Color[] array = new Color[pixels.Length];
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num; j++)
				{
					for (int k = 0; k < num; k++)
					{
						int num2 = num - j - 1;
						array[i + j * num + k * num * num] = pixels[k * num + i + num2 * num * num];
					}
				}
			}
			if (!secondaryLut)
			{
				if ((bool)threeDLookupTex)
				{
					Object.DestroyImmediate(threeDLookupTex);
				}
				threeDLookupTex = new Texture3D(num, num, num, TextureFormat.ARGB32, mipChain: false);
				threeDLookupTex.SetPixels(array);
				threeDLookupTex.Apply();
				basedOnTempTex = temp2DTex.name;
				twoDLookupTex = temp2DTex;
			}
			else
			{
				if ((bool)secondaryThreeDLookupTex)
				{
					Object.DestroyImmediate(secondaryThreeDLookupTex);
				}
				secondaryThreeDLookupTex = new Texture3D(num, num, num, TextureFormat.ARGB32, mipChain: false);
				secondaryThreeDLookupTex.SetPixels(array);
				secondaryThreeDLookupTex.Apply();
				secondaryBasedOnTempTex = temp2DTex.name;
				secondaryTwoDLookupTex = temp2DTex;
			}
		}
		else
		{
			Debug.LogError("Couldn't color correct with 3D LUT texture. PRISM will be disabled.");
			base.enabled = false;
		}
	}
}
