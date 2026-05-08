using System;
using EFT.BlitDebug;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/SunOnGlass")]
public class SunOnGlass : MonoBehaviour
{
	[Serializable]
	public class SunBack
	{
		public Transform Light;

		public Texture SunTexture;

		public Color SunColor;

		public float size = 100f;

		public Transform _trans;

		[NonSerialized]
		public Vector3 Position;

		public void Initialize(bool formEditor = false)
		{
			if (Application.isPlaying || formEditor)
			{
				_trans = CreatePolygon();
				_trans.gameObject.hideFlags = HideFlags.DontSave;
				_trans.name = "SunSprite";
				_trans.rotation = Light.rotation;
				Position = Light.rotation * Vector3.back * 800f;
				_trans.position = Position;
				_trans.localScale = new Vector3(size, size, 0.1f);
				Debug.LogError("FIX IT");
				Material material = new Material(GClass872.Find("Particles/Additive"));
				material.hideFlags = HideFlags.HideAndDontSave;
				_trans.GetComponent<Renderer>().material = material;
				material.mainTexture = SunTexture;
			}
		}

		public void UpdatePos(Vector3 camPos)
		{
			if (!(_trans == null))
			{
				Position = Light.rotation * Vector3.back * 800f;
				_trans.position = Position + camPos;
			}
		}

		public Transform GetSun()
		{
			return _trans;
		}

		public Transform CreatePolygon()
		{
			GameObject gameObject = new GameObject("SunBack", typeof(MeshFilter), typeof(MeshRenderer));
			gameObject.GetComponent<MeshFilter>().mesh = new Mesh
			{
				vertices = new Vector3[4]
				{
					new Vector3(-0.5f, -0.5f),
					new Vector3(-0.5f, 0.5f),
					new Vector3(0.5f, -0.5f),
					new Vector3(0.5f, 0.5f)
				},
				uv = new Vector2[4]
				{
					new Vector2(0f, 0f),
					new Vector2(0f, 1f),
					new Vector2(1f, 0f),
					new Vector2(1f, 1f)
				},
				triangles = new int[6] { 0, 2, 3, 3, 1, 0 },
				name = "SunOnGlass CreatePolygon"
			};
			return gameObject.transform;
		}
	}

	public float Size = 1f;

	public Transform Sun;

	public Texture ScreenTexture;

	public Shader ScreenShader;

	public Shader VisibilityCheckerShader;

	public CustomLensFlare LensFlares;

	public SunBack SunBackGround;

	public Color SunColor;

	public AnimationCurve SunCurve;

	private bool bool_0 = true;

	private bool bool_1 = true;

	private bool bool_2 = true;

	private Texture texture_0;

	private Material material_0;

	private Material material_1;

	private Camera camera_0;

	private Transform transform_0;

	private Texture2D texture2D_0;

	private RenderTexture renderTexture_0;

	private RenderTexture renderTexture_1;

	private Rect rect_0;

	private static readonly int int_0 = Shader.PropertyToID("_ScreenTex");

	private static readonly int int_1 = Shader.PropertyToID("_FallofTex");

	private static readonly int int_2 = Shader.PropertyToID("_SunColor");

	private static readonly int int_3 = Shader.PropertyToID("_SunPos");

	private static readonly int int_4 = Shader.PropertyToID("_SunVisible");

	public int Settings
	{
		set
		{
			switch (value)
			{
			default:
				bool_0 = true;
				bool_1 = true;
				bool_2 = true;
				break;
			case 0:
				bool_0 = false;
				bool_1 = false;
				bool_2 = true;
				break;
			case 1:
				bool_0 = false;
				bool_1 = true;
				bool_2 = true;
				break;
			case 2:
				bool_0 = true;
				bool_1 = true;
				bool_2 = true;
				break;
			}
		}
	}

	public void Start()
	{
		if (SystemInfo.supportsImageEffects && SystemInfo.graphicsShaderLevel >= 30)
		{
			if (ScreenShader == null)
			{
				Debug.Log("Shader are not set up! Disabling effect.");
				base.enabled = false;
			}
			camera_0 = GetComponent<Camera>();
			transform_0 = camera_0.transform;
			camera_0.depthTextureMode |= DepthTextureMode.Depth;
			SunBackGround.Initialize();
			LensFlares.Initialize();
			texture_0 = GetTexFromCurve(SunCurve);
		}
		else
		{
			base.enabled = false;
		}
	}

