using System;
using UnityEngine;
using UnityEngine.XR;

[AddComponentMenu(null)]
public class HBAO_Core : MonoBehaviour
{
	public enum Preset
	{
		FastestPerformance,
		FastPerformance,
		Normal,
		HighQuality,
		HighestQuality,
		ColoredHighestQuality,
		Custom
	}

	public enum IntegrationStage
	{
		BeforeImageEffectsOpaque,
		AfterLighting,
		BeforeReflections
	}

	public enum Quality
	{
		Lowest,
		Low,
		Medium,
		High,
		Highest
	}

	public enum Resolution
	{
		Full,
		Half,
		Quarter
	}

	public enum Deinterleaving
	{
		Disabled,
		_2x,
		_4x
	}

	public enum DisplayMode
	{
		Normal,
		AOOnly,
		ColorBleedingOnly,
		SplitWithoutAOAndWithAO,
		SplitWithAOAndAOOnly,
		SplitWithoutAOAndAOOnly
	}

	public enum Blur
	{
		None,
		Narrow,
		Medium,
		Wide,
		ExtraWide
	}

	public enum NoiseType
	{
		Random,
		Dither
	}

	public enum PerPixelNormals
	{
		GBuffer,
		Camera,
		Reconstruct
	}

	[Serializable]
	public struct Presets
	{
		public Preset preset;

		[SerializeField]
		public static Presets defaultPresets => new Presets
		{
			preset = Preset.Normal
		};
	}

	[Serializable]
	public struct GeneralSettings
	{
		[Tooltip("The stage the AO is integrated into the rendering pipeline.")]
		[Space(6f)]
		public IntegrationStage integrationStage;

		[Tooltip("The quality of the AO.")]
		[Space(10f)]
		public Quality quality;

		[Tooltip("The deinterleaving factor.")]
		public Deinterleaving deinterleaving;

		[Tooltip("The resolution at which the AO is calculated.")]
		public Resolution resolution;

		[Tooltip("The type of noise to use.")]
		[Space(10f)]
		public NoiseType noiseType;

		[Tooltip("The way the AO is displayed on screen.")]
		[Space(10f)]
		public DisplayMode displayMode;

		[SerializeField]
		public static GeneralSettings defaultSettings => new GeneralSettings
		{
			integrationStage = IntegrationStage.AfterLighting,
			quality = Quality.Medium,
			deinterleaving = Deinterleaving.Disabled,
			resolution = Resolution.Full,
			noiseType = NoiseType.Dither,
			displayMode = DisplayMode.Normal
		};
	}

	[Serializable]
	public struct AOSettings
	{
		[Tooltip("AO radius: this is the distance outside which occluders are ignored.")]
		[Space(6f)]
		[Range(0f, 2f)]
		public float radius;

		[Tooltip("Maximum radius in pixels: this prevents the radius to grow too much with close-up object and impact on performances.")]
		[Range(64f, 512f)]
		public float maxRadiusPixels;

		[Tooltip("For low-tessellated geometry, occlusion variations tend to appear at creases and ridges, which betray the underlying tessellation. To remove these artifacts, we use an angle bias parameter which restricts the hemisphere.")]
		[Range(0f, 0.5f)]
		public float bias;

		[Tooltip("This value allows to scale up the ambient occlusion values.")]
		[Range(0f, 10f)]
		public float intensity;

		[Tooltip("Enable/disable MultiBounce approximation.")]
		public bool useMultiBounce;

		[Tooltip("MultiBounce approximation influence.")]
		[Range(0f, 1f)]
		public float multiBounceInfluence;

		[Tooltip("The amount of AO offscreen samples are contributing.")]
		[Range(0f, 1f)]
		public float offscreenSamplesContribution;

		[Tooltip("The max distance to display AO.")]
		[Space(10f)]
		public float maxDistance;

		[Tooltip("The distance before max distance at which AO start to decrease.")]
		public float distanceFalloff;

		[Tooltip("The type of per pixel normals to use.")]
		[Space(10f)]
		public PerPixelNormals perPixelNormals;

		[Tooltip("This setting allow you to set the base color if the AO, the alpha channel value is unused.")]
		[Space(10f)]
		public Color baseColor;

