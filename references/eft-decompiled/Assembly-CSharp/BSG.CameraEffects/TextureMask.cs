using System.Collections.Generic;
using UnityEngine;

namespace BSG.CameraEffects;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/TextureMask")]
public class TextureMask : MonoBehaviour
{
	private const string string_0 = "_STRETCH";

	public Shader Shader;

	public Color Color;

	public Texture Mask;

	public bool Stretch;

	public float Size = 1f;

	private readonly HashSet<GInterface57> hashSet_0 = new HashSet<GInterface57>();

	private Material material_0;

	private Camera camera_0;

	private SSAA ssaa_0;

	private SSAAPropagator ssaapropagator_0;

	private static readonly int int_0 = Shader.PropertyToID("_Color");

	private static readonly int int_1 = Shader.PropertyToID("_Mask");

	private static readonly int int_2 = Shader.PropertyToID("_InvMaskSize");

	private static readonly int int_3 = Shader.PropertyToID("_InvAspect");

	private static readonly int int_4 = Shader.PropertyToID("_CameraAspect");

	public void Awake()
	{
		TryGetComponent<Camera>(out camera_0);
		TryGetComponent<SSAA>(out ssaa_0);
		ssaapropagator_0 = GetComponent<SSAAPropagator>();
	}

	public void OnEnable()
	{
		ApplySettings();
	}

	public void OnValidate()
	{
		if (camera_0 == null)
		{
			TryGetComponent<Camera>(out camera_0);
		}
		if (ssaa_0 == null)
		{
			TryGetComponent<SSAA>(out ssaa_0);
		}
		ApplySettings();
	}

	public Material method_0()
	{
		if (!(material_0 == null))
		{
			return material_0;
		}
		return material_0 = new Material(Shader);
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.GetSourceDestination(out source, out destination);
		}
		Graphics.Blit(source, destination, method_0());
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.ReleaseSourceDestination(source, destination);
		}
	}

	public void ApplySettings()
	{
		Material material = method_0();
		material.SetColor(int_0, Color);
		material.SetTexture(int_1, Mask);
		float value = 1f / Size;
		material.SetFloat(int_2, value);
		float value2 = ((Mask != null) ? ((float)Mask.height / (float)Mask.width) : 1f);
		material.SetFloat(int_3, value2);
		if (camera_0 == null)
		{
			TryGetComponent<Camera>(out camera_0);
		}
		float value3 = ((camera_0 != null) ? camera_0.aspect : ((float)(Screen.width / Screen.height)));
		material.SetFloat(int_4, value3);
		if (Stretch)
		{
			material.EnableKeyword("_STRETCH");
		}
		else
		{
			material.DisableKeyword("_STRETCH");
		}
	}

	public void TryToEnable(GInterface57 textureMaskHolder, bool on)
	{
		if (on)
		{
			hashSet_0.Add(textureMaskHolder);
		}
		else
		{
			hashSet_0.Remove(textureMaskHolder);
		}
		base.enabled = hashSet_0.Count > 0;
	}
}
