using EFT.BlitDebug;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Time of Day/Camera God Rays")]
public class TOD_Rays : TOD_ImageEffect
{
	public enum ResolutionType
	{
		Low,
		Normal,
		High
	}

	public enum BlendModeType
	{
		Screen,
		Add
	}

	public Shader GodRayShader;

	public Shader ScreenClearShader;

	public ResolutionType Resolution = ResolutionType.Normal;

	public BlendModeType BlendMode;

	[GAttribute2(0f, 4f)]
	public int BlurIterations = 2;

	[GAttribute0(0f)]
	public float BlurRadius = 2f;

	[GAttribute0(0f)]
	public float Intensity = 1f;

	[GAttribute0(0f)]
	public float MaxRadius = 0.5f;

	public bool UseDepthTexture = true;

	private Material material_0;

	private Material material_1;

	private static readonly int int_1 = Shader.PropertyToID("_BlurRadius4");

	private static readonly int int_2 = Shader.PropertyToID("_LightPosition");

	private static readonly int int_3 = Shader.PropertyToID("_LightColor");

	private static readonly int int_4 = Shader.PropertyToID("_ColorBuffer");

	private const int int_5 = 2;

	private const int int_6 = 3;

	private const int int_7 = 1;

	private const int int_8 = 0;

	private const int int_9 = 4;

	public void OnEnable()
	{
		if (!GodRayShader)
		{
			GodRayShader = GClass872.Find("Hidden/Time of Day/God Rays");
		}
		if (!ScreenClearShader)
		{
			ScreenClearShader = GClass872.Find("Hidden/Time of Day/Screen Clear");
		}
		material_0 = CreateMaterial(GodRayShader);
		material_1 = CreateMaterial(ScreenClearShader);
	}

	public void OnDisable()
	{
		if ((bool)material_0)
		{
			Object.DestroyImmediate(material_0);
		}
		if ((bool)material_1)
		{
			Object.DestroyImmediate(material_1);
		}
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!CheckSupport(UseDepthTexture))
		{
			Graphics.Blit(source, destination);
			return;
		}
		base.Sky.Components.Rays = this;
		int width;
		int height;
		int depthBuffer;
		if (Resolution == ResolutionType.High)
		{
			width = source.width;
			height = source.height;
			depthBuffer = 0;
		}
		else if (Resolution == ResolutionType.Normal)
		{
			width = source.width / 2;
			height = source.height / 2;
			depthBuffer = 0;
		}
		else
		{
			width = source.width / 4;
			height = source.height / 4;
			depthBuffer = 0;
		}
		Vector3 vector = cam.WorldToViewportPoint(base.Sky.Components.LightTransform.position);
		material_0.SetVector(int_1, new Vector4(1f, 1f, 0f, 0f) * BlurRadius);
		material_0.SetVector(int_2, new Vector4(vector.x, vector.y, vector.z, MaxRadius));
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, depthBuffer);
		RenderTexture renderTexture = null;
		if (UseDepthTexture)
		{
			DebugGraphics.Blit(source, temporary, material_0, 2);
		}
		else
		{
			DebugGraphics.Blit(source, temporary, material_0, 3);
		}
		DrawBorder(temporary, material_1);
		float num = BlurRadius * 0.0013020834f;
		material_0.SetVector(int_1, new Vector4(num, num, 0f, 0f));
		material_0.SetVector(int_2, new Vector4(vector.x, vector.y, vector.z, MaxRadius));
		for (int i = 0; i < BlurIterations; i++)
		{
			renderTexture = RenderTexture.GetTemporary(width, height, depthBuffer);
			DebugGraphics.Blit(temporary, renderTexture, material_0, 1);
			RenderTexture.ReleaseTemporary(temporary);
			num = BlurRadius * (((float)i * 2f + 1f) * 6f) / 768f;
			material_0.SetVector(int_1, new Vector4(num, num, 0f, 0f));
			temporary = RenderTexture.GetTemporary(width, height, depthBuffer);
			DebugGraphics.Blit(renderTexture, temporary, material_0, 1);
			RenderTexture.ReleaseTemporary(renderTexture);
			num = BlurRadius * (((float)i * 2f + 2f) * 6f) / 768f;
			material_0.SetVector(int_1, new Vector4(num, num, 0f, 0f));
		}
		Vector4 value = (((double)vector.z >= 0.0) ? (Intensity * (Vector4)base.Sky.RayColor) : Vector4.zero);
		material_0.SetVector(int_3, value);
		material_0.SetTexture(int_4, temporary);
		if (BlendMode == BlendModeType.Screen)
		{
			DebugGraphics.Blit(source, destination, material_0, 0);
		}
		else
		{
			DebugGraphics.Blit(source, destination, material_0, 4);
		}
		RenderTexture.ReleaseTemporary(temporary);
	}
}