		[SerializeField]
		public static AOSettings defaultSettings => new AOSettings
		{
			radius = 0.8f,
			maxRadiusPixels = 128f,
			bias = 0.05f,
			intensity = 0.75f,
			useMultiBounce = false,
			multiBounceInfluence = 1f,
			offscreenSamplesContribution = 0f,
			maxDistance = 150f,
			distanceFalloff = 50f,
			perPixelNormals = PerPixelNormals.GBuffer,
			baseColor = Color.black
		};
	}

	[Serializable]
	public struct ColorBleedingSettings
	{
		[Space(6f)]
		public bool enabled;

		[Tooltip("This value allows to control the saturation of the color bleeding.")]
		[Space(10f)]
		[Range(0f, 4f)]
		public float saturation;

		[Tooltip("This value allows to scale the contribution of the color bleeding samples.")]
		[Range(0f, 32f)]
		public float albedoMultiplier;

		[Tooltip("Use masking on emissive pixels")]
		[Range(0f, 1f)]
		public float brightnessMask;

		[Tooltip("Brightness level where masking starts/ends")]
		[GAttribute15(0f, 8f)]
		public Vector2 brightnessMaskRange;

		[SerializeField]
		public static ColorBleedingSettings defaultSettings => new ColorBleedingSettings
		{
			enabled = false,
			saturation = 0.75f,
			albedoMultiplier = 3.2f,
			brightnessMask = 1f,
			brightnessMaskRange = new Vector2(0.8f, 1.2f)
		};
	}

	[Serializable]
	public struct BlurSettings
	{
		[Tooltip("The type of blur to use.")]
		[Space(6f)]
		public Blur amount;

		[Tooltip("This parameter controls the depth-dependent weight of the bilateral filter, to avoid bleeding across edges. A zero sharpness is a pure Gaussian blur. Increasing the blur sharpness removes bleeding by using lower weights for samples with large depth delta from the current pixel.")]
		[Space(10f)]
		[Range(0f, 16f)]
		public float sharpness;

		[Tooltip("Is the blur downsampled.")]
		public bool downsample;

		[SerializeField]
		public static BlurSettings defaultSettings => new BlurSettings
		{
			amount = Blur.Medium,
			sharpness = 8f,
			downsample = false
		};
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class GAttribute14 : Attribute
	{
	}

	public abstract class Class618
	{
		public static float[] Numbers = new float[32]
		{
			0.463937f, 0.340042f, 0.223035f, 0.468465f, 0.322224f, 0.979269f, 0.031798f, 0.973392f, 0.778313f, 0.456168f,
			0.258593f, 0.330083f, 0.387332f, 0.380117f, 0.179842f, 0.910755f, 0.511623f, 0.092933f, 0.180794f, 0.620153f,
			0.101348f, 0.556342f, 0.642479f, 0.442008f, 0.215115f, 0.475218f, 0.157357f, 0.568868f, 0.501241f, 0.629229f,
			0.699218f, 0.707733f
		};
	}

	public abstract class Pass
	{
		public const int AO_LowestQuality = 0;

		public const int AO_LowQuality = 1;

		public const int AO_MediumQuality = 2;

		public const int AO_HighQuality = 3;

		public const int AO_HighestQuality = 4;

		public const int AO_Deinterleaved_LowestQuality = 5;

		public const int AO_Deinterleaved_LowQuality = 6;

		public const int AO_Deinterleaved_MediumQuality = 7;

		public const int AO_Deinterleaved_HighQuality = 8;

		public const int AO_Deinterleaved_HighestQuality = 9;

		public const int Depth_Deinterleaving_2x2 = 10;

		public const int Depth_Deinterleaving_4x4 = 11;

		public const int Normals_Deinterleaving_2x2 = 12;

		public const int Normals_Deinterleaving_4x4 = 13;

		public const int Atlas = 14;

		public const int Reinterleaving_2x2 = 15;

		public const int Reinterleaving_4x4 = 16;

		public const int Blur_X_Narrow = 17;

		public const int Blur_X_Medium = 18;

		public const int Blur_X_Wide = 19;

		public const int Blur_X_ExtraWide = 20;

