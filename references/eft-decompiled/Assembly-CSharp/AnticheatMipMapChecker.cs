using System;
using UnityEngine;

public class AnticheatMipMapChecker : MonoBehaviour
{
	[Serializable]
	public class TextureChecker
	{
		[SerializeField]
		public Texture2D CheckTexture;

		[SerializeField]
		public float CheckValue;
	}

	public static AnticheatMipMapChecker Instance;

	[HideInInspector]
	public bool IsMipMapCorrect = true;

	[SerializeField]
	private Shader _mipMapChecker;

	[SerializeField]
	private TextureChecker textureChecker;

	private Material material_0;

	private RenderTexture renderTexture_0;

	private Texture2D texture2D_0;

	private Material material_1;

	private static readonly int int_0 = Shader.PropertyToID("_MainTex");

	private static readonly int int_1 = Shader.PropertyToID("_MipMapChecker");

	public void Awake()
	{
		Instance = this;
	}

	public void Start()
	{
		method_0();
		IsMipMapCorrect = method_1();
		method_2(IsMipMapCorrect);
		method_4();
	}

	public void method_0()
	{
		renderTexture_0 = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default)
		{
			name = "CheckerRT",
			autoGenerateMips = true,
			useMipMap = true
		};
		RenderTexture active = RenderTexture.active;
		Graphics.SetRenderTarget(renderTexture_0);
		GL.Clear(clearDepth: true, clearColor: true, Color.clear);
		Material material = method_3();
		material.SetTexture(int_0, textureChecker.CheckTexture);
		material.SetPass(0);
		float x = -1f;
		float x2 = 1f;
		float y = -1f;
		float y2 = 1f;
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
		texture2D_0 = new Texture2D(512, 512, TextureFormat.RGBAFloat, mipChain: true);
		texture2D_0.name = "CheckerTexture";
		texture2D_0.ReadPixels(new Rect(0f, 0f, 512f, 512f), 0, 0);
		texture2D_0.Apply();
		RenderTexture.active = active;
	}

	public bool method_1()
	{
		Color[] pixels = texture2D_0.GetPixels();
		float num = 0f;
		for (int i = 0; i < pixels.Length; i++)
		{
			num += pixels[i].a;
		}
		return Math.Abs(textureChecker.CheckValue - num) < 20f;
	}

	public void method_2(bool isMipMapCorrect)
	{
		Shader.SetGlobalFloat(int_1, (!isMipMapCorrect) ? 1 : 0);
	}

	public Material method_3()
	{
		if (material_0 != null)
		{
			return material_0;
		}
		return material_0 = new Material(_mipMapChecker);
	}

	public void method_4()
	{
		UnityEngine.Object.DestroyImmediate(renderTexture_0);
		renderTexture_0 = null;
		UnityEngine.Object.DestroyImmediate(texture2D_0);
		texture2D_0 = null;
		UnityEngine.Object.DestroyImmediate(material_0);
	}
}
