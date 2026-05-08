using UnityEngine;

public class CirclePacker : MonoBehaviour
{
	public class GClass959
	{
		public Vector2 Pos;

		public float Radius;

		public float Alpha;
	}

	public bool Generate;

	public int Seed;

	public int Count = 16;

	[GAttribute6(0f, 1f, -1f)]
	public Vector2 Radius;

	[Range(0.8f, 1.2f)]
	public float TuneSize = 1f;

	[Range(1f, 255f)]
	public int Iterations;

	[Range(0f, 1f)]
	public float AlphaRandomness = 1f;

	public int TextureSize = 128;

	public bool FadeMips;

	[GAttribute6(0f, 1f, -1f)]
	public Vector2 FadeMinMax;

	public Material Material;

	public Material ViewMaterial;

	public bool ExportTexture;

	private RenderTexture renderTexture_0;

	private GClass959[] gclass959_0;

	private static readonly int int_0 = Shader.PropertyToID("_Size");

	public void OnValidate()
	{
		if (Generate)
		{
			Random.InitState(Seed);
			Vector2 vector = Radius / Mathf.Sqrt(Count);
			gclass959_0 = new GClass959[Count];
			for (int i = 0; i < gclass959_0.Length; i++)
			{
				gclass959_0[i] = new GClass959();
				gclass959_0[i].Pos = new Vector2(Random.value, Random.value);
				gclass959_0[i].Radius = Random.Range(vector.x, vector.y);
			}
			smethod_0(gclass959_0, Iterations);
			smethod_3(gclass959_0, 1f);
			if (renderTexture_0 == null || TextureSize != renderTexture_0.width)
			{
				renderTexture_0 = new RenderTexture(TextureSize, TextureSize, 0, RenderTextureFormat.ARGB32)
				{
					name = "CirclePacker _outputTexture",
					wrapMode = TextureWrapMode.Repeat
				};
			}
		}
		if (gclass959_0 != null)
		{
			Graphics.SetRenderTarget(renderTexture_0);
			GL.Clear(clearDepth: true, clearColor: true, new Color(0f, 0f, 0f, 0f));
			smethod_4(gclass959_0, Material, 0, TuneSize);
			ViewMaterial.mainTexture = renderTexture_0;
		}
		if (ExportTexture)
		{
			Texture2D texture = smethod_6(renderTexture_0, FadeMips);
			if (FadeMips)
			{
				smethod_7(texture, FadeMinMax.x, FadeMinMax.y);
			}
			smethod_8(texture, FadeMips);
			ExportTexture = false;
		}
	}

	public static void smethod_0(GClass959[] circles, int iterationCount)
	{
		for (int i = 0; i < iterationCount; i++)
		{
			foreach (GClass959 gClass in circles)
			{
				foreach (GClass959 gClass2 in circles)
				{
					if (gClass != gClass2)
					{
						Vector2 v = gClass2.Pos - gClass.Pos;
						v = smethod_2(v);
						float num = gClass.Radius + gClass2.Radius;
						float num2 = v.x * v.x + v.y * v.y;
						if (num2 < num * num * 1.02f)
						{
							Vector2 normalized = v.normalized;
							normalized *= (num - Mathf.Sqrt(num2)) * 0.5f;
							gClass2.Pos += normalized;
							gClass.Pos -= normalized;
							gClass2.Pos = smethod_1(gClass2.Pos);
							gClass.Pos = smethod_1(gClass.Pos);
						}
					}
				}
			}
		}
	}

	public static Vector2 smethod_1(Vector2 v)
	{
		if (v.x < 0f)
		{
			v.x += 1f;
		}
		if (v.y < 0f)
		{
			v.y += 1f;
		}
		if (v.x > 1f)
		{
			v.x -= 1f;
		}
		if (v.y > 1f)
		{
			v.y -= 1f;
		}
		return v;
	}

	public static Vector2 smethod_2(Vector2 v)
	{
		if (v.x < -0.5f)
		{
			v.x += 1f;
		}
		if (v.y < -0.5f)
		{
			v.y += 1f;
		}
		if (v.x > 0.5f)
		{
			v.x -= 1f;
		}
		if (v.y > 0.5f)
		{
			v.y -= 1f;
		}
		return v;
	}

	public static void smethod_3(GClass959[] circles, float randomness)
	{
		float[] array = new float[circles.Length];
		float num = 1f / (float)array.Length;
		float num2 = num * 0.5f;
		float num3 = num2 * randomness;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = num2 + (float)i * num + Random.Range(0f - num3, num3);
		}
		for (int j = 0; j < array.Length; j++)
		{
			int num4 = Random.Range(0, array.Length - 1);
			if (j != num4)
			{
				float num5 = array[j];
				array[j] = array[num4];
				array[num4] = num5;
			}
		}
		for (int k = 0; k < circles.Length; k++)
		{
			circles[k].Alpha = array[k];
		}
	}

	public static void smethod_4(GClass959[] circles, Material material, int pass, float tuneSize)
	{
		material.SetPass(pass);
		material.SetFloat(int_0, tuneSize);
		GL.Begin(7);
		Vector2 vector = default(Vector2);
		foreach (GClass959 gClass in circles)
		{
			Vector2 pos = gClass.Pos;
			vector.x = ((pos.x < 0.5f) ? 1f : (-1f));
			vector.y = ((pos.y < 0.5f) ? 1f : (-1f));
			GL.Color(new Color(0f, 0f, 0f, gClass.Alpha));
			smethod_5(pos, gClass.Radius);
			smethod_5(pos + new Vector2(0f, vector.y), gClass.Radius);
			smethod_5(pos + new Vector2(vector.x, 0f), gClass.Radius);
			smethod_5(pos + new Vector2(vector.x, vector.y), gClass.Radius);
		}
		GL.End();
	}

	public static void smethod_5(Vector2 center, float size)
	{
		center = center * 2f - Vector2.one;
		size *= 2f;
		Vector2 vector = new Vector2(center.x - size, center.y - size);
		Vector2 vector2 = new Vector2(center.x + size, center.y + size);
		GL.TexCoord2(0f, 0f);
		GL.Vertex3(vector.x, vector.y, 0f);
		GL.TexCoord2(0f, 1f);
		GL.Vertex3(vector.x, vector2.y, 0f);
		GL.TexCoord2(1f, 1f);
		GL.Vertex3(vector2.x, vector2.y, 0f);
		GL.TexCoord2(1f, 0f);
		GL.Vertex3(vector2.x, vector.y, 0f);
	}

	public static Texture2D smethod_6(RenderTexture renderTexture, bool mipMaps)
	{
		RenderTexture.active = renderTexture;
		Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, mipMaps);
		texture2D.name = "CirclePacker";
		texture2D.ReadPixels(new Rect(0f, 0f, renderTexture.width, renderTexture.height), 0, 0, mipMaps);
		texture2D.Apply(mipMaps, makeNoLongerReadable: false);
		return texture2D;
	}

	public static void smethod_7(Texture2D texture, float a, float b)
	{
		int mipmapCount = texture.mipmapCount;
		for (int i = 0; i < mipmapCount; i++)
		{
			float value = (float)i / (float)(mipmapCount - 1);
			Color32[] pixels = texture.GetPixels32(i);
			float num = Mathf.InverseLerp(b, a, value);
			for (int j = 0; j < pixels.Length; j++)
			{
				pixels[j].r = (byte)((float)(int)pixels[j].r * num);
			}
			texture.SetPixels32(pixels, i);
		}
		texture.Apply(updateMipmaps: false);
	}

	public static void smethod_8(Texture2D texture, bool asAsset)
	{
	}
}