		public const int Blur_Y_Narrow = 21;

		public const int Blur_Y_Medium = 22;

		public const int Blur_Y_Wide = 23;

		public const int Blur_Y_ExtraWide = 24;

		public const int Composite = 25;

		public const int Composite_MultiBounce = 26;

		public const int Debug_AO_Only = 27;

		public const int Debug_AO_Only_MultiBounce = 28;

		public const int Debug_ColorBleeding_Only = 29;

		public const int Debug_Split_WithoutAO_WithAO = 30;

		public const int Debug_Split_WithoutAO_WithAO_MultiBounce = 31;

		public const int Debug_Split_WithAO_AOOnly = 32;

		public const int Debug_Split_WithAO_AOOnly_MultiBounce = 33;

		public const int Debug_Split_WithoutAO_AOOnly = 34;

		public const int Debug_Split_WithoutAO_AOOnly_MultiBounce = 35;

		public const int Combine_Deffered = 36;

		public const int Combine_Deffered_Multiplicative = 37;

		public const int Combine_Integrated = 38;

		public const int Combine_Integrated_MultiBounce = 39;

		public const int Combine_Integrated_Multiplicative = 40;

		public const int Combine_Integrated_Multiplicative_MultiBounce = 41;

		public const int Combine_ColorBleeding = 42;

		public const int Debug_Split_Additive = 43;

		public const int Debug_Split_Additive_MultiBounce = 44;

		public const int Debug_Split_Multiplicative = 45;

		public const int Debug_Split_Multiplicative_MultiBounce = 46;
	}

	public class RenderTarget
	{
		public bool orthographic;

		public RenderingPath renderingPath;

		public bool hdr;

		public int width;

		public int height;

		public int fullWidth;

		public int fullHeight;

		public int layerWidth;

		public int layerHeight;

		public int downsamplingFactor;

		public int deinterleavingFactor;

		public int blurDownsamplingFactor;
	}

	public abstract class ShaderProperties
	{
		public static int mainTex;

		public static int hbaoTex;

		public static int noiseTex;

		public static int rt0Tex;

		public static int rt3Tex;

		public static int depthTex;

		public static int normalsTex;

		public static int[] mrtDepthTex;

		public static int[] mrtNrmTex;

		public static int[] mrtHBAOTex;

		public static int[] deinterleavingOffset;

		public static int layerOffset;

		public static int jitter;

		public static int uvToView;

		public static int worldToCameraMatrix;

		public static int fullResTexelSize;

		public static int layerResTexelSize;

		public static int targetScale;

		public static int noiseTexSize;

		public static int radius;

		public static int maxRadiusPixels;

		public static int negInvRadius2;

		public static int angleBias;

		public static int aoMultiplier;

		public static int intensity;

		public static int multiBounceInfluence;

		public static int offscreenSamplesContrib;

		public static int maxDistance;

		public static int distanceFalloff;

		public static int baseColor;

		public static int colorBleedSaturation;

		public static int albedoMultiplier;

		public static int colorBleedBrightnessMask;

		public static int colorBleedBrightnessMaskRange;

		public static int blurSharpness;

