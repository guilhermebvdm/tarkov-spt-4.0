using UnityEngine;

[DisallowMultipleComponent]
public class RainScreenDrops : MonoBehaviour
{
	[Header("Appearance settings")]
	[SerializeField]
	private Shader _blitShader;

	[SerializeField]
	[Range(0f, 1f)]
	private float _intensity = 1f;

	[SerializeField]
	private float _refraction = 0.1f;

	[SerializeField]
	private float _refractionWithoutGlass = 0.1f;

	[SerializeField]
	private int _downsamplingCount = 7;

	[Header("Drops settings")]
	[SerializeField]
	private AnimationCurve _dropScaleCurve;

	[SerializeField]
	private int _dropsAmount = 32;

	[SerializeField]
	private float _rainDropsDelay = 0.1f;

	[SerializeField]
	private Vector2 _dropScale = new Vector2(0.025f, 0.6f);

	[SerializeField]
	private float _dropLifetime = 25f;

	[SerializeField]
	private float _dropLifetimeWithoutGlass = 10f;

	[SerializeField]
	private bool _isDropsShouldMove;

	[SerializeField]
	private int _maxDropsAtOnce = 4;

	[SerializeField]
	private Material _dropMaterial;

	[SerializeField]
	private float _scaleMultiplierWithoutGlass = 3f;

	private Material material_0;

	public RenderTexture DuDvMap;

	[Space(10f)]
	private RenderTexture renderTexture_0;

	private GameObject gameObject_0;

	private GClass986 gclass986_0;

	private float float_0 = 25f;

	private float float_1;

	private SSAAPropagator ssaapropagator_0;

	private bool bool_0;

	[HideInInspector]
	public int Mode;

	[HideInInspector]
	[SerializeField]
	public float InputMinL;

	[HideInInspector]
	[SerializeField]
	public float InputMaxL = 255f;

	[HideInInspector]
	[SerializeField]
	public float InputGammaL = 1f;

	[HideInInspector]
	[SerializeField]
	public float InputMinR;

	[HideInInspector]
	[SerializeField]
	public float InputMaxR = 255f;

	[HideInInspector]
	[SerializeField]
	public float InputGammaR = 1f;

	[HideInInspector]
	[SerializeField]
	public float InputMinG;

	[HideInInspector]
	[SerializeField]
	public float InputMaxG = 255f;

	[HideInInspector]
	[SerializeField]
	public float InputGammaG = 1f;

	[HideInInspector]
	[SerializeField]
	public float InputMinB;

	[HideInInspector]
	[SerializeField]
	public float InputMaxB = 255f;

	[HideInInspector]
	[SerializeField]
	public float InputGammaB = 1f;

	[HideInInspector]
	[SerializeField]
	public float OutputMinL;

	[HideInInspector]
	[SerializeField]
	public float OutputMaxL = 255f;

	[HideInInspector]
	[SerializeField]
	public float OutputMinR;

	[HideInInspector]
	[SerializeField]
	public float OutputMaxR = 255f;

	[HideInInspector]
	[SerializeField]
	public float OutputMinG;

	[HideInInspector]
	[SerializeField]
	public float OutputMaxG = 255f;

	[HideInInspector]
	[SerializeField]
	public float OutputMinB;

	[HideInInspector]
	[SerializeField]
	public float OutputMaxB = 255f;

	private static readonly int int_0 = Shader.PropertyToID("_inputMin");

	private static readonly int int_1 = Shader.PropertyToID("_inputMax");

	private static readonly int int_2 = Shader.PropertyToID("_inputGamma");

	private static readonly int int_3 = Shader.PropertyToID("_outputMin");

	private static readonly int int_4 = Shader.PropertyToID("_outputMax");

	private static readonly int int_5 = Shader.PropertyToID("_Blured");

	private static readonly int int_6 = Shader.PropertyToID("_DudvMap");

	private static readonly int int_7 = Shader.PropertyToID("_Refraction");

	private static readonly int int_8 = Shader.PropertyToID("_Intensity");

	public float Intensity
	{
		get
		{
			return _intensity;
		}
		set
		{
			_intensity = Mathf.Clamp01(value);
			if (gclass986_0 != null)
			{
				gclass986_0.Intensity = _intensity;
			}
		}
	}

	public void ChangeGlassesState(bool hasGlasses)
	{
		bool_0 = hasGlasses;
		gclass986_0?.ChangeGlassState(hasGlasses);
	}

