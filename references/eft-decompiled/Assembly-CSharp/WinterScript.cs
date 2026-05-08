using System;
using System.Collections.Generic;
using UnityEngine;

public class WinterScript : MonoBehaviour
{
	public class GClass993
	{
		[NonSerialized]
		public AudioClip AudioClip_0;

		[NonSerialized]
		public float[] Float_0;

		[NonSerialized]
		public float[] Float_1;

		[NonSerialized]
		public float[] Float_2;

		[NonSerialized]
		public int Int_0;

		public GClass993(AudioClip sourceClip, float[] addData)
		{
			if (sourceClip == null)
			{
				Debug.Log("sourceClip is null");
			}
			if (addData == null)
			{
				Debug.Log("addData is null");
			}
			AudioClip_0 = sourceClip;
			Float_1 = new float[sourceClip.samples];
			sourceClip.GetData(Float_1, 0);
			Float_0 = new float[Float_1.Length];
			for (int i = 0; i < Float_0.Length; i++)
			{
				Float_0[i] = Float_1[i];
			}
			Float_2 = addData;
			Int_0 = ((sourceClip.samples < addData.Length) ? sourceClip.samples : addData.Length);
		}

		public void Add(float f)
		{
			for (int i = 0; i < Int_0; i++)
			{
				Float_0[i] = Float_1[i] + (Float_2[i] - Float_1[i]) * f;
			}
			AudioClip_0.SetData(Float_0, 0);
		}
	}

	public class GClass994
	{
		public class Class689
		{
			[NonSerialized]
			public static Color Color_0 = Color.white;

			[NonSerialized]
			public DetailPrototype DetailPrototype_0;

			[NonSerialized]
			public Color Color_1;

			[NonSerialized]
			public Color Color_2;

			public Class689(DetailPrototype prototype)
			{
				DetailPrototype_0 = prototype;
				Color_1 = prototype.dryColor;
				Color_2 = prototype.healthyColor;
			}

			public void Lerp(float t)
			{
				if (t >= 1f)
				{
					DetailPrototype_0.dryColor = Color_0;
					DetailPrototype_0.healthyColor = Color_0;
				}
				else
				{
					DetailPrototype_0.dryColor = smethod_0(Color_1, Color_0, t);
					DetailPrototype_0.healthyColor = smethod_0(Color_2, Color_0, t);
				}
			}

			public static Color smethod_0(Color a, Color b, float t)
			{
				return new Color(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t, a.a + (b.a - a.a) * t);
			}
		}

		[NonSerialized]
		public TerrainData TerrainData_0;

		[NonSerialized]
		public DetailPrototype[] DetailPrototype_0;

		[NonSerialized]
		public Class689[] Class689_0;

		[NonSerialized]
		public int[] Int_0;

		[NonSerialized]
		public Color32[] Color32_0;

		[NonSerialized]
		public Color32[] Color32_1;

		[NonSerialized]
		public Texture2D Texture2D_0;

		[NonSerialized]
		public float Float_0;

		public GClass994(Terrain terrain, Texture2D detailTex)
		{
			Debug.Log("TerrainDetailsRepaint Ctr");
			if (terrain == null)
			{
				Debug.Log("terrain is null");
			}
			if (detailTex == null)
			{
				Debug.Log("detailTex is null");
			}
			TerrainData_0 = terrain.terrainData;
			DetailPrototype_0 = TerrainData_0.detailPrototypes;
			if (TerrainData_0 == null)
			{
				Debug.Log("_terrainData is null");
			}
			if (DetailPrototype_0 == null)
			{
				Debug.Log("_prototypes is null");
			}
			if (DetailPrototype_0.Length == 0)
			{
				Debug.Log("_prototypes.Length equal 0");
			}
			if (DetailPrototype_0.Length != 0)
			{
				Color32_0 = detailTex.GetPixels32();
				Color32_1 = new Color32[Color32_0.Length];
				for (int i = 0; i < Color32_1.Length; i++)
				{
					Color32_1[i] = Color32_0[i];
				}
				Texture2D_0 = new Texture2D(detailTex.width, detailTex.height, TextureFormat.RGBA32, mipChain: true);
				Texture2D_0.name = "WinterScript";
				Class689_0 = new Class689[DetailPrototype_0.Length];
				for (int j = 0; j < DetailPrototype_0.Length; j++)
				{
					DetailPrototype_0[j].prototypeTexture = Texture2D_0;
					Class689_0[j] = new Class689(DetailPrototype_0[j]);
				}
			}
			Int_0 = new int[256];
		}