		static ShaderProperties()
		{
			mainTex = Shader.PropertyToID("_MainTex");
			hbaoTex = Shader.PropertyToID("_HBAOTex");
			noiseTex = Shader.PropertyToID("_NoiseTex");
			rt0Tex = Shader.PropertyToID("_rt0Tex");
			rt3Tex = Shader.PropertyToID("_rt3Tex");
			depthTex = Shader.PropertyToID("_DepthTex");
			normalsTex = Shader.PropertyToID("_NormalsTex");
			mrtDepthTex = new int[16];
			mrtNrmTex = new int[16];
			mrtHBAOTex = new int[16];
			for (int i = 0; i < 16; i++)
			{
				mrtDepthTex[i] = Shader.PropertyToID("_DepthLayerTex" + i);
				mrtNrmTex[i] = Shader.PropertyToID("_NormalLayerTex" + i);
				mrtHBAOTex[i] = Shader.PropertyToID("_HBAOLayerTex" + i);
			}
			deinterleavingOffset = new int[4]
			{
				Shader.PropertyToID("_Deinterleaving_Offset00"),
				Shader.PropertyToID("_Deinterleaving_Offset10"),
				Shader.PropertyToID("_Deinterleaving_Offset01"),
				Shader.PropertyToID("_Deinterleaving_Offset11")
			};
			layerOffset = Shader.PropertyToID("_LayerOffset");
			jitter = Shader.PropertyToID("_Jitter");
			uvToView = Shader.PropertyToID("_UVToView");
			worldToCameraMatrix = Shader.PropertyToID("_WorldToCameraMatrix");
			fullResTexelSize = Shader.PropertyToID("_FullRes_TexelSize");
			layerResTexelSize = Shader.PropertyToID("_LayerRes_TexelSize");
			targetScale = Shader.PropertyToID("_TargetScale");
			noiseTexSize = Shader.PropertyToID("_NoiseTexSize");
			radius = Shader.PropertyToID("_Radius");
			maxRadiusPixels = Shader.PropertyToID("_MaxRadiusPixels");
			negInvRadius2 = Shader.PropertyToID("_NegInvRadius2");
			angleBias = Shader.PropertyToID("_AngleBias");
			aoMultiplier = Shader.PropertyToID("_AOmultiplier");
			intensity = Shader.PropertyToID("_Intensity");
			multiBounceInfluence = Shader.PropertyToID("_MultiBounceInfluence");
			offscreenSamplesContrib = Shader.PropertyToID("_OffscreenSamplesContrib");
			maxDistance = Shader.PropertyToID("_MaxDistance");
			distanceFalloff = Shader.PropertyToID("_DistanceFalloff");
			baseColor = Shader.PropertyToID("_BaseColor");
			colorBleedSaturation = Shader.PropertyToID("_ColorBleedSaturation");
			albedoMultiplier = Shader.PropertyToID("_AlbedoMultiplier");
			colorBleedBrightnessMask = Shader.PropertyToID("_ColorBleedBrightnessMask");
			colorBleedBrightnessMaskRange = Shader.PropertyToID("_ColorBleedBrightnessMaskRange");
			blurSharpness = Shader.PropertyToID("_BlurSharpness");
		}
	}

	public Texture2D noiseTex;

	public Mesh quadMesh;

	public Shader hbaoShader;

	[SerializeField]
	[GAttribute14]
	private Presets m_Presets = Presets.defaultPresets;

	[SerializeField]
	[GAttribute14]
	private GeneralSettings m_GeneralSettings = GeneralSettings.defaultSettings;

	[SerializeField]
	[GAttribute14]
	private AOSettings m_AOSettings = AOSettings.defaultSettings;

	[SerializeField]
	[GAttribute14]
	private ColorBleedingSettings m_ColorBleedingSettings = ColorBleedingSettings.defaultSettings;

	[SerializeField]
	[GAttribute14]
	private BlurSettings m_BlurSettings = BlurSettings.defaultSettings;

	protected Material _hbaoMaterial;

	protected Camera _hbaoCamera;

	protected RenderTarget _renderTarget;

	protected const int NUM_MRTS = 4;

	protected Vector4[] _jitter = new Vector4[16];

	private Quality quality_0;

	private NoiseType noiseType_0;

	private string[] string_0 = new string[4];

	private int[] int_0 = new int[5] { 3, 4, 6, 8, 8 };

	private int int_1 = 4;

	public Presets presets
	{
		get
		{
			return m_Presets;
		}
		set
		{
			m_Presets = value;
		}
	}

	public GeneralSettings generalSettings
	{
		get
		{
			return m_GeneralSettings;
		}
		set
		{
			m_GeneralSettings = value;
		}
	}

	public AOSettings aoSettings
	{
		get
		{
			return m_AOSettings;
		}
		set
		{
			m_AOSettings = value;
		}
	}

	public ColorBleedingSettings colorBleedingSettings
	{
		get
		{
			return m_ColorBleedingSettings;
		}
		set
		{
			m_ColorBleedingSettings = value;
		}
	}

	public BlurSettings blurSettings
	{
		get
		{
			return m_BlurSettings;
		}
		set
		{
			m_BlurSettings = value;
		}
	}

