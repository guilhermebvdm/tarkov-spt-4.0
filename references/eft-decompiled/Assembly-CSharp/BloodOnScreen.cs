using System;
using EFT.BlitDebug;
using UnityEngine;

[DisallowMultipleComponent]
public class BloodOnScreen : MonoBehaviour
{
	public bool DebugMode;

	public float BloodColorValue = 1f;

	public float Refraction = 0.022f;

	public int DownsamplingCount = 1;

	public float MaxBloodTime = 5f;

	public int InitialBloodDrops = 3;

	public Color BloodColor;

	public bool GenerateUniqueMaterials = true;

	public Shader BloodShader;

	public Material BlitMaterial;

	public Material FadeoutMat;

	public Material BloodDropMaterial;

	private Material material_0;

	public int RenderTextureDimension;

	private GClass958 gclass958_0;

	private RenderTexture renderTexture_0;

	public Vector2 StartScaleDimension = new Vector2(1.5f, 2.2f);

	public Vector2 EndScaleDimension = new Vector2(0.3f, 0.7f);

	public Vector2 DropCountRange = new Vector2(5f, 10f);

	public Vector2 MaxRayLength = new Vector2(0.15f, 0.4f);

	public Vector2 StartXdistribution = new Vector2(0f, 0.075f);

	public Vector2 StartYdistribution = new Vector2(0f, 0.075f);

	public Vector2 EndXdistribution = new Vector2(0.05f, 0.3f);

	public Vector2 EndYdistribution = new Vector2(0.05f, 0.3f);

	public Vector2 DropLifetimeDistribution = new Vector2(0.05f, 1f);

	public float OffGlassScaleMultiplier = 2.5f;

	public float OffGlassLifetimeMultiplier = 2f;

	private GClass957 gclass957_0;

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

	public Vector2 center = new Vector2(0.5f, 0.5f);

	[Range(-100f, 100f)]
	public float sharpness = 10f;

	[Range(0f, 100f)]
	public float darkness = 30f;

	private float float_0;

	protected Material _vignetteMaterial;

	private float float_1 = float.MinValue;

	private RenderTexture renderTexture_1;

	private RenderTexture renderTexture_2;

	private RenderTexture renderTexture_3;

	private RenderTexture renderTexture_4;

	private bool bool_0;

	private bool bool_1;

	private SSAAPropagator ssaapropagator_0;

	private static readonly int int_0 = Shader.PropertyToID("_DudvMap");

	private static readonly int int_1 = Shader.PropertyToID("_BloodTextureAmount");

	private static readonly int int_2 = Shader.PropertyToID("_Refraction");

	private static readonly int int_3 = Shader.PropertyToID("_FadeTime");

	private static readonly int int_4 = Shader.PropertyToID("_BloodColor");

	private static readonly int int_5 = Shader.PropertyToID("_inputMin");

	private static readonly int int_6 = Shader.PropertyToID("_inputMax");

	private static readonly int int_7 = Shader.PropertyToID("_inputGamma");

	private static readonly int int_8 = Shader.PropertyToID("_outputMin");

	private static readonly int int_9 = Shader.PropertyToID("_outputMax");

	private static readonly int int_10 = Shader.PropertyToID("_Blured");

	private static readonly int int_11 = Shader.PropertyToID("_AddTex");

	private static readonly int int_12 = Shader.PropertyToID("_Data");

	private static readonly int int_13 = Shader.PropertyToID("_Color");

	public Material VignetteMaterial
	{
		get
		{
			if (_vignetteMaterial == null)
			{
				Shader shader = Shader.Find("Hidden/CC_FastVignette");
				_vignetteMaterial = new Material(shader)
				{
					hideFlags = HideFlags.HideAndDontSave
				};
			}
			return _vignetteMaterial;
		}
	}

