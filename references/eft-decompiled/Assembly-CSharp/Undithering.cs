using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Undithering")]
public class Undithering : MonoBehaviour
{
	public Shader shader;

	public bool useTriangleBlit = true;

	[SerializeField]
	private bool _distortion;

	private bool bool_0;

	private Material material_0;

	private MaterialPropertyBlock materialPropertyBlock_0;

	private static readonly int int_0 = Shader.PropertyToID("_Distortion");

	private static readonly int int_1 = Shader.PropertyToID("_Scatter");

	private CommandBuffer commandBuffer_0;

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
		commandBuffer_0 = new CommandBuffer();
		commandBuffer_0.name = "Undithering";
	}

	public void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
			return;
		}
		if (shader == null)
		{
			Debug.Log("Shaders are not set up! Disabling undither effect.");
			base.enabled = false;
		}
		else if (!shader.isSupported)
		{
			base.enabled = false;
		}
		TOD_Scattering component = base.gameObject.GetComponent<TOD_Scattering>();
		bool_0 = component != null;
	}

	public void OnDestroy()
	{
		if ((bool)material_0)
		{
			Object.DestroyImmediate(material_0);
		}
		commandBuffer_0?.Dispose();
		commandBuffer_0 = null;
	}

	[ImageEffectOpaque]
	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Material_0.SetFloat(int_0, _distortion ? 1f : 0f);
		Material_0.SetFloat(int_1, bool_0 ? 1f : 0f);
		if (useTriangleBlit)
		{
			if (materialPropertyBlock_0 == null)
			{
				materialPropertyBlock_0 = new MaterialPropertyBlock();
			}
			commandBuffer_0.Clear();
			commandBuffer_0.BlitFullscreenTriangle(source, destination, Material_0, 0, materialPropertyBlock_0);
			Graphics.ExecuteCommandBuffer(commandBuffer_0);
		}
		else
		{
			Graphics.Blit(source, destination, Material_0);
		}
	}
}