	public virtual void Start()
	{
		int_1 = SystemInfo.supportedRenderTargetCount;
	}

	public virtual void OnEnable()
	{
		if (SystemInfo.supportsImageEffects && SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
		{
			if (hbaoShader != null && !hbaoShader.isSupported)
			{
				Debug.LogWarning("HBAO shader is not supported on this platform.");
				base.enabled = false;
			}
			else if (!(hbaoShader == null))
			{
				method_0();
				_hbaoCamera.depthTextureMode |= DepthTextureMode.Depth;
				if (aoSettings.perPixelNormals == PerPixelNormals.Camera)
				{
					_hbaoCamera.depthTextureMode |= DepthTextureMode.DepthNormals;
				}
				_hbaoCamera.forceIntoRenderTexture = true;
			}
		}
		else
		{
			Debug.LogWarning("HBAO shader is not supported on this platform.");
			base.enabled = false;
		}
	}

	public virtual void OnDisable()
	{
		Shader.SetGlobalTexture(ShaderProperties.hbaoTex, Texture2D.whiteTexture);
		if (_hbaoMaterial != null)
		{
			UnityEngine.Object.DestroyImmediate(_hbaoMaterial);
		}
		if (noiseTex != null)
		{
			UnityEngine.Object.DestroyImmediate(noiseTex);
		}
		if (quadMesh != null)
		{
			UnityEngine.Object.DestroyImmediate(quadMesh);
		}
	}

	public void method_0()
	{
		if (_hbaoMaterial == null)
		{
			_hbaoMaterial = new Material(hbaoShader);
			_hbaoMaterial.hideFlags = HideFlags.HideAndDontSave;
			_hbaoCamera = GetComponent<Camera>();
		}
		if (quadMesh != null)
		{
			UnityEngine.Object.DestroyImmediate(quadMesh);
		}
		quadMesh = new Mesh();
		quadMesh.vertices = new Vector3[4]
		{
			new Vector3(-0.5f, -0.5f, 0f),
			new Vector3(0.5f, 0.5f, 0f),
			new Vector3(0.5f, -0.5f, 0f),
			new Vector3(-0.5f, 0.5f, 0f)
		};
		quadMesh.uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 1f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f)
		};
		quadMesh.triangles = new int[6] { 0, 1, 2, 1, 0, 3 };
		_renderTarget = new RenderTarget();
	}