	public void Start()
	{
		DropLifetimeDistribution.x *= 3f;
		DropLifetimeDistribution.y *= 1.2f;
		StartScaleDimension *= 1.2f;
		StartScaleDimension.x = Mathf.Min(StartXdistribution.x, EndScaleDimension.x);
		EndScaleDimension.x = StartScaleDimension.x;
		StartScaleDimension.y = Mathf.Max(StartXdistribution.y, EndScaleDimension.y);
		EndScaleDimension.y = StartScaleDimension.y;
		OffGlassScaleMultiplier = 1.8f;
		gclass958_0 = new GClass958(GetComponent<Camera>().aspect, RenderTextureDimension, BloodDropMaterial, DropLifetimeDistribution, OffGlassLifetimeMultiplier);
		GClass958 gClass = gclass958_0;
		gClass.OnImageRendered = (Action<RenderTexture>)Delegate.Combine(gClass.OnImageRendered, new Action<RenderTexture>(method_6));
		method_4();
		gclass957_0 = gclass958_0.DropPlacer;
		gclass958_0.Int32_0 = 2 * InitialBloodDrops;
		gclass957_0.Setup(StartScaleDimension, EndScaleDimension, DropCountRange, MaxRayLength, StartXdistribution, StartYdistribution, EndXdistribution, EndYdistribution, OffGlassScaleMultiplier);
		gclass958_0.ChangeGlassState(bool_1);
		gclass957_0.ChangeGlassState(bool_1);
		material_0 = new Material(BloodShader)
		{
			hideFlags = HideFlags.HideAndDontSave
		};
		method_7();
		method_0();
	}

	public void Awake()
	{
		ssaapropagator_0 = GetComponent<SSAAPropagator>();
	}

	public void method_0()
	{
		float_1 = Time.time - MaxBloodTime * 0.99f;
		gclass958_0.CaptureBloodShot(Vector3.zero);
	}

	public void OnDestroy()
	{
		GClass958 gClass = gclass958_0;
		gClass.OnImageRendered = (Action<RenderTexture>)Delegate.Remove(gClass.OnImageRendered, new Action<RenderTexture>(method_6));
		gclass958_0?.Destroy();
		method_5();
	}

