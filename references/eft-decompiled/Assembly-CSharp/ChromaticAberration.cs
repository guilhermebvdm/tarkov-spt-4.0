using EFT.BlitDebug;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/ChromaticAberration")]
public class ChromaticAberration : MonoBehaviour
{
	public float Shift = 0.01f;

	public int Aniso = 3;

	public bool Simple;

	public Shader shader;

	private Material material_0;

	private static readonly int int_0 = Shader.PropertyToID("_Shift");

	private SSAAPropagator ssaapropagator_0;

	public Material Material_0
	{
		get
		{
			if (material_0 == null)
			{
				material_0 = new Material(shader)
				{
					hideFlags = HideFlags.HideAndDontSave
				};
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
		Material material = Material_0;
		material.SetVector(int_0, new Vector4(1f - Shift, 1f + Shift, 1f - Shift * 0.5f, 1f + Shift * 0.5f));
		source.anisoLevel = Aniso;
		source.filterMode = FilterMode.Bilinear;
		DebugGraphics.Blit(source, destination, material, (!Simple) ? 1 : 0);
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.ReleaseSourceDestination(source, destination);
		}
	}
}