	public virtual void UpdateShaderProperties()
	{
		_renderTarget.orthographic = _hbaoCamera.orthographic;
		_renderTarget.renderingPath = _hbaoCamera.actualRenderingPath;
		_renderTarget.hdr = _hbaoCamera.allowHDR;
		if (XRSettings.enabled)
		{
			_renderTarget.width = XRSettings.eyeTextureDesc.width;
			_renderTarget.height = XRSettings.eyeTextureDesc.height;
		}
		else
		{
			_renderTarget.width = _hbaoCamera.pixelWidth;
			_renderTarget.height = _hbaoCamera.pixelHeight;
		}
		_renderTarget.downsamplingFactor = ((generalSettings.resolution == Resolution.Full) ? 1 : ((generalSettings.resolution == Resolution.Half) ? 2 : 4));
		_renderTarget.deinterleavingFactor = GetDeinterleavingFactor();
		_renderTarget.blurDownsamplingFactor = ((!blurSettings.downsample) ? 1 : 2);
		float num = Mathf.Tan(0.5f * _hbaoCamera.fieldOfView * (MathF.PI / 180f));
		float num2 = 1f / (1f / num * ((float)_renderTarget.height / (float)_renderTarget.width));
		float num3 = 1f / (1f / num);
		_hbaoMaterial.SetVector(ShaderProperties.uvToView, new Vector4(2f * num2, -2f * num3, -1f * num2, 1f * num3));
		_hbaoMaterial.SetMatrix(ShaderProperties.worldToCameraMatrix, _hbaoCamera.worldToCameraMatrix);
		if (generalSettings.deinterleaving != Deinterleaving.Disabled)
		{
			_renderTarget.fullWidth = _renderTarget.width + ((_renderTarget.width % _renderTarget.deinterleavingFactor != 0) ? (_renderTarget.deinterleavingFactor - _renderTarget.width % _renderTarget.deinterleavingFactor) : 0);
			_renderTarget.fullHeight = _renderTarget.height + ((_renderTarget.height % _renderTarget.deinterleavingFactor != 0) ? (_renderTarget.deinterleavingFactor - _renderTarget.height % _renderTarget.deinterleavingFactor) : 0);
			_renderTarget.layerWidth = _renderTarget.fullWidth / _renderTarget.deinterleavingFactor;
			_renderTarget.layerHeight = _renderTarget.fullHeight / _renderTarget.deinterleavingFactor;
			_hbaoMaterial.SetVector(ShaderProperties.fullResTexelSize, new Vector4(1f / (float)_renderTarget.fullWidth, 1f / (float)_renderTarget.fullHeight, _renderTarget.fullWidth, _renderTarget.fullHeight));
			_hbaoMaterial.SetVector(ShaderProperties.layerResTexelSize, new Vector4(1f / (float)_renderTarget.layerWidth, 1f / (float)_renderTarget.layerHeight, _renderTarget.layerWidth, _renderTarget.layerHeight));
			_hbaoMaterial.SetVector(ShaderProperties.targetScale, new Vector4((float)_renderTarget.fullWidth / (float)_renderTarget.width, (float)_renderTarget.fullHeight / (float)_renderTarget.height, 1f / ((float)_renderTarget.fullWidth / (float)_renderTarget.width), 1f / ((float)_renderTarget.fullHeight / (float)_renderTarget.height)));
		}
		else
		{
			_renderTarget.fullWidth = _renderTarget.width;
			_renderTarget.fullHeight = _renderTarget.height;
			if (generalSettings.resolution == Resolution.Half && aoSettings.perPixelNormals == PerPixelNormals.Reconstruct)
			{
				_hbaoMaterial.SetVector(ShaderProperties.targetScale, new Vector4(((float)_renderTarget.width + 0.5f) / (float)_renderTarget.width, ((float)_renderTarget.height + 0.5f) / (float)_renderTarget.height, 1f, 1f));
			}
			else
			{
				_hbaoMaterial.SetVector(ShaderProperties.targetScale, new Vector4(1f, 1f, 1f, 1f));
			}
		}
		if (noiseTex == null || quality_0 != generalSettings.quality || noiseType_0 != generalSettings.noiseType)
		{
			if (noiseTex != null)
			{
				UnityEngine.Object.DestroyImmediate(noiseTex);
			}
			float num4 = ((generalSettings.noiseType == NoiseType.Dither) ? 4 : 64);
			method_1((int)num4);
		}
		quality_0 = generalSettings.quality;
		noiseType_0 = generalSettings.noiseType;
		_hbaoMaterial.SetTexture(ShaderProperties.noiseTex, noiseTex);
		_hbaoMaterial.SetFloat(ShaderProperties.noiseTexSize, (noiseType_0 == NoiseType.Dither) ? 4 : 64);
		_hbaoMaterial.SetFloat(ShaderProperties.radius, aoSettings.radius * 0.5f * ((float)_renderTarget.height / (num * 2f)) / (float)_renderTarget.deinterleavingFactor);
		_hbaoMaterial.SetFloat(ShaderProperties.maxRadiusPixels, aoSettings.maxRadiusPixels / (float)_renderTarget.deinterleavingFactor);
		_hbaoMaterial.SetFloat(ShaderProperties.negInvRadius2, -1f / (aoSettings.radius * aoSettings.radius));
		_hbaoMaterial.SetFloat(ShaderProperties.angleBias, aoSettings.bias);
		_hbaoMaterial.SetFloat(ShaderProperties.aoMultiplier, 2f * (1f / (1f - aoSettings.bias)));
		_hbaoMaterial.SetFloat(ShaderProperties.intensity, aoSettings.intensity);
		_hbaoMaterial.SetFloat(ShaderProperties.multiBounceInfluence, aoSettings.multiBounceInfluence);
		_hbaoMaterial.SetFloat(ShaderProperties.offscreenSamplesContrib, aoSettings.offscreenSamplesContribution);
		_hbaoMaterial.SetFloat(ShaderProperties.maxDistance, aoSettings.maxDistance);
		_hbaoMaterial.SetFloat(ShaderProperties.distanceFalloff, aoSettings.distanceFalloff);
		_hbaoMaterial.SetColor(ShaderProperties.baseColor, aoSettings.baseColor);
		_hbaoMaterial.SetFloat(ShaderProperties.colorBleedSaturation, colorBleedingSettings.saturation);
		_hbaoMaterial.SetFloat(ShaderProperties.albedoMultiplier, colorBleedingSettings.albedoMultiplier);
		_hbaoMaterial.SetFloat(ShaderProperties.colorBleedBrightnessMask, colorBleedingSettings.brightnessMask);
		_hbaoMaterial.SetVector(ShaderProperties.colorBleedBrightnessMaskRange, colorBleedingSettings.brightnessMaskRange);
		_hbaoMaterial.SetFloat(ShaderProperties.blurSharpness, blurSettings.sharpness);
	}

