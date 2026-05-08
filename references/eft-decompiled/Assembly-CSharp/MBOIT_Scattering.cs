using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Time of Day/Camera MBOIT Scattering")]
public class MBOIT_Scattering : TOD_ImageEffect
{
	public struct Struct0
	{
		public float ColorMultiplierRed;

		public float ColorMultiplierGreen;

		public float ColorMultiplierBlue;

		public float ScatterDensityMultiplier;

		public float ScatterDensityPower;

		public float ScatterDensityBias;

		public float ScatterGreyscale;

		public float HeightFalloffFactor;
	}

	public const int SLICES_COUNT = 10;

	public bool Lighten;

	public bool FromLevelSettings = true;

	public ComputeShader ScatteringSlicesComputeShader;

	public ComputeShader ScatteringMBOITComputeShader;

	public ComputeShader ScatteringMBOITRCPComputeShader;

	public ComputeShader ScatteringMBOITFinalPassComputeShader;

	public Color ScatterColorMultiplier = new Color(1f, 1f, 1f, 0.4705f);

	public float ScatterGreyscale = 1f;

	public float ScatterDensityMultiplier = 3.49f;

	public float ScatterDensityPower = 2.37f;

	public float ScatterDensityBias = 0.59f;

	public float HeightFalloffFactor = 1f;

	public float SOFT_SCALE = 508.62f;

	public float SOFT_CONTRAST_POWER = 1f;

	public Texture2D DitheringTexture;

	[Range(0f, 0.2f)]
	public float GlobalDensity = 0.001f;

	[Range(0f, 1f)]
	public float HeightFalloff = 0.001f;

	[Range(0.95f, 1f)]
	public float SunrizeGlow = 0.95f;

	[NonSerialized]
	public bool ThermalVisionIsEnabled;

	public float MBOITDitherScale = 1f;

	private float float_0 = 0.02f;

	private float float_1 = 1.27f;

	private bool bool_2 = true;

	private bool bool_3 = true;

	public float ZeroLevel;

	private static readonly int int_1 = Shader.PropertyToID("_FrustumCornersWS");

	private static readonly int int_2 = Shader.PropertyToID("_DitheringTexture");

	private static readonly int int_3 = Shader.PropertyToID("_Density");

	private static readonly int int_4 = Shader.PropertyToID("_SunrizeGlow");

	private static int int_5 = Shader.PropertyToID("CameraParameters");

	private ComputeBuffer computeBuffer_0;

	private RenderTexture renderTexture_0;

	private RenderTexture renderTexture_1;

	private RenderTexture renderTexture_2;

	private Mesh mesh_0;

	private CommandBuffer commandBuffer_0;

	private SSAA ssaa_0;

	private Camera camera_0;

	private bool bool_4;

	private ComputeBuffer computeBuffer_1;

	private Struct0[] struct0_0 = new Struct0[1];

	public float SlicesFarDistance => float_0;

	public float SlicesDistributionExponent
	{
		get
		{
			return float_1;
		}
		set
		{
			if (value != float_1)
			{
				float_1 = value;
				bool_3 = true;
			}
		}
	}

	public bool IsDataUpdated => bool_2;

	public void Start()
	{
		if (!bool_4)
		{
			method_0();
		}
	}

