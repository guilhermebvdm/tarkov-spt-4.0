using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/TopLight")]
public class TopLight : MonoBehaviour
{
	public Shader TheShader;

	public Color Color;

	private Material material_0;

	private Color color_0;

	private static readonly int int_0 = Shader.PropertyToID("_Color");

	public Material Material_0
	{
		get
		{
			if (material_0 == null)
			{
				material_0 = new Material(TheShader);
				material_0.hideFlags = HideFlags.HideAndDontSave;
				material_0.SetColor(int_0, Color);
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
		else if (TheShader == null)
		{
			string text = GetType().Name;
			Debug.Log(text + " shaders are not set up! Disabling " + text + " effect.");
			base.enabled = false;
		}
		else if (!TheShader.isSupported)
		{
			string text2 = GetType().Name;
			Debug.Log(text2 + " shaders are not supported! Disabling " + text2 + " effect.");
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

	[ImageEffectOpaque]
	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Material material = Material_0;
		if (color_0 != Color)
		{
			color_0 = Color;
			material.SetColor(int_0, Color);
		}
		source.anisoLevel = 0;
		source.filterMode = FilterMode.Point;
		Graphics.Blit(source, destination, material);
	}
}
