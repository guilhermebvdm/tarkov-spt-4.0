using UnityEngine;

namespace Systems.Effects;

[RequireComponent(typeof(Camera))]
public class ScreenColorBlender : MonoBehaviour
{
	[SerializeField]
	private Shader _shader;

	[SerializeField]
	private Color _color1;

	[SerializeField]
	private Color _color2;

	[SerializeField]
	private AnimationCurve _curve;

	[Range(0f, 12f)]
	[SerializeField]
	private int _downsamplingValue;

	[SerializeField]
	private bool _useExistingTexture;

	[SerializeField]
	private Texture2D _blendTexture;

	[Range(0f, 1f)]
	[SerializeField]
	private float _blendValue;

	private Material material_0;

	private Texture texture_0;

	private static readonly int int_0 = Shader.PropertyToID("_BlendValue");

	private static readonly int int_1 = Shader.PropertyToID("_BlendTex");

	public void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
			return;
		}
		if (_shader == null)
		{
			Debug.Log("Screenspace color blender shader are not set up! Disabling screen space color blending.");
			base.enabled = false;
		}
		else if (!_shader.isSupported)
		{
			base.enabled = false;
		}
		material_0 = new Material(_shader)
		{
			hideFlags = HideFlags.HideAndDontSave
		};
		GenerateTextureAndReassignMaterial();
	}

	public void OnDestroy()
	{
		if (material_0 != null)
		{
			Object.DestroyImmediate(material_0);
		}
		if (!_useExistingTexture)
		{
			_blendTexture = null;
		}
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destanation)
	{
		material_0.SetFloat(int_0, Mathf.Clamp01(_blendValue));
		Graphics.Blit(source, destanation, material_0);
	}

	public void GenerateTextureAndReassignMaterial()
	{
		if (material_0 == null)
		{
			material_0 = new Material(_shader)
			{
				hideFlags = HideFlags.HideAndDontSave
			};
		}
		if (!_useExistingTexture)
		{
			int num = (int)Mathf.Pow(2f, _downsamplingValue);
			texture_0 = GClass1022.GetRadialTexture(Screen.width / num, Screen.height / num, _color1, _color2, _curve);
		}
		else
		{
			texture_0 = _blendTexture;
		}
		material_0.SetTexture(int_1, texture_0);
	}
}
