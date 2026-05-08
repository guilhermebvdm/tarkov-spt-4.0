using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class WetRenderer : OnRenderObjectConnector
{
	public Shader WetShader;

	[Range(0f, 1f)]
	public float Intensity;

	public Color DiffuseColor;

	public Color SpecColor;

	public float WetBias = 0.0016f;

	[CompilerGenerated]
	private static bool bool_0;

	private static readonly int int_0 = SnowWetRenderer._wetOffMask;

	private static readonly int int_1 = Shader.PropertyToID("_SurfaceWetIntensity");

	private static readonly int int_2 = Shader.PropertyToID("_SurfaceWetDiffuseColor");

	private static readonly int int_3 = Shader.PropertyToID("_SurfaceWetSpecColor");

	private static readonly int int_4 = Shader.PropertyToID("_SurfaceWetBias");

	private Material material_0;

	private Mesh mesh_0;

	private readonly RenderTargetIdentifier[] renderTargetIdentifier_0 = new RenderTargetIdentifier[2]
	{
		BuiltinRenderTextureType.GBuffer0,
		BuiltinRenderTextureType.GBuffer1
	};

	private readonly GClass1001 gclass1001_0 = new GClass1001("Wetting", CameraEvent.BeforeReflections);

	public static bool WetOffMaskUsed
	{
		[CompilerGenerated]
		get
		{
			return bool_0;
		}
		[CompilerGenerated]
		set
		{
			bool_0 = value;
		}
	}

	public void OnValidate()
	{
		method_1();
	}

	public override void Start()
	{
		method_0();
		method_1();
		base.Start();
	}

	public void method_0()
	{
		mesh_0 = smethod_0();
		gclass1001_0.DestroyBuffers();
	}

	public void method_1()
	{
		Shader.SetGlobalFloat(int_1, Intensity);
		Shader.SetGlobalColor(int_2, DiffuseColor);
		Shader.SetGlobalColor(int_3, SpecColor);
		Shader.SetGlobalFloat(int_4, WetBias);
	}

	public void Update()
	{
		Shader.SetGlobalFloat(int_1, Intensity);
	}

	public override void ManualOnRenderObject(Camera currentCamera)
	{
		base.ManualOnRenderObject(currentCamera);
		if ((!(currentCamera != CameraClass.Instance.Camera) || !(currentCamera != CameraClass.Instance.OpticCameraManager.Camera)) && gclass1001_0.OnRenderObject(out var buffer))
		{
			buffer.SetRenderTarget(renderTargetIdentifier_0, BuiltinRenderTextureType.CameraTarget);
			if (WetOffMaskUsed)
			{
				buffer.EnableShaderKeyword("WET_OFF_MASK");
				buffer.SetGlobalTexture(int_0, int_0);
			}
			if (mesh_0 == null)
			{
				mesh_0 = smethod_0();
			}
			buffer.DrawMesh(mesh_0, Matrix4x4.identity, method_2());
			if (WetOffMaskUsed)
			{
				buffer.ReleaseTemporaryRT(int_0);
			}
		}
	}

	public void SetDebugRain(bool debug)
	{
		Color blue = Color.blue;
		Shader.SetGlobalColor(int_2, blue);
		if (!debug)
		{
			method_1();
		}
	}

	public Material method_2()
	{
		if (material_0 != null)
		{
			return material_0;
		}
		material_0 = new Material(WetShader);
		return material_0;
	}

	public static Mesh smethod_0()
	{
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(-1f, -1f, -0.1f),
			new Vector3(-1f, 1f, -0.1f),
			new Vector3(1f, 1f, -0.1f),
			new Vector3(1f, -1f, -0.1f)
		};
		int[] triangles = new int[6] { 0, 2, 1, 0, 3, 2 };
		Vector2[] uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(3f, 0f),
			new Vector2(2f, 0f),
			new Vector2(1f, 0f)
		};
		Mesh obj = new Mesh
		{
			name = "WetRenderer CreateQuad",
			vertices = vertices,
			uv = uv,
			triangles = triangles
		};
		Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 2.1474836E+09f);
		obj.bounds = bounds;
		return obj;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		Object.DestroyImmediate(mesh_0);
	}
}