		public void Update(float t)
		{
			if (Mathf.Abs(Float_0 - t) < 0.02f)
			{
				return;
			}
			Float_0 = t;
			int num = (int)(t * 140f) - 50;
			t *= 2f;
			if (Class689_0 == null)
			{
				Debug.Log("Terrain._lerpers is null");
			}
			Class689[] class689_ = Class689_0;
			for (int i = 0; i < class689_.Length; i++)
			{
				if (class689_[i] == null)
				{
					Debug.Log("Terrain.lerper is null");
				}
			}
			class689_ = Class689_0;
			for (int i = 0; i < class689_.Length; i++)
			{
				class689_[i].Lerp(t);
			}
			for (int j = 0; j < 256; j++)
			{
				int num2 = (j >> 1) + num;
				if (num2 < 0)
				{
					num2 = 0;
				}
				if (num2 > 255)
				{
					num2 = 255;
				}
				Int_0[j] = num2;
			}
			smethod_0(Color32_0, Color32_1, Int_0);
			if (Texture2D_0 == null)
			{
				Debug.Log("_texture is null");
			}
			if (TerrainData_0 == null)
			{
				Debug.Log("_terrainData2 is null");
			}
			if (DetailPrototype_0 == null)
			{
				Debug.Log("_prototypes2 is null");
			}
			Texture2D_0.SetPixels32(Color32_1);
			Texture2D_0.Apply(updateMipmaps: true);
			TerrainData_0.detailPrototypes = DetailPrototype_0;
		}

		public static void smethod_0(Color32[] source, Color32[] current, int[] green)
		{
			for (int i = 0; i < source.Length; i++)
			{
				int b = green[source[i].g];
				current[i].r = smethod_1(source[i].r, b);
				current[i].g = smethod_1(source[i].g, b);
				current[i].b = smethod_1(source[i].b, b);
			}
		}

		public static byte smethod_1(byte a, int b)
		{
			b += a;
			if (b > 255)
			{
				b = 255;
			}
			return (byte)b;
		}
	}

	public float debugValue;

	public bool debugWrite;

	public Texture SnowTex;

	public float StartTime;

	public AnimationCurve SnowLevelCurve;

	public AnimationCurve SnowFallingCurve;

	public AnimationCurve DesaturateSunCurve;

	public AnimationCurve SunIntensityCurve = AnimationCurve.Linear(0f, 1f, 100f, 1f);

	public AnimationCurve SoundsLerpCurve;

	public AudioClip[] SnowStepClip;

	public AudioClip SnowyWind;

	public AnimationCurve MusicFadeOut;

	public AnimationCurve MusicFadeIn;

	public AnimationCurve BreathCurve;

	public float FadeShadow = 0.4f;

	public float FadeScratches = 0.7f;

	public float FadeFog = 1.2f;

	private Light light_0;

	private Color color_0;

	private Color color_1;

	private float float_0;

	private float float_1;

	private float float_2;

	private AudioSource audioSource_0;

	private AudioSource audioSource_1;

	private LinkedList<GClass993> linkedList_0 = new LinkedList<GClass993>();

	public Transform BreathSystem;

	private Transform transform_0;

	private ParticleSystem particleSystem_0;

	private Transform transform_1;

	public LayerMask DepthRendererMask;

	public Material DepthMaterial;

	public Material TerrainMaterial;

	private GClass994 gclass994_0;

	public AnimationCurve TerrainDetailCurve;

	public Texture2D[] TerrainDetails;

	private static readonly int int_0 = Shader.PropertyToID("_SpecColor");

