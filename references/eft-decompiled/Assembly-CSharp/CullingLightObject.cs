using EFT.Visual;
using UnityEngine;

[ExecuteInEditMode]
public class CullingLightObject : CullingObject
{
	[SerializeField]
	private Light _light;

	[SerializeField]
	private float _fadeStartDistance = 50f;

	[SerializeField]
	private float _fadeEndDistance = 80f;

	[SerializeField]
	private AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	[SerializeField]
	private bool _useLightIntensityFromEditor = true;

	[SerializeField]
	[Range(0f, 8f)]
	private float _maxLightIntensity = 5f;

	[SerializeField]
	private bool _takeFlareParametersFromThisScript;

	private GUIStyle guistyle_0 = new GUIStyle();

	[SerializeField]
	private bool _cullShadowsByDistance = true;

	[Header("Shadows")]
	[SerializeField]
	private float _shadowsFadeStartDistance = 12f;

	[SerializeField]
	private float _shadowsFadeEndDistance = 17f;

	private float float_1;

	private bool bool_2 = true;

	private float float_2 = 1f;

	private static int int_1;

	private int int_2;

	private int int_3;

	private const int int_4 = 20;

	private float float_3;

	private float float_4;

	private float float_5;

	private LightFlicker lightFlicker_0;

	private VolumetricLight volumetricLight_0;

	private float float_6;

	private float float_7;

	private float float_8;

	private float float_9 = -1f;

	private float float_10 = -1f;

	private float float_11 = -1f;

	private bool bool_3 = true;

	[SerializeField]
	private LightShadows _initialShadowsMode;

	public float IntensityMultiplier
	{
		get
		{
			return float_2;
		}
		set
		{
			float_2 = value;
		}
	}

	public float CurrentLightIntensity => _maxLightIntensity * float_2;

	public bool IsLightEnabled => bool_2;

	public bool TakeFlareParametersFromCullingLight => _takeFlareParametersFromThisScript;

	public bool CullShadowsByDistance => _cullShadowsByDistance;

	public override void Awake()
	{
		int_2 = int_1;
		int_1 = (int_1 + 1) % 20;
		base.Awake();
		method_3();
		method_6();
		float_2 = 1f;
		_light = GetComponent<Light>();
		if (_light == null)
		{
			Debug.LogError("Cant find light on " + base.gameObject.name + ". Turn culling light object off");
			base.enabled = false;
			return;
		}
		_light.enabled = true;
		lightFlicker_0 = GetComponent<LightFlicker>();
		volumetricLight_0 = GetComponent<VolumetricLight>();
		float_1 = (_useLightIntensityFromEditor ? _maxLightIntensity : _light.intensity);
		if (_initialShadowsMode == LightShadows.None)
		{
			_initialShadowsMode = _light.shadows;
		}
		CullDistance = _fadeEndDistance;
		if (_light.type == LightType.Point)
		{
			base.Radius = _light.range;
		}
		if (!CullingManager.Instance)
		{
			CullingManager.OnInstanceCreated += method_1;
		}
		CullingManager.OnInstanceDestroyed += method_2;
	}

	public void method_1()
	{
		CullingManager.OnInstanceCreated -= method_1;
	}

	public void method_2()
	{
		CullingManager.OnInstanceDestroyed -= method_2;
	}

	public float getMaxIntensityFinal()
	{
		return float_1;
	}

	public LightFlicker GetFlicker()
	{
		return lightFlicker_0;
	}

	public Light GetLight()
	{
		return _light;
	}

	public bool GetCullShadowsByDistance()
	{
		return _cullShadowsByDistance;
	}

	public LightShadows GetInitialShadowsMode()
	{
		return _initialShadowsMode;
	}

	public float GetFadeStartDistance()
	{
		return _fadeStartDistance;
	}

	public float GetFadEndDistance()
	{
		return _fadeEndDistance;
	}

