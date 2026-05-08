using UnityEngine;

[ExecuteInEditMode]
public class CullingAdvancedLightObject : CullingObject
{
	[SerializeField]
	private BaseLight _light;

	[SerializeField]
	private float _fadeStartDistance = 50f;

	[SerializeField]
	private float _fadeEndDistance = 80f;

	[SerializeField]
	private AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	[SerializeField]
	private float _maxLightIntensity = -10f;

	[SerializeField]
	private bool _isControlledByLampController;

	private float float_1;

	private float float_2 = 1f;

	private static int int_1;

	private int int_2;

	private int int_3;

	private const int int_4 = 20;

	private float float_3;

	private float float_4;

	private float float_5;

	public float IntensityMultiplier
	{
		set
		{
			float_2 = value;
		}
	}

	public override void Awake()
	{
		int_2 = int_1;
		int_1 = (int_1 + 1) % 20;
		base.Awake();
		float_3 = _fadeStartDistance * _fadeStartDistance;
		float_4 = _fadeEndDistance * _fadeEndDistance;
		float_5 = float_4 - float_3;
		float_2 = 1f;
		_light = GetComponentInChildren<BaseLight>(includeInactive: true);
		if (_light == null)
		{
			Debug.LogError("Cant find advanced light on " + base.gameObject.name + ". Turn culling advanced light object off");
			base.enabled = false;
			return;
		}
		if (!_isControlledByLampController)
		{
			_light.enabled = true;
		}
		float_1 = _maxLightIntensity;
		CullDistance = _fadeEndDistance;
	}

	public override void CustomUpdate()
	{
		int_3 = (int_3 + 1) % 20;
		if (int_3 == int_2)
		{
			base.CustomUpdate();
			method_1();
		}
	}

	public override void SetVisibility(bool isVisible)
	{
		if (base.IsVisible != isVisible)
		{
			base.SetVisibility(isVisible);
			method_1();
		}
	}

	public void method_1()
	{
		if (!(CullingManager.Instance == null))
		{
			float multiplier = CalculateMultiplayer();
			UpdateCullingObject(multiplier);
		}
	}

	public void UpdateCullingObject(float multiplier)
	{
		float num = ((CullingManager.Instance.IsOpticEnabled() & Mathf.Approximately(multiplier, 0f)) ? 1f : multiplier);
		num *= float_2;
		if (_light != null)
		{
			_light.m_Intensity = float_1 * num;
		}
	}

	public float CalculateMultiplayer()
	{
		float result = 0f;
		if (base.IsVisible)
		{
			float num = (CullingManager.Instance.GetCameraDistanceSqr(base.Index) - float_3) / float_5;
			if (num < 0f)
			{
				num = 0f;
			}
			else if (num > 1f)
			{
				num = 1f;
			}
			result = _fadeCurve.Evaluate(num);
		}
		return result;
	}

	public virtual void OnDisable()
	{
		float_2 = 1f;
		if (_light != null)
		{
			_light.m_Intensity = float_1;
		}
	}

	public override void OnDestroy()
	{
		float_2 = 1f;
		base.OnDestroy();
		if (_light != null)
		{
			_light.m_Intensity = float_1;
		}
	}
}
