using UnityEngine;

public class FrostbiteEffect : MonoBehaviour
{
	private const string string_0 = "Hidden/FrostbiteEffect";

	private static readonly int int_0 = Shader.PropertyToID("_BaseColor");

	private static readonly int int_1 = Shader.PropertyToID("_BaseColorMap");

	private static readonly int int_2 = Shader.PropertyToID("_NormalMap");

	private static readonly int int_3 = Shader.PropertyToID("_Tiling");

	private static readonly int int_4 = Shader.PropertyToID("_ODRF");

	private static readonly int int_5 = Shader.PropertyToID("_ShapeRadius");

	private SSAAPropagator ssaapropagator_0;

	private Material material_0;

	[SerializeField]
	private Shader _shader;

	[SerializeField]
	private Color _baseColor = new Color(0.1607843f, 0.282353f, 0.4352941f, 0.6f);

	[SerializeField]
	private Texture2D _baseColorMap;

	[SerializeField]
	private Texture2D _normalMap;

	[SerializeField]
	private Vector2 _tiling = Vector2.one;

	[SerializeField]
	[Range(0f, 1f)]
	private float _speed = 0.15f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _opacity = 0.75f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _distortion = 0.25f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _radius = 1f;

	[SerializeField]
	private Vector2 _shapeRadius = new Vector2(0.8f, 1f);

	[SerializeField]
	[Range(0f, 5f)]
	private float _falloff = 1.5f;

	public Material Material_0
	{
		get
		{
			if (_shader == null)
			{
				_shader = Shader.Find("Hidden/FrostbiteEffect");
			}
			if (material_0 == null)
			{
				material_0 = new Material(_shader)
				{
					hideFlags = HideFlags.HideAndDontSave
				};
			}
			return material_0;
		}
	}

	public float Radius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = value;
		}
	}

	public float Speed => _speed;

	public void Awake()
	{
		ssaapropagator_0 = GetComponent<SSAAPropagator>();
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.GetSourceDestination(out source, out destination);
		}
		if (!(_opacity <= 0f) && _radius < 1f)
		{
			Material_0.SetColor(int_0, _baseColor);
			Material_0.SetTexture(int_1, _baseColorMap);
			Material_0.SetTexture(int_2, _normalMap);
			Material_0.SetVector(int_4, new Vector4(_opacity, _distortion, _radius, _falloff));
			Material_0.SetVector(int_3, _tiling);
			Material_0.SetVector(int_5, _shapeRadius);
			Graphics.Blit(source, destination, Material_0);
			if (ssaapropagator_0 != null)
			{
				ssaapropagator_0.ReleaseSourceDestination(source, destination);
			}
		}
		else
		{
			GClass860.BlitOrCopy(source, destination);
		}
	}

	public void OnDestroy()
	{
		if (base.gameObject.activeInHierarchy && (bool)material_0)
		{
			GClass972.SafeDestroy(material_0);
		}
	}
}