	public void Start()
	{
		method_0();
		method_3();
		Terrain activeTerrain = Terrain.activeTerrain;
		if (activeTerrain == null)
		{
			Debug.Log("terrain is null");
		}
		gclass994_0 = null;
		if (!(activeTerrain != null) || GClass842.DisabledForNow)
		{
			return;
		}
		if (TerrainDetails == null)
		{
			Debug.Log("TerrainDetails is null");
		}
		if (TerrainDetails.Length == 0)
		{
			Debug.Log("TerrainDetails.Length equal 0");
		}
		Texture2D detailTex = TerrainDetails[0];
		if (activeTerrain.terrainData == null)
		{
			Debug.Log("terrain.terrainData is null");
		}
		if (activeTerrain.terrainData.detailPrototypes == null)
		{
			Debug.Log("terrain.terrainData.detailPrototypes is null");
		}
		DetailPrototype[] detailPrototypes = activeTerrain.terrainData.detailPrototypes;
		if (detailPrototypes == null)
		{
			Debug.Log("prototypes is null");
		}
		if (detailPrototypes.Length == 0)
		{
			Debug.Log("prototypes.Length equal 0");
		}
		if (detailPrototypes == null || detailPrototypes.Length == 0)
		{
			return;
		}
		string text = null;
		DetailPrototype[] array = detailPrototypes;
		foreach (DetailPrototype detailPrototype in array)
		{
			if (detailPrototype.prototypeTexture != null)
			{
				text = detailPrototype.prototypeTexture.name;
				break;
			}
		}
		if (text == null)
		{
			Debug.Log("detailName is null");
		}
		if (text == null)
		{
			return;
		}
		Texture2D[] terrainDetails = TerrainDetails;
		foreach (Texture2D texture2D in terrainDetails)
		{
			if (texture2D.name == text)
			{
				detailTex = texture2D;
			}
		}
		gclass994_0 = new GClass994(activeTerrain, detailTex);
	}

