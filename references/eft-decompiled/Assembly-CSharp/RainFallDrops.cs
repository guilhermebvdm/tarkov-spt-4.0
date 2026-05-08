using UnityEngine;
using UnityEngine.Rendering;

public class RainFallDrops : MonoBehaviour
{
	[SerializeField]
	[Range(0f, 1f)]
	private float _intensity = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _intensityThreshold = 0.001f;

	[Range(128f, 16383f)]
	public int Count = 8191;

	public Vector2 MinSize = new Vector2(0.04f, 0.15f);

	public Vector2 MaxSize = new Vector2(0.04f, 0.15f);

	[GAttribute6(0f, 1f, -1f)]
	public Vector2 MinMaxSideSpeed = new Vector2(0f, 1f);

	[GAttribute6(0f, 2f, -1f)]
	public Vector2 MinMaxDensity = new Vector2(0f, 1f);

	[SerializeField]
	private Material _close;

	[SerializeField]
	[Range(0f, 1f)]
	public float DropsAlphaClose = 0.116f;

	[SerializeField]
	[Range(0f, 1f)]
	public float MinAmbient = 0.2f;

	[SerializeField]
	[Range(0f, 1f)]
	public float MinAmbientAddition = 0.4f;

	[SerializeField]
	[Range(0f, 1f)]
	public float MinAmbientAdditionCoef;

	public float SideSpeed;

	private const int int_0 = 16383;

	private MeshRenderer meshRenderer_0;

	private GameObject gameObject_0;

	private Material material_0;

	private static readonly int int_1 = Shader.PropertyToID("_AlphaMult");

	private static readonly int int_2 = Shader.PropertyToID("_FallingVector");

	private static readonly int int_3 = Shader.PropertyToID("_Intensity");

	private static readonly int int_4 = Shader.PropertyToID("_SideSpeed");

	private static readonly int int_5 = Shader.PropertyToID("_RainDensity");

	private static readonly int int_6 = Shader.PropertyToID("_Size");

	private static readonly int int_7 = Shader.PropertyToID("_MinAmbient");

	public float Intensity
	{
		get
		{
			return _intensity;
		}
		set
		{
			_intensity = Mathf.Clamp01(value);
		}
	}

	public void Init()
	{
		if (material_0 == null)
		{
			material_0 = GClass6.CopyToPreventMaterialChangeInEditor(_close);
		}
		if (meshRenderer_0 == null)
		{
			meshRenderer_0 = method_1(Count, material_0);
		}
		GClass989.OnScreenChange += method_0;
		method_0();
	}

	public void method_0()
	{
		material_0.SetFloat(int_1, DropsAlphaClose);
	}

	public void Update()
	{
		if (!(meshRenderer_0 == null))
		{
			meshRenderer_0.enabled = Intensity > _intensityThreshold;
			method_2();
		}
	}

	public void OnDestroy()
	{
		if (gameObject_0 != null)
		{
			Object.Destroy(gameObject_0);
		}
	}

	public MeshRenderer method_1(int particlesCount, Material material)
	{
		gameObject_0 = new GameObject("RainFall(" + particlesCount + ")");
		gameObject_0.transform.parent = base.transform;
		gameObject_0.AddComponent<MeshFilter>().mesh = smethod_0(particlesCount);
		MeshRenderer meshRenderer = gameObject_0.AddComponent<MeshRenderer>();
		meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		meshRenderer.material = material;
		return meshRenderer;
	}

	public void method_2()
	{
		material_0.SetVector(int_2, RainController.FallingVectorV3);
		material_0.SetFloat(int_3, Intensity);
		material_0.SetFloat(int_4, SideSpeed * Mathf.LerpUnclamped(MinMaxSideSpeed.x, MinMaxSideSpeed.y, Intensity));
		material_0.SetFloat(int_5, Mathf.LerpUnclamped(MinMaxDensity.x, MinMaxDensity.y, Intensity));
		float x = (float)Screen.height / (float)Screen.width;
		Vector2 a = Vector2.LerpUnclamped(MinSize, MaxSize, Intensity);
		a = Vector2.Scale(a, new Vector2(x, 1f));
		material_0.SetVector(int_6, a);
		material_0.SetFloat(int_7, MinAmbient + MinAmbientAddition * MinAmbientAdditionCoef);
	}

	public static Mesh smethod_0(int count)
	{
		int num = count << 2;
		Vector3[] array = new Vector3[num];
		Vector4[] array2 = new Vector4[num];
		Vector3[] array3 = new Vector3[num];
		Vector2[] array4 = new Vector2[num];
		Vector2[] array5 = new Vector2[num];
		int[] array6 = new int[count * 6];
		int i = 0;
		int num2 = 0;
		for (; i < count; i++)
		{
			int num3 = i << 2;
			array6[num2++] = num3;
			array6[num2++] = num3 + 1;
			array6[num2++] = num3 + 2;
			array6[num2++] = num3 + 2;
			array6[num2++] = num3 + 3;
			array6[num2++] = num3;
		}
		int j = 0;
		int num4 = 0;
		for (; j < count; j++)
		{
			int num5 = num4++;
			int num6 = num4++;
			int num7 = num4++;
			int num8 = num4++;
			array4[num5] = new Vector2(0f, 1f);
			array4[num6] = new Vector2(0f, 0f);
			array4[num7] = new Vector2(1f, 0f);
			array4[num8] = new Vector2(1f, 1f);
			float num9 = Random.Range(0.1f, 0.6f);
			float num10 = Random.Range(num9, 1f);
			array5[num5] = new Vector2(0f - num9, num10);
			array5[num6] = new Vector2(0f - num9, 0f - num10);
			array5[num7] = new Vector2(num9, 0f - num10);
			array5[num8] = new Vector2(num9, num10);
			array[num5] = (array[num6] = (array[num7] = (array[num8] = new Vector3(Random.value, Random.value, Random.value))));
			array2[num5] = (array2[num6] = (array2[num7] = (array2[num8] = new Vector4((Random.value - 0.5f) * 100f, (Random.value - 0.5f) * 100f, (Random.value - 0.5f) * 100f, (Random.value - 0.5f) * 100f))));
			array3[num5] = (array3[num6] = (array3[num7] = (array3[num8] = Vector3.zero)));
		}
		return new Mesh
		{
			vertices = array,
			normals = array3,
			tangents = array2,
			uv = array4,
			uv2 = array5,
			triangles = array6,
			bounds = new Bounds(Vector3.zero, Vector3.one * float.MaxValue),
			name = "RainFallDrops GetMesh"
		};
	}
}
