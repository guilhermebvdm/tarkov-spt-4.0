using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT.Weather;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[GAttribute8(20001)]
public class WaterForSSR : MonoBehaviour
{
	public class GClass983
	{
		public Mesh Mesh;

		public Matrix4x4 Matrix;

		public GClass983(Mesh mesh, Matrix4x4 matrix)
		{
			Mesh = mesh;
			Matrix = matrix;
		}
	}

	[Serializable]
	public class TextureBlendSetting
	{
		public float Scale = 0.1f;

		public Vector2 MovementDirection = Vector2.one;

		[Range(0f, 1f)]
		public float Blend = 0.5f;

		public Vector4 GetVals()
		{
			return new Vector4(MovementDirection.x, MovementDirection.y, Scale, Blend);
		}
	}

	[CompilerGenerated]
	public class Class666
	{
		public WaterForSSR waterForSSR_0;

		public int i;

		public Predicate<GClass983> predicate_0;

		public bool method_0(GClass983 x)
		{
			return x.Mesh == waterForSSR_0.WaterPlanes[i].sharedMesh;
		}
	}

	[CompilerGenerated]
	private static Action<WaterForSSR> action_0;

	[CompilerGenerated]
	private static Action<WaterForSSR> action_1;

	public Shader WaterShader;

	public Texture2D RippleTexture;

	[Space]
	[Header("Textures")]
	public Texture Normals;

	public Texture NormalsDetails;

	public float NormalsDetailsMipMapBias;

	public Texture Foam;

	[Space(32f)]
	[Header("Layers Settings")]
	public TextureBlendSetting NormalsA;

	public TextureBlendSetting NormalsB;

	public TextureBlendSetting NormalsDetails0;

	[Space]
	public TextureBlendSetting FoamA;

	public TextureBlendSetting FoamB;

	[Space(32f)]
	[Header("Depth Settings")]
	public float BorderFade;

	public float BorderFadeDistStart;

	public float BorderFadeDistRange;

	public float DepthFade;

	public float DepthColorFade;

	public float DepthRefractions;

	[ColorUsage(false)]
	public Color DepthColorDeep;

	[ColorUsage(false)]
	public Color DepthColorShallow;

	[Space(32f)]
	[Header("Other")]
	public float Bumpiness;

	[Space]
	public float RippleScale;

	public float RippleBumpness;

	[Space]
	public float FoamSize;

	public float FoamIntensity;

	[ColorUsage(false)]
	public Color FoamColor;

	[Space]
	[Range(0f, 1f)]
	public float FresnelIntensity = 0.9f;

	public float FresnelPower = 5f;

	[Space]
	public float AdditionalCubemapReflectionMinWetting = 0.05f;

	public float AdditionalCubemapReflectionMaxWetting = 0.35f;

	public AnimationCurve ReflectionWettingFunc = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public Color ReflectionColor;

	public Color DiffuseColor;

	public MeshFilter[] WaterPlanes = new MeshFilter[0];

	private List<GClass983> list_0;

	private Material material_0;

	private float float_0;

	private float float_1 = -1f;

	private RenderTargetIdentifier[] renderTargetIdentifier_0;

	private int int_0;

	private int int_1;

	private int int_2;

	private int int_3;

	private int int_4;

	private int int_5;

	private int int_6;

	private int int_7;

	private int int_8;

	private int int_9;

	private int int_10;

	private int int_11;

	private int int_12;

	private int int_13;

	private int int_14;

	private int int_15;

	private int int_16;

	private int int_17;

	private int int_18;

	private int int_19;

	private int int_20;

	private int int_21;

	private int int_22;