	public void UpdateShaderKeywords()
	{
		string_0[0] = (colorBleedingSettings.enabled ? "COLOR_BLEEDING_ON" : "__");
		if (_renderTarget.orthographic)
		{
			string_0[1] = "ORTHOGRAPHIC_PROJECTION_ON";
		}
		else
		{
			string_0[1] = (IsDeferredShading() ? "DEFERRED_SHADING_ON" : "__");
		}
		string_0[2] = ((aoSettings.perPixelNormals == PerPixelNormals.Camera) ? "NORMALS_CAMERA" : ((aoSettings.perPixelNormals == PerPixelNormals.Reconstruct) ? "NORMALS_RECONSTRUCT" : "__"));
		string_0[3] = ((aoSettings.offscreenSamplesContribution > 0f) ? "OFFSCREEN_SAMPLES_CONTRIB" : "__");
		_hbaoMaterial.shaderKeywords = string_0;
	}

	public virtual void CheckParameters()
	{
		if (!IsDeferredShading() && aoSettings.perPixelNormals == PerPixelNormals.GBuffer)
		{
			m_AOSettings.perPixelNormals = PerPixelNormals.Camera;
		}
		if (generalSettings.deinterleaving != Deinterleaving.Disabled && int_1 < 4)
		{
			m_GeneralSettings.deinterleaving = Deinterleaving.Disabled;
		}
	}

	public bool IsDeferredShading()
	{
		return _hbaoCamera.actualRenderingPath == RenderingPath.DeferredShading;
	}

	public bool IsDeferredShadingOrLighting()
	{
		if (_hbaoCamera.actualRenderingPath != RenderingPath.DeferredShading)
		{
			return _hbaoCamera.actualRenderingPath == RenderingPath.DeferredLighting;
		}
		return true;
	}

	public int GetDeinterleavingFactor()
	{
		return generalSettings.deinterleaving switch
		{
			Deinterleaving._2x => 2, 
			Deinterleaving._4x => 4, 
			_ => 1, 
		};
	}

	public int GetAoPass()
	{
		return generalSettings.quality switch
		{
			Quality.Lowest => 0, 
			Quality.Low => 1, 
			Quality.Medium => 2, 
			Quality.High => 3, 
			Quality.Highest => 4, 
			_ => 2, 
		};
	}

	public int GetAoDeinterleavedPass()
	{
		return generalSettings.quality switch
		{
			Quality.Lowest => 5, 
			Quality.Low => 6, 
			Quality.Medium => 7, 
			Quality.High => 8, 
			Quality.Highest => 9, 
			_ => 7, 
		};
	}

	public int GetBlurXPass()
	{
		return blurSettings.amount switch
		{
			Blur.Narrow => 17, 
			Blur.Medium => 18, 
			Blur.Wide => 19, 
			Blur.ExtraWide => 20, 
			_ => 18, 
		};
	}

	public int GetBlurYPass()
	{
		return blurSettings.amount switch
		{
			Blur.Narrow => 21, 
			Blur.Medium => 22, 
			Blur.Wide => 23, 
			Blur.ExtraWide => 24, 
			_ => 22, 
		};
	}