	public void Update()
	{
		if (bool_2)
		{
			SunBackGround.UpdatePos(base.transform.position);
		}
	}

	public Material method_0()
	{
		if (material_1 == null)
		{
			material_1 = new Material(VisibilityCheckerShader);
			material_1.hideFlags = HideFlags.HideAndDontSave;
		}
		return material_1;
	}

	public Material method_1()
	{
		if (material_0 == null)
		{
			material_0 = new Material(ScreenShader);
			material_0.hideFlags = HideFlags.HideAndDontSave;
			material_0.SetTexture(int_0, ScreenTexture);
			material_0.SetTexture(int_1, texture_0);
		}
		return material_0;
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if ((!bool_0 && !bool_1) || !method_3(-Sun.forward, out var onScreen))
		{
			GClass860.BlitOrCopy(source, destination);
			return;
		}
		Vector4 vector = new Vector4(onScreen.x, onScreen.y, (float)Screen.width / (float)Screen.height, Size);
		method_2(source, destination, vector);
		if (bool_0)
		{
			Material material = method_1();
			Shader.SetGlobalColor(int_2, smethod_0(SunColor));
			material.SetVector(int_3, vector);
			if (Application.isEditor)
			{
				material_0.SetTexture(int_0, ScreenTexture);
				material_0.SetTexture(int_1, texture_0);
			}
			Graphics.Blit(source, destination, material);
		}
		else
		{
			Graphics.CopyTexture(source, destination);
		}
		if (bool_1)
		{
			LensFlares.Draw(onScreen);
		}
	}

	public void OnDestroy()
	{
		if (SunBackGround != null && (bool)SunBackGround.GetSun())
		{
			UnityEngine.Object.DestroyImmediate(SunBackGround.GetSun().gameObject);
		}
	}

	public void OnApplicationQuit()
	{
		if (SunBackGround.GetSun() != null)
		{
			UnityEngine.Object.DestroyImmediate(SunBackGround.GetSun().gameObject);
		}
	}

	public void method_2(RenderTexture source, RenderTexture destination, Vector4 sunPos)
	{
		Material material = method_0();
		material.SetVector(int_3, sunPos);
		if (renderTexture_0 == null)
		{
			renderTexture_0 = new RenderTexture(4, 4, 0)
			{
				name = "SunOnGlass _vc1"
			};
		}
		if (renderTexture_1 == null)
		{
			renderTexture_1 = new RenderTexture(1, 1, 0)
			{
				name = "SunOnGlass _vc2"
			};
			Shader.SetGlobalTexture(int_4, renderTexture_1);
		}
		DebugGraphics.Blit(source, destination, material, 2);
		DebugGraphics.Blit(destination, renderTexture_0, material, 0);
		DebugGraphics.Blit(renderTexture_0, renderTexture_1, material, 1);
	}

	public static Color smethod_0(Color color)
	{
		float num = 1f / ((color.r + color.g + color.b) / 3f);
		color.a = num - 1f;
		return color;
	}

	public bool method_3(Vector3 lightDirection, out Vector2 onScreen)
	{
		Vector3 position = transform_0.position;
		Vector3 vector = camera_0.WorldToViewportPoint(position + lightDirection);
		onScreen = vector;
		if (vector.z < 0f)
		{
			return false;
		}
		if (!method_4(vector))
		{
			return false;
		}
		return true;
	}

	public bool method_4(Vector2 lightPos)
	{
		if (lightPos.x < -0.2f)
		{
			return false;
		}
		if (lightPos.x > 1.2f)
		{
			return false;
		}
		if (lightPos.y < -0.2f)
		{
			return false;
		}
		if (lightPos.y > 1.2f)
		{
			return false;
		}
		return true;
	}

	public static Texture2D GetTexFromCurve(AnimationCurve curve, int width = 255)
	{
		Texture2D texture2D = new Texture2D(width, 1, TextureFormat.Alpha8, mipChain: false);
		texture2D.name = "Sunonglass";
		texture2D.wrapMode = TextureWrapMode.Clamp;
		float num = 1f / (float)width;
		float num2 = 0f;
		for (int i = 0; i < width; i++)
		{
			float num3 = Mathf.Clamp(curve.Evaluate(num2), 0f, 1f);
			texture2D.SetPixel(i, 0, new Color(num3, num3, num3, num3));
			num2 += num;
		}
		texture2D.Apply();
		return texture2D;
	}
}