	public void method_0()
	{
		mesh_0 = new Mesh();
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(-1f, -1f, 0f),
			new Vector3(1f, -1f, 0f),
			new Vector3(1f, 1f, 0f),
			new Vector3(-1f, 1f, 0f)
		};
		mesh_0.vertices = vertices;
		Vector2[] uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f),
			new Vector2(0f, 1f)
		};
		mesh_0.uv = uv;
		int[] triangles = new int[6] { 0, 1, 2, 2, 3, 0 };
		mesh_0.triangles = triangles;
		camera_0 = GetComponent<Camera>();
		commandBuffer_0 = new CommandBuffer();
		commandBuffer_0.name = "TOD_ScatteringCmd";
		camera_0.AddCommandBuffer(CameraEvent.AfterLighting, commandBuffer_0);
		ssaa_0 = GetComponent<SSAA>();
		bool_4 = true;
	}

	public void OnDestroy()
	{
		bool_4 = false;
		if (computeBuffer_0 != null)
		{
			computeBuffer_0.Release();
			computeBuffer_0 = null;
		}
		if (renderTexture_0 != null)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(renderTexture_0);
				UnityEngine.Object.Destroy(renderTexture_1);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(renderTexture_0);
				UnityEngine.Object.DestroyImmediate(renderTexture_1);
			}
			renderTexture_0 = null;
			renderTexture_1 = null;
		}
	}

	public void OnPreCull()
	{
		float num = ScatterColorMultiplier.r / ScatterColorMultiplier.a;
		float num2 = ScatterColorMultiplier.g / ScatterColorMultiplier.a;
		float num3 = ScatterColorMultiplier.b / ScatterColorMultiplier.a;
		Shader.SetGlobalColor("ScatterColorMultiplier", new Color(num, num2, num3, 1f));
		Shader.SetGlobalFloat("ScatterDensityMultiplier", ScatterDensityMultiplier);
		Shader.SetGlobalFloat("ScatterDensityPower", ScatterDensityPower);
		Shader.SetGlobalFloat("ScatterDensityBias", ScatterDensityBias);
		Shader.SetGlobalFloat("ScatterGreyscale", ScatterGreyscale);
		Shader.SetGlobalFloat("SOFT_SCALE", SOFT_SCALE);
		Shader.SetGlobalFloat("SOFT_CONTRAST_POWER", SOFT_CONTRAST_POWER);
		struct0_0[0].ColorMultiplierRed = num;
		struct0_0[0].ColorMultiplierGreen = num2;
		struct0_0[0].ColorMultiplierBlue = num3;
		struct0_0[0].ScatterDensityMultiplier = ScatterDensityMultiplier;
		struct0_0[0].ScatterDensityPower = ScatterDensityPower;
		struct0_0[0].ScatterDensityBias = ScatterDensityBias;
		struct0_0[0].ScatterGreyscale = ScatterGreyscale;
		struct0_0[0].HeightFalloffFactor = HeightFalloffFactor;
		if (computeBuffer_1 == null)
		{
			computeBuffer_1 = new ComputeBuffer(1, Marshal.SizeOf<Struct0>());
		}
		computeBuffer_1.SetData(struct0_0, 0, 0, 1);
		if (bool_3)
		{
			method_2();
		}
		OnRenderImageMBOITMode(computeBuffer_1);
	}

	public void method_1()
	{
		if (renderTexture_0 != null)
		{
			renderTexture_0.Release();
			renderTexture_0 = null;
		}
		if (renderTexture_1 != null)
		{
			renderTexture_1.Release();
			renderTexture_1 = null;
		}
		bool_2 = true;
	}

	public void method_2()
	{
		if (computeBuffer_0 == null)
		{
			computeBuffer_0 = new ComputeBuffer(10, 4);
			bool_2 = true;
		}
		float[] array = new float[10];
		for (int i = 0; i < 10; i++)
		{
			float f = (float)i / 9f;
			f = Mathf.Pow(f, SlicesDistributionExponent) * float_0;
			array[i] = f;
		}
		computeBuffer_0.SetData(array);
		bool_3 = false;
	}

	public void method_3(int width, int height)
	{
		if (renderTexture_0 != null && (renderTexture_0.width != width || renderTexture_0.height != height))
		{
			method_1();
		}
		if (computeBuffer_0 == null)
		{
			method_2();
		}
		if (renderTexture_0 == null)
		{
			renderTexture_0 = new RenderTexture(width, height, 0, GraphicsFormat.B10G11R11_UFloatPack32);
			renderTexture_0.name = "ScatterSlicesColor";
			renderTexture_0.dimension = TextureDimension.Tex2DArray;
			renderTexture_0.volumeDepth = 10;
			renderTexture_0.enableRandomWrite = true;
			renderTexture_0.Create();
			renderTexture_1 = new RenderTexture(width, height, 0, GraphicsFormat.R8_UNorm);
			renderTexture_1.name = "ScatterSlicesAlpha";
			renderTexture_1.dimension = TextureDimension.Tex2DArray;
			renderTexture_1.volumeDepth = 10;
			renderTexture_1.enableRandomWrite = true;
			renderTexture_1.Create();
		}
	}

	public void OnRenderImageMBOITMode(ComputeBuffer scatteringParameters)
	{
		int num = ((ssaa_0 != null) ? ssaa_0.GetInputWidth() : camera_0.pixelWidth);
		int num2 = ((ssaa_0 != null) ? ssaa_0.GetInputHeight() : camera_0.pixelHeight);
		method_3(num, num2);
		base.Sky.Components.MBOITScattering = this;
		commandBuffer_0.Clear();
		if (Lighten)
		{
			commandBuffer_0.EnableShaderKeyword("LIGHTEN");
		}
		else
		{
			commandBuffer_0.DisableShaderKeyword("LIGHTEN");
		}
		_ = (cam.farClipPlane - cam.nearClipPlane) / 9f;
		Vector3 position = cam.transform.position;
		commandBuffer_0.SetGlobalVector(int_3, new Vector4(HeightFalloff, position.y - ZeroLevel, GlobalDensity, 0f));
		commandBuffer_0.SetComputeTextureParam(ScatteringSlicesComputeShader, 0, "Result", renderTexture_0);
		commandBuffer_0.SetComputeTextureParam(ScatteringSlicesComputeShader, 0, "ResultAlpha", renderTexture_1);
		commandBuffer_0.SetComputeBufferParam(ScatteringSlicesComputeShader, 0, "_Slices", computeBuffer_0);
		commandBuffer_0.SetComputeBufferParam(ScatteringSlicesComputeShader, 0, "ScatteringParameters", scatteringParameters);
		float nearClipPlane = cam.nearClipPlane;
		float farClipPlane = cam.farClipPlane;
		float fieldOfView = cam.fieldOfView;
		float aspect = cam.aspect;
		Vector3 forward = cam.transform.forward;
		Vector3 right = cam.transform.right;
		Vector3 up = cam.transform.up;
		Matrix4x4 identity = Matrix4x4.identity;
		float num3 = fieldOfView * 0.5f;
		Vector3 vector = right * nearClipPlane * Mathf.Tan(num3 * (MathF.PI / 180f)) * aspect;
		Vector3 vector2 = up * nearClipPlane * Mathf.Tan(num3 * (MathF.PI / 180f));
		Vector3 vector3 = forward * nearClipPlane - vector + vector2;
		float num4 = vector3.magnitude * farClipPlane / nearClipPlane;
		vector3.Normalize();
		vector3 *= num4;
		Vector3 vector4 = forward * nearClipPlane + vector + vector2;
		vector4.Normalize();
		vector4 *= num4;
		Vector3 vector5 = forward * nearClipPlane + vector - vector2;
		vector5.Normalize();
		vector5 *= num4;
		Vector3 vector6 = forward * nearClipPlane - vector - vector2;
		vector6.Normalize();
		vector6 *= num4;
		identity.SetRow(0, vector3);
		identity.SetRow(1, vector4);
		identity.SetRow(2, vector5);
		identity.SetRow(3, vector6);
		commandBuffer_0.SetComputeMatrixParam(ScatteringSlicesComputeShader, int_1, identity);
		WindowsManager instance = WindowsManager.Instance;
		if (instance != null && instance.IsActive && instance.TryGetCameraParameters(camera_0, out var parameters))
		{
			commandBuffer_0.SetComputeBufferParam(ScatteringSlicesComputeShader, 0, int_5, parameters);
		}
		commandBuffer_0.SetComputeFloatParam(ScatteringSlicesComputeShader, int_4, SunrizeGlow);
		commandBuffer_0.SetComputeVectorParam(ScatteringSlicesComputeShader, "_ResolutionOut", new Vector4(renderTexture_0.width, renderTexture_0.height, 1f / (float)renderTexture_0.width, 1f / (float)renderTexture_0.height));
		commandBuffer_0.SetComputeVectorParam(ScatteringSlicesComputeShader, "_ResolutionIn", new Vector4(num, num2, 1f / (float)num, 1f / (float)num2));
		commandBuffer_0.SetComputeTextureParam(ScatteringSlicesComputeShader, 0, "_DepthTexture", renderTexture_2);
		commandBuffer_0.SetComputeFloatParam(ScatteringSlicesComputeShader, "_DitherScale", MBOITDitherScale);
		commandBuffer_0.SetComputeTextureParam(ScatteringSlicesComputeShader, 0, int_2, DitheringTexture);
		commandBuffer_0.SetComputeIntParam(ScatteringSlicesComputeShader, "SlicesCount", 10);
		int threadGroupsX = renderTexture_0.width / 8 + (((renderTexture_0.width & 7) > 0) ? 1 : 0);
		int threadGroupsY = renderTexture_0.height / 8 + (((renderTexture_0.height & 7) > 0) ? 1 : 0);
		commandBuffer_0.DispatchCompute(ScatteringSlicesComputeShader, 0, threadGroupsX, threadGroupsY, 1);
	}

	public void RenderMoments(CommandBuffer cmd, RenderTexture depthBuffer, RenderTexture moments0UAV, RenderTexture moments1UAV, RenderTexture fogAlpha)
	{
		renderTexture_2 = depthBuffer;
		if (base.isActiveAndEnabled)
		{
			method_3(moments0UAV.width, moments0UAV.height);
			if (computeBuffer_0 != null)
			{
				string text = "RenderFogMoments";
				cmd.BeginSample(text);
				int threadGroupsX = moments0UAV.width / 8 + (((moments0UAV.width & 7) > 0) ? 1 : 0);
				int threadGroupsY = moments0UAV.height / 8 + (((moments0UAV.height & 7) > 0) ? 1 : 0);
				cmd.SetComputeBufferParam(ScatteringMBOITComputeShader, 0, "_Slices", computeBuffer_0);
				cmd.SetComputeIntParam(ScatteringMBOITComputeShader, "SlicesCount", 10);
				cmd.SetComputeVectorParam(ScatteringMBOITComputeShader, "_ResolutionOut", new Vector4(renderTexture_0.width, renderTexture_0.height, 1f / (float)renderTexture_0.width, 1f / (float)renderTexture_0.height));
				cmd.SetComputeTextureParam(ScatteringMBOITComputeShader, 0, "Moments0", moments0UAV);
				cmd.SetComputeTextureParam(ScatteringMBOITComputeShader, 0, "Moments1", moments1UAV);
				cmd.SetComputeTextureParam(ScatteringMBOITComputeShader, 0, "TotalAlpha", fogAlpha);
				cmd.SetComputeTextureParam(ScatteringMBOITComputeShader, 0, "_slices2DArray", renderTexture_0);
				cmd.SetComputeTextureParam(ScatteringMBOITComputeShader, 0, "_slicesAlpha2DArray", renderTexture_1);
				cmd.DispatchCompute(ScatteringMBOITComputeShader, 0, threadGroupsX, threadGroupsY, 1);
				cmd.EndSample(text);
			}
		}
	}

	public void RenderNormalizationFactor(CommandBuffer cmd, RenderTexture moments0UAV, RenderTexture moments1UAV, RenderTexture momentsRcp)
	{
		if (base.isActiveAndEnabled && computeBuffer_0 != null)
		{
			string text = "RenderFogNormalizationFactor";
			cmd.BeginSample(text);
			cmd.SetComputeBufferParam(ScatteringMBOITRCPComputeShader, 0, "_Slices", computeBuffer_0);
			cmd.SetComputeIntParam(ScatteringMBOITRCPComputeShader, "SlicesCount", 10);
			cmd.SetComputeVectorParam(ScatteringMBOITRCPComputeShader, "_ResolutionOut", new Vector4(renderTexture_0.width, renderTexture_0.height, 1f / (float)renderTexture_0.width, 1f / (float)renderTexture_0.height));
			cmd.SetComputeTextureParam(ScatteringMBOITRCPComputeShader, 0, "Moments0", moments0UAV);
			cmd.SetComputeTextureParam(ScatteringMBOITRCPComputeShader, 0, "Moments1", moments1UAV);
			cmd.SetComputeTextureParam(ScatteringMBOITRCPComputeShader, 0, "MomentsRcp", momentsRcp);
			cmd.SetComputeTextureParam(ScatteringMBOITRCPComputeShader, 0, "_slices2DArray", renderTexture_0);
			cmd.SetComputeTextureParam(ScatteringMBOITRCPComputeShader, 0, "_slicesAlpha2DArray", renderTexture_1);
			int threadGroupsX = moments0UAV.width / 8 + (((moments0UAV.width & 7) > 0) ? 1 : 0);
			int threadGroupsY = moments0UAV.height / 8 + (((moments0UAV.height & 7) > 0) ? 1 : 0);
			cmd.DispatchCompute(ScatteringMBOITRCPComputeShader, 0, threadGroupsX, threadGroupsY, 1);
			cmd.EndSample(text);
		}
	}

	public void RenderWithMBOIT(CommandBuffer cmd, RenderTexture moments0UAV, RenderTexture moments1UAV, RenderTexture momentsRcp, RenderTexture resultTexture)
	{
		if (base.isActiveAndEnabled)
		{
			if (computeBuffer_0 != null)
			{
				string text = "RenderFogMBOIT";
				cmd.BeginSample(text);
				cmd.SetComputeBufferParam(ScatteringMBOITFinalPassComputeShader, 0, "_Slices", computeBuffer_0);
				cmd.SetComputeIntParam(ScatteringMBOITFinalPassComputeShader, "SlicesCount", 10);
				cmd.SetComputeVectorParam(ScatteringMBOITFinalPassComputeShader, "_ResolutionOut", new Vector4(renderTexture_0.width, renderTexture_0.height, 1f / (float)renderTexture_0.width, 1f / (float)renderTexture_0.height));
				cmd.SetComputeTextureParam(ScatteringMBOITFinalPassComputeShader, 0, "_slices2DArray", renderTexture_0);
				cmd.SetComputeTextureParam(ScatteringMBOITFinalPassComputeShader, 0, "_slicesAlpha2DArray", renderTexture_1);
				cmd.SetComputeTextureParam(ScatteringMBOITFinalPassComputeShader, 0, "Result", resultTexture);
				cmd.SetComputeTextureParam(ScatteringMBOITFinalPassComputeShader, 0, "transmittance_sum_rcp", momentsRcp);
				int threadGroupsX = moments0UAV.width / 8 + (((moments0UAV.width & 7) > 0) ? 1 : 0);
				int threadGroupsY = moments0UAV.height / 8 + (((moments0UAV.height & 7) > 0) ? 1 : 0);
				cmd.DispatchCompute(ScatteringMBOITFinalPassComputeShader, 0, threadGroupsX, threadGroupsY, 1);
				cmd.EndSample(text);
			}
			bool_2 = false;
		}
	}
}