	public static event Action<WaterForSSR> OnAdd
	{
		[CompilerGenerated]
		add
		{
			Action<WaterForSSR> action = action_0;
			Action<WaterForSSR> action2;
			do
			{
				action2 = action;
				Action<WaterForSSR> value2 = (Action<WaterForSSR>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<WaterForSSR> action = action_0;
			Action<WaterForSSR> action2;
			do
			{
				action2 = action;
				Action<WaterForSSR> value2 = (Action<WaterForSSR>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public static event Action<WaterForSSR> OnRemove
	{
		[CompilerGenerated]
		add
		{
			Action<WaterForSSR> action = action_1;
			Action<WaterForSSR> action2;
			do
			{
				action2 = action;
				Action<WaterForSSR> value2 = (Action<WaterForSSR>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<WaterForSSR> action = action_1;
			Action<WaterForSSR> action2;
			do
			{
				action2 = action;
				Action<WaterForSSR> value2 = (Action<WaterForSSR>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void Start()
	{
		OnValidate();
		if (action_0 != null)
		{
			action_0(this);
		}
	}

	public void OnEnable()
	{
		OnValidate();
		if (action_0 != null)
		{
			action_0(this);
		}
	}

	public void Update()
	{
		if (!(material_0 == null))
		{
			float t = 0f;
			if (WeatherController.Instance != null)
			{
				t = ReflectionWettingFunc.Evaluate(RainController.Intensity);
			}
			material_0.SetFloat(int_18, Mathf.Lerp(AdditionalCubemapReflectionMinWetting, AdditionalCubemapReflectionMaxWetting, t));
		}
	}

	public void OnValidate()
	{
		if (float.IsNaN(NormalsA.Blend) || float.IsNaN(NormalsB.Blend) || float.IsNaN(NormalsDetails0.Blend))
		{
			TextureBlendSetting normalsA = NormalsA;
			TextureBlendSetting normalsB = NormalsB;
			TextureBlendSetting normalsDetails = NormalsDetails0;
			float num = 0.5f;
			normalsDetails.Blend = 0.5f;
			float blend = num;
			num = 0.5f;
			normalsB.Blend = blend;
			normalsA.Blend = num;
		}
		float num2 = NormalsA.Blend + NormalsB.Blend + NormalsDetails0.Blend;
		num2 = 1f / num2;
		NormalsA.Blend *= num2;
		NormalsB.Blend *= num2;
		NormalsDetails0.Blend *= num2;
		if (float.IsNaN(FoamA.Blend) || float.IsNaN(FoamB.Blend))
		{
			TextureBlendSetting foamA = FoamA;
			TextureBlendSetting foamB = FoamB;
			float num = 0.5f;
			foamB.Blend = 0.5f;
			foamA.Blend = num;
		}
		num2 = FoamA.Blend + FoamB.Blend;
		num2 = 1f / num2;
		FoamA.Blend *= num2;
		FoamB.Blend *= num2;
		method_0();
		method_2();
		method_3();
	}

	public void method_0()
	{
		int_0 = Shader.PropertyToID("_WaterForSSR_SavedG0");
		int_1 = Shader.PropertyToID("_BumpMap");
		int_2 = Shader.PropertyToID("_BumpMapDetails");
		int_3 = Shader.PropertyToID("_FoamTex");
		int_4 = Shader.PropertyToID("_DepthVals");
		int_5 = Shader.PropertyToID("_BorderFadeDistVals");
		int_6 = Shader.PropertyToID("_Bumpiness");
		int_7 = Shader.PropertyToID("_DepthColorDeep");
		int_8 = Shader.PropertyToID("_DepthColorShallow");
		int_9 = Shader.PropertyToID("_FoamVals");
		int_10 = Shader.PropertyToID("_FoamColor");
		int_11 = Shader.PropertyToID("_ReflectionColor");
		int_12 = Shader.PropertyToID("_DiffuseColor");
		int_13 = Shader.PropertyToID("_ValsNormA");
		int_14 = Shader.PropertyToID("_ValsNormB");
		int_15 = Shader.PropertyToID("_ValsNormD");
		int_16 = Shader.PropertyToID("_ValsFoamA");
		int_17 = Shader.PropertyToID("_ValsFoamB");
		int_18 = Shader.PropertyToID("_AdditionalCubemapReflection");
		int_19 = Shader.PropertyToID("_RippleScale");
		int_20 = Shader.PropertyToID("_RippleBumpness");
		int_21 = Shader.PropertyToID("_FresnelVals");
		int_22 = Shader.PropertyToID("_RippleTex");
	}

	public void OnDisable()
	{
		if (action_1 != null)
		{
			action_1(this);
		}
	}

	public void OnDestroy()
	{
		if (action_1 != null)
		{
			action_1(this);
		}
	}

	public void InitBuffer(CommandBuffer buffer, Camera currentCamera)
	{
		method_3();
		method_1(currentCamera);
		RenderBuffer(buffer, currentCamera);
	}

	public void method_1(Camera currentCamera)
	{
		if (renderTargetIdentifier_0 == null)
		{
			renderTargetIdentifier_0 = new RenderTargetIdentifier[4]
			{
				BuiltinRenderTextureType.GBuffer0,
				BuiltinRenderTextureType.GBuffer1,
				BuiltinRenderTextureType.GBuffer2,
				currentCamera.allowHDR ? BuiltinRenderTextureType.CameraTarget : BuiltinRenderTextureType.GBuffer3
			};
		}
		renderTargetIdentifier_0[3] = (currentCamera.allowHDR ? BuiltinRenderTextureType.CameraTarget : BuiltinRenderTextureType.GBuffer3);
	}

	public void RenderBuffer(CommandBuffer buffer, Camera currentCamera)
	{
		if (material_0 == null)
		{
			method_3();
		}
		method_4();
		if (renderTargetIdentifier_0 == null)
		{
			method_1(currentCamera);
		}
		buffer.GetTemporaryRT(int_0, -1, -1, 16, FilterMode.Point, RenderTextureFormat.ARGB32);
		buffer.Blit(BuiltinRenderTextureType.GBuffer0, int_0);
		buffer.SetGlobalTexture(int_0, int_0);
		buffer.SetRenderTarget(renderTargetIdentifier_0, BuiltinRenderTextureType.CameraTarget);
		for (int i = 0; i < list_0.Count; i++)
		{
			buffer.DrawMesh(list_0[i].Mesh, list_0[i].Matrix, material_0, 0, 0);
		}
		buffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.ResolvedDepth);
		for (int j = 0; j < list_0.Count; j++)
		{
			buffer.DrawMesh(list_0[j].Mesh, list_0[j].Matrix, material_0, 0, 1);
		}
		buffer.ReleaseTemporaryRT(int_0);
	}

	public bool IsCorrectLayer(int cullingMask)
	{
		if (WaterPlanes.Length == 0)
		{
			return false;
		}
		MeshFilter meshFilter = WaterPlanes[0];
		if (meshFilter == null)
		{
			return false;
		}
		int layer = meshFilter.gameObject.layer;
		if (cullingMask == (cullingMask | (1 << layer)))
		{
			return true;
		}
		return false;
	}

	public void method_2()
	{
		if (WaterPlanes.Length == 0)
		{
			return;
		}
		if (list_0 == null)
		{
			list_0 = new List<GClass983>();
			for (int i = 0; i < WaterPlanes.Length; i++)
			{
				if (WaterPlanes[i] != null)
				{
					GClass983 item = new GClass983(WaterPlanes[i].sharedMesh, WaterPlanes[i].gameObject.transform.localToWorldMatrix);
					list_0.Add(item);
				}
			}
			return;
		}
		int j;
		for (j = 0; j < WaterPlanes.Length; j++)
		{
			if (WaterPlanes[j] != null)
			{
				list_0.Find((GClass983 x) => x.Mesh == WaterPlanes[j].sharedMesh).Matrix = WaterPlanes[j].gameObject.transform.localToWorldMatrix;
			}
		}
	}

	public void method_3()
	{
		if (material_0 == null)
		{
			material_0 = new Material(WaterShader);
		}
		NormalsDetails.mipMapBias = NormalsDetailsMipMapBias;
		material_0.SetTexture(int_1, Normals);
		material_0.SetTexture(int_2, NormalsDetails);
		material_0.SetTexture(int_3, Foam);
		material_0.SetVector(int_4, new Vector4(DepthFade, DepthRefractions, DepthColorFade, BorderFade));
		material_0.SetVector(int_5, new Vector4(BorderFadeDistStart, 1f / BorderFadeDistRange));
		material_0.SetFloat(int_6, Bumpiness);
		material_0.SetColor(int_7, DepthColorDeep);
		material_0.SetColor(int_8, DepthColorShallow);
		material_0.SetVector(int_9, new Vector4(FoamSize, 1f / FoamSize, FoamIntensity));
		material_0.SetColor(int_10, FoamColor);
		material_0.SetColor(int_11, ReflectionColor);
		material_0.SetColor(int_12, DiffuseColor);
		material_0.SetVector(int_13, NormalsA.GetVals());
		material_0.SetVector(int_14, NormalsB.GetVals());
		material_0.SetVector(int_15, NormalsDetails0.GetVals());
		material_0.SetVector(int_16, FoamA.GetVals());
		material_0.SetVector(int_17, FoamB.GetVals());
		method_4();
		material_0.SetFloat(int_19, RippleScale);
		material_0.SetFloat(int_20, RippleBumpness);
		material_0.SetVector(int_21, new Vector4(FresnelIntensity, FresnelPower));
		material_0.SetTexture(int_22, RippleTexture);
	}

	public void method_4()
	{
		if (WeatherController.Instance != null)
		{
			float_0 = ReflectionWettingFunc.Evaluate(RainController.Intensity);
		}
		if (!(Math.Abs(float_0 - float_1) < 0.01f))
		{
			float_1 = float_0;
			material_0.SetFloat(int_18, Mathf.Lerp(AdditionalCubemapReflectionMinWetting, AdditionalCubemapReflectionMaxWetting, float_0));
		}
	}
}