	public void Awake()
	{
		ssaapropagator_0 = GetComponent<SSAAPropagator>();
	}

	public void Init()
	{
		GClass987.IsDropsShouldMove = _isDropsShouldMove;
		gclass986_0 = new GClass986(_dropScaleCurve, DuDvMap, _dropsAmount, _rainDropsDelay, _dropScale, _dropLifetime, _dropLifetimeWithoutGlass, _maxDropsAtOnce, GetComponent<Camera>(), _dropMaterial, _scaleMultiplierWithoutGlass);
		gclass986_0.ChangeGlassState(bool_0);
		material_0 = new Material(_blitShader)
		{
			hideFlags = HideFlags.HideAndDontSave
		};
		material_0.SetTexture(int_6, DuDvMap);
		float_1 = float_0 + 3f;
	}

	public void OnValidate()
	{
		if (gclass986_0 != null)
		{
			gclass986_0.UpdateValues(_dropScaleCurve, DuDvMap, _dropsAmount, _rainDropsDelay, _dropScale, _dropLifetime, _dropLifetimeWithoutGlass, _maxDropsAtOnce, GetComponent<Camera>(), _dropMaterial, _scaleMultiplierWithoutGlass);
			gclass986_0.ChangeGlassState(bool_0);
		}
	}

	public void Update()
	{
		if (gclass986_0 != null)
		{
			if (Mathf.Abs(_intensity) < 0.1f)
			{
				float_1 += Time.deltaTime;
			}
			else
			{
				float_1 = 0f;
			}
			gclass986_0.Update(Time.deltaTime);
		}
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.GetSourceDestination(out source, out destination);
		}
		method_0(source, destination);
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.ReleaseSourceDestination(source, destination);
		}
	}

	public void method_0(RenderTexture source, RenderTexture destanation)
	{
		if (!(DuDvMap == null) && !(float_1 > float_0) && gclass986_0 != null && DuDvMap.IsCreated())
		{
			material_0.SetFloat(int_7, bool_0 ? _refraction : _refractionWithoutGlass);
			material_0.SetFloat(int_8, _intensity);
			if (Mode == 0)
			{
				material_0.SetVector(int_0, new Vector4(InputMinL / 255f, InputMinL / 255f, InputMinL / 255f, 1f));
				material_0.SetVector(int_1, new Vector4(InputMaxL / 255f, InputMaxL / 255f, InputMaxL / 255f, 1f));
				material_0.SetVector(int_2, new Vector4(InputGammaL, InputGammaL, InputGammaL, 1f));
				material_0.SetVector(int_3, new Vector4(OutputMinL / 255f, OutputMinL / 255f, OutputMinL / 255f, 1f));
				material_0.SetVector(int_4, new Vector4(OutputMaxL / 255f, OutputMaxL / 255f, OutputMaxL / 255f, 1f));
			}
			else
			{
				material_0.SetVector(int_0, new Vector4(InputMinR / 255f, InputMinG / 255f, InputMinB / 255f, 1f));
				material_0.SetVector(int_1, new Vector4(InputMaxR / 255f, InputMaxG / 255f, InputMaxB / 255f, 1f));
				material_0.SetVector(int_2, new Vector4(InputGammaR, InputGammaG, InputGammaB, 1f));
				material_0.SetVector(int_3, new Vector4(OutputMinR / 255f, OutputMinG / 255f, OutputMinB / 255f, 1f));
				material_0.SetVector(int_4, new Vector4(OutputMaxR / 255f, OutputMaxG / 255f, OutputMaxB / 255f, 1f));
			}
			RenderTexture temporary = RenderTexture.GetTemporary(512, 512, 0);
			RenderTexture temporary2 = RenderTexture.GetTemporary(256, 256, 0);
			Graphics.Blit(source, temporary);
			for (int i = 0; i < _downsamplingCount; i++)
			{
				Graphics.Blit(temporary, temporary2);
				Graphics.Blit(temporary2, temporary);
			}
			material_0.SetTexture(int_5, temporary);
			Graphics.Blit(source, destanation, material_0);
			RenderTexture.ReleaseTemporary(temporary);
			RenderTexture.ReleaseTemporary(temporary2);
		}
		else
		{
			GClass860.BlitOrCopy(source, destanation);
		}
	}

	public void OnDestroy()
	{
		if (gclass986_0 != null)
		{
			gclass986_0.Clear();
		}
	}

	public void SetIntensity(float intensity)
	{
		Intensity = intensity;
		base.enabled = intensity > 0f;
	}
}
