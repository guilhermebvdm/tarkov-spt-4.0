using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class SnowFlakes : MonoBehaviour
{
	public struct Struct161
	{
		public Vector4 pos;

		public Vector3 dir;

		public Vector3 color;

		public Matrix4x4 worldToLight;
	}

	private const int int_0 = 2000;

	private const float float_0 = 0.001f;

	private const int int_1 = 16383;

	private const int int_2 = 8191;

	private const int int_3 = 4095;

	private static readonly int int_4 = Shader.PropertyToID("_FallingVector");

	private static readonly int int_5 = Shader.PropertyToID("_FlakesSize");

	private static readonly int int_6 = Shader.PropertyToID("_SideSpeed");

	[SerializeField]
	private Color DefaultColor = new Color(0.1f, 0.1f, 0.1f);

	[SerializeField]
	public Vector2 FlakesSizeMin = new Vector2(0.021f, 0.0251f);

	[SerializeField]
	public Vector2 FlakesSizeMax = new Vector2(0.041f, 0.0451f);

	[Range(0f, 1f)]
	public float FlakesFarSizeFactor = 1f;

	[Range(0f, 1f)]
	public float StormSideSpeed = 0.03f;

	[GAttribute6(0f, 1f, -1f)]
	public Vector2 MinMaxSideSpeed = new Vector2(0f, 1f);

	[Range(0f, 1f)]
	public float DropsAlphaClose = 0.116f;

	[Range(0f, 1f)]
	public float MinAmbient = 0.2f;

	[Range(0f, 1f)]
	public float MinAmbientAddition = 0.4f;

	[Range(0f, 1f)]
	public float MinAmbientAdditionCoef;

	public Material Close;

	public Material Far;

	private Material material_0;

	private Material material_1;

	private Material material_2;

	private Material material_3;

	private MeshRenderer[] meshRenderer_0;

	private MeshRenderer[] meshRenderer_1;

	private Light light_0;

	public ComputeBuffer _lightsPropertiesComputeBuffer;

	public float Intensity;

	[CompilerGenerated]
	private float float_1;

	private List<Struct161> list_0 = new List<Struct161>();

	public float StormFactor
	{
		[CompilerGenerated]
		get
		{
			return float_1;
		}
		[CompilerGenerated]
		set
		{
			float_1 = value;
		}
	}

	public void Start()
	{
		material_0 = GClass6.CopyToPreventMaterialChangeInEditor(Close);
		material_1 = GClass6.CopyToPreventMaterialChangeInEditor(Far);
		light_0 = GClass4.Instance.LightObject;
		meshRenderer_0 = new MeshRenderer[2]
		{
			method_5(16383, material_0, "close"),
			method_5(16383, material_1, "far")
		};
		material_2 = GClass6.CopyToPreventMaterialChangeInEditor(Close);
		material_3 = GClass6.CopyToPreventMaterialChangeInEditor(Far);
		bool flag = SystemInfo.systemMemorySize < 2000;
		meshRenderer_1 = new MeshRenderer[2]
		{
			method_5(flag ? 8191 : 16383, material_2, "close in storm"),
			method_5(flag ? 4095 : 16383, material_3, "far in storm")
		};
		Update();
	}

	public void Update()
	{
		if (meshRenderer_0 == null)
		{
			return;
		}
		method_2();
		bool flag = Intensity > 0.001f;
		for (int i = 0; i < meshRenderer_0.Length; i++)
		{
			MeshRenderer meshRenderer = meshRenderer_0[i];
			if (meshRenderer.enabled != flag)
			{
				meshRenderer.enabled = flag;
			}
		}
		flag = flag && StormFactor > 0.001f;
		for (int j = 0; j < meshRenderer_1.Length; j++)
		{
			MeshRenderer meshRenderer2 = meshRenderer_1[j];
			if (meshRenderer2.enabled != flag)
			{
				meshRenderer2.enabled = flag;
			}
		}
	}

	public void method_0()
	{
		Vector2 vector = Vector2.LerpUnclamped(FlakesSizeMin, FlakesSizeMax, Intensity);
		material_0.SetVector(int_5, vector);
		Vector2 vector2 = vector * FlakesFarSizeFactor;
		material_1.SetVector(int_5, vector2);
		material_2.SetVector(int_5, vector);
		material_3.SetVector(int_5, vector2);
	}

	public ComputeBuffer method_1<T>(List<T> data, int tailReserveCount)
	{
		if (data.Count == 0)
		{
			Debug.LogError("Zero data!");
			return null;
		}
		ComputeBuffer computeBuffer = new ComputeBuffer(data.Count + tailReserveCount, Marshal.SizeOf<T>());
		computeBuffer.SetData(data.ToArray(), 0, 0, data.Count);
		return computeBuffer;
	}

	public void method_2()
	{
		method_0();
		float num = Mathf.LerpUnclamped(MinMaxSideSpeed.x, MinMaxSideSpeed.y, Intensity);
		num += StormFactor * StormSideSpeed;
		int num2 = 0;
		method_3(ref list_0);
		if (_lightsPropertiesComputeBuffer == null)
		{
			_lightsPropertiesComputeBuffer = method_1(list_0, 0);
		}
		else
		{
			_lightsPropertiesComputeBuffer.SetData(list_0);
		}
		method_4(material_0, num, num2);
		method_4(material_1, num, num2);
		method_4(material_2, num, num2);
		method_4(material_3, num, num2);
	}

	public void method_3(ref List<Struct161> props)
	{
		if (light_0 == null)
		{
			if (props.Count <= 0)
			{
				Struct161 item = new Struct161
				{
					pos = Vector3.zero,
					dir = Vector3.down,
					color = (Vector4)DefaultColor,
					worldToLight = Matrix4x4.identity
				};
				props.Add(item);
			}
			return;
		}
		Struct161 @struct = default(Struct161);
		@struct.pos = light_0.transform.position;
		@struct.pos.w = 1f;
		@struct.dir = light_0.transform.forward;
		Vector3 vector = new Vector3(light_0.color.r, light_0.color.g, light_0.color.b);
		@struct.color = vector * light_0.intensity;
		@struct.worldToLight = light_0.transform.worldToLocalMatrix;
		if (props.Count > 0)
		{
			props[0] = @struct;
		}
		else
		{
			props.Add(@struct);
		}
	}

	public void method_4(Material material, float sideSpeed, float rainDensity)
	{
		material.SetFloat(int_6, sideSpeed);
		material.SetBuffer("LightsParameters", _lightsPropertiesComputeBuffer);
	}

	public MeshRenderer method_5(int particlesCount, Material material, string caption)
	{
		GameObject obj = new GameObject($"SnowFlakes {caption} ({particlesCount})");
		obj.transform.parent = base.transform;
		obj.AddComponent<MeshFilter>().mesh = Class688.smethod_0(particlesCount);
		MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
		meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		meshRenderer.material = material;
		meshRenderer.enabled = false;
		return meshRenderer;
	}
}
