using System;
using System.Collections;
using System.Collections.Generic;
using EFT.BlitDebug;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class VisorEffect : MonoBehaviour
{
	public enum EMask
	{
		NoMask,
		Narrow,
		Wide
	}

	[Serializable]
	public class VisorMask
	{
		public EMask EMask;

		public Texture Texture;
	}

	private const string string_0 = "_Mask";

	private const string string_1 = "_Parameter";

	private const string string_2 = "_SwingValue";

	private const string string_3 = "_HitZoomValue";

	private const string string_4 = "_ScratchesTex";

	private const string string_5 = "_ScratcesIntensity";

	private const string string_6 = "_BlurMask";

	private const string string_7 = "_DistortMask";

	private const string string_8 = "_GlassDamageTex";

	private const string string_9 = "_Rotation";

	private const string string_10 = "_DistortIntensity";

	private const string string_11 = "_Intensity";

	private const string string_12 = "_EdgeFade";

	private const string string_13 = "_BlurTex";

	private const string string_14 = "_MinLightValue";

	private const string string_15 = "_IsAffectedByLight";

	[Range(0f, 1f)]
	public float Intensity = 1f;

	public AnimationCurve EdgeFadeCurve = AnimationCurve.Linear(0f, 1f, 0f, 1f);

	public Shader VisorShader;

	public List<VisorMask> VisorMasks;

	public Texture Mask;

	public Texture Scratches;

	[Range(0f, 1f)]
	public float ScratcesIntensity = 1f;

	[SerializeField]
	private Material _glassDamageMaterial;

	[Range(0f, 1f)]
	public float MinLightValue = 0.15f;

	public bool IsAffectedByLight = true;

	private RenderTexture renderTexture_0;

	public GameObject VisorEffectPrefab;

	[Header("Blur")]
	public Texture BlurMask;

	[Range(0f, 2f)]
	public int downsample = 1;

	[Range(0f, 10f)]
	public float blurSize = 3f;

	[Range(1f, 4f)]
	public int blurIterations = 2;

	[Header("Distort")]
	public Texture DistortMask;

	public float DistortIntensity = 1f;

	[Header("Swing")]
	[Range(-1f, 1f)]
	public float SwingValue = 1f;

	public AnimationCurve DX;

	public AnimationCurve DY;

	public AnimationCurve HitSwingCurve = AnimationCurve.Linear(0f, 1f, 0f, 1f);

	public AnimationCurve WalkRotation;

	public float HitZoomMultiply = 1.5f;

	private bool bool_0 = true;

	private float float_0;

	private Vector2 vector2_0;

	private float float_1;

	private Vector2 vector2_1 = Vector2.zero;

	private float float_2;

	private bool bool_1 = true;

	protected SSAAPropagator _ssaaPropagator;

	[HideInInspector]
	public float StepFrequency;

	[HideInInspector]
	public float Velocity;

	private Material material_0;

	private Material material_1;

	private Dictionary<EMask, Texture> dictionary_0;

	[SerializeField]
	private Shader _blurShader;

	private Material material_2;

	private GameObject gameObject_0;

	public bool Visible
	{
		get
		{
			return bool_1;
		}
		set
		{
			bool_1 = value;
			SetIntensity(Intensity);
		}
	}

	public void OnEnable()
	{
		method_9();
	}

	public void Start()
	{
		method_8();
		_ssaaPropagator = GetComponent<SSAAPropagator>();
	}

	public void OnDestroy()
	{
		Clear();
	}

	public void Update()
	{
	}

	public RenderTexture method_0()
	{
		if (renderTexture_0 != null)
		{
			return renderTexture_0;
		}
		renderTexture_0 = new RenderTexture(Screen.currentResolution.width, Screen.currentResolution.height, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Default)
		{
			name = "GlassDamageRT",
			autoGenerateMips = false,
			useMipMap = false
		};
		return renderTexture_0;
	}

	public void SetIntensity(float intensity)
	{
		Intensity = intensity;
		base.enabled = bool_1 && intensity > 0f;
	}

	public void SetMask(EMask mask)
	{
		Material material = method_4();
		method_2();
		if (dictionary_0.ContainsKey(mask))
		{
			if (mask == EMask.Narrow)
			{
				method_3();
			}
			material.SetTexture("_Mask", dictionary_0[mask]);
		}
	}

	public void DrawScratches(byte seed, int hitCount)
	{
		RenderTexture active = RenderTexture.active;
		Graphics.SetRenderTarget(method_0());
		GL.Clear(clearDepth: true, clearColor: true, Color.clear);
		UnityEngine.Random.InitState(seed);
		for (int i = 0; i < hitCount; i++)
		{
			Vector4 vector = new Vector4(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(1.6f, 2.6f));
			Draw(new Vector3(vector.x, vector.y), new Vector3(vector.w / CameraClass.Instance.Camera.aspect, vector.w), vector.z);
		}
		RenderTexture.active = active;
	}

	public void BreakCompletely(byte seed, bool isBroken)
	{
		if (isBroken)
		{
			RenderTexture active = RenderTexture.active;
			Graphics.SetRenderTarget(method_0());
			UnityEngine.Random.InitState(seed);
			float aspect = CameraClass.Instance.Camera.aspect;
			Vector4 vector = new Vector4(UnityEngine.Random.Range(-1f, 0f), -1.1f, UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(2f, 3.2f));
			Draw(new Vector3(vector.x, vector.y), new Vector3(vector.w / aspect, vector.w), vector.z);
			Vector4 vector2 = new Vector4(UnityEngine.Random.Range(0f, 1f), -1.1f, UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(2f, 3.2f));
			Draw(new Vector3(vector2.x, vector2.y), new Vector3(vector2.w / aspect, vector2.w), vector2.z);
			Vector4 vector3 = new Vector4(UnityEngine.Random.Range(-1f, 0f), 1.1f, UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(2f, 3.2f));
			Draw(new Vector3(vector3.x, vector3.y), new Vector3(vector3.w / aspect, vector3.w), vector3.z);
			Vector4 vector4 = new Vector4(UnityEngine.Random.Range(0f, 1f), 1.1f, UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(2f, 3.2f));
			Draw(new Vector3(vector4.x, vector4.y), new Vector3(vector4.w / aspect, vector4.w), vector4.z);
			Vector4 vector5 = new Vector4(-1.1f, UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(2f, 3.2f));
			Draw(new Vector3(vector5.x, vector5.y), new Vector3(vector5.w / aspect, vector5.w), vector5.z);
			Vector4 vector6 = new Vector4(-1.1f, UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(2f, 3.2f));
			Draw(new Vector3(vector6.x, vector6.y), new Vector3(vector6.w / aspect, vector6.w), vector6.z);
			Vector4 vector7 = new Vector4(1.1f, UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(2f, 3.2f));
			Draw(new Vector3(vector7.x, vector7.y), new Vector3(vector7.w / aspect, vector7.w), vector7.z);
			Vector4 vector8 = new Vector4(1.1f, UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(2f, 3.2f));
			Draw(new Vector3(vector8.x, vector8.y), new Vector3(vector8.w / aspect, vector8.w), vector8.z);
			Vector4 vector9 = new Vector4(UnityEngine.Random.Range(-0.5f, 0.5f), -1.1f, UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(2f, 3.2f));
			Draw(new Vector3(vector9.x, vector9.y), new Vector3(vector9.w / aspect, vector9.w), vector9.z);
			Vector4 vector10 = new Vector4(UnityEngine.Random.Range(-0.5f, 0.5f), 1.1f, UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(2f, 3.2f));
			Draw(new Vector3(vector10.x, vector10.y), new Vector3(vector10.w / aspect, vector10.w), vector10.z);
			RenderTexture.active = active;
		}
	}

	public void Draw(Vector3 position, Vector3 scale, float rotation)
	{
		_glassDamageMaterial.SetPass(0);
		_glassDamageMaterial.SetFloat("_Rotation", rotation);
		float x = position.x - scale.x;
		float x2 = position.x + scale.x;
		float y = 0f - position.y - scale.y;
		float y2 = 0f - position.y + scale.y;
		GL.Begin(7);
		_glassDamageMaterial.SetPass(0);
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

	public void Clear()
	{
		if (gameObject_0 != null)
		{
			UnityEngine.Object.DestroyImmediate(gameObject_0);
		}
		UnityEngine.Object.DestroyImmediate(method_0());
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.GetSourceDestination(out source, out destination);
		}
		method_1(source, destination);
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.ReleaseSourceDestination(source, destination);
		}
	}

	public void method_1(RenderTexture source, RenderTexture destination)
	{
		if (Intensity < 0.01f)
		{
			if (gameObject_0 != null && gameObject_0.activeSelf)
			{
				gameObject_0.SetActive(value: false);
			}
			GClass860.BlitOrCopy(source, destination);
			return;
		}
		if (gameObject_0 != null && !gameObject_0.activeSelf)
		{
			gameObject_0.SetActive(value: true);
		}
		Material material = method_4();
		RenderTexture myTex = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
		RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
		RenderTexture temporary2 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
		RenderTexture source2 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
		method_6(material);
		material.SetFloat("_ScratcesIntensity", ScratcesIntensity);
		material.SetFloat("_DistortIntensity", DistortIntensity);
		material.SetFloat("_Intensity", Intensity);
		material.SetFloat("_EdgeFade", EdgeFadeCurve.Evaluate(Intensity));
		material.SetFloat("_MinLightValue", MinLightValue);
		material.SetFloat("_IsAffectedByLight", Convert.ToInt32(IsAffectedByLight));
		DebugGraphics.Blit(source, temporary, material, 0);
		DebugGraphics.Blit(temporary, temporary2, material, 1);
		DebugGraphics.Blit(temporary2, source2, material, 2);
		Modify(ref source2, ref myTex);
		material.SetTexture("_BlurTex", myTex);
		DebugGraphics.Blit(source2, destination, material, 3);
		RenderTexture.ReleaseTemporary(myTex);
		RenderTexture.ReleaseTemporary(temporary);
		RenderTexture.ReleaseTemporary(temporary2);
		RenderTexture.ReleaseTemporary(source2);
	}

	public void Modify(ref RenderTexture source, ref RenderTexture myTex)
	{
		Material material = method_5();
		float num = 1f / (1f * (float)(1 << downsample));
		material.SetVector("_Parameter", new Vector4(blurSize * num, (0f - blurSize) * num, 0f, 0f));
		source.filterMode = FilterMode.Bilinear;
		int width = source.width >> downsample;
		int height = source.height >> downsample;
		RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 0, source.format);
		renderTexture.filterMode = FilterMode.Bilinear;
		DebugGraphics.Blit(source, renderTexture, material, 0);
		for (int i = 0; i < blurIterations; i++)
		{
			float num2 = (float)i * 1f;
			material.SetVector("_Parameter", new Vector4(blurSize * num + num2, (0f - blurSize) * num - num2, 0f, 0f));
			RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, source.format);
			temporary.filterMode = FilterMode.Bilinear;
			DebugGraphics.Blit(renderTexture, temporary, material, 1);
			RenderTexture.ReleaseTemporary(renderTexture);
			renderTexture = temporary;
			temporary = RenderTexture.GetTemporary(width, height, 0, source.format);
			temporary.filterMode = FilterMode.Bilinear;
			DebugGraphics.Blit(renderTexture, temporary, material, 2);
			RenderTexture.ReleaseTemporary(renderTexture);
			renderTexture = temporary;
		}
		Graphics.Blit(renderTexture, myTex);
		RenderTexture.ReleaseTemporary(renderTexture);
	}

	public void method_2()
	{
		Material material = method_4();
		material.SetTexture("_Mask", Mask);
		material.SetTexture("_ScratchesTex", Scratches);
		material.SetTexture("_BlurMask", BlurMask);
		material.SetTexture("_DistortMask", DistortMask);
		material.SetTexture("_GlassDamageTex", method_0());
	}

	public void method_3()
	{
		Material material = method_4();
		material.SetTexture("_Mask", null);
		material.SetTexture("_ScratchesTex", null);
		material.SetTexture("_BlurMask", null);
		material.SetTexture("_DistortMask", null);
		material.SetTexture("_GlassDamageTex", null);
	}

	public Material method_4()
	{
		if (material_0 != null)
		{
			return material_0;
		}
		return material_0 = new Material(VisorShader);
	}

	public Material method_5()
	{
		if (material_2 != null)
		{
			return material_2;
		}
		if (_blurShader == null)
		{
			_blurShader = GClass872.Find("Hidden/FastBlur");
			if (_blurShader == null)
			{
				Debug.LogError("Can't find 'Hidden/FastBlur'");
			}
		}
		return material_2 = new Material(_blurShader);
	}

	public void method_6(Material material)
	{
		if (!(StepFrequency < 0.01f))
		{
			if (Velocity < 0.001f)
			{
				vector2_0 = Vector2.Lerp(vector2_0, Vector2.zero, Time.deltaTime * 10f);
				float_1 = Mathf.Lerp(float_1, 0f, Time.deltaTime * 10f);
				float_0 = 0f;
			}
			else
			{
				float_0 += Time.deltaTime * StepFrequency;
				vector2_0 = new Vector2(DX.Evaluate(float_0), DY.Evaluate(float_0));
				float_1 = WalkRotation.Evaluate(float_0);
			}
			vector2_0 += vector2_1;
			float_1 += vector2_1.y;
			material.SetFloat("_Rotation", float_1);
			material.SetVector("_SwingValue", vector2_0);
			material.SetFloat("_HitZoomValue", float_2);
		}
	}

	public IEnumerator method_7()
	{
		float num = UnityEngine.Random.Range(-1f, 1f);
		float num2 = UnityEngine.Random.Range(-1f, 1f);
		bool_0 = false;
		for (float num3 = 0f; num3 < 1f; num3 += 0.1f)
		{
			float num4 = HitSwingCurve.Evaluate(num3);
			float x = num * num4;
			float y = num2 * num4;
			vector2_1 = new Vector2(x, y);
			float_2 = num4 * HitZoomMultiply;
			yield return null;
		}
		vector2_1 = Vector2.zero;
		float_2 = 0f;
		bool_0 = true;
	}

	public void method_8()
	{
		if (!(gameObject_0 != null) && Application.isPlaying)
		{
			gameObject_0 = UnityEngine.Object.Instantiate(VisorEffectPrefab);
			gameObject_0.transform.SetParent(base.transform, worldPositionStays: false);
			gameObject_0.name = "Visor Point";
			gameObject_0.SetActive(value: false);
		}
	}

	public void method_9()
	{
		dictionary_0 = new Dictionary<EMask, Texture>();
		for (int i = 0; i < VisorMasks.Count; i++)
		{
			dictionary_0.Add(VisorMasks[i].EMask, VisorMasks[i].Texture);
		}
	}
}
