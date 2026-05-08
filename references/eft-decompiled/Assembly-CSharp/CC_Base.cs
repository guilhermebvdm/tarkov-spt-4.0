using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("")]
public class CC_Base : MonoBehaviour
{
	public Shader shader;

	protected Material _material;

	protected SSAAPropagator _ssaaPropagator;

	public Material material
	{
		get
		{
			if (_material == null)
			{
				_material = new Material(shader);
				_material.hideFlags = HideFlags.HideAndDontSave;
			}
			return _material;
		}
	}

	public static bool IsLinear()
	{
		return QualitySettings.activeColorSpace == ColorSpace.Linear;
	}

	public virtual void Awake()
	{
		_ssaaPropagator = GetComponent<SSAAPropagator>();
	}

	public virtual void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
		}
		else if (!shader || !shader.isSupported)
		{
			base.enabled = false;
		}
	}

	public virtual void OnDisable()
	{
		if (base.gameObject.activeInHierarchy && (bool)_material)
		{
			Object.DestroyImmediate(_material);
		}
	}
}
