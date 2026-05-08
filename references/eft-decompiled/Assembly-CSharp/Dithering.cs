using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Dithering")]
public class Dithering : MonoBehaviour
{
	public Texture DitTex;

	public Shader shader;

	private Material material_0;

	private static readonly int int_0 = Shader.PropertyToID("_DitheringTexture");

	public Material Material_0
	{
		get
		{
			if (material_0 == null)
			{
				material_0 = new Material(shader);
				material_0.hideFlags = HideFlags.HideAndDontSave;
			}
			return material_0;
		}
	}

	public void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
		}
		else if (shader == null)
		{
			Debug.Log("Noise shaders are not set up! Disabling noise effect.");
			base.enabled = false;
		}
		else if (!shader.isSupported)
		{
			base.enabled = false;
		}
	}

	public void OnDisable()
	{
		if ((bool)material_0)
		{
			Object.DestroyImmediate(material_0);
		}
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Material material = Material_0;
		material.SetTexture(int_0, DitTex);
		Graphics.Blit(source, destination, material);
	}
}
