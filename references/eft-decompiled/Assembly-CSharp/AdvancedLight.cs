using System;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class AdvancedLight : MonoBehaviour, GInterface51
{
	public enum LightTypeEnum
	{
		Point,
		Directional
	}

	public enum ShadingTypeEnum
	{
		Diffuse,
		Specular,
		DiffuseAndSpecular
	}

	public Shader Shader;

	public LightTypeEnum LightType;

	public Mesh Mesh;

	public int SubMesh;

	public Transform LightPosition;

	[ColorUsage(false, true)]
	public Color Color;

	public float Radius;

	public ShadingTypeEnum ShadingType;

	public bool ShowDebugShapes;

	public Transform CubeControlHelper;

	public bool FullScreen;

	[HideInInspector]
	public Vector3 CubeScale = vector3_0;

	[HideInInspector]
	public Vector3 CubeShift = vector3_1;

	private Material material_0;

	private Transform transform_0;

	private Transform transform_1;

	private int int_0;

	private int int_1;

	private int int_2;

	private int int_3;

	private int int_4;

	private int int_5;

	private float float_0;

	private static readonly Vector3 vector3_0 = Vector3.one;

	private static readonly Vector3 vector3_1 = Vector3.zero;

	private static readonly int int_6 = Shader.PropertyToID("_Color");

	private static readonly int int_7 = Shader.PropertyToID("_InvSqRadius");

	private static readonly int int_8 = Shader.PropertyToID("_FullScreen");

	private static readonly int int_9 = Shader.PropertyToID("_CubeHelperScale");

	private static readonly int int_10 = Shader.PropertyToID("_CubeHelperShift");

	public void method_0()
	{
		if (Shader == null)
		{
			throw new NullReferenceException(base.name + "(AdvancedLight).Shader is null");
		}
		if (Mesh == null)
		{
			throw new NullReferenceException(base.name + "(AdvancedLight).Mesh is null");
		}
		if (material_0 == null)
		{
			material_0 = new Material(Shader);
		}
		material_0.SetColor(int_6, Color);
		Radius = Mathf.Max(Radius, 0f);
		material_0.SetFloat(int_7, 1f / (Radius * Radius));
		UpdateKeywords(material_0, ShadingType);
		UpdateKeywords(material_0, LightType);
		material_0.SetFloat(int_8, FullScreen ? 1 : 0);
		transform_0 = base.transform;
		transform_1 = ((LightPosition != null) ? LightPosition : transform_0);
		int_0 = Shader.PropertyToID("_LightPosition");
		int_1 = Shader.PropertyToID("_LightDirection");
		int_2 = Shader.PropertyToID("_SrcBlend");
		int_3 = Shader.PropertyToID("_DstBlend");
		int_4 = Shader.PropertyToID("_Color");
		int_5 = Shader.PropertyToID("_InvSqRadius");
		method_1();
	}

	public void Update()
	{
		material_0.SetColor(int_4, Color);
		if (float_0 != Radius)
		{
			float_0 = Radius;
			Radius = Mathf.Max(Radius, 0f);
			material_0.SetFloat(int_5, 1f / (Radius * Radius));
		}
		method_1();
	}

	public void method_1()
	{
		if (CubeControlHelper != null && !FullScreen)
		{
			CubeScale = CubeControlHelper.localScale;
			CubeShift = CubeControlHelper.localPosition;
			CubeControlHelper.localRotation = Quaternion.identity;
		}
		else
		{
			CubeScale = vector3_0;
			CubeShift = vector3_1;
		}
		material_0.SetVector(int_9, CubeScale);
		material_0.SetVector(int_10, CubeShift);
	}

	public static void UpdateKeywords(Material material, Enum enumValue)
	{
		string[] names = Enum.GetNames(enumValue.GetType());
		string text = enumValue.ToString();
		string[] array = names;
		foreach (string text2 in array)
		{
			if (text2 == text)
			{
				material.EnableKeyword(text2.ToUpper());
			}
			else
			{
				material.DisableKeyword(text2.ToUpper());
			}
		}
	}

	public void OnValidate()
	{
		method_0();
	}

	public void Start()
	{
		method_0();
		LightManager.Add(this);
	}

	public void OnEnable()
	{
		method_0();
		LightManager.Add(this);
	}

	public void OnDisable()
	{
		LightManager.Remove(this);
	}

	public void OnDestroy()
	{
		OnDisable();
	}

	public void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.transform.position, GClass842.True ? "AreaLight Gizmo" : "PointLight Gizmo", allowScaling: true);
	}

	public void OnDrawGizmosSelected()
	{
		if (ShowDebugShapes)
		{
			Gizmos.DrawWireMesh(Mesh, SubMesh, transform_0.position, transform_0.rotation, transform_0.lossyScale);
			Gizmos.DrawWireSphere(transform_1.position, Radius);
			Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
			Gizmos.DrawMesh(Mesh, SubMesh, transform_0.position, transform_0.rotation, transform_0.lossyScale);
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(CubeShift, CubeScale);
		}
	}

	public void Draw(CommandBuffer buffer, Plane[] frustrumPlanes, Camera cam)
	{
		Bounds bounds = Mesh.bounds;
		bounds.extents = Vector3.Scale(bounds.extents, base.transform.localScale);
		bounds.center += base.transform.position;
		if (GeometryUtility.TestPlanesAABB(frustrumPlanes, bounds) || FullScreen)
		{
			BlendMode blendMode = (cam.allowHDR ? BlendMode.One : BlendMode.DstColor);
			BlendMode blendMode2 = (cam.allowHDR ? BlendMode.One : BlendMode.Zero);
			buffer.SetGlobalFloat(int_2, (float)blendMode);
			buffer.SetGlobalFloat(int_3, (float)blendMode2);
			material_0.SetVector(int_0, transform_1.position);
			material_0.SetVector(int_1, transform_1.forward);
			buffer.DrawMesh(Mesh, base.transform.localToWorldMatrix, material_0, SubMesh, 0);
		}
	}
}
