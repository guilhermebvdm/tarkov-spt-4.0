using EFT.BlitDebug;
using Prism.Utils;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("PRISM/Prism Effects SSAO")]
public class PrismSSAO : MonoBehaviour
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

	public Material m_AOMaterial;

	public Shader m_AOShader;

	public Material m_Material3;

	public Shader m_Shader3;

	private Camera camera_0;

	private SSAA ssaa_0;

	public Texture2D lensDirtTexture;

	public bool useLensDirt = true;

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

	[Space(10f)]
	public bool advancedVignette;

	public bool advancedAO;

	private static readonly int int_0 = Shader.PropertyToID("_FogIntensity");

	private static readonly int int_1 = Shader.PropertyToID("_VignetteIntensity");

	private static readonly int int_2 = Shader.PropertyToID("_ChromaticIntensity");

	private static readonly int int_3 = Shader.PropertyToID("_Gamma");

	private static readonly int int_4 = Shader.PropertyToID("_DirtIntensity");

	private static readonly int int_5 = Shader.PropertyToID("_LutAmount");

	private static readonly int int_6 = Shader.PropertyToID("_SecondLutAmount");

	private static readonly int int_7 = Shader.PropertyToID("useNoise");

	private static readonly int int_8 = Shader.PropertyToID("useNightVision");

	private static readonly int int_9 = Shader.PropertyToID("_SunWeight");

	private static readonly int int_10 = Shader.PropertyToID("_AOIntensity");

	private static readonly int int_11 = Shader.PropertyToID("_AOLuminanceWeighting");

	private static readonly int int_12 = Shader.PropertyToID("_AOSpiralTurns");

	private static readonly int int_13 = Shader.PropertyToID("_AORadius");

	private static readonly int int_14 = Shader.PropertyToID("_AOBias");

	private static readonly int int_15 = Shader.PropertyToID("_AOTargetScale");

	private static readonly int int_16 = Shader.PropertyToID("_AOCutoff");

	private static readonly int int_17 = Shader.PropertyToID("_AOCutoffRange");

	private static readonly int int_18 = Shader.PropertyToID("_AOCameraModelView");

	private static readonly int int_19 = Shader.PropertyToID("_AOProjInfo");

	private static readonly int int_20 = Shader.PropertyToID("_AOBlurVector");

	private static readonly int int_21 = Shader.PropertyToID("_AOTex");

	private static readonly int int_22 = Shader.PropertyToID("_AOSampleCount");

	public RenderTextureFormat RenderTextureFormat_0
	{
		get
		{
			if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8))
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

	public Camera GetPrismCamera()
	{
		if (camera_0 == null)
		{
			camera_0 = GetComponent<Camera>();
			ssaa_0 = GetComponent<SSAA>();
		}
		return camera_0;
	}

	public void SetPrismPreset(PrismPreset preset)
	{
		if (!preset)
		{
			useAmbientObscurance = true;
			return;
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
		Reset();
	}

	public void OnEnable()
	{
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
		camera_0.depthTextureMode |= DepthTextureMode.Depth;
		if (useAmbientObscurance && (!IsGBufferAvailable || UsingTerrain))
		{
			camera_0.depthTextureMode |= DepthTextureMode.Depth;
			camera_0.depthTextureMode |= DepthTextureMode.DepthNormals;
		}
	}

	[ContextMenu("DontRenderDepthTexture")]
	public void method_0()
	{
		camera_0.depthTextureMode = DepthTextureMode.None;
	}

	public void OnDestroy()
	{
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
		}
		if (!m_AOShader.isSupported)
		{
			Debug.LogError("Prism (AO) is not supported on this platform, or you have a shader compilation error somewhere. Disabling AO.");
			m_AOShader = m_Shader;
			useAmbientObscurance = false;
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
		m_Material.SetFloat(int_0, 0f);
		m_Material.SetFloat(int_1, 0f);
		m_Material.SetFloat(int_2, 0f);
		m_Material.SetFloat(int_3, 0f);
		m_Material.SetFloat(int_4, 0f);
		Shader.DisableKeyword("PRISM_USE_EXPOSURE");
		m_Material.SetFloat(int_5, 0f);
		m_Material.SetFloat(int_6, 0f);
		m_Material.DisableKeyword("PRISM_GAMMA_LOOKUP");
		m_Material.DisableKeyword("PRISM_LINEAR_LOOKUP");
		Shader.DisableKeyword("PRISM_FILMIC_TONEMAP");
		Shader.DisableKeyword("PRISM_ROMB_TONEMAP");
		Shader.DisableKeyword("PRISM_ACES_TONEMAP");
		m_Material.SetFloat(int_7, 0f);
		m_Material.SetFloat(int_8, 0f);
		Shader.DisableKeyword("PRISM_USE_NIGHTVISION");
		m_Material.SetFloat(int_9, 0f);
		if (useAmbientObscurance)
		{
			if (!IsGBufferAvailable || UsingTerrain)
			{
				camera_0.depthTextureMode |= DepthTextureMode.Depth;
				camera_0.depthTextureMode |= DepthTextureMode.DepthNormals;
			}
			m_Material.SetFloat(int_10, aoIntensity);
			m_Material.SetFloat(int_11, aoLightingContribution);
			if (useAODistanceCutoff)
			{
				m_AOMaterial.EnableKeyword("_AOCUTOFF_ON");
			}
			else
			{
				m_AOMaterial.DisableKeyword("_AOCUTOFF_ON");
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
				m_AOMaterial.SetInt(int_22, aoSampleCountValue);
			}
			else
			{
				m_AOMaterial.EnableKeyword("_AOSAMPLECOUNT_CUSTOM");
				m_AOMaterial.DisableKeyword("_AOSAMPLECOUNT_LOWEST");
				m_AOMaterial.SetInt(int_22, aoSampleCountValue);
			}
			m_AOMaterial.SetInt(int_12, aoSampleCountValue);
		}
		else
		{
			m_Material.SetFloat(int_10, aoMinIntensity);
		}
	}

	public void method_3(Material aoMaterial)
	{
		m_Material.SetFloat(int_11, aoLightingContribution);
		aoMaterial.SetFloat(int_10, aoIntensity);
		aoMaterial.SetFloat(int_13, aoRadius);
		aoMaterial.SetFloat(int_14, aoBias * 0.02f);
		aoMaterial.SetFloat(int_15, aoDownsample ? 0.5f : 1f);
		aoMaterial.SetFloat(int_16, aoDistanceCutoffStart);
		aoMaterial.SetFloat(int_17, aoDistanceCutoffLength);
		aoMaterial.SetMatrix(int_18, camera_0.cameraToWorldMatrix);
		int num = (ssaa_0 ? ssaa_0.GetInputWidth() : Screen.width);
		int num2 = (ssaa_0 ? ssaa_0.GetInputHeight() : Screen.height);
		Matrix4x4 projectionMatrix = camera_0.projectionMatrix;
		aoMaterial.SetVector(value: new Vector4(-2f / ((float)num * projectionMatrix[0]), -2f / ((float)num2 * projectionMatrix[5]), (1f - projectionMatrix[2]) / projectionMatrix[0], (1f + projectionMatrix[6]) / projectionMatrix[5]), nameID: int_19);
	}

	[ImageEffectOpaque]
	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		bool flag = true;
		if (bool_0)
		{
			Graphics.CopyTexture(source, destination);
			bool_0 = false;
		}
		else if (method_2() && flag)
		{
			UpdateShaderValues();
			int num = 1;
			if (aoDownsample)
			{
				num = 2;
			}
			int width = source.width / num;
			int height = source.height / num;
			RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat_0, RenderTextureReadWrite.Linear);
			temporary.name = "PrismSSAO RT";
			method_3(m_AOMaterial);
			DebugGraphics.Blit(null, temporary, m_AOMaterial, 0);
			if (aoBlurType == AOBlurType.Fast)
			{
				for (int i = 0; i < aoBlurIterations; i++)
				{
					m_AOMaterial.SetVector(int_20, new Vector4(-1f, 0f, 0f, 0f));
					RenderTexture temporary2 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat_0, RenderTextureReadWrite.Linear);
					temporary2.name = "PrismSSAO TmpRT3";
					DebugGraphics.Blit(temporary, temporary2, m_AOMaterial, (int)aoBlurType);
					RenderTexture.ReleaseTemporary(temporary);
					m_AOMaterial.SetVector(int_20, new Vector4(0f, 1f, 0f, 0f));
					temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat_0, RenderTextureReadWrite.Linear);
					temporary.name = "PrismSSAO RT";
					DebugGraphics.Blit(temporary2, temporary, m_AOMaterial, (int)aoBlurType);
					RenderTexture.ReleaseTemporary(temporary2);
				}
			}
			else
			{
				for (int j = 0; j < aoBlurIterations; j++)
				{
					for (int k = 0; k < 2; k++)
					{
						m_AOMaterial.SetVector(int_20, new Vector4(-1f, 0f, 0f, 0f));
						RenderTexture temporary2 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat_0, RenderTextureReadWrite.Linear);
						temporary2.name = "PrismSSAO TmpRT3";
						DebugGraphics.Blit(temporary, temporary2, m_AOMaterial, (int)(aoBlurType + k));
						RenderTexture.ReleaseTemporary(temporary);
						m_AOMaterial.SetVector(int_20, new Vector4(0f, 1f, 0f, 0f));
						temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat_0, RenderTextureReadWrite.Linear);
						temporary.name = "PrismSSAO RT";
						DebugGraphics.Blit(temporary2, temporary, m_AOMaterial, (int)(aoBlurType + k));
						RenderTexture.ReleaseTemporary(temporary2);
					}
				}
			}
			if (aoShowDebug)
			{
				DebugGraphics.Blit(temporary, destination, m_AOMaterial, 2);
			}
			else
			{
				m_Material.SetTexture(int_21, temporary);
				if (isParentPrism)
				{
					Shader.SetGlobalFloat(int_10, aoIntensity);
					Shader.SetGlobalTexture(int_21, temporary);
				}
				DebugGraphics.Blit(source, destination, m_Material, 0);
			}
			RenderTexture.ReleaseTemporary(temporary);
		}
		else
		{
			Graphics.CopyTexture(source, destination);
		}
	}
}
