using System;
using Comfort.Common;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Time of Day/Camera Scattering")]
public class TOD_Scattering : TOD_ImageEffect
{
	public bool Lighten;

	public bool FromLevelSettings = true;

	public Shader ScatteringShader;

	public Texture2D DitheringTexture;

	[Range(0.001f, 0.2f)]
	public float GlobalDensity = 0.001f;

	[Range(0f, 1f)]
	public float HeightFalloff = 0.001f;

	[Range(0.95f, 1f)]
	public float SunrizeGlow = 0.95f;

	[SerializeField]
	private bool _mboit;

	public float ZeroLevel;

	private Material material_0;

	private static readonly int int_1 = Shader.PropertyToID("_FrustumCornersWS");

	private static readonly int int_2 = Shader.PropertyToID("_DitheringTexture");

	private static readonly int int_3 = Shader.PropertyToID("_Density");

	private static readonly int int_4 = Shader.PropertyToID("_SunrizeGlow");

	private static readonly int int_5 = Shader.PropertyToID("_ScatteringTex");

	private Mesh mesh_0;

	private MBOIT_Scattering mboit_Scattering_0;

	private bool bool_2;

	public bool MBOIT
	{
		get
		{
			return _mboit;
		}
		set
		{
			method_2(value);
		}
	}

	public void Start()
	{
		if (!bool_2)
		{
			method_0();
		}
	}

	public void method_0()
	{
		if (!ScatteringShader)
		{
			ScatteringShader = GClass872.Find("Hidden/Time of Day/Scattering");
		}
		material_0 = CreateMaterial(ScatteringShader);
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
		mboit_Scattering_0 = GetComponent<MBOIT_Scattering>();
		bool_2 = true;
	}

	public void method_1(ref Material mat)
	{
		if (Application.isPlaying)
		{
			UnityEngine.Object.Destroy(mat);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(mat);
		}
		mat = null;
	}

	public void OnDestroy()
	{
		bool_2 = false;
		if (material_0 != null)
		{
			method_1(ref material_0);
		}
	}

	[ImageEffectOpaque]
	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		OnRenderImageNormalMode(source, destination);
	}

	public void OnRenderImageNormalMode(RenderTexture source, RenderTexture destination)
	{
		if (!CheckSupport(needDepth: true, needHdr: true))
		{
			Graphics.Blit(source, destination);
			return;
		}
		base.Sky.Components.Scattering = this;
		float nearClipPlane = cam.nearClipPlane;
		float farClipPlane = cam.farClipPlane;
		float fieldOfView = cam.fieldOfView;
		float aspect = cam.aspect;
		Vector3 position = cam.transform.position;
		Vector3 forward = cam.transform.forward;
		Vector3 right = cam.transform.right;
		Vector3 up = cam.transform.up;
		Matrix4x4 identity = Matrix4x4.identity;
		float num = fieldOfView * 0.5f;
		Vector3 vector = right * nearClipPlane * Mathf.Tan(num * (MathF.PI / 180f)) * aspect;
		Vector3 vector2 = up * nearClipPlane * Mathf.Tan(num * (MathF.PI / 180f));
		Vector3 vector3 = forward * nearClipPlane - vector + vector2;
		float num2 = vector3.magnitude * farClipPlane / nearClipPlane;
		vector3.Normalize();
		vector3 *= num2;
		Vector3 vector4 = forward * nearClipPlane + vector + vector2;
		vector4.Normalize();
		vector4 *= num2;
		Vector3 vector5 = forward * nearClipPlane + vector - vector2;
		vector5.Normalize();
		vector5 *= num2;
		Vector3 vector6 = forward * nearClipPlane - vector - vector2;
		vector6.Normalize();
		vector6 *= num2;
		identity.SetRow(0, vector3);
		identity.SetRow(1, vector4);
		identity.SetRow(2, vector5);
		identity.SetRow(3, vector6);
		if (FromLevelSettings)
		{
			LevelSettings instance = Singleton<LevelSettings>.Instance;
			if (instance != null)
			{
				HeightFalloff = instance.HeightFalloff;
				ZeroLevel = instance.ZeroLevel;
			}
		}
		material_0.SetMatrix(int_1, identity);
		material_0.SetTexture(int_2, DitheringTexture);
		Shader.SetGlobalVector(int_3, new Vector4(HeightFalloff, position.y - ZeroLevel, GlobalDensity, 0f));
		material_0.SetFloat(int_4, SunrizeGlow);
		if (Lighten)
		{
			material_0.EnableKeyword("LIGHTEN");
		}
		else
		{
			material_0.DisableKeyword("LIGHTEN");
		}
		RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
		CustomBlit(source, temporary, material_0, 1);
		CustomBlit(source, destination, material_0);
		Shader.SetGlobalTexture(int_5, temporary);
		RenderTexture.ReleaseTemporary(temporary);
	}

	public void method_2(bool enable)
	{
		if (!bool_2)
		{
			method_0();
		}
		_mboit = enable;
		if (mboit_Scattering_0 != null && WindowsManager.InstanceIsActive())
		{
			base.enabled = !enable;
			mboit_Scattering_0.enabled = enable;
		}
	}
}
