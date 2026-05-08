using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class TubeLight : BaseLight
{
	public Color m_Color = Color.white;

	[Range(0f, 2f)]
	public float m_SpecularScale = 1f;

	[Range(0f, 500f)]
	public float m_Range = 10f;

	[Range(0.001f, 30f)]
	public float m_Radius = 0.3f;

	[Range(0f, 100f)]
	public float m_Length;

	public MeshRenderer ShadowCube;

	public MeshRenderer InvertedShadowCube;

	public MeshRenderer ShadowRendererToDraw;

	public Material ShadowRendererMaterial;

	public bool IsShadowRendererInverted;

	[Range(0f, 10f)]
	public float ShadowFeather;

	[Range(0f, 10f)]
	public float InvertedShadowFeather;

	[HideInInspector]
	public Mesh m_Sphere;

	[HideInInspector]
	public Mesh m_Capsule;

	[HideInInspector]
	public Shader m_ProxyShader;

	private Material material_0;

	[Space]
	public bool m_Ambient;

	public bool m_Negative;

	public bool m_Specular = true;

	[Space]
	public bool m_RenderSource;

	public Color m_SourceColor = Color.white;

	public Material SourceMaterial;

	private Renderer renderer_0;

	private Transform transform_0;

	private Mesh mesh_0;

	private Material[] material_1;

	private float float_0 = -1f;

	private bool bool_0;

	private MaterialPropertyBlock materialPropertyBlock_0;

	private const float float_1 = 0.001f;

	private float float_2 = -1f;

	private float float_3 = -1f;

	private bool bool_1;

	private Matrix4x4 matrix4x4_0;

	private Vector4 vector4_0;

	private Vector4 vector4_1;

	private bool bool_2;

	private bool bool_3;

	private bool bool_4;

	private int int_0;

	private bool bool_5;

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

	private Dictionary<Camera, CommandBuffer> dictionary_0 = new Dictionary<Camera, CommandBuffer>();

	private static CameraEvent cameraEvent_0 = CameraEvent.AfterImageEffectsOpaque;

	private Vector4[] vector4_2 = new Vector4[6];

	private Vector4[] vector4_3 = new Vector4[6];

	private static readonly int int_12 = Shader.PropertyToID("_EmissionColor");

	public bool Boolean_0
	{
		get
		{
			if (m_RenderSource)
			{
				return m_Radius >= 0.001f;
			}
			return false;
		}
	}

	public void Start()
	{
		if (method_0())
		{
			method_2(forceUpdate: true);
			method_3();
			method_11();
		}
	}

	public bool method_0()
	{
		if (bool_0)
		{
			return true;
		}
		if (!(m_ProxyShader == null) && !(m_Sphere == null) && !(m_Capsule == null))
		{
			material_0 = new Material(m_ProxyShader);
			material_0.hideFlags = HideFlags.HideAndDontSave;
			mesh_0 = UnityEngine.Object.Instantiate(m_Capsule);
			mesh_0.hideFlags = HideFlags.HideAndDontSave;
			base.gameObject.GetComponent<MeshFilter>().sharedMesh = mesh_0;
			renderer_0 = base.gameObject.GetComponent<MeshRenderer>();
			renderer_0.enabled = true;
			material_1 = renderer_0.sharedMaterials;
			transform_0 = base.transform;
			method_12(ShadowCube, vector4_2);
			method_12(InvertedShadowCube, vector4_3);
			bool_0 = true;
			return true;
		}
		return false;
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (materialPropertyBlock_0 == null)
		{
			materialPropertyBlock_0 = new MaterialPropertyBlock();
		}
		if (method_0())
		{
			method_2(forceUpdate: true);
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(method_5));
			if (renderer_0 == null)
			{
				renderer_0 = GetComponent<MeshRenderer>();
			}
			renderer_0.enabled = true;
			material_1 = renderer_0.sharedMaterials;
			method_11();
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		method_1();
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(method_5));
		if (renderer_0 == null)
		{
			renderer_0 = GetComponent<MeshRenderer>();
		}
		renderer_0.enabled = false;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (material_0 != null)
		{
			UnityEngine.Object.DestroyImmediate(material_0);
		}
		if (mesh_0 != null)
		{
			UnityEngine.Object.DestroyImmediate(mesh_0);
		}
		method_1();
	}

	public void method_1()
	{
		Dictionary<Camera, CommandBuffer>.Enumerator enumerator = dictionary_0.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<Camera, CommandBuffer> current = enumerator.Current;
			if (current.Key != null && current.Value != null)
			{
				current.Key.RemoveCommandBuffer(cameraEvent_0, current.Value);
			}
		}
		dictionary_0.Clear();
	}

	public void method_2(bool forceUpdate = false)
	{
		bool flag = Math.Abs(m_Length - float_0) > 0.01f;
		bool num = Math.Abs(m_Radius - float_3) > 0.01f;
		bool flag2 = Math.Abs(m_Range - float_2) > 0.01f;
		bool flag3 = Boolean_0 != bool_1;
		if (!(num || flag2 || flag || flag3) && !forceUpdate)
		{
			return;
		}
		m_Range = Mathf.Max(m_Range, 0f);
		m_Radius = Mathf.Max(m_Radius, 0.001f);
		m_Length = Mathf.Max(m_Length, 0f);
		m_Intensity = Mathf.Max(m_Intensity, 0f);
		Vector3 vector = (Boolean_0 ? (Vector3.one * m_Radius * 2f) : Vector3.one);
		if (transform_0.localScale != vector || flag)
		{
			float_0 = m_Length;
			Vector3[] vertices = m_Capsule.vertices;
			for (int i = 0; i < vertices.Length; i++)
			{
				if (Boolean_0)
				{
					vertices[i].y += Mathf.Sign(vertices[i].y) * (-0.5f + 0.25f * m_Length / m_Radius);
				}
				else
				{
					vertices[i] = Vector3.one * 0.0001f;
				}
			}
			mesh_0.vertices = vertices;
		}
		transform_0.localScale = vector;
		float num2 = m_Range + m_Radius;
		num2 += 0.5f * m_Length;
		num2 *= 1.02f;
		num2 /= m_Radius;
		mesh_0.bounds = new Bounds(Vector3.zero, Vector3.one * num2);
		float_3 = m_Radius;
		float_2 = m_Range;
		bool_1 = Boolean_0;
	}

	public void method_3()
	{
		materialPropertyBlock_0.SetVector(int_12, m_SourceColor * Mathf.Sqrt(m_Intensity) * 2f);
		renderer_0.SetPropertyBlock(materialPropertyBlock_0);
	}

	public override void ManualUpdate(float dt)
	{
		if (m_Intensity <= 0f)
		{
			if (dictionary_0.Count > 0)
			{
				method_1();
				renderer_0.enabled = false;
			}
		}
		else if (method_0())
		{
			method_2();
			renderer_0.enabled = true;
			method_11();
		}
	}

	public Color method_4()
	{
		if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
		{
			return m_Color * m_Intensity;
		}
		return new Color(Mathf.GammaToLinearSpace(m_Color.r * m_Intensity), Mathf.GammaToLinearSpace(m_Color.g * m_Intensity), Mathf.GammaToLinearSpace(m_Color.b * m_Intensity), 1f);
	}

	public void method_5(Camera cam)
	{
		if (!(m_Intensity <= 0f) && !method_13(cam) && method_0() && (cam.CompareTag("MainCamera") || cam.CompareTag("OpticCamera")))
		{
			method_6(cam);
		}
	}

	public void method_6(Camera cam)
	{
		CommandBuffer commandBuffer = null;
		if (!dictionary_0.ContainsKey(cam))
		{
			commandBuffer = new CommandBuffer();
			commandBuffer.name = base.gameObject.name;
			dictionary_0[cam] = commandBuffer;
			cam.AddCommandBuffer(cameraEvent_0, commandBuffer);
			cam.depthTextureMode |= DepthTextureMode.Depth;
		}
		else
		{
			commandBuffer = dictionary_0[cam];
			commandBuffer.Clear();
		}
		method_7();
		method_9(commandBuffer, cam);
		method_10(commandBuffer);
		if (bool_4)
		{
			commandBuffer.DrawRenderer(ShadowRendererToDraw, ShadowRendererMaterial);
		}
		int num = 1;
		if (bool_4)
		{
			num++;
			if (IsShadowRendererInverted)
			{
				num++;
			}
		}
		commandBuffer.DrawMesh(m_Sphere, matrix4x4_0, material_0, 0, 0);
		commandBuffer.DrawMesh(m_Sphere, matrix4x4_0, material_0, 0, num);
	}

	public void method_7()
	{
		if (!bool_5)
		{
			Transform transform = base.transform;
			Vector3 up = transform.up;
			Vector3 vector = transform.position - 0.5f * up * m_Length;
			vector4_1 = new Vector4(up.x, up.y, up.z, 0f);
			float num = m_Range + m_Radius;
			num += 0.5f * (m_Length - m_Radius);
			vector4_0 = new Vector4(vector.x, vector.y, vector.z, 1f / (num * num));
			Matrix4x4 matrix4x = Matrix4x4.Scale(Vector3.one * num * 2.05f);
			matrix4x4_0 = transform.localToWorldMatrix * matrix4x;
			bool_2 = ShadowCube != null && ShadowCube.gameObject.activeInHierarchy;
			bool_3 = InvertedShadowCube != null && InvertedShadowCube.gameObject.activeInHierarchy;
			bool_4 = ShadowRendererToDraw != null && ShadowRendererMaterial != null;
			method_8();
			bool_5 = true;
		}
	}

	public void method_8()
	{
		int_1 = Shader.PropertyToID("_LightPos");
		int_2 = Shader.PropertyToID("_LightAxis");
		int_3 = Shader.PropertyToID("_LightAsQuad");
		int_4 = Shader.PropertyToID("_LightRadius");
		int_5 = Shader.PropertyToID("_LightLength");
		int_6 = Shader.PropertyToID("_LightColor");
		int_7 = Shader.PropertyToID("_SpecularScale");
		int_8 = Shader.PropertyToID("_ShadowPlaneFeather");
		int_9 = Shader.PropertyToID("_ShadowPlanes");
		int_10 = Shader.PropertyToID("_InvertedShadowPlaneFeather");
		int_11 = Shader.PropertyToID("_InvertedShadowPlanes");
	}

	public void method_9(CommandBuffer buf, Camera cam)
	{
		if (m_Ambient)
		{
			buf.EnableShaderKeyword("AMBIENT");
		}
		else
		{
			buf.DisableShaderKeyword("AMBIENT");
		}
		if (m_Specular && !m_Negative && !m_Ambient)
		{
			buf.EnableShaderKeyword("SPECULAR");
		}
		else
		{
			buf.DisableShaderKeyword("SPECULAR");
		}
		if (m_Negative)
		{
			buf.EnableShaderKeyword("NEGATIVE");
		}
		else
		{
			buf.DisableShaderKeyword("NEGATIVE");
		}
		buf.DisableShaderKeyword("SCENEVIEWCAMERA");
		if (cam.CompareTag("OpticCamera"))
		{
			buf.EnableShaderKeyword("OPTICCAMERA");
		}
		else
		{
			buf.DisableShaderKeyword("OPTICCAMERA");
		}
		buf.SetGlobalVector(int_1, vector4_0);
		buf.SetGlobalVector(int_2, vector4_1);
		buf.SetGlobalFloat(int_3, 0f);
		buf.SetGlobalFloat(int_4, m_Radius);
		buf.SetGlobalFloat(int_5, m_Length);
		buf.SetGlobalVector(int_6, method_4());
		buf.SetGlobalFloat(int_7, m_SpecularScale);
	}

	public void method_10(CommandBuffer buf)
	{
		if (bool_2)
		{
			buf.EnableShaderKeyword("SHADOWCUBEPLANES");
			buf.SetGlobalFloat(int_8, ShadowFeather);
			buf.SetGlobalVectorArray(int_9, vector4_2);
		}
		else
		{
			buf.DisableShaderKeyword("SHADOWCUBEPLANES");
		}
		if (bool_3)
		{
			buf.EnableShaderKeyword("INVERTEDSHADOWCUBEPLANES");
			buf.SetGlobalFloat(int_10, InvertedShadowFeather);
			buf.SetGlobalVectorArray(int_11, vector4_3);
		}
		else
		{
			buf.DisableShaderKeyword("INVERTEDSHADOWCUBEPLANES");
		}
	}

	public void method_11()
	{
		if (Boolean_0)
		{
			method_3();
			if (material_1.Length == 0)
			{
				renderer_0.sharedMaterial = SourceMaterial;
				material_1 = renderer_0.sharedMaterials;
			}
		}
		else if (material_1.Length != 0)
		{
			renderer_0.sharedMaterials = new Material[0];
			material_1 = renderer_0.sharedMaterials;
		}
	}

	public void method_12(MeshRenderer cube, Vector4[] planes)
	{
		if (!(cube == null))
		{
			Transform transform = cube.transform;
			Vector3 position = transform.position;
			Vector3 rhs = -transform.forward;
			planes[0] = new Vector4(w: Vector3.Dot(position + -transform.forward * transform.lossyScale.z / 2f, rhs), x: rhs.x, y: rhs.y, z: rhs.z);
			rhs = transform.forward;
			planes[1] = new Vector4(w: Vector3.Dot(position + transform.forward * transform.lossyScale.z / 2f, rhs), x: rhs.x, y: rhs.y, z: rhs.z);
			rhs = -Vector3.Cross(transform.forward, transform.up);
			planes[2] = new Vector4(w: Vector3.Dot(position + rhs * transform.lossyScale.x / 2f, rhs), x: rhs.x, y: rhs.y, z: rhs.z);
			rhs = Vector3.Cross(transform.forward, transform.up);
			planes[3] = new Vector4(w: Vector3.Dot(position + rhs * transform.lossyScale.x / 2f, rhs), x: rhs.x, y: rhs.y, z: rhs.z);
			rhs = -transform.up;
			planes[4] = new Vector4(w: Vector3.Dot(position + rhs * transform.lossyScale.y / 2f, rhs), x: rhs.x, y: rhs.y, z: rhs.z);
			rhs = transform.up;
			float w = Vector3.Dot(position + rhs * transform.lossyScale.y / 2f, rhs);
			planes[5] = new Vector4(rhs.x, rhs.y, rhs.z, w);
		}
	}

	public void OnDrawGizmosSelected()
	{
		if (!(transform_0 == null))
		{
			Gizmos.color = Color.white;
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(transform_0.position, transform_0.rotation, Vector3.one);
			Gizmos.matrix = matrix;
			Gizmos.DrawWireSphere(Vector3.zero, m_Radius + m_Range + 0.5f * m_Length);
			Vector3 vector = 0.5f * Vector3.up * m_Length;
			Gizmos.DrawWireSphere(vector, m_Radius);
			if (m_Length != 0f)
			{
				Vector3 vector2 = -0.5f * Vector3.up * m_Length;
				Gizmos.DrawWireSphere(vector2, m_Radius);
				Vector3 vector3 = Vector3.forward * m_Radius;
				Gizmos.DrawLine(vector + vector3, vector2 + vector3);
				Gizmos.DrawLine(vector - vector3, vector2 - vector3);
				vector3 = Vector3.right * m_Radius;
				Gizmos.DrawLine(vector + vector3, vector2 + vector3);
				Gizmos.DrawLine(vector - vector3, vector2 - vector3);
			}
		}
	}

	public void OnDrawGizmos()
	{
	}

	public bool method_13(Camera cam)
	{
		RenderTexture targetTexture = cam.targetTexture;
		if (targetTexture != null)
		{
			return targetTexture.format == RenderTextureFormat.Shadowmap;
		}
		return false;
	}
}
