using System.Runtime.CompilerServices;
using UnityEngine;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/ScreenWater")]
public class ScreenWater : MonoBehaviour
{
	[CompilerGenerated]
	private float float_0;

	public Shader shaderRGB;

	public float _distortionCooldownTime = 2f;

	public Texture2D WaterFlows;

	public Texture2D WaterMask;

	public Texture2D WetScreen;

	public float Speed = 0.5f;

	public float InitialIntensity = 0.1f;

	private Material material_0;

	private float float_1;

	private SSAAPropagator ssaapropagator_0;

	private bool bool_0;

	private static readonly int int_0 = Shader.PropertyToID("_Splat");

	private static readonly int int_1 = Shader.PropertyToID("_Flow");

	private static readonly int int_2 = Shader.PropertyToID("_Water");

	private static readonly int int_3 = Shader.PropertyToID("_Speed");

	private static readonly int int_4 = Shader.PropertyToID("_Intens");

	public float Intensity
	{
		[CompilerGenerated]
		get
		{
			return float_0;
		}
		[CompilerGenerated]
		set
		{
			float_0 = value;
		}
	}

	public Material material
	{
		get
		{
			if (material_0 == null)
			{
				material_0 = new Material(shaderRGB);
				material_0.hideFlags = HideFlags.HideAndDontSave;
			}
			return material_0;
		}
	}

	public void Awake()
	{
		ssaapropagator_0 = GetComponent<SSAAPropagator>();
	}

	public void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
		}
		else if (shaderRGB == null)
		{
			Debug.Log("Sat shaders are not set up! Disabling saturation effect.");
			base.enabled = false;
		}
		else if (!shaderRGB.isSupported)
		{
			base.enabled = false;
		}
	}

	public void OnDestroy()
	{
		if ((bool)material_0)
		{
			Object.DestroyImmediate(material_0);
		}
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.GetSourceDestination(out source, out destination);
		}
		if (!(Intensity < Mathf.Epsilon) && bool_0)
		{
			if (RainController.IsCameraUnderRain)
			{
				float_1 += Time.deltaTime / _distortionCooldownTime;
			}
			else
			{
				float_1 -= Time.deltaTime / _distortionCooldownTime;
			}
			float_1 = Mathf.Clamp01(float_1);
			Material material = this.material;
			material.SetTexture(int_0, WaterFlows);
			material.SetTexture(int_1, WaterMask);
			material.SetTexture(int_2, WetScreen);
			material.SetFloat(int_3, Speed);
			material.SetFloat(int_4, InitialIntensity * Intensity * float_1);
			Graphics.Blit(source, destination, material);
			if (ssaapropagator_0 != null)
			{
				ssaapropagator_0.ReleaseSourceDestination(source, destination);
			}
		}
		else
		{
			GClass860.BlitOrCopy(source, destination);
			if (ssaapropagator_0 != null)
			{
				ssaapropagator_0.ReleaseSourceDestination(source, destination);
			}
		}
	}

	public void SetIntensity(float intensity)
	{
		Intensity = intensity;
		base.enabled = intensity > 0f;
	}

	public void ChangeGlassesState(bool hasGlases)
	{
		bool_0 = hasGlases;
	}
}
