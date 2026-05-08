using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EyeBurn : MonoBehaviour
{
	public class Class611
	{
		public Vector3 ScreenPosition;

		public Material Material;

		public float Rotation;

		public Vector3 Scale;

		public float StartTime;

		public float MaxTime;

		public Class611(Vector3 screenPosition, Material material, float rotation, Vector3 scale, float startTime, float maxTime)
		{
			ScreenPosition = screenPosition;
			Material = material;
			Rotation = rotation;
			Scale = scale;
			StartTime = startTime;
			MaxTime = maxTime;
		}
	}

	public Material EyeBurnScreenEffect;

	[SerializeField]
	private Material[] _eyeBurnEffectMaterial;

	[SerializeField]
	private Material[] _eyeBurnTrailMaterial;

	[SerializeField]
	private Vector2 _scaleMinMax;

	[SerializeField]
	private float _eyeBurnMaxTime = 100f;

	[SerializeField]
	private float TrailSpotEvery = 0.4f;

	[SerializeField]
	private float TrailSpotShift = 0.15f;

	[SerializeField]
	private AnimationCurve trailDecreaseCurve;

	[SerializeField]
	private int MaxTrailLength = 8;

	[SerializeField]
	private AnimationCurve _scaleCoefCurve;

	[CompilerGenerated]
	private bool bool_0;

	public bool IsPaused;

	private const string string_0 = "_Rotation";

	private const string string_1 = "_Coef";

	private RenderTexture renderTexture_0;

	private Coroutine coroutine_0;

	private List<Class611> list_0 = new List<Class611>();

	private int int_0;

	private Vector3 vector3_0;

	private static readonly int int_1 = Shader.PropertyToID("_EyeBurnTex");

	protected SSAAPropagator _ssaaPropagator;

	public bool EyesBurn
	{
		[CompilerGenerated]
		get
		{
			return bool_0;
		}
		[CompilerGenerated]
		set
		{
			bool_0 = value;
		}
	}

	public void Awake()
	{
		list_0 = new List<Class611>();
		_ssaaPropagator = GetComponent<SSAAPropagator>();
	}

	public void OnEnable()
	{
		Awake();
	}

	public void OnDisable()
	{
		list_0.Clear();
		ClearEyeBurnRT();
	}

	public void Update()
	{
		if (list_0.Count != 0 && !IsPaused)
		{
			RenderTexture active = RenderTexture.active;
			Graphics.SetRenderTarget(method_0());
			GL.Clear(clearDepth: true, clearColor: true, Color.clear);
			EyeBurnScreenEffect.SetTexture(int_1, method_0());
			for (int i = 0; i < list_0.Count; i++)
			{
				float num = Mathf.Clamp01(1f - (Time.timeSinceLevelLoad - list_0[i].StartTime) / list_0[i].MaxTime);
				float scaleCoef = _scaleCoefCurve.Evaluate(num);
				Draw(list_0[i].ScreenPosition, list_0[i].Scale, list_0[i].Rotation, list_0[i].Material, num, scaleCoef);
			}
			RenderTexture.active = active;
		}
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.GetSourceDestination(out source, out destination);
		}
		Graphics.Blit(source, destination, EyeBurnScreenEffect);
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.ReleaseSourceDestination(source, destination);
		}
	}

	public void Burn(Vector3 position, float stregnth, Vector2 deltaRotation)
	{
		float num = Mathf.Lerp(_scaleMinMax.x, _scaleMinMax.y, stregnth);
		Vector3 scale = new Vector3(num, num / (float)Screen.height * (float)Screen.width);
		float eyeBurnTime = _eyeBurnMaxTime * stregnth;
		Vector3 screenPosition = CameraClass.Instance.Camera.WorldToViewportPoint(position);
		if (method_4(screenPosition))
		{
			method_2(screenPosition, scale, eyeBurnTime);
			method_3(new Vector2(deltaRotation.x, 0f - deltaRotation.y), scale, eyeBurnTime);
			if (coroutine_0 != null)
			{
				StopCoroutine(coroutine_0);
			}
			coroutine_0 = StartCoroutine(method_1(_eyeBurnMaxTime));
		}
	}

	public void Draw(Vector3 position, Vector3 scale, float rotation, Material material, float coef, float scaleCoef)
	{
		material.SetPass(0);
		material.SetFloat("_Rotation", rotation);
		material.SetFloat("_Coef", coef);
		float x = position.x - scale.x * scaleCoef;
		float x2 = position.x + scale.x * scaleCoef;
		float y = 0f - position.y - scale.y * scaleCoef;
		float y2 = 0f - position.y + scale.y * scaleCoef;
		GL.Begin(7);
		material.SetPass(0);
		GL.Begin(7);
		GL.TexCoord2(0f, 0f);
		GL.Vertex3(x, y, 0f);
		GL.TexCoord2(1f, 0f);
		GL.Vertex3(x2, y, 0f);
		GL.TexCoord2(1f, 1f);
		GL.Vertex3(x2, y2, 0f);
		GL.TexCoord2(0f, 1f);
		GL.Vertex3(x, y2, 0f);
		GL.End();
	}

	public RenderTexture method_0()
	{
		if (renderTexture_0 != null)
		{
			return renderTexture_0;
		}
		renderTexture_0 = new RenderTexture(Screen.currentResolution.width, Screen.currentResolution.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default)
		{
			name = "EyeBurnRT",
			autoGenerateMips = false,
			useMipMap = false
		};
		return renderTexture_0;
	}

	public IEnumerator method_1(float t)
	{
		float num = 0f;
		while (num <= 1f)
		{
			if (!IsPaused)
			{
				num += Time.deltaTime / t;
			}
			yield return null;
		}
		base.enabled = false;
	}

	public void method_2(Vector3 screenPosition, Vector3 scale, float eyeBurnTime)
	{
		Material material = _eyeBurnEffectMaterial[++int_0 % _eyeBurnEffectMaterial.Length];
		int num = Random.Range(0, 360);
		Vector3 screenPosition2 = method_5(screenPosition);
		Class611 item = new Class611(screenPosition2, material, num, scale, Time.timeSinceLevelLoad, eyeBurnTime);
		vector3_0 = screenPosition2;
		list_0.Add(item);
	}

	public void method_3(Vector3 direction, Vector3 scale, float eyeBurnTime)
	{
		Vector3 normalized = direction.normalized;
		int num = Mathf.Min(MaxTrailLength, (int)(direction.magnitude / TrailSpotEvery));
		if (num >= 1)
		{
			Vector3 screenPosition = vector3_0 + normalized * TrailSpotShift * scale.x;
			for (int i = 0; i < num; i++)
			{
				Material material = _eyeBurnTrailMaterial[Random.Range(0, _eyeBurnTrailMaterial.Length)];
				int num2 = Random.Range(0, 360);
				Vector3 scale2 = scale * trailDecreaseCurve.Evaluate(i);
				Class611 item = new Class611(screenPosition, material, num2, scale2, Time.timeSinceLevelLoad, eyeBurnTime);
				list_0.Add(item);
				screenPosition += normalized * TrailSpotShift * scale2.x;
			}
		}
	}

	public bool method_4(Vector3 screenPosition)
	{
		if (screenPosition.x < 1f && screenPosition.x > 0f && screenPosition.y < 1f && screenPosition.y > 0f)
		{
			return screenPosition.z > 0f;
		}
		return false;
	}

	public Vector3 method_5(Vector3 screenPosition)
	{
		return new Vector3(screenPosition.x * 2f - 1f, screenPosition.y * 2f - 1f, screenPosition.z);
	}

	public void ClearEyeBurnRT()
	{
		RenderTexture active = RenderTexture.active;
		Graphics.SetRenderTarget(method_0());
		GL.Clear(clearDepth: true, clearColor: true, Color.clear);
		RenderTexture.active = active;
	}
}
