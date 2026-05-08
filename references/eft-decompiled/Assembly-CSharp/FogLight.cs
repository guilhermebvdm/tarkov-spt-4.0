using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class FogLight : LightOverride
{
	public enum TextureSize
	{
		x256 = 0x100,
		x512 = 0x200,
		x1024 = 0x400
	}

	public bool m_ForceOnForFog;

	[Tooltip("Only one shadowed fog AreaLight at a time.")]
	[Header("Shadows")]
	public bool m_Shadows;

	[Tooltip("Always at most half the res of the AreaLight's shadowmap.")]
	public TextureSize m_ShadowmapRes = TextureSize.x256;

	[Range(0f, 3f)]
	public int m_BlurIterations;

	[GAttribute13(0f)]
	public float m_BlurSize = 1f;

	[GAttribute13(0f)]
	[Tooltip("Affects shadow softness.")]
	public float m_ESMExponent = 40f;

	public bool m_Bounded = true;

	private bool bool_1;

	private AreaLight areaLight_1;

	private CommandBuffer commandBuffer_0;

	private CommandBuffer commandBuffer_1;

	private RenderTexture renderTexture_0;

	private ComputeBuffer computeBuffer_0;

	public Shader m_BlurShadowmapShader;

	private Material material_0;

	public Shader m_CopyShadowParamsShader;

	private Material material_1;

	private int[] int_0;

	public bool Boolean_0
	{
		get
		{
			if (m_Shadows)
			{
				return base.type == Type.Directional;
			}
			return false;
		}
	}

	public override bool GetForceOn()
	{
		return m_ForceOnForFog;
	}

	public void method_1()
	{
		areaLight_1 = GetComponent<AreaLight>();
		if (bool_1 && areaLight_1 != null && areaLight_1.m_Negative)
		{
			LightManager<FogLight>.Remove(this);
			bool_1 = false;
			method_3();
		}
		if (!bool_1 && areaLight_1 != null && !areaLight_1.m_Negative)
		{
			bool_1 = LightManager<FogLight>.Add(this);
		}
	}

	public void OnEnable()
	{
		method_1();
	}

	public new void Update()
	{
		method_1();
	}

	public void OnDisable()
	{
		LightManager<FogLight>.Remove(this);
		bool_1 = false;
		method_3();
	}

	public void method_2()
	{
		if (commandBuffer_0 == null && Boolean_0)
		{
			Light component = GetComponent<Light>();
			commandBuffer_0 = new CommandBuffer();
			commandBuffer_0.name = "Grab shadowmap for Volumetric Fog";
			component.AddCommandBuffer(LightEvent.AfterShadowMap, commandBuffer_0);
			commandBuffer_1 = new CommandBuffer();
			commandBuffer_1.name = "Grab shadow params for Volumetric Fog";
			component.AddCommandBuffer(LightEvent.BeforeScreenspaceMask, commandBuffer_1);
			material_0 = new Material(m_BlurShadowmapShader);
			material_0.hideFlags = HideFlags.HideAndDontSave;
			material_1 = new Material(m_CopyShadowParamsShader);
			material_1.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	public void UpdateDirectionalShadowmap()
	{
		method_2();
		if (commandBuffer_0 != null)
		{
			commandBuffer_0.Clear();
		}
		if (commandBuffer_1 != null)
		{
			commandBuffer_1.Clear();
		}
		if (!Boolean_0)
		{
			return;
		}
		if (computeBuffer_0 == null)
		{
			computeBuffer_0 = new ComputeBuffer(1, 336);
		}
		Graphics.SetRandomWriteTarget(2, computeBuffer_0);
		commandBuffer_1.DrawProcedural(Matrix4x4.identity, material_1, 0, MeshTopology.Points, 1);
		int num = 4096;
		int num2 = Mathf.Min((int)m_ShadowmapRes, 2048);
		int num3 = (int)Mathf.Log(4096 / num2, 2f);
		RenderTargetIdentifier renderTargetIdentifier = BuiltinRenderTextureType.CurrentActive;
		commandBuffer_0.SetShadowSamplingMode(renderTargetIdentifier, ShadowSamplingMode.RawDepth);
		RenderTextureFormat format = RenderTextureFormat.RGHalf;
		method_4(ref renderTexture_0);
		renderTexture_0 = RenderTexture.GetTemporary(num2, num2, 0, RenderTextureFormat.RGHalf, RenderTextureReadWrite.Linear);
		renderTexture_0.filterMode = FilterMode.Bilinear;
		renderTexture_0.wrapMode = TextureWrapMode.Clamp;
		if (int_0 == null || int_0.Length != num3 - 1)
		{
			int_0 = new int[num3 - 1];
		}
		int i = 0;
		int num4 = num / 2;
		for (; i < num3; i++)
		{
			commandBuffer_0.SetGlobalVector("_TexelSize", new Vector4(0.5f / (float)num4, 0.5f / (float)num4, 0f, 0f));
			RenderTargetIdentifier dest;
			if (i < num3 - 1)
			{
				int_0[i] = Shader.PropertyToID("ShadowmapDownscaleTemp" + i);
				commandBuffer_0.GetTemporaryRT(int_0[i], num4, num4, 0, FilterMode.Bilinear, format, RenderTextureReadWrite.Linear);
				dest = new RenderTargetIdentifier(int_0[i]);
			}
			else
			{
				dest = new RenderTargetIdentifier(renderTexture_0);
			}
			if (i == 0)
			{
				commandBuffer_0.SetGlobalTexture("_DirShadowmap", renderTargetIdentifier);
				commandBuffer_0.Blit(null, dest, material_0, 4);
			}
			else
			{
				commandBuffer_0.Blit(int_0[i - 1], dest, material_0, 5);
			}
			num4 /= 2;
		}
	}

	public void method_3()
	{
		if (commandBuffer_0 != null)
		{
			commandBuffer_0.Clear();
		}
		if (commandBuffer_1 != null)
		{
			commandBuffer_1.Clear();
		}
		if (computeBuffer_0 != null)
		{
			computeBuffer_0.Release();
		}
		computeBuffer_0 = null;
	}

	public bool SetUpDirectionalShadowmapForSampling(bool shadows, ComputeShader cs, int kernel)
	{
		if (shadows && computeBuffer_0 != null && !(renderTexture_0 == null))
		{
			cs.SetFloat("_DirLightShadows", 1f);
			cs.SetBuffer(kernel, "_ShadowParams", computeBuffer_0);
			cs.SetTexture(kernel, "_DirectionalShadowmap", renderTexture_0);
			return true;
		}
		cs.SetFloat("_DirLightShadows", 0f);
		return false;
	}

	public void method_4(ref RenderTexture rt)
	{
		if (!(rt == null))
		{
			RenderTexture.ReleaseTemporary(rt);
			rt = null;
		}
	}
}
