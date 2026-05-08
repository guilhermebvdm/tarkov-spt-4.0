using System;
using UnityEngine;

[ExecuteInEditMode]
public class AnalyticSource : MonoBehaviour
{
	[GAttribute11("If you want to update settings or transform wile plaing, you should call UpdateSettins manually")]
	public bool IsAdditional;

	public float LightIntensity = 7f;

	[Range(0f, 8f)]
	public float LightIntensityMultiplicator = 1f;

	public float FadeNormals;

	public float AddAmbient;

	public Vector2 DoorSize = new Vector2(0.25f, 0.25f);

	public Vector3 CubeScale = new Vector4(1f, 1f, 1f);

	public Vector3 CubeShift = new Vector4(0f, 0f, 0f);

	public Vector3 FadeOut = new Vector3(0.25f, 0.25f, 0.25f);

	public Transform CubeControlHelper;

	public AmbientLight.CullingSettings Culling;

	public StencilShadow LinkedStencilShadow;

	[HideInInspector]
	public Bounds Bounds;

	[HideInInspector]
	public MaterialPropertyBlock MaterialPropertyBlock;

	[HideInInspector]
	public Matrix4x4 LocalToWorldMatrix;

	[HideInInspector]
	public int ShaderPath;

	public int DesiredDrawPriority = -1;

	[SerializeField]
	[HideInInspector]
	private int _drawPriority = -1;

	private static readonly int int_0 = Shader.PropertyToID("_CubeScale");

	private static readonly int int_1 = Shader.PropertyToID("_CubeShift");

	private static readonly int int_2 = Shader.PropertyToID("_DoorSize");

	private static readonly int int_3 = Shader.PropertyToID("_FadeOut");

	private static readonly int int_4 = Shader.PropertyToID("_FadeNormals");

	private static readonly int int_5 = Shader.PropertyToID("_LightIntensity");

	private static readonly int int_6 = Shader.PropertyToID("_LightIntensityMultiplicator");

	private static readonly int int_7 = Shader.PropertyToID("_AddAmbient");

	private static readonly int int_8 = Shader.PropertyToID("_CubeLightMinZ");

	[NonSerialized]
	public bool IsAutocullVisible;

	public static bool EnableAutoculling = true;

	public static bool EnableDraw = true;

	public int ActualDrawPriority => _drawPriority;

	public void Awake()
	{
		IsAutocullVisible = true;
		AmbientLight.AddAnalyticSource(this);
	}

	public virtual void Start()
	{
		UpdateSettings();
	}

	public virtual void OnEnable()
	{
		AmbientLight.AddAnalyticSource(this);
		UpdateSettings();
	}

	public virtual void OnDisable()
	{
		AmbientLight.RemoveAnalyticSource(this);
	}

	public void UpdateSettings()
	{
		if (CubeControlHelper != null)
		{
			CubeScale = CubeControlHelper.localScale * 0.5f;
			CubeShift = CubeControlHelper.localPosition;
			CubeControlHelper.localRotation = Quaternion.identity;
		}
		if (MaterialPropertyBlock == null)
		{
			MaterialPropertyBlock = new MaterialPropertyBlock();
		}
		MaterialPropertyBlock.SetVector(int_0, CubeScale);
		MaterialPropertyBlock.SetVector(int_1, CubeShift);
		MaterialPropertyBlock.SetVector(int_2, DoorSize);
		MaterialPropertyBlock.SetVector(int_3, new Vector4(1f / FadeOut.x, 1f / FadeOut.y, 1f / FadeOut.z));
		MaterialPropertyBlock.SetFloat(int_4, 1f / FadeNormals);
		MaterialPropertyBlock.SetFloat(int_5, LightIntensity);
		MaterialPropertyBlock.SetFloat(int_6, LightIntensityMultiplicator);
		MaterialPropertyBlock.SetFloat(int_7, AddAmbient);
		MaterialPropertyBlock.SetFloat(int_8, CubeShift.z - CubeScale.z);
		Vector3 vector = CubeScale * 2f + CubeShift;
		float num = Mathf.Max(vector.x, Mathf.Max(vector.y, vector.z)) * 0.5f;
		num += 0.2f;
		num = Mathf.Sqrt(num * num * 3f) * 2f;
		Bounds = new Bounds(base.transform.position, new Vector3(num, num, num));
		LocalToWorldMatrix = base.transform.localToWorldMatrix;
		if (Culling != null)
		{
			Culling.Update();
		}
	}
}