	public void Update()
	{
		float num = 0f - StartTime;
		num = 60f;
		float num2 = SnowFallingCurve.Evaluate(num);
		if (light_0 != null)
		{
			light_0.color = Color.Lerp(color_0, color_1, DesaturateSunCurve.Evaluate(num));
			float num3 = 1f - num2 * FadeShadow;
			light_0.intensity = float_0 * num3 * SunIntensityCurve.Evaluate(num);
			light_0.shadowStrength = float_1 * num3;
		}
		float num4 = SoundsLerpCurve.Evaluate(num);
		if (Mathf.Abs(float_2 - num4) > 0.04f)
		{
			float_2 = num4;
			if (linkedList_0 == null)
			{
				Debug.Log("_lerpers is null");
			}
			foreach (GClass993 item in linkedList_0)
			{
				if (item == null)
				{
					Debug.Log("lerper is null");
				}
			}
			foreach (GClass993 item2 in linkedList_0)
			{
				item2.Add(num4);
			}
		}
		if (audioSource_0 != null)
		{
			audioSource_0.volume = MusicFadeOut.Evaluate(num);
			if (MusicFadeOut[MusicFadeOut.length - 1].time < num)
			{
				UnityEngine.Object.Destroy(audioSource_0);
				audioSource_0 = null;
			}
		}
		if (audioSource_1 != null)
		{
			audioSource_1.volume = MusicFadeIn.Evaluate(num);
			if (MusicFadeIn[MusicFadeIn.length - 1].time < num)
			{
				audioSource_1 = null;
			}
		}
		if (particleSystem_0 != null)
		{
			ParticleSystem.MainModule main = particleSystem_0.main;
			main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 1f, BreathCurve.Evaluate(num)));
			if (Camera.main.transform.parent != transform_1)
			{
				transform_1 = Camera.main.transform.parent;
				if (transform_1.name == "CameraShaker")
				{
					transform_0.parent = Camera.main.transform;
					transform_0.localPosition = new Vector3(0f, -0.015f, 0.0301f);
					transform_0.localRotation = Quaternion.identity;
					particleSystem_0.gameObject.SetActive(value: true);
					ParticleSystem.EmissionModule emission = particleSystem_0.emission;
					emission.enabled = true;
					particleSystem_0.Play(withChildren: false);
				}
				else
				{
					particleSystem_0.gameObject.SetActive(value: false);
					ParticleSystem.EmissionModule emission2 = particleSystem_0.emission;
					emission2.enabled = false;
					particleSystem_0.Pause(withChildren: false);
				}
			}
		}
		if (gclass994_0 != null)
		{
			gclass994_0.Update(TerrainDetailCurve.Evaluate(num));
		}
	}

	public void method_0()
	{
		light_0 = GameObject.Find("sun").GetComponent<Light>();
		color_0 = light_0.color;
		float_0 = light_0.intensity;
		float_1 = light_0.shadowStrength;
		LevelSettings levelSettings = (LevelSettings)GClass870.FindUnityObjectOfType(typeof(LevelSettings));
		_ = levelSettings != null;
		color_1 = smethod_0(color_0);
		method_2();
		AudioSource[] array = new AudioSource[0];
		if (levelSettings != null)
		{
			array = levelSettings.gameObject.GetComponentsInChildren<AudioSource>();
		}
		AudioSource[] array2 = array;
		foreach (AudioSource audioSource in array2)
		{
			if (audioSource.playOnAwake && audioSource.loop)
			{
				audioSource_0 = audioSource;
				audioSource_1 = audioSource.gameObject.AddComponent<AudioSource>();
				audioSource_1.clip = SnowyWind;
				audioSource_1.loop = true;
				audioSource_1.Play();
				break;
			}
		}
		transform_0 = UnityEngine.Object.Instantiate(BreathSystem);
		transform_0.gameObject.name = "Breath";
		particleSystem_0 = transform_0.GetComponent<ParticleSystem>();
		method_1();
	}

	public void method_1()
	{
		TerrainMaterial.SetColor(int_0, Color.black);
	}

	public void method_2()
	{
	}

	public static Color smethod_0(Color color)
	{
		float num = (color.r + color.g + color.b) / 3f;
		return new Color(num, num, num, color.a);
	}

	public void method_3()
	{
		Vector3 position = GClass870.FindUnityObjectOfType<UpperLeftAnchor>().transform.position;
		Vector3 position2 = GClass870.FindUnityObjectOfType<LowerRightAnchor>().transform.position;
		Vector2 vector = new Vector2(Mathf.Max(position.x, position2.x), Mathf.Max(position.z, position2.z));
		Vector2 vector2 = new Vector2(Mathf.Min(position.x, position2.x), Mathf.Min(position.z, position2.z));
		Vector2 vector3 = (vector + vector2) * 0.5f;
		Vector2 vector4 = vector - vector2;
		Vector2 vector5 = new Vector2(Mathf.Min(position.y, position2.y), Mathf.Max(position.y, position2.y));
		float orthographicSize = Mathf.Max(vector4.x, vector4.y);
		float farClipPlane = vector5.y - vector5.x;
		GameObject gameObject = new GameObject("SnowCam", typeof(Camera));
		gameObject.transform.position = new Vector3(vector3.x, vector5.y, vector3.y);
		gameObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
		Camera component = gameObject.GetComponent<Camera>();
		component.orthographic = true;
		component.orthographicSize = orthographicSize;
		component.aspect = 1f;
		component.farClipPlane = farClipPlane;
		component.depthTextureMode = DepthTextureMode.Depth;
		LinkedList<Renderer> linkedList = smethod_1();
		foreach (Renderer item in linkedList)
		{
			item.enabled = false;
		}
		RenderTexture dest = new RenderTexture(2048, 2048, 1, RenderTextureFormat.ARGB32)
		{
			name = "WinterScript RT"
		};
		component.cullingMask = DepthRendererMask.value;
		component.targetTexture = RenderTexture.GetTemporary(2048, 2048, 1, RenderTextureFormat.ARGB32);
		component.Render();
		Graphics.Blit(component.targetTexture, dest, DepthMaterial);
		RenderTexture.ReleaseTemporary(component.targetTexture);
		foreach (Renderer item2 in linkedList)
		{
			item2.enabled = true;
		}
		UnityEngine.Object.Destroy(gameObject);
	}

	public static LinkedList<Renderer> smethod_1()
	{
		LevelSettings levelSettings = (LevelSettings)GClass870.FindUnityObjectOfType(typeof(LevelSettings));
		if (levelSettings == null)
		{
			return new LinkedList<Renderer>();
		}
		GameObject gameObject = levelSettings.gameObject;
		HashSet<string> hashSet = new HashSet<string>(new string[7] { "Hidden/Nature/Tree Creator Bark Optimized Snow", "Hidden/Nature/Tree Creator Bark Optimized", "Hidden/Nature/Tree Creator Leaves Optimized Snow 2", "Hidden/Nature/Tree Creator Leaves Optimized", "Hidden/TerrainEngine/Details/WavingDoublePass", "Hidden/TerrainEngine/Details/BillboardWavingDoublePass", "Transparent/Cutout/Diffuse TreesSnow 1" });
		LinkedList<Renderer> linkedList = new LinkedList<Renderer>();
		Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			Material[] materials = renderer.materials;
			foreach (Material material in materials)
			{
				if (hashSet.Contains(material.shader.name))
				{
					linkedList.AddLast(renderer);
					break;
				}
			}
		}
		return linkedList;
	}

	public static Vector2 smethod_2()
	{
		return new Vector2(-50f, 60f);
	}
}