	public void method_3()
	{
		float_3 = _fadeStartDistance * _fadeStartDistance;
		float_4 = _fadeEndDistance * _fadeEndDistance;
		float_5 = float_4 - float_3;
	}

	public override void SetVisibility(bool isVisible)
	{
		if (_isVisible != isVisible)
		{
			base.SetVisibility(isVisible);
			method_4();
		}
	}

	public override void CustomUpdate()
	{
		if ((bool)CullingManager.Instance)
		{
			int_3 = (int_3 + 1) % 20;
			if (int_3 == int_2)
			{
				base.CustomUpdate();
				method_4();
			}
		}
	}

	public void method_4()
	{
		if (!CullingManager.Instance)
		{
			return;
		}
		float num = method_5();
		float num2 = CalculateShadowMultiplier();
		if (float_10 == num && float_9 == num2 && float_11 == float_2)
		{
			if (bool_3 && num > 0f && _light.intensity <= 0f)
			{
				UpdateCullingObject(num, num2);
				bool_3 = false;
			}
		}
		else
		{
			float_9 = num2;
			float_10 = num;
			float_11 = float_2;
			UpdateCullingObject(num, num2);
		}
	}

	public virtual void UpdateCullingObject(float multiplier, float shadowMultiplier)
	{
		float num = ((CullingManager.Instance.IsOpticEnabled() & Mathf.Approximately(multiplier, 0f)) ? 1f : multiplier);
		num *= float_2;
		if (lightFlicker_0 != null && lightFlicker_0.enabled)
		{
			lightFlicker_0.CullingCoef = num;
		}
		else
		{
			_light.intensity = float_1 * num;
		}
		if (volumetricLight_0 != null && volumetricLight_0.isActiveAndEnabled)
		{
			volumetricLight_0.CheckIntensity();
		}
		if (_cullShadowsByDistance)
		{
			_light.shadowStrength = shadowMultiplier;
			bool flag;
			if ((flag = Mathf.Approximately(shadowMultiplier, 0f)) && _light.shadows == _initialShadowsMode)
			{
				_light.shadows = LightShadows.None;
			}
			else if (!flag && _light.shadows == LightShadows.None)
			{
				_light.shadows = _initialShadowsMode;
			}
		}
	}

	public float method_5()
	{
		float result = 0f;
		if (_isVisible)
		{
			float time = Mathf.Clamp01((CullingManager.Instance.GetCameraDistanceSqr(base.Index) - float_3) / float_5);
			result = _fadeCurve.Evaluate(time);
		}
		return result;
	}

	public float CalculateShadowMultiplier()
	{
		if (!_cullShadowsByDistance)
		{
			return 1f;
		}
		float result = 0f;
		if (_isVisible)
		{
			float time = Mathf.Clamp01((CullingManager.Instance.GetCameraDistanceSqr(base.Index) - float_6) / float_8);
			result = _fadeCurve.Evaluate(time);
		}
		return result;
	}

	public void method_6()
	{
		float_6 = _shadowsFadeStartDistance * _shadowsFadeStartDistance;
		float_7 = _shadowsFadeEndDistance * _shadowsFadeEndDistance;
		float_8 = float_7 - float_6;
	}

	public void Switch(bool enable)
	{
		bool_2 = enable;
	}

	public virtual void OnDisable()
	{
		float_2 = 1f;
		float_9 = -1f;
		float_10 = -1f;
		float_11 = -1f;
		bool_3 = true;
		if (_light != null)
		{
			_light.intensity = float_1;
			_light.shadows = _initialShadowsMode;
			_light.shadowStrength = 1f;
		}
	}

	public override void OnDestroy()
	{
		CullingManager.OnInstanceCreated -= method_1;
		CullingManager.OnInstanceDestroyed -= method_2;
		float_2 = 1f;
		base.OnDestroy();
		if (_light != null)
		{
			_light.intensity = float_1;
			_light.shadows = _initialShadowsMode;
			_light.shadowStrength = 1f;
		}
	}

	public Light GetEditorLight()
	{
		return _light;
	}
}
