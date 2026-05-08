using System.Collections;
using UnityEngine;

public class LightPillarEffect : HalloweenEffect
{
	private static readonly int int_0 = Shader.PropertyToID("_HeightTopYFade");

	private static readonly int int_1 = Shader.PropertyToID("_DistanceFade");

	private MaterialPropertyBlock materialPropertyBlock_0;

	private MaterialPropertyBlock materialPropertyBlock_1;

	private Vector3 vector3_0;

	private bool bool_0;

	private bool bool_1;

	[SerializeField]
	private MeshRenderer _renderer;

	[SerializeField]
	private Vector2 _minMaxDistanceToFade = new Vector2(20f, 30f);

	[SerializeField]
	private AnimationCurve _lightPillarFade = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[Space]
	[SerializeField]
	private MeshRenderer _rendererGlow;

	[SerializeField]
	private Vector2 _minMaxDistanceGlowToFade = new Vector2(20f, 30f);

	[SerializeField]
	private AnimationCurve _lightPillarGlowFade = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[Space]
	[SerializeField]
	[Range(0f, 1f)]
	private float _heightFade;

	[SerializeField]
	[Range(0f, 1f)]
	private float _speed = 0.75f;

	[SerializeField]
	private ParticleSystem _fire;

	private bool bool_2;

	public void Initialize(Vector3 position)
	{
		materialPropertyBlock_0 = new MaterialPropertyBlock();
		materialPropertyBlock_1 = new MaterialPropertyBlock();
		vector3_0 = position;
		method_3();
	}

	public void SetValue(bool value)
	{
		bool_2 = value;
	}

	public override IEnumerator RunProcess()
	{
		if (bool_2)
		{
			yield return method_0();
		}
		else
		{
			yield return method_1();
		}
	}

	public IEnumerator method_0()
	{
		_fire.gameObject.SetActive(value: true);
		ParticleSystem.MainModule main = _fire.main;
		main.loop = true;
		bool_0 = true;
		bool_1 = false;
		float num = 0f;
		while (_heightFade < 1f)
		{
			num += Time.deltaTime;
			_heightFade = Mathf.Lerp(0f, 1f, Mathf.Clamp01(num * _speed));
			method_2();
			yield return null;
		}
		Abort();
	}

	public IEnumerator method_1()
	{
		ParticleSystem.MainModule main = _fire.main;
		main.loop = false;
		float num = 0f;
		while (_heightFade > 0f)
		{
			num += Time.deltaTime;
			_heightFade = Mathf.Lerp(1f, 0f, Mathf.Clamp01(num * _speed));
			method_2();
			yield return null;
		}
		bool_0 = false;
		bool_1 = true;
		Abort();
	}

	public void method_2()
	{
		materialPropertyBlock_0.SetFloat(int_0, _heightFade);
		_renderer.SetPropertyBlock(materialPropertyBlock_0);
		materialPropertyBlock_1.SetFloat(int_0, _heightFade);
		_rendererGlow.SetPropertyBlock(materialPropertyBlock_1);
	}

	public void LateUpdate()
	{
		if (bool_1 && !_fire.isPlaying && _fire.gameObject.activeSelf)
		{
			_fire.gameObject.SetActive(value: false);
			bool_1 = false;
		}
		if (bool_0 && (bool)_renderer && (bool)_rendererGlow && CameraClass.Exist && !(CameraClass.Instance.Camera == null))
		{
			float num = Vector3.Distance(CameraClass.Instance.Camera.transform.position, vector3_0);
			float time = Mathf.Clamp01((_minMaxDistanceToFade.y - num) / _minMaxDistanceToFade.x);
			materialPropertyBlock_0.SetFloat(int_1, _lightPillarFade.Evaluate(time));
			_renderer.SetPropertyBlock(materialPropertyBlock_0);
			float time2 = Mathf.Clamp01((_minMaxDistanceGlowToFade.y - num) / _minMaxDistanceGlowToFade.x);
			materialPropertyBlock_1.SetFloat(int_1, _lightPillarGlowFade.Evaluate(time2));
			_rendererGlow.SetPropertyBlock(materialPropertyBlock_1);
		}
	}

	public void OnDestroy()
	{
		Abort();
		method_3();
	}

	public void method_3()
	{
		_heightFade = 0f;
		materialPropertyBlock_0.SetFloat(int_0, _heightFade);
		materialPropertyBlock_1.SetFloat(int_0, _heightFade);
		_renderer.SetPropertyBlock(materialPropertyBlock_0);
		_rendererGlow.SetPropertyBlock(materialPropertyBlock_1);
		bool_0 = false;
		_fire.gameObject.SetActive(value: false);
	}
}