	public void Update()
	{
		if (!(Time.time - float_1 > MaxBloodTime))
		{
			gclass958_0.Update();
			method_3();
		}
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.GetSourceDestination(out source, out destination);
		}
		method_1(source, destination);
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.ReleaseSourceDestination(source, destination);
		}
	}

	public void method_1(RenderTexture source, RenderTexture destination)
	{
		if (Time.time - float_1 > MaxBloodTime)
		{
			Graphics.Blit(source, destination);
			return;
		}
		Graphics.Blit(source, renderTexture_3);
		for (int i = 0; i < DownsamplingCount; i++)
		{
			Graphics.Blit(renderTexture_3, renderTexture_4);
			Graphics.Blit(renderTexture_4, renderTexture_3);
		}
		material_0.SetTexture(int_10, renderTexture_3);
		Graphics.Blit(source, renderTexture_1, material_0);
		method_2(renderTexture_1, destination);
	}

	public void method_2(RenderTexture source, RenderTexture destination)
	{
		if (Time.time - float_0 > 6f)
		{
			Graphics.Blit(source, destination);
			return;
		}
		darkness = 15f * (Mathf.Sin(Time.time) + 1f);
		VignetteMaterial.SetVector(int_12, new Vector4(center.x, center.y, sharpness * 0.01f, darkness * 0.02f));
		DebugGraphics.Blit(source, destination, VignetteMaterial, 1);
	}

	public void Hit(Vector3 direction)
	{
		if (gclass958_0 != null && !bool_0 && UnityEngine.Random.Range(0, 3) == 0)
		{
			float_1 = Time.time;
			gclass958_0.CaptureBloodShot(direction);
			bool_0 = true;
		}
	}

	public void HitBleeding(Vector3 direction, bool isArterial = false)
	{
		if (gclass958_0 != null && !bool_0)
		{
			float_1 = Time.time;
			if (isArterial)
			{
				float_0 = Time.time;
			}
			gclass958_0.CaptureBloodShotBleeding(direction, isArterial);
			bool_0 = true;
		}
	}

	public void ChangeGlassesState(bool hasGlasses)
	{
		bool_1 = hasGlasses;
		gclass957_0?.ChangeGlassState(hasGlasses);
		gclass958_0?.ChangeGlassState(hasGlasses);
	}

	public void method_3()
	{
		FadeoutMat.SetFloat(int_3, Time.deltaTime / MaxBloodTime);
		Graphics.Blit(renderTexture_0, renderTexture_1, FadeoutMat);
		Graphics.Blit(renderTexture_1, renderTexture_0);
	}

	public void method_4()
	{
		Vector2Int vector2Int = new Vector2Int(Mathf.CeilToInt(Mathf.Pow(2f, RenderTextureDimension)), Mathf.CeilToInt(Mathf.Pow(2f, RenderTextureDimension)));
		renderTexture_0 = new RenderTexture(vector2Int.x, vector2Int.y, 0)
		{
			name = "BloodOnScreen _duDvMap"
		};
		renderTexture_1 = new RenderTexture(vector2Int.x, vector2Int.y, 0)
		{
			name = "BloodOnScreen _tempRt"
		};
		renderTexture_2 = new RenderTexture(vector2Int.x, vector2Int.y, 0)
		{
			name = "OnBloodImageRenderedTmpRT"
		};
		renderTexture_3 = new RenderTexture(512, 512, 0)
		{
			name = "BloodOnScreen 512x512"
		};
		renderTexture_4 = new RenderTexture(256, 256, 0)
		{
			name = "BloodOnScreen 256x256"
		};
	}

	public void method_5()
	{
		if (renderTexture_0 != null)
		{
			UnityEngine.Object.Destroy(renderTexture_0);
		}
		if (renderTexture_1 != null)
		{
			UnityEngine.Object.Destroy(renderTexture_1);
		}
		if (renderTexture_2 != null)
		{
			UnityEngine.Object.Destroy(renderTexture_2);
		}
		if (renderTexture_3 != null)
		{
			UnityEngine.Object.Destroy(renderTexture_3);
		}
		if (renderTexture_4 != null)
		{
			UnityEngine.Object.Destroy(renderTexture_4);
		}
		if (material_0 != null)
		{
			UnityEngine.Object.Destroy(material_0);
		}
	}

	public void method_6(RenderTexture texture)
	{
		BlitMaterial.SetTexture(int_11, renderTexture_0);
		if (bool_1)
		{
			Graphics.Blit(texture, renderTexture_1, BlitMaterial);
			Graphics.Blit(renderTexture_1, renderTexture_0);
		}
		else
		{
			GClass972.MakeBlur(texture, renderTexture_2, 34f);
			Graphics.Blit(texture, renderTexture_1, BlitMaterial);
			Graphics.Blit(renderTexture_1, renderTexture_0);
		}
		bool_0 = false;
	}

	public void method_7()
	{
		VignetteMaterial.SetColor(int_13, Color.red);
		darkness = 0f;
		material_0.SetTexture(int_0, renderTexture_0);
		material_0.SetFloat(int_2, Refraction);
		material_0.SetFloat(int_1, BloodColorValue);
		material_0.SetColor(int_4, BloodColor);
		if (Mode == 0)
		{
			material_0.SetVector(int_5, new Vector4(InputMinL / 255f, InputMinL / 255f, InputMinL / 255f, 1f));
			material_0.SetVector(int_6, new Vector4(InputMaxL / 255f, InputMaxL / 255f, InputMaxL / 255f, 1f));
			material_0.SetVector(int_7, new Vector4(InputGammaL, InputGammaL, InputGammaL, 1f));
			material_0.SetVector(int_8, new Vector4(OutputMinL / 255f, OutputMinL / 255f, OutputMinL / 255f, 1f));
			material_0.SetVector(int_9, new Vector4(OutputMaxL / 255f, OutputMaxL / 255f, OutputMaxL / 255f, 1f));
		}
		else
		{
			material_0.SetVector(int_5, new Vector4(InputMinR / 255f, InputMinG / 255f, InputMinB / 255f, 1f));
			material_0.SetVector(int_6, new Vector4(InputMaxR / 255f, InputMaxG / 255f, InputMaxB / 255f, 1f));
			material_0.SetVector(int_7, new Vector4(InputGammaR, InputGammaG, InputGammaB, 1f));
			material_0.SetVector(int_8, new Vector4(OutputMinR / 255f, OutputMinG / 255f, OutputMinB / 255f, 1f));
			material_0.SetVector(int_9, new Vector4(OutputMaxR / 255f, OutputMaxG / 255f, OutputMaxB / 255f, 1f));
		}
	}
}
