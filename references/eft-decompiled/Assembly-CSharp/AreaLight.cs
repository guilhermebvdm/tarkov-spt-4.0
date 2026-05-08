using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class AreaLight : BaseLight
{
	public enum TextureSize
	{
		x512 = 0x200,
		x1024 = 0x400,
		x2048 = 0x800,
		x4096 = 0x1000
	}

	[GAttribute13(0.001f)]
	public float size = 1f;

	[GAttribute13(1f)]
	public float length = 1f;

	[GAttribute13(0f)]
	public float depth = 2f;

	private Vector3 vector3_0 = new Vector3(1f, 1f, 2f);

	public Vector3 m_ClipBoxSize = new Vector3(2f, 2f, 4f);

	[Range(0f, 179f)]
	public float m_Angle;

	[Range(0f, 0.99f)]
	public float m_Hardness;

	[Range(0f, 2f)]
	public float m_SpecularScale = 1f;

	public Color m_Color = Color.white;

	[Space]
	public bool m_Ambient;

	public bool m_Negative;

	public bool m_Specular = true;

	[GAttribute27]
	public bool m_RenderSource = true;

	[GAttribute27]
	public Color m_SourceColor = Color.white;

	[GAttribute27]
	public Material SourceMaterial;

	[GAttribute27]
	public bool IsShadowCubeAnimated;

	[GAttribute27]
	public MeshRenderer ShadowCube;

	[GAttribute27]
	public MeshRenderer InvertedShadowCube;

	[GAttribute27]
	public MeshRenderer ShadowRendererToDraw;

	[GAttribute27]
	public Material ShadowRendererMaterial;

	[GAttribute27]
	public bool IsShadowRendererInverted;

	[GAttribute27]
	[Range(0f, 10f)]
	public float ShadowFeather;

	[GAttribute27]
	[Range(0f, 10f)]
	public float InvertedShadowFeather;

	[GAttribute27]
	public bool m_Spot;

	[GAttribute27]
	public bool m_Shadows;

	[GAttribute27]
	public LayerMask m_ShadowCullingMask = -1;

	[GAttribute27]
	public TextureSize m_ShadowmapRes = TextureSize.x2048;

	[GAttribute27]
	[GAttribute13(0f)]
	public float m_ReceiverSearchDistance = 24f;

	[GAttribute27]
	[GAttribute13(0f)]
	public float m_ReceiverDistanceScale = 5f;

	[GAttribute27]
	[GAttribute13(0f)]
	public float m_LightNearSize = 4f;

	[GAttribute27]
	[GAttribute13(0f)]
	public float m_LightFarSize = 22f;

	[GAttribute27]
	[Range(0f, 0.1f)]
	public float m_ShadowBias = 0.001f;

	private MeshRenderer meshRenderer_0;

	private Mesh mesh_0;

	private Material[] material_0;

	[HideInInspector]
	public Mesh m_Quad;

	private Vector2 vector2_0 = Vector2.zero;

	private Vector3 vector3_1 = Vector3.zero;

	private float float_0 = -1f;

	private bool bool_0;

	private MaterialPropertyBlock materialPropertyBlock_0;

	private static Vector3[] vector3_2 = new Vector3[4];

	private static readonly int int_0 = Shader.PropertyToID("_EmissionColor");

	[HideInInspector]
	public Mesh m_Cube;

	[HideInInspector]
	public Shader m_ProxyShader;

	private Material material_1;

	private static Texture2D texture2D_0;

	private static Texture2D texture2D_1;

	private static Texture2D texture2D_2;

	private Vector3 vector3_3;

	private Vector3 vector3_4;

	private float float_1;

	private Matrix4x4 matrix4x4_0;

	private Matrix4x4 matrix4x4_1;

	private Matrix4x4 matrix4x4_2;

	private Matrix4x4 matrix4x4_3;

	private bool bool_1;

	private bool bool_2;

	private bool bool_3;

	private int int_1;

	private bool bool_4;

	private Dictionary<Camera, CommandBuffer> dictionary_0 = new Dictionary<Camera, CommandBuffer>();

	private bool bool_5;

	private Vector4[] vector4_0 = new Vector4[6];

	private Vector4[] vector4_1 = new Vector4[6];

	private Bounds bounds_0;

	private Bounds bounds_1;

	private static CameraEvent cameraEvent_0 = CameraEvent.AfterImageEffectsOpaque;

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

	private static readonly float[,] float_2 = new float[4, 2]
	{
		{ 1f, 1f },
		{ 1f, -1f },
		{ -1f, -1f },
		{ -1f, 1f }
	};

	private Camera camera_0;

	private Transform transform_0;

	[HideInInspector]
	public Shader m_ShadowmapShader;

	[HideInInspector]
	public Shader m_BlurShadowmapShader;

	private Material material_2;

	private RenderTexture renderTexture_0;

	private RenderTexture renderTexture_1;

	private Texture2D texture2D_3;

	private int int_19 = -1;

	private int int_20 = -1;

	private FogLight fogLight_0;

	private RenderTexture[] renderTexture_2;

	private static readonly int int_21 = Shader.PropertyToID("_TexelSize");

	private static readonly int int_22 = Shader.PropertyToID("_Shadowmap");

	private static readonly int int_23 = Shader.PropertyToID("_ShadowmapDummy");

	private static readonly int int_24 = Shader.PropertyToID("_ZParams");

	private static readonly int int_25 = Shader.PropertyToID("_ESMExponent");

	private static readonly int int_26 = Shader.PropertyToID("_MainTex");

	private static readonly int int_27 = Shader.PropertyToID("_MainTex_TexelSize");

	private static readonly int int_28 = Shader.PropertyToID("_BlurSize");

	public void Awake()
	{
		if (method_0())
		{
			method_5();
			method_1(forceUpdate: true);
			method_4();
		}
	}

	public bool method_0()
	{
		if (bool_0)
		{
			return true;
		}
		if (!(m_Quad == null) && method_8())
		{
			meshRenderer_0 = GetComponent<MeshRenderer>();
			meshRenderer_0.enabled = true;
			mesh_0 = UnityEngine.Object.Instantiate(m_Quad);
			mesh_0.hideFlags = HideFlags.HideAndDontSave;
			base.gameObject.GetComponent<MeshFilter>().sharedMesh = mesh_0;
			material_0 = meshRenderer_0.sharedMaterials;
			Transform transform = base.transform;
			if (transform.localScale != Vector3.one)
			{
				transform.localScale = Vector3.one;
			}
			method_3();
			method_17(ShadowCube, vector4_0);
			method_17(InvertedShadowCube, vector4_1);
			bool_0 = true;
			return true;
		}
		return false;
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (method_0())
		{
			method_1(forceUpdate: true);
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(method_2));
			if (meshRenderer_0 == null)
			{
				meshRenderer_0 = GetComponent<MeshRenderer>();
			}
			meshRenderer_0.enabled = true;
			material_0 = meshRenderer_0.sharedMaterials;
			method_4();
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		method_10();
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(method_2));
		if (meshRenderer_0 == null)
		{
			meshRenderer_0 = GetComponent<MeshRenderer>();
		}
		meshRenderer_0.enabled = false;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (material_1 != null)
		{
			UnityEngine.Object.DestroyImmediate(material_1);
		}
		if (mesh_0 != null)
		{
			UnityEngine.Object.DestroyImmediate(mesh_0);
		}
		method_10();
	}

	public void method_1(bool forceUpdate = false)
	{
		if (!vector3_0.Equals(vector3_1) || !(Math.Abs(m_Angle - float_0) < 0.01f) || forceUpdate)
		{
			vector3_0.x = Mathf.Max(vector3_0.x, 0.001f);
			vector3_0.y = Mathf.Max(vector3_0.y, 0.001f);
			vector3_0.z = Mathf.Max(vector3_0.z, 0f);
			Vector2 vector = ((!m_RenderSource || !base.enabled) ? (Vector2.one * 0.0001f) : new Vector2(vector3_0.x, vector3_0.y));
			if (vector != vector2_0)
			{
				float num = vector.x * 0.5f;
				float num2 = vector.y * 0.5f;
				float newZ = -0.001f;
				vector3_2[0].Set(0f - num, num2, newZ);
				vector3_2[1].Set(num, 0f - num2, newZ);
				vector3_2[2].Set(num, num2, newZ);
				vector3_2[3].Set(0f - num, 0f - num2, newZ);
				mesh_0.vertices = vector3_2;
				vector2_0 = vector;
			}
			mesh_0.bounds = GetFrustumBounds();
			vector3_1 = vector3_0;
			float_0 = m_Angle;
		}
	}

	public override void ManualUpdate(float dt)
	{
		if (m_Intensity <= 0f)
		{
			if (bool_5)
			{
				method_10();
				meshRenderer_0.enabled = false;
			}
		}
		else if (base.gameObject.activeInHierarchy && base.enabled)
		{
			if (method_0())
			{
				method_1();
				meshRenderer_0.enabled = true;
				method_4();
			}
		}
		else
		{
			method_10();
		}
	}

	public void method_2(Camera cam)
	{
		if (!(m_Intensity <= 0f) && method_0() && (cam.CompareTag("MainCamera") || cam.CompareTag("OpticCamera")))
		{
			SetUpCommandBuffer(cam);
		}
	}

	public void method_3()
	{
		vector3_0 = new Vector3(length * size, size, depth);
		m_ClipBoxSize.x = ((m_ClipBoxSize.x < vector3_0.x) ? vector3_0.x : m_ClipBoxSize.x);
		m_ClipBoxSize.y = ((m_ClipBoxSize.y < vector3_0.y) ? vector3_0.y : m_ClipBoxSize.y);
		m_ClipBoxSize.z = ((m_ClipBoxSize.z < vector3_0.z) ? vector3_0.z : m_ClipBoxSize.z);
	}

	public void method_4()
	{
		if (m_RenderSource)
		{
			method_5();
			if (material_0.Length == 0)
			{
				meshRenderer_0.sharedMaterial = SourceMaterial;
				material_0 = meshRenderer_0.sharedMaterials;
			}
		}
		else if (material_0.Length != 0)
		{
			meshRenderer_0.sharedMaterials = new Material[0];
			material_0 = meshRenderer_0.sharedMaterials;
		}
	}

	public void method_5()
	{
		Color color = new Color(Mathf.GammaToLinearSpace(m_SourceColor.r), Mathf.GammaToLinearSpace(m_SourceColor.g), Mathf.GammaToLinearSpace(m_SourceColor.b), 1f);
		if (materialPropertyBlock_0 == null)
		{
			materialPropertyBlock_0 = new MaterialPropertyBlock();
		}
		materialPropertyBlock_0.SetVector(int_0, color * m_Intensity);
		meshRenderer_0.SetPropertyBlock(materialPropertyBlock_0);
	}

	public float GetNearToCenter()
	{
		if (m_Angle == 0f)
		{
			return 0f;
		}
		return vector3_0.y * 0.5f / Mathf.Tan(m_Angle * 0.5f * (MathF.PI / 180f));
	}

	public Matrix4x4 method_6(float zOffset)
	{
		Matrix4x4 identity = Matrix4x4.identity;
		identity.SetColumn(3, new Vector4(0f, 0f, zOffset, 1f));
		return identity;
	}

	public Matrix4x4 GetProjectionMatrix(bool linearZ = false)
	{
		Matrix4x4 matrix4x;
		if (m_Angle == 0f)
		{
			matrix4x = Matrix4x4.Ortho(-0.5f * vector3_0.x, 0.5f * vector3_0.x, -0.5f * vector3_0.y, 0.5f * vector3_0.y, 0f, 0f - vector3_0.z);
		}
		else
		{
			float nearToCenter = GetNearToCenter();
			if (linearZ)
			{
				matrix4x = method_7(m_Angle, vector3_0.x / vector3_0.y, nearToCenter, nearToCenter + vector3_0.z);
			}
			else
			{
				matrix4x = Matrix4x4.Perspective(m_Angle, vector3_0.x / vector3_0.y, nearToCenter, nearToCenter + vector3_0.z);
				matrix4x *= Matrix4x4.Scale(new Vector3(1f, 1f, -1f));
			}
			matrix4x *= method_6(nearToCenter);
		}
		return matrix4x * base.transform.worldToLocalMatrix;
	}

	public Vector4 MultiplyPoint(Matrix4x4 m, Vector3 v)
	{
		Vector4 result = default(Vector4);
		result.x = m.m00 * v.x + m.m01 * v.y + m.m02 * v.z + m.m03;
		result.y = m.m10 * v.x + m.m11 * v.y + m.m12 * v.z + m.m13;
		result.z = m.m20 * v.x + m.m21 * v.y + m.m22 * v.z + m.m23;
		result.w = m.m30 * v.x + m.m31 * v.y + m.m32 * v.z + m.m33;
		return result;
	}

	public Matrix4x4 method_7(float fov, float aspect, float near, float far)
	{
		float f = MathF.PI / 180f * fov * 0.5f;
		float num = Mathf.Cos(f) / Mathf.Sin(f);
		float num2 = 1f / (far - near);
		Matrix4x4 result = default(Matrix4x4);
		result.m00 = num / aspect;
		result.m01 = 0f;
		result.m02 = 0f;
		result.m03 = 0f;
		result.m10 = 0f;
		result.m11 = num;
		result.m12 = 0f;
		result.m13 = 0f;
		result.m20 = 0f;
		result.m21 = 0f;
		result.m22 = 2f * num2;
		result.m23 = (0f - (far + near)) * num2;
		result.m30 = 0f;
		result.m31 = 0f;
		result.m32 = 1f;
		result.m33 = 0f;
		return result;
	}

	public Vector4 GetPosition()
	{
		Transform transform = base.transform;
		if (m_Angle == 0f)
		{
			Vector3 vector = -transform.forward;
			return new Vector4(vector.x, vector.y, vector.z, 0f);
		}
		Vector3 vector2 = transform.position - GetNearToCenter() * transform.forward;
		return new Vector4(vector2.x, vector2.y, vector2.z, 1f);
	}

	public Bounds GetFrustumBounds()
	{
		if (!m_Spot && !m_Shadows)
		{
			return new Bounds(new Vector3(0f, 0f, 0.5f * m_ClipBoxSize.z), m_ClipBoxSize);
		}
		if (m_Angle == 0f)
		{
			return new Bounds(Vector3.zero, vector3_0);
		}
		float num = Mathf.Tan(m_Angle * 0.5f * (MathF.PI / 180f));
		float num2 = vector3_0.y * 0.5f / num;
		float z = vector3_0.z;
		float num3 = (num2 + vector3_0.z) * num * 2f;
		float x = vector3_0.x * num3 / vector3_0.y;
		return new Bounds(Vector3.forward * vector3_0.z * 0.5f, new Vector3(x, num3, z));
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;
		if (m_Angle == 0f)
		{
			Gizmos.matrix = base.transform.localToWorldMatrix;
			if (m_Spot || m_Shadows)
			{
				Gizmos.DrawWireCube(new Vector3(0f, 0f, 0.5f * vector3_0.z), vector3_0);
				return;
			}
			Gizmos.DrawWireCube(new Vector3(0f, 0f, 0f), new Vector3(vector3_0.x, vector3_0.y, 0f));
		}
		float nearToCenter = GetNearToCenter();
		Gizmos.matrix = base.transform.localToWorldMatrix * method_6(0f - nearToCenter);
		if (!m_Spot && !m_Shadows)
		{
			Gizmos.DrawFrustum(new Vector3(0f, 0f, nearToCenter), m_Angle, nearToCenter, nearToCenter, vector3_0.x / vector3_0.y);
		}
		else
		{
			Gizmos.DrawFrustum(new Vector3(0f, 0f, nearToCenter), m_Angle, nearToCenter + vector3_0.z, nearToCenter, vector3_0.x / vector3_0.y);
		}
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = Color.yellow;
		Bounds frustumBounds = GetFrustumBounds();
		Gizmos.DrawWireCube(frustumBounds.center, frustumBounds.size);
	}

	public bool method_8()
	{
		if (!(m_ProxyShader == null) && !(m_Cube == null))
		{
			material_1 = new Material(m_ProxyShader);
			material_1.hideFlags = HideFlags.HideAndDontSave;
			return true;
		}
		return false;
	}

	public void method_9()
	{
		material_1.SetTexture(int_12, texture2D_1);
		material_1.SetTexture(int_13, texture2D_0);
		material_1.SetTexture(int_14, texture2D_2);
	}

	public void method_10()
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
		bool_5 = false;
	}

	public CommandBuffer method_11(Camera cam)
	{
		if (cam == null)
		{
			return null;
		}
		CommandBuffer commandBuffer = null;
		if (!dictionary_0.ContainsKey(cam))
		{
			commandBuffer = new CommandBuffer();
			commandBuffer.name = base.gameObject.name;
			dictionary_0[cam] = commandBuffer;
			cam.AddCommandBuffer(cameraEvent_0, commandBuffer);
			cam.depthTextureMode |= DepthTextureMode.Depth;
			bool_5 = true;
		}
		else
		{
			commandBuffer = dictionary_0[cam];
			commandBuffer.Clear();
		}
		return commandBuffer;
	}

	public void SetUpCommandBuffer(Camera cam)
	{
		if (method_29())
		{
			return;
		}
		CommandBuffer commandBuffer = method_11(cam);
		method_12();
		method_14(commandBuffer, cam);
		if (m_Shadows)
		{
			method_25(commandBuffer);
		}
		method_15(commandBuffer);
		if (bool_3)
		{
			commandBuffer.DrawRenderer(ShadowRendererToDraw, ShadowRendererMaterial);
		}
		else
		{
			commandBuffer.DrawMesh(m_Cube, matrix4x4_1, material_1, 0, 0);
		}
		int num = int_1;
		if (bool_3)
		{
			num += 2;
			if (IsShadowRendererInverted)
			{
				num += 2;
			}
		}
		commandBuffer.DrawMesh(m_Cube, matrix4x4_1, material_1, 0, num);
	}

	public void method_12()
	{
		if (!bool_4)
		{
			float z = 0.01f;
			Transform transform = base.transform;
			vector3_3 = transform.position;
			vector3_4 = transform.forward;
			float_1 = 2f * Mathf.Atan(0.5f * m_Angle) / MathF.PI;
			matrix4x4_0 = default(Matrix4x4);
			for (int i = 0; i < 4; i++)
			{
				matrix4x4_0.SetRow(i, transform.TransformPoint(new Vector3(vector3_0.x * float_2[i, 0], vector3_0.y * float_2[i, 1], z) * 0.5f));
			}
			Bounds frustumBounds = GetFrustumBounds();
			Matrix4x4 matrix4x = Matrix4x4.TRS(new Vector3(0f, 0f, frustumBounds.size.z * 0.5f), Quaternion.identity, frustumBounds.size);
			matrix4x4_1 = transform.localToWorldMatrix * matrix4x;
			Matrix4x4 matrix4x2 = Matrix4x4.Ortho(-0.5f * frustumBounds.size.x, 0.5f * frustumBounds.size.x, -0.5f * frustumBounds.size.y, 0.5f * frustumBounds.size.y, 0f, 0f - frustumBounds.size.z);
			matrix4x4_2 = matrix4x2 * base.transform.worldToLocalMatrix;
			matrix4x4_3 = GetProjectionMatrix();
			if (texture2D_1 == null)
			{
				texture2D_1 = GClass955.LoadLUT(GClass955.LUTType.TransformInv_DisneyDiffuse);
			}
			if (texture2D_0 == null)
			{
				texture2D_0 = GClass955.LoadLUT(GClass955.LUTType.TransformInv_GGX);
			}
			if (texture2D_2 == null)
			{
				texture2D_2 = GClass955.LoadLUT(GClass955.LUTType.AmpDiffAmpSpecFresnel);
			}
			bool_1 = ShadowCube != null && ShadowCube.gameObject.activeInHierarchy;
			bool_2 = InvertedShadowCube != null && InvertedShadowCube.gameObject.activeInHierarchy;
			bool_3 = ShadowRendererToDraw != null && ShadowRendererMaterial != null;
			int_1 = ((!m_Negative && !m_Ambient && m_Shadows) ? 1 : 2);
			method_13();
			bool_4 = true;
		}
	}

	public void method_13()
	{
		int_2 = Shader.PropertyToID("_LightPos");
		int_3 = Shader.PropertyToID("_LightDir");
		int_4 = Shader.PropertyToID("_LightColor");
		int_5 = Shader.PropertyToID("_SpecularScale");
		int_6 = Shader.PropertyToID("_Hardness");
		int_7 = Shader.PropertyToID("_AtanFov");
		int_12 = Shader.PropertyToID("_TransformInv_Diffuse");
		int_13 = Shader.PropertyToID("_TransformInv_Specular");
		int_14 = Shader.PropertyToID("_AmpDiffAmpSpecFresnel");
		int_8 = Shader.PropertyToID("_LightAsQuad");
		int_9 = Shader.PropertyToID("_LightVerts");
		int_10 = Shader.PropertyToID("_ShadowProjectionMatrix");
		int_11 = Shader.PropertyToID("_ClipCube");
		int_15 = Shader.PropertyToID("_ShadowPlaneFeather");
		int_16 = Shader.PropertyToID("_ShadowPlanes");
		int_17 = Shader.PropertyToID("_InvertedShadowPlaneFeather");
		int_18 = Shader.PropertyToID("_InvertedShadowPlanes");
	}

	public void method_14(CommandBuffer buf, Camera cam)
	{
		if (m_Negative)
		{
			buf.EnableShaderKeyword("NEGATIVE");
		}
		else
		{
			buf.DisableShaderKeyword("NEGATIVE");
		}
		if (m_Ambient)
		{
			buf.EnableShaderKeyword("AMBIENT");
		}
		else
		{
			buf.DisableShaderKeyword("AMBIENT");
		}
		if (m_Spot)
		{
			buf.EnableShaderKeyword("SPOTLIGHT");
		}
		else
		{
			buf.DisableShaderKeyword("SPOTLIGHT");
		}
		if (m_Specular && !m_Negative && !m_Ambient)
		{
			buf.EnableShaderKeyword("SPECULAR");
		}
		else
		{
			buf.DisableShaderKeyword("SPECULAR");
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
		buf.SetGlobalVector(int_2, vector3_3);
		buf.SetGlobalVector(int_3, vector3_4);
		buf.SetGlobalVector(int_4, method_20());
		buf.SetGlobalFloat(int_5, m_SpecularScale);
		buf.SetGlobalFloat(int_6, m_Hardness);
		buf.SetGlobalFloat(int_7, float_1);
		method_9();
		buf.SetGlobalFloat(int_8, 0f);
		buf.SetGlobalMatrix(int_9, matrix4x4_0);
		buf.SetGlobalMatrix(int_10, matrix4x4_3);
		buf.SetGlobalMatrix(int_11, matrix4x4_2);
	}

	public void method_15(CommandBuffer buf)
	{
		if (IsShadowCubeAnimated)
		{
			method_16(ShadowCube, vector4_0, ref bounds_0);
		}
		if (bool_1)
		{
			buf.EnableShaderKeyword("SHADOWCUBEPLANES");
			buf.SetGlobalFloat(int_15, ShadowFeather);
			buf.SetGlobalVectorArray(int_16, vector4_0);
		}
		else
		{
			buf.DisableShaderKeyword("SHADOWCUBEPLANES");
		}
		if (bool_2)
		{
			buf.EnableShaderKeyword("INVERTEDSHADOWCUBEPLANES");
			buf.SetGlobalFloat(int_17, InvertedShadowFeather);
			buf.SetGlobalVectorArray(int_18, vector4_1);
		}
		else
		{
			buf.DisableShaderKeyword("INVERTEDSHADOWCUBEPLANES");
		}
	}

	public void method_16(MeshRenderer cube, Vector4[] planes, ref Bounds bounds)
	{
		if (!(cube == null) && !(cube.bounds == bounds))
		{
			method_17(cube, planes);
			bounds = cube.bounds;
		}
	}

	public void method_17(MeshRenderer cube, Vector4[] planes)
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

	public void method_18(string keyword, bool on)
	{
		if (on)
		{
			material_1.EnableKeyword(keyword);
		}
		else
		{
			material_1.DisableKeyword(keyword);
		}
	}

	public void method_19(ref RenderTexture rt)
	{
		if (!(rt == null))
		{
			RenderTexture.ReleaseTemporary(rt);
			rt = null;
		}
	}

	public Color method_20()
	{
		if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
		{
			return m_Color * m_Intensity;
		}
		return new Color(Mathf.GammaToLinearSpace(m_Color.r * m_Intensity), Mathf.GammaToLinearSpace(m_Color.g * m_Intensity), Mathf.GammaToLinearSpace(m_Color.b * m_Intensity), 1f);
	}

	public void method_21(int res)
	{
		if (renderTexture_0 != null && int_19 == Time.renderedFrameCount)
		{
			return;
		}
		if (camera_0 == null)
		{
			if (m_ShadowmapShader == null)
			{
				Debug.LogError("AreaLight's shadowmap shader not assigned.", this);
				return;
			}
			GameObject gameObject = new GameObject("Shadowmap Camera");
			gameObject.AddComponent(typeof(Camera));
			camera_0 = gameObject.GetComponent<Camera>();
			gameObject.hideFlags = HideFlags.HideAndDontSave;
			camera_0.enabled = false;
			camera_0.clearFlags = CameraClearFlags.Color;
			camera_0.renderingPath = RenderingPath.Forward;
			camera_0.backgroundColor = Color.white;
			transform_0 = gameObject.transform;
			transform_0.parent = base.transform;
			transform_0.localRotation = Quaternion.identity;
		}
		if (m_Angle == 0f)
		{
			camera_0.orthographic = true;
			transform_0.localPosition = Vector3.zero;
			camera_0.nearClipPlane = 0.05f;
			camera_0.farClipPlane = vector3_0.z;
			camera_0.orthographicSize = 0.5f * vector3_0.y;
			camera_0.aspect = vector3_0.x / vector3_0.y;
		}
		else
		{
			camera_0.orthographic = false;
			float nearToCenter = GetNearToCenter();
			transform_0.localPosition = (0f - nearToCenter) * Vector3.forward;
			camera_0.nearClipPlane = nearToCenter;
			camera_0.farClipPlane = nearToCenter + vector3_0.z;
			camera_0.fieldOfView = m_Angle;
			camera_0.aspect = vector3_0.x / vector3_0.y;
		}
		method_19(ref renderTexture_0);
		renderTexture_0 = RenderTexture.GetTemporary(res, res, 24, RenderTextureFormat.Shadowmap);
		renderTexture_0.name = "AreaLight Shadowmap";
		renderTexture_0.filterMode = FilterMode.Bilinear;
		renderTexture_0.wrapMode = TextureWrapMode.Clamp;
		camera_0.targetTexture = renderTexture_0;
		camera_0.cullingMask = 0;
		camera_0.Render();
		camera_0.cullingMask = m_ShadowCullingMask;
		bool invertCulling = GL.invertCulling;
		GL.invertCulling = false;
		camera_0.RenderWithShader(m_ShadowmapShader, "RenderType");
		GL.invertCulling = invertCulling;
		int_19 = Time.renderedFrameCount;
	}

	public RenderTexture GetBlurredShadowmap()
	{
		method_22();
		return renderTexture_1;
	}

	public void method_22()
	{
		if (renderTexture_1 != null && int_20 == Time.renderedFrameCount)
		{
			return;
		}
		method_28();
		int num = (int)m_ShadowmapRes;
		int num2 = (int)fogLight_0.m_ShadowmapRes;
		if (base.isActiveAndEnabled && m_Shadows)
		{
			num2 = Mathf.Min(num2, num / 2);
		}
		else
		{
			num = 2 * num;
		}
		method_21(num);
		RenderTexture active = RenderTexture.active;
		method_19(ref renderTexture_1);
		method_26(ref material_2, m_BlurShadowmapShader);
		int num3 = (int)Mathf.Log(num / num2, 2f);
		if (renderTexture_2 == null || renderTexture_2.Length != num3)
		{
			renderTexture_2 = new RenderTexture[num3];
		}
		RenderTextureFormat format = RenderTextureFormat.RGHalf;
		int i = 0;
		int num4 = num / 2;
		for (; i < num3; i++)
		{
			renderTexture_2[i] = RenderTexture.GetTemporary(num4, num4, 0, format, RenderTextureReadWrite.Linear);
			renderTexture_2[i].name = "AreaLight Shadow Downsample";
			renderTexture_2[i].filterMode = FilterMode.Bilinear;
			renderTexture_2[i].wrapMode = TextureWrapMode.Clamp;
			material_2.SetVector(int_21, new Vector4(0.5f / (float)num4, 0.5f / (float)num4, 0f, 0f));
			if (i == 0)
			{
				material_2.SetTexture(int_22, renderTexture_0);
				method_27();
				material_2.SetTexture(int_23, texture2D_3);
				material_2.SetVector(int_24, method_30());
				material_2.SetFloat(int_25, fogLight_0.m_ESMExponent);
				method_23(renderTexture_0, renderTexture_2[i], 0);
			}
			else
			{
				material_2.SetTexture(int_26, renderTexture_2[i - 1]);
				method_23(renderTexture_2[i - 1], renderTexture_2[i], 1);
			}
			num4 /= 2;
		}
		for (int j = 0; j < num3 - 1; j++)
		{
			RenderTexture.ReleaseTemporary(renderTexture_2[j]);
		}
		renderTexture_1 = renderTexture_2[num3 - 1];
		if (fogLight_0.m_BlurIterations > 0)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(num2, num2, 0, format, RenderTextureReadWrite.Linear);
			temporary.name = "AreaLight Shadow Blur";
			temporary.filterMode = FilterMode.Bilinear;
			temporary.wrapMode = TextureWrapMode.Clamp;
			material_2.SetVector(int_27, new Vector4(1f / (float)num2, 1f / (float)num2, 0f, 0f));
			float num5 = fogLight_0.m_BlurSize;
			for (int k = 0; k < fogLight_0.m_BlurIterations; k++)
			{
				material_2.SetFloat(int_28, num5);
				method_23(renderTexture_1, temporary, 2);
				method_23(temporary, renderTexture_1, 3);
				num5 *= 1.2f;
			}
			RenderTexture.ReleaseTemporary(temporary);
		}
		RenderTexture.active = active;
		int_20 = Time.renderedFrameCount;
	}

	public void method_23(RenderTexture src, RenderTexture dst, int pass)
	{
		RenderTexture.active = dst;
		material_2.SetTexture(int_26, src);
		material_2.SetPass(pass);
		method_24();
	}

	public void method_24()
	{
		GL.Begin(7);
		GL.TexCoord2(0f, 0f);
		GL.Vertex3(-1f, 1f, 0f);
		GL.TexCoord2(0f, 1f);
		GL.Vertex3(-1f, -1f, 0f);
		GL.TexCoord2(1f, 1f);
		GL.Vertex3(1f, -1f, 0f);
		GL.TexCoord2(1f, 0f);
		GL.Vertex3(1f, 1f, 0f);
		GL.End();
	}

	public void method_25(CommandBuffer buf)
	{
		method_21((int)m_ShadowmapRes);
		buf.SetGlobalTexture("_Shadowmap", renderTexture_0);
		method_27();
		material_1.SetTexture(int_23, texture2D_3);
		float num = (float)m_ShadowmapRes;
		float num2 = num / 2048f;
		buf.SetGlobalFloat("_ShadowReceiverWidth", num2 * m_ReceiverSearchDistance / num);
		buf.SetGlobalFloat("_ShadowReceiverDistanceScale", m_ReceiverDistanceScale * 0.5f / 10f);
		Vector2 vector = new Vector2(m_LightNearSize, m_LightFarSize) * num2 / num;
		buf.SetGlobalVector("_ShadowLightWidth", vector);
		buf.SetGlobalFloat("_ShadowBias", m_ShadowBias);
	}

	public void method_26(ref Material material, Shader shader)
	{
		if (!material)
		{
			if (!shader)
			{
				Debug.LogError("Missing shader");
				return;
			}
			material = new Material(shader);
			material.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	public void method_27()
	{
		if (!(texture2D_3 != null))
		{
			texture2D_3 = new Texture2D(1, 1, TextureFormat.Alpha8, mipChain: false, linear: true);
			texture2D_3.filterMode = FilterMode.Point;
			texture2D_3.SetPixel(0, 0, new Color(0f, 0f, 0f, 0f));
			texture2D_3.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		}
	}

	public void method_28()
	{
		if (!(fogLight_0 != null))
		{
			fogLight_0 = GetComponent<FogLight>();
		}
	}

	public bool method_29()
	{
		if (Camera.current == null)
		{
			return false;
		}
		RenderTexture targetTexture = Camera.current.targetTexture;
		if (targetTexture != null)
		{
			return targetTexture.format == RenderTextureFormat.Shadowmap;
		}
		return false;
	}

	public Vector4 method_30()
	{
		float nearToCenter = GetNearToCenter();
		float num = nearToCenter + vector3_0.z;
		return new Vector4(nearToCenter / (nearToCenter - num), (nearToCenter + num) / (nearToCenter - num), 0f, 0f);
	}
}
