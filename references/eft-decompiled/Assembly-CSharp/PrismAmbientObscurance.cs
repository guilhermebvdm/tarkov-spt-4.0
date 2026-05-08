using EFT.BlitDebug;
using Prism.Utils;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class PrismAmbientObscurance : MonoBehaviour
{
	public Material m_Material;

	public Shader m_Shader;

	public Material m_AOMaterial;

	public Shader m_AOShader;

	private Camera camera_0;

	private SSAA ssaa_0;

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

	public bool advancedAO;

	private static readonly int int_0 = Shader.PropertyToID("_AOIntensity");

	private static readonly int int_1 = Shader.PropertyToID("_AOLuminanceWeighting");

	private static readonly int int_2 = Shader.PropertyToID("_AOSampleCount");

	private static readonly int int_3 = Shader.PropertyToID("_AOSpiralTurns");

	private static readonly int int_4 = Shader.PropertyToID("_AORadius");

	private static readonly int int_5 = Shader.PropertyToID("_AOBias");

	private static readonly int int_6 = Shader.PropertyToID("_AOTargetScale");

	private static readonly int int_7 = Shader.PropertyToID("_AOCutoff");

	private static readonly int int_8 = Shader.PropertyToID("_AOCutoffRange");

	private static readonly int int_9 = Shader.PropertyToID("_AOCameraModelView");

	private static readonly int int_10 = Shader.PropertyToID("_AOProjInfo");

	private static readonly int int_11 = Shader.PropertyToID("_AOTex_PrismAO");

	private static readonly int int_12 = Shader.PropertyToID("_AOBlurVector");

	private static readonly int int_13 = Shader.PropertyToID("_AOTex");

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

	public bool UsingTerrain => Terrain.activeTerrain;

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

	public void OnEnable()
	{
		camera_0 = GetComponent<Camera>();
		ssaa_0 = GetComponent<SSAA>();
		if (!m_Shader)
		{
			m_Shader = GClass872.Find("Hidden/PrismAmbientObscurance");
			if (!m_Shader)
			{
				Debug.LogError("Couldn't find shader for PRISM! You shouldn't see this error.");
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
		if (useAmbientObscurance && (!IsGBufferAvailable || UsingTerrain))
		{
			camera_0.depthTextureMode |= DepthTextureMode.Depth;
			camera_0.depthTextureMode |= DepthTextureMode.DepthNormals;
		}
	}

	public void OnDisable()
	{
		if ((bool)m_Material)
		{
			Object.DestroyImmediate(m_Material);
			m_Material = null;
		}
		if ((bool)m_AOMaterial)
		{
			Object.DestroyImmediate(m_AOMaterial);
			m_AOMaterial = null;
		}
		if (m_AOShader == m_Shader)
		{
			m_AOShader = null;
		}
	}

	public Material method_0(Shader shader)
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

	public bool method_1()
	{
		if (m_Material == null && m_Shader != null && m_Shader.isSupported)
		{
			m_Material = method_0(m_Shader);
		}
		if (m_AOMaterial == null && m_AOShader != null && m_AOShader.isSupported)
		{
			m_AOMaterial = method_0(m_AOShader);
		}
		if (!m_Shader.isSupported)
		{
			Debug.LogError("Prism is not supported on this platform, or you have a shader compilation error somewhere. Disabling.");
			base.enabled = false;
			return false;
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
		m_AOMaterial.shaderKeywords = null;
		if (camera_0.allowHDR)
		{
			m_Material.EnableKeyword("PRISM_LINEAR_LOOKUP");
			m_Material.DisableKeyword("PRISM_GAMMA_LOOKUP");
		}
		else
		{
			m_Material.EnableKeyword("PRISM_GAMMA_LOOKUP");
			m_Material.DisableKeyword("PRISM_LINEAR_LOOKUP");
		}
		if (useAmbientObscurance)
		{
			if (!IsGBufferAvailable || UsingTerrain)
			{
				camera_0.depthTextureMode |= DepthTextureMode.Depth;
				camera_0.depthTextureMode |= DepthTextureMode.DepthNormals;
			}
			m_Material.SetFloat(int_0, aoIntensity);
			m_Material.SetFloat(int_1, aoLightingContribution);
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
				m_AOMaterial.SetInt(int_2, aoSampleCountValue);
			}
			else
			{
				m_AOMaterial.EnableKeyword("_AOSAMPLECOUNT_CUSTOM");
				m_AOMaterial.DisableKeyword("_AOSAMPLECOUNT_LOWEST");
				m_AOMaterial.SetInt(int_2, aoSampleCountValue);
			}
			m_AOMaterial.SetInt(int_3, aoSampleCountValue);
		}
		else
		{
			m_Material.SetFloat(int_0, aoMinIntensity);
		}
	}

	public void method_2(Material aoMaterial)
	{
		m_Material.SetFloat(int_1, aoLightingContribution);
		aoMaterial.SetFloat(int_0, aoIntensity);
		aoMaterial.SetFloat(int_4, aoRadius);
		aoMaterial.SetFloat(int_5, aoBias * 0.02f);
		aoMaterial.SetFloat(int_6, aoDownsample ? 0.5f : 1f);
		aoMaterial.SetFloat(int_7, aoDistanceCutoffStart);
		aoMaterial.SetFloat(int_8, aoDistanceCutoffLength);
		aoMaterial.SetMatrix(int_9, camera_0.cameraToWorldMatrix);
		int num = (ssaa_0 ? ssaa_0.GetInputWidth() : Screen.width);
		int num2 = (ssaa_0 ? ssaa_0.GetInputHeight() : Screen.height);
		Matrix4x4 projectionMatrix = camera_0.projectionMatrix;
		aoMaterial.SetVector(value: new Vector4(-2f / ((float)num * projectionMatrix[0]), -2f / ((float)num2 * projectionMatrix[5]), (1f - projectionMatrix[2]) / projectionMatrix[0], (1f + projectionMatrix[6]) / projectionMatrix[5]), nameID: int_10);
	}

	[ImageEffectOpaque]
	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!useAmbientObscurance)
		{
			GClass860.BlitOrCopy(source, destination);
			Shader.SetGlobalTexture(int_11, Texture2D.blackTexture);
			return;
		}
		if (!method_1())
		{
			Graphics.CopyTexture(source, destination);
			return;
		}
		UpdateShaderValues();
		int num = 1;
		if (aoDownsample)
		{
			num = 2;
		}
		int width = source.width / num;
		int height = source.height / num;
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat_0, RenderTextureReadWrite.Linear);
		temporary.name = "PrismAO RT";
		method_2(m_AOMaterial);
		DebugGraphics.Blit(null, temporary, m_AOMaterial, 0);
		if (aoBlurType == AOBlurType.Fast)
		{
			for (int i = 0; i < aoBlurIterations; i++)
			{
				m_AOMaterial.SetVector(int_12, new Vector4(-1f, 0f, 0f, 0f));
				RenderTexture temporary2 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat_0, RenderTextureReadWrite.Linear);
				temporary2.name = "PrismAO TmpRT3";
				DebugGraphics.Blit(temporary, temporary2, m_AOMaterial, (int)aoBlurType);
				RenderTexture.ReleaseTemporary(temporary);
				m_AOMaterial.SetVector(int_12, new Vector4(0f, 1f, 0f, 0f));
				temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat_0, RenderTextureReadWrite.Linear);
				temporary.name = "PrismAO RT";
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
					m_AOMaterial.SetVector(int_12, new Vector4(-1f, 0f, 0f, 0f));
					RenderTexture temporary2 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat_0, RenderTextureReadWrite.Linear);
					temporary2.name = "PrismAO TmpRT3";
					DebugGraphics.Blit(temporary, temporary2, m_AOMaterial, (int)(aoBlurType + k));
					RenderTexture.ReleaseTemporary(temporary);
					m_AOMaterial.SetVector(int_12, new Vector4(0f, 1f, 0f, 0f));
					temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat_0, RenderTextureReadWrite.Linear);
					temporary.name = "PrismAO RT";
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
			m_Material.SetTexture(int_13, temporary);
			Shader.SetGlobalTexture(int_11, temporary);
			DebugGraphics.Blit(source, destination, m_Material, 0);
		}
		RenderTexture.ReleaseTemporary(temporary);
	}
}