	public int GetFinalPass()
	{
		switch (generalSettings.displayMode)
		{
		default:
			return 25;
		case DisplayMode.Normal:
			if (!aoSettings.useMultiBounce)
			{
				return 25;
			}
			return 26;
		case DisplayMode.AOOnly:
			if (!aoSettings.useMultiBounce)
			{
				return 27;
			}
			return 28;
		case DisplayMode.ColorBleedingOnly:
			return 29;
		case DisplayMode.SplitWithoutAOAndWithAO:
			if (!aoSettings.useMultiBounce)
			{
				return 30;
			}
			return 31;
		case DisplayMode.SplitWithAOAndAOOnly:
			if (!aoSettings.useMultiBounce)
			{
				return 32;
			}
			return 33;
		case DisplayMode.SplitWithoutAOAndAOOnly:
			if (!aoSettings.useMultiBounce)
			{
				return 34;
			}
			return 35;
		}
	}

	public void method_1(int size)
	{
		noiseTex = new Texture2D(size, size, TextureFormat.RGB24, mipChain: false, linear: true);
		noiseTex.filterMode = FilterMode.Point;
		noiseTex.wrapMode = TextureWrapMode.Repeat;
		int num = 0;
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				float num2 = ((generalSettings.noiseType == NoiseType.Dither) ? Class618.Numbers[num++] : UnityEngine.Random.Range(0f, 1f));
				float b = ((generalSettings.noiseType == NoiseType.Dither) ? Class618.Numbers[num++] : UnityEngine.Random.Range(0f, 1f));
				float f = MathF.PI * 2f * num2 / (float)int_0[GetAoPass()];
				Color color = new Color(Mathf.Cos(f), Mathf.Sin(f), b);
				noiseTex.SetPixel(i, j, color);
			}
		}
		noiseTex.Apply();
		int k = 0;
		int num3 = 0;
		for (; k < _jitter.Length; k++)
		{
			float num4 = Class618.Numbers[num3++];
			float z = Class618.Numbers[num3++];
			float f2 = MathF.PI * 2f * num4 / (float)int_0[GetAoPass()];
			_jitter[k] = new Vector4(Mathf.Cos(f2), Mathf.Sin(f2), z, 0f);
		}
	}

	public void ApplyPreset(Preset preset)
	{
		if (preset == Preset.Custom)
		{
			m_Presets.preset = preset;
			return;
		}
		DisplayMode displayMode = generalSettings.displayMode;
		m_GeneralSettings = GeneralSettings.defaultSettings;
		m_AOSettings = AOSettings.defaultSettings;
		m_ColorBleedingSettings = ColorBleedingSettings.defaultSettings;
		m_BlurSettings = BlurSettings.defaultSettings;
		m_GeneralSettings.displayMode = displayMode;
		switch (preset)
		{
		case Preset.FastestPerformance:
			m_GeneralSettings.quality = Quality.Lowest;
			m_AOSettings.radius = 0.5f;
			m_AOSettings.maxRadiusPixels = 64f;
			m_BlurSettings.amount = Blur.ExtraWide;
			m_BlurSettings.downsample = true;
			break;
		case Preset.FastPerformance:
			m_GeneralSettings.quality = Quality.Low;
			m_AOSettings.radius = 1f;
			m_AOSettings.maxRadiusPixels = 64f;
			m_BlurSettings.amount = Blur.Wide;
			m_BlurSettings.downsample = true;
			break;
		case Preset.HighQuality:
			m_GeneralSettings.quality = Quality.High;
			m_AOSettings.radius = 1.46f;
			m_BlurSettings.amount = Blur.Narrow;
			break;
		case Preset.HighestQuality:
			m_GeneralSettings.quality = Quality.Highest;
			m_AOSettings.radius = 1.46f;
			m_AOSettings.maxRadiusPixels = 256f;
			m_BlurSettings.amount = Blur.Narrow;
			break;
		case Preset.ColoredHighestQuality:
			m_GeneralSettings.quality = Quality.Highest;
			m_AOSettings.radius = 1.46f;
			m_AOSettings.maxRadiusPixels = 256f;
			m_BlurSettings.amount = Blur.Narrow;
			m_ColorBleedingSettings.enabled = true;
			break;
		}
		m_Presets.preset = preset;
	}
}
